# `004EE23F` pair 62 `009B0AC0` / `0044C6B0` is `CActivateQuestDef`

Investigation only. No production `src/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` → `"Init Game"`
`0042F491` → `00418DCA` → `[vtbl+4]`
`004184BD` → `00418585` `004EE23F`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: recover pair 62 `CActivateQuestDef`
`004F5B7D` factory `0x4D8A32` sites
`004F5BA5` / `004F5BAC`. Confirm registrar
pair **MATCH** remaining-pairs. Then: does
Init Thing Components registration cause
no-save to construct / activate
`Q_NewOakValeIntro`? Also recover pair 63
`CCrateStackDef` `004F5C9E` factory
`0x4D8A6A` sites `004F5CC6` / `004F5CCD`.

| Field | Value | Class |
| --- | --- | --- |
| `0044C6B0` | `004F5BA5` | **PROVEN** |
| `009B0AC0` | `004F5BAC` | **PROVEN** |
| Factory | `004D8A32` `00BFEA1A(48)` then `jmp 004D5056`; vtbl **`0123C7F4`** | **PROVEN** |
| Ctor | `004D5056` `0044C0C0` then `[esi]=0123C7F4`; `or [esi+40], -1` | **PROVEN** |
| Size | **48** (`push 48` at factory; vtbl[20] `004D506C` `push 48; pop eax; ret`) | **PROVEN** |
| Remaining-pairs row 62 | name / factory / sites / 1 CTC | **MATCH** |
| Sibling payloads factory | alloc 48 / ctor `004D5056` / vtbl `0x0123C7F4` | **MATCH** |
| Registration activates `Q_NewOakValeIntro`? | **No.** Add Def Class is not `ActivateQuest`. | **DISPROVEN** |

Authority: `Fable.exe`
`listing-004c0000.txt` `004F5B7D` /
`004D8A32` / `004D5056`; `listing-00840000.txt`
`00843F50` / `00843FC0`; `listing-00480000.txt`
`004B4A10`; `listing-00780000.txt` `007B5590`
/ `007B5680` / `007B5AA4`; `fn 004F5BA5`;
`proofs/004EE23F-remaining-pairs` row 62;
`proofs/cactivatequestdef-payloads`;
`proofs/q-novi-activator-callers`;
`proofs/008421C0-activate`;
`proofs/qst-autostart-list`.
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01243E40` **`CActivateQuestDef`**.
`assembly/exe/00-index/vtbl.tsv` `0x0123C7F4`.
`xrefs.tsv` `Q_NewOakValeIntro` `0x012C5D14`.

Listing string at `004F5B7D` is
**`CActivateQuestDef`** (not invented).
Shape-2 (`push` + `0042DAE0`).

```
004F5B7D  push "CActivateQuestDef"
004F5B82  lea ecx, [ebp-1292]
004F5B88  call 0099EBF0
004F5B8D  push 0x4D8A32
004F5B92  lea eax, [ebp-1292]
004F5B98  push eax
004F5B99  lea ecx, [ebp-1788]
004F5B9F  call 0042DAE0
004F5BA4  push eax
004F5BA5  call 0044C6B0
004F5BAA  mov ecx, eax
004F5BAC  call 009B0AC0
```

```
004D8A32  push 48
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004D8A45
          mov ecx, eax
          jmp 004D5056
004D8A45  xor eax, eax
          ret

004D5056  push esi
          mov esi, ecx
          call 0044C0C0
          mov [esi], 0x123C7F4
          or [esi+40], -1
          mov eax, esi
          pop esi
          ret

004D506C  push 48
          pop eax
          ret
