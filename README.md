# FableCSharp

**FableCSharp** is an experimental C# / Vulkan reimplementation of the
runtime and rendering behaviour of **Fable: The Lost Chapters**.

The project reads the original PC game data and reconstructs Lionhead's
engine behaviour from the original assets, compiled definitions,
scripts, shaders and executable evidence. The long-term goal is not to
make a game that merely resembles Fable. The goal is to reproduce the
original game's semantics as faithfully as possible while replacing the
original DirectX 9 / Win32-facing implementation with a modern C#
runtime and Vulkan renderer.

> **You must own a legitimate copy of Fable: The Lost Chapters to use
> this project.**
>
> FableCSharp does **not** include or redistribute the original game's
> copyrighted assets, videos, audio, scripts, data banks or executable.
> It expects those files to come from the user's own legally obtained
> installation.

## Project status

FableCSharp is **work in progress**.

It is already substantially beyond an asset viewer. The repository
contains parsers for the original data formats, world reconstruction, a
Vulkan rendering path, a reconstructed quest/fiber/cutscene runtime,
DirectShow-based playback of the original video assets, camera and
scheduler behaviour recovered from `Fable.exe`, movement/runtime work,
dialogue and audio reconstruction, and an expanding parity test suite.

It is **not yet a complete replacement for the original game**. Many
engine behaviours are proven and implemented, many are partially
reconstructed, and some remain explicitly `UNREAD`.

See the detailed ledgers under `docs/`, especially:

-   `docs/PARITY.md`
-   `docs/runtime/COMMAND_COVERAGE.md`
-   `docs/runtime/COMMAND_MAP.generated.md`
-   `docs/render/DX9_VULKAN_PARITY.md`
-   the first-scene/world render-contract documentation

Skimmable done-vs-left overview: [`docs/status/`](docs/status/README.md) ([HTML](docs/status/index.html)); ledgers above stay authoritative.

## Intent

This project exists for **compatibility, preservation, technical
research and learning**.

The intention is to understand how the original PC version of *Fable:
The Lost Chapters* works and to build an independent runtime capable of
consuming a user's original game files.

The project is specifically **not** intended to distribute Fable or its
assets, provide a free copy of the game, replace the requirement to own
the original game, bypass storefront ownership/DRM/licensing
requirements, claim ownership of Lionhead/Microsoft intellectual
property, or produce a loosely similar remake by manually recreating
copyrighted game content.

The repository should contain **code, parsers, tests, documentation and
independently recovered compatibility information**, not the original
game data.

If you do not own a legitimate copy of *Fable: The Lost Chapters*, this
project is not intended to provide one.

## Legal / ownership notice

*Fable*, *Fable: The Lost Chapters*, Lionhead Studios, associated
characters, names, artwork, audio, video and game data are the property
of their respective rights holders.

FableCSharp is an independent, unofficial project and is **not
affiliated with, endorsed by, sponsored by or supported by Microsoft,
Xbox Game Studios, Lionhead Studios, Valve or Steam**.

No original game assets are licensed under the source-code license of
this repository. Users are responsible for ensuring that their use of
the project and their copy of the game complies with the laws and
licence terms applicable to them.

Nothing in this repository should be interpreted as legal advice.

## A parity project, not a visual remake

A central rule of FableCSharp is:

> **Do not make it look like Fable. Make the implementation perform the
> proven semantic equivalent of what Fable performed.**

A screenshot is useful diagnostic evidence, but it is not the parity
oracle.

For rendering, parity means reconstructing:

``` text
original Fable assets/state
        ↓
asset interpretation
        ↓
world/object transforms
        ↓
camera + view + projection
        ↓
DX9 shader/fixed-function semantics
        ↓
render states + resources
        ↓
draw/pass ordering
        ↓
explicit DX9 → Vulkan translation
        ↓
Vulkan
```

For gameplay/runtime behaviour:

``` text
original compiled data
        ↓
quest/script object
        ↓
microthread / fiber scheduler
        ↓
CCutsceneDef interpreter
        ↓
command dispatch
        ↓
world / camera / movement / animation / dialogue / audio state
        ↓
renderer
```

When behaviour is not yet established from the original executable/data,
it remains `UNREAD`, `PARTIAL`, `TEMPORARY` or `DISPROVEN`. The project
does not intentionally turn a visual approximation into a parity claim.

## Why Vulkan?

The original Windows release uses DirectX 9.

FableCSharp uses Vulkan as the rendering backend, but Vulkan is **not
intended to redefine the renderer**. The project reconstructs the
original DX9 semantics and translates them explicitly into Vulkan
equivalents.

This includes projection/viewport conventions, clip-space handling,
world/view/projection construction, depth/culling/blend state, samplers,
texture formats, shader constants, vertex formats, draw ordering and
PALSKIN/skinned-character rendering.

This separation also leaves room for a future optional **enhancement
pipeline** --- modern upscaling, HDR or other effects --- without
contaminating the original/parity renderer.

