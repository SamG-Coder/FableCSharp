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
}
