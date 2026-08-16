using Fable.Formats.IO;

namespace Fable.Formats.Textures;

public enum TextureCompression
{
    Unknown,
    Rgba8,
    Dxt1,
    Dxt3,
    Dxt5,
}

public sealed class TextureFile
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required int FormatCode { get; init; }
    public required TextureCompression Compression { get; init; }
    public required byte[] Rgba { get; init; }

    public static TextureHeader ReadHeader(ReadOnlySpan<byte> info)
    {
        if (info.Length < 14)
            throw new InvalidDataException("Texture info header is 34 bytes; got " + info.Length);
        return new TextureHeader(
            BitConverter.ToUInt16(info),
            BitConverter.ToUInt16(info.Slice(2)),
            BitConverter.ToUInt16(info.Slice(6)),
            BitConverter.ToUInt16(info.Slice(8)),
            BitConverter.ToUInt16(info.Slice(12)));
    }

    public static TextureFile Parse(uint id, string name, uint type, IReadOnlyList<byte> info, byte[] data)
    {
        var header = ReadHeader(info.ToArray());
        var compression = Classify(type, header.FormatCode, header.Width, header.Height, data.Length);
        var expected = ExpectedSize(header.Width, header.Height, compression);
        var payload = data;
        if (compression is TextureCompression.Dxt1 or TextureCompression.Dxt3 or TextureCompression.Dxt5 && LooksCompressed(data))
        {
            var cursor = 0;
            payload = Lzo.DecompressFramed(data, ref cursor, expected);
        }

        var rgba = compression switch
        {
            TextureCompression.Rgba8 => payload.Length >= header.Width * header.Height * 4
                ? payload[..(header.Width * header.Height * 4)]
                : new byte[header.Width * header.Height * 4],
            TextureCompression.Dxt1 => Dxt.Decode(payload, header.Width, header.Height, DxtKind.Dxt1),
            TextureCompression.Dxt3 => Dxt.Decode(payload, header.Width, header.Height, DxtKind.Dxt3),
            TextureCompression.Dxt5 => Dxt.Decode(payload, header.Width, header.Height, DxtKind.Dxt5),
            _ => new byte[header.Width * header.Height * 4],
        };

        return new TextureFile
        {
            Id = (int)id,
            Name = name,
            Width = header.Width,
            Height = header.Height,
            FormatCode = header.FormatCode,
            Compression = compression,
            Rgba = rgba,
        };
    }

    public static TextureCompression Classify(uint type, int formatCode, int width, int height, int packedSize)
    {
        if (type == 4 || formatCode == 1)
            return TextureCompression.Rgba8;
        if (formatCode == 31)
            return TextureCompression.Dxt1;
        if (formatCode == 35)
            return TextureCompression.Dxt5;
        if (formatCode == 32)
            return TextureCompression.Dxt5;
        var raw = width * height * 4;
        return packedSize >= raw ? TextureCompression.Rgba8 : TextureCompression.Dxt1;
    }

    public static int ExpectedSize(int width, int height, TextureCompression compression) =>
        compression switch
        {
            TextureCompression.Rgba8 => width * height * 4,
            TextureCompression.Dxt1 => Dxt.MipChainSize(width, height, 8),
            TextureCompression.Dxt3 => Dxt.MipChainSize(width, height, 16),
            TextureCompression.Dxt5 => Dxt.MipChainSize(width, height, 16),
            _ => width * height * 4,
        };

    private static bool LooksCompressed(byte[] data) =>
        data.Length >= 2 && (data[0] != 0 || data[1] != 0);
}

public readonly record struct TextureHeader(
    int Width,
    int Height,
    int FrameWidth,
    int FrameHeight,
    int FormatCode);
