namespace Fable.ExeIndex;

/// <summary>
/// 32-bit x86 decoder for Fable.exe. Length must be correct: a missed
/// <c>0F B7</c> / <c>F6</c> / x87 used to emit <c>db</c> then a fake <c>ret</c>.
/// </summary>
internal static class X86
{
    public readonly record struct Step(uint Va, string Text, bool IsRet, uint? DirectCall);

    public static List<string> Disassemble(PeImage pe, int fileOffset, int maxInsns = 64) =>
        DisassembleCore(pe, fileOffset, maxInsns, stopOnRet: true);

    /// <summary>
    /// Same as <see cref="Disassemble"/> but does not stop at the first
    /// <c>ret</c>. Needed for functions with an early-out (Diffuse2X
    /// <c>0098B5E0</c> returns immediately on arg == -1).
    /// </summary>
    public static List<string> DisassembleAll(PeImage pe, int fileOffset, int maxInsns = 64) =>
        DisassembleCore(pe, fileOffset, maxInsns, stopOnRet: false);

    public static List<Step> Walk(PeImage pe, int fileOffset, int maxInsns, bool stopOnRet)
    {
        var steps = new List<Step>(maxInsns);
        var ip = fileOffset;
        var d = pe.Data;
        for (var n = 0; n < maxInsns && ip + 1 < d.Length; n++)
        {
            var start = ip;
            var look = ip;
            while (look < d.Length && d[look] is 0x66 or 0xF2 or 0xF3 or 0xF0 or 0x64 or 0x65 or 0x26 or 0x2E or 0x36 or 0x3E)
                look++;
            uint? call = null;
            if (look + 5 <= d.Length && d[look] == 0xE8)
            {
                var rel = BitConverter.ToInt32(d, look + 1);
                call = pe.Va(look + 5 + rel);
            }

            if (!TryDecode(pe, ref ip, out var text))
            {
                steps.Add(new Step(pe.Va(start), $"db 0x{d[start]:X2}", false, null));
                ip = start + 1;
                continue;
            }

            var ret = text is "ret" or "retn" || text.StartsWith("ret ", StringComparison.Ordinal);
            steps.Add(new Step(pe.Va(start), text, ret, call));
            if (stopOnRet && ret)
                break;
        }

        return steps;
    }

    private static List<string> DisassembleCore(PeImage pe, int fileOffset, int maxInsns, bool stopOnRet)
    {
        var lines = new List<string>();
        foreach (var step in Walk(pe, fileOffset, maxInsns, stopOnRet))
            lines.Add($"  //{step.Va:X8}: {step.Text}");
        return lines;
    }

    public static int FindPrologue(PeImage pe, int from, int maxBack = 2048)
    {
        var data = pe.Data;
        var lo = Math.Max(0, from - maxBack);
        for (var i = from; i >= lo + 2; i--)
        {
            if (data[i] == 0x55 && data[i + 1] == 0x8B && data[i + 2] == 0xEC)
                return i;
        }

        for (var i = from; i > lo + 1; i--)
        {
            if (data[i - 1] == 0xCC && data[i] != 0xCC && pe.InCode(i))
                return i;
        }

        return FindImmInsn(pe, from);
    }

    public static int FindImmInsn(PeImage pe, int immSite)
    {
        var data = pe.Data;
        if (immSite > 0 && data[immSite - 1] == 0x68)
            return immSite - 1;
        if (immSite > 0 && data[immSite - 1] is >= 0xB8 and <= 0xBF)
            return immSite - 1;
        return immSite;
    }

