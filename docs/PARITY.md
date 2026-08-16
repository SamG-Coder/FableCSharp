# TLC parity ledger

Facts below are either asserted by tests against a live Steam install of
*Fable: The Lost Chapters*, or recorded here as a failed decode so we do not
retry the same wrong guess. Game assets stay in Steam. This repo only ships
parsers and notes.

## Worked

| Piece | What we know | Locked by |
|---|---|---|
| Install | TLC `data\` is the source of truth. Anniversary FableData is a fallback. | `TlcInstallTests` |
| WLD | Text region graph. Map 1 is Lookout Point at (3232, 3488). | `TlcInstallTests` |
| WAD / BBB | `FinalAlbion.wad` is a BBB bank of `.lev` / `.tng` / etc. `Find` requires the extension. | `TlcInstallTests` |
| TNG | Version-2 text. Things have `DefinitionType` plus `CTCPhysicsStandard` position and axes. Lookout has ~288 things. | `TlcInstallTests` |
| QST | Quest table includes `Q_SunnyvaleMaster`. | `TlcInstallTests` |
| names.bin | 20-byte header, then `(u32 hash, cstring)` pairs. Lists `MARKER_BASIC` and Oakvale names. | `TlcInstallTests` |
| BIGB | `graphics.big` / `textures.big` have named sub-banks. Mesh bank ids match `meshdata.h`. | `TlcInstallTests` |
| LZO | Fable framing: `u16` size, `0xFFFF` + `u32` for large blocks, last 3 bytes raw. | mesh / texture tests |
| C3D mesh | Positions parse. Packed `POSPACKED3` + scale/offset. Mesh space is centimetres (`* 0.01` to WLD). Apple tree id 5228 has 1699 tris. | `WorldGeometryTests`, `MeshFormatTests` |
| C3D materials | After the root matrix: `id`, name, `DecalID`, **`DiffuseMapID`**, bump / reflection / illumination. | `MeshFormatTests` |
| C3D UVs | After packed pos + packed normal. Packed UV is `int16 / 2048 - 8`. Stride-12 static verts put UV at byte 8. Values sit around 0–1 with some tiling. | `MeshFormatTests` |
| textures.big | 34-byte info: `u16` w/h at 0/2, frame w/h at 6/8, format at 12. **31 = DXT1**, **32 = DXT5**, **35 = DXT5 16-byte block** (sky only). Type 4 / format 1 = RGBA8. Payload is framed LZO. | `TextureFormatTests` |
| DiffuseMapID | Mesh material `DiffuseMapID` **is** the `textures.big` entry id. 3880 = oak trunk, 2119 = apple leaves, 414 = `LANDSCAPE_GRASS_PLAIN`. | `MeshFormatTests` |
| WAD `.lev` | Version 25, constant `0x1904`. 255×132 material table from 179 (`INVALID_THEME_STANDIN`, then `GROUND_*`). Sound themes after 67639. Payload starts with `21`. This file is a material/theme table, **not** a C3D mesh. | `LevFormatTests` |
| STB coarse height | Runtime `FinalAlbion_RT.stb` copy of `.lev` is ~3 MB. Bytes 0–2047 are pad (`u32[0]=1`). From 2056, 36-byte records hold two WLD-space verts on a **16-unit** lattice. Lookout is 8×8 quads, Picnic 8×6. Heights 20–80 match TNG Z. | `LevFormatTests` |
| STB section 2 offset | `u32` at 2048 is the start of the next blob (Lookout/OakVale **6144**, Picnic **4096**). Zeros fill the gap after the coarse lattice. | `LevFormatTests` |
| WAD cell table | Payload after the `21` tag is a stream of **21-byte** records, one per 1-unit cell (Lookout 16384). Bytes 10–13 are material-table slots (`0xFF` unused). Lookout is mostly slot 2 `GROUND_PATH_SAND`. | `LevFormatTests` |
| GROUND_ → LANDSCAPE_ | Material-slot `u32` id is **not** a `textures.big` id (1911 is villager legs). Names map: `GROUND_PATH_SAND` → `LANDSCAPE_PATH_SAND_01` (4133), `GROUND_GRASS` → `LANDSCAPE_GRASS_PLAIN` (414), `PATH_COBBLES_IRREGULAR_ET` → `LANDSCAPE_COBBLES_IRREGULAR_01` (4118). | `LevFormatTests` |
| Fine terrain mesh | 1-unit quads still exist for sampling. The **drawn** landscape is the STB tile mesh: index **strip** for adaptive tiles, 17×17 quads only when `v=289` and the lattice is complete. Treating 70%-filled adaptive tiles as a grid skipped ~1120 Lookout quads (sky holes). | `LevFormatTests` |
| GPU texturing | Mesh vertex is pos/normal/UV. Draws are grouped by `textures.big` id. RGBA is uploaded as `R8G8B8A8` with a repeat/linear sampler. Fragment shader samples `albedo`. Some bank ids fail framed LZO and fall back to a 1×1. | `GpuTextureTests` |
| STB tile table | After the coarse 36-byte lattice, leftover `magic/offset/size` is a tile directory (`0x012EC900`, file offset, packed size). Lookout 63 tiles, Picnic 47. Last record is a 0,0 sentinel. | `LevFormatTests` |
| STB tile payload | `u32` uncompressed size, `u32` packed size, then **raw LZO** (not framed). Inflated 32-byte header: `u16` extra-object count, `u16` vert count at +2 (289 = 17×17), `u16` strip length at +4, `u16` flag at +18 (`256` = implicit 17×17, no primary strip). | `LevFormatTests` |
| STB tile verts | 32-byte header, then `count` × **15-byte** records: `u16` world X, `u16` world Y, `f32` Z, 7 unread bytes (packed normal / colour). Mesh origin is the coarse cell's **east** edge. | `LevFormatTests` |
| STB section 2 tile | The blob at `u32@2048` is the **same** raw-LZO tile. Lookout covers WLD `[3232,3248]×[3488,3504]` (map origin, 289 verts). Picnic covers `[3104,3120]×[3520,3536]`. This is the west-south cell the table does not store. | `LevFormatTests` |
| STB tile indices | After the vertex array, adaptive tiles store a D3D triangle strip. Header `u16@+4` is **PrimitiveCount**; IndexCount is that plus 2. Treating it as an index count dropped the last 1 m triangle of every strip (Lookout area ≈ 1). | `LevFormatTests` |
| STB edge-strip objects | After the primary strip (or immediately when flag=256) sit `CPatchTesselationEdgeStrip` blobs. 30-byte header `v, primitiveCount, fmt…` then 15-byte world verts + PrimitiveCount+2 indices. Lookout tiles carry 3–16 extras; they fill the ~11% of 1 m cells the primary strip does not cover. | `LevFormatTests` |
| Camera | System.Numerics row-major is uploaded as-is (no extra transpose). Z-up. Overview `(64,-40,95)` looks at `(64,64,36)`. | `CameraProjectionTests` |
| Object basis | C3D is Z-up centimetres. TNG `RHSetForward` / `RHSetUp` are a right-handed Z-up frame (mesh X=right, Y=forward, Z=up). Streetlamp 4978 is 345 units tall in **Z**. | `WorldGeometryTests` |
| Region space | One region is drawn in **local** metres: TNG XY is 0–128 on Lookout, not WLD `(3232,3488)`. STB world XY is subtracted by MapX/MapY. Object Z matches fine terrain (median ~10 cm). `ObjectScale` multiplies the 0.01 mesh scale. | `WorldGeometryTests` |
| TNG → mesh (partial) | `meshdata.h` name match: `DefinitionType`, `MESH_` + type, or `MESH_` + stem after `OBJECT_` / `CREATURE_` / `BUILDING_`. Skip `[PHYSICS]`. Still useful as a fallback. | `MeshFormatTests` |
| names.bin offsets | Each name is `(u32 CRC, cstring)`. The game.bin name-ref is the string's offset after the 20-byte header (first string is 4). CRC is Fable polynomial `0xEDB88320` init 0. | `GameBinFormatTests` |
| game.bin container | 13-byte header: `u8 use_names_bin` (TLC = 0), `u32 file`, `u32 platform`, `u32 entry_count` (14761). Then `NameRef` × N (`type_off`, `file_off`, `counter`). Then chunk index + **zlib level 1** chunks (`78 01`). | `GameBinFormatTests` |
| game.bin OBJECT → mesh | Instance name is the `file_off` names.bin string (`OBJECT_WALL_SMALL_POST_01`). Body starts with a 3-byte preamble, then a sub-def table, then control fields. Field CRC `"Graphic"` is followed by `EngineGraphic` (`type i32`, **`bank_index i32`** = `graphics.big` mesh id). | `GameBinFormatTests` |
| Lookout TNG → mesh | Walls 5331 `MESH_SMALL_WALL_CURVED_POST_01`, Brightwood rocks 7828 `MESH_MEDIUMROCK_LICHEN_01`, streetlamps 4978 `MESH_OBJECT_STREETLAMP_OFF_02`, pillars 7168, thorn vines 3977, villager 5149. Lookout instances **> 150**. Markers/cameras stay gizmos. | `GameBinFormatTests`, `WorldGeometryTests` |
| frontend.bin / script.bin | Same GameBin container as game.bin (13-byte header, platform `0xA8E36C34`). Frontend is 810 entries, mostly `UI`. Script is 611 entries, mostly `CCutsceneDef` (`CS_ATTRACT_1`, …). | `DataCatalogTests` |
| FinalAlbion.bwd | Region index: 398 records matching every WLD map. Record = path + name + 3 flag bytes + `u32` min/max X/Y (Lookout `3232,3488`–`3360,3616`) + 9 bytes whose `u32` at +1 is the WLD **MapUID** (Lookout 162441). Declared count is 399; leftover after 398 is a second unread blob. | `DataCatalogTests`, `WorldSceneTests` |
| Starting region graph | `Misc\FinalAlbion_StartingRegionGraph.txt` is `"Region": "Neighbour", …`. Lookout lists Picnic, BowerstoneSlums, GreatwoodEntrance, guild, demon door. Picnic AABB shares Lookout's west edge. | `WorldSceneTests` |
| TNG region exits | Lookout has `REGION_ENTRANCE_POINT`, `REGION_EXIT_POINT`, `OBJECT_REGION_TRANSITION_GATE`. `EntranceConnectedToUID` packs `(MapUID << 40) \| entranceSlot`. Slot is the low 32 bits of the dest `REGION_ENTRANCE_POINT.UID` (`0xFFFFFE00_00000000 \| slot`). Lookout→Picnic is slot `0x21` at local (79.6, 55.6). ≥120 WAD exits resolve. | `WorldSceneTests` |
| Player start + walk | **New game is not Lookout.** WLD `NewRegion 4` `StartOakVale` (`TXT_REGION_OAKVALE`) contains `StartOakValeWest` / East / MemorialGarden. QST `AddTestQuest("Q_NewOakValeIntro","NOVStartHSP")`. TNG+GTG `NOVStartHSP` is on WLD map 203 `StartOakValeWest` at (34.45, 129.03), next to `HerosOldHouse`. Exe `00DBDE4A` `StartOakVale`, `00DBDF09` `CREATURE_HERO_CHILD`. Childhood TNG has **no** `REGION_EXIT_POINT` (gate `NOVI_BlockingGate`). WLD `Maps[0]` Lookout + `MAIN_START_POSITION` (102.9, 74.1) is the adult overworld first map. `CTCDRegionExit` persist (`0077947D`) stores `Active`, `Radius`, `MessageRadius`, `EntranceConnectedToUID`. Walk XY into `Radius` of an `Active` exit, then place the hero at the dest `REGION_ENTRANCE_POINT`. | `WorldSceneTests`, exe `StartOakVale` / `CTCDRegionExit` |
| Hero age stages | Three listed stages, two meshes. **Kid:** `CREATURE_HERO_CHILD` / `CREATURE_YOUNG_HERO` → graphics.big **4300** `MESH_YOUNGHERO_02`; bones `hero_young_set.bncfg` (`CREATURE_HERO_CHILD_02`); quests `Q_NewOakValeIntro` / `_PreAttack`. **Tween (Guild):** `CREATURE_HERO_TRAINING` → **4299** `MESH_HERO` (same as adult) + `CHeroMorphDef` persist `Teenager` (`0071D102`) + `hero_teen_set.bncfg`; quests `Q_GuildTraining*`; HSPs `GuildArrivalHSP` (Lookout 52.7, 69.6) then `GuildTrainingHSP` (HeroGuildComplexInside 57.6, 126.0). **Adult:** `CREATURE_HERO` → **4299** `MESH_HERO`; WLD initial `Q_SunnyvaleMaster`; Lookout graph + adult Oakvale `OakValeWest_v2` / `OakValeEast_v2`. Morph persist also `Strength` / `Will` / `Skill` / `Morality` / `Fatness` (`0071D037`). No third creature id. | `WorldSceneTests`, `GameBinFormatTests`, `DataCatalogTests`, exe `CHeroMorphDef` |
| BWD display names | After the 398 AABB records: ~94 overworld blocks of 4 strings (script, `TXT_REGION_*`, `REGION_*`, `MINIMAP_*`). **72** of those script names are WLD maps (`MapUIDCount`). Extra starts `01 01 01` + `f32` scale (Lookout 1.0) + four `i32`s; the last two are minimap XY (Picnic X < Lookout X). Then length-prefixed neighbour names (Lookout lists PicnicArea and HeroGuildComplexInside). | `WorldSceneTests` |
| GTG kick points | `REGION_KICK_TO_POINT.ScriptData` is a region name (`BowerstonePosh` / `BowerstoneSlums`). | `WorldSceneTests` |
| GlobalQuests.qst | Same `AddQuest` text as the master table. Includes `Global_WatchForHeroDeath`. | `WorldSceneTests` |
| FinalAlbion.gtg | Version-2 thing text (`NEWMAP 1`), not a .lev. Parses with the TNG reader. Global `REGION_ENTRANCE_POINT` / `HOLY_SITE_PLAYER_START`. | `DataCatalogTests` |
| .bncfg | Text bone morphs. `Creature_type: CREATURE_HERO` / `CREATURE_BS_VILLAGER_MALE`, then `Bip01 *` XYZ scales and named bone groups. 60 files in `data\Bones`. | `DataCatalogTests` |
| text.big | BIGB bank `TEXT_ENGLISH_MAIN`, 28913 UTF-16 LE strings. Id 1 is flourish on-screen help. | `DataCatalogTests` |
| Other BIGB | `fonts.big`, `dialogue.big` (lipsync), `frontend.big`, `effects.big` (particles), `shaders.big` (pixel/vertex programs). Same BIGB footer as graphics/textures. | `DataCatalogTests` |
| Sound .lug / Dialogue.lut | Lionhead audio, **not** BIGB. Magic `LiOnHeAd` + `LHFileSegmentBankInfo` / `LHAudioBankCompData`. `.met` is a sidecar (u32=1 then source WAV path). | `DataCatalogTests` |
| stars.dat | `u32` count **1330**, then 24-byte records (6 floats). | `DataCatalogTests` |
| Fable.exe world load | RTTI: `CWorld` / `CWorldMap` / `CLevelLoader` / `CEngineLandscapeMap` / `CEngineLandscapePatch` / `CLandscapeBackgroundPatch` / `CLandscapePatchTesselator` / `CPatchTesselationEdgeStrip` / `CStaticMapBankFile` / `CHeightMap` / `CEngineWaterRenderer`. `SetStaticMapFileForUse` logs **`OpenStaticMaps`** (plural), `OpenStaticMap`, `LoadWaterData`, `CloseStaticMapFile`. Region files then `Activate Topology` / `Post Load Initialise`. WLD tokens `NewMap` / `MapX` / `LoadedOnPlayerProximity TRUE;` are parsed as written. | exe string dump 2026-08-16 |
| AABB static maps | `OpenStaticMaps` + `CLandscapeBackgroundPatch` load **BWD rectangles that touch**, not the starting-region teleport graph. Lookout (3232,3488–3360,3616) touches Picnic (−128,+32), Bridge (0,+128), GuildExterior (+128,−32), Greatwood_1 (+32,−192), Greatwood_2 (−64,−192), plus two Picnic fillers with no STB `.lev`. Those five real maps are `LoadedOnPlayerProximity TRUE`; fillers are FALSE. Graph names (`BowerstoneSlums`, `GreatwoodEntrance`) are exits, not tiles. | `WorldSceneTests`, `WorldGeometryTests` |
| Active vs nearby map | Exe `OpenStaticMaps` `00B42750` has two modes (`cmp arg, 2`): mode 2 calls `OpenStaticMap` with `push 2`; `SetStaticMapFileForUse` uses mode 1 (`push 1`) then `OpenStaticMap` with that flag. `CEngineLandscapePatch` (vtbl `0x012A8200`, ctor writes at `00BF3A16`) is the current map; `CLandscapeBackgroundPatch` (vtbl `0x012A803C`, ctor `00BE6090`) is the neighbour. `Activate Topology` `004FCBB0` sets a 72-byte map-record flag at `+38` to 1 (current only). Nearby GuildExterior TNG instances `BUILDING_GUILD_LO_POLY_01` (mesh 5949). The high-poly `BUILDING_GUILD_EXTERIOR_01/02/05/06` live in `HeroGuildComplexInside.tng` (MapX 4576, not AABB-adjacent to Lookout). | `WorldSceneTests`, exe `00B42750` / `004FCBB0` |
| Neighbour scene | Lookout world mesh is local metres **plus** those five maps, offset by `MapX/MapY` delta. Scene spans X&lt;0 and X&gt;128, Y&lt;0 and Y&gt;128. Fillers stay out (STB `FindLev` skips `Filler` / `Demon`). Instances &gt; 192. | `WorldGeometryTests.Lookout_scene_opens_aabb_adjacent_static_maps` |
| Fable.exe world draw | `CRenderManager` owns `CEngineSkyRenderer` → `CEngineLandscapeRenderer` / `CLandscapeBackgroundPatch` → `CEnginePrimitiveManagerStaticMeshes` (`VSHADER_STATIC_DIRLIGHT_FOG`) → `CEngineWaterRenderer` → weather / local detail / particles / HUD. State `CEngineStateBlockDiffuse2X`. Lighting: `CEngineLightingManager` + `CShaderDirectionalLight` + `CLightDef` (47, BGRA colour + Inner/OuterRadius). Sky: `CSkyDef` `SKY_DEF`. | exe RTTI 2026-08-16, `ShaderFormatTests` |
| SKY_DEF | game.bin `SKY` / `SKY_DEF`. CRC fields: `SunTexture`=384 `GRAPHIC_ATMOSPHERIC_SUN`, `StarTexture`=401 `GRAPHIC_ATMOSPHERIC_STAR_01`, then flare `Texture` 393–399 (`LENSFLARE_01`–`07`) with `Radius` 500–6000 and `Position` −1.4..1. Time-of-day skies are `GRAPHIC_ATMOSPHERIC_SKY_MIDDAY` (391) / morning / evening / midnight. No sky C3D in graphics.big — `CEngineSkyRenderer` builds the dome. | `DataCatalogTests` |
| Scene layers | Draws sorted `SceneLayer.Sky` (391 + 401), then Landscape, then Prop. Far plane 7000 (SKY_DEF max flare radius 6000). VS fog is `oFog = min(dot(pos,c2),1)*c18.w+1` — c2/c18 unread, so we do not invent start/end. | `WorldGeometryTests` |
| shaders.big | 26 banks, **465** programs. Payload = `u32` size + D3D tokens. **353** `vs_1_1`, **101** `ps_1_1`, **11** `ps_1_4`. `PIXEL_SHADERS` entries are type 1; vertex banks are type 0. | `ShaderFormatTests` |
| Landscape passes | `SHADERS_LANDSCAPE_FOREGROUND` (33 VS) + `PSHADER_LANDSCAPE_FOREGROUND` (**2** `tex` stages). Background is 1 tex. Proc-texture pass is 2 tex (`VSHADER_LANDSCAPE_PROC_TEXTURE`). Shadow / bump / colbuff variants add more stages. Objects use `PSHADER_TEXTURE_DIFFUSE_FOG` (1 tex). | `ShaderFormatTests` |
| Cell blend slots | Lookout 128² cells always write M0+M1+M2. M3 is `0xFF` on 16139/16384. `PSHADER_LANDSCAPE_FOREGROUND` is `tex t0; tex t1; mul_x2_sat r0.xyz, t1, v0; mul_sat r0.w, t0.a, v0.a` (dest shift 1 = ×2). `PSHADER_TEXTURE_DIFFUSE_FOG` is a plain `mul` (no ×2). Not a lerp. `INVALID_THEME_STANDIN` is skipped. Two real layers bind as `TextureId`/`TextureId1`. | `ShaderFormatTests`, `WorldGeometryTests` |
| Tile extras | After XYZ: **11-11-10** packed normal + 3 bytes. Lookout: 289/289 unit Z-up; byte0 of the triple is always `0xFF`. `VSHADER_LANDSCAPE_FOREGROUND` does `mov oD0.xyz, r3` where r3 is N·L × `c20`/`c35` + ambient `c3` — **not** those 3 bytes. Byte0 scales `oD0.w` (`mul oD0.w, r4, v3.x`). Multiplying the albedo by (1, ~0.5, ~0.5) made Lookout magenta. Drawn verts use white × lighting. | `LevFormatTests`, `WorldGeometryTests` |
| Dirlight + fog | `PSHADER_LANDSCAPE_FOREGROUND` is `mul_x2_sat r0.xyz, t1, v0` with v0 = VS lighting (not tile RGB). `PSHADER_TEXTURE_DIFFUSE_FOG` is `mul` then `mul_x2` with t0. Sand DXT mean is tan (102,93,74), grass olive (62,62,25). | `ShaderFormatTests`, `TextureFormatTests` |
| Sky | `stars.dat` 1330 points on a ~6500-unit sphere (xyz, unused 0, size, brightness). Drawn with an inner dome (no bank sky mesh). `CEngineLocalDetailGenerator` / water patch tessellators still unread. | `DataCatalogTests`, `WorldGeometryTests` |

## Did not work

| Guess | Why it failed | Locked by |
|---|---|---|
| WAD `.lev` is the landscape mesh | Parses as a 255-slot theme table. `MeshFile.TryParse` returns null. | `LevFormatTests`, dump `lev` |
| Framed LZO at WAD `.lev` payload is a dense f32 height grid | Almost no values in the TNG Z range. | `LevFormatTests.Lookout_payload_lzo_does_not_decode_as_dense_f32_grid` |
| `FindMeshId("OBJECT_WALL_SMALL_POST_01")` | No `MESH_WALL_SMALL_POST_01` in `meshdata.h`. Related names exist (`MESH_BS_WALL_SMALL_POST_02`) but that is the wrong object. | `MeshFormatTests` |
| `graphics.big` name contains `BRIGHTWOOD_MEDIUMROCK` | Zero hits. Rocks are not named after the TNG def. | probe 2026-03-16 |
| `game.bin` stores ASCII `OBJECT_*` / `#definition` | 996 KB, no those strings. Names live in names.bin. | `GameBinFormatTests` |
| `game.bin` is a table of `names.bin` hashes | Name-refs are **string offsets**, not CRCs. The wall hash never appears as a ref. | `GameBinFormatTests` |
| `game.bin` is Fable-framed LZO | Consumes ~8 KB, almost no ASCII. Chunks are **zlib**, not LZO. | `GameBinFormatTests` |
| Follow every OBJECT sub-def for a mesh | `MARKER_BASIC` / `CAMERA_POINT_*` have a `CAppearanceDef` editor mesh (4511/4512). Those are not world props. Only the object's own `Graphic` or a `CReplaceableMeshDef` is used. | `GameBinFormatTests` |
| WAD 21-byte `u16` at +8 is height | Every Lookout/Picnic cell stores **60**. It is a constant, not Z. | `LevFormatTests` |
| Material slot `u32` is a textures.big id | Lookout sand slot id 1911 decodes as `TEXTURE_BOWER_FEMALE_MIDDLE_LEGS_03C`. | `LevFormatTests` |
| STB section 2 is a packed xyz stream | `stride=12` from the section header does not yield a dense height point cloud. It is a raw-LZO tile, same as the table payloads. | `LevFormatTests` |
| Header `u16@+4` is the index count | It is D3D `PrimitiveCount` (`IndexCount - 2`). The two u16s after it are the last strip verts (valid, area ≈ 1), not the next object's attach pair. | `LevFormatTests` |
| Compressed tile bytes are a 17×17 f32 grid | Without LZO, almost no values sit in the TNG Z range. | `LevFormatTests.Compressed_tile_payload_is_not_a_17_by_17_float_grid` |
| 15-byte verts on the packed tile | Z floats appear at ~15-byte gaps, but XY is not packed local. The stream is LZO; verts are world `u16` after inflate. | probe 2026-08-16 |
| Every Lookout `DiffuseMapID` framed-LZO decodes | Some bank ids overrun the LZO reader. `TryLoad` returns null; those draws use a 1×1 fallback. | `GpuTextureTests` |
| STB `.lev` is the same format as WAD `.lev` | STB blob starts `u32=1`, not version 25 / `0x1904`. | `LevFormatTests` |
| Extra `Matrix4x4.Transpose` on VP | Double transpose. Screen was solid clear-color. | `CameraProjectionTests` |
| `CreateWorld(pos, RHSetForward, RHSetUp)` | Numerics CreateWorld is Y-up and **negates** forward. Lamp mesh Z (height) mapped to world Y — props lay on their side and faced backward. | `WorldGeometryTests` |
| TNG positions are WLD (MapX+local) | Lookout things sit in 29–128, not 3232+. Adding MapX would throw them off the terrain. | `WorldGeometryTests` |
| Ignore `ObjectScale` | Rocks/pillars store 0.4–1.2. Without it they instance at 100% mesh size. | `WorldGeometryTests` |
| Draw STB tiles as a dense 1-unit grid | Adaptive tiles have ~159–280 verts, not 289. Bilinear-filling the rest raised hills through objects (a Lookout rock sat 3.3 m under stamped Z). | `LevFormatTests` |
| Tile leftover is a triangle *list* | Bytes `0,0,1,2,2,0,0,3…` are a **strip** with degenerate restarts. Reading groups of 3 dropped every other half-quad (flat ground looked like isolated triangles). | `LevFormatTests` |
| Format 35 is DXT1 | First LZO frame is 262144 bytes (16-byte blocks). DXT1 top mip is 131072. | `TextureFormatTests` |
| Format 35 is DXT3 | First 8 bytes `FFFF000000000000` are DXT5 `a0=a1=255`. As DXT3, 12/16 texels get alpha 0. Combined with a `discard` the sky became horizontal stripes. Exe `PSHADER_INNER_SKY` has no `texkill`. | `TextureFormatTests` |
| WAD `Find("Lookout")` for a `.tng` | Stem match hit `LookoutPoint.lev`. Must pass the extension. | `TlcInstallTests` |
| `FinalAlbion.gtg` is a compiled .lev / C3D | ASCII `NEWMAP 1` / `Version 2;`. First `u32` is not 25. | `DataCatalogTests` |
| PicnicArea.lug / Dialogue.lut are BIGB | Magic is `LiOnHeAd`, not `BIGB`. | `DataCatalogTests` |
| BWD declared count is the parsed region count | File says 399; 398 records match WLD. Trailing ~22 KB is not another region. | `DataCatalogTests` |
| WLD `MapUIDCount 72` is the number of maps | The file then has **398** `NewMap` blocks. 72 is some other counter. | `WorldSceneTests` |
| `EntranceConnectedToUID` is a raw thing UID | The u64 is not an instance id. High bits are MapUID; low 32 bits equal the dest entrance's low 32 bits (`0xFFFFFE00…`). | `WorldSceneTests` |
| Lookout STB 64/64 means the overworld is complete | Every Lookout tile is present (~30039 tris, 192 props). The missing ground west/east/north/south is **other WLD maps**. Teleport-graph neighbours are not those tiles. | `WorldGeometryTests`, exe `OpenStaticMaps` |
| Starting-region graph is the visual neighbourhood | Lookout graph lists Picnic, BowerstoneSlums, GreatwoodEntrance, guild interior, demon door. Only Picnic shares an AABB edge. Slums / GreatwoodEntrance / interiors do not touch Lookout's rectangle. | `WorldSceneTests` |

