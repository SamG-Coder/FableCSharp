namespace Fable.Formats.Scene;

/// <summary>
/// One CRenderManager layer from Fable.exe <c>00B26A75</c>–<c>00B276A8</c>.
/// The frame (<c>00B25950</c>) walks <c>+348…+352</c> in this order.
/// </summary>
public readonly record struct ScenePass(uint Bit, SceneSubmit Submit);

public enum SceneSubmit
{
    None,
    Unread,
    LandscapeBit4,
    LandscapeBit40,
    SkyElse,
    Sky400000,
    Water,
    Shadows,
    Primitives,
    PalskinBit100,
}

public static class ScenePasses
{
    /// <summary>
    /// Registration order. Landscape <c>vtbl+16</c> only draws bits 4 and
    /// <c>0x40</c>. Sky else-path is <c>0x2000</c>. <c>0x2000000</c> is a
    /// no-op. Static meshes are submitted once on <c>0x20</c> (first
    /// MainScene+616 bit after landscape FG); the other +616 bits stay unread.
    /// </summary>
    public static readonly ScenePass[] Registration =
    [
        new(0x00000001, SceneSubmit.None),
        new(0x00000002, SceneSubmit.Shadows),
        new(0x00000004, SceneSubmit.LandscapeBit4),
        new(0x00000008, SceneSubmit.Unread),
        new(0x00000010, SceneSubmit.Unread),
        new(0x00000040, SceneSubmit.LandscapeBit40),
        new(0x00000020, SceneSubmit.Primitives),
        new(0x00000100, SceneSubmit.PalskinBit100),
        new(0x00000400, SceneSubmit.Unread),
        new(0x00001000, SceneSubmit.Unread),
        new(0x00002000, SceneSubmit.SkyElse),
        new(0x00004000, SceneSubmit.Unread),
        new(0x00008000, SceneSubmit.Unread),
        new(0x00020000, SceneSubmit.Water),
        new(0x00100000, SceneSubmit.Unread),
        new(0x08000000, SceneSubmit.Unread),
        new(0x10000000, SceneSubmit.Unread),
        new(0x00010000, SceneSubmit.Unread),
        new(0x00040000, SceneSubmit.Unread),
        new(0x00000800, SceneSubmit.Unread),
        new(0x00080000, SceneSubmit.Unread),
        new(0x00200000, SceneSubmit.Unread),
        new(0x00400000, SceneSubmit.Sky400000),
        new(0x00800000, SceneSubmit.Unread),
        new(0x02000000, SceneSubmit.None),
        new(0x00000080, SceneSubmit.Unread),
        new(0x00000200, SceneSubmit.Unread),
        new(0x04000000, SceneSubmit.Unread),
        new(0x01000000, SceneSubmit.Unread),
        new(0x08000000, SceneSubmit.Unread),
        new(0x10000000, SceneSubmit.Unread),
        new(0x20000000, SceneSubmit.Unread),
        new(0x40000000, SceneSubmit.Unread),
        new(0x80000000, SceneSubmit.Unread),
    ];

    public static int Rank(uint bit)
    {
        for (var i = 0; i < Registration.Length; i++)
        {
            if (Registration[i].Bit == bit)
                return i;
        }

        return int.MaxValue;
    }

    public static bool Draws(SceneSubmit submit) =>
        submit is SceneSubmit.LandscapeBit4 or SceneSubmit.LandscapeBit40
            or SceneSubmit.SkyElse or SceneSubmit.Primitives
            or SceneSubmit.PalskinBit100;

    public static float ShaderMode(SceneSubmit submit) => submit switch
    {
        SceneSubmit.LandscapeBit4 => 0f,
        SceneSubmit.LandscapeBit40 => 1f,
        SceneSubmit.SkyElse => 2f,
        SceneSubmit.Primitives => 3f,
        SceneSubmit.PalskinBit100 => 3f,
        _ => 1f,
    };

    public static IReadOnlyList<ScenePass> DrawnPasses(Meshes.SceneLayer layer)
    {
        var submit = layer switch
        {
            Meshes.SceneLayer.Landscape => (SceneSubmit[]) [SceneSubmit.LandscapeBit4, SceneSubmit.LandscapeBit40],
            Meshes.SceneLayer.Sky => [SceneSubmit.SkyElse],
            Meshes.SceneLayer.Palskin => [SceneSubmit.PalskinBit100],
            _ => [SceneSubmit.Primitives],
        };
        return Registration.Where(p => submit.Contains(p.Submit)).ToArray();
    }

