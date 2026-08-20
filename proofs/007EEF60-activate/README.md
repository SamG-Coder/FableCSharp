# `007EF3A1` `004B4A10` is `CTCExpression` tick, not `Q_NewOakValeIntro`

Investigation only. No production `src/` edits.

Question: functions.tsv groups site `007EF3A1` under start
`007EEF60`. Is that a `Q_NewOakValeIntro` activator? What
name pointer and flags are pushed into `004B4A10`? Who
calls `007EEF60`?

Do **not** start at `00DBDE40` / `S_QNOVI` /
`Q_NewOakValeIntro`. This site does not hardcode that
name.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER**.

Authority: Fable.exe via ExeIndex
`assembly/exe/01-sections/text-map/listing-007c0000.txt`
(`007EEF60` / `007EF070` / `007EF200`–`007EF4DB`);
`listing-00480000.txt` (`004B4A10` `ret 12` / `004B4260`);
`listing-004c0000.txt` (`004DB050` / `004DB06C` /
`004DB072` / `004DB085` / `004D4B72`);
`listing-00880000.txt` (`00892E80` / `00892EC0`);
`calls-by-dest.tsv` dest `0x007EEF60` / `0x004B4A10`;
`e8.tsv` dest `0x007EEF60` / `0x007EF200`;
`vtbl.tsv` `0x0127185C` / `0x012401F4` / `0x0124026C`;
`rtti.txt` `CSmashableDef` `0x01376228` /
`CExpressionDef` `0x01376DCC` / `CTCExpression`
`0x0137A424`;
`strings.tsv` `"ExpressionDef"` `0x012718D0` /
`"CTCExpression"` `0x0123C2D0`;
sibling `proofs/ini-activate-quest`,
`proofs/addtestquest-token`,
`proofs/factory0-enqueue`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| Is `007EEF60` the `004B4A10` function? | **No.** `007EEF60` is 27 bytes, `ret 4`. Site `007EF3A1` is in **`007EF200`**. functions.tsv over-merged 2150 bytes | **PROVEN** |
| Name of `007EEF60`? | **`CSmashableDef` deleting dtor.** vtbl `0x0127185C` slot 0. RTTI `0x01376228` | **PROVEN** |
| Name of `007EF200` / site `007EF3A1`? | **`CTCExpression` vtbl+28** (`0x0124026C` slot 28). Def vtbl `0x012401F4` slot 21 returns type **`0x8F`**; slot 22 copies `"CTCExpression"` | **PROVEN** |
| Args to `004B4A10` at `007EF3A1`? | `ecx=[0x13B89FC]`; **arg1** `&local` copy of **`[CExpressionDef+120]`**; **arg2 `0`**; **arg3** zero-extended **`[CExpressionDef+124]`** | **PROVEN** |
| Hardcoded `Q_NewOakValeIntro` pointer? | **No.** Empty intern `0x122D70E` is the skip compare, not the name | **DISPROVEN** |
| Who `E8`s `007EEF60`? | **Nobody.** `calls-by-dest.tsv` dest `0x007EEF60`: **0 rows**. `e8.tsv`: **0** | **PROVEN** none |
| Debug / save / script opcode / childhood? | **None of those.** Thing-component tick. Script opcode is `00892E80`. Save is not this. Debug picker is `004B49E0`. Childhood is not this | **DISPROVEN** as those four |
| First no-save Oakvale activate? | **No.** Empty `+120` skips. Name is def-driven, not `Q_NewOakValeIntro` | **DISPROVEN** |

---

## 1. `007EEF60` is not the activate body

`listing-007c0000.txt`:

```
007EEF5F  int3
007EEF60  push esi
007EEF61  mov esi, ecx
007EEF63  call 004DB004
007EEF68  test [esp+8], 0x01
007EEF6D  je 007EEF78
007EEF6F  push esi
007EEF70  call 00BFE9BC
007EEF75  add esp, 4
007EEF78  mov eax, esi
007EEF7A  pop esi
007EEF7B  ret 4
007EEF7E  int3
```

