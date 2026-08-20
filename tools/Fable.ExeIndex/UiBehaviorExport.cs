using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Fable.Core;
using Fable.Formats.Defs;

namespace Fable.ExeIndex;

/// <summary>
/// Exports frontend.bin and the native UI code reachable from its widget
/// constructors as one grep-oriented evidence file. This is deliberately a
/// line protocol: an AI or a human can use rg without loading a JSON graph.
/// </summary>
internal static partial class UiBehaviorExport
{
    private const int DefaultDepth = 3;
    private const int DefaultMaxFunctions = 5000;
    private const int MaxInstructions = 2500;

    private static readonly uint[] RuntimeSeeds =
    [
        FrontendWidgetType.ConstructFn,
        FrontendWidgetType.FactoryFn,
        FrontendWidgetType.ChildAttachFn,
        FrontendWidgetType.ContainerDrawFn,
        FrontendWidgetType.SelectStateFn,
        FrontendWidgetType.StyleTickFn,
        0x0042DB40, // frontend initialization
        0x0042DC94, // frontend update
        0x0042DF9E, // frontend draw
        0x0055B8F0, // type 11/38 AABB hit test
        0x0055BF10, // hover selection
        0x00631C60, // CUIDef persist
    ];

    public static void Run(PeImage pe, GameInstall install, string[] args)
    {
        var output = ResolveOutput(args);
        var depth = ReadIntOption(args, "--max-depth", DefaultDepth, 0, 8);
        var maxFunctions = ReadIntOption(args, "--max-functions", DefaultMaxFunctions, 100, 20000);
        var namesPath = install.FindCompiledDef("names.bin")
            ?? throw new FileNotFoundException("names.bin not found");
        var frontendPath = install.FindCompiledDef("frontend.bin")
            ?? throw new FileNotFoundException("frontend.bin not found");
        var names = NamesBin.Load(namesPath);
        var bin = GameBin.Load(frontendPath, names);
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);

