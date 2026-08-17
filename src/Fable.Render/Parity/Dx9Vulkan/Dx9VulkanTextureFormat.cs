using Fable.Formats.Textures;
using Silk.NET.Vulkan;

namespace Fable.Render.Parity.Dx9Vulkan;

/// <summary>
/// First-seen CreateTexture uses DXT FourCC in
/// D3DPOOL_SCRATCH. The host decodes the top mip to
/// RGBA8 UNORM. Sampled GPU format after SCRATCH is
/// UNREAD.
/// </summary>
public static class Dx9VulkanTextureFormat
{
    // Fable DX9:
    // 009BE800 / 009BE830 / 009BE870 / 009BE8B0
    // CreateTexture FourCC DXT1/DXT3, pool 3
    // (D3DPOOL_SCRATCH), usage 0. Format 31=DXT1,
    // 32=DXT5, 35=DXT5 16-byte (sky). Type 4 /
    // format 1 = RGBA8.
    //
    // Vulkan:
    // CPU Dxt.Decode → R8G8B8A8_UNORM, mip 0 only.
    //
    // Difference:
    // SCRATCH is a system-memory surface. The format
    // the device later samples is UNREAD. Decode of
    // the top-mip blocks is the proven asset
    // interpretation (TextureFormatTests).
    //
    // Proof:
    // TextureFile.CreateTextureDxt1 009BE8B0,
    // FirstSeenCreateTextureUsesDxtFourCc,
    // FirstSeenTextureStoresRawLowerMips.
    public static Format SampledFormat => Format.R8G8B8A8Unorm;

    // Fable DX9: UNREAD (sRGB vs linear on the
    // sampled view after SCRATCH)
    // Current Vulkan: UNORM (not SRGB)
    // Status: TEMPORARY — NOT PARITY PROVEN
    public const bool TreatAsSrgb = false;

    public static int ChannelCount(TextureCompression compression) =>
        compression is TextureCompression.Dxt1 or TextureCompression.Dxt3
            or TextureCompression.Dxt5 or TextureCompression.Rgba8
            ? 4
            : 0;
}
