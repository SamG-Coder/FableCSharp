# `004F5721` pairs 58–61: `CBossDef` / `CFishingDef` / `CGuardDef` / `CInterestingToVillagersDef`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` → `"Init Game"`
`0042F491` → `00418DCA` → `[vtbl+4]`
`004184BD` → `00418585` `004EE23F`.
Do **not** invent listing strings. Class
names are `push "…"` at the four sites.
CTC names below are helper-body `push "…"`
(not in-range on `004EE23F`; remaining-pairs
counted those rows unnamed).

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**MATCH**.

Question: recover remaining-pairs **58–61**
(`CBossDef` `004F56F9` factory `0x4E0D4C`
sites `004F5721` / `004F5728`;
`CFishingDef` `004F58F0` factory `0x4E0DB9`
sites `004F5918` / `004F591F`;
`CGuardDef` `004F5A11` factory `0x4D89EC`
sites `004F5A39` / `004F5A40`;
`CInterestingToVillagersDef` `004F5AC7`
factory `0x4D89B4` sites `004F5AEF` /
`004F5AF6`). Any first-seen Oakvale
childhood? Next pair is
`CActivateQuestDef` — note `004F5B7D`.

Authority: `Fable.exe`
`listing-004c0000.txt` (`004F566B`–
`004F5BAC`, `004E0D4C`, `004E0DB9`,
`004D89EC`, `004D89B4`, `004DE8C2`,
`004DE8F5`, `004D4FCF`, `004E7EE0`);
`listing-00780000.txt` `007B1F10`;
`proofs/004EE23F-remaining-pairs` rows
57–62; `proofs/004EE23F-twentyfirst-class`;
`strings.tsv` / `vtbl.tsv` / `rtti.txt`;
`assembly/compiled-defs/game/entries.tsv`.

All four pairs: shape-2 (`push` name +
factory + `0042DAE0` + `0044C6B0` +
`009B0AC0`). Status **PROVEN**.

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| Pair 58? | `CBossDef` `004F56F9` / `004F5721` / `004F5728` factory `004E0D4C` `00BFEA1A(84)` `jmp 004DE8C2`; vtbl **`0124185C`**; size **84** | **PROVEN** |
| Pair 59? | `CFishingDef` `004F58F0` / `004F5918` / `004F591F` factory `004E0DB9` `00BFEA1A(124)` `jmp 004DE8F5`; vtbl **`012418C4`**; size **124** | **PROVEN** |
| Pair 60? | `CGuardDef` `004F5A11` / `004F5A39` / `004F5A40` factory `004D89EC` `00BFEA1A(80)` in-line `0044C0C0`; vtbl **`0123C75C`**; size **80** | **PROVEN** |
| Pair 61? | `CInterestingToVillagersDef` `004F5AC7` / `004F5AEF` / `004F5AF6` factory `004D89B4` `00BFEA1A(44)` `jmp 004D4FCF`; vtbl **`0123C6D4`**; size **44**; `[+40]=1.0f` | **PROVEN** |
| Oakvale childhood? | **No.** Init Game leftover after `CBedDef`. No `00DBDE40` / `Q_NewOakValeIntro` / `S_QNOVI`. Live `CFishingDef` sits on CREATURE_HERO, not a pond thing. Boss rows sit with Whisper / Jack / wasp queen / scorpion king. | **DISPROVEN** |
| Next pair? | **`CActivateQuestDef`** `004F5B7D` / `004F5BA5` / `004F5BAC` factory `0x4D8A32`. One CTC between (helper `"CTCCarriedActionUseActivateQuest"`). | **PROVEN** sites; factory body already in `cactivatequestdef-payloads` |
| Host? | `AddFirstDefClass` returns after twenty-first `CBedDef`. These four are **LEFTOVER**. | **PROVEN** leftover |

---

## 0. Bound: pair 57 then one CTC

`listing-004c0000.txt` after
`COccupiableDef` (remaining-pairs row 57):

```
004F566B  call 0044C6B0
004F5672  call 009B0AC0
…
004F56A5  push 0x4D4E7C
004F56B0  call 004D2EF0          ; unnamed on 004EE23F
…
004F56F9  push "CBossDef"
```

Helper `004D4E99` (called at `004F5693`):
`push "CTCBoss"`. Remaining-pairs counted
this row unnamed (no in-range string).

---

