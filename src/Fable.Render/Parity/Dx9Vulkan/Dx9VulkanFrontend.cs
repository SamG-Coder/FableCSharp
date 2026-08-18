using System.Numerics;
using System.Runtime.InteropServices;
using Fable.Formats.Scene;
using Silk.NET.Vulkan;

namespace Fable.Render.Parity.Dx9Vulkan;

/// <summary>
/// Explicit DX9 → Vulkan table for frontend
/// sprites and type-6 text. Every pipeline
/// field cites a recovered Fable.exe write
/// or is marked TEMPORARY (D3D default /
/// unread first-seen).
/// </summary>
public static class Dx9VulkanFrontend
{
    // 0041BEB0 type 0x22 dest +0x15C size 0xC0.
    // 00B23BC0 → 00B324A0. First dest+4=0:
    // 00BACFD0 + 00BAE2D0. Later dest nonempty:
    // 00BAD8A0 copies rec+12 → instance+72.
    public const uint RecordType = 0x22;
    public const int RecordBytes = 0xC0;
    public const int SubmitDestOffset = 0x15C;
    public const uint PackerFn = 0x0041BEB0;
    public const uint SubmitFn = 0x00B23BC0;
    public const uint DispatchFn = 0x00B324A0;
    public const uint SpriteFactoryFn = 0x00BACFD0;
    public const uint SpriteSubmitFn = 0x00BAE2D0;
    public const uint InstanceSubmitFn = 0x00BAD8A0;
    public const uint HandlerCtorFn = 0x00BAD040;
    public const uint EnqueueFn = 0x009DB700;
    public const uint FlushLayersFn = 0x009DA9F0;
    public const uint StateFlushFn = 0x00A058C0;
    public const uint DrawUpFn = 0x00A0AEA0;
    public const uint SetViewportFn = 0x009BEF80;
    public const uint BeginSceneFn = 0x009BEF20;
    public const uint ClearFn = 0x009D8CF0;
    public const uint EndSceneFn = 0x009BEF50;
    public const uint PresentFn = 0x009BEEB0;
    public const uint PlayAviBlitFn = 0x009DC870;

    public const string VertexShaderName = "VSHADER_2D_SPRITE";
    public const string VertexShaderBank = "SHADERS_POINT_SPRITE1";
    public const string PixelShaderName = "PSHADER_2D_TEXTURE_DIFFUSE";
    public const string PixelShaderBank = "PIXEL_SHADERS";

    // VSHADER_2D_SPRITE (shaders.big):
    //   mov oPos, v0
    //   mul r0, v0, c92.xyyy
    //   add oT1.xy, r0, c92.zwww
    //   mov oT1.zw, c0.y
    //   mov oT0, v2
    //   mov oD0, v1
    // v0 = XYZRHW, v1 = DIFFUSE, v2 = TEX1.
    public const int VsPositionInput = 0;
    public const int VsDiffuseInput = 1;
    public const int VsTexcoordInput = 2;
    public const bool VsPassthroughOPos = true;

    // 00BAE2D0 push 32; 009DA9F0 push 32;
    // 009DB810 add [+15960], 32; RHW 1.0
    // at +12 (0x3F800000).
    public const int VertexStride = 32;
    public const float RecoveredRhw = 1f;
    public const uint FvfXyzRhwDiffuseTex1 = FrontendDx9Vertex.FvfXyzRhwDiffuseTex1;

    // 00A0AEA0 DrawIndexedPrimitiveUP
    // vtbl+336: push 4 (D3DPT_TRIANGLELIST),
    // push 0 MinVertex, push 101
    // (D3DFMT_INDEX16). The 00BAE2D0
    // push 2 is SetTexture stage count,
    // not the prim type.
    public const int DrawIndexedPrimitiveUpVtbl = 336;
    public const int DrawPrimitiveUpVtbl = 332;
    public const int SetTextureVtbl = 260;
    public const int D3dptTriangleList = 4;
    public const int D3dptLineList = 2;
    public const int D3dfmtIndex16 = 101;
    public const int FrontendTextureStages = 2;

    // 009DA9F0 nonempty: 00A058C0 then
    // DrawPrimitiveUP vtbl+332. [esp+16]
    // set → prim 2 (LINELIST); clear →
    // prim 4 (TRIANGLELIST). Frontend
    // type 0x22 does not fill +16020
    // first-seen (A-dx9-submit).
    public const int DisplayQueuePrimList = D3dptLineList;
    public const int DisplayQueuePrimTris = D3dptTriangleList;

