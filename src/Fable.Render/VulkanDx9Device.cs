using Fable.Dx9;
using Fable.Render.Parity.Dx9Vulkan;

namespace Fable.Render;

/// <summary>
/// Vulkan translation of used DX9 ops.
/// Default is Shadow: Clear/Present
/// record device state and do not
/// consume the swapchain. Set
/// <see cref="OwnsSwapchainPresent"/>
/// only for a NativeSemantic unit.
/// </summary>
public sealed class VulkanDx9Device : IDirect3DDevice9
{
    public VulkanLineRenderer? Renderer { get; set; }
    /// <summary>
    /// When false, <see cref="Present"/>
    /// does not call
    /// <see cref="VulkanLineRenderer.PresentDx9"/>.
    /// Compatibility <c>host.Draw</c>
    /// owns the swapchain.
    /// </summary>
    public bool OwnsSwapchainPresent { get; set; }
    public uint LastClearArgb { get; private set; }
    public bool InScene { get; private set; }
    public int PresentCount { get; private set; }
    public Dx9Clear LastClearFlags { get; private set; }
    public float LastClearZ { get; private set; }
    public int LastClearStencil { get; private set; }

    public int CreateVertexBuffer(int length, int usage, int fvf, int pool) =>
        throw Unread(nameof(CreateVertexBuffer));

    public void LockVertexBuffer(int buffer, int offset, int size, out Memory<byte> data, int flags)
    {
        data = default;
        throw Unread(nameof(LockVertexBuffer));
    }

    public void UnlockVertexBuffer(int buffer) =>
        throw Unread(nameof(UnlockVertexBuffer));

    public int CreateIndexBuffer(int length, int usage, int format, int pool) =>
        throw Unread(nameof(CreateIndexBuffer));

    public void LockIndexBuffer(int buffer, int offset, int size, out Memory<byte> data, int flags)
    {
        data = default;
        throw Unread(nameof(LockIndexBuffer));
    }

    public void UnlockIndexBuffer(int buffer) =>
        throw Unread(nameof(UnlockIndexBuffer));

    public int CreateTexture(int width, int height, int levels, int usage, int format, int pool) =>
        throw Unread(nameof(CreateTexture));

    public void UploadTextureLevel(int texture, int level, ReadOnlySpan<byte> bytes) =>
        throw Unread(nameof(UploadTextureLevel));

    public int CreateVertexShader(ReadOnlySpan<byte> function) =>
        throw Unread(nameof(CreateVertexShader));

    public int CreatePixelShader(ReadOnlySpan<byte> function) =>
        throw Unread(nameof(CreatePixelShader));

    public void BeginScene() => InScene = true;

    public void EndScene() => InScene = false;

    public void Clear(Dx9Clear flags, uint colorArgb, float z, int stencil)
    {
        LastClearFlags = flags;
        LastClearArgb = colorArgb;
        LastClearZ = z;
        LastClearStencil = stencil;
        if (OwnsSwapchainPresent)
            Renderer?.SetDx9ClearColor(Dx9VulkanColor.FromD3dArgb(colorArgb));
    }

    public void SetViewport(in Dx9Viewport viewport) =>
        throw Unread(nameof(SetViewport));

    public void SetRenderState(int state, int value) =>
        throw Unread(nameof(SetRenderState));

    public void SetSamplerState(int sampler, int type, int value) =>
        throw Unread(nameof(SetSamplerState));

    public void SetTexture(int stage, int texture) =>
        throw Unread(nameof(SetTexture));

    public void SetStreamSource(int stream, int buffer, int offset, int stride) =>
        throw Unread(nameof(SetStreamSource));

    public void SetIndices(int buffer) =>
        throw Unread(nameof(SetIndices));

    public void SetFVF(int fvf) =>
        throw Unread(nameof(SetFVF));

    public void SetVertexDeclaration(int declaration) =>
        throw Unread(nameof(SetVertexDeclaration));

    public void SetVertexShader(int shader) =>
        throw Unread(nameof(SetVertexShader));

    public void SetPixelShader(int shader) =>
        throw Unread(nameof(SetPixelShader));

    public void SetVertexShaderConstantF(int startRegister, ReadOnlySpan<float> data) =>
        throw Unread(nameof(SetVertexShaderConstantF));

    public void SetPixelShaderConstantF(int startRegister, ReadOnlySpan<float> data) =>
        throw Unread(nameof(SetPixelShaderConstantF));

    public void DrawPrimitive(Dx9PrimitiveType type, int startVertex, int primitiveCount) =>
        throw Unread(nameof(DrawPrimitive));

    public void DrawIndexedPrimitive(
        Dx9PrimitiveType type,
        int baseVertexIndex,
        int minVertexIndex,
        int numVertices,
        int startIndex,
        int primitiveCount) =>
        throw Unread(nameof(DrawIndexedPrimitive));

    public void DrawIndexedPrimitiveUP(
        Dx9PrimitiveType type,
        int minVertexIndex,
        int numVertices,
        int primitiveCount,
        ReadOnlySpan<byte> indexData,
        int indexFormat,
        ReadOnlySpan<byte> vertexData,
        int vertexStride)
    {
        if (OwnsSwapchainPresent)
            throw Unread(nameof(DrawIndexedPrimitiveUP));
    }

    public void DrawPrimitiveUP(
        Dx9PrimitiveType type,
        int primitiveCount,
        ReadOnlySpan<byte> vertexData,
        int vertexStride)
    {
        if (OwnsSwapchainPresent)
            throw Unread(nameof(DrawPrimitiveUP));
    }

    public void Present()
    {
        PresentCount++;
        if (OwnsSwapchainPresent)
            Renderer?.PresentDx9();
    }

    private static NotSupportedException Unread(string name) =>
        new($"IDirect3DDevice9.{name} is UNREAD on first 0042DF9E Present.");
}
