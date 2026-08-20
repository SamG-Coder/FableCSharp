# `004EE23F` remaining pairs 107–109: betting / oracle / fireheart

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` (`0042F44D`) →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD` → `00418585` `004EE23F`.
Do **not** invent class names: only
`push "…"` listing strings.
Do **not** spawn Guild minigames on
no-save LookoutPoint.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: recover remaining-pairs rows
**107–109** (`CBettingDef` /
`COracleMinigameDef` /
`CFireheartMinigameDef`). For each factory:
persist ctor, size, vtbl. Childhood?
Guild minigames on no-save?

| n | `0044C6B0` | `009B0AC0` | Factory | Size | Ctor | Vtbl | Class |
| --- | --- | --- | --- | ---: | --- | --- | --- |
| 107 | `004F899A` | `004F89A1` | `004D96DD` | **88** | `0044C0C0` in-line | **`0123E3B4`** | **PROVEN** |
| 108 | `004F8B91` | `004F8B98` | `004D97E9` | **92** | `0044C0C0` in-line | **`0123E504`** | **PROVEN** |
| 109 | `004F8C47` | `004F8C4E` | `004D982F` | **60** | `jmp 004D6638` | **`0123E584`** | **PROVEN** |

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
(`004F85CF`…`004F8C4E`; factories
`004D96DD` / `004D97E9` / `004D982F`;
persist ctor `004D6638`; size helpers
`004D64F8` / `004D6601` / `004D665A`;
LoadDef `004DF5C7` / `004DF658` /
`004DF6F4`; clone `004E1913` /
`004E1975` / `004E19DD`);
`listing-00400000.txt` `00431020` /
`00431061` / `00431102` / `00431143`;
`listing-00440000.txt` `0044C0C0`;
`proofs/004EE23F-remaining-pairs` rows
106–110; `proofs/004F8E89-hasname-tail`;
`proofs/004F5721-boss-fish-guard`.
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01243AF4` **`CBettingDef`**,
`0x01243AE0` **`COracleMinigameDef`**,
`0x01243AC8` **`CFireheartMinigameDef`**.
`assembly/exe/00-index/vtbl.tsv` the three
vtbls. `rtti.txt` `.?AVC…Def@@`.
`assembly/compiled-defs/game/entries.tsv`
NULLDEF 106–108 / live 10512 / 10515 /
10516.

All three are shape-2 (`push` + `0042DAE0`).
Listing strings are **not** invented.

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| Row 107 name / sites / factory? | **`CBettingDef`** `004F8972` / `004F899A` / `004F89A1` / `0x4D96DD`. 8 CTC before. | **PROVEN** |
| Row 107 size / ctor / vtbl? | **88**; `0044C0C0` then `[esi]=0123E3B4`; vtbl[20] `004D64F8` `push 88`. Persist `004DF5C7` twelve dwords `+40…+84`. | **PROVEN** size/vtbl; Lionhead names **UNREAD** |
| Row 108 name / sites / factory? | **`COracleMinigameDef`** `004F8B69` / `004F8B91` / `004F8B98` / `0x4D97E9`. 4 CTC before. | **PROVEN** |
| Row 108 size / ctor / vtbl? | **92**; `0044C0C0` then `[esi]=0123E504`; vtbl[20] `004D6601` `push 92`. Persist `004DF658` thirteen dwords `+40…+88`. | **PROVEN** size/vtbl; Lionhead names **UNREAD** |
| Row 109 name / sites / factory? | **`CFireheartMinigameDef`** `004F8C1F` / `004F8C47` / `004F8C4E` / `0x4D982F`. 1 CTC before. | **PROVEN** |
| Row 109 size / ctor / vtbl? | **60**; `jmp 004D6638`; vtbl **`0123E584`**; vtbl[20] `004D665A` `push 60`. Ctor two `0099E4B0` at `+40` / `+44`. | **PROVEN** |
| Childhood / Oakvale? | **No.** Init Thing Components class register. Not `00DBDE40`. Not `StartOakValeWest` / `HerosOldHouse`. | **DISPROVEN** |
| Guild minigames on no-save? | **No.** Live rows sit on CREATURE_HERO with `CHeroDef` / `CFishingDef`. Not Guild Woods Things. First Present is LookoutPoint. | **DISPROVEN** |
| Next pair? | **110** `CLightningOrbDef` `004F8D40` / `004F8D68` / `004F8D6F` factory `0x4D9882`. 2 CTC between. | **PROVEN** sites |
| Host live objects? | **None.** `AddFirstDefClass` returns after 21st (`CBedDef`). Rows 107–109 are **LEFTOVER**. | **PROVEN** leftover |

**Answer:** three leftover Add Def Class
pairs. Factories allocate 88 / 92 / 60.
Two in-line `0044C0C0` + vtbl write;
`CFireheartMinigameDef` is the jmp-thunk
persist ctor. Not Oakvale. Not Guild
minigame Things on no-save. Not a file
I/O site.

---

## 1. Bound: pair 106 then eight CTC

`listing-004c0000.txt` after 106
`CCameraCollisionDef` `004F85FE`:

Eight unnamed `004D2EF0` rows. Helper
listing strings (same file, other fns;
`004EE23F` itself does **not** push
them):

| Helper at | `push` factory | Helper `push "…"` |
| --- | --- | --- |
| `004F861F` `004D626C` | `0x4D624F` | `"CTCFadeOutAndIn"` |
| `004F868A` `004D627F` | `0x4D94AB` | `"CTCFlashOnHit"` |
| `004F86F5` `004D630D` | `0x4D62ED` | `"CTCQuestCompletionUI"` |
| `004F8760` `004D64D3` | `0x4D64B6` | `"CTCCreditsUI"` |
| `004F87CB` `004D6366` | `0x4D950E` | `"CTCDestroyThingOnActionFinish"` |
| `004F8836` `004D63E8` | `0x4D9580` | `"CTCObstacle"` |
| `004F88A1` `004D64A3` | `0x4DB49D` | `"CTCFlourishTarget"` |
| `004F890C` `004D6519` | `0x4D64FC` | `"CTCBetting"` |

Remaining-pairs counted those eight
unnamed. Helper names are **not**
invented from `004Dxxxx`; they are
`push "…"` in the helper bodies.
`"CTCBetting"` is the CTC intern
**before** pair 107, not the Def name.

Then the 107th pair.

---

## 2. Pair 107 — `CBettingDef`

```
004F8972  push "CBettingDef"
004F8982  push 0x4D96DD
004F8994  call 0042DAE0
004F899A  call 0044C6B0
004F89A1  call 009B0AC0
```

`strings.tsv` `0x01243AF4`
**`CBettingDef`**. Listing `004F8972`
`68 F4 3A 24 01`. `xrefs.tsv` first hit
`0x004F8973`. RTTI `0x01379B40`.

```
004D96DD  push esi
          push 88
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D96FD
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123E3B4
          mov eax, esi
          pop esi
          ret
