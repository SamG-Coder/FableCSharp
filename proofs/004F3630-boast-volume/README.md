# `004EE23F` pairs 44–45: `CBoastingPodiumDef` / `CTCVolumeContainmentTrackerDef`

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
**out of range**. Remaining-pairs: after
`CTCActionUseSearch` later `004D2EF0` rows
are unnamed.

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**MATCH** / **UNKNOWN**.

Question: recover remaining-pairs **44–45**
from `listing-004c0000.txt`. Confirm
`CBoastingPodiumDef` `004F3608` factory
`0x4D8736` (`push 44`, `0044C0C0`, vtbl
`0x0123BF0C`) sites `004F3630` /
`004F3637` and
`CTCVolumeContainmentTrackerDef`
`004F386A` factory `0x4D94C8` (`push 48`,
`0044C0C0`, vtbl `0x0123E0C4`) sites
`004F3892` / `004F3899`. Any Oakvale
childhood / Maze use? `game.bin`
instance names. Next pair
`CThingDrainLifeShotDef`.

Authority: `Fable.exe`
`listing-004c0000.txt` (`004F3581`–
`004F3E53`, `004D8736`, `004D94C8`,
`004D46D0`, `004D46E3`, `004D46F5`,
`004D6320`, `004D6332`);
`listing-00800000.txt` `00813930` /
`008148C0`; `listing-00780000.txt`
`00784D10` / `00785010`;
`proofs/004EE23F-remaining-pairs` rows
43–46; `proofs/004F34C4-quest-card`;
`proofs/004F3338-hero-centre`;
`proofs/004F3E4C-drain-fireball`;
`strings.tsv` `0x01243F88` /
`0x01243F68`; `vtbl.tsv` `0x0123BF0C` /
`0x0123E0C4`; `rtti.txt`;
`assembly/compiled-defs/game/entries.tsv`
/ `INDEX.md` / `names.tsv`;
`docs/status/investigations/2026-08-18-first-scene-things.dump.txt`.

Both pairs: shape-2 (`push` name +
factory + `0042DAE0` + `0044C6B0` +
`009B0AC0`). Listing **MATCH**
remaining-pairs. Status **PROVEN**.

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| Pair 44? | `CBoastingPodiumDef` `004F3608` / `004F3630` / `004F3637` factory `004D8736` `00BFEA1A(44)` in-line `0044C0C0`; vtbl **`0123BF0C`**; size **44** | **PROVEN** |
| Pair 45? | `CTCVolumeContainmentTrackerDef` `004F386A` / `004F3892` / `004F3899` factory `004D94C8` `00BFEA1A(48)` in-line `0044C0C0`; vtbl **`0123E0C4`**; size **48** | **PROVEN** |
| Listing vs remaining-pairs 44–45? | Name / factory imm / sites / CTC counts **MATCH** | **PROVEN** |
| Oakvale childhood use? | **No** on this walk. Not `00DBDE40` / `Q_NewOakValeIntro` / `S_QNOVI`. Kid `SI_HERO_CHILD` cluster has **neither** type. | **DISPROVEN** |
| Maze use? | **No** as these Def types. `SI_HERO_MAZE` / `CMazeBattleDef` cluster has **neither**. Lookout `MK_GTA_MAZE*` are `MARKER_BASIC`. | **DISPROVEN** |
| `game.bin` instance names (this type)? | Pair 44: **`NULLDEF_CBoastingPodiumDef`**, **`CBoastingPodiumDef`**. Pair 45: **`NULLDEF_CTCVolumeContainmentTrackerDef`** + four unnamed **`CTCVolumeContainmentTrackerDef`**. | **PROVEN** |
| Host? | Notes already shipped for both. **Not** live 44- / 48-byte objects. | **MATCH** Notes; live ctor **LEFTOVER** |
| Next pair? | **`CThingDrainLifeShotDef`** `004F3E24` / `004F3E4C` / `004F3E53` factory `0x4D8D56`. Thirteen unnamed CTC between. | **PROVEN** sites (`004F3E4C-drain-fireball`) |

