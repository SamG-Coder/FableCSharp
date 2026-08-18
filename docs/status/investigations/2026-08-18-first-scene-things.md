# First no-save New Game Things (LookoutPoint)

Investigation only. `EngineLifecycle.cs` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Raw census from `GameInstall.TryLocate()` + `LevelLibrary` +
`EngineLifecycle` New Game (3 pumps) is
[`2026-08-18-first-scene-things.dump.txt`](2026-08-18-first-scene-things.dump.txt).
Install: TLC `C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters`.

---

## Verdict (read this first)

**First no-save Present is LookoutPoint, not Oakvale.**

Native exist set after `00501450` → `006C2170` is the three
`ContainsMap` TNGs plus one spawned `CREATURE_HERO` at
`GuildArrivalHSP`. Host `RegionThings` matches that set exactly
(465 / 465, 0 missing, 0 extra).

Native first-scene **render** of Things is primary-map Graphic /
CMultiStatic C3Ds only (`SubmitCurrentWorld` bit `0x20`) plus
landscape of opened patches that survive `00BDC2D0` (`0x4`/`0x40`)
plus sky (`0x2000`, not a Thing). Host primary C3D instance set
matches the TNG+hero Graphic set exactly (193 / 193). Neighbour
ContainsMap C3Ds exist as Things and as `PresentWorld` handles;
they are **not** in the submitted mesh.

Oakvale `StartOakValeWest` / `CAM_OVIF_SHOT2` / kid 4300 /
`FirstSceneWorld` is a different contract. It is **DISPROVEN** as
the no-save first Present.

---

## Method

```
GameInstall.TryLocate()
  WorldFile.Load(FinalAlbion.wld)          NewRegion 1
  LevelLibrary.TryLoadThings(ContainsMap)  00521AE0
  004FDBC0 LoadedOnPlayerProximity TNGs    global parse only
  EngineLifecycle ActivateNewGame + 3 Pump
    pump1 frontend → EnterGame
    pump2 dummy index 0
    pump3 00501450(1) Lookout + SubmitCurrentWorld
  PresentWorld()  expandGeometry:false
  SubmitCurrentWorld already ran on pump3
```

Compare key: `(DefinitionType, ScriptName, UID, XYZ±2cm)` for exist;
same + `ResolveSubmit.MeshIds` + `ObjectTransform` translation for
render. Rotation compared only where TNG has `RHSetForward/Up`.

---

## 1. Region  **PROVEN**

| Field | Value | Evidence |
|---|---|---|
| Native index | **1** | `00501450` → `00500540(1,0,0)`; `World.Regions[0].Index==1` |
| Dummy index 0 | not a region | `005066E0` ctor slot; first pump does not `SetRegionAsLoaded` |
| `RegionName` | `LookoutPoint` | WLD `NewRegion 1` |
| `RegionDef` | `REGION_LOOKOUT_POINT` | same |
| Display | `TXT_REGION_LOOKOUT_POINT` | same |
| Persist Oakvale | index **4** `StartOakVale` | only if `PlayerRegionName` nonempty (`00487C20`) |
| `00DBDE40` | not on this tree | `EngineLifecycleTests` + FORWARD_TREE §12 |

Host after pump3: `CurrentRegionIndex=1`,
`CurrentRegion.RegionName=LookoutPoint`,
`FirstSceneMapName=LookoutPoint`, `CurrentStaticMapName=LookoutPoint`.

---

## 2. Maps  **PROVEN**

`006C2170` walks **ContainsMap only** (`004FCBB0` + `00522720`).
`SeesMap` and BWD-touch neighbours are `00B42750` mode-1 **terrain
headers**, not TNG loads.

### ContainsMap (TNG + topology)

| Map | WLD XY | UID | prox | TNG | LEV header | STB | Grid |
|---|---|---|---|---|---|---|---|
| `BowerstoneBridge` | 3232, 3616 | 784595 | True | **88** | 494063 | 3311077 | 128×128 |
| `LookoutPoint` | 3232, 3488 | 162441 | True | **288** | 516731 | 3090776 | 128×128 |
| `GuildExterior` | 3360, 3456 | 179393 | True | **88** | 908695 | 4746916 | 160×224 |

