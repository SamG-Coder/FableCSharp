# Maze CS last line vs first Guild-take command

Investigation only. No production `src/` or `tests/` edits.

Milestone ends on Maze persist vector 0 last line
`PlayMusic MUSIC_SET_NULL,FALSE` (`00CC8EAC`).
`00D3BC60` Guild take is **OUT**. Host
`RegionTravel.MilestoneEntersGuildTake=false`.

Do **not** start at Leave / Init Game / first no-save
Lookout. This is leftover `S_QNOVI` after
`AttackOver=1`: `00DBDE40` → `00DBE3C0` →
`00DBEB20` → `00CBFB7D("CS_OAKVALEINTRO_HESDEADJIM")`.

Do **not** run `S_QGT` / `00D3BC60` / `GuildArrivalHSP`
/ `CS_GUILD_ARRIVE`. Those take the player to the Guild.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `assembly/compiled-defs/script/0485-CS_OAKVALEINTRO_HESDEADJIM.md`;
`0236-CS_GUILD_ARRIVE.md`;
`listing-00cc0000.txt` `00CC8D9B` / `00CC8EAB` /
`00CD0987` / `00CE7AAF`;
`listing-00d00000.txt` `00D3BC60`;
`listing-00d40000.txt` `00D44FBC`;
`listing-00d80000.txt` `00DBE22F`–`00DBE247` /
`00DBEB20`–`00DBEF32`;
`playmusic-interpreter-00cc8eac-00cc8eac.md`;
`RegionTravel.MazeCutscene*` / `GuildTakeFn` /
`MilestoneEntersGuildTake`;
`ScriptCommandMap` PlayMusic / FadeOut / EnableSounds;
`GlobalDispatcher` PlayMusic / EnableSounds;
`ScriptInterpreter.TryFadeSpecialCase` / `ApplySkipList`;
sibling `proofs/maze-pre-guild-stop` (stop VA; host
constants there are stale),
`proofs/raid-avi-attackover-live`,
`proofs/script-playmusic`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Exact last **in-milestone** command? | `PlayMusic MUSIC_SET_NULL,FALSE` at interpreter token `00CC8EAC`. Vector 0 index **88** / line **89 of 89**. | **PROVEN** |
| Exact first **out-of-milestone** command? | Guild take `00D3BC60` (`sub esp, 36` then `"GuildArrivalHSP"`). First Guild **gameflow** push is earlier: `00CE7AAF` `"Q_GuildTraining"`. First OUT **cutscene** line is `CS_GUILD_ARRIVE[0]` `FadeOut 0.5,0` at `00D44FBC`. | **PROVEN** |
| Host stop gate MATCH? | **Yes.** `MazeCutsceneStop=00CC8EAC`, `MazeCutsceneLastCommand="PlayMusic MUSIC_SET_NULL,FALSE"`, `GuildTakeFn=00D3BC60`, `MilestoneEntersGuildTake=false`. Tests lock all four. | **MATCH** |
| Any host skip of required Maze lines? | **No skip-vector skip** (`FirstSeenCutsceneSkipFires=false`; `ApplySkipList` not auto-called). **No** `FadeOut 0.5,0` head special-case (Maze `[0]` is bare `FadeOut`). Live `StartNewGame` **never starts** this CS — omits all 89 as leftover, not a runner skip. If the CS were started, `EnableSounds FALSE` host apply always unmutes (**PARTIAL** vs native mute). | **MATCH** no skip of vector 0; live omit **LEFTOVER** |

**Stop instruction VA: `00CC8EAC`.** Runner returns
`00DBEE7F`. Do not enter `00D3BC60`.

---

## Verdict

**Last in = `PlayMusic MUSIC_SET_NULL,FALSE` (`00CC8EAC`).
First out = `00D3BC60` Guild take. Host gate MATCH.**