---

## 0. Bound: pair 43 then one CTC

`listing-004c0000.txt` after
`CFlammableDef` (remaining-pairs row 43):

```
004F357A  call 0044C6B0
004F3581  call 009B0AC0
…
004F35A2  call 004D46D0          ; helper "CTCBoastingPodium"
004F35B4  push 0x4E7E19
004F35BF  call 004D2EF0          ; unnamed on 004EE23F
…
004F3608  push "CBoastingPodiumDef"
```

Exactly **one** `004D2EF0` between
`004F3581` and `004F3608`.
Remaining-pairs row 44 CTC column = 1
unnamed. **MATCH**.

---

## 1. Pair 44 `CBoastingPodiumDef`

| Field | Value | Class |
| --- | --- | --- |
| `push` | `004F3608` `"CBoastingPodiumDef"` | **PROVEN** |
| `0044C6B0` | `004F3630` | **PROVEN** |
| `009B0AC0` | `004F3637` | **PROVEN** |
| Factory | `004D8736` `00BFEA1A(44)` then `0044C0C0`; `[esi]=0123BF0C` | **PROVEN** |
| Twin ctor | `004D46E3` same `0044C0C0` + vtbl. Factory does **not** `jmp` here. | **PROVEN** |
| Size | **44** (`push 44` at factory; vtbl[20] `004D46F5` `push 44; pop eax; ret`) | **PROVEN** |
| Vtbl | **`0123BF0C`** slot 0 `004D875A`; 18 persist `004DE772`; 19 clone `004E0A4E`; 20 size `004D46F5` | **PROVEN** |
| CTC between 43 and 44 | **1** unnamed (`0x4E7E19`) | **PROVEN** count |

`strings.tsv` `0x01243F88`
**`CBoastingPodiumDef`**. Listing
`004F3608` `68 88 3F 24 01`.
`xrefs.tsv` first hit `0x004F3609`
`fn=0x004F34EE`. RTTI `0x013793A0`
`.?AVCBoastingPodiumDef@@`.
`names.tsv` `0x00006D8D` CRC
`0xE513A352`.

```
004F3608  push "CBoastingPodiumDef"
004F360D  lea ecx, [ebp-1552]
004F3613  call 0099EBF0
004F3618  push 0x4D8736
004F361D  lea eax, [ebp-1552]
004F3623  push eax
004F3624  lea ecx, [ebp-2308]
004F362A  call 0042DAE0
004F362F  push eax
004F3630  call 0044C6B0
004F3635  mov ecx, eax
004F3637  call 009B0AC0
```

```
004D8736  push esi
          push 44
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D8756
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123BF0C
          mov eax, esi
          pop esi
          ret
004D8756  xor eax, eax
          pop esi
          ret

004D46E3  push esi
          mov esi, ecx
          call 0044C0C0
          mov [esi], 0x123BF0C
          mov eax, esi
          pop esi
          ret

004D46F5  push 44
          pop eax
          ret
```

No extra dword stores after the vtbl
write. Persist `004DE772` is
`add ecx, 40` then `00431061` (one
float at `+40`). Size 44 = 40 + 4.
`raw` **11** = 3-byte compiled-def
header + 8 (CRC + float). **MATCH**.

Later string xrefs (not this walk):
`00813934` `fn=00813930` and
`008148C8` `fn=008148C0` intern the
same name (`push -1` + `0099EBF0`).
That is **not** Add Def Class.
`00813460` is Guild boast UI
(`TEXT_GUI_GOLD` / “You do not have
enough money to make this boast!”).
Not first-seen childhood.

---

## 2. Five unnamed CTC (pair 44 → 45)

`004D2EF0` after `004F3637` and before
`004F386A`. No in-range `push "CTC…"`.
Helper name pushes are **out of range**.

