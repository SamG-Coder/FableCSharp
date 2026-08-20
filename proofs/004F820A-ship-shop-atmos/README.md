# `004EE23F` pairs 101–103: `CShipDef` / `CShopItemDef` / `CSoundAtmospheresDef`

Investigation lock after listing recover. Host
Notes + `*DefClassRegistered` flags. Not a
live 68 / 72 / 52-byte object.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` (`0042F44D`) →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD` → `00418585` `004EE23F`.
Do **not** invent class names: only
`push "…"` listing strings in
`004EE932`…`004F9144`. CTC helper
`push "…"` bodies are out of range
(remaining-pairs counted those rows
unnamed). Do **not** invent
`ActivateQuest`.

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**MATCH**.

Question: recover remaining-pairs **101–103**
(`CShipDef` `004F81E2` factory `0x4D8799`
sites `004F820A` / `004F8211`;
`CShopItemDef` `004F822D` factory `0x4D8411`
sites `004F8255` / `004F825C`;
`CSoundAtmospheresDef` `004F8278` factory
`0x4E32E3` sites `004F82A0` / `004F82A7`).
For each factory: persist ctor, size, vtbl.

| n | `0044C6B0` | `009B0AC0` | Factory | Size | Ctor | Vtbl | Class |
| --- | --- | --- | --- | ---: | --- | --- | --- |
| 101 | `004F820A` | `004F8211` | `004D8799` | **68** | `0044C0C0` in-line | **`0123C0A4`** | **PROVEN** |
| 102 | `004F8255` | `004F825C` | `004D8411` | **72** | `jmp 004D405A` | **`0123B644`** | **PROVEN** |
| 103 | `004F82A0` | `004F82A7` | `004E32E3` | **52** | `jmp 004E1748` | **`012427A4`** | **PROVEN** |

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
(`004F7F31`…`004F83F8`; factories
`004D8799` / `004D8411` / `004E32E3`;
persist `004D405A` / `004E1748`;
size helpers `004D489D` / `004D4078` /
`004E1765`);
`listing-00400000.txt` `004331F9`;
`listing-00780000.txt` `00787720` /
`00787AE0`;
`listing-007c0000.txt` `007F52C0` /
`007F5560`;
`proofs/004EE23F-remaining-pairs` rows
100–104;
`proofs/004F7D1C-aoe-rumble` pair 100.
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01243B54` **`CShipDef`**,
`0x01243B44` **`CShopItemDef`**,
`0x01243B2C` **`CSoundAtmospheresDef`**.
`assembly/exe/00-index/vtbl.tsv`
`0x0123C0A4` / `0x0123B644` /
`0x012427A4`.
`rtti.txt` `0x013793EC` / `0x01379200` /
`0x0137B00C`.
`abs.tsv` `0x004F81F2` → `0x004D8799`,
`0x004F823D` → `0x004D8411`,
`0x004F8288` → `0x004E32E3`.

All three are shape-2 (`push` + `0042DAE0`).
Listing strings are **not** invented.

Zero-CTC cluster (remaining-pairs):
`CShopItemDef` / `CSoundAtmospheresDef`
(2 adjacent Def pairs, no `004D2EF0`
between). **MATCH**.

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| Row 101 name / sites / factory? | **`CShipDef`** `004F81E2` / `004F820A` / `004F8211` / `0x4D8799`. 7 CTC. | **PROVEN** |
| Row 101 size / ctor / vtbl? | **68**; `0044C0C0` then `[esi]=0123C0A4`; vtbl[20] `004D489D` `push 68`. Persist intern `+40`/`+44`/`+48`. | **PROVEN** size/vtbl; persist payload **UNREAD** |
| Row 102 name / sites / factory? | **`CShopItemDef`** `004F822D` / `004F8255` / `004F825C` / `0x4D8411`. 0 CTC. | **PROVEN** |
| Row 102 size / ctor / vtbl? | **72**; `jmp 004D405A`; `0044C0C0` then `[esi]=0123B644`; vtbl[20] `004D4078` `push 72`. | **PROVEN** |
| Row 103 name / sites / factory? | **`CSoundAtmospheresDef`** `004F8278` / `004F82A0` / `004F82A7` / `0x4E32E3`. 0 CTC. | **PROVEN** |
| Row 103 size / ctor / vtbl? | **52**; `jmp 004E1748`; `0044C0C0` then `[esi]=012427A4`; zeros `+40`/`+44`/`+48`; vtbl[20] `004E1765` `push 52`. | **PROVEN** |
| Childhood Oakvale / `ActivateQuest`? | **No.** Init Thing Components class register. Not `00DBDE40`. Not `00843F50`. | **DISPROVEN** |
| Host live objects? | **None.** Notes through 103 are Note-only + flags. | **PROVEN** leftover |

**Answer:** three leftover Add Def Class
pairs. Factories allocate 68 / 72 / 52.
`CShipDef` is in-line `0044C0C0` + vtbl
write; the other two `jmp` persist ctors.
Not Oakvale. Not a Thing instance. Not a
file I/O site. Not `ActivateQuest`.

---

## 1. Bound: pair 100 then seven CTC

`listing-004c0000.txt` after 100
`CRumbleDef` `004F7F31`:

Seven unnamed `004D2EF0` rows. Helper
listing strings (same file, other fns;
`004EE23F` itself does **not** push
them):

| Helper at | `push` factory | Helper `push "…"` |
| --- | --- | --- |
| `004F7F65` `004D5FDB` | `0x4E15D3` | `"CTCDPhysicalObstruction"` |
| `004F7FC4` `004D6026` | `0x4D6009` | `"CTCDiggingSpot"` |
| `004F8023` `004D6069` | `0x4DCE1D` | `"CTCDiceGame"` |
| `004F8082` `004D608F` | `0x4DCE3A` | `"CTCPlot"` |
| `004F80E1` `004D60D2` | `0x4D9367` | `"CTCSleep"` |
| `004F8140` `004D60BF` | `0x4D60A2` | `"CTCFineDialogue"` |
| `004F819F` `004D48BE` | `0x4D48A1` | `"CTCBob"` |

Remaining-pairs counted those seven
unnamed. Helper names are **not**
invented from `004Dxxxx`; they are
`push "…"` in the helper bodies.
**None** is `ActivateQuest` /
`CTCActionUseActivateQuest`.

Then the hundred-first pair.

---

## 2. Pair 101 — `CShipDef`

```
004F81E2  push "CShipDef"
004F81E7  lea ecx, [ebp-1604]
004F81ED  call 0099EBF0
004F81F2  push 0x4D8799
004F81F7  lea eax, [ebp-1604]
004F81FD  push eax
004F81FE  lea ecx, [ebp-2412]
004F8204  call 0042DAE0
004F8209  push eax
004F820A  call 0044C6B0
004F820F  mov ecx, eax
004F8211  call 009B0AC0
```

`strings.tsv` `0x01243B54` **`CShipDef`**.
Listing `004F81E2` `push "CShipDef"`.

```
004D8799  push esi
          push 68
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D87B9
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123C0A4
          mov eax, esi
          pop esi
          ret