    // 009BEF80 vtbl+188: 1024×768 MinZ 0
    // MaxZ 1. Same constants as
    // EngineLifecycle.DisplayDefault*.
    public const int DisplayWidth = 1024;
    public const int DisplayHeight = 768;
    public const float ViewportMinZ = 0f;
    public const float ViewportMaxZ = 1f;
    public const int SetViewportVtbl = 188;

    // 00BAE2D0 [ebp+312] after sub 3:
    //   ==3 → SRC/DST [0x1396F6C]=2 ONE
    //   ==4 → SRC [0x1396F6C]=2 ONE,
    //         DST [0x1396F74]=4 INVSRCCOLOR
    //   else → SRC [0x1396F78]=5 SRCALPHA,
    //          DST [0x1396F7C]=6 INVSRCALPHA
    // Widget ctor default +372=2 takes
    // the else branch. +10424 alphablend=1.
    public const int HandlerBlendOffset = 312;
    public const int WidgetBlendDefault = 2;
    public const int HandlerBlendAdditive = 3;
    public const int HandlerBlendInvSrcColor = 4;
    public const int BlendTableOne = 0x01396F6C;
    public const int BlendTableInvSrcColor = 0x01396F74;
    public const int BlendTableSrcAlpha = 0x01396F78;
    public const int BlendTableInvSrcAlpha = 0x01396F7C;
    public const int AlphaBlendEnableSlot = 10424;

    // 009DB810 HUD/PlayAVI VB fill uses
    // [0x122DED8]=1.0, not 0.5. Frontend
    // 00BAE2D0 +24 filler is UNREAD.
    // Half-pixel: UNREAD. Do not invent.
    public const uint HudNdcBiasVa = 0x0122DED8;
    public const float HudNdcBias = 1f;
    public const bool AppliesHalfPixelOffset = false;

    // 009DC870 2D dest has v=0 at dest
    // top (video shader comment). No UV
    // V flip for frontend sprites.
    public const bool UvVZeroAtDestTop = true;
    public const bool FlipsUvV = false;

    // Scissor / ALPHATESTENABLE first-seen
    // writes UNREAD. Do not invent.
    public const bool AppliesScissor = false;
    public const bool FirstSeenAlphaTest = false;

    // 00BAE2D0 writes 0 into +10324 /
    // +10344. Slot RS numbers PARTIAL.
    // 2D overlay does not consume the
    // 3D depth contract. TEMPORARY.
    public const bool TemporaryDepthTest = false;
    public const bool TemporaryDepthWrite = false;

    // Sampler MAG/MIN/MIP/ADDRESS UNREAD
    // (same as Dx9VulkanSamplerState).
    public static SamplerCreateInfo TemporarySampler =>
        Dx9VulkanSamplerState.FirstSeenTemporary();

    public static PrimitiveTopology MapPrimitive(int d3dPrimitiveType) =>
        d3dPrimitiveType switch
        {
            D3dptLineList => PrimitiveTopology.LineList,
            3 => PrimitiveTopology.LineStrip,
            D3dptTriangleList => PrimitiveTopology.TriangleList,
            5 => PrimitiveTopology.TriangleStrip,
            6 => PrimitiveTopology.TriangleFan,
            _ => PrimitiveTopology.TriangleList,
        };

    public static PrimitiveTopology FrontendTopology =>
        MapPrimitive(D3dptTriangleList);

    public static (int Src, int Dst) BlendFromHandlerMode(int handlerMode) =>
        handlerMode switch
        {
            HandlerBlendAdditive =>
                (D3dDeviceState.BlendOne, D3dDeviceState.BlendOne),
            HandlerBlendInvSrcColor =>
                (D3dDeviceState.BlendOne, 4),
            _ => (
                D3dDeviceState.FirstSeenPalskinSrcBlend,
                D3dDeviceState.FirstSeenPalskinDestBlend),
        };

