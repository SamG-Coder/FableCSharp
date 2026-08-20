# `004EE23F` remaining pairs 99–100: `CFishingRodDef` / `CRumbleDef`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` (`0042F44D`) →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD` → `00418585` `004EE23F`.
Do **not** invent class names: only
`push "…"` listing strings.
Do **not** invent `ActivateQuest`. Pair 62
`CActivateQuestDef` is earlier. This walk
does not queue `CCreatureAction_ActivateQuest`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: recover remaining-pairs rows
**99–100** (`CFishingRodDef` `004F7D9E`
factory `0x4D9321` sites `004F7DC6` /
`004F7DCD`; `CRumbleDef` `004F7F02`
factory `0x4E3290` sites `004F7F2A` /
`004F7F31`). For each: string, `0044C6B0`,
`009B0AC0`, factory, persist ctor, size,
vtbl, CTC. Childhood? **DISPROVEN**
expected.

| n | `0044C6B0` | `009B0AC0` | Factory | Size | Ctor | Vtbl | Class |
| --- | --- | --- | --- | ---: | --- | --- | --- |
| 99 | `004F7DC6` | `004F7DCD` | `004D9321` | **60** | `0044C0C0` in-line | **`0123DCA4`** | **PROVEN** |
| 100 | `004F7F2A` | `004F7F31` | `004E3290` | **64** | `jmp 004E1722` | **`0124273C`** | **PROVEN** |

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
(`004F7C79`…`004F8211`; factories
`004D9321` / `004E3290`; ctor `004E1722`;
placement `004D5F95`; size helpers
`004D5FA7` / `004E1744`);
`listing-00440000.txt` `0044C0C0`;
`listing-00400000.txt` `00431061`;
`listing-007c0000.txt` `007E2310` /
`007E2390` / `007F1810` / `007F1EE0`;
`proofs/004EE23F-remaining-pairs` rows
97–101; `proofs/004F5721-boss-fish-guard`
pair 59 `CFishingDef`.
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01243B6C` **`CFishingRodDef`**,
`0x01243B60` **`CRumbleDef`**.
`assembly/exe/00-index/vtbl.tsv`
`0x0123DCA4` / `0x0124273C`.
`rtti.txt` `.?AVCFishingRodDef@@` /
`.?AVCRumbleDef@@`.
`assembly/compiled-defs/game/entries.tsv`
indices **98** / **99**.

Both pairs are shape-2 (`push` + `0042DAE0`).
Listing strings are **not** invented.

Host `AddFirstDefClass` currently returns
after ninety-seventh `CBalverineBattleDef`.
Pair 98 `CAreaOfEffectAttackDef` is still
**UNREAD** in `src/`. Size+vtbl here
**MATCH** listing, but shipping 99–100
without 98 would skip a leftover pair
(**DIVERGE**). Proof only.

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| Row 99 name / sites / factory? | **`CFishingRodDef`** `004F7D9E` / `004F7DC6` / `004F7DCD` / `0x4D9321`. 1 CTC. | **PROVEN** |
| Row 99 size / ctor / vtbl? | **60**; `0044C0C0` then `[esi]=0123DCA4`; vtbl[20] `004D5FA7` `push 60`. Placement `004D5F95` (factory does **not** `jmp` it). | **PROVEN** size/vtbl; persist payload **UNREAD** |
| Row 100 name / sites / factory? | **`CRumbleDef`** `004F7F02` / `004F7F2A` / `004F7F31` / `0x4E3290`. 3 CTC. | **PROVEN** |
| Row 100 size / ctor / vtbl? | **64**; `jmp 004E1722`; `0044C0C0` then `[esi]=0124273C`; `+40` `004DFAC5`; `+52` `004DFAE3`; vtbl[20] `004E1744` `push 64`. | **PROVEN** size/vtbl; persist payload **UNREAD** |
| Remaining-pairs rows 99–100? | name / factory / sites / CTC counts | **MATCH** |
| Childhood Oakvale? | **No.** Init Thing Components class register. Not `00DBDE40`. Not `CFishingDef` (pair 59). Not a pond Thing. Not rumble-on-quest. | **DISPROVEN** |
| Invent `ActivateQuest` here? | **No.** | **DISPROVEN** |
| Next pair? | **101** `CShipDef` `004F81E2` / `004F820A` / `004F8211` factory `0x4D8799`. 7 CTC. | **PROVEN** sites |
| Host live objects? | **None.** Notes through 97 are Note-only + flags. Live 60 / 64-byte objects are **LEFTOVER**. | **PROVEN** leftover |

**Answer:** two leftover Add Def Class
pairs. Factories allocate 60 / 64.
`CFishingRodDef` is in-line `0044C0C0` +
vtbl write. `CRumbleDef` is the jmp-thunk
shape. Not Oakvale. Not a Thing instance.
Not a file I/O site. Not `ActivateQuest`.

Exact constants (do **not** wire until
pair 98 is recovered):

```
NinetyNinthDefClassSite    = 0x004F7DC6
NinetyNinthDefClassFactory = 0x004D9321
NinetyNinthDefClassCtor    = 0x0044C0C0
NinetyNinthDefClassVtbl    = 0x0123DCA4
NinetyNinthDefClassSize    = 60
NinetyNinthDefClassName    = "CFishingRodDef"