Classic deleting destructor (`004DB004` then optional
`00BFE9BC`). **PROVEN.**

`vtbl.tsv`:

```
0x0127185C  slot 0   0x007EEF60
0x0127185C  slot 6   0x007EECF0    ; functions.tsv names CSmashableDef
```

`rtti.txt` `0x01376228` `CSmashableDef`. **PROVEN.**

functions.tsv start `0x007EEF60` size **2150** names
`ExpressionDef|CDecapitationDef|CTCRandomAppearanceMorph|…`.
That blob swallows later int3-separated functions, including
`007EF070` and `007EF200`. The size is a start-detector miss,
not one C++ function. **PROVEN** over-merge.

---

## 2. Site `007EF3A1` is `CTCExpression` `vtbl+28`

Real start (int3 before it, `ret 4` at `007EF4DB`):

```
007EF1FF  int3
007EF200  sub esp, 0x100
007EF206  mov eax, [0x139C8A8]
007EF20B  push ebx
007EF20C  push esi
007EF20D  push edi
007EF20E  mov edi, [esp+272]     ; arg1 = Thing
007EF215  mov ebx, ecx           ; this = CTCExpression
```

`vtbl.tsv`:

```
0x0124026C  slot 28  0x007EF200
```

Ctor `004DB085` (`listing-004c0000.txt`):

```
004DB085  push esi
004DB08C  call 004DAC61
004DB091  mov [esi], 0x124026C   ; vtbl CTCExpression
004DB09A  ret 4
```

Sibling def vtbl `0x012401F4` (`004DB05C`):

```
004DB06C  mov eax, 0x8F          ; slot 21 type id
004DB071  ret
004DB072  … call 004D4B72        ; slot 22
004D4B75  push "CTCExpression"
```

`rtti.txt` `0x0137A424` `CTCExpression`. Slot 1 of the def
vtbl is `007EF070`, which `004109A0`s `"ExpressionDef"`
(`0x012718D0`). **PROVEN.**

`e8.tsv` dest `0x007EF200`: **0 rows.** Dispatch is
`[vtbl+28]`, not a named `E8`. **PROVEN.**

---

## 3. Pushes before `004B4A10` at `007EF3A1`

After `esi = [[CTCExpression+12]]` (`CExpressionDef*`) and
`[esi+116]==0`:

```
007EF36B  lea ebx, [esi+120]
007EF36E  push 0x122D70E          ; empty intern
007EF373  mov ecx, ebx
007EF375  call 005FA740           ; CString == intern?
007EF37A  test al, al
007EF37C  je 007EF422             ; equal → skip activate
007EF382  lea edx, [esp+16]
007EF386  push edx
007EF387  mov ecx, ebx
007EF389  call 00415DD0           ; copy CString +120 → local
007EF38E  xor eax, eax
007EF390  mov al, [esi+124]
007EF393  lea ecx, [esp+16]
007EF397  push eax                ; arg3 = byte [def+124]
007EF398  push 0                  ; arg2 = 0
007EF39A  push ecx                ; arg1 = &local name
007EF39B  mov ecx, [0x13B89FC]    ; QuestManager
007EF3A1  call 004B4A10
007EF3A6  … call 0099EAE0         ; destroy local
```

`004B4A10` ends `ret 12` (`listing-00480000.txt`
`004B4A96`). Three stack args. **PROVEN.**

`005FA740`: intern compare; equal (empty==empty) returns
`al=0` and the `je` skips. Non-empty returns `al=1` and
takes `004B4A10`. `0x122D70E` is the **empty intern**,
same skip used at `00416BF6` `"Activate Initial Quests"`.
**PROVEN.**

`00415DD0` copies `[ecx]` through intern table `0x13CA828`
into the out `CString`. Name is **`[CExpressionDef+120]`**,
not an immediate quest string. **PROVEN.**

`004B4A10` itself (`004B4A16`–`004B4A5A`):

