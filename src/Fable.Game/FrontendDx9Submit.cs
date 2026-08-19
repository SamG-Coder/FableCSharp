using System.Runtime.InteropServices;
using Fable.Dx9;
using Fable.Formats.Defs;
using Fable.Render;
using Fable.Render.Parity.Dx9Vulkan;

namespace Fable.Game;

/// <summary>
/// Native DX9 frontend submit records. Not a
/// Vulkan translation. Unread first-seen writes
/// stay null with the VA that was walked.
/// </summary>
public static class FrontendDx9Submit
{
    public const uint FrameFn = 0x0042DF9E;
    public const uint PackerFn = 0x0041BEB0;
    public const uint SiblingPackerFn = 0x0041BF60;
    public const uint WidgetDrawFn = 0x0041AFA0;
    public const uint EngineSubmitFn = 0x00B23BC0;
    public const uint DispatchFn = 0x00B324A0;
    public const uint SpriteFactoryFn = 0x00BACFD0;
    public const uint SpriteHandlerCtorFn = 0x00BAD040;
    public const uint SpriteDrawFn = 0x00BAE2D0;
    public const uint SpriteInstanceFn = 0x00BAD8A0;
    public const uint DisplayEnqueueFn = 0x009DB700;
    public const uint DisplayEnqueueWrapFn = 0x009DBFF0;
    public const uint HudStringEnqueueFn = 0x009DD8F0;
    public const uint Flush2dFn = 0x009D9C80;
    public const uint FlushLayersFn = 0x009DA9F0;
    public const uint StateFlushFn = 0x00A058C0;
    public const uint SpriteDipUpFn = 0x00A0AEA0;
    public const uint GlyphDrawFn = 0x00AB7C20;
    public const uint GlyphPrimitiveFn = 0x00A0ABE0;
    public const uint Type6WidgetDrawFn = 0x0054EF00;
    public const uint Type6PackerFn = 0x00543910;
    public const uint BeginSceneFn = 0x009BEF20;
    public const uint ClearFn = 0x009D8CF0;
    public const uint ClearDeviceFn = 0x009BE420;
    public const uint EndSceneFn = 0x009BEF50;
    public const uint PresentFn = 0x009BEEB0;
    public const uint SetViewportFn = 0x009BEF80;
    public const uint ViewportFromRectFn = 0x009BF490;
    public const uint PreClearViewportFn = 0x00A0B560;

    public const uint SpriteRecordType = 0x22;
    public const uint SpriteRecordTypeAlt = 0x23;
    public const int SpriteRecordBytes = 0xC0;
    public const int SpriteSubmitDestOffset = 0x15C;
    public const int SpriteSubmitVtbl = 92;
    public const int SpriteAltSubmitVtbl = 112;
    public const uint SpriteInstanceVtbl = 0x012A54BC;
    public const uint SpriteHandlerVtbl = 0x012A5664;
    public const int SpriteInstanceBytes = 0x8C;
    public const int SpriteDestCopyOffset = 72;
    public const int SpriteRecU0Offset = 68;
    public const int SpriteRecV0Offset = 72;
    public const int SpriteRecU1Offset = 76;
    public const int SpriteRecV1Offset = 80;
    public const int SpriteInstanceU0Offset = 117;
    public const int SpriteInstanceV0Offset = 121;
    public const int SpriteInstanceU1Offset = 125;
    public const int SpriteInstanceV1Offset = 129;
    public const int SpriteInstanceUvValidOffset = 133;
    public const uint QuadFillFn = 0x00BB0970;
    public const uint TextureUvFn = 0x009FC810;
    public const uint HandlerIndexInitFn = 0x00BAD040;
    public const uint UvEpsilonVa = 0x0129BA3C;
    public const float UvEpsilon = 0.0001f;
    public const uint TextureUvBiasVa = 0x0129C81C;
    public const float TextureUvOriginScale = 1f / 32768f;
    public const bool UvVZeroAtDestTop = true;
    public const bool FlipsUvV = false;
    public const bool AppliesHalfPixelOffset = false;
    public const bool PersistFlipU = false;
    public const bool PersistFlipV = false;
    public const float TextureFullU0 = 0f;
    public const float TextureFullV0 = 0f;
    public const float TextureFullU1 = 1f;
    public const float TextureFullV1 = 1f;
    public const int QuadTl = 0;
    public const int QuadTr = 1;
    public const int QuadBl = 2;
    public const int QuadBr = 3;

