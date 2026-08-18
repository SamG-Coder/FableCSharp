using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Fonts;
using Fable.Formats.Text;
using Fable.Game;

namespace Fable.Formats.Tests;

public sealed class FontFileTests
{
    private static (BigArchive Big, BankEntry Entry) OpenMain(string name)
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var big = BigArchive.Open(install.FontsBigPath);
        var bank = big.SubBanks.Single(item => item.Name == FontFile.MainBank);
        var entry = big.ReadEntries(bank).Single(item => item.Name == name);
        return (big, entry);
    }

    [Fact]
    public void ENG_ARIAL_16_is_00AB8E10_pages_and_per_char_width()
    {
        var (big, entry) = OpenMain(FontFile.UiFace);
        using (big)
        {
            var font = FontFile.Parse(entry.Name, big.Read(entry));
            Assert.Equal("Arial", font.Family);
            Assert.Equal(21, font.CellHeight);
            Assert.Equal(128, font.UvWidth);
            Assert.Equal(256, font.UvHeight);
            Assert.Equal(0x00AB8E10u, FontFile.MainFaceCtorFn);
            Assert.Equal(0x00AB96B0u, FontFile.GlyphReadFn);
            Assert.Equal(0x00AB7A10u, FontFile.GlyphLookupFn);
            Assert.True(font.Pages[0].Glyphs.Count >= 32);
            Assert.Equal(32, font.Pages[0].First);
            Assert.NotNull(font.Glyph(' '));
            Assert.NotNull(font.Glyph('A'));
            var i = font.Glyph('i');
            var w = font.Glyph('W');
            Assert.NotNull(i);
            Assert.NotNull(w);
            Assert.True(i.Value.Width < w.Value.Width,
                $"i={i.Value.Width} W={w.Value.Width}");
            ExportDir.WriteRgbaBmp(
                ExportDir.PathFor("fonts", "ENG_ARIAL_16.bmp"),
                font.UvWidth, font.UvHeight, font.Atlas);
            var bang = font.Glyph('!');
            Assert.NotNull(bang);
            Assert.True(bang.Value.Width > 0);
            var sample = "ABCDEFGHIJKLMNOPQRSTUVWXYZ 0123456789";
            var sw = font.MeasureWidth(sample) + 8;
            var sh = font.CellHeight + 8;
            var rgba = new byte[sw * sh * 4];
            font.Blit(rgba, sw, sh, 4, 4, sample);
            ExportDir.WriteRgbaBmp(ExportDir.PathFor("fonts", "ENG_ARIAL_16_sample.bmp"), sw, sh, rgba);
        }
    }

    [Fact]
    public void ENG_ARIAL_24_header_is_256x256_uv()
    {
        var (big, entry) = OpenMain("ENG_ARIAL_24");
        using (big)
        {
            var font = FontFile.Parse(entry.Name, big.Read(entry));
            Assert.Equal(32, font.CellHeight);
            Assert.Equal(256, font.UvWidth);
            Assert.Equal(256, font.UvHeight);
            ExportDir.WriteRgbaBmp(
                ExportDir.PathFor("fonts", "ENG_ARIAL_24.bmp"),
                font.UvWidth, font.UvHeight, font.Atlas);
        }
    }

    [Fact]
    public void ENG_ARIAL_16_blits_press_start_label()
    {
        var (big, entry) = OpenMain(FontFile.UiFace);
        using (big)
        {
            var font = FontFile.Parse(entry.Name, big.Read(entry));
            const string text = "Press Left Mouse Button To Continue";
            var width = Math.Max(8, font.MeasureWidth(text) + 8);
            var height = font.CellHeight + 8;
            var rgba = new byte[width * height * 4];
            font.Blit(rgba, width, height, 4, 4, text);
            Assert.Contains(rgba, b => b != 0);
            ExportDir.WriteRgbaBmp(
                ExportDir.PathFor("fonts", "press-start-text.bmp"),
                width, height, rgba);
        }
    }

    [Fact]
    public void ENG_ARIAL_16_glyph_record_is_00AB96B0_22_bytes()
    {
        var (big, entry) = OpenMain(FontFile.UiFace);
        using (big)
        {
            var font = FontFile.Parse(entry.Name, big.Read(entry));
            Assert.Equal(22, FontFile.GlyphFileBytes);
            Assert.Equal(24, FontFile.GlyphStride);
            Assert.Equal(400, font.Weight);
            Assert.False(font.Flag);
            Assert.Equal(font.CellHeight, font.MetricHeight);
            Assert.Equal(32, font.MinChar);
            Assert.Equal(127, font.MaxChar);
            Assert.Equal(22, font.LineHeight);

            var bang = font.Glyph('!')!.Value;
            Assert.Equal(0, bang.Height);
            Assert.Equal(2, bang.BearingX);
            Assert.Equal(2, bang.WidthMinus1);
            Assert.Equal(4, bang.AdvanceTail);
            Assert.Equal(3, bang.Width);
            Assert.Equal(6, bang.Advance);
            var (x0, y0, x1, y1) = font.AtlasRect(bang);
            Assert.Equal(3, x0);
            Assert.Equal(0, y0);
            Assert.Equal(6, x1);
            Assert.Equal(22, y1);
            Assert.Equal(y1 - y0, font.LineHeight);

            var a = font.Glyph('A')!.Value;
            Assert.Equal(-1, a.BearingX);
            Assert.Equal(13, a.WidthMinus1);
            Assert.Equal(12, a.AdvanceTail);
            Assert.Equal(14, a.Width);
            Assert.Equal(11, a.Advance);
            var aRect = font.AtlasRect(a);
            Assert.Equal(86, aRect.X0);
            Assert.Equal(44, aRect.Y0);
            Assert.Equal(100, aRect.X1);
            Assert.Equal(66, aRect.Y1);

            var space = font.Glyph(' ')!.Value;
            Assert.Equal(0, space.BearingX);
            Assert.Equal(1, space.WidthMinus1);
            Assert.Equal(5, space.AdvanceTail);
            Assert.Equal(2, space.Width);
            Assert.Equal(5, space.Advance);

            Assert.Equal(3f / 127f, bang.U0, 5);
            Assert.Equal(6f / 127f, bang.U1, 5);
            Assert.NotEqual(
                (int)MathF.Round(bang.U0 * 511),
                (int)MathF.Round(bang.U0 * 127));
        }
    }

    [Fact]
    public void ENG_ARIAL_16_atlas_is_128x256_rgba_pitch_512()
    {
        var (big, entry) = OpenMain(FontFile.UiFace);
        using (big)
        {
            var font = FontFile.Parse(entry.Name, big.Read(entry));
            Assert.Equal(128, font.UvWidth);
            Assert.Equal(256, font.UvHeight);
            Assert.Equal(128, font.AtlasHeaderWidth);
            Assert.Equal(256, font.AtlasHeaderHeight);
            Assert.Equal(0x2820, font.AtlasHeaderFormat);
            Assert.True(font.AtlasIsRgba);
            Assert.Equal(128 * 256 * 4, font.Atlas.Length);
            Assert.Equal(128 * 256 * 4, font.AtlasPayloadBytes);
            Assert.Equal(512, font.AtlasPitch);
            Assert.NotEqual(512, font.UvWidth);

            var whiteRgb = 0;
            var zero = 0;
            for (var i = 0; i < font.Atlas.Length; i += 4)
            {
                var r = font.Atlas[i];
                var g = font.Atlas[i + 1];
                var b = font.Atlas[i + 2];
                var a = font.Atlas[i + 3];
                if (r == 0 && g == 0 && b == 0 && a == 0)
                    zero++;
                else if (r == 255 && g == 255 && b == 255 && a > 0)
                    whiteRgb++;
            }

            Assert.True(whiteRgb > 2000, $"whiteRgb={whiteRgb}");
            Assert.True(zero > 20000, $"zero={zero}");
        }
    }

    [Fact]
    public void TEXT_GUI_MENU_PRESS_BUTTON_is_utf16_from_TEXT_ENGLISH_MAIN()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var big = BigArchive.Open(install.TextBigPath);
        var bank = big.SubBanks.Single(item => item.Name == FrontendTextDraw.PressButtonBank);
        var entry = big.ReadEntries(bank).Single(item => item.Name == FrontendTextDraw.PressButtonTag);
        var body = TextPayload.ReadUtf16(big.Read(entry));
        Assert.Equal("Press Left Mouse Button To Continue", body);
    }

    [Fact]
    public void FrontendTextDraw_emits_one_00AB7C20_quad_per_glyph()
    {
        var (big, entry) = OpenMain(FontFile.UiFace);
        using (big)
        {
            var font = FontFile.Parse(entry.Name, big.Read(entry));
            const string text = "Press Left Mouse Button To Continue";
            var quads = FrontendTextDraw.Layout(font, text, 10f, 20f);
            Assert.Equal(text.Length, quads.Count);
            Assert.Equal(0x00AB7C20u, FrontendTextDraw.GlyphDrawFn);
            Assert.Equal(0x0054EF00u, FrontendTextDraw.Type6DrawFn);
            Assert.Equal(0x27, FrontendTextDraw.Type6RecordType);
            Assert.Equal(6, FrontendTextDraw.VerticesPerGlyph);
            Assert.Equal(4, FrontendTextDraw.D3dPrimitiveTriangleList);
            Assert.NotEqual(0x22, FrontendTextDraw.Type6RecordType);

            var bang = font.Glyph('!')!.Value;
            var bangQuad = FrontendTextDraw.Layout(font, "!", 0f, 0f).Single();
            Assert.Equal('!', bangQuad.Character);
            Assert.Equal(bang.BearingX - FrontendTextDraw.HalfPixel, bangQuad.DestX0);
            Assert.Equal(-FrontendTextDraw.HalfPixel, bangQuad.DestY0);
            Assert.Equal(bangQuad.DestX0 + bang.Width, bangQuad.DestX1);
            Assert.Equal(bangQuad.DestY0 + font.LineHeight, bangQuad.DestY1);
            Assert.Equal(font.GpuU(bang.U0), bangQuad.U0);
            Assert.Equal(3f / 128f, bangQuad.U0, 5);
            Assert.Equal(6f / 128f, bangQuad.U1, 5);
            Assert.Equal(0f, bangQuad.V0);
            Assert.Equal(22f / 256f, bangQuad.V1, 5);
            Assert.Equal(FrontendTextDraw.DefaultColor, bangQuad.Color);

            var aQuad = FrontendTextDraw.Layout(font, "A", 0f, 0f).Single();
            Assert.Equal(-1 - FrontendTextDraw.HalfPixel, aQuad.DestX0);
            Assert.Equal(11, font.Glyph('A')!.Value.Advance);

            var two = FrontendTextDraw.Layout(font, "iW", 0f, 0f);
            Assert.Equal(2, two.Count);
            Assert.Equal(font.Glyph('i')!.Value.Advance - FrontendTextDraw.HalfPixel, two[1].DestX0, 3);
            Assert.True(two[0].DestX1 - two[0].DestX0 < two[1].DestX1 - two[1].DestX0);

            var wrapped = FrontendTextDraw.Layout(font, "A\nB", 0f, 0f);
            Assert.Equal(2, wrapped.Count);
            Assert.Equal(font.CellHeight - FrontendTextDraw.HalfPixel, wrapped[1].DestY0);
            Assert.Equal(font.Glyph('B')!.Value.BearingX - FrontendTextDraw.HalfPixel, wrapped[1].DestX0);
        }
    }
}
