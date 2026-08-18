# Audit: `ScriptScheduler.cs` / `EntityTaskQueue.cs`

Investigation only. No production `src/` edits.

Do **not** start at `S_QNOVI` / `AttackOver` / `00DBDE40` /
`Hero.WaitTask`. That is later `Q_NewOakValeIntro`. Leave is
`0042F2A2`. First no-save type-1 does not enter `00CBFB7D`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE** / **INVENTED**.

Sources:

- `src/Fable.Game/Scripting/ScriptScheduler.cs`
- `src/Fable.Game/Scripting/EntityTaskQueue.cs`
- `ScriptRuntime.Update` / `ActivateQuest` / `InstallRecoveredBindings`
- `EngineLifecycle.PumpQuests` / `PumpQuestList` / `ActivateNamedQuest`
- `ScriptInterpreter.Resume` / `TickScriptFrame` / `TickGamePause`
- ExeIndex `microthread-update-00a44880`, `microthread-ctor-00a44740`,
  `microthread-create-00a447d0`, `microthread-resume-00a44660`,
  `microthread-has-work-00a44930`, `microthread-yield-00a44690`
- `docs/runtime/FORWARD_TREE.md` §§10–11
- `proofs/fiber-first`, `fiber-yield-first`, `entity-task-queue`,
  `audit-newgamescript`
- `ScriptSchedulerTests`, `EngineLifecycleTests` type-1 / Init Quests

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Invented tick? | **Yes, two.** (1) `ScriptScheduler.Pump` is **not** native `00A44880`. Pairing it as the type-1 tick **invents** a list-wide resume, including **N-fold** `ScriptInterpreter.Resume` per `Update`. (2) `EntityTaskQueue.Tick` / `TickMove` is a host integrator. Native WalkTo apply is `004C72B0` stub. | **INVENTED** / **DIVERGE** |
| Wrong fiber order? | **Yes, if you treat `Scheduler._fibers` as the native walk.** Native first type-1 order is `00CB8220` / `[esi+56]` tail-insert (Sunnyvale first, Gameflow last, then Core/Barrow). `00A44880` is **per-object** `vtbl+4` draining `0x13D2828` FIFO, not create-list foreach. Host Pump is create-order and ignores fiber identity. Leave path does **not** call Pump. | **DIVERGE** / **LEFTOVER** |

---

## Verdict

`ScriptScheduler` comments claim analog of `00A44880` /
`00A44660` / `00A44690`. Addresses match. Behaviour does not.

Native has **two** walks that the host collapsed into one
`List<FiberState>`:

| Native | Role | First no-save |
|---|---|---|
| `00CB8220` → `00CB7C40` | walk `[esi+56]` constructed quests | type-1 `004B4490` |
| `00A44880` | **that object's** `vtbl+4`: enqueue on `0x13D2828`, store dt at **`this+8`**, drain FIFO via `00A44660` | Gameflow Main / Personal `vtbl+24`; **not** Sunnyvale first yield |

`EntityTaskQueue.Tick` is a third invented walk: one global
`Dictionary` of actors, lerp `World.Positions` with
`Speed * dt`. Native is per-Thing `CAction*` /
`vtbl+16/+20/+72/+104`. No-save Leave enqueues **none**.

`EngineLifecycle.PumpQuests` already walks the right native
list as **notes**. It does **not** call `Scheduler.Pump` or
`EntityTaskQueue.Tick`. Wiring those into Leave would be a
regression.

Prior proofs disagree on first create VA (`00A44740` vs
`00A447D0`). **`00A44740` is the ctor that allocates. `00A447D0`
is recreate; 0 `E8`.** Host `ActivateQuest` comments / 
`ScriptFiberTable.Create` still say `00A447D0`. **DIVERGE**
label, not a second object.

---

## 1. Invented tick

### 1a. `ScriptScheduler.Pump` is not `00A44880`

Dump (`microthread-update-00a44880`):

