# First static C3D submit after Leave

Investigation only. Production `src/` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Question: when does native first **submit** (insert + drain + DIP)
static C3D after Leave Frontend? Is that
`WorldGeometry` / `MeshBatches` / `SubmitCurrentWorld`?
Does frontend ever issue it?

Suspect VAs come from `docs/status/investigations/` C3D packets
(`2026-08-18-static-c3d.md`, `D-c3d-transforms.md`,
`A-dx9-submit.md`, `2026-08-18-scene-layers.md`,
`2026-08-18-first-scene-things.md`). Terrain sibling:
`proofs/terrain-first-draw/README.md`.

---

## Verdict

**First static C3D DIP is after Leave + region load (`00501450` /
`006C2170`). Not frontend. Not Oakvale house 6909/6911.**

| Phase | Native VA | Frontend `0042DF9E` |
|---|---|---|
| Handle | `009AD410` at thing apply | no Graphic |
| Type-`0x18` insert | `00B324A0` ← engine `+92` `00B23BC0` | type `0x22` only |
| Enqueue slot 0 | bits `0x8`/`0x10` `00B93F40` → `00BBBF70` → `00B84720(0)` | empty |
| First static DIP | bit `0x20` `00B33010` → `00B32610` → `00B849F0(0)` → `00BBC460` → `00BBC130` → `00B81640` → `00BB30A0` → `00BB2540` DIP `vtbl+328` | no type `0x18` |

Frontend **does** call engine `vtbl+32` (`0042E0BB` → `00B27D90`).
That walk is empty of type `0x18`. **DISPROVEN** as first static
C3D submit.

Host `PresentWorld` (`expandGeometry: false`) matches **open**
(`009AD410` handles + `ObjectTransform`). Host
`SubmitCurrentWorld` matches **timing** (after `HeroSpawned` /
`006C2170`) and **set** (193 primary Lookout instances) but
**not** native submission boundaries (Concat land+C3D+sky; one
`TexturedMesh`). `WorldGeometry.AddInstances` bake is **not** on
the live path.

---

## Recovered order (no-save New Game)

```
0042EC7C retail pump
  0042DF9E  frontend frame
    0041AFA0 → 0041BEB0 type 0x22 → 00B23BC0 → 00B324A0   UI
    0042E0BB  [retail+88].vtbl+32 = 00B27D90
      00B25950 → 00B2AB80 → 00B33010 bit 0x20
      no type 0x18 → slot 0 empty → no 00BB2540
0042F2A2 Leave
004184BD → vtbl+32 00416953 Load world
  004A1840 → 00B23DC0 → 00B428E0   STB miss FinalAlbion.stb
004189C2 game pump
  WorldFrame<=1 / 004AEA70=0: skip 00435530
  first 00435530: 009DA9F0 empty; no region; no C3D
later 00501450(1) LookoutPoint
  006C2170 Loading objects (ContainsMap TNG)
    Graphic apply 0077BA40 / CMultiStatic 007E15C0
      004C0050 thing-node + 009AD410 HANDLE
    UNREAD packer [rec+0]=0x18 → 00B23BC0 → 00B324A0
      factory 012A5BB8 (00BBCF30 advertised 0x18)
      vtbl+20 00BBC460 then 00BACDD0 → 00B94D30
  0051FD80 / 006AC910 hero 4299 PALSKIN (not static)
then next 00B27D90 with records
  bits 0x8 / 0x10: 00B93F40 → 00BBBF70 → 00B84720 slot 0
  bit 0x20: 00B33010 → 00B32610 (+44 = ret) → 00B849F0(0)
            → 00BBC460 → 00BBC130 → 00B81640 → 00BB30A0
            → 00BB2540  first static-lit DIP
```

Game caller of `012A0F3C+32` after Leave is still **UNREAD**.
`00435530` has no `E8` to `00B27D90`. Pairing `00B25950` inside
`00435530` is **DISPROVEN**.

