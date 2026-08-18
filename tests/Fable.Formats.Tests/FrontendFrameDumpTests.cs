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
        Assert.Contains(rows, r => r.Name == "UI_FRONTEND_BG_FORREST_2_1" && !r.Visible);
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
}
