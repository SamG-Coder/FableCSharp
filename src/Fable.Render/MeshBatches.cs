using Fable.Formats.Meshes;

namespace Fable.Render;

/// <summary>
/// Groups world triangles by textures.big id so the renderer can bind one
/// sampler and draw a contiguous vertex range.
/// </summary>
public sealed class TexturedMesh
{
    public required MeshVertex[] Vertices { get; init; }
    public required MeshDraw[] Draws { get; init; }
}

public static class MeshBatches
{
    public static TexturedMesh Build(IReadOnlyList<MeshTriangle> triangles)
    {
        var grouped = triangles
            .GroupBy(tri => (tri.TextureId, tri.TextureId1 == 0 ? tri.TextureId : tri.TextureId1))
            .OrderBy(group => group.Key.TextureId)
            .ThenBy(group => group.Key.Item2)
            .ToList();
        var vertices = new MeshVertex[triangles.Count * 3];
        var draws = new MeshDraw[grouped.Count];
        var cursor = 0;
        var draw = 0;
        foreach (var group in grouped)
        {
            var first = cursor;
            foreach (var tri in group)
            {
                vertices[cursor++] = Vert(tri.A, tri.NormalA, tri.Normal, tri.UvA, tri.ColorA);
                vertices[cursor++] = Vert(tri.B, tri.NormalB, tri.Normal, tri.UvB, tri.ColorB);
                vertices[cursor++] = Vert(tri.C, tri.NormalC, tri.Normal, tri.UvC, tri.ColorC);
            }

            draws[draw++] = new MeshDraw(group.Key.TextureId, (uint)first, (uint)(cursor - first), group.Key.Item2);
        }

        return new TexturedMesh { Vertices = vertices, Draws = draws };
    }

    private static MeshVertex Vert(System.Numerics.Vector3 p, System.Numerics.Vector3 n, System.Numerics.Vector3 face, System.Numerics.Vector2 uv, System.Numerics.Vector3 color)
    {
        var normal = n.LengthSquared() < 1e-8f ? face : n;
        var tint = color.LengthSquared() < 1e-8f ? System.Numerics.Vector3.One : color;
        return new MeshVertex(p, normal, uv, new System.Numerics.Vector4(tint, 1f));
    }
}
