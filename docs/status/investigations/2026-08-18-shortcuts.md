# Reconstruction-era shortcuts on the live render / load path

Investigation only. No production source (`src/`, `tests/`) was modified.

**Scope:** active New Game Present after `HeroSpawned`. The live chain is
`EngineLifecycle.PumpGameUpdate` → `SubmitCurrentWorld` →
`PresentWorld` / `TessellateVisible` / `MeshBatches` → `EngineFrame`
Vertices → `SilkEngineHost.Present` `SetMesh` → `VulkanLineRenderer.Draw`.

**Not live:** `FirstSceneWorld` (Oakvale / `CAM_OVIF_SHOT2` audit),
`WorldGeometry.Build(expandGeometry: true)` default, `Expand` /
`ExpandPresentedWorld`, `TessellatePrimary`, `ToFineTriangles`, F2
`FlyCamera`, `SilkEngineHost` expanded-`Triangles` fallback.

Statuses used here: **PROVEN** / **VALID BACKEND ABSTRACTION** /
**TEMPORARY** / **DISPROVEN**.

A status applies to the *claim in the left column*, not to “this code
exists”. `DISPROVEN` means the host behaviour is **not** native-equivalent
(or is not the live path). `PROVEN` means the host matches a recovered
exe fact. `VALID BACKEND ABSTRACTION` is a Vulkan / CPU stand-in that
keeps native numbers. `TEMPORARY` is a reconstruction bridge still on
the live path.

Related: `A-dx9-submit.md`, `B-camera-matrices.md`,
`C-terrain-static-map.md`, `D-c3d-transforms.md`, `E-player-palskin.md`,
`G-dx9-vulkan.md`, `H-regression-audit.md`.

---

## Verdict

The live 3D path is still the `f63b741` bridge: one-shot CPU flatten of
visible LEV cells + every primary C3D (PALSKIN dest included) into one
`TexturedMesh`, then one `SetMesh`. Native never does that.

Several “shortcuts” named in the brief are **not** host inventions:

| Named fear | Live fact |
|---|---|
| Hardcoded `LookoutPoint` region | **PROVEN** native no-save index 1 (`00501450`). Not a string seed. |
| Magic `× 0.01` C3D scale | **PROVEN** cm → metres. |
| Host landscape `W = I` | **VALID BACKEND ABSTRACTION** (STB verts already world-space). Native `T(cam)` on that VB is **DISPROVEN**. |
| Unlit dark-green | **PROVEN** first-seen leftover `c3=(0,0.125,0)` then PS `mul_x2`. Do not invent ambient. |
| Landscape `oT1=(0,0)` | **PROVEN** first-seen (unwritten `c40`/`c41`). Ugly, but not a host UV guess. |
| First-seen water empty | **PROVEN** ctor zeros / missing intern. |
| `.gtg` LoadAll | **DISPROVEN** as live (BSS `0x13B8609` default 0 → per-map `.tng`). |

The remaining live shortcuts are the soup, guessed landscape ids,
olive miss-tex, ctor dirlight frozen as “the” light, hero-eye camera
when manager V0 is still the ctor axis, bind-pose PALSKIN bake, missing
sky, primary-only props, and one-shot submit.

---

## Live chain (what actually runs)

```
EnqueueAfterDummy
  persist empty → 00501450 RequestLoadRegion(1)     // WLD index 1 = LookoutPoint
006C2170 ContainsMap TNG + SpawnHeroFromPlayerStart
  HOLY_SITE GuildArrivalHSP → 006AC910 CREATURE_HERO / 4299
  WorldCamera.SeedHero (006B3FF0 / 006B2CA0 axes)
PumpGameUpdate after OpenStaticMaps, before 00435530
  if (HeroSpawned && !WorldSubmitted) SubmitCurrentWorld()
    PresentWorld: WorldGeometry.Build(expandGeometry: false,
                                      adjacentStaticMaps: false,
                                      onlyMaps: OpenedStaticMaps)
    land  = MeshBatches.Build(opened.TessellateVisible(planes))
    props = MeshBatches.BuildMeshes(primary instances + hero fallback)
    SubmittedMesh = Concat(land, props)
    BindSubmittedTextures → GpuTexture[] on EngineFrame
SilkEngineHost.Present: SetTextures + SetMesh once (ref-equal skip)
SilkEngineHost.Draw: native 1024×768 aspect, three WVPs
  ViewProjection / SkyViewProjection / HostLandscapeViewProjection
VulkanLineRenderer: one GLSL mesh pair, pass.x = 0/1/2/3
```