```
if [0x13D2838] != 0:          // already inside a fiber
  00A44C20 enqueue global
  00A44C20 enqueue this
  00A44690 yield              // 009D8650 only
  restore [0x13D2838]
  ret                         // does not drain
else:
  00A44C20 enqueue this
  loop:
    00A44930 empty? ret       // [q]-[q+4] == 0
    00A44A70 / 00A44A80 pop
    009E1BC0
    fstp [this+8]             // pumping object, not pop
    00A44660(pop)             // [0x13D2838]=pop; 009D87F0(+16)
    jmp loop
```

Host (`ScriptScheduler.cs`):

```
foreach fiber in _fibers:     // create order
  fiber.DtAtPlus8 = dt
  if Dead: continue
  resume(fiber)
```

| Host | Native | Class |
|---|---|---|
| foreach every `FiberState` | `00A44880` is `vtbl+4` on **one** object | **INVENTED** as global tick |
| no `0x13D2828` / `00A44930` | FIFO work queue | **DIVERGE** |
| no nested `[0x13D2838]` yield | nested pump enqueues + `00A44690` | **DIVERGE** |
| write dt on **every** fiber | `fstp [ecx+8]` on the **callee `this`** | **DIVERGE** |
| `resume` = C# callback | `00A44660` → `009D87F0([this+16])` C++ stack | **DIVERGE** |
| skip only `Dead` | first-seen `+41==0` still takes `vtbl+4` | host “skip parked” already **DISPROVEN** |

`ScriptRuntime.Update` then makes the tick worse:

```
Movement.Tick(dt)
Animation.Tick(dt)
Scheduler.Pump(dt, fiber => {
  fiber.State = Running
  foreach interpreter in _interpreters:   // ALL of them
    if Yielded && !Blocked: Resume()
  fiber.State = Ready
})
```

Fiber identity is unused. N fibers ⇒ each yielded interpreter
`Resume`s **N times** per `Update`. `TickScriptFrame` decrements
once per Resume. `TickGamePause` adds `GamePauseIncrement` once
per Resume after phase 2. That is an **invented time base**.

`ScriptSchedulerTests.Two_fibers_and_quest_plus_cutscene`
creates two fibers then `Update(1/15)`. A `DoScriptFrame 1`
waiter would be resumed twice. The test only asserts
`Fibers.Count == 2`.

`Update` also writes `DtAtPlus8` onto the **other** list
(`ScriptFiber`), then Pump writes `FiberState.DtAtPlus8`. Two
dt slots. Native has one `+8` on the microthread object.

If `Scheduler.Fibers` is empty, `Update` falls back to a
**third** tick: one Resume per interpreter, no fiber. That
path is not `00A44880` either. **INVENTED**.

### 1b. Leave does not run this tick

| Call | Leave / first type-1 | Class |
|---|---|---|
| `EngineLifecycle.PumpQuests` | notes `00CB8220` / `00A44880` | **PROVEN** notes |
| `Runtime.Update` / `Scheduler.Pump` | unused (`EngineLifecycle` leftover comment) | **LEFTOVER** |
| `NewGameScript.Update` → `Runtime.Update` | Oakvale leftover VM | **DIVERGE** |
| `PumpUntilSettled` force-complete + `Update` | test helper | **LEFTOVER** |

First native yield after Leave is type-1 `00CB8220`,
Sunnyvale `00CDD360` → `vtbl+28` `006E7410` → `009D8650`.
That is **not** `ScriptScheduler.Pump` and **not**
`ScriptInterpreter`. **PROVEN** (`fiber-yield-first`,
`Type1_00CB8220_*`).

### 1c. `EntityTaskQueue.Tick` is invented motion

```
Tick(dt, world):
  foreach task in _byActor.Values:     // Dictionary order
    Walk/Run/Sneak/Follow/NavRoute → TickMove(Speed * dt)
    Slide → TickSlide (00CC5931 lerp)
    Animate* → TickAnim (no-op)
```

