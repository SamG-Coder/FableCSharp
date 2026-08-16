using System.Numerics;
using Fable.Formats;
using Fable.Formats.Levels;
using Fable.Formats.Sky;
using Fable.Render;

namespace Fable.Formats.Tests;

public sealed class CameraProjectionTests
{
    [Fact]
    public void Lookout_center_is_in_front_of_south_overview_camera()
    {
        var camera = new FlyCamera { Position = new Vector3(64f, -40f, 95f) };
        camera.LookAt(new Vector3(64f, 64f, 36f));

        var ndc = FlyCamera.Project(camera.ViewProjection(16f / 9f), new Vector3(64f, 64f, 36f));
        Assert.True(ndc.W > 0f, $"W={ndc.W}");
        Assert.InRange(ndc.X, -0.75f, 0.75f);
        Assert.False(LandscapeFrustum.FirstSeenViewUsesCreateLookAt);
    }

    [Fact]
    public void Gpu_upload_keeps_row_major_bytes_without_extra_transpose()
    {
        // Document the Vulkan push-constant convention: memcpy the System.Numerics
        // matrix as-is. GLSL column-major reads that as the needed transpose.
        var camera = new FlyCamera { Position = new Vector3(64f, -40f, 95f) };
        camera.LookAt(new Vector3(64f, 64f, 36f));
        var uploaded = camera.ViewProjection(16f / 9f);
        var ndc = FlyCamera.Project(uploaded, new Vector3(64f, 64f, 36f));
        Assert.True(ndc.W != 0f);
        Assert.InRange(ndc.X, -1f, 1f);
    }

    [Fact]
    public void Terrain_corners_stay_in_clip_from_overview()
    {
        var camera = new FlyCamera { Position = new Vector3(64f, -40f, 95f) };
        camera.LookAt(new Vector3(64f, 64f, 36f));
        var vp = camera.ViewProjection(16f / 9f);

        foreach (var point in new[]
                 {
                     new Vector3(0f, 0f, 36f),
                     new Vector3(128f, 0f, 36f),
                     new Vector3(0f, 128f, 36f),
                     new Vector3(128f, 128f, 36f),
                 })
        {
            var ndc = FlyCamera.Project(vp, point);
            Assert.True(ndc.W > 0f, $"{point} W={ndc.W}");
        }
    }

    [Fact]
    public void Extract_side_planes_keep_the_look_target_and_reject_behind()
    {
        var position = new Vector3(0f, 0f, 0f);
        var look = Vector3.UnitY;
        var cot = LandscapeFrustum.CotHalfAngle(float.DegreesToRadians(90f));
        var planes = LandscapeFrustum.ExtractSidePlanes(position, look, Vector3.UnitZ, cot, cot);
        Assert.Equal(4, planes.Length);
        Assert.False(LandscapeFrustum.AabbIsOutside(
            new Vector3(-1f, 2f, -1f), new Vector3(1f, 4f, 1f), planes));
        Assert.True(LandscapeFrustum.AabbIsOutside(
            new Vector3(-1f, -4f, -1f), new Vector3(1f, -2f, 1f), planes));
    }

