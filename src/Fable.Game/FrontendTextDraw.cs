using Fable.Formats.Fonts;

namespace Fable.Game;

/// <summary>
/// Native-faithful glyph quads from MAIN face draw
/// <c>00AB7C20</c> (font vtbl+20). Type-6 widget
/// <c>0054EF00</c> packs colour/text into type
/// <c>0x27</c> records via <c>00543910</c>; it does
/// not CPU-blit and is not type <c>0x22</c>.
/// One glyph = one quad = 2 <c>D3DPT_TRIANGLELIST</c>
/// tris, 6 × 28-byte XYZRHW+DIFFUSE+TEX1 verts.
/// </summary>
public static class FrontendTextDraw
{
    public const uint GlyphDrawFn = FontFile.GlyphDrawFn;
    public const uint Type6CtorFn = 0x0054F5C0;
    public const uint Type6DrawFn = 0x0054EF00;
    public const uint Type6FontLookupFn = 0x0054ED90;
    public const uint Type6RecordFn = 0x00543910;
    public const uint UiFaceHelperFn = FontFile.UiFaceHelperFn;
    public const uint DrawPrimitiveSubmitFn = 0x00A0ABE0;
    public const int Type6RecordType = 0x27;
    public const int Type6RecordSize = 64;
    public const int VertexStride = 28;
    public const int VerticesPerGlyph = 6;
    public const int TrianglesPerGlyph = 2;
    public const int D3dPrimitiveTriangleList = 4;
    public const int Type6PassCount = 2;
    public const bool Type6UsesDiffuseColour = true;
    public const float HalfPixel = 0.5f;
    public const uint HalfPixelVa = 0x0122F59C;
    public const uint OneVa = 0x0122DED8;
    public const uint Type6PadVa = 0x0122DCDC;
    public const float Type6OriginPad = 2f;
    public const uint Type6AlignFn = 0x0054FFF0;
    public const int AlignLeft = 0;
    public const int AlignCentre = 1;
    public const int AlignRight = 2;
    public const int Flag302CentreBit = 0x10;
    public const int Flag302RightBit = 0x20;
    /// <summary>
    /// Retail type-6 frontend text extent. The text setter
    /// <c>0054FBC0</c> computes <c>+204/+208</c>; its fit path
    /// <c>0054F8E0</c> lays text into the frontend rectangle.
    /// </summary>
    public const float FrontendLineWidth = 640f;
    public const int WidgetColourOffset = 148;
    public const uint DefaultColor = 0xFFFFFFFFu;
    public const string PressButtonTag = "TEXT_GUI_MENU_PRESS_BUTTON";
    public const string PressButtonBank = "TEXT_ENGLISH_MAIN";
    public const string PressButtonText = "Press Left Mouse Button To Continue";

    public readonly record struct GlyphQuad(
        char Character,
        float DestX0,
        float DestY0,
        float DestX1,
        float DestY1,
        float U0,
        float V0,
        float U1,
        float V1,
        uint Color,
        int AtlasX0,
        int AtlasY0,
        int AtlasX1,
        int AtlasY1,
        int GlyphIndex,
        int Width,
        int Advance);

    public readonly record struct GlyphVertex(
        float X,
        float Y,
        float U,
        float V);

    /// <summary>
    /// <c>0054EF00</c> builds two type-0x27 records. The first receives
    /// zero RGB and the widget alpha at stack bytes +36..+39; the second
    /// receives the widget colour at +32..+35.
    /// </summary>
    public static uint BlackUnderlayColor(uint widgetArgb) =>
        widgetArgb & 0xFF000000u;

    /// <summary>
    /// <c>0054FFF0</c>: bit4 → centre (1), else
    /// <c>(flag302 >> 4) &amp; 2</c> (bit5 → right).
    /// First-seen <c>+302</c> bits 4/5 stay 0 → left.
    /// </summary>
    public static int AlignFromFlag302(byte flag302)
    {
        if ((flag302 & Flag302CentreBit) != 0)
            return AlignCentre;
        return (flag302 >> 4) & AlignRight;
    }

    /// <summary>
    /// <c>0054EF00</c> leftover <c>+204</c> × scale.
    /// Centre also × <c>[0x122F59C]=0.5</c>, then
    /// <c>fsubr</c> from origin X.
    /// </summary>
    public static float Type6AlignedX(
        float originX, float leftover204, float scale, int align)
    {
        if (align == AlignCentre)
            return originX - leftover204 * scale * HalfPixel;
        if (align == AlignRight)
            return originX - leftover204 * scale;
        return originX;
    }

    /// <summary>
    /// Offset type-6 pen before <c>00AB7C20</c>: aligned X/Y plus
    /// <c>[0x122DCDC]=2</c>. The default constructor state writes
    /// <c>widget+393=0</c> at <c>0054F636</c>; in that branch the unshifted
    /// record receives the widget colour and the offset record receives black.
    /// </summary>
    public static (float X, float Y) Type6Pen(
        float originX, float originY, float leftover204, float scale, int align) =>
        (Type6AlignedX(originX, leftover204, scale, align) + Type6OriginPad,
            originY + Type6OriginPad);