    public const uint GlyphRecordType = 0x27;
    public const int GlyphRecordBytes = 64;
    public const uint GlyphFaceHelperFn = 0x0054F4B0;

    /// <summary>
    /// Type-6 packs via <c>00543910</c>
    /// type <c>0x27</c> size 64.
    /// Type-0 stays <c>0041BEB0</c>
    /// type <c>0x22</c> size <c>0xC0</c>.
    /// </summary>
    public static (uint Packer, uint Type, int Bytes) RecordForWidget(int widgetType) =>
        widgetType == FrontendWidgetType.Text
            ? (Type6PackerFn, GlyphRecordType, GlyphRecordBytes)
            : (PackerFn, SpriteRecordType, SpriteRecordBytes);

    public const int DisplayQueueBeginOffset = 16020;
    public const int DisplayQueueEndOffset = 16024;
    public const int DisplayQueueRecordBytes = 60;
    public const uint DisplayQueueCountMagic = 0x88888889;
    public const int DisplayVertexBufferOffset = 16008;
    public const int Flush2dVertexBufferOffset = 15984;
    public const int Flush2dQueueOffset = 15996;

    /// <summary>
    /// First-seen dest after ctor is 0,0,0,0
    /// (<c>0041AFA0</c> leftover <c>+204</c>
    /// never written, Width=0).
    /// <c>00B324A0</c> constructs
    /// <c>00BACFD0</c> then calls dest+4
    /// vtbl+20 <c>00BAD8A0</c>, which
    /// early-outs at <c>00BADB36</c>.
    /// <c>009DB700</c> is not a callee.
    /// </summary>
    public static FrontendDx9SpriteRecord FirstSeenEmptyDest() =>
        new()
        {
            RecordType = SpriteRecordType,
            RecordBytes = SpriteRecordBytes,
            DestX0 = 0f,
            DestY0 = 0f,
            DestX1 = 0f,
            DestY1 = 0f,
            U0 = 0f,
            V0 = 0f,
            U1 = 0f,
            V1 = 0f,
            Packer = PackerFn,
            Factory = SpriteFactoryFn,
            InstanceSubmit = SpriteInstanceFn,
            HandlerDraw = SpriteDrawFn,
            EnqueuesDisplayQueue = false,
            CallsDraw = false,
            TextureId = 0,
            RecTextureOffset = 32,
            RecUv0Offset = 68,
        };

    /// <summary>
    /// Nonempty dest: <c>00BAD8A0</c>
    /// copies rec+12 → instance+72.
    /// Direct <c>E8 009DB700</c> is
    /// still absent. Draw is factory
    /// vtbl+20 <c>00BAE2D0</c> →
    /// <c>00A0AEA0</c>.
    /// </summary>
    public static FrontendDx9SpriteRecord NonemptyDest(
        float x0, float y0, float x1, float y1) =>
        new()
        {
            RecordType = SpriteRecordType,
            RecordBytes = SpriteRecordBytes,
            DestX0 = x0,
            DestY0 = y0,
            DestX1 = x1,
            DestY1 = y1,
            U0 = 0f,
            V0 = 0f,
            U1 = 0f,
            V1 = 0f,
            Packer = PackerFn,
            Factory = SpriteFactoryFn,
            InstanceSubmit = SpriteInstanceFn,
            HandlerDraw = SpriteDrawFn,
            EnqueuesDisplayQueue = false,
            CallsDraw = true,
            TextureId = null,
            RecTextureOffset = 32,
            RecUv0Offset = 68,
        };