    private static bool TryDecode(PeImage pe, ref int ip, out string text)
    {
        text = "";
        var d = pe.Data;
        if (ip >= d.Length)
            return false;

        var opsize16 = false;
        var rep = "";
        while (ip < d.Length)
        {
            var p = d[ip];
            if (p == 0x66) { opsize16 = true; ip++; continue; }
            if (p == 0xF3) { rep = "rep "; ip++; continue; }
            if (p == 0xF2) { rep = "repne "; ip++; continue; }
            if (p is 0xF0 or 0x64 or 0x65 or 0x26 or 0x2E or 0x36 or 0x3E) { ip++; continue; }
            break;
        }

        if (ip >= d.Length)
            return false;
        var op = d[ip++];
        switch (op)
        {
            case 0x00: return ModRm(pe, d, ref ip, "add", rmFirst: true, out text, r8: true);
            case 0x01: return ModRm(pe, d, ref ip, "add", rmFirst: true, out text);
            case 0x02: return ModRm(pe, d, ref ip, "add", rmFirst: false, out text, r8: true);
            case 0x03: return ModRm(pe, d, ref ip, "add", rmFirst: false, out text);
            case 0x04: return AlImm8(d, ref ip, "add al", out text);
            case 0x05: return AlImm32(pe, d, ref ip, "add eax", out text, opsize16);
            case 0x08: return ModRm(pe, d, ref ip, "or", rmFirst: true, out text, r8: true);
            case 0x09: return ModRm(pe, d, ref ip, "or", rmFirst: true, out text);
            case 0x0A: return ModRm(pe, d, ref ip, "or", rmFirst: false, out text, r8: true);
            case 0x0B: return ModRm(pe, d, ref ip, "or", rmFirst: false, out text);
            case 0x0C: return AlImm8(d, ref ip, "or al", out text);
            case 0x0D: return AlImm32(pe, d, ref ip, "or eax", out text, opsize16);
            case 0x10: return ModRm(pe, d, ref ip, "adc", rmFirst: true, out text, r8: true);
            case 0x11: return ModRm(pe, d, ref ip, "adc", rmFirst: true, out text);
            case 0x12: return ModRm(pe, d, ref ip, "adc", rmFirst: false, out text, r8: true);
            case 0x13: return ModRm(pe, d, ref ip, "adc", rmFirst: false, out text);
            case 0x14: return AlImm8(d, ref ip, "adc al", out text);
            case 0x15: return AlImm32(pe, d, ref ip, "adc eax", out text, opsize16);
            case 0x18: return ModRm(pe, d, ref ip, "sbb", rmFirst: true, out text, r8: true);
            case 0x19: return ModRm(pe, d, ref ip, "sbb", rmFirst: true, out text);
            case 0x1A: return ModRm(pe, d, ref ip, "sbb", rmFirst: false, out text, r8: true);
            case 0x1B: return ModRm(pe, d, ref ip, "sbb", rmFirst: false, out text);
            case 0x1C: return AlImm8(d, ref ip, "sbb al", out text);
            case 0x1D: return AlImm32(pe, d, ref ip, "sbb eax", out text, opsize16);
            case 0x20: return ModRm(pe, d, ref ip, "and", rmFirst: true, out text, r8: true);
            case 0x21: return ModRm(pe, d, ref ip, "and", rmFirst: true, out text);
            case 0x22: return ModRm(pe, d, ref ip, "and", rmFirst: false, out text, r8: true);
            case 0x23: return ModRm(pe, d, ref ip, "and", rmFirst: false, out text);
            case 0x24: return AlImm8(d, ref ip, "and al", out text);
            case 0x25: return AlImm32(pe, d, ref ip, "and eax", out text, opsize16);
            case 0x28: return ModRm(pe, d, ref ip, "sub", rmFirst: true, out text, r8: true);
            case 0x29: return ModRm(pe, d, ref ip, "sub", rmFirst: true, out text);
            case 0x2A: return ModRm(pe, d, ref ip, "sub", rmFirst: false, out text, r8: true);
            case 0x2B: return ModRm(pe, d, ref ip, "sub", rmFirst: false, out text);
            case 0x2C: return AlImm8(d, ref ip, "sub al", out text);
            case 0x2D: return AlImm32(pe, d, ref ip, "sub eax", out text, opsize16);
            case 0x30: return ModRm(pe, d, ref ip, "xor", rmFirst: true, out text, r8: true);
            case 0x31: return ModRm(pe, d, ref ip, "xor", rmFirst: true, out text);
            case 0x32: return ModRm(pe, d, ref ip, "xor", rmFirst: false, out text, r8: true);
            case 0x33: return ModRm(pe, d, ref ip, "xor", rmFirst: false, out text);
            case 0x34: return AlImm8(d, ref ip, "xor al", out text);
            case 0x35: return AlImm32(pe, d, ref ip, "xor eax", out text, opsize16);
            case 0x38: return ModRm(pe, d, ref ip, "cmp", rmFirst: true, out text, r8: true);
            case 0x39: return ModRm(pe, d, ref ip, "cmp", rmFirst: true, out text);
            case 0x3A: return ModRm(pe, d, ref ip, "cmp", rmFirst: false, out text, r8: true);
            case 0x3B: return ModRm(pe, d, ref ip, "cmp", rmFirst: false, out text);
            case 0x3C: return AlImm8(d, ref ip, "cmp al", out text);
            case 0x3D: return AlImm32(pe, d, ref ip, "cmp eax", out text, opsize16);
            case 0x40: case 0x41: case 0x42: case 0x43:
            case 0x44: case 0x45: case 0x46: case 0x47:
                text = "inc " + Reg(op - 0x40);
                return true;
            case 0x48: case 0x49: case 0x4A: case 0x4B:
            case 0x4C: case 0x4D: case 0x4E: case 0x4F:
                text = "dec " + Reg(op - 0x48);
                return true;
            case 0x50: case 0x51: case 0x52: case 0x53:
            case 0x54: case 0x55: case 0x56: case 0x57:
                text = "push " + Reg(op - 0x50);
                return true;
            case 0x58: case 0x59: case 0x5A: case 0x5B:
            case 0x5C: case 0x5D: case 0x5E: case 0x5F:
                text = "pop " + Reg(op - 0x58);
                return true;
            case 0x60: text = "pushad"; return true;
            case 0x61: text = "popad"; return true;
            case 0x68:
                return PushImm(pe, d, ref ip, opsize16, out text);
            case 0x69:
                return Imul3(pe, d, ref ip, imm8: false, opsize16, out text);
            case 0x6A:
                if (ip >= d.Length) return false;
                text = $"push {(sbyte)d[ip++]}";
                return true;
            case 0x6B:
                return Imul3(pe, d, ref ip, imm8: true, opsize16, out text);
            case 0x70: return Rel8(pe, ref ip, "jo", out text);
            case 0x71: return Rel8(pe, ref ip, "jno", out text);
            case 0x72: return Rel8(pe, ref ip, "jb", out text);
            case 0x73: return Rel8(pe, ref ip, "jae", out text);
            case 0x74: return Rel8(pe, ref ip, "je", out text);
            case 0x75: return Rel8(pe, ref ip, "jne", out text);
            case 0x76: return Rel8(pe, ref ip, "jbe", out text);
            case 0x77: return Rel8(pe, ref ip, "ja", out text);
            case 0x78: return Rel8(pe, ref ip, "js", out text);
            case 0x79: return Rel8(pe, ref ip, "jns", out text);
            case 0x7A: return Rel8(pe, ref ip, "jp", out text);
            case 0x7B: return Rel8(pe, ref ip, "jnp", out text);
            case 0x7C: return Rel8(pe, ref ip, "jl", out text);
            case 0x7D: return Rel8(pe, ref ip, "jge", out text);
            case 0x7E: return Rel8(pe, ref ip, "jle", out text);
            case 0x7F: return Rel8(pe, ref ip, "jg", out text);
            case 0x80: return AluImm(pe, d, ref ip, immBytes: 1, r8: true, out text);
            case 0x81: return AluImm(pe, d, ref ip, immBytes: opsize16 ? 2 : 4, r8: false, out text);
            case 0x83: return AluImm(pe, d, ref ip, 1, false, out text, signed8: true);
            case 0x84: return ModRm(pe, d, ref ip, "test", rmFirst: true, out text, r8: true);
            case 0x85: return ModRm(pe, d, ref ip, "test", rmFirst: true, out text);
            case 0x86: return ModRm(pe, d, ref ip, "xchg", rmFirst: true, out text, r8: true);
            case 0x87: return ModRm(pe, d, ref ip, "xchg", rmFirst: true, out text);
            case 0x88: return ModRm(pe, d, ref ip, "mov", rmFirst: true, out text, r8: true);
            case 0x89: return ModRm(pe, d, ref ip, "mov", rmFirst: true, out text);
            case 0x8A: return ModRm(pe, d, ref ip, "mov", rmFirst: false, out text, r8: true);
            case 0x8B: return ModRm(pe, d, ref ip, "mov", rmFirst: false, out text);
            case 0x8D: return Lea(pe, d, ref ip, out text);
            case 0x8F: return Unary(pe, d, ref ip, "pop", out text);
            case 0x90: text = "nop"; return true;
            case 0x91: case 0x92: case 0x93:
            case 0x94: case 0x95: case 0x96: case 0x97:
                text = "xchg eax, " + Reg(op - 0x90);
                return true;
            case 0x98: text = opsize16 ? "cbw" : "cwde"; return true;
            case 0x99: text = opsize16 ? "cwd" : "cdq"; return true;
            case 0x9B: text = "wait"; return true;
            case 0x9C: text = "pushfd"; return true;
            case 0x9D: text = "popfd"; return true;
            case 0xA0: return Moffs(pe, d, ref ip, "mov al", store: false, out text);
            case 0xA1: return Moffs(pe, d, ref ip, opsize16 ? "mov ax" : "mov eax", store: false, out text);
            case 0xA2: return Moffs(pe, d, ref ip, "mov", store: true, out text, "al");
            case 0xA3: return Moffs(pe, d, ref ip, "mov", store: true, out text, opsize16 ? "ax" : "eax");
            case 0xA4: text = rep + "movsb"; return true;
            case 0xA5: text = rep + (opsize16 ? "movsw" : "movsd"); return true;
            case 0xA6: text = rep + "cmpsb"; return true;
            case 0xA7: text = rep + (opsize16 ? "cmpsw" : "cmpsd"); return true;
            case 0xA8: return AlImm8(d, ref ip, "test al", out text);
            case 0xA9: return AlImm32(pe, d, ref ip, "test eax", out text, opsize16);
            case 0xAA: text = rep + "stosb"; return true;
            case 0xAB: text = rep + (opsize16 ? "stosw" : "stosd"); return true;
            case 0xAC: text = rep + "lodsb"; return true;
            case 0xAD: text = rep + (opsize16 ? "lodsw" : "lodsd"); return true;
            case 0xAE: text = rep + "scasb"; return true;
            case 0xAF: text = rep + (opsize16 ? "scasw" : "scasd"); return true;
            case 0xB0: case 0xB1: case 0xB2: case 0xB3:
            case 0xB4: case 0xB5: case 0xB6: case 0xB7:
                if (ip >= d.Length) return false;
                text = $"mov {Reg8(op - 0xB0)}, 0x{d[ip++]:X2}";
                return true;
            case 0xB8: case 0xB9: case 0xBA: case 0xBB:
            case 0xBC: case 0xBD: case 0xBE: case 0xBF:
                return MovRegImm(pe, d, ref ip, op - 0xB8, opsize16, out text);
            case 0xC0: return Shift(pe, d, ref ip, "imm8", r8: true, out text);
            case 0xC1: return Shift(pe, d, ref ip, "imm8", r8: false, out text);
            case 0xC2:
                if (ip + 2 > d.Length) return false;
                text = $"ret {BitConverter.ToUInt16(d, ip)}";
                ip += 2;
                return true;
            case 0xC3:
                text = "ret";
                return true;
            case 0xC6: return MovImm(pe, d, ref ip, imm32: false, out text);
            case 0xC7: return MovImm(pe, d, ref ip, imm32: !opsize16, out text);
            case 0xC9: text = "leave"; return true;
            case 0xCC: text = "int3"; return true;
            case 0xCD:
                if (ip >= d.Length) return false;
                text = $"int 0x{d[ip++]:X2}";
                return true;
            case 0xD0: return Shift(pe, d, ref ip, "1", r8: true, out text);
            case 0xD1: return Shift(pe, d, ref ip, "1", r8: false, out text);
            case 0xD2: return Shift(pe, d, ref ip, "cl", r8: true, out text);
            case 0xD3: return Shift(pe, d, ref ip, "cl", r8: false, out text);
            case 0xD8: case 0xD9: case 0xDA: case 0xDB:
            case 0xDC: case 0xDD: case 0xDE: case 0xDF:
                return X87(pe, d, ref ip, op, out text);
            case 0xE0: return Rel8(pe, ref ip, "loopne", out text);
            case 0xE1: return Rel8(pe, ref ip, "loope", out text);
            case 0xE2: return Rel8(pe, ref ip, "loop", out text);
            case 0xE3: return Rel8(pe, ref ip, "jecxz", out text);
            case 0xE8: return Rel32(pe, ref ip, "call", out text);
            case 0xE9: return Rel32(pe, ref ip, "jmp", out text);
            case 0xEB: return Rel8(pe, ref ip, "jmp", out text);
            case 0xF6: return F6F7(pe, d, ref ip, wide: false, out text);
            case 0xF7: return F6F7(pe, d, ref ip, wide: !opsize16, out text);
            case 0xF8: text = "clc"; return true;
            case 0xF9: text = "stc"; return true;
            case 0xFC: text = "cld"; return true;
            case 0xFD: text = "std"; return true;
            case 0xFE: return IncDec(pe, d, ref ip, out text);
            case 0xFF: return Ff(d, pe, ref ip, out text);
            case 0x0F: return TwoByte(pe, d, ref ip, out text);
            default:
                return false;
        }
    }

