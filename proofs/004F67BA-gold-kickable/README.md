# `004EE23F` remaining pairs 69–74: gold / will-power / kickable / tavern games

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI` /
`OBJECT_GOLD_1` / chicken kick. After Leave
this walk is `FinalAlbion.wld` (`0042F44D`) →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD` → `00418585` `004EE23F`.
Do **not** invent class names: only
`push "…"` listing strings.
Do **not** invent `ActivateQuest`. Helper
`"CTCActionUseActivateQuest"` is a CTC
type-name intern, not pair 62 and not
`ActivateQuest("Q_NewOakValeIntro")`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: recover remaining-pairs rows
**69–74** (`CGoldDef` …
`CTavernGameCoinBaseDef`). For each factory:
size, ctor (`0044C0C0` vs jmp persist), vtbl.
Childhood Oakvale gold / kickable?

| n | `0044C6B0` | `009B0AC0` | Factory | Size | Ctor | Vtbl | Class |
| --- | --- | --- | --- | ---: | --- | --- | --- |
| 69 | `004F67BA` | `004F67C1` | `004D8EC5` | **44** | `0044C0C0` in-line | **`0123D2EC`** | **PROVEN** |
| 70 | `004F6946` | `004F694D` | `004D926A` | **44** | `0044C0C0` in-line | **`0123DAA4`** | **PROVEN** |
| 71 | `004F6991` | `004F6998` | `004D7C2D` | **84** | `0044C0C0` in-line | **`0123A7AC`** | **PROVEN** |
| 72 | `004F69DC` | `004F69E3` | `004E2D3B` | **420** (`0x1A4`) | `jmp 004E1049` | **`012424BC`** | **PROVEN** |
| 73 | `004F6A27` | `004F6A2E` | `004E2DB2` | **132** (`0x84`) | `jmp 004E1195` | **`0124258C`** | **PROVEN** |
| 74 | `004F6A72` | `004F6A79` | `004D8F51` | **68** | `0044C0C0` in-line | **`0123D44C`** | **PROVEN** |

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
(`004F63B3`…`004F6B00`; factories
`004D8EC5` / `004D926A` / `004D7C2D` /
`004E2D3B` / `004E2DB2` / `004D8F51`;
ctors `004E1049` / `004E1195`;
size helpers `004D584F` / `004D5E10` /
`004D33C2` / `004E1086` / `004E11BD` /
`004D5991`);
`listing-00440000.txt` `0044C0C0`;
`proofs/004EE23F-remaining-pairs` rows 69–75;
`proofs/004F5D7C-overhead-tavern` pair 68.
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01243DE4` **`CGoldDef`**,
`0x01243DC0` **`CAICreatureWillPowerIndicatorDef`**,
`0x01243DB0` **`CKickableDef`**,
`0x01243DA0` **`CTavernGameDef`**,
`0x01243D88` **`CTavernGameCardBaseDef`**,
`0x01243D70` **`CTavernGameCoinBaseDef`**.
`assembly/exe/00-index/vtbl.tsv` the six
vtbls. `rtti.txt` `.?AVC…Def@@`.

All six are shape-2 (`push` + `0042DAE0`).
Listing strings are **not** invented.

Zero-CTC cluster (remaining-pairs):
`CKickableDef` … `CTavernGameCoinBaseDef`
(4 adjacent Def pairs, no `004D2EF0`
between). **MATCH**.

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| Row 69 name / sites / factory? | **`CGoldDef`** `004F6792` / `004F67BA` / `004F67C1` / `0x4D8EC5`. | **PROVEN** |
| Row 69 size / ctor / vtbl? | **44**; `0044C0C0` then `[esi]=0123D2EC`; vtbl[20] `004D584F` `push 44`. Persist intern `+40`. | **PROVEN** size/vtbl; `+40` payload **UNREAD** |
| Row 70 name / sites / factory? | **`CAICreatureWillPowerIndicatorDef`** `004F691E` / `004F6946` / `004F694D` / `0x4D926A`. | **PROVEN** |
| Row 70 size / ctor / vtbl? | **44**; `0044C0C0` then `[esi]=0123DAA4`; vtbl[20] `004D5E10` `push 44`. Persist f32 `+40`. | **PROVEN** size/vtbl; `+40` payload **UNREAD** |
| Row 71 name / sites / factory? | **`CKickableDef`** `004F6969` / `004F6991` / `004F6998` / `0x4D7C2D`. 0 CTC. | **PROVEN** |
| Row 71 size / ctor / vtbl? | **84**; `0044C0C0` then `[esi]=0123A7AC`; vtbl[20] `004D33C2` `push 84`. Copy copies `+80` as byte. | **PROVEN** size/vtbl; persist payload **UNREAD** |
| Row 72 name / sites / factory? | **`CTavernGameDef`** `004F69B4` / `004F69DC` / `004F69E3` / `0x4E2D3B`. 0 CTC. | **PROVEN** |
| Row 72 size / ctor / vtbl? | **420** (`push 0x1A4`); `jmp 004E1049`; vtbl **`012424BC`**; vtbl[20] `004E1086` `mov eax, 0x1A4`. | **PROVEN** |
| Row 73 name / sites / factory? | **`CTavernGameCardBaseDef`** `004F69FF` / `004F6A27` / `004F6A2E` / `0x4E2DB2`. 0 CTC. | **PROVEN** |
| Row 73 size / ctor / vtbl? | **132** (`push 0x84`); `jmp 004E1195`; vtbl **`0124258C`**; vtbl[20] `004E11BD` `mov eax, 0x84`. | **PROVEN** |
| Row 74 name / sites / factory? | **`CTavernGameCoinBaseDef`** `004F6A4A` / `004F6A72` / `004F6A79` / `0x4D8F51`. 0 CTC. | **PROVEN** |
| Row 74 size / ctor / vtbl? | **68**; `0044C0C0` then `[esi]=0123D44C`; vtbl[20] `004D5991` `push 68`. | **PROVEN** size/vtbl; persist payload **UNREAD** |
| Childhood Oakvale gold / kickable? | **No.** Init Thing Components class register. Not `00DBDE40`. Not `OBJECT_GOLD_1`. Not chicken kick. | **DISPROVEN** |
| Invent `ActivateQuest` here? | **No.** Eighth CTC helper after 68 pushes `"CTCActionUseActivateQuest"`. Not pair 62. Not `Q_NewOakValeIntro`. | **DISPROVEN** |
| Next pair? | **75** `CTavernGameShoveHaPennyDef` `004F6B00` / `004F6B28` / `004F6B2F` factory `0x4E2D70` `00BFEA1A(512)` `jmp 004E1105`; vtbl **`01242524`**. | **PROVEN** |
| Host live objects? | **None.** Notes through 74 are Note-only + flags. Live 44 / 44 / 84 / 420 / 132 / 68-byte objects are **LEFTOVER**. | **PROVEN** leftover |

**Answer:** six leftover Add Def Class
pairs. Factories allocate 44 / 44 / 84 /
420 / 132 / 68. Four in-line `0044C0C0` +
vtbl write; `CTavernGameDef` and
`CTavernGameCardBaseDef` are the jmp-thunk
shape. Not Oakvale. Not a Thing instance.
Not a file I/O site. Not `ActivateQuest`.

---

## 1. Bound: pair 68 then nine CTC

`listing-004c0000.txt` after 68
`CDrunkennessDef` `004F63B3`:

Nine unnamed `004D2EF0` rows. Helper
listing strings (same file, other fns;
`004EE23F` itself does **not** push
them):

| Helper at | `push` factory | Helper `push "…"` |
| --- | --- | --- |
| `004F63D4` `004D532E` | `0x4D8CD7` | `"CTCOnHeroPush"` |
| `004F643F` `004D55BD` | `0x4D559D` | `"CTCInGameMenu"` |
| `004F64AA` `004D6056` | `0x4D6039` | `"CTCElectrocuteOnTouch"` |
| `004F6515` `004D55ED` | `0x4D55D0` | `"CTCCoopSpirit"` |
| `004F6580` `004D5756` | `0x4D8E8C` | `"CTCGeneratesExperienceOnKilling"` |
| `004F65EB` `004D57B4` | `0x4D5797` | `"CTCCreatureGeneratorCreator"` |
| `004F6656` `004D57E7` | `0x4D57C7` | `"CTCHeroReceiveItems"` |
| `004F66C1` `004D57FA` | `0x4DCB5C` | `"CTCActionUseActivateQuest"` |
| `004F672C` `004D582A` | `0x4D580D` | `"CTCGold"` |

Remaining-pairs counted those nine
unnamed. Helper names are **not**
invented from `004Dxxxx`; they are
`push "…"` in the helper bodies.

`"CTCActionUseActivateQuest"` is **not**
`CActivateQuestDef` (pair 62,
`004F5BA5`) and is **not**
`ActivateQuest("Q_NewOakValeIntro")`.
No `008421C0` / `00843F50` on this
walk.

Then the sixty-ninth pair.

---

## 2. Pair 69 — `CGoldDef`

```
004F6792  push "CGoldDef"
004F67A2  push 0x4D8EC5
004F67B4  call 0042DAE0
004F67BA  call 0044C6B0
004F67C1  call 009B0AC0
```

`strings.tsv` `0x01243DE4` **`CGoldDef`**.

```
004D8EC5  push esi
          push 44
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D8EE5
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123D2EC
          mov eax, esi
          pop esi
          ret
