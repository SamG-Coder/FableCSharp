using System.Numerics;
using Fable.Formats.Meshes;

namespace Fable.Formats.Levels;

/// <summary>
/// One native 16 m landscape cell. <c>00BF4570</c>
/// walks cell+8 mesh nodes (VB mesh+56 stride 24,
/// IB mesh+52). Host keeps the unwound faces as a
/// backend translation of the stored strip, not
/// <c>TessellatePrimary</c>.
/// </summary>
public readonly record struct LandscapePoint(Vector3 P, Vector3 N, Vector3 Extra);

public readonly record struct LandscapeCell(
    string Map,
    int CellX,
    int CellY,
    Vector3 Min,
    Vector3 Max,
    IReadOnlyList<MeshTriangle> Faces,
    int TextureId,
    int TextureId1,
    IReadOnlyList<LandscapePoint>? Points = null,
    ushort[]? StripIndices = null,
    int PrimitiveCount = 0);

public static class LandscapeCells
{
    public const int NativeStrideBytes = 16;
    public const int RecordBytes = 72;
    public const int GpuVertexStride = 24;
    public const uint LayerBackground = 0x4;
    public const uint LayerForeground = 0x40;
    public const uint SubmitFn = 0x00BF4570;
    public const uint PatchAabbFn = 0x00BDC2D0;
    /// <summary>
    /// <c>00A0AD40</c> → device vtbl+328
    /// <c>DrawIndexedPrimitive</c>. Not +332.
    /// </summary>
    public const int DrawIndexedPrimitiveVtbl = 328;
    /// <summary>IB+12. <c>D3DPT_TRIANGLESTRIP</c>.</summary>
    public const int PrimitiveTypeStrip = 5;
    /// <summary><c>D3DFMT_INDEX16</c>.</summary>
    public const int IndexFormat = 101;
}