using System.Text;

namespace Fable.Formats.Levels;

/// <summary>
/// FinalAlbion.bwd — compiled region table. Each record is a .lev path, script
/// name, then min/max WLD XY matching the WLD map rectangle.
/// </summary>
public sealed class BwdFile
{
    public const int TrailerSize = 28;

    public required IReadOnlyList<BwdRegion> Regions { get; init; }
    public required int DeclaredCount { get; init; }
    public required int DisplayOffset { get; init; }
    public required IReadOnlyList<BwdDisplay> Displays { get; init; }

    public static BwdFile Load(string path) => Parse(File.ReadAllBytes(path));

    public static BwdFile Parse(byte[] data)
    {
        if (data.Length < 8)
            throw new InvalidDataException("BWD too small.");

        var declared = BitConverter.ToInt32(data, 0);
        var cursor = 4;
        var regions = new List<BwdRegion>(Math.Max(declared, 8));
        while (cursor + 8 <= data.Length)
        {
            if (!TryReadString(data, ref cursor, out var levPath) ||
                !TryReadString(data, ref cursor, out var name) ||
                cursor + TrailerSize > data.Length)
                break;

            var flags = data.AsSpan(cursor, 3).ToArray();
            cursor += 3;
            var minX = BitConverter.ToInt32(data, cursor); cursor += 4;
            var maxX = BitConverter.ToInt32(data, cursor); cursor += 4;
            var minY = BitConverter.ToInt32(data, cursor); cursor += 4;
            var maxY = BitConverter.ToInt32(data, cursor); cursor += 4;
            var extra = data.AsSpan(cursor, 9).ToArray();
            var mapUid = extra.Length >= 5 ? BitConverter.ToInt32(extra, 1) : 0;
            cursor += 9;

            if (name.Length == 0 || minX is < 0 or > 20_000 || maxX <= minX)
                break;

            regions.Add(new BwdRegion(name, levPath, minX, maxX, minY, maxY, mapUid, flags, extra));
        }

        var displayOffset = FindDisplayStart(data, cursor);
        return new BwdFile
        {
            Regions = regions,
            DeclaredCount = declared,
            DisplayOffset = displayOffset,
            Displays = ParseDisplays(data, displayOffset),
        };
    }