    private static bool TwoByte(PeImage pe, byte[] d, ref int ip, out string text)
    {
        text = "";
        if (ip >= d.Length)
            return false;
        var op2 = d[ip++];
        if (op2 is >= 0x80 and <= 0x8F)
            return Rel32(pe, ref ip, Jcc(op2 - 0x80), out text);
        if (op2 is >= 0x90 and <= 0x9F)
            return Unary(pe, d, ref ip, "set" + Jcc(op2 - 0x90)[1..], out text);
        if (op2 is >= 0x40 and <= 0x4F)
            return ModRm(pe, d, ref ip, "cmov" + Jcc(op2 - 0x40)[1..], rmFirst: false, out text);

        var name = op2 switch
        {
            0x12 => "movlps",
            0x13 => "movlps",
            0x16 => "movhps",
            0x17 => "movhps",
            0x1F => "nop",
            0x10 or 0x11 => "movups",
            0x28 or 0x29 => "movaps",
            0x2E => "ucomiss",
            0x2F => "comiss",
            0x57 => "xorps",
            0x58 => "addps",
            0x59 => "mulps",
            0x5C => "subps",
            0x5D => "minps",
            0x5F => "maxps",
            0x70 => "pshufd",
            0xAF => "imul",
            0xB6 => "movzx",
            0xB7 => "movzx",
            0xBE => "movsx",
            0xBF => "movsx",
            0xA3 => "bt",
            0xAB => "bts",
            0xB3 => "btr",
            0xBB => "btc",
            0xBC => "bsf",
            0xBD => "bsr",
            0xC2 => "cmpps",
            0xC6 => "shufps",
            _ => $"0F_{op2:X2}",
        };

        // Almost every remaining 0F opcode takes ModR/M. Consume it so we
        // never return success with the IP still sitting on the operand.
        var rmFirst = op2 is 0xA3 or 0xAB or 0xB3 or 0xBB;
        if (!ModRm(pe, d, ref ip, name, rmFirst, out text, r8: op2 is 0xB6 or 0xBE))
            return false;

        // shufps / cmpps / pshufd / shld / shrd / bt-imm take an extra imm8.
        if (op2 is 0x70 or 0x71 or 0x72 or 0x73 or 0xA4 or 0xAC or 0xBA or 0xC2 or 0xC4 or 0xC5 or 0xC6)
        {
            if (ip >= d.Length)
                return false;
            text += $", {d[ip++]}";
        }

        return true;
    }

