# EntityTaskQueue.cs vs native entity task queue

Investigation only. No production `src` edits.

Do **not** start at `CS_OAKVALE_INTRO_FATHER` / `00DB86B0` /
`Hero.WaitTask FOO` / `Hero.SneakTo MK_OVIF_*`. That path is later
leftover `Q_NewOakValeIntro` (`00DABAC0` → `00DBDE40`), not Leave /
Init Game / first no-save Present.

Do **not** confuse **entity-task enqueue** with **region-load enqueue**
`00501450` / `006C2120`. Same English word, different objects.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE** / **INVENTED**.

Sources:

- `src/Fable.Game/Scripting/EntityTaskQueue.cs`
- `AnimationRuntime` / `MovementRuntime` in `ExecutionContext.cs`
- `EntityDispatcher.cs` (`WalkTo` / `PlayAnimation` / `WaitTask` /
  `ClearCommands`)
- `docs/runtime/COMMAND_MAP.generated.md`, `FORWARD_TREE.md` §§9–11
- `proofs/camera-after-leave/README.md`, `proofs/newgame-script/README.md`,
  `proofs/script-command-map/README.md`
- ExeIndex: player vtbl `012457FC`, AI vtbl `0127293C`,
  WaitTask `00CC0783`, WalkTo `00CC083D`/`00CC09E2`,
  PlayAnimation `00CC1527`/`004C7470`,
  `CActionPlayAnimation` `00903570`
- RTTI `CTCScriptedControl` / `CAction*@NTCScriptedControl`
- `EngineLifecycleTests` (`Init_quests_004B4260_*`,
  `No_save_does_not_activate_Q_NewOakValeIntro`);
  `WorldSceneTests` WaitTask/SneakTo pins;
  `ScriptRuntimeArchitectureTests.WalkTo_writes_destination_and_entity_task`

---

## Verdict

**Native is not a C# `Task` queue.** It is one
`CTCScriptedControl` component per Thing plus `CAction*` objects
(`CActionMoveTo`, `CActionPlayAnimation`,
`CActionWaitForThingToFinishPerformingTasks`, …).

**`EntityTaskQueue` is a host one-slot model** (`Dictionary` keyed by
actor name; `Replace` cancels the prior). It is **EQUIVALENT** as a
*current-command* record, **DIVERGE** as two separate queues
(animation vs movement) and as `TickMove` interpolation.

**First enqueue after Leave of that slot is none.** No-save New Game
does not run `00CBFB7D`, so `WalkTo` / `PlayAnimation` / `WaitTask` /
`ClearCommands` do not fire. First leftover *interpreter* enqueue is
later Oakvale father. First *named* collector after Leave is
`CTCActionUseScriptedHook` on `00501450` — **not** this queue.

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend
0042F491 Init Game → 00418DCA → 004184BD
  Init World 004A6E30
  00416953 Load FinalAlbion.wld
  004B4260 START_INITIAL_QUESTS
    Q_SunnyvaleMaster / PersonalScriptMain / …
    CS_PlayCutscene 00F01760   empty; no CCutsceneDef
    Gameflow waits on inactive Q_NewOakValeIntro
004189C2 first pumps
  type-1 00CB8220 yield   // not 00CBFB7D
  (later, E8 caller UNREAD) 00501450 region walk
    006C2120 load-job enqueue     // NOT entity task
    005198B0 / 00518DC0 CTCActionUseScriptedHook
006AC910 spawn CREATURE_HERO at GuildArrivalHSP
  does not 005B37F7 DEFAULT
  does not CActionPlayAnimation / CActionMoveTo
