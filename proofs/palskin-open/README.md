# First PALSKIN open after Leave Frontend

Investigation only. Production `src/` was not edited.

Question: when does native first **open** a PALSKIN resource after
Leave? Is that `PalskinPipelineTests` (father / kid / `SHADERS_PALSKIN`)
or Lookout adult **4299**? Frontend must not load hero skins.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**DIVERGE** / **LEFTOVER**.

Sources: `docs/status/investigations/2026-08-18-palskin.md`,
`E-player-palskin.md`, `2026-08-18-resource-manager.md`,
`2026-08-18-first-scene-things.md`; `docs/runtime/FORWARD_TREE.md`
§§4, 7, 14–15; `docs/PARITY.md` shader banks; `PalskinPipelineTests.cs`;
`MeshBank.cs` / `EngineLifecycle.cs` (read only); listings
`00A26D40` / `00A243B0` / `00A8FD40` / `00B3CB30` / `00B3B6D0`;
`EngineLifecycleTests.Install_banks_and_startup_videos_exist`.

Siblings: `proofs/xseq-first` (type-6, not this),
`proofs/c3d-first-submit` (static `0x18` DIP),
`proofs/texture-library-open` (GBANK_MAIN after Leave).

---

## Verdict

**First PALSKIN *C3D payload* is after Leave + Lookout spawn, id 4299
`MESH_HERO`. Not frontend. Not father. Not kid 4300.**

Three different “PALSKIN opens” exist. Only the third is a hero skin.

| What | First native site | Relative to Leave |
|---|---|---|
| `SHADERS_PALSKIN` **name** on shader manager | `00B3CB30` → `00B3B6D0(3, "SHADERS_PALSKIN")` at Init Engine | **before** Leave **PROVEN** (tokens, not C3D) |
| `MBANK_ALLMESHES` **directory** (type 5 indexed) | `004A6E30` → `0049E620` → `009D56C0` / `009CFBC0` | **after** Leave **PROVEN** (`ParsedCount=0`) |
| Type-5 **blob** `00A26D40` / `00A89450` / `00A8FD40` | first `00A243B0` miss on Graphic **4299** | **after** Leave + `006AC910` **PROVEN** as first hero; first draw site **PARTIAL** (host `Meshes.Get` at submit) |

Frontend never opens `MBANK_ALLMESHES`, never `00A26D40`, never
`Meshes.Get(4299|4300)`. Press Start `009AD410` is UI Type=10, not a
mesh handle.

`PalskinPipelineTests` **MATCH** the exe **format** for father 20/4
and kid 28/`0x14` plus the `00BCFB00` `c38` contract. They are
**DISPROVEN** as the first-open fixture after Leave. Adult 4299
prim0 is stride **36** / flags **22** / group **9** — **not** in that
test. `WorldShading.FirstSeenPalskinStrideBytes=28` is a **kid
LEFTOVER** name vs no-save Present.

---

## Path from Leave (no-save New Game)

```
0042EC7C retail
  Init Engine 0042E204                         // still before Leave
    00B26340 engine
    00B3CB30 shader manager 0x1436E98
      00B3B6D0 ebx, "SHADERS_STATIC"
      00B3B6D0 3,    "SHADERS_PALSKIN"         // NAME + tokens
      00B3B6D0 2/4   STATIC_BUMP / PALSKIN_BUMP
    00BD27F0 register family types 0x9/0xB/0xD // no C3D
    00BD01B8 family ctor slot0
             VSHADER_PALSKIN_DIRLIGHT_FOG      // program object
  0042DF9E 2D UI  (type 0x22 / VSHADER_2D_SPRITE)
    009AD410 UI def Type=10                    // NOT MBANK
0042F2A2 Leave frontend
0042F491 Init Game → 004184BD
  Init World 004A6E30
    0049E620 "Opening Mesh Bank"               // FIRST graphics.big MESH
      00A09F20 "MBANK_ALLMESHES"
        miss → 00A27030 size 0x460
        vtbl+4 009D56C0 → 009CFBC0 directory
        type 5 in the index; 00A26D40 not called
      004BBFD0 [0x13B8A04]
  00416953 Load world FinalAlbion.wld
004189C2 first pumps  (no region; no C3D)
later 00501450 LookoutPoint
  006C2170 ContainsMap TNG
    Graphic apply 004CA010 / 009AD410 HANDLE   // still no parse
  0051FD80 / 006AC910 CREATURE_HERO Graphic 4299
then first 00A243B0(id=4299)
  miss → vtbl+48 00A26D40
    cmp type, 1/2/4/5 → 96-byte C3D record     // FIRST PALSKIN payload
    00A89450 serialize / 00A8FD40 primitive
      adult prim0 stride 36 flags 22 group 9
  dest bind-identity (FirstSeenPlaysAnim=false)
```

