using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Wld;

namespace Fable.Game;

/// <summary>
/// Recovered Fable.exe process entry → WinMain →
/// named bootstrap → library/D3D9 → mode loop.
/// Do not start at <c>00DBDE40</c> / New Game.
/// </summary>
public sealed class EngineLifecycle
{
    public const uint ImageBase = 0x00400000;
    public const uint PeEntry = 0x00401067;
    public const uint WinMain = 0x00403480;
    public const uint BootstrapFn = 0x00402510;
    public const uint SetupLibrary = 0x009A6610;
    public const uint EngineSingletonVa = 0x013CA618;
    public const uint EngineSingletonGetter = 0x009A4EC0;
    public const uint BankManagerVa = 0x013CA79C;
    public const uint RegisterRetailBank = 0x009A8150;
    public const uint Direct3DCreate9Thunk = 0x00BFEFB0;
    public const int Direct3DSdkVersion = 32;
    public const uint GraphicsCtor = 0x009C0880;
    public const uint GraphicsInit = 0x009C0E50;
    public const uint CreateDeviceFn = 0x009BF7E0;
    public const int GraphicsObjectSize = 0x2C8;
    public const int IDirect3D9GetDeviceCapsVtbl = 56;
    public const int IDirect3D9CreateDeviceVtbl = 64;
    /// <summary>
    /// <c>D3DCREATE_FPU_PRESERVE|MULTITHREADED|SOFTWARE_VERTEXPROCESSING</c>
    /// when caps lack <c>D3DDEVCAPS_HWTRANSFORMANDLIGHT</c>.
    /// </summary>
    public const int CreateDeviceSoftwareFlags = 0x26;
    /// <summary>
    /// <c>FPU_PRESERVE|MULTITHREADED|PUREDEVICE|HARDWARE_VERTEXPROCESSING</c>
    /// when <c>DevCaps &amp; 0x100000</c>.
    /// </summary>
    public const int CreateDeviceHardwareFlags = 0x56;
    public const uint DevCapsHwTnl = 0x100000;
    public const uint ProbeGraphics = 0x004022B0;
    public const uint RunModes = 0x00412F90;
    public const uint Shutdown = 0x00401B80;
    public const uint RetailModeCtor = 0x0042EA8F;
    public const uint RetailModeVtbl = 0x01230CA0;
    public const uint RetailStart = 0x0042F75E;
    public const uint RetailPump = 0x0042EC7C;
    public const int RetailModeSize = 0x148;
    public const uint GameModeCtor = 0x00418DCA;
    public const uint GameModeVtbl = 0x0122F180;
    public const uint GameStart = 0x004184BD;
    public const uint GamePump = 0x004189C2;
    public const int GameModeSize = 0x161E8;
    public const uint InitWorldFn = 0x0041735A;
    public const uint CreatePlayersFn = 0x004166A8;
    public const uint InitGraphicsFn = 0x00416C8A;
    public const uint PlayAviPlayer = 0x006286F0;
    public const uint FrontendIntern = 0x0042F722;
    public const uint LeaveFrontendSite = 0x0042F2A2;
    public const uint InitGameSite = 0x0042F491;
    public const string FinalAlbionWld = "FinalAlbion.wld";
    public const uint VideoPlayFlagVa = 0x01375448;
    public const uint VideoPlayFlag2Va = 0x0137544A;
    public const byte DefaultVideoPlayFlag = 1;
    public const byte DefaultVideoPlayFlag2 = 1;

    public static readonly (string Stage, uint Va)[] NamedBootstrapStages =
    [
        ("Parse Command Line", 0x00402521),
        ("Setup Basic install files", 0x004025B3),
        ("Setup Language", 0x0040266F),
        ("Setup basic retail banks", 0x00402845),
        ("Setup library", 0x00403079),
        ("End basic init", 0x00403354),
    ];

    /// <summary>
    /// <c>004184BD</c> vtbl+4 after <c>00418DCA</c>.
    /// Not <c>00DBDE40</c>.
    /// </summary>
    public static readonly (string Stage, uint Apply)[] InitGameStages =
    [
        ("Init Thing Components", 0x004EE23F),
        ("Init Definition Manager", 0x00416005),
        ("Init Graphics", 0x00416C8A),
        ("Init Subtitled Message", 0x004CDB10),
        ("Init Conversation Attitude", 0x004CD670),
        ("Init Player Manager", 0x0041732A),
        ("Init Player Interface", 0x004473A0),
        ("Init World", 0x0041735A),
        ("Init Display Engine", 0x00417418),
        ("Create Players", 0x004166A8),
        ("Init Sound", 0x00417A58),
        ("Load Particles", 0x004174F1),
    ];

