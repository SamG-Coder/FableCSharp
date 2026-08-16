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
    public const uint StarDraw = 0x00B65A20;
    public const uint StarDrawCaller = 0x00B66284;
    public const uint StarDrawCallerFn = 0x00B66190;
    public const bool FirstSeenCallsStarDraw = true;
    /// <summary>
    /// <c>00B65A20</c> hops <c>[0x1436E8C]+408</c> twice via
    /// vtbl+4, then <c>D = [C+84]</c>. <c>[D+424]</c> is a
    /// pointer to two dwords. First dword <c>== 0</c> takes
    /// <c>00B65A87 ret 4</c>. The first dword is then an index
    /// into a <c>44</c>-byte record table
    /// (<c>imul 0x2E8BA2E9</c>), not a <c>stars.dat</c> walk.
    /// Map-manager <c>+424</c> is OpenStaticMaps mode (0/1/2),
    /// a different object. Fade is
    /// <c>1.0 - *[D+436]</c> uploaded as VS colour via
    /// wrapper+80. Sky ctor writes <c>[this+396]=1</c> so
    /// <c>00B66190</c> also calls weather <c>00B64FA0</c>.
    /// </summary>
    public const uint MapManagerGlobal = 0x01436E8C;
    public const int MapManagerWorldOffset = 408;
    public const int StarObjectFromHopOffset = 84;
    public const int StarListPointerOffset = 424;
    public const int StarFadePointerOffset = 436;
    public const int MapManagerModeOffset = 424;
    public const uint StarEmptyRet = 0x00B65A87;
    public const bool StarEmptyFirstDwordSkipsDraw = true;
    public const uint StarRecordReciprocal = 0x2E8BA2E9;
    public const int StarRecordStrideBytes = 44;
    public const uint OneConst = 0x0122DED8;
    public const uint WeatherDraw = 0x00B64FA0;
    public const uint WeatherDrawCaller = 0x00B6629D;
    public const uint WeatherAllZeroRet = 0x00B659A5;
    public const int WeatherIdPointer0Offset = 472;
    public const int WeatherIdPointer1Offset = 448;
    public const int WeatherIdDwordsPerPointer = 2;
    public const int WeatherIdCount = 4;
    public const int WeatherSkipCountOffset = 16;
    public const int SkyWeatherByteOffset = 396;
    public const byte FirstSeenSkyWeatherByte = 1;
    public const bool FirstSeenCallsWeatherDraw = true;
    public const bool WeatherAllZeroIdsSkipDraw = true;
    public const bool FirstSeenWeatherDrawBuildsMesh = false;
    public const bool FirstSeenStarDrawIteratesStarsDat = false;
    public const bool FirstSeenEmitsInventedStarBillboards = false;
    /// <summary>
    /// Theme-slot ctor <c>008864A0</c> zeros <c>[this+424]</c>
    /// with the surrounding block through +420. Theme copy
    /// <c>008865C0</c> at <c>00886AD2</c> writes
    /// <c>dest+424 = [src+192]</c> and treats dest+428 as a
    /// vector. Only <c>E8</c> is the six-slot loop
    /// <c>00888499</c>. <c>D</c> is <c>[C+84]</c> after the
    /// map-manager hop, not ENVIRONMENT +424 (that slot is an
    /// inline NString whose first dword ctor
    /// <c>0099E4B0</c> already writes 0). First-seen star and
    /// weather therefore still see pointer first-dwords of 0
    /// unless a non-zero <c>src+192</c> is proven; skip gates
    /// stay live.
    /// </summary>
    public const uint ThemeSlotCtor = 0x008864A0;
    public const uint ThemeSlotCopy = 0x008865C0;
    public const uint ThemeSlotCopyStarWrite = 0x00886AD2;
    public const uint ThemeSlotCopyLoop = 0x00888499;
    public const int ThemeSlotCopySourcePointerOffset = 192;
    public const int ThemeSlotVectorOffset = 428;
    public const bool FirstSeenStarListPointerCtorZero = true;
    public const bool FirstSeenStarPointerPayloadsAreNumericIds = false;

    /// <summary>
    /// Sky ctor bind at <c>00B62BA8</c>: <c>mov eax, [ecx+96]</c>
    /// then <c>test ah, 1</c>. Set → <c>PSHADER_INNER_SKY_SIMPLE</c>
    /// at <c>00B62BB6</c> into <c>this+292</c>. Clear →
    /// <c>PSHADER_INNER_SKY</c> at <c>00B62C2D</c>. <c>ah</c> is
    /// byte <c>[ecx+97]</c>, i.e. dword bit 8. Token listings live
    /// in ExeIndex <c>out/01-sections/shader-tokens/</c>. Neither
    /// program has <c>def c0/c1/c2</c>. IDirect3DDevice9
    /// <c>SetPixelShaderConstantF</c> is vtbl slot 109
    /// (<c>[dev+436]</c> / <c>0x1B4</c>), not the earlier
    /// <c>0x1A8/0x1AC</c> miss. First-seen writer of those PS
    /// constants is still unread — do not invent <c>*c2=0</c>.
    /// Live mode 2 <c>t1*v0</c> is a stand-in.
    /// </summary>
    public const uint InnerSkyPsBind = 0x00B62BA8;
    public const uint InnerSkySimpleBind = 0x00B62BB6;
    public const uint InnerSkyFullBind = 0x00B62C2D;
    public const int InnerSkyShaderStoreOffset = 292;
    public const int QualityDwordOffset = 96;
    public const int QualitySimpleAhMask = 1;
    public const int QualitySimpleBit = 8;
    public const int SetPixelShaderConstantFSlot = 436;
    public const int SetPixelShaderConstantFVtbl = 109;
    public const int SetTextureSlot = 260;
    public const bool FirstSeenInnerSkyHasConstDef = false;
    public const bool FirstSeenSkyPsC2HasWriter = false;
    public const bool FirstSeenSkyMode2IsStandIn = true;
    public const bool FirstSeenQualityBitKnown = false;
    /// <summary>
    /// Both INNER_SKY and SIMPLE: <c>mul_sat r0, r0, v0.w</c> after
    /// <c>mul_sat r0.xyz, r0, v0</c>. <c>VSHADER_INNER_SKY</c> is
    /// <c>mov oD0, v1</c> so that alpha is the dome dest+12 byte.
    /// </summary>
    public const bool FirstSeenInnerSkyMulsVertexAlpha = true;
    public const int InnerSkyVsC92 = 92;
    public const uint PsConstantWrapper0 = 0x009888FC;
    public const uint PsConstantWrapper1 = 0x00989C98;
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
    /// CRT stub <c>01224830</c> is
    /// <c>fld qword [0x12A1140]; fcos; fmul dword [0x12A1138]=13000;
    /// fstp [0x0143782C]</c>. The qword is the last-ring elevation.
    /// Ctor <c>00B627E2</c> copies +16/+20/+12 from the current
    /// <c>ENVIRONMENT</c> object at <c>[0x1436E24]</c>
    /// (+292/+288/+296). Those slots are CString first-dwords
    /// (ctor <c>0099AED0</c> zeros; persist <c>004310A7</c>).
    /// First-seen all three are 0.
    /// </summary>
    public const uint UvDivisorGlobal = 0x0143782C;
    public const uint UvDivisorInit = 0x01224830;
    public const uint UvDivisorAngleConst = 0x012A1140;
    public const uint UvDivisorScaleConst = 0x012A1138;
    public const float UvDivisorScale = 13000f;
    public const ulong UvDivisorAngleBits = 0x3FDE28C760000000UL;
    public const uint EnvironmentGlobal = 0x01436E24;
    public const uint VideoOptionsGlobal = EnvironmentGlobal;
    public const uint EnvironmentLookup = 0x00B26828;
    public const uint VideoOptionsLookup = 0x00B2640F;
    public const uint CtorThis16Write = 0x00B627E2;
    public const uint EnvironmentStringCtor = 0x0099AED0;
    public const uint EnvironmentPersist = 0x00430900;
    public const uint EnvironmentStringPersist = 0x004310A7;
    public const int This16FromOptionsOffset = 292;
    public const int This20FromOptionsOffset = 288;
    public const int This12FromOptionsOffset = 296;
    /// <summary>
    /// +288/+292/+296 are CString first-dwords: ctor
    /// <c>0099AED0</c> writes 0, persist <c>004310A7</c> assigns
    /// the intern pointer. First-seen +296 is 0 (origin Z).
    /// <c>ENVIRONMENT</c> has one filename
    /// (<c>lightning_colours.tga</c>) which fills an earlier
    /// string slot; +288/+292 stay the ctor zeros.
    /// </summary>
    public const bool FirstSeenThis16HasNumeric = true;
    public const bool FirstSeenThis20HasNumeric = true;
    public const float FirstSeenThis16 = 0f;
    public const float FirstSeenThis20 = 0f;
    public const bool FirstSeenUvDivisorHasWriter = true;
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

    public static double UvDivisorAngle =>
        System.BitConverter.Int64BitsToDouble(unchecked((long)UvDivisorAngleBits));

    public static float FirstSeenUvDivisor =>
        (float)(UvDivisorScale * Math.Cos(UvDivisorAngle));

    public static float FirstSeenInvUvDivisor => 1f / FirstSeenUvDivisor;

    /// <summary>
    /// <c>00B64FA0</c>: each id <c>== 0</c> increments the skip
    /// count; count <c>== 4</c> takes <c>00B659A5 ret 4</c>.
    /// </summary>
    public static bool WeatherSkipDraw(ReadOnlySpan<int> ids)
    {
        if (ids.Length != WeatherIdCount)
            return false;
        var skips = 0;
        foreach (var id in ids)
        {
            if (id == 0)
                skips++;
        }

        return WeatherAllZeroIdsSkipDraw && skips == WeatherIdCount;
    }

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
    /// Dest+12 alpha as 0..1. <c>PSHADER_INNER_SKY</c> multiplies
    /// by this via <c>mov oD0, v1</c> then <c>mul_sat r0, r0, v0.w</c>.
    /// </summary>
    public static float DomeAlpha(int ring) =>
        ((uint)DomeColor(ring) >> 24) / 255f;

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
