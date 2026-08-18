using System.Numerics;
using Fable.Core;
using Fable.Formats.Defs;
using Fable.Formats.Levels;
using Fable.Formats.Qst;
using Fable.Formats.Scene;
using Fable.Formats.Tng;
using Fable.Formats.Meshes;
using Fable.Formats.Wld;
using Fable.Game.Scripting;
using Fable.Render;

namespace Fable.Game;

/// <summary>
/// Recovered Fable.exe process entry → WinMain →
/// named bootstrap → library/D3D9 → mode loop.
/// Walk is <c>docs/runtime/FORWARD_TREE.md</c>.
/// Do not start at <c>00DBDE40</c> / New Game.
/// </summary>
public sealed class EngineLifecycle : IDisposable
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
    /// <summary>
    /// <c>00403079</c> copies
    /// <c>[0x137545C]</c> / <c>[0x1375460]</c>
    /// into the options record then
    /// <c>009A6610</c> → <c>009C0E50</c>.
    /// PE defaults are 1024×768. Init
    /// clamps each axis to min 32.
    /// </summary>
    public const uint DisplayWidthVa = 0x0137545C;
    public const uint DisplayHeightVa = 0x01375460;
    public const uint DisplayMaxVa = 0x0137546C;
    public const uint DisplayBppVa = 0x01375470;
    public const int DisplayDefaultWidth = 1024;
    public const int DisplayDefaultHeight = 768;
    public const int DisplayMaxDimension = 2048;
    public const int DisplayDefaultBpp = 16;
    public const int GraphicsMinDimension = 32;
    /// <summary>
    /// <c>004023F0</c> looks up
    /// <c>TEXT_GUI_WINDOW_TITLE</c>; PE
    /// UTF-16 fallback at <c>0x122D83C</c>
    /// is "Fable - The Lost Chapters".
    /// </summary>
    public const uint WindowTitleFn = 0x004023F0;
    public const uint WindowTitleVa = 0x0122D83C;
    public const string WindowTitleId = "TEXT_GUI_WINDOW_TITLE";
    public const string WindowTitleDefault = "Fable - The Lost Chapters";
    /// <summary>
    /// <c>0042E3EE</c> walks
    /// <c>[0x13B8388]</c> (engine+88 from
    /// ProbeGraphics). Poll <c>009F4ED0</c>.
    /// Event classify <c>00A03B40</c>.
    /// Frontend New Game is message 15,
    /// not WASD.
    /// </summary>
    public const uint InputDeviceVa = 0x013B8388;
    public const uint InputPollFn = 0x009F4ED0;
    public const uint InputEventFn = 0x00A03B40;
    public const uint InputActionGetter = EngineInput.Getter;
    public const uint InputActionCtor = EngineInput.Ctor;
    public const uint InputActionVtbl = EngineInput.Vtbl;
    public const uint InputActionApply = EngineInput.ActionApply;
    public const uint InputBindDefaults = EngineInput.BindDefaults;
    public const uint InputEventKeyFn = EngineInput.EventKeyFn;
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
    /// <summary>
    /// <c>0042F2AA</c> clears
    /// <c>[0x1375448]</c> then
    /// <c>0042EBB6</c> teardown.
    /// New Game <c>+41!=0</c> skips
    /// audio stop. Display
    /// <c>009BE420</c>+<c>009BEEB0</c>.
    /// </summary>
    public const uint LeaveFrontendTeardownFn = 0x0042EBB6;
    public const uint LeaveFrontendPathFn = 0x00404490;
    public const uint LeaveFrontendRecordFn = 0x004131A0;
    public const uint LeaveFrontendClearFn = 0x009BE420;
    public const uint LeaveFrontendAudioVtbl = 72;
    public const int LeaveFrontendAudioMs = 0x1F4;
    public const uint GameSingletonVa = 0x013B86A0;
    public const uint RetailSuccessorVa = 0x013B7D58;
    public const int GameReadyOffset = 90592;
    public const uint SkipParticlesVa = 0x013B8648;
    public const byte SkipParticlesFirstSeen = 0;
    /// <summary>
    /// <c>00595582</c> singleton at
    /// <c>[0x13B8B5C]</c>. Size <c>0xE0</c>,
    /// ctor <c>005953E2</c>, vtbl
    /// <c>012521A8</c>.
    /// </summary>
    public const uint FrontendUiGet = 0x00595582;
    public const uint FrontendUiCtor = 0x005953E2;
    public const uint FrontendUiVtbl = 0x012521A8;
    public const uint FrontendUiSingletonVa = 0x013B8B5C;
    public const int FrontendUiSize = 0xE0;
    public const uint FrontendUiBuildMenu = 0x00595B24;
    public const uint FrontendUiMessageFn = 0x0059A238;
    public const uint FrontendNewGameApply = 0x0059A2DA;
    public const uint FrontendNewGameThunk = 0x00594F28;
    public const uint FrontendMenuSearchFn = 0x005959AB;
    public const uint FrontendMenuMissFn = 0x00595A03;
    /// <summary>
    /// <c>0059A238</c> <c>msg-15</c> branch
    /// writes <c>[retail+41]=1</c>.
    /// </summary>
    public const int FrontendNewGameMessage = 15;
    public const int RetailNewGameFlagOffset = 41;
    public const int RetailLoadGameFlagOffset = 42;
    /// <summary>
    /// <c>0042EC7C</c> loop after Init frontend:
    /// <c>0042E3EE</c> input, <c>0042DC94</c>
    /// update, <c>0042FA30</c> zero record,
    /// <c>0042DBFA</c> fill, <c>0042DF9E</c>
    /// BeginScene / UI draw / EndScene /
    /// Present. Same device Present as PlayAVI.
    /// </summary>
    public const uint FrontendInputFn = 0x0042E3EE;
    public const uint FrontendUpdateFn = 0x0042DC94;
    public const uint FrontendRecordZeroFn = 0x0042FA30;
    public const uint FrontendRecordFillFn = 0x0042DBFA;
    public const uint FrontendDrawFn = 0x0042DF9E;
    public const uint FrontendUiDrawFn = 0x00595222;
    /// <summary>
    /// <c>00595222</c>: circular list at
    /// <c>[ui+84]</c>. Each <c>[node+20]</c>
    /// <c>vtbl+8</c>. Next is <c>004292C0</c>.
    /// </summary>
    public const uint FrontendWidgetNextFn = 0x004292C0;
    public const int FrontendWidgetListOffset = 84;
    public const int FrontendWidgetSlotOffset = 20;
    public const int FrontendWidgetDrawVtbl = 8;
    /// <summary>
    /// <c>0059899A</c> → <c>00595A06</c> stores
    /// one root at <c>[node+20]</c> via
    /// <c>0041DB1D</c> / <c>009AD410</c> /
    /// <c>0041D21B</c>. Type 0 ctor
    /// <c>0041B800</c> writes vtbl
    /// <c>0122F5D4</c>; slot +8 is
    /// <c>0041AFA0</c> (ret 20). Not
    /// UI singleton <c>012521A8+8</c>
    /// <c>0052D900</c>.
    /// </summary>
    public const uint FrontendMainMenuFn = 0x0059899A;
    public const uint FrontendMenuAttachFn = 0x00595A06;
    public const uint FrontendWidgetFactoryFn = 0x0041DB1D;
    public const uint FrontendWidgetConstructFn = 0x0041D21B;
    public const uint FrontendWidgetType0Ctor = 0x0041B800;
    public const uint FrontendWidgetVtbl = 0x0122F5D4;
    public const uint FrontendWidgetDrawFn = 0x0041AFA0;
    /// <summary>
    /// <c>0041B800</c> writes <c>[+376]=0</c>
    /// <c>[+380]=0</c> so first-seen
    /// <c>0041AFA0</c> takes
    /// <c>0041BEB0</c> at <c>0041B47C</c>,
    /// not sibling <c>0041BF60</c>.
    /// </summary>
    public const uint FrontendWidgetQueueFn = 0x0041BEB0;
    public const uint FrontendWidgetQueueSiblingFn = 0x0041BF60;
    public const uint FrontendWidgetPostCtorFn = 0x0041AC20;
    /// <summary>
    /// <c>0122F5D4+432</c> <c>00530EC0</c>.
    /// Empty def vector → <c>[+376]=0</c>
    /// and <c>0041AC20</c> skips
    /// <c>[+204]/[+208]</c>.
    /// </summary>
    public const uint FrontendWidgetFontListFn = 0x00530EC0;
    public const int FrontendWidgetFontListVtbl = 432;
    public const int FrontendWidgetDestWOffset = 204;
    public const int FrontendWidgetDestHOffset = 208;
    public const int FrontendWidgetOriginXOffset = 248;
    public const int FrontendWidgetOriginYOffset = 252;
    public const int FrontendWidgetScaleXOffset = 264;
    public const int FrontendWidgetScaleYOffset = 268;
    public const int FrontendWidgetSizeWOffset = 360;
    public const int FrontendWidgetSizeHOffset = 364;
    /// <summary>
    /// <c>0122F5D4+424</c> <c>0052F1E0</c>.
    /// First-seen dest is still 0 after
    /// scale <c>+264/+268</c> ctor 0.
    /// </summary>
    public const uint FrontendWidgetCenterFn = 0x0052F1E0;
    public const int FrontendWidgetCenterVtbl = 424;
    public const uint Frontend2dRecordType = RegionTravel.FadeOverlayRecordType;
    public const int Frontend2dRecordBytes = unchecked((int)RegionTravel.FadeOverlaySubmit);
    /// <summary>
    /// Default enqueue is <c>[edx+92]</c>
    /// dest <c>this+0x15C</c>. Alt is
    /// <c>[esi+112]</c> when both args
    /// at <c>esp+144</c>/<c>+152</c> are
    /// set (00595222 first-seen passes
    /// those as 0).
    /// </summary>
    public const int Frontend2dSubmitVtbl = RegionTravel.FadeOverlaySubmitVtbl;
    public const int Frontend2dAltSubmitVtbl = 112;
    /// <summary>
    /// <c>0042E204</c> <c>00B26340</c>
    /// alloc <c>0x178</c> ctor
    /// <c>00B260B0</c> vtbl
    /// <c>012A0F3C</c> at retail+88.
    /// Slot +92 is <c>00B23BC0</c> →
    /// <c>00B324A0([0x1436E80])</c>.
    /// Type <c>[rec]=0x22</c> indexes
    /// <c>[0x1436E84]+16</c>. Dest
    /// <c>widget+0x15C</c> <c>[+4]=0</c>
    /// first-seen. Handler
    /// <c>vtbl+20</c> UNREAD — not a
    /// memcpy into display +16020.
    /// </summary>
    public const uint FrontendEngineInitFn = 0x0042E204;
    public const uint FrontendEngineEmbedFn = 0x0042FD04;
    public const uint FrontendEngineAllocFn = 0x00B26340;
    public const uint FrontendEngineCtorFn = 0x00B260B0;
    public const uint FrontendEngineVtbl = 0x012A0F3C;
    public const int FrontendEngineObjectSize = 0x178;
    public const int FrontendEngineRetailOffset = 88;
    public const uint FrontendSubmitFn = 0x00B23BC0;
    public const uint FrontendSubmitDispatchFn = 0x00B324A0;
    public const uint FrontendSubmitSingletonVa = 0x01436E80;
    public const uint FrontendSubmitTypeTableVa = 0x01436E84;
    public const int FrontendWidgetReadyOffset = 368;
    public const int FrontendWidgetBlendOffset = 372;
    public const int FrontendWidgetFontOffset = 376;
    public const int FrontendWidgetTextureOffset = 380;
    public const int FrontendWidgetSubmitDestOffset = 0x15C;
    public const int FrontendWidgetBlendDefault = 2;
    public const int FrontendWidgetDefTypeOffset = 60;
    public const string FrontendMainMenuNoContinue =
        "UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE";
    public const string FrontendMainMenuContinue =
        "UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE";
    /// <summary>
    /// <c>0042DF9E</c> after the widget walk:
    /// <c>009D9C80</c> / <c>009DA9F0(1)</c>
    /// twice, then EndScene / Present.
    /// </summary>
    public const uint FrontendDisplayHelperFn = 0x00404A80;
    /// <summary>
    /// <c>00404A80</c> is <c>mov eax, 0x13B7CD8; ret</c>.
    /// </summary>
    public const uint FrontendDisplaySingletonVa = 0x013B7CD8;
    public const uint FrontendDisplayHelper2Fn = 0x00404C00;
    /// <summary>
    /// <c>00404C00</c>: <c>[ecx+8]==0</c>
    /// returns. Else <c>[+76].vtbl+108</c>
    /// byte to <c>[0x13750B0]</c>,
    /// <c>00CB38E0(1,1)</c>,
    /// <c>0041E5F2</c>, optional
    /// <c>0041A980</c> if
    /// <c>[input+184]</c>. First-seen
    /// singleton <c>+8</c> is BSS 0.
    /// </summary>
    public const int FrontendDisplayFlagOffset = 8;
    public const int FrontendDisplayObjectOffset = 76;
    public const int FrontendDisplayVtbl = 108;
    public const uint FrontendDisplayByteVa = 0x013750B0;
    public const uint FrontendDisplayImeFn = 0x00CB38E0;
    public const uint FrontendDisplayCursorFn = 0x0041A980;
    public const uint FrontendUiTickFn = 0x00599E3F;
    public const uint BeginSceneFn = RegionTravel.PlayAviBeginScene;
    public const uint EndSceneFn = RegionTravel.PlayAviEndScene;
    public const uint PresentFn = RegionTravel.PlayAviPresent;
    public const uint ClearColorFn = 0x009D8CF0;
    public const int FrontendRecordSize = 112;
    public const uint InitGameSite = 0x0042F491;
    public const string FinalAlbionWld = "FinalAlbion.wld";
    public const uint VideoPlayFlagVa = 0x01375448;
    public const uint VideoPlayFlag2Va = 0x0137544A;
    public const byte DefaultVideoPlayFlag = 1;
    public const byte DefaultVideoPlayFlag2 = 1;
    /// <summary>
    /// <c>0042ED85</c> writes the 32-byte
    /// slot RGBA to <c>[0x13961E0]</c>
    /// before <c>006286F0</c>. After the
    /// last video it is restored to
    /// <c>0xFF000000</c>.
    /// </summary>
    public const uint PlayAviClearColorVa = 0x013961E0;
    public const uint PlayAviClearRestoreArgb = 0xFF000000;
    /// <summary>
    /// <c>0042EE3D</c> <c>[0x13B8616]==0</c>
    /// skips the three <c>009A8840</c>
    /// bank swaps. BSS first-seen is 0.
    /// </summary>
    public const uint RetailBankSwapFlagVa = 0x013B8616;
    public const byte RetailBankSwapFlagFirstSeen = 0;
    /// <summary>
    /// After the video table: <c>[esi+9]=1</c>
    /// then <c>0042E98F</c> binds UI
    /// <c>00595582</c> at retail+180 and
    /// <c>009BFF40</c> 1024×768.
    /// </summary>
    public const uint RetailAfterAviFn = 0x0042E98F;
    public const uint DisplayModeFn = 0x009BFF40;
    public const int DisplayModeWidth = 0x400;
    public const int DisplayModeHeight = 0x300;
    /// <summary>
    /// <c>0042EF8E</c> alloc 16,
    /// <c>0042DB40</c> vtbl
    /// <c>01230C34</c>.
    /// </summary>
    public const uint FrontendHelperCtor = 0x0042DB40;
    public const uint FrontendHelperVtbl = 0x01230C34;
    public const int FrontendHelperSize = 16;
    /// <summary>
    /// <c>0042DED5(0)</c> after the
    /// post-AVI <c>009D8CF0</c> /
    /// <c>009BEEB0</c> Present.
    /// </summary>
    public const uint RetailAudioFadeFn = 0x0042DED5;
    public const uint FrontendUiShowFn = 0x005952C3;
    public const uint RetailFadeClockStartFn = 0x0062F800;
    public const uint RetailFadeClockResetFn = 0x0062F8B0;
    public const uint FrontendPostInitFn = 0x0040F0E0;

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
        ("Init Mesh Bank", InitMeshBankFn),
        ("Init UI Manager", 0x0041D198),
    ];

    public const uint InitWorldInitFn = 0x004A6E30;
    public const uint InitMeshBankFn = MeshBank.OpenFn;
    public const uint MeshBankLookupFn = MeshBank.LookupFn;
    public const uint MeshBankObjectCtor = MeshBank.ObjectCtor;
    public const uint MeshBankSetGlobalFn = MeshBank.SetGlobalFn;
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
    /// <summary>
    /// <c>0099A350</c>: <c>al=1</c>,
    /// <c>[ecx+4]=1</c>. <c>004AE940</c>
    /// therefore always writes
    /// <c>[player+9826]=1</c> /
    /// <c>[player+9824]=1</c>.
    /// </summary>
    public const uint PlayerObjectInitPredicate = 0x0099A350;
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
    public const uint GamePumpInnerStartFn = 0x0098E1B0;
    public const int GamePumpQuitLeave = 2;
    public const int GamePumpQuitUpdate = 1;
    public const int GamePumpQuitFirstSeen = 1;
    /// <summary>
    /// First <c>004189C2</c> after dummy
    /// record: <c>0040D2A0</c> singleton
    /// <c>[0x13B7D4C]</c> alloc <c>0x140</c>
    /// ctor <c>0040CEC0</c> sets
    /// <c>+51=1</c>, so <c>0040BC80</c>
    /// runs <c>00407370</c> then
    /// <c>0040A7F0</c>. Then
    /// <c>[game+40]+44</c> vtbl+220
    /// <c>00B239A0(12, 20.0f)</c> from
    /// <c>0x122F160</c>. <c>009F2660</c> /
    /// <c>009F26B0</c> lock
    /// <c>[0x13CAA90]</c>. Not a region.
    /// <c>0040A7F0</c> body PARTIAL.
    /// </summary>
    public const uint PlayAviSingletonFn = RegionTravel.PlayAviSingleton;
    public const uint PlayAviSingletonVa = RegionTravel.PlayAviSingletonVa;
    public const uint PlayAviSingletonCtor = 0x0040CEC0;
    public const int PlayAviSingletonSize = 0x140;
    public const uint PlayAviApplyFn = 0x0040BC80;
    public const uint PlayAviPrepareFn = 0x00407370;
    public const uint PlayAviApplyBodyFn = 0x0040A7F0;
    public const byte PlayAviPlus51FirstSeen = 1;
    public const int DisplayEngineFadeVtbl = 220;
    public const uint DisplayEngineFadeFn = 0x00B239A0;
    public const int DisplayEngineFadeType = 12;
    public const uint DisplayEngineFadeSecondsVa = 0x0122F160;
    public const float DisplayEngineFadeSeconds = 20f;
    public const uint InputLockObjectVa = 0x013CAA90;
    public const uint InputLockEnterFn = 0x009F2660;
    public const uint InputLockLeaveFn = 0x009F26B0;
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
    /// <summary>
    /// Game vtbl+32 <c>00416953</c>.
    /// First insn is <c>[world].vtbl+28([game+40])</c>.
    /// <c>[game+90588]</c> nonempty is
    /// "Loading save" <c>004A3200</c>.
    /// No-save is empty → "Loading world".
    /// Path is <c>+90576</c> (Leave frontend
    /// <c>0042F44D</c> / ctor <c>00415E17</c>
    /// writes <c>FinalAlbion.wld</c>), else
    /// <c>[0x13B8668]</c>, else UTF-16
    /// <c>0x122EE14</c> <c>updatedscenic.wld</c>.
    /// Then <c>004A1840(world, path)</c>.
    /// <c>[0x13B8648]==0</c> → <c>0049F180(0)</c>
    /// / Activate Initial Quests /
    /// <c>004B4A10</c>. Always <c>004BBC00</c>
    /// (<c>ret 4</c>) with <c>[0x13B8674]</c>.
    /// </summary>
    public const int GameLoadWorldVtbl = 32;
    public const uint GameLoadWorldFn = 0x00416953;
    public const int WorldPrepareVtbl = 28;
    public const uint WorldPrepareSite = 0x00416968;
    public const uint LoadSaveFn = 0x004A3200;
    public const int GameWorldPathOffset = 90576;
    public const int GameWorldPathAltOffset = 90580;
    public const int GameQuestOverrideOffset = 90584;
    public const int GameSaveNameOffset = 90588;
    public const uint GameWorldPathCopyFn = 0x00415E17;
    public const uint WorldPathGlobalVa = 0x013B8668;
    public const uint WorldPathAltGlobalVa = 0x013B866C;
    public const uint WorldPathDefaultVa = 0x0122EE14;
    public const string WorldPathDefault = "updatedscenic.wld";
    public const uint EmptyQuestNameVa = 0x0122D70E;
    public const uint AfterLoadWorldFn = 0x004BBC00;
    public const uint AfterLoadWorldArgVa = 0x013B8674;
    /// <summary>
    /// After vtbl+32, no-save
    /// <c>[0x13B8648]==0</c>:
    /// <c>0049BA70(game+90488, 60, 0)</c>
    /// (<c>0099A350</c> always 1,
    /// <c>+20=60</c>, <c>+40</c> from
    /// <c>0x1238B48</c> = 0.1),
    /// <c>00416392</c> (<c>+90394==0</c>
    /// → <c>0049E200</c> /
    /// <c>0051E530</c>+WorldFrame),
    /// <c>004AE9D0</c> writes
    /// <c>+9836/+9840/+9844</c> when
    /// <c>+9826</c>, then
    /// <c>default_user.ini</c>
    /// <c>00999230</c> and <c>user.ini</c>
    /// <c>009EC890</c>. Then QPC seed.
    /// </summary>
    public const uint PostLoadWorldReserveFn = 0x0049BA70;
    public const uint PostLoadWorldReserveSite = 0x00418901;
    public const int GamePlus90488Offset = 90488;
    public const int PostLoadWorldReserveCount = 60;
    public const uint PostLoadWorldReserveRateVa = 0x01238B48;
    public const double PostLoadWorldReserveRate = 0.1;
    public const uint WorldThingCountFn = 0x00416392;
    public const int GamePlus90394Offset = 90394;
    public const uint WorldThingCountApply = 0x0049E200;
    public const uint WorldThingCountWalk = 0x0051E530;
    public const uint PlayerBindAfterWorldFn = 0x004AE9D0;
    public const uint PlayerBindAfterWorldSite = 0x0041891D;
    public const int PlayerBindSlot0Offset = 9836;
    public const int PlayerBindSlot1Offset = 9840;
    public const int PlayerBindSlot2Offset = 9844;
    public const uint DefaultUserIniVa = 0x0122F030;
    public const uint UserIniVa = 0x0122F01C;
    public const string DefaultUserIniName = "default_user.ini";
    public const string UserIniName = "user.ini";
    public const uint FileExistsFn = 0x00999230;
    public const uint IniApplyFn = 0x009EC890;
    public const uint IniTokenizeFn = 0x009EC710;
    public const uint IniDispatchFn = 0x009EB430;
    public const uint EngineReadyCallback = 0x004167DA;
    public const int EngineReadyCallbackOffset = 240;
    public const int EngineGamePtrOffset = 244;
    public const uint StartupWadSite = 0x004A19EB;
    public const uint SetStaticMapForEngineSite = 0x004A1B7D;
    public const uint AttachPatchFn = 0x00BDF010;
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
    /// <summary>
    /// <c>004A5A40</c> at <c>004A5D88</c>
    /// when <c>[world+260]==0</c>:
    /// <c>004B4490</c> → <c>00CB8220</c>
    /// → <c>00CB7C40</c>+<c>00CB8170</c>
    /// → <c>00CB7950</c>. First-seen
    /// fiber <c>+41==0</c> takes
    /// <c>vtbl+4</c>, not
    /// <c>00A44880</c>.
    /// </summary>
    public const uint QuestManagerPumpFn = 0x004B4490;
    public const uint QuestManagerVa = 0x013B89FC;
    public const uint QuestListPumpFn = 0x00CB8220;
    public const uint QuestListWalkAFn = 0x00CB7C40;
    public const uint QuestListWalkBFn = 0x00CB8170;
    public const uint QuestFiberAttachFn = 0x00CB7950;
    public const uint QuestFiberUpdateVtbl = 24;
    public const int QuestFiberUpdateFlagOffset = 41;
    /// <summary>
    /// <c>00CB78D0</c>:
    /// <c>mov al,[esp+4]; mov [ecx+41],al; ret 4</c>.
    /// Zero <c>E8</c> callers. First-seen
    /// stays 0 so <c>00CB7950</c> does
    /// not take <c>00A44880</c>.
    /// </summary>
    public const uint FiberUpdateFlagSetter = 0x00CB78D0;
    public const uint QuestSubjectFillFn = 0x008884D0;
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
    public const uint WorldCameraCtor = WorldCamera.Ctor;
    public const uint WorldCameraVtbl = WorldCamera.Vtbl;
    public const uint WorldCameraSeedFn = WorldCamera.SeedFn;
    public const uint WorldCameraPoseFn = WorldCamera.PoseFn;
    public const int WorldCameraObjectSize = WorldCamera.ObjectSize;
    public const int WorldCameraOffset = WorldCamera.WorldOffset;
    public const uint GameCameraCtor = GameCamera.Ctor;
    public const uint GameCameraVtbl = GameCamera.Vtbl;
    public const int GameCameraObjectSize = GameCamera.ObjectSize;
    public const int GameCameraOffset = GameCamera.WorldOffset;
    public const uint GameCameraManagerCtor = GameCameraManager.Ctor;
    public const uint GameCameraManagerVtbl = GameCameraManager.Vtbl;
    public const int GameCameraManagerObjectSize = GameCameraManager.ObjectSize;
    public const int GameCameraManagerOffset = GameCameraManager.WorldOffset;
    public const uint ThingWalkApplyFn = 0x0051EBD0;
    public const uint DisplayApplyThunk = 0x00435F70;
    public const uint DisplayApplyBodyFn = 0x00435530;
    /// <summary>
    /// Tail of <c>00435530</c>:
    /// <c>009BEF20</c> BeginScene,
    /// <c>009D8CF0</c> clear,
    /// <c>009BEF50</c> EndScene,
    /// <c>009BEEB0</c> Present.
    /// Same device Present as frontend
    /// and PlayAVI. Vulkan <c>Draw</c>
    /// is that Present.
    /// </summary>
    public const uint GamePresentSite = 0x00435F50;
    /// <summary>
    /// <c>009BEF80</c> after
    /// <c>009BF7E0</c>:
    /// <c>SetViewport</c> vtbl+188,
    /// full backbuffer, MinZ 0 MaxZ 1.
    /// </summary>
    public const uint SetViewportFn = 0x009BEF80;
    public const int SetViewportVtbl = 188;
    public const float ViewportMinZ = 0f;
    public const float ViewportMaxZ = 1f;
    /// <summary>
    /// Player-manager poll
    /// <c>00446A30</c> → <c>00446330</c>
    /// / <c>009F4ED0</c>. Zero <c>E8</c>
    /// callers; game vtbl+24
    /// <c>00416E78</c> calls
    /// <c>[game+32].vtbl+4</c>.
    /// Not the retail
    /// <c>0042E3EE</c> walk.
    /// </summary>
    public const uint PlayerInputPumpFn = PlayerInterface.PumpFn;
    public const uint PlayerInputPollFn = PlayerInterface.PollFn;
    public const uint PlayerInputFallbackFn = PlayerInterface.FallbackFn;
    public const uint PlayerInterfaceCtor = PlayerInterface.Ctor;
    public const uint PlayerInterfaceVtbl = PlayerInterface.Vtbl;
    public const uint PlayerInterfacePreprocess = PlayerInterface.PreprocessFn;
    public const uint PlayerListenerVtbl = ActionInputListener.VtblVa;
    public const uint PlayerListenerAcceptFn = ActionInputListener.AcceptFn;
    public const uint PlayerListenerApplyFn = ActionInputListener.ApplyFn;
    public const uint PlayerListenerRegisterFn = PlayerInterface.RegisterThunk;
    public const uint PlayerListenerFactoryFn = ActionInputListener.FactoryFn;
    public const uint PlayerEventLookupFn = PlayerInterface.LookupFn;
    public const uint PlayerApplyFn = PlayerInterface.ApplyFn;
    public const uint PlayerApplyQueueFn = PlayerInterface.ApplyQueueFn;
    public const uint PlayerApplyPlayerFn = PlayerInterface.ApplyPlayerFn;
    public const uint PlayerOwnerCtor = PlayerInterface.OwnerCtor;
    /// <summary>
    /// <c>00435530</c> between BeginScene
    /// and EndScene: player overlay
    /// <c>00435000</c> / <c>00639E40</c>,
    /// player interface <c>00435070</c>,
    /// 2D flush <c>009D9C80</c>, layer
    /// flush <c>009DA9F0(1)</c>
    /// DrawIndexedPrimitive vtbl+332.
    /// </summary>
    public const uint DisplayPlayerOverlayFn = 0x00435000;
    public const uint DisplayPlayerOverlayApply = 0x00639E40;
    public const uint DisplayPlayerInterfaceFn = 0x00435070;
    public const uint DisplayFlush2dFn = RegionTravel.PlayAviFlush;
    public const uint DisplayFlushLayersFn = 0x009DA9F0;
    public const int DisplayFlushLayersArg = 1;
    public const int DrawIndexedPrimitiveVtbl = 332;
    /// <summary>
    /// <c>009DA9F0</c> walks
    /// <c>[this+16020, +16024)</c>.
    /// Empty jumps to <c>009DB6E6</c>.
    /// Nonempty: <c>00A058C0</c> then
    /// <c>[device+88].vtbl+332</c>
    /// (push 32, VB <c>+16008</c>, count,
    /// prim 2 or 4). No
    /// <c>cmp …,0x22</c>.
    /// </summary>
    public const int DisplayQueueBeginOffset = 16020;
    public const int DisplayQueueEndOffset = 16024;
    public const int DisplayQueueRecordSize = 60;
    public const uint DisplayQueueCountMagic = 0x88888889;
    public const int DisplayVertexBufferOffset = 16008;
    public const uint DisplayQueueEnqueueFn = 0x009DB700;
    public const uint DisplayPrimitiveFn = 0x00A058C0;
    public const int DisplayDipStride = 32;
    public const int DisplayDipPrimLines = 2;
    public const int DisplayDipPrimTris = 4;
    /// <summary>
    /// <c>009DA9F0</c> count is
    /// <c>(end-begin)*0x88888889</c>
    /// (60-byte records). Zero jumps
    /// <c>009DB6E6</c>. Enqueue is
    /// <c>009DB700</c>, not
    /// <c>0041BEB0</c> type 0x22.
    /// First-seen frontend is empty.
    /// </summary>
    public static int DisplayQueueCount(int begin, int end)
    {
        var bytes = end - begin;
        if (bytes <= 0)
            return 0;
        return bytes / DisplayQueueRecordSize;
    }

    public static bool DisplayFlushShouldDip(int begin, int end) =>
        DisplayQueueCount(begin, end) != 0;

    public static int DisplayFlushPrimitive(bool triangleStrip) =>
        triangleStrip ? DisplayDipPrimTris : DisplayDipPrimLines;
    public const uint RenderFrameFn = RegionTravel.RenderFrame;

    /// <summary>
    /// Recovered <c>00435530</c> order.
    /// Layer bits come from
    /// <see cref="ScenePasses.Registration"/>.
    /// </summary>
    public static readonly (string Name, uint Va)[] DisplaySubmitStages =
    [
        ("BeginScene", 0x009BEF20),
        ("Clear", 0x009D8CF0),
        ("PlayerOverlay", 0x00435000),
        ("PlayerInterface", 0x00435070),
        ("Flush2D", 0x009D9C80),
        ("FlushLayers", 0x009DA9F0),
        ("EndScene", 0x009BEF50),
        ("Present", 0x009BEEB0),
    ];
    public const uint CameraTimeFn = 0x00416231;
    public const uint CameraInterpolationFn = 0x0041707E;
    public const uint CameraInterpTimeFn = 0x004166E2;
    public const uint CameraClampFn = 0x0041919C;
    public const uint PlayerReadyQueryFn = 0x004AEA70;
    public const int WorldPlus164Offset = 164;
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
    /// <summary>
    /// <c>006C2170</c> "Loading objects":
    /// <c>00522720</c> then
    /// <c>00521AE0</c> Thing Manager Load
    /// From File (map <c>.tng</c>).
    /// </summary>
    public const uint LoadThingsForMapFn = 0x00522720;
    public const uint ThingManagerLoadFileFn = 0x00521AE0;
    /// <summary>
    /// <c>0051FD80</c> Load Single Thing.
    /// <c>PlayerCreature</c> binds
    /// <c>00449970</c> / <c>00487DC0</c>
    /// then <c>006AC910</c> create and
    /// <c>0051E5A0</c> Activate After Loading.
    /// </summary>
    public const uint LoadSingleThingFn = 0x0051FD80;
    public const uint NewThingParseFn = 0x00520D00;
    public const uint ActivateAfterLoadingFn = 0x0051E5A0;
    public const uint AllocateClassFn = 0x00A371C0;
    public const uint PlayerCreatureCreateFn = 0x006AC910;
    public const uint PlayerCreatureBindFn = 0x00449970;
    public const uint PlayerCreatureThingFn = 0x00487DC0;
    public const uint PlayerSlotWalkFn = 0x004498C0;
    public const uint PlayerCreatureFactoryFn = 0x0052B880;
    public const uint HolySiteFactoryFn = 0x0052AC90;
    public const uint CreateCharacterFn = 0x00489D40;
    public const uint InitCharactersFn = 0x0049F180;
    public const uint InitGuiFn = 0x0043A380;
    public const uint InitQuestsFn = 0x004B4260;
    public const uint ActivateQuestFn = 0x00CB5AD0;
    public const uint QuestRegisterFn = QuestFactoryTable.Register;
    public const uint QuestFactoryBindFn = QuestFactoryTable.Bind;
    public const uint QuestFactoryCollectFn = QuestFactoryTable.Collect;
    public const uint QuestFactoryStartFn = QuestFactoryTable.StartWalk;
    public const uint SunnyvalePersistFn = PersistTable.SunnyvaleBind;
    public const uint QuestManagerActivate = 0x004B2890;
    /// <summary>
    /// <c>004A1840</c> "Load Quests" during
    /// Loading world <c>00416ABA</c>. Parses
    /// <c>AddQuest</c> / <c>AddTestQuest</c>
    /// into world+184. <c>0049F180</c> is
    /// the next sibling, not a child.
    /// </summary>
    public const uint LoadQuestsFn = 0x004A1840;
    public const uint LoadQuestsSite = 0x00416ABA;
    public const uint QstParseFn = 0x004A0D90;
    public const uint ActivateInitialQuestsFn = 0x004B4A10;
    public const uint ActivateInitialQuestsSite = 0x00416BCF;
    public const int WorldQuestListOffset = 172;
    public const int WorldQuestDefListOffset = 184;
    public const uint InitHeroDefFn = 0x00449D90;
    public const uint InitCharacterAsFn = 0x0048A070;
    public const uint ConstructFromParamsFn = 0x006A9DD0;
    public const uint ParentConstructFn = 0x00662880;
    public const uint CreatureConstructThunk = 0x008388D0;
    public const uint CreatureConstructFn = 0x006A5950;
    public const uint ThingConstructFromDefFn = 0x004CA010;
    public const uint DefAttachFn = 0x0042AF3C;
    public const uint DefLookupFn = 0x009AD410;
    public const uint ThingTypeRegistrarFn = 0x00522A20;
    public const string PlayerCreatureName = "PlayerCreature";
    public const string PlayerHeroDefName = "PLAYER_HERO";
    public const string CreatureHeroDefName = RegionTravel.AdultCreature;
    public const string HeroScriptName = "Hero";
    public const string GuildArrivalHsp = "GuildArrivalHSP";
    public const uint BuildLoadJobFn = 0x006C27A0;
    public const uint BuildLoadJobCopyMapsFn = 0x006C2D40;
    public const uint BuildLoadJobCopyTreeFn = 0x006B9E00;
    public const int LoadJobIndexOffset = 28;
    public const int LoadJobRecordSize = 28;
    public const uint EnqueueLoadJobFn = 0x006C2120;
    public const uint LevelLoaderUpdate = 0x006C2710;
    public const uint LevelLoaderPopFn = 0x006C2BA0;
    public const uint LevelLoaderApply = 0x006C2170;
    public const uint LevelLoaderHasWork = 0x006C20A0;
    public const uint LoadTopologyFn = 0x004FF080;
    public const uint LoadTopologyHelperFn = 0x00638310;
    public const uint PostLoadTopologyFn = 0x004FF440;
    public const uint PostLoadInitialiseFn = 0x004FD020;
    public const uint PostLoadInitialiseApply = 0x00821850;
    public const uint ThingManagerActivateAfterFn = 0x0051E2F0;
    /// <summary>
    /// <c>006C2170</c> passes 3–4. Gated on
    /// job record <c>+12</c>. First-seen
    /// <c>00500540(1,0,0)</c> zeros
    /// <c>+12</c> (third arg 0 skips the
    /// fill) so both are skipped.
    /// </summary>
    public const uint JobNavPassFn = 0x00500230;
    public const uint JobNavCommitFn = 0x0050AF10;
    public const int LoadJobNavOffset = 12;
    public const uint SetRegionAsLoadedFn = 0x004FC8A0;
    public const uint MiniMapFromUiFn = 0x00437CE0;
    public const int MiniMapUiOffset = 352;
    public const uint QuestRegionNotifyFn = 0x004AFC00;
    public const uint ActivateTopologyFn = 0x004FCBB0;
    public const uint SetMapLoadingFlagFn = 0x004FCFE0;
    public const int WorldMapSetLoadedVtbl = 88;
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
    /// <c>00501450</c>: player
    /// <c>00449970</c>/<c>00487DC0</c>,
    /// <c>004FEEC0(current,0)</c> writes
    /// <c>+156=0</c>, table count
    /// <c>(+48-+44)/88</c>. Count &gt; 1
    /// loops <c>00500540(i,0,0)</c> from 1
    /// (+36 null still
    /// <c>006C27A0</c>). Then
    /// <c>00500540(saved,0,1)</c>. First
    /// seen saved is dummy 0. Later
    /// indices after 1 stay PARTIAL.
    /// E8 caller UNREAD (not in
    /// <c>004162B5</c> / <c>00418289</c>).
    /// </summary>
    public const uint LoadFromFirstRealRegionFn = 0x00501450;
    public const uint UnloadCurrentRegionFn = 0x004FEEC0;
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
    /// <summary>
    /// <c>00B428E0</c> first call.
    /// Walks the open-map list from index 1
    /// and <c>00B3EF40</c> each slot.
    /// </summary>
    public const uint CloseStaticMapFileFn = 0x00B40000;
    public const uint CloseStaticMapFilePrelude = 0x00B40070;
    public const uint OpenStaticMapsMode1Current = 0x00B3E820;
    public const uint OpenStaticMapsNameTable = 0x00B420F0;
    public const uint OpenStaticMapsAttach = 0x00B41E50;
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
        "START_INITIAL_QUESTS", "END_INITIAL_QUESTS",
    ];

    /// <summary>
    /// <c>00595B24</c> labels. Third arg is
    /// the menu id; New Game is 0.
    /// </summary>
    public static readonly (string Label, int Id)[] FrontendMenuItems =
    [
        ("UI_TEXT_NEW_GAME", 0),
        ("UI_TEXT_LOAD_GAME", 0),
        ("UI_TEXT_OPTIONS_MENU_TITLE", 24),
        ("UI_TEXT_OPTIONS_MENU_TITLE", 1),
        ("UI_TEXT_GAME_OPTIONS_MENU_TITLE", 1),
        ("UI_TEXT_VIDEO_MENU_TITLE", 5),
        ("UI_TEXT_SCOREBOARD_MENU_TITLE", 25),
        ("UI_TEXT_REDEFINE_KEYS_MENU_TITLE", 22),
        ("UI_TEXT_AUDIO_OPTIONS_MENU_TITLE", 4),
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
    public LoadTiming Timing { get; } = new();
    public EngineStage Stage { get; private set; } = EngineStage.ProcessEntry;
    public EngineMode Mode { get; private set; } = EngineMode.None;
    public int StartupVideoIndex { get; private set; }
    public bool PlayStartupVideos { get; private set; } = true;
    public bool GraphicsCreated { get; private set; }
    public int CreateDeviceFlags { get; private set; }
    /// <summary>
    /// <c>[0x137545C]</c> after
    /// <c>009C0E50</c> min-32 clamp.
    /// </summary>
    public int BackBufferWidth { get; private set; }
    public int BackBufferHeight { get; private set; }
    public int BackBufferBpp { get; private set; }
    public string WindowTitle { get; private set; } = WindowTitleDefault;
    /// <summary>
    /// <c>009BEF80</c> after CreateDevice.
    /// Full backbuffer, MinZ 0, MaxZ 1.
    /// </summary>
    public int ViewportX { get; private set; }
    public int ViewportY { get; private set; }
    public int ViewportWidth { get; private set; }
    public int ViewportHeight { get; private set; }
    public float ViewportZNear { get; private set; } = ViewportMinZ;
    public float ViewportZFar { get; private set; } = ViewportMaxZ;
    public int GamePresentCount { get; private set; }
    public int LayerFlushCount { get; private set; }
    public IReadOnlyList<uint> SubmittedLayerBits => _submittedLayers;
    /// <summary>
    /// <c>0041E5F2</c> singleton. Built on
    /// the first <c>0042E3EE</c> /
    /// <c>00418289</c> pump.
    /// </summary>
    public EngineInput Input { get; } = new();
    /// <summary>
    /// <c>004473A0</c> at <c>game+32</c>.
    /// </summary>
    public PlayerInterface Player { get; } = new();
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
    /// Authored <c>REGION.EnvironmentTheme</c>.
    /// Not applied to live lighting: first-seen
    /// is lighting-manager ctor record 0
    /// (<c>00B482A0</c> / <c>00B46C80</c>).
    /// </summary>
    public int AuthoredEnvironmentThemeId { get; private set; }
    public string? AuthoredEnvironmentTheme { get; private set; }
    /// <summary>
    /// <c>[record+36] != 0</c>. False after
    /// WLD parse; who writes the pointer is unread.
    /// </summary>
    public bool RegionObjectPresent { get; private set; }
    public bool GamePumpFirstDone { get; private set; }
    public bool PlayAviSingletonReady { get; private set; }
    public bool DisplayEngineFadeSet { get; private set; }
    public int DisplayEngineFadeKind { get; private set; }
    public float DisplayEngineFadeTime { get; private set; }
    public IReadOnlyList<string> UserIniCommands { get; private set; } = [];
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
    /// <c>[player+9826]</c>. <c>004AE940</c>
    /// sets 1 because <c>0099A350</c>
    /// always returns 1.
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
    public bool CameraInterpolationRan { get; private set; }
    public float CameraInterpolationT { get; private set; }
    /// <summary>
    /// <c>world+164</c>. Default 0 takes
    /// <c>00417097</c>. Nonzero is
    /// <c>0041714D</c> UNREAD.
    /// </summary>
    public int WorldPlus164 { get; set; }
    /// <summary>
    /// <c>004166E2</c> display time minus
    /// <c>[game+96]</c>. Startup 0.
    /// </summary>
    public double DisplayTime { get; set; }
    /// <summary>
    /// Same camera the renderer consumes.
    /// Filled by <c>006B42F0</c> from
    /// <see cref="WorldCamera"/>.
    /// </summary>
    public ScriptedCamera Camera { get; } = new();
    public WorldCamera WorldCamera { get; } = new();
    public GameCamera GameCamera { get; } = new();
    public GameCameraManager GameCameraManager { get; } = new();
    public bool WorldCameraPresent { get; private set; }
    public bool FrontendUiPresent { get; private set; }
    /// <summary>
    /// Retail mode <c>+41</c>. Nonzero takes
    /// <c>0042F297</c> Leave frontend.
    /// </summary>
    public bool RetailNewGameFlag { get; private set; }
    public int FrontendFrameCount { get; private set; }
    public int FrontendPresentCount { get; private set; }
    public int FrontendWidgetsDrawn { get; private set; }
    public int FrontendFlushCount { get; private set; }
    public int Frontend2dRecordsQueued { get; private set; }
    public uint Frontend2dLastType { get; private set; }
    public uint Frontend2dLastPacker { get; private set; }
    public int Frontend2dLastSubmitVtbl { get; private set; }
    /// <summary>
    /// First-seen <c>009DA9F0</c>
    /// <c>[+16020]==[+16024]</c>.
    /// <c>0041AFA0</c> packs to
    /// widget <c>+0x15C</c> via
    /// <c>vtbl+92</c>, not this list.
    /// </summary>
    public bool Frontend2dDipIssued { get; private set; }
    /// <summary>
    /// <c>[0x13B7CD8+8]</c>. BSS 0 skips
    /// the <c>00404C00</c> body.
    /// </summary>
    public bool FrontendDisplayFlag { get; private set; }
    public bool FrontendDisplayImeRan { get; private set; }
    public bool FrontendDisplayCursorRan { get; private set; }
    public int FrontendWidgetBlend { get; private set; }
    public int FrontendWidgetFont { get; private set; }
    /// <summary>
    /// First-seen <c>0041AFA0</c> dest after
    /// <c>+248/+264</c> ctor 0. Not PlayAVI
    /// <c>00628B79</c>.
    /// </summary>
    public float FrontendWidgetDestX0 { get; private set; }
    public float FrontendWidgetDestY0 { get; private set; }
    public float FrontendWidgetDestX1 { get; private set; }
    public float FrontendWidgetDestY1 { get; private set; }
    public int FrontendWidgetTexture { get; private set; }
    public string? FrontendMenuRoot { get; private set; }
    public bool FrontendMenuConstructed { get; private set; }
    public IReadOnlyList<string> FrontendMenuLabels =>
        FrontendMenuItems.Select(i => i.Label).ToList();
    public IReadOnlyList<int> GameTickTypes => _tickTypes;
    public bool LevelLoaderReady { get; private set; }
    public bool FirstRealRegionLoadDone { get; private set; }
    public int RegionThingMapsLoaded { get; private set; }
    public IReadOnlyList<ThingInstance> RegionThings => _regionThings;
    public ThingInstance? Hero { get; private set; }
    public bool HeroSpawned { get; private set; }
    /// <summary>
    /// Map that owns <see cref="Hero"/> —
    /// LookoutPoint <c>GuildArrivalHSP</c>
    /// on no-save, not StartOakVale.
    /// </summary>
    public string? FirstSceneMapName { get; private set; }
    public string? HeroDefinition { get; private set; }
    public int HeroMeshId { get; private set; }
    public IReadOnlyList<InsertedThing> InsertedThings => _inserted;
    public bool PlayerGuiReady { get; private set; }
    public bool QuestsInitDone { get; private set; }
    public bool QuestPumpRan { get; private set; }
    public int QuestPumpWalked { get; private set; }
    public int QuestVtbl24Calls { get; private set; }
    public bool FollowSpringRan { get; private set; }
    public bool SubjectFillNoted { get; private set; }
    public QuestFile? Quests { get; private set; }
    public ScriptRuntime? Runtime { get; private set; }
    public IReadOnlyList<string> ActivatedQuests => _activatedQuests;
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
    /// <summary>
    /// <c>00B3E820</c> current-map handle.
    /// Neighbours are <c>00B41E50</c>.
    /// </summary>
    public string? CurrentStaticMapName { get; private set; }
    public IReadOnlyList<string> NeighbourStaticMaps => _neighbourStaticMaps;
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
    public IEngineHost? Host { get; private set; }
    public WmvPlayer? StartupAvi { get; private set; }
    /// <summary>
    /// Live <c>[0x13961E0]</c> from the
    /// current 32-byte video slot.
    /// </summary>
    public uint PlayAviClearArgb { get; private set; } = PlayAviClearRestoreArgb;
    public WorldGeometry? SubmittedWorld { get; private set; }
    public Fable.Render.TexturedMesh? SubmittedMesh { get; private set; }
    public Fable.Render.TexturedMesh? SubmittedLandscape { get; private set; }
    public Fable.Render.TexturedMesh? SubmittedObjects { get; private set; }
    public int SubmittedLandscapeCells { get; private set; }
    public LoadTiming? LastLoadTiming { get; private set; }
    public bool WorldSubmitted { get; private set; }
    /// <summary>
    /// Primary-map C3Ds with a bone
    /// stream. Hero 4299 is PALSKIN,
    /// not a static flatten.
    /// </summary>
    public IReadOnlyList<uint> SubmittedPalskinMeshIds => _submittedPalskin;
    public IReadOnlyList<string> SubmittedTerrainMaps => _submittedTerrain;
    public IReadOnlyList<Fable.Render.GpuTexture> SubmittedTextures =>
        _submittedTextures;
    public TextureLibrary? Textures { get; private set; }
    public bool SubmittedHeroPalskin { get; private set; }
    /// <summary>
    /// <c>00B314E0</c> consumed the hero
    /// helper, not ctor-axis
    /// <c>+6296</c>.
    /// </summary>
    public bool RendererHelperBound { get; private set; }
    /// <summary>
    /// Wall time of the last
    /// <see cref="SubmitCurrentWorld"/>.
    /// </summary>
    public double SubmitElapsedMs { get; private set; }
    public int SubmitC3dParsed { get; private set; }
    public int PresentMeshUploads { get; private set; }

    public void AttachHost(IEngineHost host) => Host = host;

    private readonly List<string> _completed = [];
    private readonly List<string> _banks = [];
    private readonly List<int> _loadQueue = [];
    private readonly List<string> _activatedMaps = [];
    private readonly List<string> _openedStaticMaps = [];
    private readonly List<string> _neighbourStaticMaps = [];
    private readonly List<OpenedStaticMapBody> _openedBodies = [];
    private readonly List<int> _tickTypes = [];
    private readonly List<uint> _submittedPalskin = [];
    private readonly List<string> _submittedTerrain = [];
    private readonly List<Fable.Render.GpuTexture> _submittedTextures = [];
    private Fable.Render.GpuTexture[]? _submittedTextureArray;
    private readonly List<ThingInstance> _regionThings = [];
    private readonly Dictionary<string, List<ThingInstance>> _thingsByMap =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly List<InsertedThing> _inserted = [];
    private readonly List<string> _activatedQuests = [];
    private readonly List<uint> _submittedLayers = [];
    private GameBin? _defs;
    private LevelLibrary? _levels;
    public MeshBank Meshes { get; } = new();

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
        var boot = System.Diagnostics.Stopwatch.StartNew();
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
            ApplyPlayAviSlot(StartupVideos[0]);
        else
            Note(FrontendIntern, "Frontend", "FRONT_END", "skip videos");
        Timing.Add("bootstrap", boot.Elapsed.TotalMilliseconds, Stage.ToString());
    }

    /// <summary>
    /// One <c>00412F90</c> / <c>0042EC7C</c> step.
    /// Returns false when the mode loop exits.
    /// </summary>
    public bool Pump() => Pump(0f);

    /// <summary>
    /// One <c>00412F90</c> / <c>0042EC7C</c>
    /// step. The host only queues input
    /// and Presents what this returns.
    /// </summary>
    public bool Pump(float dt)
    {
        if (Stage == EngineStage.StartupVideos)
        {
            PumpInput();
            if (QueuedPlayAviSkip())
                SkipStartupVideo();
            else
                PumpStartupAvi(dt);
            PresentToHost();
            return true;
        }

        if (Stage == EngineStage.Frontend)
        {
            UnloadStartupAvi();
            if (!FrontendUiPresent)
                InitFrontendUi();
            PumpFrontendFrame();
            MaybeActivateNewGameFromInput();
            if (RetailNewGameFlag)
            {
                RequestNewGame();
                EnterGame();
            }

            PresentToHost();
            return true;
        }

        if (Stage == EngineStage.LeaveFrontend)
        {
            EnterGame();
            // 00435530 only after WorldFrame>1 and
            // maps are open. Do not Present origin
            // camera / empty mesh.
            return true;
        }

        if (Stage == EngineStage.Game)
        {
            PumpGame();
            if (WorldSubmitted && WorldCamera.Seeded)
                PresentToHost();
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

    public void UnloadStartupAvi()
    {
        if (StartupAvi is null)
            return;
        StartupAvi.Dispose();
        if (!StartupAvi.GraphReleased)
        {
            while (Stage == EngineStage.StartupVideos)
                FinishStartupVideo();
            StartupAvi = null;
            return;
        }

        StartupAvi = null;
    }

    public void SkipStartupVideo()
    {
        UnloadStartupAvi();
        if (Stage != EngineStage.StartupVideos)
            return;
        FinishStartupVideo();
        EnsureStartupAvi();
    }

    public void SubmitCurrentWorld()
    {
        if (Install is null || CurrentRegion is null || !HeroSpawned)
            return;
        if (WorldSubmitted && SubmittedMesh is not null)
            return;
        var parsedBefore = Meshes.ParsedCount;
        var clock = System.Diagnostics.Stopwatch.StartNew();
        var timing = new LoadTiming();
        UnloadStartupAvi();
        EnsureLevels();
        OpenMeshBank();
        var opened = timing.Measure("PresentWorld", PresentWorld);
        if (opened is null || _levels is null)
            return;
        SubmittedWorld = opened;
        var planes = SubmitSidePlanes();
        _submittedTerrain.Clear();
        var cells = timing.Measure("TerrainCells",
            () => opened.CollectVisibleCells(_levels, planes, _submittedTerrain),
            c => $"n={c.Count} maps={_submittedTerrain.Count}");
        var land = timing.Measure("LandDraws",
            () => MeshBatches.BuildCells(cells),
            m => $"verts={m.Vertices.Length} draws={m.Draws.Length}");
        var props = new List<(MeshFile Mesh, Matrix4x4 Transform)>();
        var seen = new HashSet<uint>();
        _submittedPalskin.Clear();
        SubmittedHeroPalskin = false;
        timing.Measure("C3D", () =>
        {
            foreach (var inst in opened.Instances)
            {
                if (!inst.Map.Equals(opened.Region, StringComparison.OrdinalIgnoreCase))
                    continue;
                var mesh = Meshes.Get(inst.MeshId);
                if (mesh is null)
                    continue;
                seen.Add(inst.MeshId);
                props.Add((mesh, inst.Transform));
                if (mesh.BoneCount > 0)
                    _submittedPalskin.Add(inst.MeshId);
            }

            return seen.Count;
        }, n => $"ids={n} inst={props.Count}");

        // 006AC910 spawn is a Thing, not a TNG
        // Graphic. Submit it as PALSKIN even if
        // PresentWorld missed the instance.
        if (HeroMeshId != 0 &&
            Hero is { PositionX: not null, PositionY: not null, PositionZ: not null } &&
            seen.Add((uint)HeroMeshId))
        {
            var heroMesh = Meshes.Get((uint)HeroMeshId);
            if (heroMesh is not null)
            {
                props.Add((heroMesh, WorldGeometry.ObjectTransform(Hero)));
                if (heroMesh.BoneCount > 0)
                    _submittedPalskin.Add((uint)HeroMeshId);
            }
        }

        SubmittedHeroPalskin = HeroMeshId != 0 &&
            _submittedPalskin.Contains((uint)HeroMeshId);
        var objects = timing.Measure("BuildMeshes",
            () => MeshBatches.BuildMeshes(props),
            m => $"verts={m.Vertices.Length}");
        var sky = timing.Measure("Sky",
            () => MeshBatches.Build(SkyGeometry.Build(Install)),
            m => $"verts={m.Vertices.Length}");
        SubmittedLandscape = land;
        var combined = MeshBatches.Concat(objects, sky);
        SubmittedObjects = new TexturedMesh
        {
            Vertices = combined.Vertices,
            Draws = MeshBatches.SortByPass(combined.Draws),
        };
        SubmittedLandscapeCells = cells.Count;
        // Sky is 00B662F0 bit 0x2000 on the
        // object family, not land soup.
        SubmittedMesh = MeshBatches.Concat(land, combined);
        timing.Measure("Textures", () => { BindSubmittedTextures(); return _submittedTextures.Count; },
            n => $"n={n}");
        WorldSubmitted = SubmittedMesh.Vertices.Length > 0;
        SubmitC3dParsed = Meshes.ParsedCount - parsedBefore;
        SubmitElapsedMs = clock.Elapsed.TotalMilliseconds;
        LastLoadTiming = timing;
        Console.WriteLine(timing.Format());
        foreach (var row in timing.Rows)
            Timing.Add("submit/" + row.Name, row.Ms, row.Extra);
        Timing.Add("submit", SubmitElapsedMs,
            WorldSubmitted
                ? $"verts={SubmittedMesh.Vertices.Length} c3d={SubmitC3dParsed}"
                : "miss");
        Note(OpenStaticMapsFn, "Submit", "World",
            WorldSubmitted
                ? $"primary {opened.Region} cells={cells.Count} meshes={seen.Count} palskin={_submittedPalskin.Count} hero={HeroMeshId} terrain={_submittedTerrain.Count} verts={SubmittedMesh.Vertices.Length} {SubmitElapsedMs:0}ms c3d={SubmitC3dParsed}"
                : "submit miss");
    }

    /// <summary>
    /// Four side planes <c>00B2FD60</c> /
    /// <c>00BDC2D0</c>. Same extract
    /// <see cref="PresentWorld"/> already
    /// builds when the camera is seeded.
    /// </summary>
    public LandscapeFrustum.Plane[]? SubmitSidePlanes()
    {
        if (!WorldCamera.Seeded)
            return null;
        LandscapeFrustum.LetterboxCots(
            float.DegreesToRadians(Camera.FovDegrees), 4f / 3f, 1f,
            out var cotH, out var cotV);
        return LandscapeFrustum.ExtractSidePlanes(
            Camera.Position, Camera.Forward, Camera.Up, cotH, cotV);
    }

    private void BindSubmittedTextures()
    {
        _submittedTextures.Clear();
        _submittedTextureArray = null;
        if (SubmittedMesh is null)
            return;
        OpenTextureBank();
        if (Textures is null)
            return;
        var ids = SubmittedMesh.Draws.SelectMany(d =>
            new[] { d.TextureId, d.TextureId1 });
        foreach (var file in Textures.LoadMany(ids))
        {
            _submittedTextures.Add(new Fable.Render.GpuTexture(
                file.Id, file.Width, file.Height, file.Rgba));
        }

        _submittedTextureArray = [.. _submittedTextures];
    }

    private void EnsureStartupAvi()
    {
        if (Stage != EngineStage.StartupVideos || Install is null)
            return;
        if (StartupAvi is not null)
            return;
        if (CurrentStartupVideo is not { } video)
            return;
        var file = RegionTravel.ResolvePlayAviFile(Install, video.RelativePath);
        if (file is null)
        {
            Note(PlayAviPlayer, "StartupVideos", "PlayAVI", "miss " + video.RelativePath);
            FinishStartupVideo();
            EnsureStartupAvi();
            return;
        }

        ApplyPlayAviSlot(video);
        StartupAvi = WmvPlayer.TryOpen(file);
        Note(PlayAviPlayer, "StartupVideos", "PlayAVI",
            video.RelativePath + " " + (WmvPlayer.LastError ?? "ok"));
        if (StartupAvi is null)
        {
            FinishStartupVideo();
            EnsureStartupAvi();
        }
    }

    private void PumpStartupAvi(float dt)
    {
        EnsureStartupAvi();
        if (StartupAvi is null)
            return;
        StartupAvi.TryAdvance(dt);
        if (!StartupAvi.Ended)
            return;
        UnloadStartupAvi();
        if (Stage == EngineStage.StartupVideos)
            FinishStartupVideo();
        // Next Pump opens the next file. This
        // Present is AviPlaying=false so the
        // host clears the previous AVI first.
    }

    private bool QueuedPlayAviSkip()
    {
        foreach (var (_, key) in Input.Applied)
        {
            if (RegionTravel.IsPlayAviSkipScan(key))
                return true;
        }

        return false;
    }

    private void MaybeActivateNewGameFromInput()
    {
        if (Stage != EngineStage.Frontend)
            return;
        foreach (var (_, key) in Input.Applied)
        {
            if (key == RegionTravel.PlayAviSkipReturn)
            {
                DispatchFrontendMessage(FrontendNewGameMessage);
                return;
            }
        }
    }

    private void PresentToHost()
    {
        Host?.Present(BuildFrame());
    }

    public EngineFrame BuildFrame()
    {
        var avi = StartupAvi;
        var runtime = Runtime;
        var playing = avi is { Rgba: not null } ||
                      runtime is { AviPlaying: true, AviRgba: not null };
        var fade = runtime?.FadeColor ?? default;
        return new EngineFrame(
            Camera,
            SubmittedWorld,
            avi?.Width ?? runtime?.AviWidth ?? 0,
            avi?.Height ?? runtime?.AviHeight ?? 0,
            avi?.Rgba ?? runtime?.AviRgba,
            avi?.FrameSerial ?? runtime?.AviFrameSerial ?? 0,
            playing,
            runtime?.OverlayAlphaByte ?? 0,
            fade.R, fade.G, fade.B,
            SubmittedLandscape?.Vertices,
            SubmittedLandscape?.Draws,
            _submittedTextureArray,
            SubmittedObjects?.Vertices,
            SubmittedObjects?.Draws,
            SubmittedLandscape?.Indices,
            PlayAviClearArgb);
    }

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
            ApplyPlayAviSlot(StartupVideos[StartupVideoIndex]);
            return;
        }

        EnterFrontendAfterAvi();
    }

    /// <summary>
    /// <c>0042ED85</c> slot RGBA →
    /// <c>[0x13961E0]</c> before
    /// <c>006286F0</c>.
    /// </summary>
    private void ApplyPlayAviSlot(StartupVideo video)
    {
        PlayAviClearArgb = video.Rgba;
        Note(RetailPump, "StartupVideos", "PlayAVI", video.RelativePath);
        Note(PlayAviClearColorVa, "StartupVideos", "PlayAVI",
            $"013961E0 {video.Rgba:X8}");
    }

    /// <summary>
    /// <c>0042EE3D</c> after the video
    /// table: <c>0042E98F</c>, Init
    /// Engine, Init frontend, then
    /// <c>009D8CF0</c> / <c>009BEEB0</c>
    /// before the UI show calls.
    /// </summary>
    private void EnterFrontendAfterAvi()
    {
        PlayAviClearArgb = PlayAviClearRestoreArgb;
        Note(RetailBankSwapFlagVa, "StartupVideos", "PlayAVI",
            $"013B8616 {RetailBankSwapFlagFirstSeen} skip 009A8840");
        Note(RetailAfterAviFn, "InitFrontend", "Frontend",
            "0042E98F [esi+9]=1 00595582 +180");
        Note(DisplayModeFn, "InitFrontend", "D3D9",
            $"009BFF40 {DisplayModeWidth}x{DisplayModeHeight}");
        Note(0x0042EF40, "InitEngine", "Engine", "Init Engine");
        Note(FrontendEngineInitFn, "InitEngine", "Engine",
            "0042E204 +88 00B26340");
        Note(FrontendEngineAllocFn, "InitEngine", "Engine",
            $"00B26340 0x{FrontendEngineObjectSize:X} 00B260B0 012A0F3C");
        Note(0x0042EF6F, "InitFrontend", "Frontend", "Init frontend");
        Note(FrontendHelperCtor, "InitFrontend", "Frontend",
            $"0042DB40 size {FrontendHelperSize} vtbl 0x{FrontendHelperVtbl:X}");
        Note(ClearColorFn, "InitFrontend", "D3D9", "009D8CF0 clear");
        Note(PresentFn, "InitFrontend", "D3D9", "009BEEB0 Present");
        Note(RetailAudioFadeFn, "InitFrontend", "Audio", "0042DED5 0");
        Note(FrontendUiShowFn, "InitFrontend", "Frontend", "005952C3");
        Note(RetailFadeClockStartFn, "InitFrontend", "Frontend", "0062F800");
        Note(RetailFadeClockResetFn, "InitFrontend", "Frontend", "0062F8B0");
        Note(FrontendPostInitFn, "InitFrontend", "Frontend", "0040F0E0");
        Note(FrontendIntern, "Frontend", "FRONT_END", "0042F722");
        Stage = EngineStage.Frontend;
        InitFrontendUi();
    }

    /// <summary>
    /// <c>0041AFA0</c> dest: if
    /// <c>+360/+364</c> are 0 use
    /// <c>+204/+208</c>, then
    /// <c>* +264/+268</c> from
    /// <c>+248/+252</c>. First-seen
    /// ctor zeros the scale so dest
    /// is 0,0,0,0 even when
    /// <c>0041AC20</c> wrote sizes.
    /// </summary>
    public static (float X0, float Y0, float X1, float Y1) FrontendWidgetDest(
        int sizeW, int sizeH,
        float leftoverW, float leftoverH,
        float originX, float originY,
        float scaleX, float scaleY,
        bool center)
    {
        var w = sizeW == 0 ? leftoverW : sizeW;
        var h = sizeH == 0 ? leftoverH : sizeH;
        w *= scaleX;
        h *= scaleY;
        var x0 = originX;
        var y0 = originY;
        if (center)
        {
            x0 -= w * RegionTravel.PlayAviLetterboxHalf;
            y0 -= h * RegionTravel.PlayAviLetterboxHalf;
        }

        return (x0, y0, x0 + w, y0 + h);
    }

    /// <summary>
    /// <c>00595582</c> then <c>00595B24</c>
    /// menu labels. Does not leave frontend.
    /// </summary>
    public void InitFrontendUi()
    {
        if (FrontendUiPresent)
            return;
        Note(FrontendUiGet, "Frontend", "UI",
            "00595582 [0x13B8B5C] size 0xE0");
        Note(FrontendUiCtor, "Frontend", "UI",
            "005953E2 vtbl 012521A8");
        Note(FrontendMainMenuFn, "Frontend", "UI", "0059899A");
        Note(FrontendMenuAttachFn, "Frontend", "UI", "00595A06 [ui+84] id=0");
        Note(InputActionGetter, "Frontend", "UI", "0041E5F2");
        Note(FrontendWidgetFactoryFn, "Frontend", "UI",
            "0041DB1D " + FrontendMainMenuNoContinue);
        Note(MeshBank.DefLookupFn, "Frontend", "UI",
            "009AD410 " + FrontendMainMenuNoContinue);
        Note(FrontendWidgetConstructFn, "Frontend", "UI",
            "0041D21B [def+60] type0 0041B800");
        Note(FrontendWidgetType0Ctor, "Frontend", "UI",
            $"0041B800 vtbl 0x{FrontendWidgetVtbl:X} +{FrontendWidgetBlendOffset}={FrontendWidgetBlendDefault}");
        Note(FrontendWidgetPostCtorFn, "Frontend", "UI",
            $"0041AC20 vtbl+{FrontendWidgetFontListVtbl} 0x{FrontendWidgetFontListFn:X}");
        FrontendWidgetBlend = FrontendWidgetBlendDefault;
        // Empty [obj+64..+68] → [+376]=0 → jbe 0041AF6F.
        // +204/+208 not written. +248/+264 ctor 0.
        FrontendWidgetFont = 0;
        FrontendWidgetTexture = 0;
        var dest = FrontendWidgetDest(
            sizeW: 0, sizeH: 0,
            leftoverW: 0, leftoverH: 0,
            originX: 0, originY: 0,
            scaleX: 0, scaleY: 0,
            center: false);
        FrontendWidgetDestX0 = dest.X0;
        FrontendWidgetDestY0 = dest.Y0;
        FrontendWidgetDestX1 = dest.X1;
        FrontendWidgetDestY1 = dest.Y1;
        Note(FrontendWidgetPostCtorFn, "Frontend", "UI",
            "0041AC20 [+376]=0 skip dest");
        Note(FrontendWidgetDrawFn, "Frontend", "UI",
            $"0041AFA0 dest {dest.X0},{dest.Y0},{dest.X1},{dest.Y1} +{FrontendWidgetOriginXOffset}/+{FrontendWidgetScaleXOffset}=0");
        FrontendMenuRoot = FrontendMainMenuNoContinue;
        FrontendMenuConstructed = true;
        Note(FrontendUiBuildMenu, "Frontend", "UI", "00595B24");
        foreach (var (label, id) in FrontendMenuItems)
            Note(FrontendUiBuildMenu, "Frontend", "UI", $"{label} id={id}");
        FrontendUiPresent = true;
    }

    /// <summary>
    /// <c>0042EC7C</c> inner frontend frame.
    /// Present is <c>009BEEB0</c> inside
    /// <c>0042DF9E</c>, same device Present
    /// the Vulkan path already translates.
    /// </summary>
    public void PumpFrontendFrame()
    {
        Note(FrontendInputFn, "Frontend", "Input",
            "0042E3EE walk [0x13B8388]");
        Note(InputDeviceVa, "Frontend", "Input", "engine+88 DINPUT8");
        Note(InputPollFn, "Frontend", "Input", "009F4ED0");
        Note(InputEventFn, "Frontend", "Input",
            "00A03B40 type [record+40]");
        Note(InputEventKeyFn, "Frontend", "Input",
            "00A03B70 key [record+0]");
        PumpInput();
        Note(FrontendUpdateFn, "Frontend", "UI", "0042DC94");
        Note(FrontendUiTickFn, "Frontend", "UI", "00599E3F");
        Note(FrontendRecordZeroFn, "Frontend", "Render",
            $"0042FA30 zero {FrontendRecordSize}");
        Note(FrontendRecordFillFn, "Frontend", "Render", "0042DBFA");
        Note(FrontendDrawFn, "Frontend", "Render", "0042DF9E");
        Note(ClearColorFn, "Frontend", "D3D9", "009D8CF0 clear");
        Note(BeginSceneFn, "Frontend", "D3D9", "009BEF20 BeginScene");
        Note(FrontendUiGet, "Frontend", "UI", "00595582");
        DrawFrontendWidgets();
        Note(InputActionGetter, "Frontend", "Input", "0041E5F2");
        FlushFrontendDisplay();
        Note(FrontendDisplayHelperFn, "Frontend", "D3D9",
            $"00404A80 0x{FrontendDisplaySingletonVa:X}");
        ApplyFrontendDisplay();
        FlushFrontendDisplay();
        Note(EndSceneFn, "Frontend", "D3D9", "009BEF50 EndScene");
        Note(PresentFn, "Frontend", "D3D9", "009BEEB0 Present");
        FrontendFrameCount++;
        FrontendPresentCount++;
    }

    /// <summary>
    /// <c>00595222</c> is the <c>[ui+84]</c>
    /// walk only: <c>[node+20].vtbl+8</c>
    /// then <c>004292C0</c>. It is not a
    /// DIP. First-seen nonempty node
    /// calls <c>0041AFA0</c>; DIP is
    /// later <c>009DA9F0</c> empty skip.
    /// </summary>
    private void DrawFrontendWidgets()
    {
        FrontendWidgetsDrawn = 0;
        Frontend2dRecordsQueued = 0;
        Note(FrontendUiDrawFn, "Frontend", "UI",
            $"00595222 [ui+{FrontendWidgetListOffset}]");
        if (!FrontendMenuConstructed)
            return;
        // One [node+20] from 00595A06. Empty
        // 00595B24 ids stay null and skip.
        Note(FrontendWidgetDrawFn, "Frontend", "UI",
            $"0041AFA0 vtbl+{FrontendWidgetDrawVtbl} 0122F5D4");
        QueueFrontend2dRecord();
        Note(FrontendWidgetNextFn, "Frontend", "UI", "004292C0");
        FrontendWidgetsDrawn = 1;
    }

    /// <summary>
    /// <c>0041AFA0</c> first-seen:
    /// <c>[+380]==0</c> and <c>[+376]==0</c>
    /// so <c>0041BEB0</c> at <c>0041B47C</c>
    /// (type <c>0x22</c>, dest 0xC0), then
    /// <c>[edx+92]</c> because 00595222
    /// passes the two optional args as 0.
    /// Sibling <c>0041BF60</c> needs
    /// <c>[+380]!=0</c> — ctor leaves 0.
    /// Dest is <c>0041AFA0</c>
    /// <c>+248/+264</c> ctor 0 →
    /// <c>0,0,0,0</c>. Packer writes
    /// type/size into that dest.
    /// </summary>
    private void QueueFrontend2dRecord()
    {
        var sibling = FrontendWidgetTexture != 0;
        var packer = sibling ? FrontendWidgetQueueSiblingFn : FrontendWidgetQueueFn;
        Note(packer, "Frontend", "UI",
            sibling
                ? $"0041BF60 type 0x{Frontend2dRecordType:X} [+380]"
                : $"0041BEB0 type 0x{Frontend2dRecordType:X} +{FrontendWidgetBlendOffset}={FrontendWidgetBlend}");
        Note(packer, "Frontend", "UI",
            $"[edx+{Frontend2dSubmitVtbl}] dest +{FrontendWidgetSubmitDestOffset:X} 0x{Frontend2dRecordBytes:X} {FrontendWidgetDestX0},{FrontendWidgetDestY0},{FrontendWidgetDestX1},{FrontendWidgetDestY1}");
        Note(FrontendEngineAllocFn, "Frontend", "UI",
            $"00B26340 size 0x{FrontendEngineObjectSize:X} vtbl 0x{FrontendEngineVtbl:X}");
        Note(FrontendSubmitFn, "Frontend", "UI",
            $"00B23BC0 → 00B324A0 [0x{FrontendSubmitSingletonVa:X}] type 0x{Frontend2dRecordType:X}");
        Note(FrontendSubmitDispatchFn, "Frontend", "UI",
            "00B324A0 [dest+4]=0 handler vtbl+20 UNREAD");
        Frontend2dLastType = Frontend2dRecordType;
        Frontend2dLastPacker = packer;
        Frontend2dLastSubmitVtbl = Frontend2dSubmitVtbl;
        Frontend2dRecordsQueued++;
    }

    /// <summary>
    /// <c>00404C00</c> on the
    /// <c>00404A80</c> singleton.
    /// First-seen <c>[+8]==0</c> returns
    /// before IME / cursor.
    /// </summary>
    private void ApplyFrontendDisplay()
    {
        Note(FrontendDisplayHelper2Fn, "Frontend", "D3D9",
            $"00404C00 [+{FrontendDisplayFlagOffset}]={(FrontendDisplayFlag ? 1 : 0)}");
        if (!FrontendDisplayFlag)
        {
            Note(FrontendDisplayHelper2Fn, "Frontend", "D3D9",
                "00404C00 skip BSS [+8]==0");
            FrontendDisplayImeRan = false;
            FrontendDisplayCursorRan = false;
            return;
        }

        Note(FrontendDisplayImeFn, "Frontend", "D3D9", "00CB38E0(1,1)");
        FrontendDisplayImeRan = true;
        Note(InputActionGetter, "Frontend", "Input", "0041E5F2");
        Note(FrontendDisplayCursorFn, "Frontend", "D3D9",
            "0041A980 [input+184] UNREAD");
        FrontendDisplayCursorRan = false;
    }

    /// <summary>
    /// <c>0042DF9E</c> <c>009D9C80</c> then
    /// <c>009DA9F0(1)</c>. Same helpers as
    /// game <c>00435530</c>.
    /// </summary>
    private void FlushFrontendDisplay()
    {
        Note(DisplayFlush2dFn, "Frontend", "D3D9",
            "009D9C80 [0x13BC800] device flags");
        Note(DisplayFlush2dFn, "Frontend", "D3D9",
            "009D9C80 [0x13CB508]+10248 bump");
        Note(DisplayFlush2dFn, "Frontend", "D3D9",
            "009D9C80 dirty-list no type 0x22 in 009D9C80-009DB000");
        var shouldDip = DisplayFlushShouldDip(0, 0);
        Note(DisplayFlushLayersFn, "Frontend", "D3D9",
            shouldDip
                ? $"009DA9F0({DisplayFlushLayersArg}) [+{DisplayQueueBeginOffset}] DIP vtbl+{DrawIndexedPrimitiveVtbl}"
                : $"009DA9F0({DisplayFlushLayersArg}) [+{DisplayQueueBeginOffset}] empty");
        Note(DisplayFlushLayersFn, "Frontend", "D3D9",
            shouldDip
                ? $"00A058C0 then vtbl+{DrawIndexedPrimitiveVtbl} prim {DisplayFlushPrimitive(false)}/{DisplayFlushPrimitive(true)}"
                : "009DA9F0 skip DIP no type 0x22");
        Frontend2dDipIssued = shouldDip;
        FrontendFlushCount++;
    }

    /// <summary>
    /// <c>0059A238</c> is UI vtbl+32
    /// (<c>012521A8+32</c> =
    /// <c>012521C8</c>). Message 15
    /// is <c>0059A2DA</c>.
    /// </summary>
    public const int FrontendUiMessageVtbl = 32;

    public void DispatchFrontendMessage(int msg)
    {
        if (Stage != EngineStage.Frontend)
            return;
        if (!FrontendUiPresent)
            InitFrontendUi();
        Note(FrontendUiMessageFn, "Frontend", "UI",
            $"0059A238 msg={msg} vtbl+{FrontendUiMessageVtbl}");
        if (msg != FrontendNewGameMessage)
            return;
        Note(FrontendNewGameApply, "Frontend", "UI",
            "0059A2DA [ui+28] vtbl+16");
        Note(FrontendNewGameThunk, "Frontend", "UI",
            $"00594F28 [retail+{RetailNewGameFlagOffset}]=1");
        RetailNewGameFlag = true;
    }

    /// <summary>
    /// <c>0059A238</c> message 15:
    /// <c>[ui+28]</c> vtbl+16 then
    /// <c>[retail+41]=1</c>.
    /// </summary>
    public void ActivateNewGame() =>
        DispatchFrontendMessage(FrontendNewGameMessage);

    /// <summary>
    /// <c>005959AB</c> walks the menu list.
    /// Miss is <c>00595A03 xor al,al</c>.
    /// </summary>
    public bool FrontendMenuContains(string label)
    {
        Note(FrontendMenuSearchFn, "Frontend", "UI", "005959AB");
        var hit = FrontendMenuItems.Any(i => i.Label == label);
        if (!hit)
            Note(FrontendMenuMissFn, "Frontend", "UI", "00595A03 miss");
        return hit;
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
        var ng = System.Diagnostics.Stopwatch.StartNew();
        Note(LeaveFrontendSite, "LeaveFrontend", "Frontend", "Leave frontend");
        Note(VideoPlayFlagVa, "LeaveFrontend", "PlayAVI",
            $"01375448=0 was {DefaultVideoPlayFlag}");
        Note(RetailBankSwapFlagVa, "LeaveFrontend", "Frontend",
            $"013B8616 {RetailBankSwapFlagFirstSeen} skip 009A78D0/009A8840");
        Note(LeaveFrontendPathFn, "LeaveFrontend", "Frontend", "00404490");
        Note(LeaveFrontendRecordFn, "LeaveFrontend", "Frontend", "004131A0");
        WorldFileName = FinalAlbionWld;
        Note(0x0042F44D, "LeaveFrontend", "World", FinalAlbionWld);
        Note(LeaveFrontendTeardownFn, "LeaveFrontend", "Frontend",
            "0042EBB6 +41 skip audio stop");
        Note(LeaveFrontendClearFn, "LeaveFrontend", "D3D9", "009BE420 clear");
        Note(PresentFn, "LeaveFrontend", "D3D9", "009BEEB0 Present");
        Stage = EngineStage.LeaveFrontend;
        Timing.Add("frontend NG", ng.Elapsed.TotalMilliseconds, FinalAlbionWld);
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
        Note(GameSingletonVa, "InitGame", "GameStart",
            $"013B86A0 game [retail+0] successor 0x{RetailSuccessorVa:X}");
        Note(0x009E9EF0, "InitGame", "GameStart", "009E9EF0 / 009E9F90 / 00416832");
        foreach (var (name, apply) in InitGameStages)
        {
            if (name == "Init Conversation Attitude")
                Note(0x0041863D, "InitGame", "InitGame", "Adding Console Variables");
            Note(apply, name, "InitGame", name);
            if (name == "Init Graphics")
                OpenTextureBank();
            if (name == "Init Player Interface")
            {
                Player.Construct();
                Note(PlayerInterfaceCtor, "InitGame", "Input",
                    "004473A0 size 0x898 vtbl 01231BDC game+32");
                Note(PlayerOwnerCtor, "InitGame", "Input",
                    "0044A3B0 game+28 size 44 +12 empty +24=0");
                Note(PlayerListenerFactoryFn, "InitGame", "Input",
                    "00488D20 00687A30 vtbl 0123758C +4");
                Note(PlayerListenerRegisterFn, "InitGame", "Input",
                    "00687A70 00A0D2B0 00A0D4F0");
            }

            if (name == "Init World")
                Note(InitWorldFn, "InitGame", "World",
                    "00418790 [world+320] from 013B7C90");
            if (name == "Load Particles")
                Note(SkipParticlesVa, "InitGame", "InitGame",
                    $"013B8648={SkipParticlesFirstSeen} run 004174F1");
        }
        Note(InitWorldFn, "Init World", "World", "004A67D0 vtbl 012390F0");
        Note(InitWorldInitFn, "Init World Init", "World", "004A6E30 vtbl+36");
        foreach (var (name, apply) in InitWorldInitStages)
        {
            Note(apply, name, "World", name);
            if (apply == InitMeshBankFn)
                OpenMeshBank();
        }
        InitWorldCameras();
        CreatePlayers();
        LoadWorld();
        GameRenderEnabled = true;
        Note(GameLoadWorldFn, "InitGame", "GameStart",
            "004188E9 [game].vtbl+32 00416953");
        FinishInitGameAfterWorld();
        Note(GameStart, "InitGame", "GameStart",
            $"[game+{GameReadyOffset}]=1 90544/90548 QPC");
        Note(GameModeCtorRenderEnable, "InitGame", "GameMode",
            "00418EC6 [game+90593]=1");
        SeedWorldTick();
        Mode = EngineMode.Game;
        Stage = EngineStage.Game;
        WorldFileName = FinalAlbionWld;
    }

    /// <summary>
    /// <c>004184BD</c> after vtbl+32.
    /// No-save <c>[0x13B8648]==0</c> only.
    /// Not a region load and not the
    /// first <c>004189C2</c> pump.
    /// </summary>
    public void FinishInitGameAfterWorld()
    {
        Note(SkipParticlesVa, "InitGame", "GameStart",
            $"013B8648={SkipParticlesFirstSeen} after 00416953");
        Note(PostLoadWorldReserveFn, "InitGame", "GameStart",
            $"0049BA70 +{GamePlus90488Offset} count={PostLoadWorldReserveCount} rate={PostLoadWorldReserveRate}");
        Note(WorldThingCountFn, "InitGame", "GameStart",
            $"00416392 +{GamePlus90394Offset}=0 → 0049E200");
        Note(WorldThingCountApply, "InitGame", "GameStart",
            "0049E200 0051E530+[0x13B89BC]");
        if (PlayerActionReady)
            Note(PlayerBindAfterWorldFn, "InitGame", "Player",
                $"004AE9D0 +{PlayerActionFlagOffset} +{PlayerBindSlot0Offset}/+{PlayerBindSlot1Offset}/+{PlayerBindSlot2Offset}");
        var defaultIni = Install is null
            ? DefaultUserIniName
            : Path.Combine(Install.Root, DefaultUserIniName);
        if (File.Exists(defaultIni))
            Note(IniApplyFn, "InitGame", "Ini",
                "009EC890 " + DefaultUserIniName);
        else
            Note(FileExistsFn, "InitGame", "Ini",
                "00999230 " + DefaultUserIniName + " miss");
        Note(UserIniVa, "InitGame", "Ini", "009EC890 " + UserIniName);
        var userIni = Install is null
            ? null
            : Path.Combine(Install.Root, UserIniName);
        if (userIni is not null && File.Exists(userIni))
        {
            Note(IniApplyFn, "InitGame", "Ini",
                "009EC890 " + UserIniName + " exists");
            Note(IniTokenizeFn, "InitGame", "Ini", "009EC710");
            Note(IniDispatchFn, "InitGame", "Ini", "009EB430 [ini+64] vtbl+4");
            ApplyUserIniCommands(userIni);
        }

        Note(EngineReadyCallback, "InitGame", "GameStart",
            $"009A4EC0 [engine+{EngineReadyCallbackOffset}]=004167DA +{EngineGamePtrOffset}=game");
    }

    /// <summary>
    /// <c>009EC710</c> walks tokens;
    /// <c>009EB430</c> looks up
    /// <c>[ini+64]</c> and calls
    /// handler vtbl+4. Command bodies
    /// (including
    /// <c>ActivateQuest("Gameflow")</c>)
    /// stay UNREAD — do not start a
    /// quest here.
    /// </summary>
    private void ApplyUserIniCommands(string path)
    {
        var names = new List<string>();
        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('#'))
                continue;
            var end = line.IndexOfAny(['(', ' ', '\t']);
            var name = end < 0 ? line.TrimEnd(';') : line[..end];
            if (name.Length == 0)
                continue;
            names.Add(name);
            Note(IniDispatchFn, "InitGame", "Ini", "009EB430 " + name);
        }

        UserIniCommands = names;
    }

    /// <summary>
    /// Game vtbl+32 <c>00416953</c>.
    /// <c>[world].vtbl+28([game+40])</c> then
    /// no-save <c>[+90588]</c> empty skips
    /// <c>004A3200</c>. Path is <c>+90576</c>
    /// <c>FinalAlbion.wld</c> from Leave
    /// frontend, not the <c>0x122EE14</c>
    /// fallback. <c>004A1840</c> then
    /// <c>[0x13B8648]==0</c> → <c>0049F180</c>
    /// / <c>004B4A10</c>, then <c>004BBC00</c>
    /// <c>ret 4</c>. Not a region load.
    /// </summary>
    public void LoadWorld()
    {
        Note(GameLoadWorldFn, "Loading world", "World",
            "00416953 vtbl+32 [+90588] empty");
        Note(WorldPrepareSite, "Loading world", "World",
            $"[world].vtbl+{WorldPrepareVtbl}([game+40])");
        Note(LoadSaveFn, "Loading world", "World",
            "004A3200 Loading save skipped");
        Note(GameLoadWorldFn, "Loading world", "World", "Loading world");
        Note(GameWorldPathCopyFn, "Loading world", "WLD",
            $"+{GameWorldPathOffset} {FinalAlbionWld} not {WorldPathDefault}");
        LoadWorldMap();
        if (SkipParticlesFirstSeen == 0)
        {
            Note(SkipParticlesVa, "Loading world", "World",
                $"013B8648={SkipParticlesFirstSeen} 0049F180");
            InitCharactersAndQuests();
        }

        Note(AfterLoadWorldFn, "Loading world", "World",
            "004BBC00 ret 4 013B8674");
    }

    /// <summary>
    /// <c>004A1840</c> from <c>00416953</c>:
    /// <c>00507C30</c> token-switch parse of
    /// <c>FinalAlbion.wld</c>. Then GTNG,
    /// global things, region graph, QST
    /// <c>004A0D90</c> into world+184,
    /// Startup WAD, Set Static Map.
    /// <c>0049F180</c> / <c>004B4A10</c> are
    /// siblings after this call, not children.
    /// <c>005066E0</c> ctor already ran in
    /// Init World Init.
    /// </summary>
    public void LoadWorldMap()
    {
        Note(LoadQuestsFn, "Load Quests", "WLD",
            "004A1840 Load Quests / WLD / Startup WAD");
        Note(LoadWldFile, "Load .wld file", "WLD", "00507C30 vtbl+12");
        WorldFileName = FinalAlbionWld;
        if (Install is not null)
        {
            if (!File.Exists(Install.WorldPath))
                Note(LoadWldFile, "Load .wld file", "WLD", "missing " + FinalAlbionWld);
            else
            {
                World = Timing.Measure("WLD", () => WorldFile.Load(Install.WorldPath),
                    w => $"maps={w.Maps.Count} regions={w.Regions.Count}");
                Note(LoadWldFile, "Load .wld file", "WLD",
                    $"maps={World.Maps.Count} regions={World.Regions.Count} quests={World.InitialQuests.Count}");
                LoadGtngFile();
                Timing.Measure("TNG global", () => { LoadGlobalThingsFile(); return GlobalThingMapsLoaded; },
                    n => $"maps={n} things={GlobalThings?.Things.Count() ?? 0}");
                Timing.Measure("region graph", () => { LoadRegionGraphFile(); return Regions?.Neighbors.Count ?? 0; },
                    n => $"nodes={n}");
                Timing.Measure("quests", () => { LoadQuestDefs(); return Quests?.Quests.Count ?? 0; },
                    n => $"defs={n}");
            }
        }

        Note(StartupWadSite, "Loading world", "WLD", "Startup WAD");
        Note(SetStaticMapForEngineSite, "Loading world", "WLD",
            "Set Static Map for Engine");
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
        EnsureLevels();
        var loaded = new List<ThingInstance>();
        foreach (var map in World.Maps)
        {
            if (!map.LoadedOnPlayerProximity)
                continue;
            var tng = _levels?.TryLoadThings(map.ScriptName);
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
        Note(PlayerObjectInitPredicate, "Create Players", "Player",
            "0099A350 al=1 [ecx+4]=1");
        PlayerActionReady = true;
        PlayerObjectReady = true;
        Note(PlayerObjectInit, "Create Players", "Player",
            $"+{PlayerActionFlagOffset}=1 +9824=1");
        Note(ThingTypeRegistrarFn, "Create Players", "Thing",
            "00522A20 PlayerCreature CREATURE 0052B880");
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
        ApplyFirstPumpAviAndFade();
        Note(GamePumpInnerStartFn, "GamePump", "Game", "0098E1B0 ret");
        Note(GamePumpQuitQuery, "GamePump", "Engine",
            $"009A6460 [engine+8]=0 → {GamePumpQuitFirstSeen}");
        GamePumpFirstDone = true;
        Note(GamePumpMemlog, "GamePump", "Game", "00415E85 memlog");
        PumpGameUpdate();
    }

    /// <summary>
    /// First <c>004189C2</c> after dummy
    /// <c>004FC180</c>: <c>0040D2A0</c> /
    /// <c>0040BC80</c> / vtbl+220
    /// <c>00B239A0(12, 20.0)</c> /
    /// <c>009F2660</c>+<c>009F26B0</c>.
    /// Then the inner loop. Not a region.
    /// </summary>
    public void ApplyFirstPumpAviAndFade()
    {
        Note(PlayAviSingletonFn, "GamePump", "PlayAVI",
            "0040D2A0 [0x13B7D4C] alloc 0x140");
        Note(PlayAviSingletonCtor, "GamePump", "PlayAVI",
            $"0040CEC0 +51={PlayAviPlus51FirstSeen}");
        PlayAviSingletonReady = true;
        Note(PlayAviApplyFn, "GamePump", "PlayAVI", "0040BC80");
        Note(PlayAviPrepareFn, "GamePump", "PlayAVI", "00407370");
        Note(PlayAviApplyBodyFn, "GamePump", "PlayAVI",
            "0040A7F0 +51 first-seen");
        Note(DisplayEngineFadeFn, "GamePump", "Display",
            $"00B239A0 vtbl+{DisplayEngineFadeVtbl} ({DisplayEngineFadeType}, {DisplayEngineFadeSeconds})");
        DisplayEngineFadeSet = true;
        DisplayEngineFadeKind = DisplayEngineFadeType;
        DisplayEngineFadeTime = DisplayEngineFadeSeconds;
        Note(InputLockObjectVa, "GamePump", "Input", "013CAA90");
        Note(InputLockEnterFn, "GamePump", "Input", "009F2660 vtbl+52 walk");
        Note(InputLockLeaveFn, "GamePump", "Input", "009F26B0 lock pair");
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
        // After 006C2170 / OpenStaticMaps,
        // before 00435530. Native draw
        // consumes already-opened maps.
        if (HeroSpawned && !WorldSubmitted)
            SubmitCurrentWorld();
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
        Input.Construct();
        Note(InputActionGetter, "GamePump", "Input",
            "0041E5F2 [0x13B8710] size 0xD0");
        if (Input.Busy)
            Note(InputActionGetter, "GamePump", "Input",
                $"+{EngineInput.BusyOffset} busy");
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
                PumpPlayerInterface();
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
    /// <c>00416E78</c> after
    /// <c>WorldFrame&gt;1</c>:
    /// <c>004457F0</c> then
    /// <c>[game+32].vtbl+4</c>
    /// <c>00446A30</c> until it
    /// returns 0.
    /// </summary>
    public void PumpPlayerInterface()
    {
        if (!Player.Present)
            return;
        if (WorldFrame <= 1)
            return;
        Player.Preprocess();
        Note(PlayerInterfacePreprocess, "GamePump", "Input",
            "004457F0 [+2196]=0");
        Note(PlayerInputPumpFn, "GamePump", "Input",
            "00446A30 [game+32] vtbl+4");
        Note(PlayerInputPollFn, "GamePump", "Input",
            "00446330 009F4ED0 vtbl+32/00449990/+16");
        Note(PlayerInputFallbackFn, "GamePump", "Input",
            "00446220 vtbl+24 [+168]");
        var n = 0;
        while (Player.Pump(Input) && n < 32)
        {
            n++;
            if (Player.LastEvent is { } ev)
                ApplyPlayerEvent(ev);
        }

        if (n > 0)
            Note(PlayerInputPumpFn, "GamePump", "Input",
                $"delivered {n}");
    }

    /// <summary>
    /// <c>00416E78</c> after a
    /// <c>00446A30</c> hit:
    /// <c>0041649C</c> unless paused.
    /// </summary>
    public void ApplyPlayerEvent(PlayerEvent ev)
    {
        if (GameModePaused)
        {
            Note(PlayerInterface.ApplyAction2Fn, "GamePump", "Input",
                "00415FF2 paused");
            return;
        }

        Note(PlayerApplyFn, "GamePump", "Input", "0041649C");
        var hit = Player.ApplyInputEvent(ev, PlayerActionReady);
        if (hit && PlayerActionReady)
        {
            Note(PlayerApplyPlayerFn, "GamePump", "Input",
                "004AE9A0 +9826 009F1650");
            Note(PlayerApplyQueueFn, "GamePump", "Input",
                $"queued {Player.QueuedCount} action {ev.Action}");
        }

        Note(PlayerInterface.ApplyWorldFn, "GamePump", "Input", "0049E1D0");
        Note(PlayerInterface.ApplyDisplayFn, "GamePump", "Input", "00434A30");
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
            ApplyCameraInterpolation();
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
    /// <c>0041707E</c>. Default
    /// <c>world+164==0</c> builds one
    /// 52-byte record, clamps t to
    /// <c>[0,1]</c>, then
    /// <c>0049E080</c> / <c>00435F70</c>.
    /// <c>0041714D</c> is UNREAD.
    /// </summary>
    public void ApplyCameraInterpolation()
    {
        Note(CameraInterpolationFn, "GamePump", "Render", "0041707E interpolation");
        if (WorldPlus164 != 0)
        {
            Note(CameraInterpolationFn, "GamePump", "Render",
                "0041714D world+164 UNREAD");
            CameraInterpolationUnread = true;
            return;
        }

        Note(CameraInterpTimeFn, "GamePump", "Time", "004166E2");
        var t = DisplayTime * CameraCatchupMin - GamePlus72;
        if (t < 0)
            t = 0;
        else if (t > 1)
            t = 1;
        Note(CameraClampFn, "GamePump", "Render", $"0041919C t={t}");
        CameraInterpolationT = (float)t;
        var old112 = GamePlus112;
        var old116 = GamePlus116;
        GamePlus112 = CameraInterpolationT;
        GamePlus116 = 0f;
        GamePlus120 = old112;
        GamePlus124 = old116;
        GamePlus128 = CameraInterpolationT;
        GamePlus132 = 0f;
        GamePlus160 = GamePlus72;
        LastCameraTime = CameraInterpolationT;
        LastCameraBlend = 0f;
        ApplyWorldCamera(CameraInterpolationT);
        Note(PlayerReadyQueryFn, "GamePump", "Player",
            $"004AEA70 +{PlayerActionFlagOffset}={PlayerActionReady}");
        ApplyDisplayCamera();
        GamePlus90424++;
        GamePlus104 = 0;
        GamePlus90594 = true;
        CameraInterpolationRan = true;
        CameraBodySteps++;
        Note(CameraInterpolationFn, "GamePump", "Render",
            $"[game+90594]=1 t={CameraInterpolationT}");
    }

    /// <summary>
    /// <c>00BFEA70</c> toward-zero fistp.
    /// </summary>
    public static int FistpTowardZero(double value) =>
        value >= 0 ? (int)value : -(int)(-value);

    /// <summary>
    /// After Environment: alloc
    /// <c>0x1970</c> <c>006B4900</c> at
    /// world+24. Then named manager
    /// <c>0069AE80</c> at +48 and
    /// <c>006FD8C0</c> at +44.
    /// </summary>
    public void InitWorldCameras()
    {
        Note(WorldCameraCtor, "Init World Init", "Camera",
            "006B4900 world+24 size 0x1970 vtbl 0125D53C");
        WorldCamera.Construct();
        WorldCameraPresent = true;
        Note(GameCameraManagerCtor, "Init Game Camera Manager", "Camera",
            "0069AE80 world+48 size 0x160 vtbl 0125C754");
        GameCameraManager.Construct();
        Note(GameCameraCtor, "Init Game Camera", "Camera",
            "006FD8C0 world+44 size 0xC8 vtbl 01264A8C");
        GameCamera.Construct();
        Note(GameCameraCtor, "Init Game Camera", "Camera",
            $"006FD8C0 +176={GameCamera.Plus176}");
    }

    /// <summary>
    /// <c>0049E080</c>: store thing, walk
    /// <c>0051EBD0</c>, blend
    /// <c>006B42F0(world+24, t)</c>.
    /// Output <c>+6296/+6312/+6328</c>
    /// writes <see cref="Camera"/>.
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
        if (!WorldCameraPresent)
            WorldCamera.Construct();
        if (!WorldCamera.Seeded)
            Note(WorldCameraSeedFn, "GamePump", "Camera", "006B3FF0 +68");
        var output = WorldCamera.Blend(tBlend);
        Note(CameraManagerBlendFn, "GamePump", "Camera",
            $"006B42F0 t={tBlend} out={output.V0}");
        // 00B314E0 copies helper +0/+12/+24
        // as eye / forward / up and
        // 00A14440-normalises the dirs.
        // First-seen +6296 is the ctor
        // axis, not an eye. First-seen
        // up is (0,0,1), not 006B2CA0 V2.
        if (WorldCamera.IsCtorAxis(output.V0) &&
            Hero is { PositionX: not null, PositionY: not null })
        {
            var eye = RegionTravel.PositionOf(Hero);
            var forward = output.V4.LengthSquared() > 1e-8f
                ? output.V4
                : WorldCamera.SlotA.V4;
            if (forward.LengthSquared() < 1e-8f)
                forward = -Vector3.UnitX;
            Camera.ApplyRendererHelper(
                eye, forward, LandscapeFrustum.FirstSeenCameraUp);
            Camera.SetFovDegrees(GameCamera.FirstSeenFovDegrees);
            RendererHelperBound = true;
        }
        else
        {
            Camera.ApplyManagerOutput(output.V0, output.V1, output.V2);
            RendererHelperBound = false;
        }
    }

    /// <summary>
    /// <c>00435F70</c> jmp <c>00435530</c>.
    /// BeginScene / overlay / flush /
    /// EndScene / Present. Layer bits
    /// are <see cref="ScenePasses"/>
    /// flushed by <c>009DA9F0</c>.
    /// </summary>
    public void ApplyDisplayCamera()
    {
        Note(DisplayApplyThunk, "GamePump", "Display",
            "00435F70 jmp 00435530");
        Note(DisplayApplyBodyFn, "GamePump", "Display", "00435530");
        Note(BeginSceneFn, "GamePump", "D3D9", "009BEF20 BeginScene");
        Note(ClearColorFn, "GamePump", "D3D9", "009D8CF0 clear");
        Note(DisplayPlayerOverlayFn, "GamePump", "Display",
            "00435000 → 00639E40");
        Note(DisplayPlayerOverlayApply, "GamePump", "Display", "00639E40");
        Note(DisplayPlayerInterfaceFn, "GamePump", "Display", "00435070");
        Note(DisplayFlush2dFn, "GamePump", "D3D9",
            "009D9C80 flush DIP vtbl+332");
        Note(DisplayFlushLayersFn, "GamePump", "D3D9",
            "009DA9F0(1) layer flush");
        Note(RenderFrameFn, "GamePump", "Display",
            "00B25950 layers +348…+352");
        FlushSubmittedLayers();
        Note(EndSceneFn, "GamePump", "D3D9", "009BEF50 EndScene");
        Note(GamePresentSite, "GamePump", "D3D9", "00435F50");
        Note(PresentFn, "GamePump", "D3D9", "009BEEB0 Present");
        GamePresentCount++;
    }

    /// <summary>
    /// <c>009DA9F0</c> draws the queued
    /// layer bits. Order is
    /// <see cref="ScenePasses.Registration"/>.
    /// </summary>
    private void FlushSubmittedLayers()
    {
        _submittedLayers.Clear();
        foreach (var pass in ScenePasses.Registration)
        {
            if (!ScenePasses.Draws(pass.Submit))
                continue;
            _submittedLayers.Add(pass.Bit);
            Note(DisplayFlushLayersFn, "GamePump", "Layer",
                $"bit 0x{pass.Bit:X} {pass.Submit}");
        }

        LayerFlushCount++;
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
        PumpQuests();
        WorldFrame++;
        Note(WorldFrameIncSite, "GamePump", "World",
            $"004A5E10 inc WorldFrame={WorldFrame}");
    }

    /// <summary>
    /// <c>004A5D88</c> <c>004B4490</c>
    /// when world+260 is 0. Walks
    /// <c>00CB8220</c>. First-seen
    /// <c>[fiber+41]==0</c> so
    /// <c>00CB7950</c> takes
    /// <c>vtbl+4</c>, not
    /// <c>00A44880</c>.
    /// </summary>
    public void PumpQuests()
    {
        Note(QuestManagerPumpFn, "GamePump", "Quest",
            $"004B4490 [0x{QuestManagerVa:X}]");
        Note(QuestListPumpFn, "GamePump", "Quest", "00CB8220");
        Note(QuestListWalkAFn, "GamePump", "Quest", "00CB7C40");
        Note(QuestListWalkBFn, "GamePump", "Quest", "00CB8170");
        Note(FiberUpdateFlagSetter, "GamePump", "Quest",
            "00CB78D0 setter 0 E8 not in 012C3000/012F72D0/0129B938");
        QuestPumpWalked = 0;
        QuestVtbl24Calls = 0;
        foreach (var name in _activatedQuests)
        {
            Note(QuestFiberAttachFn, "GamePump", "Quest",
                $"00CB7950 +{QuestFiberUpdateFlagOffset}=0 {name}");
            QuestPumpWalked++;
        }

        Runtime?.Update(1f / 30f);
        WriteHeroFromRuntime();
        QuestPumpRan = true;
    }

    /// <summary>
    /// Same Thing scripts, movement, and
    /// PALSKIN use. Not <c>CREATURE_HERO_CHILD</c>.
    /// </summary>
    private void BindRuntimeHero()
    {
        if (Runtime is null || Hero is null)
            return;
        Runtime.BindScene(_regionThings, null);
        Runtime.Bindings.BindHero(Hero);
        if (Hero.PositionX is not null)
        {
            var pos = RegionTravel.PositionOf(Hero);
            Runtime.World.Positions[ScriptBindings.HeroAlias] = pos;
            Runtime.World.Positions[HeroScriptName] = pos;
        }
    }

    private void WriteHeroFromRuntime()
    {
        if (Runtime is null || Hero is null)
            return;
        if (!Runtime.World.Positions.TryGetValue(HeroScriptName, out var pos) &&
            !Runtime.World.Positions.TryGetValue(ScriptBindings.HeroAlias, out pos))
            return;
        Hero.PositionX = pos.X;
        Hero.PositionY = pos.Y;
        Hero.PositionZ = pos.Z;
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
        var saved = CurrentRegionIndex;
        var count = (World?.Regions.Count ?? 0) + RegionTableDummyCount;
        Note(LoadFromFirstRealRegionFn, "LevelLoader", "Region",
            $"00501450 count={count} saved={saved}");
        Note(PlayerCreatureBindFn, "LevelLoader", "Player",
            "00449970 / 00487DC0");
        Note(UnloadCurrentRegionFn, "LevelLoader", "Region",
            $"004FEEC0({saved},0) +156=0");
        if (count <= 1)
            return;
        Note(LoadRegionFn, "LevelLoader", "Region",
            "00500540(1,0,0) first +36 null continues");
        RequestLoadRegion(1, sync: true);
        Note(LoadFromFirstRealRegionFn, "LevelLoader", "Region",
            $"00500540({saved},0,1) restore PARTIAL");
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
            $"006C27A0 maps={region?.ContainsMaps.Count ?? 0} +{LoadJobIndexOffset}={index}");
        Note(BuildLoadJobCopyMapsFn, "LevelLoader", "Region",
            $"006C2D40 stride={LoadJobRecordSize}");
        Note(BuildLoadJobCopyTreeFn, "LevelLoader", "Region", "006B9E00");
        _loadQueue.Add(index);
        Note(EnqueueLoadJobFn, "LevelLoader", "Region",
            $"006C2120 queue={_loadQueue.Count}");
        if (sync)
            PumpLevelLoader();
    }

    /// <summary>
    /// <c>00500540</c> sync: while
    /// <c>006C20A0</c>, <c>[loader].vtbl+4</c>
    /// <c>006C2710</c> applies one job.
    /// </summary>
    public void PumpLevelLoader()
    {
        while (_loadQueue.Count > 0)
        {
            Note(LevelLoaderHasWork, "LevelLoader", "Region", "006C20A0 nonempty");
            Note(LevelLoaderUpdate, "LevelLoader", "Region", "006C2710 Level loader update");
            ApplyLoadJob(_loadQueue[0]);
            _loadQueue.RemoveAt(0);
            Note(LevelLoaderUpdate, "LevelLoader", "Region", "Level loader update end");
            Note(LevelLoaderPopFn, "LevelLoader", "Region", "006C2BA0");
        }
    }

    /// <summary>
    /// <c>004FC8A0</c> writes
    /// <c>WorldMap+156</c>,
    /// <c>00437CE0([0x13B8790])</c>,
    /// <c>0082BA00</c>. Not
    /// <c>005064C0</c> / <c>00B428E0</c>.
    /// </summary>
    public void SetRegionAsLoaded(int index)
    {
        CurrentRegionIndex = index;
        Note(SetRegionAsLoadedFn, "LevelLoader", "Region",
            "SetRegionAsLoaded: Initialise MiniMap");
        ActivateCurrentRegion();
        BindAuthoredEnvironmentTheme();
        var name = CurrentRegion?.RegionName ?? (index == 0 ? "dummy" : "?");
        Note(SetRegionAsLoadedFn, "LevelLoader", "Region",
            $"+156={index} {name}");
        Note(MiniMapFromUiFn, "LevelLoader", "Region",
            $"00437CE0 [0x13B8790]+{MiniMapUiOffset}+40");
        Note(InitMiniMapFn, "LevelLoader", "Region", "0082BA00");
        Note(SetRegionAsLoadedFn, "LevelLoader", "Region",
            "SetRegionAsLoaded: Initialise MiniMap End");
    }

    /// <summary>
    /// <c>00B40000</c>: if <c>[+424]</c> set,
    /// <c>00B3EF40</c> each list slot from
    /// index 1, then water <c>00B6DB80</c>.
    /// </summary>
    public void CloseStaticMapFile(string? keepMap = null)
    {
        Note(CloseStaticMapFileFn, "StaticMap", "WLD",
            "00B40000 CloseStaticMapFile");
        if (_openedBodies.Count == 0 && OpenStaticMapsMode == 0)
            return;
        for (var i = 0; i < _openedBodies.Count; i++)
        {
            var name = _openedBodies[i].Name;
            if (i >= 1)
                Note(CloseStaticMapFn, "StaticMap", "WLD",
                    "00B3EF40 " + name);
            var keep = keepMap is not null &&
                name.Equals(keepMap, StringComparison.OrdinalIgnoreCase);
            if (!keep)
                _levels?.UnloadMap(name);
        }

        _openedBodies.Clear();
        _neighbourStaticMaps.Clear();
        CurrentStaticMapName = null;
        CurrentCompiledLev = null;
        CurrentHeightField = null;
        OpenStaticMapsMode = 0;
    }

    /// <summary>
    /// Engine-owned submit: opened static maps
    /// plus <c>006C2170</c> ContainsMap things.
    /// Not a second <c>graphics.big</c> dump.
    /// </summary>
    public WorldGeometry? PresentWorld()
    {
        if (Install is null || CurrentRegion is null)
            return null;
        var primary = FirstSceneMapName
                      ?? CurrentRegion.ContainsMaps.FirstOrDefault()
                      ?? CurrentRegion.RegionName;
        var things = ThingsForMap(primary);
        if (things.Count == 0)
            things = _regionThings;
        var planes = SubmitSidePlanes();

        var byMap = new Dictionary<string, IReadOnlyList<ThingInstance>>(
            StringComparer.OrdinalIgnoreCase);
        foreach (var (name, list) in _thingsByMap)
            byMap[name] = list;
        EnsureLevels();
        OpenMeshBank();
        return WorldGeometry.Build(
            Install, primary, things,
            adjacentStaticMaps: false,
            landscapePlanes: planes,
            levels: _levels,
            onlyMaps: OpenedStaticMaps.Count > 0 ? OpenedStaticMaps : null,
            thingsByMap: byMap,
            meshes: Meshes,
            expandGeometry: false);
    }

    public LevelLibrary? Levels => _levels;

    /// <summary>
    /// Draw-time tessellate / C3D parse of the
    /// current region only. Neighbour maps stay
    /// <c>00B3EFA0</c> headers. Not open.
    /// </summary>
    public WorldGeometry? ExpandPresentedWorld(WorldGeometry? opened)
    {
        if (opened is null || Install is null)
            return opened;
        EnsureLevels();
        OpenMeshBank();
        return opened.Expand(Install, _levels!, Meshes, primaryOnly: true);
    }

    /// <summary>
    /// <c>0049E620</c> Opening Mesh Bank
    /// <c>MBANK_ALLMESHES</c>. Directory only.
    /// </summary>
    public void OpenMeshBank()
    {
        if (Meshes.Opened || Install is null)
            return;
        Note(InitMeshBankFn, "Init Mesh Bank", "Bank",
            "0049E620 Opening Mesh Bank " + MeshBank.BankName);
        Note(MeshBankLookupFn, "Init Mesh Bank", "Bank", "00A09F20");
        Note(MeshBankObjectCtor, "Init Mesh Bank", "Bank",
            $"00A27030 size 0x{MeshBank.ObjectSize:X} vtbl 0129CE94");
        Note(MeshBank.OpenVtbl4, "Init Mesh Bank", "Bank",
            "009D56C0 vtbl+4 Open Bank File Async");
        Note(MeshBank.OpenBankFileAsync, "Init Mesh Bank", "Bank",
            "009A7F80 [0x13CA79C]");
        Timing.Measure("mesh bank", () => { Meshes.Open(Install); return Meshes.EntryCount; },
            n => $"entries={n} parsed={Meshes.ParsedCount}");
        Note(MeshBankSetGlobalFn, "Init Mesh Bank", "Bank",
            $"004BBFD0 [0x13B8A04] entries={Meshes.EntryCount}");
    }

    /// <summary>
    /// Init Graphics <c>00416C8A</c>:
    /// <c>GBANK_MAIN_PC</c> directory.
    /// Decode is per submitted id
    /// (<c>009BE8B0</c>), not
    /// <c>window.Load</c>.
    /// </summary>
    public void OpenTextureBank()
    {
        if (Textures is not null || Install is null)
            return;
        Note(0x00416C8A, "Init Graphics", "Bank",
            "Opening Main Graphic Bank GBANK_MAIN_PC");
        Note(Fable.Formats.Textures.TextureFile.CreateTextureDxt1Named,
            "Init Graphics", "Bank", "009BE830");
        Textures = Timing.Measure("textures.big", () => new TextureLibrary(Install),
            _ => "GBANK_MAIN_PC");
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
        string? keep = null;
        if (CurrentRegion is not null)
        {
            keep = CurrentRegion.ContainsMaps.FirstOrDefault(m =>
                       m.Equals(CurrentRegion.RegionName, StringComparison.OrdinalIgnoreCase))
                   ?? CurrentRegion.ContainsMaps.FirstOrDefault()
                   ?? CurrentRegion.RegionName;
        }

        CloseStaticMapFile(keep);
        Note(SetStaticMapFileForUseFn, "StaticMap", "WLD",
            "SetStaticMapFileForUse: EnablePoolAllocation");
        Note(SetStaticMapFileForUseFn, "StaticMap", "WLD",
            "SetStaticMapFileForUse: OpenStaticMaps");
        OpenStaticMapsMode = OpenStaticMapsUseMode;
        Note(OpenStaticMapsFn, "StaticMap", "WLD",
            $"00B42750 mode={OpenStaticMapsMode} [+424]");
        _openedStaticMaps.Clear();
        if (Install is null || World is null || CurrentRegion is null)
            return;

        var primary = CurrentRegion.ContainsMaps.FirstOrDefault(m =>
                          m.Equals(CurrentRegion.RegionName, StringComparison.OrdinalIgnoreCase))
                      ?? CurrentRegion.ContainsMaps.FirstOrDefault()
                      ?? CurrentRegion.RegionName;
        Timing.Measure("STB/LEV open", () =>
        {
            foreach (var map in WorldGeometry.StaticMapsAround(World, Install, primary))
                _openedStaticMaps.Add(map.ScriptName);
            CurrentStaticMapName = primary;
            foreach (var name in _openedStaticMaps)
            {
                var neighbour = !name.Equals(primary, StringComparison.OrdinalIgnoreCase);
                if (neighbour)
                    _neighbourStaticMaps.Add(name);
                AttachStaticMap(name, neighbour);
            }
            return _openedStaticMaps.Count;
        }, n => $"opened={n} primary={primary}");

        Note(OpenStaticMapsFn, "StaticMap", "WLD",
            $"opened={_openedStaticMaps.Count} primary={primary}");
        Note(OpenStaticMapsMode1Current, "StaticMap", "WLD",
            "00B3E820 current " + primary);
        Note(OpenStaticMapsNameTable, "StaticMap", "WLD",
            "00B420F0 name table");
    }

    /// <summary>
    /// Mode-1 STB hit: <c>00B41E50</c>
    /// (close, header, <c>00BE03A0</c> /
    /// <c>00BDD0E0</c>, neighbour
    /// <c>00BDF010</c>). <c>00B42530</c>
    /// is the STB-miss fallback only.
    /// </summary>
    public void AttachStaticMap(string name, bool neighbour)
    {
        Note(OpenStaticMapsAttach, "StaticMap", "WLD",
            neighbour ? "00B41E50 neighbour " + name : "00B41E50 current " + name);
        Note(CloseStaticMapFn, "StaticMap", "WLD", "00B3EF40");
        OpenStaticMapBody(name, neighbour);
        if (neighbour)
            Note(AttachPatchFn, "StaticMap", "WLD", "00BDF010 " + name);
    }

    /// <summary>
    /// <c>00B42530</c> miss-path open.
    /// STB-hit New Game uses
    /// <see cref="AttachStaticMap"/>.
    /// </summary>
    public void OpenStaticMap(string name)
    {
        Note(OpenStaticMapFn, "StaticMap", "WLD", "00B42530 " + name);
        Note(CloseStaticMapFn, "StaticMap", "WLD", "00B3EF40");
        OpenStaticMapBody(name, neighbour: false);
    }

    private void OpenStaticMapBody(string name, bool neighbour)
    {
        if (Install is null || World is null)
            return;

        EnsureLevels();
        var header = _levels?.PeekMapHeader(name);
        Note(ParseMapHeaderFn, "StaticMap", "WLD",
            header is null || header.Value.StbSize == 0
                ? "009CCDC0 miss " + name
                : $"009CCDC0 stb samples={header.Value.HeightSamples} {name}");

        var version = header?.Version ?? 0;
        var constant = header?.Constant ?? 0;
        Note(ParseMapHeaderFn, "StaticMap", "WLD",
            $"00B3EFA0 version={version} constant=0x{constant:X}");

        if (OpenStaticMapsMode == OpenStaticMapsUseMode)
        {
            Note(CreateBackgroundPatchFn, "StaticMap", "WLD", "00BE03A0");
            Note(BuildCurrentPatchFn, "StaticMap", "WLD", "00BDD0E0");
        }

        _openedBodies.RemoveAll(b =>
            b.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        var body = new OpenedStaticMapBody(
            name,
            header?.StbSize ?? 0,
            header?.CompiledSize ?? 0,
            header?.GridWidth ?? 0,
            header?.GridHeight ?? 0,
            header?.HeightSamples ?? 0,
            version,
            constant,
            neighbour);
        _openedBodies.Add(body);
        if (!neighbour)
        {
            CurrentCompiledLev = null;
            CurrentHeightField = null;
        }

        Note(ParseMapHeaderFn, "StaticMap", "WLD",
            $"body {name} lev={body.CompiledSize} stb={body.StbSize} " +
            $"{body.GridWidth}x{body.GridHeight} neighbour={neighbour}");
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
            Timing.Measure("region TNG", () =>
            {
                EnsureLevels();
                foreach (var map in region.ContainsMaps)
                {
                    Note(LevelLoaderApply, "LevelLoader", "Region", "Loading topology " + map);
                    Note(LoadTopologyFn, "LevelLoader", "Region",
                        "004FF080 vtbl+24 " + map);
                    Note(LoadTopologyHelperFn, "LevelLoader", "Region", "00638310");
                    Note(PostLoadTopologyFn, "LevelLoader", "Region",
                        "Post load topology 004FF440");
                    if (!_activatedMaps.Exists(m =>
                            m.Equals(map, StringComparison.OrdinalIgnoreCase)))
                        _activatedMaps.Add(map);
                }

                foreach (var map in region.ContainsMaps)
                {
                    Note(LevelLoaderApply, "LevelLoader", "Region", "Loading objects " + map);
                    LoadRegionMapThings(map);
                }

                Note(ThingManagerActivateAfterFn, "LevelLoader", "Thing", "0051E2F0");
                Note(JobNavPassFn, "LevelLoader", "Region",
                    $"00500230 +{LoadJobNavOffset}=0 skip");
                Note(JobNavCommitFn, "LevelLoader", "Region",
                    $"0050AF10 +{LoadJobNavOffset}=0 skip");
                foreach (var map in region.ContainsMaps)
                    Note(PostLoadInitialiseFn, "LevelLoader", "Region",
                        "Region Level Files: Post Load Initialise 004FD020 " + map);

                foreach (var map in region.ContainsMaps)
                {
                    Note(LevelLoaderApply, "LevelLoader", "Region",
                        "Region Level Files: Activate Topology " + map);
                    Note(ActivateTopologyFn, "LevelLoader", "Region",
                        $"004FCBB0 {map} +38=1");
                    Note(SetMapLoadingFlagFn, "LevelLoader", "Region",
                        $"004FCFE0 {map} +39");
                }

                return RegionThingMapsLoaded;
            }, n => $"maps={n} things={_regionThings.Count}");

            if (!HeroSpawned)
                SpawnHeroFromPlayerStart(_regionThings);
        }

        if (index > 0)
        {
            Note(PostRegionLoadVillages, "LevelLoader", "Region",
                $"005064C0 vtbl+{WorldMapSetLoadedVtbl} before 004FC8A0");
            SetRegionAsLoaded(index);
            Note(QuestRegionNotifyFn, "LevelLoader", "Quest",
                "004AFC00 [0x13B89FC] record+24");
            OpenStaticMapsForCurrentRegion();
        }
        else
            SetRegionAsLoaded(index);
    }

    /// <summary>
    /// <c>00522720</c> then <c>00521AE0</c>
    /// Thing Manager: Load From File.
    /// </summary>
    private void LoadRegionMapThings(string mapName)
    {
        Note(LoadThingsForMapFn, "LevelLoader", "Thing", "00522720 " + mapName);
        Note(ThingManagerLoadFileFn, "LevelLoader", "Thing",
            "00521AE0 Thing Manager: Load From File");
        if (World is null)
            return;
        var map = World.Maps.FirstOrDefault(m =>
            m.ScriptName.Equals(mapName, StringComparison.OrdinalIgnoreCase) ||
            m.FileStem.Equals(mapName, StringComparison.OrdinalIgnoreCase));
        if (map is null)
        {
            Note(ThingManagerLoadFileFn, "LevelLoader", "Thing", "no map " + mapName);
            return;
        }

        EnsureLevels();
        var tng = _levels?.TryLoadThings(map.ScriptName)
                  ?? _levels?.TryLoadThings(map.FileStem);
        if (tng is null)
        {
            Note(ThingManagerLoadFileFn, "LevelLoader", "Thing", "missing " + mapName);
            return;
        }

        var loaded = tng.Things.ToList();
        _regionThings.AddRange(loaded);
        _thingsByMap[mapName] = loaded;
        RegionThingMapsLoaded++;
        Note(ThingManagerLoadFileFn, "LevelLoader", "Thing",
            $"things={loaded.Count} {mapName}");
        Note(NewThingParseFn, "LevelLoader", "Thing",
            "00520D00 NewThing Loading entities from script");
        foreach (var thing in loaded)
            LoadSingleThing(thing);
        ActivateAfterLoading();
    }

    /// <summary>
    /// <c>0051FD80</c> Load Single Thing.
    /// <c>PlayerCreature</c> + <c>[world+258]</c>
    /// binds <c>00449970</c> / <c>00487DC0</c>
    /// (<c>player+44</c> → <c>00A01B50</c>).
    /// Else <c>00A371C0</c> Allocate Class
    /// (factory <c>0052B880</c> /
    /// <c>0052AC90</c>), Construct, Initial
    /// Activate vtbl+32 or +36/+40.
    /// </summary>
    private void LoadSingleThing(ThingInstance thing)
    {
        Note(LoadSingleThingFn, "LevelLoader", "Thing",
            "Load Single Thing 1");
        Note(LoadSingleThingFn, "LevelLoader", "Thing",
            "Load Single Thing 2 " + (thing.DefinitionType ?? "NULL"));
        if (string.Equals(thing.DefinitionType, PlayerCreatureName,
                StringComparison.Ordinal))
        {
            BindPlayerCreature(thing);
        }
        else
        {
            Note(AllocateClassFn, "LevelLoader", "Thing",
                "Load Single Thing: Allocate Class");
            if (string.Equals(thing.DefinitionType, RegionTravel.PlayerStartType,
                    StringComparison.Ordinal))
                Note(HolySiteFactoryFn, "LevelLoader", "Thing",
                    "0052AC90 HOLY_SITE " + (thing.ScriptName ?? ""));
            Note(LoadSingleThingFn, "LevelLoader", "Thing",
                "Load Single Thing: Construct Thing");
            InsertThing(thing);
            Note(LoadSingleThingFn, "LevelLoader", "Thing",
                "Load Single Thing: Initial Activate vtbl+32");
        }

        Note(LoadSingleThingFn, "LevelLoader", "Thing",
            "Load Single Thing 3");
    }

    private void BindPlayerCreature(ThingInstance thing)
    {
        Note(PlayerCreatureBindFn, "LevelLoader", "Player",
            "00449970 [0x13B86A0]+28");
        Note(PlayerSlotWalkFn, "LevelLoader", "Player",
            "004498C0 match [slot+40]");
        Note(PlayerCreatureThingFn, "LevelLoader", "Player",
            "00487DC0 +44 jmp 00A01B50");
        SpawnHero(thing, bindExisting: true);
        Note(LoadSingleThingFn, "LevelLoader", "Thing",
            "Load Single Thing: Initial Activate vtbl+36/+40");
    }

    private void ActivateAfterLoading()
    {
        Note(ActivateAfterLoadingFn, "LevelLoader", "Thing",
            "0051E5A0 Activate After Loading");
    }

    /// <summary>
    /// No-save LookoutPoint has no
    /// <c>PlayerCreature</c> NewThing. Native
    /// start marker is <c>HOLY_SITE_PLAYER_START</c>
    /// <c>GuildArrivalHSP</c>. Create is
    /// <c>006AC910</c> (CPlayer
    /// <c>00489D40</c> / factory
    /// <c>0052B880</c>), not <c>00DBDE40</c>.
    /// </summary>
    private void SpawnHeroFromPlayerStart(IReadOnlyList<ThingInstance> things)
    {
        var starts = things
            .Where(t => string.Equals(
                t.DefinitionType, RegionTravel.PlayerStartType,
                StringComparison.Ordinal))
            .ToList();
        var start = starts.FirstOrDefault(t =>
                        string.Equals(t.ScriptName, GuildArrivalHsp,
                            StringComparison.OrdinalIgnoreCase))
                    ?? starts.FirstOrDefault(t => t.PositionX is not null);
        if (start is null)
        {
            Note(PlayerCreatureCreateFn, "LevelLoader", "Player",
                "no HOLY_SITE_PLAYER_START");
            return;
        }

        Note(InitCharactersFn, "LevelLoader", "Player", "0049F180 Init Characters");
        Note(InitHeroDefFn, "LevelLoader", "Player",
            "00449D90 PLAYER_HERO then CREATURE_HERO");
        Note(CreateCharacterFn, "LevelLoader", "Player",
            "00489D40 " + (start.ScriptName ?? ""));
        foreach (var (mapName, list) in _thingsByMap)
        {
            if (!list.Contains(start))
                continue;
            FirstSceneMapName = mapName;
            break;
        }

        FirstSceneMapName ??= CurrentRegion?.RegionName;
        SpawnHero(start, bindExisting: false);
    }

    /// <summary>
    /// <c>004A1840</c> child: QST
    /// <c>004A0D90</c> AddQuest /
    /// AddTestQuest into world+184.
    /// Not <c>0049F180</c>.
    /// </summary>
    private void LoadQuestDefs()
    {
        Note(LoadQuestsSite, "Load Quests", "Quest", "00416ABA 004A1840");
        Note(QstParseFn, "Load Quests", "Quest", "004A0D90 AddQuest/AddTestQuest");
        if (Install is not null && File.Exists(Install.QuestPath))
        {
            Quests = QuestFile.Load(Install.QuestPath);
            Note(QstParseFn, "Load Quests", "Quest",
                $"quests={Quests.Quests.Count} {Path.GetFileName(Install.QuestPath)}");
        }
    }

    /// <summary>
    /// <c>00416953</c> after <c>004A1840</c>
    /// when <c>[0x13B8648]==0</c>:
    /// <c>0049F180(ecx=world, 0)</c> Init
    /// Characters / Init GUI <c>0043A380</c>
    /// / Init Quests <c>004B4260([world+172])</c>,
    /// then Activate Initial Quests
    /// (<c>+90584</c> empty vs <c>0x122D70E</c>
    /// → <c>004B4A10</c>). Not a region load
    /// and not <c>S_QNOVI</c> / <c>00DBDE40</c>.
    /// </summary>
    private void InitCharactersAndQuests()
    {
        Note(InitCharactersFn, "Init Characters", "Player",
            "0049F180 push 0 ecx=world");
        Note(PlayerCreatureBindFn, "Init Characters", "Player",
            "00449970 / 00487DC0");
        Note(InitGuiFn, "Init GUI", "UI",
            "0043A380 PLAYER_GUI_PC [0x13B8790]");
        PlayerGuiReady = true;

        var names = World?.InitialQuests ?? [];
        Note(InitQuestsFn, "Init Quests", "Quest",
            $"004B4260 [world+{WorldQuestListOffset}] count={names.Count}");
        Runtime = ScriptRuntime.Detached();
        if (Install?.FindCompiledDef("script.bin") is not null)
            Runtime.Load(ScriptBank.Load(Install), Install);

        foreach (var name in names)
        {
            if (name.Length == 0)
                continue;
            Note(ActivateQuestFn, "Init Quests", "Quest", "00CB5AD0 " + name);
            var factory = QuestFactoryTable.Find(name);
            if (factory is { } bind)
            {
                Note(QuestRegisterFn, "Init Quests", "Quest",
                    "00CD52D0 " + name +
                    (bind.ScriptName is { } script ? " → " + script : " native"));
                Note(bind.Factory, "Init Quests", "Quest",
                    $"factory 0x{bind.Factory:X} run 0x{bind.Run:X}");
                if (bind.Init == QuestFactoryTable.SunnyvaleInit)
                    Note(SunnyvalePersistFn, "Init Quests", "Quest",
                        "00CDC070 persist bind vtbl+4");
            }

            var persistent = Quests?.Quests.Any(q =>
                q.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && q.Persistent) == true;
            Runtime.ActivateQuest(name, persistent);
            _activatedQuests.Add(name);
        }

        Note(QuestManagerActivate, "Init Quests", "Quest", "004B2890");
        Note(ActivateInitialQuestsSite, "Activate Initial Quests", "Quest",
            "00416BCF +90584 empty 0122D70E → 004B4A10");
        Note(ActivateInitialQuestsFn, "Activate Initial Quests", "Quest",
            "004B4A10 [0x13B89FC] → 004B4260");
        QuestsInitDone = true;
    }

    public IReadOnlyList<ThingInstance> ThingsForMap(string mapName) =>
        _thingsByMap.TryGetValue(mapName, out var list) ? list : [];

    /// <summary>
    /// <c>006AC910</c> takes the start marker
    /// physics basis. GuildArrivalHSP is
    /// <c>+X</c> / <c>+Z</c>. Missing
    /// <c>RHSet*</c> made ObjectTransform
    /// default to <c>+Y</c>.
    /// </summary>
    private static void CopyPhysicsAxes(
        ThingInstance source, Dictionary<string, string> dest)
    {
        foreach (var key in new[]
        {
            "CTCPhysicsStandard.RHSetForwardX",
            "CTCPhysicsStandard.RHSetForwardY",
            "CTCPhysicsStandard.RHSetForwardZ",
            "CTCPhysicsStandard.RHSetUpX",
            "CTCPhysicsStandard.RHSetUpY",
            "CTCPhysicsStandard.RHSetUpZ",
            "ObjectScale",
        })
        {
            if (source.Properties.TryGetValue(key, out var value))
                dest[key] = value;
        }
    }

    private void SpawnHero(ThingInstance source, bool bindExisting)
    {
        if (HeroSpawned)
            return;
        Note(PlayerCreatureFactoryFn, "LevelLoader", "Player",
            "0052B880 PlayerCreature CREATURE");
        Note(PlayerCreatureCreateFn, "LevelLoader", "Player",
            bindExisting
                ? "006AC910 bind " + (source.ScriptName ?? PlayerCreatureName)
                : "006AC910 Create " + (source.ScriptName ?? GuildArrivalHsp));
        Note(ConstructFromParamsFn, "LevelLoader", "Player",
            "006A9DD0 ConstructFromParams");
        HeroDefinition = ResolveHeroDefinition();
        var props = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["DefinitionType"] = HeroDefinition,
            ["ScriptName"] = HeroScriptName,
        };
        CopyPhysicsAxes(source, props);
        Hero = new ThingInstance
        {
            Kind = "NewThing",
            Section = source.Section,
            DefinitionType = HeroDefinition,
            ScriptName = HeroScriptName,
            PositionX = source.PositionX,
            PositionY = source.PositionY,
            PositionZ = source.PositionZ,
            Properties = props,
        };
        var inserted = InsertThing(Hero);
        HeroMeshId = inserted.MeshId ?? 0;
        _regionThings.Add(Hero);
        if (FirstSceneMapName is { } mapName &&
            _thingsByMap.TryGetValue(mapName, out var mapThings))
            mapThings.Add(Hero);
        HeroSpawned = true;
        BindRuntimeHero();
        if (source.PositionX is not null && source.PositionY is not null)
        {
            if (!WorldCameraPresent)
                WorldCamera.Construct();
            Note(WorldCameraSeedFn, "LevelLoader", "Camera",
                "006B3FF0 +68 " + (FirstSceneMapName ?? ""));
            Note(WorldCamera.FollowSlotFn, "LevelLoader", "Camera",
                "008889C0 [this+72]");
            Note(QuestSubjectFillFn, "LevelLoader", "Camera",
                "008884D0 list helper not V0");
            SubjectFillNoted = true;
            WorldCamera.SeedHero();
            Note(WorldCamera.PoseFn, "LevelLoader", "Camera",
                "006B2CA0 +61=0 +3084=0 +412=0");
            Note(WorldCamera.NormalizeFn, "LevelLoader", "Camera",
                "00A14440 normalize");
            WorldCamera.ApplyFollowSpring();
            Note(WorldCamera.PoseFollowFn, "LevelLoader", "Camera",
                "006B3030 004978A0 LCG 00A14260");
            FollowSpringRan = WorldCamera.FollowSpringRan;
            WorldCamera.ApplyCameraTick();
            Note(WorldCamera.PoseTickFn, "LevelLoader", "Camera",
                WorldCamera.CameraTickSkipped
                    ? "006B3B80 skip +460=0 +24=-1"
                    : "006B3B80 body");
            ApplyWorldCamera(1f);
        }
    }

    private InsertedThing InsertThing(ThingInstance thing)
    {
        Note(ThingConstructFromDefFn, "LevelLoader", "Thing",
            "004CA010 " + (thing.DefinitionType ?? "NULL"));
        Note(ParentConstructFn, "LevelLoader", "Thing", "00662880");
        Note(CreatureConstructThunk, "LevelLoader", "Thing", "008388D0");
        Note(CreatureConstructFn, "LevelLoader", "Thing", "006A5950");
        Note(DefAttachFn, "LevelLoader", "Thing", "0042AF3C [thing+112]");
        var defs = EnsureDefs();
        var submit = WorldGeometry.ResolveSubmit(defs, null, thing);
        var meshId = submit.MeshIds.Count > 0 ? submit.MeshIds[0] : (int?)null;
        if (meshId is > 0)
            Note(DefLookupFn, "LevelLoader", "Thing",
                $"009AD410 mesh={meshId} {thing.DefinitionType}");
        var inserted = new InsertedThing
        {
            Thing = thing,
            MeshId = meshId,
            TypeName = submit.TypeName,
            Drawable = submit.Submitted,
        };
        _inserted.Add(inserted);
        return inserted;
    }

    /// <summary>
    /// <c>00449D90</c>: <c>009AD410("PLAYER_HERO")</c>
    /// then <c>0044BA90</c>. Miss falls back to
    /// <c>CREATURE_HERO</c> and
    /// <c>0048A070</c> InitCharacterAs.
    /// Not <c>CREATURE_HERO_CHILD</c>.
    /// </summary>
    private string ResolveHeroDefinition()
    {
        Note(DefLookupFn, "LevelLoader", "Player", "009AD410 PLAYER_HERO");
        var defs = EnsureDefs();
        if (defs?.FindEntry(PlayerHeroDefName) is not null &&
            defs.FindMeshId(PlayerHeroDefName) is > 0)
        {
            Note(InitCharacterAsFn, "LevelLoader", "Player",
                "0048A070 " + PlayerHeroDefName);
            return PlayerHeroDefName;
        }

        Note(InitHeroDefFn, "LevelLoader", "Player",
            "00449E0D CREATURE_HERO fallback");
        Note(InitCharacterAsFn, "LevelLoader", "Player",
            "0048A070 " + CreatureHeroDefName);
        return CreatureHeroDefName;
    }

    /// <summary>
    /// Name only. Do not unpack the 269-byte
    /// TOD blob onto <c>c19/c20/c18</c>.
    /// </summary>
    private void BindAuthoredEnvironmentTheme()
    {
        AuthoredEnvironmentThemeId = 0;
        AuthoredEnvironmentTheme = null;
        var def = CurrentRegion?.RegionDef;
        if (string.IsNullOrEmpty(def))
            return;
        var defs = EnsureDefs();
        if (defs is null)
            return;
        AuthoredEnvironmentThemeId = defs.FindEnvironmentThemeId(def) ?? 0;
        AuthoredEnvironmentTheme = defs.FindEnvironmentThemeName(def);
        Note(SetRegionAsLoadedFn, "LevelLoader", "Region",
            AuthoredEnvironmentTheme is null
                ? "EnvironmentTheme unread " + def
                : "EnvironmentTheme " + AuthoredEnvironmentTheme +
                  " #" + AuthoredEnvironmentThemeId);
    }

    private GameBin? EnsureDefs()
    {
        if (_defs is not null)
            return _defs;
        if (Install is null)
            return null;
        EnsureLevels();
        _defs = _levels?.Defs ?? WorldGeometry.TryLoadDefs(Install);
        return _defs;
    }

    private ThingFile? TryLoadMapTng(WorldMap map) =>
        _levels?.TryLoadThings(map.ScriptName)
        ?? _levels?.TryLoadThings(map.FileStem);

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
        CloseStaticMapFile();
        Stage = EngineStage.Shutdown;
        Mode = EngineMode.None;
    }

    public void Dispose()
    {
        UnloadStartupAvi();
        CloseStaticMapFile();
        _levels?.Dispose();
        _levels = null;
        Textures?.Dispose();
        Textures = null;
        Meshes.Dispose();
    }

    private void EnsureLevels()
    {
        if (_levels is not null || Install is null)
            return;
        _levels = new LevelLibrary(Install, World);
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

        // 009A8150 / 009AC700 / 0099EFB0 insert names only.
        // graphics.big / textures.big stay closed until
        // 0049E620 / texture open.
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
        ApplyDisplayDefaults();
        Note(DisplayWidthVa, "Setup library", "Window",
            $"00403079 [{DisplayDefaultWidth}x{DisplayDefaultHeight}]");
        Note(WindowTitleFn, "Setup library", "Window",
            "004023F0 " + WindowTitleId);
        Note(InputDeviceVa, "Setup library", "Input",
            "0042E3EE [0x13B8388]");
        CreateDeviceFlags = CreateDeviceSoftwareFlags;
        GraphicsCreated = true;
    }

    /// <summary>
    /// Queue one <c>009F4F10</c> record
    /// for the next <c>0042E3EE</c> pump.
    /// Type at <c>+40</c>, key at <c>+0</c>.
    /// </summary>
    public void QueueInput(int type, int key) => Input.Queue(type, key);

    /// <summary>
    /// <c>0042E3EE</c>: construct
    /// <c>0041E5F2</c>, poll queued
    /// events, <c>0055CB10</c> actions.
    /// </summary>
    public void PumpInput()
    {
        Input.Construct();
        Note(InputActionGetter, "Input", "Action",
            "0041E5F2 [0x13B8710]");
        Note(InputActionCtor, "Input", "Action",
            "0041E3F6 vtbl 01230134");
        Note(InputBindDefaults, "Input", "Action",
            "0041DF10 keyboard defaults");
        Input.Pump();
        Note(InputActionApply, "Input", "Action",
            $"0055CB10 n={Input.Actions.Count}");
    }

    /// <summary>
    /// <c>00403079</c> copies PE
    /// <c>[0x137545C]</c>/<c>[0x1375460]</c>
    /// then <c>009C0E50</c> clamps min 32.
    /// Title is <c>004023F0</c>
    /// <c>TEXT_GUI_WINDOW_TITLE</c>.
    /// </summary>
    private void ApplyDisplayDefaults()
    {
        var width = DisplayDefaultWidth;
        var height = DisplayDefaultHeight;
        if (width < GraphicsMinDimension)
            width = GraphicsMinDimension;
        if (height < GraphicsMinDimension)
            height = GraphicsMinDimension;
        BackBufferWidth = width;
        BackBufferHeight = height;
        BackBufferBpp = DisplayDefaultBpp;
        WindowTitle = WindowTitleDefault;
        ViewportX = 0;
        ViewportY = 0;
        ViewportWidth = width;
        ViewportHeight = height;
        ViewportZNear = ViewportMinZ;
        ViewportZFar = ViewportMaxZ;
        Note(SetViewportFn, "Setup library", "D3D9",
            $"009BEF80 SetViewport vtbl+{SetViewportVtbl} {width}x{height}");
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
    uint HeaderConstant,
    bool Neighbour = false);

/// <summary>
/// <c>004CA010</c> / <c>00662880</c> insert:
/// GameBin definition bound, mesh id from
/// <c>009AD410</c> / <c>0042AF3C</c>.
/// </summary>
public readonly record struct InsertedThing(
    ThingInstance Thing,
    int? MeshId,
    string? TypeName,
    bool Drawable);
