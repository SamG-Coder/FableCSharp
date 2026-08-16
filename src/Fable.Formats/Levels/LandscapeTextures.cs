using Fable.Formats.Defs;

namespace Fable.Formats.Levels;

/// <summary>
/// Maps WAD .lev GROUND_/PATH_ material names onto textures.h LANDSCAPE_* ids.
/// The u32 sitting at the end of a material slot is not a textures.big id.
/// </summary>
public static class LandscapeTextures
{
    public const int DefaultId = 414;
    public const int WaterId = 442;

    /// <summary>
    /// <c>00B6D6E0</c> compares the first bank u32 to 8 and returns 0
    /// otherwise. Type 8 copies the next two u32s to <c>[this]</c>/
    /// <c>[this+4]</c> and advances the stream by 8 — no further payload
    /// in that function. StartOakVale sea is 7363; there is no
    /// water-prefix STB bank.
    /// </summary>
    public const uint RequiredWaterBankType = 8;
    public const int WaterType8CopiedDwords = 2;

    /// <summary>
    /// StartOakVale <c>__ENGINE_SEA_*</c> first u32. Not compared
    /// anywhere in the water renderer (the only <c>0x1CC3</c> imm is
    /// in a KnotholeGlade/Arena string fn). <c>00B6D6E0</c> accepts
    /// type 8 only.
    /// </summary>
    public const uint SeaBankFirstU32 = 7363;

    /// <summary>
    /// Water ctor <c>00B73760</c> zeros <c>+508</c>..<c>+624</c>
    /// (ebx=0). Draw <c>00B783F0</c> treats begin==end on those
    /// vectors plus flags <c>+630</c>/<c>+645</c> as empty and
    /// <c>je 00B7A865</c>. <c>00B6D4D0</c> stores the sea intern at
    /// <c>+1448</c> and the name string at <c>+1452</c>. Missing
    /// water intern returns at <c>00B420E4</c>. First-seen never
    /// pushes a type-8 record, so the draw is empty.
    /// </summary>
    public const int WaterDrawVectorFirst = 508;
    public const int WaterDrawVectorLast = 624;
    public const bool FirstSeenWaterDrawIsEmpty = true;

    /// <summary>
    /// <c>00B6DAF0</c> is the only <c>E8</c> caller of
    /// <c>00B6D6E0</c>. Dest is <c>lea ecx,[esp+12]</c> — the
    /// two type-8 dwords land on a stack local and
    /// <c>add esp,8</c> discards them. <c>LoadWaterData</c>
    /// ignores <c>al</c>.
    /// </summary>
    public const bool WaterType8DwordsAreStoredOnRenderer = false;

    /// <summary>
    /// <c>00B6D4D0</c> <c>mov [ecx+1448], eax</c> then
    /// <c>add ecx, 0x5AC</c> copies the name string.
    /// </summary>
    public const int SeaInternOffset = 1448;
    public const int SeaNameStringOffset = 1452;

    /// <summary>
    /// Ctor <c>0099E4B0</c> at <c>+636</c> writes the first
    /// dword 0. Sea bind <c>00B6DC40</c> does
    /// <c>cmp [esi+636], 0</c> / <c>je</c> return 0 — it never
    /// reaches the <c>+1448</c> intern or the 7363 bank.
    /// <c>+1464</c> is also ctor-zero. <c>+630</c> is set only
    /// by <c>00B6D420</c> on a later bind when <c>+1464</c>
    /// is already set.
    /// </summary>
    public const int WaterWantedNameOffset = 636;
    public const int SeaBankObjectOffset = 1464;
    public const int WaterMeshReadyOffset = 630;
    /// <summary>
    /// Draw <c>00B783F0</c> at <c>00B784D1</c>: mesh-ready is
    /// <c>[+630] &amp;&amp; [+645]</c>. Either byte 0 keeps the
    /// flag clear. First-seen ctor leaves both 0.
    /// </summary>
    public const int WaterMeshReadySecondOffset = 645;
    public const bool FirstSeenWaterWantedNameIsZero = true;
    public const bool FirstSeenSeaBindRuns = false;

    /// <summary>
    /// When bind does run, <c>00B6DC40</c> → <c>009D6100</c>
    /// allocs 72 bytes and <c>009D5DF0</c> wraps the whole sea
    /// blob (first u32 still 7363) as a stream. It does not call
    /// <c>00B6D6E0</c> / type 8.
    /// </summary>
    public const int SeaStreamObjectBytes = 72;
    public const bool SeaBindUsesType8Check = false;

