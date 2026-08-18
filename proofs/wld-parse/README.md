# WLD parse: `WorldFile.cs` vs native after Leave Frontend

Investigation only. No production `src` edits.
Do **not** start at Oakvale / `StartOakVale` / `00DBDE40` unless a
later persist/name write is proven. No-save first authored region
is **LookoutPoint** (`NewRegion 1`).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **LEFTOVER** / **MISMATCH**.

Sources: `src/Fable.Formats/Wld/WorldFile.cs`;
`tools/Fable.ExeIndex/out/01-sections/world/`;
`tools/Fable.ExeIndex/out/01-sections/landscape-trace/wld-*.md`;
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`,
`listing-00480000.txt`, `listing-004c0000.txt`, `listing-00500000.txt`;
`00-index/strings.tsv` / `xrefs.tsv` / `e8.tsv`;
`docs/runtime/FORWARD_TREE.md` §10;
`docs/status/README.md`;
TLC `data\Levels\FinalAlbion.wld`.

---

## Path after Leave Frontend

```
0042F2A2  Leave frontend                    PROVEN  listing-00400000
  [0x1375448]=0
  [0x13B8616]==0 → skip bank swaps
  00404490 / 004131A0
  0042F44D  push "FinalAlbion.wld"          PROVEN  only ASCII xref 0x01230D2C
  0042EBB6  teardown (+41 skips audio stop)
0042F491  Init Game
  00418DCA  GameMode ctor 0x161E8
    00418EB0  lea ecx, [esi+90576]
    00418EBB  call 00415E17                 PROVEN  4-string copy, not a literal
004184BD  GameStart vtbl+4                  PROVEN
  [0x13B86A0]=game
  Init Thing Components / Definition Manager / Graphics / …
  00418758  "Init World"
  00418784  call 0041735A                   PROVEN  InitWorld
    004173E1  "Init World Init"
    alloc 0x198 → 004A67D0 CWorld ctor
      004A68AE  [world+172/+176/+180]=0     empty dword triple
      004A6E3C  "Init World Map"
      004A6EC0  call 005066E0               PROVEN  CWorldMap
        dummy NewRegion slot 88 B at +44
        [map+156]=0  current index
        [map+172]=1  MapUIDCount default
  Create Players 004166A8
  [0x13B8648]==0 → Load Particles 004174F1
  004188E9  call [eax+32]                   PROVEN  vtbl+32
00416953  GameLoadWorld                     PROVEN  "Loading world"
  [world].vtbl+28([game+40])
  [+90588] empty → skip 004A3200
  path:
    +90576 nonempty → FinalAlbion.wld       first-seen New Game
    else [0x13B8668]
    else 0x122EE14                          UTF-16 updatedscenic.wld
  00416ABA  call 004A1840
    Load Quests 004A0D90  FinalAlbion.qst / GlobalQuests.qst
    004FDAB0 empty 0x122D70C
    Startup WAD
    world vtbl+8 0049E220 → map vtbl+12
    00507C30  "Load .wld file"              PROVEN
    0049D970  [world+128]=1
    006C20A0 empty skip
    Set Static Map → 00B23DC0 → 00B428E0
  [0x13B8648]==0
    0049F180  Init Characters / GUI / Quests
      0049F247  lea edx, [esi+172]
      0049F24E  call 004B4260               vector of name ptrs
    +90584 empty vs 0x122D70E → skip 004B4A10
  004BBC00  ret 4
```

`00DBDE40` / `Q_NewOakValeIntro` / `StartOakVale` are **not** on
this walk. **PROVEN** (`LoadWorld_00416953_no_save_is_004A1840_then_0049F180`).

---

## Who writes `FinalAlbion.wld` vs `updatedscenic.wld`

Two different questions: **filename slot** vs **file bytes**.

### Filename (what `00416953` opens)

| String | VA | Writer on no-save New Game | Status |
|---|---|---|---|
| `FinalAlbion.wld` | `0x01230D2C` | Leave `0042F44D` → ctor copy `00415E17` into `game+90576` | **PROVEN** |
| `updatedscenic.wld` | `0x0122EE14` | nobody. Fallback only if `+90576` and `[0x13B8668]` are empty (`00416A86`) | **PROVEN** as fallback, **DISPROVEN** as first-seen New Game |

`strings.tsv` has **one** `FinalAlbion.wld` xref: `0042F44E`.
`updatedscenic` is **not** in the ASCII string table (UTF-16 PE
literal). Host `WorldFileName` must stay `FinalAlbion.wld`.

### File bytes (who emits `.wld` text)

Retail TLC already ships `data\Levels\FinalAlbion.wld`. New Game
**reads** it. It does not rewrite the file.

| Fn | Role | Tokens | Callers |
|---|---|---|---|
| `004FB990` | map writer | `MapUIDCount` `ThingManagerUIDCount` `NewMap` `MapX` `MapY` `LevelName` `LevelScriptName` `MapUID` `IsSea TRUE/FALSE;` `LoadedOnPlayerProximity TRUE/FALSE;` `EndMap;` | only `004FDA74` |
| `004FD040` | region writer | `NewRegion` `RegionName "` `NewDisplayName "` `RegionDef "` `AppearOnWorldMap;` `MiniMap*` `WorldMapOffset*` `NameGraphicOffset*` `ContainsMap "` `SeesMap "` `EndRegion;` | only `004FDA80` |
| `004FDA60` | save wrapper: maps then regions | no `START_INITIAL_QUESTS` emit | **no** `e8.tsv` caller |

