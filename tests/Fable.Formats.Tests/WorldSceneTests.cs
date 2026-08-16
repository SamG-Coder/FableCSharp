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
    public void Exit_link_low_bits_are_the_destination_entrance_uid()
    {
        var install = Require();
        using var wad = Fable.Formats.Banks.BbbArchive.Open(install.WadPath);
        var world = WorldFile.Load(install.WorldPath);
        var byUid = world.Maps.ToDictionary(map => map.MapUid, map => map);
        var files = new Dictionary<string, ThingFile>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in wad.Entries.Where(e => e.Name.EndsWith(".tng", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                files[Path.GetFileNameWithoutExtension(entry.Name)] =
                    ThingFile.Parse(System.Text.Encoding.ASCII.GetString(wad.Read(entry)));
            }
            catch
            {
                // skip unreadable
            }
        }

        var lookout = files["LookoutPoint"];
        var picnic = files["PicnicArea"];
        var picnicExit = lookout.Things.First(t =>
            t.DefinitionType == "REGION_EXIT_POINT" &&
            t.Properties["CTCDRegionExit.EntranceConnectedToUID"] == "179907590094848033");
        var link = RegionLink.Unpack(179907590094848033UL);
        Assert.Equal(163625, link.MapUid);
        Assert.Equal(0x21u, link.EntranceSlot);
        Assert.Equal(179907590094848033UL, link.Pack());
        var dest = link.FindEntrance(picnic.Things);
        Assert.NotNull(dest);
        Assert.InRange(dest!.PositionX!.Value, 79f, 80f);

        var matched = 0;
        var missingMap = 0;
        foreach (var (stem, file) in files)
        {
            foreach (var thing in file.Things)
            {
                if (thing.DefinitionType != "REGION_EXIT_POINT")
                    continue;
                if (!thing.Properties.TryGetValue("CTCDRegionExit.EntranceConnectedToUID", out var text) ||
                    !ulong.TryParse(text, out var packed))
                    continue;
                var decoded = RegionLink.Unpack(packed);
                if (!byUid.TryGetValue(decoded.MapUid, out var map))
                {
                    missingMap++;
                    continue;
                }

                var destFile = files.GetValueOrDefault(map.FileStem) ?? files.GetValueOrDefault(map.ScriptName);
                if (destFile is null)
                {
                    missingMap++;
                    continue;
                }

                Assert.NotNull(decoded.FindEntrance(destFile.Things));
                matched++;
            }
        }

        Assert.True(matched >= 120, $"matched={matched} missingMap={missingMap}");
    }

    [Fact]
    public void Bwd_display_table_titles_and_minimap_coords()
    {
        var install = Require();
        var world = WorldFile.Load(install.WorldPath);
        var bwd = BwdFile.Load(install.BwdPath);
        Assert.True(bwd.Displays.Count >= 90, $"displays={bwd.Displays.Count}");
        Assert.Equal(world.MapUidCount, bwd.Displays.Count(d => world.FindMap(d.ScriptName) is not null));

        var lookout = bwd.FindDisplay("LookoutPoint");
        var picnic = bwd.FindDisplay("PicnicArea");
        Assert.NotNull(lookout);
        Assert.NotNull(picnic);
        Assert.Equal("TXT_REGION_LOOKOUT_POINT", lookout.Value.TextKey);
        Assert.Equal("MINIMAP_LOOKOUTPOINT", lookout.Value.MinimapName);
        Assert.Equal(1f, lookout.Value.Scale, 2);
        Assert.True(picnic.Value.MapX < lookout.Value.MapX, $"picnic={picnic.Value.MapX} lookout={lookout.Value.MapX}");
        Assert.Contains("PicnicArea", lookout.Value.LinkedNames);
        Assert.Contains("HeroGuildComplexInside", lookout.Value.LinkedNames);

        using var big = Fable.Formats.Banks.BigArchive.Open(install.TextBigPath);
        var entry = big.ReadEntries(big.SubBanks[0])
            .First(e => e.Name == lookout.Value.TextKey);
        Assert.Equal("Lookout Point", Fable.Formats.Text.TextPayload.ReadUtf16(big.Read(entry)));
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