004D87B9  xor eax, eax
          pop esi
          ret
```

No extra stores after the vtbl write.
Placement ctor `004D488B` is the same
`0044C0C0` + `0123C0A4` (factory does
**not** `jmp` it). Host ctor constant is
`0044C0C0`.

`vtbl.tsv` `0x0123C0A4` slot 20 is
`004D489D`:

```
004D489D  push 68
          pop eax
          ret
```

Slot 0 is `004D87BD` (`mov [esi], 0x1230BA0`
then `009FC550`). Slots 1–17 / 21–24 are
the shared `0042D930`…`0042DAA0` family.
Slot 18 persist `004DE782`: intern
`00431102` at `+40` / `+44` / `+48`, then
`00431061` at `+52` / `+56` / `+60` /
`+64`. Seven dwords after the 40-byte
base = size 68. Slot 19 copy `004E0A67`
copies those seven dwords. Factory does
not store them. Payload **UNREAD**.

`rtti.txt` `0x013793EC` `CShipDef`.
Later leftover (not this register):
`00787720` / `00787AE0` type-name intern
(`push -1` / `"CShipDef"` / `0099EBF0`).
**DISPROVEN** as this pair.

`game.bin`: id **100** =
`NULLDEF_CShipDef` raw **59**; later
unnamed type-name rows (`13737`,
`13779`, `13837`, `13838`). Not a
childhood ship spawn.

---

## 3. Pair 102 — `CShopItemDef`

Zero `004D2EF0` between 101 and 102.

```
004F822D  push "CShopItemDef"
004F8232  lea ecx, [ebp-1612]
004F8238  call 0099EBF0
004F823D  push 0x4D8411
004F8242  lea eax, [ebp-1612]
004F8248  push eax
004F8249  lea ecx, [ebp-2428]
004F824F  call 0042DAE0
004F8254  push eax
004F8255  call 0044C6B0
004F825A  mov ecx, eax
004F825C  call 009B0AC0
```

`strings.tsv` `0x01243B44` **`CShopItemDef`**.

```
004D8411  push 72
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004D8424
          mov ecx, eax
          jmp 004D405A