`WorldSubmitted` is never cleared. The frustum used at first submit is
frozen.

---

## Named shortcuts

### 1. Hardcoded LookoutPoint

**PROVEN** as native no-save first real region. **DISPROVEN** as a
production string seed.

- `EngineLifecycle.LoadFromFirstRealRegion` calls `RequestLoadRegion(1)`
  because `00501450` does `00500540(1,0,0)` when the table count > 1.
  Index 0 is the `005066E0` dummy. WLD region 1 happens to be
  `LookoutPoint`. Comments name it; the load site does not
  `FindMap("LookoutPoint")`.
- `PresentWorld` primary is `FirstSceneMapName` (map that owns
  `GuildArrivalHSP`) else `ContainsMaps[0]` else `RegionName`.
- `SpawnHeroFromPlayerStart` prefers script `GuildArrivalHSP`, then any
  positioned `HOLY_SITE_PLAYER_START`. That marker lives on Lookout
  no-save; the string is the native start name, not a map hardcode.
- `RegionTravel.FindPlayerStart` still lists `"LookoutPointHSP"` after
  `NOVStartHSP` / `StartOakValeHSP` / `MAIN_START`. **Live spawn does
  not call that helper.** Status: leftover in a shared picker.
  **TEMPORARY** if someone reuses `FindPlayerStart` for no-save.
- `WorldGeometry.Build` `IsPrimaryStart` / kid inject is
  `StartOakValeWest` only. **DISPROVEN** as live New Game.
- `FirstSceneWorld.Region = StartOakValeWest`. **DISPROVEN** as live.

Do not “un-hardcode” index 1 into Oakvale. Oakvale is `00DBDE40` /
`Q_NewOakValeIntro`, which no-save does not run
(`EngineLifecycle.LoadQuestsAndActivate`).

### 2. Hardcoded camera

**TEMPORARY** reconstruction of `00B314E0` when manager output is still
ctor axes. **DISPROVEN** as SHOT2 / Lookout overview / invented 1.6 m
`SeedAt`.

Live game camera is `EngineLifecycle.Camera` (`ScriptedCamera`). Debug
`FlyCamera` (F2 in `Program.cs`) does not write it.

`ApplyWorldCamera` (`0049E080` / `006B42F0`):

- If blended `V0` is the ctor axis `(1,0,0)` **and** a hero exists:
  `ApplyRendererHelper(heroPos, V4 or -X, up=(0,0,1))`, FOV
  `GameCamera.FirstSeenFovDegrees` (70). `RendererHelperBound=true`.
- Else: `ApplyManagerOutput(V0, V1, V2)`.

`WorldCamera.SeedHero` only runs `006B2CA0` pose (normalised `(1,0,0)`
dirs, blend 0 → `V4=(-1,0,0)`). It does **not** write an eye.
`SeedAt` still exists and is unused on the live spawn path (**PROVEN**
removal of the 1.6 m invention).

`ScriptedCamera` ctor leftovers: FOV `RegionTravel.IntroCameraFovDegrees`
(72, Oakvale spline), `Up = FirstSeenCameraUp`. Overwritten on first
`ApplyWorldCamera`. `UseCamera(CAM_OVIF_SHOT2)` is `FirstSceneWorld` /
script helpers, **not** no-save Present (`FirstSeenCallsUseCamera=false`).

Native `00B314E0` copies helper `+0/+12/+24` (eye / forward / up). Host
substitutes **hero position** when the manager slot is still an axis.
That is a plausible first-seen stand-in (helper may be the subject) but
it is **not** the recovered helper object. A TNG camera, if bound later,
never updates `SubmittedMesh` because submit is one-shot.

`SilkEngineHost.Draw` forces `1024/768` letterbox cots and ignores the
window aspect. Native `00B30B50` uses camera `+176/+180` = 1024×768.
**VALID BACKEND ABSTRACTION** for first-seen; **TEMPORARY** if the
window is treated as the viewport.

### 3. Host landscape matrices

