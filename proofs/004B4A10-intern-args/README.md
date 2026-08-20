# `004B4A10` intern/name arg, and every `E8` of `004B4A10` / `004AF610`

Investigation only. No production `src/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Unique `00CB5AD0` `E8` is `004B42E8` inside `004B4260`.
Init Quests walks `world+172` **TRUE** only.
`Q_NewOakValeIntro` is **FALSE** catalog (`+184` / `QM+44`),
not a `004B4A10` intern.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: `004B4A10` body — what intern/name argument
does it take? Enumerate every `E8` of `004B4A10` and
`004AF610`. For each: listing site, args, first-seen
no-save?

Authority: `listing-00480000.txt` `004B4A10` / `004B4260` /
`004AF610` / `004B00C0` / `004B3CE0` / `0049F24E`;
`listing-00400000.txt` `00416C11` / `00433530`;
`listing-00600000.txt` `0061AC28` / `0061A91D` /
`0061B10A`; `listing-006c0000.txt` `006C7086` /
`006C70BA`; `listing-007c0000.txt` `007EF3A1`;
`listing-00840000.txt` `0084407E` / `008440A0`;
`listing-00880000.txt` `00892E8F` / `00892ECF` /
`00892F40` / `00896A62`; listing `call 004B4A10` (8) /
`call 004AF610` (12); siblings
`q-novi-activator-callers`, `012C5D14-fablecrc-imm`,
`004B00C0-first-gate`, `004B00C0-gate-leftover`,
`ini-activate-quest`.

---

## Verdict

**`004B4A10` takes a `CString*` (12-byte interned-string
object), plus two flags.** It does **not** intern a
`char*` and does **not** push `0x012C5D14`. Body wraps
that one name into a 4-byte-stride vector (`00433530`
count **1**) and `004B4260`s it. `004B4260` then
`004B00C0` + unique `00CB5AD0`.

**`004AF610` is not activate.** `thiscall` `ret 4`:
name ∈ `QM+56` active list (`00411570` CString cmp).
`00892F40` is a **`jmp`**, not an `E8`.

No-save first **activate** is Init Quests
`0049F24E` `004B4260([world+172], 0, 1)` — **not**
`004B4A10`. First no-save **`004B4A10` take** is later
`user.ini` `"Gameflow"` via `00892E80`. Oakvale never.

| Question | Answer | Class |
|---|---|---|
| `004B4A10` name arg? | `CString*` arg0. Flags arg1/arg2 forwarded to `004B4260`. Body always `00433530(..., 1, 1)`. | **PROVEN** |
| PE intern `0x012C5D14` as that arg? | **No.** No `E8` site `push "Q_NewOakValeIntro"`. | **DISPROVEN** |
| `E8` count `004B4A10` / `004AF610` | **8** / **12** listing `call` | **PROVEN** |
| Init Quests uses `004B4A10`? | **No.** `004B4260(world+172)` TRUE list. | **DISPROVEN** |
| First no-save `004B4A10` take? | `00892E8F` `"Gameflow"` `(1,1)` after `+90584` skip | **PROVEN** |
| First no-save `004AF610`? | `004B3D2A` inside `004B3CE0` (only `E8` of `004B3CE0` is `004B4386` after TRUE `00CB5AD0`) | **PROVEN** site |
| `Q_NewOakValeIntro` on either? | **No.** FALSE catalog. Bind + wait intern only (`012C5D14-fablecrc-imm`). | **DISPROVEN** |

---

## 1. `004B4A10` body — name is `CString*`, not a PE intern

`listing-00480000.txt` (`int3` `004B4A0F` / `004B4A99`):

```
004B4A10  sub esp, 12
          push ebp / esi / edi
          push 1
          push 1
          mov  esi, ecx                 ; [0x13B89FC] QuestManager
          mov  ecx, [esp+36]            ; arg0 CString*
          lea  eax, [esp+36]            ; &arg0
          push eax
          push ecx
          push 0
          lea  ecx, [esp+32]            ; local vector {0,0,0}
          call 00433530                 ; ret 20; count=1
          mov  edx, [esp+36]            ; arg2
          mov  eax, [esp+32]            ; arg1
          push edx
          push eax
          lea  ecx, [esp+20]
          push ecx                      ; vector*
          mov  ecx, esi
          call 004B4260                 ; ret 12
          … 0099EAE0 range + 00BFEA14 …
          mov  al, [esp+28]
          ret 12
