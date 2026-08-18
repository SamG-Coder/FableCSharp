# First native terrain / static C3D draw (after Leave)

Investigation only. Production `src/` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Question: when does native first **draw** landscape cells and static C3D?
Must that be after world/region load, not during frontend?

---

## Verdict

**First geometry DIP is after Leave + world/region load. Not frontend.**

| Draw | First native DIP | Frontend `0042DF9E` |
|---|---|---|
| Landscape FG cells | `00BF4570` ← `00BDC2D0` ← `00B6B0B0` bit `0x40` | no cell DIP |
| Landscape BG | `00BF71D0` ← `00BDC060` ← `00B6B0B0` bit `0x4` | no patch list |
| Static C3D | `00B33010` bit `0x20` → `00B32610` → slot 0 | type `0x22` UI only |

Frontend **does** call engine `vtbl+32` (`0042E0BB` → `00B27D90` → `00B25950` → landscape `vtbl+16`). That walk is empty: `00B428E0` has not opened patches; MainScene has no type `0x18`. **No terrain / static-C3D DIP.**

Host `WorldGeometry.Build` soup / `SubmitCurrentWorld` concat is **not** the native first-draw site.

---

## Recovered order (no-save New Game)

```
0042EC7C retail pump
  0042DF9E  frontend frame
    009D8CF0 / 009BEF20
    00595582 / 00595222  [ui+84] vtbl+8  0041AFA0
      0041BEB0 type 0x22 → 00B23BC0 → 00B324A0   UI only
    0042E0BB  [retail+88].vtbl+32 = 00B27D90     layer walk
      00B25950 → 00B2AB80 → 00B6B0B0 / 00B33010
      [0x1436E8C]+44 empty → no 00BDC2D0 / 00BF4570
      no type 0x18 → 0x20 slot 0 empty
    009D9C80 / 009DA9F0(1) ×2   empty 2D
    009BEF50 / 009BEEB0
retail+41
  0042F2A2 Leave
    0042EBB6  009BE420 + 009BEEB0   teardown Present, not 0042DF9E
    FinalAlbion.wld
    00418DCA Init Game
      004184BD → vtbl+32 00416953 Load world
        004A1840 → display vtbl+208 00B23DC0 → 00B428E0
          first-seen Data\Levels\FinalAlbion.stb MISS
          00B42750 does not write +424
        dummy WorldMap index 0
004189C2 game pump
  WorldFrame<=1: 00417001 skips 00435530
  WorldFrame>1, 004AEA70=0: skip 00435F70
  later 004AEA70=1: 00435F70 jmp 00435530
    BeginScene / Clear / overlay skip / interface skip
    009D9C80 / 009DA9F0(1) empty dest   FIRST GAME PRESENT
    no region, no 00501450, no landscape DIP
later 00501450(1) Lookout
  006C2170 Loading objects (ContainsMap TNG)
  004FCBB0 Activate Topology
  004FC8A0 MiniMap only (NOT 00B428E0)
  0051FD80 / 006AC910 hero
then opened patches + type 0x18 records exist
  next 00B27D90 → first 00BF4570 / bit 0x20 DIP
```

Game caller of `012A0F3C+32` after Leave is **UNREAD**. `00435530` has no `E8`/`[reg+32]` to `00B27D90`. Pairing `00B25950` inside `00435530` is **DISPROVEN**.

---

## Frontend `0042DF9E` vs landscape

Listing `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`:

```
0042DF9E  this = retail
0042E075  009D8CF0 Clear
0042E080  009BEF20 BeginScene
0042E085  00595582([esi+88]) → 00595222 UI
0042E0B2  mov ecx, [esi+88]
0042E0BB  call [eax+32]          ; 00B27D90
0042E129  009D9C80
0042E136  009DA9F0(1)
0042E13B  00404A80 / 00404C00
0042E147  009D9C80
0042E15A  009DA9F0(1)
0042E165  009BEF50
0042E170  009BEEB0
```

`012A0F3C+32` = `00B27D90` (dump `vtbl-engine-camera-bind-vtbl-012a0f4c`: slot at `012A0F3C+32`).

`00B26360` layer register ends `00B276DB mov [esi+17], 1`. `00B27D90` tests `[this+17]` then `00B27E87 call 00B25950`. After Init Engine (`0042E204`) the walk **runs**.

`00B6B0B0` (`landscape-trace` / `terrain-trace.md`):

- `arg+4==4` → walk `[0x1436E8C]+44` → `00BDC060` / `00BF71D0`
- `arg+4==0x40` → same list → `00BDC2D0` → `00BF4570`
- empty sentinel (`cmp esi, eax; je`) → **no DIP**

`00B428E0` is Leave/`00416953`, not frontend. Type `0x22` is **DISPROVEN** as C3D/landscape packer (`A-dx9-submit.md`).

