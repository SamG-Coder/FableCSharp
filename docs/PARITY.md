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
| textures.big | 34-byte info: `u16` w/h at 0/2, frame w/h at 6/8, format at 12. Format 31 = DXT1, 32 = DXT5, type 4 / format 1 = RGBA8. Payload is framed LZO. | `TextureFormatTests` |
| DiffuseMapID | Mesh material `DiffuseMapID` **is** the `textures.big` entry id. 3880 = oak trunk, 2119 = apple leaves, 414 = `LANDSCAPE_GRASS_PLAIN`. | `MeshFormatTests` |
| WAD `.lev` | Version 25, constant `0x1904`. 255×132 material table from 179 (`INVALID_THEME_STANDIN`, then `GROUND_*`). Sound themes after 67639. Payload starts with `21`. This file is a material/theme table, **not** a C3D mesh. | `LevFormatTests` |
| STB coarse height | Runtime `FinalAlbion_RT.stb` copy of `.lev` is ~3 MB. Bytes 0–2047 are pad (`u32[0]=1`). From 2056, 36-byte records hold two WLD-space verts on a **16-unit** lattice. Lookout is 8×8 quads, Picnic 8×6. Heights 20–80 match TNG Z. | `LevFormatTests` |
| STB section 2 offset | `u32` at 2048 is the start of the next blob (Lookout/OakVale **6144**, Picnic **4096**). Zeros fill the gap after the coarse lattice. | `LevFormatTests` |
| WAD cell table | Payload after the `21` tag is a stream of **21-byte** records, one per 1-unit cell (Lookout 16384). Bytes 10–13 are material-table slots (`0xFF` unused). Lookout is mostly slot 2 `GROUND_PATH_SAND`. | `LevFormatTests` |
| GROUND_ → LANDSCAPE_ | Material-slot `u32` id is **not** a `textures.big` id (1911 is villager legs). Names map: `GROUND_PATH_SAND` → `LANDSCAPE_PATH_SAND_01` (4133), `GROUND_GRASS` → `LANDSCAPE_GRASS_PLAIN` (414), `PATH_COBBLES_IRREGULAR_ET` → `LANDSCAPE_COBBLES_IRREGULAR_01` (4118). | `LevFormatTests` |
| Fine terrain mesh | 1-unit quads (Lookout 128×128×2 tris). Z is bilinear from the 16-unit lattice, then STB tile verts overwrite cells they hit. Each cell samples its `GROUND_*` texture. | `LevFormatTests` |
| GPU texturing | Mesh vertex is pos/normal/UV. Draws are grouped by `textures.big` id. RGBA is uploaded as `R8G8B8A8` with a repeat/linear sampler. Fragment shader samples `albedo`. Some bank ids fail framed LZO and fall back to a 1×1. | `GpuTextureTests` |
| STB tile table | After the coarse 36-byte lattice, leftover `magic/offset/size` is a tile directory (`0x012EC900`, file offset, packed size). Lookout 63 tiles, Picnic 47. Last record is a 0,0 sentinel. | `LevFormatTests` |
| STB tile payload | `u32` uncompressed size, `u32` packed size, then **raw LZO** (not framed). Inflated header `u16` at +2 is vertex count (289 = 17×17). | `LevFormatTests` |
| STB tile verts | 32-byte header, then `count` × **15-byte** records: `u16` world X, `u16` world Y, `f32` Z, 7 unread bytes (packed normal / colour). Mesh origin is the coarse cell's **east** edge. | `LevFormatTests` |
| Camera | System.Numerics row-major is uploaded as-is (no extra transpose). Z-up. Overview `(64,-40,95)` looks at `(64,64,36)`. | `CameraProjectionTests` |
| TNG → mesh (partial) | `meshdata.h` name match: `DefinitionType`, `MESH_` + type, or `MESH_` + stem after `OBJECT_` / `CREATURE_` / `BUILDING_`. Skip `[PHYSICS]`. Still useful as a fallback. | `MeshFormatTests` |
| names.bin offsets | Each name is `(u32 CRC, cstring)`. The game.bin name-ref is the string's offset after the 20-byte header (first string is 4). CRC is Fable polynomial `0xEDB88320` init 0. | `GameBinFormatTests` |
| game.bin container | 13-byte header: `u8 use_names_bin` (TLC = 0), `u32 file`, `u32 platform`, `u32 entry_count` (14761). Then `NameRef` × N (`type_off`, `file_off`, `counter`). Then chunk index + **zlib level 1** chunks (`78 01`). | `GameBinFormatTests` |
| game.bin OBJECT → mesh | Instance name is the `file_off` names.bin string (`OBJECT_WALL_SMALL_POST_01`). Body starts with a 3-byte preamble, then a sub-def table, then control fields. Field CRC `"Graphic"` is followed by `EngineGraphic` (`type i32`, **`bank_index i32`** = `graphics.big` mesh id). | `GameBinFormatTests` |
| Lookout TNG → mesh | Walls 5331 `MESH_SMALL_WALL_CURVED_POST_01`, Brightwood rocks 7828 `MESH_MEDIUMROCK_LICHEN_01`, streetlamps 4978 `MESH_OBJECT_STREETLAMP_OFF_02`, pillars 7168, thorn vines 3977, villager 5149. Lookout instances **> 150**. Markers/cameras stay gizmos. | `GameBinFormatTests`, `WorldGeometryTests` |

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
| STB section 2 is a packed xyz stream | `stride=12` from the section header does not yield a dense height point cloud. | `LevFormatTests` |
| Compressed tile bytes are a 17×17 f32 grid | Without LZO, almost no values sit in the TNG Z range. | `LevFormatTests.Compressed_tile_payload_is_not_a_17_by_17_float_grid` |
| 15-byte verts on the packed tile | Z floats appear at ~15-byte gaps, but XY is not packed local. The stream is LZO; verts are world `u16` after inflate. | probe 2026-08-16 |
| Every Lookout `DiffuseMapID` framed-LZO decodes | Some bank ids overrun the LZO reader. `TryLoad` returns null; those draws use a 1×1 fallback. | `GpuTextureTests` |
| STB `.lev` is the same format as WAD `.lev` | STB blob starts `u32=1`, not version 25 / `0x1904`. | `LevFormatTests` |
| Extra `Matrix4x4.Transpose` on VP | Double transpose. Screen was solid clear-color. | `CameraProjectionTests` |
| WAD `Find("Lookout")` for a `.tng` | Stem match hit `LookoutPoint.lev`. Must pass the extension. | `TlcInstallTests` |

