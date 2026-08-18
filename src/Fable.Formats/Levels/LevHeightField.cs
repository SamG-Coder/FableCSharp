using System.Numerics;
using Fable.Formats.Defs;
using Fable.Formats.Meshes;

namespace Fable.Formats.Levels;

/// <summary>
/// Landscape heights from the runtime STB copy of a .lev.
/// Coarse vertices sit on a 16-unit WLD lattice. Fine 1-unit samples start
/// as bilinear from that lattice, then STB tile verts overwrite cells they hit.
/// </summary>
public sealed class LevHeightField
{
    public const int HeaderPad = 2048;
    public const int VertexStreamOffset = 2056;
    public const float SampleSpacing = 16f;
    public const int RecordSize = 36;

    /// <summary>
    /// Vertex-stream count from STB size.
    /// Does not tessellate or stamp tiles.
    /// </summary>
    public static int CountSamplesFromSize(int stbBytes)
    {
        if (stbBytes <= VertexStreamOffset)
            return 0;
        return (stbBytes - VertexStreamOffset) / RecordSize * 2;
    }

    public required int CellsX { get; init; }
    public required int CellsY { get; init; }
    public required float OriginX { get; init; }
    public required float OriginY { get; init; }
    public required float[,] Heights { get; init; }
    public required int SampleCount { get; init; }
    public required int FineWidth { get; init; }
    public required int FineHeight { get; init; }
    public required int TileCount { get; init; }
    public required LevTileMesh Tiles { get; init; }

    private float[,]? _fineHeights;
    private int _fineSampleCount;

    /// <summary>
    /// 1 m bilinear + tile stamp. Native
    /// <c>00BF4570</c> draws stored tiles,
    /// not this grid. Built on first read.
    /// </summary>
    public float[,] FineHeights
    {
        get
        {
            EnsureFine();
            return _fineHeights!;
        }
    }

    public int FineSampleCount
    {
        get
        {
            EnsureFine();
            return _fineSampleCount;
        }
    }

    public static LevHeightField Parse(byte[] stbLev, float mapX, float mapY, int localWidth, int localHeight)
    {
        var cellsX = Math.Max(1, (int)MathF.Round(localWidth / SampleSpacing));
        var cellsY = Math.Max(1, (int)MathF.Round(localHeight / SampleSpacing));
        var heights = new float[cellsX + 1, cellsY + 1];
        var filled = new bool[cellsX + 1, cellsY + 1];
        var samples = 0;

        for (var offset = VertexStreamOffset; offset + RecordSize <= stbLev.Length; offset += RecordSize)
        {
            if (!TryReadSample(stbLev, offset, mapX, mapY, cellsX, cellsY, out var ix0, out var iy0, out var z0) ||
                !TryReadSample(stbLev, offset + 12, mapX, mapY, cellsX, cellsY, out var ix1, out var iy1, out var z1))
                break;

            heights[ix0, iy0] = z0;
            heights[ix1, iy1] = z1;
            filled[ix0, iy0] = true;
            filled[ix1, iy1] = true;
            samples += 2;
        }

        FillMissing(heights, filled, cellsX, cellsY);

        var tiles = LevTileMesh.Parse(stbLev, mapX, mapY, cellsX, cellsY);
        return new LevHeightField
        {
            CellsX = cellsX,
            CellsY = cellsY,
            OriginX = mapX,
            OriginY = mapY,
            Heights = heights,
            SampleCount = samples,
            FineWidth = localWidth,
            FineHeight = localHeight,
            TileCount = tiles.Tiles.Count,
            Tiles = tiles,
        };
    }

    private void EnsureFine()
    {
        if (_fineHeights is not null)
            return;
        var fine = new float[FineWidth + 1, FineHeight + 1];
        for (var y = 0; y <= FineHeight; y++)
        for (var x = 0; x <= FineWidth; x++)
            fine[x, y] = SampleBilinear(Heights, CellsX, CellsY, x / SampleSpacing, y / SampleSpacing);
        _fineSampleCount = Tiles.StampOnto(fine, OriginX, OriginY, FineWidth, FineHeight);
        _fineHeights = fine;
    }

    public IReadOnlyList<MeshTriangle> ToTileTriangles(
        LevCellGrid cells,
        IReadOnlyList<LevMaterial> materials,
        HeaderEnums? textures = null)
    {
        // STB primary strip + CPatchTesselationEdgeStrip only. Filling 1 m
        // holes from the cell table is not in the exe draw path (00BF4570
        // submits stored tessellation). Water/decal passes are UNREAD.
        return Tiles.ToTriangles(OriginX, OriginY, cells, materials, textures);
    }

    public IReadOnlyList<MeshTriangle> ToFineTriangles(
        LevCellGrid cells,
        IReadOnlyList<LevMaterial> materials,
        HeaderEnums? textures = null)
    {
        var bySlot = materials.ToDictionary(item => item.Slot);
        var triangles = new List<MeshTriangle>(cells.Width * cells.Height * 2);
        for (var y = 0; y < cells.Height; y++)
        for (var x = 0; x < cells.Width; x++)
        {
            var a = Corner(x, y);
            var b = Corner(x + 1, y);
            var c = Corner(x, y + 1);
            var d = Corner(x + 1, y + 1);
            var tex = ResolveTexture(cells.Cells[x, y], bySlot, textures);
            Add(triangles, a, b, d, tex);
            Add(triangles, a, d, c, tex);
        }

        return triangles;
    }

