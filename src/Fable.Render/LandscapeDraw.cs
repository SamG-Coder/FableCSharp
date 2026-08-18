using System.Numerics;
using Fable.Formats.Levels;
using Fable.Formats.Scene;
using Fable.Formats.World;

namespace Fable.Render;

/// <summary>
/// Persistent identity of one 16 m landscape cell VB/IB.
/// Native stores these on the 72-byte cell (<c>+56</c>/<c>+52</c>)
/// at open. Not a Concat range into a scene soup.
/// </summary>
public readonly record struct LandscapeBufferKey(string Map, int CellX, int CellY);

/// <summary>
/// Stage-0/1 bind for one cell. FG: <see cref="MaskId"/> on t0,
/// <see cref="AlbedoId"/> on t1. BG samples t0 only.
/// </summary>
public readonly record struct LandscapeMaterialKey(int AlbedoId, int MaskId);

/// <summary>
/// Descriptor-set pair after the FG swap
/// (<c>DrawMeshBatches</c> set0=mask set1=albedo when mode 1).
/// </summary>
public readonly record struct TextureBindKey(int Set0, int Set1);

/// <summary>
/// First-seen mesh pipeline fork. Native has no Vk pipeline cache;
/// two host pipelines (opaque / PALSKIN SRCALPHA) are the
/// recovered RS pair. Fill / alpha-test / stencil stay UNREAD.
/// </summary>
public readonly record struct MeshPipelineKey(bool SrcAlphaBlend);

/// <summary>
/// One opened 16 m cell's stored tessellation. File-local /
/// region-local verts stay here. World is identity on host STB
/// (native <c>T(cam)</c> on a camera-relative VB).
/// </summary>
public sealed class LandscapeCellMesh
{
    public const int NativeVertexStrideBytes = LandscapeTextures.GpuVertexStrideBytes;
    public const uint RequiredFlagBit = 0x4;
    public const bool NativeIsTriangleStrip = true;

    public required string Map { get; init; }
    public required int CellX { get; init; }
    public required int CellY { get; init; }
    public required Vector3 AabbMin { get; init; }
    public required Vector3 AabbMax { get; init; }

    /// <summary>Cell <c>+60</c>. FG <c>00BF4570</c> needs bit <c>0x4</c>.</summary>
    public required uint Flags { get; init; }

    /// <summary>
    /// Decoded streams (pos / n / extra). Not native stride 24.
    /// Region-local. Do not <c>Vector3.Transform</c> by <c>T(cam)</c>.
    /// </summary>
    public required MeshVertex[] LocalVertices { get; init; }

    /// <summary>
    /// Stored strip. <c>Length == PrimitiveCount + 2</c> when the
    /// primary strip is present (<see cref="LandscapeStrip"/>).
    /// </summary>
    public required ushort[] StripIndices { get; init; }

    public required int PrimitiveCount { get; init; }
    public required int TextureId { get; init; }
    public required int TextureId1 { get; init; }

    public LandscapeBufferKey BufferKey => new(Map, CellX, CellY);

    public LandscapeMaterialKey MaterialKey =>
        new(TextureId, TextureId1 == 0 ? TextureId : TextureId1);

    public bool SubmitsForeground => (Flags & RequiredFlagBit) != 0;

    public int IndexCount =>
        LandscapeStrip.IndexCountFromPrimitiveCount(PrimitiveCount, StripIndices.Length > 0);
}

/// <summary>
/// One landscape DIP. Stored cell VB/IB is bit
/// <c>0x40</c> (<c>00BF4570</c>). Bit <c>0x4</c>
/// is tessellator BG (<c>00BF71D0</c>), not this
/// mesh. W = I on host STB.
/// </summary>
public readonly record struct LandscapeDraw(LandscapeCellMesh Cell, uint PassBit)
{
    public const uint BackgroundBit = 0x4;
    public const uint ForegroundBit = 0x40;

    public static Matrix4x4 HostWorld =>
        LandscapeFrustum.HostWorldSpaceLandscapeWorld();

    public bool SrcAlphaBlend => false;

    public MeshPipelineKey PipelineKey => new(false);

    public float ShaderMode => PassBit switch
    {
        BackgroundBit => ScenePasses.ShaderMode(SceneSubmit.LandscapeBit4),
        ForegroundBit => ScenePasses.ShaderMode(SceneSubmit.LandscapeBit40),
        _ => ScenePasses.ShaderMode(SceneSubmit.LandscapeBit40),
    };

    public TextureBindKey TextureBind
    {
        get
        {
            var albedo = Cell.TextureId;
            var mask = Cell.TextureId1 == 0 ? Cell.TextureId : Cell.TextureId1;
            return PassBit == ForegroundBit
                ? new TextureBindKey(mask, albedo)
                : new TextureBindKey(albedo, mask);
        }
    }

    public int Rank => ScenePasses.Rank(PassBit);

    public static LandscapeDraw Background(LandscapeCellMesh cell) => new(cell, BackgroundBit);

    public static LandscapeDraw Foreground(LandscapeCellMesh cell) => new(cell, ForegroundBit);

    /// <summary>
    /// FG cell DIP only. <c>BothPasses</c> used to
    /// emit the same VB on bit 4; that is
    /// <c>00BDC060</c> / <c>00BF71D0</c>, not
    /// <c>00BF4570</c>.
    /// </summary>
    public static LandscapeDraw[] BothPasses(LandscapeCellMesh cell) =>
        [Foreground(cell)];
}
