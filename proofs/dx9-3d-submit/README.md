# First 3D DX9 submit after Leave (not 0x22 / 0x27)

Investigation only. Production `src/` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Question: what is the first native **3D** DX9 submit after Leave
Frontend? Is it type `0x22` / `0x27`? How does the game Present
path compare to frontend `0042DF9E`?

Siblings: `proofs/terrain-first-draw/`, `proofs/landscape-first-draw/`,
`proofs/c3d-first-submit/`, `implementer/frontend/06-dx9-submit.md`,
`docs/status/investigations/A-dx9-submit.md`,
`docs/status/investigations/2026-08-18-scene-layers.md`.

---

## Verdict

**First 3D DX9 primitive after Leave is not type `0x22` or `0x27`.**
Those are frontend 2D families (widget sprite / type-6 glyph). They
may travel `00B23BC0` → `00B324A0`, but they are screen-space
(`mov oPos, v0`), not world geometry.

After Leave, `0042DF9E` **stops**. Game Present is `00435F70` →
`00435530` → same device `009BEEB0`. That envelope **does not**
`E8` engine `vtbl+32`. 3D drain is still `00B27D90` → `00B25950`
→ layer `vtbl+16`. Frontend already calls that slot
(`0042E0BB`); the lists are empty. Game caller after Leave is
**UNREAD**.

First nonempty 3D DIP is after `00501450` / maps exist, on the
next `00B27D90`:

| First after maps | Site | Layer |
|---|---|---|
| First 3D **walk that can DIP** | `00B6B0B0` | every `00B27D90` |
| First 3D **patch submit** | `00BDC060` → `00BF71D0` | bit `0x4` BG |
| First **stored-mesh DrawIndexed** | `00BF4570` → `00A0AD40` **vtbl+328** type **5** | bit `0x40` FG |
| First static C3D DIP | `00BB2540` **vtbl+328** | bit `0x20` type `0x18` |

`009DA9F0` is the 2D `+16020` drain (**vtbl+332**). Pairing it as
the 3D layer walker is **DISPROVEN**.

---

## Present path: `0042DF9E` vs `00435530`

Same device helpers. Different envelope. **PROVEN** listings
`text-map/listing-00400000.txt` + `functions.tsv`.

| Step | Frontend `0042DF9E` | Game `00435530` (`00435F70` jmp) |
|---|---|---|
| this | retail | display `game+40` |
| viewport | `00A0B560` / `009BECE0` | `00A0BF20` / `00A0B560` / `009BECE0` |
| Clear | `009D8CF0` **before** Begin (`0xFF000000`) | `009D8CF0` **after** Begin (colour bytes 0) |
| BeginScene | `009BEF20` vtbl+164 | `009BEF20` |
| 2D widgets | `[ui+84]` `00595582` / `00595222` → type `0x22` / `0x27` | none. Overlay `00435000`, interface `00435070` (may later pack `0x22` via `0057B43F`; first-seen skip) |
| **3D layer walk** | **`[retail+88].vtbl+32` `0042E0BB` = `00B27D90`** | **no `E8` / no `[reg+32]` to `00B27D90`** |
| mid helper | `00404A80` / `00404C00` between flushes | HUD `009DD8F0` gates (first-seen closed) |
| 2D dest | `009D9C80` / `009DA9F0(1)` **twice** | `009D9C80` / `009DA9F0(1)` **once** |
| EndScene | `009BEF50` | `009BEF50` |
| Present | `009BEEB0` (four NULL) | `00435F4A` `test al` then `009BEEB0` at `00435F50` |

Leave teardown `0042EBB6` is **not** `0042DF9E`: `009BE420` +
`009BEEB0` only. **DISPROVEN** as a 3D submit.

`00417001` does not Present. After `WorldFrame>1` it may call
`00435F70` when `004AEA70=1`. First that Present: dest empty,
no region, no landscape / C3D DIP. **PROVEN.**

### `0042DF9E` recovered (E8 + the one vtbl)

```
0042E075  009D8CF0 Clear
0042E080  009BEF20 BeginScene
0042E085  00595582([esi+88]) → 00595222     ; 0x22 / 0x27 pack
0042E0B2  mov ecx, [esi+88]
0042E0BB  call [eax+32]                      ; 00B27D90
0042E129  009D9C80
0042E136  009DA9F0(1)
0042E13B  00404A80 / 00404C00
0042E147  009D9C80
0042E15A  009DA9F0(1)
0042E165  009BEF50
0042E170  009BEEB0
```

Direct `E8` callees (**PROVEN** complete):
`00415A60, 009E1BC0, 00A0B560, 009BECE0, 009D8CF0, 009BEF20,
00595582, 00595222, 0041E5F2, 0041D03C, 009D9C80, 009DA9F0,
00404A80, 00404C00, 009BEF50, 009BEEB0`.

