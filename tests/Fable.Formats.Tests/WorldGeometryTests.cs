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
    }
}
