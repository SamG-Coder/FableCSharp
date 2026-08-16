using System.Globalization;

namespace Fable.Game;

/// <summary>
/// <c>00CBFB7D</c> command walk. Continue is
/// <c>jmp 00CD17FD</c> → <c>inc [ebp-72]</c> /
/// <c>jb 00CC012E</c>. Yields stay on the unread
/// wait. Do not invent fade/AVI/wake playback.
/// </summary>
public sealed class ScriptInterpreter
{
    public string Name { get; }
    public IReadOnlyList<string> Commands { get; }
    public int InstructionPointer { get; private set; }
    public bool Yielded { get; private set; }
    public bool Finished { get; private set; }
    public bool FadeSpecialCaseApplied { get; private set; }
    public IReadOnlyList<string> Executed => _executed;

    private readonly List<string> _executed = [];

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
            if (ScriptCommand.Classify(command.Verb) == ScriptFlow.Yield)
            {
                Yielded = true;
                return;
            }

            Dispatch(command, host);
            _executed.Add(raw);
            InstructionPointer++;
        }

        Finished = true;
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

    private static string FirstToken(string arguments)
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
    /// Proven continue: PlayMusic <c>00CC8EAC</c>, FadeOut
    /// <c>00CD0987</c>, UseCamera <c>00CC9F3A</c>,
    /// NoLoadUseCamera <c>00CC9E6A</c>, PlayAnimation
    /// <c>00CC14B9</c>, CameraPause. Proven yield:
    /// DoScriptFrame / GamePause / PlayAVI / Wait*.
    /// Anything unread yields.
    /// </summary>
    public static ScriptFlow Classify(string verb)
    {
        if (verb.Equals("PlayMusic", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("FadeOut", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("FadeIn", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("UseCamera", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("NoLoadUseCamera", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("PlayAnimation", StringComparison.OrdinalIgnoreCase) ||
            verb.Equals("CameraPause", StringComparison.OrdinalIgnoreCase))
            return ScriptFlow.Continue;
        return ScriptFlow.Yield;
    }
}

public enum ScriptFlow
{
    Continue,
    Yield,
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
}
