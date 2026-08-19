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

    [Fact]
    public void New_Profile_persist_child_order_and_layers()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
        var bin = GameBin.Load(install.FindCompiledDef("frontend.bin")!, names);
        var scratch = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Temp", "grok-goal-d6d68af8e2ab", "implementer", "persist-new-profile.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(scratch)!);
        var sb = new System.Text.StringBuilder();
        foreach (var name in new[]
        {
            "UI_FRONTEND_NEW_PROFILE_SCREEN",
            "UI_TABLE_TITLE_WHOLE",
            "UI_TEXT_NEW_PROFILE_MENU_TITLE",
            "UI_BUTTON_OPTIONS_LEFT",
            "UI_OPTIONS_TEXT_SLOT_L",
            "UI_OPTIONS_TEXT_SLOT_R",
            "UI_OPTIONS_TEXT_SLOT_M",
            "UI_BUTTON_OPTIONS_RIGHT",
            "UI_OPTIONS_CONTROL_METHOD_TEXT_SLIDER",
            "UI_OPTIONS_TEXT_CONTROL_ARROWS",
            "UI_NEW_PROFILE_EDIT_BOX",
            "UI_SCOREBOARD_EDITBOX_TEXT_FE",
            "UI_OPTIONS_TEXT_SLIDER_BUTTON_TABLES",
        })
        {
            var def = FrontendUiDef.TryParse(bin.FindEntry(name)!)!;
            sb.Append(name).Append(" type=").Append(def.Type);
            sb.Append(" layer=").Append(def.Layer);
            sb.Append(" xy=").Append(def.PositionX).Append(',').Append(def.PositionY);
            sb.Append(" wh=").Append(def.Width).Append(',').Append(def.Height);
            sb.Append(" plus96=").Append(def.Plus96);
            sb.Append(" sprites=").Append(def.Sprites);
            sb.Append(" graphic=").Append(def.GraphicBankId);
            sb.Append(" text=").Append(def.TextTag ?? "-");
            sb.Append(" kids=");
            foreach (var i in def.ChildIndices)
            {
                var child = (uint)i < (uint)bin.Entries.Count
                    ? bin.Entries[i].InstanceName ?? bin.Entries[i].SourceName
                    : "?";
                sb.Append('[').Append(i).Append(':').Append(child).Append(']');
            }

            sb.Append(" spriteDefs=");
            foreach (var i in def.SpriteDefIndices)
            {
                var child = (uint)i < (uint)bin.Entries.Count
                    ? bin.Entries[i].InstanceName ?? bin.Entries[i].SourceName
                    : "?";
                sb.Append('[').Append(i).Append(':').Append(child).Append(']');
            }

            sb.AppendLine();
        }

        File.WriteAllText(scratch, sb.ToString());
        var screen = FrontendUiDef.TryParse(bin.FindEntry("UI_FRONTEND_NEW_PROFILE_SCREEN")!)!;
        Assert.Equal(5, screen.ChildIndices.Count);
        Assert.Equal("UI_TEXT_NEW_PROFILE_MENU_TITLE", bin.Entries[screen.ChildIndices[0]].InstanceName);
        Assert.Equal("UI_TABLE_TITLE_WHOLE", bin.Entries[screen.ChildIndices[2]].InstanceName);
        var left = FrontendUiDef.TryParse(bin.FindEntry("UI_BUTTON_OPTIONS_LEFT")!)!;
        Assert.Equal(3, left.SpriteDefIndices.Count);
        Assert.Equal("UI_OPTIONS_TEXT_SLOT_L", bin.Entries[left.SpriteDefIndices[0]].InstanceName);
        Assert.Equal("UI_OPTIONS_TEXT_SLOT_M", bin.Entries[left.SpriteDefIndices[2]].InstanceName);
        var leftEntry = bin.FindEntry("UI_BUTTON_OPTIONS_LEFT")!;
        var leftRaw = leftEntry.Raw;
        var spriteOff = -1;
        for (var i = 0; i + 8 <= leftRaw.Length; i++)
        {
            if (BitConverter.ToUInt32(leftRaw, i) != FrontendUiDef.SpritesCrc)
                continue;
            spriteOff = i;
            break;
        }

        Assert.True(spriteOff >= 0);
        var n = BitConverter.ToInt32(leftRaw, spriteOff + 4);
        sb.Append("LEFT sprite pairs n=").Append(n);
        var p = spriteOff + 8;
        for (var i = 0; i < n && p + 8 <= leftRaw.Length; i++, p += 8)
        {
            var key = BitConverter.ToInt32(leftRaw, p);
            var def = BitConverter.ToInt32(leftRaw, p + 4);
            var child = (uint)def < (uint)bin.Entries.Count
                ? bin.Entries[def].InstanceName
                : "?";
            sb.Append(" (").Append(key).Append(',').Append(child).Append(')');
        }

        sb.AppendLine();
        File.WriteAllText(scratch, sb.ToString());
        var editText = FrontendUiDef.TryParse(bin.FindEntry("UI_SCOREBOARD_EDITBOX_TEXT_FE")!)!;
        Assert.True(string.IsNullOrEmpty(editText.TextTag));
        Assert.Equal(new[] { 0, 1, 4 }, left.SpriteKeys);
        var first = FrontendLayout.PlaceTableCell(
            0, 3, 64f, 240f, 288f, 32f, 64f, 32f, plus96: 1, firstCapW: 64f, lastCapW: 64f);
        var right = FrontendLayout.PlaceTableCell(
            1, 3, 64f, 240f, 288f, 32f, 64f, 32f, plus96: 1, firstCapW: 64f, lastCapW: 64f);
        var mid = FrontendLayout.PlaceTableCell(
            2, 3, 64f, 240f, 288f, 32f, 8f, 32f, plus96: 1, firstCapW: 64f, lastCapW: 64f);
        Assert.Equal(64f, first.X0);
        Assert.Equal(128f, first.X1);
        Assert.Equal(288f, right.X0);
        Assert.Equal(352f, right.X1);
        Assert.Equal(first.X1, mid.X0);
        Assert.Equal(mid.X1, right.X0);
    }

    [Fact]
    public void New_Profile_type12_rows_use_persist_plus326_not_equal_Y()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
        var bin = GameBin.Load(install.FindCompiledDef("frontend.bin")!, names);
        var list = FrontendUiDef.TryParse(bin.FindEntry("UI_NEW_PROFILE_MENU")!)!;
        Assert.Equal(12, list.Type);
        Assert.Equal(30f, list.Plus326);
        Assert.Equal(4, list.ChildIndices.Count);
        Assert.Equal(0f, list.Plus322);
        Assert.Equal(0xA04E63BEu, FrontendUiDef.Plus322Crc);
        Assert.Equal((0f, 0f), FrontendLayout.ListChildAuthoredPos(0, 0f, 0f, 0f, 30f, 0f, 0f));
        Assert.Equal((0f, 30f), FrontendLayout.ListChildAuthoredPos(1, -100f, 0f, 0f, 30f, 0f, 0f));
        Assert.Equal((0f, 60f), FrontendLayout.ListChildAuthoredPos(2, -100f, 0f, 0f, 30f, 0f, 0f));
        Assert.Equal((0f, 90f), FrontendLayout.ListChildAuthoredPos(3, -100f, 70f, 0f, 30f, 0f, 0f));
        Assert.Equal(0f, FrontendLayout.ListChildAuthoredY(0, 0f, list.Plus326));
        Assert.Equal(30f, FrontendLayout.ListChildAuthoredY(1, 0f, list.Plus326));
        Assert.Equal(60f, FrontendLayout.ListChildAuthoredY(2, 0f, list.Plus326));
        Assert.Equal(90f, FrontendLayout.ListChildAuthoredY(3, 70f, list.Plus326));

        var life = ReachNewProfile();
        var rows = life.FrontendWidgets
            .Where(w => w.ParentName == "UI_NEW_PROFILE_MENU")
            .ToList();
        Assert.Equal(4, rows.Count);
        var ys = rows.Select(w => w.DestY0).Distinct().ToList();
        Assert.True(ys.Count >= 4, string.Join(",", rows.Select(w => $"{w.Name}:{w.DestY0}")));
        var method = life.FrontendWidgets.First(w =>
            w.Name == "UI_OPTIONS_TEXT_SLIDER_WHOLE_CONTROL_METHOD");
        Assert.True(method.DestX0 >= 0f, $"methodX={method.DestX0}");
        var label = life.FrontendWidgets.First(w =>
            w.Name == "UI_OPTIONS_SLIDER_TEXT_CONTROL_METHOD");
        Assert.True(label.DestX0 > 0f, $"labelX={label.DestX0}");
        var slider = life.FrontendWidgets.First(w =>
            w.Name == "UI_OPTIONS_CONTROL_METHOD_TEXT_SLIDER");
        Assert.Equal(slider.DestX0, slider.DestX1);
        Assert.Equal(slider.DestY0, slider.DestY1);
        var edit = life.FrontendWidgets.First(w => w.Name == "UI_NEW_PROFILE_EDIT_BOX");
        Assert.Equal(edit.DestX0, edit.DestX1);
        Assert.Equal(edit.DestY0, edit.DestY1);
        Assert.Equal(0x0054C3A0u, FrontendWidgetType.ListCtor);

        var left = FrontendUiDef.TryParse(bin.FindEntry("UI_BUTTON_OPTIONS_LEFT")!)!;
        Assert.Equal(2, left.Type);
        Assert.Equal(3, left.Sprites);
        Assert.True(
            left.SpriteDefIndices.Count == 3,
            "sprites=" + string.Join(",", left.SpriteDefIndices) +
            " n=" + left.Sprites + " partial=" + left.Partial);
        Assert.Equal((180f, 0f), FrontendLayout.Type2Leftover(left.Width, left.Height));
        var tableIndex = -1;
        for (var i = 0; i < life.FrontendWidgets.Count; i++)
        {
            if (life.FrontendWidgets[i].Name == "UI_BUTTON_OPTIONS_LEFT" &&
                life.FrontendWidgets[i].DestY0 == 240f)
            {
                tableIndex = i;
                break;
            }
        }

        Assert.True(tableIndex >= 0);
        var table = life.FrontendWidgets[tableIndex];
        var kids = FrontendWidgetFactory.ChildrenOf(life.FrontendWidgets, tableIndex);
        Assert.True(kids.Count >= 3, "left cells=" + kids.Count);
        var capL = life.FrontendWidgets[kids[0]];
        var capR = life.FrontendWidgets[kids[1]];
        var stretch = life.FrontendWidgets[kids[2]];
        Assert.Equal(table.DestX0, capL.DestX0);
        Assert.Equal(capL.DestX1, stretch.DestX0);
        Assert.Equal(stretch.DestX1, capR.DestX0);
        Assert.Equal(table.DestX1, capR.DestX1);
    }

    [Fact]
    public void New_Profile_apply_cancel_hit_rects_are_disjoint()
    {
        var life = ReachNewProfile();
        var apply = IndexOf(life, "UI_ACCEPT_NEW_PROFILE");
        var cancel = IndexOf(life, "UI_CANCEL");
        Assert.True(
            FrontendHitTest.TryDestPoint(life.FrontendWidgets, apply, out var ax, out var ay));
        Assert.True(
            FrontendHitTest.TryDestPoint(life.FrontendWidgets, cancel, out var cx, out var cy));
        var applyDest = life.FrontendWidgets[apply];
        var cancelDest = life.FrontendWidgets[cancel];
        Assert.Equal(applyDest.DestX0, applyDest.HitX0);
        Assert.Equal(applyDest.DestY0, applyDest.HitY0);
        Assert.Equal(applyDest.DestX1, applyDest.HitX1);
        Assert.Equal(applyDest.DestY1, applyDest.HitY1);
        Assert.Equal(cancelDest.DestX0, cancelDest.HitX0);
        Assert.Equal(cancelDest.DestY0, cancelDest.HitY0);
        Assert.Equal(cancelDest.DestX1, cancelDest.HitX1);
        Assert.Equal(cancelDest.DestY1, cancelDest.HitY1);
        foreach (var widget in life.FrontendWidgets)
        {
            if (widget.DestX1 > widget.DestX0 && widget.DestY1 > widget.DestY0)
            {
                Assert.Equal(widget.DestX0, widget.HitX0);
                Assert.Equal(widget.DestY0, widget.HitY0);
                Assert.Equal(widget.DestX1, widget.HitX1);
                Assert.Equal(widget.DestY1, widget.HitY1);
                continue;
            }

            if (widget.Type is FrontendWidgetType.TextSlider or FrontendWidgetType.EditBox)
            {
                Assert.True(widget.HitX1 > widget.HitX0 && widget.HitY1 > widget.HitY0,
                    widget.Name + " hit empty");
            }
        }

        Assert.NotEqual(ax, cx);
        var applyArea = FirstAreaDest(life, apply);
        var cancelArea = FirstAreaDest(life, cancel);
        Assert.False(
            FrontendFrameDump.Intersects(
                applyArea.X0, applyArea.Y0, applyArea.X1, applyArea.Y1,
                cancelArea.X0, cancelArea.Y0, cancelArea.X1, cancelArea.Y1));
        Assert.Equal(0x0055B8F0u, FrontendHitTest.HitTestFn);
        Assert.Null(FrontendHitTest.HitIndex(life.FrontendWidgets, 12f, 12f));
        Assert.Equal(
            apply,
            FrontendHitTest.HitIndex(life.FrontendWidgets, ax, ay));
        Assert.Equal(
            cancel,
            FrontendHitTest.HitIndex(life.FrontendWidgets, cx, cy));
        var slider = IndexOf(life, "UI_OPTIONS_CONTROL_METHOD_TEXT_SLIDER");
        var before = life.FrontendWidgets[slider].ActiveChild;
        Assert.Null(FrontendHitTest.HitIndex(life.FrontendWidgets, 96f, 304f));
        Assert.Equal(
            slider,
            FrontendHitTest.HitIndex(life.FrontendWidgets, 700f, 300f));
        var edit = IndexOf(life, "UI_NEW_PROFILE_EDIT_BOX");
        var editHit = FrontendHitTest.HitRect(life.FrontendWidgets, edit);
        Assert.True(editHit.X1 > editHit.X0 && editHit.Y1 > editHit.Y0);
        Assert.Equal(
            edit,
            FrontendHitTest.HitIndex(
                life.FrontendWidgets,
                (editHit.X0 + editHit.X1) * 0.5f,
                (editHit.Y0 + editHit.Y1) * 0.5f));
        var knob = IndexOf(life, "UI_SLIDER_CAMERA_SENSITIVITY");
        Assert.Equal(
            knob,
            FrontendHitTest.HitIndex(
                life.FrontendWidgets,
                MidX(life.FrontendWidgets[knob]),
                MidY(life.FrontendWidgets[knob])));
        var title = life.FrontendWidgets.First(w => w.Name == "UI_TEXT_NEW_PROFILE_MENU_TITLE");
        Assert.False(string.IsNullOrEmpty(title.Text));
        var editGlyph = life.FrontendWidgets.First(w =>
            w.Name == "UI_SCOREBOARD_EDITBOX_TEXT_FE");
        Assert.False(string.IsNullOrEmpty(editGlyph.Text));
        Assert.Equal(before, life.FrontendWidgets[slider].ActiveChild);
    }

    private static float MidX(FrontendWidget widget) =>
        (widget.DestX0 + widget.DestX1) * 0.5f;

    private static float MidY(FrontendWidget widget) =>
        (widget.DestY0 + widget.DestY1) * 0.5f;

    private static (float X0, float Y0, float X1, float Y1) FirstAreaDest(
        EngineLifecycle life, int index)
    {
        var w = life.FrontendWidgets[index];
        if (w.DestX1 > w.DestX0 && w.DestY1 > w.DestY0)
            return (w.DestX0, w.DestY0, w.DestX1, w.DestY1);
        foreach (var kid in FrontendWidgetFactory.ChildrenOf(life.FrontendWidgets, index))
        {
            var child = life.FrontendWidgets[kid];
            if (child.DestX1 > child.DestX0 && child.DestY1 > child.DestY0)
                return (child.DestX0, child.DestY0, child.DestX1, child.DestY1);
        }

        throw new InvalidOperationException("no dest area");
    }

    private static EngineLifecycle ReachNewProfile()
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
        return life;
    }

    private static int IndexOf(EngineLifecycle life, string name)
    {
        for (var i = 0; i < life.FrontendWidgets.Count; i++)
        {
            if (life.FrontendWidgets[i].Name == name)
                return i;
        }

        throw new InvalidOperationException(name);
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
