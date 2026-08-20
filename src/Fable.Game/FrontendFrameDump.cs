using System.Globalization;
using System.Text;
using Fable.Formats.Defs;
using Fable.Formats.Fonts;
using Fable.Render;

namespace Fable.Game;

/// <summary>
/// Engine-state Press Start frame table.
/// Rows come from constructed widgets plus
/// <see cref="FrontendSubmitBatch"/> UVs.
/// Not a screenshot decode.
/// </summary>
public readonly record struct FrontendFrameDumpRow(
    string Name,
    int Type,
    string? Parent,
    bool Visible,
    string? Texture,
    int GraphicId,
    float AuthoredX,
    float AuthoredY,
    float AuthoredW,
    float AuthoredH,
    float DestX0,
    float DestY0,
    float DestX1,
    float DestY1,
    float U0,
    float V0,
    float U1,
    float V1,
    uint Colour,
    string? Text,
    string? TextValue,
    int GlyphCount,
    int DrawOrder,
    bool Offscreen,
    bool ZeroSize,
    bool ChildDestMissesParent,
    bool InvalidUv,
    bool InvalidGlyphAdvance)
{
    public string FlagList
    {
        get
        {
            var flags = new List<string>(5);
            if (Offscreen)
                flags.Add("offscreen");
            if (ZeroSize)
                flags.Add("zero-size");
            if (ChildDestMissesParent)
                flags.Add("child-dest-misses-parent");
            if (InvalidUv)
                flags.Add("invalid-uv");
            if (InvalidGlyphAdvance)
                flags.Add("invalid-glyph-advance");
            return flags.Count == 0 ? "-" : string.Join(",", flags);
        }
    }
}

public static class FrontendFrameDump
{
    public const int ViewportWidth = EngineLifecycle.DisplayDefaultWidth;
    public const int ViewportHeight = EngineLifecycle.DisplayDefaultHeight;

    /// <summary>
    /// Persist Colour* is 0..1. All-zero
    /// with no persist <c>ColourA</c> is
    /// unread / ctor default; native
    /// <c>005339B0</c> then writes
    /// <c>+144..+147=0xFF</c>. An
    /// authored <c>ColourA=0</c> stays 0
    /// so <c>0041AFA0</c> <c>+151</c>
    /// skips the DIP.
    /// </summary>
    public static uint PackPersistColour(float r, float g, float b, float a) =>
        PackPersistColour(r, g, b, a, haveColourA: false);

    public static uint PackPersistColour(
        float r, float g, float b, float a, bool haveColourA)
    {
        if (!haveColourA && r == 0f && g == 0f && b == 0f && a == 0f)
            return 0xFFFFFFFFu;
        return ((uint)ToByte(a) << 24)
            | ((uint)ToByte(r) << 16)
            | ((uint)ToByte(g) << 8)
            | ToByte(b);
    }

    public static FrontendFrameDumpRow Row(
        FrontendWidget widget,
        FrontendWidget? parent,
        FrontendSubmitBatch? batch,
        int batchDrawStart,
        int submittedDraws,
        FontFile? face)
    {
        var (u0, v0, u1, v1, colour, batchUvBad) = ReadBatchUv(
            batch, batchDrawStart, submittedDraws, widget);
        var zero = widget.DestX1 <= widget.DestX0 || widget.DestY1 <= widget.DestY0;
        var offscreen = !Intersects(
            widget.DestX0, widget.DestY0, widget.DestX1, widget.DestY1,
            0f, 0f, ViewportWidth, ViewportHeight);
        var missParent = false;
        if (parent is { } p && p.DestX1 > p.DestX0 && p.DestY1 > p.DestY0)
        {
            missParent = !Intersects(
                widget.DestX0, widget.DestY0, widget.DestX1, widget.DestY1,
                p.DestX0, p.DestY0, p.DestX1, p.DestY1);
        }

        var invalidUv = batchUvBad
            || !UvOk(u0) || !UvOk(v0) || !UvOk(u1) || !UvOk(v1);
        var invalidAdvance = InvalidGlyphAdvance(widget, face);
        var glyphs = widget.GlyphCount;
        if (glyphs == 0 && widget.Type == FrontendWidgetType.Text)
            glyphs = CountGlyphs(widget.Text, face);
        return new FrontendFrameDumpRow(
            widget.Name,
            widget.Type,
            widget.ParentName,
            widget.Visible,
            widget.TextureName,
            widget.GraphicId,
            widget.PersistX,
            widget.PersistY,
            widget.PersistWidth,
            widget.PersistHeight,
            widget.DestX0,
            widget.DestY0,
            widget.DestX1,
            widget.DestY1,
            u0, v0, u1, v1,
            colour,
            widget.Text,
            widget.TextValue,
            glyphs,
            widget.DrawOrder,
            offscreen,
            zero,
            missParent,
            invalidUv,
            invalidAdvance);
    }

