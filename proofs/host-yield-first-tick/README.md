# Host `GameflowYieldQuest` on first type-1 `00CE7670` vs native first `00CE75B0`

Investigation only. No production `src/` edits.

Question: host sets `GameflowYieldQuest` on first
type-1 `00CE7670`. Native first `00CE75B0` is
attach-only. Count host pumps vs native.
**DIVERGE?**

Do **not** collapse `00CE75B0` onto `00CE7670`.
Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** treat `00CDD450` `0.1f` as a skipped
Gameflow pump.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: `proofs/gameflow-main-first-tick` (first
`00CE75B0` attach-`Main` only);
`EngineLifecycle.TickGameflowMain` (first type-1
`00CE7670` wait write);
`proofs/gameflow-oakvale-wait`;
`proofs/host-gameflow-tick-diverge`;
`proofs/dummy-pumps-before-region`;
`proofs/factory0-type1-tick`;
`EngineLifecycle` `Pump` / `PumpGame` /
`EvaluatePlayerCatchup` / `PumpQuestList` /
`SeedGameflowStates`;
`EngineLifecycleTests`
(`Gameflow_00CE75B0_is_Main_watcher_not_S_GF`,
`Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`,
`Type1_resume_00CB8220_is_00A44880_then_00893610_yield`,
`First_pump_0041674A_is_0_so_00418289_skips_00416E78`).

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Host set `GameflowYieldQuest` on first type-1 `00CE7670`? | **Yes.** `TickGameflowMain` writes `GameflowWaitQuest` (`Q_NewOakValeIntro`). | **PROVEN** |
| Native first `00CE75B0` attach-only? | **Yes.** `user.ini` construct: `"Main"` / `00CDD450` / `00CB7E50` / `ret`. No `00893610`. No yield. | **PROVEN** (`gameflow-main-first-tick`) |
| Host write that name at `00CE75B0`? | **No.** `SeedGameflowStates` attaches `Main`. `GameflowYieldQuest==null`. | **PROVEN** |
| Native first `00CE7670` also wait-write? | **Yes.** Same first type-1 `00CB8220` walk. Miss → `006E7410`. | **PROVEN** (`gameflow-oakvale-wait`) |
| Host pumps after `RequestNewGame` to that write? | **3** `Pump` / **2** `GamePumpFrames` / **1** type-1. | **PROVEN** |
| Native pumps to first `00CE7670`? | **0** construct pumps + **1** `004189C2` + **1** dummy inner + **N** catchup inners + **1** type-1 inner. **N** is not a closed integer. | **PROVEN** gate; **UNREAD** as a finite N |
| Yield-site **DIVERGE**? | **No.** Host write is `00CE7670`, not `00CE75B0`. | **MATCH** |
| Pump-grain **DIVERGE**? | **Yes.** One host `dt` folds many `00418AB1` inners. Same `0041674A` gate. | **DIVERGE** (already `dummy-pumps-before-region`) |

---

## Verdict

**No. Setting `GameflowYieldQuest` on first type-1
is not a DIVERGE from native first `00CE75B0`.**

Those are two sites. Native first `00CE75B0` is
attach-`Main` only and is not a pump.
`GameflowYieldQuest` stays null. Native first
`00CE7670` is later, on the first type-1 walk,
and is the wait. Host splits the same way:

1. `EnterGame` / `ActivateNamedQuest("Gameflow")`
   → `SeedGameflowStates` notes `00CE75B0`.
   Watchers=`Main`. `GameflowYieldQuest==null`.
2. First Game `Pump()` → dummy inner.
   `0041674A=0` → no `00CB8220`.
   `QuestPumpRan=false`. Yield still null.
3. First `Pump(0.1f)` → `004166E2*15-0>1`
   → type-1 → `TickGameflowMain` notes
   `00CE7670` and sets the name.

`TickGameflowMain` is **not** a late
`00CE75B0`. Collapsing the VAs is
**DISPROVEN**. Skipping dummy type-1 is
native `0041674A=0`, not a host hole.
**PROVEN**.

Pump **count** still **DIVERGE**s in grain:
host uses three `Pump` calls (one of them
`0.1f`) where native uses one `004189C2`
and an unbounded `00418AB1` loop until the
same catchup gate. Extra factory-0 name
ticks on that walk are a different
**DIVERGE** (`factory0-type1-tick`).
Neither is a yield write at construct.

---

## Count — host vs native

After `RequestNewGame` (no-save). Host
`Pump` from `Type1_00CB8220_*`. Native
`004189C2` from `dummy-pumps-before-region`.

