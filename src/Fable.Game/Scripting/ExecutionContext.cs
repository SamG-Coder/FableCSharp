using System.Globalization;
using System.Numerics;
using Fable.Formats.Anims;
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
    /// <summary>
    /// <c>00CBEB7E</c> cutscene skip. True
    /// makes WaitForAnimationEvent continue.
    /// </summary>
    public bool Skip { get; set; }
    /// <summary>
    /// <c>00CC5E97</c> always <c>[ebp-59]=1</c>.
    /// Arg ignored.
    /// </summary>
    public bool KeepEntityMap { get; set; }
    /// <summary>
    /// <c>00CC5EF1</c> always <c>[ebp-564]=1</c>.
    /// </summary>
    public bool BlackScreenSubtitles { get; set; }
    public bool YieldEnable { get; set; } = true;
    public bool StayFadedOut { get; set; }
    public bool NoDialogCam { get; set; }
    /// <summary>
    /// <c>[ebp-38]</c>: AskQuestion skips when set.
    /// </summary>
    public bool QuestionLock { get; set; }
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
    private ScriptedCamera? _live;
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
        _live = camera;
        ActiveName = name;
        camera?.UseCamera(things, name);
        // Snap UseCamera vtbl+1648 arrives immediately;
        // vtbl+1672 is idle (Playing=false).
        Busy = camera?.Playing ?? false;
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
        _live = camera;
        camera?.BeginTransition();
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
        _live = camera;
        camera?.BeginTransition();
        Busy = true;
        if (thing is { PositionX: not null })
            camera?.SetLookAt(RegionTravel.PositionOf(thing));
    }

    public void LookAtThing(ScriptedCamera? camera, ThingInstance? thing, string name)
    {
        LookAt = name;
        _live = camera;
        camera?.BeginTransition();
        Busy = true;
        if (thing is { PositionX: not null })
            camera?.SetLookAt(RegionTravel.PositionOf(thing));
    }

    public string LookBetweenA { get; private set; } = "";
    public string LookBetweenB { get; private set; } = "";
    public float LookBetweenDuration { get; private set; }
    public float LookBetweenFov { get; private set; } = -1f;
    public Vector3? LookBetweenCameraPos { get; private set; }
    public string FovMarkerThingA { get; private set; } = "";
    public string FovMarkerThingB { get; private set; } = "";
    public string FovMarkerSelected { get; private set; } = "";
    public float FovMarkerDuration { get; private set; }
    public readonly List<string> FovMarkerNames = [];
    public string RigThingA { get; private set; } = "";
    public string RigThingB { get; private set; } = "";
    public Vector3 RigOffset { get; private set; }
    public float RigSeconds { get; private set; }
    public bool RigActive { get; private set; }

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
        _live = camera;
        camera?.BeginTransition();
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
    /// <c>00CC9436</c>: teleport A to B+offset
    /// (<c>vtbl+1892</c>) then <c>vtbl+1644</c>.
    /// Loop count = arg5 * 15. Blend unread.
    /// </summary>
    public void Rig(
        ScriptedCamera? camera,
        IReadOnlyList<ThingInstance> things,
        ThingInstance? b, string nameA, string nameB,
        Vector3 offset, float seconds)
    {
        RigThingA = nameA;
        RigThingB = nameB;
        RigOffset = offset;
        RigSeconds = seconds;
        RigActive = true;
        _live = camera;
        ActiveName = nameA;
        if (b is not { PositionX: not null })
        {
            camera?.BeginTransition();
            Busy = true;
            return;
        }

        var dest = RegionTravel.PositionOf(b) + offset;
        camera?.UseCamera(things, nameA);
        camera?.SetPosition(dest);
        camera?.BeginTransition();
        Busy = true;
    }

    /// <summary>
    /// <c>00CC9710</c> / <c>00CBF13C</c>: pick one of
    /// four camera markers by XY projection of
    /// (marker-B) onto (A-B). Default flag keeps the
    /// best score (init -2). IsFalse(arg8) assigns
    /// every finite marker so the last wins.
    /// <c>vtbl+1632</c>(pos,pos,B,dur,fov).
    /// </summary>
    public void FovMarkerList(
        ScriptedCamera? camera,
        ThingInstance? a, string nameA,
        ThingInstance? b, string nameB,
        IReadOnlyList<ThingInstance?> markers,
        IReadOnlyList<string> names,
        float duration, float fovDegrees, bool pickBest, bool applyLook = true)
    {
        FovMarkerThingA = nameA;
        FovMarkerThingB = nameB;
        FovMarkerDuration = duration;
        FovMarkerNames.Clear();
        FovMarkerNames.AddRange(names);
        var selected = PickFovMarker(a, b, markers, names, pickBest);
        FovMarkerSelected = selected.Name;
        if (!applyLook || (selected.Thing is null && selected.Name.Length == 0))
            return;
        LookBetween(
            camera,
            selected.Thing, selected.Name,
            selected.Thing, selected.Name,
            default, default, duration, fovDegrees);
    }

    internal static (ThingInstance? Thing, string Name) PickFovMarker(
        ThingInstance? a, ThingInstance? b,
        IReadOnlyList<ThingInstance?> markers,
        IReadOnlyList<string> names,
        bool pickBest)
    {
        if (a is not { PositionX: not null } ||
            b is not { PositionX: not null })
            return (null, "");
        var posA = RegionTravel.PositionOf(a);
        var posB = RegionTravel.PositionOf(b);
        var dir = new Vector2(posA.X - posB.X, posA.Y - posB.Y);
        dir = UnitXy(dir);
        var best = -2f;
        ThingInstance? chosen = null;
        var chosenName = "";
        var count = Math.Min(markers.Count, names.Count);
        for (var i = 0; i < count; i++)
        {
            var marker = markers[i];
            if (marker is not { PositionX: not null })
                continue;
            var pos = RegionTravel.PositionOf(marker);
            var delta = UnitXy(new Vector2(pos.X - posB.X, pos.Y - posB.Y));
            var score = delta.X * dir.X + delta.Y * dir.Y;
            if (!pickBest || score > best)
            {
                best = score;
                chosen = marker;
                chosenName = names[i];
            }
        }

        return (chosen, chosenName);
    }

    private static Vector2 UnitXy(Vector2 v) =>
        v.LengthSquared() > 0.0001f ? Vector2.Normalize(v) : v;

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
        _live = camera;
        camera?.Reset();
        ActiveName = camera?.ActiveName ?? "";
        LookAt = "";
        Busy = false;
        if (WaitOp is not null)
            WaitOp.Complete = true;
    }

    public PendingOperation? WaitOp { get; private set; }
    public PendingOperation? MessageWaitOp { get; private set; }
    public string MessageCamera { get; private set; } = "";
    public bool MessageBusy { get; private set; }
    private int _waitSerial;

    /// <summary>
    /// <c>00CCA58F</c> leftover-polls
    /// <c>vtbl+1672</c> on the live camera.
    /// Idle (snap / no transition) continues.
    /// Busy leftover <c>vtbl+28</c> then re-poll.
    /// </summary>
    public PendingOperation WaitForCamera(ScriptedCamera? camera = null)
    {
        if (camera is not null)
            _live = camera;
        var playing = _live?.Playing ?? Busy;
        Busy = playing;
        if (!playing)
        {
            return new PendingOperation($"cam-{++_waitSerial}", "WaitForCamera", null, ActiveName)
            {
                Complete = true,
            };
        }

        WaitOp = new PendingOperation($"cam-{++_waitSerial}", "WaitForCamera", null, ActiveName);
        return WaitOp;
    }

    /// <summary>
    /// <c>00CCFF91</c> leftover-polls
    /// <c>vtbl+2316(name)</c> until true.
    /// Idle (no message camera) returns complete.
    /// </summary>
    public PendingOperation WaitForMessage(string name)
    {
        MessageCamera = name;
        if (!MessageBusy)
        {
            return new PendingOperation($"msgcam-{++_waitSerial}", "WaitForMessageCamera", null, name)
            {
                Complete = true,
            };
        }

        MessageWaitOp = new PendingOperation($"msgcam-{++_waitSerial}", "WaitForMessageCamera", null, name);
        return MessageWaitOp;
    }

    public void BeginMessageCamera(string name)
    {
        MessageCamera = name;
        MessageBusy = true;
    }

    public void CompleteWait()
    {
        Busy = false;
        MessageBusy = false;
        _live?.EndTransition();
        if (WaitOp is not null)
            WaitOp.Complete = true;
        if (MessageWaitOp is not null)
            MessageWaitOp.Complete = true;
    }

    public void Preload(string name)
    {
        if (name.Length == 0 || _preloaded.Contains(name, StringComparer.OrdinalIgnoreCase))
            return;
        _preloaded.Add(name);
    }

    /// <summary>
    /// <c>00CC7A7C</c>: <c>vtbl+1612(1)</c> then
    /// <c>vtbl+1648(name,0,0,-1,0,-1)</c> then
    /// <c>vtbl+1612(0)</c>. Not DoCameraPreloading.
    /// </summary>
    public int CameraPreloadGateVtbl { get; private set; }
    public int CameraPreloadBindVtbl { get; private set; }

    public void CameraPreload(string name)
    {
        CameraPreloadGateVtbl = 1612;
        CameraPreloadBindVtbl = 1648;
        Preload(name);
    }
}

