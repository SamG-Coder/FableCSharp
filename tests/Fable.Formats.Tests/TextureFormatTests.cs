using Fable.Core;
using Fable.Formats.Banks;
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
}
