# `004EE23F` pairs 56–57: `CAIScratchpadDef` / `COccupiableDef`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` (`0042F44D`) →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD` → `00418585` `004EE23F`.
Do **not** invent class names: only
`push "…"` listing strings in
`004EE932`…`004F9144`.

Status words: **PROVEN** / **UNKNOWN** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: recover remaining-pairs rows
**56–57**. Factory bodies, sizes, vtbls.
Thirteen unnamed CTC between 55 and 56.
Lookout / Oakvale TNG occupancy? Next
`CBossDef`.

| n | `0044C6B0` | `009B0AC0` | Factory | Size | Ctor | Vtbl | Class |
| --- | --- | --- | --- | ---: | --- | --- | --- |
| 56 | `004F55B5` | `004F55BC` | `004D4E07` | **156** (`0x9C`) | `jmp 007ABB30` | **`0126D014`** | **PROVEN** |
| 57 | `004F566B` | `004F5672` | `004D88FC` | **44** | `0044C0C0` in-line | **`0123C514`** | **PROVEN** |

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
(`004F5002`…`004F5728`; factories
`004D4E07` / `004D88FC`);
`listing-00780000.txt` `007ABB30` /
`007AC6E0`;
`proofs/004EE23F-remaining-pairs` rows 56–57;
`proofs/004F5BA5-activate-quest-def`.
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01243E88` **`CAIScratchpadDef`**,
`0x01243E78` **`COccupiableDef`**,
`0x01243E6C` **`CBossDef`**.
`assembly/exe/00-index/vtbl.tsv`
`0x0126D014` / `0x0123C514`.
`rtti.txt` `.?AVCAIScratchpadDef@@` /
`.?AVCOccupiableDef@@`.

Both pairs are shape-2 (`push` + `0042DAE0`).
Listing strings are **not** invented.

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| Row 56 name / sites / factory? | **`CAIScratchpadDef`** `004F558D` / `004F55B5` / `004F55BC` / `0x4D4E07`. | **PROVEN** |
| Row 56 size / ctor / vtbl? | **156**; `00BFEA1A(0x9C)` then `jmp 007ABB30`; base `009FBEC0`; vtbl **`0126D014`**; vtbl[20] `007AC6E0` `mov eax, 0x9C; ret`. | **PROVEN** |
| Row 57 name / sites / factory? | **`COccupiableDef`** `004F5643` / `004F566B` / `004F5672` / `0x4D88FC`. | **PROVEN** |
| Row 57 size / ctor / vtbl? | **44**; `0044C0C0` then `[esi]=0123C514`; vtbl[20] `004D4E78` `push 44; pop eax; ret`. | **PROVEN** |
| Remaining-pairs 56–57? | name / factory / sites / CTC counts | **MATCH** |
| 13 CTC between 55 and 56? | **13** `004D2EF0`. No in-range `push "…"`. | **PROVEN** count; names **UNKNOWN** |
| Lookout / Oakvale TNG occupancy? | **No on this walk.** Registrar only. Not `004FDBC0`. Not `00DBDE40`. | **DISPROVEN** |
| Lookout TNG things carry `COccupiableDef`? | `OBJECT_TOWNBENCH_01` ×4 exist. SubDef types not in `entries.tsv`. | **UNKNOWN** |
| Oakvale TNG things carry `COccupiableDef`? | Later `OBJECT_CHAIR_01` / `OBJECT_BED` exist. SubDef types not dumped. | **UNKNOWN** |
| Next pair? | **`CBossDef`** `004F56F9` / `004F5721` / `004F5728` / `0x4E0D4C`. | **PROVEN** name/sites; ctor **UNKNOWN** |
| Host live objects? | **None.** Notes + flags exist for 56–57. `AddFirstDefClass` returns after sixty-first `CInterestingToVillagersDef` `004F5AEF`. Factory `E8` **not** called. | **PROVEN** leftover live |

**Answer:** two leftover Add Def Class
pairs. `CAIScratchpadDef` alloc 156 via
`007ABB30`. `COccupiableDef` alloc 44 via
in-line `0044C0C0`. Thirteen unnamed CTC
between `CSummonableCreatureDef` and
scratch. Not Lookout occupancy. Not
Oakvale occupancy. Next is `CBossDef`.

---

## 1. Pair 56 — `CAIScratchpadDef`

`proofs/004EE23F-remaining-pairs` §5:

| n | `push` | listing string | factory imm | `0044C6B0` | `009B0AC0` | CTC between |
| --: | --- | --- | --- | --- | --- | --: |
| 56 | `004F558D` | `CAIScratchpadDef` | `0x4D4E07` | `004F55B5` | `004F55BC` | 13 |

`listing-004c0000.txt` after pair 55
`CSummonableCreatureDef` `004F5002`:

```
004F558C  push edi
004F558D  push "CAIScratchpadDef"
004F5592  lea ecx, [ebp-1248]
004F5598  call 0099EBF0
004F559D  push 0x4D4E07
004F55A2  lea eax, [ebp-1248]
004F55A8  push eax
004F55A9  lea ecx, [ebp-1756]
004F55AF  call 0042DAE0
004F55B4  push eax
004F55B5  call 0044C6B0
004F55BA  mov ecx, eax
004F55BC  call 009B0AC0
```

`004F558D` `68 88 3E 24 01` =
`push 0x01243E88`. `strings.tsv`:

```
0x01243E88	0xE43E88	CAIScratchpadDef
```

`xrefs.tsv` `0x01243E88`:

| Site | Fn | Role |
| --- | --- | --- |
| `004F558E` | `004F47F0` (`abs.tsv` greedy `004EE137`) | this registrar |
| `007AD9C8` | `007AD9C0` | later type-name intern |

`abs.tsv` `0x004F559D` → `0x004D4E07`.
`0042DAE0` is the name+factory pack helper.
Treating it as `009B0AC0` is **DISPROVEN**
(remaining-pairs §2).

```
004D4E07  push 0x9C
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004D4E1D
          mov ecx, eax
          jmp 007ABB30
