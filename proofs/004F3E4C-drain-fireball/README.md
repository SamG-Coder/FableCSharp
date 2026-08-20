# `004EE23F` forty-sixth / forty-seventh pairs

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

Question: recover pairs 46–47
`CThingDrainLifeShotDef` `004F3E24` factory
`0x4D8D56` sites `004F3E4C` / `004F3E53`
(13 unnamed CTC) and
`CFireballSpellLevelDef` `004F3EDA` factory
`0x4D8D10` sites `004F3F02` / `004F3F09`.
Read factories: sizes, `0044C0C0` vs jmp
persist ctor, vtbl imm. First-seen childhood
Oakvale: are these constructed?

| Question | Answer | Class |
| --- | --- | --- |
| Pair 46 name / sites / factory? | **`CThingDrainLifeShotDef`** `004F3E4C` / `004F3E53` / `004D8D56` | **PROVEN** |
| Pair 47 name / sites / factory? | **`CFireballSpellLevelDef`** `004F3F02` / `004F3F09` / `004D8D10` | **PROVEN** |
| Factory shape? | Both **inline `0044C0C0`**. **Not** jmp persist ctor. | **PROVEN** |
| Pair 46 size / vtbl? | **60** / **`0123CCA4`** | **PROVEN** |
| Pair 47 size / vtbl? | **44** / **`0123CC3C`** | **PROVEN** |
| Childhood Oakvale Thing construct? | **No.** Registrar only. Not `00DBDE40`. | **DISPROVEN** |
| Class object on `004EE23F`? | **Yes.** `0044C6B0` calls each factory once. | **PROVEN** leftover |

Authority: `Fable.exe`
`listing-004c0000.txt` `004F3E24` /
`004D8D56` / `004D8D10` / `004D537C` /
`004D5392`; `fn 004F3E4C`;
`proofs/004EE23F-remaining-pairs` rows 46–47;
`proofs/004F3630-boast-volume`.
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01243F50` **`CThingDrainLifeShotDef`**,
`0x01243F38` **`CFireballSpellLevelDef`**.
`assembly/exe/00-index/vtbl.tsv`
`0x0123CCA4` / `0x0123CC3C`.

Listing strings at `004F3E24` /
`004F3EDA` are **not** invented. Shape-2
(`push` + `0042DAE0`). Remaining-pairs
**MATCH**.

---

## `CThingDrainLifeShotDef` (row 46)

| Field | Value | Class |
| --- | --- | --- |
| `0044C6B0` | `004F3E4C` | **PROVEN** |
| `009B0AC0` | `004F3E53` | **PROVEN** |
| Factory | `004D8D56` `00BFEA1A(60)` then `0044C0C0`; vtbl **`0123CCA4`** | **PROVEN** |
| Persist ctor | `004D5392` same `0044C0C0` + vtbl. Factory does **not** `jmp` here. | **PROVEN** |
| Size | **60** (`push 60` at factory; vtbl[20] `004D53A4` `push 60; pop eax; ret`) | **PROVEN** |
| CTC between 45 and 46 | **13** unnamed `004D2EF0` | **PROVEN** count; names **UNREAD** |

```
004F3E24  push "CThingDrainLifeShotDef"
004F3E34  push 0x4D8D56
004F3E46  call 0042DAE0
004F3E4C  call 0044C6B0
004F3E53  call 009B0AC0
```

```
004D8D56  push esi
          push 60
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D8D76
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123CCA4
          mov eax, esi
          pop esi
          ret
004D8D76  xor eax, eax
          pop esi
          ret
```

No extra dword stores after the vtbl write.
`0044C0C0` is the 40-byte base. Extra
`+40…+56` stay uninitialized until LoadDef
`004DEA13` (vtbl[18]).

---

## `CFireballSpellLevelDef` (row 47)

| Field | Value | Class |
| --- | --- | --- |
| `0044C6B0` | `004F3F02` | **PROVEN** |
| `009B0AC0` | `004F3F09` | **PROVEN** |
| Factory | `004D8D10` `00BFEA1A(44)` then `0044C0C0`; vtbl **`0123CC3C`** | **PROVEN** |
| Persist ctor | `004D537C` same `0044C0C0` + vtbl. Factory does **not** `jmp` here. | **PROVEN** |
| Size | **44** (`push 44` at factory; vtbl[20] `004D538E` `push 44; pop eax; ret`) | **PROVEN** |
| CTC between 46 and 47 | **1** unnamed `004D2EF0` (`push 0x4DEB83` at `004F3E86`) | **PROVEN** count; name **UNREAD** |

```
004F3EDA  push "CFireballSpellLevelDef"
004F3EEA  push 0x4D8D10
004F3EFC  call 0042DAE0
004F3F02  call 0044C6B0
004F3F09  call 009B0AC0
```

```
004D8D10  push esi
          push 44
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D8D30
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123CC3C
          mov eax, esi
          pop esi
          ret
004D8D30  xor eax, eax
          pop esi
          ret