Bank `0485` persist vector 0 is **89** CStrings.
Last is `PlayMusic MUSIC_SET_NULL,FALSE`. Opcode
`00CC8EAC` looks up via `009E5120` then `vtbl+2784`,
`jmp 00CD17FD`. List ends. `00CBFB7D` returns to
`00DBEE7F` (unmute / unpause). Scene is still Oakvale
(`CAM_OVID_*` / `MK_OVID_*` / `Q__OakValeIntro_PostAttack`).

`00D3BC60` is a **different** function (`S_QGT` vtbl
`0x012CD458`+8). First work is `"GuildArrivalHSP"` then
`"LookoutPoint"` then `"Q_GuildTraining"` then
`00D44CB0` → `00D44FBC` `CS_GUILD_ARRIVE`. That is
**OUT**. Host `MilestoneEntersGuildTake=false` **MATCH**es
the native stop.

Required Maze vector 0 must run to that last PlayMusic.
Host does **not** fire skip vector 1 and does **not**
match the `FadeOut 0.5,0` special-case against Maze
`[0]`. Live Present never pumps this CS
(`StartNewGame` only starts father leftover). That is
omit of a later leftover, not a skip of required lines
inside the runner.

---

## Evidence → Original → Host → Gap

| Evidence | Original | Host | Gap |
|---|---|---|---|
| compiled-def `0485` vector 0 count **89**; last CString `PlayMusic MUSIC_SET_NULL,FALSE` | persist +60 last line | `MazeCutsceneLastCommand` same string | **MATCH** last command |
| `xrefs.tsv` `0x012D9DC8` `00DBEE5D` `fn=00DBEB20` `CS_OAKVALEINTRO_HESDEADJIM` | start wrap | `MazeCutscene` / `MazeCutsceneStart=00DBEB20` | **MATCH** name + VA |
| `listing-00d80000.txt` `00DBEE5C` push name; `00DBEE7A` `call 00CBFB7D`; next `00DBEE7F` | blocking runner | comment “returns `00DBEE7F`” | **MATCH** wrap. Host does not call it live. |
| `listing-00cc0000.txt` `00CC8EAB` `push "PlayMusic"`; token `00CC8EAC`; `009E5120` then `vtbl+2784`; `jmp 00CD17FD` | last opcode; no yield | `PlayMusicInterpreter=00CC8EAC`; `MazeCutsceneStop=00CC8EAC`; `GlobalDispatcher` PlayMusic Continue | **MATCH** stop VA + Continue. `FALSE` → `00CBEE0C` `[ebp-58]` **UNREAD** as a later gate; host ignores Arg(1). **PARTIAL** 2nd arg |
| `00CD17FD` `inc [ebp-72]` / `jb 00CC012E`; list exhausted | end of vector 0 | `CutsceneListEnd=00CD17FD` | **MATCH** end. Next insn is wrap, not Guild. |
| `00DBEE8F` `vtbl+1484(0)`; `00DBEE9B` `vtbl+1516(0)`; `00DBEF12` `"Q__OakValeIntro_PostAttack"` | still Oakvale, still `00DBEB20` | `PostAttackQuest` | **MATCH** names. **IN** leftover wrap. Not Guild. |
| `00DBE236` `call 00DBEB20`; `00DBE247` `vtbl+1488(0.5,0)` (`0x3F000000`) | native fade **after** Maze CS returns | generic FadeOut opcode | **IN** `StartOakVale`. Not Guild take. |
| `listing-00cc0000.txt` `00CE7AAF` `push "Q_GuildTraining"` after frescoes 07/09/10 | first Guild **gameflow** | none on this leftover | **PROVEN** first Guild quest push. **OUT** |
| `listing-00d00000.txt` `00D3BC60` `sub esp, 36`; `00D3BC94` `"GuildArrivalHSP"`; `00D3BCE7` `"LookoutPoint"`; `00D3BD80` `"Q_GuildTraining"`; `00D3BE0B` `call 00D44CB0` | Guild **take** | `GuildTakeFn=00D3BC60`; `EngineLifecycle.GuildArrivalHsp`; `MilestoneEntersGuildTake=false` | **MATCH** stop gate. **OUT** of milestone. Live no-save pose already uses this HSP — **LEFTOVER** vs Maze Oakvale, not this CS |
| `listing-00d40000.txt` `00D44FA1` `"CS_GUILD_ARRIVE"`; `00D44FBC` `00CBFB7D` | first OUT CS | none | **DISPROVEN** as Maze. First line `FadeOut 0.5,0` |
| `0485` vector 1 (6): `FadeOut` / `StayFadedOut` / `GamePause 0.5` / `HERO.FadeIn 0` / `MAZE.FadeIn 0` / `PlayMusic MUSIC_SET_NULL,FALSE` | skip list def+72; `00CC017C` needs `00CBEB7E` | `ApplySkipList`; `FirstSeenCutsceneSkipFires=false` | **MATCH** no auto-skip. Same last opcode if skip **did** fire |
| `00CBFB7D` head compare first line to `FadeOut 0.5,0` | Maze `[0]` is bare `FadeOut` — miss | `TryFadeSpecialCase` exact-equals `FadeSpecialCase`; `FirstSeenFadeSpecialCaseRuns=false` is father | **MATCH** Maze `[0]` not skipped by special-case |
| `00CC8D9B` `EnableSounds`; `00CBEE0C` IsFalse → `push edi` (0) else `push 1`; `vtbl+2776` | line `[2]` `EnableSounds FALSE` **mutes** | `GlobalDispatcher` `Mute(false)` always unmute; `ScriptCommandMap` apply **0** | **PARTIAL** if CS ran. Not a line drop |
| `ScriptRuntime.StartNewGame` / LiveFather `ConstructStartsCutscene=true` | father `CS_OAKVALE_INTRO_FATHER` only | `NamedScripts` has no Maze start | **LEFTOVER** omit of all 89. Not skip-vector |
| `ScriptRuntimeParityTests` four Maze/Guild constants | native stop | same values | **MATCH** gate |