## Open

1. **Full game.bin field tables.** We read `Graphic.bank_index` and `CReplaceableMeshDef`. Other controls (health, physics, inventory, quests) are still raw bytes.
2. **Creature clothing / appearance layers.** Villager Graphic is the unclothed body (`MESH_BS_MALE_MIDDLE_UNCLOTHED_01`). `CAppearanceDef` / morphs are unread.
3. **Streetlamp lit vs off.** TNG `OBJECT_STREETLAMP_LIT_SINGLE_01` maps to `MESH_OBJECT_STREETLAMP_OFF_02`. Lit state is probably a replaceable / particle, not a second mesh id.
4. **Tile extra-object formats / 7-byte extras.** Edge-strip `fmt` values (`0x5901`–`0x5904`, `0x5801`–`0x5804`, `0x7900`, …) are unread as side/LOD flags. The 7 bytes after Z are still packed 11-11-10 + RGB.
5. **GPU texturing leftovers.** Two albedo samplers, no atlas, no mipmaps, no bump/reflection/illumination maps, no DXT-on-GPU. Terrain UVs still tile every 16 world units. No shadow-buffer / colbuff variants.
14. **CPatchTesselationEdgeStrip fmt.** Extra objects parse as self-contained PrimitiveCount+2 strips. What `0x5901`–`0x5904` / `0x5801`–`0x5804` select is unread.
15. **LoadWaterData / CEngineWaterRenderer.** `SetStaticMapFileForUse` always loads water after opening the static maps. Sea / river / ice shaders (`VSHADER_WATER_FOREGROUND`, `VSHADER_SEA_BACKGROUND`) are unread.
16. **Picnic fillers.** `PicnicArea_Filler_02/03` touch Lookout but have no STB `.lev` (`LoadedOnPlayerProximity FALSE`). Likely `CLandscapeBackgroundPatch` only; WAD cells unread.
17. **CEngineLightingManager register maps.** Scene pass calls `00B46C80`/`00B46890` on `0x1436E9C` then caches a matrix at device-wrapper +496 (`009881F0`). `PSCONST_MAX_FOG_ALPHA` default is (0.5)×4; `PSCONST_TFACTOR` is (1)×4. **Which VS constant (`c20`/`c35`/`c3`/`c2`/`c18`) they become is still UNREAD.**
18. **Sky mesh / weather / local detail.** Inner/outer sky VS exist but no sky C3D is in the banks; we draw a dome. Rain/snow/mist and `CEngineLocalDetailGenerator` (`REPEATED_MESH`) are not drawn.
19. **C3DMeshLODInfo / boolean-alpha / two-sided.** RTTI `C3DMeshLODInfo` (`00A23DE0` reads a `cmp eax, 1` flag). `CStateBlockFunctionAlphaBoolean` / `CEngineStateBlockAlpha` write device slots at +10524/+10544, not a raw `D3DRS_CULLMODE`. Material `DegenerateTriangles` (diffuse 0) is stored on guild/tree C3Ds. Which LOD and which state block a nearby `LO_POLY` building uses is unread.
19. **Water patches.** Landscape `WATER_*` cells use the second texture stage. `CEngineWaterRenderer` / `LoadWaterData` sea-ice tessellation is unread.
6. **WAD cell bytes 4–7 / 14–20.** High-entropy field and flags after the material slots are unread.
7. **Animation / bones / cloth.** Parser skips the blocks so static positions survive. No skinning.
8. **Hero, combat, quests, UI, audio.** Frontend UI defs and cutscene defs parse as GameBin entries; fields inside are unread. `.lug`/`.lut` payloads, `.ogg`/`.wmv`, tattoos, and `stars.dat` channels are unread.
9. **BWD second blob.** After the 398 region records (~22 KB). The 9-byte trailer is now MapUID plus 5 unread bytes.
11. **BWD leftover header.** `u32=142` plus ~16 integers before the first display name. Graph names such as `GreatwoodEntrance` are not always a WLD `LevelScriptName`.
13. **BWD extra integer lists.** After the named neighbours, remaining u32s (edge indices?) are unread.
12. **Ten exits pack a MapUID that is not in WLD.** Dest file missing; slot rule still holds when the map exists.
10. **text.big after the first UTF-16 string.** Some entries append a `.lug` name / extra binary.

