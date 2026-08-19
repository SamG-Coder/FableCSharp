namespace Fable.Dx9;

/// <summary>
/// One IDirect3DDevice9 call with full
/// arguments. Not a lossy name+int bag.
/// </summary>
public abstract record Dx9Call;

public sealed record Dx9ClearCall(
    Dx9Clear Flags, uint ColorArgb, float Z, int Stencil) : Dx9Call;

public sealed record Dx9BeginSceneCall : Dx9Call;

public sealed record Dx9EndSceneCall : Dx9Call;

public sealed record Dx9PresentCall : Dx9Call;

public sealed record Dx9SetViewportCall(Dx9Viewport Viewport) : Dx9Call;

public sealed record Dx9CreateVertexBufferCall(
    int Handle, int Length, int Usage, int Fvf, int Pool) : Dx9Call;

public sealed record Dx9LockVertexBufferCall(
    int Buffer, int Offset, int Size, int Flags) : Dx9Call;

public sealed record Dx9UnlockVertexBufferCall(int Buffer) : Dx9Call;

public sealed record Dx9CreateIndexBufferCall(
    int Handle, int Length, int Usage, int Format, int Pool) : Dx9Call;

public sealed record Dx9LockIndexBufferCall(
    int Buffer, int Offset, int Size, int Flags) : Dx9Call;

public sealed record Dx9UnlockIndexBufferCall(int Buffer) : Dx9Call;

public sealed record Dx9CreateTextureCall(
    int Handle, int Width, int Height, int Levels, int Usage, int Format, int Pool)
    : Dx9Call;

public sealed record Dx9UploadTextureLevelCall(
    int Texture, int Level, byte[] Bytes) : Dx9Call;

public sealed record Dx9SetRenderStateCall(int State, int Value) : Dx9Call;

public sealed record Dx9SetSamplerStateCall(int Sampler, int Type, int Value) : Dx9Call;

public sealed record Dx9SetTextureCall(int Stage, int Texture) : Dx9Call;

public sealed record Dx9SetStreamSourceCall(
    int Stream, int Buffer, int Offset, int Stride) : Dx9Call;

public sealed record Dx9SetIndicesCall(int Buffer) : Dx9Call;

public sealed record Dx9SetFvfCall(int Fvf) : Dx9Call;

public sealed record Dx9SetVertexDeclarationCall(int Declaration) : Dx9Call;

public sealed record Dx9SetVertexShaderCall(int Shader) : Dx9Call;

public sealed record Dx9SetPixelShaderCall(int Shader) : Dx9Call;

public sealed record Dx9SetVertexShaderConstantFCall(
    int StartRegister, float[] Data) : Dx9Call;

public sealed record Dx9SetPixelShaderConstantFCall(
    int StartRegister, float[] Data) : Dx9Call;

public sealed record Dx9DrawPrimitiveCall(
    Dx9PrimitiveType Type, int StartVertex, int PrimitiveCount) : Dx9Call;

public sealed record Dx9DrawIndexedPrimitiveCall(
    Dx9PrimitiveType Type,
    int BaseVertexIndex,
    int MinVertexIndex,
    int NumVertices,
    int StartIndex,
    int PrimitiveCount) : Dx9Call;
