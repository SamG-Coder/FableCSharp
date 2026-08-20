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
        Assert.Equal(CommandStatus.Partial, ScriptCommandMap.StatusOf("Teleport"));
        Assert.Equal(CommandStatus.Partial, ScriptCommandMap.StatusOf("UseCamera"));
        Assert.Equal(CommandStatus.Proven, ScriptCommandMap.StatusOf("PlayAVI"));
        Assert.Equal(CommandStatus.Proven, ScriptCommandMap.StatusOf("DoScriptFrame"));
        Assert.Equal(CommandStatus.Partial, ScriptCommandMap.StatusOf("PlayAnimation"));
        Assert.Equal(CommandStatus.Partial, ScriptCommandMap.StatusOf("LookToThing"));
        Assert.Equal(CommandStatus.Partial, ScriptCommandMap.StatusOf("MuteSounds"));
        Assert.Equal(CommandStatus.Unread, ScriptCommandMap.StatusOf("NotARealVerb"));
        Assert.False(ScriptCommandMap.IsImplementedComplete("PlayAnimation"));
        Assert.True(ScriptCommandMap.IsImplementedComplete("PlayAVI"));
        Assert.True(ScriptCommandMap.IsImplementedComplete("CameraPause"));
        Assert.Equal(CommandStatus.Proven, ScriptCommandMap.Find("WalkTo")!.Value.Dispatch);
        Assert.Equal(CommandStatus.Partial, ScriptCommandMap.Find("WalkTo")!.Value.Runtime);
        Assert.Equal(CommandStatus.Proven, ScriptCommandMap.Find("RemoveThing")!.Value.Dispatch);
        Assert.Equal(CommandStatus.Proven, ScriptCommandMap.Find("SetFlag")!.Value.Runtime);
        Assert.Equal(CommandStatus.Proven, ScriptCommandMap.Find("WaitFlag")!.Value.Runtime);
        Assert.True(ScriptCommandMap.IsImplementedComplete("SetFlag"));
        Assert.True(ScriptCommandMap.IsImplementedComplete("WaitFlag"));
        Assert.Equal(0x00CD0116u, ScriptCommandMap.Find("RemoveThing")!.Value.TokenSite);
        Assert.Equal(ScriptFlow.Continue, ScriptCommand.Classify(ScriptCommand.Parse("SetTime 14")));
        Assert.Equal(ScriptFlow.Yield, ScriptCommand.Classify(ScriptCommand.Parse("NotARealVerb")));
        Assert.Contains(ScriptCommandMap.NativeTokens, t => ScriptCommandMap.Find(t.Name) is null);
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
        Assert.Equal("Q_SunnyvaleMaster", QuestFactoryTable.Recovered[0].QuestName);
        Assert.Null(QuestFactoryTable.Recovered[0].ScriptName);
        Assert.Equal("S_HB", QuestFactoryTable.Find("HeroBoasts")!.Value.ScriptName);
        Assert.Equal("S_GF", QuestFactoryTable.Find("Gameflow")!.Value.ScriptName);
        Assert.Equal("S_QNOVI", QuestFactoryTable.Find("Q_NewOakValeIntro")!.Value.ScriptName);
        Assert.Equal(0x00DBEF70u, QuestFactoryTable.Find("Q_NewOakValeIntro")!.Value.Factory);
        Assert.Equal(0x00DABAC0u, QuestFactoryTable.Find("Q_NewOakValeIntro")!.Value.Run);
        Assert.Equal(0x00DAACE0u, QuestFactoryTable.Find("Q_NewOakValeIntro")!.Value.Init);
        Assert.Equal(0x00CEF950u, QuestFactoryTable.GameflowFactory);
        Assert.Equal(0x00CE75B0u, QuestFactoryTable.GameflowMain);
        Assert.Equal(0x00CE6CF0u, QuestFactoryTable.GameflowSeed);
        Assert.Equal("OV_INTRO", QuestFactoryTable.GameflowStateNames[0]);
        Assert.Equal("SNOWSPIRE_ARRIVAL", QuestFactoryTable.GameflowStateNames[^1]);
        Assert.Equal(54, QuestFactoryTable.GameflowStateNames.Length);
        Assert.Equal(PersistKind.Bool, PersistTable.Recovered[0].Kind);
        Assert.True(PersistTable.AttackOverWriterKnown);
        Assert.True(PersistTable.AttackOverWriteIsBind);
        Assert.Equal(0x00DAADA0u, PersistTable.AttackOverWrite);
        Assert.Equal(0x00DBB2A7u, PersistTable.AttackOverStore);
        Assert.Equal(0x00DBB2A7u, RegionTravel.AttackOverStore);
        Assert.False(RegionTravel.FirstSeenAttackOverStoreRuns);
        Assert.False(RegionTravel.RaidAviIsBanditRaid);
        Assert.True(RegionTravel.AttackOverStoreAfterRaidAvi);
        Assert.Equal(16, ScriptFactoryTable.Recovered.Length);
        Assert.True(ScriptFactoryTable.DabacoRegistersBeforeSetup);
        Assert.False(ScriptFactoryTable.IntroQuestTngHasNoviNames);
        Assert.True(ScriptFactoryTable.PreAttackTngHoldsLivingNpcs);
        Assert.False(ScriptFactoryTable.PumpRunsDabaco);
        Assert.False(ScriptFactoryTable.NoviBarrelStartIsWatchBarrels);
        Assert.False(ScriptFactoryTable.NoviBullyOnWestTngFirstSeen);
        Assert.False(ScriptFactoryTable.NoviVictimOnWestTngFirstSeen);
        Assert.False(ScriptFactoryTable.NoviBarrelThugOnWestTngFirstSeen);
        Assert.False(ScriptFactoryTable.NoviCreatedBeetleOnWestTngFirstSeen);
        Assert.False(ScriptFactoryTable.OviDeadFatherOnWestTngFirstSeen);
        Assert.Equal(0x00DAC420u, ScriptFactoryTable.Find("NOVI_Theresa")!.Value.Factory);
        Assert.Equal(0x00DB97A0u, ScriptFactoryTable.Find("NOVI_Theresa")!.Value.Start);
        Assert.Equal(RegionTravel.TheresaCutscene, ScriptFactoryTable.Find("NOVI_Theresa")!.Value.CutsceneName);
        Assert.True(ScriptFactoryTable.Recovered[0].ConstructStartsCutscene);
        Assert.False(ScriptFactoryTable.Find("NOVI_Theresa")!.Value.ConstructStartsCutscene);
        Assert.Equal(0x00DB7D00u, ScriptFactoryTable.Find(RegionTravel.WatchBarrelsThing)!.Value.Factory);
        Assert.Equal(0x00DB7E10u, ScriptFactoryTable.Find(RegionTravel.WatchBarrelsThing)!.Value.Start);
        Assert.Equal(0x00DB81B0u, ScriptFactoryTable.Find("OVI_DeadFather")!.Value.Factory);
        Assert.Equal(0x00DB8300u, ScriptFactoryTable.Find("OVI_DeadFather")!.Value.Start);
        Assert.Equal(RegionTravel.DeadFatherCutscene, ScriptFactoryTable.Find("OVI_DeadFather")!.Value.CutsceneName);
        Assert.False(ScriptFactoryTable.Find("OVI_DeadFather")!.Value.ConstructStartsCutscene);
        Assert.Equal(0x00DAC760u, ScriptFactoryTable.GuardStart);
        Assert.Equal(0x00DBCD60u, ScriptFactoryTable.BullyStart);
        Assert.All(ScriptFactoryTable.Recovered, row => Assert.NotEqual(0u, row.Start));
        Assert.Equal(0x00DB7DB0u, ScriptFactoryTable.BarrelSmashFlagWriter);
        Assert.Equal(20, ScriptFactoryTable.BarrelSmashFlagVtbl);
        Assert.Equal(116, ScriptFactoryTable.BarrelSmashLatchOffset);
        Assert.Equal(0x00CB7950u, ScriptFactoryTable.BarrelSmashCaller);
        Assert.Equal(0x00F35A00u, ScriptFactoryTable.BarrelThingGoneFn);
        Assert.Equal(0x004C9B80u, ScriptFactoryTable.BarrelKillFn);
        Assert.False(ScriptFactoryTable.BarrelStartWritesLatch);
        Assert.Equal("CS_OAKVALE_INTRO_THERESA", RegionTravel.TheresaCutscene);
        Assert.Equal("CS_OAKVALE_INTRO_THERESA_MEET", RegionTravel.TheresaMeetCutscene);
        Assert.Equal("CS_OAKVALE_INTRO_THERESA_MEET_YES", RegionTravel.TheresaMeetYesCutscene);
        Assert.Equal(0x00DB97A0u, RegionTravel.TheresaMeetStart);
        Assert.Equal(0x00DB9B02u, RegionTravel.TheresaMeetSite);
        Assert.Equal(0x00DB9D5Bu, RegionTravel.TheresaMeetYesSite);
        Assert.Equal(0x00DBB21Bu, RegionTravel.TheresaRaidAviSite);
        Assert.Equal(0x00DBB249u, RegionTravel.TheresaRaidPlayAviSite);
        Assert.Equal("CS_DEAD_DAD", RegionTravel.DeadFatherCutscene);
        Assert.Equal(RegionTravel.RaidPlayAvi, "1_raid_on_oak_vale_comp.xmv");
        Assert.Equal(RegionTravel.MazeCutscene, "CS_OAKVALEINTRO_HESDEADJIM");
        Assert.Equal(0x00DBEB20u, RegionTravel.MazeCutsceneStart);
        Assert.Equal(0x00CC8EACu, RegionTravel.MazeCutsceneStop);
        Assert.Equal("PlayMusic MUSIC_SET_NULL,FALSE", RegionTravel.MazeCutsceneLastCommand);
        Assert.Equal(0x00D3BC60u, RegionTravel.GuildTakeFn);
        Assert.False(RegionTravel.MilestoneEntersGuildTake);
        Assert.Equal(RegionTravel.PostAttackQuest, "Q__OakValeIntro_PostAttack");
        var oakvale = QuestFactoryTable.Find(RegionTravel.IntroQuest);
        Assert.NotNull(oakvale);
        Assert.Equal(RegionTravel.IntroScriptName, oakvale.Value.ScriptName);
        Assert.Equal(0x00DBEF70u, oakvale.Value.Factory);
        Assert.Equal(0x00DABAC0u, oakvale.Value.Run);
        Assert.Equal(0x00DAACE0u, oakvale.Value.Init);
    }

    [Fact]
    public void ActivateQuest_Oakvale_binds_S_QNOVI_without_region_or_raid()
    {
        var runtime = ScriptRuntime.Detached();
        var quest = runtime.ActivateQuest(RegionTravel.IntroQuest);
        Assert.Equal(RegionTravel.IntroQuest, quest.Name);
        Assert.Equal(0x00DBEF70u, quest.Factory);
        Assert.Equal(0x00DABAC0u, quest.Run);
        Assert.Equal(RegionTravel.IntroScriptName, quest.ScriptName);
        Assert.False(runtime.PersistBool(NewGameScript.PersistAttackOverName));
        Assert.Equal(RegionTravel.IntroCutscene, runtime.NamedScripts[RegionTravel.LiveFatherScript]);
        Assert.Equal(RegionTravel.TheresaCutscene, runtime.NamedScripts["NOVI_Theresa"]);
        Assert.Empty(runtime.Interpreters);
        Assert.DoesNotContain(runtime.Interpreters, i => i.Name == RegionTravel.TheresaCutscene);
        Assert.DoesNotContain(runtime.Interpreters, i => i.Name == RegionTravel.DeadFatherCutscene);
        Assert.DoesNotContain(runtime.Interpreters, i => i.Name == RegionTravel.IntroCutscene);
        Assert.False(RegionTravel.FirstSeenPlus80WrittenInStartOakVale);
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
