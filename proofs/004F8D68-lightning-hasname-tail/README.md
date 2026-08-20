# `004EE23F` remaining pairs 110–111: `CLightningOrbDef` / `CHasNameDef` then 6 unnamed CTC then `ret`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` (`0042F44D`) →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD` → `00418585` `004EE23F`.
Do **not** invent a listing parser. Read
`listing-004c0000.txt` `004F8D40`…`004F9145`.
Do **not** invent class names: only
`push "…"` listing strings in
`004EE23F` range (`004EE932`…`004F9144`).
Helper-fn `push "…"` strings are **not**
in-range; remaining-pairs counts those CTC
rows unnamed. Do **not** invent helper CTC
names.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: recover remaining-pairs rows
**110–111** (`CLightningOrbDef` /
`CHasNameDef`) and the **6** unnamed
`004D2EF0` after n=111 then `ret`
`004F9144`. Factory persist ctor / size /
vtbl for both. Confirm `CHasNameDef`
numbers from the listing, not only
`proofs/004F8E89-hasname-tail`. Confirm
no further `0044C6B0` / `009B0AC0`.

| n | `0044C6B0` | `009B0AC0` | Factory | Size | Ctor | Vtbl | Class |
| --- | --- | --- | --- | ---: | --- | --- | --- |
| 110 | `004F8D68` | `004F8D6F` | `004D9882` | **60** | `0044C0C0` in-line | **`0123E5EC`** | **PROVEN** |
| 111 | `004F8E89` | `004F8E90` | `004D98C8` | **52** | `0044C0C0` in-line | **`0123E67C`** | **PROVEN** |

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
and `assembly/exe/01-sections/text-map/listing-004c0000.txt`
`004F8D40`…`004F9145`; factories
`004D9882` / `004D98C8`; size helpers
`004D6670` / `004D66AC`; persist
`004DF738` / `004DF77C`.
`assembly/exe/01-sections/text-map/e8.tsv`
`0x004F8D68`…`0x004F9139`.
`proofs/004EE23F-remaining-pairs` rows
109–111; sibling
`proofs/004F8E89-hasname-tail`.
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01243AB4` **`CLightningOrbDef`**,
`0x01243AA8` **`CHasNameDef`**.
`assembly/exe/00-index/vtbl.tsv`
`0x0123E5EC` / `0x0123E67C`.
`rtti.txt` `.?AVCLightningOrbDef@@` /
`.?AVCHasNameDef@@`.

Both pairs are shape-2 (`push` name +
factory + `0042DAE0` + `0044C6B0` +
`009B0AC0`). Listing strings are **not**
invented. `0042DAE0` is the name+factory
pack helper. Treating it as `009B0AC0`
is **DISPROVEN** (remaining-pairs §2).

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| Row 110 name / sites / factory? | **`CLightningOrbDef`** `004F8D40` / `004F8D68` / `004F8D6F` / `0x4D9882`. | **PROVEN** |
| Row 110 size / ctor / vtbl? | **60**; `0044C0C0` then `[esi]=0123E5EC`; vtbl[20] `004D6670` `push 60`. No extra stores after the vtbl write. | **PROVEN** |
| Row 111 name / sites / factory? | **`CHasNameDef`** `004F8E61` / `004F8E89` / `004F8E90` / `0x4D98C8`. Same numbers as `004F8E89-hasname-tail`; confirmed from this listing range. | **PROVEN** |
| Row 111 size / ctor / vtbl? | **52**; `0044C0C0` then `[esi]=0123E67C`; vtbl[20] `004D66AC` `push 52`. No extra stores after the vtbl write. | **PROVEN** |
| Remaining-pairs 110–111? | name / factory / sites / CTC counts | **MATCH** |
| Two CTC between 110 and 111? | **2** unnamed `004D2EF0` `004F8DAD` / `004F8E18`. No in-range `push "…"`. | **PROVEN** count; names **UNREAD** in-range |
| After n=111, before `0073B130`? | **6** unnamed `004D2EF0` at the six sites below. No in-range `push "…"`. | **PROVEN** count; names **UNREAD** in-range |
| Further `0044C6B0` / `009B0AC0` before `ret`? | **None.** Last pair is n=111. `e8.tsv` next after `004F9139` is `004F9153` (next fn). | **PROVEN** |
| Function `ret`? | **`004F9144`** `C3`. Next insn `004F9145 jmp 004F914A` (next fn). **No** `int3` pad. | **PROVEN** |
| Oakvale / first-seen childhood? | **No.** Init Thing Components class register. Not `00DBDE40`. | **DISPROVEN** |
| Host live objects? | **None.** `AddFirstDefClass` returns after hundredth `CRumbleDef`. These two + six CTC + `0073B130` are **LEFTOVER**. | **PROVEN** leftover |

**Answer:** two leftover Add Def Class
pairs. `CLightningOrbDef` alloc 60 via
in-line `0044C0C0`; vtbl **`0123E5EC`**.
`CHasNameDef` alloc 52 via in-line
`0044C0C0`; vtbl **`0123E67C`**. Then six
unnamed CTC, `0073B130`, optional
`004EBACE`, `ret` `004F9144`. Last pair
on `004EE23F`. Not Oakvale. Not a Thing
instance. Not a file I/O site.

---

## 0. Bound: pair 109 then two unnamed CTC

`listing-004c0000.txt` after 109
`CFireheartMinigameDef` `004F8C4E`
(remaining-pairs CTC-between **2**):

```
004F8C4E  call 009B0AC0
…
004F8C8C  call 004D2EF0          ; unnamed
…
004F8CF7  call 004D2EF0          ; unnamed
…
004F8D40  push "CLightningOrbDef"
```

No in-range `push "…"`. Helpers
`004D63A7` / `004D6687`, factories
`0x4D9547` / `0x4DCEA4`. Names stay
**UNREAD** in-range.

---

## 1. n=110 `CLightningOrbDef`

Remaining-pairs row 110. Listing
string at `004F8D40` is
**`CLightningOrbDef`** (not invented).
Shape-2.

```
004F8D40  68 B4 3A 24 01            push "CLightningOrbDef"
004F8D45  lea ecx, [ebp-1676]
004F8D4B  call 0099EBF0
004F8D50  68 82 98 4D 00            push 0x4D9882
004F8D55  lea eax, [ebp-1676]
004F8D5B  push eax
004F8D5C  lea ecx, [ebp-2556]
004F8D62  call 0042DAE0
004F8D67  push eax
004F8D68  call 0044C6B0
004F8D6D  mov ecx, eax
004F8D6F  call 009B0AC0
```

`004F8D40` `68 B4 3A 24 01` =
`push 0x01243AB4`. `strings.tsv`:

```
0x01243AB4	0xE43AB4	CLightningOrbDef
```

`xrefs.tsv` `0x01243AB4` first hit
`0x004F8D41`. Same listing annotates
the immediate as `"CLightningOrbDef"`.
`e8.tsv` `0x004F8D68` → `0x0044C6B0`;
`0x004F8D6F` → `0x009B0AC0`.

No `jmp` thunk: persist ctor is
in-line `0044C0C0` like twentieth
`CPerceivedThingDef` and n=111.

```
004D9882  push esi
004D9883  6A 3C  push 60
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D98A2
          mov ecx, esi
          call 0044C0C0
