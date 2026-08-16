namespace Fable.ExeIndex;

/// <summary>
/// Tiny 32-bit x86 decoder. Enough to turn Fable.exe helpers into searchable
/// lines like <c>push "DXT5"</c> / <c>call 0x5BE870</c>, not a full decompiler.
/// </summary>
internal static class X86
{
    public static List<string> Disassemble(PeImage pe, int fileOffset, int maxInsns = 64)
    {
        var data = pe.Data;
        var lines = new List<string>();
        var ip = fileOffset;
        for (var n = 0; n < maxInsns && ip + 1 < data.Length; n++)
        {
            var start = ip;
            if (!TryDecode(pe, ref ip, out var text))
            {
                lines.Add($"  //{pe.Va(start):X8}: db 0x{data[start]:X2}");
                ip = start + 1;
                continue;
            }

            lines.Add($"  //{pe.Va(start):X8}: {text}");
            if (text is "ret" or "retn" || text.StartsWith("ret ", StringComparison.Ordinal))
                break;
        }

        return lines;
    }

    public static int FindPrologue(PeImage pe, int from, int maxBack = 2048)
    {
        var data = pe.Data;
        var lo = Math.Max(0, from - maxBack);
        for (var i = from; i >= lo + 2; i--)
        {
            // push ebp / mov ebp, esp
            if (data[i] == 0x55 && data[i + 1] == 0x8B && data[i + 2] == 0xEC)
                return i;
        }

        // int3 padding then the next live byte
        for (var i = from; i > lo + 1; i--)
        {
            if (data[i - 1] == 0xCC && data[i] != 0xCC && pe.InCode(i))
                return i;
        }

        return FindImmInsn(pe, from);
    }

    /// <summary>
    /// Xref sites point at a 4-byte immediate. Prefer the <c>push imm32</c> /
    /// <c>mov r32, imm32</c> that owns it so we do not disassemble mid-immediate.
    /// </summary>
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
        var d = pe.Data;
        text = "";
        if (ip >= d.Length)
            return false;
        var op = d[ip++];
        switch (op)
        {
            case 0x50: case 0x51: case 0x52: case 0x53:
            case 0x54: case 0x55: case 0x56: case 0x57:
                text = "push " + Reg(op - 0x50);
                return true;
            case 0x58: case 0x59: case 0x5A: case 0x5B:
            case 0x5C: case 0x5D: case 0x5E: case 0x5F:
                text = "pop " + Reg(op - 0x58);
                return true;
            case 0x40: case 0x41: case 0x42: case 0x43:
            case 0x44: case 0x45: case 0x46: case 0x47:
                text = "inc " + Reg(op - 0x40);
                return true;
            case 0x48: case 0x49: case 0x4A: case 0x4B:
            case 0x4C: case 0x4D: case 0x4E: case 0x4F:
                text = "dec " + Reg(op - 0x48);
                return true;
            case 0x60: text = "pushad"; return true;
            case 0x61: text = "popad"; return true;
            case 0x90: text = "nop"; return true;
            case 0x68:
                if (ip + 4 > d.Length) return false;
                var imm32 = BitConverter.ToUInt32(d, ip);
                ip += 4;
                text = "push " + Imm(pe, imm32);
                return true;
            case 0x6A:
                if (ip >= d.Length) return false;
                text = $"push { (sbyte)d[ip++] }";
                return true;
            case 0x74:
                return Rel8(pe, ref ip, "je", out text);
            case 0x75:
                return Rel8(pe, ref ip, "jne", out text);
            case 0x7C:
                return Rel8(pe, ref ip, "jl", out text);
            case 0x7E:
                return Rel8(pe, ref ip, "jle", out text);
            case 0x7F:
                return Rel8(pe, ref ip, "jg", out text);
            case 0x01:
                return ModRm(pe, d, ref ip, "add", rmFirst: true, out text);
            case 0x03:
                return ModRm(pe, d, ref ip, "add", rmFirst: false, out text);
            case 0x29:
                return ModRm(pe, d, ref ip, "sub", rmFirst: true, out text);
            case 0x2B:
                return ModRm(pe, d, ref ip, "sub", rmFirst: false, out text);
            case 0x31:
                return ModRm(pe, d, ref ip, "xor", rmFirst: true, out text);
            case 0x33:
                return ModRm(pe, d, ref ip, "xor", rmFirst: false, out text);
            case 0x39:
                return ModRm(pe, d, ref ip, "cmp", rmFirst: true, out text);
            case 0x3B:
                return ModRm(pe, d, ref ip, "cmp", rmFirst: false, out text);
            case 0x81:
                return AluImm32(pe, d, ref ip, out text);
            case 0x83:
                return AluImm8(pe, d, ref ip, out text);
            case 0x84: case 0x85:
                return ModRm(pe, d, ref ip, "test", rmFirst: true, out text);
            case 0x88: case 0x89:
                return ModRm(pe, d, ref ip, "mov", rmFirst: true, out text);
            case 0x8A: case 0x8B:
                return ModRm(pe, d, ref ip, "mov", rmFirst: false, out text);
            case 0x8D:
                return Lea(pe, d, ref ip, out text);
            case 0xA1:
                if (ip + 4 > d.Length) return false;
                text = $"mov eax, [{Imm(pe, BitConverter.ToUInt32(d, ip))}]";
                ip += 4;
                return true;
            case 0xB8: case 0xB9: case 0xBA: case 0xBB:
            case 0xBC: case 0xBD: case 0xBE: case 0xBF:
                if (ip + 4 > d.Length) return false;
                text = $"mov {Reg(op - 0xB8)}, {Imm(pe, BitConverter.ToUInt32(d, ip))}";
                ip += 4;
                return true;
            case 0xC6:
                return MovImm(pe, d, ref ip, imm32: false, out text);
            case 0xC7:
                return MovImm(pe, d, ref ip, imm32: true, out text);
            case 0xC2:
                if (ip + 2 > d.Length) return false;
                text = $"ret {BitConverter.ToUInt16(d, ip)}";
                ip += 2;
                return true;
            case 0xC3:
                text = "ret";
                return true;
            case 0xC9:
                text = "leave";
                return true;
            case 0xCC:
                text = "int3";
                return true;
            case 0xE8:
                return Rel32(pe, ref ip, "call", out text);
            case 0xE9:
                return Rel32(pe, ref ip, "jmp", out text);
            case 0xEB:
                return Rel8(pe, ref ip, "jmp", out text);
            case 0xFF:
                return Ff(d, pe, ref ip, out text);
            case 0x0F:
                if (ip >= d.Length) return false;
                var op2 = d[ip++];
                if (op2 is 0x84 or 0x85)
                    return Rel32(pe, ref ip, op2 == 0x84 ? "je" : "jne", out text);
                text = $"op0F_{op2:X2}";
                return true;
            default:
                return false;
        }
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