    [Fact]
    public void Inverse_row0_is_c2_from_cot_scaled_inverse()
    {
        var cot = LandscapeFrustum.CotHalfAngle(float.DegreesToRadians(90f));
        var c2 = LandscapeFrustum.InverseRow0(
            Vector3.Zero, Vector3.UnitY, Vector3.UnitZ, cot, cot);
        Assert.Equal(1f, c2.X, 5);
        Assert.Equal(0f, c2.Y, 5);
        Assert.Equal(0f, c2.Z, 5);
        Assert.Equal(0f, c2.W, 5);
        LandscapeFrustum.CotScaledInverse(
            Vector3.Zero, Vector3.UnitY, Vector3.UnitZ, cot, cot,
            out var row0, out var row1, out var row2);
        Assert.Equal(c2, row0);
        Assert.Equal(2, LandscapeFrustum.InverseRow0Register);
        Assert.Equal(3, LandscapeFrustum.InverseRow1Register);
        Assert.Equal(4, LandscapeFrustum.InverseRow2Register);
        Assert.Equal(12, LandscapeFrustum.InverseColumnStrideBytes);
        Assert.Equal(0x00B54310u, LandscapeFrustum.CameraConstantUpload);
        Assert.Equal(0x00989B00u, LandscapeFrustum.SetVsConstantF4);
        Assert.False(LandscapeFrustum.FirstSeenUploadsInverseRow0AsC2);
        Assert.True(WorldShading.FirstSeenFogC2IsLinearViewZ);
        Assert.False(LandscapeFrustum.FirstSeenUsesThirdPersonView);
        Assert.Equal(new Vector3(0f, 0f, 1f), LandscapeFrustum.FirstSeenCameraUp);
        Assert.Equal(18, LandscapeFrustum.LayoutFogRegister);
        Assert.Equal(1, LandscapeFrustum.LayoutFogCount);
        Assert.Equal(56, LandscapeFrustum.LayoutFogRegisterOffset);
        Assert.Equal(new Vector4(0f, 0f, 0f, 1f), LandscapeFrustum.FogRecordColor);
        Assert.Equal(1000f, LandscapeFrustum.FogRecordStart);
        Assert.Equal(2000f, LandscapeFrustum.FogRecordEnd);
        Assert.Equal(0x00B47630u, LandscapeFrustum.FogCompute);
        Assert.Equal(0x009886C0u, LandscapeFrustum.FogColorSetter);
        Assert.Equal(0x009897C0u, LandscapeFrustum.FogColorFlush);
        Assert.Equal(444, LandscapeFrustum.WrapperFogColorOffset);
        Assert.Equal(0x20000, LandscapeFrustum.FogDirtyBit);
    }

    [Fact]
    public void First_seen_wvp_uses_helper_near_far_and_viewport_z()
    {
        LandscapeFrustum.ViewportZTerms(
            LandscapeFrustum.FirstSeenNear,
            LandscapeFrustum.FirstSeenFar,
            LandscapeFrustum.FirstSeenMinZ,
            LandscapeFrustum.FirstSeenMaxZ,
            out var m33, out var m34);
        Assert.Equal(0.1f, LandscapeFrustum.FirstSeenNear);
        Assert.Equal(4000f, LandscapeFrustum.FirstSeenFar);
        Assert.Equal(0.1f, LandscapeFrustum.FirstSeenMinZ);
        Assert.Equal(0.99f, LandscapeFrustum.FirstSeenMaxZ);
        Assert.Equal(0x00988A50u, LandscapeFrustum.WvpFlush);
        Assert.Equal(0x009883F0u, LandscapeFrustum.ProjBuilder);
        Assert.Equal(0x00988350u, LandscapeFrustum.ViewCopy);
        Assert.Equal(0x009881F0u, LandscapeFrustum.WorldCopy);
        Assert.Equal(5, LandscapeFrustum.LayoutWvpRegister);
        Assert.Equal(4, LandscapeFrustum.LayoutWvpCount);
        Assert.Equal(496, LandscapeFrustum.WrapperWorldOffset);
        Assert.Equal(560, LandscapeFrustum.WrapperViewOffset);
        Assert.Equal(624, LandscapeFrustum.WrapperProjOffset);
        Assert.Equal(752, LandscapeFrustum.WrapperWvpOffset);
        Assert.Equal(372, LandscapeFrustum.CameraProjOffset);
        Assert.Equal(0x01399D44u, LandscapeFrustum.HelperMinZConst);
        Assert.Equal(LandscapeFrustum.FirstSeenMinZ, m33 + m34 / LandscapeFrustum.FirstSeenNear, 4);
        Assert.Equal(LandscapeFrustum.FirstSeenMaxZ, m33 + m34 / LandscapeFrustum.FirstSeenFar, 4);

        LandscapeFrustum.LetterboxCots(
            float.DegreesToRadians(72f), 4f, 3f, out var cotH, out var cotV);
        var proj = FlyCamera.ProjectionMatrix(4f / 3f, 72f);
        Assert.True(LandscapeFrustum.FirstSeenProjXyIsIdentity);
        Assert.Equal(1f, proj.M11, 4);
        Assert.Equal(LandscapeFrustum.VulkanNdcYSign, proj.M22, 4);
        Assert.Equal(-m33, proj.M33, 4);
        Assert.Equal(-1f, proj.M34, 4);
        Assert.Equal(m34, proj.M43, 4);
        var cam = new Vector3(40.033936f, 130.47711f, 16.78288f);
        var lookDir = new Vector3(-0.704544f, 0.6710376f, -0.23092493f);
        var view = LandscapeFrustum.CotScaledView(cam, lookDir, Vector3.UnitZ, cotH, cotV);
        LandscapeFrustum.HelperViewAxes(lookDir, Vector3.UnitZ, out var right, out var lookN, out var upN);
        Assert.False(LandscapeFrustum.FirstSeenViewUsesCreateLookAt);
        Assert.False(LandscapeFrustum.FirstSeenViewReorthogonalizesLook);
        Assert.Equal(16, LandscapeFrustum.HelperBasisOffset);
        Assert.Equal(0x00A14440u, LandscapeFrustum.HelperNormalize);
        Assert.Equal(0x00B314E0u, LandscapeFrustum.CameraUpdate);
        Assert.Equal(right.X * cotH, view.M11, 4);
        Assert.Equal(lookN.X * cotV, view.M12, 4);
        Assert.Equal(upN.X, view.M13, 4);
        var inventedLookAt = Matrix4x4.CreateLookAt(cam, cam + lookDir, Vector3.UnitZ);
        Assert.True(MathF.Abs(inventedLookAt.M12 - view.M12) > 0.05f,
            "CreateLookAt Y axis is up, helper Y axis is look");

        var invented = Matrix4x4.CreatePerspectiveFieldOfView(
            float.DegreesToRadians(72f), 16f / 9f, 0.15f, 7000f);
        var firstSeenWide = FlyCamera.ProjectionMatrix(16f / 9f, 72f);
        Assert.NotEqual(invented.M11, firstSeenWide.M11);
        Assert.NotEqual(invented.M33, firstSeenWide.M33);
    }