| Step | Host | Native | `00CE75B0` | `00CE7670` | `GameflowYieldQuest` |
|--:|---|---|---|---|---|
| 0 | `RequestNewGame` → `LeaveFrontend` | `0042F2A2` | — | — | (unset) |
| 1 | `Pump()` → `EnterGame` / `user.ini` | Init Game suffix; **not** `004189C2` | **1** attach | 0 | **null** |
| 2 | `Pump()` → first `PumpGame` | first `004189C2`: dummy + fade + **1** inner | 0 | 0 | **null** |
| — | (folded into step 3) | later `00418AB1` inners until `004166E2*15>1` | 0 | 0 | **null** |
| 3 | `Pump(0.1f)` → first type-1 | first type-1 inner: `004A5A40` → `00CB8220` | 0 | **1** wait | **`Q_NewOakValeIntro`** |

Locked counts:

| Token | Host | Native |
|---|---|---|
| `Pump()` after `RequestNewGame` to yield write | **3** | n/a (different grain) |
| `GamePumpFrames` (`PumpGame` entries) to yield write | **2** | **1** `004189C2` entry |
| `004162B5` inners before first type-1 | **1** (dummy, skip) | **1** + **N** |
| type-1 pumps before first `00CE7670` | **0** | **0** |
| `00CE75B0` bodies | **1** (construct note) | **1** (construct) |
| `00CE7670` bodies to first yield write | **1** | **1** |
| `GameflowYieldQuest` writes | **1** (`TickGameflowMain`) | n/a (C++ stack wait) |
| `0041674A` on dummy inner | `0*15-0 <= 1` skip | same |
| `0041674A` on first type-1 | `0.1*15-0 = 1.5 > 1` | `009E1BC0-[game+96]*15-slot > 1` |
| `00CB8220` on first type-1 | host walk **12** (`QuestPumpWalked`) | **8** live factories |

`N` = however many `00418AB1` iterations it
takes for QPC `004166E2*15-slot>1`. Gate
threshold is `dt > 1/15`. Host tests pass
`0.1f` so one call hits. Client `Program.cs`
uses real frame `dt`. **DIVERGE** grain;
**PROVEN** same gate.

`QuestPumpWalked==12` vs native **8**
`00CB8220` is factory-0
`ChapterAndSceneManager` / `NPCDeath`
(`TickNamedQuestMain` else-arm). Not an
extra `00CE7670`. **DIVERGE** walk; not a
yield-site hole.

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend
0042F491 Init Game → 004184BD
  user.ini ActivateQuest("Gameflow")
    00CB7900 vtbl+12 00CE6CF0
    jmp vtbl+4 00CE75B0                 // FIRST 00CE75B0; NOT a pump
      00CDD450 push 0.1f / 64 / 1
      00A44740 fiber; +40=+41=0
      00CB7E50 attach "Main"
      ret                               // GameflowYieldQuest == null

Host Pump() #1  LeaveFrontend → EnterGame
  SeedGameflowStates notes 00CE75B0
  watchers == { Main }
  GameflowYieldQuest == null
  GamePumpFrames == 0                   // PumpGame not entered

004189C2 first pumps
Host Pump() #2  dummy + fade + first inner
  GamePumpFrames == 1
  GamePumpFirstDone was 0 → FrameDtNow stays 0
  0041674A: 0*15-0 <= 1 → al=0
  skip 00416E78 / 004A5A40 / 00CB8220
  QuestPumpRan == false                 // no TickGameflowMain
  GameflowYieldQuest == null

  native later inners: 004166E2*15-slot > 1
Host Pump(0.1f) #3
  GamePumpFrames == 2
  FrameDtNow += 0.1; DisplayTime=0.1
  0.1*15-0 = 1.5 > 1 → type-1
  004A5A40 → 004B4490 → 00CB8220
    first factory: Sunnyvale 00CDD360
    …
    last factory: Gameflow
      00CB7950 +41=0 → 00A44880
      00A446A0 vtbl+16 00CE7640
      00CDD440 jmp [vtbl+8]
      00CE7670                          // FIRST 00CE7670
        attach Core / Barrow
        00893610 "Q_NewOakValeIntro" → 0
        006E7410 / 009D8650
  Host TickGameflowMain notes that wait
  GameflowYieldQuest = GameflowWaitQuest