004D8EE5  xor eax, eax
          pop esi
          ret
```

No extra stores after the vtbl write.
Placement ctor `004D583D` is the same
`0044C0C0` + `0123D2EC` (factory does
not `jmp` it).

`vtbl.tsv` `0x0123D2EC` slot 20 is
`004D584F`:

```
004D584F  push 44
          pop eax
          ret
```

Slot 0 is `004D8EE9` (`mov [esi], 0x1230BA0`
then `009FC550`). Slots 1–17 / 21–24 are
the shared `0042D930`…`0042DAA0` /
`009ACE90` / `009FBEF0` / `009ACAB0` /
`009ACB20` family. Slot 18 persist
`004DEBC4`:

```
004DEBC4  add ecx, 40
          push ecx
          mov ecx, [esp+8]
          call 00431102
```

`00431102` is CString intern (same as
`CTavernDef` `+40`). Slot 19 copy
`004E0FED` copies dword `[+40]`. Factory
does not store `+40`.

`rtti.txt` `0x013797A0` `CGoldDef`.
Later leftover (not this register):
`006A93C0` type-name (`push -1` /
`"CGoldDef"` / `0099EBF0`);
`006AE320` typed HANDLE get
(`[vtbl+56]` → `009ADA10`).

`game.bin`: **23** rows, raw **11**,
subdefs **0**. Id **68** =
`NULLDEF_CGoldDef`; twenty-two unnamed
type-name rows (`13643`…`13664`).
Not `OBJECT_GOLD_1` (that is type
**OBJECT**, id **4644**, mesh **321**,
raw **416**, subdefs **6**).

---

## 3. Pair 70 — `CAICreatureWillPowerIndicatorDef`

Three unnamed `004D2EF0` after 69.
Helper listing strings:

| Helper at | `push` factory | Helper `push "…"` |
| --- | --- | --- |
| `004F67E2` `004D5853` | `0x4E1006` | `"CTCTargetingSpirit"` |
| `004F684D` `004D5956` | `0x4EA695` | `"CTCCreatureHitNotification"` |
| `004F68B8` `004D5E31` | `0x4D5E14` | `"CTCAICreatureWillPowerIndicator"` |

Then:

```
004F691E  push "CAICreatureWillPowerIndicatorDef"
004F692E  push 0x4D926A
004F6940  call 0042DAE0
004F6946  call 0044C6B0
004F694D  call 009B0AC0
```

`strings.tsv` `0x01243DC0`
**`CAICreatureWillPowerIndicatorDef`**.

```
004D926A  push esi
          push 44
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D928A
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123DAA4
          mov eax, esi
          pop esi
          ret