Sibling `proofs/maze-pre-guild-stop` said host last-command /
start VA were missing (**DIVERGE**). Those constants
exist now. This note supersedes that host-gap row.
Native last opcode and OUT classification there still
**PROVEN**.

---

## Timeline (leftover Oakvale → Guild)

```
00DBDE40  StartOakValeWest
  spin [this+80] until AttackOver=1          // earlier milestone
  00DBE22F  call 00DBE3C0
            ENVIRONMENT_OV_POSTATTACK
  00DBE236  call 00DBEB20
    wait M_PostAttackStart / MK_OVI_DADTRIGGER
    vtbl+1516(1) / vtbl+1484(1)
    00DBEE5C  push CS_OAKVALEINTRO_HESDEADJIM
    00DBEE7A  call 00CBFB7D                   // THIS CS
      vector 0 [0..88]
      [88] PlayMusic MUSIC_SET_NULL,FALSE     00CC8EAC  ← LAST IN
      jmp 00CD17FD                            // list end
    00DBEE7F  vtbl+1484(0) / vtbl+1516(0)     // wrap; still Oakvale
    00DBEF12  Q__OakValeIntro_PostAttack      // still IN leftover
  00DBE247  vtbl+1488(0.5, 0)                 // still StartOakVale
  00DBE2D3  ret                               // still Oakvale
00CE7670  Gameflow
  wait Q_NewOakValeIntro done
  Hook_Fresco_07_OakValeRaid
  Hook_Fresco_09_TimePassing
  Hook_Fresco_10_UneasyAlliance
  00CE7AAF  Q_GuildTraining                   ← FIRST OUT gameflow
00CD6055  S_QGT bind / factory 00D50600
00D3BC60  GuildArrivalHSP + LookoutPoint      ← FIRST OUT take fn
00D44FBC  00CBFB7D("CS_GUILD_ARRIVE")         ← FIRST OUT CS
            [0] FadeOut 0.5,0
```

