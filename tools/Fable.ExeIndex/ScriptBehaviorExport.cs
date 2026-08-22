using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Fable.Core;
using Fable.Formats.Defs;
using Fable.Formats.Qst;

namespace Fable.ExeIndex;

/// <summary>
/// Produces one grep-oriented file containing script.bin, quest declarations,
/// the script/quest VM, Gameflow, New Game, and the native region handoff.
/// Facts remain mechanical: decoded instructions are not promoted to guessed
/// gameplay semantics.
/// </summary>
internal static partial class ScriptBehaviorExport
{
    private const int DefaultDepth = 4;
    private const int DefaultMaxFunctions = 12000;
    private const int MaxInstructions = 3000;

    private static readonly (uint Va, string Label)[] RuntimeSeeds =
    [
        (0x00416953, "game_load_world"),
        (0x004189C2, "game_pump"),
        (0x004B4260, "quest_manager_construct_initial"),
        (0x004B4490, "quest_manager_pump"),
        (0x004B4A10, "activate_quest_list"),
        (0x004B4AA0, "give_quest_from_card"),
        (0x006E75C0, "script_manager_pump"),
        (0x00892E80, "activate_quest"),
        (0x00893570, "is_quest_active"),
        (0x008968C0, "give_quest_card"),
        (0x00A446A0, "fiber_entry"),
        (0x00A44880, "fiber_tick"),
        (0x00CB5C90, "bind_quest_factory"),
        (0x00CB5D80, "register_scripts"),
        (0x00CB7900, "quest_construct_hook"),
        (0x00CB7C40, "quest_list_walk"),
        (0x00CB8220, "quest_list_pump"),
        (0x00CBFB7D, "cutscene_interpreter"),
        (0x00CD52D0, "quest_factory_table"),
        (0x00CE6CF0, "gameflow_seed"),
        (0x00CE7670, "gameflow_run"),
        (0x00DAAC00, "new_oakvale_intro_ctor"),
        (0x00DABAC0, "new_oakvale_intro_run"),
        (0x00DBDE40, "start_oakvale_setup"),
        (0x00DBEF70, "new_oakvale_intro_factory"),
        (0x00487C20, "load_region_by_name"),
        (0x004FB150, "get_current_region_index"),
        (0x004FC180, "get_region_record"),
        (0x004FC8A0, "set_region_as_loaded"),
        (0x00500540, "request_load_region"),
        (0x00501450, "load_region_catalog"),
        (0x006C1BE0, "apply_level_load_job"),
        (0x006C20A0, "level_loader_has_work"),
        (0x006C2710, "level_loader_update"),
    ];

    private static readonly (uint Va, string Label, int MaxSlots)[] Vtables =
    [
        (0x01260F0C, "script_context", 420),
        (0x012C1648, "quest_base", 32),
        (0x012C3FA4, "gameflow", 40),
        (0x012D7A28, "new_oakvale_intro", 32),
        (0x012D7A3C, "new_oakvale_watcher", 32),
    ];

    public static void Run(PeImage pe, GameInstall install, string[] args)
    {
        var output = ResolveOutput(args);
        var depth = ReadIntOption(args, "--max-depth", DefaultDepth, 0, 10);
        var maxFunctions = ReadIntOption(args, "--max-functions", DefaultMaxFunctions, 100, 30000);
        var namesPath = install.FindCompiledDef("names.bin")
            ?? throw new FileNotFoundException("names.bin not found");
        var scriptPath = install.FindCompiledDef("script.bin")
            ?? throw new FileNotFoundException("script.bin not found");
        var names = NamesBin.Load(namesPath);
        var script = GameBin.Load(scriptPath, names);
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);

