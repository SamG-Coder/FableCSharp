using System.Numerics;

namespace Fable.Formats;

/// <summary>
/// First-seen dirlight from lighting-manager ctor <c>00B482A0</c> +
/// apply <c>00F39D40</c> + LayoutLights <c>00BDB400</c>.
/// <c>[+84]=19</c>, <c>[+76]=2</c> so light 0 is <c>c19</c>/<c>c20</c>.
/// <c>[+96]=35</c>, <c>[+100]=1</c>. TOD bytes at ctor are 0 so record 0
/// is copied. <c>c35</c> is the VS MAD addend, not a LIT source.
/// Fog is VS <c>oFog</c> from <c>c2</c>/<c>c18</c>; far 7000 is SKY_DEF.
/// </summary>
public static class WorldShading
{
    public const int DirLightStartRegister = 19;
    public const int RegistersPerLight = 2;
    public const int LitRegister = 35;
    /// <summary>
    /// LayoutLights <c>[+20]</c>. All 11 <c>E8</c> callers of
    /// <c>009896D0</c> push offset 1, 4, 5, 8, or 16 — never 0.
    /// First-frame <c>c38</c> is not written by that wrapper; kid stays
    /// bind-pose.
    /// </summary>
    public const int PaletteSkinStartRegister = 38;
    public const int PaletteSkinRegisterCount = 58;

    /// <summary>
    /// First-seen PALSKIN case <c>00BD3C36</c> binds family slot 0 then
    /// <c>jmp 00BD3E17</c> / <c>00BD4DA6</c>. That path does not call
    /// <c>00989A60(0)</c>. The c38 writer is only the
    /// <c>[0x1436E84]+148</c> case at <c>00BD42CD</c>. Slot 33 has no
    /// New Game registrar (lighting writes 15/16, static 36, PALSKIN
    /// 37/38; zero <c>mov [rm], 33</c> in the render window). Do not
    /// invent the four floats.
    /// </summary>
    public const bool FirstSeenUploadsPaletteC38 = false;
    public const int PaletteC38SlotIndex = 33;

    /// <summary>
    /// Offsets pushed to <c>009896D0</c>. Offset 0 (c38) is absent.
    /// </summary>
    public static bool PaletteSkinOffsetIsUploaded(int offset) =>
        offset is 1 or 4 or 5 or 8 or 16;

    /// <summary>
    /// Light <c>i</c> is <c>c[19+2i]</c>. Slot 1 (first point) is <c>c21</c>/<c>c22</c>.
    /// <c>VSHADER_*_2POINTLIGHTS_*</c> read those; first-seen 1-light VS do not.
    /// </summary>
    public const int PointLightStartRegister = 21;

    /// <summary>
    /// Lighting ctor <c>[esi+18068]=1</c>. Setter <c>00B23C00</c> writes 1
    /// (arg 0) or 2 (arg ≠ 0). Zero <c>E8</c> callers. Not the VS slot index.
    /// </summary>
    public const int LightingModeDefault = 1;

    /// <summary>
    /// Static ctor <c>00BB5040</c> / landscape <c>00B69000</c> resize the
    /// family vector to 6 via <c>00B6CBD0</c>. Slot 0 = 1-light, 1–2 =
    /// 2 lights, 3–4 = 4 lights, 5 = 5 lights. Draw <c>00BA2677</c> caps
    /// packed count at 5 then <c>remap[count]</c> at family+32.
    /// MainScene <c>00B34619</c> allocs 0xF0 and calls the ctor; +32..+52
    /// stay the ctor zeros. Init bind <c>00B8B660</c> is slot 0.
    /// </summary>
    public const int ShaderFamilySlotCount = 6;
    public const int PackedLightCountCap = 5;

    /// <summary>
    /// Lighting ctor writes packed count <c>[+160]=0</c>. Add-light is
    /// message 16 on vtbl[7] <c>00B481E0</c> → <c>00B480E0</c>.
    /// MARKER_LIGHT apply does not call that path. First-seen count is 0.
    /// </summary>
    public const int FirstSeenPackedLightCount = 0;

    /// <summary>
    /// <c>00B480E0</c> rejects when <c>[arg+112]</c> or <c>[arg+120]</c>
    /// compares below 0.1 (<c>0x12A20D0</c>), or a colour channel at
    /// +96/+100/+104 compares below 1/255 (<c>0x1231724</c>).
    /// </summary>
    public const float AddLightMin = 0.1f;
    public const float AddLightChannelMin = 1f / 255f;

    public static readonly string[] StaticFamilySlotShaders =
    [
        "VSHADER_STATIC_DIRLIGHT_FOG",
        "VSHADER_STATIC_DIRLIGHT_2POINTLIGHTS_FOG",
        "VSHADER_STATIC_DIRLIGHT_2POINTLIGHTS_FOG",
        "VSHADER_STATIC_DIRLIGHT_4POINTLIGHTS_FOG",
        "VSHADER_STATIC_DIRLIGHT_4POINTLIGHTS_FOG",
        "VSHADER_STATIC_DIRLIGHT_5POINTLIGHTS_FOG",
    ];