004D96FD  xor eax, eax
          pop esi
          ret
```

No extra stores after the vtbl write.
Placement ctor `004D64E6` is the same
`0044C0C0` + `0123E3B4` (factory does
not `jmp` it).

`vtbl.tsv` `0x0123E3B4` slot 20 is
`004D64F8`:

```
004D64F8  push 88
          pop eax
          ret
```

Slot 0 `004D9701` (`01230BA0` /
`009FC550`). Slot 18 persist
`004DF5C7`. Slot 19 clone `004E1913`.
Slots 1–17 / 21–24 are the shared
`0042D930`…`0042DAA0` /
`009ACE90` / `009ACAB0` family.

Persist LoadDef (`listing-004c0000.txt`
`004DF5C7`). Each field starts
`push 0x122D70E` (`00404500`
empty-intern sentinel; not a field
CRC) then type-2 / type-3 arms:

| Off | Helper | Kind |
| --- | --- | --- |
| `+40` `+44` `+48` `+52` | `00431061` | f32 (`fld`) |
| `+56` `+60` `+64` | `00431102` | u32 (`0040FE60`) |
| `+68` `+72` `+76` `+80` `+84` | `00431020` | u32 (`0040F8A0`) |

Twelve extra dwords; `40+48=88`.
**MATCH.** Clone `004E1913`:
`00431F10` then dword copies
`+40…+84`. Lionhead names **UNREAD**.
Ctor does not store those twelve
dwords.

`entries.tsv`: **2** rows. NULLDEF
index **106** raw **99**. Live
**10512** sits in the CREATURE_HERO
cluster (`CHeroDef` 10508,
`CHeroExperienceDef` 10513,
`CFishingDef` 10514,
`COracleMinigameDef` 10515,
`CFireheartMinigameDef` 10516). Hero
betting capability, **not** a Guild
table Thing.

Later string xrefs: `007E4CA4`
`fn=007E4CA0` (intern helper
`push "CBettingDef"`); `007E5828`
`fn=007E5820` (GetDef-by-name
`[eax+56]`). Not the persist ctor.

| Field | Value | Class |
| --- | --- | --- |
| `push` | `004F8972` `"CBettingDef"` | **PROVEN** |
| `0044C6B0` | `004F899A` | **PROVEN** |
| `009B0AC0` | `004F89A1` | **PROVEN** |
| Factory | `004D96DD` `00BFEA1A(88)` then `0044C0C0`; vtbl **`0123E3B4`** | **PROVEN** |
| Size | **88** (`push 88` at factory; vtbl[20] `004D64F8`) | **PROVEN** |

---

## 3. Four CTC then pair 108

Four unnamed `004D2EF0` between 107
and 108:

| Helper at | `push` factory | Helper `push "…"` |
| --- | --- | --- |
| `004F89C2` `004D6549` | `0x4D652C` | `"CTCLifeTimer"` |
| `004F8A2D` `004D655C` | `0x4D9723` | `"CTCFireRangedWeaponOnLeavingStateGroup"` |
| `004F8A98` `004D65A9` | `0x4DB4BA` | `"CTCAvoidRegionExit"` |
| `004F8B03` `004D65DC` | `0x4D65BC` | `"CTCOracleMinigame"` |

`"CTCOracleMinigame"` is the CTC
intern **before** pair 108, not the
Def name. Remaining-pairs counted
these four unnamed. **MATCH.**

---

## 4. Pair 108 — `COracleMinigameDef`

```
004F8B69  push "COracleMinigameDef"
004F8B79  push 0x4D97E9
004F8B8B  call 0042DAE0
004F8B91  call 0044C6B0
004F8B98  call 009B0AC0
```

`strings.tsv` `0x01243AE0`
**`COracleMinigameDef`**. Listing
`004F8B69` `68 E0 3A 24 01`.
`xrefs.tsv` first hit `0x004F8B6A`.
RTTI `0x01379B94`.

```
004D97E9  push esi
          push 92
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D9809
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123E504
          mov eax, esi
          pop esi
          ret
