using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Fable.Core;

namespace Fable.ExeIndex;

internal sealed record NativeEvidenceSpec(
    string Format,
    string OutputOption,
    string DefaultFile,
    string Description,
    IReadOnlyList<(uint Va, string Hint)> Probes,
    IReadOnlyList<string> AnchorTerms,
    IReadOnlyList<string> Artifacts,
    bool IncludePeEntry = false);

/// <summary>
/// Builds a grep-oriented, mechanically derived view of process and game-mode
/// lifecycle code. Existing host lifecycle implementations are deliberately
/// not referenced: labels are navigation aids, while instructions and edges
/// are the evidence.
/// </summary>
internal static partial class LifecycleBehaviorExport
{
    private const int DefaultDepth = 1;
    private const int DefaultCallerDepth = 0;
    private const int DefaultMaxFunctions = 2000;
    private const int MaxInstructions = 3500;
    private const int MaxVtblSlots = 512;
    private const int MaxInboundRowsPerFunction = 128;
    private static readonly Dictionary<string, (long Bytes, string Sha256)> ArtifactCache =
        new(StringComparer.OrdinalIgnoreCase);
    private static PeImage? CachedPe;
    private static List<AsciiRow>? CachedStrings;
    private static CallIndex? CachedCalls;

    // Address probes are entry points into the evidence graph, not assertions
    // about behavior. Their names are explicitly marked navigation_only.
    private static readonly (uint Va, string Hint)[] AddressProbes =
    [
        (0x00412F90, "mode_dispatch_probe"),
        (0x0042EC7C, "outer_mode_loop_probe"),
        (0x0042DB40, "frontend_construct_probe"),
        (0x0042DC94, "frontend_tick_probe"),
        (0x0042DF9E, "frontend_draw_probe"),
        (0x0042F2A2, "frontend_exit_probe"),
        (0x0042F491, "game_create_callsite_probe"),
        (0x00418DCA, "game_object_ctor_probe"),
        (0x004184BD, "game_object_start_probe"),
        (0x004189C2, "game_object_loop_probe"),
        (0x00416953, "world_open_probe"),
        (0x004A5A40, "world_tick_probe"),
        (0x004162B5, "frame_update_probe"),
        (0x004175E5, "game_leave_probe"),
        (0x00500540, "region_request_probe"),
        (0x006C2710, "level_loader_tick_probe"),
        (0x00B25950, "render_frame_probe"),
        (0x009A6370, "window_message_pump_probe"),
    ];

    private static readonly string[] AnchorTerms =
    [
        "Init Game", "Init World", "Init Graphics", "Init Display",
        "Create Players", "Load Particles", "Load World", "FinalAlbion.wld",
        "Frontend", "Press Start", "Leave Frontend", "Shutdown",
        "Setup library", "Level loader update", "OpenStaticMaps",
    ];

    public static void Run(PeImage pe, GameInstall? install, string[] args)
    {
        RunCustom(pe, install, args, new NativeEvidenceSpec(
            "FABLE_LIFECYCLE_GREP_V1",
            "--lifecycle-out",
            "engine-lifecycle-grep.txt",
            "process and game-mode lifecycle",
            AddressProbes,
            AnchorTerms,
            [
                "Levels/FinalAlbion.wld",
                "Levels/FinalAlbion.qst",
                "compiled/script.bin",
                "compiled/frontend.bin",
                "compiled/game.bin",
            ],
            IncludePeEntry: true));
    }

    internal static void RunCustom(
        PeImage pe, GameInstall? install, string[] args, NativeEvidenceSpec spec)
    {
        var output = ResolveOutput(args, spec.OutputOption, spec.DefaultFile);
        var depth = ReadIntOption(args, "--max-depth", DefaultDepth, 0, 12);
        var callerDepth = ReadIntOption(args, "--caller-depth", DefaultCallerDepth, 0, 4);
        var maxFunctions = ReadIntOption(args, "--max-functions", DefaultMaxFunctions, 100, 40000);
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);