    public static int SubmittedDraws(FrontendWidget widget)
    {
        if (!widget.Visible)
            return 0;
        var n = 0;
        if (widget.DestX1 > widget.DestX0 && widget.DestY1 > widget.DestY0 &&
            !string.IsNullOrEmpty(widget.TextureName))
            n++;
        if (widget.Type == FrontendWidgetType.Text && !string.IsNullOrEmpty(widget.Text))
            n += Math.Max(widget.GlyphCount, 0);
        return n;
    }

    public static string Format(IReadOnlyList<FrontendFrameDumpRow> rows, int batchDraws)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Engine-state Press Start frame (not a screenshot).");
        sb.AppendLine("# After Bootstrap + skip AVI + one frontend Pump.");
        sb.AppendLine($"# viewport={ViewportWidth}x{ViewportHeight} widgets={rows.Count} batchDraws={batchDraws}");
        sb.AppendLine("#");
        sb.AppendLine(
            "# name type parent visible texture GraphicId " +
            "X Y W H destX0 destY0 destX1 destY1 " +
            "U0 V0 U1 V1 colour text textTag glyphs order flags");
        foreach (var row in rows)
        {
            sb.Append(Escape(row.Name));
            sb.Append(' ').Append(row.Type);
            sb.Append(' ').Append(Escape(row.Parent));
            sb.Append(' ').Append(row.Visible ? 1 : 0);
            sb.Append(' ').Append(Escape(row.Texture));
            sb.Append(' ').Append(row.GraphicId);
            sb.Append(' ').Append(F(row.AuthoredX));
            sb.Append(' ').Append(F(row.AuthoredY));
            sb.Append(' ').Append(F(row.AuthoredW));
            sb.Append(' ').Append(F(row.AuthoredH));
            sb.Append(' ').Append(F(row.DestX0));
            sb.Append(' ').Append(F(row.DestY0));
            sb.Append(' ').Append(F(row.DestX1));
            sb.Append(' ').Append(F(row.DestY1));
            sb.Append(' ').Append(F(row.U0));
            sb.Append(' ').Append(F(row.V0));
            sb.Append(' ').Append(F(row.U1));
            sb.Append(' ').Append(F(row.V1));
            sb.Append(' ').Append(row.Colour.ToString("X8", CultureInfo.InvariantCulture));
            sb.Append(' ').Append(Escape(row.Text));
            sb.Append(' ').Append(Escape(row.TextValue));
            sb.Append(' ').Append(row.GlyphCount);
            sb.Append(' ').Append(row.DrawOrder);
            sb.Append(' ').Append(row.FlagList);
            sb.AppendLine();
        }

        var off = 0;
        var zero = 0;
        var miss = 0;
        var uv = 0;
        var adv = 0;
        foreach (var row in rows)
        {
            if (row.Offscreen)
                off++;
            if (row.ZeroSize)
                zero++;
            if (row.ChildDestMissesParent)
                miss++;
            if (row.InvalidUv)
                uv++;
            if (row.InvalidGlyphAdvance)
                adv++;
        }

