using System.Numerics;
using Fable.Formats.Scene;

namespace Fable.Render.Parity.Dx9Vulkan;

/// <summary>
/// First-seen fog / clear pack is ARGB
/// <c>0xFF000000</c> from record (0,0,0,1)*255.
/// </summary>
public static class Dx9VulkanColor
{
    // Fable DX9:
    // 00B47630 packs record +64..+76 * 255 as ARGB
    // into D3DRS_FOGCOLOR. First-seen (0,0,0,1) →
    // 0xFF000000. Live clear uses that black.
    //
    // Vulkan equivalent:
    // clear = (0, 0, 0, 1) float.
    //
    // Evidence:
    // D3dDeviceState.FirstSeenFogColorArgb,
    // WorldShading.FogRecordColor.
    public static Vector4 FromD3dArgb(uint argb) =>
        new(
            ((argb >> 16) & 0xFF) / 255f,
            ((argb >> 8) & 0xFF) / 255f,
            (argb & 0xFF) / 255f,
            ((argb >> 24) & 0xFF) / 255f);

    public static Vector4 FirstSeenClear =>
        FromD3dArgb(D3dDeviceState.FirstSeenFogColorArgb);

    // Fable DX9:
    // D3DCOLOR in the landscape extra is BGR at dest+20
    // (00BFE050). VS oT0.xy = v3.yz.
    //
    // Vulkan:
    // ExtraRgb is stored as float3 (R,G,B) = (byte0,1,2)/255
    // after the BGR unpack in LevTileMesh.
    //
    // Evidence:
    // LandscapeTextures.GpuExtraOffset / FirstSeenOt0FromV3.
    public static Vector3 FromD3dColorBgr(byte b, byte g, byte r) =>
        new(r / 255f, g / 255f, b / 255f);
}
