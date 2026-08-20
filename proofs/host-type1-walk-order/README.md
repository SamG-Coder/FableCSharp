# First `00CB7C40` after Leave vs host `PumpQuestList` order

Investigation only. No production `src/` edits.

Question: first `00CB7C40` after Leave. Head is
`Q_SunnyvaleMaster`, not Gameflow. Does host
`PumpQuestList` order **MATCH**?

Do **not** treat the first walk as Gameflow.
Do **not** collapse `004B4490` `[QM+56]` into
one `00CB7C40`. Do **not** invent
`ActivateQuest("Q_NewOakValeIntro")`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: `proofs/quest-type1-first-walk` (first
`00CB8220` / `00CB7C40` is Sunnyvale factory;
`[this+4]` head is `"Main"` `00CDD360`);
`proofs/gameflow-main-first-tick` (`00CB7C40` is
not on `00CE75B0` construct; Gameflow is later
on the same type-1);
`EngineLifecycle.PumpQuestList` /
`TickNamedQuestMain` / `TickGameflowMain`;
`proofs/factory0-type1-tick`;
`proofs/host-gameflow-tick-diverge`;
`EngineLifecycleTests`
(`Init_quests_004B4260_activates_wld_initial_list`,
`Gameflow_00CE75B0_is_Main_watcher_not_S_GF`,
`Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`).

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| First `00CB7C40` after Leave? | first type-1 `004B4490` → first live `[QM+56]` factory = Sunnyvale | **PROVEN** |
| Head of that `[this+4]`? | Sunnyvale `"Main"` → `00CDD360` | **PROVEN** |
| Is Gameflow that head? | no. Gameflow is last factory on the same pump | **DISPROVEN** |
| Host first `TickNamedQuestMain` name? | `Q_SunnyvaleMaster` (`00CDD360`) | **MATCH** |
| Host Gameflow before Sunnyvale? | no. foreach skips the string, then `TickGameflowMain` | **MATCH** |
| Host one-list `00CB7C40` of every name? | no. Native is **per factory** `[this+4]` | **DISPROVEN** |
| Full `PumpQuestList` walk **MATCH**? | **No.** Head/tail yes; factory-0 extras + trampoline shape no | **DIVERGE** |

---

## Verdict

**Head/tail MATCH. Full walk DIVERGE.**

Native first `00CB7C40` after Leave is **not**
Gameflow. It is the Sunnyvale factory’s
`[this+4]` on the first type-1 `00CB8220`.
`user.ini` already attached Gameflow. Gameflow
is the **last** live `[QM+56]` slot on that
same `004B4490`. **PROVEN**
(`quest-type1-first-walk`,
`gameflow-main-first-tick`).

Host `PumpQuestList` keeps that **first/last**
order:

1. `_activatedQuests` is `WorldPlus172` then
   `"Gameflow"` (`Init_quests_004B4260_*`).
2. foreach skips only the Gameflow **string**.
   First `TickNamedQuestMain` is
   `Q_SunnyvaleMaster` → note `00CDD360`.
3. After the nine WLD names,
   `TickGameflowMain` / Core / Barrow.

That is **MATCH** for “head Sunnyvale, not
Gameflow.”

It is **not** MATCH as a `00CB7C40` walk.

- Native: **one** `00CB8220` / `00CB7C40` /
  `00CB8170` **per live factory**. Factory 0
  (`ChapterAndSceneManager`, `NPCDeath`)
  `[slot+8]==0` → skip. Eight `00CB8220`.
  Ten `00CB7950` (seven Mains + Gameflow
  Main / Core / Barrow).
- Host: **one** trampoline note for the
  whole pump. `00CB7C40 count=` is
  `_gameflowWatchers.Count` (1 = Main at
  entry), not Sunnyvale’s `[this+4]`.
  `TickNamedQuestMain` still notes the two
  factory-0 names. `QuestPumpWalked==12`.

**DIVERGE** extras / shape
(`factory0-type1-tick`). Not a Gameflow-first
hole.