| `004D2EF0` | Factory `push` | Helper | Helper listing (out of range) |
| --- | --- | --- | --- |
| `004F3675` | `0x4D46F9` | `004D4716` | `"CTCBoastingPosition"` |
| `004F36E0` | `0x4D4729` | `004D4746` | `"CTCBoastingArea"` |
| `004F374B` | `0x4D4759` | `004D4776` | `"CTCBoastingCrowdControl"` |
| `004F37B6` | `0x4D479C` | `004D47B9` | `"CTCCreatureGroupBoastingCrowd"` |
| `004F3821` | `0x4D47CC` | `004D47E9` | `"CTCVolumeContainmentTracker"` |

Remaining-pairs row 45 CTC column = 5
unnamed. **MATCH**.

`CTCBoastingPodium` (the CTC row
before pair 44) is the component
type-name table, **not** this Def
pair. `CTCVolumeContainmentTracker`
(last of the five) is the same
relation to pair 45.

---

## 3. Pair 45 `CTCVolumeContainmentTrackerDef`

This is an Add Def Class pair
(`0044C6B0` / `009B0AC0`), **not** a
CTC row. Remaining-pairs already
locked that.

| Field | Value | Class |
| --- | --- | --- |
| `push` | `004F386A` `"CTCVolumeContainmentTrackerDef"` | **PROVEN** |
| `0044C6B0` | `004F3892` | **PROVEN** |
| `009B0AC0` | `004F3899` | **PROVEN** |
| Factory | `004D94C8` `00BFEA1A(48)` then `0044C0C0`; `[esi]=0123E0C4` | **PROVEN** |
| Twin ctor | `004D6320` same `0044C0C0` + vtbl. Factory does **not** `jmp` here. | **PROVEN** |
| Size | **48** (`push 48` at factory; vtbl[20] `004D6332` `push 48; pop eax; ret`) | **PROVEN** |
| Vtbl | **`0123E0C4`** slot 0 `004D94EC`; 18 persist `004DF32C`; 19 clone `004E1897`; 20 size `004D6332` | **PROVEN** |
| CTC between 44 and 45 | **5** unnamed | **PROVEN** count |

`strings.tsv` `0x01243F68`
**`CTCVolumeContainmentTrackerDef`**.
Listing `004F386A` `68 68 3F 24 01`.
`xrefs.tsv` first hit `0x004F386B`
`fn=0x004F3818`. RTTI `0x01379A74`
`.?AVCTCVolumeContainmentTrackerDef@@`.
`names.tsv` `0x00006DCF` CRC
`0x1C03A958`.

```
004F386A  push "CTCVolumeContainmentTrackerDef"
004F386F  lea ecx, [ebp-1424]
004F3875  call 0099EBF0
004F387A  push 0x4D94C8
004F387F  lea eax, [ebp-1424]
004F3885  push eax
004F3886  lea ecx, [ebp-2052]
004F388C  call 0042DAE0
004F3891  push eax
004F3892  call 0044C6B0
004F3897  mov ecx, eax
004F3899  call 009B0AC0
```

```
004D94C8  push esi
          push 48
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D94E8
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123E0C4
          mov eax, esi
          pop esi
          ret
004D94E8  xor eax, eax
          pop esi
          ret

004D6320  push esi
          mov esi, ecx
          call 0044C0C0
          mov [esi], 0x123E0C4
          mov eax, esi
          pop esi
          ret

004D6332  push 48
          pop eax
          ret
```

No extra stores after the vtbl write.
Persist `004DF32C`:

| Off | Helper |
| --- | --- |
| `+37` | `0043314A` (u8) |
| `+38` | `0043314A` (u8) |
| `+40` | `00431061` (f32) |
| `+44` | `00431061` (f32) |

Lionhead field names **UNREAD**.
`raw` **29**. Size **48**.

Later string xrefs (not this walk):
`00784D14` `fn=00784D10`; `00785018`
`fn=00785010`. Intern only. **Not**
Add Def Class.

---

## 4. `game.bin` instance names

