using Fable.Core;
using Fable.Formats.Levels;
using Fable.Formats.Qst;
using Fable.Formats.Tng;
using Fable.Formats.Wld;
using Fable.Game;

namespace Fable.Formats.Tests;

/// <summary>
/// World-scene chain: WLD maps, BWD AABBs/UIDs, starting region graph,
/// and TNG region exit links that pack a neighbour MapUID.
/// </summary>
public sealed class WorldSceneTests
{
    private static GameInstall Require()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        return install;
    }

    [Fact]
    public void Bwd_extra_u32_is_the_wld_map_uid()
    {
        var install = Require();
        var world = WorldFile.Load(install.WorldPath);
        var bwd = BwdFile.Load(install.BwdPath);
        var lookout = bwd.Find("LookoutPoint");
        Assert.NotNull(lookout);
        Assert.Equal(162441, lookout.Value.MapUid);
        Assert.Equal(world.FindMap("LookoutPoint")!.MapUid, lookout.Value.MapUid);

        var matched = world.Maps.Count(map => bwd.Find(map.ScriptName)?.MapUid == map.MapUid);
        Assert.True(matched >= 390, $"matched={matched}/{world.Maps.Count}");
        Assert.Equal(72, world.MapUidCount);
        Assert.Equal(398, world.Maps.Count);
    }

    [Fact]
    public void Starting_region_graph_lists_lookout_neighbours()
    {
        var install = Require();
        var graph = RegionGraph.Load(install.StartingRegionGraphPath);
        var neighbors = graph.NeighborsOf("LookoutPoint");
        Assert.Contains("PicnicArea", neighbors);
        Assert.Contains("BowerstoneSlums", neighbors);
        Assert.Contains("GreatwoodEntrance", neighbors);
        Assert.Contains("HeroGuildComplexInside", neighbors);
        Assert.Contains("DemonDoor_LookoutPoint", neighbors);
        Assert.True(graph.Neighbors.Count >= 80, $"regions={graph.Neighbors.Count}");
        Assert.Contains("LookoutPoint", graph.NeighborsOf("PicnicArea"));
    }

    [Fact]
    public void Picnic_aabb_shares_the_west_edge_of_lookout()
    {
        var install = Require();
        var bwd = BwdFile.Load(install.BwdPath);
        var lookout = bwd.Find("LookoutPoint")!.Value;
        var picnic = bwd.Find("PicnicArea")!.Value;
        Assert.Equal(lookout.MinX, picnic.MaxX);
        Assert.True(picnic.MinY < lookout.MaxY && picnic.MaxY > lookout.MinY);
    }

    [Fact]
    public void Lookout_exit_uids_pack_neighbour_map_uids()
    {
        var install = Require();
        using var levels = new LevelLibrary(install);
        var world = WorldFile.Load(install.WorldPath);
        var things = levels.LoadThings("LookoutPoint").Things.ToList();
        var exits = things.Where(t => t.DefinitionType == "REGION_EXIT_POINT").ToList();
        Assert.Equal(2, exits.Count);
        Assert.Contains(things, t => t.DefinitionType == "REGION_ENTRANCE_POINT");
        Assert.Contains(things, t => t.DefinitionType == "OBJECT_REGION_TRANSITION_GATE");

        var linked = new List<string>();
        foreach (var exit in exits)
        {
            Assert.True(exit.Properties.TryGetValue("CTCDRegionExit.EntranceConnectedToUID", out var text));
            var packed = ulong.Parse(text);
            var mapUid = RegionGraph.MapUidFromEntranceLink(packed);
            var dest = world.Maps.FirstOrDefault(map => map.MapUid == mapUid);
            Assert.NotNull(dest);
            linked.Add(dest.ScriptName);
        }

        Assert.Contains("PicnicArea", linked);
        Assert.Contains("Greatwood_1", linked);
    }

    [Fact]
    public void Gtg_kick_points_name_bowerstone_regions()
    {
        var install = Require();
        var gtg = ThingFile.Load(install.GtgPath);
        var kicks = gtg.Things.Where(t => t.DefinitionType == "REGION_KICK_TO_POINT").ToList();
        Assert.True(kicks.Count >= 2);
        Assert.Contains(kicks, t => t.Properties.GetValueOrDefault("ScriptData") == "BowerstonePosh");
        Assert.Contains(kicks, t => t.Properties.GetValueOrDefault("ScriptData") == "BowerstoneSlums");
    }

    [Fact]
    public void Global_quests_file_parses_watch_for_hero_death()
    {
        var install = Require();
        var quests = QuestFile.Load(install.GlobalQuestPath);
        Assert.Contains(quests.Quests, q => q.Name == "Global_WatchForHeroDeath" && q.Persistent);
        Assert.Contains(quests.Quests, q => q.Name.StartsWith("Expression_", StringComparison.Ordinal));
    }
}