## Exe load / render pathway

Traced in `Fable.exe` with `tools/Fable.ExeIndex` (`disasm` / `calls` / `trace-render`). Do not invent steps.

1. **`CTextureManager`** reads the 34-byte BIG info. `u16` at +12 is the format code (`31` / `32` / `35`).
2. **Framed LZO** inflates the bank payload. Format 35’s first frame is **262144** bytes (`512×512×16`) — one DXT3/DXT5 top mip — not DXT1’s 131072. Last 3 bytes of the frame are the raw tail (`DecompressFramed`).
3. **CreateTexture helpers** at VA `009BE80C` (push `DXT1`), `009BE87C` (push `DXT3`), `00416D20` (push `DXT5`) call `IDirect3DDevice9` vtable `+40` with those FourCCs. Format **35 skies are 16-byte blocks**; first 8 bytes `FF FF 00 00 00 00 00 00` are DXT5 alpha (`a0=a1=255`, opaque), not DXT3 nibbles. Reading them as DXT3 makes 75% of texels `alpha=0`.
4. **`CEngineSkyRenderer`** / `PSHADER_INNER_SKY`: `tex t0..t3` then `lrp` — **no `texkill`**. Discarding `t1.a < 0.08` punched the DXT3-misread alpha into horizontal stripes. `VSHADER_INNER_SKY` transforms `v0` by `c5–c8` and copies UVs from `v2`.
5. **Shader manager** ctor `00B3CB30` (global `0x1436E98`, size `0x31E0`). `PIXEL_SHADERS` is bound through a vtable on `this+10392`; the resulting manager pointer is stored at **`this+10904`**. Every `PSHADER_*` lookup is `00A5D720` on `[0x1436E98]+10904`. Vertex lookups are `00A5D5F0` on the same object: sky inner/outer use **`+10728`**, star field **`+10816`**, landscape foreground **`+10784`**.
6. **Shader bank slots** (`00B3B5D0`, push the integer then the name). Locked against `shaders.big` by `ShaderFormatTests.World_passes_are_landscape_static_water_and_sky`:

   | slot | bank |
   |---|---|
   | 5 | `SHADERS_SKY` |
   | 6 | `SHADERS_SKY_SCREEN_SPACE` |
   | 7 | `SHADERS_WATER_FOREGROUND` |
   | 8 | `SHADERS_WATER_BACKGROUND` |
   | 9 | `SHADERS_SEA_BACKGROUND` |
   | 10 | `SHADERS_WEATHER` |
   | 11 | `SHADERS_LANDSCAPE_BACKGROUND` |
   | 12 | `SHADERS_LANDSCAPE_FOREGROUND` |
   | 13 | `SHADERS_POS_COL_TEX1` |
   | 14 | `SHADERS_REPEATED_MESH` |
   | 16 | `SHADERS_POINT_SPRITE1` |
   | 17 | `SHADERS_ZSPRITE` |
   | 20 | `SHADERS_VERTEX_POS` |
   | 21 | `SHADER_SPRITE_GROUP` |
   | 22 | `SHADERS_DECAL_GROUP` |
   | 23 | `SHADERS_MESH_GROUP` |
   | 24 | `SHADERS_PARTICLE_SPRITE_TRAIL` |
   | 25 | `SHADERS_DEBUGGING` |
   | 26 | `SHADERS_TEXT` |

   A second registrar `00B3B6D0` takes `SHADERS_STATIC` (ebx), `SHADERS_STATIC_BUMP` (2), `SHADERS_PALSKIN` (3), `SHADERS_PALSKIN_BUMP` (4). ebx at that site is UNREAD.
