# First PALSKIN open after Leave `0042F2A2`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `CAM_OVIF_SHOT2` / father /
`CREATURE_HERO_CHILD` / Graphic **4300** / `00DBDE40`.
Those are later leftover `Q_NewOakValeIntro`, not Leave /
Init Game / first no-save Present.

Do **not** treat frontend 2D (`0042DF9E`) or
`SHADERS_PALSKIN` **name** bind as a hero-skin open.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**DIVERGE** / **LEFTOVER**.

Sources: `docs/runtime/FORWARD_TREE.md` §§4, 14–15;
`docs/PARITY.md` New Game / Leave;
`docs/status/investigations/E-player-palskin.md`,
`2026-08-18-palskin.md`, `2026-08-18-first-scene-things.md`;
`EngineLifecycle.cs` / `MeshBank.cs` (read only);
`EngineLifecycleTests` (`New_game_is_leave_frontend_then_FinalAlbion_wld`,
`Install_banks_and_startup_videos_exist`,
`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`);
listings `00B3B6D0` / `0049E620` / `00A09F20` / `00A26D40` /
`00A243B0` / `006AC910`.

Siblings: `proofs/palskin-open` (payload + `PalskinPipelineTests`),
`proofs/hero-appearance-first` (Graphic attach),
`proofs/c3d-first-submit` (static `0x18`, not PALSKIN),
`proofs/camera-after-leave` (same Leave spine),
`proofs/texture-library-open` (`GBANK_MAIN` after Leave),
`proofs/xseq-first` (type 6, not type 5).

---

## Verdict

**First PALSKIN *C3D open* after Leave `0042F2A2` is Lookout adult
Graphic 4299 `MESH_HERO`.** Not frontend. Not father. Not kid 4300.

Three different “PALSKIN opens” exist. Only the third is a skin.

| What | First native site | Relative to Leave `0042F2A2` |
|---|---|---|
| `SHADERS_PALSKIN` **name** | Init Engine `00B3CB30` → `00B3B6D0(3, "SHADERS_PALSKIN")` | **before** Leave **PROVEN** (tokens, not C3D) |
| `MBANK_ALLMESHES` **directory** | Init World `004A6E30` → `0049E620` / `00A09F20` / `009D56C0` | **after** Leave **PROVEN** (`ParsedCount=0`) |
| Type-5 **blob** `00A26D40` | first `00A243B0` miss on Graphic **4299** | **after** Leave + `006AC910` **PROVEN** id; first DIP caller **PARTIAL** |

Frontend never opens `MBANK_ALLMESHES`, never `00A26D40`, never
`Meshes.Get(4299|4300)`. Leave Present is `009BE420` + `009BEEB0`
(black). **PROVEN**.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  Init Engine 0042E204                    // still before Leave
    00B26340 engine
    00B3CB30 shader manager
      00B3B6D0 3, "SHADERS_PALSKIN"       // NAME + tokens
    00BD27F0 types 0x9 / 0xB / 0xD        // no C3D
    00BD01B8 family slot0
             VSHADER_PALSKIN_DIRLIGHT_FOG // program object
  0042DF9E 2D UI (type 0x22 / VSHADER_2D_SPRITE)
    009AD410 UI Type=10                   // NOT MBANK
0042F2A2 Leave frontend                   // not 00DBDE40
  009BE420 clear + 009BEEB0 Present
0042F491 Init Game → 00418DCA → 004184BD
  Init World 004A6E30
    0049E620 "Opening Mesh Bank"          // FIRST graphics.big MESH
      00A09F20 "MBANK_ALLMESHES"
        miss → 00A27030 size 0x460
        vtbl+4 009D56C0 → 009CFBC0 directory
        type 5 in the index; 00A26D40 not called
      004BBFD0 [0x13B8A04]
  00416953 Load world FinalAlbion.wld
004189C2 first pumps                      // no region; no C3D
later 00501450 LookoutPoint
  006C2170 ContainsMap TNG
    Graphic apply 004CA010 / 009AD410 HANDLE
  0051FD80 / 006AC910 CREATURE_HERO Graphic 4299
