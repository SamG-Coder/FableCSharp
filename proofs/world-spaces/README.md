# First world-space matrix after Leave

Investigation only. No production `src` edits.
Do **not** start at Oakvale / `CAM_OVIF_SHOT2` / `FirstSceneWorld`.
That path is the later intro contract (`Q_NewOakValeIntro` /
`00DABAC0` → `00DBDE40`), not Leave / Init Game / first no-save 3D.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**EQUIVALENT** / **LEFTOVER**.

Sources: `src/Fable.Formats/World/WorldSpaces.cs`;
`docs/render/WORLD_SPACE_CONTRACT.md`;
`docs/render/FIRST_SCENE_WORLD_PARITY.md`;
`docs/status/investigations/B-camera-matrices.md`;
`docs/status/investigations/2026-08-18-camera.md`;
`docs/status/investigations/D-c3d-transforms.md`;
`docs/status/investigations/2026-08-18-landscape-draw.md`;
`LandscapeFrustum.cs`;
`proofs/camera-after-leave/`, `terrain-first-draw/`,
`landscape-first-draw/`, `c3d-first-submit/`, `region-travel-first/`.

---

## Verdict

`WORLD_SPACE_CONTRACT.md` and `WorldSpaces.Catalog()` are the
**Oakvale-intro / SHOT2 space table**. They are not the first
world-space matrix after Leave.

| Question | Answer | Class |
|---|---|---|
| First 3D scene after Leave? | LookoutPoint + hero 4299, not `StartOakValeWest` / house 6909 | **PROVEN** |
| Contract camera origin SHOT2? | Not first no-save Present (`FirstSeenCallsUseCamera=false`) | **LEFTOVER** |
| Conversion *functions* (`STB − Map`, `T(cam)` algebra, C3D `×0.01`)? | Same units on Lookout | **PROVEN** (algebra) |
| First uploaded wrapper W after nonempty 3D? | `00B2FC50` → `009881F0` **I**, then landscape `00BF46A2` `T(cam)`, then static instance 3×4 | **PROVEN** writers; first-clock site **PARTIAL** |
| Catalog “static W = identity”? | Bind default only. First static DIP writes instance 3×4 | **DISPROVEN** as native draw W |

---

## Timeline (no-save New Game)

```
0042EC7C retail
  0042DF9E  2D UI   // VSHADER_2D_SPRITE; no 3D W
    0042E0BB  vtbl+32 = 00B27D90   // layer walk empty
0042F2A2 Leave frontend
  0042EBB6  009BE420 + 009BEEB0    // teardown Present, not 0042DF9E
0042F491 Init Game → 00418DCA → 004184BD
  Init World 004A6E30              // WorldCamera / GameCamera ctors
  00416953 LoadWorld FinalAlbion.wld
    00B23DC0 → 00B428E0            // FinalAlbion.stb MISS
004189C2 first pumps
  WorldFrame<=1: skip 00435F70
  first 00435530: dest empty; no region; no landscape / C3D DIP
later 00501450(1) LookoutPoint     // Maps[0] / NewRegion 1
  006C2170 / 0051FD80 / 006AC910   // things + hero 4299
then next 00B27D90 with records
  00B2FC50  009881F0  W = I        // bind / sky restore
  bit 0x4 / 0x40
    00BF46A2  009881F0  W = T(cam) // cam-relative landscape VB
  bit 0x20
    instance 3×4 → 009881F0        // file-local C3D
  00988A50  W*V*P → c5–c8
```

Game caller of `012A0F3C+32` after Leave is **UNREAD**. Pairing
`00B25950` *inside* `00435530` is **DISPROVEN**
(`proofs/terrain-first-draw`). First empty game Present therefore
does **not** prove a world matrix was uploaded.

`Q_NewOakValeIntro` / `CAM_OVIF_SHOT2` / `HerosOldHouse` are **not**
on this list. **PROVEN**.

---

## 1. First uploaded world matrix (wrapper+496)

Two objects stay separate (B §1):

1. **WorldCamera** `world+24` ctor `006B4900` — follow / blend bank.
2. **Render camera** `[0x1436EA0]` ctor `00B31700` — what
   `00B30B50` / `00B2FC50` turn into wrapper W/V/P.

The pointer from (1) to `[0x1436EA0]+12` on first Present is
**UNREAD**. `00B23B50` (`UseCamera`) is **DISPROVEN** as first
no-save.

| Writer | Dest | Matrix | When after Leave | Class |
|---|---|---|---|---|
| Frontend 2D | sprite batch | not wrapper W | `0042DF9E` | **DISPROVEN** as 3D W |
| Leave teardown | clear + Present | none | `0042EBB6` | **PROVEN** absence |
| First `00435530` empty dest | — | none proven | WorldFrame>1, no region | **UNREAD** upload |
| `00B2FC50` | wrapper+496 via `009881F0` | identity 3×4 (stack) + bottom `(0,0,0,1)` | first nonempty sky/bind | **PROVEN** |
| `00988290` | same +496 if `+488==0` | identity | falls through `009881F0` | **PROVEN** fn; first-seen clock **UNREAD** |
| `00BF46A2` | stack `esp+144` then `009881F0` | I + `[0x1436EA0]+84/+88/+92` last column = helper pos | first FG cell `00BF4570` | **PROVEN** |
| Static `00BB2540` path | wrapper+496 via `009881F0` | instance 3×4 (`ObjectTransform` host equivalent) | first type-`0x18` DIP | **PROVEN** owner; 3×4 *build* site **PARTIAL** |
| `00B67480` landscape setup | `009881F0(0x1436E14, …)` | identity-like 1s at +4/+20/+36 | shared bit 4 / `0x40` setup | **PROVEN** setup; not the per-cell W |

