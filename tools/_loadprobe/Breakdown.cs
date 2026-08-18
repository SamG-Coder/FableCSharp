using System.Diagnostics;
using Fable.Core;
using Fable.Formats.Defs;
using Fable.Formats.Levels;
using Fable.Game;
using Fable.Render;

static class Breakdown
{
    public static void Run(GameInstall install)
    {
        Console.WriteLine();
        Console.WriteLine("=== breakdown ===");
        var texHeader = Path.Combine(install.DataRoot, "Defs", "RetailHeaders", "pc", "textures.h");
        var meshHeader = Path.Combine(install.DataRoot, "Defs", "RetailHeaders", "meshdata.h");
        PrintSize("textures.h", texHeader);
        PrintSize("meshdata.h", meshHeader);
        var enums = Ms("HeaderEnums textures.h", () => HeaderEnums.Load(texHeader));
        Ms("HeaderEnums textures.h (2)", () => HeaderEnums.Load(texHeader));
        if (File.Exists(meshHeader))
            Ms("HeaderEnums meshdata.h", () => HeaderEnums.Load(meshHeader));
        var landscape = enums.ByName.Count(kv => kv.Key.StartsWith("LANDSCAPE_", StringComparison.Ordinal));
        Console.WriteLine($"  textures.h names={enums.ByName.Count} LANDSCAPE_*={landscape}");

        using var levels = new LevelLibrary(install);
        var height = Ms("LoadHeightField Lookout", () => levels.LoadHeightField("LookoutPoint"));
        var lev = Ms("LoadCompiledLev Lookout", () => levels.LoadCompiledLev("LookoutPoint"));
        if (height is null || lev is null)
            return;
        var cells = Ms("LevCellGrid.TryParse Lookout", () => LevCellGrid.TryParse(lev));
        if (cells is null)
            return;

        var noEnum = Ms("ToTileTriangles Lookout enums=null", () =>
            height.ToTileTriangles(cells, lev.Materials, null).Count);
        var withEnum = Ms("ToTileTriangles Lookout +textures.h", () =>
            height.ToTileTriangles(cells, lev.Materials, enums).Count);
        Console.WriteLine($"  tris noEnum={noEnum} withEnum={withEnum}");

        using var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Ms("EnterGame", () => { life.EnterGame(); return 0; });
        Ms("Pump1", () => { life.Pump(); return 0; });
        Ms("LoadFromFirstRealRegion", () =>
        {
            life.LoadFromFirstRealRegion();
            return (life.ActivatedMaps.Count, life.OpenedStaticMaps.Count, life.HeroSpawned, life.Meshes.ParsedCount);
        });

        var opened = Ms("PresentWorld first", () => life.PresentWorld());
        Console.WriteLine($"  inst={opened?.MeshInstances} maps={opened?.Regions.Count} c3d={life.Meshes.ParsedCount}");
        if (opened is null || life.Levels is null)
            return;

        var vis = Ms("TessellateVisible first +planes", () =>
            opened.TessellateVisible(life.Levels, life.SubmitSidePlanes()));
        Console.WriteLine($"  vis+planes={vis.Count}");

        var land = Ms("MeshBatches.Build land", () => MeshBatches.Build(vis));
        Console.WriteLine($"  landVerts={land.Vertices.Length}");

        var parsedBefore = life.Meshes.ParsedCount;
        var props = new List<(Fable.Formats.Meshes.MeshFile Mesh, System.Numerics.Matrix4x4 Transform)>();
        Ms("C3D Get primary instances", () =>
        {
            foreach (var inst in opened.Instances)
            {
                if (!inst.Map.Equals(opened.Region, StringComparison.OrdinalIgnoreCase))
                    continue;
                var mesh = life.Meshes.Get(inst.MeshId);
                if (mesh is not null)
                    props.Add((mesh, inst.Transform));
            }
            return props.Count;
        });
        Console.WriteLine($"  parsed {life.Meshes.ParsedCount - parsedBefore} -> {life.Meshes.ParsedCount} props={props.Count}");
        var built = Ms("MeshBatches.BuildMeshes", () => MeshBatches.BuildMeshes(props));
        Console.WriteLine($"  propVerts={built.Vertices.Length}");

        if (life.Textures is null)
            life.OpenTextureBank();
        var ids = land.Draws.Concat(built.Draws).SelectMany(d => new[] { d.TextureId, d.TextureId1 }).Distinct().ToList();
        var files = Ms($"texture first decode n={ids.Count}", () => life.Textures!.LoadMany(ids));
        Console.WriteLine($"  decoded={files.Count} rgba={files.Sum(f => (long)f.Rgba.Length)}");
        Ms("texture decode cached", () => life.Textures!.LoadMany(ids).Count);

        Console.WriteLine();
        Console.WriteLine("--- EngineLifecycle.Timing ---");
        Console.WriteLine(life.Timing.Format());
    }

    static void PrintSize(string label, string path)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine($"  {label}: MISSING");
            return;
        }
        Console.WriteLine($"  {label}: {new FileInfo(path).Length:N0} bytes");
    }

    static T Ms<T>(string name, Func<T> action)
    {
        var sw = Stopwatch.StartNew();
        var result = action();
        sw.Stop();
        Console.WriteLine($"{sw.Elapsed.TotalMilliseconds,10:0.0} ms  {name}");
        return result;
    }
}
