# First `00501450` after Leave — host `LoadFromFirstRealRegion` leftover vs dummy pumps

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `StartOakVale` / `00DBDE40` /
`Q_NewOakValeIntro`. First real region on this walk is
**LookoutPoint** (native index 1). Dummy pumps never open it.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: first `00501450` after no-save Leave. When does
host `LoadFromFirstRealRegion` run versus native dummy
`004189C2` pumps? When is that call leftover?

Authority: `proofs/dummy-pumps-before-region`,
`proofs/first-region-after-leave`;
`docs/runtime/FORWARD_TREE.md` §§8–9;
`docs/PARITY.md` After `009AC9E0` / no-save enqueue rows;
`EngineLifecycle.Pump` / `PumpGame` /
`LoadFromFirstRealRegion` / `EnqueueAfterDummy`;
`EngineLifecycleTests`
(`First_pump_004189C2_is_0040D2A0_then_00B239A0_not_a_region`,
`Second_pump_004189C2_loops_inner_not_00501450`,
`Second_pump_00501450_is_004FEEC0_then_00500540_1_0_0`,
`Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`,
`After_004AEA70_eq_1_00417001_is_00435F70_Present`).

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| First `00501450` **during** dummy pumps? | **No.** Dummy prefix, fade install, first inner, later dummy inners, first type-1, and first Present all skip it. | **PROVEN** skip |
| When after Leave, native? | After Init Game + dummy `004189C2` (unbounded inners while `[game+8]==0`). Body recovered; **who** `E8`s it is **0 hits**. | **PROVEN** after dummy; **UNREAD** caller |
| Closed N dummy inners then `00501450`? | **No integer.** Loop exit is `WM_DESTROY` only. Not a `004189C2` callee. | **UNREAD** as N; **DISPROVEN** as second inner |
| Host `Pump` / `PumpGame` when? | **Never.** Neither calls `LoadFromFirstRealRegion` / `EnqueueAfterDummy`. `FirstRealRegionLoadDone` stays false through dummy pumps. | **PROVEN** skip; **MATCH** vs dummy |
| Host leftover **when**? | Pairing `EnqueueAfterDummy` / `LoadFromFirstRealRegion` to Leave, first Game `Pump`, **second** `Pump`, or any dummy type-1 / Present. | **DISPROVEN** site; leftover glue |
| Host MATCH **when**? | Explicit `LoadFromFirstRealRegion` **after** dummy pumps (tests / `_loadprobe`). Body `00501450` → `00500540(1,0,0)` Lookout. | **DIVERGE** site; **MATCH** body |

---

## Verdict

**Not on dummy pumps. Not on second host `Pump`.**

After Leave (`0042F2A2`) native does Init Game, then one
`004189C2` with dummy index 0, one fade store, then a tight
`00418AB1` inner loop. **0** `E8` / `E9` / imm / vtbl of
`00501450` on that tree. First recovered **body** is later
and not Oakvale.

Host leftover is the **when**, not the Lookout loop:

- Old pairing: second `Pump` → `EnqueueAfterDummy` →
  `LoadFromFirstRealRegion` (stale `docs/status/investigations/2026-08-18-load-profile.md`).
- Native that second `Pump` is the **next dummy inner**.
- Live `Pump` / `PumpGame` already do **not** call it.

`EnqueueAfterDummy` remains leftover **glue**: it exists to
fire `00501450` (or persist `PlayerRegionName`) as if dummy
pumps were the trigger. Production never calls it.

Do **not** re-hook `00501450` onto `Pump`. Do **not** invent
Oakvale as this first open.

---

## 1. Native when after Leave

