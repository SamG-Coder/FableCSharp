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
    public bool ScriptCameraActive { get; private set; }
    /// <summary>
    /// <c>vtbl+1672</c>: true while a spline / path /
    /// orbit is playing. Snap <c>UseCamera</c> bind
    /// arrives immediately so this stays false.
    /// </summary>
    public bool Playing { get; private set; }

    private string _gameplayName = "";
    private Vector3 _gameplayPos;
    private Vector3 _gameplayLook;
    private Vector3 _gameplayUp = LandscapeFrustum.FirstSeenCameraUp;
    private float _gameplayFov = RegionTravel.IntroCameraFovDegrees;
    private bool _hasGameplay;

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
        if (!_hasGameplay)
            SnapshotGameplay();
        ActiveName = name;
        Position = position;
        LookAt = lookAt;
        Up = up.LengthSquared() > 1e-8f ? Vector3.Normalize(up) : LandscapeFrustum.FirstSeenCameraUp;
        FovDegrees = fovDegrees;
        ScriptCameraActive = true;
        Playing = false;
    }

    /// <summary>
    /// Path / rig / rotate / look-between start a
    /// transition. <c>WaitForCamera</c> leftover-polls
    /// until <see cref="EndTransition"/>.
    /// </summary>
    public void BeginTransition() => Playing = true;

    public void EndTransition() => Playing = false;

    public void SnapshotGameplay()
    {
        _gameplayName = ActiveName;
        _gameplayPos = Position;
        _gameplayLook = LookAt;
        _gameplayUp = Up;
        _gameplayFov = FovDegrees;
        _hasGameplay = true;
    }

    /// <summary>
    /// <c>00CC9DF1</c>: <c>vtbl+1668(0.0)</c> then
    /// <c>vtbl+1664</c>. No yield. Restores the
    /// pre-script gameplay camera snapshot.
    /// </summary>
    public void Reset()
    {
        ScriptCameraActive = false;
        Playing = false;
        if (!_hasGameplay)
        {
            ActiveName = "";
            return;
        }

        ActiveName = _gameplayName;
        Position = _gameplayPos;
        LookAt = _gameplayLook;
        Up = _gameplayUp;
        FovDegrees = _gameplayFov;
    }

    public void SetLookAt(Vector3 lookAt) => LookAt = lookAt;

    public void SetPosition(Vector3 position) => Position = position;

    public void SetUp(Vector3 up) =>
        Up = up.LengthSquared() > 1e-8f ? Vector3.Normalize(up) : LandscapeFrustum.FirstSeenCameraUp;

    public void SetFovDegrees(float fovDegrees) => FovDegrees = fovDegrees;

    /// <summary>
    /// <c>006B42F0</c> writes <c>+6296/+6312/+6328</c>
    /// into the live game camera.
    /// </summary>
    public void ApplyManagerOutput(Vector3 position, Vector3 lookAt, Vector3 up)
    {
        Position = position;
        LookAt = lookAt;
        SetUp(up);
    }

    /// <summary>
    /// <c>00B314E0</c> helper consumed by the
    /// renderer: <c>+0</c> eye, <c>+12</c>
    /// forward (normalised), <c>+24</c> up
    /// (normalised). Not slot axes.
    /// </summary>
    public void ApplyRendererHelper(Vector3 position, Vector3 forward, Vector3 up)
    {
        Position = position;
        var dir = forward.LengthSquared() > 1e-8f ? Vector3.Normalize(forward) : Vector3.UnitY;
        LookAt = position + dir;
        SetUp(up);
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

    /// <summary>
    /// Host STB is world-space. Fable <c>T(cam)</c> is
    /// for a camera-relative VB. This is the equivalent
    /// <c>p_world * I * V * P</c>.
    /// </summary>
    public Matrix4x4 HostLandscapeViewProjection(float aspect) =>
        LandscapeFrustum.ComposeWvp(
            LandscapeFrustum.HostWorldSpaceLandscapeWorld(), FlyView(aspect), WorldProj());

    public Matrix4x4 SkyViewProjection(float aspect)
    {
        LandscapeFrustum.ViewportZTerms(
            SkyPass.FirstSeenNear, SkyPass.FirstSeenFar,
            SkyPass.FirstSeenMinZ, SkyPass.FirstSeenMaxZ,
            out var m33, out var m34);
        var proj = LandscapeFrustum.FirstSeenDx9Projection(m33, m34);
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
        return LandscapeFrustum.FirstSeenDx9Projection(m33, m34);
    }
}
