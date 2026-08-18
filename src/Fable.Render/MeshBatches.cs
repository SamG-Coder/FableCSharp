using System.Numerics;
using Fable.Formats.Meshes;
using Fable.Formats.Scene;

namespace Fable.Render;

/// <summary>
/// Groups world triangles by textures.big id so the renderer can bind one
/// sampler and draw a contiguous vertex range. Draw order follows
/// <see cref="ScenePasses.Registration"/>.
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
            .GroupBy(tri => (tri.Layer, tri.TextureId, tri.TextureId1 == 0 ? tri.TextureId : tri.TextureId1, tri.SrcAlphaBlend))
            .ToList();
        var vertices = new MeshVertex[triangles.Count * 3];
        var draws = new List<MeshDraw>(grouped.Count * 2);
        var cursor = 0;
        foreach (var group in grouped)
        {
            var first = cursor;
            foreach (var tri in group)
            {
                vertices[cursor++] = Vert(tri.A, tri.NormalA, tri.Normal, tri.UvA, tri.ColorA, tri.ExtraA, tri.ColorAlphaA);
                vertices[cursor++] = Vert(tri.B, tri.NormalB, tri.Normal, tri.UvB, tri.ColorB, tri.ExtraB, tri.ColorAlphaB);
                vertices[cursor++] = Vert(tri.C, tri.NormalC, tri.Normal, tri.UvC, tri.ColorC, tri.ExtraC, tri.ColorAlphaC);
            }

            var count = (uint)(cursor - first);
            foreach (var pass in ScenePasses.DrawnPasses(group.Key.Layer))
            {
                draws.Add(new MeshDraw(
                    group.Key.TextureId,
                    (uint)first,
                    count,
                    group.Key.Item3,
                    pass.Bit,
                    ScenePasses.ShaderMode(pass.Submit),
                    group.Key.SrcAlphaBlend));
            }
        }

        draws.Sort((a, b) =>
        {
            var rank = ScenePasses.Rank(a.PassBit).CompareTo(ScenePasses.Rank(b.PassBit));
            if (rank != 0)
                return rank;
            var tex = a.TextureId.CompareTo(b.TextureId);
            return tex != 0 ? tex : a.TextureId1.CompareTo(b.TextureId1);
        });

        return new TexturedMesh { Vertices = vertices, Draws = [.. draws] };
    }

    /// <summary>
    /// One C3D parse per mesh id, verts
    /// transformed per instance. No
    /// <c>WorldGeometry</c> triangle soup.
    /// </summary>
    public static TexturedMesh BuildMeshes(
        IReadOnlyList<(MeshFile Mesh, Matrix4x4 Transform)> instances)
    {
        var tris = new List<MeshTriangle>();
        foreach (var (mesh, transform) in instances)
        {
            foreach (var tri in mesh.Triangles)
            {
                var a = Vector3.Transform(tri.A, transform);
                var b = Vector3.Transform(tri.B, transform);
                var c = Vector3.Transform(tri.C, transform);
                var n = Vector3.TransformNormal(tri.Normal, transform);
                if (n.LengthSquared() < 1e-8f)
                    n = Vector3.UnitZ;
                else
                    n = Vector3.Normalize(n);
                tris.Add(tri with
                {
                    A = a, B = b, C = c, Normal = n,
                    NormalA = Unit(Vector3.TransformNormal(tri.NormalA, transform), n),
                    NormalB = Unit(Vector3.TransformNormal(tri.NormalB, transform), n),
                    NormalC = Unit(Vector3.TransformNormal(tri.NormalC, transform), n),
                });
            }
        }

        return Build(tris);
    }

    public static TexturedMesh Concat(TexturedMesh a, TexturedMesh b)
    {
        if (a.Vertices.Length == 0)
            return b;
        if (b.Vertices.Length == 0)
            return a;
        var vertices = new MeshVertex[a.Vertices.Length + b.Vertices.Length];
        a.Vertices.CopyTo(vertices, 0);
        b.Vertices.CopyTo(vertices, a.Vertices.Length);
        var draws = new MeshDraw[a.Draws.Length + b.Draws.Length];
        a.Draws.CopyTo(draws, 0);
        var off = (uint)a.Vertices.Length;
        for (var i = 0; i < b.Draws.Length; i++)
        {
            var d = b.Draws[i];
            draws[a.Draws.Length + i] = d with { FirstVertex = d.FirstVertex + off };
        }

        return new TexturedMesh { Vertices = vertices, Draws = draws };
    }

    private static Vector3 Unit(Vector3 n, Vector3 face) =>
        n.LengthSquared() < 1e-8f ? face : Vector3.Normalize(n);

    private static MeshVertex Vert(
        System.Numerics.Vector3 p, System.Numerics.Vector3 n, System.Numerics.Vector3 face,
        System.Numerics.Vector2 uv, System.Numerics.Vector3 color, System.Numerics.Vector3 extra,
        float alpha)
    {
        var normal = n.LengthSquared() < 1e-8f ? face : n;
        var tint = color.LengthSquared() < 1e-8f ? System.Numerics.Vector3.One : color;
        return new MeshVertex(p, normal, uv, new System.Numerics.Vector4(tint, alpha), extra);
    }
}
