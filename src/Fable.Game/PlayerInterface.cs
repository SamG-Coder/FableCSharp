namespace Fable.Game;

/// <summary>
/// Init Player Interface <c>004473A0</c>:
/// alloc <c>0x898</c>, vtbl
/// <c>01231BDC</c>, stored at
/// <c>game+32</c>. Pump is vtbl+4
/// <c>00446A30</c> → <c>00446330</c>
/// <c>009F4ED0</c> then miss
/// <c>00446220</c>.
/// </summary>
public sealed class PlayerInterface
{
    public const uint Ctor = 0x004473A0;
    public const uint Vtbl = 0x01231BDC;
    public const uint Dtor = 0x00449420;
    public const uint PumpFn = 0x00446A30;
    public const int PumpVtbl = 4;
    public const uint PollFn = 0x00446330;
    public const uint FallbackFn = 0x00446220;
    public const uint PreprocessFn = 0x004457F0;
    public const uint StoreFn = 0x004193C4;
    public const uint ListInitFn = 0x00A0D4A0;
    public const uint ListInsertFn = 0x00A0D4F0;
    public const uint RegisterFn = 0x00A0D2B0;
    public const uint RegisterThunk = 0x00687A70;
    public const uint EventZeroFn = 0x00A0D300;
    public const uint EventWriteFn = 0x00A0D3A0;
    public const uint EventConsumeFn = 0x00A0D390;
    public const uint EventSlotFn = 0x00A0D3C0;
    public const uint EventPriorityFn = 0x00A0D3B0;
    public const uint LookupFn = 0x00449990;
    public const uint LookupKeyFn = 0x00487510;
    public const uint OwnerCtor = 0x0044A3B0;
    public const uint OwnerVtbl = 0x01231CD0;
    public const int OwnerSize = 44;
    public const uint ApplyFn = 0x0041649C;
    public const uint ApplyWorldHitFn = 0x0049D8C0;
    public const uint ApplyAction2Fn = 0x00415FF2;
    public const uint ApplyPlayerFn = 0x004AE9A0;
    public const uint ApplyQueueFn = 0x009F1650;
    public const uint ApplyWorldFn = 0x0049E1D0;
    public const uint ApplyDisplayFn = 0x00434A30;
    public const int GameOffset = 32;
    public const int OwnerOffset = 28;
    public const int ObjectSize = 0x898;
    public const int DisabledOffset = 1948;
    public const int FallbackFlagOffset = 2196;
    public const int EventOffset = 2016;
    public const int EventDwords = 10;
    public const int EventBytes = 40;
    public const int QueueOffset = 0x2010;
    public const uint EventDeviceFn = 0x00A03B50;
    public const uint EventField36Fn = 0x00A03B60;
    public const int EventDeviceOffset = 32;
    public const int EventField36Offset = 36;
    public const int EventFlag168Offset = 168;
    public const int EventResultOffset = 4;
    public const int EventPriorityOffset = 172;
    public const int SkipDevice = 2;
    public const int SkipKey = 15;
    public const int ResultNone = 0;
    public const int ResultSelect = 1;
    public const int ResultConsume = 2;
    public const int Action2 = 2;
    public const int WorldTickSlot1 = 1;

    public bool Present { get; private set; }
    public bool Disabled { get; set; }
    public bool FallbackArmed { get; private set; }
    public int PumpCalls { get; private set; }
    public int PollHits { get; private set; }
    public int FallbackCalls { get; private set; }
    public int AcceptHits { get; private set; }
    public int ApplyHits { get; private set; }
    public int DeliveredCount { get; private set; }
    public int QueuedCount => _queue.Count;
    public int OwnerDefaultResult { get; set; }
    public PlayerEvent? LastEvent { get; private set; }
    public IReadOnlyList<PlayerEvent> Delivered => _delivered;
    public IReadOnlyList<PlayerEvent> Queued => _queue;
    public IReadOnlyList<IPlayerInputListener> Listeners => _listeners;

    private readonly List<IPlayerInputListener> _listeners = [];
    private readonly List<PlayerEvent> _delivered = [];
    private readonly List<PlayerEvent> _queue = [];
    private readonly List<(int Key, int Result)> _ownerItems = [];

    public void Construct()
    {
        if (Present)
            return;
        Present = true;
        Disabled = false;
        FallbackArmed = false;
        OwnerDefaultResult = 0;
    }