7. **PS constants** interned just before the bank table: `PSCONST_MAX_FOG_ALPHA` default `(0.5,0.5,0.5,0.5)`, `PSCONST_TFACTOR` default `(1,1,1,1)`. Also named: `PSCONST_ZERO` / `ONE` / `HALF` / `1_0_0_0` / `0_1_0_0` / `0_0_1_0` / `0_0_0_1` / `PSCONST_SHADOW_FADE_COLOUR` plus `PSCONST_USER_0..3`, `PSCONST_OUTPUT_FACTOR`, `PSCONST_INPUT_FACTOR_0/1`.
8. **Engine components** (`Engine: Add Engine Component` @ `00B29930` appends to `this+360`). Construction order in the engine ctor includes:

   | global | ctor VA | what the listing shows |
   |---|---|---|
   | `0x1436E98` | `00B3CB30` | shader manager |
   | `0x1436E80` | `00B33B50` | intern `"MainScene"` (also builds `"RepMeshScene"`) |
   | `0x1436E60` | `00B50370` | lives next to `"EnableShadows"` |
   | `0x1436E8C` | `00B423F0` | `SetStaticMapFileForUse` |
   | `0x1436E54` | `00B73760` | water (`PSHADER_WATER_*` binds) |
   | `0x1436EA8` | `00B69000` | landscape (`PSHADER_LANDSCAPE_*` binds; pool alloc uses `[0x1436EA8]+1712`) |
   | `0x1436E50` | `00B625E0` | **`CEngineSkyRenderer`** (only caller of that ctor) |
   | `0x1436E44` | `00B52250` | weather (`EnableWeather` / `PSHADER_WEATHER_*`) |
   | `0x1436E40` | `00B5A1D0` | screen colour filter |
   | `0x1436E34` | `00B5F090` | glow |
   | `0x1436E30` | `00B5C460` | radial blur |
   | `0x1436E38` | `00B86C00` | displacement |