```

`00CBFB7D` / `.WalkTo` / `.PlayAnimation` / `.WaitTask` are
**not** on this list. **PROVEN.**

---

## 1. Native objects (what the “queue” is)

### 1a. Thing slots that scripts poke

Player `CThingPlayerCreature` vtbl `012457FC` (**PROVEN** dump):

| Off | VA | Role |
|---|---|---|
| +16 | `006A9960` | dest + gait; `or [this+146],2`; sibling of WalkTo |
| +20 | `004C72B0` | WalkTo/SneakTo/RunTo apply: `mov al,1; ret 4` |
| +24 | `004C72C0` | FollowNavRoute (`vtbl+24`) |
| +28 | `006A94B0` | FollowThing (`vtbl+28`) |
| +32 | `006A9EF0` | StopFollowingThing (`vtbl+32`) |
| +72 | `004C7470` | PlayAnimation walk of `+68` 8-byte comps |
| +76 | `006AD9D0` | PlayCombatAnim; does not read name |
| +80 | `006A9510` | PlayLoopingAnim |
| +104 | `006A9550` → `00661A40` | WaitTask / leftover busy poll (`ret 4`) |

AI `CThingAICreature` vtbl `0127293C` (**PROVEN** dump):

| Off | VA | vs player |
|---|---|---|
| +16 | `008315C0` | **not** `006A9960` |
| +20 | `004C72B0` | same stub |
| +72 | `004C7470` | same PlayAnimation walk |
| +104 | `00661A40` | same leftover `ret 4` (no `006A9550` trampoline) |

Pairing “player/AI `vtbl+16` = `006A9960`” in
`RegionTravel.CreatureGoVtbl16` is **PARTIAL** (player **PROVEN**,
AI **DISPROVEN**). Pairing dest-store to `006A5D90` is **DISPROVEN**
(`006A5D90` is ObjectScale `this+156`). Dest helper named in comments
is `00662930`. Body of `006A9960` itself is **UNREAD** as a dump in
this pass.

`004C7470` walks `[this+68, this+72)` 8-byte slots; skip
`[comp+8]!=0`; else `comp.vtbl+68(name)`. Empty list → `al=1`.
Type-90 `CTCAnimationComplex` first-seen `+68` is `00686920`
(`mov al,1; ret 4`). **PROVEN** apply; pose **PARTIAL**.

### 1b. `CTCScriptedControl` / `CAction*`

RTTI + string xrefs (**PROVEN** names, ctor bodies **UNREAD**):

| Name | String / name-setter |
|---|---|
| `CTCScriptedControl` | `0x0123A514` ← `004D2EF0` type table, factory `00719A50` |
| `CActionBase@CTCScriptedControl` | `0x01380C58` |
| `CActionMoveTo` | `007E7280` |
| `CActionPlayAnimation` | `00903570` (`0099EBF0` copy name) |
| `CActionPlayCombatAnimation` | `009035F0` |
| `CActionWait` | `00903640` |
| `CActionStopScripting` | `00903680` |
| `CActionWaitForThingToFinishPerformingTasks` | `00905160` |
| `CActionFollowCreature` | `009051D0` / `009040F0` |
| `CActionTurnToFacePosition` / Talk* / Wait / Expression / … | same family |

`00903570` is a **name setter**, not enqueue. Whether
`CTCScriptedControl` stores a **list** or a **single current**
`CAction*` is **UNREAD** (no body dump). `ClearCommands` existing as a
token, and `WaitForThingToFinishPerformingTasks` (plural), make a
pure one-slot claim **PARTIAL**.

`004D2EF0` is the CTC **type-name table** (also `CTCInventoryItem`,
`CTCSoundPlayer`, …), not a constructor. **PROVEN.**

### 1c. Interpreter apply (leftover path only)

| Verb | Token | Apply | Native effect |
|---|---|---|---|
| `.WalkTo` | `00CC083D` / `0x012C25EC` | `00CC09E2` `call [edx+20]` | stub `004C72B0`; dest via player `+16` |
| `.RunTo` | `00CC0A79` | same `00CC09E2` | mode 1 |
| `.SneakTo` | `00CC0CB5` | `00CC0E5A` `call [edx+20]` | mode 2; wait `TRUE` → leftover `+104` |
| `.PlayAnimation` | `00CC14B8` | `00CC1527` → thing `+72` | `004C7470` |
| `.PlayLoopingAnim` | `00CC1731` | `00CC186C` `vtbl+80` | not +72 |
| `.WaitTask` | `00CC0783` / `0x012C25F4` | `00CC082C` `call [eax+104]` | arg unused (`FirstSeenWaitTaskReadsName=false`) |
| `.WaitPlayAnimation` | `00CC2518` | `00CC18E0` + leftover `+104` | play then poll |
| `.ClearCommands` | `0x012C2230` | TokenSite=`0` | apply **UNREAD** |
| `.FollowThing` | `00CC19F2` | `00CC1AE9` `vtbl+28` | |
| `.FollowNavRoute` | `00CC42FA` | `00CC4350` `vtbl+24` | gait run=1 sneak=2 |

WaitTask leftover: first `+104` returns nonzero (garbage `al` after
`ret 4`) → one `vtbl+28` (`00CC07E0`) → idle `00CC7081`.
**PROVEN** as leftover poll; **not** a completion table.

---

## 2. C# `EntityTaskQueue`

```
Dictionary<actor, EntityTask>   ordinal-ignore-case
Replace(actor, kind, name, dest, speed)
  prior.Cancel(); slot = new task-{n}
