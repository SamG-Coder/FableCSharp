using System.Numerics;
using Fable.Core;
using Fable.Formats;
using Fable.Formats.Banks;
using Fable.Formats.Defs;
using Fable.Formats.IO;
using Fable.Formats.Levels;
using Fable.Formats.Meshes;
using Fable.Formats.Scene;
using Fable.Formats.Textures;
using Fable.Game;
using Fable.Render;
using Fable.Render.Parity.Dx9Vulkan;
using Silk.NET.Vulkan;

namespace Fable.Formats.Tests;

/// <summary>
/// Fresh consumer of the shipped Dx9Vulkan translations and
/// first-seen Formats math. Goldens come from exe/asset locks,
/// not screenshots.
/// </summary>
public sealed class Dx9VulkanParityTests
{
    [Fact]
    public void Known_vertex_bytes_decode_position_normal_uv()
    {
        Assert.Equal(0f, MeshFile.DecompressUv(16384));
        Assert.Equal(-8f, MeshFile.DecompressUv(0));
        Assert.Equal(8, MeshFile.PackedUvOffset(1, 12, 4, false));
        Assert.Equal(4, MeshFile.PackedNormalOffset(1, 12, 4, false));
        Assert.Equal(12, Dx9VulkanVertexFormat.PackedNormalOffset(1, 32, 0, false));

        var up = PackedDirection.Unpack(511u << 22);
        Assert.InRange(up.Z, 0.99f, 1.01f);
        Assert.InRange(MathF.Abs(up.X), 0f, 0.02f);

        var packedUv = new byte[12];
        BitConverter.GetBytes((short)16384).CopyTo(packedUv, 8);
        BitConverter.GetBytes((short)16384).CopyTo(packedUv, 10);
        Assert.Equal(Vector2.Zero, MeshFile.ReadUv(packedUv, 8, true, 1));

        var floatN = new byte[24];
        BitConverter.GetBytes(0f).CopyTo(floatN, 12);
        BitConverter.GetBytes(0f).CopyTo(floatN, 16);
        BitConverter.GetBytes(1f).CopyTo(floatN, 20);
        var n = MeshFile.ReadNormal(floatN, 12, packedNorm: false, entryType: 1);
        Assert.Equal(Vector3.UnitZ, n);
    }

    [Fact]
    public void Known_indices_unwind_to_triangles_with_strip_swap()
    {
        var b = 1;
        var c = 2;
        Dx9VulkanPrimitive.UnwindStripTriangle(0, ref b, ref c);
        Assert.Equal(1, b);
        Assert.Equal(2, c);
        Dx9VulkanPrimitive.UnwindStripTriangle(1, ref b, ref c);
        Assert.Equal(2, b);
        Assert.Equal(1, c);
        Assert.Equal(PrimitiveTopology.TriangleList, Dx9VulkanPrimitive.World);
    }

    [Fact]
    public void Known_material_maps_to_dx9_semantic_state()
    {
        Assert.Equal(22, D3dDeviceState.CullMode);
        Assert.Equal(3, D3dDeviceState.CullCcw);
        Assert.Equal(5, D3dDeviceState.FirstSeenPalskinSrcBlend);
        Assert.Equal(6, D3dDeviceState.FirstSeenPalskinDestBlend);
        Assert.Equal(1, D3dDeviceState.FirstSeenFogEnable);
        Assert.Equal(0xFF000000u, D3dDeviceState.FirstSeenFogColorArgb);
        Assert.Equal(4, D3dDeviceState.FirstSeenZFunc);
        Assert.Equal(23, D3dDeviceState.ZFunc);
        Assert.False(WorldShading.FirstSeenAppliesCullNoneFromFlag1);
        Assert.True(WorldShading.FirstSeenPalskinSrcAlphaBlend);
        Assert.Equal("PSHADER_TEXTURE_DIFFUSE", WorldShading.FirstSeenStaticPsName);
        Assert.Equal("VSHADER_STATIC_DIRLIGHT_FOG", WorldShading.FirstSeenStaticVsName);
    }