public sealed class AudioRuntime
{
    public string? Music { get; private set; }
    public string? MusicResource { get; private set; }
    public string? Sound { get; private set; }
    public bool Muted { get; private set; }
    public readonly List<ScriptAudioInstance> Instances = [];
    public readonly List<string> Cached = [];
    public string Theme { get; private set; } = "";
    public float ThemeParam { get; private set; }
    public bool ThemeFlag { get; private set; }
    public bool ThemeReset { get; private set; }

    public void PlayMusic(string track, string? resource = null)
    {
        Music = track;
        MusicResource = resource;
    }

    public void CacheMusic(string track)
    {
        if (track.Length == 0)
            return;
        if (!Cached.Contains(track, StringComparer.OrdinalIgnoreCase))
            Cached.Add(track);
    }

    public ScriptAudioInstance PlaySound(
        string name, string? source, bool spatial,
        bool criteria = false, int vtbl = 0, string? resource = null)
    {
        Sound = name;
        var inst = new ScriptAudioInstance(name, source, spatial, criteria, vtbl, resource);
        Instances.Add(inst);
        return inst;
    }

    public void StopMusic()
    {
        Music = "";
        MusicResource = null;
    }

    /// <summary>
    /// <c>00CCFA8B</c>: <c>RESET</c> is
    /// <c>vtbl+2628(param)</c>, else
    /// <c>vtbl+2624(name,param)</c>.
    /// </summary>
    public void UseTheme(string name, float param, bool flag, bool reset)
    {
        ThemeParam = param;
        ThemeFlag = flag;
        ThemeReset = reset;
        Theme = reset ? "" : name;
    }

    public void Mute(bool mute) => Muted = mute;
}

public readonly record struct ScriptAudioInstance(
    string Name,
    string? Source,
    bool Spatial,
    bool Criteria = false,
    int Vtbl = 0,
    string? Resource = null);

public sealed class DialogueRuntime
{
    public readonly List<ScriptSpeech> Speeches = [];
    public readonly List<ScriptInteractiveSpeech> Interactive = [];
    public readonly List<ScriptDialogSpeech> Dialogs = [];
    public readonly List<ScriptDialogAdSpeech> DialogAds = [];
    /// <summary>
    /// <c>00CC2C06</c> <c>vtbl+52</c> with a
    /// concatenated data key. Not Speak listener+text.
    /// </summary>
    public readonly List<(string Actor, string Key, int Mode)> DataSpeaks = [];
    /// <summary>
    /// <c>00CC2D42</c> group lines
    /// <c>prefix_10</c>, <c>prefix_20</c>, …
    /// via <c>vtbl+1464</c>.
    /// </summary>
    public readonly List<string> GroupLines = [];
    public int GroupSpeakVtbl { get; private set; }
    public DialogueSession? Session { get; private set; }
    public int ActiveCount { get; private set; }
    public bool HasActive => ActiveCount > 0 || Session is { Active: true };
    public PendingOperation? WaitOp { get; private set; }
    private int _waitSerial;

    public void Speak(string? actor, string target, string text, int mode, bool hold = false,
        string? body = null)
    {
        Speeches.Add(new ScriptSpeech(actor, target, text, mode));
        Open(actor, target, text, mode, "Speak", handle: false, hold, body);
    }

    public PendingOperation InteractiveSpeak(
        string? actor, string listener, string prompt, bool wait, string response,
        string? body = null)
    {
        Interactive.Add(new ScriptInteractiveSpeech(actor, listener, prompt, wait, response));
        Open(actor, listener, prompt, 0, "InteractiveSpeak", handle: true, hold: false, body);
        var op = new PendingOperation($"ispeak-{++_waitSerial}", "InteractiveSpeak", actor, prompt);
        if (wait)
            WaitOp = op;
        return op;
    }

    /// <summary>
    /// <c>00CC2CCD</c>: arg0+1+2 required;
    /// vtbl+1456(handle,1,1); 1460; atoi count;
    /// each i: prefix + "_" + 10*(i+1) via 1464.
    /// Session handle like InteractiveSpeak.
    /// Voice/group body UNREAD.
    /// </summary>
    public PendingOperation InteractiveSpeakGroup(
        string? actor, string listener, string prefix, int count, string? body = null)
    {
        GroupLines.Clear();
        GroupSpeakVtbl = 1464;
        for (var i = 1; i <= count; i++)
            GroupLines.Add($"{prefix}_{i * 10}");
        var first = count > 0 ? GroupLines[0] : prefix;
        Open(actor, listener, first, 0, "InteractiveSpeakGroup", handle: true, hold: false, body);
        var op = new PendingOperation($"isg-{++_waitSerial}", "InteractiveSpeakGroup", actor, prefix);
        WaitOp = op;
        return op;
    }

    public void CompleteWait()
    {
        if (WaitOp is not null)
            WaitOp.Complete = true;
        if (Session is { Verb: "AskQuestion", Answer: null })
            Session.Answer = 0;
        Dismiss();
    }

    /// <summary>
    /// <c>00CC656B</c> leftover session poll
    /// <c>vtbl+1472</c>. No handle → continue.
    /// Handle → one leftover then next line.
    /// </summary>
    public PendingOperation WaitActive()
    {
        if (WaitOp is { Complete: false } existing)
            return existing;
        WaitOp = new PendingOperation($"dialog-{++_waitSerial}", "WaitActiveDialog", null, "active");
        if (Session is not { HasHandle: true, Active: true })
            WaitOp.Complete = true;
        return WaitOp;
    }

    /// <summary>
    /// <c>00CC5FD4</c>: <c>vtbl+1468(handle,1)</c> then
    /// <c>vtbl+456(question,yes,no,caption,1)</c>.
    /// Poll <c>vtbl+156</c> until <c>&gt;=0</c>.
    /// </summary>
    public PendingOperation AskQuestion(string text, string yes, string no, string? body = null)
    {
        Open(null, "", text, 0, "AskQuestion", handle: true, hold: false, body);
        if (Session is not null)
        {
            Session.YesLabel = yes;
            Session.NoLabel = no;
        }

        WaitOp = new PendingOperation($"ask-{++_waitSerial}", "AskQuestion", null, text);
        return WaitOp;
    }

    /// <summary>
    /// <c>vtbl+156</c> result: <c>&lt;0</c> wait,
    /// <c>0</c> no, <c>!=0</c> yes.
    /// </summary>
    public void Answer(int esi)
    {
        if (Session is not null)
            Session.Answer = esi == 0 ? 0 : 1;
        CompleteWait();
    }

    public void DialogSpeak(string? actor, string listener, string text, string? body = null)
    {
        Dialogs.Add(new ScriptDialogSpeech(actor, listener, text));
        Open(actor, listener, text, 0, "DialogSpeak", handle: true, hold: false, body);
    }

    /// <summary>
    /// <c>00CC2991</c>: concat key then
    /// <c>vtbl+52(handle,key,mode,0,1,0)</c>
    /// leftover poll <c>vtbl+104</c>.
    /// Voice table UNREAD.
    /// </summary>
    public void DataSpeak(string? actor, string key, int mode, string? body = null)
    {
        DataSpeaks.Add((actor ?? "", key, mode));
        Open(actor, "", key, mode, "DataSpeak", handle: false, hold: false, body);
    }

