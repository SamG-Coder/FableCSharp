# First landscape texture-stage setup after Leave

Investigation only. Production `src/` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Question: what is the first native landscape **SetTexture** after Leave?
Is `00BF50E0` a function? Stage 0 from `cell+1468`?

Dumps: `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00bc0000.txt`,
`listing-00b40000.txt`, `landscape-trace/`, `newgame-trace/per-cell-*`.
Prior: `proofs/landscape-first-draw/`, `docs/status/investigations/2026-08-18-landscape-draw.md`.

---

## Verdict

**First landscape `SetTexture` after Leave is inside the first nonempty
`00BF4570` (bit `0x40`), not frontend.**

| Site | Call | What |
|---|---|---|
| `00BF50C5` → `00BF510D` | `IDirect3DDevice9` vtbl+**260** | **stage 0** = `[renderer + (mesh+40)*8 + 1468]` |
| `00BF5477` → `00BF5491` | same vtbl | **stage 1** = resolved mesh `+20` wrapper, or **0** |
| `00B67510` | same vtbl | after the pass: stages **0/1/2** → NULL |

`00BF50E0` is **not** a function. Decoder start there is mid-instruction.

`cell+1468` does not exist (cell is **72** bytes). Stage 0 is
`CEngineLandscapeRenderer` (`[0x1436EA8]`) `+1468`.

---

## Recovered order (no-save New Game)

```
0042E204 Init Engine
  00B69000 CEngineLandscapeRenderer ctor
    00B6917D  zero 8-byte slots at this+1468
    … VS/PS name bind …
    00B6B063  call 00B67270          ; 5× 128² CreateTexture into +1468
0042DF9E frontend
  0042E0BB → 00B27D90 → 00B6B0B0
    [0x1436E8C]+44 empty → no 00BF4570 → no per-cell SetTexture
    other bits → 00B67510 (cache 0 → no vtbl+260)
0042F2A2 Leave
  00B428E0 open maps (FinalAlbion.stb miss first-seen)
… later Lookout STB attach → list +44 nonempty …
next 00B27D90 → 00B6B0B0 bit 0x40
  00B68DA0 compact shaders (not SetTexture)
  00BDC2D0 → 00BF4570
    [cell+60] & 4, 00BF3860 AABB
    mesh loop 00BF4E90
      00BF50C5 stage 0                 FIRST landscape bind
      00BF5491 stage 1
      00A0AD40 DIP
  00B67510 unbind 0/1/2
```

Frontend **never** issues per-cell `SetTexture`. **PROVEN.**

Game caller of `00B27D90` after Leave is still **UNREAD**
(`proofs/landscape-first-draw`).

---

## `00BF50E0` is mid-instruction

INDEX dump `per-cell-settexture-stage0-00bf50e0.md` starts at `00BF50E0`
and prints `and al, 0x28`. Those two bytes are the tail of:

```
00BF50DE  mov [esp+40], eax          ; 89 44 24 28
00BF50E2  lea eax, [ecx+eax*8+1468]
```

Live prefix (`listing-00bc0000.txt`):

```
00BF50C5  mov eax, [ebx+40]          ; mesh+40  (stream u8, 00BFE162)
00BF50C8  cmp eax, [esp+40]          ; last index this cell (−1 first)
00BF50CC  je  00BF5175               ; skip bind + c2/c3
00BF50D2  mov ecx, [0x1436EA8]       ; CEngineLandscapeRenderer*
00BF50D8  mov esi, [0x1436E18]       ; device wrapper
00BF50DE  mov [esp+40], eax
00BF50E2  lea eax, [ecx+eax*8+1468]  ; &slot[index]
00BF50E9  test eax, eax              ; address; never 0
00BF50ED  mov eax, [eax]             ; IDirect3DBaseTexture9* at slot+0
00BF50F1  xor eax, eax               ; only if lea was 0
00BF50F3  cmp [esi+15616], eax       ; last stage-0 cache
00BF50F9  je  00BF5113
00BF50FB  push eax                   ; texture
00BF50FC  mov [esi+15616], eax
00BF5102  mov ecx, [esi+15600]       ; IDirect3DDevice9*
00BF510A  push 0                     ; stage
00BF510C  push ecx
00BF510D  call [edx+260]             ; SetTexture
00BF5113  mov ecx, [esi+15544]
00BF5119  mov eax, 1
00BF5122  mov [esi+15544], eax       ; if cache < 1
```