HundredthDefClassSite      = 0x004F7F2A
HundredthDefClassFactory   = 0x004E3290
HundredthDefClassCtor      = 0x004E1722
HundredthDefClassVtbl      = 0x0124273C
HundredthDefClassSize      = 64
HundredthDefClassName      = "CRumbleDef"
```

Pack helper is already
`NinthDefClassPackFn` `0x0042DAE0`.
`009B0AC0` sites: `004F7DCD` /
`004F7F31`.

---

## 1. Bound: pair 98 then one CTC

`listing-004c0000.txt` after 97
`CBalverineBattleDef` `004F7C79`:

One unnamed `004D2EF0` (`004F7CB1`,
factory `0x4D5F35`). Helper `004D5F52`
(called `004F7C97`) pushes
`"CTCTextureDecal"`. Remaining-pairs
counted that row unnamed.

Then pair 98 `CAreaOfEffectAttackDef`
`004F7CF4` / `004F7D1C` / `004F7D23`
factory `0x4E6CF3` (**PROVEN** sites
here; factory body **UNREAD** in this
proof).

Then one unnamed `004D2EF0` between 98
and 99:

| Helper at | `push` factory | Helper `push "…"` |
| --- | --- | --- |
| `004F7D41` `004D5F82` | `0x4D5F65` `004F7D5B` | `"CTCFishingRod"` |

`004D5F65` is `00BFEA1A(16)` then
`007E2450`. Remaining-pairs CTC
between = 1. **MATCH**. Helper name is
**not** invented from `004Dxxxx`; it is
`push "…"` in the helper body. In-range
`004EE23F` does **not** push it.

Then the ninety-ninth pair.

---

## 2. Pair 99 — `CFishingRodDef`

```
004F7D9E  push "CFishingRodDef"
004F7DA3  lea ecx, [ebp-1588]
004F7DA9  call 0099EBF0
004F7DAE  push 0x4D9321
004F7DB3  lea eax, [ebp-1588]
004F7DB9  push eax
004F7DBA  lea ecx, [ebp-2380]
004F7DC0  call 0042DAE0
004F7DC5  push eax
004F7DC6  call 0044C6B0
004F7DCB  mov ecx, eax
004F7DCD  call 009B0AC0
```

`strings.tsv` `0x01243B6C` **`CFishingRodDef`**.
`xrefs.tsv` `0x01243B6C`:

| Site | Fn | Role |
| --- | --- | --- |
| `004F7D9F` | `004EE137` (greedy parent of `004EE23F`) | this registrar |
| `007E2314` | `007E2310` | later type-name intern |
| `007E2398` | `007E2390` | later typed HANDLE get |

`abs.tsv` `0x004F7DAE` → `0x004D9321`.
`0042DAE0` is the name+factory pack
helper. Treating it as `009B0AC0` is
**DISPROVEN** (remaining-pairs §2).

```
004D9321  push esi
          push 60
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D9341
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123DCA4
          mov eax, esi
          pop esi
          ret