004D928A  xor eax, eax
          pop esi
          ret
```

Placement ctor `004D5DFE` writes the
same vtbl.

`vtbl.tsv` `0x0123DAA4` slot 20 is
`004D5E10`:

```
004D5E10  push 44
          pop eax
          ret
```

Slot 18 persist `004DF0F7`:

```
004DF0F7  add ecx, 40
          push ecx
          mov ecx, [esp+8]
          call 00431061
```

`00431061` is f32 (same as
`CInterestingToVillagersDef` `+40`).
Slot 19 copy `004E14FD` copies dword
`[+40]`. Factory does not store `+40`.

`rtti.txt` `0x0137993C`
`CAICreatureWillPowerIndicatorDef`.
Later leftover: `007E0DE0` type-name;
`007E1230` typed get.

`game.bin`: **6** rows, raw **11**,
subdefs **0**. Id **69** = `NULLDEF`;
five unnamed type-name rows.

---

## 4. Pair 71 — `CKickableDef` (0 CTC)

No `004D2EF0` between 70 and 71.
Adjacent Def pairs.

```
004F6969  push "CKickableDef"
004F6979  push 0x4D7C2D
004F698B  call 0042DAE0
004F6991  call 0044C6B0
004F6998  call 009B0AC0
```

`strings.tsv` `0x01243DB0` **`CKickableDef`**.

```
004D7C2D  push esi
          push 84
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D7C4D
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123A7AC
          mov eax, esi
          pop esi
          ret
