using System.Text;
using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Levels;
using Fable.Formats.Tng;
using Fable.Formats.Wld;

namespace Fable.Game;

/// <summary>
/// Resolves region TNG files from loose Anniversary extracts or the TLC WAD.
/// </summary>
public sealed class LevelLibrary : IDisposable
{
    private readonly BbbArchive? _wad;
    private readonly StbArchive? _stb;
    private readonly Dictionary<string, ThingFile?> _things =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, LevFile?> _levs =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, LevHeightField?> _heights =
        new(StringComparer.OrdinalIgnoreCase);

    public GameInstall Install { get; }
    public WorldFile World { get; }

    public LevelLibrary(GameInstall install)
    {
        Install = install;
        World = WorldFile.Load(install.WorldPath);
        _wad = File.Exists(install.WadPath) ? BbbArchive.Open(install.WadPath) : null;
        _stb = File.Exists(install.RuntimeStbPath) ? StbArchive.Open(install.RuntimeStbPath) : null;
    }

    public ThingFile LoadThings(string region) =>
        TryLoadThings(region) ?? throw new FileNotFoundException(
            $"No TNG for '{region}'. Expected a loose file under '{LooseHint(region)}' or an entry in FinalAlbion.wad.");

    public ThingFile? TryLoadThings(string region)
    {
        if (_things.TryGetValue(region, out var cached))
            return cached;
        var map = World.FindMap(region);
        if (map is null)
        {
            _things[region] = null;
            return null;
        }

        var stem = map.FileStem;
        ThingFile? file = null;
        var loose = Path.Combine(Install.LooseLevelsDirectory, stem + ".tng");
        if (File.Exists(loose))
            file = ThingFile.Load(loose);
        else if (_wad is not null)
        {
            var entry = _wad.Find(stem + ".tng")
                        ?? _wad.Find(map.LevelName.Replace(".lev", ".tng", StringComparison.OrdinalIgnoreCase));
            if (entry is not null)
                file = ThingFile.Parse(Encoding.ASCII.GetString(_wad.Read(entry)));
        }

        _things[region] = file;
        return file;
    }

    private string LooseHint(string region)
    {
        var map = World.FindMap(region);
        var stem = map?.FileStem ?? region;
        return Path.Combine(Install.LooseLevelsDirectory, stem + ".tng");
    }

    public IReadOnlyList<BankEntry> WadEntries => _wad?.Entries ?? [];

    public LevFile? LoadCompiledLev(string region)
    {
        if (_levs.TryGetValue(region, out var cached))
            return cached;
        var map = World.FindMap(region);
        var stem = map?.FileStem ?? region;
        var entry = _wad?.Find(stem + ".lev")
                    ?? _wad?.Find(region + ".lev")
                    ?? (map is null ? null : _wad?.Find(map.LevelName));
        var parsed = entry is null ? null : LevFile.Parse(_wad!.Read(entry));
        _levs[region] = parsed;
        return parsed;
    }

    public LevHeightField? LoadHeightField(string region)
    {
        if (_heights.TryGetValue(region, out var cached))
            return cached;
        if (_stb is null)
        {
            _heights[region] = null;
            return null;
        }

        var entry = _stb.FindLev(region);
        var map = World.FindMap(region);
        if (entry is null || map is null)
        {
            _heights[region] = null;
            return null;
        }

        var compiled = LoadCompiledLev(region);
        var width = compiled?.GridWidth ?? 128;
        var height = compiled?.GridHeight ?? 128;
        var field = LevHeightField.Parse(_stb.Read(entry), map.MapX, map.MapY, width, height);
        _heights[region] = field;
        return field;
    }

    public void Dispose()
    {
        _things.Clear();
        _levs.Clear();
        _heights.Clear();
        _wad?.Dispose();
        _stb?.Dispose();
    }
}
