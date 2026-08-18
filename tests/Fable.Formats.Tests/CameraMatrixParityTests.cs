using System.Numerics;
using Fable.Formats.Levels;
using Fable.Game;

namespace Fable.Formats.Tests;

/// <summary>
/// Native WVP numbers after the helper-bind
/// re-audit. Algebra is <c>00B314E0</c> /
/// <c>00B30B50</c> / <c>00988A50</c>. Pose
/// inputs are the recovered PE literals,
/// not an invented flip.
/// </summary>
public sealed class CameraMatrixParityTests
{
    private const float NativeWidth = 1024f;
    private const float NativeHeight = 768f;

    [Fact]
    public void GameCamera_ctor_look_default_is_plus_z()
    {
        Assert.Equal(0x00A0C130u, GameCamera.HelperPackFn);
        Assert.Equal(4, GameCamera.HelperOffset);
        Assert.Equal(Vector3.Zero, GameCamera.CtorPos);
        Assert.Equal(Vector3.UnitZ, GameCamera.CtorLook);
        Assert.Equal(new Vector3(1f, 1f, 1f), GameCamera.CtorUp);
        Assert.Equal(0x3E471B48u, GameCamera.FirstSeenFovTurnsBits);
        var turns = BitConverter.UInt32BitsToSingle(GameCamera.FirstSeenFovTurnsBits);
        Assert.InRange(turns * LandscapeFrustum.FovTurnsToDegrees, 69.99f, 70.01f);
        Assert.Equal(70f, GameCamera.FirstSeenFovDegrees);
        Assert.Equal(Vector3.Zero, GameCameraManager.CtorPos);
        Assert.Equal(Vector3.UnitZ, GameCameraManager.CtorLook);
        Assert.Equal(Vector3.UnitX, GameCameraManager.CtorUp);
        Assert.Equal(0x00A0C130u, GameCameraManager.HelperPackFn);
    }

    [Fact]
    public void WorldCamera_tail_is_colour_filter_not_helper()
    {
        Assert.Equal(0x006B42F0u, WorldCamera.BlendFn);
        Assert.Equal(0x008857E0u, WorldCamera.BankLerpFn);
        Assert.Equal(0x00885900u, WorldCamera.BankWeightFn);
        Assert.Equal(0x008859F0u, WorldCamera.BankPacketFn);
        Assert.Equal(0x00B23EC0u, WorldCamera.EngineApplyVtblFn);
        Assert.Equal(244, WorldCamera.EngineApplyVtblOffset);
        Assert.Equal(0x01436E40u, WorldCamera.ColourFilterObject);
        Assert.Equal(16, WorldCamera.ColourFilterPacketOffset);
        Assert.Equal(12, WorldCamera.ColourFilterSkipOffset);
        Assert.True(WorldCamera.IsCtorAxis(new Vector3(1f, 0f, 0f)));
        Assert.False(WorldCamera.IsCtorAxis(GameCamera.CtorLook));
    }

    [Fact]
    public void Letterbox_at_1024x768_is_identity_scale()
    {
        Assert.Equal(1024, EngineLifecycle.DisplayDefaultWidth);
        Assert.Equal(768, EngineLifecycle.DisplayDefaultHeight);
        Assert.Equal(0.75f, LandscapeFrustum.LetterboxFourByThree);
        var hOverW = NativeHeight / NativeWidth;
        var letterbox = (LandscapeFrustum.LetterboxFourByThree - hOverW)
            * LandscapeFrustum.FovHalfScale + 1f;
        Assert.Equal(1f, letterbox, 5);
        LandscapeFrustum.LetterboxCots(
            float.DegreesToRadians(GameCamera.FirstSeenFovDegrees),
            NativeWidth, NativeHeight, out var cotH, out var cotV);
        var expectedCotH = 1f / MathF.Tan(float.DegreesToRadians(35f));
        Assert.Equal(expectedCotH, cotH, 5);
        Assert.Equal(cotH * (NativeWidth / NativeHeight), cotV, 5);
        LandscapeFrustum.LetterboxCots(
            float.DegreesToRadians(GameCamera.FirstSeenFovDegrees),
            1600f, 900f, out var wideH, out var wideV);
        Assert.True(MathF.Abs(wideH - cotH) > 0.01f,
            "16:9 letterbox must not be used as native FOV");
        Assert.True(MathF.Abs(wideV - cotV) > 0.01f);
    }

