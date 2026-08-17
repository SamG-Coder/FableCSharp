using Fable.Game.Scripting;

namespace Fable.Game;

/// <summary>
/// Recovered <c>00CBFB7D</c> command table.
/// Status dimensions are independent: a proven
/// parse/dispatch/return is not a complete command.
/// </summary>
public static class ScriptCommandMap
{
    public const uint Runner = 0x00CBFB7D;
    public const uint LoopContinue = 0x00CD17FD;
    public const uint ActorJoin = 0x00CC707C;
    public const int CommandRuntimeOffset = ScriptBank.CommandRuntimeOffset;

    /// <summary>
    /// Dispatcher tokens in <c>0x012C1500–0x012C2C00</c>
    /// that are actual verbs (not persist keys / modes).
    /// </summary>
    public static readonly NativeCommandToken[] NativeTokens =
    [
        G("CameraFOVLookBetween", 0x012C1870),
        G("CameraLookBetween", 0x012C1888),
        G("CameraLookAt", 0x012C189C),
        G("UseCamera", 0x012C18AC),
        G("StartTimeCode", 0x012C18B8),
        G("DoCameraPreloading", 0x012C18C8),
        G("StopMusic", 0x012C18F8),
        G("PlayMusic", 0x012C1904),
        G("Play2DSound", 0x012C1910),
        G("SetLightScene", 0x012C191C),
        G("CameraShake", 0x012C192C),
        G("CameraEffect", 0x012C1938),
        G("TintScreenOut", 0x012C1948),
        G("TintScreenTo", 0x012C1968),
        G("FadeOut", 0x012C19A0),
        G("FadeIn", 0x012C19A8),
        G("StayFadedOut", 0x012C19B0),
        G("SetTime", 0x012C19C0),
        G("GameInfo", 0x012C19C8),
        G("Fullscreen", 0x012C19D4),
        G("Print", 0x012C19E4),
        G("PutInFrontOf", 0x012C19EC),
        G("ExitGame", 0x012C19FC),
        G("Remove", 0x012C1A10),
        G("RemoveEffect", 0x012C1A18),
        G("WaitForMessageCamera", 0x012C1A28),
        G("SetHeroWeapon", 0x012C1A44),
        G("PutInHeroHands", 0x012C1A54),
        G("TakeFromHero", 0x012C1A64),
        G("UseTheme", 0x012C1A7C),
        G("CrowdKill", 0x012C1AAC),
        G("CrowdLookAt", 0x012C1AB8),
        G("CrowdLookTo", 0x012C1ACC),
        G("CrowdCollide", 0x012C1AD8),
        G("CrowdCombatAnimate", 0x012C1AE8),
        G("CrowdAnimate", 0x012C1AFC),
        G("CrowdClearActions", 0x012C1B0C),
        G("CrowdMove", 0x012C1B2C),
        G("CrowdTeleport", 0x012C1B38),
        G("CrowdTeleportRipple", 0x012C1B48),
        G("CrowdRipplePosition", 0x012C1B5C),
        G("FallbackAcquire", 0x012C1B70),
        G("CrowdAcquire", 0x012C1B8C),
        G("CreditScreen", 0x012C1CD4),
        G("CrowdCreate", 0x012C1CE4),
        G("CrowdCreateMixed", 0x012C1CF0),
        G("ObjectCreate", 0x012C1D04),
        G("Create", 0x012C1D14),
        G("SmashWindows", 0x012C1D1C),
        G("RegisterScript", 0x012C1D2C),
        G("CreateNear", 0x012C1D3C),
        G("DummyEffect", 0x012C1D48),
        G("CreateEffect", 0x012C1D54),
        G("CreateLight", 0x012C1D64),
        G("WaitFlag", 0x012C1D70),
        G("CameraFOVLookBetweenPos", 0x012C1D7C),
        G("CameraPath", 0x012C1D94),
        G("CameraRotateThing", 0x012C1DA0),
        G("SetFlag", 0x012C1DB4),
        G("WaitForCamera", 0x012C1DBC),
        G("TeleportToHSP", 0x012C1DCC),
        G("PlayAVI", 0x012C1DE8),
        G("NoLoadUseCamera", 0x012C1DF8),
        G("ResetCamera", 0x012C1E08),
        G("DrawThing", 0x012C1E14),
        G("UseCameraFOVMarkerList", 0x012C1E20),
        G("CameraRig", 0x012C1E38),
        G("PutUpYourSwords", 0x012C1E44),
        G("RemoveHeroClothes", 0x012C1E54),
        G("HeroWear", 0x012C1E68),
        G("HeroTattoo", 0x012C1E74),
        G("HeroHair", 0x012C1E80),
        G("RemoveHeroWeapons", 0x012C1E8C),
        G("PlaySound", 0x012C1EAC),
        G("CacheMusic", 0x012C1EB8),
        G("EnableSounds", 0x012C1EC4),
        G("SetChestOpen", 0x012C1ED4),
        G("SetDoorOpen", 0x012C1EE4),
        G("GamePause", 0x012C1EF8),
        G("TakeObjectFromHero", 0x012C1F04),
        G("StopProgressSpinner", 0x012C1F18),
        G("StartProgressSpinner", 0x012C1F2C),
        G("DoCharacterPreload", 0x012C1F44),
        G("WaitBossFight", 0x012C1F58),
        G("GiveGold", 0x012C1F70),
        G("LiftRock", 0x012C1F7C),
        G("SetThingConscious", 0x012C1F88),
        G("TeleportThing", 0x012C1F9C),
        G("SetHomePosThing", 0x012C1FAC),
        G("SetGravityOnThing", 0x012C1FBC),
        G("PauseThing", 0x012C1FD0),
        G("CameraPreload", 0x012C1FDC),
        G("FadeThingIn", 0x012C1FEC),
        G("FadeThingOut", 0x012C1FF8),
        G("DoOneFrame", 0x012C2008),
        G("PlayObjectAnim", 0x012C2014),
        G("LookAtNothing", 0x012C2024),
        G("NoDialogCam", 0x012C2034),
        G("DebugCamera", 0x012C2040),
        G("MuteSounds", 0x012C204C),
        G("CameraPause", 0x012C2058),
        G("AnimationPause", 0x012C2064),
        G("ScriptFrame", 0x012C2074),
        G("DoScriptFrame", 0x012C2080),
        G("return", 0x012C2090),
        G("RemoveExtras", 0x012C20A0),
        G("TeleportFollowers", 0x012C20B0),
        G("ReturnFollowers", 0x012C20C4),
        G("RemoveAll", 0x012C20D4),
        G("LadyGreyIntro", 0x012C20E0),
        G("RemoveAllThings", 0x012C20F0),
        G("RegisterActor", 0x012C2100),
        G("WaitActiveDialog", 0x012C2110),
        G("GiveHero", 0x012C2124),
        G("GiveHeroHealth", 0x012C2134),
        G("GiveHeroMorality", 0x012C2144),
        G("GiveHeroExpression", 0x012C2158),
        G("AskQuestion", 0x012C21A4),
        G("HideBodies", 0x012C21B0),
        G("EnableBlackScreenSubtitles", 0x012C21BC),
        G("KeepEntityMap", 0x012C21D8),
        G("AToSkip", 0x012C21E8),
        G("Collide", 0x012C21F0),
        G("SlideTeleport", 0x012C21F8),
        E("SlideTeleport", 0x012C2208),
        E("TurnInto", 0x012C2218),
        E("Decapitate", 0x012C2224),
        E("ClearCommands", 0x012C2230),
        E("FadeCross", 0x012C2240),
        E("FadeOut", 0x012C224C),
        E("SetAlpha", 0x012C2258),
        E("Drawable", 0x012C2264),
        E("RemoveScriptedMode", 0x012C2270),
        E("AddScriptedMode", 0x012C2284),
        E("FadeIn", 0x012C2298),
        E("SetAppearanceSeed", 0x012C22A0),
        E("ResetPos", 0x012C22B4),
        E("TeleportInFrontOf", 0x012C22C0),
        E("Teleport", 0x012C22D4),
        E("Release", 0x012C22E0),
        E("AILevel", 0x012C22FC),
        E("FollowNavRoute", 0x012C2310),
        E("WaitForAnimationEvent", 0x012C2320),
        E("WaitForUnderRadius", 0x012C2338),
        E("LookInDirection", 0x012C234C),
        E("LookAt", 0x012C2360),
        E("LookAtNothing", 0x012C2368),
        E("LookToCamera", 0x012C2378),
        E("LookToThing", 0x012C2390),
        E("Collide", 0x012C23A0),
        E("Sheathe", 0x012C23D4),
        E("EntitySetMaxRunningSpeed", 0x012C23E0),
        E("EntitySetMaxWalkingSpeed", 0x012C23FC),
        E("DialogadSpeak", 0x012C2418),
        E("DialogSpeak", 0x012C2428),
        E("InteractiveSpeak", 0x012C2438),
        E("InteractiveSpeakGroup", 0x012C244C),
        E("DataSpeak", 0x012C246C),
        E("Speak", 0x012C2498),
        E("WalkUpToThing", 0x012C24A4),
        E("ModifyHealth", 0x012C24B4),
        E("HoldInHand", 0x012C24C4),
        E("FightStop", 0x012C24D0),
        E("FightWith", 0x012C24DC),
        E("Killable", 0x012C24E8),
        E("StopFollowingThing", 0x012C24F4),
        E("FollowThing", 0x012C2508),
        E("WaitPlayAnimation", 0x012C2518),
        E("PlayLoopingAnim", 0x012C252C),
        E("PlayCombatAnim", 0x012C2540),
        E("PlayAnimation", 0x012C2550),
        E("PreloadAnim", 0x012C2568),
        E("SetDrunk", 0x012C2578),
        E("SetScared", 0x012C2584),
        E("SetBound", 0x012C2590),
        E("SetPushable", 0x012C259C),
        E("SetDamageable", 0x012C25AC),
        E("SetAttackable", 0x012C25BC),
        E("SetFree", 0x012C25CC),
        E("SneakTo", 0x012C25D8),
        E("RunTo", 0x012C25E4),
        E("WalkTo", 0x012C25EC),
        E("WaitTask", 0x012C25F4),
        E("DoBossFight", 0x012C2600),
        E("SummonerAttack", 0x012C2610),
    ];

