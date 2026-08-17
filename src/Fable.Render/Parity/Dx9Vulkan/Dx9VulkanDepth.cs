using Fable.Formats.Scene;
using Silk.NET.Vulkan;

namespace Fable.Render.Parity.Dx9Vulkan;

/// <summary>
/// First-seen depth compare is D3DCMP_LESSEQUAL.
/// Vulkan maps that opcode 1:1.
/// </summary>
public static class Dx9VulkanDepth
{
    // Fable DX9:
    // D3DRS_ZFUNC = D3DCMP_LESSEQUAL (4)
    //
    // Vulkan equivalent:
    // depthCompareOp = VK_COMPARE_OP_LESS_OR_EQUAL
    //
    // Evidence:
    // docs/PARITY.md first-seen lock "D3D Z LESSEQUAL"
    // (ScenePassTests / WorldSceneTests). D3DCMP_LESSEQUAL=4.
    public const int D3dCmpLessEqual = 4;
    public const int D3dRsZFunc = 23;
    public const int D3dRsZEnable = 7;
    public const int D3dRsZWriteEnable = 14;

    public static CompareOp CompareOp(int d3dCmp) => d3dCmp switch
    {
        1 => Silk.NET.Vulkan.CompareOp.Never,
        2 => Silk.NET.Vulkan.CompareOp.Less,
        3 => Silk.NET.Vulkan.CompareOp.Equal,
        D3dCmpLessEqual => Silk.NET.Vulkan.CompareOp.LessOrEqual,
        5 => Silk.NET.Vulkan.CompareOp.Greater,
        6 => Silk.NET.Vulkan.CompareOp.NotEqual,
        7 => Silk.NET.Vulkan.CompareOp.GreaterOrEqual,
        8 => Silk.NET.Vulkan.CompareOp.Always,
        _ => Silk.NET.Vulkan.CompareOp.LessOrEqual,
    };

    public static CompareOp FirstSeenCompareOp =>
        CompareOp(D3dDeviceState.FirstSeenZFunc);

    // Fable DX9: UNREAD explicit first-seen ZENABLE/ZWRITE
    // writes. D3D defaults TRUE/TRUE. First-seen 3D
    // landscape / static-lit / PALSKIN consume a depth
    // buffer (LESSEQUAL lock).
    // Current Vulkan: test=1 write=1
    // Status: TEMPORARY — NOT PARITY PROVEN for the
    // SetRenderState site; compare-op is PROVEN.
    public const bool FirstSeenDepthTest = true;
    public const bool FirstSeenDepthWrite = true;

    public static PipelineDepthStencilStateCreateInfo FirstSeenOpaque() =>
        new()
        {
            SType = StructureType.PipelineDepthStencilStateCreateInfo,
            DepthTestEnable = FirstSeenDepthTest,
            DepthWriteEnable = FirstSeenDepthWrite,
            DepthCompareOp = FirstSeenCompareOp,
        };
}
