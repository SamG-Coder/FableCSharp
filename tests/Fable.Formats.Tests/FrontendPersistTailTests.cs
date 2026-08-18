using System.Text;
using Fable.Core;
using Fable.Formats.Defs;
using Fable.Game;

namespace Fable.Formats.Tests;

/// <summary>
/// Sequential persist tail after
/// <see cref="FrontendUiDef.UnreadNestedCrc"/>.
/// Native CUIDef load does not stop there.
/// </summary>
public sealed class FrontendPersistTailTests
{
    [Fact]
    public void Nested_crc_does_not_end_the_file()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
        var bin = GameBin.Load(install.FindCompiledDef("frontend.bin")!, names);
        var crcNames = KnownCrcNames();
        var sb = new StringBuilder();
        string[] widgets =
        [
            "UI_FRONTEND_PRESS_START_MENU",
            "UI_TITLE",
            "UI_TITLE_01",
            "UI_TITLE_02",
            "UI_BLENDING_BACKGROUNDS_FORREST",
            "UI_FRONTEND_BG_FORREST_1_1",
            "UI_FRONTEND_BG_FORREST_1_2",
            "UI_FRONTEND_BG_FORREST_2_1",
            "UI_PRESS_START_TEXT",
            "UI_PRESS_START_SWAP",
            "UI_MOUSE_POINTER",
        ];
        foreach (var name in widgets)
        {
            var entry = bin.FindEntry(name);
            Assert.NotNull(entry);
            sb.AppendLine($"===== {name} raw={entry.Raw.Length} =====");
            DumpSequential(entry, crcNames, sb);
        }

        var forest = new StringBuilder();
        foreach (var entry in bin.Entries)
        {
            if (entry.InstanceName is null ||
                !entry.InstanceName.Contains("FORREST", StringComparison.OrdinalIgnoreCase))
                continue;
            var parsed = FrontendUiDef.TryParse(entry);
            if (parsed is null)
                continue;
            forest.AppendLine(
                $"{parsed.InstanceName}\ttype={parsed.Type}\txy={parsed.PositionX},{parsed.PositionY}\t" +
                $"wh={parsed.Width},{parsed.Height}\tg={parsed.GraphicBankId}\t" +
                $"partial={parsed.Partial}\tunread=0x{(parsed.UnreadCrcs.Count == 0 ? 0 : parsed.UnreadCrcs[0]):X8}");
        }

        var brute = BruteForce(FrontendUiDef.UnreadNestedCrc);
        sb.AppendLine("nested-crc-names:");
        foreach (var hit in brute)
            sb.AppendLine($"  {hit}");

        var text = sb.ToString();
        Directory.CreateDirectory(Path.Combine("implementer", "frontend"));
        File.WriteAllText(Path.Combine("implementer", "frontend", "18-persist-tail.txt"), text);
        File.WriteAllText(ExportDir.PathFor("frontend", "persist-tail.txt"), text);
        File.WriteAllText(Path.Combine("implementer", "frontend", "17-forest-persist.txt"), forest.ToString());
        File.WriteAllText(ExportDir.PathFor("frontend", "forest-persist.txt"), forest.ToString());
        Assert.Contains("0x56A59976", text);
        Assert.Contains("UI_FRONTEND_BG_FORREST_1_1", forest.ToString());
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

    private static Dictionary<uint, string> KnownCrcNames()
    {
        string[] names =
        [
            "Type", "Children", "Width", "Height", "PositionX", "PositionY",
            "Text", "TextTag", "Font", "GraphicIndex", "Sprites", "States",
            "ColourR", "ColourG", "ColourB", "ColourA", "Layer", "Angle",
            "ScaleX", "ScaleY", "Centre", "Center", "Absolute",
            "Visible", "Enabled", "Clip", "FlipU", "FlipV",
            "Style", "Styles", "State", "Graphic", "Texture", "Sprite",
            "OffsetX", "OffsetY", "U0", "V0", "U1", "V1",
            "Justify", "Justification", "HAlign", "VAlign",
            "ScaleToViewport", "ScalePosition", "ScaleSize",
            "Relative", "ParentRelative", "Origin", "Anchor",
        ];
        var map = new Dictionary<uint, string>();
        foreach (var name in names)
            map.TryAdd(FableCrc.Hash(name), name);
        map.TryAdd(FrontendUiDef.TextTagCrc, "TextId");
        map.TryAdd(FrontendUiDef.Unknown0961Crc, "u0961");
        map.TryAdd(FrontendUiDef.Unknown38BBCrc, "u38BB");
        map.TryAdd(FrontendUiDef.Unknown6B10Crc, "u6B10");
        map.TryAdd(FrontendUiDef.UnknownF81FCrc, "uF81F");
        map.TryAdd(FrontendUiDef.UnknownE78ECrc, "uE78E");
        map.TryAdd(FrontendUiDef.Unknown9089Crc, "u9089");
        map.TryAdd(FrontendUiDef.UnknownF97DCrc, "uF97D");
        map.TryAdd(FrontendUiDef.UnknownA5F8Crc, "uA5F8");
        map.TryAdd(FrontendUiDef.UnreadNestedCrc, "NESTED");
        return map;
    }