    public static readonly ScriptCommandSpec[] All =
    [
        Spec("PlayMusic", 0x00CC8EAC, 0x00CBF7FE, "track",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "lookup 009E5120 then vtbl+2784; jmp 00CD17FD; host stores track"),
        Spec("Play2DSound", 0x00CBF89E, 0x009E5120, "name",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "009E5120 + vtbl+2792; empty skip; jmp 00CD17FD; not PlayAVI"),
        Spec("PlaySound", 0x00CC8F4E, 0x00CC8FC1, "source,name[,criteria]",
            ScriptReturn.YieldAfter, CommandParity.ScriptLayer,
            "NULL arg0 vtbl+2768; else lookup + vtbl+2756/2760; yield 00CC907D"),
        Spec("FadeOut", 0x00CD0987, 0x008907E0, "seconds,param",
            ScriptReturn.CompleteNow, CommandParity.Complete,
            "vtbl+1488 pack black; 00434C00 +188"),
        Spec("FadeIn", 0x00CC4B22, 0x0088E4C0, "seconds,param",
            ScriptReturn.CompleteNow, CommandParity.Complete,
            "vtbl+1496 clear lock; falling overlay"),
        Spec("CameraPause", 0x00CC71F1, 0x00CC7241, "flag",
            ScriptReturn.CompleteNow, CommandParity.Complete,
            "IsFalse -> [ebp-37]=0; ctor 00CBFD53=1; gates UseCamera vtbl+28"),
        Spec("Teleport", 0x00CC4678, 0x0089B780, "marker[,IsFalse]",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "marker pos 004AA980; vtbl+124; no vtbl+28; yaw write unread"),
        Spec("LookToThing", 0x00CC3B3F, 0, "target[,mode][,IsFalse]",
            ScriptReturn.YieldAfterUnlessFalse, CommandParity.ScriptLayer,
            "vtbl+1992; FOREVER wait; body UNREAD — record + yield"),
        Spec("DoScriptFrame", 0x00CC7085, 0, "[count]",
            ScriptReturn.WaitFrames, CommandParity.Complete,
            "atoi; each count one vtbl+28"),
        Spec("DoCameraPreloading", 0x00CC86D0, 0x00CBF29F, "[IsTrue]",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "collects UseCamera names vtbl+1648; vtbl+1560/1568 UNREAD"),
        Spec("UseCamera", 0x00CC9F3A, 0x00B23B50, "name",
            ScriptReturn.YieldAfter, new CommandParity(
                CommandStatus.Proven, CommandStatus.Proven, CommandStatus.Proven,
                CommandStatus.Proven, CommandStatus.Partial),
            "TNG lookup; bind ScriptedCamera pos/look/fov; one vtbl+28; spline unread"),
        Spec("NoLoadUseCamera", 0x00CC9E6A, 0x00CC907D, "name",
            ScriptReturn.YieldAfter, new CommandParity(
                CommandStatus.Proven, CommandStatus.Proven, CommandStatus.Proven,
                CommandStatus.Proven, CommandStatus.Partial),
            "separate token; same TNG bind; yield helper 00CC907D"),
        Spec("WaitForCamera", 0x00CCA41F, 0x00CCA58F, "",
            ScriptReturn.YieldAfterOrWait, CommandParity.ScriptLayer,
            "poll vtbl+1672; idle -> 00CD17FD; busy -> vtbl+28 then re-poll"),
        Spec("ResetCamera", 0x00CC9DF1, 0x00CC9E40, "",
            ScriptReturn.CompleteNow, new CommandParity(
                CommandStatus.Proven, CommandStatus.Proven, CommandStatus.Proven,
                CommandStatus.Proven, CommandStatus.Partial),
            "vtbl+1668(0.0) then vtbl+1664; jmp 00CD17FD; restores gameplay snapshot"),
        Spec("CameraShake", 0x00CD131F, 0x00CD1366, "arg0,arg1",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "atof both; vtbl+1696(arg1,arg0); jmp 00CD17FD; decay unread"),
        Spec("RemoveEffect", 0x00CD0071, 0x00CD00F8, "name",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "walk extras 12-byte list; match vtbl+432(item,0,1); not Remove lookup"),
        Spec("ScriptFrame", 0x00CC7124, 0x00CC7181, "[IsFalse]",
            ScriptReturn.CompleteNow, CommandParity.Complete,
            "IsFalse -> [ebp+103]=!IsFalse yield-enable; jmp 00CC8464"),
        Spec("DoOneFrame", 0x00CC75A8, 0x00CC7605, "",
            ScriptReturn.YieldAfter, CommandParity.Complete,
            "if [ebp+103] vtbl+28; timecode; jmp 00CC8464"),
        Spec("CreateNear", 0x00CCBEE7, 0x00CCC027, "type,near,name[,radius]",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "atof arg3; 004AA980 pos; vtbl+368 factory (not 364/392); offset unread"),
        Spec("CreateEffect", 0x00CCBB9A, 0x00CCBCDA, "type,marker[,name][,z][,IsTrue]",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "00CBF9DE marker; vtbl+400; z on marker+8; jmp 00CC864B"),
        Spec("ObjectCreate", 0x00CCC4FC, 0x00CCC62E, "type,marker,name",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "vtbl+392 object factory; empty any skip; jmp 00CC864B"),
        Spec("CrowdCreate", 0x00CCC92F, 0x00CCCAA1, "type,source,alias[,IsTrue]",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "vtbl+300(source); per-item vtbl+364; alias+i via 0099F570; 00CD3D2E"),
        Spec("CrowdCreateMixed", 0x00CCC64D, 0x00CCC7A8, "typeA,typeB,source,alias",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "vtbl+300(source); rand 00BFEB16%2 picks typeA/typeB; vtbl+364 each"),
        Spec("PlayAnimation", 0x00CC14B8, 0x004C7470, "name[,flags]",
            ScriptReturn.YieldAfter, CommandParity.ScriptLayer,
            "vtbl+72; CTCAnimationComplex +68 is 00686920 al=1; inner 0070D580 not this path"),
        Spec("PlayLoopingAnim", 0x00CC1731, 0, "name[,flags]",
            ScriptReturn.YieldAfter, CommandParity.ScriptLayer,
            "separate token after PlayCombatAnim; entity task slot"),
        Spec("PlayAVI", 0x00CCA26D, 0x006286F0, "file",
            ScriptReturn.BlockPump, CommandParity.Complete,
            "Data\\Video\\ prefix; blocking 006286F0; no vtbl+28"),
        Spec("MuteSounds", 0x00CC7258, 0, "IsFalse?",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "vtbl+2664; jmp 00CC8464; apply body UNREAD"),
        Spec("StartTimeCode", 0x00CD1373, 0, "",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "and [0x13B83C8],0; leftover increment not a pose clock"),
        Spec("GamePause", 0x00CC88D1, 0, "seconds",
            ScriptReturn.WaitScaledFrames, CommandParity.Complete,
            "atof * [0x124E640]=15; CLOCK path UNREAD"),
        Spec("Speak", 0x00CC25FD, 0, "target,text[,…]",
            ScriptReturn.YieldAfter, CommandParity.ScriptLayer,
            "vtbl+52/+104 leftover poll; session recorded; no dialogue UI"),
        Spec("InteractiveSpeak", 0x00CC2EAA, 0, "listener,prompt[,wait]",
            ScriptReturn.YieldAfterUnlessWait, CommandParity.ScriptLayer,
            "vtbl+1456/1460/1464; TRUE wait vtbl+1472 UNREAD"),
        Spec("DialogSpeak", 0x00CC3165, 0, "listener,text",
            ScriptReturn.YieldAfter, CommandParity.ScriptLayer,
            "one vtbl+28; bodies UNREAD"),
        Spec("DialogadSpeak", 0x00CC3354, 0, "target,text[,mode]",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "no vtbl+28; father +52 stub; no dialogue UI"),
        Spec("WaitTask", 0x00CC0783, 0, "name",
            ScriptReturn.YieldAfter, CommandParity.ScriptLayer,
            "poll vtbl+104 leftover; entity task slot"),
        Spec("WaitActiveDialog", 0x00CC656B, 0, "",
            ScriptReturn.YieldAfter, CommandParity.ScriptLayer,
            "session poll vtbl+1472; dismiss UNREAD"),
        Spec("WaitPlayAnimation", 0x00CC2518, 0, "",
            ScriptReturn.YieldAfter, CommandParity.ScriptLayer,
            "poll current entity anim task"),
        Spec("SneakTo", 0x00CC0CB5, 0, "marker[,speed][,wait]",
            ScriptReturn.YieldAfterOrWait, CommandParity.ScriptLayer,
            "vtbl+20 stub 004C72B0; TRUE wait leftover once; dest stored"),
        Spec("WalkTo", 0x00CC083D, 0, "marker[,speed][,wait]",
            ScriptReturn.YieldAfterOrWait, CommandParity.ScriptLayer,
            "same stub; dest + entity task; nav unread"),
        Spec("RunTo", 0x00CC25E4, 0, "marker[,speed][,wait]",
            ScriptReturn.YieldAfterOrWait, CommandParity.ScriptLayer,
            "same entity task slot as WalkTo"),
        Spec("PlayCombatAnimation", 0x00CC15E3, 0, "name[,flags]",
            ScriptReturn.YieldAfter, CommandParity.ScriptLayer,
            "vtbl+76 does not read name; no TURNING_AC90 pose"),
        Spec("PlayCombatAnim", 0x00CC15E3, 0, "name[,flags]",
            ScriptReturn.YieldAfter, CommandParity.ScriptLayer,
            "exe token alias of PlayCombatAnimation"),
        Spec("Create", 0x00CCC246, 0, "type,marker,name",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "vtbl+364; spawn body UNREAD; C# inserts ThingInstance"),
        Spec("Remove", 0x00CD0116, 0x008910D0, "name[,dead|IsTrue]",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "empty skip; dead -> vtbl+1608; else vtbl+432 008910D0/004C9B80"),
        Spec("RemoveThing", 0x00CD0116, 0x008910D0, "name",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "NOT a separate token. 00BFEAF8 n=6 matches Remove. Same apply."),
        Spec("RemoveAll", 0x00CC67B5, 0x00CC6817, "IsFalse?",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "separate path; vtbl+336 collection; vtbl+2044 per item; NOT vtbl+432"),
        Spec("RemoveAllThings", 0x00CC66A7, 0x00CC6783, "name",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "separate path; empty skip; vtbl+300(LadyGreyIntro) then vtbl+432"),
        Spec("LookInDirection", 0x00CC3F73, 0x0089BDF0, "degrees[,IsFalse]",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "vtbl+1896; heading body UNREAD"),
        Spec("SetTime", 0x00CD07D6, 0x00CD082A, "hours[,flag][,duration]",
            ScriptReturn.CompleteNow, CommandParity.Complete,
            "wrap 24 * 1/24 clamp [0,1] at clock+8; vtbl+2584 0088FDC0"),
        Spec("Get", 0, 0, "source,alias",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "script.bin Get NAME,ALIAS binds acquired alias; continue"),
        Spec("FallbackAcquire", 0x00CCD344, 0x00CCD397, "alias,type[,type…]",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "vtbl+320 candidates; first matching type; jmp 00CD17FD"),
        Spec("CrowdAnimate", 0x00CCE4EC, 0x00CCE53F, "crowd,anim,_,_,_,flags…",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "00515700 crowd; per-member 007E73F0; empty skip; jmp 00CD17FD"),
        Spec("RemoveExtras", 0x00CC6ACE, 0x00CC6B21, "IsTrue,limbo|return",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "limbo/return flags; hide extras; jmp continue"),
        Spec("StopMusic", 0, 0, "",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "clears last track; continue"),
        Spec("StayFadedOut", 0, 0, "",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "runner local stay-faded"),
        Spec("EnableSounds", 0, 0, "",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "unmute; continue"),
        Spec("NoDialogCam", 0, 0, "IsTrue",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "runner local"),
        Spec("AnimationPause", 0x00CC718B, 0x00CC718B, "flag",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "IsFalse store like CameraPause; apply body unread"),
        Spec("CameraLookAt", 0x00CCA73F, 0x00CCA953, "thing,mode[,floats]",
            ScriptReturn.YieldAfter, CommandParity.ScriptLayer,
            "empty skip; vtbl+1628; yield if [ebp+103]"),
        Spec("CameraLookBetween", 0x00CCAA6C, 0x00CCADB9, "a,b,mode,dur[,offA][,offB]",
            ScriptReturn.YieldAfter, CommandParity.ScriptLayer,
            "4 required; vtbl+1632(posA+off,posB+off,dur,-1); yield if [ebp+103]"),
        Spec("CameraFOVLookBetween", 0x00CCB479, 0x00CCB728, "a,b,mode,dur[,fovDeg]",
            ScriptReturn.YieldAfter, CommandParity.ScriptLayer,
            "same vtbl+1632; arg4 degrees*1/360 or -1; yield if [ebp+103]"),
        Spec("CameraFOVLookBetweenPos", 0x00CCB07C, 0x00CCB42C, "a,b,pos,dur[,xyz/fov]",
            ScriptReturn.YieldAfter, CommandParity.ScriptLayer,
            "vtbl+1636(posA,posB,camPos+off,dur,fov); yield if [ebp+103]"),
        Spec("PutUpYourSwords", 0x00CC9303, 0, "IsFalse?",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "token 00CC9303; sheathe apply unread"),
        Spec("RegisterActor", 0x00CC662D, 0x00CC669B, "name",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "empty skip; 004AC860 register; jmp 00CC7081"),
        Spec("CrowdAcquire", 0x00CCCEA7, 0x00515700, "type,alias",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "real members only as alias0..n"),
        Spec("CrowdClearActions", 0, 0, "crowd",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "clear member entity tasks"),
        Spec("GiveHero", 0, 0, "item[,n]",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "gift list; count default 1"),
        Spec("SetDoorOpen", 0, 0, "door,IsTrue",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "door flag"),
        Spec("ClearCommands", 0, 0, "IsTrue[,…]",
            ScriptReturn.YieldAfterUnlessFalse, CommandParity.ScriptLayer,
            "cancel entity task slot; TRUE continue else vtbl+28"),
        Spec("AddScriptedMode", 0, 0, "mode",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "record mode"),
        Spec("RemoveScriptedMode", 0, 0, "mode",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "record mode"),
        Spec("EntitySetMaxWalkingSpeed", 0, 0, "speed",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "store gait max"),
        Spec("EntitySetMaxRunningSpeed", 0, 0, "speed",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "store gait max"),
        Spec("Drawable", 0, 0, "IsFalse?",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "entity drawable flag"),
        Spec("Collide", 0, 0, "IsFalse?",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "entity collide flag"),
        Spec("SetAlpha", 0, 0, "alpha",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "entity alpha"),
        Spec("LookAt", 0, 0, "target",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "record look"),
        Spec("LookAtNothing", 0, 0, "",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "clear look"),
        Spec("PutInFrontOf", 0x00CD029F, 0x00CD0501, "mover,face,distance",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "same dest as WalkUpToThing; vtbl+1892 teleport; vtbl+1900 look; jmp 00CC864B"),
        Spec("WalkUpToThing", 0x00CC2331, 0x00CC2538, "thing,distance[,…]",
            ScriptReturn.YieldAfterOrWait, CommandParity.ScriptLayer,
            "dest=pos+atof(arg1)*(vtbl+288+12); actor vtbl+16 speed 1; leftover vtbl+104"),
        Spec("FollowThing", 0x00CC19F2, 0x00CC1AE9, "target[,speed]",
            ScriptReturn.YieldAfter, CommandParity.ScriptLayer,
            "default speed 1.0; actor vtbl+28; yield 00CC0E96 if [ebp+103]"),
        Spec("StopFollowingThing", 0x00CC1B2F, 0x00CC1BF4, "[target]",
            ScriptReturn.YieldAfter, CommandParity.ScriptLayer,
            "actor vtbl+32; jmp 00CC568C leftover"),
        Spec("SetFlag", 0x00CCA475, 0x00CCA4C8, "name,IsFalse?[,IsTrue skip]",
            ScriptReturn.YieldAfter, CommandParity.Complete,
            "008ADF10 write 0/1; [ebp-39] latch; jmp 00CC907D"),
        Spec("WaitFlag", 0x00CCB840, 0x00CCB893, "name,IsTrue?",
            ScriptReturn.YieldAfterOrWait, CommandParity.Complete,
            "008ADF10 cmp [eax],bl; match 00CD17FD; else leftover 00CCB8CE"),
    ];

