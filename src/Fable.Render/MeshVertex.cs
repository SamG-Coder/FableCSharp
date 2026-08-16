using System.Numerics;
using System.Runtime.InteropServices;

namespace Fable.Render;

[StructLayout(LayoutKind.Sequential)]
public readonly struct MeshVertex(Vector3 position, Vector3 normal, Vector3 color)
{
    public readonly Vector3 Position = position;
    public readonly Vector3 Normal = normal;
    public readonly Vector3 Color = color;

    public const uint Stride = 36;
}
