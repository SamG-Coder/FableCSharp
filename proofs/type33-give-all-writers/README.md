# Who writes kind `0x33` (51) onto `[world+96]` besides `00DBE295`

Investigation only. No production `src/` or `tests/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** treat construct `004B3CE0` / kind `0x37` as
the Gameflow wait object. Do **not** collapse Give
(`0x33`) with construct (`0x37`).
`ActivateQuestSatisfiesGameflowWait=false` stays
**locked**.

Question: besides script Give `00DBE295`
(`vtbl+1152` of `Q_NewOakValeIntro` after AttackOver),
who posts kind **51** onto `[world+96]`?
Enumerate **every** `E8` of `004B1D30` and every
`push 51` then `jmp`/`call` `00687540`. Classify
first-seen vs leftover vs post-AttackOver.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: ExeIndex
`assembly/exe/01-sections/text-map/e8.tsv`
(`004B1D30` / `00687540` / `0061AC60` / `004B3CE0` /
`00892F80`),
`calls-by-dest.tsv` dest `004B1D30` / `00687540` /
`0061AC60`,
`ff.tsv` disp `1152` / `2620`,
`listing-00480000.txt`
(`004B1D30` / `004B1DC4` / `004B3CE0` / `004B4040` /
`004B4490` / `004B2280`),
`listing-005c0000.txt`
(`005E78F0` / `005E7B77` / `005E4740`),
`listing-00600000.txt`
(`0061A420` / `0061A8A0` / `0061AC60` / `0061ACB3` /
`0061B5F0`),
`listing-00680000.txt` (`00686A70` / `00687540`),
`listing-00880000.txt`
(`00892F80` / `00893570` / `00891880` / `008ABED0` /
`00892FD0`),
`listing-00d80000.txt`
(`00DBB2A7` / `00DBDE40` / `00DBE22F` / `00DBE236` /
`00DBE28B` / `00DBE295` / `00DBE3C0` / `00DBEB20`),
`listing-004c0000.txt` (`004D7E7C` / `004D30BD`),
`listing-008c0000.txt` (`008C24DC`),
`listing-00e00000.txt` (`00E1AC1C`),
`vtbl.tsv` `0x01260F0C` slots 25 / 288 / 289 / 655,
`0x0125589C` slot 26, `0x012585B4` slot 59,
`rtti.txt` `0x0137D340` `CTCQuestCompletionUI`,
`xrefs.tsv` `0x012C5D14`;
siblings `proofs/gameflow-type33-give`,
`proofs/00893570-give-presenters`,
`proofs/00DBB2A7-attackover-store`,
`proofs/00DAAC00-sqnovi-no-save`,
`proofs/addtestquest-token`;
host
`EngineLifecycleTests.Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`
(`ActivateQuestSatisfiesGameflowWait=false`,
`QuestGiveAfterAttackOver=00DBE295`,
`GiveAfterPostAttackAndMaze=true`,
`QuestCompletionUiGiveIsFirstSeen=false`).

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Who posts kind `0x33` onto `[world+96]` besides `00DBE295`? | Two `00687540(51)` sites: **`004B1DC4`** inside Give body `004B1D30`, and **`005E7B77`** leftover `CTCQuestCompletionUI` mode 1. `00DBE295` is not a third poster: it is one `vtbl+1152` presenter of the **same** body. | **PROVEN** |
| Every `E8` of `004B1D30`? | **Two** rows: `00892F9F` (`00892F80` thunk, vtbl slot 288) and `0061ACB3` (`CTCInventoryQuests` / `PC_QUESTS_SELECTION_MENU`, `[this+343]`). Zero third. Zero `E8` of `00892F80`. | **PROVEN** |
| Every `push 51` then `jmp`/`call` `00687540`? | **Two.** `004B1DC4` `call 00687540`. `005E7B77` `jmp 005E7BCF` → `005E7BD1` `call 00687540`. Zero `jmp 00687540`. | **PROVEN** |
| Linear `push 51` (`6A 33`) in `.text`? | **Six.** Only the two above feed `00687540`. `004D7E7C` / `008C24DC` / `008C24E9` / `00E1AC1C` are other. No `68 33 00 00 00`. | **PROVEN** |
| `0061ACB3` unread? | **No.** Same body as the thunk. Leftover quest-selection UI. Unique caller `0061B5FD`. Not first-seen. Not a fourth `00687540(51)` site. | **LEFTOVER**; **DISPROVEN** as unread / first-seen |
| Any writer on no-save first Present? | **No.** `008ABED0` type `0x33` = 0. `EventPosts` are kind `55`. `00DBE295` / `004B1D30` / `005E7B77` / `0061ACB3` unreached. | **PROVEN** omit |
| Does construct `004B3CE0` of `Q_NewOakValeIntro` (`0x37`) also Give `0x33`? | **No.** Live arm posts `00687540(55, 50)` only. Unique `E8` `004B4386`. No `004B1D30`, no `1152`, no `push 51`. | **DISPROVEN** |
| Invent `ActivateQuest` to post `0x33`? | **No.** Construct still `0x37`. Wait needs type-`0x33` + name. | **DISPROVEN** |

---

## Verdict

`00893570` (`01260F0C` slot 25, `vtbl+100`) asks
`008ABED0` for event kind **`0x33` (51)** on
`[world+96]` whose `[+60]` catalogue CString equals
the wait name. Construct posts **`0x37`** on the same
list and cannot hit.

The only `00687540` sites whose first argument is
immediate **51** are `004B1DC4` (Give body `004B1D30`)
and `005E7B77` (`CTCQuestCompletionUI` tick
`005E78F0`, mode `[this+32]==1`). `calls-by-dest.tsv`
has **78** `E8` of `00687540` and **0** `jmp`. Kind at
every site is an immediate `push` immediately before
the call, or a `jmp` onto that call. Walked all 78;
only those two push 51.

`004B1D30` has **two** `E8`s. `00892F9F` is the
`vtbl+1152` thunk `00892F80` (`ret 16`). `ff.tsv`
lists **79** `call [reg+1152]` sites. **None** push
`0x012C5D14`. The name is `00891880` (`[QM+136]+48`)
while that slot ticks. The only `1152` inside
S_QNOVI `00DBDE40` / parent `00DB8680` is
**`00DBE295`**, and it runs **after** AttackOver
store `00DBB2A7`, PostAttack `00DBE3C0`, and Maze
`00DBEB20`. That is the Oakvale-**name** Give.
It is not a second `00687540(51)` writer.

The other `E8`, `0061ACB3`, **is** `004B1D30`. It is
`CTCInventoryQuests` (`004D30BD` factory → ctor
`0061A420`, vtbl `0x012585B4` slot 59 `0061B5F0`).
Name is `world+196` AddTestQuest × `[this+344]` × 28.
Gated `[this+343]`. Leftover `PC_QUESTS_SELECTION_MENU`.
Not no-save first Present. **Not UNREAD.**

`005E7B77` can re-post kind `0x33` for whatever
CString still sits at `CTCQuestCompletionUI+80`
(`0099E960` empty → skip). `004B1D30` already
called `005E4740`, which `0099EFB0`s the name
into that `+80`. Second Give, not first-seen.

On no-save New Game none of these run.
`00893570` stays 0. Constructing `Q_NewOakValeIntro`
would still post **`0x37` only**. Do not invent
`ActivateQuest`.

---

## Evidence → Original → Host → Gap

| Evidence | Original | Host | Gap |
|---|---|---|---|
| `e8.tsv` dest `004B1D30`: `00892F9F`, `0061ACB3` | Two `E8`. No third. | `QuestGiveBody=004B1D30` | **MATCH** census |
| `e8.tsv` dest `00892F80`: **0** | Thunk is vtbl-only. | `QuestGiveFn=00892F80` `QuestGiveVtbl=1152` | **MATCH** |
| `listing-00480000` `004B1DC4` `push 51` `00687540` `[QM+124]+96` | Give body. Also `0x35` first; `0x33` if out-byte 0. | `QuestGiveEventKind=0x33` | **MATCH** constants |
| `listing-005c0000` `005E7B77` `push 51` `jmp 005E7BCF` `00687540` | Mode 1, name `[esi+80]`, `0099E960` empty skip. | `QuestCompletionUiGiveFn=005E7B77` `QuestCompletionUiGiveIsFirstSeen=false` | **MATCH** leftover |
| `ff.tsv` `00DBE295` `call [edi+1152]` parent `00DB8680` | After `00DBE22F` PostAttack and `00DBE236` Maze. `vtbl+2620` copies `[QM+136]+48`. | `QuestGiveAfterAttackOver=00DBE295` `GiveAfterPostAttackAndMaze=true` | **MATCH** VA; unused on Type-1 |
| `listing-00480000` `004B4040` `push 55` `00687540` | Construct posts **`0x37`**. Unique `E8` `004B4386`. | `EventPostKind=55` `QuestConstructEventKind=0x37` | **MATCH** `0x37`. **DISPROVEN** as Give |
| `xrefs.tsv` `0x012C5D14` five sites | Bind + card + wait. **Zero** Give / `004B1D30` / `1152` literal. | `OakvaleBindSite=00CD6E27` | **PROVEN** no literal Give |
| Type-1: `EventPosts=10` all kind 55; `008ABED0` type `0x33` = 0 | Native miss stays miss. | `GameflowWaitsForeverOnNoSave=true` | **MATCH** wait-forever |
| Invent `ActivateQuest("Q_NewOakValeIntro")` | would post `0x37`, still miss `0x33` | `ActivateQuestSatisfiesGameflowWait=false` | **MATCH** lock |

---

## 1. Census: `E8` of `004B1D30`

`e8.tsv` dest `0x004B1D30` — **exactly two** rows:

```
0x0061ACB3	0x004B1D30
0x00892F9F	0x004B1D30
```

`calls-by-dest.tsv`:

```
0x004B1D30	0x0061ACB3	0x0061A6A0
0x004B1D30	0x00892F9F	0x00892D80
```

`e8.tsv` dest `0x00892F80`: **zero**. Slot 288 is
vtbl-only. `ff.tsv` disp `1152`: **79** script fibers
`call [reg+1152]`.

### Thunk `00892F80` (`listing-00880000`, `ret 16`)

```
00892F80  mov eax, [esp+16]
          …
          mov ecx, [0x13B89FC]     ; QM