    /// <summary>
    /// <c>004A6E30</c> world object vtbl+36
    /// ("Init World Init"). Map object is
    /// <c>005066E0</c>; file parse is
    /// <c>00507C30</c>.
    /// </summary>
    public static readonly (string Stage, uint Apply)[] InitWorldInitStages =
    [
        ("Init World Map", 0x005066E0),
        ("Init Environment", 0x006BBC30),
        ("Init Navigation Manager", 0x00A15670),
        ("Init Combat Manager", 0x006ED3F0),
        ("Init Thing Manager", 0x0049EBF0),
        ("Init Event Manager", 0x00687510),
        ("Init Game Camera Manager", 0x0069AE80),
        ("Init Game Camera", 0x006FD8C0),
        ("Init Mesh Bank", 0x0049E620),
        ("Init UI Manager", 0x0041D198),
    ];

    public const uint InitWorldInitFn = 0x004A6E30;
    public const uint InitWorldMapFn = 0x005066E0;
    public const uint WorldMapVtbl = 0x01244AEC;
    public const uint LoadWldFile = 0x00507C30;
    public const uint LoadGtng = 0x0050959F;
    public const uint LoadGlobalThings = 0x00509859;
    public const uint LoadRegionGraph = 0x00509982;
    public const int WorldMapObjectSize = 0xD8;
    public const int WorldMapCellShift = 5;
    public const int WorldMapBound = 0x2000;

    /// <summary>
    /// <c>00507C30</c> token switch. Same
    /// vocabulary as <see cref="WorldFile"/>.
    /// </summary>
    public static readonly string[] LoadWldTokens =
    [
        "MapUIDCount", "ThingManagerUIDCount", "LevelScriptName",
        "NewMap", "EndMap", "MapUID", "MapX", "MapY", "IsSea",
        "LoadedOnPlayerProximity", "LevelName", "NewRegion", "EndRegion",
        "RegionDef", "EnvironmentDef", "DisplayName", "RegionName",
        "NewDisplayName", "ContainsMap", "SeesMap", "AppearOnWorldMap",
    ];

    public static readonly (string Logical, string Pc)[] RetailBanks =
    [
        ("GBANK_MAIN", "GBANK_MAIN_PC"),
        ("GBANK_GUI", "GBANK_GUI_PC"),
        ("GBANK_FRONT_END", "GBANK_FRONT_END_PC"),
        ("PARTICLE_MAIN", "PARTICLE_MAIN_PC"),
        ("PARTICLE_FRONTEND", "PARTICLE_FRONTEND_PC"),
    ];

    /// <summary>
    /// 32-byte records built at <c>0042ECC5</c>.
    /// Played by <c>006286F0</c> when both
    /// <c>0x1375448</c> and <c>0x137544A</c> are set
    /// (PE defaults are 1, 1).
    /// </summary>
    public static readonly StartupVideo[] StartupVideos =
    [
        new("Data\\Video\\lionhead_logo.xmv", 640, 400, 0xFFFFFFFFu, 0x0042E3CE),
        new("Data\\Video\\Microsoft_Logo.xmv", 640, 480, 0xFF000000u, 0x0042E3CE),
        new("Data\\Video\\intro_comp.xmv", 640, 360, 0x00000000u, 0x0042E3CE),
    ];

    public ForwardLifecycleTrace Trace { get; } = new();
    public EngineStage Stage { get; private set; } = EngineStage.ProcessEntry;
    public EngineMode Mode { get; private set; } = EngineMode.None;
    public int StartupVideoIndex { get; private set; }
    public bool PlayStartupVideos { get; private set; } = true;
    public bool GraphicsCreated { get; private set; }
    public int CreateDeviceFlags { get; private set; }
    public string? WorldFileName { get; private set; }
    public WorldFile? World { get; private set; }
    public IReadOnlyList<string> CompletedStages => _completed;
    public IReadOnlyList<string> RegisteredBanks => _banks;
    public GameInstall? Install { get; private set; }

    private readonly List<string> _completed = [];
    private readonly List<string> _banks = [];

    public static int CreateDeviceBehaviorFlags(bool hardwareTnl) =>
        hardwareTnl ? CreateDeviceHardwareFlags : CreateDeviceSoftwareFlags;

