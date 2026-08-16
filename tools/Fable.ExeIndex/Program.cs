using System.Text;
using Fable.Core;
using Fable.ExeIndex;

var cmd = args.FirstOrDefault(a => a is "index" or "split" or "translate" or "all" or "disasm" or "trace-render" or "trace-landscape" or "trace-newgame" or "calls" or "imm" or "vtbl" or "disp" or "scanff") ?? "all";
var force = args.Any(a => a is "--force" or "-f");
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
var store = new DumpStore(outDir, pe.Identity, force);
Console.WriteLine($"exeId {pe.Identity}{(force ? "  force" : "")}");

switch (cmd)
{
    case "index":
        RunIndex(pe, store);
        break;
    case "split":
        if (!File.Exists(Path.Combine(outDir, "00-index", "strings.tsv")))
            RunIndex(pe, store);
        RunSplit(pe, store);
        break;
    case "translate":
        if (!Directory.Exists(Path.Combine(outDir, "01-sections")))
        {
            RunIndex(pe, store);
            RunSplit(pe, store);
        }

        RunTranslatePackets(outDir);
        break;
    case "disasm":
        RunDisasm(pe, args);
        break;
    case "trace-render":
        if (!File.Exists(Path.Combine(outDir, "00-index", "xrefs.tsv")))
            RunIndex(pe, store);
        RunTraceRender(pe, store);
        break;
    case "trace-landscape":
        if (!File.Exists(Path.Combine(outDir, "00-index", "xrefs.tsv")))
            RunIndex(pe, store);
        RunTraceLandscape(pe, store);
        break;
    case "trace-newgame":
        if (!File.Exists(Path.Combine(outDir, "00-index", "xrefs.tsv")))
            RunIndex(pe, store);
        RunTraceNewGame(pe, store);
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
        RunIndex(pe, store);
        RunSplit(pe, store);
        RunTranslatePackets(outDir);
        break;
}

store.SaveManifest();
Console.WriteLine("done.");
Console.WriteLine("Next: read out/01-sections/<family>/INDEX.md (parts are linked, not one file).");
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

static void RunTraceRender(PeImage pe, DumpStore store)
{
    const string family = "render-trace";
    if (!store.ShouldWrite(family, DumpStore.RenderTraceVersion))
    {
        Console.WriteLine($"skip  {family}  v{DumpStore.RenderTraceVersion} (exe unchanged)");
        return;
    }

    (string Name, uint Va, int N)[] parts =
    [
        ("Add Engine Component", 0x00B29880, 220),
        ("Add Render Layer", 0x00B262C0, 80),
        ("MainScene", 0x00B33B50, 80),
        ("Shader bank registrar", 0x00B3D200, 220),
        ("EnableSky", 0x00B66DC0, 80),
        ("EnableLandscape", 0x00B6CA20, 80),
        ("EnableWater", 0x00B7ED80, 80),
        ("EnableWeather", 0x00B56650, 80),
        ("EnablePrimitives", 0x00B38C90, 80),
        ("EnableShadows", 0x00B50B50, 80),
        ("Enable2DPrimitives", 0x00B4B6F0, 80),
        ("CEngineSkyRenderer init", 0x00B625E0, 80),
        ("VSHADER LANDSCAPE BLACKOUT bind", 0x00B69330, 80),
        ("VSHADER STATIC DIRLIGHT FOG bind", 0x00B8B660, 80),
    ];
    var links = new List<IndexLink>();
    foreach (var part in parts)
        links.Add(WriteFnPart(pe, store, family, part.Name, part.Va, part.N));
    store.WriteIndex(
        family, DumpStore.RenderTraceVersion, "render-trace",
        "Engine register / scene / enable sites. One file per VA. Do not invent.",
        links);
    Console.WriteLine($"trace  {family}/  parts={links.Count}  v{DumpStore.RenderTraceVersion}");
}

