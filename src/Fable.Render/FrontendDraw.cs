using System.Numerics;
using System.Runtime.InteropServices;

namespace Fable.Render;

/// <summary>
/// One GPU frontend primitive the host submits.
/// Dest pixels come from the type <c>0x22</c>
/// record (<c>0041BEB0</c> rec+12 /
/// <c>00BAD8A0</c> instance+72). This is
/// not a CPU RGBA composite.
/// </summary>
public readonly record struct FrontendDraw(
    int TextureId,
    uint FirstVertex,
    uint VertexCount,
    uint FirstIndex,
    uint IndexCount,
    int D3dSrcBlend,
    int D3dDestBlend,
    bool BlendEnable,
    int D3dPrimitiveType);

/// <summary>
/// Native dest+UV record after layout.
/// Type <c>0x22</c> sprites: dest rec+12 /
/// instance+72, stride 32, DIPUP prim 4.
/// Type <c>0x27</c> glyphs: 00AB7C20 6×28-byte
/// verts packed into <see cref="FrontendDx9Vertex"/>
/// (28 used, 4 pad).
/// </summary>
public readonly record struct FrontendDx9DrawRecord(
    float DestX0,
    float DestY0,
    float DestX1,
    float DestY1,
    float U0,
    float V0,
    float U1,
    float V1,
    uint DiffuseArgb,
    int TextureId,
    int HandlerBlendMode,
    int RecordType = 0x22,
    int VertexStride = 32,
    int NativeUsedBytes = 28,
    bool AppliesHalfPixel = false);

/// <summary>
/// Display VB stride <c>32</c>
/// (<c>009DA9F0</c> <c>push 32</c>,
/// <c>00BAE2D0</c> <c>push 32</c> into
/// <c>00A0AEA0</c>).
/// <c>VSHADER_2D_SPRITE</c>:
/// <c>mov oPos, v0</c> / <c>mov oD0, v1</c>
/// / <c>mov oT0, v2</c>. That is
/// XYZRHW + DIFFUSE + TEX1 (FVF
/// <c>0x144</c> is 28 bytes; the extra
/// 4 is the recovered stride pad).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct FrontendDx9Vertex(
    float x, float y, float z, float rhw,
    uint diffuseArgb, float u, float v)
{
    public readonly float X = x;
    public readonly float Y = y;
    public readonly float Z = z;
    public readonly float Rhw = rhw;
    public readonly uint DiffuseArgb = diffuseArgb;
    public readonly float U = u;
    public readonly float V = v;
    public readonly float Pad = 0f;

    public const uint Stride = 32;
    public const uint PositionOffset = 0;
    public const uint RhwOffset = 12;
    public const uint DiffuseOffset = 16;
    public const uint UvOffset = 20;
    public const uint NativeUsedBytes = 28;
    public const uint FvfXyzRhwDiffuseTex1 = 0x144;
}

/// <summary>
/// Vulkan-ready 2D vertex after
/// <see cref="Parity.Dx9Vulkan.Dx9VulkanFrontend"/>.
/// <c>Position.xy</c> is Vulkan NDC.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct FrontendGpuVertex(
    Vector4 position, Vector4 color, Vector2 uv, float useDiffuseColor)
{
    public readonly Vector4 Position = position;
    public readonly Vector4 Color = color;
    public readonly Vector2 Uv = uv;
    public readonly float UseDiffuseColor = useDiffuseColor;

    public const uint Stride = 44;
    public const uint ColorOffset = 16;
    public const uint UvOffset = 32;
    public const uint UseDiffuseColorOffset = 40;
}

/// <summary>
/// Present payload for <c>009BEEB0</c>.
/// Replaces <c>FrontendRgba</c> /
/// <c>SetVideoFrame</c>. CPU blit dump is
/// not this path.
/// </summary>
public readonly record struct FrontendSubmitBatch(
    FrontendGpuVertex[] Vertices,
    ushort[] Indices,
    FrontendDraw[] Draws,
    GpuTexture[] Textures,
    int ViewportX,
    int ViewportY,
    int ViewportWidth,
    int ViewportHeight,
    float MinZ,
    float MaxZ)
{
    public bool IsEmpty => Draws.Length == 0 || Vertices.Length == 0;

    public static FrontendSubmitBatch Empty { get; } = new(
        [], [], [], [], 0, 0, 0, 0, 0f, 1f);
}
