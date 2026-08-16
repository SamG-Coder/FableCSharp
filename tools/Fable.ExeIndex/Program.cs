using System.Text;
using Fable.Core;
using Fable.ExeIndex;

var cmd = args.FirstOrDefault(a => a is "index" or "split" or "translate" or "all") ?? "all";
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
