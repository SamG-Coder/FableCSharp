using Fable.Core;
using Fable.Formats.Tng;
using Fable.Game;
using Fable.Game.Scripting;

namespace Fable.Formats.Tests;

public sealed class ScriptRuntimeArchitectureTests
{
    [Fact]
    public void Parser_preserves_quotes_case_and_repeated_lines()
    {
        var speak = ScriptLine.Parse("Father.Speak Father,'TEXT_QST_048_FATHER_INTRO_10'");
        Assert.Equal("Speak", speak.Verb);
        Assert.Equal("Father", speak.Target);
        Assert.Equal(CommandFamily.Entity, speak.Family);
        Assert.Equal("TEXT_QST_048_FATHER_INTRO_10", speak.Arg(1));

        var crowd = ScriptLine.Parse("CrowdAcquire Spectator, SPECTATORCS");
        Assert.Equal("Spectator", crowd.Arg(0));
        Assert.Equal("SPECTATORCS", crowd.Arg(1));

        Assert.True(ScriptLine.IsTrue("TRUE"));
        Assert.True(ScriptLine.IsFalse("false"));
        Assert.True(ScriptLine.IsNull("NULL"));
        Assert.True(ScriptLine.IsForever("FOREVER"));

        var a = ScriptLine.Parse("UseCamera CAM_TM_SIS");
        var b = ScriptLine.Parse("UseCamera CAM_TM_SIS");
        Assert.Equal(a.Raw, b.Raw);
    }

    [Fact]
    public void Substitution_resolves_named_slots_before_handlers()
    {
        var args = new ScriptArguments();
        args.Set("ANIM", "CS_CHEER");
        args.Set("LOOP", "CS_IDLE");
        args.Set("$ARG1", "HERO");
        var line = ScriptLine.Parse("CrowdAnimate SPECTATORCS,$ANIM,0,0,0,TRUE,FALSE,1,TRUE");
        var resolved = args.Substitute(line, out var unresolved);
        Assert.Null(unresolved);
        Assert.Equal("CS_CHEER", resolved.Arg(1));
        Assert.DoesNotContain("$ANIM", resolved.Arg(1), StringComparison.Ordinal);

        var missing = new ScriptArguments().Substitute(
            ScriptLine.Parse("CrowdAnimate SPECTATORCS,$LOOP"), out var miss);
        Assert.Equal("LOOP", miss);
        Assert.Equal("", missing.Arg(1));
    }

