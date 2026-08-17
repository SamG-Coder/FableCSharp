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
        Assert.Equal(0x00DBDE40u, RegionTravel.StartOakValeSetup);
        Assert.Equal(0x00CDD450u, RegionTravel.WatchBarrelsCtor);
        Assert.Equal(0x00DBE890u, RegionTravel.WatchBarrelsCallback);
        Assert.Equal(0x00DBE4E0u, RegionTravel.ManageQuestCoreMarkersCallback);
        Assert.False(RegionTravel.FirstSeenFollowsNoviLiveFather);
        Assert.Equal(0.1f, RegionTravel.WatchBarrelsInterval);
        Assert.Equal(64, RegionTravel.WatchBarrelsCapacity);
        Assert.Equal(12f, RegionTravel.PreAttackDuration);
        Assert.Equal(0x00DBE15Eu, RegionTravel.HerosOldHouseLookup);
        Assert.Equal("Q_NewOakValeIntro", RegionTravel.IntroQuest);
        Assert.Equal("S_QNOVI", RegionTravel.IntroScriptName);
        Assert.Equal(0x00DBEF70u, RegionTravel.IntroQuestFactory);
        Assert.Equal(0x00DAAC00u, RegionTravel.IntroQuestCtor);
        Assert.Equal(0x012D7A28u, RegionTravel.IntroQuestVtbl);
        Assert.Equal(0x10C, RegionTravel.IntroQuestSize);
        Assert.Equal(0x00DABAC0u, RegionTravel.IntroQuestRun);
        Assert.Equal(2, RegionTravel.IntroQuestRunSlot);
        Assert.Equal(0x00DAC295u, RegionTravel.IntroQuestRunCallsSetup);
        Assert.Equal(0x00DAACE0u, RegionTravel.IntroQuestMainWatcher);
        Assert.Equal(0x00CDD440u, RegionTravel.IntroMainWatcherCallback);
        Assert.Equal("Main", RegionTravel.IntroMainWatcherName);
        Assert.Equal(0x00DAADD0u, RegionTravel.IntroQuestReset);
        Assert.Equal(0x00B25950u, RegionTravel.RenderFrame);
        Assert.Equal(28, RegionTravel.ScriptYieldVtbl);
        Assert.Equal(64, RegionTravel.ScriptContextOffset);
        Assert.Equal(80, RegionTravel.PreAttackGateOffset);
        Assert.Equal(2584, RegionTravel.ScriptWaitVtbl);
        Assert.False(RegionTravel.FirstSeenPlus80WrittenInStartOakVale);
        Assert.True(RegionTravel.FirstSeenFadeOpcodeInStartOakVale);
        Assert.True(RegionTravel.FirstSeenPlayMusicDoesNotYield);
        Assert.Equal(0x00CBF7FEu, RegionTravel.PlayMusicHelper);
        Assert.Equal(0x00CC8EACu, RegionTravel.PlayMusicInterpreter);
        Assert.Equal(0x009E5120u, RegionTravel.PlayMusicLookup);
        Assert.Equal(2784, RegionTravel.PlayMusicVtbl);
        Assert.Equal(0x00CD17FDu, RegionTravel.CommandLoopContinue);
        Assert.Equal(0x00CC012Eu, RegionTravel.CommandLoopNext);
        Assert.Equal(0x00CD0987u, RegionTravel.FadeOutOpcode);
        Assert.Equal(1488, RegionTravel.FadeApplyVtbl);
        Assert.Equal(0x008907E0u, RegionTravel.FadeApplyFn);
        Assert.Equal(0x00434C00u, RegionTravel.FadeStateWrite);
        Assert.Equal(0x01260F0Cu, RegionTravel.FadeInterfaceVtbl);
        Assert.True(RegionTravel.FirstSeenFadeOutIsBlack);
        Assert.True(RegionTravel.FirstSeenFadeOverlayDraws);
        Assert.False(RegionTravel.FirstSeenFadeOverlayDrawUnread);
        Assert.Equal(0x006496BCu, RegionTravel.FadeOverlayDraw);
        Assert.Equal(0x004348D0u, RegionTravel.FadeOverlayAlphaFn);
        Assert.Equal(0x0041BEB0u, RegionTravel.FadeOverlayRecord);
        Assert.Equal(0x22u, RegionTravel.FadeOverlayRecordType);
        Assert.Equal(0xC0u, RegionTravel.FadeOverlaySubmit);
        Assert.Equal(92, RegionTravel.FadeOverlaySubmitVtbl);
        Assert.Equal(-8f, RegionTravel.FadeOverlaySizePad);
        Assert.False(RegionTravel.FirstSeenFadeSpecialCaseRuns);
        Assert.Equal("FadeOut 0.5,0", RegionTravel.FadeSpecialCase);
        Assert.Equal(0.5f, RegionTravel.FadeSpecialCaseSeconds);
        Assert.Equal(1488, RegionTravel.FadeSpecialCaseVtbl);
        Assert.Equal(0x00CCA26Eu, RegionTravel.PlayAviSite);
        Assert.Equal(0x00CCA26Du, RegionTravel.PlayAviOpcode);
        Assert.Equal(0x00CD17F8u, RegionTravel.PlayAviJoin);
        Assert.Equal(1476, RegionTravel.PlayAviVtbl);
        Assert.Equal(0x0088F890u, RegionTravel.PlayAviApplyFn);
        Assert.Equal(0x0040D2A0u, RegionTravel.PlayAviSingleton);
        Assert.Equal(0x006286F0u, RegionTravel.PlayAviPlayer);
        Assert.Equal(0x00A3B9D0u, RegionTravel.PlayAviOpen);
        Assert.Equal(0x0099C1E0u, RegionTravel.PlayAviRewrite);
        Assert.Equal(0x01258DE0u, RegionTravel.PlayAviExtXmvVa);
        Assert.Equal(0x01258DECu, RegionTravel.PlayAviExtWmvVa);
        Assert.Equal(0x1B, RegionTravel.PlayAviMode);
        Assert.True(RegionTravel.FirstSeenPlayAviDoesNotYield);
        Assert.True(RegionTravel.FirstSeenPlayAviIsBlocking);
        Assert.True(RegionTravel.FirstSeenPlayAviRewritesXmv);
        Assert.Equal(@"Data\Video\", RegionTravel.PlayAviPrefix);
        Assert.Equal("dream_sequence_comp.xmv", RegionTravel.IntroPlayAvi);
        Assert.Equal("dream_sequence_comp.wmv", RegionTravel.IntroPlayAviRewritten);
        Assert.Equal(1, RegionTravel.PlayAviSkipEscape);
        Assert.Equal(57, RegionTravel.PlayAviSkipSpace);
        Assert.Equal(28, RegionTravel.PlayAviSkipReturn);
        Assert.Equal(62, RegionTravel.PlayAviSkipF4);
        Assert.Equal(0x009DC870u, RegionTravel.PlayAviBlit);
        Assert.Equal(0x009D9C80u, RegionTravel.PlayAviFlush);
        Assert.Equal(0.5f, RegionTravel.PlayAviLetterboxHalf);
        Assert.Equal(0x012AB174u, RegionTravel.PlayAviFilterGraphClsidVa);
        Assert.Equal(0x012A9934u, RegionTravel.PlayAviGraphBuilderIidVa);
        Assert.Equal(0x00A3B510u, RegionTravel.PlayAviRendererCtor);
        Assert.Equal(0x00A3B5F0u, RegionTravel.PlayAviCheckMediaType);
        Assert.Equal(0x00A3B590u, RegionTravel.PlayAviAcceptMediaType);
        Assert.Equal(0x00CA84C0u, RegionTravel.PlayAviQueryAccept);
        Assert.Equal(0x00CA8420u, RegionTravel.PlayAviQueryPinInfo);
        Assert.Equal(0x00CA4780u, RegionTravel.PlayAviLeftoverQi);
        Assert.Equal(0x00CA6080u, RegionTravel.PlayAviRendererQi);
        Assert.Equal(new Guid("1bd0ecb0-f8e2-11ce-aac6-0020af0b99a3"), RegionTravel.PlayAviIOverlayIid);
        Assert.True(RegionTravel.FirstSeenPlayAviRendererExposesMediaPosition);
        Assert.Equal(new Guid("256a6a22-fbad-11d1-82bf-00a0c9696c8f"), RegionTravel.PlayAviIPinConnectionIid);
        Assert.Equal(new Guid("56a8689d-0ad4-11ce-b03a-0020af0ba770"), RegionTravel.PlayAviMemInputPinIid);
        Assert.Equal(new Guid("71771540-2017-11cf-ae26-0020afd79767"), RegionTravel.PlayAviRendererClsid);
        Assert.Equal(0x0129D150u, RegionTravel.PlayAviRendererClsidVa);
        Assert.Equal(0x00CA84F0u, RegionTravel.PlayAviGetMediaType);
        Assert.Equal(0x8000FFFFu, RegionTravel.PlayAviGetMediaTypeUnexpected);
        Assert.Equal(0x00CA7CE0u, RegionTravel.PlayAviIPinQi);
        Assert.Equal(12u, RegionTravel.PlayAviIPinAdjust);
        Assert.Equal(0x012BF3C0u, RegionTravel.PlayAviIPinVtbl);
        Assert.Equal(0x00CA4F40u, RegionTravel.PlayAviPinCtor);
        Assert.Equal(0x00CA6A30u, RegionTravel.PlayAviGetPin);
        Assert.Equal(0xE0u, RegionTravel.PlayAviPinObjectSize);
        Assert.Equal(new Guid("56a86891-0ad4-11ce-b03a-0020af0ba770"), RegionTravel.PlayAviIPinIid);
        Assert.Equal(new Guid("e436eb7d-524f-11ce-9f53-0020af0ba770"), RegionTravel.PlayAviRgb24);
        Assert.Equal(new Guid("05589f80-c356-11ce-bf01-00aa0055595a"), RegionTravel.PlayAviFormatVideoInfo);
        Assert.True(RegionTravel.FirstSeenPlayAviQueryAcceptRequiresRgb24);
        Assert.True(RegionTravel.FirstSeenPlayAviEnumMediaTypesEmpty);
        Assert.True(RegionTravel.PlayAviQuartzDestEnumMustYieldRgb24);
        Assert.True(RegionTravel.FirstSeenPlayAviIPinIsSeparateObject);
        Assert.False(RegionTravel.FirstSeenPlayAviAdvertisesRgb32);
        Assert.Equal(new Guid("73646976-0000-0010-8000-00aa00389b71"), RegionTravel.PlayAviMediaTypeVideo);
        Assert.Equal(0, RegionTravel.PlayAviQueryAcceptHr(
            RegionTravel.PlayAviMediaTypeVideo,
            RegionTravel.PlayAviRgb24,
            RegionTravel.PlayAviFormatVideoInfo));
        Assert.Equal(1, RegionTravel.PlayAviQueryAcceptHr(
            RegionTravel.PlayAviMediaTypeVideo,
            new Guid("e436eb7e-524f-11ce-9f53-0020af0ba770"),
            RegionTravel.PlayAviFormatVideoInfo));
        Assert.Equal(1, RegionTravel.PlayAviQueryAcceptHr(
            RegionTravel.PlayAviMediaTypeVideo,
            RegionTravel.PlayAviRgb24,
            Guid.Empty));
        Assert.Equal(0x00A3B130u, RegionTravel.PlayAviRun);
        Assert.Equal(0x00A3BCF0u, RegionTravel.PlayAviDoRenderSample);
        Assert.Equal(50, RegionTravel.PlayAviRunRetry);
        Assert.Equal(1, RegionTravel.PlayAviEcComplete);
        Assert.Equal(21, RegionTravel.PlayAviTextureFormatArgb);
        Assert.Equal(new Guid("e436ebb3-524f-11ce-9f53-0020af0ba770"), RegionTravel.PlayAviFilterGraphClsid);
        Assert.Equal(new Guid("56a868a9-0ad4-11ce-b03a-0020af0ba770"), RegionTravel.PlayAviGraphBuilderIid);
        Assert.True(RegionTravel.FirstSeenPlayAviIsDirectShow);
        Assert.False(RegionTravel.FirstSeenPlayAviIsMediaFoundation);
        Assert.False(RegionTravel.FirstSeenPlayAviUsesVideoWindow);
        Assert.False(RegionTravel.FirstSeenPlayAviUsesGetCurrentImage);
        Assert.True(RegionTravel.FirstSeenPlayAviCopiesRgb24ToArgb);
        Assert.Equal(33, RegionTravel.PlayAviPresentMs);
        Assert.Equal(0x00A3B730u, RegionTravel.PlayAviCopySample);
        Assert.Equal(172, RegionTravel.PlayAviCopyVtbl);
        Assert.Equal("Fable Texture Renderer Filter", RegionTravel.PlayAviFilterName);
        Assert.Equal(new Guid("56a868b1-0ad4-11ce-b03a-0020af0ba770"), RegionTravel.PlayAviMediaControlIid);
        Assert.Equal(new Guid("56a868b6-0ad4-11ce-b03a-0020af0ba770"), RegionTravel.PlayAviMediaEventIid);
        Assert.Equal(0x8004022Au, RegionTravel.PlayAviTypeNotAccepted);
        Assert.True(RegionTravel.FirstSeenPlayAviCheckMediaTypeReadsVih);
        Assert.Equal(1, RegionTravel.PlayAviFilterMiscIsRenderer);
        Assert.Equal(0x00CA68F0u, RegionTravel.PlayAviConnectedTo);
        Assert.Equal(0x80040209u, RegionTravel.PlayAviNotConnected);
        Assert.True(RegionTravel.FirstSeenPlayAviDraws);
        Assert.True(RegionTravel.FirstSeenPlayAviLetterbox);
        Assert.Equal(0x00CC9E6Au, RegionTravel.NoLoadUseCameraSite);
        Assert.Equal(0x00CC9E69u, RegionTravel.NoLoadUseCameraOpcode);
        Assert.Equal(0x00CC9F39u, RegionTravel.UseCameraOpcode);
        Assert.Equal(0x00CCA22Cu, RegionTravel.UseCameraYield);
        Assert.Equal(0x00CBFD53u, RegionTravel.UseCameraYieldFlagWrite);
        Assert.True(RegionTravel.FirstSeenUseCameraYields);
        Assert.True(RegionTravel.FirstSeenNoLoadUseCameraYields);
        Assert.False(RegionTravel.FirstSeenPlayAvi);
        Assert.Equal("Hero", RegionTravel.IntroHeroActor);
        Assert.Equal("Father", RegionTravel.IntroFatherActor);
        Assert.Equal("MK_OVI_ID_HERO", RegionTravel.IntroHeroTeleportMarker);
        Assert.Equal("MK_OVI_ID_DAD", RegionTravel.IntroFatherTeleportMarker);
        Assert.Equal(0x0089B780u, RegionTravel.TeleportApplyFn);
        Assert.Equal(0x004AA980u, RegionTravel.TeleportMarkerPos);
        Assert.Equal(124, RegionTravel.TeleportSetPosVtbl);
        Assert.True(RegionTravel.FirstSeenTeleportAppliesPos);
        Assert.True(RegionTravel.FirstSeenTeleportReadsYaw);
        Assert.False(RegionTravel.FirstSeenTeleportAppliesYaw);
        Assert.Equal(0x0089BDF0u, RegionTravel.TeleportHeadingApply);
        Assert.Equal(1896, RegionTravel.TeleportHeadingVtbl);
        Assert.Equal(264, RegionTravel.TeleportSetYawVtbl);
        Assert.False(RegionTravel.FirstSeenWatchBarrelsSpawnsBeetle);
        Assert.False(RegionTravel.FirstSeenHandsPlayerControl);
        Assert.False(RegionTravel.FirstSeenCameraNameInExe);
        Assert.Equal("NOVI_Barrel", RegionTravel.WatchBarrelsThing);
        Assert.Equal(0x00CBF29Fu, RegionTravel.ScriptCameraHooks);
        Assert.Equal(0x00CBF3ACu, RegionTravel.ScriptUseCameraToken);
        Assert.Equal(0x00CBF3FEu, RegionTravel.ScriptCameraLookAtToken);
        Assert.Equal(0x00CC14B9u, RegionTravel.ScriptPlayAnimationToken);
        Assert.Equal(0x00CC14B8u, RegionTravel.PlayAnimationOpcode);
        Assert.Equal(0x00CC1527u, RegionTravel.PlayAnimationApply);
        Assert.Equal(0x00CC186Fu, RegionTravel.PlayAnimationYieldJoin);
        Assert.Equal(0x00CC5691u, RegionTravel.PlayAnimationYieldOnce);
        Assert.Equal(0x00CC0EBCu, RegionTravel.PlayAnimationLeftover);
        Assert.Equal(0x00CBFD57u, RegionTravel.PlayAnimationYieldAfterWrite);
        Assert.Equal(72, RegionTravel.PlayAnimationApplyVtbl);
        Assert.Equal(0x004C7470u, RegionTravel.PlayAnimationThingFn);
        Assert.Equal(68, RegionTravel.PlayAnimationComponentVtbl);
        Assert.Equal(0x01375748u, RegionTravel.PlayAnimationFlagByte);
        Assert.Equal(0x01010101u, RegionTravel.PlayAnimationFlagByteDword);
        Assert.Equal(0x012650A4u, RegionTravel.AnimationComplexVtbl);
        Assert.Equal(0x0070B3F0u, RegionTravel.AnimationComplexFactory);
        Assert.Equal(0x00686920u, RegionTravel.AnimationComplexPlus68);
        Assert.Equal(90, RegionTravel.AnimationComplexTypeId);
        Assert.Equal(0x0070D580u, RegionTravel.AnimationPlayInner);
        Assert.True(RegionTravel.FirstSeenPlayAnimationYields);
        Assert.False(RegionTravel.FirstSeenPlayAnimationAppliesPose);
        Assert.False(RegionTravel.FirstSeenPlayAnimationCallsInnerPlay);
        Assert.False(WorldShading.FirstSeenPlaysAnim);
        Assert.Equal("CS_WAKING_UP_LOOP", RegionTravel.IntroWakeLoop);
        Assert.Equal("CS_WAKING_UP_ON_STEPS", RegionTravel.IntroWakeSteps);
        Assert.Equal("CS_TIRED", RegionTravel.IntroTired);
        Assert.Equal(5.2f, RegionTravel.IntroGamePauseAfterShot2);
        Assert.Equal(0x00CD1373u, RegionTravel.StartTimeCodeOpcode);
        Assert.Equal(0x00CD13C3u, RegionTravel.StartTimeCodeApply);
        Assert.Equal(0x00CD17FDu, RegionTravel.StartTimeCodeJoin);
        Assert.Equal(0x013B83C8u, RegionTravel.StartTimeCodeGlobal);
        Assert.True(RegionTravel.FirstSeenStartTimeCodeDoesNotYield);
        Assert.Equal("CAM_OVI_ID_STANDUP", RegionTravel.IntroStandupCamera);
        Assert.Equal(0x00CC88D1u, RegionTravel.GamePauseOpcode);
        Assert.Equal(0x0099E690u, RegionTravel.GamePauseAtoi);
        Assert.Equal(15f, RegionTravel.GamePauseScale);
        Assert.Equal(1f, RegionTravel.GamePauseIncrement);
        Assert.False(RegionTravel.FirstSeenGamePauseHasClockArg);
        Assert.False(RegionTravel.FirstSeenGamePauseUsesFrameDt);
        Assert.Equal(1.6f, RegionTravel.IntroGamePauseSeconds);
        Assert.Equal(0x00CC25FDu, RegionTravel.SpeakOpcode);
        Assert.Equal(0x00CC27EAu, RegionTravel.SpeakApply);
        Assert.Equal(0x00CC2909u, RegionTravel.SpeakPoll);
        Assert.Equal(0x00CBEE5Eu, RegionTravel.SpeakIsNull);
        Assert.Equal(0x0127293Cu, RegionTravel.SpeakThingVtbl);
        Assert.Equal(0x004CD1B0u, RegionTravel.SpeakApplyStub);
        Assert.Equal(0x00661A40u, RegionTravel.SpeakPollStub);
        Assert.Equal(52, RegionTravel.SpeakApplyVtbl);
        Assert.Equal(104, RegionTravel.SpeakPollVtbl);
        Assert.True(RegionTravel.FirstSeenSpeakYieldsOnce);
        Assert.Equal("TEXT_QST_048_FATHER_INTRO_10", RegionTravel.IntroFatherSpeak);
        Assert.Equal(0x00CC2EAAu, RegionTravel.InteractiveSpeakOpcode);
        Assert.Equal(1456, RegionTravel.InteractiveSpeakBeginVtbl);
        Assert.Equal(1460, RegionTravel.InteractiveSpeakBindVtbl);
        Assert.Equal(1464, RegionTravel.InteractiveSpeakLineVtbl);
        Assert.False(RegionTravel.FirstSeenInteractiveSpeakArgIsTrue);
        Assert.True(RegionTravel.FirstSeenInteractiveSpeakYieldsOnce);
        Assert.Equal("TEXT_QST_048_FATHER_INTRO_20", RegionTravel.IntroFatherPrompt);
        Assert.Equal("TEXT_QST_048_FATHER_INTRO_30", RegionTravel.IntroFatherResponse);
        Assert.Equal(0x00CC3165u, RegionTravel.DialogSpeakOpcode);
        Assert.Equal(0x008906C0u, RegionTravel.DialogSpeakBeginFn);
        Assert.Equal(0x008907D0u, RegionTravel.DialogSpeakWaitFn);
        Assert.True(RegionTravel.FirstSeenDialogSpeakYieldsOnce);
        Assert.Equal("TEXT_QST_048_FATHER_INTRO_60", RegionTravel.IntroFatherDialog);
        Assert.Equal("HERO", RegionTravel.IntroDialogListener);
        Assert.Equal(2.0f, RegionTravel.IntroGamePauseAfterTired);
        Assert.Equal(0x00CC0783u, RegionTravel.WaitTaskOpcode);
        Assert.Equal(104, RegionTravel.WaitTaskPollVtbl);
        Assert.Equal(0x006A9550u, RegionTravel.WaitTaskHeroPoll);
        Assert.True(RegionTravel.FirstSeenWaitTaskYieldsOnce);
        Assert.False(RegionTravel.FirstSeenWaitTaskReadsName);
        Assert.Equal("FOO", RegionTravel.IntroWaitTask);
        Assert.Equal(0x00CC0CB5u, RegionTravel.SneakToOpcode);
        Assert.Equal(20, RegionTravel.SneakToApplyVtbl);
        Assert.Equal(0x004C72B0u, RegionTravel.SneakToApplyStub);
        Assert.Equal(2, RegionTravel.SneakToMode);
        Assert.False(RegionTravel.FirstSeenSneakToAppliesMove);
        Assert.False(RegionTravel.FirstSeenSneakToWaitsForArrival);
        Assert.Equal("MK_OVIF_HERO4", RegionTravel.IntroSneakMarker);
        Assert.Equal(0x00CC0F1Au, RegionTravel.SneakToWaitPoll);
        Assert.Equal(104, RegionTravel.SneakToWaitPollVtbl);
        Assert.True(RegionTravel.FirstSeenSneakToTruePollsArrival);
        Assert.True(RegionTravel.FirstSeenSneakToTrueYieldsOnce);
        Assert.Equal("MK_OVIF_HERO5", RegionTravel.IntroSneakWaitMarker);
        Assert.Equal("Hero.SneakTo MK_OVIF_HERO5,0.0,TRUE", RegionTravel.IntroCutsceneLastCommand);
        Assert.Equal(72, RegionTravel.IntroCutsceneVector1Offset);
        Assert.Equal(7, RegionTravel.IntroCutsceneVector1Count);
        Assert.Equal(0x00CC017Cu, RegionTravel.CutsceneVector1Copy);
        Assert.Equal(0x00CBEB7Eu, RegionTravel.CutsceneSkipPredicate);
        Assert.False(RegionTravel.FirstSeenCutsceneVector1AutoRuns);
        Assert.False(RegionTravel.FirstSeenCutsceneSkipFires);
        Assert.Equal(0f, RegionTravel.IntroSneakSpeed);
        Assert.Equal(0x00CC15E3u, RegionTravel.PlayCombatAnimationOpcode);
        Assert.Equal(76, RegionTravel.PlayCombatAnimationApplyVtbl);
        Assert.Equal(0x00834760u, RegionTravel.PlayCombatAnimationFatherFn);
        Assert.True(RegionTravel.FirstSeenPlayCombatAnimationYields);
        Assert.False(RegionTravel.FirstSeenPlayCombatAnimationAppliesPose);
        Assert.Equal("TURNING_AC90", RegionTravel.IntroFatherCombatAnim);
        Assert.Equal(0x00CCC246u, RegionTravel.CreateOpcode);
        Assert.Equal(364, RegionTravel.CreateApplyVtbl);
        Assert.Equal(0x008A9100u, RegionTravel.CreateApplyFn);
        Assert.True(RegionTravel.FirstSeenCreateDoesNotYield);
        Assert.Equal("CREATURE_OAKVALE_VILLAGER_FEMALE_NORMAL_MESH", RegionTravel.IntroCreateType);
        Assert.Equal("MK_OVI_ID_VS1", RegionTravel.IntroCreateMarker);
        Assert.Equal("VILL1", RegionTravel.IntroCreateName);
        Assert.Equal(0x00CC083Du, RegionTravel.WalkToOpcode);
        Assert.Equal(20, RegionTravel.WalkToApplyVtbl);
        Assert.Equal(0x004C72B0u, RegionTravel.WalkToApplyStub);
        Assert.Equal(0, RegionTravel.WalkToMode);
        Assert.False(RegionTravel.FirstSeenWalkToAppliesMove);
        Assert.False(RegionTravel.FirstSeenWalkToWaitsForArrival);
        Assert.Equal("MK_OVI_ID_VW1", RegionTravel.IntroWalkMarker);
        Assert.Equal(0.3f, RegionTravel.IntroWalkSpeed);
        Assert.Equal(0x00CC656Bu, RegionTravel.WaitActiveDialogOpcode);
        Assert.Equal(1472, RegionTravel.WaitActiveDialogPollVtbl);
        Assert.True(RegionTravel.FirstSeenWaitActiveDialogYieldsOnce);
        Assert.Equal(0x00CD0116u, RegionTravel.RemoveOpcode);
        Assert.Equal(432, RegionTravel.RemoveApplyVtbl);
        Assert.Equal(0x008910D0u, RegionTravel.RemoveApplyFn);
        Assert.True(RegionTravel.FirstSeenRemoveDoesNotYield);
        Assert.Equal("VILL1", RegionTravel.IntroRemoveName);
        Assert.Equal(0x00CC3354u, RegionTravel.DialogadSpeakOpcode);
        Assert.Equal(0x00CD3187u, RegionTravel.DialogadSpeakTable);
        Assert.Equal(0x00CC2C6Bu, RegionTravel.DialogadSpeakMissJoin);
        Assert.Equal(52, RegionTravel.DialogadSpeakApplyVtbl);
        Assert.Equal(0x004CD1B0u, RegionTravel.DialogadSpeakApplyStub);
        Assert.True(RegionTravel.FirstSeenDialogadSpeakDoesNotYield);
        Assert.False(RegionTravel.FirstSeenDialogadSpeakAppliesUi);
        Assert.Equal("TEXT_QST_048_FATHER_INTRO_100", RegionTravel.IntroFatherDialogAd);
        Assert.Equal("Father", RegionTravel.IntroDialogAdTarget);
        Assert.Equal(0x00CC3F73u, RegionTravel.LookInDirectionOpcode);
        Assert.Equal(1896, RegionTravel.LookInDirectionApplyVtbl);
        Assert.Equal(0x0089BDF0u, RegionTravel.LookInDirectionApplyFn);
        Assert.Equal(1f / 360f, RegionTravel.LookInDirectionScale);
        Assert.True(RegionTravel.FirstSeenLookInDirectionDoesNotYield);
        Assert.False(RegionTravel.FirstSeenLookInDirectionAppliesHeading);
        Assert.Equal(215f, RegionTravel.IntroLookInDirectionDegrees);
        Assert.Equal(0x00CC4B22u, RegionTravel.ScriptFadeInOut);
        Assert.Equal(0x00CB5D80u, RegionTravel.RegisteringScripts);
        Assert.Equal(0x00CB8110u, RegionTravel.QuestBaseCtor);
        Assert.Equal(0x012C1648u, RegionTravel.QuestBaseVtbl);
        Assert.False(RegionTravel.FirstSeenCallsUseCamera);
        Assert.False(RegionTravel.FirstSeenCallsPlayAnimationDispatcher);
        Assert.False(RegionTravel.FirstSeenScriptBinHasSqnovi);
        Assert.Equal("CS_OAKVALE_INTRO_FATHER", RegionTravel.IntroCutscene);
        Assert.Equal(0x00DB86B0u, RegionTravel.IntroCutsceneStart);
        Assert.Equal(0x00DB8680u, RegionTravel.IntroCutsceneDtor);
        Assert.Equal(0x00CBFB7Du, RegionTravel.IntroCutsceneRunner);
        Assert.Equal(0x00CC9F3Au, RegionTravel.UseCameraActivate);
        Assert.Equal(1648, RegionTravel.UseCameraPreloadVtbl);
        Assert.Equal(1656, RegionTravel.UseCameraActivateVtbl);
        Assert.Equal(0x012D838Cu, RegionTravel.IntroCutsceneCallbackTable);
        Assert.Equal(0x012D95B0u, RegionTravel.IntroCutsceneMicrothreadVtbl);
        Assert.True(RegionTravel.FirstSeenStartsIntroCutscene);
        Assert.Equal("NOVI_LiveFather", RegionTravel.LiveFatherScript);
        Assert.Equal("CREATURE_HERO_FATHER", RegionTravel.LiveFatherCreature);
        Assert.Equal(0x00DAC2C0u, RegionTravel.LiveFatherFactory);
        Assert.Equal(0x012D8388u, RegionTravel.LiveFatherVtbl);
        Assert.Equal(0x012D8370u, RegionTravel.NoviNameRecordVtbl);
        Assert.Equal(0x00CB8230u, RegionTravel.NoviNameRegister);
        Assert.Equal(0x00DB8520u, RegionTravel.NoviNameRecordCreate);
        Assert.Equal(0x004C97B0u, RegionTravel.ThingConstructBind);
        Assert.Equal(0x004C7CF0u, RegionTravel.ThingScriptActivate);
        Assert.Equal(0x00CB8960u, RegionTravel.ConstructNameBind);
        Assert.Equal("CAM_OVIF_SHOT2", RegionTravel.IntroFirstSeenCamera);
        Assert.Equal(0x00DB86B0u, ScriptedCamera.CutsceneStart);
        Assert.Equal(0x00CBFB7Du, ScriptedCamera.CutsceneRunner);
        Assert.Equal(1648, ScriptedCamera.PreloadVtbl);
        Assert.Equal(1656, ScriptedCamera.ActivateVtbl);
        Assert.Equal(0x00CB6EA0u, NewGameScript.ListWalk);
        Assert.Equal(0x00CB70E0u, NewGameScript.ListInvoke);
        Assert.Equal(0x00CB6CE0u, NewGameScript.PerItem);
        Assert.Equal(24, NewGameScript.ListRecordBytes);
        Assert.Equal(0x0143E8F8u, NewGameScript.ContextGlobal);
        Assert.Equal(76, NewGameScript.WaitFlagPtrOffset);
        Assert.Equal(2592u, NewGameScript.WaitFlagVtbl);
        Assert.Equal(0x00A44880u, NewGameScript.UpdateFn);
        Assert.Equal(0x00A446A0u, NewGameScript.FiberEntry);
        Assert.Equal(0x00A44660u, NewGameScript.ResumeFn);
        Assert.Equal(0x00A44690u, NewGameScript.YieldFn);
        Assert.Equal(0x009E1BC0u, NewGameScript.FrameDt);
        Assert.Equal(0x00DAADA0u, NewGameScript.PersistAttackOver);
        Assert.Equal(0x004045C0u, NewGameScript.PersistHelper);
        Assert.Equal("AttackOver", NewGameScript.PersistAttackOverName);
        Assert.Equal(8, NewGameScript.DtOffset);
        Assert.Equal(16, NewGameScript.FiberSetupVtbl);
        Assert.Equal(8, NewGameScript.FiberRunVtbl);
        Assert.Equal(NewGameScript.LiveFatherScript, RegionTravel.LiveFatherScript);
        Assert.Equal(0x00DAC2C0u, NewGameScript.LiveFatherFactory);
        Assert.Equal(0x00B23B50u, LandscapeFrustum.BindSource);
        Assert.Equal(2, LandscapeFrustum.CameraUpdateCallerCount);
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
        Assert.Contains(things, t =>
            t.DefinitionType == RegionTravel.LiveFatherCreature
            && t.ScriptName == RegionTravel.LiveFatherScript);
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
        Assert.Equal(1, LandscapeFrustum.BindCameraUpdateArg);
        Assert.Equal(0x00B2FC50u, LandscapeFrustum.ExtractOtherWritesView);
        Assert.Equal(128u, LandscapeFrustum.ViewSourceOffset);
        Assert.Equal(84, LandscapeFrustum.CameraWorldXOffset);
        LandscapeFrustum.LandscapeWorld3x4(introPos, out _, out _, out _, out var worldT);
        Assert.Equal(introPos, worldT);
        var scripted = new ScriptedCamera();
        Assert.True(scripted.UseCamera(things, RegionTravel.IntroFirstSeenCamera));
        Assert.Equal(RegionTravel.IntroFirstSeenCamera, scripted.ActiveName);
        Assert.Equal(introPos, scripted.Position);
        Assert.Equal(introLook, scripted.LookAt);
        Assert.Equal(introFov, scripted.FovDegrees);
        var debugCam = new FlyCamera { Position = scripted.Position, FovDegrees = scripted.FovDegrees };
        debugCam.LookAt(scripted.LookAt);
        debugCam.Move(Vector3.UnitY, 1f, fast: true);
        debugCam.Look(12f, -4f);
        Assert.Equal(introPos, scripted.Position);
        Assert.Equal(introLook, scripted.LookAt);
        Assert.Equal(RegionTravel.IntroFirstSeenCamera, scripted.ActiveName);
        var shotCam = new FlyCamera { Position = introPos, FovDegrees = introFov };
        shotCam.LookAt(introLook);
        var shotNdc = FlyCamera.Project(shotCam.ViewProjection(4f / 3f), introLook);
        var scriptNdc = FlyCamera.Project(scripted.ViewProjection(4f / 3f), introLook);
        Assert.InRange(scriptNdc.X, -1f, 1f);
        Assert.InRange(MathF.Abs(scriptNdc.X - shotNdc.X), 0f, 1e-4f);
        Assert.True(shotNdc.W != 0f, $"SHOT2 W={shotNdc.W}");
        Assert.InRange(shotNdc.X, -1f, 1f);
        Assert.False(LandscapeFrustum.FirstSeenViewUsesCreateLookAt);
        LandscapeFrustum.HelperViewAxes(introLook - introPos, Vector3.UnitZ, out var shotRight, out _, out _);
        Assert.True(shotRight.LengthSquared() > 0.5f);
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
        Assert.True(WorldShading.FirstSeenFogC2IsLinearViewZ);
        Assert.Equal(1f, WorldShading.EvaluateWorldFog(
            introPos, introPos, introLook - introPos), 3);
        var houseNearCam = introPos + (introLook - introPos) * 8f;
        Assert.Equal(1f, WorldShading.EvaluateWorldFog(
            houseNearCam, introPos, introLook - introPos), 3);
        Assert.True(
            WorldShading.WorldDotFogPlane(houseNearCam, introC2) > 1f,
            $"SHOT2 InverseRow0 would black the house dp={WorldShading.WorldDotFogPlane(houseNearCam, introC2)}");
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
    public void New_game_intro_runs_through_generic_script_runtime()
    {
        var install = Require();
        using var levels = new LevelLibrary(install);
        var things = levels.LoadThings("StartOakValeWest").Things.ToList();
        var camera = new ScriptedCamera();
        Assert.True(camera.UseCamera(things, RegionTravel.IntroFirstSeenCamera));

        var runtime = ScriptRuntime.StartNewGame(install, things, camera);
        var script = new NewGameScript(runtime);
        var intro = runtime.FindInterpreter(RegionTravel.IntroCutscene);
        Assert.NotNull(intro);
        Assert.Equal(RegionTravel.IntroCutscene, intro.Name);
        Assert.True(script.CutsceneStarted);
        Assert.False(script.FadeSpecialCaseApplied);
        Assert.True(script.PlayMusicRan);
        Assert.True(script.FadeOutReached);
        Assert.Equal(0.5f, script.FadeDuration);
        Assert.Equal(0f, script.FadeParam);
        Assert.True(runtime.FadeActive);
        Assert.True(runtime.FadeLocked);
        Assert.Equal((byte)0, runtime.FadeColor.R);
        Assert.Equal((byte)0, runtime.FadeColor.G);
        Assert.Equal((byte)0, runtime.FadeColor.B);
        Assert.Equal((byte)255, runtime.FadeColor.A);
        Assert.True(RegionTravel.FirstSeenFadeOutIsBlack);
        Assert.Equal(RegionTravel.IntroPlayMusic, intro.Executed[0]);
        Assert.Contains(RegionTravel.FadeSpecialCase, intro.Executed);
        Assert.Contains(intro.Executed, line => line.StartsWith("CameraPause", StringComparison.Ordinal));
        Assert.Contains("Hero.Teleport MK_OVI_ID_HERO,FALSE", intro.Executed);
        Assert.Contains("Father.Teleport MK_OVI_ID_DAD", intro.Executed);
        Assert.Contains("Father.LookToThing Hero,FOREVER", intro.Executed);
        Assert.True(RegionTravel.FirstSeenTeleportDoesNotYield);
        Assert.True(RegionTravel.FirstSeenTeleportAppliesPos);
        Assert.True(RegionTravel.FirstSeenTeleportReadsYaw);
        Assert.False(RegionTravel.FirstSeenTeleportAppliesYaw);
        Assert.False(RegionTravel.FirstSeenTeleportChangesRegion);
        Assert.True(RegionTravel.FirstSeenLookToThingYields);
        Assert.Equal(0x00CC4678u, RegionTravel.TeleportOpcode);
        Assert.Equal(0x0089B780u, RegionTravel.TeleportApplyFn);
        Assert.Equal(0x004AA980u, RegionTravel.TeleportMarkerPos);
        Assert.Equal(124, RegionTravel.TeleportSetPosVtbl);
        Assert.Equal(0x00CC3B3Fu, RegionTravel.LookToThingOpcode);
        Assert.True(intro.Yielded);
        Assert.False(intro.ExecutedVerb("UseCamera"));
        Assert.False(intro.ExecutedVerb("PlayAVI"));
        Assert.Equal("DoScriptFrame 1", intro.Commands[intro.InstructionPointer]);
        Assert.Null(intro.UnsupportedCommand);
        Assert.False(intro.ExecutedVerb("DoScriptFrame"));
        Assert.Equal(0x00CC7085u, RegionTravel.DoScriptFrameOpcode);
        Assert.Equal(1, RegionTravel.DoScriptFrameDefaultCount);
        Assert.True(RegionTravel.FirstSeenDoScriptFrameYieldsPerCount);
        Assert.Equal(1, ScriptInterpreter.ParseScriptFrameCount("1"));
        Assert.Equal(0, ScriptInterpreter.ParseScriptFrameCount("0"));
        Assert.Equal(1, ScriptInterpreter.ParseScriptFrameCount(""));
        Assert.Equal(RegionTravel.IntroFirstSeenCamera, camera.ActiveName);
        Assert.Contains(runtime.Teleports, t => t.Actor == "Hero" && t.Marker == "MK_OVI_ID_HERO");
        Assert.Contains(runtime.Teleports, t => t.Actor == "Father" && t.Marker == "MK_OVI_ID_DAD");
        Assert.True(runtime.ActorPositions.TryGetValue(RegionTravel.IntroHeroActor, out var heroPos));
        Assert.True(runtime.ActorPositions.TryGetValue(RegionTravel.IntroFatherActor, out var fatherPos));
        var heroMarker = things.First(t => t.ScriptName == RegionTravel.IntroHeroTeleportMarker);
        var dadMarker = things.First(t => t.ScriptName == RegionTravel.IntroFatherTeleportMarker);
        Assert.Equal(RegionTravel.PositionOf(heroMarker), heroPos);
        Assert.Equal(RegionTravel.PositionOf(dadMarker), fatherPos);
        var spawn = RegionTravel.FindPlayerStart(things);
        Assert.NotNull(spawn);
        Assert.NotEqual(RegionTravel.PositionOf(spawn), heroPos);
        Assert.Equal("MUSIC_SET_NULL", runtime.LastMusic);
        Assert.False(script.Gate80);
        script.Update(0.1f);
        Assert.Equal(0.1f, script.DtAtPlus8);
        Assert.False(script.Gate80);
        Assert.Equal("DoScriptFrame 1", intro.Commands[intro.InstructionPointer]);
        Assert.True(intro.Yielded);
        Assert.False(intro.ExecutedVerb("DoScriptFrame"));
        script.Update(0.1f);
        Assert.Contains("DoScriptFrame 1", intro.Executed);
        Assert.Contains("DoCameraPreloading", intro.Executed);
        Assert.True(RegionTravel.FirstSeenDoCameraPreloadingDoesNotYield);
        Assert.False(RegionTravel.FirstSeenDoCameraPreloadingHasTrueArg);
        Assert.Equal(0x00CC86D0u, RegionTravel.DoCameraPreloadingOpcode);
        Assert.Equal(0x00CBEDBAu, RegionTravel.IsTrueArgFn);
        Assert.Equal("DoScriptFrame 1", intro.Commands[intro.InstructionPointer]);
        Assert.Null(intro.UnsupportedCommand);
        Assert.Contains("CAM_OVIF_SHOT2", runtime.PreloadedCameras);
        Assert.Equal(RegionTravel.IntroFirstSeenCamera, camera.ActiveName);
        Assert.False(intro.ExecutedVerb("UseCamera"));
        Assert.False(intro.ExecutedVerb("PlayAVI"));
        script.Update(0.1f);
        Assert.Contains("PlayAVI dream_sequence_comp.xmv", intro.Executed);
        Assert.Equal(@"Data\Video\dream_sequence_comp.xmv", runtime.LastAvi);
        Assert.Equal(@"Data\Video\dream_sequence_comp.wmv", runtime.AviRelativePath);
        Assert.True(runtime.AviPlaying);
        Assert.NotNull(runtime.AviFile);
        Assert.True(File.Exists(runtime.AviFile));
        Assert.True(RegionTravel.FileHasAsfMagic(runtime.AviFile));
        Assert.True(RegionTravel.FirstSeenPlayAviDoesNotYield);
        Assert.True(RegionTravel.FirstSeenPlayAviIsBlocking);
        Assert.False(RegionTravel.FirstSeenPlayAvi);
        Assert.False(intro.ExecutedVerb("MuteSounds"));
        runtime.SkipAvi();
        script.Update(0.1f);
        Assert.Contains("MuteSounds false", intro.Executed);
        Assert.False(runtime.SoundsMuted);
        Assert.Equal(0x00CC7258u, RegionTravel.MuteSoundsOpcode);
        Assert.Equal(2664, RegionTravel.MuteSoundsVtbl);
        Assert.True(RegionTravel.FirstSeenMuteSoundsDoesNotYield);
        Assert.True(RegionTravel.FirstSeenMuteSoundsArgIsFalse);
        Assert.Equal("DoScriptFrame 2", intro.Commands[intro.InstructionPointer]);
        Assert.Null(intro.UnsupportedCommand);
        Assert.False(intro.ExecutedVerb("PlayAnimation"));
        Assert.Equal(RegionTravel.IntroFirstSeenCamera, camera.ActiveName);
        var wakeFlags = ScriptCommand.ParsePlayAnimationFlags(
            "CS_WAKING_UP_LOOP,FALSE,FALSE,TRUE,FALSE");
        Assert.False(wakeFlags.Flag1);
        Assert.False(wakeFlags.Flag2);
        Assert.True(wakeFlags.Flag3);
        Assert.False(wakeFlags.Flag4);
        Assert.False(wakeFlags.Flag5);
        Assert.True(ScriptCommand.ParsePlayAnimationFlags("CS_TIRED").Flag4);
        Assert.True(RegionTravel.FirstSeenPlayAnimationYields);
        Assert.Equal(0x00CC14B8u, RegionTravel.PlayAnimationOpcode);
        Assert.Equal(72, RegionTravel.PlayAnimationApplyVtbl);
        script.Update(0.1f);
        Assert.Equal("DoScriptFrame 2", intro.Commands[intro.InstructionPointer]);
        Assert.False(intro.ExecutedVerb("PlayAnimation"));
        script.Update(0.1f);
        Assert.Contains("Hero.PlayAnimation CS_WAKING_UP_LOOP,FALSE,FALSE,TRUE,FALSE", intro.Executed);
        Assert.DoesNotContain(intro.Executed, line => line.Contains("CS_WAKING_UP_ON_STEPS", StringComparison.Ordinal));
        Assert.Equal("Hero.PlayAnimation CS_WAKING_UP_ON_STEPS,FALSE,FALSE,TRUE,FALSE", intro.Commands[intro.InstructionPointer]);
        Assert.True(intro.Yielded);
        Assert.Null(intro.UnsupportedCommand);
        Assert.Contains(runtime.Animations, a =>
            a.Actor == "Hero" && a.Name == RegionTravel.IntroWakeLoop &&
            !a.Flag1 && !a.Flag2 && a.Flag3 && !a.Flag4 && !a.Flag5);
        script.Update(0.1f);
        Assert.Contains("Hero.PlayAnimation CS_WAKING_UP_ON_STEPS,FALSE,FALSE,TRUE,FALSE", intro.Executed);
        Assert.Equal("DoScriptFrame 4", intro.Commands[intro.InstructionPointer]);
        Assert.Contains(runtime.Animations, a =>
            a.Actor == "Hero" && a.Name == RegionTravel.IntroWakeSteps &&
            !a.Flag1 && !a.Flag2 && a.Flag3 && !a.Flag4 && !a.Flag5);
        Assert.Equal(2, runtime.Animations.Count);
        Assert.Null(intro.UnsupportedCommand);
        Assert.False(intro.ExecutedVerb("UseCamera"));
        Assert.False(intro.ExecutedVerb("StartTimeCode"));
        Assert.Equal(RegionTravel.IntroFirstSeenCamera, camera.ActiveName);
        script.Update(0.1f);
        script.Update(0.1f);
        script.Update(0.1f);
        script.Update(0.1f);
        Assert.Equal("DoScriptFrame 4", intro.Commands[intro.InstructionPointer]);
        Assert.False(intro.ExecutedVerb("StartTimeCode"));
        script.Update(0.1f);
        Assert.Contains("DoScriptFrame 4", intro.Executed);
        Assert.Contains("StartTimeCode", intro.Executed);
        Assert.Equal(0, runtime.TimeCode);
        Assert.True(RegionTravel.FirstSeenStartTimeCodeDoesNotYield);
        Assert.Equal(0x00CD1373u, RegionTravel.StartTimeCodeOpcode);
        Assert.Equal(0x013B83C8u, RegionTravel.StartTimeCodeGlobal);
        Assert.Contains("PlayMusic MUSIC_SET_OAKVALE", intro.Executed);
        Assert.Equal("MUSIC_SET_OAKVALE", runtime.LastMusic);
        Assert.Contains("NoLoadUseCamera CAM_OVI_ID_STANDUP", intro.Executed);
        Assert.Equal(RegionTravel.IntroStandupCamera, camera.ActiveName);
        Assert.True(RegionTravel.FirstSeenNoLoadUseCameraYields);
        Assert.Equal("FadeIn", intro.Commands[intro.InstructionPointer]);
        Assert.False(intro.ExecutedVerb("FadeIn"));
        script.Update(0.1f);
        Assert.Contains(intro.Executed, line => line.Equals("FadeIn", StringComparison.Ordinal));
        Assert.Equal("GamePause 1.6", intro.Commands[intro.InstructionPointer]);
        Assert.Null(intro.UnsupportedCommand);
        Assert.True(intro.Yielded);
        Assert.False(intro.ExecutedVerb("GamePause"));
        Assert.Equal(1.6f * RegionTravel.GamePauseScale, intro.GamePauseTarget);
        Assert.Equal(0f, intro.GamePauseCounter);
        Assert.Equal(0x00CC88D1u, RegionTravel.GamePauseOpcode);
        Assert.Equal(15f, RegionTravel.GamePauseScale);
        Assert.False(RegionTravel.FirstSeenGamePauseUsesFrameDt);
        Assert.False(intro.ExecutedVerb("Speak"));
        var pauseVisits = 1;
        while (intro.Yielded &&
               intro.Commands[intro.InstructionPointer].StartsWith("GamePause 1.6", StringComparison.Ordinal) &&
               pauseVisits < 40)
        {
            script.Update(0.1f);
            pauseVisits++;
        }

        Assert.Contains("GamePause 1.6", intro.Executed);
        Assert.Contains("Father.Speak Father,'TEXT_QST_048_FATHER_INTRO_10'", intro.Executed);
        Assert.Equal("GamePause 1.0", intro.Commands[intro.InstructionPointer]);
        Assert.True(intro.Yielded);
        Assert.Null(intro.UnsupportedCommand);
        Assert.True(RegionTravel.FirstSeenSpeakYieldsOnce);
        Assert.Equal(0x00CC25FDu, RegionTravel.SpeakOpcode);
        Assert.Contains(runtime.Speeches, s =>
            s.Actor == "Father" &&
            s.Target == "Father" &&
            s.Text.Contains(RegionTravel.IntroFatherSpeak, StringComparison.Ordinal) &&
            s.Mode == 0);
        Assert.Equal("GamePause 1.0", intro.Commands[intro.InstructionPointer]);
        var pause1Visits = 0;
        while (intro.Yielded &&
               intro.Commands[intro.InstructionPointer].StartsWith("GamePause 1.0", StringComparison.Ordinal) &&
               pause1Visits < 40)
        {
            script.Update(0.1f);
            pause1Visits++;
        }

        Assert.Contains("GamePause 1.0", intro.Executed);
        Assert.Contains(
            "Father.InteractiveSpeak Hero,'TEXT_QST_048_FATHER_INTRO_20',FALSE,'TEXT_QST_048_FATHER_INTRO_30'",
            intro.Executed);
        Assert.Equal("GamePause 1.2", intro.Commands[intro.InstructionPointer]);
        Assert.True(intro.Yielded);
        Assert.Null(intro.UnsupportedCommand);
        Assert.True(RegionTravel.FirstSeenInteractiveSpeakYieldsOnce);
        Assert.Contains(runtime.InteractiveSpeeches, s =>
            s.Actor == "Father" &&
            s.Listener == "Hero" &&
            s.Prompt.Contains(RegionTravel.IntroFatherPrompt, StringComparison.Ordinal) &&
            !s.Wait &&
            s.Response.Contains(RegionTravel.IntroFatherResponse, StringComparison.Ordinal));
        var pause12Visits = 0;
        while (intro.Yielded &&
               intro.Commands[intro.InstructionPointer].StartsWith("GamePause 1.2", StringComparison.Ordinal) &&
               pause12Visits < 40)
        {
            script.Update(0.1f);
            pause12Visits++;
        }

        Assert.Contains("GamePause 1.2", intro.Executed);
        Assert.Contains("UseCamera CAM_OVIF_SHOT2", intro.Executed);
        Assert.Equal(RegionTravel.IntroFirstSeenCamera, camera.ActiveName);
        Assert.Equal("GamePause 5.2", intro.Commands[intro.InstructionPointer]);
        Assert.True(intro.Yielded);
        Assert.Null(intro.UnsupportedCommand);
        Assert.True(RegionTravel.FirstSeenUseCameraYields);
        Assert.Equal(0x00CC9F39u, RegionTravel.UseCameraOpcode);
        Assert.False(RegionTravel.FirstSeenPlayAnimationAppliesPose);
        Assert.False(RegionTravel.FirstSeenPlayAnimationCallsInnerPlay);
        Assert.False(WorldShading.FirstSeenPlaysAnim);
        Assert.Equal(0x004C7470u, RegionTravel.PlayAnimationThingFn);
        Assert.Equal(0x00686920u, RegionTravel.AnimationComplexPlus68);
        Assert.Equal(0x0070D580u, RegionTravel.AnimationPlayInner);
        Assert.DoesNotContain(runtime.Animations, a => a.Name == RegionTravel.IntroTired);
        var pause52Visits = 0;
        while (intro.Yielded &&
               intro.Commands[intro.InstructionPointer].StartsWith("GamePause 5.2", StringComparison.Ordinal) &&
               pause52Visits < 100)
        {
            script.Update(0.1f);
            pause52Visits++;
        }

        Assert.Contains("GamePause 5.2", intro.Executed);
        Assert.Contains("Hero.PlayAnimation CS_TIRED", intro.Executed);
        Assert.Equal("GamePause 2.0", intro.Commands[intro.InstructionPointer]);
        Assert.True(intro.Yielded);
        Assert.Null(intro.UnsupportedCommand);
        Assert.Contains(runtime.Animations, a =>
            a.Actor == "Hero" && a.Name == RegionTravel.IntroTired && a.Flag4);
        Assert.False(WorldShading.FirstSeenPlaysAnim);
        Assert.False(intro.ExecutedVerb("DialogSpeak"));
        Assert.Equal(0x00CC3165u, RegionTravel.DialogSpeakOpcode);
        Assert.True(RegionTravel.FirstSeenDialogSpeakYieldsOnce);
        Assert.Equal(2.0f, RegionTravel.IntroGamePauseAfterTired);
        var pause20Visits = 0;
        while (intro.Yielded &&
               intro.Commands[intro.InstructionPointer].StartsWith("GamePause 2.0", StringComparison.Ordinal) &&
               pause20Visits < 50)
        {
            script.Update(0.1f);
            pause20Visits++;
        }

        Assert.Contains("GamePause 2.0", intro.Executed);
        Assert.Contains("Father.DialogSpeak HERO,'TEXT_QST_048_FATHER_INTRO_60'", intro.Executed);
        Assert.Equal("Hero.WaitTask FOO", intro.Commands[intro.InstructionPointer]);
        Assert.True(intro.Yielded);
        Assert.Null(intro.UnsupportedCommand);
        Assert.Contains(runtime.DialogSpeeches, s =>
            s.Actor == "Father" &&
            s.Listener.Equals(RegionTravel.IntroDialogListener, StringComparison.OrdinalIgnoreCase) &&
            s.Text.Contains(RegionTravel.IntroFatherDialog, StringComparison.Ordinal));
        Assert.False(intro.ExecutedVerb("WaitTask"));
        Assert.Equal(0x00CC0783u, RegionTravel.WaitTaskOpcode);
        Assert.True(RegionTravel.FirstSeenWaitTaskYieldsOnce);
        Assert.False(RegionTravel.FirstSeenWaitTaskReadsName);
        script.Update(0.1f);
        Assert.Contains("Hero.WaitTask FOO", intro.Executed);
        Assert.Equal("Hero.SneakTo MK_OVIF_HERO4,0.0,FALSE,FALSE,FALSE", intro.Commands[intro.InstructionPointer]);
        Assert.True(intro.Yielded);
        Assert.Null(intro.UnsupportedCommand);
        Assert.Contains(runtime.WaitTasks, w =>
            w.Actor == "Hero" && w.Name == RegionTravel.IntroWaitTask);
        Assert.False(intro.ExecutedVerb("SneakTo"));
        Assert.Equal(0x00CC0CB5u, RegionTravel.SneakToOpcode);
        Assert.False(RegionTravel.FirstSeenSneakToAppliesMove);
        Assert.False(RegionTravel.FirstSeenSneakToWaitsForArrival);
        script.Update(0.1f);
        Assert.Contains("Hero.SneakTo MK_OVIF_HERO4,0.0,FALSE,FALSE,FALSE", intro.Executed);
        Assert.Equal("GamePause 1.0", intro.Commands[intro.InstructionPointer]);
        Assert.True(intro.Yielded);
        Assert.Null(intro.UnsupportedCommand);
        Assert.Contains(runtime.SneakTos, s =>
            s.Actor == "Hero" &&
            s.Marker == RegionTravel.IntroSneakMarker &&
            s.Speed == RegionTravel.IntroSneakSpeed &&
            !s.Wait);
        Assert.False(intro.ExecutedVerb("PlayCombatAnimation"));
        Assert.Equal(0x00CC15E3u, RegionTravel.PlayCombatAnimationOpcode);
        Assert.False(RegionTravel.FirstSeenPlayCombatAnimationAppliesPose);
        Assert.True(RegionTravel.FirstSeenPlayCombatAnimationYields);
        var pauseAfterSneak = 0;
        while (intro.Yielded &&
               !intro.ExecutedVerb("PlayCombatAnimation") &&
               intro.Commands[intro.InstructionPointer].StartsWith("GamePause 1.0", StringComparison.Ordinal) &&
               pauseAfterSneak < 50)
        {
            script.Update(0.1f);
            pauseAfterSneak++;
        }

        Assert.Contains("GamePause 1.0", intro.Executed);
        Assert.Contains("Father.PlayCombatAnimation TURNING_AC90,FALSE,TRUE", intro.Executed);
        Assert.Equal("GamePause 1.0", intro.Commands[intro.InstructionPointer]);
        Assert.True(intro.Yielded);
        Assert.Null(intro.UnsupportedCommand);
        Assert.Contains(runtime.CombatAnimations, a =>
            a.Actor == "Father" &&
            a.Name == RegionTravel.IntroFatherCombatAnim &&
            a.FlagA && a.FlagB && !a.FlagC && a.FlagD && !a.FlagE && a.Count == 1);
        Assert.False(WorldShading.FirstSeenPlaysAnim);
        Assert.False(intro.ExecutedVerb("PlayCombatAnim"));
        var pauseAfterCombat = 0;
        while (intro.Yielded &&
               !intro.Executed.Any(line => line.Contains("CS_LOOK_LEFT", StringComparison.Ordinal)) &&
               intro.Commands[intro.InstructionPointer].StartsWith("GamePause 1.0", StringComparison.Ordinal) &&
               pauseAfterCombat < 50)
        {
            script.Update(0.1f);
            pauseAfterCombat++;
        }

        Assert.Contains("Hero.PlayAnimation CS_LOOK_LEFT,TRUE", intro.Executed);
        Assert.Equal(
            "Create CREATURE_OAKVALE_VILLAGER_FEMALE_NORMAL_MESH,MK_OVI_ID_VS1,VILL1",
            intro.Commands[intro.InstructionPointer]);
        Assert.True(intro.Yielded);
        Assert.Null(intro.UnsupportedCommand);
        Assert.Contains(runtime.Animations, a =>
            a.Actor == "Hero" && a.Name == "CS_LOOK_LEFT" && a.Flag1 && !a.Flag2);
        Assert.False(intro.ExecutedVerb("Create"));
        Assert.Equal(0x00CCC246u, RegionTravel.CreateOpcode);
        Assert.True(RegionTravel.FirstSeenCreateDoesNotYield);
        script.Update(0.1f);
        Assert.Contains(
            "Create CREATURE_OAKVALE_VILLAGER_FEMALE_NORMAL_MESH,MK_OVI_ID_VS1,VILL1",
            intro.Executed);
        Assert.Contains("VILL1.WalkTo MK_OVI_ID_VW1", intro.Executed);
        Assert.Equal("GamePause 0.8", intro.Commands[intro.InstructionPointer]);
        Assert.Null(intro.UnsupportedCommand);
        Assert.True(intro.Yielded);
        Assert.Contains(runtime.Creates, c =>
            c.Type == RegionTravel.IntroCreateType &&
            c.Marker == RegionTravel.IntroCreateMarker &&
            c.Name == RegionTravel.IntroCreateName);
        Assert.Contains(runtime.WalkTos, w =>
            w.Actor == "VILL1" &&
            w.Marker == RegionTravel.IntroWalkMarker &&
            w.Speed == RegionTravel.IntroWalkSpeed &&
            !w.Wait);
        Assert.False(RegionTravel.FirstSeenWalkToAppliesMove);
        Assert.False(intro.ExecutedVerb("WaitActiveDialog"));
        Assert.Equal(0x00CC656Bu, RegionTravel.WaitActiveDialogOpcode);
        Assert.True(RegionTravel.FirstSeenWaitActiveDialogYieldsOnce);
        var pause08 = 0;
        while (intro.Yielded &&
               !intro.ExecutedVerb("WaitActiveDialog") &&
               intro.Commands[intro.InstructionPointer].StartsWith("GamePause 0.8", StringComparison.Ordinal) &&
               pause08 < 50)
        {
            script.Update(0.1f);
            pause08++;
        }

        Assert.Contains("WaitActiveDialog", intro.Executed);
        Assert.Equal("UseCamera CAM_OVIF_SHOT3", intro.Commands[intro.InstructionPointer]);
        Assert.True(intro.Yielded);
        Assert.Null(intro.UnsupportedCommand);
        Assert.Equal(1, runtime.WaitActiveDialogCount);
        Assert.False(intro.ExecutedVerb("Remove"));
        Assert.Equal(0x00CD0116u, RegionTravel.RemoveOpcode);
        Assert.True(RegionTravel.FirstSeenRemoveDoesNotYield);
        script.Update(0.1f);
        Assert.Contains("UseCamera CAM_OVIF_SHOT3", intro.Executed);
        Assert.Equal("Hero.Teleport MK_OVIF_HERO4", intro.Commands[intro.InstructionPointer]);
        Assert.True(intro.Yielded);
        script.Update(0.1f);
        Assert.Contains("Hero.Teleport MK_OVIF_HERO4", intro.Executed);
        Assert.Contains("Hero.PlayAnimation ST_IDLE_SUBTLE,FALSE,TRUE", intro.Executed);
        Assert.Equal("Father.Speak Father,'TEXT_QST_048_FATHER_INTRO_70'", intro.Commands[intro.InstructionPointer]);
        Assert.True(intro.Yielded);
        script.Update(0.1f);
        Assert.Contains("Father.Speak Father,'TEXT_QST_048_FATHER_INTRO_70'", intro.Executed);
        Assert.Equal("NoLoadUseCamera CAM_OVIF_SHOT4", intro.Commands[intro.InstructionPointer]);
        script.Update(0.1f);
        Assert.Contains("NoLoadUseCamera CAM_OVIF_SHOT4", intro.Executed);
        Assert.Equal("Remove VILL1", intro.Commands[intro.InstructionPointer]);
        Assert.False(intro.ExecutedVerb("Remove"));
        script.Update(0.1f);
        Assert.Contains("Remove VILL1", intro.Executed);
        Assert.Contains("Father.Speak Father,'TEXT_QST_048_FATHER_INTRO_80'", intro.Executed);
        Assert.Equal("NoLoadUseCamera CAM_OVIF_SHOT3", intro.Commands[intro.InstructionPointer]);
        Assert.True(intro.Yielded);
        Assert.Null(intro.UnsupportedCommand);
        Assert.Contains(runtime.Removes, name => name == RegionTravel.IntroRemoveName);
        Assert.False(intro.ExecutedVerb("DialogadSpeak"));
        Assert.Equal(0x00CC3354u, RegionTravel.DialogadSpeakOpcode);
        Assert.True(RegionTravel.FirstSeenDialogadSpeakDoesNotYield);
        Assert.False(RegionTravel.FirstSeenDialogadSpeakAppliesUi);
        script.Update(0.1f);
        Assert.Contains("NoLoadUseCamera CAM_OVIF_SHOT3", intro.Executed);
        Assert.Equal("Father.Speak Father,'TEXT_QST_048_FATHER_INTRO_90'", intro.Commands[intro.InstructionPointer]);
        script.Update(0.1f);
        Assert.Contains("Father.Speak Father,'TEXT_QST_048_FATHER_INTRO_90'", intro.Executed);
        Assert.Equal("GamePause 0.2", intro.Commands[intro.InstructionPointer]);
        var pause02 = 0;
        while (intro.Yielded &&
               !intro.ExecutedVerb("DialogadSpeak") &&
               pause02 < 80)
        {
            script.Update(0.1f);
            pause02++;
        }

        Assert.Contains("GamePause 0.2", intro.Executed);
        Assert.Contains("NoLoadUseCamera CAM_OVIF_SHOT6START", intro.Executed);
        Assert.Contains("Father.LookToThing Hero,FALSE", intro.Executed);
        Assert.Contains("Father.DialogadSpeak Father,'TEXT_QST_048_FATHER_INTRO_100'", intro.Executed);
        Assert.Equal("GamePause 0.5", intro.Commands[intro.InstructionPointer]);
        Assert.True(intro.Yielded);
        Assert.Null(intro.UnsupportedCommand);
        Assert.Contains(runtime.DialogAdSpeeches, s =>
            s.Actor == "Father" &&
            s.Target.Equals(RegionTravel.IntroDialogAdTarget, StringComparison.OrdinalIgnoreCase) &&
            s.Text.Contains(RegionTravel.IntroFatherDialogAd, StringComparison.Ordinal) &&
            s.Mode == 0);
        Assert.False(intro.ExecutedVerb("LookInDirection"));
        Assert.Equal(0x00CC3F73u, RegionTravel.LookInDirectionOpcode);
        Assert.True(RegionTravel.FirstSeenLookInDirectionDoesNotYield);
        Assert.False(RegionTravel.FirstSeenLookInDirectionAppliesHeading);
        var pause05 = 0;
        while (intro.Yielded &&
               !intro.ExecutedVerb("LookInDirection") &&
               pause05 < 80)
        {
            script.Update(0.1f);
            pause05++;
        }

        Assert.Contains("GamePause 0.5", intro.Executed);
        Assert.Contains("NoLoadUseCamera CAM_OVIF_SHOT6", intro.Executed);
        Assert.Contains("Hero.SneakTo MK_OVIF_HERO5,0.0,FALSE,FALSE,FALSE", intro.Executed);
        Assert.Contains("GamePause 1.1", intro.Executed);
        Assert.Contains("Father.LookInDirection 215", intro.Executed);
        Assert.Contains("UseCamera CAM_OVIF_SHOT7", intro.Executed);
        Assert.Equal("Hero.SneakTo MK_OVIF_HERO5,0.0,TRUE", intro.Commands[intro.InstructionPointer]);
        Assert.True(intro.Yielded);
        Assert.Null(intro.UnsupportedCommand);
        Assert.Contains(runtime.LookInDirections, l =>
            l.Actor == "Father" &&
            l.Degrees == RegionTravel.IntroLookInDirectionDegrees &&
            l.Flag);
        Assert.True(RegionTravel.FirstSeenSneakToTrueYieldsOnce);
        var sneakWaitAt = intro.Commands.ToList().IndexOf("Hero.SneakTo MK_OVIF_HERO5,0.0,TRUE");
        Assert.True(sneakWaitAt >= 0);
        script.Update(0.1f);
        Assert.Contains("Hero.SneakTo MK_OVIF_HERO5,0.0,TRUE", intro.Executed);
        Assert.True(intro.Yielded);
        Assert.Null(intro.UnsupportedCommand);
        if (sneakWaitAt + 1 < intro.Commands.Count)
            Assert.Equal("FadeOut", intro.Commands[intro.InstructionPointer]);
        Assert.Contains(runtime.SneakTos, s =>
            s.Actor == "Hero" &&
            s.Marker == RegionTravel.IntroSneakWaitMarker &&
            s.Speed == 0f &&
            s.Wait);
        Assert.False(RegionTravel.FirstSeenSneakToAppliesMove);
        Assert.False(intro.SkipListApplied);
        Assert.DoesNotContain("Hero.Teleport MK_OVIF_HERO5", intro.Executed);
        Assert.DoesNotContain("FadeOut", intro.Executed);
        script.ApplyPersist(true);
        Assert.True(script.Gate80);

        var attract = runtime.Bank!.Find("CS_ATTRACT_1") ?? runtime.Bank.Find("CS_ATTRACT_12");
        Assert.NotNull(attract);
        var other = runtime.StartCutscene(attract.InstanceName);
        Assert.NotNull(other);
        Assert.NotEqual(RegionTravel.IntroCutscene, other.Name);
        Assert.Equal(attract.Commands[0], other.Commands[0]);
        Assert.Same(intro, runtime.FindInterpreter(RegionTravel.IntroCutscene));
    }

    [Fact]
    public void PlayAnimation_vtbl72_does_not_apply_pose()
    {
        Assert.Equal(0x004C7470u, RegionTravel.PlayAnimationThingFn);
        Assert.Equal(72, RegionTravel.PlayAnimationApplyVtbl);
        Assert.Equal(68, RegionTravel.PlayAnimationComponentVtbl);
        Assert.Equal(0x012650A4u, RegionTravel.AnimationComplexVtbl);
        Assert.Equal(0x0070B3F0u, RegionTravel.AnimationComplexFactory);
        Assert.Equal(0x00686920u, RegionTravel.AnimationComplexPlus68);
        Assert.Equal(0x0070B3C0u, RegionTravel.AnimationComplexTypeIdFn);
        Assert.Equal(90, RegionTravel.AnimationComplexTypeId);
        Assert.Equal(0x0070E710u, RegionTravel.AnimationComplexInnerCtor);
        Assert.Equal(0xBC, RegionTravel.AnimationComplexInnerSize);
        Assert.Equal(0x0070B460u, RegionTravel.AnimationComplexInnerGetter);
        Assert.Equal(0x0070D580u, RegionTravel.AnimationPlayInner);
        Assert.Equal(0x0070C050u, RegionTravel.AnimationPlayRequest);
        Assert.Equal(0x0070B600u, RegionTravel.AnimationComplexPostAttach);
        Assert.False(RegionTravel.FirstSeenPlayAnimationAppliesPose);
        Assert.False(RegionTravel.FirstSeenPlayAnimationCallsInnerPlay);
        Assert.False(WorldShading.FirstSeenPlaysAnim);
        Assert.Equal("CS_TIRED", RegionTravel.IntroTired);
        var tired = ScriptCommand.ParsePlayAnimationFlags(RegionTravel.IntroTired);
        Assert.True(tired.Flag4);
        Assert.False(tired.Flag1);
    }

    [Fact]
    public void GamePause_1_6_waits_scaled_frames_not_dt()
    {
        var interpreter = new ScriptInterpreter("pause", ["GamePause 1.6", "PlayMusic X"]);
        interpreter.RunUntilYield();
        Assert.True(interpreter.Yielded);
        Assert.Equal("GamePause 1.6", interpreter.Commands[interpreter.InstructionPointer]);
        Assert.Equal(24f, interpreter.GamePauseTarget);
        Assert.Equal(0f, interpreter.GamePauseCounter);
        var visits = 1;
        while (interpreter.Yielded && visits < 40)
        {
            interpreter.Resume();
            visits++;
        }

        Assert.Equal(26, visits);
        Assert.Contains("GamePause 1.6", interpreter.Executed);
        Assert.Contains("PlayMusic X", interpreter.Executed);
        Assert.True(interpreter.Finished);
        Assert.False(RegionTravel.FirstSeenGamePauseUsesFrameDt);
    }

    [Fact]
    public void Father_Speak_first_seen_yields_once_then_continues()
    {
        var interpreter = new ScriptInterpreter("speak",
        [
            "Father.Speak Father,'TEXT_QST_048_FATHER_INTRO_10'",
            "GamePause 1.0",
        ]);
        var host = new SpeakProbe();
        interpreter.RunUntilYield(host);
        Assert.True(interpreter.Yielded);
        Assert.Equal("GamePause 1.0", interpreter.Commands[interpreter.InstructionPointer]);
        Assert.Contains("Father.Speak Father,'TEXT_QST_048_FATHER_INTRO_10'", interpreter.Executed);
        Assert.Equal("Father", host.Last.Actor);
        Assert.Equal("Father", host.Last.Target);
        Assert.Contains("TEXT_QST_048_FATHER_INTRO_10", host.Last.Text);
        Assert.Equal(0, host.Last.Mode);
        Assert.True(RegionTravel.FirstSeenSpeakYieldsOnce);
        Assert.Equal(ScriptFlow.Continue, ScriptCommand.Classify(
            ScriptCommand.Parse("Father.Speak Father,null")));
    }

    private sealed class SpeakProbe : IScriptHost
    {
        public ScriptSpeech Last;

        public void PlayMusic(string track) { }
        public void FadeOut(float seconds, float param) { }
        public void FadeIn(float seconds, float param) { }
        public void UseCamera(string name) { }
        public void NoLoadUseCamera(string name) { }
        public void PlayAnimation(string? actor, string arguments) { }
        public void CameraPause(string arguments) { }
        public void Teleport(string? actor, string arguments) { }
        public void LookToThing(string? actor, string arguments) { }
        public void DoCameraPreloading(string arguments) { }
        public void PlayAvi(string arguments) { }
        public void MuteSounds(string arguments) { }
        public void StartTimeCode() { }
        public void GamePause(float seconds) { }

        public void Speak(string? actor, string target, string text, int mode) =>
            Last = new ScriptSpeech(actor, target, text, mode);

        public void InteractiveSpeak(
            string? actor, string listener, string prompt, bool wait, string response) { }

        public void DialogSpeak(string? actor, string listener, string text) { }

        public void WaitTask(string? actor, string name) { }

        public void SneakTo(string? actor, string marker, float speed, bool wait) { }

        public void PlayCombatAnimation(
            string? actor, string name, bool flagA, bool flagB, bool flagC, bool flagD, bool flagE, int count) { }

        public void Create(string type, string marker, string name) { }

        public void WalkTo(string? actor, string marker, float speed, bool wait) { }

        public void WaitActiveDialog() { }

        public void Remove(string name) { }

        public void DialogadSpeak(string? actor, string target, string text, int mode) { }

        public void LookInDirection(string? actor, float degrees, bool flag) { }
    }

    [Fact]
    public void Intro_vector1_copies_only_on_skip_predicate()
    {
        var install = Require();
        var bank = ScriptBank.Load(install);
        var father = bank.Find(RegionTravel.IntroCutscene);
        Assert.NotNull(father);
        Assert.Equal(7, father.Vectors[1].Count);
        Assert.Equal("FadeOut", father.Vectors[1][0]);
        Assert.Equal("FadeIn", father.Vectors[1][^1]);
        Assert.Equal(0x00CC017Cu, RegionTravel.CutsceneVector1Copy);
        Assert.Equal(0x00CBEB7Eu, RegionTravel.CutsceneSkipPredicate);
        Assert.False(RegionTravel.FirstSeenCutsceneVector1AutoRuns);
        Assert.False(RegionTravel.FirstSeenCutsceneSkipFires);
        var interpreter = new ScriptInterpreter(father.InstanceName, father.Commands);
        interpreter.RunUntilYield();
        Assert.False(interpreter.SkipListApplied);
        Assert.Equal(father.Commands[0], interpreter.Commands[0]);
        interpreter.ApplySkipList(father.Vectors[1]);
        Assert.True(interpreter.SkipListApplied);
        Assert.Equal(0, interpreter.InstructionPointer);
        Assert.Equal("FadeOut", interpreter.Commands[0]);
        Assert.Equal("FadeIn", interpreter.Commands[^1]);
        Assert.Equal(7, interpreter.Commands.Count);
        interpreter.ApplySkipList(["ignored"]);
        Assert.Equal("FadeOut", interpreter.Commands[0]);
    }

    [Fact]
    public void Hero_SneakTo_TRUE_polls_leftover_and_yields_once()
    {
        var command = "Hero.SneakTo MK_OVIF_HERO5,0.0,TRUE";
        var parsed = ScriptCommand.ParseSneakTo(ScriptCommand.Parse(command).Arguments);
        Assert.Equal("MK_OVIF_HERO5", parsed.Marker);
        Assert.Equal(0f, parsed.Speed);
        Assert.True(parsed.Wait);
        Assert.Equal(ScriptFlow.YieldAfter, ScriptCommand.Classify(ScriptCommand.Parse(command)));
        Assert.Equal(0x00CC0F1Au, RegionTravel.SneakToWaitPoll);
        Assert.Equal(0x00CC0ECDu, RegionTravel.SneakToWaitYield);
        Assert.Equal(104, RegionTravel.SneakToWaitPollVtbl);
        Assert.True(RegionTravel.FirstSeenSneakToTruePollsArrival);
        Assert.True(RegionTravel.FirstSeenSneakToTrueYieldsOnce);
        Assert.False(RegionTravel.FirstSeenSneakToAppliesMove);
        Assert.Equal("MK_OVIF_HERO5", RegionTravel.IntroSneakWaitMarker);
        var interpreter = new ScriptInterpreter("sneakw",
        [
            command,
            "FadeOut",
            "GamePause 0.5",
        ]);
        interpreter.RunUntilYield();
        Assert.Contains(command, interpreter.Executed);
        Assert.Equal("FadeOut", interpreter.Commands[interpreter.InstructionPointer]);
        Assert.True(interpreter.Yielded);
        Assert.Null(interpreter.UnsupportedCommand);
    }

    [Fact]
    public void Father_LookInDirection_records_degrees_and_does_not_yield()
    {
        var command = "Father.LookInDirection 215";
        var parsed = ScriptCommand.ParseLookInDirection(ScriptCommand.Parse(command).Arguments);
        Assert.Equal(215f, parsed.Degrees);
        Assert.True(parsed.Flag);
        Assert.True(parsed.HasDegrees);
        Assert.Equal(ScriptFlow.Continue, ScriptCommand.Classify(ScriptCommand.Parse(command)));
        Assert.Equal(ScriptFlow.Continue, ScriptCommand.Classify(ScriptCommand.Parse("Father.LookInDirection")));
        var falseArg = ScriptCommand.ParseLookInDirection("270,FALSE");
        Assert.Equal(270f, falseArg.Degrees);
        Assert.False(falseArg.Flag);
        Assert.Equal(0x00CC3F73u, RegionTravel.LookInDirectionOpcode);
        Assert.Equal(0x00CC4009u, RegionTravel.LookInDirectionApply);
        Assert.Equal(0x00CC707Cu, RegionTravel.LookInDirectionJoin);
        Assert.Equal(1896, RegionTravel.LookInDirectionApplyVtbl);
        Assert.Equal(0x0089BDF0u, RegionTravel.LookInDirectionApplyFn);
        Assert.Equal(0x01238E00u, RegionTravel.LookInDirectionScaleVa);
        Assert.Equal(0x3B360B61u, RegionTravel.LookInDirectionScaleBits);
        Assert.Equal(1f / 360f, RegionTravel.LookInDirectionScale);
        Assert.True(RegionTravel.FirstSeenLookInDirectionDoesNotYield);
        Assert.False(RegionTravel.FirstSeenLookInDirectionAppliesHeading);
        Assert.Equal(215f, RegionTravel.IntroLookInDirectionDegrees);
        var interpreter = new ScriptInterpreter("lookdir",
        [
            command,
            "UseCamera CAM_OVIF_SHOT7",
            "Hero.SneakTo MK_OVIF_HERO5,0.0,TRUE",
        ]);
        interpreter.RunUntilYield();
        Assert.Contains(command, interpreter.Executed);
        Assert.Contains("UseCamera CAM_OVIF_SHOT7", interpreter.Executed);
        Assert.Equal("Hero.SneakTo MK_OVIF_HERO5,0.0,TRUE", interpreter.Commands[interpreter.InstructionPointer]);
        Assert.True(interpreter.Yielded);
        Assert.Null(interpreter.UnsupportedCommand);
    }

    [Fact]
    public void Father_DialogadSpeak_records_line_and_does_not_yield()
    {
        var command = "Father.DialogadSpeak Father,'TEXT_QST_048_FATHER_INTRO_100'";
        var parsed = ScriptCommand.ParseDialogadSpeak(ScriptCommand.Parse(command).Arguments);
        Assert.Equal("Father", parsed.Target);
        Assert.Contains("TEXT_QST_048_FATHER_INTRO_100", parsed.Text);
        Assert.Equal(0, parsed.Mode);
        Assert.Equal(ScriptFlow.Continue, ScriptCommand.Classify(ScriptCommand.Parse(command)));
        Assert.Equal(ScriptFlow.Continue, ScriptCommand.Classify(ScriptCommand.Parse("Father.DialogadSpeak")));
        Assert.Equal(1, ScriptCommand.ParseDialogadSpeak("Father,'X',_,random").Mode);
        Assert.Equal(2, ScriptCommand.ParseDialogadSpeak("Father,'X',_,norepeat").Mode);
        Assert.Equal(3, ScriptCommand.ParseDialogadSpeak("Father,'X',_,sequence").Mode);
        Assert.Equal(0x00CC3354u, RegionTravel.DialogadSpeakOpcode);
        Assert.Equal(0x00CC34C8u, RegionTravel.DialogadSpeakMode);
        Assert.Equal(0x00CD3187u, RegionTravel.DialogadSpeakTable);
        Assert.Equal(0x00CC707Cu, RegionTravel.DialogadSpeakHitJoin);
        Assert.Equal(0x00CC2C6Bu, RegionTravel.DialogadSpeakMissJoin);
        Assert.Equal(0x00CC7081u, RegionTravel.DialogadSpeakSkip);
        Assert.Equal(52, RegionTravel.DialogadSpeakApplyVtbl);
        Assert.Equal(0x004CD1B0u, RegionTravel.DialogadSpeakApplyStub);
        Assert.Equal(0x0127293Cu, RegionTravel.DialogadSpeakThingVtbl);
        Assert.Equal(280, RegionTravel.DialogadSpeakContextSameVtbl);
        Assert.Equal(288, RegionTravel.DialogadSpeakContextNameVtbl);
        Assert.True(RegionTravel.FirstSeenDialogadSpeakDoesNotYield);
        Assert.False(RegionTravel.FirstSeenDialogadSpeakAppliesUi);
        Assert.Equal("TEXT_QST_048_FATHER_INTRO_100", RegionTravel.IntroFatherDialogAd);
        Assert.Equal("Father", RegionTravel.IntroDialogAdTarget);
        var interpreter = new ScriptInterpreter("adspeak",
        [
            command,
            "GamePause 0.5",
            "NoLoadUseCamera CAM_OVIF_SHOT6",
        ]);
        interpreter.RunUntilYield();
        Assert.Contains(command, interpreter.Executed);
        Assert.Equal("GamePause 0.5", interpreter.Commands[interpreter.InstructionPointer]);
        Assert.True(interpreter.Yielded);
        Assert.Null(interpreter.UnsupportedCommand);
    }

    [Fact]
    public void Remove_VILL1_records_name_and_does_not_yield()
    {
        var command = "Remove VILL1";
        Assert.Equal(ScriptFlow.Continue, ScriptCommand.Classify(ScriptCommand.Parse(command)));
        Assert.Equal(ScriptFlow.Continue, ScriptCommand.Classify(ScriptCommand.Parse("Remove")));
        Assert.Equal(0x00CD0116u, RegionTravel.RemoveOpcode);
        Assert.Equal(432, RegionTravel.RemoveApplyVtbl);
        Assert.Equal(0x008910D0u, RegionTravel.RemoveApplyFn);
        Assert.Equal(0x004C9B80u, RegionTravel.RemoveInner);
        Assert.True(RegionTravel.FirstSeenRemoveDoesNotYield);
        Assert.Equal("VILL1", RegionTravel.IntroRemoveName);
        var interpreter = new ScriptInterpreter("rm",
        [
            command,
            "Father.Speak Father,'TEXT_QST_048_FATHER_INTRO_80'",
            "NoLoadUseCamera CAM_OVIF_SHOT3",
        ]);
        interpreter.RunUntilYield();
        Assert.Contains(command, interpreter.Executed);
        Assert.Contains("Father.Speak Father,'TEXT_QST_048_FATHER_INTRO_80'", interpreter.Executed);
        Assert.Equal("NoLoadUseCamera CAM_OVIF_SHOT3", interpreter.Commands[interpreter.InstructionPointer]);
        Assert.Null(interpreter.UnsupportedCommand);
    }

    [Fact]
    public void WaitActiveDialog_first_seen_yields_once()
    {
        Assert.Equal(ScriptFlow.YieldAfter, ScriptCommand.Classify(ScriptCommand.Parse("WaitActiveDialog")));
        Assert.Equal(0x00CC656Bu, RegionTravel.WaitActiveDialogOpcode);
        Assert.Equal(1472, RegionTravel.WaitActiveDialogPollVtbl);
        Assert.Equal(0x008907D0u, RegionTravel.WaitActiveDialogPollFn);
        Assert.True(RegionTravel.FirstSeenWaitActiveDialogYieldsOnce);
        var interpreter = new ScriptInterpreter("wad", ["WaitActiveDialog", "UseCamera CAM_OVIF_SHOT3"]);
        interpreter.RunUntilYield();
        Assert.True(interpreter.Yielded);
        Assert.Contains("WaitActiveDialog", interpreter.Executed);
        Assert.Equal("UseCamera CAM_OVIF_SHOT3", interpreter.Commands[interpreter.InstructionPointer]);
        Assert.Null(interpreter.UnsupportedCommand);
    }

    [Fact]
    public void VILL1_WalkTo_yields_once_without_move()
    {
        var command = "VILL1.WalkTo MK_OVI_ID_VW1";
        var parsed = ScriptCommand.ParseSneakTo(ScriptCommand.Parse(command).Arguments);
        Assert.Equal("MK_OVI_ID_VW1", parsed.Marker);
        Assert.Equal(0.3f, parsed.Speed);
        Assert.False(parsed.Wait);
        Assert.Equal(ScriptFlow.YieldAfter, ScriptCommand.Classify(ScriptCommand.Parse(command)));
        Assert.Equal(ScriptFlow.Continue, ScriptCommand.Classify(ScriptCommand.Parse("WalkTo MK_OVI_ID_VW1")));
        Assert.Equal(ScriptFlow.Continue, ScriptCommand.Classify(ScriptCommand.Parse("VILL1.WalkTo")));
        Assert.Equal(0x00CC083Du, RegionTravel.WalkToOpcode);
        Assert.Equal(0x004C72B0u, RegionTravel.WalkToApplyStub);
        Assert.Equal(0, RegionTravel.WalkToMode);
        Assert.False(RegionTravel.FirstSeenWalkToAppliesMove);
        Assert.False(RegionTravel.FirstSeenWalkToWaitsForArrival);
        var interpreter = new ScriptInterpreter("walk", [command, "GamePause 0.8"]);
        interpreter.RunUntilYield();
        Assert.True(interpreter.Yielded);
        Assert.Contains(command, interpreter.Executed);
        Assert.Equal("GamePause 0.8", interpreter.Commands[interpreter.InstructionPointer]);
        Assert.Null(interpreter.UnsupportedCommand);
    }

    [Fact]
    public void Create_villager_records_args_and_does_not_yield()
    {
        var command = "Create CREATURE_OAKVALE_VILLAGER_FEMALE_NORMAL_MESH,MK_OVI_ID_VS1,VILL1";
        var parsed = ScriptCommand.ParseCreate(ScriptCommand.Parse(command).Arguments);
        Assert.Equal(RegionTravel.IntroCreateType, parsed.Type);
        Assert.Equal(RegionTravel.IntroCreateMarker, parsed.Marker);
        Assert.Equal(RegionTravel.IntroCreateName, parsed.Name);
        Assert.Equal(ScriptFlow.Continue, ScriptCommand.Classify(ScriptCommand.Parse(command)));
        Assert.Equal(ScriptFlow.Continue, ScriptCommand.Classify(ScriptCommand.Parse("Create")));
        Assert.Equal(0x00CCC246u, RegionTravel.CreateOpcode);
        Assert.Equal(364, RegionTravel.CreateApplyVtbl);
        Assert.Equal(0x008A9100u, RegionTravel.CreateApplyFn);
        Assert.True(RegionTravel.FirstSeenCreateDoesNotYield);
        var interpreter = new ScriptInterpreter("create", [command]);
        interpreter.RunUntilYield();
        Assert.Contains(command, interpreter.Executed);
        Assert.True(interpreter.Finished);
        Assert.Null(interpreter.UnsupportedCommand);
    }

    [Fact]
    public void Father_PlayCombatAnimation_yields_once_without_pose()
    {
        var command = "Father.PlayCombatAnimation TURNING_AC90,FALSE,TRUE";
        var parsed = ScriptCommand.ParsePlayCombatAnimation(ScriptCommand.Parse(command).Arguments);
        Assert.Equal("TURNING_AC90", parsed.Name);
        Assert.True(parsed.FlagA);
        Assert.True(parsed.FlagB);
        Assert.False(parsed.FlagC);
        Assert.True(parsed.FlagD);
        Assert.False(parsed.FlagE);
        Assert.Equal(1, parsed.Count);
        Assert.Equal(ScriptFlow.YieldAfter, ScriptCommand.Classify(ScriptCommand.Parse(command)));
        Assert.Equal(ScriptFlow.Continue, ScriptCommand.Classify(
            ScriptCommand.Parse("Father.PlayCombatAnimation")));
        Assert.True(ScriptCommand.IsPlayCombatAnimation("PlayCombatAnim"));
        Assert.Equal(0x00CC15E3u, RegionTravel.PlayCombatAnimationOpcode);
        Assert.Equal(76, RegionTravel.PlayCombatAnimationApplyVtbl);
        Assert.Equal(0x00834760u, RegionTravel.PlayCombatAnimationFatherFn);
        Assert.Equal(0x006AD9D0u, RegionTravel.PlayCombatAnimationPlayerFn);
        Assert.True(RegionTravel.FirstSeenPlayCombatAnimationYields);
        Assert.False(RegionTravel.FirstSeenPlayCombatAnimationAppliesPose);
        Assert.False(WorldShading.FirstSeenPlaysAnim);
        var interpreter = new ScriptInterpreter("combat", [command, "GamePause 1.0"]);
        interpreter.RunUntilYield();
        Assert.True(interpreter.Yielded);
        Assert.Contains(command, interpreter.Executed);
        Assert.Equal("GamePause 1.0", interpreter.Commands[interpreter.InstructionPointer]);
        Assert.Null(interpreter.UnsupportedCommand);
    }

    [Fact]
    public void Hero_SneakTo_first_seen_yields_once_without_move()
    {
        var command = "Hero.SneakTo MK_OVIF_HERO4,0.0,FALSE,FALSE,FALSE";
        var parsed = ScriptCommand.ParseSneakTo(ScriptCommand.Parse(command).Arguments);
        Assert.Equal("MK_OVIF_HERO4", parsed.Marker);
        Assert.Equal(0f, parsed.Speed);
        Assert.False(parsed.Wait);
        Assert.Equal(ScriptFlow.YieldAfter, ScriptCommand.Classify(ScriptCommand.Parse(command)));
        Assert.Equal(ScriptFlow.YieldAfter, ScriptCommand.Classify(
            ScriptCommand.Parse("Hero.SneakTo MK_OVIF_HERO4,0.0,TRUE")));
        Assert.Equal(ScriptFlow.Continue, ScriptCommand.Classify(
            ScriptCommand.Parse("Hero.SneakTo")));
        Assert.Equal(0x00CC0CB5u, RegionTravel.SneakToOpcode);
        Assert.Equal(0x004C72B0u, RegionTravel.SneakToApplyStub);
        Assert.Equal(2, RegionTravel.SneakToMode);
        Assert.False(RegionTravel.FirstSeenSneakToAppliesMove);
        Assert.False(RegionTravel.FirstSeenSneakToWaitsForArrival);
        var interpreter = new ScriptInterpreter("sneak", [command, "GamePause 1.0"]);
        interpreter.RunUntilYield();
        Assert.True(interpreter.Yielded);
        Assert.Contains(command, interpreter.Executed);
        Assert.Equal("GamePause 1.0", interpreter.Commands[interpreter.InstructionPointer]);
        Assert.Null(interpreter.UnsupportedCommand);
    }

    [Fact]
    public void Hero_WaitTask_FOO_yields_once_and_ignores_name()
    {
        var command = "Hero.WaitTask FOO";
        Assert.Equal(ScriptFlow.YieldAfter, ScriptCommand.Classify(ScriptCommand.Parse(command)));
        Assert.Equal(ScriptFlow.Continue, ScriptCommand.Classify(ScriptCommand.Parse("WaitTask FOO")));
        Assert.Equal(0x00CC0783u, RegionTravel.WaitTaskOpcode);
        Assert.Equal(104, RegionTravel.WaitTaskPollVtbl);
        Assert.Equal(0x012457FCu, RegionTravel.WaitTaskHeroVtbl);
        Assert.Equal(0x006A9550u, RegionTravel.WaitTaskHeroPoll);
        Assert.Equal(0x00661A40u, RegionTravel.WaitTaskPollStub);
        Assert.Equal(0x013D2838u, RegionTravel.WaitTaskFiberGlobal);
        Assert.True(RegionTravel.FirstSeenWaitTaskYieldsOnce);
        Assert.False(RegionTravel.FirstSeenWaitTaskReadsName);
        Assert.Equal("FOO", RegionTravel.IntroWaitTask);
        var interpreter = new ScriptInterpreter("wait",
        [
            command,
            "Hero.SneakTo MK_OVIF_HERO4,0.0,FALSE,FALSE,FALSE",
        ]);
        interpreter.RunUntilYield();
        Assert.True(interpreter.Yielded);
        Assert.Contains(command, interpreter.Executed);
        Assert.Equal(
            "Hero.SneakTo MK_OVIF_HERO4,0.0,FALSE,FALSE,FALSE",
            interpreter.Commands[interpreter.InstructionPointer]);
        Assert.Null(interpreter.UnsupportedCommand);
    }

    [Fact]
    public void Father_DialogSpeak_yields_once_then_continues()
    {
        var command = "Father.DialogSpeak HERO,'TEXT_QST_048_FATHER_INTRO_60'";
        var parsed = ScriptCommand.ParseDialogSpeak(ScriptCommand.Parse(command).Arguments);
        Assert.Equal("HERO", parsed.Listener);
        Assert.Contains("TEXT_QST_048_FATHER_INTRO_60", parsed.Text);
        Assert.Equal(ScriptFlow.YieldAfter, ScriptCommand.Classify(ScriptCommand.Parse(command)));
        Assert.Equal(ScriptFlow.Continue, ScriptCommand.Classify(
            ScriptCommand.Parse("Father.DialogSpeak HERO,null")));
        Assert.Equal(0x00CC3165u, RegionTravel.DialogSpeakOpcode);
        Assert.Equal(0x008906C0u, RegionTravel.DialogSpeakBeginFn);
        Assert.Equal(0x008907D0u, RegionTravel.DialogSpeakWaitFn);
        Assert.True(RegionTravel.FirstSeenDialogSpeakYieldsOnce);
        var interpreter = new ScriptInterpreter("dspeak", [command, "Hero.WaitTask FOO"]);
        interpreter.RunUntilYield();
        Assert.True(interpreter.Yielded);
        Assert.Contains(command, interpreter.Executed);
        Assert.Equal("Hero.WaitTask FOO", interpreter.Commands[interpreter.InstructionPointer]);
        Assert.Null(interpreter.UnsupportedCommand);
    }

    [Fact]
    public void Father_InteractiveSpeak_false_yields_once()
    {
        var command =
            "Father.InteractiveSpeak Hero,'TEXT_QST_048_FATHER_INTRO_20',FALSE,'TEXT_QST_048_FATHER_INTRO_30'";
        var parsed = ScriptCommand.ParseInteractiveSpeak(
            ScriptCommand.Parse(command).Arguments);
        Assert.Equal("Hero", parsed.Listener);
        Assert.Contains("TEXT_QST_048_FATHER_INTRO_20", parsed.Prompt);
        Assert.False(parsed.Wait);
        Assert.Contains("TEXT_QST_048_FATHER_INTRO_30", parsed.Response);
        Assert.Equal(ScriptFlow.YieldAfter, ScriptCommand.Classify(ScriptCommand.Parse(command)));
        Assert.Equal(ScriptFlow.Yield, ScriptCommand.Classify(
            ScriptCommand.Parse("Father.InteractiveSpeak Hero,'A',TRUE,'B'")));
        var interpreter = new ScriptInterpreter("ispeak", [command, "GamePause 1.2"]);
        interpreter.RunUntilYield();
        Assert.Contains(command, interpreter.Executed);
        Assert.Equal("GamePause 1.2", interpreter.Commands[interpreter.InstructionPointer]);
        Assert.True(interpreter.Yielded);
        Assert.True(RegionTravel.FirstSeenInteractiveSpeakYieldsOnce);
    }

    [Fact]
    public void UseCamera_shot2_binds_then_yields_once()
    {
        Assert.Equal(ScriptFlow.YieldAfter, ScriptCommand.Classify(
            ScriptCommand.Parse("UseCamera CAM_OVIF_SHOT2")));
        Assert.Equal(ScriptFlow.YieldAfter, ScriptCommand.Classify(
            ScriptCommand.Parse("NoLoadUseCamera CAM_OVI_ID_STANDUP")));
        Assert.Equal(ScriptFlow.Continue, ScriptCommand.Classify(
            ScriptCommand.Parse("UseCamera")));
        Assert.Equal(ScriptFlow.Continue, ScriptCommand.Classify(
            ScriptCommand.Parse("UseCamera null")));
        var interpreter = new ScriptInterpreter("cam",
            ["UseCamera CAM_OVIF_SHOT2", "GamePause 5.2"]);
        interpreter.RunUntilYield();
        Assert.Contains("UseCamera CAM_OVIF_SHOT2", interpreter.Executed);
        Assert.Equal("GamePause 5.2", interpreter.Commands[interpreter.InstructionPointer]);
        Assert.True(interpreter.Yielded);
        Assert.True(RegionTravel.FirstSeenUseCameraYields);
        Assert.Equal(0x00CC9F39u, RegionTravel.UseCameraOpcode);
        Assert.Equal(0x00CBFD53u, RegionTravel.UseCameraYieldFlagWrite);
    }

    [Fact]
    public void FadeOut_overlay_alpha_is_elapsed_over_duration_then_stays()
    {
        Assert.Equal(0x006496BCu, RegionTravel.FadeOverlayDraw);
        Assert.Equal(0x00648820u, RegionTravel.FadeOverlayDrawFn);
        Assert.Equal(0x004348D0u, RegionTravel.FadeOverlayAlphaFn);
        Assert.Equal(0x00434870u, RegionTravel.FadeOverlayTick);
        Assert.Equal(0x0041BEB0u, RegionTravel.FadeOverlayRecord);
        Assert.Equal(0x22u, RegionTravel.FadeOverlayRecordType);
        Assert.Equal(0xC0u, RegionTravel.FadeOverlaySubmit);
        Assert.Equal(92, RegionTravel.FadeOverlaySubmitVtbl);
        Assert.Equal(0x0125A298u, RegionTravel.FadeOverlaySizeVa);
        Assert.Equal(-8f, RegionTravel.FadeOverlaySizePad);
        Assert.Equal(0x0088E4C0u, RegionTravel.FadeInApply);
        Assert.Equal(1496, RegionTravel.FadeInApplyVtbl);
        Assert.Equal(0x00434C90u, RegionTravel.FadeInClearLock);
        Assert.Equal(255f, RegionTravel.FadeAlphaScale);
        Assert.Equal(0.0001f, RegionTravel.FadeAlphaEpsilon);
        Assert.True(RegionTravel.FirstSeenFadeOverlayDraws);
        Assert.False(RegionTravel.FirstSeenFadeOverlayDrawUnread);
        var runtime = new ScriptRuntime();
        runtime.CreateFiber("fade");
        ((IScriptHost)runtime).FadeOut(0.5f, 0f);
        Assert.True(runtime.FadeActive);
        Assert.True(runtime.FadeRising);
        Assert.Equal(0f, runtime.OverlayAlpha);
        Assert.Equal(0, runtime.OverlayAlphaByte);
        runtime.Update(0.25f);
        Assert.Equal(0.5f, runtime.OverlayAlpha);
        Assert.Equal(127, runtime.OverlayAlphaByte);
        runtime.Update(0.25f);
        Assert.Equal(1f, runtime.OverlayAlpha);
        Assert.Equal(255, runtime.OverlayAlphaByte);
        Assert.True(runtime.FadeActive);
        Assert.False(runtime.FadeRising);
        ((IScriptHost)runtime).FadeIn(0.5f, 0f);
        Assert.False(runtime.FadeLocked);
        Assert.True(runtime.FadeFalling);
        runtime.Update(0.1f);
        Assert.False(runtime.FadeActive);
        Assert.Equal(0f, runtime.OverlayAlpha);
    }

    [Fact]
    public void FadeOut_packs_black_and_locks_second_apply()
    {
        var install = Require();
        using var levels = new LevelLibrary(install);
        var things = levels.LoadThings("StartOakValeWest").Things.ToList();
        var runtime = ScriptRuntime.StartNewGame(install, things);
        Assert.True(runtime.FadeActive);
        Assert.True(runtime.FadeLocked);
        Assert.Equal((0, 0, 0, 255), runtime.FadeColor);
        Assert.Equal(0.5f, runtime.FadeDuration);
        Assert.Equal(0x008907E0u, RegionTravel.FadeApplyFn);
        Assert.Equal(188, RegionTravel.FadeActiveOffset);
        Assert.Equal(216, RegionTravel.FadeLockOffset);
        Assert.True(RegionTravel.FirstSeenFadeOverlayDraws);
        Assert.False(RegionTravel.FirstSeenFadeOverlayDrawUnread);
        Assert.Equal(0, runtime.OverlayAlpha);
        Assert.Equal(0, runtime.OverlayAlphaByte);
        Assert.True(runtime.FadeRising);
        runtime.Update(0.25f);
        Assert.Equal(0.5f, runtime.OverlayAlpha);
        runtime.Update(0.25f);
        Assert.Equal(1f, runtime.OverlayAlpha);
        Assert.Equal(255, runtime.OverlayAlphaByte);
        Assert.True(runtime.FadeActive);
        Assert.False(runtime.FadeRising);
    }

    [Fact]
    public void Cutscene_commands_come_from_persist_vectors_not_ascii_scrape()
    {
        var install = Require();
        var bank = ScriptBank.Load(install);
        var father = bank.Find(RegionTravel.IntroCutscene);
        Assert.NotNull(father);
        Assert.Equal(ScriptBank.CutsceneType, father.TypeName);
        Assert.True(father.CommandsLayoutProven);
        Assert.Equal(ScriptBank.CutsceneVectorCount, father.Vectors.Count);
        Assert.Equal(60, ScriptBank.CommandRuntimeOffset);
        Assert.Equal(0x00F2A1D0u, ScriptBank.PersistFn);
        Assert.Equal(0x00433273u, ScriptBank.VectorRead);
        Assert.Equal(0x00432EE9u, ScriptBank.VectorCopy);
        Assert.Equal(RegionTravel.IntroPlayMusic, father.Commands[0]);
        Assert.Equal(RegionTravel.FadeSpecialCase, father.Commands[1]);
        Assert.Equal("CameraPause FALSE", father.Commands[2]);
        Assert.Equal("Hero.Teleport MK_OVI_ID_HERO,FALSE", father.Commands[3]);
        Assert.Equal("Father.LookToThing Hero,FOREVER", father.Commands[5]);
        Assert.Contains("UseCamera CAM_OVIF_SHOT2", father.Commands);
        Assert.Contains("Father.LookInDirection 215", father.Commands);
        Assert.Contains("Hero.SneakTo MK_OVIF_HERO5,0.0,TRUE", father.Commands);
        Assert.Equal(RegionTravel.IntroCutsceneLastCommand, father.Commands[^1]);
        Assert.Equal(8, father.Vectors.Count);
        Assert.Equal(father.Commands.Count, father.Vectors[0].Count);
        Assert.Equal(7, father.Vectors[1].Count);
        Assert.Equal("FadeOut", father.Vectors[1][0]);
        Assert.Equal("GamePause 0.5", father.Vectors[1][1]);
        Assert.Equal("UseCamera CAM_OVIF_SHOT7", father.Vectors[1][2]);
        Assert.Equal("Hero.Teleport MK_OVIF_HERO5", father.Vectors[1][3]);
        Assert.Equal("Father.Teleport MK_OVI_ID_DAD", father.Vectors[1][4]);
        Assert.Equal("Father.LookInDirection 215", father.Vectors[1][5]);
        Assert.Equal("FadeIn", father.Vectors[1][6]);
        Assert.Empty(father.Vectors[2]);
        Assert.Empty(father.Vectors[3]);
        Assert.Empty(father.Vectors[4]);
        Assert.Empty(father.Vectors[5]);
        Assert.Empty(father.Vectors[6]);
        Assert.Empty(father.Vectors[7]);
        Assert.False(RegionTravel.FirstSeenCutsceneVector1AutoRuns);
        Assert.False(RegionTravel.FirstSeenCutsceneSkipFires);
        Assert.Equal(0x00CC017Cu, RegionTravel.CutsceneVector1Copy);
        Assert.Equal(0x00CBEB7Eu, RegionTravel.CutsceneSkipPredicate);
        Assert.Equal(0x0143E8F4u, RegionTravel.CutsceneSkipGlobal);
        Assert.Equal(168, RegionTravel.CutsceneSkipVtblA);
        Assert.Equal(176, RegionTravel.CutsceneSkipVtblB);
        Assert.Equal(0x00894440u, RegionTravel.CutsceneSkipFnA);
        Assert.Equal(0x00893B00u, RegionTravel.CutsceneSkipFnB);
        Assert.DoesNotContain(father.Commands, line => line.Equals("CCutsceneDef", StringComparison.Ordinal));
        Assert.True(father.Commands.Count >= 60);
        Assert.True(father.Vectors[0].Count == father.Commands.Count);

        var attract = bank.Find("CS_ATTRACT_12");
        Assert.NotNull(attract);
        Assert.True(attract.CommandsLayoutProven);
        Assert.Equal("SetTime 14", attract.Commands[0]);
        Assert.StartsWith("NoLoadUseCamera ", attract.Commands[3], StringComparison.Ordinal);
    }

    [Fact]
    public void First_seen_Teleport_moves_kid_and_places_father()
    {
        var install = Require();
        using var levels = new LevelLibrary(install);
        var things = levels.LoadThings("StartOakValeWest").Things.ToList();
        var spawn = RegionTravel.FindPlayerStart(things);
        Assert.NotNull(spawn);
        var heroMarker = things.First(t => t.ScriptName == RegionTravel.IntroHeroTeleportMarker);
        var dadMarker = things.First(t => t.ScriptName == RegionTravel.IntroFatherTeleportMarker);
        var runtime = ScriptRuntime.StartNewGame(install, things);
        Assert.Equal(RegionTravel.PositionOf(heroMarker), runtime.ActorPositions[RegionTravel.IntroHeroActor]);
        Assert.Equal(RegionTravel.PositionOf(dadMarker), runtime.ActorPositions[RegionTravel.IntroFatherActor]);
        Assert.NotEqual(RegionTravel.PositionOf(spawn), runtime.ActorPositions[RegionTravel.IntroHeroActor]);
        Assert.Equal(0x0089B780u, RegionTravel.TeleportApplyFn);
        Assert.Equal(0x004AA980u, RegionTravel.TeleportMarkerPos);
        Assert.True(RegionTravel.FirstSeenTeleportAppliesPos);
        Assert.False(RegionTravel.FirstSeenPlayAvi);

        var world = WorldGeometry.Build(
            install, "StartOakValeWest", things, actorPositions: runtime.ActorPositions);
        var nearHero = CountPropNear(
            world, heroMarker.PositionX!.Value, heroMarker.PositionY!.Value, 4f);
        var nearSpawn = CountPropNear(
            world, spawn.PositionX!.Value, spawn.PositionY!.Value, 4f);
        var nearDad = CountPropNear(
            world, dadMarker.PositionX!.Value, dadMarker.PositionY!.Value, 4f);
        Assert.True(nearHero > 10, $"kid mesh missing at MK_OVI_ID_HERO nearHero={nearHero}");
        Assert.True(nearDad > 10, $"Father mesh missing at MK_OVI_ID_DAD nearDad={nearDad}");
        Assert.True(nearHero > nearSpawn, $"kid still at NOVStartHSP nearHero={nearHero} nearSpawn={nearSpawn}");
    }

    [Fact]
    public void PlayAvi_rewrites_xmv_to_installed_wmv_and_blocks()
    {
        var install = Require();
        var relative = RegionTravel.PlayAviPrefix + RegionTravel.IntroPlayAvi;
        Assert.Equal(
            RegionTravel.PlayAviPrefix + RegionTravel.IntroPlayAviRewritten,
            RegionTravel.RewritePlayAviPath(relative));
        Assert.True(RegionTravel.PlayAviIsWmvPath(relative));
        var file = RegionTravel.ResolvePlayAviFile(install, relative);
        Assert.NotNull(file);
        Assert.True(File.Exists(file));
        Assert.Equal("dream_sequence_comp.wmv", Path.GetFileName(file), StringComparer.OrdinalIgnoreCase);
        Assert.True(RegionTravel.FileHasAsfMagic(file));
        using var player = WmvPlayer.TryOpen(file);
        Assert.True(
            player is { Rgba.Length: > 0, Width: >= 16, Height: >= 16 },
            $"WmvPlayer opened without a frame: {WmvPlayer.LastError}");
        Assert.True(
            player.SamplesFromGetPointer,
            $"samples are not IMediaSample::GetPointer: {WmvPlayer.LastError}");
        Assert.True(player.Rgba.Any(b => b != 0), "first frame is all zero");
        Assert.False(RegionTravel.FirstSeenPlayAviUsesVideoWindow);
        Assert.False(RegionTravel.FirstSeenPlayAviUsesGetCurrentImage);
        Assert.True(RegionTravel.FirstSeenPlayAviCopiesRgb24ToArgb);
        Assert.Equal(33, RegionTravel.PlayAviPresentMs);
        Assert.Equal(0x00A3B730u, RegionTravel.PlayAviCopySample);
        Assert.Equal(172, RegionTravel.PlayAviCopyVtbl);
        Assert.Equal("Fable Texture Renderer Filter", RegionTravel.PlayAviFilterName);
        Assert.Equal(0x009FA450u, RegionTravel.PlayAviLockRect);
        var serial = player.FrameSerial;
        Thread.Sleep(200);
        Assert.True(
            player.FrameSerial >= serial + 3,
            $"WMV present too slow serial={player.FrameSerial} start={serial} {WmvPlayer.LastError}");

        Assert.Equal(0x0099C1E0u, RegionTravel.PlayAviRewrite);
        Assert.Equal(0x00A3B9D0u, RegionTravel.PlayAviOpen);
        Assert.Equal(0x01258DE0u, RegionTravel.PlayAviExtXmvVa);
        Assert.Equal(0x01258DECu, RegionTravel.PlayAviExtWmvVa);
        Assert.Equal(0x0129D1E8u, RegionTravel.PlayAviExtAsfVa);
        Assert.True(RegionTravel.IsPlayAviSkipScan(RegionTravel.PlayAviSkipEscape));
        Assert.True(RegionTravel.IsPlayAviSkipScan(57));
        Assert.True(RegionTravel.IsPlayAviSkipScan(28));
        Assert.True(RegionTravel.IsPlayAviSkipScan(62));
        Assert.False(RegionTravel.IsPlayAviSkipScan(2));
        var box = RegionTravel.PlayAviLetterbox(640, 480, 800, 600);
        Assert.Equal(0f, box.X0, 3);
        Assert.Equal(0f, box.Y0, 3);
        Assert.Equal(1f, box.X1, 3);
        Assert.Equal(1f, box.Y1, 3);
        var tall = RegionTravel.PlayAviLetterbox(640, 480, 800, 800);
        Assert.True(tall.Y0 > 0.05f, $"letterbox y0={tall.Y0}");
        Assert.True(tall.Y1 < 0.95f, $"letterbox y1={tall.Y1}");
        var wide = RegionTravel.PlayAviLetterbox(640, 480, 1920, 1080);
        Assert.True(wide.X0 > 0.05f, $"wide letterbox x0={wide.X0}");
        Assert.True(wide.X1 < 0.95f, $"wide letterbox x1={wide.X1}");
        Assert.Equal(0f, wide.Y0, 3);
        Assert.Equal(1f, wide.Y1, 3);
        Assert.Equal(0.5f, RegionTravel.PlayAviLetterboxHalf);
        Assert.Equal(0x0122F59Cu, RegionTravel.PlayAviLetterboxHalfVa);
        Assert.Equal(0x009DC870u, RegionTravel.PlayAviBlit);

        using var levels = new LevelLibrary(install);
        var things = levels.LoadThings("StartOakValeWest").Things.ToList();
        var runtime = ScriptRuntime.StartNewGame(install, things);
        var intro = runtime.FindInterpreter(RegionTravel.IntroCutscene);
        Assert.NotNull(intro);
        runtime.Update(0.1f);
        runtime.Update(0.1f);
        runtime.Update(0.1f);
        Assert.Contains("PlayAVI dream_sequence_comp.xmv", intro.Executed);
        Assert.True(runtime.AviPlaying);
        Assert.Equal(file, runtime.AviFile, StringComparer.OrdinalIgnoreCase);
        Assert.False(intro.ExecutedVerb("MuteSounds"));
        runtime.SkipAvi();
        runtime.Update(0.1f);
        Assert.False(runtime.AviPlaying);
        Assert.Contains("MuteSounds false", intro.Executed);
    }

    private static int CountPropNear(WorldGeometry world, float x, float y, float radius)
    {
        var r2 = radius * radius;
        return world.Triangles.Count(t =>
        {
            if (t.Layer != Fable.Formats.Meshes.SceneLayer.Prop)
                return false;
            var mx = (t.A.X + t.B.X + t.C.X) / 3f - x;
            var my = (t.A.Y + t.B.Y + t.C.Y) / 3f - y;
            return mx * mx + my * my < r2;
        });
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
