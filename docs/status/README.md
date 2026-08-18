# Status — done vs left

Human overview of FableCSharp. **Not** a rewrite of the evidence
ledgers. If a number, address, or “done” claim is not in those files
or in tests/code, treat it as **UNREAD**.

| This page | The ledgers |
|---|---|
| Skim what’s locked and what’s next | Authoritative tables, addresses, tests |
| Sequenced by dependency | Do not copy their raw tables here |

**North star:** close first-scene New Game
(`StartOakValeWest` / `Q_NewOakValeIntro` / `HerosOldHouse` /
`CAM_OVIF_SHOT2`) so `CS_OAKVALE_INTRO_FATHER` can run on a real
world clock.

Snapshot: **2026-08-18**, master merge `63b03cc`, runtime HEAD
`1ebece6` (*Runtime: 006C2170 Loading objects is 00521AE0 map TNG.*).
Just locked on this path: `6e1ff8e`, `64a2e14`, `0d8f5e5`, `1ebece6`.
Master is still proving **boot / world clock**, not animation.
README’s long-term priority list still starts with animation; that
list is not the current phase.

Open the same content as a page: [index.html](index.html)
(local file, or GitHub Pages under `/status/` if Pages is `/docs`).

## How to read this

Statuses are the project’s own words:

| Word | Meaning here |
|---|---|
| **PROVEN** | Locked from exe / assets / tests |
| **PARTIAL** | Script-layer or host path recovered; apply or runtime body not |
| **UNREAD** | Not filled with a look-better guess |
| **DISPROVEN** | A previous assumption, recorded so it does not return |
| **TEMPORARY** | Stand-in classified as such (often a D3D default) |

Parse / dispatch / return ≠ apply ≠ runtime. A finished fixture is
script-layer apply, not a mesh or UI body.

This is **not** a “finish every UNREAD command” roadmap.

## Authoritative files

| Ledger | What it owns |
|---|---|
| [docs/PARITY.md](../PARITY.md) | Worked / DISPROVEN / Open |
| [docs/runtime/COMMAND_COVERAGE.md](../runtime/COMMAND_COVERAGE.md) | Token counts and per-verb overall |
| [docs/runtime/COMMAND_MAP.generated.md](../runtime/COMMAND_MAP.generated.md) | Generated parse / dispatch / apply / runtime |
| [docs/runtime/COMMAND_MAP.md](../runtime/COMMAND_MAP.md) | Hand map + StartNewGame bindings |
| [docs/runtime/FIXTURE_COMMAND_AUDIT.md](../runtime/FIXTURE_COMMAND_AUDIT.md) | Script-layer fixture outcomes |
| [docs/render/DX9_VULKAN_PARITY.md](../DX9_VULKAN_PARITY.md) | DX9 → Vulkan matrix |
| [docs/render/FIRST_SCENE_CONTRACT.md](../FIRST_SCENE_CONTRACT.md) | First-scene submit contract |
| [docs/render/FIRST_SCENE_AUDIT.md](../FIRST_SCENE_AUDIT.md) | Live-path classification + leftover UNREAD |
| [docs/render/FIRST_SCENE_WORLD_PARITY.md](../FIRST_SCENE_WORLD_PARITY.md) | World/space/visibility checklist |

Boot/clock facts below are from `EngineLifecycle` /
`EngineLifecycleTests` (not yet copied into `PARITY.md`).

## Coverage snapshot

From [COMMAND_COVERAGE.md](../runtime/COMMAND_COVERAGE.md) (generated
from exe token list `0x012C1500–0x012C2C00` + `ScriptCommandMap`):

| Metric | Count |
|---|---|
| Native command tokens | 185 |
| Recovered dispatch / return | 156 |
| Recovered apply | 93 |
| Implemented runtime | 13 |
| UNREAD tokens | 29 |

The 13 **PROVEN** overall runtimes are `FadeOut` / `FadeIn` (global and
entity), `SetTime`, `WaitFlag`, `SetFlag`, `PlayAVI`, `GamePause`,
`DoOneFrame`, `CameraPause`, `ScriptFrame`, `DoScriptFrame`.
`PlayAnimation` apply is still **PARTIAL**. First-seen create does not
play an anim (`FirstSeenPlaysAnim=false`).

Do not grind the 29 UNREAD tokens (Crowd\*, debug, `LadyGreyIntro`,
boss fights, …) as a phase.

---

## What’s done

Sequenced as the engine actually runs. Wrong assumptions named only
when a ledger or test already records them.

### Formats / first-scene world (already locked)

