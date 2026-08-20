# Maze CS through fade-out — stop before Guild

Investigation only. No production `src/` edits.

Milestone ends **immediately before** the player is
taken to the Heroes' Guild. Maze CS
`CS_OAKVALEINTRO_HESDEADJIM` is still Oakvale.
Guild transition is **out**.

Do **not** start at Leave / Init Game / first no-save
Lookout. This is leftover `S_QNOVI` post-attack
(`00DBDE40` → `00DBEB20` → `00CBFB7D`).

Do **not** run `S_QGT` `00D3BC60` / `GuildArrivalHSP` /
`CS_GUILD_ARRIVE`. Those take the player to the Guild.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: `RegionTravel.MazeCutscene`;
`assembly/compiled-defs/script/0485-CS_OAKVALEINTRO_HESDEADJIM.md`;
`assembly/compiled-defs/script/0236-CS_GUILD_ARRIVE.md`;
`listing-00d80000.txt` `00DBEB20`–`00DBEF66`;
`listing-00d00000.txt` `00D3BC60`;
`listing-00d40000.txt` `00D44FBC`;
`listing-00cc0000.txt` `00CD0987` / `00CC8EAC` /
`00CE7AAF`;
`ScriptCommandMap` (`FadeOut` / `PlayMusic` /
`WaitForCamera` / `.FadeOut`);
`QuestFactoryTable.GameflowStateNames`;
`EngineLifecycle.GuildArrivalHsp`;
`proofs/00DBB2A7-attackover-store/README.md`,
`proofs/script-waituntil/README.md`,
`proofs/cutscene-first/README.md`.

---

## Verdict

**Last in-milestone opcode is `PlayMusic` `00CC8EAC`
(`PlayMusic MUSIC_SET_NULL,FALSE`).**

That is vector 0 command **[88]** of
`CS_OAKVALEINTRO_HESDEADJIM` (`0485`, 89 lines),
**after** the screen fade. Host name is
`RegionTravel.MazeCutscene`. Native start is
`00DBEE5C` push / `00DBEE7A` `call 00CBFB7D` inside
`00DBEB20`. Scene is still Oakvale
(`CAM_OVID_*` / `MK_OVID_*`).

Screen fade on the same list is earlier:
`FadeOut` `00CD0987` (bare, after
`MAZE.FadeOut 1.0,HERO` `00CC4FF9` and
`GamePause 1.5` `00CC88D1`). Skip vector 1 is
`FadeOut` then `StayFadedOut` `00CD08C9`; first-seen
skip does not auto-run (`FirstSeenCutsceneSkipFires=false`
on father; this CS skip body is the same
`00CC017C` / `00CBEB7E` reader — **PARTIAL** as
live skip on hesdeadjim).

| Question | Answer | Class |
|---|---|---|
| Host CS name | `RegionTravel.MazeCutscene` = `CS_OAKVALEINTRO_HESDEADJIM` | **PROVEN** |
| Native start | `00DBEB20` → `00DBEE7A` `00CBFB7D` | **PROVEN** leftover |
| Last vector-0 line | `PlayMusic MUSIC_SET_NULL,FALSE` | **PROVEN** |
| Last in-milestone opcode | `00CC8EAC` | **PROVEN** |
| Last screen `FadeOut` opcode | `00CD0987` | **PROVEN** (not last line) |
| Still Oakvale after CS? | yes — `00DBEE7F` unmute / `Q__OakValeIntro_PostAttack` | **PROVEN** |
| Guild travel in this CS? | no | **DISPROVEN** |
| First Guild-out | `00CE7AAF` `Q_GuildTraining` then `00D3BC60` / `00D44FBC` | **PROVEN** |
| Host last-command / start VA | name only | **DIVERGE** |
| Host no-save `GuildArrivalHSP` as this end | Lookout adult 4299 | **DIVERGE** (OUT path) |

**Stop instruction VA: `00CC8EAC`.**