---

## 1. Last in-milestone command (`00CC8EAC`)

Token sits one byte into `push "PlayMusic"`
(`00CC8EAB`). Compare is `00BFEAF8` vs the line verb.

```
00CC8EAB  push "PlayMusic"
00CC8EDC  call 00BFEAF8              // miss → PlaySound 00CC8F4B
00CC8EFE  00403A00(arg0)==0 → 00CD17FD
00CC8F15  call 009E5120              // music map [0x143E900]
00CC8F1A  cmp eax, -1 → 00CD17FD
00CC8F2B  push edi
00CC8F2C  push 1
00CC8F2E  push eax
00CC8F2F  call [edx+2784]
00CC8F38  call 00CBEE0C              // IsFalse arg1 → [ebp-58]
00CC8F46  jmp 00CD17FD               // no yield
```

Dump `playmusic-interpreter-00cc8eac-00cc8eac.md`
misaligns at `00CC8EAC` (`add al, 0x19`). Listing
`00CC8EAB` is the real prefix. **PROVEN.**

Maze last line **has** `,FALSE`. Father leftover `[0]`
is `PlayMusic MUSIC_SET_NULL` with no second arg.
Do not collapse them. What `[ebp-58]` gates later is
**UNREAD**. Host `PlayMusic` uses `Arg(0)` only.
**PARTIAL** 2nd arg; last **command string** **MATCH**.

---

## 2. First out-of-milestone command (`00D3BC60`)

```
00D3BC60  sub esp, 36
00D3BC6E  mov eax, [esi+72]
00D3BC73  jne 00D3BE4B               // already arrived → GuildTrainingHSP
00D3BC94  push "GuildArrivalHSP"
          vtbl+288 / +280 / +1888    // teleport hero there
00D3BCE7  push "LookoutPoint"
00D3BD80  push "Q_GuildTraining"
00D3BD90  push "OBJECT_QUEST_CARD_TRAINING_MELEE"
00D3BE0B  call 00D44CB0              // CS_GUILD_ARRIVE wrap
00D3BE4D  push "GuildTrainingHSP"    // later arm
```

Not reached from `00DBEE7F`. Gameflow must finish
Oakvale intro, fire frescoes, then `00CE7AAF`
`Q_GuildTraining`, then `S_QGT` factory.

`CS_GUILD_ARRIVE` (`0236`) first line `FadeOut 0.5,0`.
Cameras `CAM_GTA_*`, music `MUSIC_SET_CUTSCENE_GUILD_ARRIVAL`.
**DISPROVEN** as Maze.

---

## 3. Maze vector 0 dump (89 required lines)

Authority: `0485-CS_OAKVALEINTRO_HESDEADJIM.md` vector 0.
All 89 are **IN**. Last is the stop.