        using var writer = new StreamWriter(output, false, new UTF8Encoding(false))
        {
            NewLine = "\n",
        };
        WriteHeader(writer, pe, frontendPath, depth, maxFunctions);
        WriteFieldCatalog(writer);
        WriteWidgetTypes(writer, pe);
        WriteWidgets(writer, bin);
        WriteFunctions(writer, pe, depth, maxFunctions);
        writer.WriteLine("END\tformat=FABLE_UI_GREP_V1");
        Console.WriteLine($"ui   {output}");
    }

    private static void WriteHeader(TextWriter writer, PeImage pe, string frontendPath, int depth, int maxFunctions)
    {
        writer.WriteLine("# FABLE_UI_GREP_V1 - generated evidence; one fact per line");
        writer.WriteLine("# grep examples:");
        writer.WriteLine("# rg '^WIDGET.*UI_SLIDER_CAMERA_SENSITIVITY' <file>");
        writer.WriteLine("# rg 'field=Slider(Left|Right)' <file>");
        writer.WriteLine("# rg 'function=0x00549B20|target=0x00549B20' <file>");
        writer.WriteLine("# Records: FIELD WIDGET_TYPE VTABLE WIDGET VALUE REL FUNCTION CALL INSN PSEUDO UNRESOLVED");
        writer.WriteLine($"META\tformat=FABLE_UI_GREP_V1\texe_id={pe.Identity}\tfrontend={Atom(frontendPath)}\tcall_depth={depth}\tmax_functions={maxFunctions}");
        writer.WriteLine("META\ttruth=INSN_is_disassembly;PSEUDO_is_mechanical_annotation;UNRESOLVED_is_not_implemented_semantics");
    }

    private static void WriteFieldCatalog(TextWriter writer)
    {
        foreach (var field in FrontendUiFieldCatalog.Fields)
            WriteField(writer, "definition", field);
        foreach (var field in FrontendUiFieldCatalog.StateFields)
            WriteField(writer, "style", field);
    }

    private static void WriteField(TextWriter writer, string owner, FrontendUiFieldCatalog.Field field) =>
        writer.WriteLine($"FIELD\towner={owner}\tname={field.Name}\tcrc=0x{field.Crc:X8}\tstorage={field.SerializedAs}\tretail_offset=+{field.RetailOffset}\tdonor_offset=+{field.DonorOffset}\tconfidence=verified");

    private static void WriteWidgetTypes(TextWriter writer, PeImage pe)
    {
        foreach (var info in FrontendWidgetType.Table)
        {
            var vtbl = ResolveVtbl(pe, info);
            writer.WriteLine($"WIDGET_TYPE\ttype={info.Type}\trole={Atom(info.Role ?? "unclassified")}\tctor=0x{info.Ctor:X8}\tsize=0x{info.Size:X}\tvtbl=0x{vtbl:X8}\tconfidence={(info.Ctor == 0 ? "unresolved" : "verified")}");
            if (vtbl == 0)
                continue;
            for (var slot = 0; slot < 160; slot++)
            {
                var offset = pe.FileOffset(vtbl + (uint)(slot * 4));
                if (offset < 0 || offset + 4 > pe.Data.Length)
                    break;
                var target = BitConverter.ToUInt32(pe.Data, offset);
                var code = pe.FileOffset(target);
                if (code < 0 || !pe.InCode(code))
                    break;
                writer.WriteLine($"VTABLE\ttype={info.Type}\trole={Atom(info.Role ?? "unclassified")}\tvtbl=0x{vtbl:X8}\tslot=+{slot * 4}\ttarget=0x{target:X8}");
            }
        }
    }

    private static void WriteWidgets(TextWriter writer, GameBin bin)
    {
        var byIndex = new Dictionary<int, string>();
        for (var i = 0; i < bin.Entries.Count; i++)
            byIndex[i] = bin.Entries[i].InstanceName ?? bin.Entries[i].SourceName ?? $"entry_{i}";

        for (var index = 0; index < bin.Entries.Count; index++)
        {
            var entry = bin.Entries[index];
            if (!string.Equals(entry.TypeName, "UI", StringComparison.Ordinal))
                continue;
            var def = FrontendUiDef.TryParse(entry);
            if (def is null)
            {
                writer.WriteLine($"UNRESOLVED\tkind=widget_parse\tindex={index}\tname={Atom(byIndex[index])}");
                continue;
            }

            writer.WriteLine($"WIDGET\tindex={index}\tname={Atom(def.InstanceName)}\ttype={def.Type}\tctor=0x{FrontendWidgetType.Ctor(def.Type):X8}\tchildren={def.ChildIndices.Count}\tschema_complete={def.SchemaComplete}\tschema_error={Atom(def.SchemaError ?? "-")}");
            foreach (var child in def.ChildIndices)
                writer.WriteLine($"REL\tkind=child\tfrom_index={index}\tfrom={Atom(def.InstanceName)}\tto_index={child}\tto={Atom(NameAt(byIndex, child))}\tconfidence=verified");

            var facts = ScanFields(entry.Raw, entry.BodyOffset > 0 ? entry.BodyOffset : FrontendUiDef.HeaderBytes);
            foreach (var fact in facts)
            {
                writer.WriteLine($"VALUE\twidget_index={index}\twidget={Atom(def.InstanceName)}\tfield={fact.Field.Name}\tcrc=0x{fact.Field.Crc:X8}\toccurrence={fact.Occurrence}\traw_offset=0x{fact.Offset:X}\tstorage={fact.Field.SerializedAs}\tvalue={Atom(fact.Value)}");
                if (fact.Field.Name is "SliderLeft" or "SliderRight" or "DownArrow" or "UpArrow" &&
                    int.TryParse(fact.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target) && target != 0)
                    writer.WriteLine($"REL\tkind={fact.Field.Name}\tfrom_index={index}\tfrom={Atom(def.InstanceName)}\tto_index={target}\tto={Atom(NameAt(byIndex, target))}\tconfidence=verified");
            }
        }
    }

    private static List<FieldFact> ScanFields(byte[] raw, int start)
    {
        var catalog = FrontendUiFieldCatalog.Fields.Concat(FrontendUiFieldCatalog.StateFields)
            .GroupBy(f => f.Crc).ToDictionary(g => g.Key, g => g.First());
        var occurrences = new Dictionary<uint, int>();
        var facts = new List<FieldFact>();
        for (var offset = Math.Max(0, start); offset + 4 <= raw.Length; offset++)
        {
            var crc = BitConverter.ToUInt32(raw, offset);
            if (!catalog.TryGetValue(crc, out var field))
                continue;
            var occurrence = occurrences.GetValueOrDefault(crc);
            occurrences[crc] = occurrence + 1;
            facts.Add(new FieldFact(field, offset, occurrence, ReadValue(raw, offset + 4, field.SerializedAs)));
        }
        return facts;
    }

    private static string ReadValue(byte[] raw, int payload, FrontendUiFieldCatalog.Storage storage)
    {
        if (payload >= raw.Length)
            return "<truncated>";
        try
        {
            return storage switch
            {
                FrontendUiFieldCatalog.Storage.Int32 => I32(raw, payload).ToString(CultureInfo.InvariantCulture),
                FrontendUiFieldCatalog.Storage.Float32 => BitConverter.ToSingle(raw, payload).ToString("R", CultureInfo.InvariantCulture),
                FrontendUiFieldCatalog.Storage.Bool8 => raw[payload] == 0 ? "false" : "true",
                FrontendUiFieldCatalog.Storage.Utf16 => ReadUtf16(raw, payload),
                FrontendUiFieldCatalog.Storage.Int32Vector => ReadVector(raw, payload, false, false),
                FrontendUiFieldCatalog.Storage.Float32Vector => ReadVector(raw, payload, true, false),
                FrontendUiFieldCatalog.Storage.Int32PairMap => ReadVector(raw, payload, false, true),
                FrontendUiFieldCatalog.Storage.Int32CStringMap => $"count={I32(raw, payload)}",
                FrontendUiFieldCatalog.Storage.States => I32(raw, payload).ToString(CultureInfo.InvariantCulture),
                _ => "<unsupported>",
            };
        }
        catch (ArgumentOutOfRangeException)
        {
            return "<truncated>";
        }
    }

    private static string ReadVector(byte[] raw, int payload, bool floats, bool pairs)
    {
        var count = I32(raw, payload);
        if (count is < 0 or > 4096)
            return $"count={count};invalid";
        var stride = pairs ? 8 : 4;
        if ((long)payload + 4L + (long)count * stride > raw.Length)
            return $"count={count};truncated";
        var values = new StringBuilder().Append("count=").Append(count).Append(";items=[");
        for (var i = 0; i < count; i++)
        {
            if (i != 0) values.Append(',');
            var at = payload + 4 + i * stride;
            if (pairs)
                values.Append(I32(raw, at)).Append(':').Append(I32(raw, at + 4));
            else if (floats)
                values.Append(BitConverter.ToSingle(raw, at).ToString("R", CultureInfo.InvariantCulture));
            else
                values.Append(I32(raw, at));
        }
        return values.Append(']').ToString();
    }

    private static string ReadUtf16(byte[] raw, int payload)
    {
        var end = payload;
        while (end + 1 < raw.Length && (raw[end] != 0 || raw[end + 1] != 0))
            end += 2;
        return Encoding.Unicode.GetString(raw, payload, Math.Max(0, end - payload));
    }

    private static int I32(byte[] raw, int offset)
    {
        if (offset < 0 || offset + 4 > raw.Length)
            throw new ArgumentOutOfRangeException(nameof(offset));
        return BitConverter.ToInt32(raw, offset);
    }

    private static void WriteFunctions(TextWriter writer, PeImage pe, int maxDepth, int maxFunctions)
    {
        var labels = SeedLabels(pe);
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
            var offset = pe.FileOffset(va);
            if (offset < 0 || !pe.InCode(offset))
                continue;
            var steps = X86.WalkFunction(pe, offset, MaxInstructions);
            if (steps.Count == 0)
                continue;
            functions[va] = steps;
            if (depth >= maxDepth)
                continue;
            foreach (var call in steps.Select(s => s.DirectCall).Where(v => v.HasValue).Select(v => v!.Value).Distinct())
            {
                var callOffset = pe.FileOffset(call);
                if (callOffset >= 0 && pe.InCode(callOffset))
                    queue.Enqueue((call, depth + 1));
            }
        }

        var callers = new Dictionary<uint, List<uint>>();
        foreach (var (va, steps) in functions)
        foreach (var target in steps.Select(s => s.DirectCall).Where(v => v.HasValue).Select(v => v!.Value).Distinct())
        {
            if (!callers.TryGetValue(target, out var list))
                callers[target] = list = [];
            list.Add(va);
        }

        foreach (var (va, steps) in functions)
        {
            labels.TryGetValue(va, out var label);
            var callerText = callers.TryGetValue(va, out var from)
                ? string.Join(',', from.Order().Select(v => $"0x{v:X8}")) : "-";
            writer.WriteLine($"FUNCTION\tfunction=0x{va:X8}\tlabel={Atom(label ?? "reachable_helper")}\tinstructions={steps.Count}\tcallers={callerText}");
            for (var i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                writer.WriteLine($"INSN\tfunction=0x{va:X8}\tva=0x{step.Va:X8}\tbytes={Atom(step.Bytes)}\tasm={Atom(step.Text)}");
                if (step.DirectCall is uint target)
                    writer.WriteLine($"CALL\tfunction=0x{va:X8}\tva=0x{step.Va:X8}\ttarget=0x{target:X8}\ttarget_label={Atom(labels.GetValueOrDefault(target, "reachable_helper"))}");
                WritePseudo(writer, va, steps, i);
            }
        }
        if (functions.Count >= maxFunctions)
            writer.WriteLine($"UNRESOLVED\tkind=function_limit\tlimit={maxFunctions}\tsuggestion=pass_--max-functions_or_reduce_--max-depth");
    }

    private static void WritePseudo(TextWriter writer, uint function, IReadOnlyList<X86.Step> steps, int index)
    {
        var step = steps[index];
        foreach (Match match in MemoryOffsetRegex().Matches(step.Text))
        {
            if (!int.TryParse(match.Groups[1].Value, out var offset))
                continue;
            var fields = FrontendUiFieldCatalog.Fields.Where(f => f.RetailOffset == offset)
                .Select(f => f.Name).ToArray();
            writer.WriteLine($"PSEUDO\tfunction=0x{function:X8}\tva=0x{step.Va:X8}\tkind=memory_access\toffset=+{offset}\tfield_candidates={Atom(fields.Length == 0 ? "unresolved" : string.Join(',', fields))}\tevidence={Atom(step.Text)}");
        }

        if (index + 1 >= steps.Count)
            return;
        var read = DefinitionReadRegex().Match(step.Text);
        var write = WidgetWriteRegex().Match(steps[index + 1].Text);
        if (!read.Success || !write.Success || read.Groups[1].Value != write.Groups[2].Value)
            return;
        var sourceOffset = int.Parse(read.Groups[2].Value, CultureInfo.InvariantCulture);
        var targetOffset = int.Parse(write.Groups[1].Value, CultureInfo.InvariantCulture);
        var candidates = FrontendUiFieldCatalog.Fields.Where(f => f.RetailOffset == sourceOffset)
            .Select(f => f.Name).ToArray();
        writer.WriteLine($"PSEUDO\tfunction=0x{function:X8}\tva=0x{step.Va:X8}\tkind=copy_definition_to_widget\tdefinition_offset=+{sourceOffset}\twidget_offset=+{targetOffset}\tfield_candidates={Atom(candidates.Length == 0 ? "unresolved" : string.Join(',', candidates))}\tevidence={Atom(step.Text + " ; " + steps[index + 1].Text)}");
    }

    private static Dictionary<uint, string> SeedLabels(PeImage pe)
    {
        var labels = RuntimeSeeds.Distinct().ToDictionary(v => v, v => v switch
        {
            FrontendWidgetType.ConstructFn => "widget_factory_switch",
            FrontendWidgetType.FactoryFn => "widget_factory_entry",
            FrontendWidgetType.ChildAttachFn => "attach_definition_children",
            FrontendWidgetType.ContainerDrawFn => "draw_child_list",
            FrontendWidgetType.SelectStateFn => "select_state",
            FrontendWidgetType.StyleTickFn => "tick_style",
            0x0042DB40 => "frontend_initialize",
            0x0042DC94 => "frontend_update",
            0x0042DF9E => "frontend_draw",
            0x0055B8F0 => "button_hit_test",
            0x0055BF10 => "button_hover_select",
            0x00631C60 => "frontend_definition_persist",
            _ => "ui_runtime_seed",
        });
        foreach (var info in FrontendWidgetType.Table.Where(i => i.Ctor != 0))
        {
            labels[info.Ctor] = $"widget_type_{info.Type}_ctor_{info.Role ?? "unclassified"}";
            var vtbl = ResolveVtbl(pe, info);
            if (vtbl == 0)
                continue;
            for (var slot = 0; slot < 160; slot++)
            {
                var offset = pe.FileOffset(vtbl + (uint)(slot * 4));
                if (offset < 0 || offset + 4 > pe.Data.Length)
                    break;
                var target = BitConverter.ToUInt32(pe.Data, offset);
                var code = pe.FileOffset(target);
                if (code < 0 || !pe.InCode(code))
                    break;
                labels.TryAdd(target, $"widget_type_{info.Type}_vtbl_slot_{slot * 4}");
            }
        }
        return labels;
    }

    /// <summary>Use the catalogued vtable, or recover the immediate assigned by the ctor.</summary>
    private static uint ResolveVtbl(PeImage pe, FrontendWidgetTypeInfo info)
    {
        if (info.Vtbl != 0)
            return info.Vtbl;
        if (info.Ctor == 0)
            return 0;
        var offset = pe.FileOffset(info.Ctor);
        if (offset < 0)
            return 0;
        foreach (var step in X86.WalkFunction(pe, offset, 100))
        {
            if (!step.Text.StartsWith("mov [", StringComparison.Ordinal))
                continue;
            foreach (var value in GrepFacts.AbsValues(step.Text))
            {
                var tableOffset = pe.FileOffset(value);
                if (tableOffset < 0 || tableOffset + 4 > pe.Data.Length)
                    continue;
                var firstTarget = BitConverter.ToUInt32(pe.Data, tableOffset);
                var code = pe.FileOffset(firstTarget);
                if (code >= 0 && pe.InCode(code))
                    return value;
            }
        }
        return 0;
    }

    private static string ResolveOutput(string[] args)
    {
        for (var i = 0; i + 1 < args.Length; i++)
            if (args[i] == "--ui-out")
                return Path.GetFullPath(args[i + 1]);
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "out", "frontend-ui-grep.txt"));
    }

    private static int ReadIntOption(string[] args, string option, int fallback, int min, int max)
    {
        for (var i = 0; i + 1 < args.Length; i++)
            if (args[i] == option && int.TryParse(args[i + 1], out var value))
                return Math.Clamp(value, min, max);
        return fallback;
    }

    private static string NameAt(IReadOnlyDictionary<int, string> names, int index) =>
        names.TryGetValue(index, out var name) ? name : "<invalid-index>";

    internal static string Atom(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("\t", "\\t", StringComparison.Ordinal)
        .Replace("\r", "\\r", StringComparison.Ordinal)
        .Replace("\n", "\\n", StringComparison.Ordinal);

    private sealed record FieldFact(FrontendUiFieldCatalog.Field Field, int Offset, int Occurrence, string Value);

    internal static IReadOnlyList<(string Field, string Value)> ScanFieldValuesForTest(byte[] raw, int start) =>
        ScanFields(raw, start).Select(f => (f.Field.Name, f.Value)).ToArray();

    [GeneratedRegex(@"\[e(?:ax|cx|dx|bx|sp|bp|si|di)\+(\d+)\]", RegexOptions.CultureInvariant)]
    private static partial Regex MemoryOffsetRegex();

    [GeneratedRegex(@"^mov (e(?:ax|cx|dx|bx|sp|bp|si|di)), \[e(?:ax|cx|dx|bx|sp|bp|si|di)\+(\d+)\]$", RegexOptions.CultureInvariant)]
    private static partial Regex DefinitionReadRegex();

    [GeneratedRegex(@"^mov \[e(?:ax|cx|dx|bx|sp|bp|si|di)\+(\d+)\], (e(?:ax|cx|dx|bx|sp|bp|si|di))$", RegexOptions.CultureInvariant)]
    private static partial Regex WidgetWriteRegex();
}