Editor/save path. **Not** Leave / Init Game. **PROVEN** as writer
bodies; **DISPROVEN** as New Game I/O.

---

## First region (not Oakvale)

Shipped `FinalAlbion.wld`:

```
NewMap 1;     LevelScriptName "LookoutPoint";     MapX 3232 MapY 3488
NewRegion 1;  RegionName "LookoutPoint";
  ContainsMap BowerstoneBridge, LookoutPoint, GuildExterior
NewRegion 4;  RegionName "StartOakVale";          ← later, persist leftover
```

| Index | Native meaning | After Leave no-save | Status |
|---|---|---|---|
| 0 | `005066E0` dummy 88-byte slot; `[record+36]=0` | `WorldMap+156` ctor 0; `004FB150` | **PROVEN** |
| 1 | first appended `NewRegion` = LookoutPoint | first *authored* region | **PROVEN** as file + table |
| 4 | `StartOakVale` | persist `PlayerRegionName` / `00487C20`, not this walk | **LEFTOVER** vs no-save |

First `004189C2` pump does **not** `SetRegionAsLoaded`.
`00501450` E8 caller is **UNREAD**; if forced it loops **all** `i`
and last `+156=141` `Filler_NorthernWastes_02` — not Oakvale.
Host must not invent `ActivateCurrentRegion(StartOakVale)`.

C# leftovers that disagree with this walk:

- `WorldFile.FindRegionContaining` comment: “New-game Oakvale is
  `StartOakVale`” — **LEFTOVER** vs no-save first region.
- `RegionTravel.StartingRegion` / `NewGameRegion = StartOakValeWest`
  — **LEFTOVER** intro-view helper, not `00416953`.

---

## Native reader `00507C30` token switch

Dual match (listing-00500000): empty intern `0x122D70E` →
`rep cmpsb`; else `004115A0`. Loop `009BA4F0` → `00507EA0` until
EOF, then `Init thing maps` / `Load GTNG` / `Load global things` /
`Load region graph`.

| Token | Site (empty / `004115A0`) | Native store | `WorldFile.Parse` |
|---|---|---|---|
| `MapUIDCount` | `00507EBF` / `005081C6` | `CWorldMap+172` | `MapUidCount` **MATCH** |
| `ThingManagerUIDCount` | `00507ED8` / `005081EE` | parent `+104/+108` via `009BA5B0` | `ThingManagerUidCount` **MATCH** (width **PARTIAL**) |
| `LevelScriptName` | `00507EF1` / `00508222` | pending map script | `ScriptName` **MATCH** |
| `NewMap` | `00507F0A` / `0050833F` | `009BA540` → slot (`esp+36`) | `Index` **MATCH** |
| `EndMap` | `00507F23` / `00508395` | 72-byte record at `+32`; `004FCA50` | commit **MATCH** |
| `MapUID` | `00507F3C` / `0050851E` | `esp+164` → record `+32` | `MapUid` **MATCH** |
| `MapX` / `MapY` | `00507F55` / `00508543` | `esp+68` / `esp+60` | **MATCH** |
| `IsSea` | `00507F87` / `00508587` | `00BFEBA8("TRUE")` → `esp+43` | `ParseBool` **MATCH** |
| `LoadedOnPlayerProximity` | `00507FA0` / `00508612` | same → `esp+19` | **MATCH** |
| `LevelName` | `00507FB9` / `0050869D` | `0041A060` path join | `Unquote` **PARTIAL** (no join) |
| `NewRegion` | `00507FD2` / `0050881C` | `006BC410` + `0051D200` append `+44`; `009BA540` int **discarded** | `Index=ParseInt` **PARTIAL** (file is 1,2,3… so values agree) |
| `EndRegion` | `00507FEB` / `0050885F` | next token | **MATCH** |
| `RegionDef` | `00508004` / `00508873` | `006BC1D0` on last record | `RegionDef` **MATCH** |
| `EnvironmentDef` | `0050801D` / `00508986` | quoted def on last record | **MISMATCH** skipped |
| `DisplayName` | `00508036` / `00508A99` | **same** handler as `RegionName`: `[end-64]` = `+24` | **MISMATCH** skipped (file uses `NewDisplayName`) |
| `RegionName` | `0050804F` / `00508ADF` | `[end-64]` = `+24` | **MATCH** |
| `NewDisplayName` | `00508068` / `00508AEF` | `[end-60]` = `+28` | `DisplayName` **MATCH** |
| `ContainsMap` | `00508081` / `00508C02` | path + map-table lookup (`+32`, stride 72) | `MapStem` **PARTIAL** (stem vs full path) |
| `SeesMap` | `0050809A` | same shape as Contains | `MapStem` **PARTIAL** |
| `AppearOnWorldMap` | `005080B3` / `00508FD5` | `[end-4]` = `+84` = 1 | **MISMATCH** skipped |
| `MiniMapGraphic` | `005080CC` / `00508FF5` | `[end-48]` | **MISMATCH** skipped |
| `MiniMapScale` | `005080E5` / `00509039` | `009BA650` float `[end-44]` | **MISMATCH** skipped |
| `MiniMapOffsetX/Y` | `005080FE` / `00509061` | floats `[end-40]/[end-36]` | **MISMATCH** skipped |
| `MiniMapRegionExitTextOffsetX/Y` | `00508130` / `005090B1` | `Name[Region] float` → `006BC4D0/F0` | **MISMATCH** skipped |
| `WorldMapOffsetX/Y` | `00508162` / `005091D9` | floats `[end-20]/[end-16]` | **MISMATCH** skipped |
| `NameGraphicOffsetX/Y` | `00508194` / `00509229` | floats `[end-12]/[end-8]` | **MISMATCH** skipped |
| `START_INITIAL_QUESTS` | **not in switch** | unknown token → `0050933B` | C# section **MISMATCH** (see quests) |
| `END_INITIAL_QUESTS` | **not in switch** | skipped | same |

