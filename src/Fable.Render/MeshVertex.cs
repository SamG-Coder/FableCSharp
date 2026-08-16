using System.Numerics;
using System.Runtime.InteropServices;

namespace Fable.Render;

[StructLayout(LayoutKind.Sequential)]
public readonly struct MeshVertex(Vector3 position, Vector3 normal)
{
    public readonly Vector3 Position = position;
    public readonly Vector3 Normal = normal;

    public const uint Stride = 24;
}
