# `004EE23F` remaining pairs 64–68: overhead / tavern / augmentations / drunkenness

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI` /
`BUILDING_OAKVALE_TAVERN`. After Leave
this walk is `FinalAlbion.wld` (`0042F44D`) →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD` → `00418585` `004EE23F`.
Do **not** invent class names: only
`push "…"` listing strings.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: recover remaining-pairs rows
**64–68** (`COverheadDisplayDef` …
`CDrunkennessDef`). For each factory:
size, ctor, vtbl. Oakvale childhood tavern?

| n | `0044C6B0` | `009B0AC0` | Factory | Size | Ctor | Vtbl | Class |
| --- | --- | --- | --- | ---: | --- | --- | --- |
| 64 | `004F5D7C` | `004F5D83` | `004D8AB0` | **40** | `0044C0C0` in-line | **`0123C8FC`** | **PROVEN** |
| 65 | `004F5E32` | `004F5E39` | `004D8AF6` | **39** | `0044C0C0` in-line | **`0123C964`** | **PROVEN** |
| 66 | `004F6029` | `004F6030` | `004D8BE1` | **44** | `0044C0C0` in-line | **`0123CA8C`** | **PROVEN** |
| 67 | `004F60DF` | `004F60E6` | `004EC526` | **140** (`0x8C`) | `jmp 004EBBA3` | **`01243974`** | **PROVEN** |
| 68 | `004F63AC` | `004F63B3` | `004D8C91` | **44** | `0044C0C0` in-line | **`0123CB3C`** | **PROVEN** |

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
(`004F5D54`…`004F63B3`; factories
`004D8AB0` / `004D8AF6` / `004D8BE1` /
`004EC526` / `004D8C91`; ctor `004EBBA3`;
size helpers `004D5128` / `004D513E` /
`004D5211` / `004EBBEC` / `004D52FA`);
`listing-00440000.txt` `0044C0C0`;
`proofs/004EE23F-remaining-pairs` rows 64–68;
`proofs/004EE23F-twentyfirst-class`.
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01243E1C` **`COverheadDisplayDef`**,
`0x01243E0C` **`CTavernTableDef`**,
`0x01243E00` **`CTavernDef`**,
`0x0122FF9C` **`CObjectAugmentationsDef`**,
`0x01243DF0` **`CDrunkennessDef`**.
`assembly/exe/00-index/vtbl.tsv` the five
vtbls. `rtti.txt` `.?AVC…Def@@`.

All five are shape-2 (`push` + `0042DAE0`).
Listing strings are **not** invented.

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| Row 64 name / sites / factory? | **`COverheadDisplayDef`** `004F5D54` / `004F5D7C` / `004F5D83` / `0x4D8AB0`. | **PROVEN** |
| Row 64 size / ctor / vtbl? | **40**; `0044C0C0` then `[esi]=0123C8FC`; vtbl[20] `004D5128` `push 40`. Three packed bools `+37…+39`. | **PROVEN** |
| Row 65 name / sites / factory? | **`CTavernTableDef`** `004F5E0A` / `004F5E32` / `004F5E39` / `0x4D8AF6`. | **PROVEN** |
| Row 65 size / ctor / vtbl? | **39**; `0044C0C0` then `[esi]=0123C964`; vtbl[20] `004D513E` `push 39`. Two packed bools `+37…+38`. | **PROVEN** |
| Row 66 name / sites / factory? | **`CTavernDef`** `004F6001` / `004F6029` / `004F6030` / `0x4D8BE1`. | **PROVEN** |
| Row 66 size / ctor / vtbl? | **44**; `0044C0C0` then `[esi]=0123CA8C`; vtbl[20] `004D5211` `push 44`. Persist dword `+40` (CString intern `00431102`). | **PROVEN** size/vtbl; `+40` payload **UNREAD** |
| Row 67 name / sites / factory? | **`CObjectAugmentationsDef`** `004F60B7` / `004F60DF` / `004F60E6` / `0x4EC526`. | **PROVEN** |
| Row 67 size / ctor / vtbl? | **140** (`push 0x8C`); `jmp 004EBBA3`; vtbl **`01243974`**; vtbl[20] `004EBBEC` `mov eax, 0x8C`. | **PROVEN** |
| Row 68 name / sites / factory? | **`CDrunkennessDef`** `004F6384` / `004F63AC` / `004F63B3` / `0x4D8C91`. | **PROVEN** |
| Row 68 size / ctor / vtbl? | **44**; `0044C0C0` then `[esi]=0123CB3C`; vtbl[20] `004D52FA` `push 44`. Persist float `+40` **`DrunkennessThresholdMult`**. | **PROVEN** |
| Oakvale childhood tavern? | **No.** This is Init Thing Components class register. Not `00DBDE40`. Not `BUILDING_OAKVALE_TAVERN`. Not `StartOakValeWest` / `HerosOldHouse`. | **DISPROVEN** |
| Host live objects? | **None.** `AddFirstDefClass` returns after 21st (`CBedDef`). Rows 64–68 are **LEFTOVER**. | **PROVEN** leftover |

**Answer:** five leftover Add Def Class
pairs. Factories allocate 40 / 39 / 44 /
140 / 44. Four in-line `0044C0C0` + vtbl
write; `CObjectAugmentationsDef` is the
jmp-thunk shape. Not Oakvale. Not a
Thing instance. Not a file I/O site.

---

## 1. Pair 64 — `COverheadDisplayDef`

`listing-004c0000.txt` after 63
`CCrateStackDef` `004F5CCD`:

One unnamed `004D2EF0` row (`push 0x4D50E6`
at `004F5D00`, helper `004D5103` at
`004F5CEE` pushes `"CTCOverheadDisplay"`).
Then the sixty-fourth pair.

`004F5D54` `push 0x01243E1C`. `strings.tsv`:

```
0x01243E1C	0xE43E1C	COverheadDisplayDef
```

```
004F5D54  push "COverheadDisplayDef"
004F5D64  push 0x4D8AB0
004F5D76  call 0042DAE0
004F5D7C  call 0044C6B0
004F5D83  call 009B0AC0
```

```
004D8AB0  push esi
          push 40
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D8AD0
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123C8FC
          mov eax, esi
          pop esi
          ret