**VALID BACKEND ABSTRACTION.** Applying native `T(cam)` to host STB
verts is **DISPROVEN**.

| Matrix | Live host | Native first-seen | Status |
|---|---|---|---|
| Landscape W | `HostWorldSpaceLandscapeWorld()` = `I` | `00BF46A2` `T(cam)` on **camera-relative** VB | Host I ≡ `(p-cam)*T(cam)`. `LandscapeWorld(cam)` on file verts = `p+cam` **DISPROVEN**. |
| Static / PALSKIN W | baked into verts; push W = I | instance 3×4 → `009881F0` wrapper+496 | Numbers can match; site is the soup (**TEMPORARY**). |
| View | `CotScaledView` (look on Z, not `CreateLookAt`) | `00B30B50` cot-scaled camera+128 | **PROVEN** builder. Input pose is §2. |
| Proj | `009883F0` XY identity, near 0.1 / far 4000 / minZ 0.1 / maxZ 0.99 | same | **PROVEN**. |
| Vulkan | `ToVulkanWvp` = `* diag(1,-1,1,1)` only in `Draw` | DX9 `M22=+1` | **VALID BACKEND ABSTRACTION**. |
| Sky WVP | computed every `Draw` | sky `00B66A01` → `00B2FC50` | Matrix **VALID**; no `0x2000` draws (**TEMPORARY**). |

`WorldSpaces.HostLandscapeClip` vs `NativeLandscapeClip` is the locked
algebra. Keep identity W on world-space STB. Do not restore
`LandscapeWorld(Position)` on the host VB.

### 4. Manual transforms

**PROVEN** TNG basis. **TEMPORARY** as CPU `Vector3.Transform` into the
soup. CreateWorld / Y-up / negate-forward is **DISPROVEN** (that laid
props on their side; comments in `ObjectTransform`).

`WorldGeometry.ObjectTransform`:

- Translation = TNG `PositionX/Y/Z` (region-local metres).
- Scale = `MeshToWorld * ObjectScale` (`ObjectScale` in `(0.01, 20)`,
  else 1).
- Axes = `RHSetForward` (default `+Y`) × `RHSetUp` (default `+Z`),
  right = forward × up, then re-orthogonalise up.

That 3×4 is the recovered instance matrix (`D-c3d-transforms.md`).
Native leaves C3D verts in centimetres and writes the 3×4 to
wrapper+496. Host `MeshBatches.BuildMeshes` and `AddInstances` (expand)
multiply every triangle, then the renderer uses identity W.

Neighbour maps: `WorldSpaces.NeighbourRegionOffset` = `ΔMapX/ΔMapY`
applied as `T(dx,dy,0)` on instances and as a vertex add on terrain.
**PROVEN** placement. Live submit then **drops** neighbour instances
(`inst.Map == opened.Region` only). **TEMPORARY**.

Missing-normal fallback `n = UnitZ` in `BuildMeshes` / expand is a host
guard. **TEMPORARY** (rare; degenerate faces).

### 5. Magic scale / offsets

| Constant | Value | Live use | Status |
|---|---|---|---|
| `WorldGeometry.MeshToWorld` / `WorldSpaces.C3dCentimetresToMetres` | 0.01 | C3D cm → TNG metres | **PROVEN** |
| `RegionExtentMetres` | 128 | catalog / typical map | **PROVEN** as typical size, not a clamp |
| LEV cell | 16 m (`>>4`) | UV / cell lookup | **PROVEN** |
| `LandscapeTextures.UvScale` | 0.125 | exe table `0x0139C5D8` → first-seen `c3` lighting, **not** oT1 | **PROVEN** table; **DISPROVEN** as albedo UV |
| AABB Z | 0 | `00BF6F80` patch test | **PROVEN** |
| Fog start/end | 1000 / 2000 | `LinearFogPlane` | **PROVEN** record. `FogEnd=7000` is SKY_DEF, not fog. |
| `ProjectOt1` | `c40=c41=0` | landscape `UvA/B/C` in the soup | **PROVEN** first-seen device default |
| ObjectScale gate | `>0.01 && <20` | reject junk TNG | **TEMPORARY** host clamp |
| `AddTerrain` header miss | 128×128 | AABB size before STB | **TEMPORARY** (Lookout-sized default; primary still loads STB) |

