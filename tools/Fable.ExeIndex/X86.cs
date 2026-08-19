namespace Fable.ExeIndex;

/// <summary>
/// 32-bit x86 decoder for Fable.exe. Length must be correct: a missed
/// <c>0F B7</c> / <c>F6</c> / x87 used to emit <c>db</c> then a fake <c>ret</c>.
/// Function starts are frame prologues or INT3-padded thiscall
/// (<c>56 8B F1</c>), not every mid-body <c>push esi; mov esi, ecx</c>.
/// SSE packed ops use <c>xmm0–7</c>, not GPRs. MSVC
/// <c>jmp [disp32+reg*4]</c> tables dump as <c>dd</c>, not fake code.
/// Do not decode <c>0x8E</c> (that hides a mid-instruction dump).
/// </summary>
internal static class X86
{
    public readonly record struct Step(uint Va, string Text, bool IsRet, uint? DirectCall, string Bytes = "");

    public static List<string> Disassemble(PeImage pe, int fileOffset, int maxInsns = 64) =>
        DisassembleCore(pe, fileOffset, maxInsns, stopOnRet: true);

    /// <summary>
    /// Same as <see cref="Disassemble"/> but does not stop at the first
    /// <c>ret</c>. Needed for functions with an early-out (Diffuse2X
    /// <c>0098B5E0</c> returns immediately on arg == -1).
    /// </summary>
    public static List<string> DisassembleAll(PeImage pe, int fileOffset, int maxInsns = 64) =>
        DisassembleCore(pe, fileOffset, maxInsns, stopOnRet: false);

    /// <summary>
    /// Walk one function: keep going past an early <c>ret</c>, but stop at
    /// INT3 padding or the next function start (frame or INT3-padded
    /// thiscall). Mid-body <c>push esi; mov esi, ecx</c> is not a start.
    /// </summary>
    public static List<Step> WalkFunction(PeImage pe, int fileOffset, int maxInsns)
    {
        var steps = Walk(pe, fileOffset, maxInsns, stopOnRet: false);
        var d = pe.Data;
        var keep = steps.Count;
        for (var i = 1; i < steps.Count; i++)
        {
            var file = pe.FileOffset(steps[i].Va);
            if (file < 0 || file + 2 >= d.Length)
                continue;
            if (d[file] == 0xCC)
            {
                keep = i;
                break;
            }

            if (IsFunctionStart(d, file))
            {
                keep = i;
                break;
            }
        }

        if (keep < steps.Count)
            steps.RemoveRange(keep, steps.Count - keep);
        return steps;
    }

    public static List<Step> Walk(PeImage pe, int fileOffset, int maxInsns, bool stopOnRet)
    {
        var steps = new List<Step>(maxInsns);
        var n = 0;
        WalkRange(pe, fileOffset, pe.Data.Length, step =>
        {
            steps.Add(step);
            n++;
            return n < maxInsns && !(stopOnRet && step.IsRet);
        });
        return steps;
    }

    /// <summary>
    /// Linear decode from <paramref name="fileOffset"/> until
    /// <paramref name="endFile"/>. <paramref name="emit"/> returns false to stop.
    /// </summary>
    public static void WalkRange(PeImage pe, int fileOffset, int endFile, Func<Step, bool> emit)
    {
        var map = GetSwitchMap(pe);
        map.PtrEntries.Clear();
        map.IndexEntries.Clear();
        var ip = fileOffset;
        var d = pe.Data;
        var limit = Math.Min(endFile, d.Length);
        while (ip < limit)
        {
            var va = pe.Va(ip);
            if (map.PointerTables.Contains(va))
            {
                var next = EmitPointerTable(pe, map, ip, limit, emit);
                if (next > ip)
                {
                    ip = next;
                    continue;
                }
            }
            else if (map.WordTables.Contains(va))
            {
                var next = EmitIndexTable(pe, map, ip, limit, elemSize: 2, emit);
                if (next > ip)
                {
                    ip = next;
                    continue;
                }
            }
            else if (map.ByteTables.Contains(va))
            {
                var next = EmitIndexTable(pe, map, ip, limit, elemSize: 1, emit);
                if (next > ip)
                {
                    ip = next;
                    continue;
                }
            }

            var start = ip;
            var look = ip;
            while (look < d.Length && d[look] is 0x66 or 0x67 or 0xF2 or 0xF3 or 0xF0 or 0x64 or 0x65 or 0x26 or 0x2E or 0x36 or 0x3E)
                look++;
            uint? call = null;
            if (look + 5 <= d.Length && d[look] == 0xE8)
            {
                var rel = BitConverter.ToInt32(d, look + 1);
                call = pe.Va(look + 5 + rel);
            }

            Step step;
            if (!TryDecode(pe, ref ip, out var text))
            {
                ip = start + 1;
                step = new Step(pe.Va(start), $"db 0x{d[start]:X2}", false, null, HexBytes(d, start, 1));
            }
            else
            {
                var ret = text is "ret" or "retn" || text.StartsWith("ret ", StringComparison.Ordinal);
                step = new Step(pe.Va(start), text, ret, call, HexBytes(d, start, ip - start));
            }

            if (!emit(step))
                return;
        }
    }