    public BwdRegion? Find(string name)
    {
        foreach (var region in Regions)
        {
            if (region.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return region;
        }

        return null;
    }

    /// <summary>
    /// Maps whose WLD rectangle shares an edge or corner with <paramref name="name"/>.
    /// Fable.exe <c>OpenStaticMaps</c> / <c>CLandscapeBackgroundPatch</c> use this
    /// neighbourhood, not the starting-region teleport graph.
    /// </summary>
    public IReadOnlyList<BwdRegion> AdjacentTo(string name)
    {
        var home = Find(name);
        if (home is null)
            return [];

        var list = new List<BwdRegion>();
        foreach (var region in Regions)
        {
            if (region.Name.Equals(home.Value.Name, StringComparison.OrdinalIgnoreCase))
                continue;
            if (home.Value.Touches(region))
                list.Add(region);
        }

        return list;
    }

    public BwdDisplay? FindDisplay(string scriptName)
    {
        foreach (var display in Displays)
        {
            if (display.ScriptName.Equals(scriptName, StringComparison.OrdinalIgnoreCase))
                return display;
        }

        return null;
    }

    private static int FindDisplayStart(byte[] data, int from)
    {
        for (var i = from; i + 16 <= data.Length; i++)
        {
            var cursor = i;
            if (TryReadIdent(data, ref cursor, out _) &&
                TryReadIdent(data, ref cursor, out var textKey) &&
                textKey.StartsWith("TXT_REGION_", StringComparison.Ordinal) &&
                TryReadIdent(data, ref cursor, out _) &&
                TryReadIdent(data, ref cursor, out _))
                return i;
        }

        return from;
    }

    private static List<BwdDisplay> ParseDisplays(byte[] data, int start)
    {
        var displays = new List<BwdDisplay>();
        var cursor = start;
        while (cursor + 16 <= data.Length)
        {
            var save = cursor;
            if (!TryReadIdent(data, ref cursor, out var script) ||
                !TryReadIdent(data, ref cursor, out var textKey) ||
                !textKey.StartsWith("TXT_", StringComparison.Ordinal) ||
                !TryReadIdent(data, ref cursor, out var regionId) ||
                !TryReadIdent(data, ref cursor, out var minimap))
            {
                var next = FindDisplayStart(data, save + 1);
                if (next <= save)
                    break;
                cursor = next;
                continue;
            }

            var extraStart = cursor;
            var extraEnd = FindDisplayStart(data, extraStart);
            if (extraEnd <= extraStart)
                extraEnd = data.Length;
            var extra = data.AsSpan(extraStart, extraEnd - extraStart).ToArray();
            ParseExtra(extra, out var scale, out var mapX, out var mapY, out var links);
            displays.Add(new BwdDisplay(script, textKey, regionId, minimap, scale, mapX, mapY, links, extra));
            cursor = extraEnd;
        }

        return displays;
    }

    private static void ParseExtra(
        byte[] extra, out float scale, out int mapX, out int mapY, out IReadOnlyList<string> links)
    {
        scale = 1f;
        mapX = 0;
        mapY = 0;
        var names = new List<string>();
        if (extra.Length >= 23)
        {
            scale = BitConverter.ToSingle(extra, 3);
            mapX = BitConverter.ToInt32(extra, 15);
            mapY = BitConverter.ToInt32(extra, 19);
        }

        var cursor = 23;
        while (cursor + 4 <= extra.Length)
        {
            if (TryReadIdent(extra, ref cursor, out var name) &&
                !name.StartsWith("TXT_", StringComparison.Ordinal))
            {
                names.Add(name);
                continue;
            }

            cursor += 4;
        }

        links = names;
    }

    private static bool TryReadIdent(byte[] data, ref int cursor, out string text)
    {
        text = "";
        if (cursor + 4 > data.Length)
            return false;
        var n = BitConverter.ToInt32(data, cursor);
        if (n is < 3 or > 80 || cursor + 4 + n > data.Length)
            return false;
        var slice = data.AsSpan(cursor + 4, n);
        foreach (var b in slice)
        {
            if (b is < (byte)'-' or > (byte)'z')
                return false;
            if (b is not ((byte)'_' or (byte)'-') && !char.IsLetterOrDigit((char)b))
                return false;
        }

        text = Encoding.ASCII.GetString(slice);
        cursor += 4 + n;
        return text.Length > 0;
    }

    private static bool TryReadString(byte[] data, ref int cursor, out string text)
    {
        text = "";
        if (cursor + 4 > data.Length)
            return false;
        var n = BitConverter.ToInt32(data, cursor);
        if (n is < 1 or > 260 || cursor + 4 + n > data.Length)
            return false;
        var slice = data.AsSpan(cursor + 4, n);
        foreach (var b in slice)
        {
            if (b is not 0 and (< 32 or > 126))
                return false;
        }
        text = Encoding.ASCII.GetString(slice).TrimEnd('\0');
        cursor += 4 + n;
        return text.Length > 0;
    }
}

public readonly record struct BwdRegion(
    string Name,
    string LevPath,
    int MinX,
    int MaxX,
    int MinY,
    int MaxY,
    int MapUid,
    byte[] Flags,
    byte[] Extra)
{
    public bool Touches(BwdRegion other) =>
        MinX <= other.MaxX && MaxX >= other.MinX &&
        MinY <= other.MaxY && MaxY >= other.MinY;
}

public readonly record struct BwdDisplay(
    string ScriptName,
    string TextKey,
    string RegionId,
    string MinimapName,
    float Scale,
    int MapX,
    int MapY,
    IReadOnlyList<string> LinkedNames,
    byte[] Extra);
