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
        Assert.True(RegionTravel.FirstSeenFadeOverlayDrawUnread);
        Assert.False(RegionTravel.FirstSeenFadeSpecialCaseRuns);
        Assert.Equal("FadeOut 0.5,0", RegionTravel.FadeSpecialCase);
        Assert.Equal(0.5f, RegionTravel.FadeSpecialCaseSeconds);
        Assert.Equal(1488, RegionTravel.FadeSpecialCaseVtbl);
        Assert.Equal(0x00CCA26Eu, RegionTravel.PlayAviSite);
        Assert.Equal(0x00CCA26Du, RegionTravel.PlayAviOpcode);
        Assert.Equal(0x00CD17F8u, RegionTravel.PlayAviJoin);
        Assert.Equal(1476, RegionTravel.PlayAviVtbl);
        Assert.True(RegionTravel.FirstSeenPlayAviDoesNotYield);
        Assert.Equal(@"Data\Video\", RegionTravel.PlayAviPrefix);
        Assert.Equal("dream_sequence_comp.xmv", RegionTravel.IntroPlayAvi);
        Assert.Equal(0x00CC9E6Au, RegionTravel.NoLoadUseCameraSite);
        Assert.Equal(0x00CC9E69u, RegionTravel.NoLoadUseCameraOpcode);
        Assert.Equal(0x00CC9F39u, RegionTravel.UseCameraOpcode);
        Assert.Equal(0x00CCA22Cu, RegionTravel.UseCameraYield);
        Assert.Equal(0x00CBFD53u, RegionTravel.UseCameraYieldFlagWrite);
        Assert.True(RegionTravel.FirstSeenUseCameraYields);
        Assert.True(RegionTravel.FirstSeenNoLoadUseCameraYields);
        Assert.False(RegionTravel.FirstSeenPlayAvi);
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
        Assert.Equal(0x01375748u, RegionTravel.PlayAnimationFlagByte);
        Assert.Equal(0x01010101u, RegionTravel.PlayAnimationFlagByteDword);
        Assert.True(RegionTravel.FirstSeenPlayAnimationYields);
        Assert.Equal("CS_WAKING_UP_LOOP", RegionTravel.IntroWakeLoop);
        Assert.Equal("CS_WAKING_UP_ON_STEPS", RegionTravel.IntroWakeSteps);
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
        Assert.True(RegionTravel.FirstSeenLookToThingYields);
        Assert.Equal(0x00CC4678u, RegionTravel.TeleportOpcode);
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
        Assert.True(RegionTravel.FirstSeenPlayAviDoesNotYield);
        Assert.False(RegionTravel.FirstSeenPlayAvi);
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
        Assert.True(RegionTravel.FirstSeenFadeOverlayDrawUnread);
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