```

Next pair is `CCrateStackDef` `004F5CC6` /
`004F5CCD` factory `004D8A6A` (**PROVEN**
name/sites/factory/size/vtbl below; not
shipped).

---

## Verdict

Pair 62 on `004EE23F` **MATCH**es
`proofs/004EE23F-remaining-pairs` row 62
and `proofs/cactivatequestdef-payloads`
factory layout.

Init Thing Components **registers** the
def class. It does **not** construct a
quest, does **not** queue
`CCreatureAction_ActivateQuest`, and does
**not** call `004B4A10`. No-save New Game
still does **not** activate
`Q_NewOakValeIntro`.

Do **not** add `ActivateQuest("Q_NewOakValeIntro")`
in `src/`.

---

## 1. Registrar pair MATCH remaining-pairs row 62

`proofs/004EE23F-remaining-pairs` §5:

| n | `push` | listing string | factory imm | `0044C6B0` | `009B0AC0` | CTC between |
| --: | --- | --- | --- | --- | --- | --: |
| 62 | `004F5B7D` | `CActivateQuestDef` | `0x4D8A32` | `004F5BA5` | `004F5BAC` | 1 |

`listing-004c0000.txt` after pair 61
`CInterestingToVillagersDef` `004F5AF6`:

| Listing | Remaining-pairs | Class |
| --- | --- | --- |
| `004F5B7D` `68 40 3E 24 01` `push "CActivateQuestDef"` | `004F5B7D` | **MATCH** |
| `004F5B8D` `push 0x4D8A32` | factory `0x4D8A32` | **MATCH** |
| `004F5B9F` `call 0042DAE0` | shape-2 pack | **MATCH** |
| `004F5BA5` `call 0044C6B0` | `004F5BA5` | **MATCH** |
| `004F5BAC` `call 009B0AC0` | `004F5BAC` | **MATCH** |
| one `004D2EF0` at `004F5B34` between 61 and 62 | CTC between = 1 | **MATCH** count |

`strings.tsv`:

```
0x01243E40	0xE43E40	CActivateQuestDef
```

Same listing annotates the immediate as
`"CActivateQuestDef"`. `xrefs.tsv`
`0x01243E40`:

| Site | Fn | Role |
| --- | --- | --- |
| `004F5B7E` | `004EE137` (greedy parent of `004EE23F`) | this registrar |
| `007B5594` | `007B5590` | later type-name intern |
| `007B5688` | `007B5680` | later def lookup |

`abs.tsv` `0x004F5B8D` → `0x004D8A32`.
`0042DAE0` is the name+factory pack helper.
Treating it as `009B0AC0` is **DISPROVEN**
(remaining-pairs §2).

In-range `004EE23F` has **no** `push "…"`
on the CTC row. Remaining-pairs counted it
unnamed. Helper `004D5043` (called
`004F5B17`) pushes
`"CTCCarriedActionUseActivateQuest"` then
`0099EBF0`. That string is **out of**
`004EE932`…`004F9144`. Do not promote it
as an in-range registrar name. Factory
imm on that row is `0x4DC936`. The CTC
row is **not** Add Def Class (no
`0044C6B0` / `009B0AC0`).

---

## 2. Factory MATCH sibling payloads

`proofs/cactivatequestdef-payloads`:

> Factory `004D8A32` alloc **48**, ctor
> `004D5056` vtbl `0x0123C7F4`,
> `[+40]\|=-1`.

Listing **MATCH**es that dump. Same thunk
shape as nineteenth `004E0B4B`:
`00BFEA1A` with immediate **48**, null →
`xor eax, eax; ret`, else
`mov ecx, eax; jmp 004D5056`.

`004D5056` calls `0044C0C0`, writes
`[esi]=0x0123C7F4`, then `or [esi+40], -1`.
No other stores. Object is 48 bytes.

`vtbl.tsv` `0x0123C7F4`:

| slot | VA | note |
| --: | --- | --- |
| 0 | `004D8A48` | dtor (`[esi]=0x1230BA0` then `009FC550`) |
| 1–17 / 21–24 | shared `0042D930`…`0042DAA0` family | no invented names |
| **18** | **`007B5740`** | persist: `+40` intern CString, `+44` bool |
| 19 | `004E0E79` | |
| **20** | **`004D506C`** | size `push 48; pop eax; ret` |

`listing-004c0000.txt` has **one** hit of
`004D8A32`: the factory body. Registrar
**pushes** the imm; it does **not** `E8`
the factory. `abs.tsv` is the only
`.text` xref of the factory VA.

RTTI `0x01379518` `.?AVCActivateQuestDef@@`
is this **def** vtbl, not the later action
vtbl `0x012752C4`.

Persist / six `game.bin` 16-byte rows stay
with the sibling: intern names **UNREAD**;
`names.bin` has **no** `Q_NewOakValeIntro`.

---

## 3. Registration is not `ActivateQuest` — DISPROVEN

`004EE23F` at this pair:

1. `0099EBF0` name `"CActivateQuestDef"`.
2. `0042DAE0` packs factory `004D8A32`.
3. `0044C6B0` `004F5BA5`.
4. `009B0AC0` `004F5BAC` Add Def Class.

That inserts a type record so later
LoadDef can construct 48-byte defs. It is
the same Add Def Class walk as
`CHeroMorphDef` … `CHasNameDef`.

`listing-004c0000.txt` has **zero**
`call 00843F50` and **zero**
`call 004B4A10`. `004EE23F` does not
construct `CCreatureAction_ActivateQuest`
and does not activate a quest.

`strings.tsv` `ActivateQuest` `0x0122F380`
xrefs `00419DAF` (`00419D90` console
registrar only). **Not** this pair.

Even a later LoadDef of
`NULLDEF_CActivateQuestDef` (game.bin id
61) would be a 48-byte def with
`[+40]=-1`, not
`ActivateQuest("Q_NewOakValeIntro")`.

---

## 4. Cross `00843F50` / `00843FC0` / `004B4A10`

Later **thing-action** path
(`q-novi-activator-callers` /
`008421C0-activate` /
`cactivatequestdef-payloads`):

```
007B5680  lookup "CActivateQuestDef"     ; not 004EE23F
          [def+40] intern, [def+44] bool