004D8424  xor eax, eax
          ret

004D405A  push esi
          mov esi, ecx
          call 0044C0C0
          lea ecx, [esi+44]
          mov [esi], 0x123B644
          call 004D4033
          and [esi+40], 0
          mov eax, esi
          pop esi
          ret
```

`004D4033` writes the `+44` block
(float zeros, `[+56]=1`, `[+48]=1.0f`).
`[+40]` is zeroed. Do not invent field
names.

`vtbl.tsv` `0x0123B644` slot 20 is
`004D4078`:

```
004D4078  push 72
          pop eax
          ret
```

Slot 0 `004D8427`. Slot 18 persist
`004DE3AF`: intern `00431102` at `+40`
and `+56`, `00431061` at `+44` / `+48` /
`+52` / `+60` / `+64` / `+68`. Eight
dwords after the 40-byte base = size 72.
Slot 19 copy `004E069F` copies `+40`
then `rep movsd` of 7 dwords from `+44`.
Payload **UNREAD**.

`rtti.txt` `0x01379200` `CShopItemDef`.
`xrefs.tsv` `0x01243B44` has **one** hit:
`004F822E` this registrar.

`game.bin`: id **101** =
`NULLDEF_CShopItemDef` raw **67**; later
unnamed type-name row `13343`. Not a
shop UI open.

Adjacent helper `"CTCShop"` /
`"CTCBlacksmiths"` live in `004D409C` /
`004D40AF` (out of `004EE23F` range).
Remaining-pairs does **not** count them
as in-range names.

---

## 4. Pair 103 — `CSoundAtmospheresDef`

Zero `004D2EF0` between 102 and 103.

```
004F8278  push "CSoundAtmospheresDef"
004F827D  lea ecx, [ebp-1620]
004F8283  call 0099EBF0
004F8288  push 0x4E32E3
004F828D  lea eax, [ebp-1620]
004F8293  push eax
004F8294  lea ecx, [ebp-2444]
004F829A  call 0042DAE0
004F829F  push eax
004F82A0  call 0044C6B0
004F82A5  mov ecx, eax
004F82A7  call 009B0AC0
```

`strings.tsv` `0x01243B2C`
**`CSoundAtmospheresDef`**.

```
004E32E3  push 52
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004E32F6
          mov ecx, eax
          jmp 004E1748
004E32F6  xor eax, eax
          ret