## Open

1. **Full game.bin field tables.** We read `Graphic.bank_index` and `CReplaceableMeshDef`. Other controls (health, physics, inventory, quests) are still raw bytes.
2. **Creature clothing / appearance layers.** Villager Graphic is the unclothed body (`MESH_BS_MALE_MIDDLE_UNCLOTHED_01`). `CAppearanceDef` / morphs are unread.
3. **Streetlamp lit vs off.** TNG `OBJECT_STREETLAMP_LIT_SINGLE_01` maps to `MESH_OBJECT_STREETLAMP_OFF_02`. Lit state is probably a replaceable / particle, not a second mesh id.
4. **Tile leftovers / west strip.** After the 15-byte vertex array the inflated tile still has a large tail (indices / extra LOD). The first 16 units of X often stay bilinear because tile meshes start on the coarse cell's east edge. Section 2's 4 KB header is unread.
5. **GPU texturing leftovers.** Sampler is 2D RGBA, one draw per texture id. No atlas, no mipmaps, no bump/reflection/illumination maps, no DXT-on-GPU. Terrain UVs still tile every 16 world units.
6. **WAD cell bytes 4–7 / 14–20.** High-entropy field and flags after the material slots are unread.
7. **Animation / bones / cloth.** Parser skips the blocks so static positions survive. No skinning.
8. **Hero, combat, quests, UI, audio.** Not started.

## How to add a note

- Prove a positive with a test against the live install.
- Prove a negative the same way (`Assert.True(inRange < threshold, "unexpectedly …")`).
- One sentence in this file pointing at the test.
- Do not check in game bytes.
