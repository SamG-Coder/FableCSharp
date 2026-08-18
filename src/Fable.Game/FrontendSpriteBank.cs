using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Defs;
using Fable.Formats.Textures;

namespace Fable.Game;

/// <summary>
/// <c>graphics/pc/frontend.big</c> bank
/// <c>GBANK_FRONT_END_PC</c>. Widget →
/// texture is persist <c>GraphicIndex</c>
/// (<c>0x38E36902</c>) as
/// <see cref="BankEntry.Id"/>, not a
/// name map.
/// </summary>
public sealed class FrontendSpriteBank : IDisposable
{
    public const string BankFile = "frontend.big";
    public const string BankName = "GBANK_FRONT_END_PC";
    public const string TitleLeft = "FRONTEND_TITLE_01_SPRITE";
    public const string TitleRight = "FRONTEND_TITLE_02_SPRITE";
    public const string MousePointer = "MOUSE_POINTER_SPRITE_FE";
    public const uint GraphicIndexCrc = FrontendUiDef.GraphicIndexCrc;

    private readonly BigArchive _big;
    private readonly Dictionary<string, BankEntry> _byName;
    private readonly Dictionary<int, BankEntry> _byId;
    private readonly Dictionary<string, string> _byWidget;
    private readonly Dictionary<string, TextureFile> _cache = new(StringComparer.OrdinalIgnoreCase);
    private static Dictionary<string, string>? _persistNames;

    public FrontendSpriteBank(GameInstall install)
    {
        var path = Path.Combine(install.DataRoot, "graphics", "pc", BankFile);
        _big = BigArchive.Open(path);
        var bank = _big.SubBanks.First(item => item.Name == BankName);
        var entries = _big.ReadEntries(bank);
        _byName = entries
            .GroupBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        _byId = entries
            .GroupBy(entry => (int)entry.Id)
            .ToDictionary(group => group.Key, group => group.First());
        _byWidget = BindPersist(install, _byId);
        _persistNames = _byWidget;
    }

    public IReadOnlyDictionary<int, string> NamesById =>
        _byId.ToDictionary(pair => pair.Key, pair => pair.Value.Name);

    public string? TryNameForId(int bankId) =>
        bankId > 0 && _byId.TryGetValue(bankId, out var entry) ? entry.Name : null;

    public TextureFile? TryLoad(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        if (_cache.TryGetValue(name, out var cached))
            return cached;
        if (!_byName.TryGetValue(name, out var entry))
            return null;
        try
        {
            var texture = TextureFile.Parse(entry.Id, entry.Name, entry.Type, entry.Info, _big.Read(entry));
            _cache[name] = texture;
            return texture;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Persist <c>GraphicIndex</c> → bank
    /// name. Missing or id 0 is null
    /// (UNREAD as a graphic).
    /// </summary>
    public static string? BankNameForWidget(string widgetName)
    {
        if (string.IsNullOrEmpty(widgetName))
            return null;
        var map = _persistNames ?? LoadPersistNames();
        _persistNames ??= map;
        return map.TryGetValue(widgetName, out var name) ? name : null;
    }

    public string? BankNameForWidgetInstance(string widgetName) =>
        _byWidget.TryGetValue(widgetName, out var name) ? name : null;

    public string? NameForWidget(string widgetName, int graphicId) =>
        TryNameForId(graphicId) ?? BankNameForWidgetInstance(widgetName);

    private static Dictionary<string, string> LoadPersistNames()
    {
        var install = GameInstall.TryLocate();
        if (install is null)
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var path = Path.Combine(install.DataRoot, "graphics", "pc", BankFile);
        using var big = BigArchive.Open(path);
        var bank = big.SubBanks.First(item => item.Name == BankName);
        var byId = big.ReadEntries(bank)
            .GroupBy(entry => (int)entry.Id)
            .ToDictionary(group => group.Key, group => group.First());
        return BindPersist(install, byId);
    }

    private static Dictionary<string, string> BindPersist(
        GameInstall install, Dictionary<int, BankEntry> byId)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var namesPath = install.FindCompiledDef("names.bin");
        var fePath = install.FindCompiledDef("frontend.bin");
        if (namesPath is null || fePath is null)
            return map;
        var bin = GameBin.Load(fePath, NamesBin.Load(namesPath));
        foreach (var entry in bin.Entries)
        {
            if (entry.TypeName != "UI")
                continue;
            var parsed = FrontendUiDef.TryParse(entry);
            if (parsed is null || parsed.GraphicBankId <= 0)
                continue;
            if (!byId.TryGetValue(parsed.GraphicBankId, out var bank))
                continue;
            var name = parsed.InstanceName;
            if (!string.IsNullOrEmpty(name))
                map[name] = bank.Name;
        }

        return map;
    }

    public void Dispose() => _big.Dispose();
}
