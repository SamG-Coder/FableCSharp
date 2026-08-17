using Fable.Core;
using Fable.Formats;
using Fable.Formats.Levels;
using Fable.Formats.World;
using Fable.Game;
using Fable.Game.Scripting;

namespace Fable.Formats.Tests;

public sealed class ScriptRuntimeParityTests
{
    [Fact]
    public void Cutscene_layout_is_persist_vectors_not_ascii_scrape()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var father = bank.Find(RegionTravel.IntroCutscene);
        Assert.NotNull(father);
        Assert.True(father.CommandsLayoutProven);
        Assert.Equal(8, father.Vectors.Count);
        Assert.Equal(father.Commands, father.Vectors[0]);
        var scrape = ScriptBank.ExtractCommands(father.Raw);
        Assert.Contains(scrape, s => s.Equals("FadeOut", StringComparison.Ordinal));
        Assert.DoesNotContain(father.Commands, s => s.Equals("FadeOut", StringComparison.Ordinal));
        Assert.Contains(father.Commands, s => s.StartsWith("FadeOut ", StringComparison.Ordinal));
        Assert.Equal(60, ScriptBank.CommandRuntimeOffset);
        Assert.Equal(0x00CBFB7Du, ScriptCommandMap.Runner);
    }

    [Fact]
    public void Command_map_does_not_mark_unread_or_record_only_as_complete()
    {
        Assert.Equal(CommandStatus.Proven, ScriptCommandMap.StatusOf("FadeOut"));
        Assert.Equal(CommandStatus.Proven, ScriptCommandMap.StatusOf("Teleport"));
        Assert.Equal(CommandStatus.Proven, ScriptCommandMap.StatusOf("UseCamera"));
        Assert.Equal(CommandStatus.Proven, ScriptCommandMap.StatusOf("PlayAVI"));
        Assert.Equal(CommandStatus.Proven, ScriptCommandMap.StatusOf("DoScriptFrame"));
        Assert.Equal(CommandStatus.Proven, ScriptCommandMap.StatusOf("PlayAnimation"));
        Assert.Equal(CommandStatus.Proven, ScriptCommandMap.StatusOf("LookToThing"));
        Assert.Equal(CommandStatus.Proven, ScriptCommandMap.StatusOf("MuteSounds"));
        Assert.Equal(CommandStatus.Unread, ScriptCommandMap.StatusOf("NotARealVerb"));
        Assert.True(ScriptCommandMap.IsImplementedComplete("PlayAnimation"));
        Assert.Equal(ScriptFlow.Continue, ScriptCommand.Classify(ScriptCommand.Parse("SetTime 14")));
        Assert.Equal(ScriptFlow.Yield, ScriptCommand.Classify(ScriptCommand.Parse("NotARealVerb")));
        Assert.Contains(ScriptCommandMap.All, s => s.Status == CommandStatus.Unread);
        Assert.Equal(0x00A447D0u, ScriptFiberTable.Create);
        Assert.Equal(0x00A44880u, ScriptFiberTable.Update);
        Assert.Equal(8, ScriptFiberTable.DtOffset);
    }

    [Fact]
    public void StartNewGame_uses_recovered_factory_table_not_oakvale_literals()
    {
        var src = File.ReadAllText(Path.Combine(
            FindRepoRoot(), "src", "Fable.Game", "ScriptRuntime.cs"));
        var start = src.IndexOf("public static ScriptRuntime StartNewGame", StringComparison.Ordinal);
        Assert.True(start >= 0);
        var body = src.Substring(start, 900);
        Assert.Contains("InstallRecoveredBindings", body);
        Assert.DoesNotContain("RegisterNamedScript(RegionTravel.LiveFatherScript", body);
        Assert.DoesNotContain("CreateFiber(RegionTravel.IntroScriptName", body);
        Assert.Equal(BindingKind.ProvenGeneric, ScriptFactoryTable.Recovered[0].Kind);
        Assert.Equal(RegionTravel.LiveFatherScript, ScriptFactoryTable.Recovered[0].ScriptName);
        Assert.Equal(RegionTravel.IntroCutscene, ScriptFactoryTable.Recovered[0].CutsceneName);
        Assert.Equal(PersistKind.Bool, PersistTable.Recovered[0].Kind);
        Assert.False(PersistTable.AttackOverWriterKnown);
    }

    [Fact]
    public void New_game_trace_is_deterministic_and_drives_shipped_runtime()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var things = levels.LoadThings(RegionTravel.NewGameRegion).Things.ToList();
        var a = ScriptRuntime.StartNewGame(install, things);
        var b = ScriptRuntime.StartNewGame(install, things);
        Assert.Equal(a.Trace.Format(), b.Trace.Format());
        Assert.NotEmpty(a.Trace.Steps);
        Assert.Contains(a.Trace.Steps, s => s.Verb == "PlayMusic");
        Assert.Contains(a.Trace.Steps, s => s.Verb == "FadeOut");
        Assert.Contains(a.Trace.Steps, s => s.Verb == "Teleport" && s.World.Contains("Hero"));
        Assert.Contains(a.Trace.Steps, s => s.Verb == "LookToThing" && s.Yielded);
        Assert.Equal(CommandStatus.Proven, a.Trace.Steps.First(s => s.Verb == "LookToThing").Status);
        Assert.Equal(ExecutionKind.YieldOnce, a.Trace.Steps.First(s => s.Verb == "LookToThing").Result);
        Assert.DoesNotContain(a.Trace.Steps, s => s.Status == CommandStatus.Unread && s.Verb == "PlayMusic");
        Assert.Contains(a.Quests, q => q.Name == RegionTravel.IntroScriptName);
        Assert.Equal(PersistKind.Bool, a.PersistType(NewGameScript.PersistAttackOverName));
        Assert.False(a.PersistBool(NewGameScript.PersistAttackOverName));
        Assert.Equal(BindingKind.ProvenGeneric, a.StartNewGameFactoryKind);
        var dest = Path.Combine(FindRepoRoot(), "docs", "runtime", "traces");
        Directory.CreateDirectory(dest);
        a.Trace.Write(Path.Combine(dest, "runtime-trace.txt"));
        File.WriteAllText(Path.Combine(FindRepoRoot(), "docs", "runtime", "COMMAND_MAP.generated.md"),
            ScriptCommandMap.FormatMarkdown());
        Assert.True(File.Exists(Path.Combine(dest, "runtime-trace.txt")));
    }

    [Fact]
    public void First_scene_world_still_shares_space_from_runtime_state()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var scene = FirstSceneWorld.Build(install);
        Assert.True(WorldSpaces.DistanceXy(scene.House, scene.Terrain) < 8f);
        Assert.True(WorldSpaces.DistanceXy(scene.House, scene.Father) < 25f);
        var (_, view, proj) = scene.WorldViewProj();
        var file = new System.Numerics.Vector3(
            scene.TerrainFile.WorldX, scene.TerrainFile.WorldY, scene.TerrainFile.Z);
        var native = WorldSpaces.NativeLandscapeClip(
            file, scene.MapX, scene.MapY, scene.Camera.Position, view, proj);
        var host = WorldSpaces.HostLandscapeClip(file, scene.MapX, scene.MapY, view, proj);
        Assert.True(WorldSpaces.NearlyEqual(native, host, 1e-3f));
        Assert.True(LandscapeFrustum.HostTcamOnWorldSpaceLandscapeIsDisproven);
        Assert.False(LandscapeStrip.FirstSeenRewindsNegativeNz);
        Assert.Equal(20, Fable.Formats.WorldShading.FatherPalskinStrideBytes);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "src", "Fable.Game", "ScriptRuntime.cs")))
                return dir.FullName;
            dir = dir.Parent;
        }

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }
}