| # | Command | Opcode (when recovered) |
|--:|---|---|
| 0 | `FadeOut` | `00CD0987` |
| 1 | `GamePause 0.5` | `00CC88D1` |
| 2 | `EnableSounds FALSE` | `00CC8D9B` |
| 3 | `UseCamera CAM_OVID_SHOT1,-1,NULL,0,0` | `00CC9F3A` |
| 4 | `Create CREATURE_BANDIT_GRUNT,MK_OVID_BANDIT1,BANDIT` | Create |
| 5 | `Create CREATURE_RIVAL_HERO_MAZE,MK_OIF_HERO2,MAZE` | Create |
| 6 | `DoScriptFrame 1` | |
| 7 | `HERO.Teleport MK_OVID_HERO` | |
| 8 | `BANDIT.PreloadAnim CS_KILLED_BY_MAZE` | |
| 9 | `DoScriptFrame 1` | |
| 10 | `DoCameraPreloading` | |
| 11 | `DoScriptFrame 1` | |
| 12 | `StartTimeCode` | |
| 13 | `PlayMusic MUSIC_SET_CUTSCENE_DEAD_DAD` | `00CC8EAC` |
| 14 | `DoScriptFrame 2` | |
| 15 | `NoLoadUseCamera CAM_OVID_SHOT1` | |
| 16 | `FadeIn` | global `vtbl+1496` |
| 17 | `GamePause 1.5,CLOCK` | `00CC88D1` |
| 18 | `HERO.PlayCombatAnimation CS_WALK_SLOW,FALSE,FALSE,TRUE,FALSE` | |
| 19 | `HERO.PlayCombatAnimation CS_LOOKING_DEAD_FATHER_CRYING,FALSE,FALSE,TRUE,FALSE` | |
| 20 | `HERO.PlayAnimation CS_SCARED_OF_BANDIT,TRUE,FALSE,TRUE,FALSE` | |
| 21 | `GamePause 4.2` | |
| 22 | `UseCamera CAM_OVID_SHOT2NEW` | `00CC9F3A` |
| 23 | `HERO.Teleport MK_OVID_CRYING` | |
| 24 | `GamePause 2.0` | |
| 25 | `BANDIT.Teleport MK_OVID_BANDIT1` | |
| 26 | `GamePause 2.4` | |
| 27 | `GamePause 3.0` | |
| 28 | `DoScriptFrame 1` | |
| 29 | `GamePause 1.0` | |
| 30 | `BANDIT.PlayCombatAnimation ST_RUN_WITH_SWORD,FALSE,FALSE,TRUE,FALSE` | |
| 31 | `BANDIT.PlayCombatAnimation ST_RUN_WITH_SWORD,FALSE,FALSE,TRUE,FALSE` | |
| 32 | `BANDIT.PlayCombatAnimation ST_RUN_WITH_SWORD,FALSE,FALSE,TRUE,FALSE` | |
| 33 | `UseCamera CAM_OVID_SHOT3NEW` | |
| 34 | `GamePause 0.4` | |
| 35 | `CreateEffect MAZE_TELEPORT_IN_01,MK_OVID_BANDIT1` | `00CCBB9A` |
| 36 | `GamePause 0.8` | |
| 37 | `BANDIT.Teleport MK_OVID_BANDIT1` | |
| 38 | `UseCamera CAM_OVID_SHOT4` | |
| 39 | `GamePause 2.0` | |
| 40 | `BANDIT.PlayCombatAnimation CS_KILLED_BY_MAZE,FALSE,TRUE,FALSE,FALSE` | |
| 41 | `BANDIT.PlayAnimation STANDARD_DEAD,TRUE,FALSE,TRUE,FALSE` | |
| 42 | `BANDIT.Teleport MK_OVID_BANDIT2` | |
| 43 | `UseCamera CAM_OVID_SHOT7NEW` | |
| 44 | `GamePause 0.2` | |
| 45 | `GamePause 0.4` | |
| 46 | `HERO.PlayAnimation CS_CONFUSED_STUNNED,TRUE,TRUE,FALSE,FALSE` | |
| 47 | `GamePause 0.1` | |
| 48 | `UseCamera CAM_OVID_SHOT6NEW` | |
| 49 | `GamePause 1.0` | |
| 50 | `UseCamera CAM_OVID_SHOT7NEW` | |
| 51 | `MAZE.Teleport MK_OVID_MAZE2` | |
| 52 | `MAZE.WaitPlayAnimation CS_MAZE_SPELL_CAST` | |
| 53 | `BANDIT.Teleport MK_OVID_BANDIT3` | |
| 54 | `MAZE.PlayCombatAnimation CS_OAKVALE_WALK,FALSE,FALSE,TRUE,FALSE` | |
| 55 | `MAZE.PlayCombatAnimation CS_OAKVALE_WALK,FALSE,FALSE,TRUE,FALSE` | |
| 56 | `GamePause 1.0` | |
| 57 | `MAZE.InteractiveSpeak HERO,'TEXT_CS_048_OVID_20',TRUE` | |
| 58 | `HERO.PlayLoopingAnimation CS_LOOKING_UP,-1,FALSE,TRUE,FALSE,FALSE` | |
| 59 | `UseCamera CAM_OVID_SHOT8NEW` | |
| 60 | `MAZE.LookToThing HERO,FOREVER` | |
| 61 | `MAZE.Teleport MK_OVID_MAZE3` | |
| 62 | `MAZE.InteractiveSpeak HERO,'TEXT_CS_048_OVID_30',TRUE` | |
| 63 | `UseCamera CAM_OVID_SHOT9NEW` | |
| 64 | `MAZE.InteractiveSpeak HERO,'TEXT_CS_048_OVID_40',TRUE` | |
| 65 | `UseCamera CAM_OVID_SHOT9A` | |
| 66 | `MAZE.PlayAnimation CS_OAKVALE_TELEPORT,FALSE,TRUE,FALSE,FALSE` | |
| 67 | `GamePause 1.0` | |
| 68 | `HERO.PlayAnimation CS_TAKES_HAND_AND_TELEPORT,FALSE,TRUE,FALSE,FALSE` | |
| 69 | `GamePause 0.1` | |
| 70 | `UseCamera CAM_OVID_SHOT10` | |
| 71 | `GamePause 1.0` | |
| 72 | `MAZE.PlayAnimation CS_OAKVALE_TELEPORT_LOOP,FALSE,FALSE,TRUE,FALSE` | |
| 73 | `HERO.PlayAnimation CS_OAKVALE_TELEPORT_LOOP,FALSE,FALSE,TRUE,FALSE` | |
| 74 | `MAZE.PlayAnimation CS_OAKVALE_TELEPORT_LOOP,FALSE,FALSE,TRUE,FALSE` | |
| 75 | `HERO.PlayAnimation CS_OAKVALE_TELEPORT_LOOP,FALSE,FALSE,TRUE,FALSE` | |
| 76 | `GamePause 4.0` | |
| 77 | `CreateEffect MAZE_HERO_TELEPORT,MK_OVID_DAD` | `00CCBB9A` |
| 78 | `WaitForCamera` | `00CCA41F` |
| 79 | `UseCamera CAM_OVID_SHOT12A` | `00CC9F3A` |
| 80 | `MAZE.FadeOut 1.0,HERO` | `00CC4FF9` entity `.FadeOut` |
| 81 | `GamePause 1.5` | `00CC88D1` |
| 82 | `FadeOut` | `00CD0987` screen `vtbl+1488` |
| 83 | `GamePause 0.5` | `00CC88D1` |
| 84 | `HERO.ClearCommands TRUE` | `00CC54C4` |
| 85 | `MAZE.ClearCommands TRUE` | `00CC54C4` |
| 86 | `HERO.FadeIn 0` | `00CC4BCF` |
| 87 | `MAZE.FadeIn 0` | `00CC4BCF` |
| **88** | **`PlayMusic MUSIC_SET_NULL,FALSE`** | **`00CC8EAC` LAST IN** |