Host `ActivatedMaps` = those three, same order as WLD ContainsMap.

### SeesMap (terrain header only)

`LookoutPoint_Filler_01`, `LookoutPoint_Filler_05`,
`PicnicArea_Filler_02`, `PicnicArea`, `Greatwood_1`, `Greatwood_2`,
`Greatwood_Filler_04`, `PicnicArea_Filler_03`, `Fisherman`,
`LookoutPoint_Filler_06`, `LookoutPoint_Filler_07`.

Fillers have **0** TNG. `PicnicArea` 143, `Greatwood_1` 313,
`Greatwood_2` 112, `Fisherman` 146 — **not** loaded by `006C2170`.

### OpenStaticMaps (`00B42750` mode 1)

`WorldGeometry.StaticMapsAround` = Contains ∪ Sees ∪ BWD AABB touch,
skip `IsSea`. Count **14**. Host `OpenedStaticMaps` is that list;
`LookoutPoint` is current (`00B3E820`); the other 13 are
`00B41E50` neighbours. PicnicArea is a neighbour header, not an
activated TNG map (`DoesNotContain("PicnicArea", ActivatedMaps)`).

---

## 3. TNG  **PROVEN**

`00522720` / `00521AE0` / `00520D00` NewThing loop. TLC has no
`.gtng` (`0050959F` skip **PROVEN**). Loose
`Levels\FinalAlbion\LookoutPoint.tng` is absent; files come from
`FinalAlbion.wad`.

### LookoutPoint.tng (288)

| Kind | Count |
|---|---|
| Object | 192 |
| Marker | 45 |
| Thing | 39 |
| AICreature | 9 |
| Holy Site | 3 |

Sections: `Gameflow:2`, `NULL:252`, `Q_FireHeart:3`,
`Q_GuildTraining:10`, `Q_WaspBoss:3`, `V_BeggarAndChild:14`,
`V_SickChild_Activate:3`, `V_StatueMaster:1`.

**No** `PlayerCreature`. **No** `CREATURE_HERO`. **No**
`CREATURE_HERO_CHILD`. **No** `PARTICLE_EMITTER_*`.

### BowerstoneBridge.tng (88)

Object 67, TrackNode 8, Marker 6, Thing 4, Building 2, Holy Site 1.

### GuildExterior.tng (88)

Marker 37, Thing 27, Object 15, AICreature 6, Building 2, Village 1.

Host `ThingsForMap`: Bridge 88, Lookout 289 (288 + spawned hero),
Guild 88. `RegionThingMapsLoaded=3`.

---

## 4. Global things  **PROVEN** parse / **UNREAD** use

BSS `[0x13B8609]=0` → `004FDBC0` per-map `.tng` for
`LoadedOnPlayerProximity` (151 maps). Host
`GlobalThings.Things.Count=21746`, `GlobalThingMapsLoaded=151`,
`SingleGlobalThingsFile=false`. `.gtg` exists on disk but is
**not** the no-save path.

The three ContainsMaps are also proximity maps, so their TNG bytes
are parsed twice (global list + `006C2170`). Host does **not**
insert `GlobalThings` into `RegionThings`. Native *use* after
`004FDBC0` is still **UNREAD** (status README leftover). It is
**DISPROVEN** that `00521AE0` is that apply.

GTNG: missing skip **PROVEN**.

---

## 5. Spawned  **PROVEN**

Every NewThing is `0051FD80` → `InsertThing` (`004CA010` /
`00662880`). Then `0051E5A0` Activate After Loading.

Because Lookout TNG has no `PlayerCreature`,
`SpawnHeroFromPlayerStart` runs after the ContainsMap loop:

```
HOLY_SITE_PLAYER_START GuildArrivalHSP
  pos (52.688, 69.597, 36.982)
  fwd (1, 0, 0)   up (0, 0, 1)
→ 0049F180 Init Characters
→ 00449D90 PLAYER_HERO miss
→ CREATURE_HERO / 0048A070
→ 006AC910 Create
→ mesh 4299
```

