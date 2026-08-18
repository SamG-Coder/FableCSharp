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
    public const float HalfPixel = 0.5f;
    public const uint DefaultColor = 0xFFFFFFFFu;
    public const string PressButtonTag = "TEXT_GUI_MENU_PRESS_BUTTON";
    public const string PressButtonBank = "TEXT_ENGLISH_MAIN";

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
        int AtlasY1);

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
        var lineHeight = font.LineHeight * scale;
        var glyphHeight = lineHeight;
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
                ay1));
            penX += glyph.Advance * scale;
        }

        return quads;
    }
}