9. **34 render layers** (`Engine: Add Render Layer` @ `00B262C0`, 34 calls from `00B26A75`–`00B276A8`). Each layer is 28 bytes, vtable `0x12A0F04`, **bit mask at +4**, flags at +8/+12, renderer vector at **+16 / +20**. Manager vector is a normal MSVC `vector<>`: **`+348` begin, `+352` end, `+356` cap**. Attach is `00B2AC80` (push onto layer+16). Bits and attachments in registration order — and the frame walks **begin → end**, so this **is** submit order:

   | +4 bit | attached global |
   |---|---|
   | `1` | none in the listing |
   | `2` | `0x1436E60` shadows |
   | `4` | `0x1436EA8` landscape |
   | `8` | `[MainScene]+616` |
   | `0x10` | `[MainScene]+616` |
   | `0x40` | landscape |
   | `0x20` | `[MainScene]+616` |
   | `0x100` / `0x400` / `0x1000` | `[MainScene]+616` |
   | `0x2000` | **sky** `0x1436E50` |
   | `0x4000` / `0x8000` | `[MainScene]+616` |
   | `0x20000` | **water** `0x1436E54` |
   | `0x10000` | displacement `0x1436E38` |
   | `0x400000` | sky again |
   | `0x2000000` | landscape again |
   | `0x4000000` | `0x1436E3C` |
   | `0x1000000` | colour filter `0x1436E40` |
   | `0x20000000` | weather + glow + radial + displacement |
   | `0x40000000` | shader manager `0x1436E98` |
   | `0x80000000` | `0x1436E7C` |

   `[MainScene]+616` is the subobject constructed in `00B33B50` (vtable `0x12A1348`), not the sky renderer.