| Item | Status | Where |
|---|---|---|
| TLC data formats (WLD, WAD/BBB, LEV/TNG, QST, bins, C3D, STB, …) | PROVEN | [PARITY.md](../PARITY.md) Worked |
| New Game *view* is `StartOakValeWest` / `HerosOldHouse` / `CAM_OVIF_SHOT2`, not Lookout | PROVEN | [FIRST_SCENE_CONTRACT.md](../render/FIRST_SCENE_CONTRACT.md), `WorldSceneTests` |
| Adult Lookout is not the first-scene camera | PROVEN | [FIRST_SCENE_WORLD_PARITY.md](../render/FIRST_SCENE_WORLD_PARITY.md) |
| TNG local vs STB WLD; `STB − (MapX, MapY)` meeting space | PROVEN | same |
| House 6909/6911, kid 4300 bind-pose, landscape strips, sky else-path `0x2000` | PROVEN | contract + world parity |
| DX9 → Vulkan first-scene submit (layers `0x4` → `0x40` → `0x20` → `0x2000`) | PROVEN / EQUIVALENT | [DX9_VULKAN_PARITY.md](../render/DX9_VULKAN_PARITY.md) |
| `T(cam)` on host world-space STB verts | DISPROVEN | world parity; submit uses identity W |
| Invented `stars.dat` billboards / 1 m landscape fill | DISPROVEN | PARITY / world parity |
| `S_QNOVI` + `NOVI_LiveFather` → `CS_OAKVALE_INTRO_FATHER` | PROVEN | [COMMAND_MAP.md](../runtime/COMMAND_MAP.md), PARITY |
| `00DB8680` starts the intro | DISPROVEN | start is `00DB86B0` (PARITY) |
| Intro fixture `CS_OAKVALE_INTRO_FATHER` | Finished def+60 (script layer) | [FIXTURE_COMMAND_AUDIT.md](../runtime/FIXTURE_COMMAND_AUDIT.md) |
| Fade / PlayAVI / `DoScriptFrame` / `UseCamera` / `GamePause` at script layer | PROVEN | command map + PARITY 0b |
| `return` is interpreter stop | DISPROVEN | it is a `RemoveExtras` named-arg |

### Phase 1 in progress — boot / world clock (current master)

Recent commits (`8bccec3` … `1ebece6`) lock the retail pump, not
`00DBDE40`. Just locked: world+24 lerp (`6e1ff8e`), frontend New Game
message (`64a2e14`), frontend Present (`0d8f5e5`), map TNG load
(`1ebece6`).

| Item | Status | Evidence |
|---|---|---|
| Launch from PE `00401067` / WinMain `00403480` / bootstrap `00402510` | PROVEN | `EngineLifecycleTests.Pe_entry_is_crt_not_new_game` |
| New Game is leave-frontend → `FinalAlbion.wld` → Init Game `004184BD` | PROVEN | same + `New_game_is_leave_frontend_then_FinalAlbion_wld` |
| Init World Map `005066E0`; Load `.wld` `00507C30` token switch | PROVEN | `Load_wld_is_00507C30_not_00DBDE40` |
| Region graph `00506D40` / `00828710` | PROVEN | `Install_banks_and_startup_videos_exist` |
| Create Players: 5 × `0x22C` slots, 4 active — not `hero_swap_*.tng` | PROVEN | `CreatePlayers_is_five_0x22C_slots_not_hero_swap` |
| Load GTNG `0050959F` stem+`.gtng`; TLC missing → skip | PROVEN | same install test (`Gtng` null, “missing”) |
| Global things: BSS `[0x13B8609]=0` → per-map `.tng` `004FDBC0`; flag → `.gtg` `004FE2A0` | PROVEN | `Gtng_is_stem_gtng_gtg_is_004FE2A0_single_file` |
| Current region index is `WorldMap+156` (`004FB150`); ctor 0 is dummy | PROVEN | `Game_pump_is_004189C2_not_00DBDE40` |
| First `004189C2` pump does **not** `SetRegionAsLoaded` | PROVEN | install test |
| No-save enqueue `00501450` → `00500540(1,0,0)` **LookoutPoint** (native index 1) | PROVEN | install test (`CurrentRegionIndex=1`) |
| Persist `PlayerRegionName` `00487C20` / `00449E60` loads named region (e.g. `StartOakVale` = 4) | PROVEN | `Persist_PlayerRegionName_is_00487C20_not_new_game` |
| `SetRegionAsLoaded` `004FC8A0` writes `+156`, then `00B42750` mode 1 | PROVEN | `e9952b8` / install test |
| `OpenStaticMap` `00B42530` STB height + compiled `.lev` (v25 / `0x1904`) | PROVEN | `7869e8e` / install test |
| `00418289` skips player/world until `[player+9826]`; `00417001` skips camera while `WorldFrame<=1` | PROVEN | `7cc44c0` / `Update_00418289_*` / `Render_00417001_*` |
| `WorldFrame` inc at `004A5E10` via `0049DFB0` type-1 (`00629270` / `004A5A40`) | PROVEN | `ced722f` / `WorldFrame_004A5E10_unblocks_004164E0` |
| Camera body `004164E0` steps `arg/15` when `[0x13B8630]>0` | PROVEN | `6d7545a` |
| `0041707E` interpolates when catchup ticks are 0 (default New Game) | PROVEN | `c3be891` |
| `006B4900` world+24 slots; `006B42F0` lerps `+6296/+6312/+6328` into `ScriptedCamera` | PROVEN | `6e1ff8e` / `World_camera_006B4900_slots_lerp_into_ScriptedCamera` |
| Frontend New Game click: `0059A238` msg 15 → `[retail+41]=1` → Leave `0042F2A2` | PROVEN | `64a2e14` / `Frontend_00595582_new_game_message_leaves_without_RequestNewGame` |
| Menu built at `00595B24` (`UI_TEXT_NEW_GAME` id=0); not `00DBDE40` | PROVEN | same |
| Frontend frame `0042EC7C`: input `0042E3EE` → fill → draw `0042DF9E` (BeginScene / UI vtbl+8 / EndScene / Present) | PROVEN | `0d8f5e5` / `Frontend_0042EC7C_frame_is_input_then_0042DF9E_Present` |
| Same Present as PlayAVI (`009BEEB0`); extra `.wmv` after draw skipped (`00595A03` always 0) | PROVEN | same |
| `006C2170` Loading objects → `00522720` / `00521AE0` current-map `.tng` (LookoutPoint on no-save) | PROVEN | `1ebece6` / `Loading_objects_00521AE0_loads_LookoutPoint_tng` |
| Game pump / first region is `00DBDE40` / StartOakVale setup | DISPROVEN | tests above |
| No-save writes `[record+36]` | DISPROVEN | `recover-record36` text in `Camera_004164E0_runs_on_install_after_WorldFrame`; null still loads |