        if (!ReferenceEquals(CachedPe, pe))
        {
            CachedPe = pe;
            CachedStrings = ExtractAscii(pe);
            CachedCalls = BuildCallIndex(pe);
        }
        var strings = CachedStrings!;
        var stringByVa = strings.ToDictionary(row => row.Va, row => row.Text);
        var callIndex = CachedCalls!;
        var seeds = BuildSeeds(pe, strings, spec);

        using var writer = new StreamWriter(output, false, new UTF8Encoding(false))
        {
            NewLine = "\n",
        };
        WriteHeader(writer, pe, spec, depth, callerDepth, maxFunctions);
        WriteArtifacts(writer, install, spec.Artifacts);
        WriteAnchors(writer, seeds);
        WriteFunctions(writer, pe, seeds, callIndex, stringByVa,
            depth, callerDepth, maxFunctions);
        writer.WriteLine($"END\tformat={spec.Format}");
        Console.WriteLine($"system {spec.DefaultFile} {output}");
    }

    private static void WriteHeader(
        TextWriter writer, PeImage pe, NativeEvidenceSpec spec,
        int depth, int callerDepth, int maxFunctions)
    {
        writer.WriteLine($"# {spec.Format} - generated executable evidence; one fact per line");
        writer.WriteLine("# No facts are imported from the current C# runtime implementation.");
        writer.WriteLine($"# Scope: {spec.Description}.");
        writer.WriteLine("# Labels and hints are navigation_only; INSN/CALL/BRANCH/MEMORY/GLOBAL/VCALL/IAT are mechanical.");
        writer.WriteLine("# rg '^SEED|^FUNCTION' <file>");
        writer.WriteLine("# rg 'function=0x004189C2|target=0x004189C2' <file>");
        writer.WriteLine("# rg '^GLOBAL|^VCALL|^UNRESOLVED' <file>");
        writer.WriteLine("# Records: META ARTIFACT STRING_ANCHOR SEED FUNCTION SUMMARY INBOUND INBOUND_SUMMARY CALL BRANCH INSN LITERAL MEMORY GLOBAL VCALL IAT VTABLE VTABLE_SLOT ABS UNRESOLVED END");
        writer.WriteLine($"META\tformat={spec.Format}\texe_id={pe.Identity}\timage_base=0x{pe.ImageBase:X8}\tentry_point=0x{pe.EntryPoint:X8}\tcall_depth={depth}\tcaller_depth={callerDepth}\tmax_functions={maxFunctions}");
        writer.WriteLine("META\ttruth=decoded_edges_and_bytes_are_evidence;seed_hints_are_not_semantics;unresolved_edges_must_not_be_guessed");
        foreach (var section in pe.Sections)
            writer.WriteLine($"META\tsection={Atom(section.Name)}\tva=0x{pe.ImageBase + section.Rva:X8}\tvirtual_size=0x{section.VirtualSize:X}\tfile_size=0x{section.FileSize:X}\tcharacteristics=0x{section.Characteristics:X8}");
    }

    private static void WriteArtifacts(
        TextWriter writer, GameInstall? install, IReadOnlyList<string> artifactSpecs)
    {
        if (install is null)
        {
            writer.WriteLine("UNRESOLVED\tkind=game_install\treason=not_found");
            return;
        }

        var candidates = artifactSpecs.Select(spec => ResolveArtifact(install, spec));
        foreach (var candidate in candidates.Where(path => !string.IsNullOrWhiteSpace(path)))
        {
            var path = Path.GetFullPath(candidate!);
            if (!File.Exists(path))
            {
                writer.WriteLine($"UNRESOLVED\tkind=artifact_missing\tpath={Atom(path)}");
                continue;
            }
            if (!ArtifactCache.TryGetValue(path, out var fact))
            {
                using var stream = File.OpenRead(path);
                fact = (stream.Length, Convert.ToHexString(SHA256.HashData(stream)));
                ArtifactCache[path] = fact;
            }
            writer.WriteLine($"ARTIFACT\tpath={Atom(path)}\tbytes={fact.Bytes}\tsha256={fact.Sha256}");
        }
    }

    private static string? ResolveArtifact(GameInstall install, string spec)
    {
        const string compiled = "compiled/";
        if (spec.StartsWith(compiled, StringComparison.OrdinalIgnoreCase))
            return install.FindCompiledDef(spec[compiled.Length..]);
        return Path.Combine(install.DataRoot,
            spec.Replace('/', Path.DirectorySeparatorChar));
    }

    private static List<Seed> BuildSeeds(
        PeImage pe, IReadOnlyList<AsciiRow> strings, NativeEvidenceSpec spec)
    {
        var seeds = new Dictionary<uint, Seed>();
        if (spec.IncludePeEntry)
            AddSeed(seeds, pe.EntryPoint, "pe_entry_point", "pe_optional_header", null);
        foreach (var (va, hint) in spec.Probes)
            AddSeed(seeds, va, hint, "curated_address_probe", null);

        foreach (var row in strings)
        {
            if (!spec.AnchorTerms.Any(term => row.Text.Contains(term, StringComparison.OrdinalIgnoreCase)))
                continue;
            foreach (var site in FindImmediateReferences(pe, row.Va))
            {
                var start = X86.FindPrologue(pe, site);
                AddSeed(seeds, pe.Va(start), "string_xref", "ascii_anchor", row);
            }
        }
        return seeds.Values.OrderBy(seed => seed.Va).ToList();
    }

    private static void AddSeed(
        Dictionary<uint, Seed> seeds, uint va, string hint, string source, AsciiRow? anchor)
    {
        if (!seeds.TryGetValue(va, out var seed))
            seeds[va] = seed = new Seed(va);
        seed.Reasons.Add(new SeedReason(hint, source, anchor));
    }

    private static void WriteAnchors(TextWriter writer, IReadOnlyList<Seed> seeds)
    {
        var seenAnchors = new HashSet<uint>();
        foreach (var seed in seeds)
        foreach (var reason in seed.Reasons)
        {
            if (reason.Anchor is { } anchor && seenAnchors.Add(anchor.Va))
                writer.WriteLine($"STRING_ANCHOR\tva=0x{anchor.Va:X8}\ttext={Atom(anchor.Text)}");
            writer.WriteLine($"SEED\tfunction=0x{seed.Va:X8}\thint={Atom(reason.Hint)}\tsource={reason.Source}\tanchor_va={(reason.Anchor is { } a ? $"0x{a.Va:X8}" : "-")}\tanchor={Atom(reason.Anchor?.Text ?? "-")}\tconfidence=navigation_only");
        }
    }

    private static void WriteFunctions(
        TextWriter writer,
        PeImage pe,
        IReadOnlyList<Seed> seeds,
        CallIndex callIndex,
        IReadOnlyDictionary<uint, string> stringByVa,
        int maxDepth,
        int maxCallerDepth,
        int maxFunctions)
    {
        var queue = new Queue<Work>();
        foreach (var seed in seeds)
        {
            // String xrefs are useful evidence roots, but recursively expanding
            // every helper they call rapidly turns a focused system export into
            // a whole-program dump. Curated/PE roots own the direct-call closure;
            // xref-only roots are still decoded in full and retain their edges.
            var xrefOnly = seed.Reasons.All(reason => reason.Source == "ascii_anchor");
            queue.Enqueue(new Work(seed.Va, xrefOnly ? maxDepth : 0, 0, "seed"));
        }
        var queued = new HashSet<uint>(seeds.Select(seed => seed.Va));
        var functions = new SortedDictionary<uint, FunctionRow>();
        var vtables = new SortedDictionary<uint, List<uint>>();

        while (queue.Count > 0 && functions.Count < maxFunctions)
        {
            var work = queue.Dequeue();
            var at = pe.FileOffset(work.Va);
            if (at < 0 || !pe.InCode(at))
                continue;
            var steps = X86.WalkFunction(pe, at, MaxInstructions);
            if (steps.Count == 0)
                continue;
            functions[work.Va] = new FunctionRow(work, steps);

            if (work.CalleeDepth < maxDepth)
            {
                foreach (var target in steps.Where(step => step.DirectCall.HasValue)
                             .Select(step => step.DirectCall!.Value).Distinct())
                    EnqueueCode(pe, queue, queued,
                        new Work(target, work.CalleeDepth + 1, work.CallerDepth, "direct_call"));

                // Expand object vtables only when the lifecycle root itself
                // references them. Expanding vtables from every helper rapidly
                // becomes a whole-program dump and obscures the lifecycle.
                foreach (var absolute in work.CalleeDepth == 0
                             ? steps.SelectMany(step => GrepFacts.AbsValues(step.Text)).Distinct()
                             : [])
                {
                    if (!TryReadVtable(pe, absolute, out var slots))
                        continue;
                    vtables.TryAdd(absolute, slots);
                    foreach (var target in slots)
                        EnqueueCode(pe, queue, queued,
                            new Work(target, work.CalleeDepth + 1, work.CallerDepth, "referenced_vtable"));
                }
            }

            if (work.CallerDepth >= maxCallerDepth ||
                !callIndex.ByTarget.TryGetValue(work.Va, out var inbound))
                continue;
            foreach (var call in inbound)
                EnqueueCode(pe, queue, queued,
                    new Work(call.Caller, work.CalleeDepth, work.CallerDepth + 1, "direct_caller"));
        }

        foreach (var (vtbl, slots) in vtables)
        {
            writer.WriteLine($"VTABLE\taddress=0x{vtbl:X8}\tslots={slots.Count}\tdiscovery=absolute_reference\tconfidence=mechanical_candidate");
            for (var slot = 0; slot < slots.Count; slot++)
                writer.WriteLine($"VTABLE_SLOT\taddress=0x{vtbl:X8}\tslot=+{slot * 4}\ttarget=0x{slots[slot]:X8}");
        }

        foreach (var (va, row) in functions)
        {
            var inbound = callIndex.ByTarget.GetValueOrDefault(va) ?? [];
            writer.WriteLine($"FUNCTION\tfunction=0x{va:X8}\tinstructions={row.Steps.Count}\tdiscovery={row.Work.Discovery}\tcallee_depth={row.Work.CalleeDepth}\tcaller_depth={row.Work.CallerDepth}\tinbound_count={inbound.Count}");
            WriteSummary(writer, pe, va, row.Steps, stringByVa);
            foreach (var call in inbound.Take(MaxInboundRowsPerFunction))
                writer.WriteLine($"INBOUND\tfunction=0x{va:X8}\tcaller_candidate=0x{call.Caller:X8}\tsite=0x{call.Site:X8}\tconfidence=call_site_decoded;caller_start_candidate");
            if (inbound.Count > MaxInboundRowsPerFunction)
                writer.WriteLine($"INBOUND_SUMMARY\tfunction=0x{va:X8}\ttotal={inbound.Count}\temitted={MaxInboundRowsPerFunction}\tomitted={inbound.Count - MaxInboundRowsPerFunction}\treason=grep_size_cap;rerun_whole_text_call_index_for_all_sites");
            WriteSteps(writer, pe, va, row.Steps, stringByVa);
            if (row.Steps.Count >= MaxInstructions)
                writer.WriteLine($"UNRESOLVED\tkind=instruction_limit\tfunction=0x{va:X8}\tlimit={MaxInstructions}");
        }

        if (functions.Count >= maxFunctions)
            writer.WriteLine($"UNRESOLVED\tkind=function_limit\tlimit={maxFunctions}\tremaining_queue={queue.Count}");
    }

    private static void WriteSummary(
        TextWriter writer, PeImage pe, uint function, IReadOnlyList<X86.Step> steps,
        IReadOnlyDictionary<uint, string> stringByVa)
    {
        var calls = steps.Where(step => step.DirectCall.HasValue)
            .Select(step => step.DirectCall!.Value).Distinct().Order().ToArray();
        var globals = steps.SelectMany(step => GlobalMemoryRegex().Matches(step.Text).Cast<Match>())
            .Select(match => uint.Parse(match.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture))
            .Distinct().Order().ToArray();
        var offsets = steps.SelectMany(step => ObjectMemoryRegex().Matches(step.Text).Cast<Match>())
            .Select(match =>
            {
                var value = ParseOffset(match.Groups[3].Value);
                return match.Groups[2].Value == "-" ? -value : value;
            }).Distinct().Order().ToArray();
        var vcalls = steps.Select(step => VirtualCallRegex().Match(step.Text))
            .Where(match => match.Success)
            .Select(match => ParseOffset(match.Groups[2].Value)).Distinct().Order().ToArray();
        var strings = steps.SelectMany(step => GrepFacts.AbsValues(step.Text))
            .Where(stringByVa.ContainsKey).Distinct().Order()
            .Select(va => $"0x{va:X8}:{Atom(stringByVa[va])}").ToArray();
        var literals = steps.Select(step => QuotedLiteralRegex().Match(step.Text))
            .Where(match => match.Success)
            .Select(match => Atom(match.Groups[1].Value)).Distinct(StringComparer.Ordinal)
            .ToArray();
        var imports = steps.Select(step => AbsoluteIndirectRegex().Match(step.Text))
            .Where(match => match.Success)
            .Select(match => uint.Parse(match.Groups[2].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture))
            .Where(pe.Iat.ContainsKey).Distinct().Order()
            .Select(slot => $"0x{slot:X8}:{Atom(pe.Iat[slot])}").ToArray();
        writer.WriteLine(
            $"SUMMARY\tfunction=0x{function:X8}" +
            $"\tcalls={JoinHex(calls)}" +
            $"\tobject_offsets={string.Join(',', offsets.Select(value => value.ToString(CultureInfo.InvariantCulture)))}" +
            $"\tglobals={JoinHex(globals)}" +
            $"\tvcalls={string.Join(',', vcalls.Select(value => $"+{value}"))}" +
            $"\timports={string.Join('|', imports)}" +
            $"\tstrings={string.Join('|', strings)}" +
            $"\tliterals={string.Join('|', literals)}");
    }

    private static string JoinHex(IEnumerable<uint> values) =>
        string.Join(',', values.Select(value => $"0x{value:X8}"));

    private static void WriteSteps(
        TextWriter writer, PeImage pe, uint function, IReadOnlyList<X86.Step> steps,
        IReadOnlyDictionary<uint, string> stringByVa)
    {
        foreach (var step in steps)
        {
            writer.WriteLine($"INSN\tfunction=0x{function:X8}\tva=0x{step.Va:X8}\tbytes={Atom(step.Bytes)}\tasm={Atom(step.Text)}");
            var literal = QuotedLiteralRegex().Match(step.Text);
            if (literal.Success)
                writer.WriteLine($"LITERAL\tfunction=0x{function:X8}\tva=0x{step.Va:X8}\tvalue={Atom(literal.Groups[1].Value)}\tevidence={Atom(step.Text)}");
            if (step.DirectCall is uint target)
                writer.WriteLine($"CALL\tfunction=0x{function:X8}\tva=0x{step.Va:X8}\ttarget=0x{target:X8}\tconfidence=decoded_direct_call");
            if (GrepFacts.TryRelTarget(step.Text, out var branch) && step.DirectCall is null)
                writer.WriteLine($"BRANCH\tfunction=0x{function:X8}\tva=0x{step.Va:X8}\ttarget=0x{branch:X8}\top={Atom(Op(step.Text))}");

            foreach (Match match in ObjectMemoryRegex().Matches(step.Text))
            {
                var sign = match.Groups[2].Value;
                var offset = ParseOffset(match.Groups[3].Value);
                if (sign == "-") offset = -offset;
                writer.WriteLine($"MEMORY\tfunction=0x{function:X8}\tva=0x{step.Va:X8}\tbase={match.Groups[1].Value}\toffset={offset.ToString(CultureInfo.InvariantCulture)}\taccess={Access(step.Text)}\tevidence={Atom(step.Text)}");
            }

            foreach (Match match in GlobalMemoryRegex().Matches(step.Text))
            {
                var address = uint.Parse(match.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                writer.WriteLine($"GLOBAL\tfunction=0x{function:X8}\tva=0x{step.Va:X8}\taddress=0x{address:X8}\taccess={Access(step.Text)}\tevidence={Atom(step.Text)}");
            }

            var virtualCall = VirtualCallRegex().Match(step.Text);
            if (virtualCall.Success)
            {
                var offset = ParseOffset(virtualCall.Groups[2].Value);
                writer.WriteLine($"VCALL\tfunction=0x{function:X8}\tva=0x{step.Va:X8}\tbase={virtualCall.Groups[1].Value}\tslot=+{offset}\tevidence={Atom(step.Text)}");
                writer.WriteLine($"UNRESOLVED\tkind=virtual_target\tfunction=0x{function:X8}\tva=0x{step.Va:X8}\tbase={virtualCall.Groups[1].Value}\tslot=+{offset}");
            }

            var indirect = AbsoluteIndirectRegex().Match(step.Text);
            if (indirect.Success)
            {
                var address = uint.Parse(indirect.Groups[2].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                if (pe.Iat.TryGetValue(address, out var import))
                    writer.WriteLine($"IAT\tfunction=0x{function:X8}\tva=0x{step.Va:X8}\toperation={indirect.Groups[1].Value}\tslot=0x{address:X8}\timport={Atom(import)}");
                else
                    writer.WriteLine($"UNRESOLVED\tkind=absolute_indirect_target\tfunction=0x{function:X8}\tva=0x{step.Va:X8}\taddress=0x{address:X8}\tevidence={Atom(step.Text)}");
            }

            foreach (var absolute in GrepFacts.AbsValues(step.Text).Distinct())
            {
                var mapped = pe.FileOffset(absolute) >= 0 || pe.Iat.ContainsKey(absolute);
                if (!mapped)
                    continue;
                writer.WriteLine($"ABS\tfunction=0x{function:X8}\tva=0x{step.Va:X8}\taddress=0x{absolute:X8}\tstring={Atom(stringByVa.GetValueOrDefault(absolute, "-"))}\tevidence={Atom(step.Text)}");
            }
        }
    }

    private static CallIndex BuildCallIndex(PeImage pe)
    {
        var index = new CallIndex();
        foreach (var section in pe.Sections)
        {
            var start = (int)section.FileOffset;
            if (!pe.InCode(start))
                continue;
            var end = Math.Min(pe.Data.Length, (int)(section.FileOffset + section.FileSize));
            X86.WalkRange(pe, start, end, step =>
            {
                if (step.DirectCall is not uint target)
                    return true;
                var site = pe.FileOffset(step.Va);
                var callerAt = X86.FindPrologue(pe, site);
                var caller = pe.Va(callerAt);
                if (!index.ByTarget.TryGetValue(target, out var list))
                    index.ByTarget[target] = list = [];
                list.Add(new CallSite(caller, step.Va));
                return true;
            });
        }
        foreach (var list in index.ByTarget.Values)
            list.Sort((a, b) => a.Site.CompareTo(b.Site));
        return index;
    }

    private static void EnqueueCode(
        PeImage pe, Queue<Work> queue, HashSet<uint> queued, Work work)
    {
        var at = pe.FileOffset(work.Va);
        if (at < 0 || !pe.InCode(at) || !queued.Add(work.Va))
            return;
        queue.Enqueue(work);
    }

    private static bool TryReadVtable(PeImage pe, uint address, out List<uint> slots)
    {
        slots = [];
        var at = pe.FileOffset(address);
        if (at < 0 || pe.InCode(at) || (address & 3) != 0)
            return false;
        for (var slot = 0; slot < MaxVtblSlots && at + slot * 4 + 4 <= pe.Data.Length; slot++)
        {
            var target = BitConverter.ToUInt32(pe.Data, at + slot * 4);
            var targetAt = pe.FileOffset(target);
            if (targetAt < 0 || !pe.InCode(targetAt))
                break;
            slots.Add(target);
        }
        if (slots.Count >= 3)
            return true;
        slots.Clear();
        return false;
    }

    private static IReadOnlyList<int> FindImmediateReferences(PeImage pe, uint value)
    {
        var refs = new List<int>();
        var bytes = BitConverter.GetBytes(value);
        foreach (var section in pe.Sections)
        {
            var start = (int)section.FileOffset;
            if (!pe.InCode(start))
                continue;
            var end = Math.Min(pe.Data.Length - 3, (int)(section.FileOffset + section.FileSize) - 3);
            for (var at = start; at < end; at++)
            {
                if (pe.Data[at] == bytes[0] && pe.Data[at + 1] == bytes[1] &&
                    pe.Data[at + 2] == bytes[2] && pe.Data[at + 3] == bytes[3])
                    refs.Add(at);
            }
        }
        return refs;
    }

    private static List<AsciiRow> ExtractAscii(PeImage pe)
    {
        var rows = new List<AsciiRow>();
        var start = -1;
        for (var at = 0; at <= pe.Data.Length; at++)
        {
            var printable = at < pe.Data.Length && pe.Data[at] is >= 32 and <= 126;
            if (printable)
            {
                if (start < 0) start = at;
                continue;
            }
            if (start >= 0 && at - start is >= 5 and <= 512)
                rows.Add(new AsciiRow(pe.Va(start), Encoding.ASCII.GetString(pe.Data, start, at - start)));
            start = -1;
        }
        return rows;
    }

    private static string Access(string asm)
    {
        var comma = asm.IndexOf(',');
        var bracket = asm.IndexOf('[');
        if (bracket < 0)
            return "unknown";
        var op = Op(asm);
        if (op is "cmp" or "test" or "push" or "call" or "jmp" or "fld" or "fild")
            return "read";
        if (op is "inc" or "dec" or "fstp" || (comma > bracket && bracket < comma))
            return "write_or_readmodifywrite";
        return "read";
    }

    private static string Op(string asm)
    {
        var space = asm.IndexOf(' ');
        return space < 0 ? asm : asm[..space];
    }

    private static int ParseOffset(string text) => text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
        ? int.Parse(text.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
        : int.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);

    private static string ResolveOutput(string[] args, string option, string defaultFile)
    {
        for (var i = 0; i + 1 < args.Length; i++)
            if (args[i] == option)
                return Path.GetFullPath(args[i + 1]);
        return Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "out", defaultFile));
    }

    private static int ReadIntOption(
        string[] args, string option, int fallback, int min, int max)
    {
        for (var i = 0; i + 1 < args.Length; i++)
            if (args[i] == option && int.TryParse(args[i + 1], NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out var value))
                return Math.Clamp(value, min, max);
        return fallback;
    }

    private static string Atom(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("\t", "\\t", StringComparison.Ordinal)
        .Replace("\r", "\\r", StringComparison.Ordinal)
        .Replace("\n", "\\n", StringComparison.Ordinal);

    private sealed class Seed(uint va)
    {
        public uint Va { get; } = va;
        public List<SeedReason> Reasons { get; } = [];
    }

    private sealed class CallIndex
    {
        public Dictionary<uint, List<CallSite>> ByTarget { get; } = [];
    }

    private readonly record struct SeedReason(string Hint, string Source, AsciiRow? Anchor);
    private readonly record struct AsciiRow(uint Va, string Text);
    private readonly record struct CallSite(uint Caller, uint Site);
    private readonly record struct Work(uint Va, int CalleeDepth, int CallerDepth, string Discovery);
    private readonly record struct FunctionRow(Work Work, IReadOnlyList<X86.Step> Steps);

    [GeneratedRegex(@"\[(e(?:ax|cx|dx|bx|sp|bp|si|di))([+-])((?:0x)?[0-9A-Fa-f]+)\]", RegexOptions.CultureInvariant)]
    private static partial Regex ObjectMemoryRegex();

    [GeneratedRegex(@"\[0x([0-9A-Fa-f]+)\]", RegexOptions.CultureInvariant)]
    private static partial Regex GlobalMemoryRegex();

    [GeneratedRegex(@"^call \[(e(?:ax|cx|dx|bx|sp|bp|si|di))\+((?:0x)?[0-9A-Fa-f]+)\]$", RegexOptions.CultureInvariant)]
    private static partial Regex VirtualCallRegex();

    [GeneratedRegex(@"^(call|jmp) \[0x([0-9A-Fa-f]+)\]$", RegexOptions.CultureInvariant)]
    private static partial Regex AbsoluteIndirectRegex();

    [GeneratedRegex("\\\"([^\\\"]+)\\\"", RegexOptions.CultureInvariant)]
    private static partial Regex QuotedLiteralRegex();
}
