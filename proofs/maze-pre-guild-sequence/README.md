# Oakvale raid aftermath → Maze arrives → CS → stop before Guild

Investigation only. No production `src/` or `tests/` edits.
Do **not** implement Guild travel. Do **not** invent Maze
spawn on no-save.

Milestone ends **before** the player is taken to the
Heroes' Guild. `CS_OAKVALEINTRO_HESDEADJIM` is still
Oakvale. `00D3BC60` / `GuildArrivalHSP` /
`CS_GUILD_ARRIVE` are **OUT**.

Do **not** start at Leave / Init Game / first no-save
Lookout. This leftover is `S_QNOVI` after
`AttackOver=1`: `00DBDE40` → `00DBE3C0` →
`00DBEB20` → `00CBFB7D("CS_OAKVALEINTRO_HESDEADJIM")`.

Lookout `MARKER_BASIC` `M_Maze` is a **Gameflow
marker** in `LookoutPoint.tng` (no Graphic). It is
**not** `CREATURE_RIVAL_HERO_MAZE`. No-save never
runs this leftover.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `listing-00d80000.txt` `00DBB218`–`00DBB2A7`
/ `00DBE1F3`–`00DBE2D3` / `00DBE3C0`–`00DBE4DC` /
`00DBEB20`–`00DBEF66`; `listing-00cc0000.txt`
`00CC8EAB` / `00CE7A11`–`00CE7AAF`;
`listing-00d00000.txt` `00D3BC60`;
`listing-00d40000.txt` `00D44FBC`;
`assembly/compiled-defs/script/0485-CS_OAKVALEINTRO_HESDEADJIM.md`;
`0236-CS_GUILD_ARRIVE.md`;
`startoak-tng.txt` (`M_PostAttackStart` /
`MK_OVI_DADTRIGGER` / `MK_OIF_HERO2` / `MK_OVID_*` /
`CAM_OVID_*`); TLC WAD `LookoutPoint.tng`;
`RegionTravel.MazeCutscene*` / `GuildTakeFn` /
`MilestoneEntersGuildTake`; siblings
`proofs/maze-cs-before-guild`,
`proofs/maze-pre-guild-stop`,
`proofs/raid-avi-attackover-live`,
`proofs/guild-arrival-hsp`,
`proofs/lookout-tng-walk`,
`proofs/lookout-marker-graphic`,
`proofs/lookout-brain-host`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Native sequence? | Raid AVI → `AttackOver=1` → PostAttack env + **live** burned Oakvale → walk to dad → **then** Maze CS → black / Give → Gameflow fresco names → Guild take. | **PROVEN** leftover |
| When does Maze **arrive**? | Inside `CS_OAKVALEINTRO_HESDEADJIM`, not on the post-attack walk. `Create CREATURE_RIVAL_HERO_MAZE,MK_OIF_HERO2,MAZE` then `CreateEffect MAZE_TELEPORT_IN_01` then `MAZE.Teleport MK_OVID_MAZE2`. | **PROVEN** |
| Last **interactive** live frame? | Burned Oakvale (`ENVIRONMENT_OV_POSTATTACK`), hero inside radius **5.0** of `MK_OVI_DADTRIGGER`, after `vtbl+1496` FadeIn. Before CS `FadeOut`. | **PROVEN** |
| Last **cutscene** live 3D frame? | `CAM_OVID_SHOT12A` at dad's body. Maze fading (`MAZE.FadeOut 1.0,HERO`). Teleport FX `MAZE_HERO_TELEPORT` at `MK_OVID_DAD`. | **PROVEN** |
| Last **in-milestone held** frame? | Screen **black** after vector 0 `[82]` `FadeOut`, through `[88]` `PlayMusic MUSIC_SET_NULL,FALSE` (`00CC8EAC`), wrap unmute, `00DBE247` fade 0.5. Still Oakvale. | **PROVEN** |
| First Guild-out? | Gameflow `00CE7AAF` `Q_GuildTraining`. Take fn `00D3BC60` (`GuildArrivalHSP` then `LookoutPoint`). OUT CS `00D44FBC` `CS_GUILD_ARRIVE`. | **PROVEN** |
| Lookout `M_Maze` this spawn? | **No.** `MARKER_BASIC`, no Graphic. | **DISPROVEN** |
| No-save Maze creature? | **No.** Leave never starts this CS. Do not spawn `CREATURE_RIVAL_HERO_MAZE` at Lookout. | **DISPROVEN** |