    /// <summary>
    /// D3DBLEND → Vulkan. 4 is
    /// INVSRCCOLOR from <c>0x1396F74</c>
    /// (handler mode 4). World-path
    /// <see cref="Dx9VulkanBlendState.ColorFactor"/>
    /// does not see that opcode.
    /// </summary>
    public static BlendFactor ColorFactor(int d3dBlend) => d3dBlend switch
    {
        D3dDeviceState.BlendZero => BlendFactor.Zero,
        D3dDeviceState.BlendOne => BlendFactor.One,
        3 => BlendFactor.SrcColor,
        4 => BlendFactor.OneMinusSrcColor,
        D3dDeviceState.BlendSrcAlpha => BlendFactor.SrcAlpha,
        D3dDeviceState.BlendInvSrcAlpha => BlendFactor.OneMinusSrcAlpha,
        _ => BlendFactor.One,
    };

    public static PipelineColorBlendAttachmentState MapBlend(int handlerMode)
    {
        var (src, dst) = BlendFromHandlerMode(handlerMode);
        return new()
        {
            BlendEnable = true,
            SrcColorBlendFactor = ColorFactor(src),
            DstColorBlendFactor = ColorFactor(dst),
            ColorBlendOp = Dx9VulkanBlendState.FirstSeenBlendOp,
            SrcAlphaBlendFactor = ColorFactor(src),
            DstAlphaBlendFactor = ColorFactor(dst),
            AlphaBlendOp = Dx9VulkanBlendState.FirstSeenBlendOp,
            ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit |
                             ColorComponentFlags.BBit | ColorComponentFlags.ABit,
        };
    }

    public static PipelineColorBlendAttachmentState DefaultSpriteBlend =>
        MapBlend(WidgetBlendDefault);

