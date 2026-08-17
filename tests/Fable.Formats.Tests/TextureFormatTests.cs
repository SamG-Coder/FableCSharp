using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Defs;
using Fable.Formats.Textures;

namespace Fable.Formats.Tests;

public sealed class TextureFormatTests
{
    private static (BigArchive Big, IReadOnlyList<BankEntry> Entries) OpenMain()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var path = Path.Combine(install.DataRoot, "graphics", "pc", "textures.big");
        Assert.True(File.Exists(path));
        var big = BigArchive.Open(path);
        var bank = big.SubBanks.Single(item => item.Name == "GBANK_MAIN_PC");
        return (big, big.ReadEntries(bank));
    }

    [Fact]
    public void Main_bank_info_headers_are_34_bytes_with_power_of_two_size()
    {
        var (big, entries) = OpenMain();
        using (big)
        {
            Assert.True(entries.Count > 1000);
            Assert.All(entries.Take(50), entry => Assert.Equal(34, entry.Info.Count));
            var grass = entries.First(e => e.Name == "LANDSCAPE_GRASS_PLAIN");
            var header = TextureFile.ReadHeader(grass.Info.ToArray());
            Assert.Equal(512, header.Width);
            Assert.Equal(512, header.Height);
            Assert.Equal(512, header.FrameWidth);
            Assert.Equal(512, header.FrameHeight);
            Assert.Equal(31, header.FormatCode);
        }
    }

    [Fact]
    public void Format_31_24bit_tga_is_dxt1_and_32_is_dxt5()
    {
        Assert.Equal(TextureCompression.Dxt1, TextureFile.Classify(0, 31, 512, 512, 1000));
        Assert.Equal(TextureCompression.Dxt5, TextureFile.Classify(0, 32, 64, 64, 1000));
        Assert.Equal(TextureCompression.Dxt5, TextureFile.Classify(0, 35, 512, 512, 1000));
        Assert.Equal(TextureCompression.Rgba8, TextureFile.Classify(4, 1, 64, 64, 1048576));
    }

    [Fact]
    public void Landscape_grass_plain_decodes_to_512_rgba()
    {
        var (big, entries) = OpenMain();
        using (big)
        {
            var grass = entries.First(e => e.Name == "LANDSCAPE_GRASS_PLAIN");
            var texture = TextureFile.Parse(grass.Id, grass.Name, grass.Type, grass.Info, big.Read(grass));
            Assert.Equal(TextureCompression.Dxt1, texture.Compression);
            Assert.Equal(512, texture.Width);
            Assert.Equal(512, texture.Height);
            Assert.Equal(512 * 512 * 4, texture.Rgba.Length);
            Assert.Contains(texture.Rgba, b => b != 0);
            var mean = texture.Rgba.Where((_, i) => i % 4 != 3).Average(b => (double)b);
            Assert.InRange(mean, 10, 250);
        }
    }

    [Fact]
    public void Landscape_sand_and_grass_are_tan_and_olive_not_magenta()
    {
        var (big, entries) = OpenMain();
        using (big)
        {
            static (double R, double G, double B) Mean(TextureFile t)
            {
                double r = 0, g = 0, b = 0;
                var n = t.Rgba.Length / 4;
                for (var i = 0; i < t.Rgba.Length; i += 4)
                {
                    r += t.Rgba[i];
                    g += t.Rgba[i + 1];
                    b += t.Rgba[i + 2];
                }

                return (r / n, g / n, b / n);
            }

            var sand = TextureFile.Parse(0, "s", 0,
                entries.First(e => e.Name == "LANDSCAPE_PATH_SAND_01").Info,
                big.Read(entries.First(e => e.Name == "LANDSCAPE_PATH_SAND_01")));
            var grass = TextureFile.Parse(0, "g", 0,
                entries.First(e => e.Name == "LANDSCAPE_GRASS_PLAIN").Info,
                big.Read(entries.First(e => e.Name == "LANDSCAPE_GRASS_PLAIN")));
            var sm = Mean(sand);
            var gm = Mean(grass);
            Assert.True(sm.R > sm.B && sm.G > sm.B, $"sand should be tan, got {sm}");
            Assert.True(sm.R < sm.G * 1.4, $"sand R/G too magenta {sm}");
            Assert.True(gm.G >= gm.B && gm.R < gm.G + 15, $"grass should be olive, got {gm}");
        }
    }

    [Fact]
    public void Atmospheric_sky_midday_is_dxt5_512_not_dxt1()
    {
        var (big, entries) = OpenMain();
        using (big)
        {
            var entry = entries.First(e => e.Name == "GRAPHIC_ATMOSPHERIC_SKY_MIDDAY");
            var header = TextureFile.ReadHeader(entry.Info.ToArray());
            Assert.Equal(35, header.FormatCode);
            var texture = TextureFile.Parse(entry.Id, entry.Name, entry.Type, entry.Info, big.Read(entry));
            Assert.Equal(TextureCompression.Dxt5, texture.Compression);
            Assert.Equal(512, texture.Width);
            var mid = (texture.Height / 2) * texture.Width * 4;
            Assert.InRange(texture.Rgba[mid + 2], 180, 255);
            Assert.Equal(255, texture.Rgba[3]);
            var lowA = 0;
            for (var i = 3; i < texture.Rgba.Length; i += 16)
                if (texture.Rgba[i] < 20) lowA++;
            Assert.True(lowA < 100, $"sky alpha punched out lowA={lowA}");
        }
    }

    [Fact]
    public void Grassblade_32bit_decodes_as_dxt5_with_alpha()
    {
        var (big, entries) = OpenMain();
        using (big)
        {
            var blade = entries.First(e => e.Name.Contains("GRASSBLADES_01", StringComparison.OrdinalIgnoreCase));
            var header = TextureFile.ReadHeader(blade.Info.ToArray());
            Assert.Equal(64, header.Width);
            Assert.Equal(32, header.FormatCode);
            var texture = TextureFile.Parse(blade.Id, blade.Name, blade.Type, blade.Info, big.Read(blade));
            Assert.Equal(TextureCompression.Dxt5, texture.Compression);
            Assert.Equal(64 * 64 * 4, texture.Rgba.Length);
            Assert.Contains(texture.Rgba.Where((_, i) => i % 4 == 3), a => a < 250);
        }
    }

    [Fact]
    public void First_seen_house_and_landscape_textures_are_dxt_create_not_rgba()
    {
        var (big, entries) = OpenMain();
        using (big)
        {
            Assert.Equal(0x009BE8B0u, TextureFile.CreateTextureDxt1);
            Assert.Equal(0x009BE800u, TextureFile.CreateTextureDxt1Probe);
            Assert.Equal(0x009BE830u, TextureFile.CreateTextureDxt1Named);
            Assert.Equal(0x009BE870u, TextureFile.CreateTextureDxt3);
            Assert.Equal(40u, TextureFile.CreateTextureVtbl);
            Assert.Equal(3, TextureFile.CreateTexturePoolScratch);
            Assert.Equal(0, TextureFile.CreateTextureUsage);
            Assert.Equal(452, TextureFile.CreateTextureLevelsOffset);
            Assert.Equal(0x00416C8Au, TextureFile.InitGraphics);
            Assert.Equal(0x00416D20u, TextureFile.InitGraphicsDxt5Name);
            Assert.True(TextureFile.FirstSeenCreateTextureUsesDxtFourCc);
            Assert.False(TextureFile.FirstSeenInitGraphicsDxt5NameIsCreateTexture);
            Assert.True(TextureFile.FirstSeenTextureStoresRawLowerMips);
            Assert.Equal(4, TextureFile.FirstSeenLowerMipStop);

            TextureFile Load(int id)
            {
                var entry = entries.First(e => e.Id == (uint)id);
                return TextureFile.Parse(entry.Id, entry.Name, entry.Type, entry.Info, big.Read(entry));
            }

            var grass = Load(414);
            var floor = Load(4130);
            var wall = Load(GameBin.HerosOldHouseInteriorWallTexture);
            foreach (var tex in new[] { grass, floor, wall })
            {
                Assert.Equal(TextureCompression.Dxt1, tex.Compression);
                var top = TextureFile.TopMipSize(tex.Width, tex.Height, tex.Compression);
                var lower = TextureFile.RawLowerMipSize(tex.Width, tex.Height, tex.Compression);
                var chain = TextureFile.ExpectedSize(tex.Width, tex.Height, tex.Compression);
                Assert.True(top > 0 && lower > 0 && chain > top + lower,
                    $"{tex.Name} top={top} lower={lower} chain={chain}");
                Assert.True(tex.PayloadBytes == top,
                    $"{tex.Name} payload={tex.PayloadBytes} top={top} leftover={tex.LeftoverBytes}");
                Assert.True(tex.LeftoverBytes == lower,
                    $"{tex.Name} leftover={tex.LeftoverBytes} lower={lower}");
                Assert.True(tex.PayloadHasRawLowerMips, $"{tex.Name} leftover={tex.LeftoverBytes}");
                Assert.False(tex.PayloadIsTopMipOnly);
                Assert.False(tex.PayloadIsFullMipChain);
                var mip256 = Dxt.Decode(
                    tex.LowerMips.AsSpan(0, TextureFile.TopMipSize(256, 256, tex.Compression)),
                    256, 256, DxtKind.Dxt1);
                Assert.Contains(mip256, b => b != 0);
            }
        }
    }
}
