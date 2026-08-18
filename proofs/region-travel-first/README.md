# RegionTravel first use after Leave Frontend

Investigation only. No production `src` edits.
Do **not** start at Oakvale / `00DBDE40` / `RegionTravel.StartingRegion`.
That path is later `Q_NewOakValeIntro` (`00DABAC0` → `00DBDE40`),
not Leave / Init Game / first no-save region.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **LEFTOVER**.

Sources: `docs/runtime/FORWARD_TREE.md` §§4–12;
`docs/status/investigations/2026-08-18-first-scene-things.md`;
`proofs/camera-after-leave/README.md`;
`RegionTravel.cs` / `WorldFile.cs` / `EngineLifecycle.cs`;
`EngineLifecycleTests` (`Frontend_0059A238_message_15_sets_retail_41`,
`Install_banks_and_startup_videos_exist`,
`Loading_objects_00521AE0_loads_LookoutPoint_tng`);
`WorldSceneTests`;
ExeIndex `calls-startoakvale-00dbde40` (1 hit: `00DAC295`).

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| Does `StartOakValeSetup` (`00DBDE40`) run during frontend? | **No.** Retail pump `0042EC7C` / `0042DF9E` never `E8` it. | **DISPROVEN** |
| First region name after Leave? | WLD `NewRegion 1` / `Maps[0]` **`LookoutPoint`**, not wiki Oakvale. | **PROVEN** |
| First `RegionTravel` *region-name* helper after Leave? | `StartingRegion` is **not** that site. It prefers hardcoded `StartOakValeWest`. | **LEFTOVER** |
| When is `00DBDE40` first-seen? | Only `00DAC295` inside `00DABAC0` (`S_QNOVI` slot 2), after Oakvale quest construct. | **PROVEN** later leftover |

---

## Timeline (no-save New Game)

```
0042EC7C retail
  PlayAVI slots
  Init frontend 0042EF6F
  0042DF9E 2D UI  (no WLD, no region, no 00DBDE40)
  msg 15 → [retail+41]=1
0042F2A2 Leave frontend          // not 00DBDE40
  0042F44D record FinalAlbion.wld
  009BE420 clear + 009BEEB0 Present
0042F491 Init Game → 00418DCA → 004184BD
  Init World 004A6E30
  00416953 Load world
    00507C30 parse FinalAlbion.wld
      NewMap 1 LevelScriptName "LookoutPoint"     // Maps[0]
      NewRegion 1 RegionName "LookoutPoint"       // native index 1
      NewRegion 4 RegionName "StartOakVale"       // persist / later
      START_INITIAL_QUESTS  (not Q_NewOakValeIntro)
004189C2 first pumps
  WorldFrame dummy: index 0, CurrentRegion=null
  no SetRegionAsLoaded, no 00501450, no 00DBDE40
later (E8 caller UNREAD)
  00501450  first real 00500540(1,0,0)
    native index 1 = WLD NewRegion 1 LookoutPoint
    ContainsMap TNG: LookoutPoint / BowerstoneBridge / GuildExterior
    HOLY_SITE_PLAYER_START GuildArrivalHSP → CREATURE_HERO
```

`Q_NewOakValeIntro` / `S_QNOVI` / `NOVStartHSP` / `CREATURE_HERO_CHILD`
are **not** on this list. **PROVEN**.

---

## 1. StartOakValeSetup during frontend?

| Claim | Class | Evidence |
|---|---|---|
| Frontend Present is 2D (`0042DF9E` / `009BEEB0`) | **PROVEN** | FORWARD_TREE §4; `PumpFrontendFrame` |
| Leave is `0042F2A2` then `FinalAlbion.wld` | **PROVEN** | `RequestNewGame`; `0042F44D` |
| `00DBDE40` E8 count on PE | **1** (`00DAC295`) | ExeIndex `calls-startoakvale-00dbde40` |
| That site is `00DABAC0` (`IntroQuestRun` slot 2) | **PROVEN** | `RegionTravel.IntroQuestRunCallsSetup` |
| RunModes / Leave / `004184BD` / `00501450` `E8` `00DBDE40` | **DISPROVEN** | FORWARD_TREE §12 |
| Host frontend / Leave / first game pump traces `Va==00DBDE40` | **DISPROVEN** | `EngineLifecycleTests` dozens of `DoesNotContain(...StartOakValeSetup)` including `Frontend_0059A238_message_15_*` and bootstrap |
| `PumpFrontendFrame` / `InitFrontendUi` notes `00DBDE40` | **DISPROVEN** | no `Note(StartOakValeSetup, …)` |
| PeEntry / WinMain / RetailPump == `00DBDE40` | **DISPROVEN** | `Pe_entry_is_crt_not_new_game` |

**Answer:** `StartOakValeSetup` must not run during frontend. It is not a
retail / Leave callee. Host must not `Note` or jump to `00DBDE40` from
`PumpFrontendFrame`, `RequestNewGame`, or first `004189C2`.

---

## 2. First region name: WLD, not wiki

“Wiki” here is the story that New Game starts in Oakvale
(`StartOakVale` / `StartOakValeWest` / kid at `NOVStartHSP`).
That is a later quest map, **not** the first WLD region.

WLD authority is `00507C30` (`LoadWldFile`) reading
`Data\Levels\FinalAlbion.wld`:

| Slot | WLD token | Name | Native index |
|---|---|---|---|
| First map | `NewMap` first / `LevelScriptName` | `LookoutPoint` | map table 0 |
| First region | `NewRegion 1` `RegionName` | `LookoutPoint` | **1** (`005066E0` dummy occupies 0) |
| Oakvale region | `NewRegion 4` `RegionName` | `StartOakVale` | **4** |
| Oakvale west map | `ContainsMap` of region 4 | `StartOakValeWest` | not first |

