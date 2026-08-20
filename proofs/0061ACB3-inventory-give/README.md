# `0061ACB3` `CTCInventoryQuests` Give leftover

Investigation + host leftover lock. No invented UI click.
Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** post kind `0x33` from Pump / Load Quests /
first Present.

Question: unique caller of Give body `004B1D30` at
`0061ACB3` is `0061B5FD`. Can that fire on no-save
first Present?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: ExeIndex
`listing-00600000.txt`
(`0061A420` / `0061A8A0` / `0061AB30` / `0061AC60` /
`0061ACB3` / `0061B590` / `0061B5F0` / `0061B5FD`),
`listing-004c0000.txt` (`004D30A8` / `004D30BD` /
`004D30C8` `"CTCInventoryQuests"`),
`listing-00480000.txt` (`004B1D30` / `004B1DC4`),
`listing-00880000.txt` (`00893570` / `008ABED0`),
`e8.tsv` dest `004B1D30` / `0061AC60` / `0061AB30` /
`0061A420` / `0061B5F0` / `0061A6A0` / `004D30A8`,
`calls-by-dest.tsv` dest `0061AC60`,
`vtbl.tsv` `0x012585B4` slots 4 / 58 / 59,
`abs.tsv` `0x004EF0C5` factory `0x004D30A8`;
siblings `proofs/type33-give-all-writers`,
`proofs/addtestquest-token`,
`proofs/hero-inventory-first`,
`proofs/oakvale-activate-unread-audit`;
host
`EngineLifecycle.InventoryQuestsGiveFn=0061ACB3`
`InventoryQuestsGiveIsFirstSeen=false`
`InventoryQuestsConfirmFn=0061AB30`
`InventoryQuestsConfirmIsNewGame=false`;
test `No_save_does_not_activate_Q_NewOakValeIntro`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Unique caller of `0061AC60` / `0061ACB3`? | **`0061B5FD`** inside `0061B5F0`. One `e8.tsv` row. | **PROVEN** |
| Unique caller of `0061B5F0`? | **None.** Zero `E8`. `vtbl.tsv` `0x012585B4` slot 59 only. Zero `abs.tsv`. | **PROVEN** |
| Can it fire on no-save first Present? | **No.** Class bind is not construct. Widget not ticked. `008ABED0` type `0x33` = 0. | **DISPROVEN** |
| Invent a UI click that posts `0x33`? | **No.** Leftover menu. Wait stays miss. | **DISPROVEN** |
| Host leftover flags? | `InventoryQuestsGiveFn=0061ACB3` `IsFirstSeen=false`. Load Quests Note omit. | **MATCH** |

---

## Verdict

`0061ACB3` **is** Give body `004B1D30` (the other of two
`E8`s; thunk is `00892F9F`). It is **not** a second
`00687540(51)` site. Path:

```
004EF0AE  004D2EF0 "CTCInventoryQuests" factory 004D30A8   // BIND class
004D30BD  call 0061A420                                   // ctor; unique E8
          vtbl 0x012585B4 slot 59 = 0061B5F0              // 0 E8
0061B5FD  call 0061AC60                                   // unique E8
          test [this+343]; je skip
0061ACB3  call 004B1D30                                   // Give 0x33
```

Init Thing Components **registers** the CTC row
(`proofs/hero-inventory-first`). It does not construct
the widget or tick slot 59. `e8.tsv` dest `004D30A8` /
`0061B5F0` / `0061A6A0`: **zero**. Opening
`PC_QUESTS_SELECTION_MENU` is leftover
(`proofs/addtestquest-token`). Sibling confirm
`0061AB30` unique `E8` is `0061B59D` in slot 58
`0061B590`, same `[this+343]` gate, already Noted
not New Game.

Ctor defaults `[+343]=1` / `[+344]=0` would Give
`world+196[0]` **if** slot 59 ran. Type-1 /
no-save `008ABED0` type `0x33` = 0, so it did not.
Do not invent that click. Do not `ActivateQuest`.

---

## Evidence → Original → Host → Gap

| Evidence | Original | Host | Gap |
|---|---|---|---|
| `e8.tsv` dest `0061AC60`: `0061B5FD` | Unique caller | `InventoryQuestsGiveFn=0061ACB3` | **MATCH** |
| `e8.tsv` dest `0061B5F0` / `0061A6A0` / `004D30A8`: 0 | Vtbl / CTC factory pointer | none on Pump | **MATCH** omit |
| `listing-00600000` `[this+343]` then `004B1D30` | Gated leftover Give | `InventoryQuestsGiveIsFirstSeen=false` | **MATCH** |
| `004D30C8` `"CTCInventoryQuests"` after factory `004D30A8` | Name of this widget | Note `"CTCInventoryQuests Give leftover"` | **MATCH** |
| `004EF0AE` bind, not ctor | Class row only on Init Thing Components | no widget tick | **MATCH** |
| Type-1 `EventPosts` kind 55; `008ABED0` type `0x33` = 0 | Slot 59 unreached | hardcoded miss | **MATCH** |
| Invent UI click / `ActivateQuest` | would DIVERGE wait | lock false | **DISPROVEN** |

