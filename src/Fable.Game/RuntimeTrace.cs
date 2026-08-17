using System.Globalization;
using System.Text;

namespace Fable.Game;

/// <summary>
/// Deterministic New Game interpreter trace. Same fields
/// every run: frame/time, script, PC, raw, verb, args,
/// return, yield, side effects, persist, interpreters,
/// camera, animation, world.
/// </summary>
public sealed class RuntimeTrace
{
    public IReadOnlyList<RuntimeTraceStep> Steps => _steps;
    private readonly List<RuntimeTraceStep> _steps = [];

    public void Add(RuntimeTraceStep step) => _steps.Add(step);

    public string Format()
    {
        var sb = new StringBuilder();
        foreach (var s in _steps)
        {
            sb.Append("frame=").Append(s.Frame);
            sb.Append(" time=").Append(s.Time.ToString("0.###", CultureInfo.InvariantCulture));
            sb.Append(" script=").Append(s.Script);
            sb.Append(" pc=").Append(s.Pc);
            sb.Append(" raw=").Append(s.Raw);
            sb.Append(" verb=").Append(s.Verb);
            sb.Append(" args=").Append(s.Arguments);
            sb.Append(" return=").Append(s.Return);
            sb.Append(" yield=").Append(s.Yielded ? 1 : 0);
            sb.Append(" finished=").Append(s.Finished ? 1 : 0);
            sb.Append(" status=").Append(s.Status);
            sb.Append(" side=").Append(s.SideEffect);
            sb.Append(" persist=").Append(s.Persist);
            sb.Append(" interpreters=").Append(s.InterpreterCount);
            sb.Append(" camera=").Append(s.Camera);
            sb.Append(" animation=").Append(s.Animation);
            sb.Append(" world=").Append(s.World);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public void Write(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(path, Format());
    }
}

public readonly record struct RuntimeTraceStep(
    int Frame,
    float Time,
    string Script,
    int Pc,
    string Raw,
    string Verb,
    string Arguments,
    ScriptFlow Return,
    bool Yielded,
    bool Finished,
    CommandStatus Status,
    string SideEffect,
    string Persist,
    int InterpreterCount,
    string Camera,
    string Animation,
    string World);

public interface IScriptTrace
{
    void OnStep(RuntimeTraceStep step);
}