    [Fact]
    public void Known_texture_header_is_dxt1_512_with_rgba_channels()
    {
        Assert.Equal(TextureCompression.Dxt1, TextureFile.Classify(0, 31, 512, 512, 1000));
        Assert.Equal(TextureCompression.Dxt5, TextureFile.Classify(0, 32, 64, 64, 1000));
        Assert.Equal(131072, TextureFile.TopMipSize(512, 512, TextureCompression.Dxt1));
        Assert.Equal(4, Dx9VulkanTextureFormat.ChannelCount(TextureCompression.Dxt1));
        Assert.Equal(Format.R8G8B8A8Unorm, Dx9VulkanTextureFormat.SampledFormat);
        Assert.False(Dx9VulkanTextureFormat.TreatAsSrgb);

        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var path = Path.Combine(install.DataRoot, "graphics", "pc", "textures.big");
        using var big = BigArchive.Open(path);
        var bank = big.SubBanks.Single(item => item.Name == "GBANK_MAIN_PC");
        var grass = big.ReadEntries(bank).First(e => e.Name == "LANDSCAPE_GRASS_PLAIN");
        var tex = TextureFile.Parse(grass.Id, grass.Name, grass.Type, grass.Info, big.Read(grass));
        Assert.Equal(31, tex.FormatCode);
        Assert.Equal(512, tex.Width);
        Assert.Equal(512 * 512 * 4, tex.Rgba.Length);
        Assert.True(tex.LeftoverBytes > 0, "raw lower mips present");
        Assert.True(TextureFile.FirstSeenTextureStoresRawLowerMips);
    }

    [Fact]
    public void Known_shot2_camera_builds_dx9_view_and_projection()
    {
        var cam = new Vector3(40.033936f, 130.47711f, 16.78288f);
        var look = new Vector3(-0.704544f, 0.6710376f, -0.23092493f);
        LandscapeFrustum.LetterboxCots(
            LandscapeFrustum.TurnsToRadians(LandscapeFrustum.FirstSeenFovTurns), 4f, 3f,
            out var cotH, out var cotV);
        var view = LandscapeFrustum.CotScaledView(cam, look, Vector3.UnitZ, cotH, cotV);
        LandscapeFrustum.HelperViewAxes(look, Vector3.UnitZ, out var right, out var lookN, out var upN);
        Assert.Equal(right.X * cotH, view.M11, 4);
        Assert.Equal(upN.X * cotV, view.M12, 4);
        Assert.Equal(lookN.X, view.M13, 4);
        Assert.True(LandscapeFrustum.FirstSeenViewLookIsZ);

        var dx9 = Dx9VulkanProjection.FirstSeenDx9Projection();
        Assert.Equal(1f, dx9.M11, 5);
        Assert.Equal(LandscapeFrustum.Dx9ProjectionYSign, dx9.M22, 5);
        Assert.True(LandscapeFrustum.FirstSeenProjWIsViewZ);
        LandscapeFrustum.ViewportZTerms(
            LandscapeFrustum.FirstSeenNear, LandscapeFrustum.FirstSeenFar,
            LandscapeFrustum.FirstSeenMinZ, LandscapeFrustum.FirstSeenMaxZ,
            out var m33, out var m34);
        Assert.Equal(m33, dx9.M33, 5);
        Assert.Equal(1f, dx9.M34, 5);
        Assert.Equal(m34, dx9.M43, 5);
    }

    [Fact]
    public void Dx9_to_vulkan_projection_flips_only_clip_y()
    {
        var dx9 = Dx9VulkanProjection.FirstSeenDx9Projection();
        var vk = Dx9VulkanProjection.ToVulkanProjection(dx9);
        Assert.Equal(dx9.M11, vk.M11);
        Assert.Equal(Dx9VulkanProjection.NdcYSign, vk.M22, 5);
        Assert.Equal(dx9.M33, vk.M33);
        Assert.Equal(dx9.M34, vk.M34);
        Assert.Equal(dx9.M43, vk.M43);
        Assert.NotEqual(dx9.M22, vk.M22);
    }

    [Fact]
    public void Fresh_consumer_shot2_house_vertex_clip_ndc_and_rs_maps()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var things = levels.LoadThings(RegionTravel.NewGameRegion);
        var shot = new ScriptedCamera();
        Assert.True(shot.UseCamera(things.Things, RegionTravel.IntroFirstSeenCamera));
        Assert.Equal(RegionTravel.IntroFirstSeenCamera, shot.ActiveName);