    public static FrontendDx9GlyphRecord Type6Glyph() =>
        new()
        {
            RecordType = GlyphRecordType,
            RecordBytes = GlyphRecordBytes,
            WidgetDraw = Type6WidgetDrawFn,
            Packer = Type6PackerFn,
            FaceDraw = GlyphDrawFn,
            Primitive = GlyphPrimitiveFn,
            VertexStride = 28,
            PrimitiveType = 4,
            HalfPixel = 0.5f,
            Rhw = 1f,
            FaceName = "ENG_ARIAL_16",
        };

    public static FrontendDx9FrameRecord FrontendFrame() =>
        new()
        {
            Frame = FrameFn,
            Viewport = SetViewportFn,
            Clear = ClearFn,
            BeginScene = BeginSceneFn,
            Flush2d = Flush2dFn,
            FlushLayers = FlushLayersFn,
            FlushLayersArg = 1,
            EndScene = EndSceneFn,
            Present = PresentFn,
            ClearBeforeBeginScene = true,
            FlushPairCount = 2,
            ClearColorArgb = 0xFF000000,
            ClearFlagsArg = 0,
            ClearFlagsDefault = 7,
            ViewportMinZ = 0f,
            ViewportMaxZ = 1f,
        };

    public static int DisplayQueueCount(int begin, int end)
    {
        var bytes = end - begin;
        return bytes <= 0 ? 0 : bytes / DisplayQueueRecordBytes;
    }

    public static bool DisplayFlushShouldDip(int begin, int end) =>
        DisplayQueueCount(begin, end) != 0;

    /// <summary>
    /// <c>00BAD8A0</c> treats both UV
    /// corners as unused when each
    /// length² ≤ <c>0x129BA3C</c>².
    /// First-seen packer writes 0,0,0,0.
    /// </summary>
    public static bool RecUvDegenerate(float u0, float v0, float u1, float v1)
    {
        var eps2 = UvEpsilon * UvEpsilon;
        return u0 * u0 + v0 * v0 <= eps2 && u1 * u1 + v1 * v1 <= eps2;
    }

    /// <summary>
    /// Submitted corner UV after
    /// <c>00BB0970</c>. Rec +68..+80 is
    /// an offset added to the texture
    /// frame quad from <c>009FC810</c>.
    /// Degenerate rec (first-seen 0,0,0,0)
    /// leaves the frame UV. Texture-miss
    /// default <c>00BB0EE4</c> is
    /// 0,0,1,1 on TL-TR-BL-BR.
    /// No <c>1-v</c>. No persist FlipU/V.
    /// </summary>
    public static (float U0, float V0, float U1, float V1) SubmittedSpriteUv(
        float recU0, float recV0, float recU1, float recV1,
        float frameU0 = TextureFullU0,
        float frameV0 = TextureFullV0,
        float frameU1 = TextureFullU1,
        float frameV1 = TextureFullV1)
    {
        if (RecUvDegenerate(recU0, recV0, recU1, recV1))
            return (frameU0, frameV0, frameU1, frameV1);
        return (
            frameU0 + recU0,
            frameV0 + recV0,
            frameU1 + recU0,
            frameV1 + recV0);
    }

    /// <summary>
    /// <c>00BAD040</c> INDEX16 at
    /// handler+44: 0,1,2,1,3,2.
    /// </summary>
    public static readonly ushort[] QuadIndices = [0, 1, 2, 1, 3, 2];
    public const int SpriteUpNumVertices = 4;
    public const int SpriteUpPrimitiveCount = 2;
    public const int SpriteUpVertexStride = 32;
    public const int GlyphUpVertexStride = 28;
    public const int GlyphUpVertsPerQuad = 6;
    public const int GlyphUpPrimitiveCount = 2;
    public const int Index16Format = 101;
    public const int DipUpMinVertexIndex = 0;