    [Fact]
    public void Projection_numbers_match_009883F0()
    {
        LandscapeFrustum.ViewportZTerms(
            LandscapeFrustum.FirstSeenNear,
            LandscapeFrustum.FirstSeenFar,
            LandscapeFrustum.FirstSeenMinZ,
            LandscapeFrustum.FirstSeenMaxZ,
            out var m33, out var q);
        const float near = 0.1f;
        const float far = 4000f;
        const float minZ = 0.1f;
        const float maxZ = 0.99f;
        var expectedQ = ((minZ - maxZ) * near * far) / (far - near);
        var expectedM33 = minZ - expectedQ / near;
        Assert.Equal(expectedQ, q, 5);
        Assert.Equal(expectedM33, m33, 5);
        Assert.Equal(-0.08900f, q, 4);
        Assert.Equal(0.99002f, m33, 4);
        var p = LandscapeFrustum.FirstSeenDx9Projection(m33, q);
        Assert.Equal(1f, p.M11);
        Assert.Equal(1f, p.M22);
        Assert.Equal(m33, p.M33, 5);
        Assert.Equal(1f, p.M34);
        Assert.Equal(q, p.M43, 5);
        Assert.Equal(0f, p.M44);
        Assert.True(LandscapeFrustum.FirstSeenProjXyIsIdentity);
        Assert.True(LandscapeFrustum.FirstSeenProjWIsViewZ);
    }

    [Fact]
    public void GameCamera_ctor_helper_wvp_at_1024x768()
    {
        LandscapeFrustum.LetterboxCots(
            float.DegreesToRadians(GameCamera.FirstSeenFovDegrees),
            NativeWidth, NativeHeight, out var cotH, out var cotV);
        LandscapeFrustum.HelperViewAxes(
            GameCamera.CtorLook, GameCamera.CtorUp,
            out var right, out var lookN, out var upN);
        Assert.Equal(Vector3.UnitZ, lookN);
        Assert.Equal(1f / MathF.Sqrt(3f), upN.X, 5);
        Assert.Equal(0.70710678f, right.X, 5);
        Assert.Equal(-0.70710678f, right.Y, 5);
        Assert.Equal(0f, right.Z, 5);

        var view = LandscapeFrustum.CotScaledView(
            GameCamera.CtorPos, GameCamera.CtorLook, GameCamera.CtorUp,
            cotH, cotV);
        Assert.Equal(1.0098509f, view.M11, 4);
        Assert.Equal(1.0993701f, view.M12, 4);
        Assert.Equal(0f, view.M13, 4);
        Assert.Equal(-1.0098509f, view.M21, 4);
        Assert.Equal(1.0993701f, view.M22, 4);
        Assert.Equal(0f, view.M23, 4);
        Assert.Equal(0f, view.M31, 4);
        Assert.Equal(1.0993701f, view.M32, 4);
        Assert.Equal(1f, view.M33, 4);
        Assert.Equal(0f, view.M41, 4);
        Assert.Equal(0f, view.M42, 4);
        Assert.Equal(0f, view.M43, 4);
        Assert.Equal(1f, view.M44, 4);

        LandscapeFrustum.ViewportZTerms(
            LandscapeFrustum.FirstSeenNear,
            LandscapeFrustum.FirstSeenFar,
            LandscapeFrustum.FirstSeenMinZ,
            LandscapeFrustum.FirstSeenMaxZ,
            out var m33, out var q);
        var proj = LandscapeFrustum.FirstSeenDx9Projection(m33, q);
        var world = LandscapeFrustum.IdentityWorld();
        Assert.Equal(Matrix4x4.Identity, world);
        var wvp = LandscapeFrustum.ComposeWvp(world, view, proj);
        var alongLook = Vector4.Transform(new Vector4(0f, 0f, 1f, 1f), wvp);
        Assert.Equal(0f, alongLook.X, 4);
        Assert.Equal(1.0993701f, alongLook.Y, 4);
        Assert.Equal(m33 + q, alongLook.Z, 4);
        Assert.Equal(1f, alongLook.W, 4);
        Assert.True(MathF.Abs(alongLook.Y) > 0.2f,
            "ctor up (1,1,1) keeps look off screen-center");
    }