| Claim | Class |
|---|---|
| Native is a C# `Task` FIFO | **INVENTED** (type comment already forbids this) |
| One current slot per actor name, `Replace` cancels | **EQUIVALENT** as a *record*; list-vs-slot native **UNREAD** |
| Two host queues (`Animation.Tasks` + `Movement.Tasks`) | **DIVERGE** vs one Thing `vtbl+104` |
| `Tick` walks every actor from `ScriptRuntime.Update` | **INVENTED** global; native ticks `CAction*` on the Thing |
| `TickMove` writes `World.Positions` | **INVENTED** vs `004C72B0` stub (`FirstSeenWalkToAppliesMove=false`) |
| Default `ResolveSpeed` `0.3f` when script speed is 0 | **INVENTED** gait; native dest via `006A9960` / `or [this+146],2` |
| Follow / NavRoute share `TickMove` | **INVENTED** (native `vtbl+28` / `+24`) |
| `TickAnim` empty, WaitPlayAnimation leftover-polls | **LEFTOVER** / **EQUIVALENT** leftover |
| `TickSlide` `i/count` toward dest | **PARTIAL** vs `00CC5931`; unused on Leave |
| First Leave enqueue of this slot | **DISPROVEN** (none) |

`Update` ticks Movement **then** Animation before any fiber.
Native script resume does not own a pre-pass that interpolates
every scripted actor.

---

## 2. Wrong fiber order

### 2a. Native first type-1 walk (not `Scheduler.Create` order of leftovers)

`[esi+56]` tail-insert. WLD `START_INITIAL_QUESTS` first,
`user.ini` Gameflow last. First Gameflow body then **inserts**
`CoreQuestReminder` / `CheckBarrowFieldsGuards` at tail.

| # | Object | Tick | `00A44880`? |
|--:|---|---|---|
| 1 | `Q_SunnyvaleMaster` | `00CDD360` `vtbl+28` | **no** — first yield |
| 2 | `PersonalScriptMain` | `00CDDCB0` | vtbl+24 is `00A44880` |
| 3 | `PersonalScript_GlobalThings` | `00CDDCB0` | same |
| 4 | `HeroBoasts` | `00CE1AF0` | later |
| 5 | `V_HeroDolls` | Main | **UNREAD** body |
| 6 | `CS_PlayCutscene` | empty `00F01760` | vtbl+24 `00A44880` |
| 7 | Gameflow `Main` | `00CE7670` | **yes** `vtbl+4` |
| 8 | `CoreQuestReminder` | `00CEF3B0` | attached **during** 7 |
| 9 | `CheckBarrowFieldsGuards` | `00CEF550` | attached **during** 7 |

**PROVEN** order (`FORWARD_TREE` “Sunnyvale first; Gameflow last”,
`QuestPumpWalked==9`).

First *any* microthread after Leave is **before** this list:
`00507C30` → `006C26B0` → `00A44740` (world-map `+188`, 36-byte,
stack `0xFA00`). **PROVEN** (`fiber-first`). Host
`ScriptScheduler` never creates it.

### 2b. What host `Create` actually appends

`ActivateQuest`:

```
CreateFiber(name)           // ScriptRuntime._fibers
Scheduler.Create(name)      // ScriptScheduler._fibers
quest.AttachFiber(state)
```

Two lists, same append order if both are called. **DIVERGE**
representation (native is one object, handle at `+16`).

Leave `InitCharactersAndQuests` walks `World.InitialQuests`
(WLD six) then user.ini Gameflow. After `EnterGame`,
`Scheduler.Fibers.Count == 7`. **PROVEN**. That is **create**
order matching rows 1–7, **missing rows 8–9**, **missing**
world-map `006C26B0`.

`PumpQuests` walks `_activatedQuests` (skip Gameflow name) then
Main / Core / Barrow. That matches native 9. It does **not**
iterate `Scheduler.Fibers`.

If `Update` / `Pump` were used instead:

- order = those 7 creates
- Core / Barrow never `Create`'d → **wrong set**
- every fiber resumes every interpreter → **wrong pairing**
- Sunnyvale fiber would `Resume` C# interpreters; native
  Sunnyvale is `00CDD360`, not `00A44880`

`InstallRecoveredBindings` / `ScriptFiberTable.Recovered`
prepends **`S_QNOVI` + `AttackOver`**. That is the **wrong
first fiber**. `ScriptSchedulerTests` first `Create` is the
same leftover pair. **DISPROVEN** as Leave.

