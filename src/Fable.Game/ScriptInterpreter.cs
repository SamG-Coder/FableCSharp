using System.Globalization;

namespace Fable.Game;

/// <summary>
/// <c>00CBFB7D</c> command walk over the <c>+60</c>
/// CString vector. Continue is <c>jmp 00CD17FD</c> /
/// actor join <c>00CC707C</c>. Unread waits stay put.
/// </summary>
public sealed class ScriptInterpreter
{
    public string Name { get; }
    public IReadOnlyList<string> Commands { get; }
    public int InstructionPointer { get; private set; }
    public bool Yielded { get; private set; }
    public bool Finished { get; private set; }
    public bool FadeSpecialCaseApplied { get; private set; }
    public string? UnsupportedCommand { get; private set; }
    public int ScriptFrameRemaining { get; private set; }
    public float GamePauseTarget { get; private set; }
    public float GamePauseCounter { get; private set; }
    public IReadOnlyList<string> Executed => _executed;

    private readonly List<string> _executed = [];
    private int _gamePausePhase;

    public ScriptInterpreter(string name, IReadOnlyList<string> commands)
    {
        Name = name;
        Commands = commands;
    }

    public void RunUntilYield(IScriptHost? host = null)
    {
        if (Finished || Yielded)
            return;
        if (InstructionPointer == 0 && !FadeSpecialCaseApplied)
            TryFadeSpecialCase(host);

        while (InstructionPointer < Commands.Count)
        {
            var raw = Commands[InstructionPointer];
            var command = ScriptCommand.Parse(raw);
            if (command.Verb.Equals("DoScriptFrame", StringComparison.OrdinalIgnoreCase))
            {
                if (!TickScriptFrame(command.Arguments))
                {
                    Yielded = true;
                    return;
                }

                _executed.Add(raw);
                InstructionPointer++;
                continue;
            }

            if (command.Verb.Equals("GamePause", StringComparison.OrdinalIgnoreCase))
            {
                if (!TickGamePause(command.Arguments))
                {
                    Yielded = true;
                    return;
                }

                host?.GamePause(ParseGamePauseSeconds(command.Arguments));
                _executed.Add(raw);
                InstructionPointer++;
                continue;
            }

            var flow = ScriptCommand.Classify(command);
            if (flow == ScriptFlow.Yield)
            {
                UnsupportedCommand = raw;
                Yielded = true;
                return;
            }

            Dispatch(command, host);
            _executed.Add(raw);
            InstructionPointer++;
            if (flow == ScriptFlow.YieldAfter)
            {
                Yielded = true;
                return;
            }
        }

        Finished = true;
    }

    /// <summary>
    /// <c>00A44660</c> resume: continue after <c>vtbl+28</c>.
    /// Unread waits re-yield on the same IP.
    /// </summary>
    public void Resume(IScriptHost? host = null)
    {
        if (Finished)
            return;
        Yielded = false;
        UnsupportedCommand = null;
        RunUntilYield(host);
    }

    public bool ExecutedVerb(string verb) =>
        _executed.Any(line =>
            ScriptCommand.Parse(line).Verb.Equals(verb, StringComparison.OrdinalIgnoreCase));