004D7C4D  xor eax, eax
          pop esi
          ret
```

Placement ctor `004D33B0` writes the
same vtbl.

`vtbl.tsv` `0x0123A7AC` slot 20 is
`004D33C2`:

```
004D33C2  push 84
          pop eax
          ret
```

Slot 18 persist `004DDC74` reads
`+40` / `+44` / `+48` / `+52` / `+56` /
`+60` (`00431061` f32), `+64`
(`00431102` intern), `+68`
(`00456B7D` u32), `+72` (`00431102`),
`+76` (`00456B7D`), `+80` (`0043314A`
bool). Slot 19 copy `004E021D` →
`004E0222` copies those dwords then
`mov al, [edi+80]` / `mov [esi+80], al`.
Last field is a byte at `+80`; alloc
is 84 (three pad bytes). Factory does
not store `+40…+80`.

`rtti.txt` `0x01378F08` `CKickableDef`.
Later leftover: `006A9430` type-name;
`006AE260` typed get.

`game.bin`: **11** rows, raw **88**,
subdefs **0**. Id **70** = `NULLDEF`;
ten unnamed type-name rows. Not
`CREATURE_CHICKEN` / `CREATURE_KICKING_CHICKEN_01`
/ `OBJECT_OAKVALE_CHICKEN_STALL` /
`OBJECT_QUEST_CARD_CHICKEN_KICKING`
(those are CREATURE / OBJECT names,
not this Def pair).

---

## 5. Pair 72 — `CTavernGameDef` (0 CTC)

No `004D2EF0` between 71 and 72.

```
004F69B4  push "CTavernGameDef"
004F69C4  push 0x4E2D3B
004F69D6  call 0042DAE0
004F69DC  call 0044C6B0
004F69E3  call 009B0AC0
```

`strings.tsv` `0x01243DA0` **`CTavernGameDef`**.

Jmp-thunk shape (same as nineteenth
`004E0B4B` / sixty-seventh `004EC526`):

```
004E2D3B  push 0x1A4
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004E2D51
          mov ecx, eax
          jmp 004E1049
