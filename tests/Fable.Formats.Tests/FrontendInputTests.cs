using Fable.Core;
using Fable.Formats.Defs;
using Fable.Game;

namespace Fable.Formats.Tests;

public sealed class FrontendInputTests
{
    [Fact]
    public void Native_ids_slots_and_attach_names_match_exe()
    {
        Assert.Equal(0xE5, FrontendMessages.PressStart);
        Assert.Equal(0x126, FrontendMessages.AcceptNewProfile);
        Assert.Equal(0x124, FrontendMessages.MainMenu);
        Assert.Equal(15, FrontendMessages.NewGame);
        Assert.Equal(0x14, FrontendMessages.PressStartSlot);
        Assert.Equal(0x17, FrontendMessages.NewProfileSlot);
        Assert.Equal(0, FrontendMessages.MainMenuSlot);
        Assert.Equal("UI_FRONTEND_PRESS_START_MENU", FrontendMessages.PressStartMenu);
        Assert.Equal("UI_FRONTEND_NEW_PROFILE_SCREEN", FrontendMessages.NewProfileMenu);
        Assert.Equal("UI_NEW_PROFILE_EDIT_BOX", FrontendMessages.NewProfileEditBox);
        Assert.Equal("UI_ACCEPT_NEW_PROFILE", FrontendMessages.AcceptNewProfileDef);
        Assert.Equal(
            "UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE",
            FrontendMessages.MainMenuNoContinue);
        Assert.Equal(4, FrontendInputMap.Type4);
        Assert.Equal(26, FrontendInputMap.ActionType4);
        Assert.Equal(3, FrontendInputMap.Type4Device);
        Assert.Equal(0x00A03C80u, FrontendInputMap.Type4RecordCtor);
        Assert.Equal(0x00AB5420u, FrontendInputMap.Type4TranslateFn);
        Assert.False(FrontendInputMap.DikPosterUnread);
        Assert.True(FrontendInputMap.NativeLmbPostsPressStart);
        Assert.False(FrontendInputMap.NativeKeyPostsE5);
        Assert.False(FrontendInputMap.NativeKeyNPostsNewGame);
        Assert.False(FrontendInputMap.NativeEnterPostsNewGame);
        Assert.True(FrontendInputMap.Leftover14OpenForDestPresentNotes);
        Assert.True(FrontendInputMap.NativeType4UsesCurrentInner);
        Assert.False(FrontendInputMap.NativeType4UsesDestAabb);
        Assert.Equal(0x0055ACB0u, FrontendInputMap.Type4InnerHoverTick);
        Assert.Equal(4, FrontendInputMap.Type4CurrentInnerOffset);
        Assert.Equal(12, FrontendInputMap.Type4DimofsButton0);
        Assert.Equal(1, FrontendInputMap.Type4RawDown);
        Assert.Equal(4, FrontendInputMap.Type6RawUp);
        Assert.Equal(12, EngineInput.Type4DimofsButton0);
        Assert.Equal(1, EngineInput.Type4RawDown);
        Assert.Equal(4, EngineInput.Type6RawUp);
        Assert.Equal(0x0059A238u, FrontendMessages.UiMessageFn);
        Assert.Equal(0x00599D5Cu, FrontendMessages.PressStartAcceptFn);
        Assert.Equal(0x00595845u, FrontendMessages.NoProfileFn);
        Assert.Equal(0x00596917u, FrontendMessages.NewProfileBindFn);
        Assert.Equal(0x00851920u, FrontendMessages.CommitNameFn);
        Assert.Equal(0x0059697Au, FrontendMessages.CommitProfileFn);
        Assert.Equal(0x0042F2A2u, FrontendMessages.LeaveFrontendSite);
        Assert.Equal(37, FrontendMessages.NewProfileEditType);
    }

