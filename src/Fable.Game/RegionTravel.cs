using System.Globalization;
using System.Numerics;
using Fable.Core;
using Fable.Formats.Levels;
using Fable.Formats.Tng;
using Fable.Formats.Wld;

namespace Fable.Game;

/// <summary>
/// New-game start and region-exit walk.
/// Kid start is WLD region <c>StartOakVale</c> / map <c>StartOakValeWest</c>
/// at <c>NOVStartHSP</c> (QST <c>AddTestQuest("Q_NewOakValeIntro","NOVStartHSP")</c>,
/// exe <c>00DBDE4A</c> <c>StartOakVale</c> / <c>00DBDF09</c> <c>CREATURE_HERO_CHILD</c>).
/// WLD <c>Maps[0]</c> LookoutPoint is the adult overworld first map, not new-game.
/// </summary>
public static class RegionTravel
{
    public const string PlayerStartType = "HOLY_SITE_PLAYER_START";
    public const string NewGameRegion = "StartOakValeWest";
    public const string NewGameStartScript = "NOVStartHSP";
    public const string MainStartScript = "MAIN_START_POSITION";
    public const string ExitType = "REGION_EXIT_POINT";
    public const string EntranceType = "REGION_ENTRANCE_POINT";
    public const string KidCreature = "CREATURE_HERO_CHILD";
    public const string TweenCreature = "CREATURE_HERO_TRAINING";
    public const string AdultCreature = "CREATURE_HERO";
    public const string IntroCameraPrefix = "CAM_OVIF_SHOT";
    public const float IntroCameraFovDegrees = 72f;
    public const string IntroHeroIsSubjectKey = "CTCCameraPointScriptedSpline.HeroIsSubject";
    public const string IntroAxisUpKey = "CTCCameraPointScriptedSpline.CoordAxisUp";
    public const bool FirstSeenHeroIsSubject = false;
    /// <summary>
    /// <c>00DBDE40</c> first-seen after the map-ready /
    /// <c>00CB7940</c> / <c>[this+80]</c> gates: lookup
    /// <c>CREATURE_HERO_CHILD</c>, then three 60-byte
    /// watchers via <c>00CDD450</c> (push <c>0.1f</c> /
    /// 64 / 1). <c>WatchBarrels</c> callback
    /// <c>00DBE890</c> is first-seen. <c>WatchForGotGold</c>
    /// is <c>00DBE2E0</c>. <c>ManageQuestCoreMarkers</c>
    /// callback <c>00DBE4E0</c> names <c>NOVI_*</c> — later
    /// intro, not first-seen, do not follow off StartOakVale.
    /// Then <c>Q_NewOakValeIntro_PreAttack</c>, write
    /// <c>12.0f</c> at vtbl+2584, lookup <c>HerosOldHouse</c>.
    /// </summary>
    public const uint StartOakValeSetup = 0x00DBDE40;
    public const uint WatchBarrelsCtor = 0x00CDD450;
    public const uint WatchBarrelsCallback = 0x00DBE890;
    public const uint WatchForGotGoldCallback = 0x00DBE2E0;
    public const uint ManageQuestCoreMarkersCallback = 0x00DBE4E0;
    public const uint WatchBarrelsVtbl = 0x012D7A3C;
    public const uint WatchBarrelsIntervalBits = 0x3DCCCCCD;
    public const int WatchBarrelsCapacity = 64;
    public const int WatchBarrelsArg2 = 1;
    public const uint PreAttackQuest = 0x00DBE0C6;
    public const uint HerosOldHouseLookup = 0x00DBE15E;
    public const float PreAttackDuration = 12f;
    public const int PreAttackDurationVtbl = 2584;
    public const bool FirstSeenFollowsNoviLiveFather = false;
    /// <summary>
    /// Registrar <c>00CD6E27</c> binds <c>Q_NewOakValeIntro</c> to
    /// script <c>S_QNOVI</c> and factory <c>00DBEF70</c>.
    /// Factory allocs <c>0x10C</c> and calls ctor <c>00DAAC00</c>
    /// (vtbl <c>0x12D7A28</c>, context at <c>+64</c>).
    /// Slot 0 is the dtor. Slot 1 builds the <c>Main</c> watcher
    /// (<c>00CDD450</c>, callback <c>00CDD440</c>) — not a frame
    /// tick. Slot 2 <c>00DABAC0</c> registers <c>NOVI_*</c> names
    /// then <c>E8 00DBDE40</c> (only caller). Slot 3
    /// <c>00DAADD0</c> clears <c>+80</c>. No <c>E8</c> of slot 2;
    /// the script VM calls <c>[vtbl+8]</c>.
    /// </summary>
    public const string IntroQuest = "Q_NewOakValeIntro";
    public const string IntroQuestPreAttack = "Q_NewOakValeIntro_PreAttack";
    public const string IntroScriptName = "S_QNOVI";
    public const uint IntroQuestFactory = 0x00DBEF70;
    public const uint IntroQuestCtor = 0x00DAAC00;
    public const uint IntroQuestVtbl = 0x012D7A28;
    public const int IntroQuestSize = 0x10C;
    public const uint IntroQuestDtor = 0x00DBEFA0;
    public const uint IntroQuestMainWatcher = 0x00DAACE0;
    public const uint IntroQuestRun = 0x00DABAC0;
    public const uint IntroQuestReset = 0x00DAADD0;
    public const int IntroQuestRunSlot = 2;
    public const uint IntroQuestRunCallsSetup = 0x00DAC295;
    public const uint IntroMainWatcherCallback = 0x00CDD440;
    public const string IntroMainWatcherName = "Main";
    public const uint RenderFrame = 0x00B25950;
    public const int ScriptYieldVtbl = 28;
    public const int ScriptContextOffset = 64;
    public const int PreAttackGateOffset = 80;
    public const int ScriptWaitVtbl = 2584;
    public const bool FirstSeenPlus80WrittenInStartOakVale = false;
    /// <summary>
    /// After PlayMusic, <c>00CD17FD</c> increments the line
    /// index and <c>jb 00CC012E</c> back into the token
    /// walk. <c>FadeOut 0.5,0</c> is the next line and is
    /// therefore reached in the same <c>00CBFB7D</c> slice.
    /// </summary>
    public const bool FirstSeenFadeOpcodeInStartOakVale = true;
    /// <summary>
    /// <c>00CBFB7D</c> at <c>00CBFD95</c>:
    /// <c>[ebp+120]!=1</c> (00DB86B0 pushes 0,0,0 after
    /// <c>push 1</c>) takes def+60 and compares the first
    /// line to <c>FadeOut 0.5,0</c>. On match it calls
    /// context <c>vtbl+1488</c> with <c>0.5</c>
    /// (<c>0x122F59C</c>) and <c>0</c>.
    /// <c>CS_OAKVALE_INTRO_FATHER</c> first line is
    /// <c>PlayMusic MUSIC_SET_NULL</c>, so that compare
    /// misses and the call is skipped (<c>00CBFE31</c>).
    /// PlayAVI is later in the same interpreter
    /// (<c>00CCA26E</c>, prefix <c>Data\Video\</c>,
    /// <c>vtbl+1476</c>). Wake is <c>.PlayAnimation</c>.
    /// </summary>
    public const bool FirstSeenFadeSpecialCaseRuns = false;
    public const string FadeSpecialCase = "FadeOut 0.5,0";
    public const uint FadeSpecialCaseHalfConst = 0x0122F59C;
    public const float FadeSpecialCaseSeconds = 0.5f;
    public const int FadeSpecialCaseVtbl = 1488;
    public const uint PlayAviSite = 0x00CCA26E;
    public const uint PlayAviOpcode = 0x00CCA26D;
    public const uint PlayAviApply = 0x00CCA2BD;
    public const uint PlayAviJoin = 0x00CD17F8;
    public const uint PlayAviConcat = 0x0099F570;
    public const int PlayAviVtbl = 1476;
    public const uint PlayAviApplyFn = 0x0088F890;
    public const uint PlayAviSingleton = 0x0040D2A0;
    public const uint PlayAviSingletonVa = 0x013B7D4C;
    public const uint PlayAviPlayer = 0x006286F0;
    public const uint PlayAviOpen = 0x00A3B9D0;
    public const uint PlayAviRewrite = 0x0099C1E0;
    public const uint PlayAviExtXmvVa = 0x01258DE0;
    public const uint PlayAviExtWmvVa = 0x01258DEC;
    public const uint PlayAviExtAsfVa = 0x0129D1E8;
    public const int PlayAviMode = 0x1B;
    public const string PlayAviPrefix = @"Data\Video\";
    public const string PlayAviExtXmv = ".xmv";
    public const string PlayAviExtWmv = ".wmv";
    public const string PlayAviExtAsf = ".asf";
    public const string IntroPlayAvi = "dream_sequence_comp.xmv";
    public const string IntroPlayAviRewritten = "dream_sequence_comp.wmv";
    public const bool FirstSeenPlayAviDoesNotYield = true;
    public const bool FirstSeenPlayAviIsBlocking = true;
    public const bool FirstSeenPlayAviRewritesXmv = true;
    public const int PlayAviSkipEscape = 1;
    public const int PlayAviSkipSpace = 57;
    public const int PlayAviSkipReturn = 28;
    public const int PlayAviSkipF4 = 62;
    /// <summary>
    /// Present loop <c>00628B79</c>: scaled dest then
    /// leftover * <c>[0x122F59C]=0.5</c> letterbox.
    /// Blit is 2D submit <c>009DC870</c> then flush
    /// <c>009D9C80</c>.
    /// </summary>
    public const uint PlayAviBlit = 0x009DC870;
    public const uint PlayAviFlush = 0x009D9C80;
    public const uint PlayAviLetterboxHalfVa = 0x0122F59C;
    public const float PlayAviLetterboxHalf = 0.5f;
    /// <summary>
    /// <c>00A3B9D0</c> CoCreate <c>0x12AB174</c> /
    /// <c>0x12A9934</c> (FilterGraph + IGraphBuilder),
    /// alloc <c>0x180</c> renderer <c>00A3B510</c>,
    /// <c>AddFilter</c> vtbl+12, <c>RenderFile</c>
    /// vtbl+52. <c>00A3B5F0</c> copies
    /// <c>VIDEOINFOHEADER</c> biWidth / abs(biHeight)
    /// and RGB24 stride <c>((w+1)*3)&amp;~3</c>. D3D
    /// texture formats 21 / 25. <c>00A3B130</c>
    /// <c>put_CurrentPosition(0)</c> then
    /// <c>IMediaControl::Run</c> vtbl+28, retry 50.
    /// <c>DoRenderSample</c> <c>00A3BCF0</c> is
    /// <c>ret</c>; pixels are
    /// <c>IMediaSample::GetPointer</c>.
    /// </summary>
    public const uint PlayAviFilterGraphClsidVa = 0x012AB174;
    public const uint PlayAviGraphBuilderIidVa = 0x012A9934;
    public const uint PlayAviCoCreateIat = 0x01440640;
    public const uint PlayAviRendererCtor = 0x00A3B510;
    public const uint PlayAviCheckMediaType = 0x00A3B5F0;
    public const uint PlayAviRun = 0x00A3B130;
    public const uint PlayAviDoRenderSample = 0x00A3BCF0;
    public const uint PlayAviRendererVtbl = 0x0129D08C;
    public const uint PlayAviPinVtbl = 0x0129D04C;
    public const uint PlayAviMemInputVtbl = 0x0129D008;
    public const int PlayAviRendererSize = 0x180;
    public const int PlayAviAddFilterVtbl = 12;
    public const int PlayAviRenderFileVtbl = 52;
    public const int PlayAviMediaControlRunVtbl = 28;
    public const int PlayAviMediaPositionPutVtbl = 32;
    public const int PlayAviRunRetry = 50;
    public const int PlayAviEcComplete = 1;
    public const int PlayAviTextureFormatArgb = 21;
    public const int PlayAviTextureFormat555 = 25;
    public const uint PlayAviSeekZeroVa = 0x0122ED70;
    public static readonly Guid PlayAviFilterGraphClsid =
        new("e436ebb3-524f-11ce-9f53-0020af0ba770");
    public static readonly Guid PlayAviGraphBuilderIid =
        new("56a868a9-0ad4-11ce-b03a-0020af0ba770");
    public const bool FirstSeenPlayAviIsDirectShow = true;
    public const bool FirstSeenPlayAviIsMediaFoundation = false;
    public const bool FirstSeenPlayAviDraws = true;
    public const bool FirstSeenPlayAviLetterbox = true;
    public static readonly byte[] PlayAviAsfMagic =
        [0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11, 0xA6, 0xD9, 0x00, 0xAA, 0x00, 0x62, 0xCE, 0x6C];
    public const uint NoLoadUseCameraSite = 0x00CC9E6A;
    /// <summary>
    /// <c>00CC9F39</c> <c>UseCamera</c>. Empty / null
    /// name <c>jmp 00CD17FD</c>. One-arg first-seen
    /// takes name bind <c>vtbl+1648</c> then
    /// <c>[ebp-37]</c> ctor <c>00CBFD53=1</c> one
    /// <c>vtbl+28</c> and leftover <c>00CD17FD</c>.
    /// <c>NoLoadUseCamera</c> <c>00CC9E69</c> is the
    /// same yield via <c>00CC907D</c>.
    /// </summary>
    public const uint UseCameraOpcode = 0x00CC9F39;
    public const uint UseCameraNameBind = 0x00CCA1E3;
    public const uint UseCameraYield = 0x00CCA22C;
    public const uint UseCameraYieldFlagWrite = 0x00CBFD53;
    public const uint NoLoadUseCameraOpcode = 0x00CC9E69;
    public const uint NoLoadUseCameraYield = 0x00CC9F28;
    public const bool FirstSeenUseCameraYields = true;
    public const bool FirstSeenNoLoadUseCameraYields = true;
    public const bool FirstSeenPlayAvi = false;
    public const uint PlayMusicHelper = 0x00CBF7FE;
    public const uint PlayMusicInterpreter = 0x00CC8EAC;
    public const uint PlayMusicLookup = 0x009E5120;
    public const int PlayMusicVtbl = 2784;
    public const uint PlayMusicBank = 0x0143E900;
    public const uint CommandLoopContinue = 0x00CD17FD;
    public const uint CommandLoopNext = 0x00CC012E;
    public const uint FadeOutOpcode = 0x00CD0987;
    public const int FadeApplyVtbl = 1488;
    public const uint FadeApplyFn = 0x008907E0;
    public const uint FadeApplyInner = 0x00890820;
    public const uint FadeApplyForward = 0x006E7370;
    public const uint FadeStateWrite = 0x00434C00;
    public const uint FadeInterfaceVtbl = 0x01260F0C;
    public const int FadeActiveOffset = 188;
    public const int FadeLockOffset = 216;
    public const byte FadeOutRed = 0;
    public const byte FadeOutGreen = 0;
    public const byte FadeOutBlue = 0;
    public const byte FadeOutAlpha = 255;
    public const bool FirstSeenFadeOutIsBlack = true;
    /// <summary>
    /// Overlay draw <c>006496BC</c> inside
    /// <c>00648820</c>: skip when <c>[+188]==0</c>.
    /// Else RGB from <c>[+212]</c>, A =
    /// <c>004364C0</c> → <c>004348D0</c> * 255
    /// (<c>00BFEA70</c>). Record is <c>0041BEB0</c>
    /// type <c>0x22</c>, submit <c>vtbl+92</c>
    /// <c>0xC0</c>. Size pad <c>[0x125A298]=-8</c>.
    /// First-seen +201=1 so A is elapsed/0.5 then
    /// stays 1. <c>FadeIn</c> <c>vtbl+1496</c>
    /// <c>0088E4C0</c> clears +216 then
    /// <c>00434C90</c>. Live consume is a
    /// screen-space type-0x22 color quad.
    /// </summary>
    public const uint FadeOverlayDraw = 0x006496BC;
    public const uint FadeOverlayDrawFn = 0x00648820;
    public const uint FadeOverlayAlphaFn = 0x004348D0;
    public const uint FadeOverlayTick = 0x00434870;
    public const uint FadeOverlayRecord = 0x0041BEB0;
    public const uint FadeOverlayRecordType = 0x22;
    public const uint FadeOverlaySubmit = 0xC0;
    public const int FadeOverlaySubmitVtbl = 92;
    public const uint FadeOverlaySizeVa = 0x0125A298;
    public const float FadeOverlaySizePad = -8f;
    public const uint FadeInOpcode = 0x00CD0922;
    public const uint FadeInApply = 0x0088E4C0;
    public const int FadeInApplyVtbl = 1496;
    public const uint FadeInClearLock = 0x00434C90;
    public const float FadeAlphaScale = 255f;
    public const float FadeAlphaEpsilon = 0.0001f;
    public const bool FirstSeenFadeOverlayDraws = true;
    public const bool FirstSeenFadeOverlayDrawUnread = false;
    public const bool FirstSeenPlayMusicDoesNotYield = true;
    public const string IntroPlayMusic = "PlayMusic MUSIC_SET_NULL";
    public const uint TeleportOpcode = 0x00CC4678;
    public const uint TeleportApply = 0x00CC47EB;
    public const int TeleportApplyVtbl = 1892;
    /// <summary>
    /// <c>00CC47EB</c> <c>vtbl+1892</c> is
    /// <c>0089B780</c>. Marker pos
    /// <c>004AA980</c> is <c>[handle+4].vtbl+24</c>
    /// (TNG <c>CTCPhysicsStandard.Position*</c>).
    /// Yaw <c>004AAA40</c> is <c>vtbl+40</c>, default
    /// <c>[0x122DEDC]=0</c>. Apply writes
    /// <c>[thing+96].vtbl+124(pos)</c>. Same-region
    /// first-seen skips <c>0049EAF0</c>.
    /// <c>00DB86B0</c> binds actor names
    /// <c>Hero</c> / <c>Father</c> via
    /// <c>00CD3D2E</c> / <c>008ABD10</c> before
    /// <c>00CBFB7D</c>.
    /// </summary>
    public const uint TeleportApplyFn = 0x0089B780;
    public const uint TeleportMarkerPos = 0x004AA980;
    public const uint TeleportMarkerYaw = 0x004AAA40;
    public const int TeleportMarkerPosVtbl = 24;
    public const int TeleportMarkerYawVtbl = 40;
    public const int TeleportSetPosVtbl = 124;
    public const int TeleportThingPosOffset = 96;
    public const uint TeleportHandleValid = 0x004AB130;
    public const uint TeleportActorBind = 0x00CD3D2E;
    public const uint TeleportActorSlot = 0x008ABD10;
    public const uint TeleportActorMapCtor = 0x00CDBF70;
    public const string IntroHeroActor = "Hero";
    public const string IntroFatherActor = "Father";
    public const string IntroHeroTeleportMarker = "MK_OVI_ID_HERO";
    public const string IntroFatherTeleportMarker = "MK_OVI_ID_DAD";
    public const bool FirstSeenTeleportAppliesPos = true;
    /// <summary>
    /// <c>00CC47B4</c> calls <c>004AAA40</c> and
    /// <c>fstp</c>s the float into
    /// <c>0089B780</c>. That fn later
    /// <c>vtbl+1896</c> <c>0089BDF0</c>. Heading
    /// write is <c>[thing+96].vtbl+264</c> or
    /// look-at <c>00753E90</c> — both unread as
    /// a mesh rotate.
    /// </summary>
    public const uint TeleportHeadingApply = 0x0089BDF0;
    public const int TeleportHeadingVtbl = 1896;
    public const int TeleportSetYawVtbl = 264;
    public const uint TeleportLookAt = 0x00753E90;
    public const bool FirstSeenTeleportReadsYaw = true;
    public const bool FirstSeenTeleportAppliesYaw = false;
    public const bool FirstSeenTeleportChangesRegion = false;
    public const uint LookToThingOpcode = 0x00CC3B3F;
    public const uint LookToThingYield = 0x00CC3CAD;
    public const uint ActorCommandJoin = 0x00CC707C;
    public const uint IsFalseArgFn = 0x00CBEE0C;
    public const int InterpreterYieldEnableOffset = 103;
    public const uint InterpreterYieldEnableWrite = 0x00CBFC65;
    public const bool FirstSeenTeleportDoesNotYield = true;
    public const bool FirstSeenLookToThingYields = true;
    public const uint DoScriptFrameOpcode = 0x00CC7085;
    public const uint DoScriptFrameWait = 0x00CC70D5;
    public const uint DoScriptFrameAtoi = 0x0099E7F0;
    public const int DoScriptFrameDefaultCount = 1;
    public const bool FirstSeenDoScriptFrameYieldsPerCount = true;
    public const uint DoCameraPreloadingOpcode = 0x00CC86D0;
    public const uint DoCameraPreloadingApply = 0x00CC8720;
    public const int DoCameraPreloadingBeginVtbl = 1564;
    public const int DoCameraPreloadingTimedVtbl = 1560;
    public const int DoCameraPreloadingEndVtbl = 1568;
    public const uint IsTrueArgFn = 0x00CBEDBA;
    public const bool FirstSeenDoCameraPreloadingDoesNotYield = true;
    public const bool FirstSeenDoCameraPreloadingHasTrueArg = false;
    public const uint MuteSoundsOpcode = 0x00CC7258;
    public const uint MuteSoundsApply = 0x00CC72A8;
    public const uint MuteSoundsJoin = 0x00CC8464;
    public const int MuteSoundsVtbl = 2664;
    public const bool FirstSeenMuteSoundsDoesNotYield = true;
    public const bool FirstSeenMuteSoundsArgIsFalse = true;
    /// <summary>
    /// <c>00CC14B8</c> push <c>.PlayAnimation</c>. Apply
    /// <c>00CC1527</c> defaults (0,0,0,1,0) then
    /// <c>00CBEDBA</c> args 1/2/3/5 and <c>00CBEE0C</c>
    /// arg 4. Thing <c>vtbl+72</c> is
    /// <c>004C7470</c>: walk <c>[this+68]→[this+72]</c>
    /// 8-byte records and, when <c>[comp+8]==0</c>,
    /// call <c>[comp.vtbl+68](name)</c>. <c>ret 4</c>
    /// leaves the five flags on the stack.
    /// CTCAnimationComplex <c>+68</c> is
    /// <c>00686920</c> <c>mov al,1; ret 4</c> (not
    /// handled). Real play <c>0070D580</c> is not on
    /// this path. Record name+flags only.
    /// <c>00CBFD57</c> writes <c>[ebp-22]=1</c>, so
    /// <c>00CC186F</c> takes <c>00CC5691</c> one
    /// <c>vtbl+28</c> then <c>00CC0EBC</c> →
    /// <c>00CC7081</c>.
    /// </summary>
    public const uint PlayAnimationOpcode = 0x00CC14B8;
    public const uint PlayAnimationApply = 0x00CC1527;
    public const uint PlayAnimationYieldJoin = 0x00CC186F;
    public const uint PlayAnimationYieldOnce = 0x00CC5691;
    public const uint PlayAnimationLeftover = 0x00CC0EBC;
    public const uint PlayAnimationYieldAfterWrite = 0x00CBFD57;
    public const int PlayAnimationApplyVtbl = 72;
    public const uint PlayAnimationThingFn = 0x004C7470;
    public const int PlayAnimationComponentVtbl = 68;
    public const uint PlayAnimationFlagByte = 0x01375748;
    public const uint PlayAnimationFlagByteDword = 0x01010101;
    public const uint AnimationComplexVtbl = 0x012650A4;
    public const uint AnimationComplexFactory = 0x0070B3F0;
    public const uint AnimationComplexPlus68 = 0x00686920;
    public const uint AnimationComplexTypeIdFn = 0x0070B3C0;
    public const int AnimationComplexTypeId = 90;
    public const uint AnimationComplexInnerCtor = 0x0070E710;
    public const int AnimationComplexInnerSize = 0xBC;
    public const uint AnimationComplexInnerGetter = 0x0070B460;
    public const uint AnimationPlayInner = 0x0070D580;
    public const uint AnimationPlayRequest = 0x0070C050;
    public const uint AnimationComplexPostAttach = 0x0070B600;
    public const bool FirstSeenPlayAnimationYields = true;
    public const bool FirstSeenPlayAnimationAppliesPose = false;
    public const bool FirstSeenPlayAnimationCallsInnerPlay = false;
    public const string IntroWakeLoop = "CS_WAKING_UP_LOOP";
    public const string IntroWakeSteps = "CS_WAKING_UP_ON_STEPS";
    public const string IntroTired = "CS_TIRED";
    public const float IntroGamePauseAfterShot2 = 5.2f;
    /// <summary>
    /// <c>00CD1373</c> push <c>StartTimeCode</c>. Match
    /// zeros <c>[0x13B83C8]</c> then
    /// <c>jmp 00CD17FD</c>. No yield. <c>00CBF344</c>
    /// is the <c>00CBF29F</c> name walk (sets a local).
    /// </summary>
    public const uint StartTimeCodeOpcode = 0x00CD1373;
    public const uint StartTimeCodeApply = 0x00CD13C3;
    public const uint StartTimeCodeJoin = 0x00CD17FD;
    public const uint StartTimeCodeGlobal = 0x013B83C8;
    public const bool FirstSeenStartTimeCodeDoesNotYield = true;
    public const string IntroStandupCamera = "CAM_OVI_ID_STANDUP";
    /// <summary>
    /// <c>00CC88D1</c> <c>GamePause</c>. First-seen
    /// <c>1.6</c> has no <c>clock</c> arg so the
    /// default path runs: <c>0099E690</c> atof,
    /// target = seconds * <c>[0x124E640]=15</c>,
    /// one <c>vtbl+28</c>, then loop <c>vtbl+28</c>
    /// adding <c>[0x122DED8]=1</c> until
    /// counter &gt;= target. CLOCK path uses
    /// <c>009E1BC0</c> and is not first-seen.
    /// </summary>
    public const uint GamePauseOpcode = 0x00CC88D1;
    public const uint GamePauseAtoi = 0x0099E690;
    public const uint GamePauseScaleVa = 0x0124E640;
    public const float GamePauseScale = 15f;
    public const uint GamePauseIncrementVa = 0x0122DED8;
    public const float GamePauseIncrement = 1f;
    public const bool FirstSeenGamePauseHasClockArg = false;
    public const bool FirstSeenGamePauseUsesFrameDt = false;
    public const float IntroGamePauseSeconds = 1.6f;
    /// <summary>
    /// <c>00CC25FD</c> <c>.Speak</c>. Skip empty /
    /// <c>00CBEE5E</c> <c>null</c> text via
    /// <c>00CC7081</c>. Apply thing <c>vtbl+52</c>,
    /// poll <c>vtbl+104</c> with <c>vtbl+28</c> until
    /// <c>al==0</c>. Father vtbl <c>0x0127293C</c>
    /// +52 is <c>004CD1B0</c> <c>al=1</c>, +104 is
    /// <c>00661A40</c> <c>ret 4</c> (leaves al) so
    /// first-seen is one yield then continue.
    /// Apply body UNREAD — record only.
    /// </summary>
    public const uint SpeakOpcode = 0x00CC25FD;
    public const uint SpeakApply = 0x00CC27EA;
    public const uint SpeakPoll = 0x00CC2909;
    public const uint SpeakIsNull = 0x00CBEE5E;
    public const uint SpeakThingVtbl = 0x0127293C;
    public const uint SpeakApplyStub = 0x004CD1B0;
    public const uint SpeakPollStub = 0x00661A40;
    public const int SpeakApplyVtbl = 52;
    public const int SpeakPollVtbl = 104;
    public const bool FirstSeenSpeakYieldsOnce = true;
    public const string IntroFatherSpeak = "TEXT_QST_048_FATHER_INTRO_10";
    /// <summary>
    /// <c>00CC2EAA</c> <c>.InteractiveSpeak</c>.
    /// Context <c>vtbl+1456/1460/1464</c> then
    /// <c>00CBEDBA</c> on arg2. First-seen
    /// <c>FALSE</c> takes one <c>vtbl+28</c> and
    /// <c>jmp 00CC707C</c>. TRUE polls unread
    /// <c>vtbl+1472</c>.
    /// </summary>
    public const uint InteractiveSpeakOpcode = 0x00CC2EAA;
    public const uint InteractiveSpeakApply = 0x00CC2F50;
    public const int InteractiveSpeakBeginVtbl = 1456;
    public const int InteractiveSpeakBindVtbl = 1460;
    public const int InteractiveSpeakLineVtbl = 1464;
    public const int InteractiveSpeakWaitVtbl = 1472;
    public const bool FirstSeenInteractiveSpeakArgIsTrue = false;
    public const bool FirstSeenInteractiveSpeakYieldsOnce = true;
    public const string IntroFatherPrompt = "TEXT_QST_048_FATHER_INTRO_20";
    public const string IntroFatherResponse = "TEXT_QST_048_FATHER_INTRO_30";
    /// <summary>
    /// <c>00CC3165</c> <c>.DialogSpeak</c>. Skip empty
    /// actor / listener / text / <c>00CBEE5E</c>
    /// <c>null</c> via <c>00CC7081</c>. Context
    /// <c>vtbl+1456/1460/1464</c> (same slots as
    /// InteractiveSpeak). Optional leftover-session
    /// poll <c>vtbl+1472</c> → <c>008907D0</c> /
    /// <c>006E5660</c> is UNREAD as UI. Then one
    /// <c>vtbl+28</c> and <c>jmp 00CC707C</c>.
    /// </summary>
    public const uint DialogSpeakOpcode = 0x00CC3165;
    public const uint DialogSpeakApply = 0x00CC31BC;
    public const uint DialogSpeakYield = 0x00CC3310;
    public const uint DialogSpeakJoin = 0x00CC707C;
    public const uint DialogSpeakBeginFn = 0x008906C0;
    public const uint DialogSpeakBindFn = 0x00890710;
    public const uint DialogSpeakLineFn = 0x00890750;
    public const uint DialogSpeakWaitFn = 0x008907D0;
    public const uint DialogSpeakWaitBody = 0x006E5660;
    public const bool FirstSeenDialogSpeakYieldsOnce = true;
    public const string IntroFatherDialog = "TEXT_QST_048_FATHER_INTRO_60";
    public const string IntroDialogListener = "HERO";
    public const float IntroGamePauseAfterTired = 2.0f;
    /// <summary>
    /// <c>00CC0783</c> <c>.WaitTask</c>. Arg is unused.
    /// No actor → <c>00CC7081</c>. Else poll thing
    /// <c>vtbl+104</c>. Hero/player <c>0x012457FC</c>
    /// +104 is <c>006A9550</c> <c>jmp 00661A40</c>
    /// <c>ret 4</c> (leaves al). First poll leftover
    /// after token dtor is non-zero so one
    /// <c>vtbl+28</c>, then <c>00CBF7FE</c> empty
    /// path leaves al=0 and the next poll continues.
    /// Fiber <c>[0x13D2838]+5</c> is 0 while running
    /// so that abort does not fire.
    /// </summary>
    public const uint WaitTaskOpcode = 0x00CC0783;
    public const uint WaitTaskPoll = 0x00CC082C;
    public const uint WaitTaskYieldLoop = 0x00CC07E0;
    public const int WaitTaskPollVtbl = 104;
    public const uint WaitTaskHeroVtbl = 0x012457FC;
    public const uint WaitTaskHeroPoll = 0x006A9550;
    public const uint WaitTaskPollStub = 0x00661A40;
    public const uint WaitTaskFiberGlobal = 0x013D2838;
    public const bool FirstSeenWaitTaskReadsName = false;
    public const bool FirstSeenWaitTaskYieldsOnce = true;
    public const string IntroWaitTask = "FOO";
    /// <summary>
    /// <c>00CC0CB5</c> <c>.SneakTo</c>. Empty actor /
    /// marker → <c>00CC7081</c>. Speed default
    /// <c>0x3E99999A</c> (0.3). First-seen
    /// <c>0.0,FALSE,FALSE,FALSE</c> so arg2/arg3
    /// IsTrue is false: one <c>vtbl+28</c> then
    /// <c>00CC7081</c> (does not poll arrival).
    /// Thing <c>vtbl+20</c> is <c>004C72B0</c>
    /// <c>al=1; ret 4</c> — no mesh move.
    /// Mode push is 2 (WalkTo 0, RunTo 1).
    /// </summary>
    public const uint SneakToOpcode = 0x00CC0CB5;
    public const uint SneakToApply = 0x00CC0E5A;
    public const uint SneakToYieldOnce = 0x00CC0E96;
    public const int SneakToApplyVtbl = 20;
    public const uint SneakToApplyStub = 0x004C72B0;
    public const int SneakToMode = 2;
    public const float SneakToDefaultSpeed = 0.3f;
    public const uint SneakToDefaultSpeedBits = 0x3E99999A;
    public const bool FirstSeenSneakToWaitsForArrival = false;
    public const bool FirstSeenSneakToAppliesMove = false;
    public const string IntroSneakMarker = "MK_OVIF_HERO4";
    public const float IntroSneakSpeed = 0f;
    /// <summary>
    /// First-seen wait is <c>Hero.SneakTo MK_OVIF_HERO5,0.0,TRUE</c>.
    /// Arg2 IsTrue takes <c>00CC0F1A</c> thing <c>vtbl+104</c>
    /// poll. Hero +104 is <c>006A9550</c> /
    /// <c>00661A40</c> leftover busy → one
    /// <c>vtbl+28</c> then idle <c>00CC7081</c>.
    /// Apply is still <c>004C72B0</c> — no mesh move.
    /// </summary>
    public const uint SneakToWaitPoll = 0x00CC0F1A;
    public const uint SneakToWaitYield = 0x00CC0ECD;
    public const int SneakToWaitPollVtbl = 104;
    public const bool FirstSeenSneakToTruePollsArrival = true;
    public const bool FirstSeenSneakToTrueYieldsOnce = true;
    public const string IntroSneakWaitMarker = "MK_OVIF_HERO5";
    public const string IntroCutsceneLastCommand = "Hero.SneakTo MK_OVIF_HERO5,0.0,TRUE";
    /// <summary>
    /// Persist vector 1 is a second CString list at
    /// def+72. Reader is <c>00CC017C</c> inside
    /// <c>00CBFB7D</c>: <c>00CBEB7E</c> skip true and
    /// <c>[ebp-21]==0</c> then <c>0049B760</c> clear
    /// and <c>00432EE9</c> copy. <c>00CBEB7E</c> is
    /// <c>[0x143E8F4]</c> ? <c>vtbl+168</c>
    /// <c>00894440</c> : <c>vtbl+176</c>
    /// <c>00893B00</c>. <c>00CD17FD</c> end-of-list
    /// recopies <c>+60</c> only when
    /// <c>[ebp+120]==1</c>; it does not walk +72.
    /// First-seen skip is false so vector 1 does not
    /// run. Skip-key bodies UNREAD.
    /// </summary>
    public const int IntroCutsceneVector1Offset = 72;
    public const int IntroCutsceneVector1Count = 7;
    public const uint CutsceneListEnd = 0x00CD17FD;
    public const uint CutsceneVector1Copy = 0x00CC017C;
    public const uint CutsceneSkipPredicate = 0x00CBEB7E;
    public const uint CutsceneSkipGlobal = 0x0143E8F4;
    public const int CutsceneSkipVtblA = 168;
    public const int CutsceneSkipVtblB = 176;
    public const uint CutsceneSkipFnA = 0x00894440;
    public const uint CutsceneSkipFnB = 0x00893B00;
    public const bool FirstSeenCutsceneVector1AutoRuns = false;
    public const bool FirstSeenCutsceneSkipFires = false;
    /// <summary>
    /// <c>00CC15E3</c> <c>.PlayCombatAnim</c> (persist
    /// <c>PlayCombatAnimation</c>). Empty actor / name
    /// → <c>00CC7081</c>. Defaults then arg2/3 IsTrue
    /// and arg4/5 IsFalse; arg1 IsTrue is discarded;
    /// arg6 atoi is the call count (default 1). Thing
    /// <c>vtbl+76</c>: Father <c>00834760</c> / player
    /// <c>006AD9D0</c> do not read the name. Then
    /// <c>00CC186F</c> / <c>00CC5691</c> one
    /// <c>vtbl+28</c>.
    /// </summary>
    public const uint PlayCombatAnimationOpcode = 0x00CC15E3;
    public const uint PlayCombatAnimationApply = 0x00CC16FD;
    public const int PlayCombatAnimationApplyVtbl = 76;
    public const uint PlayCombatAnimationFatherFn = 0x00834760;
    public const uint PlayCombatAnimationPlayerFn = 0x006AD9D0;
    public const uint ActionPlayCombatAnimationName = 0x009035F0;
    public const bool FirstSeenPlayCombatAnimationYields = true;
    public const bool FirstSeenPlayCombatAnimationAppliesPose = false;
    public const string IntroFatherCombatAnim = "TURNING_AC90";
    /// <summary>
    /// <c>00CCC246</c> <c>Create</c>. Type, marker, and
    /// name required else <c>jmp 00CD17FD</c>. Apply
    /// context <c>vtbl+364</c> <c>008A9100</c> then
    /// <c>jmp 00CD17F8</c>. No interpreter yield.
    /// Spawn body UNREAD — record only.
    /// </summary>
    public const uint CreateOpcode = 0x00CCC246;
    public const uint CreateApply = 0x00CCC3E6;
    public const uint CreateJoin = 0x00CD17F8;
    public const int CreateApplyVtbl = 364;
    public const uint CreateApplyFn = 0x008A9100;
    public const bool FirstSeenCreateDoesNotYield = true;
    public const string IntroCreateType = "CREATURE_OAKVALE_VILLAGER_FEMALE_NORMAL_MESH";
    public const string IntroCreateMarker = "MK_OVI_ID_VS1";
    public const string IntroCreateName = "VILL1";
    /// <summary>
    /// <c>00CC083D</c> <c>.WalkTo</c>. Same parse as
    /// SneakTo. Empty actor / marker →
    /// <c>00CC7081</c>. First-seen is marker only so
    /// arg2/arg3 IsTrue is false: <c>jmp 00CC0E96</c>
    /// one <c>vtbl+28</c> then <c>00CC7081</c>. Thing
    /// <c>vtbl+20</c> is <c>004C72B0</c> stub. Mode
    /// push is 0.
    /// </summary>
    public const uint WalkToOpcode = 0x00CC083D;
    public const uint WalkToApply = 0x00CC09E2;
    public const uint WalkToYieldOnce = 0x00CC0E96;
    public const int WalkToApplyVtbl = 20;
    public const uint WalkToApplyStub = 0x004C72B0;
    public const int WalkToMode = 0;
    public const bool FirstSeenWalkToWaitsForArrival = false;
    public const bool FirstSeenWalkToAppliesMove = false;
    public const string IntroWalkMarker = "MK_OVI_ID_VW1";
    public const float IntroWalkSpeed = 0.3f;
    /// <summary>
    /// <c>00CC656B</c> <c>WaitActiveDialog</c>. If
    /// <c>[ebp-44]==edi</c> (no session)
    /// <c>jmp 00CC7081</c>. Else poll context
    /// <c>vtbl+1472</c> <c>008907D0</c> /
    /// <c>006E5660</c> until <c>al==0</c>. First-seen
    /// has a leftover InteractiveSpeak/DialogSpeak
    /// session so one <c>vtbl+28</c>. Dismiss body
    /// UNREAD.
    /// </summary>
    public const uint WaitActiveDialogOpcode = 0x00CC656B;
    public const uint WaitActiveDialogPoll = 0x00CC6612;
    public const int WaitActiveDialogPollVtbl = 1472;
    public const uint WaitActiveDialogPollFn = 0x008907D0;
    public const bool FirstSeenWaitActiveDialogYieldsOnce = true;
    /// <summary>
    /// <c>00CD0116</c> <c>Remove</c>. Empty name →
    /// <c>00CD17FD</c>. Arg <c>dead</c> is
    /// <c>vtbl+1608</c> (not first-seen). Else lookup
    /// and <c>vtbl+432</c> <c>008910D0</c> →
    /// <c>004C9B80</c> (sets <c>[+145]</c> bit 0 /
    /// <c>[+146]</c> bit 2). <c>jmp 00CC864B</c>.
    /// No yield. Teardown bodies UNREAD — record only.
    /// </summary>
    public const uint RemoveOpcode = 0x00CD0116;
    public const uint RemoveApply = 0x00CD0224;
    public const uint RemoveJoin = 0x00CC864B;
    public const int RemoveApplyVtbl = 432;
    public const uint RemoveApplyFn = 0x008910D0;
    public const uint RemoveInner = 0x004C9B80;
    public const bool FirstSeenRemoveDoesNotYield = true;
    public const string IntroRemoveName = "VILL1";
    /// <summary>
    /// <c>00CC3354</c> <c>.DialogadSpeak</c>. Empty
    /// actor / target / text → <c>00CC7081</c>. Mode
    /// arg3 random=1 / norepeat=2 / sequence=3.
    /// Table <c>00CD3187</c> hit: thing <c>vtbl+52</c>
    /// then <c>jmp 00CC707C</c>. Miss: context
    /// <c>vtbl+280/+288</c> then <c>vtbl+52</c> and
    /// <c>jmp 00CC2C6B</c> (<c>0099EAE0</c> then
    /// <c>00CC7081</c>). No <c>vtbl+28</c>. Father
    /// <c>0x0127293C</c> +52 is <c>004CD1B0</c>
    /// <c>al=1</c>. No <c>00CBEE5E</c>. Do not invent
    /// dialogue UI.
    /// </summary>
    public const uint DialogadSpeakOpcode = 0x00CC3354;
    public const uint DialogadSpeakMode = 0x00CC34C8;
    public const uint DialogadSpeakTable = 0x00CD3187;
    public const uint DialogadSpeakHitJoin = 0x00CC707C;
    public const uint DialogadSpeakMissJoin = 0x00CC2C6B;
    public const uint DialogadSpeakSkip = 0x00CC7081;
    public const int DialogadSpeakApplyVtbl = 52;
    public const uint DialogadSpeakApplyStub = 0x004CD1B0;
    public const uint DialogadSpeakThingVtbl = 0x0127293C;
    public const int DialogadSpeakContextSameVtbl = 280;
    public const int DialogadSpeakContextNameVtbl = 288;
    public const bool FirstSeenDialogadSpeakDoesNotYield = true;
    public const bool FirstSeenDialogadSpeakAppliesUi = false;
    public const string IntroFatherDialogAd = "TEXT_QST_048_FATHER_INTRO_100";
    public const string IntroDialogAdTarget = "Father";
    /// <summary>
    /// <c>00CC3F73</c> <c>.LookInDirection</c>. Empty
    /// actor / degrees → <c>00CC7081</c>. <c>0099E690</c>
    /// atof * <c>[0x1238E00]=1/360</c>. Arg1
    /// <c>00CBEE0C</c> IsFalse clears default flag 1.
    /// Apply context <c>vtbl+1896</c> <c>0089BDF0</c>
    /// then <c>jmp 00CC707C</c>. No <c>vtbl+28</c>.
    /// First-seen is <c>215</c> so flag stays 1.
    /// Heading body UNREAD — record only.
    /// </summary>
    public const uint LookInDirectionOpcode = 0x00CC3F73;
    public const uint LookInDirectionApply = 0x00CC4009;
    public const uint LookInDirectionJoin = 0x00CC707C;
    public const uint LookInDirectionSkip = 0x00CC7081;
    public const int LookInDirectionApplyVtbl = 1896;
    public const uint LookInDirectionApplyFn = 0x0089BDF0;
    public const uint LookInDirectionScaleVa = 0x01238E00;
    public const uint LookInDirectionScaleBits = 0x3B360B61;
    public const float LookInDirectionScale = 1f / 360f;
    public const bool FirstSeenLookInDirectionDoesNotYield = true;
    public const bool FirstSeenLookInDirectionAppliesHeading = false;
    public const float IntroLookInDirectionDegrees = 215f;
    public const bool FirstSeenWatchBarrelsSpawnsBeetle = false;
    public const bool FirstSeenHandsPlayerControl = false;
    public const bool FirstSeenCameraNameInExe = false;
    public const string WatchBarrelsThing = "NOVI_Barrel";
    /// <summary>
    /// Text-script camera matcher <c>00CBF29F</c> strcmp-walks
    /// <c>UseCamera</c> / <c>CameraLookAt</c> /
    /// <c>CameraLookBetween</c> / <c>CameraFOVLookBetween</c>
    /// and preloads each name via context <c>vtbl+1648</c>.
    /// Its <c>E8</c> callers are <c>00CBFE3B</c> /
    /// <c>00CC8782</c> / <c>00CD1837</c> — not
    /// <c>00DBDE40</c>. Activate is inside interpreter
    /// <c>00CBFB7D</c> at <c>00CC9F3A</c>: lookup the
    /// named TNG camera, then <c>vtbl+1656</c> (thing)
    /// or <c>vtbl+1648</c> (name). <c>.PlayAnimation</c>
    /// lives in the opcode dispatcher (<c>00CC14B9</c>);
    /// that helper <c>00CBFACA</c> has only
    /// <c>00CD0DB2</c> / <c>00CD0E2E</c>. Fade is
    /// <c>00CC4B22</c> (<c>.FadeIn</c> / <c>.FadeOut</c>).
    /// First-seen <c>S_QNOVI</c> is the native quest
    /// object, not these text opcodes.
    /// </summary>
    public const uint ScriptCameraHooks = 0x00CBF29F;
    public const uint ScriptUseCameraToken = 0x00CBF3AC;
    public const uint ScriptCameraLookAtToken = 0x00CBF3FE;
    public const uint ScriptPlayAnimationToken = 0x00CC14B9;
    public const uint ScriptFadeInOut = 0x00CC4B22;
    public const uint RegisteringScripts = 0x00CB5D80;
    public const uint QuestBaseCtor = 0x00CB8110;
    public const uint QuestBaseVtbl = 0x012C1648;
    public const uint ActionPlayAnimationName = 0x00903570;
    public const bool FirstSeenCallsUseCamera = false;
    public const bool FirstSeenCallsPlayAnimationDispatcher = false;
    public const bool FirstSeenScriptBinHasSqnovi = false;
    public const string IntroCutscene = "CS_OAKVALE_INTRO_FATHER";
    public const bool FirstSeenScriptBinHasIntroCutscene = true;
    /// <summary>
    /// Xref <c>00DB88DE</c> sits in <c>00DB86B0</c>, not
    /// the dtor <c>00DB8680</c>. <c>00DB86B0</c> looks up
    /// Hero/Father then <c>00CBFB7D("CS_OAKVALE_INTRO_FATHER")</c>.
    /// First-seen reach: <c>00DABAC0</c> registers
    /// <c>NOVI_LiveFather</c> with factory <c>00DAC2C0</c>
    /// at record <c>+16</c> (vtbl <c>0x012D8370</c>) before
    /// <c>00DBDE40</c> map-wait. StartOakValeWest TNG has
    /// <c>CREATURE_HERO_FATHER</c> / <c>NOVI_LiveFather</c>.
    /// Construct <c>004C97B0</c> → <c>00CB8960</c> →
    /// <c>00DB8520</c> → <c>00DAC2C0</c> writes vtbl
    /// <c>0x012D8388</c>. Fiber persist <c>00DB8630</c>
    /// calls <c>[+52].vtbl+4</c> = <c>00DB86B0</c>.
    /// Activate <c>004C7CF0</c> → <c>004AFB00</c> →
    /// <c>00CB88B0</c> is the same name match.
    /// Do not invent <c>00CBFB7D</c> fade/AVI/wake playback.
    /// </summary>
    public const uint IntroCutsceneStart = 0x00DB86B0;
    public const uint IntroCutsceneDtor = 0x00DB8680;
    public const uint IntroCutsceneRunner = 0x00CBFB7D;
    public const uint UseCameraActivate = 0x00CC9F3A;
    public const int UseCameraPreloadVtbl = 1648;
    public const int UseCameraActivateVtbl = 1656;
    public const uint IntroCutsceneCallbackTable = 0x012D838C;
    public const uint IntroCutsceneMicrothreadVtbl = 0x012D95B0;
    public const string LiveFatherScript = "NOVI_LiveFather";
    public const string LiveFatherCreature = "CREATURE_HERO_FATHER";
    public const uint LiveFatherFactory = 0x00DAC2C0;
    public const uint LiveFatherVtbl = 0x012D8388;
    public const uint NoviNameRecordVtbl = 0x012D8370;
    public const uint NoviNameRegister = 0x00CB8230;
    public const uint NoviNameRecordCreate = 0x00DB8520;
    public const uint ThingConstructBind = 0x004C97B0;
    public const uint ThingScriptActivate = 0x004C7CF0;
    public const uint ConstructNameBind = 0x00CB8960;
    public const bool FirstSeenStartsIntroCutscene = true;
    public const string IntroFirstSeenCamera = "CAM_OVIF_SHOT2";
    public static float WatchBarrelsInterval =>
        System.BitConverter.Int32BitsToSingle(unchecked((int)WatchBarrelsIntervalBits));