    private static void DumpSequential(
        GameBinEntry entry, Dictionary<uint, string> names, StringBuilder sb)
    {
        var raw = entry.Raw;
        var cursor = entry.BodyOffset > 0 ? entry.BodyOffset : FrontendUiDef.HeaderBytes;
        if (cursor + 6 <= raw.Length &&
            BitConverter.ToUInt16(raw, cursor) == 0 &&
            BitConverter.ToUInt32(raw, cursor + 2) == FrontendUiDef.TypeCrc)
            cursor += 2;
        var steps = 0;
        while (cursor + 4 <= raw.Length && steps < 200)
        {
            var crc = BitConverter.ToUInt32(raw, cursor);
            names.TryGetValue(crc, out var label);
            var payload = cursor + 4;
            var i32 = payload + 4 <= raw.Length ? BitConverter.ToInt32(raw, payload) : 0;
            var f32 = payload + 4 <= raw.Length ? BitConverter.ToSingle(raw, payload) : 0f;
            var u8 = payload < raw.Length ? raw[payload] : (byte)0;
            sb.AppendLine(
                $"  @{cursor:D4} 0x{crc:X8} {label ?? "?"} i32={i32} f32={f32} u8={u8} remain={raw.Length - payload}");
            if (crc == FrontendUiDef.ChildrenCrc && payload + 4 <= raw.Length &&
                i32 is >= 0 and <= 256)
            {
                cursor = payload + 4 + i32 * 4;
                steps++;
                continue;
            }

            if (crc == FrontendUiDef.TextTagCrc)
            {
                var t = payload;
                while (t + 1 < raw.Length)
                {
                    var ch = BitConverter.ToUInt16(raw, t);
                    t += 2;
                    if (ch == 0)
                        break;
                }

                cursor = t;
                steps++;
                continue;
            }

            if (crc == FrontendUiDef.UnreadNestedCrc)
            {
                var n = Math.Min(64, raw.Length - payload);
                sb.AppendLine($"  nested-hex {Convert.ToHexString(raw.AsSpan(payload, n))}");
                cursor = payload;
                // Continue as CRC stream so extra States / flags are visible.
                steps++;
                if (cursor + 4 <= raw.Length && BitConverter.ToUInt32(raw, cursor) == crc)
                    cursor += 4;
                continue;
            }

            cursor = payload + 4;
            steps++;
        }
    }

    private static List<string> BruteForce(uint want)
    {
        string[] seeds =
        [
            "Style", "Styles", "UIStyle", "CUIStyle", "CStyle", "WidgetStyle",
            "Appearance", "Look", "Visual", "SpriteState", "StateDef",
            "CUISprite", "CUIGraphic", "GraphicDef", "SpriteDef",
            "CUIDefStyle", "UIState", "MenuStyle", "DrawStyle",
            "Material", "Effect", "Blend", "ClipRect", "Scissor",
            "Sound", "Action", "Event", "Message", "OnSelect",
            "Highlight", "Disabled", "Hover", "Pressed", "Selected",
            "Normal", "Focus", "Current", "Default",
            "CUIObject", "CUIBase", "CUIWidget", "CUIElement",
            "Nested", "SubDef", "ChildDef", "Embed", "Object",
            "CDef", "Def", "Record", "Block", "Chunk",
            "ScaleToScreen", "ScaleWithResolution", "ResolutionScale",
            "CentreX", "CentreY", "CenterAligned", "HCentre",
            "AbsolutePosition", "RelativePosition", "ScreenSpace",
            "UseViewport", "FitToScreen", "Widescreen",
            "CUIText", "CTextStyle", "FontStyle",
        ];
        var hits = new List<string>();
        foreach (var seed in seeds)
        {
            if (FableCrc.Hash(seed) == want)
                hits.Add(seed);
        }

        return hits;
    }
}