    private static string Jcc(int cc) => cc switch
    {
        0 => "jo", 1 => "jno", 2 => "jb", 3 => "jae",
        4 => "je", 5 => "jne", 6 => "jbe", 7 => "ja",
        8 => "js", 9 => "jns", 10 => "jp", 11 => "jnp",
        12 => "jl", 13 => "jge", 14 => "jle", 15 => "jg",
        _ => "jcc",
    };

    private static bool X87(PeImage pe, byte[] d, ref int ip, int escape, out string text)
    {
        text = "";
        if (ip >= d.Length)
            return false;
        var modrm = d[ip];
        var mod = modrm >> 6;
        var reg = (modrm >> 3) & 7;
        var rm = modrm & 7;
        if (mod == 3)
        {
            ip++;
            text = X87Reg(escape, reg, rm, modrm);
            return true;
        }

        ip++;
        if (!TryMem(pe, d, ref ip, modrm, out var mem))
            return false;
        text = $"{X87Mem(escape, reg)} {mem}";
        return true;
    }

    private static string X87Mem(int escape, int reg) => (escape, reg) switch
    {
        (0xD8, 0) => "fadd", (0xD8, 1) => "fmul", (0xD8, 2) => "fcom", (0xD8, 3) => "fcomp",
        (0xD8, 4) => "fsub", (0xD8, 5) => "fsubr", (0xD8, 6) => "fdiv", (0xD8, 7) => "fdivr",
        (0xD9, 0) => "fld", (0xD9, 2) => "fst", (0xD9, 3) => "fstp",
        (0xD9, 4) => "fldenv", (0xD9, 5) => "fldcw", (0xD9, 6) => "fnstenv", (0xD9, 7) => "fnstcw",
        (0xDA, 0) => "fiadd", (0xDA, 1) => "fimul", (0xDA, 2) => "ficom", (0xDA, 3) => "ficomp",
        (0xDA, 4) => "fisub", (0xDA, 5) => "fisubr", (0xDA, 6) => "fidiv", (0xDA, 7) => "fidivr",
        (0xDB, 0) => "fild", (0xDB, 2) => "fist", (0xDB, 3) => "fistp",
        (0xDB, 5) => "fld", (0xDB, 7) => "fstp",
        (0xDC, 0) => "fadd", (0xDC, 1) => "fmul", (0xDC, 2) => "fcom", (0xDC, 3) => "fcomp",
        (0xDC, 4) => "fsub", (0xDC, 5) => "fsubr", (0xDC, 6) => "fdiv", (0xDC, 7) => "fdivr",
        (0xDD, 0) => "fld", (0xDD, 2) => "fst", (0xDD, 3) => "fstp",
        (0xDD, 4) => "frstor", (0xDD, 6) => "fnsave", (0xDD, 7) => "fnstsw",
        (0xDE, 0) => "fiadd", (0xDE, 1) => "fimul", (0xDE, 2) => "ficom", (0xDE, 3) => "ficomp",
        (0xDE, 4) => "fisub", (0xDE, 5) => "fisubr", (0xDE, 6) => "fidiv", (0xDE, 7) => "fidivr",
        (0xDF, 0) => "fild", (0xDF, 2) => "fist", (0xDF, 3) => "fistp",
        (0xDF, 4) => "fbld", (0xDF, 5) => "fild", (0xDF, 6) => "fbstp", (0xDF, 7) => "fistp",
        _ => $"x87_{escape:X2}/{reg}",
    };

