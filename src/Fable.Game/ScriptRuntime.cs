using System.Numerics;
using Fable.Core;
using Fable.Formats.Tng;

namespace Fable.Game;

/// <summary>
/// Generic script VM: name table <c>00CB8230</c>, thing
/// activate <c>004C97B0</c> / <c>00CB8960</c>, microthread
/// <c>00A44880</c>, interpreter <c>00CBFB7D</c>. Scene
/// behaviour comes from exported command lists, not a
/// handcrafted Oakvale state machine.
/// </summary>
public sealed class ScriptRuntime : IScriptHost
{
    public ScriptBank? Bank { get; private set; }
    public float DtAtPlus8 { get; private set; }
    public float FadeDuration { get; private set; }
    public float FadeParam { get; private set; }
    public float FadeElapsed { get; private set; }
    public float FadeRemaining { get; private set; }
    public bool FadeActive { get; private set; }
    public bool FadeLocked { get; private set; }
    public bool FadeRising { get; private set; }
    public bool FadeFalling { get; private set; }
    public (byte R, byte G, byte B, byte A) FadeColor { get; private set; }
    /// <summary>
    /// <c>004348D0</c> fraction. Draw
    /// <c>006496BC</c> uses this * 255 as the
    /// overlay alpha.
    /// </summary>
    public float OverlayAlpha => OverlayFraction();
    public byte OverlayAlphaByte =>
        (byte)Math.Clamp((int)(OverlayFraction() * RegionTravel.FadeAlphaScale), 0, 255);
    public string? LastMusic { get; private set; }
    public string? LastAvi { get; private set; }
    public string? AviRelativePath { get; private set; }
    public string? AviFile { get; private set; }
    public bool AviPlaying { get; private set; }
    public int AviWidth => _avi?.Width ?? 0;
    public int AviHeight => _avi?.Height ?? 0;
    public byte[]? AviRgba => _avi?.Rgba;
    public int AviFrameSerial => _avi?.FrameSerial ?? 0;
    public bool SoundsMuted { get; private set; }
    public int TimeCode { get; private set; }
    public float LastGamePause { get; private set; }
    public string? ActiveCutscene => _interpreters.Count == 0 ? null : _interpreters[^1].Name;
    public ScriptInterpreter? ActiveInterpreter =>
        _interpreters.Count == 0 ? null : _interpreters[^1];
    public IReadOnlyList<ScriptInterpreter> Interpreters => _interpreters;
    public IReadOnlyDictionary<string, bool> PersistFields => _persist;
    public IReadOnlyDictionary<string, string> NamedScripts => _named;
    public IReadOnlyList<ScriptTeleport> Teleports => _teleports;
    public IReadOnlyDictionary<string, Vector3> ActorPositions => _actorPositions;
    public IReadOnlyList<ScriptAnimation> Animations => _animations;
    public IReadOnlyList<ScriptSpeech> Speeches => _speeches;
    public IReadOnlyList<ScriptInteractiveSpeech> InteractiveSpeeches => _interactive;
    public IReadOnlyList<ScriptDialogSpeech> DialogSpeeches => _dialogs;
    public IReadOnlyList<ScriptWaitTask> WaitTasks => _waits;
    public IReadOnlyList<ScriptSneakTo> SneakTos => _sneaks;
    public IReadOnlyList<ScriptWalkTo> WalkTos => _walks;
    public IReadOnlyList<ScriptCombatAnimation> CombatAnimations => _combatAnims;
    public IReadOnlyList<ScriptCreate> Creates => _creates;
    public IReadOnlyList<string> Removes => _removes;
    public IReadOnlyList<ScriptDialogAdSpeech> DialogAdSpeeches => _dialogAds;
    public IReadOnlyList<ScriptLookInDirection> LookInDirections => _looks;
    public int WaitActiveDialogCount { get; private set; }
    public IReadOnlyList<string> PreloadedCameras => _preloadedCameras;