---

## Suspect VAs (C3D investigations)

| VA | Packet claim | This pass |
|---|---|---|
| `0041BEB0` type `0x22` | C3D packer? | **DISPROVEN** (UI packer; A) |
| `00B23BC0` engine `+92` | insert wrapper | **PROVEN** path; first C3D caller **UNREAD** |
| `00B324A0` | type dispatch | **PROVEN** (`type=[rec]`; factory `[0x1436E84+16+type*4]`) |
| `00BBCF30` / `00BBCF48` | writes `0x18` | **PROVEN** factory advertisement, not the record packer |
| `00BBBF40` ctor | static family | **PROVEN** vtbl `012A5BB8` |
| `00BBBF70` `vtbl+28` | queue | **PROVEN** enqueue. `mov ecx, 0x18` at `00BBBF8C` is **alloc size** for helper `012A5BB0`, **not** type `0x18` |
| `00B84720` | prim-queue push | **PROVEN** slot 0/1 (or 5/6 if `[mesh+160]`) |
| `00B849F0` | prim-queue drain | **PROVEN** bit-`0x20` tail slot 0 |
| `00B33010` | MainScene+616 draw | **PROVEN** |
| `00B32610` | bit `0x20` body | **PROVEN**. Static `+44` = `00B38840` **`ret`**. **DISPROVEN** A’s “`0x20` = `00B93F40`” |
| `00B93F40` | 0x20 drain? | **DISPROVEN** as 0x20 body. **PROVEN** earlier bits `0x8`/`0x10` that **fill** slot 0 |
| `00BBC460` `vtbl+20` | subset walk | **PROVEN** (insert pack **and** drain draw) |
| `00BBC130` | `[subset+8]-7` switch | **PROVEN**. Default: factory `vtbl+20`. Cases `00BC04F0` / `00BBE090` |
| `00B81640` | 3×4 + AABB | **PROVEN** (0 `E8`; vtbl). `esi` is the 3×4; `00B817A2` → `00BB30A0` |
| `00BB30A0` | static-lit caller | **PROVEN** (`00BB3DB6` `call 00BB2540`; texture-count **1**) |
| `00BB2540` | first static DIP | **PROVEN**. Local `shl idx,4` copy; FVF `0x112` stride 32; `009881F0`; stream `vtbl+400`; **DIP `vtbl+328`**. **DISPROVEN** “`vtbl+400` is DrawPrimitive” |
| `009881F0` | instance W | **PROVEN** wrapper+496 from 3×4 columns |
| `00988A50` | W×V×P → c5–c8 | **PROVEN** |
| `00988290` | identity W fallback | **PARTIAL** (bit 0 of draw flags **UNREAD**; identity would pile cm verts at origin) |
| `009AD410` | name→HANDLE | **PROVEN** open; **DISPROVEN** as DIP |
| `00A89450` / `00A8FD40` | C3D serialize | **PROVEN** file-local blob; not the first-frame submit |
| `0077BA40` | Graphic apply | **PROVEN** Lookout single-mesh attach via `004C0050`. **DISPROVEN** as `00B324A0` |
| `007E15C0` | CMultiStatic apply | **PROVEN** 56-byte banks. **DISPROVEN** as first Lookout extras (0). House 6911/6909 is **Oakvale**, not this Present |
| `004C0050` | dest splice | **PROVEN** 24-byte thing node (`vtbl 0x122F598`). **DISPROVEN** as MainScene insert |
| `00AF7DD0` | “Mesh Renderer” | **DISPROVEN** as draw (name intern only) |
| `00B27D90` | engine `+32` | **PROVEN** drain. Game site after Leave **UNREAD** |
| `009DA9F0` | 3D walker? | **DISPROVEN** (2D `+16020`) |

---

## Frontend is empty for static C3D

`0042DF9E` listing (`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`):

```
0042E0B2  mov ecx, [esi+88]
0042E0BB  call [eax+32]          ; 00B27D90
```