    /// <summary>
    /// StartOakVale sea bank first five u32s. Word 0 is not type 8.
    /// When bind runs, <c>00BE91E0</c> (only <c>E8</c> from
    /// <c>00B6D420</c> at <c>00B6DECD</c>) reads word 0 as vertex
    /// count, word 1 as index count, word 2 as vertex payload
    /// bytes, word 3 as index payload bytes. Word 4 is the first
    /// payload dword. First-seen never reaches that reader
    /// (<see cref="FirstSeenReadsSeaPrefixWords"/>).
    /// </summary>
    public static readonly uint[] StartOakValeSeaPrefix =
        [7363, 44259, 58022, 1135617, 4340736];

    public const uint SeaMeshCopy = 0x00B6D420;
    public const uint SeaMeshBuilder = 0x00BE91E0;
    public const uint SeaMeshCopyCallSite = 0x00B6DECD;
    public const int SeaVertexStrideBytes = 12;
    public const int SeaIndexFormat = 101;
    public const int SeaMeshPrimitiveCountOffset = 180;
    public const bool FirstSeenReadsSeaPrefixWords = false;
    public const bool FirstSeenCallsSeaMeshCopy = false;

    /// <summary>
    /// Only assigner of water <c>+636</c>: <c>00B23F00</c> does
    /// <c>ecx = [0x1436E54] + 0x27C</c> then <c>0099EFB0</c>.
    /// Sibling <c>00B23900</c> is <c>this+636</c>. Zero <c>E8</c>
    /// callers. Zero <c>call [r+0x38]</c> (vtbl slot 14) in
    /// OpenStaticMaps / landscape-water / MainScene / StartOakVale.
    /// Ctor zeros the dword; dtor <c>00B71994</c> calls
    /// <c>0099EAE0</c>. First-seen never writes a map name here.
    /// </summary>
    public const uint WaterWantedNameSetter = 0x00B23F00;
    public const uint WaterWantedNameThisSetter = 0x00B23900;
    public const int WaterWantedNameSetterVtblSlot = 14;
    public const bool FirstSeenCallsWantedNameSetter = false;

    /// <summary>
    /// <c>SetStaticMapFileForUse</c> <c>00B428E0</c> always calls
    /// <c>LoadWaterData</c> <c>00B41FA0</c> at <c>00B429CB</c>
    /// (only <c>E8</c>). Lookup intern is <c>0x1436EC8</c>. Missing
    /// intern <c>je 00B420E4</c> — a bare cleanup <c>ret</c>.
    /// StartOakVale has no water-prefix STB, so first-seen takes
    /// that miss. Draw <c>00B783F0</c> is water vtbl+16
    /// (<c>0x012A3364</c>); zero <c>E8</c> callers. Empty check
    /// <c>je 00B7A865</c> is <c>pop</c>×4 / <c>add esp,40</c> /
    /// <c>ret 4</c> — no unbind and no draw. Vtbl+8
    /// <c>00B6D500</c> is <c>ret 4</c>.
    /// </summary>
    public const uint LoadWaterData = 0x00B41FA0;
    public const uint LoadWaterDataIntern = 0x1436EC8;
    public const uint LoadWaterDataOnlyCaller = 0x00B429CB;
    public const uint LoadWaterDataMissingInternRet = 0x00B420E4;
    public const uint WaterDraw = 0x00B783F0;
    public const uint WaterDrawEmptyReturn = 0x00B7A865;
    public const uint WaterRendererVtbl = 0x012A3364;
    public const int WaterDrawVtblOffset = 16;
    public const bool FirstSeenLoadWaterDataFindsIntern = false;
    public const bool FirstSeenWaterDrawEmptyIsBareRet = true;

    public static bool IsLoadableWaterBank(ReadOnlySpan<byte> bank) =>
        bank.Length >= 4 && BitConverter.ToUInt32(bank) == RequiredWaterBankType;