**Stop instruction VA: `00CC8EAC`.** Runner returns
`00DBEE7F`. Do **not** enter `00D3BC60`.

---

## Verdict

**Aftermath is live burned Oakvale. Maze arrives in
the Oakvale CS. Last live frame is `CAM_OVID_SHOT12A`
then held black. Guild take is OUT.**

```
raid AVI → AttackOver=1
  → ENVIRONMENT_OV_POSTATTACK + OBJECTIVE_06
  → teleport HERO to M_PostAttackStart + FadeIn     ← LIVE
  → walk to MK_OVI_DADTRIGGER (r=5)                 ← LAST INTERACTIVE
  → CS_OAKVALEINTRO_HESDEADJIM
       Create MAZE at MK_OIF_HERO2
       MAZE_TELEPORT_IN_01 / Teleport MK_OVID_MAZE2 ← MAZE ARRIVES
       speak / take-hand / teleport loop
       CAM_OVID_SHOT12A                             ← LAST 3D FRAME
       FadeOut → PlayMusic MUSIC_SET_NULL,FALSE     ← LAST HELD / LAST CMD
  → unmute, teardown PostAttack, fade 0.5, Give     ← still Oakvale
STOP — fresco names then 00D3BC60 are OUT of this
milestone's Guild-take boundary
```

Host `MilestoneEntersGuildTake=false` **MATCH**es
that stop. No-save Present already standing on
`GuildArrivalHSP` is the **OUT** path, not this
leftover's end.

---

## Native sequence (leftover only)

### 0. Raid AVI writes AttackOver (before aftermath)

`00DB97A0` tail after childhood objective 05:

```
00DBB218  CS_OAKVALE_INTRO_THERESA
00DBB238  call 00CBFB7D
00DBB248  Data\Video\1_raid_on_oak_vale_comp.xmv
00DBB260  call [edx+1476]              // PlayAVI
00DBB28D  vtbl+1492 black (0,0,0,255) 0.5s
00DBB29E  vtbl+2784(25)                // music
00DBB2A7  mov [ecx+80], 1              // AttackOver STORE
```

That wakes `00DBDE40`'s `HerosOldHouse` spin
(`00DBE1F3` / `00DBE200`). **PROVEN.** Must **not**
poke persist to skip the AVI (`raid-avi-attackover-live`).

`CS_BANDITRAID_*` is the adult raid. **DISPROVEN**
as this AVI.

### 1. Oakvale raid aftermath (live 3D, no Maze yet)

`00DBE22F` `call 00DBE3C0` **before** Maze:

```
00DBE3C9  Q__OakValeIntro_PostAttack     vtbl+1104  // ACTIVATE
00DBE3F2  Q_NewOakValeIntro_PreAttack    vtbl+1120  // teardown
00DBE420  vtbl+2584(0x41B80000)          // 23.0f; same slot as PreAttack 12s
00DBE42D  ENVIRONMENT_OV_POSTATTACK      vtbl+2624  // burned Oakvale
00DBE478  TEXT_QUEST_OAKVALE_INTRO_OBJECTIVE_06
          vtbl+1184
```

`00DBEB20` then **plays** that aftermath:

```
00DBEB32  wait M_PostAttackStart exists     // StartOakVale marker
00DBEBF3  teleport HERO there               // vtbl+288 / +280 / +1888
00DBEC45  V_OakVale
00DBEC95  OBJECT_TEDDY_BEAR_UNGIVEABLE
00DBECC1  00CBE87F(0x14)                    // story log 20
00DBED07  vtbl+1496                         // screen FadeIn
00DBED0F  wait MK_OVI_DADTRIGGER r=5.0      // 00CBE2FF 0x40A00000
```

