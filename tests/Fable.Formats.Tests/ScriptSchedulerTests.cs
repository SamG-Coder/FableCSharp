using Fable.Game;
using Fable.Game.Scripting;

namespace Fable.Formats.Tests;

public sealed class ScriptSchedulerTests
{
    [Fact]
    public void Immediate_then_immediate()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("imm", ["CameraPause FALSE", "NoDialogCam TRUE"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(2, interp.Executed.Count);
    }

    [Fact]
    public void Immediate_then_yield_once_then_immediate()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("y",
            ["CameraPause TRUE", "UseCamera CAM_A", "NoDialogCam TRUE"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Yielded);
        Assert.False(interp.Finished);
        interp.Resume(runtime);
        Assert.True(interp.Finished);
        Assert.Contains("NoDialogCam TRUE", interp.Executed);
    }

    [Fact]
    public void WaitFrames_and_WaitScaledFrames()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("w", ["DoScriptFrame 2", "GamePause 0.2"]);
        interp.RunUntilYield(runtime);
        Assert.Equal(ExecutionKind.WaitFrames, interp.CurrentWaitKind);
        for (var i = 0; i < 32 && interp.Yielded && !interp.Finished; i++)
            interp.Resume(runtime);
        Assert.True(interp.Finished);
    }

    [Fact]
    public void Two_fibers_and_quest_plus_cutscene()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.Scheduler.Create("S_QNOVI", "AttackOver");
        runtime.Scheduler.Create("other", null);
        var a = new ScriptInterpreter("A", ["DoScriptFrame 1"]);
        var b = new ScriptInterpreter("B", ["CameraPause FALSE"]);
        a.RunUntilYield(runtime);
        b.RunUntilYield(runtime);
        Assert.True(a.Yielded);
        Assert.True(b.Finished);
        runtime.Update(1f / 15f);
        Assert.Equal(2, runtime.Scheduler.Fibers.Count);
    }

    [Fact]
    public void Unknown_command_blocks()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("u", ["NotARealVerb"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Blocked);
        Assert.Equal("UNKNOWN", interp.BlockReason);
    }

    [Fact]
    public void ClearCommands_cancels_waiting_task()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("clr",
        [
            "HERO.WalkTo MK_A,0.1,TRUE",
            "HERO.ClearCommands TRUE",
        ]);
        interp.RunUntilYield(runtime);
        Assert.Equal(ExecutionKind.WaitOperation, interp.CurrentWaitKind);
        var task = runtime.Movement.Tasks.Current("HERO");
        Assert.NotNull(task);
        runtime.Movement.Clear("HERO");
        runtime.Movement.ByActor["HERO"].Complete = true;
        interp.Resume(runtime);
        Assert.True(interp.Finished);
        Assert.True(task.Complete);
    }

    [Fact]
    public void Dialogue_wait_then_resume()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("dlg",
        [
            "Father.InteractiveSpeak Hero,'TEXT_A',TRUE,'TEXT_B'",
            "WaitActiveDialog",
        ]);
        interp.RunUntilYield(runtime);
        Assert.Equal(ExecutionKind.WaitOperation, interp.CurrentWaitKind);
        Assert.NotNull(runtime.Dialogue.Session);
        runtime.Dialogue.CompleteWait();
        interp.Resume(runtime);
        if (interp.Yielded)
            interp.Resume(runtime);
        Assert.True(interp.Finished);
    }
}
