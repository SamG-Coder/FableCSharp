# Other type-`0x33` Give presenters of `Q_NewOakValeIntro`

Investigation only. No production `src/` or `tests/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** treat construct `004B3CE0` / kind `0x37` as
the Gameflow wait object. Do **not** collapse Give
(`0x33`) with construct (`0x37`).
`ActivateQuestSatisfiesGameflowWait=false` stays
**locked**.

Question: any **other** presenter of type-`0x33` Give
of `Q_NewOakValeIntro` besides `004B1D30` /
`00DBE295` / `005E7B77` (`CTCQuestCompletionUI`
leftover, not first-seen)?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: ExeIndex
`assembly/exe/01-sections/text-map/listing-00880000.txt`
(`00892F80` / `00893570` / `00891880` / `00892FD0`),
`listing-00480000.txt`
(`004B1D30` / `004B2280` / `004B3CE0` / `004B4490` /
`004AF740`),
`listing-00d80000.txt`
(`00DBDE40` / `00DBE295` / `00DBB2A7` / `00DBE3C0` /
`00DBEB20`),
`listing-005c0000.txt` (`005E78F0` / `005E7B77` /
`005E4740`),
`listing-00cc0000.txt` (`00CE7670` / `00CE7977` /
`00CE79C9`),
`listing-00600000.txt` (`0061AC60` / `0061ACB3` /
`0061B5F0`),
`listing-004c0000.txt` (`004D7E7C`),
`listing-008c0000.txt` (`008C24DC`),
`listing-00e00000.txt` (`00E1AC1C`),
`calls-by-dest.tsv` dest `004B1D30` / `00687540` /
`004AF740` / `00DBDE40`,
`ff.tsv` disp `1152` / `2620`,
`e8.tsv`,
`xrefs.tsv` / `xrefs-by-string.tsv` `0x012C5D14`,
`vtbl.tsv` `0x01260F0C` slots 25 / 288 / 289,
`0x0125589C` slot 26;
siblings `proofs/gameflow-type33-give`,
`proofs/quest-type-0x33`,
`proofs/gameflow-state0-wait`,
`proofs/00DBB2A7-attackover-store`,
`proofs/00DBDE40-after-activate`;
host
`EngineLifecycleTests.Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`
(`ActivateQuestSatisfiesGameflowWait=false`,
`QuestGiveAfterAttackOver=00DBE295`,
`GiveAfterPostAttackAndMaze=true`).

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Other presenter of type-`0x33` Give of `Q_NewOakValeIntro` besides `004B1D30` / `00DBE295` / `005E7B77`? | **No.** | **PROVEN** |
| Who posts kind `0x33` onto `[world+96]`? | Only **`004B1DC4`** inside `004B1D30` and **`005E7B77`** inside `005E78F0`. Census of `push 51` + `00687540`. | **PROVEN** |
| Direct `E8` of `004B1D30`? | **Two** `calls-by-dest` rows: `00892F9F` (`00892F80` thunk) and `0061ACB3` (UI, `[this+343]`). Not a third writer. | **PROVEN** |
| Recovered Oakvale-**name** Give? | **`00DBE295` only**, after AttackOver `00DBB2A7` **and** PostAttack `00DBE3C0` **and** Maze `00DBEB20`. Name is `[QM+136]+48` via `vtbl+2620` `00891880`. | **PROVEN** |
| PE literal `Q_NewOakValeIntro` at a Give site? | **No.** Five xrefs: bind `00CD6E28`/`00CD6E87`, card `00CE791E`, wait `00CE7978`/`00CE79CA`. | **PROVEN** |
| Is `005E7B77` first-seen Oakvale Give? | **No.** Mode-1 leftover after `004B1D30` already posted `0x33` and `005E4740` copied the name into `+80`. Empty `+80` skips. | **LEFTOVER** |
| Does `0061ACB3` add a fourth Oakvale presenter? | **No.** It **is** `004B1D30`. Gated `[this+343]`. Name from `0061A8A0` table × `[this+344]` × 28. Zero PE Oakvale. Not first-seen. | **DISPROVEN** as other |
| Does construct / `ActivateQuest` post `0x33`? | **No.** `004B3CE0` live arm posts **`0x37`**. `004B4A10` only `004B4260`. | **DISPROVEN** |
| Invent `ActivateQuest` to satisfy `00893570`? | **No.** Wait needs type-`0x33` + name. Construct still `0x37`. | **DISPROVEN** |

**No fourth presenter. The set is `004B1D30` (body) /
`00DBE295` (Oakvale-name site) / `005E7B77` (UI leftover).**

---

## Verdict

Gameflow `00CE7670` state 0 polls `[esi+64].vtbl+100`
`00893570("Q_NewOakValeIntro")`. That walk asks
`008ABED0` for event kind **`0x33` (51)** on
`[world+96]` whose `[+60]` catalogue CString equals
the wait name. Construct posts **`0x37`** on the same
list and cannot hit.

The only `00687540` sites that push kind **51** are
`004B1DC4` (quest-manager Give body `004B1D30`) and
`005E7B77` (`CTCQuestCompletionUI` tick `005E78F0`,
mode `[this+32]==1`). `calls-by-dest.tsv` has
**two** `E8` of `004B1D30`: the `vtbl+1152` thunk
`00892F80` and a gated UI caller `0061ACB3`. That UI
is not a new writer and does not recover the Oakvale
name.

Script Give is `01260F0C` slot 288 `vtbl+1152`
`00892F80` → `004B1D30`. `ff.tsv` lists **79**
`call [reg+1152]` sites. **None** push
`0x012C5D14`. The name is almost always
`00891880` (`[QM+136]+48`) while the ticking slot
is current. `004B4490` sets `QM+136` only around
that slot’s `00CB8220`. The only `1152` inside
S_QNOVI `00DBDE40` / parent `00DB8680` is
**`00DBE295`**, and it runs **after** AttackOver
store `00DBB2A7`, PostAttack `00DBE3C0`, and Maze
`00DBEB20`.

`005E7B77` can re-post kind `0x33` for whatever
CString still sits at `CTCQuestCompletionUI+80`
(`0099E960` empty → skip). `004B1D30` already
called `005E4740`, which `0099EFB0`s the name
into that `+80`. Second Give, not first-seen,
not a New Game presenter.

On no-save New Game none of the three run.
`00893570` stays 0. Do not invent `ActivateQuest`.

---

## Evidence → Original → Host → Gap

| Evidence | Original | Host | Gap |
|---|---|---|---|
| `listing-00cc0000` `00CE7977`/`00CE79C9` `push "Q_NewOakValeIntro"` `call [edx+100]` / `[eax+100]` | `01260F0C` slot 25 = `00893570`. Invert miss → yield. Loop `jne 00CE79B0`. No Give. | `QuestIsActiveFn=00893570` `QuestIsActiveVtbl=100` | **MATCH** wait |
| `listing-00880000` `00893570` `mov [esp+12], 0x33` `008ABED0` then `004AF3C0` | Wait object is kind **51** on `[world+96]`. | `QuestGiveEventKind=0x33` ≠ `EventPostKind` | **MATCH** kind split |
| `listing-00480000` `004B1DC4` `push 51` `00687540` `[QM+124]+96` | Give body. Also `0x35` first; `0x33` if out-byte 0. | `QuestGiveFn=00892F80` `QuestGiveVtbl=1152` `QuestGiveBody=004B1D30` | **MATCH** constants |
| `calls-by-dest` dest `004B1D30`: `00892F9F`, `0061ACB3` | Thunk + gated UI. **No third `E8`.** | none on Pump | **MATCH** omit |
| `listing-005c0000` `005E7B77` `push 51` `jmp 005E7BD1` `00687540` | Mode 1, name `[esi+80]`, `0099E960` empty skip. | `QuestCompletionUiGiveFn=005E7B77` `QuestCompletionUiGiveIsFirstSeen=false` | **MATCH** leftover |
| `ff.tsv` `00DBE295` `call [edi+1152]` parent `00DB8680` | After `00DBE22F` PostAttack and `00DBE236` Maze. `vtbl+2620` copies `[QM+136]+48`. | `QuestGiveAfterAttackOver=00DBE295` `GiveAfterPostAttackAndMaze=true` | **MATCH** VA; unused on Type-1 |
| `xrefs.tsv` `0x012C5D14` five sites | Bind + card + wait. **Zero** Give / `004B1D30` / `1152` literal. | `OakvaleBindSite=00CD6E27` | **PROVEN** no literal Give |
| `004B2280` `push 54` / `push 52` | Sibling posts `0x36` / `0x34`, **not** `0x33`. | none | **DISPROVEN** as Give |
| Type-1: `EventPosts=10` all kind 55; `GameflowYieldQuest=Q_NewOakValeIntro` | Native miss stays miss. | same asserts | **MATCH** wait-forever |
| Invent `ActivateQuest("Q_NewOakValeIntro")` | would post `0x37`, still miss `0x33` | `ActivateQuestSatisfiesGameflowWait=false` | **MATCH** lock |

---

## 1. Wait is type-`0x33` Give, not construct

`listing-00cc0000.txt` `00CE7670` state 0:

```
00CE791D  push "Q_NewOakValeIntro"
          push "OBJECT_QUEST_CARD_OAKVALE_INTRO"
          call [edx+1180]             ; card; NOT Give