00892F9F  call 004B1D30
          …
          ret 16
```

`vtbl.tsv` `0x01260F0C` slot **288** = `00892F80`.
Sibling slot 289 `00892FD0` calls `004B2280`
(`push 54` then `push 52`) — **not** Give.

### UI `0061ACB3` (`listing-00600000`)

```
0061AC60  test [this+343]
          je skip
          0061A8A0 → copy world+196
          esi = [this+344] * 28 + table
0061ACB3  call 004B1D30(esi)       ; same body
```

Unique `E8` of `0061AC60` is `0061B5FD` in `0061B5F0`
(same `[this+343]` gate, then `jmp 005BC66F`).
`vtbl.tsv` `0x012585B4` slot 59 = `0061B5F0`.
Ctor `0061A420` (`mov [esi], 0x12585B4`) is the
unique `E8` `004D30BD`; name getter next door is
`"CTCInventoryQuests"`. Sibling ctor `006224C0`
pushes `PC_QUESTS_SELECTION_MENU`.

This is **not** a second `00687540(51)` site and
**not** UNREAD: it is the leftover test-quest menu
Give (`proofs/addtestquest-token`). Default
`[this+343]=1` / `[this+344]=0` / `[this+352]=1`
in the ctor does not matter on no-save: the widget
is not ticked. Type-1 `008ABED0` type `0x33` = 0.

---

## 2. Census: `push 51` then `00687540`

Linear `6A 33` (`push 51`) in `.text`:

| Site | Next | Feeds `00687540`? | Class |
|---|---|---|---|
| `004B1DC4` | `call 00687540` | **yes** | **PROVEN** Give body |
| `005E7B77` | `jmp 005E7BCF` → `mov ecx, edi` / `call 00687540` | **yes** | **PROVEN** leftover |
| `004D7E7C` | `pop eax` / `ret` | no | **DISPROVEN** (GetType returns 51) |
| `008C24DC` / `008C24E9` | `0073A8A0` / `0073ABA0` | no | **DISPROVEN** |
| `00E1AC1C` | `call [edx+2792]` | no | **DISPROVEN** (other vtbl) |

No `68 33 00 00 00`. The only `C7 … 33 00 00 00` hit
is `007CB50E` `mov [esp+104], 0x33` — not a kind push.

`calls-by-dest.tsv` dest `00687540`: **78** `E8`.
`jmp 00687540`: **0**. Kind is always an immediate
`push` immediately before the call (sometimes with
`mov ecx` / stack zeros between delay and call).
Walked every row. Kinds that are **not** 51 include
9, 12, 16, 17, 18, 19, 20, 24, 27, 28, 30, 31, 35,
36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48,
49, **50**, **52**, **53**, **54**, **55**, 56, 57,
58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70,
71, 72, 73, 74, 75. The **51** rows are only
`004B1DC6` and `005E7BD1` (the latter shared with
mode 0 kind 73 and mode 2 kind 52).

`004AF740` (name → `QM+44` index) `E8`s at the two
writers plus siblings:

```
004B1D41    004B1D30     Give 0x35 then 0x33
004B228D    004B2280     0x36 then 0x34
004B4025    004B3CE0     construct 0x37
005E7B66    005E78F0     mode 1 → 0x33
005E7BBC    005E78F0     mode 2 → 0x34
```

No sixth namer. **PROVEN** two `0x33` posters.

---

## 3. Body `004B1D30` — first-seen? leftover? post-AttackOver?

`listing-00480000`, int3-bounded:

```
004AF740(name) → index
ecx = [QM+124]+96                  ; [world+96]
00687540(53, 50, …, index)         ; 0x35  ← 004B1D89
if out-byte == 0:
  00687540(51, 50, …, index)       ; 0x33  ← 004B1DC4
