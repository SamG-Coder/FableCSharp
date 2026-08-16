using Fable.Formats.IO;

namespace Fable.Formats.Levels;

/// <summary>
/// Per-16-unit landscape tile stored in the runtime STB copy of a .lev.
/// Each table record points at a raw-LZO payload of 15-byte world-space verts.
/// The blob at the u32@2048 offset is the same format (map-origin / west cell).
/// </summary>
public sealed class LevTileMesh
{
    public const int Section2OffsetAt = 2048;
    public const int TableOffset = 2056;
    public const int RecordSize = 36;
    public const int VertexHeaderSize = 32;
    public const int VertexStride = 15;
    public const uint RecordMagic = 0x012EC900;

    public required IReadOnlyList<LevTile> Tiles { get; init; }
    public LevTile? Section2 { get; init; }

    public static LevTileMesh Parse(byte[] stbLev, float mapX, float mapY, int cellsX, int cellsY)
    {
        var tiles = new List<LevTile>();
        var section2 = TryReadPayload(stbLev, (int)BitConverter.ToUInt32(stbLev, Section2OffsetAt), -1);
        if (section2 is not null)
            tiles.Add(section2.Value);

        for (var i = 0; i < cellsX * cellsY + 2; i++)
        {
            var rec = TableOffset + i * RecordSize;
            if (rec + RecordSize > stbLev.Length)
                break;

            var x0 = BitConverter.ToSingle(stbLev, rec);
            var y0 = BitConverter.ToSingle(stbLev, rec + 4);
            var z0 = BitConverter.ToSingle(stbLev, rec + 8);
            var ix = (int)MathF.Round((x0 - mapX) / LevHeightField.SampleSpacing);
            var iy = (int)MathF.Round((y0 - mapY) / LevHeightField.SampleSpacing);
            if (z0 is < 0f or > 200f || ix < 0 || iy < 0 || ix > cellsX || iy > cellsY)
                break;

            var off = (int)BitConverter.ToUInt32(stbLev, rec + 28);
            var size = (int)BitConverter.ToUInt32(stbLev, rec + 32);
            if (off <= 0 || size < 12 || off + size > stbLev.Length)
                continue;

            var tile = TryReadPayload(stbLev, off, i);
            if (tile is null)
                continue;

            tiles.Add(tile.Value with { CellX = ix, CellY = iy, OriginX = x0, OriginY = y0 });
        }

        return new LevTileMesh
        {
            Tiles = tiles,
            Section2 = section2,
        };
    }

    public int StampOnto(float[,] dest, float originX, float originY, int width, int height)
    {
        var stamped = 0;
        foreach (var tile in Tiles)
        {
            foreach (var vert in tile.Vertices)
            {
                if (vert.Z is < 0f or > 200f)
                    continue;
                var x = vert.WorldX - originX;
                var y = vert.WorldY - originY;
                var ix = (int)MathF.Round(x);
                var iy = (int)MathF.Round(y);
                if (ix < 0 || iy < 0 || ix > width || iy > height)
                    continue;
                if (MathF.Abs(x - ix) > 0.05f || MathF.Abs(y - iy) > 0.05f)
                    continue;
                dest[ix, iy] = vert.Z;
                stamped++;
            }
        }

        return stamped;
    }

    internal static LevTile? TryReadPayload(byte[] stbLev, int off, int index)
    {
        if (off < 0 || off + 12 > stbLev.Length)
            return null;

        var expect = BitConverter.ToInt32(stbLev, off);
        if (expect is < VertexHeaderSize + VertexStride or > 2_000_000)
            return null;

        var packed = BitConverter.ToInt32(stbLev, off + 4);
        if (packed <= 0 || off + 8 + packed > stbLev.Length)
            return null;

        var dest = new byte[expect];
        var produced = Lzo.DecompressRaw(stbLev.AsSpan(off + 8, packed), dest);
        if (produced != expect)
            return null;

        var count = BitConverter.ToUInt16(dest, 2);
        var need = VertexHeaderSize + count * VertexStride;
        if (count is 0 or > 4000 || need > dest.Length)
            return null;

        var verts = new LevTileVertex[count];
        for (var i = 0; i < count; i++)
        {
            var o = VertexHeaderSize + i * VertexStride;
            verts[i] = new LevTileVertex(
                BitConverter.ToUInt16(dest, o),
                BitConverter.ToUInt16(dest, o + 2),
                BitConverter.ToSingle(dest, o + 4));
        }

        return new LevTile(
            index,
            0, 0,
            verts[0].WorldX,
            verts[0].WorldY,
            verts,
            ReadIndices(dest, count, need));
    }

    internal static IReadOnlyList<int> ReadIndices(byte[] dest, int vertCount, int start)
    {
        if (start + 6 > dest.Length)
            return [];

        var indices = new List<int>();
        for (var o = start; o + 6 <= dest.Length; o += 6)
        {
            var a = BitConverter.ToUInt16(dest, o);
            var b = BitConverter.ToUInt16(dest, o + 2);
            var c = BitConverter.ToUInt16(dest, o + 4);
            if (a >= vertCount || b >= vertCount || c >= vertCount)
                break;
            indices.Add(a);
            indices.Add(b);
            indices.Add(c);
        }

        return indices.Count >= 3 ? indices : [];
    }
}

public readonly record struct LevTile(
    int Index,
    int CellX,
    int CellY,
    float OriginX,
    float OriginY,
    IReadOnlyList<LevTileVertex> Vertices,
    IReadOnlyList<int> Indices);

public readonly record struct LevTileVertex(ushort WorldX, ushort WorldY, float Z);
