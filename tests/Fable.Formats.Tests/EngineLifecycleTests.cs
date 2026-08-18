using Fable.Core;
using Fable.Game;

namespace Fable.Formats.Tests;

public sealed class EngineLifecycleTests
{
    [Fact]
    public void Pe_entry_is_crt_not_new_game()
    {
        Assert.Equal(0x00401067u, EngineLifecycle.PeEntry);
        Assert.Equal(0x00403480u, EngineLifecycle.WinMain);
        Assert.Equal(0x00402510u, EngineLifecycle.BootstrapFn);
        Assert.NotEqual(RegionTravel.StartOakValeSetup, EngineLifecycle.PeEntry);
        Assert.NotEqual(RegionTravel.StartOakValeSetup, EngineLifecycle.WinMain);
        Assert.NotEqual(0x00DBDE40u, EngineLifecycle.RetailPump);
        Assert.Equal(0x00412F90u, EngineLifecycle.RunModes);
        Assert.Equal(0x00418DCAu, EngineLifecycle.GameModeCtor);
        Assert.Equal(0x004184BDu, EngineLifecycle.GameStart);
        Assert.Equal(0x004189C2u, EngineLifecycle.GamePump);
        Assert.Equal(0x0041735Au, EngineLifecycle.InitWorldFn);
        Assert.Equal(0x004A6E30u, EngineLifecycle.InitWorldInitFn);
        Assert.Equal(0x00B40000u, EngineLifecycle.CloseStaticMapFileFn);
        Assert.Equal(0x00B3E820u, EngineLifecycle.OpenStaticMapsMode1Current);
        Assert.Equal(0x00B41E50u, EngineLifecycle.OpenStaticMapsAttach);
        Assert.Equal(0x0049E620u, EngineLifecycle.InitMeshBankFn);
        Assert.Equal(0x00A09F20u, EngineLifecycle.MeshBankLookupFn);
        Assert.Equal(0x009D56C0u, MeshBank.OpenVtbl4);
        Assert.Equal(0x009A7F80u, MeshBank.OpenBankFileAsync);
        Assert.Equal(0x00A27030u, EngineLifecycle.MeshBankObjectCtor);
        Assert.Equal(0x004BBFD0u, EngineLifecycle.MeshBankSetGlobalFn);
        Assert.Equal("MBANK_ALLMESHES", MeshBank.BankName);
        Assert.Equal(0x00416953u, EngineLifecycle.GameLoadWorldFn);
        Assert.Equal(32, EngineLifecycle.GameLoadWorldVtbl);
        Assert.Equal(0x004A3200u, EngineLifecycle.LoadSaveFn);
        Assert.Equal(0x00BDF010u, EngineLifecycle.AttachPatchFn);
        Assert.Equal(0x00B420F0u, EngineLifecycle.OpenStaticMapsNameTable);
    }

