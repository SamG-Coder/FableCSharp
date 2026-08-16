using System.Text;
using Fable.Core;
using Fable.ExeIndex;

var cmd = args.FirstOrDefault(a => a is "index" or "split" or "translate" or "all" or "disasm" or "fn" or "trace-render" or "trace-landscape" or "trace-newgame" or "map-newgame" or "calls" or "imm" or "vtbl" or "disp" or "scanff" or "floats" or "calldisp" or "scan") ?? "all";
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
    case "fn":
        RunFn(pe, args);
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
    case "map-newgame":
        RunMapNewGame(pe, store);
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
    case "floats":
        RunFloats(pe, args);
        break;
    case "calldisp":
        RunCallDisp(pe, args);
        break;
    case "scan":
        RunScan(pe, args);
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
        Console.Error.WriteLine("usage: imm <u32> [lo hi]");
        return;
    }

    uint lo = 0, hi = uint.MaxValue;
    var extra = args.SkipWhile(a => a != vaTok).Skip(1).ToArray();
    var ranged = extra.Length >= 2 && TryParseHex(extra[0], out lo) && TryParseHex(extra[1], out hi);
    if (!ranged)
    {
        lo = 0;
        hi = uint.MaxValue;
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
            var va = pe.Va(start);
            if (va < lo || va > hi)
                continue;
            Console.WriteLine($"0x{va:X8}  imm=0x{value:X}  {sec.Name}");
            hits++;
            if (!ranged && hits >= 40)
            {
                Console.WriteLine($"imm  {hits}+");
                return;
            }
        }
    }

    Console.WriteLine($"imm  {hits}");
}

static void RunScan(PeImage pe, string[] args)
{
    var hex = args.SkipWhile(a => a is "scan").FirstOrDefault();
    if (hex is null || hex.Length < 2 || hex.Length % 2 != 0)
    {
        Console.Error.WriteLine("usage: scan <hex-bytes> [lo hi]");
        return;
    }

    var needle = new byte[hex.Length / 2];
    for (var n = 0; n < needle.Length; n++)
    {
        if (!byte.TryParse(hex.AsSpan(n * 2, 2), System.Globalization.NumberStyles.HexNumber, null, out needle[n]))
        {
            Console.Error.WriteLine("usage: scan <hex-bytes> [lo hi]");
            return;
        }
    }

    uint lo = 0, hi = uint.MaxValue;
    var extra = args.SkipWhile(a => a != hex).Skip(1).ToArray();
    var ranged = extra.Length >= 2 && TryParseHex(extra[0], out lo) && TryParseHex(extra[1], out hi);

    var data = pe.Data;
    var hits = 0;
    foreach (var sec in pe.Sections)
    {
        if (!pe.InCode((int)sec.FileOffset))
            continue;
        var end = Math.Min(data.Length, (int)(sec.FileOffset + sec.FileSize) - needle.Length);
        for (var i = (int)sec.FileOffset; i < end; i++)
        {
            var ok = true;
            for (var j = 0; j < needle.Length; j++)
            {
                if (data[i + j] != needle[j])
                {
                    ok = false;
                    break;
                }
            }

            if (!ok)
                continue;
            var va = pe.Va(i);
            if (va < lo || va > hi)
                continue;
            Console.WriteLine($"0x{va:X8}  scan");
            hits++;
            if (!ranged && hits >= 40)
            {
                Console.WriteLine($"scan  {hits}+");
                return;
            }
        }
    }

    Console.WriteLine($"scan  {hits}");
}

