using System.Numerics;
using Fable.Core;
using Fable.Formats;
using Fable.Formats.Banks;
using Fable.Formats.Shaders;

namespace Fable.Formats.Tests;

/// <summary>
/// Fable.exe draws the world with CEngineLandscapeRenderer /
/// CEngineLightingManager and the D3D programs in shaders.big.
/// </summary>
public sealed class ShaderFormatTests
{
    private static GameInstall Require()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        return install;
    }

    [Fact]
    public void Shaders_big_is_d3d_vs11_ps11()
    {
        var install = Require();
        using var big = BigArchive.Open(install.ShadersBigPath);
        Assert.Equal(26, big.SubBanks.Count);
        Assert.Equal("PIXEL_SHADERS", big.SubBanks[0].Name);

        var programs = new List<ShaderProgram>();
        foreach (var bank in big.SubBanks)
        {
            foreach (var entry in big.ReadEntries(bank))
                programs.Add(ShaderProgram.Parse(entry.Name, bank.Name, entry.Type, big.Read(entry)));
        }

        Assert.Equal(465, programs.Count);
        Assert.Equal(353, programs.Count(p => p.Profile == "vs_1_1"));
        Assert.Equal(101, programs.Count(p => p.Profile == "ps_1_1"));
        Assert.Equal(11, programs.Count(p => p.Profile == "ps_1_4"));
        Assert.DoesNotContain(programs, p => p.Major != 1);
        Assert.All(programs.Where(p => p.Bank == "PIXEL_SHADERS"), p => Assert.True(p.IsPixel));
        Assert.All(programs.Where(p => p.Bank.StartsWith("SHADERS_", StringComparison.Ordinal)), p => Assert.True(p.IsVertex || p.Name.Contains("DUMMY")));
    }

    [Fact]
    public void World_passes_are_landscape_static_water_and_sky()
    {
        var install = Require();
        using var big = BigArchive.Open(install.ShadersBigPath);
        var names = new HashSet<string>(StringComparer.Ordinal);
        var banks = new HashSet<string>(StringComparer.Ordinal);
        foreach (var bank in big.SubBanks)
        {
            banks.Add(bank.Name);
            foreach (var entry in big.ReadEntries(bank))
                names.Add(entry.Name);
        }

        Assert.Contains("SHADERS_LANDSCAPE_FOREGROUND", banks);
        Assert.Contains("SHADERS_LANDSCAPE_BACKGROUND", banks);
        Assert.Contains("SHADERS_STATIC", banks);
        Assert.Contains("SHADERS_WATER_FOREGROUND", banks);
        Assert.Contains("SHADERS_SEA_BACKGROUND", banks);
        Assert.Contains("SHADERS_SKY", banks);
        Assert.Contains("SHADERS_WEATHER", banks);

        Assert.Contains("VSHADER_LANDSCAPE_FOREGROUND", names);
        Assert.Contains("VSHADER_LANDSCAPE_BACKGROUND", names);
        Assert.Contains("VSHADER_STATIC_DIRLIGHT_FOG", names);
        Assert.Contains("VSHADER_STATIC_UNLIT", names);
        Assert.Contains("VSHADER_WATER_FOREGROUND", names);
        Assert.Contains("VSHADER_INNER_SKY", names);
        Assert.Contains("VSHADER_OUTER_SKY", names);
        Assert.Contains("PSHADER_LANDSCAPE_FOREGROUND", names);
        Assert.Contains("PSHADER_LANDSCAPE_BACKGROUND", names);
        Assert.Contains("PSHADER_LANDSCAPE_PROC_TEXTURE", names);
        Assert.Contains("PSHADER_TEXTURE_DIFFUSE_FOG", names);
        Assert.Contains("PSHADER_INNER_SKY", names);
        Assert.Contains("PSHADER_INNER_SKY_SIMPLE", names);
        Assert.Contains("VSHADER_LANDSCAPE_FOREGROUND_BLACKOUT_PASS", names);
        Assert.Contains("VSHADER_LANDSCAPE_FOREGROUND_5LIGHTS", names);
        Assert.Contains("VSHADER_SKY_STAR_FIELD", names);
        Assert.Contains("VSHADER_SKY_SCREEN_SPACE_SPRITE", names);

        // Exe 00B3B5D0 / 00B3B6D0 register these banks by name.
        string[] exeBanks =
        [
            "PIXEL_SHADERS",
            "SHADERS_STATIC",
            "SHADERS_PALSKIN",
            "SHADERS_STATIC_BUMP",
            "SHADERS_PALSKIN_BUMP",
            "SHADERS_SKY",
            "SHADERS_SKY_SCREEN_SPACE",
            "SHADERS_WATER_FOREGROUND",
            "SHADERS_WATER_BACKGROUND",
            "SHADERS_SEA_BACKGROUND",
            "SHADERS_WEATHER",
            "SHADERS_LANDSCAPE_BACKGROUND",
            "SHADERS_LANDSCAPE_FOREGROUND",
            "SHADERS_POS_COL_TEX1",
            "SHADERS_REPEATED_MESH",
            "SHADERS_POINT_SPRITE1",
            "SHADERS_ZSPRITE",
            "SHADERS_VERTEX_POS",
            "SHADER_SPRITE_GROUP",
            "SHADERS_DECAL_GROUP",
            "SHADERS_MESH_GROUP",
            "SHADERS_PARTICLE_SPRITE_TRAIL",
            "SHADERS_DEBUGGING",
            "SHADERS_TEXT",
        ];
        foreach (var bank in exeBanks)
            Assert.Contains(bank, banks);
    }

    [Fact]
    public void Landscape_foreground_ps_samples_two_textures()
    {
        var install = Require();
        using var big = BigArchive.Open(install.ShadersBigPath);
        var pixel = big.SubBanks.First(b => b.Name == "PIXEL_SHADERS");
        var byName = big.ReadEntries(pixel).ToDictionary(e => e.Name, StringComparer.Ordinal);

        var fg = ShaderProgram.Parse("PSHADER_LANDSCAPE_FOREGROUND", pixel.Name, 1, big.Read(byName["PSHADER_LANDSCAPE_FOREGROUND"]));
        var bg = ShaderProgram.Parse("PSHADER_LANDSCAPE_BACKGROUND", pixel.Name, 1, big.Read(byName["PSHADER_LANDSCAPE_BACKGROUND"]));
        var proc = ShaderProgram.Parse("PSHADER_LANDSCAPE_PROC_TEXTURE", pixel.Name, 1, big.Read(byName["PSHADER_LANDSCAPE_PROC_TEXTURE"]));
        var obj = ShaderProgram.Parse("PSHADER_TEXTURE_DIFFUSE_FOG", pixel.Name, 1, big.Read(byName["PSHADER_TEXTURE_DIFFUSE_FOG"]));
        var unlit = ShaderProgram.Parse("PSHADER_DIFFUSE_ONLY", pixel.Name, 1, big.Read(byName["PSHADER_DIFFUSE_ONLY"]));

        Assert.Equal("ps_1_1", fg.Profile);
        Assert.Equal(2, fg.TexCount);
        Assert.Equal(1, bg.TexCount);
        Assert.Equal(2, proc.TexCount);
        Assert.Equal(1, obj.TexCount);
        Assert.Equal(0, unlit.TexCount);
        Assert.True(fg.HasMulX2, "landscape FG is mul_x2_sat t1 * v0, not a lerp");
        Assert.False(obj.HasMulX2, "PSHADER_TEXTURE_DIFFUSE_FOG is mul, not _x2");

        var landscapeVs = big.SubBanks.First(b => b.Name == "SHADERS_LANDSCAPE_FOREGROUND");
        Assert.Equal(33, big.ReadEntries(landscapeVs).Count);
        var staticVs = big.SubBanks.First(b => b.Name == "SHADERS_STATIC");
        Assert.Equal(105, big.ReadEntries(staticVs).Count);
    }

    [Fact]
    public void Pixel_bank_entries_are_type_1_vertex_banks_are_type_0()
    {
        var install = Require();
        using var big = BigArchive.Open(install.ShadersBigPath);
        var pixel = big.ReadEntries(big.SubBanks.First(b => b.Name == "PIXEL_SHADERS"));
        Assert.Equal(112, pixel.Count);
        Assert.All(pixel, e => Assert.Equal(1u, e.Type));

        var landscape = big.ReadEntries(big.SubBanks.First(b => b.Name == "SHADERS_LANDSCAPE_FOREGROUND"));
        Assert.All(landscape, e => Assert.Equal(0u, e.Type));
    }

    [Fact]
    public void First_seen_vs_read_c20_and_c35()
    {
        var install = Require();
        using var big = BigArchive.Open(install.ShadersBigPath);

        ShaderProgram Load(string bank, string name)
        {
            var b = big.SubBanks.First(s => s.Name == bank);
            var e = big.ReadEntries(b).First(x => x.Name == name);
            return ShaderProgram.Parse(e.Name, b.Name, e.Type, big.Read(e));
        }

        var land = Load("SHADERS_LANDSCAPE_FOREGROUND", "VSHADER_LANDSCAPE_FOREGROUND");
        var stat = Load("SHADERS_STATIC", "VSHADER_STATIC_DIRLIGHT_FOG");
        var skin = Load("SHADERS_PALSKIN", "VSHADER_PALSKIN_DIRLIGHT_FOG");
        foreach (var vs in new[] { land, stat, skin })
        {
            Assert.Contains(WorldShading.DirLightStartRegister, vs.ConstRegisters);
            Assert.Contains(WorldShading.DirLightStartRegister + 1, vs.ConstRegisters);
            Assert.Contains(WorldShading.LitRegister, vs.ConstRegisters);
            Assert.Contains(2, vs.ConstRegisters);
            Assert.Contains(18, vs.ConstRegisters);
        }

        Assert.Contains(3, land.ConstRegisters);
        Assert.Contains(5, stat.ConstRegisters);
        Assert.DoesNotContain(20, Load("SHADERS_STATIC", "VSHADER_STATIC_UNLIT").ConstRegisters);
        Assert.Equal(19, WorldShading.DirLightStartRegister);
        Assert.Equal(2, WorldShading.RegistersPerLight);
        Assert.Equal(35, WorldShading.LitRegister);
        Assert.Equal(38, WorldShading.PaletteSkinStartRegister);
        Assert.Equal(58, WorldShading.PaletteSkinRegisterCount);
        Assert.False(WorldShading.PaletteSkinOffsetIsUploaded(0));
        Assert.True(WorldShading.PaletteSkinOffsetIsUploaded(1));
        Assert.True(WorldShading.PaletteSkinOffsetIsUploaded(8));
        Assert.False(WorldShading.PaletteSkinOffsetIsUploaded(38));
        Assert.Equal(new Vector4(0f, 1f, 0f, 0f), WorldShading.DirLightDirection);
        Assert.Equal(0.25f, WorldShading.DirLightColor.X);
        Assert.Equal(0.25f, WorldShading.DirLightColor.Y);
        Assert.Equal(0.25f, WorldShading.DirLightColor.Z);
        Assert.Equal(1f, WorldShading.DirLightColor.W);
        Assert.Equal(new Vector4(0f, 0f, 0f, 1f), WorldShading.LitColor);
        Assert.Contains(WorldShading.PaletteSkinStartRegister, skin.ConstRegisters);
        Assert.DoesNotContain(WorldShading.PointLightStartRegister, land.ConstRegisters);
        Assert.DoesNotContain(WorldShading.PointLightStartRegister, stat.ConstRegisters);
        Assert.DoesNotContain(WorldShading.PointAttenRegister, stat.ConstRegisters);
        var two = Load("SHADERS_STATIC", "VSHADER_STATIC_DIRLIGHT_2POINTLIGHTS_FOG");
        Assert.Contains(WorldShading.PointLightStartRegister, two.ConstRegisters);
        Assert.Contains(WorldShading.PointLightStartRegister + 1, two.ConstRegisters);
        Assert.Contains(WorldShading.PointAttenRegister, two.ConstRegisters);
        Assert.Equal(21, WorldShading.PointLightStartRegister);
        Assert.Equal(31, WorldShading.PointAttenRegister);
        Assert.Equal(1, WorldShading.LightingModeDefault);
        Assert.Equal(6, WorldShading.ShaderFamilySlotCount);
        Assert.Equal(5, WorldShading.PackedLightCountCap);
        var land2 = Load("SHADERS_LANDSCAPE_FOREGROUND", "VSHADER_LANDSCAPE_FOREGROUND_2LIGHTS");
        Assert.Contains(WorldShading.PointLightStartRegister, land2.ConstRegisters);
        var five = Load("SHADERS_STATIC", "VSHADER_STATIC_DIRLIGHT_5POINTLIGHTS_FOG");
        Assert.Contains(WorldShading.PointLightStartRegister, five.ConstRegisters);
        Assert.Equal(0, WorldShading.FirstSeenPackedLightCount);
        Assert.Equal(0, WorldShading.CapPackedLightCount(-3));
        Assert.Equal(5, WorldShading.CapPackedLightCount(9));
        Assert.Equal(2, WorldShading.CapPackedLightCount(2));
        Assert.Equal(0, WorldShading.SelectFamilySlot(0));
        Assert.Equal(0, WorldShading.SelectFamilySlot(2));
        Assert.Equal("VSHADER_STATIC_DIRLIGHT_FOG", WorldShading.StaticFamilyShader(2));
        Assert.Equal("VSHADER_LANDSCAPE_FOREGROUND", WorldShading.LandscapeFamilyShader(2));
        Assert.Equal(2, WorldShading.PaletteSkinLayoutIndex);
        Assert.Equal("VSHADER_PALSKIN_DIRLIGHT_FOG", WorldShading.PalskinFamilyShader(0));
        Assert.Equal("VSHADER_PALSKIN_DIRLIGHT_FOG", WorldShading.PalskinFamilyShader(2));
        Assert.Contains(WorldShading.PaletteSkinStartRegister,
            Load("SHADERS_PALSKIN", WorldShading.PalskinFamilyShader(0)).ConstRegisters);
        Assert.Equal(WorldShading.StaticFamilySlotShaders[0], Load("SHADERS_STATIC", WorldShading.StaticFamilyShader(0)).Name);
        Assert.Equal(WorldShading.LandscapeFamilySlotShaders[0], Load("SHADERS_LANDSCAPE_FOREGROUND", WorldShading.LandscapeFamilyShader(0)).Name);
        Assert.Equal(0.1f, WorldShading.AddLightMin);
        Assert.Equal(1f / 255f, WorldShading.AddLightChannelMin, 6);
        Assert.True(WorldShading.QualifiesAsAddableLight(130f / 255f, 60f / 255f, 5f / 255f, 8f, 9f));
        Assert.False(WorldShading.QualifiesAsAddableLight(130f / 255f, 60f / 255f, 5f / 255f, 0.05f, 9f));
        Assert.False(WorldShading.QualifiesAsAddableLight(0f, 60f / 255f, 5f / 255f, 8f, 9f));
    }
}