## Full scene pass (exe)

Traced `00B27D90` → `00B25950` → layer `00B2AB80`. Do not invent VS register numbers.

1. **Frame** `00B27D90` (only vtable site `0x012A0F5C`). Device wrapper `0x1436E18`: unbind textures (`[dev+260]` = `SetTexture(stage, 0)`), optional `00B277A0` (if flags at manager +24/+36: `00B3B4A0` on the shader manager, `00B2F740` on `0x1436E58`), then **`00B25950(this, arg)`**.
2. **`00B25950` three phases** (this in ebx). Builds a stack ctx: copies 28 dwords from `arg`, stores `this+184` at ctx+120, `this+136` and `this+248` nearby. Then:

   | phase | walk | call |
   |---|---|---|
   | 1 | components `+360…+364` | if `(this+184 & query(+40))` → **`vtbl+4`** |
   | 2 | **layers `+348…+352`** | **every layer `vtbl+4`** (`00B2AB80`) — no extra mask here |
   | 3 | components `+360…+364` | if `(this+184 & query(+40))` → **`vtbl+8`** |

3. **Layer submit `00B2AB80`** (layer this, ctx). Bail if `(ctx+120 & layer+12) == 0`. `layer+12` is 0 or 1 on construction; ctx+120 is **`manager+184`**. Then three loops over attached renderers (`layer+16…+20`):

   | loop | condition | call |
   |---|---|---|
   | prepare | `ctx+120 & renderer->query(+40)` | **`vtbl+20`** (sky/landscape/water = `ret 4` stub `00B28C60`) |
   | **draw** | same **and** `renderer+8 != 0` | **`vtbl+16`** |
   | after | same query | **`vtbl+24`** (stub `00B28C70`) |

   Base ctor `00B59710` sets **`[this+8] = 1`**, so +16 runs for sky / landscape / water / MainScene+616.
   Query (`+40`) returns **`1`** for sky (`00B66DE0`), landscape (`00B6CA10`), water (`00B7ED70`).

