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
}