Host `Hero`: `DefinitionType=CREATURE_HERO`, `ScriptName=Hero`,
`HeroMeshId=4299`, XYZ copied from `GuildArrivalHSP`.
`Hero` is appended to `RegionThings` and to
`ThingsForMap("LookoutPoint")`.

`LookoutPointHSP` and `MAIN_START_POSITION` also exist (markers
near 102.8, 74.1). They are **not** the no-save start.

---

## 6. Hero (render)  **PROVEN** mesh / **PARTIAL** pose

| Item | Native expected | Host | Status |
|---|---|---|---|
| Def | `CREATURE_HERO` | same | **PROVEN** |
| Script | `Hero` (created, not TNG) | same | **PROVEN** |
| Graphic | **4299** `MESH_HERO` | `HeroMeshId=4299`, in `SubmittedPalskinMeshIds` | **PROVEN** |
| Bones / skin | 77 / 2117 faces | parsed, PALSKIN not flatten | **PROVEN** |
| Translation | GuildArrivalHSP | `T(52.688, 69.597, 36.982)` | **PROVEN** |
| Axes | HSP `(1,0,0)` / `(0,0,1)` | `Hero` has **no** `RHSet*`; `ObjectTransform` falls back to `+Y` / `+Z` | **PARTIAL** / likely **DIVERGE** rotation |
| Clothing / weapon / morph | sub-defs unread | Graphic 4299 only | **UNREAD** (E-player-palskin) |
| Anim dest | bind pose first-seen | bind locals | **PROVEN** first-seen |
| Kid 4300 | not this tree | not in `RegionThings` | **DISPROVEN** |

`SubmitCurrentWorld` hero fallback (`seen.Add(4299)`) does **not**
fire: PresentWorld already has the instance. No double-submit.

Camera seed `006B3FF0` sits on that pose. Dump camera
`pos=(52.688, 69.597, 36.982)` `fwd=(-1,0,0)` `up=(0,0,1)` `fov=70`.
FOV 70 is GameCamera ctor leftover, not a Thing. SHOT2 72 is
**DISPROVEN** as this Present.

---

## 7. NPCs  **PROVEN** exist / **UNREAD** first-seen draw

Lookout `AICreature` (9), no `CTCPhysicsStandard.Position` in the
TNG (Fable.Dump prints empty pos). `ResolveSubmit` bails on null
XYZ → `Submitted=false`. Host does not emit C3Ds.

| Script | Definition |
|---|---|
| `LookoutPointBeggar` | `CREATURE_BEGGAR_01` |
| `BeggarBully` | `CREATURE_BS_VILLAGER_BULLY_MALE` |
| `FleeingWoman` | `CREATURE_BS_VILLAGER_FEMALE` |
| `WaspHelper` | `CREATURE_BS_VILLAGER_MALE` |
| `FH_Villager` ×3 | `CREATURE_BS_VILLAGER_MALE` ×2, `_FEMALE` ×1 |
| `TalkingTrader1` | `CREATURE_TRADER_01` |
| `TalkingTrader2` | `CREATURE_TRADER_02` |

GuildExterior (exist only, not first-scene `0x20`):
`CREATURE_RIVAL_HERO_SCYTHE`, `CREATURE_DEMON_DOOR_FACE_01`,
`CREATURE_GUILDKEEPER`, `CREATURE_RIVAL_HERO_BRIAR_ROSE`,
`CREATURE_BS_VILLAGER_MALE` ×2. Same no-XYZ pattern.

Native `0051FD80` still constructs them. Who writes a first-seen
world pose (AI activate / village / `V_BeggarAndChild`) is
**UNREAD**. Do not invent marker-teleports (`BeggarHatSpawn`,
`MK_BB_*`) as first-Present C3Ds.

---

## 8. Props  **PROVEN**

Lookout Graphic objects that **must render** (185 TNG + 0
CMultiStatic extras on this map; every submitted Lookout prop is
1 mesh):

