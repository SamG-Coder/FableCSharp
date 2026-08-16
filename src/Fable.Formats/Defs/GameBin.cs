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

    /// <summary>
    /// First Graphic plus every <c>CMultiStaticMeshDef</c> bank.
    /// HerosOldHouse Graphic is 6909 (exterior); the multi list is
    /// 6911 (interior walls) then 6909. 3184 is a material name on
    /// both C3Ds and has no primitives.
    /// </summary>
    public IReadOnlyList<int> FindMeshIds(string definitionType)
    {
        var ids = new List<int>();
        var seen = new HashSet<int>();
        void Add(int? id)
        {
            if (id is > 0 && seen.Add(id.Value))
                ids.Add(id.Value);
        }

        var entry = FindEntry(definitionType);
        if (entry is null || IsEditorOnly(entry))
            return ids;

        Add(entry.MeshId);
        foreach (var sub in entry.SubDefs)
        {
            if ((uint)sub.DefIndex >= (uint)Entries.Count)
                continue;
            var child = Entries[sub.DefIndex];
            if (child.TypeName == "CReplaceableMeshDef")
                Add(child.MeshId);
            if (child.TypeName != MultiStaticMeshDefType)
                continue;
            foreach (var mesh in ReadMultiStaticMeshEntries(child.Raw))
            {
                if (FirstSeenMultiStaticSkipDraw(mesh.FlagA, (byte)mesh.Tail))
                    continue;
                Add(mesh.MeshId);
            }
        }

        return ids;
    }

    public const string MultiStaticMeshDefType = "CMultiStaticMeshDef";
    public const uint MultiStaticMeshesFieldCrc = 0x0CDCCB01;
    public const uint MultiStaticMeshFieldCrc = 0x60194A74;
    public const uint MultiStaticFlagAFieldCrc = 0x7CA90715;
    public const uint MultiStaticFlagBFieldCrc = 0x97595FC1;
    public const uint MultiStaticValueFieldCrc = 0x15DC93E9;
    public const uint MultiStaticTailFieldCrc = 0x2DF5F1FA;
    public const int MultiStaticMeshHeaderBytes = 11;
    public const int MultiStaticMeshEntryBytes = 34;
    public const int MultiStaticMeshIdOffset = 4;
    public const int MultiStaticFlagAOffset = 12;
    public const int MultiStaticFlagBOffset = 17;
    public const int MultiStaticValueOffset = 22;
    public const int MultiStaticTailOffset = 30;
    public const int HerosOldHouseExteriorMeshId = 6909;
    public const int HerosOldHouseInteriorMeshId = 6911;
    public const int HerosOldHouseInteriorWallTexture = 3172;
    public const int HerosOldHouseFloorTexture = 3184;
    public const bool FirstSeenHouseFloor3184HasPrims = false;
    public const uint MultiStaticLookup = 0x007E1400;
    public const uint MultiStaticName = 0x007E12F0;
    public const uint MultiStaticCtor = 0x007E14C0;
    public const string MultiStaticComponentType = "CTCGraphicAppearanceMultipleStaticMeshes";
    public const uint MultiStaticComponentName = 0x007E1A80;
    public const int MultiStaticComponentBytes = 0x5C;
    public const uint MultiStaticIndex = 0x007E1370;
    public const uint MultiStaticApply = 0x007E15C0;
    public const uint MultiStaticApplyVtbl = 0x0126FFB4;
    public const uint MultiStaticCountMagic = 0x92492493;
    public const int MultiStaticRuntimeStrideBytes = 56;
    public const int MultiStaticRuntimeIdOffset = 40;
    public const int MultiStaticRuntimeFlagAOffset = 44;
    public const int MultiStaticRuntimeFlagBOffset = 45;
    public const int MultiStaticRuntimeOverrideOffset = 48;
    public const int MultiStaticRuntimeSkipByteOffset = 52;
    public const uint MultiStaticSkipGlobal = 0x013756F0;
    /// <summary>
    /// <c>0x13756F0</c> is a <c>.data</c> dword whose file
    /// value is <c>-1</c>. <c>imm</c> finds only two readers
    /// (<c>007E1788</c>, <c>0077BAC5</c>) and no writer.
    /// </summary>
    public const int FirstSeenSkipGlobal = -1;
    public const bool FirstSeenSkipGlobalHasWriter = false;
    public const uint MultiStaticDefFactory = 0x004E31FA;
    public const uint MultiStaticDefPersistCtor = 0x004E1516;
    public const uint MultiStaticDefVtbl = 0x0124265C;
    public const uint MultiStaticVectorPersistSlot = 0x004EDE1B;
    public const uint MultiStaticVectorPersist = 0x004EDE2B;
    public const uint MultiStaticVectorResize = 0x004EDF0A;
    public const uint MultiStaticEntryVtbl = 0x012438A4;
    public const uint MultiStaticEntryPersist = 0x004EB8C3;
    public const uint MultiStaticEntryAssign = 0x004EB831;
    public const uint MultiStaticPersistDword = 0x00431102;
    public const uint MultiStaticPersistU8 = 0x0043314A;
    public const uint MultiStaticPersistFloat = 0x00431061;
    public const uint MultiStaticPersistTail = 0x004735D6;
    public const int MultiStaticEntryPersistVtblSlot = 18;
    public const int MultiStaticThingSkipOffset = 64;
    public const uint ThingBuildingFactory = 0x0052AC10;
    public const uint ThingBuildingBaseCtor = 0x005296B0;
    public const uint ThingTypeRegistrar = 0x00522A20;
    public const uint CreateBuildingScript = 0x0072DF50;
    public const uint ThingParentCtor = 0x004C9030;
    public const int ThingBuildingAllocBytes = 0xD8;
    /// <summary>
    /// <c>004C9030</c> <c>lea eax,[esi+32]</c> then nine
    /// dwords through <c>[eax+32]</c> so <c>CThing+64</c> is
    /// ctor-zero. First-seen skip selected for the
    /// <c>+44==0</c> path is therefore 0.
    /// </summary>
    public const int FirstSeenThingPlus64 = 0;
    public const bool FirstSeenThingPlus64IsZero = true;
    /// <summary>
    /// <c>007E15C0</c> skip is
    /// <c>[0x13756F0] &gt;= 0 &amp;&amp; selected != 0</c>.
    /// <c>selected</c> is the low byte of runtime <c>+52</c>
    /// when <c>+44 != 0</c>, else <c>[thing+64]</c>. File
    /// persist <c>004EB8C3</c> maps FlagA/FlagB/Value/Tail
    /// onto <c>+44/+45/+48/+52</c>. First-seen skip-global
    /// is the <c>.data</c> dword <c>-1</c>, so skip stays
    /// off and both house meshes instance.
    /// </summary>
    public const bool FirstSeenHouseSkipDropsInterior = false;
    public const bool FirstSeenHouseSkipDropsExterior = false;
    public const bool FirstSeenMultiStaticAppliesBothHouseMeshes = true;
    /// <summary>
    /// StartOakValeWest TNG OBJECT/BUILDING/CREATURE/GENERIC
    /// within 25 m of HerosOldHouse all have Graphic /
    /// CMultiStaticMeshDef / CReplaceableMeshDef bank ids.
    /// Apply vtbl <c>0x126FFB4</c> persist slots are
    /// <c>ret</c> stubs. File fields persist through
    /// <c>CMultiStaticMeshDef</c> vtbl <c>0x124265C[18]</c>
    /// <c>004EDE1B</c> → vector <c>004EDE2B</c> → each
    /// 56-byte <c>004EB8C3</c>. First-seen skip-global
    /// <c>-1</c> leaves both house meshes on.
    /// </summary>
    public const bool FirstSeenHouseAreaDefsResolveGraphic = true;
    public const bool FirstSeenMultiStaticPersistMapsFileFields = true;
    public const uint MultiStaticEntryRtti = 0x0137B530;
    /// <summary>
    /// <c>007E17AB</c> if runtime <c>+45 != 0</c> copies
    /// <c>+48</c> over <c>[esp+36]</c>, which was
    /// <c>004BC180</c>'s leftover float
    /// (<c>fild [0x1375710]</c> or <c>fld [obj+72]</c>).
    /// <c>[esp+36]</c> is not read again at that offset in
    /// <c>007E15C0</c>. House interior file value 40 is
    /// therefore not a proven mesh scale
    /// (<c>FirstSeenMultiStaticValueIsScale=false</c>).
    /// Persist <c>004EB8C3</c> writes that float at <c>+48</c>
    /// via <c>00431061</c>.
    /// </summary>
    public const uint MultiStaticDefaultFloat = 0x004BC180;
    public const bool FirstSeenMultiStaticValueIsScale = false;
    public const string BuyableHouseDefType = "CBuyableHouseDef";
    public const uint BuyableHouseDefLookup = 0x006C1B00;
    public const uint BuyableHouseCtor = 0x006BF8A0;
    public const uint BuyableHouseConstruct = 0x006C14D0;
    public const uint BuyableHouseReadyCheck = 0x006BFB90;
    public const uint BuyableHouseWindowSwap = 0x006C0F00;
    public const uint InsideBuildingPredicate = 0x0082E0E0;
    public const int InsideBuildingFlagOffset = 56;
    public const uint InsideBuildingFlagBit = 0x200000;
    /// <summary>
    /// <c>0082E0E0</c> needs <c>[thing+56] &amp; 0x200000</c>.
    /// First-seen parent ctor zeros that dword, so the
    /// predicate returns 0, <c>006BFB90</c> returns 0, and
    /// <c>006C14D0</c> skips <c>006C0F00</c>. Indoor/outdoor
    /// window swap is not first-seen.
    /// </summary>
    public const bool FirstSeenInsideBuildingFlag = false;
    public const bool FirstSeenBuyableHouseSwapsWindows = false;
    public const uint BuyableHousePriceFieldCrc = 0xCD2BFAC0;

    /// <summary>
    /// <c>Meshes</c> CRC then u32 count then 34-byte
    /// <c>CMultiStaticMeshEntryDef</c> records:
    /// <c>Mesh</c> CRC + bank i32 + CRC <c>0x7CA90715</c> + u8
    /// + CRC <c>0x97595FC1</c> + u8 + CRC <c>0x15DC93E9</c> +
    /// f32 + CRC <c>0x2DF5F1FA</c> + u32. HerosOldHouse interior
    /// 6911 is <c>0,1,40</c>; Graphic/exterior 6909 is
    /// <c>1,0,0</c>.
    /// </summary>
    public static IReadOnlyList<int> ReadMultiStaticMeshIds(byte[] raw)
    {
        var ids = new List<int>();
        foreach (var entry in ReadMultiStaticMeshEntries(raw))
        {
            if (entry.MeshId is > 0 and < 50_000)
                ids.Add(entry.MeshId);
        }

        return ids;
    }

    public static IReadOnlyList<MultiStaticMeshEntry> ReadMultiStaticMeshEntries(byte[] raw)
    {
        if (raw.Length < MultiStaticMeshHeaderBytes)
            return [];
        if (BitConverter.ToUInt32(raw, 3) != MultiStaticMeshesFieldCrc)
            return [];
        var count = BitConverter.ToInt32(raw, 7);
        if (count is < 0 or > 16)
            return [];
        var entries = new List<MultiStaticMeshEntry>(count);
        var o = MultiStaticMeshHeaderBytes;
        for (var i = 0; i < count && o + MultiStaticMeshEntryBytes <= raw.Length; i++)
        {
            if (BitConverter.ToUInt32(raw, o) != MultiStaticMeshFieldCrc)
                break;
            entries.Add(new MultiStaticMeshEntry(
                BitConverter.ToInt32(raw, o + MultiStaticMeshIdOffset),
                raw[o + MultiStaticFlagAOffset],
                raw[o + MultiStaticFlagBOffset],
                BitConverter.ToSingle(raw, o + MultiStaticValueOffset),
                BitConverter.ToUInt32(raw, o + MultiStaticTailOffset)));
            o += MultiStaticMeshEntryBytes;
        }

        return entries;
    }

    /// <summary>
    /// <c>007E15C0</c> at <c>007E1788</c>: skip the mesh when
    /// <c>[0x13756F0] &gt;= 0</c> and the selected byte is
    /// non-zero. Selected is runtime <c>+52</c> when
    /// <c>+44 != 0</c>, else <c>[thing+64]</c>.
    /// </summary>
    public static bool MultiStaticSkipDraw(byte runtimeFlagA, byte runtimeSkipByte, int thingPlus64, int skipGlobal)
    {
        if (skipGlobal < 0)
            return false;
        var selected = runtimeFlagA != 0 ? runtimeSkipByte : (byte)thingPlus64;
        return selected != 0;
    }

    public static bool FirstSeenMultiStaticSkipDraw(byte runtimeFlagA, byte runtimeSkipByte) =>
        MultiStaticSkipDraw(runtimeFlagA, runtimeSkipByte, FirstSeenThingPlus64, FirstSeenSkipGlobal);

    public static IReadOnlyList<int> ReadBuyableHousePrices(byte[] raw)
    {
        var crc = BuyableHousePriceFieldCrc;
        for (var i = 0; i + 8 <= raw.Length; i++)
        {
            if (BitConverter.ToUInt32(raw, i) != crc)
                continue;
            var count = BitConverter.ToInt32(raw, i + 4);
            if (count is < 1 or > 8 || i + 8 + count * 4 > raw.Length)
                continue;
            var prices = new int[count];
            for (var n = 0; n < count; n++)
                prices[n] = BitConverter.ToInt32(raw, i + 8 + n * 4);
            if (prices[0] is > 0 and < 1_000_000)
                return prices;
        }

        return [];
    }

    public const uint ParticleEmitterCreate = 0x006E0880;
    public const uint TrackNodeTypeName = 0x004C76A5;

    /// <summary>
    /// First-seen C3D instance is Graphic / CMultiStatic /
    /// CReplaceable only. Markers, cameras, track nodes, and
    /// particle emitters have no first-seen C3D. New Game's
    /// 276 "missing" were those gizmos, not house props.
    /// </summary>
    public const bool FirstSeenMissingMeshesAreGizmos = true;

    private static bool IsEditorOnly(GameBinEntry entry) =>
        !FirstSeenInstancesAsC3d(entry.TypeName, entry.InstanceName);

    public static bool FirstSeenInstancesAsC3d(string? typeName, string? instanceName)
    {
        var name = instanceName ?? typeName ?? "";
        if (typeName is "MARKER")
            return false;
        if (name.StartsWith("MARKER_", StringComparison.OrdinalIgnoreCase))
            return false;
        if (name.StartsWith("CAMERA_", StringComparison.OrdinalIgnoreCase))
            return false;
        if (name.Contains("CAMERA_POINT", StringComparison.OrdinalIgnoreCase))
            return false;
        if (name.StartsWith("TRACK_NODE", StringComparison.OrdinalIgnoreCase))
            return false;
        if (name.StartsWith("PARTICLE_EMITTER", StringComparison.OrdinalIgnoreCase))
            return false;
        return true;
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

/// <summary>
/// One 34-byte <c>CMultiStaticMeshEntryDef</c> from game.bin.
/// Persist <c>004EB8C3</c> writes Mesh/FlagA/FlagB/Value/Tail
/// onto runtime <c>+40/+44/+45/+48/+52</c>.
/// </summary>
public readonly record struct MultiStaticMeshEntry(
    int MeshId,
    byte FlagA,
    byte FlagB,
    float Value,
    uint Tail);

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
