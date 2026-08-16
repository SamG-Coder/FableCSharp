namespace Fable.Formats.Sky;

/// <summary>
/// First-seen sky else-path <c>00B662F0</c> when layer bit is not
/// <c>0x400000</c>. <c>00B30B50</c> is called on
/// <c>[0x1436EA0]</c> with source near <c>0x42C80000=100</c>,
/// far <c>0x461C4000=10000</c>, minZ from <c>0x0139A704=0.99</c>,
/// maxZ 1. Then <c>00B66190</c> draws outer <c>00B63C00</c> and
/// inner <c>00B640E0</c>. A second <c>00B30B50</c> restores the
/// world camera. <c>VSHADER_INNER_SKY</c> is
/// <c>dp4 oPos, v0, c5–c8</c> (no <c>c4</c>).
/// </summary>
public static class SkyPass
{
    public const uint Draw = 0x00B662F0;
    public const uint DrawElse = 0x00B66416;
    public const uint MeshDraw = 0x00B66190;
    public const uint OuterDraw = 0x00B63C00;
    public const uint InnerDraw = 0x00B640E0;
    public const uint SkyCameraSetup = 0x00B30B50;
    public const uint LayerBit400000 = 0x400000;
    public const uint FirstSeenLayerBit = 0x2000;
    public const bool FirstSeenUses400000 = false;
    public const float FirstSeenNear = 100f;
    public const float FirstSeenFar = 10000f;
    public const float FirstSeenMinZ = 0.99f;
    public const float FirstSeenMaxZ = 1f;
    public const uint NearImm = 0x42C80000;
    public const uint FarImm = 0x461C4000;
    public const uint MinZConst = 0x0139A704;
    public const int WvpStartRegister = 5;
    public const int WvpCount = 4;

    /// <summary>
    /// <c>00B620A0</c> builds the VB at renderer+88 (stride 24,
    /// FVF <c>0x142</c>, count <c>0x1BB=443</c>). Fill
    /// <c>00B61DD0</c> (only <c>E8</c> from <c>00B62156</c>):
    /// 9 rings × 37 verts (36 segments + wrap), start index 73.
    /// <c>elev = ring * 0x0139A710 + π/2</c>,
    /// <c>xy = 6500 * cos(elev) * (sin az, cos az)</c>,
    /// <c>z = 3250 * sin(elev) + [this+12]</c>. First-seen
    /// <c>[this+12]=0</c>. Azimuth <c>i * (1/36)</c> then
    /// <c>fmul [0x124F2B8]</c> (2π so the step is 2π/36).
    /// Cap <c>00B61B30</c> is (0,0,7000) plus 36× cylinder
    /// pairs at z=−500 / 7000, r=6500, step <c>2π/35</c>.
    /// Skirt <c>00B61CD0</c> is (0,0,−10000) plus 36×
    /// (6500 cis, z=−500) from vert 406.
    /// </summary>
    public const uint DomeSetup = 0x00B620A0;
    public const uint DomeFill = 0x00B61DD0;
    public const uint DomeFillCaller = 0x00B62156;
    public const uint CapFill = 0x00B61B30;
    public const uint SkirtFill = 0x00B61CD0;
    public const int DomeRings = 9;
    public const int DomeSegments = 36;
    public const int DomeVertsPerRing = 37;
    public const int DomeStartVertex = 73;
    public const int VertexStrideBytes = 24;
    public const int VertexCount = 0x1BB;
    public const uint Fvf = 0x142;
    public const float HorizRadius = 6500f;
    public const float VertRadius = 3250f;
    public const uint ElevStepBits = 0xBE0CBE4C;
    public const uint ElevStartBits = 0x3FC90FDB;
    public const uint Inv36Bits = 0x3CE38E39;
    public const uint TwoPiOver35Bits = 0x3E37D3FB;
    public const float PoleZ = 7000f;
    public const float SkirtZ = -500f;
    public const float NadirZ = -10000f;
    public const int FirstSeenOriginZ = 0;
    public static readonly System.Numerics.Vector3 FirstSeenOrigin = System.Numerics.Vector3.Zero;

    /// <summary>
    /// <c>00B61E61</c> <c>fmul [0x12A2900]=1.105</c> then
    /// <c>fsubr [0x122DED8]=1</c> leaves
    /// <c>vBase = 1 - 1.105 * cos(elev)</c> on the FPU for the
    /// whole ring. Colour <c>00B61EE0</c> does <c>fld 0</c>,
    /// <c>fcomp vBase</c>, <c>test ah, 0x41</c>: if
    /// <c>vBase &gt;= 0</c> keep it, else 0. Then
    /// <c>fmul [0x1230014]=255</c> and <c>00BFEA70</c>
    /// (fistp + <c>0x7FFFFFFF</c> half-adjust). Dest+12 is
    /// <c>(al &lt;&lt; 24) | 0xFFFFFF</c>.
    /// </summary>
    public const uint FloatToInt = 0x00BFEA70;
    public const uint ColourTail = 0x00B61EE0;
    public const uint UvVBaseScaleBits = 0x3F8D70A4;
    public const uint ColourScaleBits = 0x437F0000;
    public const uint ColourScaleConst = 0x01230014;
    public const uint UvVBaseScaleConst = 0x012A2900;
    public const int ColourRgbMask = 0x00FFFFFF;
    public const int FloatToIntRoundBits = 0x7FFFFFFF;