00843F50  ctor vtbl 0x012752C4           ; CCreatureAction_ActivateQuest
          copy intern → this+168
006644F0  queue
00843FC0  vtbl 0x012752C4 slot 12
004B4A10  ([0x13B89FC], &this+168, 0, this+172)
```

### `00843F50` (`ret 16`)

Ctor stores `[esi]=0x012752C4`, copies
arg3 into `this+168` (`0099EC30`), arg4
byte into `this+172`, thing into
`this+173`. RTTI `0x01384F40`
`.?AVCCreatureAction_ActivateQuest@@`.
**Not** def vtbl `0x0123C7F4`.

Six `E8` of `00843F50` (listings; **none**
in `004c0000`):

| Site | Parent | CString intern | Arg4 |
| --- | --- | --- | --- |
| `00629979` | `00629930` | `"Expression_Follow"` `0x01259170` | `0` |
| `00629A09` | `006299C0` | `"Expression_Wait"` `0x01259184` | `0` |
| `007B5AA4` | `007B57C0` `CTCCarriedActionUseActivateQuest` | `009D49B0([def+40])` | `[def+44]` |
| `007EF66C` | `007EF600` | `009D49B0([def+40])` | `[def+44]` |
| `007F0232` | `007F01F0` | `"Expression_Fish"` `0x012718F8` | `0` |
| `007F0410` | `007F03D0` | `"Expression_Dig"` `0x01271908` | `0` |

**DISPROVEN:** none of the six sites push
`0x012C5D14`. Same ctor queues expression
names. **DISPROVEN** as always-quest.

### `00843FC0`

Zero `E8`. Only `0x012752C4[12]`. Body
(`listing-00840000.txt`) copies thing
`+173` onto QuestManager, then
`004B4A10([0x13B89FC], &this+168, 0,
this+172)`. Needs a **queued** action on
a live Thing. Not on no-save Leave
(`007EF200-first-plus120` Original).

### `004B4A10`

`thiscall` `[0x13B89FC]`, `ret 12`. Wraps
arg1 CString via `00433530` then
`004B4260`. Eight `E8`; the action site
is `0084407E` inside `00843FC0`, **not**
`004EE23F`. No-save `00416C11` is
`[game+90584]` vs empty intern — skip
(`q-novi-activator-callers`).

---

## 5. `Q_NewOakValeIntro` is not this pair

`xrefs.tsv` intern `0x012C5D14`:

| Site | Fn | Role |
| --- | --- | --- |
| `00CD6E28` / `00CD6E87` | `00CD5170` | bind `S_QNOVI` |
| `00CE791E` / `00CE7978` / `00CE79CA` | `00CE7670` | Gameflow **wait** (`IsActive`) |

**Zero** hits in `004EE23F`. **Zero** hits
in `00843F50` / `00843FC0` / `004B4A10`.

`00CE7670` yields while the quest is 0.
That is **not** activate.

| Store | Has the name? |
| --- | --- |
| This registrar | **no** |
| Factory / ctor | **no** (only vtbl + `[+40]\|=-1`) |
| `names.bin` | **no** (sibling) |
| Six `game.bin` ASCII | **no** (sibling) |
| `00843F50` immediates | **no** |
| Inflated intern u32 in the six 16-byte bodies | **UNREAD** (sibling) |
| No-save `004B4A10` | **not** this class |

---

## 6. Pair 63 `CCrateStackDef` (time allowed)

| Field | Value | Class |
| --- | --- | --- |
| `0044C6B0` | `004F5CC6` | **PROVEN** |
| `009B0AC0` | `004F5CCD` | **PROVEN** |
| Factory | `004D8A6A` `00BFEA1A(48)` then in-line `0044C0C0`; vtbl **`0123C86C`** | **PROVEN** |
| Size | **48** (`push 48` at factory; vtbl[20] `004D50B2` `push 48; pop eax; ret`) | **PROVEN** |
| Remaining-pairs row 63 | name / factory / sites / 2 CTC | **MATCH** |

`strings.tsv` `0x01243E30` **`CCrateStackDef`**.
Listing `004F5C9E` `68 30 3E 24 01`.
Shape-2. No `jmp` thunk: ctor is in-line
like twentieth.

```
004F5C9E  push "CCrateStackDef"
004F5CA3  lea ecx, [ebp-1300]
004F5CA9  call 0099EBF0
004F5CAE  push 0x4D8A6A
004F5CB3  lea eax, [ebp-1300]
004F5CB9  push eax
004F5CBA  lea ecx, [ebp-1804]
004F5CC0  call 0042DAE0
004F5CC5  push eax
004F5CC6  call 0044C6B0
004F5CCB  mov ecx, eax
004F5CCD  call 009B0AC0
```

```
004D8A6A  push esi
          push 48
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D8A8A
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123C86C
          mov eax, esi
          pop esi
          ret