    [Fact]
    public void First_seen_landscape_opos_subtracts_default_zero_c4_not_inverse_row2()
    {
        Assert.True(LandscapeTextures.FirstSeenOPosSubtractsC4);
        Assert.False(LandscapeTextures.FirstSeenUploadsC4InverseRow2OnLandscape);
        Assert.Equal(Vector4.Zero, LandscapeTextures.FirstSeenC4);
        Assert.Equal(4, LandscapeFrustum.InverseRow2Register);
        LandscapeFrustum.LetterboxCots(
            float.DegreesToRadians(72f), 4f, 3f, out var cotH, out var cotV);
        LandscapeFrustum.CotScaledInverse(
            new Vector3(40f, 130.5f, 16.8f), new Vector3(0f, 1f, 0f), Vector3.UnitZ,
            cotH, cotV, out _, out _, out var row2);
        Assert.True(row2.LengthSquared() > 0.01f, $"row2={row2}");
        var pos = new Vector3(34f, 129f, 14f);
        var seen = LandscapeTextures.LandscapeOPosPosition(
            pos, LandscapeTextures.FirstSeenC4, WorldShading.FirstSeenC0.Y);
        var invented = LandscapeTextures.LandscapeOPosPosition(
            pos, row2, WorldShading.FirstSeenC0.Y);
        Assert.Equal(new Vector4(pos, 1f), seen);
        Assert.NotEqual(seen, invented);
    }

    [Fact]
    public void First_seen_view_is_camera_plus128_and_landscape_world_is_camera_translation()
    {
        Assert.Equal(128u, LandscapeFrustum.ViewSourceOffset);
        Assert.Equal(0x00B23B50u, LandscapeFrustum.BindCameraUpdate);
        Assert.Equal(1, LandscapeFrustum.BindCameraUpdateArg);
        Assert.Equal(0, LandscapeFrustum.PrePassCameraUpdateArg);
        Assert.Equal(0x00B2FC50u, LandscapeFrustum.ExtractOtherWritesView);
        Assert.Equal(0x00B66A07u, LandscapeFrustum.ExtractOtherSkyCaller);
        Assert.Equal(0x01436EA0u, LandscapeFrustum.CameraObject);
        Assert.Equal(84, LandscapeFrustum.CameraWorldXOffset);
        Assert.Equal(88, LandscapeFrustum.CameraWorldYOffset);
        Assert.Equal(92, LandscapeFrustum.CameraWorldZOffset);
        Assert.Equal(144, LandscapeFrustum.PerCellWorldStack);
        Assert.Equal(0x00BF46A2u, LandscapeFrustum.PerCellWorldFill);
        Assert.True(LandscapeFrustum.FirstSeenLandscapeWorldIsCameraTranslation);
        Assert.True(LandscapeFrustum.FirstSeenBindWritesViewFromCamera128);
        var cam = new Vector3(40.03f, 130.48f, 16.78f);
        LandscapeFrustum.LandscapeWorld3x4(cam, out var c0, out var c1, out var c2, out var c3);
        Assert.Equal(Vector3.UnitX, c0);
        Assert.Equal(Vector3.UnitY, c1);
        Assert.Equal(Vector3.UnitZ, c2);
        Assert.Equal(cam, c3);
        Assert.True(LandscapeFrustum.FirstSeenWvpIsWorldViewProj);
        Assert.Equal(LandscapeFrustum.LandscapeWorld(cam), Matrix4x4.CreateTranslation(cam));
        Assert.Equal(Matrix4x4.Identity, LandscapeFrustum.IdentityWorld());
    }

