using Silk.NET.Vulkan;

namespace Fable.Render.Parity.Dx9Vulkan;

/// <summary>
/// First-seen D3DSAMP_* writes are UNREAD. The live
/// sampler is the documented TEMPORARY host state.
/// </summary>
public static class Dx9VulkanSamplerState
{
    // Fable DX9: UNREAD
    // No first-seen SetSamplerState dump for MAG/MIN/
    // MIP/ADDRESS. D3D defaults are POINT / NONE / WRAP.
    //
    // Current Vulkan:
    // mag/min = LINEAR, mip = LINEAR, address = REPEAT,
    // MaxLod = 1 (only the uploaded top mip).
    //
    // Status: TEMPORARY — NOT PARITY PROVEN
    public static Filter MagFilter => Filter.Linear;
    public static Filter MinFilter => Filter.Linear;
    public static SamplerMipmapMode MipMode => SamplerMipmapMode.Linear;
    public static SamplerAddressMode AddressU => SamplerAddressMode.Repeat;
    public static SamplerAddressMode AddressV => SamplerAddressMode.Repeat;
    public static SamplerAddressMode AddressW => SamplerAddressMode.Repeat;
    public const float MaxLod = 1f;

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
