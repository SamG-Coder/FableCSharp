# First creature movement / nav after Leave

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` / `Hero.SneakTo MK_OVIF_*`
/ `VILL1.WalkTo`. That path is later leftover `Q_NewOakValeIntro`
(`00DABAC0` → `00DBDE40`), not Leave / Init Game / first no-save
Present.

Do **not** treat **spawn placement** (TNG `CTCPhysicsStandard` /
`GuildArrivalHSP` copy) as locomotion. Movement here means a
later XYZ change, gait, A\* query, or `CTCCreatureNavigation`
tick.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `proofs/navmesh-first/README.md`,
`proofs/entity-task-queue/README.md`,
`proofs/xseq-walk-first/README.md`,
`proofs/region-travel-first/README.md`,
`proofs/tng-spawn/README.md`,
`proofs/thing-manager-activate/README.md`,
`proofs/audit-playerinterface/README.md`,
`proofs/script-entity-cmds/README.md`,
`docs/runtime/FORWARD_TREE.md` §§7–11, 15;
`RegionTravel.cs` / `EngineLifecycle.cs` / `MovementRuntime`;
listings `004A6E30` / `00A15670` / `006AC910` / `006A9DD0` /
`006A9960` / `008315C0` / `00662930` / `004C72B0` / `004C72C0` /
`006CD540` / `006CC800` / `006CD2F0` / `006CBA00` / `00416E78` /
`0041649C` / `004C5E90` / `0051F070`;
vtbl `012457FC` / `0127293C`;
`EngineLifecycleTests` / `WorldSceneTests`.

---

## Verdict

**After Leave, native registers a navigator and later places
creatures. It does not first-seen *move* any creature.**

| Question | Answer | Class |
|---|---|---|
| Creature move / nav during frontend? | **No.** 2D UI only. No world, no Things, no `00A15670`. | **DISPROVEN** |
| First nav *object* after Leave? | `00A15670` `CNavigatorManager` at Init World, then A\* / flyer `vtbl+4`. | **PROVEN** |
| First walkable mesh / A\* *query*? | Quadtree insert skipped (`+12=0`). Live query **UNREAD**. | **DISPROVEN** insert / **UNREAD** query |
| First creature *pose* after Leave? | Lookout apply: TNG props / `TRACK_NODE_BASIC`; then `006AC910` hero at `GuildArrivalHSP` `(52.688, 69.597, 36.982)`. | **PROVEN** placement |
| First creature *locomotion* (XYZ step, gait apply, WalkTo mesh)? | **None** on this spine. | **PROVEN** absence |
| First leftover scripted dest? | Oakvale father `.SneakTo` / `.WalkTo` via `00CBFB7D`. | **LEFTOVER** |

Host `TickMove` lerp and F2 WASD `FlyCamera` are **not** that
first-seen. `MovementRuntime.SeedStart` at hero spawn is a
**placement seed**, not a step.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  0042DF9E 2D UI                         // no creature, no nav
  msg 15 → [retail+41]=1
0042F2A2 Leave frontend
  FinalAlbion.wld
0042F491 Init Game → 00418DCA → 004184BD
  Init World 004A6E30
    00A15670  CNavigatorManager [world+72]
    vtbl+4 "Navigator A Star" / "Navigator flyer"
    // no creature Things yet
  00416953 Load world
    00507C30 global TNG walk (parse; not first activate)
    004B4260 START_INITIAL_QUESTS        // no 00CBFB7D
  004AE9D0 player bind slots             // not Hero XYZ
004189C2 first pumps (dummy index 0)
  004C5E90 ret
  0051F070 [world+80]+72 empty
  WorldFrame<=1: skip 004457F0 / 00446A30
  no 00501450, no 006AC910, no WalkTo
later (E8 caller UNREAD)
  00501450 LookoutPoint 00500540(1,0,0)
    006C2170
      pass 1 topology 004FF080 / 008224E0
      pass 2 objects 00521AE0
      [rec+12]==0 skip 00500230 / 0050AF10   // no CNavQuadTree
      0051E2F0 Activate Things
        first NewThing TRACK_NODE_BASIC GuardTrack
    006AC910 CREATURE_HERO @ GuildArrivalHSP
      006A9DD0 004C9D60("CTCPhysicsControlled")
      // not 004C9D60("CTCCreatureNavigation")
      // not 005B37F7 DEFAULT / 0070D580
      // not 006A9960 dest
```