then first 00A243B0(id=4299)
  miss → vtbl+48 00A26D40                 // FIRST PALSKIN payload
    cmp type 1/2/4/5 → 96-byte C3D record
    00A89450 / 00A8FD40
      adult prim0 stride 36 flags 22 group 9
  dest bind-identity (FirstSeenPlaysAnim=false)
```

`CREATURE_HERO_FATHER` / `CREATURE_HERO_CHILD` / 4300 /
`CAM_OVIF_SHOT2` are **not** on this list. **PROVEN**.

---

## 1. PALSKIN during frontend besides 2D UI?

| Claim | Class | Evidence |
|---|---|---|
| Frontend Present is 2D (`0042DF9E` / type `0x22` / `VSHADER_2D_SPRITE`) | **PROVEN** | FORWARD_TREE §4; `PumpFrontendFrame` |
| `SHADERS_PALSKIN` named at Init Engine (still retail) | **PROVEN** | `00B3B6D0` push 3 |
| That name bind opens Graphic 4299 | **DISPROVEN** | tokens only; no `graphics.big` MESH |
| `MBANK_ALLMESHES` during `0042EC7C` | **DISPROVEN** | first `0049E620` is Init World after Leave |
| Press Start `009AD410` opens 4299/4300 | **DISPROVEN** | `0041D21B` `[def+60]` Type=**10** UI |
| Frontend UI type 6 is C3D / XSEQ | **DISPROVEN** | `0054EF00` glyphs |
| `00A26D40` / `00A89450` on frontend | **DISPROVEN** | no mesh bank yet |
| `PumpFrontendFrame` / `InitFrontendUi` call `OpenMeshBank` / `Meshes.Get` | **PROVEN** absence | `EngineLifecycle` frontend body |
| Attract / `CS_ATTRACT_*` loads hero C3D | **DISPROVEN** | no `StartCutscene` on retail |
| Leave itself parses type 5 | **DISPROVEN** | `0042F2A2` is audio/UI teardown + black Present |

**Answer:** no hero skin during frontend. Shader-manager **name**
exists after Init Engine. That is not a PALSKIN open.

---

## 2. First PALSKIN after Leave `0042F2A2`

Leave is **not** the open. It only gates Init Game.

### Directory (first `graphics.big` MESH)

`004A6E30` `"Init Mesh Bank"` `0049E620` is the first MESH bind.
Host `EnterGame` → `InitWorldInitStages` → `OpenMeshBank()`.
`MeshBank.Open`: `Opened=true`, `EntryCount` large, `ParsedCount=0`.
Type 5 ids sit in the index. **PROVEN**. **DISPROVEN** as C3D parse.

`009AD410` on TNG Graphic / `006AC910` is handle-only.
`PresentWorld(expandGeometry:false)` keeps that. **PROVEN**.

### Type-5 payload (first hero skin)

No-save Lookout:

| Item | Value | Class |
|---|---|---|
| Creature | `CREATURE_HERO` after `PLAYER_HERO` miss | **PROVEN** |
| Graphic | **4299** `MESH_HERO` | **PROVEN** |
| Bank type | 5 (same 96-byte C3D record as static 1/2/4) | **PROVEN** |
| Bones / prims | 77 / 19 | **PROVEN** file |
| Prim0 | stride **36**, flags **22**, group **9** | **PROVEN** file |
| Create plays DEFAULT | no (`005B37F7` not on `006AC910`) | **PROVEN** |
| First dest | bind identity | **PROVEN** |
| Submit palskin set | **`[4299]` only** | **PROVEN** |
| Lookout AICreature C3Ds | exist; **not** first payload | **DISPROVEN** as first |
| Kid 4300 / father | Oakvale leftover | **DISPROVEN** as this open |
| Clothing Graphic 4126 | folded hat trap | **DISPROVEN**; omit |

`00A26D40`:

```
cmp ebx, 1 / 2 / 4 / 5
je  00A26DE6          ; 96-byte C3D record
; type 3 / 6–10 elsewhere (type 6 = XSEQ)
```

Native first parse is 4299 at first `00A243B0` miss.
Exact caller of that miss (thing construct vs first `00BD71B0`)
is **PARTIAL**. Id and file are **PROVEN**.

---

## 3. What “open” is not

| Layer | VA | Parses C3D? | Hero skin? |
|---|---|---|---|
| Shader manager register | `00B3B6D0` slot 3 | **No** | **DISPROVEN** |
| Family types `0x9`/`0xB`/`0xD` | `00BD27F0` | **No** | **DISPROVEN** |
| MESH directory | `0049E620` / `009CFBC0` | **No** | **DISPROVEN** |
| `009AD410` name → handle | thing apply / UI | **No** | **DISPROVEN** |
| Get-or-load | `00A243B0` vtbl+52 | on miss | first 4299 |
| Payload | `00A26D40` vtbl+48 type 5 | **Yes** | **PROVEN** slot |
| Dest pack / upload | `00BD2D90` / `00BCFB00` | uses dest | later DIP |

`PalskinPipelineTests` lock father 20/4 and kid 28/`0x14` plus
`00BCFB00` `c38`. That is **format**, not first-open after Leave.
Adult 4299 prim0 36/22 is **absent** from that fixture.
`WorldShading.FirstSeenPalskinStrideBytes=28` is a **kid
LEFTOVER** name vs no-save Present.

---

## 4. C# after Leave vs leftover

| Host | Native first-seen | Class |
|---|---|---|
| `PumpFrontendFrame` does not `OpenMeshBank` / `Get` | no MESH / no type 5 | **MATCH** |
| `RequestNewGame` = Leave `0042F2A2` only | no MESH | **MATCH** |
| `EnterGame` → Init Mesh Bank → `MeshBank.Open` | `0049E620` | **MATCH** |
| `PresentWorld` handles only | `009AD410` | **MATCH** |
| `SubmitCurrentWorld` `Meshes.Get` primary ids + 4299 | `00A243B0` / `00A26D40` type 5 | **MATCH** timing; flatten after is **DIVERGE** (E) |
| `SubmittedPalskinMeshIds == [4299]` | one PALSKIN Graphic | **MATCH** set |
| `SubmittedHeroPalskin` = `BoneCount>0` | not `00BD71B0` | **DISPROVEN** as draw (membership only) |
| `FirstSceneWorld.TracePalskin` father | not Leave / not Lookout | **LEFTOVER** |
| `WorldGeometry` kid clone on `IsPrimaryStart` | Lookout already has adult Hero | **DISPROVEN** as this path |

`SubmitCurrentWorld` is gated on `HeroSpawned`. Frontend never
reaches that. **PROVEN**.

---

## Classifications (short)

1. **Frontend PALSKIN C3D — DISPROVEN.** Shader name exists after
   Init Engine. Frontend DIP is 2D. Leave Present is black.
2. **First MESH directory after Leave — `0049E620` `MBANK_ALLMESHES`.
   PROVEN.** Directory only.
3. **First type-5 payload after Leave — Graphic 4299 `MESH_HERO`.
   PROVEN.** Scene is Lookout, not Oakvale. Native first
   `00A243B0` caller **PARTIAL**.
4. **Father / kid 4300 / 4126 / `PalskinPipelineTests.LoadCreature`
   as this open — DISPROVEN / LEFTOVER.**
5. **Host `SubmittedHeroPalskin` as `00BD71B0` character draw —
   DISPROVEN.** It only means 4299 is in the palskin id list.

## Do not

- Open `graphics.big` / `MeshBank` / `Meshes.Get` on frontend frames.
- Treat Leave `0042F2A2` itself as a PALSKIN parse.
- Load `CREATURE_HERO_CHILD` / 4300 / father as the first PALSKIN after Leave.
- Call `SHADERS_PALSKIN` registration a hero-skin open.
- Parse every MESH id at `0049E620`.
- Submit Graphic **4126** as hair / hat / beard.
- Treat `FirstSeenPalskinStrideBytes=28` as the Lookout adult decl.

Next recoverable slice is still per-prim PALSKIN records + `c38`
for **4299** (investigation E), not a frontend load.
