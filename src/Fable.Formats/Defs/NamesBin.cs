using Fable.Formats.IO;

namespace Fable.Formats.Defs;

/// <summary>
/// Compiled Lionhead name table (names.bin).
/// Header is 20 bytes, then repeating (u32 hash, cstring name) pairs.
/// </summary>
public sealed class NamesBin
{
    public uint Unknown0 { get; }
    public uint Signature { get; }
    public uint DeclaredCount { get; }
    public IReadOnlyList<NamedHash> Entries { get; }

    public NamesBin(uint unknown0, uint signature, uint declaredCount, IReadOnlyList<NamedHash> entries)
    {
        Unknown0 = unknown0;
        Signature = signature;
        DeclaredCount = declaredCount;
        Entries = entries;
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

            var hash = reader.ReadUInt32();
            var name = BinaryText.ReadCString(reader);
            if (name.Length == 0)
                continue;
            entries.Add(new NamedHash(hash, name));
        }

        return new NamesBin(unknown0, signature, declaredCount, entries);
    }

    public IEnumerable<NamedHash> Search(string substring) =>
        Entries.Where(entry => entry.Name.Contains(substring, StringComparison.OrdinalIgnoreCase));
}

public readonly record struct NamedHash(uint Hash, string Name);