    public static string StartingRegion(WorldFile world) =>
        world.FindMap(NewGameRegion)?.ScriptName
        ?? (world.Maps.Count > 0 ? world.Maps[0].ScriptName : NewGameRegion);

    public static ThingInstance? FindPlayerStart(IEnumerable<ThingInstance> things)
    {
        var starts = things
            .Where(t => t.DefinitionType == PlayerStartType && t.PositionX is not null)
            .ToList();
        return Named(starts, NewGameStartScript)
               ?? Named(starts, "StartOakValeHSP")
               ?? Named(starts, MainStartScript)
               ?? Named(starts, "LookoutPointHSP")
               ?? starts.FirstOrDefault();
    }

    private static ThingInstance? Named(List<ThingInstance> starts, string script) =>
        starts.FirstOrDefault(t =>
            string.Equals(t.ScriptName, script, StringComparison.OrdinalIgnoreCase));

    public static Vector3 PositionOf(ThingInstance thing) =>
        new(thing.PositionX!.Value, thing.PositionY!.Value, thing.PositionZ ?? 0);

    /// <summary>
    /// <c>00B314E0</c> copies helper <c>+24</c> as up. SHOT2
    /// <c>CoordAxisUp</c> is <c>(0,0,1)</c>.
    /// </summary>
    public static Vector3 IntroCameraUp(ThingInstance thing) =>
        TryCoord(thing, IntroAxisUpKey, out var up) && up.LengthSquared() > 1e-8f
            ? Vector3.Normalize(up)
            : LandscapeFrustum.FirstSeenCameraUp;

