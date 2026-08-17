using System.Numerics;
using Fable.Formats.Levels;

namespace Fable.Render;

/// <summary>
/// Debug free-fly only. Projection/basis match the scripted
/// camera (<c>LandscapeFrustum</c> first-seen WVP). Must not
/// write game / script camera state. Exiting debug restores
/// the current scripted camera by leaving that object untouched.
/// </summary>
public sealed class FlyCamera
{
    public Vector3 Position;
    public float Yaw;
    public float Pitch;
    public float MoveSpeed = 18f;
    public float LookSpeed = 0.0055f;
    public float FastMultiplier = 4f;
    public float FovDegrees = 65f;

    public Vector3 Forward
    {
        get
        {
            var cp = MathF.Cos(Pitch);
            return Vector3.Normalize(new Vector3(
                MathF.Cos(Yaw) * cp,
                MathF.Sin(Yaw) * cp,
                MathF.Sin(Pitch)));
        }
    }

    public Vector3 Right => Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitZ));

    public Matrix4x4 ViewMatrix => ViewMatrixAt(4f / 3f);

    public Matrix4x4 ViewMatrixAt(float aspect) =>
        CotScaledView(Position, Forward, FovDegrees, aspect);

    public static Matrix4x4 CotScaledView(
        Vector3 position, Vector3 look, float fovDegrees, float aspect)
    {
        aspect = MathF.Max(aspect, 0.01f);
        LandscapeFrustum.LetterboxCots(
            float.DegreesToRadians(fovDegrees), aspect, 1f,
            out var cotH, out var cotV);
        return LandscapeFrustum.CotScaledView(position, look, Vector3.UnitZ, cotH, cotV);
    }

    public static Matrix4x4 ProjectionMatrix(float aspect, float fovDegrees = 65f)
    {
        _ = aspect;
        _ = fovDegrees;
        LandscapeFrustum.ViewportZTerms(
            LandscapeFrustum.FirstSeenNear,
            LandscapeFrustum.FirstSeenFar,
            LandscapeFrustum.FirstSeenMinZ,
            LandscapeFrustum.FirstSeenMaxZ,
            out var m33, out var m34);
        // 009883F0 is XY identity (M22=+1). Cot is on camera+128.
        // clip.w = view.z. Vulkan NDC Y is applied in Draw.
        return LandscapeFrustum.FirstSeenDx9Projection(m33, m34);
    }

    public Matrix4x4 ViewProjection(float aspect) =>
        LandscapeFrustum.ComposeWvp(
            LandscapeFrustum.IdentityWorld(), ViewMatrixAt(aspect), ProjectionMatrix(aspect, FovDegrees));

    public Matrix4x4 LandscapeViewProjection(float aspect) =>
        LandscapeFrustum.ComposeWvp(
            LandscapeFrustum.LandscapeWorld(Position), ViewMatrixAt(aspect), ProjectionMatrix(aspect, FovDegrees));

    /// <summary>
    /// <c>00B662F0</c> else-path <c>00B30B50</c> with sky source
    /// near 100 / far 10000 / minZ 0.99 / maxZ 1, then
    /// <c>VSHADER_INNER_SKY</c> <c>dp4 oPos, v0, c5–c8</c>.
    /// </summary>
    public static Matrix4x4 SkyProjectionMatrix(float aspect, float fovDegrees = 65f)
    {
        _ = aspect;
        _ = fovDegrees;
        LandscapeFrustum.ViewportZTerms(
            Fable.Formats.Sky.SkyPass.FirstSeenNear,
            Fable.Formats.Sky.SkyPass.FirstSeenFar,
            Fable.Formats.Sky.SkyPass.FirstSeenMinZ,
            Fable.Formats.Sky.SkyPass.FirstSeenMaxZ,
            out var m33, out var m34);
        return LandscapeFrustum.FirstSeenDx9Projection(m33, m34);
    }

    public Matrix4x4 SkyViewProjection(float aspect) =>
        LandscapeFrustum.ComposeWvp(
            LandscapeFrustum.IdentityWorld(), ViewMatrixAt(aspect), SkyProjectionMatrix(aspect, FovDegrees));

    public void LookAt(Vector3 target)
    {
        var dir = target - Position;
        if (dir.LengthSquared() < 1e-8f)
            return;
        dir = Vector3.Normalize(dir);
        Pitch = Math.Clamp(MathF.Asin(Math.Clamp(dir.Z, -1f, 1f)), -1.45f, 1.45f);
        Yaw = MathF.Atan2(dir.Y, dir.X);
    }

    public static Vector4 Project(Matrix4x4 viewProjection, Vector3 world)
    {
        var clip = Vector4.Transform(new Vector4(world, 1f), viewProjection);
        if (MathF.Abs(clip.W) < 1e-8f)
            return clip;
        return new Vector4(clip.X / clip.W, clip.Y / clip.W, clip.Z / clip.W, clip.W);
    }

    public void Look(float dx, float dy)
    {
        Yaw += dx * LookSpeed;
        Pitch = Math.Clamp(Pitch - dy * LookSpeed, -1.45f, 1.45f);
    }

    public void Move(Vector3 local, float dt, bool fast)
    {
        var speed = MoveSpeed * (fast ? FastMultiplier : 1f);
        var groundForward = Vector3.Normalize(new Vector3(Forward.X, Forward.Y, 0));
        if (groundForward.LengthSquared() < 0.0001f)
            groundForward = Vector3.UnitY;
        var groundRight = Vector3.Normalize(new Vector3(Right.X, Right.Y, 0));
        Position += (groundForward * local.Y + groundRight * local.X + Vector3.UnitZ * local.Z) * speed * dt;
    }
}
