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
    public void Press_Start_glyphs_are_monotonic_00AB7C20_layout()
    {
        var (big, entry) = OpenMain(FontFile.UiFace);
        using (big)
        {
            var font = FontFile.Parse(entry.Name, big.Read(entry));
            const string text = FrontendTextDraw.PressButtonText;
            var quads = FrontendTextDraw.Layout(font, text, 320f, 240f);
            Assert.Equal(text.Length, quads.Count);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("char\tglyph\tatlas\twidth\tadvance\tscreen");
            float? prev = null;
            foreach (var q in quads)
            {
                sb.AppendLine(
                    $"'{q.Character}'\t{q.GlyphIndex}\t{q.AtlasX0},{q.AtlasY0},{q.AtlasX1},{q.AtlasY1}\t" +
                    $"{q.Width}\t{q.Advance}\t{q.DestX0},{q.DestY0},{q.DestX1},{q.DestY1}");
                if (prev is { } x)
                    Assert.True(q.DestX0 >= x, sb.ToString());
                prev = q.DestX0;
                Assert.True(q.Advance != 0 || q.Character == ' ');
            }

            File.WriteAllText(ExportDir.PathFor("fonts", "press-start-glyphs.txt"), sb.ToString());
            Assert.Equal('P', quads[0].Character);
            Assert.True(quads[^1].DestX0 > quads[0].DestX0);
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

    [Fact]
    public void A_is_wider_than_i_from_22_byte_record()
    {
        var (big, entry) = OpenMain(FontFile.UiFace);
        using (big)
        {
            var font = FontFile.Parse(entry.Name, big.Read(entry));
            var a = font.Glyph('A')!.Value;
            var i = font.Glyph('i')!.Value;
            Assert.True(a.Width > i.Width, $"A={a.Width} i={i.Width}");
            Assert.True(a.Advance > i.Advance, $"A adv={a.Advance} i adv={i.Advance}");
            Assert.Equal(14, a.Width);
            Assert.Equal(3, i.Width);
        }
    }

    [Fact]
    public void Press_Start_measure_is_sum_of_00AB7B00_advances()
    {
        var (big, entry) = OpenMain(FontFile.UiFace);
        using (big)
        {
            var font = FontFile.Parse(entry.Name, big.Read(entry));
            const string text = FrontendTextDraw.PressButtonText;
            var (width, height) = font.Measure(text);
            var sum = 0;
            foreach (var ch in text)
                sum += font.GlyphAt(ch)!.Value.Advance;
            Assert.Equal(sum, width);
            Assert.Equal(font.LineHeight, height);
            Assert.Equal(0, font.Measure("").Width);
            Assert.Equal(0, font.Measure("").Height);
            Assert.Equal(font.LineHeight * 2, font.Measure("A\nB").Height);
        }
    }

    [Fact]
    public void Press_Start_glyph_table_is_monotonic_00AB7C20()
    {
        var (big, entry) = OpenMain(FontFile.UiFace);
        using (big)
        {
            var font = FontFile.Parse(entry.Name, big.Read(entry));
            const string text = FrontendTextDraw.PressButtonText;
            var quads = FrontendTextDraw.Layout(font, text, 0f, 0f);
            Assert.Equal(text.Length, quads.Count);
            for (var i = 0; i < quads.Count; i++)
            {
                Assert.Equal(text[i], quads[i].Character);
                Assert.Equal(text[i], quads[i].GlyphIndex);
                Assert.Equal(font.GlyphAt(text[i])!.Value.Width, quads[i].Width);
                Assert.Equal(font.GlyphAt(text[i])!.Value.Advance, quads[i].Advance);
                if (i > 0)
                    Assert.True(quads[i].DestX0 > quads[i - 1].DestX0,
                        $"{text[i - 1]} destX={quads[i - 1].DestX0} then {text[i]} destX={quads[i].DestX0}");
            }

            var bang = FrontendTextDraw.Layout(font, "!", 0f, 0f).Single();
            var verts = FrontendTextDraw.NativeVerts(bang);
            Assert.Equal(6, verts.Length);
            Assert.Equal(bang.U0, verts[1].U);
            Assert.Equal(bang.V0, verts[1].V);
            Assert.Equal(bang.DestX0, verts[1].X);
            Assert.Equal(bang.DestY0, verts[1].Y);
            Assert.Equal(bang.U1, verts[2].U);
            Assert.Equal(bang.V1, verts[2].V);
            Assert.Equal(bang.DestX1, verts[2].X);
            Assert.Equal(bang.DestY1, verts[2].Y);
            Assert.Equal(0x0054FFF0u, FrontendTextDraw.Type6AlignFn);
            Assert.Equal(2f, FrontendTextDraw.Type6OriginPad);
            Assert.Equal(0, FrontendTextDraw.AlignFromFlag302(0));
            Assert.Equal(1, FrontendTextDraw.AlignFromFlag302(0x10));
            Assert.Equal(2, FrontendTextDraw.AlignFromFlag302(0x20));
            Assert.Equal(10f, FrontendTextDraw.Type6AlignedX(10f, 20f, 1f, FrontendTextDraw.AlignLeft));
            Assert.Equal(0f, FrontendTextDraw.Type6AlignedX(10f, 20f, 1f, FrontendTextDraw.AlignCentre));
            Assert.Equal(-10f, FrontendTextDraw.Type6AlignedX(10f, 20f, 1f, FrontendTextDraw.AlignRight));
            WriteGlyphTable(font, quads);
        }
    }

    private static void WriteGlyphTable(FontFile font, IReadOnlyList<FrontendTextDraw.GlyphQuad> quads)
    {
        var root = AppContext.BaseDirectory;
        string? dir = null;
        while (!string.IsNullOrEmpty(root))
        {
            if (File.Exists(Path.Combine(root, "FableCSharp.slnx")))
            {
                dir = Path.Combine(root, "implementer", "frontend");
                break;
            }

            root = Path.GetDirectoryName(root)!;
        }

        dir ??= Path.Combine(AppContext.BaseDirectory, "implementer", "frontend");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "13-glyphs.md");
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# 13 — ENG_ARIAL_16 Press Start glyphs");
        sb.AppendLine();
        sb.AppendLine("Authority: `Fable.exe` `00AB96B0` / `00AB7A10` / `00AB7B00` /");
        sb.AppendLine("`00AB7C20` / `0054EF00` / `0054FFF0`. Face `fonts.big`");
        sb.AppendLine("`FONT_ENGLISH_MAIN` / `ENG_ARIAL_16`.");
        sb.AppendLine();
        sb.AppendLine("## 00AB7C20 vertex UV assignment");
        sb.AppendLine();
        sb.AppendLine("GPU UV = stored × (atlas−1)/atlas. Dest − `[0x122F59C]=0.5`.");
        sb.AppendLine("Dest height = `CellHeight+1`. Six 28-byte XYZRHW verts,");
        sb.AppendLine("prim 4, flush `00A0ABE0`.");
        sb.AppendLine();
        sb.AppendLine("| Vert | Dest | UV | Role |");
        sb.AppendLine("|---|---|---|---|");
        sb.AppendLine("| 0 | (X0, Y1) | (U0, V1) | BL |");
        sb.AppendLine("| 1 | (X0, Y0) | (U0, V0) | TL |");
        sb.AppendLine("| 2 | (X1, Y1) | (U1, V1) | BR |");
        sb.AppendLine("| 3 | (X1, Y0) | (U1, V0) | TR |");
        sb.AppendLine("| 4 | (X1, Y1) | (U1, V1) | BR |");
        sb.AppendLine("| 5 | (X0, Y0) | (U0, V0) | TL |");
        sb.AppendLine();
        sb.AppendLine("**U0/V0 is dest top-left. V is not inverted.**");
        sb.AppendLine();
        sb.AppendLine("## 0054EF00 colour, align, +204");
        sb.AppendLine();
        sb.AppendLine("- Colour: widget `+148..+151` packed as a DWORD");
        sb.AppendLine("  (`[148][149][150][151]` LE) into the type `0x27` record.");
        sb.AppendLine("- Align `vtbl+600` `0054FFF0`: `+302` bit4 → 1 centre,");
        sb.AppendLine("  bit5 → 2 right, else 0 left. First-seen bits stay 0 → left.");
        sb.AppendLine("- Scale of `+204`: `scale * [esi+204]`. Scale is `+264`");
        sb.AppendLine("  when `+392!=0`, else `+124`. Centre also × 0.5, then");
        sb.AppendLine("  `originX - that`. Writer of leftover `+204` is not this fn.");
        sb.AppendLine("- Then `+ [0x122DCDC]=2` on X and Y before the record.");
        sb.AppendLine();
        sb.AppendLine("## First scramble cause");
        sb.AppendLine();
        sb.AppendLine("Type-6 glyphs were submitted as type `0x22` sprite records");
        sb.AppendLine("(stride 32, UV 0,0,1,1 family) instead of type `0x27` /");
        sb.AppendLine("`00AB7C20` 6×28-byte verts with per-glyph GPU UV.");
        sb.AppendLine("A whole-atlas 0,0,1,1 quad at the text dest is scrambled.");
        sb.AppendLine();
        sb.AppendLine("## Press Start table (`00AB7C20` at 0,0)");
        sb.AppendLine();
        var measure = font.Measure(FrontendTextDraw.PressButtonText);
        sb.AppendLine($"`TEXT_GUI_MENU_PRESS_BUTTON` = `{FrontendTextDraw.PressButtonText}`.");
        sb.AppendLine($"Measure **{measure.Width} × {measure.Height}**. destX includes bearing − 0.5. destX is monotonic.");
        sb.AppendLine();
        sb.AppendLine("| i | char | ch | atlas X0 Y0 X1 Y1 | width | advance | destX | destY |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|");
        for (var i = 0; i < quads.Count; i++)
        {
            var q = quads[i];
            var label = q.Character == ' ' ? "sp" : q.Character.ToString();
            sb.AppendLine(
                $"| {i} | `{label}` | {q.GlyphIndex} | " +
                $"{q.AtlasX0} {q.AtlasY0} {q.AtlasX1} {q.AtlasY1} | " +
                $"{q.Width} | {q.Advance} | {q.DestX0} | {q.DestY0} |");
        }

        sb.AppendLine();
        sb.AppendLine("destX is strictly increasing. Width comes from");
        sb.AppendLine("`WidthMinus1+1` in the 22-byte record, not column scanning.");
        sb.AppendLine("Advance = BearingX + AdvanceTail.");
        sb.AppendLine();
        sb.AppendLine("## Host path");
        sb.AppendLine();
        sb.AppendLine("`FontFile.GlyphAt` / `GpuU` / `AtlasRect` match `00AB7A10` /");
        sb.AppendLine("`00AB7C20`. `FrontendTextDraw.Layout` emits those dest/UV quads.");
        sb.AppendLine("`EngineLifecycle` submits type `0x27` per glyph (not sprite");
        sb.AppendLine("0,0,1,1). Type-6 pen adds `+2` (`0x122DCDC`).");
        File.WriteAllText(path, sb.ToString());
    }
}