004D8AD0  xor eax, eax
          pop esi
          ret
```

No extra stores after the vtbl write.
Placement ctor `004D5116` is the same
`0044C0C0` + `0123C8FC` (factory does
not `jmp` it).

`vtbl.tsv` `0x0123C8FC` slot 20 is
`004D5128`. Listing:

```
004D5128  push 40
          pop eax
          ret
```

Slot 0 is `004D8AD4` (`mov [esi], 0x1230BA0`
then `009FC550`). Slots 1–17 / 21–24 are
the shared `0042D930`…`0042DAA0` /
`009ACE90` / `009FBEF0` / `009ACAB0` /
`009ACB20` family. Slot 18 persist
`007B7D20` writes three bytes at
`+37` / `+38` / `+39` (`00404500` +
`00403EB0` / `00993E30`). Slot 19 copy
`004E0EB7` copies those three bytes
after `00431F10`. Matches size 40.

`rtti.txt` `0x01379558` `COverheadDisplayDef`.
Later leftover (not this register):
`007B72A0` type-name (`push -1` /
`"COverheadDisplayDef"` / `0099EBF0`);
`007B7F50` typed HANDLE get
(`[vtbl+56]` → `009ADA10`), same shape
as `0042B0A2` / `0042AF9E`.

`game.bin`: **3** rows, raw **18**,
subdefs **0**. Id **63** =
`NULLDEF_COverheadDisplayDef`; **8906**
and **10517** type name only.

---

## 2. Pair 65 — `CTavernTableDef`

One unnamed `004D2EF0` after 64
(`push 0x4D5142` at `004F5DB6`, helper
`004D515F` at `004F5DA4` pushes
`"CTCTavernTable"`). Then:

```
004F5E0A  push "CTavernTableDef"
004F5E1A  push 0x4D8AF6
004F5E2C  call 0042DAE0
004F5E32  call 0044C6B0
004F5E39  call 009B0AC0
```

`strings.tsv` `0x01243E0C` **`CTavernTableDef`**.

```
004D8AF6  push esi
          push 39
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D8B16
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123C964
          mov eax, esi
          pop esi
          ret
004D8B16  xor eax, eax
          pop esi
          ret
