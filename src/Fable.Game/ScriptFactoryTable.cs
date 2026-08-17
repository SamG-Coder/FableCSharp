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

    public static readonly PersistSlot[] Recovered =
    [
        new(NewGameScript.PersistAttackOverName, PersistKind.Bool, false,
            AttackOverOffset, AttackOverWrite,
            BindingKind.ProvenGeneric,
            "00DAADA0 004045C0(\"AttackOver\", this+80); writer UNREAD; first-seen false"),
    ];
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
