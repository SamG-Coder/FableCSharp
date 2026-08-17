using System.Globalization;
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
    public FlagStore Flags { get; }
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
        FlagStore flags,
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
        Flags = flags;
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
    public bool AnimationPauseEnabled { get; set; } = true;
    public bool YieldEnable { get; set; } = true;
    public bool StayFadedOut { get; set; }
    public bool NoDialogCam { get; set; }
    /// <summary>
    /// <c>[ebp-39]</c>: SetFlag sets 1 after a write.
    /// A later SetFlag with IsTrue(arg2) skips rewrite.
    /// </summary>
    public bool FlagRewriteDone { get; set; }
    public IReadOnlyList<string> LightDefs { get; set; } = [];
    public IReadOnlyList<string> LightScenes { get; set; } = [];
    /// <summary>
    /// <c>[ebp-112]</c> hold written by TintScreenTo,
    /// consumed and cleared by TintScreenOut.
    /// </summary>
    public float TintHold { get; set; }
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
    public string LookAt { get; private set; } = "";
    public bool Busy { get; private set; }
    /// <summary>
    /// Script arg0 (second <c>vtbl+1696</c> float).
    /// </summary>
    public float ShakeArg0 { get; private set; }
    /// <summary>
    /// Script arg1 (first <c>vtbl+1696</c> float).
    /// </summary>
    public float ShakeArg1 { get; private set; }
    public bool ShakeActive { get; private set; }
    public float EffectArg0 { get; private set; }
    public float EffectArg1 { get; private set; }
    public float EffectArg2 { get; private set; }
    public bool EffectActive { get; private set; }
    public string PathA { get; private set; } = "";
    public string PathB { get; private set; } = "";
    public string PathC { get; private set; } = "";
    public string PathD { get; private set; } = "";
    public float PathDuration { get; private set; }
    public float TintOutDuration { get; private set; }
    public float TintOutHold { get; private set; }
    public bool TintOutActive { get; private set; }
    public float TintArg0 { get; private set; }
    public float TintArg1 { get; private set; }
    public float TintArg2 { get; private set; }
    public float TintArg3 { get; private set; }
    public float TintArg4 { get; private set; }
    public Vector3 TintRgb { get; private set; }
    public readonly List<string> TintFilters = [];
    public readonly List<string> TintTargets = [];
    public int TintHandle { get; private set; }
    public bool TintToActive { get; private set; }
    public string RotateThing { get; private set; } = "";
    public float RotateParam { get; private set; }
    public Vector3 RotateAxis { get; private set; }
    public bool RotateActive { get; private set; }
    private int _tintSerial;

    public void Bind(ScriptedCamera? camera, IReadOnlyList<ThingInstance> things, string name)
    {
        ActiveName = name;
        Busy = true;
        camera?.UseCamera(things, name);
    }

    public void ClearBusy() => Busy = false;

    /// <summary>
    /// <c>00CD131F</c> <c>vtbl+1696(arg1, arg0)</c>.
    /// Shake decay body unread — host stores the pair.
    /// </summary>
    public void Shake(float arg0, float arg1)
    {
        ShakeArg0 = arg0;
        ShakeArg1 = arg1;
        ShakeActive = true;
    }

    /// <summary>
    /// <c>00CD1258</c> <c>vtbl+1676(arg0,arg1,arg2)</c>.
    /// Filter body unread — host stores the triple.
    /// </summary>
    public void Effect(float arg0, float arg1, float arg2)
    {
        EffectArg0 = arg0;
        EffectArg1 = arg1;
        EffectArg2 = arg2;
        EffectActive = true;
    }

    /// <summary>
    /// <c>00CCAF70</c> <c>vtbl+1640</c>(pos0,pos2,pos1,pos3,dur).
    /// Spline unread — host sits at first marker, looks at second.
    /// </summary>
    public void Path(
        ScriptedCamera? camera,
        ThingInstance? a, string nameA,
        ThingInstance? b, string nameB,
        ThingInstance? c, string nameC,
        ThingInstance? d, string nameD,
        float duration)
    {
        PathA = nameA;
        PathB = nameB;
        PathC = nameC;
        PathD = nameD;
        PathDuration = duration;
        Busy = true;
        if (a is { PositionX: not null })
            camera?.SetPosition(RegionTravel.PositionOf(a));
        if (b is { PositionX: not null })
            camera?.SetLookAt(RegionTravel.PositionOf(b));
    }

    /// <summary>
    /// <c>00CD11D0</c> <c>vtbl+2704([ebp-112], dur)</c>
    /// then clears the hold. Overlay body unread.
    /// </summary>
    public void TintOut(float hold, float duration)
    {
        TintOutHold = hold;
        TintOutDuration = duration;
        TintOutActive = true;
    }

    /// <summary>
    /// <c>00CD0CE4</c> <c>vtbl+2700</c> then
    /// <c>[ebp-112]=eax</c>. RGB scaled by
    /// <c>1/255</c> at <c>0x1231724</c>.
    /// </summary>
    public int TintTo(
        float a0, float a1, float a2, float a3, float a4,
        Vector3 rgb, IReadOnlyList<string> filters, IReadOnlyList<string> targets)
    {
        TintArg0 = a0;
        TintArg1 = a1;
        TintArg2 = a2;
        TintArg3 = a3;
        TintArg4 = a4;
        TintRgb = rgb;
        TintFilters.Clear();
        TintFilters.AddRange(filters);
        TintTargets.Clear();
        TintTargets.AddRange(targets);
        TintToActive = true;
        TintHandle = ++_tintSerial;
        return TintHandle;
    }

    /// <summary>
    /// <c>00CCA609</c> <c>vtbl+1616</c>(thing, xyz, param)
    /// then <c>00CC907D</c> yield. Orbit body unread.
    /// </summary>
    public void Rotate(
        ScriptedCamera? camera, ThingInstance? thing, string name,
        float param, Vector3 axis)
    {
        RotateThing = name;
        RotateParam = param;
        RotateAxis = axis;
        RotateActive = true;
        Busy = true;
        if (thing is { PositionX: not null })
            camera?.SetLookAt(RegionTravel.PositionOf(thing));
    }

    public void LookAtThing(ScriptedCamera? camera, ThingInstance? thing, string name)
    {
        LookAt = name;
        Busy = true;
        if (thing is { PositionX: not null })
            camera?.SetLookAt(RegionTravel.PositionOf(thing));
    }

    public string LookBetweenA { get; private set; } = "";
    public string LookBetweenB { get; private set; } = "";
    public float LookBetweenDuration { get; private set; }
    public float LookBetweenFov { get; private set; } = -1f;
    public Vector3? LookBetweenCameraPos { get; private set; }

    /// <summary>
    /// <c>00CCAA6C</c> apply <c>vtbl+1632</c>: look
    /// between thing0+off and thing1+off. Blend body
    /// unread — host aims at the midpoint.
    /// </summary>
    public void LookBetween(
        ScriptedCamera? camera,
        ThingInstance? a, string nameA,
        ThingInstance? b, string nameB,
        Vector3 offsetA, Vector3 offsetB,
        float duration, float fovDegrees = -1f)
    {
        LookBetweenA = nameA;
        LookBetweenB = nameB;
        LookBetweenDuration = duration;
        LookBetweenFov = fovDegrees;
        LookAt = nameA + "|" + nameB;
        Busy = true;
        if (fovDegrees >= 0f)
            camera?.SetFovDegrees(fovDegrees);
        Vector3? posA = a is { PositionX: not null }
            ? RegionTravel.PositionOf(a) + offsetA
            : null;
        Vector3? posB = b is { PositionX: not null }
            ? RegionTravel.PositionOf(b) + offsetB
            : null;
        if (posA is { } pa && posB is { } pb)
            camera?.SetLookAt((pa + pb) * 0.5f);
        else if (posA is { } onlyA)
            camera?.SetLookAt(onlyA);
        else if (posB is { } onlyB)
            camera?.SetLookAt(onlyB);
    }

    /// <summary>
    /// <c>00CCB0D0</c> apply <c>vtbl+1636</c>: look
    /// between A/B from a third position (arg2 thing or
    /// table handle) plus arg4-6. Blend unread.
    /// </summary>
    public void LookBetweenPos(
        ScriptedCamera? camera,
        ThingInstance? a, string nameA,
        ThingInstance? b, string nameB,
        Vector3? cameraPos,
        float duration, float fovDegrees)
    {
        LookBetween(camera, a, nameA, b, nameB, default, default, duration, fovDegrees);
        LookBetweenCameraPos = cameraPos;
        if (cameraPos is { } pos)
            camera?.SetPosition(pos);
    }

    /// <summary>
    /// <c>00CC9DF1</c> <c>vtbl+1668(0)</c> +
    /// <c>vtbl+1664</c>. Clears script camera and
    /// restores the gameplay snapshot.
    /// </summary>
    public void Reset(ScriptedCamera? camera)
    {
        camera?.Reset();
        ActiveName = camera?.ActiveName ?? "";
        LookAt = "";
        Busy = false;
        if (WaitOp is not null)
            WaitOp.Complete = true;
    }

    public PendingOperation? WaitOp { get; private set; }
    private int _waitSerial;

    public PendingOperation WaitForCamera()
    {
        if (!Busy)
        {
            var done = new PendingOperation($"cam-{++_waitSerial}", "WaitForCamera", null, ActiveName)
            {
                Complete = true,
            };
            return done;
        }

        WaitOp = new PendingOperation($"cam-{++_waitSerial}", "WaitForCamera", null, ActiveName);
        return WaitOp;
    }

    public void CompleteWait()
    {
        Busy = false;
        if (WaitOp is not null)
            WaitOp.Complete = true;
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
    public readonly List<ScriptAudioInstance> Instances = [];

    public void PlayMusic(string track) => Music = track;

    public ScriptAudioInstance PlaySound(string name, string? source, bool spatial)
    {
        Sound = name;
        var inst = new ScriptAudioInstance(name, source, spatial);
        Instances.Add(inst);
        return inst;
    }

    public void StopMusic() => Music = "";
    public void Mute(bool mute) => Muted = mute;
}