    private static string X87Reg(int escape, int reg, int rm, byte modrm) => (escape, modrm) switch
    {
        (0xD9, 0xE0) => "fchs",
        (0xD9, 0xE1) => "fabs",
        (0xD9, 0xE4) => "ftst",
        (0xD9, 0xE5) => "fxam",
        (0xD9, 0xE8) => "fld1",
        (0xD9, 0xE9) => "fldl2t",
        (0xD9, 0xEA) => "fldl2e",
        (0xD9, 0xEB) => "fldpi",
        (0xD9, 0xEC) => "fldlg2",
        (0xD9, 0xED) => "fldln2",
        (0xD9, 0xEE) => "fldz",
        (0xD9, 0xF0) => "f2xm1",
        (0xD9, 0xF1) => "fyl2x",
        (0xD9, 0xF2) => "fptan",
        (0xD9, 0xF3) => "fpatan",
        (0xD9, 0xF4) => "fxtract",
        (0xD9, 0xF5) => "fprem1",
        (0xD9, 0xF6) => "fdecstp",
        (0xD9, 0xF7) => "fincstp",
        (0xD9, 0xF8) => "fprem",
        (0xD9, 0xF9) => "fyl2xp1",
        (0xD9, 0xFA) => "fsqrt",
        (0xD9, 0xFB) => "fsincos",
        (0xD9, 0xFC) => "frndint",
        (0xD9, 0xFD) => "fscale",
        (0xD9, 0xFE) => "fsin",
        (0xD9, 0xFF) => "fcos",
        (0xDA, 0xE9) => "fucompp",
        (0xDB, 0xE2) => "fnclex",
        (0xDB, 0xE3) => "fninit",
        (0xDE, 0xD9) => "fcompp",
        (0xDF, 0xE0) => "fnstsw ax",
        _ when escape == 0xD8 && reg == 0 => $"fadd st, st({rm})",
        _ when escape == 0xD8 && reg == 1 => $"fmul st, st({rm})",
        _ when escape == 0xD9 && reg == 0 => $"fld st({rm})",
        _ when escape == 0xD9 && reg == 1 => $"fxch st({rm})",
        _ when escape == 0xDD && reg == 0 => $"ffree st({rm})",
        _ when escape == 0xDD && reg == 2 => $"fst st({rm})",
        _ when escape == 0xDD && reg == 3 => $"fstp st({rm})",
        _ when escape == 0xDE && reg == 0 => $"faddp st({rm}), st",
        _ when escape == 0xDE && reg == 1 => $"fmulp st({rm}), st",
        _ when escape == 0xDE && reg == 4 => $"fsubrp st({rm}), st",
        _ when escape == 0xDE && reg == 5 => $"fsubp st({rm}), st",
        _ when escape == 0xDE && reg == 6 => $"fdivrp st({rm}), st",
        _ when escape == 0xDE && reg == 7 => $"fdivp st({rm}), st",
        _ when escape == 0xDA && reg == 5 => $"fucompp",
        _ => $"x87_{escape:X2}_{modrm:X2}",
    };