    /// <summary>
    /// SHOT2 <c>HeroIsSubject=FALSE</c>. A TRUE subject would be
    /// follow-cam; first-seen <c>00B314E0</c> does not add a hero
    /// offset (<see cref="LandscapeFrustum.FirstSeenUsesThirdPersonView"/>).
    /// </summary>
    public static bool IntroHeroIsSubject(ThingInstance thing) =>
        thing.Properties.TryGetValue(IntroHeroIsSubjectKey, out var value)
        && value.Equals("TRUE", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// First-seen Oakvale intro view from TNG <c>CAM_OVIF_SHOT*</c> next to
    /// <c>NOVStartHSP</c>. Lowest shot number wins. Spline key 0 is used when
    /// the thing is <c>CAMERA_POINT_SCRIPTED_SPLINE</c>. FOV 0.2 on splines
    /// is not degrees; scripted cams store 72.
    /// </summary>
    public static bool TryIntroCamera(
        IEnumerable<ThingInstance> things, out Vector3 position, out Vector3 lookAt, out float fovDegrees)
    {
        position = default;
        lookAt = default;
        fovDegrees = IntroCameraFovDegrees;
        ThingInstance? best = null;
        var bestShot = int.MaxValue;
        foreach (var thing in things)
        {
            if (thing.PositionX is null || thing.ScriptName is null)
                continue;
            if (!thing.ScriptName.StartsWith(IntroCameraPrefix, StringComparison.OrdinalIgnoreCase))
                continue;
            var tail = thing.ScriptName[IntroCameraPrefix.Length..];
            var digits = new string(tail.TakeWhile(char.IsDigit).ToArray());
            if (!int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out var shot))
                continue;
            if (shot >= bestShot)
                continue;
            bestShot = shot;
            best = thing;
        }

        return best is not null && TryCameraFromThing(best, out position, out lookAt, out fovDegrees, out _);
    }

