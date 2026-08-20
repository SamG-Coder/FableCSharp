# `004EE23F` forty-first `009B0AC0` / `0044C6B0` is `CHeroCentreDef`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` → `"Init Game"`
`0042F491` → `00418DCA` → `[vtbl+4]`
`004184BD` → `00418585` `004EE23F`.
Do **not** invent class names: only
`push "…"` listing strings in
`004EE932`…`004F9144`. Helper
`004Dxxxx` `push "CTC…"` strings are
**out of range**.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: recover Init Thing Components
pair 41 `CHeroCentreDef`. Confirm listing
`004F3310` / factory `0x4D86F0` /
`004F3338` `0044C6B0` / `004F333F`
`009B0AC0`. Count unnamed `004D2EF0`
CTC rows between `CMultiStaticMeshDef`
`004F3072` and this pair. Is this class
on no-save first Present? Kid 4300 child
types? `game.bin` instances?

| Field | Value | Class |
| --- | --- | --- |
| `0044C6B0` | `004F3338` | **PROVEN** |
| `009B0AC0` | `004F333F` | **PROVEN** |
| Factory | `004D86F0` `00BFEA1A(37)` then `0044C0C0`; vtbl **`0123BE54`** | **PROVEN** |
| Ctor | in-line at factory: `0044C0C0` then `[esi]=0123BE54`. Same body at `004D462A`. | **PROVEN** |
| Size | **37** (`push 37` at factory; vtbl[20] `004D463C` `push 37; pop eax; ret`) | **PROVEN** |
| Remaining-pairs row 41 | name / factory / sites / 6 CTC | **MATCH** |
| CTC between `004F3072` and this pair | **6** unnamed `004D2EF0` | **PROVEN** |
| On no-save first Present as a Thing type? | **No.** Lookout TNG kinds are OBJECT / MARKER / THING / AICreature / Holy Site. Spawned hero is `CREATURE_HERO` **4299**. | **DISPROVEN** |
| Kid 4300 child types include this class? | **UNREAD** as a 33-row list. `CREATURE_HERO_CHILD` is **not** first Present. `CHeroCentreDef` is **not** in the `SI_HERO_CHILD` cluster. | **UNREAD** list; first-Present kid **DISPROVEN** |
| `game.bin` instances | **2.** Both raw **3**, subdefs **0**. | **PROVEN** |

Authority: `Fable.exe` listing
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
`004F3310`, `004D86F0`, `004D462A`,
`004D463C`; `fn 004F3338`;
`proofs/004EE23F-remaining-pairs` row 41;
EngineLifecycle Fortieth
`CMultiStaticMeshDef` already shipped.
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01243FBC` **`CHeroCentreDef`**.
`assembly/exe/00-index/vtbl.tsv`
`0x0123BE54`.
`assembly/compiled-defs/game/entries.tsv`
ids **40 / 9456**; `INDEX.md` type count
**2**. `docs/status/investigations/2026-08-18-first-scene-things.md`.

Listing string at `004F3310` is
**`CHeroCentreDef`** (not invented).
Shape-2 (`push` + `0042DAE0`).

```
004F3310  push "CHeroCentreDef"
004F3315  lea ecx, [ebp-1408]
004F331B  call 0099EBF0
004F3320  push 0x4D86F0
004F3325  lea eax, [ebp-1408]
004F332B  push eax
004F332C  lea ecx, [ebp-2020]
004F3332  call 0042DAE0
004F3337  push eax
004F3338  call 0044C6B0
004F333D  mov ecx, eax
004F333F  call 009B0AC0
```

```
004D86F0  push esi
          push 37
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D8710
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123BE54
          mov eax, esi
          pop esi
          ret
004D8710  xor eax, eax
          pop esi
          ret

004D462A  push esi
          mov esi, ecx
          call 0044C0C0
          mov [esi], 0x123BE54
          mov eax, esi
          pop esi
          ret

004D463C  push 37
          pop eax
          ret
