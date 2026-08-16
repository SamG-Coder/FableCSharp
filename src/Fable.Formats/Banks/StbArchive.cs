using Fable.Formats.IO;

namespace Fable.Formats.Banks;

/// <summary>
/// Runtime BBBB map bank (FinalAlbion_RT.stb). Same header as WAD, but the
/// last directory entry is truncated; we keep every complete record.
/// </summary>
public sealed class StbArchive : IDisposable
{
    private readonly Stream _stream;
    public IReadOnlyList<BankEntry> Entries { get; }

    private StbArchive(Stream stream, IReadOnlyList<BankEntry> entries)
    {
        _stream = stream;
        Entries = entries;
    }

    public static StbArchive Open(string path)
    {
        var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream, System.Text.Encoding.ASCII, leaveOpen: true);
        if (reader.ReadUInt32() != BbbArchive.Magic)
            throw new InvalidDataException("Not a BBBB bank.");

        stream.Seek(20, SeekOrigin.Begin);
        var count = reader.ReadUInt32();
        stream.Seek(28, SeekOrigin.Begin);
        var footer = reader.ReadUInt32();
        stream.Seek(footer, SeekOrigin.Begin);
        var stats = reader.ReadUInt32();
        if (stats < 100_000)
            stream.Seek(stats * 8L, SeekOrigin.Current);
        else
            stream.Seek(-4, SeekOrigin.Current);

        var entries = new List<BankEntry>();
        try
        {
            for (var i = 0; i < count; i++)
            {
                var magic = reader.ReadUInt32();
                var id = reader.ReadUInt32();
                var type = reader.ReadUInt32();
                var size = reader.ReadUInt32();
                var offset = reader.ReadUInt32();
                var crc = reader.ReadUInt32();
                var name = BinaryText.ReadLengthPrefixed(reader);
                reader.ReadUInt32();
                var deps = reader.ReadUInt32();
                var depNames = new string[Math.Min(deps, 32)];
                for (var d = 0; d < depNames.Length; d++)
                    depNames[d] = BinaryText.ReadLengthPrefixed(reader);
                var info = reader.ReadUInt32();
                if (info is > 0 and < 10_000_000)
                    stream.Seek(info, SeekOrigin.Current);

                if (magic != BbbArchive.EntryMagic && size == 0)
                    continue;

                entries.Add(new BankEntry
                {
                    Id = id,
                    Type = type,
                    Size = size,
                    Offset = offset,
                    Crc = crc,
                    Name = name,
                    Dependencies = depNames,
                });
            }
        }
        catch (EndOfStreamException)
        {
            // Final directory record in TLC's STB is truncated.
        }

        return new StbArchive(stream, entries);
    }

    public BankEntry? FindLev(string region)
    {
        var needle = region + ".lev";
        foreach (var entry in Entries)
        {
            var file = Path.GetFileName(entry.Name);
            if (file.Equals(needle, StringComparison.OrdinalIgnoreCase) &&
                !file.Contains("Filler", StringComparison.OrdinalIgnoreCase) &&
                !file.Contains("Demon", StringComparison.OrdinalIgnoreCase))
                return entry;
        }

        return null;
    }

    public byte[] Read(BankEntry entry)
    {
        _stream.Seek(entry.Offset, SeekOrigin.Begin);
        var buffer = new byte[entry.Size];
        var read = _stream.Read(buffer);
        if (read != buffer.Length)
            throw new EndOfStreamException(entry.Name);
        return buffer;
    }

    public void Dispose() => _stream.Dispose();
}
