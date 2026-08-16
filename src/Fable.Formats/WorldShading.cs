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
    /// LayoutLights <c>[+20]</c>. New Game map v18 reaches 7 callers of
    /// <c>009896D0</c> in the static/palskin range. First-frame <c>c38</c>
    /// values are still UNREAD; kid stays bind-pose.
    /// </summary>
    public const int PaletteSkinStartRegister = 38;
    public const int PaletteSkinRegisterCount = 58;

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
    /// packed count at 5 then remaps. Remap dwords at family+32 UNREAD.
    /// </summary>
    public const int ShaderFamilySlotCount = 6;
    public const int PackedLightCountCap = 5;

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