static void RunCallDisp(PeImage pe, string[] args)
{
    var tok = args.SkipWhile(a => a is "calldisp").FirstOrDefault();
    if (tok is null || !TryParseHex(tok, out var disp))
    {
        Console.Error.WriteLine("usage: calldisp <disp32> [lo hi]");
        return;
    }

    uint lo = 0, hi = uint.MaxValue;
    var extra = args.SkipWhile(a => a != tok).Skip(1).ToArray();
    if (extra.Length >= 2 && TryParseHex(extra[0], out lo) && TryParseHex(extra[1], out hi))
    {
        // ranged: keep going past the default 60-hit cap
    }
    else
    {
        extra = [];
        lo = 0;
        hi = uint.MaxValue;
    }

    // call [reg+disp32]: FF 90/91/92/93/96/97 xx xx xx xx
    // call [reg+disp8]:  FF 50/51/52/53/54/55/56/57 xx
    byte[] mods32 = [0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97];
    byte[] mods8 = [0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57];
    var data = pe.Data;
    var hits = 0;
    foreach (var sec in pe.Sections)
    {
        if (!pe.InCode((int)sec.FileOffset))
            continue;
        var end = Math.Min(data.Length, (int)(sec.FileOffset + sec.FileSize) - 6);
        for (var i = (int)sec.FileOffset; i < end; i++)
        {
            if (data[i] != 0xFF)
                continue;
            var is32 = false;
            foreach (var m in mods32)
            {
                if (data[i + 1] == m)
                {
                    is32 = true;
                    break;
                }
            }

            uint va;
            if (is32)
            {
                if (BitConverter.ToUInt32(data, i + 2) != disp)
                    continue;
                va = pe.Va(i);
                if (va < lo || va > hi)
                    continue;
                Console.WriteLine($"0x{va:X8}  call [r+0x{disp:X}]");
                hits++;
            }
            else if (disp <= 0x7F)
            {
                var is8 = false;
                foreach (var m in mods8)
                {
                    if (data[i + 1] == m)
                    {
                        is8 = true;
                        break;
                    }
                }

                if (!is8 || data[i + 2] != (byte)disp)
                    continue;
                va = pe.Va(i);
                if (va < lo || va > hi)
                    continue;
                Console.WriteLine($"0x{va:X8}  call [r+0x{disp:X}]8");
                hits++;
            }
            else
                continue;

            if (extra.Length < 2 && hits >= 60)
            {
                Console.WriteLine($"calldisp  {hits}+");
                return;
            }
        }
    }

    Console.WriteLine($"calldisp  {hits}");
}