    internal static SwitchMap GetSwitchMap(PeImage pe)
    {
        if (pe.SwitchMap is { } cached)
            return cached;
        var map = new SwitchMap();
        var d = pe.Data;
        foreach (var sec in pe.Sections)
        {
            if (!pe.InCode((int)sec.FileOffset))
                continue;
            var start = (int)sec.FileOffset;
            var end = Math.Min(d.Length, (int)(sec.FileOffset + sec.FileSize)) - 6;
            for (var i = start; i < end; i++)
            {
                if (d[i] == 0xFF && TrySwitchTableVa(pe, i, out var ptrTable))
                {
                    if (map.PointerTables.Add(ptrTable))
                        map.Hits.Add(("jmp4", pe.Va(i), ptrTable));
                }
            }

            for (var i = start; i < end; i++)
            {
                if (d[i] != 0x0F)
                    continue;
                if (!TryIndexTableVa(pe, i, out var table, out var elemSize))
                    continue;
                if (!NearPointerTable(map, table))
                    continue;
                if (elemSize == 2)
                {
                    if (map.WordTables.Add(table))
                        map.Hits.Add(("movzx16", pe.Va(i), table));
                }
                else if (map.ByteTables.Add(table))
                    map.Hits.Add(("movzx8", pe.Va(i), table));
            }
        }

        pe.SwitchMap = map;
        return map;
    }

    /// <summary>
    /// MSVC <c>FF 24 /s ib</c> <c>jmp [disp32+index*4]</c> with no base.
    /// Table sits after the function <c>ret</c>.
    /// </summary>
    private static bool TrySwitchTableVa(PeImage pe, int i, out uint table)
    {
        table = 0;
        var d = pe.Data;
        if (i + 7 > d.Length || d[i] != 0xFF)
            return false;
        var modrm = d[i + 1];
        if ((modrm >> 6) != 0 || ((modrm >> 3) & 7) != 4 || (modrm & 7) != 4)
            return false;
        var sib = d[i + 2];
        if ((sib >> 6) != 2 || ((sib >> 3) & 7) == 4 || (sib & 7) != 5)
            return false;
        table = BitConverter.ToUInt32(d, i + 3);
        if ((table & 3) != 0)
            return false;
        var file = pe.FileOffset(table);
        if (file < 0 || !pe.InCode(file))
            return false;
        var jmpVa = pe.Va(i);
        return table > jmpVa && table - jmpVa < 0x10000;
    }

    private static bool NearPointerTable(SwitchMap map, uint table)
    {
        foreach (var ptr in map.PointerTables)
        {
            var d = table > ptr ? table - ptr : ptr - table;
            if (d < 512)
                return true;
        }

        return false;
    }

    private static bool TryIndexTableVa(PeImage pe, int i, out uint table, out int elemSize)
    {
        table = 0;
        elemSize = 1;
        var d = pe.Data;
        if (i + 3 >= d.Length || d[i] != 0x0F)
            return false;
        elemSize = d[i + 1] switch { 0xB7 => 2, 0xB6 or 0xBE => 1, _ => 0 };
        if (elemSize == 0)
            return false;
        var ip = i + 2;
        if (!TryReadDisp32Mem(d, ref ip, out table) || table == 0)
            return false;
        var file = pe.FileOffset(table);
        if (file < 0 || !pe.InCode(file))
            return false;
        var site = pe.Va(i);
        return table > site && table - site < 0x10000;
    }

    private static bool TryReadDisp32Mem(byte[] d, ref int ip, out uint disp)
    {
        disp = 0;
        if (ip >= d.Length)
            return false;
        var modrm = d[ip++];
        var mod = modrm >> 6;
        var rm = modrm & 7;
        if (mod == 3)
            return false;
        if (rm == 4)
        {
            if (ip >= d.Length)
                return false;
            var sib = d[ip++];
            var scale = sib >> 6;
            var index = (sib >> 3) & 7;
            var bas = sib & 7;
            if (mod != 0 || bas != 5)
                return false;
            if (index != 4 && scale != 0)
                return false;
            if (ip + 4 > d.Length)
                return false;
            disp = BitConverter.ToUInt32(d, ip);
            ip += 4;
            return true;
        }

        if (mod == 0 && rm == 5 || mod == 2)
        {
            if (ip + 4 > d.Length)
                return false;
            disp = BitConverter.ToUInt32(d, ip);
            ip += 4;
            return true;
        }

        return false;
    }

    private static int EmitPointerTable(PeImage pe, SwitchMap map, int ip, int limit, Func<Step, bool> emit)
    {
        var d = pe.Data;
        var table = pe.Va(ip);
        var n = 0;
        while (ip + 4 <= limit)
        {
            var va = pe.Va(ip);
            if (n > 0 && (map.ByteTables.Contains(va) || map.WordTables.Contains(va)))
                break;
            var ptr = BitConverter.ToUInt32(d, ip);
            var file = pe.FileOffset(ptr);
            if (file < 0 || !pe.InCode(file))
                break;
            map.PtrEntries.Add((table, n, ptr));
            if (!emit(new Step(va, $"dd 0x{ptr:X8}", false, null, HexBytes(d, ip, 4))))
                return ip + 4;
            ip += 4;
            n++;
            if (n >= 512)
                break;
        }

        return ip;
    }

