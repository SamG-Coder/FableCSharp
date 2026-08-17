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
        foreach (var slot in PersistTable.Recovered)
        {
            if (!_slots.ContainsKey(slot.Name))
                _slots[slot.Name] = PersistValue.FromBool(slot.DefaultBool);
        }
    }

    public void SetBool(string name, bool value) =>
        _slots[name] = PersistValue.FromBool(value);

    public bool Bool(string name) =>
        _slots.TryGetValue(name, out var value) &&
        value.Kind == PersistKind.Bool && value.Bool;

    public PersistKind TypeOf(string name) =>
        _slots.TryGetValue(name, out var value) ? value.Kind : PersistKind.Unread;

    public string Snapshot()
    {
        if (_slots.Count == 0)
            return "";
        return string.Join(",", _slots.Select(p => $"{p.Key}={p.Value.Kind}:{p.Value.Bool}"));
    }
}