Clear(actor) → Cancel (Complete+Cancelled)
Tick: Walk/Run/Sneak/Follow/NavRoute → TickMove
      Slide → TickSlide (00CC5931 lerp)
      Animate* → TickAnim (no duration)
```

Two instances (**DIVERGE** vs one Thing `+104`):

| Host | Native analogue |
|---|---|
| `AnimationRuntime.Tasks` | `CActionPlayAnimation` / combat / object |
| `MovementRuntime.Tasks` | `CActionMoveTo` / Follow / NavRoute / Slide |

`WaitTask` takes `Movement.Current ?? Animation.Current` then
**YieldOnce** leftover. Native polls **one** `vtbl+104`. **DIVERGE.**

`PlayAppearanceDefault` (`005B37F7`) does **not** `Replace`. Correct
vs create (create does not play DEFAULT). **PROVEN.**

`ClearCommands` cancels **both** host queues; native apply site
unread. **PARTIAL.**

`CrowdClearActions` comment: “clear member entity tasks.” Apply
TokenSite=`0`. **UNREAD** native.

`TickMove` advances `World.Positions` toward dest. Native WalkTo
apply is `004C72B0` stub (`FirstSeenWalkToAppliesMove=false`).
Host interpolation is **LEFTOVER** vs mesh; dest write via `006A9960`
/`World.Positions` is **PROVEN** as a later script helper, not Leave.

`EntityTaskKind` vs RTTI:

| Kind | Native class | Class |
|---|---|---|
| Walk / Run / Sneak | `CActionMoveTo` (gait) | **PARTIAL** (no body) |
| Animate / LoopAnimate | `CActionPlayAnimation` | **PARTIAL**; loop is `vtbl+80` not a second RTTI |
| CombatAnimate | `CActionPlayCombatAnimation` | **PARTIAL** |
| Follow | `CActionFollowCreature` | **PARTIAL** |
| NavRoute | `vtbl+24` | **UNREAD** as `CAction*` |
| Slide | `00CC57F7` lerp | **UNREAD** as `CAction*` |
| ObjectAnimate | `vtbl+1948` | **UNREAD** as `CAction*` |

Inventing `System.Threading.Tasks` / a FIFO of C# tasks is
**INVENTED**. The type comment already forbids that.

---

## 3. First enqueue after Leave

### 3a. Interpreter / `EntityTaskQueue.Replace`

| Claim | Class | Evidence |
|---|---|---|
| `00CBFB7D` on Leave / Init Game / first pumps | **DISPROVEN** | `script-command-map` §3; `FirstSeenCallsPlayAnimationDispatcher=false` |
| WLD initial quests run father / WalkTo / WaitTask | **DISPROVEN** | `CS_PlayCutscene` empty; `HasStarted(S_PSM)==false`; Gameflow yields |
| `006AC910` enqueue PlayAnimation / MoveTo | **DISPROVEN** | create does not call `005B37F7`; no `CAction*` xref on that fn |
| `005B37F7` DEFAULT is first-seen pose | **DISPROVEN** | callers are clothing GUI `005B6881` / `PC_UI_FRAME` `005B8743` |
| Host `EngineLifecycle` `Replace` after Leave | **DISPROVEN** | `InitCharactersAndQuests` → `ScriptRuntime.Detached` + factory activate only; no interpreter |
| `ScriptRuntime.StartNewGame` as Leave enqueue | **DIVERGE** | leftover Oakvale VM (`newgame-script`) |

**Answer:** zero `EntityTaskQueue` / `CActionMoveTo` /
`CActionPlayAnimation` enqueues on the Leave path.

### 3b. Things that *are* first after Leave (not this queue)

| Event | Object | Class |
|---|---|---|
| `006C2120` | region **load-job** enqueue | **PROVEN**; **not** entity task |
| `005198B0` / `00518DC0` | collect `CTCActionUseScriptedHook` (key `0xC2`) | **PROVEN** name; occupancy **PARTIAL** |
| `CTCActionUse*` family | interactables (`UseBed`, `UseChest`, …) | **DISPROVEN** as `CAction*@NTCScriptedControl` |
| `00CDDCB0` PersonalScript_ walk | 439 things named `PersonalScript_` | **UNREAD** as `CAction` enqueue; **DISPROVEN** as `00CBFB7D` |
| `00A44880` fiber tick | Gameflow / HeroBoasts / empty PlayCutscene | **PROVEN** reuse; not a command slot |

Treating `00501450` “enqueue” as the first entity task is
**INVENTED**.

### 3c. First leftover *interpreter* slot writes (not Leave)

When `Q_NewOakValeIntro` later runs, `CS_OAKVALE_INTRO_FATHER`
(`script-command-map` §4):

| Order | Verb | Slot effect |
|---:|---|---|
| 10 | `.PlayAnimation` | anim slot (`vtbl+72`) |
| 19 | `.WaitTask FOO` | leftover `+104` YieldOnce; name unused |
| 20 | `.SneakTo MK_OVIF_HERO4,0.0,FALSE…` | move slot; no wait |
| 23 | `.WalkTo MK_OVI_ID_VW1` | move slot; no wait |
| last | `.SneakTo MK_OVIF_HERO5,0.0,TRUE` | leftover `+104` once |

That is the first *recovered* `00CBFB7D` use of the slot. It is
**LEFTOVER** vs Leave.

---

## 4. C# vs native (Leave path)

| Host | Native after msg 15 | Class |
|---|---|---|
| `new EntityTaskQueue()` on `Animation`/`Movement` | component may exist on Things; empty | **LEFTOVER** objects; empty is **EQUIVALENT** |
| `Replace` on WalkTo/PlayAnimation | `00CBFB7D` apply | **EQUIVALENT** later; **DISPROVEN** as first after Leave |
| two queues | one Thing `vtbl+104` | **DIVERGE** |
| `TickMove` lerp | `004C72B0` stub | **LEFTOVER** |
| `WaitTask` YieldOnce | leftover `+104` then idle | **EQUIVALENT** leftover; not a real wait |
| `ClearCommands` cancel both | token `0x012C2230`; apply unread | **PARTIAL** |
| `PumpUntilSettled` force-complete all slots | leftover idle | **LEFTOVER** test helper |
| `StartNewGame` father WalkTo | not Leave | **DIVERGE** |

---

## Classifications (short)

1. **Native entity task queue = `CTCScriptedControl` + `CAction*` +
   Thing `vtbl+16/+20/+72/+104`.** Not `System.Threading.Tasks`.
   **PROVEN** names/vtbls; list-vs-slot **UNREAD**.
2. **`EntityTaskQueue` = one current host slot per actor name.
   `Replace` cancels.** **PROVEN** as host. Dual anim/move instances
   **DIVERGE**.
3. **First enqueue after Leave: none of this slot. PROVEN.**
   Do not count `006C2120` or `CTCActionUseScriptedHook`.
4. **First leftover interpreter enqueue is Oakvale father
   PlayAnimation → WaitTask → SneakTo → WalkTo. LEFTOVER.**
5. **`TickMove` / `StartNewGame` filling the slot on New Game —
   DIVERGE / LEFTOVER.** Keep as command-runtime notes. Do not call
   them from Leave.

Do not invent a FIFO. Do not start New Game at `Hero.WaitTask FOO`.