    /// <summary>
    /// Shadow-record recovered sprite
    /// DIPUP and glyph user verts.
    /// Empty dest is <c>00BADB36</c>.
    /// Do not emit buffered DIP(0).
    /// Sprite <c>TextureId</c> and
    /// vertex diffuse stay UNREAD as
    /// native bank/fill; dest/UV/index
    /// words are the recovered subset.
    /// </summary>
    public static void IssueRecoveredDraws(
        IDirect3DDevice9 device,
        IReadOnlyList<FrontendDx9DrawRecord> records)
    {
        foreach (var rec in records)
        {
            if (rec.DestX1 <= rec.DestX0 || rec.DestY1 <= rec.DestY0)
                continue;
            if (rec.RecordType == (int)GlyphRecordType)
                IssueGlyphUp(device, rec);
            else
                IssueSpriteUp(device, rec);
        }
    }

    public static void IssueSpriteUp(IDirect3DDevice9 device, FrontendDx9DrawRecord rec)
    {
        var vertices = PackSpriteUpVertices(rec);
        var indices = PackQuadIndexBytes();
        device.DrawIndexedPrimitiveUP(
            Dx9PrimitiveType.TriangleList,
            DipUpMinVertexIndex,
            SpriteUpNumVertices,
            SpriteUpPrimitiveCount,
            indices,
            Index16Format,
            vertices,
            SpriteUpVertexStride);
    }

    public static void IssueGlyphUp(IDirect3DDevice9 device, FrontendDx9DrawRecord rec)
    {
        var vertices = PackGlyphUpVertices(rec);
        device.DrawPrimitiveUP(
            Dx9PrimitiveType.TriangleList,
            GlyphUpPrimitiveCount,
            vertices,
            GlyphUpVertexStride);
    }

    public static byte[] PackQuadIndexBytes()
    {
        var bytes = new byte[QuadIndices.Length * sizeof(ushort)];
        MemoryMarshal.AsBytes(QuadIndices.AsSpan()).CopyTo(bytes);
        return bytes;
    }

    public static byte[] PackSpriteUpVertices(FrontendDx9DrawRecord rec)
    {
        var verts = Dx9VulkanFrontend.BuildDx9Quad(rec);
        var bytes = new byte[verts.Length * SpriteUpVertexStride];
        MemoryMarshal.AsBytes(verts.AsSpan()).CopyTo(bytes);
        return bytes;
    }

    public static byte[] PackGlyphUpVertices(FrontendDx9DrawRecord rec)
    {
        var verts = Dx9VulkanFrontend.BuildDx9GlyphList(rec);
        var src = MemoryMarshal.AsBytes(verts.AsSpan());
        var bytes = new byte[verts.Length * GlyphUpVertexStride];
        for (var i = 0; i < verts.Length; i++)
            src.Slice(i * SpriteUpVertexStride, GlyphUpVertexStride)
                .CopyTo(bytes.AsSpan(i * GlyphUpVertexStride));
        return bytes;
    }
}