004D9809  xor eax, eax
          pop esi
          ret
```

No extra stores after the vtbl write.
Placement ctor `004D65EF` is the same
`0044C0C0` + `0123E504` (factory does
not `jmp` it).

`vtbl.tsv` `0x0123E504` slot 20 is
`004D6601`:

```
004D6601  push 92
          pop eax
          ret
```

Slot 0 `004D980D` (`01230BA0` /
`009FC550`). Slot 18 persist
`004DF658`. Slot 19 clone `004E1975`.

Persist LoadDef `004DF658`:

| Off | Helper | Kind |
| --- | --- | --- |
| `+40` `+44` `+48` | `00431061` | f32 |
| `+52` | `00431102` | u32 |
| `+56` `+60` `+64` | `00431020` | u32 |
| `+72` then `+68` | `00431020` | u32 (listing order: `+72` before `+68`) |
| `+76` `+80` `+84` `+88` | `00431020` | u32 |

Thirteen extra dwords; `40+52=92`.
**MATCH.** Clone `004E1975`:
`00431F10` then dword copies
`+40…+88` in address order. Lionhead
names **UNREAD**. Ctor does not store
those thirteen dwords.

`entries.tsv`: **2** rows. NULLDEF
index **107** raw **107**. Live
**10515** on CREATURE_HERO with
`CFishingDef` 10514 /
`CFireheartMinigameDef` 10516.
Hero Oracle-minigame capability,
**not** a Guild Oracle Thing.

Later string xrefs: `007E5CE4`
`fn=007E5CE0`; `007E6E08`
`fn=007E6E00`. Same intern /
GetDef-by-name shape as betting.
Not the persist ctor.

| Field | Value | Class |
| --- | --- | --- |
| `push` | `004F8B69` `"COracleMinigameDef"` | **PROVEN** |
| `0044C6B0` | `004F8B91` | **PROVEN** |
| `009B0AC0` | `004F8B98` | **PROVEN** |
| Factory | `004D97E9` `00BFEA1A(92)` then `0044C0C0`; vtbl **`0123E504`** | **PROVEN** |
| Size | **92** (`push 92` at factory; vtbl[20] `004D6601`) | **PROVEN** |

---

## 5. One CTC then pair 109

One unnamed `004D2EF0` between 108
and 109:

| Helper at | `push` factory | Helper `push "…"` |
| --- | --- | --- |
| `004F8BB9` `004D6625` | `0x4D6605` | `"CTCFireheartMinigame"` |

Factory `004D6605` is
`00BFEA1A(0x9C)` then `007E7640`
(CTC ctor, not `0044C0C0`).
Remaining-pairs counted this row
unnamed. **MATCH.**

---

## 6. Pair 109 — `CFireheartMinigameDef`

```
004F8C1F  push "CFireheartMinigameDef"
004F8C2F  push 0x4D982F
004F8C41  call 0042DAE0
004F8C47  call 0044C6B0
004F8C4E  call 009B0AC0
```

`strings.tsv` `0x01243AC8`
**`CFireheartMinigameDef`**. Listing
`004F8C1F` `68 C8 3A 24 01`.
`xrefs.tsv` first hit `0x004F8C20`.
RTTI `0x01379BB8`.

```
004D982F  push 60
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004D9842
          mov ecx, eax
          jmp 004D6638
