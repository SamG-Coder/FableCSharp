namespace Fable.Game.Scripting;

/// <summary>
/// Named byte map at <c>008ADF10</c>. Miss inserts
/// default 0 and returns node+20. SetFlag writes 0/1.
/// WaitFlag leftover-polls until <c>[eax]==expected</c>.
/// Not a timer and not persist.
/// </summary>
public sealed class FlagStore
{
    private readonly Dictionary<string, byte> _flags =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, byte> Slots => _flags;
    public PendingOperation? WaitOp { get; private set; }
    public string? WaitName { get; private set; }
    public byte WaitExpected { get; private set; }
    private int _waitSerial;

    /// <summary>
    /// <c>008ADF10</c> insert-on-miss default 0.
    /// </summary>
    public byte GetOrInsert(string name)
    {
        if (!_flags.TryGetValue(name, out var value))
        {
            value = 0;
            _flags[name] = 0;
        }

        return value;
    }

    public void Set(string name, byte value) => _flags[name] = value;

    public bool Matches(string name, byte expected) =>
        GetOrInsert(name) == expected;

    public PendingOperation Wait(string name, byte expected)
    {
        var op = new PendingOperation($"flag-{++_waitSerial}", "WaitFlag", null, name);
        WaitName = name;
        WaitExpected = expected;
        if (GetOrInsert(name) == expected)
        {
            op.Complete = true;
            return op;
        }

        WaitOp = op;
        return op;
    }

    public bool IsWaiting(string? id) =>
        WaitOp is { Complete: false } op &&
        (id is null || op.Id == id);

    public bool Poll()
    {
        if (WaitOp is null)
            return true;
        if (WaitName is null)
            return false;
        if (GetOrInsert(WaitName) == WaitExpected)
        {
            WaitOp.Complete = true;
            return true;
        }

        return false;
    }

    public string Snapshot()
    {
        if (_flags.Count == 0)
            return "";
        return string.Join(",", _flags.Select(p => $"{p.Key}={p.Value}"));
    }
}
