using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Fonts;

namespace Fable.Game;

/// <summary>
/// <c>lang/English/fonts.big</c> bank
/// <c>FONT_ENGLISH_MAIN</c>. Type-6 UI
/// helper <c>0054F4B0</c> names
/// <c>ENG_ARIAL_16</c>. Init Fonts
/// <c>004168DC</c> names <c>ENG_ARIAL_18</c>.
/// </summary>
public sealed class FontBank : IDisposable
{
    public const string BankFile = "fonts.big";
    public const string BankName = FontFile.MainBank;

    private readonly BigArchive _big;
    private readonly Dictionary<string, BankEntry> _byName;
    private readonly Dictionary<string, FontFile> _cache = new(StringComparer.OrdinalIgnoreCase);

    public FontBank(GameInstall install)
    {
        _big = BigArchive.Open(install.FontsBigPath);
        var bank = _big.SubBanks.First(item => item.Name == BankName);
        _byName = _big.ReadEntries(bank)
            .GroupBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
    }

    public FontFile? TryLoad(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        if (_cache.TryGetValue(name, out var cached))
            return cached;
        if (!_byName.TryGetValue(name, out var entry))
            return null;
        try
        {
            var font = FontFile.Parse(entry.Name, _big.Read(entry));
            _cache[name] = font;
            return font;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public void Dispose() => _big.Dispose();
}