StartOakVale TNG (`startoak-tng.txt`):

| Script | Kind | pos |
|---|---|---|
| `M_PostAttackStart` | `MARKER_BASIC` | `(86.082, 205.976, 12.952)` |
| `MK_OIF_HERO2` | `MARKER_BASIC` | `(89.906, 197.486, 12.801)` |
| `MK_OVI_DADTRIGGER` | `MARKER_BASIC` | `(48.542, 131.908, 16.344)` |
| `MK_OVID_DAD` | `MARKER_BASIC` | `(42.243, 129.097, 15.776)` |
| `MK_OVID_MAZE2` | `MARKER_BASIC` | `(37.025, 123.724, 15.751)` |
| `CAM_OVID_SHOT12A` | camera | `(37.44, 118.218, 27.801)` |

**No** `CREATURE_RIVAL_HERO_MAZE` in that TNG.
Aftermath walk is burned Oakvale, kid hero, dad
trigger. Maze is **not** in the world yet.
**PROVEN.**

`vtbl+2584(23.0f)` vs PreAttack `12.0f`: same
slot. Blocking vs set-duration **PARTIAL**.

### 2. Maze arrives (inside the CS, still Oakvale)

After the dad-trigger wait, `00DBEB20` pauses /
mutes then starts the CS. **Not** a region change.

```
00DBEE43  vtbl+1516(1)                  // pause
00DBEE54  vtbl+1484(1)                  // mute
00DBEE5C  push "CS_OAKVALEINTRO_HESDEADJIM"
00DBEE7A  call 00CBFB7D                 // blocking
```

Bank `0485` vector 0 (89). Maze spawn lines:

| # | Command | What |
|--:|---|---|
| 4 | `Create CREATURE_BANDIT_GRUNT,MK_OVID_BANDIT1,BANDIT` | bandit |
| 5 | `Create CREATURE_RIVAL_HERO_MAZE,MK_OIF_HERO2,MAZE` | **construct** (off the dad shot; near `M_PostAttackStart`) |
| 13 | `PlayMusic MUSIC_SET_CUTSCENE_DEAD_DAD` | |
| 16 | `FadeIn` | CS visible |
| 35 | `CreateEffect MAZE_TELEPORT_IN_01,MK_OVID_BANDIT1` | **visual arrive** |
| 51 | `MAZE.Teleport MK_OVID_MAZE2` | on-camera |
| 52 | `MAZE.WaitPlayAnimation CS_MAZE_SPELL_CAST` | kills bandit |
| 57/62/64 | `MAZE.InteractiveSpeak` `TEXT_CS_048_OVID_20/30/40` | |
| 66–75 | take-hand + `CS_OAKVALE_TELEPORT(_LOOP)` | **shown** teleport; **no** region |
| 77 | `CreateEffect MAZE_HERO_TELEPORT,MK_OVID_DAD` | |
| 79 | `UseCamera CAM_OVID_SHOT12A` | **last 3D camera** |

`Create` at `MK_OIF_HERO2` is the only native Maze
creature construct on this leftover. Lookout
`M_Maze` is a different file, different kind.
**PROVEN.**

The teleport-loop is **animation in Oakvale**. It
does **not** call `00D3BC60`. Guild pose is later
OUT.

### 3. Cutscene end (still Oakvale, then black)

| # | Command | Opcode |
|--:|---|---|
| 79 | `UseCamera CAM_OVID_SHOT12A` | `00CC9F3A` |
| 80 | `MAZE.FadeOut 1.0,HERO` | `00CC4FF9` entity |
| 81 | `GamePause 1.5` | `00CC88D1` |
| 82 | `FadeOut` | `00CD0987` **screen** |
| 83–85 | `GamePause 0.5` / `HERO.ClearCommands TRUE` / `MAZE.ClearCommands TRUE` | |
| 86–87 | `HERO.FadeIn 0` / `MAZE.FadeIn 0` | entity alpha, screen already black |
| **88** | **`PlayMusic MUSIC_SET_NULL,FALSE`** | **`00CC8EAC` LAST IN** |