public readonly record struct ScriptAudioInstance(string Name, string? Source, bool Spatial);

public sealed class DialogueRuntime
{
    public readonly List<ScriptSpeech> Speeches = [];
    public readonly List<ScriptInteractiveSpeech> Interactive = [];
    public readonly List<ScriptDialogSpeech> Dialogs = [];
    public readonly List<ScriptDialogAdSpeech> DialogAds = [];
    public DialogueSession? Session { get; private set; }
    public int ActiveCount { get; private set; }
    public bool HasActive => ActiveCount > 0 || Session is { Active: true };
    public PendingOperation? WaitOp { get; private set; }
    private int _waitSerial;

    public void Speak(string? actor, string target, string text, int mode)
    {
        Speeches.Add(new ScriptSpeech(actor, target, text, mode));
        Open(actor, target, text, mode, "Speak");
    }

    public PendingOperation InteractiveSpeak(
        string? actor, string listener, string prompt, bool wait, string response)
    {
        Interactive.Add(new ScriptInteractiveSpeech(actor, listener, prompt, wait, response));
        Open(actor, listener, prompt, 0, "InteractiveSpeak");
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
        Open(actor, listener, text, 0, "DialogSpeak");
    }

    public void DialogAdSpeak(string? actor, string target, string text, int mode)
    {
        DialogAds.Add(new ScriptDialogAdSpeech(actor, target, text, mode));
        Open(actor, target, text, mode, "DialogadSpeak");
    }

