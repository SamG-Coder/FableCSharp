namespace Fable.Formats.Shaders;

/// <summary>
/// One program from data\shaders\pc\shaders.big. Payload is a u32 byte
/// length then Direct3D 8/9 tokens (vs_1_1 / ps_1_1 / ps_1_4).
/// </summary>
public sealed class ShaderProgram
{
    public const uint VertexVersionTag = 0xFFFE;
    public const uint PixelVersionTag = 0xFFFF;
    public const uint TexOpcode = 0x42;
    public const uint MulOpcode = 0x05;
    public const uint MovOpcode = 0x01;
    public const uint AddOpcode = 0x02;
    public const uint MadOpcode = 0x04;
    public const uint RsqOpcode = 0x07;
    public const uint Dp3Opcode = 0x08;
    public const uint Dp4Opcode = 0x09;
    public const uint MinOpcode = 0x0A;
    public const uint MaxOpcode = 0x0B;
    public const uint DclOpcode = 0x1F;
    public const uint DefOpcode = 0x51;
    public const uint CommentOpcode = 0xFFFE;
    public const uint EndOpcode = 0xFFFF;
    /// <summary>D3D vs_1_1 <c>LIT</c>. First-seen FG/static/PALSKIN use <c>MAD</c> <c>c35</c> instead.</summary>
    public const uint LitOpcode = 0x10;
    /// <summary>D3D <c>LRP</c>. <c>PSHADER_INNER_SKY</c> uses this; landscape FG does not.</summary>
    public const uint LrpOpcode = 0x12;
    public const int RegTypeConst = 2;
    public const int RegTypeInput = 1;
    public const int RegTypeRastOut = 4;
    public const int RegTypeAttrOut = 5;
    public const int RegTypeTexCrdOut = 6;
    public const int RastOutFog = 1;
    public const int SrcModNeg = 1;
    public const int SwizzleY = 1;
    public const int SwizzleZ = 2;

    public required string Name { get; init; }
    public required string Bank { get; init; }
    public required uint EntryType { get; init; }
    public required int DeclaredSize { get; init; }
    public required uint VersionToken { get; init; }
    public required byte[] Tokens { get; init; }

    /// <summary>
    /// Printable ASCII runs in the token blob, including the
    /// trailing CTAB after <c>end</c>. First-seen
    /// <c>PSHADER_TEXTURE_DIFFUSE</c> names
    /// <c>PSCONST_OUTPUT_FACTOR</c>.
    /// </summary>
    public IReadOnlyList<string> CommentStrings
    {
        get
        {
            var names = new List<string>();
            var i = 0;
            while (i < Tokens.Length)
            {
                if (Tokens[i] is < 32 or >= 127)
                {
                    i++;
                    continue;
                }

                var start = i;
                while (i < Tokens.Length && Tokens[i] is >= 32 and < 127)
                    i++;
                if (i - start >= 4)
                    names.Add(System.Text.Encoding.ASCII.GetString(Tokens, start, i - start));
            }

            return names;
        }
    }

    public bool IsPixel => VersionTag == PixelVersionTag;
    public bool IsVertex => VersionTag == VertexVersionTag;
    public uint VersionTag => VersionToken >> 16;
    public int Major => (int)((VersionToken >> 8) & 0xFF);
    public int Minor => (int)(VersionToken & 0xFF);
    public string Profile => $"{(IsPixel ? "ps" : "vs")}_{Major}_{Minor}";

    public int TexCount
    {
        get
        {
            var n = 0;
            for (var i = 0; i + 4 <= Tokens.Length; i += 4)
            {
                if (BitConverter.ToUInt32(Tokens, i) == TexOpcode)
                    n++;
            }

            return n;
        }
    }

