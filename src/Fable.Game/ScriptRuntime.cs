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
    public string? ActiveCutscene => _interpreters.Count == 0 ? null : _interpreters[^1].Name;
    public ScriptInterpreter? ActiveInterpreter =>
        _interpreters.Count == 0 ? null : _interpreters[^1];
    public IReadOnlyList<ScriptInterpreter> Interpreters => _interpreters;
    public IReadOnlyDictionary<string, bool> PersistFields => _persist;
    public IReadOnlyDictionary<string, string> NamedScripts => _named;

    private readonly Dictionary<string, string> _named = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, bool> _persist = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ScriptFiber> _fibers = [];
    private readonly List<ScriptInterpreter> _interpreters = [];
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
    /// script bound to each thing <c>ScriptName</c>.
    /// </summary>
    public void ActivateThings(IEnumerable<ThingInstance> things)
    {
        foreach (var thing in things)
        {
            if (thing.ScriptName is null)
                continue;
            StartNamedScript(thing.ScriptName);
        }
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
    /// <c>00A44880</c>: store dt at <c>+8</c>. Does not
    /// resume unread waits or write persist fields.
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

    void IScriptHost.PlayAnimation(string? actor, string arguments)
    {
        _ = actor;
        _ = arguments;
    }

    void IScriptHost.CameraPause(string arguments) => _ = arguments;

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