| n | Definition | Mesh |
|---|---|---|
| 50 | `OBJECT_WALL_SMALL_POST_01` | 5331 |
| 14 | `OBJECT_WALL_SMALL_STRAIGHT_01` | 5333 |
| 9 | `OBJECT_OAKVALE_FENCE_01` | 7704 |
| 7 | `OBJECT_BRIGHTWOOD_MEDIUMROCK_01` | 7828 |
| 7 | `OBJECT_WALL_SMALL_CURVED_02` | 5325 |
| 6 | `OBJECT_OAKVALE_FENCEPOST_01` | 7700 |
| 6 | `OBJECT_BRIGHTWOOD_MEDIUMROCK_04` | 7834 |
| 6 | `OBJECT_BRIGHTWOOD_MEDIUMROCK_06` | 7836 |
| 5 | `OBJECT_BRIGHTWOOD_LARGEROCK_02` | 7820 |
| 5 | `OBJECT_OAKVALE_FENCEPOST_02` | 7702 |
| 5 | `OBJECT_OAKVALE_FENCETRIPLE_01` | 7706 |
| 4 | `OBJECT_BRIGHTWOOD_MEDIUMROCK_03` | 7832 |
| 4 | `OBJECT_OK_PILLAR_COLLAPSED_01` | 7168 |
| 4 | `OBJECT_STREETSIGN_01` | 4911 |
| 4 | `OBJECT_TOWNBENCH_01` | 7548 |
| 3 | `OBJECT_BRIGHTWOOD_LARGEROCK_05` | 7826 |
| 3 | `OBJECT_BRIGHTWOOD_MEDIUMROCK_02` | 7830 |
| 3 | `OBJECT_WALL_SMALL_CURVED_BIG_01` | 5327 |
| 3 | `OBJECT_WALL_SMALL_STRAIGHT_BIG_01` | 5335 |
| 3 | `OBJECT_WALL_SMALL_STRAIGHT_BIG_BROKEN_01` | 5337 |
| 2 | `OBJECT_BRIGHTWOOD_LARGEROCK_03` | 7822 |
| 2 | `OBJECT_BRIGHTWOOD_LARGEROCK_04` | 7824 |
| 2 | `OBJECT_DEGRADABLE_THORN_VINES_01` | 3977 |
| 2 | `OBJECT_OAKVALE_FENCE_BROKEN_01` | 7708 |
| 2 | `OBJECT_OAKVALE_FENCEGATE_01` | 7712 |
| 2 | `OBJECT_OK_RUBBLE_PILLAR_01` | 7168 |
| 2 | `OBJECT_REGION_TRANSITION_GATE` | 4067 |
| 2 | `OBJECT_WALL_SMALL_CURVED_01` | 5323 |
| 2 | `OBJECT_WALL_SMALL_RIGHTANGLE_BIG_01` | 5319 |
| 1 | `OBJECT_BIGROCK_01` | 7802 |
| 1 | `OBJECT_BIGROCK_02` | 7804 |
| 1 | `OBJECT_BLASTEDTREE` | 7687 |
| 1 | `OBJECT_BS_SIGN_POST_DIRECTION_01` | 4911 |
| 1 | `OBJECT_LOOKOUT_POINT_KNIGHT_STATUE_01` | 5357 |
| 1 | `OBJECT_OAKVALE_FENCEGATE_02` | 7719 |
| 1 | `OBJECT_OAKVALE_FENCETRIPLE_BROKEN_01` | 7710 |
| 1 | `OBJECT_OK_ARCH_DOUBLE_CLOSED_HALF` | 7017 |
| 1 | `OBJECT_OK_ARCH_SINGLE_OPEN` | 7021 |
| 1 | `OBJECT_OK_PILLAR_COLLAPSED_02` | 7031 |
| 1 | `OBJECT_OK_PILLAR_COLLAPSED_03` | 7170 |
| 1 | `OBJECT_SILVER_KEY` | 7934 |
| 1 | `OBJECT_WALL_SMALL_CURVED_180` | 5309 |
| 1 | `OBJECT_WALL_SMALL_CURVED_BIG_BROKEN_01` | 5329 |
| 1 | `OBJECT_WALL_SMALL_RIGHTANGLE_SMALL_01` | 5321 |
| 1 | `OBJECT_WALL_SMALL_STRAIGHT_BIG_BROKEN_02` | 5313 |

