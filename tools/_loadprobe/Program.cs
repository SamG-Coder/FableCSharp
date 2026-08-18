using System.Diagnostics;
using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Defs;
using Fable.Formats.Levels;
using Fable.Formats.Meshes;
using Fable.Formats.Tng;
using Fable.Formats.Wld;
using Fable.Game;
using Fable.Render;

static class LoadProbe
{
    static int Main(string[] args)
    {
        var install = GameInstall.TryLocate();
        if (install is null)
        {
            Console.Error.WriteLine("FABLE_PATH / TLC install not found");
            return 2;
        }

        if (args.Any(a => a.Equals("breakdown", StringComparison.OrdinalIgnoreCase)))
        {
            Breakdown.Run(install);
            return 0;
        }

        if (args.Any(a => a.Equals("spine", StringComparison.OrdinalIgnoreCase)))
        {
            RunSpine(install);
            return 0;
        }

        Console.WriteLine($"install {install.Edition} {install.Root}");
        Console.WriteLine($"data {install.DataRoot}");
        PrintSize("wld", install.WorldPath);
        PrintSize("wad", install.WadPath);
        PrintSize("stb", install.RuntimeStbPath);
        PrintSize("bwd", install.BwdPath);
        PrintSize("graph", install.StartingRegionGraphPath);
        PrintSize("graphics.big", Path.Combine(install.DataRoot, "graphics", "graphics.big"));
        PrintSize("textures.big", Path.Combine(install.DataRoot, "graphics", "pc", "textures.big"));
        var gameBin = install.FindCompiledDef("game.bin");
        var namesBin = install.FindCompiledDef("names.bin");
        var scriptBin = install.FindCompiledDef("script.bin");
        if (gameBin is not null) PrintSize("game.bin", gameBin);
        if (namesBin is not null) PrintSize("names.bin", namesBin);
        if (scriptBin is not null) PrintSize("script.bin", scriptBin);

        Console.WriteLine();
        Console.WriteLine("=== isolated (first call, then repeat) ===");

        var world = Time("WLD parse", () => WorldFile.Load(install.WorldPath));
        Time("WLD parse (2)", () => WorldFile.Load(install.WorldPath));
        Console.WriteLine($"  maps={world.Maps.Count} regions={world.Regions.Count} proximity={world.Maps.Count(m => m.LoadedOnPlayerProximity)}");
        var lookout = world.FindRegionContaining("LookoutPoint");
        Console.WriteLine($"  Lookout ContainsMaps={string.Join(",", lookout?.ContainsMaps ?? [])}");
        Console.WriteLine($"  Lookout SeesMaps={string.Join(",", lookout?.SeesMaps ?? [])}");

        var graph = Time("region graph", () => RegionGraph.Load(install.StartingRegionGraphPath));
        Time("region graph (2)", () => RegionGraph.Load(install.StartingRegionGraphPath));
        Console.WriteLine($"  nodes={graph.Neighbors.Count}");

        var bwd = Time("BWD", () => BwdFile.Load(install.BwdPath));
        var around = MapsAround(world, bwd, "LookoutPoint");
        Console.WriteLine($"  StaticMapsAround Lookout n={around.Count} {string.Join(",", around.Select(m => m.ScriptName))}");

        var wad = Time("WAD open/index", () => BbbArchive.Open(install.WadPath));
        Time("WAD open/index (2)", () =>
        {
            using var w = BbbArchive.Open(install.WadPath);
            return w.Entries.Count;
        });
        Console.WriteLine($"  wad entries={wad.Entries.Count}");

        var stb = Time("STB open/index", () => StbArchive.Open(install.RuntimeStbPath));
        Time("STB open/index (2)", () =>
        {
            using var s = StbArchive.Open(install.RuntimeStbPath);
            return s.Entries.Count;
        });
        Console.WriteLine($"  stb entries={stb.Entries.Count}");

        Console.WriteLine("  per-map TNG (ContainsMap + StaticMapsAround):");
        var tngTargets = lookout!.ContainsMaps
            .Concat(around.Select(m => m.ScriptName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        foreach (var name in tngTargets)
        {
            var map = world.FindMap(name);
            if (map is null) continue;
            var tng = Time($"    TNG {name}", () => LoadTng(install, wad, map));
            Console.WriteLine($"      things={tng?.Things.Count() ?? 0} loose={File.Exists(Path.Combine(install.LooseLevelsDirectory, map.FileStem + ".tng"))}");
        }

        var proxMaps = world.Maps.Where(m => m.LoadedOnPlayerProximity).ToList();
        var prox = Time("all proximity TNG (004FDBC0)", () =>
        {
            var n = 0;
            var things = 0;
            foreach (var map in proxMaps)
            {
                var tng = LoadTng(install, wad, map);
                if (tng is null) continue;
                n++;
                things += tng.Things.Count();
            }
            return (n, things);
        });
        Console.WriteLine($"  proximity maps-with-tng={prox.n} things={prox.things} candidates={proxMaps.Count}");

        if (namesBin is not null)
        {
            var names = Time("names.bin", () => NamesBin.Load(namesBin));
            Time("names.bin (2)", () => NamesBin.Load(namesBin));
            if (gameBin is not null)
            {
                var defs = Time("game.bin parse", () => GameBin.Load(gameBin, names));
                Time("game.bin parse (2)", () => GameBin.Load(gameBin, names));
                Console.WriteLine($"  game.bin entries={defs.Entries.Count}");
            }
            if (scriptBin is not null)
            {
                Time("script.bin+ScriptBank", () => ScriptBank.Load(install));
                Time("script.bin+ScriptBank (2)", () => ScriptBank.Load(install));
            }
        }

        var levels = Time("LevelLibrary ctor (WLD+WAD+STB)", () => new LevelLibrary(install));
        Time("LevelLibrary ctor (2)", () =>
        {
            using var l = new LevelLibrary(install);
            return 0;
        });

        Console.WriteLine("  PeekMapHeader / LEV parse / STB height / tessellate:");
        foreach (var map in around)
        {
            var header = Time($"    Peek {map.ScriptName}", () => levels.PeekMapHeader(map.ScriptName));
            Console.WriteLine($"      lev={header?.CompiledSize} stb={header?.StbSize} grid={header?.GridWidth}x{header?.GridHeight} samples={header?.HeightSamples}");
            var lev = Time($"    LEV parse {map.ScriptName}", () => levels.LoadCompiledLev(map.ScriptName));
            var height = Time($"    STB height {map.ScriptName}", () => levels.LoadHeightField(map.ScriptName));
            if (lev is not null && height is not null)
            {
                var tris = Time($"    tessellate {map.ScriptName}", () =>
                {
                    var cells = LevCellGrid.TryParse(lev);
                    return cells is null
                        ? height.ToLocalTriangles().Count
                        : height.ToTileTriangles(cells, lev.Materials).Count;
                });
                Console.WriteLine($"      levBytes={lev.Raw.Length} tiles={height.TileCount} fine={height.FineWidth}x{height.FineHeight} tris={tris}");
            }
            else
                Console.WriteLine($"      missing lev={lev is not null} height={height is not null}");
        }

        var meshes = Time("graphics.big MeshBank.Open", () =>
        {
            var bank = new MeshBank();
            bank.Open(install);
            return bank;
        });
        Time("graphics.big MeshBank.Open (2)", () =>
        {
            var bank = new MeshBank();
            bank.Open(install);
            bank.Dispose();
            return 0;
        });
        Console.WriteLine($"  mesh entries={meshes.EntryCount}");

        var textures = Time("textures.big TextureLibrary ctor", () => new TextureLibrary(install));
        Time("textures.big TextureLibrary ctor (2)", () =>
        {
            using var t = new TextureLibrary(install);
            return 0;
        });

        Console.WriteLine();
        Console.WriteLine("=== New Game lifecycle (videos skipped) ===");
        using (var life = new EngineLifecycle())
        {
            Time("Bootstrap", () =>
            {
                life.Bootstrap(install);
                return life.Stage;
            });
            Time("skip videos", () =>
            {
                while (life.Stage == EngineStage.StartupVideos)
                    life.FinishStartupVideo();
                return life.Stage;
            });
            var entered = Time("EnterGame (WLD+global TNG+graph+quests+banks)", () =>
            {
                life.RequestNewGame();
                life.EnterGame();
                return (life.World?.Maps.Count, life.GlobalThingMapsLoaded, life.Meshes.Opened,
                    life.Textures is not null, life.Meshes.ParsedCount);
            });
            Console.WriteLine($"  maps={entered.Item1} globalTngMaps={entered.Item2} meshOpen={entered.Item3} texOpen={entered.Item4} c3d={entered.Item5}");

            var p1 = Time("Pump1 dummy (no region)", () =>
            {
                life.Pump();
                return (life.CurrentRegionIndex, life.HeroSpawned, life.WorldSubmitted, life.ActivatedMaps.Count);
            });
            Console.WriteLine($"  region={p1.Item1} hero={p1.Item2} submitted={p1.Item3} activated={p1.Item4}");

            var p2 = Time("Pump2 Lookout load + SubmitCurrentWorld", () =>
            {
                life.Pump();
                return (
                    life.CurrentRegion?.RegionName,
                    life.ActivatedMaps.Count,
                    life.OpenedStaticMaps.Count,
                    life.RegionThingMapsLoaded,
                    life.HeroSpawned,
                    life.WorldSubmitted,
                    life.SubmittedMesh?.Vertices.Length ?? 0,
                    life.SubmittedWorld?.MeshInstances ?? 0,
                    life.Meshes.ParsedCount,
                    life.SubmittedTextures.Count,
                    life.SubmittedTerrainMaps.Count);
            });
            Console.WriteLine($"  region={p2.Item1} activated={p2.Item2} opened={p2.Item3} tngMaps={p2.Item4}");
            Console.WriteLine($"  hero={p2.Item5} submitted={p2.Item6} verts={p2.Item7} inst={p2.Item8} c3d={p2.Item9} tex={p2.Item10} terrainMaps={p2.Item11}");
            Console.WriteLine($"  activatedMaps={string.Join(",", life.ActivatedMaps)}");
            Console.WriteLine($"  openedMaps={string.Join(",", life.OpenedStaticMaps)}");
            Console.WriteLine($"  terrain={string.Join(",", life.SubmittedTerrainMaps)}");
            Console.WriteLine($"  palskin={string.Join(",", life.SubmittedPalskinMeshIds)} heroMesh={life.HeroMeshId} expanded={life.SubmittedWorld?.Expanded}");
            Console.WriteLine();
            Console.WriteLine("--- EngineLifecycle.Timing ---");
            Console.WriteLine(life.Timing.Format());
            if (life.LastLoadTiming is { } submitClock)
            {
                Console.WriteLine();
                Console.WriteLine("--- LastLoadTiming (submit) ---");
                Console.WriteLine(submitClock.Format());
            }

            Time("Pump3 already submitted", () =>
            {
                life.Pump();
                return (life.WorldSubmitted, life.GamePumpFrames, life.Meshes.ParsedCount);
            });

            var presented = Time("PresentWorld (2) header-only", () => life.PresentWorld());
            Console.WriteLine($"  present2 expanded={presented?.Expanded} tris={presented?.Triangles.Count} inst={presented?.MeshInstances} maps={presented?.Regions.Count}");

            if (life.SubmittedWorld is { } opened && life.Levels is { } lib)
            {
                var vis = Time("TessellateVisible (2)", () => opened.TessellateVisible(lib));
                var prim = Time("TessellatePrimary (2)", () => opened.TessellatePrimary(lib));
                Console.WriteLine($"  visTris={vis.Count} primTris={prim.Count}");
                var land = Time("MeshBatches.Build land (2)", () => MeshBatches.Build(vis));
                Console.WriteLine($"  landVerts={land.Vertices.Length} draws={land.Draws.Length}");

                var props = new List<(MeshFile Mesh, System.Numerics.Matrix4x4 Transform)>();
                foreach (var inst in opened.Instances)
                {
                    if (!inst.Map.Equals(opened.Region, StringComparison.OrdinalIgnoreCase))
                        continue;
                    var instMesh = life.Meshes.Get(inst.MeshId);
                    if (instMesh is not null)
                        props.Add((instMesh, inst.Transform));
                }
                var built = Time("MeshBatches.BuildMeshes (2, cached C3D)", () => MeshBatches.BuildMeshes(props));
                Console.WriteLine($"  propInst={props.Count} propVerts={built.Vertices.Length}");

                var rgbaBytes = 0L;
                var texN = Time("texture decode already cached LoadMany", () =>
                {
                    var ids = (life.SubmittedMesh?.Draws ?? []).SelectMany(d => new[] { d.TextureId, d.TextureId1 });
                    var files = life.Textures!.LoadMany(ids);
                    rgbaBytes = files.Sum(f => (long)f.Rgba.Length);
                    return files.Count;
                });
                Console.WriteLine($"  cached tex files={texN} rgbaBytes={rgbaBytes}");
            }

            if (life.SubmittedMesh is { } submitted)
            {
                var bytes = submitted.Vertices.Length * (int)MeshVertex.Stride;
                Time("CPU memcpy mesh upload proxy", () =>
                {
                    var dest = new MeshVertex[submitted.Vertices.Length];
                    submitted.Vertices.CopyTo(dest, 0);
                    return dest.Length;
                });
                Console.WriteLine($"  meshBytes={bytes}");
                var texBytes = life.SubmittedTextures.Sum(t => (long)t.Rgba.Length);
                Time("CPU memcpy texture upload proxy", () =>
                {
                    long n = 0;
                    foreach (var t in life.SubmittedTextures)
                    {
                        var dest = new byte[t.Rgba.Length];
                        t.Rgba.CopyTo(dest, 0);
                        n += dest.Length;
                    }
                    return n;
                });
                Console.WriteLine($"  texRgbaBytes={texBytes} texCount={life.SubmittedTextures.Count}");
            }
        }

        wad.Dispose();
        stb.Dispose();
        levels.Dispose();
        meshes.Dispose();
        textures.Dispose();

        Console.WriteLine();
        Console.WriteLine("=== second New Game: process-warm ===");
        using (var life = new EngineLifecycle())
        {
            life.Bootstrap(install);
            while (life.Stage == EngineStage.StartupVideos)
                life.FinishStartupVideo();
            life.RequestNewGame();
            Time("EnterGame (2nd process-warm)", () =>
            {
                life.EnterGame();
                return 0;
            });
            life.Pump();
            Time("Pump2 full (2nd)", () =>
            {
                life.Pump();
                return (
                    life.SubmittedMesh?.Vertices.Length ?? 0,
                    life.Meshes.ParsedCount,
                    life.SubmittedTextures.Count);
            });
        }

        Console.WriteLine();
        Console.WriteLine("=== cold C3D sample of Lookout instance ids ===");
        using (var life = new EngineLifecycle())
        {
            life.Bootstrap(install);
            while (life.Stage == EngineStage.StartupVideos)
                life.FinishStartupVideo();
            life.RequestNewGame();
            life.EnterGame();
            life.Pump();
            life.Pump();
            var opened = life.SubmittedWorld;
            if (opened is not null)
            {
                var ids = opened.Instances
                    .Where(i => i.Map.Equals(opened.Region, StringComparison.OrdinalIgnoreCase))
                    .Select(i => i.MeshId)
                    .Distinct()
                    .ToList();
                Console.WriteLine($"  unique primary mesh ids={ids.Count} parsedDuringSubmit={life.Meshes.ParsedCount}");
                using var fresh = new MeshBank();
                Time("fresh MeshBank.Open", () =>
                {
                    fresh.Open(install);
                    return fresh.EntryCount;
                });
                var parsed = Time($"fresh parse {ids.Count} C3Ds", () =>
                {
                    var n = 0;
                    var tris = 0;
                    foreach (var id in ids)
                    {
                        var mesh = fresh.Get(id);
                        if (mesh is null) continue;
                        n++;
                        tris += mesh.Triangles.Count;
                    }
                    return (n, tris);
                });
                Console.WriteLine($"  parsed={parsed.n} tris={parsed.tris}");
                Time("cached Get same ids", () =>
                {
                    foreach (var id in ids)
                        fresh.Get(id);
                    return ids.Count;
                });
            }
        }

        return 0;
    }

    static void RunSpine(GameInstall install)
    {
        Console.WriteLine($"install {install.Edition} {install.Root}");
        Console.WriteLine("=== New Game spine ===");
        using var life = new EngineLifecycle();
        Time("Bootstrap", () => { life.Bootstrap(install); return life.Stage; });
        Time("skip videos", () =>
        {
            while (life.Stage == EngineStage.StartupVideos)
                life.FinishStartupVideo();
            return life.Stage;
        });
        Time("RequestNewGame", () => { life.RequestNewGame(); return life.Stage; });
        Time("EnterGame", () =>
        {
            life.EnterGame();
            return (life.World?.Maps.Count, life.GlobalThingMapsLoaded, life.Meshes.EntryCount,
                life.Meshes.ParsedCount, life.Textures is not null);
        });
        Time("Pump1 dummy", () => { life.Pump(); return life.CurrentRegionIndex; });
        var p2 = Time("Pump2 load+submit", () =>
        {
            life.Pump();
            return (
                life.CurrentRegion?.RegionName,
                life.OpenedStaticMaps.Count,
                life.RegionThingMapsLoaded,
                life.SubmittedMesh?.Vertices.Length ?? 0,
                life.SubmittedLandscapeCells,
                life.Meshes.ParsedCount,
                life.SubmittedTextures.Count,
                life.SubmitElapsedMs);
        });
        Console.WriteLine($"  region={p2.Item1} opened={p2.Item2} tngMaps={p2.Item3}");
        Console.WriteLine($"  verts={p2.Item4} cells={p2.Item5} c3d={p2.Item6} tex={p2.Item7} submitMs={p2.Item8:0}");
        Console.WriteLine($"  openedMaps={string.Join(",", life.OpenedStaticMaps)}");
        Console.WriteLine($"  terrain={string.Join(",", life.SubmittedTerrainMaps)}");
        Time("Pump3 already submitted", () =>
        {
            life.Pump();
            return (life.WorldSubmitted, life.GamePumpFrames);
        });
        if (life.SubmittedMesh is { } submitted)
        {
            Time("CPU memcpy mesh upload proxy", () =>
            {
                var dest = new MeshVertex[submitted.Vertices.Length];
                submitted.Vertices.CopyTo(dest, 0);
                return dest.Length;
            });
            Console.WriteLine($"  meshBytes={submitted.Vertices.Length * (int)MeshVertex.Stride}");
            Time("CPU memcpy texture upload proxy", () =>
            {
                long n = 0;
                foreach (var t in life.SubmittedTextures)
                {
                    var dest = new byte[t.Rgba.Length];
                    t.Rgba.CopyTo(dest, 0);
                    n += dest.Length;
                }
                return n;
            });
            Console.WriteLine($"  texRgbaBytes={life.SubmittedTextures.Sum(t => (long)t.Rgba.Length)} texCount={life.SubmittedTextures.Count}");
        }

        Console.WriteLine();
        Console.WriteLine("--- EngineLifecycle.Timing ---");
        Console.WriteLine(life.Timing.Format());
        if (life.LastLoadTiming is { } submitClock)
        {
            Console.WriteLine();
            Console.WriteLine("--- LastLoadTiming (submit) ---");
            Console.WriteLine(submitClock.Format());
        }
    }

    static List<WorldMap> MapsAround(WorldFile world, BwdFile bwd, string region)
    {
        var primary = world.FindMap(region);
        if (primary is null)
            return [];
        var maps = new List<WorldMap> { primary };
        var seen = new HashSet<int> { primary.MapUid };
        void TryAdd(WorldMap? map)
        {
            if (map is null || map.IsSea || !seen.Add(map.MapUid))
                return;
            maps.Add(map);
        }

        var cluster = world.FindRegionContaining(primary.ScriptName)
                      ?? world.FindRegionContaining(primary.FileStem);
        if (cluster is not null)
        {
            foreach (var name in cluster.ContainsMaps)
                TryAdd(world.FindMap(name));
            foreach (var name in cluster.SeesMaps)
                TryAdd(world.FindMap(name));
        }

        var home = bwd.Find(primary.ScriptName) ?? bwd.Find(primary.FileStem);
        if (home is null)
            return maps;
        foreach (var map in world.Maps)
        {
            var box = bwd.Find(map.ScriptName) ?? bwd.Find(map.FileStem);
            if (box is null || !home.Value.Touches(box.Value))
                continue;
            TryAdd(map);
        }

        return maps;
    }

    static ThingFile? LoadTng(GameInstall install, BbbArchive wad, WorldMap map)
    {
        var loose = Path.Combine(install.LooseLevelsDirectory, map.FileStem + ".tng");
        if (File.Exists(loose))
            return ThingFile.Load(loose);
        var entry = wad.Find(map.FileStem + ".tng")
                    ?? wad.Find(map.LevelName.Replace(".lev", ".tng", StringComparison.OrdinalIgnoreCase));
        return entry is null ? null : ThingFile.Parse(System.Text.Encoding.ASCII.GetString(wad.Read(entry)));
    }

    static void PrintSize(string label, string path)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine($"  {label}: MISSING {path}");
            return;
        }

        var info = new FileInfo(path);
        Console.WriteLine($"  {label}: {info.Length:N0} bytes");
    }

    static T Time<T>(string name, Func<T> action)
    {
        var sw = Stopwatch.StartNew();
        var result = action();
        sw.Stop();
        Console.WriteLine($"{sw.Elapsed.TotalMilliseconds,10:0.0} ms  {name}");
        return result;
    }
}
