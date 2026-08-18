namespace Fable.Game.Scripting;

/// <summary>
/// Microthread pump analog of <c>00A44880</c> /
/// resume <c>00A44660</c> / yield <c>00A44690</c>.
/// </summary>
public sealed class ScriptScheduler
{
    public IReadOnlyList<FiberState> Fibers => _fibers;
    private readonly List<FiberState> _fibers = [];
    private int _nextId = 1;

    public FiberState Create(string name, string? persistField)
    {
        var fiber = new FiberState(_nextId++, name, persistField);
        _fibers.Add(fiber);
        return fiber;
    }

    public void Pump(float dt, Action<FiberState> resume)
    {
        foreach (var fiber in _fibers)
        {
            fiber.DtAtPlus8 = dt;
            if (fiber.State == FiberRunState.Dead)
                continue;
            resume(fiber);
        }
    }
}

public sealed class FiberState
{
    public int Id { get; }
    public string Name { get; }
    public string? PersistField { get; }
    public float DtAtPlus8 { get; set; }
    public FiberRunState State { get; set; } = FiberRunState.Ready;
    public WaitKind WaitKind { get; set; }
    public string WaitTarget { get; set; } = "";
    public float WakeTime { get; set; }
    public string QueuedTask { get; set; } = "";
    public string CompletionReason { get; set; } = "";
    public int ScriptInstanceId { get; set; }

    public FiberState(int id, string name, string? persistField)
    {
        Id = id;
        Name = name;
        PersistField = persistField;
    }
}

public enum FiberRunState
{
    Ready,
    Waiting,
    Running,
    Dead,
}

/// <summary>
/// Native quest/script object. S_QNOVI is this, not a
/// CCutsceneDef. Factory / fiber / persist / then child
/// cutscene start.
/// </summary>
public sealed class QuestInstance
{
    public int Id { get; }
    public string Name { get; }
    public string? PersistField { get; }
    public FiberState? Fiber { get; set; }
    public string? ChildCutscene { get; private set; }
    public bool Started { get; private set; }
    public uint Factory { get; private set; }
    public uint Run { get; private set; }
    public uint Init { get; private set; }
    public string? ScriptName { get; private set; }

    public QuestInstance(int id, string name, string? persistField)
    {
        Id = id;
        Name = name;
        PersistField = persistField;
    }

    public void AttachFiber(FiberState fiber)
    {
        Fiber = fiber;
        fiber.ScriptInstanceId = Id;
    }

    public void StartChildCutscene(string cutsceneName)
    {
        ChildCutscene = cutsceneName;
        Started = true;
    }

    /// <summary>
    /// <c>004B3CE0</c> factory construct +
    /// run.vtbl+8. Native-only quests have
    /// no <c>CCutsceneDef</c>.
    /// </summary>
    public void StartFactory(uint factory, uint run, uint init, string? scriptName)
    {
        Factory = factory;
        Run = run;
        Init = init;
        ScriptName = scriptName;
        Started = true;
    }
}
