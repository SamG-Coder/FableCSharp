# `00507C30` world-map `vtbl+12`: complete token switch

Investigation only. No production `src/` edits.

Do **not** treat WLD `START_INITIAL_QUESTS` as a `00507C30` case.
Do **not** start at Oakvale / `00DBDE40`. First-seen New Game is
Leave `0042F44D` → `FinalAlbion.wld`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MISMATCH**.

Authority: `listing-00500000.txt` around `00507C30`;
`listing-00480000.txt` `0049E220`;
`listing-004c0000.txt` `004FDBC0` / `004FBF60` / `004FAFF0`;
`proofs/wld-parse/README.md`; TLC
`data\Levels\FinalAlbion.wld` head.
Siblings: `wld-first-region`, `tng-first-after-leave`,
`world-plus172-activate`.

---

## Verdict

| Claim | Class |
|---|---|
| `00507C30` is CWorldMap `vtbl+12` `"Load .wld file"` | **PROVEN** |
| Token switch has **31** compare cases | **PROVEN** |
| `START_INITIAL_QUESTS` / `END_INITIAL_QUESTS` have a case | **DISPROVEN** (absent from both arms; absent from `strings.tsv`) |
| First *matched* tokens on `FinalAlbion.wld` | `MapUIDCount` then `ThingManagerUIDCount` then **`NewMap`** |
| First **`NewRegion`** is after all 398 `EndMap` | **PROVEN** (file line 3992) |
| First TNG **open** is `004FDBC0` → `LookoutPoint.tng` | **PROVEN** (NewMap 1 script, not ContainsMap[0]) |
| That open is `0051FD80` / Bridge `TRACK_NODE_BASIC` | **DISPROVEN** (parse only; later region apply) |

---

## Path (first-seen no-save)

```
0042F2A2  Leave frontend
  0042F44D  "FinalAlbion.wld" → game+90576          PROVEN
00416953  Loading world
  004A1840
    QST / Startup WAD
    world vtbl+8 0049E220
      [world+20] map
      call [edx+12]                                 PROVEN  vtbl+12
        00507C30  "Load .wld file"
          token loop 009BA4F0 → 00507EA0
          00509553  Init thing maps
          0050959F  Load GTNG  stem+.gtng  TLC miss
          00509859  Load global things
            [0x13B8609]==0 → 004FDBC0               ← first TNG open
          00509982  Load region graph
    Set Static Map                                   AFTER 004FDBC0
```

`005066E0` ctor is **not** this reader. **PROVEN** (`wld-parse`).

---

## Dual-arm switch (listing-00500000)

Prologue: `sub esp, 0x1BC`. Optional `[map+188]` 36-byte
`006C26B0`. File read into a buffer. Defaults:

```
[map+172] = 1                 // MapUIDCount
[esp+19]  = 1                 // LoadedOnPlayerProximity
[esp+43]  = 0                 // IsSea
parent+104/+108 = 1 / 0       // ThingManagerUIDCount
```

Loop: `009BA4F0` until EOF (`00507E91` / `00509384` →
`00507EA0`). Current token via `009BA6F0`.

- empty intern (`[esp+20]==0`): `rep cmpsb` vs `0x122D70E`
  starting `00507EBF`
- else: `004115A0` chain starting `005081C6`

Both arms use the **same 31 names**, same fall-through
`0050933B` (unknown / `EndRegion` consume + next `009BA4F0`).