`ebx` is the `00BFE050` mesh (`00BF4E6F mov ebx, [esp+20]`; loop
`00BF4E90` uses `ebx+16` scale / `ebx+28` / `ebx+40`). **PROVEN.**

| Claim | Status |
|---|---|
| `00BF50E0` is a function / `SetTexture` entry | **DISPROVEN** |
| Stage 0 from `cell+1468` | **DISPROVEN** (72-byte cell; lea base is `[0x1436EA8]`) |
| Stage 0 = `renderer+1468+(mesh+40)*8` | **PROVEN** |
| Skip if same index in this `00BF4570` | **PROVEN** (`[esp+40]`, init `−1` at `00BF4E75`) |
| Skip `SetTexture` if wrapper `+15616` already holds that pointer | **PROVEN** |

Getter `00BF3530`: `lea eax, [this+arg*8+1468]; ret 4`. Same table.

---

## Who fills `renderer+1468`

Ctor `00B69000` at `00B6917D` zeros 8-byte records from `this+1468`
up to the compact list at `+1508`.

`00B6B063` (still ctor, Init Engine) calls **`00B67270`**:

```
eax = this + 0x5BC                 ; 1468
edi = 0 .. 4                       ; five slots
  009FA280(slot, 128×128, …)       ; CreateTexture into 8-byte rec
  009FA450                         ; lock / bits
  slot += 8
then 128×128 fill (normalize + 00BE2B70 + stos)
esi = this+1468; edi = 5
  009F9DE0(slot)                   ; Unlock
  slot += 8
```

Five **procedural 128×128** textures, ready **before Leave**.
Not a WAD `TextureId` / `TextureId1` decode. **PROVEN** as create+fill.

What the 128² image *means* (hemisphere LUT vs splat) is **PARTIAL**.
`PSHADER_LANDSCAPE_FOREGROUND` uses `t0` only as **alpha**
(`mul_sat r0.w, t0.w, v0.w`). FG `oT0.xy = v3.yz` (extra).

`mesh+40` is the stream **u8** written at `00BFE15D`. It must be a
slot index `0..4` for the lea to stay inside the table. First-seen
Lookout values **UNREAD**.

---

## Stage 1 — `00BF5491`

Not a second look at `+1468`. After `cmp ebp, 4` / `jne 00BF5363`
(first-seen FG is **not** type 4):

```
00BF5363  eax = [esp+20]             ; mesh
00BF5376  esi = [eax+20]             ; resolved tex 0
…
00BF5459  edi = [[esi+8]+28]
00BF5462  eax = [edi+8]
00BF546F  je  → xor eax, eax         ; missing wrapper
00BF5471  eax = [eax]                ; IDirect3DBaseTexture9*
00BF5477  cmp [wrapper+15620], eax
00BF548E  push 1                     ; stage
00BF5491  call [edx+260]             ; SetTexture(1, tex|0)
00BF54A3  eax = 2                    ; high-water
then SetStreamSource (vtbl+400) / SetIndices (vtbl+416) / dirty flush / DIP
```

| Claim | Status |
|---|---|
| `00BF5491` is `SetTexture(stage=1, …)` | **PROVEN** |
| Always `SetTexture(1, NULL)` unbind | **DISPROVEN** (null only if `[edi+8]==0`) |
| Source is mesh `+20` resolved wrapper, not `renderer+1468` | **PROVEN** as the walk |
| First-seen Lookout pointer is a live WAD albedo | **PARTIAL** (no capture) |
| FG RGB is PS `t1` (`mul_x2 t1, v0`) | **PROVEN** (shader tokens) |

Host “primary `TextureId` on t1, `TextureId1` on t0” is the **locked
first-seen colour contract**, not this opcode. Native t0 is the
**ctor 128² table**. Treating t0 as WAD `TextureId1` is **DISPROVEN**
as the native bind.

---

## Pass-level setup / teardown (not per-cell)

Bit `0x40` before the walk (`00B6B0B0`):

