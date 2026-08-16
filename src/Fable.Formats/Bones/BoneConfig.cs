namespace Fable.Formats.Bones;

/// <summary>
/// Text bone-scale morph (.bncfg). Maps a CREATURE_* type to per-bone XYZ scales.
/// </summary>
public sealed class BoneConfig
{
    public required string CreatureType { get; init; }
    public required IReadOnlyList<BoneScale> Bones { get; init; }
    public required IReadOnlyDictionary<string, IReadOnlyList<string>> Groups { get; init; }

    public static BoneConfig Load(string path) => Parse(File.ReadAllLines(path));

    public static BoneConfig Parse(IEnumerable<string> lines)
    {
        var creature = "";
        var bones = new List<BoneScale>();
        var groups = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line[0] == '#')
                continue;
            if (line.EndsWith(';'))
                line = line[..^1].TrimEnd();

            var colon = line.IndexOf(':');
            if (colon <= 0)
                continue;
            var key = line[..colon].Trim();
            var value = line[(colon + 1)..].Trim();
            if (key.Equals("Creature_type", StringComparison.OrdinalIgnoreCase))
            {
                creature = value.Trim('"');
                continue;
            }

            if (value.Contains('"'))
            {
                var names = new List<string>();
                var start = 0;
                while (true)
                {
                    var q0 = value.IndexOf('"', start);
                    if (q0 < 0)
                        break;
                    var q1 = value.IndexOf('"', q0 + 1);
                    if (q1 < 0)
                        break;
                    names.Add(value[(q0 + 1)..q1]);
                    start = q1 + 1;
                }
                if (names.Count > 0)
                    groups[key] = names;
                continue;
            }

            var parts = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3 &&
                float.TryParse(parts[0], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var x) &&
                float.TryParse(parts[1], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var y) &&
                float.TryParse(parts[2], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var z))
                bones.Add(new BoneScale(key, x, y, z));
        }

        if (creature.Length == 0)
            throw new InvalidDataException("bncfg missing Creature_type.");

        return new BoneConfig
        {
            CreatureType = creature,
            Bones = bones,
            Groups = groups,
        };
    }
}

public readonly record struct BoneScale(string Name, float X, float Y, float Z);
