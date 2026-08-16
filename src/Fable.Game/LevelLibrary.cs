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

    public GameInstall Install { get; }
    public WorldFile World { get; }

    public LevelLibrary(GameInstall install)
    {
        Install = install;
        World = WorldFile.Load(install.WorldPath);
        _wad = File.Exists(install.WadPath) ? BbbArchive.Open(install.WadPath) : null;
        _stb = File.Exists(install.RuntimeStbPath) ? StbArchive.Open(install.RuntimeStbPath) : null;
    }

    public ThingFile LoadThings(string region)
    {
        var map = World.FindMap(region) ?? throw new FileNotFoundException($"Region '{region}' is not in FinalAlbion.wld.");
        var stem = map.FileStem;

        var loose = Path.Combine(Install.LooseLevelsDirectory, stem + ".tng");
        if (File.Exists(loose))
            return ThingFile.Load(loose);

        if (_wad is not null)
        {
            var entry = _wad.Find(stem + ".tng")
                        ?? _wad.Find(map.LevelName.Replace(".lev", ".tng", StringComparison.OrdinalIgnoreCase));
            if (entry is not null)
                return ThingFile.Parse(Encoding.ASCII.GetString(_wad.Read(entry)));
        }

        throw new FileNotFoundException(
            $"No TNG for '{region}'. Expected loose file '{loose}' or an entry in FinalAlbion.wad.");
    }

    public IReadOnlyList<BankEntry> WadEntries => _wad?.Entries ?? [];

    public LevFile? LoadCompiledLev(string region)
    {
        var entry = _wad?.Find(region + ".lev");
        return entry is null ? null : LevFile.Parse(_wad!.Read(entry));
    }

    public LevHeightField? LoadHeightField(string region)
    {
        if (_stb is null)
            return null;
        var entry = _stb.FindLev(region);
        if (entry is null)
            return null;
        var map = World.FindMap(region);
        if (map is null)
            return null;
        var compiled = _wad?.Find(region + ".lev");
        var width = 128;
        var height = 128;
        if (compiled is not null)
        {
            var wadLev = LevFile.Parse(_wad!.Read(compiled));
            width = wadLev.GridWidth;
            height = wadLev.GridHeight;
        }

        return LevHeightField.Parse(_stb.Read(entry), map.MapX, map.MapY, width, height);
    }

    public void Dispose()
    {
        _wad?.Dispose();
        _stb?.Dispose();
    }
}
