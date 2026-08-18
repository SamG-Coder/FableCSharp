using System.Buffers.Binary;
using System.Text;

namespace Fable.Formats.Fonts;

/// <summary>
/// <c>FONT_ENGLISH_MAIN</c> face. Load
/// <c>009CC240</c>/<c>009CC2A0</c>, ctor
/// <c>00AB8E10</c>, glyph <c>00AB96B0</c>,
/// lookup <c>00AB7A10</c>, measure
/// <c>00AB7B00</c>, draw <c>00AB7C20</c>.
/// File glyph is 22 bytes; in-memory stride 24.
/// </summary>
public sealed class FontFile
{
    public const uint InitFontsFn = 0x004168DC;
    public const uint FontLookupFn = 0x009E2C80;
    public const uint MainFaceCtorFn = 0x00AB8E10;
    public const uint GlyphReadFn = 0x00AB96B0;
    public const uint GlyphLookupFn = 0x00AB7A10;
    public const uint GlyphMeasureFn = 0x00AB7B00;
    public const uint GlyphDrawFn = 0x00AB7C20;
    public const int GlyphFileBytes = 22;
    public const int GlyphStride = 24;
    public const int PageCount = 1024;
    public const int AtlasHeaderBytes = 18;
    public const int AtlasChannelBits = 8;
    public const string MainBank = "FONT_ENGLISH_MAIN";
    public const string StreamingBank = "STREAMING_FONT_ENGLISH_PC";
    public const string UiFace = "ENG_ARIAL_16";
    /// <summary>
    /// Persist Font 26051 on type-6
    /// <c>UI_PRESS_START_TEXT</c> via
    /// <c>009D49B0</c> names.bin offset.
    /// Not the <c>0054F4B0</c> helper.
    /// </summary>
    public const string PersistType6Face = "ENG_ARIAL_24";
    public const string GameFace = "ENG_ARIAL_18";
    public const uint UiFaceHelperFn = 0x0054F4B0;

    public required string Name { get; init; }
    public required string Family { get; init; }
    /// <summary><c>[face+4]</c> from the first u32. Dest height is +1.</summary>
    public required int CellHeight { get; init; }
    /// <summary><c>[face+20]</c>. MAIN Arial faces store 400 or 700.</summary>
    public required int Weight { get; init; }
    /// <summary><c>[face+24]</c> from the u8 after weight.</summary>
    public required bool Flag { get; init; }
    /// <summary><c>[face+16]</c>. Equals <see cref="CellHeight"/> on MAIN faces; unused by <c>00AB7C20</c>.</summary>
    public required int MetricHeight { get; init; }
    public required int UvWidth { get; init; }
    public required int UvHeight { get; init; }
    public required int MinChar { get; init; }
    public required int MaxChar { get; init; }
    public required byte[] Atlas { get; init; }
    public required IReadOnlyList<FontPage> Pages { get; init; }
    public int AtlasHeaderWidth { get; init; }
    public int AtlasHeaderHeight { get; init; }
    public ushort AtlasHeaderFormat { get; init; }
    public int AtlasPayloadBytes { get; init; }
    public bool AtlasIsRgba { get; init; }
    /// <summary><c>00AB7B00</c> measure line height is <c>[face+4]+1</c>. Glyph dest height in <c>00AB7C20</c> is the same.</summary>
    public int LineHeight => CellHeight + 1;
    public int AtlasWidth => UvWidth;
    public int AtlasHeight => UvHeight;
    /// <summary>CreateTexture pitch is <c>shl width, 2</c> at <c>00AB960A</c>.</summary>
    public int AtlasPitch => UvWidth * 4;