    public void Dismiss()
    {
        if (ActiveCount > 0)
            ActiveCount--;
        if (Session is not null)
            Session.Active = ActiveCount > 0;
    }

    private void Open(string? speaker, string listener, string text, int mode, string verb)
    {
        Session = new DialogueSession(speaker, listener, text, mode, verb);
        ActiveCount++;
    }
}

public sealed class DialogueSession
{
    public string? Speaker { get; }
    public string Listener { get; }
    public string Text { get; }
    public int Mode { get; }
    public string Verb { get; }
    public bool Active { get; set; } = true;

    public DialogueSession(string? speaker, string listener, string text, int mode, string verb)
    {
        Speaker = speaker;
        Listener = listener;
        Text = text;
        Mode = mode;
        Verb = verb;
    }
}

public sealed class AnimationRuntime
{
    public readonly List<ScriptAnimation> Plays = [];
    public readonly List<ScriptCombatAnimation> Combat = [];
    public readonly Dictionary<string, PendingOperation> ByActor =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, AnimationState> States =
        new(StringComparer.OrdinalIgnoreCase);
    public EntityTaskQueue Tasks { get; } = new();
    private int _next;

    public PendingOperation Play(
        string? actor, string name, bool f1, bool f2, bool f3, bool f4, bool f5)
    {
        Plays.Add(new ScriptAnimation(actor, name, f1, f2, f3, f4, f5));
        var looping = f1 || f3;
        if (actor is { Length: > 0 })
            States[actor] = new AnimationState(actor, name, looping, f1, f2, f3, f4, f5);
        var kind = looping ? EntityTaskKind.LoopAnimate : EntityTaskKind.Animate;
        var task = Tasks.Replace(actor, kind, name, null, 0f);
        var op = new PendingOperation(task.Id, "PlayAnimation", actor, name);
        if (actor is { Length: > 0 })
            ByActor[actor] = op;
        _next++;
        return op;
    }

