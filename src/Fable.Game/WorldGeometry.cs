using System.Numerics;
using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Defs;
using Fable.Formats.Levels;
using Fable.Formats.Meshes;
using Fable.Formats.Tng;

namespace Fable.Game;

public sealed class WorldGeometry
{
    public const float MeshToWorld = 0.01f;

    public required string Region { get; init; }
    public required IReadOnlyList<MeshTriangle> Triangles { get; init; }
    public required int MeshInstances { get; init; }
    public required int MissingMeshes { get; init; }

    public static WorldGeometry Build(GameInstall install, string region, IEnumerable<ThingInstance> things)
    {
        var headerPath = Path.Combine(install.DataRoot, "Defs", "RetailHeaders", "meshdata.h");
        var graphicsPath = Path.Combine(install.DataRoot, "graphics", "graphics.big");
        var enums = File.Exists(headerPath) ? HeaderEnums.Load(headerPath) : null;
        GameBin? defs = null;
        var namesPath = install.FindCompiledDef("names.bin");
        var binPath = install.FindCompiledDef("game.bin");
        if (namesPath is not null && binPath is not null)
            defs = GameBin.Load(binPath, NamesBin.Load(namesPath));
        using var big = BigArchive.Open(graphicsPath);
        var bank = big.SubBanks.First(item => item.Name.Contains("MESH", StringComparison.OrdinalIgnoreCase));
        var entries = big.ReadEntries(bank);
        var byId = entries
            .Where(entry => entry.Type is not 3)
            .GroupBy(entry => entry.Id)
            .ToDictionary(group => group.Key, group => group.First());

        var cache = new Dictionary<uint, MeshFile?>();
        var triangles = new List<MeshTriangle>(80_000);

        using (var levels = new LevelLibrary(install))
        {
            var height = levels.LoadHeightField(region);
            var compiled = levels.LoadCompiledLev(region);
            var cells = compiled is null ? null : LevCellGrid.TryParse(compiled);
            var textureHeader = Path.Combine(install.DataRoot, "Defs", "RetailHeaders", "pc", "textures.h");
            var landscapeEnums = File.Exists(textureHeader) ? HeaderEnums.Load(textureHeader) : null;
            if (height is not null && cells is not null && compiled is not null)
            {
                triangles.AddRange(height.ToFineTriangles(cells, compiled.Materials, landscapeEnums));
            }
            else if (height is not null)
            {
                foreach (var tri in height.ToLocalTriangles())
                    triangles.Add(tri with { TextureId = TextureLibrary.LandscapeGrassPlainId });
            }
        }
        var instances = 0;
        var missing = 0;

        foreach (var thing in things)
        {
            if (thing.PositionX is null || thing.DefinitionType is null)
                continue;

            var meshId = defs?.FindMeshId(thing.DefinitionType) ?? enums?.FindMeshId(thing.DefinitionType);
            if (meshId is null || !byId.TryGetValue((uint)meshId.Value, out var entry))
            {
                missing++;
                continue;
            }

            if (!cache.TryGetValue(entry.Id, out var mesh))
            {
                mesh = MeshFile.TryParse(big.Read(entry), (int)entry.Type);
                cache[entry.Id] = mesh;
            }

            if (mesh is null)
            {
                missing++;
                continue;
            }

            var transform = ObjectTransform(thing);
            foreach (var tri in mesh.Triangles)
            {
                var a = Vector3.Transform(tri.A, transform);
                var b = Vector3.Transform(tri.B, transform);
                var c = Vector3.Transform(tri.C, transform);
                var n = Vector3.TransformNormal(tri.Normal, transform);
                if (n.LengthSquared() < 1e-8f)
                    n = Vector3.UnitZ;
                else
                    n = Vector3.Normalize(n);
                triangles.Add(new MeshTriangle(a, b, c, n, tri.UvA, tri.UvB, tri.UvC, tri.TextureId));
            }

            instances++;
        }

        return new WorldGeometry
        {
            Region = region,
            Triangles = triangles,
            MeshInstances = instances,
            MissingMeshes = missing,
        };
    }

    /// <summary>
    /// C3D meshes are Z-up centimetres. TNG RHSetForward/RHSetUp are a
    /// right-handed Z-up basis. CreateWorld is Y-up and negates forward —
    /// that laid lamps and trees on their side.
    /// </summary>
    public static Matrix4x4 ObjectTransform(ThingInstance thing)
    {
        var position = new Vector3(thing.PositionX!.Value, thing.PositionY!.Value, thing.PositionZ!.Value);
        var forward = ReadAxis(thing, "CTCPhysicsStandard.RHSetForward", Vector3.UnitY);
        var up = ReadAxis(thing, "CTCPhysicsStandard.RHSetUp", Vector3.UnitZ);
        if (forward.LengthSquared() < 1e-6f)
            forward = Vector3.UnitY;
        if (up.LengthSquared() < 1e-6f)
            up = Vector3.UnitZ;
        forward = Vector3.Normalize(forward);
        up = Vector3.Normalize(up);
        var right = Vector3.Cross(forward, up);
        if (right.LengthSquared() < 1e-8f)
            right = Vector3.Cross(forward, Vector3.UnitZ);
        if (right.LengthSquared() < 1e-8f)
            right = Vector3.UnitX;
        right = Vector3.Normalize(right);
        up = Vector3.Normalize(Vector3.Cross(right, forward));

        var basis = new Matrix4x4(
            right.X, right.Y, right.Z, 0,
            forward.X, forward.Y, forward.Z, 0,
            up.X, up.Y, up.Z, 0,
            position.X, position.Y, position.Z, 1);
        return Matrix4x4.CreateScale(MeshToWorld) * basis;
    }

    private static Vector3 ReadAxis(ThingInstance thing, string prefix, Vector3 fallback)
    {
        if (thing.Properties.TryGetValue(prefix + "X", out var xText) &&
            thing.Properties.TryGetValue(prefix + "Y", out var yText) &&
            thing.Properties.TryGetValue(prefix + "Z", out var zText) &&
            float.TryParse(xText, out var x) &&
            float.TryParse(yText, out var y) &&
            float.TryParse(zText, out var z))
            return new Vector3(x, y, z);
        return fallback;
    }
}