All 185 have TNG `CTCPhysicsStandard` pos + RH axes. Host
`ObjectTransform` translations match within 5 cm. Mesh ids match
`GameBin.FindMeshIds`. Parse fail = 0.

TNG **duplicate** (two UIDs, same def, same pose):
`OBJECT_OAKVALE_FENCEPOST_01` mesh 7700 at
`(48.605, 65.920, 37.186)` UIDs `…6472` and `…6982`. Native file
has both; host submits both. Not a host invent.

No Lookout `BUILDING_*`. Bridge/Guild buildings exist (see §11)
and are not first-scene `0x20`.

---

## 9. Doors  **PROVEN** exist off-primary / **none** on Lookout

Lookout TNG has **no** `*DOOR*` definition. Fence **gates**
(`OBJECT_OAKVALE_FENCEGATE_01/02`) are props, above.

ContainsMap doors that **exist** after `006C2170` but must **not**
be in the first-scene submitted set:

| Map | n | Definition | Mesh |
|---|---|---|---|
| BowerstoneBridge | 2 | `OBJECT_BS_GATEHOUSE_MAIN_DOORS_01` | 5080 |
| GuildExterior | 1 | `OBJECT_GUILD_FRONT_DOORS_01` | 6078 |
| GuildExterior | 1 | `OBJECT_DEMON_DOOR_WALL_01` | 3987 |
| GuildExterior | 1 | `OBJECT_DEMON_DOOR_OVERGROWN_01` | 3983 |

Host Submit: 0 of these (primary filter). PresentWorld still
records them as neighbour instances (**extra handles**, not extra
draws).

---

## 10. Effects  **PROVEN** absent as Things / **UNREAD** runtime

No `PARTICLE_EMITTER_*`, no `*EFFECT*` NewThing on the three
ContainsMaps. `GameBin.FirstSeenInstancesAsC3d` would reject
particle names even if present.

Runtime particle / weather / `0x20000` water mesh when a later bind
would run: **UNREAD**. First-seen water draw is empty-out
**PROVEN**. Stars `00B65A20` first dword==0 **PROVEN** not emitted.

Sky `0x2000` 9×37 ellipsoid is **not** a Thing.

---

## 11. Lights  **PROVEN** (mesh lamps) / **UNREAD** (true lights)

### Lookout — must render (7)

All `OBJECT_STREETLAMP_LIT_SINGLE_01` mesh **4978**:

| XYZ | Forward |
|---|---|
| 80.101, 105.117, 38.338 | 0.995, 0.105, 0 |
| 126.445, 75.485, 30.164 | −0.788, −0.616, 0 |
| 50.392, 79.403, 35.648 | 0.643, 0.766, 0 |
| 72.596, 54.225, 41.337 | 0.809, −0.588, 0 |
| 71.469, 89.023, 40.000 | −0.951, 0.309, 0 |
| 98.007, 72.464, 38.121 | −0.616, −0.788, 0 |
| 47.196, 67.908, 37.114 | −0.978, −0.208, 0 |

Host submitted all seven, same mesh, translation match.

### Exist, not first-scene `0x20`

- BowerstoneBridge: 4 × same streetlamp 4978
- GuildExterior: 2 × streetlamp 4978
- GuildExterior: 1 × `MARKER_LIGHT` (TypeName `MARKER`, no C3D)

True light records / FFP / engine light list: **UNREAD**. First-seen
VS uses leftover dirlight `c19/c20/c35`, not these Things.

---

## 12. Gizmos that exist and must not render  **PROVEN**

Lookout (reject `FirstSeenInstancesAsC3d` or no Graphic):

| Bucket | n | Notes |
|---|---|---|
| Camera | 34 | `CAMERA_POINT_SCRIPTED` 12, `_SPLINE` 20, `_GLOBAL` 2. Includes `CAM_GTA_SHOT2` near the hero — **not** Oakvale SHOT2 |
| Marker | 45 | `MARKER_BASIC` 33, `MARKER_GLOBAL` 5, `MARKER_INFO_DISPLAY` 3, `GAZE_OUT_OF_BUILDING_MARKER` 3, `MARKER_FISHING_SPOT` 1 |
| Holy Site | 3 | `GuildArrivalHSP`, `LookoutPointHSP`, `MAIN_START_POSITION` |
| Travel | 5 | `REGION_ENTRANCE_POINT` ×3, `REGION_EXIT_POINT` ×2. TypeName `THING`, `AsC3d=true`, **no** mesh → `PresentWorld.MissingMeshes=10` across all ContainsMaps. Not submitted |

