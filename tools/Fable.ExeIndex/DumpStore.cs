using System.Text;
using System.Text.Json;

namespace Fable.ExeIndex;

/// <summary>
/// Writes <c>out/01-sections/&lt;family&gt;/</c> as one file per part plus an
/// INDEX that links them. A family is rewritten only when its recipe
/// version or the exe identity changes (or <see cref="Force"/> is set).
/// </summary>
internal sealed class DumpStore
{
    public const int IndexVersion = 1;
    public const int SplitVersion = 2;
    public const int LandscapeTraceVersion = 5;
    public const int RenderTraceVersion = 2;
    public const int NewGameTraceVersion = 100;
    public const int ScriptRuntimeVersion = 25;
    public const int ScriptBankVersion = 7;
    public const int ShaderTokensVersion = 7;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly string _manifestPath;
    private readonly Manifest _manifest;

    public DumpStore(string outDir, string exeId, bool force)
    {
        OutDir = outDir;
        ExeId = exeId;
        Force = force;
        _manifestPath = Path.Combine(outDir, "manifest.json");
        _manifest = LoadManifest(_manifestPath);
    }

    public string OutDir { get; }
    public string ExeId { get; }
    public bool Force { get; }

    public bool ShouldWrite(string family, int version)
    {
        if (Force)
            return true;
        if (!_manifest.Dumps.TryGetValue(family, out var rec))
            return true;
        return rec.Version != version || !string.Equals(rec.ExeId, ExeId, StringComparison.Ordinal);
    }

    public void MarkWritten(string family, int version)
    {
        _manifest.ExeId = ExeId;
        _manifest.Dumps[family] = new DumpRecord { Version = version, ExeId = ExeId };
    }

    public void SaveManifest()
    {
        Directory.CreateDirectory(OutDir);
        File.WriteAllText(_manifestPath, JsonSerializer.Serialize(_manifest, JsonOptions));
    }

    public string FamilyDir(string family)
    {
        var dir = Path.Combine(OutDir, "01-sections", family);
        Directory.CreateDirectory(dir);
        return dir;
    }

    public void WriteIndex(
        string family,
        int version,
        string title,
        string blurb,
        IReadOnlyList<IndexLink> links)
    {
        var dir = FamilyDir(family);
        var sb = new StringBuilder();
        sb.AppendLine($"# {title}");
        sb.AppendLine();
        sb.AppendLine(blurb);
        sb.AppendLine();
        sb.AppendLine($"version **{version}** · exe `{ExeId}`");
        sb.AppendLine();
        sb.AppendLine("| part | va | file |");
        sb.AppendLine("|---|---|---|");
        foreach (var link in links)
        {
            var va = link.Va == 0 ? "—" : $"0x{link.Va:X8}";
            sb.AppendLine($"| {link.Title} | {va} | [{link.Slug}.md]({link.Slug}.md) |");
        }

        File.WriteAllText(Path.Combine(dir, "INDEX.md"), sb.ToString());
        WriteStub(family + ".md", family, title);
        MarkWritten(family, version);
    }

    public void WritePart(string family, string slug, string markdown)
    {
        var dir = FamilyDir(family);
        var path = Path.Combine(dir, slug + ".md");
        File.WriteAllText(path, markdown);
    }

    public void WriteStub(string fileName, string family, string title)
    {
        var dest = Path.Combine(OutDir, "01-sections");
        Directory.CreateDirectory(dest);
        File.WriteAllText(
            Path.Combine(dest, fileName),
            $"""
            # {title}

            Split into [{family}/INDEX.md]({family}/INDEX.md). Do not grow this stub.
            """);
    }

    public static string Slug(string name, uint va)
    {
        var sb = new StringBuilder(name.Length + 12);
        foreach (var ch in name)
        {
            if (char.IsAsciiLetterOrDigit(ch))
                sb.Append(ch);
            else if (ch is ' ' or '_' or '-' or '.')
                sb.Append('-');
        }

        var stem = sb.ToString().Trim('-');
        while (stem.Contains("--", StringComparison.Ordinal))
            stem = stem.Replace("--", "-", StringComparison.Ordinal);
        return va == 0 ? stem.ToLowerInvariant() : $"{stem.ToLowerInvariant()}-{va:x8}";
    }

    private static Manifest LoadManifest(string path)
    {
        if (!File.Exists(path))
            return new Manifest();
        try
        {
            return JsonSerializer.Deserialize<Manifest>(File.ReadAllText(path), JsonOptions) ?? new Manifest();
        }
        catch (Exception)
        {
            return new Manifest();
        }
    }

    private sealed class Manifest
    {
        public string ExeId { get; set; } = "";
        public Dictionary<string, DumpRecord> Dumps { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private sealed class DumpRecord
    {
        public int Version { get; set; }
        public string ExeId { get; set; } = "";
    }
}

internal readonly record struct IndexLink(string Slug, string Title, uint Va);