    public static Viewport MapViewport(
        int x, int y, int width, int height) =>
        new()
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            MinDepth = ViewportMinZ,
            MaxDepth = ViewportMaxZ,
        };

    public static Viewport FirstSeenViewport =>
        MapViewport(0, 0, DisplayWidth, DisplayHeight);

    /// <summary>
    /// D3D viewport inverse so
    /// <c>mov oPos, v0</c> lands on dest
    /// pixels of <c>009BEF80</c>. Then
    /// <see cref="Dx9VulkanProjection.NdcYSign"/>
    /// (−1) matches D3D clip Y-up to
    /// Vulkan NDC Y-down. Not a Fable
    /// 2D Y-flip write. Half-pixel is
    /// not applied.
    /// </summary>
    public static Vector4 DestPixelToDx9Clip(
        float x, float y, float z,
        int vpX, int vpY, int vpW, int vpH)
    {
        if (vpW <= 0 || vpH <= 0)
            return new Vector4(0f, 0f, z, RecoveredRhw);
        var clipX = 2f * (x - vpX) / vpW - 1f;
        var clipY = 1f - 2f * (y - vpY) / vpH;
        return new Vector4(clipX, clipY, z, RecoveredRhw);
    }

    public static Vector4 Dx9ClipToVulkanNdc(Vector4 clip)
    {
        var ndc = Dx9VulkanProjection.ToNdc(clip);
        return ndc with { Y = ndc.Y * Dx9VulkanProjection.NdcYSign };
    }

    public static Vector4 DestPixelToVulkanNdc(
        float x, float y, float z,
        int vpX, int vpY, int vpW, int vpH) =>
        Dx9ClipToVulkanNdc(DestPixelToDx9Clip(x, y, z, vpX, vpY, vpW, vpH));

    public static FrontendDx9Vertex[] BuildDx9Quad(FrontendDx9DrawRecord rec)
    {
        var argb = rec.DiffuseArgb == 0 ? 0xFFFFFFFFu : rec.DiffuseArgb;
        return
        [
            new(rec.DestX0, rec.DestY0, 0f, RecoveredRhw, argb, rec.U0, rec.V0),
            new(rec.DestX1, rec.DestY0, 0f, RecoveredRhw, argb, rec.U1, rec.V0),
            new(rec.DestX0, rec.DestY1, 0f, RecoveredRhw, argb, rec.U0, rec.V1),
            new(rec.DestX1, rec.DestY1, 0f, RecoveredRhw, argb, rec.U1, rec.V1),
        ];
    }

    /// <summary>
    /// INDEX16 list for one dest quad.
    /// 00A0AEA0 prim 4. Winding TL-TR-BL
    /// / TR-BR-BL in dest space.
    /// </summary>
    public static readonly ushort[] QuadIndices = [0, 1, 2, 1, 3, 2];

    public static FrontendGpuVertex ToGpuVertex(
        FrontendDx9Vertex src,
        int vpX, int vpY, int vpW, int vpH)
    {
        var ndc = DestPixelToVulkanNdc(src.X, src.Y, src.Z, vpX, vpY, vpW, vpH);
        return new FrontendGpuVertex(ndc, Dx9VulkanColor.FromD3dArgb(src.DiffuseArgb),
            new Vector2(src.U, src.V));
    }

    public static FrontendDraw BuildDraw(
        FrontendDx9DrawRecord rec, uint firstVertex, uint firstIndex)
    {
        var (src, dst) = BlendFromHandlerMode(rec.HandlerBlendMode);
        return new FrontendDraw(
            rec.TextureId,
            firstVertex,
            4,
            firstIndex,
            (uint)QuadIndices.Length,
            src,
            dst,
            BlendEnable: true,
            D3dptTriangleList);
    }

    /// <summary>
    /// Vulkan-ready vertex / index /
    /// blend from one type <c>0x22</c>
    /// dest record. Empty dest
    /// (<c>00BAD8A0</c> / first-seen
    /// 0,0,0,0) yields no primitives.
    /// </summary>
    public static void AppendRecord(
        FrontendDx9DrawRecord rec,
        int vpX, int vpY, int vpW, int vpH,
        List<FrontendGpuVertex> vertices,
        List<ushort> indices,
        List<FrontendDraw> draws)
    {
        if (rec.DestX1 <= rec.DestX0 || rec.DestY1 <= rec.DestY0)
            return;
        var firstVertex = (uint)vertices.Count;
        var firstIndex = (uint)indices.Count;
        foreach (var dx9 in BuildDx9Quad(rec))
            vertices.Add(ToGpuVertex(dx9, vpX, vpY, vpW, vpH));
        var @base = (ushort)firstVertex;
        foreach (var i in QuadIndices)
            indices.Add((ushort)(@base + i));
        draws.Add(BuildDraw(rec, firstVertex, firstIndex));
    }

    public static FrontendSubmitBatch BuildBatch(
        IReadOnlyList<FrontendDx9DrawRecord> records,
        IReadOnlyList<GpuTexture> textures,
        int vpX = 0,
        int vpY = 0,
        int vpW = DisplayWidth,
        int vpH = DisplayHeight)
    {
        var vertices = new List<FrontendGpuVertex>();
        var indices = new List<ushort>();
        var draws = new List<FrontendDraw>();
        foreach (var rec in records)
            AppendRecord(rec, vpX, vpY, vpW, vpH, vertices, indices, draws);
        return new FrontendSubmitBatch(
            [.. vertices],
            [.. indices],
            [.. draws],
            [.. textures],
            vpX,
            vpY,
            vpW,
            vpH,
            ViewportMinZ,
            ViewportMaxZ);
    }

    public static VertexInputBindingDescription VertexBinding => new()
    {
        Binding = 0,
        Stride = FrontendGpuVertex.Stride,
        InputRate = VertexInputRate.Vertex,
    };

    public static VertexInputAttributeDescription[] VertexAttributes =>
    [
        new()
        {
            Location = 0,
            Binding = 0,
            Format = Format.R32G32B32A32Sfloat,
            Offset = 0,
        },
        new()
        {
            Location = 1,
            Binding = 0,
            Format = Format.R32G32B32A32Sfloat,
            Offset = FrontendGpuVertex.ColorOffset,
        },
        new()
        {
            Location = 2,
            Binding = 0,
            Format = Format.R32G32Sfloat,
            Offset = FrontendGpuVertex.UvOffset,
        },
    ];

    public static PipelineDepthStencilStateCreateInfo TemporaryDepthOff() =>
        new()
        {
            SType = StructureType.PipelineDepthStencilStateCreateInfo,
            DepthTestEnable = TemporaryDepthTest,
            DepthWriteEnable = TemporaryDepthWrite,
            DepthCompareOp = Dx9VulkanDepth.FirstSeenCompareOp,
        };

    public static Format SampledFormat => Dx9VulkanTextureFormat.SampledFormat;

    public static int NativeVertexSize => Marshal.SizeOf<FrontendDx9Vertex>();
}
