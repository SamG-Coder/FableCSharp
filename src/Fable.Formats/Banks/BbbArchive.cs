using Fable.Formats.IO;

namespace Fable.Formats.Banks;

/// <summary>
/// Lionhead BBBB bank used by FinalAlbion.wad and extracted .bbb files.
/// Footer layout matches the community WAD unpacker: stats table, then entries.
/// </summary>
public sealed class BbbArchive : IDisposable
{
    public const uint Magic = 0x42424242; // BBBB
    public const uint EntryMagic = 42;

    private readonly Stream _stream;
    private readonly bool _ownsStream;

    public uint Version { get; }
    public uint Alignment { get; }
    public uint FooterOffset { get; }
    public IReadOnlyList<BankEntry> Entries { get; }

    private BbbArchive(Stream stream, bool ownsStream, uint version, uint alignment, uint footerOffset,
        IReadOnlyList<BankEntry> entries)
    {
        _stream = stream;
        _ownsStream = ownsStream;
        Version = version;
        Alignment = alignment;
        FooterOffset = footerOffset;
        Entries = entries;
    }

    public static BbbArchive Open(string path) =>
        Open(File.OpenRead(path), ownsStream: true);

    public static BbbArchive Open(Stream stream, bool ownsStream = false)
    {
        using var reader = new BinaryReader(stream, System.Text.Encoding.ASCII, leaveOpen: true);
        var magic = reader.ReadUInt32();
        if (magic != Magic)
            throw new InvalidDataException($"Expected BBBB, got 0x{magic:X8}.");

        var version = reader.ReadUInt32();
        reader.ReadUInt32();
        reader.ReadUInt32();
        var alignment = reader.ReadUInt32();
        var entryCount = reader.ReadUInt32();
        reader.ReadUInt32();
        var footerOffset = reader.ReadUInt32();

        stream.Seek(footerOffset, SeekOrigin.Begin);
        var statsCount = reader.ReadUInt32();
        if (statsCount < 100_000)
            stream.Seek(statsCount * 8L, SeekOrigin.Current);
        else
            stream.Seek(-4, SeekOrigin.Current);

        var entries = new List<BankEntry>((int)Math.Min(entryCount, 100_000));
        for (var i = 0; i < entryCount; i++)
        {
            var entryMagic = reader.ReadUInt32();
            var id = reader.ReadUInt32();
            var type = reader.ReadUInt32();
            var size = reader.ReadUInt32();
            var offset = reader.ReadUInt32();
            var crc = reader.ReadUInt32();
            var name = BinaryText.ReadLengthPrefixed(reader);
            reader.ReadUInt32(); // timestamp
            var depCount = reader.ReadUInt32();
            var deps = new string[depCount];
            for (var d = 0; d < depCount; d++)
                deps[d] = BinaryText.ReadLengthPrefixed(reader);

            var infoSize = reader.ReadUInt32();
            if (infoSize > 0)
                stream.Seek(infoSize, SeekOrigin.Current);

            if (entryMagic != EntryMagic && size == 0)
                continue;

            entries.Add(new BankEntry
            {
                Id = id,
                Type = type,
                Size = size,
                Offset = offset,
                Crc = crc,
                Name = name,
                Dependencies = deps,
            });
        }

        return new BbbArchive(stream, ownsStream, version, alignment, footerOffset, entries);
    }

    public BankEntry? Find(string nameOrStem)
    {
        var needle = nameOrStem.Replace('/', '\\');
        var needleName = Path.GetFileName(needle);
        var needleExt = Path.GetExtension(needle);

        BankEntry? stemMatch = null;
        foreach (var entry in Entries)
        {
            var name = entry.Name.Replace('/', '\\');
            var fileName = Path.GetFileName(name);

            if (name.Equals(needle, StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals(needleName, StringComparison.OrdinalIgnoreCase))
                return entry;

            if (stemMatch is null &&
                Path.GetFileNameWithoutExtension(name)
                    .Equals(Path.GetFileNameWithoutExtension(needle), StringComparison.OrdinalIgnoreCase) &&
                (needleExt.Length == 0 ||
                 Path.GetExtension(name).Equals(needleExt, StringComparison.OrdinalIgnoreCase)))
            {
                stemMatch = entry;
            }
        }

        return stemMatch;
    }

    public byte[] Read(BankEntry entry)
    {
        if (entry.Size == 0)
            return [];

        _stream.Seek(entry.Offset, SeekOrigin.Begin);
        var buffer = new byte[entry.Size];
        var read = _stream.Read(buffer, 0, buffer.Length);
        if (read != buffer.Length)
            throw new EndOfStreamException($"Expected {entry.Size} bytes for '{entry.Name}', got {read}.");
        return buffer;
    }

    public byte[] ReadPrefix(BankEntry entry, int bytes)
    {
        if (entry.Size == 0 || bytes <= 0)
            return [];
        var n = (int)Math.Min((uint)bytes, entry.Size);
        _stream.Seek(entry.Offset, SeekOrigin.Begin);
        var buffer = new byte[n];
        var read = _stream.Read(buffer, 0, n);
        if (read != n)
            throw new EndOfStreamException($"Expected {n} header bytes for '{entry.Name}', got {read}.");
        return buffer;
    }

    public void Dispose()
    {
        if (_ownsStream)
            _stream.Dispose();
    }
}

public sealed class BankEntry
{
    public required uint Id { get; init; }
    public required uint Type { get; init; }
    public required uint Size { get; init; }
    public required uint Offset { get; init; }
    public required uint Crc { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyList<string> Dependencies { get; init; }
    public IReadOnlyList<byte> Info { get; init; } = [];
}