Screen fade `[82]` is **not** the last line. Entity
`MAZE.FadeOut` `[80]` is not screen `vtbl+1488`.
Bare `[0]` / `[82]` `FadeOut` uses default 0.5 / 0 /
black. **PROVEN** opcode; arg scrape **PARTIAL**.

Skip vector 1 (6 lines) if `00CBEB7E` fires:

```
FadeOut
StayFadedOut          00CD08C9
GamePause 0.5
HERO.FadeIn 0
MAZE.FadeIn 0
PlayMusic MUSIC_SET_NULL,FALSE   00CC8EAC
```

Same last opcode. Must **not** auto-run. Host
`FirstSeenCutsceneSkipFires=false`. **MATCH**.

---

## 4. Host skip of required Maze lines?

| Temptation | Native | Host | Class |
|---|---|---|---|
| Fire vector 1 skip | `00CC017C` needs `00CBEB7E`; first-seen false | `ApplySkipList` exists; New Game does not call it | **MATCH** no skip |
| Head `FadeOut 0.5,0` special-case | Maze `[0]` is bare `FadeOut`; compare misses | `TryFadeSpecialCase` exact `FadeOut 0.5,0` | **MATCH** `[0]` still required |
| Jump to Guild take after fade `[82]` | 6 more IN lines then PlayMusic then wrap | `MazeCutsceneLastCommand` is PlayMusic, not FadeOut | **MATCH** stop; **DISPROVEN** fade-as-last |
| `StartNewGame` starts Maze | only father via LiveFather | no Maze interpreter | **LEFTOVER** omit of 89, not an in-runner skip |
| `EnableSounds FALSE` as unmute | `00CBEE0C` → push 0 → `vtbl+2776` mute | `Mute(false)` always | **PARTIAL** apply if CS ran. Line still dispatched |
| Ignore `,FALSE` on last PlayMusic | `00CBEE0C` stores IsFalse | Arg(0) only | **PARTIAL** 2nd arg. Command still runs Continue |
| No-save `GuildArrivalHSP` as Maze end | OUT take pose | `EngineLifecycle.GuildArrivalHsp` first 3D | **LEFTOVER** OUT path. Not a Maze line skip |
| `CS_GUILD_ARRIVE` instead of hesdeadjim | different def | none | **DISPROVEN** |