static void RunFloats(PeImage pe, string[] args)
{
    var toks = args.SkipWhile(a => a is "floats").ToArray();
    if (toks.Length == 0 || !TryParseHex(toks[0], out var va))
    {
        Console.Error.WriteLine("usage: floats <va> [count]");
        return;
    }

    var count = 8;
    if (toks.Length > 1 && int.TryParse(toks[1], out var n) && n is > 0 and <= 64)
        count = n;
    var file = pe.FileOffset(va);
    if (file < 0)
    {
        Console.Error.WriteLine($"UNREAD 0x{va:X8}");
        return;
    }

    for (var i = 0; i < count; i++)
    {
        var off = file + i * 4;
        if (off + 4 > pe.Data.Length)
            break;
        var bits = BitConverter.ToUInt32(pe.Data, off);
        var f = BitConverter.ToSingle(pe.Data, off);
        Console.WriteLine($"0x{va + (uint)(i * 4):X8}  {f,12:0.########}  0x{bits:X8}");
    }
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

static void RunFn(PeImage pe, string[] args)
{
    var vaTok = args.FirstOrDefault(a => a.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                                         || (a.Length >= 6 && a.All(char.IsAsciiHexDigit)));
    var countTok = args.SkipWhile(a => a != vaTok).Skip(1).FirstOrDefault();
    if (vaTok is null || !TryParseHex(vaTok, out var va))
    {
        Console.Error.WriteLine("usage: fn <va> [max-insns]");
        return;
    }

    var n = 2000;
    if (countTok is not null && int.TryParse(countTok, out var parsed))
        n = parsed;
    var file = pe.FileOffset(va);
    if (file < 0)
    {
        Console.Error.WriteLine($"VA 0x{va:X8} is not in a mapped section.");
        return;
    }

    var exact = args.Any(a => a is "--exact");
    var startFile = exact ? file : X86.FindPrologue(pe, file);
    var startVa = pe.Va(startFile);
    var steps = X86.WalkFunction(pe, startFile, n);
    Console.WriteLine($"fn  0x{startVa:X8}  insns={steps.Count}  from 0x{va:X8}{(exact ? "  exact" : "")}");
    var calls = new List<uint>();
    foreach (var step in steps)
    {
        Console.WriteLine($"  //{step.Va:X8}: {step.Text}");
        if (step.DirectCall is { } dest)
            calls.Add(dest);
    }

    if (calls.Count > 0)
    {
        Console.WriteLine("calls");
        foreach (var c in calls.Distinct())
            Console.WriteLine($"  0x{c:X8}");
    }

    if (steps.Count >= n)
        Console.WriteLine($"truncated at {n} insns");
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
        WriteFnPart(pe, store, family, "StartOakVale HerosOldHouse", 0x00DBE0C6, 200),
        WriteFnPart(pe, store, family, "Hero exists 00CB7940", 0x00CB7940, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "String dtor 004AA840", 0x004AA840, 40),
        WriteFnPart(pe, store, family, "PostAttack quest 00DBE3C0", 0x00DBE3C0, 80),
        WriteFnPart(pe, store, family, "MARKER LIGHT apply", 0x0089FAA8, 80),
        WriteFnPart(pe, store, family, "CTCLight colour store", 0x00640BB0, 40),
        WriteFnPart(pe, store, family, "Lighting time-of-day blend", 0x00B46C80, 120),
        WriteFnPart(pe, store, family, "TOD light upload 00989830", 0x00B46EF5, 20),
        WriteFnPart(pe, store, family, "Light slot apply 00989830", 0x00989830, 40),
        WriteFnPart(pe, store, family, "Point light pack 00B44F20", 0x00B44F20, 80),
        WriteFnPart(pe, store, family, "Point light gather 00B46280", 0x00B46280, 80),
        WriteFnPart(pe, store, family, "Point light record pack 00B49320", 0x00B49320, 40),
        WriteFnPart(pe, store, family, "c31 atten flush 0098A6F6", 0x0098A6F6, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "Per-cell point pack 00BF467E", 0x00BF466E, 20),
        WriteFnPart(pe, store, family, "Disable extra light slot 00988640", 0x00988640, 20),
        WriteFnPart(pe, store, family, "Light constant flush", 0x0098A6C0, 40),
        WriteFnPart(pe, store, family, "LayoutLights name", 0x00BDB580, 20),
        WriteFnPart(pe, store, family, "LayoutLights ctor", 0x00BDB400, 120),
        WriteFnPart(pe, store, family, "Light apply 00F39D40", 0x00F39D40, 30),
        WriteFnPart(pe, store, family, "Lighting mgr ctor defaults", 0x00B482A0, 160),
        WriteFnPart(pe, store, family, "Light flush 0098A540", 0x0098A540, 80),
        WriteFnPart(pe, store, family, "c35 flush 0098A760", 0x0098A760, 30),
        WriteFnPart(pe, store, family, "c35 setter 0098B2C0", 0x0098B2C0, 50),
        WriteFnPart(pe, store, family, "TOD c35 upload 00B46F23", 0x00B46ED9, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "PALSKIN register upload 009896D0", 0x009896D0, 30),
        WriteFnPart(pe, store, family, "CThingPlayerCreature Create", 0x006AC910, 80),
        WriteWalkPart(pe, store, family, "ConstructFromParams 006A9DD0", 0x006A9DD0, 200),
        WriteWalkPart(pe, store, family, "Thing construct 006A5950", 0x006A5950, 120),
        WriteWalkPart(pe, store, family, "Parent construct 00662880", 0x00662880, 80),
        WriteWalkPart(pe, store, family, "Thing construct wrapper 008388D0", 0x008388D0, 80),
        WriteWalkPart(pe, store, family, "Thing activate 004C9CA0", 0x004C9CA0, 80),
        WriteWalkPart(pe, store, family, "StartOakVale full 00DBDE40", 0x00DBDE40, 500),
        WriteWalkPart(pe, store, family, "PlayAnimation script 00CBFACA", 0x00CBFACA, 400),
        WriteWalkPart(pe, store, family, "NONE primitive pass 00B89C30", 0x00B89C30, 2000),
        WriteWalkPart(pe, store, family, "NONE-draw layer 00BBE090", 0x00BBE090, 800),
        WriteWalkPart(pe, store, family, "NONE-draw PALSKIN 00BC3F30", 0x00BC3F30, 1000),
        WriteWalkPart(pe, store, family, "Primitive layer switch 00BBC130", 0x00BBC130, 200),
        WriteWalkPart(pe, store, family, "Static-lit CCW 00BB2540", 0x00BB2540, 900),
        WriteWalkPart(pe, store, family, "Landscape CCW 00B24850", 0x00B24850, 400),
        WriteFnPart(pe, store, family, "CPlayer CreateCharacter", 0x00489D40, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "CPlayer CreateCharacter body", 0x00489D86, 120, stopOnRet: false),
        WriteFnPart(pe, store, family, "Thing position getter 006A4D00", 0x006A4D00, 10),
        WriteFnPart(pe, store, family, "Water draw empty return 00B7A865", 0x00B7A865, 20),
        WriteFnPart(pe, store, family, "SetVSConstantF wrapper", 0x00989A60, 40),
        WriteFnPart(pe, store, family, "SetVSConstantF 4float", 0x00989B00, 40),
        WriteFnPart(pe, store, family, "Wrapper device attach", 0x0098AD45, 40),
        WriteFnPart(pe, store, family, "Inner VS object ctor", 0x0098D4A0, 40),
        WriteFnPart(pe, store, family, "Engine stores 1436E14", 0x00B264E4, 40),
        WriteFnPart(pe, store, family, "State flush SetRenderState", 0x00A044E0, 50),
        WriteFnPart(pe, store, family, "CULLMODE slot init", 0x00A047A7, 20),
        WriteFnPart(pe, store, family, "First-seen apply CCW cull", 0x00B24BF2, 40),
        WriteFnPart(pe, store, family, "Static-lit apply CCW cull", 0x00BB2DA2, 30),
        WriteU32Part(pe, store, family, "CULL table 01396FB0", 0x01396FB0, 4),
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
        WriteWalkPart(pe, store, family, "Patch frustum AABB 00BDC2D0", 0x00BDC2D0, 100),
        WriteWalkPart(pe, store, family, "Patch AABB fill 00BF6F80", 0x00BF6F80, 150),
        WriteWalkPart(pe, store, family, "Patch AABB setup 00BDC180", 0x00BDC180, 150),
        WriteWalkPart(pe, store, family, "Tessellator ctor 00BF6E20", 0x00BF6E20, 80),
        WriteWalkPart(pe, store, family, "Frustum extract 00B2FD60", 0x00B2FD60, 500),
        WriteWalkPart(pe, store, family, "Camera setup FOV inverse 00B30B50", 0x00B30B50, 600),
        WriteWalkPart(pe, store, family, "Camera update helper FOV 00B314E0", 0x00B314E0, 200),
        WriteWalkPart(pe, store, family, "Camera spline update 00B31160", 0x00B31160, 250),
        WriteWalkPart(pe, store, family, "Camera ctor 00B31700", 0x00B31700, 80),
        WriteFnPart(pe, store, family, "Spline enable +536 00B2FC10", 0x00B2FC10, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "FOV flag getter 00A0BE80", 0x00A0BE80, 4, stopOnRet: false),
        WriteFnPart(pe, store, family, "FOV H getter 00A0BE90", 0x00A0BE90, 4, stopOnRet: false),
        WriteFnPart(pe, store, family, "FOV V getter 00A0BEA0", 0x00A0BEA0, 4, stopOnRet: false),
        WriteU32Part(pe, store, family, "FOV convert 360 1238020", 0x01238020, 1),
        WriteU32Part(pe, store, family, "FOV convert 1/360 1238E00", 0x01238E00, 1),
        WriteU32Part(pe, store, family, "FOV convert 2pi 128F608", 0x0128F608, 1),
        WriteWalkPart(pe, store, family, "Frustum extract other 00B2FC50", 0x00B2FC50, 80),
        WriteWalkPart(pe, store, family, "Camera copy 00B4AF50", 0x00B4AF50, 50),
        WriteWalkPart(pe, store, family, "Frustum plane store 00A42140", 0x00A42140, 20),
        WriteWalkPart(pe, store, family, "Frustum normalize 00A14440", 0x00A14440, 30),
        WriteU32Part(pe, store, family, "FOV half scale 122F59C", 0x0122F59C, 1),
        WriteU32Part(pe, store, family, "Letterbox 4by3 1238174", 0x01238174, 1),
        WriteFnPart(pe, store, family, "Static Flag1 pass2 00BA3637", 0x00BA23C5, 30, stopOnRet: false),
        WriteU32Part(pe, store, family, "Frustum compare zero 122DEDC", 0x0122DEDC, 1),
        WriteFnPart(pe, store, family, "Unbind stages 0/1/2", 0x00B67510, 40),
        WriteFnPart(pe, store, family, "Water draw vtbl+16", 0x00B783F0, 160),
        WriteFnPart(pe, store, family, "Water draw full", 0x00B783F0, 280, stopOnRet: false),
        WriteFnPart(pe, store, family, "Sky draw vtbl+16", 0x00B662F0, 80),
        WriteFnPart(pe, store, family, "MainScene plus616 draw", 0x00B33010, 120),
        WriteFnPart(pe, store, family, "Static mesh VS bind", 0x00B8B660, 80),
        WriteFnPart(pe, store, family, "VS bind LANDSCAPE FOREGROUND", 0x00B69330, 80),
        WriteFnPart(pe, store, family, "Landscape VS family ctor", 0x00B69000, 400, stopOnRet: false),
        WriteFnPart(pe, store, family, "Landscape VS slot table 00B6CBD0", 0x00B6CBD0, 80),
        WriteFnPart(pe, store, family, "FG compact bind 00B68DA0", 0x00B68DA0, 200),
        WriteFnPart(pe, store, family, "Static 2POINTLIGHTS family 00BB5040", 0x00BB5040, 280, stopOnRet: false),
        WriteFnPart(pe, store, family, "Land layer select 00BE6F70", 0x00BE6F70, 120),
        WriteFnPart(pe, store, family, "Layer bind 00BE7BE0", 0x00BE7BE0, 160),
        WriteFnPart(pe, store, family, "Lighting mode setter 00B23C00", 0x00B23C00, 20),
        WriteFnPart(pe, store, family, "SetVertexShader wrapper 00988020", 0x00988020, 50),
        WriteFnPart(pe, store, family, "Static count-to-slot 00BA2677", 0x00BA2677, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "Gather body after enable 00B462B4", 0x00B462B4, 160, stopOnRet: false),
        WriteFnPart(pe, store, family, "Add light message 16 handler", 0x00B481E0, 30),
        WriteFnPart(pe, store, family, "Add light 00B480E0", 0x00B480E0, 80),
        WriteFnPart(pe, store, family, "Collect lights 00B47BC0", 0x00B47BC0, 80),
        WriteFnPart(pe, store, family, "Grid rebuild 00B46660", 0x00B46660, 80),
        WriteFnPart(pe, store, family, "MainScene construct static family", 0x00B34619, 30),
        WriteU32Part(pe, store, family, "Count-to-slot jump table", 0x00BA48A8, 8),
        WriteFnPart(pe, store, family, "Water type-8 reject 00B6D6E0", 0x00B6D6E0, 20),
        WriteFnPart(pe, store, family, "Water type-8 accept 00B6D6F4", 0x00B6D6F4, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "Water ingest 00B6DAF0", 0x00B6DAF0, 40),
        WriteFnPart(pe, store, family, "Water draw empty je 00B7851D", 0x00B78513, 20, stopOnRet: false),
        WriteWalkPart(pe, store, family, "Water ctor zeros 00B73760", 0x00B73760, 400),
        WriteWalkPart(pe, store, family, "Water type-8 ingest 00B6DAF0", 0x00B6DAF0, 80),
        WriteWalkPart(pe, store, family, "Water type-8 copy 00B6D6E0", 0x00B6D6E0, 80),
        WriteWalkPart(pe, store, family, "Sea bind 00B6DC40", 0x00B6DC40, 300),
        WriteWalkPart(pe, store, family, "Water rebuild vtbl1 00B71FB0", 0x00B71FB0, 200),
        WriteWalkPart(pe, store, family, "NString ctor zeros 0099E4B0", 0x0099E4B0, 10),
        WriteFnPart(pe, store, family, "Water +636 setter 00B23F00", 0x00B23F00, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "Water draw empty check 00B783F0", 0x00B783F0, 80, stopOnRet: false),
        WriteWalkPart(pe, store, family, "LoadWaterData full 00B41FA0", 0x00B41FA0, 120),
        WriteWalkPart(pe, store, family, "SetStaticMapFileForUse 00B428E0", 0x00B428E0, 80),
        WriteFnPart(pe, store, family, "Water draw empty ret 00B7A865", 0x00B7A865, 8, stopOnRet: false),
        WriteFnPart(pe, store, family, "Water vtbl+8 ret4 00B6D500", 0x00B6D500, 4, stopOnRet: false),
        WriteWalkPart(pe, store, family, "Sea stream wrap 009D6100", 0x009D6100, 120),
        WriteWalkPart(pe, store, family, "Sea stream ctor 009D5DF0", 0x009D5DF0, 80),
        WriteU32Part(pe, store, family, "Layer type byte table", 0x00BBC2EC, 6),
        WriteU32Part(pe, store, family, "Layer type jump table", 0x00BBC2D8, 5),
        WriteFnPart(pe, store, family, "PALSKIN +8 enable 00BD71B0", 0x00BD71B0, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "PALSKIN SRCALPHA 00BD3867", 0x00BD3867, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "Blend slot init 00A047D2", 0x00A047D2, 40, stopOnRet: false),
        WriteU32Part(pe, store, family, "Blend table 01396F78", 0x01396F78, 2),
        WriteFnPart(pe, store, family, "Type20 NONE-draw case 00BBC1DB", 0x00BBC1DB, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "Static submit calls switch 00BBCB59", 0x00BBCB4B, 20, stopOnRet: false),
        WriteVtblPart(pe, store, family, "Static primitive vtbl 012A5B80", 0x012A5B80, 24),
        WriteFnPart(pe, store, family, "Water +636 this-setter 00B23900", 0x00B23900, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "Water +636 dtor 00B71994", 0x00B71994, 40, stopOnRet: false),
        WriteVtblPart(pe, store, family, "Engine +636 setter table", 0x012A1000, 20),
        WriteWalkPart(pe, store, family, "Sea mesh copy 00B6D420", 0x00B6D420, 40),
        WriteWalkPart(pe, store, family, "Sea mesh builder 00BE91E0", 0x00BE91E0, 300),
        WriteWalkPart(pe, store, family, "Sea bind full 00B6DC40", 0x00B6DC40, 250),
        WriteWalkPart(pe, store, family, "C3D material serialize 00ABF6B0", 0x00ABF6B0, 120),
        WriteWalkPart(pe, store, family, "C3D mesh serialize 00A89450", 0x00A89450, 160),
        WriteFnPart(pe, store, family, "PALSKIN Flag1 mask 00BD76D2", 0x00BD76CC, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "PALSKIN type index tail 00BD77C2", 0x00BD77C2, 40, stopOnRet: false),
        WriteWalkPart(pe, store, family, "TOD blend 00B46C80", 0x00B46C80, 200),
        WriteFnPart(pe, store, family, "TOD zero copy 00B46E17", 0x00B46E17, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "Static Flag1 skip 00BA3637", 0x00BA3637, 30, stopOnRet: false),
        WriteWalkPart(pe, store, family, "PALSKIN helper ctor 00BCE740", 0x00BCE740, 20),
        WriteWalkPart(pe, store, family, "prim queue drain 00B849F0", 0x00B849F0, 120),
        WriteWalkPart(pe, store, family, "MainScene layer drain 00B33010", 0x00B33010, 200),
        WriteFnPart(pe, store, family, "Instance opacity +39 00B991F5", 0x00B991F5, 10, stopOnRet: false),
        WriteFnPart(pe, store, family, "PALSKIN queue slots 00BD7838", 0x00BD780D, 80, stopOnRet: false),
        WriteU32Part(pe, store, family, "PALSKIN helper vtbl name", 0x012A6C5C, 8),
        WriteVtblPart(pe, store, family, "PALSKIN renderer vtbl 012A78DC", 0x012A78DC, 16),
        WriteWalkPart(pe, store, family, "PALSKIN drain vtbl20 00BD7110", 0x00BD7110, 80),
        WriteWalkPart(pe, store, family, "PALSKIN drain vtbl24 00B91340", 0x00B91340, 20),
        WriteWalkPart(pe, store, family, "PALSKIN debug unwrap 00B91140", 0x00B91140, 200),
        WriteFnPart(pe, store, family, "PALSKIN pass cmp 00BD3AAF", 0x00BD3AAF, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "PALSKIN type4 case 00BD3C04", 0x00BD3C04, 40, stopOnRet: false),
        WriteVtblPart(pe, store, family, "CEngineWaterRenderer vtbl", 0x012A3364, 16),
        WriteFnPart(pe, store, family, "Water ctor vector zero 00B7397F", 0x00B7397F, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "Missing water intern ret 00B420E4", 0x00B420E4, 8, stopOnRet: false),
        WriteWalkPart(pe, store, family, "Water draw second empty 00B72180", 0x00B72180, 80),
        WriteWalkPart(pe, store, family, "c35 setter default 0098B2C0", 0x0098B2C0, 80),
        WriteFnPart(pe, store, family, "PALSKIN upload offset 8 00BAB312", 0x00BAB300, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "PALSKIN upload offset 1 00BBFFD1", 0x00BBFFC7, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "PALSKIN family ctor 00BD01B8", 0x00BD01B8, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "PALSKIN count-to-slot 00BD3C36", 0x00BD3C36, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "PALSKIN offset0 c38 00BD4591", 0x00BD456F, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "Attach LayoutLights layout 2", 0x00B3CDB5, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "Light flush uses inner+84", 0x0098A5B3, 20, stopOnRet: false),
        WriteWalkPart(pe, store, family, "PALSKIN draw entry 00BD71B0", 0x00BD71B0, 2000),
        WriteWalkPart(pe, store, family, "PALSKIN helper 00BD7110", 0x00BD7110, 400),
        WriteWalkPart(pe, store, family, "PALSKIN bone pack 00BD2D90", 0x00BD2D90, 400),
        WriteWalkPart(pe, store, family, "PALSKIN bind switch 00BD3070", 0x00BD3070, 4000),
        WriteFnPart(pe, store, family, "PALSKIN first-seen tail 00BD3E17", 0x00BD3E17, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "PALSKIN common tail 00BD4DA6", 0x00BD4DA6, 80, stopOnRet: false),
        WriteU32Part(pe, store, family, "PALSKIN pass2 jump table", 0x00BD5C40, 18),
        WriteWalkPart(pe, store, family, "PALSKIN vtbl20 wrapper 00BD7A00", 0x00BD7A00, 40),
        WriteWalkPart(pe, store, family, "PALSKIN subset ctor 00BD7A80", 0x00BD7A80, 30),
        WriteFnPart(pe, store, family, "MainScene pass-4 helper 00B32E90", 0x00B32E90, 40),
        WriteFnPart(pe, store, family, "Slot dispatch 00B324A0", 0x00B324A0, 50),
        WriteFnPart(pe, store, family, "PALSKIN default bone draw 00BCFB00", 0x00BCFB00, 80, stopOnRet: false),
        WriteWalkPart(pe, store, family, "SetVSConstantF N 0098B930", 0x0098B930, 20),
        WriteWalkPart(pe, store, family, "Derived layout ctor 00BDB260", 0x00BDB260, 40),
        WriteWalkPart(pe, store, family, "C3D 60-byte bone getter 00A4BD70", 0x00A4BD70, 10),
        WriteFnPart(pe, store, family, "C3D bone block serialize 00A894ED", 0x00A894ED, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "C3D bone 60 48 64 sizes 00A89519", 0x00A89519, 30, stopOnRet: false),
        WriteWalkPart(pe, store, family, "Hierarchy local to world 00A9E1E0", 0x00A9E1E0, 80),
        WriteFnPart(pe, store, family, "Pose mul dest=S*C3D 00BD2F91", 0x00BD2F91, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "PALSKIN influence copy 00BCFB50", 0x00BCFB50, 20, stopOnRet: false),
        WriteWalkPart(pe, store, family, "Slot table ctor 00B8FAA0", 0x00B8FAA0, 80),
        WriteWalkPart(pe, store, family, "Slot table register 00B8FAD0", 0x00B8FAD0, 80),
        WriteWalkPart(pe, store, family, "Slot 33 getter 00B9CED0", 0x00B9CED0, 20),
        WriteWalkPart(pe, store, family, "Slot 33 AABB test 00B9CEE0", 0x00B9CEE0, 80),
        WriteFnPart(pe, store, family, "Lighting slot list 15 16", 0x00B48220, 40),
        WriteFnPart(pe, store, family, "Static slot list 36", 0x00BA7770, 20),
        WriteFnPart(pe, store, family, "PALSKIN slot list 37 38", 0x00BD28A0, 40),
        WriteNewGameMap(pe, store, family),
    };
    store.WriteIndex(
        family, DumpStore.NewGameTraceVersion, "newgame-trace",
        "Click New through first-seen StartOakVale: UI, quest, kid, NewRegion, static maps, tiles, draw.",
        links);
    Console.WriteLine($"trace  {family}/  parts={links.Count}  v{DumpStore.NewGameTraceVersion}");
}

