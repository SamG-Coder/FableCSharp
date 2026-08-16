using System.Text;

namespace Fable.Formats.Levels;

/// <summary>
/// Compiled Lionhead landscape (.lev). Documented by tests in LevFormatTests.
/// </summary>
public sealed class LevFile
{
    public const int Version = 25;
    public const uint FormatConstant = 0x1904;
    public const int HeaderSize = 179;
    public const int MaterialSlotSize = 132;
    public const int MaterialSlotCount = 255;
    public const int MaterialTableEnd = HeaderSize + MaterialSlotCount * MaterialSlotSize; // 33839
    public const int SecondaryTableEnd = 67639;

    public required int GridWidth { get; init; }
    public required int GridHeight { get; init; }
    public required float CellSize { get; init; }
    public required IReadOnlyList<LevMaterial> Materials { get; init; }
    public required IReadOnlyList<string> SoundThemes { get; init; }
    public required int PayloadOffset { get; init; }
    public required byte[] Raw { get; init; }

    public int CellCount => GridWidth * GridHeight;

    public static LevFile Parse(byte[] data)
    {
        if (data.Length < MaterialTableEnd + 8)
            throw new InvalidDataException("LEV too small.");

        var version = BitConverter.ToInt32(data, 0);
        if (version != Version)
            throw new InvalidDataException($"Unexpected LEV version {version}.");

        var constant = BitConverter.ToUInt32(data, 4);
        if (constant != FormatConstant)
            throw new InvalidDataException($"Unexpected LEV constant 0x{constant:X}.");

        var width = (int)(BitConverter.ToUInt32(data, 36) >> 16);
        var height = (int)(BitConverter.ToUInt32(data, 40) >> 16);
        var cell = BitConverter.ToUInt32(data, 44) / 65536f;
        if (width is <= 0 or > 1024 || height is <= 0 or > 1024)
            throw new InvalidDataException($"Implausible grid {width}x{height}.");

        var materials = new List<LevMaterial>();
        for (var i = 0; i < MaterialSlotCount; i++)
        {
            var offset = HeaderSize + i * MaterialSlotSize;
            var name = ReadFixedCString(data, offset, 128);
            var id = BitConverter.ToUInt32(data, offset + 128);
            if (name.Length == 0)
                continue;
            materials.Add(new LevMaterial(i, name, id));
        }

        var themes = new List<string>();
        var cursor = SecondaryTableEnd + 4; // skip level tag
        while (cursor + 4 <= data.Length)
        {
            var len = BitConverter.ToInt32(data, cursor);
            if (len is < 3 or > 80 || cursor + 4 + len > data.Length)
                break;
            var name = Encoding.ASCII.GetString(data, cursor + 4, len);
            if (!name.All(ch => char.IsLetterOrDigit(ch) || ch == '_'))
                break;
            themes.Add(name);
            cursor += 4 + len;
        }

        return new LevFile
        {
            GridWidth = width,
            GridHeight = height,
            CellSize = cell,
            Materials = materials,
            SoundThemes = themes,
            PayloadOffset = cursor,
            Raw = data,
        };
    }

    private static string ReadFixedCString(byte[] data, int offset, int max)
    {
        var end = offset;
        var limit = Math.Min(data.Length, offset + max);
        while (end < limit && data[end] != 0)
        {
            if (data[end] < 32 || data[end] >= 127)
                return string.Empty;
            end++;
        }
        return Encoding.ASCII.GetString(data, offset, end - offset);
    }
}

public readonly record struct LevMaterial(int Slot, string Name, uint Id);
