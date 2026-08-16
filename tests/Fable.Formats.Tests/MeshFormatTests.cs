using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Defs;
using Fable.Formats.Meshes;
using Fable.Formats.Textures;
using Fable.Game;

namespace Fable.Formats.Tests;

/// <summary>
/// Living notes for C3D mesh materials and packed UVs. Facts are asserted
/// against the TLC apple tree and textures.big.
/// </summary>
public sealed class MeshFormatTests
{
    private static (GameInstall Install, MeshFile Mesh) LoadAppleTree()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var path = Path.Combine(install.DataRoot, "graphics", "graphics.big");
        using var big = BigArchive.Open(path);
        var bank = big.SubBanks.First(item => item.Name.Contains("MESH", StringComparison.OrdinalIgnoreCase));
        var entry = big.ReadEntries(bank).First(item => item.Id == 5228);
        return (install, MeshFile.Parse(big.Read(entry), (int)entry.Type));
    }

    [Fact]
    public void Apple_tree_materials_carry_textures_big_diffuse_ids()
    {
        var mesh = LoadAppleTree().Mesh;
        Assert.Contains(mesh.Materials, m => m.Name == "trunk" && m.DiffuseMapId == 3880);
        Assert.Contains(mesh.Materials, m => m.Name == "apple" && m.DiffuseMapId == 2132);
        Assert.Contains(mesh.Materials, m => m.Name == "leaves" && m.DiffuseMapId == 2119);
        Assert.Contains(mesh.Materials, m => m.Name == "branch" && m.DiffuseMapId == 2118);
    }

    [Fact]
    public void Apple_tree_uvs_are_mostly_in_unit_range_with_some_tiling()
    {
        var mesh = LoadAppleTree().Mesh;
        Assert.True(mesh.Triangles.Count > 100);
        Assert.Contains(mesh.Triangles, tri => tri.TextureId == 3880);
        Assert.Contains(mesh.Triangles, tri => tri.TextureId == 2119);

        var us = mesh.Triangles.SelectMany(tri => new[] { tri.UvA.X, tri.UvB.X, tri.UvC.X }).ToList();
        Assert.InRange(us.Average(), -1f, 2f);
        Assert.True(us.Max() > 0.5f);
        Assert.True(us.Min() < 0.5f);
    }

    [Fact]
    public void Packed_stride_12_puts_uv_at_byte_8()
    {
        Assert.Equal(8, MeshFile.PackedUvOffset(entryType: 1, stride: 12, initFlags: 4, hasBones: false));
        Assert.Equal(0.0f, MeshFile.DecompressUv(16384));
        Assert.Equal(-8f, MeshFile.DecompressUv(0));
    }

    [Fact]
    public void Diffuse_3880_is_oak_trunk_in_textures_big()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var textures = new TextureLibrary(install);
        var trunk = textures.TryLoad(3880);
        Assert.NotNull(trunk);
        Assert.Contains("TRUNK", trunk.Name, StringComparison.OrdinalIgnoreCase);
        Assert.True(trunk.Width >= 32);
        Assert.Equal(trunk.Width * trunk.Height * 4, trunk.Rgba.Length);

        var sample = textures.Sample(3880, new System.Numerics.Vector2(0.5f, 0.5f));
        Assert.InRange(sample.X + sample.Y + sample.Z, 0.05f, 2.9f);
    }

    [Fact]
    public void Grass_plain_enum_id_matches_textures_big()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var enums = HeaderEnums.Load(Path.Combine(install.DataRoot, "Defs", "RetailHeaders", "pc", "textures.h"));
        Assert.Equal(TextureLibrary.LandscapeGrassPlainId, enums.ByName["LANDSCAPE_GRASS_PLAIN"]);

        var path = Path.Combine(install.DataRoot, "graphics", "pc", "textures.big");
        using var big = BigArchive.Open(path);
        var bank = big.SubBanks.Single(item => item.Name == "GBANK_MAIN_PC");
        var grass = big.ReadEntries(bank).First(item => item.Id == TextureLibrary.LandscapeGrassPlainId);
        Assert.Equal("LANDSCAPE_GRASS_PLAIN", grass.Name);
        var header = TextureFile.ReadHeader(grass.Info.ToArray());
        Assert.Equal(512, header.Width);
    }

    [Fact]
    public void Object_prefix_maps_bigrock_to_mesh_enum()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var enums = HeaderEnums.Load(Path.Combine(install.DataRoot, "Defs", "RetailHeaders", "meshdata.h"));
        Assert.Equal(7802, enums.FindMeshId("OBJECT_BIGROCK_01"));
        Assert.Equal(5363, enums.FindMeshId("CREATURE_BEGGAR_01"));
        Assert.Null(enums.FindMeshId("OBJECT_WALL_SMALL_POST_01"));
        Assert.Null(enums.FindMeshId("MARKER_BASIC"));
    }
}
