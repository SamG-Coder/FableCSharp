# `004B4A10` arg0 is a caller `CString*`, not intern `0x012C5D14`

Investigation only. No production `src/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** treat PE intern `0x012C5D14` as the dword
`004B4A10` consumes. Do **not** collapse QST catalog
`world+196+0` with no-save Init Quests `world+172`.
Do **not** treat `XXXSectionStart` as a `CString` source.
`CActivateQuestDef` never intern `0x012C5D14`.
`FableCrc("Q_NewOakValeIntro")=0x8D19C362` is also
absent. `[retail+8]` does not change Init Quests.
No recovered later presenter.

Question: `004B4A10` takes caller `CString*` at
`[esp+36]`. What is every caller’s runtime CString
source? Could any CString **equal**
`"Q_NewOakValeIntro"` without the PE intern?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **UNKNOWN**.

Authority: `listing-00480000.txt` `004B4A10` /
`004B4A5A` / `004B4AA0` / `004B4B5F` / `004B4D45` /
`004A113B`;
`listing-00400000.txt` `00416C11` / `00419CE0` /
`00415DD0` / `00429950` / `00433530`;
`listing-00600000.txt` `0061AB30` / `0061AC28`;
`listing-007c0000.txt` `007EF200` / `007EF3A1`;
`listing-00840000.txt` `00843F50` / `00843FC0` /
`0084407E`;
`listing-00880000.txt` `00892E80` / `00892EC0` /
`008969A0`;
`listing-00980000.txt` `0099E960` / `0099EA60` /
`0099EBF0` / `0099EC30` / `0099EFB0` / `009A0590`;
`listing-009c0000.txt` `009D49B0`;
`listing-00c80000.txt` `00CB5AD0`;
`e8.tsv` dest `0x004B4A10` (8);
siblings `proofs/q-novi-activator-callers`,
`proofs/q-novi-later-presenter`,
`proofs/cactivatequestdef-oakvale-instances`,
`proofs/q-novi-construct-no-save-audit`,
`proofs/addtestquest-token`,
`proofs/ini-activate-quest`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| What does `004B4A10` take? | **Caller `CString*`.** After `sub esp,12` + 3 regs + `push 1`/`push 1`, `[esp+36]` is stdcall arg0. **No** `push 0x012C5D14`. | **PROVEN** |
| Does it intern the PE string? | **No.** `00433530` `0099EC30`s that object (share intern **header***, `inc [header+13]`), then `004B4A5A` `004B4260`. | **PROVEN** |
| Is `[CString]` ever the PE intern? | **No.** `0099EBF0` / `0099EA60` alloc **17-byte** header and **copy** chars (`009A0590` / `009A0300`). `[obj]` is the header. | **PROVEN** |
| Can content equal the name without the PE intern? | **Yes.** `00CB5AD0` header-ptr `cmp`, then `00429950` char compare. QST-parsed headers are the recovered case. | **PROVEN** mechanism |
| Which recovered caller holds that ASCII? | Leftover picker **`0061AC28`**: `world+196` record `+0` from `AddTestQuest("Q_NewOakValeIntro", …)` file quotes. Gate `[this+343]`. **Not** New Game. | **PROVEN** leftover |
| Any no-save first-seen caller with that content? | **No.** Empty `+90584` skip; `user.ini` `"Gameflow"`; Init Quests is `004B4260` not `004B4A10`. | **PROVEN** omit |
| `CActivateQuestDef` / TNG / `FableCrc` / `[retail+8]`? | Not this name; West `XXXSectionStart` is a **consumer**; retail+8 does not rewrite `world+172`. | **DISPROVEN** (siblings) |
| Invent `ActivateQuest("Q_NewOakValeIntro")`? | **No.** | **DISPROVEN** |

---

## Verdict

**`004B4A10` never consumes intern `0x012C5D14`.**
Arg0 is a `CString*`. The 1-name vector is a
**header-sharing copy** of whatever the caller already
held.

A CString **equals** `"Q_NewOakValeIntro"` when the
intern header’s characters match, **even if** no `.text`
site `push 0x012C5D14`. That is how QST quotes become
catalog names.

The only recovered `004B4A10` site whose source **does**
hold that ASCII is leftover quest-picker `0061AC28`
(`world+196+0`). It is **not** on the no-save walk
(`[ui+343]==0` skip). Every other recovered source is
empty / `"Gameflow"` / `Global_*` / `Expression_*` /
a live Thing slot with **no** recovered Oakvale writer.

No recovered later presenter. Do **not** invent
`ActivateQuest("Q_NewOakValeIntro")`.

---

## 1. `004B4A10` body — `[esp+36]` is arg0

`listing-00480000.txt`, int3 `004B4A09`–`004B4A0F`,
`ret 12` `004B4A96`. thiscall `ecx` = QuestManager
`[0x13B89FC]`. Stdcall 12 bytes: `CString*`, flag, flag.

```
004B4A10  sub esp, 12
          push ebp / esi / edi
          push 1                       ; leftover count for 00433530
          push 1                       ; leftover bool for 00433530
          mov esi, ecx