static void RunTraceLandscape(PeImage pe, DumpStore store)
{
    const string family = "landscape-trace";
    if (!store.ShouldWrite(family, DumpStore.LandscapeTraceVersion))
    {
        Console.WriteLine($"skip  {family}  v{DumpStore.LandscapeTraceVersion} (exe unchanged)");
        return;
    }

    var links = new List<IndexLink>
    {
        WriteVtblPart(pe, store, family, "CEngineLandscapeRenderer", 0x012A2B54, 16),
        WriteVtblPart(pe, store, family, "CEngineLandscapePatch", 0x012A8200, 16),
        WriteVtblPart(pe, store, family, "CLandscapeBackgroundPatch", 0x012A803C, 16),
        WriteFnPart(pe, store, family, "SetStaticMapFileForUse", 0x00B428E0, 80),
        WriteFnPart(pe, store, family, "OpenStaticMaps", 0x00B42750, 120),
        WriteFnPart(pe, store, family, "OpenOneMap", 0x00B42530, 180),
        WriteFnPart(pe, store, family, "ParseMapHeader", 0x00B3EFA0, 80),
        WriteFnPart(pe, store, family, "LoadWaterData", 0x00B41FA0, 80),
        WriteFnPart(pe, store, family, "Sea name onto water renderer", 0x00B6D4D0, 40),
        WriteFnPart(pe, store, family, "Activate Topology", 0x004FCBB0, 20),
        WriteFnPart(pe, store, family, "Build current patch", 0x00BDD0E0, 160),
        WriteFnPart(pe, store, family, "Attach patch", 0x00BDF010, 80),
        WriteFnPart(pe, store, family, "Create background patch", 0x00BE03A0, 80),
        WriteFnPart(pe, store, family, "Tile stream", 0x00BF9290, 160),
        WriteFnPart(pe, store, family, "Tile vector", 0x00BF97A0, 60),
        WriteFnPart(pe, store, family, "Tile vector erase", 0x00BF9420, 40),
        WriteFnPart(pe, store, family, "Tile vector helper", 0x00BF8E50, 80),
        WriteFnPart(pe, store, family, "WLD NewRegion writer", 0x004FD040, 120),
        WriteFnPart(pe, store, family, "WLD NewRegion reader", 0x0050881D, 80),
        WriteFnPart(pe, store, family, "WLD ContainsMap", 0x004FD7F9, 40),
        WriteFnPart(pe, store, family, "WLD SeesMap", 0x004FD996, 40),
        WriteFnPart(pe, store, family, "Background patch ctor", 0x00BE6090, 80),
        WriteFnPart(pe, store, family, "Landscape draw vtbl+16", 0x00B6B0B0, 160),
        WriteFnPart(pe, store, family, "Shared lighting setup", 0x00B67480, 40),
        WriteFnPart(pe, store, family, "BG bit4 setup", 0x00B671A0, 40),
        WriteFnPart(pe, store, family, "FG compact+bind", 0x00B68DA0, 200),
        WriteFnPart(pe, store, family, "FG device dirty", 0x00B677D0, 80),
        WriteFnPart(pe, store, family, "Unbind stages 0/1/2", 0x00B67510, 80),
        WriteFnPart(pe, store, family, "Patch submit bit4", 0x00BDC060, 20),
        WriteFnPart(pe, store, family, "Patch submit bit40 frustum", 0x00BDC2D0, 120),
        WriteFnPart(pe, store, family, "BG draw frustum", 0x00BF71D0, 100),
        WriteFnPart(pe, store, family, "Per-cell submit", 0x00BF4570, 200),
        WriteFnPart(pe, store, family, "Per-cell SetTexture stage0", 0x00BF50E0, 80),
        WriteFnPart(pe, store, family, "SetVSConstantF wrapper", 0x00989A60, 40),
        WriteFnPart(pe, store, family, "Layer bind", 0x00BE7BE0, 100),
        WriteFnPart(pe, store, family, "Land layer select", 0x00BE6F70, 80),
        WriteFnPart(pe, store, family, "VS bind BLACKOUT + FOREGROUND", 0x00B69330, 80),
    };
    store.WriteIndex(
        family, DumpStore.LandscapeTraceVersion, "landscape-trace",
        "New-game landscape load + draw + cull + UV/texture bind. One file per VA.",
        links);
    Console.WriteLine($"trace  {family}/  parts={links.Count}  v{DumpStore.LandscapeTraceVersion}");
}

