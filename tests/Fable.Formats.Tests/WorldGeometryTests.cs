using System.Numerics;
using Fable.Core;
using Fable.Formats.Meshes;
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
}
