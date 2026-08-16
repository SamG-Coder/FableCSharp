using System.Numerics;
using Fable.Formats.IO;

namespace Fable.Formats.Meshes;

public sealed class MeshFile
{
    public required string Name { get; init; }
    public required int EntryType { get; init; }
    public required IReadOnlyList<MeshMaterial> Materials { get; init; }
    public required IReadOnlyList<MeshTriangle> Triangles { get; init; }
    public required Vector3 BoundsMin { get; init; }
    public required Vector3 BoundsMax { get; init; }
    public int DeclaredTriangles { get; init; }
    public int StripFaces { get; init; }
    public int ListFaces { get; init; }
    public int NoBlockFaces { get; init; }
    public int DegenerateSkipped { get; init; }

    public static MeshFile? TryParse(byte[] data, int entryType = -1)
    {
        try
        {
            return Parse(data, entryType);
        }
        catch
        {
            return null;
        }
    }

    public static MeshFile Parse(byte[] data, int entryType = -1)
    {
        var cursor = 0;
        var name = ReadCString(data, ref cursor);
        cursor += 1; // AnimatedFlag
        cursor += 12 + 4 + 12 + 12; // sphere + radius + aabb
        var helperCount = ReadU16(data, ref cursor);
        var dummyCount = ReadU16(data, ref cursor);
        var packedNamesSize = ReadU16(data, ref cursor);
        var volumeCount = ReadU16(data, ref cursor);
        var generatorCount = ReadU16(data, ref cursor);

        if (helperCount > 0)
            Lzo.DecompressFramed(data, ref cursor, 20 * helperCount);
        if (dummyCount > 0)
            Lzo.DecompressFramed(data, ref cursor, 56 * dummyCount);
        if (packedNamesSize > 0)
            Lzo.DecompressFramed(data, ref cursor, packedNamesSize);

        for (var i = 0; i < volumeCount; i++)
        {
            cursor += 4;
            ReadCString(data, ref cursor);
            var planeCount = ReadU32(data, ref cursor);
            if (planeCount > 0)
                Lzo.DecompressFramed(data, ref cursor, 16 * (int)planeCount);
        }

        for (var i = 0; i < generatorCount; i++)
        {
            cursor += 48 + 4;
            ReadCString(data, ref cursor);
            cursor += 4 + 1;
        }

        var materialCount = ReadI32(data, ref cursor);
        var primitiveCount = ReadI32(data, ref cursor);
        var boneCount = ReadI32(data, ref cursor);
        var boneNameSize = ReadI32(data, ref cursor);
        cursor += 1; // cloth
        cursor += 4; // static/animated block totals

        if (boneCount is < 0 or > 1000)
            throw new InvalidDataException($"Invalid bone count {boneCount} at {cursor}.");
        if (boneCount > 0)
        {
            Need(data, cursor, 2 * boneCount);
            cursor += 2 * boneCount;
            Lzo.DecompressFramed(data, ref cursor, boneNameSize);
            Lzo.DecompressFramed(data, ref cursor, 60 * boneCount);
            Lzo.DecompressFramed(data, ref cursor, 48 * boneCount);
            Lzo.DecompressFramed(data, ref cursor, 64 * boneCount);
        }

        Need(data, cursor, 48);
        cursor += 48; // root matrix
        var afterBones = cursor;

        var materials = new List<MeshMaterial>(Math.Max(materialCount, 0));
        for (var i = 0; i < materialCount; i++)
        {
            var id = ReadI32(data, ref cursor);
            var matName = ReadCString(data, ref cursor);
            var decal = ReadI32(data, ref cursor);
            var diffuse = ReadI32(data, ref cursor);
            var bump = ReadI32(data, ref cursor);
            var reflection = ReadI32(data, ref cursor);
            var illumination = ReadI32(data, ref cursor);
            cursor += 4 + 4; // MapFlags + SelfIllumination
            cursor += 4; // two-sided / transparent / boolean-alpha / degenerate
            var useFilenames = data[cursor++] != 0;
            if (useFilenames)
            {
                for (var j = 0; j < 4; j++)
                    ReadCString(data, ref cursor);
            }

            materials.Add(new MeshMaterial(id, matName, diffuse, bump, decal, reflection, illumination));
        }

        var triangles = new List<MeshTriangle>(1024);
        var boundsMin = new Vector3(float.MaxValue);
        var boundsMax = new Vector3(float.MinValue);
        var declaredTriangles = 0;
        var stripFaces = 0;
        var listFaces = 0;
        var noBlockFaces = 0;
        var degenerateSkipped = 0;

        for (var i = 0; i < primitiveCount; i++)
        {
            uint vertexCount = 0, indexCount = 0;
            var stride = 0;
            var vBytes = 0;
            var iBytes = 0;
            try
            {
            var materialIndex = ReadI32(data, ref cursor);
            var textureId = materialIndex >= 0 && materialIndex < materials.Count
                ? materials[materialIndex].DiffuseMapId
                : 0;
            var reps = ReadI32(data, ref cursor);
            cursor += 12 + 4 + 4;
            var staticBlocks = ReadU32(data, ref cursor);
            var animatedBlocks = ReadU32(data, ref cursor);
            vertexCount = ReadU32(data, ref cursor);
            declaredTriangles += (int)ReadU32(data, ref cursor);
            indexCount = ReadU32(data, ref cursor);
            var initFlags = ReadU32(data, ref cursor);
            cursor += 8;

            var blocks = new List<(uint Count, uint Start, bool Strip)>();
            for (var b = 0; b < staticBlocks; b++)
            {
                var count = ReadU32(data, ref cursor);
                var start = ReadU32(data, ref cursor);
                var strip = data[cursor++] != 0;
                cursor += 2;
                cursor += 4;
                blocks.Add((count, start, strip));
            }

            for (var b = 0; b < animatedBlocks; b++)
            {
                cursor += 8 + 3 + 4 + 2 + 1;
                var groupCount = data[cursor++];
                cursor += groupCount;
            }

            var scale = new Vector3(ReadF32(data, ref cursor), ReadF32(data, ref cursor), ReadF32(data, ref cursor));
            cursor += 4;
            var offset = new Vector3(ReadF32(data, ref cursor), ReadF32(data, ref cursor), ReadF32(data, ref cursor));
            cursor += 4;

            Need(data, cursor, 8);
            stride = (int)ReadU32(data, ref cursor);
            ReadU32(data, ref cursor);
            if (stride <= 0)
                stride = EstimateStride((int)initFlags, animatedBlocks > 0);

            var repeat = reps <= 1 ? 1 : reps;
            vBytes = stride * (int)vertexCount * repeat;
            var beforeVerts = cursor;
            var vertices = vBytes > 0 ? Lzo.DecompressFramed(data, ref cursor, vBytes) : [];
            var afterVerts = cursor;
            iBytes = 2 * repeat * (int)indexCount;
            var indices = iBytes > 0 ? Lzo.DecompressFramed(data, ref cursor, iBytes) : [];
            var afterIdx = cursor;

            var clothCount = ReadU32(data, ref cursor);
            for (var c = 0; c < clothCount; c++)
            {
                cursor += 8;
                var progLen = (int)ReadU32(data, ref cursor);
                if (progLen > 0)
                    Lzo.DecompressFramed(data, ref cursor, progLen);
            }

            if (vertices.Length == 0 || indices.Length < 6)
                continue;

            var packedPos = (initFlags & 4) != 0 && (initFlags & 0x10) == 0;
            var packedNorm = (initFlags & 4) != 0;
            var hasBones = animatedBlocks > 0;
            if (entryType == 4 || (stride == 36 && animatedBlocks == 0) || repeat > 1)
            {
                packedPos = false;
                if (entryType == 4 || stride == 36)
                    packedNorm = false;
            }

            var uvOffset = PackedUvOffset(entryType, stride, initFlags, hasBones);
            var vertCount = vertices.Length / Math.Max(stride, 1);
            var positions = new Vector3[vertCount];
            var uvs = new Vector2[vertCount];
            for (var v = 0; v < vertCount; v++)
            {
                var o = v * stride;
                if (o + 12 > vertices.Length)
                    break;
                Vector3 p;
                if (packedPos)
                {
                    var packed = BitConverter.ToUInt32(vertices, o);
                    p = UnpackPosition(packed, scale, offset);
                }
                else
                {
                    p = new Vector3(
                        BitConverter.ToSingle(vertices, o),
                        BitConverter.ToSingle(vertices, o + 4),
                        BitConverter.ToSingle(vertices, o + 8));
                }

                positions[v] = p;
                uvs[v] = ReadUv(vertices, o + uvOffset, packedNorm, entryType);
                boundsMin = Vector3.Min(boundsMin, p);
                boundsMax = Vector3.Max(boundsMax, p);
            }

            var indexCount16 = indices.Length / 2;
            ushort IndexAt(int idx) => BitConverter.ToUInt16(indices, idx * 2);

            void AddTri(int a, int b, int c)
            {
                if ((uint)a >= (uint)positions.Length ||
                    (uint)b >= (uint)positions.Length ||
                    (uint)c >= (uint)positions.Length)
                    return;
                var pa = positions[a];
                var pb = positions[b];
                var pc = positions[c];
                var n = Vector3.Cross(pb - pa, pc - pa);
                if (n.LengthSquared() < 1e-10f)
                {
                    degenerateSkipped++;
                    return;
                }
                n = Vector3.Normalize(n);
                triangles.Add(new MeshTriangle(pa, pb, pc, n, uvs[a], uvs[b], uvs[c], textureId));
            }

            if (blocks.Count == 0)
            {
                var before = triangles.Count;
                for (var t = 0; t + 2 < indexCount16; t += 3)
                    AddTri(IndexAt(t), IndexAt(t + 1), IndexAt(t + 2));
                noBlockFaces += triangles.Count - before;
                continue;
            }

            foreach (var block in blocks)
            {
                var start = (int)block.Start;
                var before = triangles.Count;
                if (block.Strip)
                {
                    var count = (int)block.Count;
                    for (var t = 0; t < count; t++)
                    {
                        var i0 = start + t;
                        if (i0 + 2 >= indexCount16)
                            break;
                        var a = IndexAt(i0);
                        var b = IndexAt(i0 + 1);
                        var c = IndexAt(i0 + 2);
                        if ((t & 1) == 1)
                            AddTri(b, a, c);
                        else
                            AddTri(a, b, c);
                    }
                    stripFaces += triangles.Count - before;
                }
                else
                {
                    var count = (int)block.Count;
                    for (var t = 0; t < count; t++)
                    {
                        var i0 = start + t * 3;
                        if (i0 + 2 >= indexCount16)
                            break;
                        AddTri(IndexAt(i0), IndexAt(i0 + 1), IndexAt(i0 + 2));
                    }
                    listFaces += triangles.Count - before;
                }
            }
            }
            catch (Exception ex)
            {
                throw new InvalidDataException(
                    $"primitive {i}/{primitiveCount} at {cursor}/{data.Length} afterBones={afterBones} verts={vertexCount} idx={indexCount} stride={stride} vBytes={vBytes} iBytes={iBytes}: {ex.Message}", ex);
            }
        }

        if (triangles.Count == 0)
            throw new InvalidDataException(
                $"Mesh contained no triangles (cursor={cursor}/{data.Length}, afterBones={afterBones}, mats={materialCount}, prims={primitiveCount}, bones={boneCount}).");

        return new MeshFile
        {
            Name = name,
            EntryType = entryType,
            Materials = materials,
            Triangles = triangles,
            BoundsMin = boundsMin,
            BoundsMax = boundsMax,
            DeclaredTriangles = declaredTriangles,
            StripFaces = stripFaces,
            ListFaces = listFaces,
            NoBlockFaces = noBlockFaces,
            DegenerateSkipped = degenerateSkipped,
        };
    }