004E1748  push esi
          mov esi, ecx
          call 0044C0C0
          xor eax, eax
          mov [esi], 0x12427A4
          mov [esi+40], eax
          mov [esi+44], eax
          mov [esi+48], eax
          mov eax, esi
          pop esi
          ret
```

Three dwords after the 40-byte base =
size 52.

`vtbl.tsv` `0x012427A4` slot 20 is
`004E1765`:

```
004E1765  push 52
          pop eax
          ret
```

Slot 0 `004E32F9`. Slot 18 persist
`004EC153`:

```
004EC153  add ecx, 40
          push ecx
          mov ecx, [esp+8]
          call 004331F9
```

`004331F9` is a list walk (`00404500`
then `[esi+24]` branch). Payload
**UNREAD**. Slot 19 copy `004E3FD0`
copies `+40` via `00432EE9`.

`rtti.txt` `0x0137B00C`
`CSoundAtmospheresDef`. Later leftover:
`007F52C0` / `007F5560` type-name intern.
**DISPROVEN** as this pair.

`game.bin`: id **102** =
`NULLDEF_CSoundAtmospheresDef` raw **11**;
later named row `14703` **`STREETLIFE`**.
That named row is **not** this registrar
and is **not** Oakvale.

---

## 5. Next pair 104 then three CTC

After `004F82A7`, three unnamed
`004D2EF0` then `CNymphDef`
`004F83F8` / `004F8420` / `004F8427`
factory `0x4D93A0`. Remaining-pairs row
104 CTC between = 3. **MATCH**.

| Helper at | `push` factory | Helper `push "…"` |
| --- | --- | --- |
| `004F82DF` `004D6122` | `0x4DF1CC` | `"CTCSoundAtmosphereVillage"` |
| `004F8344` `004D6152` | `0x4D6135` | `"CTCQuickAccessMenu"` |
| `004F83AF` `004D617B` | `0x4E3FEF` | `"CTCNymph"` |

`"CTCSoundAtmosphereVillage"` is **not**
`CSoundAtmospheresDef` and is **not**
in-range. Do not promote it.

---

## 6. Host leftover

`EngineLifecycle.AddFirstDefClass` Notes
through HundredThird `CSoundAtmospheresDef`
(`004F82A0` / `004E32E3` / `004E1748` /
size 52 / vtbl `012427A4`) then
**returns**. `HundredThirdDefClassRegistered
= true`.

MATCH is Note-only + flag, not a live
68 / 72 / 52-byte object. Factory `E8`
is **not** on this walk.

| After Hundredth | Native | Host after Hundredth |
| --- | --- | --- |
| 7 unnamed `004D2EF0` | listing `004F7F4C`…`004F81DC` | **LEFTOVER** |
| 101 `CShipDef` `004F820A` / `004D8799` in-line `0044C0C0` size 68 vtbl `0123C0A4` | **PROVEN** (this file) | Notes + flag |
| 102 `CShopItemDef` `004F8255` / `004D8411` `jmp 004D405A` size 72 vtbl `0123B644` | **PROVEN** | Notes + flag |
| 103 `CSoundAtmospheresDef` `004F82A0` / `004E32E3` `jmp 004E1748` size 52 vtbl `012427A4` | **PROVEN** | Notes + flag |
| 3 unnamed `004D2EF0` then 104 `CNymphDef` | remaining-pairs | **LEFTOVER** |

Inventing `ActivateQuest("Q_NewOakValeIntro")`
from a Note would **DIVERGE**.

---

## Original

Hundred-first Add Def Class on `004EE23F`:

1. `0099EBF0` name `"CShipDef"`.
2. `0042DAE0` packs factory `004D8799`.
3. `0044C6B0` `004F820A`.
4. `009B0AC0` `004F8211`.

Factory alloc 68, in-line `0044C0C0`.
Vtbl `0123C0A4`. No extra dword stores.

Then `CShopItemDef` factory alloc 72,
`jmp 004D405A`. Then
`CSoundAtmospheresDef` factory alloc 52,
`jmp 004E1748`. Zero CTC between those
two.

Seven unnamed CTC between `CRumbleDef`
and `CShipDef`. Three unnamed CTC after
this cluster, then `CNymphDef`.

Not Oakvale. Not a Thing instance. Not a
file I/O site. Not `004B4A10`.

```
004EE23F  register CShipDef factory 004D8799              // no instance
004EE23F  register CShopItemDef factory 004D8411          // no instance
004EE23F  register CSoundAtmospheresDef factory 004E32E3  // no instance
00787720  later type-name intern                          // not here
007F52C0  later type-name intern                          // not here
```

---

## HundredFirst / HundredSecond / HundredThird constants

```
HundredFirstDefClassSite    = 0x004F820A
HundredFirstDefClassFactory = 0x004D8799
HundredFirstDefClassCtor    = 0x0044C0C0
HundredFirstDefClassVtbl    = 0x0123C0A4
HundredFirstDefClassSize    = 68
HundredFirstDefClassName    = "CShipDef"