`QuestFactoryTable.Recovered` order is Sunnyvale, **HeroBoasts**,
Personal… — **not** WLD file order. Iterating the factory
array as activate / pump order is **DIVERGE**
(`audit-newgamescript`). Lifecycle does not do that.

`ActiveFiber => Scheduler.Fibers.LastOrDefault()` is last
**created**, not `[0x13D2838]`. After Init Quests that is
Gameflow even while Sunnyvale is the first yielder. **DIVERGE**.

### 2c. `00A44880` drain order ≠ create list

Even for objects whose `vtbl+4` **is** `00A44880`, native
order is:

1. type-1 visits the object (`00CB7C40`)
2. that call **enqueues `this`**
3. drain FIFO; nested `00A44880` **yields** and appends

Host `Pump` is “all fibers, create index, every frame.”
That is closer to a **wrong** `00CB7C40` than to `00A44880`.

`FiberState.State` (`Ready`/`Waiting`/`Running`/`Dead`) is
not native `+5` / `+41`. Pump never sets `Waiting`. Leave
quest fibers stay `Ready`. Native first type-1 parks on
`009D8650`. **DIVERGE**.

`EntityTaskQueue` “order”: `foreach (_byActor.Values)` is
insertion order of **actor names**, then Movement queue
before Animation queue. Native is per-Thing component walk,
not a global actor map. Irrelevant on Leave (empty). Later
Oakvale father is leftover `00CBFB7D`.

---

## 3. File-level map

### `ScriptScheduler.cs`

| Symbol | Native | Class |
|---|---|---|
| comment `00A44880` / `00A44660` / `00A44690` | those VAs | **PROVEN** addresses; **DIVERGE** as this type |
| `Create` | intended `00A447D0`; live first-seen `00A44740` | **DIVERGE** name |
| `FiberState.DtAtPlus8` | `[this+8]` / `009E1BC0` | **PROVEN** offset |
| `Pump` foreach | not `00A44880`; not `00CB7C40` | **INVENTED** |
| `QuestInstance.AttachFiber` | `00CB7E50` | **PARTIAL** |
| `QuestInstance.StartFactory` | `004B3CE0` / `00CB7900` | **PARTIAL** (no `Main` body) |

### `EntityTaskQueue.cs`

| Symbol | Native | Class |
|---|---|---|
| one slot / `Replace` cancel | `CTCScriptedControl` current `CAction*` | **PARTIAL** (list **UNREAD**) |
| `Tick` / `TickMove` | no script-side lerp | **INVENTED** |
| `TickSlide` | `00CC5931` | **PARTIAL** |
| `TickAnim` | leftover `+104` poll | **LEFTOVER** |
| `Clear` | `ClearCommands` apply **UNREAD** | **PARTIAL** |

---

## Classifications (short)

1. **Invented tick — PROVEN as host.** `Pump` is not
   `00A44880`. `Update` resumes every interpreter once per
   fiber. `EntityTaskQueue.TickMove` is not `004C72B0`.
2. **Wrong fiber order — PROVEN vs native type-1 if Pump
   is treated as the walk.** Native: Sunnyvale `00CDD360`
   first, Gameflow last, Core/Barrow appended, world-map
   fiber earlier and absent. Host Pump: create list, no
   Core/Barrow, leftover `S_QNOVI` if `StartNewGame`.
3. **Leave path — PROVEN unused.** `PumpQuests` notes the
   right 9-row walk. Do not call `Scheduler.Pump` /
   `Tasks.Tick` from it.
4. **`00A447D0` as first create — DISPROVEN.** Recreate
   vtbl; ctor is `00A44740`.
5. **Nested `00A44880` / `00A44C20` queue layout —
   UNREAD** as a dump of `00A44C20`/`00A44A70`. Empty
   test `00A44930` is **PROVEN**. Do not invent a second
   host queue until those bodies are walked.

Do not invent a global fiber tick. Do not pump interpreters
inside every fiber. Do not lerp WalkTo on the script pump.
Do not start the fiber list at `S_QNOVI`.