    /// <summary>
    /// CRT <c>00401067</c> → WinMain <c>00403480</c>
    /// → named stages in <c>00402510</c> through
    /// <c>End basic init</c>. Does not enter
    /// <c>00DBDE40</c>.
    /// </summary>
    public void Bootstrap(GameInstall? install)
    {
        Install = install;
        Note(PeEntry, "ProcessEntry", "CRT", "WinMainCRTStartup");
        Stage = EngineStage.CrtStartup;
        Note(WinMain, "WinMain", "App", "CreateMutex then 00402510");
        Stage = EngineStage.WinMain;

        foreach (var (name, va) in NamedBootstrapStages)
        {
            Stage = name switch
            {
                "Parse Command Line" => EngineStage.ParseCommandLine,
                "Setup Basic install files" => EngineStage.SetupInstallFiles,
                "Setup Language" => EngineStage.SetupLanguage,
                "Setup basic retail banks" => EngineStage.SetupRetailBanks,
                "Setup library" => EngineStage.SetupLibrary,
                "End basic init" => EngineStage.EndBasicInit,
                _ => Stage,
            };
            Note(va, name, "Bootstrap", name);
            if (name == "Setup basic retail banks")
                RegisterRetailBankTable(install);
            if (name == "Setup library")
                ConstructLibrary();
            _completed.Add(name);
        }

        Note(ProbeGraphics, "ProbeGraphics", "D3D9", "004022B0 bpp 16/24/32");
        Note(RunModes, "RunModes", "ModeLoop", "00412F90 retail 0042EA8F");
        Mode = EngineMode.RetailFrontend;
        Stage = PlayStartupVideos
            ? EngineStage.StartupVideos
            : EngineStage.Frontend;
        StartupVideoIndex = 0;
        if (Stage == EngineStage.StartupVideos)
            Note(RetailPump, "StartupVideos", "PlayAVI", StartupVideos[0].RelativePath);
        else
            Note(FrontendIntern, "Frontend", "FRONT_END", "skip videos");
    }

    /// <summary>
    /// One <c>00412F90</c> / <c>0042EC7C</c> step.
    /// Returns false when the mode loop exits.
    /// </summary>
    public bool Pump()
    {
        if (Stage == EngineStage.StartupVideos)
            return true;
        if (Stage == EngineStage.Frontend)
            return true;
        if (Stage == EngineStage.LeaveFrontend)
        {
            EnterGame();
            return true;
        }

        if (Stage == EngineStage.Game)
            return true;
        if (Stage == EngineStage.Shutdown)
            return false;
        return Stage != EngineStage.Exited;
    }

    public StartupVideo? CurrentStartupVideo =>
        Stage == EngineStage.StartupVideos &&
        StartupVideoIndex >= 0 &&
        StartupVideoIndex < StartupVideos.Length
            ? StartupVideos[StartupVideoIndex]
            : null;

    /// <summary>
    /// <c>006286F0</c> returned for one table slot.
    /// After the third video: Init Engine / Init
    /// frontend (<c>0042EF40</c> / <c>0042EF6F</c>).
    /// </summary>
    public void FinishStartupVideo()
    {
        if (Stage != EngineStage.StartupVideos)
            return;
        var done = CurrentStartupVideo;
        if (done is { } finished)
            Note(PlayAviPlayer, "StartupVideos", "PlayAVI", "complete " + finished.RelativePath);
        StartupVideoIndex++;
        if (StartupVideoIndex < StartupVideos.Length)
        {
            Note(RetailPump, "StartupVideos", "PlayAVI", StartupVideos[StartupVideoIndex].RelativePath);
            return;
        }

        Note(0x0042EF40, "InitEngine", "Engine", "Init Engine");
        Note(0x0042EF6F, "InitFrontend", "Frontend", "Init frontend");
        Note(FrontendIntern, "Frontend", "FRONT_END", "0042F722");
        Stage = EngineStage.Frontend;
    }

    /// <summary>
    /// Frontend New Game (no-save). Native sets
    /// <c>[esi+41]</c> then <c>Leave frontend</c>
    /// at <c>0042F2A2</c>. Save enumerate unread —
    /// do not implement it here.
    /// </summary>
    public void RequestNewGame()
    {
        if (Stage != EngineStage.Frontend)
            return;
        Note(LeaveFrontendSite, "LeaveFrontend", "Frontend", "Leave frontend");
        Stage = EngineStage.LeaveFrontend;
        WorldFileName = FinalAlbionWld;
        Note(0x0042F44D, "LeaveFrontend", "World", FinalAlbionWld);
    }

    /// <summary>
    /// <c>Init Game</c> <c>0042F491</c> then
    /// <c>00418DCA</c> (size <c>0x161E8</c>).
    /// Quest/script start is that object's
    /// vtbl+4 — not a host jump to
    /// <c>00DBDE40</c>.
    /// </summary>
    public void EnterGame()
    {
        if (Stage is not (EngineStage.LeaveFrontend or EngineStage.Frontend))
            return;
        if (Stage == EngineStage.Frontend)
            RequestNewGame();
        Note(InitGameSite, "InitGame", "Game", "Init Game");
        Note(GameModeCtor, "InitGame", "GameMode", "00418DCA size 0x161E8 vtbl 0122F180");
        Note(GameStart, "InitGame", "GameStart", "004184BD vtbl+4");
        foreach (var (name, apply) in InitGameStages)
            Note(apply, name, "InitGame", name);
        Note(InitWorldFn, "Init World", "World", "004A67D0 vtbl 012390F0");
        Note(InitWorldInitFn, "Init World Init", "World", "004A6E30 vtbl+36");
        foreach (var (name, apply) in InitWorldInitStages)
            Note(apply, name, "World", name);
        LoadWorldMap();
        Note(CreatePlayersFn, "Create Players", "Player", "0044A530/004AE940 UNREAD");
        Mode = EngineMode.Game;
        Stage = EngineStage.Game;
        WorldFileName = FinalAlbionWld;
    }

