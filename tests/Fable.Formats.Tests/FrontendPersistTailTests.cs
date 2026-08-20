using System.Text;
using Fable.Core;
using Fable.Formats.Defs;
using Fable.Game;

namespace Fable.Formats.Tests;

public sealed class FrontendPersistTailTests
{
    [Fact]
    public void Every_frontend_persist_has_a_complete_named_schema()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
        var bin = GameBin.Load(install.FindCompiledDef("frontend.bin")!, names);

        foreach (var entry in bin.Entries.Where(e => e.TypeName == "UI"))
        {
            Assert.True(FrontendUiSchema.TryConsume(entry, out var end, out var error),
                $"{entry.InstanceName}: {error}");
            Assert.Equal(entry.Raw.Length, end);
        }

        Assert.Equal(109, FrontendUiFieldCatalog.Fields.Count);
        Assert.Equal(14, FrontendUiFieldCatalog.StateFields.Count);
    }

    [Fact]
    public void Press_Start_engine_frame_is_dumped()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.Equal(EngineStage.Frontend, life.Stage);
        Assert.True(life.Pump());
        var sb = new StringBuilder();
        sb.AppendLine("name\ttype\tparent\tvisible\tgraphic\ttex\tauthXYWH\tdest\tuv\tcolour\ttext\tglyphs\toffscreen\tzero\toutsideParent");
        var dests = new Dictionary<string, (float X0, float Y0, float X1, float Y1)>(StringComparer.OrdinalIgnoreCase);
        var order = 0;
        foreach (var w in life.FrontendWidgets)
        {
            dests[w.Name] = (w.DestX0, w.DestY0, w.DestX1, w.DestY1);
            var off = w.DestX1 <= 0 || w.DestY1 <= 0 ||
                      w.DestX0 >= EngineLifecycle.DisplayDefaultWidth ||
                      w.DestY0 >= EngineLifecycle.DisplayDefaultHeight;
            var zero = w.DestX1 <= w.DestX0 || w.DestY1 <= w.DestY0;
            var outside = false;
            if (w.ParentName is { } parent && dests.TryGetValue(parent, out var p) &&
                p.X1 > p.X0 && p.Y1 > p.Y0)
            {
                outside = w.DestX0 < p.X0 - 0.5f || w.DestY0 < p.Y0 - 0.5f ||
                          w.DestX1 > p.X1 + 0.5f || w.DestY1 > p.Y1 + 0.5f;
            }

            sb.AppendLine(
                $"{w.Name}\t{w.Type}\t{w.ParentName}\t{w.Visible}\t{w.GraphicId}\t{w.TextureName}\t" +
                $"{w.PersistX},{w.PersistY},{w.PersistWidth},{w.PersistHeight}\t" +
                $"{w.DestX0},{w.DestY0},{w.DestX1},{w.DestY1}\t" +
                $"{w.U0},{w.V0},{w.U1},{w.V1}\t0x{w.Colour:X8}\t{w.Text}\t{w.GlyphCount}\t" +
                $"{off}\t{zero}\t{outside}\torder={order}");
            order++;
        }

        sb.AppendLine($"drawOrderCount={order} batchDraws={life.FrontendBatch?.Draws.Length}");
        var text = sb.ToString();
        Directory.CreateDirectory(Path.Combine("implementer", "frontend"));
        File.WriteAllText(Path.Combine("implementer", "frontend", "17-press-start-frame.txt"), text);
        File.WriteAllText(ExportDir.PathFor("frontend", "press-start-frame.txt"), text);
        Assert.Contains("UI_FRONTEND_PRESS_START_MENU", text);
    }
}
