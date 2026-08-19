using System.Globalization;
using System.Text.RegularExpressions;

namespace Fable.ExeIndex;

/// <summary>Pulls grep columns out of a formatted insn line.</summary>
internal static class GrepFacts
{
    private static readonly Regex AbsHex = new(@"0x([0-9A-Fa-f]+)", RegexOptions.Compiled);
    private static readonly Regex DispMem = new(
        @"\[(?:e(?:ax|cx|dx|bx|sp|bp|si|di))([+-]\d+)\]",
        RegexOptions.Compiled);

    public static bool TryRelTarget(string text, out uint dest)
    {
        dest = 0;
        var sp = text.IndexOf(' ');
        if (sp <= 0)
            return false;
        var op = text.AsSpan(0, sp);
        if (!(op.StartsWith("j") || op.StartsWith("loop") || op.SequenceEqual("call") || op.SequenceEqual("jmp")))
            return false;
        if (text.Contains('[', StringComparison.Ordinal))
            return false;
        var rest = text[(sp + 1)..];
        if (rest.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            rest = rest[2..];
        var cut = rest.IndexOf(' ');
        if (cut > 0)
            rest = rest[..cut];
        return uint.TryParse(rest, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out dest);
    }

    public static List<uint> AbsValues(string text)
    {
        var list = new List<uint>();
        foreach (Match m in AbsHex.Matches(text))
        {
            if (uint.TryParse(m.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var v))
                list.Add(v);
        }

        return list;
    }

    public static bool TryDisp(string text, out int disp)
    {
        disp = 0;
        if (text.Contains("[0x", StringComparison.Ordinal))
            return false;
        var m = DispMem.Match(text);
        if (!m.Success)
            return false;
        return int.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out disp);
    }

    public static bool TryFf(string text, out string kind, out string mem)
    {
        kind = "";
        mem = "";
        if (text.StartsWith("call [", StringComparison.Ordinal))
            kind = "call";
        else if (text.StartsWith("jmp [", StringComparison.Ordinal))
            kind = "jmp";
        else
            return false;
        var a = text.IndexOf('[');
        var b = text.IndexOf(']');
        if (a < 0 || b <= a)
            return false;
        mem = text[a..(b + 1)];
        return true;
    }
}