    /// <summary>
    /// <c>005066E0</c> constructs the 0xD8 map
    /// object (shift 5, bound 0x2000).
    /// <c>00507C30</c> vtbl+12 is
    /// <c>Load .wld file</c>: token-switch parse
    /// of <c>FinalAlbion.wld</c>. GTNG / global
    /// things / region graph stay UNREAD.
    /// </summary>
    public void LoadWorldMap()
    {
        Note(InitWorldMapFn, "Init World Map", "WLD",
            "005066E0 ctor size 0xD8 shift 5 bound 0x2000 vtbl 01244AEC");
        Note(LoadWldFile, "Load .wld file", "WLD", "00507C30 vtbl+12");
        WorldFileName = FinalAlbionWld;
        if (Install is null)
            return;
        if (!File.Exists(Install.WorldPath))
        {
            Note(LoadWldFile, "Load .wld file", "WLD", "missing " + FinalAlbionWld);
            return;
        }

        World = WorldFile.Load(Install.WorldPath);
        Note(LoadWldFile, "Load .wld file", "WLD",
            $"maps={World.Maps.Count} regions={World.Regions.Count} quests={World.InitialQuests.Count}");
        Note(LoadGtng, "Load GTNG", "WLD", "UNREAD");
        Note(LoadGlobalThings, "Load global things", "WLD", "UNREAD");
        Note(LoadRegionGraph, "Load region graph", "WLD", "UNREAD");
    }

    public void ShutdownEngine()
    {
        Note(Shutdown, "Shutdown", "App", "00401B80");
        Stage = EngineStage.Shutdown;
        Mode = EngineMode.None;
    }

    private void RegisterRetailBankTable(GameInstall? install)
    {
        foreach (var (logical, pc) in RetailBanks)
        {
            _banks.Add(logical);
            _banks.Add(pc);
            Note(RegisterRetailBank, "Setup basic retail banks", "Bank",
                logical + " / " + pc);
        }

        if (install is null)
            return;
        foreach (var archive in new[]
                 {
                     Path.Combine(install.DataRoot, "graphics", "pc", "textures.big"),
                     Path.Combine(install.DataRoot, "graphics", "graphics.big"),
                 })
        {
            if (!File.Exists(archive))
                continue;
            using var big = BigArchive.Open(archive);
            foreach (var (logical, pc) in RetailBanks)
            {
                var found = big.SubBanks.Any(b =>
                    b.Name.Equals(pc, StringComparison.OrdinalIgnoreCase) ||
                    b.Name.Equals(logical, StringComparison.OrdinalIgnoreCase));
                if (found)
                    Note(RegisterRetailBank, "Setup basic retail banks", "Bank",
                        "present " + pc + " in " + Path.GetFileName(archive));
            }
        }
    }

    private void ConstructLibrary()
    {
        Note(EngineSingletonGetter, "Setup library", "Engine",
            "009A4EC0 → 0x13CA618");
        Note(GraphicsCtor, "Setup library", "D3D9",
            "009C0880 size 0x2C8 at engine+96");
        Note(Direct3DCreate9Thunk, "Setup library", "D3D9",
            "00BFEFB0 Direct3DCreate9(32)");
        Note(GraphicsInit, "Setup library", "D3D9",
            "009C0E50 GetDeviceCaps vtbl+56");
        Note(CreateDeviceFn, "Setup library", "D3D9",
            "009BF7E0 CreateDevice vtbl+64");
        CreateDeviceFlags = CreateDeviceSoftwareFlags;
        GraphicsCreated = true;
    }

    private void Note(uint va, string stage, string subsystem, string action) =>
        Trace.Add(va, stage, subsystem, action);
}

public enum EngineStage
{
    ProcessEntry,
    CrtStartup,
    WinMain,
    ParseCommandLine,
    SetupInstallFiles,
    SetupLanguage,
    SetupRetailBanks,
    SetupLibrary,
    EndBasicInit,
    StartupVideos,
    Frontend,
    LeaveFrontend,
    Game,
    Shutdown,
    Exited,
}

public enum EngineMode
{
    None,
    RetailFrontend,
    Game,
}

public readonly record struct StartupVideo(
    string RelativePath,
    int Width,
    int Height,
    uint Rgba,
    uint Callback);
