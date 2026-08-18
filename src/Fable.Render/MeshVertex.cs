using System.Numerics;
using System.Runtime.InteropServices;

namespace Fable.Render;

[StructLayout(LayoutKind.Sequential)]
public readonly struct MeshVertex(Vector3 position, Vector3 normal, Vector2 uv, Vector4 color, Vector3 extra = default)
{
    public readonly Vector3 Position = position;
    public readonly Vector3 Normal = normal;
    public readonly Vector2 Uv = uv;
    public readonly Vector4 Color = color;
    public readonly Vector3 Extra = extra;

    public const uint Stride = 60;
    public const uint UvOffset = 24;
    public const uint ColorOffset = 32;
    public const uint ExtraOffset = 48;
}

public readonly record struct MeshDraw(
    int TextureId,
    uint FirstVertex,
    uint VertexCount,
    int TextureId1 = 0,
    uint PassBit = 0,
    float ShaderMode = 1f,
    bool SrcAlphaBlend = false,
    Matrix4x4 World = default,
    uint FirstIndex = 0,
    uint IndexCount = 0)
{
    /// <summary>
    /// Native wrapper+496. Zero matrix means
    /// identity (<c>00988290</c>).
    /// </summary>
    public Matrix4x4 WorldOrIdentity =>
        World.M44 == 0 && World.M11 == 0 && World.M22 == 0
            ? Matrix4x4.Identity
            : World;

    public bool Indexed => IndexCount > 0;
}

public readonly record struct GpuTexture(int Id, int Width, int Height, byte[] Rgba)
{
    public static GpuTexture Fallback(int id = 0) =>
        new(id, 1, 1, [115, 128, 97, 255]);

    public static GpuTexture White() =>
        new(-1, 1, 1, [255, 255, 255, 255]);
}
