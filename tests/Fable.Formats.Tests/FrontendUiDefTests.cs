using Fable.Core;
using Fable.Formats.Defs;
using Fable.Formats.Fonts;
using Fable.Game;
using Fable.Render.Parity.Dx9Vulkan;

namespace Fable.Formats.Tests;

public sealed class FrontendUiDefTests
{
    private static (GameInstall Install, NamesBin Names, GameBin Bin) LoadFrontend()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
        var bin = GameBin.Load(install.FindCompiledDef("frontend.bin")!, names);
        return (install, names, bin);
    }

    [Fact]
    public void Height_crc_is_fable_crc_of_Height()
    {
        Assert.Equal(FableCrc.Hash("Height"), FrontendUiDef.HeightCrc);
        Assert.Equal(0x4323419Au, FrontendUiDef.HeightCrc);
        Assert.NotEqual(0x4341A19Au, FrontendUiDef.HeightCrc);
        Assert.Equal(FableCrc.Hash("GraphicIndex"), FrontendUiDef.GraphicIndexCrc);
        Assert.Equal(FableCrc.Hash("Type"), FrontendUiDef.TypeCrc);
        Assert.Equal(FableCrc.Hash("Children"), FrontendUiDef.ChildrenCrc);
        Assert.Equal(FableCrc.Hash("Width"), FrontendUiDef.WidthCrc);
        Assert.Equal(FableCrc.Hash("PositionX"), FrontendUiDef.PositionXCrc);
        Assert.Equal(FableCrc.Hash("PositionY"), FrontendUiDef.PositionYCrc);
        Assert.Equal(FableCrc.Hash("Font"), FrontendUiDef.FontCrc);
        Assert.Equal(FableCrc.Hash("Sprites"), FrontendUiDef.SpritesCrc);
        Assert.Equal(FableCrc.Hash("States"), FrontendUiDef.StatesCrc);
        Assert.Equal(FableCrc.Hash("ColourR"), FrontendUiDef.ColourRCrc);
        Assert.Equal(FableCrc.Hash("ColourG"), FrontendUiDef.ColourGCrc);
        Assert.Equal(FableCrc.Hash("ColourB"), FrontendUiDef.ColourBCrc);
        Assert.Equal(FableCrc.Hash("ColourA"), FrontendUiDef.ColourACrc);
        Assert.Equal(FableCrc.Hash("ZoomX"), FrontendUiDef.ZoomXCrc);
        Assert.Equal(0xE78E700Eu, FrontendUiDef.ZoomXCrc);
        Assert.Equal(FableCrc.Hash("ZoomY"), FrontendUiDef.ZoomYCrc);
        Assert.Equal(0x90894098u, FrontendUiDef.ZoomYCrc);
        Assert.NotEqual(FableCrc.Hash("ScaleX"), FrontendUiDef.ZoomXCrc);
        Assert.Equal(FableCrc.Hash("TextValue"), FrontendUiDef.TextValueCrc);
        Assert.NotEqual(FableCrc.Hash("Graphic"), FrontendUiDef.GraphicIndexCrc);
        Assert.NotEqual(FableCrc.Hash("Texture"), FrontendUiDef.GraphicIndexCrc);
        Assert.Equal(FableCrc.Hash("PositionIsCenter"), FrontendUiDef.PositionIsCenterCrc);
        Assert.Equal(FableCrc.Hash("Independant"), FrontendUiDef.IndependantCrc);
        Assert.Equal(FableCrc.Hash("UseRelativeZoom"), FrontendUiDef.UseRelativeZoomCrc);
        Assert.Equal(FableCrc.Hash("UseRelativePosition"), FrontendUiDef.UseRelativePositionCrc);
        Assert.Equal(FableCrc.Hash("TextLineBreak"), FrontendUiDef.TextLineBreakCrc);
        Assert.Equal(FableCrc.Hash("ScaleText"), FrontendUiDef.ScaleTextCrc);
        Assert.Equal(FableCrc.Hash("MeshType"), FrontendUiDef.MeshTypeCrc);
        Assert.Equal(FableCrc.Hash("DrawFromViewport"), FrontendUiDef.DrawFromViewportCrc);
        Assert.Equal(FableCrc.Hash("LayerIndependant"), FrontendUiDef.LayerIndependantCrc);
        Assert.Equal(FableCrc.Hash("BastardChild"), FrontendUiDef.BastardChildCrc);
        Assert.Equal(FableCrc.Hash("Alignement"), FrontendUiDef.AlignementCrc);
        Assert.Equal(FableCrc.Hash("RandomSwap"), FrontendUiDef.RandomSwapCrc);
        Assert.Equal(0x00631C60u, FrontendUiDef.PersistFn);
        Assert.Equal(0x0041D21Bu, FrontendWidgetType.ConstructFn);
        Assert.Equal(0x0054E3D0u, FrontendWidgetType.MenuCtor);
        Assert.Equal(0x0054F5C0u, FrontendWidgetType.TextCtor);
        Assert.Equal(0x00431102u, FrontendUiDef.PersistDwordFn);
        Assert.Equal(0x00431061u, FrontendUiDef.PersistFloatFn);
    }

    [Fact]
    public void Every_frontend_UI_entry_matches_the_complete_native_CUIDef_schema()
    {
        var (_, _, bin) = LoadFrontend();
        var uiEntries = bin.Entries.Where(entry => entry.TypeName == "UI").ToList();
        Assert.NotEmpty(uiEntries);

        foreach (var entry in uiEntries)
        {
            var complete = FrontendUiSchema.TryConsume(entry, out var end, out var error);
            Assert.True(complete, $"{entry.InstanceName}: {error}");
            Assert.Equal(entry.Raw.Length, end);

            var parsed = FrontendUiDef.TryParse(entry);
            Assert.NotNull(parsed);
            Assert.True(parsed.SchemaComplete, $"{entry.InstanceName}: {parsed.SchemaError}");
            Assert.Null(parsed.SchemaError);
        }
    }

    [Fact]
    public void Complete_CUIDef_catalog_uses_original_names_and_matching_file_CRCs()
    {
        Assert.Equal(109, FrontendUiFieldCatalog.Fields.Count);
        Assert.Equal(14, FrontendUiFieldCatalog.StateFields.Count);
        Assert.Equal(109, FrontendUiFieldCatalog.Fields.Select(f => f.Name).Distinct().Count());
        Assert.Equal(109, FrontendUiFieldCatalog.Fields.Select(f => f.Crc).Distinct().Count());

        foreach (var field in FrontendUiFieldCatalog.Fields)
            Assert.Equal(field.Crc, FableCrc.Hash(field.Name));
        foreach (var field in FrontendUiFieldCatalog.StateFields)
            Assert.Equal(field.Crc, FableCrc.Hash(field.Name));

        Assert.Equal("ActionOnLeftClicked",
            FrontendUiFieldCatalog.Fields.Single(f => f.RetailOffset == 224).Name);
        Assert.Equal("ActionOnLeftUnclicked",
            FrontendUiFieldCatalog.Fields.Single(f => f.RetailOffset == 228).Name);
        Assert.Equal("DrawFromViewport",
            FrontendUiFieldCatalog.Fields.Single(f => f.RetailOffset == 392).Name);
        Assert.Equal("PCStyle",
            FrontendUiFieldCatalog.Fields.Single(f => f.RetailOffset == 545).Name);
    }

    [Fact]
    public void Press_Start_is_type_10_with_children_and_text()
    {
        var (_, _, bin) = LoadFrontend();
        var parsed = FrontendUiDef.TryParse(bin.FindEntry("UI_FRONTEND_PRESS_START_MENU")!);
        Assert.NotNull(parsed);
        Assert.Equal(10, parsed.Type);
        Assert.Equal(FrontendWidgetType.Menu, parsed.Type);
        Assert.Equal(6, parsed.ChildIndices.Count);
        var names = parsed.ChildIndices
            .Select(i => bin.Entries[i].InstanceName)
            .ToList();
        Assert.Equal(
        [
            "UI_BLENDING_BACKGROUNDS_FORREST",
            "UI_TITLE",
            "UI_PRESS_START_SWAP",
            "UI_FRONTEND_LIST_PRESS_START_MENU",
            "UI_LEGAL_TEXT",
            "UI_MOUSE_POINTER",
        ], names);
        var text = FrontendUiDef.TryParse(bin.FindEntry("UI_PRESS_START_TEXT")!);
        Assert.NotNull(text);
        Assert.Equal(6, text.Type);
        Assert.Equal("TEXT_GUI_MENU_PRESS_BUTTON", text.TextValue);
        Assert.Equal(320f, text.PositionX);
        Assert.Equal(240f, text.PositionY);
        Assert.Equal(1f, text.ZoomX);
        Assert.Equal(1f, text.ZoomY);
        Assert.False(text.PositionIsCenter);
        Assert.False(text.Independant);
        Assert.False(text.UseRelativePosition);
        Assert.False(text.UseRelativeZoom);
        var title = FrontendUiDef.TryParse(bin.FindEntry("UI_TITLE")!);
        Assert.NotNull(title);
        Assert.Equal(5, title.Type);
        Assert.Equal(70f, title.PositionX);
        Assert.Equal(30f, title.PositionY);
        var title01 = FrontendUiDef.TryParse(bin.FindEntry("UI_TITLE_01")!);
        Assert.NotNull(title01);
        Assert.Equal(0, title01.Type);
        var title02 = FrontendUiDef.TryParse(bin.FindEntry("UI_TITLE_02")!);
        Assert.NotNull(title02);
        Assert.Equal(256f, title02.PositionX);
        var newGame = FrontendUiDef.TryParse(bin.FindEntry("UI_TEXT_NEW_GAME")!);
        Assert.NotNull(newGame);
        Assert.Equal(6, newGame.Type);
        Assert.Equal("TEXT_GUI_MENU_NEW_GAME", newGame.TextValue);
    }

    [Fact]
    public void Press_Start_type18_lists_match_00547500()
    {
        var (_, _, bin) = LoadFrontend();
        var forest = FrontendUiDef.TryParse(bin.FindEntry("UI_SWAPPING_FORREST")!)!;
        var sunbeam = FrontendUiDef.TryParse(
            bin.FindEntry("UI_SWAPPING_FORREST_SUNBEAM")!)!;
        Assert.Equal(FrontendWidgetType.Swap, forest.Type);
        Assert.Equal([0, 1, 2, 3], forest.SwappingStates);
        Assert.Equal([0f, 0f, 0f, 0f], forest.SwappingTimes);
        Assert.Equal([0, 1, 2], sunbeam.SwappingStates);
        Assert.Equal([0f, 0f, 0f], sunbeam.SwappingTimes);
        var blending = FrontendUiDef.TryParse(bin.FindEntry("BLENDING_BG_FORREST_1")!);
        Assert.NotNull(blending);
        Assert.Equal([8f, 8f, 8f, 8f], blending.StyleDurations);
    }

    [Fact]
    public void GraphicIndex_is_read_from_persist_not_a_name_map()
    {
        var (_, _, bin) = LoadFrontend();
        var title01 = FrontendUiDef.TryParse(bin.FindEntry("UI_TITLE_01")!)!;
        var title02 = FrontendUiDef.TryParse(bin.FindEntry("UI_TITLE_02")!)!;
        var forest = FrontendUiDef.TryParse(bin.FindEntry("UI_FRONTEND_BG_FORREST_1_1")!)!;
        var mouse = FrontendUiDef.TryParse(bin.FindEntry("UI_MOUSE_POINTER")!)!;
        var text = FrontendUiDef.TryParse(bin.FindEntry("UI_PRESS_START_TEXT")!)!;
        var root = FrontendUiDef.TryParse(bin.FindEntry("UI_FRONTEND_PRESS_START_MENU")!)!;
        Assert.Equal(3, title01.GraphicBankId);
        Assert.Equal(4, title02.GraphicBankId);
        Assert.Equal(206, forest.GraphicBankId);
        Assert.Equal(362, mouse.GraphicBankId);
        Assert.Equal(0, text.GraphicBankId);
        Assert.Equal(0, root.GraphicBankId);
        var raw = bin.FindEntry("UI_TITLE_01")!.Raw;
        var crcOff = IndexOfU32(raw, FrontendUiDef.GraphicIndexCrc);
        Assert.True(crcOff >= 0);
        Assert.Equal(3, BitConverter.ToInt32(raw, crcOff + 4));
        Assert.Equal(3, title01.GraphicBankId);
        Assert.Equal(0xC50CA371u, FrontendUiDef.UseRelativeZoomCrc);
        Assert.Equal(0xB466D948u, FrontendUiDef.UseRelativePositionCrc);
        Assert.Equal(0x64D3430Eu, FrontendUiDef.PositionIsCenterCrc);
        Assert.Equal(0x38BBD87Fu, FrontendUiDef.IndependantCrc);
        var titleLayerOff = IndexOfU32(raw, FrontendUiDef.LayerCrc);
        Assert.True(titleLayerOff > crcOff);
    }

    [Fact]
    public void Press_Start_remap_bits_come_from_def_520_521()
    {
        var (_, _, bin) = LoadFrontend();
        var root = FrontendUiDef.TryParse(bin.FindEntry("UI_FRONTEND_PRESS_START_MENU")!)!;
        var title = FrontendUiDef.TryParse(bin.FindEntry("UI_TITLE")!)!;
        var title01 = FrontendUiDef.TryParse(bin.FindEntry("UI_TITLE_01")!)!;
        var title02 = FrontendUiDef.TryParse(bin.FindEntry("UI_TITLE_02")!)!;
        var forest = FrontendUiDef.TryParse(bin.FindEntry("UI_BLENDING_BACKGROUNDS_FORREST")!)!;
        var forestTile = FrontendUiDef.TryParse(bin.FindEntry("UI_FRONTEND_BG_FORREST_1_1")!)!;
        var text = FrontendUiDef.TryParse(bin.FindEntry("UI_PRESS_START_TEXT")!)!;
        var mouse = FrontendUiDef.TryParse(bin.FindEntry("UI_MOUSE_POINTER")!)!;
        Assert.True(root.UseRelativeZoom);
        Assert.False(root.UseRelativePosition);
        foreach (var child in new[] { title, title01, title02, forest, forestTile, text, mouse })
        {
            Assert.False(child.UseRelativeZoom);
            Assert.False(child.UseRelativePosition);
        }

        Assert.False(title.PositionIsCenter);
        Assert.True(mouse.Independant);
        var raw = bin.FindEntry("UI_FRONTEND_PRESS_START_MENU")!.Raw;
        Assert.Equal(1, FrontendUiDef.ReadPersistU8(raw, FrontendUiDef.UseRelativeZoomCrc));
        Assert.Equal(0, FrontendUiDef.ReadPersistU8(raw, FrontendUiDef.UseRelativePositionCrc));
    }

    [Fact]
    public void Type_switch_table_comes_from_0041D7F8()
    {
        Assert.Equal(44, FrontendWidgetType.Table.Length);
        Assert.Equal(43, FrontendWidgetType.MaxType);
        Assert.Equal(0x0041D7F8u, FrontendWidgetType.JumpTableVa);
        Assert.Equal(0x0041B800u, FrontendWidgetType.Info(0).Ctor);
        Assert.Equal(0x0122F5D4u, FrontendWidgetType.Info(0).Vtbl);
        Assert.Equal(0x184, FrontendWidgetType.Info(0).Size);
        Assert.Equal("Button", FrontendWidgetType.Info(0).Role);
        Assert.Equal(0x0052CC50u, FrontendWidgetType.Info(5).Ctor);
        Assert.Equal(0x01245DE4u, FrontendWidgetType.Info(5).Vtbl);
        Assert.Equal(0x0054F5C0u, FrontendWidgetType.Info(6).Ctor);
        Assert.Equal(0x01249CCCu, FrontendWidgetType.Info(6).Vtbl);
        Assert.Equal(0x0054E3D0u, FrontendWidgetType.Info(10).Ctor);
        Assert.Equal(0x012497E4u, FrontendWidgetType.Info(10).Vtbl);
        Assert.Equal(0x16C, FrontendWidgetType.Info(10).Size);
        Assert.Equal(0u, FrontendWidgetType.Info(29).Ctor);
        Assert.False(FrontendWidgetType.TryConstruct(29));
        Assert.Equal(0x0055C650u, FrontendWidgetType.Info(32).Ctor);
        Assert.Equal(0x0124C22Cu, FrontendWidgetType.Info(32).Vtbl);
        Assert.Equal(0x005407B0u, FrontendWidgetType.Info(37).Ctor);
        Assert.Equal(0x00558B90u, FrontendWidgetType.Info(38).Ctor);
        Assert.True(FrontendWidgetType.IsContainer(10));
        Assert.False(FrontendWidgetType.IsContainer(6));
    }

    [Fact]
    public void Factory_builds_press_start_then_main_menu_from_the_same_walk()
    {
        var (install, _, bin) = LoadFrontend();
        using var sprites = new FrontendSpriteBank(install);
        var press = FrontendWidgetFactory.Build(bin, "UI_FRONTEND_PRESS_START_MENU", sprites);
        Assert.True(press.Count >= 7);
        Assert.Equal(10, press[0].Type);
        Assert.Contains(press, w => w.Name == "UI_PRESS_START_TEXT" && w.Type == 6);
        Assert.Contains(press, w => w.Name == "UI_TITLE_01");
        Assert.Contains(press, w => w.TextureName == FrontendSpriteBank.TitleLeft ||
                                    w.GraphicId == 3);
        var menu = FrontendWidgetFactory.Build(
            bin, "UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE", sprites);
        Assert.True(menu.Count >= 3);
        Assert.Equal(10, menu[0].Type);
        Assert.Contains(menu, w => w.Name == "UI_TITLE");
        var profile = FrontendWidgetFactory.Build(
            bin, "UI_FRONTEND_NEW_PROFILE_SCREEN", sprites);
        Assert.True(profile.Count >= 4);
        Assert.Equal(10, profile[0].Type);
        Assert.Contains(profile, w => w.Name == "UI_TEXT_NEW_PROFILE_MENU_TITLE" && w.Type == 6);
        Assert.Contains(profile, w => w.TextValue == "TEXT_GUI_MENU_NEW_PROFILE");
        var forest1 = Assert.Single(press, w => w.Name == "BLENDING_BG_FORREST_1");
        var forest2 = Assert.Single(press, w => w.Name == "BLENDING_BG_FORREST_2");
        var sunbeam1 = Assert.Single(press, w => w.Name == "BLENDING_BG_FORREST_SUNBEAM_1");
        var sunbeam2 = Assert.Single(press, w => w.Name == "BLENDING_BG_FORREST_SUNBEAM_2");
        var swap = Assert.Single(press, w => w.Name == "UI_SWAPPING_FORREST");
        Assert.Equal(0, swap.ActiveChild);
        Assert.True(forest1.Visible);
        Assert.True(forest1.Enabled);
        Assert.Equal(0, forest1.DrawFromViewport);
        Assert.True(forest2.Visible);
        Assert.Equal(0, forest2.DrawFromViewport);
        Assert.True(sunbeam1.Visible);
        Assert.True(sunbeam2.Visible);
        Assert.Contains(press, w => w.Name == "UI_TITLE_01" && w.Visible);
        Assert.Contains(press, w => w.Name == "UI_TITLE_02" && w.Visible);
        Assert.Contains(press, w => w.Name == "UI_PRESS_START_TEXT" && w.Visible);
        Assert.Contains(press, w => w.Name == "UI_FRONTEND_LIST_PRESS_START_MENU" && w.Visible);
        Assert.Contains(press, w => w.Name == "UI_LEGAL_TEXT" && w.Visible);
        Assert.Contains(press, w => w.Name == "UI_MOUSE_POINTER" && w.Visible);
        Assert.Contains(press, w =>
            w.Name == "UI_FRONTEND_BG_FORREST_1_1" && w.Visible);
        Assert.Contains(press, w =>
            w.Name == "UI_FRONTEND_BG_FORREST_2_1" && w.Visible);
        Assert.True(FrontendWidgetType.DrawsChildList(10));
        Assert.True(FrontendWidgetType.DrawsChildList(16));
        Assert.True(FrontendWidgetType.DrawsChildList(18));
        Assert.True(FrontendWidgetType.ContainerDrawWalksEveryChild);
        Assert.False(FrontendWidgetType.ExclusiveWalkSelectsChild);
        Assert.True(FrontendWidgetType.Leftover46ExclusiveWalkIsStale);
        Assert.False(FrontendWidgetType.SkipVtblsAreMethodCalls);
        Assert.False(FrontendWidgetType.SelectStateArg6SkipsDraw);
        Assert.Equal(0x01248A8Cu, FrontendWidgetType.TextSliderVtbl);
        Assert.Equal(0x012485ACu, FrontendWidgetType.SwapVtbl);
        Assert.Equal(0x0052F180u, FrontendWidgetType.BorrowedVisibleFn);
        Assert.Equal(0x0052F1D0u, FrontendWidgetType.ClipBitFn);
        Assert.Equal(0x0041C5A0u, FrontendWidgetType.ForwardSelectFn);
        Assert.Equal(348, FrontendWidgetType.TextSliderIndexOffset);
        Assert.Equal(0x0041AFA0u, FrontendWidgetType.LeafPresentFn);
        Assert.Equal(0x0054EF00u, FrontendWidgetType.Type6PresentFn);
        Assert.False(FrontendWidgetType.LeafDipSkipped(0xFFFFFFFFu));
        Assert.True(FrontendWidgetType.LeafDipSkipped(0x00FFFFFFu));
        Assert.Equal(0x0052C7E0u, FrontendWidgetType.StyleTickFn);
        Assert.Equal(0x10, FrontendWidgetType.StyleFlagsForceOpaque);
        Assert.Equal(0x20, FrontendWidgetType.StyleFlagsZeroDest);
        Assert.Equal(0x40, FrontendWidgetType.StyleFlagsUnitScale);
        Assert.Equal(0x0052CEB0u, FrontendWidgetType.StyleLookupFn);
        Assert.Equal(20, FrontendWidgetType.StyleLookupPayload);
        Assert.True(FrontendWidgetType.SelectsChild(18));
        Assert.True(FrontendWidgetType.SelectsChild(16));
        Assert.False(FrontendWidgetType.SelectsChild(5));
        var profileSlider = Assert.Single(profile, w => w.Name == "UI_OPTIONS_CONTROL_METHOD_TEXT_SLIDER");
        Assert.Equal(16, profileSlider.Type);
        Assert.Equal(0, profileSlider.ActiveChild);
        var arrows = Assert.Single(profile, w => w.Name == "UI_OPTIONS_TEXT_CONTROL_ARROWS");
        var wasd = Assert.Single(profile, w => w.Name == "UI_OPTIONS_TEXT_CONTROL_WASD");
        Assert.True(arrows.Visible);
        Assert.True(wasd.Visible);
        Assert.Equal(FrontendWidgetType.TextSliderFirstSeenSelect, arrows.StyleIndex);
        Assert.Equal(FrontendWidgetType.FirstSeenState, wasd.StyleIndex);
        Assert.NotEqual(arrows.Colour, wasd.Colour);
        Assert.False(FrontendWidgetType.LeafDipSkipped(arrows.Colour));
        Assert.True(FrontendWidgetType.LeafDipSkipped(wasd.Colour));
        var normal = Assert.Single(profile, w => w.Name == "UI_TEXT_NORMAL");
        var inverted = Assert.Single(profile, w => w.Name == "UI_TEXT_INVERTED");
        Assert.True(normal.Visible);
        Assert.True(inverted.Visible);
        Assert.Equal(FrontendWidgetType.TextSliderFirstSeenSelect, normal.StyleIndex);
        Assert.Equal(FrontendWidgetType.FirstSeenState, inverted.StyleIndex);
        Assert.False(FrontendWidgetType.LeafDipSkipped(normal.Colour));
        Assert.True(FrontendWidgetType.LeafDipSkipped(inverted.Colour));
        Assert.Equal(0x0041C5C0u, FrontendWidgetType.StyleExistsFn);
        Assert.Equal(0x0052E930u, FrontendWidgetType.InheritPackedColourFn);
        var acceptOn = Assert.Single(profile, w => w.Name == "UI_SPRITE_ACCEPT_ON");
        var acceptOff = Assert.Single(profile, w => w.Name == "UI_SPRITE_ACCEPT_OFF");
        Assert.True(FrontendWidgetType.LeafDipSkipped(acceptOn.Colour));
        Assert.False(FrontendWidgetType.LeafDipSkipped(acceptOff.Colour));
        Assert.Equal(0x8A69D67Eu, FrontendUiDef.DrawFromViewportCrc);
        Assert.Equal(1, arrows.Alignement);
        Assert.Equal(1, wasd.Alignement);
        Assert.Equal(FrontendTextDraw.Flag302CentreBit, arrows.Flag302 & FrontendTextDraw.Flag302CentreBit);
    }

    [Fact]
    public void Persist_00631C60_plus189_plus190_are_u8_and_font_is_names_offset()
    {
        var (install, names, bin) = LoadFrontend();
        Assert.Equal(0x00631C60u, FrontendUiDef.PersistFn);
        Assert.Equal(0xBDACBABAu, FrontendUiDef.TextLineBreakCrc);
        Assert.Equal(0xAC637D43u, FrontendUiDef.ScaleTextCrc);
        var text = FrontendUiDef.TryParse(bin.FindEntry("UI_PRESS_START_TEXT")!)!;
        Assert.Equal(26051, text.Font);
        var face = names.Get((uint)text.Font);
        Assert.False(string.IsNullOrEmpty(face));
        Assert.StartsWith("ENG_ARIAL_", face, StringComparison.Ordinal);
        Assert.Equal(face, FrontendWidgetFactory.ResolveFontFace(text.Font, names));
        Assert.Equal(FontFile.PersistType6Face, face);
        var mouse = FrontendUiDef.TryParse(bin.FindEntry("UI_MOUSE_POINTER")!)!;
        Assert.True(mouse.Independant);
        Assert.Equal(1, FrontendUiDef.ReadPersistU8(
            bin.FindEntry("UI_MOUSE_POINTER")!.Raw, FrontendUiDef.IndependantCrc));
        var widgets = FrontendWidgetFactory.Build(
            bin, "UI_FRONTEND_PRESS_START_MENU", names: names);
        var textWidget = Assert.Single(
            widgets, w => w.Name == "UI_PRESS_START_TEXT");
        Assert.Equal(26051, textWidget.Font);
        Assert.Equal(face, textWidget.FontFace);
        Assert.Equal(0x53C644E4u, FrontendUiDef.ActionOnLeftUnclickedCrc);
        Assert.Equal(228, FrontendUiDef.ActionOnLeftUnclickedRetailOffset);
        Assert.Equal(0x230364D6u, FrontendUiDef.ActionOnLeftClickedCrc);
        Assert.Equal(224, FrontendUiDef.ActionOnLeftClickedRetailOffset);
        Assert.Equal(0x00632500u, FrontendUiDef.PersistTailDwordFn);
        Assert.NotEqual(FrontendUiDef.ActionOnLeftClickedCrc, FrontendUiDef.ActionOnLeftUnclickedCrc);
        Assert.Equal(FableCrc.Hash("ActionOnLeftUnclicked"), FrontendUiDef.ActionOnLeftUnclickedCrc);
        Assert.Equal(FableCrc.Hash("ActionOnLeftClicked"), FrontendUiDef.ActionOnLeftClickedCrc);
        var accept = FrontendUiDef.TryParse(bin.FindEntry("UI_ACCEPT_NEW_PROFILE")!)!;
        Assert.Equal(38, accept.Type);
        Assert.Equal(0x126, accept.ActionOnLeftUnclicked);
        Assert.Equal(0, accept.ActionOnLeftClicked);
        Assert.NotEqual(accept.ActionOnLeftUnclicked, accept.ActionOnLeftClicked);
        var newGame = FrontendUiDef.TryParse(bin.FindEntry("UI_FRONTEND_BUTTON_NEW_GAME")!)!;
        Assert.Equal(11, newGame.Type);
        Assert.Equal(15, newGame.ActionOnLeftUnclicked);
        Assert.Equal(0, newGame.ActionOnLeftClicked);
        Assert.NotEqual(newGame.ActionOnLeftUnclicked, newGame.ActionOnLeftClicked);
        var invisible = FrontendUiDef.TryParse(bin.FindEntry("UI_FRONTEND_BUTTON_INVISIBLE")!)!;
        Assert.Equal(11, invisible.Type);
        Assert.Equal(0xE5, invisible.ActionOnLeftUnclicked);
        Assert.Equal(0, invisible.ActionOnLeftClicked);
        Assert.Equal(
            0x126,
            FrontendUiDef.ReadPersistI32(
                bin.FindEntry("UI_ACCEPT_NEW_PROFILE")!.Raw,
                FrontendUiDef.ActionOnLeftUnclickedCrc));
        Assert.True(HasAdjacentPersistI32(
            bin.FindEntry("UI_ACCEPT_NEW_PROFILE")!.Raw,
            FrontendUiDef.ActionOnLeftClickedCrc,
            FrontendUiDef.ActionOnLeftUnclickedCrc));
        Assert.True(HasAdjacentPersistI32(
            bin.FindEntry("UI_FRONTEND_BUTTON_NEW_GAME")!.Raw,
            FrontendUiDef.ActionOnLeftClickedCrc,
            FrontendUiDef.ActionOnLeftUnclickedCrc));
        Assert.Equal(228, FrontendInputMap.ActionOnLeftUnclickedDefOffset);
        var invisibleWidget = widgets.Single(w =>
            w.Name == "UI_FRONTEND_BUTTON_INVISIBLE");
        Assert.Equal(0xE5, invisibleWidget.ActionOnLeftUnclicked);
        Assert.Equal(0, invisibleWidget.ActionOnLeftClicked);
        var main = FrontendWidgetFactory.Build(
            bin,
            "UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE",
            names: names);
        var newGameWidget = main.Single(w =>
            w.Name == "UI_FRONTEND_BUTTON_NEW_GAME");
        Assert.Equal(15, newGameWidget.ActionOnLeftUnclicked);
        Assert.Equal(0, newGameWidget.ActionOnLeftClicked);
        var cancel = FrontendUiDef.TryParse(bin.FindEntry("UI_CANCEL")!)!;
        Assert.Equal(38, cancel.Type);
        Assert.Equal(FrontendMessages.CancelNewProfile, cancel.ActionOnLeftUnclicked);
        Assert.Equal(
            FrontendMessages.CancelNewProfile,
            FrontendUiDef.ReadPersistI32(
                bin.FindEntry("UI_CANCEL")!.Raw,
                FrontendUiDef.ActionOnLeftUnclickedCrc));
        _ = install;
    }

    private static bool HasAdjacentPersistI32(byte[] raw, uint firstCrc, uint secondCrc)
    {
        for (var i = 0; i + 16 <= raw.Length; i++)
        {
            if (BitConverter.ToUInt32(raw, i) == firstCrc &&
                BitConverter.ToUInt32(raw, i + 8) == secondCrc)
                return true;
        }

        return false;
    }

    [Fact]
    public void Dx9_frontend_quad_is_xyzrhw_stride_32_and_src_alpha()
    {
        Assert.Equal(32, Dx9VulkanFrontend.VertexStride);
        Assert.Equal(0x144u, Dx9VulkanFrontend.FvfXyzRhwDiffuseTex1);
        Assert.Equal(4, Dx9VulkanFrontend.D3dptTriangleList);
        Assert.Equal(0x22u, Dx9VulkanFrontend.RecordType);
        var rec = new Fable.Render.FrontendDx9DrawRecord(
            70, 30, 326, 158, 0, 0, 1, 1, 0xFFFFFFFFu, 0, 2);
        var batch = Dx9VulkanFrontend.BuildBatch([rec], []);
        Assert.False(batch.IsEmpty);
        Assert.Equal(4, batch.Vertices.Length);
        Assert.Equal(6, batch.Indices.Length);
        Assert.True(batch.Draws[0].BlendEnable);
        Assert.Equal(
            Fable.Formats.Scene.D3dDeviceState.FirstSeenPalskinSrcBlend,
            batch.Draws[0].D3dSrcBlend);
    }

    private static int IndexOfU32(byte[] raw, uint value)
    {
        for (var i = 0; i + 4 <= raw.Length; i++)
        {
            if (BitConverter.ToUInt32(raw, i) == value)
                return i;
        }

        return -1;
    }
}
