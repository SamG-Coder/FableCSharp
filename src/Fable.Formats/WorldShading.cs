using System.Numerics;
using Fable.Formats.Meshes;

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
    /// hierarchy world × 64-byte inverse-bind. First-seen (no
    /// <c>00DBDE40</c> play-anim) is that product, ≈ identity.
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
    /// VS <c>m4x3</c> / <c>00BCFB00</c> 3-row upload: <c>dot(pos.xyzw, row)</c>
    /// with <c>w=1</c>. Weights are UBYTE/255. Missing bones leave the
    /// position unchanged.
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
    /// Record 0 <c>+32</c> copied to <c>[esi+104]</c> then
    /// <c>0098B2C0</c> index 0. Flush <c>0098A760</c> uploads that
    /// float4 to <c>c35</c>.
    /// </summary>
    public static readonly Vector4 LitColor = new(0f, 0f, 0f, 1f);

    public static readonly Vector3 FogColor = new(0.52f, 0.58f, 0.68f);
    public const float FogStart = 0f;
    public const float FogEnd = 7000f;
}
