# Who first Gives type-`0x33` `Q_NewOakValeIntro` onto `[world+96]`

Investigation only. No production `src/` or `tests/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** treat construct `004B3CE0` / kind `0x37` as the
Gameflow wait object. Do **not** collapse Give (`0x33`)
with construct (`0x37`).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: ExeIndex
`assembly/exe/01-sections/text-map/listing-00cc0000.txt`
(`00CE7670` / `00CE77D7` / `00CE791D` / `00CE7977` /
`00CE79C9` / `00CD6E27`),
`listing-00880000.txt`
(`00893570` / `00893610` / `00892F80` / `00891880` /
`008ABED0` / `00892E80`),
`listing-00480000.txt`
(`004B4260` / `004B42E8` / `004B1D30` / `004B3CE0` /
`004AF740` / `004B4490` / `004B4535`),
`listing-00d80000.txt`
(`00DBDE40` / `00DBE295` / `00DAC295` / `00DABAC0`),
`listing-00c80000.txt` (`00CB5AD0` / `00CB5C90`),
`listing-006c0000.txt` (`006E7510` / `006E7410`),
`listing-00680000.txt` (`00687540`),
`listing-00600000.txt` (`0061ACB3`),
`listing-005c0000.txt` (`005E7B77`),
`e8.tsv` dest `00CB5AD0` / `004B1D30` / `00687540`,
`ff.tsv` disp `1152`,
`xrefs.tsv` / `xrefs-by-string.tsv` `0x012C5D14`,
`vtbl.tsv` `0x01260F0C` slots 25 / 288 / 655;
siblings `proofs/gameflow-state0-wait`,
`proofs/quest-type-0x33`,
`proofs/gameflow-oakvale-wait`,
`proofs/sqnovi-first-construct`,
`proofs/00DBDE40-host-gap`,
`proofs/raid-avi-attackover-live`;
host
`EngineLifecycleTests.Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`
/ `Type1_resume_00CB8220_is_00A44880_then_00893570_yield`
/ `No_save_does_not_activate_Q_NewOakValeIntro`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| What does Gameflow state 0 wait for? | `[esi+64].vtbl+100` `00893570("Q_NewOakValeIntro")` = type-`0x33` on `[world+96]` **and** `004AF3C0` name equals. **Not** construct `0x37`. | **PROVEN** |
| Who first Gives that node on no-save? | **Nobody.** Zero `vtbl+1152` / `004B1D30` on this walk. `008ABED0` type `0x33` = 0. | **PROVEN** omit |
| Wait forever, or later Give after region? | **Wait forever on no-save.** Yield loop has no timeout. Resume re-misses. Later Give is **`00DBE295` after `StartOakVale` map-ready and `AttackOver`**, not at region load, not at construct. | **PROVEN** wait; later Give **PROVEN** site, **blocked** (quest never constructed) |
| Does `004B3CE0` / `00CB5AD0` post `0x33`? | **No.** Live arm posts **`00687540(55,50)` = kind `0x37`**. Unique `00CB5AD0` `E8` is **`004B42E8`** in `004B4260`. | **DISPROVEN** as Give |
| Unique `00CB5AD0` `E8`? | **`004B42E8`** only (`e8.tsv` 1 row; `ff.tsv` 0). | **PROVEN** |
| PE literal `Q_NewOakValeIntro` at a Give site? | **No.** Five xrefs: bind `00CD6E28`/`00CD6E87`, card `00CE791E`, wait `00CE7978`/`00CE79CA`. | **PROVEN** |
| First later Give of that **name**? | `00DBE295` `vtbl+1152` while S_QNOVI ticks: `vtbl+2620` `00891880` copies `[QM+136]+48` (current slot name) into `004B1D30` → `00687540(0x33)`. | **PROVEN** site; **not** no-save |
| Invent `ActivateQuest` to leave the yield? | **No.** Construct still posts `0x37`. Wait needs `0x33`. | **DISPROVEN** |

**No-save: nobody Gives type-`0x33` `Q_NewOakValeIntro`. Gameflow waits forever. Later Give is `00DBE295` after Oakvale region **and** AttackOver, not `004B3CE0`.**

---

## Verdict

Gameflow `00CE7670` state 0 (`00CE77D7`) is a **peer waiter**.
It binds the Oakvale **card** (`vtbl+1180`) then polls
`vtbl+100` `00893570`. Miss → `006E7410` / `009D8650`.
It does **not** construct, Give, or `00CB5AD0` that name.

`00893570` walks `[iface+4]+96` = `[world+96]` for event
kind **`0x33` (51)** whose `[+60]` `QM+44` index CString
equals `"Q_NewOakValeIntro"`. Construct
`004B3CE0` inserts kind **`0x37` (55)** on the same list.
Those nodes **cannot** satisfy this wait.

The writer of kind `0x33` is `004B1D30`
(`01260F0C` slot 288 `vtbl+1152` `00892F80`) →
`00687540(51, 50, …, 004AF740(name))`. First Oakvale
**name** at that writer is **not** a PE push. It is
`00DBE295` inside `00DBDE40` **after** the `AttackOver`
spin: `vtbl+2620` `00891880` copies the ticking 52-byte
slot’s `+48` CString (set as `QM+136` by `004B4490`
around `00CB8220`). That slot is `Q_NewOakValeIntro`
only while S_QNOVI ticks.

On no-save New Game that fiber never runs
(`00CD6E27` is bind `00CB5C90`; WLD `+172` omits the
name; unique `00CB5AD0` is only `004B4260` of other
names). So: **wait forever** until a later, unread
construct lets `00DBDE40` pass `StartOakVale` map-wait
and AttackOver, then Give.

---

## Evidence → Original → Host → Gap

| Evidence | Original | Host | Gap |
|---|---|---|---|
| `listing-00cc0000` `00CE7977`/`00CE79C9` `push "Q_NewOakValeIntro"` `call [edx+100]` / `[eax+100]` | `01260F0C` slot 25 = `00893570`. Invert miss → `006E7410` yield. Loop `jne 00CE79B0`. No timeout. | `QuestIsActiveFn=00893570` `QuestIsActiveVtbl=100`. `TickGameflowMain` Note `"00893570 vtbl+100 Q_NewOakValeIntro 0"` then `GameflowYieldQuest=`. | **MATCH** wait. Host hardcodes miss `0`. |
| `listing-00880000` `00893570` `mov [esp+12], 0x33` `008ABED0` then `004AF3C0` name compare | Wait object is event kind **51** on `[world+96]`, not `QM+56`. | `QuestGiveEventKind=0x33`. Test asserts `!= EventPostKind`. | **MATCH** kind split. Host never posts `0x33`. |
| `004B400A` `push 55` / `push 50` `00687540` | Construct posts **`0x37`**. | `EventPostKind=55` `QuestConstructEventKind=0x37` `EventPostDelay=50`. `ActivateNamedQuest` `EventPosts++` kind 55. Type-1: `EventPosts=10`. | **MATCH** `0x37` count. **DISPROVEN** as wait object. |
| `e8.tsv` dest `00CB5AD0` **one** row `004B42E8`; `ff.tsv` 0 | Unique lookup is inside `004B4260` after `004B00C0`. Then `004BB720` / once `004B3CE0`. **No Give.** | `Init_quests_004B4260_*` / `Activate_quests_00CB5AD0_*`. No-save lists omit Oakvale. | **MATCH** unique site. Host must not invent a second `00CB5AD0`. |
| `xrefs.tsv` `0x012C5D14` five sites only | Bind + card + wait. **Zero** Give / `004B1D30` / `vtbl+1152` literal. | `OakvaleBindSite=00CD6E27` Note `"bind not 00CB5AD0"`. | **MATCH** bind. **PROVEN** no literal Give. |
| `00CE791D` then `vtbl+1180` `00896A30` | Card bind `OBJECT_QUEST_CARD_OAKVALE_INTRO`, **not** Give. | `QuestCardBindVtbl=1180` Note miss. | **MATCH** card. **DISPROVEN** as `0x33`. |
| `listing-00480000` `004B1DC4` `push 51` `00687540` `[QM+124]+96` | Give body. Also posts `0x35` first; `0x33` if out-byte 0. | `QuestGiveFn=00892F80` `QuestGiveVtbl=1152` `QuestGiveBody=004B1D30`. | **MATCH** constants. **PROVEN** omit on Type-1 (no Note of `00892F80`). |
| `e8.tsv` dest `004B1D30`: `00892F9F`, `0061ACB3` | Thunk + later UI table `[this+344]` gated `[this+343]`. | none on Pump | **DISPROVEN** as no-save first giver. |
| `ff.tsv` `00DBE295` `call [edi+1152]` parent `00DB8680` | `00DBDE40` after AttackOver spin / PostAttack. `vtbl+2620` `00891880` (`ret 4`) copies `[QM+136]+48`; leftover zeros + name = Give `ret 16`. | `QuestGiveAfterAttackOver=00DBE295` `NewGameScript.GiveQuestVtbl=1152`. | **MATCH** VA. Host does not execute it. |
| `00891880` `[QM+136]+48` `0099EC30`; `004B4535` `mov [esi+136], slot` then `00CB8220` | Give name = **current ticking slot** `+48`. Oakvale only while S_QNOVI is `QM+136`. | no `QM+136` analog | **PROVEN** original. Host Give of a literal would **DIVERGE** if invented. |
| `00DBDE40` `push "StartOakVale"` `call [eax+48]` then childhood then `+80` spin then `00DBE295` | Map-wait **first**. Give is **after** region-ready **and** AttackOver, still Oakvale. | `DoesNotContain LoadFromFirstRealRegion`; `CurrentRegion=null`; no `00DBDE40` Note. | **MATCH** omit. Later Give ≠ region load. |
| `00DAC295` unique `E8` of `00DBDE40` | Only from slot 2 `00DABAC0`. Zero `E8` of `00DABAC0`. | `RegionTravel.StartOakValeSetup=00DBDE40` constant. | **MATCH** VA. Fiber not scheduled. |
| Type-1 test: not in `ActivatedQuests` / `Runtime.Quests`; `GameflowYieldQuest=Q_NewOakValeIntro`; resume still yield | Native miss stays miss. | same asserts | **MATCH** wait-forever on this walk. |
| `GiveNamedObjectFn=008902E0` tattoo miss | Different vtbl (`+484`). Not quest Give. | Type-1 Note `"008902E0 tattoo 00487DC0 miss"`; resume count **1**. | **MATCH** tattoo. **DISPROVEN** as `0x33`. |

---

## Timeline (no-save New Game)

```
00CD6E27  00CB5C90 bind Q_NewOakValeIntro / S_QNOVI / 00DBEF70   // BIND
          QM+44 / world+184 include the name
          world+172 / ActivatedQuests omit it
