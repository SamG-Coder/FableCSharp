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
            verb.Equals("UseCamera", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("NoLoadUseCamera", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("CameraPause", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("Teleport", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("DoCameraPreloading", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("PlayAVI", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("MuteSounds", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("StartTimeCode", StringComparison.OrdinalIgnoreCase))
            return ScriptFlow.Continue;
        if (verb.Equals("PlayAnimation", StringComparison.OrdinalIgnoreCase))
            return ScriptFlow.YieldAfter;
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
}