static void RunMapNewGame(PeImage pe, DumpStore store)
{
    const string family = "newgame-trace";
    var links = new List<IndexLink> { WriteNewGameMap(pe, store, family) };
    store.WriteIndex(
        family, DumpStore.NewGameTraceVersion, "newgame-trace",
        "New Game / StartOakVale function map only (no other towns).",
        links);
}

static IndexLink WriteNewGameMap(PeImage pe, DumpStore store, string family)
{
    var nodes = FunctionMap.WalkNewGame(pe);
    store.WritePart(family, "fnmap", FunctionMap.ToMarkdown(nodes));
    store.WritePart(family, "fnmap-tsv", "```\n" + FunctionMap.ToTsv(nodes) + "```\n");
    Console.WriteLine($"map    newgame functions={nodes.Count}");
    return new IndexLink("fnmap", "New Game function map", 0);
}

static IndexLink WriteWalkPart(PeImage pe, DumpStore store, string family, string name, uint va, int n = 2000)
{
    var slug = DumpStore.Slug(name, va);
    var sb = new StringBuilder();
    var file = pe.FileOffset(va);
    var startVa = va;
    var startFile = file;
    if (file >= 0)
    {
        startFile = X86.FindPrologue(pe, file);
        startVa = pe.Va(startFile);
    }

    sb.AppendLine($"# {name}");
    sb.AppendLine();
    if (startFile < 0)
        sb.AppendLine("UNREAD (VA not mapped)");
    else
    {
        var steps = X86.WalkFunction(pe, startFile, n);
        sb.AppendLine($"VA `0x{startVa:X8}` · `{steps.Count}` insns (walk to next prologue). [INDEX](INDEX.md)");
        sb.AppendLine();
        var calls = new List<uint>();
        foreach (var step in steps)
        {
            sb.AppendLine($"  //{step.Va:X8}: {step.Text}");
            if (step.DirectCall is { } dest)
                calls.Add(dest);
        }

        if (calls.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Direct calls");
            sb.AppendLine();
            foreach (var c in calls.Distinct())
                sb.AppendLine($"- `{c:X8}`");
        }

        if (steps.Count >= n)
        {
            sb.AppendLine();
            sb.AppendLine($"truncated at {n} insns");
        }
    }

    store.WritePart(family, slug, sb.ToString());
    return new IndexLink(slug, name, startVa);
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

static IndexLink WriteU32Part(PeImage pe, DumpStore store, string family, string name, uint va, int n)
{
    var slug = DumpStore.Slug(name, va);
    var sb = new StringBuilder();
    sb.AppendLine($"# {name}");
    sb.AppendLine();
    sb.AppendLine($"VA `0x{va:X8}` · `{n}` u32. [INDEX](INDEX.md)");
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
            var value = BitConverter.ToUInt32(pe.Data, off);
            sb.AppendLine($"0x{va + (uint)(i * 4):X8}  {value}  0x{value:X}");
        }
    }

    store.WritePart(family, slug, sb.ToString());
    return new IndexLink(slug, name, va);
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
