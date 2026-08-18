# First landscape patch draw after Leave

Investigation only. Production `src/` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Question: what is the first native landscape **patch** draw after Leave?
Does frontend `0042DF9E` call landscape? How does host `WorldGeometry` miss?

Dumps: `tools/Fable.ExeIndex/out/01-sections/landscape-trace/` (**INDEX** v5).
Prior: `proofs/terrain-first-draw/`, `docs/status/investigations/2026-08-18-landscape-draw.md`.

---

## Verdict

**First nonempty landscape submit is after Leave + `00B428E0` open, not frontend.**

| Native site | When | What |
|---|---|---|
| `00B6B0B0` (`CEngineLandscapeRenderer` vtbl+16) | every `00B27D90` layer walk | switch `arg+4` **4** / **0x40** only |
| `00BDC060` | bit `0x4` (registered first) | `[this+4]` tessellator → `00BF71D0` BG |
| `00BDC2D0` then `00BF4570` | bit `0x40` | patch AABB then per 72-byte cell FG DIP |

Frontend **does** reach `00B6B0B0` via `0042E0BB` → `00B27D90`. The patch list is empty. **No `00BDC060` / `00BF4570` DIP.**

Host `WorldGeometry.Build` soup / `CollectVisibleCells` dump / `SubmitCurrentWorld` Concat is **not** that site.

---

## INDEX VAs (landscape-trace)

| part | va | file |
|---|---|---|
| Landscape draw vtbl+16 | `00B6B0B0` | `landscape-draw-vtbl16-00b6b0b0.md` |
| Patch submit bit4 | `00BDC060` | `patch-submit-bit4-00bdc060.md` |
| Patch submit bit40 frustum | `00BDC2D0` | `patch-submit-bit40-frustum-00bdc2d0.md` |
| Per-cell submit | `00BF4570` | `per-cell-submit-00bf4570.md` |
| SetStaticMapFileForUse | `00B428E0` | `setstaticmapfileforuse-00b428e0.md` |
| vtbl renderer | `012A2B54+16` | `vtbl-cenginelandscaperenderer-012a2b54.md` |

`012A2B54[4] = 00B6B0B0`. **PROVEN.**

---

## Recovered order (no-save New Game)

```
0042EC7C retail pump
  0042DF9E  frontend frame
    009D8CF0 / 009BEF20
    00595582 / 00595222  [ui+84] vtbl+8  type 0x22 only
    0042E0BB  [retail+88].vtbl+32 = 00B27D90
      00B25950 → 00B2AB80 → 00B6B0B0
      [0x1436E8C]+44 sentinel (esi==eax) → no 00BDC060 / no 00BDC2D0
    009D9C80 / 009DA9F0(1) ×2   2D dest
    009BEF50 / 009BEEB0
0042F2A2 Leave
  0042EBB6  teardown Present (not 0042DF9E)
  FinalAlbion.wld
  00418DCA Init Game → 00416953 Load world
    00B23DC0 → 00B428E0 SetStaticMapFileForUse
      00B42750 OpenStaticMaps
        STB hit → 00BDD0E0 / 00BDF010 attach → list +44
        first-seen FinalAlbion.stb MISS
004189C2 game pump
  WorldFrame<=1: skip 00435530
  later 004AEA70=1: 00435530 first dest empty, no region
00501450(1) Lookout
  006C2170 / 004FCBB0 / 0051FD80 / 006AC910
  later 00B428E0 opens Lookout STB   ; exact later site UNREAD
then [0x1436E8C]+44 nonempty
  next 00B27D90 → 00B6B0B0
    bit 0x4  → 00BDC060 → 00BF71D0     first patch walk
    bit 0x40 → 00BDC2D0 → 00BF4570     first stored-cell DIP
```

Game caller of `012A0F3C+32` after Leave is **UNREAD** (`00435530` has no `E8`/`[reg+32]` to `00B27D90`).

---

## `00B6B0B0` — layer switch

`landscape-draw-vtbl16-00b6b0b0.md`:

```
ebp = [0x1436E8C] + 44          ; patch list
eax = [arg+4]
cmp eax, 4                      ; 00B6B122
je  bit4
cmp eax, 64                     ; 00B6B12B
jne unbind                      ; 00B67510 then 00A05840
```

| `arg+4` | setup | walk |
|---|---|---|
| `4` | `00B67480` + `00B671A0` if `[this+1553]` | `[[node+8]]` → `00BDC060` |
| `0x40` | `00B68DA0` + `[this+1552]` then `00B67480` / `00B677D0` / `0098B5E0(2)` | same list → `00BDC2D0` |
| other | `00B67510` unbind 0/1/2 | no landscape DIP |

Empty list: `mov esi,[eax]; cmp esi,eax; je` — **no patch call**. **PROVEN.**

Registration `00B26A75`: `0x4` then `0x40`. First nonempty **patch** call after Leave is therefore **`00BDC060`**, then FG `00BDC2D0`/`00BF4570`.

---

## `00BDC060` — first patch submit (bit 4)

`patch-submit-bit4-00bdc060.md` (20 insns):

```
00BDC060  mov ecx, [ecx+4]     ; tessellator*
          test ecx, ecx
          je  ret
          push 0
          call 00BF71D0        ; BG frustum + procedural mesh
          ret
```

