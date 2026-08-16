using System.Numerics;
using Fable.Formats.Meshes;

namespace Fable.Formats;

/// <summary>
/// First-seen dirlight from lighting-manager ctor <c>00B482A0</c> +
/// apply <c>00F39D40</c> + LayoutLights <c>00BDB400</c>.
/// <c>[+84]=19</c>, <c>[+76]=2</c> so light 0 is <c>c19</c>/<c>c20</c>.
/// <c>[+96]=35</c>, <c>[+100]=1</c>. TOD bytes at ctor are 0 so record 0
/// is copied. <c>c35</c> is the VS MAD addend, not a LIT source.
/// Fog: first-seen VS is <c>mad oFog, min(dp4(pos,c2),c0.y), -c18.w, c0.y</c>.
/// First-seen <c>c2</c> is the <c>00B47630</c> linear view-Z plane
/// (start 1000 / end 2000), not inverse row 0. <c>c18</c> is record
/// <c>(0,0,0,1)</c>. LayoutBasic <c>c0=(0,1,2,0.5)</c> and
/// <c>c1=(256)×4</c> after dirty-2 flush. PALSKIN turns
/// D3DCOLOR indices with <c>v1.zyxw*c1</c> then
/// <c>a0</c>-relative <c>c38</c>. Far 7000 is SKY_DEF, not the
/// fog record. First-seen <c>FOGENABLE=1</c>. D3D <c>oFog</c>
/// saturates to <c>[0,1]</c>.
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
    /// <c>00989A60(0)</c> at <c>00BD4591</c> is only jump-table case 16
    /// (pass <see cref="PalskinJumpTablePass"/>, <c>[arg0+28]==16</c>).
    /// Slot 33 has no New Game registrar. First-seen pass is not 2, so
    /// that path does not write <c>c38</c>. Bone upload below does.
    /// </summary>
    public const bool FirstSeenUploadsPaletteC38 = false;
    public const int PaletteC38SlotIndex = 33;

    /// <summary>
    /// <c>00BD3070</c> <c>cmp [ebp+124], 2</c> then
    /// <c>jmp [0xBD5C40+([arg0+28]-1)*4]</c>. Any other pass
    /// (1 / 8 / 9 / 10 / 12 / 13 / default) skips that table.
    /// <c>00B32E90</c> (only <c>E8</c> from <c>00BD6B2A</c> in
    /// PALSKIN <c>00BD6810</c>) pushes pass <c>4</c>.
    /// </summary>
    public const int PalskinJumpTablePass = 2;
    public const int PalskinHelperPass = 4;

    /// <summary>
    /// Jump table at <c>0x00BD5C40</c>, index <c>[arg0+28]-1</c>.
    /// Case 16 is <c>00BD42CD</c> (slot-33 <c>c38</c>). Cases 5/14/15
    /// are 0. First-seen pass 4 never indexes this.
    /// </summary>
    public static readonly uint[] PalskinJumpTable =
    [
        0x00BD429A, 0x00BD3B28, 0x00BD3B8F, 0x00BD3C04,
        0,          0x00BD3CBB, 0x00BD3D44, 0x00BD3C60,
        0x00BD3E51, 0x00BD3F00, 0x00BD4011, 0x00BD40DA,
        0x00BD417D, 0,          0,          0x00BD42CD,
        0x00BD46A5, 0x00BD4A55,
    ];

    public static bool PalskinPassUsesJumpTable(int pass) =>
        pass == PalskinJumpTablePass;

    public static uint PalskinJumpTarget(int field28)
    {
        var i = field28 - 1;
        if ((uint)i >= (uint)PalskinJumpTable.Length)
            return 0;
        return PalskinJumpTable[i];
    }

    /// <summary>
    /// Default path <c>00BD549D</c> (pass 4) calls bone pack
    /// <c>00BD2D90</c> then <c>00BCFB00</c>. Packer stores 64-byte
    /// records (<c>shl eax, 6</c>). Draw copies 12 dwords (3 float4s)
    /// per influence and <c>0098B930</c> = <c>SetVSConstantF</c>.
    /// Start register is shader-manager <c>+11860</c> =
    /// derived-layout <c>+216</c> = <c>0x26</c> (38). Count field
    /// <c>+220</c> = <c>0x36</c> (54). Matrix values are the packed
    /// records — do not invent identity.
    /// </summary>
    public const bool FirstSeenBoneUploadWritesC38 = true;
    public const int BoneRecordBytes = 64;
    public const int BoneFloat4sPerInfluence = 3;
    public const int DerivedPaletteStartRegister = 38;
    public const int DerivedPaletteRegisterCount = 54;
    /// <summary>
    /// <c>00BD2D90</c> at <c>00BD2E7D</c> reads
    /// <c>[0x13D2880]</c>. Detector <c>00A5B850</c> is
    /// <c>mov eax,1; CPUID</c> then
    /// <c>mov [0x13D2880], 1</c> when SSE is present.
    /// First-seen TLC hosts have SSE, so the palette loop
    /// takes the SSE <c>mulps</c> path. <c>00BD2F91</c> is
    /// the x87 fallback of the same
    /// <c>dest = S * C3D</c> product.
    /// </summary>
    public const uint SseDetect = 0x00A5B850;
    public const uint SseMatrixFlag = 0x013D2880;
    public const uint BoneDestX87 = 0x00BD2F91;
    public const uint BoneHierarchyBuild = 0x00AA0090;
    public const byte FirstSeenSseMatrixFlag = 1;
    public const bool FirstSeenBoneDestUsesSsePath = true;

    public static int BoneConstantCount(int influenceCount)
    {
        if (influenceCount < 0)
            return 0;
        return influenceCount * BoneFloat4sPerInfluence;
    }

    /// <summary>
    /// Local 48-byte TRS in the C3D/exe layout: rotation/scale in the
    /// 3×3, translation in <c>M14/M24/M34</c> (not Numerics
    /// <c>M41</c>). <c>00A9E1E0</c> walks parent from the 60-byte
    /// block and writes this layout into the 64-byte dest.
    /// </summary>
    public static Matrix4x4 ComposeLocalBone(Matrix4x4 rotation, Vector3 scale, Vector3 translation) =>
        new(
            rotation.M11 * scale.X, rotation.M12 * scale.Y, rotation.M13 * scale.Z, translation.X,
            rotation.M21 * scale.X, rotation.M22 * scale.Y, rotation.M23 * scale.Z, translation.Y,
            rotation.M31 * scale.X, rotation.M32 * scale.Y, rotation.M33 * scale.Z, translation.Z,
            0f, 0f, 0f, 1f);

    /// <summary>
    /// Exe <c>00BD2F91</c>: <c>dest.M11 = S.row0 · C3D.col0</c> =
    /// hierarchy world × 64-byte inverse-bind. First-seen
    /// (<see cref="FirstSeenPlaysAnim"/> is false) is that product,
    /// ≈ identity.
    /// </summary>
    public static Matrix4x4 MultiplyWorldByInverseBind(Matrix4x4 world, Matrix4x4 inverseBind)
    {
        static float DotCol(float a1, float a2, float a3, float a4,
            float b1, float b2, float b3, float b4) =>
            a1 * b1 + a2 * b2 + a3 * b3 + a4 * b4;

        return new Matrix4x4(
            DotCol(world.M11, world.M12, world.M13, world.M14, inverseBind.M11, inverseBind.M21, inverseBind.M31, inverseBind.M41),
            DotCol(world.M11, world.M12, world.M13, world.M14, inverseBind.M12, inverseBind.M22, inverseBind.M32, inverseBind.M42),
            DotCol(world.M11, world.M12, world.M13, world.M14, inverseBind.M13, inverseBind.M23, inverseBind.M33, inverseBind.M43),
            DotCol(world.M11, world.M12, world.M13, world.M14, inverseBind.M14, inverseBind.M24, inverseBind.M34, inverseBind.M44),
            DotCol(world.M21, world.M22, world.M23, world.M24, inverseBind.M11, inverseBind.M21, inverseBind.M31, inverseBind.M41),
            DotCol(world.M21, world.M22, world.M23, world.M24, inverseBind.M12, inverseBind.M22, inverseBind.M32, inverseBind.M42),
            DotCol(world.M21, world.M22, world.M23, world.M24, inverseBind.M13, inverseBind.M23, inverseBind.M33, inverseBind.M43),
            DotCol(world.M21, world.M22, world.M23, world.M24, inverseBind.M14, inverseBind.M24, inverseBind.M34, inverseBind.M44),
            DotCol(world.M31, world.M32, world.M33, world.M34, inverseBind.M11, inverseBind.M21, inverseBind.M31, inverseBind.M41),
            DotCol(world.M31, world.M32, world.M33, world.M34, inverseBind.M12, inverseBind.M22, inverseBind.M32, inverseBind.M42),
            DotCol(world.M31, world.M32, world.M33, world.M34, inverseBind.M13, inverseBind.M23, inverseBind.M33, inverseBind.M43),
            DotCol(world.M31, world.M32, world.M33, world.M34, inverseBind.M14, inverseBind.M24, inverseBind.M34, inverseBind.M44),
            DotCol(world.M41, world.M42, world.M43, world.M44, inverseBind.M11, inverseBind.M21, inverseBind.M31, inverseBind.M41),
            DotCol(world.M41, world.M42, world.M43, world.M44, inverseBind.M12, inverseBind.M22, inverseBind.M32, inverseBind.M42),
            DotCol(world.M41, world.M42, world.M43, world.M44, inverseBind.M13, inverseBind.M23, inverseBind.M33, inverseBind.M43),
            DotCol(world.M41, world.M42, world.M43, world.M44, inverseBind.M14, inverseBind.M24, inverseBind.M34, inverseBind.M44));
    }

    public static Matrix4x4[] FirstSeenPalettes(IReadOnlyList<MeshBone> bones)
    {
        var palettes = new Matrix4x4[bones.Count];
        var world = new Matrix4x4[bones.Count];
        for (var i = 0; i < bones.Count; i++)
        {
            var bone = bones[i];
            var local = ComposeLocalBone(
                Matrix4x4.CreateFromQuaternion(bone.LocalRotation),
                bone.LocalScale,
                bone.LocalTranslation);
            world[i] = bone.Parent < 0 || bone.Parent >= i
                ? local
                : MultiplyWorldByInverseBind(world[bone.Parent], local);
            palettes[i] = MultiplyWorldByInverseBind(world[i], bone.Matrix);
        }

        return palettes;
    }

    /// <summary>
    /// VS: <c>mul r2, v1.zyxw, c1</c> (indices),
    /// <c>mov r3, v2.zyxw</c> (weights 0–1),
    /// <c>mov a0.x, r2.x</c>, then
    /// <c>mul/mad r, r3, c[38+a0]</c> (relative).
    /// File bytes are the integer bone ids / UBYTE weights.
    /// D3DCOLOR×<see cref="FirstSeenC1"/> recovers those ids.
    /// Both streams use the same <c>.zyxw</c> so pairs stay matched.
    /// <c>00BCFB00</c> uploads 3 float4s per influence.
    /// Missing bones leave the position unchanged.
    /// </summary>
    public static Vector3 SkinPosition(
        Vector3 position, ReadOnlySpan<byte> indices, ReadOnlySpan<byte> weights, Matrix4x4[] palettes)
    {
        var n = Math.Min(indices.Length, weights.Length);
        var sum = 0;
        for (var i = 0; i < n; i++)
            sum += weights[i];
        if (sum == 0 || palettes.Length == 0)
            return position;

        var acc = Vector3.Zero;
        var p = new Vector4(position, 1f);
        for (var i = 0; i < n; i++)
        {
            if (weights[i] == 0)
                continue;
            var bone = indices[i];
            if (bone >= palettes.Length)
                continue;
            var m = palettes[bone];
            var w = weights[i] / 255f;
            acc.X += w * (p.X * m.M11 + p.Y * m.M12 + p.Z * m.M13 + p.W * m.M14);
            acc.Y += w * (p.X * m.M21 + p.Y * m.M22 + p.Z * m.M23 + p.W * m.M24);
            acc.Z += w * (p.X * m.M31 + p.Y * m.M32 + p.Z * m.M33 + p.W * m.M34);
        }

        return acc;
    }

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
    /// Pass 2 jump-table case 4 binds family+32 remap then +56. First-seen
    /// pass 4 skips that table. Shader-manager <c>00B3CDD4</c> attaches
    /// LayoutLights as wrapper layout index 2 so <c>00989A60(0)</c> would
    /// be <c>c38</c>; first-seen writes <c>c38</c> via <c>0098B930</c>
    /// instead (<see cref="FirstSeenBoneUploadWritesC38"/>).
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
    /// Setter <c>0098B2C0</c> inits the stack to (0,0,0,1) then copies
    /// record 0 <c>+32</c> (same values at TOD 0). Flush <c>0098A760</c>
    /// is <c>SetVSConstantF</c> of LayoutLights <c>[+96]=35</c> count
    /// <c>[+100]=1</c>. First-seen VS do <c>MAD c35</c>, not <c>LIT</c>.
    /// </summary>
    public static readonly Vector4 LitColor = new(0f, 0f, 0f, 1f);

    /// <summary>
    /// LayoutBasic <c>00BDBB70</c> <c>[esi+56]=18</c> count
    /// <c>[esi+60]=1</c>. Flush <c>009897C0</c> is
    /// <c>SetVSConstantF(c18, wrapper+444, 1)</c>.
    /// </summary>
    public const int FogColorRegister = 18;

    /// <summary>
    /// <c>00B54310</c> <c>00989B00(2, cam+228/+240/+252/+264)</c>.
    /// </summary>
    public const int FogPlaneRegister = 2;

    /// <summary>
    /// Lighting ctor <c>00B482A0</c> record 0 <c>+64..+76</c> =
    /// (0,0,0,1). <c>00B47630</c> copies those four floats through
    /// <c>009886C0</c>. Invented RGB (0.52, 0.58, 0.68) is not this.
    /// </summary>
    public static readonly Vector4 FogRecordColor = new(0f, 0f, 0f, 1f);

    public static readonly Vector3 FogColor = new(0f, 0f, 0f);
    public const float FogStart = 1000f;
    public const float FogRecordEnd = 2000f;
    /// <summary>SKY_DEF max flare radius 6000 plus slack; not fog end.</summary>
    public const float FogEnd = 7000f;

    /// <summary>
    /// LayoutBasic <c>00BDBB70</c> first float4 is (0, 1, 2, 0.5).
    /// Dirty-bit 2 flush <c>00989BF0</c> uploads count 2 from
    /// register 0, so first-seen landscape <c>c0.y=1</c> after
    /// <c>00B47630</c> ORs bit 2. D3DRS_FOGENABLE is 1 on first-seen
    /// landscape bits 4 / 0x40 (<c>00B46890</c>). VS writes oFog so
    /// FOGTABLEMODE/FOGVERTEXMODE NONE still blend toward FOGCOLOR.
    /// </summary>
    public static readonly Vector4 FirstSeenC0 = new(0f, 1f, 2f, 0.5f);
    /// <summary>
    /// LayoutBasic <c>00BDBB70</c> second float4 is
    /// <c>0x43800000</c> × 4. Dirty-2 flush <c>00989BF0</c>
    /// uploads count 2 from register 0, so first-seen
    /// <c>c1=(256,256,256,256)</c>. PALSKIN
    /// <c>mul r2, v1.zyxw, c1</c> turns D3DCOLOR indices
    /// into 0–255 for <c>a0.x</c>.
    /// </summary>
    public static readonly Vector4 FirstSeenC1 = new(256f, 256f, 256f, 256f);
    public const uint LayoutBasicFlush = 0x00989BF0;
    public const int LayoutBasicFloat4Count = 2;
    public const uint FirstSeenPalskinDefaultDraw = 0x00BD549D;
    public const bool FirstSeenPalskinUsesA0RelativeC38 = true;
    /// <summary>
    /// First-seen static / PALSKIN / landscape BG:
    /// <c>mov oD0.w, c0.y</c>. FG is the c42 fade instead.
    /// </summary>
    public const bool FirstSeenStaticOd0WIsC0Y = true;
    public const bool FirstSeenPalskinOd0WIsC0Y = true;
    public const bool FirstSeenBackgroundOd0WIsC0Y = true;
    /// <summary>
    /// Static-lit <c>00BB2540</c> <c>CreateVertexBuffer</c> is
    /// FVF <c>0x112</c> (<c>XYZ|NORMAL|TEX1</c>) stride <c>32</c>.
    /// VS is <c>mov oT0, v2</c>. PALSKIN first-seen is
    /// <c>mov oT0, v4</c>. Object PS is 1-tex <c>mul t0, v0</c>.
    /// </summary>
    public const uint FirstSeenStaticFvf = 0x112;
    public const int FirstSeenStaticStrideBytes = 32;
    public const int FirstSeenStaticOt0Input = 2;
    public const int FirstSeenPalskinOt0Input = 4;
    public const bool FirstSeenStaticOt0IsV2 = true;
    public const bool FirstSeenPalskinOt0IsV4 = true;
    public const uint FirstSeenStaticLitDraw = 0x00BB2540;
    public const bool FirstSeenAppliesVertexFogBlend = true;

    /// <summary>
    /// First-seen VS: <c>mad oFog, min(dp4(pos,c2), c0.y), -c18.w, c0.y</c>.
    /// Not <c>min(dot,1)*c18.w+1</c>.
    /// </summary>
    public static float EvaluateVertexFog(float posDotC2, float c0y, float c18w) =>
        MathF.Min(posDotC2, c0y) * (-c18w) + c0y;

    /// <summary>
    /// D3D9 vertex-fog interpolator clamps <c>oFog</c> to
    /// <c>[0,1]</c>. The VS <c>mad</c> itself has no <c>_sat</c>.
    /// Live mesh VS does the same clamp.
    /// </summary>
    public const bool FirstSeenFogSaturates = true;

    /// <summary>
    /// First-seen fog <c>c2</c> is <see cref="LinearFogPlane"/> from
    /// <c>00B47630</c>. <c>00B54310</c> inverse row 0 is mesh-path
    /// <c>00B555A0</c> only — not the New Game first frame.
    /// </summary>
    public const bool FirstSeenFogC2IsLinearViewZ = true;

    public static float SaturateFog(float oFog) =>
        Math.Clamp(oFog, 0f, 1f);

    /// <summary>
    /// <c>00B47630</c> builds the fog plane from record
    /// start/end (<c>+80/+84</c> = 1000/2000) and camera
    /// <c>+276</c> (view 3x4 copy).
    /// <c>dp4(world, plane) = (viewZ - start) / (end - start)</c>
    /// with view +Z = look.
    /// Inverse row 0 is not this plane: at SHOT2 it dots to ~35
    /// and would force <c>oFog=0</c> on the whole house.
    /// </summary>
    public const int FogComputeCameraMatrixOffset = 276;

    public static Vector4 LinearFogPlane(Vector3 cameraPos, Vector3 look)
    {
        var forward = Vector3.Normalize(look);
        var denom = FogRecordEnd - FogStart;
        var scale = 1f / denom;
        return new Vector4(
            forward.X * scale,
            forward.Y * scale,
            forward.Z * scale,
            -(Vector3.Dot(forward, cameraPos) + FogStart) * scale);
    }

    public static float WorldDotFogPlane(Vector3 world, Vector4 plane) =>
        plane.X * world.X + plane.Y * world.Y + plane.Z * world.Z + plane.W;

    /// <summary>
    /// First-seen live path: saturate
    /// <c>EvaluateVertexFog(world · LinearFogPlane, c0.y, c18.w)</c>.
    /// </summary>
    public static float EvaluateWorldFog(Vector3 world, Vector3 cameraPos, Vector3 look) =>
        SaturateFog(EvaluateVertexFog(
            WorldDotFogPlane(world, LinearFogPlane(cameraPos, look)),
            FirstSeenC0.Y,
            FogRecordColor.W));

    /// <summary>
    /// Per-cell <c>00989A60(3)</c> writes table <c>0x0139C614</c>
    /// to <c>c3</c>: <c>(0, 0.125, 0, 0)</c>. Fog flush restores
    /// <c>c2</c> only. First-seen FG / static / PALSKIN all
    /// <c>add oD0.xyz, lit, c3</c>. Draw order is landscape
    /// <c>0x40</c> then primitives, so house and kid keep this
    /// leftover. Without it unlit faces are black.
    /// </summary>
    public static readonly Vector4 FirstSeenC3 = new(0f, 0.125f, 0f, 0f);
    public const uint C3LightingTable = 0x0139C614;
    public const bool FirstSeenDirLightAddsC3 = true;

    /// <summary>
    /// First-seen FG/static/PALSKIN: <c>dp3 r, n, -c19</c>;
    /// <c>max r.x, r, c0.x</c>; <c>min r.y, r, c0.x</c>;
    /// <c>mul r.x, r.x, r.x</c>; <c>mul r, r.x, c20</c>;
    /// <c>mad r, -r.y, c35, r</c>; <c>add …, c3</c>.
    /// <c>c35.rgb=0</c> so RGB is <c>max(n·-c19, 0)² * c20 + c3</c>.
    /// </summary>
    public static float DirLightNdotL(Vector3 normal)
    {
        var n = normal.LengthSquared() < 1e-8f ? Vector3.UnitZ : Vector3.Normalize(normal);
        var ndl = Vector3.Dot(n, new Vector3(
            -DirLightDirection.X, -DirLightDirection.Y, -DirLightDirection.Z));
        return MathF.Max(ndl, FirstSeenC0.X);
    }

    public static Vector3 EvaluateDirLightRgb(Vector3 normal)
    {
        var t = DirLightNdotL(normal);
        t *= t;
        return new Vector3(DirLightColor.X, DirLightColor.Y, DirLightColor.Z) * t
               + new Vector3(LitColor.X, LitColor.Y, LitColor.Z)
               + new Vector3(FirstSeenC3.X, FirstSeenC3.Y, FirstSeenC3.Z);
    }

    /// <summary>
    /// D3D vertex fog: <c>oFog * rgb + (1-oFog) * FogColor</c>.
    /// First-seen FogColor is black so this is <c>rgb * oFog</c>.
    /// </summary>
    public static Vector3 BlendVertexFog(Vector3 rgb, float oFog) =>
        rgb * SaturateFog(oFog) + FogColor * (1f - SaturateFog(oFog));

    /// <summary>
    /// <c>00DBDE40</c>, Create <c>006AC910</c>, ConstructFromParams
    /// <c>006A9DD0</c>, parent <c>00662880</c> / <c>008388D0</c> /
    /// <c>006A5950</c>, and activate <c>004C9CA0</c> have no
    /// PlayAnimation / STAND / CTCIdle call. <c>STAND</c> has zero
    /// code xrefs. <c>.PlayAnimation</c> lives in script dispatcher
    /// <c>00CBFACA</c>, not on the first-seen create path.
    /// </summary>
    public const bool FirstSeenPlaysAnim = false;

    /// <summary>
    /// First-seen landscape <c>00B24850</c> and static-lit
    /// <c>00BB2540</c> apply <c>0x01396FB0</c> CCW with no Flag1
    /// test. <c>0x01396FB8</c> NONE is applied unconditionally at
    /// the start of other primitive passes (<c>00B89C30</c> /
    /// <c>00BBE090</c> / <c>00BC3F30</c>) and restored to CCW
    /// after the draw. Flag1 is not that selector.
    /// </summary>
    public const bool FirstSeenAppliesCullNoneFromFlag1 = false;

    /// <summary>
    /// PALSKIN bind <c>00BD3070</c> / draw <c>00BD71B0</c> /
    /// default <c>00BD549D</c> never copy <c>0x01396FB0</c> /
    /// <c>0x01396FB8</c> onto CULLMODE <c>+10384</c>. They
    /// inherit landscape/static-lit CCW. MainScene
    /// <c>00B33010</c> first-seen layers <c>0x80</c>/<c>0x100</c>
    /// drain PALSKIN slots and do not call NONE-pass
    /// <c>00B89C30</c>.
    /// </summary>
    public const bool FirstSeenPalskinWritesCullMode = false;
    public const bool FirstSeenPalskinInheritsCullCcw = true;

    /// <summary>
    /// Static draw <c>00BA2350</c> tests Flag1 at <c>00BA3637</c>
    /// only after <c>cmp [esp+196], 2</c>. First-seen static-lit
    /// <c>00BB2540</c> and landscape <c>00B24850</c> apply CCW
    /// with no Flag1 test. Pass 2 is not the first-seen path.
    /// </summary>
    public const bool FirstSeenStaticPass2ReadsFlag1 = false;

    /// <summary>
    /// Layer type 20 is NONE-draw <c>00BBE090</c>, only called from
    /// <c>00BBC210</c> (jump-table case of <c>00BBC130</c>).
    /// First-seen PALSKIN <c>00BD71B0</c> tests <c>[this+8]</c> as an
    /// enable byte and never calls <c>00BBC130</c>. First-seen
    /// static-lit <c>00BB2540</c> (house 3180) also never calls it.
    /// Flag1 on hair 793 / house 3180 does not write type 20.
    /// </summary>
    public const bool FirstSeenFlag1WritesLayerType20 = false;

    /// <summary>
    /// First-seen PALSKIN bind writes SRCALPHA/INVSRCALPHA
    /// (<see cref="Fable.Formats.Scene.D3dDeviceState.FirstSeenPalskinSrcBlend"/>)
    /// with no Flag1 test. Kid hair Flag1 does not select that blend.
    /// </summary>
    public const bool FirstSeenPalskinSrcAlphaBlend = true;
    public const bool FirstSeenFlag1SelectsAlphaBlend = false;

    /// <summary>
    /// C3D material serialize <c>00ABF6B0</c> (only <c>E8</c> from
    /// mesh serialize <c>00A89450</c> at <c>00A8958B</c>, stride
    /// 48): Flag0 at +40, Flag1 at +41, Flag2 at +42, Flag3 at
    /// +43. First-seen PALSKIN draw <c>00BD71B0</c> at
    /// <c>00BD76D2</c>/<c>00BD7705</c>: <c>xor ebx,ebx</c>, then
    /// if opacity &lt; <c>0xFF</c> fill from <c>[inst+12]</c> bit 9
    /// (first-seen opacity is <c>0xFF</c> so that block is skipped).
    /// Flag2≠0 → <c>or ebx, 2</c>; else Flag1≠0 → <c>or ebx, 5</c>.
    /// That bitfield plus MapFlags <c>[mat+32]</c> picks the helper
    /// type index pushed to <c>00BCE740</c>. First-seen static-lit
    /// <c>00BB2540</c> has no +41 read.
    /// </summary>
    public const int MaterialFlag1Offset = 41;
    public const int MaterialFlag2Offset = 42;
    public const int MaterialStrideBytes = 48;
    public const uint MaterialSerialize = 0x00ABF6B0;
    public const int FirstSeenPalskinFlag1MaskOr = 5;
    public const int FirstSeenPalskinFlag2MaskOr = 2;
    public const bool FirstSeenPalskinReadsFlag1 = true;
    public const bool FirstSeenStaticLitReadsFlag1 = false;
    public const int InstanceOpacityOffset = 39;
    public const byte FirstSeenInstanceOpacity = 0xFF;

    /// <summary>
    /// <c>00BD76E0</c>–<c>00BD77C2</c> with first-seen opacity
    /// <c>0xFF</c> (the <c>[inst+12]</c> bit-9 fill is skipped).
    /// MapFlags bit 0 set and ebx bit 9 clear: remainder 2 or 6 →
    /// 11, remainder 4 → 7, else 4. Hair MapFlags=1 + Flag1 → 4.
    /// </summary>
    public static int PalskinTypeIndex(byte flag1, byte flag2, byte opacity, int mapFlags)
    {
        var bits = 0;
        if (flag2 != 0)
            bits |= FirstSeenPalskinFlag2MaskOr;
        else if (flag1 != 0)
            bits |= FirstSeenPalskinFlag1MaskOr;
        if (opacity < FirstSeenInstanceOpacity || (mapFlags & 1) == 0 || (bits & 0x200) != 0)
            return FirstSeenPalskinHairTypeIndex;
        var remainder = mapFlags & ~1;
        if (remainder is 2 or 6)
            return 11;
        if (remainder == 4)
            return 7;
        return 4;
    }

    /// <summary>
    /// Helper ctor <c>00BCE740</c> (vtbl <c>0x12A6C5C</c>, dtor
    /// <c>00BD7CB0</c>): <c>+12</c> = instance, <c>+16</c> = 0,
    /// <c>+24</c> = arg1 pointer (released by <c>00BCE4CB</c>),
    /// <c>+28</c> = type index (Flag1-derived <c>esi</c>, 4 for
    /// hair MapFlags=1 + mask 5), <c>+32</c> = fade byte.
    /// First-seen bind <c>00BD3070</c> pass 4 (<c>00BD549D</c>)
    /// does not read helper+28. <c>00BD3B1A</c> jump table uses
    /// bind-arg+28, not this field. Draw queues the helper via
    /// <c>00B84720</c> on <c>0x1436E74</c>: type
    /// <c>[inst+104]+8==1</c> → slots 10 then 14; type 0 → slot
    /// 8 then Flag1 adds slot 9. MainScene <c>00B33010</c> drains
    /// slot 14 at layer <c>0x80</c> and slots 8+10 at
    /// <c>0x100</c> through <c>00B849F0</c>, which calls
    /// <c>[helper+20].vtbl+20/+24</c>. Type 4 is not bound on
    /// first-seen (<see cref="FirstSeenPalskinDrainUsesType4"/>).
    /// </summary>
    public const uint PalskinHelperCtor = 0x00BCE740;
    public const uint PalskinHelperVtbl = 0x012A6C5C;
    public const uint PalskinHelperDtor = 0x00BD7CB0;
    public const int PalskinHelperArg1Offset = 24;
    public const int PalskinHelperTypeIndexOffset = 28;
    public const int FirstSeenPalskinHairTypeIndex = 4;
    public const int PalskinQueueSlotType1A = 10;
    public const int PalskinQueueSlotType1B = 14;
    public const int PalskinQueueSlotType0 = 8;
    public const int PalskinQueueSlotFlag1Extra = 9;
    public const uint PrimQueueDrain = 0x00B849F0;
    public const uint PrimQueueSubmit = 0x00B84720;
    public const bool FirstSeenPalskinBindUsesHelperTypeIndex = false;

    /// <summary>
    /// PALSKIN renderer ctor <c>00BCFF10</c> writes vtbl
    /// <c>0x012A78DC</c>: slot +20 = <c>00BD7110</c>, +24 =
    /// <c>00B91340</c>. Drain <c>00B849F0</c> with queue+20 byte 0
    /// calls +20; byte 1 calls +24. <c>00BD7110</c> with
    /// helper+32==0 (first-seen: <c>00BEBBB0</c> fails or extras
    /// empty) calls <c>00BD3070(helper, arg1)</c>. Jump table
    /// case 4 (<c>00BD3C04</c>) runs only when
    /// <c>[ebp+124]==2</c>; drain arg1 is queue+12, not 2, so
    /// first-seen takes default <c>00BD549D</c>. <c>00B91340</c>
    /// unwraps helper+12 and jumps to debug <c>00B91140</c>.
    /// </summary>
    public const uint PalskinRendererVtbl = 0x012A78DC;
    public const uint PalskinDrainVtbl20 = 0x00BD7110;
    public const uint PalskinDrainVtbl24 = 0x00B91340;
    public const uint PalskinType4JumpTarget = 0x00BD3C04;
    public const bool FirstSeenPalskinDrainUsesType4 = false;
}
