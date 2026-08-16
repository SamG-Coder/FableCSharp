namespace Fable.Core;

/// <summary>
/// Locates a Fable install. TLC is the simulation source of truth.
/// Anniversary is accepted as a fallback because it still ships FableData.
/// </summary>
public sealed class GameInstall
{
    public const string PathEnvironmentVariable = "FABLE_PATH";

    public static readonly string DefaultTlcPath =
        @"C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters";

    public static readonly string DefaultAnniversaryPath =
        @"C:\Program Files (x86)\Steam\steamapps\common\Fable Anniversary";

    public required string Root { get; init; }
    public required GameEdition Edition { get; init; }
    public required string DataRoot { get; init; }
    public string? CookedPcDirectory { get; init; }

    public string LevelsDirectory => Path.Combine(DataRoot, "Levels");
    public string WorldPath => Path.Combine(LevelsDirectory, "FinalAlbion.wld");
    public string QuestPath => Path.Combine(LevelsDirectory, "FinalAlbion.qst");
    public string WadPath => Path.Combine(LevelsDirectory, "FinalAlbion.wad");
    public string RuntimeStbPath => Path.Combine(LevelsDirectory, "FinalAlbion_RT.stb");
    public string LooseLevelsDirectory => Path.Combine(LevelsDirectory, "FinalAlbion");
    public string BwdPath => Path.Combine(LevelsDirectory, "FinalAlbion.bwd");
    public string GtgPath => Path.Combine(LevelsDirectory, "FinalAlbion.gtg");
    public string BonesDirectory => Path.Combine(DataRoot, "Bones");
    public string TextBigPath => Path.Combine(DataRoot, "lang", "English", "text.big");

    public static GameInstall Locate() =>
        TryLocate() ?? throw new DirectoryNotFoundException(
            "Could not find Fable. Set FABLE_PATH or install The Lost Chapters.");

    public static GameInstall? TryLocate(string? explicitRoot = null)
    {
        foreach (var candidate in CandidateRoots(explicitRoot))
        {
            if (TryOpen(candidate) is { } install)
                return install;
        }

        return null;
    }

    public static GameInstall? TryOpen(string root)
    {
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            return null;

        var tlcData = Path.Combine(root, "data");
        var tlcWorld = Path.Combine(tlcData, "Levels", "FinalAlbion.wld");
        if (File.Exists(Path.Combine(root, "Fable.exe")) && File.Exists(tlcWorld))
        {
            return new GameInstall
            {
                Root = Path.GetFullPath(root),
                Edition = GameEdition.TheLostChapters,
                DataRoot = Path.GetFullPath(tlcData),
            };
        }

        var faData = Path.Combine(root, "WellingtonGame", "FableData", "Build", "Data");
        var faWorld = Path.Combine(faData, "Levels", "FinalAlbion.wld");
        if (File.Exists(faWorld))
        {
            var cooked = Path.Combine(root, "WellingtonGame", "CookedPC");
            return new GameInstall
            {
                Root = Path.GetFullPath(root),
                Edition = GameEdition.Anniversary,
                DataRoot = Path.GetFullPath(faData),
                CookedPcDirectory = Directory.Exists(cooked) ? Path.GetFullPath(cooked) : null,
            };
        }

        return null;
    }

    public string? FindCompiledDef(string fileName)
    {
        var development = Path.Combine(DataRoot, "CompiledDefs", "Development", fileName);
        if (File.Exists(development))
            return development;

        var retail = Path.Combine(DataRoot, "CompiledDefs", fileName);
        return File.Exists(retail) ? retail : null;
    }

    public IEnumerable<string> FindBigBanks()
    {
        var roots = new[]
        {
            Path.Combine(DataRoot, "graphics"),
            Path.Combine(DataRoot, "graphics", "pc"),
            Path.Combine(DataRoot, "lang", "English"),
            Path.Combine(DataRoot, "Misc", "pc"),
            Path.Combine(DataRoot, "shaders", "pc"),
        };

        foreach (var dir in roots)
        {
            if (!Directory.Exists(dir))
                continue;

            foreach (var file in Directory.EnumerateFiles(dir, "*.big"))
                yield return file;
            foreach (var file in Directory.EnumerateFiles(dir, "*.bbb"))
                yield return file;
        }
    }

    private static IEnumerable<string> CandidateRoots(string? explicitRoot)
    {
        if (!string.IsNullOrWhiteSpace(explicitRoot))
            yield return explicitRoot;

        var env = Environment.GetEnvironmentVariable(PathEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(env))
            yield return env;

        yield return DefaultTlcPath;
        yield return DefaultAnniversaryPath;

        var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        if (!string.IsNullOrEmpty(pf))
        {
            yield return Path.Combine(pf, "Steam", "steamapps", "common", "Fable The Lost Chapters");
            yield return Path.Combine(pf, "Steam", "steamapps", "common", "Fable Anniversary");
        }
    }
}
