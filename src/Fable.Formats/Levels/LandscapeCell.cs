using System.Numerics;
using Fable.Formats.Meshes;

namespace Fable.Formats.Levels;

/// <summary>
/// One native 16 m landscape cell. <c>00BF4570</c>
/// draws stored tessellation (VB +56 stride 24,
/// IB +52) as one DIP. Host keeps the unwound
/// faces; that is a backend translation of the
/// stored strip, not <c>TessellatePrimary</c>.
/// </summary>
public readonly record struct LandscapeCell(
    string Map,
    int CellX,
    int CellY,
    Vector3 Min,
    Vector3 Max,
    IReadOnlyList<MeshTriangle> Faces,
    int TextureId,
    int TextureId1);

public static class LandscapeCells
{
    public const int NativeStrideBytes = 16;
    public const int RecordBytes = 72;
    public const int GpuVertexStride = 24;
    public const uint LayerBackground = 0x4;
    public const uint LayerForeground = 0x40;
    public const uint SubmitFn = 0x00BF4570;
    public const uint PatchAabbFn = 0x00BDC2D0;
}