    /// <summary>
    /// <c>00AB7C20</c> 6-vert list. U0/V0 is dest
    /// top-left (X0,Y0); U1/V1 is dest bottom-right.
    /// Order: BL, TL, BR, TR, BR, TL.
    /// </summary>
    public static GlyphVertex[] NativeVerts(in GlyphQuad q) =>
    [
        new(q.DestX0, q.DestY1, q.U0, q.V1),
        new(q.DestX0, q.DestY0, q.U0, q.V0),
        new(q.DestX1, q.DestY1, q.U1, q.V1),
        new(q.DestX1, q.DestY0, q.U1, q.V0),
        new(q.DestX1, q.DestY1, q.U1, q.V1),
        new(q.DestX0, q.DestY0, q.U0, q.V0),
    ];

    /// <summary>
    /// <c>00AB7C20</c> layout. Pen += bearing, dest
    /// width = <c>WidthMinus1+1</c>, dest height =
    /// <c>CellHeight+1</c>, then pen += tail.
    /// Newline advances Y by <c>CellHeight</c> (no +1).
    /// GPU UV = stored × (atlas-1)/atlas.
    /// Dest is shifted by the D3D9 half-pixel.
    /// </summary>
    public static List<GlyphQuad> Layout(
        FontFile font,
        string text,
        float x,
        float y,
        uint color = DefaultColor,
        float scale = 1f)
    {
        var quads = new List<GlyphQuad>(text.Length);
        var penX = x;
        var penY = y;
        var glyphHeight = font.LineHeight * scale;
        foreach (var ch in text)
        {
            if (ch == '\n')
            {
                penX = x;
                penY += font.CellHeight * scale;
                continue;
            }

            if (font.GlyphAt(ch) is not { } glyph)
                continue;

            var destX0 = penX + glyph.BearingX * scale - HalfPixel;
            var destY0 = penY - HalfPixel;
            var destX1 = destX0 + glyph.Width * scale;
            var destY1 = destY0 + glyphHeight;
            var (ax0, ay0, ax1, ay1) = font.AtlasRect(glyph);
            quads.Add(new GlyphQuad(
                ch,
                destX0,
                destY0,
                destX1,
                destY1,
                font.GpuU(glyph.U0),
                font.GpuV(glyph.V0),
                font.GpuU(glyph.U1),
                font.GpuV(glyph.V1),
                color,
                ax0,
                ay0,
                ax1,
                ay1,
                ch,
                glyph.Width,
                glyph.Advance));
            penX += glyph.Advance * scale;
        }

        return quads;
    }

    /// <summary>
    /// Type-6 formatted text: emulate the <c>0054FBC0</c> setter's
    /// measured <c>+204/+208</c> result and <c>0054F8E0</c> fit,
    /// then apply <c>0054FFF0</c> alignment to each line. The
    /// widget destination is an anchor point, not the top-left
    /// corner of the whole string.
    /// </summary>
    public static List<GlyphQuad> LayoutFormatted(
        FontFile font,
        string text,
        float anchorX,
        float y,
        int align,
        uint color = DefaultColor,
        float maxLineWidth = FrontendLineWidth,
        float scale = 1f)
    {
        var lines = WrapLines(font, text, maxLineWidth, scale);
        var quads = new List<GlyphQuad>(text.Length);
        for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            var line = lines[lineIndex];
            var width = font.MeasureWidth(line) * scale;
            var x = align switch
            {
                AlignCentre => anchorX - width * HalfPixel,
                AlignRight => anchorX - width,
                _ => anchorX,
            };
            quads.AddRange(Layout(
                font,
                line,
                x,
                y + lineIndex * font.CellHeight * scale,
                color,
                scale));
        }

        return quads;
    }

    public static IReadOnlyList<string> WrapLines(
        FontFile font,
        string text,
        float maxLineWidth = FrontendLineWidth,
        float scale = 1f)
    {
        if (string.IsNullOrEmpty(text))
            return [""];

        var lines = new List<string>();
        foreach (var paragraph in text.Replace("\r\n", "\n", StringComparison.Ordinal)
                     .Replace('\r', '\n').Split('\n'))
        {
            var remaining = paragraph.AsSpan();
            if (remaining.Length == 0)
            {
                lines.Add("");
                continue;
            }

            while (remaining.Length > 0)
            {
                if (font.MeasureWidth(remaining.ToString()) * scale <= maxLineWidth)
                {
                    lines.Add(remaining.Trim().ToString());
                    break;
                }

                var lastBreak = -1;
                for (var i = 0; i < remaining.Length; i++)
                {
                    if (!char.IsWhiteSpace(remaining[i]))
                        continue;
                    var candidate = remaining[..i].TrimEnd();
                    if (candidate.Length > 0 &&
                        font.MeasureWidth(candidate.ToString()) * scale <= maxLineWidth)
                        lastBreak = i;
                    else if (candidate.Length > 0)
                        break;
                }

                if (lastBreak < 0)
                {
                    var split = 1;
                    while (split < remaining.Length &&
                           font.MeasureWidth(remaining[..(split + 1)].ToString()) * scale <= maxLineWidth)
                        split++;
                    lines.Add(remaining[..split].ToString());
                    remaining = remaining[split..].TrimStart();
                }
                else
                {
                    lines.Add(remaining[..lastBreak].TrimEnd().ToString());
                    remaining = remaining[(lastBreak + 1)..].TrimStart();
                }
            }
        }

        return lines;
    }
}
