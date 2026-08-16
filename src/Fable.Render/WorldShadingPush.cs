using System.Numerics;
using System.Runtime.InteropServices;

namespace Fable.Render;

[StructLayout(LayoutKind.Sequential)]
public struct MeshPushConstants
{
    public Matrix4x4 ViewProj;
    public Vector4 CameraPos;
    public Vector4 FogColor;
    public Vector4 LightDir;

    public const uint Size = 112;
}
