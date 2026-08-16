using System.Globalization;
using System.Numerics;
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
    public const string PlayAviPrefix = @"Data\Video\";
    public const string IntroPlayAvi = "dream_sequence_comp.xmv";
    public const bool FirstSeenPlayAviDoesNotYield = true;
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
    public const bool FirstSeenFadeOverlayDrawUnread = true;
    public const bool FirstSeenPlayMusicDoesNotYield = true;
    public const string IntroPlayMusic = "PlayMusic MUSIC_SET_NULL";
    public const uint TeleportOpcode = 0x00CC4678;
    public const uint TeleportApply = 0x00CC47EB;
    public const int TeleportApplyVtbl = 1892;
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
