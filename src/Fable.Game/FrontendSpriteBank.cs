using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Textures;

namespace Fable.Game;

/// <summary>
/// <c>graphics/pc/frontend.big</c> bank
/// <c>GBANK_FRONT_END_PC</c>. Press Start
/// title is <c>FRONTEND_TITLE_01/02_SPRITE</c>,
/// forest tiles are <c>FORREST_n_m</c>.
/// </summary>
public sealed class FrontendSpriteBank : IDisposable
{
    public const string BankFile = "frontend.big";
    public const string BankName = "GBANK_FRONT_END_PC";
    public const string TitleLeft = "FRONTEND_TITLE_01_SPRITE";
    public const string TitleRight = "FRONTEND_TITLE_02_SPRITE";
    public const string MousePointer = "MOUSE_POINTER_SPRITE_FE";

    private readonly BigArchive _big;
    private readonly Dictionary<string, BankEntry> _byName;
    private readonly Dictionary<string, TextureFile> _cache = new(StringComparer.OrdinalIgnoreCase);

    public FrontendSpriteBank(GameInstall install)
    {
        var path = Path.Combine(install.DataRoot, "graphics", "pc", BankFile);
        _big = BigArchive.Open(path);
        var bank = _big.SubBanks.First(item => item.Name == BankName);
        _byName = _big.ReadEntries(bank)
            .GroupBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
    }

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

    public static string? BankNameForWidget(string widgetName)
    {
        if (widgetName.Equals("UI_TITLE_01", StringComparison.OrdinalIgnoreCase))
            return TitleLeft;
        if (widgetName.Equals("UI_TITLE_02", StringComparison.OrdinalIgnoreCase))
            return TitleRight;
        if (widgetName.Equals("UI_MOUSE_POINTER", StringComparison.OrdinalIgnoreCase))
            return MousePointer;
        const string bg = "UI_FRONTEND_BG_";
        if (widgetName.StartsWith(bg, StringComparison.OrdinalIgnoreCase))
            return widgetName[bg.Length..];
        return null;
    }

    public void Dispose() => _big.Dispose();
}