No live `2000..6000` WLD gate (removed Lookout-only leftover in
`LevTileMesh`).

### 6. World soup

**TEMPORARY** on the live path. **DISPROVEN** as native.

`SubmitCurrentWorld` still builds **one** `TexturedMesh`:

```
land  = MeshBatches.Build(TessellateVisible → List<MeshTriangle>)
props = MeshBatches.BuildMeshes(primary C3Ds)
SubmittedMesh = MeshBatches.Concat(land, props)
EngineFrame.Vertices / Draws / Textures = that blob
```

`MeshBatches.BuildMeshes` comments “No `WorldGeometry` triangle soup”.
The soup moved one function down (`H-regression-audit.md`).
`SubmittedWorld.Triangles` stays empty (`expandGeometry: false`) —
that part is **PROVEN** open/draw split.

`Concat` does **not** re-sort `ScenePasses.Rank`. Accidental order
today: land `0x4`/`0x40` then props `0x20`. No `0x2000`. PALSKIN
shares `0x20` with static (**DISPROVEN** vs native `0x80`/`0x100`
drain).

`WorldGeometry.Build` default `expandGeometry: true` (neighbours +
`SkyGeometry` + instance flatten) is **DISPROVEN** as live New Game.
`SilkEngineHost` still has an `frame.World.Expanded` `SetMesh` branch.
Unreachable while Vertices are non-empty. **DISPROVEN** as live;
delete when the soup dies.

### 7. Triangle flattening

**TEMPORARY** host. **DISPROVEN** as native.

Three flatten sites still on or beside the live path:

1. **Landscape** — `TessellateVisible` → `AddTerrain` →
   `ToTileTriangles` (stored STB strips). After the **PROVEN**
   four-plane AABB (`00BDC2D0`), host dumps **every** stored tile of
   the surviving map into one CPU list. Native walks 72-byte cells
   and `00BF4570` DIPs a stride-24 VB. `ToLocalTriangles` + paint
   `LANDSCAPE_GRASS_PLAIN` (414) is the STB-miss branch inside
   `AddTerrain`. **TEMPORARY**.
2. **Static C3D** — `BuildMeshes` transforms file triangles by
   `ObjectTransform`. Native keeps local VB + instance W.
3. **PALSKIN** — `TrianglesForPose()` (no clip → bind locals ≈
   identity) then the same world multiply. Native
   `VSHADER_PALSKIN_*` + `c38`. `SubmittedHeroPalskin` only means
   `BoneCount>0` (`E-player-palskin.md`). **DISPROVEN** as character
   draw.

`ToFineTriangles` (filled 1 m grid) is **DISPROVEN** as live and
**DISPROVEN** as native (`00BF4570` is stored tessellation only).

`TessellatePrimary` is unused. **DISPROVEN** as current path. Do not
restore it.

### 8. Guessed textures

**TEMPORARY** landscape resolver. C3D `DiffuseMapID` is **PROVEN**.

`LandscapeTextures.Resolve`:

1. Exact / stripped `LANDSCAPE_*` candidates from `GROUND_` / `_ET`.
2. Else token-score every `LANDSCAPE_*` name (skip `PROC_` / `DIST_`).
3. Else `DefaultId = 414` (`LANDSCAPE_GRASS_PLAIN`).

Special-case yields (`COBBLES_IRREGULAR_01`, `FORESTFLOOR`,
`GRASS_PLAIN`, `PROC_POPPY`, …) are host guesses. The u32 at the end
of a LEV material slot is **not** a `textures.big` id (**PROVEN**).

`TryResolve` returns null for `WATER_` / `SEA_` / `LAKE` while
`FirstSeenWaterDrawShouldSubmit` is false. **PROVEN** first-seen
(water draw empty). Not a missing-water shortcut.

C3D materials use bank ids from the mesh. First-seen static binds
**one** stage (`PSHADER_TEXTURE_DIFFUSE`); bump ids (rugs 1740, books
2315) stay unbound. **PROVEN**. Host still binds two descriptor sets
and swaps them for FG. **VALID BACKEND ABSTRACTION** for FG
t0=mask / t1=albedo; extra bind on static is harmless if set1 is unused.

### 9. Fallback textures

**TEMPORARY.**

