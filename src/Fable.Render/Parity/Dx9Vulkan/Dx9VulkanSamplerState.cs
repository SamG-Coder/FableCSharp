using Silk.NET.Vulkan;

namespace Fable.Render.Parity.Dx9Vulkan;

/// <summary>Recovered D3D9 sampler writes and their Vulkan equivalents.</summary>
public static class Dx9VulkanSamplerState
{
    public const uint StateFlushFn = 0x00A058C0;
    public const int SetSamplerStateVtbl = 276;
    public const uint FrontendAddressBranch = 0x00BAF1BD;
    public const uint FrontendFilterBranch = 0x00BAF362;
    public const uint PerFrameSamplerDefaults = 0x00B25180;
    public const uint D3dAddressWrapVa = 0x01396DC8;
    public const uint D3dAddressClampVa = 0x01396DD0;
    public const uint D3dFilterNoneVa = 0x01396E6C;
    public const uint D3dFilterPointVa = 0x01396E58;
    public const uint D3dFilterLinearVa = 0x01396E5C;
    public const int D3dAddressWrap = 1;
    public const int D3dAddressClamp = 3;
    public const int D3dFilterNone = 0;
    public const int D3dFilterPoint = 1;
    public const int D3dFilterLinear = 2;
    public const int Sprite2DFilterPreserveBit = 0x2;

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

    /// <summary>
    /// Whether type-0x22 preserves the per-frame filter state. The control is
    /// authored in frontend.bin as Sprite2DFlag, copied def +548 → widget +372
    /// → record +60, then extracted by 00BAD90B and copied to draw +160.
    /// </summary>
    public static bool PreservesPerFrameFilter(int sprite2DFlag) =>
        (sprite2DFlag & Sprite2DFilterPreserveBit) != 0;

    /// <summary>
    /// 00B25180..00B253AB queues LINEAR MIP/MAG/MIN and CLAMP U/V/W
    /// for all eight stages each frame. At 00BAF362 a clear Sprite2DFlag
    /// bit overrides MAG/MIN to POINT and MIP to NONE; a set bit preserves
    /// the LINEAR defaults. Vulkan currently uploads one frontend mip level.
    /// </summary>
    public static SamplerCreateInfo FrontendType22(int sprite2DFlag)
    {
        var linear = PreservesPerFrameFilter(sprite2DFlag);
        return new()
        {
            SType = StructureType.SamplerCreateInfo,
            MagFilter = linear ? Filter.Linear : Filter.Nearest,
            MinFilter = linear ? Filter.Linear : Filter.Nearest,
            MipmapMode = linear ? SamplerMipmapMode.Linear : SamplerMipmapMode.Nearest,
            AddressModeU = SamplerAddressMode.ClampToEdge,
            AddressModeV = SamplerAddressMode.ClampToEdge,
            AddressModeW = SamplerAddressMode.ClampToEdge,
            MaxLod = 0f,
        };
    }
}