Host after Leave + `LoadWorld` (before `00501450`):

| Field | Value | Class |
|---|---|---|
| `WorldFileName` | `FinalAlbion.wld` | **PROVEN** (`0042F44D`) |
| `World.Maps[0].ScriptName` | `LookoutPoint` | **PROVEN** (`WorldSceneTests`) |
| `World.Regions[0].Index` | `1` | **PROVEN** |
| `World.Regions[0].RegionName` | `LookoutPoint` | **PROVEN** |
| `World.Regions[3].RegionName` | `StartOakVale` | **PROVEN** |
| `CurrentRegion` on first pump | `null` (dummy 0) | **PROVEN** |
| `00501450` first `00500540(i,0,0)` | `i=1` Lookout | **PROVEN** |
| Persist `PlayerRegionName` | empty no-save | **PROVEN**; `00487C20` is continue |

First *loaded* name after dummy is therefore
`World.Regions[0].RegionName` == `LookoutPoint` (native index 1).
Do **not** invent the string from `RegionTravel.NewGameRegion`.

---

## 3. First `RegionTravel` use after Leave

`RegionTravel` is a mixed bag: PlayAVI / fade (frontend), Oakvale intro
(leftover), and a few shared thing helpers.

### During frontend (before Leave)

| Member | Role | Class |
|---|---|---|
| `PlayAvi*` / `ResolvePlayAviFile` / skip scan | startup + retail AVI | **PROVEN** frontend |
| `FadeOverlay*` aliased as `Frontend2d*` | 2D record size / vtbl | **PROVEN** frontend |
| `StartOakValeSetup` / `StartingRegion` / `NewGameRegion` | unused on this pump | **PROVEN** absence |

### After Leave / Init Game / first real region

Native first region **does not** call `RegionTravel.StartingRegion`.
`EngineLifecycle` takes the name from the parsed `WorldFile`:

```
RegionAtNativeIndex(1) → World.Regions[0].RegionName  // LookoutPoint
FirstSceneMapName from GuildArrivalHSP map            // LookoutPoint
```

First *shared* `RegionTravel` symbols on that path:

| Member | When | Class |
|---|---|---|
| `AdultCreature` (`CREATURE_HERO`) | `CreatureHeroDefName`; Lookout spawn | **PROVEN** no-save |
| `PlayerStartType` (`HOLY_SITE_PLAYER_START`) | `0051FD80` / `GuildArrivalHSP` | **PROVEN** |
| `PositionOf(Hero)` | host camera after seed | **PARTIAL** vs native helper |
| `FindPlayerStart` | **not** first-seen | **LEFTOVER** (prefers `NOVStartHSP`) |
| `StartingRegion` | **not** first-seen | **LEFTOVER** (prefers `StartOakValeWest`) |
| `StartOakValeSetup` | **not** first-seen | **DISPROVEN** as this site |

```csharp
// leftover — wiki / Oakvale intro, not Leave
public static string StartingRegion(WorldFile world) =>
    world.FindMap(NewGameRegion)?.ScriptName   // "StartOakValeWest"
    ?? (world.Maps.Count > 0 ? world.Maps[0].ScriptName : NewGameRegion);

// first-seen after Leave would be WLD order:
//   world.Regions[0].RegionName   // "LookoutPoint"
//   or world.Maps[0].ScriptName   // "LookoutPoint"
```

`FindPlayerStart` still ranks `NOVStartHSP` / `StartOakValeHSP` above
`MAIN_START_POSITION` / `LookoutPointHSP`. Live no-save spawn is
`EngineLifecycle.GuildArrivalHsp`, not that helper.

---

## 4. C# leftovers that still say Oakvale is first

| Site | What it claims | Class vs Leave |
|---|---|---|
| `RegionTravel.NewGameRegion` | `"StartOakValeWest"` | **LEFTOVER** vs no-save |
| `RegionTravel.StartingRegion` | finds that map first | **LEFTOVER** |
| `RegionTravel` type header | “Kid start is WLD StartOakVale… Maps[0] Lookout is not new-game” | **LEFTOVER** vs FORWARD_TREE |
| `FirstSceneWorld.Region` | aliases `NewGameRegion` | **LEFTOVER** (Oakvale contract only) |
| `WorldSceneTests.New_game_starts_as_kid_in_start_oakvale_not_lookout` | locks `StartingRegion==StartOakValeWest` | **LEFTOVER** vs live New Game |
| `ScenePassTests` same assert | same | **LEFTOVER** |
| `docs/render/FIRST_SCENE_CONTRACT.md` | Oakvale first scene | **LEFTOVER** vs no-save Present |
| `00CD6E27` bind `S_QNOVI` during QST parse | factory exist, not run | **PROVEN** bind; **DISPROVEN** as first runner |

Do not treat those asserts as the Leave / first-region contract.

---

## Classifications (short)

1. **`StartOakValeSetup` during frontend — DISPROVEN.** Sole `E8` is
   `00DAC295`. Frontend / Leave / first dummy pump traces never contain
   `0x00DBDE40`.
2. **First region name after Leave — WLD `LookoutPoint` (NewRegion 1 /
   Maps[0]). PROVEN.** Oakvale is native index 4 / persist-only.
3. **`RegionTravel.StartingRegion` as first-use after Leave — LEFTOVER.**
   First name is `WorldFile` parse, not the wiki `StartOakValeWest`
   override. First shared helpers are `PlayerStartType` /
   `AdultCreature` on Lookout `GuildArrivalHSP`.
4. **`00501450` caller — UNREAD.** Not the second `004189C2` inner
   iteration. Not `00DBDE40`.
