using System.Numerics;
using Fable.Formats.Defs;
using Fable.Formats.IO;
using Fable.Formats.Meshes;

namespace Fable.Formats.Levels;

/// <summary>
/// Per-16-unit landscape tile stored in the runtime STB copy of a .lev.
/// Each table record points at a raw-LZO payload of 15-byte world-space verts
/// plus optional CPatchTesselationEdgeStrip objects after the primary strip.
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
            stamped += StampVerts(dest, originX, originY, width, height, tile.Vertices);
            foreach (var extra in tile.Extras)
                stamped += StampVerts(dest, originX, originY, width, height, extra.Vertices);
        }

        return stamped;
    }

    private static int StampVerts(
        float[,] dest, float originX, float originY, int width, int height,
        IReadOnlyList<LevTileVertex> verts)
    {
        var stamped = 0;
        foreach (var vert in verts)
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

            var at = new Dictionary<(int X, int Y), (Vector3 P, Vector3 N)>();
            for (var i = 0; i < tile.Vertices.Count; i++)
            {
                var v = tile.Vertices[i];
                var p = points[i];
                at[((int)MathF.Round(p.X), (int)MathF.Round(p.Y))] = (p, v.Normal);
            }

            var minX = at.Count == 0 ? 0 : at.Keys.Min(k => k.X);
            var maxX = at.Count == 0 ? 0 : at.Keys.Max(k => k.X);
            var minY = at.Count == 0 ? 0 : at.Keys.Min(k => k.Y);
            var maxY = at.Count == 0 ? 0 : at.Keys.Max(k => k.Y);
            var span = Math.Max(1, (maxX - minX + 1) * (maxY - minY + 1));
            var filled = at.Count / (float)span;
            var useGrid = tile.Vertices.Count == 289 &&
                          (maxX - minX) == 16 && (maxY - minY) == 16 && filled >= 0.98f;

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
                    var tex = LayersAt(a.P, cells, bySlot, textures);
                    if (tex.A < 0)
                        continue;
                    Add(triangles, a, b, d, tex.A, tex.B);
                    Add(triangles, a, d, c, tex.A, tex.B);
                }
                continue;
            }

            if (tile.Indices.Count >= 3)
                AddStrip(triangles, tile.Vertices, points, tile.Indices, cells, bySlot, textures);

            foreach (var extra in tile.Extras)
            {
                var extraPoints = new Vector3[extra.Vertices.Count];
                for (var i = 0; i < extra.Vertices.Count; i++)
                {
                    var ev = extra.Vertices[i];
                    extraPoints[i] = new Vector3(ev.WorldX - originX, ev.WorldY - originY, ev.Z);
                }

                AddStrip(triangles, extra.Vertices, extraPoints, extra.Indices, cells, bySlot, textures);
            }
        }

        return triangles;
    }

    private static void AddStrip(
        List<MeshTriangle> triangles,
        IReadOnlyList<LevTileVertex> verts,
        Vector3[] points,
        IReadOnlyList<int> indices,
        LevCellGrid cells,
        Dictionary<int, LevMaterial> bySlot,
        HeaderEnums? textures)
    {
        for (var i = 0; i + 2 < indices.Count; i++)
        {
            var ia = indices[i];
            var ib = indices[i + 1];
            var ic = indices[i + 2];
            if ((uint)ia >= (uint)verts.Count || (uint)ib >= (uint)verts.Count || (uint)ic >= (uint)verts.Count)
                continue;
            var a = PointOf(verts[ia], points[ia]);
            var b = PointOf(verts[ib], points[ib]);
            var c = PointOf(verts[ic], points[ic]);
            if ((i & 1) != 0)
                (b, c) = (c, b);
            var mid = (a.P + b.P + c.P) / 3f;
            var tex = LayersAt(mid, cells, bySlot, textures);
            if (tex.A < 0)
                continue;
            Add(triangles, a, b, c, tex.A, tex.B);
        }
    }

    private static (Vector3 P, Vector3 N) PointOf(LevTileVertex v, Vector3 p) =>
        (p, v.Normal);

    private static (int A, int B) LayersAt(
        Vector3 p, LevCellGrid cells, Dictionary<int, LevMaterial> bySlot, HeaderEnums? textures)
    {
        var x = Math.Clamp((int)MathF.Floor(p.X), 0, cells.Width - 1);
        var y = Math.Clamp((int)MathF.Floor(p.Y), 0, cells.Height - 1);
        var found = new int[2];
        var n = 0;
        var anyNamed = false;
        foreach (var slot in new[] { cells.Cells[x, y].Material0, cells.Cells[x, y].Material1, cells.Cells[x, y].Material2 })
        {
            if (slot == 0xFF || !bySlot.TryGetValue(slot, out var material))
                continue;
            anyNamed = true;
            var id = LandscapeTextures.TryResolve(material.Name, textures);
            if (id is null)
                continue;
            if (n == 0 || found[n - 1] != id.Value)
                found[n++] = id.Value;
            if (n == 2)
                break;
        }

        if (n == 0)
            return anyNamed ? (-1, -1) : (LandscapeTextures.DefaultId, LandscapeTextures.DefaultId);
        return n == 1 ? (found[0], found[0]) : (found[0], found[1]);
    }

    private static void Add(
        List<MeshTriangle> triangles,
        (Vector3 P, Vector3 N) a,
        (Vector3 P, Vector3 N) b,
        (Vector3 P, Vector3 N) c,
        int textureId,
        int textureId1)
    {
        var n = Vector3.Cross(b.P - a.P, c.P - a.P);
        if (n.LengthSquared() < 1e-8f)
            return;
        if (n.Z < 0)
        {
            (b, c) = (c, b);
            n = -n;
        }

        var face = Vector3.Normalize(n);
        triangles.Add(new MeshTriangle(
            a.P, b.P, c.P, face,
            new Vector2(a.P.X * LandscapeTextures.UvScale, a.P.Y * LandscapeTextures.UvScale),
            new Vector2(b.P.X * LandscapeTextures.UvScale, b.P.Y * LandscapeTextures.UvScale),
            new Vector2(c.P.X * LandscapeTextures.UvScale, c.P.Y * LandscapeTextures.UvScale),
            textureId,
            Vector3.One, Vector3.One, Vector3.One,
            textureId1,
            a.N, b.N, c.N,
            SceneLayer.Landscape));
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
                BitConverter.ToSingle(dest, o + 4),
                PackedDirection.Unpack(BitConverter.ToUInt32(dest, o + 8)),
                PackedDirection.ColorRgb(dest[o + 12], dest[o + 13], dest[o + 14]));
        }

        var primitiveCount = BitConverter.ToUInt16(dest, 4);
        var extraCount = BitConverter.ToUInt16(dest, 0);
        var flag = BitConverter.ToUInt16(dest, 18);
        var hasPrimaryStrip = flag != 256;
        // D3D triangle-strip PrimitiveCount = IndexCount - 2.
        var indexCount = hasPrimaryStrip ? primitiveCount + 2 : 0;
        var indices = hasPrimaryStrip
            ? ReadIndices(dest, count, need, indexCount)
            : [];
        var extraStart = hasPrimaryStrip ? need + indexCount * 2 : need;
        return new LevTile(
            index,
            0, 0,
            verts[0].WorldX,
            verts[0].WorldY,
            verts,
            indices,
            ReadExtras(dest, extraStart, extraCount));
    }

    internal static IReadOnlyList<int> ReadIndices(byte[] dest, int vertCount, int start, int count = -1)
    {
        if (start + 6 > dest.Length)
            return [];

        var limit = dest.Length;
        if (count > 0)
            limit = Math.Min(limit, start + count * 2);

        var indices = new List<int>();
        for (var o = start; o + 2 <= limit; o += 2)
        {
            var index = BitConverter.ToUInt16(dest, o);
            if (index >= vertCount)
                break;
            indices.Add(index);
        }

        return indices.Count >= 3 ? indices : [];
    }

    /// <summary>
    /// CPatchTesselationEdgeStrip blobs after the primary strip.
    /// 30-byte header: vert count, primitive count, format… then 15-byte
    /// world verts and PrimitiveCount+2 strip indices.
    /// </summary>
    internal static IReadOnlyList<LevTileExtra> ReadExtras(byte[] dest, int start, int declaredObjects)
    {
        const int header = 30;
        var extras = new List<LevTileExtra>();
        var cursor = start;
        var budget = Math.Max(declaredObjects, 0) + 2;
        while (cursor + header + 15 <= dest.Length && extras.Count < budget)
        {
            var v = BitConverter.ToUInt16(dest, cursor);
            var primitives = BitConverter.ToUInt16(dest, cursor + 2);
            var fmt = BitConverter.ToUInt16(dest, cursor + 4);
            if (v is < 3 or > 400 || primitives < 3 || primitives > 4000)
                break;

            var indexCount = primitives + 2;
            var vertAt = cursor + header;
            var end = vertAt + v * VertexStride + indexCount * 2;
            if (end > dest.Length)
                break;

            var x = BitConverter.ToUInt16(dest, vertAt);
            var y = BitConverter.ToUInt16(dest, vertAt + 2);
            var z = BitConverter.ToSingle(dest, vertAt + 4);
            if (x is < 2000 or > 6000 || y is < 2000 or > 6000 || z is < 0f or > 200f)
                break;

            var verts = new LevTileVertex[v];
            for (var i = 0; i < v; i++)
            {
                var o = vertAt + i * VertexStride;
                verts[i] = new LevTileVertex(
                    BitConverter.ToUInt16(dest, o),
                    BitConverter.ToUInt16(dest, o + 2),
                    BitConverter.ToSingle(dest, o + 4),
                    PackedDirection.Unpack(BitConverter.ToUInt32(dest, o + 8)),
                    PackedDirection.ColorRgb(dest[o + 12], dest[o + 13], dest[o + 14]));
            }

            extras.Add(new LevTileExtra(
                0xFFFF,
                0xFFFF,
                fmt,
                verts,
                ReadIndices(dest, v, vertAt + v * VertexStride, indexCount)));
            cursor = end;
        }

        return extras;
    }
}

public readonly record struct LevTile(
    int Index,
    int CellX,
    int CellY,
    float OriginX,
    float OriginY,
    IReadOnlyList<LevTileVertex> Vertices,
    IReadOnlyList<int> Indices,
    IReadOnlyList<LevTileExtra> Extras);

/// <summary>
/// One CPatchTesselationEdgeStrip after the primary tile mesh.
/// </summary>
public readonly record struct LevTileExtra(
    int Attach0,
    int Attach1,
    ushort Format,
    IReadOnlyList<LevTileVertex> Vertices,
    IReadOnlyList<int> Indices);

/// <summary>
/// 15-byte STB vert. Extra 7 = packed 11-11-10 normal + 3 bytes.
/// VSHADER_LANDSCAPE_FOREGROUND writes oD0.xyz from light constants, not
/// those 3 bytes. Byte 0 of the triple is 0xFF (v3.x, oD0.w scale).
/// </summary>
public readonly record struct LevTileVertex(
    ushort WorldX,
    ushort WorldY,
    float Z,
    Vector3 Normal,
    Vector3 ExtraRgb);