static void RunTraceNewGame(PeImage pe, DumpStore store)
{
    const string family = "newgame-trace";
    if (!store.ShouldWrite(family, DumpStore.NewGameTraceVersion))
    {
        Console.WriteLine($"skip  {family}  v{DumpStore.NewGameTraceVersion} (exe unchanged)");
        return;
    }

    var links = new List<IndexLink>
    {
        WriteSitePart(pe, store, family, "UI TEXT NEW GAME", 0x00595B42, 120),
        WriteFnPart(pe, store, family, "UI FRONTEND MAIN MENU", 0x0059899A, 80),
        WriteFnPart(pe, store, family, "START NEW QUEST", 0x004B5080, 80),
        WriteSitePart(pe, store, family, "AddTestQuest", 0x004A0E93, 80),
        WriteSitePart(pe, store, family, "Q NewOakValeIntro", 0x00CD6E28, 80),
        WriteSitePart(pe, store, family, "Q NewOakValeIntro script", 0x00CE791E, 80),
        WriteFnPart(pe, store, family, "StartOakVale new game", 0x00DBDE40, 200),
        WriteFnPart(pe, store, family, "Hero spawn helper 00CB7940", 0x00CB7940, 80),
        WriteFnPart(pe, store, family, "Creature construct 004AA840", 0x004AA840, 80),
        WriteSitePart(pe, store, family, "Q NewOakValeIntro PreAttack", 0x00DBE0C9, 80),
        WriteFnPart(pe, store, family, "WLD NewRegion reader", 0x0050881D, 80),
        WriteFnPart(pe, store, family, "WLD ContainsMap", 0x004FD7F9, 40),
        WriteFnPart(pe, store, family, "WLD SeesMap", 0x004FD996, 40),
        WriteFnPart(pe, store, family, "SetStaticMapFileForUse", 0x00B428E0, 80),
        WriteFnPart(pe, store, family, "OpenStaticMaps", 0x00B42750, 120),
        WriteFnPart(pe, store, family, "OpenOneMap", 0x00B42530, 120),
        WriteFnPart(pe, store, family, "LoadWaterData", 0x00B41FA0, 80),
        WriteFnPart(pe, store, family, "Sea name onto water renderer", 0x00B6D4D0, 40),
        WriteFnPart(pe, store, family, "Activate Topology", 0x004FCBB0, 20),
        WriteFnPart(pe, store, family, "Build current patch", 0x00BDD0E0, 160),
        WriteFnPart(pe, store, family, "Tile stream", 0x00BF9290, 160),
        WriteVtblPart(pe, store, family, "CEngineLandscapeRenderer", 0x012A2B54, 16),
        WriteFnPart(pe, store, family, "Landscape draw vtbl+16", 0x00B6B0B0, 160),
        WriteFnPart(pe, store, family, "Per-cell submit", 0x00BF4570, 200),
        WriteFnPart(pe, store, family, "Per-cell after flag check", 0x00BF4649, 200),
        WriteFnPart(pe, store, family, "Per-cell DrawIndexed tail", 0x00BF5363, 200),
        WriteFnPart(pe, store, family, "Per-cell SetTexture stage0", 0x00BF50E0, 80),
        WriteFnPart(pe, store, family, "Per-cell SetTexture stage1", 0x00BF5491, 80),
        WriteFnPart(pe, store, family, "Diffuse2X 0098B5E0", 0x0098B5E0, 80),
        WriteFnPart(pe, store, family, "Diffuse2X full", 0x0098B5E0, 160, stopOnRet: false),
        WriteFnPart(pe, store, family, "Diffuse2X apply body", 0x0098B601, 120),
        WriteFnPart(pe, store, family, "State apply 00987FE0", 0x00987FE0, 60),
        WriteFnPart(pe, store, family, "State apply 00988110", 0x00988110, 60),
        WriteFnPart(pe, store, family, "State apply 00A0AA20", 0x00A0AA20, 80),
        WriteFnPart(pe, store, family, "Patch submit bit40 frustum", 0x00BDC2D0, 160),
        WriteFnPart(pe, store, family, "Unbind stages 0/1/2", 0x00B67510, 40),
        WriteFnPart(pe, store, family, "Water draw vtbl+16", 0x00B783F0, 160),
        WriteFnPart(pe, store, family, "Water draw full", 0x00B783F0, 280, stopOnRet: false),
        WriteFnPart(pe, store, family, "Sky draw vtbl+16", 0x00B662F0, 80),
        WriteFnPart(pe, store, family, "MainScene plus616 draw", 0x00B33010, 120),
        WriteFnPart(pe, store, family, "Static mesh VS bind", 0x00B8B660, 80),
        WriteFnPart(pe, store, family, "VS bind LANDSCAPE FOREGROUND", 0x00B69330, 80),
    };
    store.WriteIndex(
        family, DumpStore.NewGameTraceVersion, "newgame-trace",
        "Click New through first-seen StartOakVale: UI, quest, kid, NewRegion, static maps, tiles, draw.",
        links);
    Console.WriteLine($"trace  {family}/  parts={links.Count}  v{DumpStore.NewGameTraceVersion}");
}

