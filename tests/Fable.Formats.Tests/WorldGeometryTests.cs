using System.Numerics;
using Fable.Core;
using Fable.Formats.Meshes;
using Fable.Formats.Sky;
using Fable.Game;

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
        Assert.Equal(4300, world.PlayerMeshId);
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
}
