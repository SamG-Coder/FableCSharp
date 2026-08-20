using System.Numerics;
using System.Runtime.InteropServices;
using Fable.Dx9;
using Fable.Render.Parity.Dx9Vulkan;

namespace Fable.Render;

/// <summary>
/// Vulkan translation of used DX9 ops.
/// Frontend DIPUP / glyph UP accumulate
/// a 2D batch and Present submits it.
/// <see cref="OwnsSwapchainPresent"/>
/// is the NativeSemantic swapchain gate.
/// </summary>
public sealed class VulkanDx9Device : IDirect3DDevice9
{
    private readonly List<FrontendGpuVertex> _vertices = [];
    private readonly List<ushort> _indices = [];
    private readonly List<FrontendDraw> _draws = [];
    private readonly Dictionary<int, GpuTexture> _textures = [];
    private readonly Dictionary<int, int> _renderState = [];
    private int _nextTexture = 1;
    private int _stage0;
    private Dx9Viewport _viewport = new(
        0, 0, Dx9VulkanFrontend.DisplayWidth, Dx9VulkanFrontend.DisplayHeight,
        Dx9VulkanFrontend.ViewportMinZ, Dx9VulkanFrontend.ViewportMaxZ);

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
    public FrontendSubmitBatch LastBatch { get; private set; }

    public void BindFrontendTextures(IReadOnlyList<GpuTexture> textures)
    {
        _textures.Clear();
        foreach (var tex in textures)
            _textures[tex.Id] = tex;
    }

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

    public int CreateTexture(int width, int height, int levels, int usage, int format, int pool)
    {
        var id = _nextTexture++;
        _textures[id] = new GpuTexture(id, width, height, new byte[Math.Max(4, width * height * 4)]);
        return id;
    }

    public void UploadTextureLevel(int texture, int level, ReadOnlySpan<byte> bytes)
    {
        if (!_textures.TryGetValue(texture, out var tex))
            throw new InvalidOperationException($"texture {texture}");
        var copy = new byte[bytes.Length];
        bytes.CopyTo(copy);
        _textures[texture] = tex with { Rgba = copy };
        _ = level;
    }

    public int CreateVertexShader(ReadOnlySpan<byte> function) =>
        throw Unread(nameof(CreateVertexShader));

    public int CreatePixelShader(ReadOnlySpan<byte> function) =>
        throw Unread(nameof(CreatePixelShader));

    public void BeginScene()
    {
        InScene = true;
        _vertices.Clear();
        _indices.Clear();
        _draws.Clear();
    }

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

    public void SetViewport(in Dx9Viewport viewport) => _viewport = viewport;

    public void SetRenderState(int state, int value) => _renderState[state] = value;

    public void SetSamplerState(int sampler, int type, int value)
    {
        _ = (sampler, type, value);
    }

    public void SetTexture(int stage, int texture)
    {
        if (stage == 0)
            _stage0 = texture;
    }

    public void SetStreamSource(int stream, int buffer, int offset, int stride) =>
        throw Unread(nameof(SetStreamSource));

    public void SetIndices(int buffer) =>
        throw Unread(nameof(SetIndices));

    public void SetFVF(int fvf)
    {
        _ = fvf;
    }

    public void SetVertexDeclaration(int declaration) =>
        throw Unread(nameof(SetVertexDeclaration));

    public void SetVertexShader(int shader)
    {
        _ = shader;
    }

    public void SetPixelShader(int shader)
    {
        _ = shader;
    }

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
        if (primitiveCount <= 0 || numVertices <= 0 || vertexStride <= 0)
            return;
        _ = (type, minVertexIndex, indexFormat);
        var firstVertex = (uint)_vertices.Count;
        var firstIndex = (uint)_indices.Count;
        for (var i = 0; i < numVertices; i++)
            _vertices.Add(ToGpu(ReadVertex(vertexData, i * vertexStride, vertexStride)));
        var words = MemoryMarshal.Cast<byte, ushort>(indexData);
        foreach (var w in words)
            _indices.Add(w);
        var (src, dst) = Dx9VulkanFrontend.BlendFromHandlerMode(
            Dx9VulkanFrontend.WidgetBlendDefault);
        _draws.Add(new FrontendDraw(
            _stage0,
            firstVertex,
            (uint)numVertices,
            firstIndex,
            (uint)words.Length,
            src,
            dst,
            BlendEnable: true,
            Dx9VulkanFrontend.D3dptTriangleList));
    }

    public void DrawPrimitiveUP(
        Dx9PrimitiveType type,
        int primitiveCount,
        ReadOnlySpan<byte> vertexData,
        int vertexStride)
    {
        if (primitiveCount <= 0 || vertexStride <= 0)
            return;
        _ = type;
        var count = vertexData.Length / vertexStride;
        if (count <= 0)
            return;
        var firstVertex = (uint)_vertices.Count;
        for (var i = 0; i < count; i++)
            _vertices.Add(ToGpu(ReadVertex(vertexData, i * vertexStride, vertexStride)));
        var (src, dst) = Dx9VulkanFrontend.BlendFromHandlerMode(
            Dx9VulkanFrontend.WidgetBlendDefault);
        _draws.Add(new FrontendDraw(
            _stage0,
            firstVertex,
            (uint)count,
            0,
            0,
            src,
            dst,
            BlendEnable: true,
            Dx9VulkanFrontend.D3dptTriangleList));
    }

    public void Present()
    {
        PresentCount++;
        var textures = _textures.Values.ToArray();
        LastBatch = new FrontendSubmitBatch(
            [.. _vertices],
            [.. _indices],
            [.. _draws],
            textures,
            _viewport.X,
            _viewport.Y,
            _viewport.Width,
            _viewport.Height,
            _viewport.MinZ,
            _viewport.MaxZ);
        Renderer?.SetFrontendBatch(LastBatch.IsEmpty ? null : LastBatch);
        if (OwnsSwapchainPresent)
            Renderer?.PresentDx9();
    }

    private FrontendGpuVertex ToGpu(FrontendDx9Vertex src) =>
        Dx9VulkanFrontend.ToGpuVertex(
            src, _viewport.X, _viewport.Y, _viewport.Width, _viewport.Height);

    private static FrontendDx9Vertex ReadVertex(ReadOnlySpan<byte> data, int offset, int stride)
    {
        if (offset + 28 > data.Length || stride < 28)
            return new FrontendDx9Vertex(0, 0, 0, 1, 0xFFFFFFFFu, 0, 0);
        var slice = data.Slice(offset);
        var x = BitConverter.ToSingle(slice[..4]);
        var y = BitConverter.ToSingle(slice.Slice(4, 4));
        var z = BitConverter.ToSingle(slice.Slice(8, 4));
        var rhw = BitConverter.ToSingle(slice.Slice(12, 4));
        var diffuse = BitConverter.ToUInt32(slice.Slice(16, 4));
        var u = BitConverter.ToSingle(slice.Slice(20, 4));
        var v = BitConverter.ToSingle(slice.Slice(24, 4));
        return new FrontendDx9Vertex(x, y, z, rhw, diffuse, u, v);
    }

    private static NotSupportedException Unread(string name) =>
        new($"IDirect3DDevice9.{name} is UNREAD on first 0042DF9E Present.");
}