004E2D51  xor eax, eax
          ret

004E1049  push esi
          mov esi, ecx
          call 0044C0C0
          mov [esi], 0x12424BC
          or eax, -1
          mov [esi+144], eax
          mov [esi+284], eax
          mov [esi+288], eax
          mov [esi+292], eax
          lea eax, [esi+372]
          xor ecx, ecx
          mov [eax], ecx
          mov [eax+4], ecx
          mov [eax+8], ecx
          mov eax, esi
          pop esi
          ret

004E1086  mov eax, 0x1A4
          ret
```

`vtbl.tsv` `0x012424BC` slot 20 is
`004E1086` (size **420**). Slot 0
`004E2D54`. Slot 18 persist `004ED472`
starts at `+40` with a long
`00431020` (u32) walk, then f32 /
bool / intern / struct through
`+372` (`004ED953`) and past
`+396`. Inflated field walk
**UNREAD** here. Ctor does **not**
store `+40`; those wait for LoadDef.
Ctor defaults: `-1` at `+144` /
`+284` / `+288` / `+292`; 12-byte
zero at `+372`.

`rtti.txt` `0x0137AF1C` `CTavernGameDef`.
Later leftover: `005E9990` type-name;
`005ED4A0` typed get.

`game.bin`: **9** rows. Id **71** =
`NULLDEF` raw **741**; eight live
rows around **14040**…**14093**
(raw 741 / 773). Not
`BUILDING_OAKVALE_TAVERN` (type
**BUILDING**, id **757**).

---

## 6. Pair 73 — `CTavernGameCardBaseDef` (0 CTC)

No `004D2EF0` between 72 and 73.

```
004F69FF  push "CTavernGameCardBaseDef"
004F6A0F  push 0x4E2DB2
004F6A21  call 0042DAE0
004F6A27  call 0044C6B0
004F6A2E  call 009B0AC0
```

`strings.tsv` `0x01243D88`
**`CTavernGameCardBaseDef`**.

```
004E2DB2  push 0x84
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004E2DC8
          mov ecx, eax
          jmp 004E1195
004E2DC8  xor eax, eax
          ret

004E1195  push esi
          mov esi, ecx
          call 0044C0C0
          mov [esi], 0x124258C
          xor ecx, ecx
          mov [esi+108], ecx
          mov [esi+112], ecx
          lea eax, [esi+120]
          mov [esi+116], ecx
          mov [eax], ecx
          mov [eax+4], ecx
          mov [eax+8], ecx
          mov eax, esi
          pop esi
          ret

004E11BD  mov eax, 0x84
          ret
```

`vtbl.tsv` `0x0124258C` slot 20 is
`004E11BD` (size **132**). Slot 0
`004E2DCB`. Slot 18 persist `004F9A90`
reads `+40` (`004F9B63`), f32s,
interns, `004595A3`, `004568BC`,
`+108` (`004F9C07`), then
`add esi, 120` / `004FA0A9`. 12-byte
tail matches size 132. Payload
**UNREAD**. Ctor zeros `+108…+128`.

`rtti.txt` `0x0137AF68`
`CTavernGameCardBaseDef`. Later
leftover: `008F05B0` type-name;
`008F27A0` typed get.

`game.bin`: **5** rows. Id **72** =
`NULLDEF` raw **151**; four live
rows **14054** / **14056** / **14058**
/ **14060** (raw 983 / 1119 / 1375 /
1231).

---

## 7. Pair 74 — `CTavernGameCoinBaseDef` (0 CTC)

No `004D2EF0` between 73 and 74.

```
004F6A4A  push "CTavernGameCoinBaseDef"
004F6A5A  push 0x4D8F51
004F6A6C  call 0042DAE0
004F6A72  call 0044C6B0
004F6A79  call 009B0AC0
```

`strings.tsv` `0x01243D70`
**`CTavernGameCoinBaseDef`**.

```
004D8F51  push esi
          push 68
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D8F71
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123D44C
          mov eax, esi
          pop esi
          ret