004D8A8A  xor eax, eax
          pop esi
          ret
```

No extra dword stores after the vtbl
write. `004D50A0` also writes
`0x0123C86C`; factory does **not** `jmp`
it.

`vtbl.tsv` `0x0123C86C` slot 20 =
`004D50B2`; slot 0 = `004D8A8E`. RTTI
`0x01379538` `.?AVCCrateStackDef@@`.

`abs.tsv` `0x004F5CAE` → `0x004D8A6A`.
`xrefs.tsv` `0x01243E30`: `004F5C9F`
(this registrar), `006583CF`, `0065F7BF`
(later lookups). **Not** a quest
activator.

Two unnamed `004D2EF0` between pair 62
and 63 (`004F5BEA` factory `0x4D5070`;
`004F5C55` factory `0x4D50B6`). Helpers
`004D508D` / `004D50D3` push
`"CTCWaterWader"` / `"CTCCrateStack"`
out of range. Remaining-pairs CTC
column = 2 unnamed. **MATCH** count.

Next pair after this is
`COverheadDisplayDef` `004F5D7C` /
`004F5D83` factory `0x4D8AB0`
(remaining-pairs row 64; factory body
**UNREAD** here).

---

## 7. Host leftover

`EngineLifecycle.AddFirstDefClass` Notes
through Fortieth `CMultiStaticMeshDef`
(`004F306B` / `004E31FA` / `004E1516` /
size 52 / vtbl `0124265C`) then
**returns**. `FortiethDefClassRegistered
= true`.

Forty-first constants
(`CHeroCentreDef` `004F3338`) exist in
the host file and are **not** consumed by
`AddFirstDefClass`. Pair 62 is 22 pairs
after Fortieth.

This investigation does **not** ship pair
62. Until lead Notes / live-registers
this pair, `AddFirstDefClass` still stops
at Fortieth.

| After Fortieth | Native | Host after Fortieth |
| --- | --- | --- |
| rows 41…61 | remaining-pairs | **LEFTOVER** |
| 1 unnamed `004D2EF0` (`0x4DC936`) | listing `004F5B11`…`004F5B77` | **LEFTOVER** |
| 62 `CActivateQuestDef` `004F5BA5` / `004D8A32` `jmp 004D5056` size 48 vtbl `0123C7F4` | **PROVEN** (this file) | **LEFTOVER** (not shipped) |
| 2 unnamed `004D2EF0` | listing `004F5BC7`…`004F5C98` | **LEFTOVER** |
| 63 `CCrateStackDef` `004F5CC6` / `004D8A6A` size 48 vtbl `0123C86C` | **PROVEN** (this file) | **LEFTOVER** (not shipped) |
| rows 64…111 | remaining-pairs | **LEFTOVER** |

Note-only + flag would still **not** be a
live 48-byte object. Factory `E8` is
**not** on this walk. Inventing
`ActivateQuest("Q_NewOakValeIntro")` from
a Note would **DIVERGE**.

---

## Original

Sixty-second Add Def Class on `004EE23F`:

1. `0099EBF0` name `"CActivateQuestDef"`.
2. `0042DAE0` packs factory `004D8A32`.
3. `0044C6B0` `004F5BA5`.
4. `009B0AC0` `004F5BAC`.

Factory alloc 48, ctor `004D5056`.
Base `0044C0C0`. Vtbl `0123C7F4`.
One extra dword `+40` `or -1`.

One unnamed CTC between sixty-first
`CInterestingToVillagersDef` and this
pair. Two unnamed CTC after, then
`CCrateStackDef`.

Not Oakvale. Not a Thing instance. Not a
file I/O site. Not `004B4A10`.

```
004EE23F  register CActivateQuestDef factory 004D8A32  // no instance
00843F50  later thing-action ctor                     // not here
00843FC0  later vtbl+12                               // not here
004B4A10  needs a CString name                        // not here
00CE7670  wait Q_NewOakValeIntro == 0
```

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `004F5B7D` / `004F5BA5` / `004F5BAC` | pair 62 Add Def Class `CActivateQuestDef` | **PROVEN**; remaining-pairs **MATCH** |
| `004D8A32` / `004D5056` / `0123C7F4` | factory / ctor / vtbl; size 48 | **PROVEN**; payloads sibling **MATCH** |
| `004D506C` | vtbl[20] size 48 | **PROVEN** |
| `004F5B34` | unnamed CTC `004D2EF0` (`0x4DC936`) | **PROVEN** count; in-range name **UNREAD** |
| `007B5590` / `007B5680` | later type-name / lookup | **PROVEN**; **DISPROVEN** as this pair |
| `00843F50` | action ctor vtbl `012752C4` | **PROVEN** later; **DISPROVEN** as registrar |
| `00843FC0` / `0084407E` | action run → `004B4A10` | **PROVEN** later; **DISPROVEN** as registrar |
| `004B4A10` | QuestManager activate | **PROVEN** path; **DISPROVEN** Oakvale literal **and** this pair |
| `0x012C5D14` | `Q_NewOakValeIntro` intern | **DISPROVEN** on this pair |
| `004F5C9E` / `004F5CC6` / `004F5CCD` | pair 63 `CCrateStackDef` | **PROVEN**; remaining-pairs **MATCH** |
| `004D8A6A` / `0123C86C` | pair 63 factory / vtbl; size 48 | **PROVEN** |
| `AddFirstDefClass` | stops at Fortieth `CMultiStaticMeshDef` | **MATCH** Fortieth; pair 62 **LEFTOVER** |
| Host `ActivateQuest("Q_NewOakValeIntro")` | must not be added | **DISPROVEN** |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00840000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00780000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\rtti.txt`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\abs.tsv`
- `C:\FableCSharp\proofs\004EE23F-remaining-pairs\README.md`
- `C:\FableCSharp\proofs\cactivatequestdef-payloads\README.md`
- `C:\FableCSharp\proofs\q-novi-activator-callers\README.md`
- `C:\FableCSharp\proofs\008421C0-activate\README.md`
- `C:\FableCSharp\proofs\qst-autostart-list\README.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs` (`AddFirstDefClass`) read only