004D9898  C7 06 EC E5 23 01  mov [esi], 0x123E5EC
          mov eax, esi
          pop esi
          ret
004D98A2  xor eax, eax
          pop esi
          ret
```

No extra dword stores after the vtbl
write. Object is 60 bytes.

`vtbl.tsv` `0x0123E5EC` slot 20 is
`004D6670`. Listing:

```
004D6670  push 60
          pop eax
          ret
```

Standalone persist ctor `004D665E` is
the same `0044C0C0` then
`[esi]=0123E5EC` with no extra stores.
Factory in-lines that body instead of
`jmp 004D665E`. Slot 0 is `004D98A6`
(`mov [esi], 0x1230BA0` then
`009FC550`). Slots 1–17 / 21–24 are
the shared `0042D930`…`0042DAA0` /
`009ACE90` / `009FBEF0` / `009ACAB0` /
`009ACB20` family. Slot 18 persist
`004DF738`. Slot 19 clone `004E1A21`.
No invented names.

LoadDef `004DF738` walks five dwords
after the 40-byte `0044C0C0` base
(`+40` `00431102`, `+44` `+48` `+52`
`00431061`, `+56` `00431102`). Sites
**PROVEN**; intern / f32 payloads
**UNREAD**. Ctor does not store them.

| Field | Value | Class |
| --- | --- | --- |
| `0044C6B0` | `004F8D68` | **PROVEN** |
| `009B0AC0` | `004F8D6F` | **PROVEN** |
| Factory | `004D9882` `00BFEA1A(60)` then `0044C0C0`; vtbl **`0123E5EC`** | **PROVEN** |
| Size | **60** (`push 60` at factory; vtbl[20] `004D6670`) | **PROVEN** |

`game.bin` type row 109
`NULLDEF_CLightningOrbDef` raw **43**
is the serialized payload, not the
60-byte object.

---

## 2. Two unnamed `004D2EF0` between 110 and 111

Remaining-pairs CTC-between for n=111
is **2**. Listing:

| `004D2EF0` | helper | factory `push` | Class |
| --- | --- | --- | --- |
| `004F8DAD` | `004D6674` | `0x4E7666` | **PROVEN** sites; name **UNREAD** in-range |
| `004F8E18` | `004D66B0` | `0x4DCF15` | same |

Same CTC block as remaining-pairs §3
(`006869C0`, `push factory`,
`004D2EF0`, `004D9D2F`, `004E40C3`).
No `0044C6B0`. No `009B0AC0`. No
in-range `push "…"`. Helper `push "…"`
bodies stay out of the name column.

---

## 3. n=111 `CHasNameDef`

Remaining-pairs row 111. Listing
string at `004F8E61` is
**`CHasNameDef`** (not invented).
Shape-2. Confirmed from this range,
not copied from
`proofs/004F8E89-hasname-tail`.

```
004F8E61  68 A8 3A 24 01            push "CHasNameDef"
004F8E66  lea ecx, [ebp-1684]
004F8E6C  call 0099EBF0
004F8E71  68 C8 98 4D 00            push 0x4D98C8
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
`e8.tsv` `0x004F8E89` → `0x0044C6B0`;
`0x004F8E90` → `0x009B0AC0`.