```

`00433530` (`listing-00400000.txt`) `ret 20`: dest
vector `this`, then `(0, name, &arg0, count=1, flag=1)`.
When `edi==1` it `0099EC30`s **one** `CString` into a
4-byte-stride range. **PROVEN** one-name wrapper.

`004B4260` `ret 12`:

```
004B4260  ebp = arg0 vector*            ; [begin, end) of CString*
          for i in 0 .. (end-begin)/4:
            0099EBF0 "QuestManager: Activate Quest"
            004B00C0(name)              ; QM+44 membership
            je skip
            004B42E8  call 00CB5AD0     ; UNIQUE E8
            004BB720  12-byte record (arg1 @+8, arg2 @+9)
          004B3CE0(construct)           ; then 004AF610 per record
          ret 12
```

Init Quests `0049F24E`: `push 1; push 0; lea edx,[esi+172];
call 004B4260` — **same flags as** `00416C11`, **no**
`004B4A10`. TRUE names only. First slot
`Q_SunnyvaleMaster` (`004B00C0-first-gate`). **PROVEN.**

`004B4A10` callers therefore pass:

| Slot | Meaning |
|---|---|
| arg0 | `CString*` name (object, not `char*` VA) |
| arg1 | forwarded to `004B4260` → construct `+8` |
| arg2 | forwarded to `004B4260` → construct `+9` |

Nobody on the `E8` set intern `0x012C5D14`.
**DISPROVEN** as Oakvale activate.

---

## 2. `004AF610` body — already-active test

```
004AF610  ebx = [ecx+56]                ; circular sentinel
          edi = [ebx]
          je empty → al=0  ret 4
          esi = [arg0]                  ; CString first dword (chars*)
          eax = [[edi+8]+48]            ; quest record +48 name
          cmp / 00411570
          match → al=1  ret 4
          next node; else al=0
```

**PROVEN.** `thiscall` `ret 4`. Arg0 is `CString*`.
Not `00CB5AD0`. Not Init Quests activate.

Thunk (not `E8`): `00892F40` `mov ecx,[0x13B89FC]; jmp 004AF610`
(script vtbl name-is-active). Gameflow **wait** is
`00892F60` `jmp 004B0FC0`, not this `E8` set.

---

## 3. Every `E8` of `004B4A10` (8)

Listing `call 004B4A10` only. `ecx` always `[0x13B89FC]`
except `004B4B5F` / `004B4D45` (`this` already QM).

| # | `E8` site | Real fn (int3) | Args `(name, a1, a2)` | No-save first-seen |
|---:|---|---|---|---|
| 1 | `00416C11` | `00416953` after `"Activate Initial Quests"` | `[game+90584]`, **0**, **1** | **Skip.** `0099E960` vs empty intern `0x122D70E` → `je 00416C16`. Would not be Oakvale. **PROVEN** skip |
| 2 | `004B4B5F` | `004B4AA0` (`ret 8`, Thing arg0) | component **`0x6C` record +40**, **1**, arg1 | Need `[thing+145]` live + id `0x6C`. **DISPROVEN** as Init / first Present |
| 3 | `004B4D45` | `004B4C50` | copy of same **+40**, **1**, **1** | Debug / use-item after `009AD410`. **DISPROVEN** as no-save Init |
| 4 | `0061AC28` | `0061A6A0` quest picker | `esi` = `world+196` AddTestQuest record, **1**, **1** (empty `+24` card) | Leftover UI. **DISPROVEN** as first no-save |
| 5 | `007EF3A1` | `007EF200` `CTCExpression` vtbl+28 | copy `[esi+120]`, **0**, `[esi+124]` | Runtime nested CString. **Not** intern `0x012C5D14`. Leftover thing. **DISPROVEN** as no-save |
| 6 | `0084407E` | `00843FC0` `CCreatureAction_ActivateQuest` vtbl+12 | `[this+168]`, **0**, `[this+172]` bool | Ctor arg / `def+40`. Same ctor also `"Expression_Follow"`. **DISPROVEN** Oakvale literal; leftover action |
| 7 | `00892E8F` | `00892E80` script vtbl+276 | arg `CString*`, **1**, **1** | **Yes, later.** `user.ini` `ActivateQuest("Gameflow")` → `00419CE0`. **PROVEN** take; **DISPROVEN** Oakvale |
| 8 | `00892ECF` | `00892EC0` vtbl+278 | same `CString*`, **1**, **0** | Sibling opcode. **Not** the recovered `user.ini` arm. **DISPROVEN** as first no-save |

`functions.tsv` parents `007EEF60` / `008421C0` for rows
5–6 are **DISPROVEN** grouping (`q-novi-activator-callers`).

---

## 4. Every `E8` of `004AF610` (12)

| # | `E8` site | Real fn | Arg0 | No-save first-seen |
|---:|---|---|---|---|
| 1 | `0049EA9E` | `0049EA40` | `[arg+88]` CString | **No.** Only `E8`s `007C9A1C` / `0074C141`. Not Init Game. |
| 2 | `004AFC79` | `004AFC60` | `QM+64` `CString*` walk (4-byte stride). Any hit → `al=0` | Called from `0049EA53` inside `0049EA40`. Same leftover. |
| 3 | `004B3B18` | QM body before `004B3CE0` (over-merged `004B2510`) | stack CString after `004B0B90` | Internal QM. **DISPROVEN** as Init Quests first name |
| 4 | `004B3D2A` | **`004B3CE0`** | 12-byte construct record (name at `[esi]`) | **Yes.** Unique `E8` of `004B3CE0` is `004B4386` after TRUE `00CB5AD0`. First TRUE take is `Q_SunnyvaleMaster`. **PROVEN** |
| 5 | `004B3E8C` | same `004B3CE0` | next 12-byte record | Same construct, later index. **PROVEN** same spine |
| 6 | `004B44C8` | `004B4490` | `[node+8]` on `QM+60` list | Re-check already-active then `004B43D0`. **Not** Init Quests |
| 7 | `0061A91D` | `0061A8A0` | picker row | Leftover quest UI |
| 8 | `0061B10A` | `0061A6A0` | `esi` name; on hit append `" [Active]"` | Leftover debug string |
| 9 | `006C7086` | `006C6010` fold | stack `"Expression_Steal"` | Leftover expression id 6. **DISPROVEN** as quest activate |
| 10 | `006C70BA` | same | stack `"Expression_Picklock"` | Leftover expression id 4 |
| 11 | `008440A0` | `00844090` | `[this+168]` | Action “already active?” before vtbl+16. Leftover |
| 12 | `00896A62` | `00896A30` | `esi` after `004B0D30` | HUD `TEXT_QST_078_GM_MSG_NEW_QUEST_CARD`. Leftover card, not Init |

`004AFC60`: none of `QM+64` active → `al=1`; any
`004AF610` hit → `al=0`. Predicate, not activate.

---

## 5. No-save timeline (still no Oakvale `004B4A10`)

```
004A0D90  FinalAlbion.qst
  AddQuest TRUE  → +172 / +184 / QM+44     // Q_SunnyvaleMaster first
  AddQuest FALSE → +184 / QM+44 only       // Q_NewOakValeIntro here
  AddTestQuest   → +196
