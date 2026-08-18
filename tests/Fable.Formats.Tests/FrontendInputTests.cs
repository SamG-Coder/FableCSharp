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
        Assert.Equal("UI_FRONTEND_PRESS_START_MENU", FrontendMessages.PressStartMenu);
        Assert.Equal("UI_FRONTEND_NEW_PROFILE_SCREEN", FrontendMessages.NewProfileMenu);
        Assert.Equal("UI_NEW_PROFILE_EDIT_BOX", FrontendMessages.NewProfileEditBox);
        Assert.Equal("UI_ACCEPT_NEW_PROFILE", FrontendMessages.AcceptNewProfileDef);
        Assert.Equal(
            "UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE",
            FrontendMessages.MainMenuNoContinue);
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
    public void Type4_action_26_is_0xE5_on_press_start_only()
    {
        Assert.Equal(26, FrontendInputMap.ActionFromEvent(FrontendInputMap.Type4, 0));
        Assert.Equal(
            FrontendMessages.PressStart,
            FrontendInputMap.MessageFromAction(
                FrontendInputMap.ActionType4,
                FrontendMessages.PressStartMenu));
        Assert.Equal(
            FrontendMessages.PressStart,
            FrontendInputMap.TryMapEvent(
                FrontendInputMap.Type4, 0, FrontendMessages.PressStartMenu));
        Assert.Null(FrontendInputMap.MessageFromAction(
            FrontendInputMap.ActionType4, FrontendMessages.NewProfileMenu));
        Assert.Null(FrontendInputMap.MessageFromAction(
            FrontendInputMap.ActionType4, FrontendMessages.MainMenuNoContinue));
        Assert.Equal(0x0054E280u, FrontendInputMap.Type10ActionFn);
        Assert.Equal(0x0054E2FAu, FrontendInputMap.Type10PostSite);
        Assert.Equal(0x00598EE6u, FrontendInputMap.AttachWriteE5);
        Assert.Equal(0x0054E4F0u, FrontendInputMap.Type10StoreMsgFn);
        Assert.Equal(352, FrontendInputMap.Type10StoredMsgOffset);
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
        Assert.True(FrontendInputMap.DikPosterUnread);
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
