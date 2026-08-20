using System.Numerics;
using System.Runtime.CompilerServices;
using Fable.Core;
using Fable.Dx9;
using Fable.Formats.Banks;
using Fable.Formats.Defs;
using Fable.Formats.Fonts;
using Fable.Formats.Levels;
using Fable.Formats.Text;
using Fable.Formats.Qst;
using Fable.Formats.Scene;
using Fable.Formats.Textures;
using Fable.Formats.Tng;
using Fable.Formats.Meshes;
using Fable.Formats.Wld;
using Fable.Game.Scripting;
using Fable.Render;
using Fable.Render.Parity.Dx9Vulkan;

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
    /// <summary>
    /// Dump listing at <c>00401067</c>, not a
    /// live <c>Fable.exe</c> attach.
    /// </summary>
    public const uint CrtSehFn = 0x0040138C;
    public const uint CrtStaticCtorsFn = 0x004012CE;
    public const uint WinMainCallSite = 0x004011E7;
    public const uint GetModuleHandleIat = 0x0143FE94;
    public const uint SetAppTypeIat = 0x0144017C;
    public const int SetAppTypeGui = 2;
    public const uint WinMainAllocaFn = 0x00BFEA30;
    public const uint WinMainAllocaUnwindFn = 0x00BFE9F9;
    public const int WinMainAllocaSize = 0x32008;
    public const int WinMainScratchSize = 0x32000;
    public const uint OpenMutexIat = 0x0143FE24;
    public const uint CreateMutexIat = 0x0143FE28;
    public const uint MutexNameVa = 0x0122DC28;
    public const string MutexName = @"Global\Fable";
    public const int MutexAccess = 0x1F0001;
    public const uint WinMainZeroFn = 0x009D86B0;
    public const uint ParseCommandLineCtor = 0x00403B10;
    public const uint ParseCommandLineScan = 0x00997510;
    public const uint ParseCommandLineApply = 0x009974F0;
    public const uint SetupInstallCopyFn = 0x009D5240;
    public const uint SetupLanguageFn = 0x00415530;
    public const string LanguageFolder = "English";
    public const uint LanguagePrefixVa = 0x0122DBA8;
    public const string LanguagePrefix = @"Data\lang\";
    public const uint LanguageSettingsVa = 0x0122DBC0;
    public const string LanguageSettingsLeaf = @"\lang_settings.txt";
    public const string LanguageSettingsName = "lang_settings.txt";
    public const string LanguageSettingsRelative =
        @"Data\lang\English\lang_settings.txt";
    public const uint LanguageIniFn = 0x004045C0;
    public const string LanguageLeftAlignName = "LeftAlignText";
    public const string LanguageNoHangulName = "NoHangulWordWrap";
    public const string LanguageCapsLockName = "DisableCapsLock";
    public const uint LeftAlignTextVa = 0x013B861B;
    public const uint NoHangulWordWrapVa = 0x013B861C;
    public const uint DisableCapsLockVa = 0x013B864F;
    public const uint ApplyLeftAlignFn = 0x009BC890;
    public const uint ApplyNoHangulFn = 0x009BC8A0;
    public const uint ApplyLeftAlignDestVa = 0x013CA7EA;
    public const uint ApplyNoHangulDestVa = 0x013CA7EB;
    public const uint BankManagerInitFn = 0x009A76D0;
    /// <summary>
    /// After <c>009A6610</c>: HWND
    /// <c>[engine+148]</c> → <c>00404A80</c>
    /// then <c>00405650</c>.
    /// </summary>
    public const uint LibraryPostWindowFn = 0x00405650;
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
    /// <c>00403079</c> <c>[opt+16]=[0x137544A]</c>.
    /// <c>009A667C</c> copies that into
    /// <c>[engine+142]</c>; <c>009C0E50</c>
    /// passes it as <c>009BF7E0</c>
    /// <c>[ebx+28]</c>. <c>sete [ebp+572]</c>
    /// is D3D <c>Windowed = !flag</c>.
    /// PE default is 1 (exclusive).
    /// <c>00413C42</c> writes 0 on the
    /// command-line skip-frontend path.
    /// </summary>
    public const uint DisplayWindowFlagVa = 0x0137544A;
    public const byte DisplayWindowFlagFirstSeen = 1;
    /// <summary>
    /// <c>00403079</c> <c>mov dl,[0x1375468]</c>
    /// → <c>[opt+116]</c>. PE default 32
    /// matches <c>userst.ini</c>
    /// <c>SetZBufferDepth(32)</c>, not the
    /// windowed flag.
    /// </summary>
    public const uint DisplayZDepthVa = 0x01375468;
    public const int DisplayZDepthFirstSeen = 32;
    /// <summary>
    /// <c>009A6610</c> bit 0x04 →
    /// <c>009A64B0</c> <c>CreateWindowExW</c>
    /// style <c>WS_CAPTION|WS_SYSMENU|WS_MINIMIZEBOX</c>.
    /// Centered with <c>GetSystemMetrics</c>
    /// 0/1/4/5/6 chrome. Always a caption
    /// HWND; exclusive vs windowed is the
    /// D3D <c>[ebp+572]</c> flag, not a
    /// third-party wrapper.
    /// </summary>
    public const uint CreateWindowFn = 0x009A64B0;
    public const int CreateWindowExStyle = 0x00CA0000;
    public const int PresentParametersWindowedOffset = 572;
    public const uint GraphicsOptionsPackFn = 0x009A6A00;
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
    public const uint InitFontsFn = FontFile.InitFontsFn;
    public const uint GameFontLookupFn = FontFile.FontLookupFn;
    public const uint GameFontStoreFn = 0x00419463;
    public const int GameFontOffset = 90444;
    public const string GameFontFaceName = FontFile.GameFace;
    /// <summary>
    /// <c>004CDB10</c> joins
    /// <c>0041A080</c>
    /// <c>0x122F3D0</c>
    /// <c>Data\Defs\</c> with
    /// <c>0099BF30</c>
    /// <c>0x1239E74</c>
    /// <c>misc_def_types.h</c>
    /// then <c>00A39010</c>
    /// fills <c>[0x13B8A54]</c>.
    /// Not a spoken line.
    /// Do not invent
    /// <c>00A38E50</c> parse.
    /// </summary>
    public const uint InitSubtitledMessageFn = 0x004CDB10;
    public const uint SubtitledPrefixFn = 0x0041A080;
    public const uint SubtitledLeafFn = 0x0099BF30;
    public const uint SubtitledRegisterFn = 0x00A39010;
    public const uint SubtitledSingletonVa = 0x013B8A54;
    public const uint SubtitledPrefixVa = 0x0122F3D0;
    public const uint SubtitledLeafVa = 0x01239E74;
    public const string SubtitledDefsPrefix = @"Data\Defs\";
    public const string SubtitledDefsLeaf = "misc_def_types.h";
    public const string SubtitledDefsPath = @"Data\Defs\misc_def_types.h";
    /// <summary>
    /// <c>004CD670</c> once
    /// <c>[0x13B8A28]</c> then
    /// <c>0099EFE0</c> into
    /// <c>[0x13B8A2C]</c> /
    /// <c>[0x13B8A38]</c> /
    /// <c>[0x13B8A44]</c>.
    /// Not a spoken line.
    /// </summary>
    public const uint InitConversationAttitudeFn = 0x004CD670;
    public const uint ConversationAttitudeBindFn = 0x0099EFE0;
    public const uint ConversationAttitudeOnceVa = 0x013B8A28;
    public const uint ConversationAttitudeTable0Va = 0x013B8A2C;
    public const uint ConversationAttitudeTable1Va = 0x013B8A38;
    public const uint ConversationAttitudeTable2Va = 0x013B8A44;
    public static readonly string[] ConversationAttitudeNames0 =
    [
        "STANDARD_TALK_GENERIC",
        "STANDARD_TALK_GENERIC",
        "STANDARD_TALK_LOVE",
        "STANDARD_TALK_GENERIC",
        "STANDARD_TALK_GENERIC",
        "STANDARD_TALK_FEAR",
        "STANDARD_TALK_ANGER",
        "STANDARD_TALK_ANGER",
        "STANDARD_TALK_ANGER",
        "STANDARD_TALK_ANGER",
        "STANDARD_TALK_LOVE",
        "STANDARD_TALK_GENERIC",
        "STANDARD_TALK_ANGER",
        "STANDARD_TALK_FEAR",
        "STANDARD_TALK_ANGER",
        "STANDARD_TALK_ANGER",
        "STANDARD_TALK_LOVE",
        "STANDARD_TALK_GENERIC",
    ];
    public static readonly string[] ConversationAttitudeNames1 =
    [
        "NULL",
        "STANDARD_TALK_GENERIC",
        "STANDARD_TALK_LOVE",
        "STANDARD_TALK_ANGER",
        "STANDARD_TALK_FEAR",
        "STANDARD_TALK_GLEE",
        "STANDARD_TALK_SADNESS",
        "STANDARD_TALK_RIDICULE",
        "STANDARD_TALK_FRIENDLY",
        "STANDARD_TALK_RIDICULE",
        "STANDARD_TALK_GENERIC",
        "STANDARD_TALK_GENERIC",
    ];
    public static readonly string[] ConversationAttitudeNames2 =
    [
        "",
        "CONVERSATION_HAPPY",
        "CONVERSATION_HAPPY",
        "CONVERSATION_ANGRY",
        "CONVERSATION_FEAR",
        "CONVERSATION_HAPPY",
        "CONVERSATION_SAD",
        "CONVERSATION_BELLIGERENT",
        "CONVERSATION_HAPPY",
        "CONVERSATION_BELLIGERENT",
        "CONVERSATION_CAGED",
        "CONVERSATION_HOLDING_SWORD",
    ];
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
    /// <summary>
    /// Native Leave fades live
    /// <c>[0x13B8394]</c>
    /// <c>vtbl+72(500)</c>. Host has
    /// no player object. Do not invent
    /// <c>MUSIC_SET_*</c> to fill it.
    /// </summary>
    public const bool HostFadesLeaveFrontendAudio = false;
    public const uint FrontendAudioSingletonVa = 0x013B8394;
    public const uint GameSingletonVa = 0x013B86A0;
    public const uint RetailSuccessorVa = 0x013B7D58;
    public const int GameReadyOffset = 90592;
    public const uint SkipParticlesVa = 0x013B8648;
    public const byte SkipParticlesFirstSeen = 0;
    /// <summary>
    /// Boot loads <c>PARTICLE_MAIN</c> at
    /// <c>004174F1</c> when
    /// <c>[0x13B8648]==0</c>. First-seen
    /// running list on Lookout Present is
    /// empty. Oakvale intro authors 47
    /// <c>PARTICLE_EMITTER_PLACEABLE</c>
    /// Things; names UNREAD. Do not invent
    /// particle GPU. <c>004A67D0</c> is
    /// world ctor, not Oakvale VFX.
    /// </summary>
    public const uint LoadParticlesFn = 0x004174F1;
    public const bool FirstSeenRunningParticleListEmpty = true;
    public const int OakvalePlaceableEmitterCount = 47;
    public const bool FirstSeenCanRenderParticles = false;
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
    /// Msg 15 <c>[ui+28] vtbl+16</c>
    /// is <c>00430340</c>:
    /// <c>[retail+8]=1</c> then
    /// <c>[retail+41]=1</c>. Host
    /// Notes the store. Pump uses
    /// <c>RetailNewGameFlag</c> as
    /// both loop-exit and New Game.
    /// </summary>
    public const uint RetailPlus8StoreFn = 0x00430340;
    public const int RetailPlus8Offset = 8;
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
    /// <summary>
    /// With <see cref="Device"/> attached,
    /// <c>IssueFrontendFramePresent</c> issues
    /// Clear / recovered DIPUP / Present.
    /// <c>009DA9F0(1)</c> twice is still
    /// Note-only empty skip.
    /// </summary>
    public const bool FrontendPresentBodyIsLive = true;
    public const bool DisplayFlushQueueIsNoteOnly = true;
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
    /// First-seen populate is
    /// <c>0042E98F</c> → <c>005958F5</c>
    /// → <c>00598A1C</c>, not
    /// <c>0059899A</c>. <c>0041DB1D</c>
    /// / <c>009AD410</c> / <c>0041D21B</c>
    /// type 0 <c>0041B800</c> vtbl
    /// <c>0122F5D4</c>; draw slot +8 is
    /// <c>0041AFA0</c>. Not UI singleton
    /// <c>012521A8+8</c> <c>0052D900</c>.
    /// </summary>
    public const uint FrontendProfileBindFn = 0x005958F5;
    public const uint FrontendPressStartAttachFn = 0x00598A1C;
    /// <summary>
    /// <c>0042F015</c>
    /// <c>005952C3</c>
    /// <c>vtbl+192</c>(5) on
    /// <c>[ui+32].back()</c>
    /// after Press Start bind.
    /// </summary>
    public const uint FrontendInitSelectFn = 0x005952C3;
    public const uint FrontendMainMenuFn = 0x0059899A;
    public const uint FrontendMenuAttachFn = 0x00595A06;
    public const uint FrontendWidgetFactoryFn = 0x0041DB1D;
    public const uint FrontendWidgetConstructFn = 0x0041D21B;
    public const uint FrontendWidgetType0Ctor = 0x0041B800;
    public const uint FrontendWidgetVtbl = 0x0122F5D4;
    public const uint FrontendWidgetDrawFn = 0x0041AFA0;
    /// <summary>
    /// frontend.bin <c>Type</c> of
    /// <c>UI_FRONTEND_PRESS_START_MENU</c>
    /// is 10, not 0. Ctor
    /// <c>0054E3D0</c> → <c>0052CC50</c>
    /// vtbl <c>012497E4</c>. Draw slot +8
    /// is <c>00530260</c> (walk +176),
    /// not <c>0041AFA0</c>.
    /// </summary>
    public const int FrontendPressStartType = 10;
    public const uint FrontendPressStartCtorFn = 0x0054E3D0;
    public const uint FrontendPressStartVtbl = 0x012497E4;
    public const uint FrontendContainerDrawFn = 0x00530260;
    public const int FrontendChildListOffset = 176;
    /// <summary>
    /// <c>0052C730</c> → <c>005339B0</c>
    /// writes <c>+272/+276=1.0</c> when
    /// <c>+280==0</c>, copies +36 layout
    /// into <c>+52/+92</c>. Type 10
    /// vtbl+172 is <c>0054E4B0</c> which
    /// starts with this call; then
    /// <c>005339B0</c> walks +176
    /// <c>vtbl+172</c> on children.
    /// </summary>
    public const uint FrontendScaleInitFn = 0x0052C730;
    public const uint FrontendScaleWriteFn = 0x005339B0;
    public const uint FrontendScaleInitVtblFn = 0x0054E4B0;
    public const int FrontendScaleInitVtbl = 172;
    public const float FrontendScaleOne = 1f;
    public const string FrontendPressStartText = "UI_PRESS_START_TEXT";
    public const string FrontendPressStartTextValue = "TEXT_GUI_MENU_PRESS_BUTTON";
    /// <summary>
    /// Type-6 ctor <c>0054F5C0</c> →
    /// <c>0054ED90</c> looks up a face
    /// via <c>009E2C80</c>. Nearby
    /// helper <c>0054F4B0</c> names
    /// <c>ENG_ARIAL_16</c>.
    /// </summary>
    public const uint FrontendTextCtorFn = 0x0054F5C0;
    public const uint FrontendTextDrawFn = 0x0054EF00;
    public const uint FrontendTextVtbl = 0x01249CCC;
    public const uint FrontendUiFontFn = 0x0054F4B0;
    public const string FrontendUiFontFace = FontFile.UiFace;
    public const string FrontendTitleWidget = "UI_TITLE";
    public const string FrontendForestBackground = "UI_BLENDING_BACKGROUNDS_FORREST";
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
    /// <summary>
    /// <c>00599E3F</c> walks
    /// <c>[ui+84]</c> and
    /// <c>[node+20].vtbl+4</c>
    /// <c>0052C7E0</c> → <c>00531EC0</c>
    /// which calls vtbl+148
    /// <c>0052F5C0</c> (+264) then
    /// vtbl+136 <c>0052FFD0</c> (+248)
    /// before <c>0042DF9E</c> draw.
    /// First-seen fields are ctor 0 so
    /// dest stays 0,0,0,0.
    /// </summary>
    public const uint FrontendWidgetTickFn = 0x0052C7E0;
    public const int FrontendWidgetTickVtbl = 4;
    public const uint FrontendDestLayoutFn = 0x00531EC0;
    public const uint FrontendDestScaleFn = 0x0052F5C0;
    public const int FrontendDestScaleVtbl = 148;
    public const uint FrontendDestOriginFn = 0x0052FFD0;
    public const int FrontendDestOriginVtbl = 136;
    public const uint FrontendSpriteInstanceSubmitFn = 0x00BAD8A0;
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
    /// <c>[0x1436E84]+16</c>. First
    /// <c>00B324A0</c> constructs
    /// <c>00BACFD0</c>/<c>00BAE2D0</c>.
    /// Later frames <c>dest+4</c> set
    /// call instance <c>00BAD8A0</c>.
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
    /// <summary>
    /// <c>012A0F3C+92</c> is
    /// <c>00B23BC0</c>. <c>00B8FAA0</c>
    /// zeros the type table;
    /// <c>00B8FAD0</c> only registrant
    /// is <c>00B482A0</c> types
    /// <c>0xF</c>/<c>0x10</c>.
    /// Type <c>0x22</c> slot stays 0.
    /// </summary>
    public const uint FrontendTypeTableCtorFn = 0x00B8FAA0;
    public const uint FrontendTypeTableRegisterFn = 0x00B8FAD0;
    public const uint FrontendTypeListFn = 0x00B48220;
    public const uint FrontendTypeFactoryFn = 0x00B44EB0;
    public const uint FrontendTypeSubmitStubFn = 0x00B4A450;
    public const int FrontendTypeListA = 0xF;
    public const int FrontendTypeListB = 0x10;
    /// <summary>
    /// <c>0042E204</c> <c>00B26340</c>
    /// constructs <c>00B4AC10</c> then
    /// <c>00BAD040</c> ("VSHADER_2D_SPRITE").
    /// <c>00B4ABB0</c> → <c>00B8FAD0</c>
    /// registers types <c>0x22</c>/<c>0x23</c>
    /// before the first <c>0042DF9E</c>.
    /// </summary>
    public const uint FrontendSpriteLayerCtorFn = 0x00B4AC10;
    public const uint FrontendSpriteHandlerCtorFn = 0x00BAD040;
    public const uint FrontendSpriteTypeListFn = 0x00BB1640;
    public const uint FrontendSpriteFactoryFn = 0x00BACFD0;
    public const uint FrontendSpriteSubmitFn = 0x00BAE2D0;
    public const uint FrontendSpriteHandlerVtbl = 0x012A5664;
    public const uint FrontendSpriteInstanceVtbl = 0x012A54BC;
    public const int FrontendSpriteType = 0x22;
    public const int FrontendSpriteTypeAlt = 0x23;
    public const string FrontendSpriteShader = "VSHADER_2D_SPRITE";
    public const uint FrontendWidgetMessageNoopFn = 0x0052F040;
    public const uint FrontendDefResolveFn = 0x0042AEDA;
    public const uint FrontendDefLookupFallbackFn = 0x009E5170;
    public const uint FrontendConstructSwitchVa = 0x0041D7F8;
    public const string FrontendBinFile = "frontend.bin";
    public const int FrontendWidgetReadyOffset = 368;
    public const int FrontendWidgetBlendOffset = 372;
    public const int FrontendWidgetFontOffset = 376;
    public const int FrontendWidgetTextureOffset = 380;
    public const int FrontendWidgetSubmitDestOffset = 0x15C;
    public const int FrontendWidgetBlendDefault = 2;
    public const int FrontendWidgetDefTypeOffset = 60;
    public const string FrontendPressStartMenu =
        "UI_FRONTEND_PRESS_START_MENU";
    public const int FrontendPressStartSlot = 0x14;
    /// <summary>
    /// <c>00595A06</c> writes Main
    /// Menu into existing key
    /// <c>0</c>. First-seen attach
    /// left that cell null.
    /// </summary>
    public const int FrontendMainMenuSlot = 0;
    public const int FrontendPressStartMessage = 0xE5;
    public const int FrontendWidgetMessageVtbl = 284;
    /// <summary>
    /// <c>0059B5D7</c>
    /// <c>[ui+84]</c> map
    /// <c>operator[]</c>. Key
    /// <c>0x14</c> is Press Start.
    /// Value is node+20 widget*.
    /// </summary>
    public const uint FrontendSlotLookupFn = FrontendInputMap.SlotLookupFn;
    /// <summary>
    /// <c>0059A238</c> <c>sub ecx, 0xE5</c>
    /// → <c>00599D5C</c>. Attach-time
    /// <c>0xE5</c> is widget vtbl+284
    /// only; this is the UI message.
    /// </summary>
    public const uint FrontendPressStartAcceptFn = 0x00599D5C;
    public const uint FrontendProfileEnumFn = 0x005955AB;
    /// <summary>
    /// Empty <c>005955AB</c> from
    /// <c>00599D5C</c>. Not msg
    /// <c>0x125</c> first-seen.
    /// </summary>
    public const uint FrontendNoProfileFn = 0x00595845;
    public const int FrontendUiArmedOffset = 160;
    public const int FrontendUi100Offset = 100;
    public const int FrontendUi96Offset = 96;
    /// <summary>
    /// Next <c>00599E3F</c> after
    /// <c>[ui+160]=1</c>. Slot
    /// <c>0x17</c> is bound in
    /// <c>00598A1C</c>.
    /// </summary>
    public const uint FrontendNewProfileBindFn = 0x00596917;
    public const uint FrontendMenuSwitchFn = 0x00596763;
    public const uint FrontendUi96CtorFn = 0x00851700;
    public const uint FrontendUi96EditBoxFn = 0x00851770;
    public const int FrontendNewProfileSlot = 0x17;
    public const string FrontendNewProfileMenu =
        "UI_FRONTEND_NEW_PROFILE_SCREEN";
    public const string FrontendNewProfileEditBox =
        "UI_NEW_PROFILE_EDIT_BOX";
    public const string FrontendAcceptNewProfile =
        "UI_ACCEPT_NEW_PROFILE";
    /// <summary>
    /// <c>00851770</c> <c>cmp eax, 37</c>
    /// after vtbl+260. frontend.bin
    /// <c>UI_NEW_PROFILE_EDIT_BOX</c>
    /// type byte is 37.
    /// </summary>
    public const int FrontendNewProfileEditType = 37;
    public const int FrontendEditBoxActionA = 33;
    public const int FrontendEditBoxActionB = 34;
    /// <summary>
    /// <c>0053FE50</c> action 34 insert
    /// into type-37 <c>+356</c>. Arg 8
    /// backspace, 32 space, else glyph.
    /// Cap persist/default <c>0x32</c>.
    /// Not DIK.
    /// </summary>
    public const uint EditBoxInsertFn = 0x0053FE50;
    public const int EditBoxTextOffset = 356;
    public const int EditBoxLenOffset = 360;
    public const int EditBoxCursorOffset = 364;
    public const int EditBoxCap = 0x32;
    public const bool EditBoxTypesFromDik = false;
    /// <summary>
    /// <c>0059A238</c> msg <c>0x126</c>
    /// → <c>00851920</c>. frontend.bin
    /// <c>UI_ACCEPT_NEW_PROFILE</c>
    /// stores 0x126.
    /// </summary>
    public const int FrontendAcceptProfileMessage = 0x126;
    public const uint FrontendCommitNameFn = 0x00851920;
    public const uint FrontendProfileDefaultFn = 0x004069E0;
    public const string FrontendProfileDefaultText =
        "TEXT_GUI_PROFILE_DEFAULT";
    public const uint FrontendProfileDefaultFallbackVa = 0x0122DE80;
    public const string FrontendProfileDefaultFallback = "Default";
    /// <summary>
    /// <c>00599E3F</c> when
    /// <c>[ui+96+5]≠0</c> and
    /// <c>+4==0</c>: <c>0059697A</c>.
    /// </summary>
    public const uint FrontendCommitProfileFn = 0x0059697A;
    public const uint FrontendCanCreateProfileFn = 0x004067C0;
    /// <summary>
    /// <c>0059A238</c> msg <c>0x124</c>
    /// and one-name <c>00599D5C</c>.
    /// </summary>
    public const int FrontendMainMenuMessage = 0x124;
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
    /// Native <c>[0x13B8394].vtbl+68</c>
    /// starts frontend voice. Host Notes
    /// the site only. Do not start
    /// <c>MUSIC_SET_*</c> from New Game.
    /// </summary>
    public const uint RetailAudioFadeFn = 0x0042DED5;
    public const uint RetailAudioEngineVa = 0x013B8394;
    public const int RetailAudioStartVtbl = 68;
    public const int RetailAudioFadeVtbl = 72;
    public const uint InitSoundFn = 0x00417A58;
    /// <summary>
    /// First-seen <c>00417A58</c> after Leave
    /// (<c>[0x13B8394]</c> live): locale
    /// <c>00415550</c> <c>ENGLISH_SOUND_SETUP</c>,
    /// def lookup <c>004196B2</c>
    /// <c>MAIN_SOUND_SETUP</c>, register
    /// <c>009919C0</c> twice, atmos
    /// <c>00991C10</c>, map
    /// <c>00991840(1)</c> → <c>[game+16]</c>.
    /// Register, not play, not graphic-bank
    /// Open. Symbols first-seen compiled
    /// <c>00A38C20</c>, not <c>00A01A4F</c>.
    /// Do not start <c>MUSIC_SET_*</c> /
    /// <c>SND_MENU_04</c>.
    /// </summary>
    public const uint InitSoundLocaleFn = 0x00415550;
    public const uint InitSoundLookupFn = 0x004196B2;
    public const uint InitSoundRegisterFn = 0x009919C0;
    public const uint InitSoundAtmosRegisterFn = 0x00991C10;
    public const uint InitSoundMapLookupFn = 0x00991840;
    public const uint InitSoundSymbolsCompiledFn = 0x00A38C20;
    public const uint InitSoundSymbolsTextFn = 0x00A01A4F;
    public const string InitSoundLocaleName = "ENGLISH_SOUND_SETUP";
    public const string InitSoundMainName = "MAIN_SOUND_SETUP";
    public const bool InitSoundOpensBank = false;
    public const bool InitSoundPlaysMusicSet = false;
    public const bool FrontendClickStartsSnd = false;
    public const bool RequestNewGameStartsMusicSet = false;
    /// <summary>
    /// Script <c>PlayMusic</c>
    /// <c>00CC8EAC</c> looks up
    /// <c>009E5120</c> then
    /// <c>[0x143E8F8].vtbl+2784</c>.
    /// Host stores the name only.
    /// Do not invent an ogg/FMOD
    /// player. First-seen Gameflow
    /// never reaches PlayMusic.
    /// </summary>
    public const uint ScriptPlayMusicApplyFn = 0x00CC8EAC;
    public const uint ScriptPlayMusicLookupFn = 0x009E5120;
    public const int ScriptPlayMusicVtbl = 2784;
    public const bool ScriptPlayMusicAppliesBank = false;
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
    /// First-seen dump calls from PE
    /// <c>00401067</c> through
    /// <c>00412F90</c> RunModes.
    /// Host <see cref="Bootstrap"/> must
    /// Note these VAs in this order.
    /// Extra notes in between are allowed.
    /// </summary>
    public static uint[] PeEntryFirstSeenVas =>
    [
        PeEntry,
        CrtSehFn,
        GetModuleHandleIat,
        SetAppTypeIat,
        CrtStaticCtorsFn,
        WinMainCallSite,
        WinMain,
        WinMainAllocaFn,
        OpenMutexIat,
        CreateMutexIat,
        WinMainZeroFn,
        BootstrapFn,
        NamedBootstrapStages[0].Va,
        ParseCommandLineCtor,
        ParseCommandLineScan,
        ParseCommandLineApply,
        UserstRegisterFn,
        NamedBootstrapStages[1].Va,
        SetupInstallCopyFn,
        NamedBootstrapStages[2].Va,
        SetupLanguageFn,
        ApplyLeftAlignFn,
        ApplyNoHangulFn,
        NamedBootstrapStages[3].Va,
        BankManagerInitFn,
        RegisterRetailBank,
        NamedBootstrapStages[4].Va,
        EngineSingletonGetter,
        SetupLibrary,
        FrontendDisplayHelperFn,
        LibraryPostWindowFn,
        NamedBootstrapStages[5].Va,
        ProbeGraphics,
        RunModes,
    ];

    /// <summary>
    /// <c>004184BD</c> vtbl+4 after <c>00418DCA</c>.
    /// Not <c>00DBDE40</c>.
    /// </summary>
    public static readonly (string Stage, uint Apply)[] InitGameStages =
    [
        ("Init Thing Components", InitThingComponentsFn),
        ("Init Definition Manager", InitDefinitionManagerFn),
        ("Init Graphics", 0x00416C8A),
        // 004168DC sibling; name is logged inside the fn.
        ("Init Fonts", FontFile.InitFontsFn),
        ("Init Subtitled Message", InitSubtitledMessageFn),
        ("Init Conversation Attitude", InitConversationAttitudeFn),
        ("Init Player Manager", InitPlayerManagerFn),
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
    public const uint WorldVtbl = 0x012390F0;
    public const int WorldLoadWldVtbl = 8;
    public const uint WorldLoadWldFn = 0x0049E220;
    public const uint WorldAfterWldFn = 0x0049D970;
    public const int WorldLoadedFlagOffset = 128;
    public const uint LoadWldFile = 0x00507C30;
    public const uint LoadGtng = 0x0050959F;
    public const uint LoadGlobalThings = 0x00509859;
    public const uint LoadGlobalThingsSingle = 0x004FE2A0;
    public const uint LoadGlobalThingsPerMap = 0x004FDBC0;
    public const uint LoadGlobalThingsMapFile = 0x004FBF60;
    /// <summary>
    /// Native <c>004FDBC0</c> <c>ebx</c>
    /// starts at 1 (dummy slot 0 never
    /// pushed). Stop is
    /// <c>ebx &gt;= count</c> (TLC 399
    /// → 1..398). First
    /// <c>004FBF60</c> is LookoutPoint
    /// (NewMap 1).
    /// <c>StartOakValeWest.tng</c> is
    /// <c>ebx=203</c> during Loading
    /// world, not first Present.
    /// Host <c>break</c> after the first
    /// prox file is leftover #50.
    /// </summary>
    public const int LoadGlobalThingsEbxStart = 1;
    public const int StartOakValeWestTngEbx = 203;
    public const bool LoadGlobalThingsHostBreaksAfterFirstProx = true;
    public const uint SingleGlobalThingsFlagVa = 0x013B8609;
    public const byte DefaultSingleGlobalThingsFlag = 0;
    public const uint GtngExtVa = 0x01244BB4;
    public const uint GtgExtVa = 0x01244BDC;
    public const uint TngExtVa = 0x012442C4;
    public const string GtngExtension = ".gtng";
    public const string GtgExtension = ".gtg";
    public const string TngExtension = ".tng";
    public const uint PlayerManagerGetter = 0x0044C6B0;
    /// <summary>
    /// <c>0044C6B6</c> is
    /// <c>[0x13B879C]!=0</c>.
    /// First-seen 0 →
    /// <c>00BFEA1A(0xE0)</c>
    /// <c>0044C6C2</c>
    /// <c>0044C71F</c>.
    /// </summary>
    public const uint PlayerManagerPresentFn = 0x0044C6B6;
    public const uint PlayerManagerCtorFn = 0x0044C6C2;
    public const uint PlayerManagerStoreFn = 0x0044C71F;
    public const uint PlayerManagerVtbl = 0x01232C24;
    public const int PlayerManagerSize = 0xE0;
    public const int PlayerManagerPlus40 = 0x80000;
    /// <summary>
    /// First <c>+40</c> consume:
    /// <c>004EE337</c>
    /// <c>0044C6B0</c>
    /// <c>009B0AC0</c>
    /// <c>CHeroMorphDef</c>
    /// <c>009AD6E0</c>
    /// <c>009FC4F0</c>.
    /// Not a def parse.
    /// </summary>
    public const uint InitThingComponentsFn = 0x004EE23F;
    public const uint AddDefClassFn = 0x009B0AC0;
    public const uint LoadDefFn = 0x009AD6E0;
    public const uint LoadDefBudgetFn = 0x009FC4F0;
    public const uint FirstDefClassFactory = 0x004E4219;
    public const string FirstDefClassName = "CHeroMorphDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CHeroMorphDef</c>:
    /// <c>004EE565</c>
    /// <c>0044C6B0</c>
    /// <c>004EE56C</c>
    /// <c>CHighlightItemDef</c>
    /// factory <c>0x4D8671</c>
    /// size 72 vtbl <c>0123BD14</c>.
    /// Not a CTC row.
    /// </summary>
    public const uint SecondDefClassSite = 0x004EE565;
    public const uint SecondDefClassFactory = 0x004D8671;
    public const uint SecondDefClassVtbl = 0x0123BD14;
    public const int SecondDefClassSize = 72;
    public const string SecondDefClassName = "CHighlightItemDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CHighlightItemDef</c>:
    /// <c>004EE62B</c>
    /// <c>0044C6B0</c>
    /// <c>004EE632</c>
    /// <c>CSmokeGeneratorDef</c>
    /// factory <c>0x4DA82B</c>
    /// size 48 vtbl <c>0123E924</c>.
    /// Not <c>CTCSmokeGenerator</c>.
    /// </summary>
    public const uint ThirdDefClassSite = 0x004EE62B;
    public const uint ThirdDefClassFactory = 0x004DA82B;
    public const uint ThirdDefClassVtbl = 0x0123E924;
    public const int ThirdDefClassSize = 48;
    public const string ThirdDefClassName = "CSmokeGeneratorDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CSmokeGeneratorDef</c>:
    /// <c>004EE6FD</c>
    /// <c>0044C6B0</c>
    /// <c>004EE704</c>
    /// <c>CTimeAppearanceFadeDef</c>
    /// factory <c>0x4D84C8</c>
    /// size 56 vtbl <c>0123B7CC</c>.
    /// Not <c>CTCTimeAppearanceFade</c>.
    /// </summary>
    public const uint FourthDefClassSite = 0x004EE6FD;
    public const uint FourthDefClassFactory = 0x004D84C8;
    public const uint FourthDefClassVtbl = 0x0123B7CC;
    public const int FourthDefClassSize = 56;
    public const string FourthDefClassName = "CTimeAppearanceFadeDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CTimeAppearanceFadeDef</c>:
    /// <c>004EE92B</c>
    /// <c>0044C6B0</c>
    /// <c>004EE932</c>
    /// <c>CCreatureNavigationDef</c>
    /// factory <c>0x4DA871</c>
    /// size 56 vtbl <c>0123E98C</c>.
    /// Not the four CTC physics
    /// / nav rows.
    /// </summary>
    public const uint FifthDefClassSite = 0x004EE92B;
    public const uint FifthDefClassFactory = 0x004DA871;
    public const uint FifthDefClassVtbl = 0x0123E98C;
    public const int FifthDefClassSize = 56;
    public const string FifthDefClassName = "CCreatureNavigationDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CCreatureNavigationDef</c>:
    /// <c>004EF23D</c>
    /// <c>0044C6B0</c>
    /// <c>004EF244</c>
    /// <c>CInventoryItemDef</c>
    /// factory <c>0x44F644</c>
    /// <c>0044C108</c>
    /// size 112 vtbl <c>01231DBC</c>.
    /// Not intervening CTC rows.
    /// </summary>
    public const uint SixthDefClassSite = 0x004EF23D;
    public const uint SixthDefClassFactory = 0x0044F644;
    public const uint SixthDefClassCtor = 0x0044C108;
    public const uint SixthDefClassVtbl = 0x01231DBC;
    public const int SixthDefClassSize = 112;
    public const string SixthDefClassName = "CInventoryItemDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CInventoryItemDef</c>:
    /// <c>004EF37F</c>
    /// <c>0044C6B0</c>
    /// <c>004EF386</c>
    /// <c>CLookDef</c>
    /// factory <c>0x4D80E4</c>
    /// <c>0044C0C0</c>
    /// size 88 vtbl <c>0123AE14</c>.
    /// Not intervening CTC rows.
    /// </summary>
    public const uint SeventhDefClassSite = 0x004EF37F;
    public const uint SeventhDefClassFactory = 0x004D80E4;
    public const uint SeventhDefClassCtor = 0x0044C0C0;
    public const uint SeventhDefClassVtbl = 0x0123AE14;
    public const int SeventhDefClassSize = 88;
    public const string SeventhDefClassName = "CLookDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CLookDef</c>:
    /// <c>004EF5AD</c>
    /// <c>0044C6B0</c>
    /// <c>004EF5B4</c>
    /// <c>CReadableDef</c>
    /// factory <c>0x4DAA0E</c>
    /// <c>0044C0C0</c>
    /// size 38 vtbl <c>0123E9F4</c>.
    /// Not intervening CTC rows.
    /// </summary>
    public const uint EighthDefClassSite = 0x004EF5AD;
    public const uint EighthDefClassFactory = 0x004DAA0E;
    public const uint EighthDefClassCtor = 0x0044C0C0;
    public const uint EighthDefClassVtbl = 0x0123E9F4;
    public const int EighthDefClassSize = 38;
    public const string EighthDefClassName = "CReadableDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CReadableDef</c>:
    /// <c>004F0171</c>
    /// <c>0044C6B0</c>
    /// <c>004F0178</c>
    /// <c>CVillageDef</c>
    /// factory <c>0x4E213B</c>
    /// pack <c>0042DAE0</c>
    /// <c>004DFF04</c>
    /// size <c>0x10C</c> vtbl
    /// <c>01241DDC</c>.
    /// Not intervening CTC rows.
    /// </summary>
    public const uint NinthDefClassSite = 0x004F0171;
    public const uint NinthDefClassFactory = 0x004E213B;
    public const uint NinthDefClassPackFn = 0x0042DAE0;
    public const uint NinthDefClassCtor = 0x004DFF04;
    public const uint NinthDefClassVtbl = 0x01241DDC;
    public const int NinthDefClassSize = 0x10C;
    public const string NinthDefClassName = "CVillageDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CVillageDef</c>:
    /// <c>004F0227</c>
    /// <c>0044C6B0</c>
    /// <c>004F022E</c>
    /// <c>CVillageMemberDef</c>
    /// factory <c>0x4DA7AD</c>
    /// pack <c>0042DAE0</c>
    /// <c>0044C0C0</c>
    /// size 38 vtbl <c>0123E854</c>.
    /// Not intervening CTC rows.
    /// </summary>
    public const uint TenthDefClassSite = 0x004F0227;
    public const uint TenthDefClassFactory = 0x004DA7AD;
    public const uint TenthDefClassCtor = 0x0044C0C0;
    public const uint TenthDefClassVtbl = 0x0123E854;
    public const int TenthDefClassSize = 38;
    public const string TenthDefClassName = "CVillageMemberDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CVillageMemberDef</c>:
    /// <c>004F02DD</c>
    /// <c>0044C6B0</c>
    /// <c>004F02E4</c>
    /// <c>CBuyableHouseDef</c>
    /// factory <c>0x4E0148</c>
    /// pack <c>0042DAE0</c>
    /// <c>004DDB2C</c>
    /// size 76 vtbl <c>0124131C</c>.
    /// Not intervening CTC rows.
    /// </summary>
    public const uint EleventhDefClassSite = 0x004F02DD;
    public const uint EleventhDefClassFactory = 0x004E0148;
    public const uint EleventhDefClassCtor = 0x004DDB2C;
    public const uint EleventhDefClassVtbl = 0x0124131C;
    public const int EleventhDefClassSize = 76;
    public const string EleventhDefClassName = "CBuyableHouseDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CBuyableHouseDef</c>:
    /// <c>004F0393</c>
    /// <c>0044C6B0</c>
    /// <c>004F039A</c>
    /// <c>CBuyHouseDef</c>
    /// factory <c>0x4D7B5B</c>
    /// pack <c>0042DAE0</c>
    /// <c>0044C0C0</c>
    /// size 38 vtbl <c>0123A61C</c>.
    /// Not intervening CTC rows.
    /// </summary>
    public const uint TwelfthDefClassSite = 0x004F0393;
    public const uint TwelfthDefClassFactory = 0x004D7B5B;
    public const uint TwelfthDefClassCtor = 0x0044C0C0;
    public const uint TwelfthDefClassVtbl = 0x0123A61C;
    public const int TwelfthDefClassSize = 38;
    public const string TwelfthDefClassName = "CBuyHouseDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CBuyHouseDef</c>:
    /// <c>004F04B4</c>
    /// <c>0044C6B0</c>
    /// <c>004F04BB</c>
    /// <c>CWifeDef</c>
    /// factory <c>0x4D7BA1</c>
    /// pack <c>0042DAE0</c>
    /// <c>0044C0C0</c>
    /// size 44 vtbl <c>0123A69C</c>.
    /// </summary>
    public const uint ThirteenthDefClassSite = 0x004F04B4;
    public const uint ThirteenthDefClassFactory = 0x004D7BA1;
    public const uint ThirteenthDefClassCtor = 0x0044C0C0;
    public const uint ThirteenthDefClassVtbl = 0x0123A69C;
    public const int ThirteenthDefClassSize = 44;
    public const string ThirteenthDefClassName = "CWifeDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CWifeDef</c>:
    /// <c>004F0640</c>
    /// <c>0044C6B0</c>
    /// <c>004F0647</c>
    /// <c>CDoorDef</c>
    /// factory <c>0x4D7BE7</c>
    /// pack <c>0042DAE0</c>
    /// <c>0044C0C0</c>
    /// size 60 vtbl <c>0123A714</c>.
    /// </summary>
    public const uint FourteenthDefClassSite = 0x004F0640;
    public const uint FourteenthDefClassFactory = 0x004D7BE7;
    public const uint FourteenthDefClassCtor = 0x0044C0C0;
    public const uint FourteenthDefClassVtbl = 0x0123A714;
    public const int FourteenthDefClassSize = 60;
    public const string FourteenthDefClassName = "CDoorDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CDoorDef</c>:
    /// <c>004F06F6</c>
    /// <c>0044C6B0</c>
    /// <c>004F06FD</c>
    /// <c>CLightDef</c>
    /// factory <c>0x4D7C73</c>
    /// pack <c>0042DAE0</c>
    /// <c>0044C0C0</c>
    /// size 92 vtbl <c>0123A814</c>.
    /// </summary>
    public const uint FifteenthDefClassSite = 0x004F06F6;
    public const uint FifteenthDefClassFactory = 0x004D7C73;
    public const uint FifteenthDefClassCtor = 0x0044C0C0;
    public const uint FifteenthDefClassVtbl = 0x0123A814;
    public const int FifteenthDefClassSize = 92;
    public const string FifteenthDefClassName = "CLightDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CLightDef</c>:
    /// <c>004F07AC</c>
    /// <c>0044C6B0</c>
    /// <c>004F07B3</c>
    /// <c>CSpotLightDef</c>
    /// factory <c>0x4D7CB9</c>
    /// pack <c>0042DAE0</c>
    /// <c>0044C0C0</c>
    /// size 68 vtbl <c>0123A88C</c>.
    /// </summary>
    public const uint SixteenthDefClassSite = 0x004F07AC;
    public const uint SixteenthDefClassFactory = 0x004D7CB9;
    public const uint SixteenthDefClassCtor = 0x0044C0C0;
    public const uint SixteenthDefClassVtbl = 0x0123A88C;
    public const int SixteenthDefClassSize = 68;
    public const string SixteenthDefClassName = "CSpotLightDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CSpotLightDef</c>:
    /// <c>004F0862</c>
    /// <c>0044C6B0</c>
    /// <c>004F0869</c>
    /// <c>CClockDef</c>
    /// factory <c>0x4E4477</c>
    /// pack <c>0042DAE0</c>
    /// <c>004E380E</c>
    /// <c>0044C0C0</c>
    /// size 56 vtbl <c>01242C34</c>.
    /// </summary>
    public const uint SeventeenthDefClassSite = 0x004F0862;
    public const uint SeventeenthDefClassFactory = 0x004E4477;
    public const uint SeventeenthDefClassCtor = 0x004E380E;
    public const uint SeventeenthDefClassVtbl = 0x01242C34;
    public const int SeventeenthDefClassSize = 56;
    public const string SeventeenthDefClassName = "CClockDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CClockDef</c>:
    /// <c>004F0918</c>
    /// <c>0044C6B0</c>
    /// <c>004F091F</c>
    /// <c>CHeroDef</c>
    /// factory <c>0x4D7CFF</c>
    /// pack <c>0042DAE0</c>
    /// <c>0044C0C0</c>
    /// size 48 vtbl <c>0123A904</c>.
    /// </summary>
    public const uint EighteenthDefClassSite = 0x004F0918;
    public const uint EighteenthDefClassFactory = 0x004D7CFF;
    public const uint EighteenthDefClassCtor = 0x0044C0C0;
    public const uint EighteenthDefClassVtbl = 0x0123A904;
    public const int EighteenthDefClassSize = 48;
    public const string EighteenthDefClassName = "CHeroDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CHeroDef</c>:
    /// <c>004F0D26</c>
    /// <c>0044C6B0</c>
    /// <c>004F0D2D</c>
    /// <c>CCreatureModeDef</c>
    /// factory <c>0x4E0B4B</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(64)</c>
    /// <c>jmp 004DE7DC</c>
    /// <c>0044C0C0</c>
    /// size 64 vtbl <c>01241704</c>.
    /// Note-only + flag, not a live
    /// 64-byte object.
    /// </summary>
    public const uint NineteenthDefClassSite = 0x004F0D26;
    public const uint NineteenthDefClassFactory = 0x004E0B4B;
    public const uint NineteenthDefClassCtor = 0x004DE7DC;
    public const uint NineteenthDefClassVtbl = 0x01241704;
    public const int NineteenthDefClassSize = 64;
    public const string NineteenthDefClassName = "CCreatureModeDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CCreatureModeDef</c>:
    /// <c>004F0DDC</c>
    /// <c>0044C6B0</c>
    /// <c>004F0DE3</c>
    /// <c>CPerceivedThingDef</c>
    /// factory <c>0x4D7EB6</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(80)</c>
    /// <c>0044C0C0</c>
    /// size 80 vtbl <c>0123AA9C</c>.
    /// Note-only + flag, not a live
    /// 80-byte object.
    /// </summary>
    public const uint TwentiethDefClassSite = 0x004F0DDC;
    public const uint TwentiethDefClassFactory = 0x004D7EB6;
    public const uint TwentiethDefClassCtor = 0x0044C0C0;
    public const uint TwentiethDefClassVtbl = 0x0123AA9C;
    public const int TwentiethDefClassSize = 80;
    public const string TwentiethDefClassName = "CPerceivedThingDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CPerceivedThingDef</c>:
    /// <c>004F0E92</c>
    /// <c>0044C6B0</c>
    /// <c>004F0E99</c>
    /// <c>CBedDef</c>
    /// factory <c>0x4DA7F3</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(60)</c>
    /// <c>jmp 004D7A25</c>
    /// <c>0044C0C0</c>
    /// size 60 vtbl <c>0123E8BC</c>.
    /// Note-only + flag, not a live
    /// 60-byte object.
    /// </summary>
    public const uint TwentyFirstDefClassSite = 0x004F0E92;
    public const uint TwentyFirstDefClassFactory = 0x004DA7F3;
    public const uint TwentyFirstDefClassCtor = 0x004D7A25;
    public const uint TwentyFirstDefClassVtbl = 0x0123E8BC;
    public const int TwentyFirstDefClassSize = 60;
    public const string TwentyFirstDefClassName = "CBedDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CBedDef</c>:
    /// <c>004F0F48</c>
    /// <c>0044C6B0</c>
    /// <c>004F0F4F</c>
    /// <c>CStealthDef</c>
    /// factory <c>0x4D7EFC</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(72)</c>
    /// <c>0044C0C0</c>
    /// size 72 vtbl <c>0123AB1C</c>.
    /// Note-only + flag, not a live
    /// 72-byte object.
    /// </summary>
    public const uint TwentySecondDefClassSite = 0x004F0F48;
    public const uint TwentySecondDefClassFactory = 0x004D7EFC;
    public const uint TwentySecondDefClassCtor = 0x0044C0C0;
    public const uint TwentySecondDefClassVtbl = 0x0123AB1C;
    public const int TwentySecondDefClassSize = 72;
    public const string TwentySecondDefClassName = "CStealthDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CStealthDef</c>:
    /// <c>004F10D4</c>
    /// <c>0044C6B0</c>
    /// <c>004F10DB</c>
    /// <c>CTrophyDef</c>
    /// factory <c>0x4D7F7B</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(100)</c>
    /// <c>jmp 004D36FE</c>
    /// <c>0044C0C0</c>
    /// size 100 vtbl <c>0123AC1C</c>.
    /// Note-only + flag, not a live
    /// 100-byte object.
    /// </summary>
    public const uint TwentyThirdDefClassSite = 0x004F10D4;
    public const uint TwentyThirdDefClassFactory = 0x004D7F7B;
    public const uint TwentyThirdDefClassCtor = 0x004D36FE;
    public const uint TwentyThirdDefClassVtbl = 0x0123AC1C;
    public const int TwentyThirdDefClassSize = 100;
    public const string TwentyThirdDefClassName = "CTrophyDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CTrophyDef</c>:
    /// <c>004F11F5</c>
    /// <c>0044C6B0</c>
    /// <c>004F11FC</c>
    /// <c>CCreatureGeneratorDef</c>
    /// factory <c>0x4E0513</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(64)</c>
    /// <c>jmp 004DE1DF</c>
    /// <c>0044C0C0</c>
    /// size 64 vtbl <c>01241384</c>.
    /// Note-only + flag, not a live
    /// 64-byte object.
    /// </summary>
    public const uint TwentyFourthDefClassSite = 0x004F11F5;
    public const uint TwentyFourthDefClassFactory = 0x004E0513;
    public const uint TwentyFourthDefClassCtor = 0x004DE1DF;
    public const uint TwentyFourthDefClassVtbl = 0x01241384;
    public const int TwentyFourthDefClassSize = 64;
    public const string TwentyFourthDefClassName = "CCreatureGeneratorDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CCreatureGeneratorDef</c>:
    /// <c>004F12AB</c>
    /// <c>0044C6B0</c>
    /// <c>004F12B2</c>
    /// <c>CChestDef</c>
    /// factory <c>0x4D805C</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(60)</c>
    /// <c>jmp 004D3826</c>
    /// <c>0044C0C0</c>
    /// size 60 vtbl <c>0123ACDC</c>.
    /// Note-only + flag, not a live
    /// 60-byte object.
    /// </summary>
    public const uint TwentyFifthDefClassSite = 0x004F12AB;
    public const uint TwentyFifthDefClassFactory = 0x004D805C;
    public const uint TwentyFifthDefClassCtor = 0x004D3826;
    public const uint TwentyFifthDefClassVtbl = 0x0123ACDC;
    public const int TwentyFifthDefClassSize = 60;
    public const string TwentyFifthDefClassName = "CChestDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CChestDef</c>:
    /// <c>004F14A2</c>
    /// <c>0044C6B0</c>
    /// <c>004F14A9</c>
    /// <c>CExplodingObjectDef</c>
    /// factory <c>0x4D809E</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(52)</c>
    /// <c>0044C0C0</c>
    /// size 52 vtbl <c>0123AD7C</c>.
    /// Note-only + flag, not a live
    /// 52-byte object.
    /// </summary>
    public const uint TwentySixthDefClassSite = 0x004F14A2;
    public const uint TwentySixthDefClassFactory = 0x004D809E;
    public const uint TwentySixthDefClassCtor = 0x0044C0C0;
    public const uint TwentySixthDefClassVtbl = 0x0123AD7C;
    public const int TwentySixthDefClassSize = 52;
    public const string TwentySixthDefClassName = "CExplodingObjectDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CExplodingObjectDef</c>:
    /// <c>004F1558</c>
    /// <c>0044C6B0</c>
    /// <c>004F155F</c>
    /// <c>CContainerRewardHeroDef</c>
    /// factory <c>0x4E3C81</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(52)</c>
    /// <c>jmp 004E247A</c>
    /// <c>0044C0C0</c>
    /// size 52 vtbl <c>012428B4</c>.
    /// Note-only + flag, not a live
    /// 52-byte object.
    /// </summary>
    public const uint TwentySeventhDefClassSite = 0x004F1558;
    public const uint TwentySeventhDefClassFactory = 0x004E3C81;
    public const uint TwentySeventhDefClassCtor = 0x004E247A;
    public const uint TwentySeventhDefClassVtbl = 0x012428B4;
    public const int TwentySeventhDefClassSize = 52;
    public const string TwentySeventhDefClassName = "CContainerRewardHeroDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CContainerRewardHeroDef</c>:
    /// <c>004F1CBA</c>
    /// <c>0044C6B0</c>
    /// <c>004F1CC1</c>
    /// <c>CWeaponDef</c>
    /// factory <c>0x4E3D15</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(0xE4)</c>
    /// <c>jmp 004E2612</c>
    /// <c>0044C0C0</c>
    /// size 228 vtbl <c>0124291C</c>.
    /// Note-only + flag, not a live
    /// 228-byte object.
    /// </summary>
    public const uint TwentyEighthDefClassSite = 0x004F1CBA;
    public const uint TwentyEighthDefClassFactory = 0x004E3D15;
    public const uint TwentyEighthDefClassCtor = 0x004E2612;
    public const uint TwentyEighthDefClassVtbl = 0x0124291C;
    public const int TwentyEighthDefClassSize = 228;
    public const string TwentyEighthDefClassName = "CWeaponDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CWeaponDef</c>:
    /// <c>004F1D70</c>
    /// <c>0044C6B0</c>
    /// <c>004F1D77</c>
    /// <c>CCarryingDef</c>
    /// factory <c>0x4DFE62</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(56)</c>
    /// <c>jmp 004DD8FF</c>
    /// <c>0044C0C0</c>
    /// size 56 vtbl <c>01241194</c>.
    /// Note-only + flag, not a live
    /// 56-byte object.
    /// </summary>
    public const uint TwentyNinthDefClassSite = 0x004F1D70;
    public const uint TwentyNinthDefClassFactory = 0x004DFE62;
    public const uint TwentyNinthDefClassCtor = 0x004DD8FF;
    public const uint TwentyNinthDefClassVtbl = 0x01241194;
    public const int TwentyNinthDefClassSize = 56;
    public const string TwentyNinthDefClassName = "CCarryingDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CCarryingDef</c>:
    /// <c>004F1E26</c>
    /// <c>0044C6B0</c>
    /// <c>004F1E2D</c>
    /// <c>CCarryableDef</c>
    /// factory <c>0x4DA767</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(80)</c>
    /// <c>0044C0C0</c>
    /// size 80 vtbl <c>0123E7EC</c>.
    /// Note-only + flag, not a live
    /// 80-byte object.
    /// </summary>
    public const uint ThirtiethDefClassSite = 0x004F1E26;
    public const uint ThirtiethDefClassFactory = 0x004DA767;
    public const uint ThirtiethDefClassCtor = 0x0044C0C0;
    public const uint ThirtiethDefClassVtbl = 0x0123E7EC;
    public const int ThirtiethDefClassSize = 80;
    public const string ThirtiethDefClassName = "CCarryableDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CCarryableDef</c>:
    /// <c>004F1EDC</c>
    /// <c>0044C6B0</c>
    /// <c>004F1EE3</c>
    /// <c>CEnemyDef</c>
    /// factory <c>0x4D835A</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(44)</c>
    /// <c>0044C0C0</c>
    /// size 44 vtbl <c>0123B3F4</c>.
    /// Note-only + flag, not a live
    /// 44-byte object.
    /// </summary>
    public const uint ThirtyFirstDefClassSite = 0x004F1EDC;
    public const uint ThirtyFirstDefClassFactory = 0x004D835A;
    public const uint ThirtyFirstDefClassCtor = 0x0044C0C0;
    public const uint ThirtyFirstDefClassVtbl = 0x0123B3F4;
    public const int ThirtyFirstDefClassSize = 44;
    public const string ThirtyFirstDefClassName = "CEnemyDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CEnemyDef</c>:
    /// <c>004F22E6</c>
    /// <c>0044C6B0</c>
    /// <c>004F22ED</c>
    /// <c>COpinionOfHeroDef</c>
    /// factory <c>0x4D83D9</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(60)</c>
    /// <c>jmp 004D3F83</c>
    /// <c>0044C0C0</c>
    /// size 60 vtbl <c>0123B59C</c>.
    /// Note-only + flag, not a live
    /// 60-byte object.
    /// </summary>
    public const uint ThirtySecondDefClassSite = 0x004F22E6;
    public const uint ThirtySecondDefClassFactory = 0x004D83D9;
    public const uint ThirtySecondDefClassCtor = 0x004D3F83;
    public const uint ThirtySecondDefClassVtbl = 0x0123B59C;
    public const int ThirtySecondDefClassSize = 60;
    public const string ThirtySecondDefClassName = "COpinionOfHeroDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>COpinionOfHeroDef</c>:
    /// <c>004F2472</c>
    /// <c>0044C6B0</c>
    /// <c>004F2479</c>
    /// <c>CShopDef</c>
    /// factory <c>0x4E26BC</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(0xD0)</c>
    /// <c>jmp 004E06C3</c>
    /// <c>0044C0C0</c>
    /// size 208 vtbl <c>01241F9C</c>.
    /// Note-only + flag, not a live
    /// 208-byte object.
    /// </summary>
    public const uint ThirtyThirdDefClassSite = 0x004F2472;
    public const uint ThirtyThirdDefClassFactory = 0x004E26BC;
    public const uint ThirtyThirdDefClassCtor = 0x004E06C3;
    public const uint ThirtyThirdDefClassVtbl = 0x01241F9C;
    public const int ThirtyThirdDefClassSize = 208;
    public const string ThirtyThirdDefClassName = "CShopDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CShopDef</c>:
    /// <c>004F2593</c>
    /// <c>0044C6B0</c>
    /// <c>004F259A</c>
    /// <c>CStockItemDef</c>
    /// factory <c>0x4D8482</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(64)</c>
    /// <c>0044C0C0</c>
    /// size 64 vtbl <c>0123B74C</c>.
    /// Note-only + flag, not a live
    /// 64-byte object.
    /// </summary>
    public const uint ThirtyFourthDefClassSite = 0x004F2593;
    public const uint ThirtyFourthDefClassFactory = 0x004D8482;
    public const uint ThirtyFourthDefClassCtor = 0x0044C0C0;
    public const uint ThirtyFourthDefClassVtbl = 0x0123B74C;
    public const int ThirtyFourthDefClassSize = 64;
    public const string ThirtyFourthDefClassName = "CStockItemDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CStockItemDef</c>:
    /// <c>004F2649</c>
    /// <c>0044C6B0</c>
    /// <c>004F2650</c>
    /// <c>CGiftDef</c>
    /// factory <c>0x4D8547</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(48)</c>
    /// <c>0044C0C0</c>
    /// size 48 vtbl <c>0123B8B4</c>.
    /// Note-only + flag, not a live
    /// 48-byte object.
    /// </summary>
    public const uint ThirtyFifthDefClassSite = 0x004F2649;
    public const uint ThirtyFifthDefClassFactory = 0x004D8547;
    public const uint ThirtyFifthDefClassCtor = 0x0044C0C0;
    public const uint ThirtyFifthDefClassVtbl = 0x0123B8B4;
    public const int ThirtyFifthDefClassSize = 48;
    public const string ThirtyFifthDefClassName = "CGiftDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CGiftDef</c>:
    /// <c>004F27CD</c>
    /// <c>0044C6B0</c>
    /// <c>004F27D4</c>
    /// <c>CHeroSuitDef</c>
    /// factory <c>0x4E2809</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(52)</c>
    /// <c>jmp 004E0900</c>
    /// <c>0044C0C0</c>
    /// size 52 vtbl <c>0124216C</c>.
    /// Note-only + flag, not a live
    /// 52-byte object.
    /// </summary>
    public const uint ThirtySixthDefClassSite = 0x004F27CD;
    public const uint ThirtySixthDefClassFactory = 0x004E2809;
    public const uint ThirtySixthDefClassCtor = 0x004E0900;
    public const uint ThirtySixthDefClassVtbl = 0x0124216C;
    public const int ThirtySixthDefClassSize = 52;
    public const string ThirtySixthDefClassName = "CHeroSuitDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CHeroSuitDef</c>:
    /// <c>004F2C36</c>
    /// <c>0044C6B0</c>
    /// <c>004F2C3D</c>
    /// <c>CHeroExperienceDef</c>
    /// factory <c>0x4EBAE7</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(0xB4)</c>
    /// <c>jmp 004EB9E8</c>
    /// <c>0044C0C0</c>
    /// size 180 vtbl <c>0124390C</c>.
    /// Note-only + flag, not a live
    /// 180-byte object.
    /// </summary>
    public const uint ThirtySeventhDefClassSite = 0x004F2C36;
    public const uint ThirtySeventhDefClassFactory = 0x004EBAE7;
    public const uint ThirtySeventhDefClassCtor = 0x004EB9E8;
    public const uint ThirtySeventhDefClassVtbl = 0x0124390C;
    public const int ThirtySeventhDefClassSize = 180;
    public const string ThirtySeventhDefClassName = "CHeroExperienceDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CHeroExperienceDef</c>:
    /// <c>004F2CE8</c>
    /// <c>0044C6B0</c>
    /// <c>004F2CEF</c>
    /// <c>CExperienceDef</c>
    /// factory <c>0x4E27AE</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(80)</c>
    /// <c>jmp 004E0860</c>
    /// <c>0044C0C0</c>
    /// size 80 vtbl <c>01242104</c>.
    /// Note-only + flag, not a live
    /// 80-byte object.
    /// </summary>
    public const uint ThirtyEighthDefClassSite = 0x004F2CE8;
    public const uint ThirtyEighthDefClassFactory = 0x004E27AE;
    public const uint ThirtyEighthDefClassCtor = 0x004E0860;
    public const uint ThirtyEighthDefClassVtbl = 0x01242104;
    public const int ThirtyEighthDefClassSize = 80;
    public const string ThirtyEighthDefClassName = "CExperienceDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CExperienceDef</c>:
    /// <c>004F2FB5</c>
    /// <c>0044C6B0</c>
    /// <c>004F2FBC</c>
    /// <c>CReplaceableMeshDef</c>
    /// factory <c>0x4E60D8</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(52)</c>
    /// <c>jmp 004E3E4C</c>
    /// <c>0044C0C0</c>
    /// size 52 vtbl <c>012430BC</c>.
    /// Note-only + flag, not a live
    /// 52-byte object.
    /// </summary>
    public const uint ThirtyNinthDefClassSite = 0x004F2FB5;
    public const uint ThirtyNinthDefClassFactory = 0x004E60D8;
    public const uint ThirtyNinthDefClassCtor = 0x004E3E4C;
    public const uint ThirtyNinthDefClassVtbl = 0x012430BC;
    public const int ThirtyNinthDefClassSize = 52;
    public const string ThirtyNinthDefClassName = "CReplaceableMeshDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CReplaceableMeshDef</c>:
    /// <c>004F306B</c>
    /// <c>0044C6B0</c>
    /// <c>004F3072</c>
    /// <c>CMultiStaticMeshDef</c>
    /// factory <c>0x4E31FA</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(52)</c>
    /// <c>jmp 004E1516</c>
    /// <c>0044C0C0</c>
    /// size 52 vtbl <c>0124265C</c>.
    /// Note-only + flag, not a live
    /// 52-byte object.
    /// </summary>
    public const uint FortiethDefClassSite = 0x004F306B;
    public const uint FortiethDefClassFactory = 0x004E31FA;
    public const uint FortiethDefClassCtor = 0x004E1516;
    public const uint FortiethDefClassVtbl = 0x0124265C;
    public const int FortiethDefClassSize = 52;
    public const string FortiethDefClassName = "CMultiStaticMeshDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CMultiStaticMeshDef</c>:
    /// <c>004F3338</c>
    /// <c>0044C6B0</c>
    /// <c>004F333F</c>
    /// <c>CHeroCentreDef</c>
    /// factory <c>0x4D86F0</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(37)</c>
    /// <c>0044C0C0</c>
    /// size 37 vtbl <c>0123BE54</c>.
    /// Note-only + flag, not a live
    /// 37-byte object.
    /// </summary>
    public const uint FortyFirstDefClassSite = 0x004F3338;
    public const uint FortyFirstDefClassFactory = 0x004D86F0;
    public const uint FortyFirstDefClassCtor = 0x0044C0C0;
    public const uint FortyFirstDefClassVtbl = 0x0123BE54;
    public const int FortyFirstDefClassSize = 37;
    public const string FortyFirstDefClassName = "CHeroCentreDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CHeroCentreDef</c>:
    /// <c>004F34C4</c>
    /// <c>0044C6B0</c>
    /// <c>004F34CB</c>
    /// <c>CQuestCardDef</c>
    /// factory <c>0x4E2333</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(116)</c>
    /// <c>jmp 004E00BC</c>
    /// <c>0044C0C0</c>
    /// size 116 vtbl <c>01241E44</c>.
    /// Note-only + flag, not a live
    /// 116-byte object. Does not
    /// construct
    /// <c>Q_NewOakValeIntro</c>.
    /// </summary>
    public const uint FortySecondDefClassSite = 0x004F34C4;
    public const uint FortySecondDefClassFactory = 0x004E2333;
    public const uint FortySecondDefClassCtor = 0x004E00BC;
    public const uint FortySecondDefClassVtbl = 0x01241E44;
    public const int FortySecondDefClassSize = 116;
    public const string FortySecondDefClassName = "CQuestCardDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CQuestCardDef</c>:
    /// <c>004F357A</c>
    /// <c>0044C6B0</c>
    /// <c>004F3581</c>
    /// <c>CFlammableDef</c>
    /// factory <c>0x4E3DC3</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(76)</c>
    /// <c>jmp 004E284B</c>
    /// <c>0044C0C0</c>
    /// size 76 vtbl <c>01242984</c>.
    /// Note-only + flag, not a live
    /// 76-byte object.
    /// </summary>
    public const uint FortyThirdDefClassSite = 0x004F357A;
    public const uint FortyThirdDefClassFactory = 0x004E3DC3;
    public const uint FortyThirdDefClassCtor = 0x004E284B;
    public const uint FortyThirdDefClassVtbl = 0x01242984;
    public const int FortyThirdDefClassSize = 76;
    public const string FortyThirdDefClassName = "CFlammableDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CFlammableDef</c>:
    /// <c>004F3630</c>
    /// <c>0044C6B0</c>
    /// <c>004F3637</c>
    /// <c>CBoastingPodiumDef</c>
    /// factory <c>0x4D8736</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(44)</c>
    /// <c>0044C0C0</c>
    /// size 44 vtbl <c>0123BF0C</c>.
    /// Note-only + flag, not a live
    /// 44-byte object.
    /// </summary>
    public const uint FortyFourthDefClassSite = 0x004F3630;
    public const uint FortyFourthDefClassFactory = 0x004D8736;
    public const uint FortyFourthDefClassCtor = 0x0044C0C0;
    public const uint FortyFourthDefClassVtbl = 0x0123BF0C;
    public const int FortyFourthDefClassSize = 44;
    public const string FortyFourthDefClassName = "CBoastingPodiumDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CBoastingPodiumDef</c>:
    /// <c>004F3892</c>
    /// <c>0044C6B0</c>
    /// <c>004F3899</c>
    /// <c>CTCVolumeContainmentTrackerDef</c>
    /// factory <c>0x4D94C8</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(48)</c>
    /// <c>0044C0C0</c>
    /// size 48 vtbl <c>0123E0C4</c>.
    /// Note-only + flag, not a live
    /// 48-byte object.
    /// </summary>
    public const uint FortyFifthDefClassSite = 0x004F3892;
    public const uint FortyFifthDefClassFactory = 0x004D94C8;
    public const uint FortyFifthDefClassCtor = 0x0044C0C0;
    public const uint FortyFifthDefClassVtbl = 0x0123E0C4;
    public const int FortyFifthDefClassSize = 48;
    public const string FortyFifthDefClassName = "CTCVolumeContainmentTrackerDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CTCVolumeContainmentTrackerDef</c>:
    /// <c>004F3E4C</c>
    /// <c>0044C6B0</c>
    /// <c>004F3E53</c>
    /// <c>CThingDrainLifeShotDef</c>
    /// factory <c>0x4D8D56</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(60)</c>
    /// <c>0044C0C0</c>
    /// size 60 vtbl <c>0123CCA4</c>.
    /// Note-only + flag, not a live
    /// 60-byte object.
    /// </summary>
    public const uint FortySixthDefClassSite = 0x004F3E4C;
    public const uint FortySixthDefClassFactory = 0x004D8D56;
    public const uint FortySixthDefClassCtor = 0x0044C0C0;
    public const uint FortySixthDefClassVtbl = 0x0123CCA4;
    public const int FortySixthDefClassSize = 60;
    public const string FortySixthDefClassName = "CThingDrainLifeShotDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CThingDrainLifeShotDef</c>:
    /// <c>004F3F02</c>
    /// <c>0044C6B0</c>
    /// <c>004F3F09</c>
    /// <c>CFireballSpellLevelDef</c>
    /// factory <c>0x4D8D10</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(44)</c>
    /// <c>0044C0C0</c>
    /// size 44 vtbl <c>0123CC3C</c>.
    /// Note-only + flag, not a live
    /// 44-byte object.
    /// </summary>
    public const uint FortySeventhDefClassSite = 0x004F3F02;
    public const uint FortySeventhDefClassFactory = 0x004D8D10;
    public const uint FortySeventhDefClassCtor = 0x0044C0C0;
    public const uint FortySeventhDefClassVtbl = 0x0123CC3C;
    public const int FortySeventhDefClassSize = 44;
    public const string FortySeventhDefClassName = "CFireballSpellLevelDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CFireballSpellLevelDef</c>:
    /// <c>004F40F9</c>
    /// <c>0044C6B0</c>
    /// <c>004F4100</c>
    /// <c>CSkeletalMorphDef</c>
    /// factory <c>0x4E3DD9</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(52)</c>
    /// <c>jmp 004E2895</c>
    /// <c>0044C0C0</c>
    /// size 52 vtbl <c>012429EC</c>.
    /// Note-only + flag, not a live
    /// 52-byte object.
    /// </summary>
    public const uint FortyEighthDefClassSite = 0x004F40F9;
    public const uint FortyEighthDefClassFactory = 0x004E3DD9;
    public const uint FortyEighthDefClassCtor = 0x004E2895;
    public const uint FortyEighthDefClassVtbl = 0x012429EC;
    public const int FortyEighthDefClassSize = 52;
    public const string FortyEighthDefClassName = "CSkeletalMorphDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CSkeletalMorphDef</c>:
    /// <c>004F43C6</c>
    /// <c>0044C6B0</c>
    /// <c>004F43CD</c>
    /// <c>CTrapDef</c>
    /// factory <c>0x4E5CF2</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(100)</c>
    /// <c>jmp 004E3E2A</c>
    /// <c>0044C0C0</c>
    /// size 100 vtbl <c>01243054</c>.
    /// Note-only + flag, not a live
    /// 100-byte object.
    /// </summary>
    public const uint FortyNinthDefClassSite = 0x004F43C6;
    public const uint FortyNinthDefClassFactory = 0x004E5CF2;
    public const uint FortyNinthDefClassCtor = 0x004E3E2A;
    public const uint FortyNinthDefClassVtbl = 0x01243054;
    public const int FortyNinthDefClassSize = 100;
    public const string FortyNinthDefClassName = "CTrapDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CTrapDef</c>:
    /// <c>004F4628</c>
    /// <c>0044C6B0</c>
    /// <c>004F462F</c>
    /// <c>CParticleAttacherDef</c>
    /// factory <c>0x4E2AFA</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(52)</c>
    /// <c>jmp 004E0B9C</c>
    /// <c>0044C0C0</c>
    /// size 52 vtbl <c>01242364</c>.
    /// Note-only + flag, not a live
    /// 52-byte object. GPU attach
    /// remains skipped
    /// (<c>FirstSeenCanRenderParticles</c>).
    /// </summary>
    public const uint FiftiethDefClassSite = 0x004F4628;
    public const uint FiftiethDefClassFactory = 0x004E2AFA;
    public const uint FiftiethDefClassCtor = 0x004E0B9C;
    public const uint FiftiethDefClassVtbl = 0x01242364;
    public const int FiftiethDefClassSize = 52;
    public const string FiftiethDefClassName = "CParticleAttacherDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CParticleAttacherDef</c>:
    /// <c>004F46DE</c>
    /// <c>0044C6B0</c>
    /// <c>004F46E5</c>
    /// <c>CAnimatingObjectDef</c>
    /// factory <c>0x4EBA6E</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(72)</c>
    /// <c>jmp 004EA1F0</c>
    /// <c>0044C0C0</c>
    /// size 72 vtbl <c>0124376C</c>.
    /// Note-only + flag, not a live
    /// 72-byte object.
    /// </summary>
    public const uint FiftyFirstDefClassSite = 0x004F46DE;
    public const uint FiftyFirstDefClassFactory = 0x004EBA6E;
    public const uint FiftyFirstDefClassCtor = 0x004EA1F0;
    public const uint FiftyFirstDefClassVtbl = 0x0124376C;
    public const int FiftyFirstDefClassSize = 72;
    public const string FiftyFirstDefClassName = "CAnimatingObjectDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CAnimatingObjectDef</c>:
    /// <c>004F4A16</c>
    /// <c>0044C6B0</c>
    /// <c>004F4A1D</c>
    /// <c>CExpressionSubDef</c>
    /// factory <c>0x4D8818</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(44)</c>
    /// <c>0044C0C0</c>
    /// size 44 vtbl <c>0123C2E4</c>.
    /// Note-only + flag, not a live
    /// 44-byte object. Persist is u32
    /// at <c>+40</c>, not a quest
    /// CString. Not
    /// <c>ActivateQuest</c>.
    /// </summary>
    public const uint FiftySecondDefClassSite = 0x004F4A16;
    public const uint FiftySecondDefClassFactory = 0x004D8818;
    public const uint FiftySecondDefClassCtor = 0x0044C0C0;
    public const uint FiftySecondDefClassVtbl = 0x0123C2E4;
    public const int FiftySecondDefClassSize = 44;
    public const string FiftySecondDefClassName = "CExpressionSubDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CExpressionSubDef</c>:
    /// <c>004F4DB9</c>
    /// <c>0044C6B0</c>
    /// <c>004F4DC0</c>
    /// <c>CWillResponseDef</c>
    /// factory <c>0x4D9629</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(45)</c>
    /// <c>0044C0C0</c>
    /// size 45 vtbl <c>0123E324</c>.
    /// Note-only + flag, not a live
    /// 45-byte object.
    /// </summary>
    public const uint FiftyThirdDefClassSite = 0x004F4DB9;
    public const uint FiftyThirdDefClassFactory = 0x004D9629;
    public const uint FiftyThirdDefClassCtor = 0x0044C0C0;
    public const uint FiftyThirdDefClassVtbl = 0x0123E324;
    public const int FiftyThirdDefClassSize = 45;
    public const string FiftyThirdDefClassName = "CWillResponseDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CWillResponseDef</c>:
    /// <c>004F4F45</c>
    /// <c>0044C6B0</c>
    /// <c>004F4F4C</c>
    /// <c>CTurncoatDef</c>
    /// factory <c>0x4E0F9C</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(84)</c>
    /// <c>jmp 004DEBA3</c>
    /// <c>0044C0C0</c>
    /// size 84 vtbl <c>0124193C</c>.
    /// Note-only + flag, not a live
    /// 84-byte object.
    /// </summary>
    public const uint FiftyFourthDefClassSite = 0x004F4F45;
    public const uint FiftyFourthDefClassFactory = 0x004E0F9C;
    public const uint FiftyFourthDefClassCtor = 0x004DEBA3;
    public const uint FiftyFourthDefClassVtbl = 0x0124193C;
    public const int FiftyFourthDefClassSize = 84;
    public const string FiftyFourthDefClassName = "CTurncoatDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CTurncoatDef</c>:
    /// <c>004F4FFB</c>
    /// <c>0044C6B0</c>
    /// <c>004F5002</c>
    /// <c>CSummonableCreatureDef</c>
    /// factory <c>0x4D885E</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(48)</c>
    /// <c>jmp 004D4C3E</c>
    /// <c>0044C0C0</c>
    /// size 48 vtbl <c>0123C3A4</c>.
    /// Note-only + flag, not a live
    /// 48-byte object.
    /// </summary>
    public const uint FiftyFifthDefClassSite = 0x004F4FFB;
    public const uint FiftyFifthDefClassFactory = 0x004D885E;
    public const uint FiftyFifthDefClassCtor = 0x004D4C3E;
    public const uint FiftyFifthDefClassVtbl = 0x0123C3A4;
    public const int FiftyFifthDefClassSize = 48;
    public const string FiftyFifthDefClassName = "CSummonableCreatureDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CSummonableCreatureDef</c>:
    /// <c>004F55B5</c>
    /// <c>0044C6B0</c>
    /// <c>004F55BC</c>
    /// <c>CAIScratchpadDef</c>
    /// factory <c>0x4D4E07</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(156)</c>
    /// <c>jmp 007ABB30</c>
    /// <c>009FBEC0</c>
    /// size 156 vtbl <c>0126D014</c>.
    /// Note-only + flag, not a live
    /// 156-byte object.
    /// </summary>
    public const uint FiftySixthDefClassSite = 0x004F55B5;
    public const uint FiftySixthDefClassFactory = 0x004D4E07;
    public const uint FiftySixthDefClassCtor = 0x007ABB30;
    public const uint FiftySixthDefClassVtbl = 0x0126D014;
    public const int FiftySixthDefClassSize = 156;
    public const string FiftySixthDefClassName = "CAIScratchpadDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CAIScratchpadDef</c>:
    /// <c>004F566B</c>
    /// <c>0044C6B0</c>
    /// <c>004F5672</c>
    /// <c>COccupiableDef</c>
    /// factory <c>0x4D88FC</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(44)</c>
    /// <c>0044C0C0</c>
    /// size 44 vtbl <c>0123C514</c>.
    /// Note-only + flag, not a live
    /// 44-byte object.
    /// </summary>
    public const uint FiftySeventhDefClassSite = 0x004F566B;
    public const uint FiftySeventhDefClassFactory = 0x004D88FC;
    public const uint FiftySeventhDefClassCtor = 0x0044C0C0;
    public const uint FiftySeventhDefClassVtbl = 0x0123C514;
    public const int FiftySeventhDefClassSize = 44;
    public const string FiftySeventhDefClassName = "COccupiableDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>COccupiableDef</c>:
    /// <c>004F5721</c>
    /// <c>0044C6B0</c>
    /// <c>004F5728</c>
    /// <c>CBossDef</c>
    /// factory <c>0x4E0D4C</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(84)</c>
    /// <c>jmp 004DE8C2</c>
    /// <c>0044C0C0</c>
    /// size 84 vtbl <c>0124185C</c>.
    /// Note-only + flag, not a live
    /// 84-byte object.
    /// </summary>
    public const uint FiftyEighthDefClassSite = 0x004F5721;
    public const uint FiftyEighthDefClassFactory = 0x004E0D4C;
    public const uint FiftyEighthDefClassCtor = 0x004DE8C2;
    public const uint FiftyEighthDefClassVtbl = 0x0124185C;
    public const int FiftyEighthDefClassSize = 84;
    public const string FiftyEighthDefClassName = "CBossDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CBossDef</c>:
    /// <c>004F5918</c>
    /// <c>0044C6B0</c>
    /// <c>004F591F</c>
    /// <c>CFishingDef</c>
    /// factory <c>0x4E0DB9</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(124)</c>
    /// <c>jmp 004DE8F5</c>
    /// <c>0044C0C0</c>
    /// size 124 vtbl <c>012418C4</c>.
    /// Note-only + flag, not a live
    /// 124-byte object.
    /// </summary>
    public const uint FiftyNinthDefClassSite = 0x004F5918;
    public const uint FiftyNinthDefClassFactory = 0x004E0DB9;
    public const uint FiftyNinthDefClassCtor = 0x004DE8F5;
    public const uint FiftyNinthDefClassVtbl = 0x012418C4;
    public const int FiftyNinthDefClassSize = 124;
    public const string FiftyNinthDefClassName = "CFishingDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CFishingDef</c>:
    /// <c>004F5A39</c>
    /// <c>0044C6B0</c>
    /// <c>004F5A40</c>
    /// <c>CGuardDef</c>
    /// factory <c>0x4D89EC</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(80)</c>
    /// <c>0044C0C0</c>
    /// size 80 vtbl <c>0123C75C</c>.
    /// Note-only + flag, not a live
    /// 80-byte object.
    /// </summary>
    public const uint SixtiethDefClassSite = 0x004F5A39;
    public const uint SixtiethDefClassFactory = 0x004D89EC;
    public const uint SixtiethDefClassCtor = 0x0044C0C0;
    public const uint SixtiethDefClassVtbl = 0x0123C75C;
    public const int SixtiethDefClassSize = 80;
    public const string SixtiethDefClassName = "CGuardDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CGuardDef</c>:
    /// <c>004F5AEF</c>
    /// <c>0044C6B0</c>
    /// <c>004F5AF6</c>
    /// <c>CInterestingToVillagersDef</c>
    /// factory <c>0x4D89B4</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(44)</c>
    /// <c>jmp 004D4FCF</c>
    /// <c>0044C0C0</c>
    /// size 44 vtbl <c>0123C6D4</c>.
    /// Note-only + flag, not a live
    /// 44-byte object.
    /// </summary>
    public const uint SixtyFirstDefClassSite = 0x004F5AEF;
    public const uint SixtyFirstDefClassFactory = 0x004D89B4;
    public const uint SixtyFirstDefClassCtor = 0x004D4FCF;
    public const uint SixtyFirstDefClassVtbl = 0x0123C6D4;
    public const int SixtyFirstDefClassSize = 44;
    public const string SixtyFirstDefClassName = "CInterestingToVillagersDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CInterestingToVillagersDef</c>:
    /// <c>004F5BA5</c>
    /// <c>0044C6B0</c>
    /// <c>004F5BAC</c>
    /// <c>CActivateQuestDef</c>
    /// factory <c>0x4D8A32</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(48)</c>
    /// <c>jmp 004D5056</c>
    /// <c>0044C0C0</c>
    /// size 48 vtbl <c>0123C7F4</c>.
    /// Note-only + flag, not a live
    /// 48-byte object. Registrar only.
    /// Does not
    /// <c>ActivateQuest("Q_NewOakValeIntro")</c>.
    /// </summary>
    public const uint SixtySecondDefClassSite = 0x004F5BA5;
    public const uint SixtySecondDefClassFactory = 0x004D8A32;
    public const uint SixtySecondDefClassCtor = 0x004D5056;
    public const uint SixtySecondDefClassVtbl = 0x0123C7F4;
    public const int SixtySecondDefClassSize = 48;
    public const string SixtySecondDefClassName = "CActivateQuestDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CActivateQuestDef</c>:
    /// <c>004F5CC6</c>
    /// <c>0044C6B0</c>
    /// <c>004F5CCD</c>
    /// <c>CCrateStackDef</c>
    /// factory <c>0x4D8A6A</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(48)</c>
    /// <c>0044C0C0</c>
    /// size 48 vtbl <c>0123C86C</c>.
    /// Note-only + flag, not a live
    /// 48-byte object.
    /// </summary>
    public const uint SixtyThirdDefClassSite = 0x004F5CC6;
    public const uint SixtyThirdDefClassFactory = 0x004D8A6A;
    public const uint SixtyThirdDefClassCtor = 0x0044C0C0;
    public const uint SixtyThirdDefClassVtbl = 0x0123C86C;
    public const int SixtyThirdDefClassSize = 48;
    public const string SixtyThirdDefClassName = "CCrateStackDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CCrateStackDef</c>:
    /// <c>004F5D7C</c>
    /// <c>0044C6B0</c>
    /// <c>004F5D83</c>
    /// <c>COverheadDisplayDef</c>
    /// factory <c>0x4D8AB0</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(40)</c>
    /// <c>0044C0C0</c>
    /// size 40 vtbl <c>0123C8FC</c>.
    /// Note-only + flag, not a live
    /// 40-byte object.
    /// </summary>
    public const uint SixtyFourthDefClassSite = 0x004F5D7C;
    public const uint SixtyFourthDefClassFactory = 0x004D8AB0;
    public const uint SixtyFourthDefClassCtor = 0x0044C0C0;
    public const uint SixtyFourthDefClassVtbl = 0x0123C8FC;
    public const int SixtyFourthDefClassSize = 40;
    public const string SixtyFourthDefClassName = "COverheadDisplayDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>COverheadDisplayDef</c>:
    /// <c>004F5E32</c>
    /// <c>0044C6B0</c>
    /// <c>004F5E39</c>
    /// <c>CTavernTableDef</c>
    /// factory <c>0x4D8AF6</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(39)</c>
    /// <c>0044C0C0</c>
    /// size 39 vtbl <c>0123C964</c>.
    /// Note-only + flag, not a live
    /// 39-byte object.
    /// </summary>
    public const uint SixtyFifthDefClassSite = 0x004F5E32;
    public const uint SixtyFifthDefClassFactory = 0x004D8AF6;
    public const uint SixtyFifthDefClassCtor = 0x0044C0C0;
    public const uint SixtyFifthDefClassVtbl = 0x0123C964;
    public const int SixtyFifthDefClassSize = 39;
    public const string SixtyFifthDefClassName = "CTavernTableDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CTavernTableDef</c>:
    /// <c>004F6029</c>
    /// <c>0044C6B0</c>
    /// <c>004F6030</c>
    /// <c>CTavernDef</c>
    /// factory <c>0x4D8BE1</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(44)</c>
    /// <c>0044C0C0</c>
    /// size 44 vtbl <c>0123CA8C</c>.
    /// Note-only + flag, not a live
    /// 44-byte object. Not Oakvale
    /// tavern spawn.
    /// </summary>
    public const uint SixtySixthDefClassSite = 0x004F6029;
    public const uint SixtySixthDefClassFactory = 0x004D8BE1;
    public const uint SixtySixthDefClassCtor = 0x0044C0C0;
    public const uint SixtySixthDefClassVtbl = 0x0123CA8C;
    public const int SixtySixthDefClassSize = 44;
    public const string SixtySixthDefClassName = "CTavernDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CTavernDef</c>:
    /// <c>004F60DF</c>
    /// <c>0044C6B0</c>
    /// <c>004F60E6</c>
    /// <c>CObjectAugmentationsDef</c>
    /// factory <c>0x4EC526</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(140)</c>
    /// <c>jmp 004EBBA3</c>
    /// <c>0044C0C0</c>
    /// size 140 vtbl <c>01243974</c>.
    /// Note-only + flag, not a live
    /// 140-byte object.
    /// </summary>
    public const uint SixtySeventhDefClassSite = 0x004F60DF;
    public const uint SixtySeventhDefClassFactory = 0x004EC526;
    public const uint SixtySeventhDefClassCtor = 0x004EBBA3;
    public const uint SixtySeventhDefClassVtbl = 0x01243974;
    public const int SixtySeventhDefClassSize = 140;
    public const string SixtySeventhDefClassName = "CObjectAugmentationsDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CObjectAugmentationsDef</c>:
    /// <c>004F63AC</c>
    /// <c>0044C6B0</c>
    /// <c>004F63B3</c>
    /// <c>CDrunkennessDef</c>
    /// factory <c>0x4D8C91</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(44)</c>
    /// <c>0044C0C0</c>
    /// size 44 vtbl <c>0123CB3C</c>.
    /// Note-only + flag, not a live
    /// 44-byte object.
    /// </summary>
    public const uint SixtyEighthDefClassSite = 0x004F63AC;
    public const uint SixtyEighthDefClassFactory = 0x004D8C91;
    public const uint SixtyEighthDefClassCtor = 0x0044C0C0;
    public const uint SixtyEighthDefClassVtbl = 0x0123CB3C;
    public const int SixtyEighthDefClassSize = 44;
    public const string SixtyEighthDefClassName = "CDrunkennessDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CDrunkennessDef</c>:
    /// <c>004F67BA</c>
    /// <c>0044C6B0</c>
    /// <c>004F67C1</c>
    /// <c>CGoldDef</c>
    /// factory <c>0x4D8EC5</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(44)</c>
    /// <c>0044C0C0</c>
    /// size 44 vtbl <c>0123D2EC</c>.
    /// Note-only + flag, not a live
    /// 44-byte object.
    /// </summary>
    public const uint SixtyNinthDefClassSite = 0x004F67BA;
    public const uint SixtyNinthDefClassFactory = 0x004D8EC5;
    public const uint SixtyNinthDefClassCtor = 0x0044C0C0;
    public const uint SixtyNinthDefClassVtbl = 0x0123D2EC;
    public const int SixtyNinthDefClassSize = 44;
    public const string SixtyNinthDefClassName = "CGoldDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CGoldDef</c>:
    /// <c>004F6946</c>
    /// <c>0044C6B0</c>
    /// <c>004F694D</c>
    /// <c>CAICreatureWillPowerIndicatorDef</c>
    /// factory <c>0x4D926A</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(44)</c>
    /// <c>0044C0C0</c>
    /// size 44 vtbl <c>0123DAA4</c>.
    /// Note-only + flag, not a live
    /// 44-byte object.
    /// </summary>
    public const uint SeventiethDefClassSite = 0x004F6946;
    public const uint SeventiethDefClassFactory = 0x004D926A;
    public const uint SeventiethDefClassCtor = 0x0044C0C0;
    public const uint SeventiethDefClassVtbl = 0x0123DAA4;
    public const int SeventiethDefClassSize = 44;
    public const string SeventiethDefClassName = "CAICreatureWillPowerIndicatorDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// that indicator:
    /// <c>004F6991</c>
    /// <c>0044C6B0</c>
    /// <c>004F6998</c>
    /// <c>CKickableDef</c>
    /// factory <c>0x4D7C2D</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(84)</c>
    /// <c>0044C0C0</c>
    /// size 84 vtbl <c>0123A7AC</c>.
    /// Note-only + flag, not a live
    /// 84-byte object.
    /// </summary>
    public const uint SeventyFirstDefClassSite = 0x004F6991;
    public const uint SeventyFirstDefClassFactory = 0x004D7C2D;
    public const uint SeventyFirstDefClassCtor = 0x0044C0C0;
    public const uint SeventyFirstDefClassVtbl = 0x0123A7AC;
    public const int SeventyFirstDefClassSize = 84;
    public const string SeventyFirstDefClassName = "CKickableDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CKickableDef</c>:
    /// <c>004F69DC</c>
    /// <c>0044C6B0</c>
    /// <c>004F69E3</c>
    /// <c>CTavernGameDef</c>
    /// factory <c>0x4E2D3B</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(420)</c>
    /// <c>jmp 004E1049</c>
    /// <c>0044C0C0</c>
    /// size 420 vtbl <c>012424BC</c>.
    /// Note-only + flag, not a live
    /// 420-byte object.
    /// </summary>
    public const uint SeventySecondDefClassSite = 0x004F69DC;
    public const uint SeventySecondDefClassFactory = 0x004E2D3B;
    public const uint SeventySecondDefClassCtor = 0x004E1049;
    public const uint SeventySecondDefClassVtbl = 0x012424BC;
    public const int SeventySecondDefClassSize = 420;
    public const string SeventySecondDefClassName = "CTavernGameDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CTavernGameDef</c>:
    /// <c>004F6A27</c>
    /// <c>0044C6B0</c>
    /// <c>004F6A2E</c>
    /// <c>CTavernGameCardBaseDef</c>
    /// factory <c>0x4E2DB2</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(132)</c>
    /// <c>jmp 004E1195</c>
    /// <c>0044C0C0</c>
    /// size 132 vtbl <c>0124258C</c>.
    /// Note-only + flag, not a live
    /// 132-byte object.
    /// </summary>
    public const uint SeventyThirdDefClassSite = 0x004F6A27;
    public const uint SeventyThirdDefClassFactory = 0x004E2DB2;
    public const uint SeventyThirdDefClassCtor = 0x004E1195;
    public const uint SeventyThirdDefClassVtbl = 0x0124258C;
    public const int SeventyThirdDefClassSize = 132;
    public const string SeventyThirdDefClassName = "CTavernGameCardBaseDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CTavernGameCardBaseDef</c>:
    /// <c>004F6A72</c>
    /// <c>0044C6B0</c>
    /// <c>004F6A79</c>
    /// <c>CTavernGameCoinBaseDef</c>
    /// factory <c>0x4D8F51</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(68)</c>
    /// <c>0044C0C0</c>
    /// size 68 vtbl <c>0123D44C</c>.
    /// Note-only + flag, not a live
    /// 68-byte object.
    /// </summary>
    public const uint SeventyFourthDefClassSite = 0x004F6A72;
    public const uint SeventyFourthDefClassFactory = 0x004D8F51;
    public const uint SeventyFourthDefClassCtor = 0x0044C0C0;
    public const uint SeventyFourthDefClassVtbl = 0x0123D44C;
    public const int SeventyFourthDefClassSize = 68;
    public const string SeventyFourthDefClassName = "CTavernGameCoinBaseDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CTavernGameCoinBaseDef</c>:
    /// <c>004F6B28</c>
    /// <c>0044C6B0</c>
    /// <c>004F6B2F</c>
    /// <c>CTavernGameShoveHaPennyDef</c>
    /// factory <c>0x4E2D70</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(512)</c>
    /// <c>jmp 004E1105</c>
    /// size 512 vtbl <c>01242524</c>.
    /// Note-only + flag, not a live
    /// 512-byte object.
    /// </summary>
    public const uint SeventyFifthDefClassSite = 0x004F6B28;
    public const uint SeventyFifthDefClassFactory = 0x004E2D70;
    public const uint SeventyFifthDefClassCtor = 0x004E1105;
    public const uint SeventyFifthDefClassVtbl = 0x01242524;
    public const int SeventyFifthDefClassSize = 512;
    public const string SeventyFifthDefClassName = "CTavernGameShoveHaPennyDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// shove ha'penny:
    /// <c>004F6BDE</c>
    /// <c>0044C6B0</c>
    /// <c>004F6BE5</c>
    /// <c>CTavernGameCoinGolfDef</c>
    /// factory <c>0x4D8F97</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(92)</c>
    /// <c>0044C0C0</c>
    /// size 92 vtbl <c>0123D4D4</c>.
    /// Note-only + flag, not a live
    /// 92-byte object.
    /// </summary>
    public const uint SeventySixthDefClassSite = 0x004F6BDE;
    public const uint SeventySixthDefClassFactory = 0x004D8F97;
    public const uint SeventySixthDefClassCtor = 0x0044C0C0;
    public const uint SeventySixthDefClassVtbl = 0x0123D4D4;
    public const int SeventySixthDefClassSize = 92;
    public const string SeventySixthDefClassName = "CTavernGameCoinGolfDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// coin golf:
    /// <c>004F6D6A</c>
    /// <c>0044C6B0</c>
    /// <c>004F6D71</c>
    /// <c>CTavernGameSpotTheAdditionDef</c>
    /// factory <c>0x4E11C3</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(144)</c>
    /// <c>jmp 004DED22</c>
    /// <c>0044C0C0</c>
    /// size 144 vtbl <c>01241A4C</c>.
    /// Note-only + flag, not a live
    /// 144-byte object.
    /// </summary>
    public const uint SeventySeventhDefClassSite = 0x004F6D6A;
    public const uint SeventySeventhDefClassFactory = 0x004E11C3;
    public const uint SeventySeventhDefClassCtor = 0x004DED22;
    public const uint SeventySeventhDefClassVtbl = 0x01241A4C;
    public const int SeventySeventhDefClassSize = 144;
    public const string SeventySeventhDefClassName = "CTavernGameSpotTheAdditionDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// spot-the-addition:
    /// <c>004F6EF6</c>
    /// <c>0044C6B0</c>
    /// <c>004F6EFD</c>
    /// <c>CDecapitationDef</c>
    /// factory <c>0x4D9047</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(48)</c>
    /// <c>0044C0C0</c>
    /// size 48 vtbl <c>0123D5E4</c>.
    /// Note-only + flag, not a live
    /// 48-byte object.
    /// </summary>
    public const uint SeventyEighthDefClassSite = 0x004F6EF6;
    public const uint SeventyEighthDefClassFactory = 0x004D9047;
    public const uint SeventyEighthDefClassCtor = 0x0044C0C0;
    public const uint SeventyEighthDefClassVtbl = 0x0123D5E4;
    public const int SeventyEighthDefClassSize = 48;
    public const string SeventyEighthDefClassName = "CDecapitationDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CDecapitationDef</c>:
    /// <c>004F6FAC</c>
    /// <c>0044C6B0</c>
    /// <c>004F6FB3</c>
    /// <c>CCoinGameObstacleDef</c>
    /// factory <c>0x4D8F0B</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(44)</c>
    /// <c>0044C0C0</c>
    /// size 44 vtbl <c>0123D3E4</c>.
    /// Note-only + flag, not a live
    /// 44-byte object.
    /// </summary>
    public const uint SeventyNinthDefClassSite = 0x004F6FAC;
    public const uint SeventyNinthDefClassFactory = 0x004D8F0B;
    public const uint SeventyNinthDefClassCtor = 0x0044C0C0;
    public const uint SeventyNinthDefClassVtbl = 0x0123D3E4;
    public const int SeventyNinthDefClassSize = 44;
    public const string SeventyNinthDefClassName = "CCoinGameObstacleDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CCoinGameObstacleDef</c>:
    /// <c>004F71EA</c>
    /// <c>0044C6B0</c>
    /// <c>004F71F1</c>
    /// <c>CWallMountEffectsDef</c>
    /// factory <c>0x4D90C6</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(52)</c>
    /// <c>0044C0C0</c>
    /// size 52 vtbl <c>0123D74C</c>.
    /// Note-only + flag, not a live
    /// 52-byte object.
    /// </summary>
    public const uint EightiethDefClassSite = 0x004F71EA;
    public const uint EightiethDefClassFactory = 0x004D90C6;
    public const uint EightiethDefClassCtor = 0x0044C0C0;
    public const uint EightiethDefClassVtbl = 0x0123D74C;
    public const int EightiethDefClassSize = 52;
    public const string EightiethDefClassName = "CWallMountEffectsDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CWallMountEffectsDef</c>:
    /// <c>004F7294</c>
    /// <c>0044C6B0</c>
    /// <c>004F729B</c>
    /// <c>CFishDef</c>
    /// factory <c>0x4D910C</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(88)</c>
    /// <c>0044C0C0</c>
    /// size 88 vtbl <c>0123D7BC</c>.
    /// Note-only + flag, not a live
    /// 88-byte object.
    /// </summary>
    public const uint EightyFirstDefClassSite = 0x004F7294;
    public const uint EightyFirstDefClassFactory = 0x004D910C;
    public const uint EightyFirstDefClassCtor = 0x0044C0C0;
    public const uint EightyFirstDefClassVtbl = 0x0123D7BC;
    public const int EightyFirstDefClassSize = 88;
    public const string EightyFirstDefClassName = "CFishDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CFishDef</c>:
    /// <c>004F733E</c>
    /// <c>0044C6B0</c>
    /// <c>004F7345</c>
    /// <c>CTeleporterDef</c>
    /// factory <c>0x4D9152</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(52)</c>
    /// <c>0044C0C0</c>
    /// size 52 vtbl <c>0123D834</c>.
    /// Note-only + flag, not a live
    /// 52-byte object. Not Guild
    /// transition.
    /// </summary>
    public const uint EightySecondDefClassSite = 0x004F733E;
    public const uint EightySecondDefClassFactory = 0x004D9152;
    public const uint EightySecondDefClassCtor = 0x0044C0C0;
    public const uint EightySecondDefClassVtbl = 0x0123D834;
    public const int EightySecondDefClassSize = 52;
    public const string EightySecondDefClassName = "CTeleporterDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CTeleporterDef</c>:
    /// <c>004F7443</c>
    /// <c>0044C6B0</c>
    /// <c>004F744A</c>
    /// <c>CExplosionDef</c>
    /// factory <c>0x4E3096</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(112)</c>
    /// <c>jmp 004E1341</c>
    /// <c>0044C0C0</c>
    /// size 112 vtbl <c>012425F4</c>.
    /// Note-only + flag, not a live
    /// 112-byte object.
    /// </summary>
    public const uint EightyThirdDefClassSite = 0x004F7443;
    public const uint EightyThirdDefClassFactory = 0x004E3096;
    public const uint EightyThirdDefClassCtor = 0x004E1341;
    public const uint EightyThirdDefClassVtbl = 0x012425F4;
    public const int EightyThirdDefClassSize = 112;
    public const string EightyThirdDefClassName = "CExplosionDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CExplosionDef</c>:
    /// <c>004F760A</c>
    /// <c>0044C6B0</c>
    /// <c>004F7611</c>
    /// <c>CResurrectionItemDef</c>
    /// factory <c>0x4D91DE</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(44)</c>
    /// <c>0044C0C0</c>
    /// size 44 vtbl <c>0123D9AC</c>.
    /// Note-only + flag, not a live
    /// 44-byte object.
    /// </summary>
    public const uint EightyFourthDefClassSite = 0x004F760A;
    public const uint EightyFourthDefClassFactory = 0x004D91DE;
    public const uint EightyFourthDefClassCtor = 0x0044C0C0;
    public const uint EightyFourthDefClassVtbl = 0x0123D9AC;
    public const int EightyFourthDefClassSize = 44;
    public const string EightyFourthDefClassName = "CResurrectionItemDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CResurrectionItemDef</c>:
    /// <c>004F76B4</c>
    /// <c>0044C6B0</c>
    /// <c>004F76BB</c>
    /// <c>CKrakenDef</c>
    /// factory <c>0x4E13AD</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(124)</c>
    /// <c>jmp 004DEF86</c>
    /// <c>0044C0C0</c>
    /// size 124 vtbl <c>01241B2C</c>.
    /// Note-only + flag, not a live
    /// 124-byte object.
    /// </summary>
    public const uint EightyFifthDefClassSite = 0x004F76B4;
    public const uint EightyFifthDefClassFactory = 0x004E13AD;
    public const uint EightyFifthDefClassCtor = 0x004DEF86;
    public const uint EightyFifthDefClassVtbl = 0x01241B2C;
    public const int EightyFifthDefClassSize = 124;
    public const string EightyFifthDefClassName = "CKrakenDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CKrakenDef</c>:
    /// <c>004F775E</c>
    /// <c>0044C6B0</c>
    /// <c>004F7765</c>
    /// <c>CKrakenTentacleDef</c>
    /// factory <c>0x4D9224</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(96)</c>
    /// <c>0044C0C0</c>
    /// size 96 vtbl <c>0123DA24</c>.
    /// Note-only + flag, not a live
    /// 96-byte object.
    /// </summary>
    public const uint EightySixthDefClassSite = 0x004F775E;
    public const uint EightySixthDefClassFactory = 0x004D9224;
    public const uint EightySixthDefClassCtor = 0x0044C0C0;
    public const uint EightySixthDefClassVtbl = 0x0123DA24;
    public const int EightySixthDefClassSize = 96;
    public const string EightySixthDefClassName = "CKrakenTentacleDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// tentacle:
    /// <c>004F7808</c>
    /// <c>0044C6B0</c>
    /// <c>004F780F</c>
    /// <c>CHeroSpecialMovementDef</c>
    /// factory <c>0x4D9198</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(56)</c>
    /// <c>0044C0C0</c>
    /// size 56 vtbl <c>0123D92C</c>.
    /// Note-only + flag, not a live
    /// 56-byte object.
    /// </summary>
    public const uint EightySeventhDefClassSite = 0x004F7808;
    public const uint EightySeventhDefClassFactory = 0x004D9198;
    public const uint EightySeventhDefClassCtor = 0x0044C0C0;
    public const uint EightySeventhDefClassVtbl = 0x0123D92C;
    public const int EightySeventhDefClassSize = 56;
    public const string EightySeventhDefClassName = "CHeroSpecialMovementDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// special movement:
    /// <c>004F7911</c>
    /// <c>0044C6B0</c>
    /// <c>004F7918</c>
    /// <c>CIdleSchedulerDef</c>
    /// factory <c>0x4E6232</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(72)</c>
    /// <c>jmp 004E3F21</c>
    /// <c>00430370</c>
    /// size 72 vtbl <c>01243124</c>.
    /// Note-only + flag, not a live
    /// 72-byte object.
    /// </summary>
    public const uint EightyEighthDefClassSite = 0x004F7911;
    public const uint EightyEighthDefClassFactory = 0x004E6232;
    public const uint EightyEighthDefClassCtor = 0x004E3F21;
    public const uint EightyEighthDefClassVtbl = 0x01243124;
    public const int EightyEighthDefClassSize = 72;
    public const string EightyEighthDefClassName = "CIdleSchedulerDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// idle scheduler:
    /// <c>004F79BB</c>
    /// <c>0044C6B0</c>
    /// <c>004F79C2</c>
    /// <c>CCarriedReadableDef</c>
    /// factory <c>0x4D92B0</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(48)</c>
    /// <c>jmp 004D5ECA</c>
    /// <c>0044C0C0</c>
    /// size 48 vtbl <c>0123DB94</c>.
    /// Note-only + flag, not a live
    /// 48-byte object.
    /// </summary>
    public const uint EightyNinthDefClassSite = 0x004F79BB;
    public const uint EightyNinthDefClassFactory = 0x004D92B0;
    public const uint EightyNinthDefClassCtor = 0x004D5ECA;
    public const uint EightyNinthDefClassVtbl = 0x0123DB94;
    public const int EightyNinthDefClassSize = 48;
    public const string EightyNinthDefClassName = "CCarriedReadableDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// carried readable:
    /// <c>004F7A65</c>
    /// <c>0044C6B0</c>
    /// <c>004F7A6C</c>
    /// <c>CJackOfBladesBattleDef</c>
    /// factory <c>0x4E4748</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(128)</c>
    /// <c>00430370</c>
    /// size 128 vtbl <c>01242E3C</c>.
    /// Note-only + flag, not a live
    /// 128-byte object.
    /// </summary>
    public const uint NinetiethDefClassSite = 0x004F7A65;
    public const uint NinetiethDefClassFactory = 0x004E4748;
    public const uint NinetiethDefClassCtor = 0x00430370;
    public const uint NinetiethDefClassVtbl = 0x01242E3C;
    public const int NinetiethDefClassSize = 128;
    public const string NinetiethDefClassName = "CJackOfBladesBattleDef";
    /// <summary>
    /// Zero-CTC battle cluster after Jack.
    /// All <c>00430370</c>. Registrar only.
    /// <c>CMazeBattleDef</c> is not the
    /// childhood Maze arrival.
    /// </summary>
    public const uint NinetyFirstDefClassSite = 0x004F7AB0;
    public const uint NinetyFirstDefClassFactory = 0x004E47BF;
    public const uint NinetyFirstDefClassCtor = 0x00430370;
    public const uint NinetyFirstDefClassVtbl = 0x01242EA4;
    public const int NinetyFirstDefClassSize = 96;
    public const string NinetyFirstDefClassName = "CScorpionKingBattleDef";
    public const uint NinetySecondDefClassSite = 0x004F7AFB;
    public const uint NinetySecondDefClassFactory = 0x004E4624;
    public const uint NinetySecondDefClassCtor = 0x00430370;
    public const uint NinetySecondDefClassVtbl = 0x01242D04;
    public const int NinetySecondDefClassSize = 76;
    public const string NinetySecondDefClassName = "CThunderBattleDef";
    public const uint NinetyThirdDefClassSite = 0x004F7B46;
    public const uint NinetyThirdDefClassFactory = 0x004E4667;
    public const uint NinetyThirdDefClassCtor = 0x00430370;
    public const uint NinetyThirdDefClassVtbl = 0x01242D6C;
    public const int NinetyThirdDefClassSize = 68;
    public const string NinetyThirdDefClassName = "CWhisperBattleDef";
    public const uint NinetyFourthDefClassSite = 0x004F7B91;
    public const uint NinetyFourthDefClassFactory = 0x004E46A4;
    public const uint NinetyFourthDefClassCtor = 0x00430370;
    public const uint NinetyFourthDefClassVtbl = 0x01242DD4;
    public const int NinetyFourthDefClassSize = 64;
    public const string NinetyFourthDefClassName = "CWaspQueenBattleDef";
    public const uint NinetyFifthDefClassSite = 0x004F7BDC;
    public const uint NinetyFifthDefClassFactory = 0x004E45CE;
    public const uint NinetyFifthDefClassCtor = 0x00430370;
    public const uint NinetyFifthDefClassVtbl = 0x01242C9C;
    public const int NinetyFifthDefClassSize = 96;
    public const string NinetyFifthDefClassName = "CMazeBattleDef";
    public const uint NinetySixthDefClassSite = 0x004F7C27;
    public const uint NinetySixthDefClassFactory = 0x004E4833;
    public const uint NinetySixthDefClassCtor = 0x00430370;
    public const uint NinetySixthDefClassVtbl = 0x01242F0C;
    public const int NinetySixthDefClassSize = 96;
    public const string NinetySixthDefClassName = "CTrollBattleDef";
    public const uint NinetySeventhDefClassSite = 0x004F7C72;
    public const uint NinetySeventhDefClassFactory = 0x004E4883;
    public const uint NinetySeventhDefClassCtor = 0x00430370;
    public const uint NinetySeventhDefClassVtbl = 0x01242F74;
    public const int NinetySeventhDefClassSize = 72;
    public const string NinetySeventhDefClassName = "CBalverineBattleDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CBalverineBattleDef</c> and one
    /// unnamed CTC: <c>004F7D1C</c>
    /// <c>0044C6B0</c>
    /// <c>004F7D23</c>
    /// <c>CAreaOfEffectAttackDef</c>
    /// factory <c>0x4E6CF3</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(76)</c>
    /// <c>00430370</c>
    /// size 76 vtbl <c>0124318C</c>.
    /// Note-only + flag, not a live
    /// 76-byte object. Registrar only.
    /// Does not
    /// <c>ActivateQuest</c>.
    /// </summary>
    public const uint NinetyEighthDefClassSite = 0x004F7D1C;
    public const uint NinetyEighthDefClassFactory = 0x004E6CF3;
    public const uint NinetyEighthDefClassCtor = 0x00430370;
    public const uint NinetyEighthDefClassVtbl = 0x0124318C;
    public const int NinetyEighthDefClassSize = 76;
    public const string NinetyEighthDefClassName = "CAreaOfEffectAttackDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CAreaOfEffectAttackDef</c>:
    /// <c>004F7DC6</c>
    /// <c>0044C6B0</c>
    /// <c>004F7DCD</c>
    /// <c>CFishingRodDef</c>
    /// factory <c>0x4D9321</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(60)</c>
    /// <c>0044C0C0</c>
    /// size 60 vtbl <c>0123DCA4</c>.
    /// Note-only + flag, not a live
    /// 60-byte object.
    /// </summary>
    public const uint NinetyNinthDefClassSite = 0x004F7DC6;
    public const uint NinetyNinthDefClassFactory = 0x004D9321;
    public const uint NinetyNinthDefClassCtor = 0x0044C0C0;
    public const uint NinetyNinthDefClassVtbl = 0x0123DCA4;
    public const int NinetyNinthDefClassSize = 60;
    public const string NinetyNinthDefClassName = "CFishingRodDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CFishingRodDef</c>:
    /// <c>004F7F2A</c>
    /// <c>0044C6B0</c>
    /// <c>004F7F31</c>
    /// <c>CRumbleDef</c>
    /// factory <c>0x4E3290</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(64)</c>
    /// <c>jmp 004E1722</c>
    /// <c>0044C0C0</c>
    /// size 64 vtbl <c>0124273C</c>.
    /// Note-only + flag, not a live
    /// 64-byte object.
    /// </summary>
    public const uint HundredthDefClassSite = 0x004F7F2A;
    public const uint HundredthDefClassFactory = 0x004E3290;
    public const uint HundredthDefClassCtor = 0x004E1722;
    public const uint HundredthDefClassVtbl = 0x0124273C;
    public const int HundredthDefClassSize = 64;
    public const string HundredthDefClassName = "CRumbleDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CRumbleDef</c>:
    /// <c>004F820A</c>
    /// <c>0044C6B0</c>
    /// <c>004F8211</c>
    /// <c>CShipDef</c>
    /// factory <c>0x4D8799</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(68)</c>
    /// <c>0044C0C0</c>
    /// size 68 vtbl <c>0123C0A4</c>.
    /// Note-only + flag, not a live
    /// 68-byte object.
    /// </summary>
    public const uint HundredFirstDefClassSite = 0x004F820A;
    public const uint HundredFirstDefClassFactory = 0x004D8799;
    public const uint HundredFirstDefClassCtor = 0x0044C0C0;
    public const uint HundredFirstDefClassVtbl = 0x0123C0A4;
    public const int HundredFirstDefClassSize = 68;
    public const string HundredFirstDefClassName = "CShipDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CShipDef</c>:
    /// <c>004F8255</c>
    /// <c>0044C6B0</c>
    /// <c>004F825C</c>
    /// <c>CShopItemDef</c>
    /// factory <c>0x4D8411</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(72)</c>
    /// <c>jmp 004D405A</c>
    /// <c>0044C0C0</c>
    /// size 72 vtbl <c>0123B644</c>.
    /// Note-only + flag, not a live
    /// 72-byte object.
    /// </summary>
    public const uint HundredSecondDefClassSite = 0x004F8255;
    public const uint HundredSecondDefClassFactory = 0x004D8411;
    public const uint HundredSecondDefClassCtor = 0x004D405A;
    public const uint HundredSecondDefClassVtbl = 0x0123B644;
    public const int HundredSecondDefClassSize = 72;
    public const string HundredSecondDefClassName = "CShopItemDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CShopItemDef</c>:
    /// <c>004F82A0</c>
    /// <c>0044C6B0</c>
    /// <c>004F82A7</c>
    /// <c>CSoundAtmospheresDef</c>
    /// factory <c>0x4E32E3</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(52)</c>
    /// <c>jmp 004E1748</c>
    /// <c>0044C0C0</c>
    /// size 52 vtbl <c>012427A4</c>.
    /// Note-only + flag, not a live
    /// 52-byte object. Not Oakvale
    /// atmosphere spawn.
    /// </summary>
    public const uint HundredThirdDefClassSite = 0x004F82A0;
    public const uint HundredThirdDefClassFactory = 0x004E32E3;
    public const uint HundredThirdDefClassCtor = 0x004E1748;
    public const uint HundredThirdDefClassVtbl = 0x012427A4;
    public const int HundredThirdDefClassSize = 52;
    public const string HundredThirdDefClassName = "CSoundAtmospheresDef";
    /// <summary>
    /// Next <c>009B0AC0</c> after
    /// <c>CSoundAtmospheresDef</c>:
    /// <c>004F8420</c>
    /// <c>0044C6B0</c>
    /// <c>004F8427</c>
    /// <c>CNymphDef</c>
    /// factory <c>0x4D93A0</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(80)</c>
    /// <c>0044C0C0</c>
    /// size 80 vtbl <c>0123DE3C</c>.
    /// Note-only + flag, not a live
    /// 80-byte object.
    /// </summary>
    public const uint HundredFourthDefClassSite = 0x004F8420;
    public const uint HundredFourthDefClassFactory = 0x004D93A0;
    public const uint HundredFourthDefClassCtor = 0x0044C0C0;
    public const uint HundredFourthDefClassVtbl = 0x0123DE3C;
    public const int HundredFourthDefClassSize = 80;
    public const string HundredFourthDefClassName = "CNymphDef";
    public const uint HundredFifthDefClassSite = 0x004F84D6;
    public const uint HundredFifthDefClassFactory = 0x004D93E6;
    public const uint HundredFifthDefClassCtor = 0x0044C0C0;
    public const uint HundredFifthDefClassVtbl = 0x0123DEB4;
    public const int HundredFifthDefClassSize = 76;
    public const string HundredFifthDefClassName = "CSummonDef";
    public const uint HundredSixthDefClassSite = 0x004F85F7;
    public const uint HundredSixthDefClassFactory = 0x004D9465;
    public const uint HundredSixthDefClassCtor = 0x0044C0C0;
    public const uint HundredSixthDefClassVtbl = 0x0123DFBC;
    public const int HundredSixthDefClassSize = 44;
    public const string HundredSixthDefClassName = "CCameraCollisionDef";
    public const uint HundredSeventhDefClassSite = 0x004F899A;
    public const uint HundredSeventhDefClassFactory = 0x004D96DD;
    public const uint HundredSeventhDefClassCtor = 0x0044C0C0;
    public const uint HundredSeventhDefClassVtbl = 0x0123E3B4;
    public const int HundredSeventhDefClassSize = 88;
    public const string HundredSeventhDefClassName = "CBettingDef";
    public const uint HundredEighthDefClassSite = 0x004F8B91;
    public const uint HundredEighthDefClassFactory = 0x004D97E9;
    public const uint HundredEighthDefClassCtor = 0x0044C0C0;
    public const uint HundredEighthDefClassVtbl = 0x0123E504;
    public const int HundredEighthDefClassSize = 92;
    public const string HundredEighthDefClassName = "COracleMinigameDef";
    public const uint HundredNinthDefClassSite = 0x004F8C47;
    public const uint HundredNinthDefClassFactory = 0x004D982F;
    public const uint HundredNinthDefClassCtor = 0x004D6638;
    public const uint HundredNinthDefClassVtbl = 0x0123E584;
    public const int HundredNinthDefClassSize = 60;
    public const string HundredNinthDefClassName = "CFireheartMinigameDef";
    public const uint HundredTenthDefClassSite = 0x004F8D68;
    public const uint HundredTenthDefClassFactory = 0x004D9882;
    public const uint HundredTenthDefClassCtor = 0x0044C0C0;
    public const uint HundredTenthDefClassVtbl = 0x0123E5EC;
    public const int HundredTenthDefClassSize = 60;
    public const string HundredTenthDefClassName = "CLightningOrbDef";
    /// <summary>
    /// Last <c>009B0AC0</c> on
    /// <c>004EE23F</c>:
    /// <c>004F8E89</c>
    /// <c>0044C6B0</c>
    /// <c>004F8E90</c>
    /// <c>CHasNameDef</c>
    /// factory <c>0x4D98C8</c>
    /// pack <c>0042DAE0</c>
    /// <c>00BFEA1A(52)</c>
    /// <c>0044C0C0</c>
    /// size 52 vtbl <c>0123E67C</c>.
    /// Six unnamed <c>004D2EF0</c> then
    /// <c>ret 004F9144</c>. Registrar
    /// only. Not
    /// <c>ActivateQuest</c>.
    /// </summary>
    public const uint HundredEleventhDefClassSite = 0x004F8E89;
    public const uint HundredEleventhDefClassFactory = 0x004D98C8;
    public const uint HundredEleventhDefClassCtor = 0x0044C0C0;
    public const uint HundredEleventhDefClassVtbl = 0x0123E67C;
    public const int HundredEleventhDefClassSize = 52;
    public const string HundredEleventhDefClassName = "CHasNameDef";
    /// <summary>
    /// Only <c>E8</c> of <c>0073B130</c>
    /// is <c>004F9129</c> after
    /// <c>CHasNameDef</c>. Unrolled
    /// 8-byte <c>{tag, thunk}</c>
    /// bump vs limit
    /// <c>0x13BAD4C</c>. Grow
    /// <c>00743270</c>; second family
    /// <c>00743B30</c>; table commit
    /// <c>007441D0</c>; <c>ret
    /// 0073CB40</c>. First record tag
    /// 0 thunk <c>00742430</c>.
    /// Global bump, not the
    /// <c>esi</c> def map. Not
    /// <c>ActivateQuest</c>.
    /// </summary>
    public const uint ThingComponentsFillSite = 0x004F9129;
    public const uint ThingComponentsFillFn = 0x0073B130;
    public const uint ThingComponentsFillRet = 0x0073CB40;
    public const uint ThingComponentsFillLimitVa = 0x013BAD4C;
    public const uint ThingComponentsFillGrowFn = 0x00743270;
    public const uint ThingComponentsFillSecondFn = 0x00743B30;
    public const uint ThingComponentsFillCommitFn = 0x007441D0;
    public const uint ThingComponentsFillFirstThunk = 0x00742430;
    public const int ThingComponentsFillFirstTag = 0;
    public const uint ThingComponentsFillFirstCtor = 0x0073EAC0;
    public const uint ThingComponentsFillFirstVtbl = 0x01267588;
    public const int ThingComponentsFillFirstSize = 28;
    public const int ThingComponentsTailCtcCount = 6;
    public const uint ThingComponentsTailCtcFirstSite = 0x004F8ECE;
    public const uint ThingComponentsTailCtcFirstFactory = 0x004D66DA;
    public const uint ThingComponentsTailCtcLastSite = 0x004F90E1;
    public const uint ThingComponentsTailCtcLastFactory = 0x004DAF85;
    public const bool ThingComponentsFillIsQuestActivate = false;
    /// <summary>
    /// <c>004F9112</c> stores 1 into
    /// the IAT-adjacent flag immediately
    /// before <c>0073B130</c>, so
    /// <c>004F9139 004EBACE</c> runs
    /// on this walk (<c>ecx=esi</c>
    /// map). <c>004EBACE</c> reads
    /// <c>[esi]</c> / <c>[esi+4]</c> /
    /// <c>[esi+12]</c> u8, calls
    /// <c>004EB9A6</c>, zeros
    /// <c>[esi+13]</c>. Not
    /// <c>ActivateQuest</c>.
    /// </summary>
    public const uint ThingComponentsCommitFlagSetSite = 0x004F9112;
    public const uint ThingComponentsCommitSite = 0x004F9139;
    public const uint ThingComponentsCommitFn = 0x004EBACE;
    public const uint ThingComponentsCommitApplyFn = 0x004EB9A6;
    public const int ThingComponentsCommitPlus12 = 12;
    public const int ThingComponentsCommitPlus13 = 13;
    public const bool ThingComponentsCommitRunsThisWalk = true;
    public const uint ThingComponentsRet = 0x004F9144;
    /// <summary>
    /// <c>00416005</c> parent
    /// <c>push 1</c>:
    /// <c>0044C6B0</c>
    /// <c>[vtbl+8]=0044C72B</c>
    /// <c>009ACB10</c>
    /// <c>[[this+88]]</c>
    /// <c>009E5250</c>.
    /// Not a game.bin parse.
    /// </summary>
    public const uint InitDefinitionManagerFn = 0x00416005;
    public const uint DefinitionManagerVtbl8Fn = 0x0044C72B;
    public const int DefinitionManagerVtbl8 = 8;
    public const uint DefinitionManagerResetFn = 0x009ACB10;
    public const uint DefinitionManagerResetApply = 0x009E5250;
    public const int DefinitionManagerPlus88 = 88;
    public const int DefinitionManagerArg = 1;
    /// <summary>
    /// <c>0044C72B</c> Compile after the
    /// path list. Opens the compiled-def
    /// analog (<c>009B08C0</c>). Host
    /// Notes the open. Does not parse
    /// <c>game.bin</c>.
    /// </summary>
    public const uint DefinitionManagerCompileFn = 0x009B08C0;
    public const uint DefinitionManagerPathPrefixVa = 0x0122DAA4;
    public const string DefinitionManagerPathPrefix = "pc\\";
    public const uint DefinitionManagerFirstGlobVa = 0x01236094;
    public const string DefinitionManagerFirstGlob = "*.h";
    public const string DefinitionManagerCompileFirstClass = "CHeroPostcardGeneratorDef";
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
    /// <summary>
    /// <c>009E1BC0</c>. IAT
    /// <c>0x143FE00</c> is
    /// <c>KERNEL32!QueryPerformanceCounter</c>;
    /// <c>0x143FE04</c> is
    /// <c>QueryPerformanceFrequency</c>.
    /// </summary>
    public const uint FrameDtFn = 0x009E1BC0;
    public const uint FrameDtQpcIat = 0x0143FE00;
    public const uint FrameDtQpfIat = 0x0143FE04;
    public const uint GamePumpUpdate = 0x004162B5;
    public const uint GamePumpMemlog = 0x00415E85;
    /// <summary>
    /// After <c>004162B5</c>:
    /// <c>00416202</c> → <c>0049B9E0</c>
    /// on the <c>0049BA70</c> ring at
    /// <c>game+90488</c> (capacity 60,
    /// float stride 4, mean at +40).
    /// Then <c>00415E85</c>:
    /// <c>[0x13B85F1]==0</c> (no writer)
    /// skips the memlog body.
    /// Then <c>0044C6B0</c> /
    /// <c>009AC9E0 ret 4</c>.
    /// </summary>
    public const uint FrameDtRingFn = 0x00416202;
    public const uint FrameDtRingPushFn = 0x0049B9E0;
    public const uint FrameDtRingMeanFn = 0x0049B9A0;
    public const int FrameDtRingMeanOffset = 40;
    public const uint MemlogFlagVa = 0x013B85F1;
    public const int MemlogFlagFirstSeen = 0;
    public const uint PlayerManagerIdleFn = 0x009AC9E0;
    public const uint GamePumpQuitQuery = 0x009A6460;
    public const uint GamePumpInnerStartFn = 0x0098E1B0;
    public const int GamePumpQuitLeave = 2;
    public const int GamePumpQuitUpdate = 1;
    public const int GamePumpQuitFirstSeen = 1;
    /// <summary>
    /// <c>009A6460</c> always calls
    /// <c>009A6370</c> (PeekMessage
    /// <c>009A4F20</c>, then
    /// <c>009C00C0</c>). Return 2 iff
    /// <c>[engine+8]!=0</c>. That byte is
    /// written by WndProc <c>009A5B60</c>
    /// <c>WM_DESTROY</c> (table
    /// <c>0x9A5F7C[1]=009A5BEA</c>).
    /// First-seen New Game does not
    /// destroy the window, so return 1
    /// and loop. Not <c>00501450</c>.
    /// </summary>
    public const uint EngineMessagePumpFn = 0x009A6370;
    public const uint PeekMessageFn = 0x009A4F20;
    public const uint PeekMessageIat = 0x01440370;
    public const uint DefWindowProcIat = 0x0144037C;
    public const uint TestCooperativeLevelFn = 0x009C00C0;
    /// <summary>
    /// After empty PeekMessage:
    /// <c>009F4E20([engine+88], [engine+9])</c>.
    /// <c>00403079</c> <c>[opt+20]=5</c>
    /// when <c>[0x1375449]==0</c> (no
    /// writer) so bit 0x01 creates
    /// input and bit 0x10 does not
    /// create <c>+124</c>.
    /// </summary>
    public const uint InputFocusFn = 0x009F4E20;
    public const int EnginePlus88Offset = 88;
    public const int EnginePlus124Offset = 124;
    public const int EnginePlus9Offset = 9;
    public const int EnginePlus9AfterSetup = 1;
    public const uint EngineOptionsFlagVa = 0x01375449;
    public const int EngineOptionsFlagFirstSeen = 0;
    public const int EngineOptionsFlagsOffset = 20;
    public const int EngineOptionsFlagsFirstSeen = 5;
    public const int EngineOptionsInputBit = 0x01;
    public const int EngineOptionsSoundBit = 0x10;
    public const uint CreateInputFn = 0x00A60050;
    public const uint StoreInputFn = 0x009A7180;
    public const uint EngineWndProc = 0x009A5B60;
    public const uint EngineWndProcJumpTable = 0x009A5F7C;
    public const uint EngineQuitStoreSite = 0x009A5BEA;
    public const int WmDestroy = 2;
    public const int EnginePlus8Offset = 8;
    public const int EnginePlus8FirstSeen = 0;
    public const int GamePlus8Offset = 8;
    public const int GamePlus8FirstSeen = 0;
    public const uint GamePumpLeaveFn = 0x004175E5;
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
    /// <summary>
    /// Display <c>012A0F3C+208</c>.
    /// <c>mov ecx,[0x1436E8C]; jmp 00B428E0</c>.
    /// No <c>E8</c> to <c>00B428E0</c>.
    /// </summary>
    public const int DisplayEngineSetStaticMapVtbl = 208;
    public const uint DisplayEngineSetStaticMapThunk = 0x00B23DC0;
    public const uint MapManagerGlobalVa = 0x01436E8C;
    public const uint DeriveStaticMapNameFn = 0x0049DDD0;
    public const uint DeriveStaticMapNameSite = 0x004A18FC;
    public const uint SetStaticMapVtblCallSite = 0x004A1BD3;
    public const uint StaticMapLevelsDirVa = 0x0122F3B4;
    public const string StaticMapLevelsDir = @"Data\Levels\";
    public const uint StaticMapStbSuffixVa = 0x01238BAC;
    public const string StaticMapStbSuffix = ".stb";
    public const uint StaticMapRtStbSuffixVa = 0x01238BC8;
    public const string StaticMapRtStbSuffix = "_RT.stb";
    public const uint RetailStbFlagVa = 0x013B8616;
    public const int RetailStbFlagFirstSeen = 0;
    public const uint LoadWaterDataFn = 0x00B41FA0;
    public const uint InputLockObjectVa = 0x013CAA90;
    public const uint InputLockEnterFn = 0x009F2660;
    public const uint InputLockLeaveFn = 0x009F26B0;
    /// <summary>
    /// <c>009A57B0</c>:
    /// <c>GetForegroundWindow() == [engine+148]</c>
    /// (HWND from <c>CreateWindowExW</c>).
    /// IAT <c>0x1440378</c> is
    /// <c>USER32!GetForegroundWindow</c>,
    /// not GetTickCount. False skips
    /// vtbl+20 / vtbl+28.
    /// First-seen after AVI: window
    /// exists and is foreground → 1.
    /// </summary>
    public const uint EngineUpdateGateFn = 0x009A57B0;
    public const int EngineTickOffset = 148;
    public const int EngineHwndOffset = 148;
    public const uint GetForegroundWindowIat = 0x01440378;
    public const uint GetTickCountIat = 0x01440378;
    public const uint CreateWindowExIat = 0x01440388;
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
    /// <summary>
    /// <c>00413C50</c> (when
    /// <c>[0x137548F]!=0</c>, PE 1):
    /// register commands, then
    /// <c>00414C10</c> <c>default_userst.ini</c>
    /// if present, then <c>00414C66</c>
    /// <c>userst.ini</c> when
    /// <c>[0x1375444]!=0</c> (PE 1).
    /// Same <c>009EC890</c> walker as
    /// <c>user.ini</c>. Runs before
    /// <c>00403079</c>.
    /// </summary>
    public const uint UserstRegisterFn = 0x00413C50;
    public const uint UserstApplyFn = 0x00414C66;
    public const uint UserstIniVa = 0x0122E674;
    public const uint DefaultUserstIniVa = 0x0122E68C;
    public const uint UserstApplyFlagVa = 0x01375444;
    public const byte UserstApplyFlagFirstSeen = 1;
    public const uint UserstGateVa = 0x0137548F;
    public const byte UserstGateFirstSeen = 1;
    public const string UserstIniName = "userst.ini";
    public const string DefaultUserstIniName = "default_userst.ini";
    public const string IniSetFullscreenName = "SetFullscreen";
    public const string IniSetResolutionName = "SetResolution";
    public const uint FileExistsFn = 0x00999230;
    public const uint IniApplyFn = 0x009EC890;
    public const uint IniTokenizeFn = 0x009EC710;
    public const uint IniDispatchFn = 0x009EB430;
    public const uint IniUnknownFn = 0x009EB260;
    public const uint IniRunScriptFn = 0x009ECB70;
    public const string IniRunScriptName = "RunScript";
    public const string IniRunScriptArg = "joystick.ini";
    public const string IniRunScriptSuffix = ".ini";
    public const uint IniActivateQuestThunk = 0x00419CE0;
    public const uint IniActivateQuestGate = 0x004197B0;
    public const uint IniActivateQuestRegister = 0x00419D90;
    public const uint IniConsoleCommandsFn = 0x009ED190;
    public const uint EngineReadyCallback = 0x004167DA;
    public const int EngineReadyCallbackOffset = 240;
    public const int EngineGamePtrOffset = 244;
    public const uint StartupWadSite = 0x004A19EB;
    public const uint ExtraWadFlagVa = 0x01375456;
    public const int ExtraWadFlagFirstSeen = 0;
    public const uint WorldMapOpenBankFn = 0x004FDAB0;
    public const uint EmptyCStringVa = 0x0122D70C;
    public const uint DeriveQuestPathFn = 0x0049D770;
    public const uint QuestSuffixVa = 0x01238C40;
    public const string QuestSuffix = ".qst";
    public const uint GlobalQuestsVa = 0x01238F38;
    public const string GlobalQuestsName = @"Data\Levels\GlobalQuests.qst";
    public const uint GenerateOfflineDataSite = 0x004A1AF8;
    public const uint GenerateOfflineDataFlagVa = 0x01375446;
    public const int GenerateOfflineDataFlagFirstSeen = 0;
    public const uint SetStaticMapForEngineSite = 0x004A1B7D;
    public const uint AttachPatchFn = 0x00BDF010;
    public const uint GameModeCtorRenderEnable = 0x00418EC6;
    public const int GameRenderEnableOffset = 90593;
    public const uint FrontEndQueryFn = 0x00416296;
    public const uint GuiBlockQueryFn = 0x00490A22;
    public const uint FadeApplyFn = 0x0041649C;
    public const uint PlayerActionFn = 0x004AEAA0;
    public const int PlayerActionFlagOffset = 9826;
    /// <summary>
    /// <c>0041674A</c> (<c>004AEAA0</c>
    /// arg <c>+9836</c> = <c>[game+72]</c>
    /// from <c>004AE9D0</c>).
    /// <c>[game+9]==1</c> after
    /// <c>004189C2</c>. BSS
    /// <c>0x13B8688</c> has no <c>.text</c>
    /// writer (two <c>cmp</c> only) so
    /// first-seen takes the dt path:
    /// <c>004166E2</c> is
    /// <c>009E1BC0-[game+96]</c>
    /// (first inner 0); then
    /// <c>*15 − +9836</c>
    /// <c>fcomp 1.0</c>; <c>&lt;=</c> → 0.
    /// <c>004162B5</c> does not call
    /// vtbl+24; <c>00416E78</c> runs only
    /// when this returns 1.
    /// </summary>
    public const uint PlayerCatchupFn = 0x0041674A;
    public const uint PlayerCatchupTimeFn = 0x004166E2;
    /// <summary>
    /// <c>004166E2</c>
    /// <c>cmp [0x13B86A4]</c>.
    /// No <c>.text</c> writer
    /// (one <c>cmp</c>) so
    /// first-seen keeps the
    /// <c>009E1BC0</c> clamp.
    /// </summary>
    public const uint DisplayClockForceQpcVa = 0x013B86A4;
    public const int DisplayClockForceQpcFirstSeen = 0;
    public const int GamePlus9Offset = 9;
    public const int GamePlus9FirstSeen = 1;
    public const uint PlayerCatchupForceVa = 0x013B8688;
    public const int PlayerCatchupForceFirstSeen = 0;
    public const uint PlayerCatchupMenuVa = 0x013B860C;
    public const int PlayerCatchupMenuFirstSeen = 0;
    public const uint PlayerCatchupCutsceneVa = 0x013B8629;
    public const int PlayerCatchupCutsceneFirstSeen = 0;
    /// <summary>
    /// <c>004AEAA0</c> on hit:
    /// <c>inc [esi+9836]</c>,
    /// <c>009F1720</c> zeros
    /// <c>[game+164]</c>,
    /// <c>009F16F0</c> copies
    /// <c>0x192</c> dwords from
    /// player+8208, count=1.
    /// Record+0 is +9836 after inc.
    /// Sub[+0]=1 from
    /// <c>[esp+20]=1</c>.
    /// <c>009F16C0</c> clears builder+4.
    /// </summary>
    public const uint TickListAppendFn = 0x009F16F0;
    public const uint TickListClearFn = 0x009F1720;
    public const uint TickBuilderResetFn = 0x009F16C0;
    public const uint TickListCountFn = 0x009F1750;
    public const uint TickListAtFn = 0x009F1730;
    public const uint TickSubCountFn = 0x009F16E0;
    public const uint TickSubAtFn = 0x009F16D0;
    public const uint PlayerBindIncSite = 0x004AEB3D;
    public const int TickListStride = 0x648;
    public const int TickListCopyDwords = 0x192;
    public const int PlayerTickBuilderOffset = 8208;
    public const int TickSubRecordSize = 40;
    public const uint GameVtbl24Fn = 0x00416E78;
    public const int GameVtbl24 = 24;
    /// <summary>
    /// <c>00416E78</c> prefix before
    /// <c>WorldFrame&gt;1</c>:
    /// <c>[world+52].vtbl+4</c>,
    /// <c>00416392</c>, <c>009F4A90</c>
    /// writes <c>[0x13B8388]+60</c> (16
    /// bytes) and <c>+92=[game+72]</c>,
    /// then input <c>vtbl+8</c>.
    /// </summary>
    public const int WorldPlus52Offset = 52;
    public const int WorldPlus52Vtbl = 4;
    public const uint InputStoreRecordFn = 0x009F4A90;
    public const int InputRecordOffset = 60;
    public const int InputRecordSize = 16;
    public const int InputGamePlus72Offset = 92;
    public const int InputVtbl8 = 8;
    public const uint ClearGamePlus68Fn = 0x00416047;
    public const uint WorldFrameGetter = 0x0049D870;
    public const uint WorldFrameVa = 0x013B89BC;
    public const uint WorldFrameCopyVa = 0x013B7D70;
    /// <summary>
    /// After <c>009A57B0</c> allow:
    /// <c>and [0x13B89A8],0</c>, then
    /// vtbl+20 / vtbl+28 dt at
    /// <c>0x13B8690</c> / <c>0x13B8698</c>.
    /// <c>00417001</c> always ends
    /// <c>[0x13B7D6C]=[display+104]</c>
    /// even if <c>WorldFrame&lt;=1</c>.
    /// <c>004350D0</c> writes
    /// <c>[display+104]=0</c>.
    /// </summary>
    public const uint FrameListCountVa = 0x013B89A8;
    public const uint UpdateDtVa = 0x013B8690;
    public const uint RenderDtVa = 0x013B8698;
    public const uint DisplayPlus104CopyVa = 0x013B7D6C;
    public const int DisplayPlus104Offset = 104;
    public const int DisplayPlus104FirstSeen = 0;
    public const uint WorldGetThingFn = 0x0049E1B0;
    public const int WorldThingOffset = 80;
    public const uint StoreActiveThingFn = 0x004C74F0;
    public const uint ActiveThingVa = 0x013B8A1C;
    public const uint RenderStackZeroFn = 0x00415A60;
    public const uint SleepIat = 0x0143FE1C;
    /// <summary>
    /// Inner <c>004189C2</c> before
    /// <c>004162B5</c> when
    /// <c>[game+52]==0</c>:
    /// <c>009F8BA0(game+90556)</c>.
    /// IAT <c>0x14404B4</c> tick, store
    /// delta at <c>+4</c>.
    /// </summary>
    public const uint InnerLoopDtFn = 0x009F8BA0;
    public const uint InnerLoopDtIat = 0x014404B4;
    public const int InnerLoopDtOffset = 90556;
    public const int GamePlus52Offset = 52;
    public const int GamePlus52FirstSeen = 0;
    public const uint SleepMsVa = 0x013B8610;
    /// <summary>
    /// Unique increment: <c>004A5E10 inc [0x13B89BC]</c>
    /// at the end of world tick <c>004A5A40</c>.
    /// <c>imm 0x13B89BC</c> is 10 sites;
    /// the others are reads or the
    /// <c>"WorldFrame"</c> string.
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
    /// <c>004B4490</c>. First-seen
    /// <c>[0x1375454]==1</c>
    /// (<c>.data</c> <c>0x01</c>;
    /// one <c>.text</c> cmp).
    /// <c>004B3CE0</c> constructed
    /// at <c>004B4260</c>.
    /// <c>00CB8220</c> type-1
    /// body UNREAD. Host walk of
    /// <c>00CB7950</c> /
    /// <c>Runtime.Update</c> is leftover.
    /// </summary>
    public const uint QuestManagerPumpFn = 0x004B4490;
    public const uint QuestManagerVa = 0x013B89FC;
    /// <summary>
    /// <c>004B2850</c> <c>AddQuest</c>
    /// push onto <c>[0x13B89FC]+44</c>.
    /// Every name, TRUE and FALSE.
    /// Not <c>004A08D0</c>-cleared.
    /// </summary>
    public const uint QuestManagerPushFn = 0x004B2850;
    public const int QuestManagerPlus44Offset = 44;
    /// <summary>
    /// <c>004B4260</c> membership gate
    /// before <c>00CB5AD0</c>. Finds
    /// the name in QM+44.
    /// </summary>
    public const uint QuestActivateGateFn = 0x004B00C0;
    public const uint QuestListPumpFn = 0x00CB8220;
    public const uint QuestListWalkAFn = 0x00CB7C40;
    public const uint QuestListWalkBFn = 0x00CB8170;
    public const uint QuestFiberAttachFn = 0x00CB7950;
    /// <summary>
    /// <c>004B3CE0</c>
    /// <c>cmp [0x1375454]</c>.
    /// One <c>.text</c> imm (the cmp).
    /// <c>.data</c> at that VA is
    /// <c>0x01</c> (dword
    /// <c>0x01010101</c>) so
    /// first-seen constructs.
    /// BSS-0 stub is DISPROVEN.
    /// </summary>
    public const uint QuestFactoryGateVa = 0x01375454;
    public const int QuestFactoryGateFirstSeen = 1;
    public const uint QuestFiberUpdateVtbl = 24;
    public const int QuestFiberUpdateFlagOffset = 41;
    /// <summary>
    /// <c>00CB78D0</c> writes <c>+41</c>.
    /// First-seen <c>+41=0</c> takes
    /// <c>00CB7997</c> <c>vtbl+4</c>
    /// <c>00A44880</c>. Host “skip
    /// <c>00A44880</c>” is DISPROVEN.
    /// </summary>
    public const uint FiberUpdateFlagSetter = 0x00CB78D0;
    public const uint FiberTickFn = 0x00A44880;
    public const uint FiberResumeFn = 0x00A44660;
    public const uint FiberEntryFn = 0x00A446A0;
    /// <summary>
    /// <c>00A446A0</c> calls watcher
    /// <c>+16</c> then loops
    /// <c>[vtbl+8]</c> until <c>+5</c>.
    /// Persist bind <c>00DAADA0</c> and
    /// run <c>00DABAC0</c> are not the
    /// same <c>this</c> as that fiber
    /// entry. PARITY “fiber calls persist
    /// then run” is DISPROVEN.
    /// </summary>
    public const bool FiberCallsPersistThenRun = false;
    /// <summary>
    /// After yield, resume is
    /// <c>00A44921</c> <c>00A44660</c>
    /// → <c>009D87F0([fiber+16])</c>.
    /// It does not re-enter
    /// <c>00DABAC0</c> via
    /// <c>[S_QNOVI.vtbl+8]</c>. First
    /// enter of slot 2 is
    /// <c>00DAAD76</c> → <c>00CDD440</c>
    /// <c>jmp [eax+8]</c>.
    /// </summary>
    public const bool SqnoviReentersRunAfterYield = false;
    public const uint SqnoviMainWatcherThunk = 0x00CDD440;
    public const uint FiberYieldFn = 0x009D8650;
    public const uint WatcherRunFn = 0x00CE7640;
    public const uint GameflowTickFn = 0x00CE7670;
    public const uint GameflowState0Fn = 0x00CE77D7;
    public const uint GameflowYieldThunk = 0x006E7410;
    public const uint WatcherYieldVtbl8 = 0x00A44840;
    /// <summary>
    /// <c>00CE7670</c> wait is
    /// <c>[esi+64].vtbl+100</c>
    /// slot 25 <c>00893570</c> (name
    /// compare via type-<c>0x33</c>
    /// <c>008ABED0</c>). Sibling
    /// <c>00893610</c> is slot 26
    /// <c>vtbl+104</c> GET copy-out.
    /// First-seen both miss.
    /// </summary>
    public const uint QuestIsActiveFn = 0x00893570;
    public const int QuestIsActiveVtbl = 100;
    public const uint QuestIsGetFn = 0x00893610;
    public const int QuestIsGetVtbl = 104;
    /// <summary>
    /// <c>01260F0C</c> slot 288
    /// <c>vtbl+1152</c> <c>00892F80</c>
    /// → <c>004B1D30</c> posts kind
    /// <c>0x33</c> on <c>[world+96]</c>.
    /// Give / visible, not construct
    /// (<c>004B3CE0</c> posts
    /// <c>0x37</c>). <c>00DBDE40</c>
    /// calls this only after
    /// <c>AttackOver</c>.
    /// </summary>
    public const uint QuestGiveFn = 0x00892F80;
    public const int QuestGiveVtbl = 1152;
    public const uint QuestGiveBody = 0x004B1D30;
    public const int QuestGiveEventKind = 0x33;
    public const int QuestConstructEventKind = 0x37;
    /// <summary>
    /// <c>005E7B77</c> in
    /// <c>CTCQuestCompletionUI</c>
    /// tick <c>005E78F0</c>. Mode 1
    /// second Give after
    /// <c>004B1D30</c> already posted
    /// <c>0x33</c>. Empty <c>+80</c>
    /// skips. Not no-save first-seen.
    /// </summary>
    public const uint QuestCompletionUiGiveFn = 0x005E7B77;
    public const bool QuestCompletionUiGiveIsFirstSeen = false;
    public const uint QuestGiveAfterAttackOver = 0x00DBE295;
    /// <summary>
    /// Gameflow state 0 waits
    /// <c>vtbl+100</c> for type-<c>0x33</c>
    /// Give of <c>Q_NewOakValeIntro</c>
    /// on <c>[world+96]</c>. No-save
    /// never posts that Give. Construct
    /// posts <c>0x37</c> and cannot
    /// satisfy the wait. Later Give is
    /// <c>00DBE295</c> after AttackOver.
    /// Do not invent
    /// <c>ActivateQuest</c> to leave
    /// the yield.
    /// </summary>
    public const bool GameflowWaitsForeverOnNoSave = true;
    public const bool ActivateQuestSatisfiesGameflowWait = false;
    /// <summary>
    /// <c>CTCExpression</c> is 20 bytes
    /// (<c>004DC7E8</c>). Offset
    /// <c>+120</c> is on nested
    /// <c>EXPRESSION</c> (factory
    /// <c>0045D70B</c> size <c>0x90</c>).
    /// First CString intern at that
    /// slot is persist compile
    /// <c>00456A54</c> <c>0045228F</c>,
    /// before New Game. Tick
    /// <c>007EF200</c> reads then
    /// <c>004B4A10</c>; it does not
    /// store <c>Q_NewOakValeIntro</c>
    /// intern <c>0x012C5D14</c>.
    /// </summary>
    public const uint CtcExpressionFactory = 0x004DC7E8;
    public const int CtcExpressionSize = 20;
    public const uint ExpressionFactory = 0x0045D70B;
    public const int ExpressionSize = 0x90;
    public const uint ExpressionPlus120Persist = 0x00456A54;
    public const uint ExpressionPlus120Call = 0x00456A5A;
    public const uint ExpressionPersistFn = 0x004569A7;
    /// <summary>
    /// File payload at compiled
    /// <c>EXPRESSION+120</c> is a
    /// names.bin offset or <c>-1</c>,
    /// CRC <c>0x1FB35A1B</c> (same dword
    /// as <c>CActivateQuestDef+40</c>).
    /// Not CString helper
    /// <c>00431143</c>. 36 of 39 rows
    /// are <c>-1</c>; three are
    /// <c>Expression_Pickpocket</c> /
    /// <c>Picklock</c> / <c>Steal</c>.
    /// Never <c>Q_NewOakValeIntro</c>.
    /// </summary>
    public const uint ExpressionPlus120Crc = 0x1FB35A1B;
    public const int ExpressionPlus120EmptySentinel = -1;
    public const bool ExpressionPlus120IsOakvaleIntern = false;
    public const uint ExpressionTickFn = 0x007EF200;
    public const bool ExpressionTickWritesOakvaleIntern = false;
    public const uint OakvaleQuestIntern = 0x012C5D14;
    /// <summary>
    /// Live <c>FableCrc</c> of
    /// <c>Q_NewOakValeIntro</c>. Not
    /// the PE intern. Not in
    /// <c>CActivateQuestDef</c> 16-byte
    /// payloads. <c>names.bin</c> has
    /// no quest row.
    /// </summary>
    public const uint OakvaleQuestFableCrc = 0x8D19C362;
    public const bool CActivateQuestDefInternsOakvale = false;
    public const bool CActivateQuestDefInOakvaleTng = false;
    /// <summary>
    /// <c>00430340</c> <c>[retail+8]=1</c>
    /// does not change Init Quests.
    /// <c>0049F24E</c> always
    /// <c>lea edx,[esi+172]</c>.
    /// </summary>
    public const bool RetailPlus8ChangesInitQuests = false;
    /// <summary>
    /// Gameflow state 0 binds the Oakvale
    /// card via <c>vtbl+1180</c>
    /// <c>00CE7957</c>
    /// <c>call [edx+1180]</c> on
    /// <c>[esi+64]</c> iface
    /// <c>01260F0C</c> dest
    /// <c>008968C0</c> (<c>ret 12</c>).
    /// Sibling <c>00896A30</c> is slot
    /// 296 (<c>+1184</c>, <c>ret 16</c>),
    /// not this call. Not
    /// <c>00CB5AD0</c>.
    /// </summary>
    public const uint QuestCardBindFn = 0x008968C0;
    public const int QuestCardBindVtbl = 1180;
    public const uint QuestCardBindSiblingFn = 0x00896A30;
    public const int QuestCardBindSiblingVtbl = 1184;
    public const uint GiveNamedObjectFn = 0x008902E0;
    public const uint PlayAviFlagFn = 0x00408340;
    public const uint StoryLogFn = 0x00CBE87F;
    public const int StoryLogFirstSeen = 10;
    public const string GameflowWaitQuest = "Q_NewOakValeIntro";
    public const string GameflowWaitCard = "OBJECT_QUEST_CARD_OAKVALE_INTRO";
    public const string WatcherMain = "Main";
    public const string WatcherCoreReminder = "CoreQuestReminder";
    public const string WatcherBarrowGuards = "CheckBarrowFieldsGuards";
    public const uint CoreReminderFn = 0x00CEF3B0;
    public const uint BarrowGuardsFn = 0x00CEF550;
    public const uint QuestThingHasFn = 0x00892F60;
    public const uint QuestNameActiveFn = 0x00892F40;
    public const uint QuestThingHasBody = 0x004B0FC0;
    public const uint QuestNameActiveBody = 0x004AF610;
    public const string TraderConflictEvil = "Q_TraderConflictEvil";
    public const string TraderConflictGood = "Q_TraderConflictGood";
    public const uint EventNodeVtbl = 0x0125BE8C;
    public const uint EventNodeFireFn = 0x006872B0;
    public const uint EventTickReadFn = 0x0049D870;
    public const int EventPostKind = 55;
    public const int EventPostDelay = 50;
    public const uint SunnyvaleMainTick = 0x00CDD360;
    public const uint HeroBoastsTick = 0x00CE1AF0;
    public const uint PersonalMainTick = 0x00CDDCB0;
    public const uint QuestSubjectFillFn = 0x008884D0;
    public const uint WorldTickTableVa = 0x013B9288;
    public const uint WorldTickSlot1FnVa = 0x013B92C8;
    public const int WorldTickSlotStride = 64;
    public const int WorldTickType = 1;
    /// <summary>
    /// <c>00416670</c> → <c>00415FE0</c>
    /// only if sub[+0]==2.
    /// First-seen type 1 skips
    /// game vtbl+16.
    /// </summary>
    public const uint WalkTickBeforeDispatchFn = 0x00416670;
    public const uint ApplyTickTypeFn = 0x00415FE0;
    public const int GameVtbl16 = 16;
    /// <summary>
    /// <c>00434A60</c> calls
    /// table[type]+48.
    /// Type 1 <c>[0x13B92F8]</c>
    /// is 0 (<c>0121BA4F</c>).
    /// </summary>
    public const uint WalkTickAfterDispatchFn = 0x00434A60;
    public const uint WorldTickSlot1Plus48Va = 0x013B92F8;
    public const int WorldTickSlot1Plus48FirstSeen = 0;
    public const uint DisplayTickTailFn = 0x00434F60;
    public const int DisplayPlus232Offset = 232;
    /// <summary>
    /// Display ctor <c>00434E10</c>
    /// writes <c>+232=0x1E</c>. Host
    /// first-seen 0 skip is leftover.
    /// </summary>
    public const int DisplayPlus232Ctor = 0x1E;
    public const uint DisplayCtorFn = 0x00434E10;
    public const uint DisplayVtbl = 0x01231574;
    /// <summary>
    /// <c>00417418</c> <c>00BFEA1A(0x100)</c>
    /// then <c>00434E10</c> then
    /// <c>0041940C</c> <c>[game+40]</c>.
    /// Not frontend
    /// <see cref="DisplayObjectAllocFn"/>.
    /// Blob <c>+20/+24/+28</c> device
    /// helpers are omitted.
    /// </summary>
    public const uint DisplayAllocFn = 0x00BFEA1A;
    public const int DisplaySize = 0x100;
    public const uint DisplayBlobZeroFn = 0x00419270;
    public const int DisplayBlobSize = 36;
    public const uint DisplayStoreFn = 0x0041940C;
    public const int GameDisplayOffset = 40;
    public const int DisplayPlus4Offset = 4;
    public const int DisplayPlus4Ctor = 1;
    public const int GameDisplayPlus8Offset = 8;
    public const int DisplayPlus12Offset = 12;
    public const int DisplayPlus16Offset = 16;
    public const int DisplayPlus20Offset = 20;
    public const int DisplayPlus24Offset = 24;
    public const int DisplayPlus248Offset = 248;
    public const int GameGraphicBankOffset = 90568;
    public const int WorldMeshBankOffset = 60;
    public const uint DisplayFadeDestFn = 0x00434CD0;
    public const uint DisplayFadeDestStub = 0x009D8250;
    public const uint DisplayFadeDestFlagVa = 0x01375CDC;
    public const int DisplayFadeDestFlagFirstSeen = 0;
    public const uint DisplayPlayerOverlayLookup = 0x00449960;
    public const uint DisplayPlayerOverlayThing = 0x00487DD0;
    public const uint DisplayPlayerInterfaceApply = 0x0057B43F;
    /// <summary>
    /// <c>004A5A40</c> <c>004A5DF3</c>
    /// <c>006B3FF0</c> before
    /// <c>004A5E10</c>. Not
    /// <c>00501450</c>.
    /// </summary>
    public const uint WorldTickCameraSeedSite = 0x004A5DF3;
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
    public const uint InitPlayerManagerFn = 0x0041732A;
    public const uint PlayerOwnerStoreFn = 0x004193A0;
    public const uint PlayerOwnerVtbl = PlayerInterface.OwnerVtbl;
    public const int PlayerOwnerSize = PlayerInterface.OwnerSize;
    public const int PlayerOwnerOffset = PlayerInterface.OwnerOffset;
    public static readonly string[] PlayerOwnerHeroSwapNames =
    [
        "hero_swap_1.tng",
        "hero_swap_2.tng",
        "hero_swap_3.tng",
        "hero_swap_4.tng",
    ];
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
    /// <summary>
    /// First-seen frontend has no
    /// <c>E8 009DB700</c>. Only recovered
    /// sites are <c>009DC00E</c> and HUD
    /// <c>009DD93D</c>. Nonempty dest is
    /// <c>00BAE2D0</c> DIPUP, not
    /// <c>+16020</c>. Leftover #36 dest
    /// numbers stay UNREAD.
    /// </summary>
    public const bool FirstSeenFrontendE8Enqueue = false;
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
    public const uint PlayerThingSmartPtrFn = 0x00A01B50;
    public const uint PlayerThingSmartPtrCtor = 0x00A01B10;
    public const int PlayerSlotPlus44Offset = 44;
    /// <summary>
    /// <c>004B4490</c> after
    /// <c>00CB8220</c> skip:
    /// <c>00449970</c> /
    /// <c>00487DC0</c>. First-seen
    /// slot <c>0044BC10</c>
    /// <c>00A01B10</c> leaves
    /// <c>+48=0</c> so
    /// <c>00A01B50</c> is 0 and
    /// <c>004AFCA0</c> is skipped.
    /// </summary>
    public const uint QuestPlayerSyncFn = 0x004AFCA0;
    /// <summary>
    /// <c>004A5D91 006E75C0([world+56], flag)</c>.
    /// <c>world+56</c> is Init Scripts
    /// <c>006E7740</c> from world vtbl+28
    /// <c>004A6550</c> at <c>004A6646</c>.
    /// First-seen flag=1
    /// (<c>[thing+84]=0</c>,
    /// <c>00419680</c> on
    /// <c>004C60F0</c> <c>+4/+12=0</c>).
    /// <c>vtbl+1580</c> <c>0088E9E0</c>
    /// <c>[this+44]=0</c>.
    /// <c>vtbl+1544</c> <c>00892270</c>
    /// <c>[0x13B8790]+246=0</c>
    /// (<c>0049166E</c> <c>[+24]+222</c>).
    /// <c>WorldFrame % [0x1375550]</c>
    /// is <c>0%15==0</c> then
    /// <c>[this+60]</c> empty so
    /// <c>0059299D</c> is skipped.
    /// Not <c>00501450</c>.
    /// Next site is
    /// <c>006874B0([world+96])</c>.
    /// </summary>
    public const uint ScriptManagerPumpFn = 0x006E75C0;
    public const uint ScriptManagerCtor = 0x006E7740;
    public const uint ScriptManagerVtbl = 0x01260F0C;
    public const uint ScriptManagerVa = 0x0143E8F0;
    /// <summary>
    /// <c>00419CE0</c> <c>[world+56]</c>
    /// vtbl+1104. Not
    /// <c>00CB5AD0</c> directly.
    /// </summary>
    public const uint ScriptManagerActivateQuestFn = 0x00892E80;
    public const int ScriptManagerActivateQuestVtbl = 1104;
    public const uint InitScriptsParentFn = 0x004A6550;
    public const uint ScriptPausedGateFn = 0x0088E9E0;
    public const uint ScriptGuiGateFn = 0x00892270;
    public const uint ScriptListIterFn = 0x0059299D;
    public const uint GuiPlus24Ctor = 0x0049166E;
    public const uint PlayerGuiInstanceVa = 0x013B8790;
    public const int WorldScriptManagerOffset = 56;
    public const int ScriptManagerPlus44Offset = 44;
    public const int ScriptManagerPlus44FirstSeen = 0;
    public const int ScriptManagerPlus60Offset = 60;
    public const int GuiPlus246Offset = 246;
    public const int GuiPlus246FirstSeen = 0;
    /// <summary>
    /// <c>004A5D99 006874B0([world+96])</c>.
    /// <c>world+96</c> is Init Event
    /// Manager <c>00687510</c> from
    /// <c>004A6E30</c> at <c>004A727A</c>
    /// (alloc 8, <c>004ADF80</c>).
    /// Ctor <c>[this+4]</c> is an empty
    /// circular sentinel (alloc 96,
    /// <c>+0=+4=self</c>).
    /// <c>006874B0</c> <c>cmp [head],head</c>
    /// so first-seen returns. No
    /// <c>0049D870</c>, no node
    /// <c>vtbl+0</c>, no <c>00BFEA14</c>.
    /// <c>00687540</c> would insert, but
    /// first-seen <c>004B2890</c> walks
    /// empty <c>[quest+112]</c> (ctor
    /// sentinel; <c>004B4260</c> uses
    /// <c>+156</c> / a local vector).
    /// Not <c>00501450</c>.
    /// Next site is the 4×
    /// <c>004498C0</c> slot loop then
    /// <c>00436FB0</c>.
    /// </summary>
    public const uint EventManagerPumpFn = 0x006874B0;
    public const uint EventManagerCtor = 0x00687510;
    public const uint EventManagerPostFn = 0x00687540;
    public const uint EventNodeFreeFn = 0x00BFEA14;
    public const int WorldEventManagerOffset = 96;
    public const int EventManagerPlus4Offset = 4;
    /// <summary>
    /// <c>004A5DA1</c> 4×
    /// <c>004498C0([world+12], 0..3)</c>.
    /// <c>world+12</c> is ctor
    /// <c>[arg+8]</c> (player manager).
    /// Create Players
    /// <c>0048A210</c> /
    /// <c>0099A350</c> leaves
    /// <c>[slot+4]=1</c> and
    /// <c>[slot+40]=index</c> so
    /// <c>0099A330</c> takes
    /// <c>00488AB0</c>.
    /// <c>[+534]=1</c> already so
    /// <c>004887C0</c> is skipped.
    /// <c>00A01B50(+44)</c> miss
    /// skips <c>006A4D00</c> /
    /// vtbl+48 / <c>005063E0</c>.
    /// </summary>
    public const uint PlayerSlotValidFn = 0x0099A330;
    public const uint PlayerSlotTickFn = 0x00488AB0;
    public const uint PlayerSlotOneShotFn = 0x004887C0;
    public const int WorldPlayerManagerOffset = 12;
    public const int PlayerSlotPlus4Offset = 4;
    public const int PlayerSlotPlus4FirstSeen = 1;
    public const int PlayerSlotIndexOffset = 40;
    public const int PlayerSlotPlus534Offset = 534;
    public const int PlayerSlotPlus534FirstSeen = 1;
    public const int PlayerSlotLoopCount = 4;
    /// <summary>
    /// <c>004A5DC5 00436FB0</c> then
    /// <c>00640320(flag)</c>.
    /// Singleton <c>[0x13BA854]</c>
    /// vtbl <c>01231584</c>.
    /// Init Engine OnActivate
    /// <c>006404D0</c> inserts
    /// <c>[engine+44]</c>
    /// (<c>00B26340</c> /
    /// <c>00B260B0</c> vtbl
    /// <c>012A0F3C</c>).
    /// <c>vtbl+204</c> <c>00B23550</c>
    /// is <c>[display+8]</c>; ctor 0
    /// so <c>vtbl+36</c> <c>00B24030</c>
    /// is skipped. Not
    /// <c>00501450</c>.
    /// </summary>
    public const uint DisplayListenerGetFn = 0x00436FB0;
    public const uint DisplayListenerVa = 0x013BA854;
    public const uint DisplayListenerVtbl = 0x01231584;
    public const uint DisplayListenerPumpFn = 0x00640320;
    public const uint EnvironmentTickFn = 0x006BB990;
    public const uint EnvironmentCtor = 0x006BBC30;
    public const int EnvironmentPlus33Offset = 33;
    public const int EnvironmentPlus33FirstSeen = 0;
    public const int EnvironmentPlus24Offset = 24;
    public const int EnvironmentPlus24FirstSeen = 0;
    public const int EnvironmentDayDivisor = 15;
    public const uint EnvironmentDayDivisorVa = 0x01375550;
    public const uint BulletTimeTickFn = 0x004C5E90;
    public const uint ConversationTickFn = 0x006E60F0;
    public const uint ConversationCtor = 0x006E6150;
    public const uint ThingManagerFlushFn = 0x0051F070;
    public const uint ThingManagerCtor = 0x00523540;
    public const uint OpinionTickFn = 0x006BDC60;
    public const uint PlayerGuiTickFn = 0x0043A080;
    public const uint AtmosTickFn = 0x006B2260;
    public const uint AtmosGateVa = 0x013B8394;
    public const uint SpeechGainTickFn = 0x006E37D0;
    public const uint SpeechGainListVa = 0x013BABA0;
    public const uint DisplayListenerInsertFn = 0x006404D0;
    public const uint DisplayObjectAllocFn = 0x00B26340;
    public const uint DisplayObjectCtor = 0x00B260B0;
    public const uint DisplayObjectVtbl = 0x012A0F3C;
    public const uint DisplayActiveGateFn = 0x00B23550;
    public const uint DisplayActiveApplyFn = 0x00B24030;
    public const int DisplayPlus8Offset = 8;
    public const int DisplayPlus8FirstSeen = 0;
    public const int PlayerThingPlus145Offset = 145;
    public const int PlayerThingPlus142Offset = 142;
    public const uint PlayerCreatureFactoryFn = 0x0052B880;
    public const uint HolySiteFactoryFn = 0x0052AC90;
    public const uint CreateCharacterFn = 0x00489D40;
    /// <summary>
    /// BSS <c>[0x13B866C]</c> is the
    /// <c>SetStartingHolySite</c> CString
    /// (also <see cref="WorldPathAltGlobalVa"/>).
    /// Readers are inside <c>00488B20</c>.
    /// First post-Leave take misses; live
    /// Lookout pose is <c>GuildArrivalHSP</c>,
    /// not <c>NOVStartHSP</c>.
    /// <c>00DBDE40</c> does not read this slot.
    /// </summary>
    public const uint StartingHolySiteFindFn = 0x00488B20;
    public const uint StartingHolySiteReadName = 0x00488B68;
    public const uint SetStartingHolySiteFn = 0x00413840;
    public const bool StartingHolySiteIsNovStartOnNoSave = false;
    /// <summary>
    /// <c>userst.ini</c> stores
    /// <c>NOVStartHSP</c> before
    /// frontend. First post-Leave
    /// <c>00488B20</c> misses. Live
    /// pose is still
    /// <c>GuildArrivalHSP</c>.
    /// </summary>
    public const string StartingHolySiteStoredName = "NOVStartHSP";
    public const bool StartingHolySiteFinderMissesOnNoSave = true;
    public const uint InitCharactersFn = 0x0049F180;
    public const uint InitGuiFn = 0x0043A380;
    /// <summary>
    /// Unique <c>push 0x338</c> is
    /// Create Players <c>00487FC3</c>.
    /// Init GUI does not alloc.
    /// After no-save Init GUI nobody
    /// writes a new size.
    /// </summary>
    public const bool InitGuiIsCtor = false;
    public const bool PlayerGuiSizeWrittenAfterInitGui = false;
    /// <summary>
    /// Create Players constructs the
    /// <c>PLAYER_GUI_PC</c> singleton.
    /// Init GUI <c>0043A380</c> is reset
    /// on that object, not a widget
    /// factory and not first HUD DIP.
    /// </summary>
    public const uint PlayerGuiAllocFn = 0x00487FB0;
    public const uint PlayerGuiCtorFn = 0x0043B570;
    public const uint PlayerGuiStoreFn = 0x004195AF;
    public const uint PlayerGuiVtbl = 0x0123177C;
    public const int PlayerGuiObjectSize = 0x338;
    public const uint PlayerGuiSingletonVa = 0x013B8790;
    public const uint PlayerGuiDefVa = 0x013B878C;
    public const string PlayerGuiPcName = "PLAYER_GUI_PC";
    /// <summary>
    /// <c>CPlayerGuiDef</c> persist
    /// <c>+0x338</c> is <c>[esi+824]</c>
    /// i32 via <c>004736C4</c>
    /// <c>00431102</c>. Not instance
    /// field (size is <c>0x338</c>).
    /// Not first HUD dest. First
    /// Present after Leave still
    /// empty-skips overlay.
    /// </summary>
    public const uint PlayerGuiDefFactory = 0x00462F93;
    public const uint PlayerGuiDefCtor = 0x00459BB6;
    public const uint PlayerGuiDefVtbl = 0x012352DC;
    public const int PlayerGuiDefSize = 0xAB4;
    public const uint PlayerGuiDefPersistFn = 0x004736C4;
    public const int PlayerGuiDefPlus338 = 824;
    public const bool PlayerGuiDefPlus338IsHud = false;
    public const bool PlayerGuiFirstPresentDrawsHud = false;
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
    /// <summary>
    /// <c>004A0D90</c> flag 1:
    /// <c>004A08D0</c> clears
    /// world+184 / +172 / +196
    /// before parse.
    /// </summary>
    public const uint QstClearFn = 0x004A08D0;
    public const int QstParseFlagClear = 1;
    public const int QstParseFlagAppend = 0;
    public const uint ActivateInitialQuestsFn = 0x004B4A10;
    public const uint ActivateInitialQuestsSite = 0x00416BCF;
    /// <summary>
    /// <c>00CD6E27</c> binds
    /// <c>Q_NewOakValeIntro</c> to
    /// <c>S_QNOVI</c> / <c>00DBEF70</c>
    /// via <c>00CB5C90</c>. Not
    /// <c>00CB5AD0</c> / <c>004B4A10</c>.
    /// </summary>
    public const uint OakvaleBindSite = 0x00CD6E27;
    public const uint OakvaleFactoryFn = 0x00DBEF70;
    /// <summary>
    /// Childhood TNG has no
    /// <c>CActivateQuestDef</c> /
    /// <c>CTCActionUseActivateQuest</c>
    /// component. <c>Q_NewOakValeIntro</c>
    /// in <c>StartOakValeWest.tng</c> is
    /// only <c>XXXSectionStart</c>.
    /// </summary>
    public const bool ChildhoodTngQueuesActivateQuest = false;
    /// <summary>
    /// CWorld+212 u8
    /// <c>CreatureGenerationEnabled</c>.
    /// Ctor <c>004A68EA</c> writes 1.
    /// <c>0049EA40</c> reads this slot.
    /// Not frontend persist+212.
    /// </summary>
    public const int WorldCreatureGenerationEnabledOffset = 212;
    /// <summary>
    /// Save-stream parser. One <c>E8</c>
    /// (<c>004B58F3</c> self). Not on
    /// no-save New Game.
    /// </summary>
    public const uint StartNewQuestParseFn = 0x004B5080;
    /// <summary>
    /// Fresh-profile New Game is the
    /// no-save walk. <c>+90584</c>
    /// empty skips <c>004B4A10</c>.
    /// Profile <c>0x126</c> does not
    /// write a FableSav quest list.
    /// </summary>
    public const bool NewGameIsNoSaveWalk = true;
    public const bool NewGameWritesSaveQuestList = false;
    /// <summary>
    /// Second <c>004B4260</c> site
    /// (<c>0049EAD1</c>). Listing
    /// <c>0049EAC0</c> is
    /// <c>push 1</c> /
    /// <c>add ecx, 0xAC</c> /
    /// <c>004B4260</c> /
    /// <c>jmp 004B2890</c>.
    /// Same <c>this+172</c> TRUE
    /// vector as Init Quests.
    /// <c>functions.tsv</c> parent
    /// <c>0049EA40</c> is a different
    /// fn (<c>ret 4</c> at
    /// <c>0049EABD</c>). Zero
    /// <c>E8</c> / vtbl inbound.
    /// Not a no-save Oakvale
    /// activator.
    /// </summary>
    public const uint QuestActivatePlus172SiblingFn = 0x0049EAC0;
    public const uint QuestActivatePlus172SiblingCall = 0x0049EAD1;
    public const int QuestActivatePlus172SiblingListOffset = 0xAC;
    public const bool QuestActivatePlus172SiblingHasInbound = false;
    /// <summary>
    /// <c>00896A30</c> first <c>E8</c>.
    /// Finds a card thing; requires
    /// <c>004AF610</c> already active.
    /// </summary>
    public const uint QuestCardFindFn = 0x004B0D30;
    public const uint AddTestQuestStoreFn = 0x004A113B;
    public const int WorldAddTestQuestOffset = 196;
    /// <summary>
    /// Inventory-quests UI confirm
    /// <c>0061AB30</c> is
    /// <c>CTCInventoryQuests</c>.
    /// Gated on <c>[this+343]</c>.
    /// Not persist +228 / not
    /// <c>[retail+41]</c>. Zero
    /// construct on no-save New
    /// Game. Oakvale card <c>+24</c>
    /// nonempty takes <c>004B4C50</c>
    /// not <c>004B4A10</c>.
    /// </summary>
    public const uint InventoryQuestsConfirmFn = 0x0061AB30;
    public const bool InventoryQuestsConfirmIsNewGame = false;
    /// <summary>
    /// Second <c>E8</c> of Give body
    /// <c>004B1D30</c> is leftover
    /// <c>CTCInventoryQuests</c>
    /// <c>0061ACB3</c>. Unique caller
    /// <c>0061B5FD</c>. Not first-seen.
    /// Does not post <c>0x33</c> on
    /// no-save.
    /// </summary>
    public const uint InventoryQuestsGiveFn = 0x0061ACB3;
    public const bool InventoryQuestsGiveIsFirstSeen = false;
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
    /// <c>PlayerRegionName</c> from the FableSav
    /// <c>PLAYER</c> blob — continue, not
    /// no-save New Game. HEADER is
    /// <c>CurrentRegionName</c>, not this key.
    /// Save write is <c>00449F90</c> only
    /// (<c>0049FB5C</c> inside <c>0049F4C0</c>).
    /// Zero no-save writer.
    /// </summary>
    public const uint LoadRegionByNameFn = 0x00487C20;
    public const uint LoadRegionByNamePersist = 0x00449E60;
    public const uint SavePlayerRegionNameFn = 0x00449F90;
    public const uint SavePlayerRegionNameSite = 0x0049FB5C;
    public const uint FableSavPlayerWriter = 0x0049F4C0;
    public const uint PersistCStringTransfer = 0x004109A0;
    public const string PlayerRegionNameKey = "PlayerRegionName";
    public const string FableSavPlayerSection = "PLAYER";
    public const bool PlayerRegionNameWrittenOnNewGame = false;
    /// <summary>
    /// <c>00501450</c>: player
    /// <c>00449970</c>/<c>00487DC0</c>,
    /// <c>004FEEC0(current,0)</c> writes
    /// <c>+156=0</c>, table count
    /// <c>(+48-+44)/88</c>. Count &gt; 1
    /// loops <c>00500540(i,0,0)</c> from 1
    /// through count-1 (+36 null still
    /// <c>006C27A0</c>). After each i two
    /// collectors on <c>0049C770</c>
    /// <c>[map+8]+32 +24</c>:
    /// <c>0048D400</c> (+145 need
    /// <c>0x0C</c> forbid <c>0x21</c>,
    /// <c>006A80A0</c> bit <c>0x64</c>)
    /// and <c>005198B0</c>
    /// (<c>00518DC0</c>
    /// <c>CTCActionUseScriptedHook</c>,
    /// not a release). Then
    /// <c>RegionGraph.txt</c> and
    /// <c>00500540(saved,0,1)</c> with no
    /// sync pump. First-seen saved is 0.
    /// E8/E9/imm/vtbl of <c>00501450</c>
    /// are 0 (caller UNREAD; not
    /// <c>004162B5</c> / <c>00418289</c>).
    /// </summary>
    public const uint LoadFromFirstRealRegionFn = 0x00501450;
    /// <summary>
    /// Named inbound encodings to
    /// <c>00501450</c> remain 0
    /// (E8/E9/imm/vtbl/call r32/jmp r32/
    /// VA dword/RVA dword). Pump never
    /// calls it. Live native entry still
    /// UNREAD. Next unread is two-immediate
    /// ALU / get-PC add.
    /// </summary>
    public const int LoadFromFirstRealRegionNamedInbound = 0;
    public const bool PumpCallsLoadFromFirstRealRegion = false;
    public const uint CollectRegionThingsFn = 0x0048D400;
    public const uint CollectThingsListFn = 0x0049C770;
    public const uint CollectThingsBitTestFn = 0x006A80A0;
    public const int CollectThingsBitIndex = 0x64;
    /// <summary>
    /// Bit <c>0x64</c> is dword 3 at
    /// <c>thing+44</c>. Collect filter
    /// only. Do not name it collidable.
    /// </summary>
    public const int CollectThingsBitDword = 3;
    public const int CollectThingsBitThingOffset = 44;
    public const bool CollectThingsBitMeansCollidable = false;
    public const bool HeroAddsPhysicsControlledOnly = true;
    /// <summary>
    /// First-seen after Leave is pose
    /// persist + a Thing bitset collect,
    /// not a collision solver. Do not
    /// invent Unity-style physics.
    /// </summary>
    public const bool FirstSeenCollisionIsSolver = false;
    public const uint CtcPhysicsStandardFactory = 0x004D297B;
    public const uint CtcPhysicsStandardCtor = 0x00723FD0;
    public const int CtcPhysicsStandardSize = 0x88;
    public const uint CtcPhysicsStandardPersist = 0x00724290;
    public const int CtcPhysicsStandardPosOffset = 80;
    public const int CtcPhysicsStandardAxisOffset = 92;
    public const int ThingCollectFlagsOffset = 145;
    public const int ThingCollectFlagsNeed = 0x0C;
    public const int ThingCollectFlagsForbid = 0x21;
    public const uint CollectScriptedHookThingsFn = 0x005198B0;
    public const uint ScriptedHookCollectFn = 0x00518DC0;
    public const string ScriptedHookName = "CTCActionUseScriptedHook";
    public const int ScriptedHookKey = 0xC2;
    public const int ScriptedHookThingOffset = 56;
    public const int ScriptedHookThingBit = 4;
    /// <summary>
    /// Lookout no-save collect walks
    /// <c>CTCActionUseScriptedHook</c>
    /// key <c>0xC2</c>. Childhood barrels
    /// / gold / doors live on
    /// <c>StartOakValeWest</c> +
    /// <c>00DBDE40</c>, not this walk.
    /// Host Notes the collect; it does
    /// not attach or fire world Use.
    /// Frontend actions 25–27 are UI.
    /// </summary>
    public const bool WorldUseAttachedOnNoSave = false;
    public const bool LookoutHasBarrels = false;
    public const bool FrontendActionsAreWorldUse = false;
    public const uint EmptyNameVa = 0x0122D70E;
    public const uint RegionGraphNameVa = 0x0124467C;
    public const string RegionGraphName = "RegionGraph.txt";
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
    /// <summary>
    /// <c>[0x137544A]</c> after
    /// <c>00413C50</c> / <c>userst.ini</c>.
    /// </summary>
    public byte DisplayWindowFlag { get; private set; } = DisplayWindowFlagFirstSeen;
    /// <summary>
    /// <c>[0x13B861B]</c> then
    /// <c>009BC890</c> <c>[0x13CA7EA]</c>.
    /// </summary>
    public byte LeftAlignText { get; private set; }
    /// <summary>
    /// <c>[0x13B861C]</c> then
    /// <c>009BC8A0</c> <c>[0x13CA7EB]</c>.
    /// </summary>
    public byte NoHangulWordWrap { get; private set; }
    /// <summary>
    /// <c>[0x13B864F]</c>. File miss keeps
    /// the <c>004045C0</c> scratch 0.
    /// </summary>
    public byte DisableCapsLock { get; private set; }
    public bool LanguageSettingsLoaded { get; private set; }
    /// <summary>
    /// <c>009BF7E0</c> <c>[ebp+572] = ![ebx+28]</c>.
    /// </summary>
    public bool DeviceWindowed { get; private set; }
    public int DisplayZDepth { get; private set; } = DisplayZDepthFirstSeen;
    public int CreateWindowStyle { get; private set; } = CreateWindowExStyle;
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
    /// <c>[0x13B879C]</c> after
    /// <c>0044C6B6</c> miss →
    /// <c>0044C6C2</c> /
    /// <c>0044C71F</c>.
    /// </summary>
    public bool PlayerManagerPresent { get; private set; }
    /// <summary>
    /// Live <c>[this+40]</c> cap
    /// after <c>009FC520(0x80000)</c>.
    /// First consume is
    /// <c>CHeroMorphDef</c>.
    /// </summary>
    public int PlayerManagerPlus40Cap { get; private set; }
    public bool FirstDefClassRegistered { get; private set; }
    public string? FirstDefClass { get; private set; }
    public bool SecondDefClassRegistered { get; private set; }
    public string? SecondDefClass { get; private set; }
    public bool ThirdDefClassRegistered { get; private set; }
    public string? ThirdDefClass { get; private set; }
    public bool FourthDefClassRegistered { get; private set; }
    public string? FourthDefClass { get; private set; }
    public bool FifthDefClassRegistered { get; private set; }
    public string? FifthDefClass { get; private set; }
    public bool SixthDefClassRegistered { get; private set; }
    public string? SixthDefClass { get; private set; }
    public bool SeventhDefClassRegistered { get; private set; }
    public string? SeventhDefClass { get; private set; }
    public bool EighthDefClassRegistered { get; private set; }
    public string? EighthDefClass { get; private set; }
    public bool NinthDefClassRegistered { get; private set; }
    public string? NinthDefClass { get; private set; }
    public bool TenthDefClassRegistered { get; private set; }
    public string? TenthDefClass { get; private set; }
    public bool EleventhDefClassRegistered { get; private set; }
    public string? EleventhDefClass { get; private set; }
    public bool TwelfthDefClassRegistered { get; private set; }
    public string? TwelfthDefClass { get; private set; }
    public bool ThirteenthDefClassRegistered { get; private set; }
    public string? ThirteenthDefClass { get; private set; }
    public bool FourteenthDefClassRegistered { get; private set; }
    public string? FourteenthDefClass { get; private set; }
    public bool FifteenthDefClassRegistered { get; private set; }
    public string? FifteenthDefClass { get; private set; }
    public bool SixteenthDefClassRegistered { get; private set; }
    public string? SixteenthDefClass { get; private set; }
    public bool SeventeenthDefClassRegistered { get; private set; }
    public string? SeventeenthDefClass { get; private set; }
    public bool EighteenthDefClassRegistered { get; private set; }
    public string? EighteenthDefClass { get; private set; }
    public bool NineteenthDefClassRegistered { get; private set; }
    public string? NineteenthDefClass { get; private set; }
    public bool TwentiethDefClassRegistered { get; private set; }
    public string? TwentiethDefClass { get; private set; }
    public bool TwentyFirstDefClassRegistered { get; private set; }
    public string? TwentyFirstDefClass { get; private set; }
    public bool TwentySecondDefClassRegistered { get; private set; }
    public string? TwentySecondDefClass { get; private set; }
    public bool TwentyThirdDefClassRegistered { get; private set; }
    public string? TwentyThirdDefClass { get; private set; }
    public bool TwentyFourthDefClassRegistered { get; private set; }
    public string? TwentyFourthDefClass { get; private set; }
    public bool TwentyFifthDefClassRegistered { get; private set; }
    public string? TwentyFifthDefClass { get; private set; }
    public bool TwentySixthDefClassRegistered { get; private set; }
    public string? TwentySixthDefClass { get; private set; }
    public bool TwentySeventhDefClassRegistered { get; private set; }
    public string? TwentySeventhDefClass { get; private set; }
    public bool TwentyEighthDefClassRegistered { get; private set; }
    public string? TwentyEighthDefClass { get; private set; }
    public bool TwentyNinthDefClassRegistered { get; private set; }
    public string? TwentyNinthDefClass { get; private set; }
    public bool ThirtiethDefClassRegistered { get; private set; }
    public string? ThirtiethDefClass { get; private set; }
    public bool ThirtyFirstDefClassRegistered { get; private set; }
    public string? ThirtyFirstDefClass { get; private set; }
    public bool ThirtySecondDefClassRegistered { get; private set; }
    public string? ThirtySecondDefClass { get; private set; }
    public bool ThirtyThirdDefClassRegistered { get; private set; }
    public string? ThirtyThirdDefClass { get; private set; }
    public bool ThirtyFourthDefClassRegistered { get; private set; }
    public string? ThirtyFourthDefClass { get; private set; }
    public bool ThirtyFifthDefClassRegistered { get; private set; }
    public string? ThirtyFifthDefClass { get; private set; }
    public bool ThirtySixthDefClassRegistered { get; private set; }
    public string? ThirtySixthDefClass { get; private set; }
    public bool ThirtySeventhDefClassRegistered { get; private set; }
    public string? ThirtySeventhDefClass { get; private set; }
    public bool ThirtyEighthDefClassRegistered { get; private set; }
    public string? ThirtyEighthDefClass { get; private set; }
    public bool ThirtyNinthDefClassRegistered { get; private set; }
    public string? ThirtyNinthDefClass { get; private set; }
    public bool FortiethDefClassRegistered { get; private set; }
    public string? FortiethDefClass { get; private set; }
    public bool FortyFirstDefClassRegistered { get; private set; }
    public string? FortyFirstDefClass { get; private set; }
    public bool FortySecondDefClassRegistered { get; private set; }
    public string? FortySecondDefClass { get; private set; }
    public bool FortyThirdDefClassRegistered { get; private set; }
    public string? FortyThirdDefClass { get; private set; }
    public bool FortyFourthDefClassRegistered { get; private set; }
    public string? FortyFourthDefClass { get; private set; }
    public bool FortyFifthDefClassRegistered { get; private set; }
    public string? FortyFifthDefClass { get; private set; }
    public bool FortySixthDefClassRegistered { get; private set; }
    public string? FortySixthDefClass { get; private set; }
    public bool FortySeventhDefClassRegistered { get; private set; }
    public string? FortySeventhDefClass { get; private set; }
    public bool FortyEighthDefClassRegistered { get; private set; }
    public string? FortyEighthDefClass { get; private set; }
    public bool FortyNinthDefClassRegistered { get; private set; }
    public string? FortyNinthDefClass { get; private set; }
    public bool FiftiethDefClassRegistered { get; private set; }
    public string? FiftiethDefClass { get; private set; }
    public bool FiftyFirstDefClassRegistered { get; private set; }
    public string? FiftyFirstDefClass { get; private set; }
    public bool FiftySecondDefClassRegistered { get; private set; }
    public string? FiftySecondDefClass { get; private set; }
    public bool FiftyThirdDefClassRegistered { get; private set; }
    public string? FiftyThirdDefClass { get; private set; }
    public bool FiftyFourthDefClassRegistered { get; private set; }
    public string? FiftyFourthDefClass { get; private set; }
    public bool FiftyFifthDefClassRegistered { get; private set; }
    public string? FiftyFifthDefClass { get; private set; }
    public bool FiftySixthDefClassRegistered { get; private set; }
    public string? FiftySixthDefClass { get; private set; }
    public bool FiftySeventhDefClassRegistered { get; private set; }
    public string? FiftySeventhDefClass { get; private set; }
    public bool FiftyEighthDefClassRegistered { get; private set; }
    public string? FiftyEighthDefClass { get; private set; }
    public bool FiftyNinthDefClassRegistered { get; private set; }
    public string? FiftyNinthDefClass { get; private set; }
    public bool SixtiethDefClassRegistered { get; private set; }
    public string? SixtiethDefClass { get; private set; }
    public bool SixtyFirstDefClassRegistered { get; private set; }
    public string? SixtyFirstDefClass { get; private set; }
    public bool SixtySecondDefClassRegistered { get; private set; }
    public string? SixtySecondDefClass { get; private set; }
    public bool SixtyThirdDefClassRegistered { get; private set; }
    public string? SixtyThirdDefClass { get; private set; }
    public bool SixtyFourthDefClassRegistered { get; private set; }
    public string? SixtyFourthDefClass { get; private set; }
    public bool SixtyFifthDefClassRegistered { get; private set; }
    public string? SixtyFifthDefClass { get; private set; }
    public bool SixtySixthDefClassRegistered { get; private set; }
    public string? SixtySixthDefClass { get; private set; }
    public bool SixtySeventhDefClassRegistered { get; private set; }
    public string? SixtySeventhDefClass { get; private set; }
    public bool SixtyEighthDefClassRegistered { get; private set; }
    public string? SixtyEighthDefClass { get; private set; }
    public bool SixtyNinthDefClassRegistered { get; private set; }
    public string? SixtyNinthDefClass { get; private set; }
    public bool SeventiethDefClassRegistered { get; private set; }
    public string? SeventiethDefClass { get; private set; }
    public bool SeventyFirstDefClassRegistered { get; private set; }
    public string? SeventyFirstDefClass { get; private set; }
    public bool SeventySecondDefClassRegistered { get; private set; }
    public string? SeventySecondDefClass { get; private set; }
    public bool SeventyThirdDefClassRegistered { get; private set; }
    public string? SeventyThirdDefClass { get; private set; }
    public bool SeventyFourthDefClassRegistered { get; private set; }
    public string? SeventyFourthDefClass { get; private set; }
    public bool SeventyFifthDefClassRegistered { get; private set; }
    public string? SeventyFifthDefClass { get; private set; }
    public bool SeventySixthDefClassRegistered { get; private set; }
    public string? SeventySixthDefClass { get; private set; }
    public bool SeventySeventhDefClassRegistered { get; private set; }
    public string? SeventySeventhDefClass { get; private set; }
    public bool SeventyEighthDefClassRegistered { get; private set; }
    public string? SeventyEighthDefClass { get; private set; }
    public bool SeventyNinthDefClassRegistered { get; private set; }
    public string? SeventyNinthDefClass { get; private set; }
    public bool EightiethDefClassRegistered { get; private set; }
    public string? EightiethDefClass { get; private set; }
    public bool EightyFirstDefClassRegistered { get; private set; }
    public string? EightyFirstDefClass { get; private set; }
    public bool EightySecondDefClassRegistered { get; private set; }
    public string? EightySecondDefClass { get; private set; }
    public bool EightyThirdDefClassRegistered { get; private set; }
    public string? EightyThirdDefClass { get; private set; }
    public bool EightyFourthDefClassRegistered { get; private set; }
    public string? EightyFourthDefClass { get; private set; }
    public bool EightyFifthDefClassRegistered { get; private set; }
    public string? EightyFifthDefClass { get; private set; }
    public bool EightySixthDefClassRegistered { get; private set; }
    public string? EightySixthDefClass { get; private set; }
    public bool EightySeventhDefClassRegistered { get; private set; }
    public string? EightySeventhDefClass { get; private set; }
    public bool EightyEighthDefClassRegistered { get; private set; }
    public string? EightyEighthDefClass { get; private set; }
    public bool EightyNinthDefClassRegistered { get; private set; }
    public string? EightyNinthDefClass { get; private set; }
    public bool NinetiethDefClassRegistered { get; private set; }
    public string? NinetiethDefClass { get; private set; }
    public bool NinetyFirstDefClassRegistered { get; private set; }
    public string? NinetyFirstDefClass { get; private set; }
    public bool NinetySecondDefClassRegistered { get; private set; }
    public string? NinetySecondDefClass { get; private set; }
    public bool NinetyThirdDefClassRegistered { get; private set; }
    public string? NinetyThirdDefClass { get; private set; }
    public bool NinetyFourthDefClassRegistered { get; private set; }
    public string? NinetyFourthDefClass { get; private set; }
    public bool NinetyFifthDefClassRegistered { get; private set; }
    public string? NinetyFifthDefClass { get; private set; }
    public bool NinetySixthDefClassRegistered { get; private set; }
    public string? NinetySixthDefClass { get; private set; }
    public bool NinetySeventhDefClassRegistered { get; private set; }
    public string? NinetySeventhDefClass { get; private set; }
    public bool NinetyEighthDefClassRegistered { get; private set; }
    public string? NinetyEighthDefClass { get; private set; }
    public bool NinetyNinthDefClassRegistered { get; private set; }
    public string? NinetyNinthDefClass { get; private set; }
    public bool HundredthDefClassRegistered { get; private set; }
    public string? HundredthDefClass { get; private set; }
    public bool HundredFirstDefClassRegistered { get; private set; }
    public string? HundredFirstDefClass { get; private set; }
    public bool HundredSecondDefClassRegistered { get; private set; }
    public string? HundredSecondDefClass { get; private set; }
    public bool HundredThirdDefClassRegistered { get; private set; }
    public string? HundredThirdDefClass { get; private set; }
    public bool HundredFourthDefClassRegistered { get; private set; }
    public string? HundredFourthDefClass { get; private set; }
    public bool HundredFifthDefClassRegistered { get; private set; }
    public string? HundredFifthDefClass { get; private set; }
    public bool HundredSixthDefClassRegistered { get; private set; }
    public string? HundredSixthDefClass { get; private set; }
    public bool HundredSeventhDefClassRegistered { get; private set; }
    public string? HundredSeventhDefClass { get; private set; }
    public bool HundredEighthDefClassRegistered { get; private set; }
    public string? HundredEighthDefClass { get; private set; }
    public bool HundredNinthDefClassRegistered { get; private set; }
    public string? HundredNinthDefClass { get; private set; }
    public bool HundredTenthDefClassRegistered { get; private set; }
    public string? HundredTenthDefClass { get; private set; }
    public bool HundredEleventhDefClassRegistered { get; private set; }
    public string? HundredEleventhDefClass { get; private set; }
    public bool ThingComponentsFilled { get; private set; }
    public bool ThingComponentsCommitted { get; private set; }
    /// <summary>
    /// After <c>00416005</c>
    /// <c>0044C72B</c> /
    /// <c>009ACB10</c>.
    /// Not a game.bin parse.
    /// </summary>
    public bool DefinitionManagerPrepared { get; private set; }
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
    public IReadOnlyList<string> UserstIniCommands { get; private set; } = [];
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
    /// <summary>
    /// <c>[game+9]</c>. <c>004189C2</c>
    /// writes 1 at entry, 0 on leave.
    /// </summary>
    public bool GamePlus9 { get; private set; }
    /// <summary>
    /// <c>[game+8]</c>. First-seen 0 so
    /// <c>004189C2</c> loops after
    /// <c>009AC9E0</c>.
    /// </summary>
    public bool GamePlus8 { get; private set; }
    /// <summary>
    /// <c>[engine+8]</c>. First-seen 0.
    /// <c>WM_DESTROY</c> sets 1.
    /// </summary>
    public int EnginePlus8 { get; private set; }
    public bool GamePumpLeft { get; private set; }
    /// <summary>
    /// <c>[engine+88]</c> after
    /// <c>009A6610</c> bit 0x01.
    /// </summary>
    public bool EnginePlus88 { get; private set; }
    /// <summary>
    /// <c>[engine+9]</c>. <c>009A6610</c>
    /// writes 1. Focus compare after
    /// PeekMessage is PARTIAL (IAT
    /// <c>0x1440378</c>).
    /// </summary>
    public int EnginePlus9 { get; private set; }
    public bool EnginePlus124 { get; private set; }
    /// <summary>
    /// <c>009A64B0</c> <c>CreateWindowExW</c>
    /// wrote <c>[engine+148]</c>.
    /// </summary>
    public bool EngineWindowCreated { get; private set; }
    /// <summary>
    /// First-seen after AVI: Fable
    /// window is foreground so
    /// <c>009A57B0</c> returns 1.
    /// </summary>
    public bool EngineForeground { get; set; }
    public int DisplayPlus104 { get; private set; }
    public int DisplayPlus104Copy { get; private set; }
    /// <summary>
    /// Display <c>+232</c>. Ctor
    /// <c>0x1E</c>; <c>00434F60</c>
    /// decrements while &gt;0.
    /// </summary>
    public int DisplayPlus232 { get; set; }
    public int FrameListCount { get; private set; }
    /// <summary>
    /// <c>004AE9D0</c> <c>+9836</c> =
    /// <c>[game+72]</c>.
    /// </summary>
    public int PlayerBindSlot0 { get; private set; }
    /// <summary>
    /// <c>004AE9D0</c> <c>+9840</c> =
    /// <c>00416392</c> (first-seen 0).
    /// </summary>
    public int PlayerBindSlot1 { get; private set; }
    /// <summary>
    /// <c>004AE9D0</c> <c>+9844</c> =
    /// <c>[game+90428]</c> (first-seen 0).
    /// </summary>
    public int PlayerBindSlot2 { get; private set; }
    public bool PlayerCatchupHit { get; private set; }
    /// <summary>
    /// <c>0049B9E0</c> <c>[ring+24]</c>
    /// after each inner <c>00416202</c>.
    /// </summary>
    public int FrameDtRingSamples { get; private set; }
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
    public bool InputRecordStored { get; private set; }
    public int GamePlus76 { get; private set; }
    public int GamePlus80 { get; set; }
    public int GamePlus104 { get; private set; }
    public int GamePlus90424 { get; private set; }
    public bool GamePlus90594 { get; private set; }
    /// <summary>
    /// <c>004171F4</c> <c>[game+90596]</c>
    /// when first <c>004AEA70</c> is 0.
    /// </summary>
    public int GamePlus90596 { get; private set; }
    /// <summary>
    /// First-seen <c>0041707E</c> skipped
    /// <c>00435F70</c> because
    /// <c>004AEA70</c> is 0 and
    /// <c>[0x13B8688]</c> is 0.
    /// </summary>
    public bool DisplayPresentSkipped { get; private set; }
    public float GamePlus112 { get; private set; }
    public float GamePlus116 { get; private set; }
    public float GamePlus120 { get; private set; }
    public float GamePlus124 { get; private set; }
    public float GamePlus128 { get; private set; }
    public float GamePlus132 { get; private set; }
    public float GamePlus136 { get; private set; }
    public float GamePlus140 { get; private set; }
    /// <summary>
    /// <c>00417A58</c> tail
    /// <c>00991840(1)</c> →
    /// <c>[game+16]</c>. Register lookup,
    /// not a music player.
    /// </summary>
    public int GamePlus16 { get; private set; }
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
    /// <c>009E1BC0</c> seconds. Host
    /// <see cref="Pump(float)"/> adds
    /// <c>dt</c> after the first
    /// <c>004189C2</c> snapshot.
    /// </summary>
    public double FrameDtNow { get; set; }
    /// <summary>
    /// <c>[game+96]</c> from
    /// <c>004189DC</c> <c>fstp</c>
    /// of <c>009E1BC0</c> at
    /// <c>004189C2</c> entry.
    /// </summary>
    public double GamePlus96 { get; private set; }
    /// <summary>
    /// <c>004166E2</c>: first-seen
    /// slot clock is 0 (<c>0x122ED70</c>),
    /// so the <c>fcomp</c> clamp takes
    /// <c>009E1BC0</c>, then
    /// <c>fsub [game+96]</c>. Host
    /// sticky 0 is leftover.
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
    /// <summary>
    /// <c>[game+90444]</c> after
    /// <c>004168DC</c>
    /// <c>009E2C80</c>
    /// <c>ENG_ARIAL_18</c>.
    /// Not frontend type-6.
    /// </summary>
    public string? GameFontFace { get; private set; }
    /// <summary>
    /// <c>[0x13B8A54]</c> after
    /// <c>004CDB10</c>
    /// <c>00A39010</c>.
    /// Static ctor already
    /// <c>0121A630</c>
    /// <c>00A38500</c>.
    /// Not a spoken line.
    /// </summary>
    public bool SubtitledSymbolsRegistered { get; private set; }
    public string? SubtitledSymbolPath { get; private set; }
    /// <summary>
    /// <c>[0x13B8A28]</c> after
    /// <c>004CD670</c>
    /// <c>0099EFE0</c>.
    /// Not a spoken line.
    /// </summary>
    public bool ConversationAttitudesBound { get; private set; }
    /// <summary>
    /// <c>[game+28]</c> after
    /// <c>0041732A</c>
    /// <c>00BFEA1A(44)</c>
    /// <c>0044A3B0</c>
    /// <c>004193A0</c>.
    /// Not Create Players.
    /// </summary>
    public bool PlayerOwnerPresent { get; private set; }
    /// <summary>
    /// <c>[game+40]</c> after
    /// <c>00417418</c>
    /// <c>00BFEA1A(0x100)</c>
    /// <c>00434E10</c>
    /// <c>0041940C</c>.
    /// Not a region and not HUD.
    /// </summary>
    public bool DisplayPresent { get; private set; }
    public int DisplayPlus4 { get; private set; }
    public bool DisplayPlus8Game { get; private set; }
    public bool DisplayPlus12Owner { get; private set; }
    public bool DisplayPlus16Manager { get; private set; }
    public bool DisplayPlus20GraphicBank { get; private set; }
    public bool DisplayPlus24MeshBank { get; private set; }
    public bool DisplayPlus248World { get; private set; }
    public IReadOnlyList<string> PlayerOwnerHeroSwap =>
        _playerOwnerHeroSwap;
    private string[] _playerOwnerHeroSwap = [];
    public IReadOnlyList<string> ConversationAttitudeTable0 =>
        _conversationAttitudeTable0;
    public IReadOnlyList<string> ConversationAttitudeTable1 =>
        _conversationAttitudeTable1;
    public IReadOnlyList<string> ConversationAttitudeTable2 =>
        _conversationAttitudeTable2;
    private string[] _conversationAttitudeTable0 = [];
    private string[] _conversationAttitudeTable1 = [];
    private string[] _conversationAttitudeTable2 = [];
    public bool FrontendMenuConstructed { get; private set; }
    public int FrontendRootType { get; private set; }
    public int FrontendChildCount { get; private set; }
    public float FrontendScaleX { get; private set; }
    public float FrontendScaleY { get; private set; }
    /// <summary>
    /// TEMPORARY CPU blit dump of dest quads.
    /// Not the Present path. Present is
    /// <see cref="FrontendBatch"/>.
    /// </summary>
    public byte[]? FrontendPresentRgba { get; private set; }
    public int FrontendPresentWidth { get; private set; }
    public int FrontendPresentHeight { get; private set; }
    public FrontendSubmitBatch? FrontendBatch { get; private set; }
    private readonly List<FrontendDx9DrawRecord> _frontendDx9Records = [];
    private readonly List<FrontendDx9DrawRecord> _frontendCursorRecords = [];
    private readonly List<GpuTexture> _frontendDx9Textures = [];
    private readonly Dictionary<string, int> _frontendTextureIndex =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<FrontendTextLayoutKey, List<FrontendTextDraw.GlyphQuad>>
        _frontendTextLayouts = [];
    private string? _frontendCaretSource;
    private string? _frontendCaretText;
    private int _frontendCaretCursor = -1;
    private readonly Dictionary<List<FrontendWidget>, FrontendLayoutScratch>
        _frontendLayoutScratch = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<List<FrontendWidget>, int[]> _frontendRecordOrders =
        new(ReferenceEqualityComparer.Instance);
    public IReadOnlyList<FrontendWidget> FrontendWidgets => _frontendWidgets;
    /// <summary>
    /// Frontend virtual-space pointer
    /// (same space as dest / hit rects).
    /// </summary>
    public float FrontendPointerX { get; private set; }
    public float FrontendPointerY { get; private set; }

    /// <summary>
    /// Mouse position in dest space
    /// (<c>0055BF10</c> vtbl+64).
    /// </summary>
    public void SetFrontendPointer(float x, float y)
    {
        FrontendPointerX = x;
        FrontendPointerY = y;
    }
    private List<FrontendWidget> _frontendWidgets = [];
    /// <summary>
    /// <c>[ui+84]</c> slot id →
    /// widget tree. Native
    /// <c>00595222</c> /
    /// <c>0059A0C4</c> walk every
    /// non-null value in key order.
    /// Current
    /// <see cref="_frontendWidgets"/>
    /// is the switched screen
    /// (input).
    /// </summary>
    private readonly Dictionary<int, List<FrontendWidget>> _frontendSlotTrees = [];
    private int[] _frontendResidentSlotKeys = [];
    private readonly List<List<FrontendWidget>> _frontendResidentTrees = [];
    /// <summary>
    /// <c>[ui+84]</c> keys walked
    /// last tick/draw, in-order.
    /// </summary>
    public IReadOnlyList<int> FrontendResidentSlots { get; private set; } = [];
    /// <summary>
    /// <c>[ui+32].back()</c> slot.
    /// <c>00596763</c> calls
    /// <c>vtbl+192</c>(6) on the
    /// old back, then pushes the
    /// new widget.
    /// </summary>
    public int FrontendCurrentSlot { get; private set; }
    private readonly List<int> _frontendSubmitCounts = [];

    private sealed class FrontendLayoutScratch(int count)
    {
        public FrontendDest[] Dests { get; } = new FrontendDest[count];
        public int[] SiblingCounts { get; } = new int[count];
    }
    private FrontendSpriteBank? _frontendSprites;
    private FontBank? _frontendFonts;
    /// <summary>
    /// First-seen <c>005955AB</c> is
    /// empty (same enumerator
    /// <c>005958F5</c> skipped).
    /// </summary>
    public int FrontendProfileCount { get; set; }
    /// <summary>
    /// <c>[ui+160]</c>. Ctor 0.
    /// <c>00595845</c> writes 1.
    /// </summary>
    public bool FrontendUiArmed { get; private set; }
    /// <summary>
    /// <c>[ui+100]</c> from
    /// <c>00595845</c>.
    /// </summary>
    public bool FrontendUi100 { get; private set; }
    /// <summary>
    /// <c>[ui+96]</c> after
    /// <c>00596917</c>. First
    /// object has <c>+4=+5=0</c>
    /// so this tick still skips
    /// <c>0059899A</c>.
    /// </summary>
    public bool FrontendUi96Present { get; private set; }
    public bool FrontendUi96Accept { get; private set; }
    public bool FrontendUi96Armed { get; private set; }
    public bool FrontendEditBoxBound { get; private set; }
    /// <summary>
    /// <c>00851770</c> seeds via
    /// <c>004069E0</c>. Game
    /// singleton 0 and no text
    /// bank → UTF-16
    /// <c>0x122DE80</c> "Default".
    /// </summary>
    public string FrontendEditBoxName { get; private set; } = "";
    private int _editBoxCursor;
    /// <summary>
    /// <c>004067C0</c> /
    /// <c>00999AB0</c> writable
    /// first-seen.
    /// </summary>
    public bool FrontendCanCreateProfile { get; set; } = true;
    public GameBin? FrontendDefs { get; private set; }
    public bool FrontendDefFound { get; private set; }
    public string? FrontendDefTypeName { get; private set; }
    public bool FrontendType22HandlerRegistered { get; private set; }
    public bool FrontendEnqueueRan { get; private set; }
    public bool FrontendWidgetTickRan { get; private set; }
    public bool FrontendDestLayoutRan { get; private set; }
    public bool FrontendInstanceSubmitRan { get; private set; }
    public IReadOnlyList<string> FrontendMenuLabels =>
        FrontendMenuItems.Select(i => i.Label).ToList();
    public IReadOnlyList<int> GameTickTypes => _tickTypes;
    /// <summary>
    /// <c>[game+164]</c> count after
    /// <c>009F1720</c>+<c>009F16F0</c>.
    /// </summary>
    public int TickListCount { get; private set; }
    /// <summary>
    /// Appended record+0
    /// (<c>+9836</c> after inc).
    /// </summary>
    public int TickRecordWatermark { get; private set; }
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
    public bool ScriptPumpRan { get; private set; }
    public int ScriptPumpWalked { get; private set; }
    public bool EventPumpRan { get; private set; }
    public int EventPumpWalked { get; private set; }
    public int PlayerSlotTicks { get; private set; }
    public bool DisplayListenerPumped { get; private set; }
    public bool EnvironmentTicked { get; private set; }
    public float EnvironmentTime { get; private set; }
    public bool BulletTimeTicked { get; private set; }
    public bool ConversationTicked { get; private set; }
    public int ConversationWalked { get; private set; }
    public bool ThingManagerFlushed { get; private set; }
    public int ThingManagerFlushedCount { get; private set; }
    public bool OpinionTicked { get; private set; }
    public bool PlayerGuiTicked { get; private set; }
    public bool AtmosTicked { get; private set; }
    public bool SpeechGainTicked { get; private set; }
    public bool DisplayActiveApplyRan { get; private set; }
    public bool FollowSpringRan { get; private set; }
    public bool SubjectFillNoted { get; private set; }
    public QuestFile? Quests { get; private set; }
    public ScriptRuntime? Runtime { get; private set; }
    public IReadOnlyList<string> ActivatedQuests => _activatedQuests;
    /// <summary>
    /// <c>CWorld+172</c>: <c>AddQuest</c>
    /// TRUE names from
    /// <c>FinalAlbion.qst</c> then
    /// <c>GlobalQuests.qst</c>. Not WLD
    /// <c>START_INITIAL_QUESTS</c>.
    /// </summary>
    public IReadOnlyList<string> WorldPlus172 => _worldPlus172;
    /// <summary>
    /// <c>CWorld+184</c>: every
    /// <c>AddQuest</c> name (TRUE and
    /// FALSE) from both QST files.
    /// </summary>
    public IReadOnlyList<string> WorldPlus184 => _worldPlus184;
    /// <summary>
    /// QuestManager <c>+44</c>: every
    /// <c>AddQuest</c> name via
    /// <c>004B2850</c>. Gate table for
    /// <c>004B00C0</c>.
    /// </summary>
    public IReadOnlyList<string> QuestManagerPlus44 => _questManagerPlus44;
    /// <summary>
    /// <c>00CE6CF0</c> names inserted at
    /// <c>0x13BAE44</c> via
    /// <c>008A9DB0</c> / <c>008AE660</c>.
    /// </summary>
    public IReadOnlyList<string> GameflowStateSlots => _gameflowStates;
    public IReadOnlyList<string> GameflowWatchers => _gameflowWatchers;
    public int GameflowState { get; private set; }
    public string? GameflowYieldQuest { get; private set; }
    public int EventPosts { get; private set; }
    /// <summary>
    /// Persist <c>PlayerRegionName</c>. Empty on
    /// no-save New Game. Non-empty takes
    /// <c>00487C20</c> instead of <c>00501450</c>.
    /// </summary>
    public string? PlayerRegionName { get; set; }
    public IReadOnlyList<int> PendingLoadIndices => _loadQueue;
    public IReadOnlyList<string> ActivatedMaps => _activatedMaps;
    public int OpenStaticMapsMode { get; private set; }
    /// <summary>
    /// <c>0049DDD0</c> name copied onto
    /// map-manager <c>+48</c> before
    /// <c>00B42750(1)</c>.
    /// </summary>
    public string? StaticMapFileName { get; private set; }
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
    /// <summary>
    /// Neutral DX9 device. Shadow-records
    /// reconstructed calls. Compatibility
    /// Present stays on
    /// <see cref="IEngineHost"/> until a
    /// unit is NativeSemantic.
    /// </summary>
    public IDirect3DDevice9? Device { get; set; }
    /// <summary>
    /// Unproven capabilities stay false.
    /// Attaching <see cref="Device"/> is
    /// not migration complete.
    /// </summary>
    public Dx9SubmitCapabilities SubmitCapabilities { get; set; }
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

    private bool _retailLoopStarted;
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
    private readonly List<string> _worldPlus172 = [];
    private readonly List<string> _worldPlus184 = [];
    private readonly List<string> _questManagerPlus44 = [];
    private readonly List<string> _gameflowStates = [];
    private readonly List<string> _gameflowWatchers = [];
    private readonly List<uint> _submittedLayers = [];
    private GameBin? _defs;
    private LevelLibrary? _levels;
    public MeshBank Meshes { get; } = new();

    public static int CreateDeviceBehaviorFlags(bool hardwareTnl) =>
        hardwareTnl ? CreateDeviceHardwareFlags : CreateDeviceSoftwareFlags;

    /// <summary>
    /// Dump listing from PE <c>00401067</c>:
    /// CRT → WinMain <c>00403480</c> →
    /// named stages in <c>00402510</c>
    /// through <c>00412F90</c>. Does not
    /// enter <c>00DBDE40</c>. Native
    /// RunModes blocks; host returns here
    /// and <see cref="Pump"/> steps the
    /// retail loop.
    /// </summary>
    public void Bootstrap(GameInstall? install)
    {
        BootstrapUntilGraphics(install);
        CompleteRetailLoop();
    }

    /// <summary>
    /// CRT through Setup library
    /// <c>009A6610</c> options. Window
    /// size / <see cref="DeviceWindowed"/>
    /// are ready. CreateDevice /
    /// Present wait for
    /// <see cref="Device"/>.
    /// </summary>
    public void BootstrapUntilGraphics(GameInstall? install)
    {
        var boot = System.Diagnostics.Stopwatch.StartNew();
        Install = install;
        RunCrtStartup();
        RunWinMain();
        Timing.Add("bootstrap", boot.Elapsed.TotalMilliseconds, Stage.ToString());
    }

    /// <summary>
    /// <c>004022B0</c> / <c>00412F90</c>
    /// after the host attached
    /// <see cref="Device"/>.
    /// </summary>
    public void CompleteRetailLoop()
    {
        if (_retailLoopStarted)
            return;
        _retailLoopStarted = true;
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
    }

    /// <summary>
    /// Native exclusive is D3D
    /// <c>![0x137544A]</c>. The Silk
    /// window starts windowed; Alt+Enter
    /// toggles OS fullscreen.
    /// </summary>
    public static bool HostExclusiveWindow(bool deviceWindowed, bool forceWindowed) =>
        !forceWindowed && !deviceWindowed;

    /// <summary>
    /// Frontend Present owner. Shadow
    /// while sprite/glyph DIPs are
    /// unproven. NativeSemantic only
    /// when both capabilities are set.
    /// </summary>
    public Dx9SubmitMode FrontendSubmitMode =>
        SubmitCapabilities.FrontendMode(Device is not null);

    /// <summary>
    /// NativeSemantic frontend Present
    /// is <see cref="Device"/>, not
    /// <see cref="IEngineHost.Present"/>.
    /// Device attached is not enough.
    /// </summary>
    public bool Dx9OwnsFrontendPresent =>
        FrontendSubmitMode == Dx9SubmitMode.NativeSemantic
        && Stage == EngineStage.Frontend;

    /// <summary>
    /// Listing <c>00401067</c> MSVCR71
    /// WinMainCRTStartup. Host does not
    /// run the CRT heap; it Notes the
    /// dump IAT/E8 then enters WinMain.
    /// </summary>
    private void RunCrtStartup()
    {
        Note(PeEntry, "ProcessEntry", "CRT", "WinMainCRTStartup");
        Stage = EngineStage.CrtStartup;
        Note(CrtSehFn, "ProcessEntry", "CRT", "0040138C");
        Note(GetModuleHandleIat, "ProcessEntry", "CRT", "GetModuleHandleA");
        Note(SetAppTypeIat, "ProcessEntry", "CRT",
            $"__set_app_type({SetAppTypeGui})");
        Note(CrtStaticCtorsFn, "ProcessEntry", "CRT", "004012CE");
        Note(WinMainCallSite, "ProcessEntry", "CRT", "004011E7 call 00403480");
    }

    /// <summary>
    /// Listing <c>00403480</c>: alloca,
    /// OpenMutexW miss, CreateMutexW
    /// <c>Global\Fable</c>, zero scratch,
    /// <c>00402510</c>.
    /// </summary>
    private void RunWinMain()
    {
        Note(WinMain, "WinMain", "App", "00403480");
        Stage = EngineStage.WinMain;
        Note(WinMainAllocaFn, "WinMain", "App",
            $"00BFEA30 alloca 0x{WinMainAllocaSize:X}");
        Note(OpenMutexIat, "WinMain", "App",
            $"OpenMutexW {MutexName} 0x{MutexAccess:X} miss");
        Note(CreateMutexIat, "WinMain", "App",
            $"CreateMutexW {MutexName}");
        Note(WinMainZeroFn, "WinMain", "App",
            $"009D86B0 0x{WinMainScratchSize:X}");
        Note(BootstrapFn, "WinMain", "App", "00402510");
        RunNamedBootstrap();
    }

    private void RunNamedBootstrap()
    {
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
            if (name == "Parse Command Line")
            {
                Note(ParseCommandLineCtor, name, "Cmd", "00403B10");
                Note(ParseCommandLineScan, name, "Cmd", "00997510");
                Note(ParseCommandLineApply, name, "Cmd", "009974F0");
                ApplyUserstIni();
            }

            if (name == "Setup Basic install files")
                Note(SetupInstallCopyFn, name, "Install", "009D5240");
            if (name == "Setup Language")
                ApplyLanguageSettings();
            if (name == "Setup basic retail banks")
            {
                Note(BankManagerInitFn, name, "Bank",
                    "009A76D0 [0x13CA79C]");
                RegisterRetailBankTable(install: Install);
            }

            if (name == "Setup library")
            {
                ConstructLibrary();
                Note(FrontendDisplayHelperFn, name, "Window",
                    "00404A80 [engine+148]");
                Note(LibraryPostWindowFn, name, "Window", "00405650");
            }

            _completed.Add(name);
        }
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
            PumpFrontendFrame(dt);
            if (RetailNewGameFlag)
            {
                RequestNewGame();
                EnterGame();
            }

            if (!Dx9OwnsFrontendPresent)
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
            if (GamePumpFirstDone)
                FrameDtNow += dt;
            var presents = GamePresentCount;
            PumpGame();
            // 00435F70 / 009BEEB0 after 004AEA70=1.
            // Maps open / WorldSubmitted is not
            // a native gate on this frame.
            if (GamePresentCount > presents)
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
        TickType11Type38Hover(_frontendWidgets);
        foreach (var (type, key) in Input.Applied)
        {
            var action = FrontendInputMap.ActionFromEvent(type, key);
            if (type == EngineInput.Type15 ||
                action == FrontendInputMap.ActionType15)
            {
                ApplyEditBoxWmChar(key);
                continue;
            }

            if (action == FrontendInputMap.ActionType4)
                ArmType34Widgets();
            var mapped = action is int act
                ? FrontendInputMap.MessageFromWidgets(act, _frontendWidgets)
                : null;
            if (action == FrontendInputMap.ActionType6)
            {
                mapped ??= FrontendInputMap.MessageFromPlus228List(_frontendWidgets);
                UnarmType34Widgets();
            }
            if (mapped is not int msg)
                continue;
            DispatchFrontendMessage(msg);
            return;
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
        var present = PresentDestFromViewport(
            ViewportX, ViewportY, ViewportWidth, ViewportHeight,
            BackBufferWidth, BackBufferHeight);
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
            PlayAviClearArgb,
            FrontendPresentRgba,
            FrontendPresentWidth,
            FrontendPresentHeight,
            present.X0,
            present.Y0,
            present.X1,
            present.Y1,
            FrontendBatch);
    }

    /// <summary>
    /// <c>009BEEB0</c> Present of the
    /// <c>009BEF80</c> viewport over the
    /// <c>00403079</c> backbuffer. Not a
    /// host dest constant.
    /// </summary>
    public static (float X0, float Y0, float X1, float Y1) PresentDestFromViewport(
        int x, int y, int width, int height, int backBufferWidth, int backBufferHeight)
    {
        if (backBufferWidth <= 0 || backBufferHeight <= 0)
            return (0, 0, 1, 1);
        return (
            x / (float)backBufferWidth,
            y / (float)backBufferHeight,
            (x + width) / (float)backBufferWidth,
            (y + height) / (float)backBufferHeight);
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
        Note(FrontendSpriteLayerCtorFn, "InitEngine", "Engine",
            "00B4AC10 00B4ABB0 00B8FAD0");
        Note(FrontendSpriteHandlerCtorFn, "InitEngine", "Engine",
            $"00BAD040 {FrontendSpriteShader} vtbl 0x{FrontendSpriteHandlerVtbl:X}");
        Note(FrontendSpriteTypeListFn, "InitEngine", "Engine",
            $"00BB1640 types 0x{FrontendSpriteType:X}/0x{FrontendSpriteTypeAlt:X}");
        FrontendType22HandlerRegistered = true;
        Note(0x0042EF6F, "InitFrontend", "Frontend", "Init frontend");
        Note(FrontendHelperCtor, "InitFrontend", "Frontend",
            $"0042DB40 size {FrontendHelperSize} vtbl 0x{FrontendHelperVtbl:X}");
        Note(ClearColorFn, "InitFrontend", "D3D9", "009D8CF0 clear");
        Note(PresentFn, "InitFrontend", "D3D9", "009BEEB0 Present");
        IssueAfterAviPresent();
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
    /// <c>0042E98F</c> bind: <c>00595582</c>,
    /// <c>005958F5</c>, <c>00598A1C(0)</c>.
    /// First <c>0041DB1D</c> is Press Start,
    /// not <c>0059899A</c>.
    /// </summary>
    public void InitFrontendUi()
    {
        if (FrontendUiPresent)
            return;
        Note(FrontendUiGet, "Frontend", "UI",
            "00595582 [0x13B8B5C] size 0xE0");
        Note(FrontendUiCtor, "Frontend", "UI",
            "005953E2 vtbl 012521A8");
        Note(RetailAfterAviFn, "Frontend", "UI",
            "0042E98F +180 UI+28=pump [UI+192]=1");
        Note(FrontendProfileBindFn, "Frontend", "UI",
            "005958F5 005955AB empty skip");
        Note(FrontendPressStartAttachFn, "Frontend", "UI",
            "00598A1C arg=0 skip MEDIA_PLAYER_ERROR " + FrontendPressStartMenu);
        Note(InputActionGetter, "Frontend", "UI", "0041E5F2");
        Note(FrontendWidgetFactoryFn, "Frontend", "UI",
            "0041DB1D " + FrontendPressStartMenu +
            $" slot 0x{FrontendPressStartSlot:X}");
        ResolveFrontendDef(FrontendPressStartMenu);
        AttachPressStartWidgets();
        Note(FrontendInitSelectFn, "Frontend", "UI",
            "005952C3 vtbl+192(5) [ui+32].back()");
        SelectFrontendState(FrontendPressStartSlot, 5);
        Note(FrontendDefResolveFn, "Frontend", "UI",
            "0042AEDA 009AD9E0 [def+60] switch 0041D7F8");
        Note(FrontendWidgetConstructFn, "Frontend", "UI",
            FrontendDefFound
                ? $"0041D21B [def+60] type {FrontendRootType}"
                : "0041D21B 009AD410 miss");
        if (FrontendRootType == FrontendPressStartType)
        {
            Note(FrontendPressStartCtorFn, "Frontend", "UI",
                $"0054E3D0 type {FrontendPressStartType} vtbl 0x{FrontendPressStartVtbl:X} not 0041B800");
        }
        else
        {
            Note(FrontendWidgetType0Ctor, "Frontend", "UI",
                $"0041B800 vtbl 0x{FrontendWidgetVtbl:X} +{FrontendWidgetBlendOffset}={FrontendWidgetBlendDefault}");
        }

        Note(FrontendWidgetPostCtorFn, "Frontend", "UI",
            $"0041AC20 vtbl+{FrontendWidgetFontListVtbl} 0x{FrontendWidgetFontListFn:X}");
        FrontendWidgetBlend = FrontendWidgetBlendDefault;
        FrontendWidgetFont = 0;
        FrontendWidgetTexture = 0;
        ApplyFrontendScaleInit();
        LayoutFrontendWidgets();
        Note(FrontendWidgetPostCtorFn, "Frontend", "UI",
            FrontendRootType == FrontendPressStartType
                ? $"005331A0 children={FrontendChildCount}"
                : "0041AC20 [+376]=0 skip dest");
        Note(FrontendWidgetDrawFn, "Frontend", "UI",
            $"0041AFA0 dest {FrontendWidgetDestX0},{FrontendWidgetDestY0},{FrontendWidgetDestX1},{FrontendWidgetDestY1}");
        Note(FrontendWidgetMessageNoopFn, "Frontend", "UI",
            $"0052F040 ret 4 msg 0x{FrontendPressStartMessage:X} vtbl+{FrontendWidgetMessageVtbl}");
        Note(FrontendPressStartAttachFn, "Frontend", "UI",
            $"00598A1C msg 0x{FrontendPressStartMessage:X} vtbl+{FrontendWidgetMessageVtbl} slot 0x{FrontendPressStartSlot:X}");
        Note(FrontendUiCtor, "Frontend", "UI",
            $"00595422 [ui+{FrontendUi96Offset}]=0 [ui+{FrontendUiArmedOffset}]=0");
        FrontendMenuRoot = FrontendPressStartMenu;
        FrontendMenuConstructed = true;
        FrontendUiPresent = true;
    }

    /// <summary>
    /// <c>0042EC7C</c> inner frontend frame.
    /// Present is <c>009BEEB0</c> inside
    /// <c>0042DF9E</c>, same device Present
    /// the Vulkan path already translates.
    /// </summary>
    public void PumpFrontendFrame() => PumpFrontendFrame(0f);

    public void PumpFrontendFrame(float dt)
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
        // 0042E3EE then 0042DC94: 0xE5
        // lands before 00599E3F so
        // 00595845 and 00596917 are
        // the same frame. Hover
        // 0055BF10 is dest AABB of
        // the current pointer, not
        // TypeMouse-only.
        MaybeActivateNewGameFromInput();
        Note(FrontendUpdateFn, "Frontend", "UI", "0042DC94");
        Note(FrontendUiTickFn, "Frontend", "UI", "00599E3F");
        Note(FrontendRecordZeroFn, "Frontend", "Render",
            $"0042FA30 zero {FrontendRecordSize}");
        Note(FrontendRecordFillFn, "Frontend", "Render", "0042DBFA");
        Note(FrontendDrawFn, "Frontend", "Render", "0042DF9E");
        Note(ClearColorFn, "Frontend", "D3D9", "009D8CF0 clear");
        Note(BeginSceneFn, "Frontend", "D3D9", "009BEF20 BeginScene");
        Note(FrontendUiGet, "Frontend", "UI", "00595582");
        TickFrontendWidgets(dt);
        DrawFrontendWidgets();
        Note(InputActionGetter, "Frontend", "Input", "0041E5F2");
        FlushFrontendDisplay();
        Note(FrontendDisplayHelperFn, "Frontend", "D3D9",
            $"00404A80 0x{FrontendDisplaySingletonVa:X}");
        ApplyFrontendDisplay();
        FlushFrontendDisplay();
        Note(EndSceneFn, "Frontend", "D3D9", "009BEF50 EndScene");
        Note(PresentFn, "Frontend", "D3D9", "009BEEB0 Present");
        IssueFrontendFramePresent();
        FrontendFrameCount++;
        FrontendPresentCount++;
    }

    /// <summary>
    /// <c>0042EFF7</c> <c>009D8CF0</c>
    /// flags 0 → 7, color
    /// <c>0xFF000000</c>, then
    /// <c>009BEEB0</c>. No BeginScene.
    /// </summary>
    private void IssueAfterAviPresent()
    {
        var device = Device;
        if (device is null)
            return;
        device.Clear(
            Dx9Clear.WhenArgZero,
            FrontendDx9Submit.FrontendFrame().ClearColorArgb,
            1f,
            0);
        device.Present();
    }

    /// <summary>
    /// <c>0042DF9E</c>: <c>009D8CF0</c>
    /// then <c>009BEF20</c> vtbl+164
    /// BeginScene. Nonempty dest
    /// <c>00BAE2D0</c> → <c>00A0AEA0</c>
    /// DIPUP. Zero dest
    /// <c>00BADB36</c> skip. Glyphs
    /// <c>00AB7C20</c> → <c>00A0ABE0</c>.
    /// Empty <c>009DA9F0</c> is not
    /// “no UI draws.” Do not emit
    /// buffered <c>DrawIndexedPrimitive(0,…)</c>.
    /// </summary>
    private void IssueFrontendFramePresent()
    {
        var device = Device;
        if (device is null)
            return;
        var frame = FrontendDx9Submit.FrontendFrame();
        device.Clear(Dx9Clear.WhenArgZero, frame.ClearColorArgb, 1f, 0);
        device.BeginScene();
        if (device is VulkanDx9Device vk)
            vk.BindFrontendTextures(_frontendDx9Textures);
        device.SetViewport(new Dx9Viewport(
            0, 0, BackBufferWidth > 0 ? BackBufferWidth : DisplayDefaultWidth,
            BackBufferHeight > 0 ? BackBufferHeight : DisplayDefaultHeight,
            frame.ViewportMinZ, frame.ViewportMaxZ));
        FrontendDx9Submit.IssueRecoveredDraws(device, _frontendDx9Records);
        device.EndScene();
        device.Present();
    }

    /// <summary>
    /// <c>00599E3F</c> after the
    /// <c>ui+96</c> skip: walk
    /// <c>[ui+84]</c>,
    /// <c>[node+20].vtbl+4</c>
    /// <c>0052C7E0</c>(dt) →
    /// <c>00531EC0</c> dest layout.
    /// </summary>
    private void TickFrontendWidgets(float dt)
    {
        Note(FrontendUiTickFn, "Frontend", "UI",
            $"00599E3F [ui+{FrontendWidgetListOffset}] vtbl+{FrontendWidgetTickVtbl}");
        if (!FrontendMenuConstructed)
            return;
        if (FrontendUiArmed)
            BindNewProfileFromArmedTick();
        if (FrontendUi96Present && FrontendUi96Armed && !FrontendUi96Accept)
            CommitNewProfileFromArmedEdit();
        Note(FrontendWidgetTickFn, "Frontend", "UI",
            $"0052C7E0 vtbl+{FrontendWidgetTickVtbl} 0122F5D4");
        Note(FrontendDestLayoutFn, "Frontend", "UI",
            $"00531EC0 vtbl+{FrontendDestScaleVtbl} 0052F5C0 then vtbl+{FrontendDestOriginVtbl} 0052FFD0");
        Note(FrontendDestScaleFn, "Frontend", "UI",
            $"0052F5C0 +264 from +92/+272={FrontendScaleX}");
        Note(FrontendDestOriginFn, "Frontend", "UI",
            "0052FFD0 +248 from +52/+60");
        RebuildResidentTrees();
        for (var treeIndex = 0; treeIndex < _frontendResidentTrees.Count; treeIndex++)
        {
            var tree = _frontendResidentTrees[treeIndex];
            TickFrontendColourTransitions(tree, dt);
            TickSwappingStateComponents(tree, dt);
            LayoutFrontendWidgets(tree);
            TickType11Type38Hover(tree);
        }

        FrontendWidgetTickRan = true;
        FrontendDestLayoutRan = true;
        Note(FrontendWidgetNextFn, "Frontend", "UI", "004292C0");
    }

    /// <summary>
    /// Type 18 <c>00547380</c>. The first completed tick primes
    /// <c>+360</c>; later completed ticks compare elapsed time with the
    /// current <c>+348</c> entry and select the next key cyclically. A state
    /// selection clears completion; even a zero-duration style must complete
    /// on the following base tick before <c>0041C5E0</c> raises another edge.
    /// </summary>
    private void TickSwappingStateComponents(List<FrontendWidget> tree, float dt)
    {
        // 0052CF40(6) is the slot leave state. The detached menu remains
        // resident for draw parity but no longer receives its component tick.
        if (tree.Count == 0 || tree[0].State == 6)
            return;
        for (var i = 0; i < tree.Count; i++)
        {
            var widget = tree[i];
            if (widget.Type != FrontendWidgetType.Swap ||
                widget.SwappingStates is not { Count: > 0 } keys)
                continue;

            var elapsed = widget.SwapStateElapsed + Math.Max(0f, dt);
            if (!widget.SwapTickPrimed)
            {
                // vtbl+196 remains false while any forwarded child style is
                // interpolating. 0041C5E0 only raises the completion edge once
                // the whole subtree has finished.
                if (SubtreeHasActiveColourTransition(tree, i))
                    continue;
                tree[i] = widget with { SwapTickPrimed = true, SwapStateElapsed = 0f };
                continue;
            }

            var current = -1;
            var currentState = widget.SwapCurrentState == int.MinValue
                ? keys[0]
                : widget.SwapCurrentState;
            for (var k = 0; k < keys.Count; k++)
            {
                if (keys[k] == currentState)
                {
                    current = k;
                    break;
                }
            }
            if (current < 0)
                continue;

            var dwell = widget.SwappingTimes is { } durations &&
                (uint)current < (uint)durations.Count
                ? Math.Max(0f, durations[current])
                : 0f;
            if (elapsed < dwell)
            {
                tree[i] = widget with { SwapStateElapsed = elapsed };
                continue;
            }

            var nextState = keys[(current + 1) % keys.Count];
            ForwardSelectState(tree, i, nextState);
            tree[i] = tree[i] with
            {
                // 0052CF40 clears completion. 0052C7E0 applies an immediate
                // style on the next tick and 0041C5E0 then primes +360 again;
                // it cannot select another state on the same tick.
                SwapTickPrimed = false,
                SwapStateElapsed = 0f,
                SwapCurrentState = nextState,
            };
        }
    }

    private static void TickFrontendColourTransitions(List<FrontendWidget> tree, float dt)
    {
        var step = Math.Max(0f, dt);
        for (var i = 0; i < tree.Count; i++)
        {
            var widget = tree[i];
            if (!widget.ColourTransitionActive)
                continue;
            var elapsed = widget.ColourTransitionElapsed + step;
            var duration = widget.ColourTransitionDuration;
            var t = duration > 0f ? Math.Clamp(elapsed / duration, 0f, 1f) : 1f;
            var colour = LerpPackedColour(widget.ColourTransitionFrom,
                widget.ColourTransitionTo, t);
            tree[i] = widget with
            {
                Colour = colour,
                ColourTransitionElapsed = elapsed,
                ColourTransitionActive = t < 1f,
            };
        }
    }

    private static bool SubtreeHasActiveColourTransition(
        IReadOnlyList<FrontendWidget> tree, int root)
    {
        for (var i = 0; i < tree.Count; i++)
        {
            var current = i;
            while ((uint)current < (uint)tree.Count)
            {
                if (current == root)
                {
                    if (tree[i].ColourTransitionActive)
                        return true;
                    break;
                }
                current = tree[current].ParentIndex;
            }
        }
        return false;
    }

    private static uint LerpPackedColour(uint from, uint to, float t)
    {
        static uint Channel(uint a, uint b, float amount) =>
            (uint)Math.Clamp((int)MathF.Round(a + (b - (float)a) * amount), 0, 255);
        var a = Channel(from >> 24, to >> 24, t);
        var r = Channel((from >> 16) & 0xFF, (to >> 16) & 0xFF, t);
        var g = Channel((from >> 8) & 0xFF, (to >> 8) & 0xFF, t);
        var b = Channel(from & 0xFF, to & 0xFF, t);
        return (a << 24) | (r << 16) | (g << 8) | b;
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
        FrontendEnqueueRan = false;
        if (!FrontendMenuConstructed)
            return;
        var drawn = 0;
        var any = false;
        RebuildResidentTrees();
        for (var treeIndex = 0; treeIndex < _frontendResidentTrees.Count; treeIndex++)
        {
            var tree = _frontendResidentTrees[treeIndex];
            any = true;
            DrawSlotTree(tree, ref drawn);
        }

        if (!any)
        {
            Note(FrontendWidgetDrawFn, "Frontend", "UI",
                $"0041AFA0 vtbl+{FrontendWidgetDrawVtbl} 0122F5D4");
            QueueFrontend2dRecord(null);
            drawn = 1;
        }

        FrontendWidgetsDrawn = Math.Max(1, drawn);

        Note(FrontendWidgetNextFn, "Frontend", "UI", "004292C0");
        CompositeFrontendPresent();
    }

    /// <summary>
    /// <c>00530260</c> <c>vtbl+8</c>: walk
    /// <c>+176</c>. Skip when
    /// <c>vtbl+420</c> (<c>+302</c> bit 0)
    /// or the child is not visible.
    /// Type 5/10/12/16/18 recurse; leaves
    /// call <c>0041AFA0</c> /
    /// <c>0054EF00</c>.
    /// </summary>
    private void DrawSlotTree(IReadOnlyList<FrontendWidget> tree, ref int drawn)
    {
        if (tree.Count == 0)
            return;
        var rootType = tree[0].Type;
        if (FrontendWidgetType.DrawsChildList(rootType))
        {
            Note(FrontendContainerDrawFn, "Frontend", "UI",
                $"00530260 vtbl+{FrontendWidgetDrawVtbl} +{FrontendChildListOffset} n={Math.Max(0, tree.Count - 1)}");
            for (var root = 0; root < tree.Count; root++)
            {
                if (tree[root].ParentIndex < 0)
                    DrawContainerWalk(tree, root, ref drawn);
            }
            return;
        }

        Note(FrontendWidgetDrawFn, "Frontend", "UI",
            $"0041AFA0 vtbl+{FrontendWidgetDrawVtbl} 0122F5D4");
        QueueFrontend2dRecord(null);
        drawn = Math.Max(drawn, tree.Count);
    }

    private void DrawContainerWalk(
        IReadOnlyList<FrontendWidget> tree, int index, ref int drawn)
    {
        if ((uint)index >= (uint)tree.Count)
            return;
        var widget = tree[index];
        if (!widget.Visible)
            return;
        drawn++;
        if (FrontendWidgetType.DrawsChildList(widget.Type))
        {
            for (var child = 0; child < tree.Count; child++)
            {
                if (tree[child].ParentIndex == index)
                    DrawContainerWalk(tree, child, ref drawn);
            }
            return;
        }

        if (widget.Type == FrontendWidgetType.Text)
        {
            Note(FrontendTextDrawFn, "Frontend", "UI",
                $"0054EF00 child {widget.Name} type {widget.Type} dest {widget.DestX0},{widget.DestY0},{widget.DestX1},{widget.DestY1}");
        }
        else
        {
            Note(FrontendWidgetDrawFn, "Frontend", "UI",
                $"00530260 child {widget.Name} type {widget.Type} dest {widget.DestX0},{widget.DestY0},{widget.DestX1},{widget.DestY1}");
        }

        QueueFrontend2dRecord(widget);
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
    private void QueueFrontend2dRecord(FrontendWidget? widget)
    {
        if (widget is { } leaf && leaf.Type == FrontendWidgetType.Text)
        {
            QueueType6Record(leaf);
            return;
        }

        var destX0 = widget?.DestX0 ?? FrontendWidgetDestX0;
        var destY0 = widget?.DestY0 ?? FrontendWidgetDestY0;
        var destX1 = widget?.DestX1 ?? FrontendWidgetDestX1;
        var destY1 = widget?.DestY1 ?? FrontendWidgetDestY1;
        var destW = destX1 - destX0;
        var destH = destY1 - destY0;
        var sibling = FrontendWidgetTexture != 0;
        var packer = sibling ? FrontendWidgetQueueSiblingFn : FrontendWidgetQueueFn;
        Note(packer, "Frontend", "UI",
            sibling
                ? $"0041BF60 type 0x{Frontend2dRecordType:X} [+380]"
                : $"0041BEB0 type 0x{Frontend2dRecordType:X} +{FrontendWidgetBlendOffset}={FrontendWidgetBlend}");
        Note(packer, "Frontend", "UI",
            $"[edx+{Frontend2dSubmitVtbl}] dest +{FrontendWidgetSubmitDestOffset:X} 0x{Frontend2dRecordBytes:X} {destX0},{destY0},{destX1},{destY1}");
        Note(FrontendEngineAllocFn, "Frontend", "UI",
            $"00B26340 size 0x{FrontendEngineObjectSize:X} vtbl 0x{FrontendEngineVtbl:X}");
        Note(FrontendSubmitFn, "Frontend", "UI",
            $"00B23BC0 engine vtbl+{Frontend2dSubmitVtbl} 012A0F3C → 00B324A0 [0x{FrontendSubmitSingletonVa:X}] type 0x{Frontend2dRecordType:X}");
        if (FrontendFrameCount == 0 && destW <= 0 && destH <= 0)
        {
            Note(FrontendSpriteFactoryFn, "Frontend", "UI",
                $"00BACFD0 type 0x{FrontendSpriteType:X} vtbl 0x{FrontendSpriteInstanceVtbl:X}");
            Note(FrontendSpriteSubmitFn, "Frontend", "UI",
                "00BAE2D0 VSHADER_2D_SPRITE 00987FE0 no 009DB700");
            Note(FrontendSubmitDispatchFn, "Frontend", "UI",
                $"00B324A0 type 0x{Frontend2dRecordType:X} dest+4=0 00BACFD0+00BAE2D0");
        }
        else if (destW <= 0 && destH <= 0)
        {
            FrontendInstanceSubmitRan = true;
            Note(FrontendSpriteInstanceSubmitFn, "Frontend", "UI",
                "00BAD8A0 [rec+32]=0 [rec+64]=0 00BADB36 ret 8 no 009DB700");
            Note(FrontendSubmitDispatchFn, "Frontend", "UI",
                $"00B324A0 dest+4 set 00BAD8A0 vtbl+20 0x{FrontendSpriteInstanceVtbl:X}");
        }
        else
        {
            FrontendEnqueueRan = true;
            FrontendInstanceSubmitRan = true;
            Note(FrontendSpriteInstanceSubmitFn, "Frontend", "UI",
                $"00BAD8A0 dest {destX0},{destY0},{destX1},{destY1} 00BAE2D0 no 009DB700");
            Note(FrontendSubmitDispatchFn, "Frontend", "UI",
                $"00B324A0 dest nonempty 00BAD8A0 00BAE2D0 0x{FrontendSpriteInstanceVtbl:X}");
        }
        Frontend2dLastType = Frontend2dRecordType;
        Frontend2dLastPacker = packer;
        Frontend2dLastSubmitVtbl = Frontend2dSubmitVtbl;
        Frontend2dRecordsQueued++;
    }

    /// <summary>
    /// Type-6 <c>0054EF00</c> packs two records
    /// via <c>00543910</c>, each type
    /// <c>0x27</c> size 64: black underlay then
    /// widget colour. Not
    /// <c>0041BEB0</c> / <c>0x22</c>.
    /// </summary>
    private void QueueType6Record(FrontendWidget widget)
    {
        var pack = FrontendDx9Submit.RecordForWidget(widget.Type);
        Note(FrontendTextDrawFn, "Frontend", "UI",
            $"0054EF00 dest {widget.DestX0},{widget.DestY0},{widget.DestX1},{widget.DestY1}");
        Note(pack.Packer, "Frontend", "UI",
            $"00543910 type 0x{pack.Type:X} size {pack.Bytes}");
        Frontend2dLastType = pack.Type;
        Frontend2dLastPacker = pack.Packer;
        Frontend2dRecordsQueued += FrontendTextDraw.Type6PassCount;
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
        // 009DA9F0 drains +16020 only.
        // Nonempty widget dest is 00BAE2D0,
        // not 009DB700, so the display
        // queue stays empty first-seen.
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
        if (msg == FrontendPressStartMessage)
        {
            AcceptPressStartMessage();
            return;
        }

        if (msg == FrontendMainMenuMessage)
        {
            AttachFrontendMainMenu();
            return;
        }

        if (msg == FrontendAcceptProfileMessage)
        {
            AcceptNewProfileMessage();
            return;
        }

        if (msg != FrontendNewGameMessage)
            return;
        Note(FrontendNewGameApply, "Frontend", "UI",
            "0059A2DA [ui+28] vtbl+16");
        Note(RetailPlus8StoreFn, "Frontend", "UI",
            $"00430340 [retail+{RetailPlus8Offset}]=1");
        Note(FrontendNewGameThunk, "Frontend", "UI",
            $"00594F28 [retail+{RetailNewGameFlagOffset}]=1");
        RetailNewGameFlag = true;
    }

    /// <summary>
    /// <c>0059A238</c> msg <c>0xE5</c>
    /// → <c>00599D5C</c>. Empty
    /// <c>005955AB</c> is first-seen
    /// → <c>00595845</c>. One name
    /// → <c>0059899A</c> (not first).
    /// </summary>
    private void AcceptPressStartMessage()
    {
        Note(FrontendPressStartAcceptFn, "Frontend", "UI",
            "00599D5C 005955AB");
        Note(FrontendProfileEnumFn, "Frontend", "UI",
            $"005955AB count={FrontendProfileCount}");
        if (FrontendProfileCount == 0)
        {
            Note(FrontendNoProfileFn, "Frontend", "UI",
                $"00595845 [ui+{FrontendUiArmedOffset}]=1 [ui+{FrontendUi100Offset}]=1");
            FrontendUiArmed = true;
            FrontendUi100 = true;
            return;
        }

        if (FrontendProfileCount == 1)
        {
            AttachFrontendMainMenu();
            return;
        }

        Note(0x00597B20, "Frontend", "UI",
            "00597B20 profiles>1 UNREAD");
    }

    /// <summary>
    /// <c>0059899A</c>: empty continue
    /// list →
    /// <c>UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE</c>
    /// then <c>00595A06</c> /
    /// <c>00595B24</c>.
    /// </summary>
    private void AttachFrontendMainMenu()
    {
        var name = FrontendProfileCount == 0
            ? FrontendMainMenuNoContinue
            : FrontendMainMenuContinue;
        Note(FrontendMainMenuFn, "Frontend", "UI",
            "0059899A " + name);
        Note(FrontendMenuAttachFn, "Frontend", "UI", "00595A06");
        Note(FrontendUiBuildMenu, "Frontend", "UI", "00595B24");
        FrontendMenuRoot = name;
        ResolveFrontendDef(name);
        SwitchFrontendSlot(FrontendMainMenuSlot);
        AttachFrontendTree(name, FrontendMainMenuSlot);
        SelectFrontendState(FrontendMainMenuSlot, 5);
    }

    /// <summary>
    /// <c>00599E3F</c> when
    /// <c>[ui+160]≠0</c>: clear it,
    /// <c>00596917</c> slot <c>0x17</c>,
    /// <c>00851700</c> <c>+4=+5=0</c>.
    /// Same tick still skips
    /// <c>0059899A</c>.
    /// </summary>
    private void BindNewProfileFromArmedTick()
    {
        Note(FrontendUiTickFn, "Frontend", "UI",
            $"00599E3F [ui+{FrontendUiArmedOffset}] 00596917");
        FrontendUiArmed = false;
        Note(FrontendNewProfileBindFn, "Frontend", "UI",
            $"00596917 slot 0x{FrontendNewProfileSlot:X} " + FrontendNewProfileMenu);
        Note(FrontendMenuSwitchFn, "Frontend", "UI",
            $"00596763 slot 0x{FrontendNewProfileSlot:X}");
        Note(FrontendUi96CtorFn, "Frontend", "UI",
            $"00851700 [ui+{FrontendUi96Offset}] +4=0 +5=0");
        Note(FrontendUi96EditBoxFn, "Frontend", "UI",
            $"00851770 {FrontendNewProfileEditBox} type {FrontendNewProfileEditType}");
        Note(FrontendProfileDefaultFn, "Frontend", "UI",
            "004069E0 [0x13B86A0]=0 " + FrontendProfileDefaultText +
            $" else 0x{FrontendProfileDefaultFallbackVa:X} " +
            FrontendProfileDefaultFallback);
        Note(InputActionGetter, "Frontend", "UI",
            $"00851829 vtbl+8 then +12({FrontendEditBoxActionA}/{FrontendEditBoxActionB})");
        FrontendMenuRoot = FrontendNewProfileMenu;
        FrontendUi96Present = true;
        FrontendUi96Accept = false;
        FrontendUi96Armed = false;
        FrontendEditBoxBound = true;
        var seed = LookupFrontendText(FrontendProfileDefaultText)
            ?? FrontendProfileDefaultFallback;
        FrontendEditBoxName = seed;
        // 00851770 seeds the bound edit control before it starts accepting
        // WM_CHAR.  The insertion point follows the seeded run (the retail
        // New Profile screen visibly places the caret after "Default").
        _editBoxCursor = seed.Length;
        ResolveFrontendDef(FrontendNewProfileMenu);
        SwitchFrontendSlot(FrontendNewProfileSlot);
        AttachFrontendTree(FrontendNewProfileMenu, FrontendNewProfileSlot);
        SelectFrontendState(FrontendNewProfileSlot, 5);
        BindEditBoxSeed(seed);
    }

    /// <summary>
    /// <c>00851770</c> writes
    /// <c>004069E0</c> into type-37
    /// <c>vtbl+572</c>. The type-6
    /// child has no persist TextValue,
    /// so the seed becomes its glyph
    /// run.
    /// </summary>
    private void BindEditBoxSeed(string seed)
    {
        if (string.IsNullOrEmpty(seed))
            return;
        for (var i = 0; i < _frontendWidgets.Count; i++)
        {
            if (_frontendWidgets[i].Type != FrontendWidgetType.EditBox)
                continue;
            foreach (var kid in FrontendWidgetFactory.ChildrenOf(_frontendWidgets, i))
            {
                var child = _frontendWidgets[kid];
                if (child.Type != FrontendWidgetType.Text)
                    continue;
                if (!string.IsNullOrEmpty(child.Text))
                    continue;
                _frontendWidgets[kid] = child with { Text = seed };
            }
        }
    }

    /// <summary>
    /// <c>0053FE50</c> action 34 /
    /// WM_CHAR. Cursor is widget
    /// <c>+364</c> (0 after bind).
    /// </summary>
    private void ApplyEditBoxWmChar(int code)
    {
        if (!FrontendEditBoxBound || !FrontendUi96Present)
            return;
        Note(EditBoxInsertFn, "Frontend", "UI",
            $"0053FE50 code={code}");
        var name = FrontendEditBoxName ?? "";
        var cursor = Math.Clamp(_editBoxCursor, 0, name.Length);
        if (code == 8)
        {
            if (cursor <= 0)
                return;
            FrontendEditBoxName = name.Remove(cursor - 1, 1);
            _editBoxCursor = cursor - 1;
        }
        else if (code == 32 || code is >= 32 and <= 126)
        {
            var editBox = default(FrontendWidget);
            for (var i = 0; i < _frontendWidgets.Count; i++)
            {
                if (_frontendWidgets[i].Type == FrontendWidgetType.EditBox)
                {
                    editBox = _frontendWidgets[i];
                    break;
                }
            }
            var limit = editBox.EditBoxCharLimit > 0
                ? editBox.EditBoxCharLimit
                : EditBoxCap;
            if (name.Length >= limit ||
                (code == 32 && name.Length == 0 && editBox.DisallowSpaceAsFirstChar))
                return;
            FrontendEditBoxName = name.Insert(cursor, char.ConvertFromUtf32(code));
            _editBoxCursor = cursor + 1;
        }
        else
            return;

        SetEditBoxGlyphs(FrontendEditBoxName);
    }

    private void SetEditBoxGlyphs(string text)
    {
        for (var i = 0; i < _frontendWidgets.Count; i++)
        {
            if (_frontendWidgets[i].Type != FrontendWidgetType.EditBox)
                continue;
            foreach (var kid in FrontendWidgetFactory.ChildrenOf(_frontendWidgets, i))
            {
                var child = _frontendWidgets[kid];
                if (child.Type != FrontendWidgetType.Text)
                    continue;
                _frontendWidgets[kid] = child with { Text = text };
            }
        }
    }

    /// <summary>
    /// <c>0059A238</c> msg <c>0x126</c>
    /// → <c>00851920</c>. Nonempty
    /// edit text sets <c>+5=1</c>
    /// <c>+4=0</c>.
    /// </summary>
    private void AcceptNewProfileMessage()
    {
        Note(FrontendCommitNameFn, "Frontend", "UI",
            "00851920 " + FrontendAcceptNewProfile);
        if (!FrontendUi96Present || FrontendUi96Armed)
            return;
        var len = FrontendEditBoxName.Trim().Length;
        Note(0x00851890, "Frontend", "UI",
            $"00851890 trim len={len}");
        if (len <= 0)
            return;
        FrontendUi96Armed = true;
        FrontendUi96Accept = false;
        Note(FrontendCommitNameFn, "Frontend", "UI",
            "00851920 [ui+96+5]=1 [+4]=0");
    }

    /// <summary>
    /// <c>00599E3F</c> <c>+5≠0</c>
    /// <c>+4==0</c>, empty
    /// <c>005955AB</c> →
    /// <c>0059697A</c>.
    /// <c>004067C0</c> writable
    /// attaches
    /// <c>UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE</c>.
    /// </summary>
    private void CommitNewProfileFromArmedEdit()
    {
        Note(FrontendUiTickFn, "Frontend", "UI",
            "00599E3F [ui+96+5] 0059697A");
        Note(FrontendCommitProfileFn, "Frontend", "UI", "0059697A");
        Note(FrontendCanCreateProfileFn, "Frontend", "UI",
            FrontendCanCreateProfile
                ? "004067C0 00999AB0 writable"
                : "004067C0 00999AB0 miss");
        if (!FrontendCanCreateProfile)
            return;
        Note(FrontendMenuAttachFn, "Frontend", "UI",
            "00595A06 " + FrontendMainMenuNoContinue);
        Note(FrontendUiBuildMenu, "Frontend", "UI", "00595B24");
        Note(0x00594FA9, "Frontend", "UI",
            "00594FA9(0) clear [ui+96]");
        FrontendMenuRoot = FrontendMainMenuNoContinue;
        FrontendUi96Present = false;
        FrontendUi96Armed = false;
        ResolveFrontendDef(FrontendMainMenuNoContinue);
        SwitchFrontendSlot(FrontendMainMenuSlot);
        AttachFrontendTree(FrontendMainMenuNoContinue, FrontendMainMenuSlot);
        SelectFrontendState(FrontendMainMenuSlot, 5);
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
        FrontendBatch = null;
        FrontendPresentRgba = null;
        if (Device is VulkanDx9Device vk)
            vk.OwnsSwapchainPresent = false;
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
        Note(IniConsoleCommandsFn, "InitGame", "Ini",
            "009ED190 BindKey/RunScript");
        EnsurePlayerManagerSingleton();
        foreach (var (name, apply) in InitGameStages)
        {
            if (name == "Init Conversation Attitude")
                Note(0x0041863D, "InitGame", "InitGame", "Adding Console Variables");
            if (name == "Init World")
                Note(IniActivateQuestRegister, "InitGame", "Ini",
                    "00419D90 ActivateQuest");
            Note(apply, name, "InitGame", name);
            if (name == "Init Thing Components")
                AddFirstDefClass();
            if (name == "Init Definition Manager")
                PrepareDefinitionManager();
            if (name == "Init Graphics")
                OpenTextureBank();
            if (name == "Init Fonts")
            {
                Note(GameFontLookupFn, "Init Fonts", "Font",
                    $"009E2C80 {GameFontFaceName} [0x13B838C]");
                Note(GameFontStoreFn, "Init Fonts", "Font",
                    $"00419463 [game+{GameFontOffset}]");
                GameFontFace = GameFontFaceName;
            }
            if (name == "Init Subtitled Message")
            {
                Note(SubtitledPrefixFn, "Init Subtitled Message", "Defs",
                    $"0041A080 0x{SubtitledPrefixVa:X} {SubtitledDefsPrefix}");
                Note(SubtitledLeafFn, "Init Subtitled Message", "Defs",
                    $"0099BF30 0x{SubtitledLeafVa:X} {SubtitledDefsLeaf}");
                Note(SubtitledRegisterFn, "Init Subtitled Message", "Defs",
                    $"00A39010 [0x{SubtitledSingletonVa:X}] {SubtitledDefsPath}");
                SubtitledSymbolPath = SubtitledDefsPath;
                SubtitledSymbolsRegistered = true;
            }
            if (name == "Init Conversation Attitude")
                BindConversationAttitudes();
            if (name == "Init Player Manager")
                ApplyPlayerOwner();
            if (name == "Init Display Engine")
                ApplyDisplayEngine();
            if (name == "Init Player Interface")
            {
                Player.Construct();
                Note(PlayerInterfaceCtor, "InitGame", "Input",
                    "004473A0 size 0x898 vtbl 01231BDC game+32");
            }

            if (name == "Init World")
            {
                Note(InitWorldFn, "InitGame", "World",
                    "00418790 [world+320] from 013B7C90");
                Note(InitWorldFn, "Init World", "World", "004A67D0 vtbl 012390F0");
                Note(InitWorldInitFn, "Init World Init", "World", "004A6E30 vtbl+36");
                foreach (var (worldName, worldApply) in InitWorldInitStages)
                {
                    Note(worldApply, worldName, "World", worldName);
                    if (worldApply == InitMeshBankFn)
                        OpenMeshBank();
                }

                InitWorldCameras();
            }

            if (name == "Create Players")
                CreatePlayers();
            if (name == "Init Sound")
                ApplyInitSound();
            if (name == "Load Particles")
                Note(SkipParticlesVa, "InitGame", "InitGame",
                    $"013B8648={SkipParticlesFirstSeen} run 004174F1");
        }
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
        {
            PlayerBindSlot0 = GamePlus72;
            PlayerBindSlot1 = WorldFrame;
            PlayerBindSlot2 = 0;
            Note(PlayerBindAfterWorldFn, "InitGame", "Player",
                $"004AE9D0 +{PlayerBindSlot0Offset}={PlayerBindSlot0} +{PlayerBindSlot1Offset}={PlayerBindSlot1} +{PlayerBindSlot2Offset}={PlayerBindSlot2}");
        }
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
            ApplyUserIniCommands(userIni, "InitGame");
        }

        Note(EngineReadyCallback, "InitGame", "GameStart",
            $"009A4EC0 [engine+{EngineReadyCallbackOffset}]=004167DA +{EngineGamePtrOffset}=game");
    }

    /// <summary>
    /// <c>009EC710</c> walks tokens;
    /// <c>009EB430</c> looks up
    /// <c>[ini+64]</c> and calls
    /// handler vtbl+4.
    /// First-seen TLC <c>user.ini</c>:
    /// <c>SetMaxAnisotropy</c> is not
    /// a .text name → unknown.
    /// <c>RunScript("joystick.ini")</c>
    /// is <c>009ECB70</c> →
    /// <c>009EC890</c> /
    /// <c>00999230</c> miss.
    /// <c>ActivateQuest</c> is
    /// <c>00419D90</c> /
    /// <c>00419CE0</c>;
    /// <c>[world+56]</c> vtbl+1104
    /// is UNREAD — do not start a
    /// quest here.
    /// </summary>
    private void ApplyUserIniCommands(string path, string stage)
    {
        var names = new List<string>();
        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim().TrimEnd(';');
            if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('/') ||
                line.StartsWith('#'))
                continue;
            var paren = line.IndexOf('(');
            var space = line.IndexOfAny([' ', '\t']);
            string name;
            var arg = "";
            if (paren > 0)
            {
                name = line[..paren];
                var close = line.LastIndexOf(')');
                if (close > paren)
                    arg = line[(paren + 1)..close].Trim().Trim('"');
            }
            else
            {
                var end = space < 0 ? line.Length : space;
                name = line[..end];
            }

            if (name.Length == 0)
                continue;
            names.Add(name);
            Note(IniDispatchFn, stage, "Ini", "009EB430 " + name);
            DispatchUserIniCommand(name, arg, stage);
        }

        if (stage == "InitGame")
            UserIniCommands = names;
        else
            UserstIniCommands = names;
    }

    private void DispatchUserIniCommand(string name, string arg, string stage)
    {
        if (name == IniSetFullscreenName)
        {
            DisplayWindowFlag = ParseIniFalse(arg)
                ? (byte)0
                : (byte)1;
            DeviceWindowed = DisplayWindowFlag == 0;
            Note(DisplayWindowFlagVa, stage, "Window",
                $"009EB430 {IniSetFullscreenName} 0137544A={DisplayWindowFlag} " +
                $"009BF7E0 +{PresentParametersWindowedOffset} Windowed={DeviceWindowed}");
            return;
        }

        if (name == IniSetResolutionName)
        {
            var parts = arg.Split(',');
            if (parts.Length >= 2 &&
                int.TryParse(parts[0].Trim(), out var width) &&
                int.TryParse(parts[1].Trim(), out var height))
            {
                if (width >= GraphicsMinDimension)
                    BackBufferWidth = width;
                if (height >= GraphicsMinDimension)
                    BackBufferHeight = height;
                if (parts.Length >= 3 && int.TryParse(parts[2].Trim(), out var bpp) && bpp > 0)
                    BackBufferBpp = bpp;
                Note(DisplayWidthVa, stage, "Window",
                    $"009EB430 {IniSetResolutionName} {BackBufferWidth}x{BackBufferHeight}x{BackBufferBpp}");
            }

            return;
        }

        if (name == IniRunScriptName)
        {
            var file = arg.EndsWith(IniRunScriptSuffix, StringComparison.OrdinalIgnoreCase)
                ? arg
                : arg + IniRunScriptSuffix;
            Note(IniRunScriptFn, stage, "Ini",
                "009ECB70 " + file);
            var path = Install is null
                ? file
                : Path.Combine(Install.Root, file);
            if (File.Exists(path))
            {
                Note(IniApplyFn, stage, "Ini", "009EC890 " + file);
                ApplyUserIniCommands(path, stage);
            }
            else
                Note(FileExistsFn, stage, "Ini",
                    "00999230 " + file + " miss");
            return;
        }

        if (name == "ActivateQuest")
        {
            Note(IniActivateQuestRegister, "InitGame", "Ini",
                "00419D90 ActivateQuest");
            Note(IniActivateQuestGate, "InitGame", "Ini",
                "004197B0 xor al,al");
            Note(IniActivateQuestThunk, "InitGame", "Ini",
                $"00419CE0 [world+{WorldScriptManagerOffset}] " +
                $"vtbl+{ScriptManagerActivateQuestVtbl} 00892E80");
            Note(ScriptManagerActivateQuestFn, "InitGame", "Ini",
                "00892E80 [0x13B89FC] 004B4A10(1,1) " + arg);
            Note(ActivateInitialQuestsFn, "InitGame", "Ini",
                "004B4A10 → 004B4260 " + arg);
            ActivateNamedQuest(arg, "InitGame");
            return;
        }

        Note(IniUnknownFn, stage, "Ini",
            "009EB260 unknown input - " + name);
    }

    /// <summary>
    /// <c>00415530</c> folder
    /// <c>English</c>, join
    /// <c>Data\lang\</c> +
    /// <c>\lang_settings.txt</c>.
    /// <c>00999230</c> hit loads
    /// <c>004045C0</c> bools into
    /// <c>[0x13B861B]</c> /
    /// <c>[0x13B861C]</c> /
    /// <c>[0x13B864F]</c>. Miss
    /// skips the binds. Always
    /// <c>009BC890</c> /
    /// <c>009BC8A0</c>.
    /// </summary>
    private void ApplyLanguageSettings()
    {
        Note(SetupLanguageFn, "Setup Language", "Lang",
            "00415530 " + LanguageFolder);
        Note(LanguagePrefixVa, "Setup Language", "Lang",
            LanguagePrefix + LanguageFolder + LanguageSettingsLeaf);
        string? path = null;
        if (Install is not null)
            path = Path.Combine(
                Install.DataRoot, "lang", LanguageFolder, LanguageSettingsName);
        if (path is null || !File.Exists(path))
        {
            Note(FileExistsFn, "Setup Language", "Lang",
                "00999230 " + LanguageSettingsRelative + " miss");
        }
        else
        {
            LanguageSettingsLoaded = true;
            Note(FileExistsFn, "Setup Language", "Lang",
                "00999230 " + LanguageSettingsRelative);
            var values = ReadLangSettings(path);
            LeftAlignText = BindLanguageBool(
                LanguageLeftAlignName, LeftAlignTextVa, values);
            NoHangulWordWrap = BindLanguageBool(
                LanguageNoHangulName, NoHangulWordWrapVa, values);
            DisableCapsLock = BindLanguageBool(
                LanguageCapsLockName, DisableCapsLockVa, values);
        }

        Note(ApplyLeftAlignFn, "Setup Language", "Lang",
            $"009BC890 [0x{LeftAlignTextVa:X}]={LeftAlignText} " +
            $"[0x{ApplyLeftAlignDestVa:X}]");
        Note(ApplyNoHangulFn, "Setup Language", "Lang",
            $"009BC8A0 [0x{NoHangulWordWrapVa:X}]={NoHangulWordWrap} " +
            $"[0x{ApplyNoHangulDestVa:X}]");
    }

    private byte BindLanguageBool(
        string name, uint destVa, Dictionary<string, string> values)
    {
        var value = (byte)0;
        if (values.TryGetValue(name, out var arg) && !ParseIniFalse(arg))
            value = 1;
        Note(LanguageIniFn, "Setup Language", "Lang",
            $"004045C0 \"{name}\" [0x{destVa:X}]={value}");
        return value;
    }

    private static Dictionary<string, string> ReadLangSettings(string path)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim().TrimEnd(';');
            if (line.Length == 0 || line[0] is ';' or '/' or '#')
                continue;
            string name;
            var arg = "";
            var paren = line.IndexOf('(');
            if (paren > 0)
            {
                name = line[..paren];
                var close = line.LastIndexOf(')');
                if (close > paren)
                    arg = line[(paren + 1)..close].Trim().Trim('"');
            }
            else
            {
                var space = line.IndexOfAny([' ', '\t']);
                if (space < 0)
                    name = line;
                else
                {
                    name = line[..space];
                    arg = line[(space + 1)..].Trim();
                }
            }

            if (name.Length > 0)
                map[name] = arg;
        }

        return map;
    }

    /// <summary>
    /// <c>00413C50</c> before Setup
    /// library: <c>default_userst.ini</c>
    /// then <c>userst.ini</c> via
    /// <c>009EC890</c>.
    /// </summary>
    private void ApplyUserstIni()
    {
        Note(UserstGateVa, "Parse Command Line", "Ini",
            $"0137548F={UserstGateFirstSeen} 00413C50");
        if (UserstGateFirstSeen == 0)
            return;
        Note(UserstRegisterFn, "Parse Command Line", "Ini",
            "00413C50 009ED190 then 009EC5E0");
        if (Install is null)
            return;
        var defaults = Path.Combine(Install.Root, DefaultUserstIniName);
        if (File.Exists(defaults))
        {
            Note(DefaultUserstIniVa, "Parse Command Line", "Ini",
                "009EC890 " + DefaultUserstIniName);
            ApplyUserIniCommands(defaults, "Parse Command Line");
        }
        else
            Note(FileExistsFn, "Parse Command Line", "Ini",
                "00999230 " + DefaultUserstIniName + " miss");
        Note(UserstApplyFlagVa, "Parse Command Line", "Ini",
            $"01375444={UserstApplyFlagFirstSeen}");
        if (UserstApplyFlagFirstSeen == 0)
            return;
        var path = Path.Combine(Install.Root, UserstIniName);
        if (!File.Exists(path))
        {
            Note(FileExistsFn, "Parse Command Line", "Ini",
                "00999230 " + UserstIniName + " miss");
            return;
        }

        Note(UserstApplyFn, "Parse Command Line", "Ini",
            "00414C66 009EC890 " + UserstIniName);
        ApplyUserIniCommands(path, "Parse Command Line");
    }

    private static bool ParseIniFalse(string arg)
    {
        var value = arg.Trim();
        return value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
               value.Equals("bfalse", StringComparison.OrdinalIgnoreCase) ||
               value == "0";
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
        _questManagerPlus44.Clear();
        Note(QuestManagerPushFn, "Loading world", "Quest",
            $"004B4590 [0x{QuestManagerVa:X}]+{QuestManagerPlus44Offset}=0");
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
    /// <c>004A0D90</c> <c>FinalAlbion.qst</c>
    /// then <c>GlobalQuests.qst</c>,
    /// <c>004FDAB0</c> empty, Startup WAD,
    /// world <c>vtbl+8</c> <c>0049E220</c>
    /// → map <c>vtbl+12</c> <c>00507C30</c>,
    /// empty <c>006C20A0</c>, skip
    /// Generate Offline Data
    /// (<c>[0x1375446]==0</c>), Set Static Map.
    /// Host WLD-before-WAD is DISPROVEN.
    /// </summary>
    public void LoadWorldMap()
    {
        Note(LoadQuestsFn, "Load Quests", "WLD",
            "004A1840 Load Quests / WLD / Startup WAD");
        WorldFileName = FinalAlbionWld;
        LoadQuestDefs();
        Note(WorldMapOpenBankFn, "Loading world", "WLD",
            "004FDAB0 empty 0x122D70C");
        Note(StartupWadSite, "Loading world", "WLD", "Startup WAD");
        Note(ExtraWadFlagVa, "Loading world", "WLD",
            $"01375456={ExtraWadFlagFirstSeen} skip");
        Note(WorldLoadWldFn, "Loading world", "WLD",
            $"0049E220 vtbl+{WorldLoadWldVtbl}");
        Note(LoadWldFile, "Load .wld file", "WLD", "00507C30 vtbl+12");
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
            }
        }

        Note(WorldAfterWldFn, "Loading world", "WLD",
            $"0049D970 +{WorldLoadedFlagOffset}=1");
        Note(LevelLoaderHasWork, "Loading world", "WLD",
            "006C20A0 empty skip");
        Note(GenerateOfflineDataSite, "Loading world", "WLD",
            $"Generate Offline Data 01375446={GenerateOfflineDataFlagFirstSeen} skip");
        Note(SetStaticMapForEngineSite, "Loading world", "WLD",
            "Set Static Map for Engine");
        SetStaticMapFileForUse();
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
        // 004FDBC0 ebx=1 skips dummy slot 0.
        // First 004FBF60 is LookoutPoint (NewMap 1).
        // Native then inc ebx through every filled
        // LoadedOnPlayerProximity slot (1..count-1).
        // Host break after the first prox file is
        // leftover #50 (ThingFile.Parse OOM), not a
        // recovered NewMap-1 lock and not 00501450.
        WorldMap? first = null;
        var prox = 0;
        foreach (var map in World.Maps)
        {
            if (!map.LoadedOnPlayerProximity)
                continue;
            prox++;
            first ??= map;
        }

        if (first is { } lookout)
        {
            Note(LoadGlobalThingsPerMap, "Loading global things", "WLD",
                "004FBF60 " + lookout.ScriptName + ".tng");
            var tng = _levels?.TryLoadThings(lookout.ScriptName);
            if (tng is not null)
            {
                loaded.AddRange(tng.Things);
                GlobalThingMapsLoaded = 1;
            }

            if (prox > 1)
                Note(LoadGlobalThingsMapFile, "Loading global things", "WLD",
                    $"004FDC00 leftover host break ebx=2..{prox} unparsed={prox - 1}");
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
    /// <summary>
    /// <c>0041852D</c>
    /// <c>0044C6B6</c>. First-seen
    /// miss constructs
    /// <c>0xE0</c> /
    /// <c>0044C6C2</c> /
    /// <c>0044C71F</c> before
    /// Thing Components.
    /// </summary>
    /// <summary>
    /// <c>004CD670</c>: skip if
    /// <c>[0x13B8A28]!=0</c>, else
    /// <c>0099EFE0</c> the three
    /// tables and set the once
    /// flag. Not Speak.
    /// </summary>
    private void BindConversationAttitudes()
    {
        if (ConversationAttitudesBound)
            return;
        Note(ConversationAttitudeBindFn, "Init Conversation Attitude", "Defs",
            $"0099EFE0 [0x{ConversationAttitudeTable0Va:X}]/{ConversationAttitudeNames0.Length}");
        Note(ConversationAttitudeBindFn, "Init Conversation Attitude", "Defs",
            $"0099EFE0 [0x{ConversationAttitudeTable1Va:X}]/{ConversationAttitudeNames1.Length}");
        Note(ConversationAttitudeBindFn, "Init Conversation Attitude", "Defs",
            $"0099EFE0 [0x{ConversationAttitudeTable2Va:X}]/{ConversationAttitudeNames2.Length}");
        Note(ConversationAttitudeOnceVa, "Init Conversation Attitude", "Defs",
            $"[0x{ConversationAttitudeOnceVa:X}]=1");
        _conversationAttitudeTable0 = ConversationAttitudeNames0;
        _conversationAttitudeTable1 = ConversationAttitudeNames1;
        _conversationAttitudeTable2 = ConversationAttitudeNames2;
        ConversationAttitudesBound = true;
    }

    /// <summary>
    /// <c>0041732A</c>:
    /// <c>00BFEA1A(44)</c>
    /// <c>0044C6B0</c>
    /// <c>0044A3B0</c>
    /// <c>004193A0</c>
    /// <c>[game+28]</c>.
    /// Not Create Players.
    /// </summary>
    private void ApplyPlayerOwner()
    {
        if (PlayerOwnerPresent)
            return;
        Note(PlayerManagerGetter, "Init Player Manager", "Player",
            "0044C6B0 [0x13B879C]");
        Note(PlayerOwnerCtor, "Init Player Manager", "Player",
            $"0044A3B0 vtbl 0x{PlayerOwnerVtbl:X} size {PlayerOwnerSize} [game+{PlayerOwnerOffset}]");
        Note(PlayerOwnerStoreFn, "Init Player Manager", "Player",
            $"004193A0 [game+{PlayerOwnerOffset}]");
        _playerOwnerHeroSwap = PlayerOwnerHeroSwapNames;
        PlayerOwnerPresent = true;
    }

    /// <summary>
    /// <summary>
    /// <c>00417A58</c> first-seen after
    /// Leave: register localised / main /
    /// atmos banks. Not play, not
    /// <c>MUSIC_SET_*</c>, not
    /// <c>00A01A4F</c> first-seen.
    /// </summary>
    private void ApplyInitSound()
    {
        if (GamePlus16 != 0)
            return;
        Note(RetailAudioEngineVa, "Init Sound", "Audio",
            "013B8394 live skip je 00418286");
        Note(InitSoundLocaleFn, "Init Sound", "Audio",
            $"00415550 {InitSoundLocaleName}");
        Note(PlayerManagerGetter, "Init Sound", "Audio",
            "0044C6B0 [0x13B879C]");
        Note(InitSoundLookupFn, "Init Sound", "Audio",
            $"004196B2 {InitSoundMainName}");
        Note(InitSoundRegisterFn, "Init Sound", "Audio",
            "009919C0 Registering Localised Sound Bank");
        Note(InitSoundSymbolsCompiledFn, "Init Sound", "Audio",
            "00A38C20 compiled symbols not 00A01A4F");
        Note(InitSoundRegisterFn, "Init Sound", "Audio",
            "009919C0 Registering Sound Bank");
        Note(InitSoundAtmosRegisterFn, "Init Sound", "Audio",
            "00991C10 Registering Atmos Sound Bank");
        Note(InitSoundMapLookupFn, "Init Sound", "Audio",
            "00991840(1) [game+16]");
        GamePlus16 = 1;
    }

    /// <summary>
    /// <c>00417418</c> after Init World:
    /// zero 36-byte blob, alloc
    /// <c>0x100</c>, ctor copies six
    /// identities, store
    /// <c>[game+40]</c>. Does not fill
    /// blob <c>+20/+24/+28</c>. Does not
    /// spawn Things.
    /// </summary>
    private void ApplyDisplayEngine()
    {
        if (DisplayPresent)
            return;
        Note(DisplayBlobZeroFn, "Init Display Engine", "Display",
            $"00419270 blob {DisplayBlobSize}");
        Note(PlayerManagerGetter, "Init Display Engine", "Display",
            "0044C6B0 [0x13B879C]");
        Note(DisplayAllocFn, "Init Display Engine", "Display",
            $"00BFEA1A size 0x{DisplaySize:X}");
        DisplayPlus232 = DisplayPlus232Ctor;
        DisplayPlus4 = DisplayPlus4Ctor;
        Note(DisplayCtorFn, "Init Display Engine", "Display",
            $"00434E10 vtbl 0x{DisplayVtbl:X} +{DisplayPlus4Offset}={DisplayPlus4Ctor} " +
            $"+{GameDisplayPlus8Offset}/game +{DisplayPlus12Offset}/owner " +
            $"+{DisplayPlus16Offset}/manager +{DisplayPlus20Offset}/GBANK_MAIN " +
            $"+{DisplayPlus24Offset}/mesh +{DisplayPlus248Offset}/world " +
            $"+{DisplayPlus232Offset}={DisplayPlus232Ctor}");
        Note(DisplayStoreFn, "Init Display Engine", "Display",
            $"0041940C [game+{GameDisplayOffset}]");
        DisplayPlus8Game = true;
        DisplayPlus12Owner = PlayerOwnerPresent;
        DisplayPlus16Manager = PlayerManagerPresent;
        DisplayPlus20GraphicBank = true;
        DisplayPlus24MeshBank = true;
        DisplayPlus248World = true;
        DisplayPresent = true;
    }

    private void EnsurePlayerManagerSingleton()
    {
        Note(PlayerManagerPresentFn, "InitGame", "Player",
            $"0044C6B6 [0x{PlayerManagerVa:X}]");
        if (PlayerManagerPresent)
            return;
        Note(PlayerManagerCtorFn, "InitGame", "Player",
            $"0044C6C2 vtbl 0x{PlayerManagerVtbl:X} size 0x{PlayerManagerSize:X} +40=0x{PlayerManagerPlus40:X}");
        Note(PlayerManagerStoreFn, "InitGame", "Player",
            $"0044C71F 00450142 [0x{PlayerManagerVa:X}]");
        PlayerManagerPresent = true;
        PlayerManagerPlus40Cap = PlayerManagerPlus40;
    }

    /// <summary>
    /// <c>004EE337</c>
    /// <c>0044C6B0</c>
    /// <c>009B0AC0</c>
    /// <c>CHeroMorphDef</c>
    /// <c>009AD6E0</c>
    /// <c>009FC4F0</c>.
    /// Not a def parse.
    /// </summary>
    private void AddFirstDefClass()
    {
        if (!FirstDefClassRegistered)
        {
            Note(PlayerManagerGetter, "Init Thing Components", "Defs",
                "0044C6B0 [0x13B879C]");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FirstDefClassName}");
            Note(FirstDefClassFactory, "Init Thing Components", "Defs",
                $"004E4219 {FirstDefClassName}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                "009AD6E0 CDefinitionManager::LoadDef");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FirstDefClass = FirstDefClassName;
            FirstDefClassRegistered = true;
        }
        if (!SecondDefClassRegistered)
        {
            Note(SecondDefClassSite, "Init Thing Components", "Defs",
                $"004EE565 0044C6B0 {SecondDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SecondDefClassName}");
            Note(SecondDefClassFactory, "Init Thing Components", "Defs",
                $"004D8671 size {SecondDefClassSize} vtbl 0x{SecondDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SecondDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SecondDefClass = SecondDefClassName;
            SecondDefClassRegistered = true;
        }
        if (!ThirdDefClassRegistered)
        {
            Note(ThirdDefClassSite, "Init Thing Components", "Defs",
                $"004EE62B 0044C6B0 {ThirdDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {ThirdDefClassName}");
            Note(ThirdDefClassFactory, "Init Thing Components", "Defs",
                $"004DA82B size {ThirdDefClassSize} vtbl 0x{ThirdDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {ThirdDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            ThirdDefClass = ThirdDefClassName;
            ThirdDefClassRegistered = true;
        }
        if (!FourthDefClassRegistered)
        {
            Note(FourthDefClassSite, "Init Thing Components", "Defs",
                $"004EE6FD 0044C6B0 {FourthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FourthDefClassName}");
            Note(FourthDefClassFactory, "Init Thing Components", "Defs",
                $"004D84C8 size {FourthDefClassSize} vtbl 0x{FourthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FourthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FourthDefClass = FourthDefClassName;
            FourthDefClassRegistered = true;
        }
        if (!FifthDefClassRegistered)
        {
            Note(FifthDefClassSite, "Init Thing Components", "Defs",
                $"004EE92B 0044C6B0 {FifthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FifthDefClassName}");
            Note(FifthDefClassFactory, "Init Thing Components", "Defs",
                $"004DA871 size {FifthDefClassSize} vtbl 0x{FifthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FifthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FifthDefClass = FifthDefClassName;
            FifthDefClassRegistered = true;
        }
        if (!SixthDefClassRegistered)
        {
            Note(SixthDefClassSite, "Init Thing Components", "Defs",
                $"004EF23D 0044C6B0 {SixthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SixthDefClassName}");
            Note(SixthDefClassFactory, "Init Thing Components", "Defs",
                $"0044F644 0044C108 size {SixthDefClassSize} vtbl 0x{SixthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SixthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SixthDefClass = SixthDefClassName;
            SixthDefClassRegistered = true;
        }
        if (!SeventhDefClassRegistered)
        {
            Note(SeventhDefClassSite, "Init Thing Components", "Defs",
                $"004EF37F 0044C6B0 {SeventhDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SeventhDefClassName}");
            Note(SeventhDefClassFactory, "Init Thing Components", "Defs",
                $"004D80E4 0044C0C0 size {SeventhDefClassSize} vtbl 0x{SeventhDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SeventhDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SeventhDefClass = SeventhDefClassName;
            SeventhDefClassRegistered = true;
        }
        if (!EighthDefClassRegistered)
        {
            Note(EighthDefClassSite, "Init Thing Components", "Defs",
                $"004EF5AD 0044C6B0 {EighthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {EighthDefClassName}");
            Note(EighthDefClassFactory, "Init Thing Components", "Defs",
                $"004DAA0E 0044C0C0 size {EighthDefClassSize} vtbl 0x{EighthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {EighthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            EighthDefClass = EighthDefClassName;
            EighthDefClassRegistered = true;
        }
        if (!NinthDefClassRegistered)
        {
            Note(NinthDefClassSite, "Init Thing Components", "Defs",
                $"004F0171 0044C6B0 {NinthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {NinthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {NinthDefClassName}");
            Note(NinthDefClassFactory, "Init Thing Components", "Defs",
                $"004E213B 004DFF04 size 0x{NinthDefClassSize:X} vtbl 0x{NinthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {NinthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            NinthDefClass = NinthDefClassName;
            NinthDefClassRegistered = true;
        }
        if (!TenthDefClassRegistered)
        {
            Note(TenthDefClassSite, "Init Thing Components", "Defs",
                $"004F0227 0044C6B0 {TenthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {TenthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {TenthDefClassName}");
            Note(TenthDefClassFactory, "Init Thing Components", "Defs",
                $"004DA7AD 0044C0C0 size {TenthDefClassSize} vtbl 0x{TenthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {TenthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            TenthDefClass = TenthDefClassName;
            TenthDefClassRegistered = true;
        }
        if (!EleventhDefClassRegistered)
        {
            Note(EleventhDefClassSite, "Init Thing Components", "Defs",
                $"004F02DD 0044C6B0 {EleventhDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {EleventhDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {EleventhDefClassName}");
            Note(EleventhDefClassFactory, "Init Thing Components", "Defs",
                $"004E0148 004DDB2C size {EleventhDefClassSize} vtbl 0x{EleventhDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {EleventhDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            EleventhDefClass = EleventhDefClassName;
            EleventhDefClassRegistered = true;
        }
        if (!TwelfthDefClassRegistered)
        {
            Note(TwelfthDefClassSite, "Init Thing Components", "Defs",
                $"004F0393 0044C6B0 {TwelfthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {TwelfthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {TwelfthDefClassName}");
            Note(TwelfthDefClassFactory, "Init Thing Components", "Defs",
                $"004D7B5B 0044C0C0 size {TwelfthDefClassSize} vtbl 0x{TwelfthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {TwelfthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            TwelfthDefClass = TwelfthDefClassName;
            TwelfthDefClassRegistered = true;
        }
        if (!ThirteenthDefClassRegistered)
        {
            Note(ThirteenthDefClassSite, "Init Thing Components", "Defs",
                $"004F04B4 0044C6B0 {ThirteenthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {ThirteenthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {ThirteenthDefClassName}");
            Note(ThirteenthDefClassFactory, "Init Thing Components", "Defs",
                $"004D7BA1 0044C0C0 size {ThirteenthDefClassSize} vtbl 0x{ThirteenthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {ThirteenthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            ThirteenthDefClass = ThirteenthDefClassName;
            ThirteenthDefClassRegistered = true;
        }
        if (!FourteenthDefClassRegistered)
        {
            Note(FourteenthDefClassSite, "Init Thing Components", "Defs",
                $"004F0640 0044C6B0 {FourteenthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FourteenthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FourteenthDefClassName}");
            Note(FourteenthDefClassFactory, "Init Thing Components", "Defs",
                $"004D7BE7 0044C0C0 size {FourteenthDefClassSize} vtbl 0x{FourteenthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FourteenthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FourteenthDefClass = FourteenthDefClassName;
            FourteenthDefClassRegistered = true;
        }
        if (!FifteenthDefClassRegistered)
        {
            Note(FifteenthDefClassSite, "Init Thing Components", "Defs",
                $"004F06F6 0044C6B0 {FifteenthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FifteenthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FifteenthDefClassName}");
            Note(FifteenthDefClassFactory, "Init Thing Components", "Defs",
                $"004D7C73 0044C0C0 size {FifteenthDefClassSize} vtbl 0x{FifteenthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FifteenthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FifteenthDefClass = FifteenthDefClassName;
            FifteenthDefClassRegistered = true;
        }
        if (!SixteenthDefClassRegistered)
        {
            Note(SixteenthDefClassSite, "Init Thing Components", "Defs",
                $"004F07AC 0044C6B0 {SixteenthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SixteenthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SixteenthDefClassName}");
            Note(SixteenthDefClassFactory, "Init Thing Components", "Defs",
                $"004D7CB9 0044C0C0 size {SixteenthDefClassSize} vtbl 0x{SixteenthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SixteenthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SixteenthDefClass = SixteenthDefClassName;
            SixteenthDefClassRegistered = true;
        }
        if (!SeventeenthDefClassRegistered)
        {
            Note(SeventeenthDefClassSite, "Init Thing Components", "Defs",
                $"004F0862 0044C6B0 {SeventeenthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SeventeenthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SeventeenthDefClassName}");
            Note(SeventeenthDefClassFactory, "Init Thing Components", "Defs",
                $"004E4477 004E380E 0044C0C0 size {SeventeenthDefClassSize} vtbl 0x{SeventeenthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SeventeenthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SeventeenthDefClass = SeventeenthDefClassName;
            SeventeenthDefClassRegistered = true;
        }
        if (!EighteenthDefClassRegistered)
        {
            Note(EighteenthDefClassSite, "Init Thing Components", "Defs",
                $"004F0918 0044C6B0 {EighteenthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {EighteenthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {EighteenthDefClassName}");
            Note(EighteenthDefClassFactory, "Init Thing Components", "Defs",
                $"004D7CFF 0044C0C0 size {EighteenthDefClassSize} vtbl 0x{EighteenthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {EighteenthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            EighteenthDefClass = EighteenthDefClassName;
            EighteenthDefClassRegistered = true;
        }
        if (!NineteenthDefClassRegistered)
        {
            Note(NineteenthDefClassSite, "Init Thing Components", "Defs",
                $"004F0D26 0044C6B0 {NineteenthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {NineteenthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {NineteenthDefClassName}");
            Note(NineteenthDefClassFactory, "Init Thing Components", "Defs",
                $"004E0B4B 004DE7DC 0044C0C0 size {NineteenthDefClassSize} vtbl 0x{NineteenthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {NineteenthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            NineteenthDefClass = NineteenthDefClassName;
            NineteenthDefClassRegistered = true;
        }
        if (!TwentiethDefClassRegistered)
        {
            Note(TwentiethDefClassSite, "Init Thing Components", "Defs",
                $"004F0DDC 0044C6B0 {TwentiethDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {TwentiethDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {TwentiethDefClassName}");
            Note(TwentiethDefClassFactory, "Init Thing Components", "Defs",
                $"004D7EB6 0044C0C0 size {TwentiethDefClassSize} vtbl 0x{TwentiethDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {TwentiethDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            TwentiethDefClass = TwentiethDefClassName;
            TwentiethDefClassRegistered = true;
        }
        if (!TwentyFirstDefClassRegistered)
        {
            Note(TwentyFirstDefClassSite, "Init Thing Components", "Defs",
                $"004F0E92 0044C6B0 {TwentyFirstDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {TwentyFirstDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {TwentyFirstDefClassName}");
            Note(TwentyFirstDefClassFactory, "Init Thing Components", "Defs",
                $"004DA7F3 004D7A25 0044C0C0 size {TwentyFirstDefClassSize} vtbl 0x{TwentyFirstDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {TwentyFirstDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            TwentyFirstDefClass = TwentyFirstDefClassName;
            TwentyFirstDefClassRegistered = true;
        }
        if (!TwentySecondDefClassRegistered)
        {
            Note(TwentySecondDefClassSite, "Init Thing Components", "Defs",
                $"004F0F48 0044C6B0 {TwentySecondDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {TwentySecondDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {TwentySecondDefClassName}");
            Note(TwentySecondDefClassFactory, "Init Thing Components", "Defs",
                $"004D7EFC 0044C0C0 size {TwentySecondDefClassSize} vtbl 0x{TwentySecondDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {TwentySecondDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            TwentySecondDefClass = TwentySecondDefClassName;
            TwentySecondDefClassRegistered = true;
        }
        if (!TwentyThirdDefClassRegistered)
        {
            Note(TwentyThirdDefClassSite, "Init Thing Components", "Defs",
                $"004F10D4 0044C6B0 {TwentyThirdDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {TwentyThirdDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {TwentyThirdDefClassName}");
            Note(TwentyThirdDefClassFactory, "Init Thing Components", "Defs",
                $"004D7F7B 004D36FE 0044C0C0 size {TwentyThirdDefClassSize} vtbl 0x{TwentyThirdDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {TwentyThirdDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            TwentyThirdDefClass = TwentyThirdDefClassName;
            TwentyThirdDefClassRegistered = true;
        }
        if (!TwentyFourthDefClassRegistered)
        {
            Note(TwentyFourthDefClassSite, "Init Thing Components", "Defs",
                $"004F11F5 0044C6B0 {TwentyFourthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {TwentyFourthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {TwentyFourthDefClassName}");
            Note(TwentyFourthDefClassFactory, "Init Thing Components", "Defs",
                $"004E0513 004DE1DF 0044C0C0 size {TwentyFourthDefClassSize} vtbl 0x{TwentyFourthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {TwentyFourthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            TwentyFourthDefClass = TwentyFourthDefClassName;
            TwentyFourthDefClassRegistered = true;
        }
        if (!TwentyFifthDefClassRegistered)
        {
            Note(TwentyFifthDefClassSite, "Init Thing Components", "Defs",
                $"004F12AB 0044C6B0 {TwentyFifthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {TwentyFifthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {TwentyFifthDefClassName}");
            Note(TwentyFifthDefClassFactory, "Init Thing Components", "Defs",
                $"004D805C 004D3826 0044C0C0 size {TwentyFifthDefClassSize} vtbl 0x{TwentyFifthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {TwentyFifthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            TwentyFifthDefClass = TwentyFifthDefClassName;
            TwentyFifthDefClassRegistered = true;
        }
        if (!TwentySixthDefClassRegistered)
        {
            Note(TwentySixthDefClassSite, "Init Thing Components", "Defs",
                $"004F14A2 0044C6B0 {TwentySixthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {TwentySixthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {TwentySixthDefClassName}");
            Note(TwentySixthDefClassFactory, "Init Thing Components", "Defs",
                $"004D809E 0044C0C0 size {TwentySixthDefClassSize} vtbl 0x{TwentySixthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {TwentySixthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            TwentySixthDefClass = TwentySixthDefClassName;
            TwentySixthDefClassRegistered = true;
        }
        if (!TwentySeventhDefClassRegistered)
        {
            Note(TwentySeventhDefClassSite, "Init Thing Components", "Defs",
                $"004F1558 0044C6B0 {TwentySeventhDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {TwentySeventhDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {TwentySeventhDefClassName}");
            Note(TwentySeventhDefClassFactory, "Init Thing Components", "Defs",
                $"004E3C81 004E247A 0044C0C0 size {TwentySeventhDefClassSize} vtbl 0x{TwentySeventhDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {TwentySeventhDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            TwentySeventhDefClass = TwentySeventhDefClassName;
            TwentySeventhDefClassRegistered = true;
        }
        if (!TwentyEighthDefClassRegistered)
        {
            Note(TwentyEighthDefClassSite, "Init Thing Components", "Defs",
                $"004F1CBA 0044C6B0 {TwentyEighthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {TwentyEighthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {TwentyEighthDefClassName}");
            Note(TwentyEighthDefClassFactory, "Init Thing Components", "Defs",
                $"004E3D15 004E2612 0044C0C0 size {TwentyEighthDefClassSize} vtbl 0x{TwentyEighthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {TwentyEighthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            TwentyEighthDefClass = TwentyEighthDefClassName;
            TwentyEighthDefClassRegistered = true;
        }
        if (!TwentyNinthDefClassRegistered)
        {
            Note(TwentyNinthDefClassSite, "Init Thing Components", "Defs",
                $"004F1D70 0044C6B0 {TwentyNinthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {TwentyNinthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {TwentyNinthDefClassName}");
            Note(TwentyNinthDefClassFactory, "Init Thing Components", "Defs",
                $"004DFE62 004DD8FF 0044C0C0 size {TwentyNinthDefClassSize} vtbl 0x{TwentyNinthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {TwentyNinthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            TwentyNinthDefClass = TwentyNinthDefClassName;
            TwentyNinthDefClassRegistered = true;
        }
        if (!ThirtiethDefClassRegistered)
        {
            Note(ThirtiethDefClassSite, "Init Thing Components", "Defs",
                $"004F1E26 0044C6B0 {ThirtiethDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {ThirtiethDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {ThirtiethDefClassName}");
            Note(ThirtiethDefClassFactory, "Init Thing Components", "Defs",
                $"004DA767 0044C0C0 size {ThirtiethDefClassSize} vtbl 0x{ThirtiethDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {ThirtiethDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            ThirtiethDefClass = ThirtiethDefClassName;
            ThirtiethDefClassRegistered = true;
        }
        if (!ThirtyFirstDefClassRegistered)
        {
            Note(ThirtyFirstDefClassSite, "Init Thing Components", "Defs",
                $"004F1EDC 0044C6B0 {ThirtyFirstDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {ThirtyFirstDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {ThirtyFirstDefClassName}");
            Note(ThirtyFirstDefClassFactory, "Init Thing Components", "Defs",
                $"004D835A 0044C0C0 size {ThirtyFirstDefClassSize} vtbl 0x{ThirtyFirstDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {ThirtyFirstDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            ThirtyFirstDefClass = ThirtyFirstDefClassName;
            ThirtyFirstDefClassRegistered = true;
        }
        if (!ThirtySecondDefClassRegistered)
        {
            Note(ThirtySecondDefClassSite, "Init Thing Components", "Defs",
                $"004F22E6 0044C6B0 {ThirtySecondDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {ThirtySecondDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {ThirtySecondDefClassName}");
            Note(ThirtySecondDefClassFactory, "Init Thing Components", "Defs",
                $"004D83D9 004D3F83 0044C0C0 size {ThirtySecondDefClassSize} vtbl 0x{ThirtySecondDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {ThirtySecondDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            ThirtySecondDefClass = ThirtySecondDefClassName;
            ThirtySecondDefClassRegistered = true;
        }
        if (!ThirtyThirdDefClassRegistered)
        {
            Note(ThirtyThirdDefClassSite, "Init Thing Components", "Defs",
                $"004F2472 0044C6B0 {ThirtyThirdDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {ThirtyThirdDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {ThirtyThirdDefClassName}");
            Note(ThirtyThirdDefClassFactory, "Init Thing Components", "Defs",
                $"004E26BC 004E06C3 0044C0C0 size {ThirtyThirdDefClassSize} vtbl 0x{ThirtyThirdDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {ThirtyThirdDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            ThirtyThirdDefClass = ThirtyThirdDefClassName;
            ThirtyThirdDefClassRegistered = true;
        }
        if (!ThirtyFourthDefClassRegistered)
        {
            Note(ThirtyFourthDefClassSite, "Init Thing Components", "Defs",
                $"004F2593 0044C6B0 {ThirtyFourthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {ThirtyFourthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {ThirtyFourthDefClassName}");
            Note(ThirtyFourthDefClassFactory, "Init Thing Components", "Defs",
                $"004D8482 0044C0C0 size {ThirtyFourthDefClassSize} vtbl 0x{ThirtyFourthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {ThirtyFourthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            ThirtyFourthDefClass = ThirtyFourthDefClassName;
            ThirtyFourthDefClassRegistered = true;
        }
        if (!ThirtyFifthDefClassRegistered)
        {
            Note(ThirtyFifthDefClassSite, "Init Thing Components", "Defs",
                $"004F2649 0044C6B0 {ThirtyFifthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {ThirtyFifthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {ThirtyFifthDefClassName}");
            Note(ThirtyFifthDefClassFactory, "Init Thing Components", "Defs",
                $"004D8547 0044C0C0 size {ThirtyFifthDefClassSize} vtbl 0x{ThirtyFifthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {ThirtyFifthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            ThirtyFifthDefClass = ThirtyFifthDefClassName;
            ThirtyFifthDefClassRegistered = true;
        }
        if (!ThirtySixthDefClassRegistered)
        {
            Note(ThirtySixthDefClassSite, "Init Thing Components", "Defs",
                $"004F27CD 0044C6B0 {ThirtySixthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {ThirtySixthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {ThirtySixthDefClassName}");
            Note(ThirtySixthDefClassFactory, "Init Thing Components", "Defs",
                $"004E2809 004E0900 0044C0C0 size {ThirtySixthDefClassSize} vtbl 0x{ThirtySixthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {ThirtySixthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            ThirtySixthDefClass = ThirtySixthDefClassName;
            ThirtySixthDefClassRegistered = true;
        }
        if (!ThirtySeventhDefClassRegistered)
        {
            Note(ThirtySeventhDefClassSite, "Init Thing Components", "Defs",
                $"004F2C36 0044C6B0 {ThirtySeventhDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {ThirtySeventhDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {ThirtySeventhDefClassName}");
            Note(ThirtySeventhDefClassFactory, "Init Thing Components", "Defs",
                $"004EBAE7 004EB9E8 0044C0C0 size {ThirtySeventhDefClassSize} vtbl 0x{ThirtySeventhDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {ThirtySeventhDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            ThirtySeventhDefClass = ThirtySeventhDefClassName;
            ThirtySeventhDefClassRegistered = true;
        }
        if (!ThirtyEighthDefClassRegistered)
        {
            Note(ThirtyEighthDefClassSite, "Init Thing Components", "Defs",
                $"004F2CE8 0044C6B0 {ThirtyEighthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {ThirtyEighthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {ThirtyEighthDefClassName}");
            Note(ThirtyEighthDefClassFactory, "Init Thing Components", "Defs",
                $"004E27AE 004E0860 0044C0C0 size {ThirtyEighthDefClassSize} vtbl 0x{ThirtyEighthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {ThirtyEighthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            ThirtyEighthDefClass = ThirtyEighthDefClassName;
            ThirtyEighthDefClassRegistered = true;
        }
        if (!ThirtyNinthDefClassRegistered)
        {
            Note(ThirtyNinthDefClassSite, "Init Thing Components", "Defs",
                $"004F2FB5 0044C6B0 {ThirtyNinthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {ThirtyNinthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {ThirtyNinthDefClassName}");
            Note(ThirtyNinthDefClassFactory, "Init Thing Components", "Defs",
                $"004E60D8 004E3E4C 0044C0C0 size {ThirtyNinthDefClassSize} vtbl 0x{ThirtyNinthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {ThirtyNinthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            ThirtyNinthDefClass = ThirtyNinthDefClassName;
            ThirtyNinthDefClassRegistered = true;
        }
        if (!FortiethDefClassRegistered)
        {
            Note(FortiethDefClassSite, "Init Thing Components", "Defs",
                $"004F306B 0044C6B0 {FortiethDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FortiethDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FortiethDefClassName}");
            Note(FortiethDefClassFactory, "Init Thing Components", "Defs",
                $"004E31FA 004E1516 0044C0C0 size {FortiethDefClassSize} vtbl 0x{FortiethDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FortiethDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FortiethDefClass = FortiethDefClassName;
            FortiethDefClassRegistered = true;
        }
        if (!FortyFirstDefClassRegistered)
        {
            Note(FortyFirstDefClassSite, "Init Thing Components", "Defs",
                $"004F3338 0044C6B0 {FortyFirstDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FortyFirstDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FortyFirstDefClassName}");
            Note(FortyFirstDefClassFactory, "Init Thing Components", "Defs",
                $"004D86F0 0044C0C0 size {FortyFirstDefClassSize} vtbl 0x{FortyFirstDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FortyFirstDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FortyFirstDefClass = FortyFirstDefClassName;
            FortyFirstDefClassRegistered = true;
        }
        if (!FortySecondDefClassRegistered)
        {
            Note(FortySecondDefClassSite, "Init Thing Components", "Defs",
                $"004F34C4 0044C6B0 {FortySecondDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FortySecondDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FortySecondDefClassName}");
            Note(FortySecondDefClassFactory, "Init Thing Components", "Defs",
                $"004E2333 004E00BC 0044C0C0 size {FortySecondDefClassSize} vtbl 0x{FortySecondDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FortySecondDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FortySecondDefClass = FortySecondDefClassName;
            FortySecondDefClassRegistered = true;
        }
        if (!FortyThirdDefClassRegistered)
        {
            Note(FortyThirdDefClassSite, "Init Thing Components", "Defs",
                $"004F357A 0044C6B0 {FortyThirdDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FortyThirdDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FortyThirdDefClassName}");
            Note(FortyThirdDefClassFactory, "Init Thing Components", "Defs",
                $"004E3DC3 004E284B 0044C0C0 size {FortyThirdDefClassSize} vtbl 0x{FortyThirdDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FortyThirdDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FortyThirdDefClass = FortyThirdDefClassName;
            FortyThirdDefClassRegistered = true;
        }
        if (!FortyFourthDefClassRegistered)
        {
            Note(FortyFourthDefClassSite, "Init Thing Components", "Defs",
                $"004F3630 0044C6B0 {FortyFourthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FortyFourthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FortyFourthDefClassName}");
            Note(FortyFourthDefClassFactory, "Init Thing Components", "Defs",
                $"004D8736 0044C0C0 size {FortyFourthDefClassSize} vtbl 0x{FortyFourthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FortyFourthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FortyFourthDefClass = FortyFourthDefClassName;
            FortyFourthDefClassRegistered = true;
        }
        if (!FortyFifthDefClassRegistered)
        {
            Note(FortyFifthDefClassSite, "Init Thing Components", "Defs",
                $"004F3892 0044C6B0 {FortyFifthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FortyFifthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FortyFifthDefClassName}");
            Note(FortyFifthDefClassFactory, "Init Thing Components", "Defs",
                $"004D94C8 0044C0C0 size {FortyFifthDefClassSize} vtbl 0x{FortyFifthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FortyFifthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FortyFifthDefClass = FortyFifthDefClassName;
            FortyFifthDefClassRegistered = true;
        }
        if (!FortySixthDefClassRegistered)
        {
            Note(FortySixthDefClassSite, "Init Thing Components", "Defs",
                $"004F3E4C 0044C6B0 {FortySixthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FortySixthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FortySixthDefClassName}");
            Note(FortySixthDefClassFactory, "Init Thing Components", "Defs",
                $"004D8D56 0044C0C0 size {FortySixthDefClassSize} vtbl 0x{FortySixthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FortySixthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FortySixthDefClass = FortySixthDefClassName;
            FortySixthDefClassRegistered = true;
        }
        if (!FortySeventhDefClassRegistered)
        {
            Note(FortySeventhDefClassSite, "Init Thing Components", "Defs",
                $"004F3F02 0044C6B0 {FortySeventhDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FortySeventhDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FortySeventhDefClassName}");
            Note(FortySeventhDefClassFactory, "Init Thing Components", "Defs",
                $"004D8D10 0044C0C0 size {FortySeventhDefClassSize} vtbl 0x{FortySeventhDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FortySeventhDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FortySeventhDefClass = FortySeventhDefClassName;
            FortySeventhDefClassRegistered = true;
        }
        if (!FortyEighthDefClassRegistered)
        {
            Note(FortyEighthDefClassSite, "Init Thing Components", "Defs",
                $"004F40F9 0044C6B0 {FortyEighthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FortyEighthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FortyEighthDefClassName}");
            Note(FortyEighthDefClassFactory, "Init Thing Components", "Defs",
                $"004E3DD9 004E2895 0044C0C0 size {FortyEighthDefClassSize} vtbl 0x{FortyEighthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FortyEighthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FortyEighthDefClass = FortyEighthDefClassName;
            FortyEighthDefClassRegistered = true;
        }
        if (!FortyNinthDefClassRegistered)
        {
            Note(FortyNinthDefClassSite, "Init Thing Components", "Defs",
                $"004F43C6 0044C6B0 {FortyNinthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FortyNinthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FortyNinthDefClassName}");
            Note(FortyNinthDefClassFactory, "Init Thing Components", "Defs",
                $"004E5CF2 004E3E2A 0044C0C0 size {FortyNinthDefClassSize} vtbl 0x{FortyNinthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FortyNinthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FortyNinthDefClass = FortyNinthDefClassName;
            FortyNinthDefClassRegistered = true;
        }
        if (!FiftiethDefClassRegistered)
        {
            Note(FiftiethDefClassSite, "Init Thing Components", "Defs",
                $"004F4628 0044C6B0 {FiftiethDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FiftiethDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FiftiethDefClassName}");
            Note(FiftiethDefClassFactory, "Init Thing Components", "Defs",
                $"004E2AFA 004E0B9C 0044C0C0 size {FiftiethDefClassSize} vtbl 0x{FiftiethDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FiftiethDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FiftiethDefClass = FiftiethDefClassName;
            FiftiethDefClassRegistered = true;
        }
        if (!FiftyFirstDefClassRegistered)
        {
            Note(FiftyFirstDefClassSite, "Init Thing Components", "Defs",
                $"004F46DE 0044C6B0 {FiftyFirstDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FiftyFirstDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FiftyFirstDefClassName}");
            Note(FiftyFirstDefClassFactory, "Init Thing Components", "Defs",
                $"004EBA6E 004EA1F0 0044C0C0 size {FiftyFirstDefClassSize} vtbl 0x{FiftyFirstDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FiftyFirstDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FiftyFirstDefClass = FiftyFirstDefClassName;
            FiftyFirstDefClassRegistered = true;
        }
        if (!FiftySecondDefClassRegistered)
        {
            Note(FiftySecondDefClassSite, "Init Thing Components", "Defs",
                $"004F4A16 0044C6B0 {FiftySecondDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FiftySecondDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FiftySecondDefClassName}");
            Note(FiftySecondDefClassFactory, "Init Thing Components", "Defs",
                $"004D8818 0044C0C0 size {FiftySecondDefClassSize} vtbl 0x{FiftySecondDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FiftySecondDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FiftySecondDefClass = FiftySecondDefClassName;
            FiftySecondDefClassRegistered = true;
        }
        if (!FiftyThirdDefClassRegistered)
        {
            Note(FiftyThirdDefClassSite, "Init Thing Components", "Defs",
                $"004F4DB9 0044C6B0 {FiftyThirdDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FiftyThirdDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FiftyThirdDefClassName}");
            Note(FiftyThirdDefClassFactory, "Init Thing Components", "Defs",
                $"004D9629 0044C0C0 size {FiftyThirdDefClassSize} vtbl 0x{FiftyThirdDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FiftyThirdDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FiftyThirdDefClass = FiftyThirdDefClassName;
            FiftyThirdDefClassRegistered = true;
        }
        if (!FiftyFourthDefClassRegistered)
        {
            Note(FiftyFourthDefClassSite, "Init Thing Components", "Defs",
                $"004F4F45 0044C6B0 {FiftyFourthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FiftyFourthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FiftyFourthDefClassName}");
            Note(FiftyFourthDefClassFactory, "Init Thing Components", "Defs",
                $"004E0F9C 004DEBA3 0044C0C0 size {FiftyFourthDefClassSize} vtbl 0x{FiftyFourthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FiftyFourthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FiftyFourthDefClass = FiftyFourthDefClassName;
            FiftyFourthDefClassRegistered = true;
        }
        if (!FiftyFifthDefClassRegistered)
        {
            Note(FiftyFifthDefClassSite, "Init Thing Components", "Defs",
                $"004F4FFB 0044C6B0 {FiftyFifthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FiftyFifthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FiftyFifthDefClassName}");
            Note(FiftyFifthDefClassFactory, "Init Thing Components", "Defs",
                $"004D885E 004D4C3E 0044C0C0 size {FiftyFifthDefClassSize} vtbl 0x{FiftyFifthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FiftyFifthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FiftyFifthDefClass = FiftyFifthDefClassName;
            FiftyFifthDefClassRegistered = true;
        }
        if (!FiftySixthDefClassRegistered)
        {
            Note(FiftySixthDefClassSite, "Init Thing Components", "Defs",
                $"004F55B5 0044C6B0 {FiftySixthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FiftySixthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FiftySixthDefClassName}");
            Note(FiftySixthDefClassFactory, "Init Thing Components", "Defs",
                $"004D4E07 007ABB30 009FBEC0 size {FiftySixthDefClassSize} vtbl 0x{FiftySixthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FiftySixthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FiftySixthDefClass = FiftySixthDefClassName;
            FiftySixthDefClassRegistered = true;
        }
        if (!FiftySeventhDefClassRegistered)
        {
            Note(FiftySeventhDefClassSite, "Init Thing Components", "Defs",
                $"004F566B 0044C6B0 {FiftySeventhDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FiftySeventhDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FiftySeventhDefClassName}");
            Note(FiftySeventhDefClassFactory, "Init Thing Components", "Defs",
                $"004D88FC 0044C0C0 size {FiftySeventhDefClassSize} vtbl 0x{FiftySeventhDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FiftySeventhDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FiftySeventhDefClass = FiftySeventhDefClassName;
            FiftySeventhDefClassRegistered = true;
        }
        if (!FiftyEighthDefClassRegistered)
        {
            Note(FiftyEighthDefClassSite, "Init Thing Components", "Defs",
                $"004F5721 0044C6B0 {FiftyEighthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FiftyEighthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FiftyEighthDefClassName}");
            Note(FiftyEighthDefClassFactory, "Init Thing Components", "Defs",
                $"004E0D4C 004DE8C2 0044C0C0 size {FiftyEighthDefClassSize} vtbl 0x{FiftyEighthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FiftyEighthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FiftyEighthDefClass = FiftyEighthDefClassName;
            FiftyEighthDefClassRegistered = true;
        }
        if (!FiftyNinthDefClassRegistered)
        {
            Note(FiftyNinthDefClassSite, "Init Thing Components", "Defs",
                $"004F5918 0044C6B0 {FiftyNinthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {FiftyNinthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {FiftyNinthDefClassName}");
            Note(FiftyNinthDefClassFactory, "Init Thing Components", "Defs",
                $"004E0DB9 004DE8F5 0044C0C0 size {FiftyNinthDefClassSize} vtbl 0x{FiftyNinthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {FiftyNinthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            FiftyNinthDefClass = FiftyNinthDefClassName;
            FiftyNinthDefClassRegistered = true;
        }
        if (!SixtiethDefClassRegistered)
        {
            Note(SixtiethDefClassSite, "Init Thing Components", "Defs",
                $"004F5A39 0044C6B0 {SixtiethDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SixtiethDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SixtiethDefClassName}");
            Note(SixtiethDefClassFactory, "Init Thing Components", "Defs",
                $"004D89EC 0044C0C0 size {SixtiethDefClassSize} vtbl 0x{SixtiethDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SixtiethDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SixtiethDefClass = SixtiethDefClassName;
            SixtiethDefClassRegistered = true;
        }
        if (!SixtyFirstDefClassRegistered)
        {
            Note(SixtyFirstDefClassSite, "Init Thing Components", "Defs",
                $"004F5AEF 0044C6B0 {SixtyFirstDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SixtyFirstDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SixtyFirstDefClassName}");
            Note(SixtyFirstDefClassFactory, "Init Thing Components", "Defs",
                $"004D89B4 004D4FCF 0044C0C0 size {SixtyFirstDefClassSize} vtbl 0x{SixtyFirstDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SixtyFirstDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SixtyFirstDefClass = SixtyFirstDefClassName;
            SixtyFirstDefClassRegistered = true;
        }
        if (!SixtySecondDefClassRegistered)
        {
            Note(SixtySecondDefClassSite, "Init Thing Components", "Defs",
                $"004F5BA5 0044C6B0 {SixtySecondDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SixtySecondDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SixtySecondDefClassName}");
            Note(SixtySecondDefClassFactory, "Init Thing Components", "Defs",
                $"004D8A32 004D5056 0044C0C0 size {SixtySecondDefClassSize} vtbl 0x{SixtySecondDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SixtySecondDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SixtySecondDefClass = SixtySecondDefClassName;
            SixtySecondDefClassRegistered = true;
        }
        if (!SixtyThirdDefClassRegistered)
        {
            Note(SixtyThirdDefClassSite, "Init Thing Components", "Defs",
                $"004F5CC6 0044C6B0 {SixtyThirdDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SixtyThirdDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SixtyThirdDefClassName}");
            Note(SixtyThirdDefClassFactory, "Init Thing Components", "Defs",
                $"004D8A6A 0044C0C0 size {SixtyThirdDefClassSize} vtbl 0x{SixtyThirdDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SixtyThirdDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SixtyThirdDefClass = SixtyThirdDefClassName;
            SixtyThirdDefClassRegistered = true;
        }
        if (!SixtyFourthDefClassRegistered)
        {
            Note(SixtyFourthDefClassSite, "Init Thing Components", "Defs",
                $"004F5D7C 0044C6B0 {SixtyFourthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SixtyFourthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SixtyFourthDefClassName}");
            Note(SixtyFourthDefClassFactory, "Init Thing Components", "Defs",
                $"004D8AB0 0044C0C0 size {SixtyFourthDefClassSize} vtbl 0x{SixtyFourthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SixtyFourthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SixtyFourthDefClass = SixtyFourthDefClassName;
            SixtyFourthDefClassRegistered = true;
        }
        if (!SixtyFifthDefClassRegistered)
        {
            Note(SixtyFifthDefClassSite, "Init Thing Components", "Defs",
                $"004F5E32 0044C6B0 {SixtyFifthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SixtyFifthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SixtyFifthDefClassName}");
            Note(SixtyFifthDefClassFactory, "Init Thing Components", "Defs",
                $"004D8AF6 0044C0C0 size {SixtyFifthDefClassSize} vtbl 0x{SixtyFifthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SixtyFifthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SixtyFifthDefClass = SixtyFifthDefClassName;
            SixtyFifthDefClassRegistered = true;
        }
        if (!SixtySixthDefClassRegistered)
        {
            Note(SixtySixthDefClassSite, "Init Thing Components", "Defs",
                $"004F6029 0044C6B0 {SixtySixthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SixtySixthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SixtySixthDefClassName}");
            Note(SixtySixthDefClassFactory, "Init Thing Components", "Defs",
                $"004D8BE1 0044C0C0 size {SixtySixthDefClassSize} vtbl 0x{SixtySixthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SixtySixthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SixtySixthDefClass = SixtySixthDefClassName;
            SixtySixthDefClassRegistered = true;
        }
        if (!SixtySeventhDefClassRegistered)
        {
            Note(SixtySeventhDefClassSite, "Init Thing Components", "Defs",
                $"004F60DF 0044C6B0 {SixtySeventhDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SixtySeventhDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SixtySeventhDefClassName}");
            Note(SixtySeventhDefClassFactory, "Init Thing Components", "Defs",
                $"004EC526 004EBBA3 0044C0C0 size {SixtySeventhDefClassSize} vtbl 0x{SixtySeventhDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SixtySeventhDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SixtySeventhDefClass = SixtySeventhDefClassName;
            SixtySeventhDefClassRegistered = true;
        }
        if (!SixtyEighthDefClassRegistered)
        {
            Note(SixtyEighthDefClassSite, "Init Thing Components", "Defs",
                $"004F63AC 0044C6B0 {SixtyEighthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SixtyEighthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SixtyEighthDefClassName}");
            Note(SixtyEighthDefClassFactory, "Init Thing Components", "Defs",
                $"004D8C91 0044C0C0 size {SixtyEighthDefClassSize} vtbl 0x{SixtyEighthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SixtyEighthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SixtyEighthDefClass = SixtyEighthDefClassName;
            SixtyEighthDefClassRegistered = true;
        }
        if (!SixtyNinthDefClassRegistered)
        {
            Note(SixtyNinthDefClassSite, "Init Thing Components", "Defs",
                $"004F67BA 0044C6B0 {SixtyNinthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SixtyNinthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SixtyNinthDefClassName}");
            Note(SixtyNinthDefClassFactory, "Init Thing Components", "Defs",
                $"004D8EC5 0044C0C0 size {SixtyNinthDefClassSize} vtbl 0x{SixtyNinthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SixtyNinthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SixtyNinthDefClass = SixtyNinthDefClassName;
            SixtyNinthDefClassRegistered = true;
        }
        if (!SeventiethDefClassRegistered)
        {
            Note(SeventiethDefClassSite, "Init Thing Components", "Defs",
                $"004F6946 0044C6B0 {SeventiethDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SeventiethDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SeventiethDefClassName}");
            Note(SeventiethDefClassFactory, "Init Thing Components", "Defs",
                $"004D926A 0044C0C0 size {SeventiethDefClassSize} vtbl 0x{SeventiethDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SeventiethDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SeventiethDefClass = SeventiethDefClassName;
            SeventiethDefClassRegistered = true;
        }
        if (!SeventyFirstDefClassRegistered)
        {
            Note(SeventyFirstDefClassSite, "Init Thing Components", "Defs",
                $"004F6991 0044C6B0 {SeventyFirstDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SeventyFirstDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SeventyFirstDefClassName}");
            Note(SeventyFirstDefClassFactory, "Init Thing Components", "Defs",
                $"004D7C2D 0044C0C0 size {SeventyFirstDefClassSize} vtbl 0x{SeventyFirstDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SeventyFirstDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SeventyFirstDefClass = SeventyFirstDefClassName;
            SeventyFirstDefClassRegistered = true;
        }
        if (!SeventySecondDefClassRegistered)
        {
            Note(SeventySecondDefClassSite, "Init Thing Components", "Defs",
                $"004F69DC 0044C6B0 {SeventySecondDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SeventySecondDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SeventySecondDefClassName}");
            Note(SeventySecondDefClassFactory, "Init Thing Components", "Defs",
                $"004E2D3B 004E1049 0044C0C0 size {SeventySecondDefClassSize} vtbl 0x{SeventySecondDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SeventySecondDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SeventySecondDefClass = SeventySecondDefClassName;
            SeventySecondDefClassRegistered = true;
        }
        if (!SeventyThirdDefClassRegistered)
        {
            Note(SeventyThirdDefClassSite, "Init Thing Components", "Defs",
                $"004F6A27 0044C6B0 {SeventyThirdDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SeventyThirdDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SeventyThirdDefClassName}");
            Note(SeventyThirdDefClassFactory, "Init Thing Components", "Defs",
                $"004E2DB2 004E1195 0044C0C0 size {SeventyThirdDefClassSize} vtbl 0x{SeventyThirdDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SeventyThirdDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SeventyThirdDefClass = SeventyThirdDefClassName;
            SeventyThirdDefClassRegistered = true;
        }
        if (!SeventyFourthDefClassRegistered)
        {
            Note(SeventyFourthDefClassSite, "Init Thing Components", "Defs",
                $"004F6A72 0044C6B0 {SeventyFourthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SeventyFourthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SeventyFourthDefClassName}");
            Note(SeventyFourthDefClassFactory, "Init Thing Components", "Defs",
                $"004D8F51 0044C0C0 size {SeventyFourthDefClassSize} vtbl 0x{SeventyFourthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SeventyFourthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SeventyFourthDefClass = SeventyFourthDefClassName;
            SeventyFourthDefClassRegistered = true;
        }
        if (!SeventyFifthDefClassRegistered)
        {
            Note(SeventyFifthDefClassSite, "Init Thing Components", "Defs",
                $"004F6B28 0044C6B0 {SeventyFifthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SeventyFifthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SeventyFifthDefClassName}");
            Note(SeventyFifthDefClassFactory, "Init Thing Components", "Defs",
                $"004E2D70 004E1105 size {SeventyFifthDefClassSize} vtbl 0x{SeventyFifthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SeventyFifthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SeventyFifthDefClass = SeventyFifthDefClassName;
            SeventyFifthDefClassRegistered = true;
        }
        if (!SeventySixthDefClassRegistered)
        {
            Note(SeventySixthDefClassSite, "Init Thing Components", "Defs",
                $"004F6BDE 0044C6B0 {SeventySixthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SeventySixthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SeventySixthDefClassName}");
            Note(SeventySixthDefClassFactory, "Init Thing Components", "Defs",
                $"004D8F97 0044C0C0 size {SeventySixthDefClassSize} vtbl 0x{SeventySixthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SeventySixthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SeventySixthDefClass = SeventySixthDefClassName;
            SeventySixthDefClassRegistered = true;
        }
        if (!SeventySeventhDefClassRegistered)
        {
            Note(SeventySeventhDefClassSite, "Init Thing Components", "Defs",
                $"004F6D6A 0044C6B0 {SeventySeventhDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SeventySeventhDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SeventySeventhDefClassName}");
            Note(SeventySeventhDefClassFactory, "Init Thing Components", "Defs",
                $"004E11C3 004DED22 0044C0C0 size {SeventySeventhDefClassSize} vtbl 0x{SeventySeventhDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SeventySeventhDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SeventySeventhDefClass = SeventySeventhDefClassName;
            SeventySeventhDefClassRegistered = true;
        }
        if (!SeventyEighthDefClassRegistered)
        {
            Note(SeventyEighthDefClassSite, "Init Thing Components", "Defs",
                $"004F6EF6 0044C6B0 {SeventyEighthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SeventyEighthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SeventyEighthDefClassName}");
            Note(SeventyEighthDefClassFactory, "Init Thing Components", "Defs",
                $"004D9047 0044C0C0 size {SeventyEighthDefClassSize} vtbl 0x{SeventyEighthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SeventyEighthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SeventyEighthDefClass = SeventyEighthDefClassName;
            SeventyEighthDefClassRegistered = true;
        }
        if (!SeventyNinthDefClassRegistered)
        {
            Note(SeventyNinthDefClassSite, "Init Thing Components", "Defs",
                $"004F6FAC 0044C6B0 {SeventyNinthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {SeventyNinthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {SeventyNinthDefClassName}");
            Note(SeventyNinthDefClassFactory, "Init Thing Components", "Defs",
                $"004D8F0B 0044C0C0 size {SeventyNinthDefClassSize} vtbl 0x{SeventyNinthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {SeventyNinthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            SeventyNinthDefClass = SeventyNinthDefClassName;
            SeventyNinthDefClassRegistered = true;
        }
        if (!EightiethDefClassRegistered)
        {
            Note(EightiethDefClassSite, "Init Thing Components", "Defs",
                $"004F71EA 0044C6B0 {EightiethDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {EightiethDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {EightiethDefClassName}");
            Note(EightiethDefClassFactory, "Init Thing Components", "Defs",
                $"004D90C6 0044C0C0 size {EightiethDefClassSize} vtbl 0x{EightiethDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {EightiethDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            EightiethDefClass = EightiethDefClassName;
            EightiethDefClassRegistered = true;
        }
        if (!EightyFirstDefClassRegistered)
        {
            Note(EightyFirstDefClassSite, "Init Thing Components", "Defs",
                $"004F7294 0044C6B0 {EightyFirstDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {EightyFirstDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {EightyFirstDefClassName}");
            Note(EightyFirstDefClassFactory, "Init Thing Components", "Defs",
                $"004D910C 0044C0C0 size {EightyFirstDefClassSize} vtbl 0x{EightyFirstDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {EightyFirstDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            EightyFirstDefClass = EightyFirstDefClassName;
            EightyFirstDefClassRegistered = true;
        }
        if (!EightySecondDefClassRegistered)
        {
            Note(EightySecondDefClassSite, "Init Thing Components", "Defs",
                $"004F733E 0044C6B0 {EightySecondDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {EightySecondDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {EightySecondDefClassName}");
            Note(EightySecondDefClassFactory, "Init Thing Components", "Defs",
                $"004D9152 0044C0C0 size {EightySecondDefClassSize} vtbl 0x{EightySecondDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {EightySecondDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            EightySecondDefClass = EightySecondDefClassName;
            EightySecondDefClassRegistered = true;
        }
        if (!EightyThirdDefClassRegistered)
        {
            Note(EightyThirdDefClassSite, "Init Thing Components", "Defs",
                $"004F7443 0044C6B0 {EightyThirdDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {EightyThirdDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {EightyThirdDefClassName}");
            Note(EightyThirdDefClassFactory, "Init Thing Components", "Defs",
                $"004E3096 004E1341 0044C0C0 size {EightyThirdDefClassSize} vtbl 0x{EightyThirdDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {EightyThirdDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            EightyThirdDefClass = EightyThirdDefClassName;
            EightyThirdDefClassRegistered = true;
        }
        if (!EightyFourthDefClassRegistered)
        {
            Note(EightyFourthDefClassSite, "Init Thing Components", "Defs",
                $"004F760A 0044C6B0 {EightyFourthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {EightyFourthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {EightyFourthDefClassName}");
            Note(EightyFourthDefClassFactory, "Init Thing Components", "Defs",
                $"004D91DE 0044C0C0 size {EightyFourthDefClassSize} vtbl 0x{EightyFourthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {EightyFourthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            EightyFourthDefClass = EightyFourthDefClassName;
            EightyFourthDefClassRegistered = true;
        }
        if (!EightyFifthDefClassRegistered)
        {
            Note(EightyFifthDefClassSite, "Init Thing Components", "Defs",
                $"004F76B4 0044C6B0 {EightyFifthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {EightyFifthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {EightyFifthDefClassName}");
            Note(EightyFifthDefClassFactory, "Init Thing Components", "Defs",
                $"004E13AD 004DEF86 0044C0C0 size {EightyFifthDefClassSize} vtbl 0x{EightyFifthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {EightyFifthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            EightyFifthDefClass = EightyFifthDefClassName;
            EightyFifthDefClassRegistered = true;
        }
        if (!EightySixthDefClassRegistered)
        {
            Note(EightySixthDefClassSite, "Init Thing Components", "Defs",
                $"004F775E 0044C6B0 {EightySixthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {EightySixthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {EightySixthDefClassName}");
            Note(EightySixthDefClassFactory, "Init Thing Components", "Defs",
                $"004D9224 0044C0C0 size {EightySixthDefClassSize} vtbl 0x{EightySixthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {EightySixthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            EightySixthDefClass = EightySixthDefClassName;
            EightySixthDefClassRegistered = true;
        }
        if (!EightySeventhDefClassRegistered)
        {
            Note(EightySeventhDefClassSite, "Init Thing Components", "Defs",
                $"004F7808 0044C6B0 {EightySeventhDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {EightySeventhDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {EightySeventhDefClassName}");
            Note(EightySeventhDefClassFactory, "Init Thing Components", "Defs",
                $"004D9198 0044C0C0 size {EightySeventhDefClassSize} vtbl 0x{EightySeventhDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {EightySeventhDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            EightySeventhDefClass = EightySeventhDefClassName;
            EightySeventhDefClassRegistered = true;
        }
        if (!EightyEighthDefClassRegistered)
        {
            Note(EightyEighthDefClassSite, "Init Thing Components", "Defs",
                $"004F7911 0044C6B0 {EightyEighthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {EightyEighthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {EightyEighthDefClassName}");
            Note(EightyEighthDefClassFactory, "Init Thing Components", "Defs",
                $"004E6232 004E3F21 00430370 size {EightyEighthDefClassSize} vtbl 0x{EightyEighthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {EightyEighthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            EightyEighthDefClass = EightyEighthDefClassName;
            EightyEighthDefClassRegistered = true;
        }
        if (!EightyNinthDefClassRegistered)
        {
            Note(EightyNinthDefClassSite, "Init Thing Components", "Defs",
                $"004F79BB 0044C6B0 {EightyNinthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {EightyNinthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {EightyNinthDefClassName}");
            Note(EightyNinthDefClassFactory, "Init Thing Components", "Defs",
                $"004D92B0 004D5ECA 0044C0C0 size {EightyNinthDefClassSize} vtbl 0x{EightyNinthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {EightyNinthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            EightyNinthDefClass = EightyNinthDefClassName;
            EightyNinthDefClassRegistered = true;
        }
        if (!NinetiethDefClassRegistered)
        {
            Note(NinetiethDefClassSite, "Init Thing Components", "Defs",
                $"004F7A65 0044C6B0 {NinetiethDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {NinetiethDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {NinetiethDefClassName}");
            Note(NinetiethDefClassFactory, "Init Thing Components", "Defs",
                $"004E4748 00430370 size {NinetiethDefClassSize} vtbl 0x{NinetiethDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {NinetiethDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            NinetiethDefClass = NinetiethDefClassName;
            NinetiethDefClassRegistered = true;
        }
        if (!NinetyFirstDefClassRegistered)
        {
            Note(NinetyFirstDefClassSite, "Init Thing Components", "Defs",
                $"004F7AB0 0044C6B0 {NinetyFirstDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {NinetyFirstDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {NinetyFirstDefClassName}");
            Note(NinetyFirstDefClassFactory, "Init Thing Components", "Defs",
                $"004E47BF 00430370 size {NinetyFirstDefClassSize} vtbl 0x{NinetyFirstDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {NinetyFirstDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            NinetyFirstDefClass = NinetyFirstDefClassName;
            NinetyFirstDefClassRegistered = true;
        }
        if (!NinetySecondDefClassRegistered)
        {
            Note(NinetySecondDefClassSite, "Init Thing Components", "Defs",
                $"004F7AFB 0044C6B0 {NinetySecondDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {NinetySecondDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {NinetySecondDefClassName}");
            Note(NinetySecondDefClassFactory, "Init Thing Components", "Defs",
                $"004E4624 00430370 size {NinetySecondDefClassSize} vtbl 0x{NinetySecondDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {NinetySecondDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            NinetySecondDefClass = NinetySecondDefClassName;
            NinetySecondDefClassRegistered = true;
        }
        if (!NinetyThirdDefClassRegistered)
        {
            Note(NinetyThirdDefClassSite, "Init Thing Components", "Defs",
                $"004F7B46 0044C6B0 {NinetyThirdDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {NinetyThirdDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {NinetyThirdDefClassName}");
            Note(NinetyThirdDefClassFactory, "Init Thing Components", "Defs",
                $"004E4667 00430370 size {NinetyThirdDefClassSize} vtbl 0x{NinetyThirdDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {NinetyThirdDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            NinetyThirdDefClass = NinetyThirdDefClassName;
            NinetyThirdDefClassRegistered = true;
        }
        if (!NinetyFourthDefClassRegistered)
        {
            Note(NinetyFourthDefClassSite, "Init Thing Components", "Defs",
                $"004F7B91 0044C6B0 {NinetyFourthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {NinetyFourthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {NinetyFourthDefClassName}");
            Note(NinetyFourthDefClassFactory, "Init Thing Components", "Defs",
                $"004E46A4 00430370 size {NinetyFourthDefClassSize} vtbl 0x{NinetyFourthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {NinetyFourthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            NinetyFourthDefClass = NinetyFourthDefClassName;
            NinetyFourthDefClassRegistered = true;
        }
        if (!NinetyFifthDefClassRegistered)
        {
            Note(NinetyFifthDefClassSite, "Init Thing Components", "Defs",
                $"004F7BDC 0044C6B0 {NinetyFifthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {NinetyFifthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {NinetyFifthDefClassName}");
            Note(NinetyFifthDefClassFactory, "Init Thing Components", "Defs",
                $"004E45CE 00430370 size {NinetyFifthDefClassSize} vtbl 0x{NinetyFifthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {NinetyFifthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            NinetyFifthDefClass = NinetyFifthDefClassName;
            NinetyFifthDefClassRegistered = true;
        }
        if (!NinetySixthDefClassRegistered)
        {
            Note(NinetySixthDefClassSite, "Init Thing Components", "Defs",
                $"004F7C27 0044C6B0 {NinetySixthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {NinetySixthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {NinetySixthDefClassName}");
            Note(NinetySixthDefClassFactory, "Init Thing Components", "Defs",
                $"004E4833 00430370 size {NinetySixthDefClassSize} vtbl 0x{NinetySixthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {NinetySixthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            NinetySixthDefClass = NinetySixthDefClassName;
            NinetySixthDefClassRegistered = true;
        }
        if (!NinetySeventhDefClassRegistered)
        {
            Note(NinetySeventhDefClassSite, "Init Thing Components", "Defs",
                $"004F7C72 0044C6B0 {NinetySeventhDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {NinetySeventhDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {NinetySeventhDefClassName}");
            Note(NinetySeventhDefClassFactory, "Init Thing Components", "Defs",
                $"004E4883 00430370 size {NinetySeventhDefClassSize} vtbl 0x{NinetySeventhDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {NinetySeventhDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            NinetySeventhDefClass = NinetySeventhDefClassName;
            NinetySeventhDefClassRegistered = true;
        }
        if (!NinetyEighthDefClassRegistered)
        {
            Note(NinetyEighthDefClassSite, "Init Thing Components", "Defs",
                $"004F7D1C 0044C6B0 {NinetyEighthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {NinetyEighthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {NinetyEighthDefClassName}");
            Note(NinetyEighthDefClassFactory, "Init Thing Components", "Defs",
                $"004E6CF3 00430370 size {NinetyEighthDefClassSize} vtbl 0x{NinetyEighthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {NinetyEighthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            NinetyEighthDefClass = NinetyEighthDefClassName;
            NinetyEighthDefClassRegistered = true;
        }
        if (!NinetyNinthDefClassRegistered)
        {
            Note(NinetyNinthDefClassSite, "Init Thing Components", "Defs",
                $"004F7DC6 0044C6B0 {NinetyNinthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {NinetyNinthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {NinetyNinthDefClassName}");
            Note(NinetyNinthDefClassFactory, "Init Thing Components", "Defs",
                $"004D9321 0044C0C0 size {NinetyNinthDefClassSize} vtbl 0x{NinetyNinthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {NinetyNinthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            NinetyNinthDefClass = NinetyNinthDefClassName;
            NinetyNinthDefClassRegistered = true;
        }
        if (!HundredthDefClassRegistered)
        {
            Note(HundredthDefClassSite, "Init Thing Components", "Defs",
                $"004F7F2A 0044C6B0 {HundredthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {HundredthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {HundredthDefClassName}");
            Note(HundredthDefClassFactory, "Init Thing Components", "Defs",
                $"004E3290 004E1722 0044C0C0 size {HundredthDefClassSize} vtbl 0x{HundredthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {HundredthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            HundredthDefClass = HundredthDefClassName;
            HundredthDefClassRegistered = true;
        }
        if (!HundredFirstDefClassRegistered)
        {
            Note(HundredFirstDefClassSite, "Init Thing Components", "Defs",
                $"004F820A 0044C6B0 {HundredFirstDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {HundredFirstDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {HundredFirstDefClassName}");
            Note(HundredFirstDefClassFactory, "Init Thing Components", "Defs",
                $"004D8799 0044C0C0 size {HundredFirstDefClassSize} vtbl 0x{HundredFirstDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {HundredFirstDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            HundredFirstDefClass = HundredFirstDefClassName;
            HundredFirstDefClassRegistered = true;
        }
        if (!HundredSecondDefClassRegistered)
        {
            Note(HundredSecondDefClassSite, "Init Thing Components", "Defs",
                $"004F8255 0044C6B0 {HundredSecondDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {HundredSecondDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {HundredSecondDefClassName}");
            Note(HundredSecondDefClassFactory, "Init Thing Components", "Defs",
                $"004D8411 004D405A 0044C0C0 size {HundredSecondDefClassSize} vtbl 0x{HundredSecondDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {HundredSecondDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            HundredSecondDefClass = HundredSecondDefClassName;
            HundredSecondDefClassRegistered = true;
        }
        if (!HundredThirdDefClassRegistered)
        {
            Note(HundredThirdDefClassSite, "Init Thing Components", "Defs",
                $"004F82A0 0044C6B0 {HundredThirdDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {HundredThirdDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {HundredThirdDefClassName}");
            Note(HundredThirdDefClassFactory, "Init Thing Components", "Defs",
                $"004E32E3 004E1748 0044C0C0 size {HundredThirdDefClassSize} vtbl 0x{HundredThirdDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {HundredThirdDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            HundredThirdDefClass = HundredThirdDefClassName;
            HundredThirdDefClassRegistered = true;
        }
        if (!HundredFourthDefClassRegistered)
        {
            Note(HundredFourthDefClassSite, "Init Thing Components", "Defs",
                $"004F8420 0044C6B0 {HundredFourthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {HundredFourthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {HundredFourthDefClassName}");
            Note(HundredFourthDefClassFactory, "Init Thing Components", "Defs",
                $"004D93A0 0044C0C0 size {HundredFourthDefClassSize} vtbl 0x{HundredFourthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {HundredFourthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            HundredFourthDefClass = HundredFourthDefClassName;
            HundredFourthDefClassRegistered = true;
        }
        if (!HundredFifthDefClassRegistered)
        {
            Note(HundredFifthDefClassSite, "Init Thing Components", "Defs",
                $"004F84D6 0044C6B0 {HundredFifthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {HundredFifthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {HundredFifthDefClassName}");
            Note(HundredFifthDefClassFactory, "Init Thing Components", "Defs",
                $"004D93E6 0044C0C0 size {HundredFifthDefClassSize} vtbl 0x{HundredFifthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {HundredFifthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            HundredFifthDefClass = HundredFifthDefClassName;
            HundredFifthDefClassRegistered = true;
        }
        if (!HundredSixthDefClassRegistered)
        {
            Note(HundredSixthDefClassSite, "Init Thing Components", "Defs",
                $"004F85F7 0044C6B0 {HundredSixthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {HundredSixthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {HundredSixthDefClassName}");
            Note(HundredSixthDefClassFactory, "Init Thing Components", "Defs",
                $"004D9465 0044C0C0 size {HundredSixthDefClassSize} vtbl 0x{HundredSixthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {HundredSixthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            HundredSixthDefClass = HundredSixthDefClassName;
            HundredSixthDefClassRegistered = true;
        }
        if (!HundredSeventhDefClassRegistered)
        {
            Note(HundredSeventhDefClassSite, "Init Thing Components", "Defs",
                $"004F899A 0044C6B0 {HundredSeventhDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {HundredSeventhDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {HundredSeventhDefClassName}");
            Note(HundredSeventhDefClassFactory, "Init Thing Components", "Defs",
                $"004D96DD 0044C0C0 size {HundredSeventhDefClassSize} vtbl 0x{HundredSeventhDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {HundredSeventhDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            HundredSeventhDefClass = HundredSeventhDefClassName;
            HundredSeventhDefClassRegistered = true;
        }
        if (!HundredEighthDefClassRegistered)
        {
            Note(HundredEighthDefClassSite, "Init Thing Components", "Defs",
                $"004F8B91 0044C6B0 {HundredEighthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {HundredEighthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {HundredEighthDefClassName}");
            Note(HundredEighthDefClassFactory, "Init Thing Components", "Defs",
                $"004D97E9 0044C0C0 size {HundredEighthDefClassSize} vtbl 0x{HundredEighthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {HundredEighthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            HundredEighthDefClass = HundredEighthDefClassName;
            HundredEighthDefClassRegistered = true;
        }
        if (!HundredNinthDefClassRegistered)
        {
            Note(HundredNinthDefClassSite, "Init Thing Components", "Defs",
                $"004F8C47 0044C6B0 {HundredNinthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {HundredNinthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {HundredNinthDefClassName}");
            Note(HundredNinthDefClassFactory, "Init Thing Components", "Defs",
                $"004D982F 004D6638 0044C0C0 size {HundredNinthDefClassSize} vtbl 0x{HundredNinthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {HundredNinthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            HundredNinthDefClass = HundredNinthDefClassName;
            HundredNinthDefClassRegistered = true;
        }
        if (!HundredTenthDefClassRegistered)
        {
            Note(HundredTenthDefClassSite, "Init Thing Components", "Defs",
                $"004F8D68 0044C6B0 {HundredTenthDefClassName}");
            Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
                $"0042DAE0 {HundredTenthDefClassName}");
            Note(AddDefClassFn, "Init Thing Components", "Defs",
                $"009B0AC0 Add Def Class {HundredTenthDefClassName}");
            Note(HundredTenthDefClassFactory, "Init Thing Components", "Defs",
                $"004D9882 0044C0C0 size {HundredTenthDefClassSize} vtbl 0x{HundredTenthDefClassVtbl:X}");
            Note(LoadDefFn, "Init Thing Components", "Defs",
                $"009AD6E0 {HundredTenthDefClassName}");
            Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
                $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
            HundredTenthDefClass = HundredTenthDefClassName;
            HundredTenthDefClassRegistered = true;
        }
        if (HundredEleventhDefClassRegistered)
            return;
        Note(HundredEleventhDefClassSite, "Init Thing Components", "Defs",
            $"004F8E89 0044C6B0 {HundredEleventhDefClassName}");
        Note(NinthDefClassPackFn, "Init Thing Components", "Defs",
            $"0042DAE0 {HundredEleventhDefClassName}");
        Note(AddDefClassFn, "Init Thing Components", "Defs",
            $"009B0AC0 Add Def Class {HundredEleventhDefClassName}");
        Note(HundredEleventhDefClassFactory, "Init Thing Components", "Defs",
            $"004D98C8 0044C0C0 size {HundredEleventhDefClassSize} vtbl 0x{HundredEleventhDefClassVtbl:X}");
        Note(LoadDefFn, "Init Thing Components", "Defs",
            $"009AD6E0 {HundredEleventhDefClassName}");
        Note(LoadDefBudgetFn, "Init Thing Components", "Defs",
            $"009FC4F0 [this+40]={PlayerManagerPlus40Cap:X}");
        Note(ThingComponentsFillSite, "Init Thing Components", "Defs",
            $"004F9129 {ThingComponentsFillFn:X8} tag {ThingComponentsFillFirstTag} thunk 0x{ThingComponentsFillFirstThunk:X} limit 0x{ThingComponentsFillLimitVa:X}");
        Note(ThingComponentsFillFn, "Init Thing Components", "Defs",
            $"0073B130 00743270 00743B30 007441D0 ret 0073CB40");
        ThingComponentsFilled = true;
        Note(ThingComponentsCommitFlagSetSite, "Init Thing Components", "Defs",
            "004F9112 flag=1 before 0073B130");
        Note(ThingComponentsCommitSite, "Init Thing Components", "Defs",
            $"004F9139 {ThingComponentsCommitFn:X8} ecx=esi 004EB9A6 +{ThingComponentsCommitPlus12}/+{ThingComponentsCommitPlus13}");
        Note(ThingComponentsCommitFn, "Init Thing Components", "Defs",
            "004EBACE map commit this walk");
        ThingComponentsCommitted = true;
        Note(ThingComponentsRet, "Init Thing Components", "Defs",
            "004F9144 ret after CHasNameDef");
        HundredEleventhDefClass = HundredEleventhDefClassName;
        HundredEleventhDefClassRegistered = true;
    }

    /// <summary>
    /// <c>00416005</c>
    /// <c>0044C6B0</c>
    /// <c>[vtbl+8] 0044C72B</c>
    /// <c>009ACB10</c>
    /// <c>009E5250</c>.
    /// Parent arg is 1.
    /// Not a game.bin parse.
    /// </summary>
    private void PrepareDefinitionManager()
    {
        if (DefinitionManagerPrepared)
            return;
        Note(PlayerManagerGetter, "Init Definition Manager", "Defs",
            "0044C6B0 [0x13B879C]");
        Note(DefinitionManagerVtbl8Fn, "Init Definition Manager", "Defs",
            $"0044C72B [vtbl+{DefinitionManagerVtbl8}]");
        Note(DefinitionManagerPathPrefixVa, "Init Definition Manager", "Defs",
            "0099B6B0 " + DefinitionManagerPathPrefix);
        Note(DefinitionManagerFirstGlobVa, "Init Definition Manager", "Defs",
            "009A76A0 " + DefinitionManagerFirstGlob);
        Note(AddDefClassFn, "Init Definition Manager", "Defs",
            $"009B0AC0 {DefinitionManagerCompileFirstClass}");
        Note(DefinitionManagerCompileFn, "Init Definition Manager", "Defs",
            "009B08C0 Game Definition Manager: Compile leftover body");
        Note(DefinitionManagerResetFn, "Init Definition Manager", "Defs",
            $"009ACB10 [this+{DefinitionManagerPlus88}] arg={DefinitionManagerArg}");
        Note(DefinitionManagerResetApply, "Init Definition Manager", "Defs",
            "009E5250 list reset");
        DefinitionManagerPrepared = true;
    }

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
        Note(PlayerGuiAllocFn, "Create Players", "UI",
            $"00487FB0 alloc 0x{PlayerGuiObjectSize:X}");
        Note(PlayerGuiCtorFn, "Create Players", "UI",
            $"0043B570 vtbl {PlayerGuiVtbl:X} {PlayerGuiPcName}");
        Note(PlayerGuiStoreFn, "Create Players", "UI",
            $"004195AF [0x{PlayerGuiSingletonVa:X}]");
        Note(ThingTypeRegistrarFn, "Create Players", "Thing",
            "00522A20 PlayerCreature CREATURE 0052B880");
        Player.Register(new ActionInputListener());
        Note(PlayerListenerFactoryFn, "Create Players", "Input",
            "00488D10 00687A30 vtbl 0123758C +4");
        Note(PlayerListenerRegisterFn, "Create Players", "Input",
            "00687A70 00A0D2B0 00A0D4F0");
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
        GamePlus9 = true;
        if (GamePumpLeft)
            return;
        if (GamePumpFirstDone)
        {
            if (NoteInnerLoopHead())
                PumpGameUpdate();
            NoteInnerLoopTail();
            return;
        }

        Note(GamePump, "GamePump", "Game", "004189C2 vtbl+8");
        Note(GamePumpPlayerFn, "GamePump", "Player", "004AE9C0 game+80568");
        GamePlus96 = FrameDtNow;
        Note(FrameDtFn, "GamePump", "Time", "009E1BC0 [game+96]");
        Note(FrameDtQpcIat, "GamePump", "Time",
            "QueryPerformanceCounter IAT 0x143FE00");
        Note(PlayerCatchupFn, "GamePump", "Time",
            $"[game+{GamePlus9Offset}]={GamePlus9FirstSeen}");
        if (UseNamedStart)
        {
            Note(NamedStartFn, "GamePump", "Region", "00416268 named start");
            GamePumpFirstDone = true;
            if (NoteInnerLoopHead())
                PumpGameUpdate();
            NoteInnerLoopTail();
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
        GamePumpFirstDone = true;
        if (NoteInnerLoopHead())
            PumpGameUpdate();
        NoteInnerLoopTail();
    }

    /// <summary>
    /// <c>00418B07</c>:
    /// <c>[game+52]==0</c> first-seen so
    /// <c>009F8BA0</c> then
    /// <c>004162B5</c>, not
    /// <c>00417747</c>.
    /// </summary>
    /// <summary>
    /// One <c>004189C2</c> inner
    /// iteration. <c>009A6460</c> →
    /// <c>009A6370</c>. First-seen
    /// PeekMessage empty and
    /// <c>[engine+8]==0</c> → 1.
    /// False means return 2 (skip
    /// <c>004162B5</c>, still run tail).
    /// </summary>
    private bool NoteInnerLoopHead()
    {
        Note(GamePumpInnerStartFn, "GamePump", "Game", "0098E1B0 ret");
        Note(EngineMessagePumpFn, "GamePump", "Engine", "009A6370");
        Note(PeekMessageFn, "GamePump", "Engine",
            "009A4F20 PeekMessage first-seen empty");
        Note(GetForegroundWindowIat, "GamePump", "Engine",
            "GetForegroundWindow IAT 0x1440378 vs +148");
        EnginePlus9 = EngineWindowCreated && EngineForeground ? 1 : 0;
        if (EnginePlus88)
        {
            Note(InputFocusFn, "GamePump", "Input",
                $"009F4E20 +{EnginePlus88Offset} arg={EnginePlus9}");
        }

        Note(TestCooperativeLevelFn, "GamePump", "Engine",
            "009C00C0 TestCooperativeLevel S_OK → 1");
        Note(EngineWndProc, "GamePump", "Engine",
            $"009A5B60 table 0x{EngineWndProcJumpTable:X}");
        if (EnginePlus8 != 0)
        {
            GamePlus8 = true;
            Note(GamePumpQuitQuery, "GamePump", "Engine",
                $"009A6460 [engine+{EnginePlus8Offset}]=1 → {GamePumpQuitLeave}");
            return false;
        }

        Note(GamePumpQuitQuery, "GamePump", "Engine",
            $"009A6460 [engine+{EnginePlus8Offset}]={EnginePlus8FirstSeen} → {GamePumpQuitFirstSeen}");
        NoteInnerLoopDt();
        return true;
    }

    private void NoteInnerLoopDt()
    {
        Note(InnerLoopDtFn, "GamePump", "Time",
            $"009F8BA0 +{InnerLoopDtOffset} [game+{GamePlus52Offset}]={GamePlus52FirstSeen}");
    }

    /// <summary>
    /// After <c>004162B5</c> on the same
    /// <c>004189C2</c> inner iteration:
    /// <c>00416202</c> / <c>00415E85</c>
    /// skip / <c>0044C6B0</c>+<c>009AC9E0</c>.
    /// </summary>
    private void NoteInnerLoopTail()
    {
        if (FrameDtRingSamples < PostLoadWorldReserveCount)
            FrameDtRingSamples++;
        Note(FrameDtRingFn, "GamePump", "Time",
            $"00416202 +{GamePlus90488Offset} 0049B9E0 count={FrameDtRingSamples}");
        Note(FrameDtRingMeanFn, "GamePump", "Time",
            $"0049B9A0 +{FrameDtRingMeanOffset}");
        Note(MemlogFlagVa, "GamePump", "Game",
            $"013B85F1={MemlogFlagFirstSeen}");
        Note(GamePumpMemlog, "GamePump", "Game",
            "00415E85 skip");
        Note(PlayerManagerGetter, "GamePump", "Player",
            "0044C6B0 [0x13B879C]");
        Note(PlayerManagerIdleFn, "GamePump", "Player", "009AC9E0 ret 4");
        if (GamePlus8)
        {
            Note(GamePump, "GamePump", "Game",
                $"[game+{GamePlus8Offset}]=1 leave");
            Note(GamePumpLeaveFn, "GamePump", "Game", "004175E5");
            GamePumpLeft = true;
            return;
        }

        Note(GamePump, "GamePump", "Game",
            $"[game+{GamePlus8Offset}]={GamePlus8FirstSeen} loop");
    }

    /// <summary>
    /// WndProc <c>009A5B60</c>.
    /// <c>WM_DESTROY</c> is table slot 1
    /// <c>009A5BEA</c>:
    /// <c>[engine+232]=0</c>,
    /// <c>[engine+8]=1</c>.
    /// </summary>
    public void ApplyEngineWindowMessage(int msg)
    {
        Note(EngineWndProc, "GamePump", "Engine",
            $"009A5B60 msg={msg}");
        if (msg != WmDestroy)
            return;
        Note(EngineQuitStoreSite, "GamePump", "Engine",
            "009A5BEA WM_DESTROY [engine+8]=1");
        EnginePlus8 = 1;
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
    /// vtbl+20 / vtbl+28. Does not call
    /// vtbl+24; that is inside
    /// <c>00418289</c> after
    /// <c>004AEBA0</c> returns 1.
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

        FrameListCount = 0;
        Note(FrameListCountVa, "GamePump", "World",
            "004162CD [0x13B89A8]=0");
        Note(FrameDtFn, "GamePump", "Time", "009E1BC0 FrameDt");
        UpdateGameMode();
        Note(UpdateDtVa, "GamePump", "Time", "0x13B8690 009E1BC0-dt");
        // After 006C2170. First-seen
        // 00B428E0 already ran in 004A1840
        // and missed FinalAlbion.stb.
        if (HeroSpawned && !WorldSubmitted)
            SubmitCurrentWorld();
        Note(DisplayReadyFn, "GamePump", "Display",
            "009E9FB0 [0x13CAA38] default 0");
        RenderGameMode();
        Note(RenderDtVa, "GamePump", "Time", "0x13B8698 009E1BC0-dt");
    }

    /// <summary>
    /// <c>009A57B0</c>. Host <see cref="Pump"/>
    /// is the tick after library construct.
    /// </summary>
    public bool EvaluateEngineUpdateGate()
    {
        Note(GetForegroundWindowIat, "GamePump", "Engine",
            "009A57B0 GetForegroundWindow == [engine+148]");
        var allow = EngineWindowCreated && EngineForeground;
        EnginePlus9 = allow ? 1 : 0;
        return allow;
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
                PlayerCatchupHit = EvaluatePlayerCatchup();
                Note(PlayerActionFn, "GamePump", "Player",
                    PlayerCatchupHit
                        ? "004AEAA0 0041674A=1"
                        : "004AEAA0 0041674A=0 004AEB8A");
                if (PlayerCatchupHit)
                {
                    AppendPlayerCatchupTick();
                    Note(GameUpdateWorldFn, "GamePump", "World", "0049D9E0 ret");
                    WorldUpdateRan = true;
                    Note(GameVtbl24Fn, "GamePump", "Update", "vtbl+24 00416E78");
                    PumpPlayerInterface();
                    GameVtbl24Ran = true;
                    Note(ClearGamePlus68Fn, "GamePump", "Update", "00416047 [game+68]=0");
                    AdvanceGameTicks();
                }
            }
        }

        Note(WorldFrameGetter, "GamePump", "World",
            $"0049D870 [0x13B89BC]={WorldFrame}");
        Note(WorldFrameCopyVa, "GamePump", "World", "0x13B7D70");
        GameUpdateCount++;
    }

    /// <summary>
    /// <c>0041674A</c>. First-seen
    /// <c>[game+9]==1</c>,
    /// <c>0x13B8688==0</c> (no writer),
    /// <c>004166E2</c> is
    /// <c>009E1BC0-[game+96]</c>
    /// (slot clock 0, <c>0x13B86A4</c>
    /// no writer). First inner is 0;
    /// later inners grow with
    /// <see cref="FrameDtNow"/>.
    /// <c>+9836=[game+72]</c> ctor 0.
    /// </summary>
    public bool EvaluatePlayerCatchup()
    {
        Note(PlayerCatchupFn, "GamePump", "Time", "0041674A");
        if (!GamePlus9)
        {
            Note(PlayerCatchupFn, "GamePump", "Time", "[game+9]=0");
            return false;
        }

        Note(PlayerCatchupMenuVa, "GamePump", "Time",
            $"013B860C={PlayerCatchupMenuFirstSeen}");
        Note(PlayerCatchupForceVa, "GamePump", "Time",
            $"013B8688={PlayerCatchupForceFirstSeen} no writer");
        Note(DisplayClockForceQpcVa, "GamePump", "Time",
            $"013B86A4={DisplayClockForceQpcFirstSeen} no writer");
        Note(PlayerCatchupTimeFn, "GamePump", "Time", "004166E2");
        var fromClock = FrameDtNow - GamePlus96;
        if (fromClock > DisplayTime)
            DisplayTime = fromClock;
        Note(PlayerCatchupTimeFn, "GamePump", "Time",
            $"004166E2 009E1BC0-[game+96]={DisplayTime}");
        var scaled = DisplayTime * CameraCatchupMin - PlayerBindSlot0;
        var hit = scaled > CameraInvArgOne;
        Note(PlayerCatchupFn, "GamePump", "Time",
            hit
                ? $"004166E2*{CameraCatchupMin}-{PlayerBindSlot0} > 1"
                : $"004166E2*{CameraCatchupMin}-{PlayerBindSlot0} <= 1");
        return hit;
    }

    /// <summary>
    /// <c>00416E78</c>: prefix always
    /// (<c>[world+52].vtbl+4</c>,
    /// <c>00416392</c>, <c>009F4A90</c>,
    /// input <c>vtbl+8</c>).
    /// <c>004457F0</c> / <c>00446A30</c>
    /// only after <c>WorldFrame&gt;1</c>.
    /// Reached only when
    /// <c>004AEBA0</c> returns 1.
    /// </summary>
    public void PumpPlayerInterface()
    {
        Note(GameVtbl24Fn, "GamePump", "Update",
            $"[world+{WorldPlus52Offset}].vtbl+{WorldPlus52Vtbl}");
        Note(WorldThingCountFn, "GamePump", "Update",
            $"00416392 +{GamePlus90394Offset}=0 → 0049E200");
        Note(InputStoreRecordFn, "GamePump", "Input",
            $"009F4A90 [0x13B8388]+{InputRecordOffset}/+{InputGamePlus72Offset}=[game+72]");
        Note(InputDeviceVa, "GamePump", "Input",
            $"[0x13B8388] vtbl+{InputVtbl8}");
        InputRecordStored = true;
        Note(GetForegroundWindowIat, "GamePump", "Input",
            "00416F9D 009A57B0");
        Note(WorldFrameGetter, "GamePump", "Input",
            $"0049D870 frame={WorldFrame}");
        if (WorldFrame <= 1)
        {
            Note(GameVtbl24Fn, "GamePump", "Update",
                "0049D870<=1 skip 004457F0");
            return;
        }

        Player.Construct();
        Player.Preprocess();
        Note(PlayerInterfacePreprocess, "GamePump", "Input",
            "004457F0 [+2196]=0");
        Note(PlayerInputPumpFn, "GamePump", "Input",
            "00446A30 [game+32] vtbl+4");
        Note(PlayerInputPollFn, "GamePump", "Input",
            "00446330 009F4ED0 vtbl+32/00449990/+16");
        Note(PlayerInputFallbackFn, "GamePump", "Input",
            "00446220 vtbl+24 [+168]=0");
        var n = 0;
        while (Player.Pump(Input) && n < 32)
        {
            n++;
            if (Player.LastEvent is { } ev)
                ApplyPlayerEvent(ev);
        }

        if (n == 0)
            Note(PlayerInputPumpFn, "GamePump", "Input",
                "00446A30 al=0 no 0041649C");
        else
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
        {
            CopyDisplayPlus104();
            return;
        }
        if (!GameRenderEnabled)
        {
            Note(GameRenderFn, "GamePump", "Render", "[game+90593]=0 skip");
            CopyDisplayPlus104();
            return;
        }

        Note(WorldFrameGetter, "GamePump", "Render",
            $"0049D870 frame={WorldFrame}");
        if (WorldFrame <= 1)
        {
            Note(GameRenderFn, "GamePump", "Render",
                "WorldFrame<=1 skip camera body");
            CopyDisplayPlus104();
            return;
        }

        RenderBodyRan = true;
        if (CameraCatchupTicks <= 0)
        {
            ApplyCameraInterpolation();
            CopyDisplayPlus104();
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
        CopyDisplayPlus104();
    }

    /// <summary>
    /// <c>0041725F</c>:
    /// <c>[0x13B7D6C]=[display+104]</c>.
    /// <c>004350D0</c> first-seen 0.
    /// </summary>
    private void CopyDisplayPlus104()
    {
        DisplayPlus104 = DisplayPlus104FirstSeen;
        DisplayPlus104Copy = DisplayPlus104;
        Note(DisplayPlus104CopyVa, "GamePump", "Render",
            $"00417265 [0x13B7D6C]=[display+{DisplayPlus104Offset}]={DisplayPlus104Copy}");
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
    /// <c>0049E080</c>. <c>00435F70</c>
    /// only if <c>004AEA70</c> or
    /// <c>[0x13B8688]</c>.
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
        CameraInterpolationRan = true;
        if (FinishInterpolationDisplay())
        {
            CameraBodySteps++;
            Note(CameraInterpolationFn, "GamePump", "Render",
                $"[game+90594]=1 t={CameraInterpolationT}");
        }
    }

    /// <summary>
    /// <c>004AEA70</c>: <c>+9826==0</c>
    /// returns 1. Else
    /// <c>!0041674A([player+9848], +9836)</c>
    /// with the post-update slot.
    /// </summary>
    public bool EvaluateDisplayReady()
    {
        Note(PlayerReadyQueryFn, "GamePump", "Player",
            $"004AEA70 +{PlayerActionFlagOffset}={PlayerActionReady}");
        if (!PlayerActionReady)
        {
            Note(PlayerReadyQueryFn, "GamePump", "Player",
                "004AEA70 +9826=0 → 1");
            return true;
        }

        var scaled = DisplayTime * CameraCatchupMin - PlayerBindSlot0;
        var catchup = PlayerCatchupForceFirstSeen != 0 ||
            scaled > CameraInvArgOne;
        Note(PlayerReadyQueryFn, "GamePump", "Player",
            catchup
                ? $"004AEA70 0041674A {scaled}>1 → 0"
                : $"004AEA70 0041674A {scaled}<=1 → 1");
        return !catchup;
    }

    /// <summary>
    /// <c>004171EB</c>…<c>0041725D</c>.
    /// First-seen <c>[0x13B8688]=0</c>
    /// and <c>004AEA70=0</c> skip
    /// <c>00435F70</c> / <c>[+90594]</c>.
    /// </summary>
    public bool FinishInterpolationDisplay()
    {
        var ready = EvaluateDisplayReady();
        if (!ready)
        {
            GamePlus90596++;
            Note(PlayerReadyQueryFn, "GamePump", "Player",
                $"004171F4 [game+90596]={GamePlus90596}");
        }

        Note(PlayerCatchupForceVa, "GamePump", "Render",
            $"013B8688={PlayerCatchupForceFirstSeen} no writer");
        if (ready || PlayerCatchupForceFirstSeen != 0)
        {
            DisplayPresentSkipped = false;
            Note(CameraTimeFn, "GamePump", "Time", "00416231");
            ApplyDisplayCamera();
            GamePlus90424++;
            GamePlus104 = 0;
            GamePlus90594 = true;
            return true;
        }

        DisplayPresentSkipped = true;
        Note(DisplayApplyThunk, "GamePump", "Display",
            "004AEA70=0 [0x13B8688]=0 skip 00435F70");
        return false;
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
        // 006B42F0 writes WorldCamera
        // +6296/+6312/+6328. It does not
        // unbind camera+12. UseCamera
        // 00B23B50 sticks until the next
        // bind / ResetCamera 00CC9DF1.
        if (Camera.ScriptCameraActive)
        {
            Note(WorldCameraApplyFn, "GamePump", "Camera",
                "0049E080 skip helper write; UseCamera sticks");
            return;
        }

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
            "00435F70 jmp 00435530 push 1 +90552=1");
        Note(DisplayApplyBodyFn, "GamePump", "Display",
            DisplayPlus232 > 0
                ? $"00435530 +232={DisplayPlus232} 00434CD0"
                : "00435530 +232=0 skip 00434CD0");
        if (DisplayPlus232 > 0)
        {
            Note(DisplayFadeDestFn, "GamePump", "Display",
                "00434CD0 +216=0");
            Note(DisplayFadeDestFlagVa, "GamePump", "Display",
                $"01375CDC={DisplayFadeDestFlagFirstSeen} skip dest fade");
            Note(DisplayFadeDestStub, "GamePump", "Display",
                "009D8250 ret dest empty");
        }

        Note(BeginSceneFn, "GamePump", "D3D9", "009BEF20 BeginScene");
        Note(ClearColorFn, "GamePump", "D3D9", "009D8CF0 clear");
        Note(DisplayPlayerOverlayLookup, "GamePump", "Display",
            "00435000 00449960");
        Note(DisplayPlayerOverlayThing, "GamePump", "Display",
            "00487DD0 +44 jmp 00A01B50 miss");
        Note(DisplayPlayerOverlayFn, "GamePump", "Display",
            "00435000 skip 00639E40");
        Note(PlayerCreatureThingFn, "GamePump", "Display",
            "00435070 00487DC0 miss");
        Note(DisplayPlayerInterfaceFn, "GamePump", "Display",
            "00435070 skip 0057B43F");
        Note(DisplayFlush2dFn, "GamePump", "D3D9",
            "009D9C80 dirty-list no type 0x22");
        var shouldDip = DisplayFlushShouldDip(0, 0);
        Note(DisplayFlushLayersFn, "GamePump", "D3D9",
            shouldDip
                ? $"009DA9F0({DisplayFlushLayersArg}) [+{DisplayQueueBeginOffset}] DIP vtbl+{DrawIndexedPrimitiveVtbl}"
                : $"009DA9F0({DisplayFlushLayersArg}) [+{DisplayQueueBeginOffset}] empty dest");
        Note(DisplayFlushLayersFn, "GamePump", "D3D9",
            shouldDip
                ? $"00A058C0 then vtbl+{DrawIndexedPrimitiveVtbl}"
                : "009DA9F0 skip DIP 009DB6E6");
        if (shouldDip)
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
    /// Dispatch table slot 1 is
    /// <c>00629270</c>. First-seen
    /// <c>game+164</c> is ctor 0 so
    /// <c>009F1750</c> is empty —
    /// do not invent a type-1 queue.
    /// </summary>
    public void SeedWorldTick()
    {
        Note(WorldTickSlot1FnVa, "GamePump", "World",
            "0121BA2D [0x13B92C8]=00629270 type 1");
        Note(AdvanceGameTicksFn, "InitGame", "World",
            "0041726D game+164 ctor 0 009F1750 empty");
    }

    /// <summary>
    /// <c>004AEAA0</c> after
    /// <c>0041674A=1</c>:
    /// <c>inc [+9836]</c>,
    /// <c>009F1720</c> count=0,
    /// <c>009F16F0</c> one
    /// <c>0x648</c> record,
    /// sub[+0]=1,
    /// <c>009F16C0</c> builder+4=0.
    /// </summary>
    private void AppendPlayerCatchupTick()
    {
        PlayerBindSlot0++;
        Note(PlayerBindIncSite, "GamePump", "Player",
            $"004AEB3D inc +{PlayerBindSlot0Offset}={PlayerBindSlot0}");
        _tickTypes.Clear();
        TickListCount = 0;
        Note(TickListClearFn, "GamePump", "Player",
            "009F1720 [game+164]=0");
        TickRecordWatermark = PlayerBindSlot0;
        _tickTypes.Add(WorldTickType);
        TickListCount = 1;
        Note(TickListAppendFn, "GamePump", "Player",
            $"009F16F0 +{PlayerTickBuilderOffset} stride=0x{TickListStride:X} count=1 type={WorldTickType} +0={TickRecordWatermark}");
        Note(TickBuilderResetFn, "GamePump", "Player",
            $"009F16C0 +{PlayerTickBuilderOffset}+4=0");
    }

    /// <summary>
    /// <c>0041726D</c>:
    /// <c>009F1750</c> then
    /// <c>009F1730</c>.
    /// <c>[record+0] &gt; [game+76]</c>
    /// takes <c>0049DFB0</c>.
    /// First-seen +76=+72=0,
    /// record+0=1, flag 1.
    /// First walk skips type 1;
    /// flag walk calls
    /// <c>00629270</c>.
    /// Then +76=record+0,
    /// +72=max(+72, record+0),
    /// <c>004AE9D0</c> +9836=+72.
    /// </summary>
    public void AdvanceGameTicks()
    {
        Note(AdvanceGameTicksFn, "GamePump", "World", "0041726D");
        Note(TickListCountFn, "GamePump", "World",
            $"009F1750 count={TickListCount}");
        if (TickListCount == 0)
        {
            Note(AdvanceGameTicksFn, "GamePump", "World", "009F1750 empty");
            return;
        }

        if (TickRecordWatermark <= GamePlus76)
        {
            Note(AdvanceGameTicksFn, "GamePump", "World",
                $"[+0]={TickRecordWatermark}<=[game+76]={GamePlus76}");
            return;
        }

        var flag = GamePlus76 == GamePlus72;
        Note(WalkTickBeforeDispatchFn, "GamePump", "World", "00416670");
        foreach (var type in _tickTypes)
        {
            Note(ApplyTickTypeFn, "GamePump", "World",
                type == 2
                    ? $"00415FE0 type={type} vtbl+{GameVtbl16}"
                    : $"00415FE0 type={type} skip vtbl+{GameVtbl16}");
        }

        Note(DispatchWorldCallbacksFn, "GamePump", "World",
            $"0049DFB0 flag={(flag ? 1 : 0)} types={_tickTypes.Count}");
        if (flag)
        {
            foreach (var type in _tickTypes)
            {
                if (type != WorldTickType)
                    continue;
                Note(TickSubAtFn, "GamePump", "World",
                    $"009F16D0 [sub+0]={type}");
                Note(WorldTickThunk, "GamePump", "World", "00629270 slot 1");
                TickWorld();
            }
        }

        Note(WalkTickAfterDispatchFn, "GamePump", "World",
            $"00434A60 [0x{WorldTickSlot1Plus48Va:X}]={WorldTickSlot1Plus48FirstSeen} skip");
        GamePlus76 = TickRecordWatermark;
        if (TickRecordWatermark >= GamePlus72)
            GamePlus72 = TickRecordWatermark;
        PlayerBindSlot0 = GamePlus72;
        PlayerBindSlot1 = WorldFrame;
        Note(PlayerBindAfterWorldFn, "GamePump", "Player",
            $"004AE9D0 +{PlayerBindSlot0Offset}={PlayerBindSlot0} +{PlayerBindSlot1Offset}={PlayerBindSlot1}");
        if (DisplayPlus232 > 0)
        {
            DisplayPlus232--;
            Note(DisplayTickTailFn, "GamePump", "Display",
                $"00434F60 +{DisplayPlus232Offset}={DisplayPlus232}");
        }
        else
            Note(DisplayTickTailFn, "GamePump", "Display",
                $"00434F60 +{DisplayPlus232Offset}=0 skip");
    }

    /// <summary>
    /// <c>004A5A40</c>: first-seen
    /// <c>[world+248]==0</c>,
    /// <c>[world+260]==0</c>,
    /// <c>004B4490</c>, then
    /// <c>006E75C0</c> empty
    /// <c>[script+60]</c>, then
    /// <c>006874B0</c> empty
    /// <c>[event+4]</c>, then 4×
    /// <c>004498C0</c>/<c>00488AB0</c>
    /// <c>00A01B50</c> miss, then
    /// <c>00640320</c> <c>[+8]=0</c>
    /// skip, then <c>006BB990</c>
    /// <c>1/[0x1375550]</c>, then
    /// <c>004A5DF3 006B3FF0</c>,
    /// <c>004C5E90</c> ret,
    /// <c>006E60F0</c> empty,
    /// <c>0051F070</c> empty,
    /// <c>004A5E10</c>,
    /// <c>006BDC60</c> miss,
    /// <c>0043A080</c>,
    /// <c>006B2260</c>,
    /// <c>006E37D0</c> empty.
    /// No <c>00501450</c>.
    /// </summary>
    public void TickWorld()
    {
        Note(WorldTickFn, "GamePump", "World", "004A5A40");
        PumpQuests();
        PumpScripts();
        PumpEvents();
        PumpPlayerSlots();
        PumpDisplayListeners();
        TickEnvironment();
        if (!WorldCameraPresent)
        {
            WorldCamera.Construct();
            WorldCameraPresent = true;
        }

        if (!WorldCamera.Seeded)
        {
            Note(WorldTickCameraSeedSite, "GamePump", "Camera",
                "004A5DF3 006B3FF0");
            Note(WorldCameraSeedFn, "GamePump", "Camera", "006B3FF0 +68");
            WorldCamera.SeedHero();
        }

        TickBulletTime();
        TickConversations();
        FlushThingManager();
        WorldFrame++;
        Note(WorldFrameIncSite, "GamePump", "World",
            $"004A5E10 inc WorldFrame={WorldFrame}");
        TickOpinion();
        TickPlayerGui();
        TickAtmos();
        TickSpeechGain();
    }

    /// <summary>
    /// <c>004B4490</c> first-seen:
    /// <c>[esi+56]</c> empty skips
    /// <c>00CB8220</c>. After
    /// Gameflow construct,
    /// <c>[quest+8]</c> is the
    /// factory object:
    /// <c>00CB8220</c> →
    /// <c>00CB7C40</c> Main →
    /// <c>00CB7950</c> <c>+41=0</c>
    /// <c>vtbl+4</c> <c>00A44880</c>
    /// → <c>00CE7670</c> state 0
    /// yield on
    /// <c>Q_NewOakValeIntro</c>.
    /// <c>00CB8170</c> <c>+8=0</c>
    /// empty. Then
    /// <c>00449970</c>/<c>00487DC0</c>
    /// miss (<c>00A01B50</c> 0).
    /// </summary>
    public void PumpQuests()
    {
        Note(QuestManagerPumpFn, "GamePump", "Quest",
            $"004B4490 [0x{QuestManagerVa:X}]");
        Note(QuestFactoryGateVa, "GamePump", "Quest",
            $"01375454={QuestFactoryGateFirstSeen} .data");
        Note(QuestFactoryStartFn, "GamePump", "Quest",
            "004B3CE0 construct already at 004B4260");
        if (_activatedQuests.Count == 0)
            Note(QuestListPumpFn, "GamePump", "Quest",
                "00CB8220 skip empty [esi+56]");
        else
            PumpQuestList();
        Note(PlayerCreatureBindFn, "GamePump", "Player",
            "00449970 [game+28]+28");
        Note(PlayerSlotWalkFn, "GamePump", "Player", "004498C0");
        Note(PlayerCreatureThingFn, "GamePump", "Player",
            $"00487DC0 +{PlayerSlotPlus44Offset} jmp 00A01B50");
        Note(PlayerThingSmartPtrFn, "GamePump", "Player",
            "00A01B50 +48=0 miss");
        Note(QuestPlayerSyncFn, "GamePump", "Quest",
            "004AFCA0 skip");
        QuestVtbl24Calls = 0;
        QuestPumpRan = true;
    }

    /// <summary>
    /// <c>00CB8220</c> first-seen
    /// Gameflow: <c>00CB7C40</c>
    /// ticks Main, <c>00CB8170</c>
    /// <c>[+8]=0</c> empty.
    /// </summary>
    private void PumpQuestList()
    {
        QuestPumpWalked = 0;
        Note(QuestListPumpFn, "GamePump", "Quest",
            "00CB8220 00CB7C40 then 00CB8170");
        Note(QuestListWalkAFn, "GamePump", "Quest",
            $"00CB7C40 count={_gameflowWatchers.Count}");
        foreach (var name in _activatedQuests)
        {
            if (name == "Gameflow")
                continue;
            TickNamedQuestMain(name);
        }

        if (_gameflowWatchers.Contains(WatcherMain) &&
            GameflowYieldQuest is null)
        {
            TickGameflowMain();
            TickCoreReminder();
            TickBarrowGuards();
        }
        else if (GameflowYieldQuest is { })
            ResumeGameflowWait();
        Note(QuestListWalkBFn, "GamePump", "Quest",
            "00CB8170 [+8]=0 empty");
    }

    /// <summary>
    /// <c>00CB7950</c> <c>+40=0</c>
    /// <c>00F35A00</c> <c>+44=0</c>
    /// → 1; <c>+41=0</c> →
    /// <c>00A44880</c> →
    /// <c>00A446A0</c> <c>vtbl+16</c>
    /// <c>00CE7640</c> →
    /// <c>00CE7670</c>.
    /// </summary>
    private void TickGameflowMain()
    {
        Note(QuestFiberAttachFn, "GamePump", "Quest",
            "00CB7950 Main +41=0 vtbl+4");
        Note(FiberTickFn, "GamePump", "Quest", "00A44880");
        Note(FiberResumeFn, "GamePump", "Quest", "00A44660 [0x13D2838]");
        Note(FiberEntryFn, "GamePump", "Quest", "00A446A0 vtbl+16");
        Note(WatcherRunFn, "GamePump", "Quest", "00CE7640 00CDD440");
        Note(GameflowTickFn, "GamePump", "Quest", "00CE7670");
        AttachGameflowWatcher(WatcherCoreReminder, "GamePump");
        AttachGameflowWatcher(WatcherBarrowGuards, "GamePump");
        Note(GameflowState0Fn, "GamePump", "Quest",
            "00CE77D7 SharedRun+4=0");
        Note(PlayAviFlagFn, "GamePump", "Quest",
            "0088E090 0040D2A0 00408340 +49=1");
        Note(GiveNamedObjectFn, "GamePump", "Quest",
            "008902E0 tattoo 00487DC0 miss");
        Note(StoryLogFn, "GamePump", "Quest",
            $"00CBE87F TEXT_QST_LOG_STORY_{StoryLogFirstSeen}");
        Note(QuestCardBindFn, "GamePump", "Quest",
            "008968C0 vtbl+1180 " + GameflowWaitCard + " ret 12");
        Note(QuestIsActiveFn, "GamePump", "Quest",
            "00893570 vtbl+100 " + GameflowWaitQuest + " 0");
        Note(GameflowYieldThunk, "GamePump", "Quest",
            "006E7410 vtbl+8 00A44840 009D8650");
        Note(WatcherYieldVtbl8, "GamePump", "Quest", "00A44840 yield");
        Note(FiberYieldFn, "GamePump", "Quest",
            "009D8650 wait " + GameflowWaitQuest);
        GameflowState = 0;
        GameflowYieldQuest = GameflowWaitQuest;
        QuestPumpWalked++;
    }

    /// <summary>
    /// Later type-1 <c>00CB7950</c>:
    /// <c>+40=0</c> <c>+44=0</c>
    /// <c>00F35A00=1</c> <c>+41=0</c>
    /// → <c>vtbl+4</c> <c>00A44880</c>
    /// → <c>00A44660</c> resume.
    /// <c>00CE7670</c> is still in the
    /// <c>00893570</c> wait;
    /// quest miss → <c>00CB7940</c>
    /// / <c>006E7410</c> yield.
    /// Does not re-attach Core/Barrow
    /// or re-run tattoo/card.
    /// Host skip-<c>00A44880</c> when
    /// parked is DISPROVEN.
    /// </summary>
    private void ResumeGameflowWait()
    {
        Note(QuestFiberAttachFn, "GamePump", "Quest",
            "00CB7950 Main +41=0 vtbl+4 resume");
        Note(FiberTickFn, "GamePump", "Quest", "00A44880");
        Note(FiberResumeFn, "GamePump", "Quest",
            "00A44660 009D87F0 resume");
        Note(QuestIsActiveFn, "GamePump", "Quest",
            "00893570 vtbl+100 " + GameflowWaitQuest + " 0");
        Note(GameflowYieldThunk, "GamePump", "Quest",
            "006E7410 vtbl+8 00A44840 009D8650");
        Note(FiberYieldFn, "GamePump", "Quest",
            "009D8650 wait " + GameflowWaitQuest);
        QuestPumpWalked++;
    }

    /// <summary>
    /// Same <c>00CB7C40</c> walk after
    /// Main yield: insert-at-tail so
    /// Core is next. <c>00CEF3B0</c>
    /// <c>[+72]=0</c> → <c>vtbl+28</c>
    /// yield. Not the guild message.
    /// </summary>
    private void TickCoreReminder()
    {
        Note(QuestFiberAttachFn, "GamePump", "Quest",
            "00CB7950 CoreQuestReminder +41=0");
        Note(CoreReminderFn, "GamePump", "Quest",
            "00CEF3B0 [+72]=0");
        Note(GameflowYieldThunk, "GamePump", "Quest",
            "006E7410 wait Gameflow+72");
        Note(FiberYieldFn, "GamePump", "Quest",
            "009D8650 wait CoreQuestReminder");
        QuestPumpWalked++;
    }

    /// <summary>
    /// Same walk: <c>00CEF550</c>
    /// <c>vtbl+1144</c> <c>00892F60</c>
    /// <c>004B0FC0</c> miss both
    /// trader quests → <c>vtbl+1136</c>
    /// <c>004AF610</c> miss → yield.
    /// </summary>
    private void TickBarrowGuards()
    {
        Note(QuestFiberAttachFn, "GamePump", "Quest",
            "00CB7950 CheckBarrowFieldsGuards +41=0");
        Note(BarrowGuardsFn, "GamePump", "Quest", "00CEF550");
        Note(QuestThingHasFn, "GamePump", "Quest",
            "00892F60 004B0FC0 " + TraderConflictEvil + " 0");
        Note(QuestThingHasFn, "GamePump", "Quest",
            "00892F60 004B0FC0 " + TraderConflictGood + " 0");
        Note(QuestNameActiveFn, "GamePump", "Quest",
            "00892F40 004AF610 trader miss");
        Note(GameflowYieldThunk, "GamePump", "Quest",
            "006E7410 wait trader");
        Note(FiberYieldFn, "GamePump", "Quest",
            "009D8650 wait CheckBarrowFieldsGuards");
        QuestPumpWalked++;
    }

    /// <summary>
    /// <c>004B4490</c> walks
    /// <c>[esi+56]</c> tail-insert
    /// order: WLD list first.
    /// Sunnyvale <c>00CDD360</c>
    /// <c>vtbl+28</c> yield.
    /// HeroBoasts <c>00CE1AF0</c>
    /// empty then yield.
    /// Personal <c>00CDDCB0</c>
    /// <c>vtbl+72</c> empty.
    /// </summary>
    private void TickNamedQuestMain(string name)
    {
        Note(QuestListPumpFn, "GamePump", "Quest",
            "00CB8220 " + name);
        if (name == "Q_SunnyvaleMaster")
            Note(SunnyvaleMainTick, "GamePump", "Quest",
                "00CDD360 vtbl+28 006E7410 yield");
        else if (name == "HeroBoasts")
            Note(HeroBoastsTick, "GamePump", "Quest",
                "00CE1AF0 empty 00CE1C24 yield");
        else if (name.StartsWith("PersonalScript", StringComparison.Ordinal))
            Note(PersonalMainTick, "GamePump", "Quest",
                "00CDDCB0 vtbl+72 0089AC10 empty");
        else
            Note(QuestFiberAttachFn, "GamePump", "Quest",
                "00CB7950 " + name + " Main");
        Note(FiberYieldFn, "GamePump", "Quest",
            "009D8650 " + name);
        QuestPumpWalked++;
    }

    /// <summary>
    /// <c>006E75C0</c> first-seen:
    /// flag=1, <c>vtbl+1580</c> 0,
    /// <c>vtbl+1544</c> 0,
    /// <c>0%15==0</c>,
    /// <c>[this+60]</c> empty.
    /// </summary>
    public void PumpScripts()
    {
        Note(ScriptManagerPumpFn, "GamePump", "Script",
            $"006E75C0 [world+{WorldScriptManagerOffset}] flag=1");
        Note(InitScriptsParentFn, "GamePump", "Script",
            "004A6550 Init Scripts 006E7740");
        Note(ScriptPausedGateFn, "GamePump", "Script",
            $"vtbl+1580 [+{ScriptManagerPlus44Offset}]={ScriptManagerPlus44FirstSeen}");
        Note(ScriptGuiGateFn, "GamePump", "Script",
            $"vtbl+1544 [0x{PlayerGuiInstanceVa:X}]+{GuiPlus246Offset}={GuiPlus246FirstSeen}");
        Note(ScriptListIterFn, "GamePump", "Script",
            "0059299D skip +60 empty");
        ScriptPumpWalked = 0;
        ScriptPumpRan = true;
    }

    /// <summary>
    /// <c>006874B0</c> first-seen:
    /// <c>004B3CE0</c> posted
    /// <c>00687540(55,50)</c> per
    /// construct. <c>[node+64]=50</c>.
    /// <c>0049D870</c> is
    /// <c>[0x13B89BC]</c> WorldFrame
    /// (inc is <c>004A5E10</c>
    /// after this call) so
    /// construct+50 &gt;= now; skip
    /// <c>006872B0</c>. Empty-list
    /// is DISPROVEN when quests
    /// constructed.
    /// </summary>
    public void PumpEvents()
    {
        Note(EventManagerPumpFn, "GamePump", "Event",
            $"006874B0 [world+{WorldEventManagerOffset}]");
        if (EventPosts == 0)
        {
            Note(EventManagerCtor, "GamePump", "Event",
                "00687510 [+4] empty circular");
            Note(EventManagerPostFn, "GamePump", "Event",
                "00687540 skip empty");
            Note(EventNodeFreeFn, "GamePump", "Event",
                "00BFEA14 skip");
            EventPumpWalked = 0;
        }
        else
        {
            Note(EventManagerPostFn, "GamePump", "Event",
                $"00687540 count={EventPosts} kind={EventPostKind} delay={EventPostDelay}");
            Note(EventTickReadFn, "GamePump", "Event",
                $"0049D870 [0x{WorldFrameVa:X}]={WorldFrame}");
            Note(EventNodeFireFn, "GamePump", "Event",
                $"006872B0 skip {WorldFrame}+{EventPostDelay}>{WorldFrame}");
            EventPumpWalked = EventPosts;
        }

        EventPumpRan = true;
    }

    /// <summary>
    /// <c>004A5DA1</c> slots 0..3:
    /// <c>0099A330</c> 1,
    /// <c>00488AB0</c> skip
    /// <c>004887C0</c>,
    /// <c>00A01B50</c> miss.
    /// </summary>
    public void PumpPlayerSlots()
    {
        Note(PlayerSlotWalkFn, "GamePump", "Player",
            $"004498C0 ×{PlayerSlotLoopCount} [world+{WorldPlayerManagerOffset}]");
        for (var i = 0; i < PlayerSlotLoopCount; i++)
        {
            Note(PlayerSlotValidFn, "GamePump", "Player",
                $"0099A330 slot {i} [+{PlayerSlotPlus4Offset}]={PlayerSlotPlus4FirstSeen}");
            Note(PlayerSlotTickFn, "GamePump", "Player",
                $"00488AB0 [+{PlayerSlotPlus534Offset}]={PlayerSlotPlus534FirstSeen} skip 004887C0");
            Note(PlayerThingSmartPtrFn, "GamePump", "Player",
                "00A01B50 +44 miss skip 006A4D00");
        }

        PlayerSlotTicks = PlayerSlotLoopCount;
    }

    /// <summary>
    /// <c>00640320</c> first-seen:
    /// OnActivate inserted
    /// <c>[engine+44]</c>,
    /// <c>vtbl+204</c> <c>[+8]=0</c>
    /// skips <c>00B24030</c>.
    /// </summary>
    public void PumpDisplayListeners()
    {
        Note(DisplayListenerGetFn, "GamePump", "Display",
            $"00436FB0 [0x{DisplayListenerVa:X}]");
        Note(DisplayListenerInsertFn, "GamePump", "Display",
            "006404D0 OnActivate [engine+44]");
        Note(DisplayListenerPumpFn, "GamePump", "Display",
            "00640320 flag=1");
        Note(DisplayActiveGateFn, "GamePump", "Display",
            $"vtbl+204 [+{DisplayPlus8Offset}]={DisplayPlus8FirstSeen} skip 00B24030");
        DisplayListenerPumped = true;
        DisplayActiveApplyRan = false;
    }

    /// <summary>
    /// <c>006BB990</c> first-seen:
    /// ctor <c>+33=0</c> <c>+24=0</c>
    /// so <c>dt*(1/dayLen)</c> adds
    /// into <c>+8</c> and <c>+28</c>.
    /// dt is <c>1/[0x1375550]</c>
    /// (<c>15</c>).
    /// </summary>
    public void TickEnvironment()
    {
        Note(EnvironmentCtor, "GamePump", "World",
            $"006BBC30 +{EnvironmentPlus33Offset}={EnvironmentPlus33FirstSeen} +{EnvironmentPlus24Offset}={EnvironmentPlus24FirstSeen}");
        Note(EnvironmentTickFn, "GamePump", "World",
            $"006BB990 1/[0x{EnvironmentDayDivisorVa:X}]={EnvironmentDayDivisor}");
        EnvironmentTime += 1f / EnvironmentDayDivisor;
        EnvironmentTicked = true;
    }

    /// <summary>
    /// <c>004C5E90</c> is <c>ret</c>.
    /// </summary>
    public void TickBulletTime()
    {
        Note(BulletTimeTickFn, "GamePump", "World", "004C5E90 ret");
        BulletTimeTicked = true;
    }

    /// <summary>
    /// <c>006E60F0</c> first-seen:
    /// <c>006E6150</c> <c>[node+8]=self</c>
    /// so the walk is empty.
    /// </summary>
    public void TickConversations()
    {
        Note(ConversationCtor, "GamePump", "World",
            "006E6150 [+8]=self");
        Note(ConversationTickFn, "GamePump", "World",
            "006E60F0 empty");
        ConversationWalked = 0;
        ConversationTicked = true;
    }

    /// <summary>
    /// <c>0051F070</c> first-seen:
    /// <c>00523540</c> <c>+72=+76=0</c>.
    /// </summary>
    public void FlushThingManager()
    {
        Note(ThingManagerCtor, "GamePump", "World",
            "00523540 +72=+76=0");
        Note(ThingManagerFlushFn, "GamePump", "World",
            "0051F070 empty");
        ThingManagerFlushedCount = 0;
        ThingManagerFlushed = true;
    }

    /// <summary>
    /// <c>006BDC60</c> first-seen:
    /// <c>+48=0</c> skip
    /// <c>006BD900</c>;
    /// <c>00487DC0</c> 0 → ret.
    /// </summary>
    public void TickOpinion()
    {
        Note(OpinionTickFn, "GamePump", "World",
            "006BDC60 00487DC0 miss");
        Note(PlayerCreatureThingFn, "GamePump", "World",
            "00487DC0 skip SOUND_THEME");
        OpinionTicked = true;
    }

    /// <summary>
    /// <c>0043A080</c>
    /// <c>[world+164]=0</c>.
    /// </summary>
    public void TickPlayerGui()
    {
        Note(PlayerGuiTickFn, "GamePump", "UI",
            "0043A080 +164=0");
        PlayerGuiTicked = true;
    }

    /// <summary>
    /// <c>006B2260</c>
    /// <c>[0x13B8394]!=0</c>.
    /// Dummy has no
    /// <c>MARKER_POSITIONAL_ATMOS</c>
    /// instance.
    /// </summary>
    public void TickAtmos()
    {
        Note(AtmosTickFn, "GamePump", "World",
            $"006B2260 [0x{AtmosGateVa:X}] MARKER_POSITIONAL_ATMOS");
        AtmosTicked = true;
    }

    /// <summary>
    /// <c>006E37D0</c> first-seen:
    /// <c>[0x13BABA0]</c> circular
    /// empty.
    /// </summary>
    public void TickSpeechGain()
    {
        Note(SpeechGainTickFn, "GamePump", "World",
            $"006E37D0 [0x{SpeechGainListVa:X}] empty");
        SpeechGainTicked = true;
    }

    /// <summary>
    /// Same Thing scripts, movement, and
    /// PALSKIN use. Not <c>CREATURE_HERO_CHILD</c>.
    /// </summary>
    private void BindRuntimeHero()
    {
        if (Runtime is null || Hero is null)
            return;
        Runtime.BindScene(_regionThings, Camera);
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
    /// <c>00500540(i,0,0)</c> for i=1..count-1.
    /// After each: <c>0048D400</c> bit
    /// <c>0x64</c> then <c>005198B0</c>
    /// <c>CTCActionUseScriptedHook</c>.
    /// Then <c>RegionGraph.txt</c> and
    /// <c>00500540(saved,0,1)</c> (no pump).
    /// </summary>
    public void LoadFromFirstRealRegion()
    {
        FirstRealRegionLoadDone = true;
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
        for (var i = 1; i < count; i++)
        {
            Note(LoadRegionFn, "LevelLoader", "Region",
                i == 1
                    ? "00500540(1,0,0) first +36 null continues"
                    : $"00500540({i},0,0)");
            RequestLoadRegion(i, sync: true);
            Note(CollectThingsListFn, "LevelLoader", "Thing",
                "0049C770 [map+8]+32 +24");
            Note(CollectRegionThingsFn, "LevelLoader", "Thing",
                $"0048D400 after {i} +145 need 0x0C forbid 0x21 bit 0x64");
            Note(CollectThingsBitTestFn, "LevelLoader", "Thing",
                "006A80A0 bit 0x64 thing+32");
            Note(MapToRegionFn, "LevelLoader", "Region",
                $"004FC190 i={i}");
            Note(CollectScriptedHookThingsFn, "LevelLoader", "Thing",
                "005198B0 +145 then 00518DC0 CTCActionUseScriptedHook");
            Note(ScriptedHookCollectFn, "LevelLoader", "Thing",
                "00518DC0 +56 bit4 key=0xC2");
        }

        Note(RegionGraphNameVa, "LevelLoader", "Region", RegionGraphName);
        Note(LoadFromFirstRealRegionFn, "LevelLoader", "Region",
            $"00500540({saved},0,1) restore no-pump");
        if (saved > 0)
            RequestLoadRegion(saved, sync: false);
    }

    /// <summary>
    /// Recovered <c>00501450</c> body.
    /// Not a first-seen <c>004189C2</c>
    /// callee: after <c>009AC9E0</c>
    /// native loops while
    /// <c>[game+8]==0</c>. E8 caller
    /// still UNREAD. Not
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
    /// <c>005064C0</c>. <c>00B428E0</c>
    /// is <c>004A1840</c> vtbl+208.
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
    /// <c>0049DDD0</c> first-seen
    /// (<c>[0x13B8616]==0</c>):
    /// <c>Data\Levels\</c> + WLD stem +
    /// <c>.stb</c>. <c>_RT.stb</c> only
    /// when the retail flag is set.
    /// </summary>
    public static string DeriveStaticMapFileName(string wldPath, bool retailRt = false)
    {
        var stem = Path.GetFileNameWithoutExtension(wldPath);
        if (string.IsNullOrEmpty(stem))
            stem = Path.GetFileNameWithoutExtension(FinalAlbionWld);
        var suffix = retailRt ? StaticMapRtStbSuffix : StaticMapStbSuffix;
        return StaticMapLevelsDir + stem + suffix;
    }

    /// <summary>
    /// First-seen <c>00B428E0</c> is
    /// <c>004A1840</c> <c>004A1BD3</c>
    /// display <c>vtbl+208</c>
    /// <c>00B23DC0</c>, not
    /// <c>004FC8A0</c> / <c>00500540</c>
    /// after <c>004AFC00</c>. TLC has
    /// <c>FinalAlbion_RT.stb</c> only, so
    /// first-seen <c>.stb</c> misses
    /// <c>[+52].vtbl+12</c> and
    /// <c>00B42750</c> does not write
    /// <c>+424</c> or walk
    /// <c>00B420F0</c>.
    /// </summary>
    public void SetStaticMapFileForUse()
    {
        var retailRt = RetailStbFlagFirstSeen != 0;
        StaticMapFileName = DeriveStaticMapFileName(
            WorldFileName ?? FinalAlbionWld, retailRt);
        Note(DisplayEngineSetStaticMapThunk, "StaticMap", "WLD",
            $"00B23DC0 vtbl+{DisplayEngineSetStaticMapVtbl} [0x{MapManagerGlobalVa:X}]");
        Note(DeriveStaticMapNameFn, "StaticMap", "WLD",
            $"0049DDD0 {StaticMapFileName}");
        Note(SetStaticMapVtblCallSite, "StaticMap", "WLD",
            "004A1BD3 [display+208]");
        Note(SetStaticMapFileForUseFn, "StaticMap", "WLD",
            "SetStaticMapFileForUse: CloseStaticMapFile");
        CloseStaticMapFile();
        Note(SetStaticMapFileForUseFn, "StaticMap", "WLD",
            "SetStaticMapFileForUse: EnablePoolAllocation");
        Note(SetStaticMapFileForUseFn, "StaticMap", "WLD",
            "SetStaticMapFileForUse: OpenStaticMaps");
        Note(OpenStaticMapsFn, "StaticMap", "WLD",
            $"00B42750 mode={OpenStaticMapsUseMode} [+424]");

        var path = Install is null
            ? null
            : Path.Combine(Install.Root, StaticMapFileName);
        if (path is null || !File.Exists(path))
        {
            Note(OpenStaticMapsFn, "StaticMap", "WLD",
                $"00B42750 [+52].vtbl+12 miss {StaticMapFileName}");
        }
        else
        {
            Note(OpenStaticMapsNameTable, "StaticMap", "WLD",
                "00B420F0 name table UNREAD");
            OpenStaticMapsMode = OpenStaticMapsUseMode;
        }

        Note(SetStaticMapFileForUseFn, "StaticMap", "WLD",
            "SetStaticMapFileForUse: LoadWaterData");
        Note(LoadWaterDataFn, "StaticMap", "WLD", "00B41FA0");
    }

    /// <summary>
    /// Host fill of AABB-touch maps. Not
    /// first-seen <c>00B428E0</c> (that
    /// site misses). Kept for later
    /// recovered hit-path callers.
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
            Note(LoadRegionFn, "LevelLoader", "Region",
                "00500540 after 004AFC00 dtor ret 12");
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
    /// <c>004A0D90</c> AddQuest into
    /// world+184 (TRUE also +172).
    /// AddTestQuest is world+196
    /// only — not this catalog.
    /// Not <c>0049F180</c>.
    /// </summary>
    /// <summary>
    /// <c>0049D770</c> first-seen:
    /// <c>Data\Levels\</c> + WLD stem +
    /// <c>.qst</c>.
    /// </summary>
    public static string DeriveQuestFileName(string wldPath)
    {
        var stem = Path.GetFileNameWithoutExtension(wldPath);
        if (string.IsNullOrEmpty(stem))
            stem = Path.GetFileNameWithoutExtension(FinalAlbionWld);
        return StaticMapLevelsDir + stem + QuestSuffix;
    }

    private void LoadQuestDefs()
    {
        Note(LoadQuestsSite, "Load Quests", "Quest", "00416ABA 004A1840");
        var qst = DeriveQuestFileName(WorldFileName ?? FinalAlbionWld);
        Note(DeriveQuestPathFn, "Load Quests", "Quest", "0049D770 " + qst);
        Note(QstParseFn, "Load Quests", "Quest", "004A0D90 AddQuest/AddTestQuest");
        Note(OakvaleBindSite, "Load Quests", "Quest",
            "00CD6E27 00CB5C90 S_QNOVI 00DBEF70 bind not 00CB5AD0");
        Note(AddTestQuestStoreFn, "Load Quests", "Quest",
            $"004A113B AddTestQuest [world+{WorldAddTestQuestOffset}] store not 004B4A10");
        Note(InventoryQuestsConfirmFn, "Load Quests", "Quest",
            "0061AB30 CTCInventoryQuests leftover not New Game");
        Note(InventoryQuestsGiveFn, "Load Quests", "Quest",
            "0061ACB3 CTCInventoryQuests Give leftover not first-seen");
        Note(StartNewQuestParseFn, "Load Quests", "Quest",
            "004B5080 START_NEW_QUEST save parse 0 E8 no-save");
        Note(QuestActivatePlus172SiblingFn, "Load Quests", "Quest",
            "0049EAC0 [this+172] 0 inbound skip not 00CB5AD0");
        _worldPlus172.Clear();
        _worldPlus184.Clear();
        Quests = null;
        if (Install is not null && File.Exists(Install.QuestPath))
        {
            Note(QstClearFn, "Load Quests", "Quest",
                $"004A08D0 flag {QstParseFlagClear} clear +184/+172/+{WorldAddTestQuestOffset}");
            Quests = QuestFile.Load(Install.QuestPath);
            StoreAddQuestNames(Quests);
            Note(QstParseFn, "Load Quests", "Quest",
                $"quests={Quests.Quests.Count} {Path.GetFileName(Install.QuestPath)}");
        }

        Note(GlobalQuestsVa, "Load Quests", "Quest", GlobalQuestsName);
        if (Install is not null && File.Exists(Install.GlobalQuestPath))
        {
            var global = QuestFile.Load(Install.GlobalQuestPath);
            StoreAddQuestNames(global);
            Quests = Quests is null ? global : Quests.Append(global);
            Note(QstParseFn, "Load Quests", "Quest",
                "004A0D90 " + Path.GetFileName(Install.GlobalQuestPath));
        }
        else
            Note(FileExistsFn, "Load Quests", "Quest",
                "00999230 miss " + GlobalQuestsName);

        Note(QstParseFn, "Load Quests", "Quest",
            $"004A0D90 [world+{WorldQuestListOffset}] TRUE count={_worldPlus172.Count}");
        Note(QuestManagerPushFn, "Load Quests", "Quest",
            $"004B2850 [QM+{QuestManagerPlus44Offset}] count={_questManagerPlus44.Count}");
    }

    /// <summary>
    /// <c>004A0D90</c> <c>AddQuest</c>:
    /// name → world+184; TRUE →
    /// world+172. Not activate.
    /// </summary>
    private void StoreAddQuestNames(QuestFile file)
    {
        foreach (var quest in file.Quests)
        {
            _worldPlus184.Add(quest.Name);
            if (quest.Persistent)
                _worldPlus172.Add(quest.Name);
            _questManagerPlus44.Add(quest.Name);
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
            "0043A380 reset PLAYER_GUI_PC [0x13B8790] not ctor");
        PlayerGuiReady = true;

        var names = _worldPlus172;
        Note(InitQuestsFn, "Init Quests", "Quest",
            $"004B4260 [world+{WorldQuestListOffset}] QST TRUE count={names.Count} not WLD START_INITIAL_QUESTS");
        Runtime = ScriptRuntime.Detached();
        Runtime.BindScene(_regionThings, Camera);
        if (Install?.FindCompiledDef("script.bin") is not null)
            Runtime.Load(ScriptBank.Load(Install), Install);

        foreach (var name in names)
            ActivateNamedQuest(name, "Init Quests");

        Note(QuestManagerActivate, "Init Quests", "Quest", "004B2890");
        Note(ActivateInitialQuestsSite, "Activate Initial Quests", "Quest",
            "00416BCF +90584 empty 0122D70E skip 004B4A10");
        Note(ActivateInitialQuestsFn, "Activate Initial Quests", "Quest",
            "004B4A10 not Q_NewOakValeIntro");
        Note(QuestCardFindFn, "Init Quests", "Quest",
            "004B0D30/00896A30 need 004AF610 already active");
        QuestsInitDone = true;
    }

    /// <summary>
    /// <c>004B4260</c> after
    /// <c>004B00C0</c>:
    /// <c>00CB5AD0</c> on
    /// <c>[quest+120]</c>.
    /// </summary>
    private void ActivateNamedQuest(string name, string phase)
    {
        if (name.Length == 0)
            return;
        var inTable = name.Equals("NULL", StringComparison.OrdinalIgnoreCase) ||
            _questManagerPlus44.Exists(n =>
                n.Equals(name, StringComparison.OrdinalIgnoreCase));
        Note(QuestActivateGateFn, phase, "Quest",
            inTable
                ? $"004B00C0 [QM+{QuestManagerPlus44Offset}] {name}"
                : $"004B00C0 miss skip 00CB5AD0 {name}");
        if (!inTable)
            return;
        Runtime ??= ScriptRuntime.Detached();
        if (Runtime.Camera is null)
            Runtime.BindScene(_regionThings, Camera);
        if (Install?.FindCompiledDef("script.bin") is not null &&
            Runtime.Bank is null)
            Runtime.Load(ScriptBank.Load(Install), Install);
        Note(ActivateQuestFn, phase, "Quest", "00CB5AD0 " + name);
        Note(QuestFactoryCollectFn, phase, "Quest", "004BB720");
        Note(QuestFactoryGateVa, phase, "Quest",
            $"01375454={QuestFactoryGateFirstSeen} .data");
        var factory = QuestFactoryTable.Find(name);
        if (factory is { } bind)
        {
            Note(QuestRegisterFn, phase, "Quest",
                "00CD52D0 " + name +
                (bind.ScriptName is { } script ? " → " + script : " native"));
            Note(QuestFactoryStartFn, phase, "Quest",
                "004B3CE0 construct");
            Note(bind.Factory, phase, "Quest",
                $"factory 0x{bind.Factory:X} run 0x{bind.Run:X}");
            if (bind.Init == QuestFactoryTable.SunnyvaleInit)
            {
                Note(QuestFactoryTable.SharedRun, phase, "Quest",
                    "00CDBD20 alloc 0x144 vtbl 012C2748");
                Note(QuestFactoryTable.SunnyvaleInit, phase, "Quest",
                    "00CDBA10 zeros + _LIKE/_HATE");
                Note(SunnyvalePersistFn, phase, "Quest",
                    "00CDC070 persist bind vtbl+4");
            }
            else
                Note(QuestFactoryTable.SharedRunReuse, phase, "Quest",
                    "004AFA10 reuse 00CDBD20");
            Note(QuestFactoryTable.GameflowConstructHook, phase, "Quest",
                "00CB7900 vtbl+12 then vtbl+4");
            if (bind.Init == QuestFactoryTable.GameflowMain)
                SeedGameflowStates(phase);
        }

        var persistent = Quests?.Quests.Any(q =>
            q.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && q.Persistent) == true;
        Runtime.ActivateQuest(name, persistent);
        _activatedQuests.Add(name);
        EventPosts++;
        Note(EventManagerPostFn, phase, "Event",
            $"00687540 kind={EventPostKind} delay={EventPostDelay}");
    }

    /// <summary>
    /// <c>00CB7900</c> <c>jmp [vtbl+4]</c>
    /// after <c>vtbl+12</c> <c>00CE6CF0</c>.
    /// <c>00CE75B0</c> attaches
    /// <c>Main</c> via <c>00CDD450</c>
    /// / <c>00CB7E50</c>. Not
    /// <c>S_GF</c> <c>CCutsceneDef</c>.
    /// </summary>
    private void SeedGameflowStates(string phase)
    {
        Note(QuestFactoryTable.GameflowSeed, phase, "Quest",
            "00CE6CF0 [+68]+4=0 [+72]=0");
        Note(QuestFactoryTable.ScriptStateLookup, phase, "Quest",
            "008A9DB0 → 008AE660 [0x13BAE44]");
        foreach (var slot in QuestFactoryTable.GameflowStateNames)
        {
            if (_gameflowStates.Contains(slot))
                continue;
            _gameflowStates.Add(slot);
            Note(QuestFactoryTable.ScriptStateInsert, phase, "Quest",
                "008AE660 " + slot);
        }

        Note(QuestFactoryTable.GameflowMain, phase, "Quest",
            "00CE75B0 Main 00CDD450 / 00CB7E50");
        Note(QuestFactoryTable.GameflowWatcherCtor, phase, "Quest",
            "00CDD450 Main 0.1f");
        Note(QuestFactoryTable.GameflowWatcherAttach, phase, "Quest",
            "00CB7E50 attach");
        AttachGameflowWatcher(WatcherMain, phase);
    }

    private void AttachGameflowWatcher(string name, string phase)
    {
        if (_gameflowWatchers.Contains(name))
            return;
        _gameflowWatchers.Add(name);
        Note(QuestFactoryTable.GameflowWatcherAttach, phase, "Quest",
            "00CB7E50 " + name);
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
        if (AuthoredEnvironmentThemeId != 0)
            return;
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

    /// <summary>
    /// <c>009AD410</c> / <c>009E5170</c> on
    /// the frontend GameBin. First-seen
    /// <c>0041DB1D</c> uses
    /// <c>0044C6B0</c> when
    /// <c>[input+100]==0</c>.
    /// </summary>
    private void ResolveFrontendDef(string name)
    {
        FrontendDefFound = false;
        FrontendDefTypeName = null;
        if (Install is not null && FrontendDefs is null)
        {
            var namesPath = Install.FindCompiledDef("names.bin");
            var fePath = Install.FindCompiledDef(FrontendBinFile);
            if (namesPath is not null && fePath is not null)
                FrontendDefs = GameBin.Load(fePath, NamesBin.Load(namesPath));
        }

        var hit = FrontendDefs?.FindEntry(name) ?? EnsureDefs()?.FindEntry(name);
        Note(MeshBank.DefLookupFn, "Frontend", "UI",
            hit is null
                ? "009AD410 miss " + name
                : "009AD410 " + name + " " + (hit.TypeName ?? "?"));
        if (hit is null)
            return;
        FrontendDefFound = true;
        FrontendDefTypeName = hit.TypeName;
    }

    private void AttachPressStartWidgets()
    {
        AttachFrontendTree(FrontendPressStartMenu, FrontendPressStartSlot);
        FrontendCurrentSlot = FrontendPressStartSlot;
        WriteType10AttachMessage();
    }

    /// <summary>
    /// <c>00596763</c>: old
    /// <c>[ui+32].back()</c>
    /// <c>vtbl+192</c>(6) then
    /// push the incoming slot.
    /// <c>0052CF40</c> writes
    /// <c>+332</c>. Not a
    /// <c>+302</c> hide.
    /// </summary>
    private void SwitchFrontendSlot(int slot)
    {
        if (_frontendSlotTrees.ContainsKey(FrontendCurrentSlot) &&
            FrontendCurrentSlot != slot)
            SelectFrontendState(FrontendCurrentSlot, 6);
        FrontendCurrentSlot = slot;
        RebuildResidentTrees();
    }

    /// <summary>
    /// <c>0052CF40</c> <c>vtbl+192</c>:
    /// <c>+332 = arg</c>.
    /// <c>0041C5A0</c> <c>vtbl+188</c>
    /// stores <c>+320</c> then child
    /// <c>vtbl+192</c>. Not
    /// <c>ActiveChild</c> and not
    /// <c>+302</c>.
    /// </summary>
    private void SelectFrontendState(int slot, int state)
    {
        Note(FrontendWidgetType.SelectStateFn, "Frontend", "UI",
            $"0052CF40 vtbl+192 +332={state} slot 0x{slot:X}");
        if (!_frontendSlotTrees.TryGetValue(slot, out var tree) ||
            tree.Count == 0)
            return;
        ForwardSelectState(tree, 0, state);
        if (slot == FrontendCurrentSlot)
            _frontendWidgets = tree;
    }

    /// <summary>
    /// <c>0041C5A0</c> stores the forwarded duration at <c>+320</c>, then calls
    /// this widget's virtual <c>vtbl+192</c>.  The ordinary implementation is
    /// <c>0052CF40</c> and selects this widget only; it does not recursively
    /// force the same state onto every descendant.  Type 16 overrides that
    /// virtual at <c>00548F40</c> and handles its choice children explicitly.
    /// </summary>
    private void ForwardSelectState(List<FrontendWidget> tree, int index, int state) =>
        ForwardSelectState(tree, index, state, -1f);

    private void ForwardSelectState(
        List<FrontendWidget> tree, int index, int state, float inheritedDuration)
    {
        if ((uint)index >= (uint)tree.Count)
            return;
        var widget = tree[index];
        Note(FrontendWidgetType.ForwardSelectFn, "Frontend", "UI",
            $"0041C5A0 vtbl+188 +{FrontendWidgetType.DurationOffset} " +
            $"widget {widget.Name} +332={state}");
        var haveStyle = widget.StyleColours is { } colours &&
            (uint)state < (uint)colours.Count;
        var ownDuration = widget.StyleDurations is { } durations &&
            (uint)state < (uint)durations.Count
            ? durations[state]
            : -1f;
        var duration = inheritedDuration >= 0f ? inheritedDuration : ownDuration;
        var targetColour = haveStyle
            ? FrontendWidgetFactory.ColourAtStyle(widget, state)
            : widget.Colour;
        var transition = haveStyle && duration > 0f && targetColour != widget.Colour;
        tree[index] = widget with
        {
            State = state,
            StyleIndex = haveStyle ? state : widget.StyleIndex,
            Colour = transition ? widget.Colour : targetColour,
            ColourTransitionFrom = transition ? widget.Colour : targetColour,
            ColourTransitionTo = targetColour,
            ColourTransitionElapsed = 0f,
            ColourTransitionDuration = transition ? duration : 0f,
            ColourTransitionActive = transition,
        };

        // 00548FA2..00549075: CTextSlider state 5 first selects every
        // authored choice to state 1, then selects +348 (ActiveChild) to
        // state 3.  This is why WASD/Normal are visible after the menu's
        // entrance state while their alternatives remain inactive.
        if (widget.Type == FrontendWidgetType.Swap)
        {
            // CSwappingStateComponent publishes its selected authored state
            // to the blending children when its completion edge advances.
            for (var child = 0; child < tree.Count; child++)
                if (tree[child].ParentIndex == index)
                    ForwardSelectState(tree, child, state, duration);
            return;
        }

        if (widget.Type != FrontendWidgetType.TextSlider || state != 5)
            return;
        var childCount = FrontendWidgetFactory.ChildCount(tree, index);
        for (var child = 0; child < tree.Count; child++)
        {
            if (tree[child].ParentIndex != index)
                continue;
            Note(FrontendWidgetType.ForwardSelectFn, "Frontend", "UI",
                $"00548F40 child {tree[child].Name} +332=1");
            ForwardSelectState(tree, child, 1, duration);
        }
        if (childCount == 0)
            return;
        var selected = Math.Clamp(widget.ActiveChild, 0, childCount - 1);
        ForwardSelectState(tree,
            FrontendWidgetFactory.ChildAt(tree, index, selected), 3, duration);
    }

    /// <summary>
    /// <c>0059B5D7</c> on
    /// <c>[ui+84]</c>. Factory
    /// stores the constructed
    /// widget* at node+20. Later
    /// attaches do not drop other
    /// keys.
    /// </summary>
    private void BindFrontendSlot(int slot)
    {
        Note(FrontendSlotLookupFn, "Frontend", "UI",
            $"0059B5D7 [ui+{FrontendWidgetListOffset}] slot 0x{slot:X} node+{FrontendWidgetSlotOffset}");
        if (_frontendWidgets.Count == 0)
            return;
        _frontendSlotTrees[slot] = _frontendWidgets;
        _frontendResidentSlotKeys = _frontendSlotTrees.Keys
            .OrderBy(key => key)
            .ToArray();
        FrontendResidentSlots = _frontendResidentSlotKeys;
        RebuildResidentTrees();
    }

    /// <summary>
    /// Slot <c>0x14</c> via
    /// <c>0059B5D7</c>. Native
    /// always writes the packet
    /// through type-10
    /// <c>vtbl+284</c>
    /// <c>0054E4F0</c>. Host
    /// stores packet first dword at
    /// widget <c>+352</c>. Persist
    /// <see cref="FrontendWidget.ActionOnLeftUnclicked"/>
    /// stays 0.
    /// </summary>
    private void WriteType10AttachMessage()
    {
        Note(FrontendInputMap.AttachWriteE5, "Frontend", "UI",
            $"00598EE6 slot 0x{FrontendPressStartSlot:X} vtbl+{FrontendInputMap.WidgetMessageVtbl} {FrontendInputMap.Type10StoreMsgFn:X} +{FrontendInputMap.Type10StoredMsgOffset} 0x{FrontendPressStartMessage:X}");
        Note(FrontendSlotLookupFn, "Frontend", "UI",
            $"0059B5D7 slot 0x{FrontendPressStartSlot:X} vtbl+{FrontendInputMap.WidgetMessageVtbl} {FrontendInputMap.Type10StoreMsgFn:X}");
        if (!_frontendSlotTrees.TryGetValue(FrontendPressStartSlot, out var tree) ||
            tree.Count == 0)
            return;
        tree[0] = tree[0] with { Type10Packet = FrontendPressStartMessage };
    }

    public bool TryGetFrontendSlot(int slot, out FrontendWidget widget)
    {
        if (_frontendSlotTrees.TryGetValue(slot, out var tree) &&
            tree.Count > 0)
        {
            widget = tree[0];
            return true;
        }

        widget = default;
        return false;
    }

    public IReadOnlyList<FrontendWidget> FrontendSlotTree(int slot) =>
        _frontendSlotTrees.TryGetValue(slot, out var tree) ? tree : [];

    /// <summary>
    /// Materialises native key-order only when the resident set/current slot
    /// changes. Frame tick, draw and submit then use indexed walks without
    /// allocating iterator state machines.
    /// </summary>
    private void RebuildResidentTrees()
    {
        _frontendResidentTrees.Clear();
        if (_frontendSlotTrees.Count == 0)
        {
            if (_frontendWidgets.Count > 0)
            {
                FrontendResidentSlots = [];
                _frontendResidentTrees.Add(_frontendWidgets);
            }
            else
                FrontendResidentSlots = [];
            return;
        }

        var keys = _frontendResidentSlotKeys;
        // Retired resident content completes before the current screen. The
        // current slot is therefore the final content layer even when Main
        // Menu reuses map key 0. The type-32 pointer is registered separately
        // and deferred by CollectFrontendRecords above all screen content.
        foreach (var key in keys)
        {
            if (key == FrontendCurrentSlot)
                continue;
            var tree = _frontendSlotTrees[key];
            if (tree.Count == 0)
                continue;
            _frontendResidentTrees.Add(tree);
        }
        if (_frontendSlotTrees.TryGetValue(FrontendCurrentSlot, out var current) &&
            current.Count > 0)
            _frontendResidentTrees.Add(current);
    }

    /// <summary>
    /// Type 11/38 <c>vtbl+4</c>
    /// <c>0055ACB0</c> → <c>0055B890</c>
    /// → <c>vtbl+580</c> <c>0055BF10</c>
    /// on dest AABB of the current
    /// pointer. <c>+352</c> then
    /// action 26. Not TypeMouse-only.
    /// <c>vtbl+532</c> is list unlink
    /// <c>0055BBF0</c>, not a texture
    /// suffix.
    /// </summary>
    private void TickType11Type38Hover(List<FrontendWidget> tree)
    {
        Note(FrontendHitTest.HoverSelectFn, "Frontend", "UI",
            "0055ACB0 vtbl+580 0055BF10");
        var hitIndex = FrontendHitTest.HitIndex(
            tree, FrontendPointerX, FrontendPointerY);
        for (var i = 0; i < tree.Count; i++)
        {
            var widget = tree[i];
            if (widget.Type != FrontendInputMap.TypeButton &&
                widget.Type != FrontendInputMap.TypeAccept)
                continue;
            var hit = FrontendHitTest.Contains(
                tree, i, FrontendPointerX, FrontendPointerY)
                || hitIndex == i;
            if (hit == widget.Hovered)
                continue;

            if (hit)
            {
                // 0055BAE0 saves +332 in +348, reads vtbl+516 from the first
                // authored child, and selects that state on the button.
                var firstChild = FrontendWidgetFactory.FirstChild(tree, i);
                var hoverState = firstChild >= 0
                    ? tree[firstChild].HoveredState
                    : widget.State;
                tree[i] = widget with
                {
                    Hovered = true,
                    ActiveChild = widget.State,
                };
                ForwardSelectState(tree, i, hoverState);
                ApplyAuthoredHoverVisuals(tree, i, entering: true);
            }
            else
            {
                // 0055B9A0 restores the state saved in +348.
                var restoreState = widget.ActiveChild;
                tree[i] = widget with { Hovered = false };
                ForwardSelectState(tree, i, restoreState);
                ApplyAuthoredHoverVisuals(tree, i, entering: false);
            }
        }
    }

    private void ApplyAuthoredHoverVisuals(
        List<FrontendWidget> tree, int button, bool entering)
    {
        // 0055AEB0/0055AEF0 publish actions 26/31/27/32 after the button
        // transition. The authored descendants subscribe to that pair. Model
        // the recovered result with each descendant's CUIDef+516 state; keep
        // its prior +332 value for the matching leave notification.
        for (var child = 0; child < tree.Count; child++)
        {
            if (tree[child].ParentIndex != button)
                continue;
            var visual = tree[child];
            if (entering)
            {
                tree[child] = visual with { ActiveChild = visual.State };
                ForwardSelectState(tree, child, visual.HoveredState);
            }
            else
            {
                ForwardSelectState(tree, child, visual.ActiveChild);
            }
        }
    }

    /// <summary>
    /// Action 26 <c>[inner+364]=1</c> on
    /// the type 11/38 under the pointer
    /// (<c>0055B8F0</c>). Not every
    /// visible button.
    /// </summary>
    private void ArmType34Widgets()
    {
        UnarmType34Widgets();
        var hit = FrontendHitTest.HitIndex(
            _frontendWidgets, FrontendPointerX, FrontendPointerY);
        if (hit is not int index)
            return;
        var widget = _frontendWidgets[index];
        if (widget.Type == FrontendWidgetType.TextSlider)
        {
            ApplyTextSliderHit(index);
            return;
        }

        if (widget.Type == 15)
        {
            ApplySliderHit(index);
            return;
        }

        if (widget.Type != FrontendInputMap.TypeButton &&
            widget.Type != FrontendInputMap.TypeAccept)
            return;
        // 0055AD60: +352==0 skips vtbl+584
        // and does not write +364.
        if (!widget.Hovered)
            return;
        _frontendWidgets[index] = widget with { Armed = true };
    }

    private void ApplyTextSliderHit(int index)
    {
        var widget = _frontendWidgets[index];
        var childCount = FrontendWidgetFactory.ChildCount(_frontendWidgets, index);
        if (childCount == 0)
            return;
        var next = widget.ActiveChild;
        if (FrontendHitTest.IsLeftHalf(_frontendWidgets, index, FrontendPointerX))
            next = next <= 0 ? childCount - 1 : next - 1;
        else
            next = next + 1 >= childCount ? 0 : next + 1;
        _frontendWidgets[index] = widget with { ActiveChild = next, Armed = true };
        // 00548AF0 applies state 1 to the old choice and state 3 to the new
        // choice.  Both remain in the authored child list; Visible is not an
        // exclusive-selection flag.
        ForwardSelectState(_frontendWidgets,
            FrontendWidgetFactory.ChildAt(_frontendWidgets, index,
                Math.Clamp(widget.ActiveChild, 0, childCount - 1)), 1);
        ForwardSelectState(_frontendWidgets,
            FrontendWidgetFactory.ChildAt(_frontendWidgets, index, next), 3);
    }

    private void ApplySliderHit(int index)
    {
        var widget = _frontendWidgets[index];
        _frontendWidgets[index] = widget with { Armed = true, Hovered = true };
    }

    private void UnarmType34Widgets()
    {
        for (var i = 0; i < _frontendWidgets.Count; i++)
        {
            var widget = _frontendWidgets[i];
            if (!widget.Armed)
                continue;
            _frontendWidgets[i] = widget with { Armed = false };
        }
    }

    private void AttachFrontendTree(string rootName, int? slot = null)
    {
        if (Install is not null && _frontendSprites is null)
            _frontendSprites = new FrontendSpriteBank(Install);
        if (Install is not null && _frontendFonts is null)
            _frontendFonts = new FontBank(Install);
        _frontendWidgets = [];
        FrontendChildCount = 0;
        FrontendRootType = 0;
        if (FrontendDefs is null)
        {
            ResolveFrontendDef(rootName);
            if (FrontendDefs is null)
                return;
        }

        NamesBin? names = null;
        if (Install is not null)
        {
            var namesPath = Install.FindCompiledDef("names.bin");
            if (namesPath is not null)
                names = NamesBin.Load(namesPath);
        }

        var built = FrontendWidgetFactory.Build(
            FrontendDefs, rootName, _frontendSprites, LookupFrontendText, names);
        _frontendWidgets.AddRange(built);
        if (_frontendWidgets.Count > 0)
            FrontendRootType = _frontendWidgets[0].Type;
        FrontendChildCount = Math.Max(0, _frontendWidgets.Count - 1);
        foreach (var widget in _frontendWidgets)
        {
            if (widget.Name == rootName)
                continue;
            Note(FrontendWidgetType.ChildAttachFn, "Frontend", "UI",
                $"005331A0 child {widget.Name} type {widget.Type}");
        }

        if (slot is int key)
            BindFrontendSlot(key);
    }

    private void ApplyFrontendScaleInit()
    {
        Note(FrontendScaleInitVtblFn, "Frontend", "UI",
            $"0054E4B0 vtbl+{FrontendScaleInitVtbl} 0052C730");
        Note(FrontendScaleInitFn, "Frontend", "UI", "0052C730");
        Note(FrontendScaleWriteFn, "Frontend", "UI",
            "005339B0 +280=0 +272/+276=1.0 +144..+147=0xFF");
        FrontendScaleX = FrontendScaleOne;
        FrontendScaleY = FrontendScaleOne;
    }

    private void LayoutFrontendWidgets() =>
        LayoutFrontendWidgets(_frontendWidgets);

    private void LayoutFrontendWidgets(List<FrontendWidget> widgets)
    {
        var width = BackBufferWidth > 0 ? BackBufferWidth : DisplayDefaultWidth;
        var height = BackBufferHeight > 0 ? BackBufferHeight : DisplayDefaultHeight;
        var viewport = FrontendLayout.FirstSeenFrontend(width, height);
        if (!_frontendLayoutScratch.TryGetValue(widgets, out var scratch) ||
            scratch.Dests.Length != widgets.Count)
        {
            scratch = new FrontendLayoutScratch(widgets.Count);
            _frontendLayoutScratch[widgets] = scratch;
        }
        var dests = scratch.Dests;
        var sibling = scratch.SiblingCounts;
        Array.Clear(sibling);
        for (var i = 0; i < widgets.Count; i++)
        {
            var widget = widgets[i];
            var persistW = widget.PersistWidth > 0 ? (int)widget.PersistWidth : 0;
            var persistH = widget.PersistHeight > 0 ? (int)widget.PersistHeight : 0;
            var (leftoverW, leftoverH) = WidgetLeftover(widget);

            FrontendDest? parentDest = null;
            FrontendWidget? parentWidget = null;
            if ((uint)widget.ParentIndex < (uint)i)
            {
                parentDest = dests[widget.ParentIndex];
                parentWidget = widgets[widget.ParentIndex];
            }

            var persistX = widget.PersistX;
            var persistY = widget.PersistY;
            var siblingIndex = 0;
            if (widget.ParentIndex >= 0)
            {
                siblingIndex = sibling[widget.ParentIndex];
                sibling[widget.ParentIndex] = siblingIndex + 1;
            }

            if (parentWidget is { Type: FrontendWidgetType.List } list)
            {
                var firstX = persistX;
                var firstY = persistY;
                if (siblingIndex > 0)
                {
                    for (var s = 0; s < i; s++)
                    {
                        if (widgets[s].ParentIndex != widget.ParentIndex)
                            continue;
                        firstX = widgets[s].PersistX;
                        firstY = widgets[s].PersistY;
                        break;
                    }
                }

                (persistX, persistY) = FrontendLayout.ListChildAuthoredPos(
                    siblingIndex,
                    persistX,
                    persistY,
                    list.PositionOffsetX,
                    list.PositionOffsetY,
                    firstX,
                    firstY);
            }

            FrontendDest dest;
            var spriteClone = persistW == 0 && persistH == 0;
            if (parentWidget is { Type: FrontendWidgetType.TableType } table &&
                parentDest is { } tableDest &&
                spriteClone)
            {
                var cellCount = 0;
                var sliceIndex = 0;
                var firstCapW = 0f;
                var rightCapW = 0f;
                for (var c = 0; c < widgets.Count; c++)
                {
                    if (widgets[c].ParentIndex != widget.ParentIndex)
                        continue;
                    if (widgets[c].PersistWidth > 0 || widgets[c].PersistHeight > 0)
                        continue;
                    if (c == i)
                        sliceIndex = cellCount;
                    var cap = WidgetLeftover(widgets[c]).W;
                    if (cellCount == 0)
                        firstCapW = cap;
                    else if (cellCount == 1)
                        rightCapW = cap;
                    cellCount++;
                }

                var placed = FrontendLayout.PlaceTableCell(
                    sliceIndex,
                    cellCount,
                    tableDest.X0,
                    tableDest.Y0,
                    tableDest.X1 > tableDest.X0 ? tableDest.X1 - tableDest.X0 : leftoverW,
                    leftoverH > 0f ? leftoverH : (table.PersistHeight > 0f ? table.PersistHeight : leftoverH),
                    leftoverW,
                    leftoverH,
                    table.ExpansionType,
                    firstCapW,
                    rightCapW);
                dest = new FrontendDest(
                    placed.X0, placed.Y0,
                    tableDest.ScaleX, tableDest.ScaleY,
                    placed.X0, placed.Y0, placed.X1, placed.Y1);
            }
            else
            {
                var layout = new FrontendWidgetLayout(
                    persistX,
                    persistY,
                    PersistScaleX: widget.PersistScaleX,
                    PersistScaleY: widget.PersistScaleY,
                    PersistWidth: persistW,
                    PersistHeight: persistH,
                    LeftoverW: leftoverW,
                    LeftoverH: leftoverH,
                    PositionIsCenter: widget.PositionIsCenter,
                    Independant: widget.Independant,
                    UseRelativePosition: widget.UseRelativePosition,
                    UseRelativeZoom: widget.UseRelativeZoom);
                dest = FrontendLayout.Compute(layout, parentDest, viewport);
            }

            dests[i] = dest;
            widgets[i] = widget with
            {
                DestX0 = dest.X0,
                DestY0 = dest.Y0,
                DestX1 = dest.X1,
                DestY1 = dest.Y1,
                Leftover204 = leftoverW,
                DestScaleX = dest.ScaleX,
                TextOriginX = dest.X0,
                TextOriginY = dest.Y0,
            };
            if (i == 0 && ReferenceEquals(widgets, _frontendWidgets))
            {
                FrontendWidgetDestX0 = dest.X0;
                FrontendWidgetDestY0 = dest.Y0;
                FrontendWidgetDestX1 = dest.X1;
                FrontendWidgetDestY1 = dest.Y1;
                FrontendScaleX = dest.ScaleX;
                FrontendScaleY = dest.ScaleY;
            }
        }

        ExpandTableDests(widgets);
        AssignHitRects(widgets);
    }

    private (float W, float H) WidgetLeftover(FrontendWidget widget)
    {
        var leftoverW = 0f;
        var leftoverH = 0f;
        if (widget.Type == FrontendWidgetType.TableType)
            (leftoverW, leftoverH) = FrontendLayout.Type2Leftover(
                widget.PersistWidth, widget.PersistHeight);
        if (widget.TextureName is { } leftoverName &&
            _frontendSprites?.TryLoad(leftoverName) is { } leftoverTex)
        {
            var frameW = leftoverTex.FrameWidth > 0 ? leftoverTex.FrameWidth : leftoverTex.Width;
            var frameH = leftoverTex.FrameHeight > 0 ? leftoverTex.FrameHeight : leftoverTex.Height;
            var graphic = FrontendLayout.LeftoverFromGraphic(
                widget.GraphicId, frameW, frameH);
            if (graphic.W > leftoverW)
                leftoverW = graphic.W;
            if (graphic.H > leftoverH)
                leftoverH = graphic.H;
        }

        return (leftoverW, leftoverH);
    }

    private static void ExpandTableDests(List<FrontendWidget> widgets)
    {
        for (var i = 0; i < widgets.Count; i++)
        {
            var widget = widgets[i];
            if (widget.Type != FrontendWidgetType.TableType)
                continue;
            if (widget.DestY1 > widget.DestY0)
                continue;
            var have = false;
            var y1 = widget.DestY0;
            for (var c = 0; c < widgets.Count; c++)
            {
                if (widgets[c].ParentIndex != i)
                    continue;
                if (widgets[c].DestY1 > y1)
                {
                    y1 = widgets[c].DestY1;
                    have = true;
                }
            }

            if (!have || y1 <= widget.DestY0)
                continue;
            widgets[i] = widget with { DestY1 = y1 };
        }
    }

    private static void AssignHitRects(List<FrontendWidget> widgets)
    {
        for (var i = 0; i < widgets.Count; i++)
        {
            var widget = widgets[i];
            float x0 = widget.DestX0;
            float y0 = widget.DestY0;
            float x1 = widget.DestX1;
            float y1 = widget.DestY1;
            if (x1 <= x0 || y1 <= y0)
                TryChromeHit(widgets, i, ref x0, ref y0, ref x1, ref y1);
            widgets[i] = widget with
            {
                HitX0 = x0,
                HitY0 = y0,
                HitX1 = x1,
                HitY1 = y1,
            };
        }
    }

    /// <summary>
    /// Leftover #48: type-16/37 dest is a
    /// point. Native sibling type-2
    /// leftover is not a child dest.
    /// Do not treat this size as
    /// <c>0055B8F0</c> recovered.
    /// </summary>
    private static void TryChromeHit(
        List<FrontendWidget> widgets,
        int index,
        ref float x0,
        ref float y0,
        ref float x1,
        ref float y1)
    {
        var widget = widgets[index];
        if (widget.Type != FrontendWidgetType.TextSlider &&
            widget.Type != FrontendWidgetType.EditBox)
            return;
        var row = index;
        while ((uint)row < (uint)widgets.Count)
        {
            var parent = widgets[row].ParentIndex;
            if (parent < 0)
                break;
            if (widgets[parent].Type == FrontendWidgetType.List)
                break;
            row = parent;
        }

        var bestX0 = float.NegativeInfinity;
        var bestW = 0f;
        var bestH = 0f;
        for (var c = 0; c < widgets.Count; c++)
        {
            if (widgets[c].Type != FrontendWidgetType.TableType)
                continue;
            var walk = c;
            var under = false;
            while ((uint)walk < (uint)widgets.Count)
            {
                if (walk == row)
                {
                    under = true;
                    break;
                }

                walk = widgets[walk].ParentIndex;
                if (walk < 0)
                    break;
            }

            if (!under)
                continue;
            var w = widgets[c].DestX1 - widgets[c].DestX0;
            var h = widgets[c].DestY1 - widgets[c].DestY0;
            if (w <= 0f || h <= 0f)
                continue;
            if (widgets[c].DestX0 < bestX0)
                continue;
            bestX0 = widgets[c].DestX0;
            bestW = w;
            bestH = h;
        }

        if (bestW <= 0f || bestH <= 0f)
            return;
        x0 = widget.DestX0;
        y0 = widget.DestY0;
        x1 = x0 + bestW;
        y1 = y0 + bestH;
    }

    private string? LookupFrontendText(string id)
    {
        if (Runtime?.LookupText(id) is { } hit)
            return hit;
        if (Install is null || !File.Exists(Install.TextBigPath))
            return null;
        using var big = BigArchive.Open(Install.TextBigPath);
        foreach (var bank in big.SubBanks)
        {
            foreach (var entry in big.ReadEntries(bank))
            {
                if (entry.Name.Equals(id, StringComparison.OrdinalIgnoreCase))
                    return TextPayload.ReadUtf16(big.Read(entry));
            }
        }

        return null;
    }

    /// <summary>
    /// Present is <see cref="FrontendBatch"/>
    /// (<c>00BAE2D0</c> / <c>00AB7C20</c> →
    /// Vulkan). CPU blit into
    /// <see cref="FrontendPresentRgba"/> is a
    /// TEMPORARY test dump only.
    /// </summary>
    private void CompositeFrontendPresent()
    {
        var width = BackBufferWidth > 0 ? BackBufferWidth : DisplayDefaultWidth;
        var height = BackBufferHeight > 0 ? BackBufferHeight : DisplayDefaultHeight;
        CollectFrontendRecords();
        var records = _frontendDx9Records;
        var textures = _frontendDx9Textures;
        if (Dx9OwnsFrontendPresent)
        {
            FrontendBatch = null;
            return;
        }

        FrontendBatch = Dx9VulkanFrontend.BuildBatch(records, textures, 0, 0, width, height);
        // The bitmap is a diagnostic/headless-test artifact. A live DX9/Vulkan
        // device consumes FrontendBatch directly; rebuilding a full RGBA
        // backbuffer here allocated ~3.1 MiB and CPU-blitted every frame.
        if (Device is null)
            DumpFrontendPresentRgba(records, textures, width, height);
    }

    private void CollectFrontendRecords()
    {
        var records = _frontendDx9Records;
        var textures = _frontendDx9Textures;
        var textureIndex = _frontendTextureIndex;
        records.Clear();
        _frontendCursorRecords.Clear();
        textures.Clear();
        textureIndex.Clear();
        _frontendSubmitCounts.Clear();
        RebuildResidentTrees();
        for (var treeIndex = 0; treeIndex < _frontendResidentTrees.Count; treeIndex++)
        {
        var tree = _frontendResidentTrees[treeIndex];
        var submitBase = _frontendSubmitCounts.Count;
        for (var i = 0; i < tree.Count; i++)
            _frontendSubmitCounts.Add(0);
        var drawOrder = 0;
        // 0052CF40(6) on the old current.
        // 0052C7E0 style 0x20 zeros dest
        // so 0041AFA0 submits nothing.
        // 0041AFA0 passes signed +303 plus the inherited layer to the
        // frontend renderer. Lower layers are flushed later, independently
        // of persist-list order.
        foreach (var i in FrontendRecordOrder(tree))
        {
            var widget = tree[i];
            var drawX0 = widget.DestX0;
            var drawY0 = widget.DestY0;
            var drawX1 = widget.DestX1;
            var drawY1 = widget.DestY1;
            // 0041A980 is the type-32 vtbl+8 body. Unlike
            // 0041AFA0, it reads the live input position
            // immediately before submitting the cursor rect.
            if (widget.Type == FrontendWidgetType.Mouse)
            {
                (drawX0, drawY0, drawX1, drawY1) = FrontendSpriteDraw.CursorDest(
                    drawX0, drawY0, drawX1, drawY1,
                    FrontendPointerX, FrontendPointerY);
            }

            var u0 = widget.U0;
            var v0 = widget.V0;
            var u1 = widget.U1;
            var v1 = widget.V1;
            var haveUv = false;
            var glyphs = 0;
            // 0052FE3C..0052FFA2 composes the local style colour with the
            // inherited parent colour before 0041AFA0/0054EF00 reads +148.
            // Type 32 has its own 0041A980 live-pointer submit path and
            // remains registered when the Press Start slot is retired.  It
            // must not inherit that old root's state-6 fade or it disappears
            // from every subsequent menu.
            var colour = widget.Type == FrontendWidgetType.Mouse
                ? widget.Colour
                : FrontendWidgetFactory.EffectiveColour(tree, i);
            var targetRecords = widget.Type == FrontendWidgetType.Mouse
                ? _frontendCursorRecords
                : records;
            var recordStart = targetRecords.Count;
            if (FrontendWidgetFactory.IsPresented(tree, i) &&
                !FrontendWidgetType.LeafDipSkipped(colour) &&
                drawX1 > drawX0 && drawY1 > drawY0)
            {
                if (widget.TextureName is { } texName &&
                    _frontendSprites?.TryLoad(texName) is { } tex)
                {
                    if (!textureIndex.TryGetValue(texName, out var id))
                    {
                        id = textures.Count;
                        textureIndex[texName] = id;
                        textures.Add(new GpuTexture(id, tex.Width, tex.Height, tex.Rgba));
                    }

                    var frame = tex.FrameUv();
                    var uv = FrontendDx9Submit.SubmittedSpriteUv(
                        0f, 0f, 0f, 0f,
                        frame.U0, frame.V0, frame.U1, frame.V1);
                    targetRecords.Add(new FrontendDx9DrawRecord(
                        drawX0, drawY0, drawX1, drawY1,
                        uv.U0, uv.V0, uv.U1, uv.V1, colour, id,
                        Dx9VulkanFrontend.WidgetBlendDefault,
                        (int)Dx9VulkanFrontend.RecordType,
                        Dx9VulkanFrontend.VertexStride,
                        (int)FrontendDx9Vertex.NativeUsedBytes));
                    u0 = uv.U0;
                    v0 = uv.V0;
                    u1 = uv.U1;
                    v1 = uv.V1;
                    haveUv = true;
                }
            }

            if (FrontendWidgetFactory.IsPresented(tree, i) &&
                !FrontendWidgetType.LeafDipSkipped(colour) &&
                widget.Type == FrontendWidgetType.Text &&
                !string.IsNullOrEmpty(widget.Text))
            {
                var faceName = widget.FontFace;
                var face = faceName is not null
                    ? _frontendFonts?.TryLoad(faceName)
                    : _frontendFonts?.TryLoad(FrontendUiFontFace);
                if (face is not null)
                {
                    var atlasKey = faceName ?? FrontendUiFontFace;
                    if (!textureIndex.TryGetValue(atlasKey, out var atlasId))
                    {
                        atlasId = textures.Count;
                        textureIndex[atlasKey] = atlasId;
                        textures.Add(new GpuTexture(
                            atlasId, face.UvWidth, face.UvHeight, face.Atlas));
                    }

                    // 0054EF00 leftover +204 is the
                    // widget field, not dest width.
                    // First-seen type-6 GraphicIndex=0
                    // never writes +204.
                    var align = FrontendTextDraw.AlignFromFlag302(widget.Flag302);
                    var penY = widget.DestY0;
                    var anchorX = widget.DestX0;
                    var drawText = EditBoxTextWithCaret(tree, i, widget.Text);
                    var layoutKey = new FrontendTextLayoutKey(
                        atlasKey, drawText, anchorX, penY, align);
                    if (!_frontendTextLayouts.TryGetValue(layoutKey, out var glyphQuads))
                    {
                        // Geometry depends on the parsed face/text/layout only.
                        // Tint is packed into each live draw record below.
                        glyphQuads = FrontendTextDraw.LayoutFormatted(
                            face, drawText, anchorX, penY, align);
                        _frontendTextLayouts[layoutKey] = glyphQuads;
                    }
                    // 0054F5C0 initializes +393 to zero. In that 0054EF00
                    // branch, the unshifted record receives widget RGBA and
                    // the (+2,+2) record receives zero RGB/widget alpha.
                    // Native layer ordering leaves the latter behind the
                    // coloured glyphs, so submit the underlay first here.
                    foreach (var glyph in glyphQuads)
                    {
                        records.Add(new FrontendDx9DrawRecord(
                            glyph.DestX0 + FrontendTextDraw.Type6OriginPad,
                            glyph.DestY0 + FrontendTextDraw.Type6OriginPad,
                            glyph.DestX1 + FrontendTextDraw.Type6OriginPad,
                            glyph.DestY1 + FrontendTextDraw.Type6OriginPad,
                            glyph.U0, glyph.V0, glyph.U1, glyph.V1,
                            FrontendTextDraw.BlackUnderlayColor(colour), atlasId,
                            Dx9VulkanFrontend.WidgetBlendDefault,
                            FrontendTextDraw.Type6RecordType,
                            FrontendTextDraw.VertexStride,
                            FrontendTextDraw.VertexStride,
                            AppliesHalfPixel: true));
                    }
                    foreach (var glyph in glyphQuads)
                    {
                        records.Add(new FrontendDx9DrawRecord(
                            glyph.DestX0, glyph.DestY0, glyph.DestX1, glyph.DestY1,
                            glyph.U0, glyph.V0, glyph.U1, glyph.V1,
                            colour, atlasId,
                            Dx9VulkanFrontend.WidgetBlendDefault,
                            FrontendTextDraw.Type6RecordType,
                            FrontendTextDraw.VertexStride,
                            FrontendTextDraw.VertexStride,
                            AppliesHalfPixel: true));
                    }

                    foreach (var glyph in glyphQuads)
                    {
                        if (!haveUv)
                        {
                            u0 = glyph.U0;
                            v0 = glyph.V0;
                            u1 = glyph.U1;
                            v1 = glyph.V1;
                            haveUv = true;
                        }
                        else
                        {
                            if (glyph.U0 < u0)
                                u0 = glyph.U0;
                            if (glyph.V0 < v0)
                                v0 = glyph.V0;
                            if (glyph.U1 > u1)
                                u1 = glyph.U1;
                            if (glyph.V1 > v1)
                                v1 = glyph.V1;
                        }
                    }

                    glyphs = glyphQuads.Count;
                }
            }

            tree[i] = widget with
            {
                U0 = haveUv ? u0 : widget.U0,
                V0 = haveUv ? v0 : widget.V0,
                U1 = haveUv ? u1 : widget.U1,
                V1 = haveUv ? v1 : widget.V1,
                GlyphCount = glyphs,
                DrawOrder = drawOrder++,
            };
            _frontendSubmitCounts[submitBase + i] =
                targetRecords.Count - recordStart;
        }

        // Type-6 emits both text records in its selected frontend layer;
        // there is no later global glyph bucket.
        }
        records.AddRange(_frontendCursorRecords);
    }

    private int[] FrontendRecordOrder(List<FrontendWidget> tree)
    {
        if (_frontendRecordOrders.TryGetValue(tree, out var order) &&
            order.Length == tree.Count)
            return order;
        order = Enumerable.Range(0, tree.Count).ToArray();
        Array.Sort(order, (left, right) =>
        {
            var layer = EffectiveFrontendLayer(tree, right)
                .CompareTo(EffectiveFrontendLayer(tree, left));
            return layer != 0 ? layer : left.CompareTo(right);
        });
        _frontendRecordOrders[tree] = order;
        return order;
    }

    private static int EffectiveFrontendLayer(
        IReadOnlyList<FrontendWidget> tree, int index)
    {
        var layer = 0;
        var remaining = tree.Count + 1;
        while ((uint)index < (uint)tree.Count && remaining-- > 0)
        {
            layer += tree[index].Layer;
            index = tree[index].ParentIndex;
        }
        return layer;
    }

    private string EditBoxTextWithCaret(
        IReadOnlyList<FrontendWidget> tree, int index, string text)
    {
        if (!FrontendEditBoxBound || !FrontendUi96Present ||
            !ReferenceEquals(tree, _frontendWidgets))
            return text;
        var parent = tree[index].ParentIndex;
        if ((uint)parent >= (uint)tree.Count ||
            tree[parent].Type != FrontendWidgetType.EditBox)
            return text;
        var cursor = Math.Clamp(_editBoxCursor, 0, text.Length);
        if (_frontendCaretCursor == cursor &&
            ReferenceEquals(_frontendCaretSource, text) &&
            _frontendCaretText is not null)
            return _frontendCaretText;
        _frontendCaretSource = text;
        _frontendCaretCursor = cursor;
        _frontendCaretText = text.Insert(cursor, "|");
        return _frontendCaretText;
    }

    private readonly record struct FrontendTextLayoutKey(
        string FaceName, string Text, float X, float Y, int Align);

    /// <summary>
    /// TEMPORARY test dump. Present must not
    /// use this bitmap; host Present is
    /// <see cref="FrontendBatch"/>.
    /// </summary>
    private void DumpFrontendPresentRgba(
        List<FrontendDx9DrawRecord> records,
        List<GpuTexture> textures,
        int width,
        int height)
    {
        var rgba = new byte[width * height * 4];
        foreach (var rec in records)
        {
            if ((uint)rec.TextureId >= (uint)textures.Count)
                continue;
            var tex = textures[rec.TextureId];
            BlitFrontendQuad(rgba, width, height, rec, tex);
        }

        FrontendPresentRgba = rgba;
        FrontendPresentWidth = width;
        FrontendPresentHeight = height;
    }

    /// <summary>
    /// One row per constructed widget after
    /// dest layout and batch submit.
    /// UVs come from <see cref="FrontendBatch"/>
    /// when that widget produced draws.
    /// </summary>
    public IReadOnlyList<FrontendFrameDumpRow> DumpFrontendFrame()
    {
        var rows = new List<FrontendFrameDumpRow>(_frontendWidgets.Count);
        var byName = new Dictionary<string, FrontendWidget>(
            _frontendWidgets.Count, StringComparer.Ordinal);
        foreach (var widget in _frontendWidgets)
            byName[widget.Name] = widget;
        var face = _frontendFonts?.TryLoad(FrontendUiFontFace);
        var batch = FrontendBatch;
        var drawCursor = 0;
        for (var i = 0; i < _frontendWidgets.Count; i++)
        {
            var widget = _frontendWidgets[i];
            FrontendWidget? parent = null;
            if (widget.ParentName is { } parentName &&
                byName.TryGetValue(parentName, out var found))
                parent = found;
            var submitted = i < _frontendSubmitCounts.Count
                ? _frontendSubmitCounts[i]
                : FrontendFrameDump.SubmittedDraws(widget);
            rows.Add(FrontendFrameDump.Row(
                widget, parent, batch, drawCursor, submitted, face));
            drawCursor += submitted;
        }

        return rows;
    }

    public string WriteFrontendFrameDump(string path)
    {
        var rows = DumpFrontendFrame();
        var draws = FrontendBatch?.Draws.Length ?? 0;
        FrontendFrameDump.Write(path, rows, draws);
        return path;
    }

    public string WriteNewProfileWidgetDump(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, FrontendFrameDump.FormatNewProfile(_frontendWidgets));
        return path;
    }

    private static void BlitFrontendQuad(
        byte[] rgba, int width, int height, FrontendDx9DrawRecord rec, GpuTexture tex)
    {
        var x0 = (int)MathF.Round(rec.DestX0);
        var y0 = (int)MathF.Round(rec.DestY0);
        var x1 = (int)MathF.Round(rec.DestX1);
        var y1 = (int)MathF.Round(rec.DestY1);
        var dw = Math.Max(1, x1 - x0);
        var dh = Math.Max(1, y1 - y0);
        for (var y = 0; y < dh; y++)
        {
            var dy = y0 + y;
            if ((uint)dy >= (uint)height)
                continue;
            var v = rec.V0 + (rec.V1 - rec.V0) * ((y + 0.5f) / dh);
            var sy = Math.Clamp((int)(v * tex.Height), 0, Math.Max(0, tex.Height - 1));
            var row = dy * width * 4;
            var srcRow = sy * tex.Width * 4;
            for (var x = 0; x < dw; x++)
            {
                var dx = x0 + x;
                if ((uint)dx >= (uint)width)
                    continue;
                var u = rec.U0 + (rec.U1 - rec.U0) * ((x + 0.5f) / dw);
                var sx = Math.Clamp((int)(u * tex.Width), 0, Math.Max(0, tex.Width - 1));
                var s = srcRow + sx * 4;
                var a = tex.Rgba[s + 3];
                var sr = tex.Rgba[s];
                var sg = tex.Rgba[s + 1];
                var sb = tex.Rgba[s + 2];
                if (rec.RecordType == FrontendTextDraw.Type6RecordType)
                {
                    sr = (byte)(sr * ((rec.DiffuseArgb >> 16) & 0xFF) / 255);
                    sg = (byte)(sg * ((rec.DiffuseArgb >> 8) & 0xFF) / 255);
                    sb = (byte)(sb * (rec.DiffuseArgb & 0xFF) / 255);
                    a = (byte)(a * (rec.DiffuseArgb >> 24) / 255);
                }
                if (a == 0)
                    continue;
                var d = row + dx * 4;
                if (a == 255)
                {
                    rgba[d] = sr;
                    rgba[d + 1] = sg;
                    rgba[d + 2] = sb;
                    rgba[d + 3] = 255;
                    continue;
                }

                var ia = 255 - a;
                rgba[d] = (byte)((sr * a + rgba[d] * ia) / 255);
                rgba[d + 1] = (byte)((sg * a + rgba[d + 1] * ia) / 255);
                rgba[d + 2] = (byte)((sb * a + rgba[d + 2] * ia) / 255);
                rgba[d + 3] = 255;
            }
        }
    }

    private static void BlitFrontendTexture(
        byte[] rgba, int width, int height, int x0, int y0, TextureFile tex)
    {
        for (var y = 0; y < tex.Height; y++)
        {
            var dy = y0 + y;
            if ((uint)dy >= (uint)height)
                continue;
            var row = dy * width * 4;
            for (var x = 0; x < tex.Width; x++)
            {
                var dx = x0 + x;
                if ((uint)dx >= (uint)width)
                    continue;
                var s = (y * tex.Width + x) * 4;
                var a = tex.Rgba[s + 3];
                if (a == 0)
                    continue;
                var d = row + dx * 4;
                if (a == 255)
                {
                    rgba[d] = tex.Rgba[s];
                    rgba[d + 1] = tex.Rgba[s + 1];
                    rgba[d + 2] = tex.Rgba[s + 2];
                    rgba[d + 3] = 255;
                    continue;
                }

                var ia = 255 - a;
                rgba[d] = (byte)((tex.Rgba[s] * a + rgba[d] * ia) / 255);
                rgba[d + 1] = (byte)((tex.Rgba[s + 1] * a + rgba[d + 1] * ia) / 255);
                rgba[d + 2] = (byte)((tex.Rgba[s + 2] * a + rgba[d + 2] * ia) / 255);
                rgba[d + 3] = 255;
            }
        }
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
        _frontendSprites?.Dispose();
        _frontendSprites = null;
        _frontendFonts?.Dispose();
        _frontendFonts = null;
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
            $"00403079 [{BackBufferWidth}x{BackBufferHeight}]");
        Note(DisplayWindowFlagVa, "Setup library", "Window",
            $"00403079 [0x137544A]={DisplayWindowFlag} 009BF7E0 +{PresentParametersWindowedOffset} Windowed={DeviceWindowed}");
        Note(DisplayZDepthVa, "Setup library", "Window",
            $"00403079 [0x1375468]={DisplayZDepth}");
        Note(WindowTitleFn, "Setup library", "Window",
            "004023F0 " + WindowTitleId);
        Note(InputDeviceVa, "Setup library", "Input",
            "0042E3EE [0x13B8388]");
        Note(EngineOptionsFlagVa, "Setup library", "Engine",
            $"01375449={EngineOptionsFlagFirstSeen} [opt+{EngineOptionsFlagsOffset}]={EngineOptionsFlagsFirstSeen}");
        Note(CreateInputFn, "Setup library", "Input",
            "00A60050 / 009A7180 engine+88");
        EnginePlus88 = true;
        EnginePlus124 = false;
        EnginePlus9 = EnginePlus9AfterSetup;
        Note(CreateWindowFn, "Setup library", "Window",
            $"009A64B0 CreateWindowExW style 0x{CreateWindowStyle:X} [engine+148]");
        Note(CreateWindowExIat, "Setup library", "Window",
            "CreateWindowExW [engine+148]");
        EngineWindowCreated = true;
        EngineForeground = true;
        Note(SetupLibrary, "Setup library", "Engine",
            $"[engine+{EnginePlus9Offset}]=1 +{EnginePlus124Offset}=0");
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
    /// <c>00403079</c> copies PE /
    /// <c>userst.ini</c>
    /// <c>[0x137545C]</c>/<c>[0x1375460]</c>
    /// then <c>009C0E50</c> clamps min 32.
    /// Title is <c>004023F0</c>
    /// <c>TEXT_GUI_WINDOW_TITLE</c>.
    /// Windowed is <c>009BF7E0</c>
    /// <c>![0x137544A]</c>.
    /// </summary>
    private void ApplyDisplayDefaults()
    {
        var width = BackBufferWidth > 0 ? BackBufferWidth : DisplayDefaultWidth;
        var height = BackBufferHeight > 0 ? BackBufferHeight : DisplayDefaultHeight;
        var bpp = BackBufferBpp > 0 ? BackBufferBpp : DisplayDefaultBpp;
        if (width < GraphicsMinDimension)
            width = GraphicsMinDimension;
        if (height < GraphicsMinDimension)
            height = GraphicsMinDimension;
        BackBufferWidth = width;
        BackBufferHeight = height;
        BackBufferBpp = bpp;
        DeviceWindowed = DisplayWindowFlag == 0;
        CreateWindowStyle = CreateWindowExStyle;
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

    private void Note(uint va, string stage, string subsystem, string action)
    {
        if (Trace.CanAdd)
            Trace.Add(va, stage, subsystem, action);
    }

    private void Note(
        uint va,
        string stage,
        string subsystem,
        [InterpolatedStringHandlerArgument("")] ref TraceMessageHandler action)
    {
        if (Trace.CanAdd)
            Trace.Add(va, stage, subsystem, action.GetFormattedText());
    }

    [InterpolatedStringHandler]
    private ref struct TraceMessageHandler
    {
        private DefaultInterpolatedStringHandler _builder;

        public TraceMessageHandler(
            int literalLength,
            int formattedCount,
            EngineLifecycle lifecycle,
            out bool shouldAppend)
        {
            shouldAppend = lifecycle.Trace.CanAdd;
            _builder = shouldAppend
                ? new DefaultInterpolatedStringHandler(literalLength, formattedCount)
                : default;
        }

        public void AppendLiteral(string value) => _builder.AppendLiteral(value);
        public void AppendFormatted<T>(T value) => _builder.AppendFormatted(value);
        public void AppendFormatted<T>(T value, string? format) =>
            _builder.AppendFormatted(value, format);
        public void AppendFormatted<T>(T value, int alignment) =>
            _builder.AppendFormatted(value, alignment);
        public void AppendFormatted<T>(T value, int alignment, string? format) =>
            _builder.AppendFormatted(value, alignment, format);
        public void AppendFormatted(string? value) => _builder.AppendFormatted(value);
        public void AppendFormatted(string? value, int alignment = 0, string? format = null) =>
            _builder.AppendFormatted(value, alignment, format);

        public string GetFormattedText() => _builder.ToStringAndClear();
    }
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
