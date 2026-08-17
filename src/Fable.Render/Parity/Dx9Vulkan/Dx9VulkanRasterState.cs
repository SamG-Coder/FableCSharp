using Fable.Formats.Scene;
using Silk.NET.Vulkan;

namespace Fable.Render.Parity.Dx9Vulkan;

/// <summary>
/// First-seen <c>D3DCULL_CCW</c> after the clip-Y flip
/// keeps the same world faces as Vulkan
/// <c>FrontFace.CCW + Cull Back</c>.
/// </summary>
public static class Dx9VulkanRasterState
{
    // Fable DX9:
    // D3DRS_CULLMODE = D3DCULL_CCW (3) from 0x01396FB0
    // on landscape 00B24850 / static-lit 00BB2540.
    // PALSKIN 00BD3070 does not write CULLMODE; it
    // inherits that CCW.
    //
    // Vulkan equivalent:
    // FrontFace = VK_FRONT_FACE_COUNTER_CLOCKWISE
    // CullMode = VK_CULL_MODE_BACK_BIT
    //
    // Difference:
    // D3D viewport maps Y-up clip to Y-down screen, so
    // D3DCULL_CCW (cull screen-CCW) keeps clip-CCW
    // triangles. Dx9VulkanProjection flips clip Y so
    // Vulkan's Y-down NDC matches that screen space;
    // a clip-CCW triangle is framebuffer-CCW and is
    // kept by FrontFace.CCW + Cull Back.
    //
    // Proof:
    // D3dDeviceState.CullCcw / CullTable 0x01396FB0.
    // Concrete clip CCW (0,0)-(1,0)-(0,1): D3D draws
    // it; after M22=-1 Vulkan CCW+Back draws it.
    public static FrontFace FrontFaceAfterYFlip(int d3dCull) =>
        FrontFace.CounterClockwise;

    public static CullModeFlags CullMode(int d3dCull) => d3dCull switch
    {
        D3dDeviceState.CullNone => CullModeFlags.None,
        D3dDeviceState.CullCw => CullModeFlags.FrontBit,
        D3dDeviceState.CullCcw => CullModeFlags.BackBit,
        _ => CullModeFlags.BackBit,
    };

    public static FrontFace FirstSeenFrontFace =>
        FrontFaceAfterYFlip(D3dDeviceState.CullCcw);

    public static CullModeFlags FirstSeenCullMode =>
        CullMode(D3dDeviceState.CullCcw);

    // Fable DX9: UNREAD first-seen FILLMODE write.
    // D3D default SOLID. Current Vulkan: FILL.
    // Status: TEMPORARY — NOT PARITY PROVEN
    public static PolygonMode FirstSeenFillMode => PolygonMode.Fill;
}
