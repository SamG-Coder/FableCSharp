using Silk.NET.Vulkan;

namespace Fable.Render.Parity.Dx9Vulkan;

/// <summary>
/// First-seen world draws are triangle lists after
/// D3D strip unwind. Gizmo lines are debug-only.
/// </summary>
public static class Dx9VulkanPrimitive
{
    // Fable DX9:
    // C3D blocks: strip (odd t swaps b,a,c) or list
    // (count triangles). Landscape STB: PrimitiveCount
    // strip, IndexCount = PrimitiveCount+2, odd-i swap.
    //
    // Vulkan equivalent:
    // VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST of the
    // unwound faces. Winding of each face is preserved.
    //
    // Evidence:
    // MeshFile strip/list emit, LevTileMesh.AddStrip.
    public static PrimitiveTopology World => PrimitiveTopology.TriangleList;

    public static PrimitiveTopology DebugLines => PrimitiveTopology.LineList;

    public static void UnwindStripTriangle(int i, ref int b, ref int c)
    {
        if ((i & 1) == 1)
            (b, c) = (c, b);
    }
}