```

Size **39** is not a decode error.
`0044C0C0` touches `[esi+36]` (byte
flags). Packed fields start at `+37`.
Two bools → object ends at `+38`
inclusive = 39 bytes.

`vtbl.tsv` `0x0123C964` slot 20 is
`004D513E`:

```
004D513E  push 39
          pop eax
          ret
```

Placement ctor `004D512C` writes the
same vtbl. Slot 18 persist `004DE9D2`
is two `0043314A` bools at `+37` then
`+38`. Slot 19 copy `004E0EE3` copies
those two bytes. Three witnesses
agree on size 39.

`rtti.txt` `0x0137957C` `CTavernTableDef`.
Later leftover: `007B8120` type-name;
`007B9700` typed get.

`game.bin`: **12** rows, raw **13**,
subdefs **0**. Id **64** = `NULLDEF`;
eleven unnamed type-name rows.

---

## 3. Pair 66 — `CTavernDef`

Four unnamed `004D2EF0` after 65.
Helper listing strings (same file,
other fns; `004EE23F` itself does
**not** push them):

| Helper at | `push` factory | Helper `push "…"` |
| --- | --- | --- |
| `004F5E5A` `004D518F` | `0x4D5172` | `"CTCTavernBar"` |
| `004F5EC5` `004D51A2` | `0x4DB124` | `"CTCTavernTankard"` |
| `004F5F30` `004D51B5` | `0x4D8BA8` | `"CTCTavernJug"` |
| `004F5F9B` `004D5232` | `0x4D5215` | `"CTCTavern"` |

Then:

```
004F6001  push "CTavernDef"
004F6011  push 0x4D8BE1
004F6023  call 0042DAE0
004F6029  call 0044C6B0
004F6030  call 009B0AC0
```

`strings.tsv` `0x01243E00` **`CTavernDef`**.

```
004D8BE1  push esi
          push 44
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D8C01
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123CA8C
          mov eax, esi
          pop esi
          ret
004D8C01  xor eax, eax
          pop esi
          ret
```

No `+40` store in the factory. Size 44
is alloc + vtbl[20] only at construct.

`vtbl.tsv` `0x0123CA8C` slot 20 is
`004D5211`:

```
004D5211  push 44
          pop eax
          ret
```

Placement ctor `004D51FF` writes the
same vtbl. Slot 18 persist `004DE9F3`
is `add ecx, 40` then `00431102`
(CString intern). Slot 19 copy
`004E0F02` copies dword `[+40]`.
Intern payload **UNREAD** here.

`rtti.txt` `0x013795B8` `CTavernDef`.
Later leftover: `007EBCD0` type-name;
`007ECD00` typed get.

`game.bin`: **7** rows, raw **11**,
subdefs **0**. Id **65** = `NULLDEF`;
six unnamed type-name rows (**8894**,
**9370**, **9437**, **9501**, **9564**,
**9628**). Not `BUILDING_OAKVALE_TAVERN`
(that is type **BUILDING**, id **757**,
mesh **6913**, raw **538**, subdefs
**10**).

---

## 4. Pair 67 — `CObjectAugmentationsDef`

One unnamed `004D2EF0` after 66
(`push 0x4D3CC1` at `004F6063`, helper
`004D3CDE` at `004F6051` pushes
`"CTCObjectAugmentations"`). Then:

```
004F60B7  push "CObjectAugmentationsDef"
004F60C7  push 0x4EC526
004F60D9  call 0042DAE0
004F60DF  call 0044C6B0
004F60E6  call 009B0AC0
```

`strings.tsv` `0x0122FF9C`
**`CObjectAugmentationsDef`**. Same
string as `0042AF9E` (later leftover
typed HANDLE get, not this register).

Jmp-thunk shape (same as nineteenth
`004E0B4B` / twenty-first `004DA7F3`):

```
004EC526  push 0x8C
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004EC53C
          mov ecx, eax
          jmp 004EBBA3
004EC53C  xor eax, eax
          ret

004EBBA3  push esi
          mov esi, ecx
          call 0044C0C0
          lea ecx, [esi+48]
          mov [esi], 0x1243974
          call 004EBAB0
          xor eax, eax
          mov [esi+64], eax
          mov [esi+68], eax
          mov [esi+72], eax
          mov [esi+76], eax
          mov [esi+80], eax
          lea ecx, [esi+88]
          mov [esi+84], eax
          call 004DF8AD
          lea ecx, [esi+100]
          call 004DF8AD
          lea ecx, [esi+136]
          call 0099E4B0
          mov eax, esi
          pop esi
          ret

