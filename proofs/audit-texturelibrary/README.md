# Audit: `TextureLibrary.cs`

Investigation only. No production `src/` edits.

Question: does `TextureLibrary` eagerly load the whole game?
Frontend vs 3D? Native lifetime of `textures.big` / `GBANK_MAIN`?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `src/Fable.Game/TextureLibrary.cs`, `FrontendSpriteBank.cs`,
`EngineLifecycle.cs` (`OpenTextureBank`, `BindSubmittedTextures`,
`EnterGame`, `SubmitCurrentWorld`, `CloseStaticMapFile`, `Dispose`,
`AttachFrontendTree`, `BuildFrame`), `src/Fable.Formats/Banks/BigArchive.cs`,
`BankDirectory.cs`, `src/Fable.Formats/Textures/TextureFile.cs`,
`src/Fable.Client/Program.cs`, `SilkEngineHost.cs`,
`src/Fable.Render/VulkanLineRenderer.Frontend.cs`;
`proofs/texture-library-open/README.md`;
`docs/status/investigations/2026-08-18-resource-manager.md`;
`docs/status/investigations/2026-08-18-load-profile.md`;
`GpuTextureTests.Texture_decode_is_cached`;
`EngineLifecycleTests.Install_banks_and_startup_videos_exist`,
`World_submit_is_stable_between_frames`.

First-open vs `frontend.big` is already **PROVEN** in
`proofs/texture-library-open`. This pass is load / owner / lifetime.

---

## Verdict

**No. `TextureLibrary` is not an eager whole-game decode.**

Ctor opens `graphics/pc/textures.big` and indexes **all**
`GBANK_MAIN_PC` directory records. It does **not** `Read` payloads
and does **not** walk `_byId` into `TextureFile.Parse`. Decode is
`TryLoad` / `LoadMany` of submitted draw ids only. First Lookout
submit is tens of unique ids, not the bank.

Frontend 2D is a **different file and class**
(`frontend.big` / `FrontendSpriteBank`). `TextureLibrary` is the
3D `GBANK_MAIN` object. Production `Program` does not construct
`TextureLibrary`; `EnterGame` `"Init Graphics"` does.

Native: names at bootstrap, directory at `00416C8A`, decode at
`009FD910` / `009BE8B0`, bank survives region close. Host
`Dispose` is process teardown only.

| Claim | Class |
|---|---|
| Ctor decodes every `textures.big` payload | **DISPROVEN** |
| Ctor is whole-bank **directory** (`ReadEntries` + `_byId`) | **PROVEN** (native **MATCH** `009CFBC0`) |
| First production construct is after Leave, `Init Graphics` | **PROVEN** |
| Frontend Press Start uses `TextureLibrary` | **DISPROVEN** |
| `Sample` is the GPU / frontend path | **DISPROVEN** (tests only) |
| First submit decodes unique `SubmittedMesh` ids, cached | **PROVEN** (~40 Lookout ids, 33–36 MB RGBA) |
| Region / `CloseStaticMapFile` disposes the bank | **DISPROVEN** host; native **MATCH** keep |
| Payload format DXT scratch vs host RGBA | **DIVERGE** |
| Host `_cache` never evicts | **DIVERGE** vs `009FD970` slot (unused on region) |

---

## Path (no-save New Game)

```
Bootstrap 009A8150
  RegisterRetailBankTable
    GBANK_MAIN / GBANK_MAIN_PC          // names only
    GBANK_FRONT_END / GBANK_FRONT_END_PC
    GBANK_GUI / GBANK_GUI_PC            // names only; host never opens GUI
AVI / Press Start
  AttachFrontendTree → new FrontendSpriteBank
    frontend.big / GBANK_FRONT_END_PC
  CollectFrontendRecords → FrontendSpriteBank.TryLoad
  TextureLibrary is null                // PROVEN
Leave 0042F2A2
EnterGame 0042F491 → 004184BD
  "Init Graphics" 00416C8A
    OpenTextureBank → new TextureLibrary
      BigArchive.Open(textures.big)
      ReadEntries(GBANK_MAIN_PC)        // directory, no payload
PumpGameUpdate
  HeroSpawned && !WorldSubmitted
    SubmitCurrentWorld
      BindSubmittedTextures
        OpenTextureBank                 // no-op
        LoadMany(draw TextureId/TextureId1)
        GpuTexture[] on EngineFrame
later Present
  Silk prefers frame.Textures
  host.Textures fallback unused
Dispose
  Textures._big.Dispose()               // process end
```

---

## 1. Eager whole-game load? **DISPROVEN** (decode)

`TextureLibrary` ctor (`src/Fable.Game/TextureLibrary.cs`):

```
BigArchive.Open(data/graphics/pc/textures.big)
SubBanks First Name == "GBANK_MAIN_PC"
_byId = ReadEntries(bank).GroupBy(Id).First
_cache empty
DecodedCount == 0
```

`BigArchive.Open` reads header + footer of **named sub-banks**.
`ReadEntries` is `BankDirectory`: magic / id / type / size /
offset / crc / name / deps / **info** (34-byte header).
**No** `BigArchive.Read(entry)` in the ctor.