    /// <summary>
    /// Exe table <c>0x0139C5D8</c> uploaded via <c>00989A60</c> as VS
    /// float4s: <c>0.125</c> / <c>-0.125</c> to <c>c3</c>. First-seen
    /// <c>VSHADER_LANDSCAPE_FOREGROUND</c> does <c>add r3, r3, c3</c>
    /// (lighting) and <c>mov oT0.xy, v3.yz</c> — not <c>mad oT0</c>
    /// from world XY. Albedo is <c>oT1</c> from <c>dp4(pos,c40/c41)</c>.
    /// Cell lookup still uses <c>&gt;&gt;4</c> (16 m).
    /// </summary>
    public const float UvScale = 0.125f;
    public const uint UvTable = 0x0139C5D8;
    public const uint UvTable2 = 0x0139C614;
    public const uint PerCellDraw = 0x00BF4570;
    public const uint PerCellC1Upload = 0x00BF51D4;
    public const int LayerFlipRegister = 1;
    /// <summary>
    /// <c>00BF5175</c> <c>cmp ebp, 4</c> then <c>00989A60(1)</c>.
    /// Type 4 is the water enqueue (<c>00BF44B3</c>). First-seen FG
    /// is not type 4, so c1 is not written.
    /// </summary>
    public const int C1LayerType = 4;
    public const bool FirstSeenUploadsC1LayerFlip = false;
    public const bool FirstSeenLandscapeVsReadsC1 = false;
    public static readonly System.Numerics.Vector2 C1OneLayer = new(1f, 0f);
    public static readonly System.Numerics.Vector2 C1TwoLayer = new(0f, -1f);

    /// <summary>
    /// <c>00BFE050</c> (only <c>E8</c> from <c>00BF3E17</c>) locks a
    /// VB via <c>00BDA3D0</c> → <c>00A63150</c> with a literal
    /// stride <c>24</c> (<c>mov [esi+16], bl</c>). File verts stay
    /// 15 bytes. Per vert: u16 X / u16 Y / f32 Z, then
    /// <c>00BFDEC0</c> unpacks the 11-11-10 normal to float3 at
    /// dest+8, then extra bytes land at dest+20/+21/+22 as
    /// extra[2], extra[1], extra[0] (D3DCOLOR BGR). dest+23 is not
    /// written.
    /// </summary>
    public const int GpuVertexStrideBytes = 24;
    public const int GpuExtraOffset = 20;
    public const uint ExpandVerts = 0x00BFE050;
    public const uint ExpandVertsCaller = 0x00BF3E17;
    public const uint UnpackNormal = 0x00BFDEC0;
    public const uint CreateVertexBuffer = 0x00BDA3D0;
    public const uint CreateVertexBufferWrapper = 0x00A63150;

    /// <summary>
    /// FG VS <c>mov oT0.xy, v3.yz</c> / <c>mul oD0.w, …, v3.x</c>.
    /// v3 is the dest+20 D3DCOLOR: R = extra[0] = 0xFF so
    /// <c>v3.x=1</c>; G/B = extra[1]/extra[2]. Oakvale extras sit
    /// near 0.5. That is t0 (PS <c>t0.a</c>), not albedo.
    /// </summary>
    public const bool FirstSeenOt0FromV3 = true;
    public const bool FirstSeenOt0IsAlbedo = false;
    /// <summary>
    /// <c>VSHADER_LANDSCAPE_BACKGROUND</c> is <c>mov oT0, v3</c>
    /// so bit-4 samples ExtraRgb.XY. FG uses ExtraRgb.YZ.
    /// Live mesh shader swizzles Extra the same way.
    /// </summary>
    public const bool FirstSeenBackgroundOt0IsV3 = true;
    public const bool FirstSeenBackgroundPsMulX2 = true;

    /// <summary>
    /// FG VS albedo: <c>dp4 r5.x, pos, c40</c>;
    /// <c>dp4 r5.y, pos, c41</c>; <c>mov oT1, r5</c>.
    /// PS RGB is <c>t1 * v0</c>, so the visible UV is oT1.
    /// Per-cell <c>00BF514F</c> <c>push edi</c> with <c>edi=2</c>
    /// then <c>00989A60</c> writes table <c>0x0139C5D8</c> to
    /// <c>[inner+20]+2</c>; <c>push 3</c> writes
    /// <c>0x0139C614</c> to <c>+3</c>. Inner ctor
    /// <c>0098D4A0</c> leaves <c>[esi+20]=0</c> (record at
    /// <c>+16</c>), so those go to <c>c2</c>/<c>c3</c>. Fog
    /// flush restores <c>c2</c>; <c>c3</c> stays the table
    /// (lighting <c>add r3,r3,c3</c>). No <c>def c40</c> in
    /// the FG VS. No layout field 40. No <c>push 40</c> /
    /// <c>mov r,40</c> SetVSConstantF on the first-seen path.
    /// D3D9 default for an unwritten VS constant is 0, so
    /// first-seen <c>c40=c41=(0,0,0,0)</c> and
    /// <c>oT1=(0,0)</c>. <see cref="UvScale"/> is the <c>c3</c>
    /// table, not oT1.
    /// </summary>
    public const int Ot1RegisterX = 40;
    public const int Ot1RegisterY = 41;
    public const int PerCellFirstSlot = 2;
    public const int PerCellSecondSlot = 3;
    public const uint PerCellFirstSlotSet = 0x00BF4EB7;
    public const uint SetVsConstantF1 = 0x00989A60;
    public const uint InnerVsObjectCtor = 0x0098D4A0;
    public const int InnerRegisterBaseOffset = 20;
    public const int FirstSeenInnerRegisterBase = 0;
    public const bool FirstSeenOt1Projected = true;
    public const bool FirstSeenOt1HasExplicitWriter = false;
    public const bool FirstSeenOt1UsesDeviceDefault = true;
    public static readonly System.Numerics.Vector4 Ot1C40 = System.Numerics.Vector4.Zero;
    public static readonly System.Numerics.Vector4 Ot1C41 = System.Numerics.Vector4.Zero;