`CREATURE_HERO_FATHER` / `CREATURE_HERO_CHILD` / 4300 /
`CAM_OVIF_SHOT2` are **not** on this list. **PROVEN**.

---

## 1. What “open” is (and is not)

| Layer | VA | Parses C3D? | Hero skin? |
|---|---|---|---|
| Shader manager register | `00B3B6D0` slot 3 | **No** | **DISPROVEN** |
| Family types `0x9`/`0xB`/`0xD` | `00BD27F0` | **No** | **DISPROVEN** |
| MESH directory | `0049E620` / `009CFBC0` | **No** (`009CFBC0` reads ids/types/offsets only) | **DISPROVEN** |
| `009AD410` name → 12-byte handle | thing apply / UI | **No** | **DISPROVEN** |
| Get-or-load | `00A243B0` vtbl+52 | on miss | first 4299 |
| Payload | `00A26D40` vtbl+48 type 5 | **Yes** 96-byte record | **PROVEN** slot |
| File serialize | `00A89450` / `00A8FD40` | **Yes** verts / bones / group | **PROVEN** format |
| Dest pack / upload | `00BD2D90` / `00BCFB00` | uses dest, not a second file open | later DIP |

`00A26D40` (`listing-00a00000.txt`):

```
cmp ebx, 1
je  00A26DE6
cmp ebx, 2
je  00A26DE6
cmp ebx, 4
je  00A26DE6
cmp ebx, 5          ; PALSKIN / animated C3D
jne 00A26E7F        ; type 3 / 6–10
00A26DE6  … 96-byte record at bank+908 (vtbl 0129CD3C)
```

Type 5 is the **same** C3D record as static 1/2/4. Type 6 is XSEQ
(`proofs/xseq-first`) and is **not** first-seen here.

---

## 2. Frontend must not load hero skins

| Claim | Class | Evidence |
|---|---|---|
| Frontend Present is 2D (`0042DF9E` / type `0x22` / `VSHADER_2D_SPRITE`) | **PROVEN** | FORWARD_TREE §4; `PumpFrontendFrame` |
| `MBANK_ALLMESHES` during `0042EC7C` | **DISPROVEN** | first `0049E620` is Init World after Leave |
| `009A8150` includes the mesh bank | **DISPROVEN** | GBANK/PARTICLE pairs only |
| Press Start `009AD410` opens 4299/4300 | **DISPROVEN** | `0041D21B` `[def+60]` Type=**10** UI |
| Frontend UI type 6 is a C3D / XSEQ | **DISPROVEN** | `0054EF00` glyphs |
| `00A26D40` / `00A89450` on frontend | **DISPROVEN** | no mesh bank yet |
| `InitFrontendUi` / `PumpFrontendFrame` call `OpenMeshBank` / `Meshes.Get` | **PROVEN** absence | `EngineLifecycle` frontend body |
| Host `FirstSceneWorld.LoadMesh` / `TracePalskin` on frontend | must not; that helper is Oakvale **LEFTOVER** | not on `PumpFrontendFrame` |
| Attract / `CS_ATTRACT_*` loads hero C3D | **DISPROVEN** | no StartCutscene on retail |

**Answer:** frontend may already have `SHADERS_PALSKIN` **named** on
the shader manager (Init Engine is still retail). That is not a hero
skin. Do not `MeshBank.Open`, `Meshes.Get(4299|4300)`,
`LoadCreature(CREATURE_HERO*)`, or `FirstSceneWorld` during
`PumpFrontendFrame`.

---

## 3. Shader bank vs C3D (before Leave)

Init Engine `0042E204` (retail, **before** `0042F2A2`):

```
00B3D206  "SHADERS_STATIC"     call 00B3B6D0
00B3D233  "SHADERS_PALSKIN"    push 3; call 00B3B6D0
          "SHADERS_PALSKIN_BUMP" (slot 4)
00BD01B8  family slot 0 = VSHADER_PALSKIN_DIRLIGHT_FOG
00BD27F0  types 0x9 / 0xB / 0xD
```

