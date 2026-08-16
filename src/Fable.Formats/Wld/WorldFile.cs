namespace Fable.Formats.Wld;

public sealed class WorldFile
{
    public required IReadOnlyList<string> InitialQuests { get; init; }
    public int MapUidCount { get; init; }
    public int ThingManagerUidCount { get; init; }
    public required IReadOnlyList<WorldMap> Maps { get; init; }
    public required IReadOnlyList<WorldRegion> Regions { get; init; }

    public WorldMap? FindMap(string scriptOrFileName)
    {
        var needle = scriptOrFileName.Replace('/', '\\');
        foreach (var map in Maps)
        {
            if (map.ScriptName.Equals(needle, StringComparison.OrdinalIgnoreCase))
                return map;
            if (map.LevelName.Equals(needle, StringComparison.OrdinalIgnoreCase))
                return map;
            if (Path.GetFileNameWithoutExtension(map.LevelName)
                .Equals(Path.GetFileNameWithoutExtension(needle), StringComparison.OrdinalIgnoreCase))
                return map;
        }

        return null;
    }

    /// <summary>
    /// WLD <c>NewRegion</c> whose <c>ContainsMap</c> list includes this map.
    /// New-game Oakvale is region <c>StartOakVale</c>, not <c>Maps[0]</c>.
    /// </summary>
    public WorldRegion? FindRegionContaining(string scriptOrFileName)
    {
        var stem = MapStem(scriptOrFileName);
        foreach (var region in Regions)
        {
            foreach (var map in region.ContainsMaps)
            {
                if (map.Equals(stem, StringComparison.OrdinalIgnoreCase))
                    return region;
            }
        }

        return null;
    }

    public static WorldFile Load(string path) => Parse(File.ReadAllLines(path));