004D4E1D  xor eax, eax
          ret

007ABB30  push esi
          mov esi, ecx
          call 009FBEC0
          mov al, [esi+36]
          and al, 0xF8
          lea ecx, [esi+100]
          mov [esi+28], 0x0
          mov [esi+36], al
          mov [esi], 0x126D014
          call 0099E4B0
          lea ecx, [esi+108]
          call 0099E4B0
          lea ecx, [esi+140]
          call 0099E4B0
          lea ecx, [esi+144]
          call 0099E4B0
          mov eax, esi
          pop esi
          ret

007AC6E0  mov eax, 0x9C
          ret
```

Same thunk shape as nineteenth
`004E0B4B`: `00BFEA1A` with immediate
**156**, null → `xor eax, eax; ret`, else
`mov ecx, eax; jmp 007ABB30`.

`007ABB30` calls `009FBEC0` (shared
object/def base; `0044C0C0` also calls
it). Writes vtbl `0x0126D014`, zeros
`+28`, clears low bits of `+36`, then
four `0099E4B0` CString inits at `+100`
`+108` `+140` `+144`. No other stores.

`vtbl.tsv` `0x0126D014`:

| slot | VA | note |
| --: | --- | --- |
| 0 | `007ACAA0` | dtor |
| 1–17 / 21–24 | shared `0042D930`…`0042DAA0` family | no invented names |
| 18 | `007AD300` | |
| 19 | `007ADEF0` | |
| **20** | **`007AC6E0`** | size `mov eax, 0x9C; ret` |

RTTI `0x013833E4` `.?AVCAIScratchpadDef@@`.
`game.bin` has **6** `CAIScratchpadDef`
(INDEX). One `NULLDEF_CAIScratchpadDef`
(id 55, raw 230). Five live rows
(`8909` / `8993` / `9857` / `10342` /
`10492`) all string `TestTarget`. Persist
raw **240** is **not** the runtime 156-byte
object.

`007AD9C0` later interns the type name.
**DISPROVEN** as this pair.

---

## 2. Pair 57 — `COccupiableDef`

| n | `push` | listing string | factory imm | `0044C6B0` | `009B0AC0` | CTC between |
| --: | --- | --- | --- | --- | --- | --: |
| 57 | `004F5643` | `COccupiableDef` | `0x4D88FC` | `004F566B` | `004F5672` | 1 |

```
004F5642  push edi
004F5643  push "COccupiableDef"
004F5648  lea ecx, [ebp-1252]
004F564E  call 0099EBF0
004F5653  push 0x4D88FC
004F5658  lea eax, [ebp-1252]
004F565E  push eax
004F565F  lea ecx, [ebp-2148]
004F5665  call 0042DAE0
004F566A  push eax
004F566B  call 0044C6B0
004F5670  mov ecx, eax
004F5672  call 009B0AC0
```

`004F5643` `68 78 3E 24 01` =
`push 0x01243E78`. `strings.tsv`:

```
0x01243E78	0xE43E78	COccupiableDef
```

`xrefs.tsv` `0x01243E78`:

| Site | Fn | Role |
| --- | --- | --- |
| `004F5644` | `004F47F0` (`abs.tsv` greedy `004EE137`) | this registrar |
| `0087C2E4` | `0087C2E0` | later lookup |
| `0087C498` | `0087C490` | later type-name intern |

`abs.tsv` `0x004F5653` → `0x004D88FC`.

```
004D88FC  push esi
          push 44
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D891C
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123C514
          mov eax, esi
          pop esi
          ret
