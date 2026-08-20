#r "C:\FableCSharp\src\Fable.Formats\bin\Debug\net10.0\Fable.Formats.dll"
#r "C:\FableCSharp\src\Fable.Core\bin\Debug\net10.0\Fable.Core.dll"
using Fable.Core;
using Fable.Formats.Defs;
using Fable.Game;

var install = GameInstall.TryLocate() ?? throw new Exception("no install");
var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
var bin = GameBin.Load(install.FindCompiledDef("frontend.bin")!, names);

var parentOf = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
foreach (var e in bin.Entries)
{
    if (e.TypeName != "UI")
        continue;
    var p = FrontendUiDef.TryParse(e);
    if (p is null)
        continue;
    foreach (var i in p.ChildIndices)
    {
        if ((uint)i >= (uint)bin.Entries.Count)
            continue;
        var child = bin.Entries[i].InstanceName;
        if (!string.IsNullOrEmpty(child))
            parentOf[child] = p.InstanceName;
    }
}

Console.WriteLine("=== frontend.bin Type==39 ===");
var n39 = 0;
foreach (var e in bin.Entries)
{
    if (e.TypeName != "UI")
        continue;
    var p = FrontendUiDef.TryParse(e);
    if (p is null || p.Type != 39)
        continue;
    n39++;
    parentOf.TryGetValue(p.InstanceName, out var parent);
    var kids = string.Join(",", p.ChildIndices.Select(i =>
        (uint)i < (uint)bin.Entries.Count
            ? bin.Entries[i].InstanceName ?? $"#{i}"
            : $"#{i}"));
    Console.WriteLine(
        $"{p.InstanceName} type={p.Type} parent={parent ?? "-"} " +
        $"children=[{kids}] msg={p.MessageId} plus224={p.Plus224} text={p.TextTag}");
}
Console.WriteLine($"count={n39}");

void Walk(string root)
{
    var hits = new List<string>();
    var stack = new Stack<(string Name, string? Parent)>();
    stack.Push((root, null));
    var n = 0;
    var types = new SortedDictionary<int, int>();
    while (stack.Count > 0)
    {
        var (name, parent) = stack.Pop();
        var e = bin.FindEntry(name);
        var p = e is null ? null : FrontendUiDef.TryParse(e);
        n++;
        var t = p?.Type ?? -1;
        types[t] = types.TryGetValue(t, out var c) ? c + 1 : 1;
        if (t == 39)
            hits.Add($"{name} parent={parent ?? "-"}");
        if (p is null)
            continue;
        for (var i = p.ChildIndices.Count - 1; i >= 0; i--)
        {
            var idx = p.ChildIndices[i];
            if ((uint)idx >= (uint)bin.Entries.Count)
                continue;
            stack.Push((bin.Entries[idx].InstanceName ?? $"#{idx}", name));
        }
    }

    Console.WriteLine($"--- {root} widgets={n} type39={hits.Count} types={string.Join(",", types.Select(kv => kv.Key + ":" + kv.Value))} ---");
    foreach (var h in hits)
        Console.WriteLine("  " + h);
}

Console.WriteLine();
Console.WriteLine("=== first-seen / options / redefine trees ===");
foreach (var root in new[]
{
    "UI_FRONTEND_PRESS_START_MENU",
    "UI_FRONTEND_NEW_PROFILE_SCREEN",
    "UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE",
    "UI_FRONTEND_OPTIONS_MENU",
    "UI_FRONTEND_LIST_OPTIONS_MENU",
    "UI_FRONTEND_OPTIONS_SUB_MENU",
    "UI_FRONTEND_AUDIO_OPTIONS_MENU",
    "UI_FRONTEND_SCREEN_REDEFINE_KEYS_PC",
    "UI_FRONTEND_LIST_REDEFINE_KEYS_MENU",
    "UI_KEY_REDEFINER_BASE",
    "UI_OPTIONS_BUTTON_REDEFINE_KEYS",
    "UI_HELPERS_REDEFINE",
    "PC_UI_REDEFINER_LIST",
    "UI_OPTIONS_REDEFINE_TABLES",
})
    Walk(root);
