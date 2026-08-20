# Host `TickGameflowMain` on first `Pump(0.1f)` vs native `00CE75B0` / `00CE7670`

Investigation only. No production `src/` edits.

Do **not** collapse `00CE75B0` onto `00CE7670`.
Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** treat `00CDD450` `0.1f` as a skipped
Gameflow pump.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: `proofs/gameflow-main-first-tick/README.md`,
`proofs/gameflow-oakvale-wait/README.md`,
`proofs/dummy-pumps-before-region/README.md`,
`proofs/fiber-first/README.md`,
`proofs/fiber-yield-first/README.md`,
`proofs/quest-type1-first-walk/README.md`,
`proofs/factory0-type1-tick/README.md`;
`listing-00cc0000.txt` `00CE75B0` / `00CDD450` /
`00CE7640` / `00CE7670`;
`listing-00c80000.txt` `00CB8220` / `00CB7C40` /
`00CB7950`;
ExeIndex `microthread-update-00a44880` /
`microthread-has-work-00a44930` /
`microthread-ctor-00a44740`;
`EngineLifecycle` `Pump` / `PumpGame` /
`EvaluatePlayerCatchup` / `PumpQuestList` /
`TickGameflowMain` / `SeedGameflowStates`;
`EngineLifecycleTests`
(`Gameflow_00CE75B0_is_Main_watcher_not_S_GF`,
`Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`,
`First_pump_0041674A_is_0_so_00418289_skips_00416E78`,
`Pump_004166E2_is_009E1BC0_minus_game_plus96`).

---

## Verdict

**No. Host does not skip a Gameflow pump.
First `Pump(0.1f)` is the first type-1
`00CE7670` wait. That is not `00CE75B0`.
Not a DIVERGE of those two VAs.**

Native first `00CE75B0` is attach-`Main`
only (`00CDD450` / `00CB7E50`). It runs at
`user.ini` `ActivateQuest("Gameflow")`,
**before** `004189C2`. It is not a pump.
`GameflowYieldQuest==null` after construct.
**PROVEN** (`gameflow-main-first-tick`).

Native first `00CE7670` is later, on the
first type-1 `004B4490` walk of the Gameflow
factory (`00CB8220` → `00CB7950` `+41=0` →
`00A44880` → `00CE7640` → `00CE7670`).
State 0 `00893610("Q_NewOakValeIntro")` is
0 → yield. **PROVEN**
(`gameflow-oakvale-wait`).

Host matches that split:

1. `EnterGame` / `ActivateNamedQuest("Gameflow")`
   → `SeedGameflowStates` notes `00CE75B0`.
   Watchers=`Main`. `GameflowYieldQuest==null`.
2. First Game `Pump()` (`dt` unused while
   `!GamePumpFirstDone`) → dummy inner.
   `0041674A=0` → no `00416E78` / no
   `00CB8220`. `QuestPumpRan=false`.
3. First `Pump(0.1f)` → `004166E2*15-0>1`
   → type-1 `PumpQuests` →
   `TickGameflowMain` notes `00CE7670` wait.

Host `TickGameflowMain` is **not** a missed
`00CE75B0`. Collapsing the two VAs is
**DISPROVEN**. Skipping dummy type-1 is
native `0041674A=0`, not a host hole.
**PROVEN**.

`0.1f` is two different numbers. Do not
merge them.

| Token | Site | Class |
|---|---|---|
| `00CDD450` `push 0x3DCCCCCD` | watcher ctor → `00A44740` → `00A445D0` | **PROVEN** create arg |
| Host `Pump(0.1f)` | test `FrameDtNow` so `004166E2*15>1` | **PROVEN** catchup dt |
| First `00A44880` skips `00CE7670` until 0.1 s | `00A44930` is queue-empty, not a timer | **DISPROVEN** |

Grain vs native inner loop is already
**DIVERGE** (`dummy-pumps-before-region`):
one host `dt` folds many `00418AB1`
iterations. Same gate. Extra factory-0
name ticks on that walk are a different
**DIVERGE** (`factory0-type1-tick`).
Neither is a skipped `00CE75B0`.

| Question | Answer | Class |
|---|---|---|
| First native Gameflow body after Leave? | `00CE75B0` attach `"Main"` | **PROVEN** |
| Is that a `004189C2` / type-1 pump? | no; `user.ini` construct | **DISPROVEN** |
| First native `00CE7670`? | first type-1 Gameflow `00CB8220` | **PROVEN** |
| Host `EnterGame` `GameflowYieldQuest`? | `null`; watchers=`Main` | **PROVEN** |
| Host first Game `Pump()` `QuestPumpRan`? | `false` (`0041674A=0`) | **PROVEN** |
| Host first `Pump(0.1f)`? | first type-1; `TickGameflowMain` `00CE7670` wait | **PROVEN** |
| Does host skip `00CE75B0`? | no; `SeedGameflowStates` at construct | **DISPROVEN** |
| Does host run `00CE7670` on the dummy inner? | no | **DISPROVEN** |
| Does `00CDD450` `0.1f` delay first `00CE7670`? | no | **DISPROVEN** |
| Host skip-`Gameflow` in `PumpQuestList` foreach? | then `TickGameflowMain`; not a missed wait | **PROVEN** note |
| VA pairing `TickGameflowMain`=`00CE7670`? | **PROVEN**; not **DIVERGE** | — |
| One host `Pump` vs many native inners? | **DIVERGE** grain, same catchup gate | already proven |

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend
0042F491 Init Game → 004184BD
  user.ini ActivateQuest("Gameflow")
    00CB7900 vtbl+12 00CE6CF0
    jmp vtbl+4 00CE75B0                 // NOT a pump
      00CDD450 push 0.1f / 64 / 1
      00A44740 fiber; +40=+41=0
      00CB7E50 attach "Main"
      ret                               // GameflowYieldQuest == null