    public static ScriptCommandSpec? Find(string verb)
    {
        foreach (var spec in All)
        {
            if (spec.Verb.Equals(verb, StringComparison.OrdinalIgnoreCase))
                return spec;
        }

        return null;
    }

    public static CommandStatus StatusOf(string verb) =>
        Find(verb)?.Status ?? CommandStatus.Unread;

    public static bool IsImplementedComplete(string verb) =>
        Find(verb)?.Parity.IsComplete == true;

    public static string FormatMarkdown()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("| Verb | Token | Apply | Args | Return | Parse | Dispatch | ReturnSt | ApplySt | Runtime | Overall | Evidence |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|---|");
        foreach (var spec in All)
        {
            sb.Append("| ").Append(spec.Verb);
            sb.Append(" | `").Append(spec.TokenSite == 0 ? "—" : spec.TokenSite.ToString("X8"));
            sb.Append("` | `").Append(spec.ApplySite == 0 ? "—" : spec.ApplySite.ToString("X8"));
            sb.Append("` | ").Append(spec.Arguments);
            sb.Append(" | ").Append(spec.Return);
            sb.Append(" | ").Append(spec.Parse);
            sb.Append(" | ").Append(spec.Dispatch);
            sb.Append(" | ").Append(spec.ReturnStatus);
            sb.Append(" | ").Append(spec.Apply);
            sb.Append(" | ").Append(spec.Runtime);
            sb.Append(" | ").Append(spec.Status);
            sb.Append(" | ").Append(spec.Evidence);
            sb.AppendLine(" |");
        }