```
004D98C8  push esi
004D98C9  6A 34  push 52
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
004D66AC  push 52
          pop eax
          ret
```

Standalone persist ctor `004D669A` is
the same `0044C0C0` then
`[esi]=0123E67C`. Factory in-lines.
Slot 0 is `004D98EC` (`01230BA0` /
`009FC550`). Slots 1–17 / 21–24 shared
family. Slot 18 persist `004DF77C`.
Slot 19 clone `004E1A59`.

LoadDef `004DF77C` walks three dwords
after the base (`+40` `+44` `+48`
`00431020`). Sites **PROVEN**; intern
payloads **UNREAD**.

| Field | Value | Class |
| --- | --- | --- |
| `0044C6B0` | `004F8E89` | **PROVEN** |
| `009B0AC0` | `004F8E90` | **PROVEN** |
| Factory | `004D98C8` `00BFEA1A(52)` then `0044C0C0`; vtbl **`0123E67C`** | **PROVEN** |
| Size | **52** (`push 52` at factory; vtbl[20] `004D66AC`) | **PROVEN** |

`game.bin` type row 110
`NULLDEF_CHasNameDef` raw **27** is
the serialized payload, not the
52-byte object.

---

## 4. Six unnamed `004D2EF0` after n=111

Same CTC block. No `0044C6B0`. No
`009B0AC0`. No in-range `push "…"`.
Do **not** copy helper-fn listing
strings into the in-range name column.