then 005E4740 / 004B0160 / 004B0C80
```

`005E4740` (`listing-005c0000`):

```
005E478D  lea ebp, [esi+80]
005E4795  call 0099EFB0            ; copy name into CTCQuestCompletionUI+80
```

So the body both posts `0x33` **and** arms the leftover
UI for a second post.

Classification of **presenters** of this body:

| Presenter | When | Class |
|---|---|---|
| `00DBE295` `vtbl+1152` while S_QNOVI is `QM+136` | After AttackOver `00DBB2A7` **and** PostAttack `00DBE3C0` **and** Maze `00DBEB20` | **post-AttackOver** Oakvale-name Give. **PROVEN** site. **DISPROVEN** as no-save |
| Other 78 `ff.tsv` `1152` sites | Their slot `+48` while **their** `004B4490` ticks | **DISPROVEN** as Oakvale (no PE name; other CString) |
| `0061ACB3` | Leftover `CTCInventoryQuests` / `PC_QUESTS_SELECTION_MENU` | **LEFTOVER**. **DISPROVEN** as first-seen |

`004B4490` while walking `QM+56`:

```
004B4535  mov [QM+136], slot       ; 52-byte, name at +48
004B453E  call 00CB8220
004B4543  mov [QM+136], 0
```

`00891880` (`vtbl.tsv` slot 655 = `vtbl+2620`, `ret 4`):

```
eax = [0x13B89FC]+136
if eax: eax += 48; 0099EC30 into out CString
else: empty 0x122D70E
```

`00DBDE40` (`listing-00d80000`), unique `E8`
`00DAC295` in `00DABAC0`:

```
map-wait vtbl+48 "StartOakVale"
SPIN [this+80] AttackOver          ; writer 00DBB2A7
00DBE22F  call 00DBE3C0            ; PostAttack; vtbl+1104 "Q__OakValeIntro_PostAttack"
00DBE236  call 00DBEB20            ; Maze "M_PostAttackStart"
00DBE28B  call [eax+2620]          ; 00891880
00DBE295  call [edi+1152]          ; Give that CString
```

Give is **after** both. Name is the ticking slot, not
a PE `Q_NewOakValeIntro` push. **PROVEN** order.

---

## 4. Leftover `005E7B77` — not first-seen

`005E78F0` is `vtbl.tsv` `0x0125589C` slot 26.
RTTI `0x0137D340` `CTCQuestCompletionUI`.
Factory name at `004D6310`.

`listing-005c0000` after `[esi+32]` switch:

```
005E7AE2  mov eax, [esi+32]
          je  mode 0 → 00687540(73)     ; 0x49