    /// <summary>
    /// <c>UseCamera</c> activate <c>00CC9F3A</c> looks up the
    /// TNG camera by exact <c>ScriptName</c>.
    /// </summary>
    public static bool TryNamedCamera(
        IEnumerable<ThingInstance> things,
        string name,
        out Vector3 position,
        out Vector3 lookAt,
        out float fovDegrees,
        out Vector3 up)
    {
        position = default;
        lookAt = default;
        fovDegrees = IntroCameraFovDegrees;
        up = LandscapeFrustum.FirstSeenCameraUp;
        foreach (var thing in things)
        {
            if (thing.PositionX is null || thing.ScriptName is null)
                continue;
            if (!thing.ScriptName.Equals(name, StringComparison.OrdinalIgnoreCase))
                continue;
            return TryCameraFromThing(thing, out position, out lookAt, out fovDegrees, out up);
        }

        return false;
    }

    public static bool TryCameraFromThing(
        ThingInstance thing,
        out Vector3 position,
        out Vector3 lookAt,
        out float fovDegrees,
        out Vector3 up)
    {
        position = PositionOf(thing);
        lookAt = default;
        fovDegrees = IntroCameraFovDegrees;
        up = IntroCameraUp(thing);
        var look = Vector3.UnitY;
        if (TryCoord(thing, "CTCCameraPointScriptedSpline.KeyCameras[0].Position", out var keyPos))
            position += keyPos;
        if (TryCoord(thing, "CTCCameraPointScriptedSpline.KeyCameras[0].LookDirection", out var keyLook) ||
            TryLook(thing, "CTCCameraPointScripted.LookDirection", out keyLook))
            look = keyLook;
        if (look.LengthSquared() < 1e-8f)
            look = Vector3.UnitY;
        look = Vector3.Normalize(look);
        lookAt = position + look * 8f;
        if (TryFloatProp(thing, "CTCCameraPointScriptedSpline.KeyCameras[0].FOV", out var turns) ||
            TryFloatProp(thing, "CTCCameraPointScriptedSpline.FOV", out turns))
            fovDegrees = turns * 360f;
        else if (thing.Properties.TryGetValue("CTCCameraPointScripted.FOV", out var fovText) &&
                 TryFloat(fovText, out var fov) && fov is >= 20f and <= 120f)
            fovDegrees = fov;
        return true;
    }

