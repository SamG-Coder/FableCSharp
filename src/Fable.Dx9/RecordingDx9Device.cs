namespace Fable.Dx9;

/// <summary>
/// Records full DX9 calls. Locks return
/// writable storage of the requested size.
/// </summary>
public sealed class RecordingDx9Device : IDirect3DDevice9
{
    private readonly List<Dx9Call> _calls = [];
    private readonly Dictionary<int, byte[]> _vertex = [];
    private readonly Dictionary<int, byte[]> _index = [];
    private readonly Dictionary<int, byte[]> _texture = [];
    private int _next = 1;

    public IReadOnlyList<Dx9Call> Calls => _calls;

    public IEnumerable<string> Names => _calls.Select(NameOf);

    public int PresentCount => _calls.OfType<Dx9PresentCall>().Count();

    public static string NameOf(Dx9Call call) => call switch
    {
        Dx9ClearCall => "Clear",
        Dx9BeginSceneCall => "BeginScene",
        Dx9EndSceneCall => "EndScene",
        Dx9PresentCall => "Present",
        Dx9SetViewportCall => "SetViewport",
        Dx9CreateVertexBufferCall => "CreateVertexBuffer",
        Dx9LockVertexBufferCall => "LockVertexBuffer",
        Dx9UnlockVertexBufferCall => "UnlockVertexBuffer",
        Dx9CreateIndexBufferCall => "CreateIndexBuffer",
        Dx9LockIndexBufferCall => "LockIndexBuffer",
        Dx9UnlockIndexBufferCall => "UnlockIndexBuffer",
        Dx9CreateTextureCall => "CreateTexture",
        Dx9UploadTextureLevelCall => "UploadTextureLevel",
        Dx9SetRenderStateCall => "SetRenderState",
        Dx9SetSamplerStateCall => "SetSamplerState",
        Dx9SetTextureCall => "SetTexture",
        Dx9SetStreamSourceCall => "SetStreamSource",
        Dx9SetIndicesCall => "SetIndices",
        Dx9SetFvfCall => "SetFVF",
        Dx9SetVertexDeclarationCall => "SetVertexDeclaration",
        Dx9SetVertexShaderCall => "SetVertexShader",
        Dx9SetPixelShaderCall => "SetPixelShader",
        Dx9SetVertexShaderConstantFCall => "SetVertexShaderConstantF",
        Dx9SetPixelShaderConstantFCall => "SetPixelShaderConstantF",
        Dx9DrawPrimitiveCall => "DrawPrimitive",
        Dx9DrawIndexedPrimitiveCall => "DrawIndexedPrimitive",
        Dx9DrawIndexedPrimitiveUpCall => "DrawIndexedPrimitiveUP",
        Dx9DrawPrimitiveUpCall => "DrawPrimitiveUP",
        _ => call.GetType().Name,
    };

    public int CreateVertexBuffer(int length, int usage, int fvf, int pool)
    {
        var id = _next++;
        _vertex[id] = new byte[length];
        _calls.Add(new Dx9CreateVertexBufferCall(id, length, usage, fvf, pool));
        return id;
    }

    public void LockVertexBuffer(int buffer, int offset, int size, out Memory<byte> data, int flags)
    {
        data = Slice(_vertex, buffer, offset, size);
        _calls.Add(new Dx9LockVertexBufferCall(buffer, offset, size, flags));
    }

    public void UnlockVertexBuffer(int buffer) =>
        _calls.Add(new Dx9UnlockVertexBufferCall(buffer));

    public int CreateIndexBuffer(int length, int usage, int format, int pool)
    {
        var id = _next++;
        _index[id] = new byte[length];
        _calls.Add(new Dx9CreateIndexBufferCall(id, length, usage, format, pool));
        return id;
    }

    public void LockIndexBuffer(int buffer, int offset, int size, out Memory<byte> data, int flags)
    {
        data = Slice(_index, buffer, offset, size);
        _calls.Add(new Dx9LockIndexBufferCall(buffer, offset, size, flags));
    }

    public void UnlockIndexBuffer(int buffer) =>
        _calls.Add(new Dx9UnlockIndexBufferCall(buffer));

    public int CreateTexture(int width, int height, int levels, int usage, int format, int pool)
    {
        var id = _next++;
        _texture[id] = new byte[Math.Max(1, width * height * 4)];
        _calls.Add(new Dx9CreateTextureCall(id, width, height, levels, usage, format, pool));
        return id;
    }