`TryLoad(id)`:

1. `id <= 0` → null
2. `_cache` hit → same `TextureFile`
3. `_byId` miss → null
4. `TextureFile.Parse(..., _big.Read(entry))` → LZO framed top
   mip + DXT/RGBA → **full RGBA** into `_cache`

`LoadMany` is unique ids → `TryLoad`. `Sample` is `TryLoad` then
nearest texel; miss colour `(0.45, 0.50, 0.38)`. Production
`EngineLifecycle` never calls `Sample`. Tests only
(`GpuTextureTests`, `MeshFormatTests`).

There is **no** loop over `_byId` that decodes the bank.

Clock (load-profile, Lookout first submit):

| Step | Cost | What |
|---|---|---|
| ctor / `OpenTextureBank` | 5–16 ms | directory |
| first `LoadMany` | 87–115 ms | 40–42 ids, 33.6–35.7 MB RGBA |
| cached `LoadMany` | 0–1 ms | same `_cache` |

`World_submit_is_stable_between_frames`: second `Pump` does not
increment `DecodedCount`. `SubmitCurrentWorld` returns if
`WorldSubmitted && SubmittedMesh != null`. `BindSubmittedTextures`
runs **once** on the first-seen New Game spine.

What **is** eager: the **entire** `GBANK_MAIN_PC` directory is in
RAM as `_byId`. Native `009CFBC0` does the same
(resource-manager §3.2). That is a bank **index**, not a game
decode.

What first submit **does** decode: every unique `TextureId` /
`TextureId1` on `SubmittedMesh.Draws` (land + props + sky that
made that blob). That is the **current submit set**, not Albion.

`_cache` is the **union** of every id ever `TryLoad`ed. It never
evicts. Region travel that later submits new ids grows the CPU
RGBA set. GPU `EngineFrame.Textures` is rebuilt from the **current**
draw ids only (`BindSubmittedTextures` clears `_submittedTextures`
first).

`GBANK_GUI` is registered at bootstrap and **never** opened by
`TextureLibrary`. **DISPROVEN** that this class owns GUI / particles.

---

## 2. Frontend vs 3D — **PROVEN** split

| | Frontend 2D | 3D world |
|---|---|---|
| File | `graphics/pc/frontend.big` | `graphics/pc/textures.big` |
| Sub-bank | `GBANK_FRONT_END_PC` | `GBANK_MAIN_PC` |
| Native open | `0042DDB3` / `009F83D0("GBANK_FRONT_END")` | `00416C8A` / `009F83D0("GBANK_MAIN")` |
| Host class | `FrontendSpriteBank` | `TextureLibrary` |
| First host ctor | `AttachFrontendTree` (AVI / Press Start) | `EnterGame` `"Init Graphics"` |
| Id | persist `GraphicIndex` | mesh / land / sky bank id |
| Decode | `FrontendSpriteBank.TryLoad` | `TextureLibrary.TryLoad` |
| Present | `FrontendBatch` → `SetFrontendBatch` | `EngineFrame.Textures` → `SetTextures` |
| GpuTexture.Id | local 0..n-1 in `CollectFrontendRecords` | real `file.Id` |

`CollectFrontendRecords` never touches `life.Textures`.
`BindSubmittedTextures` never touches `_frontendSprites`.

`SilkEngineHost.Present`:

- AVI → video frame
- else nonempty `FrontendBatch` → 2D (frontend textures)
- 3D only if `frame.Vertices` / `ObjectVertices` nonempty

`BuildFrame` always passes `_submittedTextureArray`. On frontend
that field is null (`Textures` null on the frame). After first
submit it is the 3D set. Native frontend Present is 2D batch +
clear; a leftover 3D camera on the same `EngineFrame` is
documented in `proofs/camera-after-leave`, **not** a
`TextureLibrary` open.

`Program.cs` constructs `SilkEngineHost` **without** a
`TextureLibrary`. `host.Textures` stays null. The
`LoadGpuTextures` dummy-mesh fallback is **LEFTOVER** /
**TEMPORARY BRIDGE** (H-regression-audit). Live path uses
`frame.Textures` from the engine.

`0041E3F6` native UI ctor can pick `GBANK_MAIN` if retail+9 is 0.
First-seen no-save takes `GBANK_FRONT_END`. Host
`EngineInput.Construct` opens neither. See
`proofs/texture-library-open` §3.

Do **not** treat Press Start `GraphicIndex` as a `textures.big` id.

---

## 3. Native lifetime — **PROVEN** open/keep, **PARTIAL** dtor

### Native

| Layer | Lives | Site |
|---|---|---|
| Name pair `GBANK_MAIN` / `_PC` | process | `009A8150` / `RegisterRetailBankTable` |
| `textures.big` archive handle | after first named open | `009D56C0` / bank+272 |
| Graphic bank object `0x30C` vtbl `0129C8CC` | after Init Graphics | `009F83D0` miss → `009FEA20` |
| Global | process | `004BBFC0` `[0x13B8A08]` |
| Directory tables | bank object | `009CFBC0` |
| Per-id slot | bank +480, 44 B | `009FD910` get |
| DXT `CreateTexture` | first load | `009BE8B0` scratch pool |
| Evict | vtbl+56 `009FD970` | **0 E8** on region change |
| Map close | region | `00B40000` — **not** banks |