    /// <summary>
    /// <c>00687A70</c> → <c>00A0D2B0</c>
    /// → <c>00A0D4F0</c> insert at +4.
    /// </summary>
    public void Register(IPlayerInputListener listener)
    {
        listener.Bind(this);
        _listeners.Add(listener);
    }

    /// <summary>
    /// <c>0044A3B0</c> leaves +12 empty.
    /// <c>00449990</c> miss returns +24.
    /// </summary>
    public void AddOwnerItem(int key, int result) =>
        _ownerItems.Add((key, result));

    /// <summary>
    /// <c>00449990</c>: walk +12/+16
    /// comparing <c>00487510</c>
    /// <c>[item+552]</c> to
    /// <c>[event+36]</c>; miss is +24.
    /// </summary>
    public int LookupResult(int field36)
    {
        foreach (var (key, result) in _ownerItems)
        {
            if (key == field36)
                return result;
        }

        return OwnerDefaultResult;
    }

    /// <summary>
    /// <c>004457F0</c>:
    /// <c>[this+2196]=0</c>.
    /// </summary>
    public void Preprocess() => FallbackArmed = false;

    /// <summary>
    /// One <c>00446A30</c>:
    /// <c>00446330</c> then
    /// <c>00446220</c>. Copies
    /// 10 dwords when selected.
    /// </summary>
    public bool Pump(EngineInput device)
    {
        Construct();
        PumpCalls++;
        if (Disabled)
            return false;

        while (device.TryDequeue(out var type, out var key))
        {
            if (type == 0)
                continue;
            if (key == SkipKey)
                continue;

            var ev = PlayerEvent.FromDevice(type, key);
            if (TryPoll(ev))
            {
                PollHits++;
                Deliver(ev);
                return true;
            }

            if (TryFallback(ev))
            {
                FallbackCalls++;
                Deliver(ev);
                return true;
            }

            FallbackCalls++;
        }

        return false;
    }

    /// <summary>
    /// <c>00446330</c> +4 walk:
    /// vtbl+32 accept, <c>00449990</c>
    /// into dest+4, vtbl+28 gate,
    /// vtbl+16 apply. dest+4==1
    /// selects; ==2 consumes.
    /// </summary>
    public bool TryPoll(PlayerEvent ev)
    {
        foreach (var listener in _listeners)
        {
            if (!listener.Accept(ev))
                continue;
            AcceptHits++;
            ev.Result = LookupResult(ev.Field36);
            if (listener.Gate() || !ev.BlockApply)
                listener.Apply(ev);
            ApplyHits++;
            if (ev.Result == ResultSelect)
            {
                if (ev.Action < 0)
                    ev.Action = 0;
                return true;
            }
            if (ev.Result == ResultConsume)
                return false;
        }

        return false;
    }

    /// <summary>
    /// <c>00446220</c>: one-shot via
    /// +2196. Zeros event, writes
    /// <c>00449700</c> default, walks
    /// vtbl+24. Returns [+168]!=0.
    /// </summary>
    public bool TryFallback(PlayerEvent ev)
    {
        if (FallbackArmed)
            return false;
        FallbackArmed = true;
        ev.Reset();
        ev.Action = OwnerDefaultResult;
        foreach (var listener in _listeners)
            listener.Fallback(ev);
        return ev.Flag168;
    }

    /// <summary>
    /// <c>0041649C</c>: occupied
    /// <c>0049D8C0</c> or action==2
    /// <c>00415FF2</c> then
    /// <c>004AE9A0</c> → <c>009F1650</c>.
    /// Always <c>0049E1D0</c> /
    /// <c>00434A30</c>.
    /// </summary>
    public bool ApplyInputEvent(PlayerEvent ev, bool playerReady)
    {
        LastEvent = ev;
        var hit = WorldTickOccupied(ev.Action) || ev.Action == Action2;
        if (hit && playerReady)
            QueueAction(ev);
        return hit;
    }

    /// <summary>
    /// <c>0049D8C0</c>:
    /// <c>[event+0]*64+0x13B9288</c>
    /// occupied. Slot 1 is the
    /// recovered world-tick thunk.
    /// </summary>
    public static bool WorldTickOccupied(int action) =>
        action == WorldTickSlot1;