    private static bool TryLook(ThingInstance thing, string prefix, out Vector3 value)
    {
        value = default;
        return thing.Properties.TryGetValue(prefix + ".X", out var xs) &&
               thing.Properties.TryGetValue(prefix + ".Y", out var ys) &&
               thing.Properties.TryGetValue(prefix + ".Z", out var zs) &&
               TryFloat(xs, out var x) && TryFloat(ys, out var y) && TryFloat(zs, out var z) &&
               (value = new Vector3(x, y, z)).LengthSquared() > 1e-8f;
    }

    private static bool TryCoord(ThingInstance thing, string key, out Vector3 value)
    {
        value = default;
        return thing.Properties.TryGetValue(key, out var text) && TryC3d(text, out value);
    }

    private static bool TryC3d(string text, out Vector3 value)
    {
        value = default;
        var open = text.IndexOf('(');
        var close = text.LastIndexOf(')');
        if (open < 0 || close <= open)
            return false;
        var parts = text[(open + 1)..close].Split(',');
        if (parts.Length < 3)
            return false;
        if (!TryFloat(parts[0], out var x) || !TryFloat(parts[1], out var y) || !TryFloat(parts[2], out var z))
            return false;
        value = new Vector3(x, y, z);
        return true;
    }

    /// <summary>
    /// <c>006286F0</c> builds wide <c>.xmv</c> /
    /// <c>.wmv</c> and <c>0099C1E0</c> replaces the
    /// first with the second. PC TLC ships WMV.
    /// </summary>
    public static string RewritePlayAviPath(string relative)
    {
        if (relative.EndsWith(PlayAviExtXmv, StringComparison.OrdinalIgnoreCase))
            return relative[..^PlayAviExtXmv.Length] + PlayAviExtWmv;
        return relative;
    }

