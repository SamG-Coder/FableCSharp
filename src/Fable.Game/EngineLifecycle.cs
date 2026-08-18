using System.Text;
using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Levels;
using Fable.Formats.Tng;
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
    public const uint LoadGlobalThingsSingle = 0x004FE2A0;
    public const uint LoadGlobalThingsPerMap = 0x004FDBC0;
    public const uint LoadGlobalThingsMapFile = 0x004FBF60;
    public const uint SingleGlobalThingsFlagVa = 0x013B8609;
    public const byte DefaultSingleGlobalThingsFlag = 0;
    public const uint GtngExtVa = 0x01244BB4;
    public const uint GtgExtVa = 0x01244BDC;
    public const uint TngExtVa = 0x012442C4;
    public const string GtngExtension = ".gtng";
    public const string GtgExtension = ".gtg";
    public const string TngExtension = ".tng";
    public const uint PlayerManagerGetter = 0x0044C6B0;
    public const uint PlayerManagerVa = 0x013B879C;
    public const uint PlayerManagerApply = 0x0044A530;
    public const uint CreatePlayerSlotFn = 0x0044A1A0;
    public const uint CreatePlayerSlotCtor = 0x0044BC10;
    public const int CreatePlayerSlotSize = 0x22C;
    public const int CreatePlayerSlotCount = 5;
    public const int CreatePlayerActiveCount = 4;
    public const uint PlayerObjectInit = 0x004AE940;
    public const uint LoadRegionGraph = 0x00509982;
    public const uint LoadRegionGraphFn = 0x00506D40;
    public const uint InitRegionGraphFn = 0x00828710;
    public const uint PlayerGuiSingleton = 0x013B878C;
    public const int PlayerGuiGraphPathOffset = 0xA94;
    public const int WorldMapObjectSize = 0xD8;
    public const int WorldMapCellShift = 5;
    public const int WorldMapBound = 0x2000;
    /// <summary>
    /// Game-mode object <c>[esi+36]</c> is the world.
    /// </summary>
    public const int GameWorldOffset = 36;
    /// <summary>
    /// World vtbl+52 <c>004AE8C0</c>:
    /// <c>mov eax, [ecx+20]; ret</c> World Map.
    /// </summary>
    public const int WorldGetMapVtbl = 52;
    public const uint WorldGetMapFn = 0x004AE8C0;
    public const int WorldMapFieldOffset = 20;
    /// <summary>
    /// <c>004FB150</c>: <c>mov eax, [ecx+156]; ret</c>.
    /// Ctor-zeroed. Not a host StartOakVale index.
    /// </summary>
    public const uint GetCurrentRegionIndexFn = 0x004FB150;
    public const int WorldMapCurrentRegionIndexOffset = 156;
    /// <summary>
    /// <c>004FC180</c>:
    /// <c>[ecx+44] + index * 88</c>.
    /// </summary>
    public const uint GetRegionRecordFn = 0x004FC180;
    public const int WorldMapRegionTableOffset = 44;
    public const int NewRegionRecordSize = 88;
    /// <summary>
    /// 88-byte record <c>+36</c> is a refcounted
    /// pointer. Ctor <c>006BC410</c> zeros it.
    /// First pump inc/dec-touches; body unread.
    /// </summary>
    public const int NewRegionObjectOffset = 36;
    public const uint RegionRecordCtor = 0x006BC410;
    public const uint AppendRegionRecord = 0x0051D200;
    public const uint CopyRegionRecord = 0x0051A900;
    /// <summary>
    /// <c>0x13B85F6</c> then <c>0x13B85F5</c>.
    /// BSS default 0 skips
    /// <c>00416268</c> / <c>0041627F</c>.
    /// </summary>
    public const uint NamedStartFlagVa = 0x013B85F6;
    public const uint NamedStartFlag2Va = 0x013B85F5;
    public const byte DefaultNamedStartFlag = 0;
    public const uint NamedStartFn = 0x00416268;
    public const uint NamedStartAltFn = 0x0041627F;
    public const uint NamedStartStringVa = 0x013B865C;
    /// <summary>
    /// <c>0x13B8628</c> nonzero takes
    /// <c>009BFF10</c> instead of the
    /// <c>004FC180</c> record. BSS default 0.
    /// </summary>
    public const uint GamePumpFadeFlagVa = 0x013B8628;
    public const byte DefaultGamePumpFadeFlag = 0;
    public const uint GamePumpPlayerFn = 0x004AE9C0;
    public const uint FrameDtFn = 0x009E1BC0;
    public const uint GamePumpUpdate = 0x004162B5;
    public const uint GamePumpMemlog = 0x00415E85;
    public const uint GamePumpQuitQuery = 0x009A6460;
    /// <summary>
    /// <c>009A57B0</c>:
    /// <c>[engine+148] == GetTickCount</c>
    /// (<c>0x1440378</c>). False skips
    /// vtbl+20 / vtbl+28.
    /// </summary>
    public const uint EngineUpdateGateFn = 0x009A57B0;
    public const int EngineTickOffset = 148;
    public const uint GetTickCountIat = 0x01440378;
    /// <summary>
    /// Game vtbl+20 <c>00418289</c>. Fade /
    /// player <c>004AEBA0</c>; world
    /// <c>0049D9E0</c> is <c>ret</c>.
    /// </summary>
    public const int GameUpdateVtbl = 20;
    public const uint GameUpdateFn = 0x00418289;
    public const uint GameUpdatePlayerFn = 0x004AEBA0;
    public const uint GameUpdateWorldFn = 0x0049D9E0;
    /// <summary>
    /// <c>009E9FB0</c>: <c>[0x13CAA38]==0</c>
    /// → al=0 → always vtbl+28.
    /// </summary>
    public const uint DisplayReadyFn = 0x009E9FB0;
    public const uint DisplayReadyVa = 0x013CAA38;
    /// <summary>
    /// Game vtbl+28 <c>00417001</c>.
    /// </summary>
    public const int GameRenderVtbl = 28;
    public const uint GameRenderFn = 0x00417001;
    public const uint GameModeCtorRenderEnable = 0x00418EC6;
    public const int GameRenderEnableOffset = 90593;
    public const uint FrontEndQueryFn = 0x00416296;
    public const uint GuiBlockQueryFn = 0x00490A22;
    public const uint FadeApplyFn = 0x0041649C;
    public const uint PlayerActionFn = 0x004AEAA0;
    public const int PlayerActionFlagOffset = 9826;
    public const uint GameVtbl24Fn = 0x00416E78;
    public const int GameVtbl24 = 24;
    public const uint ClearGamePlus68Fn = 0x00416047;
    public const uint WorldFrameGetter = 0x0049D870;
    public const uint WorldFrameVa = 0x013B89BC;
    public const uint WorldFrameCopyVa = 0x013B7D70;
    public const uint WorldGetThingFn = 0x0049E1B0;
    public const int WorldThingOffset = 80;
    public const uint StoreActiveThingFn = 0x004C74F0;
    public const uint ActiveThingVa = 0x013B8A1C;
    public const uint RenderStackZeroFn = 0x00415A60;
    public const uint SleepIat = 0x0143FE1C;
    public const uint SleepMsVa = 0x013B8610;
    /// <summary>
    /// Unique increment: <c>004A5E10 inc [0x13B89BC]</c>
    /// at the end of world tick <c>004A5A40</c>.
    /// Thunk <c>00629270</c> is table slot 1
    /// (<c>0x13B92C8</c>). <c>0049DFB0</c>
    /// first walk skips type 1; second walk
    /// (flag) calls slot 1 only.
    /// </summary>
    public const uint AdvanceGameTicksFn = 0x0041726D;
    public const uint DispatchWorldCallbacksFn = 0x0049DFB0;
    public const uint WorldTickFn = 0x004A5A40;
    public const uint WorldTickThunk = 0x00629270;
    public const uint WorldFrameIncSite = 0x004A5E10;
    public const uint WorldTickTableVa = 0x013B9288;
    public const uint WorldTickSlot1FnVa = 0x013B92C8;
    public const int WorldTickSlotStride = 64;
    public const int WorldTickType = 1;
    public const uint CameraBodyFn = 0x004164E0;
    /// <summary>
    /// <c>00417001</c> reads <c>[0x13B8630]</c>
    /// after <c>WorldFrame&gt;1</c>. BSS 0 takes
    /// interpolation <c>0041707E</c>. Positive
    /// clamps to min <c>[0x1375550]=15</c> and
    /// calls <c>004164E0</c>.
    /// </summary>
    public const uint CameraCatchupTicksVa = 0x013B8630;
    public const uint CameraCatchupMinVa = 0x01375550;
    public const int CameraCatchupMin = 15;
    /// <summary>
    /// <c>fmul qword [0x122EDB8]</c> = 1/15.
    /// Loop count is <c>00BFEA70</c> fistp of
    /// <c>arg * (1/15)</c> toward zero.
    /// </summary>
    public const uint CameraStepScaleVa = 0x0122EDB8;
    public const double CameraStepScale = 1.0 / 15.0;
    public const uint CameraInvArgVa = 0x0122DED8;
    public const float CameraInvArgOne = 1f;
    public const uint FistpFn = 0x00BFEA70;
    public const uint WorldCameraApplyFn = 0x0049E080;
    public const uint CameraManagerBlendFn = 0x006B42F0;
    public const uint ThingWalkApplyFn = 0x0051EBD0;
    public const uint DisplayApplyThunk = 0x00435F70;
    public const uint DisplayApplyBodyFn = 0x00435530;
    public const uint CameraTimeFn = 0x00416231;
    public const uint CameraInterpolationFn = 0x0041707E;
    public const int GamePlus80Offset = 80;
    public const int GameCameraSlotOffset = 112;
    public const int CameraRecordDwords = 13;
    public const int CameraRecordSize = 52;
    public const int GamePlus90424Offset = 90424;
    public const int GamePlus104Offset = 104;
    public const int GamePlus90594Offset = 90594;
    /// <summary>
    /// <c>005066E0</c> inserts one ctor-zeroed
    /// 88-byte slot before WLD appends.
    /// Native index 0 is that dummy.
    /// <c>NewRegion N</c> lands at index N.
    /// </summary>
    public const int RegionTableDummyCount = 1;
    /// <summary>
    /// <c>00500540</c>: index → record
    /// (<c>+36</c> optional), build job,
    /// <c>006C27A0</c> / <c>006C2120</c>.
    /// Null <c>+36</c> still continues
    /// (<c>005009BE</c>).
    /// </summary>
    public const uint LoadRegionFn = 0x00500540;
    public const uint BuildLoadJobFn = 0x006C27A0;
    public const uint EnqueueLoadJobFn = 0x006C2120;
    public const uint LevelLoaderUpdate = 0x006C2710;
    public const uint LevelLoaderApply = 0x006C2170;
    public const uint LevelLoaderHasWork = 0x006C20A0;
    public const uint SetRegionAsLoadedFn = 0x004FC8A0;
    public const uint ActivateTopologyFn = 0x004FCBB0;
    public const uint SetMapLoadingFlagFn = 0x004FCFE0;
    public const uint PostRegionLoadVillages = 0x005064C0;
    public const uint InitMiniMapFn = 0x0082BA00;
    public const int WorldMapLevelLoaderOffset = 188;
    public const uint WorldMapSetLevelLoader = 0x004AF160;
    public const int MapRecordSize = 72;
    public const int MapRecordActiveOffset = 38;
    /// <summary>
    /// <c>004FC210</c>: name → native index,
    /// search from 1. 0 = miss / dummy.
    /// </summary>
    public const uint FindRegionByNameFn = 0x004FC210;
    /// <summary>
    /// <c>00487C20</c>: <c>004FC210</c> then
    /// <c>00500540(index,0,1)</c> async.
    /// Caller <c>00449E60</c> reads persist
    /// <c>PlayerRegionName</c> (HEADER) —
    /// continue, not no-save New Game.
    /// </summary>
    public const uint LoadRegionByNameFn = 0x00487C20;
    public const uint LoadRegionByNamePersist = 0x00449E60;
    /// <summary>
    /// <c>00501450</c>: if table count &gt; 1,
    /// <c>00500540(1,0,0)</c> sync. Native
    /// index 1 is LookoutPoint.
    /// </summary>
    public const uint LoadFromFirstRealRegionFn = 0x00501450;
    /// <summary>
    /// <c>004FC190</c>: map → region, search
    /// from 1 via <c>006BBFA0</c> ContainsMap.
    /// </summary>
    public const uint MapToRegionFn = 0x004FC190;
    public const uint RegionContainsMapFn = 0x006BBFA0;
    public const uint LoadRegionAtMapFn = 0x00502500;
    public const uint WorldUpdateFn = 0x004A3740;
    /// <summary>
    /// <c>00B428E0</c> then <c>00B42750</c>
    /// with mode 1. Mode 2 is the neighbour
    /// pointer list.
    /// </summary>
    public const uint SetStaticMapFileForUseFn = 0x00B428E0;
    public const uint OpenStaticMapsFn = 0x00B42750;
    public const uint OpenStaticMapFn = 0x00B42530;
    public const uint ParseMapHeaderFn = 0x00B3EFA0;
    public const uint CloseStaticMapFn = 0x00B3EF40;
    public const uint CreateBackgroundPatchFn = 0x00BE03A0;
    public const uint BuildCurrentPatchFn = 0x00BDD0E0;
    public const int OpenStaticMapsModeOffset = 424;
    public const int OpenStaticMapsUseMode = 1;
    public const int OpenStaticMapsListMode = 2;
    /// <summary>
    /// Compiled WAD <c>.lev</c> header. STB
    /// runtime copy is parsed by
    /// <see cref="LevHeightField"/>.
    /// </summary>
    public const int LevHeaderVersion = LevFile.Version;
    public const uint LevHeaderConstant = LevFile.FormatConstant;

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
    public RegionGraph? Regions { get; private set; }
    public ThingFile? Gtng { get; private set; }
    public ThingFile? GlobalThings { get; private set; }
    public int GlobalThingMapsLoaded { get; private set; }
    /// <summary>
    /// <c>0x13B8609</c>. BSS default 0 →
    /// <c>004FDBC0</c> per-map. Nonzero →
    /// <c>004FE2A0</c> <c>.gtg</c> NEWMAP file.
    /// </summary>
    public bool SingleGlobalThingsFile { get; set; }
    public int PlayerSlotsCreated { get; private set; }
    public int PlayerActiveCount { get; private set; }
    public bool PlayerObjectReady { get; private set; }
    /// <summary>
    /// <c>WorldMap+156</c>. Ctor 0 is the
    /// dummy slot, not LookoutPoint.
    /// </summary>
    public int CurrentRegionIndex { get; private set; }
    public WorldRegion? CurrentRegion { get; private set; }
    /// <summary>
    /// <c>[record+36] != 0</c>. False after
    /// WLD parse; who writes the pointer is unread.
    /// </summary>
    public bool RegionObjectPresent { get; private set; }
    public bool GamePumpFirstDone { get; private set; }
    public int GamePumpFrames { get; private set; }
    /// <summary>
    /// <c>009A57B0</c> last result. After
    /// library construct the host pump is
    /// the tick, so this is true.
    /// </summary>
    public bool EngineUpdateAllowed { get; private set; } = true;
    public int GameUpdateCount { get; private set; }
    public int GameRenderCount { get; private set; }
    /// <summary>
    /// <c>00418EC6</c> ctor writes
    /// <c>[game+90593]=1</c>.
    /// </summary>
    public bool GameRenderEnabled { get; private set; }
    /// <summary>
    /// <c>[player+9826]</c>. Default 0 →
    /// <c>004AEBA0</c> returns 0.
    /// </summary>
    public bool PlayerActionReady { get; set; }
    public bool GameModePaused { get; set; }
    public int GameSleepMs { get; set; }
    public bool FrontEndQuery { get; private set; }
    public bool GuiBlocksUpdate { get; private set; }
    public bool FadeUiActive { get; private set; }
    public bool WorldUpdateRan { get; private set; }
    public bool GameVtbl24Ran { get; private set; }
    public bool RenderBodyRan { get; private set; }
    /// <summary>
    /// <c>0049D870</c> <c>[0x13B89BC]</c>.
    /// </summary>
    public int WorldFrame { get; set; }
    /// <summary>
    /// <c>[0x13B8630]</c>. Default 0.
    /// </summary>
    public int CameraCatchupTicks { get; set; }
    /// <summary>
    /// Game-mode camera tick watermark.
    /// <c>004164E0</c> skips while
    /// <c>[+80] &gt;= [+72]</c>.
    /// </summary>
    public int GamePlus72 { get; set; }
    public int GamePlus76 { get; private set; }
    public int GamePlus80 { get; set; }
    public int GamePlus104 { get; private set; }
    public int GamePlus90424 { get; private set; }
    public bool GamePlus90594 { get; private set; }
    public float GamePlus112 { get; private set; }
    public float GamePlus116 { get; private set; }
    public float GamePlus120 { get; private set; }
    public float GamePlus124 { get; private set; }
    public float GamePlus128 { get; private set; }
    public float GamePlus132 { get; private set; }
    public float GamePlus136 { get; private set; }
    public float GamePlus140 { get; private set; }
    public int GamePlus160 { get; private set; }
    public int CameraBodySteps { get; private set; }
    public int LastCameraLoopCount { get; private set; }
    public float LastCameraBlend { get; private set; }
    public float LastCameraTime { get; private set; }
    public bool CameraInterpolationUnread { get; private set; }
    /// <summary>
    /// Same camera the renderer consumes.
    /// <c>006B42F0</c> slot lerp is PARTIAL.
    /// </summary>
    public ScriptedCamera Camera { get; } = new();
    public IReadOnlyList<int> GameTickTypes => _tickTypes;
    public bool LevelLoaderReady { get; private set; }
    public bool FirstRealRegionLoadDone { get; private set; }
    /// <summary>
    /// Persist <c>PlayerRegionName</c>. Empty on
    /// no-save New Game. Non-empty takes
    /// <c>00487C20</c> instead of <c>00501450</c>.
    /// </summary>
    public string? PlayerRegionName { get; set; }
    public IReadOnlyList<int> PendingLoadIndices => _loadQueue;
    public IReadOnlyList<string> ActivatedMaps => _activatedMaps;
    public int OpenStaticMapsMode { get; private set; }
    public IReadOnlyList<string> OpenedStaticMaps => _openedStaticMaps;
    public IReadOnlyList<OpenedStaticMapBody> OpenedMapBodies => _openedBodies;
    public LevFile? CurrentCompiledLev { get; private set; }
    public LevHeightField? CurrentHeightField { get; private set; }
    /// <summary>
    /// <c>0x13B85F6</c> / <c>0x13B85F5</c>.
    /// Default false matches BSS 0.
    /// </summary>
    public bool UseNamedStart { get; set; }
    public IReadOnlyList<string> CompletedStages => _completed;
    public IReadOnlyList<string> RegisteredBanks => _banks;
    public GameInstall? Install { get; private set; }

    private readonly List<string> _completed = [];
    private readonly List<string> _banks = [];
    private readonly List<int> _loadQueue = [];
    private readonly List<string> _activatedMaps = [];
    private readonly List<string> _openedStaticMaps = [];
    private readonly List<OpenedStaticMapBody> _openedBodies = [];
    private readonly List<int> _tickTypes = [];

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
        {
            PumpGame();
            return true;
        }
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
        CreatePlayers();
        GameRenderEnabled = true;
        Note(GameModeCtorRenderEnable, "InitGame", "GameMode",
            "00418EC6 [game+90593]=1");
        SeedWorldTick();
        Mode = EngineMode.Game;
        Stage = EngineStage.Game;
        WorldFileName = FinalAlbionWld;
    }

    /// <summary>
    /// <c>005066E0</c> constructs the 0xD8 map
    /// object (shift 5, bound 0x2000).
    /// <c>00507C30</c> vtbl+12 is
    /// <c>Load .wld file</c>: token-switch parse
    /// of <c>FinalAlbion.wld</c>. Then GTNG,
    /// global things, region graph.
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
        LoadGtngFile();
        LoadGlobalThingsFile();
        LoadRegionGraphFile();
    }

    /// <summary>
    /// <c>0050959F</c>: WLD stem + <c>.gtng</c>
    /// (<c>0x1244BB4</c>). Missing file
    /// <c>00999230</c> skips to global things.
    /// TLC has no <c>.gtng</c>.
    /// </summary>
    public void LoadGtngFile()
    {
        Note(LoadGtng, "Load GTNG", "WLD", "0050959F stem+.gtng");
        if (Install is null)
            return;
        var path = Path.ChangeExtension(Install.WorldPath, GtngExtension);
        if (!File.Exists(path))
        {
            Note(LoadGtng, "Load GTNG", "WLD", "missing " + Path.GetFileName(path));
            return;
        }

        Gtng = ThingFile.Load(path);
        Note(LoadGtng, "Load GTNG", "WLD",
            $"things={Gtng.Things.Count()} {Path.GetFileName(path)}");
    }

    /// <summary>
    /// <c>00509859</c>: <c>[0x13B8609]</c> default 0
    /// → <c>004FDBC0</c> per-map <c>.tng</c>
    /// (<c>004FBF60</c> / <c>004FAFF0</c>).
    /// Nonzero → <c>004FE2A0</c> <c>.gtg</c>
    /// NEWMAP/ENDMAP
    /// LoadAllLoadableGlobalThingsFromSingleFile.
    /// </summary>
    public void LoadGlobalThingsFile()
    {
        Note(LoadGlobalThings, "Load global things", "WLD",
            SingleGlobalThingsFile ? "004FE2A0 .gtg" : "004FDBC0 per-map .tng");
        if (Install is null)
            return;
        if (SingleGlobalThingsFile)
        {
            var path = Path.ChangeExtension(Install.WorldPath, GtgExtension);
            if (!File.Exists(path))
            {
                Note(LoadGlobalThingsSingle, "Load global things", "WLD", "missing .gtg");
                return;
            }

            GlobalThings = ThingFile.Load(path);
            Note(LoadGlobalThingsSingle, "LoadAllLoadableGlobalThingsFromSingleFile", "WLD",
                $"things={GlobalThings.Things.Count()} {Path.GetFileName(path)}");
            return;
        }

        Note(LoadGlobalThingsPerMap, "Loading global things", "WLD", "004FDBC0");
        if (World is null)
            return;
        BbbArchive? wad = null;
        if (File.Exists(Install.WadPath))
            wad = BbbArchive.Open(Install.WadPath);
        try
        {
            var loaded = new List<ThingInstance>();
            foreach (var map in World.Maps)
            {
                if (!map.LoadedOnPlayerProximity)
                    continue;
                var tng = TryLoadMapTng(map, wad);
                if (tng is null)
                    continue;
                loaded.AddRange(tng.Things);
                GlobalThingMapsLoaded++;
            }

            if (loaded.Count == 0)
            {
                Note(LoadGlobalThingsMapFile, "Load global things", "WLD", "no proximity tng");
                return;
            }

            GlobalThings = new ThingFile { Version = 2, Sections = [new ThingSection { Name = "GLOBAL", Things = loaded }] };
            Note(LoadGlobalThingsMapFile, "Load global things", "WLD",
                $"maps={GlobalThingMapsLoaded} things={loaded.Count}");
        }
        finally
        {
            wad?.Dispose();
        }
    }

    /// <summary>
    /// <c>004166A8</c>: <c>0044C6B0</c> singleton
    /// <c>0x13B879C</c>, <c>0044A530</c> creates
    /// slots 0–4 (<c>0044A1A0</c> / <c>0x22C</c>),
    /// <c>[+24]=4</c>, then <c>004AE940</c> at
    /// game+80568. Not hero_swap_*.tng (0044A3B0).
    /// </summary>
    public void CreatePlayers()
    {
        Note(PlayerManagerGetter, "Create Players", "Player", "0044C6B0 [0x13B879C]");
        Note(PlayerManagerApply, "Create Players", "Player", "0044A530 slots 0-4");
        for (var i = 0; i < CreatePlayerSlotCount; i++)
            Note(CreatePlayerSlotFn, "Create Players", "Player",
                $"slot {i} ctor 0044BC10 size 0x22C");
        PlayerSlotsCreated = CreatePlayerSlotCount;
        PlayerActiveCount = CreatePlayerActiveCount;
        Note(PlayerObjectInit, "Create Players", "Player", "004AE940 game+80568");
        PlayerObjectReady = true;
        Note(CreatePlayersFn, "Create Players", "Player",
            $"slots={PlayerSlotsCreated} active={PlayerActiveCount}");
    }

    /// <summary>
    /// <c>004189C2</c> game-mode vtbl+8.
    /// First iteration: skip named start
    /// (flags default 0), then world vtbl+52
    /// → <c>004FB150</c> → <c>004FC180</c>.
    /// Not <c>00DBDE40</c>.
    /// </summary>
    public void PumpGame()
    {
        if (Stage != EngineStage.Game)
            return;
        GamePumpFrames++;
        if (GamePumpFirstDone)
        {
            EnqueueAfterDummy();
            PumpGameUpdate();
            return;
        }

        Note(GamePump, "GamePump", "Game", "004189C2 vtbl+8");
        Note(GamePumpPlayerFn, "GamePump", "Player", "004AE9C0 game+80568");
        Note(FrameDtFn, "GamePump", "Time", "009E1BC0 FrameDt");
        if (UseNamedStart)
        {
            Note(NamedStartFn, "GamePump", "Region", "00416268 named start");
            GamePumpFirstDone = true;
            PumpGameUpdate();
            return;
        }

        Note(WorldGetMapFn, "GamePump", "World",
            "vtbl+52 004AE8C0 [world+20]");
        Note(GetCurrentRegionIndexFn, "GamePump", "Region",
            $"004FB150 [WorldMap+156]={CurrentRegionIndex}");
        Note(GetRegionRecordFn, "GamePump", "Region",
            $"004FC180 [WorldMap+44]+{CurrentRegionIndex}*{NewRegionRecordSize}");
        ActivateCurrentRegion();
        GamePumpFirstDone = true;
        Note(GamePumpMemlog, "GamePump", "Game", "00415E85 memlog");
        PumpGameUpdate();
    }

    /// <summary>
    /// <c>004162B5</c> inner frame. Not map
    /// load. <c>009A57B0</c> false skips
    /// vtbl+20 / vtbl+28.
    /// </summary>
    public void PumpGameUpdate()
    {
        Note(GamePumpUpdate, "GamePump", "Update", "004162B5");
        Note(EngineSingletonGetter, "GamePump", "Engine", "009A4EC0 0x13CA618");
        EngineUpdateAllowed = EvaluateEngineUpdateGate();
        Note(EngineUpdateGateFn, "GamePump", "Engine",
            EngineUpdateAllowed ? "009A57B0 allow" : "009A57B0 skip");
        if (!EngineUpdateAllowed)
            return;

        Note(FrameDtFn, "GamePump", "Time", "009E1BC0 FrameDt");
        UpdateGameMode();
        Note(DisplayReadyFn, "GamePump", "Display",
            "009E9FB0 [0x13CAA38] default 0");
        RenderGameMode();
    }

    /// <summary>
    /// <c>009A57B0</c>. Host <see cref="Pump"/>
    /// is the tick after library construct.
    /// </summary>
    public bool EvaluateEngineUpdateGate()
    {
        Note(GetTickCountIat, "GamePump", "Engine", "GetTickCount IAT 0x1440378");
        return GraphicsCreated;
    }

    /// <summary>
    /// Game vtbl+20 <c>00418289</c>.
    /// Sleep skip, fade only if frontend
    /// and GUI both block, else
    /// <c>004AEBA0</c> at game+80568.
    /// </summary>
    public void UpdateGameMode()
    {
        Note(GameUpdateFn, "GamePump", "Update", "vtbl+20 00418289");
        if (GameSleepMs > 0)
            Note(SleepIat, "GamePump", "Update", $"Sleep {GameSleepMs}");
        FrontEndQuery = QueryFrontEnd();
        GuiBlocksUpdate = false;
        Note(FrontEndQueryFn, "GamePump", "Update",
            FrontEndQuery ? "00416296 true" : "00416296 false");
        Note(GuiBlockQueryFn, "GamePump", "Update", "00490A22 default 0");
        if (FrontEndQuery && GuiBlocksUpdate)
        {
            FadeUiActive = true;
            Note(FadeApplyFn, "GamePump", "Update", "0041649C fade");
        }
        else
            FadeUiActive = false;

        Note(FrameDtFn, "GamePump", "Time", "009E1BC0 [game+90544]");
        if (GameModePaused)
            Note(GameUpdateFn, "GamePump", "Update", "[game+90480] paused");
        else
        {
            Note(GameUpdatePlayerFn, "GamePump", "Player",
                $"004AEBA0 +{PlayerActionFlagOffset}={PlayerActionReady}");
            if (PlayerActionReady)
            {
                Note(PlayerActionFn, "GamePump", "Player", "004AEAA0");
                Note(GameUpdateWorldFn, "GamePump", "World", "0049D9E0 ret");
                WorldUpdateRan = true;
                Note(GameVtbl24Fn, "GamePump", "Update", "vtbl+24 00416E78");
                GameVtbl24Ran = true;
                Note(ClearGamePlus68Fn, "GamePump", "Update", "00416047 [game+68]=0");
                AdvanceGameTicks();
            }
        }

        Note(WorldFrameGetter, "GamePump", "World",
            $"0049D870 [0x13B89BC]={WorldFrame}");
        Note(WorldFrameCopyVa, "GamePump", "World", "0x13B7D70");
        GameUpdateCount++;
    }

    /// <summary>
    /// <c>00416296</c>: empty GUI list →
    /// <c>009F5250</c> miss → invert → true.
    /// </summary>
    public bool QueryFrontEnd() => true;

    /// <summary>
    /// Game vtbl+28 <c>00417001</c>.
    /// World vtbl+12 / <c>004C74F0</c>, then
    /// <c>[90593]</c> and <c>WorldFrame&lt;=1</c>
    /// skip the camera body.
    /// </summary>
    public void RenderGameMode()
    {
        Note(GameRenderFn, "GamePump", "Render", "vtbl+28 00417001");
        Note(RenderStackZeroFn, "GamePump", "Render", "00415A60");
        Note(WorldGetThingFn, "GamePump", "World", "0049E1B0 [world+80]");
        Note(StoreActiveThingFn, "GamePump", "World", "004C74F0 [0x13B8A1C]");
        GameRenderCount++;
        if (!EngineUpdateAllowed)
            return;
        if (!GameRenderEnabled)
        {
            Note(GameRenderFn, "GamePump", "Render", "[game+90593]=0 skip");
            return;
        }

        Note(WorldFrameGetter, "GamePump", "Render",
            $"0049D870 frame={WorldFrame}");
        if (WorldFrame <= 1)
        {
            Note(GameRenderFn, "GamePump", "Render",
                "WorldFrame<=1 skip camera body");
            return;
        }

        RenderBodyRan = true;
        if (CameraCatchupTicks <= 0)
        {
            Note(CameraInterpolationFn, "GamePump", "Render",
                "0041707E interpolation UNREAD");
            CameraInterpolationUnread = true;
            return;
        }

        if (CameraCatchupTicks < CameraCatchupMin)
        {
            CameraCatchupTicks = CameraCatchupMin;
            Note(CameraCatchupTicksVa, "GamePump", "Render",
                $"clamp [0x13B8630]={CameraCatchupTicks}");
        }

        ApplyCameraBody(CameraCatchupTicks);
        GamePlus90594 = true;
        Note(CameraBodyFn, "GamePump", "Render", "[game+90594]=1");
    }

    /// <summary>
    /// <c>004164E0</c>. <paramref name="arg"/> is
    /// the pushed catch-up tick count.
    /// </summary>
    public void ApplyCameraBody(int arg)
    {
        Note(CameraBodyFn, "GamePump", "Render", "004164E0 camera body");
        var count = FistpTowardZero(arg * CameraStepScale);
        Note(FistpFn, "GamePump", "Render",
            $"00BFEA70 count={count} arg={arg}");
        LastCameraLoopCount = count;
        if (GamePlus80 >= GamePlus72)
        {
            Note(CameraBodyFn, "GamePump", "Render",
                $"[game+80]={GamePlus80}>=[game+72]={GamePlus72} skip");
            return;
        }

        if (count <= 0)
        {
            Note(CameraBodyFn, "GamePump", "Render", "count<=0 skip");
            return;
        }

        var invArg = CameraInvArgOne / arg;
        for (var i = 0; i < count; i++)
        {
            Note(RenderStackZeroFn, "GamePump", "Render", "00415A60 record");
            var tBlend = i / (float)count;
            var tTime = (i + 1) * invArg;
            var old112 = GamePlus112;
            var old116 = GamePlus116;
            var old128 = GamePlus128;
            var old132 = GamePlus132;
            GamePlus112 = tTime;
            GamePlus116 = tBlend;
            GamePlus120 = old112;
            GamePlus124 = old116;
            GamePlus128 = tTime;
            GamePlus132 = tBlend;
            GamePlus136 = old128;
            GamePlus140 = old132;
            GamePlus160 = GamePlus72;
            LastCameraTime = tTime;
            LastCameraBlend = tBlend;
            ApplyWorldCamera(tBlend);
            Note(CameraTimeFn, "GamePump", "Time",
                "00416231 009E1BC0-[game+96]");
            ApplyDisplayCamera();
            GamePlus90424++;
            GamePlus80 = GamePlus72;
            GamePlus104 = 0;
            CameraBodySteps++;
            Note(CameraBodyFn, "GamePump", "Render",
                $"step {i + 1}/{count} t={tBlend}");
        }
    }

    /// <summary>
    /// <c>00BFEA70</c> toward-zero fistp.
    /// </summary>
    public static int FistpTowardZero(double value) =>
        value >= 0 ? (int)value : -(int)(-value);

    /// <summary>
    /// <c>0049E080</c>: store thing, walk
    /// <c>0051EBD0</c>, blend
    /// <c>006B42F0(world+24, t)</c>.
    /// Slot lerp body is PARTIAL.
    /// </summary>
    public void ApplyWorldCamera(float tBlend)
    {
        if (tBlend < 0f)
            tBlend = 0f;
        else if (tBlend > 1f)
            tBlend = 1f;
        Note(WorldCameraApplyFn, "GamePump", "Camera", "0049E080");
        Note(StoreActiveThingFn, "GamePump", "World", "004C74F0");
        Note(ThingWalkApplyFn, "GamePump", "World", "0051EBD0");
        Note(CameraManagerBlendFn, "GamePump", "Camera",
            $"006B42F0 t={tBlend}");
    }

    /// <summary>
    /// <c>00435F70</c> jmp <c>00435530</c>.
    /// Display frame body PARTIAL.
    /// </summary>
    public void ApplyDisplayCamera()
    {
        Note(DisplayApplyThunk, "GamePump", "Display",
            "00435F70 jmp 00435530");
        Note(DisplayApplyBodyFn, "GamePump", "Display", "00435530");
    }

    /// <summary>
    /// Table init <c>0121BA2D</c> stores
    /// <c>00629270</c> at slot 1. Seeded so
    /// <c>0049DFB0</c> second walk can fire.
    /// </summary>
    public void SeedWorldTick()
    {
        if (_tickTypes.Contains(WorldTickType))
            return;
        _tickTypes.Add(WorldTickType);
        Note(WorldTickSlot1FnVa, "GamePump", "World",
            "0121BA2D [0x13B92C8]=00629270 type 1");
    }

    /// <summary>
    /// <c>0041726D</c>: walk game+164. If
    /// <c>[+76]==[+72]</c> (ctor 0), flag 1
    /// and <c>0049DFB0</c> second walk calls
    /// slot 1.
    /// </summary>
    public void AdvanceGameTicks()
    {
        Note(AdvanceGameTicksFn, "GamePump", "World", "0041726D");
        if (_tickTypes.Count == 0)
        {
            Note(AdvanceGameTicksFn, "GamePump", "World", "009F1750 empty");
            return;
        }

        var flag = GamePlus76 == GamePlus72;
        Note(DispatchWorldCallbacksFn, "GamePump", "World",
            $"0049DFB0 flag={(flag ? 1 : 0)} types={_tickTypes.Count}");
        if (!flag)
            return;
        foreach (var type in _tickTypes)
        {
            if (type != WorldTickType)
                continue;
            Note(WorldTickThunk, "GamePump", "World", "00629270 slot 1");
            TickWorld();
        }
    }

    /// <summary>
    /// <c>004A5A40</c> ends at
    /// <c>004A5E10 inc [0x13B89BC]</c>.
    /// </summary>
    public void TickWorld()
    {
        Note(WorldTickFn, "GamePump", "World", "004A5A40");
        WorldFrame++;
        Note(WorldFrameIncSite, "GamePump", "World",
            $"004A5E10 inc WorldFrame={WorldFrame}");
    }

    /// <summary>
    /// Native table index is the
    /// <c>NewRegion N</c> token. Index 0 is
    /// the <c>005066E0</c> dummy, not
    /// LookoutPoint. <c>[record+36]</c> is
    /// still null after parse.
    /// </summary>
    public void ActivateCurrentRegion()
    {
        CurrentRegion = RegionAtNativeIndex(CurrentRegionIndex);
        RegionObjectPresent = false;
        if (CurrentRegionIndex == 0)
        {
            Note(GetRegionRecordFn, "GamePump", "Region",
                "index=0 dummy 005066E0 record+36 null");
            Note(RegionRecordCtor, "GamePump", "Region",
                "006BC410 +36 zero; touch skipped");
            return;
        }

        if (CurrentRegion is null)
        {
            Note(GetRegionRecordFn, "GamePump", "Region",
                $"index={CurrentRegionIndex} no record");
            return;
        }

        Note(GetRegionRecordFn, "GamePump", "Region",
            $"index={CurrentRegionIndex} {CurrentRegion.RegionName} record+36 null");
        Note(RegionRecordCtor, "GamePump", "Region",
            "006BC410 +36 zero; touch skipped");
    }

    public WorldRegion? RegionAtNativeIndex(int index)
    {
        if (World is null || index <= 0)
            return null;
        foreach (var region in World.Regions)
        {
            if (region.Index == index)
                return region;
        }

        return null;
    }

    /// <summary>
    /// <c>004FC210</c>. Starts at index 1.
    /// </summary>
    public int FindRegionIndexByName(string? name)
    {
        if (World is null || string.IsNullOrEmpty(name))
            return 0;
        foreach (var region in World.Regions)
        {
            if (region.RegionName.Equals(name, StringComparison.OrdinalIgnoreCase))
                return region.Index;
        }

        return 0;
    }

    /// <summary>
    /// <c>00487C20</c> / <c>004FC210</c>.
    /// Third arg 1 = async.
    /// </summary>
    public void LoadRegionByName(string? name)
    {
        Note(LoadRegionByNameFn, "LevelLoader", "Region", "00487C20");
        var index = FindRegionIndexByName(name);
        Note(FindRegionByNameFn, "LevelLoader", "Region",
            $"004FC210 {name ?? "null"} index={index}");
        if (index == 0)
            return;
        RequestLoadRegion(index, sync: false);
    }

    /// <summary>
    /// <c>00501450</c>: table count &gt; 1 then
    /// <c>00500540(1,0,0)</c>. No-save New Game
    /// after the dummy first pump.
    /// </summary>
    public void LoadFromFirstRealRegion()
    {
        var count = (World?.Regions.Count ?? 0) + RegionTableDummyCount;
        Note(LoadFromFirstRealRegionFn, "LevelLoader", "Region",
            $"00501450 count={count}");
        if (count <= 1)
            return;
        RequestLoadRegion(1, sync: true);
    }

    /// <summary>
    /// After dummy <c>004189C2</c>: persist
    /// name uses <c>00487C20</c>, else
    /// <c>00501450</c> index 1. Not
    /// <c>00DBDE40</c>.
    /// </summary>
    public void EnqueueAfterDummy()
    {
        if (FirstRealRegionLoadDone || UseNamedStart)
            return;
        FirstRealRegionLoadDone = true;
        if (!string.IsNullOrEmpty(PlayerRegionName))
        {
            Note(LoadRegionByNamePersist, "LevelLoader", "Region",
                "00449E60 PlayerRegionName");
            LoadRegionByName(PlayerRegionName);
            if (_loadQueue.Count > 0)
                PumpLevelLoader();
            return;
        }

        LoadFromFirstRealRegion();
    }

    /// <summary>
    /// <c>00500540</c> then <c>006C27A0</c> /
    /// <c>006C2120</c>. Does not invent
    /// StartOakVale. <c>sync</c> is the third
    /// arg: 0 pumps <c>006C2710</c> until empty.
    /// </summary>
    public void RequestLoadRegion(int index, bool sync = true)
    {
        EnsureLevelLoader();
        Note(LoadRegionFn, "LevelLoader", "Region",
            $"00500540 index={index} record+36 unread");
        var region = RegionAtNativeIndex(index);
        if (index != 0 && region is null)
        {
            Note(LoadRegionFn, "LevelLoader", "Region",
                $"index={index} missing");
            return;
        }

        Note(BuildLoadJobFn, "LevelLoader", "Region",
            $"006C27A0 maps={region?.ContainsMaps.Count ?? 0} +28={index}");
        _loadQueue.Add(index);
        Note(EnqueueLoadJobFn, "LevelLoader", "Region",
            $"006C2120 queue={_loadQueue.Count}");
        if (sync)
            PumpLevelLoader();
    }

    /// <summary>
    /// <c>006C2710</c> "Level loader update".
    /// </summary>
    public void PumpLevelLoader()
    {
        Note(LevelLoaderUpdate, "LevelLoader", "Region", "006C2710 Level loader update");
        while (_loadQueue.Count > 0)
        {
            Note(LevelLoaderHasWork, "LevelLoader", "Region", "006C20A0 nonempty");
            ApplyLoadJob(_loadQueue[0]);
            _loadQueue.RemoveAt(0);
        }
    }

    /// <summary>
    /// <c>004FC8A0</c> writes
    /// <c>WorldMap+156</c> then Initialise MiniMap.
    /// </summary>
    public void SetRegionAsLoaded(int index)
    {
        CurrentRegionIndex = index;
        Note(SetRegionAsLoadedFn, "LevelLoader", "Region",
            "SetRegionAsLoaded: Initialise MiniMap");
        ActivateCurrentRegion();
        var name = CurrentRegion?.RegionName ?? (index == 0 ? "dummy" : "?");
        Note(SetRegionAsLoadedFn, "LevelLoader", "Region",
            $"index={index} {name}");
        Note(InitMiniMapFn, "LevelLoader", "Region", "0082BA00 Initialise MiniMap");
        Note(PostRegionLoadVillages, "LevelLoader", "Region",
            "005064C0 Post Region Load Villages");
        OpenStaticMapsForCurrentRegion();
    }

    /// <summary>
    /// <c>00B428E0</c> <c>SetStaticMapFileForUse</c>
    /// then <c>00B42750</c> mode 1. Map set is
    /// existing <see cref="WorldGeometry.StaticMapsAround"/>.
    /// </summary>
    public void OpenStaticMapsForCurrentRegion()
    {
        Note(SetStaticMapFileForUseFn, "StaticMap", "WLD",
            "SetStaticMapFileForUse: CloseStaticMapFile");
        Note(SetStaticMapFileForUseFn, "StaticMap", "WLD",
            "SetStaticMapFileForUse: EnablePoolAllocation");
        Note(SetStaticMapFileForUseFn, "StaticMap", "WLD",
            "SetStaticMapFileForUse: OpenStaticMaps");
        OpenStaticMapsMode = OpenStaticMapsUseMode;
        Note(OpenStaticMapsFn, "StaticMap", "WLD",
            $"00B42750 mode={OpenStaticMapsMode} [+424]");
        _openedStaticMaps.Clear();
        _openedBodies.Clear();
        CurrentCompiledLev = null;
        CurrentHeightField = null;
        if (Install is null || World is null || CurrentRegion is null)
            return;

        var primary = CurrentRegion.ContainsMaps.FirstOrDefault(m =>
                          m.Equals(CurrentRegion.RegionName, StringComparison.OrdinalIgnoreCase))
                      ?? CurrentRegion.ContainsMaps.FirstOrDefault()
                      ?? CurrentRegion.RegionName;
        foreach (var map in WorldGeometry.StaticMapsAround(World, Install, primary))
            _openedStaticMaps.Add(map.ScriptName);

        Note(OpenStaticMapsFn, "StaticMap", "WLD",
            $"opened={_openedStaticMaps.Count} primary={primary}");
        OpenStaticMap(primary);
    }

    /// <summary>
    /// <c>00B42530</c>: STB name lookup
    /// <c>009CCDC0</c>, blob copy, header
    /// <c>00B3EFA0</c>, version words, then
    /// mode-1 patch <c>00BE03A0</c> /
    /// <c>00BDD0E0</c>. Compiled WAD
    /// <c>.lev</c> is <see cref="LevFile"/>;
    /// STB runtime copy is
    /// <see cref="LevHeightField"/>.
    /// </summary>
    public void OpenStaticMap(string name)
    {
        Note(OpenStaticMapFn, "StaticMap", "WLD", "00B42530 " + name);
        Note(CloseStaticMapFn, "StaticMap", "WLD", "00B3EF40");
        if (Install is null || World is null)
            return;

        BbbArchive? wad = null;
        StbArchive? stb = null;
        try
        {
            if (File.Exists(Install.WadPath))
                wad = BbbArchive.Open(Install.WadPath);
            if (File.Exists(Install.RuntimeStbPath))
                stb = StbArchive.Open(Install.RuntimeStbPath);

            var map = World.FindMap(name);
            var stem = map?.FileStem ?? name;
            var stbEntry = stb?.FindLev(stem) ?? stb?.FindLev(name);
            var stbBytes = stbEntry is null ? null : stb!.Read(stbEntry);
            Note(OpenStaticMapFn, "StaticMap", "WLD",
                stbBytes is null
                    ? "009CCDC0 miss " + name
                    : $"009CCDC0 stb={stbBytes.Length} {name}");

            var compiledEntry = wad?.Find(stem + ".lev")
                                ?? wad?.Find(name + ".lev")
                                ?? (map is null
                                    ? null
                                    : wad?.Find(map.LevelName));
            LevFile? compiled = null;
            if (compiledEntry is not null)
                compiled = LevFile.Parse(wad!.Read(compiledEntry));

            var version = compiled is null ? 0 : LevFile.Version;
            var constant = compiled is null ? 0u : LevFile.FormatConstant;
            Note(ParseMapHeaderFn, "StaticMap", "WLD",
                $"00B3EFA0 version={version} constant=0x{constant:X}");

            LevHeightField? height = null;
            if (stbBytes is not null && map is not null)
            {
                var width = compiled?.GridWidth ?? 128;
                var heightCells = compiled?.GridHeight ?? 128;
                height = LevHeightField.Parse(stbBytes, map.MapX, map.MapY, width, heightCells);
            }

            if (OpenStaticMapsMode == OpenStaticMapsUseMode)
            {
                Note(CreateBackgroundPatchFn, "StaticMap", "WLD", "00BE03A0");
                Note(BuildCurrentPatchFn, "StaticMap", "WLD", "00BDD0E0");
            }

            var body = new OpenedStaticMapBody(
                name,
                stbBytes?.Length ?? 0,
                compiled?.Raw.Length ?? 0,
                compiled?.GridWidth ?? height?.FineWidth ?? 0,
                compiled?.GridHeight ?? height?.FineHeight ?? 0,
                height?.SampleCount ?? 0,
                compiled is null ? version : LevFile.Version,
                constant);
            _openedBodies.Add(body);
            CurrentCompiledLev = compiled;
            CurrentHeightField = height;
            Note(OpenStaticMapFn, "StaticMap", "WLD",
                $"body {name} lev={body.CompiledSize} stb={body.StbSize} " +
                $"{body.GridWidth}x{body.GridHeight} samples={body.HeightSamples}");
        }
        finally
        {
            wad?.Dispose();
            stb?.Dispose();
        }
    }

    private void EnsureLevelLoader()
    {
        if (LevelLoaderReady)
            return;
        Note(WorldMapSetLevelLoader, "LevelLoader", "Region",
            "004AF160 [WorldMap+188] CLevelLoader");
        LevelLoaderReady = true;
    }

    private void ApplyLoadJob(int index)
    {
        var region = RegionAtNativeIndex(index);
        Note(LevelLoaderApply, "LevelLoader", "Region",
            $"006C2170 index={index} {region?.RegionName ?? (index == 0 ? "dummy" : "?")}");
        if (region is not null)
        {
            foreach (var map in region.ContainsMaps)
            {
                Note(LevelLoaderApply, "LevelLoader", "Region", "Loading topology " + map);
                if (!_activatedMaps.Exists(m =>
                        m.Equals(map, StringComparison.OrdinalIgnoreCase)))
                    _activatedMaps.Add(map);
                Note(ActivateTopologyFn, "LevelLoader", "Region",
                    $"004FCBB0 {map} +38=1");
                Note(SetMapLoadingFlagFn, "LevelLoader", "Region",
                    $"004FCFE0 {map} +39");
                Note(LevelLoaderApply, "LevelLoader", "Region", "Loading objects " + map);
            }

            Note(LevelLoaderApply, "LevelLoader", "Region",
                "Region Level Files: Activate Topology");
        }

        SetRegionAsLoaded(index);
    }

    private ThingFile? TryLoadMapTng(WorldMap map, BbbArchive? wad)
    {
        if (Install is null)
            return null;
        var stem = map.FileStem;
        var loose = Path.Combine(Install.LooseLevelsDirectory, stem + TngExtension);
        if (File.Exists(loose))
            return ThingFile.Load(loose);
        if (wad is null)
            return null;
        var entry = wad.Find(stem + TngExtension)
                    ?? wad.Find(map.LevelName.Replace(".lev", TngExtension, StringComparison.OrdinalIgnoreCase));
        return entry is null ? null : ThingFile.Parse(Encoding.ASCII.GetString(wad.Read(entry)));
    }

    /// <summary>
    /// <c>00509982</c> → <c>00506D40</c> with the
    /// path at <c>PLAYER_GUI_PC+0xA94</c>.
    /// <c>00828710</c> is Initialise Region Graph.
    /// TLC file is
    /// <c>Misc\FinalAlbion_StartingRegionGraph.txt</c>.
    /// </summary>
    public void LoadRegionGraphFile()
    {
        Note(LoadRegionGraph, "Load region graph", "WLD",
            "00506D40 path PLAYER_GUI_PC+0xA94");
        Note(InitRegionGraphFn, "Initialise Region Graph", "WLD", "00828710");
        if (Install is null)
            return;
        var path = Install.StartingRegionGraphPath;
        if (!File.Exists(path))
        {
            Note(LoadRegionGraphFn, "Load region graph", "WLD", "missing");
            return;
        }

        Regions = RegionGraph.Load(path);
        Note(LoadRegionGraphFn, "Load region graph", "WLD",
            $"nodes={Regions.Neighbors.Count} {Path.GetFileName(path)}");
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

/// <summary>
/// One <c>00B42530</c> open: compiled WAD
/// <c>.lev</c> plus STB height field.
/// </summary>
public readonly record struct OpenedStaticMapBody(
    string Name,
    int StbSize,
    int CompiledSize,
    int GridWidth,
    int GridHeight,
    int HeightSamples,
    int HeaderVersion,
    uint HeaderConstant);