```
0042F2A2  Leave frontend
0042F491  Init Game 00418DCA → 004184BD
  00416953  Loading world / 004BBC00 ret 4
  user.ini ActivateQuest("Gameflow")     // watcher; 0 E8 00501450
004189C2  once
  dummy 004FC180 index 0                 // +36 null; CurrentRegion=null
  fade  00B239A0(12, 20.0) once
  loop 00418AB1 until WM_DESTROY
    004162B5 / 009A57B0 / 004166E2
    later type-1 004B4490 / 00CB8220     // still dummy; yield Oakvale wait
    later 00435F70 Present               // still dummy
    009AC9E0
    [game+8]==0 → again                  // not 00501450
later 00501450                           // E8 caller UNREAD
  004FEEC0(0,0)
  for i=1..141  00500540(i,0,0)          // i=1 LookoutPoint
```

| Site after Leave | `00501450`? | Class |
|---|---|---|
| Leave / Init Game suffix / Gameflow | no | **PROVEN** (`first-region-after-leave` §1) |
| Dummy prefix `004FC180` index 0 | no | **PROVEN** |
| Fade `00B239A0` | no | **PROVEN** |
| First inner (`004166E2=0`) | no | **PROVEN** |
| Later dummy type-1 / `00CB8220` | no | **PROVEN** |
| First `00435F70` Present | no | **PROVEN** |
| Second `00418AB1` inner | no | **PROVEN** |
| `WM_DESTROY` leave `004175E5` | no | **PROVEN** |
| Recovered `00501450` body | after dummy; caller missing | **PROVEN** body; **UNREAD** site |

`dummy-pumps-before-region`: count of inners *before* first
`00501450` is **not a closed integer** on this tree.

---

## 2. Host when (`Pump` after `RequestNewGame`)

```
Pump #0  Stage LeaveFrontend → EnterGame     // 004BBC00; no 004189C2
Pump #1  PumpGame first                      // dummy + fade + ONE inner
Pump #2+ PumpGame GamePumpFirstDone          // ONE inner each
```

`Pump` (`Stage==Game`) only `PumpGame` then maybe Present.
`PumpGame` dummy arm: `ActivateCurrentRegion` (index 0
return), fade, `GamePumpFirstDone=true`, one inner.
Later arms: one inner. **No** `EnqueueAfterDummy`. **No**
`LoadFromFirstRealRegion`.

| Host call | Native analogue | `00501450`? | Class |
|---|---|---|---|
| `Pump()` after New Game | EnterGame | no | **MATCH** skip |
| next `Pump()` | dummy + fade + first inner | no | **MATCH** skip |
| next `Pump()` | next `00418AB1` inner | no | **MATCH** skip; **DISPROVEN** leftover enqueue |
| `Pump(0.1f)` / `Pump(0.25f)` | catchup type-1 | no | **MATCH** skip |
| later Present | `00435F70` | no | **MATCH** skip |
| explicit `LoadFromFirstRealRegion()` | unread `00501450` site | yes (stand-in) | **DIVERGE** when; **MATCH** body |
| `EnqueueAfterDummy()` from `Pump` | — | would be yes | **LEFTOVER** (not wired) |

`SilkEngineHost` / `Program` / `FirstSceneWorld` do not call
either method. `_loadprobe` `Breakdown` does
`EnterGame` → `Pump1` → **then** `LoadFromFirstRealRegion`
(after dummy, not instead of it).

`FirstRealRegionLoadDone` is only set inside
`LoadFromFirstRealRegion` / `EnqueueAfterDummy`. Dummy
pumps leave it **false**. **PROVEN**
(`Second_pump_004189C2_loops_inner_not_00501450`,
`First_pump_004189C2_*`,
`After_004AEA70_eq_1_*`).

---

## 3. Leftover vs MATCH — the “when”

### Leftover (do not implement / do not restore)

| When host would call `00501450` | Why leftover |
|---|---|
| Leave / `EnterGame` | Init Game has 0 `E8`. WLD parse is not `006C2170`. |
| First Game `Pump` (dummy prefix) | `004FC180` index 0; `je` dummy; no region job. |
| **Second** `Pump` / `EnqueueAfterDummy` | Next inner `00418AB1`. Test name `Second_pump_00501450_*` is a **body** test; it calls `LoadFromFirstRealRegion` **after** dummy, it does not assert `Pump` did. |
| Dummy type-1 / `00CB8220` | Yield on inactive Oakvale wait. Still index 0. |
| First Present / `WorldSubmitted` gate | Native Present on dummy. Seed-only-after-`00501450` is leftover (`PARITY.md`). |
| `EnqueueAfterDummy` as a `Pump` callee | Glue that treats dummy pumps as the trigger. **LEFTOVER API.** Persist arm is `00487C20`, not no-save. |