    private void TryFadeSpecialCase(IScriptHost? host)
    {
        if (Commands.Count == 0)
            return;
        if (!Commands[0].Equals(RegionTravel.FadeSpecialCase, StringComparison.Ordinal))
            return;
        FadeSpecialCaseApplied = true;
        host?.FadeOut(RegionTravel.FadeSpecialCaseSeconds, 0f);
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
        if (_gamePausePhase == 0)
        {
            GamePauseTarget = ParseGamePauseSeconds(arguments) * RegionTravel.GamePauseScale;
            GamePauseCounter = 0f;
            _gamePausePhase = 1;
            return false;
        }

        if (_gamePausePhase == 1)
        {
            _gamePausePhase = 2;
            if (GamePauseCounter >= GamePauseTarget)
            {
                _gamePausePhase = 0;
                return true;
            }

            return false;
        }

        GamePauseCounter += RegionTravel.GamePauseIncrement;
        if (GamePauseCounter < GamePauseTarget)
            return false;
        _gamePausePhase = 0;
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

    private static void Dispatch(ScriptCommand command, IScriptHost? host)
    {
        if (host is null)
            return;
        if (command.Verb.Equals("PlayMusic", StringComparison.OrdinalIgnoreCase))
            host.PlayMusic(command.Arguments);
        else if (command.Verb.Equals("FadeOut", StringComparison.OrdinalIgnoreCase))
        {
            ParseFadeArgs(command.Arguments, out var seconds, out var param);
            host.FadeOut(seconds, param);
        }
        else if (command.Verb.Equals("FadeIn", StringComparison.OrdinalIgnoreCase))
        {
            ParseFadeArgs(command.Arguments, out var seconds, out var param);
            host.FadeIn(seconds, param);
        }
        else if (command.Verb.Equals("UseCamera", StringComparison.OrdinalIgnoreCase))
            host.UseCamera(FirstToken(command.Arguments));
        else if (command.Verb.Equals("NoLoadUseCamera", StringComparison.OrdinalIgnoreCase))
            host.NoLoadUseCamera(FirstToken(command.Arguments));
        else if (command.Verb.Equals("PlayAnimation", StringComparison.OrdinalIgnoreCase))
            host.PlayAnimation(command.Actor, command.Arguments);
        else if (command.Verb.Equals("CameraPause", StringComparison.OrdinalIgnoreCase))
            host.CameraPause(command.Arguments);
        else if (command.Verb.Equals("Teleport", StringComparison.OrdinalIgnoreCase))
            host.Teleport(command.Actor, command.Arguments);
        else if (command.Verb.Equals("LookToThing", StringComparison.OrdinalIgnoreCase))
            host.LookToThing(command.Actor, command.Arguments);
        else if (command.Verb.Equals("DoCameraPreloading", StringComparison.OrdinalIgnoreCase))
            host.DoCameraPreloading(command.Arguments);
        else if (command.Verb.Equals("PlayAVI", StringComparison.OrdinalIgnoreCase))
            host.PlayAvi(command.Arguments);
        else if (command.Verb.Equals("MuteSounds", StringComparison.OrdinalIgnoreCase))
            host.MuteSounds(command.Arguments);
        else if (command.Verb.Equals("StartTimeCode", StringComparison.OrdinalIgnoreCase))
            host.StartTimeCode();
        else if (command.Verb.Equals("Speak", StringComparison.OrdinalIgnoreCase))
        {
            var speech = ScriptCommand.ParseSpeak(command.Arguments);
            if (speech.Target.Length != 0 &&
                speech.Text.Length != 0 &&
                !ScriptCommand.IsNullArg(speech.Text))
                host.Speak(command.Actor, speech.Target, speech.Text, speech.Mode);
        }
        else if (command.Verb.Equals("InteractiveSpeak", StringComparison.OrdinalIgnoreCase))
        {
            var speech = ScriptCommand.ParseInteractiveSpeak(command.Arguments);
            host.InteractiveSpeak(
                command.Actor, speech.Listener, speech.Prompt, speech.Wait, speech.Response);
        }
        else if (command.Verb.Equals("DialogSpeak", StringComparison.OrdinalIgnoreCase))
        {
            var speech = ScriptCommand.ParseDialogSpeak(command.Arguments);
            if (speech.Listener.Length != 0 &&
                speech.Text.Length != 0 &&
                !ScriptCommand.IsNullArg(speech.Text))
                host.DialogSpeak(command.Actor, speech.Listener, speech.Text);
        }
        else if (command.Verb.Equals("WaitTask", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrEmpty(command.Actor))
                host.WaitTask(command.Actor, ScriptInterpreter.FirstToken(command.Arguments));
        }
        else if (command.Verb.Equals("SneakTo", StringComparison.OrdinalIgnoreCase))
        {
            var sneak = ScriptCommand.ParseSneakTo(command.Arguments);
            if (sneak.Marker.Length != 0)
                host.SneakTo(command.Actor, sneak.Marker, sneak.Speed, sneak.Wait);
        }
        else if (command.Verb.Equals("WalkTo", StringComparison.OrdinalIgnoreCase))
        {
            var walk = ScriptCommand.ParseSneakTo(command.Arguments);
            if (walk.Marker.Length != 0)
                host.WalkTo(command.Actor, walk.Marker, walk.Speed, walk.Wait);
        }
        else if (ScriptCommand.IsPlayCombatAnimation(command.Verb))
        {
            var anim = ScriptCommand.ParsePlayCombatAnimation(command.Arguments);
            if (anim.Name.Length != 0)
                host.PlayCombatAnimation(
                    command.Actor, anim.Name, anim.FlagA, anim.FlagB, anim.FlagC, anim.FlagD, anim.FlagE, anim.Count);
        }
        else if (command.Verb.Equals("Create", StringComparison.OrdinalIgnoreCase))
        {
            var create = ScriptCommand.ParseCreate(command.Arguments);
            if (create.Type.Length != 0 &&
                create.Marker.Length != 0 &&
                create.Name.Length != 0)
                host.Create(create.Type, create.Marker, create.Name);
        }
    }

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
        var space = raw.IndexOf(' ');
        var head = space < 0 ? raw : raw[..space];
        var arguments = space < 0 ? "" : raw[(space + 1)..];
        var dot = head.LastIndexOf('.');
        if (dot > 0)
            return new ScriptCommand(raw, head[(dot + 1)..], head[..dot], arguments);
        return new ScriptCommand(raw, head, null, arguments);
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

    public static ScriptFlow Classify(ScriptCommand command)
    {
        var verb = command.Verb;
        if (verb.Equals("PlayMusic", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("FadeOut", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("FadeIn", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("CameraPause", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("Teleport", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("DoCameraPreloading", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("PlayAVI", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("MuteSounds", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("StartTimeCode", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("Create", StringComparison.OrdinalIgnoreCase))
            return ScriptFlow.Continue;
        if (verb.Equals("UseCamera", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("NoLoadUseCamera", StringComparison.OrdinalIgnoreCase))
        {
            var name = ScriptInterpreter.FirstToken(command.Arguments);
            if (name.Length == 0 || IsNullArg(name))
                return ScriptFlow.Continue;
            return ScriptFlow.YieldAfter;
        }

        if (verb.Equals("PlayAnimation", StringComparison.OrdinalIgnoreCase))
            return ScriptFlow.YieldAfter;
        if (verb.Equals("Speak", StringComparison.OrdinalIgnoreCase))
        {
            var speech = ParseSpeak(command.Arguments);
            if (speech.Target.Length == 0 ||
                speech.Text.Length == 0 ||
                IsNullArg(speech.Text))
                return ScriptFlow.Continue;
            return ScriptFlow.YieldAfter;
        }

        if (verb.Equals("InteractiveSpeak", StringComparison.OrdinalIgnoreCase))
        {
            var speech = ParseInteractiveSpeak(command.Arguments);
            if (speech.Listener.Length == 0 || speech.Prompt.Length == 0)
                return ScriptFlow.Continue;
            return speech.Wait ? ScriptFlow.Yield : ScriptFlow.YieldAfter;
        }
        if (verb.Equals("DialogSpeak", StringComparison.OrdinalIgnoreCase))
        {
            var speech = ParseDialogSpeak(command.Arguments);
            if (speech.Listener.Length == 0 ||
                speech.Text.Length == 0 ||
                IsNullArg(speech.Text))
                return ScriptFlow.Continue;
            return ScriptFlow.YieldAfter;
        }
        if (verb.Equals("WaitTask", StringComparison.OrdinalIgnoreCase))
            return string.IsNullOrEmpty(command.Actor) ? ScriptFlow.Continue : ScriptFlow.YieldAfter;
        if (verb.Equals("SneakTo", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("WalkTo", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(command.Actor))
                return ScriptFlow.Continue;
            var move = ParseSneakTo(command.Arguments);
            if (move.Marker.Length == 0)
                return ScriptFlow.Continue;
            return move.Wait ? ScriptFlow.Yield : ScriptFlow.YieldAfter;
        }
        if (IsPlayCombatAnimation(verb))
        {
            var anim = ParsePlayCombatAnimation(command.Arguments);
            if (anim.Name.Length == 0)
                return ScriptFlow.Continue;
            return ScriptFlow.YieldAfter;
        }
        if (verb.Equals("LookToThing", StringComparison.OrdinalIgnoreCase))
        {
            var args = SplitArgs(command.Arguments);
            if (args.Length >= 3 && IsFalseArg(args[2]))
                return ScriptFlow.Continue;
            return ScriptFlow.YieldAfter;
        }

        return ScriptFlow.Yield;
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
}
