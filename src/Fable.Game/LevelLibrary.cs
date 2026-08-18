using System.Text;
using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Defs;
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
    private readonly Dictionary<string, IReadOnlyList<LandscapeCell>> _cells =
        new(StringComparer.OrdinalIgnoreCase);

    public GameInstall Install { get; }
    public WorldFile World { get; }
    private HeaderEnums? _landscapeEnums;
    private bool _landscapeEnumsLoaded;
    private HeaderEnums? _meshEnums;
    private bool _meshEnumsLoaded;
    private GameBin? _defs;
    private bool _defsLoaded;

    /// <summary>
    /// <c>textures.h</c> once. Native
    /// landscape slots are not a per-triangle
    /// header walk.
    /// </summary>
    public HeaderEnums? LandscapeEnums
    {
        get
        {
            if (_landscapeEnumsLoaded)
                return _landscapeEnums;
            var path = Path.Combine(
                Install.DataRoot, "Defs", "RetailHeaders", "pc", "textures.h");
            _landscapeEnums = File.Exists(path) ? HeaderEnums.Load(path) : null;
            _landscapeEnumsLoaded = true;
            return _landscapeEnums;
        }
    }

    /// <summary>
    /// <c>meshdata.h</c> once. Graphic ids
    /// are not a per-thing header walk.
    /// </summary>
    public HeaderEnums? MeshEnums
    {
        get
        {
            if (_meshEnumsLoaded)
                return _meshEnums;
            var path = Path.Combine(
                Install.DataRoot, "Defs", "RetailHeaders", "meshdata.h");
            _meshEnums = File.Exists(path) ? HeaderEnums.Load(path) : null;
            _meshEnumsLoaded = true;
            return _meshEnums;
        }
    }

    /// <summary>
    /// <c>game.bin</c> + <c>names.bin</c>
    /// process-lifetime. Not a per-map open.
    /// </summary>
    public GameBin? Defs
    {
        get
        {
            if (_defsLoaded)
                return _defs;
            _defs = WorldGeometry.TryLoadDefs(Install);
            _defsLoaded = true;
            return _defs;
        }
    }

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

    /// <summary>
    /// <c>00B3EFA0</c> / STB directory size.
    /// Does not parse materials, tiles, or
    /// the fine height grid.
    /// </summary>
    public StaticMapHeader? PeekMapHeader(string region)
    {
        var map = World.FindMap(region);
        var stem = map?.FileStem ?? region;
        BankEntry? lev = _wad?.Find(stem + ".lev")
                         ?? _wad?.Find(region + ".lev")
                         ?? (map is null ? null : _wad?.Find(map.LevelName));
        LevHeader? header = null;
        if (lev is not null && _wad is not null)
        {
            var prefix = _wad.ReadPrefix(lev, LevFile.NativeHeaderBytes);
            var parsed = LevFile.ReadHeader(prefix);
            header = parsed with { SourceBytes = (int)lev.Size };
        }

        var stb = _stb?.FindLev(region) ?? _stb?.FindLev(stem);
        var stbSize = stb is null ? 0 : (int)stb.Size;
        return new StaticMapHeader(
            region,
            header?.Version ?? 0,
            header?.Constant ?? 0,
            header?.GridWidth ?? 0,
            header?.GridHeight ?? 0,
            header?.SourceBytes ?? 0,
            stbSize,
            LevHeightField.CountSamplesFromSize(stbSize));
    }

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

    /// <summary>
    /// Region-lifetime stored tessellation
    /// (<c>00BDD0E0</c> / <c>00BF4570</c>).
    /// </summary>
    public IReadOnlyList<LandscapeCell> LoadCells(string region)
    {
        if (_cells.TryGetValue(region, out var cached))
            return cached;
        var height = LoadHeightField(region);
        var compiled = LoadCompiledLev(region);
        if (height is null || compiled is null)
        {
            _cells[region] = [];
            return [];
        }

        var grid = LevCellGrid.TryParse(compiled);
        if (grid is null)
        {
            _cells[region] = [];
            return [];
        }

        var built = height.Tiles.ToCells(
            height.OriginX, height.OriginY, grid, compiled.Materials,
            LandscapeEnums, region);
        _cells[region] = built;
        return built;
    }

    /// <summary>
    /// <c>00B3EF40</c> map-slot release:
    /// LEV / STB / stored cells. TNG stays
    /// with the Thing Manager. Banks and the
    /// WAD/STB handles stay process-lifetime
    /// (<c>00B40000</c> does not close
    /// <c>MBANK_ALLMESHES</c>).
    /// </summary>
    public void UnloadMap(string region)
    {
        foreach (var key in Aliases(region))
        {
            _levs.Remove(key);
            _heights.Remove(key);
            _cells.Remove(key);
        }
    }

    public void UnloadThings(string region)
    {
        foreach (var key in Aliases(region))
            _things.Remove(key);
    }

    public bool HasCachedCells(string region) => _cells.ContainsKey(region);

    public bool HasCachedThings(string region) => _things.ContainsKey(region);

    private IEnumerable<string> Aliases(string region)
    {
        yield return region;
        var map = World.FindMap(region);
        if (map is null)
            yield break;
        if (!map.ScriptName.Equals(region, StringComparison.OrdinalIgnoreCase))
            yield return map.ScriptName;
        if (!map.FileStem.Equals(region, StringComparison.OrdinalIgnoreCase))
            yield return map.FileStem;
    }

    public void Dispose()
    {
        _things.Clear();
        _levs.Clear();
        _heights.Clear();
        _cells.Clear();
        _wad?.Dispose();
        _stb?.Dispose();
    }
}

/// <summary>
/// <c>00B3EFA0</c> + STB directory size.
/// Not a parsed LEV or height field.
/// </summary>
public readonly record struct StaticMapHeader(
    string Name,
    int Version,
    uint Constant,
    int GridWidth,
    int GridHeight,
    int CompiledSize,
    int StbSize,
    int HeightSamples);
