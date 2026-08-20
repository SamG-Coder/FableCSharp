# Childhood objective writer VAs `01`–`06`

Investigation. Listing MATCH on all six first-push
sites. Host already locked `01` / `05` / `06` plus
`ChildhoodObjectivesRunOnNoSave=false`. This note
confirms `02` / `03` / `04`. Constants for those
three may land in `RegionTravel` **only** as data
locks. Do **not** run them. Do **not** auto-complete
quests. Do **not** invent `ActivateQuest`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: dump `listing-00d80000.txt`
`00DAC1BA` / `00DB080A` / `00DBE34F` /
`00DB4A93` / `00DB9DE6` / `00DBE478`
(and extra `01` `00DB91A8`, extra `05`
`00DBA277` / `00DBA766`);
`00-index/strings.tsv` intern
`0x012D8244` … `0x012D9D54`;
`xrefs-by-string.tsv` (push+1);
`src/Fable.Game/RegionTravel.cs`
`ChildhoodObjective*Fn`;
siblings `proofs/oakvale-childhood-objectives`,
`proofs/raid-avi-attackover-live`,
`proofs/novi-factory-starts`,
`proofs/q-novi-activator-callers`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Is `01` `00DAC1BA`? | **Yes.** `push "TEXT_QUEST_OAKVALE_INTRO_OBJECTIVE_01"` then `vtbl+2620` / `+1184`. Owner `00DABAC0`. | **PROVEN** / host **MATCH** |
| Is `02` `00DB080A`? | **Yes.** Same HUD shape. Owner `00DB0660`. | **PROVEN** |
| Is `03` `00DBE34F`? | **Yes.** Same HUD shape. Owner `WatchForGotGold` `00DBE2E0`. | **PROVEN** |
| Is `04` `00DB4A93`? | **Yes.** Same HUD shape. Owner `00DB4095`. | **PROVEN** |
| Is `05` `00DB9DE6`? | **Yes.** First of three pushes in `00DB97A0`. | **PROVEN** / host **MATCH** |
| Is `06` `00DBE478`? | **Yes.** Owner `00DBE3C0` PostAttack. | **PROVEN** / host **MATCH** |
| No-save runs any of these? | **No.** Quest never constructed. | **DISPROVEN** (`ChildhoodObjectivesRunOnNoSave=false`) |
| Invent `ActivateQuest` so they fire? | **No.** | **DISPROVEN** |

---

## Verdict

**All six first-push VAs MATCH the dump.** Host already
names `01` / `05` / `06`. `02` / `03` / `04` are the
same `push -1` / intern / `0099EBF0` / `vtbl+2620` /
`vtbl+1184` shape. Integrator constants:

```
ChildhoodObjective01Fn = 0x00DAC1BA   // locked
ChildhoodObjective02Fn = 0x00DB080A   // MATCH extra
ChildhoodObjective03Fn = 0x00DBE34F   // MATCH extra
ChildhoodObjective04Fn = 0x00DB4A93   // MATCH extra
ChildhoodObjective05Fn = 0x00DB9DE6   // locked
ChildhoodObjective06Fn = 0x00DBE478   // locked
ChildhoodObjectivesRunOnNoSave = false
```

Do **not** call these from Pump. They are push sites,
not activate, not Give, not smash.

---

## 1. Listing vs claimed VA

`xrefs-by-string.tsv` stores **push+1** (imm). The
instruction VA is that dword minus one. Host consts
are the `push "TEXT_…"` mnemonic line.

| # | Intern | `push` listing | xref site | Owner `fn=` | Extra listing |
|--:|---|---|---|---|---|
| 01 | `0x012D8244` | **`00DAC1BA`** | `00DAC1BB` | `00DABAC0` | `00DB91A8` (`fn=00DB8680`) |
| 02 | `0x012D8978` | **`00DB080A`** | `00DB080B` | `00DB0660` | none |
| 03 | `0x012D9D2C` | **`00DBE34F`** | `00DBE350` | `00DBE2E0` | none |
| 04 | `0x012D8EFC` | **`00DB4A93`** | `00DB4A94` | `00DB4095` | none |
| 05 | `0x012D9894` | **`00DB9DE6`** | `00DB9DE7` | `00DB97A0` | `00DBA277` / `00DBA766` (same fn) |
| 06 | `0x012D9D54` | **`00DBE478`** | `00DBE479` | `00DBE3C0` | none |

Shared HUD tail (every site):

```
push -1
push intern
call 0099EBF0              ; CString
… empty intern pair …
call [ctx.vtbl+2620]
call [ctx.vtbl+1184]       ; apply
call 0099EAE0 ×4           ; dtor
```

**PROVEN** identical apply. `02` / `04` use
`[esi+64]` or `[esi+4]` as context; still
`+2620` / `+1184`.

---

## 2. Extra pushes — not the locked const

`01` second site `00DB91A8` is father body
(`00DB8680` grouping / `00DB86B0` start). Host
locks the **first** `00DABAC0` push.

`05` later `00DBA277` / `00DBA766` are re-entry
in the same Theresa start. Host locks the
**MEET_YES** first push `00DB9DE6`.

Do **not** rename the locked const to an extra
site.

---

## 3. Host before / after this note

Already locked (`RegionTravel` /
`EngineLifecycleTests.No_save_does_not_activate_*`
/ `WorldSceneTests`):

| Const | Value | Listing |
|---|---|---|
| `ChildhoodObjective01Fn` | `00DAC1BA` | **MATCH** |
| `ChildhoodObjective05Fn` | `00DB9DE6` | **MATCH** |
| `ChildhoodObjective06Fn` | `00DBE478` | **MATCH** |
| `ChildhoodObjectivesRunOnNoSave` | `false` | **MATCH** omit |

`02` / `03` / `04` listing **MATCH**. Data-only
consts now sit next to `01` / `05` / `06`. Still
**false** run on no-save: `DoesNotContain`
`Q_NewOakValeIntro` on `ActivatedQuests` /
`Runtime.Quests`; bind site `00CD6E27` is **not**
`00CB5AD0`.

No Pump analog of `vtbl+2620` for these six
strings. **PROVEN** host gap. Keep it.

---

## 4. Do not

| Temptation | Class |
|---|---|
| `ActivateQuest("Q_NewOakValeIntro")` so 01–05 fire | **DISPROVEN** (`q-novi-activator-callers`) |
| Auto-complete 02/03/04 by poking gold / `+148` / teddy | **DISPROVEN** (`oakvale-childhood-objectives`) |
| Treat 06 as a childhood deed | **DISPROVEN** (PostAttack after `AttackOver`) |
| Call `ChildhoodObjective*Fn` from Pump | **DISPROVEN** (push sites, not a host API) |
| Collapse extra `05` pushes into skip-MEET | **DISPROVEN** (`raid-avi-attackover-live`) |

---

## Classifications (short)

1. **`01` `00DAC1BA` / `05` `00DB9DE6` / `06`
   `00DBE478` — PROVEN.** Host **MATCH**.
2. **`02` `00DB080A` / `03` `00DBE34F` / `04`
   `00DB4A93` — PROVEN.** Extra consts **MATCH**.
3. **No-save run — DISPROVEN.** Keep
   `ChildhoodObjectivesRunOnNoSave=false`.
4. **ActivateQuest invented to reach them —
   DISPROVEN.**

---

## Next UNREAD

Who later constructs `Q_NewOakValeIntro` so these
`push` sites can execute. Not this note. Not Pump.
Not a host HUD helper.
