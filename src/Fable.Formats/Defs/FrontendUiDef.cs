using System.Text;

namespace Fable.Formats.Defs;

/// <summary>
/// frontend.bin <c>UI</c> persist. Field CRCs are
/// Lionhead names: <c>Type</c> <c>0x0DA8270B</c>,
/// <c>Children</c> <c>0x3DC30C85</c>,
/// <c>Width</c> <c>0x8BF99D36</c>,
/// <c>PositionX</c> <c>0x1EDB8A31</c>,
/// <c>PositionY</c> <c>0x69DCBAA7</c>.
/// Text tag after <c>0xE215EF13</c> is UTF-16
/// (PRESS_START_TEXT → <c>TEXT_GUI_MENU_PRESS_BUTTON</c>).
/// </summary>
public sealed class FrontendUiDef
{
    public const uint TypeCrc = 0x0DA8270B;
    public const uint ChildrenCrc = 0x3DC30C85;
    public const uint WidthCrc = 0x8BF99D36;
    public const uint HeightCrc = 0x4341A19A;
    public const uint PositionXCrc = 0x1EDB8A31;
    public const uint PositionYCrc = 0x69DCBAA7;
    public const uint TextTagCrc = 0xE215EF13;

    public required string InstanceName { get; init; }
    public int Type { get; init; }
    public IReadOnlyList<int> ChildIndices { get; init; } = [];
    public float Width { get; init; }
    public float Height { get; init; }
    public float PositionX { get; init; }
    public float PositionY { get; init; }
    public string? TextTag { get; init; }

    public static FrontendUiDef? TryParse(GameBinEntry entry)
    {
        if (entry.TypeName != "UI" || entry.Raw.Length < 8)
            return null;
        var raw = entry.Raw;
        var type = 0;
        var children = new List<int>();
        var width = 0f;
        var height = 0f;
        var px = 0f;
        var py = 0f;
        string? text = null;
        for (var cursor = 0; cursor + 8 <= raw.Length; cursor++)
        {
            var crc = BitConverter.ToUInt32(raw, cursor);
            var payload = cursor + 4;
            if (crc == TypeCrc)
            {
                type = BitConverter.ToInt32(raw, payload);
                continue;
            }

            if (crc == ChildrenCrc)
            {
                var n = BitConverter.ToInt32(raw, payload);
                if (n is < 0 or > 64)
                    continue;
                children.Clear();
                var p = payload + 4;
                for (var i = 0; i < n && p + 4 <= raw.Length; i++, p += 4)
                    children.Add(BitConverter.ToInt32(raw, p));
                continue;
            }

            if (crc == WidthCrc)
            {
                var value = BitConverter.ToSingle(raw, payload);
                if (IsSize(value))
                    width = value;
                continue;
            }

            if (crc == HeightCrc)
            {
                var value = BitConverter.ToSingle(raw, payload);
                if (IsSize(value))
                    height = value;
                continue;
            }

            if (crc == PositionXCrc)
            {
                var value = BitConverter.ToSingle(raw, payload);
                if (IsPos(value))
                    px = value;
                continue;
            }

            if (crc == PositionYCrc)
            {
                var value = BitConverter.ToSingle(raw, payload);
                if (IsPos(value))
                    py = value;
                continue;
            }

            if (crc == TextTagCrc)
            {
                var t = payload;
                text = ReadUtf16(raw, ref t);
            }
        }

        return new FrontendUiDef
        {
            InstanceName = entry.InstanceName ?? entry.SourceName ?? "UI",
            Type = type,
            ChildIndices = children,
            Width = width,
            Height = height,
            PositionX = px,
            PositionY = py,
            TextTag = text,
        };
    }

    private static bool IsSize(float value) =>
        float.IsFinite(value) && value is > 0f and <= 4096f;

    private static bool IsPos(float value) =>
        float.IsFinite(value) && value is >= -2048f and <= 4096f;

    private static string? ReadUtf16(byte[] raw, ref int cursor)
    {
        var start = cursor;
        while (cursor + 1 < raw.Length)
        {
            var ch = BitConverter.ToUInt16(raw, cursor);
            cursor += 2;
            if (ch == 0)
                break;
        }

        var bytes = cursor - start;
        if (bytes < 4)
            return null;
        var text = Encoding.Unicode.GetString(raw, start, bytes);
        var nul = text.IndexOf('\0');
        return nul >= 0 ? text[..nul] : text;
    }
}
