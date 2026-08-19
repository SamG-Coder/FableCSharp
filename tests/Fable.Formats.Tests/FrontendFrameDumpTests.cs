using Fable.Core;
using Fable.Game;

namespace Fable.Formats.Tests;

public sealed class FrontendFrameDumpTests
{
    [Fact]
    public void Press_Start_frame_dump_is_engine_state_after_one_pump()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.Equal(EngineStage.Frontend, life.Stage);
        Assert.True(life.Pump());

        var rows = life.DumpFrontendFrame();
        Assert.Contains(rows, r => r.Name == EngineLifecycle.FrontendPressStartMenu);
        Assert.True(life.FrontendChildCount >= 6, $"children={life.FrontendChildCount}");
        Assert.True(rows.Count >= 7, $"widgets={rows.Count}");
        Assert.Contains(rows, r =>
            r.Name == EngineLifecycle.FrontendPressStartText &&
            r.TextTag == EngineLifecycle.FrontendPressStartTextTag);
        Assert.Contains(rows, r => r.Name == "UI_FRONTEND_BG_FORREST_1_1" && r.Visible);
        Assert.Contains(rows, r => r.Name == "UI_FRONTEND_BG_FORREST_2_1" && r.Visible);
        var col3 = Assert.Single(rows, r => r.Name == "UI_FRONTEND_BG_FORREST_1_3");
        Assert.Equal(819f, col3.DestX0);
        Assert.Equal(1024f, col3.DestX1);

        var table = FrontendFrameDump.Format(rows, life.FrontendBatch?.Draws.Length ?? 0);
        var implementer = Path.Combine(
            RepoRoot(), "implementer", "frontend", "17-press-start-frame.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(implementer)!);
        File.WriteAllText(implementer, table);
        Assert.True(File.Exists(implementer));
        Assert.Contains("UI_FRONTEND_PRESS_START_MENU", table, StringComparison.Ordinal);
        Assert.Contains("TEXT_GUI_MENU_PRESS_BUTTON", table, StringComparison.Ordinal);

        if (Directory.Exists(ExportDir.Root) ||
            File.Exists(Path.Combine(RepoRoot(), "FableCSharp.slnx")))
        {
            var export = ExportDir.PathFor("frontend", "press-start-frame.txt");
            File.WriteAllText(export, table);
        }
    }

    private static string RepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(dir))
        {
            if (File.Exists(Path.Combine(dir, "FableCSharp.slnx")))
                return dir;
            dir = Path.GetDirectoryName(dir);
        }

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }

    [Fact]
    public void New_Profile_widget_dump_has_required_fields()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.QueueInput(EngineInput.Type4, 0);
        life.QueueInput(EngineInput.Type6, 0);
        Assert.True(life.Pump());
        Assert.Equal(EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        var text = FrontendFrameDump.FormatNewProfile(life.FrontendWidgets);
        Assert.Contains("UI_FRONTEND_NEW_PROFILE_SCREEN", text, StringComparison.Ordinal);
        Assert.Contains("UI_NEW_PROFILE_MENU", text, StringComparison.Ordinal);
        Assert.Contains("UI_ACCEPT_NEW_PROFILE", text, StringComparison.Ordinal);
        Assert.Contains("UI_CANCEL", text, StringComparison.Ordinal);
        Assert.Contains("authoredX", text, StringComparison.Ordinal);
        Assert.Contains("hitX0", text, StringComparison.Ordinal);
        Assert.Contains("textOriginX", text, StringComparison.Ordinal);
        var rows = life.FrontendWidgets.Where(w => w.ParentName == "UI_NEW_PROFILE_MENU").ToList();
        Assert.True(rows.Select(w => w.DestY0).Distinct().Count() >= 4);
        foreach (var widget in life.FrontendWidgets)
        {
            if (widget.DestX1 > widget.DestX0 && widget.DestY1 > widget.DestY0)
            {
                Assert.Equal(widget.DestX0, widget.HitX0);
                Assert.Equal(widget.DestY0, widget.HitY0);
                Assert.Equal(widget.DestX1, widget.HitX1);
                Assert.Equal(widget.DestY1, widget.HitY1);
            }
        }

        life.SetFrontendPointer(12f, 12f);
        Assert.Null(FrontendHitTest.HitIndex(life.FrontendWidgets, 12f, 12f));
        var scratch = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Temp", "grok-goal-d6d68af8e2ab", "implementer");
        Directory.CreateDirectory(scratch);
        var stamp = File.Exists(Path.Combine(scratch, "new-profile-widgets-1.txt")) ? "2" : "1";
        life.WriteNewProfileWidgetDump(Path.Combine(scratch, $"new-profile-widgets-{stamp}.txt"));
        File.WriteAllText(
            Path.Combine(scratch, $"new-profile-launch-{stamp}.log"),
            $"screen={life.FrontendMenuRoot} widgets={life.FrontendWidgets.Count} emptyHit=null");
    }
}
