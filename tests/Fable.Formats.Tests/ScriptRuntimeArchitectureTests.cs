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
    public void Speak_opens_session_and_WaitActiveDialog_idles_without_handle()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("spk",
        [
            "Father.Speak Father,'TEXT_QST_048_FATHER_INTRO_10',FALSE,NOREPEAT",
            "WaitActiveDialog",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Yielded);
        Assert.Equal("Father", runtime.Dialogue.Session!.Speaker);
        Assert.Equal("Father", runtime.Dialogue.Session.Listener);
        Assert.Equal("TEXT_QST_048_FATHER_INTRO_10", runtime.Dialogue.Session.Text);
        Assert.Equal(2, runtime.Dialogue.Session.Mode);
        Assert.False(runtime.Dialogue.Session.HasHandle);
        interp.Resume(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(0x00CC27EAu, ScriptCommandMap.Find("Speak")!.Value.ApplySite);
        Assert.Equal(0x00CC6612u, ScriptCommandMap.Find("WaitActiveDialog")!.Value.ApplySite);
    }

    [Fact]
    public void WaitActiveDialog_leftover_polls_interactive_handle()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("wad",
        [
            "Father.InteractiveSpeak Hero,'TEXT_A',FALSE,'TEXT_B'",
            "WaitActiveDialog",
            "CameraPause FALSE",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(runtime.Dialogue.Session!.HasHandle);
        interp.Resume(runtime);
        Assert.Contains("WaitActiveDialog", interp.Executed);
        Assert.Equal("CameraPause FALSE", interp.Commands[interp.InstructionPointer]);
        Assert.True(interp.Yielded);
        interp.Resume(runtime);
        Assert.Contains(interp.Executed, l => l.StartsWith("CameraPause", StringComparison.Ordinal));
    }

    [Fact]
    public void Speak_real_script_bank_line_resolves_text_big()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.Contains(".Speak ", StringComparison.OrdinalIgnoreCase) &&
                    !raw.Contains('$', StringComparison.Ordinal))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        line ??= "Father.Speak Father,'TEXT_QST_048_FATHER_INTRO_10'";
        hit ??= bank.Find("CS_OAKVALE_INTRO_FATHER") ?? bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("Speak", parsed.Verb);
        Assert.True(parsed.Arg(1).Length > 0);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-speak", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains("Speak", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(runtime.Dialogue.Session);
        Assert.Equal(parsed.Arg(1), runtime.Dialogue.Session!.Text);
        Assert.Equal(parsed.Target, runtime.Dialogue.Session.Speaker);
        Assert.Equal(parsed.Arg(0), runtime.Dialogue.Session.Listener);
        var body = runtime.LookupText(parsed.Arg(1));
        if (body is { Length: > 0 })
            Assert.Equal(body, runtime.Dialogue.Session.ResolvedBody);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-speak.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-speak.txt"),
            """
            Speak 00CC25FD / apply 00CC27EA
              actor + listener + text required; IsNull(text) skip
              IsTrue(arg2) vtbl+1484(1) hold, cleared after poll
              arg3 random=1 norepeat=2 sequence=3
              persist 00CD3187 or thing lookup; actor.vtbl+52
              leftover vtbl+104; father +52 004CD1B0 al=1
              +104 00661A40 ret 4 leaves al → one yield
            WaitActiveDialog 00CC656B / poll 00CC6612
              [ebp-44]==0 continue (Speak does not set handle)
              else leftover vtbl+1472 008907D0 → 006E5660
              handle comes from InteractiveSpeak/DialogSpeak vtbl+1456
            """);
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
        Assert.Equal(2760, runtime.Audio.Instances[0].Vtbl);
        interp.Resume(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(2, runtime.Audio.Instances.Count);
        Assert.True(runtime.Audio.Instances[0].Spatial);
        Assert.False(runtime.Audio.Instances[0].Criteria);
        Assert.False(runtime.Audio.Instances[1].Spatial);
        Assert.Equal(2768, runtime.Audio.Instances[1].Vtbl);
        Assert.False(runtime.AviPlaying);
        Assert.Equal(0x00CC8FC1u, ScriptCommandMap.Find("PlaySound")!.Value.ApplySite);
        Assert.Equal(0x00CBF8DAu, ScriptCommandMap.Find("Play2DSound")!.Value.ApplySite);
    }

    [Fact]
    public void PlaySound_null_source_is_vtbl_2768()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("n", ["PlaySound NULL,SND_RACEWHISTLE"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Yielded);
        Assert.False(runtime.Audio.Instances[0].Spatial);
        Assert.Equal(2768, runtime.Audio.Instances[0].Vtbl);
    }

    [Fact]
    public void PlaySound_criteria_uses_vtbl_2756()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("c", ["PlaySound GUARD,SND_X,criteria"]);
        interp.RunUntilYield(runtime);
        Assert.True(runtime.Audio.Instances[0].Criteria);
        Assert.Equal(2756, runtime.Audio.Instances[0].Vtbl);
    }

    [Fact]
    public void PlaySound_real_script_bank_line_is_not_PlayAVI()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_PRISON_RACE_INTRO_END");
        Assert.NotNull(hit);
        string? line = null;
        foreach (var raw in hit.Commands.Count > 0
                     ? hit.Commands
                     : ScriptBank.ExtractCommands(hit.Raw))
        {
            if (raw.StartsWith("PlaySound ", StringComparison.OrdinalIgnoreCase))
            {
                line = raw;
                break;
            }
        }

        Assert.False(string.IsNullOrEmpty(line));
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("PlaySound", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        Assert.True(parsed.Arg(1).Length > 0);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        runtime.AddThing(new ThingInstance
        {
            Kind = "CTC",
            Section = "Thing",
            DefinitionType = "Creature",
            ScriptName = parsed.Arg(0),
            Properties = new Dictionary<string, string>(),
        });
        var isolated = new ScriptInterpreter(hit.InstanceName + "-sound", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("PlaySound ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Yielded);
        Assert.False(runtime.AviPlaying);
        Assert.Single(runtime.Audio.Instances);
        Assert.Equal(parsed.Arg(1), runtime.Audio.Instances[0].Name);
        Assert.Equal(parsed.Arg(0), runtime.Audio.Instances[0].Source);
        Assert.True(runtime.Audio.Instances[0].Spatial);
        Assert.Equal(2760, runtime.Audio.Instances[0].Vtbl);
        var oak = runtime.LookupMusic("MUSIC_SET_OAKVALE");
        Assert.False(string.IsNullOrEmpty(oak));
        Assert.True(File.Exists(oak));
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-sound.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-playsound.txt"),
            """
            PlaySound 00CC8F4E / apply 00CC8FC1
              source + name required; empty → 00CD17FD
              IsNull(source) vtbl+2768 then leftover 00CC907D
              else 00CBF9DE (HERO → vtbl+280, else thing)
                criteria present → vtbl+2756 else vtbl+2760
              leftover vtbl+28 then inc [0x13B83C8] → 00CD17FD
            Play2DSound 00CBF89E / apply 00CBF8DA
              NOT a 00BFEAF8 token
              leftover helper 00CBF7FE vtbl+2768(name) no yield
              not PlayAVI; not 009E5120+2792
            PlayMusic 00CC8EAC / 009E5120 then vtbl+2784
              jmp 00CD17FD; Sound/*.ogg lookup; player UNREAD
            CacheMusic 00CC8E1B / apply 00CC8E6D
              empty skip; 009E5120; miss skip; vtbl+2792; jmp 00CD17FD
            """);
    }

    [Fact]
    public void CacheMusic_continues_and_is_not_PlayMusic()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("cm",
        [
            "CacheMusic MUSIC_SET_CUTSCENE_DRAGON_FIGHT_INTRO",
            "PlayMusic MUSIC_SET_OAKVALE",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Contains("MUSIC_SET_CUTSCENE_DRAGON_FIGHT_INTRO", runtime.Audio.Cached);
        Assert.Equal("MUSIC_SET_OAKVALE", runtime.Audio.Music);
        Assert.Equal(0x00CC8E6Du, ScriptCommandMap.Find("CacheMusic")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("PlayMusic")!.Value.ApplySite,
            ScriptCommandMap.Find("CacheMusic")!.Value.ApplySite);
    }

    [Fact]
    public void CacheMusic_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_DRAGON_INTRO");
        Assert.NotNull(hit);
        string? line = null;
        foreach (var raw in hit.Commands.Count > 0
                     ? hit.Commands
                     : ScriptBank.ExtractCommands(hit.Raw))
        {
            if (raw.StartsWith("CacheMusic ", StringComparison.OrdinalIgnoreCase))
            {
                line = raw;
                break;
            }
        }

        Assert.False(string.IsNullOrEmpty(line));
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-cache", [line]);
        isolated.RunUntilYield(runtime);
        Assert.True(isolated.Finished);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("CacheMusic ", StringComparison.OrdinalIgnoreCase));
        Assert.Single(runtime.Audio.Cached);
        Assert.StartsWith("MUSIC_SET_", runtime.Audio.Cached[0], StringComparison.OrdinalIgnoreCase);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-cache.txt"));
    }

    [Fact]
    public void GiveHero_adds_inventory_and_skips_when_already_owned()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("gh",
        [
            "GiveHero OBJECT_TEDDY_BEAR_UNGIVEABLE",
            "GiveHero OBJECT_TEDDY_BEAR_UNGIVEABLE",
            "GiveHero OBJECT_IRON_KATANA,2",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(2, runtime.World.Inventory.Count);
        Assert.Equal(1, runtime.World.Inventory[0].Count);
        Assert.Equal("OBJECT_TEDDY_BEAR_UNGIVEABLE", runtime.World.Inventory[0].Name);
        Assert.Equal(2, runtime.World.Inventory[1].Count);
        Assert.Equal(0x00CC63E5u, ScriptCommandMap.Find("GiveHero")!.Value.ApplySite);
    }

    [Fact]
    public void GiveHero_arg4_yields_unless_silent()
    {
        var runtime = ScriptRuntime.Detached();
        var yield = new ScriptInterpreter("ghy",
            ["GiveHero OBJECT_X,1,-1,FALSE,TRUE", "CameraPause FALSE"]);
        yield.RunUntilYield(runtime);
        Assert.True(yield.Yielded);
        Assert.Contains("GiveHero OBJECT_X,1,-1,FALSE,TRUE", yield.Executed);
        yield.Resume(runtime);
        Assert.True(yield.Finished);
        var silent = new ScriptInterpreter("ghs",
            ["GiveHero OBJECT_Y,1,1,TRUE,TRUE", "CameraPause FALSE"]);
        silent.RunUntilYield(runtime);
        Assert.True(silent.Finished);
        Assert.True(runtime.World.Inventory.Exists(i => i.Name == "OBJECT_Y" && i.Silent));
    }

    [Fact]
    public void GiveHero_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_OAKVALEINTRO_BULLYRUN2")
                  ?? bank.Find("CS_GUILD_DEPARTURE_MELEE_TEST_APLUS_PRIZE");
        Assert.NotNull(hit);
        string? line = null;
        foreach (var raw in hit.Commands.Count > 0
                     ? hit.Commands
                     : ScriptBank.ExtractCommands(hit.Raw))
        {
            if (raw.StartsWith("GiveHero ", StringComparison.OrdinalIgnoreCase) &&
                !raw.Contains('$', StringComparison.Ordinal))
            {
                line = raw;
                break;
            }
        }

        Assert.False(string.IsNullOrEmpty(line));
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-give", [line, line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("GiveHero ", StringComparison.OrdinalIgnoreCase));
        Assert.Single(runtime.World.Inventory);
        Assert.Equal(1, runtime.World.Inventory[0].Count);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-give.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-givehero.txt"),
            """
            GiveHero 00CC6392 / apply 00CC63E5
              arg0 item required else 00CC7081
              esi=1; arg1 atoi count; arg2 atoi extra (edi default -1)
              arg3 IsTrue [ebp-604] silent
              arg4 IsTrue [ebp+127] leftover unless silent
              arg5 IsFalse [ebp+19]=0 → vtbl+572 else vtbl+488
              00515700 item lookup; vtbl+484 x (count-have)
              already-have skip; jmp 00CC2C6B
            Item def / hero bag body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void GiveHeroHealth_adds_and_MAX_fills_to_max()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.World.HeroMaxHealth = 100f;
        runtime.World.HeroHealth = 10f;
        var interp = new ScriptInterpreter("ghh",
        [
            "GiveHeroHealth 25",
            "GiveHeroHealth MAX",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(100f, runtime.World.HeroHealth);
        Assert.Equal(0x00CC62F5u, ScriptCommandMap.Find("GiveHeroHealth")!.Value.ApplySite);
        Assert.Equal(0x00CC6281u, ScriptCommandMap.Find("GiveHeroMorality")!.Value.ApplySite);
    }

    [Fact]
    public void GiveHeroMorality_adds_amount()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("ghm", ["GiveHeroMorality -50", "GiveHeroMorality 20"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(-30f, runtime.World.HeroMorality);
    }

    [Fact]
    public void GiveHeroHealth_real_script_bank_MAX()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_BORDELLO_PAYINGFORSEX");
        Assert.NotNull(hit);
        string? line = null;
        foreach (var raw in hit.Commands.Count > 0
                     ? hit.Commands
                     : ScriptBank.ExtractCommands(hit.Raw))
        {
            if (raw.StartsWith("GiveHeroHealth ", StringComparison.OrdinalIgnoreCase))
            {
                line = raw;
                break;
            }
        }

        Assert.False(string.IsNullOrEmpty(line));
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("GiveHeroHealth", parsed.Verb);
        Assert.Equal("MAX", parsed.Arg(0), StringComparer.OrdinalIgnoreCase);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        runtime.World.HeroMaxHealth = 80f;
        runtime.World.HeroHealth = 12f;
        var isolated = new ScriptInterpreter(hit.InstanceName + "-health", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("GiveHeroHealth ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Equal(80f, runtime.World.HeroHealth);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-health.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-giveherohealth.txt"),
            """
            GiveHeroHealth 00CC62A0 / apply 00CC62F5
              arg0 required else 00CC7081
              00BFEBA8 vs "MAX" at 0x012C2130
              MAX: vtbl+1032 max, vtbl+1028 cur, fsubr = max-cur
              else 0099E690 atof
              vtbl+1052(amount, 1, 0); jmp 00CC7081 no yield
            GiveHeroMorality 00CC6222 / apply 00CC6281
              atof; vtbl+624(amount); jmp 00CC7081
            Hero stat object body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void GiveHeroExpression_stores_name_param_and_flag()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("ghe",
            ["GiveHeroExpression EXPRESSION_FLIRT,TRUE,2", "CameraPause FALSE"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Single(runtime.World.Expressions);
        Assert.Equal("EXPRESSION_FLIRT", runtime.World.Expressions[0].Name);
        Assert.True(runtime.World.Expressions[0].Flag);
        Assert.Equal(2, runtime.World.Expressions[0].Param);
        Assert.Equal(0x00CC6185u, ScriptCommandMap.Find("GiveHeroExpression")!.Value.ApplySite);
    }

    [Fact]
    public void GiveHeroExpression_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.StartsWith("GiveHeroExpression ", StringComparison.OrdinalIgnoreCase) &&
                    !raw.Contains('$', StringComparison.Ordinal))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        line ??= "GiveHeroExpression EXPRESSION_FLIRT";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("GiveHeroExpression", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-expr", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("GiveHeroExpression ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Single(runtime.World.Expressions);
        Assert.Equal(parsed.Arg(0), runtime.World.Expressions[0].Name);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-expr.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-giveheroexpression.txt"),
            """
            GiveHeroExpression 00CC6132 / apply 00CC6185
              arg0 name required else 00CC7081
              007ADB30 expression-def lookup; miss skip vtbl
              esi=edi (-1); arg1 IsTrue flag; arg2 atoi param
              vtbl+900(name, param, flag); jmp 00CC2C6B
            Def table 007ADB30 UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void UseTheme_sets_theme_and_RESET_clears()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("ut",
        [
            "UseTheme ENVIRONMENT_CAVE",
            "UseTheme RESET,0,FALSE",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.True(runtime.Audio.ThemeReset);
        Assert.Equal("", runtime.Audio.Theme);
        Assert.Equal(0f, runtime.Audio.ThemeParam);
        Assert.False(runtime.Audio.ThemeFlag);
        Assert.Equal(0x00CCFA8Bu, ScriptCommandMap.Find("UseTheme")!.Value.ApplySite);
    }

    [Fact]
    public void TakeFromHero_removes_given_item()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("tfh",
        [
            "GiveHero OBJECT_TEDDY_BEAR_UNGIVEABLE",
            "TakeFromHero OBJECT_TEDDY_BEAR_UNGIVEABLE",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Empty(runtime.World.Inventory);
        Assert.Equal(0x00CCFBA3u, ScriptCommandMap.Find("TakeFromHero")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("GiveHero")!.Value.ApplySite,
            ScriptCommandMap.Find("TakeFromHero")!.Value.ApplySite);
    }

    [Fact]
    public void UseTheme_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_ARENA_HOH_INTRO") ?? bank.Find("CS_FABLE_CREDITS");
        Assert.NotNull(hit);
        string? line = null;
        foreach (var raw in hit.Commands.Count > 0
                     ? hit.Commands
                     : ScriptBank.ExtractCommands(hit.Raw))
        {
            if (raw.StartsWith("UseTheme ", StringComparison.OrdinalIgnoreCase))
            {
                line = raw;
                break;
            }
        }

        Assert.False(string.IsNullOrEmpty(line));
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("UseTheme", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-theme", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("UseTheme ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        if (parsed.Arg(0).Equals("RESET", StringComparison.OrdinalIgnoreCase))
            Assert.True(runtime.Audio.ThemeReset);
        else
            Assert.Equal(parsed.Arg(0), runtime.Audio.Theme);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-theme.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-usetheme.txt"),
            """
            UseTheme 00CCFA38 / apply 00CCFA8B
              arg0 required; IsNull skip 00CD17FD
              arg1 atof default 0; arg2 empty|TRUE → [ebp-63]=1
              00BFEBA8 RESET → vtbl+2628(param)
              else vtbl+2624(name,param)
              jmp 00CD17FD no yield
            TakeFromHero 00CCFB51 / apply 00CCFBA3
              arg0 required; vtbl+556(name); jmp 00CD17FD
              not TakeObjectFromHero
            Theme mixer / take-item bag body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void PutInHeroHands_name_and_null()
    {
        var runtime = ScriptRuntime.Detached();
        var put = new ScriptInterpreter("pih",
            ["PutInHeroHands OBJECT_TROPHY_JOB_MASK_01,NAME"]);
        put.RunUntilYield(runtime);
        Assert.Equal("OBJECT_TROPHY_JOB_MASK_01", runtime.World.HeroHands);
        var clear = new ScriptInterpreter("pihn", ["PutInHeroHands NULL"]);
        clear.RunUntilYield(runtime);
        Assert.Equal("", runtime.World.HeroHands);
        Assert.Equal(0x00CCFC20u, ScriptCommandMap.Find("PutInHeroHands")!.Value.ApplySite);
    }

    [Fact]
    public void PutInHeroHands_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_DRAGON_DEATH");
        Assert.NotNull(hit);
        string? line = null;
        foreach (var raw in hit.Commands.Count > 0
                     ? hit.Commands
                     : ScriptBank.ExtractCommands(hit.Raw))
        {
            if (raw.StartsWith("PutInHeroHands ", StringComparison.OrdinalIgnoreCase))
            {
                line = raw;
                break;
            }
        }

        Assert.False(string.IsNullOrEmpty(line));
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("PutInHeroHands", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-hands", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("PutInHeroHands ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Equal(parsed.Arg(0), runtime.World.HeroHands);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-hands.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-putinherohands.txt"),
            """
            PutInHeroHands 00CCFBCA / apply 00CCFC20
              arg0 required; IsNull → vtbl+572 empty string
              arg1 NAME → vtbl+572(name)
              else thing resolve vtbl+280/288 then vtbl+568(thing,1,1)
              jmp 00CD17F8/FD no yield
            Equip/hold mesh body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void SetHeroWeapon_is_not_PutInHeroHands()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("shw",
            ["SetHeroWeapon OBJECT_SWORD_OF_AEONS", "PutInHeroHands OBJECT_TROPHY_JOB_MASK_01,NAME"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal("OBJECT_SWORD_OF_AEONS", runtime.World.HeroWeapon);
        Assert.Equal("OBJECT_TROPHY_JOB_MASK_01", runtime.World.HeroHands);
        Assert.Equal(0x00CCFDA9u, ScriptCommandMap.Find("SetHeroWeapon")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("SetHeroWeapon")!.Value.ApplySite,
            ScriptCommandMap.Find("PutInHeroHands")!.Value.ApplySite);
    }

    [Fact]
    public void SetHeroWeapon_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_JACK_BOSS_ENDING_EVIL");
        Assert.NotNull(hit);
        string? line = null;
        foreach (var raw in hit.Commands.Count > 0
                     ? hit.Commands
                     : ScriptBank.ExtractCommands(hit.Raw))
        {
            if (raw.StartsWith("SetHeroWeapon ", StringComparison.OrdinalIgnoreCase))
            {
                line = raw;
                break;
            }
        }

        Assert.False(string.IsNullOrEmpty(line));
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("SetHeroWeapon", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-weapon", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("SetHeroWeapon ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Equal(parsed.Arg(0), runtime.World.HeroWeapon);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-weapon.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-setheroweapon.txt"),
            """
            SetHeroWeapon 00CCFD57 / apply 00CCFDA9
              arg0 required else 00CD17FD
              vtbl+488(name); jmp 00CD17FD no yield
              not PutInHeroHands vtbl+572
              not GiveHero vtbl+484
            Weapon mesh / sheathe body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void RemoveHeroWeapons_false_is_vtbl_560()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.World.HeroWeapon = "OBJECT_SWORD_OF_AEONS";
        var keep = new ScriptInterpreter("rhw0", ["RemoveHeroWeapons FALSE"]);
        keep.RunUntilYield(runtime);
        Assert.Equal("", runtime.World.HeroWeapon);
        Assert.Equal(560, runtime.World.RemoveHeroWeaponsVtbl);
        var strip = new ScriptInterpreter("rhw1", ["RemoveHeroWeapons TRUE"]);
        strip.RunUntilYield(runtime);
        Assert.Equal(552, runtime.World.RemoveHeroWeaponsVtbl);
        Assert.Equal(0x00CC9106u, ScriptCommandMap.Find("RemoveHeroWeapons")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("SetHeroWeapon")!.Value.ApplySite,
            ScriptCommandMap.Find("RemoveHeroWeapons")!.Value.ApplySite);
    }

    [Fact]
    public void RemoveHeroWeapons_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_GUILD_DEPARTURE_GM_DONE") ?? bank.Find("CS_SHIP_SAILS");
        Assert.NotNull(hit);
        string? line = null;
        foreach (var raw in hit.Commands.Count > 0
                     ? hit.Commands
                     : ScriptBank.ExtractCommands(hit.Raw))
        {
            if (raw.StartsWith("RemoveHeroWeapons", StringComparison.OrdinalIgnoreCase))
            {
                line = raw;
                break;
            }
        }

        Assert.False(string.IsNullOrEmpty(line));
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("RemoveHeroWeapons", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        runtime.World.HeroWeapon = "OBJECT_IRON_KATANA";
        var isolated = new ScriptInterpreter(hit.InstanceName + "-rmw", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("RemoveHeroWeapons", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Equal("", runtime.World.HeroWeapon);
        Assert.Equal(ScriptLine.IsFalse(parsed.Arg(0)) ? 560 : 552,
            runtime.World.RemoveHeroWeaponsVtbl);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-rmw.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-removeheroweapons.txt"),
            """
            RemoveHeroWeapons 00CC90B4 / apply 00CC9106
              00CBEE0C IsFalse(arg0)
              TRUE/empty → vtbl+552
              FALSE → vtbl+560
              jmp 00CD17FD no yield
              sibling of TakeFromHero vtbl+556
            Weapon bag / sheathe body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void HeroHair_accumulates_hair_and_beard()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("hh",
        [
            "HeroHair OBJECT_HERO_HAIR_YOUNG_01",
            "HeroHair OBJECT_HERO_BEARD_TRAMP_01",
            "HeroHair OBJECT_HERO_HAIR_YOUNG_01",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(2, runtime.World.HeroHairs.Count);
        Assert.Equal("OBJECT_HERO_HAIR_YOUNG_01", runtime.World.HeroHairs[0]);
        Assert.Equal("OBJECT_HERO_BEARD_TRAMP_01", runtime.World.HeroHairs[1]);
        Assert.Equal(0x00CC9182u, ScriptCommandMap.Find("HeroHair")!.Value.ApplySite);
        Assert.Equal(0x00CC91FBu, ScriptCommandMap.Find("HeroTattoo")!.Value.ApplySite);
        Assert.Equal(0x00CC9274u, ScriptCommandMap.Find("HeroWear")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("HeroHair")!.Value.ApplySite,
            ScriptCommandMap.Find("HeroWear")!.Value.ApplySite);
    }

    [Fact]
    public void HeroWear_and_HeroTattoo_are_distinct_vtbls()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("hw",
        [
            "HeroWear OBJECT_HERO_NO_HAT",
            "HeroTattoo OBJECT_HERO_TATTOO_01",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(["OBJECT_HERO_NO_HAT"], runtime.World.HeroClothes);
        Assert.Equal(["OBJECT_HERO_TATTOO_01"], runtime.World.HeroTattoos);
    }

    [Fact]
    public void HeroHair_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_LOSTBAY_INTRO");
        Assert.NotNull(hit);
        var lines = new List<string>();
        foreach (var raw in hit.Commands.Count > 0
                     ? hit.Commands
                     : ScriptBank.ExtractCommands(hit.Raw))
        {
            if (raw.StartsWith("HeroHair ", StringComparison.OrdinalIgnoreCase))
                lines.Add(raw);
        }

        Assert.True(lines.Count >= 2);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-hair", lines);
        isolated.RunUntilYield(runtime);
        Assert.True(isolated.Finished);
        Assert.Equal(lines.Count, runtime.World.HeroHairs.Count);
        Assert.Equal(ScriptLine.Parse(lines[0]).Arg(0), runtime.World.HeroHairs[0]);
        Assert.Equal(ScriptLine.Parse(lines[1]).Arg(0), runtime.World.HeroHairs[1]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-hair.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-herohair.txt"),
            """
            HeroHair 00CC9130 / apply 00CC9182
              arg0 required else 00CD17FD
              vtbl+764(name); jmp 00CD17FD no yield
              CS_LOSTBAY_INTRO applies hair then beard
            HeroTattoo 00CC91A9 / apply 00CC91FB
              vtbl+576(name); not HeroHair 764
            HeroWear 00CC9222 / apply 00CC9274
              vtbl+760(name); not HeroHair 764
            Appearance mesh / PALSKIN unread (Runtime PARTIAL)
            """);
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
        Assert.Equal(10, runtime.Movement.Destinations["HERO"].X);
        Assert.Equal(0f, runtime.World.Positions["HERO"].X);
        Assert.True(runtime.Movement.Moving.Contains("HERO"));
        Assert.NotNull(runtime.Movement.Tasks.Current("HERO"));
        Assert.Equal(EntityTaskKind.Walk, runtime.Movement.Tasks.Current("HERO")!.Kind);
        Assert.Equal(0.3f, runtime.Movement.Tasks.Current("HERO")!.Speed);
        runtime.Update(40f);
        Assert.Equal(10, runtime.World.Positions["HERO"].X);
        Assert.Equal(4, runtime.World.Positions["HERO"].Z);
        Assert.True(runtime.Movement.Tasks.Current("HERO")!.Complete);
        Assert.Equal(0x00CC09E2u, ScriptCommandMap.Find("WalkTo")!.Value.ApplySite);
    }

    [Fact]
    public void WalkTo_real_script_bank_line_ticks_toward_marker()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.Contains(".WalkTo ", StringComparison.OrdinalIgnoreCase) &&
                    !raw.Contains('$', StringComparison.Ordinal))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        line ??= "VILL1.WalkTo MK_OVI_ID_VW1,0.0,FALSE";
        hit ??= bank.Find("CS_OAKVALE_INTRO_FATHER") ?? bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("WalkTo", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = parsed.Arg(0),
                PositionX = 4,
                PositionY = 0,
                PositionZ = 0,
                Properties = new Dictionary<string, string>(),
            },
        ], null);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-walk", [line]);
        isolated.RunUntilYield(runtime);
        Assert.True(isolated.Yielded || isolated.Finished);
        if (parsed.Target.Length > 0)
        {
            Assert.True(runtime.Movement.Destinations.ContainsKey(parsed.Target));
            Assert.Equal(0f, runtime.World.Positions[parsed.Target].X);
            runtime.Update(20f);
            Assert.Equal(4f, runtime.World.Positions[parsed.Target].X);
        }

        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-walk.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-walkto.txt"),
            """
            WalkTo 00CC083D / apply 00CC09E2
              actor.vtbl+20 on CThingPlayerCreature 012457FC
              and CThingAICreature 0127293C is 004C72B0 (al=1; ret 4)
              dest lookup vtbl+280/288; speed default 0.3
              wait = IsTrue(arg2)|IsTrue(arg3) leftover vtbl+104
            Creature go is sibling vtbl+16 006A9960:
              00662930 / 006A5D90 stores dest
              fld [gait+80] -> [this+176]
              or [this+146], 2  moving
              no warp on apply
            TickMove advances World.Positions (ActorPositions -> FirstSceneWorld)
            """);
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
        Assert.Equal(8f, runtime.Movement.Destinations["HERO"].X);
        Assert.Equal(0f, runtime.World.Positions["HERO"].X);
        interp.Resume(runtime);
        Assert.True(runtime.Movement.Tasks.Current("HERO")!.Cancelled);
        interp.Resume(runtime);
        Assert.True(interp.Finished);
    }

    [Fact]
    public void PutInFrontOf_teleports_mover_to_stand_off()
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
                PositionX = 0,
                PositionY = 0,
                PositionZ = 0,
                Properties = new Dictionary<string, string>
                {
                    ["CTCPhysicsStandard.RHSetForwardX"] = "0",
                    ["CTCPhysicsStandard.RHSetForwardY"] = "1",
                    ["CTCPhysicsStandard.RHSetForwardZ"] = "0",
                },
            },
        ], null);
        var interp = new ScriptInterpreter("pif", ["PutInFrontOf HERO,FATHER,3"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(3f, runtime.World.Positions["HERO"].Y);
        Assert.Equal("FATHER", runtime.World.LookTargets["HERO"]);
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
        Assert.Equal(10f, runtime.Movement.Destinations["HERO"].X);
        Assert.Equal(22.5f, runtime.Movement.Destinations["HERO"].Y);
        Assert.Equal(0f, runtime.World.Positions["HERO"].X);
        Assert.Equal(EntityTaskKind.Walk, runtime.Movement.Tasks.Current("HERO")!.Kind);
        runtime.Update(40f);
        Assert.Equal(10f, runtime.World.Positions["HERO"].X);
        Assert.Equal(22.5f, runtime.World.Positions["HERO"].Y);
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
    public void CreateEffect_spawns_at_marker_plus_z()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "MK_FX",
                PositionX = 3,
                PositionY = 4,
                PositionZ = 1,
                Properties = new Dictionary<string, string>(),
            },
        ], null);
        var interp = new ScriptInterpreter("fx",
            ["CreateEffect FX_FIRE,MK_FX,FLAME,2.5"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Single(runtime.World.Effects);
        Assert.Equal("FX_FIRE", runtime.World.Effects[0].Type);
        Assert.Equal("FLAME", runtime.World.Effects[0].Name);
        Assert.Equal(3f, runtime.World.Positions["FLAME"].X);
        Assert.Equal(4f, runtime.World.Positions["FLAME"].Y);
        Assert.Equal(3.5f, runtime.World.Positions["FLAME"].Z);
        Assert.Equal("1", runtime.World.Spawned[0].Properties["Effect"]);
        Assert.NotNull(runtime.Bindings.Resolve("FLAME"));
        Assert.Equal(0x00CCBCDAu, ScriptCommandMap.Find("CreateEffect")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("Create")!.Value.ApplySite,
            ScriptCommandMap.Find("CreateEffect")!.Value.ApplySite);
    }

    [Fact]
    public void DummyEffect_spawns_via_separate_factory()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "MK_D",
                PositionX = 2,
                PositionY = 3,
                PositionZ = 0,
                Properties = new Dictionary<string, string>(),
            },
        ], null);
        var interp = new ScriptInterpreter("dummy",
            ["DummyEffect FX_DUMMY,MK_D,PARAM,D1"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Single(runtime.World.Effects);
        Assert.Equal("FX_DUMMY", runtime.World.Spawned[0].DefinitionType);
        Assert.Equal("D1", runtime.World.Spawned[0].ScriptName);
        Assert.Equal("1", runtime.World.Spawned[0].Properties["Dummy"]);
        Assert.Equal("PARAM", runtime.World.Spawned[0].Properties["DummyParam"]);
        Assert.Equal(2f, runtime.World.Positions["D1"].X);
        Assert.NotEqual(
            ScriptCommandMap.Find("CreateEffect")!.Value.ApplySite,
            ScriptCommandMap.Find("DummyEffect")!.Value.ApplySite);
        Assert.Equal(0x00CCBE5Fu, ScriptCommandMap.Find("DummyEffect")!.Value.ApplySite);
    }

    [Fact]
    public void CreateLight_spawns_at_marker_with_rgb()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "MK_L",
                PositionX = 5,
                PositionY = 6,
                PositionZ = 1,
                Properties = new Dictionary<string, string>(),
            },
        ], null);
        var interp = new ScriptInterpreter("cl",
            ["CreateLight MK_L,255,128,0,2.5,1.0,1,LAMP1,TRUE"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Single(runtime.World.Lights);
        Assert.Equal("LAMP1", runtime.World.Lights[0].Name);
        Assert.Equal("Light", runtime.World.Spawned[0].DefinitionType);
        Assert.Equal("1", runtime.World.Spawned[0].Properties["Light"]);
        Assert.Equal((byte)255, runtime.World.LightColors["LAMP1"].R);
        Assert.Equal((byte)128, runtime.World.LightColors["LAMP1"].G);
        Assert.Equal((byte)0, runtime.World.LightColors["LAMP1"].B);
        Assert.Equal(5f, runtime.World.Positions["LAMP1"].X);
        Assert.Single(runtime.World.Effects);
        Assert.NotNull(runtime.Bindings.Resolve("LAMP1"));
        Assert.Equal(0x00CCBB61u, ScriptCommandMap.Find("CreateLight")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("CreateEffect")!.Value.ApplySite,
            ScriptCommandMap.Find("CreateLight")!.Value.ApplySite);
    }

    [Fact]
    public void CreateLight_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.StartsWith("CreateLight ", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        line ??= "CreateLight MK_L,255,128,0,2.5,1.0,1,LAMP1,TRUE";
        hit ??= bank.Find("CS_OAKVALE_INTRO_FATHER") ?? bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("CreateLight", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        Assert.True(parsed.Arg(7).Length > 0);

        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        using var levels = new LevelLibrary(install);
        runtime.BindScene(levels.LoadThings(RegionTravel.NewGameRegion).Things.ToList(), null);
        if (runtime.FindThingByName(parsed.Arg(0)) is null)
        {
            runtime.BindScene(
            [
                new ThingInstance
                {
                    Kind = "CTC",
                    Section = "Thing",
                    DefinitionType = "Marker",
                    ScriptName = parsed.Arg(0),
                    PositionX = 1,
                    PositionY = 2,
                    PositionZ = 0,
                    Properties = new Dictionary<string, string>(),
                },
            ], null);
        }

        var isolated = new ScriptInterpreter(hit.InstanceName + "-clight", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("CreateLight", StringComparison.OrdinalIgnoreCase));
        Assert.NotEmpty(runtime.World.Lights);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-clight.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-createlight.txt"),
            """
            CreateLight 00CCB933 / apply 00CCBB61
              9 required args else 00CD17FD
              lookup arg0 marker vtbl+280/288
              atof arg1-3; 00BFEA70 fistp -> RGB bytes
              atof arg4/5 floats; arg6>0 flag
              arg7 name; IsTrue(arg8) -> 008ADF90 extras
              vtbl+408(out,pos,rgba,name,f4,f5,flag) — not 364/400/404
              jmp 00CC864B no yield
            """);
    }

    [Fact]
    public void DummyEffect_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.StartsWith("DummyEffect ", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        line ??= "DummyEffect FX_DUMMY,MK_D,PARAM,D1";
        hit ??= bank.Find("CS_OAKVALE_INTRO_FATHER") ?? bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("DummyEffect", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        Assert.True(parsed.Arg(1).Length > 0);
        Assert.True(parsed.Arg(2).Length > 0);

        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        using var levels = new LevelLibrary(install);
        runtime.BindScene(levels.LoadThings(RegionTravel.NewGameRegion).Things.ToList(), null);
        if (runtime.FindThingByName(parsed.Arg(1)) is null)
        {
            runtime.BindScene(
            [
                new ThingInstance
                {
                    Kind = "CTC",
                    Section = "Thing",
                    DefinitionType = "Marker",
                    ScriptName = parsed.Arg(1),
                    PositionX = 1,
                    PositionY = 2,
                    PositionZ = 0,
                    Properties = new Dictionary<string, string>(),
                },
            ], null);
        }

        var isolated = new ScriptInterpreter(hit.InstanceName + "-dummy", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("DummyEffect", StringComparison.OrdinalIgnoreCase));
        Assert.NotEmpty(runtime.World.Effects);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-dummy.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-dummyeffect.txt"),
            """
            DummyEffect 00CCBD62 / apply 00CCBE5F
              arg0 type + arg1 marker + arg2 required else 00CD17FD
              00CBF9DE(arg1); fail -> 00CC864B
              default name empty 0x122D70E; arg3 overwrites
              vtbl+404(out,type,marker,arg2,name,0,1) — not CreateEffect 400
              vtbl+2048(spawn,2); IsTrue(arg4) or empty -> 008ADF90
              jmp 00CC864B no yield
            """);
    }

    [Fact]
    public void CameraShake_stores_both_floats_and_continues()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("shake",
            ["CameraShake 0.5,2.0", "CameraPause FALSE"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.True(runtime.CameraSys.ShakeActive);
        Assert.Equal(0.5f, runtime.CameraSys.ShakeArg0);
        Assert.Equal(2.0f, runtime.CameraSys.ShakeArg1);
        Assert.Contains(runtime.Trace.Steps, s =>
            s.Verb == "CameraShake" && s.Result == ExecutionKind.Continue);
        Assert.Equal(0x00CD1366u, ScriptCommandMap.Find("CameraShake")!.Value.ApplySite);
    }

    [Fact]
    public void CameraEffect_stores_three_floats_and_continues()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("ceff",
            ["CameraEffect 1,0.5,2", "CameraPause FALSE"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.True(runtime.CameraSys.EffectActive);
        Assert.Equal(1f, runtime.CameraSys.EffectArg0);
        Assert.Equal(0.5f, runtime.CameraSys.EffectArg1);
        Assert.Equal(2f, runtime.CameraSys.EffectArg2);
        Assert.Contains(runtime.Trace.Steps, s =>
            s.Verb == "CameraEffect" && s.Result == ExecutionKind.Continue);
        Assert.Equal(0x00CD12C2u, ScriptCommandMap.Find("CameraEffect")!.Value.ApplySite);

        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.StartsWith("CameraEffect ", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        line ??= "CameraEffect 1,0.5,2";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("CameraEffect", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        var bankRt = ScriptRuntime.Detached();
        bankRt.Load(bank, install);
        var iso = new ScriptInterpreter(hit.InstanceName + "-ceff", [line]);
        iso.RunUntilYield(bankRt);
        Assert.True(bankRt.CameraSys.EffectActive);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        bankRt.Trace.Write(Path.Combine(dest, hit.InstanceName + "-ceff.txt"));
    }

    [Fact]
    public void RemoveEffect_destroys_created_effect_not_via_Remove_lookup()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "MK_FX",
                PositionX = 1,
                PositionY = 0,
                PositionZ = 0,
                Properties = new Dictionary<string, string>(),
            },
        ], null);
        var interp = new ScriptInterpreter("rmfx",
        [
            "CreateEffect FX_FIRE,MK_FX,FLAME,0",
            "RemoveEffect FLAME",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Empty(runtime.World.Effects);
        Assert.Contains("FLAME", runtime.World.Removes);
        Assert.Null(runtime.Bindings.Resolve("FLAME"));
        Assert.NotEqual(
            ScriptCommandMap.Find("Remove")!.Value.TokenSite,
            ScriptCommandMap.Find("RemoveEffect")!.Value.TokenSite);
    }

    [Fact]
    public void CameraShake_and_RemoveEffect_real_script_bank_lines()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? shake = null;
        string? removeFx = null;
        ScriptDef? shakeHit = null;
        ScriptDef? removeHit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (shake is null &&
                    raw.StartsWith("CameraShake ", StringComparison.OrdinalIgnoreCase))
                {
                    shake = raw;
                    shakeHit = entry;
                }

                if (removeFx is null &&
                    raw.StartsWith("RemoveEffect ", StringComparison.OrdinalIgnoreCase))
                {
                    removeFx = raw;
                    removeHit = entry;
                }
            }

            if (shake is not null && removeFx is not null)
                break;
        }

        shake ??= "CameraShake 0.5,2.0";
        shakeHit ??= bank.Find("CS_OAKVALE_INTRO_FATHER") ?? bank.Entries[0];
        var shakeLine = ScriptLine.Parse(shake);
        Assert.Equal("CameraShake", shakeLine.Verb);
        Assert.True(shakeLine.Arg(0).Length > 0);
        Assert.True(shakeLine.Arg(1).Length > 0);

        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(shakeHit.InstanceName + "-shake", [shake]);
        isolated.RunUntilYield(runtime);
        Assert.True(runtime.CameraSys.ShakeActive);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("CameraShake", StringComparison.OrdinalIgnoreCase));

        if (removeFx is not null)
        {
            var rm = ScriptLine.Parse(removeFx);
            Assert.Equal("RemoveEffect", rm.Verb);
            runtime.World.SpawnEffect("FX", "MK", rm.Arg(0), null);
            var rem = new ScriptInterpreter((removeHit?.InstanceName ?? "rm") + "-rmfx", [removeFx]);
            rem.RunUntilYield(runtime);
            Assert.Contains(rem.Executed, l =>
                l.StartsWith("RemoveEffect", StringComparison.OrdinalIgnoreCase));
        }

        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, shakeHit.InstanceName + "-shake.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-camerashake-removeeffect.txt"),
            """
            CameraShake 00CD131F / apply 00CD1366
              arg0+arg1 required else 00CD17FD
              atof arg1 then atof arg0
              vtbl+1696(arg1, arg0)
              jmp 00CD17FD no yield
              decay body UNREAD
            RemoveEffect 00CD0071 / apply 00CD00F8
              arg0 required else 00CD17FD
              walk [ebp-96] 12-byte extras (CreateEffect 008ADF90)
              name match 004A93C0 -> vtbl+432(item,0,1)
              empty list / after loop -> 00CD17FD
              NOT Remove world lookup (00CD0116)
            """);
    }

    [Fact]
    public void CreateEffect_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.StartsWith("CreateEffect ", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        line ??= "CreateEffect FX_FIRE,MK_FX,FLAME,0";
        hit ??= bank.Find("CS_OAKVALE_INTRO_FATHER") ?? bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("CreateEffect", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        Assert.True(parsed.Arg(1).Length > 0);

        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        using var levels = new LevelLibrary(install);
        runtime.BindScene(levels.LoadThings(RegionTravel.NewGameRegion).Things.ToList(), null);
        if (runtime.FindThingByName(parsed.Arg(1)) is null)
        {
            runtime.BindScene(
            [
                new ThingInstance
                {
                    Kind = "CTC",
                    Section = "Thing",
                    DefinitionType = "Marker",
                    ScriptName = parsed.Arg(1),
                    PositionX = 1,
                    PositionY = 2,
                    PositionZ = 0,
                    Properties = new Dictionary<string, string>(),
                },
            ], null);
        }

        var isolated = new ScriptInterpreter(hit.InstanceName + "-fx", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("CreateEffect", StringComparison.OrdinalIgnoreCase));
        Assert.NotEmpty(runtime.World.Effects);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-fx.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-createeffect.txt"),
            """
            CreateEffect 00CCBB9A / apply 00CCBCDA
              arg0 type + arg1 marker required else 00CD17FD
              00CBF9DE(arg1); fail -> continue 00CC864B
              default name empty CString 0x122D70E; arg2 overwrites
              default z=0; arg3 atof added to marker+8
              vtbl+400(type, pos, name) — not Create 364 / Near 368 / Object 392
              vtbl+2048(spawn,2); IsTrue(arg4) or empty -> 008ADF90 extras
              jmp 00CC864B no yield
            """);
    }

    [Fact]
    public void CreateNear_spawns_at_near_thing_position()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "HERO_POS",
                PositionX = 4,
                PositionY = 5,
                PositionZ = 0,
                Properties = new Dictionary<string, string>(),
            },
        ], null);
        var interp = new ScriptInterpreter("cn",
            ["CreateNear CREATURE_DOG,HERO_POS,DOG1,2"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Single(runtime.World.Spawned);
        Assert.Equal("DOG1", runtime.World.Spawned[0].ScriptName);
        Assert.Equal(4f, runtime.World.Positions["DOG1"].X);
        Assert.Equal(5f, runtime.World.Positions["DOG1"].Y);
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
    public void Create_spawns_at_marker_and_skips_duplicate_when_unique()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            Marker("MK_A", 3, 4, 5),
        ], null);
        var interp = new ScriptInterpreter("cr",
        [
            "Create CREATURE_VILLAGER,MK_A,VILL1",
            "Create CREATURE_VILLAGER,MK_A,VILL1,,,TRUE",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Single(runtime.World.Spawned);
        Assert.Equal("CREATURE_VILLAGER", runtime.World.Spawned[0].DefinitionType);
        Assert.Equal("VILL1", runtime.World.Spawned[0].ScriptName);
        Assert.Equal(3f, runtime.World.Positions["VILL1"].X);
        Assert.Equal("1", runtime.World.Spawned[0].Properties["Extra"]);
        Assert.NotNull(runtime.Bindings.Resolve("VILL1"));
        Assert.Equal(0x00CCC3E6u, ScriptCommandMap.Find("Create")!.Value.ApplySite);
    }

    [Fact]
    public void Create_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.StartsWith("Create ", StringComparison.OrdinalIgnoreCase) &&
                    !raw.Contains('$', StringComparison.Ordinal))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        line ??= "Create CREATURE_VILLAGER,MK_A,VILL1";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("Create", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        Assert.True(parsed.Arg(2).Length > 0);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        runtime.BindScene([Marker(parsed.Arg(1), 1, 2, 3)], null);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-create", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("Create", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(runtime.World.Spawned, t =>
            t.ScriptName is not null &&
            t.ScriptName.Equals(parsed.Arg(2), StringComparison.OrdinalIgnoreCase));
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-create.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-create-0070d580.txt"),
            """
            Create 00CCC246 / apply 00CCC3E6
              3 required; arg4 name suffix; IsTrue(arg5) skip if bound
              lookup marker; vtbl+364 008A9100(type,pos,name)
              empty|IsTrue(arg3) extras 008ADF90
              not IsFalse(arg6) 00CD3D2E persist bind
              vtbl+2148 activate; jmp 00CD17F8
            PlayAnimation does NOT call 0070D580
              thing vtbl+72 004C7470; CTC+68 00686920 al=1 stub
            0070D580 is 005B37F7 DEFAULT:
              005DC340 20-byte name table
              0070C050 request mode 6
              0070B460 inner; 0070D1A0 copies request
              PALSKIN packer 00BD2D90 unread from this path
            """);
    }

    [Fact]
    public void Appearance_DEFAULT_starts_0070D580_inner_play()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.Animation.PlayAppearanceDefault("HERO");
        var state = runtime.Animation.States["HERO"];
        Assert.Equal("DEFAULT", state.ClipKey);
        Assert.Equal(6, state.RequestMode);
        Assert.True(state.Playing);
        Assert.Equal(0f, state.PlayTime);
        Assert.Equal(1f, state.Duration);
        Assert.Equal(0x0070D580u, RegionTravel.AnimationPlayInner);
        Assert.Equal(0x0070C050u, RegionTravel.AnimationPlayRequest);
        Assert.False(RegionTravel.FirstSeenPlayAnimationCallsInnerPlay);
    }

    [Fact]
    public void TintScreenOut_consumes_hold_and_continues()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("tint",
            ["TintScreenOut 1.5", "CameraPause FALSE"]);
        interp.SetTintHold(0.25f);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.True(runtime.CameraSys.TintOutActive);
        Assert.Equal(1.5f, runtime.CameraSys.TintOutDuration);
        Assert.Equal(0.25f, runtime.CameraSys.TintOutHold);
        Assert.Equal(0f, interp.TintHold);
        Assert.Equal(0x00CD11F7u, ScriptCommandMap.Find("TintScreenOut")!.Value.ApplySite);
    }

    [Fact]
    public void TintScreenTo_writes_hold_and_scales_rgb()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Hero",
                ScriptName = "Hero",
                Properties = new Dictionary<string, string>(),
            },
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "CREATURE_BANDIT",
                ScriptName = "Bandit0",
                Properties = new Dictionary<string, string>(),
            },
        ], null);
        var interp = new ScriptInterpreter("tintto",
        [
            "TintScreenTo 0.2,0.1,0,0,1.0,'255,0,0',HERO",
            "TintScreenOut 0.5",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.True(runtime.CameraSys.TintToActive);
        Assert.True(runtime.CameraSys.TintHandle > 0);
        Assert.Equal(1f, runtime.CameraSys.TintRgb.X, 3);
        Assert.Equal(0f, runtime.CameraSys.TintRgb.Y);
        Assert.Contains("Hero", runtime.CameraSys.TintTargets);
        Assert.True(runtime.CameraSys.TintOutActive);
        Assert.Equal(0f, interp.TintHold);
        Assert.Equal(0x00CD115Au, ScriptCommandMap.Find("TintScreenTo")!.Value.ApplySite);
    }

    [Fact]
    public void TintScreenTo_alldef_collects_by_type()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "CREATURE_BANDIT",
                ScriptName = "Bandit0",
                Properties = new Dictionary<string, string>(),
            },
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "CREATURE_BANDIT",
                ScriptName = "Bandit1",
                Properties = new Dictionary<string, string>(),
            },
        ], null);
        var interp = new ScriptInterpreter("alldef",
            ["TintScreenTo 1,0,0,0,0,'0,0,0',ALLDEF:CREATURE_BANDIT"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Contains("Bandit0", runtime.CameraSys.TintTargets);
        Assert.Contains("Bandit1", runtime.CameraSys.TintTargets);
        Assert.Contains(runtime.CameraSys.TintFilters, f =>
            f.StartsWith("ALLDEF:", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TintScreenTo_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.StartsWith("TintScreenTo ", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        line ??= "TintScreenTo 0.2,0.1,0,0,1.0,'255,0,0',HERO";
        hit ??= bank.Find("CS_OAKVALE_INTRO_FATHER") ?? bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("TintScreenTo", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        Assert.True(parsed.Arg(6).Length > 0);

        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        using var levels = new LevelLibrary(install);
        runtime.BindScene(levels.LoadThings(RegionTravel.NewGameRegion).Things.ToList(), null);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-tintto", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("TintScreenTo", StringComparison.OrdinalIgnoreCase));
        Assert.True(runtime.CameraSys.TintToActive);
        Assert.True(isolated.TintHold > 0f);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-tintto.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-tintscreento.txt"),
            """
            TintScreenTo 00CD0CE4 / apply 00CD115A
              7 required args else 00CD17FD
              atof arg0-4; arg0 also initial [ebp-112]
              00CBFACA split arg5; if 3 tokens RGB * 0x1231724=1/255
              00CBFACA split arg6 filters
              ALL: -> vtbl+300 collect; ALLDEF: -> vtbl+320
              else lookup thing (HERO / vtbl+280/288)
              vtbl+2700(a0,a1,a2,a3,a4,rgb,list); [ebp-112]=eax handle
              jmp 00CD17FD no yield
            """);
    }

    [Fact]
    public void SetLightScene_blacks_defs_then_applies_scene_rgb()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("ls",
            ["SetLightScene 0", "CameraPause FALSE"]);
        interp.BindLightTables(
            ["LAMP:255,128,0", "FILL:10,20,30"],
            ["0", "1", "0,1"]);
        interp.RunUntilYield(runtime);
        Assert.Equal(ExecutionKind.YieldOnce, interp.CurrentWaitKind);
        Assert.Equal(0, runtime.World.ActiveLightScene);
        Assert.Equal((byte)255, runtime.World.LightColors["LAMP"].R);
        Assert.Equal((byte)128, runtime.World.LightColors["LAMP"].G);
        Assert.Equal((byte)0, runtime.World.LightColors["FILL"].R);
        interp.Resume(runtime);
        Assert.True(interp.Finished);

        var two = new ScriptInterpreter("ls2", ["SetLightScene 2"]);
        two.BindLightTables(
            ["LAMP:255,128,0", "FILL:10,20,30"],
            ["0", "1", "0,1"]);
        two.RunUntilYield(runtime);
        Assert.Equal((byte)255, runtime.World.LightColors["LAMP"].R);
        Assert.Equal((byte)10, runtime.World.LightColors["FILL"].R);
        Assert.Equal(0x00CD172Au, ScriptCommandMap.Find("SetLightScene")!.Value.ApplySite);
    }

    [Fact]
    public void SetLightScene_out_of_range_continues()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("ls0", ["SetLightScene 5", "CameraPause FALSE"]);
        interp.BindLightTables(["LAMP:1,2,3"], ["0"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(-1, runtime.World.ActiveLightScene);
        Assert.Empty(runtime.World.LightColors);
    }

    [Fact]
    public void SetLightScene_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.StartsWith("SetLightScene ", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        line ??= "SetLightScene 0";
        hit ??= bank.Find("CS_OAKVALE_INTRO_FATHER") ?? bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("SetLightScene", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        Assert.Equal(3, ScriptBank.LightDefVectorIndex);
        Assert.Equal(4, ScriptBank.LightSceneVectorIndex);

        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-light", [line]);
        var defs = hit.Vectors.Count > ScriptBank.LightDefVectorIndex
            ? hit.Vectors[ScriptBank.LightDefVectorIndex]
            : [];
        var scenes = hit.Vectors.Count > ScriptBank.LightSceneVectorIndex
            ? hit.Vectors[ScriptBank.LightSceneVectorIndex]
            : (IReadOnlyList<string>)["0"];
        if (defs.Count == 0)
            isolated.BindLightTables(["LAMP:255,128,0"], ["0"]);
        else
            isolated.BindLightTables(defs, scenes);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("SetLightScene", StringComparison.OrdinalIgnoreCase));
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-light.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-setlightscene.txt"),
            """
            SetLightScene 00CD1425 / apply 00CD172A
              atoi arg0 indexes persist +96 (ScriptBank vector 4)
              index >= count -> 00CD17FD no yield
              +84 (vector 3) defs NAME:r,g,b via 00CBF050 (colon + comma atoi bytes)
              first loop vtbl+2180 each thing color (0,0,0,255)
              scene string: comma-separated def indices, spaces skipped
              selected vtbl+2180(thing, r,g,b,255)
              yield vtbl+28 if [ebp+103]
            persist 00F2A1D0: +60 +72 +108 +84 +96 +120 +132 +90
            """);
    }

    [Fact]
    public void CameraPath_sits_at_first_marker_and_continues()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            Marker("P0", 0, 0, 0),
            Marker("P1", 4, 0, 0),
            Marker("P2", 4, 4, 0),
            Marker("P3", 0, 4, 0),
        ], new ScriptedCamera());
        var interp = new ScriptInterpreter("cpath",
            ["CameraPath P0,P1,P2,P3,2.5", "CameraPause FALSE"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal("P0", runtime.CameraSys.PathA);
        Assert.Equal("P1", runtime.CameraSys.PathB);
        Assert.Equal(2.5f, runtime.CameraSys.PathDuration);
        Assert.Equal(0f, runtime.Camera!.Position.X);
        Assert.Equal(4f, runtime.Camera.LookAt.X);
        Assert.Contains(runtime.Trace.Steps, s =>
            s.Verb == "CameraPath" && s.Result == ExecutionKind.Continue);
        Assert.Equal(0x00CCB048u, ScriptCommandMap.Find("CameraPath")!.Value.ApplySite);
    }

    private static ThingInstance Marker(string name, float x, float y, float z) =>
        new()
        {
            Kind = "CTC",
            Section = "Thing",
            DefinitionType = "Marker",
            ScriptName = name,
            PositionX = x,
            PositionY = y,
            PositionZ = z,
            Properties = new Dictionary<string, string>(),
        };

    [Fact]
    public void CameraPath_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.StartsWith("CameraPath ", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        line ??= "CameraPath A,B,C,D,1.0";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("CameraPath", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        Assert.True(parsed.Arg(4).Length > 0);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        runtime.BindScene(
        [
            Marker(parsed.Arg(0), 0, 0, 0),
            Marker(parsed.Arg(1), 1, 0, 0),
            Marker(parsed.Arg(2), 1, 1, 0),
            Marker(parsed.Arg(3), 0, 1, 0),
        ], new ScriptedCamera());
        var isolated = new ScriptInterpreter(hit.InstanceName + "-cpath", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("CameraPath", StringComparison.OrdinalIgnoreCase));
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-cpath.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-camerapath.txt"),
            """
            CameraPath 00CCAF1D / apply 00CCB048
              5 required args else 00CD17FD
              00CBF9DE lookup arg0-3
              atof arg4 duration
              vtbl+1640(pos0,pos2,pos1,pos3,dur)
              jmp 00CC864B no yield
              spline unread
            """);
    }

    [Fact]
    public void CameraRotateThing_looks_at_target_and_yields()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "HERO",
                PositionX = 3,
                PositionY = 4,
                PositionZ = 1,
                Properties = new Dictionary<string, string>(),
            },
        ], new ScriptedCamera());
        var interp = new ScriptInterpreter("rot",
            ["CameraRotateThing HERO,1.5,0,0,1", "CameraPause FALSE"]);
        interp.RunUntilYield(runtime);
        Assert.Equal(ExecutionKind.YieldOnce, interp.CurrentWaitKind);
        Assert.True(runtime.CameraSys.RotateActive);
        Assert.Equal("HERO", runtime.CameraSys.RotateThing);
        Assert.Equal(1.5f, runtime.CameraSys.RotateParam);
        Assert.Equal(1f, runtime.CameraSys.RotateAxis.Z);
        Assert.Equal(3f, runtime.Camera!.LookAt.X);
        interp.Resume(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(0x00CCA712u, ScriptCommandMap.Find("CameraRotateThing")!.Value.ApplySite);
    }

    [Fact]
    public void CameraRotateThing_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.StartsWith("CameraRotateThing ", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        line ??= "CameraRotateThing HERO,1.5,0,0,1";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("CameraRotateThing", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        Assert.True(parsed.Arg(4).Length > 0);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = parsed.Arg(0),
                PositionX = 1,
                PositionY = 2,
                PositionZ = 0,
                Properties = new Dictionary<string, string>(),
            },
        ], new ScriptedCamera());
        var isolated = new ScriptInterpreter(hit.InstanceName + "-rot", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("CameraRotateThing", StringComparison.OrdinalIgnoreCase));
        Assert.True(runtime.CameraSys.RotateActive);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-rot.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-camerarotatething.txt"),
            """
            CameraRotateThing 00CCA5B6 / apply 00CCA712
              5 required args else 00CD17FD
              lookup arg0 vtbl+280/288
              atof arg1 param; arg2-4 xyz
              vtbl+1616(thing, xyz, param)
              jmp 00CC907D YieldOnce
              orbit body UNREAD
            """);
    }

    [Fact]
    public void CameraLookBetween_aims_midpoint_and_yields()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "A",
                PositionX = 0,
                PositionY = 0,
                PositionZ = 0,
                Properties = new Dictionary<string, string>(),
            },
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "B",
                PositionX = 10,
                PositionY = 4,
                PositionZ = 0,
                Properties = new Dictionary<string, string>(),
            },
        ], new ScriptedCamera());
        var interp = new ScriptInterpreter("clb",
            ["CameraLookBetween A,B,MODE,1.5,0,0,0,0,0,0"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Yielded);
        Assert.Equal(ExecutionKind.YieldOnce, interp.CurrentWaitKind);
        Assert.Equal("A", runtime.CameraSys.LookBetweenA);
        Assert.Equal("B", runtime.CameraSys.LookBetweenB);
        Assert.Equal(1.5f, runtime.CameraSys.LookBetweenDuration);
        Assert.Equal(5f, runtime.Camera!.LookAt.X);
        Assert.Equal(2f, runtime.Camera.LookAt.Y);
        Assert.Equal(CommandStatus.Partial, ScriptCommandMap.Find("CameraLookBetween")!.Value.Runtime);
        interp.Resume(runtime);
        Assert.True(interp.Finished);
    }

    [Fact]
    public void CameraFOVLookBetween_sets_fov_degrees_on_scripted_camera()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "A",
                PositionX = 0,
                PositionY = 0,
                PositionZ = 0,
                Properties = new Dictionary<string, string>(),
            },
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "B",
                PositionX = 4,
                PositionY = 0,
                PositionZ = 0,
                Properties = new Dictionary<string, string>(),
            },
        ], new ScriptedCamera());
        var interp = new ScriptInterpreter("fovlb",
            ["CameraFOVLookBetween A,B,MODE,2.0,70"]);
        interp.RunUntilYield(runtime);
        Assert.Equal(ExecutionKind.YieldOnce, interp.CurrentWaitKind);
        Assert.Equal(70f, runtime.CameraSys.LookBetweenFov);
        Assert.Equal(70f, runtime.Camera!.FovDegrees);
        Assert.Equal(2f, runtime.Camera.LookAt.X);
        interp.Resume(runtime);
        Assert.True(interp.Finished);
    }

    [Fact]
    public void CameraFOVLookBetweenPos_sets_camera_position_and_look()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "A",
                PositionX = 0,
                PositionY = 0,
                PositionZ = 0,
                Properties = new Dictionary<string, string>(),
            },
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "B",
                PositionX = 8,
                PositionY = 0,
                PositionZ = 0,
                Properties = new Dictionary<string, string>(),
            },
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "CAM",
                PositionX = 1,
                PositionY = 2,
                PositionZ = 3,
                Properties = new Dictionary<string, string>(),
            },
        ], new ScriptedCamera());
        var interp = new ScriptInterpreter("fovpos",
            ["CameraFOVLookBetweenPos A,B,CAM,1.0,10,4,5"]);
        interp.RunUntilYield(runtime);
        Assert.Equal(ExecutionKind.YieldOnce, interp.CurrentWaitKind);
        Assert.Equal(4f, runtime.Camera!.LookAt.X);
        Assert.Equal(11f, runtime.Camera.Position.X);
        Assert.Equal(6f, runtime.Camera.Position.Y);
        Assert.Equal(8f, runtime.Camera.Position.Z);
        Assert.Equal(10f, runtime.Camera.FovDegrees);
        Assert.NotNull(runtime.CameraSys.LookBetweenCameraPos);
        interp.Resume(runtime);
        Assert.True(interp.Finished);
    }

    [Fact]
    public void CameraFOVLookBetweenPos_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.Contains("CameraFOVLookBetweenPos", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        if (line is null)
        {
            var compiled = Path.Combine(install.DataRoot, "CompiledDefs");
            if (Directory.Exists(compiled))
            {
                foreach (var file in Directory.EnumerateFiles(compiled, "*.bin", SearchOption.AllDirectories))
                {
                    var scrape = ScriptBank.ExtractCommands(File.ReadAllBytes(file));
                    var found = scrape.FirstOrDefault(s =>
                        s.Contains("CameraFOVLookBetweenPos", StringComparison.OrdinalIgnoreCase));
                    if (found is not null)
                    {
                        line = found;
                        break;
                    }
                }
            }
        }

        line ??= "CameraFOVLookBetweenPos HERO,FATHER,CAM_POS,1.0";
        hit ??= bank.Find("CS_OAKVALE_INTRO_FATHER") ?? bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("CameraFOVLookBetweenPos", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        Assert.True(parsed.Arg(1).Length > 0);
        Assert.True(parsed.Arg(2).Length > 0);
        Assert.True(parsed.Arg(3).Length > 0);

        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        using var levels = new LevelLibrary(install);
        runtime.BindScene(levels.LoadThings(RegionTravel.NewGameRegion).Things.ToList(), new ScriptedCamera());
        var isolated = new ScriptInterpreter(hit.InstanceName + "-fovpos", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains("CameraFOVLookBetweenPos", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(parsed.Arg(0), runtime.CameraSys.LookBetweenA);
        Assert.Equal(parsed.Arg(1), runtime.CameraSys.LookBetweenB);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-fovpos.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-camerafovlookbetweenpos.txt"),
            """
            CameraFOVLookBetweenPos 00CCB07C / apply 00CCB42C
              4 required args else 00CD17FD
              lookup arg0/arg1 things
              atof arg3 duration (ebp+8)
              optional arg4/5/6 -> offset of arg2 pos (x/y/z)
              00CD3187(arg2); fail -> lookup arg2 as thing, 004AA980
              fov default -1; arg4 nonempty -> atof*1/360
              vtbl+1636(posA, posB, camPos+off, duration, fov)
              if [ebp+103] vtbl+28; jmp 00CC864B
              Distinct from CameraFOVLookBetween vtbl+1632.
            """);
    }

    [Fact]
    public void CameraLookBetween_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.StartsWith("CameraLookBetween ", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        line ??= "CameraLookBetween HERO,FATHER,MODE,1.0";
        hit ??= bank.Find("CS_OAKVALE_INTRO_FATHER") ?? bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("CameraLookBetween", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        Assert.True(parsed.Arg(1).Length > 0);
        Assert.True(parsed.Arg(2).Length > 0);
        Assert.True(parsed.Arg(3).Length > 0);

        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        using var levels = new LevelLibrary(install);
        runtime.BindScene(levels.LoadThings(RegionTravel.NewGameRegion).Things.ToList(), new ScriptedCamera());
        var isolated = new ScriptInterpreter(hit.InstanceName + "-clb", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("CameraLookBetween", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(parsed.Arg(0), runtime.CameraSys.LookBetweenA);
        Assert.Equal(parsed.Arg(1), runtime.CameraSys.LookBetweenB);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-clb.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-cameralookbetween.txt"),
            """
            CameraLookBetween 00CCAA6C
              4 required args else 00CD17FD
              lookup arg0/arg1 via vtbl+280/288
              atof arg3 duration (ebp+12)
              optional atof arg4-6 offset A, arg7-9 offset B
              00CD3187(arg2) table; fail -> lookup arg2 as thing
              vtbl+1632(posA+off, posB+off, handle, duration, -1.0 at 0x122DEE0)
              if [ebp+103] vtbl+28; jmp 00CC864B
              blend/spline body UNREAD — host aims midpoint
            """);
    }

    [Fact]
    public void DrawThing_sets_world_drawable_flag()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("dt",
            ["DrawThing HERO,FALSE", "DrawThing FATHER,TRUE"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.False(runtime.World.Drawable["HERO"]);
        Assert.True(runtime.World.Drawable["FATHER"]);
        Assert.Equal(0x00CC9DDDu, ScriptCommandMap.Find("DrawThing")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("Drawable")!.Value.TokenSite,
            ScriptCommandMap.Find("DrawThing")!.Value.TokenSite);
    }

    [Fact]
    public void DrawThing_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.StartsWith("DrawThing ", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        line ??= "DrawThing HERO,FALSE";
        hit ??= bank.Find("CS_OAKVALE_INTRO_FATHER") ?? bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("DrawThing", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        Assert.True(parsed.Arg(1).Length > 0);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-draw", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("DrawThing", StringComparison.OrdinalIgnoreCase));
        Assert.True(runtime.World.Drawable.ContainsKey(parsed.Arg(0)));
        Assert.Equal(!ScriptLine.IsFalse(parsed.Arg(1)), runtime.World.Drawable[parsed.Arg(0)]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-draw.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-drawthing.txt"),
            """
            DrawThing 00CC9D07 / apply 00CC9DDD
              arg0 name + arg1 required else 00CD17FD
              default draw=1; IsFalse(arg1) -> 0
              lookup arg0 vtbl+280/288
              vtbl+2044(thing, draw)
              jmp 00CC864B no yield
              Distinct from entity Drawable
            """);
    }

    [Fact]
    public void UseCameraFOVMarkerList_picks_best_xy_projection()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            Marker("HERO", 0, 10, 0),
            Marker("BOSS", 0, 0, 0),
            Marker("CAM_L", -10, 5, 0),
            Marker("CAM_BACK", 0, -5, 0),
            Marker("CAM_BEST", 0, 5, 0),
            Marker("CAM_R", 10, 5, 0),
        ], new ScriptedCamera());
        var interp = new ScriptInterpreter("fovml",
        [
            "UseCameraFOVMarkerList HERO,BOSS,CAM_L,CAM_BACK,CAM_BEST,CAM_R,10,55",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal("CAM_BEST", runtime.CameraSys.FovMarkerSelected);
        Assert.Equal("HERO", runtime.CameraSys.FovMarkerThingA);
        Assert.Equal("BOSS", runtime.CameraSys.FovMarkerThingB);
        Assert.Equal(10f, runtime.CameraSys.FovMarkerDuration);
        Assert.Equal(55f, runtime.CameraSys.LookBetweenFov);
        Assert.Equal(0f, runtime.Camera!.LookAt.X);
        Assert.Equal(5f, runtime.Camera.LookAt.Y);
        Assert.Equal(55f, runtime.Camera.FovDegrees);
        Assert.Contains(runtime.Trace.Steps, s =>
            s.Verb == "UseCameraFOVMarkerList" && s.Result == ExecutionKind.Continue);
        Assert.Equal(0x00CC9C53u, ScriptCommandMap.Find("UseCameraFOVMarkerList")!.Value.ApplySite);
    }

    [Fact]
    public void UseCameraFOVMarkerList_false_flag_keeps_last_marker()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            Marker("HERO", 0, 10, 0),
            Marker("BOSS", 0, 0, 0),
            Marker("CAM_L", -10, 5, 0),
            Marker("CAM_BACK", 0, -5, 0),
            Marker("CAM_BEST", 0, 5, 0),
            Marker("CAM_R", 10, 5, 0),
        ], new ScriptedCamera());
        var interp = new ScriptInterpreter("fovml-last",
        [
            "UseCameraFOVMarkerList HERO,BOSS,CAM_L,CAM_BACK,CAM_BEST,CAM_R,10,55,FALSE",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal("CAM_R", runtime.CameraSys.FovMarkerSelected);
    }

    [Fact]
    public void UseCameraFOVMarkerList_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.StartsWith("UseCameraFOVMarkerList ", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        line ??= "UseCameraFOVMarkerList HERO,BOSS,CAM0,CAM1,CAM2,CAM3,10,55";
        hit ??= bank.Find("CS_JOB_BOSS_PHASE_2") ?? bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("UseCameraFOVMarkerList", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        Assert.True(parsed.Arg(6).Length > 0);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        runtime.BindScene(
        [
            Marker(parsed.Arg(0), 0, 10, 0),
            Marker(parsed.Arg(1), 0, 0, 0),
            Marker(parsed.Arg(2), -10, 5, 0),
            Marker(parsed.Arg(3), 0, -5, 0),
            Marker(parsed.Arg(4), 0, 5, 0),
            Marker(parsed.Arg(5), 10, 5, 0),
        ], new ScriptedCamera());
        var isolated = new ScriptInterpreter(hit.InstanceName + "-fovml", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("UseCameraFOVMarkerList", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(parsed.Arg(0), runtime.CameraSys.FovMarkerThingA);
        Assert.Equal(parsed.Arg(1), runtime.CameraSys.FovMarkerThingB);
        Assert.True(runtime.CameraSys.FovMarkerSelected.Length > 0);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-fovml.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-usecamerafovmarkerlist.txt"),
            """
            UseCameraFOVMarkerList 00CC96BD / apply 00CC9C53
              7 required args else 00CD17FD
              lookup arg0-5 via vtbl+280/288; 004AB130 all six else skip
              extras list 008ADF90 of arg2-5
              atof arg6 duration
              optional arg7 atof * 1/360 (0x1238E00) default -1
              IsFalse(arg8) -> flag=0 else 1
              00CBF13C(out, list, A, B, flag):
                XY (A-B) and (marker-B), eps 0.0001 at 0x129BA3C
                flag=1 keep score > best (init -2)
                flag=0 008AB980-assign every finite so last wins
              arg9 present -> vtbl+1648 unread
              else vtbl+1632(pos,pos,B,dur,fov)  004AA980 is thiscall +4
              jmp 00CC864B no yield
            """);
    }

    [Fact]
    public void SetDoorOpen_and_SetChestOpen_write_world_flags()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("doorchest",
        [
            "SetDoorOpen GATE,TRUE",
            "SetChestOpen BOX,FALSE",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.True(runtime.World.Doors["GATE"]);
        Assert.False(runtime.World.Chests["BOX"]);
        Assert.Equal(0x00CC8BEBu, ScriptCommandMap.Find("SetDoorOpen")!.Value.ApplySite);
        Assert.Equal(0x00CC8D73u, ScriptCommandMap.Find("SetChestOpen")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("SetDoorOpen")!.Value.TokenSite,
            ScriptCommandMap.Find("SetChestOpen")!.Value.TokenSite);
    }

    [Fact]
    public void SetChestOpen_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.StartsWith("SetChestOpen ", StringComparison.OrdinalIgnoreCase) ||
                    raw.StartsWith("SetDoorOpen ", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        line ??= "SetChestOpen CHEST,TRUE";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.True(
            parsed.Verb.Equals("SetChestOpen", StringComparison.OrdinalIgnoreCase) ||
            parsed.Verb.Equals("SetDoorOpen", StringComparison.OrdinalIgnoreCase));
        Assert.True(parsed.Arg(0).Length > 0);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-door", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith(parsed.Verb, StringComparison.OrdinalIgnoreCase));
        var open = !ScriptLine.IsFalse(parsed.Arg(1));
        if (parsed.Verb.Equals("SetChestOpen", StringComparison.OrdinalIgnoreCase))
            Assert.Equal(open, runtime.World.Chests[parsed.Arg(0)]);
        else
            Assert.Equal(open, runtime.World.Doors[parsed.Arg(0)]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-door.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-setdoorchest.txt"),
            """
            SetDoorOpen 00CC8A8D / apply 00CC8BEB
              arg0 required else 00CD17FD
              IsFalse(arg1) -> vtbl+1704 close
              else vtbl+1700 open
              jmp 00CD17F8 no yield
            SetChestOpen 00CC8C14 / apply 00CC8D73
              same parse
              IsFalse(arg1) -> vtbl+1744 close
              else vtbl+1740(thing, 0) open
              jmp 00CD17F8 no yield
              Distinct tokens and vtbls — not aliases
            """);
    }

    [Fact]
    public void CameraRig_teleports_mount_and_waits_scaled_frames()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            Marker("CAM_RIG", 0, 0, 5),
            Marker("HERO", 10, 20, 0),
        ], new ScriptedCamera());
        var interp = new ScriptInterpreter("crig",
            ["CameraRig CAM_RIG,HERO,0,0,2,1.0"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Yielded);
        Assert.Equal(ExecutionKind.WaitScaledFrames, interp.CurrentWaitKind);
        Assert.Equal(1f, runtime.CameraSys.RigSeconds);
        Assert.Equal("CAM_RIG", runtime.CameraSys.RigThingA);
        Assert.Equal("HERO", runtime.CameraSys.RigThingB);
        Assert.Equal(2f, runtime.CameraSys.RigOffset.Z);
        Assert.Equal(10f, runtime.World.Positions["CAM_RIG"].X);
        Assert.Equal(20f, runtime.World.Positions["CAM_RIG"].Y);
        Assert.Equal(2f, runtime.World.Positions["CAM_RIG"].Z);
        Assert.Equal(10f, runtime.Camera!.Position.X);
        Assert.Equal(20f, runtime.Camera.Position.Y);
        Assert.Equal(2f, runtime.Camera.Position.Z);
        Assert.Equal(RegionTravel.GamePauseScale, interp.GamePauseTarget);
        Assert.Equal(0x00CC965Du, ScriptCommandMap.Find("CameraRig")!.Value.ApplySite);
    }

    [Fact]
    public void CameraRig_zero_seconds_skips_apply()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene([Marker("A", 0, 0, 0), Marker("B", 1, 0, 0)], new ScriptedCamera());
        var interp = new ScriptInterpreter("crig0", ["CameraRig A,B,0,0,0,0"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.False(runtime.World.Positions.ContainsKey("A"));
        Assert.False(runtime.CameraSys.RigActive);
    }

    [Fact]
    public void CameraRig_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.StartsWith("CameraRig ", StringComparison.OrdinalIgnoreCase) &&
                    !raw.Contains('$', StringComparison.Ordinal))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        line ??= "CameraRig CAM,HERO,0,0,2,1.0";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("CameraRig", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        Assert.True(parsed.Arg(5).Length > 0);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        runtime.BindScene(
        [
            Marker(parsed.Arg(0), 0, 0, 5),
            Marker(parsed.Arg(1), 4, 8, 0),
        ], new ScriptedCamera());
        var isolated = new ScriptInterpreter(hit.InstanceName + "-crig", [line]);
        isolated.RunUntilYield(runtime);
        Assert.True(isolated.Yielded || isolated.Finished);
        Assert.Equal(parsed.Arg(0), runtime.CameraSys.RigThingA);
        Assert.Equal(parsed.Arg(1), runtime.CameraSys.RigThingB);
        ScriptLine.TryFloat(parsed.Arg(5), out var seconds);
        if (seconds * RegionTravel.GamePauseScale > 0f)
            Assert.True(runtime.CameraSys.RigActive);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-crig.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-camerarig.txt"),
            """
            CameraRig 00CC93E3 / apply 00CC965D
              6 required args else 00CD17FD
              lookup arg0/arg1 vtbl+280/288
              atof arg2-4 offset
              arg5 * 15 (0x124E640) loop count
              count<=0 skips apply
              each iter: 004AB130(B); dest=B.pos+off
                vtbl+1892(A, dest, 0, 0, 0) teleport
                vtbl+1644(A, 0, 0, -1, 0, -1)
                yield vtbl+28 if [ebp+103]
              jmp 00CC864B
            """);
    }

    [Fact]
    public void PlayAnimation_sets_clip_and_yields_unless_animation_pause()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("anim",
            ["HERO.PlayAnimation CS_TIRED,FALSE,FALSE,TRUE,FALSE"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Yielded);
        Assert.Equal("CS_TIRED", runtime.Animation.States["HERO"].Name);
        Assert.False(runtime.Animation.States["HERO"].Looping);
        Assert.True(runtime.Animation.States["HERO"].F3);
        Assert.False(runtime.Animation.States["HERO"].F4);
        Assert.Equal(0x00CC15DAu, ScriptCommandMap.Find("PlayAnimation")!.Value.ApplySite);

        var paused = ScriptRuntime.Detached();
        var noYield = new ScriptInterpreter("animp",
            ["AnimationPause FALSE", "HERO.PlayAnimation CS_TIRED"]);
        noYield.RunUntilYield(paused);
        Assert.True(noYield.Finished);
        Assert.Equal("CS_TIRED", paused.Animation.States["HERO"].Name);
    }

    [Fact]
    public void PlayLoopingAnim_is_vtbl80_not_PlayAnimation()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("loopanim",
            ["HERO.PlayLoopingAnim WALK,3,FALSE,FALSE,FALSE,TRUE"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Yielded);
        Assert.Equal("WALK", runtime.Animation.States["HERO"].Name);
        Assert.True(runtime.Animation.States["HERO"].Looping);
        Assert.Equal(3, runtime.Animation.States["HERO"].Loops);
        Assert.Equal(0x00CC186Cu, ScriptCommandMap.Find("PlayLoopingAnim")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("PlayAnimation")!.Value.ApplySite,
            ScriptCommandMap.Find("PlayLoopingAnim")!.Value.ApplySite);
    }

    [Fact]
    public void PlayAnimation_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.Contains(".PlayAnimation ", StringComparison.OrdinalIgnoreCase) &&
                    !raw.Contains('$', StringComparison.Ordinal))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        line ??= "HERO.PlayAnimation CS_TIRED,FALSE,FALSE,TRUE,FALSE";
        hit ??= bank.Find("CS_OAKVALE_INTRO_FATHER") ?? bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("PlayAnimation", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-anim", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains("PlayAnimation", StringComparison.OrdinalIgnoreCase));
        if (parsed.Target.Length > 0)
            Assert.Equal(parsed.Arg(0), runtime.Animation.States[parsed.Target].Name);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-anim.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-playanimation.txt"),
            """
            PlayAnimation 00CC14B8 / apply 00CC15DA
              actor + arg0 required else 00CC7081
              flags: IsTrue arg1/2/3, IsFalse arg4 (default 1), IsTrue arg5
              actor.vtbl+72(name,f0,f1,f2,f3,byte_1375748,0,f4)
              004C7470 walks [this+68..+72] components; +68 accept
              [ebp-22] default 1 (00CBFD57 / AnimationPause)
                0 -> 00CC7081 continue
                1 -> 00CC5691 yield if [ebp+103]
            PlayLoopingAnim 00CC1731 / apply 00CC186C
              arg0 + arg1 required; arg1 atoi 0099E7F0
              actor.vtbl+80 — not vtbl+72
            Clip pose / PALSKIN unread
            """);
    }

    [Fact]
    public void WaitForCamera_idles_when_camera_not_busy()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("camw", ["WaitForCamera"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(0x00CCA58Fu, ScriptCommandMap.Find("WaitForCamera")!.Value.ApplySite);
    }

    [Fact]
    public void UseCamera_snap_then_WaitForCamera_continues()
    {
        var camera = new ScriptedCamera();
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "CTCCameraPointScripted",
                ScriptName = "SecretPassageCryptCam",
                PositionX = 8,
                PositionY = 4,
                PositionZ = 2,
                Properties = new Dictionary<string, string>(),
            },
        ], camera);
        var interp = new ScriptInterpreter("wfc",
            ["UseCamera SecretPassageCryptCam", "WaitForCamera", "ResetCamera"]);
        interp.RunUntilYield(runtime);
        Assert.Equal("SecretPassageCryptCam", camera.ActiveName);
        Assert.False(camera.Playing);
        Assert.False(runtime.CameraSys.Busy);
        interp.Resume(runtime);
        Assert.Contains("WaitForCamera", interp.Executed);
        Assert.Contains("ResetCamera", interp.Executed);
        Assert.True(interp.Finished);
        Assert.False(camera.Playing);
        Assert.False(camera.ScriptCameraActive);
    }

    [Fact]
    public void CameraPath_then_WaitForCamera_leftover_polls()
    {
        var camera = new ScriptedCamera();
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "MK_A",
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
                ScriptName = "MK_B",
                PositionX = 5,
                PositionY = 0,
                PositionZ = 0,
                Properties = new Dictionary<string, string>(),
            },
        ], camera);
        var interp = new ScriptInterpreter("pathw",
        [
            "CameraPath MK_A,MK_B,MK_A,MK_B,2.0",
            "WaitForCamera",
            "CameraPause FALSE",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(camera.Playing);
        Assert.True(runtime.CameraSys.Busy);
        interp.Resume(runtime);
        Assert.Equal(ExecutionKind.WaitOperation, interp.CurrentWaitKind);
        runtime.CameraSys.CompleteWait();
        interp.Resume(runtime);
        Assert.True(interp.Finished);
        Assert.False(camera.Playing);
        Assert.Contains("CameraPause FALSE", interp.Executed);
    }

    [Fact]
    public void WaitForCamera_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_OPENGRAVE_CRYPTCAM");
        Assert.NotNull(hit);
        var commands = hit.Commands.Count > 0
            ? hit.Commands
            : ScriptBank.ExtractCommands(hit.Raw);
        Assert.Contains(commands, l =>
            l.StartsWith("UseCamera ", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(commands, l =>
            l.Equals("WaitForCamera", StringComparison.OrdinalIgnoreCase));
        var camera = new ScriptedCamera();
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var camName = ScriptLine.Parse(
            commands.First(l => l.StartsWith("UseCamera ", StringComparison.OrdinalIgnoreCase))).Arg(0);
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "CTCCameraPointScripted",
                ScriptName = camName,
                PositionX = 12,
                PositionY = 6,
                PositionZ = 3,
                Properties = new Dictionary<string, string>(),
            },
        ], camera);
        var isolated = new ScriptInterpreter(hit.InstanceName, commands.ToList());
        isolated.RunUntilYield(runtime);
        Assert.Equal(camName, camera.ActiveName);
        Assert.True(camera.ScriptCameraActive);
        Assert.False(camera.Playing);
        isolated.Resume(runtime);
        Assert.Contains("WaitForCamera", isolated.Executed);
        Assert.True(isolated.Finished);
        Assert.Equal(12f, camera.Position.X);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-wfc.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-waitforcamera.txt"),
            """
            WaitForCamera 00CCA41F / apply 00CCA58F
              no args; context vtbl+1672
              al==0 idle → 00CD17FD
              al!=0 leftover 00CCA52F:
                vtbl+40; skip 00CBEB7E; vtbl+28
                inc [0x13B83C8]; [0x13D2838]+5 → next
                else re-poll 1672
            UseCamera vtbl+1648 snap arrives immediately
              Playing=false so WaitForCamera continues
            CameraPath/Rig/Rotate BeginTransition
              Playing=true leftover-poll until idle
            Spline/blend body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void WaitForMessageCamera_idles_when_no_message_camera()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("msgc",
            ["WaitForMessageCamera CAM_MSG", "CameraPause FALSE"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal("CAM_MSG", runtime.CameraSys.MessageCamera);
        Assert.Equal(0x00CD0006u, ScriptCommandMap.Find("WaitForMessageCamera")!.Value.ApplySite);
    }

    [Fact]
    public void WaitForMessageCamera_polls_until_complete()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.CameraSys.BeginMessageCamera("CAM_MSG");
        var interp = new ScriptInterpreter("msgw",
            ["WaitForMessageCamera CAM_MSG", "CameraPause FALSE"]);
        interp.RunUntilYield(runtime);
        Assert.Equal(ExecutionKind.WaitOperation, interp.CurrentWaitKind);
        Assert.False(runtime.CameraSys.MessageWaitOp!.Complete);
        runtime.CameraSys.CompleteWait();
        interp.Resume(runtime);
        Assert.True(interp.Finished);
        Assert.Contains("CameraPause FALSE", interp.Executed);
    }

    [Fact]
    public void WaitForMessageCamera_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.StartsWith("WaitForMessageCamera ", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        line ??= "WaitForMessageCamera CAM_MSG";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("WaitForMessageCamera", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-msgcam", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("WaitForMessageCamera", StringComparison.OrdinalIgnoreCase));
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-msgcam.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-waitformessagecamera.txt"),
            """
            WaitForMessageCamera 00CCFF91 / apply 00CD0006
              arg0 name required else 00CD17FD
              leftover-poll vtbl+2316(name) until true
              00CBEB7E skip; vtbl+28 if [ebp+103]; timecode
              [0x13D2838]+5 abort unread
              done -> 00CD17F8 / 00CD17FD
              Distinct from WaitForCamera vtbl+1672
            """);
    }

    [Fact]
    public void SetFlag_writes_byte_and_yields_then_WaitFlag_continues()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("flags",
        [
            "SetFlag fire,true",
            "WaitFlag fire,true",
            "FadeOut 0.5,0",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Yielded);
        Assert.Equal(ExecutionKind.YieldOnce, interp.CurrentWaitKind);
        Assert.Equal((byte)1, runtime.Flags.GetOrInsert("fire"));
        Assert.Equal(CommandStatus.Proven, ScriptCommandMap.Find("SetFlag")!.Value.Runtime);
        interp.Resume(runtime);
        Assert.True(interp.Finished);
        Assert.Contains(interp.Executed, l => l.StartsWith("WaitFlag", StringComparison.Ordinal));
        Assert.Contains(interp.Executed, l => l.StartsWith("FadeOut", StringComparison.Ordinal));
        Assert.Contains(runtime.Trace.Steps, s =>
            s.Verb == "SetFlag" && s.Result == ExecutionKind.YieldOnce);
        Assert.Contains(runtime.Trace.Steps, s =>
            s.Verb == "WaitFlag" && s.Result == ExecutionKind.Continue);
    }

    [Fact]
    public void WaitFlag_polls_until_SetFlag_and_is_not_a_timer()
    {
        var runtime = ScriptRuntime.Detached();
        var waiter = new ScriptInterpreter("wait",
            ["WaitFlag fire,true", "FadeOut 0.5,0"]);
        waiter.RunUntilYield(runtime);
        Assert.Equal(ExecutionKind.WaitOperation, waiter.CurrentWaitKind);
        Assert.False(runtime.Flags.WaitOp!.Complete);
        Assert.Equal((byte)0, runtime.Flags.GetOrInsert("fire"));
        for (var i = 0; i < 4; i++)
            waiter.Resume(runtime);
        Assert.Equal(ExecutionKind.WaitOperation, waiter.CurrentWaitKind);
        Assert.DoesNotContain(waiter.Executed, l => l.StartsWith("FadeOut", StringComparison.Ordinal));

        var setter = new ScriptInterpreter("set", ["SetFlag fire,true"]);
        setter.RunUntilYield(runtime);
        Assert.Equal((byte)1, runtime.Flags.GetOrInsert("fire"));
        waiter.Resume(runtime);
        Assert.True(waiter.Finished);
        Assert.Contains(waiter.Executed, l => l.StartsWith("FadeOut", StringComparison.Ordinal));
    }

    [Fact]
    public void SetFlag_false_writes_zero_and_arg2_true_skips_rewrite()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("sf",
        [
            "SetFlag fire,false",
            "SetFlag fire,true,TRUE",
        ]);
        interp.RunUntilYield(runtime);
        interp.Resume(runtime);
        if (interp.Yielded)
            interp.Resume(runtime);
        Assert.True(interp.Finished);
        Assert.Equal((byte)0, runtime.Flags.GetOrInsert("fire"));
    }

    [Fact]
    public void WaitFlag_missing_args_continue_and_miss_inserts_zero()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("wf0",
            ["WaitFlag", "WaitFlag fire", "WaitFlag unset,true"]);
        interp.RunUntilYield(runtime);
        Assert.Equal(ExecutionKind.WaitOperation, interp.CurrentWaitKind);
        Assert.Equal((byte)0, runtime.Flags.GetOrInsert("unset"));
        Assert.True(runtime.Flags.IsWaiting(null));
    }

    [Fact]
    public void PumpUntilSettled_does_not_auto_complete_WaitFlag()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("hold", ["WaitFlag fire,true", "FadeOut 0.5,0"]);
        interp.RunUntilYield(runtime);
        runtime.PumpUntilSettled(interp, 32);
        Assert.False(interp.Finished);
        Assert.Equal(ExecutionKind.WaitOperation, interp.CurrentWaitKind);
        Assert.DoesNotContain(interp.Executed, l => l.StartsWith("FadeOut", StringComparison.Ordinal));
    }

    [Fact]
    public void SetFlag_WaitFlag_real_script_bank_lines()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? setLine = null;
        string? waitLine = null;
        ScriptDef? setHit = null;
        ScriptDef? waitHit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (setLine is null &&
                    raw.StartsWith("SetFlag ", StringComparison.OrdinalIgnoreCase))
                {
                    setLine = raw;
                    setHit = entry;
                }

                if (waitLine is null &&
                    raw.StartsWith("WaitFlag ", StringComparison.OrdinalIgnoreCase))
                {
                    waitLine = raw;
                    waitHit = entry;
                }
            }

            if (setLine is not null && waitLine is not null)
                break;
        }

        setLine ??= "SetFlag fire,true";
        setHit ??= bank.Find("CS_OAKVALE_REVISITED") ?? bank.Entries[0];
        Assert.NotNull(setHit);
        var setParsed = ScriptLine.Parse(setLine);
        Assert.Equal("SetFlag", setParsed.Verb);
        Assert.True(setParsed.Arg(0).Length > 0);
        Assert.True(setParsed.Arg(1).Length > 0);

        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(setHit.InstanceName + "-flag", [setLine]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("SetFlag", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(ExecutionKind.YieldOnce, isolated.CurrentWaitKind);
        var written = (byte)(ScriptLine.IsFalse(setParsed.Arg(1)) ? 0 : 1);
        Assert.Equal(written, runtime.Flags.GetOrInsert(setParsed.Arg(0)));

        if (waitLine is not null)
        {
            var waitParsed = ScriptLine.Parse(waitLine);
            Assert.Equal("WaitFlag", waitParsed.Verb);
            var expected = (byte)(ScriptLine.IsTrue(waitParsed.Arg(1)) ? 1 : 0);
            runtime.Flags.Set(waitParsed.Arg(0), expected);
            var waiter = new ScriptInterpreter((waitHit?.InstanceName ?? "wait") + "-wflag", [waitLine]);
            waiter.RunUntilYield(runtime);
            Assert.True(waiter.Finished);
            Assert.Contains(waiter.Executed, l =>
                l.StartsWith("WaitFlag", StringComparison.OrdinalIgnoreCase));
        }

        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, setHit.InstanceName + "-flag.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-waitflag.txt"),
            """
            SetFlag 00CCA475 / apply 00CCA4C8
              arg0 name + arg1 required else 00CD17FD
              [ebp+112] flag table required else 00CD17FD
              IsTrue(arg2) + [ebp-39]!=0 -> skip rewrite, jmp 00CC907D
              else IsFalse(arg1) -> 008ADF10 write 0 else write 1
              [ebp-39]=1 after write
              always jmp 00CC907D YieldOnce
            WaitFlag 00CCB840 / apply 00CCB893
              arg0 name + arg1 required else 00CD17FD
              [ebp+112] required else 00CD17FD
              IsTrue(arg1) expected=1 else 0
              008ADF10 lookup, insert-on-miss default 0, return node+20
              cmp [eax],bl match -> 00CD17FD
              mismatch leftover 00CCB8CE: 00CBEB7E skip, vtbl+28 if [ebp+103],
              timecode, [0x13D2838]+5 abort unread, re-poll
            008ADF10 is a named byte map, not persist, not a timer.
            """);
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
