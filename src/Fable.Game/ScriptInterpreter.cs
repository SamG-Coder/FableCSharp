using System.Globalization;
using Fable.Game.Scripting;

namespace Fable.Game;

/// <summary>
/// <c>00CBFB7D</c> fetch / dispatch / PC loop over the
/// def+60 CString vector. Handlers return results;
/// this loop does not consult <see cref="ScriptCommand.Classify"/>.
/// </summary>
public sealed class ScriptInterpreter
{
    public string Name => State.Name;
    public IReadOnlyList<string> Commands
    {
        get => State.Commands;
        private set => State.Commands = value;
    }

    public int InstructionPointer
    {
        get => State.Pc;
        private set => State.Pc = value;
    }

    public bool Yielded
    {
        get => State.Yielded;
        private set => State.Yielded = value;
    }

    public bool Finished
    {
        get => State.Finished;
        private set => State.Finished = value;
    }

    public bool SkipListApplied
    {
        get => State.SkipListApplied;
        private set => State.SkipListApplied = value;
    }

    public bool FadeSpecialCaseApplied
    {
        get => State.FadeSpecialCaseApplied;
        private set => State.FadeSpecialCaseApplied = value;
    }

    public string? UnsupportedCommand { get; private set; }
    public int ScriptFrameRemaining
    {
        get => State.ScriptFrameRemaining;
        private set => State.ScriptFrameRemaining = value;
    }

    public float GamePauseTarget
    {
        get => State.GamePauseTarget;
        private set => State.GamePauseTarget = value;
    }

    public float GamePauseCounter
    {
        get => State.GamePauseCounter;
        private set => State.GamePauseCounter = value;
    }

    public IReadOnlyList<string> Executed => State.Executed;
    public bool Blocked => State.Blocked;
    public string? BlockReason => State.BlockReason;
    public bool CameraPauseEnabled => State.CameraPauseEnabled;
    public ExecutionKind CurrentWaitKind => State.WaitKind;
    internal CutsceneState State { get; }

    public ScriptInterpreter(string name, IReadOnlyList<string> commands)
    {
        State = new CutsceneState(name, commands);
    }

    /// <summary>
    /// Persist <c>+84/+96</c> tables for SetLightScene.
    /// </summary>
    public void BindLightTables(IReadOnlyList<string> defs, IReadOnlyList<string> scenes)
    {
        State.LightDefs = defs;
        State.LightScenes = scenes;
    }

    public void SetTintHold(float hold) => State.TintHold = hold;

    public float TintHold => State.TintHold;

    public void RunUntilYield(IScriptHost? host = null)
    {
        var runtime = host as ScriptRuntime ?? ScriptRuntime.Detached();
        RunUntilYield(runtime.BindInterpreter(this));
    }

    public void RunUntilYield(ScriptExecutionContext ctx)
    {
        if (Finished || Yielded || Blocked)
            return;
        if (InstructionPointer == 0 && !FadeSpecialCaseApplied)
            TryFadeSpecialCase(ctx.Runtime);

        while (InstructionPointer < Commands.Count)
        {
            if (State.WaitKind is ExecutionKind.WaitFrames
                or ExecutionKind.WaitScaledFrames
                or ExecutionKind.BlockPump
                or ExecutionKind.WaitOperation)
            {
                if (!TickWait(ctx))
                {
                    Yielded = true;
                    return;
                }

                var done = Commands[InstructionPointer];
                if (!State.Executed.Contains(done))
                    State.Executed.Add(done);
                State.WaitKind = ExecutionKind.Continue;
                State.WaitOperationId = null;
                InstructionPointer++;
                continue;
            }

            var raw = Commands[InstructionPointer];
            var line = ScriptLine.Parse(raw);
            var resolved = ctx.Arguments.Substitute(line, out var unresolved);
            if (unresolved is not null)
            {
                EnterBlocked("UNRESOLVED ARG", raw);
                Record(ctx, resolved, CommandResult.Blocked(
                    "UNRESOLVED ARG", CommandStatus.Unread, resolved.Family, raw));
                return;
            }

            var result = resolved.Family == CommandFamily.Entity
                ? EntityDispatcher.Dispatch(resolved, ctx)
                : GlobalDispatcher.Dispatch(resolved, ctx);

            if (result.Kind == ExecutionKind.Blocked)
            {
                EnterBlocked(result.YieldReason, raw);
                Record(ctx, resolved, result);
                return;
            }

            if (result.Kind == ExecutionKind.Continue)
            {
                if (!State.Executed.Contains(raw))
                    State.Executed.Add(raw);
                if (result.AdvancePc)
                    InstructionPointer++;
                Record(ctx, resolved, result);
                continue;
            }

            State.WaitKind = result.Kind;
            State.WaitOperationId = result.OperationId;
            State.YieldReason = result.YieldReason;
            State.ResumeReason = result.ResumeReason;
            Yielded = true;
            if (result.Kind is ExecutionKind.YieldOnce or ExecutionKind.BlockPump
                or ExecutionKind.WaitOperation)
            {
                if (!State.Executed.Contains(raw))
                    State.Executed.Add(raw);
                if (result.Kind == ExecutionKind.YieldOnce && result.AdvancePc)
                    InstructionPointer++;
            }

            Record(ctx, resolved, result);
            return;
        }

        Finished = true;
    }

