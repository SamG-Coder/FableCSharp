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
    public int PayloadBytes { get; init; }
    public int LeftoverBytes { get; init; }
    public byte[] LowerMips { get; init; } = [];

    /// <summary>
    /// Device <c>CreateTexture</c> <c>[vtbl+40]</c> at
    /// <c>009BE800</c> / <c>009BE830</c> / <c>009BE870</c> /
    /// <c>009BE8B0</c>. Pushes FourCC <c>DXT1</c>/<c>DXT3</c>,
    /// pool <c>3</c> (<c>D3DPOOL_SCRATCH</c>), usage <c>0</c>,
    /// levels <c>[wrapper+452]</c>, height <c>[+92]</c>, width
    /// <c>[+96]</c>. <c>00416C8A</c> is <c>Init Graphics</c>;
    /// <c>00416D20</c> is a <c>DXT5</c> CString after DXT1
    /// create, not DXT5 CreateTexture. First-seen 512 DXT1
    /// files are framed-LZO top mip plus raw 256..4 leftover.
    /// </summary>
    public const uint CreateTextureDxt1 = 0x009BE8B0;
    public const uint CreateTextureDxt1Probe = 0x009BE800;
    public const uint CreateTextureDxt1Named = 0x009BE830;
    public const uint CreateTextureDxt3 = 0x009BE870;
    public const uint CreateTextureVtbl = 40;
    public const int CreateTexturePoolScratch = 3;
    public const int CreateTextureUsage = 0;
    public const int CreateTextureLevelsOffset = 452;
    public const uint InitGraphics = 0x00416C8A;
    public const uint InitGraphicsDxt5Name = 0x00416D20;
    public const bool FirstSeenCreateTextureUsesDxtFourCc = true;
    public const bool FirstSeenInitGraphicsDxt5NameIsCreateTexture = false;
    public const bool FirstSeenTextureStoresRawLowerMips = true;
    public const int FirstSeenLowerMipStop = 4;

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
        var top = TopMipSize(header.Width, header.Height, compression);
        var payload = data;
        var lower = Array.Empty<byte>();
        var framed = compression is TextureCompression.Dxt1 or TextureCompression.Dxt3
                or TextureCompression.Dxt5
            || (compression is TextureCompression.Rgba8 && data.Length < top);
        if (framed && LooksCompressed(data))
        {
            var cursor = 0;
            payload = Lzo.DecompressFramed(data, ref cursor, top, out var produced);
            if (produced > 0 && produced < payload.Length)
                payload = payload[..produced];
            if (cursor < data.Length)
                lower = data[cursor..];
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
            PayloadBytes = payload.Length,
            LeftoverBytes = lower.Length,
            LowerMips = lower,
        };
    }

    public static int TopMipSize(int width, int height, TextureCompression compression)
    {
        if (compression == TextureCompression.Rgba8)
            return width * height * 4;
        var bw = Math.Max(1, (width + 3) / 4);
        var bh = Math.Max(1, (height + 3) / 4);
        return bw * bh * (compression == TextureCompression.Dxt1 ? 8 : 16);
    }

    public static int RawLowerMipSize(int width, int height, TextureCompression compression)
    {
        var total = 0;
        var w = Math.Max(1, width / 2);
        var h = Math.Max(1, height / 2);
        while (w >= FirstSeenLowerMipStop || h >= FirstSeenLowerMipStop)
        {
            total += TopMipSize(w, h, compression);
            if (w <= FirstSeenLowerMipStop && h <= FirstSeenLowerMipStop)
                break;
            w = Math.Max(1, w / 2);
            h = Math.Max(1, h / 2);
        }

        return total;
    }

    public bool PayloadIsTopMipOnly =>
        PayloadBytes > 0 && PayloadBytes == TopMipSize(Width, Height, Compression) && LeftoverBytes == 0;

    public bool PayloadHasRawLowerMips =>
        LeftoverBytes > 0 && LeftoverBytes == RawLowerMipSize(Width, Height, Compression);

    public bool PayloadIsFullMipChain =>
        PayloadBytes > 0 && PayloadBytes == ExpectedSize(Width, Height, Compression)
        && ExpectedSize(Width, Height, Compression) > TopMipSize(Width, Height, Compression);

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