## Reverse-engineering methodology

The project follows an evidence-first convergence loop:

``` text
PROVE
  ↓
PERSIST EVIDENCE
  ↓
IMPLEMENT
  ↓
INTEGRATE
  ↓
TEST
  ↓
COMMIT
  ↓
SELECT NEXT PARITY GAP
```

Incorrect assumptions are deliberately recorded as **DISPROVEN** so they
do not quietly return later.

Script command parity is tracked at multiple levels:

``` text
Parse
Dispatch
Return
Apply
Runtime
```

Knowing how a command is parsed and dispatched is not the same as having
implemented its complete downstream engine behaviour.

## What has been reconstructed

The authoritative details live in `docs/PARITY.md`; this is a high-level
overview.

### Original data formats

Substantial portions of the TLC data set are understood, including WLD,
WAD/BBB, LEV/TNG, QST, `names.bin`, `game.bin`, `frontend.bin`,
`script.bin`, `graphics.big`, `textures.big`, C3D meshes/materials,
Fable-framed LZO data and STB landscape/tile data.

Recovered behaviour includes packed C3D positions/normals/UVs, material
fields, diffuse texture IDs, DXT texture formats and mip payloads, STB
tile meshes, adaptive triangle strips and edge-strip geometry.

### World reconstruction

Important recovered coordinate-space behaviour includes:

-   TNG positions are region-local.
-   STB terrain vertices use WLD/global coordinates.
-   Map-origin conversion brings terrain and TNG objects into a common
    region space.
-   C3D geometry is Z-up and stored in centimetres.
-   TNG object basis follows the recovered right-handed Z-up convention.
-   Native landscape rendering uses camera-relative vertices with a
    camera translation.
-   The host can retain world-space STB vertices with identity world
    transform where mathematically equivalent.
-   First-seen visibility and render-layer submission are reconstructed
    from native paths instead of being flattened into one generic pass.

Earlier visual workarounds have been removed where executable evidence
disproved them.

### DX9 → Vulkan rendering

The project has a dedicated DX9/Vulkan parity layer. Recovered work
includes first-scene camera/view/projection behaviour,
`clip.w = view.z`, Vulkan clip-Y translation, C3D normal decoding,
correct static/PALSKIN UV sources, world-space landscape handling,
first-seen render layers, landscape visibility, material/texture
behaviour, PALSKIN register/palette interpretation and shader/state
reconstruction.

Rendering remains active work, especially complete character
pose/animation parity and other unresolved shader/material/state paths.

### Script / quest runtime

The runtime has been rebuilt away from a simple flat command player.

Important findings include:

-   `CCutsceneDef` commands come from persisted command vectors, not
    arbitrary printable-string scraping.
-   `S_QNOVI` is a native quest/script object with persist state and a
    fiber, not simply another `script.bin` list.
-   Runtime execution is modelled around quest/fiber/cutscene behaviour.
-   Command results distinguish immediate continuation, one-shot yields,
    frame waits, scaled waits, blocking operations and explicit
    unread/block states.
-   Unknown commands are not silently treated as successful no-ops.
-   Dynamic bindings, flags, persist state and runtime traces are being
    reconstructed.

### Camera / cutscenes

A substantial scripted-camera family is recovered, including
`UseCamera`, `NoLoadUseCamera`, `ResetCamera`, `CameraPause`,
`CameraLookAt`, `CameraLookBetween`, FOV variants, `CameraPath`,
`CameraRotateThing`, `CameraRig`, `UseCameraFOVMarkerList`, camera
waits, effects/shake, tint and light-scene commands.

These commands drive runtime camera state intended to feed the Vulkan
renderer rather than a separate fake cutscene camera.

### Movement / entity tasks

Movement is no longer treated as an instant teleport simply to advance
scripts.

Recent executable work established that the suspected creature `vtbl+20`
path is a stub for the relevant creature classes while sibling `vtbl+16`
participates in creature movement. The runtime now advances actor
movement state over ticks and feeds actor positions back into the
first-scene world.

Full navigation/locomotion parity remains active work.

### Animation

Animation command semantics are being separated according to the
original engine:

-   `PlayAnimation` uses actor `vtbl+72`.
-   `PlayLoopingAnim` is a distinct `vtbl+80` path.
-   its second argument is a loop count rather than another boolean
    flag.
-   animation-related yielding follows recovered cutscene state rather
    than a blanket "animation always yields" rule.

Complete animation resource lookup, clip sampling, skeletal pose
evaluation and PALSKIN pose integration remain unfinished.

### Dialogue / text

The runtime has begun reconstructing actual dialogue sessions and
original text lookup instead of merely recording that a `Speak` command
occurred.

`Speak`, `InteractiveSpeak`, `DialogSpeak`, `DialogadSpeak` and
`WaitActiveDialog` are among the recovered paths. Deeper UI, dismissal
and voice behaviour remains partial.

### Audio

Script-level behaviour has been recovered for parts of `PlaySound`,
`Play2DSound`, `PlayMusic`, `CacheMusic` and sound enable/mute handling.

