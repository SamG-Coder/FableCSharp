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
            .GroupBy(tri => (tri.Layer, tri.TextureId, tri.TextureId1 == 0 ? tri.TextureId : tri.TextureId1, tri.SrcAlphaBlend, tri.Flag1))
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
            foreach (var pass in ScenePasses.DrawnPasses(group.Key.Layer, group.Key.Flag1))
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
    /// Extra <c>CPatchTesselationEdgeStrip</c>
    /// faces after the primary strip
    /// (<c>00BF4E90</c>). Not WAD 1 m fill.
    /// </summary>
    public static int ExtraFaceCount(LandscapeCell cell)
    {
        if (cell.ExtraStrips is not { Count: > 0 } extras)
            return 0;
        var n = 0;
        foreach (var extra in extras)
            n += extra.Faces.Count;
        return n;
    }

    /// <summary>
    /// One INDEX16 DIP for the primary strip
    /// then one per extra mesh+60
    /// (<c>00BF4570</c> / <c>00BF4E90</c>).
    /// Does not regroup across cells or merge
    /// extras onto the primary IB.
    /// </summary>
    public static TexturedMesh BuildCells(
        IReadOnlyList<LandscapeCell> cells)
    {
        var count = 0;
        var indexCount = 0;
        var drawCount = 0;
        foreach (var cell in cells)
        {
            CountMesh(cell.Points, cell.PrimitiveCount, cell.Faces, ref count, ref indexCount, ref drawCount);
            if (cell.ExtraStrips is not { Count: > 0 } extras)
                continue;
            foreach (var extra in extras)
                CountMesh(extra.Points, extra.PrimitiveCount, extra.Faces, ref count, ref indexCount, ref drawCount);
        }

        var vertices = new MeshVertex[count];
        var indices = indexCount > 0 ? new ushort[indexCount] : [];
        var draws = new List<MeshDraw>(drawCount);
        var cursor = 0;
        var icursor = 0;
        foreach (var cell in cells)
        {
            EmitMesh(
                cell.Points, cell.StripIndices, cell.PrimitiveCount, cell.Faces,
                cell.TextureId, cell.TextureId1,
                vertices, indices, ref cursor, ref icursor, draws);
            if (cell.ExtraStrips is not { Count: > 0 } extras)
                continue;
            foreach (var extra in extras)
                EmitMesh(
                    extra.Points, extra.StripIndices, extra.PrimitiveCount, extra.Faces,
                    extra.TextureId, extra.TextureId1,
                    vertices, indices, ref cursor, ref icursor, draws);
        }

        if (icursor < indices.Length)
            Array.Resize(ref indices, icursor);
        return new TexturedMesh { Vertices = vertices, Draws = [.. draws], Indices = indices };
    }

    private static void CountMesh(
        IReadOnlyList<LandscapePoint>? points,
        int primitiveCount,
        IReadOnlyList<MeshTriangle> faces,
        ref int verts,
        ref int indices,
        ref int draws)
    {
        if (primitiveCount > 0 && points is { Count: > 0 })
        {
            verts += points.Count;
            indices += primitiveCount * 3;
            draws++;
        }
        else if (faces.Count > 0)
        {
            verts += faces.Count * 3;
            draws++;
        }
    }

    private static void EmitMesh(
        IReadOnlyList<LandscapePoint>? points,
        ushort[]? strip,
        int primitiveCount,
        IReadOnlyList<MeshTriangle> faces,
        int textureId,
        int textureId1,
        MeshVertex[] vertices,
        ushort[] indices,
        ref int cursor,
        ref int icursor,
        List<MeshDraw> draws)
    {
        var first = cursor;
        uint firstIndex = 0;
        uint nIndex = 0;
        if (primitiveCount > 0 &&
            points is { Count: > 0 } pts &&
            strip is { Length: >= 3 } idx)
        {
            foreach (var p in pts)
                vertices[cursor++] = Vert(p.P, p.N, p.N, default, Vector3.One, p.Extra, 1f);
            firstIndex = (uint)icursor;
            for (var t = 0; t + 2 < idx.Length; t++)
            {
                var (ia, ib, ic) = LandscapeStrip.Unwind(t, idx[t], idx[t + 1], idx[t + 2]);
                if ((uint)ia >= (uint)pts.Count ||
                    (uint)ib >= (uint)pts.Count ||
                    (uint)ic >= (uint)pts.Count)
                    continue;
                indices[icursor++] = (ushort)ia;
                indices[icursor++] = (ushort)ib;
                indices[icursor++] = (ushort)ic;
            }

            nIndex = (uint)(icursor - (int)firstIndex);
        }
        else
        {
            foreach (var tri in faces)
            {
                vertices[cursor++] = Vert(tri.A, tri.NormalA, tri.Normal, tri.UvA, tri.ColorA, tri.ExtraA, tri.ColorAlphaA);
                vertices[cursor++] = Vert(tri.B, tri.NormalB, tri.Normal, tri.UvB, tri.ColorB, tri.ExtraB, tri.ColorAlphaB);
                vertices[cursor++] = Vert(tri.C, tri.NormalC, tri.Normal, tri.UvC, tri.ColorC, tri.ExtraC, tri.ColorAlphaC);
            }
        }

        var n = (uint)(cursor - first);
        if (n == 0)
            return;
        draws.Add(new MeshDraw(
            textureId, (uint)first, n,
            textureId1 == 0 ? textureId : textureId1,
            LandscapeCells.LayerForeground, 1f, false,
            IndexCount: nIndex, FirstIndex: firstIndex));
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
                    tri.Layer,
                    tri.Flag1)))
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
                    foreach (var pass in ScenePasses.DrawnPasses(layer, group.Key.Flag1))
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
