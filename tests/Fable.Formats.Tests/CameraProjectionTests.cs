using System.Numerics;
using Fable.Formats.Levels;
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
        Assert.InRange(ndc.Y, -0.75f, 0.75f);
        Assert.InRange(ndc.Z, 0f, 1f);
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
        Assert.True(ndc.W > 0f);
        Assert.InRange(ndc.X, -1f, 1f);
        Assert.InRange(ndc.Y, -1f, 1f);
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
        Assert.True(LandscapeFrustum.FirstSeenUploadsInverseRow0AsC2);
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
        Assert.Equal(cotH, proj.M11, 4);
        Assert.Equal(-cotV, proj.M22, 4);
        Assert.Equal(-m33, proj.M33, 4);
        Assert.Equal(-1f, proj.M34, 4);
        Assert.Equal(m34, proj.M43, 4);

        var invented = Matrix4x4.CreatePerspectiveFieldOfView(
            float.DegreesToRadians(72f), 16f / 9f, 0.15f, 7000f);
        var firstSeenWide = FlyCamera.ProjectionMatrix(16f / 9f, 72f);
        Assert.NotEqual(invented.M11, firstSeenWide.M11);
        Assert.NotEqual(invented.M33, firstSeenWide.M33);
    }
}