        using var writer = new StreamWriter(output, false, new UTF8Encoding(false))
        {
            NewLine = "\n",
        };
        WriteHeader(writer, pe, scriptPath, depth, maxFunctions);
        WriteEntries(writer, script);
        WriteQuestFiles(writer, install);
        var labels = WriteVtables(writer, pe);
        WriteFunctions(writer, pe, labels, depth, maxFunctions);
        writer.WriteLine("END\tformat=FABLE_SCRIPT_GREP_V1");
        Console.WriteLine($"script {output}");
    }

    private static void WriteHeader(
        TextWriter writer, PeImage pe, string scriptPath, int depth, int maxFunctions)
    {
        writer.WriteLine("# FABLE_SCRIPT_GREP_V1 - generated evidence; one fact per line");
        writer.WriteLine("# rg '^ENTRY.*S_Q' <file>");
        writer.WriteLine("# rg 'function=0x00CE7670|target=0x008968C0' <file>");
        writer.WriteLine("# rg 'slot=+1180|Q_NewOakValeIntro|StartOakVale' <file>");
        writer.WriteLine("# Records: META ENTRY STRING SUBDEF RAW QST VTABLE FUNCTION CALL INSN MEMORY VCALL ABS UNRESOLVED END");
        writer.WriteLine($"META\tformat=FABLE_SCRIPT_GREP_V1\texe_id={pe.Identity}\tscript={Atom(scriptPath)}\tcall_depth={depth}\tmax_functions={maxFunctions}");
        writer.WriteLine("META\ttruth=INSN_is_disassembly;MEMORY_VCALL_ABS_are_mechanical;labels_are_navigation_hints;UNRESOLVED_is_not_implemented_semantics");
    }

    private static void WriteEntries(TextWriter writer, GameBin script)
    {
        foreach (var entry in script.Entries)
        {
            var name = entry.InstanceName ?? entry.SourceName ?? $"entry_{entry.Index}";
            writer.WriteLine($"ENTRY\tindex={entry.Index}\ttype={Atom(entry.TypeName ?? "-")}\tname={Atom(name)}\tsource={Atom(entry.SourceName ?? "-")}\treal={entry.IsReal}\ttemplate={entry.IsTemplate}\tbody_offset=0x{entry.BodyOffset:X}\traw_bytes={entry.Raw.Length}\tunknown0={entry.Unknown0}");
            foreach (var sub in entry.SubDefs)
            {
                var target = (uint)sub.DefIndex < (uint)script.Entries.Count
                    ? script.Entries[sub.DefIndex].InstanceName ?? script.Entries[sub.DefIndex].SourceName ?? $"entry_{sub.DefIndex}"
                    : "<invalid-index>";
                writer.WriteLine($"SUBDEF\towner_index={entry.Index}\towner={Atom(name)}\tname_crc=0x{sub.NameCrc:X8}\ttarget_index={sub.DefIndex}\ttarget={Atom(target)}");
            }

            foreach (var (offset, value) in ExtractAscii(entry.Raw))
                writer.WriteLine($"STRING\tentry_index={entry.Index}\tentry={Atom(name)}\toffset=0x{offset:X}\tvalue={Atom(value)}");
            for (var offset = 0; offset < entry.Raw.Length; offset += 32)
            {
                var count = Math.Min(32, entry.Raw.Length - offset);
                writer.WriteLine($"RAW\tentry_index={entry.Index}\tentry={Atom(name)}\toffset=0x{offset:X}\thex={Convert.ToHexString(entry.Raw, offset, count)}");
            }
        }
    }

    private static void WriteQuestFiles(TextWriter writer, GameInstall install)
    {
        foreach (var path in new[] { install.QuestPath, install.GlobalQuestPath })
        {
            if (!File.Exists(path))
            {
                writer.WriteLine($"UNRESOLVED\tkind=quest_file_missing\tpath={Atom(path)}");
                continue;
            }
            var questFile = QuestFile.Load(path);
            foreach (var quest in questFile.Quests)
                writer.WriteLine($"QST\tfile={Atom(Path.GetFileName(path))}\tname={Atom(quest.Name)}\tpersistent={quest.Persistent}");
            foreach (var quest in questFile.TestQuests)
                writer.WriteLine($"QST_TEST\tfile={Atom(Path.GetFileName(path))}\tname={Atom(quest.Name)}\tstart_holy_site={Atom(quest.StartHolySite)}\tkind={quest.Kind}\tdescription={Atom(quest.Description)}\tini={Atom(quest.IniFile)}\tend_script={Atom(quest.EndScript)}\tquest_card={Atom(quest.QuestCard)}");
        }
    }

    private static Dictionary<uint, string> WriteVtables(TextWriter writer, PeImage pe)
    {
        var labels = RuntimeSeeds.ToDictionary(x => x.Va, x => x.Label);
        foreach (var (vtbl, label, maxSlots) in Vtables)
        {
            for (var slot = 0; slot < maxSlots; slot++)
            {
                var at = pe.FileOffset(vtbl + (uint)(slot * 4));
                if (at < 0 || at + 4 > pe.Data.Length)
                    break;
                var target = BitConverter.ToUInt32(pe.Data, at);
                var code = pe.FileOffset(target);
                if (code < 0 || !pe.InCode(code))
                    break;
                var targetLabel = $"{label}_vtbl_slot_{slot * 4}";
                labels.TryAdd(target, targetLabel);
                writer.WriteLine($"VTABLE\tname={label}\tvtbl=0x{vtbl:X8}\tslot=+{slot * 4}\ttarget=0x{target:X8}\ttarget_label={targetLabel}");
            }
        }
        return labels;
    }

    private static void WriteFunctions(
        TextWriter writer, PeImage pe, Dictionary<uint, string> labels,
        int maxDepth, int maxFunctions)
    {
        var queue = new Queue<(uint Va, int Depth)>();
        foreach (var seed in labels.Keys.Order())
            queue.Enqueue((seed, 0));
        var seen = new HashSet<uint>();
        var functions = new SortedDictionary<uint, List<X86.Step>>();
        while (queue.Count > 0 && functions.Count < maxFunctions)
        {
            var (va, depth) = queue.Dequeue();
            if (!seen.Add(va))
                continue;
            var at = pe.FileOffset(va);
            if (at < 0 || !pe.InCode(at))
                continue;
            var steps = X86.WalkFunction(pe, at, MaxInstructions);
            if (steps.Count == 0)
                continue;
            functions[va] = steps;
            if (depth >= maxDepth)
                continue;
            foreach (var target in steps.Where(s => s.DirectCall.HasValue)
                         .Select(s => s.DirectCall!.Value).Distinct())
            {
                var targetAt = pe.FileOffset(target);
                if (targetAt >= 0 && pe.InCode(targetAt))
                    queue.Enqueue((target, depth + 1));
            }
        }

        var callers = new Dictionary<uint, List<uint>>();
        foreach (var (va, steps) in functions)
        foreach (var target in steps.Where(s => s.DirectCall.HasValue)
                     .Select(s => s.DirectCall!.Value).Distinct())
        {
            if (!callers.TryGetValue(target, out var list))
                callers[target] = list = [];
            list.Add(va);
        }

        foreach (var (va, steps) in functions)
        {
            var from = callers.TryGetValue(va, out var values)
                ? string.Join(',', values.Order().Select(v => $"0x{v:X8}"))
                : "-";
            writer.WriteLine($"FUNCTION\tfunction=0x{va:X8}\tlabel={Atom(labels.GetValueOrDefault(va, "reachable_helper"))}\tinstructions={steps.Count}\tcallers={from}");
            foreach (var step in steps)
            {
                writer.WriteLine($"INSN\tfunction=0x{va:X8}\tva=0x{step.Va:X8}\tbytes={Atom(step.Bytes)}\tasm={Atom(step.Text)}");
                if (step.DirectCall is uint target)
                    writer.WriteLine($"CALL\tfunction=0x{va:X8}\tva=0x{step.Va:X8}\ttarget=0x{target:X8}\ttarget_label={Atom(labels.GetValueOrDefault(target, "reachable_helper"))}");
                foreach (Match match in MemoryRegex().Matches(step.Text))
                    writer.WriteLine($"MEMORY\tfunction=0x{va:X8}\tva=0x{step.Va:X8}\tbase={match.Groups[1].Value}\toffset=+{match.Groups[2].Value}\tevidence={Atom(step.Text)}");
                var virtualCall = VirtualCallRegex().Match(step.Text);
                if (virtualCall.Success)
                    writer.WriteLine($"VCALL\tfunction=0x{va:X8}\tva=0x{step.Va:X8}\tbase={virtualCall.Groups[1].Value}\tslot=+{virtualCall.Groups[2].Value}\tevidence={Atom(step.Text)}");
                foreach (var absolute in GrepFacts.AbsValues(step.Text))
                {
                    var value = TryAscii(pe, absolute);
                    writer.WriteLine($"ABS\tfunction=0x{va:X8}\tva=0x{step.Va:X8}\taddress=0x{absolute:X8}\tstring={Atom(value ?? "-")}\tevidence={Atom(step.Text)}");
                }
            }
        }
        if (functions.Count >= maxFunctions)
            writer.WriteLine($"UNRESOLVED\tkind=function_limit\tlimit={maxFunctions}\tsuggestion=pass_--max-functions_or_reduce_--max-depth");
    }

    private static string? TryAscii(PeImage pe, uint va)
    {
        var at = pe.FileOffset(va);
        if (at < 0 || at >= pe.Data.Length || pe.Data[at] is < 32 or > 126)
            return null;
        var end = at;
        while (end < pe.Data.Length && pe.Data[end] is >= 32 and <= 126 && end - at < 512)
            end++;
        return end - at >= 3 ? Encoding.ASCII.GetString(pe.Data, at, end - at) : null;
    }

    private static List<(int Offset, string Value)> ExtractAscii(byte[] raw)
    {
        var values = new List<(int, string)>();
        for (var i = 0; i < raw.Length;)
        {
            if (raw[i] is < 32 or > 126) { i++; continue; }
            var start = i;
            while (i < raw.Length && raw[i] is >= 32 and <= 126) i++;
            if (i - start >= 4)
                values.Add((start, Encoding.ASCII.GetString(raw, start, i - start)));
        }
        return values;
    }

    private static string ResolveOutput(string[] args)
    {
        for (var i = 0; i + 1 < args.Length; i++)
            if (args[i] == "--script-out")
                return Path.GetFullPath(args[i + 1]);
        return Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "out", "script-behavior-grep.txt"));
    }

    private static int ReadIntOption(
        string[] args, string option, int fallback, int min, int max)
    {
        for (var i = 0; i + 1 < args.Length; i++)
            if (args[i] == option && int.TryParse(
                    args[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                return Math.Clamp(value, min, max);
        return fallback;
    }

    private static string Atom(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("\t", "\\t", StringComparison.Ordinal)
        .Replace("\r", "\\r", StringComparison.Ordinal)
        .Replace("\n", "\\n", StringComparison.Ordinal);

    [GeneratedRegex(@"\[(e(?:ax|cx|dx|bx|sp|bp|si|di))\+(\d+)\]", RegexOptions.CultureInvariant)]
    private static partial Regex MemoryRegex();

    [GeneratedRegex(@"^call \[(e(?:ax|cx|dx|bx|sp|bp|si|di))\+(\d+)\]$", RegexOptions.CultureInvariant)]
    private static partial Regex VirtualCallRegex();
}
