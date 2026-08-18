# First region name from `FinalAlbion.wld` after Leave

Investigation only. No production `src` edits.
Do **not** take wiki / intro Oakvale (`StartOakVale` /
`StartOakValeWest` / `00DBDE40`) unless the shipped WLD or the
Leave walk writes that name first. It does not.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **LEFTOVER**.

Sources: TLC `data\Levels\FinalAlbion.wld`;
`src/Fable.Formats/Wld/WorldFile.cs`;
`EngineLifecycle.RequestNewGame` / `LoadWorld` / `LoadWorldMap` /
`LoadFromFirstRealRegion`;
`EngineLifecycleTests.Install_banks_and_startup_videos_exist`;
`proofs/wld-parse/README.md`; `proofs/region-travel-first/README.md`;
`docs/runtime/FORWARD_TREE.md` §10.

---

## Verdict

**`LookoutPoint`.**

After Leave, no-save New Game records `FinalAlbion.wld` then
`00507C30` parses that file. The first `NewRegion` / `RegionName`
token is `"LookoutPoint"`. Native table index **1** (dummy slot 0
is not a region). Oakvale is `NewRegion 4` `StartOakVale` — later
in the same file, persist / intro leftover, not first.

| Claim | Class |
|---|---|
| Leave writes `FinalAlbion.wld` (`0042F44D` → `game+90576`) | **PROVEN** |
| `00416953` opens that path, not `updatedscenic.wld` | **PROVEN** |
| First `RegionName` in the file is `LookoutPoint` | **PROVEN** (file bytes) |
| First authored table row is native index 1 = `LookoutPoint` | **PROVEN** |
| First `00500540(i,0,0)` after dummy is `i=1` Lookout | **PROVEN** |
| First region is Oakvale / `StartOakVale` / `StartOakValeWest` | **DISPROVEN** |
| `RegionTravel.NewGameRegion` / `StartingRegion` as this name | **LEFTOVER** |

---

## Path after Leave (filename, then parse)

```
0042F2A2  Leave frontend                         PROVEN
  0042F44D  "FinalAlbion.wld" → +90576           PROVEN
0042F491  Init Game → 00418DCA → 004184BD
  004188E9  [game].vtbl+32
00416953  Loading world                          PROVEN
  [+90588] empty → skip 004A3200
  path +90576 FinalAlbion.wld                    PROVEN
  004A1840
    00507C30  Load .wld file                     PROVEN
      NewMap 1  LevelScriptName "LookoutPoint"   file Maps[0]
      NewRegion 1  RegionName "LookoutPoint"     file Regions[0]
      NewRegion 4  RegionName "StartOakVale"     later
004189C2  first pump
  [WorldMap+156]=0  dummy 005066E0               PROVEN
  CurrentRegion = null; no SetRegionAsLoaded
later (E8 caller UNREAD)
  00501450  00500540(1,0,0) first real           PROVEN
    native index 1 = LookoutPoint
    ContainsMap TNG: BowerstoneBridge / LookoutPoint / GuildExterior
```

`00416953` is **not** a region load. It fills the map/region
tables from the WLD. The first *name* that table yields is the
first `NewRegion` block.

---

## File authority (TLC `FinalAlbion.wld`)

141 `RegionName` lines. First four authored regions:

```
NewMap 1;
  LevelName "FinalAlbion\LookoutPoint.lev";
  LevelScriptName "LookoutPoint";          // Maps[0]
  MapX 3232; MapY 3488; MapUID 162441;

NewRegion 1;
  RegionName "LookoutPoint";
  NewDisplayName "TXT_REGION_LOOKOUT_POINT";
  RegionDef "REGION_LOOKOUT_POINT";
  ContainsMap BowerstoneBridge.lev
  ContainsMap LookoutPoint.lev
  ContainsMap GuildExterior.lev

NewRegion 2;  RegionName "PicnicArea";
NewRegion 3;  RegionName "BowerstoneSlums";
NewRegion 4;  RegionName "StartOakVale";   // Oakvale — not first
  ContainsMap StartOakValeWest / MemorialGarden / StartOakValeEast
```

Last authored row is `NewRegion 141` `Filler_NorthernWastes_02`.
That is the **tail** of a full `00501450` loop (`+156=141`), not
the first name.

`WorldFile.Parse` stores those in order: `Regions[0].Index==1`,
`Regions[0].RegionName=="LookoutPoint"`, `Regions[3]==StartOakVale`.
`EngineLifecycle.RegionAtNativeIndex(1)` is that first row.

---

## What “first loaded” is not

| Candidate | Why not |
|---|---|
| Dummy index 0 | `005066E0` empty 88-byte slot. `[record+36]=0`. No `RegionName`. |
| `Maps[0]` as a region | It happens to be Lookout too, but the region token is `NewRegion`. |
| `StartOakVale` / West | File index **4**. Persist `00487C20` / `PlayerRegionName` or later `00DBDE40`. |
| `RegionTravel.StartingRegion` | Hardcodes `StartOakValeWest`. Unused on Leave / `00416953`. |
| `Filler_NorthernWastes_02` | Last `00501450` write. Host test after a full loop. |
| `Q_NewOakValeIntro` | START_INITIAL_QUESTS / QST leftover. Not a WLD region name. |

Wiki “you start in Oakvale” is a later quest map. Native no-save
first region name from this file is **LookoutPoint**.

---

## Host lock

`EngineLifecycleTests` after Leave + `LoadWorld` + first pump:

- `WorldFileName == FinalAlbion.wld`
- `World.Regions[0].RegionName == LookoutPoint`
- `RegionAtNativeIndex(1)` is that record
- `CurrentRegion` still null (dummy)
- after `LoadFromFirstRealRegion`, first `SetRegionAsLoaded` action
  contains `LookoutPoint`; `CurrentRegion` is **not** `StartOakVale`

Do not bind `FirstSceneWorld.Region` / `NewGameRegion` as the
Leave answer.

---

## Open

| Item | Class |
|---|---|
| `00501450` first E8 caller | **UNREAD** (body recovered; not `00DBDE40`) |
| First non-null `[NewRegion record+36]` writer | **UNREAD** |
| Persist `PlayerRegionName` writer on continue | **UNREAD** (not this walk) |
