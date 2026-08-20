namespace Fable.Game;

/// <summary>
/// <c>0042E3EE</c> poll + <c>0041E5F2</c>
/// action singleton. Event type is
/// <c>[record+40]</c> (<c>00A03B40</c>),
/// key is <c>[record+0]</c>
/// (<c>00A03B70</c>). Not WASD.
/// </summary>
public sealed class EngineInput
{
    public const uint Getter = 0x0041E5F2;
    public const uint Ctor = 0x0041E3F6;
    public const uint SingletonVa = 0x013B8710;
    public const int ObjectSize = 0xD0;
    public const uint Vtbl = 0x01230134;
    public const uint ActionApply = 0x0055CB10;
    public const uint KeyStore = 0x0042D4F7;
    public const int LastKeyOffset = 192;
    public const int BusyOffset = 156;
    public const uint BindDefaults = 0x0041DF10;
    public const int BindTableOffset = 36;
    public const uint EventTypeFn = 0x00A03B40;
    public const uint EventKeyFn = 0x00A03B70;
    public const int EventTypeOffset = 40;
    public const int EventKeyOffset = 0;
    public const int TypeKey = 1;
    /// <summary>
    /// <c>0042E3EE</c> <c>[record+40]==4</c>
    /// → action 26. LMB down
    /// (<c>00A03C80</c>, device 3).
    /// Not a DIK.
    /// </summary>
    public const int Type4 = 4;
    /// <summary>
    /// LMB up (<c>00A03D60</c>) →
    /// action 28.
    /// </summary>
    public const int Type6 = 6;
    /// <summary>
    /// RMB down (<c>00A03D90</c>) →
    /// action 35.
    /// </summary>
    public const int Type7 = 7;
    public const int Type10 = 10;
    /// <summary>
    /// Mouse move (<c>00A03FB0</c>),
    /// not the Press Start click.
    /// </summary>
    public const int TypeMouse = 13;
    /// <summary>
    /// WM_CHAR harvest <c>00A03CB0</c>
    /// → action 34. Not a DIK.
    /// </summary>
    public const int Type15 = 15;
    public const int ActionType15 = 34;
    public const int Type4Device = 3;
    public const int Type4DimofsButton0 = 12;
    public const int Type4RawDown = 1;
    public const int Type6RawUp = 4;
    public const int TypeAnalog = 17;
    public const int ActionFromKey = 33;
    public const int ActionType4 = 26;
    public const int ActionType6 = 28;
    public const int ActionType10 = 27;
    /// <summary>
    /// Action 27 is RMB hover-in
    /// (<c>0055AE01</c> / <c>vtbl+592</c>),
    /// not persist release and not
    /// <c>CTCActionUse*</c> barrels /
    /// gold / doors.
    /// </summary>
    public const bool ActionType10IsWorldUse = false;
    public const bool ActionType10IsRmbHover = true;
    public const int ActionMouse = 25;
    /// <summary>
    /// <c>0055CB10</c> records/broadcasts
    /// classified actions. First-seen
    /// consumers are frontend type 11 /
    /// type 32. Not a locomotion apply.
    /// Pad A is type 19 action 22, not
    /// type 4. Stick type 17 sets NESW
    /// bits and does not
    /// <c>0055CB10</c>. Movement slots
    /// are <c>0x6F/0x70/0x72/0x6D</c>,
    /// not WASD.
    /// </summary>
    public const bool ActionApplyIsLocomotion = false;
    public const int TypePadA = 19;
    public const int ActionPadA = 22;
    public const bool TypeAnalogPostsActionApply = false;

    /// <summary>
    /// <c>0041DF10(0)</c> keyboard defaults.
    /// Slots 0–3 are the movement keys
    /// <c>0042E3EE</c> hard-compares.
    /// </summary>
    public static readonly (int Slot, int Key)[] KeyboardDefaults =
    [
        (0, 0x6F), (1, 0x70), (2, 0x72), (3, 0x6D),
        (4, 0x73), (5, 0x6E), (6, 0x1E), (7, 0x30),
        (8, 0x36), (9, 0x67), (10, 0x10), (11, 0x11),
        (12, 0x2C), (13, 0x2D), (14, 0x18), (15, 0x26),
    ];

    public const int KeyMove0 = 0x6F;
    public const int KeyMove1 = 0x70;
    public const int KeyMove2 = 0x72;
    public const int KeyMove3 = 0x6D;
    public const int KeyDikA = 0x1E;
    public const int KeyDikB = 0x30;
    public const int KeyDikY = 0x15;