```

LoadDef vtbl[18] `004DEA03` is `add ecx, 40`
then `00431102` (one dword after the base).

---

## `0044C0C0` vs jmp persist ctor

Siblings `CQuestCardDef` / `CFlammableDef` /
`CBedDef` alloc then **`jmp` persist ctor**.
These two factories **inline** `0044C0C0` +
vtbl write. Adjacent persist ctors exist
and write the **same** vtbls; factories do
**not** jump to them. No listing `E8` /
`jmp` to `004D537C` / `004D5392`.

```
004D537C  push esi
          mov esi, ecx
          call 0044C0C0
          mov [esi], 0x123CC3C
          mov eax, esi
          pop esi
          ret
004D538E  push 44
          pop eax
          ret

004D5392  push esi
          mov esi, ecx
          call 0044C0C0
          mov [esi], 0x123CCA4
          mov eax, esi
          pop esi
          ret
004D53A4  push 60
          pop eax
          ret
```

`007BBB08` / `007BBB1B` intern the same
strings (`push -1` + `0099EBF0`). That is
**not** Add Def Class.

---

## 13 unnamed CTC (pair 45 → 46)

`004D2EF0` after `CTCVolumeContainmentTrackerDef`
`004F3899` and before `004F3E24`. No in-range
`push "CTC…"`. Helper `004Dxxxx` name pushes
are **not** copied here.

| n | helper | factory `push` | `004D2EF0` |
| --: | --- | --- | --- |
| 1 | `004D6353` | `0x4D6336` | `004F38D7` |
| 2 | `004D5401` | `0x4DEACF` | `004F3942` |
| 3 | `004D5434` | `0x4D5414` | `004F39AD` |
| 4 | `004D53EE` | `0x4DEA93` | `004F3A18` |
| 5 | `004D53A8` | `0x4DEA57` | `004F3A83` |
| 6 | `004D5447` | `0x4D8D9C` | `004F3AEE` |
| 7 | `004D549F` | `0x4D8DD8` | `004F3B59` |
| 8 | `004D54DA` | `0x4DEB0B` | `004F3BC4` |
| 9 | `004D54ED` | `0x4D8E14` | `004F3C2F` |
| 10 | `004D5529` | `0x4DEB47` | `004F3C9A` |
| 11 | `004D5562` | `0x4D8E50` | `004F3D05` |
| 12 | `004D53DB` | `0x4D53BB` | `004F3D70` |
| 13 | `004D554F` | `0x4DCB3C` | `004F3DDB` |

---

## First-seen childhood Oakvale

`004EE23F` is Init Thing Components. It is
**not** region / TNG / hero create.
**DISPROVEN** as `00DBDE40` /
`Q_NewOakValeIntro` / childhood Oakvale
Thing construction.

No-save first Present is Lookout
(`CREATURE_HERO` 4299), not Oakvale kid
4300. Oakvale intro is leftover.

What **is** constructed on this walk: one
60-byte class object (`004D8D56`) and one
44-byte class object (`004D8D10`) when
`0044C6B0` consumes the factories. That is
the registrar, same as every leftover pair
after `CBedDef`. Not a drain-life shot.
Not a fireball.

`game.bin` still holds compiled rows
(`NULLDEF_CThingDrainLifeShotDef` + 1,
`NULLDEF_CFireballSpellLevelDef` + 4).
Those are def-manager load, not Oakvale
TNG `NewThing`. **UNREAD** here as a
first-seen apply.

`007BE88C` / `007BE92E` look up the same
class names (`[eax+56]` then `009ADA10`).
Callers `007BDF4D` /
`HERO_ABILITY_FIREBALL_EXPLOSION_*` are
ability persist, **not** Oakvale spawn.

---

## Host

`AddFirstDefClass` Notes through twenty-first
`CBedDef` `004F0E92` then returns.

No `CThingDrainLifeShotDef`. No
`CFireballSpellLevelDef`. No `0x4D8D56` /
`0x4D8D10`. Live 60- / 44-byte objects are
**LEFTOVER**. Sites **MATCH** remaining-pairs
rows 46–47.

---

## Next: `CSkeletalMorphDef`

Remaining-pairs row 48. Four unnamed
`004D2EF0` after `004F3F09`
(`0x4D58C6` / `0x4D58F6` / `0x4D5926` /
`0x4D47FC`) then:

```
004F40D1  push "CSkeletalMorphDef"
004F40E1  push 0x4E3DD9
004F40F3  call 0042DAE0
004F40F9  call 0044C6B0
004F4100  call 009B0AC0
```

Factory `004E3DD9` is the **other** shape:
`00BFEA1A(52)` then **`jmp 004E2895`**.
Persist ctor body / vtbl **UNREAD** here.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00780000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\assembly\compiled-defs\game\entries.tsv`
- `C:\FableCSharp\proofs\004EE23F-remaining-pairs\README.md`
- `C:\FableCSharp\proofs\004F3630-boast-volume\README.md`
- `C:\FableCSharp\proofs\004F34C4-quest-card\README.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs` (`AddFirstDefClass`; read only)