004D891C  xor eax, eax
          pop esi
          ret

004D4E66  push esi
          mov esi, ecx
          call 0044C0C0
          mov [esi], 0x123C514
          mov eax, esi
          pop esi
          ret

004D4E78  push 44
          pop eax
          ret
```

No `jmp` thunk: ctor is in-line like
twentieth `CPerceivedThingDef`.
`004D4E66` writes the same vtbl; factory
does **not** `jmp` it. No extra dword
stores after the vtbl write.

`vtbl.tsv` `0x0123C514`:

| slot | VA | note |
| --: | --- | --- |
| 0 | `004D8920` | dtor (`[esi]=0x1230BA0` then `009FC550`) |
| 1–17 / 21–24 | shared `0042D930`…`0042DAA0` family | no invented names |
| 18 | `004DE8B2` | |
| 19 | `004E0D33` | |
| **20** | **`004D4E78`** | size `push 44; pop eax; ret` |

RTTI `0x0137946C` `.?AVCOccupiableDef@@`.
`game.bin` has **68** `COccupiableDef`
(INDEX). One `NULLDEF_COccupiableDef`
(id 56, raw 11). Live rows are unnamed
11-byte persist blobs. Persist raw **11**
is **not** the runtime 44-byte object.

`0087C2E0` / `0087C490` later look up the
type name. **DISPROVEN** as this pair.

One unnamed `004D2EF0` between 56 and 57
(`004F55FA` factory `0x4DB0AD`). Helper
`004D4E53` (called `004F55DD`) pushes
`"CTCSinglePersonOccupiable"` then
`0099EBF0`. That string is **out of**
`004EE932`…`004F9144`. Do not promote it
as an in-range registrar name. Remaining-
pairs CTC column = 1 unnamed. **MATCH**
count.

---

## 3. Thirteen unnamed CTC (55 → 56)

In-range `004EE23F` has **no** `push "…"`
on these rows. Remaining-pairs counted
them unnamed. Helpers below push CTC
names **out of** `004EE932`…`004F9144`.
Do not copy those strings into the
registrar table. In-range names stay
**UNKNOWN**.

| # | helper | `004D2EF0` | factory `push` | out-of-range helper string |
| --: | --- | --- | --- | --- |
| 1 | `004D4BCB` | `004F5040` | `0x4D4BAE` | `CTCShopKeeper` |
| 2 | `004D4BFB` | `004F50AB` | `0x4D4BDE` | `CTCSummonSpell` |
| 3 | `004D4C75` | `004F5116` | `0x4D4C58` | `CTCBulletTime` |
| 4 | `004D4CA5` | `004F5181` | `0x4D4C88` | `CTCPhysicalShieldSpell` |
| 5 | `004D4CD8` | `004F51EC` | `0x4D4CB8` | `CTCWillLightning` |
| 6 | `004D4D08` | `004F5257` | `0x4D4CEB` | `CTCWillEnflame` |
| 7 | `004D4D38` | `004F52C2` | `0x4D4D1B` | `CTCForcePushPower` |
| 8 | `004D4D4B` | `004F532D` | `0x4E2B8E` | `CTCPreCalculatedNavigationRoute` |
| 9 | `004D4789` | `004F5398` | `0x4E75A0` | `CTCCreatureGroup` |
| 10 | `004D4DC1` | `004F5403` | `0x4D4DA1` | `CTCBattleCharge` |
| 11 | `004D4DF4` | `004F546E` | `0x4D4DD4` | `CTCAssassinRush` |
| 12 | `004D4D5E` | `004F54D9` | `0x4E762D` | `CTCCreatureGroupTag` |
| 13 | `004D4E40` | `004F5544` | `0x4D4E20` | `CTCAIScratchpad` |

Thirteen `004D2EF0`. **MATCH** remaining-
pairs CTC column. No `0044C6B0`. No
`009B0AC0`. Treating a CTC row as Add
Def Class is **DISPROVEN**.

Row 13 factory `0x4D4E20` allocs `0x84`
then `007ABB80` (vtbl `0126D07C`). That
is the CTC next to the Def pair, not the
Def itself.

---

## 4. Lookout / Oakvale TNG occupancy — DISPROVEN here

`004EE23F` at these pairs:

1. `0099EBF0` name `"CAIScratchpadDef"` /
   `"COccupiableDef"`.
2. `0042DAE0` packs the factory.
3. `0044C6B0` `004F55B5` / `004F566B`.
4. `009B0AC0` `004F55BC` / `004F5672`.

That inserts type records so later
LoadDef can construct 156-byte /
44-byte defs. It is the same Add Def
Class walk as `CHeroMorphDef` …
`CHasNameDef`.

`listing-004c0000.txt` in this range has
**zero** `call 004FDBC0`, **zero**
`call 004FBF60`, **zero** TNG parse,
**zero** `NewThing`. No region apply.
No hero create.

| Claim | Class |
| --- | --- |
| This walk occupies a Lookout bench / Oakvale bed | **DISPROVEN** |
| This walk opens `LookoutPoint.tng` | **DISPROVEN** (later `004FDBC0`) |
| This walk opens `StartOakVale*.tng` | **DISPROVEN** (`leftover-50-tng-ebx`) |
| This walk is `00DBDE40` / `Q_NewOakValeIntro` | **DISPROVEN** |

First TNG **open** after Leave is later
`004FDBC0` `LookoutPoint.tng`
(`proofs/tng-first-after-leave` /
`004FDBC0-open`). Oakvale West is the
same pump at `ebx=203`, still Loading
world, **not** first Present
(`leftover-50-tng-ebx`).

Lookout TNG (`lookout-tng-walk` /
`2026-08-18-first-scene-things`):
**288** things, **192** Object.
Graphic props include
`OBJECT_TOWNBENCH_01` ×4 (mesh 7548).
**No** `OBJECT_BED*` / `OBJECT_CHAIR*`.
**No** `CAIScratchpad` / `TestTarget`
DefinitionType.

Oakvale later submit
(`docs/render/traces/visibility-layers.txt`)
includes `OBJECT_CHAIR_01` and
`OBJECT_BED`. That is a **later**
ContainsMap / OpenStaticMaps tree, not
this registrar.

`game.bin` `OBJECT_TOWNBENCH_01` /
`OBJECT_CHAIR_01` / `OBJECT_BED` have
SubDef counts 5 / 5 / 8.
`entries.tsv` does **not** list child
types. Whether those SubDefs include
`COccupiableDef` is **UNKNOWN**.

`CAIScratchpadDef` live instances are
five `TestTarget` rows. Lookout first-
scene DefinitionTypes do not include
them. Oakvale TNG `TestTarget`:
**UNKNOWN**.

Do **not** invent occupancy gameplay
from this pair. Do **not** add Lookout
bench sit / Oakvale bed occupy in
`src/` from this walk.

---

## 5. Next pair — `CBossDef` (row 58)

| Field | Value | Class |
| --- | --- | --- |
| `push` | `004F56F9` `"CBossDef"` | **PROVEN** |
| factory imm | `0x4E0D4C` | **PROVEN** |
| `0044C6B0` | `004F5721` | **PROVEN** |
| `009B0AC0` | `004F5728` | **PROVEN** |
| CTC between 57 and 58 | 1 unnamed (`004F56B0` factory `0x4D4E7C`) | **PROVEN** count |
| Factory thunk | `004E0D4C` `00BFEA1A(84)` then `jmp 004DE8C2` | **PROVEN** size; ctor body **UNKNOWN** |

```
004F56F8  push edi
004F56F9  push "CBossDef"
004F56FE  lea ecx, [ebp-1260]
004F5704  call 0099EBF0
004F5709  push 0x4E0D4C
004F570E  lea eax, [ebp-1260]
004F5714  push eax
004F5715  lea ecx, [ebp-2468]
004F571B  call 0042DAE0
004F5720  push eax
004F5721  call 0044C6B0
004F5726  mov ecx, eax
004F5728  call 009B0AC0
```

`strings.tsv` `0x01243E6C` **`CBossDef`**.
Helper `004D4E99` (called `004F5693`)
pushes `"CTCBoss"` out of range.
Remaining-pairs CTC column = 1 unnamed.
**MATCH** count.

Factory body / vtbl of `004DE8C2` stay
**UNKNOWN** here. Next investigation is
that pair.

---

## 6. Host leftover

`EngineLifecycle.AddFirstDefClass` Notes
through sixty-first
`CInterestingToVillagersDef` `004F5AEF`
then **returns**. Pairs 56–57 have
Note-only + flag (`CAIScratchpadDef` /
`COccupiableDef`; sizes 156 / 44; vtbls
`0126D014` / `0123C514`). No live
156-byte / 44-byte object. Factory `E8`
is **not** on this walk.

This investigation does **not** ship
live objects.

| Native | Host |
| --- | --- |
| 13 unnamed `004D2EF0` `004F501D`…`004F5587` | **LEFTOVER** (no CTC Notes) |
| 56 `CAIScratchpadDef` `004F55B5` / `004D4E07` `jmp 007ABB30` size 156 vtbl `0126D014` | **MATCH** Note + flag; live object **LEFTOVER** |
| 1 unnamed `004D2EF0` (`0x4DB0AD`) `004F55D7`…`004F563D` | **LEFTOVER** |
| 57 `COccupiableDef` `004F566B` / `004D88FC` size 44 vtbl `0123C514` | **MATCH** Note + flag; live object **LEFTOVER** |
| 1 unnamed `004D2EF0` (`0x4D4E7C`) `004F568D`…`004F56F3` | **LEFTOVER** |
| 58 `CBossDef` `004F5721` / `004E0D4C` | name/sites **PROVEN** here; ctor **UNKNOWN** here |
| rows 59…111 | remaining-pairs |

Note-only + flag is **not** a live
156-byte / 44-byte object. Inventing
Lookout / Oakvale occupancy from a Note
would **DIVERGE**.

---

## Original

Fifty-sixth Add Def Class on `004EE23F`:

1. `0099EBF0` name `"CAIScratchpadDef"`.
2. `0042DAE0` packs factory `004D4E07`.
3. `0044C6B0` `004F55B5`.
4. `009B0AC0` `004F55BC`.

Factory alloc 156, ctor `007ABB30`.
Base `009FBEC0`. Vtbl `0126D014`.
Four CString inits `+100…+144`.

Fifty-seventh:

1. `0099EBF0` name `"COccupiableDef"`.
2. `0042DAE0` packs factory `004D88FC`.
3. `0044C6B0` `004F566B`.
4. `009B0AC0` `004F5672`.

Factory alloc 44, in-line `0044C0C0`.
Vtbl `0123C514`. No extra stores.

Thirteen unnamed CTC between
`CSummonableCreatureDef` and scratch.
One unnamed CTC after scratch, then
occupiable. One unnamed CTC after
occupiable, then `CBossDef`.

Not Oakvale. Not Lookout TNG. Not a
Thing instance. Not a file I/O site.

```
004EE23F  register CAIScratchpadDef / COccupiableDef
004FDBC0  later LookoutPoint.tng open          // not here
006C2170  later ContainsMap construct          // not here
00DBDE40  Oakvale intro                        // not here
```

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `004F558D` / `004F55B5` / `004F55BC` | pair 56 Add Def Class `CAIScratchpadDef` | **PROVEN**; remaining-pairs **MATCH** |
| `004D4E07` / `007ABB30` / `0126D014` | factory / ctor / vtbl; size 156 | **PROVEN** |
| `007AC6E0` | vtbl[20] size 156 | **PROVEN** |
| `004F5643` / `004F566B` / `004F5672` | pair 57 Add Def Class `COccupiableDef` | **PROVEN**; remaining-pairs **MATCH** |
| `004D88FC` / `0123C514` | factory / vtbl; size 44 | **PROVEN** |
| `004D4E78` | vtbl[20] size 44 | **PROVEN** |
| 13× `004D2EF0` `004F5040`…`004F5544` | unnamed CTC | **PROVEN** count; in-range names **UNKNOWN** |
| `004F55FA` | unnamed CTC `0x4DB0AD` | **PROVEN** count; in-range name **UNKNOWN** |
| `004F56B0` | unnamed CTC `0x4D4E7C` | **PROVEN** count; in-range name **UNKNOWN** |
| `007AD9C0` / `0087C2E0` / `0087C490` | later type-name / lookup | **PROVEN**; **DISPROVEN** as this pair |
| `004F56F9` / `004F5721` / `004F5728` | pair 58 `CBossDef` | **PROVEN** name/sites |
| `004E0D4C` / `004DE8C2` | pair 58 factory thunk size 84 | **PROVEN** size; ctor **UNKNOWN** |
| `004FDBC0` / `LookoutPoint.tng` | first TNG open | **DISPROVEN** as this pair |
| `00DBDE40` / Oakvale TNG occupy | Oakvale intro / later furniture | **DISPROVEN** as this pair |
| Lookout bench / Oakvale bed SubDefs | `COccupiableDef` child? | **UNKNOWN** |
| `AddFirstDefClass` | Notes 56–57 + flags; returns after sixty-first `CInterestingToVillagersDef` | Notes **MATCH**; live 156/44-byte objects **LEFTOVER** |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00780000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\rtti.txt`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\abs.tsv`
- `C:\FableCSharp\assembly\compiled-defs\game\INDEX.md`
- `C:\FableCSharp\assembly\compiled-defs\game\entries.tsv`
- `C:\FableCSharp\proofs\004EE23F-remaining-pairs\README.md`
- `C:\FableCSharp\proofs\004F5BA5-activate-quest-def\README.md`
- `C:\FableCSharp\proofs\tng-first-after-leave\README.md`
- `C:\FableCSharp\proofs\lookout-tng-walk\README.md`
- `C:\FableCSharp\proofs\leftover-50-tng-ebx\README.md`
- `C:\FableCSharp\docs\status\investigations\2026-08-18-first-scene-things.md`
- `C:\FableCSharp\docs\render\traces\visibility-layers.txt`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs` (`AddFirstDefClass`) read only