    public bool Present { get; private set; }
    public int LastKey { get; private set; }
    public int Mask { get; private set; }
    public bool Busy { get; set; }
    public IReadOnlyList<int> Actions => _actions;
    public IReadOnlyList<(int Type, int Key)> Applied => _applied;

    private readonly List<int> _actions = [];
    private readonly List<(int Type, int Key)> _applied = [];
    private readonly List<(int Type, int Key)> _queue = [];

    /// <summary>
    /// <c>0041E5F2</c>: alloc <c>0xD0</c>,
    /// ctor <c>0041E3F6</c> vtbl
    /// <c>01230134</c>, <c>0041DF10(0)</c>.
    /// </summary>
    public void Construct()
    {
        if (Present)
            return;
        Present = true;
        Busy = false;
        LastKey = 0;
    }

    public void Queue(int type, int key) => _queue.Add((type, key));

    public int PendingCount => _queue.Count;

    /// <summary>
    /// <c>009F4ED0</c> / <c>009F4F10</c>
    /// one record for <c>00446330</c>.
    /// </summary>
    public bool TryDequeue(out int type, out int key)
    {
        if (_queue.Count == 0)
        {
            type = 0;
            key = 0;
            return false;
        }

        (type, key) = _queue[0];
        _queue.RemoveAt(0);
        return true;
    }

    /// <summary>
    /// One <c>0042E3EE</c> poll:
    /// <c>and [ebp-4],0</c>, apply queued
    /// events, then mask → <c>0055CB10</c>.
    /// </summary>
    public void Pump()
    {
        Construct();
        BeginPoll();
        foreach (var (type, key) in _queue)
            ApplyEvent(type, key);
        _queue.Clear();
        EndPoll();
    }

    public void BeginPoll()
    {
        Construct();
        Mask = 0;
        _actions.Clear();
        _applied.Clear();
    }

    /// <summary>
    /// One <c>009F4F10</c> record.
    /// Type 1 stores <c>+192</c> and
    /// dispatches action 33, then ORs
    /// the proven movement bits.
    /// </summary>
    public void ApplyEvent(int type, int key)
    {
        Construct();
        _applied.Add((type, key));
        if (type == TypeKey)
        {
            LastKey = key;
            Dispatch(ActionFromKey);
            Mask |= KeyBit(key);
            return;
        }

        if (type == Type4)
        {
            Dispatch(ActionType4);
            return;
        }

        if (type == Type6)
        {
            Dispatch(ActionType6);
            return;
        }

        if (type == Type10)
        {
            Dispatch(ActionType10);
            return;
        }

        if (type == TypeMouse)
            Dispatch(ActionMouse);
    }

    /// <summary>
    /// After the poll loop: priority
    /// encoder on <c>[ebp-4]</c> then
    /// <c>0055CB10</c>.
    /// </summary>
    public void EndPoll()
    {
        if ((Mask & 0x400) != 0 || (Mask & 0x100) != 0)
            Dispatch(4);
        else if ((Mask & 0x800) != 0 || (Mask & 0x200) != 0)
            Dispatch(5);
        else if ((Mask & 0x20000) != 0)
            Dispatch(22);
        else if ((Mask & 0x44) != 0)
        {
            Dispatch(2);
            Dispatch(20);
        }
        else if ((Mask & 0x88) != 0)
        {
            Dispatch(3);
            Dispatch(21);
        }
        else if ((Mask & 0x11) != 0)
            Dispatch(0);
        else if ((Mask & 0x22) != 0)
            Dispatch(1);
        else if ((Mask & 0x1000) != 0)
            Dispatch(8);
        else if ((Mask & 0x2000) != 0)
            Dispatch(9);
        else if ((Mask & 0x4000) != 0)
            Dispatch(10);
        else if ((Mask & 0x8000) != 0)
            Dispatch(11);
        else if ((Mask & 0x10000) != 0)
            Dispatch(23);
    }

    public static int KeyBit(int key) => key switch
    {
        KeyMove3 => 0x1,
        KeyMove2 => 0x2,
        KeyMove0 => 0x4,
        KeyMove1 => 0x8,
        KeyDikA => 0x100,
        KeyDikB => 0x200,
        KeyDikY => 0x20000,
        _ => 0,
    };

    /// <summary>
    /// <c>0055CB10</c> listener walk.
    /// No recovered player-move listener
    /// yet — actions are recorded.
    /// </summary>
    public void Dispatch(int action) => _actions.Add(action);
}