    public static WorldFile Parse(IEnumerable<string> lines)
    {
        var quests = new List<string>();
        var maps = new List<WorldMap>();
        var regions = new List<WorldRegion>();
        var inQuests = false;
        WorldMapBuilder? current = null;
        WorldRegionBuilder? currentRegion = null;
        var mapUidCount = 0;
        var thingManagerUidCount = 0;

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal))
                continue;
            if (line.EndsWith(';'))
                line = line[..^1];

            if (line.Equals("START_INITIAL_QUESTS", StringComparison.OrdinalIgnoreCase))
            {
                inQuests = true;
                continue;
            }

            if (line.Equals("END_INITIAL_QUESTS", StringComparison.OrdinalIgnoreCase))
            {
                inQuests = false;
                continue;
            }

            if (inQuests)
            {
                quests.Add(line);
                continue;
            }

            if (StartsWithToken(line, "NewMap", out var newMapRest))
            {
                currentRegion = null;
                current = new WorldMapBuilder { Index = ParseInt(newMapRest) };
                continue;
            }

            if (line.Equals("EndMap", StringComparison.OrdinalIgnoreCase))
            {
                if (current is not null)
                    maps.Add(current.Build());
                current = null;
                continue;
            }

            if (StartsWithToken(line, "NewRegion", out var newRegionRest))
            {
                current = null;
                currentRegion = new WorldRegionBuilder { Index = ParseInt(newRegionRest) };
                continue;
            }

            if (line.Equals("EndRegion", StringComparison.OrdinalIgnoreCase))
            {
                if (currentRegion is not null)
                    regions.Add(currentRegion.Build());
                currentRegion = null;
                continue;
            }

            if (currentRegion is not null)
            {
                if (StartsWithToken(line, "RegionName", out var regionName))
                    currentRegion.RegionName = Unquote(regionName);
                else if (StartsWithToken(line, "NewDisplayName", out var display))
                    currentRegion.DisplayName = Unquote(display);
                else if (StartsWithToken(line, "RegionDef", out var def))
                    currentRegion.RegionDef = Unquote(def);
                else if (StartsWithToken(line, "ContainsMap", out var contains))
                    currentRegion.ContainsMaps.Add(MapStem(contains));
                else if (StartsWithToken(line, "SeesMap", out var sees))
                    currentRegion.SeesMaps.Add(MapStem(sees));
                continue;
            }

            if (current is not null)
            {
                if (StartsWithToken(line, "MapX", out var mapX)) current.MapX = ParseInt(mapX);
                else if (StartsWithToken(line, "MapY", out var mapY)) current.MapY = ParseInt(mapY);
                else if (StartsWithToken(line, "LevelName", out var level)) current.LevelName = Unquote(level);
                else if (StartsWithToken(line, "LevelScriptName", out var script)) current.ScriptName = Unquote(script);
                else if (StartsWithToken(line, "MapUID", out var uid)) current.MapUid = ParseInt(uid);
                else if (StartsWithToken(line, "IsSea", out var sea)) current.IsSea = ParseBool(sea);
                else if (StartsWithToken(line, "LoadedOnPlayerProximity", out var prox))
                    current.LoadedOnPlayerProximity = ParseBool(prox);
                continue;
            }

            if (StartsWithToken(line, "MapUIDCount", out var uidCount))
                mapUidCount = ParseInt(uidCount);
            else if (StartsWithToken(line, "ThingManagerUIDCount", out var thingCount))
                thingManagerUidCount = ParseInt(thingCount);
        }

        return new WorldFile
        {
            InitialQuests = quests,
            MapUidCount = mapUidCount,
            ThingManagerUidCount = thingManagerUidCount,
            Maps = maps,
            Regions = regions,
        };
    }

    private static bool StartsWithToken(string line, string token, out string rest)
    {
        if (line.StartsWith(token, StringComparison.OrdinalIgnoreCase) &&
            (line.Length == token.Length || char.IsWhiteSpace(line[token.Length])))
        {
            rest = line[token.Length..].Trim();
            return true;
        }

        rest = string.Empty;
        return false;
    }

    private static string Unquote(string value)
    {
        value = value.Trim();
        if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
            return value[1..^1];
        return value;
    }

    internal static string MapStem(string pathOrName)
    {
        var value = Unquote(pathOrName).Replace('/', '\\');
        return Path.GetFileNameWithoutExtension(value);
    }

    private static int ParseInt(string value) => int.Parse(value.Trim().Trim('"'));

    private static bool ParseBool(string value) =>
        value.Trim().Equals("TRUE", StringComparison.OrdinalIgnoreCase);

    private sealed class WorldMapBuilder
    {
        public int Index;
        public int MapX;
        public int MapY;
        public string LevelName = "";
        public string ScriptName = "";
        public int MapUid;
        public bool IsSea;
        public bool LoadedOnPlayerProximity;

        public WorldMap Build() => new()
        {
            Index = Index,
            MapX = MapX,
            MapY = MapY,
            LevelName = LevelName,
            ScriptName = ScriptName,
            MapUid = MapUid,
            IsSea = IsSea,
            LoadedOnPlayerProximity = LoadedOnPlayerProximity,
        };
    }

    private sealed class WorldRegionBuilder
    {
        public int Index;
        public string RegionName = "";
        public string DisplayName = "";
        public string RegionDef = "";
        public List<string> ContainsMaps { get; } = [];
        public List<string> SeesMaps { get; } = [];

        public WorldRegion Build() => new()
        {
            Index = Index,
            RegionName = RegionName,
            DisplayName = DisplayName,
            RegionDef = RegionDef,
            ContainsMaps = ContainsMaps,
            SeesMaps = SeesMaps,
        };
    }
}

/// <summary>
/// WLD <c>NewRegion</c> / <c>EndRegion</c> block. <c>ContainsMap</c> is the
/// playable cluster; <c>SeesMap</c> is the visible neighbourhood (fillers,
/// seas). Exe writer <c>004FD040</c> emits both as quoted <c>.lev</c> paths.
/// </summary>
public sealed class WorldRegion
{
    public required int Index { get; init; }
    public required string RegionName { get; init; }
    public required string DisplayName { get; init; }
    public required string RegionDef { get; init; }
    public required IReadOnlyList<string> ContainsMaps { get; init; }
    public required IReadOnlyList<string> SeesMaps { get; init; }
}

public sealed class WorldMap
{
    public required int Index { get; init; }
    public required int MapX { get; init; }
    public required int MapY { get; init; }
    public required string LevelName { get; init; }
    public required string ScriptName { get; init; }
    public required int MapUid { get; init; }
    public required bool IsSea { get; init; }
    public required bool LoadedOnPlayerProximity { get; init; }

    public string FileStem => Path.GetFileNameWithoutExtension(LevelName);
}
