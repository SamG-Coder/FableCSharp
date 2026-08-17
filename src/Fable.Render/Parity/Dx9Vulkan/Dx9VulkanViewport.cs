using Silk.NET.Vulkan;

namespace Fable.Render.Parity.Dx9Vulkan;

/// <summary>
/// First-seen viewport Z is baked into <c>009883F0</c>
/// (minZ 0.1 / maxZ 0.99). Vulkan's viewport depth
/// range stays 0..1 so those clip Z values are not
/// applied twice.
/// </summary>
public static class Dx9VulkanViewport
{
    // Fable DX9:
    // Camera helper minZ 0.1 / maxZ 0.99 written into
    // proj M33/M34 (009883F0 / 00B3106C). Viewport
    // width/height are camera+176/+180. Half-pixel
    // offset UNREAD.
    //
    // Vulkan:
    // viewport.x/y = 0, width/height = framebuffer,
    // minDepth = 0, maxDepth = 1.
    //
    // Difference:
    // D3D viewport MinZ/MaxZ would scale NDC Z again.
    // Fable already put MinZ/MaxZ in P, so Vulkan
    // 0..1 leaves clip.z/w in [0.1, 0.99].
    //
    // Proof:
    // LandscapeFrustum.FirstSeenMinZ / FirstSeenMaxZ /
    // ViewportZTerms. First-seen D3D SetViewport
    // MinZ/MaxZ UNREAD — do not invent a second scale.
    public const float MinDepth = 0f;
    public const float MaxDepth = 1f;
    public const float OffsetX = 0f;
    public const float OffsetY = 0f;

    // Fable DX9: UNREAD (half-pixel / 2D ortho)
    // Current Vulkan: no extra origin shift
    // Status: TEMPORARY — NOT PARITY PROVEN
    public const bool AppliesHalfPixelOffset = false;

    public static Viewport FromFramebuffer(uint width, uint height) =>
        new()
        {
            X = OffsetX,
            Y = OffsetY,
            Width = width,
            Height = height,
            MinDepth = MinDepth,
            MaxDepth = MaxDepth,
        };
}