    /// <summary>
    /// <c>009F1650</c> at
    /// player+<c>0x2010</c>.
    /// </summary>
    public void QueueAction(PlayerEvent ev)
    {
        if (ev.Replaceable)
        {
            for (var i = 0; i < _queue.Count; i++)
            {
                if (_queue[i].Action == ev.Action && _queue[i].Key == ev.Key)
                {
                    _queue[i] = ev;
                    return;
                }
            }
        }

        _queue.Add(ev);
    }

    private void Deliver(PlayerEvent ev)
    {
        LastEvent = ev;
        _delivered.Add(ev);
        DeliveredCount++;
    }
}

/// <summary>
/// 10-dword event copied by
/// <c>00A0D3C0</c> / <c>009F1650</c>.
/// dest+4 is <c>00449990</c> result.
/// </summary>
public sealed class PlayerEvent
{
    public int Action { get; set; }
    public int Result { get; set; }
    public int Type { get; set; }
    public int Key { get; set; }
    public int Device { get; set; }
    public int Field36 { get; set; }
    public bool Flag168 { get; set; }
    public int Priority { get; set; } = 2;
    public bool Replaceable { get; set; }
    public bool BlockApply { get; set; }

    public static PlayerEvent FromDevice(int type, int key) => new()
    {
        Type = type,
        Key = key,
        Device = 0,
        Field36 = 0,
        Result = 0,
        Action = -1,
        Priority = 2,
    };

    /// <summary>
    /// <c>00A0D300</c>.
    /// </summary>
    public void Reset()
    {
        Action = -1;
        Result = 0;
        Flag168 = false;
        Priority = 2;
    }

    /// <summary>
    /// <c>00A0D390</c>: dest+4=2,
    /// +168=0.
    /// </summary>
    public void Consume()
    {
        Result = PlayerInterface.ResultConsume;
        Flag168 = false;
    }
}

/// <summary>
/// +4 list object. vtbl+32 accept,
/// +28 gate, +16 apply, +24 fallback,
/// +4 register.
/// </summary>
public interface IPlayerInputListener
{
    uint Vtbl { get; }
    void Bind(PlayerInterface owner);
    bool Accept(PlayerEvent ev);
    bool Gate();
    void Apply(PlayerEvent ev);
    void Fallback(PlayerEvent ev);
}

/// <summary>
/// <c>0123758C</c> from Create Players
/// <c>00488D10</c> (not Init Player Interface).
/// Accept <c>00687DB0</c>, apply
/// <c>00687FD0</c>, gate
/// <c>004863A0</c>, fallback
/// <c>00486390</c> ret.
/// </summary>
public sealed class ActionInputListener : IPlayerInputListener
{
    public const uint VtblVa = 0x0123758C;
    public const uint AcceptFn = 0x00687DB0;
    public const uint ApplyFn = 0x00687FD0;
    public const uint GateFn = 0x004863A0;
    public const uint FallbackFn = 0x00486390;
    public const uint CtorFn = 0x00687A30;
    public const uint FactoryFn = 0x00488D10;
    public const int DeviceKeyboard = 1;

    public uint Vtbl => VtblVa;
    public int AcceptCalls { get; private set; }
    public int ApplyCalls { get; private set; }

    public void Bind(PlayerInterface owner)
    {
    }

    /// <summary>
    /// <c>00687DB0</c>: device!=1
    /// accept; device==1 filters
    /// <c>00A03B60</c> via
    /// <c>004874F0</c>.
    /// </summary>
    public bool Accept(PlayerEvent ev)
    {
        AcceptCalls++;
        if (ev.Device != DeviceKeyboard)
            return true;
        return ev.Field36 == 0;
    }

    /// <summary>
    /// <c>004863A0</c> <c>mov al,1</c>.
    /// </summary>
    public bool Gate() => true;

    /// <summary>
    /// <c>00687FD0</c>: type 1 jumps
    /// to cleanup without
    /// <c>00A0D390</c>.
    /// </summary>
    public void Apply(PlayerEvent ev)
    {
        ApplyCalls++;
        if (ev.Type == EngineInput.TypeKey)
            return;
        ev.Consume();
    }

    /// <summary>
    /// <c>00486390</c> <c>ret 4</c>.
    /// </summary>
    public void Fallback(PlayerEvent ev)
    {
    }
}