    private static bool F6F7(PeImage pe, byte[] d, ref int ip, bool wide, out string text)
    {
        text = "";
        if (ip >= d.Length)
            return false;
        var modrm = d[ip++];
        var reg = (modrm >> 3) & 7;
        if (!TryMem(pe, d, ref ip, modrm, out var mem, r8: !wide && (modrm >> 6) == 3))
            return false;
        switch (reg)
        {
            case 0:
                if (wide)
                {
                    if (ip + 4 > d.Length) return false;
                    var imm = BitConverter.ToUInt32(d, ip);
                    ip += 4;
                    text = $"test {mem}, {Imm(pe, imm)}";
                }
                else
                {
                    if (ip >= d.Length) return false;
                    text = $"test {mem}, 0x{d[ip++]:X2}";
                }
                return true;
            case 2: text = $"not {mem}"; return true;
            case 3: text = $"neg {mem}"; return true;
            case 4: text = $"mul {mem}"; return true;
            case 5: text = $"imul {mem}"; return true;
            case 6: text = $"div {mem}"; return true;
            case 7: text = $"idiv {mem}"; return true;
            default:
                text = $"grp3/{reg} {mem}";
                return true;
        }
    }

    private static bool Shift(PeImage pe, byte[] d, ref int ip, string count, bool r8, out string text)
    {
        text = "";
        if (ip >= d.Length)
            return false;
        var modrm = d[ip++];
        var name = ((modrm >> 3) & 7) switch
        {
            0 => "rol", 1 => "ror", 2 => "rcl", 3 => "rcr",
            4 => "shl", 5 => "shr", 7 => "sar", var r => $"shift{r}",
        };
        if (!TryMem(pe, d, ref ip, modrm, out var mem, r8: r8 && (modrm >> 6) == 3))
            return false;
        if (count == "imm8")
        {
            if (ip >= d.Length) return false;
            text = $"{name} {mem}, {d[ip++]}";
        }
        else
            text = $"{name} {mem}, {count}";
        return true;
    }

    private static bool AluImm(PeImage pe, byte[] d, ref int ip, int immBytes, bool r8, out string text, bool signed8 = false)
    {
        text = "";
        if (ip >= d.Length)
            return false;
        var modrm = d[ip++];
        var name = ((modrm >> 3) & 7) switch
        {
            0 => "add", 1 => "or", 2 => "adc", 3 => "sbb",
            4 => "and", 5 => "sub", 6 => "xor", 7 => "cmp",
            _ => "alu",
        };
        if (!TryMem(pe, d, ref ip, modrm, out var mem, r8: r8 && (modrm >> 6) == 3))
            return false;
        if (immBytes == 4)
        {
            if (ip + 4 > d.Length) return false;
            var imm = BitConverter.ToUInt32(d, ip);
            ip += 4;
            text = $"{name} {mem}, {Imm(pe, imm)}";
        }
        else if (immBytes == 2)
        {
            if (ip + 2 > d.Length) return false;
            text = $"{name} {mem}, 0x{BitConverter.ToUInt16(d, ip):X}";
            ip += 2;
        }
        else
        {
            if (ip >= d.Length) return false;
            var imm = (sbyte)d[ip++];
            text = signed8 || !r8
                ? $"{name} {mem}, {imm}"
                : $"{name} {mem}, 0x{(byte)imm:X2}";
        }

        return true;
    }

