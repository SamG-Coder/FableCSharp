using System.Numerics;
using Fable.Formats.Defs;
using Fable.Formats.IO;
using Fable.Formats.Meshes;

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

    public IReadOnlyList<MeshTriangle> ToTriangles(
        float originX,
        float originY,
        LevCellGrid cells,
        IReadOnlyList<LevMaterial> materials,
        HeaderEnums? textures = null)
    {
        var bySlot = materials.ToDictionary(item => item.Slot);
        var triangles = new List<MeshTriangle>(Tiles.Count * 512);
        foreach (var tile in Tiles)
        {
            var points = new Vector3[tile.Vertices.Count];
            for (var i = 0; i < tile.Vertices.Count; i++)
            {
                var v = tile.Vertices[i];
                points[i] = new Vector3(v.WorldX - originX, v.WorldY - originY, v.Z);
            }

            var at = new Dictionary<(int X, int Y), Vector3>();
            foreach (var p in points)
                at[((int)MathF.Round(p.X), (int)MathF.Round(p.Y))] = p;

            var minX = at.Count == 0 ? 0 : at.Keys.Min(k => k.X);
            var maxX = at.Count == 0 ? 0 : at.Keys.Max(k => k.X);
            var minY = at.Count == 0 ? 0 : at.Keys.Min(k => k.Y);
            var maxY = at.Count == 0 ? 0 : at.Keys.Max(k => k.Y);
            var span = Math.Max(1, (maxX - minX + 1) * (maxY - minY + 1));
            var filled = at.Count / (float)span;
            var useGrid = at.Count >= 4 && (maxX - minX) >= 8 && (maxY - minY) >= 8 && filled >= 0.7f;

            if (useGrid)
            {
                for (var y = minY; y < maxY; y++)
                for (var x = minX; x < maxX; x++)
                {
                    if (!at.TryGetValue((x, y), out var a) ||
                        !at.TryGetValue((x + 1, y), out var b) ||
                        !at.TryGetValue((x, y + 1), out var c) ||
                        !at.TryGetValue((x + 1, y + 1), out var d))
                        continue;
                    var tex = TextureAt(a, cells, bySlot, textures);
                    Add(triangles, a, b, d, tex);
                    Add(triangles, a, d, c, tex);
                }
                continue;
            }

            if (tile.Indices.Count >= 3)
            {
                for (var i = 0; i + 2 < tile.Indices.Count; i++)
                {
                    var a = points[tile.Indices[i]];
                    var b = points[tile.Indices[i + 1]];
                    var c = points[tile.Indices[i + 2]];
                    if ((i & 1) != 0)
                        (b, c) = (c, b);
                    Add(triangles, a, b, c, TextureAt(a, cells, bySlot, textures));
                }
            }
        }

        return triangles;
    }

    private static int TextureAt(
        Vector3 p, LevCellGrid cells, Dictionary<int, LevMaterial> bySlot, HeaderEnums? textures)
    {
        var x = Math.Clamp((int)MathF.Floor(p.X), 0, cells.Width - 1);
        var y = Math.Clamp((int)MathF.Floor(p.Y), 0, cells.Height - 1);
        foreach (var slot in new[] { cells.Cells[x, y].Material0, cells.Cells[x, y].Material1, cells.Cells[x, y].Material2 })
        {
            if (slot == 0xFF || !bySlot.TryGetValue(slot, out var material))
                continue;
            if (textures is not null)
                return LandscapeTextures.Resolve(material.Name, textures);
        }

        return LandscapeTextures.DefaultId;
    }

    private static void Add(List<MeshTriangle> triangles, Vector3 a, Vector3 b, Vector3 c, int textureId)
    {
        var n = Vector3.Cross(b - a, c - a);
        if (n.LengthSquared() < 1e-8f)
            return;
        if (n.Z < 0)
        {
            (b, c) = (c, b);
            n = -n;
        }

        triangles.Add(new MeshTriangle(
            a, b, c, Vector3.Normalize(n),
            new Vector2(a.X / LevHeightField.SampleSpacing, a.Y / LevHeightField.SampleSpacing),
            new Vector2(b.X / LevHeightField.SampleSpacing, b.Y / LevHeightField.SampleSpacing),
            new Vector2(c.X / LevHeightField.SampleSpacing, c.Y / LevHeightField.SampleSpacing),
            textureId));
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
        for (var o = start; o + 2 <= dest.Length; o += 2)
        {
            var index = BitConverter.ToUInt16(dest, o);
            if (index >= vertCount)
                break;
            indices.Add(index);
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