0049F21B  "Init Quests"
0049F24E  004B4260([world+172], 0, 1)       // NOT 004B4A10
  004B00C0 take → 00CB5AD0
  004B3CE0 → 004B3D2A 004AF610             // first 004AF610
0049F259  004B2890
00416BCF  +90584 empty skip 004B4A10
user.ini  00892E80 004B4A10("Gameflow",1,1) // first 004B4A10 take
00CE7670  wait intern 0x012C5D14           // 00892F60, not 004B4A10
007EF200 / 00843FC0 / 0x6C+40               // no PE intern
```

`Q_NewOakValeIntro` never enters `004B4A10` / `004AF610`
as a recovered first-seen name. **PROVEN** absence.

---

## 6. Host (read-only)

`EngineLifecycle.InitCharactersAndQuests` walks
`_worldPlus172` then Notes `+90584` skip and
`"004B4A10 not Q_NewOakValeIntro"`. `user.ini`
`ActivateQuest` Notes `00892E80` `004B4A10(1,1)` then
`ActivateNamedQuest("Gameflow")`. **MATCH.**

Do **not** add `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** retarget Init Quests through `004B4A10`.
A later live Thing CString equal to intern `0x012C5D14`
stays **UNKNOWN** (`012C5D14-fablecrc-imm`).

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `004B4A10` | one `CString*` → `00433530(1)` → `004B4260` | **PROVEN** |
| `00433530` | vector copy `ret 20` | **PROVEN** |
| `004B4260` / `004B42E8` | TRUE/name list + unique `00CB5AD0` | **PROVEN** |
| `0049F24E` | Init Quests `world+172` | **PROVEN**; **DISPROVEN** as `004B4A10` |
| `00416C11` | `+90584` | **PROVEN** skip no-save |
| `00892E8F` | `"Gameflow"` `(1,1)` | **PROVEN** first `004B4A10` take |
| `004AF610` | `QM+56` membership | **PROVEN** IsActive |
| `004B3D2A` | first no-save `004AF610` | **PROVEN** |
| `00892F40` | `jmp 004AF610` | **PROVEN** thunk; **not** `E8` |
| intern `0x012C5D14` as `004B4A10` arg | — | **DISPROVEN** |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00600000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-006c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-007c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00840000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00880000.txt`
- `C:\FableCSharp\proofs\q-novi-activator-callers\README.md`
- `C:\FableCSharp\proofs\012C5D14-fablecrc-imm\README.md`
- `C:\FableCSharp\proofs\004B00C0-first-gate\README.md`