0049F24E  004B4260([world+172])                                 // NOT Oakvale
004B42E8  00CB5AD0                                              // unique E8; other names
          004B3CE0 → 00687540(55,50)                            // kind 0x37
user.ini  ActivateQuest("Gameflow")                             // 00892E80 → 004B4A10
          same 004B42E8 / same 0x37 post
first type-1 00CB8220
  Gameflow Main 00A44880 → 00CE7640 → 00CE7670
    attach CoreQuestReminder / CheckBarrowFieldsGuards
    SharedRun+4=0 → 00CE77D7
      vtbl+1180 card OBJECT_QUEST_CARD_OAKVALE_INTRO            // NOT Give
      vtbl+100 00893570("Q_NewOakValeIntro")
        [world+96] 008ABED0 type 0x33 → 0
        006E7410 → 009D8650                                     // WAIT
later type-1
  00A44880 / 009D87F0 resume
  00893570 still 0 → yield                                      // FOREVER on this walk
  no 00CB5AD0, no 004B1D30, no 00DBE295, no region
```

`00DABAC0` / `00DBDE40` / `S_QNOVI` are **not** on this
list. **PROVEN.**

---

## 1. Wait is Give-kind `0x33`, not construct `0x37`

`listing-00cc0000.txt` state 0:

```
00CE791D  push "Q_NewOakValeIntro"
          push "OBJECT_QUEST_CARD_OAKVALE_INTRO"
          call [edx+1180]             ; 00896A30 card; NOT Give