**Not** `00BF4570`. BG VB/IB live on the tessellator / `CLandscapeBackgroundPatch` (`+192`/`+188`), wrapper `00A0ACA0`. **PROVEN.**

---

## `00BF4570` — first stored-cell DIP (bit 0x40)

Caller is **`00BDC2D0`** (`00BDC3A4`), not `00BDC060`.

`00BDC2D0`: if `[this+8]` cells exist, 4-plane AABB on tessellator `+168`/`+180` (Z=0). Missing `[this+4]` submits every cell. Then:

```
cell = [this+8] + (row * cols + col) * 72
call 00BF4570
```

`00BF4570` head:

```
test [ebp+60], 0x04
je  skip                       ; 00BF5864
… 00BF3860 cell AABB …
```

DIP (`2026-08-18-landscape-draw.md`): mesh node from cell `+8`, IB `mesh+52`, VB `mesh+56`, type 5 strip, `00A0AD40` → device vtbl+**328**. Cell `+52/+56/+68` are AABB / origin / refcount, **not** IB/VB/NumVerts.

---

## Frontend `0042DF9E` vs landscape

Listing `text-map/listing-00400000.txt` / `functions.tsv`:

```
0042DF9E  this = retail
0042E075  009D8CF0 Clear
0042E080  009BEF20 BeginScene
0042E085  00595582([esi+88]) → 00595222 UI
0042E0BB  call [eax+32]          ; 00B27D90  (not E8 landscape)
0042E129  009D9C80
0042E136  009DA9F0(1)
0042E147  009D9C80
0042E15A  009DA9F0(1)
0042E165  009BEF50
0042E170  009BEEB0
```

Direct `E8` callees (**PROVEN** complete for this fn):

`00415A60, 009E1BC0, 00A0B560, 009BECE0, 009D8CF0, 009BEF20, 00595582, 00595222, 0041E5F2, 0041D03C, 009D9C80, 009DA9F0, 00404A80, 00404C00, 009BEF50, 009BEEB0`

None of `00B6B0B0` / `00BDC060` / `00BDC2D0` / `00BF4570` / `00BF71D0`.

**“0042DF9E does not call landscape”:**

| Claim | Status |
|---|---|
| Does not `E8` `00B6B0B0` / `00BDC060` / `00BF4570` | **PROVEN** |
| Never reaches landscape `vtbl+16` | **DISPROVEN** (`0042E0BB` → `00B27D90` → `00B6B0B0`) |
| Issues first landscape DIP | **DISPROVEN** (empty `+44`, no cell/`+60` bit 4) |
| Type `0x22` is landscape packer | **DISPROVEN** (UI only) |

---

## `WorldGeometry.cs` mismatch

| Host | Native | Class |
|---|---|---|
| `Build(..., expandGeometry: true)` default: neighbours + `SkyGeometry` soup | open is `00B3EFA0` / `00B428E0`; draw is later `00B6B0B0` | **DISPROVEN** as first draw |
| `PresentWorld` `expandGeometry: false` | headers + handles only | **PROVEN** as open-shaped |
| `AddTerrain` / `TessellateVisible` map AABB then **all** tiles | `00BDC2D0` then per-cell `00BF3860` then mesh list | **DIVERGE** |
| `CollectVisibleCells` dumps every cell with faces; no `+60 & 4` | `00BF4570` requires flag bit `0x4` | **DIVERGE** |
| `TessellatePrimary` unused at submit | not the native path | **DISPROVEN** as path |
| `SubmitCurrentWorld` `Concat(land, C3D, sky)` one mesh | layers `0x4` → `0x40` → `0x20` → `0x2000` | **DISPROVEN** as native |
| one-shot `WorldSubmitted` after `HeroSpawned` | redraw every `00B27D90` | **TEMPORARY BRIDGE** |
| `LandscapeDraw` comment “VB/IB on cell `+56/+52`” | those offsets are origin / AABB max.Z; buffers on `00BFE050` | **DISPROVEN** |
| `BothPasses` same VB on bit 4 | bit 4 is `00BDC060`/`00BF71D0` | **DISPROVEN** (host now FG-only) |

Must-after-load (no frontend soup): **PROVEN.**

---

## Classification

| Claim | Status |
|---|---|
| First landscape **patch** walk after maps exist is `00BDC060` on bit `0x4` | **PROVEN** (order + empty-list skip) |
| First stored-cell DIP is `00BF4570` on bit `0x40` | **PROVEN** as DIP; first-frame clock **PARTIAL** |
| That DIP is after Leave, not `0042DF9E` widgets | **PROVEN** |
| `0042DF9E` `E8`s landscape | **DISPROVEN** |
| `0042DF9E` never reaches `00B6B0B0` | **DISPROVEN** |
| `00BDC060` DIPs the 16 m cell mesh | **DISPROVEN** |
| Game `00B27D90` site after Leave | **UNREAD** |
| Native Lookout STB hit after `FinalAlbion.stb` miss | **UNREAD** |

Dumps: `landscape-trace/INDEX.md`, the three VAs above, listing `0042DF9E` / `00B27D90`, `scene-pass.md`, `2026-08-18-landscape-draw.md`.
