# Dummy pumps before first region — host leftover vs MATCH

Investigation only. No production `src/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`,
`00DBDE40`, or StartOakVale. Dummy pumps stay on
WorldMap index **0**. First real region (later, caller
**UNREAD**) is **LookoutPoint**, not Oakvale.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: dummy pumps before first region. Host leftover
vs **MATCH**? First leftover?

Authority: `proofs/dummy-pumps-before-region`,
`proofs/host-00501450-timing`,
`proofs/first-region-after-leave`;
dump listings cited there (`004189C2` / `00418AB1` /
`004162B5` / `009A57B0` / `00501450`);
`docs/runtime/FORWARD_TREE.md` §§8–9;
`docs/PARITY.md` first-pump / After `009AC9E0` /
`game+164` / no-save enqueue rows;
`EngineLifecycle.Pump` / `PumpGame` / `PumpGameUpdate`
/ `SeedWorldTick` / `EnqueueAfterDummy` /
`LoadFromFirstRealRegion`;
`EngineLifecycleTests`
(`First_pump_004189C2_is_0040D2A0_then_00B239A0_not_a_region`,
`First_pump_0041674A_is_0_so_00418289_skips_00416E78`,
`Second_pump_004189C2_loops_inner_not_00501450`,
`Pump_004166E2_is_009E1BC0_minus_game_plus96`,
`Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`,
`After_004AEA70_eq_1_00417001_is_00435F70_Present`).

Do **not** start Oakvale.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Dummy pumps leftover vs native? | **No leftover mutation.** Dummy prefix, fade store, first inner skip, later type-1 / Present still on index 0. Live `Pump` / `PumpGame` never open a region. | **MATCH** walk; **PROVEN** skip of `00501450` |
| Host leftover **side effect** on this walk? | **None.** `SeedWorldTick` does not queue type 1. `EnqueueAfterDummy` is not a `Pump` callee. | **PROVEN** skip; **DISPROVEN** leftover mutation |
| Host leftover **artifacts**? | Unused `EnqueueAfterDummy` glue; `GetTickCountIat` alias; `SeedWorldTick` Notes; stale “seed/Present only after `00501450`” / “`00CB8220` UNREAD” comments. | **LEFTOVER** name / glue / theater |
| Grain leftover? | One host `Pump` after first = one inner. Native one `004189C2` = tight `00418AB1` loop. Same gates. | **DIVERGE** grain; **MATCH** skip |
| First leftover? | **`SeedWorldTick` leftover Notes** at end of `EnterGame` (immediately before dummy). First leftover **on** a dummy inner is the `GetTickCountIat` alias. First leftover **that would break MATCH** is wiring `EnqueueAfterDummy` onto `Pump`. | **PROVEN** first leftover note; **DISPROVEN** as work to add |

---

## Verdict

**Dummy pumps MATCH. No leftover side effect. First leftover is leftover theater, not a missing region.**

Native after Loading world `004BBC00 ret 4` / Init Game
suffix: one `004189C2`, dummy `004FC180` index 0, one
`00B239A0(12, 20.0)` store, then `00418AB1` until
`WM_DESTROY`. **0** `E8` / `E9` / imm / vtbl of
`00501450`. First inner does not `00CB8220`. Later
dummy inners do, still on index 0, and yield the
Oakvale **wait** (not activate).

Host after `RequestNewGame`:

```
Pump #0  LeaveFrontend → EnterGame     // SeedWorldTick Notes; no 004189C2
Pump #1  PumpGame first                // dummy + fade + ONE inner
Pump #2+ PumpGame GamePumpFirstDone    // ONE inner each
```

That walk **MATCH**es the recovered tree: no region,
`CurrentRegion=null`, `FirstRealRegionLoadDone=false`,
`QuestPumpRan=false` on the first dummy inner.

Do **not** treat leftover glue as the next implement.
Do **not** pair dummy pumps to Lookout / Oakvale.

---

## 1. MATCH (keep)

| Host | Native | Class |
|---|---|---|
| `EnterGame` / `004BBC00` then first Game `Pump` | dummy `004FC180` index 0, `je` +36 null | **MATCH** |
| `ApplyFirstPumpAviAndFade` | `0040D2A0` / `00B239A0(12, 20.0)` once, then inner | **MATCH** site; fade body **PARTIAL** |
| First inner `0041674A=0` | skip `00416E78` / `0041726D` / `00CB8220` | **MATCH** |
| `EvaluateEngineUpdateGate` | `009A57B0` `GetForegroundWindow==[+148]` | **MATCH** |
| `DisplayTime = FrameDtNow - GamePlus96` | `004166E2` `009E1BC0-[game+96]` | **MATCH** |
| `vtbl+24` only after catchup | `004AEBA0==1` only | **MATCH** |
| Type-1 `TickWorld` / later `00435F70` on dummy | still index 0; 0 `E8` `00501450` | **MATCH** skip |
| `Pump` / `PumpGame` never call `EnqueueAfterDummy` / `LoadFromFirstRealRegion` | next inner, not enqueue | **MATCH** skip |
| `SeedWorldTick` does **not** `009F16F0` | first `game+164` ctor 0 | **MATCH** skip |

Tests lock the skip:
`First_pump_004189C2_*`,
`First_pump_0041674A_*`,
`Second_pump_004189C2_loops_inner_not_00501450`,
`Type1_00CB8220_*`,
`After_004AEA70_eq_1_*`.

---

## 2. DISPROVEN leftovers (do not restore)

