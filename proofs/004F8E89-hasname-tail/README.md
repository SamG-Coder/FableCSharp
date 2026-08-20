# `004EE23F` tail after n=111 is `CHasNameDef` then 6 unnamed `004D2EF0` then `ret`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` (`0042F44D`) →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD` → `00418585` `004EE23F`.
Do **not** invent a listing parser. Read
`listing-004c0000.txt` after `004F8E61`.
Do **not** invent class names: only
`push "…"` listing strings in
`004EE23F` range (`004EE932`…`004F9144`).
Helper-fn `push "…"` strings are **not**
in-range; remaining-pairs counts those CTC
rows unnamed. This file records helper VAs
and factory imms without promoting helper
strings to in-range names.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: recover the **tail** of `004EE23F`
after remaining-pairs n=111 (`CHasNameDef`
`004F8E61` factory `0x4D98C8` sites
`004F8E89` / `004F8E90`). After n=111: 6
unnamed `004D2EF0` (`004F8ECE`, `004F8F39`,
`004F8FA0`, `004F900B`, `004F9076`,
`004F90E1`). `ret` `004F9144`. Confirm
from `listing-004c0000.txt`. Any of these
on first-seen childhood? Skim pairs 64–70;
recover factories for `COverheadDisplayDef`,
`CTavernTableDef`, `CTavernDef` if time
(childhood Oakvale tavern?).

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
`004F8E61`–`004F9145` and factories
`004D98C8` / `004D8AB0` / `004D8AF6` /
`004D8BE1`; sibling
`proofs/004EE23F-remaining-pairs` rows
64–70 and 111;
`proofs/004EE23F-thing-components`;
`proofs/004EE23F-twentyfirst-class`.
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01243AA8` **`CHasNameDef`**.
`assembly/exe/00-index/vtbl.tsv`
`0x0123E67C`.

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| Last Add Def Class on `004EE23F`? | **`CHasNameDef`** n=111. Shape-2 (`push` + `0042DAE0`). `0044C6B0` `004F8E89`. `009B0AC0` `004F8E90`. Factory `004D98C8` `00BFEA1A(52)` then `0044C0C0`; vtbl **`0123E67C`**. | **PROVEN** leftover |
| Further `0044C6B0` / `009B0AC0` before `ret`? | **None.** | **PROVEN** |
| After n=111, before `0073B130`? | **6** unnamed `004D2EF0` at the six sites below. No in-range `push "…"`. | **PROVEN** count; names **UNREAD** in-range |
| Function `ret`? | **`004F9144`**. Next insn `004F9145 jmp 004F914A` (next fn). **No** `int3` pad on the boundary. | **PROVEN** |
| First-seen childhood / Oakvale tavern spawn? | **No.** Type-register on `"Init Thing Components"`, not `00DBDE40` / `StartOakVale` / `HerosOldHouse`. | **DISPROVEN** |
| Pairs 64–66 factories? | `COverheadDisplayDef` size **40** vtbl `0123C8FC`; `CTavernTableDef` size **39** vtbl `0123C964`; `CTavernDef` size **44** vtbl `0123CA8C`. | **PROVEN** leftover |

**Answer:** n=111 is the last pair. Tail is
six unnamed CTC rows, then `0073B130`,
optional `004EBACE`, `ret` `004F9144`.
Not childhood. Not a Thing instance.

---

## 1. n=111 `CHasNameDef`

`listing-004c0000.txt` / remaining-pairs
row 111. Listing string at `004F8E61` is
**`CHasNameDef`** (not invented). Shape-2.

Two unnamed `004D2EF0` between n=110
`CLightningOrbDef` `004F8D6F` and this
pair (`004F8DAD` factory `0x4E7666`
helper `004D6674`; `004F8E18` factory
`0x4DCF15` helper `004D66B0`). Then:

```
004F8E61  push "CHasNameDef"
004F8E66  lea ecx, [ebp-1684]
004F8E6C  call 0099EBF0
004F8E71  push 0x4D98C8
004F8E76  lea eax, [ebp-1684]
004F8E7C  push eax
004F8E7D  lea ecx, [ebp-2572]
004F8E83  call 0042DAE0
004F8E88  push eax
004F8E89  call 0044C6B0
004F8E8E  mov ecx, eax
004F8E90  call 009B0AC0
```

`004F8E61` `68 A8 3A 24 01` =
`push 0x01243AA8`. `strings.tsv`:

```
0x01243AA8	0xE43AA8	CHasNameDef
```

`xrefs.tsv` `0x01243AA8` first hit
`0x004F8E62`. Same listing annotates
the immediate as `"CHasNameDef"`.

No `jmp` thunk: ctor is in-line like
twentieth `CPerceivedThingDef`.

```
004D98C8  push esi
          push 52
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D98E8
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123E67C
          mov eax, esi
          pop esi
          ret
