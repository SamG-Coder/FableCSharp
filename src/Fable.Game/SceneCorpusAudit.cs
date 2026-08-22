using System.Diagnostics;
using System.Numerics;
using System.Text;
using Fable.Core;
using Fable.Formats.Scene;
using Fable.Formats.Wld;
using Fable.Render;

namespace Fable.Game;

/// <summary>
/// Builds every retail WLD map through the same parsers and mesh batching used
/// by the live renderer, then emits a compact grep-first parity inventory.
/// Pixel payloads are represented by stable texture-id sentinels so the audit
/// remains bounded instead of retaining the full texture bank in RAM.
/// </summary>
public static class SceneCorpusAudit
{
    public const int FormatVersion = 1;

    public static SceneCorpusSummary Run(GameInstall install, string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        using var levels = new LevelLibrary(install);
        using var meshes = new MeshBank();
        using var textures = new TextureLibrary(install);
        meshes.Open(install);

        var rows = new List<SceneCorpusRow>(levels.World.Maps.Count);
        using var writer = new StreamWriter(outputPath, false, new UTF8Encoding(false));
        writer.WriteLine($"FABLE_SCENE_CORPUS_GREP_V{FormatVersion}");
        writer.WriteLine($"SOURCE\twld={Atom(Path.GetFileName(install.WorldPath))}\tmaps={levels.World.Maps.Count}\tregions={levels.World.Regions.Count}\tmesh_entries={meshes.EntryCount}\ttexture_entries={textures.EntryCount}");
        writer.WriteLine("SCOPE\tgeometry=full_primary_map\tobjects=full_c3d\ttextures=bank_directory_only\tcamera=identity_audit\twindow=none");
        writer.WriteLine("GAP\tname=dynamic_environment_constants\tdx9=per_frame_constant_table\tvulkan=first_seen_literals\tstatus=not_implemented");
        writer.WriteLine("GAP\tname=native_shader_identity\tdx9=shader_bank_program_per_draw\tvulkan=shader_mode_branch\tstatus=partially_classified");

        foreach (var map in levels.World.Maps.OrderBy(m => m.Index))
        {
            var row = AuditMap(levels, meshes, textures, map);
            rows.Add(row);
            WriteRow(writer, row);
            levels.UnloadMap(map.FileStem);
            levels.UnloadThings(map.FileStem);
            meshes.ReleaseParsed();
        }

        var summary = new SceneCorpusSummary(
            rows.Count,
            rows.Count(r => r.Issues.Count == 0),
            rows.Count(r => r.Issues.Count > 0),
            rows.Sum(r => r.Issues.Count),
            rows.Sum(r => r.LandscapeVertices),
            rows.Sum(r => r.ObjectVertices));
        writer.WriteLine($"SUMMARY\tscenes={summary.Scenes}\tclean={summary.Clean}\tflagged={summary.Flagged}\tissues={summary.Issues}\tlandscape_vertices={summary.LandscapeVertices}\tobject_vertices={summary.ObjectVertices}");
        return summary;
    }

