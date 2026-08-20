namespace Fable.Game;

/// <summary>
/// Recovered name-table / factory bindings. Exe
/// <c>00DABAC0</c> registers script names + factories
/// before <c>00DBDE40</c>. Each factory only
/// constructs (release <c>00CDEE00</c>). Start is
/// object <c>vtbl+4</c>. Only Father's first
/// <c>00CBFB7D</c> is <c>CS_OAKVALE_INTRO_FATHER</c>
/// (<c>00DB88F8</c>). Theresa <c>00DB97A0</c> first
/// named work is <c>M_TriggerOutro</c>; first
/// <c>00CBFB7D</c> is MEET at <c>00DB9B28</c>; raid
/// THERESA is <c>00DBB238</c>. DeadFather
/// <c>00DB8300</c> uses <c>007E73F0</c>, not
/// <c>00CBFB7D</c>. The rest wander or watch. TNG
/// <c>ScriptName</c> then <c>004C97B0</c> /
/// <c>00CB8960</c> looks up the name. Not an
/// Oakvale fact inside <c>StartNewGame</c>.
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
    public const uint TheresaFactory = 0x00DAC420;
    public const uint TheresaStart = 0x00DB97A0;
    public const uint TheresaVtbl = 0x012D83A4;
    public const uint GuardFactory = 0x00DAC580;
    public const uint GuardStart = 0x00DAC760;
    public const uint GuardVtbl = 0x012D83C0;
    public const uint VillagerFactory = 0x00DADE50;
    public const uint VillagerStart = 0x00DADF80;
    public const uint VillagerVtbl = 0x012D8678;
    public const uint BullyFactory = 0x00DAEC60;
    public const uint BullyStart = 0x00DBCD60;
    public const uint BullyVtbl = 0x012D879C;
    public const uint VictimFactory = 0x00DAEDE0;
    public const uint VictimStart = 0x00DBB310;
    public const uint VictimVtbl = 0x012D87B8;
    public const uint TeddyGirlFactory = 0x00DAEF50;
    public const uint TeddyGirlStart = 0x00DAF080;
    public const uint TeddyGirlVtbl = 0x012D87D4;
    public const uint AffairManFactory = 0x00DB0880;
    public const uint AffairManStart = 0x00DB09E0;
    public const uint AffairManVtbl = 0x012D89EC;
    public const uint AffairWomanFactory = 0x00DB1DB0;
    public const uint AffairWomanStart = 0x00DB1F00;
    public const uint AffairWomanVtbl = 0x012D8C08;
    public const uint AffairWifeFactory = 0x00DB29A0;
    public const uint AffairWifeStart = 0x00DB2B10;
    public const uint AffairWifeVtbl = 0x012D8C98;
    public const uint BookTraderFactory = 0x00DB3E30;
    public const uint BookTraderStart = 0x00DB3FA0;
    public const uint BookTraderVtbl = 0x012D8E80;
    public const uint BarrelManFactory = 0x00DB51B0;
    public const uint BarrelManStart = 0x00DB5330;
    public const uint BarrelManVtbl = 0x012D9014;
    public const uint BarrelThugFactory = 0x00DB6B40;
    public const uint BarrelThugStart = 0x00DB6C60;
    public const uint BarrelThugVtbl = 0x012D9234;
    public const uint BarrelFactory = 0x00DB7D00;
    public const uint BarrelStart = 0x00DB7E10;
    public const uint BarrelVtbl = 0x012D94F0;
    /// <summary>
    /// <c>NOVI_Barrel</c> <c>vtbl+20</c>
    /// writes quest <c>+116/+117=1</c>
    /// and copies 12 bytes from
    /// <c>[esi+8].vtbl+24</c> to
    /// <c>quest+118</c>. Live caller is
    /// <c>00CB7950</c> when
    /// <c>00F35A00</c> reports the bound
    /// thing gone after kill
    /// <c>004C9B80</c>. Start
    /// <c>00DB7E10</c> does not write
    /// the latch. WatchBarrels only
    /// polls. Do not invent a host
    /// use/physics smash.
    /// </summary>
    public const uint BarrelSmashFlagWriter = 0x00DB7DB0;
    public const int BarrelSmashFlagVtbl = 20;
    public const int BarrelSmashLatchOffset = 116;
    public const uint BarrelSmashCaller = 0x00CB7950;
    public const uint BarrelThingGoneFn = 0x00F35A00;
    public const uint BarrelKillFn = 0x004C9B80;
    public const bool BarrelStartWritesLatch = false;
    /// <summary>
    /// <c>00DABAC0</c> registers all 16
    /// names before <c>00DBDE40</c>.
    /// Starts are thing <c>vtbl+4</c>.
    /// <c>Q_NewOakValeIntro</c> TNG has
    /// no <c>NOVI_*</c>. Living NPCs +
    /// named barrels are PreAttack TNG.
    /// Pump never runs this table.
    /// </summary>
    public const bool DabacoRegistersBeforeSetup = true;
    public const bool IntroQuestTngHasNoviNames = false;
    public const bool PreAttackTngHoldsLivingNpcs = true;
    public const bool PumpRunsDabaco = false;
    public const bool NoviBarrelStartIsWatchBarrels = false;
    public const bool NoviBullyOnWestTngFirstSeen = false;
    public const bool NoviVictimOnWestTngFirstSeen = false;
    public const bool NoviBarrelThugOnWestTngFirstSeen = false;
    public const bool NoviCreatedBeetleOnWestTngFirstSeen = false;
    public const bool OviDeadFatherOnWestTngFirstSeen = false;
    public const uint CreatedBeetleFactory = 0x00DB7FF0;
    public const uint CreatedBeetleStart = 0x00DB80C0;
    public const uint CreatedBeetleVtbl = 0x012D9560;
    public const uint DeadFatherFactory = 0x00DB81B0;
    public const uint DeadFatherStart = 0x00DB8300;
    public const uint DeadFatherVtbl = 0x012D957C;

    public static readonly ScriptNameFactory[] Recovered =
    [
        new(RegionTravel.LiveFatherScript, RegionTravel.IntroCutscene,
            LiveFatherFactory, LiveFatherStart,
            BindingKind.ProvenGeneric,
            "00DABAC0 writes factory 00DAC2C0 at +16 (0x012D8370); " +
            "TNG CREATURE_HERO_FATHER ScriptName NOVI_LiveFather; " +
            "004C97B0 → 00CB8960 → 00DB8520 → 00DAC2C0; " +
            "fiber 00DB8630 [+52].vtbl+4 = 00DB86B0 starts CS_OAKVALE_INTRO_FATHER",
            true),
        Bind("NOVI_Theresa", TheresaFactory, TheresaStart,
            RegionTravel.TheresaCutscene,
            "vtbl+4 00DB97A0 first work M_TriggerOutro; " +
            "00CBFB7D MEET 00DB9B28 then THERESA raid 00DBB238"),
        Bind("NOVI_Guard", GuardFactory, GuardStart),
        Bind("NOVI_Villager", VillagerFactory, VillagerStart),
        Bind("NOVI_Bully", BullyFactory, BullyStart),
        Bind("NOVI_Victim", VictimFactory, VictimStart),
        Bind("NOVI_TeddyGirl", TeddyGirlFactory, TeddyGirlStart),
        Bind("NOVI_AffairMan", AffairManFactory, AffairManStart),
        Bind("NOVI_AffairWoman", AffairWomanFactory, AffairWomanStart),
        Bind("NOVI_AffairWife", AffairWifeFactory, AffairWifeStart),
        Bind("NOVI_BookTrader", BookTraderFactory, BookTraderStart),
        Bind("NOVI_BarrelMan", BarrelManFactory, BarrelManStart),
        Bind("NOVI_BarrelThug", BarrelThugFactory, BarrelThugStart),
        Bind(RegionTravel.WatchBarrelsThing, BarrelFactory, BarrelStart, "",
            "vtbl+4 00DB7E10 WaitForUnderRadius then break-barrels text; " +
            "WatchBarrels 00DBE890 is 00DBDE40 not this start; " +
            "vtbl+20 00DB7DB0 writes quest+116"),
        Bind("NOVI_CreatedBeetle", CreatedBeetleFactory, CreatedBeetleStart),
        Bind("OVI_DeadFather", DeadFatherFactory, DeadFatherStart,
            RegionTravel.DeadFatherCutscene,
            "vtbl+4 00DB8300 first named MK_OVID_DAD then CS_DEAD_DAD via 007E73F0; " +
            "not 00CBFB7D at construct"),
    ];

    static ScriptNameFactory Bind(
        string name, uint factory, uint start, string cutscene = "", string? evidence = null) =>
        new(name, cutscene, factory, start, BindingKind.ProvenGeneric,
            evidence ??
            "00DABAC0 [edi+16] factory before 00CB8230; " +
            $"object vtbl+4 start 0x{start:X8}");

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
    string Evidence,
    bool ConstructStartsCutscene = false);

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
    /// <summary>
    /// <c>00DAADA0</c> is
    /// <c>004045C0("AttackOver", this+80)</c>
    /// bind, seed 0. Not the
    /// <c>00DBB2A7</c> store.
    /// </summary>
    public const bool AttackOverWriteIsBind = true;
    /// <summary>
    /// <c>00DBB2A7</c> <c>mov [ecx+80],1</c>
    /// after <c>CS_OAKVALE_INTRO_THERESA</c>
    /// and PlayAVI <c>1_raid_on_oak_vale_comp.xmv</c>.
    /// Not a store inside <c>00DBDE40</c>.
    /// </summary>
    public const uint AttackOverStore = 0x00DBB2A7;
    public const int AttackOverOffset = 80;
    public const bool AttackOverWriterKnown = true;

    public const uint BindBool = 0x004045C0;
    public const uint BindInt = 0x00410BE0;
    public const uint SunnyvaleBind = 0x00CDC070;

    public static readonly PersistSlot[] Recovered =
    [
        new(NewGameScript.PersistAttackOverName, PersistKind.Bool, false,
            AttackOverOffset, AttackOverWrite,
            BindingKind.ProvenGeneric,
            "00DAADA0 004045C0(\"AttackOver\", this+80); store 00DBB2A7 after raid AVI"),
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
            "00A446A0 watcher +16 is 00DAAD70; persist 00DAADA0 is S_QNOVI vtbl+16 on a different this; FiberCallsPersistThenRun=false"),
    ];
}

public readonly record struct ScriptFiberBind(
    string Name,
    string PersistField,
    BindingKind Kind,
    string Evidence);