    [Fact]
    public void Isolated_table_empty_0xE5_then_0x126_then_15()
    {
        var state = new FrontendMessages.State();
        Assert.Equal(FrontendMessages.Screen.PressStart, state.Screen);
        Assert.Equal(0, state.ProfileCount);

        FrontendMessages.ApplyMessage(state, FrontendMessages.PressStart);
        Assert.True(state.UiArmed);
        Assert.True(state.Ui100);
        Assert.Equal(FrontendMessages.Screen.PressStart, state.Screen);
        Assert.False(state.RetailNewGameFlag);

        FrontendMessages.ApplyTick(state);
        Assert.False(state.UiArmed);
        Assert.Equal(FrontendMessages.Screen.NewProfile, state.Screen);
        Assert.True(state.Ui96Present);
        Assert.False(state.Ui96Plus5);
        Assert.Equal("Default", state.EditName);

        FrontendMessages.ApplyMessage(state, FrontendMessages.AcceptNewProfile);
        Assert.True(state.Ui96Plus5);
        Assert.False(state.Ui96Plus4);
        Assert.Equal(FrontendMessages.Screen.NewProfile, state.Screen);

        FrontendMessages.ApplyTick(state);
        Assert.Equal(FrontendMessages.Screen.MainMenuNoContinue, state.Screen);
        Assert.False(state.Ui96Present);
        Assert.False(state.RetailNewGameFlag);

        FrontendMessages.ApplyMessage(state, FrontendMessages.NewGame);
        Assert.True(state.RetailNewGameFlag);
    }

    [Fact]
    public void Isolated_0xE5_and_0x126_are_same_frame_as_tick()
    {
        var e5 = new FrontendMessages.State();
        FrontendMessages.ApplyMessageThenTick(e5, FrontendMessages.PressStart);
        Assert.Equal(FrontendMessages.Screen.NewProfile, e5.Screen);

        var accept = new FrontendMessages.State
        {
            Screen = FrontendMessages.Screen.NewProfile,
            Ui96Present = true,
        };
        FrontendMessages.ApplyMessageThenTick(
            accept, FrontendMessages.AcceptNewProfile);
        Assert.Equal(FrontendMessages.Screen.MainMenuNoContinue, accept.Screen);
    }

    [Fact]
    public void Isolated_one_name_0xE5_attaches_main_menu()
    {
        var state = new FrontendMessages.State { ProfileCount = 1 };
        FrontendMessages.ApplyMessageThenTick(state, FrontendMessages.PressStart);
        Assert.Equal(FrontendMessages.Screen.MainMenuNoContinue, state.Screen);
        Assert.False(state.UiArmed);
        Assert.False(state.RetailNewGameFlag);
    }

    [Fact]
    public void Isolated_many_names_0xE5_is_00597B20_unread()
    {
        var state = new FrontendMessages.State { ProfileCount = 2 };
        FrontendMessages.ApplyMessageThenTick(state, FrontendMessages.PressStart);
        Assert.Equal(FrontendMessages.Screen.PressStart, state.Screen);
        Assert.False(state.UiArmed);
        Assert.Equal(0x00597B20u, FrontendMessages.ProfilesManyFn);
    }

    [Fact]
    public void Isolated_0x124_empty_continue_is_no_continue_menu()
    {
        var state = new FrontendMessages.State();
        FrontendMessages.ApplyMessage(state, FrontendMessages.MainMenu);
        Assert.Equal(FrontendMessages.Screen.MainMenuNoContinue, state.Screen);
        Assert.False(state.RetailNewGameFlag);
    }

    [Fact]
    public void Isolated_empty_name_0x126_does_not_arm()
    {
        var state = new FrontendMessages.State
        {
            Screen = FrontendMessages.Screen.NewProfile,
            Ui96Present = true,
            EditName = "   ",
        };
        FrontendMessages.ApplyMessageThenTick(
            state, FrontendMessages.AcceptNewProfile);
        Assert.False(state.Ui96Plus5);
        Assert.Equal(FrontendMessages.Screen.NewProfile, state.Screen);
    }

    [Fact]
    public void Isolated_0x126_not_writable_stays_on_new_profile()
    {
        var state = new FrontendMessages.State
        {
            Screen = FrontendMessages.Screen.NewProfile,
            Ui96Present = true,
            CanCreateProfile = false,
        };
        FrontendMessages.ApplyMessageThenTick(
            state, FrontendMessages.AcceptNewProfile);
        Assert.True(state.Ui96Plus5);
        Assert.Equal(FrontendMessages.Screen.NewProfile, state.Screen);
    }

