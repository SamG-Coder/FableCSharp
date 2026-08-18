# First entity-targeted script commands after Leave

Investigation only. No production `src/` edits.

Do **not** start at `CS_OAKVALE_INTRO_FATHER` / `Hero.Teleport` /
`Father.LookToThing` / `00DB86B0`. That path is later leftover
`Q_NewOakValeIntro` (`00DABAC0` → `00DBDE40` → TNG `NOVI_LiveFather`).
Leave is `0042F2A2`. First no-save pumps do not enter `00CBFB7D`,
so they never call `EntityDispatcher`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources:

- `src/Fable.Game/Scripting/EntityDispatcher.cs`
- `ScriptInterpreter.cs` (entity vs global split)
- `ScriptLine.cs` (`target.verb` → `CommandFamily.Entity`)
- `docs/runtime/COMMAND_MAP.md`, `traces/runtime-trace.txt`
- `tools/Fable.ExeIndex/out/01-sections/script-bank/0481-cs-oakvale-intro-father.md`
- `script-bank/native-sqnovi.md`
- `proofs/script-interpreter/README.md`, `script-command-map`,
  `entity-task-queue`, `newgame-script`, `camera-after-leave`,
  `dialogue-first`, `tng-spawn`, `player-bind-world`
- `EngineLifecycleTests` (`Init_quests_004B4260_*`,
  `No_save_does_not_activate_Q_NewOakValeIntro`);
  `WorldSceneTests` (`FirstSeenCallsPlayAnimationDispatcher=false`);
  `RegionTravel.FirstSeenCallsPlayAnimationDispatcher`

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| First `EntityDispatcher` / `target.verb` after Leave? | **none** | **PROVEN** |
| First Thing those cmds would hit after Leave? | **none** — runner not entered | **PROVEN** |
| First *live* Thing after Leave named `Hero`? | Lookout `CREATURE_HERO` mesh **4299** at `GuildArrivalHSP` | **PROVEN** spawn; **DISPROVEN** as a script target |
| First leftover entity lines (not Leave)? | `Hero.Teleport` then `Father.Teleport` then `Father.LookToThing` | **PROVEN** leftover |
| First leftover Things? | `Hero` + `Father` (`NOVI_LiveFather` / `CREATURE_HERO_FATHER`) | **PROVEN** leftover bind |

`EntityDispatcher` is the host analog of the entity join
`00CC707C` (actor handle in `ebx`). Native only reaches it from
runner `00CBFB7D` after a `target.verb` parse (`0099E5A0(46)`).
Leave starts quest factories, not that loop.

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend                 // no EntityDispatcher
0042F491 Init Game → 004184BD
  00416953 Load FinalAlbion.wld
    00CD6E27 00CB5C90 bind Q_NewOakValeIntro / S_QNOVI   BIND ONLY
    00507C30 START_INITIAL_QUESTS → world+172
  004B4260 six WLD names
    CS_PlayCutscene 00F01760 empty                      // no CCutsceneDef
  user.ini ActivateQuest("Gameflow")
    00CE75B0 Main; S_GF CCutsceneDef DISPROVEN
004189C2 first pumps
  00CB8220 → 00A44880
  00CE7670 state 0 yields on Q_NewOakValeIntro miss
later (E8 caller UNREAD)
  00501450 LookoutPoint
    006AC910 CREATURE_HERO ScriptName=Hero mesh 4299    // spawn, not .Teleport
```

`00DB86B0` / `Hero.Teleport` / `Father.*` / `00CC4678` are
**not** on this list. **PROVEN.**

---

## 1. What `EntityDispatcher` is

`C:\FableCSharp\src\Fable.Game\Scripting\EntityDispatcher.cs`

Host `if Eq(verb)` table for **entity** lines. Family is not a
native opcode class. `ScriptLine.Parse` sets
`Family = Entity` when the unquoted head has a `.`
(`LastIndexOf` analog of `0099E5A0(46)`).

```
ScriptInterpreter.RunUntilYield
  ScriptLine.Parse
  if Family == Entity → EntityDispatcher.Dispatch
  else               → GlobalDispatcher.Dispatch
