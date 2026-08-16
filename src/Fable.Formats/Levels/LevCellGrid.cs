namespace Fable.Formats.Levels;

/// <summary>
/// Per-cell table in the WAD .lev payload. Each cell is 21 bytes starting
/// with tag 21. Bytes 10-13 are material-table slots (0xFF = unused).
/// The u16 at +8 is a constant (~60), not height.
/// </summary>
public sealed class LevCellGrid
{
    public const int RecordSize = 21;
    public const int Tag = 21;

    public required int Width { get; init; }
    public required int Height { get; init; }
    public required LevCell[,] Cells { get; init; }
    public required int RecordCount { get; init; }

    public static LevCellGrid? TryParse(LevFile lev)
    {
        var width = lev.GridWidth;
        var height = lev.GridHeight;
        var need = width * height;
        var cells = new LevCell[width, height];
        var cursor = lev.PayloadOffset;
        var count = 0;
        while (cursor + RecordSize <= lev.Raw.Length &&
               BitConverter.ToInt32(lev.Raw, cursor) == Tag &&
               count < need)
        {
            var x = count % width;
            var y = count / width;
            cells[x, y] = new LevCell(
                lev.Raw[cursor + 10],
                lev.Raw[cursor + 11],
                lev.Raw[cursor + 12],
                lev.Raw[cursor + 13],
                BitConverter.ToUInt16(lev.Raw, cursor + 8));
            cursor += RecordSize;
            count++;
        }

        if (count < need)
            return null;

        return new LevCellGrid
        {
            Width = width,
            Height = height,
            Cells = cells,
            RecordCount = count,
        };
    }
}

public readonly record struct LevCell(
    byte Material0,
    byte Material1,
    byte Material2,
    byte Material3,
    ushort Constant60);