Dump columns: `index type instance source
mesh raw subdefs strings`.
`INDEX.md` type counts **2** /
**5**. `GameBin.HasSubDefTable` is
OBJECT / CREATURE / BUILDING / THING /
MARKER / … — **not** these two types.
`writeParts` is false for `game.bin`;
parent `SubDefs` rows that point at
these ids are **UNREAD**.

### Pair 44 `CBoastingPodiumDef` — 2 rows

`names.tsv`:

| Offset | CRC | Name |
| --- | --- | --- |
| `0x00006D6E` | `0x2DE9F7F3` | `NULLDEF_CBoastingPodiumDef` |
| `0x00006D8D` | `0xE513A352` | `CBoastingPodiumDef` |

| Id | Type | Instance | Source | Raw | Subdefs | ASCII |
| ---: | --- | --- | --- | ---: | ---: | --- |
| 43 | `CBoastingPodiumDef` | `NULLDEF_CBoastingPodiumDef` | `NULLDEF_CBoastingPodiumDef` | 11 | 0 | *(empty)* |
| 12657 | `CBoastingPodiumDef` | `CBoastingPodiumDef` | *(empty)* | 11 | 0 | *(empty)* |

NULLDEF index **43** sits after
`NULLDEF_CFlammableDef` **42** and
before `NULLDEF_CTCVolumeContainmentTrackerDef`
**44**. **MATCH** remaining-pairs
order (43 / 44 / 45).

Id **12657** sits after
`SI_FRESCO_DOME_PILLAR` (`12655`),
not the hero / Maze clusters.

Named **OBJECT** rows (different
type; not this Def class):

| Id | Type | Instance | Mesh | Subdefs |
| ---: | --- | --- | ---: | ---: |
| 4026 | `OBJECT` | `OBJECT_GUILD_PEDESTAL_BOASTING_01` | 6080 | 5 |
| 4027 | `OBJECT` | `OBJECT_GUILD_BOASTING_PODIUM_01` | 6096 | 6 |
| 4105 | `OBJECT` | `OBJECT_HERO_BOASTING_PODIUM_01` | 6003 | 6 |
| 4106 | `OBJECT` | `OBJECT_HERO_BOASTING_PODIUM_DRUM_01` | 6005 | 5 |

Whether any of those six-subdef
tables point at **43** or **12657**
is **UNREAD**.

### Pair 45 `CTCVolumeContainmentTrackerDef` — 5 rows

`names.tsv`:

| Offset | CRC | Name |
| --- | --- | --- |
| `0x00006DA4` | `0x33464289` | `NULLDEF_CTCVolumeContainmentTrackerDef` |
| `0x00006DCF` | `0x1C03A958` | `CTCVolumeContainmentTrackerDef` |

| Id | Type | Instance | Source | Raw | Subdefs | ASCII |
| ---: | --- | --- | --- | ---: | ---: | --- |
| 44 | `CTCVolumeContainmentTrackerDef` | `NULLDEF_CTCVolumeContainmentTrackerDef` | `NULLDEF_CTCVolumeContainmentTrackerDef` | 29 | 0 | *(empty)* |
| 8901 | `CTCVolumeContainmentTrackerDef` | `CTCVolumeContainmentTrackerDef` | *(empty)* | 29 | 0 | *(empty)* |
| 9194 | `CTCVolumeContainmentTrackerDef` | `CTCVolumeContainmentTrackerDef` | *(empty)* | 29 | 0 | *(empty)* |
| 10083 | `CTCVolumeContainmentTrackerDef` | `CTCVolumeContainmentTrackerDef` | *(empty)* | 29 | 0 | *(empty)* |
| 10524 | `CTCVolumeContainmentTrackerDef` | `CTCVolumeContainmentTrackerDef` | *(empty)* | 29 | 0 | *(empty)* |

Live instance names are the type
string (unnamed nested). No distinct
`NULLDEF_`-style live name.

Clustering only (same method as
`004F5721-boss-fish-guard`
`CFishingDef` **10514**):