- `00B68DA0` — compact 8-byte recs at renderer `+1508`; attach
  `VSHADER_` / `PSHADER_LANDSCAPE_*` (`00988140` / `00988020` in
  the per-cell type jump). **Not** `SetTexture`.
- `00B67480` lights + fog; identity-like 3×4.
- `00B677D0` device dirty.
- `0098B5E0(2)` Diffuse2X state block.

After the walk: **`00B67510`**

```
SetIndices(0)                      ; vtbl+416
SetStreamSource(0,0,0,0)           ; vtbl+400
if [+15616]  SetTexture(0, 0); [+15616]=0
if [+15620]  SetTexture(1, 0); [+15620]=0
if [+15624]  SetTexture(2, 0); [+15624]=0
```

Same cache words as the per-cell binds. **PROVEN.**

Material jump `00BF4F18 jmp [0xBF586C+(type-1)*4]` only when the
layer type changes (`00BF4F0F je 00BF50C5`). Type 4 writes VS **c1**
and is water enqueue — first-seen FG skips. **PROVEN.**

Right after stage 0, `00989A60` uploads table `0x0139C5D8` with
`edi=2` → **c2**, `0x0139C614` → **c3**. Fog restore clobbers c2.
Not albedo UV. **PROVEN** (`LandscapeTextures.PerCellFirstSlot`).

---

## Wrapper cache (device object `[0x1436E18]`)

| Off | Role |
|---:|---|
| +15600 | `IDirect3DDevice9*` |
| +15604 | last IB |
| +15616 | last stage-0 texture |
| +15620 | last stage-1 texture |
| +15624 | last stage-2 texture |
| +15544 | high-water stage+1 (1 after s0, 2 after s1, 3 after unbind 2) |

---

## Host / old notes

| Host or note | Native | Class |
|---|---|---|
| `SetTexture` stage 0 from `cell+1468` (C / PARITY / materials) | renderer table + `mesh+40` | **DISPROVEN** |
| `00BF50E0` as the bind site | mid-`mov [esp+40],eax` | **DISPROVEN** |
| `00BF5491` is albedo stage 1 | opcode yes; NULL-only no | **PROVEN** / **DISPROVEN** |
| Host FG `TextureBind(mask=TextureId1, albedo=TextureId)` | t1 walk is mesh `+20`; t0 is 128² table | **DIVERGE** as opcode; RGB contract **PARTIAL** |
| `LandscapeDraw.cs` “VB/IB on cell +56/+52” | mesh `+56/+52` | **DISPROVEN** (prior) |
| `00B68DA0` binds the two stages | shaders + compact list | **DISPROVEN** as SetTexture |
| Frontend `0042DF9E` first landscape bind | empty `+44` | **DISPROVEN** |
| Table filled at first DIP | ctor `00B67270` at Init Engine | **DISPROVEN** as after Leave |

---

## Classification

| Claim | Status |
|---|---|
| First landscape `SetTexture` after Leave is `00BF510D` stage 0 inside first `00BF4570` | **PROVEN** as site; first-frame clock **PARTIAL** (same as DIP) |
| Stage 0 pointer = `[ [0x1436EA8] + (mesh+40)*8 + 1468 ]` | **PROVEN** |
| That table is five ctor-created 128×128 textures | **PROVEN** |
| `00BF50E0` / `cell+1468` | **DISPROVEN** |
| Stage 1 site is `00BF5491` `push 1` + vtbl+260 | **PROVEN** |
| Stage 1 is always unbind 0 | **DISPROVEN** |
| `00B67510` unbinds 0/1/2 after the pass | **PROVEN** |
| Frontend issues this bind | **DISPROVEN** |
| First-seen `mesh+40` / live stage-1 COM pointer | **UNREAD** |
| Pixel meaning of the five 128² images | **PARTIAL** |

Dumps: `listing-00bc0000.txt` `00BF50C5` / `00BF5491` / `00BFE15D`,
`listing-00b40000.txt` `00B67270` / `00B6B063` / `00B67510`,
`landscape-trace/per-cell-settexture-stage0-00bf50e0.md` (misaligned),
`newgame-trace/per-cell-settexture-stage1-00bf5491.md`,
`2026-08-18-landscape-draw.md` §7 (already flipped `cell+1468`).