Stale note: `2026-08-18-load-profile.md` “Pump2:
`EnqueueAfterDummy` → `LoadFromFirstRealRegion`”. That
**when** is **DISPROVEN**. Keep the ms as a cost of the
**body** if invoked, not as a dummy-pump schedule.

### MATCH (keep)

| When | What |
|---|---|
| After dummy pumps, **explicit** | `LoadFromFirstRealRegion` notes `00501450`, `004FEEC0(0,0)`, `00500540(1,0,0)` Lookout, loop through 141, restore `(0,0,1)`. |
| During dummy pumps, **no call** | Live `Pump` / `PumpGame`. |

Grain **DIVERGE**: one native `004189C2` = many inners; one
host `Pump` after first = one inner. Same skip of
`00501450`. Do not “fix” grain by enqueueing Lookout.

---

## 4. Tests (lock the when)

| Test | After Leave | `LoadFromFirstRealRegion` |
|---|---|---|
| `LoadWorld_00416953_*` / Gameflow / Init suffix | `Pump()` EnterGame | absent; `FirstRealRegionLoadDone=false` |
| `First_pump_004189C2_*` | + dummy `Pump()` | absent |
| `Second_pump_004189C2_loops_inner_not_00501450` | + third `Pump()` | absent |
| `Type1_00CB8220_*` / `After_004AEA70_eq_1_*` | catchup / Present | absent |
| `Second_pump_00501450_*` | dummy `Pump()` **then explicit** | present; enqueue **after** dummy Note |
| `Loading_objects_00521AE0_*` / hero `0051FD80` | two `Pump()` **then explicit** | Lookout TNG / `GuildArrivalHSP` |
| `Persist_PlayerRegionName_*` | dummy then `EnqueueAfterDummy()` | **no** `00501450` (named persist 4) |

`Second_pump_00501450_*` asserts
`enqueue > dummy` and
“`00501450` body after dummy; not a `004189C2` `E8`”.
That is the MATCH when. The test **name** still says
second pump; do not read it as `Pump` calling `00501450`.

---

## 5. Not these

| Candidate “first `00501450` when” | Class |
|---|---|
| Immediately after Leave | **DISPROVEN** |
| After `0049F180` / `004B4260` / Gameflow | **DISPROVEN** |
| Dummy index 0 / fade / first inner | **DISPROVEN** |
| Second host `Pump` | **DISPROVEN** leftover site |
| First type-1 / first Present | **DISPROVEN** |
| `00DBDE40` / StartOakVale / kid spawn | **DISPROVEN** as this first |
| Live `Pump` auto-load | **DISPROVEN**; would be leftover |

---

## Classifications (short)

1. **First `00501450` after Leave is not a dummy pump. PROVEN.**
2. **Native when: after dummy `004189C2`; E8 caller UNREAD. PROVEN / UNREAD.**
3. **Host leftover when: `LoadFromFirstRealRegion` / `EnqueueAfterDummy` on Leave, first Game `Pump`, second `Pump`, or dummy type-1/Present. DISPROVEN site.**
4. **Live `Pump` / `PumpGame` never call it. PROVEN MATCH vs dummy pumps.**
5. **MATCH when: explicit call after dummy. DIVERGE site; MATCH Lookout body. Not Oakvale.**

---

## Open

| Item | Class |
|---|---|
| Who transfers control to `00501450` (computed ptr / unread vtbl / never) | **UNREAD** |
| Finite native inner count before that transfer | **UNREAD** (not this tree) |
| Drop unused `EnqueueAfterDummy` | leftover API; not a dummy-pump schedule |
