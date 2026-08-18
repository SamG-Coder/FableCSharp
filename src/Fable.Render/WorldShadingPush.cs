using System.Numerics;
using System.Runtime.InteropServices;

namespace Fable.Render;

[StructLayout(LayoutKind.Sequential)]
public struct MeshPushConstants
{
    public Matrix4x4 ViewProj;
    public Vector4 CameraPos;
    public Vector4 LightDir;
    public Vector4 LightColor;
    public Vector4 Pass;

    public const uint Size = 128;

    public static Vector4 PackPass(float mode)
    {
        var lit = Fable.Formats.WorldShading.LitColor;
        return new Vector4(mode, lit.X, lit.Y, lit.Z);
    }

    /// <summary>
    /// <c>p * W * V * P</c>. Host Numerics is
    /// row-vector; <paramref name="viewProj"/>
    /// is already the Vulkan-flipped VP.
    /// </summary>
    public static Matrix4x4 WorldViewProj(Matrix4x4 world, Matrix4x4 viewProj) =>
        world * viewProj;

    /// <summary>
    /// Row-vector plane so
    /// <c>dot(local, plane') = dot(local*W, plane)</c>.
    /// </summary>
    public static Vector4 TransformPlane(Matrix4x4 world, Vector4 plane)
    {
        var n = new Vector3(plane.X, plane.Y, plane.Z);
        var t = world.Translation;
        var nLocal = Vector3.TransformNormal(n, Matrix4x4.Transpose(world));
        return new Vector4(nLocal, plane.W + Vector3.Dot(t, n));
    }

    public static Vector3 TransformLightDir(Matrix4x4 world, Vector3 dir)
    {
        if (!Matrix4x4.Invert(world, out var inv))
            return dir;
        return Vector3.Normalize(Vector3.TransformNormal(dir, Matrix4x4.Transpose(inv)));
    }
}