```

`00DABAC0` / `00DBDE40` / `S_QNOVI` are
**not** on this list. **PROVEN**.

Construct vs first type-1 is locked by
`Gameflow_00CE75B0_*` then
`Type1_00CB8220_*` (`Pump()` /
`Pump()` / `Pump(0.1f)`).

---

## 1. Two VAs — attach is not the yield write

`00CE75B0` ends `00CE763A ret` before
`00CE7640` / `00CE7670`. First x86
`sub esp, 8`. Body: alloc 60, `"Main"`,
`00CDD450`, `00CB7E50`, return. No
`00893610`. No `006E7410`. **PROVEN**.

Host `SeedGameflowStates` (from
`ActivateNamedQuest` during `EnterGame`):

```
Note GameflowMain  "00CE75B0 Main 00CDD450 / 00CB7E50"
Note GameflowWatcherCtor  "00CDD450 Main 0.1f"
AttachGameflowWatcher(WatcherMain)
```

It does **not** assign `GameflowYieldQuest`.
`Gameflow_00CE75B0_*` asserts `null` and
watchers=`Main` after `Pump()` #1.
**PROVEN**.

`TickGameflowMain` (`EngineLifecycle`):

```
Note GameflowTickFn "00CE7670"
… attach Core / Barrow …
Note QuestIsActiveFn "00893610 Q_NewOakValeIntro 0"
Note FiberYieldFn    "009D8650 wait Q_NewOakValeIntro"
GameflowYieldQuest = GameflowWaitQuest
```

That is the type-1 wait, not construct.
`Type1_00CB8220_*` asserts the name after
`Pump(0.1f)` only. **PROVEN**.

Treating the host write as a first
`00CE75B0` body is **LEFTOVER**. The host
already split the two sites.

---

## 2. Host `Pump` grain vs native inners

`EngineLifecycle.Pump`:

```
if Stage==LeaveFrontend: EnterGame(); return
if Stage==Game:
  if GamePumpFirstDone: FrameDtNow += dt
  PumpGame()
```

`PumpGame` first call: dummy `004FC180`
index 0, fade `00B239A0`, one inner.
`EvaluatePlayerCatchup` (`0041674A`):
`[game+9]=1`,
`004166E2=009E1BC0-[game+96]`.
First inner `FrameDtNow==GamePlus96==0`
→ `0*15-0<=1` → skip vtbl+24 /
`004A5A40`. **PROVEN**.

Next `Pump(0.1f)`: `GamePumpFirstDone`
so `FrameDtNow=0.1`. `0.1*15>1` →
`AppendPlayerCatchupTick` type 1 →
`TickWorld` → `PumpQuests` →
`TickGameflowMain`. **PROVEN**.

Native first dummy inner also skips
type-1. Later inners hit the same
`004166E2*15-slot>1` gate, still on
index 0, still inside **one**
`004189C2`. Host folds those inners
into one `dt`. **DIVERGE** grain;
**PROVEN** same gate.

That grain is **not** “host waited one
fewer Gameflow tick.” Dummy type-1
count is **0** on both sides. First
`00CE7670` count to the write is **1**
on both sides.

---

## 3. `PumpQuestList` skip-`Gameflow` is not a skipped wait

```
foreach name in _activatedQuests:
  if name == "Gameflow": continue
  TickNamedQuestMain(name)
if watchers has Main && GameflowYieldQuest is null:
  TickGameflowMain()
  TickCoreReminder()
  TickBarrowGuards()
```

The `continue` drops the Gameflow
**string** from the named-WLD arm. The
same call still `TickGameflowMain` on
the first type-1. After the write,
later type-1 takes `ResumeGameflowWait`
(`Type1_resume_*`). **PROVEN**.

Native walks Gameflow last on `[QM+56]`
as its own `00CB8220`. Host order
(WLD names, then Main / Core / Barrow)
matches tail-insert. **PROVEN** note.

---

## What not to implement

- Do not run `TickGameflowMain` at
  `00CE75B0` / `EnterGame`.
- Do not skip `TickGameflowMain` on the
  first type-1 because ctor `0.1f` “has
  not elapsed”.
- Do not `ActivateQuest("Q_NewOakValeIntro")`
  to “finish” that wait.
- Do not treat test `Pump(0.1f)` as the
  fiber interval.
- Do not “fix” the three-`Pump` host
  sequence by inventing extra
  `00CE7670` calls to match native
  inner count.

---

## Classifications (short)

1. **Host `GameflowYieldQuest` write —
   PROVEN first type-1 `00CE7670`.**
   Not first `00CE75B0`. **MATCH** vs
   native wait site.
2. **Native first `00CE75B0` —
   PROVEN attach-only.** Host
   `SeedGameflowStates` keeps yield
   null. **MATCH**.
3. **Yield-site DIVERGE — DISPROVEN.**
4. **Host pumps to the write — PROVEN
   3 `Pump` / 2 `GamePumpFrames` / 1
   type-1.** Native is 1 `004189C2`
   + unbounded inners + 1 type-1.
   **DIVERGE** grain; same
   `0041674A` gate.
5. **Factory-0 extra named ticks —
   DIVERGE** already proven; not an
   extra `00CE7670`.
