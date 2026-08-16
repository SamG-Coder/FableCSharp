using Fable.Formats.IO;

namespace Fable.Formats.Defs;

/// <summary>
/// Compiled Lionhead name table (names.bin).
/// Header is 20 bytes, then repeating (u32 hash, cstring name) pairs.
/// game.bin name-refs use the string's offset after the 20-byte header
/// (CRC size included: first string is at offset 4).
/// </summary>
public sealed class NamesBin
{
    public const int HeaderSize = 20;

    public uint Unknown0 { get; }
    public uint Signature { get; }
    public uint DeclaredCount { get; }
    public IReadOnlyList<NamedHash> Entries { get; }
    public IReadOnlyDictionary<uint, NamedHash> ByOffset { get; }

    public NamesBin(uint unknown0, uint signature, uint declaredCount, IReadOnlyList<NamedHash> entries)
    {
        Unknown0 = unknown0;
        Signature = signature;
        DeclaredCount = declaredCount;
        Entries = entries;
        ByOffset = entries.ToDictionary(entry => entry.Offset, entry => entry);
    }

    public static NamesBin Load(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        var unknown0 = reader.ReadUInt32();
        var signature = reader.ReadUInt32();
        var declaredCount = reader.ReadUInt32();
        reader.ReadUInt32();
        reader.ReadUInt32();

        var entries = new List<NamedHash>((int)Math.Min(declaredCount, 200_000));
        while (stream.Position < stream.Length)
        {
            if (stream.Length - stream.Position < 5)
                break;

            var crcStart = (int)stream.Position;
            var hash = reader.ReadUInt32();
            var name = BinaryText.ReadCString(reader);
            if (name.Length == 0)
                continue;
            var offset = (uint)(crcStart + 4 - HeaderSize);
            entries.Add(new NamedHash(hash, name, offset));
        }

        return new NamesBin(unknown0, signature, declaredCount, entries);
    }

    public string? Get(uint offset) =>
        ByOffset.TryGetValue(offset, out var entry) ? entry.Name : null;

    public NamedHash? Find(string name)
    {
        foreach (var entry in Entries)
        {
            if (entry.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return entry;
        }

        return null;
    }

    public IEnumerable<NamedHash> Search(string substring) =>
        Entries.Where(entry => entry.Name.Contains(substring, StringComparison.OrdinalIgnoreCase));
}

public readonly record struct NamedHash(uint Hash, string Name, uint Offset = 0);
