using System.Numerics;
using Fable.Formats.Meshes;

namespace Fable.Formats.Levels;

/// <summary>
/// Coarse landscape lattice stored in the runtime STB copy of a .lev.
/// Vertices sit on a 16-unit grid in WLD space (MapX/MapY origin).
/// </summary>
public sealed class LevHeightField
{
    public const int HeaderPad = 2048;
    public const int VertexStreamOffset = 2056;
    public const float SampleSpacing = 16f;
    public const int RecordSize = 36;

    public required int CellsX { get; init; }
    public required int CellsY { get; init; }
    public required float OriginX { get; init; }
    public required float OriginY { get; init; }
    public required float[,] Heights { get; init; }
    public required int SampleCount { get; init; }

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
        return new LevHeightField
        {
            CellsX = cellsX,
            CellsY = cellsY,
            OriginX = mapX,
            OriginY = mapY,
            Heights = heights,
            SampleCount = samples,
        };
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

    private static void Add(List<MeshTriangle> triangles, Vector3 a, Vector3 b, Vector3 c)
    {
        var n = Vector3.Cross(b - a, c - a);
        if (n.LengthSquared() < 1e-8f)
            return;
        triangles.Add(new MeshTriangle(a, b, c, Vector3.Normalize(n)));
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
