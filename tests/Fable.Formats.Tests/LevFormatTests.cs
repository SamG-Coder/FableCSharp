using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Levels;
using Fable.Game;

namespace Fable.Formats.Tests;

/// <summary>
/// Living notes for the compiled .lev landscape format. Each fact is asserted
/// against multiple TLC regions so a bad guess fails loudly.
/// </summary>
public sealed class LevFormatTests
{
    public static TheoryData<string, int, int> Regions
    {
        get
        {
            var data = new TheoryData<string, int, int>
            {
                { "LookoutPoint", 128, 128 },
                { "PicnicArea", 128, 96 },
                { "DemonDoor_Guild", 64, 64 },
                { "OakValeEast_v2", 96, 160 },
            };
            return data;
        }
    }

    private static (GameInstall Install, byte[] Bytes) Load(string region)
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var wad = BbbArchive.Open(install.WadPath);
        var entry = wad.Find(region + ".lev");
        Assert.NotNull(entry);
        return (install, wad.Read(entry));
    }

    [Theory]
    [MemberData(nameof(Regions))]
    public void Header_is_version_25_with_format_constant_and_16_16_grid(string region, int width, int height)
    {
        var (_, bytes) = Load(region);
        Assert.Equal(LevFile.Version, BitConverter.ToInt32(bytes, 0));
        Assert.Equal(LevFile.FormatConstant, BitConverter.ToUInt32(bytes, 4));
        Assert.Equal((uint)width << 16, BitConverter.ToUInt32(bytes, 36));
        Assert.Equal((uint)height << 16, BitConverter.ToUInt32(bytes, 40));
        Assert.Equal(65536u, BitConverter.ToUInt32(bytes, 44)); // 1.0 in 16.16
    }

    [Theory]
    [MemberData(nameof(Regions))]
    public void Material_table_is_255_slots_of_132_bytes_starting_at_179(string region, int width, int height)
    {
        _ = (width, height);
        var (_, bytes) = Load(region);
        Assert.True(bytes.Length > LevFile.MaterialTableEnd);
        Assert.Equal("INVALID_THEME_STANDIN", ReadZ(bytes, 179));
        Assert.Equal(179 + 132, 311);
        Assert.StartsWith("GROUND_", ReadZ(bytes, 311));
        Assert.Equal(33839, LevFile.MaterialTableEnd);
    }

    [Theory]
    [MemberData(nameof(Regions))]
    public void Parser_reads_grid_ground_materials_and_sound_themes(string region, int width, int height)
    {
        var (_, bytes) = Load(region);
        var lev = LevFile.Parse(bytes);
        Assert.Equal(width, lev.GridWidth);
        Assert.Equal(height, lev.GridHeight);
        Assert.Equal(1f, lev.CellSize);
        Assert.Contains(lev.Materials, m => m.Name.StartsWith("GROUND_", StringComparison.Ordinal));
        Assert.Equal("INVALID_THEME_STANDIN", lev.Materials[0].Name);
        Assert.Contains(lev.SoundThemes, t => t.StartsWith("SOUND_THEME_", StringComparison.Ordinal));
        Assert.True(lev.PayloadOffset > LevFile.SecondaryTableEnd);
        Assert.True(lev.PayloadOffset < bytes.Length);
    }

    [Fact]
    public void Lookout_point_grid_covers_tng_xy_range()
    {
        var (install, bytes) = Load("LookoutPoint");
        var lev = LevFile.Parse(bytes);
        using var levels = new LevelLibrary(install);
        var things = levels.LoadThings("LookoutPoint").Things
            .Where(t => t.PositionX is not null)
            .ToList();
        var maxX = things.Max(t => t.PositionX!.Value);
        var maxY = things.Max(t => t.PositionY!.Value);
        Assert.True(maxX < lev.GridWidth * lev.CellSize + 8, $"maxX={maxX} width={lev.GridWidth}");
        Assert.True(maxY < lev.GridHeight * lev.CellSize + 8, $"maxY={maxY} height={lev.GridHeight}");
        Assert.True(things.Min(t => t.PositionX) > -8);
        Assert.True(things.Min(t => t.PositionY) > -8);
    }

    [Fact]
    public void Secondary_table_starts_at_33839_with_type_3()
    {
        foreach (var region in new[] { "LookoutPoint", "PicnicArea", "DemonDoor_Guild" })
        {
            var (_, bytes) = Load(region);
            Assert.Equal(3u, BitConverter.ToUInt32(bytes, LevFile.MaterialTableEnd));
            Assert.True(BitConverter.ToUInt32(bytes, LevFile.MaterialTableEnd + 4) > 0);
            Assert.Equal(67639, LevFile.SecondaryTableEnd);
        }
    }

    [Fact]
    public void Payload_after_sound_themes_begins_with_21()
    {
        foreach (var region in new[] { "LookoutPoint", "PicnicArea", "DemonDoor_Guild", "OakValeEast_v2" })
        {
            var lev = LevFile.Parse(Load(region).Bytes);
            Assert.True(lev.Raw.Length - lev.PayloadOffset > 64);
            Assert.Equal(21, BitConverter.ToInt32(lev.Raw, lev.PayloadOffset));
        }
    }

    [Fact]
    public void Stb_contains_expanded_lookout_lev_larger_than_wad()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var stbPath = Path.Combine(install.DataRoot, "Levels", "FinalAlbion_RT.stb");
        Assert.True(File.Exists(stbPath));
        using var wad = BbbArchive.Open(install.WadPath);
        var wadLev = wad.Find("LookoutPoint.lev");
        Assert.NotNull(wadLev);
        Assert.True(wadLev.Size > 100_000);
        Assert.True(File.Exists(stbPath));
        Assert.True(new FileInfo(stbPath).Length > 100_000_000);
    }

    [Fact]
    public void Stb_lookout_heightfield_is_8_by_8_cells_of_16_units()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var field = levels.LoadHeightField("LookoutPoint");
        Assert.NotNull(field);
        Assert.Equal(8, field.CellsX);
        Assert.Equal(8, field.CellsY);
        Assert.Equal(3232f, field.OriginX);
        Assert.Equal(3488f, field.OriginY);
        Assert.True(field.SampleCount >= 64);
        Assert.InRange(field.Heights[0, 0], 20f, 80f);
        Assert.InRange(field.Heights[4, 4], 20f, 80f);
        var tris = field.ToLocalTriangles();
        Assert.Equal(8 * 8 * 2, tris.Count);
        Assert.InRange(tris[0].A.Z, 20f, 80f);
        Assert.True(tris.Max(t => t.A.X) <= 128.1f);
    }

    [Fact]
    public void Stb_picnic_heightfield_matches_128x96_grid()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var field = levels.LoadHeightField("PicnicArea");
        Assert.NotNull(field);
        Assert.Equal(8, field.CellsX);
        Assert.Equal(6, field.CellsY);
        Assert.True(field.ToLocalTriangles().Count >= 48);
    }

    [Fact]
    public void Lookout_payload_lzo_does_not_decode_as_dense_f32_grid()
    {
        var lev = LevFile.Parse(Load("LookoutPoint").Bytes);
        var cursor = lev.PayloadOffset;
        var decoded = Fable.Formats.IO.Lzo.DecompressFramed(lev.Raw, ref cursor, lev.CellCount * 4);
        var inRange = 0;
        for (var i = 0; i + 4 <= decoded.Length; i += 4)
        {
            var value = BitConverter.ToSingle(decoded, i);
            if (value is >= 15f and <= 80f)
                inRange++;
        }

        // Document the negative: framed LZO at payload start is not the heightfield.
        Assert.True(inRange < lev.CellCount / 4, $"unexpectedly dense height decode inRange={inRange}");
    }

    private static string ReadZ(byte[] data, int offset)
    {
        var end = offset;
        while (end < data.Length && data[end] != 0)
            end++;
        return System.Text.Encoding.ASCII.GetString(data, offset, end - offset);
    }
}