00CE7977  push "Q_NewOakValeIntro"
00CE7995  call [edx+100]              ; 00893570
          neg/sbb/inc                 ; hit→0 miss→1
00CE79AE  je 00CE7A02                 ; skip wait if already Given
00CE79B0  call [edx+28]               ; 006E7410 yield
          …
00CE79C9  push "Q_NewOakValeIntro"
00CE79E7  call [eax+100]
00CE7A00  jne 00CE79B0                ; still miss → yield again
```

No `call […+1152]`, no `004B1D30`, no `00CB5AD0` in
`00CE7670`. **PROVEN** wait-only.

`00893570` (`listing-00880000`):

```
ecx = [iface+4]+96                    ; [world+96]
[key+0] = 0x33
call 008ABED0
hit: [payload+60] → 004AF3C0 → CString == arg
miss / name mismatch: al=0
```

Sibling GET `00893610` is slot 26 (`vtbl+104`): same
type `0x33`, **no** name compare. Wait does **not**
call it. Host used to label the wait `00893610`;
current `QuestIsActiveFn=00893570` is **MATCH**.

`004B3CE0` live arm (`004B400A`):

```
ecx = [QM+124]+96                     ; same list
push 50
push 55                               ; 0x37
call 00687540
```

Factory-0 stub posts **no** event. **DISPROVEN** as
the wait’s node (`proofs/quest-type-0x33`).

---

## 2. Unique `00CB5AD0` `E8` = `004B42E8` in `004B4260`

`e8.tsv`:

```
0x004B42E8	0x00CB5AD0
```

One row. `ff.tsv` has **no** `00CB5AD0`. `00CB5AD0`
is a name→factory lookup (`00CB65D0` on `[manager+4]`);
it does **not** post events.

`004B4260`:

```
004B42D7  call 004B00C0               ; QM+44 membership
004B42DE  je  004B4363                ; skip lookup
004B42E4  mov ecx, [edi+120]
004B42E8  call 00CB5AD0               ; UNIQUE
          004BB720 factory or 0
