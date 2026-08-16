using System.Text;

namespace Fable.ExeIndex;

/// <summary>
/// Walks every function reachable from New Game / StartOakVale seeds.
/// Does not seed Lookout or later campaign maps.
/// </summary>
internal static class FunctionMap
{
    public const int MaxDepth = 12;
    public const int MaxFunctions = 8000;
    public const int MaxInsns = 400;

    /// <summary>
    /// Code windows used by New Game / StartOakVale first scene only.
    /// Menu-wide frontend and later-town ranges are omitted.
    /// </summary>
    public static readonly (uint Lo, uint Hi, string Name)[] NewGameRanges =
    [
        (0x00595B00, 0x00595D00, "UI-new-game"),
        (0x004B5000, 0x004B5200, "start-new-quest"),
        (0x00489D00, 0x0048A200, "CreateCharacter"),
        (0x004FD000, 0x004FE000, "WLD-region"),
        (0x006A9D00, 0x006AD200, "PlayerCreature"),
        (0x0089FA00, 0x0089FC00, "MARKER-LIGHT"),
        (0x00988000, 0x0098C000, "VS-wrapper"),
        (0x00B32000, 0x00B34000, "MainScene-prims"),
        (0x00B41000, 0x00B4A000, "maps-lighting"),
        (0x00B66000, 0x00B7F000, "landscape-water"),
        (0x00B8B000, 0x00BD4000, "static-palskin"),
        (0x00BDB000, 0x00BDC800, "LayoutLights"),
        (0x00BF4000, 0x00BF6000, "per-cell"),
        (0x00DBDE00, 0x00DBF000, "StartOakVale"),
    ];

    public static readonly (string Name, uint Va)[] NewGameSeeds =
    [
        ("UI TEXT NEW GAME", 0x00595B24),
        ("UI FRONTEND MAIN MENU", 0x0059899A),
        ("START NEW QUEST", 0x004B5080),
        ("StartOakVale", 0x00DBDE40),
        ("HerosOldHouse tail", 0x00DBE0C6),
        ("hero-exists", 0x00CB7940),
        ("CThingPlayerCreature Create", 0x006AC910),
        ("CPlayer CreateCharacter", 0x00489D40),
        ("ConstructFromParams", 0x006A9DD0),
        ("LayoutLights ctor", 0x00BDB400),
        ("lighting ctor", 0x00B482A0),
        ("TOD blend", 0x00B46C80),
        ("c35 flush", 0x0098A760),
        ("c35 setter", 0x0098B2C0),
        ("PALSKIN register upload", 0x009896D0),
        ("SetVSConstantF wrapper", 0x00989A60),
        ("OpenStaticMaps", 0x00B42750),
        ("LoadWaterData", 0x00B41FA0),
        ("landscape draw", 0x00B6B0B0),
        ("per-cell submit", 0x00BF4570),
        ("water draw", 0x00B783F0),
        ("static VS bind", 0x00B8B660),
        ("PALSKIN VS bind", 0x00BD01B8),
        ("MARKER LIGHT", 0x0089FAA8),
        ("CAM intro writer", 0x004FD040),
    ];

    public sealed class Node
    {
        public required uint Va { get; init; }
        public required int Depth { get; init; }
        public required string Seed { get; init; }
        public required int Insns { get; init; }
        public required IReadOnlyList<uint> Calls { get; init; }
        public required IReadOnlyList<string> Strings { get; init; }
    }

    public static List<Node> WalkNewGame(PeImage pe)
    {
        var queue = new Queue<(uint Va, int Depth, string Seed)>();
        foreach (var (name, va) in NewGameSeeds)
        {
            var file = pe.FileOffset(va);
            if (file < 0)
                continue;
            var start = pe.Va(X86.FindPrologue(pe, file));
            queue.Enqueue((start, 0, name));
        }

        foreach (var start in ScanRangeStarts(pe))
            queue.Enqueue((start, 0, "range"));

        var seen = new HashSet<uint>();
        var nodes = new List<Node>();
        while (queue.Count > 0 && nodes.Count < MaxFunctions)
        {
            var (va, depth, seed) = queue.Dequeue();
            if (!seen.Add(va))
                continue;
            var file = pe.FileOffset(va);
            if (file < 0 || !pe.InCode(file))
                continue;

            var steps = X86.Walk(pe, file, MaxInsns, stopOnRet: false);
            var calls = new List<uint>();
            var strings = new List<string>();
            foreach (var step in steps)
            {
                if (step.DirectCall is { } dest && pe.FileOffset(dest) >= 0)
                    calls.Add(dest);
                CollectQuoted(step.Text, strings);
            }

            nodes.Add(new Node
            {
                Va = va,
                Depth = depth,
                Seed = seed,
                Insns = steps.Count,
                Calls = calls,
                Strings = strings,
            });

            if (depth >= MaxDepth)
                continue;
            foreach (var dest in calls)
            {
                var destFile = pe.FileOffset(dest);
                if (destFile < 0 || !pe.InCode(destFile))
                    continue;
                var start = pe.Va(X86.FindPrologue(pe, destFile));
                if (!seen.Contains(start))
                    queue.Enqueue((start, depth + 1, seed));
            }
        }

        nodes.Sort((a, b) => a.Va.CompareTo(b.Va));
        return nodes;
    }

