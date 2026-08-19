namespace Fable.Formats.Tests;

/// <summary>
/// Reads gitignored <c>assembly/exe</c> text-map
/// dumps. PE entry walks start here, not from a
/// live <c>Fable.exe</c> process.
/// </summary>
internal sealed class AssemblyTextMap
{
    public const string ManifestName = "manifest.json";
    public const uint ListingSpan = 0x40000;

    public string Root { get; }
    public string TextMapDir { get; }

    private AssemblyTextMap(string root)
    {
        Root = root;
        TextMapDir = Path.Combine(root, "01-sections", "text-map");
    }

    public static AssemblyTextMap? TryLocate(string? explicitRoot = null)
    {
        foreach (var candidate in CandidateRoots(explicitRoot))
        {
            var manifest = Path.Combine(candidate, ManifestName);
            var listing = Path.Combine(
                candidate, "01-sections", "text-map", "listing-00400000.txt");
            if (File.Exists(manifest) && File.Exists(listing))
                return new AssemblyTextMap(Path.GetFullPath(candidate));
        }

        return null;
    }

    public string ListingPath(uint va)
    {
        var chunk = va & ~(ListingSpan - 1);
        return Path.Combine(TextMapDir, $"listing-{chunk:x8}.txt");
    }

    public string? Line(uint va)
    {
        var path = ListingPath(va);
        if (!File.Exists(path))
            return null;
        var prefix = va.ToString("X8") + "  ";
        foreach (var line in File.ReadLines(path))
        {
            if (line.StartsWith(prefix, StringComparison.Ordinal))
                return line;
        }

        return null;
    }

    public string? Text(uint va)
    {
        var line = Line(va);
        if (line is null)
            return null;
        var tab = line.IndexOf("  ", 10, StringComparison.Ordinal);
        if (tab < 0)
            return line;
        var rest = line.AsSpan(10).TrimStart();
        var sp = rest.IndexOf("  ");
        if (sp < 0)
            return rest.ToString().Trim();
        return rest[(sp + 2)..].ToString().Trim();
    }

    public uint? E8Dest(uint site)
    {
        var path = Path.Combine(TextMapDir, "e8.tsv");
        var key = $"0x{site:X8}\t";
        foreach (var line in File.ReadLines(path))
        {
            if (!line.StartsWith(key, StringComparison.Ordinal))
                continue;
            var tab = line.IndexOf('\t');
            if (tab < 0 || tab + 1 >= line.Length)
                return null;
            return ParseHex(line[(tab + 1)..]);
        }

        return null;
    }

    public string? IatName(uint slot)
    {
        var path = Path.Combine(Root, "00-index", "iat.tsv");
        var key = $"0x{slot:X8}\t";
        foreach (var line in File.ReadLines(path))
        {
            if (!line.StartsWith(key, StringComparison.Ordinal))
                continue;
            var tab = line.IndexOf('\t');
            return tab < 0 ? line : line[(tab + 1)..];
        }

        return null;
    }

    public string Utf16FromVtbl(uint va)
    {
        var path = Path.Combine(Root, "00-index", "vtbl.tsv");
        var key = $"0x{va:X8}\t";
        var chars = new List<char>();
        foreach (var line in File.ReadLines(path))
        {
            if (!line.StartsWith(key, StringComparison.Ordinal))
            {
                if (chars.Count > 0)
                    break;
                continue;
            }

            var parts = line.Split('\t');
            if (parts.Length < 3)
                continue;
            var dword = ParseHex(parts[2]);
            var lo = (char)(dword & 0xFFFF);
            var hi = (char)(dword >> 16);
            if (lo == 0)
                break;
            chars.Add(lo);
            if (hi == 0)
                break;
            chars.Add(hi);
        }

        return new string(chars.ToArray());
    }

    private static uint ParseHex(string text)
    {
        text = text.Trim();
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            text = text[2..];
        return Convert.ToUInt32(text, 16);
    }

    private static IEnumerable<string> CandidateRoots(string? explicitRoot)
    {
        if (!string.IsNullOrWhiteSpace(explicitRoot))
            yield return explicitRoot;

        var env = Environment.GetEnvironmentVariable("FABLE_ASSEMBLY");
        if (!string.IsNullOrWhiteSpace(env))
            yield return env;

        foreach (var start in new[]
                 {
                     Directory.GetCurrentDirectory(),
                     AppContext.BaseDirectory,
                 })
        {
            var dir = new DirectoryInfo(start);
            while (dir is not null)
            {
                yield return Path.Combine(dir.FullName, "assembly", "exe");
                dir = dir.Parent;
            }
        }
    }
}
