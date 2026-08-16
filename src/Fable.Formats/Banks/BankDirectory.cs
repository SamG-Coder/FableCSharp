using Fable.Formats.IO;

namespace Fable.Formats.Banks;

internal static class BankDirectory
{
    public const uint EntryMagic = 42;

    public static List<BankEntry> ReadEntries(Stream stream, uint entryCount)
    {
        using var reader = new BinaryReader(stream, System.Text.Encoding.ASCII, leaveOpen: true);
        var statsCount = reader.ReadUInt32();
        if (statsCount < 1000)
            stream.Seek(statsCount * 8L, SeekOrigin.Current);
        else
            stream.Seek(-4, SeekOrigin.Current);

        var entries = new List<BankEntry>((int)Math.Min(entryCount, 200_000));
        for (var i = 0; i < entryCount; i++)
        {
            var magic = reader.ReadUInt32();
            var id = reader.ReadUInt32();
            var type = reader.ReadUInt32();
            var size = reader.ReadUInt32();
            var offset = reader.ReadUInt32();
            var crc = reader.ReadUInt32();
            var name = BinaryText.ReadLengthPrefixed(reader);
            reader.ReadUInt32();
            var depCount = reader.ReadUInt32();
            var deps = new string[depCount];
            for (var d = 0; d < depCount; d++)
                deps[d] = BinaryText.ReadLengthPrefixed(reader);

            var infoSize = reader.ReadUInt32();
            var info = infoSize > 0 ? reader.ReadBytes((int)infoSize) : [];

            if (magic != EntryMagic && size == 0)
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
                Info = info,
            });
        }

        return entries;
    }
}