HundredSecondDefClassSite    = 0x004F8255
HundredSecondDefClassFactory = 0x004D8411
HundredSecondDefClassCtor    = 0x004D405A
HundredSecondDefClassVtbl    = 0x0123B644
HundredSecondDefClassSize    = 72
HundredSecondDefClassName    = "CShopItemDef"

HundredThirdDefClassSite    = 0x004F82A0
HundredThirdDefClassFactory = 0x004E32E3
HundredThirdDefClassCtor    = 0x004E1748
HundredThirdDefClassVtbl    = 0x012427A4
HundredThirdDefClassSize    = 52
HundredThirdDefClassName    = "CSoundAtmospheresDef"
```

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `004F81E2` / `004F820A` / `004F8211` | pair 101 Add Def Class `CShipDef` | **PROVEN**; remaining-pairs **MATCH** |
| `004D8799` / `0044C0C0` / `0123C0A4` | factory / ctor / vtbl; size 68 | **PROVEN** |
| `004D489D` | vtbl[20] size 68 | **PROVEN** |
| `004F822D` / `004F8255` / `004F825C` | pair 102 Add Def Class `CShopItemDef` | **PROVEN**; remaining-pairs **MATCH** |
| `004D8411` / `004D405A` / `0123B644` | factory / persist ctor / vtbl; size 72 | **PROVEN** |
| `004D4078` | vtbl[20] size 72 | **PROVEN** |
| `004F8278` / `004F82A0` / `004F82A7` | pair 103 Add Def Class `CSoundAtmospheresDef` | **PROVEN**; remaining-pairs **MATCH** |
| `004E32E3` / `004E1748` / `012427A4` | factory / persist ctor / vtbl; size 52 | **PROVEN** |
| `004E1765` | vtbl[20] size 52 | **PROVEN** |
| seven `004D2EF0` `004F7F65`…`004F819F` | unnamed CTC | **PROVEN** count; in-range name **UNREAD** |
| `00787720` / `00787AE0` | later `CShipDef` type-name | **PROVEN**; **DISPROVEN** as this pair |
| `007F52C0` / `007F5560` | later `CSoundAtmospheresDef` type-name | **PROVEN**; **DISPROVEN** as this pair |
| `004F83F8` / `004F8420` | pair 104 `CNymphDef` | **PROVEN** sites; factory body **UNREAD** here |
| `AddFirstDefClass` | Notes through HundredThird | **MATCH** Notes+flag; live object **LEFTOVER** |
| Host `ActivateQuest("Q_NewOakValeIntro")` | must not be added | **DISPROVEN** |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00780000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-007c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\rtti.txt`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\abs.tsv`
- `C:\FableCSharp\assembly\compiled-defs\game\entries.tsv`
- `C:\FableCSharp\proofs\004EE23F-remaining-pairs\README.md`
- `C:\FableCSharp\proofs\004F7D1C-aoe-rumble\README.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs` (`AddFirstDefClass`)