| # | Token | Empty intern | `004115A0` | Handler |
|--:|---|---|---|---|
| 1 | `MapUIDCount` | `00507EBF` | `005081C6` | `[map+172] = 009BA540` |
| 2 | `ThingManagerUIDCount` | `00507ED8` | `005081EE` | parent `+104/+108` via `009BA5B0` |
| 3 | `LevelScriptName` | `00507EF1` | `00508222` | pending script (`009BA790`) |
| 4 | `NewMap` | `00507F0A` | `0050833F` | `"Loading maps"`; `009BA540` → `[esp+36]` |
| 5 | `EndMap` | `00507F23` | `00508395` | 72-byte slot; `004FCA50` |
| 6 | `MapUID` | `00507F3C` | `0050851E` | `[esp+164]` |
| 7 | `MapX` | `00507F55` | `00508543` | `[esp+68]` |
| 8 | `MapY` | `00507F6E` | `00508565` | `[esp+60]` |
| 9 | `IsSea` | `00507F87` | `00508587` | `00BFEBA8("TRUE")` → `[esp+43]` |
| 10 | `LoadedOnPlayerProximity` | `00507FA0` | `00508612` | same → `[esp+19]` |
| 11 | `LevelName` | `00507FB9` | `0050869D` | `0041A060` path join |
| 12 | `NewRegion` | `00507FD2` | `0050881C` | `006BC410` + `0051D200` append `+44`; `009BA540` **discarded** |
| 13 | `EndRegion` | `00507FEB` | `0050885F` | next token (`0050933B`) |
| 14 | `RegionDef` | `00508004` | `00508873` | `006BC1D0` last record |
| 15 | `EnvironmentDef` | `0050801D` | `00508986` | quoted def last record |
| 16 | `DisplayName` | `00508036` | `00508A99` | **same** as `RegionName`: `[end-64]` |
| 17 | `RegionName` | `0050804F` | `00508ADF` | `[end-64]` = `+24` |
| 18 | `NewDisplayName` | `00508068` | `00508AEF` | `[end-60]` = `+28` |
| 19 | `ContainsMap` | `00508081` | `00508C02` | path + map-table lookup stride 72 |
| 20 | `SeesMap` | `0050809A` | `00508DE3` | same shape; `00485FF3` |
| 21 | `AppearOnWorldMap` | `005080B3` | `00508FD5` | `[end-4]` = 1 |
| 22 | `MiniMapGraphic` | `005080CC` | `00508FF5` | `[end-48]` |
| 23 | `MiniMapScale` | `005080E5` | `00509039` | `009BA650` `[end-44]` |
| 24 | `MiniMapOffsetX` | `005080FE` | `00509061` | `[end-40]` |
| 25 | `MiniMapOffsetY` | `00508117` | `00509089` | `[end-36]` |
| 26 | `MiniMapRegionExitTextOffsetX` | `00508130` | `005090B1` | `006BC4D0` |
| 27 | `MiniMapRegionExitTextOffsetY` | `00508149` | `00509145` | `006BC4F0` |
| 28 | `WorldMapOffsetX` | `00508162` | `005091D9` | `[end-20]` |
| 29 | `WorldMapOffsetY` | `0050817B` | `00509201` | `[end-16]` |
| 30 | `NameGraphicOffsetX` | `00508194` | `00509229` | `[end-12]` |
| 31 | `NameGraphicOffsetY` | `005081AD` | `00509251` | `[end-8]` |
| — | **default** | `005081C1` `jmp 0050933B` | last `je 0050933B` | skip token |

`EndMap` writes `[record+36]=1`, `[record+40]=proximity`,
`[record+64]=IsSea`, script at `+24`. Dummy slot 0 is the
`005066E0` 88-byte region row, not this 72-byte map table.

Tokenizer `009BA4F0` / `009BA790` body **UNREAD**.

---

## `START_INITIAL_QUESTS` has **no** case

Empty-intern compares (`00507EBF`–`005081BB`) and the
`004115A0` chain (`005081C6`–`0050925F`) never push or
`cmpsb` `START_INITIAL_QUESTS` or `END_INITIAL_QUESTS`.
`strings.tsv` has **zero** hits for either name.

Unknown tokens fall through to `0050933B`. File head
therefore does **not** fill `CWorld+172`. That vector is
QST `AddQuest` TRUE (`004A10C4`). **PROVEN**
(`wld-parse`, `world-plus172-activate`).

`FORWARD_TREE` §10 listing those names under `00507C30`
is **LEFTOVER**. Host `EngineLifecycle.LoadWldTokens`
includes the two quest sentinels and **omits** every
`MiniMap*` / offset token — **MISMATCH** vs this switch.

`00501D30` is a **second**, smaller reader (`NewMap` /
`EndMap` / `LoadedOnPlayerProximity` / `LevelName` only).
Not first-seen `vtbl+12`.

---

## Which tokens fire on `FinalAlbion.wld` first-seen

File head (TLC):

