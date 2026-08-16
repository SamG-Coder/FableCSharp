using System.Numerics;
using Fable.Formats.Levels;
using Fable.Formats.Sky;
using Fable.Formats.Tng;

namespace Fable.Game;

/// <summary>
/// Live New Game camera owned by script / TNG helper state.
/// First-seen bind is TNG <c>CAM_OVIF_SHOT2</c> via
/// <c>00B23B50</c> / <c>00B314E0</c>. Cutscene interpreter
/// <c>00CBFB7D</c> later matches <c>UseCamera</c> at
/// <c>00CC9F3A</c> and binds the named TNG camera through
/// context <c>vtbl+1656</c> (thing) or <c>vtbl+1648</c>
/// (name). Preload <c>00CBF29F</c> only walks those names.
/// This object is the game camera — debug fly must not
/// write it.
/// </summary>
public sealed class ScriptedCamera
{
    public const uint CutsceneStart = 0x00DB86B0;
    public const uint CutsceneDtor = 0x00DB8680;
    public const uint CutsceneRunner = 0x00CBFB7D;
    public const uint UseCameraPreload = 0x00CBF29F;
    public const uint UseCameraActivate = 0x00CC9F3A;
    public const int PreloadVtbl = 1648;
    public const int ActivateVtbl = 1656;
    public const uint CallbackTable = 0x012D838C;
    public const uint LiveFatherVtbl = 0x012D8388;
    public const uint LiveFatherFactory = 0x00DAC2C0;
    public const uint MicrothreadVtbl = 0x012D95B0;
    public const uint MicrothreadPersist = 0x00DB8630;

    public string ActiveName { get; private set; } = "";
    public Vector3 Position { get; private set; }
    public Vector3 LookAt { get; private set; }
    public Vector3 Up { get; private set; } = LandscapeFrustum.FirstSeenCameraUp;
    public float FovDegrees { get; private set; } = RegionTravel.IntroCameraFovDegrees;

    public Vector3 Forward
    {
        get
        {
            var dir = LookAt - Position;
            return dir.LengthSquared() < 1e-8f ? Vector3.UnitY : Vector3.Normalize(dir);
        }
    }

    /// <summary>
    /// <c>UseCamera</c> / first-seen helper bind: store the
    /// named TNG camera. Does not invent fade or spline play.
    /// </summary>
    public void Bind(string name, Vector3 position, Vector3 lookAt, Vector3 up, float fovDegrees)
    {
        ActiveName = name;
        Position = position;
        LookAt = lookAt;
        Up = up.LengthSquared() > 1e-8f ? Vector3.Normalize(up) : LandscapeFrustum.FirstSeenCameraUp;
        FovDegrees = fovDegrees;
    }

    public bool UseCamera(IEnumerable<ThingInstance> things, string name) =>
        RegionTravel.TryNamedCamera(things, name, out var position, out var lookAt, out var fov, out var up)
        && BindAndTrue(name, position, lookAt, up, fov);

    private bool BindAndTrue(string name, Vector3 position, Vector3 lookAt, Vector3 up, float fov)
    {
        Bind(name, position, lookAt, up, fov);
        return true;
    }

    public Matrix4x4 ViewMatrixAt(float aspect) =>
        FlyView(aspect);

    private Matrix4x4 FlyView(float aspect)
    {
        aspect = MathF.Max(aspect, 0.01f);
        LandscapeFrustum.LetterboxCots(
            float.DegreesToRadians(FovDegrees), aspect, 1f,
            out var cotH, out var cotV);
        return LandscapeFrustum.CotScaledView(Position, Forward, Up, cotH, cotV);
    }

    public Matrix4x4 ViewProjection(float aspect) =>
        LandscapeFrustum.ComposeWvp(
            LandscapeFrustum.IdentityWorld(), FlyView(aspect), WorldProj());

    public Matrix4x4 LandscapeViewProjection(float aspect) =>
        LandscapeFrustum.ComposeWvp(
            LandscapeFrustum.LandscapeWorld(Position), FlyView(aspect), WorldProj());

    public Matrix4x4 SkyViewProjection(float aspect)
    {
        LandscapeFrustum.ViewportZTerms(
            SkyPass.FirstSeenNear, SkyPass.FirstSeenFar,
            SkyPass.FirstSeenMinZ, SkyPass.FirstSeenMaxZ,
            out var m33, out var m34);
        var proj = LandscapeFrustum.FirstSeenProjection(
            m33, m34, LandscapeFrustum.VulkanNdcYSign);
        return LandscapeFrustum.ComposeWvp(
            LandscapeFrustum.IdentityWorld(), FlyView(aspect), proj);
    }

    private static Matrix4x4 WorldProj()
    {
        LandscapeFrustum.ViewportZTerms(
            LandscapeFrustum.FirstSeenNear,
            LandscapeFrustum.FirstSeenFar,
            LandscapeFrustum.FirstSeenMinZ,
            LandscapeFrustum.FirstSeenMaxZ,
            out var m33, out var m34);
        return LandscapeFrustum.FirstSeenProjection(
            m33, m34, LandscapeFrustum.VulkanNdcYSign);
    }
}