Runner returns to `00DBEE7F`. Do not enter Guild.

---

## Timeline (leftover Oakvale → Guild)

```
00DBDE40  StartOakValeWest
  +80 AttackOver wait
  00DBE22F  call 00DBE3C0
            ENVIRONMENT_OV_POSTATTACK
            end Q_NewOakValeIntro_PreAttack
  00DBE236  call 00DBEB20
    wait M_PostAttackStart
    wait MK_OVI_DADTRIGGER
    vtbl+1516(1) / vtbl+1484(1)     // pause / mute
    00DBEE5C  push CS_OAKVALEINTRO_HESDEADJIM
    00DBEE7A  call 00CBFB7D          // THIS CS
      … vector 0 through FadeOut …
      FadeOut                         00CD0987
      GamePause 0.5                   00CC88D1
      HERO.ClearCommands TRUE         00CC54C4
      MAZE.ClearCommands TRUE
      HERO.FadeIn 0                   00CC4BCF
      MAZE.FadeIn 0
      PlayMusic MUSIC_SET_NULL,FALSE  00CC8EAC  ← LAST IN
      jmp 00CD17FD                    // list end
    00DBEE7F  vtbl+1484(0) / vtbl+1516(0)
    00DBEF12  Q__OakValeIntro_PostAttack
  00DBE247  vtbl+1488(0.5, 0)         // native fade
  00DBE2D3  ret                       // still Oakvale
00CE7670  Gameflow
  wait Q_NewOakValeIntro done
  Hook_Fresco_07_OakValeRaid
  Hook_Fresco_09_TimePassing
  Hook_Fresco_10_UneasyAlliance
  00CE7AAF  Q_GuildTraining           ← FIRST OUT
00CD6055  S_QGT bind / factory 00D50600
00D3BC60  GuildArrivalHSP + LookoutPoint
00D44CB0  00D44FBC CS_GUILD_ARRIVE    ← OUT CS
```

---

## 1. Host name

`RegionTravel.MazeCutscene` =
`"CS_OAKVALEINTRO_HESDEADJIM"`.

Locked by `ScriptRuntimeParityTests` and
`DataCatalogTests` (`script.bin` index **485**).
No start VA, no last-command string, no last
opcode. Father leftover has
`IntroCutsceneLastCommand` /
`IntroCutsceneStart` `00DB86B0`. Maze does not.

| Host | Native | Class |
|---|---|---|
| `MazeCutscene` string | `0x012D9DC8` / `00DBEE5C` | **PROVEN** name |
| `IntroCutsceneStart` analog | `00DBEB20` / `00DBEE7A` | **DIVERGE** (missing) |
| `IntroCutsceneLastCommand` analog | `PlayMusic MUSIC_SET_NULL,FALSE` | **DIVERGE** (missing) |
| `FadeOutOpcode` reuse | `00CD0987` same dispatcher | **PROVEN** opcode |
| `PlayMusicInterpreter` reuse | `00CC8EAC` same dispatcher | **PROVEN** opcode |

---

## 2. Native start (still Oakvale)

`00DBEB20` strings: `M_PostAttackStart`,
`V_OakVale`, `OBJECT_TEDDY_BEAR_UNGIVEABLE`,
`MK_OVI_DADTRIGGER`, `HERO`,
`CS_OAKVALEINTRO_HESDEADJIM`,
`Q__OakValeIntro_PostAttack`.

No `LookoutPoint` / `GuildArrivalHSP` /
`CS_GUILD_ARRIVE` / `S_QGT`. **PROVEN** Oakvale.

Wrap around the runner matches Guild CS wrap
(`00D44F8C` / `00D44F99`) but the name is Maze:

```
00DBEE43  push 1 ; call [eax+1516]
00DBEE54  push 1 ; call [edx+1484]
00DBEE5C  push "CS_OAKVALEINTRO_HESDEADJIM"
00DBEE7A  call 00CBFB7D
00DBEE7F  push 0 ; call [eax+1484]
00DBEE9B  push 0 ; call [edx+1516]
```