| Id | Neighbour cluster | Oakvale / Maze? |
| ---: | --- | --- |
| 8901 | shops / `CWeaponDef` `SWORD` | **UNREAD** parent |
| 9194 | tavern tables / `CQuestCardDef` | **UNREAD** parent |
| 10083 | `SI_COOP_SPIRIT` / `CCoopSpiritDef` | **DISPROVEN** Oakvale / Maze |
| 10524 | CREATURE_HERO (`CHeroDef` 10508, `CHeroExperienceDef` 10513, `CFishingDef` 10514, `CStealthDef` 10520, `CWeaponDef` 10526 `SWORD`) | hero capability, **not** Oakvale pond / Maze |

Parent `SubDefs` dwords: **UNREAD**.

---

## 5. Oakvale childhood / Maze

This registrar pair does **not**
spawn Things and does **not** open a
TNG.

| Claim | Answer | Class |
| --- | --- | --- |
| This walk is Oakvale / `00DBDE40`? | **No.** Parent is `004EE23F`. | **DISPROVEN** |
| Constructs `Q_NewOakValeIntro` / `S_QNOVI`? | **No.** | **DISPROVEN** |
| First no-save Present region | LookoutPoint (`NewRegion 1`), not `StartOakValeWest` | **PROVEN** (`2026-08-18-first-scene-things`) |
| Spawned hero | `CREATURE_HERO` mesh **4299** | **PROVEN** |
| `CREATURE_HERO_CHILD` / kid 4300 as this Present | **No.** | **DISPROVEN** |
| Kid `SI_HERO_CHILD` cluster (`10537`…`10543`) includes these types? | **No.** `CAppearanceDef` / `CPhysicsDef` / `CCreatureDef` / `CHeroDef` / `CEntitySoundDef` / `CSkeletalMorphDef` / `CHeroMorphDef`. | **DISPROVEN** in that cluster; 33-row `SubDefs` list still **UNREAD** |
| Maze NPC cluster (`SI_HERO_MAZE` `10621`, `CMazeBattleDef` `10614` / `10628` / `10632`) includes these types? | **No.** | **DISPROVEN** |
| Lookout `MK_GTA_MAZE*` | `MARKER_BASIC` Guild-training waypoints. Not these Def classes. | **DISPROVEN** as this pair |
| GuildExterior first-seen neighbour | `OBJECT_GUILD_BOASTING_PODIUM_01` mesh **6096** + `MARKER_BOAST_AREA`. Guild, **not** Oakvale childhood. | **PROVEN** TNG object; **DISPROVEN** as Oakvale childhood |
| Lookout primary TNG has a boasting podium? | First-scene dump: **no** `OBJECT_*BOASTING*` on LookoutPoint itself. | **DISPROVEN** on primary map |

Guild boast UI (`00813460`) and
`HeroBoasts` quest factory
`00CE6C40` are later Guild
gameplay. Not this intern.

---

## 6. Thirteen unnamed CTC then next pair

`004D2EF0` after `004F3899` and
before `004F3E24`. Remaining-pairs
row 46 CTC column = 13 unnamed.
**MATCH** (`004F3E4C-drain-fireball`
table). Helpers out of range; first
is `004D6353` `"CTCDeadShotInThingTracker"`.

```
004F3E24  push "CThingDrainLifeShotDef"
004F3E34  push 0x4D8D56
004F3E46  call 0042DAE0
004F3E4C  call 0044C6B0
004F3E53  call 009B0AC0
```

Next pair is **`CThingDrainLifeShotDef`**
`004F3E4C` / `004F3E53` factory
`004D8D56` `00BFEA1A(60)` in-line
`0044C0C0`; vtbl **`0123CCA4`**.
Already recovered in
`proofs/004F3E4C-drain-fireball`.

---

## Original

Forty-fourth / forty-fifth Add Def
Class on `004EE23F`:

1. `0099EBF0` name
   `"CBoastingPodiumDef"`.
2. `0042DAE0` packs factory
   `004D8736`.