    private static int EmitIndexTable(PeImage pe, SwitchMap map, int ip, int limit, int elemSize, Func<Step, bool> emit)
    {
        var d = pe.Data;
        var table = pe.Va(ip);
        var n = 0;
        var max = elemSize == 2 ? 256 : 256;
        while (ip + elemSize <= limit && n < max)
        {
            var va = pe.Va(ip);
            if (d[ip] == 0xCC)
                break;
            if (n > 0 && IsFunctionStart(d, ip))
                break;
            if (n > 0 && map.PointerTables.Contains(va))
                break;
            if (elemSize == 2 && n > 0 && map.ByteTables.Contains(va))
                break;
            if (elemSize == 2)
            {
                var w = BitConverter.ToUInt16(d, ip);
                map.IndexEntries.Add((table, n, w));
                if (!emit(new Step(va, $"dw 0x{w:X4}", false, null, HexBytes(d, ip, 2))))
                    return ip + 2;
                ip += 2;
            }
            else
            {
                var b = d[ip];
                map.IndexEntries.Add((table, n, b));
                if (!emit(new Step(va, $"db 0x{b:X2}", false, null, HexBytes(d, ip, 1))))
                    return ip + 1;
                ip++;
            }

            n++;
        }

        return ip;
    }

    internal static string HexBytes(byte[] d, int start, int len)
    {
        if (len <= 0 || start < 0 || start + len > d.Length)
            return "";
        var sb = new System.Text.StringBuilder(len * 3);
        for (var i = 0; i < len; i++)
        {
            if (i > 0)
                sb.Append(' ');
            sb.Append(d[start + i].ToString("X2"));
        }

        return sb.ToString();
    }

    private enum VecKind { None, Xmm, Mm }

    private static bool IsSseXmm(byte op2) => op2 is
        0x10 or 0x11 or 0x12 or 0x13 or 0x16 or 0x17 or
        0x28 or 0x29 or 0x2E or 0x2F or
        0x51 or 0x52 or 0x53 or 0x54 or 0x55 or 0x56 or 0x57 or
        0x58 or 0x59 or 0x5C or 0x5D or 0x5E or 0x5F or
        0xC2 or 0xC6;

    private static bool IsMmx(byte op2) =>
        op2 is (>= 0x60 and <= 0x76 and not 0x70) or 0x7E or 0x7F
        or (>= 0xD1 and <= 0xD5) or (>= 0xD8 and <= 0xDF)
        or (>= 0xE0 and <= 0xE5) or (>= 0xE8 and <= 0xEF)
        or (>= 0xF1 and <= 0xF8) or (>= 0xFA and <= 0xFE);

    private static string SseMnemonic(byte op2, bool opsize16, string rep)
    {
        var f3 = rep.StartsWith("rep ", StringComparison.Ordinal);
        var f2 = rep.StartsWith("repne ", StringComparison.Ordinal);
        var suf = f2 ? "sd" : f3 ? "ss" : opsize16 ? "pd" : "ps";
        return op2 switch
        {
            0x10 or 0x11 => f3 ? "movss" : f2 ? "movsd" : opsize16 ? "movupd" : "movups",
            0x12 => "movlps",
            0x13 => "movlps",
            0x16 => "movhps",
            0x17 => "movhps",
            0x28 or 0x29 => opsize16 ? "movapd" : "movaps",
            0x2E => opsize16 ? "ucomisd" : "ucomiss",
            0x2F => opsize16 ? "comisd" : "comiss",
            0x51 => "sqrt" + suf,
            0x52 => f3 ? "rsqrtss" : "rsqrtps",
            0x53 => f3 ? "rcpss" : "rcpps",
            0x54 => opsize16 ? "andpd" : "andps",
            0x55 => opsize16 ? "andnpd" : "andnps",
            0x56 => opsize16 ? "orpd" : "orps",
            0x57 => opsize16 ? "xorpd" : "xorps",
            0x58 => "add" + suf,
            0x59 => "mul" + suf,
            0x5C => "sub" + suf,
            0x5D => "min" + suf,
            0x5E => "div" + suf,
            0x5F => "max" + suf,
            0x70 => f3 ? "pshufhw" : f2 ? "pshuflw" : "pshufd",
            0xC2 => "cmp" + suf,
            0xC6 => opsize16 ? "shufpd" : "shufps",
            _ => $"0F_{op2:X2}",
        };
    }

    private static List<string> DisassembleCore(PeImage pe, int fileOffset, int maxInsns, bool stopOnRet)
    {
        var lines = new List<string>();
        foreach (var step in Walk(pe, fileOffset, maxInsns, stopOnRet))
            lines.Add($"  //{step.Va:X8}: {step.Text}");
        return lines;
    }

    /// <summary>
    /// If <paramref name="file"/> sits inside an instruction, return the
    /// start of that instruction. Used by <c>disasm</c> / <c>fn --exact</c>
    /// so a ModRM VA does not dump <c>db</c> then a fake stream.
    /// Does not treat the snap as a function entry.
    /// </summary>
    public static int FindInsnStart(PeImage pe, int file)
    {
        if (file < 0 || file >= pe.Data.Length)
            return file;
        var ip = file;
        if (TryDecode(pe, ref ip, out _) && ip > file)
            return file;
        var lo = Math.Max(0, file - 15);
        for (var start = file - 1; start >= lo; start--)
        {
            ip = start;
            if (!TryDecode(pe, ref ip, out _))
                continue;
            if (start <= file && file < ip)
                return start;
        }

        return file;
    }