| Site | Pixel | When |
|---|---|---|
| `GpuTexture.Fallback()` | `(115,128,97,255)` olive 1×1 | missing / short RGBA; `VulkanLineRenderer` default set |
| `GpuTexture.White()` | `(255,255,255,255)` id `-1` | always uploaded; sky unlit id; host `LoadGpuTextures` append |
| `TextureLibrary.Sample` miss | `(0.45, 0.50, 0.38)` | same olive, CPU sample only |

`BindSubmittedTextures` does **not** append White. Engine ids that
fail `TryLoad` never enter `EngineFrame.Textures`; the renderer then
samples `_fallbackTexture` (olive). That is a visible “forced green”
on unresolved landscape tokens and missing C3D ids.

DXT → host RGBA8 UNORM mip0 (`R8G8B8A8Unorm`, no sRGB view) is
**VALID BACKEND ABSTRACTION** with **TEMPORARY** colour space
(native sRGB UNREAD).

Sampler `LINEAR / REPEAT / MaxLod=1` is explicitly
**TEMPORARY — NOT PARITY PROVEN** (`Dx9VulkanSamplerState`). D3D
defaults if unread writes are POINT / NONE / WRAP.

### 10. Default lighting

**PROVEN** first-seen dirlight. **TEMPORARY** as a frozen “the world
is this light forever”.

Live FS (`LineShaders.MeshFragment`) replicates:

- `c19 = (0,1,0,0)`, `c20 = (0.25)×3` (lighting ctor record 0 /
  `00F39D40`).
- `c35 = (0,0,0,1)` (`0098B2C0` stack default; apply does **not**
  write c35).
- leftover `c3 = (0,0.125,0)` (per-cell table `0x0139C614`; fog
  flush restores `c2` only).
- Packed light count first-seen **0**. Family slot remap ctor-zero
  → shader slot 0 (`VSHADER_*_DIRLIGHT_FOG`).
- `MARKER_LIGHT` does not call add-light `00B480E0`.

`WorldShading.EvaluateDirLightRgb` = `max(n·-c19,0)² * c20 + c35 + c3`.
Unlit faces = `(0, 0.125, 0)` then PS `mul_x2` → **dark green**. That
is native first-seen, not a missing ambient.

Host lights in the **fragment** shader from interpolated normals.
Native lights in the **VS** (`oD0`). **VALID BACKEND ABSTRACTION**
for first-seen 1-light; **TEMPORARY** once point lights exist.

Script `LightColors` in `ExecutionContext` is **not** wired to
`MeshPushConstants`. Live 3D ignores it.

### 11. Forced green

Three different greens. Do not merge them.

| Source | Colour | Status | Action |
|---|---|---|---|
| Leftover `c3` + `mul_x2` | dark green unlit | **PROVEN** first-seen | Keep until a later writer of `c3` is recovered. |
| Guessed / default 414 grass + `oT1=(0,0)` | grass texel (0,0) as a flat fill | resolver **TEMPORARY**; UV **PROVEN** first-seen | Fix ids; do **not** invent `c40`/`c41` until a writer is found. |
| Olive `Fallback` `(115,128,97)` | missing bind | **TEMPORARY** | Miss → skip draw or native missing-tex, not olive. |

There is no `rgb = (0,1,0)` override in the live mesh shader.

### 12. Fake Hero

**DISPROVEN** as a second invented kid on the live path.
**PROVEN** as `006AC910` `CREATURE_HERO` / mesh 4299.
**TEMPORARY** extra submit if `PresentWorld` missed the instance.

Live spawn:

- No TNG `PlayerCreature` on no-save Lookout.
- `SpawnHeroFromPlayerStart` clones `GuildArrivalHSP` as
  `PLAYER_HERO` if that def has a mesh, else `CREATURE_HERO`
  (`00449E0D`). TLC takes the creature fallback. Mesh **4299**.
- Hero is inserted into `_regionThings` / `_thingsByMap` so
  `PresentWorld` should already list it.

`SubmitCurrentWorld` still does `seen.Add(HeroMeshId)` and
`ObjectTransform(Hero)` if the instance was missing. Defensive,
native-shaped (Create is a Thing, not a TNG Graphic).

`WorldGeometry.Build` kid inject (`CREATURE_HERO_CHILD` at
`NOVStartHSP` when `IsPrimaryStart`) is Oakvale-expand only.
**DISPROVEN** as live.