    public void DialogAdSpeak(string? actor, string target, string text, int mode)
    {
        DialogAds.Add(new ScriptDialogAdSpeech(actor, target, text, mode));
        Open(actor, target, text, mode, "DialogadSpeak", handle: false, hold: false, null);
    }

    public void Dismiss()
    {
        if (ActiveCount > 0)
            ActiveCount--;
        if (Session is not null)
            Session.Active = ActiveCount > 0;
        if (Session is { HasHandle: true } && ActiveCount == 0)
            Session.HasHandle = false;
    }

    private void Open(string? speaker, string listener, string text, int mode, string verb,
        bool handle, bool hold, string? body)
    {
        Session = new DialogueSession(speaker, listener, text, mode, verb)
        {
            HasHandle = handle,
            Hold = hold,
            ResolvedBody = body ?? "",
        };
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
    public bool HasHandle { get; set; }
    public bool Hold { get; set; }
    public string ResolvedBody { get; set; } = "";
    public string YesLabel { get; set; } = "";
    public string NoLabel { get; set; } = "";
    /// <summary>
    /// <c>[ebp-180]</c>: <c>vtbl+156</c> <c>esi!=0</c>.
    /// </summary>
    public int? Answer { get; set; }

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
    /// <summary>
    /// <c>00CC140E</c> empty/basic <c>vtbl+2148</c>
    /// else <c>vtbl+2144(handle,name)</c>.
    /// </summary>
    public readonly List<(string Actor, string Name, int Vtbl)> Preloads = [];
    public readonly Dictionary<string, PendingOperation> ByActor =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, AnimationState> States =
        new(StringComparer.OrdinalIgnoreCase);
    public EntityTaskQueue Tasks { get; } = new();
    /// <summary>
    /// <c>005DC340</c> 20-byte name table at
    /// appearance+52. Miss falls through to
    /// <c>DEFAULT</c> (<c>00662A00</c>).
    /// </summary>
    public readonly Dictionary<string, AnimationClipRecord> Clips =
        new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// Thing <c>+68</c> component list walked
    /// by <c>004C7470</c>. Slot is 8 bytes.
    /// Type 90 is <c>CTCAnimationComplex</c>.
    /// </summary>
    public readonly Dictionary<string, List<AnimationComponent>> Components =
        new(StringComparer.OrdinalIgnoreCase);
    private int _next;

    public const int AnimationComplexTypeId = 90;
    public const uint ThingPlayVtbl72 = 0x004C7470;
    public const uint ComponentPlus68 = 0x00686920;
    public const uint LookupFn = 0x00662A00;
    public const uint RequestFn = 0x0070C050;
    public const uint InnerPlayFn = 0x0070D580;
    public const uint InnerGetter = 0x0070B460;
    public const int DefaultRequestMode = 6;
    public const string DefaultClipName = "DEFAULT";

    /// <summary>
    /// <c>004C7470</c> requires a type-90
    /// slot. <c>+8==0</c> so <c>+68</c> runs.
    /// </summary>
    public AnimationComponent EnsureComplex(string actor)
    {
        if (!Components.TryGetValue(actor, out var list))
        {
            list = [];
            Components[actor] = list;
        }

        var hit = list.Find(c => c.TypeId == AnimationComplexTypeId);
        if (hit is not null)
            return hit;
        hit = new AnimationComponent(AnimationComplexTypeId, 0, true);
        list.Add(hit);
        return hit;
    }

    /// <summary>
    /// <c>004C7470</c>: walk <c>[this+68..+72)</c>.
    /// Skip <c>[comp+8]!=0</c>. Else
    /// <c>vtbl+68(name)</c>. First-seen
    /// <c>00686920</c> is <c>al=1; ret 4</c>.
    /// Empty list returns 1.
    /// </summary>
    public bool WalkPlay(string actor, string name)
    {
        if (!Components.TryGetValue(actor, out var list) || list.Count == 0)
            return true;
        foreach (var comp in list)
        {
            if (comp.Disabled)
                continue;
            if (!comp.AcceptName(name))
                return false;
        }

        return true;
    }

    /// <summary>
    /// <c>00662A00</c>: <c>005DC2E0</c> contains
    /// name at appearance+52; else <c>DEFAULT</c>.
    /// </summary>
    public AnimationClipRecord LookupClip(string name)
    {
        if (name.Length > 0 && Clips.TryGetValue(name, out var hit))
            return hit;
        if (Clips.TryGetValue(DefaultClipName, out var fallback))
            return fallback;
        return new AnimationClipRecord(DefaultClipName, 1f);
    }

    /// <summary>
    /// <c>0070C050</c> request then
    /// <c>0070D580</c> on the type-90 inner
    /// (<c>0070B460</c> <c>[comp+12]</c>).
    /// Mode &lt;=0 skips the time walk
    /// (<c>jle 0070D71D</c>); channel
    /// duration is <c>[clip+44]/max(mode,1)</c>.
    /// </summary>
    public void ApplyInner(AnimationState state, AnimationClipRecord clip, int mode)
    {
        var playMode = mode <= 0 ? 1 : mode;
        state.ClipKey = clip.Name;
        state.RequestMode = playMode;
        state.Playing = true;
        state.InnerApplied = true;
        state.ChannelArmed = true;
        state.PlayTime = 0f;
        state.Duration = clip.Duration / playMode;
        state.Step = 1f / playMode;
    }

    /// <summary>
    /// <c>00CC140E</c>: empty or BASIC → 2148;
    /// else 2144(handle,name). Not PlayAnimation 72.
    /// Clip cache UNREAD.
    /// </summary>
    public void Preload(string? actor, string name)
    {
        var basic = name.Length == 0 || ScriptLine.TokenMatches(name, "basic");
        Preloads.Add((actor ?? "", basic ? "basic" : name, basic ? 2148 : 2144));
    }

    public PendingOperation Play(
        string? actor, string name, bool f1, bool f2, bool f3, bool f4, bool f5)
    {
        Plays.Add(new ScriptAnimation(actor, name, f1, f2, f3, f4, f5));
        if (actor is { Length: > 0 })
        {
            EnsureComplex(actor);
            var accepted = WalkPlay(actor, name);
            var state = new AnimationState(actor, name, false, f1, f2, f3, f4, f5)
            {
                Walked = true,
                Plus68Accepted = accepted,
            };
            var clip = LookupClip(name);
            ApplyInner(state, clip, clip.Name.Equals(DefaultClipName, StringComparison.OrdinalIgnoreCase)
                ? DefaultRequestMode
                : 1);
            States[actor] = state;
        }
        var kind = EntityTaskKind.Animate;
        var task = Tasks.Replace(actor, kind, name, null, 0f);
        var op = new PendingOperation(task.Id, "PlayAnimation", actor, name);
        if (actor is { Length: > 0 })
            ByActor[actor] = op;
        _next++;
        return op;
    }

    /// <summary>
    /// <c>00CC1788</c> <c>vtbl+80</c>. Arg1 is the
    /// <c>0099E7F0</c> loop integer, not a flag.
    /// Clip pose unread.
    /// </summary>
    public PendingOperation PlayLoop(
        string? actor, string name, int loops,
        bool f1, bool f2, bool f3, bool f4, bool f5)
    {
        Plays.Add(new ScriptAnimation(actor, name, f1, f2, f3, f4, f5));
        if (actor is { Length: > 0 })
            States[actor] = new AnimationState(actor, name, true, f1, f2, f3, f4, f5) { Loops = loops };
        var task = Tasks.Replace(actor, EntityTaskKind.LoopAnimate, name, null, 0f);
        var op = new PendingOperation(task.Id, "PlayLoopingAnim", actor, name);
        if (actor is { Length: > 0 })
            ByActor[actor] = op;
        _next++;
        return op;
    }

    /// <summary>
    /// <c>00CC74DE</c> <c>vtbl+1948(thing,name,IsTrue)</c>.
    /// Always preceded by <c>vtbl+2048(thing,2)</c>.
    /// Not PlayAnimation 72.
    /// </summary>
    public PendingOperation PlayObject(string? actor, string name, bool flag)
    {
        Plays.Add(new ScriptAnimation(actor, name, flag, false, false, false, false));
        if (actor is { Length: > 0 })
        {
            var state = new AnimationState(actor, name, false, flag, false, false, false, false)
            {
                RequestMode = 1948,
            };
            BeginInnerPlay(state, name, 1948);
            States[actor] = state;
        }

        var task = Tasks.Replace(actor, EntityTaskKind.ObjectAnimate, name, null, 0f);
        var op = new PendingOperation(task.Id, "PlayObjectAnim", actor, name);
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

    /// <summary>
    /// <c>00CC4252</c> leftover poll
    /// <c>004AAF60</c> → inner <c>vtbl+236</c>.
    /// Event table UNREAD.
    /// </summary>
    public readonly Dictionary<string, string> EventWaits =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, int> EventWaitVtbl =
        new(StringComparer.OrdinalIgnoreCase);

    public PendingOperation WaitEvent(string? actor, string ev)
    {
        var key = actor ?? "";
        if (key.Length > 0)
        {
            EventWaits[key] = ev;
            EventWaitVtbl[key] = 236;
        }

        if (Current(actor) is { } playing)
            return playing;
        var op = new PendingOperation($"ev-{++_next}", "WaitForAnimationEvent", actor, ev);
        if (key.Length > 0)
            ByActor[key] = op;
        return op;
    }

    /// <summary>
    /// <c>0070C050</c> request then <c>0070D580</c>.
    /// Appearance DEFAULT <c>0070B4D0</c> /
    /// <c>005B37F7</c> uses mode 6. Script
    /// <c>vtbl+72</c> is <c>004C7470</c>; the
    /// type-90 inner is the same
    /// <c>0070B460</c> object.
    /// </summary>
    public void BeginInnerPlay(AnimationState state, string clip, int mode)
    {
        if (!Clips.ContainsKey(clip) &&
            !clip.Equals(DefaultClipName, StringComparison.OrdinalIgnoreCase))
            Clips[clip] = new AnimationClipRecord(clip, 1f);
        ApplyInner(state, LookupClip(clip), mode);
    }

    /// <summary>
    /// <c>0070D580</c> <c>[esi+140]=1/mode</c>
    /// then <c>[channel+64]+=step</c>.
    /// Duration from <c>[clip+44]/mode</c>.
    /// Clip keyframes unread — PALSKIN stays
    /// bind pose until sampled.
    /// </summary>
    public void Tick(float dt, WorldRuntime world)
    {
        Tasks.Tick(dt, world);
        foreach (var state in States.Values)
        {
            if (!state.Playing)
                continue;
            var step = state.Step > 0f ? state.Step : Math.Max(dt, 0f);
            state.PlayTime += step;
            if (state.Looping)
                continue;
            if (state.Duration > 0f && state.PlayTime + 1e-6f >= state.Duration)
            {
                state.Playing = false;
                Tasks.Current(state.Actor)?.MarkComplete();
                if (ByActor.TryGetValue(state.Actor, out var op))
                    op.Complete = true;
            }
        }
    }

    public IReadOnlyDictionary<string, string> PoseNames()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (actor, state) in States)
        {
            if (state.ClipKey.Length > 0)
                map[actor] = state.ClipKey;
            else if (state.Name.Length > 0)
                map[actor] = state.Name;
        }

        return map;
    }

