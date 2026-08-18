using Fable.Core;
using Fable.Formats.Defs;
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
        Assert.NotEqual(FableCrc.Hash("TextTag"), FrontendUiDef.TextTagCrc);
        Assert.NotEqual(FableCrc.Hash("Graphic"), FrontendUiDef.GraphicIndexCrc);
        Assert.NotEqual(FableCrc.Hash("Texture"), FrontendUiDef.GraphicIndexCrc);
        Assert.Equal(0x0041D21Bu, FrontendWidgetType.ConstructFn);
        Assert.Equal(0x0054E3D0u, FrontendWidgetType.MenuCtor);
        Assert.Equal(0x0054F5C0u, FrontendWidgetType.TextCtor);
        Assert.Equal(0x00431102u, FrontendUiDef.PersistDwordFn);
        Assert.Equal(0x00431061u, FrontendUiDef.PersistFloatFn);
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
        Assert.Equal("TEXT_GUI_MENU_PRESS_BUTTON", text.TextTag);
        Assert.Equal(320f, text.PositionX);
        Assert.Equal(240f, text.PositionY);
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
        Assert.Equal("TEXT_GUI_MENU_NEW_GAME", newGame.TextTag);
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
        Assert.True(title01.Partial);
        Assert.Equal(FrontendUiDef.UnreadNestedCrc, title01.UnreadCrcs[0]);
        var raw = bin.FindEntry("UI_TITLE_01")!.Raw;
        var crcOff = IndexOfU32(raw, FrontendUiDef.GraphicIndexCrc);
        Assert.True(crcOff >= 0);
        Assert.Equal(3, BitConverter.ToInt32(raw, crcOff + 4));
        Assert.Equal(3, title01.GraphicBankId);
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
        Assert.Contains(profile, w => w.TextTag == "TEXT_GUI_MENU_NEW_PROFILE");
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
