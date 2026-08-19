namespace Fable.Dx9;

/// <summary>
/// Neutral DX9 device. No Silk, no Vulkan.
/// Subset proven in TLC first-seen Present.
/// </summary>
public interface IDirect3DDevice9
{
    int CreateVertexBuffer(int length, int usage, int fvf, int pool);
    void LockVertexBuffer(int buffer, int offset, int size, out Memory<byte> data, int flags);
    void UnlockVertexBuffer(int buffer);
    int CreateIndexBuffer(int length, int usage, int format, int pool);
    void LockIndexBuffer(int buffer, int offset, int size, out Memory<byte> data, int flags);
    void UnlockIndexBuffer(int buffer);
    int CreateTexture(int width, int height, int levels, int usage, int format, int pool);
    void UploadTextureLevel(int texture, int level, ReadOnlySpan<byte> bytes);
    int CreateVertexShader(ReadOnlySpan<byte> function);
    int CreatePixelShader(ReadOnlySpan<byte> function);
    void BeginScene();
    void EndScene();
    void Clear(Dx9Clear flags, uint colorArgb, float z, int stencil);
    void SetViewport(in Dx9Viewport viewport);
    void SetRenderState(int state, int value);
    void SetSamplerState(int sampler, int type, int value);
    void SetTexture(int stage, int texture);
    void SetStreamSource(int stream, int buffer, int offset, int stride);
    void SetIndices(int buffer);
    void SetFVF(int fvf);
    void SetVertexDeclaration(int declaration);
    void SetVertexShader(int shader);
    void SetPixelShader(int shader);
    void SetVertexShaderConstantF(int startRegister, ReadOnlySpan<float> data);
    void SetPixelShaderConstantF(int startRegister, ReadOnlySpan<float> data);
    void DrawPrimitive(Dx9PrimitiveType type, int startVertex, int primitiveCount);
    void DrawIndexedPrimitive(
        Dx9PrimitiveType type,
        int baseVertexIndex,
        int minVertexIndex,
        int numVertices,
        int startIndex,
        int primitiveCount);
    void Present();
}
