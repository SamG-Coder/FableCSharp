using System.Numerics;
using Fable.Core;
using Fable.Formats;
using Fable.Formats.Levels;
using Fable.Formats.Qst;
using Fable.Formats.Scene;
using Fable.Formats.Tng;
using Fable.Formats.Wld;
using Fable.Game;
using Fable.Render;

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
    public void World_file_parses_new_region_contains_and_sees()
    {
        var world = WorldFile.Parse("""
            NewMap 1
            LevelName "FinalAlbion\LookoutPoint.lev"
            LevelScriptName "LookoutPoint"
            MapUID 1
            EndMap
            NewRegion 4
            RegionName "StartOakVale"
            NewDisplayName "TXT_REGION_OAKVALE"
            RegionDef "REGION_OAK_VALE_INTRO"
            ContainsMap "FinalAlbion\StartOakValeWest.lev"
            ContainsMap "FinalAlbion\StartOakValeEast.lev"
            SeesMap "FinalAlbion\StartOakVale_Filler_01.lev"
            EndRegion
            """.Split('\n'));
        Assert.Single(world.Maps);
        Assert.Single(world.Regions);
        var region = world.Regions[0];
        Assert.Equal(4, region.Index);
        Assert.Equal("StartOakVale", region.RegionName);
        Assert.Equal("TXT_REGION_OAKVALE", region.DisplayName);
        Assert.Equal(new[] { "StartOakValeWest", "StartOakValeEast" }, region.ContainsMaps);
        Assert.Equal(new[] { "StartOakVale_Filler_01" }, region.SeesMaps);
        Assert.Equal("StartOakVale", world.FindRegionContaining("StartOakValeWest")!.RegionName);
        Assert.Null(world.FindRegionContaining("LookoutPoint"));
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
    public void Lookout_aabb_neighbours_are_the_exe_static_maps()
    {
        var install = Require();
        var world = WorldFile.Load(install.WorldPath);
        var bwd = BwdFile.Load(install.BwdPath);
        var names = bwd.AdjacentTo("LookoutPoint").Select(r => r.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Contains("PicnicArea", names);
        Assert.Contains("BowerstoneBridge", names);
        Assert.Contains("Greatwood_1", names);
        Assert.Contains("Greatwood_2", names);
        Assert.Contains("GuildExterior", names);
        Assert.Contains("PicnicArea_Filler_02", names);
        Assert.Contains("PicnicArea_Filler_03", names);
        Assert.DoesNotContain("BowerstoneSlums", names);
        Assert.DoesNotContain("GreatwoodEntrance", names);

        var lookout = world.FindMap("LookoutPoint")!;
        Assert.True(lookout.LoadedOnPlayerProximity);
        foreach (var name in new[] { "PicnicArea", "BowerstoneBridge", "Greatwood_1", "Greatwood_2", "GuildExterior" })
            Assert.True(world.FindMap(name)!.LoadedOnPlayerProximity, name);
        foreach (var name in new[] { "PicnicArea_Filler_02", "PicnicArea_Filler_03" })
            Assert.False(world.FindMap(name)!.LoadedOnPlayerProximity, name);
    }

    [Fact]
    public void Nearby_guild_tng_is_lo_poly_active_guild_tng_is_high_poly_parts()
    {
        var install = Require();
        using var levels = new LevelLibrary(install);
        var world = WorldFile.Load(install.WorldPath);
        var near = world.FindMap("GuildExterior")!;
        var active = world.FindMap("HeroGuildComplexInside")!;
        Assert.True(near.LoadedOnPlayerProximity);
        Assert.True(active.LoadedOnPlayerProximity);
        Assert.True(near.MapX < active.MapX, $"near={near.MapX} active={active.MapX}");

        var exterior = levels.LoadThings("GuildExterior").Things
            .Select(t => t.DefinitionType)
            .Where(n => n is not null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var inside = levels.LoadThings("HeroGuildComplexInside").Things
            .Select(t => t.DefinitionType)
            .Where(n => n is not null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("BUILDING_GUILD_LO_POLY_01", exterior);
        Assert.DoesNotContain("BUILDING_GUILD_EXTERIOR_01", exterior);
        Assert.Contains("BUILDING_GUILD_EXTERIOR_01", inside);
        Assert.Contains("BUILDING_GUILD_EXTERIOR_02", inside);
        Assert.Contains("BUILDING_GUILD_EXTERIOR_05", inside);
        Assert.Contains("BUILDING_GUILD_EXTERIOR_06", inside);
        Assert.DoesNotContain("BUILDING_GUILD_LO_POLY_01", inside);
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
    public void Lookout_main_start_and_active_exits_follow_ctcd_region_exit()
    {
        var install = Require();
        using var levels = new LevelLibrary(install);
        var world = WorldFile.Load(install.WorldPath);
        Assert.Equal("LookoutPoint", world.Maps[0].ScriptName);

        var things = levels.LoadThings("LookoutPoint").Things.ToList();
        var start = RegionTravel.FindPlayerStart(things);
        Assert.NotNull(start);
        Assert.Equal("MAIN_START_POSITION", start.ScriptName);
        Assert.InRange(start.PositionX!.Value, 100f, 105f);
        Assert.InRange(start.PositionY!.Value, 72f, 76f);

        var exits = RegionTravel.ActiveExits(things);
        Assert.Equal(2, exits.Count);
        Assert.Contains(exits, e => world.Maps.Any(m => m.MapUid == e.Link.MapUid && m.ScriptName == "PicnicArea"));
        Assert.Contains(exits, e => world.Maps.Any(m => m.MapUid == e.Link.MapUid && m.ScriptName == "Greatwood_1"));
        Assert.All(exits, e => Assert.InRange(e.Radius, 2f, 6f));

        var picnicExit = exits.First(e => world.Maps.First(m => m.MapUid == e.Link.MapUid).ScriptName == "PicnicArea");
        Assert.NotNull(RegionTravel.HitExit(exits, picnicExit.Position));
        Assert.Null(RegionTravel.HitExit(exits, RegionTravel.PositionOf(start)));

        var picnicThings = levels.LoadThings("PicnicArea").Things.ToList();
        var entrance = RegionTravel.FindEntrance(picnicThings, picnicExit.Link);
        Assert.NotNull(entrance);
        Assert.InRange(entrance.PositionX!.Value, 75f, 85f);

        var back = RegionTravel.ActiveExits(picnicThings)
            .First(e => world.Maps.First(m => m.MapUid == e.Link.MapUid).ScriptName == "LookoutPoint");
        var backEntrance = RegionTravel.FindEntrance(things, back.Link);
        Assert.NotNull(backEntrance);
    }

    [Fact]
    public void New_game_starts_as_kid_in_start_oakvale_not_lookout()
    {
        var install = Require();
        using var levels = new LevelLibrary(install);
        var world = WorldFile.Load(install.WorldPath);
        var qst = QuestFile.Load(install.QuestPath);
        Assert.Equal("StartOakValeWest", RegionTravel.StartingRegion(world));
        Assert.Equal("LookoutPoint", world.Maps[0].ScriptName);
        Assert.Contains(qst.Quests, q => q.Name == "Q_NewOakValeIntro");
        Assert.Contains(qst.Quests, q => q.Name == "Q_NewOakValeIntro_PreAttack");
        Assert.Contains(qst.Quests, q => q.Name == "Q_GuildTraining");
        Assert.Contains(qst.Quests, q => q.Name == "Q_SunnyvaleMaster" && q.Persistent);

        var oak = world.FindRegionContaining("StartOakValeWest");
        Assert.NotNull(oak);
        Assert.Equal("StartOakVale", oak.RegionName);
        Assert.Equal(4, oak.Index);
        Assert.Equal("TXT_REGION_OAKVALE", oak.DisplayName);
        Assert.Equal("REGION_OAK_VALE_INTRO", oak.RegionDef);
        Assert.Contains("StartOakValeWest", oak.ContainsMaps);
        Assert.Contains("StartOakValeEast", oak.ContainsMaps);
        Assert.Contains("StartOakvaleMemorialGarden", oak.ContainsMaps);
        Assert.Equal(3, oak.ContainsMaps.Count);
        Assert.Contains("StartOakVale_Filler_01", oak.SeesMaps);
        Assert.Contains("StartOakVale_Sea_01", oak.SeesMaps);
        Assert.Equal(9, oak.SeesMaps.Count);

        var kid = world.FindMap("StartOakValeWest")!;
        Assert.Equal(203, kid.Index);
        Assert.Equal(3456, kid.MapX);
        Assert.Equal(736, kid.MapY);
        var east = world.FindMap("StartOakValeEast")!;
        var garden = world.FindMap("StartOakvaleMemorialGarden")!;
        Assert.True(east.MapX > kid.MapX);
        Assert.True(garden.MapY > kid.MapY);

        var things = levels.LoadThings("StartOakValeWest").Things.ToList();
        var start = RegionTravel.FindPlayerStart(things);
        Assert.NotNull(start);
        Assert.Equal("NOVStartHSP", start.ScriptName);
        Assert.InRange(start.PositionX!.Value, 33f, 36f);
        Assert.InRange(start.PositionY!.Value, 127f, 131f);
        Assert.True(RegionTravel.TryIntroCamera(things, out var introPos, out var introLook, out var introFov));
        Assert.InRange(introPos.X, 38f, 42f);
        Assert.InRange(introPos.Y, 128f, 132f);
        Assert.InRange(introFov, 65f, 80f);
        Assert.Equal(0.2f, LandscapeFrustum.FirstSeenFovTurns);
        Assert.False(LandscapeFrustum.FirstSeenTwoFovFlag);
        Assert.False(LandscapeFrustum.FirstSeenSplineEnabled);
        Assert.Equal(0x00B314E0u, LandscapeFrustum.CameraUpdate);
        Assert.Equal(0x00B31160u, LandscapeFrustum.SplineUpdate);
        Assert.Equal(0x00B31700u, LandscapeFrustum.CameraCtor);
        Assert.Equal(0x00B2FC10u, LandscapeFrustum.SplineEnable);
        Assert.Equal(0x00A0BE80u, LandscapeFrustum.FovFlagGetter);
        Assert.Equal(0x00A0BE90u, LandscapeFrustum.FovHGetter);
        Assert.Equal(2, LandscapeFrustum.TwoFovFlagBit);
        Assert.Equal(536, LandscapeFrustum.SplineFlagOffset);
        var shot = things.First(t => t.ScriptName == "CAM_OVIF_SHOT2");
        Assert.Equal("CAMERA_POINT_SCRIPTED_SPLINE", shot.DefinitionType);
        Assert.False(RegionTravel.IntroHeroIsSubject(shot));
        Assert.False(RegionTravel.FirstSeenHeroIsSubject);
        Assert.False(LandscapeFrustum.FirstSeenUsesThirdPersonView);
        Assert.Equal(new Vector3(0f, 0f, 1f), RegionTravel.IntroCameraUp(shot));
        Assert.Equal(LandscapeFrustum.FirstSeenCameraUp, RegionTravel.IntroCameraUp(shot));
        Assert.Equal(0x00B23B50u, LandscapeFrustum.BindSource);
        Assert.Equal(0x00B2FBF0u, LandscapeFrustum.StoreSource);
        Assert.Equal(12, LandscapeFrustum.HelperLookOffset);
        Assert.Equal(24, LandscapeFrustum.HelperUpOffset);
        Assert.Equal(0x0137F67Cu, LandscapeFrustum.ViewThirdPersonRtti);
        Assert.Equal("0.2", shot.Properties["CTCCameraPointScriptedSpline.FOV"]);
        Assert.Equal("0.2", shot.Properties["CTCCameraPointScriptedSpline.KeyCameras[0].FOV"]);
        Assert.False(shot.Properties.ContainsKey("CTCCameraPointScripted.FOV"));
        Assert.Equal(72f, LandscapeFrustum.FirstSeenFovTurns * LandscapeFrustum.FovTurnsToDegrees, 3);
        Assert.Equal(0.1f, LandscapeFrustum.FirstSeenNear);
        Assert.Equal(4000f, LandscapeFrustum.FirstSeenFar);
        Assert.Equal(0.1f, LandscapeFrustum.FirstSeenMinZ);
        Assert.Equal(0.99f, LandscapeFrustum.FirstSeenMaxZ);
        Assert.Equal(0x00988A50u, LandscapeFrustum.WvpFlush);
        Assert.Equal(5, LandscapeFrustum.LayoutWvpRegister);
        var shotCam = new FlyCamera { Position = introPos, FovDegrees = introFov };
        shotCam.LookAt(introLook);
        var shotNdc = FlyCamera.Project(shotCam.ViewProjection(4f / 3f), introLook);
        Assert.True(shotNdc.W > 0f, $"SHOT2 W={shotNdc.W}");
        Assert.InRange(shotNdc.X, -1f, 1f);
        Assert.InRange(shotNdc.Y, -1f, 1f);
        Assert.InRange(shotNdc.Z, LandscapeFrustum.FirstSeenMinZ, LandscapeFrustum.FirstSeenMaxZ);
        LandscapeFrustum.LetterboxCots(
            LandscapeFrustum.TurnsToRadians(LandscapeFrustum.FirstSeenFovTurns), 4f, 3f,
            out var introCotH, out var introCotV);
        var introC2 = LandscapeFrustum.InverseRow0(
            introPos, introLook - introPos, Vector3.UnitZ, introCotH, introCotV);
        Assert.True(float.IsFinite(introC2.X) && float.IsFinite(introC2.W), $"c2={introC2}");
        Assert.Equal(2, WorldShading.FogPlaneRegister);
        Assert.Equal(new Vector4(0f, 0f, 0f, 1f), WorldShading.FogRecordColor);
        Assert.Equal(1000f, WorldShading.FogStart);
        Assert.Equal(2000f, WorldShading.FogRecordEnd);
        Assert.Equal(1f, WorldShading.EvaluateVertexFog(0f, WorldShading.FirstSeenC0.Y, WorldShading.FogRecordColor.W));
        Assert.True(WorldShading.FirstSeenAppliesVertexFogBlend);
        Assert.Equal(1, D3dDeviceState.FirstSeenFogEnable);
        Assert.Equal(
            LandscapeFrustum.CotHalfAngle(float.DegreesToRadians(72f)),
            LandscapeFrustum.CotHalfAngle(LandscapeFrustum.TurnsToRadians(0.2f)), 4);
        Assert.True((introLook - introPos).Length() > 1f);
        LandscapeFrustum.LetterboxCots(
            float.DegreesToRadians(introFov), 4f, 3f, out var shotCotH, out var shotCotV);
        var shotPlanes = LandscapeFrustum.ExtractSidePlanes(
            introPos, introLook - introPos, Vector3.UnitZ, shotCotH, shotCotV);
        Assert.Equal(4, shotPlanes.Length);
        var oakHeight = levels.LoadHeightField("StartOakValeWest")!;
        LandscapeFrustum.PatchAabb(0f, 0f, oakHeight.FineWidth, oakHeight.FineHeight, out var oakMin, out var oakMax);
        Assert.Equal(0f, oakMin.Z);
        Assert.Equal(0f, oakMax.Z);
        Assert.False(LandscapeFrustum.AabbIsOutside(oakMin, oakMax, shotPlanes));
        LandscapeFrustum.PatchAabb(2000f, 2000f, 16f, 16f, out var farMin, out var farMax);
        Assert.True(LandscapeFrustum.AabbIsOutside(farMin, farMax, shotPlanes));
        var culled = WorldGeometry.Build(
            install, "StartOakValeWest", things, landscapePlanes: shotPlanes);
        Assert.Contains("StartOakValeWest", culled.Regions);
        Assert.Contains(culled.Triangles, t =>
            t.Layer == Fable.Formats.Meshes.SceneLayer.Landscape &&
            Math.Abs((t.A.X + t.B.X + t.C.X) / 3f - 34f) < 20f &&
            Math.Abs((t.A.Y + t.B.Y + t.C.Y) / 3f - 129f) < 20f);
        Assert.DoesNotContain(culled.Triangles, t =>
            t.Layer == Fable.Formats.Meshes.SceneLayer.Landscape &&
            (t.A.X + t.B.X + t.C.X) / 3f > 400f);
        Assert.Contains(things, t => t.ScriptName == "HerosOldHouse");
        var indoorLight = things.First(t =>
            t.DefinitionType == "MARKER_LIGHT" &&
            t.PositionX is not null &&
            Math.Abs(t.PositionX.Value - 33.91f) < 0.2f &&
            Math.Abs(t.PositionY!.Value - 131.55f) < 0.2f);
        Assert.Equal("TRUE", indoorLight.Properties["CTCLight.Active"]);
        Assert.Equal("CRGBColour(130,60,5,255)", indoorLight.Properties["CTCLight.Colour"]);
        Assert.Equal("8.0", indoorLight.Properties["CTCLight.InnerRadius"]);
        Assert.Equal("9.0", indoorLight.Properties["CTCLight.OuterRadius"]);
        Assert.True(WorldShading.QualifiesAsAddableLight(130f / 255f, 60f / 255f, 5f / 255f, 8f, 9f));
        Assert.Equal(0, WorldShading.SelectFamilySlot(WorldShading.FirstSeenPackedLightCount));
        Assert.Equal("VSHADER_STATIC_DIRLIGHT_FOG", WorldShading.StaticFamilyShader(WorldShading.FirstSeenPackedLightCount));
        Assert.Contains(things, t => t.DefinitionType == "GENERIC_INTERNAL_FIREPLACE");
        Assert.Contains(things, t => t.DefinitionType == "OBJECT_BUILDING_DOOR_3");
        Assert.Empty(RegionTravel.ActiveExits(things));

        var guild = levels.LoadThings("HeroGuildComplexInside").Things.ToList();
        Assert.Contains(guild, t => t.DefinitionType == "HOLY_SITE_PLAYER_START"
                                    && t.ScriptName == "GuildTrainingHSP");
        var lookout = levels.LoadThings("LookoutPoint").Things.ToList();
        Assert.Contains(lookout, t => t.DefinitionType == "HOLY_SITE_PLAYER_START"
                                      && t.ScriptName == "GuildArrivalHSP");
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