`Q_NewOakValeIntro` / `Hero.SneakTo` / `VILL1.WalkTo` /
`00A1C010` A\* walk are **not** on this list. **PROVEN.**

---

## 1. Frontend — no creature nav / move

| Claim | Class | Evidence |
|---|---|---|
| Frontend Present is 2D (`0042DF9E`) | **PROVEN** | `camera-after-leave`; FORWARD_TREE §4 |
| `00A15670` during frontend | **DISPROVEN** | one `E8`: `004A6FFB` Init World |
| Hero / AI Thing during frontend | **DISPROVEN** | `006AC910` only after `00501450` |
| WASD / type-4 as walk | **DISPROVEN** | `audit-playerinterface`; type 4 is Press Start `0xE5` |
| `PlayerInterface` `004473A0` on retail | **DISPROVEN** | first construct is Init Game after Leave |

**Answer:** no creature exists to move, and no navigator is
constructed, until after Leave.

---

## 2. First nav after Leave — manager, not a step

Sibling `navmesh-first` already locks the objects. Short form:

```
004A6E30
  "Init Navigation Manager"
  00A15670  alloc 48, vtbl 0x129CA84 → [world+72]
  [nav].vtbl+4  "Navigator A Star"
  [nav].vtbl+4  "Navigator flyer"
  006B97E0 → [world+84]
```

Lookout apply then:

| Site | Role | First-seen |
|---|---|---|
| `004FF080` / `008224E0` | region topology grid | **PROVEN** load |
| `00500230` / `0050AF10` | `CNavQuadTree` insert | **DISPROVEN** (`job+12=0`) |
| `00A15890` `nav.data` | visualiser dump | **DISPROVEN** as New Game |
| `00A1C010` A\* family | code exists; `E8` self-only | live query **UNREAD** |
| `TRACK_NODE_BASIC` `GuardTrack` | first activated NewThing | **PROVEN** object; **DISPROVEN** as a walker |

That is **register A\*** and **load topology**, not **run A\***
and not a creature taking a step.

Host `InitWorldInitStages` names `"Init Navigation Manager"`
`0x00A15670`. **EQUIVALENT** order. Host does not allocate the
48-byte object. **PARTIAL.**

---

## 3. First creatures after Leave — placed, not moving

### 3a. Hero (`006AC910`)

Reached after dummy pump + `00501450` Lookout. Not frontend.
Not `00DBDE40`.

```
006AC910
  004C7380 / 0052AB20
  006A9DD0 ConstructFromParams
    00662880 parent
    0042B0A2([this+112])
    004C9D60("CTCPhysicsControlled")     // only named add
  006A06E0 / vtbl+12 basis
  004C9CA0 activate
```

| Field | Value | Class |
|---|---|---|
| Def / script / mesh | `CREATURE_HERO` / `Hero` / **4299** | **PROVEN** |
| XYZ | `GuildArrivalHSP` `(52.688, 69.597, 36.982)` | **PROVEN** copy |
| `CTCPhysicsControlled` | `004C9D60` from `006A9EAB` | **PROVEN** |
| `CTCCreatureNavigation` on create | not in `006A9DD0` / `006AC910` | **DISPROVEN** |
| `005B37F7` DEFAULT / `0070D580` | not on create | **DISPROVEN** |
| `006A9960` dest / `or [+146],2` | not on create | **DISPROVEN** |
| First dest / PALSKIN | bind locals | **PROVEN** (`xseq-walk-first`) |

`004C9CA0` is activate (`vtbl+32/+36/+40`, then `or [+146],4` /
`[+145],1`). It is **not** a move.

Host `BindRuntimeHero` writes `World.Positions["Hero"]` from
that same HSP pose. **EQUIVALENT** seed. **DISPROVEN** as a
step.

### 3b. Lookout AI

