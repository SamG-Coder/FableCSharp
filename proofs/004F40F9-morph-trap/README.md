# `004EE23F` forty-eighth / forty-ninth pairs

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` (`0042F44D`) →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD` → `00418585` `004EE23F`.
Do **not** invent CTC names from `004Dxxxx`
helpers. Remaining-pairs: after
`CTCActionUseSearch` later `004D2EF0` rows
are unnamed.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: recover pairs 48–49
`CSkeletalMorphDef` `004F40D1` factory
`0x4E3DD9` (`004E3DD9` `push 52`
`jmp 004E2895`; `004E2895` `0044C0C0`
vtbl `0x012429EC` zeros `+40`/`+44`/`+48`)
sites `004F40F9` / `004F4100` and
`CTrapDef` `004F439E` factory `0x4E5CF2`
sites `004F43C6` / `004F43CD`. Kid
PALSKIN / Graphic **4300**: does
`CSkeletalMorphDef` apply on first-seen?
`CTrapDef` in StartOakVale TNG? Next
`CParticleAttacherDef`.

| Question | Answer | Class |
| --- | --- | --- |
| Pair 48 name / sites / factory? | **`CSkeletalMorphDef`** `004F40F9` / `004F4100` / `004E3DD9` | **PROVEN** |
| Pair 49 name / sites / factory? | **`CTrapDef`** `004F43C6` / `004F43CD` / `004E5CF2` | **PROVEN** |
| Pair 48 factory shape? | `00BFEA1A(52)` then **`jmp 004E2895`**. Persist ctor **is** the factory dest. | **PROVEN** |
| Pair 48 size / vtbl / extras? | **52** / **`012429EC`** / ctor-zero `+40` `+44` `+48` | **PROVEN** |
| Pair 49 factory shape? | `00BFEA1A(100)` then **`jmp 004E3E2A`**. | **PROVEN** |
| Pair 49 size / vtbl / extras? | **100** / **`01243054`** / `00430345` at `+40`; `[esi+64]=-1` | **PROVEN** |
| Kid PALSKIN / 4300 first-seen apply? | Registrar only on this walk. Dest is bind. Live Present is adult **4299**. | **DISPROVEN** as first-seen dest. See §Kid. |
| `CTrapDef` in StartOakVale TNG? | **No** on `StartOakValeWest` (874 things, 0 `TRAP`). | **DISPROVEN** West. East/Garden **UNREAD**. |
| Class objects on `004EE23F`? | **Yes.** `0044C6B0` calls each factory once. | **PROVEN** leftover |
| Next pair? | **`CParticleAttacherDef`** `004F4628` / `004F462F` / `0x4E2AFA` | **PROVEN** name/sites/factory |

Authority: `Fable.exe`
`listing-004c0000.txt` `004F40D1` /
`004E3DD9` / `004E2895` / `004E5CF2` /
`004E3E2A`; `fn 004F40F9`;
`proofs/004EE23F-remaining-pairs` rows 48–49;
`proofs/004F3E4C-drain-fireball`.
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01243F24` **`CSkeletalMorphDef`**,
`0x01243F18` **`CTrapDef`**.
`assembly/exe/00-index/vtbl.tsv`
`0x012429EC` / `0x01243054`.
`tools/Fable.ExeIndex/out/startoak-tng.txt`.

Listing strings at `004F40D1` /
`004F439E` are **not** invented. Shape-2
(`push` + `0042DAE0`). Remaining-pairs
**MATCH**.

---

## `CSkeletalMorphDef` (row 48)

| Field | Value | Class |
| --- | --- | --- |
| `0044C6B0` | `004F40F9` | **PROVEN** |
| `009B0AC0` | `004F4100` | **PROVEN** |
| Factory | `004E3DD9` `00BFEA1A(52)` then `jmp 004E2895`; vtbl **`012429EC`** | **PROVEN** |
| Persist ctor | `004E2895` **is** the jmp dest: `0044C0C0` + vtbl + three zero dwords | **PROVEN** |
| Size | **52** (`push 52` at factory; vtbl[20] `004E28B2` `push 52; pop eax; ret`) | **PROVEN** |
| CTC between 47 and 48 | **4** unnamed `004D2EF0` | **PROVEN** count; names **UNREAD** |

```
004F40D1  push "CSkeletalMorphDef"
004F40E1  push 0x4E3DD9
004F40F3  call 0042DAE0
004F40F9  call 0044C6B0
004F4100  call 009B0AC0
```

```
004E3DD9  push 52
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004E3DEC
          mov ecx, eax
          jmp 004E2895