3. `0044C6B0` `004F3630`.
4. `009B0AC0` `004F3637`.
5. Five unnamed CTC.
6. `0099EBF0` name
   `"CTCVolumeContainmentTrackerDef"`.
7. `0042DAE0` packs factory
   `004D94C8`.
8. `0044C6B0` `004F3892`.
9. `009B0AC0` `004F3899`.

Factory 44: alloc 44, in-line
`0044C0C0`, vtbl `0123BF0C`.
Factory 45: alloc 48, in-line
`0044C0C0`, vtbl `0123E0C4`.
No extra dword inits.

Not Oakvale. Not Maze. Not a Thing
instance. Not a file I/O site.

---

## Host

`EngineLifecycle.AddFirstDefClass`
already Notes FortyFourth
`CBoastingPodiumDef` (`004F3630` /
`004D8736` / `0044C0C0` / size 44 /
vtbl `0123BF0C`) and FortyFifth
`CTCVolumeContainmentTrackerDef`
(`004F3892` / `004D94C8` /
`0044C0C0` / size 48 / vtbl
`0123E0C4`). Tests
`Init_Thing_Components_004F3637_adds_CBoastingPodiumDef`
and
`Init_Thing_Components_004F3899_adds_CTCVolumeContainmentTrackerDef`
assert the constants and Note order
before `"Init Definition Manager"`,
and assert no
`RegionTravel.StartOakValeSetup`.

Note-only + flag. **Not** a live
44- or 48-byte object. Factory `E8`
is **not** on this walk.

Host Notes **MATCH** the listing
sites. Live ctor is **LEFTOVER**.
This investigation does **not** edit
`src/`.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `004F3608` / `004F3630` / `004F3637` | pair 44 `CBoastingPodiumDef` | **PROVEN** leftover |
| `004D8736` | factory `00BFEA1A(44)` `0044C0C0` vtbl `0123BF0C` | **PROVEN** |
| `004D46F5` | size 44 | **PROVEN** |
| `004DE772` | persist one float `+40` | **PROVEN** helper; field name **UNREAD** |
| `004F386A` / `004F3892` / `004F3899` | pair 45 `CTCVolumeContainmentTrackerDef` | **PROVEN** leftover |
| `004D94C8` | factory `00BFEA1A(48)` `0044C0C0` vtbl `0123E0C4` | **PROVEN** |
| `004D6332` | size 48 | **PROVEN** |
| `004DF32C` | persist `+37` `+38` `+40` `+44` | **PROVEN** helper; field names **UNREAD** |
| 1 + 5 unnamed `004D2EF0` | CTC counts | **PROVEN** count; names **UNREAD** in-range |
| `004F3E4C` / `004F3E53` | next pair `CThingDrainLifeShotDef` | **PROVEN** (`004F3E4C-drain-fireball`) |
| `00DBDE40` | Oakvale childhood | **DISPROVEN** here |
| `OBJECT_GUILD_BOASTING_PODIUM_01` | GuildExterior first-seen prop | **PROVEN** TNG; **DISPROVEN** Oakvale childhood |
| `AddFirstDefClass` FortyFourth / FortyFifth | Note-only | **MATCH** Notes; live **LEFTOVER** |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00780000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00800000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\assembly\exe\00-index\xrefs.tsv`
- `C:\FableCSharp\assembly\compiled-defs\game\entries.tsv`
- `C:\FableCSharp\assembly\compiled-defs\game\INDEX.md`
- `C:\FableCSharp\assembly\compiled-defs\names.tsv`
- `C:\FableCSharp\proofs\004EE23F-remaining-pairs\README.md`
- `C:\FableCSharp\proofs\004F34C4-quest-card\README.md`
- `C:\FableCSharp\proofs\004F3E4C-drain-fireball\README.md`
- `C:\FableCSharp\docs\status\investigations\2026-08-18-first-scene-things.dump.txt`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs` (`AddFirstDefClass` FortyFourth / FortyFifth) read only