    private static bool Rel8(PeImage pe, ref int ip, string name, out string text)
    {
        text = "";
        if (ip >= pe.Data.Length) return false;
        var rel = (sbyte)pe.Data[ip++];
        text = $"{name} {pe.Va(ip + rel):X8}";
        return true;
    }

    private static bool Rel32(PeImage pe, ref int ip, string name, out string text)
    {
        text = "";
        if (ip + 4 > pe.Data.Length) return false;
        var rel = BitConverter.ToInt32(pe.Data, ip);
        ip += 4;
        text = $"{name} {pe.Va(ip + rel):X8}";
        return true;
    }

    private static bool AlImm8(byte[] d, ref int ip, string name, out string text)
    {
        text = "";
        if (ip >= d.Length) return false;
        text = $"{name}, 0x{d[ip++]:X2}";
        return true;
    }

    private static bool AlImm32(PeImage pe, byte[] d, ref int ip, string name, out string text, bool opsize16)
    {
        text = "";
        if (opsize16)
        {
            if (ip + 2 > d.Length) return false;
            text = $"{name}, 0x{BitConverter.ToUInt16(d, ip):X}";
            ip += 2;
            return true;
        }

        if (ip + 4 > d.Length) return false;
        text = $"{name}, {Imm(pe, BitConverter.ToUInt32(d, ip))}";
        ip += 4;
        return true;
    }

    private static bool PushImm(PeImage pe, byte[] d, ref int ip, bool opsize16, out string text)
    {
        text = "";
        if (opsize16)
        {
            if (ip + 2 > d.Length) return false;
            text = $"push 0x{BitConverter.ToUInt16(d, ip):X}";
            ip += 2;
            return true;
        }

        if (ip + 4 > d.Length) return false;
        text = "push " + Imm(pe, BitConverter.ToUInt32(d, ip));
        ip += 4;
        return true;
    }

    private static bool MovRegImm(PeImage pe, byte[] d, ref int ip, int reg, bool opsize16, out string text)
    {
        text = "";
        if (opsize16)
        {
            if (ip + 2 > d.Length) return false;
            text = $"mov {Reg(reg)}, 0x{BitConverter.ToUInt16(d, ip):X}";
            ip += 2;
            return true;
        }

        if (ip + 4 > d.Length) return false;
        text = $"mov {Reg(reg)}, {Imm(pe, BitConverter.ToUInt32(d, ip))}";
        ip += 4;
        return true;
    }

    private static bool Moffs(PeImage pe, byte[] d, ref int ip, string dest, bool store, out string text, string? src = null)
    {
        text = "";
        if (ip + 4 > d.Length) return false;
        var addr = Imm(pe, BitConverter.ToUInt32(d, ip));
        ip += 4;
        text = store ? $"{dest} [{addr}], {src}" : $"{dest}, [{addr}]";
        return true;
    }

    private static bool Imul3(PeImage pe, byte[] d, ref int ip, bool imm8, bool opsize16, out string text)
    {
        text = "";
        if (ip >= d.Length) return false;
        var modrm = d[ip++];
        var reg = Reg((modrm >> 3) & 7);
        if (!TryMem(pe, d, ref ip, modrm, out var mem))
            return false;
        if (imm8)
        {
            if (ip >= d.Length) return false;
            text = $"imul {reg}, {mem}, {(sbyte)d[ip++]}";
            return true;
        }

        if (opsize16)
        {
            if (ip + 2 > d.Length) return false;
            text = $"imul {reg}, {mem}, 0x{BitConverter.ToUInt16(d, ip):X}";
            ip += 2;
            return true;
        }

        if (ip + 4 > d.Length) return false;
        text = $"imul {reg}, {mem}, {Imm(pe, BitConverter.ToUInt32(d, ip))}";
        ip += 4;
        return true;
    }

    private static bool ModRm(PeImage pe, byte[] d, ref int ip, string name, bool rmFirst, out string text, bool r8 = false)
    {
        text = "";
        if (ip >= d.Length) return false;
        var modrm = d[ip++];
        var reg = r8 ? Reg8((modrm >> 3) & 7) : Reg((modrm >> 3) & 7);
        if (!TryMem(pe, d, ref ip, modrm, out var mem, r8: r8 && (modrm >> 6) == 3))
            return false;
        text = rmFirst ? $"{name} {mem}, {reg}" : $"{name} {reg}, {mem}";
        return true;
    }

    private static bool MovImm(PeImage pe, byte[] d, ref int ip, bool imm32, out string text)
    {
        text = "";
        if (ip >= d.Length) return false;
        var modrm = d[ip++];
        if (!TryMem(pe, d, ref ip, modrm, out var mem))
            return false;
        if (imm32)
        {
            if (ip + 4 > d.Length) return false;
            var imm = BitConverter.ToUInt32(d, ip);
            ip += 4;
            text = $"mov {mem}, {Imm(pe, imm)}";
        }
        else
        {
            if (ip >= d.Length) return false;
            text = $"mov {mem}, 0x{d[ip++]:X2}";
        }

        return true;
    }

