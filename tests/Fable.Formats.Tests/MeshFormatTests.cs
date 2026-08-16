using System.Numerics;
using Fable.Core;
using Fable.Formats;
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

    [Fact]
    public void Heros_old_house_c3d_emits_walls_and_floors()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var path = Path.Combine(install.DataRoot, "graphics", "graphics.big");
        using var big = BigArchive.Open(path);
        var bank = big.SubBanks.First(item => item.Name.Contains("MESH", StringComparison.OrdinalIgnoreCase));
        var entry = big.ReadEntries(bank).First(item => item.Id == 6909);
        var mesh = MeshFile.Parse(big.Read(entry), (int)entry.Type);
        Assert.Contains(mesh.Materials, m => m.Name.Contains("Wall", StringComparison.OrdinalIgnoreCase) && m.DiffuseMapId == 345);
        Assert.Contains(mesh.Materials, m => m.DiffuseMapId == 3184);
        Assert.Equal(2, mesh.PrimitiveCount);
        Assert.True(mesh.Triangles.Count > 200, $"house tris={mesh.Triangles.Count}");
        Assert.Contains(mesh.Triangles, t => t.TextureId == 345);
        Assert.Contains(mesh.Triangles, t => t.TextureId == 3180);
        Assert.DoesNotContain(mesh.Triangles, t => t.TextureId == 3184);
        Assert.DoesNotContain(mesh.Triangles, t => t.TextureId == 3182);
        Assert.Equal(2, mesh.PrimitiveReports.Count);
        Assert.True(mesh.DeclaredTriangles >= mesh.Triangles.Count);
        var degenerate = mesh.Materials.Single(m => m.Name == "DegenerateTriangles");
        Assert.Equal(0, degenerate.DiffuseMapId);
        Assert.Equal(1, degenerate.Flag3);
        Assert.Equal(0, degenerate.Flag0);
        Assert.Equal(0, degenerate.Flag1);
        Assert.Equal(0, degenerate.Flag2);
        Assert.Contains(mesh.Materials, m => m.Name.Contains("Wall", StringComparison.OrdinalIgnoreCase) && m.Flag3 == 0);
        var walls3180 = mesh.Materials.Single(m => m.DiffuseMapId == 3180);
        Assert.Equal(1, walls3180.Flag1);
    }

    [Fact]
    public void Kid_c3d_stores_hair_flag1_and_bones()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var path = Path.Combine(install.DataRoot, "graphics", "graphics.big");
        using var big = BigArchive.Open(path);
        var bank = big.SubBanks.First(item => item.Name.Contains("MESH", StringComparison.OrdinalIgnoreCase));
        var entry = big.ReadEntries(bank).First(item => item.Id == 4300);
        var mesh = MeshFile.Parse(big.Read(entry), (int)entry.Type);
        Assert.Equal("MESH_YOUNGHERO_02", mesh.Name);
        Assert.Equal(76, mesh.BoneCount);
        Assert.Equal(76, mesh.Bones.Count);
        Assert.Equal(64, MeshFile.BoneMatrixBytes);
        Assert.Equal(60, MeshFile.BoneInfoBytes);
        Assert.Equal(48, MeshFile.BoneLocalBytes);
        Assert.Equal(WorldShading.BoneRecordBytes, MeshFile.BoneMatrixBytes);
        Assert.Equal(WorldShading.BoneFloat4sPerInfluence * 16, 48);
        var root = mesh.Bones[0];
        Assert.Equal("Scene Root", root.Name);
        Assert.Equal(-1, root.Parent);
        Assert.Equal(1u, root.Flags);
        Assert.Equal(Matrix4x4.Identity, root.Matrix);
        Assert.Equal(new Vector4(1f, 0f, 0f, 0f), root.UploadRow0);
        Assert.Equal(new Vector4(0f, 1f, 0f, 0f), root.UploadRow1);
        Assert.Equal(new Vector4(0f, 0f, 1f, 0f), root.UploadRow2);
        Assert.Equal(76 * 3, WorldShading.BoneConstantCount(mesh.BoneCount));
        var bip = mesh.Bones[3];
        Assert.Equal("Bip01", bip.Name);
        Assert.Equal(2, bip.Parent);
        Assert.Equal(0f, bip.Matrix.M41);
        Assert.Equal(0f, bip.Matrix.M42);
        Assert.Equal(0f, bip.Matrix.M43);
        Assert.Equal(1f, bip.Matrix.M44);
        Assert.Equal(6.10849f, bip.Matrix.M14, 4);
        Assert.Equal(0.28064f, bip.Matrix.M24, 4);
        Assert.Equal(-103.51969f, bip.Matrix.M34, 4);
        var palettes = WorldShading.FirstSeenPalettes(mesh.Bones);
        Assert.Equal(76, palettes.Length);
        AssertNearIdentity(palettes[0]);
        AssertNearIdentity(palettes[3]);
        AssertNearIdentity(palettes[20]);
        var raw = new Vector3(-11.42f, 11.39f, 180.37f);
        Assert.Equal(raw, WorldShading.SkinPosition(
            raw, new byte[] { 0, 0, 0, 0 }, new byte[] { 255, 0, 0, 0 }, palettes));
        Assert.Equal("VSHADER_PALSKIN_DIRLIGHT_FOG",
            WorldShading.PalskinFamilyShader(WorldShading.FirstSeenPackedLightCount));
        var hair = mesh.Materials.Single(m => m.Name.Contains("Hair", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(1, hair.Flag1);
        Assert.Equal(0, hair.Flag3);
        Assert.DoesNotContain(mesh.Materials, m => m.Name == "DegenerateTriangles");
    }

    private static void AssertNearIdentity(Matrix4x4 m)
    {
        Assert.Equal(1f, m.M11, 3);
        Assert.Equal(1f, m.M22, 3);
        Assert.Equal(1f, m.M33, 3);
        Assert.Equal(1f, m.M44, 3);
        Assert.Equal(0f, m.M12, 3);
        Assert.Equal(0f, m.M13, 3);
        Assert.Equal(0f, m.M14, 3);
        Assert.Equal(0f, m.M21, 3);
        Assert.Equal(0f, m.M24, 3);
        Assert.Equal(0f, m.M31, 3);
        Assert.Equal(0f, m.M34, 3);
    }
}
