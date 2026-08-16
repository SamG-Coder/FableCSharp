using System.Numerics;

namespace Fable.Render;

/// <summary>Z-up fly camera matching Fable world space (X/Y ground, Z height).</summary>
public sealed class FlyCamera
{
    public Vector3 Position;
    public float Yaw;
    public float Pitch;
    public float MoveSpeed = 18f;
    public float LookSpeed = 0.0055f;
    public float FastMultiplier = 4f;

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

    public Matrix4x4 ViewMatrix =>
        Matrix4x4.CreateLookAt(Position, Position + Forward, Vector3.UnitZ);

    public static Matrix4x4 ProjectionMatrix(float aspect, float fovDegrees = 65f)
    {
        var proj = Matrix4x4.CreatePerspectiveFieldOfView(
            float.DegreesToRadians(fovDegrees),
            MathF.Max(aspect, 0.01f),
            0.15f,
            7000f);
        proj.M22 *= -1f; // Vulkan NDC is Y-down
        return proj;
    }

    public Matrix4x4 ViewProjection(float aspect) => ViewMatrix * ProjectionMatrix(aspect);

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