    /// <summary>
    /// FG VS: <c>dp3 r5, r2, c42</c>; <c>add r4, r5, c42.w</c>;
    /// <c>mul oD0.w, r4, v3.x</c>. No first-seen <c>push 42</c> /
    /// <c>def c42</c> / layout field 42. D3D default is 0, so
    /// first-seen <c>oD0.w=0</c>. Landscape alphablend is off,
    /// so RGB still draws. BG / static / PALSKIN are
    /// <c>mov oD0.w, c0.y</c> and do not read c42.
    /// </summary>
    public const int Od0WFadeRegister = 42;
    public const bool FirstSeenForegroundOd0WUsesC42 = true;
    public const bool FirstSeenWritesC42 = false;
    public static readonly System.Numerics.Vector4 FirstSeenC42 = System.Numerics.Vector4.Zero;

    public static float EvaluateForegroundOd0W(
        System.Numerics.Vector3 r2, System.Numerics.Vector4 c42, float v3x) =>
        (r2.X * c42.X + r2.Y * c42.Y + r2.Z * c42.Z + c42.W) * v3x;

    /// <summary>
    /// FG VS: <c>mov r0.xy, v0</c>; <c>mov r0.z, v1.x</c>;
    /// <c>mov r0.w, c0.y</c>; <c>add r1, r0, -c4</c>;
    /// <c>dp4 oPos, r1, c5–c8</c>. <c>00B54310</c> writes inverse
    /// row 2 to <c>c4</c> only from mesh draw <c>00B555A0</c>
    /// (0 <c>E8</c>). Landscape draw <c>00B6B0B0</c> / per-cell
    /// <c>00BF4570</c> do not call it and do not
    /// <c>00989A60(4)</c>. D3D default <c>c4=(0,0,0,0)</c>.
    /// </summary>
    public const int OPosC4Register = 4;
    public const uint C4InverseRow2Upload = 0x00B545D5;
    public const uint C4InverseRow2UploadFn = 0x00B54310;
    public const uint C4InverseRow2UploadCaller = 0x00B555A0;
    public const bool FirstSeenOPosSubtractsC4 = true;
    public const bool FirstSeenUploadsC4InverseRow2OnLandscape = false;
    public const bool FirstSeenC4UsesDeviceDefault = true;
    public static readonly System.Numerics.Vector4 FirstSeenC4 = System.Numerics.Vector4.Zero;

    /// <summary>
    /// First-seen <c>c0.y=1</c>, so <c>r0.w=1</c> then
    /// <c>r1 = (pos, 1) - c4</c>.
    /// </summary>
    public static System.Numerics.Vector4 LandscapeOPosPosition(
        System.Numerics.Vector3 pos, System.Numerics.Vector4 c4, float c0y) =>
        new System.Numerics.Vector4(pos, c0y) - c4;

    public static System.Numerics.Vector2 Ot0FromExtra(System.Numerics.Vector3 extraRgb) =>
        new(extraRgb.Y, extraRgb.Z);

    public static System.Numerics.Vector2 ProjectOt1(System.Numerics.Vector3 pos)
    {
        var p = new System.Numerics.Vector4(pos, 1f);
        return new System.Numerics.Vector2(
            System.Numerics.Vector4.Dot(p, Ot1C40),
            System.Numerics.Vector4.Dot(p, Ot1C41));
    }