## 1. Pair 58 `CBossDef`

| Field | Value | Class |
| --- | --- | --- |
| `push` | `004F56F9` `"CBossDef"` | **PROVEN** |
| `0044C6B0` | `004F5721` | **PROVEN** |
| `009B0AC0` | `004F5728` | **PROVEN** |
| Factory | `004E0D4C` `00BFEA1A(84)` then `jmp 004DE8C2` | **PROVEN** |
| Ctor | `004DE8C2` `0044C0C0`; `[esi]=0124185C`; zero `+40…+72` (nine dwords = three 12-byte vectors) | **PROVEN** |
| Size | **84** (`push 84` at factory; vtbl[20] `004DE8F1` `push 84; pop eax; ret`) | **PROVEN** |
| Vtbl | **`0124185C`** slot 0 `004E0D62`; 18 persist `004E7EE0`; 19 clone `004E2C03` | **PROVEN** |

`strings.tsv` `0x01243E6C` **`CBossDef`**.
Listing `004F56F9` `68 6C 3E 24 01`.
`xrefs.tsv` first hit `0x004F56FA`
`fn=0x004F47F0`. RTTI `0x0137AB24`.

```
004F56F9  push "CBossDef"
004F5709  push 0x4E0D4C
004F571B  call 0042DAE0
004F5721  call 0044C6B0
004F5728  call 009B0AC0
```

```
004E0D4C  push 84
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004E0D5F
          mov ecx, eax
          jmp 004DE8C2
004E0D5F  xor eax, eax
          ret

004DE8C2  push esi
          mov esi, ecx
          call 0044C0C0
          xor eax, eax
          mov [esi], 0x124185C
          mov [esi+40], eax
          mov [esi+44], eax
          mov [esi+48], eax
          mov [esi+52], eax
          mov [esi+56], eax
          mov [esi+60], eax
          mov [esi+64], eax
          mov [esi+68], eax
          mov [esi+72], eax
          mov eax, esi
          pop esi
          ret

004DE8F1  push 84
          pop eax
          ret
```

Dtor `004E0D7E` frees `[esi+64]`,
`[esi+52]`, `[esi+40]` then
`[esi]=01230BA0` `jmp 009FC550`.

Persist `004E7EE0` (slot 18):

| Off | Helper | Type |
| --- | --- | --- |
| `+40` | `00466A47` | dword vector (12 bytes) |
| `+52` | `00466A47` | dword vector |
| `+64` | `00466A47` | dword vector |
| `+76` | `00431102` | i32 |
| `+80` | `00431061` | f32 |

`00466A47` is the dword-vector persist
(`sar esi, 2` element size 4). Three
vectors + i32 + float = 12+12+12+4+4
= 44 extra; `40+44=84`. Ctor zeros the
vectors only; `+76` / `+80` filled by
persist. Clone `004E2C03` copies the
three vectors via `00454886` then
`+76` / `+80` as dwords.

Lionhead field names **UNREAD**.

`entries.tsv`: **18** rows (NULLDEF
index **57** + 17 live). Live neighbours
are battle defs (`CWhisperBattleDef`
9098, `CJackOfBladesBattleDef` 9112,
`CWaspQueenBattleDef` 10443,
`CScorpionKingBattleDef` 10467) and
`SI_HORNET_QUEEN` / `SI_SCORPION_KING`.
**DISPROVEN** Oakvale childhood.

Later string xrefs (not this walk):
`007AECC4` `fn=007AEBC0`; `007AFA98`
`fn=007AFA90`.

---

## 2. Pair 59 `CFishingDef`

Four `004D2EF0` between 58 and 59
(remaining-pairs unnamed). Helpers:

| Site | Factory imm | Helper | Helper `push "…"` |
| --- | --- | --- | --- |
| `004F575B` | `0x4D4EAC` | `004D4EC9` | `CTCRockTrollShield` |
| `004F57C6` | `0x4D8942` | `004D4EDC` | `CTCCombatLeader` |
| `004F5831` | `0x4D4F21` | `004D4F3E` | `CTCFishingSpot` |
| `004F589C` | `0x4D4F51` | `004D4F6E` | `CTCFishing` |

`CTCFishingSpot` is the pond-spot CTC,
**not** this Def pair. `CFishDef` is
remaining-pairs row **81** (`004F726C`).