    /// <summary>
    /// Standard <c>push ebp; mov ebp, esp</c>, MSVC large-frame
    /// <c>push ebp; lea ebp, [esp+disp]</c>, INT3-padded thiscall, or
    /// the first non-INT3 byte after padding. Stops at INT3 so a
    /// mid-function VA does not walk into the previous function
    /// (PALSKIN bind <c>00BD3070</c> is <c>lea ebp</c>, not
    /// <c>mov ebp, esp</c>).
    /// </summary>
    public static int FindPrologue(PeImage pe, int from, int maxBack = 16384)
    {
        var data = pe.Data;
        var lo = Math.Max(0, from - maxBack);
        for (var i = from; i >= lo + 2; i--)
        {
            if (IsFunctionStart(data, i))
                return i;
            // Two INT3s — a lone 0xCC is often a displacement (PALSKIN
            // `mov edx, [eax+0x3CC]` at 00BD41E4 is not a function start).
            if (i < from && data[i] == 0xCC && data[i - 1] == 0xCC)
            {
                var start = i + 1;
                while (start < data.Length && data[start] == 0xCC)
                    start++;
                if (start <= from && pe.InCode(start))
                    return start;
                break;
            }
        }

        for (var i = from; i > lo + 1; i--)
        {
            if (data[i - 1] == 0xCC && data[i] != 0xCC && pe.InCode(i))
                return i;
        }

        return FindImmInsn(pe, from);
    }

    public static bool IsFramePrologue(byte[] data, int i)
    {
        if (i + 2 >= data.Length || data[i] != 0x55)
            return false;
        // push ebp; mov ebp, esp
        if (data[i + 1] == 0x8B && data[i + 2] == 0xEC)
            return true;
        if (i + 3 >= data.Length || data[i + 1] != 0x8D)
            return false;
        // push ebp; lea ebp, [esp+disp8]
        if (data[i + 2] == 0x6C && data[i + 3] == 0x24)
            return true;
        // push ebp; lea ebp, [esp+disp32]
        return i + 4 < data.Length && data[i + 2] == 0xAC && data[i + 3] == 0x24;
    }

    /// <summary>
    /// MSVC thiscall: keep <c>ecx</c> in esi/ebx/edi. Do not use as a
    /// start unless <see cref="IsFunctionStart"/> saw INT3 padding —
    /// the same bytes appear mid-body.
    /// </summary>
    public static bool IsThiscallPrologue(byte[] data, int i)
    {
        if (i + 2 >= data.Length)
            return false;
        // push esi; mov esi, ecx
        if (data[i] == 0x56 && data[i + 1] == 0x8B && data[i + 2] == 0xF1)
            return true;
        // push ebx; mov ebx, ecx
        if (data[i] == 0x53 && data[i + 1] == 0x8B && data[i + 2] == 0xD9)
            return true;
        // push edi; mov edi, ecx
        if (data[i] == 0x57 && data[i + 1] == 0x8B && data[i + 2] == 0xF9)
            return true;
        // push ebx; push esi; mov esi, ecx
        return i + 3 < data.Length
            && data[i] == 0x53 && data[i + 1] == 0x56
            && data[i + 2] == 0x8B && data[i + 3] == 0xF1;
    }

    /// <summary>
    /// MSVC <c>/hotpatch</c> two-byte nop <c>mov edi, edi</c> then a
    /// real prologue.
    /// </summary>
    public static bool IsHotpatchPrefix(byte[] data, int i) =>
        i + 1 < data.Length && data[i] == 0x8B && data[i + 1] == 0xFF;

