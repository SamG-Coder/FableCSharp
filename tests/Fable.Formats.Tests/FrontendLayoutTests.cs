using Fable.Core;
using Fable.Formats.Defs;
using Fable.Game;

namespace Fable.Formats.Tests;

public sealed class FrontendLayoutTests
{
    private static readonly FrontendViewport FirstSeen =
        FrontendLayout.FirstSeenFrontend(1024f, 768f);

    [Fact]
    public void Root_Width_0_stays_dest_0_0_0_0()
    {
        var root = new FrontendWidgetLayout(
            PositionX: 0f,
            PositionY: 0f,
            PersistWidth: 0,
            PersistHeight: 0,
            LeftoverW: 0f,
            LeftoverH: 0f);
        var dest = FrontendLayout.Compute(root, parent: null, FirstSeen);
        Assert.Equal(0f, dest.X0);
        Assert.Equal(0f, dest.Y0);
        Assert.Equal(0f, dest.X1);
        Assert.Equal(0f, dest.Y1);
        Assert.Equal(0x0041AFA0u, FrontendLayout.SubmitDestFn);
        Assert.Equal(360, FrontendLayout.SizeWOffset);
        Assert.Equal(204, FrontendLayout.DestWOffset);
    }

    [Fact]
    public void Scale_init_writes_1_when_plus_280_is_0()
    {
        var one = FrontendLayout.InitInheritedScale(0);
        Assert.Equal(1f, one.ScaleX);
        Assert.Equal(1f, one.ScaleY);
        var left = FrontendLayout.InitInheritedScale(1, 2f, 3f);
        Assert.Equal(2f, left.ScaleX);
        Assert.Equal(3f, left.ScaleY);
        Assert.Equal(272, FrontendLayout.InheritScaleXOffset);
        Assert.Equal(280, FrontendLayout.InheritScaleFlagOffset);
        Assert.Equal(0x005339B0u, FrontendLayout.ScaleWriteFn);
    }

    [Fact]
    public void Press_Start_type6_dest_is_a_point()
    {
        var root = FrontendLayout.Compute(
            new FrontendWidgetLayout(0f, 0f),
            parent: null,
            FirstSeen);
        Assert.Equal(0f, root.X0);
        Assert.Equal(0f, root.X1);

        // Type-6 GraphicIndex=0: leftover +204/+208
        // stay 0. Dest is the remapped origin.
        var text = new FrontendWidgetLayout(
            PositionX: 320f,
            PositionY: 240f,
            LeftoverW: 0f,
            LeftoverH: 0f);
        var dest = FrontendLayout.Compute(text, root, FirstSeen);
        Assert.Equal(320f, dest.OriginX);
        Assert.Equal(240f, dest.OriginY);
        Assert.Equal(320f, dest.X0);
        Assert.Equal(240f, dest.Y0);
        Assert.Equal(320f, dest.X1);
        Assert.Equal(240f, dest.Y1);
    }

    [Fact]
    public void First_seen_does_not_invent_640_480_scale()
    {
        Assert.True(FirstSeen.ResolutionScaleEnabled);
        Assert.Equal(1024f, FirstSeen.Width);
        Assert.Equal(768f, FirstSeen.Height);
        Assert.False(FirstSeen.GamePresent);
        Assert.Equal((1f, 1f), FrontendLayout.GlobalUiScale(FirstSeen));

        var identity = FrontendLayout.ApplyResolutionScale(320f, 240f, FirstSeen);
        Assert.Equal(512f, identity.X);
        Assert.Equal(384f, identity.Y);

        var text = new FrontendWidgetLayout(
            PositionX: 320f,
            PositionY: 240f,
            LeftoverW: 10f,
            LeftoverH: 10f);
        var dest = FrontendLayout.Compute(text, parent: null, FirstSeen);
        Assert.Equal(320f, dest.OriginX);
        Assert.Equal(240f, dest.Y0);
        Assert.NotEqual(512f, dest.OriginX);
        Assert.Equal(640f, FrontendLayout.AuthoredWidth);
        Assert.Equal(480f, FrontendLayout.AuthoredHeight);
    }