1. `00433530` (`ret 20`) builds a 1-name vector from arg1.
2. Forwards **caller arg2 / arg3** into `004B4260`
   (`ret 12`).

So this site is:

```
004B4A10(QM, &name[+120], 0, [def+124])
  → 004B4260(vector{name}, 0, [def+124])
```

**PROVEN** forwarding. Compare other `004B4A10` sites
(`calls-by-dest.tsv` dest `0x004B4A10`, 8 rows):

| Site | Parent | Pushes (arg1, arg2, arg3) | Class |
|---|---|---|---|
| `00416C11` | `00416953` LoadWorld | `+90584`, **0**, **1** | `"Activate Initial Quests"`; empty intern skips |
| `00892E8F` | `00892D80` | name, **1**, **1** | **script / ini** `ActivateQuest` |
| `00892ECF` | `00892D80` | name, **1**, **0** | script variant |
| `004B4B5F` / `004B4D45` | `004B49E0` | picker record | **debug picker** |
| `0061AC28` | `0061A6A0` | test-card / menu | **`PC_QUESTS_SELECTION_MENU` leftover** |
| `0084407E` | `008421C0` | `&[obj+168]`, **0**, `[obj+172]` | `CTC*ActivateQuest` action |
| **`007EF3A1`** | **`007EF200`** | **`&[+120]`, `0`, `[+124]`** | **`CTCExpression` tick** |

**DISPROVEN** as debug (`004B49E0`), save (`004B5080`
`START_NEW_QUEST` is a different VA; 0 external `E8` of
that as this parent), script opcode (`00892E80`), or
childhood (`Q_NewOakValeIntro` / `S_QNOVI` / `00DBDE40`).

---

## 4. Who calls `007EEF60`

`calls-by-dest.tsv` format is `dest  site  containing_fn`.
Grep first column `0x007EEF60`: **no rows**.

`e8.tsv` dest `0x007EEF60`: **no rows**.

Callers of `007EEF60` are **vtbl slot 0** of
`CSmashableDef` (`0x0127185C`), i.e. destroy/free of that
def, not quest activate. **PROVEN** none-`E8`.

`calls-by-dest.tsv` rows that list `0x007EEF60` as
**column 3** are callees **inside the over-merged blob**,
including `007EF3A1 → 004B4A10`. Those are not callers of
the dtor.

---

## 5. Gate around the activate

`007EF200` this = `CTCExpression`. `mov ebx, [ebx+4]` then
component id **`0x8F`** via `004365B0` when the Thing flag
at `+48` is set; else `ebp` is the earlier expression
object. `esi = [ebp+12]` is the `CExpressionDef`.

| `[def+116]` | Path |
|---|---|
| ≠0 | `004C7A10` / `0041649C` (`[0x13B86A0]`) — **not** `004B4A10` |
| 0, `+120` empty | skip |
| 0, `+120` non-empty | **`004B4A10`** as above |
| after activate, `[def+126]≠0` | `008430B0` / `006644F0` follow-on |

What concrete string lives at `CExpressionDef+120` on
first no-save Lookout / Oakvale things is **UNREAD** here.
Most defs leave it empty → `005FA740` skip. Host must not
invent `ActivateQuest("Q_NewOakValeIntro")` from this VA.

---

## Timeline (this site only)

```
Thing tick  [CTCExpression.vtbl+28] 007EF200(thing)
  007EAB90
  optional 0x8F component lookup
  esi = [expression+12]             // CExpressionDef
  [esi+116]==0
  005FA740([esi+120], empty intern)
    al==0 → skip
    al==1 → 00415DD0 copy
            004B4A10(QM, &copy, 0, [esi+124])
              00433530 one-name vector
              004B4260(vector, 0, [esi+124])
```

Not on the no-save New Game activate walk
(`0049F24E` WLD `+172` / `user.ini` `00892E80`
`"Gameflow"`). **PROVEN** as a later Thing-component path
when some `CExpressionDef+120` is non-empty.