        var house = things.Things.First(t => t.ScriptName == "HerosOldHouse");
        var hx = house.PositionX!.Value;
        var hy = house.PositionY!.Value;
        var world = WorldGeometry.Build(install, RegionTravel.NewGameRegion, things.Things);
        var wall = world.Triangles.First(t =>
            t.Layer == SceneLayer.Prop
            && t.TextureId == GameBin.HerosOldHouseInteriorWallTexture
            && MathF.Abs((t.A.X + t.B.X + t.C.X) / 3f - hx) < 20f
            && MathF.Abs((t.A.Y + t.B.Y + t.C.Y) / 3f - hy) < 20f);
        var vertex = wall.A;

        var dx9Wvp = shot.ViewProjection(4f / 3f);
        Assert.Equal(LandscapeFrustum.Dx9ProjectionYSign, FlyCamera.ProjectionMatrix(4f / 3f, 72f).M22, 5);

        var dx9Clip = Dx9VulkanProjection.TransformClip(dx9Wvp, vertex);
        var vkWvp = Dx9VulkanProjection.ToVulkanWvp(dx9Wvp);
        var vkClip = Dx9VulkanProjection.TransformClip(vkWvp, vertex);
        var vkNdc = Dx9VulkanProjection.ToNdc(vkClip);
        var dx9Ndc = Dx9VulkanProjection.ToNdc(dx9Clip);

        Assert.Equal(dx9Clip.X, vkClip.X, 4);
        Assert.Equal(-dx9Clip.Y, vkClip.Y, 4);
        Assert.Equal(dx9Clip.Z, vkClip.Z, 4);
        Assert.Equal(dx9Clip.W, vkClip.W, 4);
        Assert.True(vkClip.W > 0f, $"SHOT2 house W={vkClip.W} v={vertex}");
        Assert.Equal(-dx9Ndc.Y, vkNdc.Y, 4);
        Assert.InRange(vkNdc.X, -2f, 2f);
        Assert.InRange(vkNdc.Y, -2f, 2f);

