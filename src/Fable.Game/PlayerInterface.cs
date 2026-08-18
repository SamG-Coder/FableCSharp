namespace Fable.Game;

/// <summary>
/// Init Player Interface <c>004473A0</c>:
/// alloc <c>0x898</c>, vtbl
/// <c>01231BDC</c>, stored at
/// <c>game+32</c>. Pump is vtbl+4
/// <c>00446A30</c> → <c>00446330</c>
/// <c>009F4ED0</c>. Listeners:
/// vtbl+32 accept, vtbl+16 apply.
/// Zero <c>E8</c> of <c>00446A30</c>.
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
    public const int GameOffset = 32;
    public const int ObjectSize = 0x898;
    public const int DisabledOffset = 1948;
    public const int FallbackFlagOffset = 2196;
    public const int EventOffset = 2016;
    public const int EventDwords = 10;
    public const uint EventDeviceFn = 0x00A03B50;
    public const int EventDeviceOffset = 32;
    public const int SkipDevice = 2;
    public const int SkipKey = 15;

    public bool Present { get; private set; }
    public bool Disabled { get; set; }
    public int PumpCalls { get; private set; }
    public int DeliveredCount { get; private set; }
    public IReadOnlyList<(int Type, int Key)> Delivered => _delivered;

    private readonly List<IPlayerInputListener> _listeners = [];
    private readonly List<(int Type, int Key)> _delivered = [];

    public void Construct()
    {
        if (Present)
            return;
        Present = true;
        Disabled = false;
    }

    public void Register(IPlayerInputListener listener) =>
        _listeners.Add(listener);

    /// <summary>
    /// <c>004457F0</c>:
    /// <c>[this+2196]=0</c>.
    /// </summary>
    public void Preprocess()
    {
    }

    /// <summary>
    /// One <c>00446A30</c>:
    /// <c>00446330</c> poll
    /// <c>009F4ED0</c>, skip
    /// device 2 / key 15, walk
    /// listeners vtbl+32 / +16.
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
            foreach (var listener in _listeners)
            {
                if (!listener.Accept(type, key))
                    continue;
                listener.Apply(type, key);
                _delivered.Add((type, key));
                DeliveredCount++;
                return true;
            }
        }

        return false;
    }
}

/// <summary>
/// Listener on the
/// <c>00446330</c> list at
/// <c>+4</c>. vtbl+32 filter,
/// vtbl+16 apply. Fallback
/// walk is vtbl+24.
/// </summary>
public interface IPlayerInputListener
{
    bool Accept(int type, int key);
    void Apply(int type, int key);
}

/// <summary>
/// Records every event.
/// Stand-in until a recovered
/// listener vtbl is bound.
/// </summary>
public sealed class RecordingInputListener : IPlayerInputListener
{
    public List<(int Type, int Key)> Hits { get; } = [];

    public bool Accept(int type, int key) => type != 0;

    public void Apply(int type, int key) => Hits.Add((type, key));
}
