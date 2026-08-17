using Fable.Formats.Scene;
using Silk.NET.Vulkan;

namespace Fable.Render.Parity.Dx9Vulkan;

/// <summary>
/// First-seen opaque draws have alphablend off.
/// PALSKIN bind writes SRCALPHA / INVSRCALPHA.
/// </summary>
public static class Dx9VulkanBlendState
{
    // Fable DX9:
    // D3DRS_SRCBLEND = D3DBLEND_SRCALPHA (5)
    // D3DRS_DESTBLEND = D3DBLEND_INVSRCALPHA (6)
    // D3DRS_ALPHABLENDENABLE = 1
    // PALSKIN 00BD3867 / 00BD38D4, no Flag1 test.
    //
    // Vulkan equivalent:
    // srcColor = VK_BLEND_FACTOR_SRC_ALPHA
    // dstColor = VK_BLEND_FACTOR_ONE_MINUS_SRC_ALPHA
    // colorOp = VK_BLEND_OP_ADD
    //
    // Evidence:
    // D3dDeviceState.FirstSeenPalskinSrcBlend / DestBlend.
    public static BlendFactor ColorFactor(int d3dBlend) => d3dBlend switch
    {
        D3dDeviceState.BlendZero => BlendFactor.Zero,
        D3dDeviceState.BlendOne => BlendFactor.One,
        D3dDeviceState.BlendSrcAlpha => BlendFactor.SrcAlpha,
        D3dDeviceState.BlendInvSrcAlpha => BlendFactor.OneMinusSrcAlpha,
        _ => BlendFactor.One,
    };

    public static BlendFactor FirstSeenPalskinSrc =>
        ColorFactor(D3dDeviceState.FirstSeenPalskinSrcBlend);

    public static BlendFactor FirstSeenPalskinDst =>
        ColorFactor(D3dDeviceState.FirstSeenPalskinDestBlend);

    // Fable DX9: UNREAD first-seen BLENDOP write.
    // D3D default ADD. Current Vulkan: ADD.
    // Status: TEMPORARY — NOT PARITY PROVEN
    public static BlendOp FirstSeenBlendOp => BlendOp.Add;

    // Fable DX9: UNREAD ALPHATESTENABLE / ALPHAREF /
    // ALPHAFUNC on first-seen landscape / static-lit.
    // Slot numbers exist (D3dDeviceState.AlphaTestEnable).
    // Current Vulkan: no discard.
    // Status: TEMPORARY — NOT PARITY PROVEN
    public const bool FirstSeenAlphaTest = false;

    public static PipelineColorBlendAttachmentState Opaque() =>
        new()
        {
            BlendEnable = false,
            ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit |
                             ColorComponentFlags.BBit | ColorComponentFlags.ABit,
        };

    public static PipelineColorBlendAttachmentState PalskinSrcAlpha() =>
        new()
        {
            BlendEnable = true,
            SrcColorBlendFactor = FirstSeenPalskinSrc,
            DstColorBlendFactor = FirstSeenPalskinDst,
            ColorBlendOp = FirstSeenBlendOp,
            SrcAlphaBlendFactor = FirstSeenPalskinSrc,
            DstAlphaBlendFactor = FirstSeenPalskinDst,
            AlphaBlendOp = FirstSeenBlendOp,
            ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit |
                             ColorComponentFlags.BBit | ColorComponentFlags.ABit,
        };
}