    public static bool IsUsable(string materialName) =>
        materialName.Length > 0 &&
        !materialName.StartsWith("INVALID", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Exe <c>OpenStaticMaps</c> <c>00B4282B</c> hands <c>SEA_*</c> to
    /// <c>CEngineWaterRenderer</c> via <c>__ENGINE_SEA_STATIC_MAP_BANK_FILE__</c>.
    /// <c>LoadWaterData</c> <c>00B41FA0</c> does the same for
    /// <c>__ENGINE_WATER_STATIC_MAP_BANK_FILE__</c>. Those are not landscape FG.
    /// </summary>
    public static bool IsWaterOrSeaPass(string materialName) =>
        materialName.StartsWith("WATER_", StringComparison.OrdinalIgnoreCase) ||
        materialName.StartsWith("SEA_", StringComparison.OrdinalIgnoreCase) ||
        materialName.Contains("LAKE", StringComparison.OrdinalIgnoreCase);

    public static int? TryResolve(string materialName, HeaderEnums? textures)
    {
        if (!IsUsable(materialName))
            return null;
        // LoadWaterData 00B41FA0 rejects unless the bank u32 is 8.
        // StartOakVale sea is 7363. Draw 00B783F0 returns when the
        // renderer vectors are empty. Not landscape FG.
        if (IsWaterOrSeaPass(materialName))
            return null;
        return textures is null ? DefaultId : Resolve(materialName, textures);
    }

    public static int WaterTexture(HeaderEnums? textures) =>
        textures is not null && textures.ByName.TryGetValue("LANDSCAPE_WATER", out var id)
            ? id
            : WaterId;

    public static int Resolve(string materialName, HeaderEnums textures)
    {
        foreach (var key in Candidates(materialName))
        {
            if (textures.ByName.TryGetValue(key, out var id) && !key.Contains("PROC_", StringComparison.Ordinal))
                return id;
        }

        var tokens = Tokens(materialName);
        var best = DefaultId;
        var bestScore = 0;
        foreach (var (name, id) in textures.ByName)
        {
            if (!name.StartsWith("LANDSCAPE_", StringComparison.Ordinal) ||
                name.Contains("PROC_", StringComparison.Ordinal) ||
                name.Contains("DIST_", StringComparison.Ordinal))
                continue;
            var score = 0;
            foreach (var token in Tokens(name))
            {
                if (tokens.Contains(token))
                    score += 2;
                else if (tokens.Any(t => name.Contains(t, StringComparison.Ordinal)))
                    score += 1;
            }
            if (score > bestScore)
            {
                bestScore = score;
                best = id;
            }
        }

        return bestScore > 0 ? best : DefaultId;
    }

    public static IEnumerable<string> Candidates(string materialName)
    {
        var rest = materialName;
        if (rest.StartsWith("GROUND_", StringComparison.Ordinal))
            rest = rest["GROUND_".Length..];
        if (rest.EndsWith("_ET", StringComparison.Ordinal))
            rest = rest[..^3];

        yield return "LANDSCAPE_" + materialName;
        yield return "LANDSCAPE_" + rest;
        yield return "LANDSCAPE_" + rest + "_01";
        yield return "LANDSCAPE_" + rest + "_PLAIN";
        if (rest.Contains("COBBLE", StringComparison.Ordinal))
            yield return "LANDSCAPE_COBBLES_IRREGULAR_01";
        if (rest.Contains("FOREST", StringComparison.Ordinal) || rest.Contains("LEAF", StringComparison.Ordinal))
            yield return "LANDSCAPE_FORESTFLOOR";
        if (rest == "GRASS")
            yield return "LANDSCAPE_GRASS_PLAIN";
        if (rest.Contains("POPPY", StringComparison.Ordinal))
            yield return "LANDSCAPE_PROC_POPPY";
        if (rest.Contains("DANDELION", StringComparison.Ordinal))
            yield return "LANDSCAPE_PROC_DANDELIONS";
        if (materialName.StartsWith("WATER_", StringComparison.Ordinal) ||
            rest.Contains("WATER", StringComparison.Ordinal) ||
            rest.Contains("LAKE", StringComparison.Ordinal))
            yield return "LANDSCAPE_WATER";
    }

    private static HashSet<string> Tokens(string name)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in name.Split('_', StringSplitOptions.RemoveEmptyEntries))
        {
            if (part is "GROUND" or "LANDSCAPE" or "PROC" or "DIST" or "01" or "02" or "ET" or "THE")
                continue;
            set.Add(part);
        }
        return set;
    }
}