    public CommandResult EvaluateOne(ScriptRuntime runtime)
    {
        var ctx = runtime.BindInterpreter(this);
        var line = ScriptLine.Parse(Commands[InstructionPointer]);
        var resolved = ctx.Arguments.Substitute(line, out var unresolved);
        if (unresolved is not null)
            return CommandResult.Blocked("UNRESOLVED ARG", CommandStatus.Unread, resolved.Family, line.Raw);
        return resolved.Family == CommandFamily.Entity
            ? EntityDispatcher.Dispatch(resolved, ctx)
            : GlobalDispatcher.Dispatch(resolved, ctx);
    }

    /// <summary>
    /// <c>00A44660</c> resume: continue after <c>vtbl+28</c>.
    /// Blocked / unread waits stay on the same PC.
    /// </summary>
    public void Resume(IScriptHost? host = null)
    {
        if (Finished || Blocked)
            return;
        Yielded = false;
        UnsupportedCommand = null;
        if (State.WaitKind == ExecutionKind.YieldOnce)
            State.WaitKind = ExecutionKind.Continue;
        RunUntilYield(host);
    }

    internal void Resume(ScriptExecutionContext ctx)
    {
        if (Finished || Blocked)
            return;
        Yielded = false;
        UnsupportedCommand = null;
        if (State.WaitKind == ExecutionKind.YieldOnce)
            State.WaitKind = ExecutionKind.Continue;
        RunUntilYield(ctx);
    }

    /// <summary>
    /// <c>00CC017C</c>: when <c>00CBEB7E</c> is true
    /// and <c>[ebp-21]==0</c>, clear the working list
    /// and copy def+72. First-seen skip is false so
    /// New Game does not call this.
    /// </summary>
    public void ApplySkipList(IReadOnlyList<string> lines)
    {
        if (SkipListApplied)
            return;
        Commands = lines;
        InstructionPointer = 0;
        Yielded = false;
        Finished = false;
        UnsupportedCommand = null;
        SkipListApplied = true;
        ScriptFrameRemaining = 0;
        GamePauseTarget = 0f;
        GamePauseCounter = 0f;
        State.GamePausePhase = 0;
        State.WaitKind = ExecutionKind.Continue;
    }

    public bool ExecutedVerb(string verb) =>
        State.Executed.Any(line =>
            ScriptCommand.Parse(line).Verb.Equals(verb, StringComparison.OrdinalIgnoreCase));

    private void TryFadeSpecialCase(ScriptRuntime runtime)
    {
        if (Commands.Count == 0)
            return;
        if (!Commands[0].Equals(RegionTravel.FadeSpecialCase, StringComparison.Ordinal))
            return;
        FadeSpecialCaseApplied = true;
        runtime.ApplyFadeOut(RegionTravel.FadeSpecialCaseSeconds, 0f);
    }

    private void EnterBlocked(string reason, string raw)
    {
        State.Blocked = true;
        State.BlockReason = reason;
        UnsupportedCommand = raw;
        Yielded = true;
    }

    private bool TickWait(ScriptExecutionContext ctx)
    {
        switch (State.WaitKind)
        {
            case ExecutionKind.WaitFrames:
                return TickScriptFrame("");
            case ExecutionKind.WaitScaledFrames:
                return TickGamePause("");
            case ExecutionKind.BlockPump:
                if (ctx.Runtime.AviPlaying)
                    return false;
                State.AviAt = -1;
                return true;
            case ExecutionKind.WaitOperation:
                return OperationComplete(ctx);
            default:
                return true;
        }
    }

