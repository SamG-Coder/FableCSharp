namespace Fable.Game;

/// <summary>
/// Forward-lifecycle log: native VA, stage,
/// object/subsystem, construction, resource,
/// update/render, video, world, New Game.
/// Answers "what happened from WinMain to
/// this frame?"
/// </summary>
public sealed class ForwardLifecycleTrace
{
    public readonly List<ForwardLifecycleEvent> Events = [];

    public void Add(
        uint va,
        string stage,
        string subsystem,
        string action,
        string? detail = null)
    {
        Events.Add(new ForwardLifecycleEvent(va, stage, subsystem, action, detail ?? ""));
    }

    public void Write(string path)
    {
        using var writer = new StreamWriter(path);
        foreach (var e in Events)
        {
            writer.Write("0x");
            writer.Write(e.Va.ToString("X8"));
            writer.Write('\t');
            writer.Write(e.Stage);
            writer.Write('\t');
            writer.Write(e.Subsystem);
            writer.Write('\t');
            writer.Write(e.Action);
            if (e.Detail.Length > 0)
            {
                writer.Write('\t');
                writer.Write(e.Detail);
            }

            writer.WriteLine();
        }
    }
}

public readonly record struct ForwardLifecycleEvent(
    uint Va,
    string Stage,
    string Subsystem,
    string Action,
    string Detail);