Unknown tokens fall through to `005081C1` / `0050933B`. Tokenizer
`009BA4F0` / `009BA790` body **UNREAD** (`;` / quotes). C# trims,
drops `//`, strips a trailing `;` — matches the shipped file.

`00501D30` is a **second**, smaller reader (`NewMap` / `EndMap` /
`LoadedOnPlayerProximity` / `LevelName` only) used to collect level
paths. Not the New Game `vtbl+12` load.

---

## `START_INITIAL_QUESTS` is not the `00507C30` list

File head:

```
START_INITIAL_QUESTS;
Q_SunnyvaleMaster;
PersonalScriptMain;
PersonalScript_GlobalThings;
HeroBoasts;
V_HeroDolls;
CS_PlayCutscene;
END_INITIAL_QUESTS;
```

Those strings are **absent** from `strings.tsv` as WLD tokens.
`004FB990` does not emit them.

Runtime list `CWorld+172` (begin/end/cap):

1. `004A67D0` zeros `+172/+176/+180`.
2. `004A0D90` `AddQuest` with persistent `TRUE` (`00BFEBA8`)
   `push_back`s the name at `004A10C4` `lea esi, [ebp+172]`.
3. `AddTestQuest` `004A113B` stores `world+196` only — **not** `+172`.
4. `0049F24E` `004B4260([world+172])` activates that QST vector.

`CWorldMap+172` is **MapUIDCount** (`005081E3`). Conflating it with
`CWorld+172` is **DISPROVEN**.

C# `WorldFile.InitialQuests` from the WLD block happens to name the
same six quests as persistent `AddQuest` in `FinalAlbion.qst`.
Host `004B4260 [world+172] count=World.InitialQuests` is therefore
**PARTIAL**: names match authored data; attributing the fill to
`00507C30` is **DISPROVEN**.

`EngineLifecycle.LoadWldTokens` lists `START_INITIAL_QUESTS` /
`END_INITIAL_QUESTS` and omits every `MiniMap*` / offset token.
**MISMATCH** vs the `00507C30` switch.

---

## C# vs native (summary)

| Topic | Classification |
|---|---|
| New Game opens `FinalAlbion.wld` via Leave `0042F44D` | **PROVEN** |
| `updatedscenic.wld` first-seen New Game | **DISPROVEN** (fallback `0x122EE14` only) |
| Runtime writes `.wld` bytes on this path | **DISPROVEN** |
| First authored region LookoutPoint `NewRegion 1` | **PROVEN** |
| First region Oakvale / `StartOakVale` / `Maps[0]` myth | **DISPROVEN** (`Maps[0]` *is* Lookout; Oakvale is region 4 leftover) |
| Map/region tokens C# already stores | **PROVEN** match for shipped file |
| MiniMap / world-map / `AppearOnWorldMap` / `EnvironmentDef` | **MISMATCH** (native stores; C# drops) |
| `ContainsMap`/`SeesMap` as stems | **PARTIAL** (enough for `FindRegionContaining`) |
| `NewRegion N` as table index | **PARTIAL** (native appends; N discarded) |
| `START_INITIAL_QUESTS` in `00507C30` | **DISPROVEN** |
| `world+172` initial quests from QST `AddQuest` TRUE | **PROVEN** |
| `00507C30` tokenizer / padding at `0x122D70E` | **UNREAD** |
| `004FDA60` vtbl / editor menu | **UNREAD** |

Do not wire `FirstSceneWorld` / `RegionTravel.NewGameRegion` to
Oakvale from this parse. Persist `PlayerRegionName` writer stays
**UNREAD**.