00CE7977  push "Q_NewOakValeIntro"
00CE7995  call [edx+100]              ; 00893570
          neg/sbb/inc
00CE79AE  je 00CE7A02                 ; already Given → skip wait
00CE79B0  call [edx+28]               ; yield
00CE79C9  push "Q_NewOakValeIntro"
00CE79E7  call [eax+100]
00CE7A00  jne 00CE79B0
```

No `call […+1152]`, no `004B1D30`, no `00CB5AD0` in
this waiter. **PROVEN** wait-only.

`00893570` (`listing-00880000`):

```
ecx = [iface+4]+96                    ; [world+96]
[key+0] = 0x33
call 008ABED0
hit: [payload+60] → 004AF3C0 → CString == arg
miss / name mismatch: al=0
```

`vtbl.tsv` `0x01260F0C` slot 25 = `00893570`. Sibling
GET `00893610` is slot 26 (`vtbl+104`): same type,
no name compare. Wait does **not** call it.

`004B3CE0` live arm posts `00687540(55, 50)` =
kind **`0x37`**. Factory-0 stub posts nothing.
**DISPROVEN** as this wait’s node.

---

## 2. Census: who can post kind `0x33`

### `00687540` first-arg `51`

Linear `push 51` (`6A 33`) in `.text`:

| Site | Next | Class |
|---|---|---|
| `004B1DC4` | `call 00687540` | **PROVEN** Give body |
| `005E7B77` | `jmp 005E7BD1` → `00687540` | **PROVEN** second writer |
| `004D7E7C` | `pop eax` / `ret` | **DISPROVEN** (GetType returns 51) |
| `008C24DC` / `008C24E9` | `0073A8A0` / `0073ABA0` | **DISPROVEN** (not `[world+96]`) |
| `00E1AC1C` | `call [edx+2792]` | **DISPROVEN** (other vtbl) |

No `68 33 00 00 00`. Kind at every `00687540` site
is an immediate `push` immediately before the call
or a `jmp` to it. `calls-by-dest` dest `00687540`
is 78 rows; the only kind-51 rows are the two
above.

`0089A3FD` (same `00892D80` family as the Give
thunks) pushes **70** (`0x46`). `004B2280`
(`vtbl` slot 289 `00892FD0`) pushes **54** then
**52**. Neither is Give.

`004AF740` (name → `QM+44` index) `E8`s:

```
004B1D41    004B1D30     Give 0x35 then 0x33
004B228D    004B2280     0x36 then 0x34
004B4025    004B3CE0     construct 0x37
005E7B66    005E78F0     mode 1 → 0x33
005E7BBC    005E78F0     mode 2 → 0x34
```

No sixth namer. **PROVEN** two `0x33` posters.

### `004B1D30` body

`listing-00480000`:

```
004AF740(name) → index
ecx = [QM+124]+96
00687540(53, 50, …, index)            ; 0x35
if out-byte == 0:
  00687540(51, 50, …, index)          ; 0x33  ← 004B1DC4