    /// <summary>
    /// <c>005B37F7</c> <c>DEFAULT</c> via
    /// <c>005DC340</c> 20-byte name table then
    /// <c>0070C050</c> → <c>0070B460</c> →
    /// <c>0070D580</c>.
    /// </summary>
    public void PlayAppearanceDefault(string actor)
    {
        var state = new AnimationState(actor, "DEFAULT", true, false, false, false, true, false);
        BeginInnerPlay(state, "DEFAULT", 6);
        States[actor] = state;
    }
}

public sealed class AnimationState
{
    public string Actor { get; }
    public string Name { get; }
    public bool Looping { get; }
    public bool F1 { get; }
    public bool F2 { get; }
    public bool F3 { get; }
    public bool F4 { get; }
    public bool F5 { get; }
    public int Loops { get; set; }
    public string ClipKey { get; set; } = "";
    public int RequestMode { get; set; }
    public bool Playing { get; set; }
    public float PlayTime { get; set; }
    public float Duration { get; set; }
    public float Step { get; set; }
    public bool Walked { get; set; }
    public bool Plus68Accepted { get; set; }
    public bool InnerApplied { get; set; }
    public bool ChannelArmed { get; set; }

    public AnimationState(
        string actor, string name, bool looping,
        bool f1, bool f2, bool f3, bool f4, bool f5)
    {
        Actor = actor;
        Name = name;
        Looping = looping;
        F1 = f1;
        F2 = f2;
        F3 = f3;
        F4 = f4;
        F5 = f5;
    }
}

/// <summary>
/// <c>004C7470</c> 8-byte slot.
/// Type 90 = <c>0070B3C0</c>.
/// <c>+68</c> first-seen is
/// <c>00686920</c> accept.
/// </summary>
public sealed class AnimationComponent
{
    public int TypeId { get; }
    public bool Disabled { get; }
    public bool Plus68Returns { get; }

    public AnimationComponent(int typeId, int plus8, bool plus68Returns)
    {
        TypeId = typeId;
        Disabled = plus8 != 0;
        Plus68Returns = plus68Returns;
    }

    /// <summary>
    /// <c>00686920</c> <c>mov al,1; ret 4</c>.
    /// Name is ignored.
    /// </summary>
    public bool AcceptName(string name) => Plus68Returns;
}

/// <summary>
/// <c>005DC340</c> 20-byte table entry.
/// Duration is <c>[clip+44]</c> in
/// <c>0070D580</c>. Unread clips use 1.
/// </summary>
public sealed class AnimationClipRecord
{
    public string Name { get; }
    public float Duration { get; }
    public XSeqFile? Sequence { get; }

    public AnimationClipRecord(string name, float duration, XSeqFile? sequence = null)
    {
        Name = name;
        Duration = duration > 0f ? duration : 1f;
        Sequence = sequence;
    }
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
    public readonly Dictionary<string, Vector3> Destinations =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly HashSet<string> Moving = new(StringComparer.OrdinalIgnoreCase);
    public EntityTaskQueue Tasks { get; } = new();
    private int _next;

    /// <summary>
    /// <c>006A9960</c> / <c>006A5D90</c>: copy gait
    /// speed and <c>or [this+146],2</c>. Do not warp.
    /// Missing start stays at the actor thing, else 0.
    /// </summary>
    public void SeedStart(string? actor, ThingInstance? thing, WorldRuntime world)
    {
        if (actor is not { Length: > 0 })
            return;
        Moving.Add(actor);
        if (world.Positions.ContainsKey(actor))
            return;
        if (thing is { PositionX: not null })
            world.Positions[actor] = RegionTravel.PositionOf(thing);
        else
            world.Positions[actor] = Vector3.Zero;
    }

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

    /// <summary>
    /// <c>00CC4350</c> actor <c>vtbl+24(route,gait,IsTrue,0)</c>.
    /// Gait 0 default, run=1, sneak=2. Not WalkTo 16.
    /// Route spline UNREAD.
    /// </summary>
    public readonly Dictionary<string, string> NavRoutes =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, int> NavGaits =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, int> NavVtbl =
        new(StringComparer.OrdinalIgnoreCase);

    public PendingOperation FollowNav(string? actor, string route, int gait, bool wait)
    {
        var key = actor ?? "";
        if (key.Length > 0)
        {
            NavRoutes[key] = route;
            NavGaits[key] = gait;
            NavVtbl[key] = 24;
        }

        _ = wait;
        return Queue(actor, EntityTaskKind.NavRoute, route, null, gait switch
        {
            1 => ResolveSpeed(actor, 0f, true),
            _ => ResolveSpeed(actor, 0f, false),
        });
    }