```

Native: `ebx` null → global `jmp 00CD17FD`; else entity apply
then join `00CC707C`. **PROVEN** pairing.

First host `if` is `Teleport`. That is **not** first compared
inside `00CBFB7D` (first recovered token is `.WaitTask`
`00CC0783`). Dispatcher order **DIVERGE** vs prefix chain;
first leftover *executed* entity verb happens to be `Teleport`.

Empty `Target` continues with no apply. Unknown verb →
`Blocked("UNKNOWN")`.

---

## 2. After Leave — no entity command

| Claim | Class | Evidence |
|---|---|---|
| `00CBFB7D` on Leave / Init Game / first pumps | **DISPROVEN** | `script-interpreter` §2A; no `E8` |
| `CS_PlayCutscene` runs a `CCutsceneDef` | **DISPROVEN** | factory `00F01760` size 72; `ScriptName==null` |
| `S_PSM` / `S_GF` / `S_QNOVI` interpreter | **DISPROVEN** | `HasStarted==false`; Gameflow yields |
| Any `EntityDispatcher` verb | **DISPROVEN** | `FirstSeenCallsPlayAnimationDispatcher=false`; no `.WalkTo` / `.Teleport` / `.Speak` |
| `ScriptRuntime.StartNewGame` as Leave | **DIVERGE** | leftover Oakvale VM (`newgame-script`) |

Things that *do* exist after Leave are **not** script targets:

| Object | After Leave | Targeted by `target.verb`? |
|---|---|---|
| Lookout `CREATURE_HERO` / `ScriptName=Hero` / 4299 | later `006AC910` | **DISPROVEN** |
| `004AE9D0` player bind slots | Init Game suffix | **DISPROVEN** (not a Thing) |
| `NOVI_LiveFather` / `Father` | not constructed | **DISPROVEN** |
| Oakvale `Hero` at `MK_OVI_ID_HERO` | not this region | **DISPROVEN** |
| Lookout `CAM_GTA_*` | TNG gizmos | **DISPROVEN** (`UseCamera` is Global) |

**Answer:** after Leave there is **no** entity-targeted script
command and **no** Thing those commands would name.

---

## 3. Leftover first *interpreter* entity lines (not Leave)

When `Q_NewOakValeIntro` later runs, `00DABAC0` registers
`NOVI_LiveFather` → `00DAC2C0` → fiber `00DB8630` →
`00DB86B0` binds `Hero` / `Father` (`00CD3D2E` / `008ABD10`)
and pushes `CS_OAKVALE_INTRO_FATHER` into `00CBFB7D`.

Dump: `0481-cs-oakvale-intro-father.md`. Host trace:
`docs/runtime/traces/runtime-trace.txt`.

Head of that def is Global (`PlayMusic` / `FadeOut` /
`CameraPause`). First entity lines:

| Order | Raw | Verb | Thing | Arg / other Thing | `EntityDispatcher` |
|---:|---|---|---|---|---|
| 4 | `Hero.Teleport MK_OVI_ID_HERO,FALSE` | `Teleport` | **Hero** | marker `MK_OVI_ID_HERO` | first `if`; Continue; `World.Teleport` |
| 5 | `Father.Teleport MK_OVI_ID_DAD` | `Teleport` | **Father** | marker `MK_OVI_ID_DAD` | same |
| 6 | `Father.LookToThing Hero,FOREVER` | `LookToThing` | **Father** | look-at **Hero** | YieldOnce `vtbl+28` (arg2 empty, not IsFalse) |

That is the first leftover `00CC707C` trio. Trace `pc=4..6`
matches. Native `.Teleport` `00CC4678` → `vtbl+1892`
`0089B780`; `.LookToThing` `00CC3B3F` → `vtbl+1992` then
yield unless third arg IsFalse. **PROVEN** leftover.

Next leftover entity verbs on the same def (still not Leave):

| Later | Thing | Verb | Notes |
|---|---|---|---|
| `Hero.PlayAnimation CS_WAKING_UP_LOOP` / `_ON_STEPS` / `CS_TIRED` / `CS_LOOK_LEFT` / `ST_IDLE_SUBTLE` | Hero | `PlayAnimation` | `vtbl+72` |
| `Father.Speak` / `InteractiveSpeak` / `DialogSpeak` / `DialogadSpeak` | Father | dialogue | listener Hero / HERO / Father |
| `Hero.WaitTask FOO` | Hero | `WaitTask` | leftover `+104`; name unused |
| `Hero.SneakTo MK_OVIF_HERO4` / `MK_OVIF_HERO5` | Hero | `SneakTo` | last `TRUE` leftover once |
| `Father.PlayCombatAnimation TURNING_AC90` | Father | alias → `PlayCombatAnim` | `vtbl+76` |
| `VILL1.WalkTo MK_OVI_ID_VW1` | **VILL1** | `WalkTo` | after Global `Create …,VILL1` |
| `Father.LookInDirection 215` | Father | `LookInDirection` | |

### Which Thing (leftover)

| Script name | Native object | Class vs Leave |
|---|---|---|
| `Hero` | Oakvale intro bind, **not** Lookout 4299. Later leftover spawn is child (`CREATURE_HERO_CHILD`) on `00DBDE40`. | **LEFTOVER** |
| `Father` | TNG `NOVI_LiveFather` / `CREATURE_HERO_FATHER` factory `00DAC2C0` vtbl `012D8388` | **LEFTOVER** |
| `VILL1` | `Create CREATURE_OAKVALE_VILLAGER_FEMALE_NORMAL_MESH,MK_OVI_ID_VS1,VILL1` then `Remove VILL1` | **LEFTOVER** mid-cutscene |

Do not pair leftover `Hero.Teleport` to the Lookout
`GuildArrivalHSP` adult hero. **DISPROVEN.**

---

## 4. C# vs native (Leave path)

| Host | Native after msg 15 | Class |
|---|---|---|
| `EntityDispatcher.Dispatch` | unused | **LEFTOVER** vs Leave |
| `ScriptInterpreter` entity split | unused | **LEFTOVER** vs Leave |
| `StartNewGame` → father Teleport | invented Oakvale VM | **DIVERGE** |
| empty queues / no `.WalkTo` | no `00CBFB7D` | **EQUIVALENT** absence |

---

## Classifications (short)

1. **First entity-targeted script command after Leave — none.
   PROVEN.** `EntityDispatcher` is not on the tree.
2. **First Thing those commands hit after Leave — none. PROVEN.**
   Lookout `Hero` 4299 is a spawn, not a `target.verb`.
3. **First leftover entity trio — `Hero.Teleport` /
   `Father.Teleport` / `Father.LookToThing Hero`. PROVEN** as
   `CS_OAKVALE_INTRO_FATHER` `+60` lines 4–6. **LEFTOVER** vs Leave.
4. **Leftover Things — Oakvale `Hero` + `Father`
   (`NOVI_LiveFather`). PROVEN** bind at `00DB86B0`. **DISPROVEN**
   as Lookout 4299 / Leave construct.
5. **`EntityDispatcher` first `if` (`Teleport`) matches leftover
   first verb, not runner compare order (`.WaitTask`). PARTIAL /
   DIVERGE** as a table, **PROVEN** as leftover execute.

Do not start New Game at `Hero.Teleport`. Do not treat Lookout
`ScriptName=Hero` as the first `EntityDispatcher` target.