    private static SceneCorpusRow AuditMap(
        LevelLibrary levels, MeshBank meshes, TextureLibrary textures, WorldMap map)
    {
        var timer = Stopwatch.StartNew();
        var issues = new List<string>();
        var tng = levels.TryLoadThings(map);
        var things = tng?.Things.ToList() ?? [];
        var lev = levels.LoadCompiledLev(map);
        var height = levels.LoadHeightField(map);
        var cells = levels.LoadCells(map);

        if (!map.IsSea && lev is null)
            issues.Add("MISSING_LEV");
        if (!map.IsSea && height is null)
            issues.Add("MISSING_STB_HEIGHT");
        var world = WorldGeometry.Build(
            levels.Install,
            map.FileStem,
            things,
            adjacentStaticMaps: false,
            levels: levels,
            onlyMaps: [map.FileStem],
            meshes: meshes,
            expandGeometry: false);

        if (world.MissingMeshes > 0)
        {
            var details = things.Select(thing => WorldGeometry.ResolveSubmit(
                    levels.Defs, levels.MeshEnums, thing))
                .Where(submit => submit.AsC3d && submit.MeshIds.Count == 0)
                .Select(submit => submit.Definition + "@" + (submit.TypeName ?? "NULL"))
                .Distinct(StringComparer.OrdinalIgnoreCase).Order();
            issues.Add("UNRESOLVED_MESH_DEFINITION:" + world.MissingMeshes + ":" +
                       string.Join(',', details));
        }

        var props = new List<(Fable.Formats.Meshes.MeshFile Mesh, Matrix4x4 Transform)>();
        var missingMeshBank = 0;
        foreach (var instance in world.Instances)
        {
            var mesh = meshes.Get(instance.MeshId);
            if (mesh is null)
            {
                missingMeshBank++;
                continue;
            }
            props.Add((mesh, instance.Transform));
        }
        if (missingMeshBank > 0)
            issues.Add("MISSING_MESH_BANK_ENTRY:" + missingMeshBank);

        var landscape = MeshBatches.BuildCells(cells);
        var objects = MeshBatches.BuildMeshes(props);
        if (landscape.Vertices.Length == 0 && objects.Vertices.Length == 0 && things.Count > 0)
            issues.Add("EMPTY_RENDER_SUBMISSION_WITH_THINGS:" + things.Count);
        ValidateStream("LAND", landscape, issues);
        ValidateStream("OBJECT", objects, issues);

        var draws = landscape.Draws.Concat(objects.Draws).ToArray();
        var textureIds = draws.SelectMany(d => new[] { d.TextureId, d.TextureId1 })
            .Where(id => id > 0).Distinct().Order().ToArray();
        var missingTextures = textureIds.Where(id => !textures.Contains(id)).ToArray();
        if (missingTextures.Length > 0)
            issues.Add("MISSING_TEXTURE_BANK_ENTRY:" + missingTextures.Length);

        var unknownPasses = draws.Select(d => d.PassBit).Distinct()
            .Where(bit => ScenePasses.Rank(bit) == int.MaxValue).ToArray();
        if (unknownPasses.Length > 0)
            issues.Add("UNKNOWN_PASS:" + string.Join(',', unknownPasses.Select(v => $"0x{v:X}")));
        var unsupportedPasses = draws.Select(d => d.PassBit).Distinct()
            .Where(bit => ScenePasses.Registration.FirstOrDefault(p => p.Bit == bit) is var pass &&
                          pass.Bit != 0 && !ScenePasses.Draws(pass.Submit)).ToArray();
        if (unsupportedPasses.Length > 0)
            issues.Add("UNIMPLEMENTED_DRAW_PASS:" + string.Join(',', unsupportedPasses.Select(v => $"0x{v:X}")));
        if (draws.Any(d => d.ShaderMode is < 0f or > 3f || !float.IsFinite(d.ShaderMode)))
            issues.Add("INVALID_SHADER_MODE");

        var sentinels = textureIds.Select(id => new GpuTexture(id, 1, 1,
            [(byte)id, (byte)(id >> 8), (byte)(id >> 16), 255])).ToArray();
        var packet = new SceneRenderPacket
        {
            SceneName = map.FileStem,
            LandscapeVertices = landscape.Vertices,
            LandscapeDraws = landscape.Draws,
            LandscapeIndices = landscape.Indices,
            ObjectVertices = objects.Vertices,
            ObjectDraws = objects.Draws,
            Textures = sentinels,
            ViewProjection = Matrix4x4.Identity,
            LandscapeViewProjection = Matrix4x4.Identity,
            SkyViewProjection = Matrix4x4.Identity,
            CameraPosition = Vector3.Zero,
            FogPlane = Vector4.Zero,
        };

        var owners = levels.World.Regions
            .Where(region => region.ContainsMaps.Any(name => MapEquals(name, map)))
            .Select(region => region.RegionName).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var viewers = levels.World.Regions
            .Where(region => region.SeesMaps.Any(name => MapEquals(name, map)))
            .Select(region => region.RegionName).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        return new SceneCorpusRow(
            map.Index, map.FileStem, map.ScriptName, map.IsSea,
            owners, viewers, tng?.Sections.Count ?? 0, things.Count,
            cells.Count, landscape.Vertices.Length, landscape.Indices.Length,
            landscape.Draws.Length, world.Instances.Count, objects.Vertices.Length,
            objects.Draws.Length, textureIds.Length, packet.ContentHash(),
            timer.Elapsed.TotalMilliseconds, issues);
    }