| Token | Native | Host | Class |
|---|---|---|---|
| First `00CB7C40` `this` | Sunnyvale factory | first named tick `Q_SunnyvaleMaster` | **MATCH** |
| First `[this+4]` head | Sunnyvale `"Main"` `00CDD360` | `SunnyvaleMainTick` `00CDD360` | **MATCH** |
| Gameflow as first walk | last factory | after WLD names | **MATCH** last |
| `00CB8220` count | **8** | 1 trampoline note + 9 named `00CB8220 name` | **DIVERGE** shape |
| `00CB7950` count | **10** | **12** (`QuestPumpWalked`) | **DIVERGE** |
| Factory 0 on walk | visit `[QM+56]`, skip `00CB8220` | `TickNamedQuestMain` else-arm | **DIVERGE** |
| `00CB7C40 count=` note | n/a (per factory) | Gameflow watchers at entry | **LEFTOVER** as first-list size |
| `GameflowWaitQuest` as first head | `00CE7670` on last factory | `TickGameflowMain` after names | **LEFTOVER** as first-walk label |

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend
0042F491 Init Game → 004184BD
  004B4260([world+172])                 // 9 QST TRUE
    [QM+56] tail-insert
      1 Q_SunnyvaleMaster        +8=run
      2 ChapterAndSceneManager   +8=0
      3 PersonalScriptMain       +8=run
      4 PersonalScript_GlobalThings +8=run
      5 NPCDeath                 +8=0
      6 HeroBoasts               +8=run
      7 V_HeroDolls              +8=run
      8 CS_PlayCutscene          +8=run
      9 Global_WatchForHeroDeath +8=run
  user.ini ActivateQuest("Gameflow")    // AFTER +172
    00CE75B0 attach Main                // NOT 00CB7C40
    [QM+56] tail = Gameflow +8=run
004189C2 first pumps
  dummy inner 0041674A=0 → no 00CB8220
  first type-1 004A5A40 → 004B4490
    walk [esi+56]:
      Sunnyvale +8!=0 → 00CB8220
        00CB7C40                        // FIRST 00CB7C40
          [this+4] head = Main 00CDD360
        jmp 00CB8170
      ChapterAndSceneManager +8==0 SKIP
      … live factories …
      NPCDeath +8==0 SKIP
      … live factories …
      Gameflow +8!=0 → 00CB8220         // LAST
        00CB7C40 Main then Core / Barrow
        00CE7670 wait Q_NewOakValeIntro

Host Pump() #1  Leave → EnterGame
  WorldPlus172 then ActivateNamedQuest("Gameflow")
  GameflowYieldQuest == null
Host Pump() #2  dummy GamePump; QuestPumpRan=false
Host Pump(0.1f) first type-1
  PumpQuestList
    note 00CB8220 00CB7C40 then 00CB8170
    note 00CB7C40 count=_gameflowWatchers (1)
    TickNamedQuestMain Q_SunnyvaleMaster     // FIRST name
    TickNamedQuestMain ChapterAndSceneManager // extra
    TickNamedQuestMain PersonalScriptMain
    TickNamedQuestMain PersonalScript_GlobalThings
    TickNamedQuestMain NPCDeath              // extra
    TickNamedQuestMain HeroBoasts
    TickNamedQuestMain V_HeroDolls
    TickNamedQuestMain CS_PlayCutscene
    TickNamedQuestMain Global_WatchForHeroDeath
    TickGameflowMain                         // LAST factory
    TickCoreReminder
    TickBarrowGuards
    note 00CB8170 [+8]=0 empty
  QuestPumpWalked == 12