| Claim | Status |
|---|---|
| Frontend never `E8`s `00BB2540` / `00BBC460` | **PROVEN** |
| Frontend never reaches `00B27D90` / `00B33010` | **DISPROVEN** (`0042E0BB`) |
| Frontend issues first static C3D DIP | **DISPROVEN** (no type `0x18`, slot 0 empty) |
| Type `0x22` is the C3D packer | **DISPROVEN** |

Leave `0042F2A2` stops `0042DF9E`. Teardown Present `0042EBB6`
is not a 3D submit.

---

## First nonempty set (Lookout, not Oakvale)

After `00501450(1)` + `006C2170` ContainsMap TNG + `006AC910`:

| Bucket | n | First `0x20`? |
|---|---|---|
| Lookout Graphic props (walls/fences/rocks/…) | 185 | **yes** static |
| `OBJECT_STREETLAMP_LIT_SINGLE_01` **4978** | 7 | **yes** static |
| Hero `CREATURE_HERO` C3D **4299** | 1 | **no** — PALSKIN bits `0x100`/`0x80` |
| Bridge + Guild Graphics | 69+18 | **handles only** (`PresentWorld`); **not** first `0x20` |
| House **6909 / 6911** | 0 | **DISPROVEN** as this Present |
| Kid **4300** / SHOT2 | 0 | **DISPROVEN** |
| Markers / cameras / holy / travel | exist | **DISPROVEN** as C3D |

Primary Graphic submit **193 / 193** (45 unique mesh ids).
Neighbour C3Ds stay `WorldMeshInstance` handles. **PROVEN**
(`2026-08-18-first-scene-things.md`).

Which Lookout instance is the **first** `00BB2540` DIP in slot-0
walk order is **UNREAD**. The **site** is **PROVEN**.

---

## Native submit vs draw (do not collapse)

```
006C2170 NewThing
  0077BA40 / 007E15C0     attach HANDLE + 56-byte banks     OPEN
  UNREAD [rec+0]=0x18
    00B23BC0 → 00B324A0 → 00BBC460 → 00B94D30              INSERT
00B27D90
  0x8 / 0x10  00B93F40 → 00BBBF70 → 00B84720(0)            ENQUEUE
  0x20        00B33010 → 00B849F0(0) → 00BB2540 DIP        DRAW
```

`00BBC460` runs at **insert** (`00B324A0` handler `vtbl+20`) and
again at **drain** (slot-0 `vtbl+20`). First **DIP** is the drain
call, not the apply.

`00BB2540` copies file-local positions (`shl ebx, 4`) with **no**
world `fmul`, then `009881F0([0x1436E14], 3×4)`. CPU flatten of
every triangle is **DISPROVEN**.

---

## `WorldGeometry` / mesh submit compare

Live host path (`EngineLifecycle`, read only):

```
PumpGameUpdate          // 004162B5, after 006C2170
  if HeroSpawned && !WorldSubmitted
    SubmitCurrentWorld
      PresentWorld      // WorldGeometry.Build expand=false
      CollectVisibleCells / BuildCells     // land 0x4/0x40
      foreach opened.Instances, Map==primary
        Meshes.Get + props.Add((mesh, inst.Transform))
      optional 006AC910 hero (does not fire: 4299 already in list)
      MeshBatches.BuildMeshes(props)       // file-local VB + World
      Concat(objects, sky) then Concat(land, that)
```

