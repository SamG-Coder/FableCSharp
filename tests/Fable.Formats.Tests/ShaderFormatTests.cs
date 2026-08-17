using System.Numerics;
using Fable.Core;
using Fable.Formats;
using Fable.Formats.Banks;
using Fable.Formats.Levels;
using Fable.Formats.Scene;
using Fable.Formats.Shaders;
using Fable.Formats.Sky;

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
        Assert.True(bg.HasMulX2, "landscape BG is mul_x2_sat t0 * v0");
        Assert.True(LandscapeTextures.FirstSeenBackgroundPsMulX2);
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
        var bgVs = Load("SHADERS_LANDSCAPE_BACKGROUND", "VSHADER_LANDSCAPE_BACKGROUND");
        foreach (var vs in new[] { land, stat, skin })
        {
            Assert.Contains(WorldShading.DirLightStartRegister, vs.ConstRegisters);
            Assert.Contains(WorldShading.DirLightStartRegister + 1, vs.ConstRegisters);
            Assert.Contains(WorldShading.LitRegister, vs.ConstRegisters);
            Assert.Contains(WorldShading.FogPlaneRegister, vs.ConstRegisters);
            Assert.Contains(WorldShading.FogColorRegister, vs.ConstRegisters);
            Assert.Contains(2, vs.ConstRegisters);
            Assert.Contains(3, vs.ConstRegisters);
            Assert.Contains(18, vs.ConstRegisters);
            Assert.False(vs.HasLit);
            Assert.False(WorldShading.FirstSeenVsUsesLitC35);
            Assert.Equal(0x10u, ShaderProgram.LitOpcode);
            Assert.True(vs.TryGetDirLightAddsC3(), vs.Name);
        }
        Assert.True(WorldShading.FirstSeenDirLightAddsC3);
        Assert.Equal(new Vector4(0f, 0.125f, 0f, 0f), WorldShading.FirstSeenC3);
        Assert.True(skin.TryGetPalskinA0RelativeC38());
        Assert.False(land.TryGetPalskinA0RelativeC38());
        Assert.False(stat.TryGetPalskinA0RelativeC38());
        Assert.True(skin.TryGetPalskinInputs(out var pIn, out var iIn, out var wIn, out var nIn, out var uvIn));
        Assert.Equal(0, pIn);
        Assert.Equal(1, iIn);
        Assert.Equal(2, wIn);
        Assert.Equal(3, nIn);
        Assert.Equal(4, uvIn);
        Assert.False(stat.TryGetPalskinInputs(out _, out _, out _, out _, out _));
        Assert.True(skin.TryGetOd0WFromC0Y());
        Assert.True(stat.TryGetOd0WFromC0Y());
        Assert.True(bgVs.TryGetOd0WFromC0Y());
        Assert.False(land.TryGetOd0WFromC0Y());
        Assert.True(land.TryGetForegroundOd0WFromC42());
        Assert.False(skin.TryGetForegroundOd0WFromC42());
        Assert.False(stat.TryGetForegroundOd0WFromC42());
        Assert.True(WorldShading.FirstSeenStaticOd0WIsC0Y);
        Assert.True(WorldShading.FirstSeenPalskinOd0WIsC0Y);
        Assert.True(WorldShading.FirstSeenBackgroundOd0WIsC0Y);
        Assert.True(LandscapeTextures.FirstSeenForegroundOd0WUsesC42);
        Assert.False(LandscapeTextures.FirstSeenWritesC42);
        Assert.Equal(Vector4.Zero, LandscapeTextures.FirstSeenC42);
        Assert.Equal(0f, LandscapeTextures.EvaluateForegroundOd0W(
            new Vector3(1f, 2f, 3f), LandscapeTextures.FirstSeenC42, 1f));
        Assert.Contains(42, land.ConstRegisters);
        Assert.DoesNotContain(42, stat.ConstRegisters);
        Assert.DoesNotContain(42, skin.ConstRegisters);
        Assert.False(land.HasConstDef(LandscapeTextures.Od0WFadeRegister));
        Assert.True(land.TryGetOPosWvpC5C8());
        Assert.True(stat.TryGetOPosWvpC5C8());
        Assert.True(skin.TryGetOPosWvpC5C8());
        Assert.False(LandscapeFrustum.FirstSeenVsReadsSeparateWvp);
        Assert.Equal(5, LandscapeFrustum.WvpStartRegister);
        Assert.Equal(4, LandscapeFrustum.WvpRegisterCount);
        Assert.Equal(752, LandscapeFrustum.WvpUploadOffset);
        foreach (var vs in new[] { land, stat, skin })
        {
            for (var r = 9; r <= 16; r++)
                Assert.DoesNotContain(r, vs.ConstRegisters);
        }
        Assert.True(stat.TryGetOt0FromInput(out var statOt0));
        Assert.Equal(2, statOt0);
        Assert.True(skin.TryGetOt0FromInput(out var skinOt0));
        Assert.Equal(4, skinOt0);
        Assert.False(land.TryGetOt0FromInput(out _));
        Assert.True(WorldShading.FirstSeenStaticOt0IsV2);
        Assert.True(WorldShading.FirstSeenPalskinOt0IsV4);
        Assert.Equal(0x112u, WorldShading.FirstSeenStaticFvf);
        Assert.Equal(32, WorldShading.FirstSeenStaticStrideBytes);
        Assert.Equal(0x00BB2540u, WorldShading.FirstSeenStaticLitDraw);
        Assert.Equal(0x00BB30A0u, WorldShading.FirstSeenStaticLitCaller);
        Assert.Equal(0x00B8B630u, WorldShading.FirstSeenStaticCompactCtor);
        Assert.Equal(0x00BB301Eu, WorldShading.FirstSeenStaticSetTexture);
        Assert.Equal(1, WorldShading.FirstSeenStaticTextureStages);
        Assert.Equal("PSHADER_TEXTURE_DIFFUSE", WorldShading.FirstSeenStaticPsName);
        Assert.Equal("VSHADER_STATIC_DIRLIGHT_FOG", WorldShading.FirstSeenStaticVsName);
        Assert.True(WorldShading.FirstSeenStaticPsMulX2);
        Assert.True(WorldShading.FirstSeenStaticPsC0HasWriter);
        Assert.Equal(Vector4.One, WorldShading.FirstSeenStaticPsC0);
        Assert.Equal("PSCONST_OUTPUT_FACTOR", WorldShading.FirstSeenStaticPsC0Name);
        Assert.Equal(2, WorldShading.FirstSeenStaticPsC0BankSlot);
        Assert.Equal(new Vector3(1f, 1f, 1f),
            WorldShading.FirstSeenEvaluateTextureDiffuseRgb(new Vector3(0.5f, 0.5f, 0.5f), Vector3.One));

        Assert.Contains(3, land.ConstRegisters);
        Assert.DoesNotContain(1, land.ConstRegisters);
        Assert.Contains(LandscapeTextures.Ot1RegisterX, land.ConstRegisters);
        Assert.Contains(LandscapeTextures.Ot1RegisterY, land.ConstRegisters);
        Assert.False(LandscapeTextures.FirstSeenLandscapeVsReadsC1);
        Assert.False(LandscapeTextures.FirstSeenUploadsC1LayerFlip);
        Assert.True(land.TryGetOt0FromV3(out var ot0Reg));
        Assert.Equal(3, ot0Reg);
        Assert.True(bgVs.TryGetBackgroundOt0FromV3(out var bgOt0));
        Assert.Equal(3, bgOt0);
        Assert.False(bgVs.TryGetOt0FromV3(out _));
        Assert.True(LandscapeTextures.FirstSeenBackgroundOt0IsV3);
        Assert.True(land.TryGetOt1Projected(out var ot1));
        Assert.Equal(0, ot1.PosType);
        Assert.True(land.TryGetOPosSubtractC4(out var opos));
        Assert.Equal(0, opos.XyInput);
        Assert.Equal(1, opos.ZInput);
        Assert.True(LandscapeTextures.FirstSeenOPosSubtractsC4);
        Assert.False(LandscapeTextures.FirstSeenUploadsC4InverseRow2OnLandscape);
        Assert.True(LandscapeTextures.FirstSeenC4UsesDeviceDefault);
        Assert.Equal(Vector4.Zero, LandscapeTextures.FirstSeenC4);
        Assert.Equal(4, LandscapeTextures.OPosC4Register);
        Assert.Equal(0x00B545D5u, LandscapeTextures.C4InverseRow2Upload);
        Assert.Equal(0x00B54310u, LandscapeTextures.C4InverseRow2UploadFn);
        Assert.Equal(0x00B555A0u, LandscapeTextures.C4InverseRow2UploadCaller);
        Assert.False(stat.TryGetOPosSubtractC4(out _));
        Assert.False(skin.TryGetOPosSubtractC4(out _));
        Assert.DoesNotContain(4, stat.ConstRegisters);
        Assert.DoesNotContain(4, skin.ConstRegisters);
        Assert.Contains(4, land.ConstRegisters);
        Assert.Equal(
            new Vector4(40f, 130f, 16f, 1f),
            LandscapeTextures.LandscapeOPosPosition(new Vector3(40f, 130f, 16f), LandscapeTextures.FirstSeenC4, WorldShading.FirstSeenC0.Y));
        Assert.False(LandscapeFrustum.FirstSeenUploadsInverseRow2AsC4OnLandscape);
        Assert.True(LandscapeTextures.FirstSeenOt0FromV3);
        Assert.False(LandscapeTextures.FirstSeenOt0IsAlbedo);
        Assert.True(LandscapeTextures.FirstSeenOt1Projected);
        Assert.False(LandscapeTextures.FirstSeenOt1HasExplicitWriter);
        Assert.True(LandscapeTextures.FirstSeenOt1UsesDeviceDefault);
        Assert.Equal(Vector4.Zero, LandscapeTextures.Ot1C40);
        Assert.Equal(Vector4.Zero, LandscapeTextures.Ot1C41);
        Assert.False(land.HasConstDef(LandscapeTextures.Ot1RegisterX));
        Assert.False(land.HasConstDef(LandscapeTextures.Ot1RegisterY));
        Assert.Equal(2, LandscapeTextures.PerCellFirstSlot);
        Assert.Equal(3, LandscapeTextures.PerCellSecondSlot);
        Assert.Equal(0, LandscapeTextures.FirstSeenInnerRegisterBase);
        Assert.Equal(24, LandscapeTextures.GpuVertexStrideBytes);
        Assert.Equal(6, ShaderProgram.RegTypeTexCrdOut);
        Assert.Equal(1, ShaderProgram.SwizzleY);
        Assert.Equal(2, ShaderProgram.SwizzleZ);
        Assert.Equal(4, LandscapeTextures.C1LayerType);
        Assert.Equal(1, LandscapeTextures.LayerFlipRegister);
        Assert.Equal(new Vector2(1f, 0f), LandscapeTextures.C1OneLayer);
        Assert.Equal(new Vector2(0f, -1f), LandscapeTextures.C1TwoLayer);
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
        Assert.Equal(0x00F39D40u, WorldShading.LightApply);
        Assert.Equal(0x0098B2C0u, WorldShading.C35Setter);
        Assert.False(WorldShading.FirstSeenLightApplyWritesC35);
        Assert.True(WorldShading.FirstSeenC35IsSetterDefault);
        Assert.Equal(0x00BD2D90u, WorldShading.PalskinPacker);
        Assert.Equal(288, WorldShading.PalskinPackerDestOffset);
        Assert.True(WorldShading.FirstSeenPalskinPackerRebuildsWhenDestNull);
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
        Assert.False(WorldShading.FirstSeenUploadsPaletteC38);
        Assert.Equal(33, WorldShading.PaletteC38SlotIndex);
        Assert.Equal(16 + WorldShading.PaletteC38SlotIndex * 4, 148);
        Assert.Equal(2, WorldShading.PalskinJumpTablePass);
        Assert.Equal(4, WorldShading.PalskinHelperPass);
        Assert.True(WorldShading.PalskinPassUsesJumpTable(2));
        Assert.False(WorldShading.PalskinPassUsesJumpTable(4));
        Assert.False(WorldShading.PalskinPassUsesJumpTable(0x20));
        Assert.Equal(18, WorldShading.PalskinJumpTable.Length);
        Assert.Equal(0x00BD3C04u, WorldShading.PalskinJumpTarget(4));
        Assert.Equal(0x00BD42CDu, WorldShading.PalskinJumpTarget(16));
        Assert.Equal(0u, WorldShading.PalskinJumpTarget(5));
        Assert.Equal(0u, WorldShading.PalskinJumpTarget(0));
        Assert.True(WorldShading.FirstSeenBoneUploadWritesC38);
        Assert.Equal(new Vector4(256f, 256f, 256f, 256f), WorldShading.FirstSeenC1);
        Assert.Equal(0x00989BF0u, WorldShading.LayoutBasicFlush);
        Assert.Equal(2, WorldShading.LayoutBasicFloat4Count);
        Assert.Equal(0x00BD549Du, WorldShading.FirstSeenPalskinDefaultDraw);
        Assert.True(WorldShading.FirstSeenPalskinUsesA0RelativeC38);
        Assert.Equal(38, WorldShading.DerivedPaletteStartRegister);
        Assert.Equal(54, WorldShading.DerivedPaletteRegisterCount);
        Assert.Equal(64, WorldShading.BoneRecordBytes);
        Assert.Equal(3, WorldShading.BoneFloat4sPerInfluence);
        Assert.Equal(0, WorldShading.BoneConstantCount(-1));
        Assert.Equal(6, WorldShading.BoneConstantCount(2));
        Assert.Equal("VSHADER_PALSKIN_DIRLIGHT_FOG", WorldShading.PalskinFamilyShader(0));
        Assert.Equal("VSHADER_PALSKIN_DIRLIGHT_FOG", WorldShading.PalskinFamilyShader(2));
        Assert.False(WorldShading.FirstSeenPlaysAnim);
        Assert.False(WorldShading.FirstSeenAppliesCullNoneFromFlag1);
        Assert.False(WorldShading.FirstSeenFlag1WritesLayerType20);
        Assert.True(WorldShading.FirstSeenPalskinSrcAlphaBlend);
        Assert.False(WorldShading.FirstSeenFlag1SelectsAlphaBlend);
        Assert.True(WorldShading.FirstSeenPalskinReadsFlag1);
        Assert.False(WorldShading.FirstSeenStaticLitReadsFlag1);
        Assert.Equal(41, WorldShading.MaterialFlag1Offset);
        Assert.Equal(5, WorldShading.FirstSeenPalskinFlag1MaskOr);
        Assert.Equal(2, WorldShading.FirstSeenPalskinFlag2MaskOr);
        Assert.Equal(4, WorldShading.PalskinTypeIndex(1, 0, 0xFF, 1));
        Assert.Equal(0xFF, WorldShading.FirstSeenInstanceOpacity);
        Assert.Equal(28, WorldShading.PalskinHelperTypeIndexOffset);
        Assert.False(WorldShading.FirstSeenPalskinBindUsesHelperTypeIndex);
        Assert.False(WorldShading.FirstSeenPalskinDrainUsesType4);
        Assert.Equal(0x00B84720u, WorldShading.PrimQueueSubmit);
        Assert.Equal(0x00BD7110u, WorldShading.PalskinDrainVtbl20);
        Assert.Equal(0x00BD3C04u, WorldShading.PalskinType4JumpTarget);
        Assert.Contains(WorldShading.PaletteSkinStartRegister,
            Load("SHADERS_PALSKIN", WorldShading.PalskinFamilyShader(0)).ConstRegisters);
        Assert.Equal(WorldShading.StaticFamilySlotShaders[0], Load("SHADERS_STATIC", WorldShading.StaticFamilyShader(0)).Name);
        Assert.Equal(WorldShading.LandscapeFamilySlotShaders[0], Load("SHADERS_LANDSCAPE_FOREGROUND", WorldShading.LandscapeFamilyShader(0)).Name);
        Assert.Equal(0.1f, WorldShading.AddLightMin);
        Assert.Equal(1f / 255f, WorldShading.AddLightChannelMin, 6);
        Assert.True(WorldShading.QualifiesAsAddableLight(130f / 255f, 60f / 255f, 5f / 255f, 8f, 9f));
        Assert.False(WorldShading.QualifiesAsAddableLight(130f / 255f, 60f / 255f, 5f / 255f, 0.05f, 9f));
        Assert.False(WorldShading.QualifiesAsAddableLight(0f, 60f / 255f, 5f / 255f, 8f, 9f));
        Assert.Equal(2, WorldShading.FogPlaneRegister);
        Assert.Equal(18, WorldShading.FogColorRegister);
        Assert.Equal(new Vector4(0f, 0f, 0f, 1f), WorldShading.FogRecordColor);
        Assert.Equal(0f, WorldShading.FogColor.X);
        Assert.Equal(0f, WorldShading.FogColor.Y);
        Assert.Equal(0f, WorldShading.FogColor.Z);
        Assert.Equal(1000f, WorldShading.FogStart);
        Assert.Equal(2000f, WorldShading.FogRecordEnd);
        Assert.Equal(7000f, WorldShading.FogEnd);
        Assert.Equal(LandscapeFrustum.InverseRow0Register, WorldShading.FogPlaneRegister);
        Assert.Equal(LandscapeFrustum.LayoutFogRegister, WorldShading.FogColorRegister);
        Assert.Equal(LandscapeFrustum.FogRecordColor, WorldShading.FogRecordColor);
        foreach (var vs in new[] { land, stat, skin })
            Assert.True(vs.TryGetVertexFogSequence(out _), vs.Name);
        Assert.True(land.TryGetVertexFogSequence(out var fog));
        Assert.Equal(0, fog.PosType);
        Assert.Equal(0, fog.PosRegister);
        Assert.Equal(1f, WorldShading.FirstSeenC0.Y);
        Assert.Equal(1f, WorldShading.EvaluateVertexFog(0f, 1f, 1f));
        Assert.Equal(1f, WorldShading.EvaluateVertexFog(0f, 1f, 0f));
        Assert.Equal(0f, WorldShading.EvaluateVertexFog(2f, 1f, 1f));
        Assert.Equal(0.5f, WorldShading.EvaluateVertexFog(0.5f, 1f, 1f));
        Assert.True(WorldShading.FirstSeenAppliesVertexFogBlend);
        Assert.True(WorldShading.FirstSeenFogC2IsLinearViewZ);
        Assert.True(WorldShading.FirstSeenFogSaturates);
        Assert.False(LandscapeFrustum.FirstSeenUploadsInverseRow0AsC2);
        Assert.Equal(28, D3dDeviceState.FogEnable);
        Assert.Equal(1, D3dDeviceState.FirstSeenFogEnable);
        Assert.Equal(0xFF000000u, D3dDeviceState.FirstSeenFogColorArgb);
        Assert.Equal(0.75f, WorldShading.EvaluateVertexFog(
            0.25f, WorldShading.FirstSeenC0.Y, WorldShading.FogRecordColor.W), 5);
        Assert.Equal(ShaderProgram.Dp4Opcode, 0x09u);
        Assert.Equal(ShaderProgram.MinOpcode, 0x0Au);
        Assert.Equal(ShaderProgram.MadOpcode, 0x04u);
        Assert.Equal(1, ShaderProgram.RastOutFog);
    }

    [Fact]
    public void First_seen_inner_sky_opos_is_dp4_v0_c5()
    {
        var install = Require();
        using var big = BigArchive.Open(install.ShadersBigPath);
        var bank = big.SubBanks.First(s => s.Name == "SHADERS_SKY");
        var entry = big.ReadEntries(bank).First(e => e.Name == "VSHADER_INNER_SKY");
        var vs = ShaderProgram.Parse(entry.Name, bank.Name, entry.Type, big.Read(entry));
        Assert.True(vs.TryGetSkyOPosWvp());
        Assert.DoesNotContain(4, vs.ConstRegisters);
        Assert.Contains(5, vs.ConstRegisters);
        Assert.Contains(8, vs.ConstRegisters);
        Assert.Equal(0x00B662F0u, SkyPass.Draw);
        Assert.Equal(0x00B66190u, SkyPass.MeshDraw);
        Assert.Equal(0x2000u, SkyPass.FirstSeenLayerBit);
        Assert.False(SkyPass.FirstSeenUses400000);
        Assert.Equal(100f, SkyPass.FirstSeenNear);
        Assert.Equal(10000f, SkyPass.FirstSeenFar);
        Assert.Equal(0.99f, SkyPass.FirstSeenMinZ);
        Assert.Equal(1f, SkyPass.FirstSeenMaxZ);
        Assert.Equal(0x42C80000u, SkyPass.NearImm);
        Assert.Equal(0x461C4000u, SkyPass.FarImm);
        Assert.Equal(0x0139A704u, SkyPass.MinZConst);
        var land = big.ReadEntries(big.SubBanks.First(s => s.Name == "SHADERS_LANDSCAPE_FOREGROUND"))
            .First(e => e.Name == "VSHADER_LANDSCAPE_FOREGROUND");
        Assert.False(ShaderProgram.Parse(land.Name, "SHADERS_LANDSCAPE_FOREGROUND", land.Type, big.Read(land))
            .TryGetSkyOPosWvp());
    }

    [Fact]
    public void First_seen_static_ps_is_texture_diffuse_mul_v0_c0_mulx2_t0()
    {
        var install = Require();
        using var big = BigArchive.Open(install.ShadersBigPath);
        var pixel = big.SubBanks.First(s => s.Name == "PIXEL_SHADERS");
        var byName = big.ReadEntries(pixel).ToDictionary(e => e.Name, StringComparer.Ordinal);
        Assert.True(byName.ContainsKey(WorldShading.FirstSeenStaticPsName));
        var diffuse = ShaderProgram.Parse(
            WorldShading.FirstSeenStaticPsName, pixel.Name, 1, big.Read(byName[WorldShading.FirstSeenStaticPsName]));
        var fog = ShaderProgram.Parse(
            "PSHADER_TEXTURE_DIFFUSE_FOG", pixel.Name, 1, big.Read(byName["PSHADER_TEXTURE_DIFFUSE_FOG"]));
        Assert.True(diffuse.TryGetTextureDiffusePs());
        Assert.Equal(1, diffuse.TexCount);
        Assert.True(diffuse.HasMulX2);
        Assert.Contains("mul_x2", diffuse.ToListing());
        Assert.Contains("c0", diffuse.ToListing());
        Assert.DoesNotContain("def c0", diffuse.ToListing());
        Assert.False(fog.TryGetTextureDiffusePs());
        Assert.Equal(1, WorldShading.FirstSeenStaticTextureStages);
        Assert.Equal(0x00BB301Eu, WorldShading.FirstSeenStaticSetTexture);
        Assert.Equal(0x00BB30A0u, WorldShading.FirstSeenStaticLitCaller);
        Assert.Equal(0x00B8B630u, WorldShading.FirstSeenStaticCompactCtor);
        Assert.Equal(0x00988020u, WorldShading.FirstSeenStaticSetVertexShader);
        Assert.Equal(0x00988140u, WorldShading.FirstSeenStaticAttachPixelShader);
        Assert.True(WorldShading.FirstSeenStaticPsC0HasWriter);
        Assert.Equal(Vector4.One, WorldShading.FirstSeenStaticPsC0);
        Assert.Equal("PSCONST_OUTPUT_FACTOR", WorldShading.FirstSeenStaticPsC0Name);
        Assert.Equal(2, WorldShading.FirstSeenStaticPsC0BankSlot);
        Assert.Equal(0x0098ACF0u, WorldShading.FirstSeenStaticPsC0Writer);
        Assert.Equal(0x0098DB20u, WorldShading.FirstSeenStaticPsC0SlotAssign);
        Assert.Contains(WorldShading.FirstSeenStaticPsC0Name, diffuse.CommentStrings);
        Assert.Equal(0xDC, WorldShading.FirstSeenPalskinAttachPsOffset);
        Assert.Equal(new Vector3(1f, 1f, 1f),
            WorldShading.FirstSeenEvaluateTextureDiffuseRgb(new Vector3(0.5f, 0.5f, 0.5f), Vector3.One));
    }

    [Fact]
    public void First_seen_inner_sky_ps_is_four_tex_lrp_mulx2_c2()
    {
        var install = Require();
        using var big = BigArchive.Open(install.ShadersBigPath);
        var pixel = big.SubBanks.First(s => s.Name == "PIXEL_SHADERS");
        var byName = big.ReadEntries(pixel).ToDictionary(e => e.Name, StringComparer.Ordinal);
        var inner = ShaderProgram.Parse("PSHADER_INNER_SKY", pixel.Name, 1, big.Read(byName["PSHADER_INNER_SKY"]));
        var simple = ShaderProgram.Parse("PSHADER_INNER_SKY_SIMPLE", pixel.Name, 1, big.Read(byName["PSHADER_INNER_SKY_SIMPLE"]));
        var fog = ShaderProgram.Parse("PSHADER_TEXTURE_DIFFUSE_FOG", pixel.Name, 1, big.Read(byName["PSHADER_TEXTURE_DIFFUSE_FOG"]));
        Assert.True(inner.TryGetInnerSkyPs());
        Assert.False(simple.TryGetInnerSkyPs());
        Assert.False(fog.TryGetInnerSkyPs());
        Assert.True(simple.TryGetInnerSkySimplePs());
        Assert.False(inner.TryGetInnerSkySimplePs());
        Assert.Equal(4, inner.TexCount);
        Assert.Equal(2, simple.TexCount);
        Assert.Contains(0, inner.ConstRegisters);
        Assert.Contains(1, inner.ConstRegisters);
        Assert.Contains(2, inner.ConstRegisters);
        Assert.False(inner.HasConstDef(0));
        Assert.False(inner.HasConstDef(1));
        Assert.False(inner.HasConstDef(2));
        Assert.Contains("lrp", inner.ToListing());
        Assert.Contains("mul_x2", inner.ToListing());
        Assert.Contains("c2", inner.ToListing());
        Assert.DoesNotContain("def c2", inner.ToListing());
        Assert.Equal(0x12u, ShaderProgram.LrpOpcode);
        Assert.Equal(0x00B62BA8u, SkyPass.InnerSkyPsBind);
        Assert.Equal(0x00B62BB6u, SkyPass.InnerSkySimpleBind);
        Assert.Equal(0x00B62C2Du, SkyPass.InnerSkyFullBind);
        Assert.Equal(292, SkyPass.InnerSkyShaderStoreOffset);
        Assert.Equal(96, SkyPass.QualityDwordOffset);
        Assert.Equal(1, SkyPass.QualitySimpleAhMask);
        Assert.Equal(8, SkyPass.QualitySimpleBit);
        Assert.Equal(436, SkyPass.SetPixelShaderConstantFSlot);
        Assert.Equal(109, SkyPass.SetPixelShaderConstantFVtbl);
        Assert.Equal(260, SkyPass.SetTextureSlot);
        Assert.False(SkyPass.FirstSeenInnerSkyHasConstDef);
        Assert.False(SkyPass.FirstSeenSkyPsC2HasWriter);
        Assert.True(SkyPass.FirstSeenSkyMode2IsStandIn);
        Assert.False(SkyPass.FirstSeenQualityBitKnown);
        Assert.True(SkyPass.FirstSeenInnerSkyMulsVertexAlpha);
        Assert.Equal(92, SkyPass.InnerSkyVsC92);
        Assert.Equal(0x009888E0u, SkyPass.PsConstantWrapper);
        Assert.Equal(0x009888FCu, SkyPass.PsConstantWrapperCall);
        Assert.Equal(0x00989BF0u, SkyPass.LayoutBasicPsFlush);
        Assert.Equal(0x00989C98u, SkyPass.LayoutBasicPsUpload);
        Assert.Equal(0x013BC3B0u, SkyPass.LayoutBasicPsScratch);
        Assert.Equal(972, SkyPass.LayoutBasicPsBankOffset);
        Assert.Equal(484, SkyPass.LayoutBasicPsRecordOffset);
        Assert.True(SkyPass.FirstSeenSkyCallsLayoutBasicPsFlush);
        Assert.Equal(0x00B631DFu, SkyPass.SkyPsFlush0);
        Assert.Equal(0x00B66086u, SkyPass.SkyPsFlush4);
        Assert.Equal(2, SkyPass.LayoutBasicPsDirtyBit);
        Assert.Equal(0x00988A20u, SkyPass.WvpFlush);
        Assert.Equal(0x009897C0u, SkyPass.FogColorFlush);
        Assert.Equal(1f, SkyPass.DomeAlpha(0), 5);
        Assert.InRange(SkyPass.DomeAlpha(SkyPass.DomeRings - 1), 0f, 0.08f);
        Assert.Equal(2f, ScenePasses.ShaderMode(SceneSubmit.SkyElse));
    }
}
