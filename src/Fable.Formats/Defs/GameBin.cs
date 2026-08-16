using System.IO.Compression;

namespace Fable.Formats.Defs;

/// <summary>
/// Compiled Lionhead def binary (game.bin / script.bin / frontend.bin).
/// Header + name-refs into names.bin, then zlib-1 chunks of control-byte defs.
/// Documented by GameBinFormatTests.
/// </summary>
public sealed class GameBin
{
    public const int HeaderSize = 13;
    public const int MaxChunkInflate = 32 * 1024;

    public required bool UseNamesBin { get; init; }
    public required uint FileIndicator { get; init; }
    public required uint PlatformIndicator { get; init; }
    public required IReadOnlyList<GameBinNameRef> NameRefs { get; init; }
    public required IReadOnlyList<GameBinChunk> Chunks { get; init; }
    public required IReadOnlyList<GameBinEntry> Entries { get; init; }

    private Dictionary<string, GameBinEntry>? _byInstance;

    public static GameBin Load(string path, NamesBin names)
    {
        var bytes = File.ReadAllBytes(path);
        return Parse(bytes, names);
    }

    public static GameBin Parse(byte[] bytes, NamesBin names)
    {
        if (bytes.Length < HeaderSize + 8)
            throw new InvalidDataException("game.bin too small.");

        var cursor = 0;
        var useNames = bytes[cursor++] == 1;
        var fileIndicator = ReadU32(bytes, ref cursor);
        var platform = ReadU32(bytes, ref cursor);
        var entryCount = ReadU32(bytes, ref cursor);
        if (entryCount > 200_000)
            throw new InvalidDataException($"Implausible game.bin entry count {entryCount}.");

        var nameRefs = new GameBinNameRef[entryCount];
        for (var i = 0; i < entryCount; i++)
        {
            var defOff = ReadU32(bytes, ref cursor);
            var fileOff = ReadU32(bytes, ref cursor);
            var counter = ReadU32(bytes, ref cursor);
            nameRefs[i] = new GameBinNameRef(
                defOff,
                fileOff,
                counter,
                names.Get(defOff),
                names.Get(fileOff));
        }

        var chunkCount = ReadU32(bytes, ref cursor);
        ReadU32(bytes, ref cursor); // reserved
        if (chunkCount is 0 or > 10_000)
            throw new InvalidDataException($"Implausible chunk count {chunkCount}.");

        var index = new List<(uint Offset, uint Cumulative)>();
        for (var i = 0; i < chunkCount - 1; i++)
            index.Add((ReadU32(bytes, ref cursor), ReadU32(bytes, ref cursor)));

        // Optional sentinel: both fields equal remaining compressed size.
        if (cursor + 8 <= bytes.Length)
        {
            var a = BitConverter.ToUInt32(bytes, cursor);
            var b = BitConverter.ToUInt32(bytes, cursor + 4);
            var remaining = (uint)(bytes.Length - (cursor + 8));
            if (a == b && a == remaining)
                cursor += 8;
        }

        var dataStart = cursor;
        var chunks = new List<GameBinChunk>();
        var entries = new List<GameBinEntry>((int)entryCount);
        uint entryBase = 0;
        for (var i = 0; i < index.Count; i++)
        {
            var start = dataStart + (int)index[i].Offset;
            var end = i + 1 < index.Count
                ? dataStart + (int)index[i + 1].Offset
                : bytes.Length;
            if (start < 0 || end > bytes.Length || start >= end)
                throw new InvalidDataException($"Bad chunk span {start}..{end}.");

            var inflated = InflateZlib(bytes.AsSpan(start, end - start));
            var count = index[i].Cumulative - entryBase;
            var chunkEntries = ParseChunkEntries(inflated, entryBase, count, nameRefs);
            chunks.Add(new GameBinChunk(entryBase, count, inflated.Length, end - start));
            entries.AddRange(chunkEntries);
            entryBase = index[i].Cumulative;
        }

        return new GameBin
        {
            UseNamesBin = useNames,
            FileIndicator = fileIndicator,
            PlatformIndicator = platform,
            NameRefs = nameRefs,
            Chunks = chunks,
            Entries = entries,
        };
    }