**Correction vs a “first region is Oakvale” reading:**
`WorldMap+156` is the *index field*. No-save New Game’s first real
write is **1 = LookoutPoint**. First-scene *view* is still
`StartOakValeWest` (PARITY / first-scene contract). Persist name
`StartOakVale` is index **4**. The retail New Game *click/message*
is now **PROVEN** (`0059A238` / `[retail+41]`). Who writes persist
`PlayerRegionName` is still **UNREAD**. `[esi+42]` load/save is
**UNREAD**. `00521AE0` is per-map TNG, not global-things apply.

---

## What’s left

Dependency order. Reorder only if a ledger or test disagrees.

### 1. Finish boot / clock (now)

Keep locking today’s exe facts. Do not invent an Oakvale write on
the no-save path.

| Left | Status | Notes |
|---|---|---|
| First non-null `[NewRegion record+36]` writer | UNREAD | Null is the native no-save state |
| `[0x13B8630]` catchup-tick writers | UNREAD | 3 immediate sites; default 0 |
| `0041714D` when `world+164 != 0` | UNREAD | Default New Game is `world+164==0` |
| Slot fields beyond `+6296/+6312/+6328` (weights / `+6340/+6352`) | UNREAD | Lerp into `ScriptedCamera` is PROVEN; leftover slot bodies are not |
| `00435530` display apply | PARTIAL | Thunk `00435F70` is the jmp |
| Who writes persist `PlayerRegionName` on New Game | UNREAD | Click/message path is PROVEN; persist HEADER writer is not |
| `[esi+42]` load/save | UNREAD | `recover-00595582`; `[esi+41]` Leave is PROVEN |
| Global-things *use* after `004FDBC0` / `.gtg` parse | UNREAD | Load switch is PROVEN; `00521AE0` is per-map TNG, not this apply |
| GTNG file body | N/A on TLC | Missing skip is PROVEN |
| MiniMap `0082BA00` / villages `005064C0` bodies | UNREAD | Named from `SetRegionAsLoaded`; not claimed as runtime |
| Wire persist-Oakvale (or a proven New Game region write) to `FirstSceneWorld` | UNREAD | Host first-scene lists are a separate reconstructed path |

Until `WorldFrame` ticks and camera/player gates open, the intro
fiber has no native clock.

### 2. First-scene intro fiber (apply / runtime)

Script-layer walk of `CS_OAKVALE_INTRO_FATHER` is already **Finished
def+60**. Leftover is apply/runtime, not “map the next unread
opcode.” Last persist-vector-0 command:
`Hero.SneakTo MK_OVIF_HERO5,0.0,TRUE`
(`RegionTravel.IntroCutsceneLastCommand`).