| Field | Value | Class |
| --- | --- | --- |
| `push` | `004F58F0` `"CFishingDef"` | **PROVEN** |
| `0044C6B0` | `004F5918` | **PROVEN** |
| `009B0AC0` | `004F591F` | **PROVEN** |
| Factory | `004E0DB9` `00BFEA1A(124)` then `jmp 004DE8F5` | **PROVEN** |
| Ctor | `004DE8F5` `0044C0C0`; `[esi]=012418C4`; zero `+40` `+44` `+48` (empty vector) | **PROVEN** |
| Size | **124** (`push 124` at factory; vtbl[20] `004DE912`) | **PROVEN** |
| Vtbl | **`012418C4`** slot 0 `004E0DCF`; 18 persist `007B1F10`; 19 clone `004E2C4D` | **PROVEN** |

`strings.tsv` `0x01243E60` **`CFishingDef`**.
Listing `004F58F0` `68 60 3E 24 01`.
RTTI `0x0137AB3C`.

```
004F58F0  push "CFishingDef"
004F5900  push 0x4E0DB9
004F5912  call 0042DAE0
004F5918  call 0044C6B0
004F591F  call 009B0AC0
```

```
004E0DB9  push 124
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004E0DCC
          mov ecx, eax
          jmp 004DE8F5
004E0DCC  xor eax, eax
          ret

004DE8F5  push esi
          mov esi, ecx
          call 0044C0C0
          xor eax, eax
          mov [esi], 0x12418C4
          mov [esi+40], eax
          mov [esi+44], eax
          mov [esi+48], eax
          mov eax, esi
          pop esi
          ret

004DE912  push 124
          pop eax
          ret
```

Dtor `004E0DEB` frees `[esi+40]` then
`009FC550`.

Persist `007B1F10` (`listing-00780000.txt`).
Each field starts `push 0x122D70E`
(`00404500` empty-intern sentinel; not
a field CRC) then type-2 / type-3 arms:

| Off | Type-3 / type-2 |
| --- | --- |
| `+40` | `00466A47` dword vector |
| `+52` `+56` `+60` `+64` | `00993EE0` / `0040EFB0` |
| `+68` | `00993EB0` / `0040FE60` |
| `+72` `+76` `+80` `+84` `+88` `+92` `+96` `+100` `+104` `+108` | `00993EE0` / `0040EFB0` |
| `+112` `+116` | `00993EB0` / `0040FE60` |
| `+120` | `00993EE0` / `0040EFB0` |

Vector 12 + 18×4 = 84 extra;
`40+84=124`. Clone `004E2C4D`:
`00454886` on `+40`, then dword copies
`+52…+120`. Lionhead names **UNREAD**.

`entries.tsv`: **2** rows. NULLDEF
index **58**. Live **10514** sits in
the CREATURE_HERO cluster (`CHeroDef`
10508, `CHeroExperienceDef` 10513,
`COracleMinigameDef` 10515,
`CFireheartMinigameDef` 10516,
`CStealthDef` 10520, `CWeaponDef`
10526 `SWORD`). Hero fishing
capability, **not** an Oakvale pond
object. **DISPROVEN** as Oakvale
childhood first-seen.

Later string xrefs: `007B0714`
`fn=007B0710`; `007B2548`
`fn=007B2540`.

---

## 3. Pair 60 `CGuardDef`

Two `004D2EF0` between 59 and 60:

| Site | Factory imm | Helper | Helper `push "…"` |
| --- | --- | --- | --- |
| `004F5952` | `0x4D897B` | `004D4F81` | `CTCStealableItemLocation` |
| `004F59BD` | `0x4D5013` | `004D5030` | `CTCGuard` |

| Field | Value | Class |
| --- | --- | --- |
| `push` | `004F5A11` `"CGuardDef"` | **PROVEN** |
| `0044C6B0` | `004F5A39` | **PROVEN** |
| `009B0AC0` | `004F5A40` | **PROVEN** |
| Factory | `004D89EC` `00BFEA1A(80)` then in-line `0044C0C0` (no `jmp` thunk) | **PROVEN** |
| Ctor | vtbl write only; no extra stores | **PROVEN** |
| Size | **80** (`push 80` at factory; vtbl[20] `004D500F`) | **PROVEN** |
| Vtbl | **`0123C75C`** slot 0 `004D8A10`; 18 persist `004DE926`; 19 clone `004E0E23` | **PROVEN** |

