using System.Numerics;
using Fable.Core;
using Fable.Formats;
using Fable.Formats.Defs;
using Fable.Formats.Levels;
using Fable.Formats.Meshes;
using Fable.Formats.Sky;
using Fable.Game;
using Fable.Render;

namespace Fable.Formats.Tests;

public sealed class WorldGeometryTests
{
    [Fact]
    public void Apple_tree_mesh_parses()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var path = Path.Combine(install.DataRoot, "graphics", "graphics.big");
        using var big = Fable.Formats.Banks.BigArchive.Open(path);
        var bank = big.SubBanks.First(item => item.Name.Contains("MESH"));
        var entry = big.ReadEntries(bank).First(e => e.Id == 5228);
        var mesh = MeshFile.Parse(big.Read(entry), (int)entry.Type);
        Assert.True(mesh.Triangles.Count > 100);
    }

    [Fact]
    public void Lookout_tng_is_local_region_space_on_the_heightfield()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var map = levels.World.FindMap("LookoutPoint")!;
        var height = levels.LoadHeightField("LookoutPoint")!;
        var things = levels.LoadThings("LookoutPoint").Things.Where(t => t.PositionX is not null).ToList();
        Assert.True(things.Max(t => t.PositionX) < 130);
        Assert.True(things.Max(t => t.PositionY) < 130);
        Assert.True(things.Min(t => t.PositionX) > -1);
        Assert.DoesNotContain(things, t => t.PositionX > map.MapX);
        var objs = things.Where(t => (t.DefinitionType ?? "").StartsWith("OBJECT_")).ToList();
        var near = 0;
        foreach (var thing in objs)
        {
            var x = (int)MathF.Round(thing.PositionX!.Value);
            var y = (int)MathF.Round(thing.PositionY!.Value);
            if (x < 0 || y < 0 || x > height.FineWidth || y > height.FineHeight)
                continue;
            if (Math.Abs(thing.PositionZ!.Value - height.FineHeights[x, y]) < 1.5f)
                near++;
        }

        Assert.True(near > objs.Count * 3 / 4, $"onGround={near}/{objs.Count}");
    }

    [Fact]
    public void ObjectScale_shrinks_the_instance()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var rock = levels.LoadThings("LookoutPoint").Things
            .First(t => t.DefinitionType == "OBJECT_BRIGHTWOOD_LARGEROCK_04" &&
                        t.Properties.GetValueOrDefault("ObjectScale") == "0.4");
        var scaled = WorldGeometry.ObjectTransform(rock);
        var props = new Dictionary<string, string>(rock.Properties, StringComparer.OrdinalIgnoreCase);
        props["ObjectScale"] = "1.0";
        var clone = new Fable.Formats.Tng.ThingInstance
        {
            Kind = rock.Kind,
            Section = rock.Section,
            DefinitionType = rock.DefinitionType,
            ScriptName = rock.ScriptName,
            Uid = rock.Uid,
            Player = rock.Player,
            PositionX = rock.PositionX,
            PositionY = rock.PositionY,
            PositionZ = rock.PositionZ,
            Properties = props,
        };
        var full = WorldGeometry.ObjectTransform(clone);
        var s = scaled.M11 / full.M11;
        Assert.InRange(Math.Abs(s), 0.35f, 0.45f);
    }

    [Fact]
    public void Streetlamp_stands_on_world_z_not_createworld_y()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var lamp = levels.LoadThings("LookoutPoint").Things
            .First(t => t.DefinitionType == "OBJECT_STREETLAMP_LIT_SINGLE_01");
        var path = Path.Combine(install.DataRoot, "graphics", "graphics.big");
        using var big = Fable.Formats.Banks.BigArchive.Open(path);
        var bank = big.SubBanks.First(item => item.Name.Contains("MESH"));
        var entry = big.ReadEntries(bank).First(e => e.Id == 4978);
        var mesh = MeshFile.Parse(big.Read(entry), (int)entry.Type);
        var extent = mesh.BoundsMax - mesh.BoundsMin;
        Assert.True(extent.Z > extent.X && extent.Z > extent.Y, $"lamp mesh tall={extent}");

        var xform = WorldGeometry.ObjectTransform(lamp);
        var zs = mesh.Triangles.SelectMany(t => new[]
        {
            Vector3.Transform(t.A, xform).Z,
            Vector3.Transform(t.B, xform).Z,
            Vector3.Transform(t.C, xform).Z,
        }).ToList();
        var ys = mesh.Triangles.SelectMany(t => new[]
        {
            Vector3.Transform(t.A, xform).Y,
            Vector3.Transform(t.B, xform).Y,
            Vector3.Transform(t.C, xform).Y,
        }).ToList();
        Assert.True(zs.Max() - zs.Min() > 2.5f, $"world Z span {zs.Min():0.00}..{zs.Max():0.00}");
        Assert.True(zs.Max() - zs.Min() > ys.Max() - ys.Min(), "lamp must be taller in Z than along Y");

        var createWorld = Matrix4x4.CreateScale(WorldGeometry.MeshToWorld) *
                          Matrix4x4.CreateWorld(
                              new Vector3(lamp.PositionX!.Value, lamp.PositionY!.Value, lamp.PositionZ!.Value),
                              Vector3.Normalize(new Vector3(
                                  float.Parse(lamp.Properties["CTCPhysicsStandard.RHSetForwardX"]),
                                  float.Parse(lamp.Properties["CTCPhysicsStandard.RHSetForwardY"]),
                                  float.Parse(lamp.Properties["CTCPhysicsStandard.RHSetForwardZ"]))),
                              Vector3.UnitZ);
        var cwZ = mesh.Triangles.Select(t => Vector3.Transform(t.A, createWorld).Z).ToList();
        Assert.True(cwZ.Max() - cwZ.Min() < 1.5f, "CreateWorld still lays the Z-up lamp on its side");
    }

    [Fact]
    public void Lookout_point_instances_world_meshes()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var things = levels.LoadThings("LookoutPoint");
        var world = WorldGeometry.Build(install, "LookoutPoint", things.Things);
        Assert.True(world.MeshInstances > 5, $"instances={world.MeshInstances} missing={world.MissingMeshes}");
        Assert.True(world.Triangles.Count > 100);
        Assert.True(world.Triangles.Count > 128, "expected terrain quads plus props");
        Assert.Contains(world.Triangles, tri =>
            tri.TextureId is 4133 or 414 or TextureLibrary.LandscapeGrassPlainId);
        Assert.Contains(world.Triangles, tri => tri.TextureId > 0 && tri.TextureId != TextureLibrary.LandscapeGrassPlainId);
        Assert.True(world.MeshInstances > 150, $"game.bin should instance walls/rocks/lamps; instances={world.MeshInstances} missing={world.MissingMeshes}");
    }

    [Fact]
    public void Open_records_instances_without_c3d_or_tiles()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        using var meshes = new MeshBank();
        meshes.Open(install);
        var things = levels.LoadThings("LookoutPoint");
        var opened = WorldGeometry.Build(
            install, "LookoutPoint", things.Things,
            adjacentStaticMaps: false,
            levels: levels,
            meshes: meshes,
            expandGeometry: false);
        Assert.False(opened.Expanded);
        Assert.Empty(opened.Triangles);
        Assert.True(opened.MeshInstances > 150, $"instances={opened.MeshInstances}");
        Assert.Equal(opened.MeshInstances, opened.Instances.Count);
        Assert.Equal(0, meshes.ParsedCount);
        var drawn = opened.Expand(install, levels, meshes, primaryOnly: true);
        Assert.True(drawn.Expanded);
        Assert.True(drawn.Triangles.Count > 128);
        Assert.True(meshes.ParsedCount > 0);
        Assert.All(drawn.Regions, name =>
            Assert.Equal("LookoutPoint", name));
    }

    [Fact]
    public void Lookout_scene_opens_aabb_adjacent_static_maps()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var things = levels.LoadThings("LookoutPoint");
        var world = WorldGeometry.Build(install, "LookoutPoint", things.Things);

        Assert.Contains("LookoutPoint", world.Regions);
        Assert.Contains("PicnicArea", world.Regions);
        Assert.Contains("BowerstoneBridge", world.Regions);
        Assert.Contains("Greatwood_1", world.Regions);
        Assert.Contains("Greatwood_2", world.Regions);
        Assert.Contains("GuildExterior", world.Regions);
        Assert.Contains("PicnicArea_Filler_02", world.Regions);
        Assert.Contains("PicnicArea_Filler_03", world.Regions);
        Assert.DoesNotContain("BowerstoneSlums", world.Regions);

        var minX = world.Triangles.Min(t => MathF.Min(t.A.X, MathF.Min(t.B.X, t.C.X)));
        var maxX = world.Triangles.Max(t => MathF.Max(t.A.X, MathF.Max(t.B.X, t.C.X)));
        var minY = world.Triangles.Min(t => MathF.Min(t.A.Y, MathF.Min(t.B.Y, t.C.Y)));
        var maxY = world.Triangles.Max(t => MathF.Max(t.A.Y, MathF.Max(t.B.Y, t.C.Y)));
        Assert.True(minX < -1f, $"west Picnic tiles missing, minX={minX}");
        Assert.True(maxX > 129f, $"east Guild tiles missing, maxX={maxX}");
        Assert.True(minY < -1f, $"south Greatwood tiles missing, minY={minY}");
        Assert.True(maxY > 129f, $"north Bridge tiles missing, maxY={maxY}");
        Assert.True(world.MeshInstances > 192, $"neighbour props missing; instances={world.MeshInstances}");
        Assert.Contains(world.Triangles, t => t.Layer == Fable.Formats.Meshes.SceneLayer.Sky);
        Assert.Contains(world.Triangles, t => t.TextureId == Fable.Formats.Sky.SkyDef.MiddaySkyTextureId);
        Assert.Contains(world.Triangles, t => t.TextureId1 != 0 && t.TextureId1 != t.TextureId);
        var sand = world.Triangles.First(t => t.TextureId == 4133);
        Assert.Equal(1f, sand.ColorA.X, 2);
        Assert.Equal(1f, sand.ColorA.Y, 2);
        Assert.Equal(1f, sand.ColorA.Z, 2);
    }

    [Fact]
    public void TessellateVisible_uses_00bdc2d0_aabb()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var things = levels.LoadThings("LookoutPoint");
        var opened = WorldGeometry.Build(
            install, "LookoutPoint", things.Things,
            adjacentStaticMaps: false,
            levels: levels,
            onlyMaps: ["LookoutPoint", "PicnicArea"],
            expandGeometry: false);
        Assert.False(opened.Expanded);
        var unculled = opened.TessellateVisible(levels);
        Assert.True(unculled.Count > 128);

        LandscapeFrustum.LetterboxCots(
            float.DegreesToRadians(60f), 4f / 3f, 1f, out var cotH, out var cotV);
        var planes = LandscapeFrustum.ExtractSidePlanes(
            new Vector3(64f, 64f, 10f), Vector3.UnitY, Vector3.UnitZ, cotH, cotV);
        var accepted = new List<string>();
        var visible = opened.TessellateVisible(levels, planes, accepted);
        Assert.Contains("LookoutPoint", accepted);
        Assert.True(visible.Count > 0);

        var primary = levels.World.FindMap("LookoutPoint")!;
        var picnic = levels.World.FindMap("PicnicArea")!;
        var height = levels.LoadHeightField("PicnicArea")!;
        var off = Fable.Formats.World.WorldSpaces.NeighbourRegionOffset(
            picnic.MapX, picnic.MapY, primary.MapX, primary.MapY);
        LandscapeFrustum.PatchAabb(
            off.X, off.Y, height.FineWidth, height.FineHeight, out var min, out var max);
        if (LandscapeFrustum.AabbIsOutside(min, max, planes))
            Assert.DoesNotContain("PicnicArea", accepted);
        else
            Assert.Contains("PicnicArea", accepted);
    }

    [Fact]
    public void New_game_oakvale_loads_contains_and_sees_maps()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var things = levels.LoadThings("StartOakValeWest");
        var world = WorldGeometry.Build(install, "StartOakValeWest", things.Things);

        foreach (var name in new[]
                 {
                     "StartOakValeWest", "StartOakValeEast", "StartOakvaleMemorialGarden",
                     "StartOakVale_Filler_01", "StartOakVale_Filler_02", "StartOakVale_Filler_03",
                     "StartOakVale_Filler_04", "StartOakVale_Filler_05",
                     "StartOakVale_Sea_01", "StartOakVale_Sea_02", "StartOakVale_Sea_03",
                     "StartOakVale_Sea_04",
                 })
            Assert.Contains(name, world.Regions);

        var minX = world.Triangles.Min(t => MathF.Min(t.A.X, MathF.Min(t.B.X, t.C.X)));
        var maxX = world.Triangles.Max(t => MathF.Max(t.A.X, MathF.Max(t.B.X, t.C.X)));
        var minY = world.Triangles.Min(t => MathF.Min(t.A.Y, MathF.Min(t.B.Y, t.C.Y)));
        var maxY = world.Triangles.Max(t => MathF.Max(t.A.Y, MathF.Max(t.B.Y, t.C.Y)));
        Assert.True(minX < -90f, $"west filler/sea missing, minX={minX}");
        Assert.True(maxX > 129f, $"east village missing, maxX={maxX}");
        Assert.True(minY < -1f, $"south sea missing, minY={minY}");
        Assert.True(maxY > 224f, $"north filler missing, maxY={maxY}");
        Assert.DoesNotContain(world.Triangles, t => t.TextureId == 442);
        Assert.False(LandscapeTextures.FirstSeenWaterDrawShouldSubmit);
        Assert.False(LandscapeTextures.FirstSeenWaterDrawReachesSecondGate);
        Assert.Equal(4300, world.PlayerMeshId);
        Assert.False(WorldShading.FirstSeenLodInfoSwapsMesh);
        Assert.Equal(1, WorldShading.MeshLodInfoReady_00A23DE0(0));
        Assert.InRange(world.PlayerHeight, 1.0f, 2.2f);
        var start = RegionTravel.FindPlayerStart(things.Things);
        Assert.NotNull(start);
        var nearKid = world.Triangles.Count(t =>
        {
            var mx = (t.A.X + t.B.X + t.C.X) / 3f;
            var my = (t.A.Y + t.B.Y + t.C.Y) / 3f;
            var dx = mx - start.PositionX!.Value;
            var dy = my - start.PositionY!.Value;
            return dx * dx + dy * dy < 4f && t.Layer == Fable.Formats.Meshes.SceneLayer.Prop;
        });
        Assert.True(nearKid > 10, $"kid mesh missing at NOVStartHSP nearKid={nearKid}");

        var house = things.Things.First(t => t.ScriptName == "HerosOldHouse");
        Assert.Equal("BUILDING_OAKVALE_HOUSE_MEDIUM_SINGLE_FLOOR_BUYABLE", house.DefinitionType);
        var houseTris = CountPropNear(world, house.PositionX!.Value, house.PositionY!.Value, 20f);
        Assert.True(houseTris > 100, $"HerosOldHouse mesh missing houseTris={houseTris}");
        var hx = house.PositionX!.Value;
        var hy = house.PositionY!.Value;
        var interiorWalls = world.Triangles.Count(t =>
            t.Layer == Fable.Formats.Meshes.SceneLayer.Prop
            && t.TextureId == GameBin.HerosOldHouseInteriorWallTexture
            && NearXY(t, hx, hy, 20f));
        Assert.True(interiorWalls > 100, $"interior 6911 walls missing n={interiorWalls}");
        Assert.DoesNotContain(
            world.Triangles.Where(t => NearXY(t, hx, hy, 20f)),
            t => t.TextureId == GameBin.HerosOldHouseFloorTexture);
        var landFloor = world.Triangles.Count(t =>
            t.Layer == Fable.Formats.Meshes.SceneLayer.Landscape
            && t.TextureId == 4130
            && NearXY(t, hx, hy, 12f));
        Assert.True(landFloor > 0, $"house floor landscape PATH_STONEY missing n={landFloor}");
        foreach (var def in new[]
                 {
                     "OBJECT_KHG_BED_03", "OBJECT_TABLE_LARGE_ROUND_01",
                     "OBJECT_WOODEN_LAMP_OFF", "OBJECT_BS_RUG_ROUND_DIAMONDS_01",
                     "GENERIC_INTERNAL_FIREPLACE", "OBJECT_BUILDING_DOOR_3",
                     "OBJECT_CHAIR_01", "OBJECT_CUPBOARD_MEDIUM",
                     "OBJECT_BOOKSHELF_01", "OBJECT_HOME_TABLE_3_STOOLS",
                     "OBJECT_KHG_BED_01", "OBJECT_BS_TABLELAMP_UNLIT_01",
                 })
        {
            var thing = things.Things.First(t => t.DefinitionType == def && t.PositionX is not null);
            var n = CountPropNear(world, thing.PositionX!.Value, thing.PositionY!.Value, 3f);
            Assert.True(n > 0, $"{def} emitted 0 prop tris");
        }

        var height = levels.LoadHeightField("StartOakValeWest")!;
        var compiled = levels.LoadCompiledLev("StartOakValeWest")!;
        var cells = Fable.Formats.Levels.LevCellGrid.TryParse(compiled)!;
        var enums = Fable.Formats.Defs.HeaderEnums.Load(
            Path.Combine(install.DataRoot, "Defs", "RetailHeaders", "pc", "textures.h"));
        var stripOnly = height.Tiles.ToTriangles(height.OriginX, height.OriginY, cells, compiled.Materials, enums);
        var drawn = height.ToTileTriangles(cells, compiled.Materials, enums);
        Assert.Equal(stripOnly.Count, drawn.Count);

        var namesPath = install.FindCompiledDef("names.bin");
        var binPath = install.FindCompiledDef("game.bin");
        Assert.NotNull(namesPath);
        Assert.NotNull(binPath);
        var bin = GameBin.Load(binPath, NamesBin.Load(namesPath));
        var nearHouse = things.Things
            .Where(t => t.DefinitionType is not null && t.PositionX is not null
                && MathF.Abs(t.PositionX.Value - hx) < 25f
                && MathF.Abs(t.PositionY!.Value - hy) < 25f)
            .ToList();
        var worldTypes = nearHouse
            .Select(t => t.DefinitionType!)
            .Where(d => d.StartsWith("OBJECT_", StringComparison.Ordinal)
                || d.StartsWith("BUILDING_", StringComparison.Ordinal)
                || d.StartsWith("CREATURE_", StringComparison.Ordinal)
                || d.StartsWith("GENERIC_", StringComparison.Ordinal))
            .Distinct()
            .OrderBy(d => d)
            .ToList();
        var unresolved = worldTypes.Where(d => bin.FindMeshIds(d).Count == 0).ToList();
        Assert.True(unresolved.Count == 0,
            "first-seen house-area OBJECT/BUILDING/CREATURE/GENERIC without Graphic: "
            + string.Join(", ", unresolved));
        var kid = bin.FindEntry(RegionTravel.KidCreature);
        Assert.NotNull(kid);
        Assert.Equal(4300, kid.MeshId);
        var kidChildTypes = kid.SubDefs
            .Where(s => (uint)s.DefIndex < (uint)bin.Entries.Count)
            .Select(s => bin.Entries[s.DefIndex].TypeName ?? "")
            .ToList();
        Assert.DoesNotContain("CMultiStaticMeshDef", kidChildTypes);
        Assert.True(GameBin.FirstSeenHouseAreaDefsResolveGraphic);
        Assert.True(GameBin.FirstSeenMultiStaticPersistMapsFileFields);
        Assert.Equal(-1, GameBin.FirstSeenSkipGlobal);
        Assert.False(GameBin.FirstSeenSkipGlobalHasWriter);
        Assert.Equal(0x0137B530u, GameBin.MultiStaticEntryRtti);
        Assert.True(GameBin.FirstSeenMissingMeshesAreGizmos);
        Assert.False(GameBin.FirstSeenInstancesAsC3d("MARKER", "MARKER_BASIC"));
        Assert.False(GameBin.FirstSeenInstancesAsC3d("MARKER", "GAZE_OUT_OF_BUILDING_MARKER"));
        Assert.False(GameBin.FirstSeenInstancesAsC3d("MARKER", "GAZE_AT_WORK_MARKER"));
        Assert.False(GameBin.FirstSeenInstancesAsC3d("HOLY_SITE", "HOLY_SITE_PLAYER_START"));
        Assert.False(GameBin.FirstSeenInstancesAsC3d("THING", "PARTICLE_EMITTER_PLACEABLE"));
        Assert.False(GameBin.FirstSeenInstancesAsC3d("THING", "TRACK_NODE_BASIC"));
        Assert.False(GameBin.FirstSeenInstancesAsC3d("THING", "CAMERA_POINT_SCRIPTED_SPLINE"));
        Assert.True(GameBin.FirstSeenInstancesAsC3d("BUILDING", house.DefinitionType));
        var graphicsPath = Path.Combine(install.DataRoot, "graphics", "graphics.big");
        using (var big = Fable.Formats.Banks.BigArchive.Open(graphicsPath))
        {
            var meshBank = big.SubBanks.First(item => item.Name.Contains("MESH", StringComparison.OrdinalIgnoreCase));
            var entries = big.ReadEntries(meshBank);
            var extraMaps = new List<string>();
            foreach (var def in worldTypes)
            {
                foreach (var id in bin.FindMeshIds(def))
                {
                    var entry = entries.FirstOrDefault(item => item.Id == id);
                    if (entry is null)
                        continue;
                    var mesh = MeshFile.TryParse(big.Read(entry), (int)entry.Type);
                    if (mesh is null)
                        continue;
                    extraMaps.AddRange(mesh.Materials
                        .Where(m => m.BumpMapId != 0 || m.ReflectionMapId != 0 || m.IlluminationMapId != 0)
                        .Select(m => $"{def}/{id}/{m.Name} b={m.BumpMapId} r={m.ReflectionMapId} i={m.IlluminationMapId}"));
                }
            }

            Assert.True(WorldShading.FirstSeenHouseAreaC3dStoresBump);
            Assert.False(WorldShading.FirstSeenBindsC3dBump);
            Assert.Equal(1, WorldShading.FirstSeenStaticTextureStages);
            Assert.Equal("PSHADER_TEXTURE_DIFFUSE", WorldShading.FirstSeenStaticPsName);
            Assert.Contains(extraMaps, s => s.Contains($"b={WorldShading.FirstSeenRugBumpMapId}"));
            Assert.Contains(extraMaps, s => s.Contains($"b={WorldShading.FirstSeenBookBumpMapId}"));
            Assert.DoesNotContain(extraMaps, s => s.Contains("r=") && !s.Contains("r=0"));
        }
        Assert.True(world.MissingMeshes == 0,
            "first-seen C3D holes: " + string.Join(", ", world.MissingMeshDefs.Take(20))
            + $" count={world.MissingMeshes}");
    }

    [Fact]
    public void First_seen_house_faces_shot2_and_kid_is_not_on_porch_until_sneak()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var things = levels.LoadThings("StartOakValeWest").Things.ToList();
        var runtime = ScriptRuntime.StartNewGame(install, things);
        var world = WorldGeometry.Build(
            install, "StartOakValeWest", things, actorPositions: runtime.ActorPositions);
        var house = things.First(t => t.ScriptName == "HerosOldHouse");
        var spawn = RegionTravel.FindPlayerStart(things)!;
        var hero = things.First(t => t.ScriptName == RegionTravel.IntroHeroTeleportMarker);
        var sneak = things.First(t => t.ScriptName == RegionTravel.IntroSneakMarker);
        var shot = new ScriptedCamera();
        Assert.True(shot.UseCamera(things, RegionTravel.IntroFirstSeenCamera));
        var stand = new ScriptedCamera();
        Assert.True(stand.UseCamera(things, RegionTravel.IntroStandupCamera));
        var kid = runtime.ActorPositions[RegionTravel.IntroHeroActor];
        var door = things.First(t => t.DefinitionType == "OBJECT_BUILDING_DOOR_3");
        var hx = house.PositionX!.Value;
        var hy = house.PositionY!.Value;
        var near = world.Triangles.Where(t =>
            t.Layer == SceneLayer.Prop && NearXY(t, hx, hy, 20f)).ToList();
        var tex = string.Join(",", near.GroupBy(t => t.TextureId)
            .OrderByDescending(g => g.Count()).Take(8)
            .Select(g => $"{g.Key}x{g.Count()}"));
        var mid = near.Count == 0 ? Vector3.Zero : (near[0].A + near[0].B + near[0].C) / 3f;
        var clip = FlyCamera.Project(shot.ViewProjection(4f / 3f), mid);
        var lookClip = FlyCamera.Project(shot.ViewProjection(4f / 3f), shot.LookAt);
        var houseClip = FlyCamera.Project(
            shot.ViewProjection(4f / 3f), new Vector3(hx, hy, shot.LookAt.Z));
        var anyIn = CountPropInView(world, shot);
        var shotHouse = CountInView(world, shot, GameBin.HerosOldHouseInteriorWallTexture);
        var standHouse = CountInView(world, stand, GameBin.HerosOldHouseInteriorWallTexture);
        var shotExt = CountInView(world, shot, 3180);
        var notes =
            $"SHOT2 {shot.Position} -> {shot.LookAt} int={shotHouse} ext={shotExt} any={anyIn} near={near.Count} tex={tex} " +
            $"mid={mid} clip={clip} lookClip={lookClip} houseClip={houseClip}; " +
            $"STANDUP {stand.Position} -> {stand.LookAt} int={standHouse}; " +
            $"kid {kid} HSP {RegionTravel.PositionOf(spawn)} HERO {RegionTravel.PositionOf(hero)} " +
            $"HERO4 {RegionTravel.PositionOf(sneak)} house {hx},{hy}";
        Assert.InRange(lookClip.X, -0.25f, 0.25f);
        Assert.InRange(lookClip.Y, -0.5f, 0.5f);
        Assert.True(lookClip.W > 0f, "SHOT2 look-at behind camera: " + notes);
        Assert.True(shotHouse + shotExt + anyIn > 20, "house not in SHOT2: " + notes);
        Assert.True(
            DistXy(kid, RegionTravel.PositionOf(hero)) < 1f,
            "script Teleport did not place kid at MK_OVI_ID_HERO: " + notes);
        Assert.Equal(RegionTravel.IntroFirstSeenCamera, shot.ActiveName);
    }

    [Fact]
    public void First_seen_sky_dome_is_6500_by_3250_ellipsoid()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var zenith = SkyPass.EllipsoidPoint(0, 0);
        Assert.InRange(zenith.X, -1f, 1f);
        Assert.InRange(zenith.Y, -1f, 1f);
        Assert.Equal(SkyPass.VertRadius, zenith.Z, 0);
        Assert.Equal(9, SkyPass.DomeRings);
        Assert.Equal(36, SkyPass.DomeSegments);
        Assert.Equal(6500f, SkyPass.HorizRadius);
        Assert.Equal(3250f, SkyPass.VertRadius);
        Assert.Equal(Vector3.Zero, SkyPass.FirstSeenOrigin);
        Assert.Equal(0x00B61DD0u, SkyPass.DomeFill);
        Assert.Equal(0x00B620A0u, SkyPass.DomeSetup);
        Assert.Equal(0x1BB, SkyPass.VertexCount);
        Assert.Equal(24, SkyPass.VertexStrideBytes);
        var sky = SkyGeometry.Build(install);
        var dome = sky.Where(t => t.TextureId == SkyDef.MiddaySkyTextureId).ToList();
        Assert.True(dome.Count > 8 * 36 * 2, $"dome tris={dome.Count}");
        var verts = dome.SelectMany(t => new[] { t.A, t.B, t.C }).ToList();
        Assert.Contains(verts, p => p.Z > 3000f && p.LengthSquared() < 3250f * 3250f + 1f);
        Assert.Contains(verts, p => MathF.Sqrt(p.X * p.X + p.Y * p.Y) > 5000f);
        Assert.DoesNotContain(verts, p =>
        {
            var d = p - new Vector3(64f, 64f, 0f);
            return MathF.Abs(d.Length() - 1800f) < 5f;
        });
    }

    [Fact]
    public void First_seen_sky_dome_colour_is_white_with_vbase_alpha()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        Assert.Equal(1.105f, SkyPass.UvVBaseScale, 4);
        Assert.Equal(255f, SkyPass.ColourScale);
        Assert.Equal(0x00BFEA70u, SkyPass.FloatToInt);
        Assert.Equal(0x00B61EE0u, SkyPass.ColourTail);
        Assert.Equal(0x012A2900u, SkyPass.UvVBaseScaleConst);
        Assert.Equal(0x01230014u, SkyPass.ColourScaleConst);
        Assert.Equal(0x00FFFFFF, SkyPass.ColourRgbMask);
        Assert.Equal(1f, SkyPass.UvVBase(0), 5);
        Assert.Equal(unchecked((int)0xFFFFFFFF), SkyPass.DomeColor(0));
        var horizon = SkyPass.DomeColor(SkyPass.DomeRings - 1);
        Assert.Equal(0x00FFFFFF, horizon & 0x00FFFFFF);
        Assert.InRange((uint)horizon >> 24, 0u, 20u);
        Assert.Equal(255, SkyPass.FloatToByte(255f));
        Assert.Equal(0, SkyPass.FloatToByte(0f));
        Assert.Equal(-0.0001f, SkyPass.CapPoleUv, 6);
        Assert.Equal(0, SkyPass.CapPoleColor);
        Assert.Equal(1f, SkyPass.CapCylinderV);
        Assert.Equal(0x00B627E2u, SkyPass.CtorThis16Write);
        Assert.Equal(292, SkyPass.This16FromOptionsOffset);
        Assert.Equal(288, SkyPass.This20FromOptionsOffset);
        Assert.Equal(296, SkyPass.This12FromOptionsOffset);
        Assert.Equal(0x01436E24u, SkyPass.EnvironmentGlobal);
        Assert.Equal(0x00B26828u, SkyPass.EnvironmentLookup);
        Assert.Equal(0x0143782Cu, SkyPass.UvDivisorGlobal);
        Assert.Equal(0x01224830u, SkyPass.UvDivisorInit);
        Assert.Equal(0x012A1138u, SkyPass.UvDivisorScaleConst);
        Assert.Equal(13000f, SkyPass.UvDivisorScale);
        Assert.True(SkyPass.FirstSeenUvDivisorHasWriter);
        Assert.InRange(SkyPass.FirstSeenUvDivisor, 11500f, 11700f);
        Assert.Equal(
            (float)(13000.0 * Math.Cos(SkyPass.UvDivisorAngle)),
            SkyPass.FirstSeenUvDivisor, 3);
        Assert.True(SkyPass.FirstSeenThis16HasNumeric);
        Assert.True(SkyPass.FirstSeenThis20HasNumeric);
        Assert.Equal(0f, SkyPass.FirstSeenThis16);
        Assert.Equal(0f, SkyPass.FirstSeenThis20);
        Assert.Equal(0x0099AED0u, SkyPass.EnvironmentStringCtor);
        Assert.Equal(0x004310A7u, SkyPass.EnvironmentStringPersist);
        Assert.Equal(Vector2.Zero, SkyPass.DomeUv(
            3, 9, SkyPass.FirstSeenThis16, SkyPass.FirstSeenThis20, SkyPass.FirstSeenInvUvDivisor));
        var installDome = SkyGeometry.Build(install);
        var midday = installDome.Where(t => t.TextureId == SkyDef.MiddaySkyTextureId).ToList();
        Assert.Contains(midday, t => t.UvA == Vector2.Zero && t.UvB == Vector2.Zero && t.UvC == Vector2.Zero);
        Assert.DoesNotContain(midday.Take(8 * 36 * 2), t =>
            t.UvA.X > 0.01f || t.UvA.Y > 0.01f);
        Assert.Equal(0x00B65A20u, SkyPass.StarDraw);
        Assert.Equal(0x00B66190u, SkyPass.StarDrawCallerFn);
        Assert.True(SkyPass.FirstSeenCallsStarDraw);
        Assert.Equal(0x01436E8Cu, SkyPass.MapManagerGlobal);
        Assert.Equal(408, SkyPass.MapManagerWorldOffset);
        Assert.Equal(84, SkyPass.StarObjectFromHopOffset);
        Assert.Equal(424, SkyPass.StarListPointerOffset);
        Assert.Equal(436, SkyPass.StarFadePointerOffset);
        Assert.Equal(0x00B65A87u, SkyPass.StarEmptyRet);
        Assert.True(SkyPass.StarEmptyFirstDwordSkipsDraw);
        Assert.Equal(0x2E8BA2E9u, SkyPass.StarRecordReciprocal);
        Assert.Equal(44, SkyPass.StarRecordStrideBytes);
        Assert.Equal(0x00B64FA0u, SkyPass.WeatherDraw);
        Assert.Equal(0x00B6629Du, SkyPass.WeatherDrawCaller);
        Assert.Equal(0x00B659A5u, SkyPass.WeatherAllZeroRet);
        Assert.Equal(472, SkyPass.WeatherIdPointer0Offset);
        Assert.Equal(448, SkyPass.WeatherIdPointer1Offset);
        Assert.Equal(4, SkyPass.WeatherIdCount);
        Assert.True(SkyPass.WeatherAllZeroIdsSkipDraw);
        Assert.False(SkyPass.FirstSeenWeatherDrawBuildsMesh);
        Assert.Equal(396, SkyPass.SkyWeatherByteOffset);
        Assert.Equal(1, SkyPass.FirstSeenSkyWeatherByte);
        Assert.True(SkyPass.FirstSeenCallsWeatherDraw);
        Assert.False(SkyPass.FirstSeenStarDrawIteratesStarsDat);
        Assert.False(SkyPass.FirstSeenEmitsInventedStarBillboards);
        Assert.Equal(0x00B62BA8u, SkyPass.InnerSkyPsBind);
        Assert.False(SkyPass.FirstSeenSkyPsC2HasWriter);
        Assert.True(SkyPass.FirstSeenSkyMode2IsStandIn);
        Assert.True(SkyPass.FirstSeenInnerSkyMulsVertexAlpha);
        Assert.Contains(midday, t => t.ColorAlphaA > 0.99f);
        Assert.Contains(midday, t => t.ColorAlphaA < 0.1f || t.ColorAlphaC < 0.1f);
        Assert.DoesNotContain(installDome, t => t.TextureId == SkyDef.StarTextureIdDefault);
        var invented = new Vector2(0f / 36f, 0f / 8f);
        Assert.Equal(Vector2.Zero, SkyPass.DomeUv(0, 0, 0f, 0f, SkyPass.FirstSeenInvUvDivisor));
        Assert.NotEqual(invented, SkyPass.DomeUv(1, 9, 1f, 1f, SkyPass.FirstSeenInvUvDivisor));
    }

    [Fact]
    public void First_seen_star_draw_does_not_emit_stars_dat_billboards()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        Assert.True(SkyPass.StarEmptyFirstDwordSkipsDraw);
        Assert.False(SkyPass.FirstSeenStarDrawIteratesStarsDat);
        Assert.False(SkyPass.FirstSeenEmitsInventedStarBillboards);
        Assert.Equal(44, SkyPass.StarRecordStrideBytes);
        Assert.Equal(1, SkyPass.FirstSeenSkyWeatherByte);
        Assert.True(SkyPass.FirstSeenCallsWeatherDraw);
        Assert.Equal(0x00B659A5u, SkyPass.WeatherAllZeroRet);
        Assert.True(SkyPass.WeatherAllZeroIdsSkipDraw);
        Assert.False(SkyPass.FirstSeenWeatherDrawBuildsMesh);
        Assert.Equal(472, SkyPass.WeatherIdPointer0Offset);
        Assert.Equal(448, SkyPass.WeatherIdPointer1Offset);
        Assert.Equal(4, SkyPass.WeatherIdCount);
        Assert.True(SkyPass.WeatherSkipDraw([0, 0, 0, 0]));
        Assert.False(SkyPass.WeatherSkipDraw([0, 0, 0, 1]));
        Assert.False(SkyPass.WeatherSkipDraw([401, 0, 0, 0]));
        Assert.Equal(0x008864A0u, SkyPass.ThemeSlotCtor);
        Assert.Equal(0x008865C0u, SkyPass.ThemeSlotCopy);
        Assert.Equal(0x00886AD2u, SkyPass.ThemeSlotCopyStarWrite);
        Assert.Equal(192, SkyPass.ThemeSlotCopySourcePointerOffset);
        Assert.Equal(428, SkyPass.ThemeSlotVectorOffset);
        Assert.True(SkyPass.FirstSeenStarListPointerCtorZero);
        Assert.False(SkyPass.FirstSeenStarPointerPayloadsAreNumericIds);
        var sky = SkyGeometry.Build(install);
        Assert.DoesNotContain(sky, t => t.TextureId == SkyDef.StarTextureIdDefault);
        var inventedOrigin = new Vector3(64f, 64f, 0f);
        Assert.DoesNotContain(sky, t =>
        {
            var mid = (t.A + t.B + t.C) / 3f;
            return Vector3.Distance(new Vector3(mid.X, mid.Y, 0f), inventedOrigin) < 200f
                   && mid.Z > 1000f;
        });
    }

    private static bool NearXY(MeshTriangle t, float x, float y, float radius)
    {
        var mx = (t.A.X + t.B.X + t.C.X) / 3f - x;
        var my = (t.A.Y + t.B.Y + t.C.Y) / 3f - y;
        return mx * mx + my * my < radius * radius;
    }

    private static int CountPropNear(WorldGeometry world, float x, float y, float radius)
    {
        var r2 = radius * radius;
        return world.Triangles.Count(t =>
        {
            if (t.Layer != Fable.Formats.Meshes.SceneLayer.Prop)
                return false;
            var mx = (t.A.X + t.B.X + t.C.X) / 3f - x;
            var my = (t.A.Y + t.B.Y + t.C.Y) / 3f - y;
            return mx * mx + my * my < r2;
        });
    }

    private static float DistXy(Vector3 a, Vector3 b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    private static int CountInView(WorldGeometry world, ScriptedCamera camera, int textureId)
    {
        var vp = camera.ViewProjection(4f / 3f);
        var n = 0;
        foreach (var t in world.Triangles)
        {
            if (t.Layer != SceneLayer.Prop || t.TextureId != textureId)
                continue;
            if (InView(vp, (t.A + t.B + t.C) / 3f))
                n++;
        }

        return n;
    }

    private static int CountPropInView(WorldGeometry world, ScriptedCamera camera)
    {
        var vp = camera.ViewProjection(4f / 3f);
        return world.Triangles.Count(t =>
            t.Layer == SceneLayer.Prop && InView(vp, (t.A + t.B + t.C) / 3f));
    }

    private static bool InView(Matrix4x4 vp, Vector3 mid)
    {
        var clip = FlyCamera.Project(vp, mid);
        return clip.W > 0f
            && clip.X is >= -1.2f and <= 1.2f
            && clip.Y is >= -1.2f and <= 1.2f;
    }

    [Fact]
    public void Lookout_cells_match_stb_tiles()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var cells = levels.LoadCells("LookoutPoint");
        Assert.True(cells.Count >= 40, $"cells={cells.Count}");
        Assert.All(cells, c => Assert.True(c.Faces.Count > 0));
        var opened = WorldGeometry.Build(
            install, "LookoutPoint", [],
            adjacentStaticMaps: false, levels: levels,
            onlyMaps: ["LookoutPoint"], expandGeometry: false);
        var visible = opened.CollectVisibleCells(levels);
        Assert.Equal(cells.Count, visible.Count);
        var mesh = MeshBatches.BuildCells(visible);
        Assert.Equal(visible.Sum(c => c.Faces.Count * 3), mesh.Vertices.Length);
        Assert.Equal(visible.Count, mesh.Draws.Length);
        Assert.Contains(mesh.Draws, d => d.PassBit == LandscapeCells.LayerForeground);
        Assert.DoesNotContain(mesh.Draws, d => d.PassBit == LandscapeCells.LayerBackground);
    }

    [Fact]
    public void Lookout_tile_origin_is_region_local()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var map = levels.World.FindMap("LookoutPoint");
        Assert.NotNull(map);
        var height = levels.LoadHeightField("LookoutPoint");
        Assert.NotNull(height);
        var compiled = levels.LoadCompiledLev("LookoutPoint");
        Assert.NotNull(compiled);
        var cells = LevCellGrid.TryParse(compiled);
        Assert.NotNull(cells);
        var tris = height.ToTileTriangles(cells, compiled.Materials);
        Assert.True(tris.Count > 0);
        var minX = tris.Min(t => MathF.Min(t.A.X, MathF.Min(t.B.X, t.C.X)));
        var maxX = tris.Max(t => MathF.Max(t.A.X, MathF.Max(t.B.X, t.C.X)));
        var minY = tris.Min(t => MathF.Min(t.A.Y, MathF.Min(t.B.Y, t.C.Y)));
        var maxY = tris.Max(t => MathF.Max(t.A.Y, MathF.Max(t.B.Y, t.C.Y)));
        Assert.InRange(minX, -2f, 16f);
        Assert.InRange(minY, -2f, 16f);
        Assert.True(maxX < map.MapX, $"maxX={maxX} mapX={map.MapX}");
        Assert.True(maxY < map.MapY, $"maxY={maxY} mapY={map.MapY}");
        Assert.InRange(maxX, 64f, 160f);
        Assert.InRange(maxY, 64f, 160f);
    }

    [Fact]
    public void Instance_world_is_009881f0_not_baked_verts()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var lamp = levels.LoadThings("LookoutPoint").Things
            .First(t => t.DefinitionType == "OBJECT_STREETLAMP_LIT_SINGLE_01");
        var xform = WorldGeometry.ObjectTransform(lamp);
        using var bank = new MeshBank();
        bank.Open(install);
        var mesh = bank.Get(4978);
        Assert.NotNull(mesh);
        var built = MeshBatches.BuildMeshes([(mesh, xform)]);
        Assert.True(built.Draws.Length > 0);
        Assert.True(built.Draws.All(d => d.World == xform));
        Assert.All(built.Draws, d => Assert.Equal(0x20u, d.PassBit));
        var origin = Vector3.Transform(Vector3.Zero, xform);
        Assert.InRange((origin - new Vector3(
            lamp.PositionX!.Value, lamp.PositionY!.Value, lamp.PositionZ ?? 0f)).Length(),
            0f, 0.05f);
        Assert.True(built.Vertices[0].Position.Length() < 500f);
        var plane = new Vector4(0, 0, 1, -3);
        var p = new Vector3(1, 2, 4);
        var lhs = Vector4.Dot(new Vector4(Vector3.Transform(p, xform), 1f), plane);
        var rhs = Vector4.Dot(new Vector4(p, 1f), MeshPushConstants.TransformPlane(xform, plane));
        Assert.InRange(MathF.Abs(lhs - rhs), 0f, 1e-3f);
        var shifted = xform * Matrix4x4.CreateTranslation(8f, 0f, 0f);
        var twice = MeshBatches.BuildMeshes([(mesh, xform), (mesh, shifted)]);
        Assert.Equal(built.Vertices.Length, twice.Vertices.Length);
        Assert.True(twice.Draws.Length >= built.Draws.Length * 2);
        Assert.Equal(twice.Draws[0].FirstVertex, twice.Draws[built.Draws.Length].FirstVertex);
        Assert.Equal(xform, twice.Draws[0].World);
        Assert.Equal(shifted, twice.Draws[built.Draws.Length].World);
    }

    [Fact]
    public void Palskin_submit_uses_file_triangles_not_repose()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var bank = new MeshBank();
        bank.Open(install);
        var mesh = bank.Get(4299);
        Assert.NotNull(mesh);
        Assert.True(mesh.BoneCount > 0);
        var built = MeshBatches.BuildMeshes([(mesh, Matrix4x4.Identity)]);
        Assert.Equal(mesh.Triangles.Count * 3, built.Vertices.Length);
        Assert.All(built.Draws, d => Assert.Equal(0x100u, d.PassBit));
    }

    [Fact]
    public void MeshBank_does_not_reparse_c3d()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var bank = new MeshBank();
        bank.Open(install);
        var first = bank.Get(4299);
        var parsed = bank.ParsedCount;
        var second = bank.Get(4299);
        Assert.Same(first, second);
        Assert.Equal(parsed, bank.ParsedCount);
    }
}