004D98E8  xor eax, eax
          pop esi
          ret
```

No extra dword stores after the vtbl
write. Object is 52 bytes.

`vtbl.tsv` `0x0123E67C` slot 20 is
`004D66AC`. Listing:

```
004D66AC  6A 34  push 52
004D66AE  58     pop eax
004D66AF  C3     ret
```

Slot 0 is `004D98EC` (`mov [esi], 0x1230BA0`
then `009FC550`). Slots 1–17 / 21–24 are
the shared `0042D930`…`0042DAA0` /
`009ACE90` / `009FBEF0` / `009ACAB0` /
`009ACB20` family. No invented names.

| Field | Value | Class |
| --- | --- | --- |
| `0044C6B0` | `004F8E89` | **PROVEN** |
| `009B0AC0` | `004F8E90` | **PROVEN** |
| Factory | `004D98C8` `00BFEA1A(52)` then `0044C0C0`; vtbl **`0123E67C`** | **PROVEN** |
| Size | **52** (`push 52` at factory; vtbl[20] `004D66AC`) | **PROVEN** |

---

## 2. Six unnamed `004D2EF0` after n=111

Same CTC block as remaining-pairs §3:
helper, `006869C0` (or one inlined
immediate), `push factory`, `004D2EF0`,
`004D9D2F`, `004E40C3`. No `0044C6B0`.
No `009B0AC0`. No in-range `push "…"`.

Do **not** copy helper-fn listing
strings into the in-range name column
(remaining-pairs method). Helper VAs
are listed so the row is locatable.
Factory bodies are `00BFEA1A` then a
CTC ctor (not `0044C0C0`). CTC vtbl /
LoadDef **UNREAD**.

| # | `004D2EF0` | helper | factory `push` | factory body | Class |
| --: | --- | --- | --- | --- | --- |
| 1 | `004F8ECE` | `004D66F7` | `0x4D66DA` | `00BFEA1A(104)` then `00562EC0` | **PROVEN** sites/factory/size; name **UNREAD** in-range |
| 2 | `004F8F39` | `004D6727` | `0x4D670A` | `00BFEA1A(72)` then `0055D520` | same |
| 3 | `004F8FA0` | `004D66C3` | `0x4E400C` | `00BFEA1A(48)` then `004E3401` | same; **shape note** below |
| 4 | `004F900B` | `004D673A` | `0x4DCF6E` | `00BFEA1A(20)` then `004DB575` | same |
| 5 | `004F9076` | `004D674D` | `0x4D990E` | `00BFEA1A(16)` then `004D6760` | same |
| 6 | `004F90E1` | `004D3780` | `0x4DAF85` | `00BFEA1A(20)` then `004D7FF6` | same |

Row 3 is the only tail CTC that does
**not** call `006869C0` (`xor eax,eax;
ret`). Listing:

```
004F8F81  lea ecx, [ebp-328]
004F8F87  call 004D66C3
004F8F8C  lea eax, [ebp-328]
004F8F92  push eax
004F8F93  push 30
004F8F95  push 0x4E400C
004F8F9A  lea ecx, [ebp-6252]
004F8FA0  call 004D2EF0
```

`004D2EF0` still gets
`{factory, arg1, name}`. Here arg1 is
immediate **30**, not the stub 0.

Row 1 listing (shape of 1, 2, 4, 5, 6):

```
004F8EAB  lea ecx, [ebp-312]
004F8EB1  call 004D66F7
004F8EB6  lea eax, [ebp-312]
004F8EBC  push eax
004F8EBD  call 006869C0
004F8EC2  push eax
004F8EC3  push 0x4D66DA
004F8EC8  lea ecx, [ebp-6204]
004F8ECE  call 004D2EF0
004F8ED3  push eax
004F8ED4  lea eax, [ebp-312]
004F8EDA  push eax
004F8EDB  lea ecx, [ebp-11932]
004F8EE1  call 004D9D2F
004F8EE6  push eax
004F8EE7  mov ecx, esi
004F8EE9  call 004E40C3
```

Factory `004D66DA`:

```
004D66DA  push esi
          push 104
          mov esi, ecx
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004D66F3
          push esi
          mov ecx, eax
          call 00562EC0
          pop esi
          ret
004D66F3  xor eax, eax
          pop esi
          ret