    /// <summary>
    /// Script prefix is <c>Data\Video\</c> from the
    /// install root (same folder as <c>Fable.exe</c>).
    /// Missing file is <c>00A3B9D0</c> fail — no play.
    /// </summary>
    public static string? ResolvePlayAviFile(GameInstall install, string relative)
    {
        var rewritten = RewritePlayAviPath(relative);
        var full = Path.Combine(install.Root, rewritten.Replace('\\', Path.DirectorySeparatorChar));
        return File.Exists(full) ? full : null;
    }

    public static bool PlayAviIsWmvPath(string relative)
    {
        var rewritten = RewritePlayAviPath(relative);
        return rewritten.EndsWith(PlayAviExtWmv, StringComparison.OrdinalIgnoreCase) ||
               rewritten.EndsWith(PlayAviExtAsf, StringComparison.OrdinalIgnoreCase);
    }

    public static bool FileHasAsfMagic(string path)
    {
        using var stream = File.OpenRead(path);
        Span<byte> header = stackalloc byte[PlayAviAsfMagic.Length];
        if (stream.Read(header) < header.Length)
            return false;
        return header.SequenceEqual(PlayAviAsfMagic);
    }

    public static bool IsPlayAviSkipScan(int dik) =>
        dik is PlayAviSkipEscape or PlayAviSkipSpace or PlayAviSkipReturn or PlayAviSkipF4;