These were host guesses. Live `EngineLifecycle` already
does **not** do them.

| Old leftover | Native | Class |
|---|---|---|
| `EnqueueAfterDummy` on second `Pump` | next `00418AB1` inner | **DISPROVEN** |
| Always-run vtbl+24 | only after `004AEAA0` | **DISPROVEN** |
| Sticky `DisplayTime=0` | later inners grow | **DISPROVEN** |
| `009A57B0` = GetTickCount / `GraphicsCreated` | GetForegroundWindow | **DISPROVEN** |
| `SeedWorldTick` **queueing** type 1 | ctor 0; first `0041674A` skips `0041726D` | **DISPROVEN** |
| Seed / Present only after `00501450` | type-1 `006B3FF0` + later `00435F70` on dummy | **DISPROVEN** as a live gate |
| `PresentToHost` gated on `WorldSubmitted` / maps open | Present follows `004AEA70=1` | **DISPROVEN** |
| `00CB8220` body UNREAD | `00CB7C40` + `00CB8170` | **DISPROVEN** unread |

`Pump` Game arm comment already says maps open /
`WorldSubmitted` is **not** the Present gate.

---

## 3. Leftover artifacts that remain

No leftover **mutation** on dummy pumps. These are leftover
**names / glue / notes**.

### 3.1 First leftover — `SeedWorldTick` Notes

`EnterGame` tail (`EngineLifecycle` after `+90592`):

```
SeedWorldTick();   // before Mode=Game; before first 004189C2
```

Body:

```
Note(WorldTickSlot1FnVa, "GamePump", …,
    "0121BA2D [0x13B92C8]=00629270 type 1");
Note(AdvanceGameTicksFn, "InitGame", …,
    "0041726D game+164 ctor 0 009F1750 empty");
```

Native dispatch slot 1 is **static** (`0121BA2D`). First
`0041726D` is **not** this site — first dummy inner
skips it. The Notes are leftover theater: stage
`"GamePump"` during `EnterGame`, as if a type-1 seed ran.

`Note` is `Trace.Add` only. **No** `_tickTypes.Add`.
**PROVEN** leftover comment; **DISPROVEN** leftover queue.

This is the **first leftover** on the host New Game
path that reaches dummy pumps.

### 3.2 First leftover **on** a dummy inner — `GetTickCountIat`

```
GetForegroundWindowIat = 0x01440378;   // MATCH
GetTickCountIat        = 0x01440378;   // leftover alias
```

Dummy inner `009A57B0` uses IAT `0x1440378` =
`USER32!GetForegroundWindow`. Same dword is **not**
GetTickCount. Live gate uses `GetForegroundWindowIat`.
The alias is leftover name only.

### 3.3 First leftover that would **break MATCH** — `EnqueueAfterDummy`

```
public void EnqueueAfterDummy()
{
    if (FirstRealRegionLoadDone || UseNamedStart) return;
    … PlayerRegionName persist …
    LoadFromFirstRealRegion();   // 00501450
}
```

Not called from `Pump` / `PumpGame` / `SilkEngineHost` /
`Program` / `FirstSceneWorld`. One test
(`Persist_PlayerRegionName_*`) calls it **explicitly**
after dummy. Body tests call `LoadFromFirstRealRegion`
**after** dummy, not from the second `Pump`.

The method is leftover **glue**: it treats dummy pumps as
the `00501450` trigger. Wiring it back onto `Pump` is the
first leftover that would **mutate** this MATCH walk.

`LoadFromFirstRealRegion` body itself is **MATCH**
(`00501450` → `00500540(1,0,0)` Lookout). Site is
**DIVERGE** / **UNREAD**. See `host-00501450-timing`.

### 3.4 Stale comments (not first)

| Leftover text | Truth | Class |
|---|---|---|
| Seed / Present only after `00501450` | dummy type-1 + later Present | **LEFTOVER** comment (`dummy-pumps-before-region` table) |
| `00CB8220` body UNREAD | `00CB7C40`+`00CB8170` | **LEFTOVER** comment |
| `2026-08-18-load-profile.md` Pump2 `EnqueueAfterDummy` | live `Pump` does not | **LEFTOVER** stale schedule |

---

## 4. DIVERGE grain (not leftover work)

```
native  004189C2 once → many 00418AB1
host    Pump #1 = dummy+fade+one inner
        Pump #2+ = one inner
```

Host folds native catchup inners into one `dt`
(`Pump(0.1f)` / `Pump(0.25f)`). Same `0041674A` gate.
**DIVERGE** count, **MATCH** skip of `00501450`.

Do **not** “fix” grain by enqueueing a region.

---

## 5. Host sites (read only)

`Pump` Game (`EngineLifecycle` ~2758): `PumpGame` then
Present iff `GamePresentCount` grew. No enqueue.

`PumpGame` (~4366): first pass dummy + fade + one inner;
later passes one inner. No `EnqueueAfterDummy`.

`SeedWorldTick` (~5197): two Notes. Called from
`EnterGame` (~3895) only.

`EnqueueAfterDummy` (~5983): leftover API. Production
never calls it.

---

## What not to implement

- Do not call `00501450` from `Pump` / `PumpGame`.
- Do not restore `EnqueueAfterDummy` as “second frame”.
- Do not queue type 1 from `SeedWorldTick`.
- Do not gate dummy seed / Present on maps open.
- Do not treat `GetTickCountIat` as the `009A57B0` IAT.
- Do not activate Oakvale from dummy `00CB8220`.