`00CC8EAC` `jmp 00CD17FD` (list end). Runner
returns `00DBEE7F`:

```
00DBEE8F  vtbl+1484(0)                  // unmute
00DBEE9B  vtbl+1516(0)                  // unpause
00DBEEBE  V_OakVale
00DBEF12  Q__OakValeIntro_PostAttack    vtbl+1120  // TEARDOWN
00DBEF51  vtbl+2788(0)
```

Then `00DBEB20` returns into `00DBDE40`:

```
00DBE247  vtbl+1488(0.5, 0)             // screen fade; already black
00DBE264  vtbl+2068
00DBE271  vtbl+2280(1)
00DBE295  vtbl+1152 Give                // Q_NewOakValeIntro name
00DBE2D3  ret                           // still Oakvale
```

`00DBEF12` is **teardown** (`vtbl+1120`), not
activate. Activate was `00DBE3C9` (`vtbl+1104`).
**PROVEN.** Scene name is still Oakvale.

Skip vector 1 (6 lines) does **not** auto-run.
Same last opcode if it did. Host
`FirstSeenCutsceneSkipFires=false`. **MATCH.**

### 4. Guild transition (OUT — do not implement)

Give unblocks Gameflow `00CE7670` state 0
(`00893570("Q_NewOakValeIntro")` hit). Still
`SharedRun+4=0` (`OV_INTRO`):

```
00CE7A11  Hook_Fresco_07_OakValeRaid     0044BFF0
00CE7A4F  Hook_Fresco_09_TimePassing
00CE7A7F  Hook_Fresco_10_UneasyAlliance
00CE7AAF  Q_GuildTraining                ← FIRST OUT gameflow
00CE7AE7  vtbl+1116 00892EE0 / 004B4260
00CE7FFB  SharedRun+4 = 0x64             // GUILD_TRAINING arm
00CD6055  S_QGT / factory 00D50600
00D3BC60  GuildArrivalHSP + LookoutPoint ← FIRST OUT take
00D3BE0B  call 00D44CB0
00D44FBC  00CBFB7D("CS_GUILD_ARRIVE")    ← OUT CS
            [0] FadeOut 0.5,0
            CAM_GTA_* / MUSIC_SET_CUTSCENE_GUILD_ARRIVAL
            RegisterActor CutsceneMaze   // Guild Maze, not Oakvale Create
```

Fresco **names** are Gameflow intern + list bind,
not a 3D frame and not `00CBFB7D`. Visual fresco
screen **PARTIAL**. They sit **after** Maze Give
and **before** `Q_GuildTraining`. This milestone
does **not** run them as Guild travel.

`00D3BC60` first work is teleport hero to
`GuildArrivalHSP` (`vtbl+288` / `+280` / `+1888`)
then wait `LookoutPoint`. That **is** the take.
**OUT.** Do not implement it here.

---

## Last live frame before Guild

Three layers. Do not collapse them.

| Layer | Frame | Still Guild-in? |
|---|---|---|
| Last **interactive** 3D | Burned Oakvale, FadeIn, hero at `MK_OVI_DADTRIGGER` r=5 | **IN** leftover `00DBEB20`, **before** CS |
| Last **cutscene** 3D | `CAM_OVID_SHOT12A` + Maze entity fade + `MAZE_HERO_TELEPORT` | **IN** CS vector 0 `[79]`–`[81]` |
| Last **held** frame | Screen black after `[82]` `FadeOut`, music nulled `[88]`, wrap + `00DBE247` | **IN** leftover. **This is the stop.** |