The original lower-level audio player/resource lifecycle remains
incomplete.

### Object creation / effects / world state

Recovered command paths include `Create`, `ObjectCreate`, `CreateNear`,
crowd creation, `CreateEffect`, `DummyEffect`, `CreateLight`, removal
commands, `DrawThing`, door/chest state, flags/waits, light-scene state
and tint state.

Proving the script-layer call is deliberately kept separate from proving
the full downstream object lifetime/render implementation.

## Original video playback / PlayAVI

The original game uses a custom DirectShow renderer path for video
playback.

FableCSharp reconstructs that behaviour rather than opening cutscenes in
a separate media-player window. Work includes FilterGraph integration,
custom filter/pin behaviour, RGB24 negotiation,
`IMediaSample::GetPointer`, allocator/sample lifetime, native
`IReferenceClock` timing, `WaitForSingleObjectEx` pacing, blocking
`PlayAVI` pump semantics, frame copy into the game renderer, bottom-up
RGB24 orientation, letterboxing and Vulkan upload without per-frame
`vkQueueWaitIdle`.

The video path is kept separate from script-controlled dialogue/audio.

## Requirements

### Required

-   .NET 10 SDK
-   a **legitimate PC copy of Fable: The Lost Chapters**
-   the original TLC game files installed locally

The default Steam location is:

``` text
C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters
```

Set `FABLE_PATH` to override the installation path where supported.

### Windows-specific functionality

The current `PlayAVI` implementation reconstructs the original Windows
DirectShow/COM path and is therefore Windows-specific.

The format/runtime/Vulkan architecture is kept separate enough that
alternative media backends could be implemented for Linux/macOS later
without changing recovered Fable script semantics.

## No game assets in this repository

The repository must not contain redistributed copies of the original
game's executable or asset/data files, including `Fable.exe`,
BIG/WAD/STB/TNG/LEV banks, `game.bin`, `script.bin`, videos, music,
voices, textures, models or animations.

Those files remain in the user's own legal installation. FableCSharp
reads them locally at runtime or during tests/tools.

## Building

``` bat
dotnet build
dotnet test
dotnet run --project src\Fable.Client
```

Many tests intentionally read the user's live TLC installation.

A passing host test is not automatically parity evidence if its expected
value was invented by the host. Evidence-backed tests should derive
expectations from original files or persisted reverse-engineering
evidence.

## Inspection tools

``` bat
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

## Repository layout

  -----------------------------------------------------------------------
  Path                                Purpose
  ----------------------------------- -----------------------------------
  `src/Fable.Core`                    Install discovery and common
                                      infrastructure

  `src/Fable.Formats`                 Original TLC data-format readers

  `src/Fable.Game`                    Runtime, world, scripts, quests,
                                      camera and game-state
                                      reconstruction

  `src/Fable.Game/Scripting`          Dispatch, scheduler, bindings,
                                      arguments, tasks and persist
                                      infrastructure

  `src/Fable.Render`                  Vulkan renderer and DX9 semantic
                                      translation

  `src/Fable.Client`                  Interactive client

  `tools/Fable.Dump`                  CLI inspection utility

  `tests`                             Format/runtime/render parity tests

  `docs`                              Persisted parity evidence and
                                      reconstruction notes
  -----------------------------------------------------------------------

## Current development priorities

The parity loop currently concentrates on the highest-impact unresolved
engine systems rather than simply increasing command-count coverage.

Major active areas include:

1.  animation resource lookup, clip evaluation, skeletal pose generation
    and PALSKIN integration;
2.  movement/navigation and entity task completion;
3.  dialogue/voice/audio lifecycle;
4.  generic quest lifecycle beyond the first reconstructed quest;
5.  complete creation/removal ownership semantics;
6.  feeding runtime-created effects/lights/state into the DX9-equivalent
    Vulkan renderer;
7.  remaining world/render/shader/state parity.

## Contributing

Contributions should follow the evidence-first approach.

Please avoid patches that merely make a scene look correct through
guessed offsets, arbitrary timers, fake movement, placeholder dialogue
completion or renderer-specific compensation.

A useful parity change should ideally explain:

-   what previous assumption was wrong;
-   what original evidence establishes the behaviour;
-   how the C# implementation reproduces it;
-   how it is tested;
-   what remains unread.

Keep original copyrighted game data out of commits, issues and pull
requests.

## Long-term direction

The primary target is a faithful independent runtime for the original
TLC data.

Once the parity path is sufficiently mature, the architecture can
support optional enhancements after or around the proven render contract
--- for example improved scaling, HDR or other modern rendering features
--- while preserving an original/parity mode.

The important distinction is:

``` text
Fable data + recovered Fable semantics
                ↓
         parity runtime
                ↓
      DX9 semantic renderer
                ↓
       Vulkan translation
                ↓
          parity output
                ↓
      optional enhancements
```

Modern enhancements should never redefine the original parity contract.