    [Fact]
    public void Forest_tile_without_remap_stays_in_authored_640_space()
    {
        var tile = new FrontendWidgetLayout(
            PositionX: 512f,
            PositionY: 0f,
            LeftoverW: 128f,
            LeftoverH: 256f);
        var dest = FrontendLayout.Compute(tile, parent: null, FirstSeen);
        Assert.Equal(512f, dest.X0);
        Assert.Equal(640f, dest.X1);
        Assert.Equal(256f, dest.Y1);
    }

    [Fact]
    public void Forest_tile_with_native_remap_bits_fills_1024_viewport()
    {
        var tile = new FrontendWidgetLayout(
            PositionX: 512f,
            PositionY: 0f,
            LeftoverW: 128f,
            LeftoverH: 256f,
            ScaleOriginToViewport: true,
            ScaleSizeToViewport: true);
        var dest = FrontendLayout.Compute(tile, parent: null, FirstSeen);
        Assert.Equal(819f, dest.X0);
        Assert.Equal(1024f, dest.X1);
        Assert.Equal(410f, dest.Y1);
    }

    [Fact]
    public void Persist_remap_flags_scale_authored_640_480_to_viewport()
    {
        var remapped = new FrontendWidgetLayout(
            PositionX: 320f,
            PositionY: 240f,
            LeftoverW: 10f,
            LeftoverH: 10f,
            ScaleOriginToViewport: true,
            ScaleSizeToViewport: true);
        var dest = FrontendLayout.Compute(remapped, parent: null, FirstSeen);
        Assert.Equal(512f, dest.OriginX);
        Assert.Equal(384f, dest.OriginY);
        Assert.Equal(512f, dest.X0);
        Assert.Equal(384f, dest.Y0);
        Assert.Equal(528f, dest.X1);
        Assert.Equal(400f, dest.Y1);
    }

    [Fact]
    public void Children_inherit_parent_dest_scale()
    {
        var parent = new FrontendDest(100f, 40f, 2f, 2f, 100f, 40f, 100f, 40f);
        var child = new FrontendWidgetLayout(
            PositionX: 10f,
            PositionY: 5f,
            LeftoverW: 8f,
            LeftoverH: 4f);
        var dest = FrontendLayout.Compute(child, parent, FirstSeen);
        Assert.Equal(120f, dest.OriginX);
        Assert.Equal(50f, dest.OriginY);
        Assert.Equal(2f, dest.ScaleX);
        Assert.Equal(2f, dest.ScaleY);
        Assert.Equal(120f, dest.X0);
        Assert.Equal(50f, dest.Y0);
        Assert.Equal(136f, dest.X1);
        Assert.Equal(58f, dest.Y1);
    }

    [Fact]
    public void Submit_uses_leftover_when_persist_size_is_0()
    {
        var leftover = FrontendLayout.ComputeSubmitDest(
            0, 0, 64f, 32f, 0f, 0f, 1f, 1f, center: false);
        Assert.Equal((0f, 0f, 64f, 32f), leftover);
        var persist = FrontendLayout.ComputeSubmitDest(
            20, 10, 64f, 32f, 0f, 0f, 1f, 1f, center: false);
        Assert.Equal((0f, 0f, 20f, 10f), persist);
        var centered = FrontendLayout.ComputeSubmitDest(
            10, 10, 0f, 0f, 100f, 100f, 1f, 1f, center: true);
        Assert.Equal((95f, 95f, 105f, 105f), centered);
        Assert.Equal(0.5f, FrontendLayout.Half);
        Assert.Equal(0x0052F1E0u, FrontendLayout.CenterFn);
    }

    [Fact]
    public void Press_Start_persist_positions_are_640_480_pixels()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
        var bin = GameBin.Load(install.FindCompiledDef("frontend.bin")!, names);
        var rootPos = FirstPos(bin.FindEntry("UI_FRONTEND_PRESS_START_MENU")!.Raw);
        var titlePos = FirstPos(bin.FindEntry("UI_TITLE")!.Raw);
        var textPos = FirstPos(bin.FindEntry("UI_PRESS_START_TEXT")!.Raw);
        Assert.Equal((0f, 0f), rootPos);
        Assert.Equal((70f, 30f), titlePos);
        Assert.Equal((320f, 240f), textPos);

        var rootDest = FrontendLayout.Compute(
            new FrontendWidgetLayout(rootPos.X, rootPos.Y),
            parent: null,
            FirstSeen);
        Assert.Equal(0f, rootDest.X0);
        Assert.Equal(0f, rootDest.X1);