Bridge also: `TRACK_NODE_BASIC` ×8, `NAVIGATION_SEED` ×1,
`HOLY_SITE_PLAYER_START` ×1.

Host Submit: 0 gizmos. Match.

---

## 13. PresentWorld vs SubmitCurrentWorld

### PresentWorld (`expandGeometry: false`)  **PROVEN**

| Field | Value |
|---|---|
| `Region` | `LookoutPoint` |
| `Expanded` | false |
| `Triangles` | empty |
| `Regions` | 14 opened maps |
| `MeshInstances` | **280** = 193 Lookout + 69 Bridge + 18 Guild |
| `PlayerMeshId` | 4299 |
| `MissingMeshes` | 10 (`REGION_ENTRANCE_POINT` / `REGION_EXIT_POINT`) |

Neighbour instances are **extra vs primary-only draw**, but they
match native **exist** (those TNG Graphics were `006C2170` loaded).
Sees/BWD maps contribute **0** instances (no `thingsByMap` entry).

### SubmitCurrentWorld  **PROVEN**

Filter: `inst.Map == opened.Region` (LookoutPoint) + hero fallback.

| Field | Value |
|---|---|
| `SubmittedWorld` | same unexpanded PresentWorld |
| Primary C3D instances | **193** (45 unique mesh ids) |
| `SubmittedMesh` verts / draws | 917496 / 590 |
| PALSKIN ids | `[4299]` |
| Terrain maps that emitted strips | `LookoutPoint`, `PicnicArea_Filler_02`, `PicnicArea`, `Greatwood_2`, `PicnicArea_Filler_03`, `Fisherman` |
| Layer bits | `0x4`, `0x40`, `0x20`, `0x2000` |
| Hero fallback | not used |

Bridge / Guild / filler / Greatwood_1 landscapes are opened but
did not pass the host whole-map `00BDC2D0` AABB (camera on Lookout
looking −X). Native is **per-patch** AABB; host is still
**DIVERGE** at cell grain (investigation C). Which neighbour
**tiles** should appear is PARTIAL; which neighbour **Things**
should draw is primary-only **PROVEN**.

---

## 14. Compare table

| Class | Count | What |
|---|---|---|
| **Missing exist** | **0** | 465 expected = 465 `RegionThings` |
| **Extra exist** | **0** | no Oakvale / kid / GlobalThings leak into `RegionThings` |
| **Duplicate exist** | **0** host keys | TNG itself has one stacked fencepost (two UIDs) |
| **Missing render** | **0** | 193 Graphic instances all submitted |
| **Extra render** | **0** | no gizmo / neighbour C3D / kid in `SubmittedMesh` props |
| **Duplicate render** | **1 pair** | the two TNG fenceposts at `(48.605, 65.920, 37.186)` — native file, host correct |
| **Wrong map** | **0** in Submit | all 193 `map=LookoutPoint` |
| **Wrong transform (translation)** | **0** | ±5 cm vs TNG |
| **Wrong transform (rotation)** | **Hero only** | HSP `+X`; host `Hero` default `+Y`. Props match RHSet | **PARTIAL** |
| **Wrong definition** | **0** | `CREATURE_HERO` not `PLAYER_HERO` / not child |
| **Wrong graphics** | **0** parsed | 45/45 C3Ds in `MBANK_ALLMESHES`; hero 4299 PALSKIN |

### Host extras that are not Submit extras

| Item | Where | Native | Status |
|---|---|---|---|
| 69 Bridge + 18 Guild mesh **handles** | `PresentWorld.Instances` | Things exist; first Present `0x20` is primary-only | **PARTIAL** (handles extra, draws match) |
| `GlobalThings` 21746 | parsed, not inserted | parse **PROVEN**; apply **UNREAD** | |
| Whole-map AABB then dump-all tiles | terrain | per-cell `00BF4570` | **DIVERGE** (C) — not a Thing |