| # | `004D2EF0` | helper | factory `push` | factory body | Class |
| --: | --- | --- | --- | --- | --- |
| 1 | `004F8ECE` | `004D66F7` | `0x4D66DA` | `00BFEA1A(104)` then `00562EC0` | **PROVEN** sites/factory/size; name **UNREAD** in-range |
| 2 | `004F8F39` | `004D6727` | `0x4D670A` | `00BFEA1A(72)` then `0055D520` | same |
| 3 | `004F8FA0` | `004D66C3` | `0x4E400C` | `00BFEA1A(48)` then `004E3401` | same; **shape note** below |
| 4 | `004F900B` | `004D673A` | `0x4DCF6E` | `00BFEA1A(20)` then `004DB575` | same |
| 5 | `004F9076` | `004D674D` | `0x4D990E` | `00BFEA1A(16)` then `004D6760` | same |
| 6 | `004F90E1` | `004D3780` | `0x4DAF85` | `00BFEA1A(20)` then `004D7FF6` | same |

`e8.tsv` those six sites dest
`004D2EF0`. No seventh before
`0073B130`.

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

---

## 5. Epilogue, `ret`, no further pair

```
004F9129  E8 02 20 24 00            call 0073B130
004F912E  cmp ["etWindowLongA"], 0x00
004F9135  je 004F913E
004F9137  mov ecx, esi
004F9139  call 004EBACE
004F913E  pop edi
004F913F  pop esi
004F9140  add ebp, 120
004F9143  leave
004F9144  C3                        ret
004F9145  E9 00 00 00 00            jmp 004F914A
```

`e8.tsv` after `0x004F8E90` dest
`009B0AC0`: the six `004D2EF0`, then
`0x004F9129` → `0073B130`,
`0x004F9139` → `004EBACE`, then
`0x004F9153` (next function). **No**
`0044C6B0`. **No** `009B0AC0`.

`004F9129` is the only `E8` of
`0073B130` in this fn
(`004EE23F-thing-components`). Flag
dump-label `"etWindowLongA"` is
IAT-adjacent, not a type string.
`004EBACE` runs only if that flag is
nonzero; `ecx=esi` (the map from
`004E1B5D`). Inner `0073B130` table
**UNREAD**.

`004F9144` is `ret`. Next insn is
`004F9145 jmp 004F914A` (next
function). **No** `int3` pad on this
boundary. Matches remaining-pairs §2.

No file I/O. No Thing spawn.

---

## 6. Host leftover

`EngineLifecycle.AddFirstDefClass`
returns after hundredth `CRumbleDef`
(`004F7F2A` / factory `004E3290`).
No `CLightningOrbDef` /
`CHasNameDef`. No `0x4D9882` /
`0x4D98C8`. No tail `004D2EF0`.
No `0073B130`.

| If host adds… | Leftover is… |
| --- | --- |
| Note-only through n=100 (current) | n=101 `CShipDef` … n=110 `CLightningOrbDef` / n=111 `CHasNameDef`, six tail CTC, `0073B130` / `004EBACE` |
| Note-only all 111 names including `CHasNameDef` | still live `009AD6E0` / `009FC4F0` on each object (**not** MATCH); six CTC + `0073B130` still leftover |
| live Add Def Class for all 111 | next omit is the six unnamed `004D2EF0`, then `0073B130` / `004EBACE` |

---

## 7. Returned DefClass constants

Same shape as `HundredthDefClass*`
(`CRumbleDef`). Site is the
`0044C6B0` call. Ctor is in-line
`0044C0C0` (no jmp thunk). Note-only
+ flag, not a live 60- / 52-byte
object.

