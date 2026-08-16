using Fable.Formats.IO;

namespace Fable.Formats.Levels;

/// <summary>
/// Per-16-unit landscape tile stored in the runtime STB copy of a .lev.
/// Each table record points at a raw-LZO payload of 15-byte world-space verts.
/// </summary>
public sealed class LevTileMesh
{
    public const int TableOffset = 2056;
    public const int RecordSize = 36;
    public const int VertexHeaderSize = 32;
    public const int VertexStride = 15;
    public const uint RecordMagic = 0x012EC900;

    public required IReadOnlyList<LevTile> Tiles { get; init; }

    public static LevTileMesh Parse(byte[] stbLev, float mapX, float mapY, int cellsX, int cellsY)
    {
        var tiles = new List<LevTile>();
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

            var verts = DecompressVertices(stbLev.AsSpan(off, size));
            if (verts.Count == 0)
                continue;

            tiles.Add(new LevTile(i, ix, iy, x0, y0, verts));
        }

        return new LevTileMesh { Tiles = tiles };
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

    internal static IReadOnlyList<LevTileVertex> DecompressVertices(ReadOnlySpan<byte> payload)
    {
        if (payload.Length < 12)
            return [];

        var expect = BitConverter.ToInt32(payload);
        if (expect is < VertexHeaderSize + VertexStride or > 2_000_000)
            return [];

        var dest = new byte[expect];
        var produced = Lzo.DecompressRaw(payload[8..], dest);
        if (produced != expect)
            return [];

        var count = BitConverter.ToUInt16(dest, 2);
        var need = VertexHeaderSize + count * VertexStride;
        if (count is 0 or > 4000 || need > dest.Length)
            return [];

        var verts = new LevTileVertex[count];
        for (var i = 0; i < count; i++)
        {
            var o = VertexHeaderSize + i * VertexStride;
            verts[i] = new LevTileVertex(
                BitConverter.ToUInt16(dest, o),
                BitConverter.ToUInt16(dest, o + 2),
                BitConverter.ToSingle(dest, o + 4));
        }

        return verts;
    }
}

public readonly record struct LevTile(
    int Index,
    int CellX,
    int CellY,
    float OriginX,
    float OriginY,
    IReadOnlyList<LevTileVertex> Vertices);

public readonly record struct LevTileVertex(ushort WorldX, ushort WorldY, float Z);
