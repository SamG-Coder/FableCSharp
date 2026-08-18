using System.Numerics;
using Fable.Formats.Levels;
using Fable.Formats.Meshes;
using Fable.Formats.Scene;
using Fable.Formats.World;

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
    public ushort[] Indices { get; init; } = [];
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
    /// One draw per 16 m cell
    /// (<c>00BF4570</c> DIP). Does not
    /// regroup across cells.
    /// </summary>
    public static TexturedMesh BuildCells(
        IReadOnlyList<LandscapeCell> cells)
    {
        var count = 0;
        var indexCount = 0;
        foreach (var cell in cells)
        {
            if (cell.PrimitiveCount > 0 && cell.Points is { Count: > 0 })
            {
                count += cell.Points.Count;
                indexCount += cell.PrimitiveCount * 3;
            }
            else
                count += cell.Faces.Count * 3;
        }

        var vertices = new MeshVertex[count];
        var indices = indexCount > 0 ? new ushort[indexCount] : [];
        var draws = new List<MeshDraw>(cells.Count);
        var cursor = 0;
        var icursor = 0;
        foreach (var cell in cells)
        {
            var first = cursor;
            uint firstIndex = 0;
            uint nIndex = 0;
            if (cell.PrimitiveCount > 0 &&
                cell.Points is { Count: > 0 } points &&
                cell.StripIndices is { Length: >= 3 } strip)
            {
                foreach (var p in points)
                    vertices[cursor++] = Vert(p.P, p.N, p.N, default, Vector3.One, p.Extra, 1f);
                firstIndex = (uint)icursor;
                for (var t = 0; t + 2 < strip.Length; t++)
                {
                    var (ia, ib, ic) = LandscapeStrip.Unwind(t, strip[t], strip[t + 1], strip[t + 2]);
                    if ((uint)ia >= (uint)points.Count ||
                        (uint)ib >= (uint)points.Count ||
                        (uint)ic >= (uint)points.Count)
                        continue;
                    indices[icursor++] = (ushort)ia;
                    indices[icursor++] = (ushort)ib;
                    indices[icursor++] = (ushort)ic;
                }

                nIndex = (uint)(icursor - (int)firstIndex);
            }
            else
            {
                foreach (var tri in cell.Faces)
                {
                    vertices[cursor++] = Vert(tri.A, tri.NormalA, tri.Normal, tri.UvA, tri.ColorA, tri.ExtraA, tri.ColorAlphaA);
                    vertices[cursor++] = Vert(tri.B, tri.NormalB, tri.Normal, tri.UvB, tri.ColorB, tri.ExtraB, tri.ColorAlphaB);
                    vertices[cursor++] = Vert(tri.C, tri.NormalC, tri.Normal, tri.UvC, tri.ColorC, tri.ExtraC, tri.ColorAlphaC);
                }
            }

            var n = (uint)(cursor - first);
            if (n == 0)
                continue;
            draws.Add(new MeshDraw(
                cell.TextureId, (uint)first, n,
                cell.TextureId1 == 0 ? cell.TextureId : cell.TextureId1,
                LandscapeCells.LayerForeground, 1f, false,
                IndexCount: nIndex, FirstIndex: firstIndex));
        }

        if (icursor < indices.Length)
            Array.Resize(ref indices, icursor);
        return new TexturedMesh { Vertices = vertices, Draws = [.. draws], Indices = indices };
    }

    /// <summary>
    /// File-local C3D verts, one VB range per
    /// <see cref="MeshFile"/>. World is the
    /// instance 3×4 (<c>009881F0</c>
    /// wrapper+496). Native
    /// <c>00BB2540</c> copies locals once and
    /// DIP each instance; it does not bake
    /// <c>ObjectTransform</c> into a soup.
    /// </summary>
    public static TexturedMesh BuildMeshes(
        IReadOnlyList<(MeshFile Mesh, Matrix4x4 Transform)> instances)
    {
        var unique = 0;
        var seenMesh = new HashSet<MeshFile>();
        foreach (var (mesh, _) in instances)
        {
            if (seenMesh.Add(mesh))
                unique += mesh.Triangles.Count * 3;
        }

        var vertices = new MeshVertex[unique];
        var templates = new Dictionary<MeshFile, MeshDraw[]>();
        var draws = new List<MeshDraw>(instances.Count);
        var cursor = 0;
        foreach (var (mesh, transform) in instances)
        {
            if (!templates.TryGetValue(mesh, out var groups))
            {
                // First-seen dest is already in
                // MeshFile.Triangles (00A9E1E0 × IBM).
                var source = mesh.Triangles;
                var layer = mesh.BoneCount > 0 ? SceneLayer.Palskin : SceneLayer.Prop;
                var built = new List<MeshDraw>();
                foreach (var group in source.GroupBy(tri => (
                    tri.TextureId,
                    tri.TextureId1 == 0 ? tri.TextureId : tri.TextureId1,
                    tri.SrcAlphaBlend,
                    tri.Layer)))
                {
                    var first = cursor;
                    foreach (var tri in group)
                    {
                        vertices[cursor++] = Vert(tri.A, tri.NormalA, tri.Normal, tri.UvA, tri.ColorA, tri.ExtraA, tri.ColorAlphaA);
                        vertices[cursor++] = Vert(tri.B, tri.NormalB, tri.Normal, tri.UvB, tri.ColorB, tri.ExtraB, tri.ColorAlphaB);
                        vertices[cursor++] = Vert(tri.C, tri.NormalC, tri.Normal, tri.UvC, tri.ColorC, tri.ExtraC, tri.ColorAlphaC);
                    }

                    var n = (uint)(cursor - first);
                    if (n == 0)
                        continue;
                    foreach (var pass in ScenePasses.DrawnPasses(layer))
                    {
                        built.Add(new MeshDraw(
                            group.Key.TextureId, (uint)first, n, group.Key.Item2,
                            pass.Bit, ScenePasses.ShaderMode(pass.Submit),
                            group.Key.SrcAlphaBlend || mesh.BoneCount > 0));
                    }
                }

                groups = [.. built];
                templates[mesh] = groups;
            }

            foreach (var draw in groups)
                draws.Add(draw with { World = transform });
        }

        return new TexturedMesh { Vertices = vertices, Draws = [.. draws] };
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

        var aIdx = a.Indices;
        var bIdx = b.Indices;
        ushort[] indices = [];
        if (aIdx.Length + bIdx.Length > 0)
        {
            indices = new ushort[aIdx.Length + bIdx.Length];
            aIdx.CopyTo(indices, 0);
            bIdx.CopyTo(indices, aIdx.Length);
        }

        var indexOff = (uint)aIdx.Length;
        for (var i = 0; i < b.Draws.Length; i++)
        {
            var d = draws[a.Draws.Length + i];
            if (d.IndexCount > 0)
                draws[a.Draws.Length + i] = d with { FirstIndex = d.FirstIndex + indexOff };
        }

        return new TexturedMesh { Vertices = vertices, Draws = SortByPass(draws), Indices = indices };
    }

    public static MeshDraw[] SortByPass(IReadOnlyList<MeshDraw> draws)
    {
        var list = draws.ToArray();
        Array.Sort(list, (a, b) =>
        {
            var rank = ScenePasses.Rank(a.PassBit).CompareTo(ScenePasses.Rank(b.PassBit));
            return rank != 0 ? rank : a.TextureId.CompareTo(b.TextureId);
        });
        return list;
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
