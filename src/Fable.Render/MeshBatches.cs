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
            .GroupBy(tri => tri.TextureId)
            .OrderBy(group => group.Key)
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
                vertices[cursor++] = new MeshVertex(tri.A, tri.Normal, tri.UvA);
                vertices[cursor++] = new MeshVertex(tri.B, tri.Normal, tri.UvB);
                vertices[cursor++] = new MeshVertex(tri.C, tri.Normal, tri.UvC);
            }

            draws[draw++] = new MeshDraw(group.Key, (uint)first, (uint)(cursor - first));
        }

        return new TexturedMesh { Vertices = vertices, Draws = draws };
    }
}
