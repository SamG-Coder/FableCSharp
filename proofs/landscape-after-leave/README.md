# First landscape draw after Leave Frontend

Investigation only. Production `src/` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Question: when is the first native landscape **draw** after Leave?
Is that during frontend `0042DF9E`?

Siblings: `proofs/landscape-first-draw/`, `proofs/terrain-first-draw/`,
`proofs/landscape-tex-stages/`, `proofs/stb-first-open/`,
`proofs/dx9-3d-submit/`. Dumps:
`tools/Fable.ExeIndex/out/01-sections/landscape-trace/` (INDEX v5).

---

## Verdict

**First nonempty landscape DIP is after Leave + opened patches.
Not during frontend.**

Frontend **does** reach landscape `vtbl+16`. That is a walk, not a
draw. The patch list is empty. **No `00BDC060` / `00BF4570` DIP.**

| Phase | Native | During frontend? |
|---|---|---|
| Layer walk | `00B6B0B0` (`012A2B54+16`) from every `00B27D90` | **yes** (`0042E0BB`) — empty `+44` |
| First patch submit | `00BDC060` → `00BF71D0` bit `0x4` | **no** |
| First stored-cell DIP | `00BDC2D0` → `00BF4570` → `00A0AD40` vtbl+328 type 5 bit `0x40` | **no** |
| Maps that fill `+44` | `00B428E0` / `00B42750` after Leave | **no** (frontend never opens STB) |
| Leave Present | `0042EBB6` `009BE420` + `009BEEB0` | teardown only; **not** a landscape draw |

Host `PumpFrontendFrame` is 2D widgets. `SubmitCurrentWorld` runs
only after `HeroSpawned`. **PROVEN** as timing. Concat soup is
**not** the native DIP site.

---

## Recovered order (no-save New Game)

```
0042EC7C retail pump
  0042DF9E  frontend frame
    009D8CF0 / 009BEF20
    00595582 / 00595222  type 0x22 UI only
    0042E0BB  [retail+88].vtbl+32 = 00B27D90
      00B25950 → 00B2AB80 → 00B6B0B0
      [0x1436E8C]+44 sentinel (esi==eax)
        no 00BDC060 / no 00BDC2D0 / no 00BF4570
    009D9C80 / 009DA9F0(1) ×2   2D dest
    009BEF50 / 009BEEB0
0042F2A2 Leave
  0042EBB6  teardown Present (not 0042DF9E)
  FinalAlbion.wld
0042F491 Init Game → 00418DCA → 00416953 Load world
  004A1840 → display vtbl+208 00B23DC0 → 00B428E0
    00B42750 OpenStaticMaps
      first-seen Data\Levels\FinalAlbion.stb MISS
      +44 still empty
004189C2 game pump
  WorldFrame<=1: skip 00435530
  later 004AEA70=1: first dest empty, no region, no landscape DIP
later 00501450(1) Lookout
  006C2170 / 004FCBB0 / 0051FD80 / 006AC910
  later 00B428E0 opens Lookout STB     ; exact later site UNREAD
then [0x1436E8C]+44 nonempty
  next 00B27D90 → 00B6B0B0
    bit 0x4  → 00BDC060 → 00BF71D0     first patch walk
    bit 0x40 → 00BDC2D0 → 00BF4570     first stored-cell DIP
```

Game caller of `012A0F3C+32` after Leave is **UNREAD**.
`00435530` has no `E8` / `[reg+32]` to `00B27D90`. Pairing
`00B25950` inside `00435530` is **DISPROVEN**.

---

## Frontend is not a landscape draw

Listing `text-map/listing-00400000.txt` / `functions.tsv`:

```
0042DF9E  this = retail
0042E075  009D8CF0 Clear
0042E080  009BEF20 BeginScene
0042E085  00595582([esi+88]) → 00595222 UI
0042E0BB  call [eax+32]          ; 00B27D90
0042E129  009D9C80
0042E136  009DA9F0(1)
0042E165  009BEF50
0042E170  009BEEB0
```

Direct `E8` callees of `0042DF9E` (**PROVEN** complete for this fn):