    public static string ToMarkdown(IReadOnlyList<Node> nodes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# New Game function map");
        sb.AppendLine();
        sb.AppendLine("Every function in New Game / `StartOakVale` code ranges, plus callees.");
        sb.AppendLine("Lookout and later campaign maps are not scanned.");
        sb.AppendLine();
        sb.AppendLine($"functions **{nodes.Count}** · depth ≤ {MaxDepth} · ranges {NewGameRanges.Length} · [INDEX](INDEX.md)");
        sb.AppendLine();
        sb.AppendLine("## Seeds");
        sb.AppendLine();
        foreach (var (name, va) in NewGameSeeds)
            sb.AppendLine($"- `{name}` `0x{va:X8}`");
        sb.AppendLine();
        sb.AppendLine("## Hits");
        sb.AppendLine();
        WriteHits(sb, nodes, "StartOakVale");
        WriteHits(sb, nodes, "CREATURE_HERO_CHILD");
        WriteHits(sb, nodes, "HerosOldHouse");
        WriteHits(sb, nodes, "PALSKIN");
        WriteHits(sb, nodes, "CAM_OVIF");
        WriteHits(sb, nodes, "WatchBarrels");
        WriteHits(sb, nodes, "CThingPlayerCreature");
        WriteHits(sb, nodes, "CTCAnimation");
        WriteHits(sb, nodes, "009896D0");
        WriteHits(sb, nodes, "2LIGHTS");
        WriteHits(sb, nodes, "2POINTLIGHTS");
        WriteHits(sb, nodes, "VSHADER_STATIC_DIRLIGHT_FOG");
        WriteHits(sb, nodes, "VSHADER_LANDSCAPE_FOREGROUND");
        WriteHits(sb, nodes, "MARKER_LIGHT");
        WriteHits(sb, nodes, "ENGINE_WATER");
        sb.AppendLine();
        sb.AppendLine("## Functions");
        sb.AppendLine();
        sb.AppendLine("| va | depth | seed | insns | strings | calls |");
        sb.AppendLine("|---|---|---|---|---|---|");
        var shown = 0;
        foreach (var n in nodes)
        {
            if (shown++ >= 600)
            {
                sb.AppendLine($"| … | | | | | {nodes.Count - 600} more |");
                break;
            }

            var str = n.Strings.Count == 0 ? "" : "`" + Trunc(string.Join("; ", n.Strings.Take(3)), 48) + "`";
            var call = n.Calls.Count == 0 ? "" : string.Join(" ", n.Calls.Take(4).Select(c => $"`{c:X8}`"));
            if (n.Calls.Count > 4)
                call += $" +{n.Calls.Count - 4}";
            sb.AppendLine($"| `0x{n.Va:X8}` | {n.Depth} | {n.Seed} | {n.Insns} | {str} | {call} |");
        }

        return sb.ToString();
    }

    public static string ToTsv(IReadOnlyList<Node> nodes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("va\tdepth\tseed\tinsns\tstrings\tcalls");
        foreach (var n in nodes)
        {
            var str = string.Join("|", n.Strings).Replace('\t', ' ');
            var call = string.Join(",", n.Calls.Select(c => c.ToString("X8")));
            sb.AppendLine($"0x{n.Va:X8}\t{n.Depth}\t{n.Seed}\t{n.Insns}\t{str}\t{call}");
        }

        return sb.ToString();
    }

    public static IReadOnlyList<uint> ScanRangeStarts(PeImage pe)
    {
        var starts = new HashSet<uint>();
        var data = pe.Data;
        foreach (var (lo, hi, _) in NewGameRanges)
        {
            var a = pe.FileOffset(lo);
            var b = pe.FileOffset(hi - 1);
            if (a < 0 || b < 0)
                continue;
            for (var i = a + 2; i < b; i++)
            {
                if (data[i - 1] != 0xCC || data[i] == 0xCC || !pe.InCode(i))
                    continue;
                starts.Add(pe.Va(i));
            }
        }

        return starts.OrderBy(v => v).ToList();
    }

    private static void WriteHits(StringBuilder sb, IReadOnlyList<Node> nodes, string key)
    {
        var hits = nodes.Where(n =>
            n.Strings.Any(s => s.Contains(key, StringComparison.OrdinalIgnoreCase)) ||
            n.Seed.Contains(key, StringComparison.OrdinalIgnoreCase) ||
            n.Calls.Any(c => c.ToString("X8").Contains(key, StringComparison.OrdinalIgnoreCase))).ToList();
        sb.AppendLine($"- **{key}**: {hits.Count} fns" + (hits.Count == 0
            ? ""
            : " — " + string.Join(", ", hits.Take(12).Select(h => $"`0x{h.Va:X8}`"))));
    }

    private static void CollectQuoted(string text, List<string> strings)
    {
        var i = text.IndexOf('"');
        if (i < 0)
            return;
        var j = text.IndexOf('"', i + 1);
        if (j <= i + 1)
            return;
        var s = text[(i + 1)..j];
        if (s.Length >= 4 && !strings.Contains(s, StringComparer.Ordinal))
            strings.Add(s);
    }

    private static string Trunc(string s, int n) => s.Length <= n ? s : s[..n] + "…";
}
