using System.Numerics;
using Fable.Formats;
using Fable.Formats.Levels;

namespace Fable.Render.Parity.Dx9Vulkan;

/// <summary>
/// First-seen VS consumes <c>c5–c8</c> as the
/// <c>00988A50</c> product. The host uploads that
/// 4×4 as a push-constant <c>mat4</c>.
/// </summary>
public static class Dx9VulkanShaderConstants
{
    // Fable DX9:
    // 00988A50: world+496 * view+560 * proj+624 → +752
    // SetVSConstantF(c5, count=4). VS dp4 oPos, pos,
    // c5–c8. System.Numerics / GPU consume p * W * V * P.
    // Row-major bytes uploaded as-is; GLSL column-major
    // reads them as the needed transpose. Do not
    // Transpose() again.
    //
    // Vulkan equivalent:
    // push.viewProj = that product (after clip-Y flip).
    //
    // Evidence:
    // LandscapeFrustum.WvpFlush 00988A50,
    // FirstSeenWvpIsWorldViewProj,
    // CameraProjectionTests gpu-upload lock.
    public const int WvpStartRegister = LandscapeFrustum.WvpStartRegister;
    public const int WvpRegisterCount = LandscapeFrustum.WvpRegisterCount;

    public static Matrix4x4 PackWvp(Matrix4x4 world, Matrix4x4 view, Matrix4x4 dx9Proj) =>
        Dx9VulkanProjection.ToVulkanWvp(
            LandscapeFrustum.ComposeWvp(world, view, dx9Proj));

    public static Vector4 FogPlane(Vector3 cameraPos, Vector3 look) =>
        WorldShading.LinearFogPlane(cameraPos, look);

    public static Vector4 DirLightDirection => WorldShading.DirLightDirection;
    public static Vector4 DirLightColor => WorldShading.DirLightColor;
    public static Vector4 LitColor => WorldShading.LitColor;
    public static Vector4 C0 => WorldShading.FirstSeenC0;
    public static Vector4 C1 => WorldShading.FirstSeenC1;
    public static Vector4 C3 => WorldShading.FirstSeenC3;
    public static Vector4 FogColor => WorldShading.FogRecordColor;
}