    public static FontFile Parse(string name, byte[] data)
    {
        var o = 0;
        var family = ReadCString(data, ref o);
        var cell = (int)ReadU32(data, ref o);
        var weight = (int)ReadU32(data, ref o);
        var flag = data[o++] != 0;
        var metric = (int)ReadU32(data, ref o);
        var uvW = (int)ReadU32(data, ref o);
        var uvH = (int)ReadU32(data, ref o);
        var minCh = (int)ReadU32(data, ref o);
        var maxCh = (int)ReadU32(data, ref o);
        var buckets = ReadU32(data, ref o);
        var pages = new FontPage[PageCount];
        for (uint b = 0; b < buckets; b++)
        {
            var page = ReadU32(data, ref o);
            var first = (ushort)ReadU32(data, ref o);
            var count = (ushort)ReadU32(data, ref o);
            if (page >= PageCount)
                throw new InvalidDataException($"{name} page {page}");
            var glyphs = new FontGlyph[count];
            for (var i = 0; i < count; i++)
                glyphs[i] = ReadGlyph(data, ref o);
            pages[page] = new FontPage(first, glyphs);
        }

        var atlas = Array.Empty<byte>();
        var headerW = uvW;
        var headerH = uvH;
        ushort headerFmt = 0;
        var payloadBytes = 0;
        var isRgba = false;
        if (o + 4 <= data.Length)
        {
            var n = (int)ReadU32(data, ref o);
            if (n >= AtlasHeaderBytes && o + n <= data.Length)
            {
                headerW = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(o + 12));
                headerH = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(o + 14));
                headerFmt = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(o + 16));
                o += AtlasHeaderBytes;
                payloadBytes = n - AtlasHeaderBytes;
                (atlas, isRgba) = DecodeAtlas(data.AsSpan(o, payloadBytes), uvW, uvH);
            }
        }

        return new FontFile
        {
            Name = name,
            Family = family,
            CellHeight = cell,
            Weight = weight,
            Flag = flag,
            MetricHeight = metric,
            UvWidth = uvW,
            UvHeight = uvH,
            MinChar = minCh,
            MaxChar = maxCh,
            Atlas = atlas,
            Pages = pages,
            AtlasHeaderWidth = headerW,
            AtlasHeaderHeight = headerH,
            AtlasHeaderFormat = headerFmt,
            AtlasPayloadBytes = payloadBytes,
            AtlasIsRgba = isRgba,
        };
    }

    public FontGlyph? Glyph(char ch) => GlyphAt(ch);

    /// <summary><c>00AB7A10</c> page slot: <c>ch >> 6</c>.</summary>
    public static int PageIndex(int ch) => ch >> 6;

    /// <summary><c>00AB7A10</c> in-page index before subtracting first.</summary>
    public static int SlotIndex(int ch) => ch & 63;

    /// <summary>
    /// <c>00AB7A10</c> / <c>00AB7C20</c>: page =
    /// <c>ch >> 6</c>, index = <c>(ch &amp; 63) - first</c>,
    /// then stride 24.
    /// </summary>
    public FontGlyph? GlyphAt(int ch)
    {
        if (ch < MinChar || ch > MaxChar)
            return null;
        var page = Pages[PageIndex(ch)];
        if (page.Glyphs.Count == 0)
            return null;
        var i = SlotIndex(ch) - page.First;
        if ((uint)i >= (uint)page.Glyphs.Count)
            return null;
        return page.Glyphs[i];
    }

    /// <summary>
    /// <c>00AB7B00</c> out+0 / out+4. Width is the
    /// max line of <c>BearingX + AdvanceTail</c>.
    /// Height is <c>CellHeight+1</c> per line;
    /// empty string is 0×0.
    /// </summary>
    public (int Width, int Height) Measure(string text)
    {
        var width = 0;
        var line = 0;
        var lines = 0;
        var any = false;
        foreach (var ch in text)
        {
            any = true;
            if (ch == '\n')
            {
                if (line > width)
                    width = line;
                line = 0;
                lines++;
                continue;
            }

            if (GlyphAt(ch) is { } glyph)
                line += glyph.Advance;
        }

        if (line > width)
            width = line;
        return any ? (width, (lines + 1) * LineHeight) : (0, 0);
    }

    public int MeasureWidth(string text) => Measure(text).Width;

    /// <summary>
    /// File UV is <c>pixel / (atlas-1)</c>.
    /// <c>00AB7C20</c> GPU u is <c>U*(w-1)/w</c> = <c>pixel/w</c>.
    /// </summary>
    public int AtlasX(float storedU) => (int)MathF.Round(storedU * (UvWidth - 1));

    public int AtlasY(float storedV) => (int)MathF.Round(storedV * (UvHeight - 1));

    public float GpuU(float storedU) => storedU * (UvWidth - 1) / UvWidth;

    public float GpuV(float storedV) => storedV * (UvHeight - 1) / UvHeight;

    public (int X0, int Y0, int X1, int Y1) AtlasRect(in FontGlyph glyph) =>
        (AtlasX(glyph.U0), AtlasY(glyph.V0), AtlasX(glyph.U1), AtlasY(glyph.V1));

    public void Blit(byte[] rgba, int destW, int destH, int x, int y, string text)
    {
        var penX = x;
        var penY = y;
        foreach (var ch in text)
        {
            if (ch == '\n')
            {
                penX = x;
                penY += CellHeight;
                continue;
            }

            if (GlyphAt(ch) is not { } glyph)
                continue;
            BlitGlyph(rgba, destW, destH, penX + glyph.BearingX, penY, glyph);
            penX += glyph.Advance;
        }
    }

    private void BlitGlyph(
        byte[] rgba, int destW, int destH, int dx, int dy, FontGlyph glyph)
    {
        var (sx0, sy0, sx1, sy1) = AtlasRect(glyph);
        var gw = glyph.Width;
        var gh = LineHeight;
        if (gw <= 0 || gh <= 0 || Atlas.Length == 0)
            return;
        var srcW = Math.Max(1, sx1 - sx0);
        var srcH = Math.Max(1, sy1 - sy0);
        for (var row = 0; row < gh; row++)
        {
            var ty = dy + row;
            if ((uint)ty >= (uint)destH)
                continue;
            var sy = sy0 + row * srcH / gh;
            if ((uint)sy >= (uint)UvHeight)
                continue;
            var srcRow = sy * UvWidth * 4;
            var dstRow = ty * destW * 4;
            for (var col = 0; col < gw; col++)
            {
                var tx = dx + col;
                if ((uint)tx >= (uint)destW)
                    continue;
                var sx = sx0 + col * srcW / gw;
                if ((uint)sx >= (uint)UvWidth)
                    continue;
                var s = srcRow + sx * 4;
                var a = Atlas[s + 3];
                if (a == 0)
                    continue;
                var d = dstRow + tx * 4;
                if (a == 255)
                {
                    rgba[d] = Atlas[s];
                    rgba[d + 1] = Atlas[s + 1];
                    rgba[d + 2] = Atlas[s + 2];
                    rgba[d + 3] = 255;
                    continue;
                }

                var ia = 255 - a;
                rgba[d] = (byte)((Atlas[s] * a + rgba[d] * ia) / 255);
                rgba[d + 1] = (byte)((Atlas[s + 1] * a + rgba[d + 1] * ia) / 255);
                rgba[d + 2] = (byte)((Atlas[s + 2] * a + rgba[d + 2] * ia) / 255);
                rgba[d + 3] = 255;
            }
        }
    }

    private static (byte[] Rgba, bool IsRgba) DecodeAtlas(
        ReadOnlySpan<byte> payload, int width, int height)
    {
        var pitch = width * 4;
        var need = pitch * height;
        var rgba = new byte[need];
        if (payload.Length >= need)
        {
            payload[..need].CopyTo(rgba);
            return (rgba, true);
        }

        if (payload.Length == width * height)
        {
            for (var i = 0; i < payload.Length; i++)
            {
                var v = payload[i];
                var d = i * 4;
                rgba[d] = 255;
                rgba[d + 1] = 255;
                rgba[d + 2] = 255;
                rgba[d + 3] = v;
            }
        }

        return (rgba, false);
    }

    /// <summary>
    /// <c>00AB96B0</c> reads four floats then three
    /// int16s (2 bytes consumed, dword-stored at +16/+18/+20).
    /// </summary>
    private static FontGlyph ReadGlyph(byte[] data, ref int o)
    {
        var u0 = BitConverter.ToSingle(data, o); o += 4;
        var v0 = BitConverter.ToSingle(data, o); o += 4;
        var u1 = BitConverter.ToSingle(data, o); o += 4;
        var v1 = BitConverter.ToSingle(data, o); o += 4;
        var bearing = BinaryPrimitives.ReadInt16LittleEndian(data.AsSpan(o)); o += 2;
        var widthM1 = BinaryPrimitives.ReadInt16LittleEndian(data.AsSpan(o)); o += 2;
        var tail = BinaryPrimitives.ReadInt16LittleEndian(data.AsSpan(o)); o += 2;
        return new FontGlyph(u0, v0, u1, v1, bearing, widthM1, tail);
    }

    private static uint ReadU32(byte[] data, ref int o)
    {
        var value = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(o));
        o += 4;
        return value;
    }

    private static string ReadCString(byte[] data, ref int o)
    {
        var start = o;
        while (o < data.Length && data[o] != 0)
            o++;
        var text = Encoding.ASCII.GetString(data, start, o - start);
        if (o < data.Length)
            o++;
        return text;
    }
}

public readonly record struct FontPage(
    ushort First,
    IReadOnlyList<FontGlyph> Glyphs);

/// <summary>
/// 22 file bytes from <c>00AB96B0</c>. Height is not
/// in the record; dest height is <c>CellHeight+1</c>.
/// </summary>
public readonly record struct FontGlyph(
    float U0,
    float V0,
    float U1,
    float V1,
    short BearingX,
    short WidthMinus1,
    short AdvanceTail)
{
    public int Width => WidthMinus1 + 1;
    /// <summary><c>00AB7B00</c> / <c>00AB7C20</c> add +16 then +20 to the pen.</summary>
    public int Advance => BearingX + AdvanceTail;
    public int X => 0;
    public int Y => 0;
    public int Height => 0;
}