    [Fact]
    public void First_seen_wvp_splits_cot_view_and_landscape_t_cam()
    {
        var cam = new Vector3(40.033936f, 130.47711f, 16.78288f);
        var look = new Vector3(34.397583f, 135.84541f, 14.935481f);
        var house = new Vector3(34f, 129f, 14f);
        var camera = new FlyCamera { Position = cam, FovDegrees = 72f };
        camera.LookAt(look);

        Assert.True(LandscapeFrustum.FirstSeenWvpIsWorldViewProj);
        Assert.True(LandscapeFrustum.FirstSeenProjXyIsIdentity);
        Assert.True(LandscapeFrustum.FirstSeenView128IsCotScaled);
        Assert.True(LandscapeFrustum.FirstSeenLandscapeWorldIsCameraTranslation);

        var prop = camera.ViewProjection(4f / 3f);
        var land = camera.LandscapeViewProjection(4f / 3f);
        var lookNdc = FlyCamera.Project(prop, look);
        Assert.True(lookNdc.W != 0f, $"look W={lookNdc.W}");
        Assert.InRange(lookNdc.X, -0.05f, 0.05f);
        LandscapeFrustum.HelperViewAxes(camera.Forward, Vector3.UnitZ, out _, out var lookN, out var upN);
        var along = Vector3.Dot(look - cam, lookN);
        var height = Vector3.Dot(look - cam, upN);
        Assert.True(MathF.Abs(along) > 1f, $"along={along}");
        Assert.True(MathF.Abs(lookNdc.Y) > 0.2f,
            "helper Y is look, so the look point is not CreateLookAt screen-center");

        var landOfRelative = FlyCamera.Project(land, house - cam);
        var propOfHouse = FlyCamera.Project(prop, house);
        Assert.Equal(propOfHouse.X, landOfRelative.X, 3);
        Assert.Equal(propOfHouse.Y, landOfRelative.Y, 3);
        Assert.Equal(propOfHouse.Z, landOfRelative.Z, 3);

        var landOfHouse = FlyCamera.Project(land, house);
        Assert.True(
            MathF.Abs(landOfHouse.X - propOfHouse.X) > 0.1f ||
            MathF.Abs(landOfHouse.Y - propOfHouse.Y) > 0.1f,
            "landscape T(cam) must not be the shared prop VP");

        LandscapeFrustum.LetterboxCots(
            float.DegreesToRadians(72f), 4f, 3f, out var cotH, out var cotV);
        LandscapeFrustum.ViewportZTerms(
            LandscapeFrustum.FirstSeenNear, LandscapeFrustum.FirstSeenFar,
            LandscapeFrustum.FirstSeenMinZ, LandscapeFrustum.FirstSeenMaxZ,
            out var m33, out var m34);
        var view = LandscapeFrustum.CotScaledView(cam, camera.Forward, Vector3.UnitZ, cotH, cotV);
        var proj = LandscapeFrustum.FirstSeenProjection(m33, m34, LandscapeFrustum.VulkanNdcYSign);
        var composed = LandscapeFrustum.ComposeWvp(
            LandscapeFrustum.IdentityWorld(), view, proj);
        var composedLook = FlyCamera.Project(composed, look);
        Assert.Equal(lookNdc.X, composedLook.X, 4);
        Assert.Equal(lookNdc.Y, composedLook.Y, 4);
    }