```

---

## 1. Native first `00CB7C40` is Sunnyvale

`00CB8220` (`listing-00c80000.txt`):

```
00CB8220  push esi
00CB8223  call 00CB7C40
00CB822B  jmp 00CB8170
```

One `.text` `call 00CB7C40`: `00CB8223`.
One `.text` `call 00CB8220`: `004B453E`
inside `004B4490`. Not `00CE75B0` /
`00CB7900`. **PROVEN**.

`004B4490` walks `[QM+56]` tail-insert
and calls `00CB8220([slot+8])` only when
`[slot+8] != 0`. First live slot after
no-save Init Quests is `Q_SunnyvaleMaster`.
**PROVEN**.

`00CB7C40` walks **that factory’s**
`[this+4]`, not `[QM+56]`. Head is
Sunnyvale `"Main"` (`00CDD380` /
`00CB7E50` first insert). First
`00CB7950` → `00CDD430` → `00CDD360`.
**PROVEN**.

Gameflow’s `"Main"` lives on **Gameflow**
`[this+4]`. First `00CE7670` is the last
`00CB8220` on this pump, not the first
`00CB7C40`. **PROVEN**.

---

## 2. Host `PumpQuestList` order

`C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`

```
foreach (var name in _activatedQuests)
{
    if (name == "Gameflow")
        continue;
    TickNamedQuestMain(name);
}
if (_gameflowWatchers.Contains(WatcherMain) &&
    GameflowYieldQuest is null)
{
    TickGameflowMain();
    TickCoreReminder();
    TickBarrowGuards();
}
```

`_activatedQuests` after `EnterGame`:

| # | Name | Native `00CB8220`? | Host tick |
|--:|---|---|---|
| 1 | `Q_SunnyvaleMaster` | **first** | first `TickNamedQuestMain` `00CDD360` |
| 2 | `ChapterAndSceneManager` | skip `[+8]==0` | else-arm `00CB7950` |
| 3 | `PersonalScriptMain` | later | `00CDDCB0` |
| 4 | `PersonalScript_GlobalThings` | later | `00CDDCB0` |
| 5 | `NPCDeath` | skip `[+8]==0` | else-arm `00CB7950` |
| 6 | `HeroBoasts` | later | `00CE1AF0` |
| 7 | `V_HeroDolls` | later | else-arm |
| 8 | `CS_PlayCutscene` | later | else-arm |
| 9 | `Global_WatchForHeroDeath` | later | else-arm |
| 10 | `Gameflow` | **last** | skipped in foreach; then Main / Core / Barrow |

Rows 1 and 10 **MATCH** native first/last
factory. Rows 2 and 5 **DIVERGE**. Locked
by `Init_quests_004B4260_*` (list) and
`Type1_00CB8220_*` (`QuestPumpWalked==12`,
`SunnyvaleMainTick` present,
`GameflowTickFn` present, no Oakvale
activate).

The skip-`Gameflow` `continue` is **not**
a missed wait. `TickGameflowMain` still
runs `00CE7670` on this type-1
(`host-gameflow-tick-diverge`).

---

## 3. What “order MATCH” is not

| Claim | Class |
|---|---|
| First host name is Gameflow / `Q_NewOakValeIntro` | **DISPROVEN** |
| First native `00CB7C40` is Gameflow Main | **DISPROVEN** |
| Host `00CB7C40 count=` is Sunnyvale `[this+4]` length | **DISPROVEN** (Gameflow watchers) |
| One host trampoline = one native `00CB8220` | **DIVERGE** shape |
| `QuestPumpWalked==12` = native `00CB8220` count | **DISPROVEN** (native 8) |
| `Runtime.Update` / `Scheduler.Pump` is this walk | **LEFTOVER** (unused on Leave `Pump()`) |

`fiber-yield-first` once said host
`QuestPumpWalked==9`. Current host is
**12**. Native live `00CB8220` is **8**.
Use `quest-type1-first-walk` for the
count.

---

## Classifications (short)

1. **First `00CB7C40` after Leave — PROVEN
   Sunnyvale factory, first type-1.** Head
   of `[this+4]` is `Q_SunnyvaleMaster`
   Main (`00CDD360`), not Gameflow.
2. **Host first/last order — MATCH.**
   `PumpQuestList` ticks Sunnyvale first
   and Gameflow last (`TickGameflowMain`
   after the WLD names).
3. **Full `PumpQuestList` as `00CB7C40`
   — DIVERGE.** Native is per-factory
   `[this+4]` and skips factory 0. Host
   one trampoline + two extra name ticks
   + `QuestPumpWalked==12`.
4. **Gameflow as first walk head —
   DISPROVEN** native and host. First
   `00CE75B0` is construct, not this
   walk. First `00CE7670` is last factory.