Do not spawn a second hero. Do not flatten 4299 as the character
renderer (`E-player-palskin.md`).

### 13. Static pose

**PROVEN** first-seen (no play-anim). **TEMPORARY** / **DISPROVEN**
as the draw site.

`WorldShading.FirstSeenPlaysAnim = false`. Create `006AC910` has no
`PlayAnimation` / `STAND` / `CTCIdle`. `005B37F7` `DEFAULT` is clothing
GUI / `PC_UI_FRAME` only. Dest ≈ identity (`FirstSeenPalettes`).

So a bind-pose hero on **frame 0** matches native. Host then **bakes**
that dest into the soup and never uploads `c38`, never samples XSEQ
(`PaletteForPose` ignores `time`). Later clips cannot move the
Presented verts. That is a static pose **because of the soup**, not
because first-seen is T-pose-as-policy.

`WorldGeometry.ActorPoses` is filled only on expand / `FirstSceneWorld`.
Live `PresentWorld` does not pass `actorPoses`.

### 14. Load-all

Split the phrase.

| Behaviour | Live? | Status |
|---|---|---|
| `004FE2A0` LoadAllLoadableGlobalThingsFromSingleFile (`.gtg`) | No. `SingleGlobalThingsFile` default false | **DISPROVEN** as live |
| `006C2170` load every `ContainsMap` TNG | Yes | **PROVEN** |
| `OpenStaticMaps` `StaticMapsAround` (Contains + Sees + BWD-touch, skip sea) | Yes | **PROVEN** / **VALID** cluster |
| Tessellate **all cells** of every AABB-accepted opened map | Yes | **TEMPORARY** (native per-cell DIP after the same AABB) |
| Draw **every** primary C3D (no object frustum) | Yes | **TEMPORARY** |
| Draw neighbour C3Ds | No | **TEMPORARY** hole (native draws visible objects on opened patches) |
| Decode every GBANK texture at Init | No. `LoadMany` of submitted ids | **PROVEN** (`009BE8B0`) |
| Parse every C3D in the bank | No. `MeshBank.Get` on instance ids | **PROVEN** |
| One-shot never reload on travel / turn | Yes | **TEMPORARY** |

`WorldGeometry.Build(adjacentStaticMaps: true)` load-all-neighbours +
expand is **DISPROVEN** as live (`PresentWorld` passes `false` and
`onlyMaps: OpenedStaticMaps`).

---

## Other live leftovers (not in the brief, same class)

| Leftover | Status | Why it matters |
|---|---|---|
| Production submit drops `SkyGeometry` | **TEMPORARY** / **DISPROVEN** vs native `0x2000` | Lookout Present has no dome. Sky WVP is computed and unused. |
| One GLSL mesh pair + `pass.x` | **VALID** first-seen contracts; **DISPROVEN** as PALSKIN / later lights | BG/FG/static RGB match; no `c38`. |
| `MeshVertex` stride 60 | **TEMPORARY** | Native land 24 / static 32 / kid PALSKIN 28. |
| `SetTextures` `DeviceWaitIdle` + destroy/recreate | **TEMPORARY** perf | Engine now ref-equals skip; first Present still stalls. |
| Alpha test absent (`no discard`) | **TEMPORARY** (native write UNREAD) | Hair / cutouts. |
| `WorldSubmitted` never cleared | **TEMPORARY** | Frozen frustum + frozen pose. |
| `FirstSceneWorld` / expand soup still compiled | **DISPROVEN** as live | Tests lock the old path. Do not re-hook. |

---

## Classification table (quick)