None of `00BF4570` / `00BB2540` / `00B6B0B0` / `00B25950`.

### `00435530` recovered E8s that matter

```
004356A7  009BEF20 BeginScene
004356CC  009D8CF0 Clear
00435752  [edx+8] / 00435768 [edx+12]   ; gated [esi+224], not engine
004357D0  00435000 overlay
004357D8  00435070 interface
00435AA4 / 00435B57 / 00435BDD / 00435CC7  009DD8F0  ; gated HUD
00435D40  009D9C80
00435D4D  009DA9F0(1)
00435D58  009BEF50
00435F50  009BEEB0
```

**No** `00B27D90` / `00B25950` / `00B6B0B0` / `00BF4570` /
`00BB2540`. `[esi+224]` vtbl+8/+12 is display, not
`012A0F3C+32`.

The only `call [reg+32]` sites in `listing-00400000.txt`:

| Site | Object | Status |
|---|---|---|
| `0042E0BB` | `[retail+88]` engine `012A0F3C` | **PROVEN** 3D drain |
| `004188E9` | game vtbl+32 `00416953` | **PROVEN** Load world. **DISPROVEN** as scene submit |
| `0041E70F` / `0042083A` | other objects | **DISPROVEN** as `00B27D90` |

---

## `0x22` / `0x27` are not 3D

| Type | Packer | Drain / DX9 | Space |
|---|---|---|---|
| `0x22` widget sprite | `0041BEB0` dest `+0x15C` size `0xC0` → vtbl+92 `00B23BC0` | instance `00BAD8A0`; draw `00BAE2D0` → `00A0AEA0` **DIPUP vtbl+336** prim **4**, stride 32, `VSHADER_2D_SPRITE` | clip/screen. dest 0 first-seen → `00BADB36` **no DIP** |
| `0x27` type-6 glyph | `0054EF00` / `00543910` `mov [esi], 0x27` size 64 | `00AB7C20` → `00A0ABE0` **DrawPrimitive vtbl+324** prim **4**, 6×28-byte XYZRHW | screen −0.5 |
| HUD / PlayAVI 2D | `009DB700` 60-byte `+16020` | `009DA9F0` **vtbl+332** prim 2 or 4 | NDC. first-seen **empty** |

**DISPROVEN:**

- `0x22` / `0x27` as landscape / C3D / PALSKIN / sky packer
- nonempty dest `00BAD8A0` `E8 009DB700` (callers of `009DB700`
  are only `009DC00E` / `009DD93D`)
- `009DA9F0` switching on type `0x22` (no `cmp …,0x22` in
  `009D9C80–009DB000`)
- first 3D DIP being frontend glyph / title sprite

Frontend **does** run `00B27D90` on every `0042DF9E`. That walk
sees type `0x22` dests and empty landscape / type-`0x18` lists.
**DISPROVEN** as the first 3D DIP.

---

## Recovered order (no-save New Game)

```
0042EC7C retail
  0042DF9E
    0x22 / 0x27 pack (UI)
    0042E0BB 00B27D90
      00B25950 → 00B2AB80
        0x4 / 0x40  00B6B0B0   [0x1436E8C]+44 sentinel → no DIP
        0x20        00B33010   no type 0x18 → slot 0 empty
        0x2000      00B662F0   sky object exists; first-seen
                                Lookout dome is after maps (PARTIAL clock)
        0x20000     00B783F0   empty-out
    009DA9F0 ×2  empty +16020
    009BEEB0
retail+41
  0042F2A2 Leave                         ; 0042DF9E stops
    0042EBB6  009BE420 + 009BEEB0        ; not 3D
    FinalAlbion.wld
    00418DCA Init Game
      004188E9 game vtbl+32 00416953     ; Load world, not 00B27D90
        004A1840 → 00B23DC0 → 00B428E0
          FinalAlbion.stb MISS
004189C2 game pump
  WorldFrame<=1: skip 00435530
  later 004AEA70=1: first 00435530
    BeginScene / Clear / overlay skip / interface skip
    009DA9F0 → 009DB6E6                  ; FIRST GAME PRESENT, no 3D
later 00501450(1) Lookout + 006C2170 + STB attach
  next 00B27D90                          ; game site UNREAD
    0x4   00BDC060 → 00BF71D0            ; first 3D patch submit
    0x40  00BDC2D0 → 00BF4570 → 00A0AD40 ; first stored-mesh DIP
    0x20  00B849F0(0) → 00BB2540         ; first static C3D
    0x2000 00B662F0 else                 ; sky
    then 00435530 009DA9F0 (still 2D)
```

---

## First 3D DX9 call (after maps)

### `00B27D90` (`012A0F3C+32`)