    public PendingOperation PlayCombat(
        string? actor, string name, bool a, bool b, bool c, bool d, bool e, int count)
    {
        Combat.Add(new ScriptCombatAnimation(actor, name, a, b, c, d, e, count));
        if (actor is { Length: > 0 })
            States[actor] = new AnimationState(actor, name, false, a, b, c, d, e);
        var task = Tasks.Replace(actor, EntityTaskKind.CombatAnimate, name, null, 0f);
        var op = new PendingOperation(task.Id, "PlayCombatAnimation", actor, name);
        if (actor is { Length: > 0 })
            ByActor[actor] = op;
        _next++;
        return op;
    }

    public void Clear(string? actor)
    {
        Tasks.Clear(actor);
        if (actor is { Length: > 0 } && ByActor.TryGetValue(actor, out var op))
            op.Complete = true;
    }

    public PendingOperation? Current(string? actor) =>
        actor is { Length: > 0 } && ByActor.TryGetValue(actor, out var op) ? op : null;
}

public readonly record struct AnimationState(
    string Actor,
    string Name,
    bool Looping,
    bool F1,
    bool F2,
    bool F3,
    bool F4,
    bool F5);

public sealed class MovementRuntime
{
    public readonly List<ScriptSneakTo> Sneaks = [];
    public readonly List<ScriptWalkTo> Walks = [];
    public readonly List<(string? Actor, string Marker, float Speed, bool Wait)> Runs = [];
    public readonly Dictionary<string, PendingOperation> ByActor =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, float> WalkSpeed = new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, float> RunSpeed = new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, Vector3> Destinations =
        new(StringComparer.OrdinalIgnoreCase);
    public EntityTaskQueue Tasks { get; } = new();
    private int _next;

    public PendingOperation Sneak(
        string? actor, string marker, float speed, bool wait, Vector3? dest)
    {
        Sneaks.Add(new ScriptSneakTo(actor, marker, speed, wait));
        return Queue(actor, EntityTaskKind.Sneak, marker, dest, ResolveSpeed(actor, speed, false));
    }

    public PendingOperation Walk(
        string? actor, string marker, float speed, bool wait, Vector3? dest)
    {
        Walks.Add(new ScriptWalkTo(actor, marker, speed, wait));
        return Queue(actor, EntityTaskKind.Walk, marker, dest, ResolveSpeed(actor, speed, false));
    }

    public PendingOperation Run(
        string? actor, string marker, float speed, bool wait, Vector3? dest)
    {
        Runs.Add((actor, marker, speed, wait));
        return Queue(actor, EntityTaskKind.Run, marker, dest, ResolveSpeed(actor, speed, true));
    }

    public PendingOperation Follow(string? actor, string target, float speed, Vector3? dest) =>
        Queue(actor, EntityTaskKind.Follow, target, dest, speed > 0f ? speed : 1f);

    public void Clear(string? actor)
    {
        Tasks.Clear(actor);
        if (actor is { Length: > 0 } && ByActor.TryGetValue(actor, out var op))
            op.Complete = true;
    }

    public PendingOperation? Current(string? actor) =>
        actor is { Length: > 0 } && ByActor.TryGetValue(actor, out var op) ? op : null;

    public void Tick(float dt, WorldRuntime world) => Tasks.Tick(dt, world);

    private float ResolveSpeed(string? actor, float speed, bool run)
    {
        if (speed > 0f)
            return speed;
        if (actor is { Length: > 0 })
        {
            if (run && RunSpeed.TryGetValue(actor, out var runMax) && runMax > 0f)
                return runMax;
            if (WalkSpeed.TryGetValue(actor, out var walkMax) && walkMax > 0f)
                return walkMax;
        }

        return 0f;
    }