    /// <summary>
    /// Frame prologue, or INT3-padded thiscall / hotpatch. A lone
    /// mid-body <c>56 8B F1</c> is not a start.
    /// </summary>
    public static bool IsFunctionStart(byte[] data, int i)
    {
        if (IsFramePrologue(data, i))
            return true;
        if (i <= 0 || data[i - 1] != 0xCC)
            return false;
        if (IsThiscallPrologue(data, i))
            return true;
        if (IsHotpatchPrefix(data, i) && i + 2 < data.Length)
            return IsFramePrologue(data, i + 2) || IsThiscallPrologue(data, i + 2);
        return false;
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
            if (p == 0x67) { ip++; continue; }
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
            case 0x06: text = "push es"; return true;
            case 0x07: text = "pop es"; return true;
            case 0x08: return ModRm(pe, d, ref ip, "or", rmFirst: true, out text, r8: true);
            case 0x09: return ModRm(pe, d, ref ip, "or", rmFirst: true, out text);
            case 0x0A: return ModRm(pe, d, ref ip, "or", rmFirst: false, out text, r8: true);
            case 0x0B: return ModRm(pe, d, ref ip, "or", rmFirst: false, out text);
            case 0x0C: return AlImm8(d, ref ip, "or al", out text);
            case 0x0D: return AlImm32(pe, d, ref ip, "or eax", out text, opsize16);
            case 0x0E: text = "push cs"; return true;
            case 0x10: return ModRm(pe, d, ref ip, "adc", rmFirst: true, out text, r8: true);
            case 0x11: return ModRm(pe, d, ref ip, "adc", rmFirst: true, out text);
            case 0x12: return ModRm(pe, d, ref ip, "adc", rmFirst: false, out text, r8: true);
            case 0x13: return ModRm(pe, d, ref ip, "adc", rmFirst: false, out text);
            case 0x14: return AlImm8(d, ref ip, "adc al", out text);
            case 0x15: return AlImm32(pe, d, ref ip, "adc eax", out text, opsize16);
            case 0x16: text = "push ss"; return true;
            case 0x17: text = "pop ss"; return true;
            case 0x18: return ModRm(pe, d, ref ip, "sbb", rmFirst: true, out text, r8: true);
            case 0x19: return ModRm(pe, d, ref ip, "sbb", rmFirst: true, out text);
            case 0x1A: return ModRm(pe, d, ref ip, "sbb", rmFirst: false, out text, r8: true);
            case 0x1B: return ModRm(pe, d, ref ip, "sbb", rmFirst: false, out text);
            case 0x1C: return AlImm8(d, ref ip, "sbb al", out text);
            case 0x1D: return AlImm32(pe, d, ref ip, "sbb eax", out text, opsize16);
            case 0x1E: text = "push ds"; return true;
            case 0x1F: text = "pop ds"; return true;
            case 0x20: return ModRm(pe, d, ref ip, "and", rmFirst: true, out text, r8: true);
            case 0x21: return ModRm(pe, d, ref ip, "and", rmFirst: true, out text);
            case 0x22: return ModRm(pe, d, ref ip, "and", rmFirst: false, out text, r8: true);
            case 0x23: return ModRm(pe, d, ref ip, "and", rmFirst: false, out text);
            case 0x24: return AlImm8(d, ref ip, "and al", out text);
            case 0x25: return AlImm32(pe, d, ref ip, "and eax", out text, opsize16);
            case 0x27: text = "daa"; return true;
            case 0x28: return ModRm(pe, d, ref ip, "sub", rmFirst: true, out text, r8: true);
            case 0x29: return ModRm(pe, d, ref ip, "sub", rmFirst: true, out text);
            case 0x2A: return ModRm(pe, d, ref ip, "sub", rmFirst: false, out text, r8: true);
            case 0x2B: return ModRm(pe, d, ref ip, "sub", rmFirst: false, out text);
            case 0x2C: return AlImm8(d, ref ip, "sub al", out text);
            case 0x2D: return AlImm32(pe, d, ref ip, "sub eax", out text, opsize16);
            case 0x2F: text = "das"; return true;
            case 0x30: return ModRm(pe, d, ref ip, "xor", rmFirst: true, out text, r8: true);
            case 0x31: return ModRm(pe, d, ref ip, "xor", rmFirst: true, out text);
            case 0x32: return ModRm(pe, d, ref ip, "xor", rmFirst: false, out text, r8: true);
            case 0x33: return ModRm(pe, d, ref ip, "xor", rmFirst: false, out text);
            case 0x34: return AlImm8(d, ref ip, "xor al", out text);
            case 0x35: return AlImm32(pe, d, ref ip, "xor eax", out text, opsize16);
            case 0x37: text = "aaa"; return true;
            case 0x38: return ModRm(pe, d, ref ip, "cmp", rmFirst: true, out text, r8: true);
            case 0x39: return ModRm(pe, d, ref ip, "cmp", rmFirst: true, out text);
            case 0x3A: return ModRm(pe, d, ref ip, "cmp", rmFirst: false, out text, r8: true);
            case 0x3B: return ModRm(pe, d, ref ip, "cmp", rmFirst: false, out text);
            case 0x3C: return AlImm8(d, ref ip, "cmp al", out text);
            case 0x3D: return AlImm32(pe, d, ref ip, "cmp eax", out text, opsize16);
            case 0x3F: text = "aas"; return true;
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
            case 0x62: return Bound(pe, d, ref ip, out text);
            case 0x63: return ModRm(pe, d, ref ip, "arpl", rmFirst: true, out text);
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
            case 0x6C: text = rep + "insb"; return true;
            case 0x6D: text = rep + (opsize16 ? "insw" : "insd"); return true;
            case 0x6E: text = rep + "outsb"; return true;
            case 0x6F: text = rep + (opsize16 ? "outsw" : "outsd"); return true;
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
            // 32-bit alias of 80 (ALU r/m8, imm8). Invalid in 64-bit.
            case 0x82: return AluImm(pe, d, ref ip, immBytes: 1, r8: true, out text);
            case 0x83: return AluImm(pe, d, ref ip, 1, false, out text, signed8: true);
            case 0x84: return ModRm(pe, d, ref ip, "test", rmFirst: true, out text, r8: true);
            case 0x85: return ModRm(pe, d, ref ip, "test", rmFirst: true, out text);
            case 0x86: return ModRm(pe, d, ref ip, "xchg", rmFirst: true, out text, r8: true);
            case 0x87: return ModRm(pe, d, ref ip, "xchg", rmFirst: true, out text);
            case 0x88: return ModRm(pe, d, ref ip, "mov", rmFirst: true, out text, r8: true);
            case 0x89: return ModRm(pe, d, ref ip, "mov", rmFirst: true, out text);
            case 0x8A: return ModRm(pe, d, ref ip, "mov", rmFirst: false, out text, r8: true);
            case 0x8B: return ModRm(pe, d, ref ip, "mov", rmFirst: false, out text);
            case 0x8C: return MovSreg(pe, d, ref ip, toSreg: false, out text);
            case 0x8D: return Lea(pe, d, ref ip, out text);
            // 0x8E is MOV Sreg,r/m. Do not decode: a mid-instruction
            // VA (ModRM of lea) must stay db so the dump does not
            // pretend it was a segment move.
            case 0x8F: return Unary(pe, d, ref ip, "pop", out text);
            case 0x90: text = "nop"; return true;
            case 0x91: case 0x92: case 0x93:
            case 0x94: case 0x95: case 0x96: case 0x97:
                text = "xchg eax, " + Reg(op - 0x90);
                return true;
            case 0x98: text = opsize16 ? "cbw" : "cwde"; return true;
            case 0x99: text = opsize16 ? "cwd" : "cdq"; return true;
            case 0x9A: return FarPtr(pe, d, ref ip, "call far", out text);
            case 0x9B: text = "wait"; return true;
            case 0x9C: text = "pushfd"; return true;
            case 0x9D: text = "popfd"; return true;
            case 0x9E: text = "sahf"; return true;
            case 0x9F: text = "lahf"; return true;
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
            case 0xC4: return LesLds(pe, d, ref ip, "les", out text);
            case 0xC5: return LesLds(pe, d, ref ip, "lds", out text);
            case 0xC6: return MovImm(pe, d, ref ip, imm32: false, out text);
            case 0xC7: return MovImm(pe, d, ref ip, imm32: !opsize16, out text);
            case 0xC8:
                if (ip + 3 > d.Length) return false;
                text = $"enter {BitConverter.ToUInt16(d, ip)}, {d[ip + 2]}";
                ip += 3;
                return true;
            case 0xC9: text = "leave"; return true;
            case 0xCA:
                if (ip + 2 > d.Length) return false;
                text = $"retf {BitConverter.ToUInt16(d, ip)}";
                ip += 2;
                return true;
            case 0xCB: text = "retf"; return true;
            case 0xCC: text = "int3"; return true;
            case 0xCD:
                if (ip >= d.Length) return false;
                text = $"int 0x{d[ip++]:X2}";
                return true;
            case 0xCE: text = "into"; return true;
            case 0xCF: text = "iret"; return true;
            case 0xD0: return Shift(pe, d, ref ip, "1", r8: true, out text);
            case 0xD1: return Shift(pe, d, ref ip, "1", r8: false, out text);
            case 0xD2: return Shift(pe, d, ref ip, "cl", r8: true, out text);
            case 0xD3: return Shift(pe, d, ref ip, "cl", r8: false, out text);
            case 0xD4: return AamAad(d, ref ip, "aam", out text);
            case 0xD5: return AamAad(d, ref ip, "aad", out text);
            case 0xD6: text = "salc"; return true;
            case 0xD7: text = "xlat"; return true;
            case 0xD8: case 0xD9: case 0xDA: case 0xDB:
            case 0xDC: case 0xDD: case 0xDE: case 0xDF:
                return X87(pe, d, ref ip, op, out text);
            case 0xE0: return Rel8(pe, ref ip, "loopne", out text);
            case 0xE1: return Rel8(pe, ref ip, "loope", out text);
            case 0xE2: return Rel8(pe, ref ip, "loop", out text);
            case 0xE3: return Rel8(pe, ref ip, "jecxz", out text);
            case 0xE4: return PortImm(d, ref ip, "in al", out text);
            case 0xE5: return PortImm(d, ref ip, opsize16 ? "in ax" : "in eax", out text);
            case 0xE6: return PortImmOut(d, ref ip, "al", out text);
            case 0xE7: return PortImmOut(d, ref ip, opsize16 ? "ax" : "eax", out text);
            case 0xE8: return Rel32(pe, ref ip, "call", out text);
            case 0xE9: return Rel32(pe, ref ip, "jmp", out text);
            case 0xEA: return FarPtr(pe, d, ref ip, "jmp far", out text);
            case 0xEB: return Rel8(pe, ref ip, "jmp", out text);
            case 0xEC: text = "in al, dx"; return true;
            case 0xED: text = opsize16 ? "in ax, dx" : "in eax, dx"; return true;
            case 0xEE: text = "out dx, al"; return true;
            case 0xEF: text = opsize16 ? "out dx, ax" : "out dx, eax"; return true;
            case 0xF1: text = "int1"; return true;
            case 0xF4: text = "hlt"; return true;
            case 0xF6: return F6F7(pe, d, ref ip, wide: false, out text);
            case 0xF7: return F6F7(pe, d, ref ip, wide: !opsize16, out text);
            case 0xF5: text = "cmc"; return true;
            case 0xF8: text = "clc"; return true;
            case 0xF9: text = "stc"; return true;
            case 0xFA: text = "cli"; return true;
            case 0xFB: text = "sti"; return true;
            case 0xFC: text = "cld"; return true;
            case 0xFD: text = "std"; return true;
            case 0xFE: return IncDec(pe, d, ref ip, out text);
            case 0xFF: return Ff(d, pe, ref ip, out text);
            case 0x0F: return TwoByte(pe, d, ref ip, out text, opsize16, rep);
            default:
                return false;
        }
    }

    private static bool TwoByte(PeImage pe, byte[] d, ref int ip, out string text, bool opsize16, string rep)
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

        // No ModR/M. CPUID (0F A2) / RDTSC (0F 31) / BSWAP (0F C8–CF)
        // used to steal the next bytes as a fake operand and desync
        // SSE-detect at 00A5B850.
        switch (op2)
        {
            case 0x05: text = "syscall"; return true;
            case 0x06: text = "clts"; return true;
            case 0x07: text = "sysret"; return true;
            case 0x08: text = "invd"; return true;
            case 0x09: text = "wbinvd"; return true;
            case 0x0B: text = "ud2"; return true;
            case 0x30: text = "wrmsr"; return true;
            case 0x31: text = "rdtsc"; return true;
            case 0x32: text = "rdmsr"; return true;
            case 0x33: text = "rdpmc"; return true;
            case 0x34: text = "sysenter"; return true;
            case 0x35: text = "sysexit"; return true;
            case 0x77: text = "emms"; return true;
            case 0xA0: text = "push fs"; return true;
            case 0xA1: text = "pop fs"; return true;
            case 0xA2: text = "cpuid"; return true;
            case 0xA8: text = "push gs"; return true;
            case 0xA9: text = "pop gs"; return true;
            case 0xAA: text = "rsm"; return true;
        }

        if (op2 is >= 0xC8 and <= 0xCF)
        {
            text = "bswap " + Reg(op2 - 0xC8);
            return true;
        }

        var name = op2 switch
        {
            0x12 => "movlps",
            0x13 => "movlps",
            0x16 => "movhps",
            0x17 => "movhps",
            0x1F => "nop",
            0x10 or 0x11 => "movups",
            0x28 or 0x29 => "movaps",
            0x2A => "cvtpi2ps",
            0x2C => "cvttps2pi",
            0x2D => "cvtps2pi",
            0x2E => "ucomiss",
            0x2F => "comiss",
            0x51 => "sqrtps",
            0x52 => "rsqrtps",
            0x53 => "rcpps",
            0x54 => "andps",
            0x55 => "andnps",
            0x56 => "orps",
            0x57 => "xorps",
            0x58 => "addps",
            0x59 => "mulps",
            0x5C => "subps",
            0x5D => "minps",
            0x5E => "divps",
            0x5F => "maxps",
            0x70 => "pshufd",
            0xAE => "fxsave",
            0xAF => "imul",
            0xB0 => "cmpxchg",
            0xB1 => "cmpxchg",
            0xB6 => "movzx",
            0xB7 => "movzx",
            0xBE => "movsx",
            0xBF => "movsx",
            0xA3 => "bt",
            0xA4 => "shld",
            0xA5 => "shld",
            0xAB => "bts",
            0xAC => "shrd",
            0xAD => "shrd",
            0xB3 => "btr",
            0xBB => "btc",
            0xBC => "bsf",
            0xBD => "bsr",
            0xC0 => "xadd",
            0xC1 => "xadd",
            0xC2 => "cmpps",
            0xC6 => "shufps",
            0xC7 => "cmpxchg8b",
            _ => $"0F_{op2:X2}",
        };

        if (op2 is 0xB0 or 0xC0)
            return ModRm(pe, d, ref ip, name, rmFirst: true, out text, r8: true);

        if (IsSseXmm(op2))
        {
            name = SseMnemonic(op2, opsize16, rep);
            if (!ModRm(pe, d, ref ip, name, rmFirst: op2 is 0x11 or 0x13 or 0x17 or 0x29, out text, vec: VecKind.Xmm))
                return false;
            if (op2 is 0x70 or 0xC2 or 0xC6)
            {
                if (ip >= d.Length)
                    return false;
                text += $", {d[ip++]}";
            }

            return true;
        }

        if (op2 is 0x2A)
        {
            if (!ModRmMixedVec(pe, d, ref ip, "cvtpi2ps", VecKind.Xmm, VecKind.Mm, out text))
                return false;
            return true;
        }

        if (op2 is 0x2C or 0x2D)
        {
            if (!ModRmMixedVec(pe, d, ref ip, name, VecKind.Mm, VecKind.Xmm, out text))
                return false;
            return true;
        }

        if (op2 == 0x70)
        {
            var pshuf = SseMnemonic(0x70, opsize16, rep);
            var vec = opsize16 || rep.Length > 0 ? VecKind.Xmm : VecKind.Mm;
            if (vec == VecKind.Mm)
                pshuf = "pshufw";
            if (!ModRm(pe, d, ref ip, pshuf, rmFirst: false, out text, vec: vec))
                return false;
            if (ip >= d.Length)
                return false;
            text += $", {d[ip++]}";
            return true;
        }

        if (IsMmx(op2))
        {
            if (!ModRm(pe, d, ref ip, name, rmFirst: false, out text, vec: VecKind.Mm))
                return false;
            if (op2 is 0x70)
            {
                if (ip >= d.Length)
                    return false;
                text += $", {d[ip++]}";
            }

            return true;
        }

        // movzx/movsx dest is 32-bit; only the r/m source is 8-bit (B6/BE).
        if (op2 is 0xB6 or 0xB7 or 0xBE or 0xBF)
        {
            if (ip >= d.Length)
                return false;
            var modrm = d[ip++];
            var dest = Reg((modrm >> 3) & 7);
            var src8 = op2 is 0xB6 or 0xBE;
            if (!TryMem(pe, d, ref ip, modrm, out var mem, r8: src8 && (modrm >> 6) == 3))
                return false;
            text = $"{name} {dest}, {mem}";
            return true;
        }

        // Almost every remaining 0F opcode takes ModR/M. Consume it so we
        // never return success with the IP still sitting on the operand.
        var rmFirst = op2 is 0xA3 or 0xAB or 0xB3 or 0xBB;
        if (!ModRm(pe, d, ref ip, name, rmFirst, out text))
            return false;

        // shufps / cmpps / pshufd / shld-imm / shrd-imm / bt-imm take an extra imm8.
        if (op2 is 0x70 or 0x71 or 0x72 or 0x73 or 0xA4 or 0xAC or 0xBA or 0xC2 or 0xC4 or 0xC5 or 0xC6)
        {
            if (ip >= d.Length)
                return false;
            text += $", {d[ip++]}";
        }

        if (op2 is 0xA5 or 0xAD)
            text += ", cl";

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
        (0xDD, 0) => "fld qword", (0xDD, 2) => "fst", (0xDD, 3) => "fstp",
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
        _ when escape == 0xD8 && reg == 2 => $"fcom st({rm})",
        _ when escape == 0xD8 && reg == 3 => $"fcomp st({rm})",
        _ when escape == 0xD8 && reg == 4 => $"fsub st, st({rm})",
        _ when escape == 0xD8 && reg == 5 => $"fsubr st, st({rm})",
        _ when escape == 0xD8 && reg == 6 => $"fdiv st, st({rm})",
        _ when escape == 0xD8 && reg == 7 => $"fdivr st, st({rm})",
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
            case 1: // undocumented TEST alias of /0
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

    private static bool ModRm(PeImage pe, byte[] d, ref int ip, string name, bool rmFirst, out string text, bool r8 = false, VecKind vec = VecKind.None)
    {
        text = "";
        if (ip >= d.Length) return false;
        var modrm = d[ip++];
        var regn = (modrm >> 3) & 7;
        var reg = vec switch
        {
            VecKind.Xmm => Xmm(regn),
            VecKind.Mm => Mm(regn),
            _ => r8 ? Reg8(regn) : Reg(regn),
        };
        if (!TryMem(pe, d, ref ip, modrm, out var mem, r8: r8 && (modrm >> 6) == 3, vec: vec))
            return false;
        text = rmFirst ? $"{name} {mem}, {reg}" : $"{name} {reg}, {mem}";
        return true;
    }

    private static bool ModRmMixedVec(PeImage pe, byte[] d, ref int ip, string name, VecKind dst, VecKind src, out string text)
    {
        text = "";
        if (ip >= d.Length) return false;
        var modrm = d[ip++];
        var dest = dst == VecKind.Xmm ? Xmm((modrm >> 3) & 7) : Mm((modrm >> 3) & 7);
        if (!TryMem(pe, d, ref ip, modrm, out var mem, vec: (modrm >> 6) == 3 ? src : VecKind.None))
            return false;
        text = $"{name} {dest}, {mem}";
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

    private static bool MovSreg(PeImage pe, byte[] d, ref int ip, bool toSreg, out string text)
    {
        text = "";
        if (ip >= d.Length) return false;
        var modrm = d[ip++];
        var sreg = Sreg((modrm >> 3) & 7);
        if (!TryMem(pe, d, ref ip, modrm, out var mem))
            return false;
        text = toSreg ? $"mov {sreg}, {mem}" : $"mov {mem}, {sreg}";
        return true;
    }

    private static bool LesLds(PeImage pe, byte[] d, ref int ip, string name, out string text)
    {
        text = "";
        if (ip >= d.Length) return false;
        var modrm = d[ip++];
        var dest = Reg((modrm >> 3) & 7);
        if (!TryMem(pe, d, ref ip, modrm, out var mem))
            return false;
        text = $"{name} {dest}, {mem}";
        return true;
    }

    private static bool Bound(PeImage pe, byte[] d, ref int ip, out string text)
    {
        text = "";
        if (ip >= d.Length) return false;
        var modrm = d[ip++];
        var dest = Reg((modrm >> 3) & 7);
        if (!TryMem(pe, d, ref ip, modrm, out var mem))
            return false;
        text = $"bound {dest}, {mem}";
        return true;
    }

    private static bool FarPtr(PeImage pe, byte[] d, ref int ip, string name, out string text)
    {
        text = "";
        if (ip + 6 > d.Length) return false;
        var off = BitConverter.ToUInt32(d, ip);
        var seg = BitConverter.ToUInt16(d, ip + 4);
        ip += 6;
        text = $"{name} 0x{seg:X4}:0x{off:X8}";
        return true;
    }

    private static bool AamAad(byte[] d, ref int ip, string name, out string text)
    {
        text = "";
        if (ip >= d.Length) return false;
        var imm = d[ip++];
        text = imm == 10 ? name : $"{name} {imm}";
        return true;
    }

    private static bool PortImm(byte[] d, ref int ip, string dest, out string text)
    {
        text = "";
        if (ip >= d.Length) return false;
        text = $"{dest}, 0x{d[ip++]:X2}";
        return true;
    }

    private static bool PortImmOut(byte[] d, ref int ip, string src, out string text)
    {
        text = "";
        if (ip >= d.Length) return false;
        text = $"out 0x{d[ip++]:X2}, {src}";
        return true;
    }

    private static string Sreg(int i) => i switch
    {
        0 => "es", 1 => "cs", 2 => "ss", 3 => "ds",
        4 => "fs", 5 => "gs", _ => $"sreg{i}",
    };

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

    private static bool TryMem(PeImage pe, byte[] d, ref int ip, byte modrm, out string text, bool r8 = false, VecKind vec = VecKind.None)
    {
        text = "";
        var mod = modrm >> 6;
        var rm = modrm & 7;
        if (mod == 3)
        {
            text = vec switch
            {
                VecKind.Xmm => Xmm(rm),
                VecKind.Mm => Mm(rm),
                _ => r8 ? Reg8(rm) : Reg(rm),
            };
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

    private static string Xmm(int i) => $"xmm{i}";

    private static string Mm(int i) => $"mm{i}";

    private static string Imm(PeImage pe, uint value)
    {
        if (value is 0x31545844) return "DXT1";
        if (value is 0x33545844) return "DXT3";
        if (value is 0x35545844) return "DXT5";
        var file = pe.FileOffset(value);
        // Code bytes are often printable (push ebx = 'S'). Only
        // data-section immediates may be named as strings.
        if (file >= 0 && file < pe.Data.Length && !pe.InCode(file)
            && pe.Data[file] is >= 32 and <= 126)
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