    /// <summary>
    /// <c>00628B79</c> letterbox: fit the WMV in the
    /// viewport, offset leftover * 0.5 on each side.
    /// Returns dest in 0–1 screen UV.
    /// </summary>
    public static (float X0, float Y0, float X1, float Y1) PlayAviLetterbox(
        int videoWidth, int videoHeight, int screenWidth, int screenHeight)
    {
        var vw = (float)Math.Max(1, videoWidth);
        var vh = (float)Math.Max(1, videoHeight);
        var sw = (float)Math.Max(1, screenWidth);
        var sh = (float)Math.Max(1, screenHeight);
        var destH = sw * vh / vw;
        var destW = sw;
        if (destH > sh)
        {
            destH = sh;
            destW = sh * vw / vh;
        }

        var x0 = (sw - destW) * PlayAviLetterboxHalf / sw;
        var y0 = (sh - destH) * PlayAviLetterboxHalf / sh;
        return (x0, y0, x0 + destW / sw, y0 + destH / sh);
    }

    public static Vector3 ForwardOf(ThingInstance thing)
    {
        if (thing.Properties.TryGetValue("CTCPhysicsStandard.RHSetForwardX", out var xs) &&
            thing.Properties.TryGetValue("CTCPhysicsStandard.RHSetForwardY", out var ys) &&
            thing.Properties.TryGetValue("CTCPhysicsStandard.RHSetForwardZ", out var zs) &&
            TryFloat(xs, out var x) && TryFloat(ys, out var y) && TryFloat(zs, out var z))
        {
            var f = new Vector3(x, y, z);
            if (f.LengthSquared() > 1e-6f)
                return Vector3.Normalize(f);
        }

        return Vector3.UnitY;
    }

