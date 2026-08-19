using Silk.NET.Vulkan;

namespace Fable.Render.Parity.Dx9Vulkan;

/// <summary>
/// First-seen SetSamplerState is unread.
/// Live state is the D3D9 device default:
/// POINT / NONE / WRAP.
/// </summary>
public static class Dx9VulkanSamplerState
{
    // Fable DX9: no first-seen SetSamplerState
    // dump. D3D9 defaults:
    // MAG/MIN = POINT, MIP = NONE, ADDRESS = WRAP.
    // Dest snap is fistp integer so POINT vs
    // LINEAR is the same on 1:1 pixel quads.
    public static Filter MagFilter => Filter.Nearest;
    public static Filter MinFilter => Filter.Nearest;
    public static SamplerMipmapMode MipMode => SamplerMipmapMode.Nearest;
    public static SamplerAddressMode AddressU => SamplerAddressMode.Repeat;
    public static SamplerAddressMode AddressV => SamplerAddressMode.Repeat;
    public static SamplerAddressMode AddressW => SamplerAddressMode.Repeat;
    public const float MaxLod = 0f;

    public static SamplerCreateInfo FirstSeenTemporary() =>
        new()
        {
            SType = StructureType.SamplerCreateInfo,
            MagFilter = MagFilter,
            MinFilter = MinFilter,
            MipmapMode = MipMode,
            AddressModeU = AddressU,
            AddressModeV = AddressV,
            AddressModeW = AddressW,
            MaxLod = MaxLod,
        };
}