**PROVEN** body. Tests `[this+17]` (set at Init Engine
`00B276DB`). Then `00B23A90`, stream clear vtbl+400 / +416 /
+260, optional `00B277A0`, **`00B25950`**. `ret 8`. Zero `E8`
callers.

### Landscape FG DIP — first stored 3D primitive

**PROVEN** (`2026-08-18-landscape-draw.md`):

```
00BF55DB  prims=[mesh+70]  verts=[mesh+68]
          call 00A0AD40
00A0AD40  type = [IB+12] = 5     ; D3DPT_TRIANGLESTRIP
          IDirect3DDevice9.vtbl+328 DrawIndexedPrimitive
            (5, 0, 0, NumVerts, 0, PrimitiveCount)
          stream vtbl+400  stride 24
          indices vtbl+416
```

Not vtbl+332. Not type `0x22`. Cell must have `+60` bit `0x4`.
Mesh VB/IB live on `00BFE050` (`+56`/`+52`), **not** on the
72-byte cell.

### Landscape BG (`0x4`)

**PROVEN** as the **first** layer that can submit 3D (registration
`00B26A75`: `0x4` then `0x40`). `00BDC060` is `if ([this+4])
00BF71D0`. Recovered `00BF71D0` (100 insns) is frustum +
`00BE7BE0` bind; procedural mesh DIP is `00BE6880` / wrapper
`00A0ACA0` (VB bg-patch `+192`, IB `+188`). **Not** `00BF4570`.
Whether that DIP fires on the first nonempty Lookout frame is
**PARTIAL** (clock). The **order** is **PROVEN**.

### Static C3D (`0x20`)

After land. Type `0x18` → `00BBC460` → `00BB2540` DIP
**vtbl+328**, FVF `0x112` stride 32. Packer **UNREAD**. Not
`0041BEB0`.

---

## Host DIVERGE

| Host | Native | Class |
|---|---|---|
| `ApplyDisplayCamera` flushes `ScenePasses` on `009DA9F0` | `009DA9F0` is `+16020` 2D; 3D is `00B27D90` | **DISPROVEN** pairing |
| `DisplaySubmitStages` has no engine `vtbl+32` | frontend has it; game site unread | **DIVERGE** / leftover |
| `PumpFrontendFrame` skips `0042E0BB` | `0042DF9E` always calls it | **DIVERGE** (empty 3D, but the call exists) |
| status row “`00435530` flushes ScenePasses via `009DA9F0` bits `0x4`…” | that is the host Note, not the listing | **DISPROVEN** as native |
| `SubmitCurrentWorld` Concat land+C3D(+sky) one mesh | per-layer DIP | **DISPROVEN** |
| `FlushSubmittedLayers` one-shot after `HeroSpawned` | every `00B27D90` | **TEMPORARY BRIDGE** |
| `00B25950` inside `00435530` | only inside `00B27D90` | **DISPROVEN** |
| Type `0x22` / `0x27` as first world submit | 2D families | **DISPROVEN** |

---

## Classification

| Claim | Status |
|---|---|
| First 3D DX9 DIP after Leave is not `0x22` / `0x27` | **PROVEN** |
| `0x22` is widget sprite; `0x27` is glyph | **PROVEN** |
| Leave stops `0042DF9E`; game Present is `00435530` / `009BEEB0` | **PROVEN** |
| `0042DF9E` Clear-then-Begin; `00435530` Begin-then-Clear | **PROVEN** |
| `0042DF9E` flushes `009DA9F0` twice; game once | **PROVEN** |
| `0042DF9E` calls engine `vtbl+32`; `00435530` does not | **PROVEN** |
| Frontend `00B27D90` issues first terrain / C3D DIP | **DISPROVEN** (empty lists) |
| First game Present already has 3D | **DISPROVEN** |
| First stored 3D DIP is `00BF4570` vtbl+328 on bit `0x40` | **PROVEN** (site) / first-frame clock **PARTIAL** |
| First 3D *layer* after maps is bit `0x4` `00BF71D0` | **PROVEN** (order) / DIP clock **PARTIAL** |
| `009DA9F0` is the 3D layer walker | **DISPROVEN** |
| Game caller of `012A0F3C+32` after Leave | **UNREAD** |
| Type `0x18` thing packer | **UNREAD** |

Dumps: listing `0042DF9E` / `00435530` / `00B27D90` /
`0042EBB6`, `functions.tsv`, `landscape-trace/`
(`00B6B0B0` / `00BDC060` / `00BF4570` / `00BF71D0`),
`implementer/frontend/06-dx9-submit.md`,
`A-dx9-submit.md`, `2026-08-18-scene-layers.md`.
Host unread-only: `EngineLifecycle.ApplyDisplayCamera`,
`PumpFrontendFrame`, `SubmitCurrentWorld`.