Required vector 0 is 89 lines through PlayMusic.
Host constants do **not** drop the last line.
Host does **not** substitute Guild CS.
Live runner never reaches this leftover — that is
omit of a later quest, not skip-vector on Maze.

---

## 5. What must NOT run

Out of milestone = player taken to Guild.

| Site | What | Class |
|---|---|---|
| `00CE7AAF` | Gameflow `Q_GuildTraining` | **OUT** first gameflow |
| `00CE7B15` | state `GUILD_TRAINING` | **OUT** |
| `00CD6055` / `00CD6089` | `S_QGT` bind, factory `00D50600` | **OUT** |
| `00D3BC60` | `GuildArrivalHSP` then `LookoutPoint` | **OUT** take |
| `00D3BD81` | `Q_GuildTraining` inside take | **OUT** |
| `00D3BE4E` | `GuildTrainingHSP` | **OUT** |
| `00D44FBC` | `00CBFB7D("CS_GUILD_ARRIVE")` | **OUT** CS |

Do **not**:

1. Teleport hero to `GuildArrivalHSP` from Maze.
2. Start `S_QGT` / `00D3BC60`.
3. Run `CS_GUILD_ARRIVE`.
4. Treat screen `FadeOut` `[82]` as the stop.
5. Fire skip vector 1 to drop `[0..81]`.
6. Treat host no-save Lookout 4299 as this leftover’s end.

`00DBEE7F` unmute and `00DBEF12` PostAttack stay
Oakvale — **IN** wrap, **OUT** of the CS list, **IN**
of leftover `00DBEB20`. Milestone **command** stop is
still PlayMusic.

---

## Classifications (short)

1. **Last in-milestone command — `PlayMusic MUSIC_SET_NULL,FALSE` `00CC8EAC`. PROVEN.**
2. **First out-of-milestone command — `00D3BC60` Guild take (`GuildArrivalHSP`). First gameflow OUT `00CE7AAF` `Q_GuildTraining`. PROVEN.**
3. **Host stop gate `MilestoneEntersGuildTake=false` + last-command / stop VA. MATCH.**
4. **No host skip-vector / no `FadeOut 0.5,0` special-case skip of required Maze lines. MATCH.**
5. **Live `StartNewGame` never starts Maze — LEFTOVER omit of 89, not an in-runner skip.**
6. **`EnableSounds FALSE` / PlayMusic `,FALSE` apply PARTIAL if the CS is ever pumped.**