    /// <summary>
    /// <c>00B61E4D</c> <c>fld [ecx+16]</c>, <c>fsub st, st(1)</c>
    /// (sin elev), <c>fmul [ecx+20]</c> →
    /// <c>uvScale = (this+16 - sin(elev)) * this+20</c>.
    /// <c>U = x * (1/[0x0143782C]) * uvScale</c>,
    /// <c>V = y * (1/[0x0143782C]) * uvScale</c>.
    /// Ctor <c>00B627E2</c> copies those slots from the current
    /// <c>ENGINE_VIDEO_OPTIONS_*</c> object at
    /// <c>[0x1436E24]</c>: +16 ← +292, +20 ← +288, +12 ← +296.
    /// First-seen +12 is 0 (origin Z). +16 / +20 and the
    /// <c>0x0143782C</c> divisor have no first-seen numeric
    /// writer — do not invent <c>(seg/36, ring/8)</c>.
    /// </summary>
    public const uint UvDivisorGlobal = 0x0143782C;
    public const uint VideoOptionsGlobal = 0x01436E24;
    public const uint VideoOptionsLookup = 0x00B2640F;
    public const uint CtorThis16Write = 0x00B627E2;
    public const int This16FromOptionsOffset = 292;
    public const int This20FromOptionsOffset = 288;
    public const int This12FromOptionsOffset = 296;
    public const bool FirstSeenThis16HasNumeric = false;
    public const bool FirstSeenThis20HasNumeric = false;
    public const bool FirstSeenUvDivisorHasWriter = false;
    public const uint CapPoleUvBits = 0x38D1B717;
    public const int CapPoleColor = 0;
    public const int CapCylinderColor = unchecked((int)0xFFFFFFFF);
    public const float CapCylinderV = 1f;

    public static float ElevStart =>
        System.BitConverter.Int32BitsToSingle(unchecked((int)ElevStartBits));

    public static float ElevStep =>
        System.BitConverter.Int32BitsToSingle(unchecked((int)ElevStepBits));

    public static float Inv36 =>
        System.BitConverter.Int32BitsToSingle(unchecked((int)Inv36Bits));

    public static float TwoPiOver35 =>
        System.BitConverter.Int32BitsToSingle(unchecked((int)TwoPiOver35Bits));

    public static float UvVBaseScale =>
        System.BitConverter.Int32BitsToSingle(unchecked((int)UvVBaseScaleBits));

    public static float ColourScale =>
        System.BitConverter.Int32BitsToSingle(unchecked((int)ColourScaleBits));

    public static float CapPoleUv =>
        -System.BitConverter.Int32BitsToSingle(unchecked((int)CapPoleUvBits));

    public static float Elev(int ring) => ElevStart + ring * ElevStep;

    /// <summary>
    /// Leftover after the ring setup: <c>1 - 1.105 * cos(elev)</c>.
    /// </summary>
    public static float UvVBase(int ring) => 1f - UvVBaseScale * MathF.Cos(Elev(ring));

    /// <summary>
    /// <c>00BFEA70</c>: fistp then add <c>0x7FFFFFFF</c> to the
    /// remainder bits and adc/sbb the integer. Half-adjust toward
    /// nearest; exact integers stay exact.
    /// </summary>
    public static int FloatToByte(float value)
    {
        if (value == 0f)
            return 0;
        var truncated = (int)value;
        var frac = value - truncated;
        if (value > 0f)
            return frac >= 0.5f ? truncated + 1 : truncated;
        return frac <= -0.5f ? truncated - 1 : truncated;
    }

    /// <summary>
    /// Dest+12 D3DCOLOR: RGB white, alpha = rounded
    /// <c>max(vBase, 0) * 255</c>.
    /// </summary>
    public static int DomeColor(int ring)
    {
        var vBase = UvVBase(ring);
        var alpha = vBase > 0f ? vBase : 0f;
        return (FloatToByte(alpha * ColourScale) << 24) | ColourRgbMask;
    }

    /// <summary>
    /// Dome dest+16/+20. Pass the first-seen video-options floats
    /// and <c>1/[0x0143782C]</c> when those writers are dumped.
    /// </summary>
    public static System.Numerics.Vector2 DomeUv(
        int ring, int seg, float this16, float this20, float invDivisor)
    {
        var p = EllipsoidPoint(ring, seg);
        var uvScale = (this16 - MathF.Sin(Elev(ring))) * this20;
        return new System.Numerics.Vector2(
            p.X * invDivisor * uvScale,
            p.Y * invDivisor * uvScale);
    }

    public static System.Numerics.Vector3 EllipsoidPoint(int ring, int seg)
    {
        var elev = Elev(ring);
        var az = seg % DomeSegments * Inv36 * 6.283185307179586f;
        var ce = MathF.Cos(elev);
        var se = MathF.Sin(elev);
        return FirstSeenOrigin + new System.Numerics.Vector3(
            HorizRadius * ce * MathF.Sin(az),
            HorizRadius * ce * MathF.Cos(az),
            VertRadius * se);
    }
}