004D8F71  xor eax, eax
          pop esi
          ret
```

Placement ctor `004D597F` writes the
same vtbl.

`vtbl.tsv` `0x0123D44C` slot 20 is
`004D5991`:

```
004D5991  push 68
          pop eax
          ret
```

Slot 18 persist `004DEC2C` reads
`+40` / `+44` / `+48` / `+52` / `+56`
(`00431061` f32) then `+60` / `+64`
(`00431102` intern). Slot 19 copy
`004E10C1` → `004E10C6` copies
`+40…+64`. Size 68 = through `+67`.
Factory does not store those fields.

`rtti.txt` `0x013797DC`
`CTavernGameCoinBaseDef`. Later
leftover: `008EE950` type-name;
`008F00F0` typed get.

`game.bin`: **3** rows. Id **73** =
`NULLDEF` raw **59**; two live rows
**14041** / **14049** (raw 59).

---

## 8. Base `0044C0C0` (four in-line)

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
`+40`. That is why 44 (one intern or
f32 at `+40`) sits next to 68 (five
f32 + two intern) and 84 (six f32 +
two intern + two u32 + bool).

The two jmp factories still call
`0044C0C0` inside `004E1049` /
`004E1195`.

---

## 9. Oakvale childhood gold / kickable — DISPROVEN

This walk is `004EE23F` Init Thing
Components. Remaining-pairs already
locked: **not** Oakvale.

| Claim | Status |
| --- | --- |
| Pairs 69–74 run on `00DBDE40` / `Q_NewOakValeIntro` | **DISPROVEN** |
| These sites spawn gold or a kickable Thing | **DISPROVEN** (class register only) |
| `CGoldDef` **is** `OBJECT_GOLD_1` | **DISPROVEN** (`OBJECT` id **4644**, not a Def pair) |
| Last-barrel gold is this register | **DISPROVEN** (WatchBarrels leftover `00DBE890` `vtbl+2340`) |
| `CKickableDef` **is** Oakvale chicken kick | **DISPROVEN** (`CREATURE_CHICKEN` / `CREATURE_KICKING_CHICKEN_01` / `OBJECT_QUEST_CARD_CHICKEN_KICKING` are other types) |
| `OBJECT_OAKVALE_CHICKEN_STALL` is this pair | **DISPROVEN** (OBJECT name only) |
| No-save first playable is gold / kickable | **DISPROVEN** (LookoutPoint / GuildArrivalHSP) |
| `CTavernGameDef` is Oakvale tavern | **DISPROVEN** (sibling `004F5D7C`; remaining-pairs row **72** is this cluster, still not Oakvale) |
| Eighth CTC is `ActivateQuest("Q_NewOakValeIntro")` | **DISPROVEN** (helper `"CTCActionUseActivateQuest"` only) |

`CGoldDef` / `CKickableDef` /
`CTavernGameDef` are type registrars
for every gold pile, kickable, and
tavern minigame in Albion. Oakvale
childhood does not special-case these
six `009B0AC0` sites.

`OBJECT_GOLD_1` exists in `game.bin`
(**PROVEN** later leftover, WatchBarrels
last smash). `CREATURE_CHICKEN` exists
as a name (**PROVEN** name only; not
this register).

---

## 10. Host

`EngineLifecycle.AddFirstDefClass`
Notes through seventy-fourth
`CTavernGameCoinBaseDef`
(`004F6A72` / `004D8F51` / size 68 /
vtbl `0123D44C`) then **returns**.

Constants for 69–74 **MATCH** this
file (sites / factories / ctors /
sizes / vtbls). Notes are Note-only +
flags, not `00BFEA1A` / vtbl write.

`src/` has **0** live objects for
these six classes. Tests lock pair 68
(`CDrunkennessDef`); 69–74 flags are
untested here.

| After 68 | Native | Host |
| --- | --- | --- |
| 69 `CGoldDef` size 44 vtbl `0123D2EC` | **PROVEN** (this file) | Note-only **LEFTOVER** object |
| 70 `CAICreatureWillPowerIndicatorDef` size 44 vtbl `0123DAA4` | **PROVEN** | Note-only **LEFTOVER** object |
| 71 `CKickableDef` size 84 vtbl `0123A7AC` | **PROVEN** | Note-only **LEFTOVER** object |
| 72 `CTavernGameDef` size 420 vtbl `012424BC` | **PROVEN** | Note-only **LEFTOVER** object |
| 73 `CTavernGameCardBaseDef` size 132 vtbl `0124258C` | **PROVEN** | Note-only **LEFTOVER** object |
| 74 `CTavernGameCoinBaseDef` size 68 vtbl `0123D44C` | **PROVEN** | Note-only **LEFTOVER** object |
| 75 `CTavernGameShoveHaPennyDef` … 111 `CHasNameDef` | remaining-pairs | **LEFTOVER** |

Live 44 / 44 / 84 / 420 / 132 /
68-byte objects are **LEFTOVER**.
`+40` intern / f32 and the kickable /
tavern-game tables are **UNREAD** in
the host (there is no object).

---

## 11. Next pair 75

One unnamed `004D2EF0` after 74
(`push 0x4D5995` at `004F6AAC`, helper
`004D59B5` at `004F6A9A` pushes
`"CTCTavernGameShoveHaPenny"`). Then:

```
004F6B00  push "CTavernGameShoveHaPennyDef"
004F6B10  push 0x4E2D70
004F6B22  call 0042DAE0
004F6B28  call 0044C6B0
004F6B2F  call 009B0AC0
```

Factory (same listing, not this
cluster’s object):

```
004E2D70  push 0x200
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004E2D86
          mov ecx, eax
          jmp 004E1105