005E7B21  cmp eax, 1
          jne mode 2
005E7B26  lea ebp, [esi+80]
          0099E960(0x122D70E)           ; empty? skip
          004AF740(ebp)
005E7B57  call 00686A70                 ; [0x13B8A1C]+36 = world
          ecx = [eax+96]
005E7B77  push 51
          jmp 005E7BCF → 00687540
mode 2:   same +80, push 52             ; 0x34
```

`00686A70` (`listing-00680000`): `mov eax, [0x13B8A1C];
mov eax, [eax+36]; ret`. Same `[world+96]` list as
`[QM+124]+96`.

First-seen `+80` is empty → `0099E960` miss → no post.
Mode 1 is a **second** `0x33` of a name the Give body
already posted. `QuestCompletionUiGiveIsFirstSeen=false`.
**LEFTOVER.** Not a New Game / no-save presenter.

---

## 5. No-save first Present: nobody writes `0x33`

```
00CD6E27  00CB5C90 bind Q_NewOakValeIntro     // BIND, not Give
0049F24E  004B4260([world+172])               // NOT Oakvale
004B42E8  00CB5AD0                            // unique; other names
004B3CE0  00687540(55,50)                     // kind 0x37
user.ini  ActivateQuest("Gameflow")           // 0x37 of Gameflow
type-1 00CB8220
  00CE7670  00893570("Q_NewOakValeIntro")
            [world+96] 008ABED0 type 0x33 → 0
            006E7410 yield