`00416C8A` also probes device FourCC (`009BE800` / `830` / `870` /
`8B0`) and `009FD4E0` **11 scratch** textures. That is capability
setup, **not** a walk of `textures.big`. Host `OpenTextureBank`
`Note`s `009BE830` and does **not** CreateTexture. **DIVERGE**
probe; **MATCH** “no bank decode at init”.

`00B40000` / host `CloseStaticMapFile` unload map slots / water.
Do **not** dispose `TextureLibrary`. **MATCH**.

Graphic dtor `0x419036` / `00401B80` Shutdown walking
`[0x13B8A08]`: **UNREAD** this pass. Host `EngineLifecycle.Dispose`
closes `_big` (the `FileStream`). `ShutdownEngine` does **not**.

Leave `0042DD28` releases the **FRONT_END** retail+64 handle, not
`GBANK_MAIN`. Host keeps `_frontendSprites` until `Dispose` —
**DIVERGE** frontend lifetime, not MAIN.

### Host

| Object | Construct | Destroy |
|---|---|---|
| `TextureLibrary` | `OpenTextureBank` once (`Textures != null`) | `Dispose` only |
| `_big` | ctor `File.OpenRead` | `Dispose` → `_big.Dispose()` |
| `_byId` | ctor | **kept** until GC (`Dispose` does not clear; `MeshBank.Dispose` does) |
| `_cache` RGBA | first `TryLoad` per id | **never evicted** |
| `_submittedTextures` | each `BindSubmittedTextures` | cleared then rebuilt from current ids |
| `_submittedTextureArray` | same | `BuildFrame` identity reused until next bind |

`OpenTextureBank` is the **only** production `new TextureLibrary`
(`EngineLifecycle` 5933). Guard: `Textures is not null || Install is null`.
Callers: `EnterGame` `"Init Graphics"`, `BindSubmittedTextures`.
Tests / `_loadprobe` construct their own.

`TextureLibrary.Dispose` does **not** null `_byId` or `_cache`.
After dispose a `TryLoad` miss that hits `_byId` would `Read` a
closed stream. Production does not `TryLoad` after `Dispose`.
**PARTIAL** hygiene vs `MeshBank.Dispose`.

Native keeps DXT on the device. Host stores decoded RGBA in
`_cache` **and** copies it into `GpuTexture` for Vulkan.
**DIVERGE** format and RAM (load-profile: ~35 MB for 40 ids).

`009FD910`: if slot dirty, evict then load. Host is parse-once.
First-seen New Game does not need evict. Later memory pressure:
**UNREAD** native dispatcher.

---

## 4. Host vs native

| Item | Class |
|---|---|
| Names-only at `009A8150` | **MATCH** |
| Directory at `00416C8A` / ctor | **MATCH** |
| No payload at open | **MATCH** |
| Decode per submitted id, not `window.Load` | **MATCH** timing |
| One bank object, process life | **MATCH** |
| Survive `00B40000` | **MATCH** |
| Frontend is other BIGB | **MATCH** |
| `Program` does not own the library | **MATCH** (`991bab2`) |
| DXT scratch vs LZO+DXT→RGBA | **DIVERGE** |
| 11 init scratch textures | host skip **DIVERGE** |
| `009FD970` evict | host never **DIVERGE** / unused first-seen |
| `Dispose` vs native shutdown dtor | **PARTIAL** |
| `SilkEngineHost.Textures` fallback | **LEFTOVER** |
| `Sample` CPU nearest | **LEFTOVER** (tests) |
| `BuildFrame` always carries texture array field | null until submit; **MATCH** enough |

---

## Do not

- Decode `GBANK_MAIN` at ctor / Init Graphics / Bootstrap / AVI / Press Start.
- Collapse `frontend.big` and `textures.big` into one library.
- Use Press Start `GraphicIndex` as a `TextureLibrary` id.
- Dispose `TextureLibrary` on `CloseStaticMapFile` / region travel.
- “Fix” load by walking every `_byId` into `Parse`.
- Treat `DecodedCount == _byId.Count` as success.
- Construct `TextureLibrary` in `Program` / `window.Load`.

---

## Open

- Exact `GBANK_MAIN_PC` entry count on TLC (directory size **UNREAD**
  here; ctor still indexes all of them).
- `00401B80` / graphic dtor vs host `Dispose` (**UNREAD**).
- First `009FD910` id after Leave (**UNREAD** this pass; decode
  timing is **PROVEN** as submit, not open).
- Who calls `009FD970` under pressure (**UNREAD**).
- `GBANK_GUI` first native open (**UNREAD**; not this class).
- Skip-frontend / `[0x13B8648]` first bank (**UNREAD**; not no-save).