```
START_INITIAL_QUESTS;            // default 0050933B
Q_SunnyvaleMaster;               // default
PersonalScriptMain;              // default
PersonalScript_GlobalThings;     // default
HeroBoasts;                      // default
V_HeroDolls;                     // default
CS_PlayCutscene;                 // default
END_INITIAL_QUESTS;              // default

MapUIDCount 72;                  // FIRST match
ThingManagerUIDCount 1;
NewMap 1;                        // FIRST NewMap
MapX 3232; MapY 3488;
LevelName "FinalAlbion\LookoutPoint.lev";
LevelScriptName "LookoutPoint";
MapUID 162441;
IsSea FALSE;
LoadedOnPlayerProximity TRUE;
EndMap;
```

First `NewRegion` (line 3992, after `NewMap 398`):

```
NewRegion 1;
RegionName "LookoutPoint";
NewDisplayName "TXT_REGION_LOOKOUT_POINT";
RegionDef "REGION_LOOKOUT_POINT";
AppearOnWorldMap;
MiniMapGraphic MINIMAP_LOOKOUTPOINT;
MiniMapScale / MiniMapOffsetX/Y
WorldMapOffsetX/Y  NameGraphicOffsetX/Y
MiniMapRegionExitTextOffsetX/Y[…]
ContainsMap  BowerstoneBridge, LookoutPoint, GuildExterior
SeesMap      …
EndRegion;
```

| Token | Fires on this file? | First hit |
|---|---|---|
| `START_INITIAL_QUESTS` / `END_INITIAL_QUESTS` | no case | head, default |
| six quest names | no case | head, default |
| `MapUIDCount` | yes | first match |
| `ThingManagerUIDCount` | yes | second match |
| `NewMap` … `EndMap` inner set | yes | `NewMap 1` Lookout |
| `NewRegion` … `EndRegion` set | yes | `NewRegion 1` Lookout |
| `EnvironmentDef` | case, **file has none** | never |
| `DisplayName` (bare) | case, **file uses `NewDisplayName`** | never |

Census: **398** `NewMap`, **141** `NewRegion`,
**151** `LoadedOnPlayerProximity TRUE` (247 FALSE).
`MapUIDCount 72` is stored as written; it is **not**
the map-table length.

---

## `NewMap` / `NewRegion` / first TNG `004FDBC0`

All three are **inside** `00507C30`. Order:

1. **`NewMap 1`** — first map token. Script `LookoutPoint`.
   Slot index from `009BA540` (file `1`).
2. **`NewRegion 1`** — after every `EndMap`. Appends region
   table `+44`. File `1` is discarded. `ContainsMap[0]` is
   `BowerstoneBridge.lev`.
3. **EOF** then `"Load global things"` `00509859`.
   `[0x13B8609]==0` → `call 004FDBC0` (`00509946`).

`004FDBC0` (listing-004c0000):

```
ebx = 1                         // skip unused slot 0
edi = 0x48                      // stride 72
while ebx < map_count:
  if [slot+36] && [slot+40]:    // EndMap wrote 1 and proximity
    004FBF60(ebx)
      record+24 script → 004FAFF0(0x12442C4 ".tng")
      005223F0                  // parse; 0051FD80 UNREAD live
  ebx++
```

First open is therefore **`LookoutPoint.tng`** (NewMap 1,
native map index 1, proximity TRUE). **Not**
`BowerstoneBridge.tng` (that is later `00501450` /
`006C2170` ContainsMap order, first `0051FD80`).

`.gtg` `004FE2A0` is the `[0x13B8609]!=0` arm. Default 0
skips it. TLC `FinalAlbion.gtng` miss at `0050959F` is
**PROVEN** skip.

---

## Host notes (no edit)

- `WorldFile.Parse` still consumes `START_INITIAL_QUESTS`
  into `InitialQuests`. Native `00507C30` does not.
  Names happen to overlap QST TRUE; attributing the fill
  to this switch is **DISPROVEN**.
- `LoadWldTokens` must not be treated as the native
  vocabulary until MiniMap/offset tokens are listed and
  the quest sentinels are dropped.
- `FindRegionContaining` “New-game Oakvale is
  `StartOakVale`” remains **LEFTOVER** vs this walk.

---

## Open

| Item | Class |
|---|---|
| `009BA4F0` / `009BA790` tokenize `;` / quotes / `[Name]` | **UNREAD** |
| Live `[manager+128]` after first `004FBF60` (`005223F0`) | **UNREAD** |
| `00501D30` caller vs first-seen | not this `vtbl+12` |