Nine `AICreature` names exist (`LookoutPointBeggar`,
`BeggarBully`, `FleeingWoman`, `WaspHelper`, `FH_Villager` ×3,
`TalkingTrader1/2`). TNG has **no** `CTCPhysicsStandard.Position`.
Native `0051FD80` still constructs them. First-seen world pose
writer **UNREAD**. Host `ResolveSubmit` drops them (`Submitted=false`).

Do not invent `BeggarHatSpawn` / `MK_BB_*` teleports as first
Present motion.

AI vtbl `0127293C`:

| Off | VA | vs player `012457FC` |
|---|---|---|
| +16 | `008315C0` | player is `006A9960` |
| +20 | `004C72B0` | same stub |
| +24 | `004C72C0` | same FollowNavRoute stub |
| +36 | `00832260` | AI ready / neighbour walk; not Leave |

`RegionTravel.CreatureGoVtbl16 = 0x006A9960` is **PARTIAL**
(player **PROVEN**, AI **DISPROVEN**). Both dest helpers call
`00662930`.

### 3c. First NewThing is not a creature

`0051E2F0` vector[0] on first Lookout apply is
`TRACK_NODE_BASIC` / `GuardTrack` at
`(76.686, 30.849, 17.517)` (`thing-manager-activate`).
Patrol **node**, not a walking Thing.

---

## 4. The move *slots* — proven bodies, unused first-seen

Player `vtbl+16` `006A9960` (**PROVEN** listing):

```
006A9960  00662930(arg)           ; dest store
          je fail
          fld [this+224]+80
          fst [this+176]          ; gait copy
          or  [this+146], 2
          al=1
```

AI `vtbl+16` `008315C0` does the same `00662930` +
`or [+146],2` + gait, then allocs extra 40-byte state
(`00834D30` at `+352`). **PROVEN** as dest, not as Leave.

`00662930` → `00838930` then `004C7990` / `00513160`.
Success does **not** write mesh XYZ.

Apply stubs (**PROVEN** 3-insn):

| Slot | VA | Body |
|---|---|---|
| WalkTo / SneakTo / RunTo `vtbl+20` | `004C72B0` | `mov al,1; ret 4` |
| FollowNavRoute `vtbl+24` | `004C72C0` | `mov al,1; ret 4` |

`FirstSeenWalkToAppliesMove=false` /
`FirstSeenSneakToAppliesMove=false` stay correct.

Interpreter that would *call* those slots is `00CBFB7D`.
Leave / Init Quests / first pumps do **not** enter it
(`FirstSeenCallsPlayAnimationDispatcher=false`;
`script-entity-cmds`). **PROVEN.**

---

## 5. `CTCCreatureNavigation` — names exist, not first-seen attach

| VA | What it is | Class vs Leave |
|---|---|---|
| `006CD540` | `0099EBF0("CTCCreatureNavigation")` then `ret 4` | **PROVEN** name setter. **DISPROVEN** as ctor / factory |
| `006CB4C0` | same pattern for `"CCreatureNavigationDef"` | **PROVEN** name setter |
| `006CD2F0` | def lookup: push name, `vtbl+56`, `009ADA10` | **PROVEN** lookup body. Callers `006CD934` / `006CDB63` / `006CDBA3` / later `00918C0A`… — **not** `006AC910` / Leave |
| `006CBA00` | `ret` | **PROVEN** stub. Sole `E8` is `004EE27C` inside type table `004EE137` |
| `006CC800` | large tick: `0049D870` WorldFrame, `00BFEC30`, `006A4D00`, `00661CD0` | **PROVEN** body. Tag `CCreatureNavigationDef\|CTCCreatureNavigation`. **0** `E8` sites (vtbl-only). First-seen call **UNREAD** |
| RTTI `CTCCreatureNavigation` `0x0137FE28` | name | **PROVEN** RTTI; occupancy **UNREAD** |

`004EE137` also pushes the string next to
`CTCPhysicsControlled` / `CTCPhysicsNavigator` — persist /
type-name table, not a Leave construct.

**Answer:** first Lookout hero is **physics-controlled**, not
creature-navigated. Do not pair `006CD540` as “factory ran.”

---

## 6. First pumps do not step Things

