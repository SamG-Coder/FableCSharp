namespace Fable.Game;

/// <summary>
/// Observation façade over <see cref="ScriptRuntime"/> for
/// first-seen <c>S_QNOVI</c>. Addresses match
/// <c>script-runtime</c> / <c>script-bank</c> exports.
/// Scene behaviour is produced by the generic VM, not
/// by setting these properties.
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
    /// <summary>
    /// Give / visible <c>vtbl+1152</c>.
    /// Not construct. S_QNOVI posts it
    /// at <c>00DBE295</c> after
    /// <c>AttackOver</c>.
    /// </summary>
    public const uint GiveQuestVtbl = 1152;
    /// <summary>
    /// PreAttack start is
    /// <c>vtbl+1104</c>, not Give.
    /// Give <c>00DBE295</c> is after
    /// raid AVI, PostAttack, and Maze.
    /// Barrel timer callback
    /// <c>00DB4F70</c> is first-seen
    /// on <c>00DABAC0</c>, not smash.
    /// </summary>
    public const uint StartBarrelTimerCallback = 0x00DB4F70;
    public const bool GiveAfterPostAttackAndMaze = true;
    public const string ChocolateBoxDef = "OBJECT_CHOCOLATE_BOX_UNGIVEABLE";
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

    public ScriptRuntime Runtime { get; }

    public NewGameScript(ScriptRuntime runtime) => Runtime = runtime;

    public float DtAtPlus8 => Runtime.DtAtPlus8;
    public bool Gate80 => Runtime.PersistBool(PersistAttackOverName);
    public bool CutsceneStarted => Runtime.HasStarted(RegionTravel.IntroCutscene);
    public bool FadeSpecialCaseApplied =>
        Runtime.FindInterpreter(RegionTravel.IntroCutscene)?.FadeSpecialCaseApplied ?? false;
    public bool PlayMusicRan => Runtime.ExecutedVerb(RegionTravel.IntroCutscene, "PlayMusic");
    public bool FadeOutReached => Runtime.ExecutedVerb(RegionTravel.IntroCutscene, "FadeOut");
    public float FadeDuration => Runtime.FadeDuration;
    public float FadeParam => Runtime.FadeParam;
    public float OverlayAlpha => Runtime.OverlayAlpha;
    public byte OverlayAlphaByte => Runtime.OverlayAlphaByte;

    public void ApplyPersist(bool attackOver) =>
        Runtime.ApplyPersist(PersistAttackOverName, attackOver);

    public void Update(float dt) => Runtime.Update(dt);
}