    private static void ValidateStream(string name, TexturedMesh mesh, List<string> issues)
    {
        if (mesh.Vertices.Any(v =>
                !Finite(v.Position) || !Finite(v.Normal) || !Finite(v.Uv) ||
                !Finite(v.Color) || !Finite(v.Extra)))
            issues.Add(name + "_NONFINITE_VERTEX");

        foreach (var draw in mesh.Draws)
        {
            if ((ulong)draw.FirstVertex + draw.VertexCount > (ulong)mesh.Vertices.Length)
            {
                issues.Add(name + "_DRAW_VERTEX_RANGE");
                break;
            }
            if (!draw.Indexed)
                continue;
            if ((ulong)draw.FirstIndex + draw.IndexCount > (ulong)mesh.Indices.Length)
            {
                issues.Add(name + "_DRAW_INDEX_RANGE");
                break;
            }
            var end = (int)(draw.FirstIndex + draw.IndexCount);
            for (var i = (int)draw.FirstIndex; i < end; i++)
            {
                if (mesh.Indices[i] < draw.VertexCount)
                    continue;
                issues.Add(name + "_INDEX_VALUE_RANGE");
                return;
            }
        }
    }

    private static bool Finite(Vector2 v) => float.IsFinite(v.X) && float.IsFinite(v.Y);
    private static bool Finite(Vector3 v) => float.IsFinite(v.X) && float.IsFinite(v.Y) && float.IsFinite(v.Z);
    private static bool Finite(Vector4 v) => float.IsFinite(v.X) && float.IsFinite(v.Y) && float.IsFinite(v.Z) && float.IsFinite(v.W);
    private static bool MapEquals(string value, WorldMap map) =>
        Path.GetFileNameWithoutExtension(value).Equals(map.FileStem, StringComparison.OrdinalIgnoreCase) ||
        value.Equals(map.ScriptName, StringComparison.OrdinalIgnoreCase);

    private static void WriteRow(TextWriter writer, SceneCorpusRow row)
    {
        writer.WriteLine($"SCENE\tindex={row.Index}\tfile={Atom(row.FileStem)}\tscript={Atom(row.ScriptName)}\tsea={row.IsSea}\towners={Csv(row.Owners)}\tviewers={Csv(row.Viewers)}\ttng_sections={row.TngSections}\tthings={row.Things}\tcells={row.Cells}\tland_vertices={row.LandscapeVertices}\tland_indices={row.LandscapeIndices}\tland_draws={row.LandscapeDraws}\tinstances={row.Instances}\tobject_vertices={row.ObjectVertices}\tobject_draws={row.ObjectDraws}\ttextures={row.Textures}\thash={row.SubmissionHash}\tms={row.ElapsedMs:0.0}\tstatus={(row.Issues.Count == 0 ? "clean" : "flagged")}");
        foreach (var issue in row.Issues)
            writer.WriteLine($"ISSUE\tscene={Atom(row.FileStem)}\tcode={Atom(issue)}");
    }

    private static string Csv(IEnumerable<string> values) => string.Join(',', values.Select(Atom));
    private static string Atom(string value) => value.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
}

public sealed record SceneCorpusRow(
    int Index, string FileStem, string ScriptName, bool IsSea,
    IReadOnlyList<string> Owners, IReadOnlyList<string> Viewers,
    int TngSections, int Things, int Cells,
    int LandscapeVertices, int LandscapeIndices, int LandscapeDraws,
    int Instances, int ObjectVertices, int ObjectDraws, int Textures,
    string SubmissionHash, double ElapsedMs, IReadOnlyList<string> Issues);

public readonly record struct SceneCorpusSummary(
    int Scenes, int Clean, int Flagged, int Issues,
    long LandscapeVertices, long ObjectVertices);