    public static readonly string[] LandscapeFamilySlotShaders =
    [
        "VSHADER_LANDSCAPE_FOREGROUND",
        "VSHADER_LANDSCAPE_FOREGROUND_2LIGHTS",
        "VSHADER_LANDSCAPE_FOREGROUND_2LIGHTS",
        "VSHADER_LANDSCAPE_FOREGROUND_4LIGHTS",
        "VSHADER_LANDSCAPE_FOREGROUND_4LIGHTS",
        "VSHADER_LANDSCAPE_FOREGROUND_5LIGHTS",
    ];

    /// <summary>
    /// PALSKIN family ctor <c>00BD01B8</c> stores the same 6-slot names.
    /// Draw <c>00BD3C36</c> uses family+32 remap then +56. Shader-manager
    /// <c>00B3CDD4</c> attaches LayoutLights as wrapper layout index 2
    /// so <c>00989A60(0)</c> at <c>00BD4591</c> is <c>c38</c>. First-seen
    /// does not take that case (<see cref="FirstSeenUploadsPaletteC38"/>).
    /// </summary>
    public const int PaletteSkinLayoutIndex = 2;

    public static readonly string[] PalskinFamilySlotShaders =
    [
        "VSHADER_PALSKIN_DIRLIGHT_FOG",
        "VSHADER_PALSKIN_DIRLIGHT_2POINTLIGHTS_FOG",
        "VSHADER_PALSKIN_DIRLIGHT_2POINTLIGHTS_FOG",
        "VSHADER_PALSKIN_DIRLIGHT_4POINTLIGHTS_FOG",
        "VSHADER_PALSKIN_DIRLIGHT_4POINTLIGHTS_FOG",
        "VSHADER_PALSKIN_DIRLIGHT_5POINTLIGHTS_FOG",
    ];

    /// <summary>
    /// Draw <c>00BA2606</c> does <c>min(count, 5)</c>. Remap at +32 is
    /// ctor-zero on the family object, so the index is 0. First-seen
    /// packed count is 0.
    /// </summary>
    public static int CapPackedLightCount(int packedCount)
    {
        if (packedCount < 0)
            return 0;
        return packedCount > PackedLightCountCap ? PackedLightCountCap : packedCount;
    }

    /// <summary>
    /// Family+32 remap dwords are the ctor zeros (MainScene does not
    /// refill them). Slot is 0. Do not invent remap[i]=i.
    /// </summary>
    public static int SelectFamilySlot(int packedCount)
    {
        _ = CapPackedLightCount(packedCount);
        return 0;
    }

    public static string StaticFamilyShader(int packedCount) =>
        StaticFamilySlotShaders[SelectFamilySlot(packedCount)];

    public static string LandscapeFamilyShader(int packedCount) =>
        LandscapeFamilySlotShaders[SelectFamilySlot(packedCount)];

    public static string PalskinFamilyShader(int packedCount) =>
        PalskinFamilySlotShaders[SelectFamilySlot(packedCount)];

    /// <summary>
    /// Same compares as <c>00B480E0</c>. Colour channels are 0..1.
    /// </summary>
    public static bool QualifiesAsAddableLight(
        float red, float green, float blue, float field112, float field120)
    {
        if (field112 < AddLightMin || field120 < AddLightMin)
            return false;
        return red >= AddLightChannelMin
            && green >= AddLightChannelMin
            && blue >= AddLightChannelMin;
    }

    /// <summary>
    /// LayoutLights <c>[+108]=31</c> count 4. Flush <c>0098A6F6</c> uploads
    /// <c>[wrapper+416]</c> to <c>c31</c> only when packed light count &gt; 1.
    /// </summary>
    public const int PointAttenRegister = 31;

    /// <summary>Ctor <c>[esi+48]</c> = (0, 1, 0); apply writes w=0.</summary>
    public static readonly Vector4 DirLightDirection = new(0f, 1f, 0f, 0f);

    /// <summary>Record 0 <c>+0</c> = 0x3E800000 × 3, w=1.</summary>
    public static readonly Vector4 DirLightColor = new(0.25f, 0.25f, 0.25f, 1f);

    /// <summary>
    /// Record 0 <c>+32</c> copied to <c>[esi+104]</c> then
    /// <c>0098B2C0</c> index 0. Flush <c>0098A760</c> uploads that
    /// float4 to <c>c35</c>.
    /// </summary>
    public static readonly Vector4 LitColor = new(0f, 0f, 0f, 1f);

    public static readonly Vector3 FogColor = new(0.52f, 0.58f, 0.68f);
    public const float FogStart = 0f;
    public const float FogEnd = 7000f;
}