004D9341  xor eax, eax
          pop esi
          ret
```

No extra stores after the vtbl write.
Placement ctor `004D5F95` is the same
`0044C0C0` + `0123DCA4` (factory does
**not** `jmp` it). Same in-line shape as
sixty-ninth `CGoldDef` `004D8EC5` /
eighty-first `CFishDef` `004D910C`.

`vtbl.tsv` `0x0123DCA4`:

| slot | VA | note |
| --: | --- | --- |
| 0 | `004D9345` | dtor (`[esi]=0x1230BA0` then `009FC550`) |
| 1–17 / 21–24 | shared `0042D930`…`0042DAA0` / `009ACE90` / `009FBEF0` / `009ACAB0` / `009ACB20` family | no invented names |
| **18** | **`004DF107`** | persist: five `00431061` f32 at `+40`…`+56` |
| 19 | `004E159B` → `004E15A0` | copy five dwords `+40`…`+56` after `00431F10` |
| **20** | **`004D5FA7`** | size `push 60; pop eax; ret` |

Factory `push 60` **MATCH**es vtbl[20]
`push 60`. Size **60**.

Slot 18 persist `004DF107`:

```
004DF107  push esi
          mov esi, ecx
          push edi
          mov edi, [esp+12]
          lea eax, [esi+40]
          push eax
          mov ecx, edi
          call 00431061
          lea eax, [esi+44]
          push eax
          mov ecx, edi
          call 00431061
          lea eax, [esi+48]
          push eax
          mov ecx, edi
          call 00431061
          lea eax, [esi+52]
          push eax
          mov ecx, edi
          call 00431061
          add esi, 56
          push esi
          mov ecx, edi
          call 00431061
          pop edi
          pop esi
          ret 4
```

`00431061` is f32 persist (`fldz` /
`00993EE0`; sibling
`004F3630-boast-volume`). Five floats =
20 bytes; `40+20=60`. Factory does not
store `+40`…`+56`. Lionhead field names
**UNREAD**.

`rtti.txt` `0x013799B0` `CFishingRodDef`.
Later leftover (not this register):
`007E2310` type-name (`push -1` /
`"CFishingRodDef"` / `0099EBF0`);
`007E2390` typed HANDLE get
(`[vtbl+56]` → `009ADA10` family).

`game.bin`: **4** rows (INDEX count
**MATCH**). Id **98** =
`NULLDEF_CFishingRodDef` raw **43**
subdefs **0**; three live type-name rows
`12878` / `12885` / `13383` (raw **43**,
neighbours `CInventoryItemDef` /
`CStockItemDef` / `CCarryableDef` /
`CCarryingDef` / `CContextSensitiveItemDef`).
Not `CFishingDef` (pair 59, id **58**,
`NULLDEF_CFishingDef`). Not a childhood
pond Thing.

---

## 3. Three CTC between 99 and 100

After `CFishingRodDef` `004F7DCD`, three
unnamed `004D2EF0`. Helper listing
strings (same file, other fns;
`004EE23F` itself does **not** push
them):

| Helper at | `push` factory | Helper `push "…"` |
| --- | --- | --- |
| `004F7DEB` `004D5FC8` | `0x4D5FAB` `004F7E05` | `"CTCBoastUI"` |
| `004F7E4A` `004D3EE6` | `0x4D3EC9` `004F7E64` | `"CTCThingUIDDummyForSerialisation"` |
| `004F7EA9` `004D5FF2` | `0x4DCDC4` `004F7EBF` | `"CTCDRumble"` |

Third row skips `006869C0` and
`push 28` (immediate) then factory.
Still a `004D2EF0` CTC row. Remaining-pairs
CTC between = 3. **MATCH**.

`"CTCBoastUI"` is **not** pair 44
`CBoastingPodiumDef`. `"CTCDRumble"` is
**not** `CRumbleDef` (that is the next
Def pair). Do not promote helper strings
as registrar names.

Then the hundredth pair.

---

## 4. Pair 100 — `CRumbleDef`

```
004F7F02  push "CRumbleDef"
004F7F07  lea ecx, [ebp-1596]
004F7F0D  call 0099EBF0
004F7F12  push 0x4E3290
004F7F17  lea eax, [ebp-1596]
004F7F1D  push eax
004F7F1E  lea ecx, [ebp-2396]
004F7F24  call 0042DAE0
004F7F29  push eax
004F7F2A  call 0044C6B0
004F7F2F  mov ecx, eax
004F7F31  call 009B0AC0
```

`strings.tsv` `0x01243B60` **`CRumbleDef`**.
`xrefs.tsv` `0x01243B60`:

| Site | Fn | Role |
| --- | --- | --- |
| `004F7F03` | `004EE137` | this registrar |
| `007F1814` | `007F1810` | later type-name intern |
| `007F1EE8` | `007F1EE0` | later typed HANDLE get |

`abs.tsv` `0x004F7F12` → `0x004E3290`.

Jmp-thunk shape (same as nineteenth
`004E0B4B` / seventy-second `004E2D3B`):

```
004E3290  push 64
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004E32A3
          mov ecx, eax
          jmp 004E1722