004E3DEC  xor eax, eax
          ret

004E2895  push esi
          mov esi, ecx
          call 0044C0C0
          xor eax, eax
          mov [esi], 0x12429EC
          mov [esi+40], eax
          mov [esi+44], eax
          mov [esi+48], eax
          mov eax, esi
          pop esi
          ret

004E28B2  push 52
          pop eax
          ret
```

`0044C0C0` is the 40-byte base. Factory
**does** `jmp` the persist ctor (same
shape as `CQuestCardDef` /
`CFlammableDef`, not the inline
drain/fireball pair). Extra dwords
`+40` `+44` `+48` are ctor-zero.

`vtbl.tsv` `0x012429EC` slot 18
`004E7E36` (`add ecx, 40` then
`004657F2`). Slot 0 dtor `004E3DEF`.
Slots 1–17 / 21–24 are the shared
`0042D930`… family. No invented names.

`00786470` / `00787060` intern the same
string (`push -1` + `0099EBF0`). That is
**not** Add Def Class.

`game.bin`: **65** `CSkeletalMorphDef`
(`INDEX.md`), including
`NULLDEF_CSkeletalMorphDef` idx **47**
raw **11**.

---

## 4 unnamed CTC (pair 47 → 48)

`004D2EF0` after `CFireballSpellLevelDef`
`004F3F09` and before `004F40D1`. No
in-range `push "CTC…"`. Helper
`004Dxxxx` name pushes are **not**
copied as in-range names.

| n | helper | factory `push` | `004D2EF0` |
| --: | --- | --- | --- |
| 1 | `004D58E3` | `0x4D58C6` | `004F3F47` |
| 2 | `004D5913` | `0x4D58F6` | `004F3FB2` |
| 3 | `004D5943` | `0x4D5926` | `004F401D` |
| 4 | `004D4819` | `0x4D47FC` | `004F4088` |

Row 4 helper `004D4819` (out of range)
pushes `"CTCSkeletalMorph"` then
`0099EBF0`. Factory `004D47FC` is
`00BFEA1A(36)` → `007865E0`. Remaining-pairs
still counts this row **unnamed**.

---

## Kid PALSKIN / 4300 first-seen apply?

No-save first Present is Lookout
`CREATURE_HERO` Graphic **4299**, not
Oakvale kid **4300**. Census:
`2026-08-18-first-scene-things.md`.
`CREATURE_HERO_CHILD` in Lookout TNG:
**False**.

| Claim | Answer | Class |
| --- | --- | --- |
| This pair *registers* `CSkeletalMorphDef` | Init Thing Components `004F40F9` / `004F4100` | **PROVEN** leftover |
| Adult Lookout 4299 uses this class | **No.** `CHeroMorphDef` idx **10535** on `CREATURE_HERO`. | **DISPROVEN** |
| Kid 4300 is this Present | **No.** Oakvale leftover `00DBDE40`. | **DISPROVEN** |
| First-seen PALSKIN dest is this morph | Bind / file triangles. `FirstSeenPlaysAnim=false`. | **DISPROVEN** |
| `CREATURE_HERO_CHILD` has this subdef? | 33 `SubDefs`. Adjacent cluster `SI_HERO_CHILD` **10541** then `CSkeletalMorphDef` **10542**. 33-row list not dumped. | **PARTIAL** cluster. List **UNREAD**. |
| `007868A0` apply on first-seen | vtbl `0126B6F4[4]`: find def `00787060`, `'SKEL'` `00786670`, then `00786700`. **No** `E8`. | **PROVEN** body. First callee **UNREAD**. |
| `00786870` dirty → `00786700` | Later anim / clothing UI. | **PROVEN** later. First frame bind. |
| `00835C80` `004C9D60("CTCSkeletalMorph")` | only `E8` is `0066407F` `_DEAD_CREATURE` | **DISPROVEN** as first |

**Answer:** `CSkeletalMorphDef` does
**not** apply on first-seen kid PALSKIN
dest. This walk only interns the class.
Live first Present never constructs
4300. Oakvale leftover spawn +
`007868A0` first callee stay **UNREAD**.
Do not treat bind dest as a morph
product.

`CHeroMorphDef` persist
(Strength/Will/Skill/Morality/Fatness/
Teenager) is a different class
(`004EE304`). Adult apply of those
floats is also **UNREAD**, not this pair.

---

## `CTrapDef` (row 49)

| Field | Value | Class |
| --- | --- | --- |
| `0044C6B0` | `004F43C6` | **PROVEN** |
| `009B0AC0` | `004F43CD` | **PROVEN** |
| Factory | `004E5CF2` `00BFEA1A(100)` then `jmp 004E3E2A`; vtbl **`01243054`** | **PROVEN** |
| Persist ctor | `004E3E2A` **is** the jmp dest | **PROVEN** |
| Size | **100** (`push 100` at factory; vtbl[20] `004E3E48` `push 100; pop eax; ret`) | **PROVEN** |
| CTC between 48 and 49 | **6** unnamed `004D2EF0` | **PROVEN** count; names **UNREAD** |

```
004F439E  push "CTrapDef"
004F43AE  push 0x4E5CF2
004F43C0  call 0042DAE0
004F43C6  call 0044C6B0
004F43CD  call 009B0AC0
```

```
004E5CF2  push 100
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004E5D05
          mov ecx, eax
          jmp 004E3E2A