        sb.AppendLine("#");
        sb.AppendLine(
            $"# flags offscreen={off} zero-size={zero} " +
            $"child-dest-misses-parent={miss} invalid-uv={uv} " +
            $"invalid-glyph-advance={adv}");
        return sb.ToString();
    }

    public static string FormatNewProfile(IReadOnlyList<FrontendWidget> widgets)
    {
        ArgumentNullException.ThrowIfNull(widgets);
        var sb = new StringBuilder();
        sb.AppendLine("# New Profile widget dump");
        sb.AppendLine("# name type parent state selected authoredX authoredY authoredW authoredH drawX0 drawY0 drawX1 drawY1 hitX0 hitY0 hitX1 hitY1 textOriginX textOriginY texture layer alpha visible enabled message");
        for (var i = 0; i < widgets.Count; i++)
        {
            var w = widgets[i];
            var alpha = (int)(w.Colour >> 24);
            sb.Append(Escape(w.Name));
            sb.Append(' ').Append(w.Type);
            sb.Append(' ').Append(Escape(w.ParentName));
            sb.Append(' ').Append(w.State);
            sb.Append(' ').Append(w.ActiveChild);
            sb.Append(' ').Append(F(w.PersistX));
            sb.Append(' ').Append(F(w.PersistY));
            sb.Append(' ').Append(F(w.PersistWidth));
            sb.Append(' ').Append(F(w.PersistHeight));
            sb.Append(' ').Append(F(w.DestX0));
            sb.Append(' ').Append(F(w.DestY0));
            sb.Append(' ').Append(F(w.DestX1));
            sb.Append(' ').Append(F(w.DestY1));
            sb.Append(' ').Append(F(w.HitX0));
            sb.Append(' ').Append(F(w.HitY0));
            sb.Append(' ').Append(F(w.HitX1));
            sb.Append(' ').Append(F(w.HitY1));
            sb.Append(' ').Append(F(w.TextOriginX));
            sb.Append(' ').Append(F(w.TextOriginY));
            sb.Append(' ').Append(Escape(w.TextureName));
            sb.Append(' ').Append(w.Layer);
            sb.Append(' ').Append(alpha);
            sb.Append(' ').Append(w.Visible ? 1 : 0);
            sb.Append(' ').Append(w.Enabled ? 1 : 0);
            sb.Append(' ').Append(w.ActionOnLeftUnclicked);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static void Write(string path, IReadOnlyList<FrontendFrameDumpRow> rows, int batchDraws)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, Format(rows, batchDraws));
    }

    public static bool Intersects(
        float x0, float y0, float x1, float y1,
        float ox0, float oy0, float ox1, float oy1) =>
        x0 < ox1 && x1 > ox0 && y0 < oy1 && y1 > oy0;

    internal static (float U0, float V0, float U1, float V1, uint Colour, bool InvalidUv)
        ReadBatchUv(
            FrontendSubmitBatch? batch,
            int start,
            int count,
            FrontendWidget widget)
    {
        if (batch is { Draws.Length: > 0 } b &&
            count > 0 &&
            start >= 0 &&
            start + count <= b.Draws.Length)
        {
            var minU = float.PositiveInfinity;
            var minV = float.PositiveInfinity;
            var maxU = float.NegativeInfinity;
            var maxV = float.NegativeInfinity;
            var invalid = false;
            var have = false;
            for (var i = 0; i < count; i++)
            {
                var draw = b.Draws[start + i];
                var last = draw.FirstVertex + draw.VertexCount;
                if (last > (uint)b.Vertices.Length)
                {
                    invalid = true;
                    continue;
                }

                for (var v = draw.FirstVertex; v < last; v++)
                {
                    var uv = b.Vertices[v].Uv;
                    have = true;
                    if (uv.X < minU)
                        minU = uv.X;
                    if (uv.Y < minV)
                        minV = uv.Y;
                    if (uv.X > maxU)
                        maxU = uv.X;
                    if (uv.Y > maxV)
                        maxV = uv.Y;
                    if (!UvOk(uv.X) || !UvOk(uv.Y))
                        invalid = true;
                }
            }

            if (have)
                return (minU, minV, maxU, maxV, widget.Colour, invalid);
        }

        return (widget.U0, widget.V0, widget.U1, widget.V1, widget.Colour, false);
    }

    internal static bool InvalidGlyphAdvance(FrontendWidget widget, FontFile? face)
    {
        if (face is null || string.IsNullOrEmpty(widget.Text))
            return false;
        foreach (var ch in widget.Text)
        {
            if (ch is ' ' or '\t' or '\n' or '\r')
                continue;
            if (face.GlyphAt(ch) is { } glyph && glyph.Advance <= 0)
                return true;
        }

        return false;
    }

    internal static int CountGlyphs(string? text, FontFile? face)
    {
        if (string.IsNullOrEmpty(text) || face is null)
            return 0;
        var n = 0;
        foreach (var ch in text)
        {
            if (ch == '\n')
                continue;
            if (face.GlyphAt(ch) is not null)
                n++;
        }

        return n;
    }

    private static bool UvOk(float value) => value is >= 0f and <= 1f;

    private static byte ToByte(float value)
    {
        var scaled = value <= 1f ? value * 255f : value;
        return (byte)Math.Clamp((int)MathF.Round(scaled), 0, 255);
    }

    private static string F(float value) =>
        value.ToString("0.###", CultureInfo.InvariantCulture);

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "-";
        if (value.IndexOfAny([' ', '\t', '\r', '\n']) < 0)
            return value;
        return '"' + value.Replace("\"", "''", StringComparison.Ordinal) + '"';
    }
}