static IndexLink WriteFnPart(PeImage pe, DumpStore store, string family, string name, uint va, int n, bool stopOnRet = true)
{
    var slug = DumpStore.Slug(name, va);
    var sb = new StringBuilder();
    sb.AppendLine($"# {name}");
    sb.AppendLine();
    sb.AppendLine($"VA `0x{va:X8}` · `{n}` insns. [INDEX](INDEX.md)");
    sb.AppendLine();
    var file = pe.FileOffset(va);
    if (file < 0)
        sb.AppendLine("UNREAD (VA not mapped)");
    else if (stopOnRet)
    {
        foreach (var line in X86.Disassemble(pe, file, n))
            sb.AppendLine(line);
    }
    else
    {
        foreach (var line in X86.DisassembleAll(pe, file, n))
            sb.AppendLine(line);
    }

    store.WritePart(family, slug, sb.ToString());
    return new IndexLink(slug, name, va);
}

static IndexLink WriteSitePart(PeImage pe, DumpStore store, string family, string name, uint siteVa, int n)
{
    var file = pe.FileOffset(siteVa);
    var startVa = siteVa;
    var startFile = file;
    if (file >= 0)
    {
        startFile = X86.FindPrologue(pe, file);
        startVa = pe.Va(startFile);
    }

    return WriteFnPart(pe, store, family, name, startVa, n);
}

