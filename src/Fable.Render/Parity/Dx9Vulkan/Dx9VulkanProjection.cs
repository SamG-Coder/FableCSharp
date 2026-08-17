using System.Numerics;
using Fable.Formats.Levels;

namespace Fable.Render.Parity.Dx9Vulkan;

/// <summary>
/// DX9 clip Y is up; Vulkan NDC Y is down. Fable's
/// <c>009883F0</c> writes <c>M11=M22=1</c>. The sign
/// change is applied here, not inside the Fable
/// projection builder.
/// </summary>
public static class Dx9VulkanProjection
{
    // Fable DX9:
    // 009883F0 wrapper+624: M11=M22=1, M33/Q from
    // helper near 0.1 / far 4000 / minZ 0.1 / maxZ 0.99.
    // Exe memory M34=Q M43=1. VS dp4 on those rows is
    // clip.w = view.z. Numerics stores the transpose
    // (M34=1, M43=Q) so p*P matches that VS.
    // Cot lives on camera+128, not in proj.
    //
    // Vulkan:
    // NDC Y is down. Flip the Y row of the already-built
    // DX9 WVP (or of P before W*V*P). Equivalent:
    // wvp * diag(1, -1, 1, 1) for row-vector p*W*V*P.
    //
    // Difference:
    // D3D9 clip Y-up vs Vulkan NDC Y-down. Not a Fable
    // constant. Do not bake this into 009883F0.
    //
    // Proof:
    // LandscapeFrustum.ProjBuilder 009883F0 /
    // FirstSeenProjXyIsIdentity. Vulkan spec
    // "Basic Code: Coordinate Transformations".
    public const float NdcYSign = -1f;

    public static readonly Matrix4x4 ClipYFlip = new(
        1f, 0f, 0f, 0f,
        0f, NdcYSign, 0f, 0f,
        0f, 0f, 1f, 0f,
        0f, 0f, 0f, 1f);

    public static Matrix4x4 ToVulkanProjection(Matrix4x4 dx9Projection) =>
        dx9Projection * ClipYFlip;

    public static Matrix4x4 ToVulkanWvp(Matrix4x4 dx9Wvp) =>
        dx9Wvp * ClipYFlip;

    public static Matrix4x4 FirstSeenDx9Projection()
    {
        LandscapeFrustum.ViewportZTerms(
            LandscapeFrustum.FirstSeenNear,
            LandscapeFrustum.FirstSeenFar,
            LandscapeFrustum.FirstSeenMinZ,
            LandscapeFrustum.FirstSeenMaxZ,
            out var m33, out var m34);
        return LandscapeFrustum.FirstSeenProjection(m33, m34, LandscapeFrustum.Dx9ProjectionYSign);
    }

    public static Matrix4x4 FirstSeenVulkanProjection() =>
        ToVulkanProjection(FirstSeenDx9Projection());

    public static Vector4 TransformClip(Matrix4x4 wvp, Vector3 world) =>
        Vector4.Transform(new Vector4(world, 1f), wvp);

    public static Vector4 ToNdc(Vector4 clip)
    {
        if (MathF.Abs(clip.W) < 1e-8f)
            return clip;
        return new Vector4(clip.X / clip.W, clip.Y / clip.W, clip.Z / clip.W, clip.W);
    }
}