then 004B0160 / 004B0C80 / 005E4740
```

`00892F80` (`ret 16`): `ecx=[0x13B89FC]`,
`call 004B1D30` at `00892F9F`. Slot 288.

`calls-by-dest.tsv`:

```
0x004B1D30	0x0061ACB3	0x0061A6A0
0x004B1D30	0x00892F9F	0x00892D80
```

Two rows. Zero `E8` of `00892F80` (vtbl only).
**PROVEN** no third body.

---

## 3. Recovered Oakvale-name Give is only `00DBE295`

`00891880` (`listing-00880000`, `ret 4`):

```
eax = [0x13B89FC]+136
if eax: eax += 48; 0099EC30 into out CString
else: empty 0x122D70E
```

`004B4490` while walking `QM+56`:

```
004B4535  mov [QM+136], slot          ; 52-byte, name at +48
004B453E  call 00CB8220
004B4543  mov [QM+136], 0
```

So `vtbl+2620` + `vtbl+1152` Gives **the ticking
slot’s `+48`**. That CString is `Q_NewOakValeIntro`
only while S_QNOVI’s slot is `QM+136`.

`00DBDE40` (`listing-00d80000`), unique `E8`
`00DAC295` in `00DABAC0`:

```
map-wait vtbl+48 "StartOakVale"       ; 00DBDE49
READ / SPIN [this+80] AttackOver      ; writer 00DBB2A7
00DBE22F  call 00DBE3C0               ; PostAttack
00DBE236  call 00DBEB20               ; Maze (M_PostAttackStart)
00DBE28B  call [eax+2620]             ; 00891880
00DBE295  call [edi+1152]             ; Give that CString
```

`00DBE3C0` pushes `Q__OakValeIntro_PostAttack` and
`vtbl+1104` (ActivateQuest, construct `0x37` of a
**different** name). `00DBEB20` is
`RegionTravel.MazeCutsceneStart`. Give is **after**
both. **PROVEN** order.

`ff.tsv` disp `1152`: **79** sites, all script
fibers. The only row whose parent is `00DB8680`
(covers `00DBDE40`) is **`00DBE295`**. Next `1152`
is `00DC3C49` in `00DC2750`. `00DABAC0` /
`00DAAD80` have **zero** `1152`. PostAttack and
Maze use `1104` / `1120` / `1184` / `2584`, not
Give.

`xrefs-by-string.tsv` `Q_NewOakValeIntro`: five
sites, none a `1152` / `004B1D30`. Other `1152`
sites Give **their** slot name when **their**
`004B4490` runs. **DISPROVEN** as Oakvale.

---

## 4. `005E7B77` is leftover, not first-seen

`005E78F0` is `vtbl.tsv` `0x0125589C` slot 26.
RTTI `0x0137D340` `CTCQuestCompletionUI`.

`listing-005c0000` after `[esi+32]` switch:

```
005E7AE2  mov eax, [esi+32]
          je  mode 0 → 00687540(73)     ; 0x49