`strings.tsv` `0x01243E54` **`CGuardDef`**.
Listing `004F5A11` `68 54 3E 24 01`.
RTTI `0x01379500`. Same in-line factory
shape as twentieth `CPerceivedThingDef`.

```
004F5A11  push "CGuardDef"
004F5A21  push 0x4D89EC
004F5A33  call 0042DAE0
004F5A39  call 0044C6B0
004F5A40  call 009B0AC0
```

```
004D89EC  push esi
          push 80
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D8A0C
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123C75C
          mov eax, esi
          pop esi
          ret
004D8A0C  xor eax, eax
          pop esi
          ret

004D500F  push 80
          pop eax
          ret
```

Persist `004DE926`: ten `00431102` i32
at `+40` `+44` `+48` `+52` `+56` `+60`
`+64` `+68` `+72` `+76`. Extra 40;
`40+40=80`. Clone `004E0E23` copies
those ten dwords after `00431F10`.
Dtor `004D8A10` is the shared
`[esi]=01230BA0` / `009FC550` (no
vector free). Field names **UNREAD**.

`entries.tsv`: **6** rows. NULLDEF
index **59**. Five live, raw **83**,
ASCII `{dq2`. Neighbours: creature /
`CAIScratchpadDef` `TestTarget` /
`SI_UNDEAD` / `COpinionOfHeroDef`.
**DISPROVEN** Oakvale childhood
(no childhood guards).

Later string xrefs: `007B2854`
`fn=007B2850`; `007B44E8`
`fn=007B44E0`.

---

## 4. Pair 61 `CInterestingToVillagersDef`

One `004D2EF0` between 60 and 61:

| Site | Factory imm | Helper | Helper `push "…"` |
| --- | --- | --- | --- |
| `004F5A73` | `0x4DC821` | `004D4FEA` | `CTCInterestingToVillagers` |

| Field | Value | Class |
| --- | --- | --- |
| `push` | `004F5AC7` `"CInterestingToVillagersDef"` | **PROVEN** |
| `0044C6B0` | `004F5AEF` | **PROVEN** |
| `009B0AC0` | `004F5AF6` | **PROVEN** |
| Factory | `004D89B4` `00BFEA1A(44)` then `jmp 004D4FCF` | **PROVEN** |
| Ctor | `004D4FCF` `0044C0C0`; `fld1` `fstp [esi+40]`; vtbl **`0123C6D4`** | **PROVEN** |
| Size | **44** (`push 44` at factory; vtbl[20] `004D4FE6`) | **PROVEN** |
| Vtbl | **`0123C6D4`** slot 0 `004D89CA`; 18 persist `004DE916`; 19 clone `004E0E0A` | **PROVEN** |

`strings.tsv` `0x01240C8C`
**`CInterestingToVillagersDef`**.
Listing `004F5AC7` `68 8C 0C 24 01`.
RTTI `0x013794D4`.

```
004F5AC7  push "CInterestingToVillagersDef"
004F5AD7  push 0x4D89B4
004F5AE9  call 0042DAE0
004F5AEF  call 0044C6B0
004F5AF6  call 009B0AC0
```

```
004D89B4  push 44
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004D89C7
          mov ecx, eax
          jmp 004D4FCF
004D89C7  xor eax, eax
          ret

004D4FCF  push esi
          mov esi, ecx
          call 0044C0C0
          fld1
          fstp [esi+40]
          mov [esi], 0x123C6D4
          mov eax, esi
          pop esi
          ret

004D4FE6  push 44
          pop eax
          ret
```

Persist `004DE916`: `add ecx, 40` then
`00431061` (f32 at `+40`). Default
**1.0**. Clone `004E0E0A` copies
`[edi+40]` after `00431F10`. Extra 4;
`40+4=44`. Raw game.bin **11**. Field
name **UNREAD**.

`entries.tsv`: **19** rows. NULLDEF
index **60**. Live clusters include
crate / shop (`CCrateStackDef` 11285–
11288 then 11289, then `CShopDef`
11292) and later village-object runs
(13697 / 13743 / 13913–13916). No
Oakvale / childhood instance name
(dump instance column is the type
name). **DISPROVEN** as this walk
being Oakvale; live payload names
**UNREAD**.

Later string xref: `004DC863`
`fn=004DC858` (CTC factory region).

