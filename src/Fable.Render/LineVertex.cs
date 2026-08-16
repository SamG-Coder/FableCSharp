using System.Numerics;
using System.Runtime.InteropServices;

namespace Fable.Render;

[StructLayout(LayoutKind.Sequential)]
public readonly struct LineVertex(Vector3 position, Vector3 color)
{
    public readonly Vector3 Position = position;
    public readonly Vector3 Color = color;

    public const uint Stride = 24;
}
