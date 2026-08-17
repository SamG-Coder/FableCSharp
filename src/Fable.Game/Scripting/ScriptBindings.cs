using System.Numerics;
using Fable.Formats.Tng;

namespace Fable.Game.Scripting;

/// <summary>
/// Live name environment: globals (HERO), RegisterActor,
/// Create aliases, CrowdAcquire + indexed SPECTATORCS0..n,
/// invocation-local overwrite.
/// </summary>
public sealed class ScriptBindings
{
    public const string HeroAlias = "HERO";

    private readonly Dictionary<string, BindingSlot> _slots =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<string>> _crowds =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _registered = [];
    private readonly List<string> _changes = [];

    public IReadOnlyDictionary<string, BindingSlot> Slots => _slots;
    public IReadOnlyList<string> RegisteredActors => _registered;
    public IReadOnlyList<string> DrainChanges()
    {
        var copy = _changes.ToArray();
        _changes.Clear();
        return copy;
    }

    public void BindHero(ThingInstance? thing)
    {
        Bind(HeroAlias, BindingKindSlot.Global, thing, HeroAlias);
        if (thing?.ScriptName is { Length: > 0 } name)
            Bind(name, BindingKindSlot.Global, thing, name);
    }

    public void BindSceneThing(ThingInstance thing)
    {
        if (thing.ScriptName is { Length: > 0 } name)
            Bind(name, BindingKindSlot.Scene, thing, name);
    }

    public void RegisterActor(string name)
    {
        if (name.Length == 0)
            return;
        if (!_registered.Contains(name, StringComparer.OrdinalIgnoreCase))
            _registered.Add(name);
        Bind(name, BindingKindSlot.Registered, null, name);
    }

    public void BindCreated(
        string name, string type, string marker, Vector3? position, ThingInstance? thing = null)
    {
        Bind(name, BindingKindSlot.Created, thing, name, type, marker, position);
    }

    public void BindCrowd(string type, string alias, IReadOnlyList<ThingInstance> members)
    {
        var name = alias.Length == 0 ? type : alias;
        var ids = new List<string>();
        for (var i = 0; i < members.Count; i++)
        {
            var indexName = name + i.ToString(System.Globalization.CultureInfo.InvariantCulture);
            ids.Add(indexName);
            Bind(indexName, BindingKindSlot.CrowdIndex, members[i], indexName);
        }

        _crowds[name] = ids;
        Bind(name, BindingKindSlot.Crowd, null, name, type, "", null);
    }

    public bool TryCrowd(string name, out IReadOnlyList<string> members)
    {
        if (_crowds.TryGetValue(name, out var list))
        {
            members = list;
            return true;
        }

        members = [];
        return false;
    }

    public void BindAcquired(string alias, ThingInstance? thing, string source)
    {
        if (alias.Length == 0)
            return;
        Bind(alias, BindingKindSlot.Acquired, thing, source);
    }

    public BindingSlot? Resolve(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        return _slots.TryGetValue(name, out var slot) ? slot : null;
    }

    public void Unbind(string name)
    {
        if (_slots.Remove(name))
            _changes.Add($"unbind {name}");
        _crowds.Remove(name);
    }

    private void Bind(
        string name,
        BindingKindSlot kind,
        ThingInstance? thing,
        string alias,
        string type = "",
        string marker = "",
        Vector3? position = null)
    {
        _slots[name] = new BindingSlot(name, kind, thing, alias, type, marker, position);
        _changes.Add($"{kind}:{name}");
    }
}

public readonly record struct BindingSlot(
    string Name,
    BindingKindSlot Kind,
    ThingInstance? Thing,
    string Alias,
    string Type,
    string Marker,
    Vector3? Position);

public enum BindingKindSlot
{
    Global,
    Scene,
    Registered,
    Created,
    Crowd,
    CrowdIndex,
    Acquired,
    Local,
}