### Host / native non-Thing submit (out of Thing set, in the frame)

| Item | Native | Host | Status |
|---|---|---|---|
| Landscape Lookout | yes | yes | **PROVEN** |
| Landscape 5 neighbours | some patches | those 5 maps | **PARTIAL** grain |
| Sky dome | `0x2000` | yes | **PROVEN** |
| Water mesh | empty-out | none | **PROVEN** |
| Stars | off | none | **PROVEN** |

---

## 15. Exact first-Present Thing render set

The Things that **should exist and render** on first no-save New
Game (LookoutPoint `0x20`):

1. **Hero** `CREATURE_HERO` / `Hero` / C3D **4299** at
   `GuildArrivalHSP` `(52.688, 69.597, 36.982)`.
2. **7** `OBJECT_STREETLAMP_LIT_SINGLE_01` / **4978** (table §11).
3. **185** Lookout Graphic props (table §8). Includes 2 fence
   gates, 2 region-transition gates, 1 knight statue, 1 silver key.
4. **Not** the 9 Lookout AICreatures (no TNG pose).
5. **Not** cameras / markers / holy sites / track nodes / travel
   gizmos.
6. **Not** Bridge or Guild buildings, doors, lamps, village def
   `VILLAGE_GUILD_COMPLEX_OUTSIDE` (4516), demon-door meshes.

Exist-only (constructed, no first-scene C3D): the rest of the 464
TNG NewThings + those 9 AI + 34 cameras + 48 markers/holy + 5
travel on Lookout, plus all 176 Bridge+Guild Things.

---

## Ledger

| Claim | Status | Evidence |
|---|---|---|
| No-save first region / first rendered scene is LookoutPoint index 1 | **PROVEN** | WLD + lifecycle pump3 + tests |
| First Present is Oakvale / SHOT2 / kid 4300 / `00DBDE40` | **DISPROVEN** | same |
| ContainsMap TNG set is Bridge + Lookout + Guild | **PROVEN** | WLD + `006C2170` + `ActivatedMaps` |
| Sees/BWD maps get TNG on this load | **DISPROVEN** | 0 `006C2170`; PicnicArea not in `ActivatedMaps` |
| GTNG missing skip | **PROVEN** | TLC no `.gtng` |
| Global `004FDBC0` parse of prox `.tng` | **PROVEN** | 151 maps / ~21k things |
| Global-things *apply* into the scene | **UNREAD** | not `00521AE0` |
| Lookout TNG 288, no PlayerCreature | **PROVEN** | wad parse + dump |
| Hero spawn at `GuildArrivalHSP` mesh 4299 | **PROVEN** | `0051FD80` test + dump |
| `RegionThings` == Contains TNG + Hero | **PROVEN** | 465=465 |
| Primary Graphic submit == TNG Graphic + Hero | **PROVEN** | 193=193, 0 miss, 0 extra |
| Neighbour C3Ds in Submit | **DISPROVEN** | primary filter |
| Neighbour C3D **handles** in PresentWorld | **PROVEN** host fact; native draw **PROVEN** absent | 69+18 |
| Prop translation / mesh id | **PROVEN** | dump compare |
| Hero rotation from HSP axes | **PARTIAL** | host drops RHSet on the created Thing |
| NPC first-seen mesh | **UNREAD** | TNG has no XYZ; activate pose unread |
| Particle / true light Things | **PROVEN** none in TNG; runtime **UNREAD** | |
| `REGION_*_POINT` as C3D | **DISPROVEN** as draw | `AsC3d` true, no Graphic; 10 “missing” |
| TNG stacked fencepost | **PROVEN** file dup, host submits both | UIDs `…6472` / `…6982` |
| Landscape / sky / water / layers | **PROVEN** as frame, not Things | `0x4/0x40/0x20/0x2000` |
| Per-patch vs whole-map terrain AABB | **DIVERGE** | investigation C; not a Thing miss |

Do not wire `FirstSceneWorld` / `StartOakValeWest` onto this
Present. Persist `PlayerRegionName` writer stays **UNREAD**.
