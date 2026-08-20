namespace Fable.Game;

/// <summary>
/// Quest-manager name table filled by
/// <c>00CD52D0</c> ("Registering Master
/// Script" / "Registering Important
/// Scripts") via <c>00CB5C90</c>.
/// <c>00CB5AD0</c> looks up
/// <c>[manager+120]</c> and returns the
/// factory record or null.
/// <c>004B4260</c> then
/// <c>004BB720</c> / <c>004B3CE0</c>
/// constructs the factory and run
/// objects. Not TNG <see cref="ScriptFactoryTable"/>.
/// </summary>
public static class QuestFactoryTable
{
    public const uint Register = 0x00CD52D0;
    public const uint Bind = 0x00CB5C90;
    public const uint Lookup = 0x00CB5AD0;
    public const uint Search = 0x00CB65D0;
    public const uint Collect = 0x004BB720;
    public const uint StartWalk = 0x004B3CE0;
    public const uint SharedRun = 0x00CDBD20;
    public const uint SharedRunVtbl = 0x012C2748;
    public const int SharedRunSize = 0x144;
    public const uint SunnyvaleFactory = 0x00CDD550;
    public const uint SunnyvaleInit = 0x00CDBA10;
    public const uint SunnyvaleVtbl = 0x012C2F64;
    public const int SunnyvaleSize = 72;
    public const uint HeroBoastsFactory = 0x00CE6C40;
    public const uint HeroBoastsVtbl = 0x012C3688;
    public const uint HeroBoastsMain = 0x00CE1A30;
    public const int HeroBoastsSize = 80;
    public const uint PersonalMainFactory = 0x00CDE2F0;
    public const uint PersonalGlobalFactory = 0x00CE19A0;
    public const uint HeroDollsFactory = 0x00E98640;
    public const uint HeroDollsVtbl = 0x012ECE7C;
    public const int HeroDollsSize = 76;
    public const uint PlayCutsceneFactory = 0x00F01760;
    public const uint PlayCutsceneVtbl = 0x012F72D0;
    public const int PlayCutsceneSize = 72;
    public const uint GameflowFactory = 0x00CEF950;
    public const uint GameflowVtbl = 0x012C3FA4;
    public const uint GameflowMain = 0x00CE75B0;
    public const uint GameflowSeed = 0x00CE6CF0;
    public const int GameflowSize = 100;
    public const string GameflowScript = "S_GF";
    public const uint GameflowConstructHook = 0x00CB7900;
    public const uint GameflowWatcherCtor = 0x00CDD450;
    public const uint GameflowWatcherAttach = 0x00CB7E50;
    public const uint SharedRunReuse = 0x004AFA10;
    /// <summary>
    /// <c>00CD9A12</c> bind: empty
    /// script <c>0x122D70E</c>, factory
    /// <c>00EE90A0</c>, run <c>ebx</c>
    /// (not <c>00CDBD20</c>), persist
    /// <c>bl</c>.
    /// </summary>
    public const uint WatchForHeroDeathFactory = 0x00EE90A0;
    public const uint WatchForHeroDeathBind = 0x00CD9A12;
    public const string WatchForHeroDeathName = "Global_WatchForHeroDeath";
    public const uint ScriptStateLookup = 0x008A9DB0;
    public const uint ScriptStateInsert = 0x008AE660;
    public const uint ScriptStateMapVa = 0x013BAE44;
    public static readonly string[] GameflowStateNames =
    [
        "OV_INTRO",
        "GUILD_TRAINING",
        "WASP_BOSS",
        "DOING_WASP_BOSS",
        "VISIT_MAZE_1_GLOBAL",
        "VISIT_MAZE_1_BSSLUMS",
        "PRE_ORCH_FARM",
        "DOING_ORCH_FARM",
        "TRADER_ESCORT_GLOBAL",
        "TRADER_ESCORT_BSSLUMS",
        "VISIT_MAZE_2_GLOBAL",
        "VISIT_MAZE_2_BSSLUMS",
        "DOING_BANDIT_CAMP_GLOBAL",
        "DOING_BANDIT_CAMP_BANDITCAMP",
        "VISIT_MAZE_3_GLOBAL",
        "VISIT_MAZE_3_GUILD",
        "FIND_ARCHAEOLOGIST",
        "PRE_WHITE_BALV_GLOBAL",
        "PRE_WHITE_BALV_WITCHWOOD",
        "DOING_WHITE_BALV_GLOBAL",
        "DOING_WHITE_BALV_KHG",
        "PRE_ARENA_GLOBAL",
        "PRE_ARENA_KHG",
        "WHISPERS_FATE",
        "PRE_MCC_GLOBAL",
        "PRE_MCC_BSTONES",
        "DOING_MCC_GLOBAL",
        "DOING_GRAVEYARD_GLOBAL",
        "DOING_GRAVEYARD_GRAVEYARD",
        "HOOK_COAST_GATEWAY_GLOBAL",
        "HOOK_COAST_GATEWAY_DARKWOOD",
        "IN_HOOK_COAST_POST_DRAGON_PRE_BATTLE_GLOBAL",
        "IN_HOOK_COAST_POST_DRAGON_PRE_BATTLE_HOOKCOAST",
        "AFTER_WIZARD_BATTLE_GLOBAL",
        "DOING_FOCAL_SITES_GLOBAL",
        "DOING_FOCAL_SITES_GUILD",
        "FINAL_BATTLE_GLOBAL",
        "FINAL_BATTLE_GUILD",
        "AFTER_FINAL_BATTLE_GLOBAL",
        "AFTER_FINAL_BATTLE_KILLED_SISTER_GLOBAL",
        "AFTER_FINAL_BATTLE_SPARED_SISTER_GLOBAL",
        "AFTER_FINAL_BATTLE_GOT_SWORD_GLOBAL",
        "AFTER_FINAL_BATTLE_DIDNT_GET_SWORD_GLOBAL",
        "LOOKOUT_POINT_DEMON_DOOR_READY",
        "SUMMON_THE_SHIP",
        "NORTHERN_WASTES_OPEN",
        "SCARY_NECROPOLIS",
        "NECROPOLIS_FINISHED",
        "THUNDER_KILLED",
        "BRIAR_ROSE_KILLED",
        "GUILDMASTER_KILLED",
        "NONE_KILLED",
        "DRAGON_GATE_OPEN",
        "SNOWSPIRE_ARRIVAL",
    ];
    public const string MasterLike = "_LIKE";
    public const string MasterHate = "_HATE";
    public const uint MasterLikeVa = 0x0143E938;
    public const uint MasterHateVa = 0x0143E93C;