```

`00DABAC0` / `00DBDE40` / `00DBE295` / `004B1D30` /
`005E7B77` / `0061ACB3` are **not** on this list.
`S_QNOVI` is not constructed (`proofs/00DAAC00-sqnovi-no-save`).
`008ABED0` type `0x33` stays 0. Host Type-1
`EventPosts=10` all kind 55. **PROVEN** omit.

---

## 6. Construct `0x37` never also Gives `0x33`

`004B3CE0` live arm (`listing-00480000`):

```
004B400A  …
004B4025  call 004AF740            ; name → index
          ecx = [QM+124]+96
004B403E  push 50
004B4040  push 55                  ; 0x37
004B4042  call 00687540
```

Unique `e8.tsv` dest `004B3CE0`: **`004B4386`** inside
`004B4260`. Factory-0 stub posts **nothing**.
No `call 004B1D30`, no `push 51`, no `call […+1152]`
in this function. `00CB7900` starts the fiber; Give
is a later `1152` if that fiber ever reaches it.

Even a later proven `004B4260` of `Q_NewOakValeIntro`
would post **`0x37` at construct** and still need
`00DBE295` (or another Give of the same CString)
to satisfy Gameflow. Inventing `ActivateQuest` would
take this arm and still miss. **DISPROVEN.**

---

## 7. Rejected lookalikes

| Claim | Class |
|---|---|
| `00DBE295` is a distinct `00687540(51)` writer | **DISPROVEN** (it **is** `004B1D30` via `1152`) |
| Third `E8` of `004B1D30` | **DISPROVEN** (census = 2) |
| `0061ACB3` unread | **DISPROVEN** (site + leftover class known) |
| `0061ACB3` first-seen / no-save Present | **DISPROVEN** / **LEFTOVER** |
| `005E7B77` first-seen Oakvale Give | **LEFTOVER** |
| `004B3CE0` / `00CB5AD0` / `004B4A10` / `vtbl+1104` posts `0x33` | **DISPROVEN** (`0x37` or lookup) |
| `004B2280` / slot 289 is Give | **DISPROVEN** (`0x36`/`0x34`) |
| `0089A3FD` is Give | **DISPROVEN** (kind 70) |
| `004D7E7C` / `008C24DC` / `00E1AC1C` insert on `[world+96]` | **DISPROVEN** |
| Other 78 `1152` sites Give `Q_NewOakValeIntro` | **DISPROVEN** (other slot `+48`; no PE name) |
| `00DBE3C0` / `00DBEB20` Give Oakvale | **DISPROVEN** (Activate / Maze; Give is after) |
| `QM+44` / `004AF610` satisfies `00893570` | **DISPROVEN** |
| No-save first Present posts `0x33` | **DISPROVEN** |
| Invent `ActivateQuest("Q_NewOakValeIntro")` | **DISPROVEN** |

---

## Host

`EngineLifecycleTests.Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`

| Host | Native | Class |
|---|---|---|
| `QuestIsActiveFn=00893570` / vtbl `100` | wait slot 25 | **MATCH** |
| `QuestGiveFn=00892F80` / vtbl `1152` / body `004B1D30` / kind `0x33` | Give writer | **MATCH** |
| `QuestGiveAfterAttackOver=00DBE295` | only Oakvale-name `1152` | **MATCH** VA; **PROVEN** unused on this test |
| `GiveAfterPostAttackAndMaze=true` | `00DBE22F` / `00DBE236` then `00DBE295` | **MATCH** |
| `QuestCompletionUiGiveFn=005E7B77` first-seen `false` | leftover mode 1 | **MATCH** |
| `ActivateQuestSatisfiesGameflowWait=false` | construct is `0x37` | **MATCH** lock |
| `GameflowWaitsForeverOnNoSave=true` | no `0x33` on this walk | **MATCH** |
| `EventPosts=10` all kind 55 | WLD TRUE + Gameflow constructs | **MATCH** `0x37` only |
| `0061ACB3` / `0061B5F0` | leftover UI; none on Pump | **MATCH** omit |

Keep the hardcoded `00893570` miss. Do not post
`0x33`. Do not invent `ActivateQuest`.

---

## Classifications (short)

1. **Writers of kind `0x33` onto `[world+96]` — PROVEN two
   `00687540(51)` sites.** `004B1DC4` inside `004B1D30`,
   and leftover `005E7B77`. `00DBE295` is a presenter of
   the first body, not a third poster.
2. **`E8` of `004B1D30` — PROVEN two.** Thunk `00892F9F`
   (`vtbl+1152`, 79 fibers; Oakvale-name is `00DBE295`
   **post-AttackOver**) and leftover `0061ACB3`
   (`CTCInventoryQuests`). `0061ACB3` is **LEFTOVER**,
   **DISPROVEN** as unread / first-seen.
3. **No-save first Present — PROVEN nobody.** Type `0x33`
   on `[world+96]` stays 0. Construct posts `0x37` only.
4. **Construct `Q_NewOakValeIntro` (`0x37`) also Give
   `0x33` — DISPROVEN.** `004B3CE0` has no Give. Lock
   `ActivateQuestSatisfiesGameflowWait=false`.

---

## Sources (absolute)

- `C:\FableCSharp\assembly\exe\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\calls-by-dest.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\ff.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-005c0000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00600000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00680000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00880000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-008c0000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00d80000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00e00000.txt`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\assembly\exe\00-index\rtti.txt`
- `C:\FableCSharp\assembly\exe\00-index\xrefs.tsv`
- `C:\FableCSharp\proofs\gameflow-type33-give\README.md`
- `C:\FableCSharp\proofs\00893570-give-presenters\README.md`
- `C:\FableCSharp\proofs\00DBB2A7-attackover-store\README.md`
- `C:\FableCSharp\proofs\00DAAC00-sqnovi-no-save\README.md`
- `C:\FableCSharp\proofs\addtestquest-token\README.md`