    /// <summary>
    /// <c>00CC57F7</c> / <c>00CC5A8D</c>: lerp
    /// src→dest over atoi count (default 100).
    /// Instant apply lands on dest.
    /// </summary>
    public PendingOperation Slide(
        string? actor, string from, string to, Vector3 src, Vector3 dest, int count, bool wait)
    {
        if (actor is { Length: > 0 })
        {
            Destinations[actor] = dest;
            Moving.Add(actor);
        }

        if (!wait || count <= 0)
        {
            if (actor is { Length: > 0 })
            {
                // World set by caller via Teleport.
            }

            var done = new PendingOperation($"slide-{++_next}", "SlideTeleport", actor, to);
            done.Complete = true;
            return done;
        }

        var task = Tasks.Replace(actor, EntityTaskKind.Slide, to, dest, 0f);
        task.Source = src;
        task.SlideCount = count;
        task.SlideIndex = 0;
        var op = new PendingOperation(task.Id, "SlideTeleport", actor, to);
        if (actor is { Length: > 0 })
            ByActor[actor] = op;
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

    public void Tick(float dt, WorldRuntime world)
    {
        Tasks.Tick(dt, world);
        List<string>? done = null;
        foreach (var actor in Moving)
        {
            var task = Tasks.Current(actor);
            if (task is { Complete: true } && ByActor.TryGetValue(actor, out var op))
                op.Complete = true;
            if (task is null || task.Complete)
                (done ??= []).Add(actor);
        }

        if (done is null)
            return;
        foreach (var actor in done)
            Moving.Remove(actor);
    }

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

        return 0.3f;
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

public sealed class HeroExpression
{
    public string Name { get; }
    public int Param { get; set; }
    public bool Flag { get; set; }

    public HeroExpression(string name, int param, bool flag)
    {
        Name = name;
        Param = param;
        Flag = flag;
    }
}

public sealed class HeroInventoryItem
{
    public string Name { get; }
    public int Count { get; set; }
    public int Extra { get; set; }
    public bool Silent { get; set; }

    public HeroInventoryItem(string name, int count, int extra, bool silent)
    {
        Name = name;
        Count = count;
        Extra = extra;
        Silent = silent;
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
    /// <summary>
    /// <c>00CC3D36</c> <c>vtbl+1996(handle,!IsFalse)</c>.
    /// Default 1. Not LookToThing 1992.
    /// </summary>
    public readonly Dictionary<string, bool> LookToCamera =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, Vector3> Positions = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// <c>00CC409B</c> leftover until
    /// <c>00CBE2FF</c> <c>dist^2 &lt; r^2</c>.
    /// </summary>
    public readonly Dictionary<string, string> UnderRadiusTargets =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, float> UnderRadius =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, PendingOperation> UnderRadiusOps =
        new(StringComparer.OrdinalIgnoreCase);
    private int _radiusSerial;
    public readonly Dictionary<string, bool> Doors = new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, bool> Chests = new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, bool> Drawable = new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, bool> Collide = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// <c>00CC45E5</c> <c>vtbl+32(handle,actor,level)</c>.
    /// Default 4; HIGH=3; MEDIUM=2. No LOW token.
    /// </summary>
    public readonly Dictionary<string, int> AILevels =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, int> AILevelVtbl =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, float> Alpha = new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, string> Flags = new(StringComparer.OrdinalIgnoreCase);
    public readonly List<string> Modes = [];
    public readonly List<(string Item, int Count)> HeroGifts = [];
    public readonly List<HeroInventoryItem> Inventory = [];
    public readonly List<string> TakenObjects = [];
    public readonly List<(bool Hide, string Mode)> ExtraOps = [];
    public readonly List<(string Verb, string Arg)> RemoveFamily = [];
    public readonly List<ScriptCreate> Effects = [];
    public readonly List<ScriptCreate> Lights = [];
    public readonly Dictionary<string, (byte R, byte G, byte B, byte A)> LightColors =
        new(StringComparer.OrdinalIgnoreCase);
    public int ActiveLightScene { get; set; } = -1;
    /// <summary>
    /// <c>00CC93D5</c> <c>vtbl+520</c> always runs.
    /// </summary>
    public bool SwordsUp { get; set; }
    /// <summary>
    /// <c>TRUE</c> path classifies via <c>vtbl+788/792</c>
    /// as MELEE or RANGED. Bodies unread.
    /// </summary>
    public string SwordClass { get; set; } = "";
    public bool SwordClassifyRequested { get; set; }
    /// <summary>
    /// <c>00CC83F1</c> <c>vtbl+504(delta)</c>.
    /// GiveGold ensures gold is at least the requested
    /// amount (requested − already-have).
    /// </summary>
    public int HeroGold { get; set; }
    public readonly Dictionary<string, string> Sheathed =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, int> SheatheVtbl =
        new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// <c>00CC21CB</c> <c>vtbl+892(actor,item,IsTrue)</c>.
    /// Distinct from <c>PutInHeroHands</c> 572/568.
    /// </summary>
    public readonly Dictionary<string, string> HeldInHand =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, bool> HeldInHandFlag =
        new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// <c>00CC22AE</c> <c>vtbl+1060(actor,atof,0)</c>.
    /// Distinct from <c>GiveHeroHealth</c> 1052.
    /// </summary>
    public readonly Dictionary<string, float> Health =
        new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// <c>00CC12B7</c> <c>vtbl+1984(actor,!IsFalse)</c>.
    /// Empty arg stays 1. Not SetBound 1976.
    /// </summary>
    public readonly Dictionary<string, bool> Scared =
        new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// <c>00CC1360</c> <c>vtbl+1988(actor,!IsFalse)</c>.
    /// Empty arg stays 1. Not SetScared 1984.
    /// </summary>
    public readonly Dictionary<string, bool> Drunk =
        new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// <c>00CC4AC3</c> <c>004AA9A0</c> handle
    /// <c>vtbl+28</c> then <c>vtbl+1892</c>.
    /// SetHomePosThing body UNREAD.
    /// </summary>
    public readonly Dictionary<string, Vector3> HomePos =
        new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// <c>00CC11FD</c> <c>vtbl+1976(actor,!IsFalse)</c>.
    /// Arg0 required. Not SetScared 1984.
    /// </summary>
    public readonly Dictionary<string, bool> Bound =
        new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// <c>00CC1C82</c> <c>vtbl+2068(actor,!IsFalse,1)</c>.
    /// Arg0 required. Extra imm 1. Not SetBound 1976.
    /// </summary>
    public readonly Dictionary<string, bool> Killable =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, int> KillableExtra =
        new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// <c>00CC1144</c> <c>vtbl+3376(actor,IsTrue)</c>.
    /// Default 0. Not SetBound IsFalse/1976.
    /// </summary>
    public readonly Dictionary<string, bool> Pushable =
        new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// <c>00CC10A6</c> always <c>vtbl+2064(actor,0)</c>
    /// then extras <c>008ADF90</c>. Arg ignored.
    /// </summary>
    public readonly Dictionary<string, bool> Damageable =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, int> DamageableVtbl =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly List<string> ExtrasAppended = [];
    /// <summary>
    /// <c>00CC1008</c> always <c>vtbl+1832(actor,0)</c>
    /// then extras <c>008ADF90</c>. Arg ignored.
    /// </summary>
    public readonly Dictionary<string, bool> Attackable =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, int> AttackableVtbl =
        new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// <c>00CC0F7E</c> unary <c>vtbl+1980(actor)</c>.
    /// Arg ignored. No extras. Not SetAttackable 1832.
    /// </summary>
    public readonly HashSet<string> Freed =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, int> SetFreeVtbl =
        new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// <c>00CC4663</c> <c>00CD2770</c> drops
    /// <c>actor+8</c> then zeros it. Not SetFree.
    /// </summary>
    /// <summary>
    /// <c>00CC5F4E</c> <c>vtbl+1604(1)</c> unless
    /// arg0 present and not IsTrue (then 0).
    /// </summary>
    public bool HideBodies { get; private set; }
    public int HideBodiesVtbl { get; private set; }
    /// <summary>
    /// <c>00CC1EE1</c> <c>vtbl+2388(actor,target)</c>.
    /// Not AILevel 32.
    /// </summary>
    public readonly Dictionary<string, string> FightTargets =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, int> FightVtbl =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly HashSet<string> Released =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, uint> ReleaseFn =
        new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// <c>00CC68ED</c> <c>vtbl+924</c> then
    /// <c>HeroFollower0</c> bind. Not TeleportFollowers 956.
    /// </summary>
    public bool FollowersReturned { get; private set; }
    public int FollowerReturnVtbl { get; private set; }
    public readonly List<string> Followers = [];
    /// <summary>
    /// <c>00CC6A2E</c> <c>vtbl+956</c>. TRUE wraps
    /// fade 1492/1504 then FadeIn 1496.
    /// </summary>
    public bool FollowersTeleported { get; private set; }
    public bool FollowerTeleportFade { get; private set; }
    public int FollowerTeleportVtbl { get; private set; }
    /// <summary>
    /// <c>00CC4B7E</c> <c>vtbl+1916(actor,atoi)</c>.
    /// Signed seed. Not a boolean flag.
    /// </summary>
    public readonly Dictionary<string, int> AppearanceSeed =
        new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// <c>00CC8094</c> <c>vtbl+2324(thing,IsTrue,extra)</c>.
    /// Default 0. Not SetScared default-1.
    /// </summary>
    public readonly Dictionary<string, bool> Conscious =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, int> ConsciousVtbl =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, string> ConsciousExtra =
        new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// <c>00CC7B24</c> <c>vtbl+2048(thing,mode)</c>.
    /// TRUE → 1, FALSE → 2. Not a 0/1 boolean.
    /// </summary>
    public readonly Dictionary<string, int> PauseModes =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, int> PauseVtbl =
        new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// <c>00CC7BEB</c> <c>vtbl+2128(thing,!IsFalse)</c>.
    /// Default 1. Not PauseThing modes 1/2.
    /// </summary>
    public readonly Dictionary<string, bool> GravityOn =
        new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, int> GravityVtbl =
        new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// <c>00CC828C</c> <c>vtbl+896(HERO,arg0,arg1)</c>.
    /// Always hero handle. Args are raw strings.
    /// </summary>
    public readonly List<(string Arg0, string Arg1)> LiftRocks = [];
    public int LiftRockVtbl { get; private set; }
    /// <summary>
    /// <c>00CC7A1A</c> / <c>00CC781B</c>
    /// <c>vtbl+2040(thing,alpha,1)</c>.
    /// Not screen FadeIn 1496.
    /// </summary>
    public int FadeThingVtbl { get; private set; }
    public bool ExtrasHidden { get; private set; }
    public string ExtraMode { get; private set; } = "";
    /// <summary>
    /// <c>00CC6B79</c> <c>00BFEBA8 "limbo"</c>
    /// → <c>[ebp+127]=1</c>. Draw via 1812.
    /// </summary>
    public bool ExtraLimbo { get; private set; }
    /// <summary>
    /// <c>00CC6BC4</c> <c>00BFEBA8 "return"</c>
    /// → <c>[ebp+19]=1</c>. Show path
    /// <c>vtbl+1892</c> at <c>00CC6F74</c>.
    /// Not an interpreter stop.
    /// </summary>
    public bool ExtraReturn { get; private set; }
    public int ExtraDrawVtbl { get; private set; }
    public int ExtraReturnVtbl { get; private set; }
    public string ExtraParkMarker { get; private set; } = "";
    public float TimeOfDayHours { get; set; }
    public float TimeOfDayFraction { get; set; }
    /// <summary>
    /// Hero current health. <c>vtbl+1028</c>.
    /// </summary>
    public float HeroHealth { get; set; }
    /// <summary>
    /// Hero max health. <c>vtbl+1032</c>.
    /// </summary>
    public float HeroMaxHealth { get; set; }
    /// <summary>
    /// Hero morality. <c>vtbl+624</c>.
    /// </summary>
    public float HeroMorality { get; set; }
    public readonly List<HeroExpression> Expressions = [];
    /// <summary>
    /// <c>00CCFC20</c> item currently in the hero's
    /// hands. Empty after a NULL put.
    /// </summary>
    public string HeroHands { get; set; } = "";
    /// <summary>
    /// <c>00CCFDA9</c> <c>vtbl+488(name)</c>.
    /// Distinct from <c>PutInHeroHands</c> <c>vtbl+572</c>.
    /// </summary>
    public string HeroWeapon { get; set; } = "";
    /// <summary>
    /// Last <c>RemoveHeroWeapons</c> vtbl: 552 default,
    /// 560 when <c>IsFalse(arg0)</c>.
    /// </summary>
    public int RemoveHeroWeaponsVtbl { get; set; }
    /// <summary>
    /// <c>00CC9182</c> <c>vtbl+764(name)</c>. Hair and
    /// beard both use this verb — pieces accumulate.
    /// </summary>
    public readonly List<string> HeroHairs = [];
    /// <summary>
    /// <c>00CC91FB</c> <c>vtbl+576(name)</c>.
    /// </summary>
    public readonly List<string> HeroTattoos = [];
    /// <summary>
    /// <c>00CC9274</c> <c>vtbl+760(name)</c>.
    /// </summary>
    public readonly List<string> HeroClothes = [];

    public void ApplyHeroHair(string name)
    {
        if (name.Length == 0)
            return;
        foreach (var existing in HeroHairs)
        {
            if (existing.Equals(name, StringComparison.OrdinalIgnoreCase))
                return;
        }

        HeroHairs.Add(name);
    }

    public void ApplyHeroTattoo(string name)
    {
        if (name.Length == 0)
            return;
        foreach (var existing in HeroTattoos)
        {
            if (existing.Equals(name, StringComparison.OrdinalIgnoreCase))
                return;
        }

        HeroTattoos.Add(name);
    }

    public void ApplyHeroWear(string name)
    {
        if (name.Length == 0)
            return;
        foreach (var existing in HeroClothes)
        {
            if (existing.Equals(name, StringComparison.OrdinalIgnoreCase))
                return;
        }

        HeroClothes.Add(name);
    }

    /// <summary>
    /// <c>00CC92ED</c> <c>vtbl+756</c>. No args.
    /// Clothes only — hair/tattoo stay.
    /// </summary>
    public void RemoveHeroClothes() => HeroClothes.Clear();

    /// <summary>
    /// <c>00CC8348</c>: atoi, subtract already-have
    /// Gold, <c>vtbl+504(delta)</c>.
    /// </summary>
    public int GiveGold(int amount)
    {
        if (amount <= 0)
            return 0;
        var delta = amount - HeroGold;
        if (delta <= 0)
            return 0;
        HeroGold += delta;
        return delta;
    }

    /// <summary>
    /// <c>00CC37F8</c>: melee 2032, ranged 2036,
    /// false 2028, none 2024. TRUE/other is no extra
    /// vtbl. Join <c>00CC2C6B</c>.
    /// </summary>
    public void Sheathe(string actor, string mode)
    {
        var key = actor ?? "";
        var vtbl = 0;
        if (mode.Equals("melee", StringComparison.OrdinalIgnoreCase))
            vtbl = 2032;
        else if (mode.Equals("ranged", StringComparison.OrdinalIgnoreCase))
            vtbl = 2036;
        else if (mode.Equals("false", StringComparison.OrdinalIgnoreCase))
            vtbl = 2028;
        else if (mode.Equals("none", StringComparison.OrdinalIgnoreCase))
            vtbl = 2024;
        Sheathed[key] = mode;
        SheatheVtbl[key] = vtbl;
    }

    /// <summary>
    /// <c>00CC21CB</c>: arg0 required; IsTrue(arg1);
    /// actor <c>vtbl+48</c> then engine <c>vtbl+892</c>.
    /// Attach mesh UNREAD.
    /// </summary>
    public void HoldInHand(string actor, string item, bool flag)
    {
        var key = actor ?? "";
        if (item.Length == 0)
            return;
        HeldInHand[key] = item;
        HeldInHandFlag[key] = flag;
    }

    /// <summary>
    /// <c>00CC22AE</c>: atof(arg0); actor
    /// <c>vtbl+48</c> then <c>vtbl+1060(name,amt,0)</c>.
    /// No MAX token. Clamp unread.
    /// </summary>
    public float ModifyHealth(string actor, float amount)
    {
        var key = actor ?? "";
        Health.TryGetValue(key, out var current);
        current += amount;
        Health[key] = current;
        return current;
    }

    /// <summary>
    /// <c>00CC12B7</c>: default 1; IsFalse(arg0) → 0;
    /// actor <c>vtbl+48</c> then <c>vtbl+1984</c>.
    /// AI reaction UNREAD.
    /// </summary>
    public void SetScared(string actor, bool scared)
    {
        Scared[actor ?? ""] = scared;
    }

    /// <summary>
    /// <c>00CC4501</c>: arg0 required; default 4;
    /// HIGH=3; MEDIUM=2; actor vtbl+48;
    /// <c>vtbl+32(handle,actor,level)</c>.
    /// AI brain UNREAD.
    /// </summary>
    public void SetAILevel(string actor, int level)
    {
        var key = actor ?? "";
        AILevels[key] = level;
        AILevelVtbl[key] = 32;
    }

    /// <summary>
    /// <c>00CC1360</c>: default 1; IsFalse(arg0) → 0;
    /// actor <c>vtbl+48</c> then <c>vtbl+1988</c>.
    /// Drunk gait UNREAD.
    /// </summary>
    public void SetDrunk(string actor, bool drunk)
    {
        Drunk[actor ?? ""] = drunk;
    }

    /// <summary>
    /// <c>00CC11FD</c>: arg0 required; default 1;
    /// IsFalse → 0; <c>vtbl+1976</c>. Bind pose UNREAD.
    /// </summary>
    public void SetBound(string actor, bool bound)
    {
        Bound[actor ?? ""] = bound;
    }

    /// <summary>
    /// <c>00CC1C82</c>: arg0 required; default 1;
    /// IsFalse → 0; <c>vtbl+2068(actor,flag,1)</c>.
    /// Death/AI body UNREAD.
    /// </summary>
    public void SetKillable(string actor, bool killable)
    {
        var key = actor ?? "";
        Killable[key] = killable;
        KillableExtra[key] = 1;
    }

    /// <summary>
    /// <c>00CC1144</c>: default 0; IsTrue(arg0) → 1;
    /// <c>vtbl+3376</c>. Physics body UNREAD.
    /// </summary>
    public void SetPushable(string actor, bool pushable)
    {
        Pushable[actor ?? ""] = pushable;
    }

    /// <summary>
    /// <c>00CC10A6</c>: no arg parse; always
    /// <c>vtbl+2064(name,0)</c> then <c>008ADF90</c>.
    /// DISPROVES IsFalse(arg0) lump.
    /// </summary>
    public void SetDamageable(string actor)
    {
        var key = actor ?? "";
        Damageable[key] = false;
        DamageableVtbl[key] = 2064;
        ExtrasAppended.Add(key);
    }

    /// <summary>
    /// <c>00CC1008</c>: no arg parse; always
    /// <c>vtbl+1832(name,0)</c> then <c>008ADF90</c>.
    /// Not SetDamageable 2064.
    /// </summary>
    public void SetAttackable(string actor)
    {
        var key = actor ?? "";
        Attackable[key] = false;
        AttackableVtbl[key] = 1832;
        ExtrasAppended.Add(key);
    }

    /// <summary>
    /// <c>00CC0F7E</c>: no arg parse; unary
    /// <c>vtbl+1980(name)</c>; no extras.
    /// Not SetBound 1976 / SetScared 1984.
    /// </summary>
    public void SetFree(string actor)
    {
        var key = actor ?? "";
        Freed.Add(key);
        SetFreeVtbl[key] = 1980;
    }

    /// <summary>
    /// <c>00CC4663</c>: unary <c>00CD2770</c>
    /// teardown of <c>actor+8</c> then
    /// <c>and [actor+8],0</c>. Drops the
    /// AILevel bind slot. Not SetFree 1980.
    /// Slot object dtor UNREAD.
    /// </summary>
    /// <summary>
    /// <c>00CC5F4E</c>: empty/IsTrue → 1604(1);
    /// else 1604(0). Body mesh UNREAD.
    /// </summary>
    /// <summary>
    /// <c>00CC1E7B</c>: 00CD2770 slot drop;
    /// vtbl+32(handle,actor,0); vtbl+2388(actor,target).
    /// Combat brain UNREAD.
    /// </summary>
    public void FightWith(string actor, string target)
    {
        var key = actor ?? "";
        FightTargets[key] = target ?? "";
        FightVtbl[key] = 2388;
    }

    public void SetHideBodies(bool hide)
    {
        HideBodies = hide;
        HideBodiesVtbl = 1604;
    }

    public void Release(string actor)
    {
        var key = actor ?? "";
        Released.Add(key);
        ReleaseFn[key] = 0x00CD2770;
        AILevels.Remove(key);
        AILevelVtbl.Remove(key);
    }

    /// <summary>
    /// <c>00CC68ED</c>: arg0 required; IsTrue →
    /// vtbl+924(HERO) + HeroFollower0 + 008ADF90
    /// each valid member. FALSE only clears flag.
    /// Follower list body UNREAD.
    /// </summary>
    public void ReturnFollowers(bool restore)
    {
        FollowersReturned = restore;
        FollowerReturnVtbl = restore ? 924 : 0;
        if (restore && !Followers.Contains("HeroFollower0", StringComparer.OrdinalIgnoreCase))
            Followers.Add("HeroFollower0");
    }

    /// <summary>
    /// <c>00CC6A2E</c>: empty list skips.
    /// IsTrue fades 1492/1504 then vtbl+956 then 1496.
    /// Follower warp UNREAD.
    /// </summary>
    public void TeleportFollowers(bool fade)
    {
        if (Followers.Count == 0)
            return;
        FollowersTeleported = true;
        FollowerTeleportFade = fade;
        FollowerTeleportVtbl = 956;
    }

    /// <summary>
    /// <c>00CBE2FF</c>: both <c>vtbl+300</c> then
    /// pos <c>vtbl+24</c>; success iff
    /// <c>dist^2 &lt; radius^2</c>. Strict.
    /// </summary>
    public bool IsUnderRadius(string actor, string target, float radius)
    {
        if (!Positions.TryGetValue(actor ?? "", out var a) ||
            !Positions.TryGetValue(target ?? "", out var b))
            return false;
        var d = a - b;
        return d.LengthSquared() < radius * radius;
    }

    public PendingOperation WaitUnderRadius(string actor, string target, float radius)
    {
        var key = actor ?? "";
        UnderRadiusTargets[key] = target ?? "";
        UnderRadius[key] = radius;
        var op = new PendingOperation($"rad-{++_radiusSerial}", "WaitForUnderRadius", actor, target ?? "");
        UnderRadiusOps[key] = op;
        return op;
    }

    /// <summary>
    /// <c>00CC4B7E</c>: atoi(arg0); actor
    /// <c>vtbl+48</c>; <c>004AB130</c> then
    /// <c>vtbl+1916(name,seed)</c>. PALSKIN unread.
    /// </summary>
    public void SetAppearanceSeed(string actor, int seed)
    {
        AppearanceSeed[actor ?? ""] = seed;
    }

    /// <summary>
    /// <c>00CC8094</c>: default 0; IsTrue(arg1) → 1;
    /// optional arg2 extra; <c>vtbl+2324</c>.
    /// Consciousness body UNREAD.
    /// </summary>
    public void SetThingConscious(string actor, bool conscious, string extra)
    {
        var key = actor ?? "";
        Conscious[key] = conscious;
        ConsciousVtbl[key] = 2324;
        ConsciousExtra[key] = extra ?? "";
    }

    /// <summary>
    /// <c>00CC7B24</c>: arg0+arg1 required;
    /// <c>00CBF9DE</c>+<c>004AB130</c>;
    /// IsFalse(arg1) → mode 2 else mode 1;
    /// <c>vtbl+2048(thing,mode)</c>.
    /// Pause/sim body UNREAD.
    /// </summary>
    public void PauseThing(string actor, int mode)
    {
        var key = actor ?? "";
        PauseModes[key] = mode;
        PauseVtbl[key] = 2048;
    }

    /// <summary>
    /// <c>00CC7BEB</c>: arg0+arg1 required;
    /// HERO vtbl+280 else 288; default 1;
    /// IsFalse(arg1) → 0; <c>vtbl+2128(thing,flag)</c>.
    /// Physics body UNREAD.
    /// </summary>
    public void SetGravityOnThing(string actor, bool on)
    {
        var key = actor ?? "";
        GravityOn[key] = on;
        GravityVtbl[key] = 2128;
    }

    /// <summary>
    /// <c>00CC828C</c>: arg0+arg1 required;
    /// always HERO <c>vtbl+280</c>;
    /// <c>vtbl+896(hero,arg0,arg1)</c>.
    /// Lift/attach body UNREAD.
    /// </summary>
    public void LiftRock(string arg0, string arg1)
    {
        LiftRocks.Add((arg0 ?? "", arg1 ?? ""));
        LiftRockVtbl = 896;
    }

    /// <summary>
    /// <c>00CC7881</c> / <c>00CC7682</c>:
    /// final <c>vtbl+2040(thing,end,1)</c>.
    /// Mesh fade steps UNREAD.
    /// </summary>
    public void FadeThing(string actor, float alpha)
    {
        Alpha[actor ?? ""] = alpha;
        FadeThingVtbl = 2040;
    }

    /// <summary>
    /// <c>00CC63E5</c>: give <c>count - already</c>
    /// via <c>vtbl+484</c>. Requested ≤ owned skips.
    /// </summary>
    /// <summary>
    /// <c>00CC6375</c> <c>vtbl+1052(amount,1,0)</c>.
    /// MAX uses <c>max-current</c>.
    /// </summary>
    public float GiveHeroHealth(float amount)
    {
        HeroHealth += amount;
        if (HeroMaxHealth > 0f && HeroHealth > HeroMaxHealth)
            HeroHealth = HeroMaxHealth;
        if (HeroHealth < 0f)
            HeroHealth = 0f;
        return amount;
    }

    public float GiveHeroHealthMax()
    {
        var missing = HeroMaxHealth > HeroHealth ? HeroMaxHealth - HeroHealth : 0f;
        return GiveHeroHealth(missing);
    }

    /// <summary>
    /// <c>00CC6281</c> <c>vtbl+624(amount)</c>.
    /// </summary>
    public void GiveHeroMorality(float amount) => HeroMorality += amount;

    /// <summary>
    /// <c>00CC6185</c> <c>vtbl+900(name,param,flag)</c>.
    /// Def lookup <c>007ADB30</c> unread — name is stored.
    /// </summary>
    public void GiveHeroExpression(string name, int param, bool flag)
    {
        if (name.Length == 0)
            return;
        foreach (var existing in Expressions)
        {
            if (!existing.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                continue;
            existing.Param = param;
            existing.Flag = flag;
            return;
        }

        Expressions.Add(new HeroExpression(name, param, flag));
    }

    /// <summary>
    /// <c>00CC8898</c> <c>vtbl+500(name)</c>.
    /// Takes one object instance. Distinct from
    /// <c>TakeFromHero</c> <c>vtbl+556</c> (whole slot).
    /// </summary>
    public int TakeObjectFromHero(string item)
    {
        if (item.Length == 0)
            return 0;
        TakenObjects.Add(item);
        if (HeroHands.Equals(item, StringComparison.OrdinalIgnoreCase))
            HeroHands = "";
        for (var i = 0; i < Inventory.Count; i++)
        {
            if (!Inventory[i].Name.Equals(item, StringComparison.OrdinalIgnoreCase))
                continue;
            Inventory[i].Count--;
            if (Inventory[i].Count <= 0)
                Inventory.RemoveAt(i);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// <c>00CCFBA3</c> <c>vtbl+556(name)</c>.
    /// No count — the named slot is removed.
    /// </summary>
    public int TakeFromHero(string item)
    {
        if (item.Length == 0)
            return 0;
        var removed = 0;
        for (var i = Inventory.Count - 1; i >= 0; i--)
        {
            if (!Inventory[i].Name.Equals(item, StringComparison.OrdinalIgnoreCase))
                continue;
            removed += Inventory[i].Count;
            Inventory.RemoveAt(i);
        }

        return removed;
    }

    public int GiveHero(string item, int count, int extra = -1, bool silent = false)
    {
        if (item.Length == 0 || count <= 0)
            return 0;
        HeroGifts.Add((item, count));
        var have = 0;
        HeroInventoryItem? slot = null;
        foreach (var entry in Inventory)
        {
            if (!entry.Name.Equals(item, StringComparison.OrdinalIgnoreCase))
                continue;
            slot = entry;
            have = entry.Count;
            break;
        }

        var add = count - have;
        if (add <= 0)
            return 0;
        if (slot is null)
        {
            slot = new HeroInventoryItem(item, 0, extra, silent);
            Inventory.Add(slot);
        }

        slot.Count += add;
        slot.Extra = extra;
        slot.Silent = silent;
        return add;
    }

    /// <summary>
    /// <c>00CC6B21</c>: hide = !IsFalse(arg0);
    /// arg1 00BFEBA8 limbo → 1812; return →
    /// [ebp+19] and show-path vtbl+1892;
    /// else marker 008AB980 + park 1892.
    /// Extras list body UNREAD.
    /// </summary>
    public void RemoveExtras(bool hide, string mode, bool limbo, bool ret)
    {
        ExtraOps.Add((hide, mode ?? ""));
        ExtraMode = mode ?? "";
        ExtrasHidden = hide;
        ExtraLimbo = limbo;
        ExtraReturn = ret;
        ExtraDrawVtbl = limbo ? 1812 : 2044;
        ExtraReturnVtbl = ret && !hide ? 1892 : 0;
        ExtraParkMarker = !limbo && !ret && hide && ExtraMode.Length > 0
            ? ExtraMode
            : "";
    }

    public void RemoveExtras(bool hide, string mode) =>
        RemoveExtras(
            hide,
            mode,
            ScriptLine.TokenMatches(mode ?? "", "limbo"),
            ScriptLine.TokenMatches(mode ?? "", "return"));

    public void Teleport(string? actor, string marker, Vector3? position)
    {
        Teleports.Add(new ScriptTeleport(actor, marker, position));
        if (actor is { Length: > 0 } && position is { } pos)
            Positions[actor] = pos;
    }

    /// <summary>
    /// <c>004AA9A0</c>: HomePos if set, else thing
    /// handle <c>vtbl+28</c> (TNG spawn).
    /// </summary>
    public bool TryHomeDest(string actor, ThingInstance? thing, out Vector3 dest)
    {
        if (HomePos.TryGetValue(actor, out dest))
            return true;
        if (thing is { PositionX: not null })
        {
            dest = RegionTravel.PositionOf(thing);
            return true;
        }

        dest = default;
        return false;
    }

    /// <summary>
    /// <c>00CC4AC3</c> / <c>00CC7D3C</c>:
    /// <c>004AA9A0</c> then <c>vtbl+1892</c>.
    /// </summary>
    public bool ResetPos(string actor, Vector3 dest)
    {
        if (actor.Length == 0)
            return false;
        Teleport(actor, "ResetPos", dest);
        return true;
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

    public ThingInstance Spawn(string type, string marker, string name, Vector3? pos) =>
        Spawn(type, marker, name, pos, extras: false);

    /// <summary>
    /// <c>00CCC3E6</c> <c>vtbl+364</c> <c>008A9100</c>.
    /// Empty/IsTrue(arg3) extras <c>008ADF90</c>.
    /// </summary>
    public ThingInstance Spawn(string type, string marker, string name, Vector3? pos, bool extras)
    {
        var props = new Dictionary<string, string>
        {
            ["Marker"] = marker,
            ["Created"] = "1",
        };
        if (extras)
            props["Extra"] = "1";
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
        if (extras)
            Effects.Add(new ScriptCreate(type, marker, name));
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

    /// <summary>
    /// <c>00CCB986</c> <c>vtbl+408</c> light factory.
    /// Distinct from Create 364 / Effect 400 / Dummy 404.
    /// </summary>
    public ThingInstance SpawnLight(
        string marker, string name, byte r, byte g, byte b,
        float param0, float param1, bool flag, Vector3? pos, bool extras)
    {
        var thing = Spawn("Light", marker, name, pos);
        var props = new Dictionary<string, string>(thing.Properties, StringComparer.OrdinalIgnoreCase)
        {
            ["Light"] = "1",
            ["R"] = r.ToString(CultureInfo.InvariantCulture),
            ["G"] = g.ToString(CultureInfo.InvariantCulture),
            ["B"] = b.ToString(CultureInfo.InvariantCulture),
            ["Param0"] = param0.ToString(CultureInfo.InvariantCulture),
            ["Param1"] = param1.ToString(CultureInfo.InvariantCulture),
            ["Flag"] = flag ? "1" : "0",
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
        Lights.Add(new ScriptCreate("Light", marker, name));
        LightColors[name] = (r, g, b, 255);
        if (extras)
            Effects.Add(new ScriptCreate("Light", marker, name));
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
        Doors.Remove(name);
        Chests.Remove(name);
        Collide.Remove(name);
        Effects.RemoveAll(e =>
            e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        Lights.RemoveAll(e =>
            e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        LightColors.Remove(name);
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
