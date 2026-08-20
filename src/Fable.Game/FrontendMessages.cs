namespace Fable.Game;

/// <summary>
/// Frontend UI message ids, slots, attach
/// names, and the <c>0059A238</c> /
/// <c>00599E3F</c> transition table.
/// Guards are first-seen native: empty
/// <c>005955AB</c>, one 4-byte name,
/// nonempty trim, <c>004067C0</c>
/// writable.
/// </summary>
public static class FrontendMessages
{
    public const uint UiMessageFn = 0x0059A238;
    public const int UiMessageVtbl = 32;
    public const uint PressStartAcceptFn = 0x00599D5C;
    public const uint ProfileEnumFn = 0x005955AB;
    public const uint NoProfileFn = 0x00595845;
    public const uint UiTickFn = 0x00599E3F;
    public const uint NewProfileBindFn = 0x00596917;
    public const uint MenuSwitchFn = 0x00596763;
    public const uint Ui96CtorFn = 0x00851700;
    public const uint Ui96EditBoxFn = 0x00851770;
    public const uint CommitNameFn = 0x00851920;
    public const uint CommitProfileFn = 0x0059697A;
    public const uint CanCreateProfileFn = 0x004067C0;
    public const uint MainMenuFn = 0x0059899A;
    public const uint MenuAttachFn = 0x00595A06;
    public const uint NewGameApply = 0x0059A2DA;
    public const uint NewGameThunk = 0x00594F28;
    public const uint ProfilesManyFn = 0x00597B20;
    public const uint LeaveFrontendSite = 0x0042F2A2;

    /// <summary>
    /// <c>0059A238</c> <c>sub ecx, 0xE5</c>
    /// → <c>00599D5C</c>.
    /// </summary>
    public const int PressStart = 0xE5;

    /// <summary>
    /// <c>0059A238</c> → <c>00851920</c>.
    /// frontend.bin <c>UI_ACCEPT_NEW_PROFILE</c>
    /// stores this id. No <c>mov […], 0x126</c>
    /// writer in .text.
    /// </summary>
    public const int AcceptNewProfile = 0x126;

    /// <summary>
    /// <c>0059A238</c> → <c>0059899A</c>.
    /// </summary>
    public const int MainMenu = 0x124;

    /// <summary>
    /// <c>0059A238</c> msg-15 → <c>0059A2DA</c>
    /// <c>[ui+28].vtbl+16</c> then
    /// <c>00594F28</c> <c>[retail+41]=1</c>.
    /// </summary>
    public const int NewGame = 15;

    /// <summary>
    /// frontend.bin <c>UI_CANCEL</c> persist
    /// <c>0x53C644E4</c> +228. Host dump
    /// type-38 point posts this id. No
    /// recovered <c>0059A238</c> branch.
    /// </summary>
    public const int CancelNewProfile = 86;

    public const int PressStartSlot = 0x14;
    public const int NewProfileSlot = 0x17;
    /// <summary>
    /// <c>00595A06</c> overwrites
    /// existing key <c>0</c>.
    /// </summary>
    public const int MainMenuSlot = 0;
    public const int UiArmedOffset = 160;
    public const int Ui100Offset = 100;
    public const int Ui96Offset = 96;
    public const int RetailNewGameFlagOffset = 41;
    public const int NewProfileEditType = 37;

    public const string PressStartMenu = "UI_FRONTEND_PRESS_START_MENU";
    public const string NewProfileMenu = "UI_FRONTEND_NEW_PROFILE_SCREEN";
    public const string NewProfileEditBox = "UI_NEW_PROFILE_EDIT_BOX";
    public const string AcceptNewProfileDef = "UI_ACCEPT_NEW_PROFILE";
    public const string MainMenuNoContinue =
        "UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE";
    public const string MainMenuContinue = "UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE";
    public const string ProfileDefaultFallback = "Default";

    public enum Screen
    {
        PressStart,
        NewProfile,
        MainMenuNoContinue,
        MainMenuContinue,
        LeaveFrontend,
    }

    /// <summary>
    /// Isolated <c>0059A238</c> +
    /// <c>00599E3F</c> state. First-seen
    /// empty profile list, one name,
    /// writable create.
    /// </summary>
    public sealed class State
    {
        public Screen Screen { get; set; } = Screen.PressStart;
        public int ProfileCount { get; set; }
        public int ContinueCount { get; set; }
        public bool UiArmed { get; set; }
        public bool Ui100 { get; set; }
        public bool Ui96Present { get; set; }
        public bool Ui96Plus4 { get; set; }
        public bool Ui96Plus5 { get; set; }
        public bool CanCreateProfile { get; set; } = true;
        public string EditName { get; set; } = ProfileDefaultFallback;
        public bool RetailNewGameFlag { get; set; }

