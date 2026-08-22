namespace Fable.Game;

/// <summary>
/// Managed names for the recovered blocks in native <c>S_QNOVI</c>.
/// The names describe control-flow ownership only; they do not promote
/// unresolved native calls to gameplay semantics.
/// </summary>
public enum IntroFatherParentPhase : byte
{
    WaitingForChild = 0,
    FadeTail = 1,
    HighlightingInstruction = 2,
    AcquireScriptedThingMode3 = 3,
    TestHeroScriptName = 4,
    AcquireScriptedThingMode4 = 5,
    FatherGoodDeedLoop = 6,
    Finished = 7,
}

public readonly record struct WorldEventRecord(int Type, int Timestamp, int Source = 0);

/// <summary>
/// The timestamped journal queried by native <c>008ABED0</c>. This is
/// deliberately separate from keyboard/device input and from delayed
/// <c>00687540</c> event-manager work items.
/// </summary>
public sealed class WorldEventJournal
{
    private readonly List<WorldEventRecord> _records = [];

    public IReadOnlyList<WorldEventRecord> Records => _records;

    public void Post(int type, int timestamp, int source = 0) =>
        _records.Add(new WorldEventRecord(type, timestamp, source));

    public bool Contains(int type, int firstTimestamp, int lastTimestamp)
    {
        if (lastTimestamp < firstTimestamp)
            (firstTimestamp, lastTimestamp) = (lastTimestamp, firstTimestamp);
        for (var i = _records.Count - 1; i >= 0; i--)
        {
            var record = _records[i];
            if (record.Type == type &&
                record.Timestamp >= firstTimestamp &&
                record.Timestamp <= lastTimestamp)
                return true;
        }

        return false;
    }
}

public readonly record struct ScriptedThingLease(
    int Serial, string ScriptName, int Mode);

/// <summary>
/// Managed counterpart of CGameScriptInterface vtable slot 8
/// (<c>0089B5B0</c>). The returned native object is
/// <c>CScriptGameResourceObjectScriptedThing</c> (vtable
/// <c>0x0128D86C</c>), not an AI action or a game-mode transition.
/// </summary>
public sealed class ScriptedThingLeaseRuntime
{
    private int _serial;
    private readonly HashSet<string> _component31Busy =
        new(StringComparer.OrdinalIgnoreCase);

    public ScriptedThingLease? Current { get; private set; }

    public bool IsComponent31Busy(string scriptName) =>
        _component31Busy.Contains(scriptName);

    public void SetComponent31Busy(string scriptName, bool busy)
    {
        if (busy)
            _component31Busy.Add(scriptName);
        else
            _component31Busy.Remove(scriptName);
    }

    public bool TryAcquire(string scriptName, int mode, bool active, bool componentBusy)
    {
        // 0089B5B0 rejects inactive/removed Things and yields while the
        // component-31 +24 busy byte remains set. It returns true as soon as
        // it queues the resource wrapper; it does not wait for an AI action.
        if (!active || componentBusy)
            return false;

        Current = new ScriptedThingLease(++_serial, scriptName, mode);
        return true;
    }

    public void Release() => Current = null;
}

public sealed class QuestInstructionRuntime
{
    public string Key { get; private set; } = "";
    public string Body { get; private set; } = "";
    public bool Active { get; private set; }
    public int Serial { get; private set; }

    public void Show(string key, string? body)
    {
        Key = key;
        Body = body ?? "";
        Active = true;
        Serial++;
    }

    public void Clear() => Active = false;
}

public sealed class QuestHudRuntime
{
    private readonly List<QuestHudItem> _items = [];
    public IReadOnlyList<QuestHudItem> Items => _items;

    public int Create(string name, float value)
    {
        var handle = _items.Count + 1;
        _items.Add(new QuestHudItem(handle, name, value, false));
        return handle;
    }

    public void SetEnabled(int handle, bool enabled)
    {
        var index = handle - 1;
        if ((uint)index >= (uint)_items.Count)
            return;
        _items[index] = _items[index] with { Enabled = enabled };
    }
}

public readonly record struct QuestHudItem(
    int Handle, string Name, float Value, bool Enabled);