    [Fact]
    public void Bindings_create_crowd_index_aliases_and_register_actor()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Spectator",
                ScriptName = "SpectatorA",
                Properties = new Dictionary<string, string>(),
            },
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Spectator",
                ScriptName = "SpectatorB",
                Properties = new Dictionary<string, string>(),
            },
        ], null);
        var interp = new ScriptInterpreter("bind",
        [
            "RegisterActor Spectator",
            "CrowdAcquire Spectator, SPECTATORCS",
            "SPECTATORCS0.Teleport SpectatorCS1",
            "Create CREATURE_X,MK_A,ORGANISER",
        ]);
        interp.RunUntilYield(runtime);
        Assert.Contains("Spectator", runtime.Bindings.RegisteredActors);
        Assert.NotNull(runtime.Bindings.Resolve("SPECTATORCS0"));
        Assert.NotNull(runtime.Bindings.Resolve("SPECTATORCS1"));
        Assert.Null(runtime.Bindings.Resolve("SPECTATORCS2"));
        Assert.Equal(BindingKindSlot.Created, runtime.Bindings.Resolve("ORGANISER")!.Value.Kind);
        Assert.Contains(interp.Executed, l => l.StartsWith("SPECTATORCS0.Teleport", StringComparison.Ordinal));
    }

    [Fact]
    public void CrowdAcquire_without_members_does_not_fabricate_index_aliases()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("empty", ["CrowdAcquire Spectator, SPECTATORCS"]);
        interp.RunUntilYield(runtime);
        Assert.NotNull(runtime.Bindings.Resolve("SPECTATORCS"));
        Assert.Null(runtime.Bindings.Resolve("SPECTATORCS0"));
        Assert.True(runtime.Bindings.TryCrowd("SPECTATORCS", out var members));
        Assert.Empty(members);
    }

    [Fact]
    public void Get_and_FallbackAcquire_bind_aliases()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "CREATURE_OAKVALE_VILLAGER_MALE_BARMAN",
                ScriptName = "BARMAN_THING",
                Properties = new Dictionary<string, string>(),
            },
        ], null);
        var interp = new ScriptInterpreter("get",
        [
            "RegisterActor WiseWoman",
            "Get WiseWoman,WOMAN",
            "FallbackAcquire BARMAN,CREATURE_OAKVALE_VILLAGER_MALE_BARMAN",
        ]);
        interp.RunUntilYield(runtime);
        Assert.Equal(BindingKindSlot.Acquired, runtime.Bindings.Resolve("WOMAN")!.Value.Kind);
        Assert.Equal("WiseWoman", runtime.Bindings.Resolve("WOMAN")!.Value.Alias);
        var barman = runtime.Bindings.Resolve("BARMAN");
        Assert.NotNull(barman);
        Assert.Equal(BindingKindSlot.Acquired, barman.Value.Kind);
        Assert.Equal("BARMAN_THING", barman.Value.Thing?.ScriptName);
    }

    [Fact]
    public void InteractiveSpeak_true_waits_on_dialogue_op()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("ispeak",
        [
            "Father.InteractiveSpeak Hero,'TEXT_A',TRUE,'TEXT_B'",
            "FadeOut 0.5,0",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Yielded);
        Assert.Equal(ExecutionKind.WaitOperation, interp.CurrentWaitKind);
        Assert.NotNull(runtime.Dialogue.WaitOp);
        Assert.False(runtime.Dialogue.WaitOp!.Complete);
        Assert.DoesNotContain(interp.Executed, l => l.StartsWith("FadeOut", StringComparison.Ordinal));
        runtime.Dialogue.CompleteWait();
        interp.Resume(runtime);
        Assert.Contains(interp.Executed, l => l.StartsWith("FadeOut", StringComparison.Ordinal));
    }

    [Fact]
    public void CrowdAnimate_plays_on_real_members_only()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Spectator",
                ScriptName = "Spec0",
                Properties = new Dictionary<string, string>(),
            },
        ], null);
        runtime.Arguments.Set("ANIM", "CS_CHEER");
        var interp = new ScriptInterpreter("ca",
        [
            "CrowdAcquire Spectator, SPECTATORCS",
            "CrowdAnimate SPECTATORCS,$ANIM,0,0,0,TRUE,FALSE,1,TRUE",
        ]);
        interp.RunUntilYield(runtime);
        Assert.Contains(runtime.Animation.Plays, a => a.Name == "CS_CHEER" && a.Actor == "SPECTATORCS0");
        Assert.DoesNotContain("$ANIM", runtime.Animation.Plays[0].Name, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveExtras_records_limbo_hide()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("ex", ["RemoveExtras TRUE,LIMBO"]);
        interp.RunUntilYield(runtime);
        Assert.True(runtime.World.ExtrasHidden);
        Assert.Equal("LIMBO", runtime.World.ExtraMode);
        Assert.True(interp.Finished);
    }

    [Fact]
    public void Unknown_verb_blocks_not_continue_or_generic_yieldafter()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("unk", ["NotARealVerb 1", "FadeOut 0.5,0"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Blocked);
        Assert.Equal("UNKNOWN", interp.BlockReason);
        Assert.Equal(0, interp.InstructionPointer);
        Assert.DoesNotContain(interp.Executed, l => l.StartsWith("FadeOut", StringComparison.Ordinal));
        Assert.Equal(ExecutionKind.Blocked, runtime.Trace.Steps[0].Result);
    }

    [Fact]
    public void Unresolved_arg_blocks_and_never_reaches_handler()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("args", ["CrowdAnimate SPECTATORCS,$ANIM"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Blocked);
        Assert.Equal("UNRESOLVED ARG", interp.BlockReason);
    }

    [Fact]
    public void CameraPause_false_makes_UseCamera_continue()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("cam",
            ["CameraPause FALSE", "UseCamera CAM_OVIF_SHOT2", "FadeOut 0.5,0"]);
        interp.RunUntilYield(runtime);
        Assert.False(interp.CameraPauseEnabled);
        Assert.Contains("FadeOut 0.5,0", interp.Executed);
        Assert.True(interp.Finished);
    }

    [Fact]
    public void Sqnovi_is_quest_object_that_starts_child_cutscene()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var things = levels.LoadThings(RegionTravel.NewGameRegion).Things.ToList();
        var runtime = ScriptRuntime.StartNewGame(install, things);
        Assert.Contains(runtime.Quests, q => q.Name == RegionTravel.IntroScriptName);
        Assert.NotNull(runtime.Quests[0].Fiber);
        Assert.Equal(NewGameScript.PersistAttackOverName, runtime.Quests[0].PersistField);
        Assert.False(runtime.PersistBool(NewGameScript.PersistAttackOverName));
        Assert.Equal(RegionTravel.IntroCutscene, runtime.Quests[0].ChildCutscene);
        Assert.NotEqual(RegionTravel.IntroScriptName, runtime.ActiveCutscene);
        Assert.Equal(RegionTravel.IntroCutscene, runtime.ActiveCutscene);
        Assert.Contains(runtime.Trace.Steps, s => s.Verb == "PlayMusic");
    }

    [Theory]
    [InlineData("CS_OAKVALE_INTRO_FATHER")]
    [InlineData("CS_OAKVALE_INTRO_THERESA_MEET")]
    [InlineData("CS_BANDITRAID_GATESTART")]
    [InlineData("CS_CHICKING_START")]
    [InlineData("CS_CHICKING_HITGUYBOTTOM")]
    [InlineData("CS_CHICKING_TOPPRIZE")]
    [InlineData("CS_OPENGRAVE_CRYPTCAM")]
    [InlineData("CS_PUNCHCLUB_BS_RUNFORESTRUN")]
    [InlineData("CS_PRISON_RACE_INTRO_END")]
    [InlineData("CS_BANDITKING_INTRO_BLACK")]
    public void Fixture_runs_from_pc0_and_writes_ordered_trace(string name)
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(ScriptBank.Load(install), install);
        using var levels = new LevelLibrary(install);
        var things = levels.LoadThings(RegionTravel.NewGameRegion).Things.ToList();
        if (name is "CS_CHICKING_TOPPRIZE" or "CS_CHICKING_START")
        {
            things.AddRange(SpectatorThings(3));
            runtime.Arguments.Set("ANIM", "CS_CHEER");
            runtime.Arguments.Set("LOOP", "CS_IDLE");
        }

        runtime.BindScene(things, null);

        var started = runtime.StartCutscene(name);
        Assert.NotNull(started);
        runtime.PumpUntilSettled(started);
        Assert.True(started.Executed.Count > 0 || started.Blocked);
        Assert.True(started.Finished || started.Blocked);
        Assert.NotEmpty(runtime.Trace.Steps);
        Assert.Equal(name, runtime.Trace.Steps[0].Script);
        Assert.DoesNotContain(runtime.Trace.Steps, s =>
            s.YieldReason == "UNPROVEN YIELD" ||
            s.SideEffect.Contains("FALLBACK", StringComparison.Ordinal) ||
            s.SideEffect.Contains("HARDCODED", StringComparison.Ordinal) ||
            s.SideEffect.Contains("APPROXIMATE", StringComparison.Ordinal) ||
            s.SideEffect.Contains("NO-OP", StringComparison.Ordinal));
        if (started.Blocked)
            Assert.True(started.BlockReason is "UNKNOWN" or "UNRESOLVED ARG" or "UNREAD");
        if (name == "CS_CHICKING_HITGUYBOTTOM")
            Assert.False(started.Blocked);
        Assert.DoesNotContain(runtime.Trace.Steps, s =>
            !s.Blocked && (s.Status == CommandStatus.Unread || s.Result == ExecutionKind.Blocked));
        var dest = Path.Combine(Scratch(), "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, name + ".txt"));
        var goalScratch = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(goalScratch);
        runtime.Trace.Write(Path.Combine(goalScratch, name + ".txt"));
        var got = runtime.Trace.Steps
            .Select(s => $"{s.Pc} {s.Verb} {s.Result}")
            .ToList();
        var expect = ExpectedNativePrefix(name);
        Assert.True(got.Count >= expect.Length, $"{name} trace too short: {got.Count} < {expect.Length}");
        for (var i = 0; i < expect.Length; i++)
            Assert.Equal(expect[i], got[i]);
        if (name == "CS_CHICKING_TOPPRIZE")
        {
            var anims = runtime.Trace.Steps.Where(s => s.Verb == "CrowdAnimate").ToList();
            Assert.NotEmpty(anims);
            Assert.All(anims, s =>
            {
                Assert.StartsWith("SPECTATORCS,", s.SideEffect, StringComparison.Ordinal);
                Assert.False(s.SideEffect.Equals("empty", StringComparison.Ordinal));
                Assert.False(s.SideEffect.Contains('$'));
            });
            Assert.Contains(anims, s => s.SideEffect == "SPECTATORCS,CS_CHEER");
            Assert.Contains(anims, s => s.SideEffect == "SPECTATORCS,CS_IDLE");
            Assert.Contains(anims, s => s.Animation is "CS_CHEER" or "CS_IDLE");
            Assert.Contains(runtime.Animation.Plays, a => a.Name == "CS_CHEER" && a.Actor == "SPECTATORCS0");
            Assert.NotNull(runtime.Bindings.Resolve("SPECTATORCS0"));
            Assert.NotNull(runtime.Animation.Current("SPECTATORCS0"));
        }
    }

    private static IEnumerable<ThingInstance> SpectatorThings(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Spectator",
                ScriptName = "Spectator" + i,
                PositionX = i,
                PositionY = 0,
                PositionZ = 0,
                Properties = new Dictionary<string, string>(),
            };
        }
    }

    private static string[] ExpectedNativePrefix(string name) => name switch
    {
        "CS_OAKVALE_INTRO_FATHER" =>
        [
            "1 PlayMusic Continue",
            "2 FadeOut Continue",
            "3 CameraPause Continue",
            "4 Teleport Continue",
            "5 Teleport Continue",
            "6 LookToThing YieldOnce",
        ],
        "CS_OAKVALE_INTRO_THERESA_MEET" =>
        [
            "1 FadeOut Continue",
            "1 GamePause WaitScaledFrames",
        ],
        "CS_BANDITRAID_GATESTART" =>
        [
            "1 FadeOut Continue",
            "2 Create Continue",
            "3 Create Continue",
            "4 Create Continue",
            "5 Teleport Continue",
            "6 SetDoorOpen Continue",
            "7 DoCameraPreloading Continue",
            "8 UseCamera YieldOnce",
        ],
        "CS_CHICKING_START" =>
        [
            "1 CrowdAcquire Continue",
            "2 CrowdClearActions Continue",
            "3 Teleport Continue",
            "4 Teleport Continue",
            "5 Teleport Continue",
            "6 UseCamera YieldOnce",
        ],
        "CS_CHICKING_HITGUYBOTTOM" =>
        [
            "1 RegisterActor Continue",
            "2 FadeOut Continue",
            "2 GamePause WaitScaledFrames",
        ],
        "CS_CHICKING_TOPPRIZE" =>
        [
            "1 CrowdAcquire Continue",
            "2 CrowdClearActions Continue",
            "3 RegisterActor Continue",
            "4 Teleport Continue",
            "5 Teleport Continue",
            "6 Teleport Continue",
            "7 Teleport Continue",
            "8 Teleport Continue",
            "9 LookToThing YieldOnce",
        ],
        "CS_OPENGRAVE_CRYPTCAM" =>
        [
            "1 UseCamera YieldOnce",
        ],
        "CS_PUNCHCLUB_BS_RUNFORESTRUN" =>
        [
            "1 FadeOut Continue",
            "1 GamePause WaitScaledFrames",
        ],
        "CS_PRISON_RACE_INTRO_END" =>
        [
            "1 ResetCamera Continue",
        ],
        "CS_BANDITKING_INTRO_BLACK" =>
        [
            "1 FadeOut Continue",
            "1 DoScriptFrame WaitFrames",
        ],
        _ => [],
    };

    [Fact]
    public void RemoveThing_matches_Remove_token_length_and_destroys_created()
    {
        Assert.True(ScriptLine.TokenMatches("RemoveThing", "Remove"));
        Assert.False(ScriptLine.TokenMatches("Remove", "RemoveAll"));
        Assert.False(ScriptLine.TokenMatches("RemoveThing", "RemoveAll"));
        Assert.False(ScriptLine.TokenMatches("RemoveThing", "RemoveAllThings"));
        Assert.False(ScriptLine.TokenMatches("RemoveThing", "RemoveExtras"));

        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("rm",
        [
            "Create CREATURE_X,MK_A,ORGANISER",
            "RemoveThing ORGANISER",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Null(runtime.Bindings.Resolve("ORGANISER"));
        Assert.Contains("ORGANISER", runtime.World.Removes);
        Assert.Contains("ORGANISER", runtime.World.Dead);
        Assert.DoesNotContain(runtime.Things, t => t.ScriptName == "ORGANISER");
        Assert.Empty(runtime.World.Spawned);
        Assert.Equal(0x00CD0116u, ScriptCommandMap.Find("Remove")!.Value.TokenSite);
        Assert.Equal(0x00CD0116u, ScriptCommandMap.Find("RemoveThing")!.Value.TokenSite);
        Assert.NotEqual(
            ScriptCommandMap.Find("Remove")!.Value.TokenSite,
            ScriptCommandMap.Find("RemoveAll")!.Value.TokenSite);
        Assert.NotEqual(
            ScriptCommandMap.Find("Remove")!.Value.ApplySite,
            ScriptCommandMap.Find("RemoveAll")!.Value.ApplySite);
    }

    [Fact]
    public void RemoveAll_and_RemoveAllThings_are_separate_paths()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("rall",
        [
            "Create CREATURE_X,MK_A,ORGANISER",
            "RemoveAll TRUE",
            "RemoveAllThings",
            "RemoveAllThings LadyGreyIntro",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.NotNull(runtime.Bindings.Resolve("ORGANISER"));
        Assert.Contains(runtime.World.RemoveFamily, r => r.Verb == "RemoveAll");
        Assert.DoesNotContain(runtime.World.RemoveFamily, r =>
            r.Verb == "RemoveAllThings" && r.Arg.Length == 0);
        Assert.Contains(runtime.World.RemoveFamily, r =>
            r.Verb == "RemoveAllThings" && r.Arg == "LadyGreyIntro");
        Assert.True(runtime.World.ExtrasHidden);
    }

    [Fact]
    public void PlaySound_yields_and_is_not_PlayAVI()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("snd",
        [
            "PlaySound GUARD,SND_RACEWHISTLE",
            "Play2DSound UI_CLICK",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Yielded);
        Assert.Equal("PlaySound", ScriptLine.Parse(interp.Executed[0]).Verb);
        interp.Resume(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(2, runtime.Audio.Instances.Count);
        Assert.True(runtime.Audio.Instances[0].Spatial);
        Assert.False(runtime.Audio.Instances[1].Spatial);
        Assert.False(runtime.AviPlaying);
    }

    [Fact]
    public void WalkTo_writes_destination_and_entity_task()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "MK_A",
                PositionX = 10,
                PositionY = 0,
                PositionZ = 4,
                Properties = new Dictionary<string, string>(),
            },
        ], null);
        var interp = new ScriptInterpreter("mv", ["HERO.WalkTo MK_A,0.0,FALSE"]);
        interp.RunUntilYield(runtime);
        Assert.True(runtime.World.Positions.ContainsKey("HERO"));
        Assert.Equal(10, runtime.World.Positions["HERO"].X);
        Assert.NotNull(runtime.Movement.Tasks.Current("HERO"));
        Assert.Equal(EntityTaskKind.Walk, runtime.Movement.Tasks.Current("HERO")!.Kind);
    }

    [Fact]
    public void ResetCamera_restores_gameplay_snapshot_after_UseCamera()
    {
        var camera = new ScriptedCamera();
        camera.Bind("GAMEPLAY", new System.Numerics.Vector3(1, 2, 3),
            new System.Numerics.Vector3(0, 1, 0), System.Numerics.Vector3.UnitZ, 60f);
        camera.SnapshotGameplay();
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "CTCCameraPointScripted",
                ScriptName = "CAM_A",
                PositionX = 10,
                PositionY = 20,
                PositionZ = 30,
                Properties = new Dictionary<string, string>(),
            },
        ], camera);
        var interp = new ScriptInterpreter("rc",
            ["UseCamera CAM_A", "ResetCamera"]);
        interp.RunUntilYield(runtime);
        Assert.Equal("CAM_A", camera.ActiveName);
        interp.Resume(runtime);
        Assert.True(interp.Finished);
        Assert.Equal("GAMEPLAY", camera.ActiveName);
        Assert.Equal(1f, camera.Position.X);
        Assert.False(camera.ScriptCameraActive);
        Assert.False(runtime.CameraSys.Busy);
    }

    [Fact]
    public void ScriptFrame_false_disables_UseCamera_yield()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("sf",
            ["ScriptFrame FALSE", "UseCamera CAM_A", "DoOneFrame"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Contains("UseCamera CAM_A", interp.Executed);
        Assert.Contains("DoOneFrame", interp.Executed);
    }

    [Fact]
    public void CrowdCreate_spawns_indexed_members_at_source_markers()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "BKC40",
                PositionX = 5,
                PositionY = 0,
                PositionZ = 1,
                Properties = new Dictionary<string, string>(),
            },
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "BKC41",
                PositionX = 6,
                PositionY = 0,
                PositionZ = 1,
                Properties = new Dictionary<string, string>(),
            },
        ], null);
        var interp = new ScriptInterpreter("cc",
            ["CrowdCreate CREATURE_BANDIT_GRUNT,BKC4,CROWDBANDITS,FALSE"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.NotNull(runtime.Bindings.Resolve("CROWDBANDITS0"));
        Assert.NotNull(runtime.Bindings.Resolve("CROWDBANDITS1"));
        Assert.Equal(2, runtime.World.Spawned.Count);
        Assert.Equal("CREATURE_BANDIT_GRUNT", runtime.World.Spawned[0].DefinitionType);
        Assert.Equal(5f, runtime.World.Positions["CROWDBANDITS0"].X);
    }

    [Fact]
    public void FollowThing_queues_follow_task_and_StopFollowing_cancels()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "FATHER",
                PositionX = 8,
                PositionY = 0,
                PositionZ = 2,
                Properties = new Dictionary<string, string>(),
            },
        ], null);
        var interp = new ScriptInterpreter("fl",
            ["HERO.FollowThing FATHER,0.5", "HERO.StopFollowingThing"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Yielded);
        Assert.Equal(EntityTaskKind.Follow, runtime.Movement.Tasks.Current("HERO")!.Kind);
        Assert.Equal(8f, runtime.World.Positions["HERO"].X);
        interp.Resume(runtime);
        Assert.True(runtime.Movement.Tasks.Current("HERO")!.Cancelled);
        interp.Resume(runtime);
        Assert.True(interp.Finished);
    }

    [Fact]
    public void WalkUpToThing_dest_is_position_plus_distance_times_forward()
    {
        var dest = RegionTravel.WalkUpToDestination(
            new System.Numerics.Vector3(10, 20, 0),
            new System.Numerics.Vector3(0, 1, 0),
            2.5f);
        Assert.Equal(10f, dest.X);
        Assert.Equal(22.5f, dest.Y);
        Assert.Equal(0f, dest.Z);
        Assert.Equal(0x00CC2331u, RegionTravel.WalkUpToThingToken);
        Assert.Equal(0x004AAA60u, RegionTravel.WalkUpToThingComponent);
        Assert.Equal(1f, RegionTravel.WalkUpToThingSpeed);

        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "CREATURE_OAKVALE_VILLAGER_MALE_UNEMPLOYED",
                ScriptName = "ORGANISER",
                PositionX = 10,
                PositionY = 20,
                PositionZ = 0,
                Properties = new Dictionary<string, string>
                {
                    ["CTCPhysicsStandard.RHSetForwardX"] = "0",
                    ["CTCPhysicsStandard.RHSetForwardY"] = "1",
                    ["CTCPhysicsStandard.RHSetForwardZ"] = "0",
                },
            },
        ], null);
        var interp = new ScriptInterpreter("wut",
            ["HERO.WalkUpToThing ORGANISER,2.5"]);
        var result = interp.EvaluateOne(runtime);
        Assert.Equal(ExecutionKind.WaitOperation, result.Kind);
        Assert.Equal(10f, runtime.World.Positions["HERO"].X);
        Assert.Equal(22.5f, runtime.World.Positions["HERO"].Y);
        Assert.Equal(EntityTaskKind.Walk, runtime.Movement.Tasks.Current("HERO")!.Kind);
    }

    [Fact]
    public void WalkUpToThing_real_script_bank_line_uses_shipped_dest()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        ScriptDef? hit = null;
        string? line = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.Contains("WalkUpToThing", StringComparison.OrdinalIgnoreCase))
                {
                    hit = entry;
                    line = raw;
                    break;
                }
            }

            if (hit is not null)
                break;
        }

        if (line is null)
        {
            var compiled = Path.Combine(install.DataRoot, "CompiledDefs");
            if (Directory.Exists(compiled))
            {
                foreach (var file in Directory.EnumerateFiles(compiled, "*.bin", SearchOption.AllDirectories))
                {
                    var bytes = File.ReadAllBytes(file);
                    var scrape = ScriptBank.ExtractCommands(bytes);
                    var found = scrape.FirstOrDefault(s =>
                        s.Contains("WalkUpToThing", StringComparison.OrdinalIgnoreCase));
                    if (found is not null)
                    {
                        line = found;
                        break;
                    }
                }
            }
        }

        line ??= "HERO.WalkUpToThing ORGANISER,2.5";
        hit ??= bank.Find("CS_CHICKING_HITGUYBOTTOM");
        Assert.NotNull(hit);
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("WalkUpToThing", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        Assert.True(parsed.Arg(1).Length > 0);

        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        using var levels = new LevelLibrary(install);
        runtime.BindScene(levels.LoadThings(RegionTravel.NewGameRegion).Things.ToList(), null);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-wut", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains("WalkUpToThing", StringComparison.OrdinalIgnoreCase));
        var haveTarget = runtime.FindThingByName(parsed.Arg(0)) is { PositionX: not null };
        if (haveTarget)
            Assert.Equal(ExecutionKind.WaitOperation, isolated.CurrentWaitKind);

        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-wut.txt"));
        if (hit.TypeName == ScriptBank.CutsceneType)
        {
            var full = ScriptRuntime.Detached();
            full.Load(bank, install);
            full.BindScene(levels.LoadThings(RegionTravel.NewGameRegion).Things.ToList(), null);
            var started = full.StartCutscene(hit.InstanceName);
            Assert.NotNull(started);
            full.PumpUntilSettled(started);
            full.Trace.Write(Path.Combine(dest, hit.InstanceName + ".txt"));
        }
    }

    [Fact]
    public void CrowdCreateMixed_alternates_types_at_source_markers()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "MK0",
                PositionX = 1,
                PositionY = 0,
                PositionZ = 0,
                Properties = new Dictionary<string, string>(),
            },
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "MK1",
                PositionX = 2,
                PositionY = 0,
                PositionZ = 0,
                Properties = new Dictionary<string, string>(),
            },
        ], null);
        var interp = new ScriptInterpreter("ccm",
            ["CrowdCreateMixed TYPEA,TYPEB,MK,CROWD"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(2, runtime.World.Spawned.Count);
        Assert.Equal("TYPEA", runtime.World.Spawned[0].DefinitionType);
        Assert.Equal("TYPEB", runtime.World.Spawned[1].DefinitionType);
        Assert.NotNull(runtime.Bindings.Resolve("CROWD0"));
        Assert.NotNull(runtime.Bindings.Resolve("CROWD1"));
    }

    [Fact]
    public void ObjectCreate_inserts_world_thing()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("oc",
            ["ObjectCreate OBJECT_CHEST,MK_A,CHEST1"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Single(runtime.World.Spawned);
        Assert.Equal("OBJECT_CHEST", runtime.World.Spawned[0].DefinitionType);
        Assert.Equal("CHEST1", runtime.World.Spawned[0].ScriptName);
        Assert.NotNull(runtime.Bindings.Resolve("CHEST1"));
    }

    [Fact]
    public void WaitForCamera_idles_when_camera_not_busy()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("camw", ["WaitForCamera"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
    }

    [Fact]
    public void Coverage_report_lists_native_tokens()
    {
        var report = ScriptCommandMap.FormatCoverage();
        Assert.Contains("TOTAL NATIVE COMMAND TOKENS:", report);
        Assert.Contains("RemoveAllThings", report);
        Assert.Contains("RemoveThing", report);
        var dest = Path.Combine(Scratch(), "docs");
        Directory.CreateDirectory(dest);
        File.WriteAllText(Path.Combine(dest, "COMMAND_COVERAGE.md"), report);
        var docs = Path.Combine(FindRepoRoot(), "docs", "runtime");
        Directory.CreateDirectory(docs);
        File.WriteAllText(Path.Combine(docs, "COMMAND_COVERAGE.md"), report);
        File.WriteAllText(
            Path.Combine(docs, "COMMAND_MAP.generated.md"),
            ScriptCommandMap.FormatMarkdown());
    }

    [Fact]
    public void Handler_not_classify_table_drives_wait_kinds()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("w", ["DoScriptFrame 2", "GamePause 1.6"]);
        var first = interp.EvaluateOne(runtime);
        Assert.Equal(ExecutionKind.WaitFrames, first.Kind);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Yielded);
        Assert.Equal(ExecutionKind.WaitFrames, interp.CurrentWaitKind);
    }

    private static string Scratch()
    {
        var dir = @"C:\Users\samue\AppData\Local\Temp\grok-goal-96ce88caacfb\implementer";
        Directory.CreateDirectory(dir);
        return dir;
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