static IndexLink WriteVtblPart(PeImage pe, DumpStore store, string family, string name, uint va, int n)
{
    var slug = DumpStore.Slug("vtbl-" + name, va);
    var sb = new StringBuilder();
    sb.AppendLine($"# vtbl {name}");
    sb.AppendLine();
    sb.AppendLine($"VA `0x{va:X8}`. [INDEX](INDEX.md)");
    sb.AppendLine();
    var file = pe.FileOffset(va);
    if (file < 0)
        sb.AppendLine("UNREAD (VA not mapped)");
    else
    {
        for (var i = 0; i < n; i++)
        {
            var off = file + i * 4;
            if (off + 4 > pe.Data.Length)
                break;
            var slot = BitConverter.ToUInt32(pe.Data, off);
            var mapped = pe.FileOffset(slot) >= 0;
            sb.AppendLine($"[{i,2}] +{i * 4,3}  0x{slot:X8}{(mapped ? "" : "  (unmapped)")}");
        }
    }

    store.WritePart(family, slug, sb.ToString());
    return new IndexLink(slug, "vtbl " + name, va);
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

static void RunIndex(PeImage pe, DumpStore store)
{
    const string family = "index";
    if (!store.ShouldWrite(family, DumpStore.IndexVersion))
    {
        Console.WriteLine($"skip  {family}  v{DumpStore.IndexVersion} (exe unchanged)");
        return;
    }

    var dir = Path.Combine(store.OutDir, "00-index");
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
    store.MarkWritten(family, DumpStore.IndexVersion);
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

static void RunSplit(PeImage pe, DumpStore store)
{
    var index = Path.Combine(store.OutDir, "00-index");
    var fourccFile = Path.Combine(index, "fourcc.tsv");
    if (!File.Exists(fourccFile))
        File.WriteAllLines(fourccFile, ScanFourCc(pe));
    var xrefs = File.ReadAllLines(Path.Combine(index, "xrefs.tsv"));
    var rtti = File.Exists(Path.Combine(index, "rtti.txt"))
        ? File.ReadAllLines(Path.Combine(index, "rtti.txt"))
        : [];

    foreach (var section in AllSections())
    {
        var family = section.Name;
        if (!store.ShouldWrite("split-" + family, DumpStore.SplitVersion))
        {
            Console.WriteLine($"skip  {family}  v{DumpStore.SplitVersion} (exe unchanged)");
            continue;
        }

        var hits = xrefs.Where(l => section.Keys.Any(k =>
            l.Contains(k, StringComparison.OrdinalIgnoreCase))).Take(80).ToList();
        var types = rtti.Where(l => section.Keys.Any(k =>
            l.Contains(k, StringComparison.OrdinalIgnoreCase))).Take(40).ToList();
        var links = new List<IndexLink>();

        var rttiMd = new StringBuilder();
        rttiMd.AppendLine($"# {section.Name} RTTI");
        rttiMd.AppendLine();
        foreach (var t in types)
            rttiMd.AppendLine("- " + t);
        store.WritePart(family, "rtti", rttiMd.ToString());
        links.Add(new IndexLink("rtti", "RTTI", 0));

        var xrefMd = new StringBuilder();
        xrefMd.AppendLine($"# {section.Name} string xrefs");
        xrefMd.AppendLine();
        foreach (var h in hits)
            xrefMd.AppendLine("- " + h);
        store.WritePart(family, "xrefs", xrefMd.ToString());
        links.Add(new IndexLink("xrefs", "String xrefs", 0));

        if (section.Name == "texture" && File.Exists(fourccFile))
        {
            foreach (var line in File.ReadAllLines(fourccFile))
            {
                var parts = line.Split('\t');
                if (parts.Length < 3 || !TryParseHex(parts[1], out var va))
                    continue;
                var file = pe.FileOffset(va);
                if (file < 0)
                    continue;
                links.Add(WriteFnPart(pe, store, family, parts[2], va, 24));
            }
        }

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
            var label = parts[3].Length > 48 ? parts[3][..48] : parts[3];
            links.Add(WriteFnPart(pe, store, family, "site " + label, pe.Va(start), 28));
            if (seenSite.Count >= 16)
                break;
        }

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
            links.Add(WriteFnPart(pe, store, family, "fn", pe.Va(fn), 40));

        store.WriteIndex(family, DumpStore.SplitVersion, section.Name, section.Blurb, links);
        Console.WriteLine($"split  {family}/  xrefs={hits.Count} parts={links.Count}");
    }
}

static void RunTranslatePackets(string outDir)
{
    var src = Path.Combine(outDir, "01-sections");
    var dest = Path.Combine(outDir, "02-translate");
    var pseudo = Path.Combine(outDir, "03-pseudo");
    Directory.CreateDirectory(dest);
    Directory.CreateDirectory(pseudo);
    var indexes = Directory.Exists(src)
        ? Directory.GetFiles(src, "INDEX.md", SearchOption.AllDirectories)
        : [];
    if (indexes.Length == 0)
        indexes = Directory.Exists(src) ? Directory.GetFiles(src, "*.md") : [];
    foreach (var file in indexes)
    {
        var name = Path.GetFileName(file).Equals("INDEX.md", StringComparison.OrdinalIgnoreCase)
            ? new DirectoryInfo(Path.GetDirectoryName(file)!).Name
            : Path.GetFileNameWithoutExtension(file);
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
