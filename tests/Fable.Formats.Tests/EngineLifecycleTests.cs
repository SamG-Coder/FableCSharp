using System.Numerics;
using Fable.Core;
using Fable.Formats;
using Fable.Formats.Defs;
using Fable.Formats.Fonts;
using Fable.Formats.Levels;
using Fable.Formats.Scene;
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
        Assert.Equal(28, EngineLifecycle.WorldPrepareVtbl);
        Assert.Equal(0x00416968u, EngineLifecycle.WorldPrepareSite);
        Assert.Equal(0x004A3200u, EngineLifecycle.LoadSaveFn);
        Assert.Equal(90576, EngineLifecycle.GameWorldPathOffset);
        Assert.Equal(90584, EngineLifecycle.GameQuestOverrideOffset);
        Assert.Equal(90588, EngineLifecycle.GameSaveNameOffset);
        Assert.Equal(0x00415E17u, EngineLifecycle.GameWorldPathCopyFn);
        Assert.Equal(0x013B8668u, EngineLifecycle.WorldPathGlobalVa);
        Assert.Equal(0x0122EE14u, EngineLifecycle.WorldPathDefaultVa);
        Assert.Equal("updatedscenic.wld", EngineLifecycle.WorldPathDefault);
        Assert.Equal(0x0122D70Eu, EngineLifecycle.EmptyQuestNameVa);
        Assert.Equal(0x004BBC00u, EngineLifecycle.AfterLoadWorldFn);
        Assert.Equal(0x013B8674u, EngineLifecycle.AfterLoadWorldArgVa);
        Assert.Equal(0x0049BA70u, EngineLifecycle.PostLoadWorldReserveFn);
        Assert.Equal(60, EngineLifecycle.PostLoadWorldReserveCount);
        Assert.Equal(0.1, EngineLifecycle.PostLoadWorldReserveRate);
        Assert.Equal(0x00416392u, EngineLifecycle.WorldThingCountFn);
        Assert.Equal(0x0049E200u, EngineLifecycle.WorldThingCountApply);
        Assert.Equal(0x004AE9D0u, EngineLifecycle.PlayerBindAfterWorldFn);
        Assert.Equal("default_user.ini", EngineLifecycle.DefaultUserIniName);
        Assert.Equal("user.ini", EngineLifecycle.UserIniName);
        Assert.Equal("userst.ini", EngineLifecycle.UserstIniName);
        Assert.Equal("default_userst.ini", EngineLifecycle.DefaultUserstIniName);
        Assert.Equal(0x00413C50u, EngineLifecycle.UserstRegisterFn);
        Assert.Equal(0x00414C66u, EngineLifecycle.UserstApplyFn);
        Assert.Equal(0x0122E674u, EngineLifecycle.UserstIniVa);
        Assert.Equal(0x0137544Au, EngineLifecycle.DisplayWindowFlagVa);
        Assert.Equal((byte)1, EngineLifecycle.DisplayWindowFlagFirstSeen);
        Assert.Equal(0x009A64B0u, EngineLifecycle.CreateWindowFn);
        Assert.Equal(0x00CA0000, EngineLifecycle.CreateWindowExStyle);
        Assert.Equal(572, EngineLifecycle.PresentParametersWindowedOffset);
        Assert.Equal(0x009EC890u, EngineLifecycle.IniApplyFn);
        Assert.Equal(0x009ECB70u, EngineLifecycle.IniRunScriptFn);
        Assert.Equal(0x009EB260u, EngineLifecycle.IniUnknownFn);
        Assert.Equal(0x00419CE0u, EngineLifecycle.IniActivateQuestThunk);
        Assert.Equal(0x00892E80u, EngineLifecycle.ScriptManagerActivateQuestFn);
        Assert.Equal(1104, EngineLifecycle.ScriptManagerActivateQuestVtbl);
        Assert.Equal(0x009EC710u, EngineLifecycle.IniTokenizeFn);
        Assert.Equal(0x009EB430u, EngineLifecycle.IniDispatchFn);
        Assert.Equal(0x0040D2A0u, EngineLifecycle.PlayAviSingletonFn);
        Assert.Equal(0x0040CEC0u, EngineLifecycle.PlayAviSingletonCtor);
        Assert.Equal(0x140, EngineLifecycle.PlayAviSingletonSize);
        Assert.Equal(0x0040BC80u, EngineLifecycle.PlayAviApplyFn);
        Assert.Equal(0x0040A7F0u, EngineLifecycle.PlayAviApplyBodyFn);
        Assert.Equal(0x00B239A0u, EngineLifecycle.DisplayEngineFadeFn);
        Assert.Equal(220, EngineLifecycle.DisplayEngineFadeVtbl);
        Assert.Equal(12, EngineLifecycle.DisplayEngineFadeType);
        Assert.Equal(20f, EngineLifecycle.DisplayEngineFadeSeconds);
        Assert.Equal(0x009F2660u, EngineLifecycle.InputLockEnterFn);
        Assert.Equal(0x009F26B0u, EngineLifecycle.InputLockLeaveFn);
        Assert.Equal(0x0098E1B0u, EngineLifecycle.GamePumpInnerStartFn);
        Assert.Equal(0x009A6460u, EngineLifecycle.GamePumpQuitQuery);
        Assert.Equal(1, EngineLifecycle.GamePumpQuitFirstSeen);
        Assert.Equal(0x004FEEC0u, EngineLifecycle.UnloadCurrentRegionFn);
        Assert.Equal(0x00500540u, EngineLifecycle.LoadRegionFn);
        Assert.Equal(0x006C27A0u, EngineLifecycle.BuildLoadJobFn);
        Assert.Equal(0x006C2D40u, EngineLifecycle.BuildLoadJobCopyMapsFn);
        Assert.Equal(0x006B9E00u, EngineLifecycle.BuildLoadJobCopyTreeFn);
        Assert.Equal(28, EngineLifecycle.LoadJobRecordSize);
        Assert.Equal(0x006C2BA0u, EngineLifecycle.LevelLoaderPopFn);
        Assert.Equal(0x004FF080u, EngineLifecycle.LoadTopologyFn);
        Assert.Equal(0x00638310u, EngineLifecycle.LoadTopologyHelperFn);
        Assert.Equal(0x004FF440u, EngineLifecycle.PostLoadTopologyFn);
        Assert.Equal(0x004FD020u, EngineLifecycle.PostLoadInitialiseFn);
        Assert.Equal(0x0051E2F0u, EngineLifecycle.ThingManagerActivateAfterFn);
        Assert.Equal(88, EngineLifecycle.WorldMapSetLoadedVtbl);
        Assert.Equal(0x004167DAu, EngineLifecycle.EngineReadyCallback);
        Assert.Equal(0x0049F180u, EngineLifecycle.InitCharactersFn);
        Assert.Equal(0x004B4A10u, EngineLifecycle.ActivateInitialQuestsFn);
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
        Assert.Equal(0xFFFFFFFFu, EngineLifecycle.StartupVideos[0].Rgba);
        Assert.Equal(0xFF000000u, EngineLifecycle.StartupVideos[1].Rgba);
        Assert.Equal(0x00000000u, EngineLifecycle.StartupVideos[2].Rgba);
        Assert.Equal(0x013961E0u, EngineLifecycle.PlayAviClearColorVa);
        Assert.Equal(0x0042E98Fu, EngineLifecycle.RetailAfterAviFn);
        Assert.Equal(0x0042DB40u, EngineLifecycle.FrontendHelperCtor);
        Assert.Equal(0x0042DED5u, EngineLifecycle.RetailAudioFadeFn);
        Assert.Equal(0x009BFF40u, EngineLifecycle.DisplayModeFn);
        Assert.Equal(0x400, EngineLifecycle.DisplayModeWidth);
        Assert.Equal(0x300, EngineLifecycle.DisplayModeHeight);
    }

    [Fact]
    public void Retail_0042EC7C_after_AVI_clears_then_inits_frontend()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        Assert.Equal(0xFFFFFFFFu, life.PlayAviClearArgb);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PlayAviClearColorVa &&
            e.Action.Contains("FFFFFFFF", StringComparison.Ordinal));
        life.FinishStartupVideo();
        Assert.Equal(0xFF000000u, life.PlayAviClearArgb);
        life.FinishStartupVideo();
        Assert.Equal(0x00000000u, life.PlayAviClearArgb);
        life.FinishStartupVideo();
        Assert.Equal(EngineStage.Frontend, life.Stage);
        Assert.Equal(EngineLifecycle.PlayAviClearRestoreArgb, life.PlayAviClearArgb);
        var events = life.Trace.Events;
        Assert.Contains(events, e => e.Va == EngineLifecycle.RetailBankSwapFlagVa);
        Assert.Contains(events, e => e.Va == EngineLifecycle.RetailAfterAviFn);
        Assert.Contains(events, e => e.Va == EngineLifecycle.DisplayModeFn);
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.ClearColorFn && e.Stage == "InitFrontend");
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.PresentFn && e.Stage == "InitFrontend");
        Assert.Contains(events, e => e.Va == EngineLifecycle.RetailAudioFadeFn);
        Assert.Contains(events, e => e.Va == EngineLifecycle.FrontendHelperCtor);
        Assert.Contains(events, e => e.Va == EngineLifecycle.FrontendUiShowFn);
        Assert.Contains(events, e => e.Va == EngineLifecycle.FrontendPostInitFn);
        var complete = events.FindIndex(e =>
            e.Va == EngineLifecycle.PlayAviPlayer &&
            e.Action.Contains("complete", StringComparison.Ordinal) &&
            e.Action.Contains("intro_comp", StringComparison.Ordinal));
        var after = events.FindIndex(e => e.Va == EngineLifecycle.RetailAfterAviFn);
        var present = events.FindIndex(e =>
            e.Va == EngineLifecycle.PresentFn && e.Stage == "InitFrontend");
        var engine = events.FindIndex(e => e.Va == EngineLifecycle.FrontendEngineInitFn);
        Assert.True(complete >= 0 && after > complete, "0042E98F after last PlayAVI");
        Assert.True(engine > after, "Init Engine after 0042E98F");
        Assert.True(present > engine, "009BEEB0 after Init Engine");
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
    public void Frontend_009DA9F0_first_seen_is_empty_skip_not_type_22()
    {
        Assert.False(EngineLifecycle.DisplayFlushShouldDip(0, 0));
        Assert.True(EngineLifecycle.DisplayFlushShouldDip(0, 60));
        Assert.Equal(1, EngineLifecycle.DisplayQueueCount(0, 60));
        Assert.Equal(2, EngineLifecycle.DisplayFlushPrimitive(false));
        Assert.Equal(4, EngineLifecycle.DisplayFlushPrimitive(true));
        Assert.Equal(60, EngineLifecycle.DisplayQueueRecordSize);
        Assert.Equal(0x009DB700u, EngineLifecycle.DisplayQueueEnqueueFn);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.True(life.Pump());
        Assert.False(life.Frontend2dDipIssued);
        Assert.False(EngineLifecycle.DisplayFlushShouldDip(0, 0));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayFlushLayersFn &&
            e.Action.Contains("empty", StringComparison.Ordinal));
    }

    [Fact]
    public void Frontend_0059A238_message_15_sets_retail_41()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.RetailNewGameFlag);
        life.DispatchFrontendMessage(14);
        Assert.False(life.RetailNewGameFlag);
        life.DispatchFrontendMessage(EngineLifecycle.FrontendNewGameMessage);
        Assert.True(life.RetailNewGameFlag);
        Assert.Equal(EngineStage.Frontend, life.Stage);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendUiMessageFn &&
            e.Action.Contains("msg=15", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.FrontendNewGameApply);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.FrontendNewGameThunk);
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.Equal("FinalAlbion.wld", life.WorldFileName);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Frontend_0059A238_msg_E5_empty_005955AB_is_00595845_then_00596917()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.Equal(
            EngineLifecycle.FrontendPressStartMenu, life.FrontendMenuRoot);
        Assert.Equal(0, life.FrontendProfileCount);
        Assert.False(life.FrontendUiArmed);
        Assert.False(life.FrontendUi96Present);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendMainMenuFn);
        life.DispatchFrontendMessage(EngineLifecycle.FrontendPressStartMessage);
        Assert.True(life.FrontendUiArmed);
        Assert.True(life.FrontendUi100);
        Assert.False(life.FrontendUi96Present);
        Assert.Equal(
            EngineLifecycle.FrontendPressStartMenu, life.FrontendMenuRoot);
        Assert.False(life.RetailNewGameFlag);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendUiMessageFn &&
            e.Action.Contains("msg=229", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendPressStartAcceptFn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendNoProfileFn);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendMainMenuFn);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendNewProfileBindFn);
        Assert.True(life.Pump());
        Assert.False(life.FrontendUiArmed);
        Assert.True(life.FrontendUi96Present);
        Assert.False(life.FrontendUi96Accept);
        Assert.False(life.FrontendUi96Armed);
        Assert.Equal(
            EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        Assert.Equal(EngineStage.Frontend, life.Stage);
        Assert.False(life.RetailNewGameFlag);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendNewProfileBindFn &&
            e.Action.Contains("0x17", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendMenuSwitchFn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendUi96CtorFn &&
            e.Action.Contains("+4=0", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendMainMenuFn);
        Assert.Equal(0x00599D5Cu, EngineLifecycle.FrontendPressStartAcceptFn);
        Assert.Equal(0x00595845u, EngineLifecycle.FrontendNoProfileFn);
        Assert.Equal(0x00596917u, EngineLifecycle.FrontendNewProfileBindFn);
        Assert.Equal(0x17, EngineLifecycle.FrontendNewProfileSlot);
        Assert.Equal(0xE5, EngineLifecycle.FrontendPressStartMessage);
        Assert.Equal(0x124, EngineLifecycle.FrontendMainMenuMessage);
    }

    [Fact]
    public void Frontend_press_start_Return_does_not_post_0xE5_or_15()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.QueueInput(EngineInput.TypeKey, RegionTravel.PlayAviSkipReturn);
        Assert.True(life.Pump());
        Assert.Equal(
            EngineLifecycle.FrontendPressStartMenu, life.FrontendMenuRoot);
        Assert.False(life.RetailNewGameFlag);
        Assert.Equal(EngineStage.Frontend, life.Stage);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendUiMessageFn &&
            e.Action.Contains("msg=229", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendUiMessageFn &&
            e.Action.Contains("msg=15", StringComparison.Ordinal));
        Assert.Equal(
            FrontendInputMap.ActionFromKey,
            FrontendInputMap.ActionFromEvent(
                EngineInput.TypeKey, RegionTravel.PlayAviSkipReturn));
        Assert.Null(FrontendInputMap.TryMapEvent(
            EngineInput.TypeKey,
            RegionTravel.PlayAviSkipReturn,
            EngineLifecycle.FrontendPressStartMenu));
    }

    [Fact]
    public void Frontend_press_start_type4_posts_0xE5_then_new_profile()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.Equal(4, EngineInput.Type4);
        Assert.Equal(26, EngineInput.ActionType4);
        life.QueueInput(EngineInput.Type4, 0);
        Assert.True(life.Pump());
        Assert.Equal(
            EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        Assert.Equal("Default", life.FrontendEditBoxName);
        Assert.False(life.RetailNewGameFlag);
        Assert.Equal(EngineStage.Frontend, life.Stage);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendUiMessageFn &&
            e.Action.Contains("msg=229", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendUiMessageFn &&
            e.Action.Contains("msg=15", StringComparison.Ordinal));
    }

    [Fact]
    public void Frontend_00851770_seeds_Default_then_0x126_is_0059697A_main_menu()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.DispatchFrontendMessage(EngineLifecycle.FrontendPressStartMessage);
        Assert.True(life.Pump());
        Assert.Equal(
            EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        Assert.True(life.FrontendEditBoxBound);
        Assert.Equal(
            EngineLifecycle.FrontendProfileDefaultFallback,
            life.FrontendEditBoxName);
        Assert.False(life.FrontendUi96Armed);
        Assert.Equal(37, EngineLifecycle.FrontendNewProfileEditType);
        Assert.Equal(0x126, EngineLifecycle.FrontendAcceptProfileMessage);
        Assert.Equal(0x0122DE80u, EngineLifecycle.FrontendProfileDefaultFallbackVa);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendUi96EditBoxFn &&
            e.Action.Contains("type 37", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendProfileDefaultFn &&
            e.Action.Contains("Default", StringComparison.Ordinal));
        life.DispatchFrontendMessage(EngineLifecycle.FrontendAcceptProfileMessage);
        Assert.True(life.FrontendUi96Armed);
        Assert.False(life.FrontendUi96Accept);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendCommitNameFn &&
            e.Action.Contains("[+4]=0", StringComparison.Ordinal));
        Assert.True(life.Pump());
        Assert.Equal(
            EngineLifecycle.FrontendMainMenuNoContinue, life.FrontendMenuRoot);
        Assert.False(life.FrontendUi96Present);
        Assert.False(life.RetailNewGameFlag);
        Assert.Equal(EngineStage.Frontend, life.Stage);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendCommitProfileFn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendCanCreateProfileFn &&
            e.Action.Contains("writable", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendMenuAttachFn &&
            e.Action.Contains("NO_CONTINUE", StringComparison.Ordinal));
    }

    [Fact]
    public void Frontend_type4_then_injected_0x126_then_15_leaves()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.QueueInput(EngineInput.Type4, 0);
        Assert.True(life.Pump());
        Assert.Equal(
            EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        Assert.Equal("Default", life.FrontendEditBoxName);
        Assert.False(life.RetailNewGameFlag);
        life.QueueInput(EngineInput.TypeKey, RegionTravel.PlayAviSkipReturn);
        Assert.True(life.Pump());
        Assert.Equal(
            EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        Assert.False(life.RetailNewGameFlag);
        life.DispatchFrontendMessage(EngineLifecycle.FrontendAcceptProfileMessage);
        Assert.True(life.Pump());
        Assert.Equal(
            EngineLifecycle.FrontendMainMenuNoContinue, life.FrontendMenuRoot);
        Assert.False(life.RetailNewGameFlag);
        Assert.Equal(EngineStage.Frontend, life.Stage);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendUiMessageFn &&
            e.Action.Contains("msg=294", StringComparison.Ordinal));
        life.QueueInput(EngineInput.TypeKey, RegionTravel.PlayAviSkipReturn);
        Assert.True(life.Pump());
        Assert.False(life.RetailNewGameFlag);
        Assert.Equal(EngineStage.Frontend, life.Stage);
        life.DispatchFrontendMessage(EngineLifecycle.FrontendNewGameMessage);
        Assert.True(life.Pump());
        Assert.True(life.RetailNewGameFlag);
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.Equal("FinalAlbion.wld", life.WorldFileName);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.LeaveFrontendSite);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.InitGameSite);
    }

    [Fact]
    public void Frontend_0059A238_msg_124_attaches_main_menu_no_continue()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.DispatchFrontendMessage(EngineLifecycle.FrontendMainMenuMessage);
        Assert.Equal(
            EngineLifecycle.FrontendMainMenuNoContinue, life.FrontendMenuRoot);
        Assert.False(life.RetailNewGameFlag);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendMainMenuFn &&
            e.Action.Contains("NO_CONTINUE", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendMenuAttachFn);
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
            0042E98F: +180, 005958F5, 00598A1C(0)
              0041DB1D UI_FRONTEND_PRESS_START_MENU slot 0x14
              msg 0xE5 vtbl+284
              0059899A / MAIN_MENU later, not first-seen
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
            EngineLifecycle.FrontendPressStartMenu, life.FrontendMenuRoot);
        Assert.Equal(0x00598A1Cu, EngineLifecycle.FrontendPressStartAttachFn);
        Assert.Equal(0x14, EngineLifecycle.FrontendPressStartSlot);
        Assert.Equal(0xE5, EngineLifecycle.FrontendPressStartMessage);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendPressStartAttachFn &&
            e.Action.Contains("PRESS_START", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendMainMenuFn);
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
        Assert.Equal(0x00B23BC0u, EngineLifecycle.FrontendSubmitFn);
        Assert.Equal(0x00B324A0u, EngineLifecycle.FrontendSubmitDispatchFn);
        Assert.Equal(0x012A0F3Cu, EngineLifecycle.FrontendEngineVtbl);
        Assert.Equal(0x178, EngineLifecycle.FrontendEngineObjectSize);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.FrontendSubmitFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.FrontendSubmitDispatchFn);
        Assert.True(life.FrontendType22HandlerRegistered);
        Assert.False(life.FrontendEnqueueRan);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendSpriteHandlerCtorFn &&
            e.Action.Contains("VSHADER_2D_SPRITE", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendSpriteSubmitFn &&
            e.Action.Contains("00BAE2D0", StringComparison.Ordinal));
        Assert.Equal(0x00BAD040u, EngineLifecycle.FrontendSpriteHandlerCtorFn);
        Assert.Equal(0x00BAE2D0u, EngineLifecycle.FrontendSpriteSubmitFn);
        Assert.True(life.FrontendWidgetTickRan);
        Assert.True(life.FrontendDestLayoutRan);
        Assert.False(life.FrontendInstanceSubmitRan);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendWidgetTickFn &&
            e.Action.Contains("0052C7E0", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendDestLayoutFn &&
            e.Action.Contains("00531EC0", StringComparison.Ordinal));
        Assert.Equal(0x0052C7E0u, EngineLifecycle.FrontendWidgetTickFn);
        Assert.Equal(0x00531EC0u, EngineLifecycle.FrontendDestLayoutFn);
        Assert.Equal(0x00BAD8A0u, EngineLifecycle.FrontendSpriteInstanceSubmitFn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendWidgetMessageNoopFn &&
            e.Action.Contains("ret 4", StringComparison.Ordinal));
        Assert.Equal(0x00B23BC0u, EngineLifecycle.FrontendSubmitFn);
        Assert.Equal(0x0052F040u, EngineLifecycle.FrontendWidgetMessageNoopFn);
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
        var present = Array.LastIndexOf(vas, EngineLifecycle.PresentFn);
        Assert.True(begin >= 0 && begin < ui && ui < flush && flush < end && end < present);
        Assert.Equal(RegionTravel.PlayAviPresent, EngineLifecycle.PresentFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.True(life.Pump());
        Assert.True(life.FrontendInstanceSubmitRan);
        Assert.False(life.FrontendEnqueueRan);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendSpriteInstanceSubmitFn &&
            e.Action.Contains("00BADB36", StringComparison.Ordinal));
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
                [ui+84] vtbl+4 0052C7E0 → 00531EC0
                0052F5C0 +264 / 0052FFD0 +248 ctor 0
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
    public void Frontend_0041AC20_dest_is_0041AFA0_scale_not_PlayAVI()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.True(life.Pump());
        var dest = EngineLifecycle.FrontendWidgetDest(0, 0, 0, 0, 0, 0, 0, 0, false);
        Assert.Equal((0f, 0f, 0f, 0f), dest);
        Assert.Equal(dest.X0, life.FrontendWidgetDestX0);
        Assert.Equal(dest.Y0, life.FrontendWidgetDestY0);
        Assert.Equal(dest.X1, life.FrontendWidgetDestX1);
        Assert.Equal(dest.Y1, life.FrontendWidgetDestY1);
        Assert.Equal(0x00595222u, EngineLifecycle.FrontendUiDrawFn);
        Assert.Equal(0x00530EC0u, EngineLifecycle.FrontendWidgetFontListFn);
        Assert.Equal(432, EngineLifecycle.FrontendWidgetFontListVtbl);
        Assert.Equal(204, EngineLifecycle.FrontendWidgetDestWOffset);
        Assert.Equal(248, EngineLifecycle.FrontendWidgetOriginXOffset);
        Assert.Equal(264, EngineLifecycle.FrontendWidgetScaleXOffset);
        Assert.Equal(1, life.FrontendWidgetsDrawn);
        Assert.True(life.FrontendWidgetTickRan);
        Assert.True(life.FrontendDestLayoutRan);
        Assert.False(life.Frontend2dDipIssued);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendUiDrawFn &&
            e.Action.Contains("[ui+84]", StringComparison.Ordinal));
        var scaled = EngineLifecycle.FrontendWidgetDest(640, 400, 0, 0, 0, 0, 1, 1, false);
        Assert.Equal((0f, 0f, 640f, 400f), scaled);
        var centered = EngineLifecycle.FrontendWidgetDest(10, 10, 0, 0, 100, 100, 1, 1, true);
        Assert.Equal((95f, 95f, 105f, 105f), centered);
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
        Assert.True(life.FrontendWidgetsDrawn >= 1);
        Assert.True(life.Frontend2dRecordsQueued >= 1);
        Assert.Equal(0x0041BEB0u, life.Frontend2dLastPacker);
        Assert.Equal(0, life.FrontendWidgetFont);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendPressStartCtorFn &&
            e.Action.Contains("0054E3D0", StringComparison.Ordinal));
        Assert.Equal(EngineLifecycle.FrontendPressStartType, life.FrontendRootType);
        Assert.True(life.FrontendChildCount >= 6);
        Assert.Contains(life.FrontendWidgets, w => w.Name == EngineLifecycle.FrontendPressStartText);
        Assert.Contains(life.FrontendWidgets, w =>
            w.TextTag == EngineLifecycle.FrontendPressStartTextTag);
        Assert.True(life.FrontendDefFound);
        Assert.Equal("UI", life.FrontendDefTypeName);
        Assert.True(life.FrontendType22HandlerRegistered);
        Assert.True(life.FrontendWidgetTickRan);
        Assert.True(life.FrontendDestLayoutRan);
        Assert.True(life.FrontendEnqueueRan);
        Assert.True(life.Frontend2dDipIssued);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayFlush2dFn &&
            e.Action.Contains("0x13BC800", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayFlush2dFn &&
            e.Action.Contains("009D9C80-009DB000", StringComparison.Ordinal));
        Assert.True(life.Frontend2dDipIssued);
        Assert.Equal(16020, EngineLifecycle.DisplayQueueBeginOffset);
        Assert.Equal(332, EngineLifecycle.DrawIndexedPrimitiveVtbl);
        Assert.Equal(0x00A058C0u, EngineLifecycle.DisplayPrimitiveFn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayFlushLayersFn &&
            e.Action.Contains("DIP vtbl+", StringComparison.Ordinal));
        Assert.False(life.FrontendDisplayFlag);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.FrontendWidgetDrawFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.FrontendWidgetFactoryFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.FrontendWidgetQueueFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Frontend_PRESS_START_is_type_10_with_text_child()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.Equal(EngineStage.Frontend, life.Stage);
        Assert.Equal(10, life.FrontendRootType);
        Assert.Equal(0x0054E3D0u, EngineLifecycle.FrontendPressStartCtorFn);
        Assert.Equal(0x00530260u, EngineLifecycle.FrontendContainerDrawFn);
        Assert.Equal(0x0052C730u, EngineLifecycle.FrontendScaleInitFn);
        Assert.Equal(0x005339B0u, EngineLifecycle.FrontendScaleWriteFn);
        Assert.Equal(1024f / 640f, life.FrontendScaleX);
        Assert.True(life.FrontendChildCount >= 6);
        Assert.Contains(life.FrontendWidgets, w => w.Name == "UI_TITLE");
        Assert.Contains(life.FrontendWidgets, w => w.Name == "UI_BLENDING_BACKGROUNDS_FORREST");
        Assert.Contains(life.FrontendWidgets, w => w.Name == "UI_PRESS_START_TEXT");
        var text = Assert.Single(life.FrontendWidgets, w => w.Name == "UI_PRESS_START_TEXT");
        Assert.Equal("TEXT_GUI_MENU_PRESS_BUTTON", text.TextTag);
        Assert.Equal(6, text.Type);
        Assert.True(life.Pump());
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendContainerDrawFn &&
            e.Action.Contains("00530260", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendScaleWriteFn &&
            e.Action.Contains("005339B0", StringComparison.Ordinal));
        var drawn = life.FrontendWidgets.First(w => w.Name == "UI_PRESS_START_TEXT");
        Assert.Equal(512f, drawn.DestX0);
        Assert.Equal(384f, drawn.DestY0);
        Assert.Equal(512f, drawn.DestX1);
        Assert.Equal(384f, drawn.DestY1);
        Assert.False(string.IsNullOrEmpty(drawn.Text));
        Assert.Equal(0x0054F5C0u, EngineLifecycle.FrontendTextCtorFn);
        Assert.Equal(0x0054EF00u, EngineLifecycle.FrontendTextDrawFn);
        Assert.Equal(FontFile.UiFace, EngineLifecycle.FrontendUiFontFace);
        Assert.True(life.FrontendEnqueueRan);
        Assert.True(life.Frontend2dDipIssued);
        Assert.NotNull(life.FrontendBatch);
        Assert.False(life.FrontendBatch.Value.IsEmpty);
        Assert.Equal(4, life.FrontendBatch.Value.Draws[0].D3dPrimitiveType);
        Assert.Equal(5, life.FrontendBatch.Value.Draws[0].D3dSrcBlend);
        Assert.Equal(6, life.FrontendBatch.Value.Draws[0].D3dDestBlend);
        Assert.NotNull(life.FrontendPresentRgba);
        Assert.Equal(EngineLifecycle.DisplayDefaultWidth, life.FrontendPresentWidth);
        Assert.Contains(life.FrontendPresentRgba, b => b == 255);
        ExportDir.WriteRgbaBmp(
            ExportDir.PathFor("frontend", "press-start.bmp"),
            life.FrontendPresentWidth, life.FrontendPresentHeight,
            life.FrontendPresentRgba);
        var frame = life.BuildFrame();
        Assert.NotNull(frame.FrontendBatch);
        Assert.False(frame.FrontendBatch.Value.IsEmpty);
        Assert.NotNull(frame.FrontendRgba);
        Assert.True(frame.FrontendWidth > 0);
        Assert.Contains(life.FrontendWidgets, w =>
            w.TextureName == FrontendSpriteBank.TitleLeft);
        Assert.Contains(life.FrontendWidgets, w =>
            w.TextureName == FrontendSpriteBank.TitleRight);
        var title = life.FrontendWidgets.First(w => w.Name == "UI_TITLE_01");
        Assert.Equal(112f, title.DestX0);
        Assert.Equal(48f, title.DestY0);
        Assert.Equal(522f, title.DestX1);
        Assert.Equal(253f, title.DestY1);
        Assert.Contains(life.FrontendWidgets, w =>
            w.Name.Contains("FORREST_1_1", StringComparison.Ordinal) &&
            w.TextureName == "FORREST_1_1");
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
        life.LoadFromFirstRealRegion();
        Assert.Contains("LookoutPoint", life.ActivatedMaps);
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
        life.LoadFromFirstRealRegion();
        Assert.Equal("LookoutPoint", life.FirstSceneMapName);
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
        Assert.False(life.WorldUpdateRan);
        Assert.False(life.PlayerCatchupHit);
        Assert.Equal(0, life.WorldFrame);
        Assert.Equal("LookoutPoint", life.FirstSceneMapName);
        Assert.Contains(life.ThingsForMap("LookoutPoint"), t =>
            t.DefinitionType == RegionTravel.PlayerStartType &&
            t.ScriptName == EngineLifecycle.GuildArrivalHsp);
        Assert.Contains(life.ThingsForMap("LookoutPoint"), t =>
            ReferenceEquals(t, life.Hero));
        Assert.True(WorldCamera.IsCtorAxis(life.WorldCamera.SlotA.V0));
        Assert.True(life.RendererHelperBound);
        Assert.True((life.Camera.Position - new System.Numerics.Vector3(
            start.PositionX!.Value, start.PositionY!.Value, start.PositionZ ?? 0f)).Length() < 20f);
        Assert.Equal(LandscapeFrustum.FirstSeenCameraUp, life.Camera.Up);
        Assert.Equal(GameCamera.FirstSeenFovDegrees, life.Camera.FovDegrees);
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
              006B3FF0 +68 → 006B2CA0 pose (not 1.6m eye)
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
            life.ActivatedQuests.Take(6));
        Assert.Equal("Gameflow", life.ActivatedQuests[6]);
        Assert.Equal(7, life.ActivatedQuests.Count);
        Assert.Contains(life.World!.InitialQuests, q => q == "Q_SunnyvaleMaster");
        Assert.NotNull(life.Quests);
        Assert.Contains(life.Quests.Quests, q => q.Name == "Q_SunnyvaleMaster" && q.Persistent);
        Assert.NotNull(life.Runtime);
        Assert.Equal(7, life.Runtime.Quests.Count);
        Assert.Equal(7, life.Runtime.Scheduler.Fibers.Count);
        Assert.All(life.Runtime.Quests, q => Assert.NotNull(q.Fiber));
        Assert.All(life.Runtime.Quests, q => Assert.True(q.Started));
        Assert.Equal(
            QuestFactoryTable.GameflowFactory,
            life.Runtime.Quests.Single(q => q.Name == "Gameflow").Factory);
        Assert.Contains("OV_INTRO", life.GameflowStateSlots);
        Assert.Contains("SNOWSPIRE_ARRIVAL", life.GameflowStateSlots);
        Assert.Equal(
            QuestFactoryTable.GameflowStateNames.Length,
            life.GameflowStateSlots.Count);
        Assert.False(life.Runtime.HasStarted(QuestFactoryTable.GameflowScript));
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
        Assert.False(life.DeviceWindowed);
        Assert.Equal(1, life.DisplayWindowFlag);
        Assert.Equal(0x00CA0000, life.CreateWindowStyle);
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
              [0x1375468]=32 Z depth
              [0x137544A]=1 exclusive (no userst)
              009A6610 bit 0x04 → 009A64B0
                CreateWindowExW style 0xCA0000
              009C0E50 → 009BF7E0 [ebx+28]
                sete [ebp+572] Windowed=!flag
              009BEEB0 Present of 009BEF80 viewport
            004023F0 TEXT_GUI_WINDOW_TITLE
              PE 0x122D83C UTF-16
              "Fable - The Lost Chapters"
            0042E3EE input walk [0x13B8388]
              ProbeGraphics stores engine+88
              poll 009F4ED0; events 00A03B40
              frontend New Game is msg 15
            Present remains 009BEEB0 via Vulkan Draw.
            Not 1600x900. Not WASD as game input.
            Not a d3d9 ForceWindowedMode wrapper.
            Not 00DBDE40.
            """);
    }

    [Fact]
    public void Userst_00413C50_SetFullscreen_false_is_009BF7E0_windowed()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var userst = Path.Combine(install.Root, EngineLifecycle.UserstIniName);
        Assert.True(File.Exists(userst));
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        Assert.Contains(EngineLifecycle.IniSetFullscreenName, life.UserstIniCommands);
        Assert.Contains(EngineLifecycle.IniSetResolutionName, life.UserstIniCommands);
        Assert.True(life.DeviceWindowed);
        Assert.Equal(0, life.DisplayWindowFlag);
        Assert.Equal(1024, life.BackBufferWidth);
        Assert.Equal(768, life.BackBufferHeight);
        Assert.Equal(16, life.BackBufferBpp);
        Assert.Equal(0x00CA0000, life.CreateWindowStyle);
        var dest = EngineLifecycle.PresentDestFromViewport(
            life.ViewportX, life.ViewportY,
            life.ViewportWidth, life.ViewportHeight,
            life.BackBufferWidth, life.BackBufferHeight);
        Assert.Equal(0, dest.X0);
        Assert.Equal(0, dest.Y0);
        Assert.Equal(1, dest.X1);
        Assert.Equal(1, dest.Y1);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.UserstApplyFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.DisplayWindowFlagVa);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.CreateWindowFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
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

        Assert.Equal(0, life.WorldFrame);
        Assert.False(life.RenderBodyRan);
        Assert.Equal(0, life.GamePresentCount);
        life.WorldFrame = 2;
        life.QueueInput(EngineInput.TypeKey, EngineInput.KeyMove3);
        Assert.True(life.Pump(0.1f));
        Assert.True(life.RenderBodyRan);
        Assert.True(life.GamePresentCount >= 1);
        Assert.True(life.Player.PumpCalls >= 1);
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
        Assert.Equal(0, life.LayerFlushCount);
        Assert.Empty(life.SubmittedLayerBits);
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
              +232 ctor 0x1E so 00434CD0
                +216=0 01375CDC=0 skip dest fade
                009D8250 ret dest empty
              00435000 00487DD0 miss skip 00639E40
              00435070 00487DC0 miss skip 0057B43F
              009D9C80 dirty-list
              009DA9F0(1) +16020==+16024 empty
              00435530 does not call 00B25950
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
        Assert.True(life.Pump());
        Assert.Equal(0, life.WorldFrame);
        Assert.Contains(life.Player.Listeners, l => l.Vtbl == ActionInputListener.VtblVa);
        life.WorldFrame = 2;
        life.DisplayTime = 1.0;
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
        var gameflow = life.Runtime.Quests.Single(q => q.Name == "Gameflow");
        Assert.Equal(QuestFactoryTable.GameflowFactory, gameflow.Factory);
        Assert.Equal(QuestFactoryTable.GameflowScript, gameflow.ScriptName);
        Assert.DoesNotContain(life.Runtime.Quests, q => q.Name == RegionTravel.IntroScriptName);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.ActivateQuestFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.QuestRegisterFn);
        Assert.Contains(life.Trace.Events, e => e.Va == QuestFactoryTable.SunnyvaleFactory);
        Assert.Contains(life.Trace.Events, e => e.Va == QuestFactoryTable.HeroBoastsFactory);
        Assert.False(life.Runtime.HasStarted("S_HB"));
        Assert.False(life.Runtime.HasStarted("S_PSM"));
        Assert.False(life.Runtime.HasStarted(QuestFactoryTable.GameflowScript));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == QuestFactoryTable.GameflowMain &&
            e.Action.Contains("00CE75B0", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == QuestFactoryTable.GameflowSeed &&
            e.Action.Contains("00CE6CF0", StringComparison.Ordinal));
        Assert.Contains("OV_INTRO", life.GameflowStateSlots);
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
                [0x1375454]=1 .data (not BSS 0)
                Sunnyvale flag 1: 00CDBD20 + 00CDBA10
                others 004AFA10 reuse SharedRun
                00CB7900 vtbl+12 then vtbl+4
            Gameflow 00CE6CF0 seeds OV_INTRO…SNOWSPIRE_ARRIVAL
            via 008A9DB0/008AE660 [0x13BAE44].
            00CE75B0 Main watcher 00CDD450/00CB7E50.
            Not S_GF CCutsceneDef. Not S_QNOVI / 00DBDE40.
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
        Assert.False(life.WorldUpdateRan);
        Assert.False(life.PlayerCatchupHit);
        Assert.Equal(0, life.WorldFrame);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerObjectInit);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerObjectInitPredicate);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerCatchupFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.GameUpdateWorldFn);
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
            004AEBA0 reaches 004AEAA0, but first-seen
            0041674A is 0 so 00418289 skips vtbl+24
            and 0041726D. Not 00DBDE40.
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
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.LeaveFrontendTeardownFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.LeaveFrontendClearFn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PresentFn && e.Stage == "LeaveFrontend");
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GameSingletonVa);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.SkipParticlesVa);
        var leavePresent = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.PresentFn && e.Stage == "LeaveFrontend");
        var ctor = life.Trace.Events.FindIndex(e => e.Va == EngineLifecycle.GameModeCtor);
        Assert.True(leavePresent >= 0 && ctor > leavePresent,
            "0042EBB6 Present before 00418DCA");
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
        Assert.False(life.WorldUpdateRan);
        Assert.False(life.GameVtbl24Ran);
        Assert.False(life.PlayerCatchupHit);
        Assert.False(life.RenderBodyRan);
        Assert.Equal(0, life.WorldFrame);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.EngineUpdateGateFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GameUpdateFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GameRenderFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GameUpdatePlayerFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.GameUpdateWorldFn);
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
    public void LoadWorld_00416953_no_save_is_004A1840_then_0049F180()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.Null(life.CurrentRegion);
        Assert.False(life.FirstRealRegionLoadDone);
        var events = life.Trace.Events;
        var loadWorld = events.FindIndex(e =>
            e.Va == EngineLifecycle.GameLoadWorldFn &&
            e.Action.Contains("00416953", StringComparison.Ordinal));
        var prepare = events.FindIndex(e => e.Va == EngineLifecycle.WorldPrepareSite);
        var skipSave = events.FindIndex(e =>
            e.Va == EngineLifecycle.LoadSaveFn &&
            e.Action.Contains("skipped", StringComparison.Ordinal));
        var path = events.FindIndex(e =>
            e.Va == EngineLifecycle.GameWorldPathCopyFn &&
            e.Action.Contains("FinalAlbion.wld", StringComparison.Ordinal));
        var loadMap = events.FindIndex(e => e.Va == EngineLifecycle.LoadQuestsFn);
        var wad = events.FindIndex(e => e.Va == EngineLifecycle.StartupWadSite);
        var staticMap = events.FindIndex(e =>
            e.Va == EngineLifecycle.SetStaticMapForEngineSite);
        var initChars = events.FindIndex(e =>
            e.Va == EngineLifecycle.InitCharactersFn &&
            e.Stage == "Init Characters");
        var activate = events.FindIndex(e =>
            e.Va == EngineLifecycle.ActivateInitialQuestsFn);
        var after = events.FindIndex(e => e.Va == EngineLifecycle.AfterLoadWorldFn);
        Assert.True(loadWorld >= 0 && prepare > loadWorld, "vtbl+28 after 00416953");
        Assert.True(skipSave > prepare, "004A3200 skip after vtbl+28");
        Assert.True(path > skipSave, "+90576 after skip-save");
        Assert.True(loadMap > path, "004A1840 after path");
        var wld = events.FindIndex(e =>
            e.Va == EngineLifecycle.LoadWldFile &&
            e.Action.Contains("00507C30", StringComparison.Ordinal));
        var empty = events.FindIndex(e =>
            e.Va == EngineLifecycle.LevelLoaderHasWork &&
            e.Action.Contains("empty", StringComparison.Ordinal));
        var offline = events.FindIndex(e =>
            e.Va == EngineLifecycle.GenerateOfflineDataSite);
        Assert.True(wad > loadMap && wld > wad,
            "00507C30 after Startup WAD");
        Assert.True(empty > wld && offline > empty && staticMap > offline,
            "empty 006C20A0 then skip Generate Offline then Set Static Map");
        var thunk = events.FindIndex(e =>
            e.Va == EngineLifecycle.DisplayEngineSetStaticMapThunk);
        var derive = events.FindIndex(e => e.Va == EngineLifecycle.DeriveStaticMapNameFn);
        var use = events.FindIndex(e => e.Va == EngineLifecycle.SetStaticMapFileForUseFn);
        Assert.True(thunk > staticMap && derive > thunk && use > derive,
            "00B23DC0 / 0049DDD0 / 00B428E0 inside Set Static Map");
        Assert.True(initChars > use, "0049F180 after 00B428E0");
        Assert.True(activate > initChars, "004B4A10 after 0049F180");
        Assert.True(after > activate, "004BBC00 after 004B4A10");
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.AfterLoadWorldFn &&
            e.Action.Contains("ret 4", StringComparison.Ordinal));
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.GameWorldPathCopyFn &&
            e.Action.Contains("updatedscenic.wld", StringComparison.Ordinal));
        Assert.Equal("FinalAlbion.wld", life.WorldFileName);
        Assert.NotEqual(EngineLifecycle.WorldPathDefault, life.WorldFileName);
        Assert.True(life.PlayerGuiReady);
        Assert.True(life.QuestsInitDone);
        Assert.Empty(life.ActivatedQuests);
        Assert.DoesNotContain(events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(events, e => e.Va == EngineLifecycle.NamedStartFn);
        Assert.DoesNotContain(events, e => e.Va == EngineLifecycle.LoadRegionFn);
    }

    [Fact]
    public void InitGame_004184BD_after_00416953_reserves_then_user_ini()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.Null(life.CurrentRegion);
        Assert.False(life.FirstRealRegionLoadDone);
        var events = life.Trace.Events;
        var afterWorld = events.FindIndex(e => e.Va == EngineLifecycle.AfterLoadWorldFn);
        var reserve = events.FindIndex(e => e.Va == EngineLifecycle.PostLoadWorldReserveFn);
        var count = events.FindIndex(e => e.Va == EngineLifecycle.WorldThingCountFn);
        var bind = events.FindIndex(e => e.Va == EngineLifecycle.PlayerBindAfterWorldFn);
        var defaultMiss = events.FindIndex(e =>
            e.Va == EngineLifecycle.FileExistsFn &&
            e.Action.Contains("default_user.ini", StringComparison.Ordinal));
        var userIni = events.FindIndex(e =>
            e.Va == EngineLifecycle.UserIniVa &&
            e.Action.Contains("user.ini", StringComparison.Ordinal));
        var callback = events.FindIndex(e => e.Va == EngineLifecycle.EngineReadyCallback);
        var seed = events.FindIndex(e =>
            e.Va == EngineLifecycle.GameStart &&
            e.Action.Contains("90592", StringComparison.Ordinal));
        Assert.True(afterWorld >= 0 && reserve > afterWorld,
            "0049BA70 after 004BBC00");
        Assert.True(count > reserve, "00416392 after 0049BA70");
        Assert.True(bind > count, "004AE9D0 after 00416392");
        Assert.True(defaultMiss > bind, "default_user.ini miss after bind");
        Assert.True(userIni > defaultMiss, "user.ini after default miss");
        Assert.True(callback > userIni, "004167DA store after ini");
        Assert.True(seed > callback, "+90592 seed last");
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.PostLoadWorldReserveFn &&
            e.Action.Contains("count=60", StringComparison.Ordinal));
        Assert.DoesNotContain(events, e =>
            e.Va == EngineLifecycle.IniApplyFn &&
            e.Action.Contains("default_user.ini", StringComparison.Ordinal));
        Assert.DoesNotContain(events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(events, e => e.Va == EngineLifecycle.LoadRegionFn);
        Assert.False(life.GamePumpFirstDone);
    }

    [Fact]
    public void UserIni_009EC890_RunScript_joystick_is_00999230_miss()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var userIni = Path.Combine(install.Root, EngineLifecycle.UserIniName);
        Assert.True(File.Exists(userIni));
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.Contains("SetMaxAnisotropy", life.UserIniCommands);
        Assert.Contains("RunScript", life.UserIniCommands);
        Assert.Contains("ActivateQuest", life.UserIniCommands);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.IniUnknownFn &&
            e.Action.Contains("SetMaxAnisotropy", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.IniRunScriptFn &&
            e.Action.Contains("joystick.ini", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FileExistsFn &&
            e.Action.Contains("joystick.ini", StringComparison.Ordinal) &&
            e.Action.Contains("miss", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.IniActivateQuestThunk &&
            e.Action.Contains("00892E80", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.IniActivateQuestGate &&
            e.Action.Contains("xor al,al", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.ScriptManagerActivateQuestFn &&
            e.Action.Contains("004B4A10", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.ActivateQuestFn &&
            e.Action.Contains("Gameflow", StringComparison.Ordinal));
        Assert.Contains(life.Runtime!.Quests, q => q.Name == "Gameflow");
        Assert.True(life.Runtime.Quests.Single(q => q.Name == "Gameflow").Started);
        Assert.Equal(
            QuestFactoryTable.GameflowScript,
            life.Runtime.Quests.Single(q => q.Name == "Gameflow").ScriptName);
        Assert.False(life.Runtime.HasStarted(QuestFactoryTable.GameflowScript));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == QuestFactoryTable.GameflowMain);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.QuestFactoryStartFn &&
            e.Action.Contains("construct", StringComparison.Ordinal));
        Assert.Equal(1, EngineLifecycle.QuestFactoryGateFirstSeen);
        Assert.Contains("OV_INTRO", life.GameflowStateSlots);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.LoadFromFirstRealRegionFn);
        Assert.Equal(0x00892E80u, EngineLifecycle.ScriptManagerActivateQuestFn);
        Assert.Equal(1104, EngineLifecycle.ScriptManagerActivateQuestVtbl);
        Assert.Equal(0x01260F0Cu, EngineLifecycle.ScriptManagerVtbl);
        Assert.Equal(56, EngineLifecycle.WorldScriptManagerOffset);
        Assert.False(life.FirstRealRegionLoadDone);
    }

    [Fact]
    public void Gameflow_00CE75B0_is_Main_watcher_not_S_GF()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        var gameflow = life.Runtime!.Quests.Single(q => q.Name == "Gameflow");
        Assert.True(gameflow.Started);
        Assert.Equal(QuestFactoryTable.GameflowFactory, gameflow.Factory);
        Assert.Equal(QuestFactoryTable.SharedRun, gameflow.Run);
        Assert.Equal(QuestFactoryTable.GameflowMain, gameflow.Init);
        Assert.Equal(QuestFactoryTable.GameflowScript, gameflow.ScriptName);
        Assert.Null(gameflow.ChildCutscene);
        Assert.False(life.Runtime.HasStarted(QuestFactoryTable.GameflowScript));
        Assert.Equal(1, EngineLifecycle.QuestFactoryGateFirstSeen);
        Assert.Equal(0x00CDBD20u, QuestFactoryTable.SharedRun);
        Assert.Equal(0x00CE75B0u, QuestFactoryTable.GameflowMain);
        Assert.Equal(0x00CE6CF0u, QuestFactoryTable.GameflowSeed);
        Assert.Equal(0x00CB7900u, QuestFactoryTable.GameflowConstructHook);
        Assert.Equal(54, life.GameflowStateSlots.Count);
        Assert.Equal("OV_INTRO", life.GameflowStateSlots[0]);
        Assert.Equal("SNOWSPIRE_ARRIVAL", life.GameflowStateSlots[^1]);
        Assert.Equal(new[] { EngineLifecycle.WatcherMain }, life.GameflowWatchers);
        Assert.Null(life.GameflowYieldQuest);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == QuestFactoryTable.SharedRun &&
            e.Action.Contains("00CDBD20", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == QuestFactoryTable.SharedRunReuse &&
            e.Action.Contains("004AFA10", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == QuestFactoryTable.GameflowConstructHook);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == QuestFactoryTable.GameflowSeed);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == QuestFactoryTable.GameflowMain &&
            e.Action.Contains("Main", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == QuestFactoryTable.GameflowWatcherCtor);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.LoadFromFirstRealRegionFn);
        Assert.Null(life.CurrentRegion);
        Assert.False(life.FirstRealRegionLoadDone);
    }

    [Fact]
    public void Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.Equal(new[] { EngineLifecycle.WatcherMain }, life.GameflowWatchers);
        Assert.Null(life.GameflowYieldQuest);
        Assert.True(life.Pump());
        Assert.False(life.QuestPumpRan);
        Assert.True(life.Pump(0.1f));
        Assert.True(life.QuestPumpRan);
        Assert.Equal(9, life.QuestPumpWalked);
        Assert.Equal(7, life.EventPosts);
        Assert.Equal(7, life.EventPumpWalked);
        Assert.Equal(50, EngineLifecycle.EventPostDelay);
        Assert.Equal(55, EngineLifecycle.EventPostKind);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.SunnyvaleMainTick &&
            e.Action.Contains("00CDD360", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.HeroBoastsTick);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PersonalMainTick);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.EventNodeFireFn &&
            e.Action.Contains("skip", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.EventTickReadFn &&
            e.Action.Contains("13B89BC", StringComparison.Ordinal));
        Assert.Equal(0x00CDD360u, EngineLifecycle.SunnyvaleMainTick);
        Assert.Equal(0x006872B0u, EngineLifecycle.EventNodeFireFn);
        Assert.Equal(0x0049D870u, EngineLifecycle.EventTickReadFn);
        Assert.True(life.EnvironmentTicked);
        Assert.True(life.EnvironmentTime > 0f);
        Assert.True(life.BulletTimeTicked);
        Assert.True(life.ConversationTicked);
        Assert.Equal(0, life.ConversationWalked);
        Assert.True(life.ThingManagerFlushed);
        Assert.Equal(0, life.ThingManagerFlushedCount);
        Assert.True(life.OpinionTicked);
        Assert.True(life.PlayerGuiTicked);
        Assert.True(life.AtmosTicked);
        Assert.True(life.SpeechGainTicked);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.EnvironmentTickFn &&
            e.Action.Contains("006BB990", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.BulletTimeTickFn &&
            e.Action.Contains("ret", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.ConversationTickFn &&
            e.Action.Contains("empty", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.ThingManagerFlushFn &&
            e.Action.Contains("empty", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.OpinionTickFn &&
            e.Action.Contains("miss", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.SpeechGainTickFn &&
            e.Action.Contains("empty", StringComparison.Ordinal));
        var env = life.Trace.Events.FindIndex(e => e.Va == EngineLifecycle.EnvironmentTickFn);
        var cam = life.Trace.Events.FindIndex(e => e.Va == EngineLifecycle.WorldTickCameraSeedSite);
        var frame = life.Trace.Events.FindIndex(e => e.Va == EngineLifecycle.WorldFrameIncSite);
        var speech = life.Trace.Events.FindIndex(e => e.Va == EngineLifecycle.SpeechGainTickFn);
        Assert.True(env >= 0 && cam > env, "006BB990 before 006B3FF0");
        Assert.True(frame > cam, "004A5E10 after 006B3FF0");
        Assert.True(speech > frame, "006E37D0 after WorldFrame inc");
        Assert.Equal(0x006BB990u, EngineLifecycle.EnvironmentTickFn);
        Assert.Equal(15, EngineLifecycle.EnvironmentDayDivisor);
        Assert.Equal(0x004C5E90u, EngineLifecycle.BulletTimeTickFn);
        Assert.Equal(0x006E60F0u, EngineLifecycle.ConversationTickFn);
        Assert.Equal(0x0051F070u, EngineLifecycle.ThingManagerFlushFn);
        Assert.Equal(0x006BDC60u, EngineLifecycle.OpinionTickFn);
        Assert.Equal(0x0043A080u, EngineLifecycle.PlayerGuiTickFn);
        Assert.Equal(0x006B2260u, EngineLifecycle.AtmosTickFn);
        Assert.Equal(0x006E37D0u, EngineLifecycle.SpeechGainTickFn);
        Assert.Equal(0, life.GameflowState);
        Assert.Equal(EngineLifecycle.GameflowWaitQuest, life.GameflowYieldQuest);
        Assert.Contains(EngineLifecycle.WatcherCoreReminder, life.GameflowWatchers);
        Assert.Contains(EngineLifecycle.WatcherBarrowGuards, life.GameflowWatchers);
        Assert.DoesNotContain(life.ActivatedQuests, q => q == EngineLifecycle.GameflowWaitQuest);
        Assert.DoesNotContain(life.Runtime!.Quests, q => q.Name == EngineLifecycle.GameflowWaitQuest);
        Assert.DoesNotContain(life.ActivatedQuests, q => q == EngineLifecycle.TraderConflictEvil);
        Assert.DoesNotContain(life.ActivatedQuests, q => q == EngineLifecycle.TraderConflictGood);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.CoreReminderFn &&
            e.Action.Contains("+72]=0", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.BarrowGuardsFn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.QuestThingHasFn &&
            e.Action.Contains(EngineLifecycle.TraderConflictEvil, StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FiberYieldFn &&
            e.Action.Contains(EngineLifecycle.WatcherCoreReminder, StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FiberYieldFn &&
            e.Action.Contains(EngineLifecycle.WatcherBarrowGuards, StringComparison.Ordinal));
        Assert.Equal(0x00CEF3B0u, EngineLifecycle.CoreReminderFn);
        Assert.Equal(0x00CEF550u, EngineLifecycle.BarrowGuardsFn);
        Assert.Equal(0x004B0FC0u, EngineLifecycle.QuestThingHasBody);
        Assert.Equal(0x004AF610u, EngineLifecycle.QuestNameActiveBody);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.QuestListPumpFn &&
            e.Action.Contains("00CB7C40", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.FiberTickFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GameflowTickFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GameflowState0Fn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.GiveNamedObjectFn &&
            e.Action.Contains("miss", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.QuestIsActiveFn &&
            e.Action.Contains(EngineLifecycle.GameflowWaitQuest, StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FiberYieldFn &&
            e.Action.Contains(EngineLifecycle.GameflowWaitQuest, StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.QuestListWalkBFn &&
            e.Action.Contains("empty", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.LoadFromFirstRealRegionFn);
        Assert.Null(life.CurrentRegion);
        Assert.False(life.FirstRealRegionLoadDone);
        Assert.Equal(0x00CB8220u, EngineLifecycle.QuestListPumpFn);
        Assert.Equal(0x00A44880u, EngineLifecycle.FiberTickFn);
        Assert.Equal(0x00CE7670u, EngineLifecycle.GameflowTickFn);
        Assert.Equal(0x006E7410u, EngineLifecycle.GameflowYieldThunk);
        Assert.Equal(0x004167DAu, EngineLifecycle.EngineReadyCallback);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.EngineReadyCallback &&
            e.Action.Contains("call", StringComparison.Ordinal));
    }

    [Fact]
    public void Type1_resume_00CB8220_is_00A44880_then_00893610_yield()
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
        Assert.True(life.Pump(0.25f));
        Assert.Equal(EngineLifecycle.GameflowWaitQuest, life.GameflowYieldQuest);
        Assert.Equal(1, life.Trace.Events.Count(e =>
            e.Va == EngineLifecycle.GiveNamedObjectFn));
        Assert.True(life.Pump(0.25f));
        Assert.True(life.QuestPumpRan);
        Assert.True(life.QuestPumpWalked >= 1);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.QuestFiberAttachFn &&
            e.Action.Contains("resume", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FiberTickFn &&
            e.Action.Contains("00A44880", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FiberResumeFn &&
            e.Action.Contains("009D87F0", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.QuestIsActiveFn &&
            e.Action.Contains(EngineLifecycle.GameflowWaitQuest, StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FiberYieldFn &&
            e.Action.Contains(EngineLifecycle.GameflowWaitQuest, StringComparison.Ordinal));
        Assert.Equal(1, life.Trace.Events.Count(e =>
            e.Va == EngineLifecycle.GiveNamedObjectFn));
        Assert.Equal(1, life.GameflowWatchers.Count(w => w == EngineLifecycle.WatcherCoreReminder));
        Assert.Equal(1, life.GameflowWatchers.Count(w => w == EngineLifecycle.WatcherBarrowGuards));
        Assert.DoesNotContain(life.ActivatedQuests, q => q == EngineLifecycle.GameflowWaitQuest);
        Assert.DoesNotContain(life.Runtime!.Quests, q => q.Name == EngineLifecycle.GameflowWaitQuest);
        Assert.Equal(EngineLifecycle.GameflowWaitQuest, life.GameflowYieldQuest);
        Assert.Equal(0, life.GameflowState);
        Assert.Equal(0x00A44880u, EngineLifecycle.FiberTickFn);
        Assert.Equal(0x00A44660u, EngineLifecycle.FiberResumeFn);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.LoadFromFirstRealRegionFn);
        Assert.Null(life.CurrentRegion);
    }

    [Fact]
    public void No_save_does_not_activate_Q_NewOakValeIntro()
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
        Assert.True(life.Pump(0.25f));
        Assert.Contains(life.World!.InitialQuests, q => q == "Q_SunnyvaleMaster");
        Assert.DoesNotContain(life.World.InitialQuests, q =>
            q == EngineLifecycle.GameflowWaitQuest);
        Assert.DoesNotContain(life.ActivatedQuests, q =>
            q == EngineLifecycle.GameflowWaitQuest);
        Assert.DoesNotContain(life.Runtime!.Quests, q =>
            q.Name == EngineLifecycle.GameflowWaitQuest);
        Assert.Equal(EngineLifecycle.GameflowWaitQuest, life.GameflowYieldQuest);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.OakvaleBindSite &&
            e.Action.Contains("bind not 00CB5AD0", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.AddTestQuestStoreFn &&
            e.Action.Contains("store not 004B4A10", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.StartNewQuestParseFn &&
            e.Action.Contains("0 E8 no-save", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.ActivateInitialQuestsSite &&
            e.Action.Contains("skip 004B4A10", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.QuestIsActiveFn &&
            e.Action.Contains(EngineLifecycle.GameflowWaitQuest, StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.LoadFromFirstRealRegionFn);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == RegionTravel.StartOakValeSetup);
        Assert.Equal(0x00CD6E27u, EngineLifecycle.OakvaleBindSite);
        Assert.Equal(0x00DBEF70u, EngineLifecycle.OakvaleFactoryFn);
        Assert.Equal(0x004B5080u, EngineLifecycle.StartNewQuestParseFn);
        Assert.Equal(0x004B0D30u, EngineLifecycle.QuestCardFindFn);
        Assert.Equal(0x004A113Bu, EngineLifecycle.AddTestQuestStoreFn);
        Assert.Null(life.CurrentRegion);
    }

    [Fact]
    public void After_WorldFrame_gt_1_00416E78_is_004457F0_then_00446A30()
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
        Assert.True(life.Pump(0.25f));
        Assert.Equal(1, life.WorldFrame);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.GameVtbl24Fn &&
            e.Action.Contains("0049D870<=1 skip 004457F0", StringComparison.Ordinal));
        Assert.True(life.Pump(0.25f));
        Assert.Equal(2, life.WorldFrame);
        Assert.True(life.Pump(0.25f));
        Assert.Equal(3, life.WorldFrame);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.GetForegroundWindowIat &&
            e.Action.Contains("00416F9D 009A57B0", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.WorldFrameGetter &&
            e.Action.Contains("0049D870 frame=2", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PlayerInterfacePreprocess &&
            e.Action.Contains("004457F0 [+2196]=0", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PlayerInputPumpFn &&
            e.Action.Contains("00446A30 [game+32] vtbl+4", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PlayerInputPollFn &&
            e.Action.Contains("00446330", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PlayerInputFallbackFn &&
            e.Action.Contains("00446220 vtbl+24 [+168]=0", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PlayerInputPumpFn &&
            e.Action.Contains("00446A30 al=0 no 0041649C", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PlayerApplyFn);
        Assert.Equal(0, life.Player.DeliveredCount);
        Assert.True(life.Player.Present);
        Assert.Equal(0x009A57B0u, EngineLifecycle.EngineUpdateGateFn);
        Assert.Equal(0x0049D870u, EngineLifecycle.WorldFrameGetter);
        Assert.Equal(0x004457F0u, EngineLifecycle.PlayerInterfacePreprocess);
        Assert.Equal(0x00446A30u, EngineLifecycle.PlayerInputPumpFn);
        Assert.Equal(0x00446330u, EngineLifecycle.PlayerInputPollFn);
        Assert.Equal(0x00446220u, EngineLifecycle.PlayerInputFallbackFn);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.LoadFromFirstRealRegionFn);
        Assert.Null(life.CurrentRegion);
    }

    [Fact]
    public void After_WorldFrame_gt_1_00417001_is_0041707E_then_004AEA70_skip()
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
        Assert.True(life.Pump(0.25f));
        Assert.Equal(1, life.WorldFrame);
        Assert.False(life.RenderBodyRan);
        Assert.False(life.CameraInterpolationRan);
        Assert.Equal(0, life.GamePresentCount);
        Assert.True(life.WorldCamera.Seeded);
        Assert.True(life.Pump(0.25f));
        Assert.Equal(2, life.WorldFrame);
        Assert.True(life.RenderBodyRan);
        Assert.True(life.CameraInterpolationRan);
        Assert.False(life.CameraInterpolationUnread);
        Assert.Equal(1f, life.CameraInterpolationT);
        Assert.True(life.DisplayPresentSkipped);
        Assert.False(life.GamePlus90594);
        Assert.Equal(1, life.GamePlus90596);
        Assert.Equal(0, life.GamePresentCount);
        Assert.True(life.PlayerActionReady);
        Assert.Equal(2, life.PlayerBindSlot0);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.CameraInterpolationFn &&
            e.Action.Contains("0041707E", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.WorldCameraApplyFn &&
            e.Action.Contains("0049E080", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.CameraManagerBlendFn &&
            e.Action.Contains("006B42F0", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PlayerReadyQueryFn &&
            e.Action.Contains("004AEA70 0041674A", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayApplyThunk &&
            e.Action.Contains("skip 00435F70", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.CameraBodyFn);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PresentFn &&
            e.Stage == "GamePump");
        Assert.Equal(0x0041707Eu, EngineLifecycle.CameraInterpolationFn);
        Assert.Equal(0x0049E080u, EngineLifecycle.WorldCameraApplyFn);
        Assert.Equal(0x006B42F0u, EngineLifecycle.CameraManagerBlendFn);
        Assert.Equal(0x004AEA70u, EngineLifecycle.PlayerReadyQueryFn);
        Assert.Equal(0x00435F70u, EngineLifecycle.DisplayApplyThunk);
        Assert.Equal(0x013B8688u, EngineLifecycle.PlayerCatchupForceVa);
        Assert.Equal(0, EngineLifecycle.PlayerCatchupForceFirstSeen);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.LoadFromFirstRealRegionFn);
        Assert.Null(life.CurrentRegion);
    }

    [Fact]
    public void After_004AEA70_eq_1_00417001_is_00435F70_Present()
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
        Assert.True(life.Pump(0.25f));
        Assert.True(life.Pump(0.25f));
        Assert.Equal(2, life.WorldFrame);
        Assert.True(life.DisplayPresentSkipped);
        Assert.Equal(0, life.GamePresentCount);
        var n = 0;
        while (n < 8 && life.GamePresentCount == 0)
        {
            Assert.True(life.Pump());
            n++;
        }

        Assert.Equal(5, n);
        Assert.Equal(7, life.WorldFrame);
        Assert.Equal(7, life.PlayerBindSlot0);
        Assert.Equal(1, life.GamePresentCount);
        Assert.False(life.DisplayPresentSkipped);
        Assert.True(life.GamePlus90594);
        Assert.Equal(5, life.GamePlus90596);
        Assert.True(life.CameraInterpolationRan);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PlayerReadyQueryFn &&
            e.Action.Contains("004AEA70 0041674A", StringComparison.Ordinal) &&
            e.Action.Contains("<=1 → 1", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayApplyThunk &&
            e.Action.Contains("00435F70 jmp 00435530", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayApplyBodyFn &&
            e.Action.Contains("00434CD0", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayFadeDestStub &&
            e.Action.Contains("009D8250 ret dest empty", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayPlayerOverlayFn &&
            e.Action.Contains("skip 00639E40", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayPlayerInterfaceFn &&
            e.Action.Contains("skip 0057B43F", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.BeginSceneFn && e.Stage == "GamePump");
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.ClearColorFn && e.Stage == "GamePump");
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayFlush2dFn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayFlushLayersFn &&
            e.Action.Contains("empty dest", StringComparison.Ordinal));
        Assert.Empty(life.SubmittedLayerBits);
        Assert.Equal(0, life.LayerFlushCount);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.RenderFrameFn && e.Stage == "GamePump");
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.EndSceneFn && e.Stage == "GamePump");
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PresentFn && e.Stage == "GamePump");
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.GamePresentSite);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.CameraBodyFn);
        Assert.Equal(0x00435F70u, EngineLifecycle.DisplayApplyThunk);
        Assert.Equal(0x00435530u, EngineLifecycle.DisplayApplyBodyFn);
        Assert.Equal(0x009BEEB0u, EngineLifecycle.PresentFn);
        Assert.Equal(0x00434E10u, EngineLifecycle.DisplayCtorFn);
        Assert.Equal(0x00434CD0u, EngineLifecycle.DisplayFadeDestFn);
        Assert.Equal(0x009D8250u, EngineLifecycle.DisplayFadeDestStub);
        Assert.Equal(0x1E, EngineLifecycle.DisplayPlus232Ctor);
        Assert.Equal(0x1E - 7, life.DisplayPlus232);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.LoadFromFirstRealRegionFn);
        Assert.Null(life.CurrentRegion);
        Assert.False(life.WorldSubmitted);
    }

    [Fact]
    public void First_pump_004189C2_is_0040D2A0_then_00B239A0_not_a_region()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.False(life.GamePumpFirstDone);
        Assert.False(life.PlayAviSingletonReady);
        Assert.True(life.Pump());
        Assert.True(life.GamePumpFirstDone);
        Assert.True(life.PlayAviSingletonReady);
        Assert.True(life.DisplayEngineFadeSet);
        Assert.Equal(12, life.DisplayEngineFadeKind);
        Assert.Equal(20f, life.DisplayEngineFadeTime);
        Assert.Equal(0, life.CurrentRegionIndex);
        Assert.Null(life.CurrentRegion);
        Assert.False(life.FirstRealRegionLoadDone);
        var events = life.Trace.Events;
        var dummy = events.FindIndex(e =>
            e.Va == EngineLifecycle.GetRegionRecordFn &&
            e.Action.Contains("dummy", StringComparison.Ordinal));
        var avi = events.FindIndex(e => e.Va == EngineLifecycle.PlayAviSingletonFn);
        var ctor = events.FindIndex(e => e.Va == EngineLifecycle.PlayAviSingletonCtor);
        var apply = events.FindIndex(e => e.Va == EngineLifecycle.PlayAviApplyBodyFn);
        var fade = events.FindIndex(e => e.Va == EngineLifecycle.DisplayEngineFadeFn);
        var enter = events.FindIndex(e => e.Va == EngineLifecycle.InputLockEnterFn);
        var leave = events.FindIndex(e => e.Va == EngineLifecycle.InputLockLeaveFn);
        var innerDt = events.FindIndex(e => e.Va == EngineLifecycle.InnerLoopDtFn);
        var update = events.FindIndex(e => e.Va == EngineLifecycle.GamePumpUpdate);
        Assert.True(dummy >= 0 && avi > dummy, "0040D2A0 after dummy record");
        Assert.True(ctor > avi && apply > ctor, "0040CEC0 then 0040A7F0");
        Assert.True(fade > apply, "00B239A0 after 0040A7F0");
        Assert.True(enter > fade && leave > enter, "009F2660/009F26B0 after fade");
        Assert.True(innerDt > leave && update > innerDt,
            "009F8BA0 then 004162B5 after first-pump tail");
        var ring = events.FindIndex(e => e.Va == EngineLifecycle.FrameDtRingFn);
        var memlog = events.FindIndex(e => e.Va == EngineLifecycle.GamePumpMemlog);
        var idle = events.FindIndex(e => e.Va == EngineLifecycle.PlayerManagerIdleFn);
        Assert.True(ring > update, "00416202 after 004162B5");
        Assert.True(memlog > ring, "00415E85 after 00416202");
        Assert.True(idle > memlog, "009AC9E0 after 00415E85");
        Assert.Equal(1, life.FrameDtRingSamples);
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.GamePumpMemlog &&
            e.Action.Contains("skip", StringComparison.Ordinal));
        Assert.False(life.InputRecordStored);
        Assert.False(life.PlayerCatchupHit);
        Assert.False(life.GameVtbl24Ran);
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.PlayerCatchupFn &&
            e.Action.Contains("<= 1", StringComparison.Ordinal));
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.PlayerActionFn &&
            e.Action.Contains("004AEB8A", StringComparison.Ordinal));
        Assert.DoesNotContain(events, e => e.Va == EngineLifecycle.GameVtbl24Fn);
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.DisplayEngineFadeFn &&
            e.Action.Contains("20", StringComparison.Ordinal));
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.GamePumpInnerStartFn &&
            e.Action.Contains("ret", StringComparison.Ordinal));
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.GamePumpQuitQuery &&
            e.Action.Contains("→ 1", StringComparison.Ordinal));
        Assert.DoesNotContain(events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(events, e => e.Va == EngineLifecycle.LoadRegionFn);
        Assert.DoesNotContain(events, e => e.Va == EngineLifecycle.LoadFromFirstRealRegionFn);
    }

    [Fact]
    public void First_pump_0041674A_is_0_so_00418289_skips_00416E78()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.Equal(0, life.PlayerBindSlot0);
        Assert.Equal(0, life.PlayerBindSlot1);
        Assert.Equal(0, life.PlayerBindSlot2);
        Assert.True(life.Pump());
        Assert.True(life.GamePlus9);
        Assert.Equal(0.0, life.DisplayTime);
        Assert.False(life.PlayerCatchupHit);
        Assert.False(life.GameVtbl24Ran);
        Assert.False(life.WorldUpdateRan);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.TickListAppendFn);
        life.DisplayTime = 1.0;
        life.UpdateGameMode();
        Assert.True(life.PlayerCatchupHit);
        Assert.True(life.GameVtbl24Ran);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.GameVtbl24Fn &&
            e.Action.Contains("skip 004457F0", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.TickListAppendFn);
    }

    [Fact]
    public void Pump_004166E2_is_009E1BC0_minus_game_plus96()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.True(life.Pump());
        Assert.Equal(0.0, life.GamePlus96);
        Assert.Equal(0.0, life.FrameDtNow);
        Assert.Equal(0.0, life.DisplayTime);
        Assert.False(life.PlayerCatchupHit);
        Assert.True(life.Pump(0.1f));
        Assert.Equal(0.0, life.GamePlus96);
        Assert.Equal(0.1f, (float)life.FrameDtNow);
        Assert.Equal(0.1f, (float)life.DisplayTime);
        Assert.True(life.PlayerCatchupHit);
        Assert.True(life.GameVtbl24Ran);
        Assert.Equal(1, life.TickListCount);
        Assert.Equal(new[] { EngineLifecycle.WorldTickType }, life.GameTickTypes);
        Assert.Equal(1, life.TickRecordWatermark);
        Assert.Equal(1, life.WorldFrame);
        Assert.Equal(1, life.PlayerBindSlot0);
        Assert.Equal(1, life.GamePlus72);
        Assert.Equal(1, life.GamePlus76);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PlayerCatchupTimeFn &&
            e.Action.Contains("009E1BC0-[game+96]", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PlayerCatchupFn &&
            e.Action.Contains("> 1", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerBindIncSite);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.TickListClearFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.WalkTickBeforeDispatchFn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.ApplyTickTypeFn &&
            e.Action.Contains("skip", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.WalkTickAfterDispatchFn &&
            e.Action.Contains("skip", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.WorldTickCameraSeedSite);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.WorldFrameIncSite);
        Assert.True(life.WorldCamera.Seeded);
        Assert.True(life.QuestPumpRan);
        Assert.Equal(0, life.QuestPumpWalked);
        Assert.True(life.ScriptPumpRan);
        Assert.Equal(0, life.ScriptPumpWalked);
        Assert.True(life.EventPumpRan);
        Assert.Equal(0, life.EventPumpWalked);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.EventManagerPumpFn &&
            e.Action.Contains("[world+96]", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.EventManagerCtor &&
            e.Action.Contains("empty", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.EventManagerPostFn &&
            e.Action.Contains("skip", StringComparison.Ordinal));
        Assert.Equal(0x006874B0u, EngineLifecycle.EventManagerPumpFn);
        Assert.Equal(0x00687510u, EngineLifecycle.EventManagerCtor);
        Assert.Equal(0x00687540u, EngineLifecycle.EventManagerPostFn);
        Assert.Equal(96, EngineLifecycle.WorldEventManagerOffset);
        Assert.Equal(4, life.PlayerSlotTicks);
        Assert.True(life.DisplayListenerPumped);
        Assert.False(life.DisplayActiveApplyRan);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PlayerSlotTickFn &&
            e.Action.Contains("skip 004887C0", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayActiveGateFn &&
            e.Action.Contains("skip 00B24030", StringComparison.Ordinal));
        Assert.Equal(0x00488AB0u, EngineLifecycle.PlayerSlotTickFn);
        Assert.Equal(0x00436FB0u, EngineLifecycle.DisplayListenerGetFn);
        Assert.Equal(0x00640320u, EngineLifecycle.DisplayListenerPumpFn);
        Assert.Equal(0x00B23550u, EngineLifecycle.DisplayActiveGateFn);
        Assert.Equal(0, EngineLifecycle.DisplayPlus8FirstSeen);
        Assert.Equal(1, EngineLifecycle.PlayerSlotPlus4FirstSeen);
        Assert.Equal(1, EngineLifecycle.PlayerSlotPlus534FirstSeen);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.QuestListPumpFn &&
            e.Action.Contains("skip empty", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.ScriptManagerPumpFn &&
            e.Action.Contains("flag=1", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.ScriptGuiGateFn &&
            e.Action.Contains("+246=0", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.ScriptListIterFn &&
            e.Action.Contains("skip", StringComparison.Ordinal));
        Assert.Equal(0x006E75C0u, EngineLifecycle.ScriptManagerPumpFn);
        Assert.Equal(0x006E7740u, EngineLifecycle.ScriptManagerCtor);
        Assert.Equal(0x00892270u, EngineLifecycle.ScriptGuiGateFn);
        Assert.Equal(0x013B8790u, EngineLifecycle.PlayerGuiInstanceVa);
        Assert.Equal(0, EngineLifecycle.GuiPlus246FirstSeen);
        Assert.Equal(0, EngineLifecycle.ScriptManagerPlus44FirstSeen);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerCreatureBindFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerCreatureThingFn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PlayerThingSmartPtrFn &&
            e.Action.Contains("miss", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.QuestPlayerSyncFn &&
            e.Action.Contains("skip", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.QuestFiberAttachFn);
        Assert.Equal(0x00A01B50u, EngineLifecycle.PlayerThingSmartPtrFn);
        Assert.Equal(0x004AFCA0u, EngineLifecycle.QuestPlayerSyncFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.LoadFromFirstRealRegionFn);
        Assert.Equal(0x01375454u, EngineLifecycle.QuestFactoryGateVa);
        Assert.Equal(1, EngineLifecycle.QuestFactoryGateFirstSeen);
        Assert.Equal(0x00416670u, EngineLifecycle.WalkTickBeforeDispatchFn);
        Assert.Equal(0x00415FE0u, EngineLifecycle.ApplyTickTypeFn);
        Assert.Equal(0x00434A60u, EngineLifecycle.WalkTickAfterDispatchFn);
        Assert.Equal(0x013B92F8u, EngineLifecycle.WorldTickSlot1Plus48Va);
        Assert.Equal(0, EngineLifecycle.WorldTickSlot1Plus48FirstSeen);
        Assert.Equal(0x004A5DF3u, EngineLifecycle.WorldTickCameraSeedSite);
        Assert.Equal(0x0143FE00u, EngineLifecycle.FrameDtQpcIat);
        Assert.Equal(0x0143FE04u, EngineLifecycle.FrameDtQpfIat);
        Assert.Equal(0x013B86A4u, EngineLifecycle.DisplayClockForceQpcVa);
        Assert.Equal(0, EngineLifecycle.DisplayClockForceQpcFirstSeen);
        Assert.Equal(0x648, EngineLifecycle.TickListStride);
        Assert.Equal(0x192, EngineLifecycle.TickListCopyDwords);
        Assert.Equal(8208, EngineLifecycle.PlayerTickBuilderOffset);
        Assert.Equal(0x009F16C0u, EngineLifecycle.TickBuilderResetFn);
        Assert.Equal(0x009F1750u, EngineLifecycle.TickListCountFn);
        Assert.True(life.Pump(0.1f));
        Assert.Equal(2, life.WorldFrame);
        Assert.Equal(2, life.PlayerBindSlot0);
    }

    [Fact]
    public void First_pump_00416202_is_0049B9E0_then_00415E85_skip()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.Equal(0, life.FrameDtRingSamples);
        Assert.True(life.Pump());
        Assert.Equal(1, life.FrameDtRingSamples);
        var events = life.Trace.Events;
        var update = events.FindIndex(e => e.Va == EngineLifecycle.GamePumpUpdate);
        var ring = events.FindIndex(e => e.Va == EngineLifecycle.FrameDtRingFn);
        var mean = events.FindIndex(e => e.Va == EngineLifecycle.FrameDtRingMeanFn);
        var memlog = events.FindIndex(e => e.Va == EngineLifecycle.GamePumpMemlog);
        var getter = events.FindLastIndex(e =>
            e.Va == EngineLifecycle.PlayerManagerGetter && e.Stage == "GamePump");
        var idle = events.FindIndex(e => e.Va == EngineLifecycle.PlayerManagerIdleFn);
        Assert.True(update >= 0 && ring > update, "00416202 after 004162B5");
        Assert.True(mean > ring && memlog > mean, "0049B9A0 then 00415E85");
        Assert.True(getter > memlog && idle > getter, "0044C6B0 then 009AC9E0");
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.MemlogFlagVa &&
            e.Action.Contains("013B85F1=0", StringComparison.Ordinal));
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.GamePumpMemlog &&
            e.Action.Contains("skip", StringComparison.Ordinal));
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.PlayerManagerIdleFn &&
            e.Action.Contains("ret 4", StringComparison.Ordinal));
        Assert.DoesNotContain(events, e =>
            e.Va == EngineLifecycle.GamePumpMemlog &&
            e.Action.Contains("memlog", StringComparison.Ordinal));
        Assert.Equal(0x00416202u, EngineLifecycle.FrameDtRingFn);
        Assert.Equal(0x0049B9E0u, EngineLifecycle.FrameDtRingPushFn);
        Assert.Equal(0x0049B9A0u, EngineLifecycle.FrameDtRingMeanFn);
        Assert.Equal(0x013B85F1u, EngineLifecycle.MemlogFlagVa);
        Assert.Equal(0, EngineLifecycle.MemlogFlagFirstSeen);
        Assert.Equal(0x009AC9E0u, EngineLifecycle.PlayerManagerIdleFn);
    }

    [Fact]
    public void Second_pump_004189C2_loops_inner_not_00501450()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.True(life.Pump());
        Assert.True(life.GamePumpFirstDone);
        Assert.False(life.FirstRealRegionLoadDone);
        Assert.False(life.GamePlus8);
        var before = life.Trace.Events.Count;
        Assert.True(life.Pump());
        Assert.False(life.FirstRealRegionLoadDone);
        Assert.False(life.GamePlus8);
        Assert.Equal(2, life.FrameDtRingSamples);
        var later = life.Trace.Events.Skip(before).ToList();
        Assert.Contains(later, e =>
            e.Va == EngineLifecycle.GamePumpQuitQuery &&
            e.Action.Contains("→ 1", StringComparison.Ordinal));
        Assert.Contains(later, e => e.Va == EngineLifecycle.GamePumpInnerStartFn);
        Assert.Contains(later, e => e.Va == EngineLifecycle.GamePumpUpdate);
        Assert.Contains(later, e =>
            e.Va == EngineLifecycle.GamePump &&
            e.Action.Contains("[game+8]=0 loop", StringComparison.Ordinal));
        Assert.DoesNotContain(later, e => e.Va == EngineLifecycle.LoadFromFirstRealRegionFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.LoadRegionFn);
        Assert.Equal(0, EngineLifecycle.EnginePlus8FirstSeen);
        Assert.Equal(0, EngineLifecycle.GamePlus8FirstSeen);
        Assert.Equal(1, EngineLifecycle.GamePumpQuitFirstSeen);
        Assert.Equal(2, EngineLifecycle.GamePumpQuitLeave);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.EngineMessagePumpFn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PeekMessageFn &&
            e.Action.Contains("empty", StringComparison.Ordinal));
        var peek = life.Trace.Events.FindIndex(e => e.Va == EngineLifecycle.PeekMessageFn);
        var focus = life.Trace.Events.FindIndex(e => e.Va == EngineLifecycle.InputFocusFn);
        var coop = life.Trace.Events.FindIndex(e => e.Va == EngineLifecycle.TestCooperativeLevelFn);
        Assert.True(peek >= 0 && focus > peek && coop > focus,
            "009F4E20 after empty PeekMessage before 009C00C0");
        Assert.True(life.EnginePlus88);
        Assert.False(life.EnginePlus124);
        Assert.Equal(1, life.EnginePlus9);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.TestCooperativeLevelFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.EngineQuitStoreSite);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.GamePumpLeaveFn);
    }

    [Fact]
    public void WmDestroy_009A5BEA_sets_engine_plus8_and_leaves_004189C2()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.True(life.Pump());
        Assert.False(life.GamePlus8);
        Assert.Equal(0, life.EnginePlus8);
        life.ApplyEngineWindowMessage(EngineLifecycle.WmDestroy);
        Assert.Equal(1, life.EnginePlus8);
        Assert.True(life.Pump());
        Assert.True(life.GamePlus8);
        Assert.True(life.GamePumpLeft);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.EngineQuitStoreSite &&
            e.Action.Contains("WM_DESTROY", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.GamePumpQuitQuery &&
            e.Action.Contains("→ 2", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.GamePump &&
            e.Action.Contains("[game+8]=1 leave", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.GamePumpLeaveFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.LoadFromFirstRealRegionFn);
        Assert.Equal(0x009A6370u, EngineLifecycle.EngineMessagePumpFn);
        Assert.Equal(0x009A4F20u, EngineLifecycle.PeekMessageFn);
        Assert.Equal(0x009A5B60u, EngineLifecycle.EngineWndProc);
        Assert.Equal(0x009A5F7Cu, EngineLifecycle.EngineWndProcJumpTable);
        Assert.Equal(0x009A5BEAu, EngineLifecycle.EngineQuitStoreSite);
        Assert.Equal(2, EngineLifecycle.WmDestroy);
        Assert.Equal(0x004175E5u, EngineLifecycle.GamePumpLeaveFn);
        Assert.Equal(0x009C00C0u, EngineLifecycle.TestCooperativeLevelFn);
        Assert.Equal(0x009F4E20u, EngineLifecycle.InputFocusFn);
        Assert.Equal(5, EngineLifecycle.EngineOptionsFlagsFirstSeen);
        Assert.Equal(0, EngineLifecycle.EngineOptionsFlagFirstSeen);
        Assert.Equal(88, EngineLifecycle.EnginePlus88Offset);
        Assert.Equal(124, EngineLifecycle.EnginePlus124Offset);
    }

    [Fact]
    public void First_pump_009F4E20_is_after_empty_PeekMessage()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.True(life.EnginePlus88);
        Assert.False(life.EnginePlus124);
        Assert.Equal(1, life.EnginePlus9);
        Assert.Equal(5, EngineLifecycle.EngineOptionsFlagsFirstSeen);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.InputFocusFn);
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.True(life.Pump());
        var events = life.Trace.Events;
        var peek = events.FindIndex(e => e.Va == EngineLifecycle.PeekMessageFn);
        var focus = events.FindIndex(e => e.Va == EngineLifecycle.InputFocusFn);
        var coop = events.FindIndex(e => e.Va == EngineLifecycle.TestCooperativeLevelFn);
        Assert.True(peek >= 0 && focus > peek, "009F4E20 after 009A4F20");
        Assert.True(coop > focus, "009C00C0 after 009F4E20");
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.InputFocusFn &&
            e.Action.Contains("+88", StringComparison.Ordinal));
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.CreateInputFn);
        Assert.DoesNotContain(events, e =>
            e.Va == 0x00A3EB20u);
    }

    [Fact]
    public void Second_pump_00501450_is_004FEEC0_then_00500540_1_0_0()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.False(life.GamePumpFirstDone);
        Assert.True(life.Pump());
        Assert.True(life.GamePumpFirstDone);
        Assert.Equal(0, life.CurrentRegionIndex);
        Assert.Null(life.CurrentRegion);
        Assert.False(life.FirstRealRegionLoadDone);
        life.LoadFromFirstRealRegion();
        Assert.True(life.FirstRealRegionLoadDone);
        var events = life.Trace.Events;
        var dummy = events.FindIndex(e =>
            e.Va == EngineLifecycle.GetRegionRecordFn &&
            e.Action.Contains("dummy", StringComparison.Ordinal));
        var enqueue = events.FindIndex(e =>
            e.Va == EngineLifecycle.LoadFromFirstRealRegionFn &&
            e.Action.Contains("00501450", StringComparison.Ordinal));
        var unload = events.FindIndex(e => e.Va == EngineLifecycle.UnloadCurrentRegionFn);
        var first = events.FindIndex(e =>
            e.Va == EngineLifecycle.LoadRegionFn &&
            e.Action.Contains("(1,0,0)", StringComparison.Ordinal));
        var second = events.FindIndex(e =>
            e.Va == EngineLifecycle.LoadRegionFn &&
            e.Action.Contains("(2,0,0)", StringComparison.Ordinal));
        var last = events.FindIndex(e =>
            e.Va == EngineLifecycle.LoadRegionFn &&
            e.Action.Contains("(141,0,0)", StringComparison.Ordinal));
        var collect = events.FindIndex(e =>
            e.Va == EngineLifecycle.CollectRegionThingsFn &&
            e.Action.Contains("after 1", StringComparison.Ordinal));
        var graph = events.FindIndex(e => e.Va == EngineLifecycle.RegionGraphNameVa);
        var restore = events.FindIndex(e =>
            e.Va == EngineLifecycle.LoadFromFirstRealRegionFn &&
            e.Action.Contains("(0,0,1)", StringComparison.Ordinal));
        Assert.True(dummy >= 0 && enqueue > dummy, "00501450 body after dummy; not a 004189C2 E8");
        Assert.True(unload > enqueue, "004FEEC0 after 00501450");
        Assert.True(first > unload, "00500540(1,0,0) after 004FEEC0");
        Assert.True(second > first && last > second,
            "00500540(i,0,0) through count-1");
        Assert.True(collect > first, "0048D400 after first 00500540");
        Assert.True(graph > last && restore > graph,
            "RegionGraph.txt then 00500540(0,0,1)");
        Assert.Equal(141, life.CurrentRegionIndex);
        Assert.Equal("Filler_NorthernWastes_02", life.CurrentRegion!.RegionName);
        Assert.Equal(0x0048D400u, EngineLifecycle.CollectRegionThingsFn);
        Assert.Equal(0x0049C770u, EngineLifecycle.CollectThingsListFn);
        Assert.Equal(0x006A80A0u, EngineLifecycle.CollectThingsBitTestFn);
        Assert.Equal(0x64, EngineLifecycle.CollectThingsBitIndex);
        Assert.Equal(145, EngineLifecycle.ThingCollectFlagsOffset);
        Assert.Equal(0x0C, EngineLifecycle.ThingCollectFlagsNeed);
        Assert.Equal(0x21, EngineLifecycle.ThingCollectFlagsForbid);
        Assert.Equal(0x005198B0u, EngineLifecycle.CollectScriptedHookThingsFn);
        Assert.Equal(0x00518DC0u, EngineLifecycle.ScriptedHookCollectFn);
        Assert.Equal("CTCActionUseScriptedHook", EngineLifecycle.ScriptedHookName);
        Assert.Equal(0xC2, EngineLifecycle.ScriptedHookKey);
        Assert.Equal("RegionGraph.txt", EngineLifecycle.RegionGraphName);
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.CollectScriptedHookThingsFn &&
            e.Action.Contains("CTCActionUseScriptedHook", StringComparison.Ordinal));
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.CollectRegionThingsFn &&
            e.Action.Contains("0x64", StringComparison.Ordinal));
        Assert.DoesNotContain(events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(events, e => e.Va == EngineLifecycle.NamedStartFn);
    }

    [Fact]
    public void Apply_006C2170_is_topology_then_objects_then_004FCBB0()
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
        life.LoadFromFirstRealRegion();
        Assert.Contains("LookoutPoint", life.ActivatedMaps);
        var events = life.Trace.Events;
        var job = events.FindIndex(e => e.Va == EngineLifecycle.BuildLoadJobFn);
        var copy = events.FindIndex(e => e.Va == EngineLifecycle.BuildLoadJobCopyMapsFn);
        var enqueue = events.FindIndex(e => e.Va == EngineLifecycle.EnqueueLoadJobFn);
        var update = events.FindIndex(e =>
            e.Va == EngineLifecycle.LevelLoaderUpdate &&
            e.Action.Contains("006C2710", StringComparison.Ordinal));
        var apply = events.FindIndex(e =>
            e.Va == EngineLifecycle.LevelLoaderApply &&
            e.Action.Contains("006C2170", StringComparison.Ordinal));
        var topo = events.FindIndex(e => e.Va == EngineLifecycle.LoadTopologyFn);
        var objects = events.FindIndex(e =>
            e.Va == EngineLifecycle.LevelLoaderApply &&
            e.Action.Contains("Loading objects", StringComparison.Ordinal));
        var postInit = events.FindIndex(e => e.Va == EngineLifecycle.PostLoadInitialiseFn);
        var activate = events.FindIndex(e => e.Va == EngineLifecycle.ActivateTopologyFn);
        var loaded = events.FindIndex(e =>
            e.Va == EngineLifecycle.SetRegionAsLoadedFn &&
            e.Action.Contains("+156=1", StringComparison.Ordinal));
        var end = events.FindIndex(e =>
            e.Va == EngineLifecycle.LevelLoaderUpdate &&
            e.Action.Contains("end", StringComparison.Ordinal));
        Assert.True(job >= 0 && copy > job && enqueue > copy, "006C27A0 then 006C2120");
        Assert.True(update > enqueue && apply > update, "006C2710 then 006C2170");
        Assert.True(topo > apply && objects > topo, "Loading topology before objects");
        Assert.True(postInit > objects && activate > postInit,
            "004FD020 then 004FCBB0 after objects");
        Assert.True(loaded > activate, "004FC8A0 after 004FCBB0");
        Assert.True(end > loaded, "Level loader update end after apply");
        Assert.Contains(life.ActivatedMaps, m => m == "LookoutPoint");
        Assert.DoesNotContain(events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void SetRegionAsLoaded_004FC8A0_is_minimap_after_005064C0()
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
        life.LoadFromFirstRealRegion();
        Assert.Equal(141, life.CurrentRegionIndex);
        var events = life.Trace.Events;
        var skipNav = events.FindIndex(e =>
            e.Va == EngineLifecycle.JobNavPassFn &&
            e.Action.Contains("skip", StringComparison.Ordinal));
        var skipCommit = events.FindIndex(e =>
            e.Va == EngineLifecycle.JobNavCommitFn &&
            e.Action.Contains("skip", StringComparison.Ordinal));
        var villages = events.FindIndex(e =>
            e.Va == EngineLifecycle.PostRegionLoadVillages &&
            e.Action.Contains("before 004FC8A0", StringComparison.Ordinal));
        var plus156 = events.FindIndex(e =>
            e.Va == EngineLifecycle.SetRegionAsLoadedFn &&
            e.Action.Contains("+156=1", StringComparison.Ordinal));
        var ui = events.FindIndex(e => e.Va == EngineLifecycle.MiniMapFromUiFn);
        var mini = events.FindIndex(e => e.Va == EngineLifecycle.InitMiniMapFn);
        var miniEnd = events.FindIndex(e =>
            e.Va == EngineLifecycle.SetRegionAsLoadedFn &&
            e.Action.Contains("MiniMap End", StringComparison.Ordinal));
        var notify = events.FindIndex(e => e.Va == EngineLifecycle.QuestRegionNotifyFn);
        var afterNotify = events.FindIndex(e =>
            e.Va == EngineLifecycle.LoadRegionFn &&
            e.Action.Contains("after 004AFC00", StringComparison.Ordinal));
        var staticMap = events.FindIndex(e =>
            e.Va == EngineLifecycle.SetStaticMapFileForUseFn);
        Assert.True(skipNav >= 0 && skipCommit > skipNav, "00500230/0050AF10 +12 skip");
        Assert.True(villages > skipCommit && plus156 > villages,
            "005064C0 before 004FC8A0");
        Assert.True(ui > plus156 && mini > ui && miniEnd > mini,
            "00437CE0 then 0082BA00");
        Assert.True(notify > miniEnd, "004AFC00 after 004FC8A0");
        Assert.True(afterNotify > notify, "00500540 dtor after 004AFC00");
        Assert.True(staticMap >= 0 && staticMap < villages,
            "00B428E0 during 004A1840, not after 004AFC00");
        Assert.Equal(0x00437CE0u, EngineLifecycle.MiniMapFromUiFn);
        Assert.Equal(0x00500230u, EngineLifecycle.JobNavPassFn);
        Assert.Equal(0x0050AF10u, EngineLifecycle.JobNavCommitFn);
        Assert.Equal(12, EngineLifecycle.LoadJobNavOffset);
        Assert.Equal(0x004AFC00u, EngineLifecycle.QuestRegionNotifyFn);
        Assert.DoesNotContain(events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void LoadWorld_004A1840_set_static_map_is_00B23DC0_then_00B428E0()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.Equal(@"Data\Levels\FinalAlbion.stb", life.StaticMapFileName);
        Assert.Equal(0, life.OpenStaticMapsMode);
        Assert.Empty(life.OpenedStaticMaps);
        var events = life.Trace.Events;
        var site = events.FindIndex(e => e.Va == EngineLifecycle.SetStaticMapForEngineSite);
        var thunk = events.FindIndex(e =>
            e.Va == EngineLifecycle.DisplayEngineSetStaticMapThunk);
        var derive = events.FindIndex(e =>
            e.Va == EngineLifecycle.DeriveStaticMapNameFn &&
            e.Action.Contains("FinalAlbion.stb", StringComparison.Ordinal));
        var vtbl = events.FindIndex(e => e.Va == EngineLifecycle.SetStaticMapVtblCallSite);
        var use = events.FindIndex(e => e.Va == EngineLifecycle.SetStaticMapFileForUseFn);
        var miss = events.FindIndex(e =>
            e.Va == EngineLifecycle.OpenStaticMapsFn &&
            e.Action.Contains("miss", StringComparison.Ordinal));
        var water = events.FindIndex(e => e.Va == EngineLifecycle.LoadWaterDataFn);
        var initChars = events.FindIndex(e =>
            e.Va == EngineLifecycle.InitCharactersFn &&
            e.Stage == "Init Characters");
        Assert.True(site >= 0 && thunk > site && derive > thunk && vtbl > derive,
            "004A1840 site then 00B23DC0 / 0049DDD0 / vtbl+208");
        Assert.True(use > vtbl && miss > use && water > miss,
            "00B428E0 miss then 00B41FA0");
        Assert.True(initChars > water, "0049F180 after Set Static Map");
        Assert.False(File.Exists(
            Path.Combine(install.Root, "Data", "Levels", "FinalAlbion.stb")));
        Assert.True(File.Exists(
            Path.Combine(install.Root, "Data", "Levels", "FinalAlbion_RT.stb")));
        Assert.DoesNotContain(events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void LoadWorld_004A1840_after_wad_is_00507C30_then_empty_006C20A0()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.NotNull(life.World);
        var events = life.Trace.Events;
        var qst = events.FindIndex(e =>
            e.Va == EngineLifecycle.DeriveQuestPathFn &&
            e.Action.Contains("FinalAlbion.qst", StringComparison.Ordinal));
        var parseQst = events.FindIndex(e =>
            e.Va == EngineLifecycle.QstParseFn &&
            e.Action.Contains("FinalAlbion.qst", StringComparison.Ordinal));
        var global = events.FindIndex(e =>
            e.Va == EngineLifecycle.QstParseFn &&
            e.Action.Contains("GlobalQuests.qst", StringComparison.Ordinal));
        var bank = events.FindIndex(e => e.Va == EngineLifecycle.WorldMapOpenBankFn);
        var wad = events.FindIndex(e => e.Va == EngineLifecycle.StartupWadSite);
        var extra = events.FindIndex(e =>
            e.Va == EngineLifecycle.ExtraWadFlagVa &&
            e.Action.Contains("skip", StringComparison.Ordinal));
        var worldVtbl = events.FindIndex(e => e.Va == EngineLifecycle.WorldLoadWldFn);
        var wld = events.FindIndex(e =>
            e.Va == EngineLifecycle.LoadWldFile &&
            e.Action.StartsWith("maps=", StringComparison.Ordinal));
        var loaded = events.FindIndex(e => e.Va == EngineLifecycle.WorldAfterWldFn);
        var empty = events.FindIndex(e =>
            e.Va == EngineLifecycle.LevelLoaderHasWork &&
            e.Action.Contains("empty", StringComparison.Ordinal));
        var offline = events.FindIndex(e =>
            e.Va == EngineLifecycle.GenerateOfflineDataSite &&
            e.Action.Contains("skip", StringComparison.Ordinal));
        var staticMap = events.FindIndex(e =>
            e.Va == EngineLifecycle.SetStaticMapForEngineSite);
        Assert.True(qst >= 0 && parseQst > qst && global > parseQst,
            "0049D770 FinalAlbion.qst then GlobalQuests.qst");
        Assert.True(bank > global && wad > bank && extra > wad,
            "004FDAB0 then Startup WAD then 01375456 skip");
        Assert.True(worldVtbl > extra && wld > worldVtbl && loaded > wld,
            "0049E220 then 00507C30 then +128=1");
        Assert.True(empty > loaded && offline > empty && staticMap > offline,
            "006C20A0 empty then Generate Offline skip then Set Static Map");
        Assert.Equal(@"Data\Levels\FinalAlbion.qst",
            EngineLifecycle.DeriveQuestFileName(EngineLifecycle.FinalAlbionWld));
        Assert.Equal(0x0049E220u, EngineLifecycle.WorldLoadWldFn);
        Assert.Equal(8, EngineLifecycle.WorldLoadWldVtbl);
        Assert.Equal(0x004FDAB0u, EngineLifecycle.WorldMapOpenBankFn);
        Assert.Equal(0x01375446u, EngineLifecycle.GenerateOfflineDataFlagVa);
        Assert.DoesNotContain(events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(events, e =>
            e.Va == EngineLifecycle.LoadRegionFn &&
            e.Action.Contains("00500540", StringComparison.Ordinal));
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
        Assert.False(life.EngineWindowCreated);
    }

    [Fact]
    public void First_pump_009A57B0_is_GetForegroundWindow_eq_hwnd()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.True(life.EngineWindowCreated);
        Assert.True(life.EngineForeground);
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.True(life.Pump());
        Assert.True(life.EngineUpdateAllowed);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.GetForegroundWindowIat &&
            e.Action.Contains("GetForegroundWindow", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.EngineUpdateGateFn &&
            e.Action.Contains("allow", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.GetForegroundWindowIat &&
            e.Action.Contains("GetTickCount", StringComparison.Ordinal));
        Assert.Equal(1, life.GameUpdateCount);
        life.EngineForeground = false;
        life.PumpGameUpdate();
        Assert.False(life.EngineUpdateAllowed);
        Assert.Equal(1, life.GameUpdateCount);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.EngineUpdateGateFn &&
            e.Action.Contains("skip", StringComparison.Ordinal));
        Assert.Equal(0x01440388u, EngineLifecycle.CreateWindowExIat);
    }

    [Fact]
    public void First_pump_00417001_copies_display_plus104_when_WorldFrame_le_1()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.RequestNewGame();
        Assert.True(life.Pump());
        Assert.True(life.Pump());
        Assert.Equal(0, life.WorldFrame);
        Assert.Equal(0, life.FrameListCount);
        Assert.Equal(0, life.DisplayPlus104);
        Assert.Equal(0, life.DisplayPlus104Copy);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrameListCountVa &&
            e.Action.Contains("[0x13B89A8]=0", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.UpdateDtVa);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.RenderDtVa);
        var skip = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.GameRenderFn &&
            e.Action.Contains("WorldFrame<=1", StringComparison.Ordinal));
        var copy = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.DisplayPlus104CopyVa);
        Assert.True(skip >= 0 && copy > skip,
            "00417265 after WorldFrame<=1 skip");
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayPlus104CopyVa &&
            e.Action.Contains("[display+104]=0", StringComparison.Ordinal));
        Assert.False(life.RenderBodyRan);
        Assert.Equal(0x013B89A8u, EngineLifecycle.FrameListCountVa);
        Assert.Equal(0x013B8690u, EngineLifecycle.UpdateDtVa);
        Assert.Equal(0x013B8698u, EngineLifecycle.RenderDtVa);
        Assert.Equal(0x013B7D6Cu, EngineLifecycle.DisplayPlus104CopyVa);
        Assert.Equal(104, EngineLifecycle.DisplayPlus104Offset);
        Assert.Equal(0, EngineLifecycle.DisplayPlus104FirstSeen);
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
        Assert.False(life.WorldUpdateRan);
        Assert.False(life.PlayerCatchupHit);
        life.DisplayTime = 1.0;
        life.UpdateGameMode();
        Assert.True(life.PlayerCatchupHit);
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
        Assert.Equal(0, life.WorldFrame);
        Assert.False(life.RenderBodyRan);
        life.WorldFrame = 2;
        life.RenderGameMode();
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
        Assert.Empty(life.GameTickTypes);
        Assert.True(life.PlayerActionReady);
        Assert.False(life.PlayerCatchupHit);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.WorldFrameIncSite);
        Assert.Equal(0, life.WorldFrame);
        Assert.False(life.RenderBodyRan);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.AdvanceGameTicksFn &&
            e.Stage == "GamePump");
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PlayerCatchupFn &&
            e.Action.Contains("<= 1", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.InnerLoopDtFn);
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
        Assert.Equal(0, life.WorldFrame);
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
        cam.ComputePose();
        cam.ApplyFollowSpring();
        cam.ApplyCameraTick();
        Assert.True(cam.PoseComputed);
        Assert.True(cam.FollowSpringRan);
        Assert.True(cam.CameraTickSkipped);
        Assert.Equal(-1.0, cam.CameraTickTimer);
        Assert.Equal(0x006B3B80u, WorldCamera.PoseTickFn);
        Assert.Equal(0.2f, cam.SlotA.Weight0);
        Assert.Equal(new System.Numerics.Vector3(1f, 0f, 0f), cam.SlotA.V0);
        Assert.Equal(new System.Numerics.Vector3(1f, 0f, 0f), cam.SlotA.V2);
        Assert.Equal(new System.Numerics.Vector3(1f, 0f, 0f), cam.SlotA.V3);
        Assert.Equal(new System.Numerics.Vector3(-1f, 0f, 0f), cam.SlotA.V4);
        Assert.Equal(0x006B2CA0u, WorldCamera.PoseFn);
        Assert.Equal(0x00A14440u, WorldCamera.NormalizeFn);
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
        Assert.Equal(0x00416202u, EngineLifecycle.FrameDtRingFn);
        Assert.Equal(0x00415E85u, EngineLifecycle.GamePumpMemlog);
        Assert.Equal(0x009AC9E0u, EngineLifecycle.PlayerManagerIdleFn);
        Assert.Equal(0x009A57B0u, EngineLifecycle.EngineUpdateGateFn);
        Assert.Equal(148, EngineLifecycle.EngineTickOffset);
        Assert.Equal(148, EngineLifecycle.EngineHwndOffset);
        Assert.Equal(0x01440378u, EngineLifecycle.GetForegroundWindowIat);
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
        Assert.Equal(0x0041674Au, EngineLifecycle.PlayerCatchupFn);
        Assert.Equal(0x004166E2u, EngineLifecycle.PlayerCatchupTimeFn);
        Assert.Equal(9, EngineLifecycle.GamePlus9Offset);
        Assert.Equal(0x013B8688u, EngineLifecycle.PlayerCatchupForceVa);
        Assert.Equal(0, EngineLifecycle.PlayerCatchupForceFirstSeen);
        Assert.Equal(0x009F16F0u, EngineLifecycle.TickListAppendFn);
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
        Assert.Equal(0x00B23DC0u, EngineLifecycle.DisplayEngineSetStaticMapThunk);
        Assert.Equal(208, EngineLifecycle.DisplayEngineSetStaticMapVtbl);
        Assert.Equal(0x0049DDD0u, EngineLifecycle.DeriveStaticMapNameFn);
        Assert.Equal(0x004A1BD3u, EngineLifecycle.SetStaticMapVtblCallSite);
        Assert.Equal(".stb", EngineLifecycle.StaticMapStbSuffix);
        Assert.Equal("_RT.stb", EngineLifecycle.StaticMapRtStbSuffix);
        Assert.Equal(@"Data\Levels\FinalAlbion.stb",
            EngineLifecycle.DeriveStaticMapFileName(EngineLifecycle.FinalAlbionWld));
        Assert.Equal(@"Data\Levels\FinalAlbion_RT.stb",
            EngineLifecycle.DeriveStaticMapFileName(EngineLifecycle.FinalAlbionWld, true));
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
        Assert.Equal(0xE5, EngineLifecycle.FrontendPressStartMessage);
        Assert.Equal(0x124, EngineLifecycle.FrontendMainMenuMessage);
        Assert.Equal(0x00599D5Cu, EngineLifecycle.FrontendPressStartAcceptFn);
        Assert.Equal(0x00595845u, EngineLifecycle.FrontendNoProfileFn);
        Assert.Equal(0x00596917u, EngineLifecycle.FrontendNewProfileBindFn);
        Assert.Equal(0x00596763u, EngineLifecycle.FrontendMenuSwitchFn);
        Assert.Equal(0x00851700u, EngineLifecycle.FrontendUi96CtorFn);
        Assert.Equal(0x17, EngineLifecycle.FrontendNewProfileSlot);
        Assert.Equal(
            "UI_FRONTEND_NEW_PROFILE_SCREEN",
            EngineLifecycle.FrontendNewProfileMenu);
        Assert.Equal(0x126, EngineLifecycle.FrontendAcceptProfileMessage);
        Assert.Equal(0x00851920u, EngineLifecycle.FrontendCommitNameFn);
        Assert.Equal(0x0059697Au, EngineLifecycle.FrontendCommitProfileFn);
        Assert.Equal(0x004069E0u, EngineLifecycle.FrontendProfileDefaultFn);
        Assert.Equal("Default", EngineLifecycle.FrontendProfileDefaultFallback);
        Assert.Equal(37, EngineLifecycle.FrontendNewProfileEditType);
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
        life.EnqueueAfterDummy();
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

        life.LoadFromFirstRealRegion();
        Assert.True(life.FirstRealRegionLoadDone);
        Assert.True(life.Pump());
        Assert.Equal(2, life.GameUpdateCount);
        Assert.Equal(2, life.GameRenderCount);
        Assert.Equal(0, life.WorldFrame);
        Assert.Empty(life.GameTickTypes);
        Assert.Equal(141, life.CurrentRegionIndex);
        Assert.NotNull(life.CurrentRegion);
        Assert.Equal("Filler_NorthernWastes_02", life.CurrentRegion.RegionName);
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
        Assert.Equal(0, life.OpenStaticMapsMode);
        Assert.Empty(life.OpenedStaticMaps);
        Assert.Equal(@"Data\Levels\FinalAlbion.stb", life.StaticMapFileName);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.OpenStaticMapsFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.SetStaticMapFileForUseFn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayEngineSetStaticMapThunk);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.OpenStaticMapsFn &&
            e.Action.Contains("miss", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.LoadWaterDataFn);
        Assert.Empty(life.OpenedMapBodies);
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
        Assert.Null(life.CurrentStaticMapName);
        Assert.Empty(life.NeighbourStaticMaps);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.CloseStaticMapFileFn);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.OpenStaticMapsNameTable);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.AttachPatchFn);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == EngineLifecycle.OpenStaticMapsAttach);
        Assert.Contains("LookoutPoint", life.ActivatedMaps);
        Assert.Contains("PicnicArea", life.ActivatedMaps);
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
        Assert.True(WorldCamera.IsCtorAxis(life.WorldCamera.SlotA.V0));
        Assert.True(life.RendererHelperBound);
        Assert.True((life.Camera.Position - RegionTravel.PositionOf(life.Hero!)).Length() < 1f);
        Assert.Equal(LandscapeFrustum.FirstSeenCameraUp, life.Camera.Up);
        Assert.Equal(GameCamera.FirstSeenFovDegrees, life.Camera.FovDegrees);
        Assert.Same(life.SubmittedWorld, life.BuildFrame().World);
        Assert.True(life.SubmitElapsedMs > 0);
        Assert.True(life.Levels!.HasCachedCells("LookoutPoint"));
        Assert.Contains(life.SubmittedMesh!.Draws, d => d.PassBit == 0x2000);
        Assert.Equal(GameBin.FirstSeenEnvironmentThemeId, life.AuthoredEnvironmentThemeId);
        Assert.Equal(GameBin.FirstSeenEnvironmentThemeName, life.AuthoredEnvironmentTheme);
        Assert.NotEqual(GameBin.OakvaleEnvironmentName, life.AuthoredEnvironmentTheme);
        Assert.Equal(0, WorldShading.FirstSeenPackedLightCount);
        Assert.Equal(7, life.SubmittedWorld!.Instances.Count(i =>
            i.MeshId == 4978 &&
            i.Map.Equals("LookoutPoint", StringComparison.OrdinalIgnoreCase)));
        var heroFwd = Vector3.Normalize(
            Vector3.TransformNormal(Vector3.UnitY, WorldGeometry.ObjectTransform(life.Hero!)));
        Assert.True((heroFwd - Vector3.UnitX).Length() < 0.05f,
            $"hero forward={heroFwd} expected +X from GuildArrivalHSP");
        Assert.NotNull(life.Runtime);
        Assert.Same(life.Hero, life.Runtime.Bindings.Resolve("HERO")?.Thing);
        Assert.Same(life.Hero, life.Runtime.Bindings.Resolve("Hero")?.Thing);
        Assert.True((life.Runtime.World.Positions["HERO"] -
                     RegionTravel.PositionOf(life.Hero!)).Length() < 0.05f);
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
        life.LoadFromFirstRealRegion();
        Assert.True(life.PlayerActionReady);
        Assert.Equal(0, life.WorldFrame);
        life.WorldFrame = 2;
        Assert.False(life.RegionObjectPresent);
        life.CameraCatchupTicks = 0;
        life.RenderGameMode();
        Assert.True(life.CameraInterpolationRan);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.CameraInterpolationFn);
        life.CameraCatchupTicks = 15;
        life.GamePlus72 = 1;
        life.RenderGameMode();
        Assert.True(life.CameraBodySteps >= 1);
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
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.WorldCameraPoseFn);
        Assert.True(life.WorldCamera.Seeded);
        Assert.True(life.WorldCamera.PoseComputed);
        Assert.True(life.FollowSpringRan);
        Assert.True(life.WorldCamera.CameraTickSkipped);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == WorldCamera.PoseTickFn &&
            e.Action.Contains("skip", StringComparison.Ordinal));
        Assert.Equal(new System.Numerics.Vector3(1f, 0f, 0f), life.WorldCamera.SlotA.V2);
        Assert.Equal(new System.Numerics.Vector3(-1f, 0f, 0f), life.WorldCamera.SlotA.V4);
        Assert.Equal(new System.Numerics.Vector3(1f, 0f, 0f), life.WorldCamera.SlotA.V0);
        Assert.Equal(0.2f, life.WorldCamera.SlotA.Weight0);
        Assert.NotEqual(
            System.Numerics.Vector3.UnitZ * 1.6f,
            life.Camera.Position - (life.Hero is { } hero
                ? RegionTravel.PositionOf(hero)
                : System.Numerics.Vector3.Zero));
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
            ScriptedCamera.ApplyManagerOutput(+6296/+6312/+6328)
            is slot state. 00B314E0 consumes the helper
            (hero eye, 006B2CA0 V4, FirstSeenCameraUp)
            when +6296 is still the ctor axis.
            Not 00DBDE40.
            """);
    }

    [Fact]
    public void World_submit_is_stable_between_frames()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.Pump());
        life.LoadFromFirstRealRegion();
        Assert.True(life.Pump());
        Assert.True(life.WorldSubmitted);
        var mesh = life.SubmittedMesh;
        var parsed = life.Meshes.ParsedCount;
        var decoded = life.Textures?.DecodedCount ?? 0;
        var elapsed = life.SubmitElapsedMs;
        Assert.True(life.Pump());
        Assert.Same(mesh, life.SubmittedMesh);
        Assert.Equal(parsed, life.Meshes.ParsedCount);
        Assert.Equal(decoded, life.Textures?.DecodedCount ?? 0);
        Assert.Equal(elapsed, life.SubmitElapsedMs);
        Assert.True(life.SubmittedLandscapeCells > 0);
        Assert.Same(life.BuildFrame().Vertices, life.SubmittedLandscape!.Vertices);
        Assert.Same(life.BuildFrame().ObjectVertices, life.SubmittedObjects!.Vertices);
        Assert.Same(life.BuildFrame().Textures, life.BuildFrame().Textures);
        Assert.NotNull(life.LastLoadTiming);
    }

    [Fact]
    public void Native_draw_order_is_begin_layers_end_present()
    {
        Assert.Equal(0x00435530u, EngineLifecycle.DisplayApplyBodyFn);
        Assert.Equal(0x009DA9F0u, EngineLifecycle.DisplayFlushLayersFn);
        Assert.Equal(0x00B25950u, RegionTravel.RenderFrame);
        Assert.Equal(
            new uint[] { 0x4, 0x40, 0x20, 0x100, 0x2000, 0x80, 0x200 },
            ScenePasses.Registration.Where(p => ScenePasses.Draws(p.Submit))
                .Select(p => p.Bit)
                .ToArray());
    }
}