    /// <summary>
    /// First-seen bits recovered for New Game Oakvale. Unread
    /// registration entries stay Unread — they are not flattened
    /// into one opaque pass.
    /// </summary>
    public static IReadOnlyList<FirstSeenLayerContract> FirstSeenLayers { get; } =
    [
        new(0x4, SceneSubmit.LandscapeBit4, "landscape background",
            "VSHADER_LANDSCAPE_FOREGROUND family slot 0",
            "PSHADER_LANDSCAPE_BACKGROUND mul_x2 t0*v0",
            "LESSEQUAL", "off", "CCW", "VS oFog",
            "T(cam) native / identity host STB", "landscape"),
        new(0x40, SceneSubmit.LandscapeBit40, "landscape foreground",
            "VSHADER_LANDSCAPE_FOREGROUND",
            "PSHADER_LANDSCAPE_FOREGROUND mul_x2 t1*v0",
            "LESSEQUAL", "off", "CCW", "VS oFog",
            "T(cam) native / identity host STB", "landscape"),
        new(0x20, SceneSubmit.Primitives, "static + PALSKIN",
            "VSHADER_STATIC_DIRLIGHT_FOG / VSHADER_PALSKIN_DIRLIGHT_FOG",
            "PSHADER_TEXTURE_DIFFUSE",
            "LESSEQUAL", "off; PALSKIN SRCALPHA/INVSRCALPHA", "CCW inherit",
            "VS oFog", "identity W", "static / PALSKIN"),
        new(0x2000, SceneSubmit.SkyElse, "inner sky else-path",
            "VSHADER_INNER_SKY dp4 oPos v0 c5-c8",
            "PSHADER_INNER_SKY or _SIMPLE; PS c0/c1/c2 UNREAD",
            "MinZ 0.99 MaxZ 1", "UNREAD", "CCW", "UNREAD",
            "identity W; sky P 100/10000", "sky"),
    ];
}

public readonly record struct FirstSeenLayerContract(
    uint Bit,
    SceneSubmit Submit,
    string Contents,
    string VertexShader,
    string PixelShader,
    string Depth,
    string Blend,
    string Cull,
    string Fog,
    string WorldTransform,
    string Family);

/// <summary>
/// D3D9 render-state numbers the first-seen landscape / static-lit paths
/// apply through device wrapper <c>0x1436E18</c> slot <c>+10384</c>.
/// </summary>
public static class D3dDeviceState
{
    /// <summary>D3DRS_CULLMODE. Written at <c>00A047B8</c> as <c>0x16</c>.</summary>
    public const int CullMode = 22;

    /// <summary>
    /// Dword at <c>0x01396FB0</c>. Landscape <c>00B24BF7</c> and static-lit
    /// <c>00BB2DA2</c> copy it onto slot <c>+10388</c>. <c>3</c> is
    /// <c>D3DCULL_CCW</c>. <c>0x01396FB8</c> is <c>1</c> (NONE) for other
    /// primitive passes that apply it unconditionally and then restore
    /// CCW after the draw. C3D Flag1 is not the first-seen NONE selector
    /// (<see cref="Fable.Formats.WorldShading.FirstSeenAppliesCullNoneFromFlag1"/>).
    /// </summary>
    public const int CullCcw = 3;

    public const int CullNone = 1;
    public const int CullCw = 2;
    public const uint CullTable = 0x01396FB0;
    public const uint CullTableNone = 0x01396FB8;

    /// <summary>
    /// Wrapper ctor <c>00A04630</c> <c>mov edx, 1</c> then
    /// every RS slot including CULLMODE <c>+10384</c>
    /// <c>[+17]=dl</c>. Flush <c>00A044E0</c> decs that
    /// type: 1 → <c>IDirect3DDevice9::SetRenderState</c>
    /// <c>[vtbl+228]</c> with <c>[slot+12]</c> (RS 22)
    /// and <c>[slot+4]</c> (first-seen 3).
    /// </summary>
    public const int FlushTypeRenderState = 1;
    public const int FlushTypeOffset = 17;
    public const int FlushRsOffset = 12;
    public const int SetRenderStateVtbl = 228;
    public const uint SlotCtor = 0x00A04630;
    public const uint SlotFlush = 0x00A044E0;
    public const bool FirstSeenCullFlushIsSetRenderState = true;

    /// <summary>
    /// D3DRS_SPECULARENABLE. Slot init <c>00A04B44</c> writes
    /// <c>0x1D</c> at wrapper <c>+10736</c>; slot base
    /// <c>+10724</c>, value <c>+10728</c>. PALSKIN bind
    /// <c>00BD3070</c> at <c>00BD30AF</c> sets the value to 1.
    /// First-seen VS families do not write <c>oD1</c>, so the
    /// FFP specular addend stays 0.
    /// </summary>
    public const int SpecularEnable = 29;
    public const int SpecularEnableSlot = 10724;
    public const int FirstSeenPalskinSpecularEnable = 1;

    /// <summary>
    /// Layer switch <c>00BBC130</c>: <c>type = [obj+8]</c>,
    /// <c>index = type-7</c>, byte table <c>0x00BBC2EC</c>,
    /// jump <c>0x00BBC2D8</c>. Type <c>20</c> is table byte 1
    /// → <c>00BBC1DB</c> → NONE-draw <c>00BBE090</c>.
    /// Flag1 does not write this type on first-seen hair/3180
    /// (<see cref="Fable.Formats.WorldShading.FirstSeenFlag1WritesLayerType20"/>).
    /// </summary>
    public const int PrimitiveTypeNoneDraw = 20;