        return sb.ToString();
    }

    public static string FormatCoverage()
    {
        var recovered = All.ToDictionary(s => s.Verb, s => s, StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var total = NativeTokens.Length;
        var dispatch = 0;
        var ret = 0;
        var apply = 0;
        var runtime = 0;
        var unread = 0;
        var global = 0;
        var entity = 0;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# Native command coverage");
        sb.AppendLine();
        sb.AppendLine("Generated from exe token list `0x012C1500–0x012C2C00` + `ScriptCommandMap`.");
        sb.AppendLine();
        sb.AppendLine("| Token | Family | Parse | Dispatch | Return | Apply | Runtime | Overall |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|");
        foreach (var token in NativeTokens)
        {
            seen.Add(token.Name);
            if (token.Family == CommandFamily.Global)
                global++;
            else
                entity++;
            if (!recovered.TryGetValue(token.Name, out var spec))
            {
                unread++;
                sb.Append("| ").Append(token.Name);
                sb.Append(" | ").Append(token.Family);
                sb.AppendLine(" | Unread | Unread | Unread | Unread | Unread | Unread |");
                continue;
            }

            if (spec.Dispatch == CommandStatus.Proven)
                dispatch++;
            if (spec.ReturnStatus == CommandStatus.Proven)
                ret++;
            if (spec.Apply == CommandStatus.Proven)
                apply++;
            if (spec.Runtime == CommandStatus.Proven)
                runtime++;
            if (spec.Status == CommandStatus.Unread)
                unread++;
            sb.Append("| ").Append(token.Name);
            sb.Append(" | ").Append(token.Family);
            sb.Append(" | ").Append(spec.Parse);
            sb.Append(" | ").Append(spec.Dispatch);
            sb.Append(" | ").Append(spec.ReturnStatus);
            sb.Append(" | ").Append(spec.Apply);
            sb.Append(" | ").Append(spec.Runtime);
            sb.Append(" | ").Append(spec.Status);
            sb.AppendLine(" |");
        }

        foreach (var spec in All)
        {
            if (seen.Contains(spec.Verb))
                continue;
            sb.Append("| ").Append(spec.Verb);
            sb.Append(" | script.bin");
            sb.Append(" | ").Append(spec.Parse);
            sb.Append(" | ").Append(spec.Dispatch);
            sb.Append(" | ").Append(spec.ReturnStatus);
            sb.Append(" | ").Append(spec.Apply);
            sb.Append(" | ").Append(spec.Runtime);
            sb.Append(" | ").Append(spec.Status);
            sb.AppendLine(" |");
        }

        sb.Insert(0,
            $"""
            TOTAL NATIVE COMMAND TOKENS: {total}
            GLOBAL: {global}
            ENTITY: {entity}
            RECOVERED DISPATCH: {dispatch}
            RECOVERED RETURN: {ret}
            RECOVERED APPLY: {apply}
            IMPLEMENTED RUNTIME: {runtime}
            UNREAD: {unread}

            """);
        return sb.ToString();
    }

    private static ScriptCommandSpec Spec(
        string verb, uint token, uint apply, string args,
        ScriptReturn ret, CommandParity parity, string evidence) =>
        new(verb, token, apply, args, ret, parity.Overall, evidence, parity);

    private static NativeCommandToken G(string name, uint va) =>
        new(name, va, CommandFamily.Global);

    private static NativeCommandToken E(string name, uint va) =>
        new(name, va, CommandFamily.Entity);
}