004B4A1C  mov ecx, [esp+36]            ; arg0 CString*
004B4A20  lea eax, [esp+36]            ; &arg0 (1-slot range)
          push eax / push ecx / push 0
          lea ecx, [esp+32]            ; empty 12-byte vector
          xor [esp+32..40]
004B4A44  call 00433530                ; 0099EC30 copy 1 name
          push [vec+4] / push [vec]
004B4A5A  call 004B4260
          … dtor …
          ret 12
```

Stack after `sub 12` + 3 regs + two `push 1`:
`[esp+36]` = original `[esp+4]` = **arg0**. The two
`push 1` are **not** the name; `00433530` reads them
as count=`1` (`[esp+32]` at `00433539`) and a flag
(`[esp+48]` at `004335E6`). Count `1` →
`0099EC30(dst, arg0)`.

**PROVEN:** no PE imm. Unique `00CB5AD0` stays
`004B42E8` inside `004B4260`.

---

## 2. CString identity vs PE intern vs content

| Helper | What it stores |
|---|---|
| `0099EA60` | `00BFEA1A(17)` header; `009A0590` strlen of **char\*** then `009A0300` copy; `[header+13]=1` |
| `0099EBF0` | `[obj]=0`; then `0099EA60(char*, n)` → `[obj]=header*` |
| `0099EC30` / `0099EFB0` | `[dst]=[src]` header*; `inc [header+13]` (**share**, not strdup of chars) |
| `00415DD0` | `[src]` dword → `009D49B0(0x13CA828, dst, dword)` → `0099EBF0` from names table / `-1` empty |
| `0099E960` | `[obj]` null → `rep cmpsb` vs empty intern `0x122D70E`; else `004115A0` |

`push 0x012C5D14; call 0099EBF0` therefore builds a
**new header** whose **characters** are the PE string.
`[CString]` is **not** `0x012C5D14`.

`00CB5AD0` (`listing-00c80000.txt`):

```
mov eax, [edi]              ; map-key header*
mov ecx, [ebx]              ; lookup header*
cmp eax, ecx → hit
… 00429950(eax) with ecx = lookup CString
test al / je hit            ; 00429950 al=0 on equal
```

`00429950` walks `[header]` as char*. **Content
equality hits the factory table even when header
pointers differ.**

`FableCrc("Q_NewOakValeIntro")=0x8D19C362` is a
names.bin hasher, **not** a CString key. Absent from
the six `CActivateQuestDef` rows and from `names.bin`.

---

## 3. Eight `E8`s — runtime CString source

`e8.tsv` dest `0x004B4A10`: **8**. `ff.tsv` dest **0**.

| `E8` | Real fn | CString* passed | Runtime source of `[CString]` | Oakvale content without PE intern? |
|---|---|---|---|---|
| `00416C11` | `00416953` suffix after `0049F180` | `lea edi, [game+90584]` | Game-object slot vs empty `0x122D70E` (`0099E960`). Equal → **skip**. | **No** recovered writer. Slot empty on no-save. |
| `004B4B5F` | **`004B4AA0`** | `[comp 0x6C record + 40]` | Live Thing `[+145]`. Writer **`008969A0`**: `0099EFB0([esp+24] → +40)` then `004B4AA0`. | **Only if** that caller CString already had the chars. No PE push. First Thing **UNKNOWN**. |
| `004B4D45` | sibling of `004B4AA0` | `lea ebp, [record+40]` after `0099EFB0` from `[esp+32]` | Same `+40` slot, flags `(1,1)`. | Same as row above. |
| `0061AC28` | **`0061AB30`** | `esi` = 28-byte `world+196` record (**`+0` name**) | QST `AddTestQuest` quoted name via `0099EBF0` / `009B9E00`. Shipped row **is** `Q_NewOakValeIntro`. Gate `[this+343]`. Empty `+24` card → this `E8`; else `004B4C50`. | **Yes (content).** Leftover picker, **DISPROVEN** New Game. |
| `007EF3A1` | **`007EF200`** `CTCExpression` | stack copy of nested `+120` | Non-empty vs `0x122D70E` (`005FA740`); `00415DD0` inflate/copy. `[+116]≠0` → camera **instead**. | Persist / Lookout TNG **≠** this name. Live later Thing **UNKNOWN**. |
| `0084407E` | **`00843FC0`** | `lea eax, [this+168]` | Ctor `00843F50` `0099EC30`s arg3 into `+168`. Six ctor `E8`s: `Expression_*` PE char\* (headers) or `[CActivateQuestDef+40]`. | Defs are `Global_*`. **DISPROVEN** Oakvale. |
| `00892E8F` | **`00892E80`** slot 276 | thunk `[esp+4]` | `00419CE0`: `0099E4B0` + `0099EFB0([cmd]+8)` then `call [vtbl+1104]`. TLC `user.ini` **`ActivateQuest("Gameflow")`**. | **No** (that file). Other ini **0** `ActivateQuest`. |
| `00892ECF` | **`00892EC0`** slot 278 | same thunk arg | Same CString\*, flags `(1,0)`. | Same as `00892E80`. |

Grouping parents `007EEF60` / `008421C0` / `00892D80` /
`004B49E0` / `0061A6A0` / `00416953` are the usual
greatest-start lie. Real fns are int3-bounded as above.

---

## 4. Could content match without `0x012C5D14`?

**Mechanism: yes. No-save first-seen: no recovered source.**

```
QST quote  0099EBF0(file char*)  → header A (copy of ASCII)
PE push    0099EBF0(0x012C5D14) → header B (copy of same ASCII)
004B4A10   0099EC30 share header A or B
00CB5AD0   cmp headers; else 00429950 chars  → HIT either way
```

Recovered **file** ASCII that equals the name:

| Store | Path to `004B4A10`? | No-save? |
|---|---|---|
| `FinalAlbion.qst` `AddQuest(..., FALSE)` | **No** (`004B2850` → `QM+44` / `world+184` catalog only) | catalog only |
| `AddTestQuest("Q_NewOakValeIntro", …)` `world+196+0` | **Yes**, leftover `0061AC28` | **`[ui+343]` skip** |
| `user.ini` | **Yes** `00892E80` | `"Gameflow"` only |
| `CActivateQuestDef` `+7` names offset | **Yes** via `00843F50` → `+168` | `Global_*` |
| `EXPRESSION+120` persist | **Yes** via `007EF200` | not this name |
| `StartOakValeWest.tng` `XXXSectionStart` | **No** (`ThingInstance.Section` consumer) | not a CString |
| `LookoutPoint.tng` | **No** (0 token) | first TNG |

`0061AC28` is the existence proof that **content
without PE intern** can reach `004B4A10`. It is
**LEFTOVER**, not the missing no-save presenter.

`008969A0` can `0099EFB0` **any** caller CString onto
`0x6C+40` then `004B4AA0`. That caller is HUD/script,
not `push 0x012C5D14`. First live Thing whose copied
chars equal the name stays **UNKNOWN**
(`q-novi-later-presenter`: nobody recovered).

---

## Timeline (no-save) — still no Oakvale `004B4A10`

```
004A0D90  QST
  AddQuest FALSE  → QM+44 / world+184     // catalog CString, not 004B4A10
  AddTestQuest    → world+196+0           // Oakvale content; picker only