Held black is still Oakvale (`CAM_OVID_*` last
camera, `Q__OakValeIntro_PostAttack` teardown,
Give of `Q_NewOakValeIntro`). It is **not**
Lookout, **not** `CAM_GTA_*`, **not** adult 4299
at `GuildArrivalHSP`.

`HERO.FadeIn 0` / `MAZE.FadeIn 0` after the
screen fade restore **entity** alpha on a **black**
buffer. They are **not** a visible Oakvale fade-in
and **not** Guild arrival.

Do **not** treat `[82]` screen `FadeOut` as the
last **command** (six more IN lines). Do **not**
treat `00DBE247` as Guild fade (`00D3BE2E` is the
take's own `vtbl+1492` **after** HSP teleport).

---

## Lookout `M_Maze` is not this spawn

No-save New Game: message 15 → Leave `0042F2A2` →
`FinalAlbion.wld` → first TNG `LookoutPoint.tng`
(`lookout-tng-walk`).

First two `NewThing`s:

```
#1  Marker  MARKER_BASIC  M_Maze            (49.669, 76.648, 35.252)  no Graphic
#2  Marker  MARKER_BASIC  M_LadyGameflow    (50.621, 78.386, 35.617)  no Graphic
```

`FindMeshId("MARKER_BASIC")` is **null**. Dump
`mesh=-`. `GuildArrivalHSP` is holy-site **#35**,
constructed later (`guild-arrival-hsp`). Hero
`006AC910` mesh **4299** **uses** that HSP.
**DISPROVEN** as Maze the rival hero.

First leftover **brain** on that Lookout walk is
`CAIBrain` on `FH_Villager` `CREATURE_BS_VILLAGER_MALE`
(`lookout-brain-host` / `lookout-brain-name`). Not
Oakvale `WaitTask`. Not Maze.

| Temptation | Class |
|---|---|
| Spawn `CREATURE_RIVAL_HERO_MAZE` at Lookout `M_Maze` on no-save | **DISPROVEN** |
| Treat `M_Maze` Graphic as Maze mesh | **DISPROVEN** (no Graphic) |
| Treat no-save `GuildArrivalHSP` 4299 as Maze CS end | **DISPROVEN** / **LEFTOVER** OUT path |
| Start this CS from Leave / dummy Pump | **DISPROVEN** (`Q_NewOakValeIntro` never activates) |
| `RegisterActor CutsceneMaze` in `CS_GUILD_ARRIVE` as this arrive | **DISPROVEN** (OUT Guild CS) |

Oakvale leftover Create uses StartOakVale marker
`MK_OIF_HERO2` `(89.906, 197.486, 12.801)`, not
Lookout `(49.669, 76.648, 35.252)`.

---

## Timeline

```
00DBB238  CS_OAKVALE_INTRO_THERESA
00DBB260  1_raid_on_oak_vale_comp.xmv
00DBB2A7  AttackOver=1

00DBDE40  spin exits
00DBE3C0  activate PostAttack / env POSTATTACK / OBJECTIVE_06
00DBEB20
  wait / teleport M_PostAttackStart
  FadeIn                                           ← LIVE AFTERMATH
  wait MK_OVI_DADTRIGGER r=5                       ← LAST INTERACTIVE
  pause / mute
  00CBFB7D CS_OAKVALEINTRO_HESDEADJIM
    [5]  Create MAZE at MK_OIF_HERO2
    [35] MAZE_TELEPORT_IN_01                       ← MAZE ARRIVES
    [51] MAZE.Teleport MK_OVID_MAZE2
    [79] CAM_OVID_SHOT12A                          ← LAST 3D
    [80] MAZE.FadeOut 1.0,HERO
    [82] FadeOut                                   ← screen black
    [88] PlayMusic MUSIC_SET_NULL,FALSE  00CC8EAC  ← LAST IN / HELD
  unmute / teardown PostAttack
00DBE247  vtbl+1488(0.5,0)
00DBE295  Give
00DBE2D3  ret                                      ← still Oakvale
                                               STOP
00CE7A11  fresco name hooks                        // after Give; not Guild take
00CE7AAF  Q_GuildTraining                          ← OUT gameflow
00D3BC60  GuildArrivalHSP / LookoutPoint           ← OUT take (do not implement)
00D44FBC  CS_GUILD_ARRIVE                          ← OUT CS
```

---

## Evidence → Original → Host → Gap

| Evidence | Original | Host | Gap |
|---|---|---|---|
| `00DBB2A7` after raid AVI | aftermath **after** store | `AttackOverStore`; live omit | **MATCH** VA. Must not poke persist |
| `00DBE3C0` env + PostAttack activate | burned Oakvale **before** Maze CS | `PostAttackQuest` / `PostAttackEnvironment` | **MATCH** names. No live run |
| `00DBEB32` / `00DBED0F` waits | live walk, no Maze creature | none | **PROVEN** dump. Host never waits |
| `0485` `[5]` Create Maze | only Oakvale Maze construct | no Create analog | **LEFTOVER** omit of 89 |
| `0485` `[79]`–`[88]` | last 3D then black then PlayMusic | `MazeCutsceneStop=00CC8EAC` / last-command string | **MATCH** stop |
| `00DBEE7F` wrap | still Oakvale | comment returns `00DBEE7F` | **MATCH** |
| `00D3BC60` HSP teleport | Guild **take** | `GuildTakeFn`; `MilestoneEntersGuildTake=false` | **MATCH** gate. Do not implement take |
| Lookout `M_Maze` | marker, no Graphic | `LoadGlobalThingsFile` parse | **MATCH** kind. **DISPROVEN** as spawn |
| No-save `GuildArrivalHSP` 4299 | OUT first 3D | `SpawnHeroFromPlayerStart` | **LEFTOVER** vs this leftover's end |

---

## What must NOT run / invent

| Site | Why | Class |
|---|---|---|
| `00CE7AAF` `Q_GuildTraining` | first OUT gameflow | **OUT** |
| `00D3BC60` HSP + `LookoutPoint` | takes the player to Guild | **OUT** |
| `00D44FBC` `CS_GUILD_ARRIVE` | Guild cameras / Guild Maze actor | **OUT** |
| Teleport hero to `GuildArrivalHSP` from Maze | that **is** the take | **OUT** |
| Spawn Maze at Lookout `M_Maze` | wrong map, wrong kind | **DISPROVEN** |
| Spawn Maze on no-save Leave | leftover never starts | **DISPROVEN** |
| `ApplyPersist(AttackOver,true)` to reach this | skips raid AVI | **DISPROVEN** |
| Fire skip vector 1 | drops `[0..81]` | **DISPROVEN** auto |
| Treat `[82]` FadeOut as last command | six more IN lines | **DISPROVEN** |
| Treat no-save 4299 as last Maze frame | OUT path | **DISPROVEN** |

---

## Classifications (short)

1. **Native sequence — raid AVI → AttackOver → live
   burned Oakvale (`M_PostAttackStart` / dad trigger)
   → Maze CS Create+teleport-in → black PlayMusic →
   Give. Guild take after that is OUT. PROVEN.**
2. **Maze arrives in `CS_OAKVALEINTRO_HESDEADJIM` at
   `MK_OIF_HERO2` / `MAZE_TELEPORT_IN_01` /
   `MK_OVID_MAZE2`. Not Lookout `M_Maze`. PROVEN.**
3. **Last interactive live frame — dad trigger r=5
   after aftermath FadeIn. Last cutscene 3D —
   `CAM_OVID_SHOT12A`. Last held frame — screen
   black through `00CC8EAC`. Still Oakvale. PROVEN.**
4. **`00D3BC60` / `GuildArrivalHSP` / `CS_GUILD_ARRIVE`
   — OUT. Do not implement Guild travel.**
5. **No-save Maze creature / Lookout `M_Maze` as this
   spawn — DISPROVEN. Do not invent it.**