| Leftover on this fiber | Status | Where |
|---|---|---|
| `PlayAnimation` pose (`vtbl+72` / CTC `+68` `00686920` stub; `0070D580` not this path) | PARTIAL | COMMAND_MAP, PARITY 0b |
| `Create` `008A9100` / `Remove` `004C9B80` mesh | UNREAD | PARITY 0b “next unread” |
| Skip-key bodies / vector 1 | UNREAD | first-seen skip does not fire |
| Who writes `[quest+80]` / `AttackOver` after `vtbl+2584(12)` + `HerosOldHouse` | UNREAD | not a `mov` in `00DBDE00–00DBF000` |
| `LookToThing` / `LookInDirection` heading bodies | PARTIAL | record + yield / no yaw write |
| `MuteSounds` apply; `DoCameraPreloading` `vtbl+1560/1568` | PARTIAL | |
| Dialogue UI (`Speak` / `InteractiveSpeak` / `DialogSpeak`) | PARTIAL | one yield; no invented UI |
| `SneakTo` / `WalkTo` mesh move (`004C72B0` stub) | PARTIAL | `FirstSeenSneakToAppliesMove=false` |
| `PlayCombatAnimation` pose | PARTIAL | `vtbl+76` does not read the name |
| `call [vtbl+8]` resume site; `vtbl+28` yield body; `Main` `00CDD440` | UNREAD | PARITY 0b |

`DoScriptFrame` / `PlayAVI` / cameras / fades are **PROVEN** at the
script layer. Do not invent fade/AVI/wake playback beyond those
bodies.

### 3. First-scene render leftovers

Only items already listed. No visual guesses.

From [FIRST_SCENE_AUDIT.md](../render/FIRST_SCENE_AUDIT.md)
“UNREAD leftover (do not invent)” and the contract STATE table:

- D3DSAMP MAG/MIN/MIP/ADDRESS / anisotropy / LOD bias
- sRGB vs linear on the sampled view
- `ALPHATESTENABLE` / `ALPHAREF` / `ALPHAFUNC`
- `FILLMODE` / `COLORWRITEENABLE` first-seen writes
- `ZENABLE` / `ZWRITE` SetRenderState site
- stencil, scissor, half-pixel
- 2D / ortho and cinematic-vs-gameplay leftover projection
- sky PS `c0/c1/c2` bank values and quality bit
- water mesh *when bind would run* (first-seen draw is empty-out)
- particles / HUD / shadows / `0x400000` sky (not submitted)

TEMPORARY stand-ins (LINEAR/REPEAT, MaxLod=1, Z test+write on) stay
classified. PlayAVI *script* apply is PROVEN; leftover timing vs
Steam is PARITY Open item 0, not a first-scene 3D invent.

### 4. Animation (after boot)

README item 1, *after* a ticking world. First-seen wake lines
(`CS_WAKING_UP_*`) need the clock; create still has
`FirstSeenPlaysAnim=false`.

| Left | Status |
|---|---|
| Animation resource lookup | UNREAD / PARTIAL |
| Clip evaluation | UNREAD |
| Skeletal pose | UNREAD (`FirstSeenPlayAnimationAppliesPose=false`) |
| PALSKIN dest beyond bind-pose identity | first-seen dest ≈ identity is PROVEN; play-anim product is not |

### 5. Later (README order, after first-scene boot)

| # | Area | Status |
|---|---|---|
| 2 | Movement / navigation / entity task completion | PARTIAL (dest+gait recovered; `vtbl+20` stub) |
| 3 | Dialogue / voice / audio lifecycle | PARTIAL (session recorded; UI/voice unread) |
| 4 | Generic quest lifecycle beyond `S_QNOVI` | PARTIAL |
| 5 | Create / remove ownership | PARTIAL (script record; mesh unread) |
| 6 | Runtime effects / lights / state → Vulkan | PARTIAL |
| 7 | Remaining world / shader / state | see phase 3 |

---

## Out of scope for this page

- Optional enhancements (HDR, upscale, DLSS)
- Linux / non-DirectShow `PlayAVI`
- Grinding Crowd\* / debug / `LadyGreyIntro` / boss UNREAD tokens
- Making scenes “look like Fable”

---

## Phase check vs 2026-08-18 hypothesis

The boot-first sequence holds. Corrections from the repo:

1. **No-save first real region is LookoutPoint**, not Oakvale.
   Oakvale is persist-name index 4 or the first-scene *view* contract.
   New Game *click* is PROVEN; persist `PlayerRegionName` writer is not.
2. **GTNG is not an unread file on TLC** — missing skip is PROVEN.
   `00521AE0` loads the current map `.tng`. Remaining UNREAD is
   global-things *use* after `004FDBC0` / `.gtg` parse, plus
   `[record+36]` and catchup-tick writers.
3. **Intro opcodes are already walked.** Phase 2 is apply/runtime
   leftovers on that fiber, not “finish UNREAD tokens.”
4. **README animation-first is the long-term engine list**, not
   today’s master. Animation stays after the clock.

When this page and a ledger disagree, the ledger wins.