    private PendingOperation Queue(
        string? actor, EntityTaskKind kind, string marker, Vector3? dest, float speed)
    {
        if (actor is { Length: > 0 } && dest is { } d)
            Destinations[actor] = d;
        var task = Tasks.Replace(actor, kind, marker, dest, speed);
        var op = new PendingOperation(task.Id, kind.ToString(), actor, marker);
        if (actor is { Length: > 0 })
            ByActor[actor] = op;
        _next++;
        return op;
    }
}

public sealed class WorldRuntime
{
    public readonly List<ScriptCreate> Creates = [];
    public readonly List<string> Removes = [];
    public readonly List<ThingInstance> Spawned = [];
    public readonly HashSet<string> Dead = new(StringComparer.OrdinalIgnoreCase);
    public readonly List<ScriptTeleport> Teleports = [];
    public readonly List<ScriptLookToThing> LookToThings = [];
    public readonly List<ScriptLookInDirection> Looks = [];
    public readonly Dictionary<string, string> LookTargets = new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, Vector3> Positions = new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, bool> Doors = new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, bool> Drawable = new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, bool> Collide = new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, float> Alpha = new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, string> Flags = new(StringComparer.OrdinalIgnoreCase);
    public readonly List<string> Modes = [];
    public readonly List<(string Item, int Count)> HeroGifts = [];
    public readonly List<(bool Hide, string Mode)> ExtraOps = [];
    public readonly List<(string Verb, string Arg)> RemoveFamily = [];
    public readonly List<ScriptCreate> Effects = [];
    public readonly Dictionary<string, (byte R, byte G, byte B, byte A)> LightColors =
        new(StringComparer.OrdinalIgnoreCase);
    public int ActiveLightScene { get; set; } = -1;
    public bool SwordsUp { get; set; }
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

    public IReadOnlyList<ThingInstance> CollectByName(IEnumerable<ThingInstance> things, string source)
    {
        var hits = new List<ThingInstance>();
        foreach (var thing in things)
        {
            if (thing.ScriptName is not null &&
                (thing.ScriptName.Equals(source, StringComparison.OrdinalIgnoreCase) ||
                 thing.ScriptName.StartsWith(source, StringComparison.OrdinalIgnoreCase)))
            {
                hits.Add(thing);
                continue;
            }

            if (thing.DefinitionType is not null &&
                thing.DefinitionType.Equals(source, StringComparison.OrdinalIgnoreCase))
                hits.Add(thing);
        }

        return hits;
    }

    public IReadOnlyList<ThingInstance> CollectByType(IEnumerable<ThingInstance> things, string type)
    {
        var hits = new List<ThingInstance>();
        if (type.Length == 0)
            return hits;
        foreach (var thing in things)
        {
            if (thing.DefinitionType is not null &&
                thing.DefinitionType.Equals(type, StringComparison.OrdinalIgnoreCase))
                hits.Add(thing);
        }

        return hits;
    }

    public ThingInstance Spawn(string type, string marker, string name, Vector3? pos)
    {
        var props = new Dictionary<string, string>
        {
            ["Marker"] = marker,
            ["Created"] = "1",
        };
        var thing = new ThingInstance
        {
            Kind = "CTC",
            Section = "Thing",
            DefinitionType = type,
            ScriptName = name,
            PositionX = pos?.X,
            PositionY = pos?.Y,
            PositionZ = pos?.Z,
            Properties = props,
        };
        Creates.Add(new ScriptCreate(type, marker, name));
        Spawned.Add(thing);
        Dead.Remove(name);
        if (pos is { } p)
            Positions[name] = p;
        return thing;
    }

    /// <summary>
    /// <c>00CCBBEE</c> <c>vtbl+400</c> effect factory.
    /// Distinct from Create <c>vtbl+364</c>.
    /// </summary>
    public ThingInstance SpawnEffect(string type, string marker, string name, Vector3? pos)
    {
        var thing = Spawn(type, marker, name, pos);
        var props = new Dictionary<string, string>(thing.Properties, StringComparer.OrdinalIgnoreCase)
        {
            ["Effect"] = "1",
        };
        var tagged = new ThingInstance
        {
            Kind = thing.Kind,
            Section = thing.Section,
            DefinitionType = thing.DefinitionType,
            ScriptName = thing.ScriptName,
            PositionX = thing.PositionX,
            PositionY = thing.PositionY,
            PositionZ = thing.PositionZ,
            Properties = props,
        };
        Spawned[^1] = tagged;
        Effects.Add(new ScriptCreate(type, marker, name));
        return tagged;
    }

