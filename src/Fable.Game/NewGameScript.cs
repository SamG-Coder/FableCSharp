namespace Fable.Game;

/// <summary>
/// First-seen <c>S_QNOVI</c> runner matching
/// <c>script-runtime</c> v8 / <c>script-bank</c> v1.
/// Per-frame update is microthread pump <c>00A44880</c>
/// (dt <c>009E1BC0</c> stored at <c>+8</c>, resume
/// <c>00A44660</c>). Fiber <c>00A446A0</c> calls
/// persist slot <c>+16</c> (<c>00DAADA0</c>
/// <c>AttackOver</c> at <c>this+80</c> via
/// <c>004045C0</c>) then run slot <c>+8</c>
/// (<c>00DABAC0</c>). Yield is <c>00A44690</c>.
/// Do not invent the <c>+80</c> writer.
/// Cutscene start is <c>00DB86B0</c> →
/// <c>00CBFB7D("CS_OAKVALE_INTRO_FATHER")</c>. First-seen
/// <c>00DABAC0</c> registers <c>NOVI_LiveFather</c> then
/// waits the map; TNG Father construct starts that body.
/// First slice: PlayMusic (no yield) then FadeOut 0.5,0
/// via <c>vtbl+1488</c>. Special-case FadeOut does not run.
/// PlayAVI / wake are later. Do not invent their playback.
/// </summary>
public sealed class NewGameScript
{
    public const uint RegisteringScripts = 0x00CB5D80;
    public const uint BindFactory = 0x00CB5C90;
    public const uint StoreFactory = 0x00CB7210;
    public const uint StartList = 0x00CB7780;
    public const uint ListInvoke = 0x00CB70E0;
    public const uint ListWalk = 0x00CB6EA0;
    public const uint PerItem = 0x00CB6CE0;
    public const int ListRecordBytes = 24;
    public const uint ContextGlobal = 0x0143E8F8;
    public const int WaitFlagPtrOffset = 76;
    public const uint WaitFlagVtbl = 2592;
    public const uint StartQuestVtbl = 1104;
    public const uint UpdateFn = 0x00A44880;
    public const uint FiberEntry = 0x00A446A0;
    public const uint ResumeFn = 0x00A44660;
    public const uint YieldFn = 0x00A44690;
    public const uint FrameDt = 0x009E1BC0;
    public const uint CreateFiber = 0x00A447D0;
    public const uint PersistAttackOver = 0x00DAADA0;
    public const uint PersistHelper = 0x004045C0;
    public const string PersistAttackOverName = "AttackOver";
    public const int DtOffset = 8;
    public const int FiberFlagOffset = 5;
    public const int FiberSetupVtbl = 16;
    public const int FiberRunVtbl = 8;
    public const uint Scheduler = 0x013D2828;
    public const uint LiveFatherFactory = 0x00DAC2C0;
    public const uint LiveFatherVtbl = 0x012D8388;
    public const string LiveFatherScript = "NOVI_LiveFather";

    public enum Phase
    {
        NotStarted,
        Setup,
        PreAttackWait,
        WaitingGate80,
    }

    public Phase Current { get; private set; } = Phase.NotStarted;
    public float DtAtPlus8 { get; private set; }
    public bool Gate80 { get; private set; }
    /// <summary>
    /// <c>00DABAC0</c> always registers
    /// <c>NOVI_LiveFather</c> before the map-wait.
    /// TNG Father construct then starts <c>00DB86B0</c>.
    /// </summary>
    public bool CutsceneStarted { get; private set; }
    /// <summary>
    /// Runner special-case at <c>00CBFDD0</c> is off for
    /// first-seen <c>CS_OAKVALE_INTRO_FATHER</c>.
    /// </summary>
    public bool FadeSpecialCaseApplied { get; private set; }
    public bool PlayMusicRan { get; private set; }
    public bool FadeOutReached { get; private set; }
    public float FadeDuration { get; private set; }
    public float FadeParam { get; private set; }

    public void Start()
    {
        Current = Phase.Setup;
        DtAtPlus8 = 0f;
        // 00A447D0 creates the fiber. 00A446A0 then
        // 00DAADA0 persist AttackOver and 00DABAC0 run.
        // 00DABAC0 registers NOVI_LiveFather + 00DAC2C0.
        CutsceneStarted = true;
        // 00CC8EAC PlayMusic then 00CD17FD → 00CC012E.
        // Next line FadeOut 0.5,0 calls vtbl+1488(0.5, 0).
        PlayMusicRan = true;
        FadeOutReached = true;
        FadeDuration = RegionTravel.FadeSpecialCaseSeconds;
        FadeParam = 0f;
        ApplyPersist(Gate80);
        Current = Phase.PreAttackWait;
    }

    /// <summary>
    /// Save/load field <c>AttackOver</c> at <c>this+80</c>
    /// (<c>00DAADA0</c> / <c>004045C0</c>).
    /// </summary>
    public void ApplyPersist(bool attackOver)
    {
        Gate80 = attackOver;
        if (attackOver && Current is Phase.Setup or Phase.PreAttackWait)
            Current = Phase.WaitingGate80;
    }

    /// <summary>
    /// <c>00A44880</c>: store dt at <c>+8</c>, resume fiber.
    /// Does not write <c>+80</c>.
    /// </summary>
    public void Update(float dt)
    {
        if (Current is Phase.NotStarted or Phase.WaitingGate80)
            return;
        if (dt < 0f)
            return;
        DtAtPlus8 = dt;
        if (Current == Phase.Setup)
            Start();
    }

    public void Tick(float dt) => Update(dt);
}