        Assert.Equal(CompareOp.LessOrEqual, Dx9VulkanDepth.CompareOp(D3dDeviceState.FirstSeenZFunc));
        Assert.Equal(CompareOp.LessOrEqual, Dx9VulkanDepth.FirstSeenCompareOp);
        Assert.Equal(FrontFace.CounterClockwise, Dx9VulkanRasterState.FirstSeenFrontFace);
        Assert.Equal(CullModeFlags.BackBit, Dx9VulkanRasterState.FirstSeenCullMode);
        Assert.Equal(CullModeFlags.BackBit, Dx9VulkanRasterState.CullMode(D3dDeviceState.CullCcw));
    }

    [Fact]
    public void Winding_preserve_and_depth_compare_equivalence()
    {
        var ib = 1;
        var ic = 2;
        Dx9VulkanPrimitive.UnwindStripTriangle(0, ref ib, ref ic);
        Assert.Equal((1, 2), (ib, ic));
        Dx9VulkanPrimitive.UnwindStripTriangle(1, ref ib, ref ic);
        Assert.Equal((2, 1), (ib, ic));

        Assert.Equal(CompareOp.LessOrEqual, Dx9VulkanDepth.CompareOp(4));
        Assert.Equal(CompareOp.Less, Dx9VulkanDepth.CompareOp(2));
        Assert.Equal(CompareOp.Always, Dx9VulkanDepth.CompareOp(8));
    }

    [Fact]
    public void Uv_and_colour_channel_conversion()
    {
        Assert.Equal(0f, MeshFile.DecompressUv(16384));
        var rgb = Dx9VulkanColor.FromD3dColorBgr(10, 20, 30);
        Assert.Equal(30 / 255f, rgb.X, 5);
        Assert.Equal(20 / 255f, rgb.Y, 5);
        Assert.Equal(10 / 255f, rgb.Z, 5);
        var clear = Dx9VulkanColor.FromD3dArgb(D3dDeviceState.FirstSeenFogColorArgb);
        Assert.Equal(Vector4.UnitW, clear);
        Assert.Equal(Vector4.UnitW, Dx9VulkanColor.FirstSeenClear);
    }

    [Fact]
    public void Shader_constant_registers_and_bone_palette_layout()
    {
        Assert.Equal(5, Dx9VulkanShaderConstants.WvpStartRegister);
        Assert.Equal(4, Dx9VulkanShaderConstants.WvpRegisterCount);
        Assert.Equal(WorldShading.FirstSeenC0, Dx9VulkanShaderConstants.C0);
        Assert.Equal(WorldShading.FirstSeenC1, Dx9VulkanShaderConstants.C1);
        Assert.Equal(WorldShading.FirstSeenC3, Dx9VulkanShaderConstants.C3);
        Assert.Equal(WorldShading.DirLightDirection, Dx9VulkanShaderConstants.DirLightDirection);
        Assert.Equal(38, WorldShading.DerivedPaletteStartRegister);
        Assert.Equal(3, WorldShading.BoneFloat4sPerInfluence);
        Assert.Equal(9, WorldShading.BoneConstantCount(3));
        Assert.Equal(64, WorldShading.BoneRecordBytes);
        Assert.True(WorldShading.FirstSeenBoneUploadWritesC38);
        Assert.False(WorldShading.FirstSeenPlaysAnim);
        Assert.Equal(38, Dx9VulkanShaderConstants.PaletteStartRegister);
        Assert.Equal(3, Dx9VulkanShaderConstants.PaletteFloat4sPerBone);
        Assert.True(Dx9VulkanShaderConstants.FirstSeenPaletteIsBindPose);
        Assert.Equal(28, Dx9VulkanVertexFormat.FirstSeenPalskinStride);
        Assert.Equal(0x14u, Dx9VulkanVertexFormat.FirstSeenPalskinInitFlags);
        Assert.Equal(12, Dx9VulkanVertexFormat.PalskinBlendIndexOffset(1, 28, 0x14, true));
        Assert.Equal(16, Dx9VulkanVertexFormat.PalskinBlendWeightOffset(1, 28, 0x14, true));
        Assert.Equal(20, Dx9VulkanVertexFormat.PackedNormalOffset(1, 28, 0x14, true));
        Assert.Equal(24, Dx9VulkanVertexFormat.PackedUvOffset(1, 28, 0x14, true));

        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var path = Path.Combine(install.DataRoot, "graphics", "graphics.big");
        using var big = BigArchive.Open(path);
        var bank = big.SubBanks.First(item => item.Name.Contains("MESH", StringComparison.OrdinalIgnoreCase));
        var entry = big.ReadEntries(bank).First(item => item.Id == 4300);
        var kid = MeshFile.Parse(big.Read(entry), (int)entry.Type);
        var palettes = WorldShading.FirstSeenPalettes(kid.Bones);
        Assert.Equal(kid.Bones.Count, palettes.Length);
        Assert.True(palettes.Length >= 76);
        var identityish = 0;
        foreach (var m in palettes)
        {
            var delta = MathF.Abs(m.M11 - 1f) + MathF.Abs(m.M22 - 1f) + MathF.Abs(m.M33 - 1f)
                        + MathF.Abs(m.M41) + MathF.Abs(m.M42) + MathF.Abs(m.M43);
            if (delta < 0.05f)
                identityish++;
        }

        Assert.True(identityish > 0, "bind-pose palettes should include near-identity bones");
    }

    [Fact]
    public void Sampler_and_blend_translations()
    {
        var sampler = Dx9VulkanSamplerState.FirstSeenTemporary();
        Assert.Equal(Filter.Nearest, sampler.MagFilter);
        Assert.Equal(SamplerAddressMode.Repeat, sampler.AddressModeU);
        Assert.Equal(0f, sampler.MaxLod);

        var frontend = Dx9VulkanSamplerState.FrontendType22();
        Assert.Equal(Filter.Nearest, frontend.MagFilter);
        Assert.Equal(Filter.Nearest, frontend.MinFilter);
        Assert.Equal(SamplerAddressMode.ClampToEdge, frontend.AddressModeU);
        Assert.Equal(SamplerAddressMode.ClampToEdge, frontend.AddressModeV);
        Assert.Equal(0f, frontend.MaxLod);

        Assert.Equal(BlendFactor.SrcAlpha, Dx9VulkanBlendState.FirstSeenPalskinSrc);
        Assert.Equal(BlendFactor.OneMinusSrcAlpha, Dx9VulkanBlendState.FirstSeenPalskinDst);
        var alpha = Dx9VulkanBlendState.PalskinSrcAlpha();
        Assert.True(alpha.BlendEnable);
        Assert.Equal(BlendFactor.SrcAlpha, alpha.SrcColorBlendFactor);
        var opaque = Dx9VulkanBlendState.Opaque();
        Assert.False(opaque.BlendEnable);
    }

    [Fact]
    public void First_seen_pass_order_and_shader_names()
    {
        Assert.True(ScenePasses.Rank(0x4) < ScenePasses.Rank(0x40));
        Assert.True(ScenePasses.Rank(0x40) < ScenePasses.Rank(0x20));
        Assert.True(ScenePasses.Rank(0x20) < ScenePasses.Rank(0x100));
        Assert.True(ScenePasses.Rank(0x100) < ScenePasses.Rank(0x2000));
        Assert.True(ScenePasses.Rank(0x2000) < ScenePasses.Rank(0x80));
        Assert.True(ScenePasses.Rank(0x80) < ScenePasses.Rank(0x200));
        Assert.True(ScenePasses.Rank(0x2000) < ScenePasses.Rank(0x20000));
        Assert.Equal("VSHADER_LANDSCAPE_FOREGROUND", WorldShading.LandscapeFamilyShader(0));
        Assert.Equal("VSHADER_STATIC_DIRLIGHT_FOG", WorldShading.StaticFamilyShader(0));
        Assert.Equal("VSHADER_PALSKIN_DIRLIGHT_FOG", WorldShading.PalskinFamilyShader(0));
        Assert.False(LandscapeTextures.FirstSeenWaterDrawShouldSubmit);
        Assert.Contains("mul_x2", LineShaders.MeshFragment, StringComparison.Ordinal);
        Assert.Contains("min(dp, c0y)", LineShaders.MeshVertex, StringComparison.Ordinal);
    }

    [Fact]
    public void PackWvp_matches_compose_then_y_flip()
    {
        var cam = new Vector3(40.033936f, 130.47711f, 16.78288f);
        var look = new Vector3(-0.704544f, 0.6710376f, -0.23092493f);
        LandscapeFrustum.LetterboxCots(
            LandscapeFrustum.TurnsToRadians(0.2f), 4f, 3f, out var cotH, out var cotV);
        var view = LandscapeFrustum.CotScaledView(cam, look, Vector3.UnitZ, cotH, cotV);
        var dx9 = Dx9VulkanProjection.FirstSeenDx9Projection();
        var packed = Dx9VulkanShaderConstants.PackWvp(
            LandscapeFrustum.IdentityWorld(), view, dx9);
        var expected = Dx9VulkanProjection.ToVulkanWvp(
            LandscapeFrustum.ComposeWvp(LandscapeFrustum.IdentityWorld(), view, dx9));
        Assert.Equal(expected, packed);

        var house = new Vector3(34f, 129f, 14f);
        var ndc = Dx9VulkanProjection.ToNdc(Dx9VulkanProjection.TransformClip(packed, house));
        Assert.True(ndc.W > 0f);

        var tcam = Dx9VulkanShaderConstants.PackWvp(
            LandscapeFrustum.LandscapeWorld(cam), view, dx9);
        var worldLand = Dx9VulkanShaderConstants.PackWorldSpaceLandscapeWvp(view, dx9);
        Assert.Equal(packed, worldLand);
        var tcamNdc = Dx9VulkanProjection.ToNdc(Dx9VulkanProjection.TransformClip(tcam, house));
        Assert.True(
            MathF.Abs(tcamNdc.X - ndc.X) > 0.1f || MathF.Abs(tcamNdc.Y - ndc.Y) > 0.1f,
            "T(cam) on world-space house is not the host landscape WVP");
        Assert.True(LandscapeFrustum.FirstSeenLandscapeFileVertsAreWorldSpace);
        Assert.True(LandscapeFrustum.FirstSeenLandscapeDeviceVbIsCameraRelative);
        Assert.True(LandscapeFrustum.HostTcamOnWorldSpaceLandscapeIsDisproven);
        Assert.Equal(Matrix4x4.Identity, LandscapeFrustum.HostWorldSpaceLandscapeWorld());
        Assert.True(Dx9VulkanShaderConstants.UnlitRgbIsC3Leftover);
        Assert.True(Dx9VulkanShaderConstants.SkyPsConstantsUnread);
        var unlit = WorldShading.EvaluateDirLightRgb(Vector3.Zero);
        Assert.Equal(new Vector3(0f, 0.125f, 0f), unlit);
        var unlitPs = WorldShading.FirstSeenEvaluateTextureDiffuseRgb(Vector3.One, unlit);
        Assert.Equal(0f, unlitPs.X, 5);
        Assert.Equal(0.25f, unlitPs.Y, 5);
        Assert.Equal(0f, unlitPs.Z, 5);
    }
}