```

No seventh `004D2EF0` before
`0073B130`.

---

## 3. Epilogue and `ret`

```
004F9129  call 0073B130
004F912E  cmp ["etWindowLongA"], 0x00
004F9135  je 004F913E
004F9137  mov ecx, esi
004F9139  call 004EBACE
004F913E  pop edi
004F913F  pop esi
004F9140  add ebp, 120
004F9143  leave
004F9144  ret
004F9145  jmp 004F914A
```

`004F9129` is the only `E8` of
`0073B130` (`004EE23F-thing-components`).
Flag dump-label `"etWindowLongA"` is
IAT-adjacent, not a type string.
`004EBACE` runs only if that flag is
nonzero; `ecx=esi` (the map from
`004E1B5D`). Inner `0073B130` table
**UNREAD**.

`004F9144` is `ret`. Next insn is
`004F9145 jmp 004F914A` (next function).
**No** `int3` pad on this boundary.
Matches remaining-pairs §2.

No `0044C6B0`. No `009B0AC0`. No
file I/O. No Thing spawn.

---

## 4. Pairs 64–70 (skim) and tavern factories

Remaining-pairs §5 names (listing
strings in-range; not invented):

| n | listing string | factory imm | `0044C6B0` | `009B0AC0` | CTC between |
| --: | --- | --- | --- | --- | --: |
| 64 | `COverheadDisplayDef` | `0x4D8AB0` | `004F5D7C` | `004F5D83` | 1 |
| 65 | `CTavernTableDef` | `0x4D8AF6` | `004F5E32` | `004F5E39` | 1 |
| 66 | `CTavernDef` | `0x4D8BE1` | `004F6029` | `004F6030` | 4 |
| 67 | `CObjectAugmentationsDef` | `0x4EC526` | `004F60DF` | `004F60E6` | 1 |
| 68 | `CDrunkennessDef` | `0x4D8C91` | `004F63AC` | `004F63B3` | 6 |
| 69 | `CGoldDef` | `0x4D8EC5` | `004F67BA` | `004F67C1` | 9 |
| 70 | `CAICreatureWillPowerIndicatorDef` | `0x4D926A` | `004F6946` | `004F694D` | 3 |

Factories recovered for 64–66 only
(time). All Shape-2. 67–70 factory
bodies **UNREAD** here.

### 4.1 `COverheadDisplayDef` (n=64)

```
004F5D54  push "COverheadDisplayDef"
004F5D64  push 0x4D8AB0
004F5D76  call 0042DAE0
004F5D7C  call 0044C6B0
004F5D83  call 009B0AC0
```

`strings.tsv` `0x01243E1C`
**`COverheadDisplayDef`**. One unnamed
CTC before this pair (`004F5D0B`
factory `0x4D50E6` helper `004D5103`).

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

`vtbl.tsv` `0x0123C8FC` slot 20
`004D5128`:

```
004D5128  push 40
          pop eax
          ret
```

Slot 0 `004D8AD4` (`01230BA0` /
`009FC550`). Size **40**.

### 4.2 `CTavernTableDef` (n=65)

```
004F5E0A  push "CTavernTableDef"
004F5E1A  push 0x4D8AF6
004F5E2C  call 0042DAE0
004F5E32  call 0044C6B0
004F5E39  call 009B0AC0
```

`strings.tsv` `0x01243E0C`
**`CTavernTableDef`**. One unnamed
CTC before (`004F5DC1` factory
`0x4D5142` helper `004D515F`).

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

`vtbl.tsv` `0x0123C964` slot 20
`004D513E`:

```
004D513E  push 39
          pop eax
          ret
```

Size **39** is thin (same class of
listing-proven odd size as eighth
`CReadableDef` 38). Factory and
vtbl[20] agree. No extra stores after
the vtbl write.

### 4.3 `CTavernDef` (n=66)

```
004F6001  push "CTavernDef"
004F6011  push 0x4D8BE1
004F6023  call 0042DAE0
004F6029  call 0044C6B0
004F6030  call 009B0AC0
```

`strings.tsv` `0x01243E00`
**`CTavernDef`**. Four unnamed CTC
between n=65 and this pair
(`004F5E77` `0x4D5172` helper
`004D518F`; `004F5EE2` `0x4DB124`
helper `004D51A2`; `004F5F4D`
`0x4D8BA8` helper `004D51B5`;
`004F5FB8` `0x4D5215` helper
`004D5232`).

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

`vtbl.tsv` `0x0123CA8C` slot 20
`004D5211`:

```
004D5211  push 44
          pop eax
          ret