---

## 5. Next pair (for the next agent)

One `004D2EF0` after 61, then pair 62:

```
004F5B29  push 0x4DC936
004F5B34  call 004D2EF0
…
004F5B7D  push "CActivateQuestDef"
004F5B8D  push 0x4D8A32
004F5B9F  call 0042DAE0
004F5BA5  call 0044C6B0
004F5BAC  call 009B0AC0
```

Helper `004D5043` `push
"CTCCarriedActionUseActivateQuest"`.
Remaining-pairs row 62: factory
`0x4D8A32` sites `004F5BA5` /
`004F5BAC`. Factory body / 48-byte
ctor `004D5056` already in
`proofs/cactivatequestdef-payloads`.
Do **not** treat those 16-byte rows
as New Game autostart.

---

## 6. Not Oakvale childhood

Parent of these four `009B0AC0` is
`004EE23F` Init Thing Components
after Leave → `FinalAlbion.wld`.
**No** `00DBDE40` / region / TNG /
hero create on these sites.

| Claim | Class |
| --- | --- |
| This walk *is* Oakvale childhood | **DISPROVEN** |
| `CBossDef` first-seen as childhood content | **DISPROVEN** (Whisper / JoB / wasp queen / scorpion king neighbours) |
| `CFishingDef` is the Oakvale pond | **DISPROVEN** (hero cluster; pond CTC is `CTCFishingSpot`; fish objects are `CFishDef` row 81) |
| `CGuardDef` first-seen childhood guards | **DISPROVEN** |
| `CInterestingToVillagersDef` first-seen Oakvale NPC | **DISPROVEN** as this walk; live instance names **UNREAD** |

Childhood fishing *use* of the hero
`CFishingDef` is a later Thing apply,
not this registrar. Do not invent
`ActivateQuest("Q_NewOakValeIntro")`
or an Oakvale pond spawn here.

---

## 7. Host leftover

`EngineLifecycle.AddFirstDefClass`
Notes through twenty-first `CBedDef`
(`004F0E92` / `004DA7F3` / `004D7A25`
/ size 60 / vtbl `0123E8BC`) then
**returns**.

Pairs 58–61 (and the CTC between)
are still leftover after that return.
Same leftover as remaining-pairs
§6 / twenty-first-class gap after
row 21. Note-only of these four would
**MATCH** the listing sites; live
84 / 124 / 80 / 44-byte objects stay
**LEFTOVER**.

Not a Thing instance. Not a file I/O
site.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `004F5672` | pair 57 `COccupiableDef` `009B0AC0` | **PROVEN** earlier |
| `004F56B0` | CTC `0x4D4E7C` helper `"CTCBoss"` | **PROVEN** helper string; unnamed in remaining-pairs |
| `004F56F9` / `004F5721` / `004F5728` | pair 58 `CBossDef` | **PROVEN** leftover |
| `004E0D4C` / `004DE8C2` | factory / ctor size 84 vtbl `0124185C` | **PROVEN** |
| `004F58F0` / `004F5918` / `004F591F` | pair 59 `CFishingDef` | **PROVEN** leftover |
| `004E0DB9` / `004DE8F5` | factory / ctor size 124 vtbl `012418C4` | **PROVEN** |
| `004F5A11` / `004F5A39` / `004F5A40` | pair 60 `CGuardDef` | **PROVEN** leftover |
| `004D89EC` | factory size 80 vtbl `0123C75C` | **PROVEN** |
| `004F5AC7` / `004F5AEF` / `004F5AF6` | pair 61 `CInterestingToVillagersDef` | **PROVEN** leftover |
| `004D89B4` / `004D4FCF` | factory / ctor size 44 vtbl `0123C6D4` | **PROVEN** |
| `004F5B7D` | pair 62 `CActivateQuestDef` (next) | **PROVEN** name/sites; body elsewhere |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00780000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\assembly\exe\00-index\rtti.txt`
- `C:\FableCSharp\assembly\exe\00-index\xrefs.tsv`
- `C:\FableCSharp\assembly\compiled-defs\game\entries.tsv`
- `C:\FableCSharp\proofs\004EE23F-remaining-pairs\README.md`
- `C:\FableCSharp\proofs\004EE23F-twentyfirst-class\README.md`
- `C:\FableCSharp\proofs\cactivatequestdef-payloads\README.md`
