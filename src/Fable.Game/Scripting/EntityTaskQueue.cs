using System.Numerics;

namespace Fable.Game.Scripting;

/// <summary>
/// Native entity command slot: one current task per
/// actor. WalkTo / PlayAnimation / ClearCommands
/// replace the slot; they are not a C# Task queue.
/// WaitTask / WaitPlayAnimation poll this slot
/// (vtbl+104 leftover).
/// </summary>
public sealed class EntityTaskQueue
{
    private readonly Dictionary<string, EntityTask> _byActor =
        new(StringComparer.OrdinalIgnoreCase);
    private int _next;

    public IReadOnlyDictionary<string, EntityTask> ByActor => _byActor;

    public EntityTask Replace(
        string? actor, EntityTaskKind kind, string name, Vector3? destination, float speed)
    {
        var task = new EntityTask($"task-{++_next}", kind, actor, name, destination, speed);
        if (actor is { Length: > 0 })
        {
            if (_byActor.TryGetValue(actor, out var prior))
                prior.Cancel();
            _byActor[actor] = task;
        }

        return task;
    }

    public EntityTask? Current(string? actor) =>
        actor is { Length: > 0 } && _byActor.TryGetValue(actor, out var task) ? task : null;

    public void Clear(string? actor)
    {
        if (actor is { Length: > 0 } && _byActor.TryGetValue(actor, out var task))
            task.Cancel();
    }

    public void Tick(float dt, WorldRuntime world)
    {
        foreach (var task in _byActor.Values)
        {
            if (task.Complete)
                continue;
            if (task.Kind is EntityTaskKind.Walk or EntityTaskKind.Run
                or EntityTaskKind.Sneak or EntityTaskKind.Follow)
                task.TickMove(dt, world);
            else if (task.Kind is EntityTaskKind.Animate or EntityTaskKind.CombatAnimate
                     or EntityTaskKind.LoopAnimate)
                task.TickAnim();
        }
    }
}

public enum EntityTaskKind
{
    None,
    Walk,
    Run,
    Sneak,
    Animate,
    LoopAnimate,
    CombatAnimate,
    Follow,
}

public sealed class EntityTask
{
    public string Id { get; }
    public EntityTaskKind Kind { get; }
    public string? Actor { get; }
    public string Name { get; }
    public Vector3? Destination { get; }
    public float Speed { get; }
    public bool Complete { get; private set; }
    public bool Cancelled { get; private set; }

    public EntityTask(
        string id, EntityTaskKind kind, string? actor, string name,
        Vector3? destination, float speed)
    {
        Id = id;
        Kind = kind;
        Actor = actor;
        Name = name;
        Destination = destination;
        Speed = speed;
    }

    public void MarkComplete() => Complete = true;

    public void Cancel()
    {
        Cancelled = true;
        Complete = true;
    }

    public void TickMove(float dt, WorldRuntime world)
    {
        if (Actor is not { Length: > 0 } || Destination is not { } dest)
        {
            Complete = true;
            return;
        }

        if (!world.Positions.TryGetValue(Actor, out var pos))
        {
            world.Positions[Actor] = dest;
            Complete = true;
            return;
        }

        if (Speed <= 0f)
        {
            world.Positions[Actor] = dest;
            Complete = true;
            return;
        }

        var delta = dest - pos;
        var dist = delta.Length();
        var step = Speed * Math.Max(dt, 0f);
        if (step >= dist || dist <= 0.0001f)
        {
            world.Positions[Actor] = dest;
            Complete = true;
            return;
        }

        world.Positions[Actor] = pos + delta * (step / dist);
    }

    public void TickAnim()
    {
        // Playback length unread. Leftover poll
        // (WaitPlayAnimation / PumpUntilSettled)
        // marks complete; this tick only keeps
        // the slot alive.
    }

    public PendingOperation AsOperation() =>
        new(Id, Kind.ToString(), Actor, Name) { Complete = Complete };
}