    private static bool AluImm8(PeImage pe, byte[] d, ref int ip, out string text)
    {
        text = "";
        if (ip >= d.Length) return false;
        var modrm = d[ip++];
        var name = ((modrm >> 3) & 7) switch { 0 => "add", 4 => "and", 5 => "sub", 7 => "cmp", var r => $"alu{r}" };
        if (!TryMem(pe, d, ref ip, modrm, out var mem))
            return false;
        if (ip >= d.Length) return false;
        text = $"{name} {mem}, {(sbyte)d[ip++]}";
        return true;
    }

    private static bool AluImm32(PeImage pe, byte[] d, ref int ip, out string text)
    {
        text = "";
        if (ip >= d.Length) return false;
        var modrm = d[ip++];
        var name = ((modrm >> 3) & 7) switch { 0 => "add", 4 => "and", 5 => "sub", 7 => "cmp", var r => $"alu{r}" };
        if (!TryMem(pe, d, ref ip, modrm, out var mem))
            return false;
        if (ip + 4 > d.Length) return false;
        var imm = BitConverter.ToUInt32(d, ip);
        ip += 4;
        text = $"{name} {mem}, {Imm(pe, imm)}";
        return true;
    }

    private static bool ModRm(PeImage pe, byte[] d, ref int ip, string name, bool rmFirst, out string text)
    {
        text = "";
        if (ip >= d.Length) return false;
        var modrm = d[ip++];
        var reg = Reg((modrm >> 3) & 7);
        if (!TryMem(pe, d, ref ip, modrm, out var mem))
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

    private static bool Ff(byte[] d, PeImage pe, ref int ip, out string text)
    {
        text = "";
        if (ip >= d.Length) return false;
        var modrm = d[ip++];
        var name = ((modrm >> 3) & 7) switch { 2 => "call", 4 => "jmp", 6 => "push", var r => $"ff/{r}" };
        if (!TryMem(pe, d, ref ip, modrm, out var mem))
            return false;
        text = $"{name} {mem}";
        return true;
    }

    private static bool TryMem(PeImage pe, byte[] d, ref int ip, byte modrm, out string text)
    {
        text = "";
        var mod = modrm >> 6;
        var rm = modrm & 7;
        if (mod == 3)
        {
            text = Reg(rm);
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