    private readonly Dictionary<string, string> _named = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, bool> _persist = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ScriptFiber> _fibers = [];
    private readonly List<ScriptInterpreter> _interpreters = [];
    private readonly List<ScriptTeleport> _teleports = [];
    private readonly Dictionary<string, Vector3> _actorPositions = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ScriptAnimation> _animations = [];
    private readonly List<ScriptSpeech> _speeches = [];
    private readonly List<ScriptInteractiveSpeech> _interactive = [];
    private readonly List<ScriptDialogSpeech> _dialogs = [];
    private readonly List<ScriptWaitTask> _waits = [];
    private readonly List<ScriptSneakTo> _sneaks = [];
    private readonly List<ScriptWalkTo> _walks = [];
    private readonly List<ScriptCombatAnimation> _combatAnims = [];
    private readonly List<ScriptCreate> _creates = [];
    private readonly List<string> _removes = [];
    private readonly List<ScriptDialogAdSpeech> _dialogAds = [];
    private readonly List<ScriptLookInDirection> _looks = [];
    private readonly List<string> _preloadedCameras = [];
    private IReadOnlyList<ThingInstance> _things = [];
    private ScriptedCamera? _camera;
    private GameInstall? _install;
    private WmvPlayer? _avi;

    public void Load(ScriptBank bank, GameInstall? install = null)
    {
        Bank = bank;
        _install = install;
    }

    public void BindScene(IEnumerable<ThingInstance> things, ScriptedCamera? camera)
    {
        _things = things as IReadOnlyList<ThingInstance> ?? things.ToList();
        _camera = camera;
    }

    /// <summary>
    /// <c>00CB8230</c> name record. Factory body for
    /// <c>NOVI_LiveFather</c> is <c>00DAC2C0</c> →
    /// <c>00DB86B0</c> → <c>00CBFB7D(cutscene)</c>.
    /// </summary>
    public void RegisterNamedScript(string scriptName, string cutsceneName) =>
        _named[scriptName] = cutsceneName;

    /// <summary>
    /// <c>00A447D0</c> create + <c>00A446A0</c> persist slot.
    /// Does not invent the <c>+80</c> writer.
    /// </summary>
    public ScriptFiber CreateFiber(string name, string? persistField = null)
    {
        if (persistField is not null && !_persist.ContainsKey(persistField))
            _persist[persistField] = false;
        var fiber = new ScriptFiber(name, persistField);
        _fibers.Add(fiber);
        return fiber;
    }

    public void ApplyPersist(string name, bool value) => _persist[name] = value;

    public bool PersistBool(string name) =>
        _persist.TryGetValue(name, out var value) && value;

    /// <summary>
    /// <c>004C97B0</c> / <c>00CB8960</c>: start the named
    /// script only when the name is in the registry.
    /// </summary>
    public ScriptInterpreter? ActivateThing(ThingInstance thing)
    {
        if (thing.ScriptName is null)
            return null;
        return StartNamedScript(thing.ScriptName);
    }

    public void ActivateThings(IEnumerable<ThingInstance> things)
    {
        foreach (var thing in things)
            ActivateThing(thing);
    }

    public ScriptInterpreter? StartNamedScript(string scriptName)
    {
        if (!_named.TryGetValue(scriptName, out var cutscene))
            return null;
        return StartCutscene(cutscene);
    }

    public ScriptInterpreter? StartCutscene(string cutsceneName)
    {
        if (FindInterpreter(cutsceneName) is { } existing)
            return existing;
        var def = Bank?.Find(cutsceneName);
        if (def is null)
            return null;
        var interpreter = new ScriptInterpreter(def.InstanceName, def.Commands);
        _interpreters.Add(interpreter);
        interpreter.RunUntilYield(this);
        return interpreter;
    }