| Shortcut | Class | Live? |
|---|---|---|
| Lookout via WLD index 1 / `GuildArrivalHSP` | **PROVEN** | Yes |
| `"LookoutPoint"` / `"LookoutPointHSP"` string seed | **DISPROVEN** as live seed; HSP name is leftover in unused picker | Picker unused |
| Camera = hero eye + ctor `V4` + up Z | **TEMPORARY** | Yes |
| `SeedAt(1.6 m)` / SHOT2 as no-save camera | **DISPROVEN** | No |
| Host landscape `W = I` | **VALID BACKEND ABSTRACTION** | Yes |
| Native `T(cam)` on world-space STB | **DISPROVEN** | Must not return |
| `ObjectTransform` RH basis + `×0.01` | **PROVEN** numbers; **TEMPORARY** bake site | Yes |
| CreateWorld Y-up / negate-forward | **DISPROVEN** | No |
| One `TexturedMesh` Concat soup | **TEMPORARY** / **DISPROVEN** as native | Yes |
| `expandGeometry: true` world soup | **DISPROVEN** as live | No |
| CPU flatten cells + C3D + PALSKIN dest | **TEMPORARY** / **DISPROVEN** as native | Yes |
| `ToFineTriangles` fill-grid | **DISPROVEN** | No |
| `LandscapeTextures` token score + default 414 | **TEMPORARY** | Yes |
| Water/sea skipped first-seen | **PROVEN** | Yes |
| Olive `GpuTexture.Fallback` | **TEMPORARY** | Yes |
| Ctor dirlight + leftover `c3` | **PROVEN** first-seen; **TEMPORARY** if frozen after lights exist | Yes |
| Invented ambient to kill green | **DISPROVEN** (would diverge) | — |
| Extra kid / fake Oakvale hero | **DISPROVEN** as live | No |
| Hero 4299 Thing + defensive resubmit | **PROVEN** + **TEMPORARY** fallback | Yes |
| Bind-pose dest | **PROVEN** first-seen | Yes |
| Bind-pose **baked into soup** | **TEMPORARY** / **DISPROVEN** as PALSKIN draw | Yes |
| `.gtg` LoadAll | **DISPROVEN** as live | No |
| Open Contains+Sees+BWD maps | **PROVEN** | Yes |
| Dump-all cells / all primary C3Ds | **TEMPORARY** | Yes |

---

## Removal / replacement plan (visual impact first)

Do not revert the `IEngineHost` / Pump-owns-AVI-and-New-Game split.
Do not restore `SeedAt`, CreateWorld, `T(cam)` on STB, fill-grid
terrain, or invented ambient.

### 1. Kill the Concat soup — typed draws

**Impact: whole frame (placement, layering, lighting, character, sky).**

Replace `SubmittedMesh = Concat(land, BuildMeshes(props))` with
records the native drain already has:

- Landscape: keep opened-patch AABB, then **per-cell** (or per-strip)
  draws from a stride-24-equivalent VB. Stop `List<MeshTriangle>` as
  the Present payload.
- Static: local VB + instance 3×4 as W (`009881F0`). Layer `0x20`.
- PALSKIN: dest / `c38` / `VSHADER_PALSKIN_*`. Layers `0x80`/`0x100`.
  Keep `TrianglesForPose` / `PaletteForPose` as **CPU helpers**, not
  the draw.
- Sky: emit `0x2000` (`SkyGeometry` or native dome submit). The WVP
  is already computed.

`EngineFrame.Vertices` becomes a temporary Vulkan upload, not the
engine’s world model. `Fable.Game` should stop owning
`Fable.Render.MeshVertex[]`.

Unlock tests that currently require
`SubmittedMesh.Vertices.Length > 128` as “success”.

### 2. Camera helper, not hero-as-eye

**Impact: entire viewpoint.**

Recover the `00B314E0` helper object (`+0/+12/+24`) and bind that.
Keep ctor-axis detection so slot `(1,0,0)` is never treated as an
eye. Drop `ScriptedCamera` 72° Oakvale default; first-seen game FOV
is `GameCamera.FirstSeenFovDegrees` (70) / helper turns 0.2.

Until the helper is recovered, the current hero-eye stand-in is the
least-wrong temporary — do not replace it with SHOT2 or a Lookout
overview.

### 3. Landscape material → id

**Impact: ground colour / detail (after UVs; first-seen oT1 is still 0).**

Replace token scoring + default 414 with the exe material map
(textures.h exact names already tried first). Log unresolved names
instead of silently painting grass. Olive fallback must not be the
miss path.

Do **not** invent `c40`/`c41` writers to “fix” flat landscape. First-seen
`oT1=(0,0)` is proven. Recover the later writer; then oT1 becomes
projected world UV and guessed ids will suddenly show.

### 4. PALSKIN as PALSKIN (after 1)

**Impact: hero / creatures (bind-pose is correct on frame 0; motion and
separate layer after that).**

