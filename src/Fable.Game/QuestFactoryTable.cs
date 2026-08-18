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