    public ScriptInterpreter? FindInterpreter(string name) =>
        _interpreters.Find(item => item.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public bool HasStarted(string cutsceneName) => FindInterpreter(cutsceneName) is not null;

    public bool ExecutedVerb(string cutsceneName, string verb) =>
        FindInterpreter(cutsceneName)?.ExecutedVerb(verb) ?? false;

    /// <summary>
    /// <c>00A44880</c>: store dt at <c>+8</c>, then
    /// <c>00A44660</c> resume. Unread waits re-yield.
    /// Does not write persist fields.
    /// </summary>
    public void Update(float dt)
    {
        if (_fibers.Count == 0 && _interpreters.Count == 0)
            return;
        if (dt < 0f)
            return;
        DtAtPlus8 = dt;
        foreach (var fiber in _fibers)
            fiber.DtAtPlus8 = dt;
        TickFade(dt);
        TickAvi(dt);
        foreach (var interpreter in _interpreters)
        {
            if (interpreter.Yielded)
                interpreter.Resume(this);
        }
    }

    /// <summary>
    /// <c>00434870</c> on the +188 fade record.
    /// Rising adds dt to elapsed until duration.
    /// Falling subtracts dt from remaining then
    /// clears +188.
    /// </summary>
    private void TickFade(float dt)
    {
        if (FadeRising)
        {
            FadeElapsed += dt;
            if (FadeElapsed >= FadeDuration)
            {
                FadeElapsed = FadeDuration;
                FadeRising = false;
            }

            return;
        }

        if (!FadeFalling)
            return;
        FadeRemaining -= dt;
        if (FadeRemaining > 0f)
            return;
        FadeRemaining = 0f;
        FadeActive = false;
        FadeFalling = false;
        FadeRising = false;
    }

    /// <summary>
    /// <c>004348D0</c>: rising elapsed/duration,
    /// falling remaining/param, else +188 → 1 else 0.
    /// Duration ≤ 0.0001 returns 1.
    /// </summary>
    private float OverlayFraction()
    {
        if (FadeRising)
        {
            if (FadeDuration <= RegionTravel.FadeAlphaEpsilon)
                return 1f;
            return FadeElapsed / FadeDuration;
        }

        if (FadeFalling)
        {
            if (FadeParam <= RegionTravel.FadeAlphaEpsilon)
                return 1f;
            return FadeRemaining / FadeParam;
        }

        return FadeActive ? 1f : 0f;
    }

    /// <summary>
    /// New Game wiring only. Recovered
    /// <c>NOVI_LiveFather</c> → <c>CS_OAKVALE_INTRO_FATHER</c>
    /// binding, then generic activate + interpret.
    /// </summary>
    public static ScriptRuntime StartNewGame(
        GameInstall install,
        IEnumerable<ThingInstance> things,
        ScriptedCamera? camera = null)
    {
        var list = things as IReadOnlyList<ThingInstance> ?? things.ToList();
        var runtime = new ScriptRuntime();
        runtime.Load(ScriptBank.Load(install), install);
        runtime.BindScene(list, camera);
        runtime.RegisterNamedScript(RegionTravel.LiveFatherScript, RegionTravel.IntroCutscene);
        runtime.CreateFiber(RegionTravel.IntroScriptName, NewGameScript.PersistAttackOverName);
        runtime.ActivateThings(list);
        return runtime;
    }

    void IScriptHost.PlayMusic(string track) => LastMusic = track;

    /// <summary>
    /// <c>008907E0</c> <c>vtbl+1488</c>: pack
    /// <c>(0,0,0,255)</c>, call <c>vtbl+1492</c> →
    /// <c>00434C00</c> (+188=1, +201=1, +192=seconds).
    /// Then <c>[+216]=1</c> lock. Overlay draw is
    /// <c>006496BC</c> gated on +188.
    /// </summary>
    void IScriptHost.FadeOut(float seconds, float param)
    {
        FadeDuration = seconds;
        FadeParam = param;
        FadeColor = (0, 0, 0, 255);
        if (FadeLocked)
            return;
        FadeActive = true;
        FadeLocked = true;
        FadeRising = true;
        FadeFalling = false;
        FadeElapsed = 0f;
        FadeRemaining = 0f;
    }

    /// <summary>
    /// Bare <c>FadeIn</c> is <c>vtbl+1496</c>
    /// <c>0088E4C0</c>: clear <c>[+216]</c> then
    /// <c>00434C90</c> (+201=0, +200=1). Next
    /// <c>00434870</c> tick can clear +188.
    /// </summary>
    void IScriptHost.FadeIn(float seconds, float param)
    {
        FadeDuration = seconds;
        FadeParam = param;
        FadeLocked = false;
        if (!FadeActive)
            return;
        FadeRising = false;
        FadeFalling = true;
        FadeRemaining = param;
    }

    void IScriptHost.UseCamera(string name) => BindCamera(name);

    void IScriptHost.NoLoadUseCamera(string name) => BindCamera(name);

    /// <summary>
    /// <c>00CC14B8</c>: thing <c>vtbl+72</c>
    /// (<c>004C7470</c>) walks components and calls
    /// <c>[comp.vtbl+68](name)</c>. CTCAnimationComplex
    /// <c>+68</c> is <c>00686920</c> <c>al=1</c> (not
    /// handled). Inner play <c>0070D580</c> is not on
    /// this path — record name+flags only.
    /// <c>[ebp-22]</c> ctor 1 at <c>00CBFD57</c> then
    /// <c>00CC186F</c> → <c>00CC5691</c> one
    /// <c>vtbl+28</c>.
    /// </summary>
    void IScriptHost.PlayAnimation(string? actor, string arguments)
    {
        var args = ScriptCommand.SplitArgs(arguments);
        var name = args.Length == 0 ? "" : args[0];
        var flags = ScriptCommand.ParsePlayAnimationFlags(arguments);
        _animations.Add(new ScriptAnimation(
            actor, name, flags.Flag1, flags.Flag2, flags.Flag3, flags.Flag4, flags.Flag5));
    }

    void IScriptHost.CameraPause(string arguments) => _ = arguments;

    /// <summary>
    /// <c>00CC4678</c> → <c>vtbl+1892</c>
    /// <c>0089B780</c>: marker pos
    /// <c>004AA980</c> (<c>[handle+4].vtbl+24</c>),
    /// then <c>[thing+96].vtbl+124(pos)</c>.
    /// <c>00CC47B4</c> also reads yaw
    /// <c>004AAA40</c> into <c>0089BDF0</c>.
    /// Do not invent the unread heading write.
    /// </summary>
    void IScriptHost.Teleport(string? actor, string arguments)
    {
        var args = ScriptCommand.SplitArgs(arguments);
        var marker = args.Length == 0 ? "" : args[0];
        var thing = FindThing(marker);
        Vector3? position = thing is { PositionX: not null } ? RegionTravel.PositionOf(thing) : null;
        _teleports.Add(new ScriptTeleport(actor, marker, position));
        if (actor is { Length: > 0 } && position is { } pos)
            _actorPositions[actor] = pos;
    }

    void IScriptHost.LookToThing(string? actor, string arguments) =>
        _ = (actor, arguments);

    /// <summary>
    /// <c>00CCA26D</c>: prefix <c>Data\Video\</c> then
    /// <c>vtbl+1476</c> <c>0088F890</c> →
    /// <c>0040D2A0</c> then blocking
    /// <c>006286F0(edx=0x1B)</c>. <c>0099C1E0</c>
    /// rewrites <c>.xmv</c> → <c>.wmv</c>.
    /// Interpreter stays in the apply until the
    /// player returns (no <c>vtbl+28</c>).
    /// </summary>
    void IScriptHost.PlayAvi(string arguments)
    {
        var file = ScriptInterpreter.FirstToken(arguments);
        LastAvi = file.Length == 0 ? null : RegionTravel.PlayAviPrefix + file;
        AviRelativePath = LastAvi is null ? null : RegionTravel.RewritePlayAviPath(LastAvi);
        AviFile = LastAvi is null || _install is null
            ? null
            : RegionTravel.ResolvePlayAviFile(_install, LastAvi);
        _avi?.Dispose();
        _avi = AviFile is null ? null : WmvPlayer.TryOpen(AviFile);
        AviPlaying = AviFile is not null;
    }

    /// <summary>
    /// <c>006286F0</c> skip scan 1 / 57 / 28 / 62.
    /// </summary>
    public void SkipAvi()
    {
        _avi?.Dispose();
        _avi = null;
        AviPlaying = false;
    }

    /// <summary>
    /// One <c>006286F0</c> iteration inside the
    /// blocking PlayAVI apply: 33 ms wait analog
    /// then the current sample. Interpreter
    /// <c>TickPlayAvi</c> stays on the line until
    /// this returns (EOF / skip). No extra window.
    /// </summary>
    private void TickAvi(float dt)
    {
        if (!AviPlaying)
            return;
        if (_avi is null)
            return;
        if (_avi.TryAdvance(dt) && !_avi.Ended)
            return;
        if (_avi.Ended)
            SkipAvi();
    }

    /// <summary>
    /// <c>00CC7258</c>: <c>00CBEE0C</c> IsFalse →
    /// <c>vtbl+2664(0)</c>, else <c>(1)</c>.
    /// <c>jmp 00CC8464</c>. No yield. Body UNREAD.
    /// </summary>
    void IScriptHost.MuteSounds(string arguments) =>
        SoundsMuted = !ScriptCommand.IsFalseArg(ScriptInterpreter.FirstToken(arguments));

    /// <summary>
    /// <c>00CD1373</c>: <c>and [0x13B83C8], 0</c> then
    /// <c>jmp 00CD17FD</c>. No yield. Do not invent
    /// the leftover increment as a pose clock.
    /// </summary>
    void IScriptHost.StartTimeCode() => TimeCode = 0;

    /// <summary>
    /// <c>00CC88D1</c> default wait is scaled frames,
    /// not wall-clock dt. CLOCK arg is unread here.
    /// </summary>
    void IScriptHost.GamePause(float seconds) => LastGamePause = seconds;

    /// <summary>
    /// <c>00CC25FD</c>: thing <c>vtbl+52</c> then poll
    /// <c>vtbl+104</c>. Father <c>0x0127293C</c> +52 is
    /// <c>004CD1B0</c> <c>al=1</c>, +104 is
    /// <c>00661A40</c> <c>ret 4</c> (leaves al). First
    /// poll is busy, one <c>vtbl+28</c>, next poll idle.
    /// Do not invent dialogue UI.
    /// </summary>
    void IScriptHost.Speak(string? actor, string target, string text, int mode) =>
        _speeches.Add(new ScriptSpeech(actor, target, text, mode));

    /// <summary>
    /// <c>00CC2EAA</c>: context <c>vtbl+1456/1460/1464</c>
    /// then if third arg not TRUE one <c>vtbl+28</c>
    /// and <c>jmp 00CC707C</c>. Bodies UNREAD.
    /// </summary>
    void IScriptHost.InteractiveSpeak(
        string? actor, string listener, string prompt, bool wait, string response) =>
        _interactive.Add(new ScriptInteractiveSpeech(actor, listener, prompt, wait, response));

    /// <summary>
    /// <c>00CC3165</c>: context <c>vtbl+1456/1460/1464</c>
    /// then one <c>vtbl+28</c> and <c>jmp 00CC707C</c>.
    /// Bodies UNREAD — record only.
    /// </summary>
    void IScriptHost.DialogSpeak(string? actor, string listener, string text) =>
        _dialogs.Add(new ScriptDialogSpeech(actor, listener, text));

    /// <summary>
    /// <c>00CC0783</c>: name unused. Poll thing
    /// <c>vtbl+104</c>. Hero stub leaves al; first
    /// leftover is busy so one <c>vtbl+28</c> then
    /// continue. Do not invent a task table.
    /// </summary>
    void IScriptHost.WaitTask(string? actor, string name) =>
        _waits.Add(new ScriptWaitTask(actor, name));

    /// <summary>
    /// <c>00CC0CB5</c>: thing <c>vtbl+20</c> is
    /// <c>004C72B0</c> stub. TRUE wait polls
    /// <c>vtbl+104</c> leftover once. Record only —
    /// no mesh move.
    /// </summary>
    void IScriptHost.SneakTo(string? actor, string marker, float speed, bool wait) =>
        _sneaks.Add(new ScriptSneakTo(actor, marker, speed, wait));

    /// <summary>
    /// <c>00CC083D</c>: thing <c>vtbl+20</c> is
    /// <c>004C72B0</c> stub. First-seen does not
    /// wait for arrival. Record only — no mesh move.
    /// </summary>
    void IScriptHost.WalkTo(string? actor, string marker, float speed, bool wait) =>
        _walks.Add(new ScriptWalkTo(actor, marker, speed, wait));

    /// <summary>
    /// <c>00CC15E3</c>: thing <c>vtbl+76</c> does not
    /// read the name. Record only — no TURNING_AC90
    /// pose. <c>[ebp-22]</c> one <c>vtbl+28</c>.
    /// </summary>
    void IScriptHost.PlayCombatAnimation(
        string? actor, string name, bool flagA, bool flagB, bool flagC, bool flagD, bool flagE, int count) =>
        _combatAnims.Add(new ScriptCombatAnimation(actor, name, flagA, flagB, flagC, flagD, flagE, count));

    /// <summary>
    /// <c>00CCC246</c>: <c>vtbl+364</c> then
    /// <c>jmp 00CD17F8</c>. No yield. Spawn body
    /// UNREAD — record only.
    /// </summary>
    void IScriptHost.Create(string type, string marker, string name) =>
        _creates.Add(new ScriptCreate(type, marker, name));

    /// <summary>
    /// <c>00CC656B</c>: leftover session poll
    /// <c>vtbl+1472</c>. Dismiss UNREAD — one yield.
    /// </summary>
    void IScriptHost.WaitActiveDialog() => WaitActiveDialogCount++;

    /// <summary>
    /// <c>00CD0116</c>: <c>vtbl+432</c> then
    /// <c>jmp 00CC864B</c>. No yield. Teardown
    /// UNREAD — record only.
    /// </summary>
    void IScriptHost.Remove(string name) => _removes.Add(name);

    /// <summary>
    /// <c>00CC3354</c>: thing <c>vtbl+52</c> then
    /// <c>jmp 00CC707C</c> / <c>00CC2C6B</c> →
    /// <c>00CC7081</c>. No <c>vtbl+28</c>. Father
    /// +52 is <c>004CD1B0</c> stub. Record only —
    /// do not invent dialogue UI.
    /// </summary>
    void IScriptHost.DialogadSpeak(string? actor, string target, string text, int mode) =>
        _dialogAds.Add(new ScriptDialogAdSpeech(actor, target, text, mode));

    /// <summary>
    /// <c>00CC3F73</c>: context <c>vtbl+1896</c>
    /// <c>0089BDF0</c> then <c>jmp 00CC707C</c>. No
    /// <c>vtbl+28</c>. Heading body UNREAD — record
    /// only, do not invent a yaw write.
    /// </summary>
    void IScriptHost.LookInDirection(string? actor, float degrees, bool flag) =>
        _looks.Add(new ScriptLookInDirection(actor, degrees, flag));

    /// <summary>
    /// <c>00CC86D0</c> default path: <c>00CBF29F</c> with
    /// <c>dl=0</c> collects UseCamera names via
    /// <c>vtbl+1648</c>. First-seen has no TRUE arg so
    /// <c>vtbl+1560</c> is skipped. <c>vtbl+1564/+1568</c>
    /// bodies stay UNREAD.
    /// </summary>
    void IScriptHost.DoCameraPreloading(string arguments)
    {
        _ = arguments;
        var source = ActiveInterpreter?.Commands;
        if (source is null)
            return;
        foreach (var line in source)
        {
            var command = ScriptCommand.Parse(line);
            if (!command.Verb.Equals("UseCamera", StringComparison.OrdinalIgnoreCase) &&
                !command.Verb.Equals("CameraLookAt", StringComparison.OrdinalIgnoreCase) &&
                !command.Verb.Equals("CameraLookBetween", StringComparison.OrdinalIgnoreCase) &&
                !command.Verb.Equals("CameraFOVLookBetween", StringComparison.OrdinalIgnoreCase))
                continue;
            var name = ScriptInterpreter.FirstToken(command.Arguments);
            if (name.Length == 0 || _preloadedCameras.Contains(name, StringComparer.OrdinalIgnoreCase))
                continue;
            _preloadedCameras.Add(name);
        }
    }

    private ThingInstance? FindThing(string name)
    {
        if (name.Length == 0)
            return null;
        foreach (var thing in _things)
        {
            if (thing.ScriptName is not null &&
                thing.ScriptName.Equals(name, StringComparison.OrdinalIgnoreCase))
                return thing;
        }

        return null;
    }

    private void BindCamera(string name)
    {
        if (_camera is null || name.Length == 0)
            return;
        _camera.UseCamera(_things, name);
    }
}

public sealed class ScriptFiber
{
    public string Name { get; }
    public string? PersistField { get; }
    public float DtAtPlus8 { get; set; }

