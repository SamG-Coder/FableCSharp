namespace Fable.Formats.Tng;

public sealed class ThingFile
{
    public int Version { get; init; }
    public required IReadOnlyList<ThingSection> Sections { get; init; }

    public IEnumerable<ThingInstance> Things => Sections.SelectMany(section => section.Things);

    public static ThingFile Load(string path) => Parse(File.ReadAllLines(path));

    public static ThingFile Parse(string text) =>
        Parse(text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'));

    public static ThingFile Parse(IEnumerable<string> lines)
    {
        var sections = new List<ThingSection>();
        var currentSectionName = "NULL";
        var currentThings = new List<ThingInstance>();
        ThingBuilder? current = null;
        var version = 0;

        void FlushSection()
        {
            sections.Add(new ThingSection
            {
                Name = currentSectionName,
                Things = currentThings,
            });
            currentThings = [];
        }

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0)
                continue;
            if (line.EndsWith(';'))
                line = line[..^1].TrimEnd();

            if (StartsWithToken(line, "Version", out var versionText))
            {
                version = int.Parse(versionText);
                continue;
            }

            if (StartsWithToken(line, "XXXSectionStart", out var sectionName))
            {
                currentSectionName = sectionName;
                continue;
            }

            if (line.Equals("XXXSectionEnd", StringComparison.OrdinalIgnoreCase))
            {
                FlushSection();
                currentSectionName = "NULL";
                continue;
            }

            if (StartsWithToken(line, "NewThing", out var kind))
            {
                current = new ThingBuilder { Kind = kind, Section = currentSectionName };
                continue;
            }

            if (line.Equals("EndThing", StringComparison.OrdinalIgnoreCase))
            {
                if (current is not null)
                    currentThings.Add(current.Build());
                current = null;
                continue;
            }

            if (current is null)
                continue;

            if (line.StartsWith("Start", StringComparison.Ordinal) && !line.Contains(' '))
            {
                current.Block = line["Start".Length..];
                continue;
            }

            if (line.StartsWith("End", StringComparison.Ordinal) && current.Block is not null &&
                line.Equals("End" + current.Block, StringComparison.Ordinal))
            {
                current.Block = null;
                continue;
            }

            SplitProperty(line, out var key, out var value);
            current.Set(key, value);
        }

        if (currentThings.Count > 0)
            FlushSection();