4. **`vtbl+16` switches on the layer bit (`arg+4`)**:

   | Renderer | +16 VA | bits handled |
   |---|---|---|
   | Landscape `0x12A2B54` | `00B6B0B0` | **`4`**: `00B67480`, `00B671A0`, walk static-map list `00BDC060`, `00B67510`. **`0x40`**: `00B68DA0`, `00B67480`, `00B677D0`, `0098B5E0(2)`, walk list `00BDC2D0`. Other bits: profiler only. |
   | Sky `0x12A293C` | `00B662F0` | **`0x400000`** → `00B64550`. Else uses `0x1436EA0+20` and continues (outer/inner/stars). |
   | Water `0x12A3364` | `00B783F0` | Reads water vectors at +508/+512/+520/+524. Body UNREAD (decoder). |
   | `[MainScene]+616` `0x12A1348` | `00B33010` | `lea ebx, [this-616]` then switch on the bit: **`2`** uses shadow global `0x1436E60`; **`0x20`** → `00B32610`; compares `0x400` / `0x80`. |

5. **Landscape shared setup `00B67480`** (both bit 4 and 0x40): `00B46C80` + `00B46890` on **`0x1436E9C`** (the 0x46D0 object from ctor `00B482A0`), then `009881F0(0x1436E14, matrix)` with **1.0f at +4, +20, +36** (identity-like 4×4 cached on the device wrapper at +496…+556, last `w=1`). This is **not** yet a D3D `SetVertexShaderConstant` index — the wrapper stores the matrix; the c20/c35/c3 slot map is still UNREAD.
6. **Patch submit** `00BDC2D0` / `00BDC060`: require `[patch+8]` and `[patch+4]`, touch `0x1436EA0+0x1C8`, then x87 (`fld`). Exact DrawIndexed path UNREAD.
7. **Pre-pass `00B277A0`**: `00B3B4A0(shader manager)` releases caches at +2288/+2296/+2304 and calls `00BD9B50([0x1436EA8]+1712)` — same pool object as `EnablePoolAllocation`.
8. **`CEngineStateBlockDiffuse2X` apply** — still RTTI only. `0098B5E0(n)` is a device-wrapper call used with **2** (landscape 0x40) and **3** (earlier in `00B25882`).
9. **Client walk.** `ScenePasses.Registration` is the 34-layer table. We submit landscape on bits **4** (1-tex `PSHADER_LANDSCAPE_BACKGROUND` shape) and **0x40** (proven `mul_x2` FG), static meshes once on **0x20** (first MainScene+616 bit after FG; other +616 bits unread), sky else-path on **0x2000**. Water / shadows / `0x400000` sky stay out. Locked by `ScenePassTests`.

