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
        Assert.True(runtime.World.ExtraLimbo);
        Assert.False(runtime.World.ExtraReturn);
        Assert.Equal(1812, runtime.World.ExtraDrawVtbl);
        Assert.True(interp.Finished);
    }

    [Fact]
    public void Return_token_is_RemoveExtras_named_arg_not_verb()
    {
        var runtime = ScriptRuntime.Detached();
        var verb = new ScriptInterpreter("retverb", ["return", "FadeOut 0.5,0"]);
        verb.RunUntilYield(runtime);
        Assert.True(verb.Blocked);
        Assert.Equal("UNKNOWN", verb.BlockReason);
        Assert.Equal(0x00CC6BC4u, ScriptCommandMap.Find("return")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("RemoveExtras")!.Value.ApplySite,
            ScriptCommandMap.Find("return")!.Value.ApplySite);

        var show = new ScriptInterpreter("retshow",
        [
            "RemoveExtras TRUE,LIMBO",
            "RemoveExtras FALSE,RETURN",
        ]);
        show.RunUntilYield(runtime);
        Assert.True(show.Finished);
        Assert.False(runtime.World.ExtrasHidden);
        Assert.True(runtime.World.ExtraReturn);
        Assert.False(runtime.World.ExtraLimbo);
        Assert.Equal(2044, runtime.World.ExtraDrawVtbl);
        Assert.Equal(1892, runtime.World.ExtraReturnVtbl);
        var empty = new ScriptInterpreter("retempty", ["RemoveExtras"]);
        empty.RunUntilYield(runtime);
        Assert.True(empty.Finished);
        Assert.True(runtime.World.ExtrasHidden);
    }

    [Fact]
    public void RemoveExtras_FALSE_RETURN_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var name in new[]
                 {
                     "CS_OAKVALE_INTRO_THERESA",
                     "CS_OAKVALE_INTRO_THERESA_MEET",
                 })
        {
            hit = bank.Entries.FirstOrDefault(e =>
                e.InstanceName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (hit is null)
                continue;
            foreach (var raw in hit.Commands.Count > 0
                         ? hit.Commands
                         : ScriptBank.ExtractCommands(hit.Raw))
            {
                if (raw.StartsWith("RemoveExtras ", StringComparison.OrdinalIgnoreCase) &&
                    raw.Contains("RETURN", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        if (line is null)
        {
            foreach (var entry in bank.Entries)
            {
                foreach (var raw in entry.Commands.Count > 0
                             ? entry.Commands
                             : ScriptBank.ExtractCommands(entry.Raw))
                {
                    if (raw.StartsWith("RemoveExtras ", StringComparison.OrdinalIgnoreCase) &&
                        raw.Contains("RETURN", StringComparison.OrdinalIgnoreCase) &&
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
        }

        Assert.False(string.IsNullOrEmpty(line));
        Assert.NotNull(hit);
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("RemoveExtras", parsed.Verb);
        Assert.True(ScriptLine.TokenMatches(parsed.Arg(1), "return"));
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-rex", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("RemoveExtras ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Equal(!ScriptLine.IsFalse(parsed.Arg(0)), runtime.World.ExtrasHidden);
        Assert.True(runtime.World.ExtraReturn);
        Assert.Equal(1892, runtime.World.ExtraReturnVtbl);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-rex.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-return.txt"),
            """
            return 00CC6B82 / apply 00CC6BC4  (NOT a verb)
              00BFEBA8 named-arg vs RemoveExtras arg1
              match → [ebp+19]=1; skip 008AB980
              consumer 00CC6F74: show+return vtbl+1892
              DISPROVES interpreter-stop
            RemoveExtras 00CC6ACE / apply 00CC6B21
              hide=!IsFalse(arg0); empty hides
              00BFEBA8 limbo → [ebp+127] vtbl+1812
              00BFEBA8 return → [ebp+19] show 1892
              else marker 00CBF9DE+008AB980 park 1892
              not-limbo draw vtbl+2044
            Extras list body UNREAD (Runtime PARTIAL)
            """);
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
    public void TakeObjectFromHero_takes_one_not_the_slot()
    {
        var runtime = ScriptRuntime.Detached();
        var first = new ScriptInterpreter("tofh",
        [
            "GiveHero OBJECT_MANA_POTION,2",
            "TakeObjectFromHero OBJECT_MANA_POTION",
        ]);
        first.RunUntilYield(runtime);
        Assert.True(first.Finished);
        Assert.Single(runtime.World.Inventory);
        Assert.Equal(1, runtime.World.Inventory[0].Count);
        runtime.World.HeroHands = "OBJECT_MANA_POTION";
        var second = new ScriptInterpreter("tofh2",
            ["TakeObjectFromHero OBJECT_MANA_POTION"]);
        second.RunUntilYield(runtime);
        Assert.Empty(runtime.World.Inventory);
        Assert.Equal("", runtime.World.HeroHands);
        Assert.Equal(2, runtime.World.TakenObjects.Count);
        Assert.Equal(0x00CC8898u, ScriptCommandMap.Find("TakeObjectFromHero")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("TakeFromHero")!.Value.ApplySite,
            ScriptCommandMap.Find("TakeObjectFromHero")!.Value.ApplySite);
    }

    [Fact]
    public void TakeObjectFromHero_real_script_bank_line()
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
                if (raw.StartsWith("TakeObjectFromHero ", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "TakeObjectFromHero OBJECT_PRISON_CELL_KEY";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("TakeObjectFromHero", parsed.Verb);
        Assert.True(parsed.Arg(0).Length > 0);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        runtime.World.GiveHero(parsed.Arg(0), 1);
        runtime.World.HeroHands = parsed.Arg(0);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-tofh", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("TakeObjectFromHero ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Empty(runtime.World.Inventory);
        Assert.Equal("", runtime.World.HeroHands);
        Assert.Equal(parsed.Arg(0), runtime.World.TakenObjects[0]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-tofh.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-takeobjectfromhero.txt"),
            """
            TakeObjectFromHero 00CC8846 / apply 00CC8898
              arg0 required else 00CD17FD
              vtbl+500(name); jmp 00CD17FD no yield
              not TakeFromHero vtbl+556 (whole slot)
            Object-def / drop body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void PutUpYourSwords_false_still_sheathes()
    {
        var runtime = ScriptRuntime.Detached();
        var down = new ScriptInterpreter("pus0", ["PutUpYourSwords FALSE"]);
        down.RunUntilYield(runtime);
        Assert.True(runtime.World.SwordsUp);
        Assert.False(runtime.World.SwordClassifyRequested);
        var up = new ScriptInterpreter("pus1", ["PutUpYourSwords TRUE"]);
        up.RunUntilYield(runtime);
        Assert.True(runtime.World.SwordsUp);
        Assert.True(runtime.World.SwordClassifyRequested);
        Assert.Equal(0x00CC9356u, ScriptCommandMap.Find("PutUpYourSwords")!.Value.ApplySite);
    }

    [Fact]
    public void PutUpYourSwords_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_ARENA_HOH_INTRO") ?? bank.Find("CS_GUILD_MELEE_INTRO");
        Assert.NotNull(hit);
        string? line = null;
        foreach (var raw in hit.Commands.Count > 0
                     ? hit.Commands
                     : ScriptBank.ExtractCommands(hit.Raw))
        {
            if (raw.StartsWith("PutUpYourSwords", StringComparison.OrdinalIgnoreCase))
            {
                line = raw;
                break;
            }
        }

        Assert.False(string.IsNullOrEmpty(line));
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        runtime.World.SwordsUp = false;
        var isolated = new ScriptInterpreter(hit.InstanceName + "-swords", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("PutUpYourSwords", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.True(runtime.World.SwordsUp);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-swords.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-putupyourswords.txt"),
            """
            PutUpYourSwords 00CC9300 / apply 00CC9356
              IsTrue(arg0) → vtbl+280 hero, vtbl+788 MELEE else vtbl+792 RANGED
              always vtbl+520 sheathe; jmp 00CD17FD
              FALSE/empty still sheathes (DISPROVES SwordsUp=!IsFalse)
            Classify 788/792 bodies UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void GiveGold_ensures_at_least_requested()
    {
        var runtime = ScriptRuntime.Detached();
        var first = new ScriptInterpreter("gg1", ["GiveGold 100"]);
        first.RunUntilYield(runtime);
        Assert.Equal(100, runtime.World.HeroGold);
        var again = new ScriptInterpreter("gg2", ["GiveGold 50"]);
        again.RunUntilYield(runtime);
        Assert.Equal(100, runtime.World.HeroGold);
        var more = new ScriptInterpreter("gg3", ["GiveGold 250"]);
        more.RunUntilYield(runtime);
        Assert.Equal(250, runtime.World.HeroGold);
        Assert.Equal(0x00CC8348u, ScriptCommandMap.Find("GiveGold")!.Value.ApplySite);
    }

    [Fact]
    public void Sheathe_melee_is_vtbl_2032_not_PutUpYourSwords()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("sh",
        [
            "HERO.Sheathe MELEE",
            "SCYTHE.Sheathe none",
            "WHISPER.Sheathe TRUE",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal("MELEE", runtime.World.Sheathed["HERO"]);
        Assert.Equal(2032, runtime.World.SheatheVtbl["HERO"]);
        Assert.Equal("none", runtime.World.Sheathed["SCYTHE"]);
        Assert.Equal(2024, runtime.World.SheatheVtbl["SCYTHE"]);
        Assert.Equal("TRUE", runtime.World.Sheathed["WHISPER"]);
        Assert.Equal(0, runtime.World.SheatheVtbl["WHISPER"]);
        Assert.Equal(0x00CC37F8u, ScriptCommandMap.Find("Sheathe")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("PutUpYourSwords")!.Value.ApplySite,
            ScriptCommandMap.Find("Sheathe")!.Value.ApplySite);
    }

    [Fact]
    public void Sheathe_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_BANDITKING_THERESA") ?? bank.Find("CS_JACK_DEATH");
        Assert.NotNull(hit);
        string? line = null;
        foreach (var raw in hit.Commands.Count > 0
                     ? hit.Commands
                     : ScriptBank.ExtractCommands(hit.Raw))
        {
            if (raw.Contains(".Sheathe ", StringComparison.OrdinalIgnoreCase))
            {
                line = raw;
                break;
            }
        }

        Assert.False(string.IsNullOrEmpty(line));
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("Sheathe", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-sheathe", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".Sheathe ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Equal(parsed.Arg(0), runtime.World.Sheathed[parsed.Target ?? ""]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-sheathe.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-givegold-sheathe.txt"),
            """
            GiveGold 00CC82F5 / apply 00CC8348
              arg0 atoi; empty skip
              00515700 lookup "Gold"; if have, edi -= count
              edi<=0 skip; else vtbl+504(edi)
            Sheathe 00CC37A2 / apply 00CC37F8
              melee vtbl+2032; ranged 2036; false 2028; none 2024
              TRUE/other no extra vtbl; jmp 00CC2C6B
              not PutUpYourSwords vtbl+520
            Gold/sheathe mesh UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void HoldInHand_is_vtbl_892_not_PutInHeroHands()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("hih",
        [
            "SMITH.HoldInHand OBJECT_HAMMER_SMITH,TRUE",
            "M1.HoldInHand OBJECT_KHG_LOG_PILE_03,TRUE",
            "GUARD.HoldInHand OBJECT_SWORD",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal("OBJECT_HAMMER_SMITH", runtime.World.HeldInHand["SMITH"]);
        Assert.True(runtime.World.HeldInHandFlag["SMITH"]);
        Assert.Equal("OBJECT_KHG_LOG_PILE_03", runtime.World.HeldInHand["M1"]);
        Assert.True(runtime.World.HeldInHandFlag["M1"]);
        Assert.Equal("OBJECT_SWORD", runtime.World.HeldInHand["GUARD"]);
        Assert.False(runtime.World.HeldInHandFlag["GUARD"]);
        Assert.Equal("", runtime.World.HeroHands);
        Assert.Equal(0x00CC21CBu, ScriptCommandMap.Find("HoldInHand")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("PutInHeroHands")!.Value.ApplySite,
            ScriptCommandMap.Find("HoldInHand")!.Value.ApplySite);
        var empty = new ScriptInterpreter("hih0", ["SMITH.HoldInHand"]);
        empty.RunUntilYield(runtime);
        Assert.True(empty.Finished);
        Assert.Equal("OBJECT_HAMMER_SMITH", runtime.World.HeldInHand["SMITH"]);
    }

    [Fact]
    public void HoldInHand_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_ATTRACT_3") ?? bank.Find("CS_ATTRACT_12");
        Assert.NotNull(hit);
        string? line = null;
        foreach (var raw in hit.Commands.Count > 0
                     ? hit.Commands
                     : ScriptBank.ExtractCommands(hit.Raw))
        {
            if (raw.Contains(".HoldInHand ", StringComparison.OrdinalIgnoreCase))
            {
                line = raw;
                break;
            }
        }

        Assert.False(string.IsNullOrEmpty(line));
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("HoldInHand", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-hold", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".HoldInHand ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Equal(parsed.Arg(0), runtime.World.HeldInHand[parsed.Target ?? ""]);
        Assert.Equal(ScriptLine.IsTrue(parsed.Arg(1)),
            runtime.World.HeldInHandFlag[parsed.Target ?? ""]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-hold.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-holdinhand.txt"),
            """
            HoldInHand 00CC2175 / apply 00CC21CB
              ebx required else 00CC7081
              arg0 00403A00 empty skip 00CC7081
              arg1 00CBEDBA IsTrue
              actor vtbl+48 name
              CGameScriptInterface vtbl+892(actor,item,flag)
              [ebp+103] leftover vtbl+28; 00CBF7FE; jmp 00CC707C
              not PutInHeroHands vtbl+572/568
            Attach mesh UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void ModifyHealth_is_vtbl_1060_not_GiveHeroHealth()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.World.HeroHealth = 50;
        runtime.World.HeroMaxHealth = 100;
        var interp = new ScriptInterpreter("mh",
        [
            "GUARD.ModifyHealth -25",
            "GUARD.ModifyHealth 10",
            "HERO.ModifyHealth 5",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(-15f, runtime.World.Health["GUARD"]);
        Assert.Equal(5f, runtime.World.Health["HERO"]);
        Assert.Equal(50f, runtime.World.HeroHealth);
        Assert.Equal(0x00CC22AEu, ScriptCommandMap.Find("ModifyHealth")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("GiveHeroHealth")!.Value.ApplySite,
            ScriptCommandMap.Find("ModifyHealth")!.Value.ApplySite);
        var empty = new ScriptInterpreter("mh0", ["GUARD.ModifyHealth"]);
        empty.RunUntilYield(runtime);
        Assert.True(empty.Finished);
        Assert.Equal(-15f, runtime.World.Health["GUARD"]);
    }

    [Fact]
    public void ModifyHealth_real_script_bank_or_isolated()
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
                if (raw.Contains(".ModifyHealth ", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "BANDIT.ModifyHealth -50";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("ModifyHealth", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-hp", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".ModifyHealth ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        ScriptLine.TryFloat(parsed.Arg(0), out var amount);
        Assert.Equal(amount, runtime.World.Health[parsed.Target ?? ""]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-hp.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-modifyhealth.txt"),
            """
            ModifyHealth 00CC2258 / apply 00CC22AE
              ebx required else 00CC7081
              arg0 00403A00 empty skip; 0099E690 atof
              actor vtbl+48 name
              CGameScriptInterface vtbl+1060(actor,amt,0)
              leftover +28; 00CBF7FE; jmp 00CC707C
              not GiveHeroHealth vtbl+1052 / MAX
            Health mesh / clamp UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void SetScared_default_on_IsFalse_off_not_SetBound()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("ss",
        [
            "TRADERS.SetScared FALSE",
            "FISH.SetScared TRUE",
            "FARMER.SetScared",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.False(runtime.World.Scared["TRADERS"]);
        Assert.True(runtime.World.Scared["FISH"]);
        Assert.True(runtime.World.Scared["FARMER"]);
        Assert.Equal(0x00CC12B7u, ScriptCommandMap.Find("SetScared")!.Value.ApplySite);
        Assert.False(ScriptCommandMap.Find("SetBound") is { ApplySite: 0x00CC12B7 });
    }

    [Fact]
    public void SetScared_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_DARKWOOD_TRADER_INFECTED_SCARED")
                  ?? bank.Find("CS_FISHCOMP_HIT2")
                  ?? bank.Find("CS_ORCHARD_EVIL_OUTRO");
        Assert.NotNull(hit);
        string? line = null;
        foreach (var raw in hit.Commands.Count > 0
                     ? hit.Commands
                     : ScriptBank.ExtractCommands(hit.Raw))
        {
            if (raw.Contains(".SetScared", StringComparison.OrdinalIgnoreCase))
            {
                line = raw;
                break;
            }
        }

        Assert.False(string.IsNullOrEmpty(line));
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("SetScared", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-scared", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".SetScared", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Equal(!ScriptLine.IsFalse(parsed.Arg(0)),
            runtime.World.Scared[parsed.Target ?? ""]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-scared.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-setscared.txt"),
            """
            SetScared 00CC1265 / apply 00CC12B7
              ebx required else 00CC7081
              default flag=1; 00CBEE0C IsFalse(arg0) → 0
              empty arg stays 1 (no 00403A00 skip)
              actor vtbl+48; vtbl+1984(actor,flag); jmp 00CC707C
              SetBound is vtbl+1976 and requires arg0
              Killable is vtbl+2068(actor,flag,1)
            AI reaction UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void SetBound_requires_arg_and_is_vtbl_1976()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("sb",
        [
            "PRIS.SetBound TRUE",
            "PRIS.SetBound FALSE",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.False(runtime.World.Bound["PRIS"]);
        var empty = new ScriptInterpreter("sb0", ["PRIS.SetBound"]);
        empty.RunUntilYield(runtime);
        Assert.True(empty.Finished);
        Assert.False(runtime.World.Bound["PRIS"]);
        var on = new ScriptInterpreter("sb1", ["GUARD.SetBound TRUE"]);
        on.RunUntilYield(runtime);
        Assert.True(runtime.World.Bound["GUARD"]);
        Assert.Equal(0x00CC11FDu, ScriptCommandMap.Find("SetBound")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("SetScared")!.Value.ApplySite,
            ScriptCommandMap.Find("SetBound")!.Value.ApplySite);
    }

    [Fact]
    public void SetBound_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_HANGINGTREE_EVIL_OUTRO")
                  ?? bank.Find("CS_HANGINGTREE_EVIL_EXECUTION");
        Assert.NotNull(hit);
        string? line = null;
        foreach (var raw in hit.Commands.Count > 0
                     ? hit.Commands
                     : ScriptBank.ExtractCommands(hit.Raw))
        {
            if (raw.Contains(".SetBound ", StringComparison.OrdinalIgnoreCase))
            {
                line = raw;
                break;
            }
        }

        Assert.False(string.IsNullOrEmpty(line));
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("SetBound", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-bound", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".SetBound ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Equal(!ScriptLine.IsFalse(parsed.Arg(0)),
            runtime.World.Bound[parsed.Target ?? ""]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-bound.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-setbound.txt"),
            """
            SetBound 00CC11AB / apply 00CC11FD
              ebx required else 00CC7081
              arg0 00403A00 empty skip 00CC7081
              default flag=1; 00CBEE0C IsFalse → 0
              actor vtbl+48; vtbl+1976(actor,flag); jmp 00CC707C
              not SetScared vtbl+1984 (optional arg)
            Bind pose UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void Killable_requires_arg_and_is_vtbl_2068()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("k",
        [
            "GUARD.Killable TRUE",
            "GUARD.Killable FALSE",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.False(runtime.World.Killable["GUARD"]);
        Assert.Equal(1, runtime.World.KillableExtra["GUARD"]);
        var empty = new ScriptInterpreter("k0", ["GUARD.Killable"]);
        empty.RunUntilYield(runtime);
        Assert.True(empty.Finished);
        Assert.False(runtime.World.Killable["GUARD"]);
        var on = new ScriptInterpreter("k1", ["BANDIT.Killable TRUE"]);
        on.RunUntilYield(runtime);
        Assert.True(runtime.World.Killable["BANDIT"]);
        Assert.Equal(1, runtime.World.KillableExtra["BANDIT"]);
        Assert.Equal(0x00CC1C82u, ScriptCommandMap.Find("Killable")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("SetBound")!.Value.ApplySite,
            ScriptCommandMap.Find("Killable")!.Value.ApplySite);
    }

    [Fact]
    public void Killable_real_script_bank_or_isolated()
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
                if (raw.Contains(".Killable ", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "GUARD.Killable FALSE";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("Killable", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-kill", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".Killable ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Equal(!ScriptLine.IsFalse(parsed.Arg(0)),
            runtime.World.Killable[parsed.Target ?? ""]);
        Assert.Equal(1, runtime.World.KillableExtra[parsed.Target ?? ""]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-kill.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-killable.txt"),
            """
            Killable 00CC1C30 / apply 00CC1C82
              ebx required else 00CC7081
              arg0 00403A00 empty skip 00CC7081
              default flag=1; 00CBEE0C IsFalse → 0
              actor vtbl+48; push 1; vtbl+2068(actor,flag,1)
              jmp 00CC707C
              not SetBound vtbl+1976; not SetScared 1984
              SetPushable is IsTrue default 0 vtbl+3376
            Death/AI body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void SetPushable_IsTrue_default_off_not_SetBound()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("sp",
        [
            "SCARED.SetPushable FALSE",
            "JACK.SetPushable TRUE",
            "MUM.SetPushable",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.False(runtime.World.Pushable["SCARED"]);
        Assert.True(runtime.World.Pushable["JACK"]);
        Assert.False(runtime.World.Pushable["MUM"]);
        Assert.Equal(0x00CC1144u, ScriptCommandMap.Find("SetPushable")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("SetBound")!.Value.ApplySite,
            ScriptCommandMap.Find("SetPushable")!.Value.ApplySite);
    }

    [Fact]
    public void SetPushable_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var name in new[]
                 {
                     "CS_JACK_BOSS_INTRO", "CS_FOCALSITE_4",
                     "CS_OAKVALE_INTRO_THERESA",
                 })
        {
            var entry = bank.Find(name);
            if (entry is null)
                continue;
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (raw.Contains(".SetPushable", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        Assert.False(string.IsNullOrEmpty(line));
        Assert.NotNull(hit);
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("SetPushable", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-push", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".SetPushable", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Equal(ScriptLine.IsTrue(parsed.Arg(0)),
            runtime.World.Pushable[parsed.Target ?? ""]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-push.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-setpushable.txt"),
            """
            SetPushable 00CC10F2 / apply 00CC1144
              ebx required else 00CC7081
              default flag=0; 00CBEDBA IsTrue(arg0) → 1
              empty stays 0 (no 00403A00 skip)
              actor vtbl+48; 004ABE90; vtbl+3376; jmp 00CC707C
              not SetBound IsFalse/default 1/vtbl+1976
              SetDamageable is vtbl+2064(actor,0)+008ADF90, ignores arg
            Physics body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void SetDamageable_ignores_arg_and_is_vtbl_2064()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("sd",
        [
            "HERO.SetDamageable FALSE",
            "GUARD.SetDamageable TRUE",
            "BANDIT.SetDamageable",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.False(runtime.World.Damageable["HERO"]);
        Assert.False(runtime.World.Damageable["GUARD"]);
        Assert.False(runtime.World.Damageable["BANDIT"]);
        Assert.Equal(2064, runtime.World.DamageableVtbl["HERO"]);
        Assert.Equal(2064, runtime.World.DamageableVtbl["GUARD"]);
        Assert.Contains("HERO", runtime.World.ExtrasAppended);
        Assert.Contains("GUARD", runtime.World.ExtrasAppended);
        Assert.Equal(0x00CC10A6u, ScriptCommandMap.Find("SetDamageable")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("Killable")!.Value.ApplySite,
            ScriptCommandMap.Find("SetDamageable")!.Value.ApplySite);
    }

    [Fact]
    public void SetDamageable_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_WASPBOSS_QUEEN");
        Assert.NotNull(hit);
        string? line = null;
        foreach (var raw in hit.Commands.Count > 0
                     ? hit.Commands
                     : ScriptBank.ExtractCommands(hit.Raw))
        {
            if (raw.Contains(".SetDamageable", StringComparison.OrdinalIgnoreCase))
            {
                line = raw;
                break;
            }
        }

        Assert.False(string.IsNullOrEmpty(line));
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("SetDamageable", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-dmg", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".SetDamageable", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.False(runtime.World.Damageable[parsed.Target ?? ""]);
        Assert.Equal(2064, runtime.World.DamageableVtbl[parsed.Target ?? ""]);
        Assert.Contains(parsed.Target ?? "", runtime.World.ExtrasAppended);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-dmg.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-setdamageable.txt"),
            """
            SetDamageable 00CC1054 / apply 00CC10A6
              ebx required else 00CC7081
              NO arg parse — FALSE/TRUE/empty ignored
              actor vtbl+48; vtbl+2064(actor,0); extras 008ADF90
              jmp 00CC707C
              DISPROVES IsFalse(arg0) flag lump
              not Killable vtbl+2068(actor,flag,1)
              SetAttackable sibling is vtbl+1832(actor,0)+008ADF90
            Damage mesh UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void SetAttackable_ignores_arg_and_is_vtbl_1832()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("sa",
        [
            "HERO.SetAttackable FALSE",
            "GUARD.SetAttackable TRUE",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.False(runtime.World.Attackable["HERO"]);
        Assert.False(runtime.World.Attackable["GUARD"]);
        Assert.Equal(1832, runtime.World.AttackableVtbl["HERO"]);
        Assert.Equal(0x00CC1008u, ScriptCommandMap.Find("SetAttackable")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("SetDamageable")!.Value.ApplySite,
            ScriptCommandMap.Find("SetAttackable")!.Value.ApplySite);
    }

    [Fact]
    public void SetAttackable_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_HAUNTED_CELLAROPENS");
        Assert.NotNull(hit);
        string? line = null;
        foreach (var raw in hit.Commands.Count > 0
                     ? hit.Commands
                     : ScriptBank.ExtractCommands(hit.Raw))
        {
            if (raw.Contains(".SetAttackable", StringComparison.OrdinalIgnoreCase))
            {
                line = raw;
                break;
            }
        }

        Assert.False(string.IsNullOrEmpty(line));
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("SetAttackable", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-atk", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".SetAttackable", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.False(runtime.World.Attackable[parsed.Target ?? ""]);
        Assert.Equal(1832, runtime.World.AttackableVtbl[parsed.Target ?? ""]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-atk.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-setattackable.txt"),
            """
            SetAttackable 00CC0FB6 / apply 00CC1008
              ebx required else 00CC7081
              NO arg parse — FALSE/TRUE ignored
              actor vtbl+48; vtbl+1832(actor,0); extras 008ADF90
              jmp 00CC707C
              not SetDamageable vtbl+2064
            Combat lock UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void SetFree_is_unary_vtbl_1980_not_SetAttackable()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("sf",
        [
            "HOST1.SetFree TRUE",
            "HOST2.SetFree FALSE",
            "HOST3.SetFree",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Contains("HOST1", runtime.World.Freed);
        Assert.Contains("HOST2", runtime.World.Freed);
        Assert.Contains("HOST3", runtime.World.Freed);
        Assert.Equal(1980, runtime.World.SetFreeVtbl["HOST1"]);
        Assert.Equal(1980, runtime.World.SetFreeVtbl["HOST2"]);
        Assert.DoesNotContain("HOST1", runtime.World.ExtrasAppended);
        Assert.Equal(0x00CC0F7Eu, ScriptCommandMap.Find("SetFree")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("SetAttackable")!.Value.ApplySite,
            ScriptCommandMap.Find("SetFree")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("SetBound")!.Value.ApplySite,
            ScriptCommandMap.Find("SetFree")!.Value.ApplySite);
    }

    [Fact]
    public void SetFree_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_BANDITCAMP_HOSTAGEFREE")
                  ?? bank.Find("CS_BANDITCAMP_HOSTAGEFREE_NOTOPEN");
        Assert.NotNull(hit);
        string? line = null;
        foreach (var raw in hit.Commands.Count > 0
                     ? hit.Commands
                     : ScriptBank.ExtractCommands(hit.Raw))
        {
            if (raw.Contains(".SetFree", StringComparison.OrdinalIgnoreCase))
            {
                line = raw;
                break;
            }
        }

        Assert.False(string.IsNullOrEmpty(line));
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("SetFree", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-free", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".SetFree", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Contains(parsed.Target ?? "", runtime.World.Freed);
        Assert.Equal(1980, runtime.World.SetFreeVtbl[parsed.Target ?? ""]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-free.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-setfree.txt"),
            """
            SetFree 00CC0F2C / apply 00CC0F7E
              ebx required else 00CC7081
              NO arg parse — TRUE/FALSE/empty ignored
              actor vtbl+48; unary vtbl+1980(actor)
              no extras 008ADF90
              jmp 00CC707C
              not SetAttackable vtbl+1832(actor,0)+extras
              not SetBound 1976 / SetScared 1984
            Hostage/AI release body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void SetAppearanceSeed_is_atoi_vtbl_1916()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("sas",
        [
            "WOMAN.SetAppearanceSeed 20",
            "Girl.SetAppearanceSeed -1293062401",
            "UNDEAD.SetAppearanceSeed",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(20, runtime.World.AppearanceSeed["WOMAN"]);
        Assert.Equal(-1293062401, runtime.World.AppearanceSeed["Girl"]);
        Assert.Equal(0, runtime.World.AppearanceSeed["UNDEAD"]);
        Assert.Equal(0x00CC4B7Eu, ScriptCommandMap.Find("SetAppearanceSeed")!.Value.ApplySite);
    }

    [Fact]
    public void SetAppearanceSeed_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_ATTRACT_1") ?? bank.Find("CS_ATTRACT_5");
        Assert.NotNull(hit);
        string? line = null;
        foreach (var raw in hit.Commands.Count > 0
                     ? hit.Commands
                     : ScriptBank.ExtractCommands(hit.Raw))
        {
            if (raw.Contains(".SetAppearanceSeed ", StringComparison.OrdinalIgnoreCase))
            {
                line = raw;
                break;
            }
        }

        Assert.False(string.IsNullOrEmpty(line));
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("SetAppearanceSeed", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-seed", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".SetAppearanceSeed ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        ScriptLine.TryInt(parsed.Arg(0), out var seed);
        Assert.Equal(seed, runtime.World.AppearanceSeed[parsed.Target ?? ""]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-seed.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-setappearanceseed.txt"),
            """
            SetAppearanceSeed 00CC4B2C / apply 00CC4B7E
              ebx required else 00CC7081
              actor vtbl+48; 0099E7F0 atoi(arg0) — signed
              004AB130 name-valid; miss skip vtbl
              vtbl+1916(actor,seed); jmp 00CC707C
              not a boolean flag lump
            PALSKIN appearance UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void SetDrunk_default_on_IsFalse_off_not_SetScared()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("sd",
        [
            "DRUNK.SetDrunk FALSE",
            "PATRON.SetDrunk TRUE",
            "HERO.SetDrunk",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.False(runtime.World.Drunk["DRUNK"]);
        Assert.True(runtime.World.Drunk["PATRON"]);
        Assert.True(runtime.World.Drunk["HERO"]);
        Assert.Equal(0x00CC1360u, ScriptCommandMap.Find("SetDrunk")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("SetScared")!.Value.ApplySite,
            ScriptCommandMap.Find("SetDrunk")!.Value.ApplySite);
    }

    [Fact]
    public void SetDrunk_real_script_bank_or_isolated()
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
                if (raw.Contains(".SetDrunk", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "PATRON.SetDrunk TRUE";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("SetDrunk", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-drunk", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".SetDrunk", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Equal(!ScriptLine.IsFalse(parsed.Arg(0)),
            runtime.World.Drunk[parsed.Target ?? ""]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-drunk.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-setdrunk.txt"),
            """
            SetDrunk 00CC130E / apply 00CC1360
              ebx required else 00CC7081
              default flag=1; 00CBEE0C IsFalse(arg0) → 0
              empty arg stays 1 (no 00403A00 skip)
              actor vtbl+48; vtbl+1988(actor,flag); jmp 00CC707C
              not SetScared vtbl+1984
            Drunk gait UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void TeleportInFrontOf_is_vtbl_1892_not_WalkUpToThing()
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
        var interp = new ScriptInterpreter("tif", ["HERO.TeleportInFrontOf FATHER,3"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(3f, runtime.World.Positions["HERO"].Y);
        Assert.Equal("FATHER", runtime.World.LookTargets["HERO"]);
        Assert.Equal(0x00CC485Fu, ScriptCommandMap.Find("TeleportInFrontOf")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("WalkUpToThing")!.Value.ApplySite,
            ScriptCommandMap.Find("TeleportInFrontOf")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("PutInFrontOf")!.Value.ApplySite,
            ScriptCommandMap.Find("TeleportInFrontOf")!.Value.ApplySite);
    }

    [Fact]
    public void TeleportInFrontOf_real_script_bank_or_isolated()
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
                if (raw.Contains(".TeleportInFrontOf ", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "HERO.TeleportInFrontOf FATHER,2";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("TeleportInFrontOf", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        runtime.World.Positions[parsed.Arg(0)] = new System.Numerics.Vector3(0, 0, 0);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-tif", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".TeleportInFrontOf ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.True(runtime.World.Positions.ContainsKey(parsed.Target ?? ""));
        Assert.Equal(parsed.Arg(0), runtime.World.LookTargets[parsed.Target ?? ""]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-tif.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-teleportinfrontof.txt"),
            """
            TeleportInFrontOf 00CC4809 / apply 00CC485F
              ebx required; arg0+arg1 00403A00 empty skip
              resolve arg0 HERO vtbl+280 else vtbl+288
              dest = pos + atof(arg1)*(vtbl+288+12)
              vtbl+1892(actor,dest,0,0,0) teleport
              vtbl+1900(actor,thing,1) look
              leftover +28 if [ebp+103]; jmp 00CC707C
              not WalkUpToThing vtbl+16; not PutInFrontOf 00CD0501
            Nav mesh UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void ResetPos_uses_handle_vtbl28_not_marker_004AA980()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "CREATURE_HERO",
                ScriptName = "HERO",
                PositionX = 10,
                PositionY = 20,
                PositionZ = 0,
                Properties = new Dictionary<string, string>(),
            },
        ], null);
        runtime.World.Teleport("HERO", "MK_AWAY", new System.Numerics.Vector3(100, 100, 0));
        Assert.Equal(100f, runtime.World.Positions["HERO"].X);
        var interp = new ScriptInterpreter("rp", ["HERO.ResetPos"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(10f, runtime.World.Positions["HERO"].X);
        Assert.Equal(20f, runtime.World.Positions["HERO"].Y);
        Assert.Equal(0x00CC4AC3u, ScriptCommandMap.Find("ResetPos")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("Teleport")!.Value.ApplySite,
            ScriptCommandMap.Find("ResetPos")!.Value.ApplySite);
    }

    [Fact]
    public void ResetPos_home_overrides_thing_spawn()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.World.HomePos["HERO"] = new System.Numerics.Vector3(7, 8, 1);
        runtime.World.Positions["HERO"] = new System.Numerics.Vector3(1, 2, 3);
        var interp = new ScriptInterpreter("rph", ["HERO.ResetPos"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(7f, runtime.World.Positions["HERO"].X);
        Assert.Equal(8f, runtime.World.Positions["HERO"].Y);
        Assert.Equal(1f, runtime.World.Positions["HERO"].Z);
    }

    [Fact]
    public void ResetPos_real_script_bank_or_isolated()
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
                if (raw.Contains(".ResetPos", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "HERO.ResetPos";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("ResetPos", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var actor = parsed.Target ?? "HERO";
        runtime.World.HomePos[actor] = new System.Numerics.Vector3(4, 5, 0);
        runtime.World.Positions[actor] = new System.Numerics.Vector3(99, 99, 0);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-reset", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".ResetPos", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Equal(4f, runtime.World.Positions[actor].X);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-reset.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-resetpos.txt"),
            """
            ResetPos 00CC4A71 / apply 00CC4AC3
              ebx required else 00CC7081
              actor vtbl+48; 004AB130 miss skip vtbl
              004AA9A0 [handle+4].vtbl+28 dest
              null handle → [0x143E8E0] default
              vtbl+1892(actor,pos,0,0,0); jmp 00CC707C
              not Teleport marker 004AA980
            SetHomePosThing is global ResetPos, not a HomePos write
            """);
    }

    [Fact]
    public void SetHomePosThing_is_global_ResetPos_not_a_home_write()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "CREATURE_HERO",
                ScriptName = "HERO",
                PositionX = 10,
                PositionY = 20,
                PositionZ = 0,
                Properties = new Dictionary<string, string>(),
            },
        ], null);
        runtime.World.Teleport("HERO", "MK_AWAY", new System.Numerics.Vector3(100, 100, 0));
        Assert.Equal(100f, runtime.World.Positions["HERO"].X);
        var interp = new ScriptInterpreter("shp", ["SetHomePosThing HERO"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(10f, runtime.World.Positions["HERO"].X);
        Assert.Equal(20f, runtime.World.Positions["HERO"].Y);
        Assert.False(runtime.World.HomePos.ContainsKey("HERO"));
        Assert.Equal(0x00CC7D3Cu, ScriptCommandMap.Find("SetHomePosThing")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("ResetPos")!.Value.ApplySite,
            ScriptCommandMap.Find("SetHomePosThing")!.Value.ApplySite);
        var empty = new ScriptInterpreter("shp0", ["SetHomePosThing"]);
        empty.RunUntilYield(runtime);
        Assert.True(empty.Finished);
        Assert.Equal(10f, runtime.World.Positions["HERO"].X);
    }

    [Fact]
    public void SetHomePosThing_real_script_bank_or_isolated()
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
                if (raw.StartsWith("SetHomePosThing ", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "SetHomePosThing HERO";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("SetHomePosThing", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var name = parsed.Arg(0);
        runtime.World.HomePos[name] = new System.Numerics.Vector3(4, 5, 0);
        runtime.World.Positions[name] = new System.Numerics.Vector3(99, 99, 0);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-home", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("SetHomePosThing ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Equal(4f, runtime.World.Positions[name].X);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-home.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-sethomepos.txt"),
            """
            SetHomePosThing 00CC7CE9 / apply 00CC7D3C
              arg0 00403A00 empty skip 00CC8464
              HERO 004A93C0 → vtbl+280 else vtbl+288
              004AB130 miss skip; 004AA9A0 dest
              vtbl+1892(thing,pos,0,0,0); jmp 00CC8231
              DISPROVES HomePos writer — no store, no pos args
              global sibling of ResetPos 00CC4AC3
            Handle vtbl+28 body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void TeleportThing_uses_marker_004AA980_not_SetHomePosThing()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.BindScene(
        [
            new ThingInstance
            {
                Kind = "CTC",
                Section = "Thing",
                DefinitionType = "Marker",
                ScriptName = "MK_MI_LADY1",
                PositionX = 5,
                PositionY = 6,
                PositionZ = 0,
                Properties = new Dictionary<string, string>(),
            },
        ], null);
        var interp = new ScriptInterpreter("tt",
            ["TeleportThing LadyGrey,MK_MI_LADY1"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(5f, runtime.World.Positions["LadyGrey"].X);
        Assert.Equal(6f, runtime.World.Positions["LadyGrey"].Y);
        Assert.Equal(0x00CC7E7Fu, ScriptCommandMap.Find("TeleportThing")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("SetHomePosThing")!.Value.ApplySite,
            ScriptCommandMap.Find("TeleportThing")!.Value.ApplySite);
        var empty = new ScriptInterpreter("tt0", ["TeleportThing LadyGrey"]);
        empty.RunUntilYield(runtime);
        Assert.True(empty.Finished);
        Assert.Equal(5f, runtime.World.Positions["LadyGrey"].X);
    }

    [Fact]
    public void TeleportThing_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_MAYOREXPOSE_INTRO")
                  ?? bank.Find("CS_OAKVALEINTRO_BRATHIT")
                  ?? bank.Find("CS_DRAGON_DEATH");
        Assert.NotNull(hit);
        string? line = null;
        foreach (var raw in hit.Commands.Count > 0
                     ? hit.Commands
                     : ScriptBank.ExtractCommands(hit.Raw))
        {
            if (raw.StartsWith("TeleportThing ", StringComparison.OrdinalIgnoreCase))
            {
                line = raw;
                break;
            }
        }

        Assert.False(string.IsNullOrEmpty(line));
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("TeleportThing", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        runtime.World.Positions[parsed.Arg(1)] = new System.Numerics.Vector3(3, 4, 0);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-tpth", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("TeleportThing ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.True(runtime.World.Positions.ContainsKey(parsed.Arg(0)));
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-tpth.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-teleportthing.txt"),
            """
            TeleportThing 00CC7E2C / apply 00CC7E7F
              arg0+arg1 00403A00 empty skip 00CC8464
              resolve each: HERO vtbl+280 else vtbl+288
              004AB130 both; IsFalse(arg2)->0 else 1
              004AAA40 yaw; 004AA980 marker pos
              vtbl+1892(thing,pos,yaw,flag,0); jmp 00CC8231
              not SetHomePosThing 004AA9A0
            Yaw 004AAA40 body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void SetThingConscious_default_off_IsTrue_on_vtbl_2324()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("stc",
        [
            "SetThingConscious FIGHTER,TRUE",
            "SetThingConscious TYLER,FALSE",
            "SetThingConscious KO",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.True(runtime.World.Conscious["FIGHTER"]);
        Assert.False(runtime.World.Conscious["TYLER"]);
        Assert.False(runtime.World.Conscious["KO"]);
        Assert.Equal(2324, runtime.World.ConsciousVtbl["FIGHTER"]);
        Assert.Equal(0x00CC8094u, ScriptCommandMap.Find("SetThingConscious")!.Value.ApplySite);
        var empty = new ScriptInterpreter("stc0", ["SetThingConscious"]);
        empty.RunUntilYield(runtime);
        Assert.True(empty.Finished);
        Assert.False(runtime.World.Conscious.ContainsKey(""));
    }

    [Fact]
    public void SetThingConscious_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        var hit = bank.Find("CS_PUNCHCLUB_BC_ROUNDWON")
                  ?? bank.Find("CS_PUNCHCLUB_OV_FINALWIN")
                  ?? bank.Find("CS_PUNCHCLUB_BS_FINALWIN");
        Assert.NotNull(hit);
        string? line = null;
        foreach (var raw in hit.Commands.Count > 0
                     ? hit.Commands
                     : ScriptBank.ExtractCommands(hit.Raw))
        {
            if (raw.StartsWith("SetThingConscious ", StringComparison.OrdinalIgnoreCase))
            {
                line = raw;
                break;
            }
        }

        Assert.False(string.IsNullOrEmpty(line));
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("SetThingConscious", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-con", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("SetThingConscious ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Equal(ScriptLine.IsTrue(parsed.Arg(1)),
            runtime.World.Conscious[parsed.Arg(0)]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-con.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-setthingconscious.txt"),
            """
            SetThingConscious 00CC8041 / apply 00CC8094
              arg0 00403A00 empty skip 00CC8464
              default flag=0; 00CBEDBA IsTrue(arg1) → 1
              00CD3187 actor-map else HERO vtbl+280/288
              optional arg2 0099EFB0 extra string
              vtbl+2324(thing,flag,extra); jmp 00CC8469
              not SetScared default-1 / vtbl+1984
            Consciousness / KO body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void LookToCamera_is_vtbl_1996_not_LookToThing()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("ltc",
        [
            "HERO.LookToCamera TRUE",
            "FATHER.LookToCamera FALSE",
            "WHISPER.LookToCamera",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.True(runtime.World.LookToCamera["HERO"]);
        Assert.Equal("CAMERA", runtime.World.LookTargets["HERO"]);
        Assert.False(runtime.World.LookToCamera["FATHER"]);
        Assert.Equal("", runtime.World.LookTargets["FATHER"]);
        Assert.True(runtime.World.LookToCamera["WHISPER"]);
        Assert.Equal("CAMERA", runtime.World.LookTargets["WHISPER"]);
        Assert.Equal(0x00CC3D36u, ScriptCommandMap.Find("LookToCamera")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("LookToThing")!.Value.ApplySite,
            ScriptCommandMap.Find("LookToCamera")!.Value.ApplySite);
    }

    [Fact]
    public void LookToCamera_real_script_bank_or_isolated()
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
                if (raw.Contains(".LookToCamera", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "HERO.LookToCamera TRUE";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("LookToCamera", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-ltc", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".LookToCamera", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        var flag = !ScriptLine.IsFalse(parsed.Arg(0));
        Assert.Equal(flag, runtime.World.LookToCamera[parsed.Target ?? ""]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-ltc.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-looktocamera.txt"),
            """
            LookToCamera 00CC3CE4 / apply 00CC3D36
              ebx required else 00CC7081
              default flag=1; 00CBEE0C IsFalse(arg0) → 0
              00CBF9DE(arg0); vtbl+1996(handle,flag)
              jmp 00CC707C
              not LookToThing vtbl+1992
            Camera look IK UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void PauseThing_TRUE_is_vtbl_2048_mode_1_FALSE_is_2()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("pause",
        [
            "PauseThing SPECTATORCS0,TRUE",
            "PauseThing HauntedHouseClock,FALSE",
            "PauseThing GUARD1,false",
            "PauseThing",
            "PauseThing ONLYNAME",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(1, runtime.World.PauseModes["SPECTATORCS0"]);
        Assert.Equal(2, runtime.World.PauseModes["HauntedHouseClock"]);
        Assert.Equal(2, runtime.World.PauseModes["GUARD1"]);
        Assert.Equal(2048, runtime.World.PauseVtbl["SPECTATORCS0"]);
        Assert.False(runtime.World.PauseModes.ContainsKey("ONLYNAME"));
        Assert.Equal(0x00CC7B24u, ScriptCommandMap.Find("PauseThing")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("SetThingConscious")!.Value.ApplySite,
            ScriptCommandMap.Find("PauseThing")!.Value.ApplySite);
    }

    [Fact]
    public void PauseThing_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var name in new[]
                 {
                     "CS_CHICKING_INTRO",
                     "CS_HANGINGTREE_EVIL_ESCAPE",
                     "CS_JACK_DEATH",
                     "CS_HAUNTED_DOORCLOSES",
                 })
        {
            hit = bank.Entries.FirstOrDefault(e =>
                e.InstanceName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (hit is null)
                continue;
            foreach (var raw in hit.Commands.Count > 0
                         ? hit.Commands
                         : ScriptBank.ExtractCommands(hit.Raw))
            {
                if (raw.StartsWith("PauseThing ", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        if (line is null)
        {
            foreach (var entry in bank.Entries)
            {
                foreach (var raw in entry.Commands.Count > 0
                             ? entry.Commands
                             : ScriptBank.ExtractCommands(entry.Raw))
                {
                    if (raw.StartsWith("PauseThing ", StringComparison.OrdinalIgnoreCase) &&
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
        }

        Assert.False(string.IsNullOrEmpty(line));
        Assert.NotNull(hit);
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("PauseThing", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-pause", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("PauseThing ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        var mode = ScriptLine.IsFalse(parsed.Arg(1)) ? 2 : 1;
        Assert.Equal(mode, runtime.World.PauseModes[parsed.Arg(0)]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-pause.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-pausething.txt"),
            """
            PauseThing 00CC7AD5 / apply 00CC7B24
              arg0+arg1 00403A00 empty skip 00CC8464
              00CBF9DE(arg0); 004AB130
              00CBEE0C IsFalse(arg1) → push 2 else push 1
              vtbl+2048(thing,mode); jmp 00CC82E9
              not 0/1 boolean; not SetGravityOnThing 2128
            Pause/sim body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void SetGravityOnThing_is_vtbl_2128_not_PauseThing()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("grav",
        [
            "SetGravityOnThing HERO,FALSE",
            "SetGravityOnThing FireHeart,TRUE",
            "SetGravityOnThing CS_AEONS,false",
            "SetGravityOnThing",
            "SetGravityOnThing ONLYNAME",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.False(runtime.World.GravityOn["HERO"]);
        Assert.True(runtime.World.GravityOn["FireHeart"]);
        Assert.False(runtime.World.GravityOn["CS_AEONS"]);
        Assert.Equal(2128, runtime.World.GravityVtbl["HERO"]);
        Assert.False(runtime.World.GravityOn.ContainsKey("ONLYNAME"));
        Assert.Equal(0x00CC7BEBu, ScriptCommandMap.Find("SetGravityOnThing")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("PauseThing")!.Value.ApplySite,
            ScriptCommandMap.Find("SetGravityOnThing")!.Value.ApplySite);
    }

    [Fact]
    public void SetGravityOnThing_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var name in new[]
                 {
                     "CS_DRAGON_OUTRO_EVIL",
                     "CS_SHIP_SAILS",
                     "CS_SUMSHIP_INTRO",
                     "CS_FIREHEART_OUTRO",
                     "CS_JACK_DEATH",
                 })
        {
            hit = bank.Entries.FirstOrDefault(e =>
                e.InstanceName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (hit is null)
                continue;
            foreach (var raw in hit.Commands.Count > 0
                         ? hit.Commands
                         : ScriptBank.ExtractCommands(hit.Raw))
            {
                if (raw.StartsWith("SetGravityOnThing ", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        if (line is null)
        {
            foreach (var entry in bank.Entries)
            {
                foreach (var raw in entry.Commands.Count > 0
                             ? entry.Commands
                             : ScriptBank.ExtractCommands(entry.Raw))
                {
                    if (raw.StartsWith("SetGravityOnThing ", StringComparison.OrdinalIgnoreCase) &&
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
        }

        Assert.False(string.IsNullOrEmpty(line));
        Assert.NotNull(hit);
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("SetGravityOnThing", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-grav", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("SetGravityOnThing ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        var on = !ScriptLine.IsFalse(parsed.Arg(1));
        Assert.Equal(on, runtime.World.GravityOn[parsed.Arg(0)]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-grav.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-setgravityonthing.txt"),
            """
            SetGravityOnThing 00CC7B98 / apply 00CC7BEB
              arg0+arg1 00403A00 empty skip 00CC8464
              HERO vtbl+280 else vtbl+288
              004ABE90; 004AB130
              default flag=1; 00CBEE0C IsFalse(arg1) → 0
              vtbl+2128(thing,flag); jmp 00CC8231
              not PauseThing vtbl+2048 modes 1/2
            Physics body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void LiftRock_is_vtbl_896_HERO_two_strings()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("lift",
        [
            "LiftRock ROCK1,MK_ROCK",
            "LiftRock",
            "LiftRock ONLYNAME",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Single(runtime.World.LiftRocks);
        Assert.Equal(("ROCK1", "MK_ROCK"), runtime.World.LiftRocks[0]);
        Assert.Equal(896, runtime.World.LiftRockVtbl);
        Assert.Equal(0x00CC828Cu, ScriptCommandMap.Find("LiftRock")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("HoldInHand")!.Value.ApplySite,
            ScriptCommandMap.Find("LiftRock")!.Value.ApplySite);
    }

    [Fact]
    public void LiftRock_real_script_bank_or_isolated()
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
                if (raw.StartsWith("LiftRock ", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "LiftRock ROCK1,MK_ROCK";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("LiftRock", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-lift", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("LiftRock ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Equal((parsed.Arg(0), parsed.Arg(1)), runtime.World.LiftRocks[0]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-lift.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-liftrock.txt"),
            """
            LiftRock 00CC823D / apply 00CC828C
              arg0+arg1 00403A00 empty skip 00CC8464
              always HERO vtbl+280; 004ABE90
              vtbl+896(hero,arg0,arg1) raw strings
              004AA840; jmp 00CC8464
              not HoldInHand vtbl+892
            Lift/attach body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void FadeThingIn_snaps_0_to_1_not_screen_FadeIn()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("fti",
        [
            "FadeThingIn FireHeart,0",
            "FadeThingIn GraveyardPathNewDoor,2.0",
            "FadeThingIn",
            "FadeThingIn ONLYNAME",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(1f, runtime.World.Alpha["FireHeart"]);
        Assert.Equal(1f, runtime.World.Alpha["GraveyardPathNewDoor"]);
        Assert.Equal(2040, runtime.World.FadeThingVtbl);
        Assert.False(runtime.World.Alpha.ContainsKey("ONLYNAME"));
        Assert.Equal(0x00CC7881u, ScriptCommandMap.Find("FadeThingIn")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("FadeIn")!.Value.ApplySite,
            ScriptCommandMap.Find("FadeThingIn")!.Value.ApplySite);
    }

    [Fact]
    public void FadeThingOut_duration_0_snaps_to_0_not_screen_FadeOut()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("fto",
        [
            "FadeThingOut SKEL,0",
            "FadeThingOut",
            "FadeThingOut ONLYNAME",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(0f, runtime.World.Alpha["SKEL"]);
        Assert.Equal(2040, runtime.World.FadeThingVtbl);
        Assert.False(runtime.World.Alpha.ContainsKey("ONLYNAME"));
        Assert.Equal(0x00CC7682u, ScriptCommandMap.Find("FadeThingOut")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("FadeOut")!.Value.ApplySite,
            ScriptCommandMap.Find("FadeThingOut")!.Value.ApplySite);
    }

    [Fact]
    public void FadeThingIn_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var name in new[] { "CS_OPENGRAVE_PATHDOOR", "CS_SUMSHIP_INTRO" })
        {
            hit = bank.Entries.FirstOrDefault(e =>
                e.InstanceName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (hit is null)
                continue;
            foreach (var raw in hit.Commands.Count > 0
                         ? hit.Commands
                         : ScriptBank.ExtractCommands(hit.Raw))
            {
                if (raw.StartsWith("FadeThingIn ", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        Assert.False(string.IsNullOrEmpty(line));
        Assert.NotNull(hit);
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("FadeThingIn", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-fadin", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("FadeThingIn ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Equal(1f, runtime.World.Alpha[parsed.Arg(0)]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-fadin.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-fadething.txt"),
            """
            FadeThingIn 00CC782E / apply 00CC7881
            FadeThingOut 00CC762F / apply 00CC7682
              arg0+arg1 00403A00 empty skip 00CC8464
              resolve thing HERO vtbl+280 else 288
              In defaults start=0 end=1; Out start=1 end=0
              optional atof arg2/arg3 override start/end
              atof arg1 duration; fcom 0x122DEDC=0
              duration<=0 skip loop; loop only while current>end
              In 0->1 therefore snaps to 1 (no leftover)
              Out 1->0 leftover if [ebp+103]; *15 steps
              each step + final: vtbl+2040(thing,alpha,1)
              jmp 00CC8231
              not screen FadeIn 1496 / FadeOut 1488
            Intermediate mesh fade UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void PlayObjectAnim_is_vtbl_1948_after_unpause_mode_2()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("poa",
        [
            "AnimationPause FALSE",
            "PlayObjectAnim GATE,OPEN,TRUE",
            "PlayObjectAnim DOOR,CLOSE",
            "PlayObjectAnim",
            "PlayObjectAnim ONLYNAME",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(2, runtime.World.PauseModes["GATE"]);
        Assert.Equal(2, runtime.World.PauseModes["DOOR"]);
        Assert.Equal("OPEN", runtime.Animation.States["GATE"].Name);
        Assert.Equal(1948, runtime.Animation.States["GATE"].RequestMode);
        Assert.Equal("CLOSE", runtime.Animation.States["DOOR"].Name);
        Assert.False(runtime.Animation.States.ContainsKey("ONLYNAME"));
        Assert.Equal(0x00CC74DEu, ScriptCommandMap.Find("PlayObjectAnim")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("PlayAnimation")!.Value.ApplySite,
            ScriptCommandMap.Find("PlayObjectAnim")!.Value.ApplySite);
    }

    [Fact]
    public void PlayObjectAnim_real_script_bank_or_isolated()
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
                if (raw.StartsWith("PlayObjectAnim ", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "PlayObjectAnim GATE,OPEN,TRUE";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("PlayObjectAnim", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-poa",
            ["AnimationPause FALSE", line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("PlayObjectAnim ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Equal(2, runtime.World.PauseModes[parsed.Arg(0)]);
        Assert.Equal(parsed.Arg(1), runtime.Animation.States[parsed.Arg(0)].Name);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-poa.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-playobjectanim.txt"),
            """
            PlayObjectAnim 00CC748B / apply 00CC74DE
              arg0+arg1 00403A00 empty skip 00CC8464
              default flag=0; 00CBEDBA IsTrue(arg2) → 1
              always vtbl+288 (not HERO 280)
              vtbl+2048(thing,2) unpause
              vtbl+1948(thing,anim,flag)
              leftover if [ebp-22] AnimationPause + [ebp+103]
              jmp 00CC82E9
              not PlayAnimation vtbl+72
            Object clip body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void CameraPreload_is_vtbl_1648_wrapped_by_1612()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("cpre",
        [
            "CameraPreload CAM_OVI_SHOT1",
            "CameraPreload",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Contains("CAM_OVI_SHOT1", runtime.CameraSys.Preloaded);
        Assert.Equal(1612, runtime.CameraSys.CameraPreloadGateVtbl);
        Assert.Equal(1648, runtime.CameraSys.CameraPreloadBindVtbl);
        Assert.Equal(0x00CC7A7Cu, ScriptCommandMap.Find("CameraPreload")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("DoCameraPreloading")!.Value.ApplySite,
            ScriptCommandMap.Find("CameraPreload")!.Value.ApplySite);
    }

    [Fact]
    public void CameraPreload_real_script_bank_or_isolated()
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
                if (raw.StartsWith("CameraPreload ", StringComparison.OrdinalIgnoreCase) &&
                    !raw.Contains("DoCamera", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "CameraPreload CAM_OVI_SHOT1";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("CameraPreload", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-cpre", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("CameraPreload ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Contains(parsed.Arg(0), runtime.CameraSys.Preloaded);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-cpre.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-camerapreload.txt"),
            """
            CameraPreload 00CC7A2D / apply 00CC7A7C
              arg0 00403A00 empty skip 00CC8464
              vtbl+1612(1)
              fld 0x122DEE0=-1; vtbl+1648(name,0,0,-1,0,-1)
              vtbl+1612(0); jmp 00CC8464
              not DoCameraPreloading 00CBF29F / 1560/1568
            Camera resource body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void FollowNavRoute_is_vtbl_24_gait_run1_sneak2()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("fnr",
        [
            "ScriptFrame FALSE",
            "HERO.FollowNavRoute ROUTE1",
            "GUARD.FollowNavRoute ROUTE2,run,TRUE",
            "BANDIT.FollowNavRoute PATH,sneak",
            "HERO.FollowNavRoute",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal("ROUTE1", runtime.Movement.NavRoutes["HERO"]);
        Assert.Equal(0, runtime.Movement.NavGaits["HERO"]);
        Assert.Equal(1, runtime.Movement.NavGaits["GUARD"]);
        Assert.Equal(2, runtime.Movement.NavGaits["BANDIT"]);
        Assert.Equal(24, runtime.Movement.NavVtbl["HERO"]);
        Assert.Equal(EntityTaskKind.NavRoute, runtime.Movement.Tasks.Current("GUARD")!.Kind);
        Assert.Equal(0x00CC4350u, ScriptCommandMap.Find("FollowNavRoute")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("FollowThing")!.Value.ApplySite,
            ScriptCommandMap.Find("FollowNavRoute")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("WalkTo")!.Value.ApplySite,
            ScriptCommandMap.Find("FollowNavRoute")!.Value.ApplySite);
    }

    [Fact]
    public void FollowNavRoute_real_script_bank_or_isolated()
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
                if (raw.Contains(".FollowNavRoute", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "HERO.FollowNavRoute ROUTE1,run,TRUE";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("FollowNavRoute", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-fnr",
            ["ScriptFrame FALSE", line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".FollowNavRoute", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        var gait = 0;
        if (ScriptLine.TokenMatches(parsed.Arg(1), "run"))
            gait = 1;
        else if (ScriptLine.TokenMatches(parsed.Arg(1), "sneak"))
            gait = 2;
        Assert.Equal(parsed.Arg(0), runtime.Movement.NavRoutes[parsed.Target ?? ""]);
        Assert.Equal(gait, runtime.Movement.NavGaits[parsed.Target ?? ""]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-fnr.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-follownavroute.txt"),
            """
            FollowNavRoute 00CC42FA / apply 00CC4350
              ebx actor else 00CC7081
              arg0 00403A00 empty skip
              00BFEBA8 "run" → gait 1
              00BFEBA8 "sneak" → gait 2
              default gait 0
              00CBEDBA IsTrue(arg2) wait flag
              resolve arg0 HERO 280 else 288
              actor vtbl+24(route,gait,flag,0)
              leftover 00CC5691 if [ebp+103]
              not WalkTo/RunTo/SneakTo vtbl+16
            Route spline UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void AILevel_HIGH_is_3_MEDIUM_2_default_4()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("ail",
        [
            "GUARD.AILevel HIGH",
            "BANDIT.AILevel MEDIUM",
            "HERO.AILevel LOW",
            "WHISPER.AILevel",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(3, runtime.World.AILevels["GUARD"]);
        Assert.Equal(2, runtime.World.AILevels["BANDIT"]);
        Assert.Equal(4, runtime.World.AILevels["HERO"]);
        Assert.Equal(32, runtime.World.AILevelVtbl["GUARD"]);
        Assert.False(runtime.World.AILevels.ContainsKey("WHISPER"));
        Assert.Equal(0x00CC4501u, ScriptCommandMap.Find("AILevel")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("SetScared")!.Value.ApplySite,
            ScriptCommandMap.Find("AILevel")!.Value.ApplySite);
    }

    [Fact]
    public void AILevel_real_script_bank_or_isolated()
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
                if (raw.Contains(".AILevel", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "GUARD.AILevel HIGH";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("AILevel", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-ail", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".AILevel", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        var level = 4;
        if (ScriptLine.TokenMatches(parsed.Arg(0), "HIGH"))
            level = 3;
        else if (ScriptLine.TokenMatches(parsed.Arg(0), "MEDIUM"))
            level = 2;
        Assert.Equal(level, runtime.World.AILevels[parsed.Target ?? ""]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-ail.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-ailevel.txt"),
            """
            AILevel 00CC44A9 / apply 00CC4501
              ebx actor else 00CC7081
              arg0 00403A00 empty skip
              default edi=4
              00BFEBA8 HIGH → 3
              00BFEBA8 MEDIUM → 2
              no LOW token
              actor vtbl+48 handle; 004AB130
              00CD2770(actor); vtbl+32(handle,actor,level)
              004AA8B0+00CD3D2E+008ABD10; jmp 00CC707C
            AI brain UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void WaitForAnimationEvent_polls_vtbl_236_not_WaitPlayAnimation()
    {
        var runtime = ScriptRuntime.Detached();
        var empty = new ScriptInterpreter("wfae0", ["HERO.WaitForAnimationEvent"]);
        empty.RunUntilYield(runtime);
        Assert.True(empty.Finished);
        Assert.False(runtime.Animation.EventWaits.ContainsKey("HERO"));

        var wait = new ScriptInterpreter("wfae", ["HERO.WaitForAnimationEvent FOOTSTEP"]);
        wait.RunUntilYield(runtime);
        Assert.True(wait.Yielded);
        Assert.False(wait.Finished);
        Assert.Equal("FOOTSTEP", runtime.Animation.EventWaits["HERO"]);
        Assert.Equal(236, runtime.Animation.EventWaitVtbl["HERO"]);
        Assert.Equal(0x00CC4252u, ScriptCommandMap.Find("WaitForAnimationEvent")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("WaitPlayAnimation")!.Value.ApplySite,
            ScriptCommandMap.Find("WaitForAnimationEvent")!.Value.ApplySite);
    }

    [Fact]
    public void WaitForAnimationEvent_real_script_bank_or_isolated()
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
                if (raw.Contains(".WaitForAnimationEvent", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "HERO.WaitForAnimationEvent FOOTSTEP";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("WaitForAnimationEvent", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-wfae", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".WaitForAnimationEvent", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(parsed.Arg(0), runtime.Animation.EventWaits[parsed.Target ?? ""]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-wfae.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-waitforanimationevent.txt"),
            """
            WaitForAnimationEvent 00CC41FC / apply 00CC4252
              ebx actor else 00CC7081
              arg0 00403A00 empty skip
              00CBEB7E skip-true → 00CC7081
              actor vtbl+48; 004AB130
              leftover poll 004AAF60 → [handle+4].vtbl+236
              [0x13D2838]+5 breaks; jmp 00CC707C
              not WaitPlayAnimation
            Event table / vtbl+236 body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void Release_drops_actor_slot_not_SetFree()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("rel",
        [
            "GUARD.AILevel HIGH",
            "GUARD.Release",
            "BANDIT.Release",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Contains("GUARD", runtime.World.Released);
        Assert.Contains("BANDIT", runtime.World.Released);
        Assert.False(runtime.World.AILevels.ContainsKey("GUARD"));
        Assert.Equal(0x00CD2770u, runtime.World.ReleaseFn["GUARD"]);
        Assert.DoesNotContain("GUARD", runtime.World.Freed);
        Assert.Equal(0x00CC4663u, ScriptCommandMap.Find("Release")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("SetFree")!.Value.ApplySite,
            ScriptCommandMap.Find("Release")!.Value.ApplySite);
    }

    [Fact]
    public void Release_real_script_bank_or_isolated()
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
                if (raw.Contains(".Release", StringComparison.OrdinalIgnoreCase) &&
                    !raw.Contains('$', StringComparison.Ordinal) &&
                    !raw.Contains("Remove", StringComparison.OrdinalIgnoreCase))
                {
                    var parsedTry = ScriptLine.Parse(raw);
                    if (parsedTry.Verb.Equals("Release", StringComparison.OrdinalIgnoreCase))
                    {
                        line = raw;
                        hit = entry;
                        break;
                    }
                }
            }

            if (line is not null)
                break;
        }

        line ??= "GUARD.Release";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("Release", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-rel", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".Release", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Contains(parsed.Target ?? "", runtime.World.Released);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-rel.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-release.txt"),
            """
            Release 00CC4610 / apply 00CC4663
              ebx actor else 00CC7081
              no args
              00CD2770(actor): 007E70E0(actor+8) refcount drop
              then and [actor+8],0
              jmp 00CC7081
              not SetFree vtbl+1980; not Remove
            Slot object dtor UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void WaitForUnderRadius_continues_when_dist_sq_lt_r_sq()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.World.Positions["HERO"] = new System.Numerics.Vector3(0, 0, 0);
        runtime.World.Positions["FATHER"] = new System.Numerics.Vector3(1, 0, 0);
        var inside = new ScriptInterpreter("wur-in", ["HERO.WaitForUnderRadius FATHER,2.0"]);
        inside.RunUntilYield(runtime);
        Assert.True(inside.Finished);
        Assert.Equal("FATHER", runtime.World.UnderRadiusTargets["HERO"]);
        Assert.Equal(2f, runtime.World.UnderRadius["HERO"]);
        Assert.Equal(0x00CC409Bu, ScriptCommandMap.Find("WaitForUnderRadius")!.Value.ApplySite);

        runtime.World.Positions["BANDIT"] = new System.Numerics.Vector3(10, 0, 0);
        var far = new ScriptInterpreter("wur-out", ["HERO.WaitForUnderRadius BANDIT,2.0"]);
        far.RunUntilYield(runtime);
        Assert.True(far.Yielded);
        Assert.False(far.Finished);
        var empty = new ScriptInterpreter("wur0", ["HERO.WaitForUnderRadius"]);
        empty.RunUntilYield(runtime);
        Assert.True(empty.Finished);
    }

    [Fact]
    public void WaitForUnderRadius_real_script_bank_or_isolated()
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
                if (raw.Contains(".WaitForUnderRadius", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "HERO.WaitForUnderRadius FATHER,2.0";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("WaitForUnderRadius", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        runtime.World.Positions[parsed.Target ?? ""] = new System.Numerics.Vector3(0, 0, 0);
        runtime.World.Positions[parsed.Arg(0)] = new System.Numerics.Vector3(0.5f, 0, 0);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-wur", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".WaitForUnderRadius", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(parsed.Arg(0), runtime.World.UnderRadiusTargets[parsed.Target ?? ""]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-wur.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-waitforunderradius.txt"),
            """
            WaitForUnderRadius 00CC4045 / apply 00CC409B
              ebx actor; arg0+arg1 required
              atof arg1 radius; resolve arg0 280/288
              actor vtbl+48; 00CBE2FF(handle,target,r)
              both vtbl+300 then pos vtbl+24
              success iff dist^2 < r^2 (strict)
              00CBEB7E skip continue
              else leftover loop 00CC40CE
            Mesh/pos vtbl+24 body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void ReturnFollowers_TRUE_binds_HeroFollower0_not_TeleportFollowers()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("rfol",
        [
            "ReturnFollowers FALSE",
            "ReturnFollowers TRUE",
            "ReturnFollowers",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.True(runtime.World.FollowersReturned);
        Assert.Equal(924, runtime.World.FollowerReturnVtbl);
        Assert.Contains("HeroFollower0", runtime.World.Followers);
        Assert.False(runtime.World.FollowersTeleported);
        Assert.Equal(0x00CC68EDu, ScriptCommandMap.Find("ReturnFollowers")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("TeleportFollowers")!.Value.ApplySite,
            ScriptCommandMap.Find("ReturnFollowers")!.Value.ApplySite);
    }

    [Fact]
    public void TeleportFollowers_empty_list_skips_956()
    {
        var runtime = ScriptRuntime.Detached();
        var empty = new ScriptInterpreter("tfol0", ["TeleportFollowers TRUE"]);
        empty.RunUntilYield(runtime);
        Assert.True(empty.Finished);
        Assert.False(runtime.World.FollowersTeleported);

        var prep = new ScriptInterpreter("tfol",
        [
            "ReturnFollowers TRUE",
            "TeleportFollowers TRUE",
        ]);
        prep.RunUntilYield(runtime);
        Assert.True(prep.Finished);
        Assert.True(runtime.World.FollowersTeleported);
        Assert.True(runtime.World.FollowerTeleportFade);
        Assert.Equal(956, runtime.World.FollowerTeleportVtbl);
        Assert.Equal(0x00CC6A2Eu, ScriptCommandMap.Find("TeleportFollowers")!.Value.ApplySite);
    }

    [Fact]
    public void ReturnFollowers_real_script_bank_line()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? line = null;
        ScriptDef? hit = null;
        foreach (var name in new[]
                 {
                     "CS_RANSOM_OUTRO_GOOD_TELL",
                     "CS_RANSOM_OUTRO_GOOD",
                     "CS_RANSOM_OUTRO_GOOD_NOTELL",
                 })
        {
            hit = bank.Entries.FirstOrDefault(e =>
                e.InstanceName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (hit is null)
                continue;
            foreach (var raw in hit.Commands.Count > 0
                         ? hit.Commands
                         : ScriptBank.ExtractCommands(hit.Raw))
            {
                if (raw.StartsWith("ReturnFollowers ", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        Assert.False(string.IsNullOrEmpty(line));
        Assert.NotNull(hit);
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("ReturnFollowers", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-rfol", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("ReturnFollowers ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Equal(ScriptLine.IsTrue(parsed.Arg(0)), runtime.World.FollowersReturned);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-rfol.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-followers.txt"),
            """
            ReturnFollowers 00CC689A / apply 00CC68ED
              arg0 required; IsTrue → vtbl+924(HERO)
              HeroFollower0 00CD3187+0041C820
              each valid vtbl+300 → 008ADF90
              FALSE [ebp-40]=0 no restore
            TeleportFollowers 00CC69DA / apply 00CC6A2E
              empty list skip
              IsTrue: vtbl+1492/1504 0.5 then 956 then FadeIn 1496
              not ReturnFollowers 924
            Follower warp/list UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void PreloadAnim_basic_is_2148_named_is_2144()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("pre",
        [
            "HERO.PreloadAnim",
            "HERO.PreloadAnim basic",
            "GUARD.PreloadAnim CS_WAVE",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(3, runtime.Animation.Preloads.Count);
        Assert.Equal(2148, runtime.Animation.Preloads[0].Vtbl);
        Assert.Equal(2148, runtime.Animation.Preloads[1].Vtbl);
        Assert.Equal(("GUARD", "CS_WAVE", 2144), runtime.Animation.Preloads[2]);
        Assert.Equal(0x00CC140Eu, ScriptCommandMap.Find("PreloadAnim")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("PlayAnimation")!.Value.ApplySite,
            ScriptCommandMap.Find("PreloadAnim")!.Value.ApplySite);
    }

    [Fact]
    public void PreloadAnim_real_script_bank_or_isolated()
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
                if (raw.Contains(".PreloadAnim", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "HERO.PreloadAnim CS_WAVE";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("PreloadAnim", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-pre", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".PreloadAnim", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Single(runtime.Animation.Preloads);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-pre.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-preloadanim.txt"),
            """
            PreloadAnim 00CC13B8 / apply 00CC140E
              ebx actor else 00CC7081
              actor vtbl+48 handle
              empty or 00BFEBA8 BASIC → vtbl+2148(handle)
              else vtbl+2144(handle,arg0)
              jmp 00CC707C
              not PlayAnimation vtbl+72
            Clip cache UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void AToSkip_writes_skip_global_KeepEntityMap_always_on()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("skip",
        [
            "AToSkip",
            "HERO.WaitForAnimationEvent FOOTSTEP",
            "KeepEntityMap FALSE",
            "HideBodies FALSE",
            "HideBodies TRUE",
            "EnableBlackScreenSubtitles",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Contains(interp.Executed, l =>
            l.Contains(".WaitForAnimationEvent", StringComparison.OrdinalIgnoreCase));
        Assert.True(runtime.World.HideBodies);
        Assert.Equal(1604, runtime.World.HideBodiesVtbl);
        Assert.Equal(0x00CC5E2Eu, ScriptCommandMap.Find("AToSkip")!.Value.ApplySite);
        Assert.Equal(0x00CC5E97u, ScriptCommandMap.Find("KeepEntityMap")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("AToSkip")!.Value.ApplySite,
            ScriptCommandMap.Find("KeepEntityMap")!.Value.ApplySite);
    }

    [Fact]
    public void AToSkip_FALSE_does_not_skip_WaitForAnimationEvent()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("skip0",
        [
            "AToSkip FALSE",
            "HERO.WaitForAnimationEvent FOOTSTEP",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Yielded);
        Assert.False(interp.Finished);
        Assert.Equal("FOOTSTEP", runtime.Animation.EventWaits["HERO"]);
    }

    [Fact]
    public void KeepEntityMap_and_AToSkip_real_script_bank_lines()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var bank = ScriptBank.Load(install);
        string? keepLine = null;
        ScriptDef? keepHit = null;
        string? skipLine = null;
        ScriptDef? skipHit = null;
        string? hideLine = null;
        ScriptDef? hideHit = null;
        foreach (var entry in bank.Entries)
        {
            foreach (var raw in entry.Commands.Count > 0
                         ? entry.Commands
                         : ScriptBank.ExtractCommands(entry.Raw))
            {
                if (keepLine is null &&
                    raw.StartsWith("KeepEntityMap", StringComparison.OrdinalIgnoreCase) &&
                    !raw.Contains('$', StringComparison.Ordinal))
                {
                    keepLine = raw;
                    keepHit = entry;
                }

                if (skipLine is null &&
                    raw.StartsWith("AToSkip", StringComparison.OrdinalIgnoreCase) &&
                    !raw.Contains('$', StringComparison.Ordinal))
                {
                    skipLine = raw;
                    skipHit = entry;
                }

                if (hideLine is null &&
                    raw.StartsWith("HideBodies ", StringComparison.OrdinalIgnoreCase) &&
                    !raw.Contains('$', StringComparison.Ordinal))
                {
                    hideLine = raw;
                    hideHit = entry;
                }
            }

            if (keepLine is not null && skipLine is not null && hideLine is not null)
                break;
        }

        keepLine ??= "KeepEntityMap TRUE";
        keepHit ??= bank.Entries[0];
        skipLine ??= "AToSkip";
        skipHit ??= bank.Entries[0];
        hideLine ??= "HideBodies FALSE";
        hideHit ??= bank.Entries[0];
        Assert.Equal("KeepEntityMap", ScriptLine.Parse(keepLine).Verb);
        Assert.Equal("AToSkip", ScriptLine.Parse(skipLine).Verb);
        Assert.Equal("HideBodies", ScriptLine.Parse(hideLine).Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter("map-skip-hide", [keepLine, skipLine, hideLine]);
        isolated.RunUntilYield(runtime);
        Assert.True(isolated.Finished);
        var hide = ScriptLine.Parse(hideLine);
        var expectHide = hide.Arg(0).Length == 0 || ScriptLine.IsTrue(hide.Arg(0));
        Assert.Equal(expectHide, runtime.World.HideBodies);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, "atoskip-keepentitymap.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-atoskip.txt"),
            """
            AToSkip 00CC5DDE / apply 00CC5E2E
              00CBEE0C IsFalse; !IsFalse → [0x143E8F4]
              empty enables skip; 00CBEB7E reads it
            KeepEntityMap 00CC5E47 / apply 00CC5E97
              always [ebp-59]=1; args ignored
            EnableBlackScreenSubtitles 00CC5EA1 / apply 00CC5EF1
              always [ebp-564]=1
            HideBodies 00CC5EFE / apply 00CC5F4E
              empty/IsTrue vtbl+1604(1); else 1604(0)
            Body mesh / map retain UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void InteractiveSpeakGroup_builds_prefix_10_20_not_InteractiveSpeak()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("isg",
        [
            "ScriptFrame FALSE",
            "HERO.InteractiveSpeakGroup FATHER,TEXT_QST_FOO,3",
            "HERO.InteractiveSpeakGroup",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(
            new[] { "TEXT_QST_FOO_10", "TEXT_QST_FOO_20", "TEXT_QST_FOO_30" },
            runtime.Dialogue.GroupLines);
        Assert.Equal(1464, runtime.Dialogue.GroupSpeakVtbl);
        Assert.Equal("InteractiveSpeakGroup", runtime.Dialogue.Session?.Verb);
        Assert.Equal(0x00CC2CCDu, ScriptCommandMap.Find("InteractiveSpeakGroup")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("InteractiveSpeak")!.Value.ApplySite,
            ScriptCommandMap.Find("InteractiveSpeakGroup")!.Value.ApplySite);
    }

    [Fact]
    public void InteractiveSpeakGroup_real_script_bank_or_isolated()
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
                if (raw.Contains(".InteractiveSpeakGroup", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "HERO.InteractiveSpeakGroup FATHER,TEXT_QST_FOO,2";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("InteractiveSpeakGroup", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-isg",
            ["ScriptFrame FALSE", line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".InteractiveSpeakGroup", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.NotEmpty(runtime.Dialogue.GroupLines);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-isg.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-interactivespeakgroup.txt"),
            """
            InteractiveSpeakGroup 00CC2C76 / apply 00CC2CCD
              ebx actor; arg0+1+2 required
              vtbl+48; resolve arg0 280/288
              vtbl+1456(handle,1,1) session
              vtbl+1460(session,listener)
              009E1960 atoi arg2 count
              each: prefix + "_" + 10*(i) via 1464
              leftover x count; jmp 00CC707C
              not InteractiveSpeak
            Voice/group body UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void DataSpeak_concatenates_key_not_Speak_listener_text()
    {
        var runtime = ScriptRuntime.Detached();
        var first = new ScriptInterpreter("ds1", ["HERO.DataSpeak FOO,BAR"]);
        first.RunUntilYield(runtime);
        Assert.True(first.Yielded);
        Assert.Equal(("HERO", "FOOBAR", 0), runtime.Dialogue.DataSpeaks[0]);
        var second = new ScriptInterpreter("ds2",
        [
            "GUARD.DataSpeak _A,_B,PREFIX,sequence",
            "HERO.DataSpeak",
        ]);
        second.RunUntilYield(runtime);
        Assert.True(second.Yielded);
        Assert.Equal(2, runtime.Dialogue.DataSpeaks.Count);
        Assert.Equal(("GUARD", "PREFIX_A_B", 3), runtime.Dialogue.DataSpeaks[1]);
        Assert.Equal("DataSpeak", runtime.Dialogue.Session?.Verb);
        Assert.Equal(0x00CC2991u, ScriptCommandMap.Find("DataSpeak")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("Speak")!.Value.ApplySite,
            ScriptCommandMap.Find("DataSpeak")!.Value.ApplySite);
    }

    [Fact]
    public void DataSpeak_real_script_bank_or_isolated()
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
                if (raw.Contains(".DataSpeak", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "HERO.DataSpeak FOO,BAR,DATA,norepeat";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("DataSpeak", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-ds", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".DataSpeak", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Yielded);
        Assert.Single(runtime.Dialogue.DataSpeaks);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-ds.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-dataspeak.txt"),
            """
            DataSpeak 00CC293A / apply 00CC2991
              ebx actor; arg0+arg1 required
              empty arg2: concat arg0+arg1
              DATA: 004AA900 name + arg0 + arg1
              else: arg2 + arg0 + arg1
              arg3 00BFEBA8 random=1 norepeat=2 sequence=3
              00CD3187; vtbl+52(handle,key,mode,0,1,0)
              leftover poll vtbl+104; jmp 00CC7081
              not Speak listener+text
            Voice table UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void FightWith_is_vtbl_2388_not_AILevel()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("fw",
        [
            "ScriptFrame FALSE",
            "GUARD.FightWith HERO",
            "BANDIT.FightWith",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal("HERO", runtime.World.FightTargets["GUARD"]);
        Assert.Equal(2388, runtime.World.FightVtbl["GUARD"]);
        Assert.False(runtime.World.FightTargets.ContainsKey("BANDIT"));
        Assert.Equal(0x00CC1D41u, ScriptCommandMap.Find("FightWith")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("AILevel")!.Value.ApplySite,
            ScriptCommandMap.Find("FightWith")!.Value.ApplySite);
    }

    [Fact]
    public void FightWith_real_script_bank_or_isolated()
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
                if (raw.Contains(".FightWith", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "GUARD.FightWith HERO";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("FightWith", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-fw",
            ["ScriptFrame FALSE", line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains(".FightWith", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Equal(parsed.Arg(0), runtime.World.FightTargets[parsed.Target ?? ""]);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-fw.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-fightwith.txt"),
            """
            FightWith 00CC1CEA / apply 00CC1D41
              ebx actor; arg0 required
              00CD3187 table; resolve target 280/288
              00CD2770 slot drop; vtbl+32(handle,actor,0)
              vtbl+2388(actor,target); leftover if yield
              jmp cleanup
              not AILevel HIGH/MEDIUM/4
            Combat brain UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void SlideTeleport_lerps_count_steps_vtbl_1892()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.World.Positions["A"] = new System.Numerics.Vector3(0, 0, 0);
        runtime.World.Positions["B"] = new System.Numerics.Vector3(10, 0, 0);
        var instant = new ScriptInterpreter("sli", ["HERO.SlideTeleport A,B,4"]);
        instant.RunUntilYield(runtime);
        Assert.True(instant.Finished);
        Assert.Equal(10f, runtime.World.Positions["HERO"].X);
        Assert.Equal("B", runtime.World.LookTargets["HERO"]);
        runtime.World.Positions["HERO"] = new System.Numerics.Vector3(0, 0, 0);
        var wait = new ScriptInterpreter("slw", ["HERO.SlideTeleport A,B,2,TRUE"]);
        wait.RunUntilYield(runtime);
        Assert.True(wait.Yielded);
        Assert.Equal(ExecutionKind.WaitOperation, wait.CurrentWaitKind);
        runtime.Movement.Tick(1f, runtime.World);
        Assert.Equal(5f, runtime.World.Positions["HERO"].X);
        runtime.Movement.Tick(1f, runtime.World);
        Assert.Equal(10f, runtime.World.Positions["HERO"].X);
        wait.Resume(runtime);
        Assert.True(wait.Finished);
        Assert.Equal(0x00CC57F7u, ScriptCommandMap.Find("SlideTeleport")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("Teleport")!.Value.ApplySite,
            ScriptCommandMap.Find("SlideTeleport")!.Value.ApplySite);
        var empty = new ScriptInterpreter("sl0", ["HERO.SlideTeleport A"]);
        empty.RunUntilYield(runtime);
        Assert.True(empty.Finished);
    }

    [Fact]
    public void SlideTeleport_global_uses_actor_from_to()
    {
        var runtime = ScriptRuntime.Detached();
        runtime.World.Positions["A"] = new System.Numerics.Vector3(0, 2, 0);
        runtime.World.Positions["B"] = new System.Numerics.Vector3(0, 8, 0);
        var interp = new ScriptInterpreter("slg",
            ["ScriptFrame FALSE", "SlideTeleport HERO,A,B,3"]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(8f, runtime.World.Positions["HERO"].Y);
    }

    [Fact]
    public void SlideTeleport_real_script_bank_or_isolated()
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
                if (raw.Contains("SlideTeleport ", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "HERO.SlideTeleport MK_A,MK_B,2";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("SlideTeleport", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        string from;
        string to;
        string actor;
        if (parsed.Family == CommandFamily.Entity)
        {
            actor = parsed.Target ?? "HERO";
            from = parsed.Arg(0);
            to = parsed.Arg(1);
        }
        else
        {
            actor = parsed.Arg(0);
            from = parsed.Arg(1);
            to = parsed.Arg(2);
        }

        runtime.World.Positions[from] = new System.Numerics.Vector3(1, 0, 0);
        runtime.World.Positions[to] = new System.Numerics.Vector3(5, 0, 0);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-slide",
            ["ScriptFrame FALSE", line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.Contains("SlideTeleport ", StringComparison.OrdinalIgnoreCase));
        if (isolated.Yielded)
        {
            for (var i = 0; i < 128 && isolated.Yielded; i++)
            {
                runtime.Movement.Tick(1f, runtime.World);
                isolated.Resume(runtime);
            }
        }

        Assert.True(isolated.Finished);
        Assert.True(runtime.World.Positions.ContainsKey(actor));
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-slide.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-slideteleport.txt"),
            """
            SlideTeleport entity 00CC57A1 / apply 00CC57F7
              from+to required; count atoi default 100
              00CBF9DE + 004AA980 both ends
              pos = src+(dest-src)*i/count; i=1..count
              vtbl+1892 each step; IsTrue(arg3) leftover/step
              IsFalse(arg4) zeros yaw increment
            Global 00CC5A3A / apply 00CC5A8D
              actor,from,to required; leftover if [ebp+103]
              IsFalse(arg4) uses from 004AAA40 yaw
              final vtbl+1892 snap
            Yaw 1/(2pi)*atan UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void AskQuestion_polls_vtbl_156_until_answer()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("aq",
        [
            "AskQuestion TEXT_QST_FOO",
            "AskQuestion TEXT_AGAIN",
            "SetTime 12",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Yielded);
        Assert.False(interp.Finished);
        Assert.Equal(ExecutionKind.WaitOperation, interp.CurrentWaitKind);
        Assert.Equal("AskQuestion", runtime.Dialogue.Session!.Verb);
        Assert.Equal("TEXT_QST_FOO", runtime.Dialogue.Session.Text);
        Assert.Equal("TEXT_OBJECT_HERO_ANSWER_YES", runtime.Dialogue.Session.YesLabel);
        Assert.Equal("TEXT_OBJECT_HERO_ANSWER_NO", runtime.Dialogue.Session.NoLabel);
        Assert.Null(runtime.Dialogue.Session.Answer);
        Assert.True(runtime.Dialogue.Session.HasHandle);
        runtime.Dialogue.Answer(1);
        interp.Resume(runtime);
        Assert.True(interp.Finished);
        Assert.Equal(1, runtime.Dialogue.Session.Answer);
        Assert.Equal("TEXT_QST_FOO", runtime.Dialogue.Session.Text);
        Assert.Equal(0x00CC5FD4u, ScriptCommandMap.Find("AskQuestion")!.Value.ApplySite);
        var custom = new ScriptInterpreter("aq2",
            ["AskQuestion TEXT_QST_BAR,TEXT_YES_CUSTOM,TEXT_NO_CUSTOM"]);
        custom.RunUntilYield(runtime);
        Assert.Equal("TEXT_YES_CUSTOM", runtime.Dialogue.Session!.YesLabel);
        Assert.Equal("TEXT_NO_CUSTOM", runtime.Dialogue.Session.NoLabel);
        var empty = new ScriptInterpreter("aq0", ["AskQuestion"]);
        empty.RunUntilYield(runtime);
        Assert.True(empty.Finished);
    }

    [Fact]
    public void AskQuestion_real_script_bank_or_isolated()
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
                if (raw.StartsWith("AskQuestion ", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "AskQuestion TEXT_OBJECT_HERO_ANSWER_YES";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("AskQuestion", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-ask", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("AskQuestion ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Yielded);
        Assert.Equal(ExecutionKind.WaitOperation, isolated.CurrentWaitKind);
        Assert.Equal(parsed.Arg(0), runtime.Dialogue.Session!.Text);
        var yes = parsed.Arg(1);
        if (yes.Length == 0)
            yes = "TEXT_OBJECT_HERO_ANSWER_YES";
        Assert.Equal(yes, runtime.Dialogue.Session.YesLabel);
        runtime.Dialogue.Answer(0);
        isolated.Resume(runtime);
        Assert.True(isolated.Finished);
        Assert.Equal(0, runtime.Dialogue.Session.Answer);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-ask.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-askquestion.txt"),
            """
            AskQuestion 00CC5F81 / apply 00CC5FD4
              arg0 00403A00 empty skip 00CC7081
              [ebp-38] != 0 skip 00CC7081
              default YES TEXT_OBJECT_HERO_ANSWER_YES
              default NO  TEXT_OBJECT_HERO_ANSWER_NO
              arg1/arg2 0099EFB0 overwrite if non-empty
              vtbl+1468([ebp-44],1)
              vtbl+456(question,yes,no,caption,1)
              leftover +28; poll vtbl+156 until esi>=0
              esi!=0 → [ebp-180]=1 else 0; jmp 00CC2C6B
            Question UI / input UNREAD (Runtime PARTIAL)
            """);
    }

    [Fact]
    public void GiveGold_real_script_bank_or_isolated()
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
                if (raw.StartsWith("GiveGold ", StringComparison.OrdinalIgnoreCase) &&
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

        line ??= "GiveGold 100";
        hit ??= bank.Entries[0];
        var parsed = ScriptLine.Parse(line);
        Assert.Equal("GiveGold", parsed.Verb);
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        var isolated = new ScriptInterpreter(hit.InstanceName + "-gold", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("GiveGold ", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        ScriptLine.TryInt(parsed.Arg(0), out var amount);
        Assert.True(runtime.World.HeroGold >= amount);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-gold.txt"));
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
    public void RemoveHeroClothes_clears_wear_not_hair()
    {
        var runtime = ScriptRuntime.Detached();
        var interp = new ScriptInterpreter("rhc",
        [
            "HeroWear OBJECT_HERO_NO_HAT",
            "HeroHair OBJECT_HERO_HAIR_YOUNG_01",
            "RemoveHeroClothes",
        ]);
        interp.RunUntilYield(runtime);
        Assert.True(interp.Finished);
        Assert.Empty(runtime.World.HeroClothes);
        Assert.Equal(["OBJECT_HERO_HAIR_YOUNG_01"], runtime.World.HeroHairs);
        Assert.Equal(0x00CC92EDu, ScriptCommandMap.Find("RemoveHeroClothes")!.Value.ApplySite);
        Assert.NotEqual(
            ScriptCommandMap.Find("HeroWear")!.Value.ApplySite,
            ScriptCommandMap.Find("RemoveHeroClothes")!.Value.ApplySite);
    }

    [Fact]
    public void RemoveHeroClothes_real_script_bank_or_isolated()
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
                if (raw.Equals("RemoveHeroClothes", StringComparison.OrdinalIgnoreCase) ||
                    raw.StartsWith("RemoveHeroClothes ", StringComparison.OrdinalIgnoreCase))
                {
                    line = raw;
                    hit = entry;
                    break;
                }
            }

            if (line is not null)
                break;
        }

        line ??= "RemoveHeroClothes";
        hit ??= bank.Entries[0];
        var runtime = ScriptRuntime.Detached();
        runtime.Load(bank, install);
        runtime.World.ApplyHeroWear("OBJECT_HERO_NO_HAT");
        var isolated = new ScriptInterpreter(hit.InstanceName + "-clothes", [line]);
        isolated.RunUntilYield(runtime);
        Assert.Contains(isolated.Executed, l =>
            l.StartsWith("RemoveHeroClothes", StringComparison.OrdinalIgnoreCase));
        Assert.True(isolated.Finished);
        Assert.Empty(runtime.World.HeroClothes);
        var dest = Path.Combine(
            @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
        Directory.CreateDirectory(dest);
        runtime.Trace.Write(Path.Combine(dest, hit.InstanceName + "-clothes.txt"));
        File.WriteAllText(
            Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
                "recover-removeheroclothes.txt"),
            """
            RemoveHeroClothes 00CC929B / apply 00CC92ED
              no args; vtbl+756(); jmp 00CD17FD
              does not take a name; not HeroWear vtbl+760
            Clothes mesh unread (Runtime PARTIAL)
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