    public static bool PrimitiveTypeUsesNoneDraw(int type) =>
        type == PrimitiveTypeNoneDraw;

    /// <summary>D3DRS_SRCBLEND. Slot init <c>00A047EA</c> writes <c>0x13</c>.</summary>
    public const int SrcBlend = 19;

    /// <summary>D3DRS_DESTBLEND. Slot init <c>00A04825</c> writes <c>0x14</c>.</summary>
    public const int DestBlend = 20;

    /// <summary>D3DRS_ALPHABLENDENABLE. Slot init <c>00A04860</c> writes <c>0x1B</c>.</summary>
    public const int AlphaBlendEnable = 27;

    /// <summary>D3DRS_ALPHATESTENABLE. Slot init <c>00A0489B</c> writes <c>0x0F</c>.</summary>
    public const int AlphaTestEnable = 15;

    /// <summary>
    /// D3DRS_FOGENABLE. Slot init <c>00A0495C</c> writes <c>0x1C</c>
    /// at wrapper <c>+10576</c>; slot base <c>+10564</c>, value
    /// <c>+10568</c>. Ctor <c>00A04654</c> <c>xor eax,eax</c> so
    /// default 0. Setter <c>00B46890</c> writes 1. First-seen
    /// landscape <c>00B67480</c> and MainScene <c>00B32AD0</c> (bits
    /// 4 and 0x40) both call it.
    /// </summary>
    public const int FogEnable = 28;
    public const int FogEnableSlot = 10564;
    public const int FogEnableValueOffset = 10568;
    public const int FirstSeenFogEnable = 1;
    public const uint FogEnableSetter = 0x00B46890;
    public const uint FogEnableLandscape = 0x00B67480;
    public const uint FogEnableMainScene = 0x00B32AD0;

    /// <summary>
    /// D3DRS_FOGCOLOR. Slot <c>+10584</c>, RS at <c>+10596</c> =
    /// <c>0x22</c>. <c>00B47630</c> packs record <c>+64..+76</c> *
    /// 255 as ARGB. First-seen (0,0,0,1) → <c>0xFF000000</c>.
    /// </summary>
    public const int FogColor = 34;
    public const int FogColorSlot = 10584;
    public const uint FirstSeenFogColorArgb = 0xFF000000;

    /// <summary>D3DRS_FOGTABLEMODE. Slot <c>+10664</c>, default 0 (NONE).</summary>
    public const int FogTableMode = 35;
    public const int FogTableModeSlot = 10664;
    public const int FirstSeenFogTableMode = 0;

    /// <summary>D3DRS_FOGVERTEXMODE. Slot <c>+10684</c>, default 0 (NONE).</summary>
    public const int FogVertexMode = 140;
    public const int FogVertexModeSlot = 10684;
    public const int FirstSeenFogVertexMode = 0;

    public const int BlendZero = 1;
    public const int BlendOne = 2;
    public const int BlendSrcAlpha = 5;
    public const int BlendInvSrcAlpha = 6;

    /// <summary>
    /// PALSKIN bind <c>00BD3070</c> pass 4 falls through
    /// <c>00BD35ED</c> → <c>00BD3867</c> <c>[0x01396F78]=5</c>
    /// SRCALPHA and <c>00BD38D4</c> <c>[0x01396F7C]=6</c>
    /// INVSRCALPHA. <c>+10424</c> alphablend enable is 1.
    /// No Flag1 test.
    /// </summary>
    public const int FirstSeenPalskinSrcBlend = BlendSrcAlpha;
    public const int FirstSeenPalskinDestBlend = BlendInvSrcAlpha;

    /// <summary>D3DRS_ZENABLE. D3D default TRUE.</summary>
    public const int ZEnable = 7;

    /// <summary>D3DRS_ZWRITEENABLE. D3D default TRUE.</summary>
    public const int ZWriteEnable = 14;

    /// <summary>D3DRS_ZFUNC. D3D default / first-seen LESSEQUAL.</summary>
    public const int ZFunc = 23;

    /// <summary>D3DCMP_LESSEQUAL.</summary>
    public const int CmpLessEqual = 4;

    /// <summary>
    /// First-seen lock: landscape / static-lit consume
    /// D3DCMP_LESSEQUAL. The SetRenderState site for
    /// ZENABLE/ZWRITE is UNREAD; D3D defaults are TRUE.
    /// </summary>
    public const int FirstSeenZFunc = CmpLessEqual;
    public const int FirstSeenZEnable = 1;
    public const int FirstSeenZWriteEnable = 1;

    /// <summary>D3DRS_FILLMODE. First-seen write UNREAD.</summary>
    public const int FillMode = 8;
    public const int FillSolid = 3;

    /// <summary>D3DRS_COLORWRITEENABLE. First-seen write UNREAD.</summary>
    public const int ColorWriteEnable = 168;
    public const int ColorWriteAll = 0xF;
}