    internal static int PackedUvOffset(int entryType, int stride, uint initFlags, bool hasBones)
    {
        var packedPos = (initFlags & 4) != 0 && (initFlags & 0x10) == 0;
        var packedNorm = (initFlags & 4) != 0;
        if (entryType == 4 || (stride == 36 && !hasBones))
            return 24;
        if (!hasBones && stride == 24 && !packedPos && !packedNorm)
            return 16;
        if (!hasBones && stride == 20 && packedPos && !packedNorm)
            return 12;

        var posSize = packedPos ? 4 : 12;
        var normOff = posSize + (hasBones ? 8 : 0);
        return normOff + (packedNorm ? 4 : 12);
    }

    internal static Vector2 ReadUv(byte[] vertices, int offset, bool packedNorm, int entryType)
    {
        if (offset < 0 || offset + 4 > vertices.Length)
            return Vector2.Zero;
        if (packedNorm && entryType == 4)
        {
            return new Vector2(
                BitConverter.ToUInt16(vertices, offset) / 65535f,
                BitConverter.ToUInt16(vertices, offset + 2) / 65535f);
        }

        if (packedNorm)
        {
            return new Vector2(
                DecompressUv(BitConverter.ToInt16(vertices, offset)),
                DecompressUv(BitConverter.ToInt16(vertices, offset + 2)));
        }

        if (offset + 8 > vertices.Length)
            return Vector2.Zero;
        return new Vector2(
            BitConverter.ToSingle(vertices, offset),
            BitConverter.ToSingle(vertices, offset + 4));
    }

