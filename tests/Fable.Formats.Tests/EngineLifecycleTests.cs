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
        Assert.Equal(0x0040138Cu, EngineLifecycle.CrtSehFn);
        Assert.Equal(0x004012CEu, EngineLifecycle.CrtStaticCtorsFn);
        Assert.Equal(0x004011E7u, EngineLifecycle.WinMainCallSite);
        Assert.Equal(0x00BFEA30u, EngineLifecycle.WinMainAllocaFn);
        Assert.Equal(0x009D86B0u, EngineLifecycle.WinMainZeroFn);
        Assert.Equal(0x0143FE24u, EngineLifecycle.OpenMutexIat);
        Assert.Equal(0x0143FE28u, EngineLifecycle.CreateMutexIat);
        Assert.Equal(@"Global\Fable", EngineLifecycle.MutexName);
        Assert.Equal(0x1F0001, EngineLifecycle.MutexAccess);
        Assert.Equal(0x32008, EngineLifecycle.WinMainAllocaSize);
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
    public void Assembly_dump_pe_entry_is_crt_then_winmain()
    {
        var dump = AssemblyTextMap.TryLocate();
        Assert.NotNull(dump);

        Assert.Equal("push 116", dump.Text(EngineLifecycle.PeEntry));
        Assert.Equal("call 0040138C", dump.Text(0x0040106E));
        Assert.Equal(EngineLifecycle.CrtSehFn, dump.E8Dest(0x0040106E));
        Assert.Equal(EngineLifecycle.CrtStaticCtorsFn, dump.E8Dest(0x00401115));
        Assert.Equal(EngineLifecycle.WinMain, dump.E8Dest(EngineLifecycle.WinMainCallSite));
        Assert.Equal("call 00403480", dump.Text(EngineLifecycle.WinMainCallSite));

        Assert.Equal("mov eax, 0x32008", dump.Text(EngineLifecycle.WinMain));
        Assert.Equal("call 00BFEA30", dump.Text(0x00403485));
        Assert.Equal("call [0x143FE24]", dump.Text(0x004034A2));
        Assert.Equal("call [0x143FE28]", dump.Text(0x004034B3));
        Assert.Equal("call 009D86B0", dump.Text(0x004034D6));
        Assert.Equal("call 00402510", dump.Text(0x004034F1));
        Assert.Equal("KERNEL32.dll!OpenMutexW", dump.IatName(EngineLifecycle.OpenMutexIat));
        Assert.Equal("KERNEL32.dll!CreateMutexW", dump.IatName(EngineLifecycle.CreateMutexIat));
        Assert.Equal("KERNEL32.dll!GetModuleHandleA", dump.IatName(EngineLifecycle.GetModuleHandleIat));
        Assert.Equal("MSVCR71.dll!__set_app_type", dump.IatName(EngineLifecycle.SetAppTypeIat));
        Assert.Equal(EngineLifecycle.MutexName, dump.Utf16FromVtbl(EngineLifecycle.MutexNameVa));

        Assert.Equal("sub esp, 0x168", dump.Text(EngineLifecycle.BootstrapFn));
        foreach (var (name, va) in EngineLifecycle.NamedBootstrapStages)
            Assert.Equal($"push \"{name}\"", dump.Text(va));

        Assert.Equal("push ebx", dump.Text(EngineLifecycle.ParseCommandLineCtor));
        Assert.Equal(EngineLifecycle.ParseCommandLineCtor, dump.E8Dest(0x00402553));
        Assert.Equal(EngineLifecycle.ParseCommandLineScan, dump.E8Dest(0x00402583));
        Assert.Equal(EngineLifecycle.ParseCommandLineApply, dump.E8Dest(0x0040258A));
        Assert.Equal(EngineLifecycle.UserstRegisterFn, dump.E8Dest(0x004025A1));
        Assert.Equal(EngineLifecycle.SetupInstallCopyFn, dump.E8Dest(0x0040262A));
        Assert.Equal(EngineLifecycle.SetupLanguageFn, dump.E8Dest(0x0040269F));
        Assert.Equal("push \"English\"", dump.Text(0x00415533));
        Assert.Equal(EngineLifecycle.LanguagePrefix, dump.Utf16FromVtbl(EngineLifecycle.LanguagePrefixVa));
        Assert.Equal(EngineLifecycle.LanguageSettingsLeaf, dump.Utf16FromVtbl(EngineLifecycle.LanguageSettingsVa));
        Assert.Equal("push \"LeftAlignText\"", dump.Text(0x00402774));
        Assert.Equal("push \"NoHangulWordWrap\"", dump.Text(0x00402794));
        Assert.Equal("push \"DisableCapsLock\"", dump.Text(0x004027B4));
        Assert.Equal(EngineLifecycle.LanguageIniFn, dump.E8Dest(0x00402785));
        Assert.Equal(EngineLifecycle.LanguageIniFn, dump.E8Dest(0x004027A5));
        Assert.Equal(EngineLifecycle.LanguageIniFn, dump.E8Dest(0x004027C5));
        Assert.Equal(EngineLifecycle.ApplyLeftAlignFn, dump.E8Dest(0x0040282C));
        Assert.Equal(EngineLifecycle.ApplyNoHangulFn, dump.E8Dest(0x00402837));
        Assert.Equal(EngineLifecycle.BankManagerInitFn, dump.E8Dest(0x00402875));
        Assert.Equal(EngineLifecycle.RegisterRetailBank, dump.E8Dest(0x004028A9));
        Assert.Equal(EngineLifecycle.EngineSingletonGetter, dump.E8Dest(0x00403325));
        Assert.Equal(EngineLifecycle.SetupLibrary, dump.E8Dest(0x0040332C));
        Assert.Equal(EngineLifecycle.FrontendDisplayHelperFn, dump.E8Dest(0x00403346));
        Assert.Equal(EngineLifecycle.LibraryPostWindowFn, dump.E8Dest(0x0040334D));
        Assert.Equal(EngineLifecycle.ProbeGraphics, dump.E8Dest(0x00403389));
        Assert.Equal(EngineLifecycle.RunModes, dump.E8Dest(0x004033C2));
    }

    [Fact]
    public void Bootstrap_first_seen_matches_assembly_pe_entry()
    {
        var dump = AssemblyTextMap.TryLocate();
        Assert.NotNull(dump);
        Assert.Equal("call 00403480", dump.Text(EngineLifecycle.WinMainCallSite));

        var life = new EngineLifecycle();
        life.Bootstrap(null);
        var events = life.Trace.Events;
        var last = -1;
        foreach (var va in EngineLifecycle.PeEntryFirstSeenVas)
        {
            var i = events.FindIndex(last + 1, e => e.Va == va);
            Assert.True(i > last, $"missing or out of order 0x{va:X8} after {last}");
            last = i;
        }

        Assert.DoesNotContain(events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.Equal(EngineStage.StartupVideos, life.Stage);
        Assert.Equal(EngineMode.RetailFrontend, life.Mode);
        Assert.False(life.LanguageSettingsLoaded);
        Assert.Equal((byte)0, life.LeftAlignText);
        Assert.Equal((byte)0, life.NoHangulWordWrap);
        Assert.Equal((byte)0, life.DisableCapsLock);
        Assert.DoesNotContain(events, e =>
            e.Va == EngineLifecycle.LanguageIniFn && e.Stage == "Setup Language");
    }

    [Fact]
    public void Setup_language_004045C0_reads_lang_settings_txt()
    {
        var dump = AssemblyTextMap.TryLocate();
        Assert.NotNull(dump);
        Assert.Equal(EngineLifecycle.LanguageIniFn, dump.E8Dest(0x00402785));
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var path = Path.Combine(
            install.DataRoot, "lang", EngineLifecycle.LanguageFolder,
            EngineLifecycle.LanguageSettingsName);
        Assert.True(File.Exists(path), path);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        Assert.True(life.LanguageSettingsLoaded);
        Assert.Equal((byte)0, life.LeftAlignText);
        Assert.Equal((byte)0, life.NoHangulWordWrap);
        Assert.Equal((byte)0, life.DisableCapsLock);
        var events = life.Trace.Events;
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.LanguageIniFn &&
            e.Action.Contains("LeftAlignText", StringComparison.Ordinal));
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.LanguageIniFn &&
            e.Action.Contains("NoHangulWordWrap", StringComparison.Ordinal));
        Assert.Contains(events, e =>
            e.Va == EngineLifecycle.LanguageIniFn &&
            e.Action.Contains("DisableCapsLock", StringComparison.Ordinal));
        Assert.Contains(events, e => e.Va == EngineLifecycle.ApplyLeftAlignFn);
        Assert.Contains(events, e => e.Va == EngineLifecycle.ApplyNoHangulFn);
        var bind = events.FindIndex(e =>
            e.Va == EngineLifecycle.LanguageIniFn && e.Stage == "Setup Language");
        var apply = events.FindIndex(e => e.Va == EngineLifecycle.ApplyLeftAlignFn);
        Assert.True(bind >= 0 && apply > bind, "009BC890 after 004045C0");
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
        Assert.Equal(0x013B8394u, EngineLifecycle.RetailAudioEngineVa);
        Assert.Equal(68, EngineLifecycle.RetailAudioStartVtbl);
        Assert.Equal(72, EngineLifecycle.RetailAudioFadeVtbl);
        Assert.Equal(0x00417A58u, EngineLifecycle.InitSoundFn);
        Assert.False(EngineLifecycle.RequestNewGameStartsMusicSet);
        Assert.False(EngineLifecycle.InitSoundPlaysMusicSet);
        Assert.False(EngineLifecycle.InitSoundOpensBank);
        Assert.False(EngineLifecycle.FrontendClickStartsSnd);
        Assert.Equal(0x009919C0u, EngineLifecycle.InitSoundRegisterFn);
        Assert.Equal(0x00991C10u, EngineLifecycle.InitSoundAtmosRegisterFn);
        Assert.Equal(0x00991840u, EngineLifecycle.InitSoundMapLookupFn);
        Assert.Equal(0x00A38C20u, EngineLifecycle.InitSoundSymbolsCompiledFn);
        Assert.Equal(0x00A01A4Fu, EngineLifecycle.InitSoundSymbolsTextFn);
        Assert.Equal("ENGLISH_SOUND_SETUP", EngineLifecycle.InitSoundLocaleName);
        Assert.Equal("MAIN_SOUND_SETUP", EngineLifecycle.InitSoundMainName);
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
        Assert.False(EngineLifecycle.FirstSeenFrontendE8Enqueue);
        Assert.False(EngineLifecycle.FiberCallsPersistThenRun);
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
        Assert.Equal(0x00430340u, EngineLifecycle.RetailPlus8StoreFn);
        Assert.Equal(8, EngineLifecycle.RetailPlus8Offset);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.RetailPlus8StoreFn &&
            e.Action.Contains("[retail+8]=1", StringComparison.Ordinal));
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
    public void Frontend_attach_0xE5_is_slot_0x14_0059B5D7_not_type10_walk()
    {
        Assert.Equal(0x0059B5D7u, EngineLifecycle.FrontendSlotLookupFn);
        Assert.Equal(FrontendInputMap.SlotLookupFn, EngineLifecycle.FrontendSlotLookupFn);
        Assert.Equal(20, EngineLifecycle.FrontendWidgetSlotOffset);
        Assert.Equal(84, EngineLifecycle.FrontendWidgetListOffset);
        Assert.Equal(0x14, EngineLifecycle.FrontendPressStartSlot);
        Assert.Equal(0x0054E4F0u, FrontendInputMap.Type10StoreMsgFn);

        var empty = new EngineLifecycle();
        empty.Bootstrap(null);
        while (empty.Stage == EngineStage.StartupVideos)
            empty.FinishStartupVideo();
        Assert.False(empty.TryGetFrontendSlot(
            EngineLifecycle.FrontendPressStartSlot, out _));
        Assert.Contains(empty.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendSlotLookupFn &&
            e.Action.Contains("0059B5D7", StringComparison.Ordinal));
        Assert.Contains(empty.Trace.Events, e =>
            e.Va == FrontendInputMap.AttachWriteE5 &&
            e.Action.Contains("slot 0x14", StringComparison.Ordinal));
        Assert.DoesNotContain(empty.FrontendWidgets, w =>
            w.Type10Packet == EngineLifecycle.FrontendPressStartMessage);

        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.True(life.TryGetFrontendSlot(
            EngineLifecycle.FrontendPressStartSlot, out var slot));
        Assert.Equal(EngineLifecycle.FrontendPressStartMenu, slot.Name);
        Assert.Equal(10, slot.Type);
        Assert.Equal(0, slot.ActionOnLeftUnclicked);
        Assert.Equal(EngineLifecycle.FrontendPressStartMessage, slot.Type10Packet);
        Assert.Equal(5, slot.State);
        Assert.Equal(0x005952C3u, EngineLifecycle.FrontendInitSelectFn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendInitSelectFn &&
            e.Action.Contains("vtbl+192(5)", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendSlotLookupFn &&
            e.Action.Contains("slot 0x14", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == FrontendInputMap.AttachWriteE5 &&
            e.Action.Contains("54E4F0", StringComparison.Ordinal));
        life.DispatchFrontendMessage(EngineLifecycle.FrontendPressStartMessage);
        Assert.True(life.Pump());
        Assert.Equal(
            EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        Assert.DoesNotContain(life.FrontendWidgets, w =>
            w.Type == 10 &&
            w.ActionOnLeftUnclicked == EngineLifecycle.FrontendPressStartMessage);
        Assert.True(life.TryGetFrontendSlot(
            EngineLifecycle.FrontendPressStartSlot, out var kept));
        Assert.Equal(EngineLifecycle.FrontendPressStartMenu, kept.Name);
        Assert.Equal(0, kept.ActionOnLeftUnclicked);
        Assert.Equal(EngineLifecycle.FrontendPressStartMessage, kept.Type10Packet);
        Assert.True(life.TryGetFrontendSlot(
            EngineLifecycle.FrontendNewProfileSlot, out var profile));
        Assert.Equal(EngineLifecycle.FrontendNewProfileMenu, profile.Name);
        Assert.Equal(10, profile.Type);
        Assert.Equal(0, profile.ActionOnLeftUnclicked);
        Assert.Equal(6, kept.State);
        Assert.Equal(5, profile.State);
        Assert.Equal(0x0052CF40u, FrontendWidgetType.SelectStateFn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == FrontendWidgetType.SelectStateFn &&
            e.Action.Contains("+332=6", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == FrontendWidgetType.SelectStateFn &&
            e.Action.Contains("+332=5", StringComparison.Ordinal));
        Assert.Equal(
            new[]
            {
                EngineLifecycle.FrontendPressStartSlot,
                EngineLifecycle.FrontendNewProfileSlot,
            },
            life.FrontendResidentSlots);
    }

    [Fact]
    public void Frontend_tick_and_draw_walk_resident_ui84_slots()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.True(life.Pump());
        Assert.Equal(
            new[] { EngineLifecycle.FrontendPressStartSlot },
            life.FrontendResidentSlots);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendUiDrawFn &&
            e.Action.Contains("[ui+84]", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendUiTickFn &&
            e.Action.Contains("[ui+84]", StringComparison.Ordinal));
        life.DispatchFrontendMessage(EngineLifecycle.FrontendPressStartMessage);
        Assert.True(life.Pump());
        Assert.Equal(
            new[]
            {
                EngineLifecycle.FrontendPressStartSlot,
                EngineLifecycle.FrontendNewProfileSlot,
            },
            life.FrontendResidentSlots);
        Assert.Equal(
            EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        Assert.DoesNotContain(life.FrontendWidgets, w =>
            w.Name == EngineLifecycle.FrontendPressStartMenu);
    }

    [Fact]
    public void Frontend_ui84_keeps_slot_0x14_and_0x17_after_main_menu()
    {
        Assert.Equal(0, EngineLifecycle.FrontendMainMenuSlot);
        Assert.Equal(0, FrontendMessages.MainMenuSlot);
        Assert.Equal(0x14, EngineLifecycle.FrontendPressStartSlot);
        Assert.Equal(0x17, EngineLifecycle.FrontendNewProfileSlot);
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.DispatchFrontendMessage(EngineLifecycle.FrontendPressStartMessage);
        Assert.True(life.Pump());
        life.DispatchFrontendMessage(EngineLifecycle.FrontendAcceptProfileMessage);
        Assert.True(life.Pump());
        Assert.Equal(
            EngineLifecycle.FrontendMainMenuNoContinue, life.FrontendMenuRoot);
        Assert.True(life.TryGetFrontendSlot(
            EngineLifecycle.FrontendPressStartSlot, out var press));
        Assert.Equal(0, press.ActionOnLeftUnclicked);
        Assert.Equal(EngineLifecycle.FrontendPressStartMessage, press.Type10Packet);
        Assert.True(life.TryGetFrontendSlot(
            EngineLifecycle.FrontendNewProfileSlot, out var profile));
        Assert.Equal(EngineLifecycle.FrontendNewProfileMenu, profile.Name);
        Assert.True(life.TryGetFrontendSlot(
            EngineLifecycle.FrontendMainMenuSlot, out var menu));
        Assert.Equal(EngineLifecycle.FrontendMainMenuNoContinue, menu.Name);
        Assert.Equal(10, menu.Type);
        Assert.Equal(0, menu.ActionOnLeftUnclicked);
        Assert.Equal(
            new[]
            {
                EngineLifecycle.FrontendMainMenuSlot,
                EngineLifecycle.FrontendPressStartSlot,
                EngineLifecycle.FrontendNewProfileSlot,
            },
            life.FrontendResidentSlots);
        Assert.DoesNotContain(life.FrontendWidgets, w =>
            w.Type == 10 &&
            w.ActionOnLeftUnclicked == EngineLifecycle.FrontendPressStartMessage);
    }

    [Fact]
    public void Init_Fonts_004168DC_stores_ENG_ARIAL_18_at_game_plus90444()
    {
        Assert.Equal(0x004168DCu, EngineLifecycle.InitFontsFn);
        Assert.Equal(0x009E2C80u, EngineLifecycle.GameFontLookupFn);
        Assert.Equal(0x00419463u, EngineLifecycle.GameFontStoreFn);
        Assert.Equal(90444, EngineLifecycle.GameFontOffset);
        Assert.Equal("ENG_ARIAL_18", EngineLifecycle.GameFontFaceName);
        Assert.NotEqual(FontFile.UiFace, EngineLifecycle.GameFontFaceName);
        Assert.NotEqual(FontFile.PersistType6Face, EngineLifecycle.GameFontFaceName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.Null(life.GameFontFace);
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.Equal(FontFile.GameFace, life.GameFontFace);
        var graphics = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.InitGraphicsFn);
        var fonts = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.InitFontsFn);
        var subtitled = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Subtitled Message");
        Assert.True(graphics >= 0 && fonts > graphics);
        Assert.True(subtitled > fonts);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.GameFontLookupFn &&
            e.Action.Contains("ENG_ARIAL_18", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Subtitled_004CDB10_registers_00A39010_at_13B8A54()
    {
        Assert.Equal(0x004CDB10u, EngineLifecycle.InitSubtitledMessageFn);
        Assert.Equal(0x0041A080u, EngineLifecycle.SubtitledPrefixFn);
        Assert.Equal(0x0099BF30u, EngineLifecycle.SubtitledLeafFn);
        Assert.Equal(0x00A39010u, EngineLifecycle.SubtitledRegisterFn);
        Assert.Equal(0x013B8A54u, EngineLifecycle.SubtitledSingletonVa);
        Assert.Equal(0x0122F3D0u, EngineLifecycle.SubtitledPrefixVa);
        Assert.Equal(0x01239E74u, EngineLifecycle.SubtitledLeafVa);
        Assert.Equal(@"Data\Defs\", EngineLifecycle.SubtitledDefsPrefix);
        Assert.Equal("misc_def_types.h", EngineLifecycle.SubtitledDefsLeaf);
        Assert.Equal(@"Data\Defs\misc_def_types.h", EngineLifecycle.SubtitledDefsPath);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.SubtitledSymbolsRegistered);
        Assert.Null(life.SubtitledSymbolPath);
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.True(life.SubtitledSymbolsRegistered);
        Assert.Equal(EngineLifecycle.SubtitledDefsPath, life.SubtitledSymbolPath);
        var fonts = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.InitFontsFn);
        var prefix = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SubtitledPrefixFn);
        var leaf = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SubtitledLeafFn);
        var register = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SubtitledRegisterFn);
        var attitude = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Conversation Attitude");
        Assert.True(fonts >= 0 && prefix > fonts);
        Assert.True(leaf > prefix && register > leaf);
        Assert.True(attitude > register);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Action.Contains("Speak", StringComparison.OrdinalIgnoreCase) &&
            e.Stage == "Init Subtitled Message");
    }

    [Fact]
    public void Init_Conversation_004CD670_binds_STANDARD_TALK_tables()
    {
        Assert.Equal(0x004CD670u, EngineLifecycle.InitConversationAttitudeFn);
        Assert.Equal(0x0099EFE0u, EngineLifecycle.ConversationAttitudeBindFn);
        Assert.Equal(0x013B8A28u, EngineLifecycle.ConversationAttitudeOnceVa);
        Assert.Equal(0x013B8A2Cu, EngineLifecycle.ConversationAttitudeTable0Va);
        Assert.Equal(0x013B8A38u, EngineLifecycle.ConversationAttitudeTable1Va);
        Assert.Equal(0x013B8A44u, EngineLifecycle.ConversationAttitudeTable2Va);
        Assert.Equal(18, EngineLifecycle.ConversationAttitudeNames0.Length);
        Assert.Equal(12, EngineLifecycle.ConversationAttitudeNames1.Length);
        Assert.Equal(12, EngineLifecycle.ConversationAttitudeNames2.Length);
        Assert.Equal("STANDARD_TALK_GENERIC", EngineLifecycle.ConversationAttitudeNames0[0]);
        Assert.Equal("STANDARD_TALK_GENERIC", EngineLifecycle.ConversationAttitudeNames0[17]);
        Assert.Equal("NULL", EngineLifecycle.ConversationAttitudeNames1[0]);
        Assert.Equal("STANDARD_TALK_FRIENDLY", EngineLifecycle.ConversationAttitudeNames1[8]);
        Assert.Equal("", EngineLifecycle.ConversationAttitudeNames2[0]);
        Assert.Equal("CONVERSATION_HOLDING_SWORD", EngineLifecycle.ConversationAttitudeNames2[11]);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.ConversationAttitudesBound);
        Assert.Empty(life.ConversationAttitudeTable0);
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.True(life.ConversationAttitudesBound);
        Assert.Equal(EngineLifecycle.ConversationAttitudeNames0, life.ConversationAttitudeTable0);
        Assert.Equal(EngineLifecycle.ConversationAttitudeNames1, life.ConversationAttitudeTable1);
        Assert.Equal(EngineLifecycle.ConversationAttitudeNames2, life.ConversationAttitudeTable2);
        var register = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SubtitledRegisterFn);
        var console = life.Trace.Events.FindIndex(e =>
            e.Va == 0x0041863D);
        var bind = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ConversationAttitudeBindFn);
        var once = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ConversationAttitudeOnceVa);
        var players = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Player Manager");
        Assert.True(register >= 0 && console > register);
        Assert.True(bind > console && once > bind);
        Assert.True(players > once);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Action.Contains("Speak", StringComparison.OrdinalIgnoreCase) &&
            e.Stage == "Init Conversation Attitude");
    }

    [Fact]
    public void Init_Player_Manager_0041732A_stores_44byte_owner_at_game_plus28()
    {
        Assert.Equal(0x0041732Au, EngineLifecycle.InitPlayerManagerFn);
        Assert.Equal(0x0044A3B0u, EngineLifecycle.PlayerOwnerCtor);
        Assert.Equal(0x004193A0u, EngineLifecycle.PlayerOwnerStoreFn);
        Assert.Equal(0x01231CD0u, EngineLifecycle.PlayerOwnerVtbl);
        Assert.Equal(44, EngineLifecycle.PlayerOwnerSize);
        Assert.Equal(28, EngineLifecycle.PlayerOwnerOffset);
        Assert.Equal(
            new[]
            {
                "hero_swap_1.tng",
                "hero_swap_2.tng",
                "hero_swap_3.tng",
                "hero_swap_4.tng",
            },
            EngineLifecycle.PlayerOwnerHeroSwapNames);
        Assert.NotEqual(EngineLifecycle.CreatePlayersFn, EngineLifecycle.InitPlayerManagerFn);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.PlayerOwnerPresent);
        Assert.Empty(life.PlayerOwnerHeroSwap);
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.True(life.PlayerOwnerPresent);
        Assert.Equal(EngineLifecycle.PlayerOwnerHeroSwapNames, life.PlayerOwnerHeroSwap);
        var once = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ConversationAttitudeOnceVa);
        var getter = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.PlayerManagerGetter &&
            e.Stage == "Init Player Manager");
        var ctor = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.PlayerOwnerCtor);
        var store = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.PlayerOwnerStoreFn);
        var iface = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.PlayerInterfaceCtor);
        var create = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.PlayerManagerGetter &&
            e.Stage == "Create Players");
        Assert.True(once >= 0 && getter > once);
        Assert.True(ctor > getter && store > ctor);
        Assert.True(iface > store, "0044A3B0 before 004473A0");
        Assert.True(create > iface, "0044C6B0 getter later Create Players");
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.PlayerOwnerCtor &&
            e.Stage == "InitGame" &&
            e.Action.Contains("0044A3B0", StringComparison.Ordinal) &&
            e.Action.Contains("+24=0", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_World_004A67D0_runs_inside_0041735A_before_00417418()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        var worldCtor = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.InitWorldFn &&
            e.Action.Contains("004A67D0", StringComparison.Ordinal));
        var worldInit = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.InitWorldInitFn);
        var display = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.DisplayCtorFn);
        var particles = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SkipParticlesVa &&
            e.Action.Contains("run 004174F1", StringComparison.Ordinal));
        Assert.True(worldCtor >= 0 && worldInit > worldCtor);
        Assert.True(display > worldInit, "004A6E30 before 00417418");
        Assert.True(particles > display, "00417418 before 004174F1");
        Assert.True(life.DisplayPresent);
        Assert.True(life.DisplayPlus248World);
        Assert.True(life.DisplayPlus24MeshBank);
        var store = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.DisplayStoreFn);
        Assert.True(store > display && particles > store);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Display_Engine_00417418_stores_0x100_at_game_plus40()
    {
        Assert.Equal(0x00BFEA1Au, EngineLifecycle.DisplayAllocFn);
        Assert.Equal(0x100, EngineLifecycle.DisplaySize);
        Assert.Equal(0x00419270u, EngineLifecycle.DisplayBlobZeroFn);
        Assert.Equal(36, EngineLifecycle.DisplayBlobSize);
        Assert.Equal(0x0041940Cu, EngineLifecycle.DisplayStoreFn);
        Assert.Equal(40, EngineLifecycle.GameDisplayOffset);
        Assert.Equal(1, EngineLifecycle.DisplayPlus4Ctor);
        Assert.Equal(248, EngineLifecycle.DisplayPlus248Offset);
        Assert.Equal(24, EngineLifecycle.DisplayPlus24Offset);
        Assert.NotEqual(EngineLifecycle.DisplayObjectAllocFn, EngineLifecycle.DisplayAllocFn);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.DisplayPresent);
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.True(life.DisplayPresent);
        Assert.Equal(EngineLifecycle.DisplayPlus4Ctor, life.DisplayPlus4);
        Assert.InRange(life.DisplayPlus232, 0, EngineLifecycle.DisplayPlus232Ctor);
        Assert.True(life.DisplayPlus8Game);
        Assert.True(life.DisplayPlus12Owner);
        Assert.True(life.DisplayPlus16Manager);
        Assert.True(life.DisplayPlus20GraphicBank);
        Assert.True(life.DisplayPlus24MeshBank);
        Assert.True(life.DisplayPlus248World);
        var worldInit = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.InitWorldInitFn);
        var mesh = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.InitMeshBankFn);
        var alloc = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.DisplayAllocFn);
        var ctor = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.DisplayCtorFn);
        var store = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.DisplayStoreFn);
        var particles = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SkipParticlesVa &&
            e.Action.Contains("run 004174F1", StringComparison.Ordinal));
        Assert.True(worldInit >= 0 && mesh > worldInit);
        Assert.True(alloc > mesh && ctor > alloc && store > ctor);
        Assert.True(particles > store, "0041940C before 004174F1");
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayCtorFn &&
            e.Action.Contains("+248", StringComparison.Ordinal) &&
            e.Action.Contains("+24", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayStoreFn &&
            e.Action.Contains("[game+40]", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == 0x009F2F90u);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == 0x00415BF0u);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == 0x009F2F60u);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == 0x00A0BF20u);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == 0x004350D0u);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Game_0044C6B6_ensures_0xE0_singleton_before_Thing_Components()
    {
        Assert.Equal(0x0044C6B6u, EngineLifecycle.PlayerManagerPresentFn);
        Assert.Equal(0x0044C6C2u, EngineLifecycle.PlayerManagerCtorFn);
        Assert.Equal(0x0044C71Fu, EngineLifecycle.PlayerManagerStoreFn);
        Assert.Equal(0x01232C24u, EngineLifecycle.PlayerManagerVtbl);
        Assert.Equal(0xE0, EngineLifecycle.PlayerManagerSize);
        Assert.Equal(0x80000, EngineLifecycle.PlayerManagerPlus40);
        Assert.NotEqual(EngineLifecycle.PlayerManagerGetter, EngineLifecycle.PlayerManagerPresentFn);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.PlayerManagerPresent);
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.PlayerManagerPresent);
        var present = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.PlayerManagerPresentFn);
        var ctor = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.PlayerManagerCtorFn);
        var store = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.PlayerManagerStoreFn);
        var things = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Thing Components");
        var getter = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.PlayerManagerGetter &&
            e.Stage == "Create Players");
        Assert.True(present >= 0 && ctor > present && store > ctor);
        Assert.True(things > store, "0044C71F before Init Thing Components");
        Assert.True(getter > things, "0044C6B0 getter is later Create Players");
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004EE23F_adds_CHeroMorphDef_against_plus40()
    {
        Assert.Equal(0x004EE23Fu, EngineLifecycle.InitThingComponentsFn);
        Assert.Equal(0x009B0AC0u, EngineLifecycle.AddDefClassFn);
        Assert.Equal(0x009AD6E0u, EngineLifecycle.LoadDefFn);
        Assert.Equal(0x009FC4F0u, EngineLifecycle.LoadDefBudgetFn);
        Assert.Equal(0x004E4219u, EngineLifecycle.FirstDefClassFactory);
        Assert.Equal("CHeroMorphDef", EngineLifecycle.FirstDefClassName);
        Assert.Equal(0x80000, EngineLifecycle.PlayerManagerPlus40);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.FirstDefClassRegistered);
        Assert.Null(life.FirstDefClass);
        Assert.Equal(0, life.PlayerManagerPlus40Cap);
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.True(life.PlayerManagerPresent);
        Assert.Equal(EngineLifecycle.PlayerManagerPlus40, life.PlayerManagerPlus40Cap);
        Assert.True(life.FirstDefClassRegistered);
        Assert.Equal(EngineLifecycle.FirstDefClassName, life.FirstDefClass);
        var store = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.PlayerManagerStoreFn);
        var getter = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.PlayerManagerGetter &&
            e.Stage == "Init Thing Components");
        var add = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.AddDefClassFn);
        var factory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FirstDefClassFactory);
        var load = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.LoadDefFn);
        var budget = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.LoadDefBudgetFn);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(store >= 0 && getter > store);
        Assert.True(add > getter && factory > add);
        Assert.True(load > factory && budget > load);
        Assert.True(defs > budget, "CHeroMorphDef before Init Definition Manager");
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004EE565_adds_CHighlightItemDef()
    {
        Assert.Equal(0x004EE565u, EngineLifecycle.SecondDefClassSite);
        Assert.Equal(0x004D8671u, EngineLifecycle.SecondDefClassFactory);
        Assert.Equal(0x0123BD14u, EngineLifecycle.SecondDefClassVtbl);
        Assert.Equal(72, EngineLifecycle.SecondDefClassSize);
        Assert.Equal("CHighlightItemDef", EngineLifecycle.SecondDefClassName);
        Assert.NotEqual(EngineLifecycle.FirstDefClassName, EngineLifecycle.SecondDefClassName);
        Assert.NotEqual(EngineLifecycle.FirstDefClassFactory, EngineLifecycle.SecondDefClassFactory);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.SecondDefClassRegistered);
        Assert.Null(life.SecondDefClass);
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.True(life.FirstDefClassRegistered);
        Assert.Equal(EngineLifecycle.FirstDefClassName, life.FirstDefClass);
        Assert.True(life.SecondDefClassRegistered);
        Assert.Equal(EngineLifecycle.SecondDefClassName, life.SecondDefClass);
        var firstFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FirstDefClassFactory);
        var secondSite = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SecondDefClassSite);
        var secondAdd = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.AddDefClassFn &&
            e.Action.Contains(EngineLifecycle.SecondDefClassName, StringComparison.Ordinal));
        var secondFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SecondDefClassFactory);
        var secondLoad = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.LoadDefFn);
        var secondBudget = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.LoadDefBudgetFn);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(firstFactory >= 0 && secondSite > firstFactory);
        Assert.True(secondAdd > secondSite && secondFactory > secondAdd);
        Assert.True(secondLoad > secondFactory && secondBudget > secondLoad);
        Assert.True(defs > secondBudget, "CHighlightItemDef before Init Definition Manager");
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.AddDefClassFn &&
            e.Action.Contains("CTCSimpleAppearanceMorph", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004EE632_adds_CSmokeGeneratorDef()
    {
        Assert.Equal(0x004EE62Bu, EngineLifecycle.ThirdDefClassSite);
        Assert.Equal(0x004DA82Bu, EngineLifecycle.ThirdDefClassFactory);
        Assert.Equal(0x0123E924u, EngineLifecycle.ThirdDefClassVtbl);
        Assert.Equal(48, EngineLifecycle.ThirdDefClassSize);
        Assert.Equal("CSmokeGeneratorDef", EngineLifecycle.ThirdDefClassName);
        Assert.NotEqual(EngineLifecycle.SecondDefClassName, EngineLifecycle.ThirdDefClassName);
        Assert.NotEqual(EngineLifecycle.SecondDefClassFactory, EngineLifecycle.ThirdDefClassFactory);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.ThirdDefClassRegistered);
        Assert.Null(life.ThirdDefClass);
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.True(life.SecondDefClassRegistered);
        Assert.Equal(EngineLifecycle.SecondDefClassName, life.SecondDefClass);
        Assert.True(life.ThirdDefClassRegistered);
        Assert.Equal(EngineLifecycle.ThirdDefClassName, life.ThirdDefClass);
        var secondFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SecondDefClassFactory);
        var thirdSite = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirdDefClassSite);
        var thirdAdd = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.AddDefClassFn &&
            e.Action.Contains(EngineLifecycle.ThirdDefClassName, StringComparison.Ordinal));
        var thirdFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirdDefClassFactory);
        var thirdLoad = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.LoadDefFn);
        var thirdBudget = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.LoadDefBudgetFn);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(secondFactory >= 0 && thirdSite > secondFactory);
        Assert.True(thirdAdd > thirdSite && thirdFactory > thirdAdd);
        Assert.True(thirdLoad > thirdFactory && thirdBudget > thirdLoad);
        Assert.True(defs > thirdBudget, "CSmokeGeneratorDef before Init Definition Manager");
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.AddDefClassFn &&
            e.Action.Contains("CTCSmokeGenerator", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004EE704_adds_CTimeAppearanceFadeDef()
    {
        Assert.Equal(0x004EE6FDu, EngineLifecycle.FourthDefClassSite);
        Assert.Equal(0x004D84C8u, EngineLifecycle.FourthDefClassFactory);
        Assert.Equal(0x0123B7CCu, EngineLifecycle.FourthDefClassVtbl);
        Assert.Equal(56, EngineLifecycle.FourthDefClassSize);
        Assert.Equal("CTimeAppearanceFadeDef", EngineLifecycle.FourthDefClassName);
        Assert.NotEqual(EngineLifecycle.ThirdDefClassName, EngineLifecycle.FourthDefClassName);
        Assert.NotEqual(EngineLifecycle.ThirdDefClassFactory, EngineLifecycle.FourthDefClassFactory);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.FourthDefClassRegistered);
        Assert.Null(life.FourthDefClass);
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.True(life.ThirdDefClassRegistered);
        Assert.Equal(EngineLifecycle.ThirdDefClassName, life.ThirdDefClass);
        Assert.True(life.FourthDefClassRegistered);
        Assert.Equal(EngineLifecycle.FourthDefClassName, life.FourthDefClass);
        var thirdFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirdDefClassFactory);
        var fourthSite = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FourthDefClassSite);
        var fourthAdd = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.AddDefClassFn &&
            e.Action.Contains(EngineLifecycle.FourthDefClassName, StringComparison.Ordinal));
        var fourthFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FourthDefClassFactory);
        var fourthLoad = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.LoadDefFn);
        var fourthBudget = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.LoadDefBudgetFn);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(thirdFactory >= 0 && fourthSite > thirdFactory);
        Assert.True(fourthAdd > fourthSite && fourthFactory > fourthAdd);
        Assert.True(fourthLoad > fourthFactory && fourthBudget > fourthLoad);
        Assert.True(defs > fourthBudget, "CTimeAppearanceFadeDef before Init Definition Manager");
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.AddDefClassFn &&
            e.Action.Contains("CTCTimeAppearanceFade", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004EE932_adds_CCreatureNavigationDef()
    {
        Assert.Equal(0x004EE92Bu, EngineLifecycle.FifthDefClassSite);
        Assert.Equal(0x004DA871u, EngineLifecycle.FifthDefClassFactory);
        Assert.Equal(0x0123E98Cu, EngineLifecycle.FifthDefClassVtbl);
        Assert.Equal(56, EngineLifecycle.FifthDefClassSize);
        Assert.Equal("CCreatureNavigationDef", EngineLifecycle.FifthDefClassName);
        Assert.NotEqual(EngineLifecycle.FourthDefClassName, EngineLifecycle.FifthDefClassName);
        Assert.NotEqual(EngineLifecycle.FourthDefClassFactory, EngineLifecycle.FifthDefClassFactory);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.FifthDefClassRegistered);
        Assert.Null(life.FifthDefClass);
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.True(life.FourthDefClassRegistered);
        Assert.Equal(EngineLifecycle.FourthDefClassName, life.FourthDefClass);
        Assert.True(life.FifthDefClassRegistered);
        Assert.Equal(EngineLifecycle.FifthDefClassName, life.FifthDefClass);
        var fourthFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FourthDefClassFactory);
        var fifthSite = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FifthDefClassSite);
        var fifthAdd = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.AddDefClassFn &&
            e.Action.Contains(EngineLifecycle.FifthDefClassName, StringComparison.Ordinal));
        var fifthFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FifthDefClassFactory);
        var fifthLoad = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.LoadDefFn);
        var fifthBudget = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.LoadDefBudgetFn);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fourthFactory >= 0 && fifthSite > fourthFactory);
        Assert.True(fifthAdd > fifthSite && fifthFactory > fifthAdd);
        Assert.True(fifthLoad > fifthFactory && fifthBudget > fifthLoad);
        Assert.True(defs > fifthBudget, "CCreatureNavigationDef before Init Definition Manager");
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.AddDefClassFn &&
            (e.Action.Contains("CTCPhysicsLight", StringComparison.Ordinal) ||
             e.Action.Contains("CTCPhysicsStandard", StringComparison.Ordinal) ||
             e.Action.Contains("CTCPhysicsControlled", StringComparison.Ordinal) ||
             e.Action.Contains("CTCCreatureNavigation", StringComparison.Ordinal)));
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004EF244_adds_CInventoryItemDef()
    {
        Assert.Equal(0x004EF23Du, EngineLifecycle.SixthDefClassSite);
        Assert.Equal(0x0044F644u, EngineLifecycle.SixthDefClassFactory);
        Assert.Equal(0x0044C108u, EngineLifecycle.SixthDefClassCtor);
        Assert.Equal(0x01231DBCu, EngineLifecycle.SixthDefClassVtbl);
        Assert.Equal(112, EngineLifecycle.SixthDefClassSize);
        Assert.Equal("CInventoryItemDef", EngineLifecycle.SixthDefClassName);
        Assert.NotEqual(EngineLifecycle.FifthDefClassName, EngineLifecycle.SixthDefClassName);
        Assert.NotEqual(EngineLifecycle.FifthDefClassFactory, EngineLifecycle.SixthDefClassFactory);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.SixthDefClassRegistered);
        Assert.Null(life.SixthDefClass);
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.True(life.FifthDefClassRegistered);
        Assert.Equal(EngineLifecycle.FifthDefClassName, life.FifthDefClass);
        Assert.True(life.SixthDefClassRegistered);
        Assert.Equal(EngineLifecycle.SixthDefClassName, life.SixthDefClass);
        var fifthFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FifthDefClassFactory);
        var sixthSite = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixthDefClassSite);
        var sixthAdd = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.AddDefClassFn &&
            e.Action.Contains(EngineLifecycle.SixthDefClassName, StringComparison.Ordinal));
        var sixthFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixthDefClassFactory);
        var sixthLoad = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.LoadDefFn);
        var sixthBudget = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.LoadDefBudgetFn);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fifthFactory >= 0 && sixthSite > fifthFactory);
        Assert.True(sixthAdd > sixthSite && sixthFactory > sixthAdd);
        Assert.True(sixthLoad > sixthFactory && sixthBudget > sixthLoad);
        Assert.True(defs > sixthBudget, "CInventoryItemDef before Init Definition Manager");
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004EF386_adds_CLookDef()
    {
        Assert.Equal(0x004EF37Fu, EngineLifecycle.SeventhDefClassSite);
        Assert.Equal(0x004D80E4u, EngineLifecycle.SeventhDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.SeventhDefClassCtor);
        Assert.Equal(0x0123AE14u, EngineLifecycle.SeventhDefClassVtbl);
        Assert.Equal(88, EngineLifecycle.SeventhDefClassSize);
        Assert.Equal("CLookDef", EngineLifecycle.SeventhDefClassName);
        Assert.NotEqual(EngineLifecycle.SixthDefClassName, EngineLifecycle.SeventhDefClassName);
        Assert.NotEqual(EngineLifecycle.SixthDefClassFactory, EngineLifecycle.SeventhDefClassFactory);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.SeventhDefClassRegistered);
        Assert.Null(life.SeventhDefClass);
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.True(life.SixthDefClassRegistered);
        Assert.Equal(EngineLifecycle.SixthDefClassName, life.SixthDefClass);
        Assert.True(life.SeventhDefClassRegistered);
        Assert.Equal(EngineLifecycle.SeventhDefClassName, life.SeventhDefClass);
        var sixthFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixthDefClassFactory);
        var seventhSite = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventhDefClassSite);
        var seventhAdd = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.AddDefClassFn &&
            e.Action.Contains(EngineLifecycle.SeventhDefClassName, StringComparison.Ordinal));
        var seventhFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventhDefClassFactory);
        var seventhLoad = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.LoadDefFn);
        var seventhBudget = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.LoadDefBudgetFn);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(sixthFactory >= 0 && seventhSite > sixthFactory);
        Assert.True(seventhAdd > seventhSite && seventhFactory > seventhAdd);
        Assert.True(seventhLoad > seventhFactory && seventhBudget > seventhLoad);
        Assert.True(defs > seventhBudget, "CLookDef before Init Definition Manager");
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.AddDefClassFn &&
            (e.Action.Contains("CTCCreatureExpression", StringComparison.Ordinal) ||
             e.Action.Contains("CTCLook", StringComparison.Ordinal)));
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004EF5B4_adds_CReadableDef()
    {
        Assert.Equal(0x004EF5ADu, EngineLifecycle.EighthDefClassSite);
        Assert.Equal(0x004DAA0Eu, EngineLifecycle.EighthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.EighthDefClassCtor);
        Assert.Equal(0x0123E9F4u, EngineLifecycle.EighthDefClassVtbl);
        Assert.Equal(38, EngineLifecycle.EighthDefClassSize);
        Assert.Equal("CReadableDef", EngineLifecycle.EighthDefClassName);
        Assert.NotEqual(EngineLifecycle.SeventhDefClassName, EngineLifecycle.EighthDefClassName);
        Assert.NotEqual(EngineLifecycle.SeventhDefClassFactory, EngineLifecycle.EighthDefClassFactory);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.EighthDefClassRegistered);
        Assert.Null(life.EighthDefClass);
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.True(life.SeventhDefClassRegistered);
        Assert.Equal(EngineLifecycle.SeventhDefClassName, life.SeventhDefClass);
        Assert.True(life.EighthDefClassRegistered);
        Assert.Equal(EngineLifecycle.EighthDefClassName, life.EighthDefClass);
        var seventhFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventhDefClassFactory);
        var eighthSite = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EighthDefClassSite);
        var eighthAdd = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.AddDefClassFn &&
            e.Action.Contains(EngineLifecycle.EighthDefClassName, StringComparison.Ordinal));
        var eighthFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EighthDefClassFactory);
        var eighthLoad = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.LoadDefFn);
        var eighthBudget = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.LoadDefBudgetFn);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(seventhFactory >= 0 && eighthSite > seventhFactory);
        Assert.True(eighthAdd > eighthSite && eighthFactory > eighthAdd);
        Assert.True(eighthLoad > eighthFactory && eighthBudget > eighthLoad);
        Assert.True(defs > eighthBudget, "CReadableDef before Init Definition Manager");
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.AddDefClassFn &&
            (e.Action.Contains("CTCActionUseTorch", StringComparison.Ordinal) ||
             e.Action.Contains("CTCActionUseReadable", StringComparison.Ordinal)));
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F0178_adds_CVillageDef()
    {
        Assert.Equal(0x004F0171u, EngineLifecycle.NinthDefClassSite);
        Assert.Equal(0x004E213Bu, EngineLifecycle.NinthDefClassFactory);
        Assert.Equal(0x0042DAE0u, EngineLifecycle.NinthDefClassPackFn);
        Assert.Equal(0x004DFF04u, EngineLifecycle.NinthDefClassCtor);
        Assert.Equal(0x01241DDCu, EngineLifecycle.NinthDefClassVtbl);
        Assert.Equal(0x10C, EngineLifecycle.NinthDefClassSize);
        Assert.Equal("CVillageDef", EngineLifecycle.NinthDefClassName);
        Assert.NotEqual(EngineLifecycle.EighthDefClassName, EngineLifecycle.NinthDefClassName);
        Assert.NotEqual(EngineLifecycle.EighthDefClassFactory, EngineLifecycle.NinthDefClassFactory);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.NinthDefClassRegistered);
        Assert.Null(life.NinthDefClass);
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.True(life.EighthDefClassRegistered);
        Assert.Equal(EngineLifecycle.EighthDefClassName, life.EighthDefClass);
        Assert.True(life.NinthDefClassRegistered);
        Assert.Equal(EngineLifecycle.NinthDefClassName, life.NinthDefClass);
        var eighthFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EighthDefClassFactory);
        var ninthSite = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.NinthDefClassSite);
        var ninthPack = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.NinthDefClassPackFn);
        var ninthAdd = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.AddDefClassFn &&
            e.Action.Contains(EngineLifecycle.NinthDefClassName, StringComparison.Ordinal));
        var ninthFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.NinthDefClassFactory);
        var ninthLoad = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.LoadDefFn);
        var ninthBudget = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.LoadDefBudgetFn);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(eighthFactory >= 0 && ninthSite > eighthFactory);
        Assert.True(ninthPack > ninthSite && ninthAdd > ninthPack);
        Assert.True(ninthFactory > ninthAdd && ninthLoad > ninthFactory);
        Assert.True(ninthBudget > ninthLoad);
        Assert.True(defs > ninthBudget, "CVillageDef before Init Definition Manager");
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.AddDefClassFn &&
            e.Action.Contains("CTCActionUseSearch", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F022E_adds_CVillageMemberDef()
    {
        Assert.Equal(0x004F0227u, EngineLifecycle.TenthDefClassSite);
        Assert.Equal(0x004DA7ADu, EngineLifecycle.TenthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.TenthDefClassCtor);
        Assert.Equal(0x0123E854u, EngineLifecycle.TenthDefClassVtbl);
        Assert.Equal(38, EngineLifecycle.TenthDefClassSize);
        Assert.Equal("CVillageMemberDef", EngineLifecycle.TenthDefClassName);
        Assert.NotEqual(EngineLifecycle.NinthDefClassName, EngineLifecycle.TenthDefClassName);
        Assert.NotEqual(EngineLifecycle.NinthDefClassFactory, EngineLifecycle.TenthDefClassFactory);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.TenthDefClassRegistered);
        Assert.Null(life.TenthDefClass);
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.True(life.NinthDefClassRegistered);
        Assert.Equal(EngineLifecycle.NinthDefClassName, life.NinthDefClass);
        Assert.True(life.TenthDefClassRegistered);
        Assert.Equal(EngineLifecycle.TenthDefClassName, life.TenthDefClass);
        var ninthFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.NinthDefClassFactory);
        var tenthSite = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TenthDefClassSite);
        var tenthAdd = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.AddDefClassFn &&
            e.Action.Contains(EngineLifecycle.TenthDefClassName, StringComparison.Ordinal));
        var tenthFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TenthDefClassFactory);
        var tenthLoad = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.LoadDefFn);
        var tenthBudget = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.LoadDefBudgetFn);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(ninthFactory >= 0 && tenthSite > ninthFactory);
        Assert.True(tenthAdd > tenthSite && tenthFactory > tenthAdd);
        Assert.True(tenthLoad > tenthFactory && tenthBudget > tenthLoad);
        Assert.True(defs > tenthBudget, "CVillageMemberDef before Init Definition Manager");
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F02E4_adds_CBuyableHouseDef()
    {
        Assert.Equal(0x004F02DDu, EngineLifecycle.EleventhDefClassSite);
        Assert.Equal(0x004E0148u, EngineLifecycle.EleventhDefClassFactory);
        Assert.Equal(0x004DDB2Cu, EngineLifecycle.EleventhDefClassCtor);
        Assert.Equal(0x0124131Cu, EngineLifecycle.EleventhDefClassVtbl);
        Assert.Equal(76, EngineLifecycle.EleventhDefClassSize);
        Assert.Equal("CBuyableHouseDef", EngineLifecycle.EleventhDefClassName);
        Assert.NotEqual(EngineLifecycle.TenthDefClassName, EngineLifecycle.EleventhDefClassName);
        Assert.NotEqual(EngineLifecycle.TenthDefClassFactory, EngineLifecycle.EleventhDefClassFactory);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.EleventhDefClassRegistered);
        Assert.Null(life.EleventhDefClass);
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.True(life.TenthDefClassRegistered);
        Assert.Equal(EngineLifecycle.TenthDefClassName, life.TenthDefClass);
        Assert.True(life.EleventhDefClassRegistered);
        Assert.Equal(EngineLifecycle.EleventhDefClassName, life.EleventhDefClass);
        var tenthFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TenthDefClassFactory);
        var eleventhSite = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EleventhDefClassSite);
        var eleventhAdd = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.AddDefClassFn &&
            e.Action.Contains(EngineLifecycle.EleventhDefClassName, StringComparison.Ordinal));
        var eleventhFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EleventhDefClassFactory);
        var eleventhLoad = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.LoadDefFn);
        var eleventhBudget = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.LoadDefBudgetFn);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(tenthFactory >= 0 && eleventhSite > tenthFactory);
        Assert.True(eleventhAdd > eleventhSite && eleventhFactory > eleventhAdd);
        Assert.True(eleventhLoad > eleventhFactory && eleventhBudget > eleventhLoad);
        Assert.True(defs > eleventhBudget, "CBuyableHouseDef before Init Definition Manager");
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F039A_adds_CBuyHouseDef()
    {
        Assert.Equal(0x004F0393u, EngineLifecycle.TwelfthDefClassSite);
        Assert.Equal(0x004D7B5Bu, EngineLifecycle.TwelfthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.TwelfthDefClassCtor);
        Assert.Equal(0x0123A61Cu, EngineLifecycle.TwelfthDefClassVtbl);
        Assert.Equal(38, EngineLifecycle.TwelfthDefClassSize);
        Assert.Equal("CBuyHouseDef", EngineLifecycle.TwelfthDefClassName);
        Assert.NotEqual(EngineLifecycle.EleventhDefClassName, EngineLifecycle.TwelfthDefClassName);
        Assert.NotEqual(EngineLifecycle.EleventhDefClassFactory, EngineLifecycle.TwelfthDefClassFactory);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.TwelfthDefClassRegistered);
        Assert.Null(life.TwelfthDefClass);
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.True(life.EleventhDefClassRegistered);
        Assert.Equal(EngineLifecycle.EleventhDefClassName, life.EleventhDefClass);
        Assert.True(life.TwelfthDefClassRegistered);
        Assert.Equal(EngineLifecycle.TwelfthDefClassName, life.TwelfthDefClass);
        var eleventhFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EleventhDefClassFactory);
        var twelfthSite = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwelfthDefClassSite);
        var twelfthAdd = life.Trace.Events.FindLastIndex(e =>
            e.Va == EngineLifecycle.AddDefClassFn &&
            e.Action.Contains(EngineLifecycle.TwelfthDefClassName, StringComparison.Ordinal));
        var twelfthFactory = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwelfthDefClassFactory);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(eleventhFactory >= 0 && twelfthSite > eleventhFactory);
        Assert.True(twelfthAdd > twelfthSite && twelfthFactory > twelfthAdd);
        Assert.True(defs > twelfthFactory, "CBuyHouseDef before Init Definition Manager");
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F04BB_adds_CWifeDef()
    {
        Assert.Equal(0x004F04B4u, EngineLifecycle.ThirteenthDefClassSite);
        Assert.Equal(0x004D7BA1u, EngineLifecycle.ThirteenthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.ThirteenthDefClassCtor);
        Assert.Equal(0x0123A69Cu, EngineLifecycle.ThirteenthDefClassVtbl);
        Assert.Equal(44, EngineLifecycle.ThirteenthDefClassSize);
        Assert.Equal("CWifeDef", EngineLifecycle.ThirteenthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.TwelfthDefClassRegistered);
        Assert.True(life.ThirteenthDefClassRegistered);
        Assert.Equal("CWifeDef", life.ThirteenthDefClass);
        var twelfth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwelfthDefClassFactory);
        var thirteenth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirteenthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(twelfth >= 0 && thirteenth > twelfth && defs > thirteenth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F0647_adds_CDoorDef()
    {
        Assert.Equal(0x004F0640u, EngineLifecycle.FourteenthDefClassSite);
        Assert.Equal(0x004D7BE7u, EngineLifecycle.FourteenthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.FourteenthDefClassCtor);
        Assert.Equal(0x0123A714u, EngineLifecycle.FourteenthDefClassVtbl);
        Assert.Equal(60, EngineLifecycle.FourteenthDefClassSize);
        Assert.Equal("CDoorDef", EngineLifecycle.FourteenthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.ThirteenthDefClassRegistered);
        Assert.True(life.FourteenthDefClassRegistered);
        Assert.Equal("CDoorDef", life.FourteenthDefClass);
        var thirteenth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirteenthDefClassFactory);
        var fourteenth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FourteenthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(thirteenth >= 0 && fourteenth > thirteenth && defs > fourteenth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F06FD_adds_CLightDef()
    {
        Assert.Equal(0x004F06F6u, EngineLifecycle.FifteenthDefClassSite);
        Assert.Equal(0x004D7C73u, EngineLifecycle.FifteenthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.FifteenthDefClassCtor);
        Assert.Equal(0x0123A814u, EngineLifecycle.FifteenthDefClassVtbl);
        Assert.Equal(92, EngineLifecycle.FifteenthDefClassSize);
        Assert.Equal("CLightDef", EngineLifecycle.FifteenthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FourteenthDefClassRegistered);
        Assert.True(life.FifteenthDefClassRegistered);
        Assert.Equal("CLightDef", life.FifteenthDefClass);
        var fourteenth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FourteenthDefClassFactory);
        var fifteenth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FifteenthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fourteenth >= 0 && fifteenth > fourteenth && defs > fifteenth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F07B3_adds_CSpotLightDef()
    {
        Assert.Equal(0x004F07ACu, EngineLifecycle.SixteenthDefClassSite);
        Assert.Equal(0x004D7CB9u, EngineLifecycle.SixteenthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.SixteenthDefClassCtor);
        Assert.Equal(0x0123A88Cu, EngineLifecycle.SixteenthDefClassVtbl);
        Assert.Equal(68, EngineLifecycle.SixteenthDefClassSize);
        Assert.Equal("CSpotLightDef", EngineLifecycle.SixteenthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FifteenthDefClassRegistered);
        Assert.True(life.SixteenthDefClassRegistered);
        Assert.Equal("CSpotLightDef", life.SixteenthDefClass);
        var fifteenth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FifteenthDefClassFactory);
        var sixteenth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixteenthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fifteenth >= 0 && sixteenth > fifteenth && defs > sixteenth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F0869_adds_CClockDef()
    {
        Assert.Equal(0x004F0862u, EngineLifecycle.SeventeenthDefClassSite);
        Assert.Equal(0x004E4477u, EngineLifecycle.SeventeenthDefClassFactory);
        Assert.Equal(0x004E380Eu, EngineLifecycle.SeventeenthDefClassCtor);
        Assert.Equal(0x01242C34u, EngineLifecycle.SeventeenthDefClassVtbl);
        Assert.Equal(56, EngineLifecycle.SeventeenthDefClassSize);
        Assert.Equal("CClockDef", EngineLifecycle.SeventeenthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SixteenthDefClassRegistered);
        Assert.True(life.SeventeenthDefClassRegistered);
        Assert.Equal("CClockDef", life.SeventeenthDefClass);
        var sixteenth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixteenthDefClassFactory);
        var seventeenth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventeenthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(sixteenth >= 0 && seventeenth > sixteenth && defs > seventeenth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F091F_adds_CHeroDef()
    {
        Assert.Equal(0x004F0918u, EngineLifecycle.EighteenthDefClassSite);
        Assert.Equal(0x004D7CFFu, EngineLifecycle.EighteenthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.EighteenthDefClassCtor);
        Assert.Equal(0x0123A904u, EngineLifecycle.EighteenthDefClassVtbl);
        Assert.Equal(48, EngineLifecycle.EighteenthDefClassSize);
        Assert.Equal("CHeroDef", EngineLifecycle.EighteenthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SeventeenthDefClassRegistered);
        Assert.True(life.EighteenthDefClassRegistered);
        Assert.Equal("CHeroDef", life.EighteenthDefClass);
        var seventeenth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventeenthDefClassFactory);
        var eighteenth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EighteenthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(seventeenth >= 0 && eighteenth > seventeenth && defs > eighteenth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F0D2D_adds_CCreatureModeDef()
    {
        Assert.Equal(0x004F0D26u, EngineLifecycle.NineteenthDefClassSite);
        Assert.Equal(0x004E0B4Bu, EngineLifecycle.NineteenthDefClassFactory);
        Assert.Equal(0x004DE7DCu, EngineLifecycle.NineteenthDefClassCtor);
        Assert.Equal(0x01241704u, EngineLifecycle.NineteenthDefClassVtbl);
        Assert.Equal(64, EngineLifecycle.NineteenthDefClassSize);
        Assert.Equal("CCreatureModeDef", EngineLifecycle.NineteenthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.EighteenthDefClassRegistered);
        Assert.True(life.NineteenthDefClassRegistered);
        Assert.Equal("CCreatureModeDef", life.NineteenthDefClass);
        var eighteenth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EighteenthDefClassSite);
        var nineteenth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.NineteenthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(eighteenth >= 0 && nineteenth > eighteenth && defs > nineteenth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F0DE3_adds_CPerceivedThingDef()
    {
        Assert.Equal(0x004F0DDCu, EngineLifecycle.TwentiethDefClassSite);
        Assert.Equal(0x004D7EB6u, EngineLifecycle.TwentiethDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.TwentiethDefClassCtor);
        Assert.Equal(0x0123AA9Cu, EngineLifecycle.TwentiethDefClassVtbl);
        Assert.Equal(80, EngineLifecycle.TwentiethDefClassSize);
        Assert.Equal("CPerceivedThingDef", EngineLifecycle.TwentiethDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.NineteenthDefClassRegistered);
        Assert.True(life.TwentiethDefClassRegistered);
        Assert.Equal("CPerceivedThingDef", life.TwentiethDefClass);
        var nineteenth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.NineteenthDefClassSite);
        var twentieth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwentiethDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(nineteenth >= 0 && twentieth > nineteenth && defs > twentieth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F0E99_adds_CBedDef()
    {
        Assert.Equal(0x004F0E92u, EngineLifecycle.TwentyFirstDefClassSite);
        Assert.Equal(0x004DA7F3u, EngineLifecycle.TwentyFirstDefClassFactory);
        Assert.Equal(0x004D7A25u, EngineLifecycle.TwentyFirstDefClassCtor);
        Assert.Equal(0x0123E8BCu, EngineLifecycle.TwentyFirstDefClassVtbl);
        Assert.Equal(60, EngineLifecycle.TwentyFirstDefClassSize);
        Assert.Equal("CBedDef", EngineLifecycle.TwentyFirstDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.TwentiethDefClassRegistered);
        Assert.True(life.TwentyFirstDefClassRegistered);
        Assert.Equal("CBedDef", life.TwentyFirstDefClass);
        var twentieth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwentiethDefClassSite);
        var twentyFirst = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwentyFirstDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(twentieth >= 0 && twentyFirst > twentieth && defs > twentyFirst);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F0F4F_adds_CStealthDef()
    {
        Assert.Equal(0x004F0F48u, EngineLifecycle.TwentySecondDefClassSite);
        Assert.Equal(0x004D7EFCu, EngineLifecycle.TwentySecondDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.TwentySecondDefClassCtor);
        Assert.Equal(0x0123AB1Cu, EngineLifecycle.TwentySecondDefClassVtbl);
        Assert.Equal(72, EngineLifecycle.TwentySecondDefClassSize);
        Assert.Equal("CStealthDef", EngineLifecycle.TwentySecondDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.TwentyFirstDefClassRegistered);
        Assert.True(life.TwentySecondDefClassRegistered);
        Assert.Equal("CStealthDef", life.TwentySecondDefClass);
        var twentyFirst = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwentyFirstDefClassSite);
        var twentySecond = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwentySecondDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(twentyFirst >= 0 && twentySecond > twentyFirst && defs > twentySecond);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F10DB_adds_CTrophyDef()
    {
        Assert.Equal(0x004F10D4u, EngineLifecycle.TwentyThirdDefClassSite);
        Assert.Equal(0x004D7F7Bu, EngineLifecycle.TwentyThirdDefClassFactory);
        Assert.Equal(0x004D36FEu, EngineLifecycle.TwentyThirdDefClassCtor);
        Assert.Equal(0x0123AC1Cu, EngineLifecycle.TwentyThirdDefClassVtbl);
        Assert.Equal(100, EngineLifecycle.TwentyThirdDefClassSize);
        Assert.Equal("CTrophyDef", EngineLifecycle.TwentyThirdDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.TwentySecondDefClassRegistered);
        Assert.True(life.TwentyThirdDefClassRegistered);
        Assert.Equal("CTrophyDef", life.TwentyThirdDefClass);
        var twentySecond = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwentySecondDefClassSite);
        var twentyThird = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwentyThirdDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(twentySecond >= 0 && twentyThird > twentySecond && defs > twentyThird);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F11FC_adds_CCreatureGeneratorDef()
    {
        Assert.Equal(0x004F11F5u, EngineLifecycle.TwentyFourthDefClassSite);
        Assert.Equal(0x004E0513u, EngineLifecycle.TwentyFourthDefClassFactory);
        Assert.Equal(0x004DE1DFu, EngineLifecycle.TwentyFourthDefClassCtor);
        Assert.Equal(0x01241384u, EngineLifecycle.TwentyFourthDefClassVtbl);
        Assert.Equal(64, EngineLifecycle.TwentyFourthDefClassSize);
        Assert.Equal("CCreatureGeneratorDef", EngineLifecycle.TwentyFourthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.TwentyThirdDefClassRegistered);
        Assert.True(life.TwentyFourthDefClassRegistered);
        Assert.Equal("CCreatureGeneratorDef", life.TwentyFourthDefClass);
        var twentyThird = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwentyThirdDefClassSite);
        var twentyFourth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwentyFourthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(twentyThird >= 0 && twentyFourth > twentyThird && defs > twentyFourth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F12B2_adds_CChestDef()
    {
        Assert.Equal(0x004F12ABu, EngineLifecycle.TwentyFifthDefClassSite);
        Assert.Equal(0x004D805Cu, EngineLifecycle.TwentyFifthDefClassFactory);
        Assert.Equal(0x004D3826u, EngineLifecycle.TwentyFifthDefClassCtor);
        Assert.Equal(0x0123ACDCu, EngineLifecycle.TwentyFifthDefClassVtbl);
        Assert.Equal(60, EngineLifecycle.TwentyFifthDefClassSize);
        Assert.Equal("CChestDef", EngineLifecycle.TwentyFifthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.TwentyFourthDefClassRegistered);
        Assert.True(life.TwentyFifthDefClassRegistered);
        Assert.Equal("CChestDef", life.TwentyFifthDefClass);
        var twentyFourth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwentyFourthDefClassSite);
        var twentyFifth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwentyFifthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(twentyFourth >= 0 && twentyFifth > twentyFourth && defs > twentyFifth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F14A9_adds_CExplodingObjectDef()
    {
        Assert.Equal(0x004F14A2u, EngineLifecycle.TwentySixthDefClassSite);
        Assert.Equal(0x004D809Eu, EngineLifecycle.TwentySixthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.TwentySixthDefClassCtor);
        Assert.Equal(0x0123AD7Cu, EngineLifecycle.TwentySixthDefClassVtbl);
        Assert.Equal(52, EngineLifecycle.TwentySixthDefClassSize);
        Assert.Equal("CExplodingObjectDef", EngineLifecycle.TwentySixthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.TwentyFifthDefClassRegistered);
        Assert.True(life.TwentySixthDefClassRegistered);
        Assert.Equal("CExplodingObjectDef", life.TwentySixthDefClass);
        var twentyFifth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwentyFifthDefClassSite);
        var twentySixth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwentySixthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(twentyFifth >= 0 && twentySixth > twentyFifth && defs > twentySixth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F155F_adds_CContainerRewardHeroDef()
    {
        Assert.Equal(0x004F1558u, EngineLifecycle.TwentySeventhDefClassSite);
        Assert.Equal(0x004E3C81u, EngineLifecycle.TwentySeventhDefClassFactory);
        Assert.Equal(0x004E247Au, EngineLifecycle.TwentySeventhDefClassCtor);
        Assert.Equal(0x012428B4u, EngineLifecycle.TwentySeventhDefClassVtbl);
        Assert.Equal(52, EngineLifecycle.TwentySeventhDefClassSize);
        Assert.Equal("CContainerRewardHeroDef", EngineLifecycle.TwentySeventhDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.TwentySixthDefClassRegistered);
        Assert.True(life.TwentySeventhDefClassRegistered);
        Assert.Equal("CContainerRewardHeroDef", life.TwentySeventhDefClass);
        var twentySixth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwentySixthDefClassSite);
        var twentySeventh = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwentySeventhDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(twentySixth >= 0 && twentySeventh > twentySixth && defs > twentySeventh);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F1CC1_adds_CWeaponDef()
    {
        Assert.Equal(0x004F1CBAu, EngineLifecycle.TwentyEighthDefClassSite);
        Assert.Equal(0x004E3D15u, EngineLifecycle.TwentyEighthDefClassFactory);
        Assert.Equal(0x004E2612u, EngineLifecycle.TwentyEighthDefClassCtor);
        Assert.Equal(0x0124291Cu, EngineLifecycle.TwentyEighthDefClassVtbl);
        Assert.Equal(228, EngineLifecycle.TwentyEighthDefClassSize);
        Assert.Equal("CWeaponDef", EngineLifecycle.TwentyEighthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.TwentySeventhDefClassRegistered);
        Assert.True(life.TwentyEighthDefClassRegistered);
        Assert.Equal("CWeaponDef", life.TwentyEighthDefClass);
        var twentySeventh = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwentySeventhDefClassSite);
        var twentyEighth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwentyEighthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(twentySeventh >= 0 && twentyEighth > twentySeventh && defs > twentyEighth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F1D77_adds_CCarryingDef()
    {
        Assert.Equal(0x004F1D70u, EngineLifecycle.TwentyNinthDefClassSite);
        Assert.Equal(0x004DFE62u, EngineLifecycle.TwentyNinthDefClassFactory);
        Assert.Equal(0x004DD8FFu, EngineLifecycle.TwentyNinthDefClassCtor);
        Assert.Equal(0x01241194u, EngineLifecycle.TwentyNinthDefClassVtbl);
        Assert.Equal(56, EngineLifecycle.TwentyNinthDefClassSize);
        Assert.Equal("CCarryingDef", EngineLifecycle.TwentyNinthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.TwentyEighthDefClassRegistered);
        Assert.True(life.TwentyNinthDefClassRegistered);
        Assert.Equal("CCarryingDef", life.TwentyNinthDefClass);
        var twentyEighth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwentyEighthDefClassSite);
        var twentyNinth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwentyNinthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(twentyEighth >= 0 && twentyNinth > twentyEighth && defs > twentyNinth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F1E2D_adds_CCarryableDef()
    {
        Assert.Equal(0x004F1E26u, EngineLifecycle.ThirtiethDefClassSite);
        Assert.Equal(0x004DA767u, EngineLifecycle.ThirtiethDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.ThirtiethDefClassCtor);
        Assert.Equal(0x0123E7ECu, EngineLifecycle.ThirtiethDefClassVtbl);
        Assert.Equal(80, EngineLifecycle.ThirtiethDefClassSize);
        Assert.Equal("CCarryableDef", EngineLifecycle.ThirtiethDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.TwentyNinthDefClassRegistered);
        Assert.True(life.ThirtiethDefClassRegistered);
        Assert.Equal("CCarryableDef", life.ThirtiethDefClass);
        var twentyNinth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.TwentyNinthDefClassSite);
        var thirtieth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirtiethDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(twentyNinth >= 0 && thirtieth > twentyNinth && defs > thirtieth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F1EE3_adds_CEnemyDef()
    {
        Assert.Equal(0x004F1EDCu, EngineLifecycle.ThirtyFirstDefClassSite);
        Assert.Equal(0x004D835Au, EngineLifecycle.ThirtyFirstDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.ThirtyFirstDefClassCtor);
        Assert.Equal(0x0123B3F4u, EngineLifecycle.ThirtyFirstDefClassVtbl);
        Assert.Equal(44, EngineLifecycle.ThirtyFirstDefClassSize);
        Assert.Equal("CEnemyDef", EngineLifecycle.ThirtyFirstDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.ThirtiethDefClassRegistered);
        Assert.True(life.ThirtyFirstDefClassRegistered);
        Assert.Equal("CEnemyDef", life.ThirtyFirstDefClass);
        var thirtieth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirtiethDefClassSite);
        var thirtyFirst = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirtyFirstDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(thirtieth >= 0 && thirtyFirst > thirtieth && defs > thirtyFirst);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F22ED_adds_COpinionOfHeroDef()
    {
        Assert.Equal(0x004F22E6u, EngineLifecycle.ThirtySecondDefClassSite);
        Assert.Equal(0x004D83D9u, EngineLifecycle.ThirtySecondDefClassFactory);
        Assert.Equal(0x004D3F83u, EngineLifecycle.ThirtySecondDefClassCtor);
        Assert.Equal(0x0123B59Cu, EngineLifecycle.ThirtySecondDefClassVtbl);
        Assert.Equal(60, EngineLifecycle.ThirtySecondDefClassSize);
        Assert.Equal("COpinionOfHeroDef", EngineLifecycle.ThirtySecondDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.ThirtyFirstDefClassRegistered);
        Assert.True(life.ThirtySecondDefClassRegistered);
        Assert.Equal("COpinionOfHeroDef", life.ThirtySecondDefClass);
        var thirtyFirst = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirtyFirstDefClassSite);
        var thirtySecond = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirtySecondDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(thirtyFirst >= 0 && thirtySecond > thirtyFirst && defs > thirtySecond);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F2479_adds_CShopDef()
    {
        Assert.Equal(0x004F2472u, EngineLifecycle.ThirtyThirdDefClassSite);
        Assert.Equal(0x004E26BCu, EngineLifecycle.ThirtyThirdDefClassFactory);
        Assert.Equal(0x004E06C3u, EngineLifecycle.ThirtyThirdDefClassCtor);
        Assert.Equal(0x01241F9Cu, EngineLifecycle.ThirtyThirdDefClassVtbl);
        Assert.Equal(208, EngineLifecycle.ThirtyThirdDefClassSize);
        Assert.Equal("CShopDef", EngineLifecycle.ThirtyThirdDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.ThirtySecondDefClassRegistered);
        Assert.True(life.ThirtyThirdDefClassRegistered);
        Assert.Equal("CShopDef", life.ThirtyThirdDefClass);
        var thirtySecond = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirtySecondDefClassSite);
        var thirtyThird = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirtyThirdDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(thirtySecond >= 0 && thirtyThird > thirtySecond && defs > thirtyThird);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F259A_adds_CStockItemDef()
    {
        Assert.Equal(0x004F2593u, EngineLifecycle.ThirtyFourthDefClassSite);
        Assert.Equal(0x004D8482u, EngineLifecycle.ThirtyFourthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.ThirtyFourthDefClassCtor);
        Assert.Equal(0x0123B74Cu, EngineLifecycle.ThirtyFourthDefClassVtbl);
        Assert.Equal(64, EngineLifecycle.ThirtyFourthDefClassSize);
        Assert.Equal("CStockItemDef", EngineLifecycle.ThirtyFourthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.ThirtyThirdDefClassRegistered);
        Assert.True(life.ThirtyFourthDefClassRegistered);
        Assert.Equal("CStockItemDef", life.ThirtyFourthDefClass);
        var thirtyThird = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirtyThirdDefClassSite);
        var thirtyFourth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirtyFourthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(thirtyThird >= 0 && thirtyFourth > thirtyThird && defs > thirtyFourth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F2650_adds_CGiftDef()
    {
        Assert.Equal(0x004F2649u, EngineLifecycle.ThirtyFifthDefClassSite);
        Assert.Equal(0x004D8547u, EngineLifecycle.ThirtyFifthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.ThirtyFifthDefClassCtor);
        Assert.Equal(0x0123B8B4u, EngineLifecycle.ThirtyFifthDefClassVtbl);
        Assert.Equal(48, EngineLifecycle.ThirtyFifthDefClassSize);
        Assert.Equal("CGiftDef", EngineLifecycle.ThirtyFifthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.ThirtyFourthDefClassRegistered);
        Assert.True(life.ThirtyFifthDefClassRegistered);
        Assert.Equal("CGiftDef", life.ThirtyFifthDefClass);
        var thirtyFourth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirtyFourthDefClassSite);
        var thirtyFifth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirtyFifthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(thirtyFourth >= 0 && thirtyFifth > thirtyFourth && defs > thirtyFifth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F27D4_adds_CHeroSuitDef()
    {
        Assert.Equal(0x004F27CDu, EngineLifecycle.ThirtySixthDefClassSite);
        Assert.Equal(0x004E2809u, EngineLifecycle.ThirtySixthDefClassFactory);
        Assert.Equal(0x004E0900u, EngineLifecycle.ThirtySixthDefClassCtor);
        Assert.Equal(0x0124216Cu, EngineLifecycle.ThirtySixthDefClassVtbl);
        Assert.Equal(52, EngineLifecycle.ThirtySixthDefClassSize);
        Assert.Equal("CHeroSuitDef", EngineLifecycle.ThirtySixthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.ThirtyFifthDefClassRegistered);
        Assert.True(life.ThirtySixthDefClassRegistered);
        Assert.Equal("CHeroSuitDef", life.ThirtySixthDefClass);
        var thirtyFifth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirtyFifthDefClassSite);
        var thirtySixth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirtySixthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(thirtyFifth >= 0 && thirtySixth > thirtyFifth && defs > thirtySixth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F2C3D_adds_CHeroExperienceDef()
    {
        Assert.Equal(0x004F2C36u, EngineLifecycle.ThirtySeventhDefClassSite);
        Assert.Equal(0x004EBAE7u, EngineLifecycle.ThirtySeventhDefClassFactory);
        Assert.Equal(0x004EB9E8u, EngineLifecycle.ThirtySeventhDefClassCtor);
        Assert.Equal(0x0124390Cu, EngineLifecycle.ThirtySeventhDefClassVtbl);
        Assert.Equal(180, EngineLifecycle.ThirtySeventhDefClassSize);
        Assert.Equal("CHeroExperienceDef", EngineLifecycle.ThirtySeventhDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.ThirtySixthDefClassRegistered);
        Assert.True(life.ThirtySeventhDefClassRegistered);
        Assert.Equal("CHeroExperienceDef", life.ThirtySeventhDefClass);
        var thirtySixth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirtySixthDefClassSite);
        var thirtySeventh = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirtySeventhDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(thirtySixth >= 0 && thirtySeventh > thirtySixth && defs > thirtySeventh);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F2CEF_adds_CExperienceDef()
    {
        Assert.Equal(0x004F2CE8u, EngineLifecycle.ThirtyEighthDefClassSite);
        Assert.Equal(0x004E27AEu, EngineLifecycle.ThirtyEighthDefClassFactory);
        Assert.Equal(0x004E0860u, EngineLifecycle.ThirtyEighthDefClassCtor);
        Assert.Equal(0x01242104u, EngineLifecycle.ThirtyEighthDefClassVtbl);
        Assert.Equal(80, EngineLifecycle.ThirtyEighthDefClassSize);
        Assert.Equal("CExperienceDef", EngineLifecycle.ThirtyEighthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.ThirtySeventhDefClassRegistered);
        Assert.True(life.ThirtyEighthDefClassRegistered);
        Assert.Equal("CExperienceDef", life.ThirtyEighthDefClass);
        var thirtySeventh = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirtySeventhDefClassSite);
        var thirtyEighth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirtyEighthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(thirtySeventh >= 0 && thirtyEighth > thirtySeventh && defs > thirtyEighth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F2FBC_adds_CReplaceableMeshDef()
    {
        Assert.Equal(0x004F2FB5u, EngineLifecycle.ThirtyNinthDefClassSite);
        Assert.Equal(0x004E60D8u, EngineLifecycle.ThirtyNinthDefClassFactory);
        Assert.Equal(0x004E3E4Cu, EngineLifecycle.ThirtyNinthDefClassCtor);
        Assert.Equal(0x012430BCu, EngineLifecycle.ThirtyNinthDefClassVtbl);
        Assert.Equal(52, EngineLifecycle.ThirtyNinthDefClassSize);
        Assert.Equal("CReplaceableMeshDef", EngineLifecycle.ThirtyNinthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.ThirtyEighthDefClassRegistered);
        Assert.True(life.ThirtyNinthDefClassRegistered);
        Assert.Equal("CReplaceableMeshDef", life.ThirtyNinthDefClass);
        var thirtyEighth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirtyEighthDefClassSite);
        var thirtyNinth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirtyNinthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(thirtyEighth >= 0 && thirtyNinth > thirtyEighth && defs > thirtyNinth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F3072_adds_CMultiStaticMeshDef()
    {
        Assert.Equal(0x004F306Bu, EngineLifecycle.FortiethDefClassSite);
        Assert.Equal(0x004E31FAu, EngineLifecycle.FortiethDefClassFactory);
        Assert.Equal(0x004E1516u, EngineLifecycle.FortiethDefClassCtor);
        Assert.Equal(0x0124265Cu, EngineLifecycle.FortiethDefClassVtbl);
        Assert.Equal(52, EngineLifecycle.FortiethDefClassSize);
        Assert.Equal("CMultiStaticMeshDef", EngineLifecycle.FortiethDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.ThirtyNinthDefClassRegistered);
        Assert.True(life.FortiethDefClassRegistered);
        Assert.Equal("CMultiStaticMeshDef", life.FortiethDefClass);
        var thirtyNinth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThirtyNinthDefClassSite);
        var fortieth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FortiethDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(thirtyNinth >= 0 && fortieth > thirtyNinth && defs > fortieth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F333F_adds_CHeroCentreDef()
    {
        Assert.Equal(0x004F3338u, EngineLifecycle.FortyFirstDefClassSite);
        Assert.Equal(0x004D86F0u, EngineLifecycle.FortyFirstDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.FortyFirstDefClassCtor);
        Assert.Equal(0x0123BE54u, EngineLifecycle.FortyFirstDefClassVtbl);
        Assert.Equal(37, EngineLifecycle.FortyFirstDefClassSize);
        Assert.Equal("CHeroCentreDef", EngineLifecycle.FortyFirstDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FortiethDefClassRegistered);
        Assert.True(life.FortyFirstDefClassRegistered);
        Assert.Equal("CHeroCentreDef", life.FortyFirstDefClass);
        var fortieth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FortiethDefClassSite);
        var fortyFirst = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FortyFirstDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fortieth >= 0 && fortyFirst > fortieth && defs > fortyFirst);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F34CB_adds_CQuestCardDef()
    {
        Assert.Equal(0x004F34C4u, EngineLifecycle.FortySecondDefClassSite);
        Assert.Equal(0x004E2333u, EngineLifecycle.FortySecondDefClassFactory);
        Assert.Equal(0x004E00BCu, EngineLifecycle.FortySecondDefClassCtor);
        Assert.Equal(0x01241E44u, EngineLifecycle.FortySecondDefClassVtbl);
        Assert.Equal(116, EngineLifecycle.FortySecondDefClassSize);
        Assert.Equal("CQuestCardDef", EngineLifecycle.FortySecondDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FortyFirstDefClassRegistered);
        Assert.True(life.FortySecondDefClassRegistered);
        Assert.Equal("CQuestCardDef", life.FortySecondDefClass);
        var fortyFirst = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FortyFirstDefClassSite);
        var fortySecond = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FortySecondDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fortyFirst >= 0 && fortySecond > fortyFirst && defs > fortySecond);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(life.Runtime.Quests, q => q.Name == "Q_NewOakValeIntro");
    }

    [Fact]
    public void Init_Thing_Components_004F3581_adds_CFlammableDef()
    {
        Assert.Equal(0x004F357Au, EngineLifecycle.FortyThirdDefClassSite);
        Assert.Equal(0x004E3DC3u, EngineLifecycle.FortyThirdDefClassFactory);
        Assert.Equal(0x004E284Bu, EngineLifecycle.FortyThirdDefClassCtor);
        Assert.Equal(0x01242984u, EngineLifecycle.FortyThirdDefClassVtbl);
        Assert.Equal(76, EngineLifecycle.FortyThirdDefClassSize);
        Assert.Equal("CFlammableDef", EngineLifecycle.FortyThirdDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FortySecondDefClassRegistered);
        Assert.True(life.FortyThirdDefClassRegistered);
        Assert.Equal("CFlammableDef", life.FortyThirdDefClass);
        var fortySecond = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FortySecondDefClassSite);
        var fortyThird = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FortyThirdDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fortySecond >= 0 && fortyThird > fortySecond && defs > fortyThird);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F3637_adds_CBoastingPodiumDef()
    {
        Assert.Equal(0x004F3630u, EngineLifecycle.FortyFourthDefClassSite);
        Assert.Equal(0x004D8736u, EngineLifecycle.FortyFourthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.FortyFourthDefClassCtor);
        Assert.Equal(0x0123BF0Cu, EngineLifecycle.FortyFourthDefClassVtbl);
        Assert.Equal(44, EngineLifecycle.FortyFourthDefClassSize);
        Assert.Equal("CBoastingPodiumDef", EngineLifecycle.FortyFourthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FortyThirdDefClassRegistered);
        Assert.True(life.FortyFourthDefClassRegistered);
        Assert.Equal("CBoastingPodiumDef", life.FortyFourthDefClass);
        var fortyThird = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FortyThirdDefClassSite);
        var fortyFourth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FortyFourthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fortyThird >= 0 && fortyFourth > fortyThird && defs > fortyFourth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F3899_adds_CTCVolumeContainmentTrackerDef()
    {
        Assert.Equal(0x004F3892u, EngineLifecycle.FortyFifthDefClassSite);
        Assert.Equal(0x004D94C8u, EngineLifecycle.FortyFifthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.FortyFifthDefClassCtor);
        Assert.Equal(0x0123E0C4u, EngineLifecycle.FortyFifthDefClassVtbl);
        Assert.Equal(48, EngineLifecycle.FortyFifthDefClassSize);
        Assert.Equal("CTCVolumeContainmentTrackerDef", EngineLifecycle.FortyFifthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FortyFourthDefClassRegistered);
        Assert.True(life.FortyFifthDefClassRegistered);
        Assert.Equal("CTCVolumeContainmentTrackerDef", life.FortyFifthDefClass);
        var fortyFourth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FortyFourthDefClassSite);
        var fortyFifth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FortyFifthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fortyFourth >= 0 && fortyFifth > fortyFourth && defs > fortyFifth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F3E53_adds_CThingDrainLifeShotDef()
    {
        Assert.Equal(0x004F3E4Cu, EngineLifecycle.FortySixthDefClassSite);
        Assert.Equal(0x004D8D56u, EngineLifecycle.FortySixthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.FortySixthDefClassCtor);
        Assert.Equal(0x0123CCA4u, EngineLifecycle.FortySixthDefClassVtbl);
        Assert.Equal(60, EngineLifecycle.FortySixthDefClassSize);
        Assert.Equal("CThingDrainLifeShotDef", EngineLifecycle.FortySixthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FortyFifthDefClassRegistered);
        Assert.True(life.FortySixthDefClassRegistered);
        Assert.Equal("CThingDrainLifeShotDef", life.FortySixthDefClass);
        var fortyFifth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FortyFifthDefClassSite);
        var fortySixth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FortySixthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fortyFifth >= 0 && fortySixth > fortyFifth && defs > fortySixth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F3F09_adds_CFireballSpellLevelDef()
    {
        Assert.Equal(0x004F3F02u, EngineLifecycle.FortySeventhDefClassSite);
        Assert.Equal(0x004D8D10u, EngineLifecycle.FortySeventhDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.FortySeventhDefClassCtor);
        Assert.Equal(0x0123CC3Cu, EngineLifecycle.FortySeventhDefClassVtbl);
        Assert.Equal(44, EngineLifecycle.FortySeventhDefClassSize);
        Assert.Equal("CFireballSpellLevelDef", EngineLifecycle.FortySeventhDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FortySixthDefClassRegistered);
        Assert.True(life.FortySeventhDefClassRegistered);
        Assert.Equal("CFireballSpellLevelDef", life.FortySeventhDefClass);
        var fortySixth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FortySixthDefClassSite);
        var fortySeventh = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FortySeventhDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fortySixth >= 0 && fortySeventh > fortySixth && defs > fortySeventh);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F4100_adds_CSkeletalMorphDef()
    {
        Assert.Equal(0x004F40F9u, EngineLifecycle.FortyEighthDefClassSite);
        Assert.Equal(0x004E3DD9u, EngineLifecycle.FortyEighthDefClassFactory);
        Assert.Equal(0x004E2895u, EngineLifecycle.FortyEighthDefClassCtor);
        Assert.Equal(0x012429ECu, EngineLifecycle.FortyEighthDefClassVtbl);
        Assert.Equal(52, EngineLifecycle.FortyEighthDefClassSize);
        Assert.Equal("CSkeletalMorphDef", EngineLifecycle.FortyEighthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FortySeventhDefClassRegistered);
        Assert.True(life.FortyEighthDefClassRegistered);
        Assert.Equal("CSkeletalMorphDef", life.FortyEighthDefClass);
        var fortySeventh = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FortySeventhDefClassSite);
        var fortyEighth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FortyEighthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fortySeventh >= 0 && fortyEighth > fortySeventh && defs > fortyEighth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F43CD_adds_CTrapDef()
    {
        Assert.Equal(0x004F43C6u, EngineLifecycle.FortyNinthDefClassSite);
        Assert.Equal(0x004E5CF2u, EngineLifecycle.FortyNinthDefClassFactory);
        Assert.Equal(0x004E3E2Au, EngineLifecycle.FortyNinthDefClassCtor);
        Assert.Equal(0x01243054u, EngineLifecycle.FortyNinthDefClassVtbl);
        Assert.Equal(100, EngineLifecycle.FortyNinthDefClassSize);
        Assert.Equal("CTrapDef", EngineLifecycle.FortyNinthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FortyEighthDefClassRegistered);
        Assert.True(life.FortyNinthDefClassRegistered);
        Assert.Equal("CTrapDef", life.FortyNinthDefClass);
        var fortyEighth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FortyEighthDefClassSite);
        var fortyNinth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FortyNinthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fortyEighth >= 0 && fortyNinth > fortyEighth && defs > fortyNinth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F462F_adds_CParticleAttacherDef()
    {
        Assert.Equal(0x004F4628u, EngineLifecycle.FiftiethDefClassSite);
        Assert.Equal(0x004E2AFAu, EngineLifecycle.FiftiethDefClassFactory);
        Assert.Equal(0x004E0B9Cu, EngineLifecycle.FiftiethDefClassCtor);
        Assert.Equal(0x01242364u, EngineLifecycle.FiftiethDefClassVtbl);
        Assert.Equal(52, EngineLifecycle.FiftiethDefClassSize);
        Assert.Equal("CParticleAttacherDef", EngineLifecycle.FiftiethDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FortyNinthDefClassRegistered);
        Assert.True(life.FiftiethDefClassRegistered);
        Assert.Equal("CParticleAttacherDef", life.FiftiethDefClass);
        Assert.False(EngineLifecycle.FirstSeenCanRenderParticles);
        var fortyNinth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FortyNinthDefClassSite);
        var fiftieth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FiftiethDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fortyNinth >= 0 && fiftieth > fortyNinth && defs > fiftieth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F46E5_adds_CAnimatingObjectDef()
    {
        Assert.Equal(0x004F46DEu, EngineLifecycle.FiftyFirstDefClassSite);
        Assert.Equal(0x004EBA6Eu, EngineLifecycle.FiftyFirstDefClassFactory);
        Assert.Equal(0x004EA1F0u, EngineLifecycle.FiftyFirstDefClassCtor);
        Assert.Equal(0x0124376Cu, EngineLifecycle.FiftyFirstDefClassVtbl);
        Assert.Equal(72, EngineLifecycle.FiftyFirstDefClassSize);
        Assert.Equal("CAnimatingObjectDef", EngineLifecycle.FiftyFirstDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FiftiethDefClassRegistered);
        Assert.True(life.FiftyFirstDefClassRegistered);
        Assert.Equal("CAnimatingObjectDef", life.FiftyFirstDefClass);
        var fiftieth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FiftiethDefClassSite);
        var fiftyFirst = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FiftyFirstDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fiftieth >= 0 && fiftyFirst > fiftieth && defs > fiftyFirst);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F4A1D_adds_CExpressionSubDef()
    {
        Assert.Equal(0x004F4A16u, EngineLifecycle.FiftySecondDefClassSite);
        Assert.Equal(0x004D8818u, EngineLifecycle.FiftySecondDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.FiftySecondDefClassCtor);
        Assert.Equal(0x0123C2E4u, EngineLifecycle.FiftySecondDefClassVtbl);
        Assert.Equal(44, EngineLifecycle.FiftySecondDefClassSize);
        Assert.Equal("CExpressionSubDef", EngineLifecycle.FiftySecondDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FiftyFirstDefClassRegistered);
        Assert.True(life.FiftySecondDefClassRegistered);
        Assert.Equal("CExpressionSubDef", life.FiftySecondDefClass);
        var fiftyFirst = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FiftyFirstDefClassSite);
        var fiftySecond = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FiftySecondDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fiftyFirst >= 0 && fiftySecond > fiftyFirst && defs > fiftySecond);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(life.Runtime.Quests, q => q.Name == "Q_NewOakValeIntro");
    }

    [Fact]
    public void Init_Thing_Components_004F4DC0_adds_CWillResponseDef()
    {
        Assert.Equal(0x004F4DB9u, EngineLifecycle.FiftyThirdDefClassSite);
        Assert.Equal(0x004D9629u, EngineLifecycle.FiftyThirdDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.FiftyThirdDefClassCtor);
        Assert.Equal(0x0123E324u, EngineLifecycle.FiftyThirdDefClassVtbl);
        Assert.Equal(45, EngineLifecycle.FiftyThirdDefClassSize);
        Assert.Equal("CWillResponseDef", EngineLifecycle.FiftyThirdDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FiftySecondDefClassRegistered);
        Assert.True(life.FiftyThirdDefClassRegistered);
        Assert.Equal("CWillResponseDef", life.FiftyThirdDefClass);
        var fiftySecond = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FiftySecondDefClassSite);
        var fiftyThird = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FiftyThirdDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fiftySecond >= 0 && fiftyThird > fiftySecond && defs > fiftyThird);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F4F4C_adds_CTurncoatDef()
    {
        Assert.Equal(0x004F4F45u, EngineLifecycle.FiftyFourthDefClassSite);
        Assert.Equal(0x004E0F9Cu, EngineLifecycle.FiftyFourthDefClassFactory);
        Assert.Equal(0x004DEBA3u, EngineLifecycle.FiftyFourthDefClassCtor);
        Assert.Equal(0x0124193Cu, EngineLifecycle.FiftyFourthDefClassVtbl);
        Assert.Equal(84, EngineLifecycle.FiftyFourthDefClassSize);
        Assert.Equal("CTurncoatDef", EngineLifecycle.FiftyFourthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FiftyThirdDefClassRegistered);
        Assert.True(life.FiftyFourthDefClassRegistered);
        Assert.Equal("CTurncoatDef", life.FiftyFourthDefClass);
        var fiftyThird = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FiftyThirdDefClassSite);
        var fiftyFourth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FiftyFourthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fiftyThird >= 0 && fiftyFourth > fiftyThird && defs > fiftyFourth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F5002_adds_CSummonableCreatureDef()
    {
        Assert.Equal(0x004F4FFBu, EngineLifecycle.FiftyFifthDefClassSite);
        Assert.Equal(0x004D885Eu, EngineLifecycle.FiftyFifthDefClassFactory);
        Assert.Equal(0x004D4C3Eu, EngineLifecycle.FiftyFifthDefClassCtor);
        Assert.Equal(0x0123C3A4u, EngineLifecycle.FiftyFifthDefClassVtbl);
        Assert.Equal(48, EngineLifecycle.FiftyFifthDefClassSize);
        Assert.Equal("CSummonableCreatureDef", EngineLifecycle.FiftyFifthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FiftyFourthDefClassRegistered);
        Assert.True(life.FiftyFifthDefClassRegistered);
        Assert.Equal("CSummonableCreatureDef", life.FiftyFifthDefClass);
        var fiftyFourth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FiftyFourthDefClassSite);
        var fiftyFifth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FiftyFifthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fiftyFourth >= 0 && fiftyFifth > fiftyFourth && defs > fiftyFifth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F55BC_adds_CAIScratchpadDef()
    {
        Assert.Equal(0x004F55B5u, EngineLifecycle.FiftySixthDefClassSite);
        Assert.Equal(0x004D4E07u, EngineLifecycle.FiftySixthDefClassFactory);
        Assert.Equal(0x007ABB30u, EngineLifecycle.FiftySixthDefClassCtor);
        Assert.Equal(0x0126D014u, EngineLifecycle.FiftySixthDefClassVtbl);
        Assert.Equal(156, EngineLifecycle.FiftySixthDefClassSize);
        Assert.Equal("CAIScratchpadDef", EngineLifecycle.FiftySixthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FiftyFifthDefClassRegistered);
        Assert.True(life.FiftySixthDefClassRegistered);
        Assert.Equal("CAIScratchpadDef", life.FiftySixthDefClass);
        var fiftyFifth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FiftyFifthDefClassSite);
        var fiftySixth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FiftySixthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fiftyFifth >= 0 && fiftySixth > fiftyFifth && defs > fiftySixth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F5672_adds_COccupiableDef()
    {
        Assert.Equal(0x004F566Bu, EngineLifecycle.FiftySeventhDefClassSite);
        Assert.Equal(0x004D88FCu, EngineLifecycle.FiftySeventhDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.FiftySeventhDefClassCtor);
        Assert.Equal(0x0123C514u, EngineLifecycle.FiftySeventhDefClassVtbl);
        Assert.Equal(44, EngineLifecycle.FiftySeventhDefClassSize);
        Assert.Equal("COccupiableDef", EngineLifecycle.FiftySeventhDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FiftySixthDefClassRegistered);
        Assert.True(life.FiftySeventhDefClassRegistered);
        Assert.Equal("COccupiableDef", life.FiftySeventhDefClass);
        var fiftySixth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FiftySixthDefClassSite);
        var fiftySeventh = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FiftySeventhDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fiftySixth >= 0 && fiftySeventh > fiftySixth && defs > fiftySeventh);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F5728_adds_CBossDef()
    {
        Assert.Equal(0x004F5721u, EngineLifecycle.FiftyEighthDefClassSite);
        Assert.Equal(0x004E0D4Cu, EngineLifecycle.FiftyEighthDefClassFactory);
        Assert.Equal(0x004DE8C2u, EngineLifecycle.FiftyEighthDefClassCtor);
        Assert.Equal(0x0124185Cu, EngineLifecycle.FiftyEighthDefClassVtbl);
        Assert.Equal(84, EngineLifecycle.FiftyEighthDefClassSize);
        Assert.Equal("CBossDef", EngineLifecycle.FiftyEighthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FiftySeventhDefClassRegistered);
        Assert.True(life.FiftyEighthDefClassRegistered);
        Assert.Equal("CBossDef", life.FiftyEighthDefClass);
        var fiftySeventh = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FiftySeventhDefClassSite);
        var fiftyEighth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FiftyEighthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fiftySeventh >= 0 && fiftyEighth > fiftySeventh && defs > fiftyEighth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F591F_adds_CFishingDef()
    {
        Assert.Equal(0x004F5918u, EngineLifecycle.FiftyNinthDefClassSite);
        Assert.Equal(0x004E0DB9u, EngineLifecycle.FiftyNinthDefClassFactory);
        Assert.Equal(0x004DE8F5u, EngineLifecycle.FiftyNinthDefClassCtor);
        Assert.Equal(0x012418C4u, EngineLifecycle.FiftyNinthDefClassVtbl);
        Assert.Equal(124, EngineLifecycle.FiftyNinthDefClassSize);
        Assert.Equal("CFishingDef", EngineLifecycle.FiftyNinthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FiftyEighthDefClassRegistered);
        Assert.True(life.FiftyNinthDefClassRegistered);
        Assert.Equal("CFishingDef", life.FiftyNinthDefClass);
        var fiftyEighth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FiftyEighthDefClassSite);
        var fiftyNinth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FiftyNinthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fiftyEighth >= 0 && fiftyNinth > fiftyEighth && defs > fiftyNinth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F5A40_adds_CGuardDef()
    {
        Assert.Equal(0x004F5A39u, EngineLifecycle.SixtiethDefClassSite);
        Assert.Equal(0x004D89ECu, EngineLifecycle.SixtiethDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.SixtiethDefClassCtor);
        Assert.Equal(0x0123C75Cu, EngineLifecycle.SixtiethDefClassVtbl);
        Assert.Equal(80, EngineLifecycle.SixtiethDefClassSize);
        Assert.Equal("CGuardDef", EngineLifecycle.SixtiethDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.FiftyNinthDefClassRegistered);
        Assert.True(life.SixtiethDefClassRegistered);
        Assert.Equal("CGuardDef", life.SixtiethDefClass);
        var fiftyNinth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.FiftyNinthDefClassSite);
        var sixtieth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixtiethDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(fiftyNinth >= 0 && sixtieth > fiftyNinth && defs > sixtieth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F5AF6_adds_CInterestingToVillagersDef()
    {
        Assert.Equal(0x004F5AEFu, EngineLifecycle.SixtyFirstDefClassSite);
        Assert.Equal(0x004D89B4u, EngineLifecycle.SixtyFirstDefClassFactory);
        Assert.Equal(0x004D4FCFu, EngineLifecycle.SixtyFirstDefClassCtor);
        Assert.Equal(0x0123C6D4u, EngineLifecycle.SixtyFirstDefClassVtbl);
        Assert.Equal(44, EngineLifecycle.SixtyFirstDefClassSize);
        Assert.Equal("CInterestingToVillagersDef", EngineLifecycle.SixtyFirstDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SixtiethDefClassRegistered);
        Assert.True(life.SixtyFirstDefClassRegistered);
        Assert.Equal("CInterestingToVillagersDef", life.SixtyFirstDefClass);
        var sixtieth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixtiethDefClassSite);
        var sixtyFirst = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixtyFirstDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(sixtieth >= 0 && sixtyFirst > sixtieth && defs > sixtyFirst);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(life.Runtime.Quests, q => q.Name == "Q_NewOakValeIntro");
    }

    [Fact]
    public void Init_Thing_Components_004F5BAC_adds_CActivateQuestDef()
    {
        Assert.Equal(0x004F5BA5u, EngineLifecycle.SixtySecondDefClassSite);
        Assert.Equal(0x004D8A32u, EngineLifecycle.SixtySecondDefClassFactory);
        Assert.Equal(0x004D5056u, EngineLifecycle.SixtySecondDefClassCtor);
        Assert.Equal(0x0123C7F4u, EngineLifecycle.SixtySecondDefClassVtbl);
        Assert.Equal(48, EngineLifecycle.SixtySecondDefClassSize);
        Assert.Equal("CActivateQuestDef", EngineLifecycle.SixtySecondDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SixtyFirstDefClassRegistered);
        Assert.True(life.SixtySecondDefClassRegistered);
        Assert.Equal("CActivateQuestDef", life.SixtySecondDefClass);
        var sixtyFirst = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixtyFirstDefClassSite);
        var sixtySecond = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixtySecondDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(sixtyFirst >= 0 && sixtySecond > sixtyFirst && defs > sixtySecond);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(life.Runtime.Quests, q => q.Name == "Q_NewOakValeIntro");
        Assert.False(EngineLifecycle.ActivateQuestSatisfiesGameflowWait);
    }

    [Fact]
    public void Init_Thing_Components_004F5CCD_adds_CCrateStackDef()
    {
        Assert.Equal(0x004F5CC6u, EngineLifecycle.SixtyThirdDefClassSite);
        Assert.Equal(0x004D8A6Au, EngineLifecycle.SixtyThirdDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.SixtyThirdDefClassCtor);
        Assert.Equal(0x0123C86Cu, EngineLifecycle.SixtyThirdDefClassVtbl);
        Assert.Equal(48, EngineLifecycle.SixtyThirdDefClassSize);
        Assert.Equal("CCrateStackDef", EngineLifecycle.SixtyThirdDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SixtySecondDefClassRegistered);
        Assert.True(life.SixtyThirdDefClassRegistered);
        Assert.Equal("CCrateStackDef", life.SixtyThirdDefClass);
        var sixtySecond = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixtySecondDefClassSite);
        var sixtyThird = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixtyThirdDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(sixtySecond >= 0 && sixtyThird > sixtySecond && defs > sixtyThird);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F5D83_adds_COverheadDisplayDef()
    {
        Assert.Equal(0x004F5D7Cu, EngineLifecycle.SixtyFourthDefClassSite);
        Assert.Equal(0x004D8AB0u, EngineLifecycle.SixtyFourthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.SixtyFourthDefClassCtor);
        Assert.Equal(0x0123C8FCu, EngineLifecycle.SixtyFourthDefClassVtbl);
        Assert.Equal(40, EngineLifecycle.SixtyFourthDefClassSize);
        Assert.Equal("COverheadDisplayDef", EngineLifecycle.SixtyFourthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SixtyThirdDefClassRegistered);
        Assert.True(life.SixtyFourthDefClassRegistered);
        Assert.Equal("COverheadDisplayDef", life.SixtyFourthDefClass);
        var sixtyThird = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixtyThirdDefClassSite);
        var sixtyFourth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixtyFourthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(sixtyThird >= 0 && sixtyFourth > sixtyThird && defs > sixtyFourth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F5E39_adds_CTavernTableDef()
    {
        Assert.Equal(0x004F5E32u, EngineLifecycle.SixtyFifthDefClassSite);
        Assert.Equal(0x004D8AF6u, EngineLifecycle.SixtyFifthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.SixtyFifthDefClassCtor);
        Assert.Equal(0x0123C964u, EngineLifecycle.SixtyFifthDefClassVtbl);
        Assert.Equal(39, EngineLifecycle.SixtyFifthDefClassSize);
        Assert.Equal("CTavernTableDef", EngineLifecycle.SixtyFifthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SixtyFourthDefClassRegistered);
        Assert.True(life.SixtyFifthDefClassRegistered);
        Assert.Equal("CTavernTableDef", life.SixtyFifthDefClass);
        var sixtyFourth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixtyFourthDefClassSite);
        var sixtyFifth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixtyFifthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(sixtyFourth >= 0 && sixtyFifth > sixtyFourth && defs > sixtyFifth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F6030_adds_CTavernDef()
    {
        Assert.Equal(0x004F6029u, EngineLifecycle.SixtySixthDefClassSite);
        Assert.Equal(0x004D8BE1u, EngineLifecycle.SixtySixthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.SixtySixthDefClassCtor);
        Assert.Equal(0x0123CA8Cu, EngineLifecycle.SixtySixthDefClassVtbl);
        Assert.Equal(44, EngineLifecycle.SixtySixthDefClassSize);
        Assert.Equal("CTavernDef", EngineLifecycle.SixtySixthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SixtyFifthDefClassRegistered);
        Assert.True(life.SixtySixthDefClassRegistered);
        Assert.Equal("CTavernDef", life.SixtySixthDefClass);
        var sixtyFifth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixtyFifthDefClassSite);
        var sixtySixth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixtySixthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(sixtyFifth >= 0 && sixtySixth > sixtyFifth && defs > sixtySixth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(life.Runtime.Quests, q => q.Name == "Q_NewOakValeIntro");
    }

    [Fact]
    public void Init_Thing_Components_004F60E6_adds_CObjectAugmentationsDef()
    {
        Assert.Equal(0x004F60DFu, EngineLifecycle.SixtySeventhDefClassSite);
        Assert.Equal(0x004EC526u, EngineLifecycle.SixtySeventhDefClassFactory);
        Assert.Equal(0x004EBBA3u, EngineLifecycle.SixtySeventhDefClassCtor);
        Assert.Equal(0x01243974u, EngineLifecycle.SixtySeventhDefClassVtbl);
        Assert.Equal(140, EngineLifecycle.SixtySeventhDefClassSize);
        Assert.Equal("CObjectAugmentationsDef", EngineLifecycle.SixtySeventhDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SixtySixthDefClassRegistered);
        Assert.True(life.SixtySeventhDefClassRegistered);
        Assert.Equal("CObjectAugmentationsDef", life.SixtySeventhDefClass);
        var sixtySixth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixtySixthDefClassSite);
        var sixtySeventh = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixtySeventhDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(sixtySixth >= 0 && sixtySeventh > sixtySixth && defs > sixtySeventh);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F63B3_adds_CDrunkennessDef()
    {
        Assert.Equal(0x004F63ACu, EngineLifecycle.SixtyEighthDefClassSite);
        Assert.Equal(0x004D8C91u, EngineLifecycle.SixtyEighthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.SixtyEighthDefClassCtor);
        Assert.Equal(0x0123CB3Cu, EngineLifecycle.SixtyEighthDefClassVtbl);
        Assert.Equal(44, EngineLifecycle.SixtyEighthDefClassSize);
        Assert.Equal("CDrunkennessDef", EngineLifecycle.SixtyEighthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SixtySeventhDefClassRegistered);
        Assert.True(life.SixtyEighthDefClassRegistered);
        Assert.Equal("CDrunkennessDef", life.SixtyEighthDefClass);
        var sixtySeventh = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixtySeventhDefClassSite);
        var sixtyEighth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixtyEighthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(sixtySeventh >= 0 && sixtyEighth > sixtySeventh && defs > sixtyEighth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F67C1_adds_CGoldDef()
    {
        Assert.Equal(0x004F67BAu, EngineLifecycle.SixtyNinthDefClassSite);
        Assert.Equal(0x004D8EC5u, EngineLifecycle.SixtyNinthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.SixtyNinthDefClassCtor);
        Assert.Equal(0x0123D2ECu, EngineLifecycle.SixtyNinthDefClassVtbl);
        Assert.Equal(44, EngineLifecycle.SixtyNinthDefClassSize);
        Assert.Equal("CGoldDef", EngineLifecycle.SixtyNinthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SixtyEighthDefClassRegistered);
        Assert.True(life.SixtyNinthDefClassRegistered);
        Assert.Equal("CGoldDef", life.SixtyNinthDefClass);
        var sixtyEighth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixtyEighthDefClassSite);
        var sixtyNinth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixtyNinthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(sixtyEighth >= 0 && sixtyNinth > sixtyEighth && defs > sixtyNinth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F694D_adds_CAICreatureWillPowerIndicatorDef()
    {
        Assert.Equal(0x004F6946u, EngineLifecycle.SeventiethDefClassSite);
        Assert.Equal(0x004D926Au, EngineLifecycle.SeventiethDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.SeventiethDefClassCtor);
        Assert.Equal(0x0123DAA4u, EngineLifecycle.SeventiethDefClassVtbl);
        Assert.Equal(44, EngineLifecycle.SeventiethDefClassSize);
        Assert.Equal("CAICreatureWillPowerIndicatorDef", EngineLifecycle.SeventiethDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SixtyNinthDefClassRegistered);
        Assert.True(life.SeventiethDefClassRegistered);
        Assert.Equal("CAICreatureWillPowerIndicatorDef", life.SeventiethDefClass);
        var sixtyNinth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SixtyNinthDefClassSite);
        var seventieth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventiethDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(sixtyNinth >= 0 && seventieth > sixtyNinth && defs > seventieth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F6998_adds_CKickableDef()
    {
        Assert.Equal(0x004F6991u, EngineLifecycle.SeventyFirstDefClassSite);
        Assert.Equal(0x004D7C2Du, EngineLifecycle.SeventyFirstDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.SeventyFirstDefClassCtor);
        Assert.Equal(0x0123A7ACu, EngineLifecycle.SeventyFirstDefClassVtbl);
        Assert.Equal(84, EngineLifecycle.SeventyFirstDefClassSize);
        Assert.Equal("CKickableDef", EngineLifecycle.SeventyFirstDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SeventiethDefClassRegistered);
        Assert.True(life.SeventyFirstDefClassRegistered);
        Assert.Equal("CKickableDef", life.SeventyFirstDefClass);
        var seventieth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventiethDefClassSite);
        var seventyFirst = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventyFirstDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(seventieth >= 0 && seventyFirst > seventieth && defs > seventyFirst);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F69E3_adds_CTavernGameDef()
    {
        Assert.Equal(0x004F69DCu, EngineLifecycle.SeventySecondDefClassSite);
        Assert.Equal(0x004E2D3Bu, EngineLifecycle.SeventySecondDefClassFactory);
        Assert.Equal(0x004E1049u, EngineLifecycle.SeventySecondDefClassCtor);
        Assert.Equal(0x012424BCu, EngineLifecycle.SeventySecondDefClassVtbl);
        Assert.Equal(420, EngineLifecycle.SeventySecondDefClassSize);
        Assert.Equal("CTavernGameDef", EngineLifecycle.SeventySecondDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SeventyFirstDefClassRegistered);
        Assert.True(life.SeventySecondDefClassRegistered);
        Assert.Equal("CTavernGameDef", life.SeventySecondDefClass);
        var seventyFirst = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventyFirstDefClassSite);
        var seventySecond = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventySecondDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(seventyFirst >= 0 && seventySecond > seventyFirst && defs > seventySecond);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F6A2E_adds_CTavernGameCardBaseDef()
    {
        Assert.Equal(0x004F6A27u, EngineLifecycle.SeventyThirdDefClassSite);
        Assert.Equal(0x004E2DB2u, EngineLifecycle.SeventyThirdDefClassFactory);
        Assert.Equal(0x004E1195u, EngineLifecycle.SeventyThirdDefClassCtor);
        Assert.Equal(0x0124258Cu, EngineLifecycle.SeventyThirdDefClassVtbl);
        Assert.Equal(132, EngineLifecycle.SeventyThirdDefClassSize);
        Assert.Equal("CTavernGameCardBaseDef", EngineLifecycle.SeventyThirdDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SeventySecondDefClassRegistered);
        Assert.True(life.SeventyThirdDefClassRegistered);
        Assert.Equal("CTavernGameCardBaseDef", life.SeventyThirdDefClass);
        var seventySecond = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventySecondDefClassSite);
        var seventyThird = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventyThirdDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(seventySecond >= 0 && seventyThird > seventySecond && defs > seventyThird);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F6A79_adds_CTavernGameCoinBaseDef()
    {
        Assert.Equal(0x004F6A72u, EngineLifecycle.SeventyFourthDefClassSite);
        Assert.Equal(0x004D8F51u, EngineLifecycle.SeventyFourthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.SeventyFourthDefClassCtor);
        Assert.Equal(0x0123D44Cu, EngineLifecycle.SeventyFourthDefClassVtbl);
        Assert.Equal(68, EngineLifecycle.SeventyFourthDefClassSize);
        Assert.Equal("CTavernGameCoinBaseDef", EngineLifecycle.SeventyFourthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SeventyThirdDefClassRegistered);
        Assert.True(life.SeventyFourthDefClassRegistered);
        Assert.Equal("CTavernGameCoinBaseDef", life.SeventyFourthDefClass);
        var seventyThird = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventyThirdDefClassSite);
        var seventyFourth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventyFourthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(seventyThird >= 0 && seventyFourth > seventyThird && defs > seventyFourth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(life.Runtime.Quests, q => q.Name == "Q_NewOakValeIntro");
    }

    [Fact]
    public void Init_Thing_Components_004F6B2F_adds_CTavernGameShoveHaPennyDef()
    {
        Assert.Equal(0x004F6B28u, EngineLifecycle.SeventyFifthDefClassSite);
        Assert.Equal(0x004E2D70u, EngineLifecycle.SeventyFifthDefClassFactory);
        Assert.Equal(0x004E1105u, EngineLifecycle.SeventyFifthDefClassCtor);
        Assert.Equal(0x01242524u, EngineLifecycle.SeventyFifthDefClassVtbl);
        Assert.Equal(512, EngineLifecycle.SeventyFifthDefClassSize);
        Assert.Equal("CTavernGameShoveHaPennyDef", EngineLifecycle.SeventyFifthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SeventyFourthDefClassRegistered);
        Assert.True(life.SeventyFifthDefClassRegistered);
        Assert.Equal("CTavernGameShoveHaPennyDef", life.SeventyFifthDefClass);
        var seventyFourth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventyFourthDefClassSite);
        var seventyFifth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventyFifthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(seventyFourth >= 0 && seventyFifth > seventyFourth && defs > seventyFifth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F6BE5_adds_CTavernGameCoinGolfDef()
    {
        Assert.Equal(0x004F6BDEu, EngineLifecycle.SeventySixthDefClassSite);
        Assert.Equal(0x004D8F97u, EngineLifecycle.SeventySixthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.SeventySixthDefClassCtor);
        Assert.Equal(0x0123D4D4u, EngineLifecycle.SeventySixthDefClassVtbl);
        Assert.Equal(92, EngineLifecycle.SeventySixthDefClassSize);
        Assert.Equal("CTavernGameCoinGolfDef", EngineLifecycle.SeventySixthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SeventyFifthDefClassRegistered);
        Assert.True(life.SeventySixthDefClassRegistered);
        Assert.Equal("CTavernGameCoinGolfDef", life.SeventySixthDefClass);
        var seventyFifth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventyFifthDefClassSite);
        var seventySixth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventySixthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(seventyFifth >= 0 && seventySixth > seventyFifth && defs > seventySixth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F6D71_adds_CTavernGameSpotTheAdditionDef()
    {
        Assert.Equal(0x004F6D6Au, EngineLifecycle.SeventySeventhDefClassSite);
        Assert.Equal(0x004E11C3u, EngineLifecycle.SeventySeventhDefClassFactory);
        Assert.Equal(0x004DED22u, EngineLifecycle.SeventySeventhDefClassCtor);
        Assert.Equal(0x01241A4Cu, EngineLifecycle.SeventySeventhDefClassVtbl);
        Assert.Equal(144, EngineLifecycle.SeventySeventhDefClassSize);
        Assert.Equal("CTavernGameSpotTheAdditionDef", EngineLifecycle.SeventySeventhDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SeventySixthDefClassRegistered);
        Assert.True(life.SeventySeventhDefClassRegistered);
        Assert.Equal("CTavernGameSpotTheAdditionDef", life.SeventySeventhDefClass);
        var seventySixth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventySixthDefClassSite);
        var seventySeventh = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventySeventhDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(seventySixth >= 0 && seventySeventh > seventySixth && defs > seventySeventh);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F6EFD_adds_CDecapitationDef()
    {
        Assert.Equal(0x004F6EF6u, EngineLifecycle.SeventyEighthDefClassSite);
        Assert.Equal(0x004D9047u, EngineLifecycle.SeventyEighthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.SeventyEighthDefClassCtor);
        Assert.Equal(0x0123D5E4u, EngineLifecycle.SeventyEighthDefClassVtbl);
        Assert.Equal(48, EngineLifecycle.SeventyEighthDefClassSize);
        Assert.Equal("CDecapitationDef", EngineLifecycle.SeventyEighthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SeventySeventhDefClassRegistered);
        Assert.True(life.SeventyEighthDefClassRegistered);
        Assert.Equal("CDecapitationDef", life.SeventyEighthDefClass);
        var seventySeventh = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventySeventhDefClassSite);
        var seventyEighth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventyEighthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(seventySeventh >= 0 && seventyEighth > seventySeventh && defs > seventyEighth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F6FB3_adds_CCoinGameObstacleDef()
    {
        Assert.Equal(0x004F6FACu, EngineLifecycle.SeventyNinthDefClassSite);
        Assert.Equal(0x004D8F0Bu, EngineLifecycle.SeventyNinthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.SeventyNinthDefClassCtor);
        Assert.Equal(0x0123D3E4u, EngineLifecycle.SeventyNinthDefClassVtbl);
        Assert.Equal(44, EngineLifecycle.SeventyNinthDefClassSize);
        Assert.Equal("CCoinGameObstacleDef", EngineLifecycle.SeventyNinthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SeventyEighthDefClassRegistered);
        Assert.True(life.SeventyNinthDefClassRegistered);
        Assert.Equal("CCoinGameObstacleDef", life.SeventyNinthDefClass);
        var seventyEighth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventyEighthDefClassSite);
        var seventyNinth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventyNinthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(seventyEighth >= 0 && seventyNinth > seventyEighth && defs > seventyNinth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F71F1_adds_CWallMountEffectsDef()
    {
        Assert.Equal(0x004F71EAu, EngineLifecycle.EightiethDefClassSite);
        Assert.Equal(0x004D90C6u, EngineLifecycle.EightiethDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.EightiethDefClassCtor);
        Assert.Equal(0x0123D74Cu, EngineLifecycle.EightiethDefClassVtbl);
        Assert.Equal(52, EngineLifecycle.EightiethDefClassSize);
        Assert.Equal("CWallMountEffectsDef", EngineLifecycle.EightiethDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.SeventyNinthDefClassRegistered);
        Assert.True(life.EightiethDefClassRegistered);
        Assert.Equal("CWallMountEffectsDef", life.EightiethDefClass);
        var seventyNinth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.SeventyNinthDefClassSite);
        var eightieth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EightiethDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(seventyNinth >= 0 && eightieth > seventyNinth && defs > eightieth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F729B_adds_CFishDef()
    {
        Assert.Equal(0x004F7294u, EngineLifecycle.EightyFirstDefClassSite);
        Assert.Equal(0x004D910Cu, EngineLifecycle.EightyFirstDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.EightyFirstDefClassCtor);
        Assert.Equal(0x0123D7BCu, EngineLifecycle.EightyFirstDefClassVtbl);
        Assert.Equal(88, EngineLifecycle.EightyFirstDefClassSize);
        Assert.Equal("CFishDef", EngineLifecycle.EightyFirstDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.EightiethDefClassRegistered);
        Assert.True(life.EightyFirstDefClassRegistered);
        Assert.Equal("CFishDef", life.EightyFirstDefClass);
        var eightieth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EightiethDefClassSite);
        var eightyFirst = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EightyFirstDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(eightieth >= 0 && eightyFirst > eightieth && defs > eightyFirst);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F7345_adds_CTeleporterDef()
    {
        Assert.Equal(0x004F733Eu, EngineLifecycle.EightySecondDefClassSite);
        Assert.Equal(0x004D9152u, EngineLifecycle.EightySecondDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.EightySecondDefClassCtor);
        Assert.Equal(0x0123D834u, EngineLifecycle.EightySecondDefClassVtbl);
        Assert.Equal(52, EngineLifecycle.EightySecondDefClassSize);
        Assert.Equal("CTeleporterDef", EngineLifecycle.EightySecondDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.EightyFirstDefClassRegistered);
        Assert.True(life.EightySecondDefClassRegistered);
        Assert.Equal("CTeleporterDef", life.EightySecondDefClass);
        var eightyFirst = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EightyFirstDefClassSite);
        var eightySecond = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EightySecondDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(eightyFirst >= 0 && eightySecond > eightyFirst && defs > eightySecond);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(life.Runtime.Quests, q => q.Name == "Q_NewOakValeIntro");
    }

    [Fact]
    public void Init_Thing_Components_004F744A_adds_CExplosionDef()
    {
        Assert.Equal(0x004F7443u, EngineLifecycle.EightyThirdDefClassSite);
        Assert.Equal(0x004E3096u, EngineLifecycle.EightyThirdDefClassFactory);
        Assert.Equal(0x004E1341u, EngineLifecycle.EightyThirdDefClassCtor);
        Assert.Equal(0x012425F4u, EngineLifecycle.EightyThirdDefClassVtbl);
        Assert.Equal(112, EngineLifecycle.EightyThirdDefClassSize);
        Assert.Equal("CExplosionDef", EngineLifecycle.EightyThirdDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.EightySecondDefClassRegistered);
        Assert.True(life.EightyThirdDefClassRegistered);
        Assert.Equal("CExplosionDef", life.EightyThirdDefClass);
        var eightySecond = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EightySecondDefClassSite);
        var eightyThird = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EightyThirdDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(eightySecond >= 0 && eightyThird > eightySecond && defs > eightyThird);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F7611_adds_CResurrectionItemDef()
    {
        Assert.Equal(0x004F760Au, EngineLifecycle.EightyFourthDefClassSite);
        Assert.Equal(0x004D91DEu, EngineLifecycle.EightyFourthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.EightyFourthDefClassCtor);
        Assert.Equal(0x0123D9ACu, EngineLifecycle.EightyFourthDefClassVtbl);
        Assert.Equal(44, EngineLifecycle.EightyFourthDefClassSize);
        Assert.Equal("CResurrectionItemDef", EngineLifecycle.EightyFourthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.EightyThirdDefClassRegistered);
        Assert.True(life.EightyFourthDefClassRegistered);
        Assert.Equal("CResurrectionItemDef", life.EightyFourthDefClass);
        var eightyThird = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EightyThirdDefClassSite);
        var eightyFourth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EightyFourthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(eightyThird >= 0 && eightyFourth > eightyThird && defs > eightyFourth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F76BB_adds_CKrakenDef()
    {
        Assert.Equal(0x004F76B4u, EngineLifecycle.EightyFifthDefClassSite);
        Assert.Equal(0x004E13ADu, EngineLifecycle.EightyFifthDefClassFactory);
        Assert.Equal(0x004DEF86u, EngineLifecycle.EightyFifthDefClassCtor);
        Assert.Equal(0x01241B2Cu, EngineLifecycle.EightyFifthDefClassVtbl);
        Assert.Equal(124, EngineLifecycle.EightyFifthDefClassSize);
        Assert.Equal("CKrakenDef", EngineLifecycle.EightyFifthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.EightyFourthDefClassRegistered);
        Assert.True(life.EightyFifthDefClassRegistered);
        Assert.Equal("CKrakenDef", life.EightyFifthDefClass);
        var eightyFourth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EightyFourthDefClassSite);
        var eightyFifth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EightyFifthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(eightyFourth >= 0 && eightyFifth > eightyFourth && defs > eightyFifth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F7765_adds_CKrakenTentacleDef()
    {
        Assert.Equal(0x004F775Eu, EngineLifecycle.EightySixthDefClassSite);
        Assert.Equal(0x004D9224u, EngineLifecycle.EightySixthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.EightySixthDefClassCtor);
        Assert.Equal(0x0123DA24u, EngineLifecycle.EightySixthDefClassVtbl);
        Assert.Equal(96, EngineLifecycle.EightySixthDefClassSize);
        Assert.Equal("CKrakenTentacleDef", EngineLifecycle.EightySixthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.EightyFifthDefClassRegistered);
        Assert.True(life.EightySixthDefClassRegistered);
        Assert.Equal("CKrakenTentacleDef", life.EightySixthDefClass);
        var eightyFifth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EightyFifthDefClassSite);
        var eightySixth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EightySixthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(eightyFifth >= 0 && eightySixth > eightyFifth && defs > eightySixth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F780F_adds_CHeroSpecialMovementDef()
    {
        Assert.Equal(0x004F7808u, EngineLifecycle.EightySeventhDefClassSite);
        Assert.Equal(0x004D9198u, EngineLifecycle.EightySeventhDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.EightySeventhDefClassCtor);
        Assert.Equal(0x0123D92Cu, EngineLifecycle.EightySeventhDefClassVtbl);
        Assert.Equal(56, EngineLifecycle.EightySeventhDefClassSize);
        Assert.Equal("CHeroSpecialMovementDef", EngineLifecycle.EightySeventhDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.EightySixthDefClassRegistered);
        Assert.True(life.EightySeventhDefClassRegistered);
        Assert.Equal("CHeroSpecialMovementDef", life.EightySeventhDefClass);
        var eightySixth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EightySixthDefClassSite);
        var eightySeventh = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EightySeventhDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(eightySixth >= 0 && eightySeventh > eightySixth && defs > eightySeventh);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F7918_adds_CIdleSchedulerDef()
    {
        Assert.Equal(0x004F7911u, EngineLifecycle.EightyEighthDefClassSite);
        Assert.Equal(0x004E6232u, EngineLifecycle.EightyEighthDefClassFactory);
        Assert.Equal(0x004E3F21u, EngineLifecycle.EightyEighthDefClassCtor);
        Assert.Equal(0x01243124u, EngineLifecycle.EightyEighthDefClassVtbl);
        Assert.Equal(72, EngineLifecycle.EightyEighthDefClassSize);
        Assert.Equal("CIdleSchedulerDef", EngineLifecycle.EightyEighthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.EightySeventhDefClassRegistered);
        Assert.True(life.EightyEighthDefClassRegistered);
        Assert.Equal("CIdleSchedulerDef", life.EightyEighthDefClass);
        var eightySeventh = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EightySeventhDefClassSite);
        var eightyEighth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EightyEighthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(eightySeventh >= 0 && eightyEighth > eightySeventh && defs > eightyEighth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F79C2_adds_CCarriedReadableDef()
    {
        Assert.Equal(0x004F79BBu, EngineLifecycle.EightyNinthDefClassSite);
        Assert.Equal(0x004D92B0u, EngineLifecycle.EightyNinthDefClassFactory);
        Assert.Equal(0x004D5ECAu, EngineLifecycle.EightyNinthDefClassCtor);
        Assert.Equal(0x0123DB94u, EngineLifecycle.EightyNinthDefClassVtbl);
        Assert.Equal(48, EngineLifecycle.EightyNinthDefClassSize);
        Assert.Equal("CCarriedReadableDef", EngineLifecycle.EightyNinthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.EightyEighthDefClassRegistered);
        Assert.True(life.EightyNinthDefClassRegistered);
        Assert.Equal("CCarriedReadableDef", life.EightyNinthDefClass);
        var eightyEighth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EightyEighthDefClassSite);
        var eightyNinth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EightyNinthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(eightyEighth >= 0 && eightyNinth > eightyEighth && defs > eightyNinth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F7A6C_adds_CJackOfBladesBattleDef()
    {
        Assert.Equal(0x004F7A65u, EngineLifecycle.NinetiethDefClassSite);
        Assert.Equal(0x004E4748u, EngineLifecycle.NinetiethDefClassFactory);
        Assert.Equal(0x00430370u, EngineLifecycle.NinetiethDefClassCtor);
        Assert.Equal(0x01242E3Cu, EngineLifecycle.NinetiethDefClassVtbl);
        Assert.Equal(128, EngineLifecycle.NinetiethDefClassSize);
        Assert.Equal("CJackOfBladesBattleDef", EngineLifecycle.NinetiethDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.EightyNinthDefClassRegistered);
        Assert.True(life.NinetiethDefClassRegistered);
        Assert.Equal("CJackOfBladesBattleDef", life.NinetiethDefClass);
        var eightyNinth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.EightyNinthDefClassSite);
        var ninetieth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.NinetiethDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(eightyNinth >= 0 && ninetieth > eightyNinth && defs > ninetieth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F7AB7_to_004F7C79_adds_battle_cluster()
    {
        Assert.Equal("CScorpionKingBattleDef", EngineLifecycle.NinetyFirstDefClassName);
        Assert.Equal(0x004E47BFu, EngineLifecycle.NinetyFirstDefClassFactory);
        Assert.Equal(0x01242EA4u, EngineLifecycle.NinetyFirstDefClassVtbl);
        Assert.Equal(96, EngineLifecycle.NinetyFirstDefClassSize);
        Assert.Equal("CThunderBattleDef", EngineLifecycle.NinetySecondDefClassName);
        Assert.Equal(76, EngineLifecycle.NinetySecondDefClassSize);
        Assert.Equal("CWhisperBattleDef", EngineLifecycle.NinetyThirdDefClassName);
        Assert.Equal(68, EngineLifecycle.NinetyThirdDefClassSize);
        Assert.Equal("CWaspQueenBattleDef", EngineLifecycle.NinetyFourthDefClassName);
        Assert.Equal(64, EngineLifecycle.NinetyFourthDefClassSize);
        Assert.Equal("CMazeBattleDef", EngineLifecycle.NinetyFifthDefClassName);
        Assert.Equal(0x004E45CEu, EngineLifecycle.NinetyFifthDefClassFactory);
        Assert.Equal(96, EngineLifecycle.NinetyFifthDefClassSize);
        Assert.Equal("CTrollBattleDef", EngineLifecycle.NinetySixthDefClassName);
        Assert.Equal(96, EngineLifecycle.NinetySixthDefClassSize);
        Assert.Equal("CBalverineBattleDef", EngineLifecycle.NinetySeventhDefClassName);
        Assert.Equal(0x004E4883u, EngineLifecycle.NinetySeventhDefClassFactory);
        Assert.Equal(72, EngineLifecycle.NinetySeventhDefClassSize);
        Assert.Equal(0x00430370u, EngineLifecycle.NinetyFirstDefClassCtor);
        Assert.Equal(EngineLifecycle.NinetyFirstDefClassCtor, EngineLifecycle.NinetySeventhDefClassCtor);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.NinetiethDefClassRegistered);
        Assert.True(life.NinetyFirstDefClassRegistered);
        Assert.True(life.NinetySeventhDefClassRegistered);
        Assert.Equal("CScorpionKingBattleDef", life.NinetyFirstDefClass);
        Assert.Equal("CMazeBattleDef", life.NinetyFifthDefClass);
        Assert.Equal("CBalverineBattleDef", life.NinetySeventhDefClass);
        var jack = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.NinetiethDefClassSite);
        var scorpion = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.NinetyFirstDefClassSite);
        var maze = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.NinetyFifthDefClassSite);
        var balverine = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.NinetySeventhDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(jack >= 0 && scorpion > jack && maze > scorpion &&
            balverine > maze && defs > balverine);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(life.Runtime.Quests, q => q.Name == "Q_NewOakValeIntro");
    }

    [Fact]
    public void Init_Thing_Components_004F7D1C_adds_CAreaOfEffectAttackDef()
    {
        Assert.Equal(0x004F7D1Cu, EngineLifecycle.NinetyEighthDefClassSite);
        Assert.Equal(0x004E6CF3u, EngineLifecycle.NinetyEighthDefClassFactory);
        Assert.Equal(0x00430370u, EngineLifecycle.NinetyEighthDefClassCtor);
        Assert.Equal(0x0124318Cu, EngineLifecycle.NinetyEighthDefClassVtbl);
        Assert.Equal(76, EngineLifecycle.NinetyEighthDefClassSize);
        Assert.Equal("CAreaOfEffectAttackDef", EngineLifecycle.NinetyEighthDefClassName);
        Assert.Equal(EngineLifecycle.NinetySeventhDefClassCtor, EngineLifecycle.NinetyEighthDefClassCtor);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.NinetySeventhDefClassRegistered);
        Assert.True(life.NinetyEighthDefClassRegistered);
        Assert.Equal("CBalverineBattleDef", life.NinetySeventhDefClass);
        Assert.Equal("CAreaOfEffectAttackDef", life.NinetyEighthDefClass);
        var balverine = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.NinetySeventhDefClassSite);
        var aoe = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.NinetyEighthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(balverine >= 0 && aoe > balverine && defs > aoe);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(life.Runtime.Quests, q => q.Name == "Q_NewOakValeIntro");
    }

    [Fact]
    public void Init_Thing_Components_004F7DCD_adds_CFishingRodDef()
    {
        Assert.Equal(0x004F7DC6u, EngineLifecycle.NinetyNinthDefClassSite);
        Assert.Equal(0x004D9321u, EngineLifecycle.NinetyNinthDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.NinetyNinthDefClassCtor);
        Assert.Equal(0x0123DCA4u, EngineLifecycle.NinetyNinthDefClassVtbl);
        Assert.Equal(60, EngineLifecycle.NinetyNinthDefClassSize);
        Assert.Equal("CFishingRodDef", EngineLifecycle.NinetyNinthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.NinetyEighthDefClassRegistered);
        Assert.True(life.NinetyNinthDefClassRegistered);
        Assert.Equal("CFishingRodDef", life.NinetyNinthDefClass);
        var ninetyEighth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.NinetyEighthDefClassSite);
        var ninetyNinth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.NinetyNinthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(ninetyEighth >= 0 && ninetyNinth > ninetyEighth && defs > ninetyNinth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Init_Thing_Components_004F7F31_adds_CRumbleDef()
    {
        Assert.Equal(0x004F7F2Au, EngineLifecycle.HundredthDefClassSite);
        Assert.Equal(0x004E3290u, EngineLifecycle.HundredthDefClassFactory);
        Assert.Equal(0x004E1722u, EngineLifecycle.HundredthDefClassCtor);
        Assert.Equal(0x0124273Cu, EngineLifecycle.HundredthDefClassVtbl);
        Assert.Equal(64, EngineLifecycle.HundredthDefClassSize);
        Assert.Equal("CRumbleDef", EngineLifecycle.HundredthDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.NinetyNinthDefClassRegistered);
        Assert.True(life.HundredthDefClassRegistered);
        Assert.Equal("CRumbleDef", life.HundredthDefClass);
        var ninetyNinth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.NinetyNinthDefClassSite);
        var hundredth = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.HundredthDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(ninetyNinth >= 0 && hundredth > ninetyNinth && defs > hundredth);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(life.Runtime.Quests, q => q.Name == "Q_NewOakValeIntro");
    }

    [Fact]
    public void Init_Thing_Components_004F8211_to_004F82A7_adds_ship_shop_atmos()
    {
        Assert.Equal(0x004F820Au, EngineLifecycle.HundredFirstDefClassSite);
        Assert.Equal(0x004D8799u, EngineLifecycle.HundredFirstDefClassFactory);
        Assert.Equal(0x0044C0C0u, EngineLifecycle.HundredFirstDefClassCtor);
        Assert.Equal(0x0123C0A4u, EngineLifecycle.HundredFirstDefClassVtbl);
        Assert.Equal(68, EngineLifecycle.HundredFirstDefClassSize);
        Assert.Equal("CShipDef", EngineLifecycle.HundredFirstDefClassName);
        Assert.Equal(0x004F8255u, EngineLifecycle.HundredSecondDefClassSite);
        Assert.Equal(0x004D8411u, EngineLifecycle.HundredSecondDefClassFactory);
        Assert.Equal(0x004D405Au, EngineLifecycle.HundredSecondDefClassCtor);
        Assert.Equal(0x0123B644u, EngineLifecycle.HundredSecondDefClassVtbl);
        Assert.Equal(72, EngineLifecycle.HundredSecondDefClassSize);
        Assert.Equal("CShopItemDef", EngineLifecycle.HundredSecondDefClassName);
        Assert.Equal(0x004F82A0u, EngineLifecycle.HundredThirdDefClassSite);
        Assert.Equal(0x004E32E3u, EngineLifecycle.HundredThirdDefClassFactory);
        Assert.Equal(0x004E1748u, EngineLifecycle.HundredThirdDefClassCtor);
        Assert.Equal(0x012427A4u, EngineLifecycle.HundredThirdDefClassVtbl);
        Assert.Equal(52, EngineLifecycle.HundredThirdDefClassSize);
        Assert.Equal("CSoundAtmospheresDef", EngineLifecycle.HundredThirdDefClassName);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.HundredthDefClassRegistered);
        Assert.True(life.HundredFirstDefClassRegistered);
        Assert.True(life.HundredSecondDefClassRegistered);
        Assert.True(life.HundredThirdDefClassRegistered);
        Assert.Equal("CShipDef", life.HundredFirstDefClass);
        Assert.Equal("CShopItemDef", life.HundredSecondDefClass);
        Assert.Equal("CSoundAtmospheresDef", life.HundredThirdDefClass);
        var rumble = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.HundredthDefClassSite);
        var ship = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.HundredFirstDefClassSite);
        var shop = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.HundredSecondDefClassSite);
        var atmos = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.HundredThirdDefClassSite);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(rumble >= 0 && ship > rumble && shop > ship &&
            atmos > shop && defs > atmos);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(life.Runtime.Quests, q => q.Name == "Q_NewOakValeIntro");
    }

    [Fact]
    public void Init_Thing_Components_004F8427_to_004F8E90_finishes_004EE23F()
    {
        Assert.Equal("CNymphDef", EngineLifecycle.HundredFourthDefClassName);
        Assert.Equal(80, EngineLifecycle.HundredFourthDefClassSize);
        Assert.Equal(0x0123DE3Cu, EngineLifecycle.HundredFourthDefClassVtbl);
        Assert.Equal("CSummonDef", EngineLifecycle.HundredFifthDefClassName);
        Assert.Equal(76, EngineLifecycle.HundredFifthDefClassSize);
        Assert.Equal("CCameraCollisionDef", EngineLifecycle.HundredSixthDefClassName);
        Assert.Equal(44, EngineLifecycle.HundredSixthDefClassSize);
        Assert.Equal(0x004D9465u, EngineLifecycle.HundredSixthDefClassFactory);
        Assert.Equal("CBettingDef", EngineLifecycle.HundredSeventhDefClassName);
        Assert.Equal(88, EngineLifecycle.HundredSeventhDefClassSize);
        Assert.Equal("COracleMinigameDef", EngineLifecycle.HundredEighthDefClassName);
        Assert.Equal(92, EngineLifecycle.HundredEighthDefClassSize);
        Assert.Equal("CFireheartMinigameDef", EngineLifecycle.HundredNinthDefClassName);
        Assert.Equal(60, EngineLifecycle.HundredNinthDefClassSize);
        Assert.Equal(0x004D6638u, EngineLifecycle.HundredNinthDefClassCtor);
        Assert.Equal("CLightningOrbDef", EngineLifecycle.HundredTenthDefClassName);
        Assert.Equal(60, EngineLifecycle.HundredTenthDefClassSize);
        Assert.Equal(0x0123E5ECu, EngineLifecycle.HundredTenthDefClassVtbl);
        Assert.Equal("CHasNameDef", EngineLifecycle.HundredEleventhDefClassName);
        Assert.Equal(52, EngineLifecycle.HundredEleventhDefClassSize);
        Assert.Equal(0x004F8E89u, EngineLifecycle.HundredEleventhDefClassSite);
        Assert.Equal(0x004D98C8u, EngineLifecycle.HundredEleventhDefClassFactory);
        Assert.Equal(0x0123E67Cu, EngineLifecycle.HundredEleventhDefClassVtbl);
        Assert.Equal(0x004F9144u, EngineLifecycle.ThingComponentsRet);
        Assert.Equal(0x004F9129u, EngineLifecycle.ThingComponentsFillSite);
        Assert.Equal(0x0073B130u, EngineLifecycle.ThingComponentsFillFn);
        Assert.Equal(0x0073CB40u, EngineLifecycle.ThingComponentsFillRet);
        Assert.Equal(0x013BAD4Cu, EngineLifecycle.ThingComponentsFillLimitVa);
        Assert.Equal(0x00743270u, EngineLifecycle.ThingComponentsFillGrowFn);
        Assert.Equal(0x00743B30u, EngineLifecycle.ThingComponentsFillSecondFn);
        Assert.Equal(0x007441D0u, EngineLifecycle.ThingComponentsFillCommitFn);
        Assert.Equal(0x00742430u, EngineLifecycle.ThingComponentsFillFirstThunk);
        Assert.Equal(0, EngineLifecycle.ThingComponentsFillFirstTag);
        Assert.Equal(0x0073EAC0u, EngineLifecycle.ThingComponentsFillFirstCtor);
        Assert.Equal(0x01267588u, EngineLifecycle.ThingComponentsFillFirstVtbl);
        Assert.Equal(28, EngineLifecycle.ThingComponentsFillFirstSize);
        Assert.Equal(6, EngineLifecycle.ThingComponentsTailCtcCount);
        Assert.Equal(0x004F8ECEu, EngineLifecycle.ThingComponentsTailCtcFirstSite);
        Assert.Equal(0x004D66DAu, EngineLifecycle.ThingComponentsTailCtcFirstFactory);
        Assert.Equal(0x004F90E1u, EngineLifecycle.ThingComponentsTailCtcLastSite);
        Assert.Equal(0x004DAF85u, EngineLifecycle.ThingComponentsTailCtcLastFactory);
        Assert.False(EngineLifecycle.ThingComponentsFillIsQuestActivate);
        Assert.Equal(80, RegionTravel.AttackOverPlus80);
        Assert.Equal((byte)1, RegionTravel.AttackOverStoredValue);
        Assert.False(RegionTravel.FirstSeenAttackOverStoreRuns);
        Assert.Equal(0x004F9112u, EngineLifecycle.ThingComponentsCommitFlagSetSite);
        Assert.Equal(0x004F9139u, EngineLifecycle.ThingComponentsCommitSite);
        Assert.Equal(0x004EBACEu, EngineLifecycle.ThingComponentsCommitFn);
        Assert.Equal(0x004EB9A6u, EngineLifecycle.ThingComponentsCommitApplyFn);
        Assert.Equal(12, EngineLifecycle.ThingComponentsCommitPlus12);
        Assert.Equal(13, EngineLifecycle.ThingComponentsCommitPlus13);
        Assert.True(EngineLifecycle.ThingComponentsCommitRunsThisWalk);
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.HundredThirdDefClassRegistered);
        Assert.True(life.HundredFourthDefClassRegistered);
        Assert.True(life.HundredSixthDefClassRegistered);
        Assert.True(life.HundredEleventhDefClassRegistered);
        Assert.True(life.ThingComponentsFilled);
        Assert.True(life.ThingComponentsCommitted);
        Assert.Equal("CNymphDef", life.HundredFourthDefClass);
        Assert.Equal("CCameraCollisionDef", life.HundredSixthDefClass);
        Assert.Equal("CHasNameDef", life.HundredEleventhDefClass);
        var atmos = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.HundredThirdDefClassSite);
        var nymph = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.HundredFourthDefClassSite);
        var hasName = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.HundredEleventhDefClassSite);
        var fill = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThingComponentsFillSite);
        var commit = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThingComponentsCommitSite);
        var ret = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.ThingComponentsRet);
        var defs = life.Trace.Events.FindIndex(e =>
            e.Action == "Init Definition Manager");
        Assert.True(atmos >= 0 && nymph > atmos && hasName > nymph &&
            fill > hasName && commit > fill && ret > commit && defs > ret);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(life.Runtime.Quests, q => q.Name == "Q_NewOakValeIntro");
        Assert.True(EngineLifecycle.GameflowWaitsForeverOnNoSave);
        Assert.False(EngineLifecycle.ActivateQuestSatisfiesGameflowWait);
    }

    [Fact]
    public void Init_Definition_Manager_00416005_resets_plus88_via_vtbl8()
    {
        Assert.Equal(0x00416005u, EngineLifecycle.InitDefinitionManagerFn);
        Assert.Equal(0x0044C72Bu, EngineLifecycle.DefinitionManagerVtbl8Fn);
        Assert.Equal(8, EngineLifecycle.DefinitionManagerVtbl8);
        Assert.Equal(0x009ACB10u, EngineLifecycle.DefinitionManagerResetFn);
        Assert.Equal(0x009E5250u, EngineLifecycle.DefinitionManagerResetApply);
        Assert.Equal(88, EngineLifecycle.DefinitionManagerPlus88);
        Assert.Equal(1, EngineLifecycle.DefinitionManagerArg);
        Assert.Equal(0x009B08C0u, EngineLifecycle.DefinitionManagerCompileFn);
        Assert.Equal("pc\\", EngineLifecycle.DefinitionManagerPathPrefix);
        Assert.Equal("*.h", EngineLifecycle.DefinitionManagerFirstGlob);
        Assert.Equal("CHeroPostcardGeneratorDef", EngineLifecycle.DefinitionManagerCompileFirstClass);
        Assert.Equal(0x01232C24u, EngineLifecycle.PlayerManagerVtbl);
        var map = AssemblyTextMap.TryLocate();
        Assert.NotNull(map);
        Assert.Equal(
            EngineLifecycle.DefinitionManagerVtbl8Fn,
            map.VtblDest(EngineLifecycle.PlayerManagerVtbl, 2));
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.DefinitionManagerPrepared);
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.True(life.FirstDefClassRegistered);
        Assert.True(life.DefinitionManagerPrepared);
        var add = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.AddDefClassFn);
        var getter = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.PlayerManagerGetter &&
            e.Stage == "Init Definition Manager");
        var vtbl8 = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.DefinitionManagerVtbl8Fn);
        var compile = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.DefinitionManagerCompileFn);
        var reset = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.DefinitionManagerResetFn);
        var apply = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.DefinitionManagerResetApply);
        var graphics = life.Trace.Events.FindIndex(e =>
            e.Va == EngineLifecycle.InitGraphicsFn);
        Assert.True(add >= 0 && getter > add);
        Assert.True(vtbl8 > getter && compile > vtbl8 && reset > compile && apply > reset);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.AddDefClassFn &&
            e.Stage == "Init Definition Manager" &&
            e.Action.Contains("CHeroPostcardGeneratorDef", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DefinitionManagerCompileFn &&
            e.Action.Contains("parse", StringComparison.OrdinalIgnoreCase));
        Assert.True(graphics > apply, "00416005 before Init Graphics");
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Frontend_press_start_type4_without_widgets_does_not_invent_0xE5()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.Equal(4, EngineInput.Type4);
        Assert.Equal(26, EngineInput.ActionType4);
        Assert.Equal(3, EngineInput.Type4Device);
        Assert.Equal(6, EngineInput.Type6);
        Assert.Equal(28, EngineInput.ActionType6);
        Assert.Equal(7, EngineInput.Type7);
        life.QueueInput(EngineInput.Type4, 0);
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
    public void Frontend_type4_posts_stored_0xE5_then_0x126_then_15()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.QueueInput(EngineInput.Type4, 0);
        life.QueueInput(EngineInput.Type6, 0);
        Assert.True(life.Pump());
        Assert.Equal(
            EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        Assert.Equal("Default", life.FrontendEditBoxName);
        Assert.False(life.RetailNewGameFlag);
        Assert.True(FrontendWidgetType.DrawsChildList(life.FrontendRootType));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendContainerDrawFn &&
            e.Action.Contains("00530260", StringComparison.Ordinal));
        life.QueueInput(EngineInput.TypeKey, RegionTravel.PlayAviSkipReturn);
        Assert.True(life.Pump());
        Assert.Equal(
            EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        Assert.False(life.RetailNewGameFlag);
        ClickNamed(life, "UI_ACCEPT_NEW_PROFILE");
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
        ClickNamed(life, "UI_FRONTEND_BUTTON_NEW_GAME");
        Assert.True(life.RetailNewGameFlag);
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.Equal("FinalAlbion.wld", life.WorldFileName);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.LeaveFrontendSite);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.InitGameSite);
    }

    [Fact]
    public void Frontend_0041AC20_dest_and_0xE5_new_profile_0x126_main_menu_15()
    {
        Assert.Equal(0x0041AC20u, FrontendLayout.LeftoverFn);
        Assert.Equal(204, FrontendLayout.DestWOffset);
        Assert.Equal(360, FrontendLayout.SizeWOffset);
        Assert.Equal(264, FrontendLayout.DestScaleXOffset);
        Assert.Equal((0f, 0f), FrontendLayout.LeftoverFromGraphic(0, 32f, 32f));
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.True(life.Pump());
        Assert.Equal(
            EngineLifecycle.FrontendPressStartMenu, life.FrontendMenuRoot);
        var pressText = life.FrontendWidgets.First(w => w.Name == "UI_PRESS_START_TEXT");
        Assert.Equal(0, pressText.GraphicId);
        Assert.Equal(0f, pressText.Leftover204);
        Assert.Equal(512f, pressText.DestX0);
        Assert.Equal(384f, pressText.DestY0);
        Assert.Equal(pressText.DestX0, pressText.DestX1);
        Assert.Equal(pressText.DestY0, pressText.DestY1);
        var title = life.FrontendWidgets.First(w => w.Name == "UI_TITLE_01");
        Assert.True(title.GraphicId != 0);
        Assert.True(title.Leftover204 > 0f);
        Assert.True(title.DestX1 > title.DestX0);
        life.QueueInput(EngineInput.Type4, 0);
        life.QueueInput(EngineInput.Type6, 0);
        Assert.True(life.Pump());
        Assert.Equal(
            EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        Assert.Equal("Default", life.FrontendEditBoxName);
        Assert.False(life.RetailNewGameFlag);
        Assert.All(
            life.FrontendWidgets.Where(w => w.Type == FrontendWidgetType.Text),
            w => Assert.Equal(0f, w.Leftover204));
        ClickNamed(life, "UI_ACCEPT_NEW_PROFILE");
        Assert.Equal(
            EngineLifecycle.FrontendMainMenuNoContinue, life.FrontendMenuRoot);
        Assert.False(life.RetailNewGameFlag);
        Assert.Contains(life.FrontendWidgets, w =>
            w.Name == "UI_FRONTEND_BUTTON_NEW_GAME" &&
            w.ActionOnLeftUnclicked == FrontendMessages.NewGame);
        Assert.All(
            life.FrontendWidgets.Where(w =>
                w.GraphicId == 0 && w.Type != FrontendWidgetType.TableType),
            w => Assert.Equal(0f, w.Leftover204));
        ClickNamed(life, "UI_FRONTEND_BUTTON_NEW_GAME");
        Assert.True(life.RetailNewGameFlag);
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.Equal("FinalAlbion.wld", life.WorldFileName);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.LeaveFrontendSite);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
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
        Assert.True(EngineLifecycle.FrontendPresentBodyIsLive);
        Assert.True(EngineLifecycle.DisplayFlushQueueIsNoteOnly);
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
        Assert.Contains(life.Trace.Events, e =>
            e.Va == FrontendTextDraw.Type6RecordFn &&
            e.Action.Contains("0x27", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendWidgetQueueFn &&
            e.Action.Contains("0041BEB0", StringComparison.Ordinal));
        Assert.Equal(0x0041BEB0u, life.Frontend2dLastPacker);
        Assert.Equal(0, life.FrontendWidgetFont);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendPressStartCtorFn &&
            e.Action.Contains("0054E3D0", StringComparison.Ordinal));
        Assert.Equal(EngineLifecycle.FrontendPressStartType, life.FrontendRootType);
        Assert.True(life.FrontendChildCount >= 6);
        Assert.Contains(life.FrontendWidgets, w => w.Name == EngineLifecycle.FrontendPressStartText);
        Assert.Contains(life.FrontendWidgets, w =>
            w.TextValue == EngineLifecycle.FrontendPressStartTextValue);
        Assert.True(life.FrontendDefFound);
        Assert.Equal("UI", life.FrontendDefTypeName);
        Assert.True(life.FrontendType22HandlerRegistered);
        Assert.True(life.FrontendWidgetTickRan);
        Assert.True(life.FrontendDestLayoutRan);
        Assert.True(life.FrontendEnqueueRan);
        Assert.False(life.Frontend2dDipIssued);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayFlush2dFn &&
            e.Action.Contains("0x13BC800", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayFlush2dFn &&
            e.Action.Contains("009D9C80-009DB000", StringComparison.Ordinal));
        Assert.False(life.Frontend2dDipIssued);
        Assert.Equal(16020, EngineLifecycle.DisplayQueueBeginOffset);
        Assert.Equal(332, EngineLifecycle.DrawIndexedPrimitiveVtbl);
        Assert.Equal(0x00A058C0u, EngineLifecycle.DisplayPrimitiveFn);
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayFlushLayersFn &&
            e.Action.Contains("DIP vtbl+", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayFlushLayersFn &&
            e.Action.Contains("empty", StringComparison.Ordinal));
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
        Assert.True(life.Pump(0.5f));
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
        Assert.Equal("TEXT_GUI_MENU_PRESS_BUTTON", text.TextValue);
        Assert.Equal(6, text.Type);
        var forestOne = life.FrontendWidgets
            .Select((widget, index) => (widget, index))
            .Single(pair => pair.widget.Name == "UI_FRONTEND_BG_FORREST_1_1").index;
        var forestTwo = life.FrontendWidgets
            .Select((widget, index) => (widget, index))
            .Single(pair => pair.widget.Name == "UI_FRONTEND_BG_FORREST_2_1").index;
        Assert.True(forestOne >= 0);
        Assert.True(forestTwo >= 0);
        Assert.Equal(0xFFFFFFFFu,
            FrontendWidgetFactory.EffectiveColour(life.FrontendWidgets, forestOne));
        Assert.Equal(0x00FFFFFFu,
            FrontendWidgetFactory.EffectiveColour(life.FrontendWidgets, forestTwo));
        life.SetFrontendPointer(347f, 276f);
        Assert.True(life.Pump());
        Assert.Equal(347f, life.FrontendPointerX);
        Assert.Equal(276f, life.FrontendPointerY);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.FrontendContainerDrawFn &&
            e.Action.Contains("00530260", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == FrontendInputMap.AttachWriteE5 &&
            e.Action.Contains("slot 0x14", StringComparison.Ordinal));
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
        Assert.Contains(life.Trace.Events, e =>
            e.Va == FrontendTextDraw.Type6RecordFn &&
            e.Action.Contains("0x27", StringComparison.Ordinal));
        Assert.Equal(FontFile.UiFace, EngineLifecycle.FrontendUiFontFace);
        Assert.True(life.FrontendEnqueueRan);
        Assert.False(life.Frontend2dDipIssued);
        Assert.Equal(0f, drawn.Leftover204);
        Assert.NotNull(life.FrontendBatch);
        Assert.False(life.FrontendBatch.Value.IsEmpty);
        var frontendBatch = life.FrontendBatch.Value;
        var glyphDraws = frontendBatch.Draws
            .Where(draw => draw.IndexCount == 0 && draw.VertexCount == 6)
            .ToArray();
        Assert.NotEmpty(glyphDraws);
        Assert.All(glyphDraws, draw => Assert.Equal(1f,
            frontendBatch.Vertices[(int)draw.FirstVertex].UseDiffuseColor));
        Assert.Contains(glyphDraws, draw =>
            frontendBatch.Vertices[(int)draw.FirstVertex].Color == new Vector4(0f, 0f, 0f, 1f));
        Assert.Contains(glyphDraws, draw =>
            frontendBatch.Vertices[(int)draw.FirstVertex].Color == Vector4.One);
        var blackDrawIndex = Array.FindIndex(frontendBatch.Draws, draw =>
            draw.IndexCount == 0 && draw.VertexCount == 6 &&
            frontendBatch.Vertices[(int)draw.FirstVertex].Color ==
                new Vector4(0f, 0f, 0f, 1f));
        Assert.True(blackDrawIndex >= 0);
        var blackDraw = frontendBatch.Draws[blackDrawIndex];
        var blackVertex = frontendBatch.Vertices[(int)blackDraw.FirstVertex];
        var colourDrawIndex = Array.FindIndex(
            frontendBatch.Draws, blackDrawIndex + 1, draw =>
                draw.IndexCount == 0 && draw.VertexCount == 6 &&
                draw.TextureId == blackDraw.TextureId &&
                frontendBatch.Vertices[(int)draw.FirstVertex].Uv == blackVertex.Uv &&
                frontendBatch.Vertices[(int)draw.FirstVertex].Color == Vector4.One);
        Assert.True(colourDrawIndex > blackDrawIndex);
        var colourVertex = frontendBatch.Vertices[
            (int)frontendBatch.Draws[colourDrawIndex].FirstVertex];
        Assert.Equal(-FrontendTextDraw.Type6OriginPad * 2f /
            EngineLifecycle.DisplayDefaultWidth,
            colourVertex.Position.X - blackVertex.Position.X, 5);
        Assert.Equal(-FrontendTextDraw.Type6OriginPad * 2f /
            EngineLifecycle.DisplayDefaultHeight,
            colourVertex.Position.Y - blackVertex.Position.Y, 5);

        // UI_MOUSE_POINTER is the final authored child. 00595222 submits it
        // after the two text records rather than globally bucketing glyphs.
        var cursorDraw = frontendBatch.Draws[^1];
        Assert.Equal(6u, cursorDraw.IndexCount);
        Assert.True(Array.FindLastIndex(frontendBatch.Draws,
            draw => draw.IndexCount == 0 && draw.VertexCount == 6) <
            frontendBatch.Draws.Length - 1);
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
    public void Frontend_type18_primes_then_cycles_zero_dwell_forest_states()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();

        Assert.True(life.Pump(1f / 60f));
        var swap = life.FrontendWidgets.First(w => w.Name == "UI_SWAPPING_FORREST");
        Assert.True(swap.SwapTickPrimed);
        Assert.Equal(0, swap.SwapCurrentState);

        Assert.True(life.Pump(1f / 60f));
        swap = life.FrontendWidgets.First(w => w.Name == "UI_SWAPPING_FORREST");
        Assert.Equal(1, swap.SwapCurrentState);
        Assert.False(swap.SwapTickPrimed);

        // The swap's dwell is zero, but its blending children parse an
        // authored eight-second style transition from frontend.bin.
        Assert.True(life.Pump(4f));
        var blendingForestOne = life.FrontendWidgets
            .Single(widget => widget.Name == "BLENDING_BG_FORREST_1");
        Assert.True(blendingForestOne.ColourTransitionActive);
        Assert.Equal(0x80FFFFFFu, blendingForestOne.Colour);

        Assert.True(life.Pump(4f));
        swap = life.FrontendWidgets.First(w => w.Name == "UI_SWAPPING_FORREST");
        Assert.Equal(1, swap.SwapCurrentState);
        Assert.True(swap.SwapTickPrimed);
        var forestOne = life.FrontendWidgets
            .Select((widget, index) => (widget, index))
            .Single(pair => pair.widget.Name == "UI_FRONTEND_BG_FORREST_1_1").index;
        var forestTwo = life.FrontendWidgets
            .Select((widget, index) => (widget, index))
            .Single(pair => pair.widget.Name == "UI_FRONTEND_BG_FORREST_2_1").index;
        Assert.Equal(0x00FFFFFFu,
            FrontendWidgetFactory.EffectiveColour(life.FrontendWidgets, forestOne));
        Assert.Equal(0xFFFFFFFFu,
            FrontendWidgetFactory.EffectiveColour(life.FrontendWidgets, forestTwo));
    }

    [Fact]
    public void Frontend_dumps_press_start_new_profile_main_menu_after_avi_skip()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.Equal(EngineStage.Frontend, life.Stage);
        Assert.True(life.Pump());
        Assert.Equal(EngineLifecycle.FrontendPressStartMenu, life.FrontendMenuRoot);
        Assert.NotNull(life.FrontendBatch);
        Assert.False(life.FrontendBatch.Value.IsEmpty);
        Assert.Equal(4, life.FrontendBatch.Value.Draws[0].D3dPrimitiveType);
        Assert.Equal(5, life.FrontendBatch.Value.Draws[0].D3dSrcBlend);
        Assert.Equal(6, life.FrontendBatch.Value.Draws[0].D3dDestBlend);
        var pressText = life.FrontendWidgets.First(w => w.Name == "UI_PRESS_START_TEXT");
        Assert.Equal((512f, 384f, 512f, 384f),
            (pressText.DestX0, pressText.DestY0, pressText.DestX1, pressText.DestY1));
        var title = life.FrontendWidgets.First(w => w.Name == "UI_TITLE_01");
        Assert.Equal((112f, 48f, 522f, 253f),
            (title.DestX0, title.DestY0, title.DestX1, title.DestY1));
        WriteScreenDump(life, "press-start");
        Assert.Contains(life.FrontendResidentSlots, s => s == EngineLifecycle.FrontendPressStartSlot);

        life.QueueInput(EngineInput.Type4, 0);
        life.QueueInput(EngineInput.Type6, 0);
        Assert.True(life.Pump());
        Assert.Equal(EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        Assert.True(life.Pump(0.25f));
        Assert.Equal("Default", life.FrontendEditBoxName);
        Assert.Contains(life.FrontendWidgets, w => w.Name == "UI_ACCEPT_NEW_PROFILE");
        var arrowsChoice = life.FrontendWidgets.Single(w =>
            w.Name == "UI_OPTIONS_TEXT_CONTROL_ARROWS");
        Assert.True(arrowsChoice.Visible);
        Assert.Equal(FrontendWidgetType.TextSliderFirstSeenSelect,
            arrowsChoice.StyleIndex);
        Assert.False(FrontendWidgetType.LeafDipSkipped(arrowsChoice.Colour));
        var wasdChoice = life.FrontendWidgets.Single(w =>
            w.Name == "UI_OPTIONS_TEXT_CONTROL_WASD");
        Assert.True(wasdChoice.Visible);
        // 00548FA2 first sends state 1 to every authored choice, then
        // 00549075 sends state 3 to the active choice.
        Assert.Equal(1, wasdChoice.StyleIndex);
        Assert.True(FrontendWidgetType.LeafDipSkipped(wasdChoice.Colour));
        Assert.Contains(life.FrontendWidgets, w =>
            w.Name.Contains("COASTAL_SUNBEAM_2_1", StringComparison.Ordinal) && w.Visible);
        Assert.Contains(life.FrontendWidgets, w =>
            w.Name.Contains("COASTAL_1_1", StringComparison.Ordinal) && w.Visible);
        var npText = life.FrontendWidgets.First(w => w.Name == "UI_TEXT_NEW_PROFILE_MENU_TITLE");
        Assert.Equal(npText.DestX0, npText.DestX1);
        Assert.Equal(0f, npText.Leftover204);
        Assert.NotNull(life.FrontendBatch);
        Assert.False(life.FrontendBatch.Value.IsEmpty);
        WriteScreenDump(life, "new-profile");
        Assert.Contains(life.FrontendResidentSlots, s => s == EngineLifecycle.FrontendPressStartSlot);
        Assert.Contains(life.FrontendResidentSlots, s => s == EngineLifecycle.FrontendNewProfileSlot);
        Assert.True(life.TryGetFrontendSlot(EngineLifecycle.FrontendPressStartSlot, out var leftover));
        Assert.Equal(6, leftover.State);
        Assert.Contains(life.FrontendSlotTree(EngineLifecycle.FrontendPressStartSlot),
            w => w.Name == "UI_PRESS_START_TEXT" && w.State == 6 && w.Visible);
        Assert.Contains(life.FrontendSlotTree(EngineLifecycle.FrontendPressStartSlot),
            w => w.Name.Contains("FORREST_1_1", StringComparison.Ordinal) && w.Visible);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == FrontendWidgetType.ForwardSelectFn &&
            e.Action.Contains("0041C5A0", StringComparison.Ordinal));

        ClickNamed(life, "UI_ACCEPT_NEW_PROFILE");
        Assert.Equal(EngineLifecycle.FrontendMainMenuNoContinue, life.FrontendMenuRoot);
        Assert.Contains(life.FrontendWidgets, w =>
            w.Name == "UI_FRONTEND_BUTTON_NEW_GAME" &&
            w.ActionOnLeftUnclicked == FrontendMessages.NewGame);
        Assert.True(life.Pump(0.25f));
        var newGameText = life.FrontendWidgets
            .Select((widget, index) => (widget, index))
            .Single(pair => pair.widget.Name == "UI_TEXT_NEW_GAME");
        Assert.NotEqual(0u,
            FrontendWidgetFactory.EffectiveColour(
                life.FrontendWidgets, newGameText.index) & 0xFF000000u);
        Assert.NotNull(life.FrontendBatch);
        Assert.False(life.FrontendBatch.Value.IsEmpty);
        WriteScreenDump(life, "main-menu");
        Assert.Contains(life.FrontendResidentSlots, s => s == EngineLifecycle.FrontendMainMenuSlot);
        Assert.Contains(life.FrontendResidentSlots, s => s == EngineLifecycle.FrontendPressStartSlot);
        Assert.Contains(life.FrontendResidentSlots, s => s == EngineLifecycle.FrontendNewProfileSlot);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);

        ClickNamed(life, "UI_FRONTEND_BUTTON_NEW_GAME");
        Assert.True(life.RetailNewGameFlag);
        Assert.Equal(EngineStage.Game, life.Stage);
    }

    private static void ClickNamed(EngineLifecycle life, string name)
    {
        var index = -1;
        for (var i = 0; i < life.FrontendWidgets.Count; i++)
        {
            if (life.FrontendWidgets[i].Name == name)
            {
                index = i;
                break;
            }
        }

        Assert.True(index >= 0, name);
        Assert.True(
            FrontendHitTest.TryDestPoint(life.FrontendWidgets, index, out var x, out var y),
            name + " dest empty");
        life.SetFrontendPointer(x, y);
        life.QueueInput(EngineInput.TypeMouse, 0);
        Assert.True(life.Pump());
        life.QueueInput(EngineInput.Type4, 0);
        life.QueueInput(EngineInput.Type6, 0);
        Assert.True(life.Pump());
    }

    private static void WriteScreenDump(EngineLifecycle life, string name)
    {
        Assert.NotNull(life.FrontendPresentRgba);
        ExportDir.WriteRgbaBmp(
            ExportDir.PathFor("frontend", name + ".bmp"),
            life.FrontendPresentWidth, life.FrontendPresentHeight,
            life.FrontendPresentRgba);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"screen={life.FrontendMenuRoot} stage={life.Stage} batch={!life.FrontendBatch!.Value.IsEmpty}");
        for (var index = 0; index < life.FrontendWidgets.Count; index++)
        {
            var w = life.FrontendWidgets[index];
            sb.AppendLine(
                $"{w.Name}\tt={w.Type}\tdest={w.DestX0},{w.DestY0},{w.DestX1},{w.DestY1}\t" +
                $"parent={w.ParentIndex}\tlayer={w.Layer}\tstate={w.State}/{w.StyleIndex}\t" +
                $"colour=0x{w.Colour:X8}/0x{FrontendWidgetFactory.EffectiveColour(life.FrontendWidgets, index):X8}\t" +
                $"g={w.GraphicId}\t+204={w.Leftover204}\ttex={w.TextureName}\taction={w.ActionOnLeftUnclicked}");
        }

        File.WriteAllText(ExportDir.PathFor("frontend", name + "-dests.txt"), sb.ToString());
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
        Assert.Equal(0x00488B20u, EngineLifecycle.StartingHolySiteFindFn);
        Assert.Equal(0x00488B68u, EngineLifecycle.StartingHolySiteReadName);
        Assert.Equal(0x00413840u, EngineLifecycle.SetStartingHolySiteFn);
        Assert.Equal(0x013B866Cu, EngineLifecycle.WorldPathAltGlobalVa);
        Assert.False(EngineLifecycle.StartingHolySiteIsNovStartOnNoSave);
        Assert.True(EngineLifecycle.StartingHolySiteFinderMissesOnNoSave);
        Assert.Equal("NOVStartHSP", EngineLifecycle.StartingHolySiteStoredName);
        Assert.NotEqual(RegionTravel.NewGameStartScript, EngineLifecycle.GuildArrivalHsp);
        Assert.NotEqual(RegionTravel.NewGameStartScript, life.Hero.ScriptName);
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
            world+172 writer is 004A0D90 AddQuest TRUE
            (FinalAlbion.qst then GlobalQuests.qst).
            00507C30 has no START_INITIAL_QUESTS case.
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
        Assert.NotNull(life.Runtime);
        Assert.Same(life.Camera, life.Runtime.Camera);
        Assert.Equal(
            new[]
            {
                "Q_SunnyvaleMaster",
                "ChapterAndSceneManager",
                "PersonalScriptMain",
                "PersonalScript_GlobalThings",
                "NPCDeath",
                "HeroBoasts",
                "V_HeroDolls",
                "CS_PlayCutscene",
                "Global_WatchForHeroDeath",
            },
            life.WorldPlus172);
        Assert.Equal(life.WorldPlus172, life.ActivatedQuests.Take(9));
        Assert.Equal("Gameflow", life.ActivatedQuests[9]);
        Assert.Equal(10, life.ActivatedQuests.Count);
        Assert.DoesNotContain("Q_NewOakValeIntro", life.WorldPlus172);
        Assert.DoesNotContain("Q_NewOakValeIntro", life.ActivatedQuests);
        Assert.Contains(life.World!.InitialQuests, q => q == "Q_SunnyvaleMaster");
        Assert.Equal(6, life.World.InitialQuests.Count);
        Assert.DoesNotContain(life.World.InitialQuests, q => q == "ChapterAndSceneManager");
        Assert.DoesNotContain(life.World.InitialQuests, q => q == "NPCDeath");
        Assert.DoesNotContain(life.World.InitialQuests, q => q == "Global_WatchForHeroDeath");
        Assert.NotNull(life.Quests);
        Assert.Contains(life.Quests.Quests, q => q.Name == "Q_SunnyvaleMaster" && q.Persistent);
        Assert.Contains(life.Quests.Quests, q =>
            q.Name == "Global_WatchForHeroDeath" && q.Persistent);
        Assert.Contains(life.WorldPlus184, q => q == "Q_NewOakValeIntro");
        Assert.Equal(life.WorldPlus184, life.QuestManagerPlus44);
        Assert.Contains(life.QuestManagerPlus44, q => q == "Gameflow");
        Assert.Contains(life.QuestManagerPlus44, q => q == "Q_NewOakValeIntro");
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.QuestManagerPushFn &&
            e.Action.Contains("004B2850", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.QuestActivateGateFn &&
            e.Action.Contains("Q_SunnyvaleMaster", StringComparison.Ordinal));
        Assert.Equal(0x004B2850u, EngineLifecycle.QuestManagerPushFn);
        Assert.Equal(0x004B00C0u, EngineLifecycle.QuestActivateGateFn);
        Assert.Equal(44, EngineLifecycle.QuestManagerPlus44Offset);
        Assert.NotNull(life.Runtime);
        Assert.Equal(10, life.Runtime.Quests.Count);
        Assert.Equal(10, life.Runtime.Scheduler.Fibers.Count);
        Assert.All(life.Runtime.Quests, q => Assert.NotNull(q.Fiber));
        Assert.All(
            life.Runtime.Quests.Where(q => QuestFactoryTable.Find(q.Name) is not null),
            q => Assert.True(q.Started));
        Assert.False(life.Runtime.Quests.Single(q => q.Name == "ChapterAndSceneManager").Started);
        Assert.False(life.Runtime.Quests.Single(q => q.Name == "NPCDeath").Started);
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
              004A0D90 AddQuest TRUE (flag 1 then 0)
              FinalAlbion.qst then GlobalQuests.qst
              Q_SunnyvaleMaster ChapterAndSceneManager
              PersonalScriptMain PersonalScript_GlobalThings
              NPCDeath HeroBoasts V_HeroDolls
              CS_PlayCutscene Global_WatchForHeroDeath
            00507C30 has no START_INITIAL_QUESTS case
            00416ABA 004A1840 Load Quests
              004A08D0 flag 1 clear +184/+172/+196
              004A0D90 AddQuest → +184; TRUE → +172
            00416ABF [0x13B8648]==0
              0049F180(ecx=world) Init Quests
              004B4260([world+172])
              00CB5AD0 lookup / 00A447D0 fiber
            00416BCF empty +90584 → 004B4A10
            user.ini Gameflow is later, not +172
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
        var userstText = File.ReadAllText(userst);
        var exclusive = userstText.Contains("SetFullscreen(true)", StringComparison.OrdinalIgnoreCase)
            || userstText.Contains("SetFullscreen(1)", StringComparison.OrdinalIgnoreCase);
        Assert.Equal(exclusive ? 1 : 0, life.DisplayWindowFlag);
        Assert.Equal(!exclusive, life.DeviceWindowed);
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
              Create Players 00488D10 00687A30 vtbl 0123758C
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
        Assert.All(
            life.Runtime.Quests.Where(q => QuestFactoryTable.Find(q.Name) is not null),
            q => Assert.True(q.Started));
        var watch = life.Runtime.Quests.Single(q =>
            q.Name == QuestFactoryTable.WatchForHeroDeathName);
        Assert.True(watch.Started);
        Assert.Equal(QuestFactoryTable.WatchForHeroDeathFactory, watch.Factory);
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
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerGuiAllocFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerGuiCtorFn);
        Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.PlayerGuiStoreFn);
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
        Assert.Equal(13, EngineLifecycle.InitGameStages.Length);
        Assert.Equal("Init Fonts", EngineLifecycle.InitGameStages[3].Stage);
        Assert.Equal("Init World", EngineLifecycle.InitGameStages[8].Stage);
        Assert.Equal(FontFile.InitFontsFn, EngineLifecycle.InitFontsFn);
        Assert.Equal(FontFile.GameFace, life.GameFontFace);
        Assert.Equal(90444, EngineLifecycle.GameFontOffset);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.InitFontsFn &&
            e.Action.Contains("Init Fonts", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.GameFontStoreFn &&
            e.Action.Contains("90444", StringComparison.Ordinal));
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
        Assert.Equal(12, life.QuestPumpWalked);
        Assert.Equal(10, life.EventPosts);
        Assert.Equal(10, life.EventPumpWalked);
        Assert.Equal(50, EngineLifecycle.EventPostDelay);
        Assert.Equal(55, EngineLifecycle.EventPostKind);
        Assert.Equal(0x37, EngineLifecycle.QuestConstructEventKind);
        Assert.Equal(EngineLifecycle.QuestConstructEventKind, EngineLifecycle.EventPostKind);
        Assert.Equal(0x33, EngineLifecycle.QuestGiveEventKind);
        Assert.NotEqual(EngineLifecycle.QuestGiveEventKind, EngineLifecycle.EventPostKind);
        Assert.Equal(0x00892F80u, EngineLifecycle.QuestGiveFn);
        Assert.Equal(1152, EngineLifecycle.QuestGiveVtbl);
        Assert.Equal(0x004B1D30u, EngineLifecycle.QuestGiveBody);
        Assert.Equal(0x00DBE295u, EngineLifecycle.QuestGiveAfterAttackOver);
        Assert.True(EngineLifecycle.GameflowWaitsForeverOnNoSave);
        Assert.False(EngineLifecycle.ActivateQuestSatisfiesGameflowWait);
        Assert.False(RegionTravel.FirstSeenAttackOverStoreRuns);
        Assert.False(RegionTravel.RaidAviIsBanditRaid);
        Assert.True(RegionTravel.AttackOverStoreAfterRaidAvi);
        Assert.Equal("1_raid_on_oak_vale_comp.xmv", RegionTravel.RaidPlayAvi);
        Assert.Equal(0x00DB97A0u, RegionTravel.TheresaMeetStart);
        Assert.Equal(0x00DBB2A7u, RegionTravel.AttackOverStore);
        Assert.Equal(1152u, NewGameScript.GiveQuestVtbl);
        Assert.Equal(1104u, NewGameScript.StartQuestVtbl);
        Assert.True(NewGameScript.GiveAfterPostAttackAndMaze);
        Assert.Equal(0x00DB4F70u, NewGameScript.StartBarrelTimerCallback);
        Assert.Equal("OBJECT_CHOCOLATE_BOX_UNGIVEABLE", NewGameScript.ChocolateBoxDef);
        Assert.Equal(1180, EngineLifecycle.QuestCardBindVtbl);
        Assert.Equal(0x008968C0u, EngineLifecycle.QuestCardBindFn);
        Assert.Equal(0x00896A30u, EngineLifecycle.QuestCardBindSiblingFn);
        Assert.Equal(1184, EngineLifecycle.QuestCardBindSiblingVtbl);
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
        Assert.Equal(0x00893570u, EngineLifecycle.QuestIsActiveFn);
        Assert.Equal(100, EngineLifecycle.QuestIsActiveVtbl);
        Assert.Equal(0x00893610u, EngineLifecycle.QuestIsGetFn);
        Assert.Equal(104, EngineLifecycle.QuestIsGetVtbl);
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
    public void Type1_resume_00CB8220_is_00A44880_then_00893570_yield()
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
        Assert.False(EngineLifecycle.SqnoviReentersRunAfterYield);
        Assert.Equal(0x00CDD440u, EngineLifecycle.SqnoviMainWatcherThunk);
        Assert.Equal(20, EngineLifecycle.CtcExpressionSize);
        Assert.Equal(0x90, EngineLifecycle.ExpressionSize);
        Assert.Equal(0x00456A54u, EngineLifecycle.ExpressionPlus120Persist);
        Assert.Equal(0x00456A5Au, EngineLifecycle.ExpressionPlus120Call);
        Assert.Equal(0x004569A7u, EngineLifecycle.ExpressionPersistFn);
        Assert.Equal(0x1FB35A1Bu, EngineLifecycle.ExpressionPlus120Crc);
        Assert.Equal(-1, EngineLifecycle.ExpressionPlus120EmptySentinel);
        Assert.False(EngineLifecycle.ExpressionPlus120IsOakvaleIntern);
        Assert.Equal(0x007EF200u, EngineLifecycle.ExpressionTickFn);
        Assert.False(EngineLifecycle.ExpressionTickWritesOakvaleIntern);
        Assert.Equal(0x012C5D14u, EngineLifecycle.OakvaleQuestIntern);
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
        Assert.DoesNotContain(life.WorldPlus172, q => q == "Q_NewOakValeIntro");
        Assert.DoesNotContain(life.ActivatedQuests, q => q == "Q_NewOakValeIntro");
        Assert.Contains(life.WorldPlus172, q => q == "Global_WatchForHeroDeath");
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
        Assert.False(EngineLifecycle.InventoryQuestsConfirmIsNewGame);
        Assert.Equal(0x0061AB30u, EngineLifecycle.InventoryQuestsConfirmFn);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.InventoryQuestsConfirmFn &&
            e.Action.Contains("not New Game", StringComparison.Ordinal));
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.StartNewQuestParseFn &&
            e.Action.Contains("0 E8 no-save", StringComparison.Ordinal));
        Assert.Equal(0x0049EAC0u, EngineLifecycle.QuestActivatePlus172SiblingFn);
        Assert.Equal(0x0049EAD1u, EngineLifecycle.QuestActivatePlus172SiblingCall);
        Assert.Equal(0xAC, EngineLifecycle.QuestActivatePlus172SiblingListOffset);
        Assert.False(EngineLifecycle.QuestActivatePlus172SiblingHasInbound);
        Assert.False(EngineLifecycle.ChildhoodTngQueuesActivateQuest);
        Assert.Equal(212, EngineLifecycle.WorldCreatureGenerationEnabledOffset);
        Assert.False(EngineLifecycle.FirstSeenCanRenderParticles);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.QuestActivatePlus172SiblingFn &&
            e.Action.Contains("0 inbound skip", StringComparison.Ordinal));
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
        var oakvale = QuestFactoryTable.Find("Q_NewOakValeIntro");
        Assert.NotNull(oakvale);
        Assert.Equal(RegionTravel.IntroScriptName, oakvale.Value.ScriptName);
        Assert.Equal(RegionTravel.IntroQuestFactory, oakvale.Value.Factory);
        Assert.Equal(RegionTravel.IntroQuestRun, oakvale.Value.Run);
        Assert.DoesNotContain(life.Runtime.Quests, q => q.Name == "Q_NewOakValeIntro");
        Assert.True(string.IsNullOrEmpty(life.Runtime.LastMusic));
        Assert.False(EngineLifecycle.RequestNewGameStartsMusicSet);
        Assert.False(EngineLifecycle.InitSoundPlaysMusicSet);
        Assert.False(EngineLifecycle.ScriptPlayMusicAppliesBank);
        Assert.Equal(0x00CC8EACu, EngineLifecycle.ScriptPlayMusicApplyFn);
        Assert.Equal(2784, EngineLifecycle.ScriptPlayMusicVtbl);
        Assert.False(EngineLifecycle.QuestCompletionUiGiveIsFirstSeen);
        Assert.Equal(0x005E7B77u, EngineLifecycle.QuestCompletionUiGiveFn);
        Assert.Equal(0x0061ACB3u, EngineLifecycle.InventoryQuestsGiveFn);
        Assert.False(EngineLifecycle.InventoryQuestsGiveIsFirstSeen);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.InventoryQuestsGiveFn &&
            e.Action.Contains("not first-seen", StringComparison.Ordinal));
        Assert.Equal(0x004B1D30u, EngineLifecycle.QuestGiveBody);
        Assert.Equal(0x33, EngineLifecycle.QuestGiveEventKind);
        Assert.Equal(0x37, EngineLifecycle.QuestConstructEventKind);
        Assert.False(RegionTravel.WatchBarrelsSmashHasDistance);
        Assert.False(RegionTravel.WatchBarrelsSmashIsRadius);
        Assert.False(RegionTravel.ChildhoodObjectivesRunOnNoSave);
        Assert.Equal(0x00DB7E10u, RegionTravel.WatchBarrelsInstructionFn);
        Assert.Equal(0x00DAC1BAu, RegionTravel.ChildhoodObjective01Fn);
        Assert.Equal(0x00DB080Au, RegionTravel.ChildhoodObjective02Fn);
        Assert.Equal(0x00DBE34Fu, RegionTravel.ChildhoodObjective03Fn);
        Assert.Equal(0x00DB4A93u, RegionTravel.ChildhoodObjective04Fn);
        Assert.Equal(0x00DB9DE6u, RegionTravel.ChildhoodObjective05Fn);
        Assert.Equal(0x00DBE478u, RegionTravel.ChildhoodObjective06Fn);
        Assert.Equal(0x338, EngineLifecycle.PlayerGuiObjectSize);
        Assert.Equal(0xAB4, EngineLifecycle.PlayerGuiDefSize);
        Assert.Equal(824, EngineLifecycle.PlayerGuiDefPlus338);
        Assert.False(EngineLifecycle.PlayerGuiDefPlus338IsHud);
        Assert.False(EngineLifecycle.PlayerGuiFirstPresentDrawsHud);
        Assert.False(EngineLifecycle.InitGuiIsCtor);
        Assert.False(EngineLifecycle.PlayerGuiSizeWrittenAfterInitGui);
        Assert.False(EngineLifecycle.HostFadesLeaveFrontendAudio);
        Assert.True(EngineLifecycle.NewGameIsNoSaveWalk);
        Assert.False(EngineLifecycle.NewGameWritesSaveQuestList);
        Assert.Equal(0x004B5080u, EngineLifecycle.StartNewQuestParseFn);
        Assert.False(RegionTravel.GamePlayAviOwnsPump);
        Assert.False(FrontendLayout.TryChromeHitIsNativeHit);
        Assert.False(EngineLifecycle.RetailPlus8ChangesInitQuests);
        Assert.False(EngineLifecycle.CActivateQuestDefInternsOakvale);
        Assert.Equal(0x8D19C362u, EngineLifecycle.OakvaleQuestFableCrc);
        Assert.Equal(1, life.GamePlus16);
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.InitSoundRegisterFn &&
            e.Action.Contains("009919C0", StringComparison.Ordinal));
        Assert.DoesNotContain(life.Trace.Events, e =>
            e.Va == EngineLifecycle.InitSoundSymbolsTextFn);
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
        Assert.True(life.DisplayPresent);
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
        Assert.False(EngineLifecycle.PumpCallsLoadFromFirstRealRegion);
        Assert.Equal(0, EngineLifecycle.LoadFromFirstRealRegionNamedInbound);
        Assert.True(EngineLifecycle.FirstSeenRunningParticleListEmpty);
        Assert.False(EngineLifecycle.FirstSeenCanRenderParticles);
        Assert.Equal(47, EngineLifecycle.OakvalePlaceableEmitterCount);
        Assert.Equal(0x004174F1u, EngineLifecycle.LoadParticlesFn);
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
        Assert.Equal(3, EngineLifecycle.CollectThingsBitDword);
        Assert.Equal(44, EngineLifecycle.CollectThingsBitThingOffset);
        Assert.False(EngineLifecycle.CollectThingsBitMeansCollidable);
        Assert.True(EngineLifecycle.HeroAddsPhysicsControlledOnly);
        Assert.False(EngineLifecycle.FirstSeenCollisionIsSolver);
        Assert.Equal(0x004D297Bu, EngineLifecycle.CtcPhysicsStandardFactory);
        Assert.Equal(0x00723FD0u, EngineLifecycle.CtcPhysicsStandardCtor);
        Assert.Equal(0x88, EngineLifecycle.CtcPhysicsStandardSize);
        Assert.Equal(0x00724290u, EngineLifecycle.CtcPhysicsStandardPersist);
        Assert.Equal(80, EngineLifecycle.CtcPhysicsStandardPosOffset);
        Assert.Equal(92, EngineLifecycle.CtcPhysicsStandardAxisOffset);
        Assert.Equal(145, EngineLifecycle.ThingCollectFlagsOffset);
        Assert.Equal(0x0C, EngineLifecycle.ThingCollectFlagsNeed);
        Assert.Equal(0x21, EngineLifecycle.ThingCollectFlagsForbid);
        Assert.Equal(0x005198B0u, EngineLifecycle.CollectScriptedHookThingsFn);
        Assert.Equal(0x00518DC0u, EngineLifecycle.ScriptedHookCollectFn);
        Assert.Equal("CTCActionUseScriptedHook", EngineLifecycle.ScriptedHookName);
        Assert.Equal(0xC2, EngineLifecycle.ScriptedHookKey);
        Assert.Equal(56, EngineLifecycle.ScriptedHookThingOffset);
        Assert.Equal(4, EngineLifecycle.ScriptedHookThingBit);
        Assert.False(EngineLifecycle.WorldUseAttachedOnNoSave);
        Assert.False(EngineLifecycle.LookoutHasBarrels);
        Assert.False(EngineLifecycle.FrontendActionsAreWorldUse);
        Assert.False(EngineInput.ActionType10IsWorldUse);
        Assert.True(EngineInput.ActionType10IsRmbHover);
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
        Assert.True(life.DisplayPresent);
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
        Assert.False(EngineInput.ActionApplyIsLocomotion);
        Assert.Equal(19, EngineInput.TypePadA);
        Assert.Equal(22, EngineInput.ActionPadA);
        Assert.False(EngineInput.TypeAnalogPostsActionApply);
        Assert.False(RegionTravel.FirstSeenHandsPlayerControl);
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
        Assert.Equal(0x00449E60u, EngineLifecycle.LoadRegionByNamePersist);
        Assert.Equal(0x00449F90u, EngineLifecycle.SavePlayerRegionNameFn);
        Assert.Equal(0x0049FB5Cu, EngineLifecycle.SavePlayerRegionNameSite);
        Assert.Equal(0x0049F4C0u, EngineLifecycle.FableSavPlayerWriter);
        Assert.Equal(0x004109A0u, EngineLifecycle.PersistCStringTransfer);
        Assert.Equal("PlayerRegionName", EngineLifecycle.PlayerRegionNameKey);
        Assert.Equal("PLAYER", EngineLifecycle.FableSavPlayerSection);
        Assert.False(EngineLifecycle.PlayerRegionNameWrittenOnNewGame);
        Assert.False(RegionTravel.StartOakValeSetupLoadsRegion);
        Assert.Equal(48, RegionTravel.StartOakValeWaitVtbl);
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
        Assert.Equal(0x00487FB0u, EngineLifecycle.PlayerGuiAllocFn);
        Assert.Equal(0x0043B570u, EngineLifecycle.PlayerGuiCtorFn);
        Assert.Equal(0x004195AFu, EngineLifecycle.PlayerGuiStoreFn);
        Assert.Equal(0x0123177Cu, EngineLifecycle.PlayerGuiVtbl);
        Assert.Equal(0x338, EngineLifecycle.PlayerGuiObjectSize);
        Assert.Equal(0x013B8790u, EngineLifecycle.PlayerGuiSingletonVa);
        Assert.Equal("PLAYER_GUI_PC", EngineLifecycle.PlayerGuiPcName);
        Assert.NotEqual(EngineLifecycle.PlayerGuiCtorFn, EngineLifecycle.InitGuiFn);
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
        Assert.Equal(1, EngineLifecycle.LoadGlobalThingsEbxStart);
        Assert.Equal(203, EngineLifecycle.StartOakValeWestTngEbx);
        Assert.True(EngineLifecycle.LoadGlobalThingsHostBreaksAfterFirstProx);
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