    public ScriptFiber(string name, string? persistField)
    {
        Name = name;
        PersistField = persistField;
    }
}

public readonly record struct ScriptTeleport(string? Actor, string Marker, Vector3? Position);

public readonly record struct ScriptAnimation(
    string? Actor,
    string Name,
    bool Flag1,
    bool Flag2,
    bool Flag3,
    bool Flag4,
    bool Flag5);

public readonly record struct ScriptSpeech(
    string? Actor,
    string Target,
    string Text,
    int Mode);

public readonly record struct ScriptInteractiveSpeech(
    string? Actor,
    string Listener,
    string Prompt,
    bool Wait,
    string Response);

public readonly record struct ScriptDialogSpeech(
    string? Actor,
    string Listener,
    string Text);

public readonly record struct ScriptWaitTask(string? Actor, string Name);

public readonly record struct ScriptSneakTo(string? Actor, string Marker, float Speed, bool Wait);

public readonly record struct ScriptWalkTo(string? Actor, string Marker, float Speed, bool Wait);

public readonly record struct ScriptCombatAnimation(
    string? Actor,
    string Name,
    bool FlagA,
    bool FlagB,
    bool FlagC,
    bool FlagD,
    bool FlagE,
    int Count);

public readonly record struct ScriptCreate(string Type, string Marker, string Name);

public readonly record struct ScriptDialogAdSpeech(
    string? Actor,
    string Target,
    string Text,
    int Mode);

public readonly record struct ScriptLookInDirection(string? Actor, float Degrees, bool Flag);
