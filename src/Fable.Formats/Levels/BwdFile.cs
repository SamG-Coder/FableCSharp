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

        return new BwdFile { Regions = regions };
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
    byte[] Extra);