    public static readonly QuestNameFactory[] Recovered =
    [
        new("Q_SunnyvaleMaster", null,
            SunnyvaleFactory, SharedRun, SunnyvaleInit,
            SunnyvaleVtbl, SunnyvaleSize, true,
            BindingKind.ProvenGeneric,
            "00CD52D0 push Q_SunnyvaleMaster; 00CB5C90 " +
            "factory 00CDD550 run 00CDBD20 persist 1; " +
            "004B3CE0 run.vtbl+8 00CDBA10 zeros + _LIKE/_HATE; " +
            "no CCutsceneDef"),
        new("HeroBoasts", "S_HB",
            HeroBoastsFactory, SharedRun, HeroBoastsMain,
            HeroBoastsVtbl, HeroBoastsSize, false,
            BindingKind.ProvenGeneric,
            "00CD52D0 HeroBoasts / S_HB; factory 00CE6C40 " +
            "size 80 vtbl 012C3688; vtbl+4 00CE1A30 Main " +
            "watcher 00CDD450; fiber 00A447D0"),
        new("PersonalScriptMain", "S_PSM",
            PersonalMainFactory, SharedRun, 0,
            0, 0, false,
            BindingKind.ProvenGeneric,
            "00CD52D0 PersonalScriptMain / S_PSM; " +
            "factory 00CDE2F0 run 00CDBD20"),
        new("PersonalScript_GlobalThings", "S_PSGT",
            PersonalGlobalFactory, SharedRun, 0,
            0, 0, false,
            BindingKind.ProvenGeneric,
            "00CD52D0 PersonalScript_GlobalThings / S_PSGT; " +
            "factory 00CE19A0 run 00CDBD20"),
        new("V_HeroDolls", "S_VHDS",
            HeroDollsFactory, SharedRun, 0,
            HeroDollsVtbl, HeroDollsSize, false,
            BindingKind.ProvenGeneric,
            "00CD52D0 V_HeroDolls / S_VHDS; factory " +
            "00E98640 size 76 vtbl 012ECE7C"),
        new("CS_PlayCutscene", null,
            PlayCutsceneFactory, SharedRun, 0,
            PlayCutsceneVtbl, PlayCutsceneSize, false,
            BindingKind.ProvenGeneric,
            "00CD52D0 CS_PlayCutscene empty factory " +
            "00F01760 size 72 vtbl 012F72D0; no CCutsceneDef"),
        new("Gameflow", GameflowScript,
            GameflowFactory, SharedRun, GameflowMain,
            GameflowVtbl, GameflowSize, false,
            BindingKind.ProvenGeneric,
            "00CD52D0 Gameflow / S_GF; factory 00CEF950 " +
            "size 100 vtbl 012C3FA4; flag 0 → 004AFA10 " +
            "reuse 00CDBD20; 00CB7900 vtbl+12 00CE6CF0 " +
            "then vtbl+4 00CE75B0 Main 00CDD450/00CB7E50"),
        new(WatchForHeroDeathName, null,
            WatchForHeroDeathFactory, 0, 0,
            0, 0, false,
            BindingKind.ProvenGeneric,
            "00CD9A12 Global_WatchForHeroDeath empty " +
            "0x122D70E; factory 00EE90A0 run ebx not " +
            "00CDBD20 persist bl; 00CB5C90"),
        new(RegionTravel.IntroQuest, RegionTravel.IntroScriptName,
            RegionTravel.IntroQuestFactory, RegionTravel.IntroQuestRun,
            RegionTravel.IntroQuestMainWatcher,
            RegionTravel.IntroQuestVtbl, RegionTravel.IntroQuestSize, false,
            BindingKind.ProvenGeneric,
            "00CD6E27 00CB5C90 Q_NewOakValeIntro / S_QNOVI " +
            "factory 00DBEF70 ctor 00DAAC00 size 0x10C vtbl " +
            "012D7A28; slot 1 00DAACE0 Main; slot 2 00DABAC0 " +
            "E8 00DBDE40; not 00CD52D0; no-save 004B4260 " +
            "does not construct"),
    ];

    public static QuestNameFactory? Find(string questName)
    {
        foreach (var factory in Recovered)
        {
            if (factory.QuestName.Equals(questName, StringComparison.OrdinalIgnoreCase))
                return factory;
        }

        return null;
    }
}

public readonly record struct QuestNameFactory(
    string QuestName,
    string? ScriptName,
    uint Factory,
    uint Run,
    uint Init,
    uint ObjectVtbl,
    int ObjectSize,
    bool PersistentBind,
    BindingKind Kind,
    string Evidence);