then once 004B3CE0                    ; 0x37 or stub
```

Callers of `004B4260`: `0049F24E` Init Quests
`[world+172]`, `004B4A5A` 1-name wrapper
(`00892E80` ActivateQuest), `004B5B84` save
`START_ACTIVE_QUESTS`, `0049EAD1` stub. First-seen
lists **exclude** `Q_NewOakValeIntro`
(`sqnovi-first-construct`). **DISPROVEN** as Give
**and** as no-save construct of this name.

Do not invent a second `00CB5AD0("Q_NewOakValeIntro")`.

---

## 3. Give writer `004B1D30` / `vtbl+1152`

`00892F80` (`ret 16`): `ecx=[0x13B89FC]`,
`call 004B1D30`. Slot 288.

`004B1D30`:

```
004AF740(name) → index
ecx = [QM+124]+96
00687540(53, 50, …, index)            ; 0x35
if out-byte == 0:
  00687540(51, 50, …, index)          ; 0x33  ← 004B1DC4
then 004B0160 / 004B0C80              ; card / helper
```

Direct `E8` of `004B1D30`: `00892F9F` (this thunk),
`0061ACB3` (UI, `[esi+343]` gate, name from
`[this+344]` table). Second writer of raw
`push 51` `00687540`: `005E7B77` (`[esi+32]==1`,
name `[esi+80]`). Neither is first-seen Leave /
Type-1. **PROVEN** omit.

Script `ff.tsv` disp `1152` is a large family of
quest fibers calling the same slot. **None** of those
sites push `0x012C5D14`. Oakvale’s Give uses the
**current slot name**.

---

## 4. First later Give of the Oakvale **name**: `00DBE295`

`00DBDE40` (`listing-00d80000`), only `E8` `00DAC295`
in `00DABAC0`:

```
map-wait vtbl+48 "StartOakVale"       ; 00DBDE49 / loop 00DBDE81
00CB7940 abort → ret
READ [this+80] AttackOver             ; 00DBDED9; 1 → skip to PostAttack
CREATURE_HERO_CHILD + three watchers
vtbl+1104 "Q_NewOakValeIntro_PreAttack"   ; CONSTRUCT 0x37 of a *other* name
12 s / HerosOldHouse
SPIN [this+80]                        ; 00DBE200; writer is 00DBB2A7
PostAttack 00DBE3C0 / 00DBEB20
00DBE28B  call [eax+2620]             ; 00891880 copy [QM+136]+48
00DBE295  call [edi+1152]             ; Give that CString
```

`00891880` (`ret 4`):

```
eax = [0x13B89FC]+136
eax += 48
0099EC30 into out CString
```

`004B4490` while ticking a factory object:

```
004B4535  mov [QM+136], slot          ; 52-byte 004B0310, name at +48
004B453E  call 00CB8220
004B4543  mov [QM+136], 0
```

So `00DBE295` Gives **`Q_NewOakValeIntro` iff S_QNOVI’s
slot is `QM+136`**. That requires a prior `004B4260` of
that name (unique `00CB5AD0` `004B42E8`) **and** the
fiber reaching past map-wait + AttackOver.

Give is **after** region-ready, **not** the region load
itself. First no-save real region is Lookout, not
`StartOakVale`. **DISPROVEN** as “Give when Lookout /
any region loads.”

`vtbl+1104` at `00DBE0E0` is ActivateQuest of
**`Q_NewOakValeIntro_PreAttack`**, construct `0x37` of
a different name. **DISPROVEN** as this Give.

---

## 5. Wait forever on no-save

Miss path has no frame cap, no `SharedRun+4` bump
(`00CE77D7` writes `[ecx+4]=0` every entry), no
fallback Give. Resume `00A44880` / `009D87F0` re-enters
the same `00893570` and yields. **PROVEN**
(`gameflow-state0-wait`; Type-1 resume test:
`GameflowYieldQuest` still set, still not in
`ActivatedQuests`).

`QM+44` already contains the name from QST
`AddQuest(..., FALSE)`. Membership is **not** a
type-`0x33` node. **PROVEN.**

---

## Host
`EngineLifecycleTests.Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`

| Host | Native | Class |
|---|---|---|
| `QuestIsActiveFn=00893570` / vtbl `100` | wait slot 25 | **MATCH** |
| `QuestGiveFn=00892F80` / vtbl `1152` / body `004B1D30` / kind `0x33` | Give writer | **MATCH** constants |
| `QuestGiveAfterAttackOver=00DBE295` | later Give site | **MATCH** VA; **PROVEN** unused on this test |
| `EventPostKind=55` `== QuestConstructEventKind` `!= QuestGiveEventKind` | `0x37` vs `0x33` | **MATCH** |
| `EventPosts=10` all kind 55 | WLD TRUE + Gameflow constructs | **MATCH** `0x37` only |
| `GameflowYieldQuest=Q_NewOakValeIntro` | `00893570` miss | **MATCH** |
| not in `ActivatedQuests` / `Runtime.Quests` | no `004B4260` of that name | **MATCH** |
| `GiveNamedObjectFn` tattoo miss, count 1 on resume | `vtbl+484` | **MATCH**; **not** quest Give |
| `CurrentRegion=null` / no `LoadFromFirstRealRegion` | no `00DBDE40` map-wait | **MATCH** omit |
| `TickGameflowMain` always Note `"… 0"` | live `008ABED0` | **MATCH** first-seen; would **DIVERGE** if a later Give existed and host still hardcoded 0 |
| Invent `ActivateNamedQuest("Q_NewOakValeIntro")` | would post `0x37`, still miss `0x33` | **DISPROVEN** unblock |

`ResumeGameflowWait` re-Notes the same miss. Host never
calls `00892F80`. Keep it that way on this walk.

---

## What this is not

| Claim | Class |
|---|---|
| `00CE7670` Gives or constructs Oakvale | **DISPROVEN** |
| `004B3CE0` / `00CB5AD0` / `ActivateQuest` posts type `0x33` | **DISPROVEN** (`0x37` or nothing) |
| `QM+44` / `004AF610` unblocks `00893570` | **DISPROVEN** |
| Card `vtbl+1180` / tattoo `008902E0` is Give | **DISPROVEN** |
| Unique `00CB5AD0` is anywhere but `004B42E8` | **DISPROVEN** |
| No-save first Present / Lookout region Gives Oakvale | **DISPROVEN** |
| `00DBE295` runs on no-save | **DISPROVEN** |
| Wait-success is Guild / `HeroGuildComplex` | **DISPROVEN** (`gameflow-state0-wait`) |
| Invent `ActivateQuest("Q_NewOakValeIntro")` | **DISPROVEN** |

Activator of **construct** remains **UNREAD** (not this
file). Even a later proven `004B4260` of that name
would still need `00DBE295` (or another Give of the
same CString) to satisfy Gameflow.

---

## Classifications (short)

1. **Wait — PROVEN `00893570` (`vtbl+100`) for type-`0x33`
   Give on `[world+96]` named `Q_NewOakValeIntro`.**
   Miss → `006E7410`. No timeout. Resume same miss.
   **Not** construct `0x37` / `004B3CE0`.
2. **No-save first Give — PROVEN nobody.** Unique
   `00CB5AD0` `E8` is `004B42E8` in `004B4260` (other
   names, kind `0x37`). Literal xrefs are bind + card +
   wait. `004B1D30` / `00DBE295` unreached.
   **Wait forever** on this walk.
3. **Later Give — PROVEN site `00DBE295` after
   `StartOakVale` map-wait and `AttackOver`.** Name from
   `[QM+136]+48` while S_QNOVI ticks. **Not** at region
   load. **Blocked** until an unread construct runs that
   fiber. Do not invent `ActivateQuest` to fake it.
4. **Host Type-1 — MATCH wait / MATCH `0x37` vs `0x33`
   constants / MATCH omit Give and region.** Keep
   hardcoded miss. Do not post `0x33` or activate Oakvale
   from Pump.