`009881F0` itself: 3×4 column-stride-12 → wrapper+496 as rows,
bottom `(0,0,0,1)`, dirty `0xC990`. **PROVEN** (B §1.4).

No first-seen `SetTransform(D3DTS_WORLD)`. WVP is VS `c5–c8` from
`00988A50`. **PROVEN** absence.

Native landscape: `file → region-local → cam-rel VB → T(cam) → V → P`.
Host: `file → region-local → I → V → P`. Clip matches
(`WorldSpaces.NativeLandscapeClip` / `HostLandscapeClip`). Host I is
**EQUIVALENT**. `T(cam)` on host world verts is **DISPROVEN**.

---

## 2. `WorldSpaces.Catalog()` vs native after Leave

Contract rows are copied from `WorldSpaces.cs` / `WORLD_SPACE_CONTRACT.md`.
**After Leave** is the no-save first nonempty 3D (Lookout), not SHOT2.

| Space (catalog) | Contract claim | Native after Leave | Class vs Leave |
|---|---|---|---|
| graphics.big C3D local | cm; X right Y forward Z up; `×0.01` | same file; first DIP is Lookout C3D, not house 6909 | **PROVEN** units. House 6909 **LEFTOVER** as first mesh |
| C3D units / centimetres | `×0.01` → TNG metres | same; no `×0.01` on STB Z | **PROVEN** |
| TNG object local transform | `RHSetForward`/`RHSetUp`; `CreateWorld` **DISPROVEN** | same on Lookout TNG (`WorldGeometryTests` lamps/rocks) | **PROVEN**. Compose VA **UNREAD** |
| region-local coordinates | metres; origin current `(MapX,MapY)`; Oakvale house ~34,129 | Lookout TNG XY **0–128**, not 3232+; house numbers are Oakvale | **PROVEN** space. Oakvale example **LEFTOVER** |
| WLD/global map coordinates | local = WLD − (MapX, MapY); cites Oakvale 3456/736 **and** Lookout 3232/3488 | first map is Lookout **3232/3488** (BWD min equals WLD) | **PROVEN** Lookout row. Oakvale-as-first **LEFTOVER** |
| STB file coordinates | ushort XY = WLD; `StbFileToRegionLocal` | Lookout STB `[3232,3248]×[3488,3504]`; first-seen `FinalAlbion.stb` **MISS** | **PROVEN** convert. First STB open **PARTIAL** (later site **UNREAD**) |
| expanded Fable landscape VB | `p_camrel = p_local − cam` (`00BFE050`) | same expand; first DIP `00BF4570` after maps exist | **PROVEN** |
| camera-relative landscape | `T(cam)` `00BF46A2`; host `T(cam)` on file verts **DISPROVEN** | same; first nonempty FG after Lookout open | **PROVEN** |
| camera/world | origin **SHOT2 helper +0/+12/+24**; not `CreateLookAt` | helper *layout* +0/+12/+24 is `00B314E0`. First source is **not** SHOT2. Live pointer **UNREAD**. Best ctor default: GameCamera `00A0C130` pos 0 look +Z up `(1,1,1)` FOV 70° turns | Layout **PROVEN**. SHOT2 origin **LEFTOVER**. First eye **UNREAD** |
| static-object world | **identity W** `009881F0` | bind default I **then** instance 3×4 on first `00BB2540` | Identity-as-draw-W **DISPROVEN**. Instance W **PROVEN** |
| skinned-character | `0.01` + `dest[group[a0/3]]`; file byte = VS offset | first PALSKIN is hero **4299**, not kid 4300 / father | Skin rule **PROVEN**. Kid/father first-seen **LEFTOVER** |
| view space | cot-scaled; look on Z; `clip.w = view.z` | same builders `00B30B50` / `00988350`. Inputs not SHOT2 | Builder **PROVEN**. Pose **UNREAD** / host **DIVERGE** |
| clip space | `009883F0`; Y flip **not** Fable P | same P 0.1/4000/0.1/0.99 on world; sky 100/10000/0.99/1 | **PROVEN** |
| Vulkan NDC | `ToVulkanWvp` Y flip **EQUIVALENT** | translation only; not a Fable space | **EQUIVALENT** |

`WorldSpaces.RegionExtentMetres = 128` is typical map size, **not** a
clamp. **PROVEN** as typical.

---

## 3. Meeting-space rule (still the first convert)

The unique file → draw meeting space after Leave is the same subtract
the contract names, applied to **Lookout**:

```
TNG / helper / camera   already region-local metres
STB file XY             WLD ushorts
localXY                 STB.WorldXY − (MapX, MapY)     // 3232, 3488
neighbour               ΔMapX / ΔMapY
C3D                     cm × 0.01, then ObjectTransform
```

`WorldGeometryTests` (Lookout): TNG max XY < 130, never `> MapX`;
most `OBJECT_*` sit on fine height. Adding MapX to TNG would throw
props kilometres off the terrain. **PROVEN**.

Oakvale house / father / kid / SHOT2 numeric meeting
(`WorldPipelineTests.House_father_kid_…`) is a **valid intro
fixture**, **LEFTOVER** as the first after Leave.

---

## 4. `WORLD_SPACE_CONTRACT.md` leftovers vs Leave

| Contract sentence | After Leave | Class |
|---|---|---|
| First New Game is `StartOakValeWest` / `Q_NewOakValeIntro` / `CAM_OVIF_SHOT2` / `HerosOldHouse` | First region `LookoutPoint`; no `UseCamera` | **LEFTOVER** scene |
| Native camera is TNG helper ~40, 130 (SHOT2) | Not first Present. Host `FirstSceneWorld.WorldViewProj()` is a **third** matrix | **LEFTOVER** |
| `FirstSceneWorld` is a live function of the contract | Oakvale soup + SHOT2 72° 4:3. Live New Game submits Lookout | **LEFTOVER** vs Leave |
| Static world = identity `009881F0` | Host bake + I can match clip; native draw W is instance 3×4 | **EQUIVALENT** after bake / **DISPROVEN** native site |
| Locked “host landscape uses identity W” | Still correct for host STB | **EQUIVALENT** (keep) |
| Locked “`T(cam)` on host STB world verts **DISPROVEN**” | Still true | **PROVEN** (keep) |
| ReadExtras 2000–6000 gate | Lookout-only leftover; Oakvale MapY=736. Already **DISPROVEN** in contract | stays **DISPROVEN** |

B §0: do not treat the contract as gospel where it names SHOT2 as
the first Present camera. Transform parity of the *live* host camera
vs first no-save Present is **not** proven (B §3; camera-after-leave).

---

## 5. C# vs native

| Host | Native after Leave | Class |
|---|---|---|
| `WorldSpaces.StbFileToRegionLocal` / `NeighbourRegionOffset` | same subtract / ΔMap | **PROVEN** |
| `WorldSpaces.NativeLandscapeClip` / `HostLandscapeClip` | `T(cam)` on cam-rel ≡ I on world STB | **EQUIVALENT** |
| `WorldSpaces.C3dLocalToMetres` | C3D cm only | **PROVEN** |
| `WorldSpaces.Catalog()` Oakvale / SHOT2 origins | Lookout first | **LEFTOVER** labels |
| `Catalog` static “identity W for static/PALSKIN” | instance 3×4 → `009881F0` | **DISPROVEN** as native draw W |
| `FirstSceneWorld.Build` + `WorldPipelineTests` | Oakvale intro fixture | **LEFTOVER** vs Leave |
| `LandscapeFrustum.IdentityWorld()` on landscape submit | host world STB | **EQUIVALENT** |
| `LandscapeFrustum.LandscapeWorld(cam)` on host verts | `p+cam` | **DISPROVEN** |
| `WorldGeometry.ObjectTransform` as wrapper W | native instance 3×4 | **PROVEN** numbers / **PARTIAL** VA |
| `MeshBatches.BuildMeshes` bake then I | file-local VB + `009881F0` 3×4 | **DIVERGE** site (`c3d-first-submit`) |
| `EngineFrame.Camera` SHOT2 72° / WorldCamera V0-as-eye | helper **UNREAD**; ctor FOV 70° turns | **DIVERGE** / **LEFTOVER** |
| Frontend `EngineFrame` carrying a 3D camera | 2D only | **LEFTOVER** (camera-after-leave) |

---

## Classifications (short)

1. **First world-space matrix after Leave is not the Oakvale catalog.**
   Scene is Lookout. SHOT2 / house / kid / father rows are
   **LEFTOVER**.
2. **First *uploaded* W (once 3D exists):**
   `00B2FC50`/`009881F0` **I**, then `00BF46A2` **`T(cam)`** on
   cam-relative landscape, then static **instance 3×4**.
   Empty first `00435530` does not prove an upload. **PROVEN**
   writers, **PARTIAL** first-clock.
3. **Convert functions stay locked.** `STB − (MapX, MapY)`, C3D
   `×0.01`, `CreateWorld` **DISPROVEN**, host `T(cam)` on world STB
   **DISPROVEN**, host landscape I **EQUIVALENT**.
4. **Catalog “static W = identity” is bind default, not first
   static DIP.** Native world-matrix owner is wrapper+496 from the
   instance 3×4. **DISPROVEN** as the draw W.
5. **Camera origin on the contract is over-applied.** Helper
   layout +0/+12/+24 is **PROVEN**. First no-save helper pointer is
   **UNREAD**. Live host pose is **DIVERGE**.
