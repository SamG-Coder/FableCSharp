namespace Fable.Formats.Wld;

/// <summary>
/// data\Misc\FinalAlbion_StartingRegionGraph.txt — adjacency of region script names.
/// </summary>
public sealed class RegionGraph
{
    public required IReadOnlyDictionary<string, IReadOnlyList<string>> Neighbors { get; init; }

    public static RegionGraph Load(string path) => Parse(File.ReadAllLines(path));

    public static RegionGraph Parse(IEnumerable<string> lines)
    {
        var map = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal))
                continue;
            if (line.EndsWith(';'))
                line = line[..^1].TrimEnd();

            var colon = line.IndexOf(':');
            if (colon < 0)
                continue;
            var from = Unquote(line[..colon]);
            if (from.Length == 0)
                continue;

            var neighbors = new List<string>();
            var rest = line[(colon + 1)..];
            var start = 0;
            while (true)
            {
                var q0 = rest.IndexOf('"', start);
                if (q0 < 0)
                    break;
                var q1 = rest.IndexOf('"', q0 + 1);
                if (q1 < 0)
                    break;
                neighbors.Add(rest[(q0 + 1)..q1]);
                start = q1 + 1;
            }

            map[from] = neighbors;
        }

        return new RegionGraph { Neighbors = map };
    }

    public IReadOnlyList<string> NeighborsOf(string region) =>
        Neighbors.TryGetValue(region, out var list) ? list : [];

    public static int MapUidFromEntranceLink(ulong entranceConnectedToUid) =>
        RegionLink.Unpack(entranceConnectedToUid).MapUid;

    private static string Unquote(string value)
    {
        value = value.Trim();
        if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
            return value[1..^1];
        return value;
    }
}
