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
        Assert.Null(life.CurrentStartupVideo);
        Assert.Null(life.WorldFileName);
        Assert.True(life.Pump());
        Assert.NotEqual(EngineStage.Game, life.Stage);
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
        Assert.Contains(life.Trace.Events, e =>
            e.Action.StartsWith("present GBANK_MAIN_PC", StringComparison.Ordinal));
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
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
            "traces");
        Directory.CreateDirectory(dest);
        life.Trace.Write(Path.Combine(dest, "winmain-forward.txt"));
        life.Trace.Write(Path.Combine(dest, "init-world-map.txt"));
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
}