    private static bool OperationComplete(ScriptExecutionContext ctx)
    {
        var id = ctx.Cutscene.WaitOperationId;
        if (id is null)
            return true;
        if (ctx.Dialogue.WaitOp is { } dialog && dialog.Id == id)
            return dialog.Complete;
        if (ctx.Camera.WaitOp is { } cam && cam.Id == id)
            return cam.Complete;
        if (ctx.Camera.MessageWaitOp is { } msg && msg.Id == id)
            return msg.Complete;
        if (ctx.Flags.WaitOp is { } flag && flag.Id == id)
            return ctx.Flags.Poll();
        foreach (var op in ctx.Animation.ByActor.Values)
        {
            if (op.Id == id)
                return op.Complete;
        }

        foreach (var op in ctx.Movement.ByActor.Values)
        {
            if (op.Id == id)
                return op.Complete;
        }

        return false;
    }

    /// <summary>
    /// <c>00CC70D5</c>: default count 1, <c>0099E7F0</c>
    /// atoi, <c>esi&lt;=0</c> skips. Each loop iteration
    /// with <c>[ebp+103]=1</c> is one <c>vtbl+28</c>.
    /// </summary>
    private bool TickScriptFrame(string arguments)
    {
        if (ScriptFrameRemaining == 0)
        {
            ScriptFrameRemaining = ParseScriptFrameCount(arguments);
            if (ScriptFrameRemaining <= 0)
                return true;
            return false;
        }

        ScriptFrameRemaining--;
        return ScriptFrameRemaining == 0;
    }

    /// <summary>
    /// <c>00CC88D1</c> default path (no <c>clock</c>):
    /// <c>0099E690</c> atof, target = seconds *
    /// <c>[0x124E640]=15</c>, first <c>vtbl+28</c>,
    /// then loop <c>vtbl+28</c> + add
    /// <c>[0x122DED8]=1</c> until counter &gt;= target.
    /// CLOCK path is unread for first-seen 1.6.
    /// </summary>
    private bool TickGamePause(string arguments)
    {
        if (State.GamePausePhase == 0)
        {
            GamePauseTarget = ParseGamePauseSeconds(arguments) * RegionTravel.GamePauseScale;
            GamePauseCounter = 0f;
            State.GamePausePhase = 1;
            return false;
        }

        if (State.GamePausePhase == 1)
        {
            State.GamePausePhase = 2;
            if (GamePauseCounter >= GamePauseTarget)
            {
                State.GamePausePhase = 0;
                return true;
            }

            return false;
        }

        GamePauseCounter += RegionTravel.GamePauseIncrement;
        if (GamePauseCounter < GamePauseTarget)
            return false;
        State.GamePausePhase = 0;
        return true;
    }