`shaders.big` tokens are the shader-manager directory, same layer as
`PIXEL_SHADERS` / `SHADERS_POINT_SPRITE1` (frontend 2D uses the
latter). **PROVEN** name bind. Whether every `SHADERS_PALSKIN`
program is decoded at ctor vs first bind is **PARTIAL**; it is still
**not** `graphics.big` 4299.

Host frontend DIP banks (`Dx9VulkanFrontend`) are
`SHADERS_POINT_SPRITE1` / `PIXEL_SHADERS`. **MATCH** skip of
`VSHADER_PALSKIN_*` on frontend frames.

---

## 4. First MESH directory after Leave

`004A6E30` `"Init Mesh Bank"` `0049E620` is the first
`graphics.big` MESH bind. Directory only. `MeshBank.Open`:
`Opened=true`, `EntryCount` large, `ParsedCount=0`. Type 5 ids sit
in the index. **PROVEN**.

`009AD410` on TNG Graphic / `006AC910` is handle-only.
`PresentWorld(expandGeometry:false)` keeps that. **PROVEN**.

---

## 5. First PALSKIN C3D payload — 4299

No-save Lookout:

| Item | Value | Class |
|---|---|---|
| Creature | `CREATURE_HERO` after `PLAYER_HERO` miss | **PROVEN** |
| Graphic | **4299** `MESH_HERO` | **PROVEN** |
| Bones / prims | 77 / 19 | **PROVEN** file |
| Prim0 | stride **36**, flags **22**, group **9** (torso_back) | **PROVEN** file / **PARTIAL** vs tests |
| Prim 16+18 | stride **28**, flags **20** (mouth / eye shadow) | **PROVEN** file |
| Create plays DEFAULT | no (`005B37F7` not on `006AC910`) | **PROVEN** |
| First dest | bind identity | **PROVEN** |
| Submit palskin set | **`[4299]` only** | **PROVEN** (`2026-08-18-first-scene-things`) |
| Lookout AICreature C3Ds | exist; **not** submitted | **PROVEN** exist / **DISPROVEN** as first payload |
| Kid 4300 / father | Oakvale leftover | **DISPROVEN** as this open |
| Clothing Graphic 4126 | folded hat trap | **DISPROVEN**; omit |

Host first parse: `SubmitCurrentWorld` → `Meshes.Get(4299)` among
the 45 primary ids (193 instances + hero already in the list).
Native first parse is the same id at first `00A243B0` miss
(draw/submit). Exact native caller of that first miss is **PARTIAL**
(thing construct vs first `00BD71B0`). Id and file are **PROVEN**.

---

## 6. `PalskinPipelineTests` vs exe

`tests/Fable.Formats.Tests/PalskinPipelineTests.cs` opens
`graphics.big` itself (`LoadCreature`) and, in one fact,
`shaders.big` / `SHADERS_PALSKIN` / `VSHADER_PALSKIN_DIRLIGHT_FOG`.
That is a **format** lock, not `EngineLifecycle` New Game.

### MATCH exe

| Test / constant | Exe |
|---|---|
| `C3dPrimitiveSerialize = 00A8FD40` | primitive serialize |
| `C3dAnimatedBlockWrite = 00A8E770`; +23 count, +24 ids | `00BCFB00` reads the same |
| `PalskinGpuAddressOffset(i) = i*3` | `a0` into `c[38+a0]` |
| Father stride 20 / flags 4 / packed pos | `00A8FD40` file fields |
| File blend byte `% 3 == 0`; `group[byte/3]` | register offset, not mesh bone |
| `PackSubsetRegisters` ↔ `EvaluatePalskinVsPosition` | dest 3×4, no 4th row |
| Kid samples stride 28 / posSize 12; father 20 / 4 | two decls, not one FVF |
| VS inputs `v0..v4`; `TryGetPalskinA0RelativeC38`; WVP `c5–c8` | `VSHADER_PALSKIN_DIRLIGHT_FOG` |

### DISPROVEN / LEFTOVER as *first open after Leave*