    [Fact]
    public void Type4_drives_lifecycle_0xE5_then_0x126_then_15()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.Equal(
            EngineLifecycle.FrontendPressStartMenu, life.FrontendMenuRoot);
        Assert.Contains(life.FrontendWidgets, w =>
            w.Type == FrontendInputMap.Type10 &&
            w.Type10Packet == FrontendMessages.PressStart);
        Assert.Contains(life.FrontendWidgets, w =>
            w.Type == FrontendInputMap.TypeButton &&
            w.ActionOnLeftUnclicked == FrontendMessages.PressStart);
        life.QueueInput(FrontendInputMap.Type4, 0);
        life.QueueInput(FrontendInputMap.Type6, 0);
        Assert.True(life.Pump());
        Assert.Equal(
            EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        Assert.Equal("Default", life.FrontendEditBoxName);
        Assert.False(life.RetailNewGameFlag);
        Assert.Contains(life.FrontendWidgets, w =>
            w.Name == "UI_ACCEPT_NEW_PROFILE" &&
            w.ActionOnLeftUnclicked == FrontendMessages.AcceptNewProfile);
        ClickNamed(life, "UI_ACCEPT_NEW_PROFILE");
        Assert.Equal(
            EngineLifecycle.FrontendMainMenuNoContinue, life.FrontendMenuRoot);
        Assert.False(life.RetailNewGameFlag);
        Assert.Contains(life.FrontendWidgets, w =>
            w.Name == "UI_FRONTEND_BUTTON_NEW_GAME" &&
            w.ActionOnLeftUnclicked == FrontendMessages.NewGame);
        ClickNamed(life, "UI_FRONTEND_BUTTON_NEW_GAME");
        Assert.True(life.RetailNewGameFlag);
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.Equal("FinalAlbion.wld", life.WorldFileName);
    }