    public GameBinEntry? FindEntry(string definitionType)
    {
        _byInstance ??= Entries
            .Where(entry => entry.InstanceName is not null)
            .GroupBy(entry => entry.InstanceName!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        return _byInstance.TryGetValue(definitionType, out var hit) ? hit : null;
    }

    public int? FindMeshId(string definitionType)
    {
        var entry = FindEntry(definitionType);
        if (entry is null || IsEditorOnly(entry))
            return null;
        if (entry.MeshId is > 0)
            return entry.MeshId;

        foreach (var sub in entry.SubDefs)
        {
            if ((uint)sub.DefIndex >= (uint)Entries.Count)
                continue;
            var child = Entries[sub.DefIndex];
            if (child.TypeName == "CReplaceableMeshDef" && child.MeshId is > 0)
                return child.MeshId;
        }

        return null;
    }

    private static bool IsEditorOnly(GameBinEntry entry)
    {
        var name = entry.InstanceName ?? entry.TypeName ?? "";
        return entry.TypeName is "MARKER" ||
               name.StartsWith("MARKER_", StringComparison.OrdinalIgnoreCase) ||
               name.StartsWith("CAMERA_", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("CAMERA_POINT", StringComparison.OrdinalIgnoreCase);
    }

    private static List<GameBinEntry> ParseChunkEntries(
        byte[] data, uint entryBase, uint count, GameBinNameRef[] nameRefs)
    {
        var entries = new List<GameBinEntry>((int)count);
        if (count == 0)
            return entries;

        var cursor = 0;
        var offsets = new int[count];
        for (var i = 0; i < count; i++)
            offsets[i] = ReadU16(data, ref cursor);

        for (var i = 0; i < count; i++)
        {
            var start = offsets[i];
            var end = i + 1 < count ? offsets[i + 1] : data.Length;
            if (start < 0 || end > data.Length || start > end)
                throw new InvalidDataException($"Bad entry span {start}..{end} in chunk @{entryBase}.");

            var global = (int)entryBase + i;
            var nameRef = (uint)global < nameRefs.Length ? nameRefs[global] : default;
            var raw = data.AsSpan(start, end - start).ToArray();
            entries.Add(ParseEntry(raw, global, nameRef));
        }

        return entries;
    }

    private static GameBinEntry ParseEntry(byte[] raw, int index, GameBinNameRef nameRef)
    {
        var isReal = raw.Length > 0 && raw[0] != 0;
        var isTemplate = raw.Length > 1 && raw[1] != 0;
        var unknown = raw.Length > 2 ? raw[2] : (byte)0;
        var cursor = Math.Min(3, raw.Length);
        var subDefs = Array.Empty<GameBinSubDef>();
        if (HasSubDefTable(nameRef.TypeName) && cursor + 2 <= raw.Length)
        {
            var n = BitConverter.ToUInt16(raw, cursor);
            cursor += 2;
            if (n < 400 && cursor + n * 12 <= raw.Length)
            {
                subDefs = new GameBinSubDef[n];
                for (var i = 0; i < n; i++)
                {
                    var crc = BitConverter.ToUInt32(raw, cursor);
                    var defIndex = BitConverter.ToInt32(raw, cursor + 4);
                    var owner = BitConverter.ToInt32(raw, cursor + 8);
                    subDefs[i] = new GameBinSubDef(crc, defIndex, owner);
                    cursor += 12;
                }
            }
        }

        var meshId = ReadMeshId(raw, cursor, nameRef.TypeName);
        return new GameBinEntry
        {
            Index = index,
            IsReal = isReal,
            IsTemplate = isTemplate,
            Unknown0 = unknown,
            TypeName = nameRef.TypeName,
            InstanceName = GuessInstanceName(nameRef),
            SourceName = nameRef.FileName,
            SubDefs = subDefs,
            MeshId = meshId,
            BodyOffset = cursor,
            Raw = raw,
        };
    }

    private static string? GuessInstanceName(GameBinNameRef nameRef)
    {
        if (LooksLikeInstance(nameRef.FileName))
            return nameRef.FileName;
        if (LooksLikeInstance(nameRef.TypeName))
            return nameRef.TypeName;
        return nameRef.FileName ?? nameRef.TypeName;
    }

    private static bool LooksLikeInstance(string? name) =>
        name is not null &&
        (name.StartsWith("OBJECT_", StringComparison.OrdinalIgnoreCase) ||
         name.StartsWith("CREATURE_", StringComparison.OrdinalIgnoreCase) ||
         name.StartsWith("BUILDING_", StringComparison.OrdinalIgnoreCase) ||
         name.StartsWith("MARKER_", StringComparison.OrdinalIgnoreCase) ||
         name.StartsWith("CAMERA_", StringComparison.OrdinalIgnoreCase) ||
         name.Contains('\\') ||
         name.Contains('/'));

    private static int? ReadMeshId(byte[] raw, int bodyOffset, string? typeName)
    {
        if (typeName is "CReplaceableMeshDef")
            return ReadReplaceableMeshId(raw, bodyOffset);
        return ReadGraphicBankIndex(raw, bodyOffset);
    }

    private static int? ReadReplaceableMeshId(byte[] raw, int offset)
    {
        // After optional Meshes control, a u32 count then entries of
        // (i32 bank, f32 anim, f32 size, u8 alpha, u8 type).
        for (var i = offset; i + 20 <= raw.Length; i++)
        {
            var count = BitConverter.ToInt32(raw, i);
            if (count is < 1 or > 16)
                continue;
            var stride = 14;
            if (i + 4 + count * stride > raw.Length)
                continue;
            var bank = BitConverter.ToInt32(raw, i + 4);
            var anim = BitConverter.ToSingle(raw, i + 8);
            var size = BitConverter.ToSingle(raw, i + 12);
            if (bank is > 0 and < 50_000 &&
                anim is >= 0f and <= 16f &&
                size is >= 0f and <= 16f)
                return bank;
        }

        return null;
    }

    private static int? ReadGraphicBankIndex(byte[] raw, int offset)
    {
        var graphicCrc = FableCrc.Hash("Graphic");
        for (var i = offset; i + 8 <= raw.Length; i += 1)
        {
            if (BitConverter.ToUInt32(raw, i) != graphicCrc)
                continue;
            // EngineGraphic after the field id: type i32, bank i32, ...
            if (i + 12 > raw.Length)
                continue;
            var bank = BitConverter.ToInt32(raw, i + 8);
            if (bank is > 0 and < 50_000)
                return bank;
        }

        return null;
    }

    private static bool HasSubDefTable(string? typeName) =>
        typeName is "OBJECT" or "CREATURE" or "BUILDING" or "THING" or "MARKER" or
            "HOLY_SITE" or "VILLAGE" or "SHOT" or "SWITCH" or "NOISE" or
            "PHYSICAL_SWITCH" or "OBJECT_FAMILY" or "REGION";

    private static byte[] InflateZlib(ReadOnlySpan<byte> compressed)
    {
        using var input = new MemoryStream(compressed.ToArray(), writable: false);
        using var zlib = new ZLibStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        zlib.CopyTo(output);
        return output.ToArray();
    }

    private static uint ReadU32(byte[] data, ref int cursor)
    {
        if (cursor + 4 > data.Length)
            throw new InvalidDataException($"Truncated u32 at {cursor}.");
        var value = BitConverter.ToUInt32(data, cursor);
        cursor += 4;
        return value;
    }

    private static int ReadU16(byte[] data, ref int cursor)
    {
        if (cursor + 2 > data.Length)
            throw new InvalidDataException($"Truncated u16 at {cursor}.");
        var value = BitConverter.ToUInt16(data, cursor);
        cursor += 2;
        return value;
    }
}

public readonly record struct GameBinNameRef(
    uint TypeOffset,
    uint FileOffset,
    uint Counter,
    string? TypeName,
    string? FileName);

public readonly record struct GameBinSubDef(uint NameCrc, int DefIndex, int OwnerIndex);

public sealed class GameBinChunk
{
    public uint EntryBase { get; }
    public uint EntryCount { get; }
    public int InflatedSize { get; }
    public int CompressedSize { get; }

    public GameBinChunk(uint entryBase, uint entryCount, int inflatedSize, int compressedSize)
    {
        EntryBase = entryBase;
        EntryCount = entryCount;
        InflatedSize = inflatedSize;
        CompressedSize = compressedSize;
    }
}

public sealed class GameBinEntry
{
    public required int Index { get; init; }
    public required bool IsReal { get; init; }
    public required bool IsTemplate { get; init; }
    public required byte Unknown0 { get; init; }
    public string? TypeName { get; init; }
    public string? InstanceName { get; init; }
    public string? SourceName { get; init; }
    public required IReadOnlyList<GameBinSubDef> SubDefs { get; init; }
    public int? MeshId { get; init; }
    public required int BodyOffset { get; init; }
    public required byte[] Raw { get; init; }
}
