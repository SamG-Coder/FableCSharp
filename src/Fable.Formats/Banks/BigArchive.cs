using Fable.Formats.IO;

namespace Fable.Formats.Banks;

/// <summary>
/// Lionhead BIGB archive (graphics.big, textures.big, fonts.big).
/// The header points at a footer of named sub-banks.
/// </summary>
public sealed class BigArchive : IDisposable
{
    public const uint Magic = 0x42474942; // BIGB

    private readonly Stream _stream;
    private readonly bool _ownsStream;

    public uint Version { get; }
    public uint FooterOffset { get; }
    public uint FooterSize { get; }
    public IReadOnlyList<BigSubBank> SubBanks { get; }

    private BigArchive(Stream stream, bool ownsStream, uint version, uint footerOffset, uint footerSize,
        IReadOnlyList<BigSubBank> subBanks)
    {
        _stream = stream;
        _ownsStream = ownsStream;
        Version = version;
        FooterOffset = footerOffset;
        FooterSize = footerSize;
        SubBanks = subBanks;
    }

    public static BigArchive Open(string path) => Open(File.OpenRead(path), ownsStream: true);

    public static BigArchive Open(Stream stream, bool ownsStream = false)
    {
        using var reader = new BinaryReader(stream, System.Text.Encoding.ASCII, leaveOpen: true);
        var magic = reader.ReadUInt32();
        if (magic != Magic)
            throw new InvalidDataException($"Expected BIGB, got 0x{magic:X8}.");

        var version = reader.ReadUInt32();
        var footerOffset = reader.ReadUInt32();
        var footerSize = reader.ReadUInt32();

        stream.Seek(footerOffset, SeekOrigin.Begin);
        var bankCount = reader.ReadUInt32();
        var banks = new List<BigSubBank>((int)Math.Min(bankCount, 10_000));
        for (var i = 0; i < bankCount; i++)
        {
            banks.Add(new BigSubBank
            {
                Name = BinaryText.ReadCString(reader),
                Version = reader.ReadUInt32(),
                EntryCount = reader.ReadUInt32(),
                Offset = reader.ReadUInt32(),
                Size = reader.ReadUInt32(),
                Alignment = reader.ReadUInt32(),
            });
        }

        return new BigArchive(stream, ownsStream, version, footerOffset, footerSize, banks);
    }

    public IReadOnlyList<BankEntry> ReadEntries(BigSubBank bank)
    {
        _stream.Seek(bank.Offset, SeekOrigin.Begin);
        return BankDirectory.ReadEntries(_stream, bank.EntryCount);
    }

    public byte[] Read(BankEntry entry)
    {
        if (entry.Size == 0)
            return [];
        _stream.Seek(entry.Offset, SeekOrigin.Begin);
        var buffer = new byte[entry.Size];
        var read = _stream.Read(buffer);
        if (read != buffer.Length)
            throw new EndOfStreamException($"Expected {entry.Size} bytes for '{entry.Name}', got {read}.");
        return buffer;
    }

    public void Dispose()
    {
        if (_ownsStream)
            _stream.Dispose();
    }
}

public sealed class BigSubBank
{
    public required string Name { get; init; }
    public required uint Version { get; init; }
    public required uint EntryCount { get; init; }
    public required uint Offset { get; init; }
    public required uint Size { get; init; }
    public required uint Alignment { get; init; }
}
