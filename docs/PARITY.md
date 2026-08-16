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
| STB coarse height | Runtime `FinalAlbion_RT.stb` copy of `.lev` is ~3 MB. From offset 2056, 36-byte records hold two WLD-space verts on a **16-unit** lattice. Lookout is 8×8 quads, Picnic 8×6. Heights 20–80 match TNG Z. | `LevFormatTests` |
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
| STB `.lev` is the same format as WAD `.lev` | STB blob starts `u32=1`, not version 25 / `0x1904`. | `LevFormatTests` + probe |
| Extra `Matrix4x4.Transpose` on VP | Double transpose. Screen was solid clear-color. | `CameraProjectionTests` |
| WAD `Find("Lookout")` for a `.tng` | Stem match hit `LookoutPoint.lev`. Must pass the extension. | `TlcInstallTests` |

## Open

1. **Full game.bin field tables.** We read `Graphic.bank_index` and `CReplaceableMeshDef`. Other controls (health, physics, inventory, quests) are still raw bytes.
2. **Creature clothing / appearance layers.** Villager Graphic is the unclothed body (`MESH_BS_MALE_MIDDLE_UNCLOTHED_01`). `CAppearanceDef` / morphs are unread.
3. **Streetlamp lit vs off.** TNG `OBJECT_STREETLAMP_LIT_SINGLE_01` maps to `MESH_OBJECT_STREETLAMP_OFF_02`. Lit state is probably a replaceable / particle, not a second mesh id.
4. **Finer STB landscape.** 3 MB after the 16-unit lattice. `u32@2048=6144`, `u32@2052=3371`. Not a dense f32 grid in the first 200 KB.
5. **GPU texturing.** UVs and RGBA decode. The client currently *samples* those textures onto vertex color. A Vulkan atlas / sampler is the next renderer step.
6. **WAD `.lev` payload after the `21` tag.** Ground material per cell is still unread.
7. **Animation / bones / cloth.** Parser skips the blocks so static positions survive. No skinning.
8. **Hero, combat, quests, UI, audio.** Not started.

## How to add a note

- Prove a positive with a test against the live install.
- Prove a negative the same way (`Assert.True(inRange < threshold, "unexpectedly …")`).
- One sentence in this file pointing at the test.
- Do not check in game bytes.