    internal static float DecompressUv(short value) => value / 2048f - 8f;

    private static Vector3 UnpackPosition(uint packed, Vector3 scale, Vector3 offset)
    {
        var ix = (int)(packed & 0x7FF);
        if ((ix & 0x400) != 0) ix |= unchecked((int)0xFFFFF800);
        var iy = (int)((packed >> 11) & 0x7FF);
        if ((iy & 0x400) != 0) iy |= unchecked((int)0xFFFFF800);
        var iz = (int)(packed >> 22);
        if ((iz & 0x200) != 0) iz |= unchecked((int)0xFFFFFC00);
        return new Vector3(
            ix * 0.0009775171f * scale.X + offset.X,
            iy * 0.0009775171f * scale.Y + offset.Y,
            iz * 0.0019569471f * scale.Z + offset.Z);
    }

    private static int EstimateStride(int initFlags, bool animated)
    {
        var posComp = (initFlags & 4) != 0 && (initFlags & 0x10) == 0;
        var normComp = (initFlags & 4) != 0;
        var bump = (initFlags & 2) != 0;
        var stride = posComp ? 4 : 12;
        if (animated) stride += 8;
        stride += normComp ? 4 : 12;
        stride += normComp ? 4 : 8;
        if (bump) stride += normComp ? 8 : 16;
        return stride;
    }

