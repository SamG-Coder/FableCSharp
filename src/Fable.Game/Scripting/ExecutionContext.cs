using System.Numerics;
using Fable.Formats.Tng;

namespace Fable.Game.Scripting;

/// <summary>
/// Per-cutscene + world services used by handlers.
/// </summary>
public sealed class ScriptExecutionContext
{
    public ScriptRuntime Runtime { get; }
    public ScriptBindings Bindings { get; }
    public ScriptArguments Arguments { get; }
    public PersistStore Persist { get; }
    public CameraRuntime Camera { get; }
    public AudioRuntime Audio { get; }
    public DialogueRuntime Dialogue { get; }
    public AnimationRuntime Animation { get; }
    public MovementRuntime Movement { get; }
    public WorldRuntime World { get; }
    public CutsceneState Cutscene { get; }

    public ScriptExecutionContext(
        ScriptRuntime runtime,
        ScriptBindings bindings,
        ScriptArguments arguments,
        PersistStore persist,
        CameraRuntime camera,
        AudioRuntime audio,
        DialogueRuntime dialogue,
        AnimationRuntime animation,
        MovementRuntime movement,
        WorldRuntime world,
        CutsceneState cutscene)
    {
        Runtime = runtime;
        Bindings = bindings;
        Arguments = arguments;
        Persist = persist;
        Camera = camera;
        Audio = audio;
        Dialogue = dialogue;
        Animation = animation;
        Movement = movement;
        World = world;
        Cutscene = cutscene;
    }

    public ThingInstance? FindThing(string name) =>
        Bindings.Resolve(name)?.Thing ?? Runtime.FindThingByName(name);
}

/// <summary>
/// Runner locals: <c>[ebp-37]</c> CameraPause,
/// <c>[ebp+103]</c> yield-enable.
/// </summary>
public sealed class CutsceneState
{
    public string Name { get; }
    public IReadOnlyList<string> Commands { get; set; }
    public int Pc { get; set; }
    public bool Yielded { get; set; }
    public bool Finished { get; set; }
    public bool Blocked { get; set; }
    public string? BlockReason { get; set; }
    public bool FadeSpecialCaseApplied { get; set; }
    public bool SkipListApplied { get; set; }
    public bool CameraPauseEnabled { get; set; } = true;
    public bool YieldEnable { get; set; } = true;
    public bool StayFadedOut { get; set; }
    public bool NoDialogCam { get; set; }
    public int ScriptFrameRemaining { get; set; }
    public float GamePauseTarget { get; set; }
    public float GamePauseCounter { get; set; }
    public int GamePausePhase { get; set; }
    public int AviAt { get; set; } = -1;
    public string? WaitOperationId { get; set; }
    public ExecutionKind WaitKind { get; set; }
    public string YieldReason { get; set; } = "";
    public string ResumeReason { get; set; } = "";
    public int InstanceId { get; set; }
    public readonly List<string> Executed = [];

    public CutsceneState(string name, IReadOnlyList<string> commands)
    {
        Name = name;
        Commands = commands;
    }
}

public sealed class CameraRuntime
{
    private readonly List<string> _preloaded = [];
    public IReadOnlyList<string> Preloaded => _preloaded;
    public string ActiveName { get; private set; } = "";

    public void Bind(ScriptedCamera? camera, IReadOnlyList<ThingInstance> things, string name)
    {
        ActiveName = name;
        camera?.UseCamera(things, name);
    }

    public void Preload(string name)
    {
        if (name.Length == 0 || _preloaded.Contains(name, StringComparer.OrdinalIgnoreCase))
            return;
        _preloaded.Add(name);
    }
}

public sealed class AudioRuntime
{
    public string? Music { get; private set; }
    public string? Sound { get; private set; }
    public bool Muted { get; private set; }

    public void PlayMusic(string track) => Music = track;
    public void PlaySound(string name) => Sound = name;
    public void StopMusic() => Music = "";
    public void Mute(bool mute) => Muted = mute;
}

public sealed class DialogueRuntime
{
    public readonly List<ScriptSpeech> Speeches = [];
    public readonly List<ScriptInteractiveSpeech> Interactive = [];
    public readonly List<ScriptDialogSpeech> Dialogs = [];
    public readonly List<ScriptDialogAdSpeech> DialogAds = [];
    public int ActiveCount { get; private set; }
    public bool HasActive => ActiveCount > 0;
    public PendingOperation? WaitOp { get; private set; }
    private int _waitSerial;

    public void Speak(string? actor, string target, string text, int mode)
    {
        Speeches.Add(new ScriptSpeech(actor, target, text, mode));
        ActiveCount++;
    }

    public PendingOperation InteractiveSpeak(
        string? actor, string listener, string prompt, bool wait, string response)
    {
        Interactive.Add(new ScriptInteractiveSpeech(actor, listener, prompt, wait, response));
        ActiveCount++;
        var op = new PendingOperation($"ispeak-{++_waitSerial}", "InteractiveSpeak", actor, prompt);
        if (wait)
            WaitOp = op;
        return op;
    }

    public void CompleteWait()
    {
        if (WaitOp is not null)
            WaitOp.Complete = true;
        Dismiss();
    }

