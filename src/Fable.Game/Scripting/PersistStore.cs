namespace Fable.Game.Scripting;

/// <summary>
/// Persist slots. AttackOver is <c>00DAADA0</c>
/// <c>004045C0("AttackOver", this+80)</c>. Writer UNREAD.
/// </summary>
public sealed class PersistStore
{
    private readonly Dictionary<string, PersistValue> _slots =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, PersistValue> Slots => _slots;

    public void InstallRecovered()
    {
        Install(PersistTable.Recovered);
    }

    public void Install(IEnumerable<PersistSlot> slots)
    {
        foreach (var slot in slots)
        {
            if (_slots.ContainsKey(slot.Name))
                continue;
            _slots[slot.Name] = slot.Kind == PersistKind.Int
                ? PersistValue.FromInt(0)
                : PersistValue.FromBool(slot.DefaultBool);
        }
    }

    public void SetBool(string name, bool value) =>
        _slots[name] = PersistValue.FromBool(value);

    public void SetInt(string name, int value) =>
        _slots[name] = PersistValue.FromInt(value);

    public bool Bool(string name) =>
        _slots.TryGetValue(name, out var value) &&
        value.Kind == PersistKind.Bool && value.Bool;

    public int Int32(string name) =>
        _slots.TryGetValue(name, out var value) && value.Kind == PersistKind.Int
            ? value.Int32
            : 0;

    public PersistKind TypeOf(string name) =>
        _slots.TryGetValue(name, out var value) ? value.Kind : PersistKind.Unread;

    public string Snapshot()
    {
        if (_slots.Count == 0)
            return "";
        return string.Join(",", _slots.Select(p => $"{p.Key}={p.Value.Kind}:{p.Value.Bool}"));
    }
}