Dummy / early `004189C2` (FORWARD_TREE §§8, 11):

| Site | First-seen | Move? |
|---|---|---|
| `004C5E90` | `ret` | **DISPROVEN** |
| `0051F070` | `[world+80]+72==+76` → `jbe` empty | **DISPROVEN** (list clear, not a walker) |
| `006E60F0` / `006E37D0` | empty lists | **DISPROVEN** |
| `006BB990` | world+28 timer `+8/+28 += dt` | **DISPROVEN** as Thing XYZ |
| `00416E78` WorldFrame≤1 | skip `004457F0` / `00446A30` | **PROVEN** skip |
| first WorldFrame>1 `00446A30` | `al=0`, no `0041649C` | **PROVEN** |

`0041649C` itself is player-manager / fade / HUD
(`004AE9A0` / `00434A30`), **not** `006A9960`. Even when it
later runs, it is **UNREAD** as locomotion (same leftover as
`audit-playerinterface` §Open).

Native keyboard defaults are `0x6F/0x70/0x72/0x6D`, not WASD.
Host F2 `FlyCamera` WASD is **LEFTOVER** debug.

---

## 7. Leftover first *scripted* dest (not Leave)

When `Q_NewOakValeIntro` later runs (`script-command-map` /
`entity-task-queue`):

| Order | Verb | Effect |
|---:|---|---|
| … | `.PlayAnimation` | anim slot; not XYZ |
| | `.WaitTask FOO` | leftover `+104` |
| | `.SneakTo MK_OVIF_HERO4` | dest via `vtbl+16`; apply stub |
| | `.WalkTo MK_OVI_ID_VW1` | same |
| last | `.SneakTo MK_OVIF_HERO5,0.0,TRUE` | leftover poll |

Host `EntityTaskQueue.TickMove` then lerps `World.Positions`.
Native `004C72B0` does not. **LEFTOVER** vs mesh;
**DISPROVEN** as first-seen after Leave.

`3420` wake loop / `PlayLoopingAnim WALK` are **LEFTOVER**
clips (`xseq-walk-first`). They are not a nav step.

---

## 8. C# vs native (Leave path)

| Host | Native after msg 15 | Class |
|---|---|---|
| Note `"Init Navigation Manager"` | `00A15670` 48-byte object | **PARTIAL** (name only) |
| Skip `00500230` / `0050AF10` | `+12=0` | **MATCH** |
| `006AC910` at HSP; seed `World.Positions` | same pose | **MATCH** placement |
| `004C9D60("CTCPhysicsControlled")` | `006A9EAB` | **MATCH** |
| Attach `CTCCreatureNavigation` on create | not called | **MATCH** skip |
| `TickMove` toward dest | `004C72B0` stub | **LEFTOVER** |
| `StartNewGame` father WalkTo | not Leave | **DIVERGE** |
| F2 WASD `FlyCamera` | not `00416E78` | **LEFTOVER** |
| `CreatureGoVtbl16=006A9960` for AI | AI is `008315C0` | **DIVERGE** comment |

---

## Classifications (short)

1. **Frontend creature move / nav — DISPROVEN.**
2. **First nav after Leave — `00A15670` + A\* *register*.
   PROVEN.** First Lookout topology `004FF080` **PROVEN**.
   First `CNavQuadTree` / live A\* **DISPROVEN** / **UNREAD**.
3. **First creature XYZ after Leave — spawn placement.
   PROVEN.** Hero `GuildArrivalHSP`. Lookout AI pose
   **UNREAD**. `GuardTrack` is a node.
4. **First locomotion (dest apply / mesh step / nav tick) —
   none. PROVEN absence.** `00CBFB7D` off; `vtbl+20/+24`
   stubs; `006A9960` unused; `00446A30` does not
   `0041649C`; `006CC800` vtbl-only and not on this tree.
5. **`006CD540` as first nav factory — DISPROVEN.** It is a
   CString name write. Hero adds `CTCPhysicsControlled`.
6. **Oakvale SneakTo / WalkTo / host `TickMove` — LEFTOVER.**

Do not start New Game locomotion at `Hero.WalkTo` or WASD.
Do not call HSP copy a move.
