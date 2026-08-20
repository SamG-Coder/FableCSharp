# First type-1 `00CB8220` vs host `PumpQuestList`

Investigation only. No production `src/` or `tests/` edits.

Question: first type-1 `00CB8220` vs host first
Game pump. **MATCH** or **LEFTOVER**? First
leftover field?

Do **not** start Oakvale (`Q_NewOakValeIntro` /
`00DBDE40` / `S_QNOVI`). That name is a wait
on the last factory, not an activate.

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**DIVERGE** / **MATCH**.

Authority: `proofs/00CB8220-first-pump` (native
counts 8/8 posts or 8/10 watchers; host 12/10
leftover); dump `listing-00c80000.txt`
`00CB8220` / `00CB7C40` / `00CB7950` /
`00CB8170`; `listing-00480000.txt` `004B4490`
`004B453E`; `EngineLifecycle.TickWorld` /
`PumpQuests` / `PumpQuestList` first Game
`Pump(0.1f)`; siblings `host-type1-walk-order`,
`factory0-type1-tick`;
`EngineLifecycleTests.Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| First native `00CB8220` after Leave? | first type-1 `004A5A40` → `004B4490` → `004B453E`. Dummy inner skips. | **PROVEN** |
| Is that host first Game `Pump()`? | **No.** First Game `Pump()` is dummy (`QuestPumpRan=false`). First `00CB8220` is `Pump(0.1f)`. | **PROVEN** |
| Full host walk **MATCH** native `00CB8220`? | **No.** Head/tail names yes; trampoline + factory-0 ticks + count no. | **LEFTOVER** / **DIVERGE** |
| First leftover **field**? | **`QuestPumpWalked`.** Locked `12`. Native is **8** `00CB8220` (or **10** `00CB7950`). | **PROVEN leftover** |
| Next leftover count on the same pump? | `EventPosts` / `EventPumpWalked` `10` vs native **8** `00687540(55,50)`. Construct, not the trampoline. | **LEFTOVER** (already `00CB8220-first-pump`) |

---

## Verdict

**LEFTOVER. First leftover field is
`QuestPumpWalked`.**

Native first `00CB8220` is not a host
property. It is `00CB7C40` then
`jmp 00CB8170` on **one** live factory
(`[quest+8]`). Eight of those on the
first type-1. Host `PumpQuestList` is
one trampoline note plus a name foreach.
The first public field that stores that
walk is `QuestPumpWalked`, and the first
type-1 test locks it to **12**. That
number is not a dump count.

Head/tail still **MATCH** (Sunnyvale
first, Gameflow last, Oakvale wait only).
Treating the host pump as native
`00CB8220` is **DISPROVEN**.

Do **not** grow work to make
`QuestPumpWalked==8` without changing
the factory-0 else-arm. Do **not**
activate Oakvale from this leftover.

---

## Timeline (no-save New Game)

```
004189C2 first pumps
  dummy inner 0041674A=0 → no 004B4490
Host Pump() #2  QuestPumpRan=false

first type-1 004A5A40
  004B4490 [esi+56]
    8 × 00CB8220([quest+8])          // FIRST 00CB8220
      00CB7C40 [run+4] → 00CB7950
        FIRST STORE [run+44]=watcher
        +40=0 00F35A00 +41=0 → vtbl+4
        keep → [run+44]=0
      jmp 00CB8170 [run+8] empty
    2 × factory-0 [+8]==0 SKIP
  006874B0 8 posts, fire 0

Host Pump(0.1f)
  PumpQuests
    QuestVtbl24Calls=0
    PumpQuestList
      QuestPumpWalked=0
      note 00CB8220 00CB7C40 then 00CB8170
      note 00CB7C40 count=_gameflowWatchers (1)
      TickNamedQuestMain ×9 (incl. two stubs)
      TickGameflowMain / Core / Barrow
      QuestPumpWalked==12                 // FIRST LEFTOVER FIELD
    QuestPumpRan=true
  PumpEvents EventPumpWalked=EventPosts==10
