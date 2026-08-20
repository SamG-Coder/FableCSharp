# Factory 0 type-1 tick vs host leftover

Investigation only. No production `src/` edits.

Question: factory 0 type-1 tick vs host. Still leftover?
First leftover field on first Game pump?

Do **not** invent a factory / `Main` / fiber for
`ChapterAndSceneManager` / `NPCDeath`. They have **no**
PE string and **no** `00CD52D0` row.

Do **not** invent a `Started=false` skip of the stub or
of this pump. That flag is not a type-1 gate
(`factory0-construct`).

Do **not** start at `S_QNOVI` / `00DBDE40` /
`Q_NewOakValeIntro`. Those names are not on the no-save
`world+172` walk.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: `proofs/factory0-type1-tick`,
`proofs/factory0-construct`,
`proofs/factory0-stub-vs-fiber`;
also `004B4063-stub-layout`, `00CB8220-first-pump`,
`host-type1-walk-order`, `host-004B2890-leftover`;
ExeIndex `listing-00480000.txt` `004B4490` /
`004B4517`–`004B454E`;
`listing-00c80000.txt` `00CB8220` / `00CB7C40` /
`00CB7950`;
`EngineLifecycle.Pump` / `PumpGame` / `PumpQuests` /
`PumpQuestList` / `TickNamedQuestMain`;
`ScriptRuntime.ActivateQuest` / `QuestInstance`;
`EngineLifecycleTests.Init_quests_004B4260_*` /
`Type1_00CB8220_*`.

`factory0-type1-tick` closed native skip vs host
else-arm **DIVERGE**. `factory0-stub-vs-fiber` closed
construct `CreateFiber` leftover. This note is that
tick leftover **still on the host**, and which field
is first leftover on the first Game pump.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Type-1 factory-0 tick still leftover? | **Yes.** Host `TickNamedQuestMain` still ticks both stubs. Native `cmp [eax+8], 0` / `je 004B4549`. No host `[+8]` gate. | **LEFTOVER** / **DIVERGE** |
| Native first type-1 writes a stub field? | **No.** `004B4490` **reads** `[quest+8]==0` and skips. No `00CB8220`. | **PROVEN** skip |
| Host first type-1 writes a `QuestInstance` field on those names? | **No.** Else-arm is `Note` + `QuestPumpWalked++` only. `Started` / `Fiber` / `Factory` stay construct values. | **PROVEN** no stub-analog store |
| First leftover **field** (object map)? | `QuestInstance.Fiber` on `ChapterAndSceneManager`. Native stub `[+8]=0`. Host still attached a fiber at activate. | **LEFTOVER** |
| First leftover **pump field** (type-1)? | Extra `QuestPumpWalked++` on that same name (walk 1→2). Native `00CB8220` count stays 8. | **LEFTOVER** |
| First leftover **name** on the type-1 walk? | `ChapterAndSceneManager` (`world+172` row 2). `NPCDeath` is the second leftover name (row 5). | **PROVEN** |
| First **Game** `Pump()` leftover from this tick? | **None.** Dummy inner `0041674A=0` → no `004B4490`. `QuestPumpRan=false`. | **PROVEN** skip |
| `Runtime.Update` / `Scheduler.Pump` leftover mutation here? | **No.** Unused on Leave `Pump()`. Fiber `State` / `DtAtPlus8` not written. | **LEFTOVER** pairing; **DISPROVEN** pump mutation |
| `Started=false` leftover skip of this tick? | **No.** Flag unused as a pump predicate. | **DISPROVEN** |

**Still leftover. First leftover field is `Fiber` (`[+8]`).**
First type-1 leftover name is `ChapterAndSceneManager`.
First Game pump does not run this leftover.

---

## Verdict

**Yes. The type-1 factory-0 tick is still leftover.**

Native first type-1 (`004A5A40` → `004A5D88` `004B4490`)
**visits** both 52-byte stubs on `[manager+56]` and
**skips** `00CB8220` because `[quest+8]==0`
(`factory0-type1-tick`). Host `PumpQuestList` still
`TickNamedQuestMain`s every `_activatedQuests` name
except the string `"Gameflow"`. Rows 2 and 5 hit the
else-arm:

```
00CB7950 ChapterAndSceneManager Main
009D8650 ChapterAndSceneManager
```

Native never issues those two `00CB7950`s. Locked
`Type1_00CB8220_*` `QuestPumpWalked==12` still counts
them. **LEFTOVER** / **DIVERGE**.

That leftover is **trace + host counter**, not a new
stub fill. `QuestInstance` on those names is unchanged
by the pump. The leftover **field** that is still wrong
at first Game / first type-1 is the construct fiber
(`factory0-stub-vs-fiber`): host `Fiber != null` vs
native `[+8]=0`. Type-1 then treats that fiber as a
tickable `Main`.