        return new ThingFile
        {
            Version = version,
            Sections = sections,
        };
    }

    /// <summary>
    /// Counts the structural records in an ASCII TNG without materialising
    /// lines, property dictionaries, or <see cref="ThingInstance"/> objects.
    /// This is used by the native <c>004FDBC0</c> proximity-file open walk,
    /// where retaining every property from every map is not required merely
    /// to reproduce the file traversal.
    /// </summary>
    public static ThingFileSummary Scan(ReadOnlySpan<byte> data)
    {
        var version = 0;
        var sections = 0;
        var things = 0;
        var inThing = false;
        var offset = 0;
        while (offset < data.Length)
        {
            var end = data[offset..].IndexOf((byte)'\n');
            if (end < 0)
                end = data.Length - offset;
            var line = TrimAscii(data.Slice(offset, end));
            if (!line.IsEmpty && line[^1] == (byte)';')
                line = TrimAscii(line[..^1]);

            if (StartsWithAsciiToken(line, "Version"u8, out var rest))
                version = ParsePositiveInt(rest);
            else if (StartsWithAsciiToken(line, "XXXSectionStart"u8, out _))
                sections++;
            else if (StartsWithAsciiToken(line, "NewThing"u8, out _))
                inThing = true;
            else if (EqualsAscii(line, "EndThing"u8))
            {
                if (inThing)
                    things++;
                inThing = false;
            }

            offset += end + (offset + end < data.Length ? 1 : 0);
        }

        return new ThingFileSummary(version, sections, things, data.Length);
    }

    private static ReadOnlySpan<byte> TrimAscii(ReadOnlySpan<byte> value)
    {
        var start = 0;
        while (start < value.Length && IsAsciiSpace(value[start]))
            start++;
        var end = value.Length;
        while (end > start && IsAsciiSpace(value[end - 1]))
            end--;
        return value[start..end];
    }

    private static bool StartsWithAsciiToken(
        ReadOnlySpan<byte> line, ReadOnlySpan<byte> token, out ReadOnlySpan<byte> rest)
    {
        if (line.Length < token.Length)
        {
            rest = [];
            return false;
        }
        for (var i = 0; i < token.Length; i++)
        {
            var a = line[i];
            var b = token[i];
            if (a is >= (byte)'A' and <= (byte)'Z')
                a += (byte)('a' - 'A');
            if (b is >= (byte)'A' and <= (byte)'Z')
                b += (byte)('a' - 'A');
            if (a != b)
            {
                rest = [];
                return false;
            }
        }
        if (line.Length > token.Length && !IsAsciiSpace(line[token.Length]))
        {
            rest = [];
            return false;
        }
        rest = TrimAscii(line[token.Length..]);
        return true;
    }

    private static int ParsePositiveInt(ReadOnlySpan<byte> value)
    {
        var result = 0;
        foreach (var b in value)
        {
            if (b is < (byte)'0' or > (byte)'9')
                break;
            result = checked(result * 10 + b - (byte)'0');
        }
        return result;
    }

    private static bool EqualsAscii(ReadOnlySpan<byte> value, ReadOnlySpan<byte> expected)
    {
        if (value.Length != expected.Length)
            return false;
        for (var i = 0; i < value.Length; i++)
        {
            var a = value[i];
            var b = expected[i];
            if (a is >= (byte)'A' and <= (byte)'Z')
                a += (byte)('a' - 'A');
            if (b is >= (byte)'A' and <= (byte)'Z')
                b += (byte)('a' - 'A');
            if (a != b)
                return false;
        }
        return true;
    }

    private static bool IsAsciiSpace(byte value) =>
        value is (byte)' ' or (byte)'\t' or (byte)'\r';

    private static bool StartsWithToken(string line, string token, out string rest)
    {
        if (line.StartsWith(token, StringComparison.OrdinalIgnoreCase) &&
            (line.Length == token.Length || char.IsWhiteSpace(line[token.Length])))
        {
            rest = Unquote(line[token.Length..].Trim());
            return true;
        }

        rest = string.Empty;
        return false;
    }

    private static void SplitProperty(string line, out string key, out string value)
    {
        var space = line.IndexOf(' ');
        if (space < 0)
        {
            key = line;
            value = string.Empty;
            return;
        }

        key = line[..space];
        value = Unquote(line[(space + 1)..].Trim());
    }

    private static string Unquote(string value)
    {
        if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
            return value[1..^1];
        return value;
    }

    private sealed class ThingBuilder
    {
        public string Kind = "";
        public string Section = "";
        public string? Block;
        public readonly Dictionary<string, string> Properties = new(StringComparer.OrdinalIgnoreCase);

        public void Set(string key, string value)
        {
            var fullKey = Block is null ? key : $"{Block}.{key}";
            Properties[fullKey] = value;
        }

        public ThingInstance Build()
        {
            Properties.TryGetValue("DefinitionType", out var definition);
            Properties.TryGetValue("ScriptName", out var script);
            Properties.TryGetValue("UID", out var uidText);
            Properties.TryGetValue("Player", out var playerText);
            Properties.TryGetValue("CTCPhysicsStandard.PositionX", out var xText);
            Properties.TryGetValue("CTCPhysicsStandard.PositionY", out var yText);
            Properties.TryGetValue("CTCPhysicsStandard.PositionZ", out var zText);

            return new ThingInstance
            {
                Kind = Kind,
                Section = Section,
                DefinitionType = definition,
                ScriptName = script,
                Uid = ulong.TryParse(uidText, out var uid) ? uid : null,
                Player = int.TryParse(playerText, out var player) ? player : null,
                PositionX = float.TryParse(xText, out var x) ? x : null,
                PositionY = float.TryParse(yText, out var y) ? y : null,
                PositionZ = float.TryParse(zText, out var z) ? z : null,
                Properties = Properties,
            };
        }
    }
}

public readonly record struct ThingFileSummary(
    int Version, int SectionCount, int ThingCount, int SourceBytes);

public sealed class ThingSection
{
    public required string Name { get; init; }
    public required IReadOnlyList<ThingInstance> Things { get; init; }
}

public sealed class ThingInstance
{
    public required string Kind { get; init; }
    public required string Section { get; init; }
    public string? DefinitionType { get; init; }
    public string? ScriptName { get; init; }
    public ulong? Uid { get; init; }
    public int? Player { get; init; }
    public float? PositionX { get; set; }
    public float? PositionY { get; set; }
    public float? PositionZ { get; set; }
    public required IReadOnlyDictionary<string, string> Properties { get; init; }
}