Appearance + attachments + palette upload. No second Graphic. No
CPU flatten of 4299 into `0x20`. Sample XSEQ when
`0070D580` actually plays.

### 5. Neighbour objects + re-submit

**Impact: props off the primary AABB; camera turns.**

`TessellateVisible` already accepts neighbour **terrain**. Submit
neighbour **instances** that survive the same visibility the native
object drain uses (unread packer — until then: opened-patch +
frustum). Clear `WorldSubmitted` on `CloseStaticMapFile`, travel, and
when side planes change enough to change the accepted set.

### 6. Lighting after first-seen

**Impact: unlit/dark-green faces once `c3` is no longer the leftover.**

Keep ctor dirlight + leftover `c3` for frame 0. Wire packed lights /
`MARKER_LIGHT` / `00B480E0` when those sites are recovered. Move the
dirlight into the VS (`oD0`) when PALSKIN/static split exists. Do not
add a host ambient to “fix green”.

### 7. Backend leftovers (lower visual, do after 1)

- Sampler: dump first-seen `SetSamplerState`; until then prefer D3D
  defaults (POINT / WRAP) over LINEAR if the look is too blurry.
- sRGB / DXT view: recover native format; stop assuming UNORM.
- Alpha test `discard` when the RS write is found (hair / foliage).
- Stop `DeviceWaitIdle` on texture set; pool descriptors.
- Force 4:3 cots only while camera `+176/+180` is 1024×768.

### 8. Delete dead reconstruction (no visual if unused)

Safe once tests stop calling them as the live stand-in:

- `ExpandPresentedWorld` / production `Expand` hookup (keep as a
  debug dump if needed).
- `SilkEngineHost` expanded-`Triangles` branch.
- `TessellatePrimary` as a path (helper only).
- `WorldGeometry.Build` default `expandGeometry: true` — flip the
  default to `false` so tests must opt in to the soup.
- `FindPlayerStart("LookoutPointHSP")` if nothing calls it.
- `FirstSceneWorld` stays an Oakvale **audit** fixture, not a
  renderer.

### Explicitly do not remove

- Index-1 no-save region load.
- `GuildArrivalHSP` → `CREATURE_HERO` / 4299.
- `MeshToWorld = 0.01`.
- Host landscape identity W.
- First-seen water skip.
- First-seen `c3` leftover (the “forced green” that is real).
- First-seen `oT1=0` until a writer exists.
- `IEngineHost` / Pump ownership of AVI and New Game.

---

## Suggested order of work

1. Split Present payload (land / static / PALSKIN / sky) — biggest
   visual and the prerequisite for everything else.
2. Camera helper bind.
3. Landscape id resolver (visible the moment oT1 is non-zero; still
   removes olive / wrong 414 on mask stage 0).
4. PALSKIN records + later clips.
5. Neighbour C3Ds + clear `WorldSubmitted`.
6. Recovered lights / sampler / alpha test.
7. Delete expand soup and host fallbacks.

---

## Files read (production, no edits)

- `src/Fable.Game/EngineLifecycle.cs` (`SubmitCurrentWorld`,
  `PresentWorld`, `ApplyWorldCamera`, spawn / region load)
- `src/Fable.Render/MeshBatches.cs`
- `src/Fable.Client/SilkEngineHost.cs`, `Program.cs`
- `src/Fable.Game/WorldGeometry.cs`, `ScriptedCamera.cs`,
  `WorldCamera.cs`, `FirstSceneWorld.cs`, `SkyGeometry.cs`,
  `RegionTravel.cs`, `TextureLibrary.cs`, `IEngineHost.cs`
- `src/Fable.Formats/Levels/LandscapeTextures.cs`,
  `LandscapeFrustum.cs`, `LevHeightField.cs`, `LevTileMesh.cs`
- `src/Fable.Formats/WorldShading.cs`, `World/WorldSpaces.cs`,
  `Scene/ScenePass.cs`, `Meshes/MeshFile.cs`
- `src/Fable.Render/VulkanLineRenderer.cs`,
  `VulkanLineRenderer.Textures.cs`, `LineShaders.cs`,
  `MeshVertex.cs`, `WorldShadingPush.cs`,
  `Parity/Dx9Vulkan/Dx9VulkanShaderConstants.cs`,
  `Dx9VulkanSamplerState.cs`