    /// <summary>
    /// ps_1.1 dest shift 1 is <c>_x2</c>: <c>sat(2 * src0 * src1)</c>.
    /// Landscape FG and the object fog shaders use this, not a lerp.
    /// </summary>
    public bool HasMulX2
    {
        get
        {
            for (var i = 0; i + 8 <= Tokens.Length; i += 4)
            {
                if (BitConverter.ToUInt32(Tokens, i) != MulOpcode)
                    continue;
                var dest = BitConverter.ToUInt32(Tokens, i + 4);
                if (((dest >> 24) & 0xF) == 1)
                    return true;
            }

            return false;
        }
    }

    public bool HasLit
    {
        get
        {
            for (var i = 0; i + 4 <= Tokens.Length; i += 4)
            {
                if (BitConverter.ToUInt32(Tokens, i) == LitOpcode)
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// D3D register numbers of type CONST (2) in dest/src tokens.
    /// vs_1_1 / ps_1_1 use the D3D8-style type field at bits 28–30.
    /// </summary>
    public IReadOnlyList<int> ConstRegisters
    {
        get
        {
            var set = new SortedSet<int>();
            for (var i = 4; i + 4 <= Tokens.Length; i += 4)
            {
                var token = BitConverter.ToUInt32(Tokens, i);
                if (token is 0x0000FFFF or 0x0000FFFE)
                    continue;
                var type = (int)((token >> 28) & 7);
                if (type != 2)
                    continue;
                var reg = (int)(token & 0x7FF);
                if (reg is >= 0 and < 96)
                    set.Add(reg);
            }

            return set.ToList();
        }
    }

    /// <summary>
    /// First-seen FG / static / PALSKIN:
    /// <c>dp4 r.x, pos, c2</c>; <c>mov r.w, c0.y</c>;
    /// <c>min r.x, r.x, r.w</c>;
    /// <c>mad oFog, r.x, -c18.w, r.w</c>.
    /// </summary>
    public bool TryGetVertexFogSequence(out VertexFogSequence seq)
    {
        seq = default;
        var insns = DecodeInstructions();
        for (var i = 0; i + 3 < insns.Count; i++)
        {
            var dp4 = insns[i];
            var mov = insns[i + 1];
            var min = insns[i + 2];
            var mad = insns[i + 3];
            if (dp4.Opcode != Dp4Opcode || mov.Opcode != MovOpcode
                || min.Opcode != MinOpcode || mad.Opcode != MadOpcode)
                continue;
            if (!dp4.Src1Is(RegTypeConst, 2) || dp4.DestType != 0)
                continue;
            if (!mov.Src0Is(RegTypeConst, 0) || !mov.Src0SwizzleY
                || mov.DestNum != dp4.DestNum || !mov.DestMaskW)
                continue;
            if (min.DestNum != dp4.DestNum || min.Src0Num != dp4.DestNum
                || min.Src1Num != dp4.DestNum || !min.Src1SwizzleW)
                continue;
            if (mad.DestType != RegTypeRastOut || mad.DestNum != RastOutFog)
                continue;
            if (!mad.Src1Is(RegTypeConst, 18) || mad.Src1Mod != SrcModNeg
                || !mad.Src1SwizzleW)
                continue;
            if (mad.Src0Num != dp4.DestNum || mad.Src2Num != dp4.DestNum
                || !mad.Src2SwizzleW)
                continue;
            seq = new VertexFogSequence(dp4.Src0Num, dp4.Src0Type);
            return true;
        }

        return false;
    }

    /// <summary>
    /// First-seen inner/outer sky: <c>dp4 oPos, v0, c5–c8</c>.
    /// No <c>c4</c> subtract.
    /// </summary>
    public bool TryGetOPosWvpC5C8()
    {
        var saw = new bool[4];
        foreach (var insn in DecodeInstructions())
        {
            if (insn.Opcode != Dp4Opcode)
                continue;
            if (!insn.Src1Is(RegTypeConst, insn.Src1Num) || insn.Src1Num is < 5 or > 8)
                continue;
            saw[insn.Src1Num - 5] = true;
        }

        return saw[0] && saw[1] && saw[2] && saw[3];
    }

    /// <summary>
    /// First-seen inner/outer sky: <c>dp4 oPos, v0, c5–c8</c>.
    /// No <c>c4</c> subtract.
    /// </summary>
    public bool TryGetSkyOPosWvp()
    {
        var insns = DecodeInstructions();
        if (insns.Count < 4)
            return false;
        for (var k = 0; k < 4; k++)
        {
            var dp = insns[k];
            if (dp.Opcode != Dp4Opcode || dp.DestType != RegTypeRastOut || dp.DestNum != 0)
                return false;
            if (dp.Src0Type != RegTypeInput || dp.Src0Num != 0
                || !dp.Src1Is(RegTypeConst, 5 + k))
                return false;
        }

        return true;
    }

    /// <summary>
    /// First-seen landscape FG: <c>mov oT0.xy, v3.yz</c>.
    /// That is t0 (PS alpha), not the albedo UV.
    /// </summary>
    public bool TryGetOt0FromV3(out int vReg)
    {
        vReg = 0;
        foreach (var insn in DecodeInstructions())
        {
            if (insn.Opcode != MovOpcode)
                continue;
            if (insn.DestType != RegTypeTexCrdOut || insn.DestNum != 0 || !insn.DestMaskXYOnly)
                continue;
            if (insn.Src0Type != RegTypeInput)
                continue;
            if (insn.Src0Swizzle0 != SwizzleY || insn.Src0Swizzle1 != SwizzleZ)
                continue;
            vReg = insn.Src0Num;
            return true;
        }

        return false;
    }

    /// <summary>
    /// First-seen landscape BG: <c>mov oT0, v3</c> then
    /// <c>mov oT0.w, c0.y</c>. Sample UV is ExtraRgb.XY, not YZ.
    /// </summary>
    public bool TryGetBackgroundOt0FromV3(out int vReg)
    {
        vReg = 0;
        foreach (var insn in DecodeInstructions())
        {
            if (insn.Opcode != MovOpcode)
                continue;
            if (insn.DestType != RegTypeTexCrdOut || insn.DestNum != 0)
                continue;
            if (!insn.DestMaskXYZW)
                continue;
            if (insn.Src0Type != RegTypeInput)
                continue;
            if (insn.Src0Swizzle0 != 0 || insn.Src0Swizzle1 != 1)
                continue;
            vReg = insn.Src0Num;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Static / PALSKIN: <c>mov oT0, vN</c> (full mask, xyzw).
    /// First-seen static is <c>v2</c> (FVF TEX1); PALSKIN is <c>v4</c>.
    /// </summary>
    public bool TryGetOt0FromInput(out int vReg)
    {
        vReg = 0;
        foreach (var insn in DecodeInstructions())
        {
            if (insn.Opcode != MovOpcode)
                continue;
            if (insn.DestType != RegTypeTexCrdOut || insn.DestNum != 0)
                continue;
            if (!insn.DestMaskXYZW)
                continue;
            if (insn.Src0Type != RegTypeInput)
                continue;
            if (insn.Src0Swizzle0 != 0 || insn.Src0Swizzle1 != 1)
                continue;
            vReg = insn.Src0Num;
            return true;
        }

        return false;
    }

    /// <summary>
    /// First-seen landscape FG:
    /// <c>mov r0.xy, v0</c>; <c>mov r0.z, v1.x</c>;
    /// <c>mov r0.w, c0.y</c>; <c>add r1, r0, -c4</c>;
    /// <c>dp4 oPos, r1, c5–c8</c>. Static / PALSKIN have no
    /// <c>c4</c> and do not take this sequence.
    /// </summary>
    public bool TryGetOPosSubtractC4(out OPosSubtractC4 seq)
    {
        seq = default;
        var insns = DecodeInstructions();
        for (var i = 0; i + 7 < insns.Count; i++)
        {
            var xy = insns[i];
            var z = insns[i + 1];
            var w = insns[i + 2];
            var sub = insns[i + 3];
            if (xy.Opcode != MovOpcode || z.Opcode != MovOpcode || w.Opcode != MovOpcode
                || sub.Opcode != AddOpcode)
                continue;
            if (xy.DestType != 0 || !xy.DestMaskXYOnly || xy.Src0Type != RegTypeInput)
                continue;
            if (z.DestType != 0 || z.DestNum != xy.DestNum || !z.DestMaskZ || z.DestMaskX
                || z.Src0Type != RegTypeInput)
                continue;
            if (w.DestType != 0 || w.DestNum != xy.DestNum || !w.DestMaskW
                || !w.Src0Is(RegTypeConst, 0) || !w.Src0SwizzleY)
                continue;
            if (sub.DestType != 0 || !sub.DestMaskXYZW || sub.Src0Num != xy.DestNum
                || !sub.Src1Is(RegTypeConst, 4) || sub.Src1Mod != SrcModNeg)
                continue;
            var ok = true;
            for (var k = 0; k < 4; k++)
            {
                var dp = insns[i + 4 + k];
                if (dp.Opcode != Dp4Opcode || dp.DestType != RegTypeRastOut || dp.DestNum != 0)
                {
                    ok = false;
                    break;
                }

                if (dp.Src0Num != sub.DestNum || !dp.Src1Is(RegTypeConst, 5 + k))
                {
                    ok = false;
                    break;
                }
            }

            if (!ok)
                continue;
            seq = new OPosSubtractC4(xy.Src0Num, z.Src0Num, sub.DestNum);
            return true;
        }

        return false;
    }

    /// <summary>
    /// First-seen landscape FG albedo: <c>dp4 r.x, pos, c40</c>;
    /// <c>dp4 r.y, pos, c41</c>; <c>mov oT1, r</c>.
    /// </summary>
    public bool TryGetOt1Projected(out Ot1Projected seq)
    {
        seq = default;
        var insns = DecodeInstructions();
        for (var i = 0; i + 2 < insns.Count; i++)
        {
            var dpX = insns[i];
            var dpY = insns[i + 1];
            var mov = insns[i + 2];
            if (dpX.Opcode != Dp4Opcode || dpY.Opcode != Dp4Opcode || mov.Opcode != MovOpcode)
                continue;
            if (dpX.DestType != 0 || dpY.DestType != 0 || dpX.DestNum != dpY.DestNum)
                continue;
            if (!dpX.DestMaskX || !dpY.DestMaskY)
                continue;
            if (!dpX.Src1Is(RegTypeConst, 40) || !dpY.Src1Is(RegTypeConst, 41))
                continue;
            if (mov.DestType != RegTypeTexCrdOut || mov.DestNum != 1)
                continue;
            if (mov.Src0Type != 0 || mov.Src0Num != dpX.DestNum)
                continue;
            seq = new Ot1Projected(dpX.Src0Num, dpX.Src0Type);
            return true;
        }

        return false;
    }

    /// <summary>
    /// vs_1_1 <c>def cN</c> tokens. First-seen landscape FG has none
    /// for c40/c41 — those registers are not baked into the program.
    /// </summary>
    public bool HasConstDef(int register)
    {
        var i = 4;
        while (i + 4 <= Tokens.Length)
        {
            var head = BitConverter.ToUInt32(Tokens, i);
            var op = head & 0xFFFF;
            if (op == EndOpcode)
                break;
            if (op == CommentOpcode)
            {
                i += 4 + (int)((head >> 16) & 0x7FFF) * 4;
                continue;
            }
            if (op == DclOpcode)
            {
                i += 12;
                continue;
            }
            if (op == DefOpcode)
            {
                if (i + 24 > Tokens.Length)
                    break;
                var dest = BitConverter.ToUInt32(Tokens, i + 4);
                if ((int)(dest & 0x7FF) == register)
                    return true;
                i += 24;
                continue;
            }

            i += 8 + SrcCount(op) * 4;
        }

        return false;
    }

    public IReadOnlyList<DecodedInsn> DecodeInstructions()
    {
        var list = new List<DecodedInsn>();
        var i = 4;
        while (i + 4 <= Tokens.Length)
        {
            var head = BitConverter.ToUInt32(Tokens, i);
            var op = head & 0xFFFF;
            if (op == EndOpcode)
                break;
            if (op == CommentOpcode)
            {
                var n = (int)((head >> 16) & 0x7FFF);
                i += 4 + n * 4;
                continue;
            }

            if (op == DclOpcode)
            {
                i += 12;
                continue;
            }

            if (op == DefOpcode)
            {
                i += 24;
                continue;
            }

            var srcs = SrcCount(op);
            var need = 8 + srcs * 4;
            if (i + need > Tokens.Length)
                break;
            var dest = BitConverter.ToUInt32(Tokens, i + 4);
            var s0 = srcs > 0 ? BitConverter.ToUInt32(Tokens, i + 8) : 0u;
            var s1 = srcs > 1 ? BitConverter.ToUInt32(Tokens, i + 12) : 0u;
            var s2 = srcs > 2 ? BitConverter.ToUInt32(Tokens, i + 16) : 0u;
            list.Add(new DecodedInsn(op, dest, s0, s1, s2));
            i += need;
        }

        return list;
    }

    /// <summary>
    /// Human-readable vs_1_1 / ps_1_1 listing for ExeIndex <c>out/</c>.
    /// Includes <c>def</c> so missing first-seen constants stay visible.
    /// </summary>
    public string ToListing()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(Profile);
        var i = 4;
        while (i + 4 <= Tokens.Length)
        {
            var head = BitConverter.ToUInt32(Tokens, i);
            var op = head & 0xFFFF;
            if (op == EndOpcode)
            {
                sb.AppendLine("end");
                break;
            }

            if (op == CommentOpcode)
            {
                var n = (int)((head >> 16) & 0x7FFF);
                sb.AppendLine($"; comment {n} dwords");
                i += 4 + n * 4;
                continue;
            }

            if (op == DclOpcode)
            {
                i += 12;
                continue;
            }

            if (op == DefOpcode)
            {
                if (i + 24 > Tokens.Length)
                    break;
                var dest = BitConverter.ToUInt32(Tokens, i + 4);
                var x = BitConverter.ToSingle(Tokens, i + 8);
                var y = BitConverter.ToSingle(Tokens, i + 12);
                var z = BitConverter.ToSingle(Tokens, i + 16);
                var w = BitConverter.ToSingle(Tokens, i + 20);
                sb.AppendLine($"def {FormatReg((int)((dest >> 28) & 7), (int)(dest & 0x7FF))}, {x}, {y}, {z}, {w}");
                i += 24;
                continue;
            }

            var srcs = SrcCount(op);
            var need = 8 + srcs * 4;
            if (i + need > Tokens.Length)
                break;
            var d = BitConverter.ToUInt32(Tokens, i + 4);
            sb.Append(OpcodeName(op));
            sb.Append(DestShiftName((int)((d >> 24) & 0xF)));
            if (((d >> 20) & 0xF) == 1)
                sb.Append("_sat");
            sb.Append(' ');
            sb.Append(FormatDest(d));
            for (var s = 0; s < srcs; s++)
            {
                sb.Append(", ");
                sb.Append(FormatSrc(BitConverter.ToUInt32(Tokens, i + 8 + s * 4)));
            }

            sb.AppendLine();
            i += need;
        }

        return sb.ToString();
    }

    /// <summary>
    /// <c>PSHADER_INNER_SKY</c>: four <c>tex</c>, <c>lrp</c> on
    /// <c>c0.w</c>/<c>c1.w</c>, then <c>mul_x2 …, c2</c>. No
    /// <c>def c0/c1/c2</c> — those come from the device.
    /// </summary>
    public bool TryGetInnerSkyPs()
    {
        if (TexCount != 4 || HasConstDef(0) || HasConstDef(1) || HasConstDef(2))
            return false;
        var sawLrpC0 = false;
        var sawLrpC1 = false;
        var sawMulX2C2 = false;
        foreach (var insn in DecodeInstructions())
        {
            if (insn.Opcode == LrpOpcode && insn.Src0Is(RegTypeConst, 0) && insn.Src0SwizzleW == 3)
                sawLrpC0 = true;
            if (insn.Opcode == LrpOpcode && insn.Src0Is(RegTypeConst, 1) && insn.Src0SwizzleW == 3)
                sawLrpC1 = true;
            if (insn.Opcode == MulOpcode && insn.DestShift == 1 && insn.Src1Is(RegTypeConst, 2))
                sawMulX2C2 = true;
        }

        return sawLrpC0 && sawLrpC1 && sawMulX2C2;
    }

    /// <summary>
    /// <c>PSHADER_INNER_SKY_SIMPLE</c>: two <c>tex</c>, last
    /// <c>mul_x2 …, c1</c>. No <c>def c0/c1</c>.
    /// </summary>
    /// <summary>
    /// <c>PSHADER_TEXTURE_DIFFUSE</c>: one <c>tex</c>,
    /// <c>mul r0, v0, c0</c>, then <c>mul_x2 … t0</c>.
    /// </summary>
    public bool TryGetTextureDiffusePs()
    {
        if (TexCount != 1 || !HasMulX2)
            return false;
        var sawMulV0C0 = false;
        var sawMulX2T0 = false;
        foreach (var insn in DecodeInstructions())
        {
            if (insn.Opcode == MulOpcode && insn.DestShift == 0
                && insn.Src0Is(RegTypeInput, 0) && insn.Src1Is(RegTypeConst, 0))
                sawMulV0C0 = true;
            if (insn.Opcode == MulOpcode && insn.DestShift == 1)
                sawMulX2T0 = true;
        }

        return sawMulV0C0 && sawMulX2T0;
    }

    public bool TryGetInnerSkySimplePs()
    {
        if (TexCount != 2 || HasConstDef(0) || HasConstDef(1))
            return false;
        foreach (var insn in DecodeInstructions())
        {
            if (insn.Opcode == MulOpcode && insn.DestShift == 1 && insn.Src1Is(RegTypeConst, 1))
                return true;
        }

        return false;
    }

    private static string OpcodeName(uint op) => op switch
    {
        MovOpcode => "mov",
        AddOpcode => "add",
        0x03 => "sub",
        MadOpcode => "mad",
        MulOpcode => "mul",
        0x06 => "rcp",
        RsqOpcode => "rsq",
        Dp3Opcode => "dp3",
        Dp4Opcode => "dp4",
        MinOpcode => "min",
        MaxOpcode => "max",
        LitOpcode => "lit",
        LrpOpcode => "lrp",
        TexOpcode => "tex",
        0x41 => "texkill",
        _ => $"op_{op:X}",
    };

    private static string DestShiftName(int shift) => shift switch
    {
        1 => "_x2",
        2 => "_x4",
        3 => "_x8",
        0xF => "_d2",
        0xE => "_d4",
        0xD => "_d8",
        _ => "",
    };

    private static string FormatDest(uint dest)
    {
        var name = FormatReg((int)((dest >> 28) & 7), (int)(dest & 0x7FF));
        var mask = (int)((dest >> 16) & 0xF);
        if (mask == 0xF)
            return name;
        var sb = new System.Text.StringBuilder(name);
        sb.Append('.');
        if ((mask & 1) != 0) sb.Append('x');
        if ((mask & 2) != 0) sb.Append('y');
        if ((mask & 4) != 0) sb.Append('z');
        if ((mask & 8) != 0) sb.Append('w');
        return sb.ToString();
    }

    private static string FormatSrc(uint src)
    {
        var name = FormatReg((int)((src >> 28) & 7), (int)(src & 0x7FF));
        var mod = (int)((src >> 24) & 0xF);
        var prefix = mod switch
        {
            SrcModNeg => "-",
            6 => "1-",
            _ => "",
        };
        var x = (int)((src >> 16) & 3);
        var y = (int)((src >> 18) & 3);
        var z = (int)((src >> 20) & 3);
        var w = (int)((src >> 22) & 3);
        if (x == 0 && y == 1 && z == 2 && w == 3)
            return prefix + name;
        if (x == y && y == z && z == w)
            return prefix + name + "." + "xyzw"[x];
        return prefix + name + "." + "xyzw"[x] + "xyzw"[y] + "xyzw"[z] + "xyzw"[w];
    }

    private static string FormatReg(int type, int num) => type switch
    {
        0 => "r" + num,
        RegTypeInput => "v" + num,
        RegTypeConst => "c" + num,
        3 => "t" + num,
        RegTypeRastOut => num == RastOutFog ? "oFog" : "oPos",
        RegTypeAttrOut => "oD" + num,
        RegTypeTexCrdOut => "oT" + num,
        _ => "r" + type + "_" + num,
    };

    private static int SrcCount(uint op) => op switch
    {
        MovOpcode or 0x06 or RsqOpcode or 0x0E or 0x0F or LitOpcode or 0x13
            or 0x4E or 0x4F => 1,
        0x02 or 0x03 or MulOpcode or Dp3Opcode or Dp4Opcode or MinOpcode
            or MaxOpcode or 0x0C or 0x0D or 0x11 => 2,
        MadOpcode or 0x12 => 3,
        TexOpcode or 0x40 => 0,
        _ => 0,
    };

    /// <summary>
    /// First-seen FG/static/PALSKIN: <c>dp3 …, -c19</c> then
    /// <c>mul …, c20</c> / <c>mad …, c35</c> / <c>add …, c3</c>
    /// into <c>oD0.xyz</c>.
    /// </summary>
    public bool TryGetDirLightAddsC3()
    {
        var sawNegC19 = false;
        var sawC20 = false;
        var sawC35 = false;
        foreach (var insn in DecodeInstructions())
        {
            if (insn.Opcode == Dp3Opcode && insn.Src1Is(RegTypeConst, 19)
                && insn.Src1Mod == SrcModNeg)
                sawNegC19 = true;
            if (insn.Opcode == MulOpcode && insn.Src1Is(RegTypeConst, 20))
                sawC20 = true;
            if (insn.Opcode == MadOpcode && insn.Src1Is(RegTypeConst, 35))
                sawC35 = true;
            if (sawNegC19 && sawC20 && sawC35 && insn.Opcode == AddOpcode
                && insn.Src1Is(RegTypeConst, 3))
                return true;
        }

        return false;
    }

    /// <summary>
    /// PALSKIN: <c>mov a0.x, …</c> then <c>mul/mad …, c38</c> with
    /// the vs_1_1 relative-address bit (0x2000) on the const src.
    /// </summary>
    public bool TryGetPalskinA0RelativeC38()
    {
        var sawAddr = false;
        foreach (var insn in DecodeInstructions())
        {
            if (insn.Opcode == MovOpcode && insn.DestType == 3)
                sawAddr = true;
            if (sawAddr && (insn.Opcode == MulOpcode || insn.Opcode == MadOpcode)
                && insn.Src1Is(RegTypeConst, 38) && (insn.Src1 & 0x2000) != 0)
                return true;
        }

        return false;
    }

    /// <summary>
    /// <c>mov oD0.w, c0.y</c>. First-seen static / PALSKIN / landscape
    /// background. Not the FG <c>c42</c> fade.
    /// </summary>
    public bool TryGetOd0WFromC0Y()
    {
        foreach (var insn in DecodeInstructions())
        {
            if (insn.Opcode == MovOpcode && insn.DestType == RegTypeAttrOut
                && insn.DestNum == 0 && insn.DestMask == 8
                && insn.Src0Is(RegTypeConst, 0) && insn.Src0SwizzleW == SwizzleY)
                return true;
        }

        return false;
    }

    /// <summary>
    /// FG: <c>dp3 r, r, c42</c>; <c>add r, r, c42.w</c>;
    /// <c>mul oD0.w, r, v3.x</c>.
    /// </summary>
    public bool TryGetForegroundOd0WFromC42()
    {
        var sawDp3 = false;
        var sawAdd = false;
        foreach (var insn in DecodeInstructions())
        {
            if (insn.Opcode == Dp3Opcode && insn.Src1Is(RegTypeConst, 42))
                sawDp3 = true;
            if (insn.Opcode == AddOpcode && insn.Src1Is(RegTypeConst, 42))
                sawAdd = true;
            if (sawDp3 && sawAdd && insn.Opcode == MulOpcode
                && insn.DestType == RegTypeAttrOut && insn.DestNum == 0
                && insn.DestMask == 8 && insn.Src1Is(RegTypeInput, 3))
                return true;
        }

        return false;
    }

    public readonly record struct VertexFogSequence(int PosRegister, int PosType);

    public readonly record struct Ot1Projected(int PosRegister, int PosType);

    public readonly record struct OPosSubtractC4(int XyInput, int ZInput, int SubtractDest);

    public readonly record struct DecodedInsn(uint Opcode, uint Dest, uint Src0, uint Src1, uint Src2)
    {
        public int DestNum => (int)(Dest & 0x7FF);
        public int DestType => (int)((Dest >> 28) & 7);
        public int DestMask => (int)((Dest >> 16) & 0xF);
        public int DestShift => (int)((Dest >> 24) & 0xF);
        public int Src2Type => (int)((Src2 >> 28) & 7);
        public bool DestMaskX => (DestMask & 1) != 0;
        public bool DestMaskY => (DestMask & 2) != 0;
        public bool DestMaskZ => (DestMask & 4) != 0;
        public bool DestMaskW => (Dest & 0x00080000) != 0;
        public bool DestMaskXYOnly => DestMask == 3;
        public bool DestMaskXYZW => DestMask == 0xF;
        public int Src0Num => (int)(Src0 & 0x7FF);
        public int Src0Type => (int)((Src0 >> 28) & 7);
        public int Src0Swizzle0 => (int)((Src0 >> 16) & 3);
        public int Src0Swizzle1 => (int)((Src0 >> 18) & 3);
        public int Src0SwizzleW => (int)((Src0 >> 22) & 3);
        public bool Src0SwizzleY => Src0Swizzle0 == 1;
        public int Src1Num => (int)(Src1 & 0x7FF);
        public int Src1Mod => (int)((Src1 >> 24) & 0xF);
        public bool Src1SwizzleW => ((Src1 >> 16) & 3) == 3;
        public int Src2Num => (int)(Src2 & 0x7FF);
        public bool Src2SwizzleW => ((Src2 >> 16) & 3) == 3;
        public bool Src0Is(int type, int num) => Src0Type == type && Src0Num == num;
        public bool Src1Is(int type, int num) =>
            ((int)((Src1 >> 28) & 7)) == type && Src1Num == num;
    }

    public static ShaderProgram Parse(string name, string bank, uint entryType, byte[] data)
    {
        if (data.Length < 8)
            throw new InvalidDataException($"Shader '{name}' is too small ({data.Length}).");

        var declared = BitConverter.ToInt32(data, 0);
        var version = BitConverter.ToUInt32(data, 4);
        var tag = version >> 16;
        if (tag is not (VertexVersionTag or PixelVersionTag))
            throw new InvalidDataException($"Shader '{name}' is not a D3D vs/ps token stream (0x{version:X8}).");

        return new ShaderProgram
        {
            Name = name,
            Bank = bank,
            EntryType = entryType,
            DeclaredSize = declared,
            VersionToken = version,
            Tokens = data[4..],
        };
    }
}