`00DBE3C0` runs **before** the Maze wait
(`00DBE22F` then `00DBE236`). It swaps
environment to `ENVIRONMENT_OV_POSTATTACK`.
Burned Oakvale, not Guild. **PROVEN**.

---

## 3. Vector 0 through fade-out

Bank `0485` persist vector 0 count **89**.
Authority: compiled-def + `script-bank/0485-*.md`.

Open (black, still Oakvale):

| # | Line | Opcode |
|---|---|---|
| 0 | `FadeOut` | `00CD0987` |
| 1 | `GamePause 0.5` | `00CC88D1` |
| 2 | `EnableSounds FALSE` | `00CC8D9B` |
| 3 | `UseCamera CAM_OVID_SHOT1,-1,NULL,0,0` | `00CC9F3A` |
| 4 | `Create CREATURE_BANDIT_GRUNT,MK_OVID_BANDIT1,BANDIT` | token |
| 5 | `Create CREATURE_RIVAL_HERO_MAZE,MK_OIF_HERO2,MAZE` | token |

Body (Oakvale dead-dad): `CAM_OVID_SHOT*`,
`MK_OVID_*`, `MUSIC_SET_CUTSCENE_DEAD_DAD`,
bandit kill, Maze speak
`TEXT_CS_048_OVID_20/30/40`, teleport-hand
anims. Late camera wait:

```
CreateEffect MAZE_HERO_TELEPORT,MK_OVID_DAD   00CCBB9A
WaitForCamera                                 00CCA41F
UseCamera CAM_OVID_SHOT12A                    00CC9F3A
MAZE.FadeOut 1.0,HERO                         00CC4FF9
GamePause 1.5                                 00CC88D1
FadeOut                                       00CD0987
```

`MAZE.FadeOut` is entity `.FadeOut`
(`0x012C224C`), **not** screen `vtbl+1488`.
Screen pack is global `FadeOut` `00CD0987` →
`008907E0` / `vtbl+1488`. Bare line uses
default 0.5 / 0 / black (same as first-seen
`FadeOut 0.5,0` apply). **PROVEN** opcode.
Arg scrape **PARTIAL** (bare vs `0.5,0`).

After screen fade (still this CS, still
Oakvale, screen black):

| # | Line | Opcode |
|---|---|---|
| 83 | `FadeOut` | `00CD0987` |
| 84 | `GamePause 0.5` | `00CC88D1` |
| 85 | `HERO.ClearCommands TRUE` | `00CC54C4` |
| 86 | `MAZE.ClearCommands TRUE` | `00CC54C4` |
| 87 | `HERO.FadeIn 0` | `00CC4BCF` |
| 88 | `MAZE.FadeIn 0` | `00CC4BCF` |
| **89 / last** | **`PlayMusic MUSIC_SET_NULL,FALSE`** | **`00CC8EAC`** |

`00CC8EAC` looks up via `009E5120` then
`vtbl+2784`, **`jmp 00CD17FD`**. No yield.
List ends. Runner returns `00DBEE7F`.

Skip vector 1 (def+72, 6 lines) if skip fires:

```
FadeOut
StayFadedOut          00CD08C9
GamePause 0.5
HERO.FadeIn 0
MAZE.FadeIn 0
PlayMusic MUSIC_SET_NULL,FALSE   00CC8EAC
```

Same last opcode. Vector 1 does **not**
auto-run (`00CC017C` needs `00CBEB7E`).
**PARTIAL** as live skip here.

`EnableSounds` / `StayFadedOut` live in the
dispatcher (`00CC8D9B` / `00CD08C9`). Host
`ScriptCommandMap` still records apply **0**.
**PARTIAL** apply; **PROVEN** token sites.

---

## 4. What must NOT run

Out of milestone = player taken to Guild.

