using System.Text;
using Fable.Core;
using Fable.ExeIndex;
using Fable.Formats.Banks;
using Fable.Formats.Defs;
using Fable.Formats.Qst;
using Fable.Formats.Shaders;

var cmd = args.FirstOrDefault(a => a is "index" or "split" or "translate" or "all" or "disasm" or "fn" or "trace-render" or "trace-landscape" or "trace-newgame" or "trace-script" or "export-scripts" or "trace-shaders" or "map-newgame" or "calls" or "imm" or "vtbl" or "disp" or "scanff" or "floats" or "calldisp" or "scan" or "datascan") ?? "all";
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
        RunTraceScriptRuntime(pe, store);
        RunTraceShaders(pe, store);
        break;
    case "trace-script":
        if (!File.Exists(Path.Combine(outDir, "00-index", "xrefs.tsv")))
            RunIndex(pe, store);
        RunTraceScriptRuntime(pe, store);
        RunExportScriptBank(pe, store, install);
        break;
    case "export-scripts":
        if (!File.Exists(Path.Combine(outDir, "00-index", "xrefs.tsv")))
            RunIndex(pe, store);
        RunExportScriptBank(pe, store, install);
        break;
    case "trace-shaders":
        RunTraceShaders(pe, store);
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
        RunScan(pe, args, codeOnly: true);
        break;
    case "datascan":
        RunScan(pe, args, codeOnly: false);
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

