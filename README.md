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

Skimmable done-vs-left overview: [`docs/status/`](docs/status/README.md) ([HTML](docs/status/index.html), [live](https://samg-coder.github.io/FableCSharp/docs/status/index.html)); ledgers above stay authoritative.
