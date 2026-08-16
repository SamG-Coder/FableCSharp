using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Defs;
using Fable.Formats.Qst;
using Fable.Formats.Tng;
using Fable.Formats.Wld;
using Fable.Game;

namespace Fable.Formats.Tests;

public sealed class TlcInstallTests
{
    private static GameInstall RequireInstall()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        return install;
    }

    [Fact]
    public void Locates_lost_chapters_first()
    {
        var install = RequireInstall();
        Assert.Equal(GameEdition.TheLostChapters, install.Edition);
        Assert.True(File.Exists(install.WorldPath));
        Assert.True(File.Exists(install.WadPath));
        Assert.NotNull(install.FindCompiledDef("names.bin"));
    }

    [Fact]
    public void World_starts_at_lookout_point()
    {
        var world = WorldFile.Load(RequireInstall().WorldPath);
        Assert.Contains("Q_SunnyvaleMaster", world.InitialQuests);
        Assert.True(world.Maps.Count >= 70);

        var first = world.Maps[0];
        Assert.Equal(1, first.Index);
        Assert.Equal("LookoutPoint", first.ScriptName);
        Assert.Equal(3232, first.MapX);
        Assert.Equal(3488, first.MapY);
        Assert.NotNull(world.FindMap("PicnicArea"));
    }

    [Fact]
    public void Wad_contains_lookout_point_tng()
    {
        using var wad = BbbArchive.Open(RequireInstall().WadPath);
        Assert.True(wad.Entries.Count > 100);
        var tng = wad.Find("LookoutPoint.tng");
        Assert.NotNull(tng);
        Assert.True(tng.Size > 100);
        Assert.Contains("LookoutPoint", tng.Name, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Lookout_point_things_have_positions()
    {
        using var levels = new LevelLibrary(RequireInstall());
        var file = levels.LoadThings("LookoutPoint");
        var things = file.Things.ToList();
        Assert.True(things.Count > 10);
        Assert.Contains(things, thing => thing.DefinitionType == "MARKER_BASIC");
        Assert.Contains(things, thing => thing.PositionX is not null);
    }

    [Fact]
    public void Tng_text_parser_reads_sample()
    {
        const string sample = """
            Version 2;
            XXXSectionStart Gameflow;
            NewThing Marker;
            Player -1;
            UID 18446741874686296750;
            DefinitionType "MARKER_BASIC";
            ScriptName M_Maze;
            StartCTCPhysicsStandard;
            PositionX 49.669189;
            PositionY 76.648438;
            PositionZ 35.252132;
            EndCTCPhysicsStandard;
            EndThing;
            XXXSectionEnd;
            """;

        var file = ThingFile.Parse(sample);
        var thing = Assert.Single(file.Things);
        Assert.Equal("Marker", thing.Kind);
        Assert.Equal("MARKER_BASIC", thing.DefinitionType);
        Assert.Equal("M_Maze", thing.ScriptName);
        Assert.Equal(49.669189f, thing.PositionX);
        Assert.Equal(76.648438f, thing.PositionY);
        Assert.Equal(35.252132f, thing.PositionZ);
    }

    [Fact]
    public void Quest_table_includes_opening()
    {
        var quests = QuestFile.Load(RequireInstall().QuestPath);
        Assert.Contains(quests.Quests, quest => quest.Name == "Q_SunnyvaleMaster" && quest.Persistent);
        Assert.True(quests.Quests.Count > 50);
    }

    [Fact]
    public void Names_bin_lists_marker_and_oakvale()
    {
        var path = RequireInstall().FindCompiledDef("names.bin");
        Assert.NotNull(path);
        var names = NamesBin.Load(path);
        Assert.True(names.Entries.Count > 10_000);
        Assert.Contains(names.Entries, entry => entry.Name == "MARKER_BASIC");
        Assert.Contains(names.Entries, entry => entry.Name.Contains("OAKVALE", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Graphics_big_has_mesh_subbanks()
    {
        var install = RequireInstall();
        var path = Path.Combine(install.DataRoot, "graphics", "graphics.big");
        Assert.True(File.Exists(path));
        using var big = BigArchive.Open(path);
        Assert.NotEmpty(big.SubBanks);
        Assert.Contains(big.SubBanks, bank => bank.Name.Contains("MESH", StringComparison.OrdinalIgnoreCase));
    }
}