static void RunScan(PeImage pe, string[] args, bool codeOnly = true)
{
    var hex = args.SkipWhile(a => a is "scan" or "datascan").FirstOrDefault();
    if (hex is null || hex.Length < 2 || hex.Length % 2 != 0)
    {
        Console.Error.WriteLine("usage: scan|datascan <hex-bytes> [lo hi]");
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
        if (codeOnly && !pe.InCode((int)sec.FileOffset))
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

static void RunExportScriptBank(PeImage pe, DumpStore store, GameInstall? install)
{
    const string family = "script-bank";
    if (!store.ShouldWrite(family, DumpStore.ScriptBankVersion))
    {
        Console.WriteLine($"skip  {family}  v{DumpStore.ScriptBankVersion} (exe unchanged)");
        return;
    }

    if (install is null)
    {
        Console.Error.WriteLine("script-bank: no TLC install, skip");
        return;
    }

    var namesPath = install.FindCompiledDef("names.bin");
    var scriptPath = install.FindCompiledDef("script.bin");
    if (namesPath is null || scriptPath is null)
    {
        Console.Error.WriteLine("script-bank: names.bin/script.bin missing");
        return;
    }

    var names = NamesBin.Load(namesPath);
    var script = GameBin.Load(scriptPath, names);
    var links = new List<IndexLink>();
    var tsv = new StringBuilder();
    tsv.AppendLine("index\ttype\tinstance\traw\tstrings\tnewgame");
    var newGame = new StringBuilder();
    newGame.AppendLine("# New Game script.bin entries");
    newGame.AppendLine();
    newGame.AppendLine("S_QNOVI is **not** in this bank. First-seen is the native quest object.");
    newGame.AppendLine();

    foreach (var entry in script.Entries)
    {
        var type = entry.TypeName ?? "";
        var inst = entry.InstanceName ?? "";
        var strings = ExtractAscii(entry.Raw);
        var isNew = IsNewGameScript(type, inst, strings);
        tsv.Append(entry.Index).Append('\t').Append(type).Append('\t').Append(inst)
            .Append('\t').Append(entry.Raw.Length).Append('\t')
            .Append(string.Join("|", strings.Take(8))).Append('\t')
            .Append(isNew ? "1" : "0").AppendLine();
        if (!isNew && entry.TypeName == "CCutsceneDef")
            continue;
        if (!isNew && script.Entries.Count > 0 && !inst.StartsWith("S_Q", StringComparison.Ordinal))
            continue;

        var slug = DumpStore.Slug($"{entry.Index:D4}-{inst}", 0);
        var body = new StringBuilder();
        body.AppendLine($"# {inst}");
        body.AppendLine();
        body.AppendLine($"type `{type}` · index **{entry.Index}** · raw **{entry.Raw.Length}** · newgame **{isNew}**.");
        body.AppendLine();
        if (entry.SubDefs.Count > 0)
        {
            body.AppendLine("subdefs:");
            foreach (var sub in entry.SubDefs)
                body.AppendLine($"- `{sub.DefIndex}`");
            body.AppendLine();
        }

        if (strings.Count > 0)
        {
            body.AppendLine("strings:");
            foreach (var s in strings)
                body.AppendLine($"- `{s}`");
            body.AppendLine();
        }

        body.AppendLine("```");
        var n = Math.Min(entry.Raw.Length, 96);
        for (var i = 0; i < n; i += 16)
        {
            body.Append($"{i:X4}  ");
            for (var j = 0; j < 16 && i + j < n; j++)
                body.Append($"{entry.Raw[i + j]:X2} ");
            body.AppendLine();
        }

        body.AppendLine("```");
        store.WritePart(family, slug, body.ToString());
        links.Add(new IndexLink(slug, inst.Length == 0 ? type : inst, 0));
        if (isNew)
            newGame.AppendLine($"- [{inst}]({slug}.md) `{type}` raw {entry.Raw.Length}");
    }

    store.WritePart(family, "entries-tsv", "```\n" + tsv + "```\n");
    links.Insert(0, new IndexLink("entries-tsv", "script.bin TSV", 0));
    store.WritePart(family, "newgame", newGame.ToString());
    links.Insert(1, new IndexLink("newgame", "New Game script.bin", 0));

    var qst = QuestFile.Load(install.QuestPath);
    var qstMd = new StringBuilder();
    qstMd.AppendLine("# FinalAlbion.qst");
    qstMd.AppendLine();
    foreach (var q in qst.Quests)
        qstMd.AppendLine($"- `{q.Name}` persistent **{q.Persistent}**");
    store.WritePart(family, "quests-qst", qstMd.ToString());
    links.Insert(2, new IndexLink("quests-qst", "FinalAlbion.qst", 0));

    var cmdMd = new StringBuilder();
    cmdMd.AppendLine("# Exe script command strings");
    cmdMd.AppendLine();
    cmdMd.AppendLine("ASCII in `0x012C1500`–`0x012C2C00` (dispatcher tokens).");
    cmdMd.AppendLine();
    foreach (var (va, text) in ExeAscii(pe, 0x012C1500, 0x012C2C00))
        cmdMd.AppendLine($"- `0x{va:X8}` `{text}`");
    store.WritePart(family, "exe-commands", cmdMd.ToString());
    links.Insert(3, new IndexLink("exe-commands", "exe command tokens", 0));

    var native = new StringBuilder();
    native.AppendLine("# Native S_QNOVI");
    native.AppendLine();
    native.AppendLine("Not a script.bin entry. Factory `00DBEF70` / ctor `00DAAC00` / vtbl `0x12D7A28`.");
    native.AppendLine();
    native.AppendLine("| step | VA | what |");
    native.AppendLine("|---|---|---|");
    native.AppendLine("| update | `00A44880` | microthread pump; dt via `009E1BC0` into `+8` |");
    native.AppendLine("| fiber | `00A446A0` | `[vtbl+16]` then loop `[vtbl+8]` until `+5` |");
    native.AppendLine("| persist AttackOver | `00DAADA0` | `004045C0(\"AttackOver\", this+80)` |");
    native.AppendLine("| run | `00DABAC0` → `00DBDE40` | native first-seen body |");
    native.AppendLine("| yield | `[ctx+28]` / `00A44690` | `009D8650` fiber switch |");
    native.AppendLine("| wait 12s | `[ctx+2584](12.0)` | after `[ctx+2592](1,&+76)` |");
    native.AppendLine("| gate | `+80` | persist name **AttackOver**; writer still UNREAD |");
    native.AppendLine("| cutscene start | `00DB86B0` | pushes `CS_OAKVALE_INTRO_FATHER` into `00CBFB7D`; xref `00DB88DE` is here, not dtor `00DB8680` |");
    native.AppendLine("| cutscene runner | `00CBFB7D` | CCutsceneDef interpreter; special-cases `FadeOut 0.5,0` then `00CBF29F` preload |");
    native.AppendLine("| UseCamera preload | `00CBF29F` | collects `UseCamera` / `CameraLookAt` names → `vtbl+1648` |");
    native.AppendLine("| UseCamera activate | `00CC9F3A` | lookup TNG name; bind `vtbl+1656` (thing) or `vtbl+1648` (name) |");
    native.AppendLine("| first-seen start | `NOVI_LiveFather` | `00DABAC0` registers name + factory `00DAC2C0` at `+16` (`0x012D8370`). TNG `CREATURE_HERO_FATHER` / `NOVI_LiveFather`. Construct `004C97B0` → `00CB8960` → `00DB8520` → `00DAC2C0` writes vtbl `0x012D8388`. Fiber `00DB8630` calls `[+52].vtbl+4` = `00DB86B0`. Names are registered before `00DBDE40` map-wait. |");
    native.AppendLine("| FadeOut special-case | `00CBFDD0` | `[ebp+120]!=1` (00DB86B0 pushes 0,0,0) compares first line to `FadeOut 0.5,0` then `vtbl+1488(0.5,0)`. First line is `PlayMusic` so the call is skipped. |");
    native.AppendLine("| PlayMusic | `00CC8EAC` / `00CBF7FE` | lookup `009E5120` then `vtbl+2784`. Jumps `00CD17FD` (no yield). |");
    native.AppendLine("| command loop | `00CD17FD` | `inc [ebp-72]` then `jb 00CC012E`. Next line is `FadeOut 0.5,0`. |");
    native.AppendLine("| FadeOut opcode | `00CD0987` | same-slice after PlayMusic. Parses 0.5 / 0 / default black. Apply `vtbl+1488(0.5,0)` then `jmp 00CD17FD`. |");
    native.AppendLine("| PlayAVI | `00CCA26D` | first arg required else `jmp 00CD17FD`. Prefix `Data\\Video\\` via `0099F570`, `vtbl+1476`, `jmp 00CD17F8` (dtor then `00CD17FD`). **No** `vtbl+28`. Apply body UNREAD. |");
    native.AppendLine("| MuteSounds | `00CC7258` | `00CBEE0C` IsFalse → `vtbl+2664(0)` else `(1)`. `jmp 00CC8464` (next token). **No** `vtbl+28`. First-seen `false` unmutes. Apply body UNREAD. |");
    native.AppendLine("| NoLoadUseCamera | `00CC9E6A` | separate token from `UseCamera`. |");
    native.AppendLine("| .Teleport | `00CC4678` | lookup marker `vtbl+280/+288`, apply `vtbl+1892`. Second arg `00CBEE0C` is **IsFalse**. **No** `vtbl+28`. `jmp 00CC707C`. |");
    native.AppendLine("| .LookToThing | `00CC3B3F` | apply `vtbl+1992`, parse `forever`. Third arg `00CBEE0C` (IsFalse) skips wait. Else if `[ebp+103]` (set **1** at `00CBFC65`) **`call [eax+28]`** then `00CBF7FE` / `jmp 00CC707C`. |");
    native.AppendLine("| actor join | `00CC707C` | dtor then next token `DoScriptFrame`. Teleport does not wait there. |");
    native.AppendLine("| DoScriptFrame | `00CC7085` | default count **1** (`xor esi; inc esi`). Arg via `0099E7F0` atoi. `esi<=0` skips. Loop: if `[ebp+103]` **`call [eax+28]`**, then `00CBF7FE`, `dec esi`. First-seen `[ebp+103]=1`. |");
    native.AppendLine("| DoCameraPreloading | `00CC86D0` | `vtbl+1564`, then if first arg `00CBEDBA` IsTrue: `vtbl+1560`(float, default 2.0). Else `00CBF29F`(dl=0) preload. Then `vtbl+1568`. **`jmp 00CD17FD`** (no yield). First-seen has no args. |");
    native.AppendLine("| IsFalse | `00CBEE0C` | strcmp arg to `false` via `00BFEBA8`. |");
    store.WritePart(family, "native-sqnovi", native.ToString());
    links.Insert(4, new IndexLink("native-sqnovi", "native S_QNOVI", 0));

    store.WriteIndex(
        family, DumpStore.ScriptBankVersion, "script-bank",
        "script.bin entries, QST names, exe command tokens, native S_QNOVI. out/ is gitignored.",
        links);
    Console.WriteLine($"trace  {family}/  parts={links.Count}  v{DumpStore.ScriptBankVersion}");
}

static bool IsNewGameScript(string type, string inst, IReadOnlyList<string> strings)
{
    foreach (var s in new[] { type, inst }.Concat(strings))
    {
        if (s.Contains("NOV", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("OakVale", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("Oakvale", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("Q_New", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("S_QNOVI", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("S_QHOH", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("OVIF", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("PreAttack", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("HerosOld", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("AttackOver", StringComparison.OrdinalIgnoreCase))
            return true;
    }

    return false;
}

static List<string> ExtractAscii(byte[] raw)
{
    var list = new List<string>();
    var i = 0;
    while (i < raw.Length)
    {
        if (raw[i] is < 32 or > 126)
        {
            i++;
            continue;
        }

        var start = i;
        while (i < raw.Length && raw[i] is >= 32 and <= 126)
            i++;
        if (i - start >= 4)
            list.Add(Encoding.ASCII.GetString(raw, start, i - start));
    }

    return list;
}

static List<(uint Va, string Text)> ExeAscii(PeImage pe, uint lo, uint hi)
{
    var list = new List<(uint, string)>();
    var data = pe.Data;
    foreach (var sec in pe.Sections)
    {
        var end = Math.Min(data.Length, (int)(sec.FileOffset + sec.FileSize));
        var i = (int)sec.FileOffset;
        while (i < end)
        {
            var va = pe.Va(i);
            if (va < lo || va > hi || data[i] is < 32 or > 126)
            {
                i++;
                continue;
            }

            var start = i;
            while (i < end && data[i] is >= 32 and <= 126)
                i++;
            if (i - start >= 4)
                list.Add((pe.Va(start), Encoding.ASCII.GetString(data, start, i - start)));
        }
    }

    return list;
}

static void RunTraceScriptRuntime(PeImage pe, DumpStore store)
{
    const string family = "script-runtime";
    if (!store.ShouldWrite(family, DumpStore.ScriptRuntimeVersion))
    {
        Console.WriteLine($"skip  {family}  v{DumpStore.ScriptRuntimeVersion} (exe unchanged)");
        return;
    }

    var links = new List<IndexLink>
    {
        WriteWalkPart(pe, store, family, "Registering Scripts 00CB5D80", 0x00CB5D80, 80),
        WriteWalkPart(pe, store, family, "quest table 00CD52D0", 0x00CD52D0, 80),
        WriteWalkPart(pe, store, family, "script def 00F2A0F0", 0x00F2A0F0, 80),
        WriteWalkPart(pe, store, family, "script bind 00CB5C90", 0x00CB5C90, 120),
        WriteWalkPart(pe, store, family, "script alias 00CB5AC0", 0x00CB5AC0, 40),
        WriteWalkPart(pe, store, family, "start scripts 00CB7780", 0x00CB7780, 200),
        WriteWalkPart(pe, store, family, "script invoke 00CB70E0", 0x00CB70E0, 200),
        WriteWalkPart(pe, store, family, "script walk 00CB6EA0", 0x00CB6EA0, 200),
        WriteWalkPart(pe, store, family, "script per-item 00CB6CE0", 0x00CB6CE0, 200),
        WriteWalkPart(pe, store, family, "script start item 00CB62F0", 0x00CB62F0, 200),
        WriteWalkPart(pe, store, family, "script start item 00CB6420", 0x00CB6420, 200),
        WriteCallsPart(pe, store, family, "calls script start 00CB62F0", 0x00CB62F0),
        WriteCallsPart(pe, store, family, "calls script start 00CB6420", 0x00CB6420),
        WriteWalkPart(pe, store, family, "script per-item 00CB6B00", 0x00CB6B00, 120),
        WriteCallsPart(pe, store, family, "calls script per-item 00CB6CE0", 0x00CB6CE0),
        WriteWalkPart(pe, store, family, "script walk tail 00CB6860", 0x00CB6860, 120),
        WriteCallsPart(pe, store, family, "calls script walk 00CB6EA0", 0x00CB6EA0),
        WriteWalkPart(pe, store, family, "script store factory 00CB7210", 0x00CB7210, 80),
        WriteWalkPart(pe, store, family, "script partition 00CB7310", 0x00CB7310, 80),
        WriteCallsPart(pe, store, family, "calls script invoke 00CB70E0", 0x00CB70E0),
        WriteWalkPart(pe, store, family, "quest base slot2 00CBD4C0", 0x00CBD4C0, 40),
        WriteWalkPart(pe, store, family, "quest base slot0 00CBD4F0", 0x00CBD4F0, 40),
        WriteWalkPart(pe, store, family, "quest base slot1 00CBD4B0", 0x00CBD4B0, 40),
        WriteWalkPart(pe, store, family, "quest base slot3 00CBD4D0", 0x00CBD4D0, 40),
        WriteWalkPart(pe, store, family, "quest base slot4 00CBD4E0", 0x00CBD4E0, 40),
        WriteWalkPart(pe, store, family, "microthread create 00A447D0", 0x00A447D0, 80),
        WriteWalkPart(pe, store, family, "microthread 00A44840", 0x00A44840, 80),
        WriteWalkPart(pe, store, family, "microthread update 00A44880", 0x00A44880, 200),
        WriteWalkPart(pe, store, family, "microthread fiber entry 00A446A0", 0x00A446A0, 80),
        WriteWalkPart(pe, store, family, "microthread resume 00A44660", 0x00A44660, 40),
        WriteWalkPart(pe, store, family, "microthread has-work 00A44930", 0x00A44930, 40),
        WriteWalkPart(pe, store, family, "frame dt 009E1BC0", 0x009E1BC0, 30),
        WriteWalkPart(pe, store, family, "microthread yield 00A44690", 0x00A44690, 40),
        WriteCallsPart(pe, store, family, "calls microthread resume 00A44660", 0x00A44660),
        WriteCallsPart(pe, store, family, "calls microthread fiber 00A446A0", 0x00A446A0),
        WriteCallDispPart(pe, store, family, "calldisp vtbl+24 update quests", 0x18, 0x00CB0000, 0x00CC0000),
        WriteCallDispPart(pe, store, family, "calldisp vtbl+24 update mgr", 0x18, 0x00A44000, 0x00A46000),
        WriteWalkPart(pe, store, family, "microthread ctor 00A44740", 0x00A44740, 80),
        WriteCallsPart(pe, store, family, "calls microthread update 00A44880", 0x00A44880),
        WriteCallsPart(pe, store, family, "calls microthread 00A44840", 0x00A44840),
        WriteCallsPart(pe, store, family, "calls microthread create 00A447D0", 0x00A447D0),
        WriteVtblPart(pe, store, family, "watcher vtbl 012D7A3C", 0x012D7A3C, 16),
        WriteWalkPart(pe, store, family, "S_QNOVI slot4 00DAADA0", 0x00DAADA0, 40),
        WriteWalkPart(pe, store, family, "AttackOver persist 004045C0", 0x004045C0, 80),
        WriteCallsPart(pe, store, family, "calls AttackOver persist 004045C0", 0x004045C0),
        WriteWalkPart(pe, store, family, "S_QNOVI slot5 00DAAD80", 0x00DAAD80, 40),
        WriteWalkPart(pe, store, family, "S_QNOVI slot9 00DAAD70", 0x00DAAD70, 20),
        WriteWalkPart(pe, store, family, "S_QNOVI slot10 00CDD410", 0x00CDD410, 20),
        WriteWalkPart(pe, store, family, "S_QNOVI slot11 00CDD420", 0x00CDD420, 20),
        WriteFnPart(pe, store, family, "script global first use 00CBE0C2", 0x00CBE0C0, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "+80 setter al 00CFAE04", 0x00CFAE00, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "+80 setter al 00D037D4", 0x00D037D0, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "+80 imm1 00D11781", 0x00D11780, 16, stopOnRet: false),
        WriteWalkPart(pe, store, family, "quest base ctor 00CB8110", 0x00CB8110, 40),
        WriteVtblPart(pe, store, family, "quest base vtbl 012C1648", 0x012C1648, 16),
        WriteWalkPart(pe, store, family, "S_QNOVI factory 00DBEF70", 0x00DBEF70, 20),
        WriteWalkPart(pe, store, family, "S_QNOVI ctor 00DAAC00", 0x00DAAC00, 80),
        WriteVtblPart(pe, store, family, "S_QNOVI vtbl 012D7A28", 0x012D7A28, 16),
        WriteWalkPart(pe, store, family, "S_QNOVI slot0 dtor 00DBEFA0", 0x00DBEFA0, 20),
        WriteWalkPart(pe, store, family, "S_QNOVI slot1 Main 00DAACE0", 0x00DAACE0, 40),
        WriteWalkPart(pe, store, family, "S_QNOVI slot2 run 00DABAC0", 0x00DABAC0, 80),
        WriteFnPart(pe, store, family, "S_QNOVI slot2 calls setup 00DAC293", 0x00DAC293, 16, stopOnRet: false),
        WriteWalkPart(pe, store, family, "S_QNOVI slot3 reset 00DAADD0", 0x00DAADD0, 80),
        WriteWalkPart(pe, store, family, "StartOakVale 00DBDE40", 0x00DBDE40, 200),
        WriteFnPart(pe, store, family, "PreAttack wait setup 00DBE128", 0x00DBE128, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "PreAttack +80 spin 00DBE1FA", 0x00DBE1FA, 40, stopOnRet: false),
        WriteWalkPart(pe, store, family, "hero-exists 00CB7940", 0x00CB7940, 20),
        WriteWalkPart(pe, store, family, "watcher ctor 00CDD450", 0x00CDD450, 40),
        WriteWalkPart(pe, store, family, "watcher register 00CB7E50", 0x00CB7E50, 80),
        WriteWalkPart(pe, store, family, "WatchBarrels 00DBE890", 0x00DBE890, 80),
        WriteWalkPart(pe, store, family, "script camera hooks 00CBF29F", 0x00CBF29F, 220),
        WriteWalkPart(pe, store, family, "cutscene runner 00CBFB7D", 0x00CBFB7D, 200),
        WriteFnPart(pe, store, family, "cutscene runner exact 00CBFB7D", 0x00CBFB7D, 280, stopOnRet: false),
        WriteFnPart(pe, store, family, "cutscene FadeOut 0.5 site 00CBFDD0", 0x00CBFDD0, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "cutscene arg120 00CBFD95", 0x00CBFD95, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "00DB86B0 calls runner 00DB88DB", 0x00DB88DB, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayAVI site 00CCA26E", 0x00CCA26E, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayAVI token 00CCA26D", 0x00CCA26D, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayAVI apply 00CCA2BD", 0x00CCA2BD, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "command continue join 00CD17F8", 0x00CD17F8, 12, stopOnRet: false),
        WriteWalkPart(pe, store, family, "CString concat 0099F570", 0x0099F570, 30),
        WriteCallDispPart(pe, store, family, "calldisp vtbl+1476 PlayAVI", 0x5C4, 0x00CCA280, 0x00CCA320),
        WriteFnPart(pe, store, family, "NoLoadUseCamera token 00CC9E69", 0x00CC9E69, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "NoLoadUseCamera yield 00CC9F28", 0x00CC9F28, 8, stopOnRet: false),
        WriteFnPart(pe, store, family, "NoLoadUseCamera yield helper 00CC907D", 0x00CC907D, 12, stopOnRet: false),
        WriteWalkPart(pe, store, family, "PlayMusic helper 00CBF7FE", 0x00CBF7FE, 120),
        WriteFnPart(pe, store, family, "PlayMusic helper site 00CBF8F4", 0x00CBF8F4, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayMusic interpreter 00CC8EAC", 0x00CC8EAC, 60, stopOnRet: false),
        WriteCallsPart(pe, store, family, "calls PlayMusic helper 00CBF7FE", 0x00CBF7FE),
        WriteFnPart(pe, store, family, "FadeOut opcode exact 00CD0987", 0x00CD0987, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "FadeOut after match 00CD09DF", 0x00CD09DF, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "FadeOut apply 00CD0AF0", 0x00CD0AF0, 60, stopOnRet: false),
        WriteFnPart(pe, store, family, "command loop continue 00CD17FD", 0x00CD17FD, 80, stopOnRet: false),
        WriteCallDispPart(pe, store, family, "calldisp vtbl+1488 fade runner", 0x5D0, 0x00CBFD00, 0x00CBFE50),
        WriteCallDispPart(pe, store, family, "calldisp vtbl+1496 FadeOut", 0x5D8, 0x00CD0980, 0x00CD0C00),
        WriteImmPart(pe, store, family, "imm fade 0.5 122F59C", 0x122F59C, 0x00CBFD00, 0x00CBFE50),
        WriteWalkPart(pe, store, family, "script interface ctor 006E7740", 0x006E7740, 40),
        WriteVtblPart(pe, store, family, "CGameScriptInterface vtbl+1488 012614DC", 0x012614DC, 4),
        WriteWalkPart(pe, store, family, "FadeOut vtbl+1488 008907E0", 0x008907E0, 20),
        WriteWalkPart(pe, store, family, "Fade vtbl+1492 00890820", 0x00890820, 30),
        WriteWalkPart(pe, store, family, "Fade forward 006E7370", 0x006E7370, 20),
        WriteWalkPart(pe, store, family, "Fade state write 00434C00", 0x00434C00, 40),
        WriteWalkPart(pe, store, family, "CS_OAKVALE_INTRO_FATHER start 00DB86B0", 0x00DB86B0, 200),
        WriteWalkPart(pe, store, family, "intro-father dtor 00DB8680", 0x00DB8680, 20),
        WriteWalkPart(pe, store, family, "intro-father persist 00DB8630", 0x00DB8630, 40),
        WriteWalkPart(pe, store, family, "NOVI_LiveFather factory 00DAC2C0", 0x00DAC2C0, 60),
        WriteWalkPart(pe, store, family, "NOVI name-record create 00DB8520", 0x00DB8520, 80),
        WriteWalkPart(pe, store, family, "NOVI name register 00CB8230", 0x00CB8230, 80),
        WriteWalkPart(pe, store, family, "NOVI name flush 00CB8930", 0x00CB8930, 40),
        WriteWalkPart(pe, store, family, "construct name bind 00CB8960", 0x00CB8960, 120),
        WriteWalkPart(pe, store, family, "activate name start 00CB88B0", 0x00CB88B0, 40),
        WriteWalkPart(pe, store, family, "thing construct bind 004C97B0", 0x004C97B0, 40),
        WriteWalkPart(pe, store, family, "thing script activate 004C7CF0", 0x004C7CF0, 30),
        WriteWalkPart(pe, store, family, "thing activate scripts 004AFB00", 0x004AFB00, 40),
        WriteWalkPart(pe, store, family, "thing construct scripts 004AFA60", 0x004AFA60, 40),
        WriteWalkPart(pe, store, family, "thing activate wrapper 00664370", 0x00664370, 30),
        WriteFnPart(pe, store, family, "UseCamera token 00CC9F39", 0x00CC9F39, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "UseCamera name bind 00CCA1AA", 0x00CCA1AA, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "UseCamera yield 00CCA22C", 0x00CCA22C, 16, stopOnRet: false),
        WriteFnPart(pe, store, family, "UseCamera ebp-37 ctor 00CBFD53", 0x00CBFD53, 8, stopOnRet: false),
        WriteVtblPart(pe, store, family, "NOVI name-record vtbl 012D8370", 0x012D8370, 4),
        WriteVtblPart(pe, store, family, "NOVI_LiveFather vtbl 012D8388", 0x012D8388, 8),
        WriteVtblPart(pe, store, family, "intro-father microthread 012D95B0", 0x012D95B0, 8),
        WriteU32Part(pe, store, family, "NOVI_LiveFather rdata 012D8370", 0x012D8370, 16),
        WriteCallsPart(pe, store, family, "calls cutscene runner 00CBFB7D", 0x00CBFB7D),
        WriteCallsPart(pe, store, family, "calls intro-father start 00DB86B0", 0x00DB86B0),
        WriteCallsPart(pe, store, family, "calls LiveFather factory 00DAC2C0", 0x00DAC2C0),
        WriteCallsPart(pe, store, family, "calls name-record create 00DB8520", 0x00DB8520),
        WriteCallsPart(pe, store, family, "calls thing script activate 004C7CF0", 0x004C7CF0),
        WriteCallsPart(pe, store, family, "calls thing construct bind 004C97B0", 0x004C97B0),
        WriteImmPart(pe, store, family, "imm name-record vtbl 012D8370", 0x012D8370, 0x00DABA00, 0x00DAC200),
        WriteImmPart(pe, store, family, "imm LiveFather vtbl 012D8388", 0x012D8388, 0x00DAC200, 0x00DAC400),
        WriteScanPart(pe, store, family, "scan LiveFather factory +16", "C74710C0C2DA00", 0x00DABB00, 0x00DABB20),
        WriteWalkPart(pe, store, family, "PlayAnimation splitter 00CBFACA", 0x00CBFACA, 40),
        WriteFnPart(pe, store, family, "PlayAnimation token 00CC14B8", 0x00CC14B8, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayAnimation apply 00CC1527", 0x00CC1527, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayAnimation yield-join 00CC186F", 0x00CC186F, 8, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayAnimation yield-once 00CC5691", 0x00CC5691, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayAnimation leftover 00CC0EBC", 0x00CC0EBC, 12, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayAnimation ebp-22 ctor 00CBFD57", 0x00CBFD57, 8, stopOnRet: false),
        WriteU32Part(pe, store, family, "PlayAnimation flag byte 01375748", 0x01375748, 1),
        WriteCallDispPart(pe, store, family, "calldisp vtbl+72 PlayAnimation", 0x48, 0x00CC14B8, 0x00CC186F),
        WriteWalkPart(pe, store, family, "PlayAnimation thing vtbl+72 004C7470", 0x004C7470, 40),
        WriteVtblPart(pe, store, family, "player thing vtbl 012457FC", 0x012457FC, 24),
        WriteVtblPart(pe, store, family, "CTCAnimationComplex vtbl 012650A4", 0x012650A4, 24),
        WriteWalkPart(pe, store, family, "CTCAnimationComplex factory 0070B3F0", 0x0070B3F0, 30),
        WriteFnPart(pe, store, family, "CTCAnimationComplex +68 stub 00686920", 0x00686920, 4, stopOnRet: false),
        WriteFnPart(pe, store, family, "CTCAnimationComplex type 90 0070B3C0", 0x0070B3C0, 4, stopOnRet: false),
        WriteFnPart(pe, store, family, "CTCAnimationComplex inner getter 0070B460", 0x0070B460, 4, stopOnRet: false),
        WriteWalkPart(pe, store, family, "CTCAnimationComplex inner play 0070D580", 0x0070D580, 80),
        WriteWalkPart(pe, store, family, "CTCAnimationComplex play request 0070C050", 0x0070C050, 30),
        WriteFnPart(pe, store, family, "CTCAnimationComplex post-attach 0070B600", 0x0070B600, 4, stopOnRet: false),
        WriteWalkPart(pe, store, family, "appearance DEFAULT play 005B37F7", 0x005B37F7, 80),
        WriteCallsPart(pe, store, family, "calls inner play 0070D580", 0x0070D580),
        WriteWalkPart(pe, store, family, "named component attach 004C9D60", 0x004C9D60, 80),
        WriteWalkPart(pe, store, family, "FadeIn FadeOut 00CC4B22", 0x00CC4B22, 80),
        WriteFnPart(pe, store, family, "StayFadedOut 00CD087E", 0x00CD087E, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "Teleport token 00CC4678", 0x00CC4678, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "Teleport apply 00CC47B4", 0x00CC47B4, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "LookToThing token 00CC3B3F", 0x00CC3B3F, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "LookToThing yield 00CC3C94", 0x00CC3C94, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "actor command join 00CC707C", 0x00CC707C, 50, stopOnRet: false),
        WriteWalkPart(pe, store, family, "IsFalse arg 00CBEE0C", 0x00CBEE0C, 40),
        WriteFnPart(pe, store, family, "runner ebp+103 yield-enable 00CBFC65", 0x00CBFC65, 8, stopOnRet: false),
        WriteWalkPart(pe, store, family, "CCutsceneDef ctor 00F29D00", 0x00F29D00, 50),
        WriteWalkPart(pe, store, family, "CCutsceneDef persist 00F2A1D0", 0x00F2A1D0, 50),
        WriteFnPart(pe, store, family, "CString vector persist 004331F9", 0x004331F9, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "CString vector read 00433273", 0x00433273, 50, stopOnRet: false),
        WriteWalkPart(pe, store, family, "def+60 vector copy 00432EE9", 0x00432EE9, 40),
        WriteFnPart(pe, store, family, "command loop index 00CC0205", 0x00CC0205, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "DoScriptFrame token 00CC7085", 0x00CC7085, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "DoScriptFrame wait 00CC70D5", 0x00CC70D5, 30, stopOnRet: false),
        WriteWalkPart(pe, store, family, "CString atoi 0099E7F0", 0x0099E7F0, 40),
        WriteFnPart(pe, store, family, "DoCameraPreloading token 00CC86D0", 0x00CC86D0, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "DoCameraPreloading apply 00CC8720", 0x00CC8720, 30, stopOnRet: false),
        WriteWalkPart(pe, store, family, "IsTrue arg 00CBEDBA", 0x00CBEDBA, 40),
        WriteFnPart(pe, store, family, "MuteSounds token 00CC7258", 0x00CC7258, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "MuteSounds apply 00CC72A8", 0x00CC72A8, 16, stopOnRet: false),
        WriteFnPart(pe, store, family, "MuteSounds join 00CC8464", 0x00CC8464, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "StartTimeCode token 00CD1373", 0x00CD1373, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "StartTimeCode apply 00CD13C3", 0x00CD13C3, 8, stopOnRet: false),
        WriteU32Part(pe, store, family, "StartTimeCode global 013B83C8", 0x013B83C8, 1),
        WriteFnPart(pe, store, family, "GamePause token 00CC88D1", 0x00CC88D1, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "GamePause default wait 00CC89F4", 0x00CC89F4, 40, stopOnRet: false),
        WriteWalkPart(pe, store, family, "GamePause atof 0099E690", 0x0099E690, 40),
        WriteU32Part(pe, store, family, "GamePause scale 0124E640", 0x0124E640, 1),
        WriteU32Part(pe, store, family, "GamePause increment 0122DED8", 0x0122DED8, 1),
        WriteFnPart(pe, store, family, "Speak token 00CC25FD", 0x00CC25FD, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "Speak apply 00CC27EA", 0x00CC27EA, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "Speak poll 00CC2909", 0x00CC2909, 20, stopOnRet: false),
        WriteWalkPart(pe, store, family, "Speak IsNull 00CBEE5E", 0x00CBEE5E, 30),
        WriteWalkPart(pe, store, family, "Speak apply stub 004CD1B0", 0x004CD1B0, 8),
        WriteWalkPart(pe, store, family, "Speak poll stub 00661A40", 0x00661A40, 4),
        WriteVtblPart(pe, store, family, "CThingAICreature vtbl 0127293C", 0x0127293C, 32),
        WriteFnPart(pe, store, family, "InteractiveSpeak token 00CC2EAA", 0x00CC2EAA, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "InteractiveSpeak apply 00CC2F50", 0x00CC2F50, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "InteractiveSpeak yield 00CC30B9", 0x00CC30B9, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "DialogSpeak token 00CC3165", 0x00CC3165, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "DialogSpeak apply 00CC31BC", 0x00CC31BC, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "DialogSpeak yield 00CC3310", 0x00CC3310, 20, stopOnRet: false),
        WriteWalkPart(pe, store, family, "dialog begin vtbl+1456 008906C0", 0x008906C0, 30),
        WriteWalkPart(pe, store, family, "dialog bind vtbl+1460 00890710", 0x00890710, 30),
        WriteWalkPart(pe, store, family, "dialog line vtbl+1464 00890750", 0x00890750, 40),
        WriteWalkPart(pe, store, family, "dialog wait vtbl+1472 008907D0", 0x008907D0, 8),
        WriteWalkPart(pe, store, family, "dialog wait body 006E5660", 0x006E5660, 20),
        WriteFnPart(pe, store, family, "WaitTask token 00CC0783", 0x00CC0783, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "WaitTask poll 00CC082C", 0x00CC082C, 8, stopOnRet: false),
        WriteFnPart(pe, store, family, "WaitTask yield-loop 00CC07E0", 0x00CC07E0, 16, stopOnRet: false),
        WriteVtblPart(pe, store, family, "player thing vtbl 012457FC +104", 0x012457FC, 32),
        WriteFnPart(pe, store, family, "WaitTask hero poll 006A9550", 0x006A9550, 4, stopOnRet: false),
        WriteWalkPart(pe, store, family, "WaitTask poll stub 00661A40", 0x00661A40, 4),
        WriteU32Part(pe, store, family, "fiber global 013D2838", 0x013D2838, 1),
        WriteFnPart(pe, store, family, "SneakTo token 00CC0CB5", 0x00CC0CB5, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "SneakTo apply 00CC0E5A", 0x00CC0E5A, 16, stopOnRet: false),
        WriteFnPart(pe, store, family, "SneakTo yield-once 00CC0E96", 0x00CC0E96, 16, stopOnRet: false),
        WriteWalkPart(pe, store, family, "SneakTo thing vtbl+20 stub 004C72B0", 0x004C72B0, 4),
        WriteFnPart(pe, store, family, "PlayCombatAnim token 00CC15E3", 0x00CC15E3, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayCombatAnim apply 00CC16FD", 0x00CC16FD, 16, stopOnRet: false),
        WriteWalkPart(pe, store, family, "PlayCombatAnim Father vtbl+76 00834760", 0x00834760, 80),
        WriteWalkPart(pe, store, family, "PlayCombatAnim player vtbl+76 006AD9D0", 0x006AD9D0, 80),
        WriteWalkPart(pe, store, family, "CActionPlayCombatAnimation 009035F0", 0x009035F0, 12),
        WriteFnPart(pe, store, family, "Create token 00CCC246", 0x00CCC246, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "Create apply 00CCC3E6", 0x00CCC3E6, 20, stopOnRet: false),
        WriteWalkPart(pe, store, family, "Create vtbl+364 008A9100", 0x008A9100, 80),
        WriteFnPart(pe, store, family, "WalkTo token 00CC083D", 0x00CC083D, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "WalkTo apply 00CC09E2", 0x00CC09E2, 16, stopOnRet: false),
        WriteFnPart(pe, store, family, "WalkTo yield-once 00CC0E96", 0x00CC0E96, 16, stopOnRet: false),
        WriteFnPart(pe, store, family, "WaitActiveDialog token 00CC656B", 0x00CC656B, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "WaitActiveDialog poll 00CC6612", 0x00CC6612, 12, stopOnRet: false),
        WriteFnPart(pe, store, family, "Remove token 00CD0116", 0x00CD0116, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "Remove apply 00CD0224", 0x00CD0224, 12, stopOnRet: false),
        WriteWalkPart(pe, store, family, "Remove vtbl+432 008910D0", 0x008910D0, 30),
        WriteWalkPart(pe, store, family, "Remove inner 004C9B80", 0x004C9B80, 50),
        WriteFnPart(pe, store, family, "DialogadSpeak token 00CC3354", 0x00CC3354, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "DialogadSpeak mode 00CC34C8", 0x00CC34C8, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "DialogadSpeak miss join 00CC2C6B", 0x00CC2C6B, 8, stopOnRet: false),
        WriteWalkPart(pe, store, family, "DialogadSpeak table 00CD3187", 0x00CD3187, 8),
        WriteVtblPart(pe, store, family, "CCutsceneDef vtbl 012FB6E0", 0x012FB6E0, 24),
        WriteCallsPart(pe, store, family, "calls S_QNOVI factory 00DBEF70", 0x00DBEF70),
        WriteCallsPart(pe, store, family, "calls S_QNOVI run 00DABAC0", 0x00DABAC0),
        WriteCallsPart(pe, store, family, "calls StartOakVale 00DBDE40", 0x00DBDE40),
        WriteCallsPart(pe, store, family, "calls script bind 00CB5C90", 0x00CB5C90),
        WriteCallsPart(pe, store, family, "calls start scripts 00CB7780", 0x00CB7780),
        WriteCallsPart(pe, store, family, "calls camera hooks 00CBF29F", 0x00CBF29F),
        WriteImmPart(pe, store, family, "imm script global 143E8F8", 0x143E8F8, 0x00CB0000, 0x00DC0000),
        WriteImmPart(pe, store, family, "imm script global engine", 0x143E8F8, 0x00B20000, 0x00B40000),
        WriteCallDispPart(pe, store, family, "calldisp vtbl+28 yield intro", 0x1C, 0x00DBDE00, 0x00DBF000),
        WriteCallDispPart(pe, store, family, "calldisp vtbl+28 yield scripts", 0x1C, 0x00CB0000, 0x00CE0000),
        WriteCallDispPart(pe, store, family, "calldisp vtbl+2584 wait intro", 0xA18, 0x00DBDE00, 0x00DBF000),
        WriteCallDispPart(pe, store, family, "calldisp vtbl+2584 wait scripts", 0xA18, 0x00CB0000, 0x00CE0000),
        WriteCallDispPart(pe, store, family, "calldisp vtbl+2592 flag intro", 0xA20, 0x00DBDE00, 0x00DBF000),
        WriteCallDispPart(pe, store, family, "calldisp vtbl+8 slot2 scripts", 0x8, 0x00CB7000, 0x00CB9000),
        WriteScanPart(pe, store, family, "scan +80 imm1 scripts", "C6465001", 0x00CB0000, 0x00DC0000),
        WriteScanPart(pe, store, family, "scan +80 al scripts", "884650", 0x00CB0000, 0x00DC0000),
        WriteScanPart(pe, store, family, "scan +80 ebx imm1", "C6435001", 0x00CB0000, 0x00DC0000),
        WriteU32Part(pe, store, family, "CGameScriptInterface RTTI", 0x013801F4, 8),
        WriteU32Part(pe, store, family, "script global 143E8F8 dword", 0x0143E8F8, 4),
    };
    store.WriteIndex(
        family, DumpStore.ScriptRuntimeVersion, "script-runtime",
        "New Game S_QNOVI VM: register, factory, vtbl, yield, wait, +80 gate, text-opcode hooks. Do not invent.",
        links);
    Console.WriteLine($"trace  {family}/  parts={links.Count}  v{DumpStore.ScriptRuntimeVersion}");
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
        WriteWalkPart(pe, store, family, "THING TYPE BUILDING name 004C75B0", 0x004C75B0, 40),
        WriteWalkPart(pe, store, family, "Thing type registrar BUILDING 00522A20", 0x00522A20, 400),
        WriteWalkPart(pe, store, family, "CThingBuilding factory 0052AC10", 0x0052AC10, 40),
        WriteWalkPart(pe, store, family, "CThingBuilding base ctor 005296B0", 0x005296B0, 40),
        WriteWalkPart(pe, store, family, "CThing parent ctor 004C9030", 0x004C9030, 80),
        WriteWalkPart(pe, store, family, "CThingBuilding vtbl3 006A5AF0", 0x006A5AF0, 120),
        WriteWalkPart(pe, store, family, "CREATEBUILDING script 0072E290", 0x0072E290, 160),
        WriteWalkPart(pe, store, family, "CREATEBUILDING body 0072DF50", 0x0072DF50, 160),
        WriteWalkPart(pe, store, family, "CMultiStaticMeshDef name 007E12F0", 0x007E12F0, 20),
        WriteWalkPart(pe, store, family, "CMultiStaticMeshDef lookup 007E1400", 0x007E1400, 80),
        WriteWalkPart(pe, store, family, "CMultiStaticMeshDef ctor 007E14C0", 0x007E14C0, 40),
        WriteWalkPart(pe, store, family, "CMultiStaticMeshDef apply 007E15C0", 0x007E15C0, 400),
        WriteImmPart(pe, store, family, "imm MultiStatic FlagA CRC", 0x7CA90715, 0x007E0000, 0x007E2000),
        WriteImmPart(pe, store, family, "imm MultiStatic FlagB CRC", 0x97595FC1, 0x007E0000, 0x007E2000),
        WriteImmPart(pe, store, family, "imm MultiStatic FlagA CRC defs", 0x7CA90715, 0x00430000, 0x00440000),
        WriteWalkPart(pe, store, family, "CMultiStaticMeshDef lookup 007E1400 persist", 0x007E1400, 120),
        WriteWalkPart(pe, store, family, "CMultiStatic index 007E1370", 0x007E1370, 40),
        WriteWalkPart(pe, store, family, "CMultiStatic parent ctor 00686800", 0x00686800, 80),
        WriteVtblPart(pe, store, family, "CMultiStatic apply vtbl 126FFB4 full", 0x0126FFB4, 12),
        WriteFnPart(pe, store, family, "CMultiStatic vtbl1 007E1590", 0x007E1590, 40),
        WriteWalkPart(pe, store, family, "CMultiStatic vtbl2 persist 007E1990", 0x007E1990, 200),
        WriteWalkPart(pe, store, family, "CMultiStatic vtbl6 007E1AA0", 0x007E1AA0, 80),
        WriteFnPart(pe, store, family, "CMultiStatic vtbl10 007E1570", 0x007E1570, 20),
        WriteFnPart(pe, store, family, "MultiStatic +45 override 007E17AB", 0x007E17AB, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "CMultiStaticMeshDef factory 004E31FA", 0x004E31FA, 16, stopOnRet: false),
        WriteFnPart(pe, store, family, "CMultiStaticMeshDef persist ctor 004E1516", 0x004E1516, 80),
        WriteFnPart(pe, store, family, "CMultiStatic persist this+40 004EDE1B", 0x004EDE1B, 8, stopOnRet: false),
        WriteFnPart(pe, store, family, "CMultiStatic vector persist 004EDE2B", 0x004EDE2B, 120),
        WriteFnPart(pe, store, family, "CMultiStatic vector resize 004EDF0A", 0x004EDF0A, 80),
        WriteVtblPart(pe, store, family, "CMultiStatic entry vtbl 12438A4", 0x012438A4, 24),
        WriteFnPart(pe, store, family, "CMultiStatic entry persist 004EB8C3", 0x004EB8C3, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "CMultiStatic entry assign 004EB831", 0x004EB831, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "CMultiStatic entry copy 004EB864", 0x004EB864, 24, stopOnRet: false),
        WriteFnPart(pe, store, family, "persist dword 00431102", 0x00431102, 30),
        WriteFnPart(pe, store, family, "persist u8 0043314A", 0x0043314A, 40),
        WriteFnPart(pe, store, family, "persist float 00431061", 0x00431061, 30),
        WriteFnPart(pe, store, family, "persist tail dword 004735D6", 0x004735D6, 30),
        WriteFnPart(pe, store, family, "persist tail reader 00473617", 0x00473617, 50),
        WriteFnPart(pe, store, family, "skip-global other apply 0077BA40", 0x0077BA40, 40, stopOnRet: false),
        WriteImmPart(pe, store, family, "imm skip-global 0x13756F0", 0x013756F0, 0x00400000, 0x01000000),
        WriteWalkPart(pe, store, family, "Default float 004BC180", 0x004BC180, 20),
        WriteWalkPart(pe, store, family, "SetVSConstantF1 00989A60", 0x00989A60, 30),
        WriteFnPart(pe, store, family, "Per-cell slot2 00BF5150", 0x00BF5150, 16, stopOnRet: false),
        WriteFnPart(pe, store, family, "Per-cell slot3 00BF5170", 0x00BF5170, 16, stopOnRet: false),
        WriteFnPart(pe, store, family, "Per-cell c1 flip 00BF51D4", 0x00BF51D4, 12, stopOnRet: false),
        WriteWalkPart(pe, store, family, "Landscape shared setup 00B67480", 0x00B67480, 30),
        WriteWalkPart(pe, store, family, "LayoutBasic fields 00BDBB70", 0x00BDBB70, 250),
        WriteWalkPart(pe, store, family, "LayoutRepeatedMesh 00BDB080", 0x00BDB080, 120),
        WriteWalkPart(pe, store, family, "CTC multi-static name 007E1A80", 0x007E1A80, 20),
        WriteFnPart(pe, store, family, "PALSKIN dest x87 00BD2F91", 0x00BD2F91, 80, stopOnRet: false),
        WriteWalkPart(pe, store, family, "PALSKIN hierarchy 00AA0090", 0x00AA0090, 200),
        WriteFnPart(pe, store, family, "SSE detect CPUID 00A5B850", 0x00A5B850, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "SSE flag write 00A5B860", 0x00A5B860, 8, stopOnRet: false),
        WriteVtblPart(pe, store, family, "CMultiStaticMeshDef apply vtbl 126FFB4", 0x0126FFB4, 8),
        WriteWalkPart(pe, store, family, "CTCBuyableHouse ctor 006BF8A0", 0x006BF8A0, 40),
        WriteWalkPart(pe, store, family, "CTCBuyableHouse construct 006C14D0", 0x006C14D0, 220),
        WriteWalkPart(pe, store, family, "CTCBuyableHouse ready 006BFB90", 0x006BFB90, 130),
        WriteWalkPart(pe, store, family, "CTCBuyableHouse window swap 006C0F00", 0x006C0F00, 40),
        WriteWalkPart(pe, store, family, "Inside-building predicate 0082E0E0", 0x0082E0E0, 30),
        WriteWalkPart(pe, store, family, "CBuyableHouseDef lookup 006C1B00", 0x006C1B00, 80),
        WriteFnPart(pe, store, family, "CThing parent +64 zero 004C9058", 0x004C9058, 20, stopOnRet: false),
        WriteWalkPart(pe, store, family, "Theme slot ctor zeros +424 008864A0", 0x008864A0, 80),
        WriteWalkPart(pe, store, family, "Theme slot copy dest+424 008865C0", 0x008865C0, 700),
        WriteFnPart(pe, store, family, "Theme slot copy star write 00886AD2", 0x00886AC6, 20, stopOnRet: false),
        WriteWalkPart(pe, store, family, "WatchBarrels ctor 00CDD450", 0x00CDD450, 40),
        WriteWalkPart(pe, store, family, "WatchBarrels callback 00DBE890", 0x00DBE890, 200),
        WriteWalkPart(pe, store, family, "Component add by name 004C9D60", 0x004C9D60, 80),
        WriteVtblPart(pe, store, family, "CThingBuilding vtbl 0124509C", 0x0124509C, 16),
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
        WriteFnPart(pe, store, family, "Static-lit caller 00BB30A0", 0x00BB30A0, 400),
        WriteFnPart(pe, store, family, "Static compact ctor 00B8B630", 0x00B8B630, 80),
        WriteFnPart(pe, store, family, "Static SetTexture bind 00BB301E", 0x00BB301E, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "SetVertexShader wrapper 00988020", 0x00988020, 40),
        WriteFnPart(pe, store, family, "Attach PS record 00988140", 0x00988140, 20),
        WriteFnPart(pe, store, family, "PS const wrapper ctor 0098ACF0", 0x0098ACF0, 200),
        WriteFnPart(pe, store, family, "PSCONST slot assign 0098DB20", 0x0098DB20, 40),
        WriteFnPart(pe, store, family, "PS record ctor 00A5EC40", 0x00A5EC40, 30),
        WriteFnPart(pe, store, family, "PS name to slot 0098A9A0", 0x0098A9A0, 80),
        WriteImmPart(pe, store, family, "imm PSCONST_OUTPUT_FACTOR", 0x0129A104, 0x00980000, 0x00990000),
        WriteFnPart(pe, store, family, "PALSKIN attach PS +DC 00BD5486", 0x00BD5480, 20, stopOnRet: false),
        WriteWalkPart(pe, store, family, "PARTICLE_EMITTER_NORMAL create 006E0880", 0x006E0880, 80),
        WriteFnPart(pe, store, family, "THING_TYPE_TRACK_NODE name 004C76A5", 0x004C76A5, 16, stopOnRet: false),
        WriteCallDispPart(pe, store, family, "SetTexture 0x104 static-lit", 0x104, 0x00BB2000, 0x00BB8000),
        WriteFnPart(pe, store, family, "Static-lit FVF 0x112 00BB2633", 0x00BB2631, 20, stopOnRet: false),
        WriteWalkPart(pe, store, family, "CreateVertexBuffer wrapper 00A63150", 0x00A63150, 40),
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
        WriteFnPart(pe, store, family, "SPECULARENABLE slot init 00A04B2C", 0x00A04B2C, 16, stopOnRet: false),
        WriteFnPart(pe, store, family, "PALSKIN SPECULARENABLE 00BD30AF", 0x00BD30AF, 20, stopOnRet: false),
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
        WriteFnPart(pe, store, family, "Per-cell c1 flip 00BF5175", 0x00BF5175, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "Per-cell edi slot2 00BF4EB7", 0x00BF4EB7, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "SetVSConstantF1 00989A60", 0x00989A60, 30),
        WriteFnPart(pe, store, family, "Inner VS object ctor 0098D4A0", 0x0098D4A0, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "LayoutRepeatedMesh ctor 00BDB080", 0x00BDB080, 80),
        WriteFnPart(pe, store, family, "Tile expand 15to24 00BFE050", 0x00BFE050, 200, stopOnRet: false),
        WriteFnPart(pe, store, family, "Tile expand copy loop 00BFE490", 0x00BFE490, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "Unpack tile normal 00BFDEC0", 0x00BFDEC0, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "Create landscape VB 00BDA3D0", 0x00BDA3D0, 60),
        WriteFnPart(pe, store, family, "CreateVertexBuffer wrapper 00A63150", 0x00A63150, 50),
        WriteU32Part(pe, store, family, "UV table 0139C5D8", 0x0139C5D8, 8),
        WriteU32Part(pe, store, family, "Layer type jump 00BF586C", 0x00BF586C, 5),
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
        WriteWalkPart(pe, store, family, "Camera constant upload c2 00B54310", 0x00B54310, 400),
        WriteFnPart(pe, store, family, "c4 inverse row2 upload 00B545D5", 0x00B545BE, 20, stopOnRet: false),
        WriteWalkPart(pe, store, family, "Mesh draw 00B555A0 c4 caller", 0x00B555A0, 200),
        WriteFnPart(pe, store, family, "Landscape draw vtbl16 no c4", 0x00B6B0B0, 40),
        WriteWalkPart(pe, store, family, "Fog compute 00B47630", 0x00B47630, 220),
        WriteFnPart(pe, store, family, "Landscape fog slot 00B46890", 0x00B46890, 30),
        WriteFnPart(pe, store, family, "FOGENABLE slot init 00A0495C", 0x00A04944, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "FOGCOLOR slot init 00A04A59", 0x00A04A59, 20, stopOnRet: false),
        WriteWalkPart(pe, store, family, "Landscape setup FOGENABLE 00B67480", 0x00B67480, 40),
        WriteWalkPart(pe, store, family, "MainScene FOGENABLE bits 00B32AD0", 0x00B32AD0, 80),
        WriteWalkPart(pe, store, family, "LayoutBasic 00BDBB70", 0x00BDBB70, 250),
        WriteWalkPart(pe, store, family, "LayoutBasic flush c0 c1 00989BF0", 0x00989BF0, 80),
        WriteWalkPart(pe, store, family, "PALSKIN default draw 00BD549D", 0x00BD549D, 200),
        WriteFnPart(pe, store, family, "Fog colour setter 009886C0", 0x009886C0, 20),
        WriteFnPart(pe, store, family, "Fog colour flush c18 009897C0", 0x009897C0, 40),
        WriteFnPart(pe, store, family, "Fog plane setter 00988600", 0x00988600, 20),
        WriteFnPart(pe, store, family, "Lighting record alloc 00B4A4C0", 0x00B4A4C0, 50),
        WriteFnPart(pe, store, family, "Lighting ctor fog defaults 00B4844C", 0x00B4844C, 20, stopOnRet: false),
        WriteWalkPart(pe, store, family, "Camera update helper FOV 00B314E0", 0x00B314E0, 200),
        WriteFnPart(pe, store, family, "Bind camera push1 00B23B50", 0x00B23B50, 20),
        WriteWalkPart(pe, store, family, "Extract other view 00B2FC50", 0x00B2FC50, 80),
        WriteFnPart(pe, store, family, "View copy 00988350", 0x00988350, 40),
        WriteFnPart(pe, store, family, "World copy 009881F0", 0x009881F0, 40),
        WriteFnPart(pe, store, family, "Proj copy 00988540", 0x00988540, 20),
        WriteFnPart(pe, store, family, "Per-cell world Tcam 00BF46A2", 0x00BF46A2, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "Sky calls 00B2FC50", 0x00B66A01, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "WVP x87 multiply 00988B90", 0x00988B90, 40, stopOnRet: false),
        WriteWalkPart(pe, store, family, "WVP flush 00988A50", 0x00988A50, 200),
        WriteFnPart(pe, store, family, "NString ctor zeros 0099E4B0", 0x0099E4B0, 8),
        WriteFnPart(pe, store, family, "Star list first dword 00B65A64", 0x00B65A61, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "Weather id pointers 00B64FE5", 0x00B64FE5, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "Proj builder 009883F0", 0x009883F0, 40),
        WriteFnPart(pe, store, family, "View copy 00988350", 0x00988350, 40),
        WriteFnPart(pe, store, family, "World copy 009881F0", 0x009881F0, 60),
        WriteFnPart(pe, store, family, "World identity 00988290", 0x00988290, 30),
        WriteFnPart(pe, store, family, "Proj copy 00988540", 0x00988540, 40),
        WriteU32Part(pe, store, family, "Helper minZ 01399D44", 0x01399D44, 2),
        WriteFnPart(pe, store, family, "Bind camera source 00B23B50", 0x00B23B50, 20),
        WriteFnPart(pe, store, family, "Store camera helper +12 00B2FBF0", 0x00B2FBF0, 8),
        WriteVtblPart(pe, store, family, "Engine camera bind vtbl 012A0F4C", 0x012A0F4C, 8),
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
        WriteWalkPart(pe, store, family, "Sky draw else 00B662F0", 0x00B662F0, 200),
        WriteWalkPart(pe, store, family, "Sky dome setup 00B620A0", 0x00B620A0, 100),
        WriteWalkPart(pe, store, family, "Sky dome fill 00B61DD0", 0x00B61DD0, 200),
        WriteWalkPart(pe, store, family, "Sky cap fill 00B61B30", 0x00B61B30, 80),
        WriteWalkPart(pe, store, family, "Sky skirt fill 00B61CD0", 0x00B61CD0, 80),
        WriteU32Part(pe, store, family, "Sky horiz radius 12A2930", 0x012A2930, 2),
        WriteU32Part(pe, store, family, "Sky elev step 139A710", 0x0139A710, 1),
        WriteWalkPart(pe, store, family, "Sky mesh draw 00B66190", 0x00B66190, 120),
        WriteWalkPart(pe, store, family, "Sky outer 00B63C00", 0x00B63C00, 80),
        WriteWalkPart(pe, store, family, "Sky inner 00B640E0", 0x00B640E0, 80),
        WriteU32Part(pe, store, family, "Sky minZ 0139A704", 0x0139A704, 1),
        WriteU32Part(pe, store, family, "Sky scale 12A2AD8", 0x012A2AD8, 2),
        WriteWalkPart(pe, store, family, "Sky float-to-int 00BFEA70", 0x00BFEA70, 40),
        WriteFnPart(pe, store, family, "Sky dome colour tail 00B61EE0", 0x00B61EE0, 40, stopOnRet: false),
        WriteU32Part(pe, store, family, "Sky vBase scale 12A2900", 0x012A2900, 1),
        WriteU32Part(pe, store, family, "Sky colour 255 1230014", 0x01230014, 1),
        WriteU32Part(pe, store, family, "Sky cap pole UV 129BA3C", 0x0129BA3C, 1),
        WriteFnPart(pe, store, family, "Sky ctor this+16 00B627E2", 0x00B627B6, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "ENGINE_VIDEO_OPTIONS lookup", 0x00B26407, 40, stopOnRet: false),
        WriteU32Part(pe, store, family, "Sky UV divisor 143782C", 0x0143782C, 1),
        WriteFnPart(pe, store, family, "Sky UV divisor CRT 01224830", 0x01224830, 8, stopOnRet: false),
        WriteU32Part(pe, store, family, "Sky UV divisor angle 12A1140", 0x012A1140, 2),
        WriteU32Part(pe, store, family, "Sky UV divisor scale 12A1138", 0x012A1138, 1),
        WriteFnPart(pe, store, family, "ENVIRONMENT lookup 00B26828", 0x00B26826, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "ENVIRONMENT CString ctor 0099AED0", 0x0099AED0, 8),
        WriteFnPart(pe, store, family, "ENVIRONMENT string persist 004310A7", 0x004310A7, 40),
        WriteFnPart(pe, store, family, "ENVIRONMENT Transfer +288", 0x00430BF3, 20, stopOnRet: false),
        WriteWalkPart(pe, store, family, "Sky star draw 00B65A20", 0x00B65A20, 80),
        WriteFnPart(pe, store, family, "Sky star draw exact 00B65A20", 0x00B65A20, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "Sky star empty gate 00B65A61", 0x00B65A61, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "Sky star fade 00B65A8A", 0x00B65A8A, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "OpenStaticMaps mode +424", 0x00B42750, 40),
        WriteFnPart(pe, store, family, "Map manager +408 setter 00B42ED0", 0x00B42ED0, 40),
        WriteFnPart(pe, store, family, "Sky ctor weather byte +396", 0x00B62757, 8, stopOnRet: false),
        WriteWalkPart(pe, store, family, "Sky weather draw 00B64FA0", 0x00B64FA0, 80),
        WriteFnPart(pe, store, family, "Sky weather draw exact 00B64FA0", 0x00B64FA0, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "Sky weather id load 00B64FE5", 0x00B64FE5, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "Sky weather all-zero skip 00B651A8", 0x00B651A8, 12, stopOnRet: false),
        WriteFnPart(pe, store, family, "Sky weather all-zero ret 00B659A5", 0x00B659A5, 8, stopOnRet: false),
        WriteFnPart(pe, store, family, "Sky weather id-zero inc 00B659C9", 0x00B659C9, 12, stopOnRet: false),
        WriteFnPart(pe, store, family, "ENVIRONMENT Transfer +448 NString", 0x00430F2D, 16, stopOnRet: false),
        WriteFnPart(pe, store, family, "ENVIRONMENT dtor NString +448", 0x00431820, 20, stopOnRet: false),
        WriteWalkPart(pe, store, family, "Sky gather texture ids 00B63800", 0x00B63800, 80),
        WriteFnPart(pe, store, family, "Sky inner PS bind 00B62BA8", 0x00B62B90, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "Sky inner SIMPLE name 00B62BB6", 0x00B62BB6, 16, stopOnRet: false),
        WriteFnPart(pe, store, family, "Sky inner FULL name 00B62C2D", 0x00B62C2D, 16, stopOnRet: false),
        WriteCallDispPart(pe, store, family, "SetPixelShaderConstantF 0x1B4 sky", 0x1B4, 0x00B62000, 0x00B67000),
        WriteCallDispPart(pe, store, family, "SetTexture 0x104 sky", 0x104, 0x00B62000, 0x00B67000),
        WriteCallDispPart(pe, store, family, "SetPixelShaderConstantF 0x1B4 wrappers", 0x1B4, 0x00988000, 0x0098C000),
        WriteFnPart(pe, store, family, "ENVIRONMENT NString persist 00431143", 0x00431143, 40),
        WriteFnPart(pe, store, family, "ENVIRONMENT Transfer +424", 0x00430DC1, 12, stopOnRet: false),
        WriteFnPart(pe, store, family, "Sky mesh draw calls stars", 0x00B6627A, 20, stopOnRet: false),
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
        WriteWalkPart(pe, store, family, "intro parent NOVI 00DABAC0", 0x00DABAC0, 80),
        WriteFnPart(pe, store, family, "intro parent calls StartOakVale 00DAC250", 0x00DAC250, 30, stopOnRet: false),
        WriteWalkPart(pe, store, family, "S_QNOVI entry 00DBEF70", 0x00DBEF70, 200),
        WriteWalkPart(pe, store, family, "S_QNOVI ctor 00DAAC00", 0x00DAAC00, 200),
        WriteCallsPart(pe, store, family, "calls S_QNOVI ctor 00DAAC00", 0x00DAAC00),
        WriteVtblPart(pe, store, family, "S_QNOVI vtbl 012D7A28", 0x012D7A28, 24),
        WriteWalkPart(pe, store, family, "S_QNOVI vtbl0 00DBEFA0", 0x00DBEFA0, 40),
        WriteWalkPart(pe, store, family, "S_QNOVI vtbl1 00DAACE0", 0x00DAACE0, 120),
        WriteWalkPart(pe, store, family, "S_QNOVI vtbl3 00DAADD0", 0x00DAADD0, 40),
        WriteWalkPart(pe, store, family, "water vtbl+4 prepare 00B71FB0", 0x00B71FB0, 200),
        WriteCallsPart(pe, store, family, "calls water prepare 00B71FB0", 0x00B71FB0),
        WriteWalkPart(pe, store, family, "water type-4 enqueue 00BF44B3", 0x00BF44B3, 80),
        WriteFnPart(pe, store, family, "per-cell type-4 cmp 00BF5175", 0x00BF5175, 40, stopOnRet: false),
        WriteCallsPart(pe, store, family, "calls water enqueue 00BF44B3", 0x00BF44B3),
        WriteCallsPart(pe, store, family, "calls water enqueue fn 00BF44A0", 0x00BF44A0),
        WriteWalkPart(pe, store, family, "script camera hooks 00CBF29F", 0x00CBF29F, 220),
        WriteWalkPart(pe, store, family, "cutscene runner 00CBFB7D", 0x00CBFB7D, 200),
        WriteFnPart(pe, store, family, "cutscene runner exact 00CBFB7D", 0x00CBFB7D, 280, stopOnRet: false),
        WriteFnPart(pe, store, family, "cutscene FadeOut 0.5 site 00CBFDD0", 0x00CBFDD0, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "cutscene arg120 00CBFD95", 0x00CBFD95, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "00DB86B0 calls runner 00DB88DB", 0x00DB88DB, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayAVI site 00CCA26E", 0x00CCA26E, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayAVI token 00CCA26D", 0x00CCA26D, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayAVI apply 00CCA2BD", 0x00CCA2BD, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "command continue join 00CD17F8", 0x00CD17F8, 12, stopOnRet: false),
        WriteWalkPart(pe, store, family, "CString concat 0099F570", 0x0099F570, 30),
        WriteCallDispPart(pe, store, family, "calldisp vtbl+1476 PlayAVI", 0x5C4, 0x00CCA280, 0x00CCA320),
        WriteFnPart(pe, store, family, "NoLoadUseCamera token 00CC9E69", 0x00CC9E69, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "NoLoadUseCamera yield 00CC9F28", 0x00CC9F28, 8, stopOnRet: false),
        WriteFnPart(pe, store, family, "NoLoadUseCamera yield helper 00CC907D", 0x00CC907D, 12, stopOnRet: false),
        WriteWalkPart(pe, store, family, "PlayMusic helper 00CBF7FE", 0x00CBF7FE, 120),
        WriteFnPart(pe, store, family, "PlayMusic helper site 00CBF8F4", 0x00CBF8F4, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayMusic interpreter 00CC8EAC", 0x00CC8EAC, 60, stopOnRet: false),
        WriteCallsPart(pe, store, family, "calls PlayMusic helper 00CBF7FE", 0x00CBF7FE),
        WriteFnPart(pe, store, family, "FadeOut opcode exact 00CD0987", 0x00CD0987, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "FadeOut after match 00CD09DF", 0x00CD09DF, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "FadeOut apply 00CD0AF0", 0x00CD0AF0, 60, stopOnRet: false),
        WriteFnPart(pe, store, family, "command loop continue 00CD17FD", 0x00CD17FD, 80, stopOnRet: false),
        WriteCallDispPart(pe, store, family, "calldisp vtbl+1488 fade runner", 0x5D0, 0x00CBFD00, 0x00CBFE50),
        WriteCallDispPart(pe, store, family, "calldisp vtbl+1496 FadeOut", 0x5D8, 0x00CD0980, 0x00CD0C00),
        WriteWalkPart(pe, store, family, "CS_OAKVALE_INTRO_FATHER start 00DB86B0", 0x00DB86B0, 200),
        WriteWalkPart(pe, store, family, "intro-father dtor 00DB8680", 0x00DB8680, 20),
        WriteWalkPart(pe, store, family, "NOVI_LiveFather factory 00DAC2C0", 0x00DAC2C0, 60),
        WriteWalkPart(pe, store, family, "NOVI name-record create 00DB8520", 0x00DB8520, 80),
        WriteWalkPart(pe, store, family, "NOVI name register 00CB8230", 0x00CB8230, 80),
        WriteWalkPart(pe, store, family, "construct name bind 00CB8960", 0x00CB8960, 120),
        WriteWalkPart(pe, store, family, "thing construct bind 004C97B0", 0x004C97B0, 40),
        WriteWalkPart(pe, store, family, "thing script activate 004C7CF0", 0x004C7CF0, 30),
        WriteWalkPart(pe, store, family, "thing activate scripts 004AFB00", 0x004AFB00, 40),
        WriteFnPart(pe, store, family, "UseCamera token 00CC9F39", 0x00CC9F39, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "UseCamera name bind 00CCA1AA", 0x00CCA1AA, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "UseCamera yield 00CCA22C", 0x00CCA22C, 16, stopOnRet: false),
        WriteFnPart(pe, store, family, "UseCamera ebp-37 ctor 00CBFD53", 0x00CBFD53, 8, stopOnRet: false),
        WriteVtblPart(pe, store, family, "NOVI name-record vtbl 012D8370", 0x012D8370, 4),
        WriteVtblPart(pe, store, family, "NOVI_LiveFather vtbl 012D8388", 0x012D8388, 8),
        WriteVtblPart(pe, store, family, "intro-father microthread 012D95B0", 0x012D95B0, 8),
        WriteU32Part(pe, store, family, "NOVI_LiveFather rdata 012D8370", 0x012D8370, 16),
        WriteCallsPart(pe, store, family, "calls cutscene runner 00CBFB7D", 0x00CBFB7D),
        WriteCallsPart(pe, store, family, "calls LiveFather factory 00DAC2C0", 0x00DAC2C0),
        WriteScanPart(pe, store, family, "scan LiveFather factory +16", "C74710C0C2DA00", 0x00DABB00, 0x00DABB20),
        WriteFnPart(pe, store, family, "UseCamera site 00CBF3AC", 0x00CBF3AC, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "CameraLookAt site 00CBF3FE", 0x00CBF3FE, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayAnimation token 00CC14B8", 0x00CC14B8, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayAnimation apply 00CC1527", 0x00CC1527, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayAnimation yield-join 00CC186F", 0x00CC186F, 8, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayAnimation yield-once 00CC5691", 0x00CC5691, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayAnimation leftover 00CC0EBC", 0x00CC0EBC, 12, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayAnimation ebp-22 ctor 00CBFD57", 0x00CBFD57, 8, stopOnRet: false),
        WriteU32Part(pe, store, family, "PlayAnimation flag byte 01375748", 0x01375748, 1),
        WriteWalkPart(pe, store, family, "PlayAnimation thing vtbl+72 004C7470", 0x004C7470, 40),
        WriteVtblPart(pe, store, family, "player thing vtbl 012457FC", 0x012457FC, 24),
        WriteVtblPart(pe, store, family, "CTCAnimationComplex vtbl 012650A4", 0x012650A4, 24),
        WriteWalkPart(pe, store, family, "CTCAnimationComplex factory 0070B3F0", 0x0070B3F0, 30),
        WriteFnPart(pe, store, family, "CTCAnimationComplex +68 stub 00686920", 0x00686920, 4, stopOnRet: false),
        WriteFnPart(pe, store, family, "CTCAnimationComplex type 90 0070B3C0", 0x0070B3C0, 4, stopOnRet: false),
        WriteFnPart(pe, store, family, "CTCAnimationComplex inner getter 0070B460", 0x0070B460, 4, stopOnRet: false),
        WriteWalkPart(pe, store, family, "CTCAnimationComplex inner play 0070D580", 0x0070D580, 80),
        WriteWalkPart(pe, store, family, "CTCAnimationComplex play request 0070C050", 0x0070C050, 30),
        WriteFnPart(pe, store, family, "CTCAnimationComplex post-attach 0070B600", 0x0070B600, 4, stopOnRet: false),
        WriteWalkPart(pe, store, family, "appearance DEFAULT play 005B37F7", 0x005B37F7, 80),
        WriteCallsPart(pe, store, family, "calls inner play 0070D580", 0x0070D580),
        WriteFnPart(pe, store, family, "Teleport token 00CC4678", 0x00CC4678, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "Teleport apply 00CC47B4", 0x00CC47B4, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "LookToThing token 00CC3B3F", 0x00CC3B3F, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "LookToThing yield 00CC3C94", 0x00CC3C94, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "actor command join 00CC707C", 0x00CC707C, 50, stopOnRet: false),
        WriteWalkPart(pe, store, family, "IsFalse arg 00CBEE0C", 0x00CBEE0C, 40),
        WriteFnPart(pe, store, family, "runner ebp+103 yield-enable 00CBFC65", 0x00CBFC65, 8, stopOnRet: false),
        WriteFnPart(pe, store, family, "DoScriptFrame token 00CC7085", 0x00CC7085, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "DoScriptFrame wait 00CC70D5", 0x00CC70D5, 30, stopOnRet: false),
        WriteWalkPart(pe, store, family, "CString atoi 0099E7F0", 0x0099E7F0, 40),
        WriteFnPart(pe, store, family, "DoCameraPreloading token 00CC86D0", 0x00CC86D0, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "DoCameraPreloading apply 00CC8720", 0x00CC8720, 30, stopOnRet: false),
        WriteWalkPart(pe, store, family, "IsTrue arg 00CBEDBA", 0x00CBEDBA, 40),
        WriteFnPart(pe, store, family, "MuteSounds token 00CC7258", 0x00CC7258, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "MuteSounds apply 00CC72A8", 0x00CC72A8, 16, stopOnRet: false),
        WriteFnPart(pe, store, family, "MuteSounds join 00CC8464", 0x00CC8464, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "StartTimeCode token 00CD1373", 0x00CD1373, 20, stopOnRet: false),
        WriteFnPart(pe, store, family, "StartTimeCode apply 00CD13C3", 0x00CD13C3, 8, stopOnRet: false),
        WriteU32Part(pe, store, family, "StartTimeCode global 013B83C8", 0x013B83C8, 1),
        WriteFnPart(pe, store, family, "GamePause token 00CC88D1", 0x00CC88D1, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "GamePause default wait 00CC89F4", 0x00CC89F4, 40, stopOnRet: false),
        WriteWalkPart(pe, store, family, "GamePause atof 0099E690", 0x0099E690, 40),
        WriteU32Part(pe, store, family, "GamePause scale 0124E640", 0x0124E640, 1),
        WriteU32Part(pe, store, family, "GamePause increment 0122DED8", 0x0122DED8, 1),
        WriteFnPart(pe, store, family, "Speak token 00CC25FD", 0x00CC25FD, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "Speak apply 00CC27EA", 0x00CC27EA, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "Speak poll 00CC2909", 0x00CC2909, 20, stopOnRet: false),
        WriteWalkPart(pe, store, family, "Speak IsNull 00CBEE5E", 0x00CBEE5E, 30),
        WriteWalkPart(pe, store, family, "Speak apply stub 004CD1B0", 0x004CD1B0, 8),
        WriteWalkPart(pe, store, family, "Speak poll stub 00661A40", 0x00661A40, 4),
        WriteVtblPart(pe, store, family, "CThingAICreature vtbl 0127293C", 0x0127293C, 32),
        WriteFnPart(pe, store, family, "InteractiveSpeak token 00CC2EAA", 0x00CC2EAA, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "InteractiveSpeak apply 00CC2F50", 0x00CC2F50, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "InteractiveSpeak yield 00CC30B9", 0x00CC30B9, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "DialogSpeak token 00CC3165", 0x00CC3165, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "DialogSpeak apply 00CC31BC", 0x00CC31BC, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "DialogSpeak yield 00CC3310", 0x00CC3310, 20, stopOnRet: false),
        WriteWalkPart(pe, store, family, "dialog begin vtbl+1456 008906C0", 0x008906C0, 30),
        WriteWalkPart(pe, store, family, "dialog bind vtbl+1460 00890710", 0x00890710, 30),
        WriteWalkPart(pe, store, family, "dialog line vtbl+1464 00890750", 0x00890750, 40),
        WriteWalkPart(pe, store, family, "dialog wait vtbl+1472 008907D0", 0x008907D0, 8),
        WriteWalkPart(pe, store, family, "dialog wait body 006E5660", 0x006E5660, 20),
        WriteFnPart(pe, store, family, "WaitTask token 00CC0783", 0x00CC0783, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "WaitTask poll 00CC082C", 0x00CC082C, 8, stopOnRet: false),
        WriteFnPart(pe, store, family, "WaitTask yield-loop 00CC07E0", 0x00CC07E0, 16, stopOnRet: false),
        WriteVtblPart(pe, store, family, "player thing vtbl 012457FC +104", 0x012457FC, 32),
        WriteFnPart(pe, store, family, "WaitTask hero poll 006A9550", 0x006A9550, 4, stopOnRet: false),
        WriteWalkPart(pe, store, family, "WaitTask poll stub 00661A40", 0x00661A40, 4),
        WriteU32Part(pe, store, family, "fiber global 013D2838", 0x013D2838, 1),
        WriteFnPart(pe, store, family, "SneakTo token 00CC0CB5", 0x00CC0CB5, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "SneakTo apply 00CC0E5A", 0x00CC0E5A, 16, stopOnRet: false),
        WriteFnPart(pe, store, family, "SneakTo yield-once 00CC0E96", 0x00CC0E96, 16, stopOnRet: false),
        WriteWalkPart(pe, store, family, "SneakTo thing vtbl+20 stub 004C72B0", 0x004C72B0, 4),
        WriteFnPart(pe, store, family, "PlayCombatAnim token 00CC15E3", 0x00CC15E3, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "PlayCombatAnim apply 00CC16FD", 0x00CC16FD, 16, stopOnRet: false),
        WriteWalkPart(pe, store, family, "PlayCombatAnim Father vtbl+76 00834760", 0x00834760, 80),
        WriteWalkPart(pe, store, family, "PlayCombatAnim player vtbl+76 006AD9D0", 0x006AD9D0, 80),
        WriteWalkPart(pe, store, family, "CActionPlayCombatAnimation 009035F0", 0x009035F0, 12),
        WriteFnPart(pe, store, family, "Create token 00CCC246", 0x00CCC246, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "Create apply 00CCC3E6", 0x00CCC3E6, 20, stopOnRet: false),
        WriteWalkPart(pe, store, family, "Create vtbl+364 008A9100", 0x008A9100, 80),
        WriteFnPart(pe, store, family, "WalkTo token 00CC083D", 0x00CC083D, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "WalkTo apply 00CC09E2", 0x00CC09E2, 16, stopOnRet: false),
        WriteFnPart(pe, store, family, "WalkTo yield-once 00CC0E96", 0x00CC0E96, 16, stopOnRet: false),
        WriteFnPart(pe, store, family, "WaitActiveDialog token 00CC656B", 0x00CC656B, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "WaitActiveDialog poll 00CC6612", 0x00CC6612, 12, stopOnRet: false),
        WriteFnPart(pe, store, family, "Remove token 00CD0116", 0x00CD0116, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "Remove apply 00CD0224", 0x00CD0224, 12, stopOnRet: false),
        WriteWalkPart(pe, store, family, "Remove vtbl+432 008910D0", 0x008910D0, 30),
        WriteWalkPart(pe, store, family, "Remove inner 004C9B80", 0x004C9B80, 50),
        WriteFnPart(pe, store, family, "DialogadSpeak token 00CC3354", 0x00CC3354, 50, stopOnRet: false),
        WriteFnPart(pe, store, family, "DialogadSpeak mode 00CC34C8", 0x00CC34C8, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "DialogadSpeak miss join 00CC2C6B", 0x00CC2C6B, 8, stopOnRet: false),
        WriteWalkPart(pe, store, family, "DialogadSpeak table 00CD3187", 0x00CD3187, 8),
        WriteWalkPart(pe, store, family, "FadeIn FadeOut 00CC4B22", 0x00CC4B22, 80),
        WriteWalkPart(pe, store, family, "Registering Scripts 00CB5D80", 0x00CB5D80, 80),
        WriteWalkPart(pe, store, family, "quest base ctor 00CB8110", 0x00CB8110, 80),
        WriteWalkPart(pe, store, family, "CActionPlayAnimation 00903570", 0x00903570, 40),
        WriteCallsPart(pe, store, family, "calls UseCamera helper 00CBF29F", 0x00CBF29F),
        WriteCallsPart(pe, store, family, "calls PlayAnimation dispatcher 00CBFACA", 0x00CBFACA),
        WriteWalkPart(pe, store, family, "water query vtbl+40 00B7ED70", 0x00B7ED70, 20),
        WriteVtblPart(pe, store, family, "water vtbl 012A3364 full", 0x012A3364, 16),
        WriteFnPart(pe, store, family, "S_QNOVI bind site 00CD6E1D", 0x00CD6E1D, 40, stopOnRet: false),
        WriteWalkPart(pe, store, family, "quest script bind 00CB5C90", 0x00CB5C90, 80),
        WriteWalkPart(pe, store, family, "quest script alias 00CB5AC0", 0x00CB5AC0, 40),
        WriteWalkPart(pe, store, family, "NOVI name register 00CB8230", 0x00CB8230, 40),
        WriteWalkPart(pe, store, family, "watcher register 00CB7E50", 0x00CB7E50, 80),
        WriteWalkPart(pe, store, family, "WatchBarrels ctor 00CDD450", 0x00CDD450, 40),
        WriteFnPart(pe, store, family, "WatchBarrels callback 00DBE890", 0x00DBE890, 220, stopOnRet: false),
        WriteFnPart(pe, store, family, "WatchForGotGold 00DBE2E0", 0x00DBE2E0, 80, stopOnRet: false),
        WriteFnPart(pe, store, family, "PreAttack +80 wait 00DBE1FA", 0x00DBE1FA, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "PreAttack 12s vtbl 00DBE139", 0x00DBE134, 20, stopOnRet: false),
        WriteWalkPart(pe, store, family, "PostAttack 00DBE3C0", 0x00DBE3C0, 80),
        WriteWalkPart(pe, store, family, "render frame 00B25950", 0x00B25950, 120),
        WriteWalkPart(pe, store, family, "bind camera 00B23B50", 0x00B23B50, 20),
        WriteWalkPart(pe, store, family, "store camera helper 00B2FBF0", 0x00B2FBF0, 20),
        WriteFnPart(pe, store, family, "pre-pass camera 00B2798F", 0x00B27980, 40, stopOnRet: false),
        WriteFnPart(pe, store, family, "StayFadedOut 00CD087E", 0x00CD087E, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "FadeOut opcode 00CD096F", 0x00CD096F, 30, stopOnRet: false),
        WriteFnPart(pe, store, family, "FadeIn opcode 00CD08xx", 0x00CD08A0, 20, stopOnRet: false),
        WriteWalkPart(pe, store, family, "CAM_SHOT parser 004CE550", 0x004CE550, 80),
        WriteCallsPart(pe, store, family, "calls StartOakVale 00DBDE40", 0x00DBDE40),
        WriteCallsPart(pe, store, family, "calls camera update 00B314E0", 0x00B314E0),
        WriteCallsPart(pe, store, family, "calls store helper 00B2FBF0", 0x00B2FBF0),
        WriteCallsPart(pe, store, family, "calls S_QNOVI 00DBEF70", 0x00DBEF70),
        WriteCallsPart(pe, store, family, "calls intro parent 00DABAC0", 0x00DABAC0),
        WriteScanPart(pe, store, family, "scan +80 esi+50 StartOakVale", "C64650", 0x00DBDE00, 0x00DBF000),
        WriteScanPart(pe, store, family, "scan +80 al StartOakVale", "884650", 0x00DBDE00, 0x00DBF000),
        WriteScanPart(pe, store, family, "scan +80 esi32 StartOakVale", "C68650000000", 0x00DB0000, 0x00DC0000),
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

static void RunTraceShaders(PeImage pe, DumpStore store)
{
    const string family = "shader-tokens";
    if (!store.ShouldWrite(family, DumpStore.ShaderTokensVersion))
    {
        Console.WriteLine($"skip  {family}  v{DumpStore.ShaderTokensVersion} (exe unchanged)");
        return;
    }

    var install = GameInstall.TryLocate();
    if (install is null || !File.Exists(install.ShadersBigPath))
    {
        Console.Error.WriteLine("shaders.big not found; skip shader-tokens");
        return;
    }

    var wanted = new (string Bank, string Name)[]
    {
        ("PIXEL_SHADERS", "PSHADER_INNER_SKY"),
        ("PIXEL_SHADERS", "PSHADER_INNER_SKY_SIMPLE"),
        ("PIXEL_SHADERS", "PSHADER_OUTER_SKY"),
        ("PIXEL_SHADERS", "PSHADER_SKY_STAR_FIELD"),
        ("PIXEL_SHADERS", "PSHADER_TEXTURE_DIFFUSE_FOG"),
        ("PIXEL_SHADERS", "PSHADER_TEXTURE_DIFFUSE"),
        ("PIXEL_SHADERS", "PSHADER_LANDSCAPE_FOREGROUND"),
        ("PIXEL_SHADERS", "PSHADER_LANDSCAPE_BACKGROUND"),
        ("SHADERS_SKY", "VSHADER_INNER_SKY"),
        ("SHADERS_SKY", "VSHADER_OUTER_SKY"),
        ("SHADERS_SKY", "VSHADER_SKY_STAR_FIELD"),
        ("SHADERS_LANDSCAPE_FOREGROUND", "VSHADER_LANDSCAPE_FOREGROUND"),
        ("SHADERS_LANDSCAPE_BACKGROUND", "VSHADER_LANDSCAPE_BACKGROUND"),
        ("SHADERS_STATIC", "VSHADER_STATIC_DIRLIGHT_FOG"),
        ("SHADERS_PALSKIN", "VSHADER_PALSKIN_DIRLIGHT_FOG"),
    };

    var links = new List<IndexLink>();
    var tsv = new StringBuilder();
    tsv.AppendLine("bank\tname\tprofile\ttex\tconst\tops");
    using (var big = BigArchive.Open(install.ShadersBigPath))
    {
        foreach (var (bankName, name) in wanted)
        {
            var bank = big.SubBanks.FirstOrDefault(b => b.Name == bankName);
            if (bank is null)
                continue;
            var entry = big.ReadEntries(bank).FirstOrDefault(e => e.Name == name);
            if (entry is null)
                continue;
            var program = ShaderProgram.Parse(name, bankName, entry.Type, big.Read(entry));
            var slug = DumpStore.Slug(name, 0);
            var sb = new StringBuilder();
            sb.AppendLine($"# {name}");
            sb.AppendLine();
            sb.AppendLine($"bank `{bankName}` · `{program.Profile}` · tex **{program.TexCount}** · declared **{program.DeclaredSize}**.");
            sb.AppendLine();
            sb.AppendLine("const: " + string.Join(", ", program.ConstRegisters.Select(r => "c" + r)));
            if (program.CommentStrings.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("ctab: " + string.Join(", ", program.CommentStrings));
            }
            sb.AppendLine();
            sb.AppendLine("```");
            sb.Append(program.ToListing());
            sb.AppendLine("```");
            store.WritePart(family, slug, sb.ToString());
            links.Add(new IndexLink(slug, name, 0));
            tsv.Append(bankName).Append('\t').Append(name).Append('\t')
                .Append(program.Profile).Append('\t').Append(program.TexCount).Append('\t')
                .Append(string.Join(",", program.ConstRegisters)).Append('\t')
                .Append(string.Join(" ", program.DecodeInstructions().Select(i => $"0x{i.Opcode:X}")))
                .AppendLine();
        }
    }

    store.WritePart(family, "tokens-tsv", "```\n" + tsv + "```\n");
    links.Add(new IndexLink("tokens-tsv", "Token opcode TSV", 0));
    links.Add(WriteFnPart(pe, store, family, "Sky inner PS bind 00B62BA8", 0x00B62BA0, 24, stopOnRet: false));
    links.Add(WriteCallDispPart(pe, store, family, "SetPixelShaderConstantF 0x1B4 sky", 0x1B4, 0x00B62000, 0x00B67000));
    links.Add(WriteCallDispPart(pe, store, family, "SetTexture 0x104 sky", 0x104, 0x00B62000, 0x00B67000));
    links.Add(WriteCallDispPart(pe, store, family, "SetPixelShaderConstantF 0x1B4 wrappers", 0x1B4, 0x00988000, 0x0098C000));
    links.Add(WriteCallDispPart(pe, store, family, "SetPixelShaderConstantF 0x1B4 first-scene", 0x1B4, 0x00B20000, 0x00B80000));
    links.Add(WriteFnPart(pe, store, family, "PS constant wrapper 009888E0", 0x009888E0, 16, stopOnRet: false));
    links.Add(WriteWalkPart(pe, store, family, "LayoutBasic PS flush 00989BF0", 0x00989BF0, 80));
    links.Add(WriteCallsPart(pe, store, family, "calls PS wrapper 009888E0", 0x009888E0));
    links.Add(WriteCallsPart(pe, store, family, "calls LayoutBasic PS flush 00989BF0", 0x00989BF0));
    links.Add(WriteFnPart(pe, store, family, "Sky PS flush 00B631DF", 0x00B631C0, 24, stopOnRet: false));
    links.Add(WriteFnPart(pe, store, family, "Sky PS flush 00B634A7", 0x00B63488, 24, stopOnRet: false));
    links.Add(WriteFnPart(pe, store, family, "Sky PS flush 00B63767", 0x00B63748, 24, stopOnRet: false));
    links.Add(WriteFnPart(pe, store, family, "Sky PS flush 00B65937", 0x00B65918, 24, stopOnRet: false));
    links.Add(WriteFnPart(pe, store, family, "Sky PS flush 00B66086", 0x00B66068, 24, stopOnRet: false));
    links.Add(WriteImmPart(pe, store, family, "imm c92 sky", 92, 0x00B62000, 0x00B67000));
    links.Add(WriteImmPart(pe, store, family, "imm c92 wrappers", 92, 0x00988000, 0x0098C000));
    links.Add(WriteScanPart(pe, store, family, "push 92 sky", "6A5C", 0x00B62000, 0x00B67000));
    links.Add(WriteScanPart(pe, store, family, "push 40 landscape", "6A28", 0x00B60000, 0x00C00000));
    links.Add(WriteImmPart(pe, store, family, "imm 40 landscape", 40, 0x00B60000, 0x00C00000));
    links.Add(WriteImmPart(pe, store, family, "imm 41 landscape", 41, 0x00B60000, 0x00C00000));
    links.Add(WriteScanPart(pe, store, family, "push 40 wrappers", "6A28", 0x00988000, 0x0098C000));
    store.WriteIndex(
        family, DumpStore.ShaderTokensVersion, "shader-tokens",
        "First-seen New Game shader token listings from shaders.big plus bind/PS-constant opcode hits. This is the opcode database — dump here instead of grepping.",
        links);
    Console.WriteLine($"trace  {family}/  parts={links.Count}  v{DumpStore.ShaderTokensVersion}");
}

static IndexLink WriteScanPart(
    PeImage pe, DumpStore store, string family, string name, string hex, uint lo, uint hi)
{
    var slug = DumpStore.Slug(name, 0);
    var sb = new StringBuilder();
    sb.AppendLine($"# {name}");
    sb.AppendLine();
    sb.AppendLine($"scan `{hex}` in `0x{lo:X8}`–`0x{hi:X8}`. [INDEX](INDEX.md)");
    sb.AppendLine();
    var needle = new byte[hex.Length / 2];
    for (var n = 0; n < needle.Length; n++)
        byte.TryParse(hex.AsSpan(n * 2, 2), System.Globalization.NumberStyles.HexNumber, null, out needle[n]);
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
            sb.AppendLine($"- `0x{va:X8}`");
            hits++;
        }
    }

    sb.AppendLine();
    sb.AppendLine($"hits **{hits}**");
    store.WritePart(family, slug, sb.ToString());
    return new IndexLink(slug, name, 0);
}

static IndexLink WriteCallsPart(PeImage pe, DumpStore store, string family, string name, uint target)
{
    var slug = DumpStore.Slug(name, target);
    var sb = new StringBuilder();
    sb.AppendLine($"# {name}");
    sb.AppendLine();
    sb.AppendLine($"`E8` sites that call `0x{target:X8}`. [INDEX](INDEX.md)");
    sb.AppendLine();
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
            sb.AppendLine($"- `0x{pe.Va(i):X8}`");
            hits++;
        }
    }

    sb.AppendLine();
    sb.AppendLine($"hits **{hits}**");
    store.WritePart(family, slug, sb.ToString());
    return new IndexLink(slug, name, target);
}

static IndexLink WriteImmPart(
    PeImage pe, DumpStore store, string family, string name, uint value, uint lo, uint hi)
{
    var slug = DumpStore.Slug(name, value);
    var sb = new StringBuilder();
    sb.AppendLine($"# {name}");
    sb.AppendLine();
    sb.AppendLine($"imm `0x{value:X}` in `0x{lo:X8}`–`0x{hi:X8}`. [INDEX](INDEX.md)");
    sb.AppendLine();
    var data = pe.Data;
    var hits = 0;
    foreach (var sec in pe.Sections)
    {
        if (!pe.InCode((int)sec.FileOffset))
            continue;
        var end = Math.Min(data.Length, (int)(sec.FileOffset + sec.FileSize) - 3);
        for (var i = (int)sec.FileOffset; i < end; i++)
        {
            if (BitConverter.ToUInt32(data, i) != value)
                continue;
            var start = X86.FindImmInsn(pe, i);
            var va = pe.Va(start);
            if (va < lo || va > hi)
                continue;
            sb.AppendLine($"- `0x{va:X8}`");
            hits++;
        }
    }

    sb.AppendLine();
    sb.AppendLine($"hits **{hits}**");
    store.WritePart(family, slug, sb.ToString());
    return new IndexLink(slug, name, value);
}

static IndexLink WriteCallDispPart(
    PeImage pe, DumpStore store, string family, string name, uint disp, uint lo, uint hi)
{
    var slug = DumpStore.Slug(name, disp);
    var sb = new StringBuilder();
    sb.AppendLine($"# {name}");
    sb.AppendLine();
    sb.AppendLine($"`call [r+0x{disp:X}]` in `0x{lo:X8}`–`0x{hi:X8}`. [INDEX](INDEX.md)");
    sb.AppendLine();
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
            uint va;
            var is32 = false;
            foreach (var m in mods32)
            {
                if (data[i + 1] == m)
                {
                    is32 = true;
                    break;
                }
            }

            if (is32)
            {
                if (BitConverter.ToUInt32(data, i + 2) != disp)
                    continue;
                va = pe.Va(i);
                if (va < lo || va > hi)
                    continue;
                sb.AppendLine($"- `0x{va:X8}` call [r+0x{disp:X}]");
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
                sb.AppendLine($"- `0x{va:X8}` call [r+0x{disp:X}]8");
                hits++;
            }
        }
    }

    sb.AppendLine();
    sb.AppendLine($"hits **{hits}**");
    store.WritePart(family, slug, sb.ToString());
    return new IndexLink(slug, name, disp);
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