    public IReadOnlyList<MeshTriangle> ToLocalTriangles()
    {
        var triangles = new List<MeshTriangle>(CellsX * CellsY * 2);
        for (var y = 0; y < CellsY; y++)
        for (var x = 0; x < CellsX; x++)
        {
            var a = new Vector3(x * SampleSpacing, y * SampleSpacing, Heights[x, y]);
            var b = new Vector3((x + 1) * SampleSpacing, y * SampleSpacing, Heights[x + 1, y]);
            var c = new Vector3(x * SampleSpacing, (y + 1) * SampleSpacing, Heights[x, y + 1]);
            var d = new Vector3((x + 1) * SampleSpacing, (y + 1) * SampleSpacing, Heights[x + 1, y + 1]);
            Add(triangles, a, b, d);
            Add(triangles, a, d, c);
        }

        return triangles;
    }

    private Vector3 Corner(int x, int y)
    {
        var z = x >= 0 && y >= 0 && x <= FineWidth && y <= FineHeight
            ? FineHeights[x, y]
            : SampleBilinear(Heights, CellsX, CellsY, x / SampleSpacing, y / SampleSpacing);
        return new Vector3(x, y, z);
    }

    private float SampleBilinear(float fx, float fy) =>
        SampleBilinear(Heights, CellsX, CellsY, fx, fy);

    private static float SampleBilinear(float[,] heights, int cellsX, int cellsY, float fx, float fy)
    {
        var x0 = Math.Clamp((int)MathF.Floor(fx), 0, cellsX);
        var y0 = Math.Clamp((int)MathF.Floor(fy), 0, cellsY);
        var x1 = Math.Min(x0 + 1, cellsX);
        var y1 = Math.Min(y0 + 1, cellsY);
        var tx = Math.Clamp(fx - x0, 0f, 1f);
        var ty = Math.Clamp(fy - y0, 0f, 1f);
        var a = heights[x0, y0];
        var b = heights[x1, y0];
        var c = heights[x0, y1];
        var d = heights[x1, y1];
        return (a * (1 - tx) + b * tx) * (1 - ty) + (c * (1 - tx) + d * tx) * ty;
    }

    private static int ResolveTexture(LevCell cell, Dictionary<int, LevMaterial> bySlot, HeaderEnums? textures)
    {
        foreach (var slot in new[] { cell.Material0, cell.Material1, cell.Material2 })
        {
            if (slot == 0xFF || !bySlot.TryGetValue(slot, out var material))
                continue;
            if (textures is not null)
                return LandscapeTextures.Resolve(material.Name, textures);
        }

        return LandscapeTextures.DefaultId;
    }

    private static void Add(
        List<MeshTriangle> triangles, Vector3 a, Vector3 b, Vector3 c, int textureId = 0, int textureId1 = 0)
    {
        var n = Vector3.Cross(b - a, c - a);
        if (n.LengthSquared() < 1e-8f)
            return;
        triangles.Add(new MeshTriangle(
            a, b, c, Vector3.Normalize(n),
            LandscapeTextures.ProjectOt1(a),
            LandscapeTextures.ProjectOt1(b),
            LandscapeTextures.ProjectOt1(c),
            textureId,
            Vector3.One, Vector3.One, Vector3.One,
            textureId1 == 0 ? textureId : textureId1,
            default, default, default,
            SceneLayer.Landscape));
    }

    private static bool TryReadSample(
        byte[] data, int offset, float mapX, float mapY, int cellsX, int cellsY,
        out int ix, out int iy, out float z)
    {
        var x = BitConverter.ToSingle(data, offset);
        var y = BitConverter.ToSingle(data, offset + 4);
        z = BitConverter.ToSingle(data, offset + 8);
        ix = (int)MathF.Round((x - mapX) / SampleSpacing);
        iy = (int)MathF.Round((y - mapY) / SampleSpacing);
        return ix is >= 0 && iy is >= 0 && ix <= cellsX && iy <= cellsY
               && z is >= 0f and <= 200f
               && MathF.Abs((x - mapX) - ix * SampleSpacing) < 0.05f
               && MathF.Abs((y - mapY) - iy * SampleSpacing) < 0.05f;
    }

    private static void FillMissing(float[,] heights, bool[,] filled, int cellsX, int cellsY)
    {
        for (var y = 0; y <= cellsY; y++)
        for (var x = 0; x <= cellsX; x++)
        {
            if (filled[x, y])
                continue;
            var sum = 0f;
            var n = 0;
            for (var dy = -1; dy <= 1; dy++)
            for (var dx = -1; dx <= 1; dx++)
            {
                var nx = x + dx;
                var ny = y + dy;
                if (nx < 0 || ny < 0 || nx > cellsX || ny > cellsY || !filled[nx, ny])
                    continue;
                sum += heights[nx, ny];
                n++;
            }
            heights[x, y] = n > 0 ? sum / n : 0f;
        }
    }
}