004D9842  xor eax, eax
          ret

004D6638  push esi
          mov esi, ecx
          call 0044C0C0
          lea ecx, [esi+40]
          mov [esi], 0x123E584
          call 0099E4B0
          lea ecx, [esi+44]
          call 0099E4B0
          mov eax, esi
          pop esi
          ret

004D665A  push 60
          pop eax
          ret
```

Same thunk shape as nineteenth
`004E0B4B`: `00BFEA1A` with immediate
**60**, null → `xor eax, eax; ret`, else
`mov ecx, eax; jmp 004D6638`.

Persist ctor `004D6638` calls
`0044C0C0`, writes vtbl `0x0123E584`,
then two `0099E4B0` empty-CString
inits at `+40` / `+44`. No stores at
`+48` / `+52` / `+56`.

`vtbl.tsv` `0x0123E584` slot 20 is
`004D665A` (`push 60`). Slot 0
`004D9845` → dtor `004D9861`:
`0099EAE0` at `+44` then `+40`, then
`01230BA0` `jmp 009FC550`. Slot 18
persist `004DF6F4`. Slot 19 clone
`004E19DD`.

Persist LoadDef `004DF6F4`:

| Off | Helper | Kind |
| --- | --- | --- |
| `+40` `+44` | `00431143` | CString intern-from-stream |
| `+48` `+52` `+56` | `00431020` | u32 |

Two CString + three dwords;
`40+8+12=60`. **MATCH.** Clone
`004E19DD`: `00431F10` then
`0099EFB0` on `+40` / `+44`, dword
copies `+48…+56`.

`entries.tsv`: **2** rows. NULLDEF
index **108** raw **37**. Live
**10516** strings
`GATEWAY_IDLE_01|FLOURISH_WISP_SHORT_01`
(payload values for the two CString
slots, not Lionhead field names).
Hero Fireheart-minigame capability
on CREATURE_HERO, **not** a Guild
wisp Thing.

Later string xrefs: `007E7074`
`fn=007E7070`; `007E7D88`
`fn=007E7D80`. Same intern /
GetDef-by-name shape. Not the
persist ctor.

| Field | Value | Class |
| --- | --- | --- |
| `push` | `004F8C1F` `"CFireheartMinigameDef"` | **PROVEN** |
| `0044C6B0` | `004F8C47` | **PROVEN** |
| `009B0AC0` | `004F8C4E` | **PROVEN** |
| Factory | `004D982F` `00BFEA1A(60)` then `jmp 004D6638` | **PROVEN** |
| Ctor | `004D6638` `0044C0C0`; `[esi]=0123E584`; `0099E4B0` `+40` `+44` | **PROVEN** |
| Size | **60** (`push 60` at factory; vtbl[20] `004D665A`) | **PROVEN** |
| Vtbl | **`0123E584`** slot 0 `004D9845`; 18 persist `004DF6F4`; 19 clone `004E19DD` | **PROVEN** |

---

## 7. Next pair 110 (sites only)

After 109, two unnamed `004D2EF0`
then remaining-pairs row 110:

| Helper at | `push` factory | Helper `push "…"` |
| --- | --- | --- |
| `004F8C6F` `004D63A7` | `0x4D9547` | `"CTCDestroyThingOnSpecialAbilityFinish"` |
| `004F8CDA` `004D6687` | `0x4DCEA4` | `"CTCLightningOrb"` |

Listing after `004F8C4E`:

```
004F8C6F  call 004D63A7          ; "CTCDestroyThingOnSpecialAbilityFinish"
004F8C81  push 0x4D9547
004F8C8C  call 004D2EF0
…
004F8CDA  call 004D6687          ; "CTCLightningOrb"
```

Then `004F8D40` `push "CLightningOrbDef"`
factory `0x4D9882` sites `004F8D68` /
`004F8D6F`. Factory body already
visible next to 109 (`004D9882`
`00BFEA1A(60)`). LoadDef / vtbl for
110 **UNREAD** here.

Two CTC **MATCH** remaining-pairs
row 110.

---

## 8. Not childhood. Not Guild on no-save

No `00DBDE40` / region / TNG / hero
create on pairs 107–109. Parent is
`004EE23F`.

First-seen childhood is
`StartOakValeWest` / `CAM_OVIF_SHOT2` /
`HerosOldHouse` after Leave
(`docs/render/FIRST_SCENE_WORLD_PARITY.md`).
Registering these three here is the
global Init Game type walk, not
spawning an Oakvale or Guild Thing.
**DISPROVEN** as childhood first-seen.

No-save first Present is LookoutPoint
(`docs/status/investigations/2026-08-18-first-scene-things.md`),
not Guild Woods / Heroes' Guild. Live
`CBettingDef` / `COracleMinigameDef` /
`CFireheartMinigameDef` sit on
CREATURE_HERO as capability defs
(same cluster as `CFishingDef`).
**DISPROVEN** as Guild minigame Things
on no-save. Host must not invent
Guild Oracle / Fireheart / betting
table spawns on LookoutPoint.

---

## 9. Host leftover

`EngineLifecycle.AddFirstDefClass`
returns after twenty-first
`CBedDef` Note-only
(`004EE23F-twentyfirst-class`).
No `CBettingDef` /
`COracleMinigameDef` /
`CFireheartMinigameDef`. No
`0x4D96DD` / `0x4D97E9` /
`0x4D982F`.

Whole remaining `004EE23F` walk after
n=21 is still leftover
(`004EE23F-thing-components` /
`004EE23F-remaining-pairs` §6).

| If host adds… | Leftover is… |
| --- | --- |
| Note-only through n=21 (current) | n=22 `CStealthDef` … n=111 `CHasNameDef`, six tail CTC, `0073B130` / `004EBACE` |
| Note-only all 111 names including these three | still live `009AD6E0` / `009FC4F0` on each object (**not** MATCH) |
| live Add Def Class for all 111 | next omit is the six unnamed `004D2EF0`, then `0073B130` / `004EBACE` |

---

## Original

Pairs 107–109 Add Def Class on
`004EE23F`:

1. `0099EBF0` names `"CBettingDef"` /
   `"COracleMinigameDef"` /
   `"CFireheartMinigameDef"`.
2. `0042DAE0` packs factories
   `004D96DD` / `004D97E9` /
   `004D982F`.
3. `0044C6B0` `004F899A` /
   `004F8B91` / `004F8C47`.
4. `009B0AC0` `004F89A1` /
   `004F8B98` / `004F8C4E`.

Betting alloc 88, in-line `0044C0C0`,
vtbl `0123E3B4`. Oracle alloc 92,
in-line `0044C0C0`, vtbl `0123E504`.
Fireheart alloc 60, `jmp 004D6638`,
vtbl `0123E584`, two empty CStrings.

Not Oakvale. Not Guild minigames on
no-save. Not a Thing instance. Not a
file I/O site.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `004F8972` / `004F899A` / `004F89A1` | pair 107 `CBettingDef` | **PROVEN** leftover |
| `004D96DD` / `0123E3B4` / size 88 | factory / vtbl / size | **PROVEN** |
| `004D64E6` / `004D64F8` / `004DF5C7` / `004E1913` | placement / size-slot / persist / clone | **PROVEN** |
| `004F8B69` / `004F8B91` / `004F8B98` | pair 108 `COracleMinigameDef` | **PROVEN** leftover |
| `004D97E9` / `0123E504` / size 92 | factory / vtbl / size | **PROVEN** |
| `004D65EF` / `004D6601` / `004DF658` / `004E1975` | placement / size-slot / persist / clone | **PROVEN** |
| `004F8C1F` / `004F8C47` / `004F8C4E` | pair 109 `CFireheartMinigameDef` | **PROVEN** leftover |
| `004D982F` / `004D6638` / `0123E584` / size 60 | factory / persist ctor / vtbl / size | **PROVEN** |
| `004D665A` / `004DF6F4` / `004E19DD` | size-slot / persist / clone | **PROVEN** |
| eight CTC `004F863C`…`004F8929` | between 106 and 107 | **PROVEN** sites; names **UNREAD** in-range |
| four CTC `004F89DF`…`004F8B20` | between 107 and 108 | **PROVEN** sites; names **UNREAD** in-range |
| one CTC `004F8BD6` | between 108 and 109 | **PROVEN** sites; name **UNREAD** in-range |
| `004F8D40` / `004F8D68` / `004F8D6F` | next pair 110 `CLightningOrbDef` | **PROVEN** sites; factory body **UNREAD** here |
| `00DBDE40` / first-seen childhood | these three | **DISPROVEN** |
| Guild Oracle / Fireheart / betting Things on no-save Lookout | these three | **DISPROVEN** |
| `AddFirstDefClass` | Notes through n=21 `CBedDef` | remaining **LEFTOVER** |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\assembly\exe\00-index\rtti.txt`
- `C:\FableCSharp\assembly\compiled-defs\game\entries.tsv`
- `C:\FableCSharp\proofs\004EE23F-remaining-pairs\README.md`
- `C:\FableCSharp\proofs\004EE23F-thing-components\README.md`
- `C:\FableCSharp\proofs\004EE23F-twentyfirst-class\README.md`
- `C:\FableCSharp\proofs\004F8E89-hasname-tail\README.md`
- `C:\FableCSharp\proofs\004F5721-boss-fish-guard\README.md`
- `C:\FableCSharp\docs\render\FIRST_SCENE_WORLD_PARITY.md`