| Host | Native | Class |
|---|---|---|
| `PresentWorld` `expandGeometry: false` | `009AD410` handles + LEV/STB headers | **PROVEN** open |
| `Instances` 280 = 193+69+18 | exist set; draw is primary-only | **PROVEN** / neighbour handles extra |
| `AddInstances` `Vector3.Transform` | only if `expand=true` | **DISPROVEN** as live first submit |
| `WorldGeometry.Build` default `expand=true` / `FirstSceneWorld` soup | Oakvale leftover | **DISPROVEN** as this Present |
| `ObjectTransform` = `0.01*ObjectScale * (right,fwd,up,T)` | instance 3×4 → `009881F0` | **EQUIVALENT** product; exe `0.01` site **UNREAD** |
| `MeshBatches.BuildMeshes` file-local verts, `MeshDraw.World` | `00BB2540` local copy + wrapper W | **PROVEN** as host now; **EQUIVALENT** to native W owner |
| `Concat(land, objects, sky)` one `TexturedMesh` | separate families / VBs / bits | **DISPROVEN** as native structure |
| `SubmittedObjects` then land buffer | land `00BF4570` vs static `00BB2540` | **DIVERGE** container; draws keep `World` |
| `VulkanLineRenderer` `draw.World * VP` for non-land | `00988A50` W×V×P → c5–c8 | **EQUIVALENT** |
| PALSKIN 4299 in same `BuildMeshes` `0x20` soup | slots 8/10/14 on `0x100`/`0x80` | **DISPROVEN** as native layer |
| One-shot `WorldSubmitted` | every `00B27D90` | **TEMPORARY BRIDGE** |
| Submit after `HeroSpawned`, not in `0042DF9E` | after `006C2170`, not frontend | **PROVEN** *timing* |
| `FlushSubmittedLayers` on `009DA9F0` | `00B25950` | **DISPROVEN** pairing |

`InstanceDraw.StaticLit` (`PassBit=0x20`, local verts + W) is the
closer host object to `00BB2540`. Live `SubmitCurrentWorld` still
feeds `TexturedMesh` + Concat, not that type.

---

## Classification table

| Claim | Status |
|---|---|
| First static C3D DIP is `00BB2540` on bit `0x20` after Lookout things exist | **PROVEN** (site) / first-instance clock **UNREAD** |
| Drain is `00B33010` → `00B32610` → `00B849F0(0)` | **PROVEN** |
| `0x20` is octree `00B93F40` | **DISPROVEN** (that is `0x8`/`0x10` enqueue) |
| Type `0x18` family = `012A5BB8` / `00BBCF30` | **PROVEN** |
| Thing packer `[rec+0]=0x18` | **UNREAD** (scans: UI / inflate / factory / alloc-size) |
| `0077BA40` / `007E15C0` / `004C0050` are that packer | **DISPROVEN** |
| C3D verts stay file-local cm; W is instance 3×4 | **PROVEN** |
| Host live bake into verts | **DISPROVEN** (`BuildMeshes` no longer `Vector3.Transform`) |
| Concat land+C3D+sky is native | **DISPROVEN** |
| First set is Lookout Graphic 192 + hero PALSKIN 4299 | **PROVEN** |
| First set is Oakvale house 6909/6911 / kid 4300 | **DISPROVEN** |
| Frontend `0042DF9E` issues static C3D | **DISPROVEN** |
| First game Present (`004AEA70=1`) already has C3D | **DISPROVEN** (empty dest, no region) |
| After region load for nonempty Lookout | **PROVEN** (things + host maps) / STB re-open **UNREAD** |
| Game caller of engine `vtbl+32` after Leave | **UNREAD** |
| Keep-instance-W flag bit 0 | **PARTIAL** |
| Mesh root 48-byte (`00A89564`) folded into 3×4 | **UNREAD** |

Dumps: `newgame-trace/static-lit-*`, `slot-dispatch-00b324a0`,
`cmultistaticmeshdef-apply-007e15c0`, `skip-global-other-apply-0077ba40`,
listing `00B81640` / `00BB2540` / `00BB30A0` / `00BBC460` /
`00BBBF70` / `00BBCF30` / `0042DF9E`. Host (unread-only):
`WorldGeometry.cs`, `EngineLifecycle.SubmitCurrentWorld`,
`MeshBatches.BuildMeshes`, `InstanceDraw.cs`.
