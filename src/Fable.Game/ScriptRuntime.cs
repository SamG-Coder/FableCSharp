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
    public string? LastMusic { get; private set; }
    public string? LastAvi { get; private set; }
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
    public IReadOnlyList<ScriptAnimation> Animations => _animations;
    public IReadOnlyList<string> PreloadedCameras => _preloadedCameras;

    private readonly Dictionary<string, string> _named = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, bool> _persist = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ScriptFiber> _fibers = [];
    private readonly List<ScriptInterpreter> _interpreters = [];
    private readonly List<ScriptTeleport> _teleports = [];
    private readonly List<ScriptAnimation> _animations = [];
    private readonly List<string> _preloadedCameras = [];
    private IReadOnlyList<ThingInstance> _things = [];
    private ScriptedCamera? _camera;

    public void Load(ScriptBank bank) => Bank = bank;

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
        foreach (var interpreter in _interpreters)
        {
            if (interpreter.Yielded)
                interpreter.Resume(this);
        }
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
        runtime.Load(ScriptBank.Load(install));
        runtime.BindScene(list, camera);
        runtime.RegisterNamedScript(RegionTravel.LiveFatherScript, RegionTravel.IntroCutscene);
        runtime.CreateFiber(RegionTravel.IntroScriptName, NewGameScript.PersistAttackOverName);
        runtime.ActivateThings(list);
        return runtime;
    }

    void IScriptHost.PlayMusic(string track) => LastMusic = track;

    void IScriptHost.FadeOut(float seconds, float param)
    {
        FadeDuration = seconds;
        FadeParam = param;
    }

    void IScriptHost.FadeIn(float seconds, float param)
    {
        FadeDuration = seconds;
        FadeParam = param;
    }

    void IScriptHost.UseCamera(string name) => BindCamera(name);

    void IScriptHost.NoLoadUseCamera(string name) => BindCamera(name);

    /// <summary>
    /// <c>00CC14B8</c>: thing <c>vtbl+72</c> with name +
    /// five flags + <c>[0x1375748]</c> + 0. Body UNREAD —
    /// record only. <c>[ebp-22]</c> ctor 1 at
    /// <c>00CBFD57</c> then <c>00CC186F</c> →
    /// <c>00CC5691</c> one <c>vtbl+28</c>.
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

    void IScriptHost.Teleport(string? actor, string arguments)
    {
        var args = ScriptCommand.SplitArgs(arguments);
        var marker = args.Length == 0 ? "" : args[0];
        var thing = FindThing(marker);
        Vector3? position = thing is { PositionX: not null } ? RegionTravel.PositionOf(thing) : null;
        _teleports.Add(new ScriptTeleport(actor, marker, position));
    }

    void IScriptHost.LookToThing(string? actor, string arguments) =>
        _ = (actor, arguments);

    /// <summary>
    /// <c>00CCA26D</c>: prefix <c>Data\Video\</c> then
    /// <c>vtbl+1476</c>. Interpreter <c>jmp 00CD17F8</c>
    /// (no yield). Do not invent video playback.
    /// </summary>
    void IScriptHost.PlayAvi(string arguments)
    {
        var file = ScriptInterpreter.FirstToken(arguments);
        LastAvi = file.Length == 0 ? null : RegionTravel.PlayAviPrefix + file;
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
