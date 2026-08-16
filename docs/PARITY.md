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
| TNG → mesh (partial) | `meshdata.h` name match: `DefinitionType`, `MESH_` + type, or `MESH_` + stem after `OBJECT_` / `CREATURE_` / `BUILDING_`. Skip `[PHYSICS]`. Lookout: 39 exact + a few prefix hits (BIGROCK, beggar, trader). Markers have no mesh. | `MeshFormatTests`, `WorldGeometryTests` |

## Did not work

| Guess | Why it failed | Locked by |
|---|---|---|
| WAD `.lev` is the landscape mesh | Parses as a 255-slot theme table. `MeshFile.TryParse` returns null. | `LevFormatTests`, dump `lev` |
| Framed LZO at WAD `.lev` payload is a dense f32 height grid | Almost no values in the TNG Z range. | `LevFormatTests.Lookout_payload_lzo_does_not_decode_as_dense_f32_grid` |
| `FindMeshId("OBJECT_WALL_SMALL_POST_01")` | No `MESH_WALL_SMALL_POST_01` in `meshdata.h`. Related names exist (`MESH_BS_WALL_SMALL_POST_02`) but that is the wrong object. | `MeshFormatTests` |
| `graphics.big` name contains `BRIGHTWOOD_MEDIUMROCK` | Zero hits. Rocks are not named after the TNG def. | probe 2026-03-16 |
| `game.bin` stores ASCII `OBJECT_*` / `#definition` | 996 KB, no those strings. | `GameBinFormatTests` |
| `game.bin` is a table of `names.bin` hashes | `OBJECT_WALL_SMALL_POST_01` hash never appears on a 4-byte align. | `GameBinFormatTests` |
| `game.bin` is Fable-framed LZO | Consumes ~8 KB, almost no ASCII. | `GameBinFormatTests` |
| STB `.lev` is the same format as WAD `.lev` | STB blob starts `u32=1`, not version 25 / `0x1904`. | `LevFormatTests` + probe |
| Extra `Matrix4x4.Transpose` on VP | Double transpose. Screen was solid clear-color. | `CameraProjectionTests` |
| WAD `Find("Lookout")` for a `.tng` | Stem match hit `LookoutPoint.lev`. Must pass the extension. | `TlcInstallTests` |

## Open

1. **`game.bin` object defs.** This is the missing TNG → mesh link for walls, lamps, Brightwood rocks, pillars, most creatures. Fable Explorer treats it as a control-byte compiled def stream. Not text, not names.bin hashes, not framed LZO.
2. **Finer STB landscape.** 3 MB after the 16-unit lattice. `u32@2048=6144`, `u32@2052=3371`. Not a dense f32 grid in the first 200 KB.
3. **GPU texturing.** UVs and RGBA decode. The client currently *samples* those textures onto vertex color. A Vulkan atlas / sampler is the next renderer step.
4. **WAD `.lev` payload after the `21` tag.** Ground material per cell is still unread.
5. **Animation / bones / cloth.** Parser skips the blocks so static positions survive. No skinning.
6. **Hero, combat, quests, UI, audio.** Not started.

## How to add a note

- Prove a positive with a test against the live install.
- Prove a negative the same way (`Assert.True(inRange < threshold, "unexpectedly …")`).
- One sentence in this file pointing at the test.
- Do not check in game bytes.