Do **not** implement a skip from `Started=false`.
Do **not** invent a factory so the else-arm becomes
true. Do **not** call `Runtime.Update` to “use” the
fiber.

---

## Timeline (no-save)

```
Host Pump() #1          Leave → EnterGame / 004B4260
  ActivateNamedQuest ×9 + Gameflow
    ChapterAndSceneManager / NPCDeath:
      CreateFiber + Scheduler.Create     // leftover field
      Started=false  Factory=0           // MATCH no-run
      EventPosts++                       // leftover post
Host Pump() #2          first Game pump (dummy)
  0041674A=0 → no 00416E78 / no 004B4490
  QuestPumpRan=false  QuestPumpWalked=0
  factory-0 tick leftover NOT run
Host Pump(0.1f)         first type-1
  PumpQuests → PumpQuestList
    TickNamedQuestMain Q_SunnyvaleMaster     // MATCH
    TickNamedQuestMain ChapterAndSceneManager // FIRST leftover
    …
    TickNamedQuestMain NPCDeath              // second leftover
    TickGameflowMain / Core / Barrow
  QuestPumpWalked==12
```

Native same first type-1: 10 `[QM+56]` visits, **8**
`00CB8220`, **10** `00CB7950`, factory-0 ticks **0**.

---

## 1. Still leftover — host has no `[+8]` gate

`004B4490` (`listing-00480000.txt`):

```
004B4522:
  mov eax, [edi+8]                 // 52-byte slot
  cmp [eax+8], ebx                 // ebx=0
  je  004B4549                     // stub → next node
  mov ecx, [eax+8]
  call 00CB8220
```

Host (`EngineLifecycle.PumpQuestList`):

```
foreach (var name in _activatedQuests)
{
    if (name == "Gameflow")
        continue;
    TickNamedQuestMain(name);
}
```

The skip is the Gameflow **string**, not `[quest+8]`.
`TickNamedQuestMain` for factory 0:

```
Note(00CB8220 name);
Note(00CB7950 name + " Main");     // else-arm
Note(009D8650 name);
QuestPumpWalked++;
```

No read of `quest.Started`, `quest.Fiber`, `quest.Factory`,
or `quest.Run`. **PROVEN** leftover tick.

`V_HeroDolls` / `CS_PlayCutscene` /
`Global_WatchForHeroDeath` also take the else-arm.
Those are **live** factories (`QuestFactoryTable`).
Native **does** `00CB8220` them. Do **not** fold them
into factory 0 (`factory0-type1-tick`).

| Token | Native | Host | Class |
|---|---|---|---|
| `[QM+56]` visit stubs | yes | names on `_activatedQuests` | **MATCH** list |
| `00CB8220` on stubs | no | yes (`00CB8220 name` Note) | **LEFTOVER** |
| `00CB7950` on stubs | no | yes (else-arm) | **LEFTOVER** |
| `QuestPumpWalked` | n/a (8 `00CB8220`) | **12** | **LEFTOVER** |
| `Runtime.Update` on stubs | no `00A44880` | unused | leftover pairing |

Stale host XML on `QuestManagerPumpFn`
(`EngineLifecycle` ~1256): “`00CB8220` type-1 body
UNREAD.” Body is **PROVEN** (`factory0-type1-tick`).
That sentence is leftover **comment**. The leftover
**work** is the else-arm, not an unread native hole.

---

## 2. First leftover field (stub map vs host)

`004B4063` field map (`004B4063-stub-layout`), still
true at first Game pump — native type-1 does not store:

| Stub | Host `QuestInstance` | First Game / first type-1 | Class |
|---|---|---|---|
| `[+0]` id | `Id` | unchanged | **MATCH** |
| `[+4]=0` | `Factory==0` | unchanged | **MATCH** |
| `[+8]=0` | `Fiber != null` | still attached; not a skip | **LEFTOVER** first field |
| `[+12…+24]=0` | no host analog | unread / unused | **UNREAD** |
| `[+36]=1` | `Started==false` | unused as pump skip | **DISPROVEN** as `[+36]` |
| `[+37]=1` | no host analog | — | **UNREAD** |
| `[+48]` name | `Name` | `ChapterAndSceneManager` / `NPCDeath` | **MATCH** |
| no watcher | `Scheduler` fiber + `CreateFiber` | still present | **LEFTOVER** |

`Fiber` is the first leftover field in declaration
order after the MATCH id/name/persist prefix. It is
the `[+8]` analog the type-1 gate would read. Host
never stores a run pointer, so the leftover is
**existence of a fiber**, not a later overwrite.