    /// <summary>
    /// <c>00CC656B</c> leftover session poll
    /// <c>vtbl+1472</c>. Waits on the active line.
    /// </summary>
    public PendingOperation WaitActive()
    {
        if (WaitOp is { Complete: false } existing)
            return existing;
        WaitOp = new PendingOperation($"dialog-{++_waitSerial}", "WaitActiveDialog", null, "active");
        if (ActiveCount == 0)
            WaitOp.Complete = true;
        return WaitOp;
    }

    public void DialogSpeak(string? actor, string listener, string text)
    {
        Dialogs.Add(new ScriptDialogSpeech(actor, listener, text));
        ActiveCount++;
    }

    public void DialogAdSpeak(string? actor, string target, string text, int mode)
    {
        DialogAds.Add(new ScriptDialogAdSpeech(actor, target, text, mode));
        ActiveCount++;
    }

    public void Dismiss()
    {
        if (ActiveCount > 0)
            ActiveCount--;
    }
}

public sealed class AnimationRuntime
{
    public readonly List<ScriptAnimation> Plays = [];
    public readonly List<ScriptCombatAnimation> Combat = [];
    public readonly Dictionary<string, PendingOperation> ByActor =
        new(StringComparer.OrdinalIgnoreCase);
    private int _next;

    public PendingOperation Play(
        string? actor, string name, bool f1, bool f2, bool f3, bool f4, bool f5)
    {
        Plays.Add(new ScriptAnimation(actor, name, f1, f2, f3, f4, f5));
        var op = new PendingOperation($"anim-{++_next}", "PlayAnimation", actor, name);
        if (actor is { Length: > 0 })
            ByActor[actor] = op;
        return op;
    }

    public PendingOperation PlayCombat(
        string? actor, string name, bool a, bool b, bool c, bool d, bool e, int count)
    {
        Combat.Add(new ScriptCombatAnimation(actor, name, a, b, c, d, e, count));
        var op = new PendingOperation($"combat-{++_next}", "PlayCombatAnimation", actor, name);
        if (actor is { Length: > 0 })
            ByActor[actor] = op;
        return op;
    }

    public void Clear(string? actor)
    {
        if (actor is { Length: > 0 } && ByActor.TryGetValue(actor, out var op))
            op.Complete = true;
    }

    public PendingOperation? Current(string? actor) =>
        actor is { Length: > 0 } && ByActor.TryGetValue(actor, out var op) ? op : null;
}

public sealed class MovementRuntime
{
    public readonly List<ScriptSneakTo> Sneaks = [];
    public readonly List<ScriptWalkTo> Walks = [];
    public readonly List<(string? Actor, string Marker, float Speed, bool Wait)> Runs = [];
    public readonly Dictionary<string, PendingOperation> ByActor =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, float> WalkSpeed = new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, float> RunSpeed = new(StringComparer.OrdinalIgnoreCase);
    private int _next;

    public PendingOperation Sneak(string? actor, string marker, float speed, bool wait)
    {
        Sneaks.Add(new ScriptSneakTo(actor, marker, speed, wait));
        return Queue(actor, "SneakTo", marker);
    }

    public PendingOperation Walk(string? actor, string marker, float speed, bool wait)
    {
        Walks.Add(new ScriptWalkTo(actor, marker, speed, wait));
        return Queue(actor, "WalkTo", marker);
    }

    public PendingOperation Run(string? actor, string marker, float speed, bool wait)
    {
        Runs.Add((actor, marker, speed, wait));
        return Queue(actor, "RunTo", marker);
    }

    public void Clear(string? actor)
    {
        if (actor is { Length: > 0 } && ByActor.TryGetValue(actor, out var op))
            op.Complete = true;
    }

    public PendingOperation? Current(string? actor) =>
        actor is { Length: > 0 } && ByActor.TryGetValue(actor, out var op) ? op : null;

    private PendingOperation Queue(string? actor, string kind, string marker)
    {
        var op = new PendingOperation($"move-{++_next}", kind, actor, marker);
        if (actor is { Length: > 0 })
            ByActor[actor] = op;
        return op;
    }
}

public sealed class WorldRuntime
{
    public readonly List<ScriptCreate> Creates = [];
    public readonly List<string> Removes = [];
    public readonly List<ScriptTeleport> Teleports = [];
    public readonly List<ScriptLookToThing> LookToThings = [];
    public readonly List<ScriptLookInDirection> Looks = [];
    public readonly Dictionary<string, string> LookTargets = new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, Vector3> Positions = new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, bool> Doors = new(StringComparer.OrdinalIgnoreCase);
    public readonly List<string> Modes = [];
    public readonly List<(string Item, int Count)> HeroGifts = [];
    public readonly List<(bool Hide, string Mode)> ExtraOps = [];
    public bool ExtrasHidden { get; private set; }
    public string ExtraMode { get; private set; } = "";
    public float TimeOfDayHours { get; set; }
    public float TimeOfDayFraction { get; set; }

    public void RemoveExtras(bool hide, string mode)
    {
        ExtraOps.Add((hide, mode));
        ExtraMode = mode;
        ExtrasHidden = hide;
    }

    public void Teleport(string? actor, string marker, Vector3? position)
    {
        Teleports.Add(new ScriptTeleport(actor, marker, position));
        if (actor is { Length: > 0 } && position is { } pos)
            Positions[actor] = pos;
    }
}