    [Fact]
    public void Bootstrap_follows_named_00402510_stages()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        Assert.Equal(
            new[]
            {
                "Parse Command Line",
                "Setup Basic install files",
                "Setup Language",
                "Setup basic retail banks",
                "Setup library",
                "End basic init",
            },
            life.CompletedStages);
        Assert.Equal(EngineMode.RetailFrontend, life.Mode);
        Assert.Equal(EngineStage.StartupVideos, life.Stage);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.Equal(0x009A4EC0u, EngineLifecycle.EngineSingletonGetter);
        Assert.Equal(0x013CA618u, EngineLifecycle.EngineSingletonVa);
    }

    [Fact]
    public void Retail_banks_register_in_exe_order()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        Assert.Equal(
            new[]
            {
                "GBANK_MAIN", "GBANK_MAIN_PC",
                "GBANK_GUI", "GBANK_GUI_PC",
                "GBANK_FRONT_END", "GBANK_FRONT_END_PC",
                "PARTICLE_MAIN", "PARTICLE_MAIN_PC",
                "PARTICLE_FRONTEND", "PARTICLE_FRONTEND_PC",
            },
            life.RegisteredBanks);
        Assert.Equal(0x009A8150u, EngineLifecycle.RegisterRetailBank);
        Assert.Equal(0x013CA79Cu, EngineLifecycle.BankManagerVa);
    }

    [Fact]
    public void CreateDevice_flags_are_0x26_or_0x56()
    {
        Assert.Equal(0x26, EngineLifecycle.CreateDeviceBehaviorFlags(false));
        Assert.Equal(0x56, EngineLifecycle.CreateDeviceBehaviorFlags(true));
        Assert.Equal(32, EngineLifecycle.Direct3DSdkVersion);
        Assert.Equal(64, EngineLifecycle.IDirect3D9CreateDeviceVtbl);
        Assert.Equal(56, EngineLifecycle.IDirect3D9GetDeviceCapsVtbl);
        Assert.Equal(0x00BFEFB0u, EngineLifecycle.Direct3DCreate9Thunk);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        Assert.True(life.GraphicsCreated);
        Assert.Equal(EngineLifecycle.CreateDeviceSoftwareFlags, life.CreateDeviceFlags);
    }

    [Fact]
    public void Startup_videos_are_three_006286F0_slots()
    {
        Assert.Equal(3, EngineLifecycle.StartupVideos.Length);
        Assert.Equal(@"Data\Video\lionhead_logo.xmv", EngineLifecycle.StartupVideos[0].RelativePath);
        Assert.Equal(640, EngineLifecycle.StartupVideos[0].Width);
        Assert.Equal(400, EngineLifecycle.StartupVideos[0].Height);
        Assert.Equal(@"Data\Video\Microsoft_Logo.xmv", EngineLifecycle.StartupVideos[1].RelativePath);
        Assert.Equal(640, EngineLifecycle.StartupVideos[1].Width);
        Assert.Equal(480, EngineLifecycle.StartupVideos[1].Height);
        Assert.Equal(@"Data\Video\intro_comp.xmv", EngineLifecycle.StartupVideos[2].RelativePath);
        Assert.Equal(640, EngineLifecycle.StartupVideos[2].Width);
        Assert.Equal(360, EngineLifecycle.StartupVideos[2].Height);
        Assert.Equal(0x006286F0u, EngineLifecycle.PlayAviPlayer);
        Assert.Equal(1, EngineLifecycle.DefaultVideoPlayFlag);
        Assert.Equal(1, EngineLifecycle.DefaultVideoPlayFlag2);
    }

    [Fact]
    public void Pump_plays_videos_then_frontend_not_new_game()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        Assert.Equal(EngineStage.StartupVideos, life.Stage);
        Assert.Equal(@"Data\Video\lionhead_logo.xmv", life.CurrentStartupVideo!.Value.RelativePath);
        life.FinishStartupVideo();
        Assert.Equal(@"Data\Video\Microsoft_Logo.xmv", life.CurrentStartupVideo!.Value.RelativePath);
        life.FinishStartupVideo();
        Assert.Equal(@"Data\Video\intro_comp.xmv", life.CurrentStartupVideo!.Value.RelativePath);
        life.FinishStartupVideo();
        Assert.Equal(EngineStage.Frontend, life.Stage);
        Assert.True(life.FrontendUiPresent);
        Assert.Contains("UI_TEXT_NEW_GAME", life.FrontendMenuLabels);
        Assert.Null(life.CurrentStartupVideo);
        Assert.Null(life.WorldFileName);
        Assert.True(life.Pump());
        Assert.NotEqual(EngineStage.Game, life.Stage);
    }

    [Fact]
    public void Frontend_00595582_new_game_message_leaves_without_RequestNewGame()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.Equal(EngineStage.Frontend, life.Stage);
        Assert.True(life.FrontendUiPresent);
        Assert.True(life.FrontendMenuContains("UI_TEXT_NEW_GAME"));
        Assert.False(life.FrontendMenuContains("UI_TEXT_NOT_A_MENU"));
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.FrontendUiGet);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.FrontendUiCtor);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.FrontendMenuMissFn);
        Assert.False(life.RetailNewGameFlag);
        life.ActivateNewGame();
        Assert.True(life.RetailNewGameFlag);
        Assert.Equal(EngineStage.Frontend, life.Stage);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.FrontendNewGameApply);
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.Equal("FinalAlbion.wld", life.WorldFileName);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.LeaveFrontendSite);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
            "traces");
        Directory.CreateDirectory(dest);
        life.Trace.Write(Path.Combine(dest, "frontend-00595582.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-00595582.txt"),
            """
            00595582: singleton [0x13B8B5C]
              alloc 0xE0, ctor 005953E2, vtbl 012521A8
            00595B24 builds menu:
              UI_TEXT_NEW_GAME id=0
              UI_TEXT_LOAD_GAME id=0
              OPTIONS 24/1, VIDEO 5, SCOREBOARD 25,
              REDEFINE 22, AUDIO 4
            0059A238 message pump:
              msg==15 → 0059A2DA
                [ui+28] vtbl+16
                [retail+41]=1 (also 00594F28)
            0042EC7C: [esi+42] load/save UNREAD
                       [esi+41] Leave frontend 0042F2A2
            005959AB menu search; miss 00595A03 xor al,al
            Not 00DBDE40. Save enumerate unread.
            """);
    }

    [Fact]
    public void Frontend_0042EC7C_frame_is_input_then_0042DF9E_Present()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.Equal(0, life.FrontendFrameCount);
        Assert.True(life.Pump());
        Assert.Equal(1, life.FrontendFrameCount);
        Assert.Equal(1, life.FrontendPresentCount);
        var vas = life.Trace.Events
            .Where(e => e.Va is
                EngineLifecycle.FrontendInputFn or
                EngineLifecycle.FrontendRecordFillFn or
                EngineLifecycle.FrontendDrawFn or
                EngineLifecycle.BeginSceneFn or
                EngineLifecycle.FrontendUiDrawFn or
                EngineLifecycle.FrontendWidgetNextFn or
                EngineLifecycle.DisplayFlush2dFn or
                EngineLifecycle.DisplayFlushLayersFn or
                EngineLifecycle.FrontendDisplayHelperFn or
                EngineLifecycle.EndSceneFn or
                EngineLifecycle.PresentFn)
            .Select(e => e.Va)
            .ToArray();
        Assert.Contains(EngineLifecycle.FrontendInputFn, vas);
        Assert.Contains(EngineLifecycle.FrontendRecordFillFn, vas);
        Assert.Contains(EngineLifecycle.FrontendDrawFn, vas);
        Assert.Contains(EngineLifecycle.BeginSceneFn, vas);
        Assert.Contains(EngineLifecycle.EndSceneFn, vas);
        Assert.Contains(EngineLifecycle.PresentFn, vas);
        Assert.Equal(1, life.FrontendWidgetsDrawn);
        Assert.True(life.FrontendMenuConstructed);
        Assert.Equal(
            EngineLifecycle.FrontendMainMenuNoContinue, life.FrontendMenuRoot);
        Assert.Equal(2, life.FrontendFlushCount);
        Assert.Equal(84, EngineLifecycle.FrontendWidgetListOffset);
        Assert.Equal(8, EngineLifecycle.FrontendWidgetDrawVtbl);
        Assert.Equal(0x004292C0u, EngineLifecycle.FrontendWidgetNextFn);
        Assert.Equal(0x0041AFA0u, EngineLifecycle.FrontendWidgetDrawFn);
        Assert.Equal(0x0122F5D4u, EngineLifecycle.FrontendWidgetVtbl);
        Assert.Equal(0x0041DB1Du, EngineLifecycle.FrontendWidgetFactoryFn);
        Assert.NotEqual(0x0052D900u, EngineLifecycle.FrontendWidgetDrawFn);
        Assert.Equal(0x00404A80u, EngineLifecycle.FrontendDisplayHelperFn);
        Assert.Equal(0x013B7CD8u, EngineLifecycle.FrontendDisplaySingletonVa);
        Assert.Equal(0x0041BEB0u, EngineLifecycle.FrontendWidgetQueueFn);
        Assert.Equal(0x0041BF60u, EngineLifecycle.FrontendWidgetQueueSiblingFn);
        Assert.Equal(0x22u, EngineLifecycle.Frontend2dRecordType);
        Assert.Equal(0xC0, EngineLifecycle.Frontend2dRecordBytes);
        Assert.Equal(92, EngineLifecycle.Frontend2dSubmitVtbl);
        Assert.Equal(2, life.FrontendWidgetBlend);
        Assert.Equal(0, life.FrontendWidgetFont);
        Assert.Equal(0, life.FrontendWidgetTexture);
        Assert.Equal(1, life.Frontend2dRecordsQueued);
        Assert.Equal(0x0041BEB0u, life.Frontend2dLastPacker);
        Assert.Equal(0x22u, life.Frontend2dLastType);
        Assert.Equal(92, life.Frontend2dLastSubmitVtbl);
        Assert.False(life.FrontendDisplayFlag);
        Assert.False(life.FrontendDisplayImeRan);
        Assert.False(life.FrontendDisplayCursorRan);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendWidgetQueueFn &&
            e.Action.Contains("0041BEB0", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendDisplayHelper2Fn &&
            e.Action.Contains("skip", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendDisplayImeFn);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendWidgetQueueSiblingFn);
        var begin = Array.IndexOf(vas, EngineLifecycle.BeginSceneFn);
        var ui = Array.IndexOf(vas, EngineLifecycle.FrontendUiDrawFn);
        var flush = Array.IndexOf(vas, EngineLifecycle.DisplayFlush2dFn);
        var end = Array.IndexOf(vas, EngineLifecycle.EndSceneFn);
        var present = Array.IndexOf(vas, EngineLifecycle.PresentFn);
        Assert.True(begin >= 0 && begin < ui && ui < flush && flush < end && end < present);
        Assert.Equal(RegionTravel.PlayAviPresent, EngineLifecycle.PresentFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
            "traces");
        Directory.CreateDirectory(dest);
        life.Trace.Write(Path.Combine(dest, "frontend-0042E3EE.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-0042E3EE.txt"),
            """
            0042EC7C inner frontend loop (0042F041):
              0042E3EE input walk [0x13B8388]
              0042DC94 update dt + 00599E3F
              0042FA30 zero 112-byte record
              0042DBFA fill record (zeros + retail+204)
              0042DF9E draw:
                009D8CF0 clear
                009BEF20 BeginScene
                00595582 / 00595222 [ui+84]
                [node+20] vtbl+8 = 0041AFA0 (0122F5D4)
                not 0052D900 / 012521A8
                0041B800 [+372]=2 [+376]=0 [+380]=0
                0041BEB0 type 0x22 (not 0041BF60)
                [edx+92] dest +15C 0xC0
                009D9C80 / 009DA9F0(1)
                00404A80 / 00404C00 [+8]==0 skip
                009D9C80 / 009DA9F0(1)
                009BEF50 EndScene
                009BEEB0 IDirect3DDevice9::Present
            Same Present as PlayAVI. Vulkan Draw is
            that Present, not a second swapchain.
            00595A03 after 0042DF9E is always 0; extra
            .wmv path skipped. Not 00DBDE40.
            """);
    }

    [Fact]
    public void Frontend_present_runs_on_install_after_videos()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.Equal(EngineStage.Frontend, life.Stage);
        Assert.True(life.Pump());
        Assert.Equal(1, life.FrontendPresentCount);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PresentFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.BeginSceneFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.DisplayFlush2dFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.DisplayFlushLayersFn);
        Assert.Equal(1, life.FrontendWidgetsDrawn);
        Assert.Equal(1, life.Frontend2dRecordsQueued);
        Assert.Equal(0x0041BEB0u, life.Frontend2dLastPacker);
        Assert.False(life.FrontendDisplayFlag);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.FrontendWidgetDrawFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.FrontendWidgetFactoryFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.FrontendWidgetQueueFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Loading_objects_00521AE0_loads_LookoutPoint_tng()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.Pump());
        Assert.True(life.Pump());
        Assert.Equal("LookoutPoint", life.CurrentRegion!.RegionName);
        Assert.True(life.RegionThingMapsLoaded > 0);
        Assert.True(life.RegionThings.Count > 0, $"things={life.RegionThings.Count}");
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.LoadThingsForMapFn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.ThingManagerLoadFileFn &&
            e.Action.StartsWith("things=", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
            "traces");
        Directory.CreateDirectory(dest);
        life.Trace.Write(Path.Combine(dest, "loading-objects-00521AE0.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-00521AE0.txt"),
            """
            006C2170 Loading objects:
              00522720 thing-manager for map
              00521AE0 Thing Manager: Load From File
            Map .tng via ThingFile (loose or WAD).
            No-save first region LookoutPoint.
            Not 00DBDE40 / StartOakVale.
            """);
    }

    [Fact]
    public void Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.Pump());
        Assert.True(life.Pump());
        Assert.Equal("LookoutPoint", life.CurrentRegion!.RegionName);
        Assert.Contains(life.RegionThings, t =>
            t.DefinitionType == RegionTravel.PlayerStartType &&
            t.ScriptName == EngineLifecycle.GuildArrivalHsp);
        Assert.True(life.HeroSpawned);
        Assert.NotNull(life.Hero);
        Assert.Equal(EngineLifecycle.CreatureHeroDefName, life.Hero.DefinitionType);
        Assert.Equal(RegionTravel.AdultCreature, life.HeroDefinition);
        Assert.Equal(EngineLifecycle.HeroScriptName, life.Hero.ScriptName);
        Assert.Equal(4299, life.HeroMeshId);
        Assert.Contains(life.InsertedThings, t =>
            ReferenceEquals(t.Thing, life.Hero) && t.Drawable && t.MeshId == 4299);
        Assert.Contains(life.InsertedThings, t => t.Drawable && !ReferenceEquals(t.Thing, life.Hero));
        var start = life.RegionThings.First(t =>
            t.DefinitionType == RegionTravel.PlayerStartType &&
            t.ScriptName == EngineLifecycle.GuildArrivalHsp);
        Assert.Equal(start.PositionX, life.Hero.PositionX);
        Assert.Equal(start.PositionY, life.Hero.PositionY);
        Assert.Equal(start.PositionZ, life.Hero.PositionZ);
        Assert.Contains(life.RegionThings, t => ReferenceEquals(t, life.Hero));
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.LoadSingleThingFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.NewThingParseFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.AllocateClassFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.HolySiteFactoryFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerCreatureCreateFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.CreateCharacterFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.ActivateAfterLoadingFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerCreatureFactoryFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.ParentConstructFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.ThingConstructFromDefFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.InitCharactersFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.InitHeroDefFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.ConstructFromParamsFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.InitGuiFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.InitQuestsFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.LoadQuestsFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.ActivateInitialQuestsFn);
        Assert.True(life.PlayerGuiReady);
        Assert.True(life.QuestsInitDone);
        Assert.Contains("Q_SunnyvaleMaster", life.ActivatedQuests);
        Assert.Contains("PersonalScriptMain", life.ActivatedQuests);
        Assert.Contains("CS_PlayCutscene", life.ActivatedQuests);
        Assert.DoesNotContain(RegionTravel.IntroScriptName, life.ActivatedQuests);
        Assert.DoesNotContain(RegionTravel.IntroQuest, life.ActivatedQuests);
        Assert.NotNull(life.Runtime);
        Assert.Equal(life.ActivatedQuests.Count, life.Runtime.Quests.Count);
        Assert.Contains(life.Runtime.Quests, q => q.Name == "Q_SunnyvaleMaster" && q.Fiber is not null);
        Assert.DoesNotContain(life.Runtime.Quests, q => q.Name == RegionTravel.IntroScriptName);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(life.RegionThings, t =>
            t.DefinitionType == RegionTravel.KidCreature);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
            "traces");
        Directory.CreateDirectory(dest);
        life.Trace.Write(Path.Combine(dest, "load-single-thing-0051FD80.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-0051FD80.txt"),
            """
            00521AE0 → 00520D00 NewThing loop
              "Loading entities from script"
              0051FD80 Load Single Thing (ret 8)
            0051FD80:
              parse type; 00528760 def lookup
              [world+258] && "PlayerCreature":
                [0x13B86A0]+28 → 00449970
                004498C0 walk slots match [slot+40]
                00487DC0 add ecx,44; jmp 00A01B50
                Initial Activate vtbl+36/+40
              else:
                Allocate Class 00A371C0
                  factory table (00522A20)
                  PlayerCreature/CREATURE 0052B880
                  Holy Site/HOLY_SITE 0052AC90
                Construct Thing vtbl+64 / +16
                Initial Activate vtbl+32
            0051E5A0 Activate After Loading
              walk [manager+24]; 004C8CF0 / 004AFA60
            LookoutPoint TNG has no PlayerCreature.
            Start marker HOLY_SITE_PLAYER_START
            GuildArrivalHSP. Hero via 0049F180 Init
            Characters → 00449D90 PLAYER_HERO miss →
            CREATURE_HERO → 0048A070 / 006AC910 /
            006A9DD0 → 00662880 → 008388D0 →
            006A5950 → 004CA010 / 0042AF3C mesh 4299.
            Not 00DBDE40 / CREATURE_HERO_CHILD.
            """);
        Assert.True(life.PlayerActionReady);
        Assert.True(life.WorldUpdateRan);
        Assert.True(life.WorldFrame >= 2);
        Assert.Equal("LookoutPoint", life.FirstSceneMapName);
        Assert.Contains(life.ThingsForMap("LookoutPoint"), t =>
            t.DefinitionType == RegionTravel.PlayerStartType &&
            t.ScriptName == EngineLifecycle.GuildArrivalHsp);
        Assert.Contains(life.ThingsForMap("LookoutPoint"), t =>
            ReferenceEquals(t, life.Hero));
        Assert.NotEqual(WorldCamera.DefaultAxisX, life.WorldCamera.SlotA.V0.X);
        Assert.True((life.Camera.Position - new System.Numerics.Vector3(
            start.PositionX!.Value, start.PositionY!.Value, start.PositionZ ?? 0f)).Length() < 20f);
        Assert.DoesNotContain("StartOakVale", life.FirstSceneMapName);
        var defs = WorldGeometry.TryLoadDefs(install);
        var submit = WorldGeometry.ResolveSubmit(defs, null, life.Hero);
        Assert.True(submit.Submitted);
        Assert.Contains(4299, submit.MeshIds);
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-00662880.txt"),
            """
            006A9DD0 → 00662880 ret 28
              008388D0 arg0>0 → 006A5950
              [0x13B8A1C]+40 vtbl+64 lookup
              004CA010 bind def [thing+140/+112]
              0042AF3C / 009AD9E0 appearance
              004C7990 then 00513160
            0049F180 Init Characters
              00449D90: 009AD410 PLAYER_HERO
              miss → push CREATURE_HERO
              0048A070 InitCharacterAs → 00489D40
              006AC910 Create mesh 4299
            Not CREATURE_HERO_CHILD / 00DBDE40.
            """);
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-0049F180.txt"),
            """
            00416ABA Loading world:
              004A1840 Load Quests
                004A0D90 AddQuest/AddTestQuest → world+184
              [0x13B8648]==0 no-save → 0049F180(world)
              0049F180 Init GUI 0043A380
              Init Quests 004B4260([world+172])
            world+172 writer is 00507C30
            START_INITIAL_QUESTS:
              Q_SunnyvaleMaster, PersonalScriptMain,
              PersonalScript_GlobalThings, HeroBoasts,
              V_HeroDolls, CS_PlayCutscene
            00416BCF Activate Initial Quests
              game+90584 empty → 004B4A10 → 004B4260
            Fibers via 00A447D0 / ScriptScheduler.
            Not S_QNOVI / 00DBDE40 / Q_NewOakValeIntro.
            """);
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-first-scene.txt"),
            """
            After 0051FD80 + 004AE940:
              FirstSceneMap LookoutPoint
              ThingsForMap includes GuildArrivalHSP + Hero
              006B3FF0 SeedAt hero eye, both slots
              006B42F0 lerp t=0 stays on hero
              Client BindLifecycleFirstRegion builds
              WorldGeometry from those things.
              Not StartOakVale / 00DBDE40.
            """);
    }

    [Fact]
    public void Init_quests_004B4260_activates_wld_initial_list()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        life.EnterGame();
        Assert.True(life.QuestsInitDone);
        Assert.Equal(6, life.ActivatedQuests.Count);
        Assert.Equal(
            new[]
            {
                "Q_SunnyvaleMaster",
                "PersonalScriptMain",
                "PersonalScript_GlobalThings",
                "HeroBoasts",
                "V_HeroDolls",
                "CS_PlayCutscene",
            },
            life.ActivatedQuests);
        Assert.Contains(life.World!.InitialQuests, q => q == "Q_SunnyvaleMaster");
        Assert.NotNull(life.Quests);
        Assert.Contains(life.Quests.Quests, q => q.Name == "Q_SunnyvaleMaster" && q.Persistent);
        Assert.NotNull(life.Runtime);
        Assert.Equal(6, life.Runtime.Quests.Count);
        Assert.Equal(6, life.Runtime.Scheduler.Fibers.Count);
        Assert.All(life.Runtime.Quests, q => Assert.NotNull(q.Fiber));
        Assert.All(life.Runtime.Quests, q => Assert.True(q.Started));
        Assert.Equal(QuestFactoryTable.SunnyvaleFactory,
            life.Runtime.Quests.Single(q => q.Name == "Q_SunnyvaleMaster").Factory);
        Assert.DoesNotContain(life.ActivatedQuests, q => q == RegionTravel.IntroScriptName);
        Assert.DoesNotContain(life.ActivatedQuests, q => q == RegionTravel.IntroQuest);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.LoadQuestsFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.QstParseFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.InitQuestsFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.ActivateQuestFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.ActivateInitialQuestsFn);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
            "traces");
        Directory.CreateDirectory(dest);
        life.Trace.Write(Path.Combine(dest, "init-quests-004B4260.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-004B4260.txt"),
            """
            No-save writer of [world+172]:
              00507C30 WLD START_INITIAL_QUESTS
              Q_SunnyvaleMaster PersonalScriptMain
              PersonalScript_GlobalThings HeroBoasts
              V_HeroDolls CS_PlayCutscene
            00416ABA 004A1840 Load Quests
              004A0D90 AddQuest → world+184
            00416ABF [0x13B8648]==0
              0049F180(ecx=world) Init Quests
              004B4260([world+172])
              00CB5AD0 lookup / 00A447D0 fiber
            00416BCF empty +90584 → 004B4A10
            Not S_QNOVI / 00DBDE40.
            """);
    }

    [Fact]
    public void Window_00403079_defaults_1024x768_and_title()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        Assert.Equal(1024, life.BackBufferWidth);
        Assert.Equal(768, life.BackBufferHeight);
        Assert.Equal(16, life.BackBufferBpp);
        Assert.Equal("Fable - The Lost Chapters", life.WindowTitle);
        Assert.Equal(0, life.ViewportX);
        Assert.Equal(0, life.ViewportY);
        Assert.Equal(1024, life.ViewportWidth);
        Assert.Equal(768, life.ViewportHeight);
        Assert.Equal(0f, life.ViewportZNear);
        Assert.Equal(1f, life.ViewportZFar);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.SetViewportFn);
        Assert.Equal(EngineLifecycle.WindowTitleId, "TEXT_GUI_WINDOW_TITLE");
        Assert.True(life.BackBufferWidth >= EngineLifecycle.GraphicsMinDimension);
        Assert.True(life.BackBufferHeight >= EngineLifecycle.GraphicsMinDimension);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.DisplayWidthVa);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.WindowTitleFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.InputDeviceVa);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-window-input.txt"),
            """
            00403079 Setup library:
              [0x137545C]=1024 [0x1375460]=768
              [0x137546C]=2048 [0x1375470]=16
              009A6610 → 009C0E50 clamp min 32
            004023F0 TEXT_GUI_WINDOW_TITLE
              PE 0x122D83C UTF-16
              "Fable - The Lost Chapters"
            0042E3EE input walk [0x13B8388]
              ProbeGraphics stores engine+88
              poll 009F4ED0; events 00A03B40
              frontend New Game is msg 15
            Present remains 009BEEB0 via Vulkan Draw.
            Not 1600x900. Not WASD as game input.
            Not 00DBDE40.
            """);
    }

    [Fact]
    public void Input_0042E3EE_dispatches_0041E5F2_actions()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.Equal(EngineStage.Frontend, life.Stage);
        Assert.False(life.Input.Present);
        life.QueueInput(EngineInput.TypeKey, EngineInput.KeyMove3);
        Assert.True(life.Pump());
        Assert.True(life.Input.Present);
        Assert.Equal(EngineInput.KeyMove3, life.Input.LastKey);
        Assert.Equal(0x1, life.Input.Mask);
        Assert.Equal(new[] { 33, 0 }, life.Input.Actions);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.InputActionGetter);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.InputActionApply);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.InputBindDefaults);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);

        life.QueueInput(EngineInput.TypeKey, EngineInput.KeyMove2);
        life.Pump();
        Assert.Equal(new[] { 33, 1 }, life.Input.Actions);

        life.QueueInput(EngineInput.TypeKey, EngineInput.KeyMove0);
        life.Pump();
        Assert.Equal(new[] { 33, 2, 20 }, life.Input.Actions);

        life.QueueInput(EngineInput.TypeKey, EngineInput.KeyMove1);
        life.Pump();
        Assert.Equal(new[] { 33, 3, 21 }, life.Input.Actions);

        life.QueueInput(EngineInput.TypeKey, EngineInput.KeyDikA);
        life.Pump();
        Assert.Equal(new[] { 33, 4 }, life.Input.Actions);

        life.QueueInput(EngineInput.TypeKey, EngineInput.KeyDikB);
        life.Pump();
        Assert.Equal(new[] { 33, 5 }, life.Input.Actions);

        Assert.Equal(0x6F, EngineInput.KeyboardDefaults[0].Key);
        Assert.Equal(0x70, EngineInput.KeyboardDefaults[1].Key);
        Assert.Equal(0x72, EngineInput.KeyboardDefaults[2].Key);
        Assert.Equal(0x6D, EngineInput.KeyboardDefaults[3].Key);
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-0042E3EE.txt"),
            """
            0042E3EE only caller 0042F0AC (retail frontend).
            Event type [record+40] 00A03B40
            Event key  [record+0]  00A03B70
            Type 1: +192 = key (0042D4F7); 0055CB10(33)
              111/0x6F slot0 → mask 0x4 → actions 2,20
              112/0x70 slot1 → mask 0x8 → actions 3,21
              114/0x72 slot2 → mask 0x2 → action 1
              109/0x6D slot3 → mask 0x1 → action 0
              30/DIK_A       → mask 0x100 → action 4
              48/DIK_B       → mask 0x200 → action 5
              21/DIK_Y       → mask 0x20000 → action 22
            0041E5F2 singleton [0x13B8710] size 0xD0
              ctor 0041E3F6 vtbl 01230134
              0041DF10(0) keyboard defaults at +36
              vtbl+0 0055CB10 listener walk
            00418289 constructs the same singleton.
            Game poll 00446462 / 004963E6 unread.
            Not WASD. Not 00DBDE40.
            """);
    }

    [Fact]
    public void Game_00435530_Presents_009BEEB0_and_pumps_input()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        life.EnterGame();
        Assert.Equal(1024, life.ViewportWidth);
        Assert.Equal(768, life.ViewportHeight);
        Assert.Equal(0, life.GamePresentCount);
        var n = 0;
        while (n < 8 && life.GamePresentCount == 0)
        {
            Assert.True(life.Pump());
            n++;
        }

        Assert.True(life.WorldFrame >= 2);
        Assert.True(life.RenderBodyRan);
        Assert.True(life.GamePresentCount >= 1);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.BeginSceneFn && e.Stage == "GamePump");
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.EndSceneFn && e.Stage == "GamePump");
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PresentFn && e.Stage == "GamePump");
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GamePresentSite);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.ClearColorFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.SetViewportFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.DisplayPlayerOverlayFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.DisplayFlush2dFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.DisplayFlushLayersFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.RenderFrameFn);
        Assert.True(life.LayerFlushCount >= 1);
        Assert.Equal(
            new uint[] { 0x4, 0x40, 0x20, 0x2000 },
            life.SubmittedLayerBits);
        Assert.True(Fable.Formats.Scene.ScenePasses.Rank(0x4) <
                    Fable.Formats.Scene.ScenePasses.Rank(0x20));
        Assert.True(life.Input.Present);
        life.QueueInput(EngineInput.TypeKey, EngineInput.KeyMove3);
        life.Pump();
        Assert.True(life.Player.PumpCalls >= 1);
        Assert.True(life.Player.AcceptHits >= 1);
        Assert.Equal(0, life.Player.DeliveredCount);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-main-dx9.txt"),
            """
            Main DX9 after 00403079 / 009BF7E0:
              backbuffer [0x137545C]x[0x1375460] = 1024x768
              009BEF80 SetViewport vtbl+188
                X=0 Y=0 W=1024 H=768 MinZ=0 MaxZ=1
            Present sites (same 009BEEB0 device Present):
              00628C82 PlayAVI
              0042DF9E / 0042EC73 frontend
              00435530 / 00435F50 game display
                009BEF20 BeginScene
                009D8CF0 clear
                009BEF50 EndScene
                009BEEB0 Present
            00417001 does not Present; it calls
            00435F70 → 00435530 after WorldFrame>1.
              00435000 → 00639E40 player overlay
              00435070 player interface
              009D9C80 flush DIP vtbl+332
              009DA9F0(1) layer flush
              00B25950 bits 0x4,0x40,0x20,0x2000
            Client Draw is that Present, not a
            second swapchain.
            00416E78 [game+32].vtbl+4 00446A30
            after WorldFrame>1. Not 0042E3EE.
            Not 00DBDE40.
            """);
    }

    [Fact]
    public void Player_interface_00446A30_pumps_listeners_after_WorldFrame()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        life.EnterGame();
        Assert.True(life.Player.Present);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerInterfaceCtor);
        var n = 0;
        while (n < 8 && life.WorldFrame <= 1)
        {
            Assert.True(life.Pump());
            n++;
        }

        Assert.True(life.WorldFrame >= 2);
        Assert.Contains(life.Player.Listeners, l => l.Vtbl == ActionInputListener.VtblVa);
        life.QueueInput(EngineInput.TypeKey, EngineInput.KeyMove3);
        life.Pump();
        Assert.True(life.Player.PumpCalls >= 1);
        Assert.True(life.Player.AcceptHits >= 1);
        Assert.True(life.Player.FallbackCalls >= 1);
        Assert.Equal(0, life.Player.DeliveredCount);
        Assert.Equal(0, life.Player.OwnerDefaultResult);
        Assert.Equal(0, life.Player.LookupResult(0));
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerInputPumpFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerInputPollFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerInputFallbackFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerInterfacePreprocess);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerListenerFactoryFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GameVtbl24Fn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);

        life.Player.AddOwnerItem(0, PlayerInterface.ResultSelect);
        life.QueueInput(EngineInput.TypeKey, EngineInput.KeyMove3);
        life.Pump();
        Assert.Equal(1, life.Player.DeliveredCount);
        Assert.Equal(EngineInput.KeyMove3, life.Player.Delivered[0].Key);
        Assert.Equal(PlayerInterface.ResultSelect, life.Player.Delivered[0].Result);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerApplyFn);
        Assert.Equal(0, life.Player.QueuedCount);
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-00446A30.txt"),
            """
            Init Player Interface 004473A0
              alloc 0x898; vtbl 01231BDC
              store 004193C4 at game+32
              00A0D4A0 zeros +4 list
              [+1788] = game+28 (0044A3B0)
                +12 empty; +24 default 0
              00488D20 00687A30 vtbl 0123758C
              00687A70 → 00A0D2B0 → 00A0D4F0
            00416E78 vtbl+24 after WorldFrame>1:
              004457F0 [+2196]=0
              [game+32].vtbl+4 = 00446A30
              hit → 0041649C
            00446A30:
              00446330 009F4ED0
              skip device==2 && key==15
              skip type==0
              +4 walk:
                vtbl+32 00687DB0 accept
                  device!=1 → true
                00449990 dest+4 ([event+36])
                vtbl+28 gate 004863A0
                vtbl+16 00687FD0
                  type 1 does not 00A0D390
                dest+4==1 select
                dest+4==2 consume 009F55C0
              miss → 00446220
                +2196 one-shot
                00A0D300; 00449700
                vtbl+24 00486390 ret
                return [+168]!=0
            0041649C:
              0049D8C0 table[action] or
              00415FF2 action==2
                → 004AE9A0 +9826
                → 009F1650 player+0x2010
              always 0049E1D0 / 00434A30
            012317A8 +1960 is 00445CB0,
            not the +4 list.
            Not RecordingInputListener.
            Not 0042E3EE. Not 00DBDE40.
            """);
    }

    [Fact]
    public void Player_apply_0041649C_queues_009F1650_on_action_2()
    {
        var life = new EngineLifecycle();
        life.Player.Construct();
        life.PlayerActionReady = true;
        var ev = new PlayerEvent { Action = PlayerInterface.Action2, Type = EngineInput.TypeKey, Key = EngineInput.KeyMove3 };
        Assert.True(life.Player.ApplyInputEvent(ev, life.PlayerActionReady));
        Assert.Equal(1, life.Player.QueuedCount);
        Assert.Equal(PlayerInterface.Action2, life.Player.Queued[0].Action);
        Assert.False(life.Player.ApplyInputEvent(
            new PlayerEvent { Action = 99 }, life.PlayerActionReady));
        Assert.Equal(1, life.Player.QueuedCount);
        Assert.True(PlayerInterface.WorldTickOccupied(PlayerInterface.WorldTickSlot1));
        Assert.Equal(0x00687DB0u, ActionInputListener.AcceptFn);
        Assert.Equal(0x00687FD0u, ActionInputListener.ApplyFn);
        Assert.Equal(0x0123758Cu, ActionInputListener.VtblVa);
        Assert.Equal(0x0041649Cu, PlayerInterface.ApplyFn);
        Assert.Equal(0x009F1650u, PlayerInterface.ApplyQueueFn);
        Assert.Equal(0x00446220u, PlayerInterface.FallbackFn);
        Assert.Equal(0x00A0D4F0u, PlayerInterface.ListInsertFn);
        Assert.Equal(0x00449990u, PlayerInterface.LookupFn);
    }

    [Fact]
    public void Activate_quests_00CB5AD0_starts_factory_scripts()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        life.EnterGame();
        Assert.True(life.QuestsInitDone);
        Assert.NotNull(life.Runtime);
        var master = life.Runtime.Quests.Single(q => q.Name == "Q_SunnyvaleMaster");
        Assert.True(master.Started);
        Assert.Equal(QuestFactoryTable.SunnyvaleFactory, master.Factory);
        Assert.Equal(QuestFactoryTable.SharedRun, master.Run);
        Assert.Equal(QuestFactoryTable.SunnyvaleInit, master.Init);
        Assert.Null(master.ScriptName);
        Assert.Null(master.ChildCutscene);
        Assert.Equal(PersistKind.Bool, life.Runtime.PersistType("HauntedBarrowFieldsCompleted"));
        Assert.False(life.Runtime.PersistBool("HauntedBarrowFieldsCompleted"));
        Assert.Equal(PersistKind.Int, life.Runtime.PersistType("ArcheryHighScore"));
        Assert.Equal(0, life.Runtime.PersistInt("ArcheryHighScore"));
        Assert.Equal(PersistKind.Int, life.Runtime.PersistType("MaxChickenKickingScore"));
        Assert.False(life.Runtime.PersistBool("ArenaFinished"));
        Assert.Equal(38, PersistTable.Sunnyvale.Length);
        Assert.Equal(0x00CDC070u, PersistTable.SunnyvaleBind);
        Assert.Equal(0x004045C0u, PersistTable.BindBool);
        Assert.Equal(0x00410BE0u, PersistTable.BindInt);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.SunnyvalePersistFn);
        var boasts = life.Runtime.Quests.Single(q => q.Name == "HeroBoasts");
        Assert.True(boasts.Started);
        Assert.Equal(QuestFactoryTable.HeroBoastsFactory, boasts.Factory);
        Assert.Equal("S_HB", boasts.ScriptName);
        var personal = life.Runtime.Quests.Single(q => q.Name == "PersonalScriptMain");
        Assert.True(personal.Started);
        Assert.Equal("S_PSM", personal.ScriptName);
        var global = life.Runtime.Quests.Single(q => q.Name == "PersonalScript_GlobalThings");
        Assert.True(global.Started);
        Assert.Equal("S_PSGT", global.ScriptName);
        var dolls = life.Runtime.Quests.Single(q => q.Name == "V_HeroDolls");
        Assert.True(dolls.Started);
        Assert.Equal("S_VHDS", dolls.ScriptName);
        var play = life.Runtime.Quests.Single(q => q.Name == "CS_PlayCutscene");
        Assert.True(play.Started);
        Assert.Equal(QuestFactoryTable.PlayCutsceneFactory, play.Factory);
        Assert.Null(play.ScriptName);
        Assert.All(life.Runtime.Quests, q => Assert.True(q.Started));
        Assert.DoesNotContain(life.Runtime.Quests, q => q.Name == RegionTravel.IntroScriptName);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.ActivateQuestFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.QuestRegisterFn);
        Assert.Contains(life.Trace.Events, e => e.Va == QuestFactoryTable.SunnyvaleFactory);
        Assert.Contains(life.Trace.Events, e => e.Va == QuestFactoryTable.HeroBoastsFactory);
        if (life.Runtime.Bank?.Find("S_HB") is not null)
            Assert.True(life.Runtime.HasStarted("S_HB"));
        if (life.Runtime.Bank?.Find("S_PSM") is not null)
            Assert.True(life.Runtime.HasStarted("S_PSM"));
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
            "traces");
        Directory.CreateDirectory(dest);
        life.Trace.Write(Path.Combine(dest, "activate-quest-00CB5AD0.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-00CB5AD0.txt"),
            """
            00CD52D0 Registering Master Script
              Q_SunnyvaleMaster 00CB5C90
              factory 00CDD550 run 00CDBD20 persist 1
            00CD52D0 Registering Important Scripts
              HeroBoasts/S_HB 00CE6C40
              PersonalScriptMain/S_PSM 00CDE2F0
              PersonalScript_GlobalThings/S_PSGT 00CE19A0
              V_HeroDolls/S_VHDS 00E98640
              CS_PlayCutscene 00F01760
            004B4260 Activate Quest
              004B00C0 predicate
              00CB5AD0 lookup [manager+120]
              004BB720 enqueue 12-byte
              004B3CE0:
                run 00CDBD20 size 0x144 vtbl 012C2748
                vtbl+8 00CDBA10 zeros + _LIKE/_HATE
                factory ctor then 00CB8690 START_SCRIPT_DATA
                fiber 00A447D0
            Q_SunnyvaleMaster has no CCutsceneDef.
            00CDC070 vtbl+4 persist bind:
              004045C0 bool (HauntedBarrowFieldsCompleted +17 …)
              00410BE0 int (ArcheryHighScore +68 …)
              defaults 00CDBA10 zeros
            S_HB/S_PSM/S_PSGT/S_VHDS start when bank has them.
            Not S_QNOVI / 00DBDE40.
            """);
    }

    [Fact]
    public void CreatePlayers_004AE940_sets_plus9826_via_0099A350()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.PlayerActionReady);
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.True(life.PlayerActionReady);
        Assert.False(life.WorldUpdateRan);
        Assert.True(life.Pump());
        Assert.True(life.WorldUpdateRan);
        Assert.Equal(1, life.WorldFrame);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerObjectInit);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerObjectInitPredicate);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GameUpdateWorldFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
            "traces");
        Directory.CreateDirectory(dest);
        life.Trace.Write(Path.Combine(dest, "player-004AE940.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-004AE940.txt"),
            """
            004AE940 (ecx = game+80568 player object):
              call 0099A350
              0099A350: mov al,1; mov [ecx+4],al; ret
              ALWAYS al==1
              [esi+9825]=0
              [esi+9828/9832/9836]=0
              [esi+9848]=arg
              [esi+9826]=1
              [esi+9824]=1
              return 1
            DISPROVEN: +9826 stays 0 after Create Players.
            00418289 / 004AEBA0 therefore take the
            player / 004AEAA0 / world / vtbl+24 path
            on the first 004162B5 pump. WorldFrame
            increments via 004A5E10. Not 00DBDE40.
            """);
    }

    [Fact]
    public void New_game_is_leave_frontend_then_FinalAlbion_wld()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.Equal(EngineStage.Frontend, life.Stage);
        life.RequestNewGame();
        Assert.Equal(EngineStage.LeaveFrontend, life.Stage);
        Assert.Equal("FinalAlbion.wld", life.WorldFileName);
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.Equal(EngineMode.Game, life.Mode);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GameModeCtor);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.Equal(0x00418DCAu, EngineLifecycle.GameModeCtor);
        Assert.Equal(0x0122F180u, EngineLifecycle.GameModeVtbl);
        Assert.Equal(0x004184BDu, EngineLifecycle.GameStart);
        Assert.Equal(0x004189C2u, EngineLifecycle.GamePump);
        Assert.Equal(0x0041735Au, EngineLifecycle.InitWorldFn);
        Assert.Equal(0x004166A8u, EngineLifecycle.CreatePlayersFn);
        Assert.Equal(0x00416C8Au, EngineLifecycle.InitGraphicsFn);
        Assert.Equal(12, EngineLifecycle.InitGameStages.Length);
        Assert.Equal("Init World", EngineLifecycle.InitGameStages[7].Stage);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.InitWorldFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.InitWorldMapFn);
        Assert.Equal("Init World Map", EngineLifecycle.InitWorldInitStages[0].Stage);
        Assert.Equal(0x005066E0u, EngineLifecycle.InitWorldMapFn);
        Assert.Contains(life.Trace.Events, e => e.Action == "Init Thing Components");
        Assert.Contains(life.Trace.Events, e => e.Action == "Init Thing Manager");
        Assert.Equal(0x161E8, EngineLifecycle.GameModeSize);
        Assert.Equal(0x148, EngineLifecycle.RetailModeSize);
        Assert.Equal(0x01230CA0u, EngineLifecycle.RetailModeVtbl);
        Assert.Equal(0x0042F75Eu, EngineLifecycle.RetailStart);
        Assert.Equal(0x0042EC7Cu, EngineLifecycle.RetailPump);
        Assert.Null(life.World);
        Assert.Null(life.Gtng);
        Assert.Equal(5, life.PlayerSlotsCreated);
        Assert.Equal(4, life.PlayerActiveCount);
        Assert.True(life.PlayerObjectReady);
        Assert.True(life.WorldCameraPresent);
        Assert.True(life.GameCamera.Constructed);
        Assert.True(life.GameCameraManager.Constructed);
        Assert.Equal(22, life.GameCamera.Plus176);
        Assert.Equal(0x0125D53Cu, life.WorldCamera.VtblValue);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.WorldCameraCtor);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GameCameraCtor);
        Assert.Null(life.CurrentRegion);
        Assert.False(life.GamePumpFirstDone);
        Assert.True(life.Pump());
        Assert.True(life.GamePumpFirstDone);
        Assert.Null(life.CurrentRegion);
        Assert.Equal(0, life.CurrentRegionIndex);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.WorldGetMapFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GetCurrentRegionIndexFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GetRegionRecordFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.NamedStartFn);
        Assert.True(life.EngineUpdateAllowed);
        Assert.Equal(1, life.GameUpdateCount);
        Assert.Equal(1, life.GameRenderCount);
        Assert.True(life.GameRenderEnabled);
        Assert.True(life.PlayerActionReady);
        Assert.False(life.FadeUiActive);
        Assert.True(life.WorldUpdateRan);
        Assert.True(life.GameVtbl24Ran);
        Assert.False(life.RenderBodyRan);
        Assert.Equal(1, life.WorldFrame);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.EngineUpdateGateFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GameUpdateFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GameRenderFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GameUpdatePlayerFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GameUpdateWorldFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerObjectInitPredicate);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.StoreActiveThingFn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.GameRenderFn &&
            e.Action.Contains("WorldFrame<=1", StringComparison.Ordinal));
        Assert.True(life.Pump());
        Assert.Equal(2, life.GameUpdateCount);
        Assert.Equal(2, life.GameRenderCount);
    }

    [Fact]
    public void Inner_frame_009A57B0_skips_when_library_not_constructed()
    {
        var life = new EngineLifecycle();
        Assert.False(life.GraphicsCreated);
        life.PumpGameUpdate();
        Assert.False(life.EngineUpdateAllowed);
        Assert.Equal(0, life.GameUpdateCount);
        Assert.Equal(0, life.GameRenderCount);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.EngineUpdateGateFn &&
            e.Action.Contains("skip", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.GameUpdateFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.GameRenderFn);
    }

    [Fact]
    public void Update_00418289_player_flag_runs_world_and_vtbl24()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.True(life.Pump());
        Assert.True(life.PlayerActionReady);
        Assert.True(life.WorldUpdateRan);
        life.UpdateGameMode();
        Assert.True(life.WorldUpdateRan);
        Assert.True(life.GameVtbl24Ran);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerActionFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GameUpdateWorldFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GameVtbl24Fn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Render_00417001_camera_body_requires_WorldFrame_above_1()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.True(life.Pump());
        Assert.Equal(1, life.WorldFrame);
        Assert.False(life.RenderBodyRan);
        Assert.True(life.Pump());
        Assert.Equal(2, life.WorldFrame);
        Assert.True(life.RenderBodyRan);
        Assert.True(life.CameraInterpolationRan);
        Assert.False(life.CameraInterpolationUnread);
        Assert.Equal(0f, life.CameraInterpolationT);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.CameraInterpolationFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.WorldCameraApplyFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.DisplayApplyBodyFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.CameraBodyFn);
    }

    [Fact]
    public void WorldFrame_004A5E10_unblocks_004164E0()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.True(life.Pump());
        Assert.True(life.Pump());
        Assert.Contains(1, life.GameTickTypes);
        Assert.True(life.PlayerActionReady);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.WorldFrameIncSite);
        Assert.Equal(2, life.WorldFrame);
        Assert.True(life.RenderBodyRan);
        Assert.True(life.CameraInterpolationRan);
        Assert.False(life.CameraInterpolationUnread);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.CameraInterpolationFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.WorldCameraApplyFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.WorldTickFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.AdvanceGameTicksFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.CameraBodyFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Camera_004164E0_steps_arg_over_15_when_plus72_ahead()
    {
        var life = new EngineLifecycle();
        life.GamePlus72 = 1;
        life.GamePlus80 = 0;
        life.ApplyCameraBody(15);
        Assert.Equal(1, life.LastCameraLoopCount);
        Assert.Equal(1, life.CameraBodySteps);
        Assert.Equal(1, life.GamePlus80);
        Assert.Equal(1, life.GamePlus72);
        Assert.Equal(1f / 15f, life.LastCameraTime);
        Assert.Equal(0f, life.LastCameraBlend);
        Assert.Equal(1f / 15f, life.GamePlus112);
        Assert.Equal(0f, life.GamePlus116);
        Assert.Equal(0f, life.GamePlus120);
        Assert.Equal(1, life.GamePlus160);
        Assert.Equal(1, life.GamePlus90424);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.CameraBodyFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.FistpFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.WorldCameraApplyFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.CameraManagerBlendFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.ThingWalkApplyFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.DisplayApplyThunk);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.DisplayApplyBodyFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.CameraTimeFn);
        Assert.NotNull(life.Camera);
    }

    [Fact]
    public void Camera_004164E0_skips_when_plus80_catches_plus72()
    {
        var life = new EngineLifecycle();
        life.ApplyCameraBody(15);
        Assert.Equal(1, life.LastCameraLoopCount);
        Assert.Equal(0, life.CameraBodySteps);
        Assert.Equal(0, life.GamePlus80);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.CameraBodyFn &&
            e.Action.Contains("skip", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.WorldCameraApplyFn);
    }

    [Fact]
    public void Camera_004164E0_two_steps_when_arg_is_30()
    {
        var life = new EngineLifecycle();
        life.GamePlus72 = 4;
        life.ApplyCameraBody(30);
        Assert.Equal(2, life.LastCameraLoopCount);
        Assert.Equal(2, life.CameraBodySteps);
        Assert.Equal(4, life.GamePlus80);
        Assert.Equal(2f / 30f, life.LastCameraTime);
        Assert.Equal(0.5f, life.LastCameraBlend);
        Assert.Equal(1f / 30f, life.GamePlus120);
        Assert.Equal(0f, life.GamePlus124);
    }

    [Fact]
    public void Render_00417001_clamps_catchup_ticks_and_runs_004164E0()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.True(life.Pump());
        Assert.Equal(1, life.WorldFrame);
        Assert.False(life.RenderBodyRan);
        life.WorldFrame = 2;
        life.CameraCatchupTicks = 3;
        life.GamePlus72 = 1;
        life.RenderGameMode();
        Assert.Equal(15, life.CameraCatchupTicks);
        Assert.True(life.GamePlus90594);
        Assert.Equal(1, life.CameraBodySteps);
        Assert.Equal(1, life.GamePlus80);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.CameraCatchupTicksVa);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.CameraBodyFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.WorldCameraApplyFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.CameraInterpolationFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.False(life.RegionObjectPresent);
    }

    [Fact]
    public void Camera_0041707E_clamps_t_and_skips_unread_world164()
    {
        var life = new EngineLifecycle();
        life.DisplayTime = 2.0 / 15.0;
        life.GamePlus72 = 0;
        life.ApplyCameraInterpolation();
        Assert.True(life.CameraInterpolationRan);
        Assert.Equal(1f, life.CameraInterpolationT);
        Assert.True(life.GamePlus90594);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.CameraClampFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.WorldCameraApplyFn);

        var unread = new EngineLifecycle { WorldPlus164 = 1 };
        unread.ApplyCameraInterpolation();
        Assert.True(unread.CameraInterpolationUnread);
        Assert.False(unread.CameraInterpolationRan);
        Assert.DoesNotContain(unread.Trace.Events, e => e.Va == EngineLifecycle.WorldCameraApplyFn);
        Assert.Equal(new System.Numerics.Vector3(1f, 0f, 0f), life.Camera.Position);
        Assert.Equal(new System.Numerics.Vector3(1f, 0f, 0f), life.Camera.LookAt);
    }

    [Fact]
    public void World_camera_006B4900_slots_lerp_into_ScriptedCamera()
    {
        var cam = new WorldCamera();
        cam.Construct();
        Assert.Equal(0x0125D53Cu, cam.VtblValue);
        Assert.False(cam.Seeded);
        Assert.Equal(new System.Numerics.Vector3(1f, 0f, 0f), cam.SlotA.V0);
        Assert.Equal(0.2f, cam.SlotA.Weight0);
        var at0 = cam.Blend(0f);
        Assert.True(cam.Seeded);
        Assert.Equal(System.Numerics.Vector3.Zero, at0.V0);
        var at1 = cam.Blend(1f);
        Assert.Equal(new System.Numerics.Vector3(1f, 0f, 0f), at1.V0);
        var mid = cam.Blend(0.5f);
        Assert.Equal(0.5f, mid.V0.X);
        cam.SeedAt(
            new System.Numerics.Vector3(4f, 5f, 6f),
            new System.Numerics.Vector3(7f, 8f, 9f),
            System.Numerics.Vector3.UnitZ);
        Assert.Equal(new System.Numerics.Vector3(4f, 5f, 6f), cam.SlotA.V0);
        Assert.Equal(new System.Numerics.Vector3(4f, 5f, 6f), cam.SlotB.V0);
        Assert.Equal(new System.Numerics.Vector3(4f, 5f, 6f), cam.Blend(0f).V0);
        cam.WriteTarget(
            new System.Numerics.Vector3(10f, 20f, 30f),
            new System.Numerics.Vector3(11f, 21f, 31f),
            System.Numerics.Vector3.UnitZ);
        var snapped = cam.Blend(1f);
        Assert.Equal(new System.Numerics.Vector3(10f, 20f, 30f), snapped.V0);
        Assert.Equal(new System.Numerics.Vector3(11f, 21f, 31f), snapped.V1);
    }

    [Fact]
    public void Game_pump_is_004189C2_not_00DBDE40()
    {
        Assert.Equal(0x004189C2u, EngineLifecycle.GamePump);
        Assert.Equal(0x004AE8C0u, EngineLifecycle.WorldGetMapFn);
        Assert.Equal(52, EngineLifecycle.WorldGetMapVtbl);
        Assert.Equal(20, EngineLifecycle.WorldMapFieldOffset);
        Assert.Equal(0x004FB150u, EngineLifecycle.GetCurrentRegionIndexFn);
        Assert.Equal(156, EngineLifecycle.WorldMapCurrentRegionIndexOffset);
        Assert.Equal(0x004FC180u, EngineLifecycle.GetRegionRecordFn);
        Assert.Equal(44, EngineLifecycle.WorldMapRegionTableOffset);
        Assert.Equal(88, EngineLifecycle.NewRegionRecordSize);
        Assert.Equal(36, EngineLifecycle.NewRegionObjectOffset);
        Assert.Equal(0, EngineLifecycle.DefaultNamedStartFlag);
        Assert.Equal(0x00416268u, EngineLifecycle.NamedStartFn);
        Assert.Equal(0x004162B5u, EngineLifecycle.GamePumpUpdate);
        Assert.Equal(0x009A57B0u, EngineLifecycle.EngineUpdateGateFn);
        Assert.Equal(148, EngineLifecycle.EngineTickOffset);
        Assert.Equal(0x00418289u, EngineLifecycle.GameUpdateFn);
        Assert.Equal(20, EngineLifecycle.GameUpdateVtbl);
        Assert.Equal(0x00417001u, EngineLifecycle.GameRenderFn);
        Assert.Equal(28, EngineLifecycle.GameRenderVtbl);
        Assert.Equal(0x009E9FB0u, EngineLifecycle.DisplayReadyFn);
        Assert.Equal(0x004AEBA0u, EngineLifecycle.GameUpdatePlayerFn);
        Assert.Equal(0x0049D9E0u, EngineLifecycle.GameUpdateWorldFn);
        Assert.Equal(0x00418EC6u, EngineLifecycle.GameModeCtorRenderEnable);
        Assert.Equal(90593, EngineLifecycle.GameRenderEnableOffset);
        Assert.Equal(0x00416296u, EngineLifecycle.FrontEndQueryFn);
        Assert.Equal(0x00490A22u, EngineLifecycle.GuiBlockQueryFn);
        Assert.Equal(0x004AEAA0u, EngineLifecycle.PlayerActionFn);
        Assert.Equal(9826, EngineLifecycle.PlayerActionFlagOffset);
        Assert.Equal(0x00416E78u, EngineLifecycle.GameVtbl24Fn);
        Assert.Equal(0x0049D870u, EngineLifecycle.WorldFrameGetter);
        Assert.Equal(0x004C74F0u, EngineLifecycle.StoreActiveThingFn);
        Assert.Equal(0x0049E1B0u, EngineLifecycle.WorldGetThingFn);
        Assert.Equal(0x00415A60u, EngineLifecycle.RenderStackZeroFn);
        Assert.Equal(0x004A5E10u, EngineLifecycle.WorldFrameIncSite);
        Assert.Equal(0x004A5A40u, EngineLifecycle.WorldTickFn);
        Assert.Equal(0x00629270u, EngineLifecycle.WorldTickThunk);
        Assert.Equal(0x0041726Du, EngineLifecycle.AdvanceGameTicksFn);
        Assert.Equal(0x0049DFB0u, EngineLifecycle.DispatchWorldCallbacksFn);
        Assert.Equal(0x004164E0u, EngineLifecycle.CameraBodyFn);
        Assert.Equal(0x013B8630u, EngineLifecycle.CameraCatchupTicksVa);
        Assert.Equal(0x01375550u, EngineLifecycle.CameraCatchupMinVa);
        Assert.Equal(15, EngineLifecycle.CameraCatchupMin);
        Assert.Equal(0x0122EDB8u, EngineLifecycle.CameraStepScaleVa);
        Assert.Equal(1.0 / 15.0, EngineLifecycle.CameraStepScale);
        Assert.Equal(0x00BFEA70u, EngineLifecycle.FistpFn);
        Assert.Equal(0x0049E080u, EngineLifecycle.WorldCameraApplyFn);
        Assert.Equal(0x006B42F0u, EngineLifecycle.CameraManagerBlendFn);
        Assert.Equal(0x00435F70u, EngineLifecycle.DisplayApplyThunk);
        Assert.Equal(0x00435530u, EngineLifecycle.DisplayApplyBodyFn);
        Assert.Equal(0x0041707Eu, EngineLifecycle.CameraInterpolationFn);
        Assert.Equal(0x004166E2u, EngineLifecycle.CameraInterpTimeFn);
        Assert.Equal(0x0041919Cu, EngineLifecycle.CameraClampFn);
        Assert.Equal(0x004AEA70u, EngineLifecycle.PlayerReadyQueryFn);
        Assert.Equal(0x006B4900u, EngineLifecycle.WorldCameraCtor);
        Assert.Equal(0x0125D53Cu, EngineLifecycle.WorldCameraVtbl);
        Assert.Equal(0x1970, EngineLifecycle.WorldCameraObjectSize);
        Assert.Equal(24, EngineLifecycle.WorldCameraOffset);
        Assert.Equal(0x006FD8C0u, EngineLifecycle.GameCameraCtor);
        Assert.Equal(0x01264A8Cu, EngineLifecycle.GameCameraVtbl);
        Assert.Equal(0xC8, EngineLifecycle.GameCameraObjectSize);
        Assert.Equal(44, EngineLifecycle.GameCameraOffset);
        Assert.Equal(0x0069AE80u, EngineLifecycle.GameCameraManagerCtor);
        Assert.Equal(0x160, EngineLifecycle.GameCameraManagerObjectSize);
        Assert.Equal(22, EngineLifecycle.FistpTowardZero(
            EngineLifecycle.CameraCatchupMin * GameCamera.Scale));
        Assert.Equal(13, EngineLifecycle.CameraRecordDwords);
        Assert.Equal(52, EngineLifecycle.CameraRecordSize);
        Assert.Equal(1, EngineLifecycle.FistpTowardZero(15 * EngineLifecycle.CameraStepScale));
        Assert.Equal(2, EngineLifecycle.FistpTowardZero(30 * EngineLifecycle.CameraStepScale));
        Assert.Equal(0, EngineLifecycle.FistpTowardZero(14 * EngineLifecycle.CameraStepScale));
        Assert.Equal(1, EngineLifecycle.WorldTickType);
        Assert.Equal(1, EngineLifecycle.RegionTableDummyCount);
        Assert.Equal(0x00500540u, EngineLifecycle.LoadRegionFn);
        Assert.Equal(0x00522720u, EngineLifecycle.LoadThingsForMapFn);
        Assert.Equal(0x00521AE0u, EngineLifecycle.ThingManagerLoadFileFn);
        Assert.Equal(0x006C2120u, EngineLifecycle.EnqueueLoadJobFn);
        Assert.Equal(0x006C2710u, EngineLifecycle.LevelLoaderUpdate);
        Assert.Equal(0x006C2170u, EngineLifecycle.LevelLoaderApply);
        Assert.Equal(0x004FC8A0u, EngineLifecycle.SetRegionAsLoadedFn);
        Assert.Equal(0x004FCBB0u, EngineLifecycle.ActivateTopologyFn);
        Assert.Equal(188, EngineLifecycle.WorldMapLevelLoaderOffset);
        Assert.Equal(0x004FC210u, EngineLifecycle.FindRegionByNameFn);
        Assert.Equal(0x00487C20u, EngineLifecycle.LoadRegionByNameFn);
        Assert.Equal(0x00449E60u, EngineLifecycle.LoadRegionByNamePersist);
        Assert.Equal(0x00501450u, EngineLifecycle.LoadFromFirstRealRegionFn);
        Assert.Equal(0x00B42750u, EngineLifecycle.OpenStaticMapsFn);
        Assert.Equal(0x00B428E0u, EngineLifecycle.SetStaticMapFileForUseFn);
        Assert.Equal(1, EngineLifecycle.OpenStaticMapsUseMode);
        Assert.Equal(2, EngineLifecycle.OpenStaticMapsListMode);
        Assert.Equal(424, EngineLifecycle.OpenStaticMapsModeOffset);
        Assert.Equal(0x00B3EFA0u, EngineLifecycle.ParseMapHeaderFn);
        Assert.Equal(0x00B3EF40u, EngineLifecycle.CloseStaticMapFn);
        Assert.Equal(0x00B40000u, EngineLifecycle.CloseStaticMapFileFn);
        Assert.Equal(0x00BE03A0u, EngineLifecycle.CreateBackgroundPatchFn);
        Assert.Equal(0x00BDD0E0u, EngineLifecycle.BuildCurrentPatchFn);
        Assert.Equal(0x00595582u, EngineLifecycle.FrontendUiGet);
        Assert.Equal(0x005953E2u, EngineLifecycle.FrontendUiCtor);
        Assert.Equal(0x012521A8u, EngineLifecycle.FrontendUiVtbl);
        Assert.Equal(0xE0, EngineLifecycle.FrontendUiSize);
        Assert.Equal(0x00595B24u, EngineLifecycle.FrontendUiBuildMenu);
        Assert.Equal(0x0059A238u, EngineLifecycle.FrontendUiMessageFn);
        Assert.Equal(15, EngineLifecycle.FrontendNewGameMessage);
        Assert.Equal(41, EngineLifecycle.RetailNewGameFlagOffset);
        Assert.Equal(0x00595A03u, EngineLifecycle.FrontendMenuMissFn);
        Assert.Equal("UI_TEXT_NEW_GAME", EngineLifecycle.FrontendMenuItems[0].Label);
        Assert.Equal(0x0042E3EEu, EngineLifecycle.FrontendInputFn);
        Assert.Equal(0x013B8388u, EngineLifecycle.InputDeviceVa);
        Assert.Equal(0x009F4ED0u, EngineLifecycle.InputPollFn);
        Assert.Equal(0x00A03B40u, EngineLifecycle.InputEventFn);
        Assert.Equal(0x0041E5F2u, EngineLifecycle.InputActionGetter);
        Assert.Equal(0x0041E3F6u, EngineLifecycle.InputActionCtor);
        Assert.Equal(0x01230134u, EngineLifecycle.InputActionVtbl);
        Assert.Equal(0x0055CB10u, EngineLifecycle.InputActionApply);
        Assert.Equal(0x0041DF10u, EngineLifecycle.InputBindDefaults);
        Assert.Equal(0x00A03B70u, EngineLifecycle.InputEventKeyFn);
        Assert.Equal(0x00435F50u, EngineLifecycle.GamePresentSite);
        Assert.Equal(0x009BEF80u, EngineLifecycle.SetViewportFn);
        Assert.Equal(188, EngineLifecycle.SetViewportVtbl);
        Assert.Equal(0x00446A30u, EngineLifecycle.PlayerInputPumpFn);
        Assert.Equal(0x00446330u, EngineLifecycle.PlayerInputPollFn);
        Assert.Equal(0x00446220u, EngineLifecycle.PlayerInputFallbackFn);
        Assert.Equal(0x0041649Cu, EngineLifecycle.PlayerApplyFn);
        Assert.Equal(0x009F1650u, EngineLifecycle.PlayerApplyQueueFn);
        Assert.Equal(0x0123758Cu, EngineLifecycle.PlayerListenerVtbl);
        Assert.Equal(0x00687DB0u, EngineLifecycle.PlayerListenerAcceptFn);
        Assert.Equal(0x004473A0u, EngineLifecycle.PlayerInterfaceCtor);
        Assert.Equal(0x01231BDCu, EngineLifecycle.PlayerInterfaceVtbl);
        Assert.Equal(0x004457F0u, EngineLifecycle.PlayerInterfacePreprocess);
        Assert.Equal(0x898, PlayerInterface.ObjectSize);
        Assert.Equal(32, PlayerInterface.GameOffset);
        Assert.Equal(4, PlayerInterface.PumpVtbl);
        Assert.Equal(0x00435000u, EngineLifecycle.DisplayPlayerOverlayFn);
        Assert.Equal(0x00639E40u, EngineLifecycle.DisplayPlayerOverlayApply);
        Assert.Equal(0x00435070u, EngineLifecycle.DisplayPlayerInterfaceFn);
        Assert.Equal(0x009D9C80u, EngineLifecycle.DisplayFlush2dFn);
        Assert.Equal(0x009DA9F0u, EngineLifecycle.DisplayFlushLayersFn);
        Assert.Equal(1, EngineLifecycle.DisplayFlushLayersArg);
        Assert.Equal(332, EngineLifecycle.DrawIndexedPrimitiveVtbl);
        Assert.Equal(8, EngineLifecycle.DisplaySubmitStages.Length);
        Assert.Equal(0x009BEF20u, EngineLifecycle.DisplaySubmitStages[0].Va);
        Assert.Equal(0x009BEEB0u, EngineLifecycle.DisplaySubmitStages[^1].Va);
        Assert.Equal(0xD0, EngineInput.ObjectSize);
        Assert.Equal(0x6F, EngineInput.KeyMove0);
        Assert.Equal(0x1E, EngineInput.KeyDikA);
        Assert.Equal(0x004023F0u, EngineLifecycle.WindowTitleFn);
        Assert.Equal(0x0122D83Cu, EngineLifecycle.WindowTitleVa);
        Assert.Equal("TEXT_GUI_WINDOW_TITLE", EngineLifecycle.WindowTitleId);
        Assert.Equal("Fable - The Lost Chapters", EngineLifecycle.WindowTitleDefault);
        Assert.Equal(1024, EngineLifecycle.DisplayDefaultWidth);
        Assert.Equal(768, EngineLifecycle.DisplayDefaultHeight);
        Assert.Equal(32, EngineLifecycle.GraphicsMinDimension);
        Assert.Equal(16, EngineLifecycle.DisplayDefaultBpp);
        Assert.Equal(0x0137545Cu, EngineLifecycle.DisplayWidthVa);
        Assert.Equal(0x01375460u, EngineLifecycle.DisplayHeightVa);
        Assert.Equal(0x00CD52D0u, EngineLifecycle.QuestRegisterFn);
        Assert.Equal(0x004BB720u, EngineLifecycle.QuestFactoryCollectFn);
        Assert.Equal(0x004B3CE0u, EngineLifecycle.QuestFactoryStartFn);
        Assert.Equal(0x0042DC94u, EngineLifecycle.FrontendUpdateFn);
        Assert.Equal(0x0042FA30u, EngineLifecycle.FrontendRecordZeroFn);
        Assert.Equal(0x0042DBFAu, EngineLifecycle.FrontendRecordFillFn);
        Assert.Equal(0x0042DF9Eu, EngineLifecycle.FrontendDrawFn);
        Assert.Equal(0x00595222u, EngineLifecycle.FrontendUiDrawFn);
        Assert.Equal(RegionTravel.PlayAviBeginScene, EngineLifecycle.BeginSceneFn);
        Assert.Equal(RegionTravel.PlayAviEndScene, EngineLifecycle.EndSceneFn);
        Assert.Equal(RegionTravel.PlayAviPresent, EngineLifecycle.PresentFn);
        Assert.Equal(0x009D8CF0u, EngineLifecycle.ClearColorFn);
        Assert.Equal(112, EngineLifecycle.FrontendRecordSize);
        Assert.Equal(25, EngineLifecycle.LevHeaderVersion);
        Assert.Equal(0x1904u, EngineLifecycle.LevHeaderConstant);
        Assert.NotEqual(0x00DBDE40u, EngineLifecycle.GamePump);
        Assert.NotEqual(RegionTravel.StartOakValeSetup, EngineLifecycle.GetRegionRecordFn);
        Assert.NotEqual(RegionTravel.StartOakValeSetup, EngineLifecycle.SetRegionAsLoadedFn);
    }

    [Fact]
    public void Persist_PlayerRegionName_is_00487C20_not_new_game()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle { PlayerRegionName = "StartOakVale" };
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.True(life.Pump());
        Assert.Equal(0, life.CurrentRegionIndex);
        Assert.Null(life.CurrentRegion);
        Assert.True(life.Pump());
        Assert.Equal(4, life.CurrentRegionIndex);
        Assert.Equal("StartOakVale", life.CurrentRegion!.RegionName);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.LoadRegionByNameFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.FindRegionByNameFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.LoadFromFirstRealRegionFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Load_wld_is_00507C30_not_00DBDE40()
    {
        Assert.Equal(0x00507C30u, EngineLifecycle.LoadWldFile);
        Assert.Equal(0x005066E0u, EngineLifecycle.InitWorldMapFn);
        Assert.NotEqual(RegionTravel.StartOakValeSetup, EngineLifecycle.LoadWldFile);
        Assert.Contains("NewMap", EngineLifecycle.LoadWldTokens);
        Assert.Contains("SeesMap", EngineLifecycle.LoadWldTokens);
        Assert.Contains("ThingManagerUIDCount", EngineLifecycle.LoadWldTokens);
    }

    [Fact]
    public void CreatePlayers_is_five_0x22C_slots_not_hero_swap()
    {
        Assert.Equal(0x004166A8u, EngineLifecycle.CreatePlayersFn);
        Assert.Equal(0x0044A530u, EngineLifecycle.PlayerManagerApply);
        Assert.Equal(0x0044A1A0u, EngineLifecycle.CreatePlayerSlotFn);
        Assert.Equal(0x0044BC10u, EngineLifecycle.CreatePlayerSlotCtor);
        Assert.Equal(0x22C, EngineLifecycle.CreatePlayerSlotSize);
        Assert.Equal(5, EngineLifecycle.CreatePlayerSlotCount);
        Assert.Equal(4, EngineLifecycle.CreatePlayerActiveCount);
        Assert.Equal(0x004AE940u, EngineLifecycle.PlayerObjectInit);
        Assert.Equal(0x0044C6B0u, EngineLifecycle.PlayerManagerGetter);
        Assert.NotEqual(0x0044A3B0u, EngineLifecycle.CreatePlayersFn);
    }

    [Fact]
    public void Gtng_is_stem_gtng_gtg_is_004FE2A0_single_file()
    {
        Assert.Equal(".gtng", EngineLifecycle.GtngExtension);
        Assert.Equal(".gtg", EngineLifecycle.GtgExtension);
        Assert.Equal(0x01244BB4u, EngineLifecycle.GtngExtVa);
        Assert.Equal(0x01244BDCu, EngineLifecycle.GtgExtVa);
        Assert.Equal(0x004FE2A0u, EngineLifecycle.LoadGlobalThingsSingle);
        Assert.Equal(0x004FDBC0u, EngineLifecycle.LoadGlobalThingsPerMap);
        Assert.Equal(0, EngineLifecycle.DefaultSingleGlobalThingsFlag);
        Assert.False(new EngineLifecycle().SingleGlobalThingsFile);
    }

    [Fact]
    public void Install_banks_and_startup_videos_exist()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        Assert.Contains(life.Trace.Events, e =>
            e.Action.Equals("GBANK_MAIN / GBANK_MAIN_PC", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Action.StartsWith("present GBANK_MAIN_PC", StringComparison.Ordinal));
        Assert.Contains("GBANK_MAIN_PC", life.RegisteredBanks);
        foreach (var video in EngineLifecycle.StartupVideos)
        {
            var file = RegionTravel.ResolvePlayAviFile(install, video.RelativePath);
            Assert.True(file is not null && File.Exists(file), video.RelativePath);
        }

        Assert.True(File.Exists(install.WorldPath));
        Assert.Equal("FinalAlbion.wld", Path.GetFileName(install.WorldPath));
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        life.EnterGame();
        Assert.NotNull(life.World);
        Assert.Equal(EngineLifecycle.LoadWldFile, 0x00507C30u);
        Assert.Equal(0xD8, EngineLifecycle.WorldMapObjectSize);
        Assert.Equal(5, EngineLifecycle.WorldMapCellShift);
        Assert.Equal(0x2000, EngineLifecycle.WorldMapBound);
        Assert.Equal(0x01244AECu, EngineLifecycle.WorldMapVtbl);
        Assert.Contains("NewRegion", EngineLifecycle.LoadWldTokens);
        Assert.Contains("ContainsMap", EngineLifecycle.LoadWldTokens);
        Assert.Contains("START_INITIAL_QUESTS", EngineLifecycle.LoadWldTokens);
        Assert.True(life.World.Maps.Count >= 70);
        var oak = life.World.FindRegionContaining("StartOakValeWest");
        Assert.NotNull(oak);
        Assert.Equal("StartOakVale", oak.RegionName);
        Assert.Contains("StartOakValeWest", oak.ContainsMaps);
        Assert.Contains("StartOakValeEast", oak.ContainsMaps);
        Assert.Contains("StartOakvaleMemorialGarden", oak.ContainsMaps);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.LoadWldFile && e.Action.StartsWith("maps=", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.LoadGtng);
        Assert.Null(life.Gtng);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.LoadGtng && e.Action.StartsWith("missing", StringComparison.Ordinal));
        Assert.Equal(5, life.PlayerSlotsCreated);
        Assert.Equal(4, life.PlayerActiveCount);
        Assert.True(life.PlayerObjectReady);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.CreatePlayersFn && e.Action.StartsWith("slots=5", StringComparison.Ordinal));
        Assert.NotNull(life.Regions);
        Assert.Equal(0x00506D40u, EngineLifecycle.LoadRegionGraphFn);
        Assert.Equal(0x00828710u, EngineLifecycle.InitRegionGraphFn);
        Assert.True(life.Regions.Neighbors.Count >= 80);
        Assert.Contains("PicnicArea", life.Regions.NeighborsOf("LookoutPoint"));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.LoadRegionGraphFn &&
            e.Action.StartsWith("nodes=", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.Null(life.CurrentRegion);
        Assert.True(life.Pump());
        Assert.True(life.GamePumpFirstDone);
        Assert.Equal(0, life.CurrentRegionIndex);
        Assert.Null(life.CurrentRegion);
        Assert.Equal(1, life.World.Regions[0].Index);
        Assert.Equal("LookoutPoint", life.World.Regions[0].RegionName);
        Assert.Equal(4, life.World.Regions[3].Index);
        Assert.Equal("StartOakVale", life.World.Regions[3].RegionName);
        Assert.Same(life.World.Regions[0], life.RegionAtNativeIndex(1));
        Assert.Same(life.World.Regions[3], life.RegionAtNativeIndex(4));
        Assert.Null(life.RegionAtNativeIndex(0));
        Assert.False(life.RegionObjectPresent);
        Assert.Empty(life.ActivatedMaps);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GamePump);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.WorldGetMapFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GetCurrentRegionIndexFn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.GetRegionRecordFn &&
            e.Action.Contains("dummy", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.SetRegionAsLoadedFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GamePumpUpdate);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GameUpdateFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GameRenderFn);
        Assert.True(life.EngineUpdateAllowed);
        Assert.Equal(1, life.GameUpdateCount);
        Assert.Equal(1, life.GameRenderCount);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.NamedStartFn);
        Assert.False(life.FirstRealRegionLoadDone);
        Assert.True(string.IsNullOrEmpty(life.PlayerRegionName));

        Assert.True(life.Pump());
        Assert.True(life.FirstRealRegionLoadDone);
        Assert.Equal(2, life.GameUpdateCount);
        Assert.Equal(2, life.GameRenderCount);
        Assert.Equal(2, life.WorldFrame);
        Assert.Contains(1, life.GameTickTypes);
        Assert.Equal(1, life.CurrentRegionIndex);
        Assert.NotNull(life.CurrentRegion);
        Assert.Equal("LookoutPoint", life.CurrentRegion.RegionName);
        Assert.Contains("LookoutPoint", life.ActivatedMaps);
        Assert.Contains("BowerstoneBridge", life.ActivatedMaps);
        Assert.Contains("GuildExterior", life.ActivatedMaps);
        Assert.Empty(life.PendingLoadIndices);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.LoadFromFirstRealRegionFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.LoadRegionFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.EnqueueLoadJobFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.LoadRegionByNameFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.LevelLoaderUpdate);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.ActivateTopologyFn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.SetRegionAsLoadedFn &&
            e.Action.Contains("LookoutPoint", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.NotEqual("StartOakVale", life.CurrentRegion.RegionName);
        Assert.Equal(1, life.OpenStaticMapsMode);
        Assert.Contains("LookoutPoint", life.OpenedStaticMaps);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.OpenStaticMapsFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.SetStaticMapFileForUseFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.ParseMapHeaderFn);
        Assert.Contains(life.OpenedMapBodies, b => b.Name == "LookoutPoint");
        var body = life.OpenedMapBodies.Single(b => b.Name == "LookoutPoint");
        Assert.Equal(25, body.HeaderVersion);
        Assert.Equal(0x1904u, body.HeaderConstant);
        Assert.True(body.CompiledSize > 1000, $"lev={body.CompiledSize}");
        Assert.True(body.StbSize > 0, $"stb={body.StbSize}");
        Assert.True(body.GridWidth >= 64, $"w={body.GridWidth}");
        Assert.True(body.HeightSamples > 0, $"samples={body.HeightSamples}");
        Assert.Null(life.CurrentCompiledLev);
        Assert.Null(life.CurrentHeightField);
        Assert.True(life.Meshes.Opened);
        Assert.True(life.Meshes.EntryCount > 100, $"entries={life.Meshes.EntryCount}");
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.InitMeshBankFn);
        Assert.Contains(life.Trace.Events, e => e.Va == MeshBank.OpenVtbl4);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GameLoadWorldFn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.GameLoadWorldFn &&
            e.Action.Contains("Loading world", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.LoadSaveFn &&
            e.Action.Contains("skipped", StringComparison.Ordinal));
        Assert.Equal("LookoutPoint", life.CurrentStaticMapName);
        Assert.Contains("PicnicArea", life.NeighbourStaticMaps);
        Assert.Contains(life.OpenedMapBodies, b => b.Name == "LookoutPoint" && !b.Neighbour);
        Assert.Contains(life.OpenedMapBodies, b => b.Name == "PicnicArea" && b.Neighbour);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.OpenStaticMapsNameTable);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.AttachPatchFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.CloseStaticMapFileFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.OpenStaticMapsAttach);
        Assert.True(life.OpenedMapBodies.Count > 1, $"bodies={life.OpenedMapBodies.Count}");
        Assert.Contains(life.OpenedMapBodies, b => b.Name == "PicnicArea");
        Assert.Contains("LookoutPoint", life.ActivatedMaps);
        Assert.DoesNotContain("PicnicArea", life.ActivatedMaps);
        Assert.True(life.WorldSubmitted);
        Assert.NotNull(life.SubmittedWorld);
        Assert.False(life.SubmittedWorld.Expanded);
        Assert.Empty(life.SubmittedWorld.Triangles);
        Assert.NotNull(life.SubmittedMesh);
        Assert.True(life.SubmittedMesh.Vertices.Length > 128);
        Assert.Equal(4299, life.HeroMeshId);
        Assert.Contains(life.SubmittedPalskinMeshIds, id => id == 4299);
        Assert.True(life.SubmittedHeroPalskin);
        var heroMesh = life.Meshes.Get(4299);
        Assert.NotNull(heroMesh);
        Assert.True(heroMesh.BoneCount > 0, $"hero bones={heroMesh.BoneCount}");
        Assert.True(heroMesh.SkinFaces.Count > 0);
        Assert.Contains(life.SubmittedWorld.Instances, i =>
            i.MeshId == 4299 ||
            string.Equals(i.Definition, EngineLifecycle.CreatureHeroDefName,
                StringComparison.OrdinalIgnoreCase));
        Assert.Contains("LookoutPoint", life.SubmittedTerrainMaps);
        Assert.NotNull(life.SubmitSidePlanes());
        Assert.NotNull(life.Textures);
        Assert.True(life.SubmittedTextures.Count > 0, "engine 009BE8B0 ids");
        var frameTex = life.BuildFrame().Textures;
        Assert.NotNull(frameTex);
        Assert.Equal(life.SubmittedTextures.Count, frameTex.Length);
        Assert.Equal("LookoutPoint", life.SubmittedWorld.Region);
        var presented = life.PresentWorld();
        Assert.NotNull(presented);
        Assert.Equal("LookoutPoint", presented.Region);
        Assert.False(presented.Expanded);
        Assert.Empty(presented.Triangles);
        Assert.True(presented.MeshInstances > 0, $"instances={presented.MeshInstances}");
        Assert.True(life.Meshes.ParsedCount > 0);
        Assert.Equal(
            life.Camera.Position.X, life.WorldCamera.SlotA.V0.X, 3);
        Assert.Same(life.SubmittedWorld, life.BuildFrame().World);
        life.CloseStaticMapFile();
        Assert.Empty(life.OpenedMapBodies);
        Assert.Equal(0, life.OpenStaticMapsMode);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
            "traces");
        Directory.CreateDirectory(dest);
        life.Trace.Write(Path.Combine(dest, "winmain-forward.txt"));
        life.Trace.Write(Path.Combine(dest, "init-world-map.txt"));
        life.Trace.Write(Path.Combine(dest, "game-pump-004189C2.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-game-pump.txt"),
            """
            004189C2 game-mode vtbl+8 first iteration:
              [esi+36] world, vtbl+52 = 004AE8C0 [world+20] World Map
              004FB150 [WorldMap+156] current region index (ctor 0)
              004FC180 [WorldMap+44] + index*88
              [record+36] refcount touch; 006BC410 zeros it
            005066E0 inserts dummy 88-byte slot 0 before WLD
            append. NewRegion N is native index N.
            Index 0 dummy. Index 1 LookoutPoint.
            Index 4 StartOakVale. First pump does not
            SetRegionAsLoaded.
            00487C20 is 00449E60 PlayerRegionName
            persist HEADER (continue), not New Game.
            00501450 no-save: count>1 then
            00500540(1,0,0) LookoutPoint.
            00500540 LoadRegion → 006C27A0 job +28=index
            → 006C2120 enqueue [WorldMap+188]
            → 006C2710 / 006C2170 Loading topology
            → 004FCBB0 +38=1 → 004FC8A0 writes +156.
            [record+36] writer unread; null still loads.
            Not 00DBDE40.
            """);
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-init-world-map.txt"),
            """
            Init World Map 005066E0 is the 0xD8 ctor (vtbl 01244AEC,
            shift 5, bound 0x2000). Not the WLD parser.
            00507C30 vtbl+12 is "Load .wld file": token switch
            NewMap/EndMap/NewRegion/ContainsMap/SeesMap/...
            Same vocabulary as WorldFile.Load(FinalAlbion.wld).
            Load GTNG 0050959F stem+.gtng (0x1244BB4); TLC missing skips.
            Load global things 00509859: [0x13B8609] default 0 →
            004FDBC0 per-map .tng (004FBF60/004FAFF0). Nonzero →
            004FE2A0 .gtg NEWMAP LoadAllLoadableGlobalThingsFromSingleFile.
            Create Players 004166A8: 0044C6B0 [0x13B879C], 0044A530
            slots 0-4 size 0x22C, [+24]=4, 004AE940. Not 0044A3B0
            hero_swap_*.tng.
            Load region graph 00509982 → 00506D40 / 00828710.
            Not 00DBDE40 / StartOakVale setup.
            """);
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-main-forward.txt"),
            """
            PE AddressOfEntryPoint 0x00401067 MSVCR71 WinMainCRTStartup
            WinMain 00403480 CreateMutex then 00402510
            00402510 named stages:
              Parse Command Line
              Setup Basic install files
              Setup Language
              Setup basic retail banks 009A8150 @ 0x13CA79C
              Setup library 009A6610 / 009A4EC0 → 0x13CA618
              End basic init
            D3D9: 00BFEFB0 Direct3DCreate9(32); 009C0E50 GetDeviceCaps +56
                  009BF7E0 CreateDevice +64 flags 0x26 / 0x56
            004022B0 probe bpp; 00412F90 mode loop
            retail 0042EA8F vtbl 01230CA0 start 0042F75E pump 0042EC7C
            videos 006286F0: lionhead_logo 640x400, Microsoft_Logo 640x480,
                    intro_comp 640x360 (flags 0x1375448/4A default 1)
            Leave frontend → FinalAlbion.wld → Init Game 00418DCA
            NOT 00DBDE40
            """);
    }

    [Fact]
    public void Single_file_global_things_loads_gtg_newmap()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle { SingleGlobalThingsFile = true };
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        life.EnterGame();
        Assert.NotNull(life.GlobalThings);
        Assert.Equal(2, life.GlobalThings.Version);
        Assert.True(life.GlobalThings.Things.Count() > 100);
        Assert.Contains(life.GlobalThings.Things, t => t.DefinitionType == "HOLY_SITE_PLAYER_START");
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.LoadGlobalThingsSingle &&
            e.Action.StartsWith("things=", StringComparison.Ordinal));
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        life.Trace.Write(Path.Combine(dest, "load-gtg.txt"));
    }

    [Fact]
    public void Camera_004164E0_runs_on_install_after_WorldFrame()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.True(life.Pump());
        Assert.True(life.Pump());
        Assert.True(life.PlayerActionReady);
        Assert.Equal(2, life.WorldFrame);
        Assert.False(life.RegionObjectPresent);
        life.RenderGameMode();
        Assert.True(life.CameraInterpolationRan);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.CameraInterpolationFn);
        life.CameraCatchupTicks = 15;
        life.GamePlus72 = 1;
        life.RenderGameMode();
        Assert.Equal(3, life.CameraBodySteps);
        Assert.Equal(1, life.LastCameraLoopCount);
        Assert.Equal(1, life.GamePlus80);
        Assert.True(life.GamePlus90594);
        Assert.NotNull(life.Camera);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.CameraBodyFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.WorldCameraApplyFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.CameraManagerBlendFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.DisplayApplyBodyFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.WorldCameraCtor);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.WorldCameraSeedFn);
        Assert.True(life.WorldCamera.Seeded);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
            "traces");
        Directory.CreateDirectory(dest);
        life.Trace.Write(Path.Combine(dest, "camera-004164E0.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-004164E0.txt"),
            """
            004164E0 camera body (86 insn, ret 4):
              fild arg; fmul qword [0x122EDB8]=1/15
              00BFEA70 fistp toward zero = loop count
              skip if [game+80]>=[game+72] or count<=0
              each step: 00415A60 52-byte record
                +0/+16 = (i+1)/arg
                +4/+20 = i/count
                snapshot game+112/+116/+128/+132
                +48 = game+72
                rep movsd record -> game+112
              0049E080 world apply:
                004C74F0 [world+80]
                0051EBD0 thing walk vtbl+84/+104
                006B42F0(world+24, t) camera-manager lerp
                  (slots +3084/+6188/+6296 PARTIAL)
              00416231 time = 009E1BC0-[game+96]
              00435F70 jmp 00435530 display (PARTIAL)
              [game+90424]++ ; [game+80]=[game+72]
            00417001: WorldFrame<=1 skip
              [0x13B8630]<=0 -> 0041707E interpolation
              else clamp min [0x1375550]=15, call 004164E0
              then [game+90594]=1
            [0x13B8630] writers unread (3 imm sites).
            0041707E default New Game path (ticks==0):
              world+164==0; t=clamp(004166E2*15-[game+72],0,1)
              0041919C; 0049E080; 004AEA70 (+9826==0 → true)
              00435F70; [90594]=1
              0041714D world+164!=0 UNREAD
            [record+36] first writer is not on no-save:
              ctor 006BC410 zeros; 0051A900/00519B80 copy
              0051D924 setter has 0 callers
              only [table+i*88+36] site is 00500540 READ
              004FC180 callers read +24/+86; none write +36
              00523540 CThingManager ctor not called here
              null → 005009BE still loads. Not 00DBDE40.
            """);
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-record36.txt"),
            """
            DISPROVEN: no-save New Game writes NewRegion [record+36].

            PROVEN:
            006BC410 ctor zeros +36
            0051A900 copy + AddRef [obj+4]
            00519B80 assign Release/AddRef
            0051D924 mov [ecx+36],eax; ret 4 — 0 E8 / 0 data
            00500540 READ [table+index*88+36]; null → 005009BE
            004189C2 AddRef/Release touch only
            004FC180 callers (00418A57, 00449FEA, 0049E600,
              0049F8A2, 004A5C83, ...) read name/flags
            Object layout matches CThingManager 00523540
              (+60/+144/+164) but that ctor is not called
              from 004F0000-006D0000 on this path.

            Null is the native no-save state.
            First non-null writer still UNREAD (not this path).
            """);
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-006B42F0.txt"),
            """
            world+24 is NOT 006FD8C0.
            004A6E30 after Init Environment:
              alloc 0x1970, ctor 006B4900, store [world+24]
            006B4900 vtbl 0125D53C
              6x 008864A0 at +84 stride 0x1F4
              6x 008864A0 at +3188
              +3092/+3108 = (1,0,0)
              +3088/+3104 = 0.2
              +68 = 0
            006B42F0(world+24, t):
              clamp t [0,1]
              +68==0 → 006B3FF0 seed
              out+6296 = (1-t)*+6196 + t*+3092
              out+6312 = lerp +3108/+6212
              out+6328 = lerp +3120/+6224
            0069AE80 alloc 0x160 → world+48, copy world+52
            006FD8C0 alloc 0xC8 → world+44 vtbl 01264A8C
              +176 = fistp(15*1.5)=22
            ScriptedCamera.ApplyManagerOutput(+6296,+6312,+6328).
            Not 00DBDE40.
            """);
    }
}