005E7B21  cmp eax, 1
          jne mode 2
005E7B26  lea ebp, [esi+80]
          0099E960(0x122D70E)           ; empty? skip
          004AF740(ebp)
005E7B77  push 51
          jmp 005E7BD1 → 00687540
mode 2:   same +80, push 52             ; 0x34
```

`004B1D30` already called `005E4740`:

```
005E478D  lea ebp, [esi+80]
005E4795  call 0099EFB0                 ; copy name into +80
```

So mode 1 is a **second** `0x33` of a name the
Give body already posted. First-seen `+80` is
empty → `0099E960` miss → no post.
`QuestCompletionUiGiveIsFirstSeen=false`.
**LEFTOVER.** Not a New Game / no-save presenter.

---

## 5. `0061ACB3` is the same body, not Oakvale

`0061AC60` (`listing-00600000`):

```
test [this+343]
je skip
0061A8A0 → table
esi = [this+344] * 28 + table
call 004B1D30(esi)
```

Unique `E8` of `0061AC60` is `0061B5FD` in
`0061B5F0`, same `[this+343]` gate, then
`jmp 005BC66F`. Nearby `0061B6B7` pushes
`"UI_SELECT_FOR_LIST"`. Name is a runtime table
slot, not `0x012C5D14`.

This is the **other `E8` of `004B1D30`**, already
counted in §2. It does not add a writer and does
not recover an Oakvale-name site. Not first-seen
Leave / Type-1. **DISPROVEN** as a fourth
presenter.

---

## 6. Rejected `0x33` lookalikes

| Claim | Class |
|---|---|
| `004B3CE0` / `00CB5AD0` / `004B4A10` / `vtbl+1104` posts `0x33` | **DISPROVEN** (`0x37` or lookup) |
| `004B2280` / slot 289 is Give | **DISPROVEN** (`0x36`/`0x34`) |
| `0089A3FD` is Give | **DISPROVEN** (kind 70) |
| `004D7E7C` / `008C24DC` / `00E1AC1C` insert on `[world+96]` | **DISPROVEN** |
| Card `vtbl+1180` / tattoo `008902E0` is Give | **DISPROVEN** |
| Other 78 `1152` sites Give `Q_NewOakValeIntro` | **DISPROVEN** (other slot `+48`; no PE name) |
| `00DBE3C0` / `00DBEB20` Give Oakvale | **DISPROVEN** (Activate / Maze; Give is after) |
| `QM+44` / `004AF610` satisfies `00893570` | **DISPROVEN** |
| `0061ACB3` is a distinct Oakvale presenter | **DISPROVEN** (same `004B1D30`) |
| Invent `ActivateQuest("Q_NewOakValeIntro")` | **DISPROVEN** |

---

## Host

`EngineLifecycleTests.Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`

| Host | Native | Class |
|---|---|---|
| `QuestIsActiveFn=00893570` / vtbl `100` | wait slot 25 | **MATCH** |
| `QuestGiveFn=00892F80` / vtbl `1152` / body `004B1D30` / kind `0x33` | Give writer | **MATCH** |
| `QuestGiveAfterAttackOver=00DBE295` | only Oakvale-name site | **MATCH** VA; **PROVEN** unused on this test |
| `GiveAfterPostAttackAndMaze=true` | `00DBE22F` / `00DBE236` then `00DBE295` | **MATCH** |
| `QuestCompletionUiGiveFn=005E7B77` first-seen `false` | leftover mode 1 | **MATCH** |
| `ActivateQuestSatisfiesGameflowWait=false` | construct is `0x37` | **MATCH** lock |
| `GameflowWaitsForeverOnNoSave=true` | no `0x33` on this walk | **MATCH** |
| `EventPosts=10` all kind 55 | WLD TRUE + Gameflow constructs | **MATCH** `0x37` only |

Keep the hardcoded `00893570` miss. Do not post
`0x33`. Do not invent `ActivateQuest`.

---

## Classifications (short)

1. **Other presenter — PROVEN none.** The set is
   `004B1D30` (body) / `00DBE295` (Oakvale-name
   `vtbl+1152`) / `005E7B77` (`CTCQuestCompletionUI`
   leftover). No fourth `00687540(51)`, no third
   `E8` of `004B1D30`, no other `1152` that
   recovers `Q_NewOakValeIntro`.
2. **Oakvale-name Give — PROVEN `00DBE295` after
   AttackOver `00DBB2A7` and PostAttack `00DBE3C0`
   and Maze `00DBEB20`.** Name from `[QM+136]+48`
   while S_QNOVI ticks. Zero PE literal at Give.
3. **`005E7B77` — PROVEN leftover, not first-seen.**
   Mode 1 re-posts `0x33` from `+80` after
   `004B1D30` → `005E4740` already copied the name.
4. **`ActivateQuest` / construct — DISPROVEN as
   this wait.** Kind `0x37`. Lock
   `ActivateQuestSatisfiesGameflowWait=false`.