0049F24E  004B4260([world+172])           // not 004B4A10
00416C11  [game+90584] empty → skip
user.ini  00892E80 004B4A10("Gameflow")   // header from ini token
0061AB30  [+343]==0 skip
007EF200 / 00843FC0 / 004B4AA0            // need live Thing; Lookout 0
```

---

## What this is not

| Claim | Class |
|---|---|
| `004B4A10` arg0 is intern `0x012C5D14` | **DISPROVEN** (`[esp+36]` `CString*`) |
| `[CString]` dword is the PE intern | **DISPROVEN** (17-byte header) |
| Content equality requires that PE pointer | **DISPROVEN** (`00429950`) |
| `0061AC28` is New Game activate | **DISPROVEN** (`[+343]` leftover) |
| `world+196+0` Oakvale is Init Quests | **DISPROVEN** (`+172` TRUE only) |
| `CActivateQuestDef` / TNG present this name | **DISPROVEN** |
| `[retail+8]=1` adds TRUE Oakvale | **DISPROVEN** |
| Host `ActivateQuest("Q_NewOakValeIntro")` | **DISPROVEN** |

---

## Remaining UNKNOWN

1. First live Thing after a later region whose
   `CTCExpression+120` / action `+168` / `0x6C+40`
   **characters** equal `"Q_NewOakValeIntro"`
   (header may or may not match a PE-sourced header).
2. Writer of `[game+90584]` if any later fill is
   not empty — no-save skip **PROVEN**.

Until (1) dumps, the later presenter stays **nobody
recovered**. Do not invent it.

---

## Host

`EngineLifecycle` Notes `00416BCF` skip and
`"004B4A10 not Q_NewOakValeIntro"`.
`ActivateNamedQuest` walks `world+172` only.
`No_save_does_not_activate_Q_NewOakValeIntro`.
**MATCH.**

Do **not** add `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** treat QST `AddTestQuest` content as a
no-save `004B4A10`.

---

## Sources (absolute)

- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00600000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-007c0000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00840000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00880000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-009c0000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00c80000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\proofs\q-novi-activator-callers\README.md`
- `C:\FableCSharp\proofs\q-novi-later-presenter\README.md`
- `C:\FableCSharp\proofs\cactivatequestdef-oakvale-instances\README.md`
- `C:\FableCSharp\proofs\q-novi-construct-no-save-audit\README.md`
- `C:\FableCSharp\proofs\addtestquest-token\README.md`