    public static IReadOnlyList<RegionExit> ActiveExits(IEnumerable<ThingInstance> things)
    {
        var list = new List<RegionExit>();
        foreach (var thing in things)
        {
            if (thing.DefinitionType != ExitType || thing.PositionX is null)
                continue;
            if (!IsTrue(thing, "CTCDRegionExit.Active"))
                continue;
            if (!thing.Properties.TryGetValue("CTCDRegionExit.EntranceConnectedToUID", out var uidText) ||
                !ulong.TryParse(uidText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var packed))
                continue;
            var radius = ReadFloat(thing, "CTCDRegionExit.Radius") ?? 3.5f;
            list.Add(new RegionExit(thing, RegionLink.Unpack(packed), radius, PositionOf(thing)));
        }

        return list;
    }

    public static RegionExit? HitExit(IEnumerable<RegionExit> exits, Vector3 position)
    {
        foreach (var exit in exits)
        {
            var dx = position.X - exit.Position.X;
            var dy = position.Y - exit.Position.Y;
            if (dx * dx + dy * dy <= exit.Radius * exit.Radius)
                return exit;
        }

        return null;
    }

    public static ThingInstance? FindEntrance(IEnumerable<ThingInstance> destThings, RegionLink link) =>
        link.FindEntrance(destThings);

    private static bool IsTrue(ThingInstance thing, string key) =>
        thing.Properties.TryGetValue(key, out var text) &&
        text.Equals("TRUE", StringComparison.OrdinalIgnoreCase);

    private static float? ReadFloat(ThingInstance thing, string key)
    {
        if (!thing.Properties.TryGetValue(key, out var text) || !TryFloat(text, out var value))
            return null;
        return value;
    }

    private static bool TryFloatProp(ThingInstance thing, string key, out float value)
    {
        value = 0f;
        return thing.Properties.TryGetValue(key, out var text) && TryFloat(text, out value);
    }

    private static bool TryFloat(string text, out float value) =>
        float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
}

public readonly record struct RegionExit(
    ThingInstance Thing,
    RegionLink Link,
    float Radius,
    Vector3 Position);
