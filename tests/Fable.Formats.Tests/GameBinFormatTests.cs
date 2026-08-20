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
        Assert.Equal(GameBin.FirstSeenEnvironmentThemeId,
            bin.FindEnvironmentThemeId(GameBin.LookoutRegionDefName));
        Assert.Equal(GameBin.FirstSeenEnvironmentThemeName,
            bin.FindEnvironmentThemeName(GameBin.LookoutRegionDefName));
        Assert.Equal(GameBin.FirstSeenEnvironmentThemeName,
            bin.FindEnvironmentThemeName("REGION_OAK_VALE_INTRO"));
        Assert.NotEqual(GameBin.OakvaleEnvironmentName,
            bin.FindEnvironmentThemeName(GameBin.LookoutRegionDefName));
        Assert.Equal(FableCrc.Hash(GameBin.EnvironmentThemeField),
            FableCrc.Hash("EnvironmentTheme"));
        Assert.Equal(269, GameBin.EnvironmentThemeRecordBytes);
        Assert.Equal(112, GameBin.LightingRecordBytes);
        Assert.Equal(7168, bin.FindMeshId("OBJECT_OK_PILLAR_COLLAPSED_01"));
        Assert.Equal(3977, bin.FindMeshId("OBJECT_DEGRADABLE_THORN_VINES_01"));
        Assert.Equal(5149, bin.FindMeshId("CREATURE_BS_VILLAGER_MALE"));
        Assert.Equal(4299, bin.FindMeshId("CREATURE_HERO"));
        Assert.Equal(4299, bin.FindMeshId("CREATURE_HERO_TRAINING"));
        Assert.Equal(4300, bin.FindMeshId("CREATURE_HERO_CHILD"));
        Assert.Equal(4300, bin.FindMeshId("CREATURE_YOUNG_HERO"));
        Assert.Equal(6909, bin.FindMeshId("BUILDING_OAKVALE_HOUSE_MEDIUM_SINGLE_FLOOR_BUYABLE"));
        var houseIds = bin.FindMeshIds("BUILDING_OAKVALE_HOUSE_MEDIUM_SINGLE_FLOOR_BUYABLE");
        Assert.Contains(GameBin.HerosOldHouseExteriorMeshId, houseIds);
        Assert.Contains(GameBin.HerosOldHouseInteriorMeshId, houseIds);
        Assert.Equal(0x0CDCCB01u, GameBin.MultiStaticMeshesFieldCrc);
        Assert.Equal(FableCrc.Hash("Meshes"), GameBin.MultiStaticMeshesFieldCrc);
        Assert.Equal(FableCrc.Hash("Mesh"), GameBin.MultiStaticMeshFieldCrc);
        Assert.Equal(0x007E1400u, GameBin.MultiStaticLookup);
        Assert.Equal(0x007E15C0u, GameBin.MultiStaticApply);
        Assert.Equal("CTCGraphicAppearanceMultipleStaticMeshes", GameBin.MultiStaticComponentType);
        Assert.Equal(0x007E1A80u, GameBin.MultiStaticComponentName);
        Assert.Equal(0x5C, GameBin.MultiStaticComponentBytes);
        Assert.Equal(0x007E1370u, GameBin.MultiStaticIndex);
        Assert.Equal(0x0126FFB4u, GameBin.MultiStaticApplyVtbl);
        Assert.Equal(56, GameBin.MultiStaticRuntimeStrideBytes);
        Assert.Equal(40, GameBin.MultiStaticRuntimeIdOffset);
        Assert.Equal(0x0052AC10u, GameBin.ThingBuildingFactory);
        Assert.Equal(0x005296B0u, GameBin.ThingBuildingBaseCtor);
        Assert.Equal(0x00522A20u, GameBin.ThingTypeRegistrar);
        Assert.False(GameBin.FirstSeenHouseFloor3184HasPrims);
        Assert.True(GameBin.FirstSeenMultiStaticAppliesBothHouseMeshes);
        Assert.True(GameBin.FirstSeenHouseAreaDefsResolveGraphic);
        Assert.Equal(0x0137B530u, GameBin.MultiStaticEntryRtti);
        Assert.False(GameBin.FirstSeenMultiStaticValueIsScale);
        Assert.Equal(0x004BC180u, GameBin.MultiStaticDefaultFloat);
        Assert.Equal(-1, GameBin.FirstSeenSkipGlobal);
        Assert.False(GameBin.FirstSeenSkipGlobalHasWriter);
        Assert.True(GameBin.FirstSeenMultiStaticPersistMapsFileFields);
        Assert.Equal(0x004EB8C3u, GameBin.MultiStaticEntryPersist);
        Assert.Equal(0x004EDE1Bu, GameBin.MultiStaticVectorPersistSlot);
        Assert.Equal(0x00431102u, GameBin.MultiStaticPersistDword);
        Assert.Equal(0x0043314Au, GameBin.MultiStaticPersistU8);
        Assert.Equal(0x00431061u, GameBin.MultiStaticPersistFloat);
        Assert.Equal(0x004735D6u, GameBin.MultiStaticPersistTail);
        Assert.Equal(44, GameBin.MultiStaticRuntimeFlagAOffset);
        Assert.Equal(45, GameBin.MultiStaticRuntimeFlagBOffset);
        Assert.Equal(48, GameBin.MultiStaticRuntimeOverrideOffset);
        Assert.Equal(52, GameBin.MultiStaticRuntimeSkipByteOffset);
        Assert.False(GameBin.FirstSeenHouseSkipDropsInterior);
        Assert.False(GameBin.FirstSeenHouseSkipDropsExterior);
        var house = bin.FindEntry("BUILDING_OAKVALE_HOUSE_MEDIUM_SINGLE_FLOOR_BUYABLE")!;
        var multi = house.SubDefs
            .Select(sub => bin.Entries[sub.DefIndex])
            .First(child => child.TypeName == GameBin.MultiStaticMeshDefType);
        var houseEntries = GameBin.ReadMultiStaticMeshEntries(multi.Raw);
        Assert.Equal(2, houseEntries.Count);
        Assert.Equal(GameBin.HerosOldHouseInteriorMeshId, houseEntries[0].MeshId);
        Assert.Equal(0, houseEntries[0].FlagA);
        Assert.Equal(1, houseEntries[0].FlagB);
        Assert.Equal(40f, houseEntries[0].Value);
        Assert.Equal(0u, houseEntries[0].Tail);
        Assert.Equal(GameBin.HerosOldHouseExteriorMeshId, houseEntries[1].MeshId);
        Assert.Equal(1, houseEntries[1].FlagA);
        Assert.Equal(0, houseEntries[1].FlagB);
        Assert.Equal(0f, houseEntries[1].Value);
        Assert.Equal(0u, houseEntries[1].Tail);
        Assert.Equal(0x7CA90715u, GameBin.MultiStaticFlagAFieldCrc);
        Assert.Equal(0x97595FC1u, GameBin.MultiStaticFlagBFieldCrc);
        Assert.Equal(0x15DC93E9u, GameBin.MultiStaticValueFieldCrc);
        Assert.False(GameBin.MultiStaticSkipDraw(0, 0, 0, 0));
        Assert.False(GameBin.MultiStaticSkipDraw(1, 0, 1, 0));
        Assert.True(GameBin.MultiStaticSkipDraw(1, 1, 0, 0));
        Assert.True(GameBin.MultiStaticSkipDraw(0, 0, 1, 0));
        Assert.False(GameBin.MultiStaticSkipDraw(1, 1, 1, -1));
        Assert.True(GameBin.FirstSeenThingPlus64IsZero);
        Assert.Equal(0, GameBin.FirstSeenThingPlus64);
        Assert.False(GameBin.FirstSeenMultiStaticSkipDraw(0, 1));
        Assert.False(GameBin.FirstSeenMultiStaticSkipDraw(1, 1));
        Assert.False(GameBin.FirstSeenMultiStaticSkipDraw(houseEntries[1].FlagA, (byte)houseEntries[1].Tail));
        Assert.False(GameBin.FirstSeenMultiStaticSkipDraw(houseEntries[0].FlagA, (byte)houseEntries[0].Tail));
        Assert.False(GameBin.FirstSeenInsideBuildingFlag);
        Assert.False(GameBin.FirstSeenBuyableHouseSwapsWindows);
        Assert.Equal(0x006BF8A0u, GameBin.BuyableHouseCtor);
        Assert.Equal(0x006C14D0u, GameBin.BuyableHouseConstruct);
        Assert.Equal(0x0082E0E0u, GameBin.InsideBuildingPredicate);
        Assert.Equal(0x200000u, GameBin.InsideBuildingFlagBit);
        Assert.Equal(56, GameBin.InsideBuildingFlagOffset);
        var buyable = house.SubDefs
            .Select(sub => bin.Entries[sub.DefIndex])
            .First(child => child.TypeName == GameBin.BuyableHouseDefType);
        var prices = GameBin.ReadBuyableHousePrices(buyable.Raw);
        Assert.Equal(new[] { 5000, 7500, 11000, 16000 }, prices);
        Assert.Equal(GameBin.BuyableHousePriceFieldCrc, FableCrc.Hash("Price"));
        Assert.Equal(6556, bin.FindMeshId("OBJECT_KHG_BED_03"));
        Assert.Equal(7583, bin.FindMeshId("OBJECT_TABLE_LARGE_ROUND_01"));
        Assert.Equal(7544, bin.FindMeshId("OBJECT_WOODEN_LAMP_OFF"));
        Assert.Equal(4901, bin.FindMeshId("OBJECT_BS_RUG_ROUND_DIAMONDS_01"));
        Assert.True(bin.FindMeshId("GENERIC_INTERNAL_FIREPLACE") is > 0,
            "fireplace Graphic missing");
        Assert.True(bin.FindMeshId("OBJECT_BUILDING_DOOR_3") is > 0, "door Graphic missing");
        Assert.True(bin.FindMeshId("OBJECT_CHAIR_01") is > 0, "chair Graphic missing");
        Assert.True(bin.FindMeshId("OBJECT_CUPBOARD_MEDIUM") is > 0, "cupboard Graphic missing");
        Assert.True(bin.FindMeshId("OBJECT_BOOKSHELF_01") is > 0, "bookshelf Graphic missing");
        Assert.True(bin.FindMeshId("OBJECT_HOME_TABLE_3_STOOLS") is > 0, "stool table Graphic missing");
        Assert.True(bin.FindMeshId("OBJECT_KHG_BED_01") is > 0, "second bed Graphic missing");
        Assert.True(bin.FindMeshId("OBJECT_BS_TABLELAMP_UNLIT_01") is > 0, "table lamp Graphic missing");

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
    public void CActivateQuestDef_payloads_are_16_bytes_and_do_not_intern_Q_NewOakValeIntro()
    {
        var (_, names, bin) = Load();
        var rows = bin.Entries
            .Where(entry => entry.TypeName == "CActivateQuestDef")
            .ToList();
        Assert.Equal(
            new[] { 61, 9241, 9248, 12277, 12857, 12874 },
            rows.Select(entry => entry.Index));
        Assert.Equal("NULLDEF_CActivateQuestDef", rows[0].InstanceName);
        Assert.Equal("0000001B5AB31FFFFFFFFF784B39BF01", Convert.ToHexString(rows[0].Raw));
        Assert.Equal("0100011B5AB31F8D1F0500784B39BF00", Convert.ToHexString(rows[1].Raw));
        Assert.Equal("0100011B5AB31F8D1F0500784B39BF00", Convert.ToHexString(rows[2].Raw));
        Assert.Equal("0100011B5AB31FEFA10500784B39BF00", Convert.ToHexString(rows[3].Raw));
        Assert.Equal("0100011B5AB31F51A60500784B39BF00", Convert.ToHexString(rows[4].Raw));
        Assert.Equal("0100011B5AB31FD0A60500784B39BF00", Convert.ToHexString(rows[5].Raw));
        const uint oakvaleIntern = 0x012C5D14;
        const uint nameCrc = 0x1FB35A1B;
        const uint flagCrc = 0xBF394B78;
        Assert.Equal(0x8D19C362u, FableCrc.Hash("Q_NewOakValeIntro"));
        Assert.Equal(EngineLifecycle.OakvaleQuestFableCrc, FableCrc.Hash("Q_NewOakValeIntro"));
        Assert.False(EngineLifecycle.CActivateQuestDefInternsOakvale);
        Assert.False(EngineLifecycle.CActivateQuestDefInOakvaleTng);
        foreach (var row in rows)
        {
            Assert.Equal(16, row.Raw.Length);
            Assert.Equal(nameCrc, BitConverter.ToUInt32(row.Raw, 3));
            Assert.Equal(flagCrc, BitConverter.ToUInt32(row.Raw, 11));
            for (var i = 0; i + 4 <= row.Raw.Length; i++)
            {
                Assert.NotEqual(oakvaleIntern, BitConverter.ToUInt32(row.Raw, i));
                Assert.NotEqual(EngineLifecycle.OakvaleQuestFableCrc,
                    BitConverter.ToUInt32(row.Raw, i));
            }
        }

        Assert.Equal(unchecked((uint)-1), BitConverter.ToUInt32(rows[0].Raw, 7));
        Assert.Equal(1, rows[0].Raw[15]);
        Assert.Equal("Global_OpenChest", names.Get(BitConverter.ToUInt32(rows[1].Raw, 7)));
        Assert.Equal("Global_OpenChest", names.Get(BitConverter.ToUInt32(rows[2].Raw, 7)));
        Assert.Equal("Global_GiveHeroItemsFromRewardChest", names.Get(BitConverter.ToUInt32(rows[3].Raw, 7)));
        Assert.Equal("Global_TeleportToHeroGuild", names.Get(BitConverter.ToUInt32(rows[4].Raw, 7)));
        Assert.Equal("Global_ToggleTimeDisplay", names.Get(BitConverter.ToUInt32(rows[5].Raw, 7)));
        Assert.All(rows.Skip(1), row => Assert.Equal(0, row.Raw[15]));
        Assert.NotEqual("Q_NewOakValeIntro", names.Get(BitConverter.ToUInt32(rows[4].Raw, 7)));
        Assert.NotEqual(nameCrc, FableCrc.Hash("QuestName"));
        Assert.NotEqual(flagCrc, FableCrc.Hash("AlwaysActive"));
    }

    [Fact]
    public void Script_bin_payloads_do_not_intern_Q_NewOakValeIntro()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
        var path = install.FindCompiledDef("script.bin");
        Assert.NotNull(path);
        var bin = GameBin.Load(path, names);
        const uint oakvaleIntern = EngineLifecycle.OakvaleQuestIntern;
        foreach (var row in bin.Entries)
        {
            for (var i = 0; i + 4 <= row.Raw.Length; i++)
                Assert.NotEqual(oakvaleIntern, BitConverter.ToUInt32(row.Raw, i));
        }

        Assert.Null(names.Find("Q_NewOakValeIntro"));
    }

    [Fact]
    public void Expression_plus120_persist_is_not_Q_NewOakValeIntro()
    {
        var (_, names, bin) = Load();
        var rows = bin.Entries
            .Where(entry => entry.TypeName == "EXPRESSION")
            .ToList();
        Assert.Equal(39, rows.Count);
        Assert.Equal(0x1FB35A1Bu, EngineLifecycle.ExpressionPlus120Crc);
        Assert.False(EngineLifecycle.ExpressionPlus120IsOakvaleIntern);
        const uint oakvaleIntern = EngineLifecycle.OakvaleQuestIntern;
        foreach (var row in rows)
        {
            for (var i = 0; i + 4 <= row.Raw.Length; i++)
                Assert.NotEqual(oakvaleIntern, BitConverter.ToUInt32(row.Raw, i));
        }

        Assert.Null(names.Find("Q_NewOakValeIntro"));
        foreach (var name in new[]
                 {
                     "Expression_Pickpocket",
                     "Expression_Picklock",
                     "Expression_Steal",
                 })
            Assert.NotNull(names.Find(name));
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