| Site | What | Class |
|---|---|---|
| `00CE7AAF` | Gameflow `Q_GuildTraining` after frescoes | **OUT** |
| `00CE7B15` | state `GUILD_TRAINING` | **OUT** |
| `00CE7B8F` | `VILLAGE_GUILD_COMPLEX_INSIDE` | **OUT** |
| `00CD6055` / `00CD6089` | `S_QGT` bind, factory `00D50600` | **OUT** |
| `00D3BC60` | vtbl `0x012CD458`+8; `GuildArrivalHSP` then `LookoutPoint` | **OUT** |
| `00D3BD81` | `Q_GuildTraining` inside `00D3BC60` | **OUT** |
| `00D3BE4E` | `GuildTrainingHSP` | **OUT** |
| `00D44FBC` | `00CBFB7D("CS_GUILD_ARRIVE")` | **OUT** |

`CS_GUILD_ARRIVE` (`0236`) is a **different**
def: `CAM_GTA_*` / `MK_GTA_*` /
`MUSIC_SET_CUTSCENE_GUILD_ARRIVAL` /
`TEXT_CS_028_ARRIVAL_*`. First line
`FadeOut 0.5,0`. **DISPROVEN** as Maze CS.

Do **not**:

1. Teleport hero to `GuildArrivalHSP`.
2. Start `S_QGT` / `00D3BC60`.
3. Run `CS_GUILD_ARRIVE`.
4. Switch gameflow `OV_INTRO` → `GUILD_TRAINING`.
5. Treat host no-save Lookout 4299 as this
   leftover's end.

`00DBE247` `vtbl+1488(0.5,0)` after
`00DBEB20` is still `StartOakVale` — **IN**.
`Q__OakValeIntro_PostAttack` activate at
`00DBEF12` is still Oakvale — **IN**.

---

## 5. Host gap

| Host | Native leftover | Gap |
|---|---|---|
| `RegionTravel.MazeCutscene` | name at `00DBEE5C` | name only |
| no `MazeCutsceneStart` | `00DBEB20` / `00DBEE7A` | missing VA |
| no last-command lock | `PlayMusic MUSIC_SET_NULL,FALSE` | missing string |
| `FadeOutOpcode` `00CD0987` | Maze screen fade | opcode exists; not bound to this CS |
| `PlayMusicInterpreter` `00CC8EAC` | last line | opcode exists; not bound to this CS |
| `ScriptRuntime.StartNewGame` | father `CS_OAKVALE_INTRO_FATHER` | never starts Maze |
| `EngineLifecycle.GuildArrivalHsp` | `00D3BC60` destination | **runs OUT as first 3D** |
| `SpawnHeroFromPlayerStart` prefers `GuildArrivalHSP` | adult 4299 Lookout | **DIVERGE** vs Oakvale kid Maze CS |

Host already stands on the Guild-out HSP for
no-save Present. That is **not** this
milestone. Do not fold Maze fade into
`GuildArrivalHSP` / `CS_GUILD_ARRIVE`.

`EnableSounds` / `StayFadedOut` apply fns
remain **0** on the host map. Screen `FadeOut`
and `PlayMusic` apply are already proven for
father leftover; reuse those, do not invent
Guild travel from `MUSIC_SET_NULL`.

---

## Classifications (short)

1. **Last in-milestone opcode — `00CC8EAC`
   `PlayMusic MUSIC_SET_NULL,FALSE`. PROVEN.**
2. **Last screen fade — `00CD0987` `FadeOut`.
   PROVEN, not last line.**
3. **Maze CS still Oakvale — PROVEN.**
4. **`S_QGT` / `00D3BC60` / `CS_GUILD_ARRIVE`
   — OUT. Must not run.**
5. **Host `MazeCutscene` — name only. DIVERGE
   vs start VA + last command.**
6. **Host `GuildArrivalHSP` first spawn —
   DIVERGE (OUT path as first 3D).**
