namespace Fable.Game;

/// <summary>
/// Recovered <c>00CBFB7D</c> command table. Status is the
/// side-effect contract, not “the verb token exists”.
/// UNREAD verbs stay unread — they are not complete no-ops.
/// </summary>
public static class ScriptCommandMap
{
    public const uint Runner = 0x00CBFB7D;
    public const uint LoopContinue = 0x00CD17FD;
    public const uint ActorJoin = 0x00CC707C;
    public const int CommandRuntimeOffset = ScriptBank.CommandRuntimeOffset;

    public static readonly ScriptCommandSpec[] All =
    [
        Spec("PlayMusic", 0x00CC8EAC, 0x00CBF7FE, "track",
            ScriptReturn.CompleteNow, CommandStatus.Proven,
            "lookup 009E5120 then vtbl+2784; jmp 00CD17FD; host stores track"),
        Spec("FadeOut", 0x00CD0987, 0x008907E0, "seconds,param",
            ScriptReturn.CompleteNow, CommandStatus.Proven,
            "vtbl+1488 pack black; 00434C00 +188"),
        Spec("FadeIn", 0x00CC4B22, 0x0088E4C0, "seconds,param",
            ScriptReturn.CompleteNow, CommandStatus.Proven,
            "vtbl+1496 clear lock; falling overlay"),
        Spec("CameraPause", 0x00CC71F1, 0x00CC7241, "flag",
            ScriptReturn.CompleteNow, CommandStatus.Proven,
            "IsFalse -> [ebp-37]=0; ctor 00CBFD53=1; gates UseCamera vtbl+28"),
        Spec("Teleport", 0x00CC4678, 0x0089B780, "marker[,IsFalse]",
            ScriptReturn.CompleteNow, CommandStatus.Proven,
            "marker pos 004AA980; vtbl+124; no vtbl+28; yaw write unread"),
        Spec("LookToThing", 0x00CC3B3F, 0, "target[,mode][,IsFalse]",
            ScriptReturn.YieldAfterUnlessFalse, CommandStatus.Proven,
            "vtbl+1992; FOREVER wait; body UNREAD — record + yield"),
        Spec("DoScriptFrame", 0x00CC7085, 0, "[count]",
            ScriptReturn.WaitFrames, CommandStatus.Proven,
            "atoi; each count one vtbl+28"),
        Spec("DoCameraPreloading", 0x00CC86D0, 0x00CBF29F, "[IsTrue]",
            ScriptReturn.CompleteNow, CommandStatus.Proven,
            "collects UseCamera names vtbl+1648; vtbl+1560/1568 UNREAD"),
        Spec("UseCamera", 0x00CC9F3A, 0x00B23B50, "name",
            ScriptReturn.YieldAfter, CommandStatus.Proven,
            "TNG lookup; bind helper; one vtbl+28"),
        Spec("NoLoadUseCamera", 0x00CC9E6A, 0x00CC907D, "name",
            ScriptReturn.YieldAfter, CommandStatus.Proven,
            "separate token; yield helper 00CC907D"),
        Spec("PlayAnimation", 0x00CC14B8, 0x004C7470, "name[,flags]",
            ScriptReturn.YieldAfter, CommandStatus.Proven,
            "vtbl+72; CTCAnimationComplex +68 is 00686920 al=1; inner 0070D580 not this path"),
        Spec("PlayAVI", 0x00CCA26D, 0x006286F0, "file",
            ScriptReturn.BlockPump, CommandStatus.Proven,
            "Data\\Video\\ prefix; blocking 006286F0; no vtbl+28"),
        Spec("MuteSounds", 0x00CC7258, 0, "IsFalse?",
            ScriptReturn.CompleteNow, CommandStatus.Proven,
            "vtbl+2664; jmp 00CC8464; apply body UNREAD"),
        Spec("StartTimeCode", 0x00CD1373, 0, "",
            ScriptReturn.CompleteNow, CommandStatus.Proven,
            "and [0x13B83C8],0; leftover increment not a pose clock"),
        Spec("GamePause", 0x00CC88D1, 0, "seconds",
            ScriptReturn.WaitScaledFrames, CommandStatus.Proven,
            "atof * [0x124E640]=15; CLOCK path UNREAD"),
        Spec("Speak", 0x00CC25FD, 0, "target,text[,…]",
            ScriptReturn.YieldAfter, CommandStatus.Proven,
            "vtbl+52/+104 leftover poll; no dialogue UI"),
        Spec("InteractiveSpeak", 0x00CC2EAA, 0, "listener,prompt[,wait]",
            ScriptReturn.YieldAfterUnlessWait, CommandStatus.Proven,
            "vtbl+1456/1460/1464; TRUE wait vtbl+1472 UNREAD"),
        Spec("DialogSpeak", 0x00CC3165, 0, "listener,text",
            ScriptReturn.YieldAfter, CommandStatus.Proven,
            "one vtbl+28; bodies UNREAD"),
        Spec("DialogadSpeak", 0x00CC3354, 0, "target,text[,mode]",
            ScriptReturn.CompleteNow, CommandStatus.Proven,
            "no vtbl+28; father +52 stub; no dialogue UI"),
        Spec("WaitTask", 0x00CC0783, 0, "name",
            ScriptReturn.YieldAfter, CommandStatus.Proven,
            "poll vtbl+104 leftover; no task table"),
        Spec("WaitActiveDialog", 0x00CC656B, 0, "",
            ScriptReturn.YieldAfter, CommandStatus.Proven,
            "session poll vtbl+1472; dismiss UNREAD"),
        Spec("SneakTo", 0x00CC0CB5, 0, "marker[,speed][,wait]",
            ScriptReturn.YieldAfterOrWait, CommandStatus.Proven,
            "vtbl+20 stub 004C72B0; TRUE wait leftover once; no mesh move"),
        Spec("WalkTo", 0x00CC083D, 0, "marker[,speed][,wait]",
            ScriptReturn.YieldAfterOrWait, CommandStatus.Proven,
            "same stub; first-seen does not wait"),
        Spec("PlayCombatAnimation", 0x00CC15E3, 0, "name[,flags]",
            ScriptReturn.YieldAfter, CommandStatus.Proven,
            "vtbl+76 does not read name; no TURNING_AC90 pose"),
        Spec("PlayCombatAnim", 0x00CC15E3, 0, "name[,flags]",
            ScriptReturn.YieldAfter, CommandStatus.Proven,
            "exe token alias of PlayCombatAnimation"),
        Spec("Create", 0x00CCC246, 0, "type,marker,name",
            ScriptReturn.CompleteNow, CommandStatus.Proven,
            "vtbl+364; spawn body UNREAD"),
        Spec("Remove", 0x00CD0116, 0, "name",
            ScriptReturn.CompleteNow, CommandStatus.Proven,
            "vtbl+432; teardown UNREAD"),
        Spec("LookInDirection", 0x00CC3F73, 0x0089BDF0, "degrees[,IsFalse]",
            ScriptReturn.CompleteNow, CommandStatus.Proven,
            "vtbl+1896; heading body UNREAD"),
        Spec("SetTime", 0x00CD07D6, 0x00CD082A, "hours[,flag][,duration]",
            ScriptReturn.CompleteNow, CommandStatus.Proven,
            "wrap 24 * 1/24 clamp [0,1] at clock+8; vtbl+2584 0088FDC0"),
        Spec("RemoveThing", 0, 0, "name",
            ScriptReturn.Unread, CommandStatus.Unread,
            "script.bin token; not in exe 012C1500-012C2C00 dispatcher strings"),
        Spec("Get", 0, 0, "source,alias",
            ScriptReturn.CompleteNow, CommandStatus.Proven,
            "script.bin Get NAME,ALIAS binds acquired alias; continue"),
        Spec("FallbackAcquire", 0x00CCD344, 0x00CCD397, "alias,type[,type…]",
            ScriptReturn.CompleteNow, CommandStatus.Proven,
            "vtbl+320 candidates; first matching type; jmp 00CD17FD"),
        Spec("CrowdAnimate", 0x00CCE4EC, 0x00CCE53F, "crowd,anim,_,_,_,flags…",
            ScriptReturn.CompleteNow, CommandStatus.Proven,
            "00515700 crowd; per-member 007E73F0; empty skip; jmp 00CD17FD"),
        Spec("RemoveExtras", 0x00CC6ACE, 0x00CC6B21, "IsTrue,limbo|return",
            ScriptReturn.CompleteNow, CommandStatus.Proven,
            "limbo/return flags; hide extras; jmp continue"),
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
        StatusOf(verb) == CommandStatus.Proven;

    public static string FormatMarkdown()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("| Verb | Token | Apply | Args | Return | Status | Evidence |");
        sb.AppendLine("|---|---|---|---|---|---|---|");
        foreach (var spec in All)
        {
            sb.Append("| ").Append(spec.Verb);
            sb.Append(" | `").Append(spec.TokenSite == 0 ? "—" : spec.TokenSite.ToString("X8"));
            sb.Append("` | `").Append(spec.ApplySite == 0 ? "—" : spec.ApplySite.ToString("X8"));
            sb.Append("` | ").Append(spec.Arguments);
            sb.Append(" | ").Append(spec.Return);
            sb.Append(" | ").Append(spec.Status);
            sb.Append(" | ").Append(spec.Evidence);
            sb.AppendLine(" |");
        }

        return sb.ToString();
    }

    private static ScriptCommandSpec Spec(
        string verb, uint token, uint apply, string args,
        ScriptReturn ret, CommandStatus status, string evidence) =>
        new(verb, token, apply, args, ret, status, evidence);
}

public readonly record struct ScriptCommandSpec(
    string Verb,
    uint TokenSite,
    uint ApplySite,
    string Arguments,
    ScriptReturn Return,
    CommandStatus Status,
    string Evidence);

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