| Test fixture | First-seen after Leave | Class |
|---|---|---|
| `LoadCreature(LiveFatherCreature)` | father not in Lookout submit | **LEFTOVER** (Oakvale SHOT2) |
| `LoadCreature(KidCreature)` → 4300 | adult 4299 | **LEFTOVER** |
| `FirstSeenPalskinStrideBytes = 28` | adult prim0 **36** | **LEFTOVER** name |
| `Father_skinned_cm_through_shot2_wvp_*` | Lookout `006B3FF0`, not SHOT2 | **LEFTOVER** camera |
| Independent `BigArchive.Open(graphics.big)` | one `MeshBank` after `0049E620` | test-only; production must not |
| Independent `shaders.big` open in the test | Init Engine already named the bank | test-only |

### UNTESTED vs first-seen 4299

Adult prim0 group `{4,6,7,11,13,14,15,36,35}` → `c38–c64` (27
float4s). Prim 18 one bone 57 → `c38–c40`. Host
`WorldShading.PackSubsetRegisters` is the right helper; **no**
`PalskinPipelineTests` fact loads 4299.

---

## 7. Host vs native

| Host | Native first-seen | Class |
|---|---|---|
| `PumpFrontendFrame` does not `OpenMeshBank` / `Get` | no MESH / no type 5 | **MATCH** |
| `EnterGame` → `Init Mesh Bank` → `MeshBank.Open` directory | `0049E620` | **MATCH** |
| `PresentWorld` handles only | `009AD410` | **MATCH** |
| `SubmitCurrentWorld` `Meshes.Get` 45 ids incl. 4299 | `00A243B0` / `00A26D40` type 5 | **MATCH** timing; flatten after is **DIVERGE** (E) |
| `SubmittedPalskinMeshIds == [4299]` | one PALSKIN Graphic | **MATCH** set |
| `SubmittedHeroPalskin` = `BoneCount>0` | not `00BD71B0` | **DISPROVEN** as draw (keep as membership only) |
| `FirstSceneWorld.TracePalskin` father | not Leave / not Lookout | **LEFTOVER** |
| `WorldGeometry` kid clone on `IsPrimaryStart` | Lookout already has adult Hero | **DISPROVEN** as this path |
| Test `LoadCreature` during frontend | native does not | **must not** in production |

---

## Classification table

| Claim | Status |
|---|---|
| `SHADERS_PALSKIN` registered at Init Engine (`00B3B6D0` push 3) | **PROVEN** |
| That register opens Graphic 4299 | **DISPROVEN** |
| First `graphics.big` MESH open is `0049E620` after Leave | **PROVEN** |
| That directory parses type-5 C3Ds | **DISPROVEN** |
| `00A26D40` type 5 is the C3D payload (same 96-byte record as 1/2/4) | **PROVEN** |
| First no-save type-5 payload is **4299** `MESH_HERO` | **PROVEN** |
| First payload is father / kid 4300 / 4126 | **DISPROVEN** |
| Create / first Present plays a clip before dest | **DISPROVEN** (`FirstSeenPlaysAnim=false`) |
| Frontend loads hero skins | **DISPROVEN** |
| Frontend `009AD410` is MBANK | **DISPROVEN** (UI Type 10) |
| `PalskinPipelineTests` lock `00A8FD40` / `00BCFB00` / father+kid decls | **PROVEN** format |
| Those tests are the first-open after Leave | **DISPROVEN** |
| Adult 4299 36/22 in `PalskinPipelineTests` | **DISPROVEN** (absent) |
| Lookout submit palskin set is only 4299 | **PROVEN** |
| Native first `00A243B0` *caller* for 4299 | **PARTIAL** |
| `SHADERS_PALSKIN` token decode at ctor vs first DIP | **PARTIAL** |

---

## Do not

- Open `graphics.big` / `MeshBank` / `Meshes.Get` on frontend frames.
- Load `CREATURE_HERO_CHILD` / 4300 / father as the first PALSKIN after Leave.
- Treat `PalskinPipelineTests.LoadCreature` as `EngineLifecycle` New Game.
- Treat `FirstSeenPalskinStrideBytes=28` as the Lookout adult decl.
- Submit Graphic **4126** as hair / hat / beard.
- Parse every MESH id at `0049E620`.
- Call `SHADERS_PALSKIN` registration a hero-skin open.
- Invent a second `graphics.big` dump beside `MeshBank`.

Next recoverable slice is still per-prim PALSKIN records + `c38`
for **4299** (investigation E), not widening the test fixture to
run during frontend.
