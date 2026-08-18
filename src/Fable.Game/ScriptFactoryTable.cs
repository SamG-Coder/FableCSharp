namespace Fable.Game;

/// <summary>
/// Recovered name-table / factory bindings. Exe
/// <c>00DABAC0</c> registers script names + factories
/// before <c>00DBDE40</c>. TNG <c>ScriptName</c> then
/// <c>004C97B0</c> / <c>00CB8960</c> looks up the name
/// and the factory starts the cutscene. This is the
/// generic mechanism — not an Oakvale fact inside
/// <c>StartNewGame</c>.
/// </summary>
public static class ScriptFactoryTable
{
    public const uint NameRegister = 0x00CB8230;
    public const uint ConstructBind = 0x00CB8960;
    public const uint ThingConstruct = 0x004C97B0;
    public const uint SqnoviRun = 0x00DABAC0;
    public const uint LiveFatherFactory = 0x00DAC2C0;
    public const uint LiveFatherStart = 0x00DB86B0;
    public const uint LiveFatherVtbl = 0x012D8388;
    public const uint LiveFatherRdata = 0x012D8370;

    public static readonly ScriptNameFactory[] Recovered =
    [
        new(RegionTravel.LiveFatherScript, RegionTravel.IntroCutscene,
            LiveFatherFactory, LiveFatherStart,
            BindingKind.ProvenGeneric,
            "00DABAC0 writes factory 00DAC2C0 at +16 (0x012D8370); " +
            "TNG CREATURE_HERO_FATHER ScriptName NOVI_LiveFather; " +
            "004C97B0 → 00CB8960 → 00DB8520 → 00DAC2C0; " +
            "fiber 00DB8630 [+52].vtbl+4 = 00DB86B0 starts CS_OAKVALE_INTRO_FATHER"),
    ];

    public static ScriptNameFactory? Find(string scriptName)
    {
        foreach (var factory in Recovered)
        {
            if (factory.ScriptName.Equals(scriptName, StringComparison.OrdinalIgnoreCase))
                return factory;
        }

        return null;
    }
}

public readonly record struct ScriptNameFactory(
    string ScriptName,
    string CutsceneName,
    uint Factory,
    uint Start,
    BindingKind Kind,
    string Evidence);

public enum BindingKind
{
    ProvenGeneric,
    Temporary,
    Hardcoded,
}

/// <summary>
/// Recovered persist slots. Type comes from the
/// storage site, not from a host bool dictionary.
/// </summary>
public static class PersistTable
{
    public const uint AttackOverHelper = 0x004045C0;
    public const uint AttackOverWrite = 0x00DAADA0;
    public const int AttackOverOffset = 80;
    public const bool AttackOverWriterKnown = false;

    public const uint BindBool = 0x004045C0;
    public const uint BindInt = 0x00410BE0;
    public const uint SunnyvaleBind = 0x00CDC070;

    public static readonly PersistSlot[] Recovered =
    [
        new(NewGameScript.PersistAttackOverName, PersistKind.Bool, false,
            AttackOverOffset, AttackOverWrite,
            BindingKind.ProvenGeneric,
            "00DAADA0 004045C0(\"AttackOver\", this+80); writer UNREAD; first-seen false"),
    ];

    /// <summary>
    /// <c>00CDC070</c> Q_SunnyvaleMaster
    /// vtbl+4. Bool via <c>004045C0</c>,
    /// int via <c>00410BE0</c>. Defaults
    /// are <c>00CDBA10</c> zeros.
    /// </summary>
    public static readonly PersistSlot[] Sunnyvale =
    [
        Bool("HauntedBarrowFieldsCompleted", 17),
        Bool("GrannyMemoryReturned", 74),
        Bool("IsLunaHuman", 75),
        Bool("FriendOfForeman", 72),
        Bool("BridgeOpened", 73),
        Bool("CondemnedManDead", 76),
        Bool("CondemnedManForgiven", 77),
        Bool("CondemnedManMeetsBodyGuard", 78),
        Bool("CondemnedManMeetsBodyGuardCutSceneStart", 79),
        Bool("CondemnedManMeetsBodyGuardCutSceneFinished", 80),
        Bool("SeenAbbeyMotherAtGuild", 96),
        Bool("DefeatedThunder", 97),
        Bool("LostToThunder", 98),
        Bool("KilledThunder", 99),
        Bool("CollectedSoulFromArena", 100),
        Bool("KilledBriar", 101),
        Bool("CollectedSoulFromMother", 102),
        Bool("KilledGM", 103),
        Bool("CollectedSoulFromNostro", 104),
        Bool("WhisperKilledByHero", 116),
        Bool("ArenaFinished", 117),
        Bool("GatesRequireClosing", 118),
        Bool("GatesRequireOpening", 119),
        Bool("HangingTreeBanditKilled", 292),
        Bool("HangingTreeGuardKilled", 293),
        Int("ArcheryHighScore", 68),
        Int("OrchardFarmRaidLastCompleted", 88),
        Int("OrchardFarmTraderEscortCounter", 92),
        Int("DeliveredSoul", 108),
        Int("HighestSkillScore", 168),
        Int("GlobalMeleeGrade", 180),
        Int("GlobalSkillGrade", 184),
        Int("GlobalWillGrade", 188),
        Int("AmbushTradersKillCount", 204),
        Int("AmbushTradersBanditHireCount", 208),
        Int("PrisonRaceNumber", 232),
        Int("MaxChickenKickingScore", 248),
        Int("JackBossBattleResult", 136),
    ];

    private static PersistSlot Bool(string name, int offset) =>
        new(name, PersistKind.Bool, false, offset, BindBool,
            BindingKind.ProvenGeneric,
            $"00CDC070 004045C0(\"{name}\", this+{offset})");

    private static PersistSlot Int(string name, int offset) =>
        new(name, PersistKind.Int, false, offset, BindInt,
            BindingKind.ProvenGeneric,
            $"00CDC070 00410BE0(\"{name}\", this+{offset})");
}

public readonly record struct PersistSlot(
    string Name,
    PersistKind Kind,
    bool DefaultBool,
    int Offset,
    uint Site,
    BindingKind Binding,
    string Evidence);

public enum PersistKind
{
    Bool,
    Int,
    Float,
    Unread,
}

public readonly record struct PersistValue(PersistKind Kind, bool Bool, int Int32, float Float32)
{
    public static PersistValue FromBool(bool value) => new(PersistKind.Bool, value, 0, 0f);
    public static PersistValue FromInt(int value) => new(PersistKind.Int, false, value, 0f);
}

/// <summary>
/// S_QNOVI fiber: <c>00A447D0</c> create,
/// persist association <c>00DAADA0</c>.
/// </summary>
public static class ScriptFiberTable
{
    public const uint Create = 0x00A447D0;
    public const uint Update = 0x00A44880;
    public const uint Resume = 0x00A44660;
    public const uint Yield = 0x00A44690;
    public const uint FiberEntry = 0x00A446A0;
    public const int DtOffset = 8;

    public static readonly ScriptFiberBind[] Recovered =
    [
        new(RegionTravel.IntroScriptName, NewGameScript.PersistAttackOverName,
            BindingKind.ProvenGeneric,
            "S_QNOVI fiber 00A446A0 persist AttackOver; not an Oakvale string in StartNewGame"),
    ];
}

public readonly record struct ScriptFiberBind(
    string Name,
    string PersistField,
    BindingKind Kind,
    string Evidence);