    [Fact]
    public void First_seen_sky_wvp_uses_100_10000_and_far_slice()
    {
        LandscapeFrustum.ViewportZTerms(
            SkyPass.FirstSeenNear, SkyPass.FirstSeenFar,
            SkyPass.FirstSeenMinZ, SkyPass.FirstSeenMaxZ,
            out var m33, out var m34);
        Assert.Equal(SkyPass.FirstSeenMinZ, m33 + m34 / SkyPass.FirstSeenNear, 4);
        Assert.Equal(SkyPass.FirstSeenMaxZ, m33 + m34 / SkyPass.FirstSeenFar, 4);
        var sky = FlyCamera.SkyProjectionMatrix(4f / 3f, 72f);
        var world = FlyCamera.ProjectionMatrix(4f / 3f, 72f);
        Assert.NotEqual(world.M33, sky.M33);
        Assert.Equal(100f, SkyPass.FirstSeenNear);
        Assert.Equal(10000f, SkyPass.FirstSeenFar);
        Assert.Equal(0x00B662F0u, SkyPass.Draw);
        Assert.Equal(0x2000u, SkyPass.FirstSeenLayerBit);
        Assert.False(SkyPass.FirstSeenUses400000);
    }

    [Fact]
    public void First_seen_fog_c2_is_linear_view_z_not_inverse_row0()
    {
        var cam = new Vector3(40.033936f, 130.47711f, 16.78288f);
        var look = new Vector3(-0.704544f, 0.6710376f, -0.23092493f);
        var house = new Vector3(34f, 129f, 14f);

        Assert.True(WorldShading.FirstSeenFogC2IsLinearViewZ);
        Assert.True(WorldShading.FirstSeenFogSaturates);
        Assert.False(LandscapeFrustum.FirstSeenUploadsInverseRow0AsC2);
        Assert.Equal(276, WorldShading.FogComputeCameraMatrixOffset);
        Assert.Equal(276, LandscapeFrustum.ViewUnscaledCopyOffset);
        Assert.True(LandscapeFrustum.FirstSeenFogUsesUnscaledView);
        Assert.True(LandscapeFrustum.FirstSeenView128IsCotScaled);
        Assert.Equal(0x00B30B50u, LandscapeFrustum.ViewBuilder);
        Assert.Equal(0x00B47630u, LandscapeFrustum.FogCompute);

        var plane = WorldShading.LinearFogPlane(cam, look);
        var dp = WorldShading.WorldDotFogPlane(house, plane);
        Assert.True(dp < 0f, $"linear dp={dp}");
        Assert.Equal(1f, WorldShading.EvaluateWorldFog(house, cam, look), 3);
        Assert.Equal(1f, WorldShading.EvaluateWorldFog(cam, cam, look), 3);

        LandscapeFrustum.LetterboxCots(
            float.DegreesToRadians(72f), 4f, 3f, out var cotH, out var cotV);
        var inverse = LandscapeFrustum.InverseRow0(cam, look, Vector3.UnitZ, cotH, cotV);
        var inverseDp = WorldShading.WorldDotFogPlane(house, inverse);
        Assert.True(inverseDp > 1f, $"inverse dp={inverseDp}");
        Assert.Equal(0f, WorldShading.SaturateFog(
            WorldShading.EvaluateVertexFog(inverseDp, WorldShading.FirstSeenC0.Y, WorldShading.FogRecordColor.W)), 3);

        Assert.Equal(0f, WorldShading.DirLightNdotL(Vector3.UnitY), 5);
        Assert.Equal(1f, WorldShading.DirLightNdotL(-Vector3.UnitY), 5);
        Assert.Equal(0f, WorldShading.DirLightNdotL(Vector3.UnitZ), 5);
        Assert.True(WorldShading.FirstSeenDirLightAddsC3);
        Assert.Equal(new Vector4(0f, 0.125f, 0f, 0f), WorldShading.FirstSeenC3);
        Assert.Equal(0x0139C614u, WorldShading.C3LightingTable);
        Assert.Equal(new Vector3(0.25f, 0.375f, 0.25f), WorldShading.EvaluateDirLightRgb(-Vector3.UnitY));
        Assert.Equal(new Vector3(0f, 0.125f, 0f), WorldShading.EvaluateDirLightRgb(Vector3.UnitZ));
        Assert.Equal(1f, WorldShading.SaturateFog(2f));
        Assert.Equal(0f, WorldShading.SaturateFog(-1f));
    }
}
