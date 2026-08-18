namespace Fable.Render.Parity.Dx9Vulkan;

/// <summary>
/// Recovered Fable.exe DX9 constants for
/// the frontend submit path. Not a Vulkan
/// map. D3D defaults that were not written
/// first-seen stay unmarked as proven.
/// </summary>
public static class Dx9FrontendState
{
    public const uint SpriteRecordType = 0x22;
    public const uint SpriteRecordTypeAlt = 0x23;
    public const int SpriteRecordBytes = 0xC0;
    public const int SpriteDestOffset = 12;
    public const int SpriteTextureOffset = 32;
    public const int SpriteUv0Offset = 68;
    public const int SpriteUvV0Offset = 72;
    public const int SpriteUvU1Offset = 76;
    public const int SpriteUvV1Offset = 80;
    public const int SpriteInstanceDestOffset = 72;
    public const int SpriteInstanceU0Offset = 117;
    public const uint QuadFillFn = 0x00BB0970;
    public const uint TextureUvFn = 0x009FC810;
    public const bool UvVZeroAtDestTop = true;
    public const bool FlipsUvV = false;
    public const float TextureFullU0 = 0f;
    public const float TextureFullV0 = 0f;
    public const float TextureFullU1 = 1f;
    public const float TextureFullV1 = 1f;
    public const int SpriteInstanceBytes = 0x8C;
    public const uint SpriteInstanceVtbl = 0x012A54BC;
    public const uint SpriteHandlerVtbl = 0x012A5664;
    public const int SpriteFactoryType22 = 34;
    public const int SpriteFactoryType23 = 35;

    public const uint GlyphRecordType = 0x27;
    public const int GlyphRecordBytes = 64;
    public const int GlyphVertexStride = 28;
    public const int GlyphVertsPerQuad = 6;
    public const int GlyphTrisPerQuad = 2;
    public const int GlyphRhwOffset = 12;
    public const int GlyphDiffuseOffset = 16;
    public const int GlyphUvOffset = 20;

    public const int DisplayQueueRecordBytes = 60;
    public const int DisplayQueueBeginOffset = 16020;
    public const int DisplayQueueEndOffset = 16024;
    public const uint DisplayQueueCountMagic = 0x88888889;
    public const int DisplayVertexStride = 32;
    public const int DisplayVertexBufferOffset = 16008;

    public const int DrawPrimitiveVtbl = 324;
    public const int DrawPrimitiveUpVtbl = 332;
    public const int DrawIndexedPrimitiveUpVtbl = 336;
    public const int SetTextureVtbl = 260;
    public const int SetViewportVtbl = 188;
    public const int BeginSceneVtbl = 164;
    public const int EndSceneVtbl = 168;
    public const int ClearVtbl = 172;
    public const int PresentVtbl = 68;

    public const int D3dptPointList = 1;
    public const int D3dptLineList = 2;
    public const int D3dptLineStrip = 3;
    public const int D3dptTriangleList = 4;
    public const int D3dptTriangleStrip = 5;
    public const int D3dptTriangleFan = 6;
    public const int D3dfmtIndex16 = 101;

    public const int SpritePrimitiveType = D3dptTriangleList;
    public const int GlyphPrimitiveType = D3dptTriangleList;
    public const int DisplayQueuePrimA = D3dptLineList;
    public const int DisplayQueuePrimB = D3dptTriangleList;

    public const int SpriteVertexStride = 32;
    public const int SpriteUsedBytes = 28;
    public const int SpriteTextureStages = 2;

    public const string VertexShader = "VSHADER_2D_SPRITE";
    public const string VertexShaderBank = "SHADERS_POINT_SPRITE1";
    public const string ClockVertexShader = "VSHADER_2D_CLOCK_SPRITE";
    public const string PixelShader = "PSHADER_2D_CLOCK_SPRITE";
    public const string PixelShaderAdditive = "PSHADER_2D_CLOCK_SPRITE_ADDITIVE";
    public const string DisplayQueueVertexShader = "VSHADER_BBBLIB_2D";
    public const int VsPositionInput = 0;
    public const int VsDiffuseInput = 1;
    public const int VsTexcoordInput = 2;
    public const bool VsPassthroughOPos = true;

    public const int BlendOne = 2;
    public const int BlendInvSrcColor = 4;
    public const int BlendSrcAlpha = 5;
    public const int BlendInvSrcAlpha = 6;
    public const uint BlendTableOne = 0x01396F6C;
    public const uint BlendTableInvSrcColor = 0x01396F74;
    public const uint BlendTableSrcAlpha = 0x01396F78;
    public const uint BlendTableInvSrcAlpha = 0x01396F7C;
    public const int WidgetBlendDefault = 2;
    public const int HandlerBlendOffset = 164;
    public const int AlphaBlendEnableSlot = 10424;

    public const uint ColorScaleVa = 0x01231724;
    public const float ColorScale = 1f / 255f;
    public const uint HalfPixelVa = 0x0122F59C;
    public const float GlyphHalfPixel = 0.5f;
    public const uint HudNdcBiasVa = 0x0122DED8;
    public const float HudNdcBias = 1f;
    public const uint UvEpsilonVa = 0x0129BA3C;
    public const float UvEpsilon = 0.0001f;

    public const float ViewportMinZ = 0f;
    public const float ViewportMaxZ = 1f;
    public const uint ViewportMaxZBits = 0x3F800000;
    public const int DisplayDefaultWidth = 1024;
    public const int DisplayDefaultHeight = 768;

    public const uint ClearColorArgb = 0xFF000000;
    public const float ClearZ = 1f;
    public const int ClearStencil = 0;
    public const int ClearFlagsDefault = 7;
    public const int D3dClearTarget = 1;
    public const int D3dClearZBuffer = 2;
    public const int D3dClearStencil = 4;

    public const uint DeviceObjectVa = 0x013B8390;
    public const uint DisplayObjectVa = 0x013B8384;

    /// <summary>
    /// Matching FVF for v0/v1/v2
    /// (XYZRHW|DIFFUSE|TEX1). No
    /// recovered <c>SetFVF 0x144</c>
    /// write in <c>00BAE2D0</c>.
    /// </summary>
    public const uint InferredFvfXyzRhwDiffuseTex1 = 0x144;

    public static (int Src, int Dst) BlendFromHandlerMode(int mode) =>
        mode switch
        {
            3 => (BlendOne, BlendOne),
            4 => (BlendOne, BlendInvSrcColor),
            _ => (BlendSrcAlpha, BlendInvSrcAlpha),
        };

    public static int DisplayFlushPrimitive(bool usePrimA) =>
        usePrimA ? DisplayQueuePrimA : DisplayQueuePrimB;
}
