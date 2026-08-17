using Fable.Formats;
using Fable.Formats.Meshes;
using Silk.NET.Vulkan;

namespace Fable.Render.Parity.Dx9Vulkan;

/// <summary>
/// Host <see cref="MeshVertex"/> is the decoded C3D /
/// landscape stream, not a silent modern remap.
/// </summary>
public static class Dx9VulkanVertexFormat
{
    // Fable DX9:
    // Static-lit 00BB2540 FVF 0x112 (XYZ|NORMAL|TEX1)
    // stride 32. Landscape GPU expand 00BFE050 stride
    // 24: u16 X/Y, f32 Z, float3 normal, D3DCOLOR
    // extra at +20. PALSKIN kid 4300: flags 0x14
    // stride 28 = float3 pos + D3DCOLOR v1 indices +
    // D3DCOLOR v2 weights + packed n + packed UV.
    // VS: mul v1.zyxw * c1; mov r3, v2.zyxw; dp3 v3;
    // mov oT0, v4. .zyxw of D3DCOLOR is memory BGRA.
    // Indices are uint16. C3D UV packed int16/2048-8.
    //
    // Vulkan:
    // MeshVertex: float3 pos, float3 normal, float2 uv,
    // float4 color, float3 extra. Triangle list
    // (strips unwound with odd-index swap). First-seen
    // PALSKIN is CPU-skinned with dest = S*C3D
    // (bind-pose identity) including the VS dp3 normal.
    //
    // Difference:
    // Interleaved host layout. Semantics match the
    // decoded streams (pos/normal/UV/extra). Color.rgb
    // is unused by first-seen VS oD0 (lighting).
    //
    // Proof:
    // WorldShading.FirstSeenStaticFvf / StrideBytes,
    // FirstSeenPalskinStrideBytes / InitFlags,
    // LandscapeTextures.GpuVertexStrideBytes,
    // MeshFile.DecompressUv, PackedDirection.Unpack.
    public const uint HostStride = MeshVertex.Stride;
    public const int FirstSeenPalskinStride = WorldShading.FirstSeenPalskinStrideBytes;
    public const uint FirstSeenPalskinInitFlags = WorldShading.FirstSeenPalskinInitFlags;

    public static Format Position => Format.R32G32B32Sfloat;
    public static Format Normal => Format.R32G32B32Sfloat;
    public static Format Uv => Format.R32G32Sfloat;
    public static Format Color => Format.R32G32B32A32Sfloat;
    public static Format Extra => Format.R32G32B32Sfloat;

    public static int PackedUvOffset(int entryType, int stride, uint initFlags, bool hasBones) =>
        MeshFile.PackedUvOffset(entryType, stride, initFlags, hasBones);

    public static int PackedNormalOffset(int entryType, int stride, uint initFlags, bool hasBones) =>
        MeshFile.PackedNormalOffset(entryType, stride, initFlags, hasBones);

    public static int PalskinBlendIndexOffset(int entryType, int stride, uint initFlags, bool hasBones) =>
        MeshFile.PalskinBlendIndexOffset(entryType, stride, initFlags, hasBones);

    public static int PalskinBlendWeightOffset(int entryType, int stride, uint initFlags, bool hasBones) =>
        MeshFile.PalskinBlendWeightOffset(entryType, stride, initFlags, hasBones);
}