```

`004E1105` calls `004E1049`
(`CTavernGameDef` ctor) then overwrites
`[esi]=01242524` and zeros `+420`.
vtbl[20] `004E1127` `mov eax, 0x200`.
Size **512**. Factory body **PROVEN**
in this listing; remaining-pairs row
75 name/sites **MATCH**. Host has **0**
Notes for 75.

---

## Original

Sixty-ninth through seventy-fourth Add
Def Class on `004EE23F`:

1. `0099EBF0` name (six listing strings).
2. `0042DAE0` packs factory.
3. `0044C6B0` / `009B0AC0` at the six
   site pairs.
4. Four factories: `00BFEA1A` +
   `0044C0C0` + vtbl. Two factories:
   `00BFEA1A(420)` + `jmp 004E1049`;
   `00BFEA1A(132)` + `jmp 004E1195`.

CTC between (remaining-pairs counts;
helper names from the helper `push`
in this listing, not from
`004EE23F` itself): 9 + 3 + 0 + 0 +
0 + 0.

Not Oakvale. Not a Thing. Not file I/O.
Not `ActivateQuest`.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00440000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00680000.txt` (`006A93C0` / `006A9430` / `006AE320`)
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\assembly\exe\00-index\rtti.txt`
- `C:\FableCSharp\assembly\compiled-defs\game\entries.tsv`
- `C:\FableCSharp\assembly\compiled-defs\game\INDEX.md`
- `C:\FableCSharp\assembly\compiled-defs\names.tsv`
- `C:\FableCSharp\proofs\004EE23F-remaining-pairs\README.md`
- `C:\FableCSharp\proofs\004F5D7C-overhead-tavern\README.md`
- `C:\FableCSharp\proofs\004F5BA5-activate-quest-def\README.md`
- `C:\FableCSharp\proofs\watchbarrels-00DBE890\README.md`