    [Fact]
    public void Type15_WM_CHAR_inserts_at_edit_box_cursor_not_DIK()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.QueueInput(FrontendInputMap.Type4, 0);
        life.QueueInput(FrontendInputMap.Type6, 0);
        Assert.True(life.Pump());
        Assert.Equal("Default", life.FrontendEditBoxName);
        Assert.Equal(15, EngineInput.Type15);
        Assert.Equal(34, FrontendInputMap.ActionType15);
        Assert.Equal(0x0053FE50u, EngineLifecycle.EditBoxInsertFn);
        Assert.Equal(356, EngineLifecycle.EditBoxTextOffset);
        Assert.Equal(0x32, EngineLifecycle.EditBoxCap);
        Assert.False(EngineLifecycle.EditBoxTypesFromDik);
        life.QueueInput(EngineInput.Type15, 'H');
        Assert.True(life.Pump());
        Assert.Equal("DefaultH", life.FrontendEditBoxName);
        life.QueueInput(EngineInput.Type15, 8);
        Assert.True(life.Pump());
        Assert.Equal("Default", life.FrontendEditBoxName);
    }

    [Fact]
    public void Main_Menu_Type4_Type6_posts_15_from_current_pointer_without_TypeMouse()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.QueueInput(FrontendInputMap.Type4, 0);
        life.QueueInput(FrontendInputMap.Type6, 0);
        Assert.True(life.Pump());
        ClickNamed(life, "UI_ACCEPT_NEW_PROFILE");
        Assert.Equal(
            EngineLifecycle.FrontendMainMenuNoContinue, life.FrontendMenuRoot);
        Assert.False(life.RetailNewGameFlag);
        var index = IndexOf(life, "UI_FRONTEND_BUTTON_NEW_GAME");
        Assert.True(
            FrontendHitTest.TryDestPoint(life.FrontendWidgets, index, out var x, out var y),
            "UI_FRONTEND_BUTTON_NEW_GAME dest empty");
        life.SetFrontendPointer(x, y);
        life.QueueInput(FrontendInputMap.Type4, 0);
        life.QueueInput(FrontendInputMap.Type6, 0);
        Assert.True(life.Pump());
        Assert.True(life.RetailNewGameFlag);
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.Equal("FinalAlbion.wld", life.WorldFileName);
    }

    [Fact]
    public void Queue_drives_lifecycle_without_a_key()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.Equal(
            EngineLifecycle.FrontendPressStartMenu, life.FrontendMenuRoot);

        var map = new FrontendInputMap();
        map.Queue(FrontendMessages.PressStart);
        Assert.True(map.TryDequeue(out var first));
        Assert.Equal(FrontendMessages.PressStart, first);
        life.DispatchFrontendMessage(first);
        Assert.True(life.Pump());
        Assert.Equal(
            EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        Assert.Equal("Default", life.FrontendEditBoxName);
        Assert.False(life.RetailNewGameFlag);
        Assert.Equal(EngineStage.Frontend, life.Stage);

        map.Queue(FrontendMessages.AcceptNewProfile);
        Assert.True(map.TryDequeue(out var second));
        life.DispatchFrontendMessage(second);
        Assert.True(life.Pump());
        Assert.Equal(
            EngineLifecycle.FrontendMainMenuNoContinue, life.FrontendMenuRoot);
        Assert.False(life.RetailNewGameFlag);
        Assert.Equal(EngineStage.Frontend, life.Stage);

        map.Queue(FrontendMessages.NewGame);
        Assert.True(map.TryDequeue(out var third));
        life.DispatchFrontendMessage(third);
        Assert.True(life.Pump());
        Assert.True(life.RetailNewGameFlag);
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.Equal("FinalAlbion.wld", life.WorldFileName);
    }

    [Fact]
    public void Type4_action_26_posts_stored_widget_message()
    {
        Assert.Equal(26, FrontendInputMap.ActionFromEvent(FrontendInputMap.Type4, 0));
        var press = new List<FrontendWidget>
        {
            new("UI_FRONTEND_PRESS_START_MENU", 10, 0, 0, 0, 0, null, null,
                Type10Packet: FrontendMessages.PressStart),
        };
        Assert.Equal(
            FrontendMessages.PressStart,
            FrontendInputMap.MessageFromWidgets(
                FrontendInputMap.ActionType4, press));
        Assert.Equal(
            FrontendMessages.PressStart,
            FrontendInputMap.TryMapEvent(FrontendInputMap.Type4, 0, press));
        Assert.Null(FrontendInputMap.MessageFromAction(
            FrontendInputMap.ActionType4, FrontendMessages.PressStartMenu));
        Assert.Null(FrontendInputMap.MessageFromAction(
            FrontendInputMap.ActionType4, FrontendMessages.NewProfileMenu));
        Assert.Null(FrontendInputMap.MessageFromAction(
            FrontendInputMap.ActionType4, FrontendMessages.MainMenuNoContinue));
        Assert.Equal(0x0054E280u, FrontendInputMap.Type10ActionFn);
        Assert.Equal(0x0054E2FAu, FrontendInputMap.Type10PostSite);
        Assert.Equal(0x00598EE6u, FrontendInputMap.AttachWriteE5);
        Assert.Equal(0x0054E4F0u, FrontendInputMap.Type10StoreMsgFn);
        Assert.Equal(0x0059B5D7u, FrontendInputMap.SlotLookupFn);
        Assert.Equal(84, FrontendInputMap.SlotMapOffset);
        Assert.Equal(20, FrontendInputMap.SlotNodeValueOffset);
        Assert.Equal(352, FrontendInputMap.Type10StoredMsgOffset);
        Assert.Equal(228, FrontendInputMap.ActionOnLeftUnclickedDefOffset);
        Assert.Equal(FrontendUiDef.ActionOnLeftUnclickedRetailOffset, FrontendInputMap.ActionOnLeftUnclickedDefOffset);
        Assert.Equal(224, FrontendInputMap.Action26PostDefOffset);
        Assert.Equal(372, FrontendInputMap.Action26ListOffset);
        Assert.Equal(380, FrontendInputMap.ActionOnLeftUnclickedListOffset);
        Assert.Equal(0x0055AF60u, FrontendInputMap.Type34ClickFn);
        Assert.Equal(0x0055ACF0u, FrontendInputMap.ActionOnLeftUnclickedPostFn);
        Assert.Equal(0x0055B9D0u, FrontendInputMap.Action26NopFn);
        var accept = new List<FrontendWidget>
        {
            new("UI_FRONTEND_NEW_PROFILE_SCREEN", 10, 0, 0, 0, 0, null, null),
            new("UI_ACCEPT_NEW_PROFILE", 38, 0, 0, 0, 0, null, null,
                ActionOnLeftUnclicked: FrontendMessages.AcceptNewProfile,
                Armed: true),
        };
        Assert.Null(FrontendInputMap.MessageFromWidgets(
            FrontendInputMap.ActionType4, accept));
        Assert.Equal(
            FrontendMessages.AcceptNewProfile,
            FrontendInputMap.MessageFromPlus228List(accept));
        Assert.Equal(
            FrontendMessages.AcceptNewProfile,
            FrontendInputMap.MessageFromWidgets(
                FrontendInputMap.ActionType6, accept));
        Assert.Null(FrontendInputMap.MessageFromPlus228List(
        [
            accept[1] with { Armed = false },
        ]));
        Assert.Null(FrontendInputMap.MessageFromAction(
            FrontendInputMap.ActionType4, FrontendMessages.NewProfileMenu));
    }

    [Fact]
    public void New_Profile_empty_space_Type4_Type6_does_not_accept()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.QueueInput(FrontendInputMap.Type4, 0);
        life.QueueInput(FrontendInputMap.Type6, 0);
        Assert.True(life.Pump());
        Assert.Equal(EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        life.SetFrontendPointer(12f, 12f);
        life.QueueInput(FrontendInputMap.Type4, 0);
        life.QueueInput(FrontendInputMap.Type6, 0);
        Assert.True(life.Pump());
        Assert.Equal(EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        Assert.False(life.RetailNewGameFlag);
        Assert.Null(FrontendHitTest.HitIndex(life.FrontendWidgets, 12f, 12f));
    }

    [Fact]
    public void New_Profile_hover_0055BF10_swaps_type38_on_off()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.QueueInput(FrontendInputMap.Type4, 0);
        life.QueueInput(FrontendInputMap.Type6, 0);
        Assert.True(life.Pump());
        Assert.Equal(EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        var apply = IndexOf(life, "UI_ACCEPT_NEW_PROFILE");
        Assert.True(
            FrontendHitTest.TryDestPoint(life.FrontendWidgets, apply, out var ax, out var ay));
        var offIdle = life.FrontendWidgets.First(w => w.Name == "UI_SPRITE_ACCEPT_OFF");
        var onIdle = life.FrontendWidgets.First(w => w.Name == "UI_SPRITE_ACCEPT_ON");
        var hoverAudioSerial = life.FrontendAudioSerial;
        Assert.Equal(255u, offIdle.Colour >> 24);
        Assert.Equal(0u, onIdle.Colour >> 24);
        life.SetFrontendPointer(ax, ay);
        life.QueueInput(FrontendInputMap.TypeMouse, 0);
        Assert.True(life.Pump());
        Assert.True(life.FrontendWidgets[apply].Hovered);
        var offHover = life.FrontendWidgets.First(w => w.Name == "UI_SPRITE_ACCEPT_OFF");
        var onHover = life.FrontendWidgets.First(w => w.Name == "UI_SPRITE_ACCEPT_ON");
        Assert.Equal(0u, offHover.Colour >> 24);
        Assert.Equal(255u, onHover.Colour >> 24);
        Assert.True(life.FrontendAudioSerial > hoverAudioSerial);
        Assert.Equal("CS_GUI_1", life.FrontendAudioCue);
        Assert.Equal(apply, FrontendHitTest.HitIndex(life.FrontendWidgets, ax, ay));
        Assert.Equal(0x0055BF10u, FrontendHitTest.HoverSelectFn);
        Assert.Equal(0x00530260u, FrontendWidgetType.ContainerDrawFn);
        life.QueueInput(FrontendInputMap.Type4, 0);
        var clickAudioSerial = life.FrontendAudioSerial;
        life.QueueInput(FrontendInputMap.Type6, 0);
        Assert.True(life.Pump());
        Assert.True(life.FrontendAudioSerial > clickAudioSerial);
        Assert.Equal(EngineLifecycle.FrontendMainMenuNoContinue, life.FrontendMenuRoot);
    }

    [Fact]
    public void New_Profile_name_hover_updates_authored_components_without_replacing_nested_sprites()
    {
        var life = ReachNewProfile();
        var button = IndexOf(life, "UI_NEW_PROFILE_BUTTON");
        Assert.True(FrontendHitTest.TryDestPoint(life.FrontendWidgets, button, out var x, out var y));
        var immediateChildren = life.FrontendWidgets
            .Select((widget, index) => (widget, index))
            .Where(pair => pair.widget.ParentIndex == button)
            .Select(pair => pair.index)
            .ToArray();
        Assert.NotEmpty(immediateChildren);
        var nestedSprites = life.FrontendWidgets
            .Select((widget, index) => (widget, index))
            .Where(pair => pair.widget.ParentIndex != button &&
                           IsDescendant(life.FrontendWidgets, pair.index, button))
            .ToDictionary(pair => pair.index, pair =>
                (pair.widget.GraphicId, pair.widget.TextureName));
        var hit = FrontendHitTest.HitRect(life.FrontendWidgets, button);
        Assert.True(FrontendHitTest.Contains(life.FrontendWidgets, button, x, y),
            $"point={x},{y} hit={hit}");
        life.SetFrontendPointer(x, y);
        life.QueueInput(FrontendInputMap.TypeMouse, 0);
        Assert.True(life.Pump());
        Assert.True(life.FrontendWidgets[button].Hovered);
        var selectedState = life.FrontendWidgets[button].State;
        Assert.All(immediateChildren, child =>
            Assert.Equal(selectedState, life.FrontendWidgets[child].State));
        Assert.All(nestedSprites, pair =>
        {
            var current = life.FrontendWidgets[pair.Key];
            Assert.Equal(pair.Value.GraphicId, current.GraphicId);
            Assert.Equal(pair.Value.TextureName, current.TextureName);
        });

        var title = life.FrontendWidgets.First(w =>
            w.Name == "UI_TEXT_NEW_PROFILE_MENU_TITLE");
        Assert.True(title.Visible);
        Assert.False(string.IsNullOrEmpty(title.Text));
        Assert.True(title.GlyphCount > 0);
    }

    [Fact]
    public void New_Profile_arrow_hover_uses_button_state_and_authored_graphic_bounds()
    {
        var life = ReachNewProfile();
        var slider = IndexOf(life, "UI_OPTIONS_CONTROL_METHOD_TEXT_SLIDER");
        var arrow = life.FrontendWidgets
            .Select((widget, index) => (widget, index))
            .First(pair => pair.widget.Name == "UI_OPTIONS_LEFT_ARROW_TEXT" &&
                           pair.widget.ControlOwnerIndex == slider).index;
        Assert.Equal(1, life.FrontendWidgets[arrow].HoveredState);
        Assert.True(FrontendHitTest.TryDestPoint(
            life.FrontendWidgets, arrow, out var x, out var y));
        life.SetFrontendPointer(x, y);
        life.QueueInput(FrontendInputMap.TypeMouse, 0);
        Assert.True(life.Pump());
        Assert.True(life.FrontendWidgets[arrow].Hovered);
        Assert.All(life.FrontendWidgets
            .Select((widget, index) => (widget, index))
            .Where(pair => IsDescendant(life.FrontendWidgets, pair.index, arrow)),
            pair => Assert.Equal(1, pair.widget.State));
    }

    [Fact]
    public void New_Profile_Apply_and_Cancel_follow_their_authored_messages()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.QueueInput(FrontendInputMap.Type4, 0);
        life.QueueInput(FrontendInputMap.Type6, 0);
        Assert.True(life.Pump());
        ClickNamed(life, "UI_CANCEL");
        Assert.Equal(EngineLifecycle.FrontendPressStartMenu, life.FrontendMenuRoot);
        life.QueueInput(FrontendInputMap.Type4, 0);
        life.QueueInput(FrontendInputMap.Type6, 0);
        Assert.True(life.Pump());
        ClickNamed(life, "UI_ACCEPT_NEW_PROFILE");
        Assert.Equal(EngineLifecycle.FrontendMainMenuNoContinue, life.FrontendMenuRoot);
    }

    [Fact]
    public void New_Profile_per_control_LMB_uses_dest_not_empty_space()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.QueueInput(FrontendInputMap.Type4, 0);
        life.QueueInput(FrontendInputMap.Type6, 0);
        Assert.True(life.Pump());
        Assert.Equal(EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);

        life.SetFrontendPointer(12f, 12f);
        life.QueueInput(FrontendInputMap.Type4, 0);
        life.QueueInput(FrontendInputMap.Type6, 0);
        Assert.True(life.Pump());
        Assert.Equal(EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        Assert.Null(FrontendHitTest.HitIndex(life.FrontendWidgets, 12f, 12f));

        var slider = IndexOf(life, "UI_OPTIONS_CONTROL_METHOD_TEXT_SLIDER");
        var before = life.FrontendWidgets[slider].ActiveChild;
        life.SetFrontendPointer(96f, 304f);
        life.QueueInput(FrontendInputMap.Type4, 0);
        life.QueueInput(FrontendInputMap.Type6, 0);
        Assert.True(life.Pump());
        Assert.Equal(EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        Assert.Null(FrontendHitTest.HitIndex(life.FrontendWidgets, 96f, 304f));
        Assert.Equal(before, life.FrontendWidgets[slider].ActiveChild);

        var rightArrow = life.FrontendWidgets
            .Select((widget, index) => (widget, index))
            .First(pair => pair.widget.Name == "UI_OPTIONS_RIGHT_ARROW_TEXT" &&
                           pair.widget.ControlOwnerIndex == slider).index;
        ClickIndex(life, rightArrow);
        Assert.Equal(EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        Assert.NotEqual(before, life.FrontendWidgets[slider].ActiveChild);

        var knob = IndexOf(life, "UI_SLIDER_CAMERA_SENSITIVITY");
        var knobBefore = life.FrontendWidgets[knob].SliderValueX;
        var numericRight = life.FrontendWidgets
            .Select((widget, index) => (widget, index))
            .First(pair => pair.widget.Name == "UI_OPTIONS_RIGHT_ARROW" &&
                           pair.widget.ControlOwnerIndex == knob).index;
        ClickIndex(life, numericRight);
        Assert.Equal(knobBefore + life.FrontendWidgets[knob].StepX,
            life.FrontendWidgets[knob].SliderValueX, 5);
        ClickIndex(life, knob);
        Assert.Equal(EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        Assert.Equal(
            knob,
            FrontendHitTest.HitIndex(
                life.FrontendWidgets,
                life.FrontendPointerX,
                life.FrontendPointerY));

        ClickNamed(life, "UI_CANCEL");
        Assert.Equal(EngineLifecycle.FrontendPressStartMenu, life.FrontendMenuRoot);
        life.QueueInput(FrontendInputMap.Type4, 0);
        life.QueueInput(FrontendInputMap.Type6, 0);
        Assert.True(life.Pump());
        ClickNamed(life, "UI_ACCEPT_NEW_PROFILE");
        Assert.Equal(EngineLifecycle.FrontendMainMenuNoContinue, life.FrontendMenuRoot);
    }

    private static void ClickNamed(EngineLifecycle life, string name) =>
        ClickIndex(life, IndexOf(life, name));

    private static void ClickIndex(EngineLifecycle life, int index)
    {
        Assert.True(
            FrontendHitTest.TryDestPoint(life.FrontendWidgets, index, out var x, out var y),
            life.FrontendWidgets[index].Name + " dest empty");
        var expected = life.FrontendWidgets[index].ControlOwnerIndex >= 0
            ? life.FrontendWidgets[index].ControlOwnerIndex
            : index;
        Assert.Equal(expected, FrontendHitTest.HitIndex(life.FrontendWidgets, x, y));
        life.SetFrontendPointer(x, y);
        life.QueueInput(FrontendInputMap.TypeMouse, 0);
        Assert.True(life.Pump());
        life.QueueInput(FrontendInputMap.Type4, 0);
        life.QueueInput(FrontendInputMap.Type6, 0);
        Assert.True(life.Pump());
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

    private static EngineLifecycle ReachNewProfile()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var life = new EngineLifecycle();
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.QueueInput(FrontendInputMap.Type4, 0);
        life.QueueInput(FrontendInputMap.Type6, 0);
        Assert.True(life.Pump());
        return life;
    }

    private static bool IsDescendant(
        IReadOnlyList<FrontendWidget> widgets, int index, int ancestor)
    {
        var parent = widgets[index].ParentIndex;
        while ((uint)parent < (uint)widgets.Count)
        {
            if (parent == ancestor)
                return true;
            parent = widgets[parent].ParentIndex;
        }
        return false;
    }

    [Fact]
    public void Keyboard_and_Return_do_not_map_to_a_frontend_message()
    {
        const int dikReturn = 28;
        const int dikEscape = 1;
        Assert.Equal(
            FrontendInputMap.ActionFromKey,
            FrontendInputMap.ActionFromEvent(FrontendInputMap.TypeKey, dikReturn));
        Assert.Null(FrontendInputMap.MessageFromAction(
            FrontendInputMap.ActionFromKey, FrontendMessages.PressStartMenu));
        Assert.Null(FrontendInputMap.TryMapEvent(
            FrontendInputMap.TypeKey, dikReturn, FrontendMessages.PressStartMenu));
        Assert.Null(FrontendInputMap.TryMapEvent(
            FrontendInputMap.TypeKey, dikEscape, FrontendMessages.PressStartMenu));
        Assert.Null(FrontendInputMap.TryMapEvent(
            FrontendInputMap.TypeKey, dikReturn, FrontendMessages.NewProfileMenu));
        Assert.Null(FrontendInputMap.MessageFromAction(
            FrontendInputMap.ActionFromKey, FrontendMessages.MainMenuNoContinue));
        Assert.False(FrontendInputMap.DikPosterUnread);
    }

    [Fact]
    public void Accept_0x126_and_NewGame_15_have_no_recovered_action()
    {
        Assert.Null(FrontendInputMap.MessageFromAction(
            FrontendInputMap.ActionType4, FrontendMessages.NewProfileMenu));
        Assert.Null(FrontendInputMap.MessageFromAction(
            FrontendInputMap.ActionFromKey, FrontendMessages.NewProfileMenu));
        Assert.Null(FrontendInputMap.MessageFromAction(
            FrontendInputMap.EditBoxActionA, FrontendMessages.NewProfileMenu));
        Assert.Null(FrontendInputMap.MessageFromAction(
            FrontendInputMap.ActionType4, FrontendMessages.MainMenuNoContinue));
        Assert.Equal(0x0041E6D3u, FrontendInputMap.InputVtblMessageFn);
        Assert.Equal(56, FrontendInputMap.InputVtblMessage);
        Assert.Equal(0x0052F040u, FrontendInputMap.WidgetMessageNoopFn);
        Assert.Equal(284, FrontendInputMap.WidgetMessageVtbl);
        Assert.Equal(0x0122F5D4u, FrontendInputMap.GenericWidgetVtbl);
        Assert.Equal(0x012497E4u, FrontendInputMap.Type10WidgetVtbl);
    }

    [Fact]
    public void Proven_0042E3EE_type_to_action()
    {
        Assert.Equal(33, FrontendInputMap.ActionFromEvent(1, 28));
        Assert.Equal(26, FrontendInputMap.ActionFromEvent(4, 0));
        Assert.Equal(28, FrontendInputMap.ActionFromEvent(6, 0));
        Assert.Equal(35, FrontendInputMap.ActionFromEvent(7, 0));
        Assert.Equal(27, FrontendInputMap.ActionFromEvent(10, 0));
        Assert.Equal(25, FrontendInputMap.ActionFromEvent(13, 0));
        Assert.Equal(34, FrontendInputMap.ActionFromEvent(15, 0));
        Assert.Null(FrontendInputMap.ActionFromEvent(17, 0));
        Assert.Equal(40, FrontendInputMap.EventTypeOffset);
        Assert.Equal(0, FrontendInputMap.EventKeyOffset);
        Assert.Equal(0x0042E3EEu, FrontendInputMap.InputPollFn);
        Assert.Equal(0x0055CB10u, FrontendInputMap.ActionApply);
    }
}