004EBBEC  mov eax, 0x8C
          ret
```

`vtbl.tsv` `0x01243974` slot 20 is
`004EBBEC` (size **140**). Slot 0
`004ECAE1`. Slot 18 persist `004EC53F`
reads `+40` / `+44` (`00431102`
CStrings), then `+48` (`004EC825`),
`+60` (`00431020`), `+64` / `+76`
(`00466A47`), `+88` / `+100`
(`004EC5FA`), `+112…+132` (`00431102`).
Ctor does **not** store `+40` / `+44`;
those wait for LoadDef.

`rtti.txt` `0x0137B57C`
`CObjectAugmentationsDef`. Later
leftover: `0042AF9E` /
`0041CC70` typed get.

`game.bin`: **66** rows. `NULLDEF` raw
**120**; live rows 524 / 532 / 604
(ASCII `V3r#|6iit`, some
`weapon_pos_a`). Inflated field walk
**UNREAD** here.

---

## 5. Pair 68 — `CDrunkennessDef`

Six unnamed `004D2EF0` after 67.
Helper listing strings:

| Helper at | `push` factory | Helper `push "…"` |
| --- | --- | --- |
| `004F6107` `004D5262` | `0x4D5245` | `"CTCFollowed"` |
| `004F6172` `004D52C2` | `0x4D52A5` | `"CTCTrespasser"` |
| `004F61DD` `004D5292` | `0x4D5275` | `"CTCTrespassable"` |
| `004F6248` `004D52D5` | `0x4DB141` | `"CTCGossip"` |
| `004F62B3` `004D5710` | `0x4D56F3` | `"CTCHitOnCollision"` |
| `004F631E` `004D531B` | `0x4D52FE` | `"CTCDrunkenness"` |

Then:

```
004F6384  push "CDrunkennessDef"
004F6394  push 0x4D8C91
004F63A6  call 0042DAE0
004F63AC  call 0044C6B0
004F63B3  call 009B0AC0
```

`strings.tsv` `0x01243DF0` **`CDrunkennessDef`**.

```
004D8C91  push esi
          push 44
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D8CB1
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123CB3C
          mov eax, esi
          pop esi
          ret
004D8CB1  xor eax, eax
          pop esi
          ret
```

`vtbl.tsv` `0x0123CB3C` slot 20 is
`004D52FA`:

```
004D52FA  push 44
          pop eax
          ret
```

Placement ctor `004D52E8` writes the
same vtbl. Slot 18 persist `004DC96F`:

```
004DC96F  add ecx, 40
          push "DrunkennessThresholdMult"
          call 00410620
```

`strings.tsv` `0x01240CA8`
**`DrunkennessThresholdMult`**. Slot 19
copy `004E0F1B` copies dword `[+40]`.
Factory does not store `+40`.

`rtti.txt` `0x013795D4` `CDrunkennessDef`.
Later leftover: `007BB720` type-name;
`007BB920` typed get.

`game.bin`: **4** rows, raw **11**,
subdefs **0**. Id **67** = `NULLDEF`;
three unnamed type-name rows.

After this pair: 9 unnamed `004D2EF0`
then remaining-pairs row 69 `CGoldDef`
`004F67BA` / `004F67C1` factory
`0x4D8EC5` (name/sites **PROVEN** in
remaining-pairs; factory body **UNREAD**
here). First CTC helper after 68 is
`004D532E` `"CTCOnHeroPush"` at
`004F63D4`.

---

## 6. Base `0044C0C0` (all five)

`listing-00440000.txt`:

```
0044C0C0  push esi
          mov esi, ecx
          call 009FBEC0
          and [esi+36], 0xF8
          mov [esi+28], 0x0
          mov [esi], 0x1231D54
          mov eax, esi
          pop esi
          ret
```

Derived factory then overwrites `[esi]`
with the class vtbl. Packed bools live
at `+37` onward. Dword fields start at
`+40`. That is why 39 (two bools) and
40 (three bools) sit next to 44
(one dword at `+40`).

---

## 7. Oakvale childhood tavern — DISPROVEN

