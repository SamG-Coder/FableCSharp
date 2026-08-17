using System.Globalization;
using System.Text;
using Fable.Game.Scripting;

namespace Fable.Game;

/// <summary>
/// Structural runtime + scheduler trace.
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
            sb.Append(" quest=").Append(s.Quest);
            sb.Append(" script=").Append(s.Script);
            sb.Append(" pc=").Append(s.Pc);
            sb.Append(" raw=").Append(s.Raw);
            sb.Append(" verb=").Append(s.Verb);
            sb.Append(" target=").Append(s.Target);
            sb.Append(" args=").Append(s.Arguments);
            sb.Append(" family=").Append(s.Family);
            sb.Append(" result=").Append(s.Result);
            sb.Append(" status=").Append(s.Status);
            sb.Append(" yield_reason=").Append(s.YieldReason);
            sb.Append(" resume_reason=").Append(s.ResumeReason);
            sb.Append(" op=").Append(s.OperationId);
            sb.Append(" yield=").Append(s.Yielded ? 1 : 0);
            sb.Append(" finished=").Append(s.Finished ? 1 : 0);
            sb.Append(" blocked=").Append(s.Blocked ? 1 : 0);
            sb.Append(" side=").Append(s.SideEffect);
            sb.Append(" persist=").Append(s.Persist);
            sb.Append(" interpreters=").Append(s.InterpreterCount);
            sb.Append(" camera=").Append(s.Camera);
            sb.Append(" animation=").Append(s.Animation);
            sb.Append(" world=").Append(s.World);
            sb.Append(" bind=").Append(s.BindingChange);
            sb.Append(" fiber=").Append(s.FiberId);
            sb.Append(" inst=").Append(s.ScriptInstanceId);
            sb.Append(" fiber_state=").Append(s.FiberState);
            sb.Append(" wake=").Append(s.WakeTime.ToString("0.###", CultureInfo.InvariantCulture));
            sb.Append(" wait=").Append(s.WaitKind);
            sb.Append(" wait_target=").Append(s.WaitTarget);
            sb.Append(" queued=").Append(s.QueuedTask);
            sb.Append(" done=").Append(s.CompletionReason);
            sb.Append(" parse=").Append(s.Parse);
            sb.Append(" dispatch=").Append(s.Dispatch);
            sb.Append(" apply=").Append(s.Apply);
            sb.Append(" runtime=").Append(s.Runtime);
            sb.Append(" task=").Append(s.Task);
            sb.Append(" dialogue=").Append(s.Dialogue);
            sb.Append(" audio=").Append(s.Audio);
            sb.Append(" created=").Append(s.CreatedThing);
            sb.Append(" removed=").Append(s.RemovedThing);
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
    string Quest,
    string Script,
    int Pc,
    string Raw,
    string Verb,
    string Target,
    string Arguments,
    CommandFamily Family,
    ExecutionKind Result,
    CommandStatus Status,
    string YieldReason,
    string ResumeReason,
    string OperationId,
    bool Yielded,
    bool Finished,
    bool Blocked,
    string SideEffect,
    string Persist,
    int InterpreterCount,
    string Camera,
    string Animation,
    string World,
    string BindingChange,
    int FiberId,
    int ScriptInstanceId,
    string FiberState,
    float WakeTime,
    string WaitKind,
    string WaitTarget,
    string QueuedTask,
    string CompletionReason,
    CommandStatus Parse = CommandStatus.Unread,
    CommandStatus Dispatch = CommandStatus.Unread,
    CommandStatus Apply = CommandStatus.Unread,
    CommandStatus Runtime = CommandStatus.Unread,
    string Task = "",
    string Dialogue = "",
    string Audio = "",
    string CreatedThing = "",
    string RemovedThing = "");

public interface IScriptTrace
{
    void OnStep(RuntimeTraceStep step);
}