```

`0042DAE0` is the later name+factory
pack helper. It is **not** Add Def
Class.

Does not construct `Q_NewOakValeIntro`.
Note-only + flag, not a live 37-byte
object.

Next pair is `CQuestCardDef` `004F349C` /
`004F34C4` / `004F34CB` factory `004E2333`
(remaining-pairs row 42; 3 unnamed CTC
between).

---

## 1. Pair sites

`listing-004c0000.txt` after Fortieth
`CMultiStaticMeshDef` `004F3072`:

`004F3310` `68 BC 3F 24 01` =
`push 0x01243FBC`. `strings.tsv`:

```
0x01243FBC	0xE43FBC	CHeroCentreDef
```

Same listing annotates the immediate as
`"CHeroCentreDef"`. `xrefs.tsv`
`0x01243FBC` first hit `0x004F3311`
`fn=0x004F3045` (this registrar walk).
Later lookups `0077D340` / `0077E420`
are **not** Add Def Class.

`004F3320` `push 0x4D86F0` then
`0042DAE0` / `0044C6B0` / `009B0AC0`.
`abs.tsv` `0x004F3320` → `0x004D86F0`.
Matches remaining-pairs row 41.

---

## 2. Factory / vtbl / size

`004D86F0` is the same in-line thunk
shape as twentieth `004D7EB6`:
`00BFEA1A` with immediate **37**,
null → `xor eax, eax; ret`, else
`0044C0C0` then `[esi]=0x0123BE54`.
No extra stores. No `jmp` to
`004D462A`; that helper is the same
ctor next to the `CTCHeroCentre` name
thunk.

`vtbl.tsv` `0x0123BE54`:

| Slot | Dest | Note |
| ---: | --- | --- |
| 0 | `004D8714` | dtor: `[esi]=0x1230BA0` then `009FC550` |
| 18 | `0077D3A0` | `ret 4` (persist stub) |
| 19 | `004E0A00` | shared |
| 20 | `004D463C` | size **37** |

Slots 1–17 / 21–24 are the shared
`0042D930`…`0042DAA0` /
`009ACE90` / `009FBEF0` / `009ACAB0` /
`009ACB20` family. No invented names.

RTTI `0x01379380` `.?AVCHeroCentreDef@@`.

---

## 3. Six unnamed `004D2EF0` between `004F3072` and this pair

A CTC row is a `004D2EF0` in this fn
(same block: helper, `006869C0`,
`push factory`, `004D2EF0`, `004D9D2F`,
`004E40C3`). No `0044C6B0`. No
`009B0AC0`. Remaining-pairs method:
after `CTCActionUseSearch` there is
**no** in-range `push "…"`. Count
those as unnamed. Do **not** copy
helper RTTI into the pair table.

Listing `004F3072`…`004F3310` has
exactly **six** `call 004D2EF0`:

| `004D2EF0` | Factory `push` | Helper | Helper listing (out of range) |
| --- | --- | --- | --- |
| `004F30B0` | `0x4D44B8` | `004D44D5` | `"CTCAnimationComplex"` |
| `004F311B` | `0x4D4518` | `004D4535` | `"CTCHeroAttachableAppearanceModifiers"` |
| `004F3186` | `0x4E09C7` | `004D45A1` | `"CTCCreatureItemLevitationHero"` |
| `004F31F1` | `0x4E0984` | `004D458E` | `"CTCCreatureItemLevitationNymph"` |
| `004F325C` | `0x4D86B7` | `004D45B4` | `"CTCCreatureHive"` |
| `004F32C7` | `0x4D45FA` | `004D4617` | `"CTCHeroCentre"` |

```
004F3072  call 009B0AC0          ; CMultiStaticMeshDef
…
004F30A5  push 0x4D44B8
004F30B0  call 004D2EF0
…
004F3110  push 0x4D4518
004F311B  call 004D2EF0
…
004F317B  push 0x4E09C7
004F3186  call 004D2EF0
…
004F31E6  push 0x4E0984
004F31F1  call 004D2EF0
…
004F3251  push 0x4D86B7
004F325C  call 004D2EF0
…
004F32BC  push 0x4D45FA
004F32C7  call 004D2EF0
…
004F3310  push "CHeroCentreDef"
```

Remaining-pairs CTC column for row 41
is **6 unnamed**. **MATCH**.

After this pair, **three** more unnamed
`004D2EF0` then `CQuestCardDef`
(remaining-pairs row 42 column = 3).
**MATCH**.

| `004D2EF0` | Factory `push` | Helper | Helper listing (out of range) |
| --- | --- | --- | --- |
| `004F337D` | `0x4D4640` | `004D465D` | `"CTCHeroCentreDoorMarker"` |
| `004F33E8` | `0x4D4670` | `004D468D` | `"CTCHeroGuide"` |
| `004F3453` | `0x4D2E44` | `004D2E61` | `"CTCQuestCard"` |

```
004F349C  push "CQuestCardDef"
004F34AC  push 0x4E2333
004F34BE  call 0042DAE0
004F34C4  call 0044C6B0
004F34CB  call 009B0AC0
```

---

## 4. `game.bin` instances

Dump columns: `index type instance source mesh raw subdefs strings`.
`INDEX.md` type `CHeroCentreDef` count
**2**. `names.tsv`:

| Offset | CRC | Name |
| --- | --- | --- |
| `0x00006CE8` | `0x5BEF6C26` | `NULLDEF_CHeroCentreDef` |
| `0x00006D03` | `0x9C091C44` | `CHeroCentreDef` |

| Id | Type | Instance | Source | Raw | Subdefs | ASCII |
| ---: | --- | --- | --- | ---: | ---: | --- |
| 40 | `CHeroCentreDef` | `NULLDEF_CHeroCentreDef` | `NULLDEF_CHeroCentreDef` | 3 | 0 | *(empty)* |
| 9456 | `CHeroCentreDef` | `CHeroCentreDef` | *(empty)* | 3 | 0 | *(empty)* |

`raw` **3** is the compiled-def header
only (`GameBin.ParseEntry`: isReal /
isTemplate / unknown). No field table.
**MATCH** vtbl[18] `0077D3A0` `ret 4`.

NULLDEF index **40** sits in the class
default table immediately after
`NULLDEF_CMultiStaticMeshDef` **39** and
before `NULLDEF_CQuestCardDef` **41**.
**MATCH** remaining-pairs order
(Fortieth / this pair / Forty-second).

Id **9456** sits in a
`CAppearanceDef` / `CShopDef` /
`CMultiStaticMeshDef` cluster
(`9447`…`9463`), not the hero-creature
cluster. Parent `OBJECT` / `CREATURE` /
`BUILDING` `SubDefs` row that points at
**40** or **9456**: **UNREAD** (game.bin
`writeParts` is false; no per-entry
subdef dump).

---

## 5. Kid 4300 child types

`CREATURE_HERO_CHILD` (`RegionTravel.KidCreature`):

| Field | Value | Class |
| --- | --- | --- |
| Type | `CREATURE` | **PROVEN** `entries.tsv` **1472** |
| Graphic | **4300** | **PROVEN** `GameBinFormatTests` |
| SubDefs | **33** | **PROVEN** count |
| Raw | 1699 | **PROVEN** length |

The 33 child **type names** are
**UNREAD** here. `Fable.Dump bin`
prints only the first eight `SubDefs`.
`DumpGameBinFamily` does not write
per-entry markdown for `game.bin`.

Adjacent compiled-def cluster at
`SI_HERO_CHILD` (`10537`…`10543`):
`CAppearanceDef`, `CPhysicsDef`,
`CCreatureDef`, `CHeroDef`,
`CEntitySoundDef`, `CSkeletalMorphDef`,
`CHeroMorphDef`. **No**
`CHeroCentreDef`. **PARTIAL**
clustering only. Whether def index
**40** or **9456** is among the 33
`SubDefs` dwords is **UNREAD**.

`WorldGeometryTests` already
**DISPROVEN** `CMultiStaticMeshDef` as
a kid child. This class was not in that
assert.

Kid 4300 / `CREATURE_HERO_CHILD` is
**not** no-save first Present (next
section).

---

## 6. No-save first Present

First no-save Present is LookoutPoint
(`NewRegion 1`), not Oakvale. Census:
`2026-08-18-first-scene-things.md` /
`.dump.txt`.

| Claim | Answer | Class |
| --- | --- | --- |
| First Present region | LookoutPoint | **PROVEN** |
| Spawned hero | `CREATURE_HERO` mesh **4299** at `GuildArrivalHSP` | **PROVEN** |
| `CREATURE_HERO_CHILD` in Lookout TNG | **False** | **PROVEN** |
| Kid 4300 as this Present | **No.** | **DISPROVEN** |
| `CHeroCentreDef` as a TNG `DefinitionType` | **No.** `HasSubDefTable` is OBJECT / CREATURE / BUILDING / THING / MARKER / … | **DISPROVEN** |
| Component on a first-Present Thing | GuildExterior is a Lookout `ContainsMap` neighbour (BUILDING_GUILD_* exist as Things). Whether any of those `SubDefs` point at **40** / **9456** | **UNREAD** |

This registrar pair does **not** spawn
Lookout Things and does **not** open a
TNG.

---

## Original

Forty-first Add Def Class on `004EE23F`:

1. `0099EBF0` name `"CHeroCentreDef"`.
2. `0042DAE0` packs factory `004D86F0`.
3. `0044C6B0` `004F3338`.
4. `009B0AC0` `004F333F`.

Factory alloc 37, ctor in-line
`0044C0C0`. Vtbl `0123BE54`. No extra
dword inits.

Six unnamed CTC between Fortieth
`CMultiStaticMeshDef` `004F3072` and
this pair. Three unnamed CTC after,
then `CQuestCardDef`.

Not Oakvale. Not a Thing instance. Not a
file I/O site. Not kid 4300.

---

## Host

`EngineLifecycle.AddFirstDefClass`
already Notes Fortieth
`CMultiStaticMeshDef` (`004F306B` /
`004E31FA` / `004E1516` / size 52 /
vtbl `0124265C`).

Forty-first constants and Notes are
already in host: site `004F3338`,
factory `004D86F0`, ctor `0044C0C0`,
vtbl `0123BE54`, size 37, name
`CHeroCentreDef`. `AddFirstDefClass`
Notes the same pack / Add Def Class /
factory / LoadDef / `009FC4F0`
`[this+40]` line, then
`FortyFirstDefClassRegistered = true`.

Note-only + flag. **Not** a live
37-byte object. Factory `E8` is **not**
on this walk.

Host Notes **MATCH** the listing sites.
Live ctor is **LEFTOVER**.
This investigation does **not** edit
`src/`.
