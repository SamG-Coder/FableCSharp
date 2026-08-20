using Fable.Core;
using Fable.Formats.Defs;

if (args.Contains("--catalog"))
{
    TransformDump.Run();
    return;
}

var install = GameInstall.TryLocate() ?? throw new InvalidOperationException("no install");
var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
var bin = GameBin.Load(install.FindCompiledDef("frontend.bin")!, names);
var requested = args.Where(a => a != "--catalog").ToHashSet(StringComparer.OrdinalIgnoreCase);
var count = 0;

foreach (var entry in bin.Entries.Where(e => e.TypeName == "UI"))
{
    if (requested.Count != 0 && (entry.InstanceName is null || !requested.Contains(entry.InstanceName)))
        continue;

    var parsed = FrontendUiDef.TryParse(entry)
        ?? throw new InvalidDataException($"Could not parse {entry.InstanceName}");
    if (!FrontendUiSchema.TryConsume(entry, out var end, out var error) || end != entry.Raw.Length)
        throw new InvalidDataException($"{entry.InstanceName}: {error ?? $"ended at {end}/{entry.Raw.Length}"}");

    Console.WriteLine(
        $"{parsed.InstanceName}\ttype={parsed.Type}\tchildren={parsed.ChildIndices.Count}\t" +
        $"text={parsed.TextValue ?? "-"}\tgraphic={parsed.GraphicBankId}\t" +
        $"xy={parsed.PositionX},{parsed.PositionY}\twh={parsed.Width},{parsed.Height}\t" +
        $"positionOffset={parsed.PositionOffsetX},{parsed.PositionOffsetY}\t" +
        $"actionLeftClicked={parsed.ActionOnLeftClicked}\t" +
        $"actionLeftUnclicked={parsed.ActionOnLeftUnclicked}\t" +
        $"drawFromViewport={parsed.DrawFromViewport}\talignement={parsed.Alignement}\t" +
        $"schemaComplete={parsed.SchemaComplete}");
    count++;
}

Console.WriteLine($"Validated {count} UI entries through exact EOF.");