    [Fact]
    public void Host_helper_bind_wvp_at_origin_looks_neg_x()
    {
        var cam = new ScriptedCamera();
        cam.ApplyRendererHelper(
            Vector3.Zero, -Vector3.UnitX, LandscapeFrustum.FirstSeenCameraUp);
        cam.SetFovDegrees(GameCamera.FirstSeenFovDegrees);
        Assert.Equal(Vector3.Zero, cam.Position);
        Assert.True((cam.Forward + Vector3.UnitX).Length() < 1e-5f);
        Assert.Equal(LandscapeFrustum.FirstSeenCameraUp, cam.Up);
        Assert.Equal(70f, cam.FovDegrees);

        var aspect = NativeWidth / NativeHeight;
        LandscapeFrustum.LetterboxCots(
            float.DegreesToRadians(70f), NativeWidth, NativeHeight,
            out var cotH, out var cotV);
        var view = cam.ViewMatrixAt(aspect);
        // right = up × look = (0,0,1)×(-1,0,0) = (0,-1,0)
        Assert.Equal(0f, view.M11, 5);
        Assert.Equal(0f, view.M12, 5);
        Assert.Equal(-1f, view.M13, 5);
        Assert.Equal(-cotH, view.M21, 4);
        Assert.Equal(0f, view.M22, 5);
        Assert.Equal(0f, view.M23, 5);
        Assert.Equal(0f, view.M31, 5);
        Assert.Equal(cotV, view.M32, 4);
        Assert.Equal(0f, view.M33, 5);

        var wvp = cam.ViewProjection(aspect);
        var along = Vector4.Transform(new Vector4(-1f, 0f, 0f, 1f), wvp);
        Assert.Equal(0f, along.X, 4);
        Assert.Equal(0f, along.Y, 4);
        Assert.True(along.W > 0f);
        var hostLand = cam.HostLandscapeViewProjection(aspect);
        Assert.Equal(wvp, hostLand);
    }

    [Fact]
    public void Viewport_depth_and_near_far_stay_dx9()
    {
        Assert.Equal(0.1f, LandscapeFrustum.FirstSeenNear);
        Assert.Equal(4000f, LandscapeFrustum.FirstSeenFar);
        Assert.Equal(0.1f, LandscapeFrustum.FirstSeenMinZ);
        Assert.Equal(0.99f, LandscapeFrustum.FirstSeenMaxZ);
        Assert.Equal(0x009BEF80u, EngineLifecycle.SetViewportFn);
        Assert.Equal(188, EngineLifecycle.SetViewportVtbl);
        Assert.Equal(0f, EngineLifecycle.ViewportMinZ);
        Assert.Equal(1f, EngineLifecycle.ViewportMaxZ);
        Assert.Equal(176, LandscapeFrustum.ViewportWidthOffset);
        Assert.Equal(180, LandscapeFrustum.ViewportHeightOffset);
        Assert.Equal(1f, LandscapeFrustum.Dx9ProjectionYSign);
    }

    [Fact]
    public void Interpolation_first_seen_is_identity_when_slots_match()
    {
        var world = new WorldCamera();
        world.Construct();
        world.SeedHero();
        var t0 = world.Blend(0f);
        var t1 = world.Blend(1f);
        Assert.True(WorldCamera.IsCtorAxis(t0.V0));
        Assert.Equal(t0.V0, t1.V0);
        Assert.Equal(t0.V1, t1.V1);
        var mid = world.Blend(0.5f);
        Assert.True(WorldCamera.IsCtorAxis(mid.V0));
        Assert.Equal(-Vector3.UnitX, world.SlotA.V4);
    }

    [Fact]
    public void Scripted_bind_survives_until_host_apply_stomps()
    {
        var cam = new ScriptedCamera();
        cam.Bind("CAM_OVIF_SHOT2",
            new Vector3(40f, 130f, 16f),
            new Vector3(34f, 135f, 14f),
            LandscapeFrustum.FirstSeenCameraUp,
            RegionTravel.IntroCameraFovDegrees);
        Assert.True(cam.ScriptCameraActive);
        Assert.Equal("CAM_OVIF_SHOT2", cam.ActiveName);
        Assert.Equal(72f, cam.FovDegrees);
        cam.Reset();
        Assert.False(cam.ScriptCameraActive);
        Assert.Equal("", cam.ActiveName);

        cam.Bind("CAM_OVIF_SHOT2",
            new Vector3(40f, 130f, 16f),
            new Vector3(34f, 135f, 14f),
            LandscapeFrustum.FirstSeenCameraUp, 72f);
        cam.ApplyRendererHelper(
            Vector3.Zero, -Vector3.UnitX, LandscapeFrustum.FirstSeenCameraUp);
        cam.SetFovDegrees(GameCamera.FirstSeenFovDegrees);
        Assert.True(cam.ScriptCameraActive);
        Assert.Equal(Vector3.Zero, cam.Position);
        Assert.Equal(70f, cam.FovDegrees);
    }
}