`00415A60, 009E1BC0, 00A0B560, 009BECE0, 009D8CF0, 009BEF20,
00595582, 00595222, 0041E5F2, 0041D03C, 009D9C80, 009DA9F0,
00404A80, 00404C00, 009BEF50, 009BEEB0`

None of `00B6B0B0` / `00BDC060` / `00BDC2D0` / `00BF4570` /
`00BF71D0` / `00B428E0`.

`00B6B0B0` empty-list skip (`landscape-draw-vtbl16-00b6b0b0.md`):

```
ebp = [0x1436E8C] + 44
mov eax, [ebp]
mov esi, [eax]
cmp esi, eax
je  skip                    ; no 00BDC060 / 00BDC2D0
```

Leave itself (`0042F2A2`) is audio/UI teardown plus
`0042EBB6` clear/Present. **DISPROVEN** as a landscape draw.

| Claim | Status |
|---|---|
| `0042DF9E` `E8`s landscape DIP | **DISPROVEN** |
| Frontend never reaches `00B6B0B0` | **DISPROVEN** (`0042E0BB` → `00B27D90`) |
| Frontend issues first landscape DIP | **DISPROVEN** (empty `+44`) |
| Type `0x22` is a landscape packer | **DISPROVEN** (UI only) |
| Frontend opens STB / `00B428E0` | **DISPROVEN** (`stb-first-open`) |
| Leave Present is a landscape frame | **DISPROVEN** (`009BE420` + `009BEEB0`) |

---

## First draw after Leave

Registration walks bit `0x4` then `0x40`. After maps exist:

| `arg+4` | Call | What |
|---|---|---|
| `4` | `00BDC060` | `[this+4]` tessellator → `00BF71D0` BG. **Not** the 16 m cell mesh. |
| `0x40` | `00BDC2D0` | AABB then per 72-byte cell `00BF4570`. Mesh IB/VB on `00BFE050` `+52`/`+56`. DIP `00A0AD40` type 5 strip. |

First nonempty **patch** call after Leave is therefore
`00BDC060`. First stored-cell **DrawIndexed** is `00BF4570`.
Both require `[0x1436E8C]+44` nonempty. That list is written
by `00BDD0E0` / `00BDF010` on an STB **hit**. First-seen
`FinalAlbion.stb` **misses**. First hit is later Lookout.

Load is not draw: `00B428E0` during `004A1840` does not DIP.

---

## Host

| Host | Native | Class |
|---|---|---|
| `PumpFrontendFrame` 2D `FrontendBatch` | `0042DF9E` widgets + empty `00B27D90` | **PROVEN** as no landscape verts |
| `SubmitCurrentWorld` after `HeroSpawned` | after `006C2170` + opened patches | **PROVEN** timing |
| `WorldSubmitted` false on first `004189C2` | no region, dest empty | **PROVEN** (`EngineLifecycleTests`) |
| `Concat(land, C3D, sky)` one mesh | layers `0x4` → `0x40` → `0x20` → `0x2000` | **DISPROVEN** as native DIP |
| one-shot `WorldSubmitted` | redraw every `00B27D90` | **TEMPORARY BRIDGE** |

---

## Classification

| Claim | Status |
|---|---|
| First landscape **DIP** is after Leave, not frontend | **PROVEN** |
| Frontend reaches landscape `vtbl+16` with an empty list | **PROVEN** |
| First patch walk after maps exist is `00BDC060` bit `0x4` | **PROVEN** |
| First stored-cell DIP is `00BF4570` bit `0x40` | **PROVEN** as DIP; first-frame clock **PARTIAL** |
| Leave / Init Game / first dest-empty Present DIPs landscape | **DISPROVEN** |
| Game `00B27D90` site after Leave | **UNREAD** |
| Native Lookout STB hit after `FinalAlbion.stb` miss | **UNREAD** |

Dumps: `landscape-trace/INDEX.md`, `landscape-draw-vtbl16-00b6b0b0.md`,
`patch-submit-bit4-00bdc060.md`, `patch-submit-bit40-frustum-00bdc2d0.md`,
`per-cell-submit-00bf4570.md`, listing `0042DF9E` / `00B27D90`.