    public void UploadTextureLevel(int texture, int level, ReadOnlySpan<byte> bytes)
    {
        if (!_texture.TryGetValue(texture, out var store))
            throw new InvalidOperationException($"texture {texture}");
        bytes.CopyTo(store.AsSpan(0, Math.Min(store.Length, bytes.Length)));
        _calls.Add(new Dx9UploadTextureLevelCall(texture, level, bytes.ToArray()));
    }

    public int CreateVertexShader(ReadOnlySpan<byte> function) =>
        throw new NotSupportedException("CreateVertexShader unread on first frontend Present.");

    public int CreatePixelShader(ReadOnlySpan<byte> function) =>
        throw new NotSupportedException("CreatePixelShader unread on first frontend Present.");

    public void BeginScene() => _calls.Add(new Dx9BeginSceneCall());

    public void EndScene() => _calls.Add(new Dx9EndSceneCall());

    public void Clear(Dx9Clear flags, uint colorArgb, float z, int stencil) =>
        _calls.Add(new Dx9ClearCall(flags, colorArgb, z, stencil));

    public void SetViewport(in Dx9Viewport viewport) =>
        _calls.Add(new Dx9SetViewportCall(viewport));

    public void SetRenderState(int state, int value) =>
        _calls.Add(new Dx9SetRenderStateCall(state, value));

    public void SetSamplerState(int sampler, int type, int value) =>
        _calls.Add(new Dx9SetSamplerStateCall(sampler, type, value));

    public void SetTexture(int stage, int texture) =>
        _calls.Add(new Dx9SetTextureCall(stage, texture));

    public void SetStreamSource(int stream, int buffer, int offset, int stride) =>
        _calls.Add(new Dx9SetStreamSourceCall(stream, buffer, offset, stride));

    public void SetIndices(int buffer) =>
        _calls.Add(new Dx9SetIndicesCall(buffer));

    public void SetFVF(int fvf) =>
        _calls.Add(new Dx9SetFvfCall(fvf));

    public void SetVertexDeclaration(int declaration) =>
        _calls.Add(new Dx9SetVertexDeclarationCall(declaration));

    public void SetVertexShader(int shader) =>
        _calls.Add(new Dx9SetVertexShaderCall(shader));

    public void SetPixelShader(int shader) =>
        _calls.Add(new Dx9SetPixelShaderCall(shader));

    public void SetVertexShaderConstantF(int startRegister, ReadOnlySpan<float> data) =>
        _calls.Add(new Dx9SetVertexShaderConstantFCall(startRegister, data.ToArray()));

    public void SetPixelShaderConstantF(int startRegister, ReadOnlySpan<float> data) =>
        _calls.Add(new Dx9SetPixelShaderConstantFCall(startRegister, data.ToArray()));

    public void DrawPrimitive(Dx9PrimitiveType type, int startVertex, int primitiveCount) =>
        _calls.Add(new Dx9DrawPrimitiveCall(type, startVertex, primitiveCount));

    public void DrawIndexedPrimitive(
        Dx9PrimitiveType type,
        int baseVertexIndex,
        int minVertexIndex,
        int numVertices,
        int startIndex,
        int primitiveCount) =>
        _calls.Add(new Dx9DrawIndexedPrimitiveCall(
            type, baseVertexIndex, minVertexIndex, numVertices, startIndex, primitiveCount));

    public void DrawIndexedPrimitiveUP(
        Dx9PrimitiveType type,
        int minVertexIndex,
        int numVertices,
        int primitiveCount,
        ReadOnlySpan<byte> indexData,
        int indexFormat,
        ReadOnlySpan<byte> vertexData,
        int vertexStride) =>
        _calls.Add(new Dx9DrawIndexedPrimitiveUpCall(
            type,
            minVertexIndex,
            numVertices,
            primitiveCount,
            indexData.ToArray(),
            indexFormat,
            vertexData.ToArray(),
            vertexStride));

    public void DrawPrimitiveUP(
        Dx9PrimitiveType type,
        int primitiveCount,
        ReadOnlySpan<byte> vertexData,
        int vertexStride) =>
        _calls.Add(new Dx9DrawPrimitiveUpCall(
            type, primitiveCount, vertexData.ToArray(), vertexStride));

    public void Present() => _calls.Add(new Dx9PresentCall());

    private static Memory<byte> Slice(Dictionary<int, byte[]> map, int id, int offset, int size)
    {
        if (!map.TryGetValue(id, out var buf))
            throw new InvalidOperationException($"buffer {id}");
        if (offset < 0 || offset > buf.Length)
            throw new ArgumentOutOfRangeException(nameof(offset));
        var length = size == 0 ? buf.Length - offset : size;
        if (length < 0 || offset + length > buf.Length)
            throw new ArgumentOutOfRangeException(nameof(size));
        return buf.AsMemory(offset, length);
    }
}