    public static float ParseGamePauseSeconds(string arguments)
    {
        var token = FirstToken(arguments.Trim());
        if (token.Length == 0)
            return 0f;
        return float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds)
            ? seconds
            : 0f;
    }

    public static int ParseScriptFrameCount(string arguments)
    {
        var token = FirstToken(arguments.Trim());
        if (token.Length == 0)
            return RegionTravel.DoScriptFrameDefaultCount;
        var n = 0;
        var negative = false;
        var sawDigit = false;
        foreach (var ch in token)
        {
            if (ch == '-')
            {
                negative = true;
                continue;
            }

            if (ch == '.')
                break;
            if (ch is < '0' or > '9')
                break;
            sawDigit = true;
            n = n * 10 + (ch - '0');
        }

        if (!sawDigit)
            return RegionTravel.DoScriptFrameDefaultCount;
        return negative ? -n : n;
    }

    /// <summary>
    /// Evidence-only helper. Execution does not use this.
    /// Prefer <see cref="EvaluateOne"/>.
    /// </summary>
    public static ScriptFlow FlowOf(CommandResult result) =>
        result.Kind switch
        {
            ExecutionKind.Continue => ScriptFlow.Continue,
            ExecutionKind.Blocked => ScriptFlow.Yield,
            ExecutionKind.WaitFrames or ExecutionKind.WaitScaledFrames
                or ExecutionKind.BlockPump or ExecutionKind.WaitOperation => ScriptFlow.Yield,
            _ => ScriptFlow.YieldAfter,
        };

    internal static void ParseFadeArgs(string arguments, out float seconds, out float param)
    {
        seconds = RegionTravel.FadeSpecialCaseSeconds;
        param = 0f;
        if (arguments.Length == 0)
            return;
        var parts = arguments.Split(',');
        if (parts.Length > 0)
            float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out seconds);
        if (parts.Length > 1)
            float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out param);
    }

    private void Record(ScriptExecutionContext ctx, ScriptLine line, CommandResult result)
    {
        var runtime = ctx.Runtime;
        var lastAnim = runtime.Animations.Count > 0 ? runtime.Animations[^1].Name : "";
        var changes = ctx.Bindings.DrainChanges();
        var spec = ScriptCommandMap.Find(line.Verb);
        runtime.Trace.Add(new RuntimeTraceStep(
            runtime.Frame,
            runtime.Time,
            ctx.Runtime.ActiveQuestName,
            Name,
            InstructionPointer,
            line.Raw,
            line.Verb,
            line.Target ?? "",
            string.Join(",", line.Args),
            line.Family,
            result.Kind,
            result.Status,
            result.YieldReason,
            result.ResumeReason,
            result.OperationId ?? "",
            Yielded,
            Finished,
            Blocked,
            result.SideEffect,
            ctx.Persist.Snapshot(),
            runtime.Interpreters.Count,
            runtime.CameraName,
            lastAnim,
            runtime.TraceWorldSnapshot(),
            string.Join(",", changes),
            runtime.ActiveFiber?.Id ?? 0,
            State.InstanceId,
            runtime.ActiveFiber?.State.ToString() ?? "",
            runtime.ActiveFiber?.WakeTime ?? 0f,
            State.WaitKind.ToString(),
            State.WaitOperationId ?? "",
            result.OperationId ?? "",
            State.ResumeReason,
            Parse: spec?.Parse ?? CommandStatus.Unread,
            Dispatch: spec?.Dispatch ?? (result.Kind == ExecutionKind.Blocked
                ? CommandStatus.Unread
                : CommandStatus.Proven),
            Apply: spec?.Apply ?? CommandStatus.Unread,
            Runtime: spec?.Runtime ?? CommandStatus.Unread,
            Task: result.OperationId ?? "",
            Dialogue: ctx.Dialogue.Session is { } session
                ? $"{session.Verb}:{session.Text}"
                : "",
            Audio: ctx.Audio.Sound ?? "",
            CreatedThing: result.BindingChange.StartsWith("Created:", StringComparison.Ordinal)
                ? result.BindingChange[8..]
                : "",
            RemovedThing: ctx.World.Removes.Count > 0 &&
                          result.BindingChange.StartsWith("unbind ", StringComparison.Ordinal)
                ? result.BindingChange[7..]
                : ""));
    }

    internal static string FirstToken(string arguments)
    {
        var end = arguments.IndexOfAny([',', ' ']);
        return end < 0 ? arguments : arguments[..end];
    }
}

