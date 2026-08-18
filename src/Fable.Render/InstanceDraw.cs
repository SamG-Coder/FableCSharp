using System.Numerics;
using Fable.Formats;
using Fable.Formats.Scene;
using Fable.Render.Parity.Dx9Vulkan;

namespace Fable.Render;

/// <summary>
/// Static-lit <c>00BB2540</c> vs PALSKIN <c>00BD3070</c>.
/// Do not flatten PALSKIN dest into the static path.
/// </summary>
public enum InstanceKind
{
    StaticLit,
    Palskin,
}

/// <summary>
/// Persistent file-local VB/IB identity. Shared by every TNG
/// instance of the same mesh primitive. World stays on the draw.
/// </summary>
public readonly record struct InstanceBufferKey(uint MeshId, int PrimitiveIndex, InstanceKind Kind);

/// <summary>
/// One-stage static / PALSKIN material. First-seen does not bind bump
/// (<see cref="WorldShading.FirstSeenBindsC3dBump"/>).
/// </summary>
public readonly record struct InstanceMaterialKey(
    int DiffuseId,
    bool SrcAlphaBlend,
    InstanceKind Kind);

/// <summary>
/// Packed dest rows for <c>00BCFB00</c> <c>SetVSConstantF(c38, n*3)</c>.
/// Empty on static-lit. Not a CPU-skinned world triangle list.
/// </summary>
public readonly record struct PalskinPalette(Vector4[] Registers)
{
    public static PalskinPalette Empty { get; } = new([]);

    public int StartRegister => Dx9VulkanShaderConstants.PaletteStartRegister;

    public int RegisterCount => Registers.Length;

    public bool IsEmpty => Registers.Length == 0;

    /// <summary>
    /// Group-order 3×4 rows. File blend bytes are register offsets
    /// into this bank, not mesh bone ids.
    /// </summary>
    public static PalskinPalette FromDest(Matrix4x4[] dest, ReadOnlySpan<byte> groupBones) =>
        new(WorldShading.PackSubsetRegisters(dest, groupBones));
}

/// <summary>
/// One C3D DIP. Local centimetre verts + instance W
/// (<c>009881F0</c> wrapper+496). PALSKIN adds dest <c>c38</c>
/// and <c>DrawIndexed</c>. Do not <c>Vector3.Transform</c> into
/// the landscape VB.
/// </summary>
public sealed class InstanceDraw
{
    public const uint StaticPassBit = 0x20;

    /// <summary>
    /// <c>00B33010</c> drains type-0 / type-1 first slots here
    /// (slots 8+10). Registration index 7, after <c>0x20</c>,
    /// before sky <c>0x2000</c>.
    /// </summary>
    public const uint PalskinPassBit100 = 0x100;

    /// <summary>
    /// Type-1 second slot (14). Registration index 25, after sky.
    /// </summary>
    public const uint PalskinPassBit80 = 0x80;

    public required uint MeshId { get; init; }
    public required int PrimitiveIndex { get; init; }
    public required InstanceKind Kind { get; init; }

    /// <summary>
    /// File-local centimetres. Static-lit native copies these
    /// with no matrix (<c>00BB2540</c>). PALSKIN VS skins them.
    /// </summary>
    public required MeshVertex[] LocalVertices { get; init; }

    /// <summary>
    /// PALSKIN file IB. Empty for first-seen static-lit
    /// (<c>DrawPrimitive</c> vtbl+400, no IB).
    /// </summary>
    public required ushort[] Indices { get; init; }

    /// <summary>
    /// Numerics last-row form of the instance 3×4
    /// (<c>ObjectTransform</c>). <c>clip = p_local * W * V * P</c>.
    /// </summary>
    public required Matrix4x4 World { get; init; }

    public required int TextureId { get; init; }

    public int TextureId1 { get; init; }

    public bool SrcAlphaBlend { get; init; }

    public uint PassBit { get; init; } = StaticPassBit;

    public PalskinPalette Palette { get; init; } = PalskinPalette.Empty;

    public byte[] GroupBones { get; init; } = [];

    public byte Flag1 { get; init; }

    public InstanceBufferKey BufferKey => new(MeshId, PrimitiveIndex, Kind);

    public InstanceMaterialKey MaterialKey => new(TextureId, SrcAlphaBlend, Kind);

    public MeshPipelineKey PipelineKey => new(SrcAlphaBlend);

    public TextureBindKey TextureBind => new(TextureId, TextureId1 == 0 ? TextureId : TextureId1);

    public float ShaderMode => ScenePasses.ShaderMode(SceneSubmit.Primitives);

    public int Rank => ScenePasses.Rank(PassBit);

    public bool NativeUsesIndexBuffer => Kind == InstanceKind.Palskin;

    public static InstanceDraw StaticLit(
        uint meshId,
        int primitiveIndex,
        MeshVertex[] localVertices,
        Matrix4x4 world,
        int textureId) =>
        new()
        {
            MeshId = meshId,
            PrimitiveIndex = primitiveIndex,
            Kind = InstanceKind.StaticLit,
            LocalVertices = localVertices,
            Indices = [],
            World = world,
            TextureId = textureId,
            SrcAlphaBlend = false,
            PassBit = StaticPassBit,
        };

    public static InstanceDraw Palskin(
        uint meshId,
        int primitiveIndex,
        MeshVertex[] localVertices,
        ushort[] indices,
        Matrix4x4 world,
        int textureId,
        PalskinPalette palette,
        byte[]? groupBones = null,
        byte flag1 = 0) =>
        new()
        {
            MeshId = meshId,
            PrimitiveIndex = primitiveIndex,
            Kind = InstanceKind.Palskin,
            LocalVertices = localVertices,
            Indices = indices,
            World = world,
            TextureId = textureId,
            SrcAlphaBlend = WorldShading.FirstSeenPalskinSrcAlphaBlend,
            PassBit = PalskinPassBit100,
            Palette = palette,
            GroupBones = groupBones ?? [],
            Flag1 = flag1,
        };
}