`StartFactory` miss / `Started=false` still **MATCH**es
“no `004B0310` / no `00CB7900`.” It does **not** MATCH
“no tick.” `Started` is leftover **comment** as a
construct name, not a leftover **store** on this pump
(`factory0-construct`).

---

## 3. First leftover on first Game pump vs first type-1

First **Game** `Pump()` after `EnterGame` is the dummy
inner (`host-type1-walk-order` / `dummy-pumps-before-region`):

| Field | After dummy | Class |
|---|---|---|
| `QuestPumpRan` | `false` | **MATCH** no `004B4490` |
| `QuestPumpWalked` | `0` | **MATCH** no walk |
| `quest.Fiber` | still set | leftover from construct, **not** this pump |
| `quest.Started` | `false` | **MATCH** no-run |
| factory-0 `00CB7950` Notes | none yet | **PROVEN** skip |

First leftover **field written by this leftover tick**
is therefore **not** on the first Game pump. It is on
the first type-1 `Pump(0.1f)`:

1. `TickNamedQuestMain("Q_SunnyvaleMaster")` — **MATCH**
   (`QuestPumpWalked` 0→1).
2. `TickNamedQuestMain("ChapterAndSceneManager")` —
   **first leftover name**. Notes `00CB8220` /
   `00CB7950` / `009D8650`. `QuestPumpWalked` 1→2.
   That increment is the first leftover **pump field**.

`NPCDeath` is the same leftover, later on the same
walk. `EventPosts==10` is leftover from **construct**,
not this pump (`factory0-stub-vs-fiber` /
`00CB8220-first-pump`).

`PumpQuests` also Notes `01375454` and
“`004B3CE0` construct already at `004B4260`” before
the foreach. Those are leftover **comments**
(`host-gate-va-leftover` / `factory0-construct`).
They are not factory-0 field stores. `004B4490` does
not construct and does not read `01375454`.

---

## 4. What is leftover vs what is not

| Host action on first type-1 | Native factory 0 | Class |
|---|---|---|
| `TickNamedQuestMain` else-arm | `[+8]==0` skip | **LEFTOVER** / **DIVERGE** |
| `QuestPumpWalked++` on stubs | no `00CB8220` | **LEFTOVER** |
| `quest.Fiber` still non-null | `[+8]=0` | **LEFTOVER** field |
| `CreateFiber` / `Scheduler.Create` (already) | no `00CB7900` | **LEFTOVER** (construct) |
| `EventPosts==10` (already) | 8 posts | **LEFTOVER** (construct) |
| `Note` `00CB7950` / `009D8650` | no site | leftover **trace** |
| `Started` stays false | `[+4]=[+8]=0` | **MATCH** no-run |
| quest object still exists | 52-byte slot on `+56` | **MATCH** |
| `Runtime.Update` resumes stub fiber | no `00A44880` | leftover unused; **no** mutation |
| Dummy first Game pump ticks stubs | `0041674A=0` skip | **MATCH** skip |

---

## What this is not

| Claim | Class |
|---|---|
| Factory-0 type-1 leftover is gone | **DISPROVEN** (else-arm still runs) |
| Native first type-1 `00CB7950`s the stubs | **DISPROVEN** |
| First leftover name is `NPCDeath` / Gameflow | **DISPROVEN** (`ChapterAndSceneManager`) |
| First leftover field is `Started` | **DISPROVEN** (MATCH no-run; unused skip) |
| First leftover field is `[+36]` | **DISPROVEN** (stub stores 1) |
| First Game dummy pump writes this leftover | **DISPROVEN** (`QuestPumpRan=false`) |
| `QuestPumpWalked==12` is native `00CB8220` | **DISPROVEN** (native 8) |
| `V_HeroDolls` else-arm is factory 0 | **DISPROVEN** |
| `Started=false` should skip `004B4063` / `+56` | **DISPROVEN** — do not invent |
| Second registrar fills those two names | **UNREAD** — do not invent |

---

## Classifications (short)

1. **Factory-0 type-1 tick is still leftover —
   PROVEN LEFTOVER / DIVERGE.** Host
   `TickNamedQuestMain` else-arm. Native
   `[quest+8]==0` skip. Authority
   `factory0-type1-tick` + current
   `PumpQuestList`.
2. **First leftover field — `QuestInstance.Fiber`
   (`[+8]` analog) — PROVEN LEFTOVER.** Still
   set at first Game pump. Type-1 does not
   overwrite it; it ignores the null run.
3. **First leftover name / pump field on first
   type-1 — `ChapterAndSceneManager` /
   extra `QuestPumpWalked++` — PROVEN.**
4. **First Game dummy pump does not run this
   leftover — PROVEN.**
5. **`Started=false` as skip of this tick —
   DISPROVEN.** Do not invent it.
