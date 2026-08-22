using System.Numerics;
using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Textures;

namespace Fable.Game;

/// <summary>
/// Decodes textures.big on demand. Mesh DiffuseMapID is the bank entry id.
/// </summary>
public sealed class TextureLibrary : IDisposable
{
    public const int LandscapeGrassPlainId = 414;

    private readonly BigArchive _big;
    private readonly Dictionary<uint, BankEntry> _byId;
    private readonly Dictionary<uint, TextureFile> _cache = new();

    public int DecodedCount => _cache.Count;
    public int EntryCount => _byId.Count;

    /// <summary>Bank-directory lookup without decoding or retaining RGBA.</summary>
    public bool Contains(int id) => id > 0 && _byId.ContainsKey((uint)id);

    public TextureLibrary(GameInstall install)
    {
        var path = Path.Combine(install.DataRoot, "graphics", "pc", "textures.big");
        _big = BigArchive.Open(path);
        var bank = _big.SubBanks.First(item => item.Name == "GBANK_MAIN_PC");
        _byId = _big.ReadEntries(bank)
            .GroupBy(entry => entry.Id)
            .ToDictionary(group => group.Key, group => group.First());
    }

    public TextureFile? TryLoad(int id)
    {
        if (id <= 0)
            return null;
        var key = (uint)id;
        if (_cache.TryGetValue(key, out var cached))
            return cached;
        if (!_byId.TryGetValue(key, out var entry))
            return null;

        try
        {
            var texture = TextureFile.Parse(entry.Id, entry.Name, entry.Type, entry.Info, _big.Read(entry));
            _cache[key] = texture;
            return texture;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public IReadOnlyList<TextureFile> LoadMany(IEnumerable<int> ids)
    {
        var files = new List<TextureFile>();
        var seen = new HashSet<int>();
        foreach (var id in ids)
        {
            if (!seen.Add(id))
                continue;
            var texture = TryLoad(id);
            if (texture is not null)
                files.Add(texture);
        }

        return files;
    }

    public Vector3 Sample(int id, Vector2 uv)
    {
        var texture = TryLoad(id);
        if (texture is null || texture.Width <= 0 || texture.Height <= 0)
            return new Vector3(0.45f, 0.50f, 0.38f);

        var u = uv.X - MathF.Floor(uv.X);
        var v = uv.Y - MathF.Floor(uv.Y);
        var x = Math.Clamp((int)(u * texture.Width), 0, texture.Width - 1);
        var y = Math.Clamp((int)(v * texture.Height), 0, texture.Height - 1);
        var offset = (y * texture.Width + x) * 4;
        return new Vector3(
            texture.Rgba[offset] / 255f,
            texture.Rgba[offset + 1] / 255f,
            texture.Rgba[offset + 2] / 255f);
    }

    public void Dispose() => _big.Dispose();
}