public readonly record struct NativeCommandToken(
    string Name,
    uint StringVa,
    CommandFamily Family);

public readonly record struct ScriptCommandSpec(
    string Verb,
    uint TokenSite,
    uint ApplySite,
    string Arguments,
    ScriptReturn Return,
    CommandStatus Status,
    string Evidence,
    CommandParity Parity)
{
    public CommandStatus Parse => Parity.Parse;
    public CommandStatus Dispatch => Parity.Dispatch;
    public CommandStatus ReturnStatus => Parity.Return;
    public CommandStatus Apply => Parity.Apply;
    public CommandStatus Runtime => Parity.Runtime;
}

public readonly record struct CommandParity(
    CommandStatus Parse,
    CommandStatus Dispatch,
    CommandStatus Return,
    CommandStatus Apply,
    CommandStatus Runtime)
{
    public static readonly CommandParity Complete =
        new(CommandStatus.Proven, CommandStatus.Proven, CommandStatus.Proven,
            CommandStatus.Proven, CommandStatus.Proven);

    public static readonly CommandParity ScriptLayer =
        new(CommandStatus.Proven, CommandStatus.Proven, CommandStatus.Proven,
            CommandStatus.Partial, CommandStatus.Partial);

    public static readonly CommandParity UnreadAll =
        new(CommandStatus.Unread, CommandStatus.Unread, CommandStatus.Unread,
            CommandStatus.Unread, CommandStatus.Unread);

    public CommandStatus Overall
    {
        get
        {
            if (Dispatch == CommandStatus.Unread && Parse == CommandStatus.Unread)
                return CommandStatus.Unread;
            if (Parse == CommandStatus.Proven &&
                Dispatch == CommandStatus.Proven &&
                Return == CommandStatus.Proven &&
                Apply == CommandStatus.Proven &&
                Runtime == CommandStatus.Proven)
                return CommandStatus.Proven;
            return CommandStatus.Partial;
        }
    }

    public bool IsComplete => Overall == CommandStatus.Proven;
}

public enum CommandStatus
{
    Proven,
    Partial,
    Unread,
}

public enum ScriptReturn
{
    CompleteNow,
    YieldAfter,
    YieldAfterUnlessFalse,
    YieldAfterUnlessWait,
    YieldAfterOrWait,
    WaitFrames,
    WaitScaledFrames,
    BlockPump,
    Unread,
}