Host Pump()  LeaveFrontend → EnterGame
  SeedGameflowStates notes 00CE75B0
  watchers == { Main }
  GameflowYieldQuest == null

004189C2 first pumps
Host Pump()  dummy + fade + first inner
  GamePumpFirstDone was 0 → FrameDtNow stays 0
  0041674A: 0*15-0 <= 1 → al=0
  skip 00416E78 / 004A5A40 / 00CB8220
  QuestPumpRan == false                 // no TickGameflowMain

  later inners: 004166E2*15-slot > 1
Host Pump(0.1f)
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

## 1. Two VAs — construct is not a tick

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

It does **not** set `GameflowYieldQuest`.
`Gameflow_00CE75B0_*` asserts `null` and
watchers=`Main` after the `EnterGame`
`Pump()`. **PROVEN**.

`TickGameflowMain` notes `00CE7670`,
attaches Core / Barrow, notes
`00893610` miss, sets
`GameflowYieldQuest=GameflowWaitQuest`.
That is the type-1 wait, not construct.
**PROVEN**.

Treating `TickGameflowMain` as the first
`00CE75B0` body is **LEFTOVER** (same
class as `GameflowWaitQuest` at
construct). The host already split the
two sites.

---

## 2. First `Pump(0.1f)` is first type-1, not dummy

`EngineLifecycle.Pump`:

```
if Stage==Game:
  if GamePumpFirstDone: FrameDtNow += dt
  PumpGame()
```

`PumpGame` first call: dummy
`004FC180` index 0, fade `00B239A0`,
one inner. `EvaluatePlayerCatchup`
(`0041674A`): `[game+9]=1`,
`004166E2=009E1BC0-[game+96]`.
First inner `FrameDtNow==GamePlus96==0`
→ `0*15-0<=1` → skip vtbl+24 /
`004A5A40`. **PROVEN**
(`First_pump_0041674A_*`).

Next `Pump(0.1f)`: `GamePumpFirstDone`
so `FrameDtNow=0.1`. `0.1*15>1` →
`AppendPlayerCatchupTick` type 1 →
`TickWorld` → `PumpQuests` →
`TickGameflowMain`. **PROVEN**
(`Pump_004166E2_*`,
`Type1_00CB8220_*`).

Native first dummy inner also skips
type-1. Later inners hit the same
`004166E2*15-slot>1` gate, still on
index 0. Host folds those inners into
one `dt`. **DIVERGE** grain; **PROVEN**
same gate (`dummy-pumps-before-region`).

Client `Program.cs` passes real frame
`dt`, not a forced `0.1f`. Tests use
`0.1f` / `0.25f` as a catchup shortcut.
That is not a skipped Gameflow pump.

---

## 3. `00CDD450` `0.1f` is not a deferred first tick

`00CDD450`:

```
push 0x3DCCCCCD        // 0.1f
push 64
push 1
call 00A44740
```

`00A44740` forwards flag / stack / 0.1f
to `00A445D0`, then `009D8710(00A446A0)`
into `[this+16]`. Create, not pump.
**PROVEN** (`fiber-first`).

First type-1 `00CB7950`: first-seen
`+40=0`, `00F35A00=1`, `+41=0` →
`vtbl+4` `00A44880`. Host skip-parked
`00A44880` is **DISPROVEN**
(`fiber-yield-first`, `Type1_resume_*`).

`00A44880` first-seen `[0x13D2838]==0`:
enqueue on `0x13D2828`, `00A44930`, then
`009E1BC0` → `[this+8]`, `00A44660`.
`00A44930` is six insns:

```
eax = [ecx] - [ecx+4]
neg / sbb / inc          // 1 if empty, 0 if work
```

Empty → `jne` epilogue. After enqueue,
queue is not empty → dequeue → resume
immediately → `00A446A0` `[vtbl+16]`
then `[vtbl+8]` `00CE7670`. **PROVEN**
dump; **DISPROVEN** as a 0.1 s gate on
the first Gameflow tick.

`00A44880` `fstp [this+8]` stores
**frame** dt (`009E1BC0`), not the
ctor 0.1f, on that pump.

---

## 4. `PumpQuestList` skip-`Gameflow` is not a skipped wait

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
same call still `TickGameflowMain`.
Native walks Gameflow last on
`[QM+56]` as its own `00CB8220`. Host
order (WLD names, then Main / Core /
Barrow) matches tail-insert. **PROVEN**
note.

That skip does **not** omit `00CE7670`.
What it fails to omit is factory-0
`ChapterAndSceneManager` / `NPCDeath`
(`TickNamedQuestMain` else-arm). Native
`[slot+8]==0` skips those `00CB8220`s.
`QuestPumpWalked==12` vs native 8.
**DIVERGE** extra ticks
(`factory0-type1-tick`), not a hole
before `00CE7670`.

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

---

## Classifications (short)

1. **`00CE75B0` vs `00CE7670` — PROVEN
   two sites.** Attach at construct;
   wait on first type-1. Host
   `SeedGameflowStates` then
   `TickGameflowMain`.
2. **Host skip of a Gameflow pump —
   DISPROVEN.** Dummy `Pump()` skips
   type-1 with native `0041674A=0`.
   First `Pump(0.1f)` **is** that
   type-1.
3. **`TickGameflowMain`=`00CE7670` wait
   — PROVEN pairing. Not DIVERGE.**
4. **Ctor `0.1f` delays first
   `00CE7670` — DISPROVEN.**
   `00A44930` is queue-empty.
5. **Host `Pump` grain vs native inner
   loop — DIVERGE** already proven;
   same catchup gate. Factory-0 extra
   named ticks — separate **DIVERGE**.