004E32A3  xor eax, eax
          ret

004E1722  push esi
          mov esi, ecx
          call 0044C0C0
          lea ecx, [esi+40]
          mov [esi], 0x124273C
          call 004DFAC5
          lea ecx, [esi+52]
          call 004DFAE3
          mov eax, esi
          pop esi
          ret

004E1744  push 64
          pop eax
          ret
```

`vtbl.tsv` `0x0124273C`:

| slot | VA | note |
| --: | --- | --- |
| 0 | `004E32A6` | dtor → `004E32C2`: `+52` `004DD5EE`, `+40` `004DD5AD`, `[esi]=01230BA0`, `009FC550` |
| 1–17 / 21–24 | shared `0042D930`…`0042DAA0` family | no invented names |
| **18** | **`004E676A`** | persist: `+40` `004E678B`, `+52` `004E6A26` |
| 19 | `004E64EE` → `004E64F3` | copy `+40` `004E6520`, `+52` `004E6645` after `00431F10` |
| **20** | **`004E1744`** | size `push 64; pop eax; ret` |

Factory `push 64` **MATCH**es vtbl[20]
`push 64`. Size **64**. Persist ctor is
the jmp dest `004E1722` (not the in-line
`0044C0C0` itself).

`004DFAC5` / `004DFAE3` default-construct
the two trailing 12-byte containers
(`004DD583` / `004DD5C4` empty-list
headers). `40+12+12=64`. Slot 18 persist
walks those containers (`004E678B`
starts with type tag `0x122D70E` /
`00404500`, same gate as `00431061`,
then list walk `004E67C0`). Inflated
field names **UNREAD**.

`rtti.txt` `0x0137AFF0` `CRumbleDef`.
Later leftover: `007F1810` type-name;
`007F1EE0` typed get (`[eax+56]`).

`game.bin`: **2** rows (INDEX count
**MATCH**). Id **99** =
`NULLDEF_CRumbleDef` raw **19** subdefs
**0**; one live row `14655` (raw **83**,
neighbours `CPhysicsDef` /
`CTargetingDef` / `CAppearanceDef`).
Not a rumble-on-Oakvale-quest object.

---

## 5. Seven CTC then pair 101 `CShipDef`

After `CRumbleDef` `004F7F31`, seven
unnamed `004D2EF0`. Helper listing
strings:

| Helper at | `push` factory | Helper `push "…"` |
| --- | --- | --- |
| `004F7F4F` `004D5FDB` | `0x4E15D3` `004F7F65` (`push 29`) | `"CTCDPhysicalObstruction"` |
| `004F7FAA` `004D6026` | `0x4D6009` `004F7FC4` | `"CTCDiggingSpot"` |
| `004F8009` `004D6069` | `0x4DCE1D` `004F8023` | `"CTCDiceGame"` |
| `004F8068` `004D608F` | `0x4DCE3A` `004F8082` | `"CTCPlot"` |
| `004F80C7` `004D60D2` | `0x4D9367` `004F80E1` | `"CTCSleep"` |
| `004F8126` `004D60BF` | `0x4D60A2` `004F8140` | `"CTCFineDialogue"` |
| `004F8185` `004D48BE` | `0x4D48A1` `004F819F` | `"CTCBob"` |

Remaining-pairs CTC between 100 and 101
= 7. **MATCH**.

Then:

```
004F81E2  push "CShipDef"
004F81F2  push 0x4D8799
004F8204  call 0042DAE0
004F820A  call 0044C6B0
004F8211  call 009B0AC0
```

Pair 101 factory body **UNREAD** here.
Zero CTC between 101 `CShipDef` and 102
`CShopItemDef` (remaining-pairs cluster).

---

## 6. Childhood / `ActivateQuest` — DISPROVEN

`004EE23F` at these pairs:

1. `0099EBF0` name `"CFishingRodDef"` /
   `"CRumbleDef"`.
2. `0042DAE0` packs factory.
3. `0044C6B0` `004F7DC6` / `004F7F2A`.
4. `009B0AC0` `004F7DCD` / `004F7F31`
   Add Def Class.

That inserts type records so later
LoadDef can construct 60-byte /
64-byte defs. Same walk as
`CHeroMorphDef` … `CHasNameDef`.

No `00DBDE40`. No `Q_NewOakValeIntro`.
No `S_QNOVI`. No `00843F50` /
`00843FC0` / `004B4A10`. Pair 59
`CFishingDef` is the hero fishing
component (sibling
`004F5721-boss-fish-guard`); this is
the **rod** def class, not a pond
instance and not `Expression_Fish`.

`strings.tsv` `ActivateQuest` is the
console registrar only (`00419D90`).
**Not** these pairs.

---

## 7. Host leftover

`EngineLifecycle.AddFirstDefClass`
returns after ninety-seventh
`CBalverineBattleDef` `004F7C72`.

No `CAreaOfEffectAttackDef`. No
`CFishingRodDef`. No `CRumbleDef`.
No `0x4D9321` / `0x4E3290`.

| If host adds… | Leftover is… |
| --- | --- |
| through 97 (current) | 98 `CAreaOfEffectAttackDef`, then these two, then 101…111 |
| Note-only 99–100 without 98 | **DIVERGE** (skip `004F7D1C`) |
| live Add Def Class for 99–100 | still live `009AD6E0` / `009FC4F0` on each object (**not** MATCH) |

Size+vtbl **MATCH** listing. Do **not**
ship `src/` until pair 98 is recovered
in order.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `004F7D9E` / `004F7DC6` / `004F7DCD` | pair 99 `CFishingRodDef` | **PROVEN** leftover |
| `004D9321` | factory `00BFEA1A(60)` in-line `0044C0C0` vtbl `0123DCA4` | **PROVEN** |
| `004D5F95` / `004D5FA7` | placement / size 60 | **PROVEN** |
| `0123DCA4` | def vtbl; slot 18 `004DF107` five f32 | **PROVEN** size/vtbl; payload **UNREAD** |
| `004F7F02` / `004F7F2A` / `004F7F31` | pair 100 `CRumbleDef` | **PROVEN** leftover |
| `004E3290` | factory `00BFEA1A(64)` `jmp 004E1722` | **PROVEN** |
| `004E1722` / `004E1744` | persist ctor / size 64 | **PROVEN** |
| `0124273C` | def vtbl; slot 18 `004E676A` two containers | **PROVEN** size/vtbl; payload **UNREAD** |
| `004F7D5B` / `004F7E05` / `004F7E64` / `004F7EBF` | CTC rows (1+3) | **PROVEN** count; names from helpers, not in-range |
| `004F820A` / `004F8211` | next pair 101 `CShipDef` | **PROVEN** sites |
| `00DBDE40` | Oakvale | **DISPROVEN** here |
| `ActivateQuest` / `00843F50` | quest activate | **DISPROVEN** here |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-007c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\rtti.txt`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\abs.tsv`
- `C:\FableCSharp\assembly\compiled-defs\game\entries.tsv`
- `C:\FableCSharp\proofs\004EE23F-remaining-pairs\README.md`
- `C:\FableCSharp\proofs\004F5721-boss-fish-guard\README.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs` (`AddFirstDefClass` through 97)