/// <summary>
/// Type <c>0x22</c> sprite. Dest pixels
/// are rec+12 / instance+72.
/// </summary>
public readonly record struct FrontendDx9SpriteRecord
{
    public uint RecordType { get; init; }
    public int RecordBytes { get; init; }
    public float DestX0 { get; init; }
    public float DestY0 { get; init; }
    public float DestX1 { get; init; }
    public float DestY1 { get; init; }
    public float U0 { get; init; }
    public float V0 { get; init; }
    public float U1 { get; init; }
    public float V1 { get; init; }
    public uint Packer { get; init; }
    public uint Factory { get; init; }
    public uint InstanceSubmit { get; init; }
    public uint HandlerDraw { get; init; }
    public bool EnqueuesDisplayQueue { get; init; }
    public bool CallsDraw { get; init; }
    public int RecTextureOffset { get; init; }
    public int RecUv0Offset { get; init; }

    /// <summary>
    /// rec+32 / instance+92. First-seen
    /// <c>0041BEB0</c> leaves 0.
    /// Nonempty texture id UNREAD as a
    /// concrete bank index at submit.
    /// </summary>
    public int? TextureId { get; init; }

    /// <summary>Per-vertex diffuse. UNREAD in 00BAE2D0 vertex fill.</summary>
    public uint? DiffuseArgb { get; init; }

    /// <summary>RHW written into sprite verts. UNREAD (00BAE2D0 +24 fill).</summary>
    public float? Rhw { get; init; }

    /// <summary>
    /// Half-pixel on sprite dest. No 0.5
    /// in <c>00BAD8A0</c> / <c>00BAE2D0</c>.
    /// <c>00BB0970</c> uses 0.5 as half
    /// dest size, not a UV write.
    /// </summary>
    public float? HalfPixel { get; init; }

    /// <summary>Sampler MAG/MIN/ADDRESS. UNREAD first-seen SetSamplerState.</summary>
    public int? SamplerMag { get; init; }

    /// <summary>D3DRS_ALPHATESTENABLE. UNREAD first-seen write.</summary>
    public bool? AlphaTest { get; init; }

    public bool DestEmpty => DestX1 <= DestX0 || DestY1 <= DestY0;
}

/// <summary>
/// Type-6 widget text. Packer
/// <c>00543910</c> writes type
/// <c>0x27</c>, not <c>0x22</c>.
/// GPU emit is <c>00AB7C20</c>.
/// </summary>
public readonly record struct FrontendDx9GlyphRecord
{
    public uint RecordType { get; init; }
    public int RecordBytes { get; init; }
    public uint WidgetDraw { get; init; }
    public uint Packer { get; init; }
    public uint FaceDraw { get; init; }
    public uint Primitive { get; init; }
    public int VertexStride { get; init; }
    public int PrimitiveType { get; init; }
    public float HalfPixel { get; init; }
    public float Rhw { get; init; }
    public string FaceName { get; init; }

    /// <summary>Sampler. UNREAD first-seen SetSamplerState on the atlas.</summary>
    public int? SamplerMag { get; init; }

    /// <summary>D3DRS_ALPHATESTENABLE. UNREAD.</summary>
    public bool? AlphaTest { get; init; }

    /// <summary>FVF SetFVF write. UNREAD. Used bytes are 28.</summary>
    public uint? Fvf { get; init; }
}

/// <summary>
/// <c>0042DF9E</c> frame wrapper.
/// Same Present as PlayAVI
/// (<c>009BEEB0</c>).
/// </summary>
public readonly record struct FrontendDx9FrameRecord
{
    public uint Frame { get; init; }
    public uint Viewport { get; init; }
    public uint Clear { get; init; }
    public uint BeginScene { get; init; }
    public uint Flush2d { get; init; }
    public uint FlushLayers { get; init; }
    public int FlushLayersArg { get; init; }
    public uint EndScene { get; init; }
    public uint Present { get; init; }
    public bool ClearBeforeBeginScene { get; init; }
    public int FlushPairCount { get; init; }
    public uint ClearColorArgb { get; init; }
    public int ClearFlagsArg { get; init; }
    public int ClearFlagsDefault { get; init; }
    public float ViewportMinZ { get; init; }
    public float ViewportMaxZ { get; init; }

    /// <summary>
    /// Backbuffer w/h live at device+404/+408
    /// (<c>009BEDC0</c>). PE default 1024×768
    /// is <see cref="EngineLifecycle.DisplayDefaultWidth"/>.
    /// First-seen store of those slots is the
    /// display create, not this frame.
    /// </summary>
    public int? ViewportWidth { get; init; }

    public int? ViewportHeight { get; init; }

    /// <summary>Scissor. UNREAD first-seen SetScissorRect.</summary>
    public bool? Scissor { get; init; }
}