---

## 1. Unique caller `0061B5FD`

`calls-by-dest.tsv`:

```
0x0061AC60	0x0061B5FD	0x0061B5F0
```

One row. `listing-00600000`, int3-bounded:

```
0061B5F0  push esi
          mov esi, ecx
          mov al, [esi+343]
          test al, al
          je  0061B602
0061B5FD  call 0061AC60
0061B602  mov ecx, esi
          pop esi
          jmp 005BC66F              ; base
```

Sibling slot 58 `0061B590` is confirm:

```
0061B590  test [esi+343]
          je  alternate
0061B59D  call 0061AB30             ; unique E8 of confirm
```

`vtbl.tsv` `0x012585B4`:

| Slot | Off | Dest |
|---:|---:|---|
| 4 | 16 | `0061A6A0` (family; 0 `E8`) |
| 58 | 232 | `0061B590` confirm |
| 59 | 236 | `0061B5F0` Give |

Zero `E8` / `abs` of `0061B5F0`. Slot 59 is an input
virtual, not a first-Present tick.

---

## 2. `0061AC60` is the same Give body

```
0061AC60  test [this+343]
          je skip
          0061A8A0                  ; copy world+196
          esi = [this+344] * 28 + table
          push 0,0,0
0061ACB3  call 004B1D30(esi)        ; name at record+0
```

`e8.tsv` dest `004B1D30`: **two** rows only
(`00892F9F` thunk, this site). Name is the selected
AddTestQuest CString, not a PE `Q_NewOakValeIntro`
push (`xrefs.tsv` `0x012C5D14` is bind + card + wait).

`0061A8A0` copies `world+196`. Confirm `0061AB30`
can later `004B4A10` / `004B4C50`. Give here does
**not** construct. Construct is still `0x37`.

---

## 3. No-save first Present does not reach slot 59

```
004EE23F  Init Thing Components
  004EF0AE  "CTCInventoryQuests" → factory 004D30A8   // BIND
004A1840  Load Quests
  AddTestQuest → world+196 only                      // store
0043A380  PLAYER_GUI_PC                              // not this widget
type-1 00CB8220
  00CE7670  00893570 → 008ABED0 type 0x33 = 0
0061A420 / 0061B5F0 / 0061ACB3                       // not here
```

`e8.tsv` dest `0061A420`: **one** row `004D30BD`
inside factory `004D30A8`. Factory itself has **0**
`E8`; `abs.tsv` `004EF0C5` stores the pointer.
Construct of the widget is `call eax` after a later
name lookup, not this walk.

If slot 59 had run, ctor `[+343]=1` / `[+344]=0`
would Give `world+196[0]` through `004B1D30` →
`00687540(51)` (index hit). Observed type `0x33`
count is **0**. **PROVEN** unreached.

---

## Host

`No_save_does_not_activate_Q_NewOakValeIntro`

| Host | Native | Class |
|---|---|---|
| `InventoryQuestsGiveFn=0061ACB3` | second `E8` of `004B1D30` | **MATCH** |
| `InventoryQuestsGiveIsFirstSeen=false` | leftover slot 59 | **MATCH** |
| Load Quests Note `"0061ACB3 … leftover not first-seen"` | omit, same family as `0061AB30` | **MATCH** |
| `InventoryQuestsConfirmFn=0061AB30` not New Game | sibling slot 58 | **MATCH** |
| `QuestGiveBody=004B1D30` kind `0x33` | body | **MATCH** constants |
| no `0x33` EventPost | slot 59 unreached | **MATCH** |
| Invent UI click / `ActivateQuest` | leftover / still `0x37` | **DISPROVEN** |

Do not call `0061AC60` from Pump. Do not post `0x33`.

---

## Classifications (short)

1. **Unique caller — PROVEN `0061B5FD` in `0061B5F0`
   (vtbl slot 59, 0 `E8`).**
2. **No-save first Present — DISPROVEN.** Class bind
   is not construct. Type `0x33` stays 0.
3. **Leftover — PROVEN.** Same widget as confirm
   `0061AB30`. Host `IsFirstSeen=false` **MATCH**.
4. **Invent UI click / `ActivateQuest` — DISPROVEN.**

---

## Sources (absolute)

- `C:\FableCSharp\assembly\exe\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\calls-by-dest.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00600000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\proofs\type33-give-all-writers\README.md`
- `C:\FableCSharp\proofs\addtestquest-token\README.md`
- `C:\FableCSharp\proofs\hero-inventory-first\README.md`
- `C:\FableCSharp\proofs\oakvale-activate-unread-audit\README.md`