    private static void Need(byte[] data, int cursor, int bytes)
    {
        if (cursor < 0 || cursor + bytes > data.Length)
            throw new InvalidDataException($"Truncated mesh at {cursor}+{bytes} / {data.Length}.");
    }

    private static string ReadCString(byte[] data, ref int cursor)
    {
        var start = cursor;
        while (cursor < data.Length && data[cursor] != 0)
            cursor++;
        var text = System.Text.Encoding.ASCII.GetString(data, start, cursor - start);
        if (cursor < data.Length)
            cursor++;
        return text;
    }

    private static ushort ReadU16(byte[] data, ref int cursor)
    {
        Need(data, cursor, 2);
        var value = BitConverter.ToUInt16(data, cursor);
        cursor += 2;
        return value;
    }

    private static uint ReadU32(byte[] data, ref int cursor)
    {
        Need(data, cursor, 4);
        var value = BitConverter.ToUInt32(data, cursor);
        cursor += 4;
        return value;
    }

    private static int ReadI32(byte[] data, ref int cursor)
    {
        Need(data, cursor, 4);
        var value = BitConverter.ToInt32(data, cursor);
        cursor += 4;
        return value;
    }

    private static float ReadF32(byte[] data, ref int cursor)
    {
        Need(data, cursor, 4);
        var value = BitConverter.ToSingle(data, cursor);
        cursor += 4;
        return value;
    }
}

public readonly record struct MeshMaterial(
    int Id,
    string Name,
    int DiffuseMapId,
    int BumpMapId,
    int DecalId,
    int ReflectionMapId,
    int IlluminationMapId);

public readonly record struct MeshTriangle(
    Vector3 A,
    Vector3 B,
    Vector3 C,
    Vector3 Normal,
    Vector2 UvA = default,
    Vector2 UvB = default,
    Vector2 UvC = default,
    int TextureId = 0,
    Vector3 ColorA = default,
    Vector3 ColorB = default,
    Vector3 ColorC = default,
    int TextureId1 = 0,
    Vector3 NormalA = default,
    Vector3 NormalB = default,
    Vector3 NormalC = default,
    SceneLayer Layer = SceneLayer.Prop);

/// <summary>
/// Geometry bucket. <see cref="Fable.Formats.Scene.ScenePasses"/> maps these
/// onto exe layer bits (landscape 4 / 0x40, sky 0x2000, props 0x20).
/// </summary>
public enum SceneLayer
{
    Sky = 0,
    Landscape = 1,
    Prop = 2,
}
