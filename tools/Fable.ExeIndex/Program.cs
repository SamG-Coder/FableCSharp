using System.Text;
using Fable.Core;
using Fable.ExeIndex;

var cmd = args.FirstOrDefault(a => a is "index" or "split" or "translate" or "all" or "disasm" or "trace-render" or "calls" or "imm" or "vtbl" or "disp" or "scanff") ?? "all";
var install = GameInstall.TryLocate();
var exePath = args.FirstOrDefault(a => a.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
              ?? (install is null ? null : Path.Combine(install.Root, "Fable.exe"));
if (exePath is null || !File.Exists(exePath))
{
    Console.Error.WriteLine("Fable.exe not found. Set FABLE_PATH or pass the path.");
    return 2;
}

// Always land in tools/Fable.ExeIndex/out (gitignored). Never dump at repo root.
var outDir = ResolveOutDir(args);
Directory.CreateDirectory(outDir);

Console.WriteLine($"exe  {exePath}");
Console.WriteLine($"out  {outDir}");
Console.WriteLine($"step {cmd}");

var pe = PeImage.Load(exePath);

switch (cmd)
{
    case "index":
        RunIndex(pe, outDir);
        break;
    case "split":
        if (!File.Exists(Path.Combine(outDir, "00-index", "strings.tsv")))
            RunIndex(pe, outDir);
        RunSplit(pe, outDir);
        break;
    case "translate":
        if (!Directory.Exists(Path.Combine(outDir, "01-sections")))
        {
            RunIndex(pe, outDir);
            RunSplit(pe, outDir);
        }

        RunTranslatePackets(outDir);
        break;
    case "disasm":
        RunDisasm(pe, args);
        break;
    case "trace-render":
        if (!File.Exists(Path.Combine(outDir, "00-index", "xrefs.tsv")))
            RunIndex(pe, outDir);
        RunTraceRender(pe, outDir);
        break;
    case "calls":
        RunCalls(pe, args);
        break;
    case "imm":
        RunImm(pe, args);
        break;
    case "vtbl":
        RunVtbl(pe, args);
        break;
    case "disp":
        RunDisp(pe, args);
        break;
    case "scanff":
        RunScanFf(pe, args);
        break;
    default:
        RunIndex(pe, outDir);
        RunSplit(pe, outDir);
        RunTranslatePackets(outDir);
        break;
}

Console.WriteLine("done.");
Console.WriteLine("Next: translate each out/02-translate/<section>.prompt.md into out/03-pseudo/<section>.md");
Console.WriteLine("(out/ is gitignored — do not commit dumps)");
return 0;

static void RunCalls(PeImage pe, string[] args)
{
    var vaTok = args.FirstOrDefault(a => a.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                                         || (a.Length >= 6 && a.All(char.IsAsciiHexDigit)));
    if (vaTok is null || !TryParseHex(vaTok, out var target))
    {
        Console.Error.WriteLine("usage: calls <fn-va>");
        return;
    }

    var data = pe.Data;
    var hits = 0;
    foreach (var sec in pe.Sections)
    {
        if (!pe.InCode((int)sec.FileOffset))
            continue;
        var end = Math.Min(data.Length, (int)(sec.FileOffset + sec.FileSize) - 4);
        for (var i = (int)sec.FileOffset; i < end; i++)
        {
            if (data[i] != 0xE8)
                continue;
            var rel = BitConverter.ToInt32(data, i + 1);
            var dest = pe.Va(i + 5 + rel);
            if (dest != target)
                continue;
            var site = pe.Va(i);
            Console.WriteLine($"0x{site:X8}  call 0x{target:X8}");
            hits++;
            if (hits >= 80)
                return;
        }
    }

    Console.WriteLine($"calls  {hits}");
}

static void RunImm(PeImage pe, string[] args)
{
    var vaTok = args.SkipWhile(a => a is "imm").FirstOrDefault(a =>
        a.StartsWith("0x", StringComparison.OrdinalIgnoreCase) || (a.Length >= 2 && a.All(char.IsAsciiHexDigit)));
    if (vaTok is null || !TryParseHex(vaTok, out var value))
    {
        Console.Error.WriteLine("usage: imm <u32>");
        return;
    }

    var data = pe.Data;
    var hits = 0;
    foreach (var sec in pe.Sections)
    {
        var code = pe.InCode((int)sec.FileOffset);
        var end = Math.Min(data.Length, (int)(sec.FileOffset + sec.FileSize) - 3);
        for (var i = (int)sec.FileOffset; i < end; i++)
        {
            if (BitConverter.ToUInt32(data, i) != value)
                continue;
            var start = code ? X86.FindImmInsn(pe, i) : i;
            Console.WriteLine($"0x{pe.Va(start):X8}  imm=0x{value:X}  {sec.Name}");
            hits++;
            if (hits >= 40)
            {
                Console.WriteLine($"imm  {hits}+");
                return;
            }
        }
    }

    Console.WriteLine($"imm  {hits}");
}

static void RunScanFf(PeImage pe, string[] args)
{
    var tok = args.SkipWhile(a => a is "scanff").FirstOrDefault();
    if (tok is null || !int.TryParse(tok, out var off))
    {
        Console.Error.WriteLine("usage: scanff <vtbl-byte-offset>");
        return;
    }

    byte[][] pats =
    [
        [0xFF, 0x50, (byte)off],
        [0xFF, 0x51, (byte)off],
        [0xFF, 0x52, (byte)off],
        [0xFF, 0x53, (byte)off],
        [0xFF, 0x56, (byte)off],
        [0xFF, 0x57, (byte)off],
    ];
    var data = pe.Data;
    var hits = 0;
    foreach (var sec in pe.Sections)
    {
        if (!pe.InCode((int)sec.FileOffset))
            continue;
        var end = Math.Min(data.Length, (int)(sec.FileOffset + sec.FileSize) - 3);
        for (var i = (int)sec.FileOffset; i < end; i++)
        {
            var ok = false;
            foreach (var p in pats)
            {
                if (data[i] == p[0] && data[i + 1] == p[1] && data[i + 2] == p[2])
                {
                    ok = true;
                    break;
                }
            }

            if (!ok)
                continue;
            var va = pe.Va(i);
            if (va < 0x00B20000 || va > 0x00B90000)
                continue;
            Console.WriteLine($"0x{va:X8}  call [r+{off}]");
            hits++;
            if (hits >= 40)
            {
                Console.WriteLine($"scanff  {hits}+");
                return;
            }
        }
    }

    Console.WriteLine($"scanff  {hits}");
}

static void RunDisp(PeImage pe, string[] args)
{
    var tok = args.SkipWhile(a => a is "disp").FirstOrDefault(a =>
        a.StartsWith("0x", StringComparison.OrdinalIgnoreCase) || a.All(char.IsAsciiHexDigit) || a.All(char.IsDigit));
    if (tok is null)
    {
        Console.Error.WriteLine("usage: disp <displacement>");
        return;
    }

    uint disp;
    if (tok.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
    {
        if (!TryParseHex(tok, out disp))
        {
            Console.Error.WriteLine("usage: disp <displacement>");
            return;
        }
    }
    else if (!uint.TryParse(tok, out disp))
    {
        Console.Error.WriteLine("usage: disp <displacement>");
        return;
    }

    uint lo = 0, hi = uint.MaxValue;
    var extra = args.SkipWhile(a => a != tok).Skip(1).ToArray();
    if (extra.Length >= 2 && TryParseHex(extra[0], out lo) && TryParseHex(extra[1], out hi))
    {
        RunDispRange(pe, disp, lo, hi);
        return;
    }

    var needle = BitConverter.GetBytes(disp);
    var data = pe.Data;
    var hits = 0;
    foreach (var sec in pe.Sections)
    {
        if (!pe.InCode((int)sec.FileOffset))
            continue;
        var end = Math.Min(data.Length, (int)(sec.FileOffset + sec.FileSize) - 4);
        for (var i = (int)sec.FileOffset; i < end; i++)
        {
            if (data[i] != needle[0] || data[i + 1] != needle[1] || data[i + 2] != needle[2] || data[i + 3] != needle[3])
                continue;
            // typical 8B / 8D / 89 / FF modrm + disp32
            if (i < 2)
                continue;
            Console.WriteLine($"0x{pe.Va(i - 2):X8}  +{disp}  {data[i - 2]:X2} {data[i - 1]:X2}");
            hits++;
            if (hits >= 50)
            {
                Console.WriteLine($"disp  {hits}+");
                return;
            }
        }
    }

    Console.WriteLine($"disp  {hits}");
}

static void RunDispRange(PeImage pe, uint disp, uint lo, uint hi)
{
    var needle = BitConverter.GetBytes(disp);
    var data = pe.Data;
    var hits = 0;
    foreach (var sec in pe.Sections)
    {
        if (!pe.InCode((int)sec.FileOffset))
            continue;
        var end = Math.Min(data.Length, (int)(sec.FileOffset + sec.FileSize) - 4);
        for (var i = (int)sec.FileOffset + 2; i < end; i++)
        {
            if (data[i] != needle[0] || data[i + 1] != needle[1] || data[i + 2] != needle[2] || data[i + 3] != needle[3])
                continue;
            var va = pe.Va(i - 2);
            if (va < lo || va > hi)
                continue;
            Console.WriteLine($"0x{va:X8}  +{disp}  {data[i - 2]:X2} {data[i - 1]:X2}");
            hits++;
        }
    }

    Console.WriteLine($"disp-range  {hits}");
}

static void RunVtbl(PeImage pe, string[] args)
{
    var vaTok = args.SkipWhile(a => a is "vtbl").FirstOrDefault(a =>
        a.StartsWith("0x", StringComparison.OrdinalIgnoreCase) || (a.Length >= 6 && a.All(char.IsAsciiHexDigit)));
    var countTok = args.SkipWhile(a => a != vaTok).Skip(1).FirstOrDefault();
    if (vaTok is null || !TryParseHex(vaTok, out var va))
    {
        Console.Error.WriteLine("usage: vtbl <va> [count]");
        return;
    }

    var n = 16;
    if (countTok is not null && int.TryParse(countTok, out var parsed))
        n = parsed;
    var file = pe.FileOffset(va);
    if (file < 0)
    {
        Console.Error.WriteLine($"VA 0x{va:X8} is not mapped.");
        return;
    }

    for (var i = 0; i < n; i++)
    {
        var off = file + i * 4;
        if (off + 4 > pe.Data.Length)
            break;
        var slot = BitConverter.ToUInt32(pe.Data, off);
        var mapped = pe.FileOffset(slot) >= 0;
        Console.WriteLine($"[{i,2}] +{i * 4,3}  0x{slot:X8}{(mapped ? "" : "  (unmapped)")}");
    }
}

static void RunDisasm(PeImage pe, string[] args)
{
    var vaTok = args.FirstOrDefault(a => a.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                                         || (a.Length >= 6 && a.All(char.IsAsciiHexDigit)));
    var countTok = args.SkipWhile(a => a != vaTok).Skip(1).FirstOrDefault();
    if (vaTok is null || !TryParseHex(vaTok, out var va))
    {
        Console.Error.WriteLine("usage: disasm <va> [insn-count]");
        return;
    }

    var n = 64;
    if (countTok is not null && int.TryParse(countTok, out var parsed))
        n = parsed;
    var file = pe.FileOffset(va);
    if (file < 0)
    {
        Console.Error.WriteLine($"VA 0x{va:X8} is not in a mapped section.");
        return;
    }

    foreach (var line in X86.Disassemble(pe, file, n))
        Console.WriteLine(line);
}

static void RunTraceRender(PeImage pe, string outDir)
{
    var dest = Path.Combine(outDir, "01-sections");
    Directory.CreateDirectory(dest);
    uint[] vas =
    [
        0x00B29880, // just before first "Engine: Add Engine Component"
        0x00B262C0, // Add Render Layer
        0x00B33B50, // MainScene
        0x00B3D200, // shader bank registrar (LANDSCAPE_FOREGROUND nearby)
        0x00B66DC0, // EnableSky
        0x00B6CA20, // EnableLandscape
        0x00B7ED80, // EnableWater
        0x00B56650, // EnableWeather
        0x00B38C90, // EnablePrimitives
        0x00B50B50, // EnableShadows
        0x00B4B6F0, // Enable2DPrimitives
        0x00B625E0, // CEngineSkyRenderer init
        0x00B69330, // VSHADER_LANDSCAPE_FOREGROUND_BLACKOUT_PASS bind
        0x00B8B660, // VSHADER_STATIC_DIRLIGHT_FOG bind
    ];
    var sb = new StringBuilder();
    sb.AppendLine("# render-trace");
    sb.AppendLine();
    sb.AppendLine("Raw decode of engine register / scene / enable sites. Do not invent.");
    foreach (var va in vas)
    {
        var file = pe.FileOffset(va);
        sb.AppendLine();
        sb.AppendLine($"## 0x{va:X8}");
        if (file < 0)
        {
            sb.AppendLine("UNREAD (VA not mapped)");
            continue;
        }

        var n = va == 0x00B29880 || va == 0x00B3D200 ? 220 : 80;
        foreach (var line in X86.Disassemble(pe, file, n))
            sb.AppendLine(line);
    }

    var path = Path.Combine(dest, "render-trace.md");
    File.WriteAllText(path, sb.ToString());
    Console.WriteLine($"trace  {path}");
}

static string ResolveOutDir(string[] args)
{
    for (var i = 0; i < args.Length - 1; i++)
    {
        if (args[i] is not ("--out" or "-o"))
            continue;
        var explicitDir = Path.GetFullPath(args[i + 1]);
        WarnIfDumpLooksTracked(explicitDir);
        return explicitDir;
    }

    foreach (var start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
    {
        for (var dir = new DirectoryInfo(start); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Fable.ExeIndex.csproj")))
                return Path.Combine(dir.FullName, "out");
        }
    }

    throw new InvalidOperationException(
        "Could not find Fable.ExeIndex.csproj to place out/. Pass --out <dir> (that dir must stay gitignored).");
}

static void WarnIfDumpLooksTracked(string dir)
{
    var full = Path.GetFullPath(dir).Replace('\\', '/').TrimEnd('/') + "/";
    if (full.Contains("/Fable.ExeIndex/out/", StringComparison.OrdinalIgnoreCase) ||
        full.EndsWith("/Fable.ExeIndex/out/", StringComparison.OrdinalIgnoreCase))
        return;
    Console.Error.WriteLine($"warning: dump dir {dir} is not tools/Fable.ExeIndex/out — keep it gitignored.");
}

static void RunIndex(PeImage pe, string outDir)
{
    var dir = Path.Combine(outDir, "00-index");
    Directory.CreateDirectory(dir);
    var data = pe.Data;

    File.WriteAllLines(Path.Combine(dir, "imports.txt"), pe.Imports);
    File.WriteAllLines(Path.Combine(dir, "sections.txt"),
        pe.Sections.Select(s => $"{s.Name}\trva=0x{s.Rva:X}\tfile=0x{s.FileOffset:X}\tsize={s.FileSize}\tchar=0x{s.Characteristics:X}"));

    var strings = ExtractStrings(pe);
    File.WriteAllLines(Path.Combine(dir, "strings.tsv"),
        strings.Select(s => $"0x{s.Va:X8}\t0x{s.File:X}\t{Escape(s.Text)}"));

    var rtti = strings.Where(s => s.Text.StartsWith(".?AV", StringComparison.Ordinal) ||
                                  s.Text.StartsWith(".?AU", StringComparison.Ordinal)).ToList();
    File.WriteAllLines(Path.Combine(dir, "rtti.txt"), rtti.Select(s => $"0x{s.Va:X8}\t{Demangle(s.Text)}"));

    var immAt = new Dictionary<uint, List<int>>();
    foreach (var sec in pe.Sections)
    {
        if ((sec.Characteristics & 0x20000000) == 0 && sec.Name is not (".text" or "CODE"))
            continue;
        var end = Math.Min(data.Length, (int)(sec.FileOffset + sec.FileSize) - 3);
        for (var i = (int)sec.FileOffset; i < end; i++)
        {
            var v = BitConverter.ToUInt32(data, i);
            if (!immAt.TryGetValue(v, out var list))
                immAt[v] = list = [];
            if (list.Count < 32)
                list.Add(i);
        }
    }

    var xrefs = new List<string>();
    foreach (var s in strings)
    {
        if (s.Text.Length < 6 || !immAt.TryGetValue(s.Va, out var sites))
            continue;
        foreach (var site in sites)
        {
            if (!pe.InCode(site))
                continue;
            var fn = X86.FindPrologue(pe, site);
            xrefs.Add($"0x{s.Va:X8}\t0x{pe.Va(site):X8}\tfn=0x{pe.Va(fn):X8}\t{Escape(s.Text)}");
        }
    }

    File.WriteAllLines(Path.Combine(dir, "xrefs.tsv"), xrefs);
    File.WriteAllLines(Path.Combine(dir, "fourcc.tsv"), ScanFourCc(pe));
    Console.WriteLine($"index  strings={strings.Count} rtti={rtti.Count} xrefs={xrefs.Count}");
}

static List<string> ScanFourCc(PeImage pe)
{
    var data = pe.Data;
    uint[] codes = [0x31545844, 0x33545844, 0x35545844]; // DXT1 / DXT3 / DXT5
    var names = new Dictionary<uint, string>
    {
        [0x31545844] = "DXT1",
        [0x33545844] = "DXT3",
        [0x35545844] = "DXT5",
    };
    var lines = new List<string>();
    foreach (var sec in pe.Sections)
    {
        if (!pe.InCode((int)sec.FileOffset))
            continue;
        var end = Math.Min(data.Length, (int)(sec.FileOffset + sec.FileSize) - 3);
        for (var i = (int)sec.FileOffset; i < end; i++)
        {
            var v = BitConverter.ToUInt32(data, i);
            if (!names.TryGetValue(v, out var name))
                continue;
            if (i > 0 && data[i - 1] is not (0x68 or >= 0xB8 and <= 0xBF))
                continue;
            var start = X86.FindImmInsn(pe, i);
            lines.Add($"0x{v:X8}\t0x{pe.Va(start):X8}\t{name}");
        }
    }

    return lines;
}

static void RunSplit(PeImage pe, string outDir)
{
    var index = Path.Combine(outDir, "00-index");
    var dest = Path.Combine(outDir, "01-sections");
    Directory.CreateDirectory(dest);
    var fourccFile = Path.Combine(index, "fourcc.tsv");
    if (!File.Exists(fourccFile))
        File.WriteAllLines(fourccFile, ScanFourCc(pe));
    var xrefs = File.ReadAllLines(Path.Combine(index, "xrefs.tsv"));
    var rtti = File.Exists(Path.Combine(index, "rtti.txt"))
        ? File.ReadAllLines(Path.Combine(index, "rtti.txt"))
        : [];

    foreach (var section in AllSections())
    {
        var hits = xrefs.Where(l => section.Keys.Any(k =>
            l.Contains(k, StringComparison.OrdinalIgnoreCase))).Take(80).ToList();
        var types = rtti.Where(l => section.Keys.Any(k =>
            l.Contains(k, StringComparison.OrdinalIgnoreCase))).Take(40).ToList();

        var sb = new StringBuilder();
        sb.AppendLine($"# {section.Name}");
        sb.AppendLine();
        sb.AppendLine(section.Blurb);
        sb.AppendLine();
        sb.AppendLine("## RTTI");
        foreach (var t in types)
            sb.AppendLine("- " + t);
        sb.AppendLine();
        sb.AppendLine("## String xrefs");
        foreach (var h in hits)
            sb.AppendLine("- " + h);
        var fourccPath = Path.Combine(index, "fourcc.tsv");
        if (File.Exists(fourccPath) && section.Name == "texture")
        {
            sb.AppendLine();
            sb.AppendLine("## FourCC immediates (DXT1/3/5)");
            foreach (var line in File.ReadAllLines(fourccPath))
            {
                sb.AppendLine("- " + line);
                var parts = line.Split('\t');
                if (parts.Length < 3 || !TryParseHex(parts[1], out var va))
                    continue;
                var file = pe.FileOffset(va);
                if (file < 0)
                    continue;
                sb.AppendLine();
                sb.AppendLine($"### {parts[2]} @ {va:X8}");
                foreach (var dis in X86.Disassemble(pe, file, 24))
                    sb.AppendLine(dis);
            }
        }

        sb.AppendLine();
        sb.AppendLine("## Xref sites (decode from the push/mov that owns the string)");
        var seenSite = new HashSet<uint>();
        foreach (var line in hits)
        {
            var parts = line.Split('\t');
            if (parts.Length < 4)
                continue;
            if (!TryParseHex(parts[1], out var siteVa))
                continue;
            if (!seenSite.Add(siteVa))
                continue;
            var file = pe.FileOffset(siteVa);
            if (file < 0)
                continue;
            var start = X86.FindImmInsn(pe, file);
            sb.AppendLine();
            sb.AppendLine($"### {parts[3]} @ {pe.Va(start):X8}");
            foreach (var dis in X86.Disassemble(pe, start, 28))
                sb.AppendLine(dis);
            if (seenSite.Count >= 16)
                break;
        }

        sb.AppendLine();
        sb.AppendLine("## Functions (prologue if found)");
        var fns = new HashSet<int>();
        foreach (var siteVa in seenSite)
        {
            var file = pe.FileOffset(siteVa);
            if (file < 0)
                continue;
            var fn = X86.FindPrologue(pe, file);
            if (fn != X86.FindImmInsn(pe, file))
                fns.Add(fn);
        }

        foreach (var fn in fns.OrderBy(v => v).Take(10))
        {
            sb.AppendLine();
            sb.AppendLine($"### fn_{pe.Va(fn):X8}");
            foreach (var dis in X86.Disassemble(pe, fn, 40))
                sb.AppendLine(dis);
        }

        File.WriteAllText(Path.Combine(dest, section.File), sb.ToString());
        Console.WriteLine($"split  {section.File} xrefs={hits.Count} fns={fns.Count}");
    }
}

static void RunTranslatePackets(string outDir)
{
    var src = Path.Combine(outDir, "01-sections");
    var dest = Path.Combine(outDir, "02-translate");
    var pseudo = Path.Combine(outDir, "03-pseudo");
    Directory.CreateDirectory(dest);
    Directory.CreateDirectory(pseudo);
    foreach (var file in Directory.EnumerateFiles(src, "*.md"))
    {
        var name = Path.GetFileNameWithoutExtension(file);
        var packet = File.ReadAllText(file);
        var prompt = """
            You are translating a Fable.exe index packet into readable C-like pseudocode.

            Rules:
            - Only describe what the listing shows. Do not invent registers, fog distances, or formats.
            - Name functions from nearby strings / RTTI (e.g. push "DXT5" => CreateTextureDxt5).
            - Keep file VAs in comments.
            - If a step is unread, write UNREAD.
            - Output markdown with: Pathway (numbered), Pseudocode, Open questions.

            Packet:
            """ + packet;
        File.WriteAllText(Path.Combine(dest, name + ".prompt.md"), prompt);
        var stub = Path.Combine(pseudo, name + ".md");
        if (!File.Exists(stub))
        {
            File.WriteAllText(stub, $"""
                # {name} (untranslated)

                Run an agent on `02-translate/{name}.prompt.md` and replace this file
                with the Pathway + Pseudocode result.
                """);
        }

        Console.WriteLine($"translate packet  {name}.prompt.md");
    }
}

static List<AsciiString> ExtractStrings(PeImage pe)
{
    var data = pe.Data;
    var list = new List<AsciiString>();
    var start = -1;
    for (var i = 0; i <= data.Length; i++)
    {
        var ok = i < data.Length && data[i] is >= 32 and <= 126;
        if (ok)
        {
            if (start < 0) start = i;
            continue;
        }

        if (start >= 0 && i - start is >= 5 and <= 180)
        {
            var text = Encoding.ASCII.GetString(data, start, i - start);
            list.Add(new AsciiString(pe.Va(start), start, text));
        }

        start = -1;
    }

    return list;
}

static string Demangle(string rtti)
{
    if (rtti.Length < 6) return rtti;
    var body = rtti[4..];
    if (body.EndsWith("@@", StringComparison.Ordinal))
        body = body[..^2];
    return body;
}

static string Escape(string s) => s.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');

static bool TryParseHex(string token, out uint value)
{
    var span = token.AsSpan().Trim();
    if (span.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        span = span[2..];
    return uint.TryParse(span, System.Globalization.NumberStyles.HexNumber, null, out value);
}

static Section[] AllSections() =>
[
    new("texture", "texture.md", "CTextureManager load path: format u16@+12, LZO, CreateTexture DXT1/3/5.",
        ["Texture", "DXT1", "DXT3", "DXT5", "CreateTexture", "CTextureManager", "LockRect"]),
    new("sky", "sky.md", "CEngineSkyRenderer + CSkyDef / INNER_SKY / OUTER_SKY / stars.",
        ["CEngineSkyRenderer", "INNER_SKY", "OUTER_SKY", "STAR_FIELD", "EnableSky", "CSkyDef", "SKY_DEF"]),
    new("landscape", "landscape.md", "CEngineLandscapeRenderer, mesh builder, tessellator, edge strips.",
        ["CEngineLandscape", "CLandscape", "Tessel", "EdgeStrip", "StaticMap", "OpenStatic"]),
    new("render", "render.md", "CRenderManager pass order, state blocks, lighting.",
        ["CRenderManager", "CEngineStateBlock", "Diffuse2X", "CEngineLighting", "CEngineLayerRenderer",
         "BeginScene", "Add Render Layer", "Mesh Renderer", "Sprite Renderer", "Decal Renderer"]),
    new("water", "water.md", "CEngineWaterRenderer + water shaders.",
        ["CEngineWater", "WATER_", "LoadWater", "CWaterPatch"]),
    new("world", "world.md", "CWorld / CLevelLoader / OpenStaticMaps / region activate.",
        ["CWorld", "CLevelLoader", "CWorldMap", "OpenStatic", "LoadedOnPlayerProximity", "Activate Topology", "SetStaticMap"]),
    new("shaders", "shaders.md", "Named VS/PS banks used by sky, landscape, objects, water.",
        ["PSHADER_", "VSHADER_", "INNER_SKY", "OUTER_SKY", "LANDSCAPE_FOREGROUND", "TEXTURE_DIFFUSE_FOG"]),
];

internal readonly record struct AsciiString(uint Va, int File, string Text);

internal readonly record struct Section(string Name, string File, string Blurb, string[] Keys);