    /// <summary>
    /// <c>00CCBDB6</c> <c>vtbl+404</c> dummy factory.
    /// Distinct from CreateEffect <c>vtbl+400</c>.
    /// </summary>
    public ThingInstance SpawnDummy(
        string type, string marker, string name, string param, Vector3? pos)
    {
        var thing = Spawn(type, marker, name, pos);
        var props = new Dictionary<string, string>(thing.Properties, StringComparer.OrdinalIgnoreCase)
        {
            ["Dummy"] = "1",
            ["DummyParam"] = param,
        };
        var tagged = new ThingInstance
        {
            Kind = thing.Kind,
            Section = thing.Section,
            DefinitionType = thing.DefinitionType,
            ScriptName = thing.ScriptName,
            PositionX = thing.PositionX,
            PositionY = thing.PositionY,
            PositionZ = thing.PositionZ,
            Properties = props,
        };
        Spawned[^1] = tagged;
        Effects.Add(new ScriptCreate(type, marker, name));
        return tagged;
    }

    public void Destroy(string name)
    {
        if (name.Length == 0)
            return;
        Removes.Add(name);
        Dead.Add(name);
        Spawned.RemoveAll(t =>
            t.ScriptName is not null &&
            t.ScriptName.Equals(name, StringComparison.OrdinalIgnoreCase));
        Positions.Remove(name);
        Drawable.Remove(name);
        Collide.Remove(name);
        Effects.RemoveAll(e =>
            e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// <c>00CD0071</c>: walk extras list, name match,
    /// <c>vtbl+432(item,0,1)</c>. Not world <c>Remove</c>
    /// lookup.
    /// </summary>
    public bool RemoveEffect(string name)
    {
        var hit = Effects.Exists(e =>
            e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (!hit && !Spawned.Exists(t =>
                t.ScriptName is not null &&
                t.ScriptName.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                t.Properties.ContainsKey("Effect")))
            return false;
        Destroy(name);
        return true;
    }

    /// <summary>
    /// <c>00CD1425</c>: black every <c>+84</c> def via
    /// <c>vtbl+2180</c>, then apply scene
    /// <c>+96[index]</c> comma indices with parsed RGB.
    /// </summary>
    public void ApplyLightScene(
        IReadOnlyList<string> defs, IReadOnlyList<string> scenes, int index)
    {
        ActiveLightScene = index;
        var parsed = new List<(string Name, byte R, byte G, byte B)>(defs.Count);
        foreach (var raw in defs)
        {
            if (!TryParseLightDef(raw, out var name, out var r, out var g, out var b))
                continue;
            parsed.Add((name, r, g, b));
            LightColors[name] = (0, 0, 0, 255);
        }

        if ((uint)index >= (uint)scenes.Count)
            return;
        var scene = scenes[index];
        var token = new System.Text.StringBuilder();
        void Flush()
        {
            if (token.Length == 0)
                return;
            if (int.TryParse(token.ToString(), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out var i) &&
                (uint)i < (uint)parsed.Count)
            {
                var light = parsed[i];
                LightColors[light.Name] = (light.R, light.G, light.B, 255);
            }

            token.Clear();
        }

        foreach (var ch in scene)
        {
            if (ch == ',')
                Flush();
            else if (ch != ' ')
                token.Append(ch);
        }

        Flush();
    }

    internal static bool TryParseLightDef(
        string raw, out string name, out byte r, out byte g, out byte b)
    {
        name = "";
        r = 0;
        g = 0;
        b = 0;
        if (raw.Length == 0)
            return false;
        var colon = raw.IndexOf(':');
        if (colon < 0)
        {
            name = raw;
            return true;
        }

        name = raw[..colon];
        var parts = raw[(colon + 1)..].Split(',');
        if (parts.Length > 0 && int.TryParse(parts[0].Trim(), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out var rv))
            r = (byte)rv;
        if (parts.Length > 1 && int.TryParse(parts[1].Trim(), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out var gv))
            g = (byte)gv;
        if (parts.Length > 2 && int.TryParse(parts[2].Trim(), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out var bv))
            b = (byte)bv;
        return name.Length > 0;
    }
}
