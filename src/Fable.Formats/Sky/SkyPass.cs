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

    public static float ElevStart =>
        System.BitConverter.Int32BitsToSingle(unchecked((int)ElevStartBits));

    public static float ElevStep =>
        System.BitConverter.Int32BitsToSingle(unchecked((int)ElevStepBits));

    public static float Inv36 =>
        System.BitConverter.Int32BitsToSingle(unchecked((int)Inv36Bits));

    public static float TwoPiOver35 =>
        System.BitConverter.Int32BitsToSingle(unchecked((int)TwoPiOver35Bits));

    public static System.Numerics.Vector3 EllipsoidPoint(int ring, int seg)
    {
        var elev = ElevStart + ring * ElevStep;
        var az = seg % DomeSegments * Inv36 * 6.283185307179586f;
        var ce = MathF.Cos(elev);
        var se = MathF.Sin(elev);
        return FirstSeenOrigin + new System.Numerics.Vector3(
            HorizRadius * ce * MathF.Sin(az),
            HorizRadius * ce * MathF.Cos(az),
            VertRadius * se);
    }
}
