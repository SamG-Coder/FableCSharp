using System.Text;
using Fable.Core;
using Fable.Formats.Defs;
using Fable.Formats.IO;

namespace Fable.Formats.Tests;

/// <summary>
/// Negative notes for compiled game.bin. It is not text, not names.bin hashes,
/// and not Fable-framed LZO. Object-to-mesh links stay unread until this
/// control-byte format is decoded.
/// </summary>
public sealed class GameBinFormatTests
{
    [Fact]
    public void Game_bin_has_no_ascii_object_names()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var path = install.FindCompiledDef("game.bin");
        Assert.NotNull(path);
        var bytes = File.ReadAllBytes(path);
        Assert.True(bytes.Length > 100_000);
        var text = Encoding.ASCII.GetString(bytes);
        Assert.DoesNotContain("OBJECT_WALL_SMALL_POST_01", text);
        Assert.DoesNotContain("CREATURE_HERO", text);
        Assert.DoesNotContain("#definition", text);
    }

    [Fact]
    public void Names_bin_hashes_are_not_the_game_bin_index()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
        var game = File.ReadAllBytes(install.FindCompiledDef("game.bin")!);
        var wall = names.Entries.First(entry => entry.Name == "OBJECT_WALL_SMALL_POST_01");
        var hits = 0;
        for (var i = 0; i + 4 <= game.Length; i += 4)
        {
            if (BitConverter.ToUInt32(game, i) == wall.Hash)
                hits++;
        }

        Assert.Equal(0, hits);
    }

    [Fact]
    public void Framed_lzo_at_start_is_not_the_def_table()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bytes = File.ReadAllBytes(install.FindCompiledDef("game.bin")!);
        var cursor = 0;
        var decoded = Lzo.DecompressFramed(bytes, ref cursor, 2_000_000);
        var ascii = decoded.Count(value => value is >= 32 and <= 126);
        Assert.True(ascii < 1000, $"unexpectedly dense ASCII in framed LZO decode ascii={ascii}");
        Assert.True(cursor < bytes.Length / 4);
    }
}