        var titleDest = FrontendLayout.Compute(
            new FrontendWidgetLayout(
                titlePos.X, titlePos.Y,
                LeftoverW: 256f, LeftoverH: 128f),
            rootDest,
            FirstSeen);
        Assert.True(titleDest.X1 > titleDest.X0);
        Assert.Equal(70f, titleDest.OriginX);
        Assert.Equal(30f, titleDest.OriginY);
        Assert.True(titleDest.OriginY < 240f);

        var textDest = FrontendLayout.Compute(
            new FrontendWidgetLayout(
                textPos.X, textPos.Y,
                LeftoverW: 0f, LeftoverH: 0f),
            rootDest,
            FirstSeen);
        Assert.Equal(textDest.X0, textDest.X1);
        Assert.Equal(textDest.Y0, textDest.Y1);
        Assert.Equal(320f, textDest.OriginX);
        Assert.Equal(240f, textDest.OriginY);
    }

    [Fact]
    public void Press_Start_root_remapSize_scales_child_origin_to_viewport()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
        var bin = GameBin.Load(install.FindCompiledDef("frontend.bin")!, names);
        var rootDef = FrontendUiDef.TryParse(bin.FindEntry("UI_FRONTEND_PRESS_START_MENU")!)!;
        var titleDef = FrontendUiDef.TryParse(bin.FindEntry("UI_TITLE")!)!;
        var textDef = FrontendUiDef.TryParse(bin.FindEntry("UI_PRESS_START_TEXT")!)!;
        Assert.True(rootDef.ScaleSizeToViewport);
        Assert.False(rootDef.ScaleOriginToViewport);
        Assert.False(titleDef.ScaleSizeToViewport);
        Assert.False(titleDef.ScaleOriginToViewport);

        var root = FrontendLayout.Compute(
            new FrontendWidgetLayout(
                rootDef.PositionX, rootDef.PositionY,
                ScaleSizeToViewport: rootDef.ScaleSizeToViewport,
                ScaleOriginToViewport: rootDef.ScaleOriginToViewport),
            parent: null,
            FirstSeen);
        Assert.Equal(0f, root.X0);
        Assert.Equal(0f, root.X1);
        Assert.Equal(0f, root.OriginX);
        var scaledOne = FrontendLayout.ApplyResolutionScale(1f, 1f, FirstSeen);
        Assert.Equal(scaledOne.X, root.ScaleX);
        Assert.Equal(scaledOne.Y, root.ScaleY);

        var title = FrontendLayout.Compute(
            new FrontendWidgetLayout(
                titleDef.PositionX, titleDef.PositionY,
                LeftoverW: 256f, LeftoverH: 128f,
                ScaleSizeToViewport: titleDef.ScaleSizeToViewport,
                ScaleOriginToViewport: titleDef.ScaleOriginToViewport),
            root,
            FirstSeen);
        Assert.Equal(70f * scaledOne.X, title.OriginX);
        Assert.Equal(30f * scaledOne.Y, title.OriginY);

        var text = FrontendLayout.Compute(
            new FrontendWidgetLayout(
                textDef.PositionX, textDef.PositionY,
                LeftoverW: 0f, LeftoverH: 0f,
                ScaleSizeToViewport: textDef.ScaleSizeToViewport,
                ScaleOriginToViewport: textDef.ScaleOriginToViewport),
            root,
            FirstSeen);
        Assert.Equal(320f * scaledOne.X, text.OriginX);
        Assert.Equal(240f * scaledOne.Y, text.OriginY);
        Assert.Equal(text.X0, text.X1);
        Assert.Equal(text.Y0, text.Y1);
        Assert.Equal(512f, text.X0);
        Assert.Equal(384f, text.Y0);
    }

    private static (float X, float Y) FirstPos(byte[] raw)
    {
        float x = 0f;
        float y = 0f;
        for (var i = 0; i + 8 <= raw.Length; i++)
        {
            var crc = BitConverter.ToUInt32(raw, i);
            if (crc == FrontendUiDef.PositionXCrc)
            {
                var value = BitConverter.ToSingle(raw, i + 4);
                if (x == 0f && value != 0f)
                    x = value;
            }
            else if (crc == FrontendUiDef.PositionYCrc)
            {
                var value = BitConverter.ToSingle(raw, i + 4);
                if (y == 0f && value != 0f)
                    y = value;
            }
        }

        return (x, y);
    }

    [Fact]
    public void Press_Start_first_seen_dest_table_matches_0041AFA0()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
        var bin = GameBin.Load(install.FindCompiledDef("frontend.bin")!, names);
        using var sprites = new FrontendSpriteBank(install);
        var widgets = FrontendWidgetFactory.Build(bin, "UI_FRONTEND_PRESS_START_MENU", sprites);
        var viewport = FrontendLayout.FirstSeenFrontend(1024f, 768f);
        var dests = new Dictionary<string, FrontendDest>(StringComparer.OrdinalIgnoreCase);
        foreach (var widget in widgets)
        {
            var leftoverW = 0f;
            var leftoverH = 0f;
            if (widget.GraphicId != 0 &&
                widget.TextureName is { } texName &&
                sprites.TryLoad(texName) is { } tex)
            {
                leftoverW = tex.FrameWidth;
                leftoverH = tex.FrameHeight;
            }

            dests.TryGetValue(widget.ParentName ?? "", out var parent);
            FrontendDest? parentDest = widget.ParentName is null ? null : parent;
            dests[widget.Name] = FrontendLayout.Compute(
                new FrontendWidgetLayout(
                    widget.PersistX,
                    widget.PersistY,
                    PersistScaleX: widget.PersistScaleX,
                    PersistScaleY: widget.PersistScaleY,
                    PersistWidth: widget.PersistWidth > 0 ? (int)widget.PersistWidth : 0,
                    PersistHeight: widget.PersistHeight > 0 ? (int)widget.PersistHeight : 0,
                    LeftoverW: leftoverW,
                    LeftoverH: leftoverH,
                    Center: widget.Center,
                    Absolute: widget.Absolute,
                    ScaleOriginToViewport: widget.ScaleOriginToViewport,
                    ScaleSizeToViewport: widget.ScaleSizeToViewport),
                parentDest,
                viewport);
        }

        Assert.Equal((0f, 0f, 0f, 0f), Rect(dests["UI_FRONTEND_PRESS_START_MENU"]));
        Assert.Equal((0f, 0f, 0f, 0f), Rect(dests["UI_BLENDING_BACKGROUNDS_FORREST"]));
        Assert.Equal((0f, 0f, 410f, 410f), Rect(dests["UI_FRONTEND_BG_FORREST_1_1"]));
        Assert.Equal((410f, 0f, 819f, 410f), Rect(dests["UI_FRONTEND_BG_FORREST_1_2"]));
        Assert.Equal((819f, 0f, 1024f, 410f), Rect(dests["UI_FRONTEND_BG_FORREST_1_3"]));
        Assert.Equal((0f, 410f, 410f, 819f), Rect(dests["UI_FRONTEND_BG_FORREST_1_4"]));
        Assert.Equal((112f, 48f, 112f, 48f), Rect(dests["UI_TITLE"]));
        Assert.Equal((112f, 48f, 522f, 253f), Rect(dests["UI_TITLE_01"]));
        Assert.Equal((522f, 48f, 931f, 253f), Rect(dests["UI_TITLE_02"]));
        Assert.Equal((512f, 384f, 512f, 384f), Rect(dests["UI_PRESS_START_TEXT"]));
        Assert.Equal((512f, 544f, 512f, 544f), Rect(dests["UI_LEGAL_TEXT"]));
        Assert.Equal((0f, 0f, 32f, 32f), Rect(dests["UI_MOUSE_POINTER"]));
        Assert.Equal(
            FrontendLayout.ApplyResolutionScale(1f, 1f, viewport).X,
            dests["UI_TITLE_01"].ScaleX);
        Assert.False(widgets.First(w => w.Name == "UI_TITLE_01").Center);
        Assert.False(widgets.First(w => w.Name == "UI_TITLE_01").ScaleOriginToViewport);
        var title01 = FrontendUiDef.TryParse(bin.FindEntry("UI_TITLE_01")!)!;
        Assert.Equal(1f, title01.ZoomX);
        Assert.Equal(3, title01.GraphicBankId);
        var titleTex = sprites.TryLoad(FrontendSpriteBank.TitleLeft)!;
        Assert.Equal(titleTex.Width, titleTex.FrameWidth);
        Assert.Equal(256, titleTex.FrameWidth);
        Assert.Equal(128, titleTex.FrameHeight);
        var textWidget = widgets.First(w => w.Name == "UI_PRESS_START_TEXT");
        Assert.Equal(0, textWidget.GraphicId);
        Assert.False(textWidget.ScaleSizeToViewport);
        Assert.False(textWidget.ScaleOriginToViewport);
        var textDest = dests["UI_PRESS_START_TEXT"];
        Assert.Equal(textDest.X0, textDest.X1);
        Assert.Equal(textDest.Y0, textDest.Y1);
        Assert.Equal(512f, textDest.OriginX);
        Assert.InRange(textDest.OriginY, 383.99f, 384.01f);
    }

    private static (float X0, float Y0, float X1, float Y1) Rect(FrontendDest dest) =>
        (dest.X0, dest.Y0, dest.X1, dest.Y1);

    [Fact]
    public void Leftover204_is_0041AC20_graphic_index_not_persist_size()
    {
        Assert.Equal(0x0041AC20u, FrontendLayout.LeftoverFn);
        Assert.Equal(376, FrontendLayout.GraphicIndexOffset);
        Assert.Equal(84, FrontendLayout.BankFrameWVtbl);
        Assert.Equal(88, FrontendLayout.BankFrameHVtbl);
        Assert.Equal((0f, 0f), FrontendLayout.LeftoverFromGraphic(0, 256f, 128f));
        Assert.Equal((256f, 128f), FrontendLayout.LeftoverFromGraphic(3, 256f, 128f));
        var persistWins = FrontendLayout.ComputeSubmitDest(
            10, 8, 256f, 128f, 0f, 0f, 1f, 1f, center: false);
        Assert.Equal((0f, 0f, 10f, 8f), persistWins);
        var leftoverWhenPersistZero = FrontendLayout.ComputeSubmitDest(
            0, 0, 32f, 16f, 10f, 20f, 2f, 2f, center: false);
        Assert.Equal((10f, 20f, 74f, 52f), leftoverWhenPersistZero);
        var type6 = FrontendLayout.ComputeSubmitDest(
            0, 0, 0f, 0f, 512f, 384f, 1.6f, 1.6f, center: false);
        Assert.Equal((512f, 384f, 512f, 384f), type6);
    }

    [Fact]
    public void Type6_leftover204_is_widget_plus204_not_dest_width()
    {
        Assert.Equal(204, FrontendLayout.DestWOffset);
        Assert.Equal(264, FrontendLayout.DestScaleXOffset);
        // First-seen type-6 GraphicIndex=0: +204 stays 0.
        // Dest scale +264 is used; leftover width is not dest W.
        const float leftover204 = 0f;
        var destScale = FirstSeen.Width / FrontendLayout.AuthoredWidth;
        Assert.Equal(512f, FrontendTextDraw.Type6AlignedX(
            512f, leftover204, destScale, FrontendTextDraw.AlignLeft));
        Assert.Equal(512f, FrontendTextDraw.Type6AlignedX(
            512f, leftover204, destScale, FrontendTextDraw.AlignCentre));
        Assert.Equal(512f, FrontendTextDraw.Type6AlignedX(
            512f, leftover204, destScale, FrontendTextDraw.AlignRight));
        Assert.NotEqual(512f, FrontendTextDraw.Type6AlignedX(
            512f, 16f, destScale, FrontendTextDraw.AlignCentre));
        var pen = FrontendTextDraw.Type6Pen(
            512f, 384f, leftover204, destScale, FrontendTextDraw.AlignLeft);
        Assert.Equal(512f + FrontendTextDraw.Type6OriginPad, pen.X);
        Assert.Equal(384f + FrontendTextDraw.Type6OriginPad, pen.Y);
    }

    [Fact]
    public void Y_increases_down_with_no_flip()
    {
        var dest = FrontendLayout.ComputeSubmitDest(
            0, 0, 8f, 8f, 0f, 30f, 1f, 1f, center: false);
        Assert.Equal(30f, dest.Y0);
        Assert.Equal(38f, dest.Y1);
        Assert.True(dest.Y1 > dest.Y0);
    }
}
