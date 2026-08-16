using System.Text;
using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Defs;
using Fable.Formats.IO;
using Fable.Formats.Meshes;
using Fable.Game;

namespace Fable.Formats.Tests;

/// <summary>
/// Living notes for compiled game.bin. The file is a 13-byte header, name-refs
/// into names.bin (string offsets, not hashes), then zlib-1 chunks of
/// control-byte defs. OBJECT.Graphic.bank_index is the graphics.big mesh id.
/// </summary>
public sealed class GameBinFormatTests
{
    private static (GameInstall Install, NamesBin Names, GameBin Bin) Load()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
        var bin = GameBin.Load(install.FindCompiledDef("game.bin")!, names);
        return (install, names, bin);
    }

    [Fact]
    public void Header_is_13_bytes_with_14761_entries_and_zlib_chunks()
    {
        var (_, _, bin) = Load();
        Assert.False(bin.UseNamesBin);
        Assert.Equal(0xA6C69C21u, bin.FileIndicator);
        Assert.Equal(0xA8E36C34u, bin.PlatformIndicator);
        Assert.Equal(14761, bin.Entries.Count);
        Assert.True(bin.Chunks.Count > 100);
        Assert.All(bin.Chunks, chunk => Assert.True(chunk.InflatedSize > chunk.CompressedSize / 2));
    }

    [Fact]
    public void Name_refs_are_names_bin_string_offsets_not_hashes()
    {
        var (_, names, bin) = Load();
        var wall = names.Find("OBJECT_WALL_SMALL_POST_01");
        Assert.NotNull(wall);
        Assert.Equal(wall.Value.Hash, FableCrc.Hash("OBJECT_WALL_SMALL_POST_01"));
        Assert.Contains(bin.NameRefs, r => r.FileOffset == wall.Value.Offset);
        Assert.DoesNotContain(bin.NameRefs, r => r.FileOffset == wall.Value.Hash);
    }

    [Fact]
    public void Object_graphic_bank_index_is_the_mesh_id()
    {
        var (install, _, bin) = Load();
        Assert.Equal(5331, bin.FindMeshId("OBJECT_WALL_SMALL_POST_01"));
        Assert.Equal(7828, bin.FindMeshId("OBJECT_BRIGHTWOOD_MEDIUMROCK_01"));
        Assert.Equal(4978, bin.FindMeshId("OBJECT_STREETLAMP_LIT_SINGLE_01"));
        Assert.Equal(7168, bin.FindMeshId("OBJECT_OK_PILLAR_COLLAPSED_01"));
        Assert.Equal(3977, bin.FindMeshId("OBJECT_DEGRADABLE_THORN_VINES_01"));
        Assert.Equal(5149, bin.FindMeshId("CREATURE_BS_VILLAGER_MALE"));
        Assert.Equal(4299, bin.FindMeshId("CREATURE_HERO"));
        Assert.Equal(4299, bin.FindMeshId("CREATURE_HERO_TRAINING"));
        Assert.Equal(4300, bin.FindMeshId("CREATURE_HERO_CHILD"));
        Assert.Equal(4300, bin.FindMeshId("CREATURE_YOUNG_HERO"));
        Assert.Equal(6909, bin.FindMeshId("BUILDING_OAKVALE_HOUSE_MEDIUM_SINGLE_FLOOR_BUYABLE"));
        Assert.Equal(6556, bin.FindMeshId("OBJECT_KHG_BED_03"));
        Assert.Equal(7583, bin.FindMeshId("OBJECT_TABLE_LARGE_ROUND_01"));
        Assert.Equal(7544, bin.FindMeshId("OBJECT_WOODEN_LAMP_OFF"));
        Assert.Equal(4901, bin.FindMeshId("OBJECT_BS_RUG_ROUND_DIAMONDS_01"));

        var path = Path.Combine(install.DataRoot, "graphics", "graphics.big");
        using var big = BigArchive.Open(path);
        var bank = big.SubBanks.First(item => item.Name.Contains("MESH", StringComparison.OrdinalIgnoreCase));
        var entries = big.ReadEntries(bank);
        var wall = MeshFile.Parse(big.Read(entries.First(e => e.Id == 5331)), 1);
        Assert.Equal("MESH_SMALL_WALL_CURVED_POST_01", wall.Name);
        var rock = MeshFile.Parse(big.Read(entries.First(e => e.Id == 7828)), 1);
        Assert.Equal("MESH_MEDIUMROCK_LICHEN_01", rock.Name);
        var lamp = MeshFile.Parse(big.Read(entries.First(e => e.Id == 4978)), 1);
        Assert.Equal("MESH_OBJECT_STREETLAMP_OFF_02", lamp.Name);
        var hero = MeshFile.Parse(big.Read(entries.First(e => e.Id == 4299)), 1);
        Assert.Equal("MESH_HERO", hero.Name);
        var kid = MeshFile.Parse(big.Read(entries.First(e => e.Id == 4300)), 1);
        Assert.Equal("MESH_YOUNGHERO_02", kid.Name);
    }

    [Fact]
    public void Markers_and_cameras_do_not_resolve_to_editor_meshes()
    {
        var (_, _, bin) = Load();
        Assert.Null(bin.FindMeshId("MARKER_BASIC"));
        Assert.Null(bin.FindMeshId("CAMERA_POINT_SCRIPTED"));
        Assert.Null(bin.FindMeshId("CAMERA_POINT_SCRIPTED_SPLINE"));
        var marker = bin.FindEntry("MARKER_BASIC");
        Assert.NotNull(marker);
        Assert.Equal("MARKER", marker.TypeName);
        Assert.Contains(marker.SubDefs, sub => bin.Entries[sub.DefIndex].TypeName == "CAppearanceDef");
    }

    [Fact]
    public void Game_bin_has_no_ascii_object_names()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var text = Encoding.ASCII.GetString(File.ReadAllBytes(install.FindCompiledDef("game.bin")!));
        Assert.DoesNotContain("OBJECT_WALL_SMALL_POST_01", text);
        Assert.DoesNotContain("#definition", text);
    }

    [Fact]
    public void Framed_lzo_at_start_is_not_the_def_table()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bytes = File.ReadAllBytes(install.FindCompiledDef("game.bin")!);
        var cursor = 0;
        var decoded = Lzo.DecompressFramed(bytes, ref cursor, 2_000_000);
        var ascii = decoded.Count(value => value is >= 32 and <= 126);
        Assert.True(ascii < 1000, $"unexpectedly dense ASCII in framed LZO decode ascii={ascii}");
    }

    [Fact]
    public void Lookout_instances_most_placeable_objects()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var things = levels.LoadThings("LookoutPoint");
        var world = WorldGeometry.Build(install, "LookoutPoint", things.Things, adjacentStaticMaps: false);
        Assert.True(world.MeshInstances > 150, $"instances={world.MeshInstances} missing={world.MissingMeshes}");
        Assert.True(world.MissingMeshes < 120, $"missing={world.MissingMeshes} instances={world.MeshInstances}");
    }
}