    private static bool Lea(PeImage pe, byte[] d, ref int ip, out string text)
    {
        text = "";
        if (ip >= d.Length) return false;
        var modrm = d[ip++];
        var reg = Reg((modrm >> 3) & 7);
        if (!TryMem(pe, d, ref ip, modrm, out var mem))
            return false;
        text = $"lea {reg}, {mem}";
        return true;
    }

    private static bool Unary(PeImage pe, byte[] d, ref int ip, string name, out string text)
    {
        text = "";
        if (ip >= d.Length) return false;
        var modrm = d[ip++];
        if (!TryMem(pe, d, ref ip, modrm, out var mem))
            return false;
        text = $"{name} {mem}";
        return true;
    }

    private static bool IncDec(PeImage pe, byte[] d, ref int ip, out string text)
    {
        text = "";
        if (ip >= d.Length) return false;
        var modrm = d[ip++];
        var name = ((modrm >> 3) & 7) switch { 0 => "inc", 1 => "dec", var r => $"fe/{r}" };
        if (!TryMem(pe, d, ref ip, modrm, out var mem, r8: (modrm >> 6) == 3))
            return false;
        text = $"{name} {mem}";
        return true;
    }

    private static bool Ff(byte[] d, PeImage pe, ref int ip, out string text)
    {
        text = "";
        if (ip >= d.Length) return false;
        var modrm = d[ip++];
        var name = ((modrm >> 3) & 7) switch
        {
            0 => "inc", 1 => "dec", 2 => "call", 3 => "call",
            4 => "jmp", 5 => "jmp", 6 => "push", var r => $"ff/{r}",
        };
        if (!TryMem(pe, d, ref ip, modrm, out var mem))
            return false;
        text = $"{name} {mem}";
        return true;
    }

    private static bool TryMem(PeImage pe, byte[] d, ref int ip, byte modrm, out string text, bool r8 = false)
    {
        text = "";
        var mod = modrm >> 6;
        var rm = modrm & 7;
        if (mod == 3)
        {
            text = r8 ? Reg8(rm) : Reg(rm);
            return true;
        }

        string addr;
        if (rm == 4)
        {
            if (ip >= d.Length) return false;
            var sib = d[ip++];
            var scale = 1 << (sib >> 6);
            var index = (sib >> 3) & 7;
            var bas = sib & 7;
            addr = index == 4 ? Reg(bas) : $"{Reg(bas)}+{Reg(index)}*{scale}";
            if (mod == 0 && bas == 5)
            {
                if (ip + 4 > d.Length) return false;
                var disp = BitConverter.ToUInt32(d, ip);
                ip += 4;
                addr = index == 4 ? Imm(pe, disp) : $"{Imm(pe, disp)}+{Reg(index)}*{scale}";
            }
        }
        else if (mod == 0 && rm == 5)
        {
            if (ip + 4 > d.Length) return false;
            addr = Imm(pe, BitConverter.ToUInt32(d, ip));
            ip += 4;
        }
        else
        {
            addr = Reg(rm);
        }

        if (mod == 1)
        {
            if (ip >= d.Length) return false;
            var disp = (sbyte)d[ip++];
            addr += disp < 0 ? disp.ToString() : "+" + disp;
        }
        else if (mod == 2)
        {
            if (ip + 4 > d.Length) return false;
            var disp = BitConverter.ToInt32(d, ip);
            ip += 4;
            addr += disp < 0 ? disp.ToString() : "+" + disp;
        }

        text = $"[{addr}]";
        return true;
    }

    private static string Reg(int i) => i switch
    {
        0 => "eax", 1 => "ecx", 2 => "edx", 3 => "ebx",
        4 => "esp", 5 => "ebp", 6 => "esi", 7 => "edi",
        _ => "r?",
    };

    private static string Reg8(int i) => i switch
    {
        0 => "al", 1 => "cl", 2 => "dl", 3 => "bl",
        4 => "ah", 5 => "ch", 6 => "dh", 7 => "bh",
        _ => "r8?",
    };

    private static string Imm(PeImage pe, uint value)
    {
        if (value is 0x31545844) return "DXT1";
        if (value is 0x33545844) return "DXT3";
        if (value is 0x35545844) return "DXT5";
        var file = pe.FileOffset(value);
        if (file >= 0 && file < pe.Data.Length && pe.Data[file] is >= 32 and <= 126)
        {
            var end = file;
            while (end < pe.Data.Length && end - file < 48 && pe.Data[end] is >= 32 and <= 126)
                end++;
            if (end - file >= 4)
                return $"\"{System.Text.Encoding.ASCII.GetString(pe.Data, file, end - file)}\"";
        }

        return $"0x{value:X}";
    }
}