004E5D05  xor eax, eax
          ret

004E3E2A  push esi
          mov esi, ecx
          call 0044C0C0
          lea ecx, [esi+40]
          mov [esi], 0x1243054
          call 00430345
          or [esi+64], -1
          mov eax, esi
          pop esi
          ret

004E3E48  push 100
          pop eax
          ret
```

`00430345` is the empty-string helper
on `+40` (not a name intern of
`CTrapDef`). `[esi+64]` starts at `-1`.

`vtbl.tsv` `0x01243054` slot 18
`004E5D08` (file persist: `+40` string,
`+52`/`+56` helpers, several
`00431061` / `00431102` dwords through
`+96`). Slot 0 dtor `004E5EEC`.

`00787BAB` / `00788E5D` intern the same
string. **Not** Add Def Class.

`game.bin`: **14** `CTrapDef`
(`INDEX.md`), including
`NULLDEF_CTrapDef` idx **48** raw
**107**. Compiled `OBJECT_*TRAP*`
owners are arena / cave / template
rows, not this registrar.

---

## 6 unnamed CTC (pair 48 → 49)

| n | helper | factory `push` | `004D2EF0` |
| --: | --- | --- | --- |
| 1 | `004D4901` | `0x4E28F1` | `004F413E` |
| 2 | `004D4914` | `0x4E292A` | `004F41A9` |
| 3 | `004D482C` | `0x4D877C` | `004F4214` |
| 4 | `004D48EE` | `0x4D48D1` | `004F427F` |
| 5 | `004D4944` | `0x4D4927` | `004F42EA` |
| 6 | `004D49A4` | `0x4D4987` | `004F4355` |

Helper `004D482C` (out of range) pushes
`"CTCSwitchableNavigation"`. Still
unnamed in-range.

---

## `CTrapDef` in StartOakVale TNG?

`004EE23F` is Init Thing Components. It
is **not** region / TNG / hero create.
**DISPROVEN** as `00DBDE40` NewThing.

`Fable.Dump tng` census
`tools/Fable.ExeIndex/out/startoak-tng.txt`:

```
Region StartOakValeWest: version=2 sections=4 things=874
```

**0** `TRAP` in Kind / DefinitionType /
ScriptName. No `OBJECT_ARROW_TRAP_*` /
`OBJECT_CAVE_BEARTRAP_*` /
`OBJECT_HERO_ARENA_*TRAP*` /
`OBJECT_TRAP_TEMPLATE`.

House-area first-seen objects within
25 m of `HerosOldHouse`
(`WorldGeometryTests`) are beds /
tables / lamps / fireplace / door /
chairs / cupboard / bookshelf. **No**
trap def.

| Map | Trap `DefinitionType`? | Class |
| --- | --- | --- |
| `StartOakValeWest` (intro 874) | **None** | **DISPROVEN** |
| `StartOakValeEast` | not in this dump | **UNREAD** |
| `StartOakvaleMemorialGarden` | not in this dump | **UNREAD** |
| Lookout first Present TNG | no trap census here; this walk is not Lookout load | n/a |

**Answer:** `CTrapDef` is **not** in
StartOakValeWest TNG. This pair does
not construct a trap Thing. East /
Garden dumps were not in
`startoak-tng.txt`.

---

## Host

`AddFirstDefClass` Notes through
forty-seventh `CFireballSpellLevelDef`
`004F3F02` then **returns**.

No `CSkeletalMorphDef`. No `CTrapDef`.
No `0x4E3DD9` / `0x4E5CF2`. Live 52- /
100-byte objects are **LEFTOVER**.
Sites **MATCH** remaining-pairs rows
48–49.

This investigation does **not** edit
`src/`.

---

## Next: `CParticleAttacherDef`

Remaining-pairs row 50. Five unnamed
`004D2EF0` after `004F43CD`
(`0x4D4957` / `0x4E7E46` / `0x4D87DF` /
`0x4D4A13` / `0x4E0BBD`) then:

```
004F4600  push "CParticleAttacherDef"
004F4610  push 0x4E2AFA
004F4622  call 0042DAE0
004F4628  call 0044C6B0
004F462F  call 009B0AC0
```

Factory `004E2AFA` is the **same jmp
shape** as pair 48: `00BFEA1A(52)` then
**`jmp 004E0B9C`**.

```
004E0B9C  push esi
          mov esi, ecx
          call 0044C0C0
          xor eax, eax
          mov [esi], 0x1242364
          mov [esi+40], eax
          mov [esi+44], eax
          mov [esi+48], eax
          mov eax, esi
          pop esi
          ret
004E0BB9  push 52
          pop eax
          ret
```

Size **52**, vtbl **`01242364`**, ctor-zero
`+40` `+44` `+48`. `vtbl.tsv` slot 20
`004E0BB9`. **PROVEN** name / sites /
factory / size / vtbl. **Not** shipped.

Lookout streetlamp sub `#11459` first
Present emit stays **UNREAD**
(`particles-first-seen`). Not this
registrar.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00780000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\startoak-tng.txt`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\assembly\compiled-defs\game\entries.tsv`
- `C:\FableCSharp\assembly\compiled-defs\game\INDEX.md`
- `C:\FableCSharp\proofs\004EE23F-remaining-pairs\README.md`
- `C:\FableCSharp\proofs\004F3E4C-drain-fireball\README.md`
- `C:\FableCSharp\proofs\bone-config-first\README.md`
- `C:\FableCSharp\proofs\palskin-child-hero\README.md`
- `C:\FableCSharp\docs\status\investigations\2026-08-18-first-scene-things.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs` (`AddFirstDefClass`; read only)