**“0042DF9E does not call landscape”** is therefore:

| Claim | Status |
|---|---|
| Does not `E8 00B6B0B0` / `00BF4570` | **PROVEN** |
| Never reaches landscape `vtbl+16` | **DISPROVEN** (`0042E0BB`) |
| Issues first terrain / static C3D DIP | **DISPROVEN** (empty list, type `0x22`) |

---

## First landscape / static C3D DIP after Leave

| Step | VA | Status |
|---|---|---|
| Leave stops `0042DF9E` | `0042F2A2` / `0042EBB6` | **PROVEN** |
| Open is load, not draw | `00B428E0` → `00B42750` | **PROVEN** |
| First-seen STB is miss | `FinalAlbion.stb` absent | **PROVEN** (`DISPROVEN` as Lookout open) |
| Dummy pump is not a region | index 0, no `00501450` | **PROVEN** |
| First `00435530` dest empty | `009DA9F0` → `009DB6E6`; `SubmittedLayerBits` empty | **PROVEN** |
| Things / hero after `006C2170` | `00522720` / `006AC910` | **PROVEN** |
| `004FC8A0` opens STB | — | **DISPROVEN** (MiniMap) |
| Native site that later hits Lookout STB | later `00B23DC0` / `00B428E0` | **UNREAD** |
| Game `00B27D90` after Leave | not in `00435530` | **UNREAD** |
| First FG DIP | `00BF4570` mesh `+52/+56`, strip type 5 | **PROVEN** as the DIP; **PARTIAL** as first-frame clock |
| First static C3D | bit `0x20` type `0x18` slot 0 | **PROVEN** drain; packer **UNREAD** |

Must-after-load (no frontend soup): **PROVEN**.

---

## `WorldGeometry.cs` vs native

| Host | Native | Class |
|---|---|---|
| `PresentWorld` `expandGeometry: false` | `00B3EFA0` headers + `009AD410` handles | **PROVEN** open |
| `Build` default `expand=true` (neighbours + sky soup) | not live New Game | **DISPROVEN** as first draw |
| `TessellatePrimary` | unused; not native | **DISPROVEN** as path |
| `CollectVisibleCells` / `TessellateVisible` whole-map AABB then dump tiles | `00BDC2D0` then per-cell `00BF4570` / `00BF3860` | **DIVERGE** (C / landscape-draw) |
| `AddInstances` bake `ObjectTransform` into verts | instance 3×4 → `009881F0` c5–c8 | **DIVERGE** (D) |
| `SubmitCurrentWorld` `Concat(land, C3D, sky)` one VB | layers `0x4` → `0x40` → `0x20` → `0x2000` | **DISPROVEN** as native |
| Submit after `HeroSpawned` / `006C2170`, before `00435530` | consume already-opened maps | **PROVEN** *timing* |
| `FlushSubmittedLayers` on `009DA9F0` | `009DA9F0` is 2D `+16020`; walk is `00B25950` | **DISPROVEN** pairing |
| `00B25950` inside `00435530` | only `00B27D90`; game site unread | **DISPROVEN** pairing |
| First `00435530` empty / no region | `After_004AEA70_eq_1_*` | **PROVEN** |
| One-shot `WorldSubmitted` | redraw every `00B27D90` | **TEMPORARY BRIDGE** |

---

## Classification table

| Claim | Status |
|---|---|
| First terrain DIP is `00BF4570` on bit `0x40` after maps exist | **PROVEN** |
| First static C3D drain is MainScene bit `0x20` type `0x18` | **PROVEN** (drain) / packer **UNREAD** |
| That DIP is after Leave, not frontend widgets | **PROVEN** |
| After world load (`00416953` / `004A1840`) | **PROVEN** (open attempted) |
| After region load (`00501450` / `006C2170`) for nonempty Lookout | **PROVEN** (things + host maps) / native STB re-open **UNREAD** |
| First game Present (`004AEA70=1`) already has terrain | **DISPROVEN** (empty dest, no region) |
| `0042DF9E` issues landscape / static C3D DIP | **DISPROVEN** |
| `0042DF9E` never calls `00B27D90` / `00B6B0B0` | **DISPROVEN** |
| `009DA9F0` is the 3D layer walker | **DISPROVEN** |
| `SubmitCurrentWorld` tessellate+flatten+one mesh is native | **DISPROVEN** |
| Game display `00435530` `E8`s `00B25950` | **DISPROVEN** |
| Game caller of engine `vtbl+32` after Leave | **UNREAD** |
| Native Lookout STB hit after `FinalAlbion.stb` miss | **UNREAD** |

Dumps: `landscape-trace/`, `terrain-trace.md`, `C-terrain-static-map.md`, `A-dx9-submit.md`, `2026-08-18-landscape-draw.md`, `2026-08-18-scene-layers.md`, listing `0042DF9E` / `00435530` / `00B27D90`.