```

---

## 1. Native `00CB8220` writes no own fields

`listing-00c80000.txt`:

```
00CB8220  push esi
00CB8221  mov esi, ecx
00CB8223  call 00CB7C40
00CB8228  mov ecx, esi
00CB822A  pop esi
00CB822B  jmp 00CB8170
```

`this` is `[quest+8]` (run). One `.text`
`call 00CB8220`: `004B453E`. **PROVEN**.

Callee stores on first-seen keep:

| VA | Field | First-seen |
|---|---|---|
| `00CB7950` | `[run+44]=watcher` then `0` | **PROVEN** |
| `00CB7950` | `[watcher+40]` | 0 |
| `00CB7950` | `[watcher+41]` | 0 → `vtbl+4` (not `vtbl+24`) |
| `00CB7C40` | `[run+4]` circular | Sunnyvale `"Main"` first |
| `00CB8170` | `[run+8]` vector | empty (`je 00CB81FD`) |

Host has **no** `QuestRunPlus44` /
per-factory `[run+4]` / `[run+8]`.
Those missing stores are **UNREAD** on
the host object, not leftover host
fields. The leftover is the **counter
that pretends to be the walk**.

---

## 2. Host first Game pump fields

`TickWorld` first type-1 is
`PumpQuests` then `PumpScripts` /
`PumpEvents` … (`EngineLifecycle`
`TickWorld`). First Game `Pump()` does
not enter that (`EvaluatePlayerCatchup`
`0041674A=0`). **PROVEN**.

Public fields after first `Pump(0.1f)`,
declaration order at the quest block:

| Host field | After first type-1 | Native `00CB8220` | Class |
|---|---|---|---|
| `QuestPumpRan` | `true` | type-1 ran | **MATCH** |
| **`QuestPumpWalked`** | **`12`** | **8** `00CB8220` / **10** `00CB7950` | **LEFTOVER** (first) |
| `QuestVtbl24Calls` | `0` | `+41==0` skips `vtbl+24` | **MATCH** |
| `ScriptPumpWalked` | `0` | `006E75C0` `[+60]` empty | **MATCH** (not this trampoline) |
| `EventPumpWalked` | `10` | **8** posts, fire **0** | **LEFTOVER** (`006874B0`) |
| `GameflowWatchers` | Main+Core+Barrow | Gameflow `[this+4]` insert-at-tail | **MATCH** last factory |
| `GameflowState` | `0` | `00CE77D7` | **MATCH** |
| `GameflowYieldQuest` | `Q_NewOakValeIntro` | `00893610` miss → yield | **MATCH** wait; **DISPROVEN** activate |
| `EventPosts` | `10` | **8** `00687540(55,50)` at construct | **LEFTOVER** (not this pump) |

`Type1_00CB8220_*` locks `QuestPumpWalked==12`
then `EventPosts==10` then
`EventPumpWalked==10`. First of those
three that belongs to `00CB8220` is
`QuestPumpWalked`.

---

## 3. Why `12` (how the leftover grows)

`PumpQuestList` `++` on every
`TickNamedQuestMain` plus Gameflow
Main / Core / Barrow:

| Host `++` | Native `00CB8220`? | Native `00CB7950`? |
|---|---|---|
| `Q_SunnyvaleMaster` | first | Main `00CDD360` |
| `ChapterAndSceneManager` | **skip `[+8]==0`** | **0** |
| `PersonalScriptMain` | yes | Main |
| `PersonalScript_GlobalThings` | yes | Main |
| `NPCDeath` | **skip `[+8]==0`** | **0** |
| `HeroBoasts` | yes | Main |
| `V_HeroDolls` | yes | Main (**PARTIAL** body) |
| `CS_PlayCutscene` | yes | Main |
| `Global_WatchForHeroDeath` | yes | Main (**PARTIAL** body) |
| Gameflow Main | last | `00CE7670` |
| Core | same last `[run+4]` | `00CEF3B0` |
| Barrow | same last `[run+4]` | `00CEF550` |

12 host ticks. 8 native `00CB8220`.
10 native `00CB7950`. The two extra
`QuestPumpWalked` increments are the
factory-0 else-arm (`factory0-type1-tick`).
The trampoline is one note for all eight
live factories (`host-type1-walk-order`).

First leftover **note field** on the
same function (before any `++`) is
`00CB7C40 count=_gameflowWatchers.Count`
(Gameflow Main at entry, not Sunnyvale
`[run+4]`). First leftover **stored
property** is still `QuestPumpWalked`.

---

## 4. What this is not

| Claim | Class |
|---|---|
| Host first Game `Pump()` is first `00CB8220` | **DISPROVEN** (dummy) |
| `QuestPumpWalked==12` is native `00CB8220` count | **DISPROVEN** |
| `QuestPumpRan` leftover | **DISPROVEN** (MATCH) |
| `QuestVtbl24Calls=0` leftover | **DISPROVEN** (MATCH `+41`) |
| `GameflowYieldQuest` leftover vs first `00CE75B0` | **DISPROVEN** (`host-yield-first-tick`; write is `00CE7670`) |
| `EventPosts` first leftover of **this** trampoline | **DISPROVEN** (construct `ActivateNamedQuest`) |
| First leftover field is `[run+44]` on host | **UNREAD** host; native **PROVEN** store then clear |
| WLD six / Oakvale as this walk | **DISPROVEN** (`00CB8220-first-pump`) |

---

## Classifications (short)

1. **First type-1 `00CB8220` vs host —
   LEFTOVER**, not MATCH. Head/tail
   names MATCH. Trampoline shape and
   factory-0 ticks DIVERGE.
2. **First leftover field —
   `QuestPumpWalked` PROVEN leftover.**
   Host `12`. Native `8` `00CB8220`.
3. **`QuestPumpRan` / `QuestVtbl24Calls=0`
   / Gameflow wait — MATCH.** Do not
   fold those into the leftover.
4. **Do not start Oakvale.** Wait only.