public readonly struct ScriptCommand
{
    public string Raw { get; }
    public string Verb { get; }
    public string? Actor { get; }
    public string Arguments { get; }

    public ScriptCommand(string raw, string verb, string? actor, string arguments)
    {
        Raw = raw;
        Verb = verb;
        Actor = actor;
        Arguments = arguments;
    }

    public static ScriptCommand Parse(string raw)
    {
        var line = ScriptLine.Parse(raw);
        var arguments = line.Args.Count == 0 ? "" : string.Join(",", line.Args);
        return new ScriptCommand(raw, line.Verb, line.Target, arguments);
    }

    /// <summary>
    /// <c>00CBEE5E</c>: strcmp arg to <c>null</c>.
    /// </summary>
    public static bool IsNullArg(string? text) =>
        text is not null && text.Equals("null", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// <c>00CC25FD</c> args: target, text, optional
    /// IsTrue (vtbl+1484), optional random/norepeat/
    /// sequence mode. First-seen is target+text only.
    /// </summary>
    public static (string Target, string Text, int Mode) ParseSpeak(string arguments)
    {
        var args = SplitArgs(arguments);
        var target = args.Length == 0 ? "" : args[0];
        var text = args.Length < 2 ? "" : args[1];
        var mode = 0;
        if (args.Length > 3)
        {
            if (args[3].Equals("random", StringComparison.OrdinalIgnoreCase))
                mode = 1;
            else if (args[3].Equals("norepeat", StringComparison.OrdinalIgnoreCase))
                mode = 2;
            else if (args[3].Equals("sequence", StringComparison.OrdinalIgnoreCase))
                mode = 3;
        }

        return (target, text, mode);
    }

    /// <summary>
    /// <c>00CC2EAA</c>: listener, prompt, optional
    /// IsTrue wait, optional extra lines via
    /// <c>vtbl+1464</c>. First-seen third arg is
    /// FALSE so one <c>vtbl+28</c> then
    /// <c>00CC707C</c>. TRUE wait polls unread
    /// <c>vtbl+1472</c>.
    /// </summary>
    public static (string Listener, string Prompt, bool Wait, string Response)
        ParseInteractiveSpeak(string arguments)
    {
        var args = SplitArgs(arguments);
        var listener = args.Length == 0 ? "" : args[0];
        var prompt = args.Length < 2 ? "" : args[1];
        var wait = args.Length > 2 && IsTrueArg(args[2]);
        var response = args.Length < 4 ? "" : args[3];
        return (listener, prompt, wait, response);
    }

    /// <summary>
    /// <c>00CC3165</c>: listener, text. Empty / null
    /// skip via <c>00CC7081</c>. Then one
    /// <c>vtbl+28</c> and <c>jmp 00CC707C</c>.
    /// </summary>
    public static (string Listener, string Text) ParseDialogSpeak(string arguments)
    {
        var args = SplitArgs(arguments);
        var listener = args.Length == 0 ? "" : args[0];
        var text = args.Length < 2 ? "" : args[1];
        return (listener, text);
    }

    /// <summary>
    /// <c>00CC3354</c>: target, text, optional
    /// random/norepeat/sequence on arg3. Empty
    /// actor / target / text skip via
    /// <c>00CC7081</c>. No <c>00CBEE5E</c>.
    /// </summary>
    public static (string Target, string Text, int Mode) ParseDialogadSpeak(string arguments) =>
        ParseSpeak(arguments);

    /// <summary>
    /// <c>00CC3F73</c>: atof degrees * <c>[0x1238E00]</c>
    /// (1/360). Arg1 IsFalse clears default flag 1.
    /// Empty actor / degrees → <c>00CC7081</c>.
    /// </summary>
    public static (float Degrees, bool Flag, bool HasDegrees) ParseLookInDirection(string arguments)
    {
        var args = SplitArgs(arguments);
        var hasDegrees = args.Length > 0 && args[0].Length != 0;
        var degrees = 0f;
        if (hasDegrees)
            float.TryParse(args[0], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out degrees);
        var flag = args.Length <= 1 || !IsFalseArg(args[1]);
        return (degrees, flag, hasDegrees);
    }

    /// <summary>
    /// <c>00CC0CB5</c>: marker, optional speed
    /// (default 0.3), arg2/arg3 IsTrue wait-for
    /// arrival. First-seen is not wait.
    /// </summary>
    public static (string Marker, float Speed, bool Wait) ParseSneakTo(string arguments)
    {
        var args = SplitArgs(arguments);
        var marker = args.Length == 0 ? "" : args[0];
        var speed = RegionTravel.SneakToDefaultSpeed;
        if (args.Length > 1 &&
            float.TryParse(args[1], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var parsed))
            speed = parsed;
        var wait = (args.Length > 2 && IsTrueArg(args[2])) ||
                   (args.Length > 3 && IsTrueArg(args[3]));
        return (marker, speed, wait);
    }

    /// <summary>
    /// Persist verb is <c>PlayCombatAnimation</c>; exe
    /// token is <c>.PlayCombatAnim</c>.
    /// </summary>
    public static bool IsPlayCombatAnimation(string verb) =>
        verb.Equals("PlayCombatAnimation", StringComparison.OrdinalIgnoreCase) ||
        verb.Equals("PlayCombatAnim", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// <c>00CC15E3</c>: name required. Arg1 IsTrue is
    /// discarded. Arg2/3 IsTrue set flags; arg4/5
    /// IsFalse clear defaults 1; arg6 atoi is count
    /// (default 1); arg7 IsTrue.
    /// </summary>
    public static (string Name, bool FlagA, bool FlagB, bool FlagC, bool FlagD, bool FlagE, int Count)
        ParsePlayCombatAnimation(string arguments)
    {
        var args = SplitArgs(arguments);
        var name = args.Length == 0 ? "" : args[0];
        var flagA = args.Length <= 5 || !IsFalseArg(args[5]);
        var flagB = args.Length > 2 && IsTrueArg(args[2]);
        var flagC = args.Length > 3 && IsTrueArg(args[3]);
        var flagD = args.Length <= 4 || !IsFalseArg(args[4]);
        var flagE = args.Length > 7 && IsTrueArg(args[7]);
        var count = 1;
        if (args.Length > 6 &&
            int.TryParse(args[6], System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var parsed) &&
            parsed > 0)
            count = parsed;
        return (name, flagA, flagB, flagC, flagD, flagE, count);
    }

    /// <summary>
    /// <c>00CCC246</c>: type, marker, name required.
    /// Empty any → <c>00CD17FD</c>. Else apply
    /// <c>vtbl+364</c> and <c>jmp 00CD17F8</c>.
    /// </summary>
    public static (string Type, string Marker, string Name) ParseCreate(string arguments)
    {
        var args = SplitArgs(arguments);
        var type = args.Length == 0 ? "" : args[0];
        var marker = args.Length < 2 ? "" : args[1];
        var name = args.Length < 3 ? "" : args[2];
        return (type, marker, name);
    }

    /// <summary>
    /// <c>00CBEE0C</c>: strcmp arg to <c>false</c>.
    /// </summary>
    public static bool IsFalseArg(string? text) =>
        text is not null && text.Equals("false", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// <c>00CBEDBA</c>: strcmp arg to <c>true</c>.
    /// </summary>
    public static bool IsTrueArg(string? text) =>
        text is not null && text.Equals("true", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// <c>00CC1527</c> defaults then <c>00CBEDBA</c> /
    /// <c>00CBEE0C</c>. Arg 4 default is 1 and
    /// <c>IsFalse</c> clears it. Do not invent pose.
    /// </summary>
    public static (bool Flag1, bool Flag2, bool Flag3, bool Flag4, bool Flag5)
        ParsePlayAnimationFlags(string arguments)
    {
        var args = SplitArgs(arguments);
        var flag1 = args.Length > 1 && IsTrueArg(args[1]);
        var flag2 = args.Length > 2 && IsTrueArg(args[2]);
        var flag3 = args.Length > 3 && IsTrueArg(args[3]);
        var flag4 = args.Length <= 4 || !IsFalseArg(args[4]);
        var flag5 = args.Length > 5 && IsTrueArg(args[5]);
        return (flag1, flag2, flag3, flag4, flag5);
    }

    public static string[] SplitArgs(string arguments)
    {
        if (arguments.Length == 0)
            return [];
        var parts = arguments.Split(',');
        for (var i = 0; i < parts.Length; i++)
            parts[i] = parts[i].Trim();
        return parts;
    }

    /// <summary>
    /// Dry-run the real dispatcher. Not a Verb→ScriptFlow table.
    /// </summary>
    public static ScriptFlow Classify(ScriptCommand command)
    {
        var runtime = ScriptRuntime.Detached();
        var interpreter = new ScriptInterpreter("classify", [command.Raw]);
        return ScriptInterpreter.FlowOf(interpreter.EvaluateOne(runtime));
    }
}

public enum ScriptFlow
{
    Continue,
    Yield,
    YieldAfter,
}

public interface IScriptHost
{
    void PlayMusic(string track);
    void FadeOut(float seconds, float param);
    void FadeIn(float seconds, float param);
    void UseCamera(string name);
    void NoLoadUseCamera(string name);
    void PlayAnimation(string? actor, string arguments);
    void CameraPause(string arguments);
    void Teleport(string? actor, string arguments);
    void LookToThing(string? actor, string arguments);
    void DoCameraPreloading(string arguments);
    void PlayAvi(string arguments);
    bool AviPlaying => false;
    void MuteSounds(string arguments);
    void StartTimeCode();
    void GamePause(float seconds);
    void Speak(string? actor, string target, string text, int mode);
    void InteractiveSpeak(
        string? actor, string listener, string prompt, bool wait, string response);
    void DialogSpeak(string? actor, string listener, string text);
    void WaitTask(string? actor, string name);
    void SneakTo(string? actor, string marker, float speed, bool wait);
    void WalkTo(string? actor, string marker, float speed, bool wait);
    void PlayCombatAnimation(
        string? actor, string name, bool flagA, bool flagB, bool flagC, bool flagD, bool flagE, int count);
    void Create(string type, string marker, string name);
    void WaitActiveDialog();
    void Remove(string name);
    void DialogadSpeak(string? actor, string target, string text, int mode);
    void LookInDirection(string? actor, float degrees, bool flag);
}
