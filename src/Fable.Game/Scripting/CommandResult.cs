namespace Fable.Game.Scripting;

/// <summary>
/// Handler result. The scheduler, not a Verb→ScriptFlow
/// table, decides continuation.
/// </summary>
public readonly record struct CommandResult(
    ExecutionKind Kind,
    CommandStatus Status,
    CommandFamily Family,
    string YieldReason,
    string ResumeReason,
    string? OperationId,
    string SideEffect,
    string BindingChange,
    bool AdvancePc)
{
    public static CommandResult Continue(
        CommandStatus status,
        CommandFamily family,
        string side,
        string binding = "") =>
        new(ExecutionKind.Continue, status, family, "", "", null, side, binding, true);

    public static CommandResult YieldOnce(
        CommandStatus status,
        CommandFamily family,
        string reason,
        string side,
        string? op = null) =>
        new(ExecutionKind.YieldOnce, status, family, reason, "vtbl+28", op, side, "", true);

    public static CommandResult Wait(
        ExecutionKind kind,
        CommandStatus status,
        CommandFamily family,
        string yieldReason,
        string resumeReason,
        string? op,
        string side,
        bool advanceWhenDone = true) =>
        new(kind, status, family, yieldReason, resumeReason, op, side, "", advanceWhenDone);

    public static CommandResult Blocked(
        string reason,
        CommandStatus status,
        CommandFamily family,
        string raw) =>
        new(ExecutionKind.Blocked, status, family, reason, "", null, raw, "", false);

    public bool IsBlocked => Kind == ExecutionKind.Blocked;
}

public enum ExecutionKind
{
    Continue,
    YieldOnce,
    WaitFrames,
    WaitScaledFrames,
    BlockPump,
    WaitOperation,
    Blocked,
}

public enum WaitKind
{
    None,
    Frames,
    ScaledFrames,
    YieldOnce,
    Task,
    Dialogue,
    Media,
    Blocked,
}

public sealed class PendingOperation
{
    public string Id { get; }
    public string Kind { get; }
    public string? Actor { get; }
    public string Target { get; }
    public bool Complete { get; set; }

    public PendingOperation(string id, string kind, string? actor, string target)
    {
        Id = id;
        Kind = kind;
        Actor = actor;
        Target = target;
    }
}
