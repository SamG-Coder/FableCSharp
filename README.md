# FableCSharp

A C# / Vulkan client that plays **Fable: The Lost Chapters** from the original game files.

Anniversary is a UE3 skin over the same Lionhead simulation. This repo treats TLC `data\` as the source of truth. Remaster meshes can be a later renderer backend.

## Requirements

- .NET 10 SDK
- A Steam install of *Fable: The Lost Chapters*

Default path:

`C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters`

Override with `FABLE_PATH` or pass the folder as the first `Fable.Dump` argument.

Game assets stay in Steam. Nothing from the install is copied into this repo.

## Fly Lookout Point

```bat
dotnet run --project src\Fable.Client
dotnet run --project src\Fable.Client -- PicnicArea
```

| Key | Action |
|---|---|
| WASD | Move on the ground plane |
| Q / E or Ctrl / Space | Down / up |
| Shift | Sprint |
| Right mouse | Look |
| F1 | Dump the first 40 things to the console |
| Esc | Quit |

Each TNG thing is an RGB axis gizmo. Objects resolve through `game.bin` `Graphic.bank_index` (with a `meshdata.h` name fallback) and are drawn as lit triangles, colored by sampling the mesh `DiffuseMapID` out of `textures.big` (UVs from the C3D vertex). Landscape comes from the runtime `FinalAlbion_RT.stb` copy of the `.lev` (16-unit height lattice; Lookout is 8×8 quads) using `LANDSCAPE_GRASS_PLAIN`. The smaller WAD `.lev` is a compiled material/theme table, not the mesh.

What has decoded and what has not is in [docs/PARITY.md](docs/PARITY.md). New facts go there as tests.

## Dump the install

```bat
dotnet run --project tools\Fable.Dump -- info
dotnet run --project tools\Fable.Dump -- wld
dotnet run --project tools\Fable.Dump -- wad Lookout
dotnet run --project tools\Fable.Dump -- tng LookoutPoint
dotnet run --project tools\Fable.Dump -- names MARKER
dotnet run --project tools\Fable.Dump -- qst
dotnet run --project tools\Fable.Dump -- big graphics.big
dotnet run --project tools\Fable.Dump -- tex LANDSCAPE_GRASS_PLAIN
dotnet run --project tools\Fable.Dump -- bin OBJECT_WALL_SMALL_POST_01
```

## Tests

```bat
dotnet test
```

Tests read the live TLC install.

## Layout

| Project | Role |
|---|---|
| `src/Fable.Core` | Install locator |
| `src/Fable.Formats` | WLD, TNG, QST, WAD/BBB, BIG, names.bin, game.bin, C3D, textures, UPK header |
| `src/Fable.Game` | Region / TNG resolution, texture library |
| `tools/Fable.Dump` | CLI inspector |
| `src/Fable.Render` | Silk.NET Vulkan line renderer + fly camera |
| `src/Fable.Client` | Lookout Point gizmo viewer |

Texture payloads are Fable-framed LZO. Format code 31 is DXT1, 32 is DXT5. `LANDSCAPE_GRASS_PLAIN` decodes to 512×512 RGBA. Mesh `DiffuseMapID` is that bank's entry id.

`game.bin` is zlib-chunked compiled defs. `OBJECT.Graphic.bank_index` is the `graphics.big` mesh id, so Lookout walls / rocks / lamps instance now.

## Next

1. Finer landscape (the STB blob is 3MB; we only sample the 16-unit lattice)
2. GPU sample textures (UVs already land as vertex color)
3. Creature clothing / appearance layers
4. Walkable hero instead of a fly camera