```
/// Next 009B0AC0 after CFireheartMinigameDef:
/// 004F8D68 0044C6B0 004F8D6F
/// CLightningOrbDef
/// factory 0x4D9882
/// pack 0042DAE0
/// 00BFEA1A(60)
/// 0044C0C0
/// size 60 vtbl 0123E5EC.
public const uint HundredTenthDefClassSite = 0x004F8D68;
public const uint HundredTenthDefClassFactory = 0x004D9882;
public const uint HundredTenthDefClassCtor = 0x0044C0C0;
public const uint HundredTenthDefClassVtbl = 0x0123E5EC;
public const int HundredTenthDefClassSize = 60;
public const string HundredTenthDefClassName = "CLightningOrbDef";

/// Next 009B0AC0 after CLightningOrbDef:
/// 004F8E89 0044C6B0 004F8E90
/// CHasNameDef
/// factory 0x4D98C8
/// pack 0042DAE0
/// 00BFEA1A(52)
/// 0044C0C0
/// size 52 vtbl 0123E67C.
/// Last Add Def Class on 004EE23F.
public const uint HundredEleventhDefClassSite = 0x004F8E89;
public const uint HundredEleventhDefClassFactory = 0x004D98C8;
public const uint HundredEleventhDefClassCtor = 0x0044C0C0;
public const uint HundredEleventhDefClassVtbl = 0x0123E67C;
public const int HundredEleventhDefClassSize = 52;
public const string HundredEleventhDefClassName = "CHasNameDef";
```

---

## Original

Last two Add Def Class pairs on
`004EE23F`:

1. `0099EBF0` name `"CLightningOrbDef"`.
2. `0042DAE0` packs factory `004D9882`.
3. `0044C6B0` `004F8D68`.
4. `009B0AC0` `004F8D6F`.
5. Factory alloc 60, in-line `0044C0C0`.
   Vtbl `0123E5EC`. No extra dwords.

Then two unnamed CTC.

6. `0099EBF0` name `"CHasNameDef"`.
7. `0042DAE0` packs factory `004D98C8`.
8. `0044C6B0` `004F8E89`.
9. `009B0AC0` `004F8E90`.
10. Factory alloc 52, in-line `0044C0C0`.
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
| `004F8D40` / `004F8D68` / `004F8D6F` | pair 110 `CLightningOrbDef` | **PROVEN** leftover |
| `004D9882` / `0123E5EC` / size 60 | factory / vtbl / size | **PROVEN** |
| `004D6670` / `004DF738` | size getter / LoadDef | **PROVEN** sites; LoadDef payload **UNREAD** |
| `004F8E61` / `004F8E89` / `004F8E90` | last pair `CHasNameDef` | **PROVEN** leftover |
| `004D98C8` / `0123E67C` / size 52 | factory / vtbl / size | **PROVEN** |
| `004D66AC` / `004DF77C` | size getter / LoadDef | **PROVEN** sites; LoadDef payload **UNREAD** |
| `004F8DAD` / `004F8E18` | two CTC between 110 and 111 | **PROVEN** sites; names **UNREAD** in-range |
| `004F8ECE` `004F8F39` `004F8FA0` `004F900B` `004F9076` `004F90E1` | six tail `004D2EF0` | **PROVEN** sites; names **UNREAD** in-range |
| `0x4D66DA` `0x4D670A` `0x4E400C` `0x4DCF6E` `0x4D990E` `0x4DAF85` | those six factories | **PROVEN** imm + alloc size |
| `004F8F93` `push 30` | tail CTC #3 arg1 | **PROVEN** shape; not `006869C0` |
| `004F9129` `0073B130` | post-table fill | **PROVEN** only-`E8`; body **UNREAD** |
| `004F9139` `004EBACE` | map commit if flag | **PROVEN** site; flag VA **PARTIAL** |
| `004F9144` `ret` | fn end | **PROVEN** |
| `004F9145` | next fn; no `int3` | **PROVEN** |
| further `0044C6B0` / `009B0AC0` | none before `ret` | **PROVEN** |
| `00DBDE40` / first-seen childhood | this tail | **DISPROVEN** |
| `AddFirstDefClass` | Notes through n=100 `CRumbleDef` | remaining **LEFTOVER** |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\assembly\exe\00-index\rtti.txt`
- `C:\FableCSharp\assembly\compiled-defs\game\entries.tsv`
- `C:\FableCSharp\proofs\004EE23F-remaining-pairs\README.md`
- `C:\FableCSharp\proofs\004F8E89-hasname-tail\README.md`
- `C:\FableCSharp\proofs\004EE23F-thing-components\README.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs` (`AddFirstDefClass`, read only)
