using System.Text;
using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Bones;
using Fable.Formats.Defs;
using Fable.Formats.Levels;
using Fable.Formats.Text;
using Fable.Formats.Tng;
using Fable.Formats.Wld;

namespace Fable.Formats.Tests;

/// <summary>
/// Living notes for the rest of TLC data\: compiled defs besides game.bin,
/// the region index, bone morphs, text, and the remaining BIGB / Lionhead banks.
/// </summary>
public sealed class DataCatalogTests
{
    private static GameInstall Require()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        return install;
    }

    [Fact]
    public void Frontend_and_script_bins_are_gamebin()
    {
        var install = Require();
        var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
        var frontend = GameBin.Load(Path.Combine(install.DataRoot, "CompiledDefs", "frontend.bin"), names);
        var script = GameBin.Load(Path.Combine(install.DataRoot, "CompiledDefs", "script.bin"), names);

        Assert.False(frontend.UseNamesBin);
        Assert.Equal(0xA8E36C34u, frontend.PlatformIndicator);
        Assert.Equal(810, frontend.Entries.Count);
        Assert.True(frontend.Entries.Count(e => e.TypeName == "UI") > 700);

        Assert.Equal(0xA8E36C34u, script.PlatformIndicator);
        Assert.Equal(611, script.Entries.Count);
        Assert.True(script.Entries.Count(e => e.TypeName == "CCutsceneDef") > 500);
        Assert.Contains(script.Entries, e => e.InstanceName == "CS_ATTRACT_1");
    }

    [Fact]
    public void Bwd_lists_every_wld_map_with_world_aabb()
    {
        var install = Require();
        var world = WorldFile.Load(install.WorldPath);
        var bwd = BwdFile.Load(install.BwdPath);
        Assert.Equal(world.Maps.Count, bwd.Regions.Count);
        Assert.True(bwd.Regions.Count >= 70);

        var lookout = bwd.Find("LookoutPoint");
        Assert.NotNull(lookout);
        Assert.Equal(3232, lookout.Value.MinX);
        Assert.Equal(3488, lookout.Value.MinY);
        Assert.Equal(3360, lookout.Value.MaxX);
        Assert.Equal(3616, lookout.Value.MaxY);
        Assert.EndsWith("LookoutPoint.lev", lookout.Value.LevPath, StringComparison.OrdinalIgnoreCase);

        var picnic = bwd.Find("PicnicArea");
        Assert.NotNull(picnic);
        Assert.Equal(3104, picnic.Value.MinX);
        Assert.Equal(3520, picnic.Value.MinY);
        Assert.Equal(world.FindMap("PicnicArea")!.MapX, picnic.Value.MinX);
    }

    [Fact]
    public void Gtg_is_version_2_thing_text_not_a_lev_mesh()
    {
        var install = Require();
        var text = File.ReadAllText(install.GtgPath);
        Assert.StartsWith("NEWMAP", text, StringComparison.Ordinal);
        var things = ThingFile.Parse(text);
        Assert.Equal(2, things.Version);
        Assert.True(things.Things.Count() > 100);
        Assert.Contains(things.Things, t => t.DefinitionType == "REGION_ENTRANCE_POINT");
        Assert.Contains(things.Things, t => t.DefinitionType == "HOLY_SITE_PLAYER_START");
        Assert.NotEqual(25, BitConverter.ToInt32(File.ReadAllBytes(install.GtgPath), 0));
    }

    [Fact]
    public void Bncfg_scales_hero_and_villager_bones()
    {
        var install = Require();
        var hero = BoneConfig.Load(Path.Combine(install.BonesDirectory, "hero_weak.bncfg"));
        Assert.Equal("CREATURE_HERO", hero.CreatureType);
        Assert.True(hero.Bones.Count >= 10);
        Assert.Contains(hero.Bones, b => b.Name == "Bip01 Head" && b.X is > 0.5f and < 1.5f);
        Assert.True(hero.Groups.ContainsKey("thigh"));
        Assert.Contains(hero.Groups["thigh"], n => n.Contains("Thigh", StringComparison.Ordinal));

        var villager = BoneConfig.Load(Path.Combine(install.BonesDirectory, "bs_male_weak.bncfg"));
        Assert.Equal("CREATURE_BS_VILLAGER_MALE", villager.CreatureType);
        Assert.True(villager.Bones.Count >= 8);
        Assert.Equal(60, Directory.GetFiles(install.BonesDirectory, "*.bncfg").Length);

        var teen = BoneConfig.Load(Path.Combine(install.BonesDirectory, "hero_teen_set.bncfg"));
        Assert.Equal("CREATURE_HERO", teen.CreatureType);
        var young = BoneConfig.Load(Path.Combine(install.BonesDirectory, "hero_young_set.bncfg"));
        Assert.Equal("CREATURE_HERO_CHILD_02", young.CreatureType);
    }

    [Fact]
    public void Text_big_is_utf16_english_strings()
    {
        var install = Require();
        using var big = BigArchive.Open(install.TextBigPath);
        Assert.Equal("TEXT_ENGLISH_MAIN", big.SubBanks[0].Name);
        var entries = big.ReadEntries(big.SubBanks[0]);
        Assert.True(entries.Count > 10_000);
        var first = entries.First(e => e.Id == 1);
        Assert.Equal("TEXT_QST_028_ONSCREENHELP_FLOURISH_BASIC", first.Name);
        var text = TextPayload.ReadUtf16(big.Read(first));
        Assert.StartsWith("If you get three hits", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Remaining_big_banks_are_bigb()
    {
        var install = Require();
        foreach (var (rel, bank) in new[]
                 {
                     (@"lang\English\fonts.big", "FONT_ENGLISH_MAIN"),
                     (@"lang\English\dialogue.big", "LIPSYNC_ENGLISH_MAIN"),
                     (@"graphics\pc\frontend.big", "GBANK_FRONT_END_PC"),
                     (@"Misc\pc\effects.big", "PARTICLE_MAIN_PC"),
                     (@"shaders\pc\shaders.big", "PIXEL_SHADERS"),
                 })
        {
            var path = Path.Combine(install.DataRoot, rel);
            using var big = BigArchive.Open(path);
            Assert.Contains(big.SubBanks, b => b.Name == bank);
        }
    }

    [Fact]
    public void Sound_lug_and_dialogue_lut_are_lionhead_audio_not_bigb()
    {
        var install = Require();
        var lug = File.ReadAllBytes(Path.Combine(install.DataRoot, "Sound", "PicnicArea.lug"));
        var met = File.ReadAllBytes(Path.Combine(install.DataRoot, "Sound", "PicnicArea.met"));
        var lut = File.ReadAllBytes(Path.Combine(install.DataRoot, "lang", "English", "Dialogue.lut"));
        Assert.Equal("LiOnHeAd", Encoding.ASCII.GetString(lug, 0, 8));
        Assert.Equal("LHFileSegmentBankInfo", Encoding.ASCII.GetString(lug, 8, 21).TrimEnd('\0'));
        Assert.Equal(1, BitConverter.ToInt32(met, 0));
        Assert.Contains("Picnic", Encoding.ASCII.GetString(met), StringComparison.OrdinalIgnoreCase);
        Assert.Equal("LiOnHeAd", Encoding.ASCII.GetString(lut, 0, 8));
        Assert.Equal("LHAudioBankCompData", Encoding.ASCII.GetString(lut, 8, 19).TrimEnd('\0'));
        Assert.NotEqual(BigArchive.Magic, BitConverter.ToUInt32(lug, 0));
        Assert.NotEqual(BigArchive.Magic, BitConverter.ToUInt32(lut, 0));
    }

    [Fact]
    public void Stars_dat_is_count_then_24_byte_records()
    {
        var install = Require();
        var bytes = File.ReadAllBytes(Path.Combine(install.DataRoot, "Misc", "stars.dat"));
        var count = BitConverter.ToInt32(bytes, 0);
        Assert.Equal(1330, count);
        Assert.Equal(4 + count * 24, bytes.Length);
        Assert.False(float.IsNaN(BitConverter.ToSingle(bytes, 4)));

        var stars = Fable.Formats.Sky.StarField.Parse(bytes);
        Assert.Equal(1330, stars.Stars.Count);
        Assert.True(stars.Stars.All(s => s.Position.Length() > 100));
        Assert.True(stars.Stars.All(s => s.Size is >= 0 and <= 120));
        Assert.Equal(0f, BitConverter.ToSingle(bytes, 16), 3);
    }

    [Fact]
    public void Sky_def_names_sun_star_and_flare_textures()
    {
        var install = Require();
        var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
        var bin = GameBin.Load(install.FindCompiledDef("game.bin")!, names);
        var sky = Fable.Formats.Sky.SkyDef.TryLoadFromGameBin(bin);
        Assert.NotNull(sky);
        Assert.Equal(384, sky.SunTextureId);
        Assert.Equal(401, sky.StarTextureId);
        Assert.Contains(sky.Flares, f => f.TextureId == 393);
        Assert.True(sky.MaxRadius >= 6000f, $"maxRadius={sky.MaxRadius}");

        using var tex = BigArchive.Open(Path.Combine(install.DataRoot, "graphics", "pc", "textures.big"));
        var entries = tex.ReadEntries(tex.SubBanks.First(b => b.Name.Contains("MAIN")));
        Assert.Equal("GRAPHIC_ATMOSPHERIC_SUN", entries.First(e => e.Id == 384).Name);
        Assert.Equal("GRAPHIC_ATMOSPHERIC_STAR_01", entries.First(e => e.Id == 401).Name);
        Assert.Equal("GRAPHIC_ATMOSPHERIC_SKY_MIDDAY", entries.First(e => e.Id == 391).Name);
    }

    [Fact]
    public void Bwd_count_field_is_not_one_more_full_region()
    {
        var install = Require();
        var raw = File.ReadAllBytes(install.BwdPath);
        var declared = BitConverter.ToInt32(raw, 0);
        var parsed = BwdFile.Parse(raw).Regions.Count;
        Assert.Equal(398, parsed);
        Assert.Equal(399, declared);
        Assert.True(raw.Length > 40_000);
    }
}