This walk is `004EE23F` Init Thing
Components. Remaining-pairs already
locked: **not** Oakvale.

| Claim | Status |
| --- | --- |
| Pairs 64–68 run on `00DBDE40` / `Q_NewOakValeIntro` | **DISPROVEN** |
| These sites construct a tavern Thing | **DISPROVEN** (class register only) |
| `CTavernDef` **is** `BUILDING_OAKVALE_TAVERN` | **DISPROVEN** (BUILDING id **757**, not a Def pair) |
| No-save first playable is the Oakvale tavern | **DISPROVEN** (LookoutPoint / GuildArrivalHSP) |
| Childhood intro view is the tavern | **DISPROVEN** (`StartOakValeWest` / `HerosOldHouse` / `CAM_OVIF_SHOT2`) |
| `BUILDING_OAKVALE_TAVERN` exists in `game.bin` | **PROVEN** later leftover (id **757**) |
| `OBJECT_OAKVALE_TAVERN_BAR` exists | **PROVEN** later leftover (id **3236**) |
| `SIM_BUILDING_TAVERN_OAKVALE` exists | **PROVEN** name only; not this register |
| `CTavernGameDef` is this cluster | **DISPROVEN** (remaining-pairs row **72**) |

`CTavernDef` / `CTavernTableDef` /
`CDrunkennessDef` are type registrars
for every tavern in Albion (Bowerstone,
Hook Coast, Snowspire, Bloodstone,
bandit camp, Oakvale adult, …). Oakvale
childhood does not special-case these
five `009B0AC0` sites.

---

## 8. Host

`EngineLifecycle.AddFirstDefClass` Notes
through twenty-first `CBedDef`
(`004F0E92` / `004DA7F3` / size 60 /
vtbl `0123E8BC`) then **returns**.

`src/` has **0** hits for these five
class names and **0** hits for
`004F5D7C` / `004D8AB0`.

| After 21st | Native | Host |
| --- | --- | --- |
| rows 22…63 | remaining-pairs | **LEFTOVER** |
| 64 `COverheadDisplayDef` size 40 vtbl `0123C8FC` | **PROVEN** (this file) | **LEFTOVER** |
| 65 `CTavernTableDef` size 39 vtbl `0123C964` | **PROVEN** | **LEFTOVER** |
| 66 `CTavernDef` size 44 vtbl `0123CA8C` | **PROVEN** | **LEFTOVER** |
| 67 `CObjectAugmentationsDef` size 140 vtbl `01243974` | **PROVEN** | **LEFTOVER** |
| 68 `CDrunkennessDef` size 44 vtbl `0123CB3C` | **PROVEN** | **LEFTOVER** |
| 69 `CGoldDef` … 111 `CHasNameDef` | remaining-pairs | **LEFTOVER** |

Live 40 / 39 / 44 / 140 / 44-byte
objects are **LEFTOVER**. `+37…+39`
bools, `+40` intern / float, and the
augmentation tables are **UNREAD** in
the host (there is no object).

---

## Original

Sixty-fourth through sixty-eighth Add
Def Class on `004EE23F`:

1. `0099EBF0` name (five listing strings).
2. `0042DAE0` packs factory.
3. `0044C6B0` / `009B0AC0` at the five
   site pairs.
4. Four factories: `00BFEA1A` +
   `0044C0C0` + vtbl. One factory:
   `00BFEA1A(140)` + `jmp 004EBBA3`.

CTC between (remaining-pairs counts;
helper names from the helper `push`
in this listing, not from
`004EE23F` itself): 1 + 1 + 4 + 1 + 6.

Not Oakvale. Not a Thing. Not file I/O.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00440000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00780000.txt` (`007B7D20` / `007B72A0` / `007B7F50`)
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-007c0000.txt` (`007EBCD0`)
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\assembly\exe\00-index\rtti.txt`
- `C:\FableCSharp\assembly\compiled-defs\game\entries.tsv`
- `C:\FableCSharp\assembly\compiled-defs\game\INDEX.md`
- `C:\FableCSharp\assembly\compiled-defs\names.tsv`
- `C:\FableCSharp\proofs\004EE23F-remaining-pairs\README.md`
- `C:\FableCSharp\proofs\004EE23F-twentyfirst-class\README.md`