10. **Per-renderer shader stores**
    - Sky: `PSHADER_INNER_SKY` / `_SIMPLE` → `this+292`; `PSHADER_OUTER_SKY` → `+300`; `PSHADER_SKY_STAR_FIELD` → `+260`; `VSHADER_OUTER_SKY` → `+244`; `VSHADER_SKY_STAR_FIELD` → `+252`.
    - Landscape: `PSHADER_LANDSCAPE_FOREGROUND` → `this+1388`. VS family is `VSHADER_LANDSCAPE_FOREGROUND` plus `_2LIGHTS` / `_4LIGHTS` / `_5LIGHTS` × bump / spot / shadow / colbuff, and `_BLACKOUT_PASS`.
    - Water: `PSHADER_WATER_SKY_MAP` → `+452`; `PSHADER_WATER_ENVIRONMENT_MAP` → `+436`.
11. **`EnableSky` / `EnableLandscape` / `EnableWater` / `EnableWeather` / `EnableShadows` / `EnablePrimitives`** are `ret 4` name interners, not the draw path.
12. **State blocks** exist as RTTI only in this dump: `CEngineStateBlockSolid`, `Solid2X`, `Alpha`, `Alpha2X`, `AdditiveAlpha`, `AddSmoothAlpha`, `Alpha2XTFactor`, `DiffuseOnly`, **`Diffuse2X`**, `DiffuseEnv2X`.
13. **`CEngineLandscapeMeshBuilder` / `CPatchTesselationEdgeStrip`**: 17×17 (`v=289`, flag 256) is a full grid; other tiles use PrimitiveCount+2 strip indices plus stored edge-strip objects.

## Exe dump tool

`tools/Fable.ExeIndex` indexes a local `Fable.exe` (`index` → `split` → `translate` packets). Dumps land in `tools/Fable.ExeIndex/out/` and **stay gitignored** (root + tool `.gitignore`). Do not commit strings, disasm, or the AI packets. After a dump, translate each `out/02-translate/*.prompt.md` into `out/03-pseudo/*.md`.

## How to add a note

- Prove a positive with a test against the live install.
- Prove a negative the same way (`Assert.True(inRange < threshold, "unexpectedly …")`).
- One sentence in this file pointing at the test.
- Do not check in game bytes.