```

Slot 0 `004D8C05` (`01230BA0` /
`009FC550`). Size **44**.

| Field | n=64 | n=65 | n=66 |
| --- | --- | --- | --- |
| Name | `COverheadDisplayDef` | `CTavernTableDef` | `CTavernDef` |
| Factory | `004D8AB0` | `004D8AF6` | `004D8BE1` |
| Size | **40** | **39** | **44** |
| Vtbl | `0123C8FC` | `0123C964` | `0123CA8C` |
| Base | `0044C0C0` | `0044C0C0` | `0044C0C0` |

---

## 5. Not first-seen childhood

No `00DBDE40` / region / TNG / hero
create on n=111, the six tail CTC, or
pairs 64–66. Parent is `004EE23F`.

First-seen childhood is
`StartOakValeWest` / `CAM_OVIF_SHOT2` /
`HerosOldHouse` after Leave
(`docs/render/FIRST_SCENE_WORLD_PARITY.md`).
Registering `CTavernDef` /
`CTavernTableDef` here is the global
Init Game type walk, not spawning the
Oakvale tavern Thing. **DISPROVEN** as
childhood tavern spawn. **DISPROVEN**
as first-seen `HerosOldHouse` object.

`CHasNameDef` is the last named Def
on this walk, not a childhood
instance.

---

## 6. Host leftover

`EngineLifecycle.AddFirstDefClass`
returns after twenty-first
`CBedDef` Note-only
(`004EE23F-twentyfirst-class`).
No `COverheadDisplayDef` /
`CTavernTableDef` / `CTavernDef` /
`CHasNameDef`. No `0x4D8AB0` /
`0x4D8AF6` / `0x4D8BE1` /
`0x4D98C8`. No tail `004D2EF0`.
No `0073B130`.

Whole remaining `004EE23F` walk after
n=21 is still leftover
(`004EE23F-thing-components` /
`004EE23F-remaining-pairs` §6).

| If host adds… | Leftover is… |
| --- | --- |
| Note-only through n=21 (current) | n=22 `CStealthDef` … n=111 `CHasNameDef`, six tail CTC, `0073B130` / `004EBACE` |
| Note-only all 111 names including `CHasNameDef` | still live `009AD6E0` / `009FC4F0` on each object (**not** MATCH); six CTC + `0073B130` still leftover |
| live Add Def Class for all 111 | next omit is the six unnamed `004D2EF0`, then `0073B130` / `004EBACE` |

---

## Original

Last Add Def Class on `004EE23F`:

1. `0099EBF0` name `"CHasNameDef"`.
2. `0042DAE0` packs factory `004D98C8`.
3. `0044C6B0` `004F8E89`.
4. `009B0AC0` `004F8E90`.

Factory alloc 52, in-line `0044C0C0`.
Vtbl `0123E67C`. No extra dwords.

Then six unnamed CTC `004D2EF0` (row 3
pushes 30 instead of `006869C0`).
Then `0073B130`, optional `004EBACE`,
`ret` `004F9144`.

Not Oakvale. Not a Thing instance. Not a
file I/O site.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `004F8E61` / `004F8E89` / `004F8E90` | last pair `CHasNameDef` | **PROVEN** leftover |
| `004D98C8` / `0123E67C` / size 52 | factory / vtbl / size | **PROVEN** |
| `004F8ECE` `004F8F39` `004F8FA0` `004F900B` `004F9076` `004F90E1` | six tail `004D2EF0` | **PROVEN** sites; names **UNREAD** in-range |
| `0x4D66DA` `0x4D670A` `0x4E400C` `0x4DCF6E` `0x4D990E` `0x4DAF85` | those six factories | **PROVEN** imm + alloc size |
| `004F8F93` `push 30` | tail CTC #3 arg1 | **PROVEN** shape; not `006869C0` |
| `004F9129` `0073B130` | post-table fill | **PROVEN** only-`E8`; body **UNREAD** |
| `004F9139` `004EBACE` | map commit if flag | **PROVEN** site; flag VA **PARTIAL** |
| `004F9144` `ret` | fn end | **PROVEN** |
| `004F9145` | next fn; no `int3` | **PROVEN** |
| `004F5D7C` / `004D8AB0` / size 40 / `0123C8FC` | n=64 `COverheadDisplayDef` | **PROVEN** leftover |
| `004F5E32` / `004D8AF6` / size 39 / `0123C964` | n=65 `CTavernTableDef` | **PROVEN** leftover |
| `004F6029` / `004D8BE1` / size 44 / `0123CA8C` | n=66 `CTavernDef` | **PROVEN** leftover |
| n=67…70 names | remaining-pairs listing strings | **PROVEN** names; factories **UNREAD** here |
| `00DBDE40` / first-seen childhood tavern | this tail | **DISPROVEN** |
| `AddFirstDefClass` | Notes through n=21 `CBedDef` | remaining **LEFTOVER** |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\proofs\004EE23F-remaining-pairs\README.md`
- `C:\FableCSharp\proofs\004EE23F-thing-components\README.md`
- `C:\FableCSharp\proofs\004EE23F-twentyfirst-class\README.md`
- `C:\FableCSharp\docs\render\FIRST_SCENE_WORLD_PARITY.md`