        public string MenuRoot => Screen switch
        {
            Screen.PressStart => PressStartMenu,
            Screen.NewProfile => NewProfileMenu,
            Screen.MainMenuContinue => MainMenuContinue,
            Screen.LeaveFrontend => MainMenuNoContinue,
            _ => MainMenuNoContinue,
        };
    }

    /// <summary>
    /// One <c>0059A238</c> dispatch.
    /// Same-frame bind is <see cref="ApplyTick"/>.
    /// </summary>
    public static void ApplyMessage(State state, int msg)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (msg == PressStart)
        {
            AcceptPressStart(state);
            return;
        }

        if (msg == AcceptNewProfile)
        {
            AcceptProfile(state);
            return;
        }

        if (msg == MainMenu)
        {
            AttachMainMenu(state);
            return;
        }

        if (msg == CancelNewProfile && state.Screen == Screen.NewProfile)
        {
            state.Screen = Screen.PressStart;
            state.Ui96Present = false;
            state.Ui96Plus4 = false;
            state.Ui96Plus5 = false;
            return;
        }

        if (msg == NewGame)
            state.RetailNewGameFlag = true;
    }

    /// <summary>
    /// <c>00599E3F</c> after
    /// <c>0042DC94</c>. <c>[ui+160]</c>
    /// → <c>00596917</c> slot <c>0x17</c>.
    /// <c>[ui+96+5]≠0</c> and <c>+4==0</c>
    /// with empty <c>005955AB</c> →
    /// <c>0059697A</c>.
    /// </summary>
    public static void ApplyTick(State state)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (state.UiArmed)
        {
            state.UiArmed = false;
            state.Screen = Screen.NewProfile;
            state.Ui96Present = true;
            state.Ui96Plus4 = false;
            state.Ui96Plus5 = false;
            if (string.IsNullOrEmpty(state.EditName))
                state.EditName = ProfileDefaultFallback;
            return;
        }

        if (!state.Ui96Present || !state.Ui96Plus5 || state.Ui96Plus4)
            return;
        if (state.ProfileCount != 0)
            return;
        if (!state.CanCreateProfile)
            return;
        state.Screen = Screen.MainMenuNoContinue;
        state.Ui96Present = false;
        state.Ui96Plus5 = false;
        state.Ui96Plus4 = false;
    }

    /// <summary>
    /// Message then tick: 0xE5 empty
    /// <c>00595845</c> and <c>00596917</c>
    /// are the same <c>0042DC94</c> frame.
    /// </summary>
    public static void ApplyMessageThenTick(State state, int msg)
    {
        ApplyMessage(state, msg);
        ApplyTick(state);
    }

    /// <summary>
    /// <c>00599D5C</c>: empty
    /// <c>005955AB</c> → <c>00595845</c>.
    /// One 4-byte name → <c>0059899A</c>.
    /// Else <c>00597B20</c> unread (no change).
    /// </summary>
    private static void AcceptPressStart(State state)
    {
        if (state.ProfileCount == 0)
        {
            state.UiArmed = true;
            state.Ui100 = true;
            return;
        }

        if (state.ProfileCount == 1)
            AttachMainMenu(state);
    }

    /// <summary>
    /// <c>00851920</c>: nonempty trim
    /// sets <c>+5=1</c> <c>+4=0</c>.
    /// Already-armed or empty name is a no-op.
    /// </summary>
    private static void AcceptProfile(State state)
    {
        if (!state.Ui96Present || state.Ui96Plus5)
            return;
        if (state.EditName.Trim().Length <= 0)
            return;
        state.Ui96Plus5 = true;
        state.Ui96Plus4 = false;
    }

    /// <summary>
    /// <c>0059899A</c>: empty continue
    /// list → <c>NO_LIVEAWARE_NO_CONTINUE</c>.
    /// </summary>
    private static void AttachMainMenu(State state)
    {
        state.Screen = state.ContinueCount == 0
            ? Screen.MainMenuNoContinue
            : Screen.MainMenuContinue;
        state.Ui96Present = false;
        state.Ui96Plus5 = false;
        state.Ui96Plus4 = false;
        state.UiArmed = false;
    }
}
