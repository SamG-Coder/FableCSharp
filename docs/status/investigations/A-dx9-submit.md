# A — Native DX9 draw submission graph

Investigation only. No production source (`src/`, `tests/`) was modified.
Dumps: `tools/Fable.ExeIndex` `fn` / `calls` / `vtbl` / `calldisp` / `disasm`
plus `out/01-sections/{landscape-trace,newgame-trace,render}` and
`out/03-pseudo/scene-pass.md`.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

---

## Verdict (read this first)

There are **two different queues** and a **third immediate path**.

| Path | Owner | Insert | Drain / draw | First-seen New Game |
|---|---|---|---|---|
| 2D sprite/text | display `0x13B8384` `+16020` | `009DB700` / `009DD8F0` | `009DA9F0` DIP vtbl+332 | **empty** |
| Typed scene records | MainScene `0x1436E80` lists | `00B324A0` → handler vtbl+20 → `00B94D30` | `00B25950` → `00B2AB80` → renderer vtbl+16 | frontend widgets only (type `0x22`) |
| Landscape cells | patch objects | stored at load (not `+16020`) | `00B6B0B0` bit `4`/`0x40` → `00BDC2D0`/`00BF4570` | first-seen **empty 3D** until engine vtbl+32 runs |

`SubmitCurrentWorld` (tessellate + flatten C3D + one `TexturedMesh` + `SetMesh`) is **DISPROVEN** as native-equivalent.

Frontend type `0x22` is **not** the world / terrain / C3D / creature / PALSKIN / effects insert path.

`00435530` does **not** `E8 00B25950`. The engine scene walk is `012A0F3C+32` = `00B27D90`. The only recovered **E8-less** site that is actually the engine object is frontend `0042E0BB` `[retail+88].vtbl+32`. Game-display call of that slot is **UNREAD**.

---

## Answers

### 1. Is `SubmitCurrentWorld`'s "tessellate + flatten C3D + one TexturedMesh + SetMesh" native-equivalent?

**DISPROVEN.**

Native:

- Terrain is **per opened patch** `00BDC2D0` four-plane AABB, then **per 72-byte cell** `00BF4570` (flag `+60` bit `0x4`). VB stride 24 lives on the cell (`+56`), IB at `+52`. Not a host triangle soup.
- C3D / static / PALSKIN are **typed records** inserted into MainScene lists (`00B94D30` / `00B8FDF0`), drained later by layer bit. PALSKIN draw is `00BD7110` / `00BD71B0` / `00BD3070` (palette + VS constants). Not `TrianglesForPose` flattened into the landscape mesh.
- First-scene host `SubmittedMesh = Concat(land, BuildMeshes(props))` + one `SetMesh` has **no** native twin.

See also `docs/status/investigations/C-terrain-static-map.md`.

### 2. Where are landscape patches submitted (`00BDC2D0` / `00BF4570` / layer bits)?

**PROVEN** insert-at-load / **PROVEN** draw-from-layer / **not** `+16020`.

- Layer bits first-seen drawn: `0x4` → `0x40` → `0x20` → `0x2000` (`ScenePasses.Registration`; landscape only `4` and `0x40`).
- `00B6B0B0` (`CEngineLandscapeRenderer` `012A2B54+16`):
  - `layer+4 == 4`: `00B67480` lighting, `00B671A0` BG, walk `[0x1436E8C]+44` → `00BDC060`.
  - `layer+4 == 0x40`: `00B68DA0` FG compact+bind, `00B67480`, `00B677D0` device dirty, `0098B5E0(2)`, walk same list → **`00BDC2D0`**.
  - else: unbind `00B67510`.
- `00BDC2D0` only caller: `00B6B1A5`. Uses `[0x1436EA0]+0x1C8` four planes vs patch AABB `[esi+168..188]`. Pass → grid `[edi+8]` cells `lea ecx,[base+(row*cols+col)*72]` → **`00BF4570`**.
- `00BF4570` only caller: `00BDC3A4`. Immediate cell path (state blocks, `009881F0` matrix, `00BF50E0` stage-0 texture). **DIP site inside the 0x2F0-frame body is PARTIAL** (decoder desync at `00BF46A0`). It does **not** write `+16020`.

### 3. Where are C3D / mesh renderer records queued vs drawn?

**PARTIAL** (queue vs drain split is proven; thing-side packer is UNREAD).

**Queued (not type `0x22`):**

- Shared insert: `00BACDD0` → `00B94D30` (octree `00B94BF0`/`00B93B30` when flags allow) or `00B8FDF0` list splice onto MainScene `+32` / `+36`.
- `00B324A0` is the type switch that creates/reuses a handler and then inserts. Callers: **only** `00B23BC0` (engine `012A0F3C+92`) and `00B32E90` (hard-coded type **4**).
- Mesh **families** live on MainScene (`00B33B50`):
  - PALSKIN / animated: `00BD1EF0` `"EnableAnimatedMeshes"` vtbl `012A7914` / `012A78DC`. Types **`0x9`, `0xB`, `0xD`** (`00BD27F0`). Sibling list `00BD28A0` types `0x25`, `0x26`.
  - Static: `00BBBF40` vtbl `012A5BB8`. Type **`0x18`** (`00BBCF30`).
  - Lighting factory `00B482A0` registers types **`0xF`, `0x10`** via the only `E8 00B8FAD0`.
- Static **draw** switch `00BBC460` (`012A5BB8+20`) walks subsets and `00BBC130` re-looks-up `[0x1436E84]+16+type*4` then **factory vtbl+20**. That is drain-time, not first insert.

**Drawn:**

- `00B25950` layers `[manager+348, +352)` → `00B2AB80` → renderer `vtbl+16`.
- MainScene+616 vtbl `012A1348+16` = `00B33010`. Bit `0x20` is the first-seen primitives drain (`00B93F40` / `00B849F0` → record `+20` vtbl+20 / +24).
- PALSKIN drain/draw: `012A7914+20` = `00BD7110` (or wrapper `00BD7A00`); `+28` = `00BD71B0`; software/bind body `00BD3070` (VS `00989A60`, state `00988020` / `00988140`). Immediate device constants. **Not** `+16020`.

**UNREAD:** the thing/world packer that fills a mesh-type record and calls engine vtbl+92 / `00B324A0` with type `0x9`/`0x18`/…. It is **not** `0041BEB0`.

### 4. What does handler vtbl+20 of `00B324A0` actually do?

**PARTIAL** for the dispatcher; **UNREAD** for type `0x22`.

`00B324A0(this=0x1436E80, dest, rec, flags, extra)`:

1. `type = [rec+0]`.
2. `factory = [[0x1436E84]+16+type*4]` (`00B8FAD0` writes those slots).
3. If `dest+4` already holds a handler with `[handler+8]==type`: **`call [handler.vtbl+20](rec, flags)`**, then `00B91840`.
4. Else release old `dest+4`, `factory.vtbl+4` construct, store at `dest+4`, then **`call [handler.vtbl+20](rec, flags)`**.
5. If `flags & 0x40`: `00B4A670` → `00B8FDF0` list insert. Else `00BACDD0` → `00B94D30`.
6. First-seen dest+4 = 0. If factory is also 0: `je 00B325FA` and **vtbl+20 is never called**.

Known family vtbl+20 (these are **not** type `0x22`):

| Family | vtbl | +20 | Effect |
|---|---|---|---|
| PALSKIN | `012A7914` | `00BD7110` | pack / subset drain; may `00BD3070` |
| Static | `012A5BB8` | `00BBC460` | subset walk + `00BBC130` |
| Lighting | `012A2274` | `00B4A450` | **`ret 8` stub** |

Type `0x22` factory slot `[table+16+0x22*4]` has **no** recovered `00B8FAD0` writer (lighting is `0xF`/`0x10` only). So type-`0x22` handler vtbl+20 remains **UNREAD** (and first-seen is consistent with factory 0 / no DIP: host `Frontend2dDipIssued=false`).

`00B91840` (after a cache hit): if `!(flags & 0x40)` then `00B8FE60` + `00BACDD0` again.

### 5. What is the first nonempty `009DA9F0` path after New Game Present?

**PROVEN empty first-seen. PARTIAL for the first nonempty producer.**

- `009DA9F0` count = `([this+16024]-[this+16020]) * 0x88888889` (60-byte records). Zero → `009DB6E6` skip DIP. **First-seen is this skip.**
- Nonempty tail: `00A058C0` then `[device+88].vtbl+332` with VB `[this+16008]`, stride 32, prim **2** or **4**. Then `009E15E0` / `009E1440` clear both vectors.
- The **only** recovered `E8 009DB700` sites are `009DC00E` and `009DD93D`. `009DD8F0` (called from `00435530` HUD/debug strings) is the game-side producer.
- Frontend widgets do **not** fill `+16020`; they pack type `0x22` and call dest vtbl+92. Frontend `0042DF9E` still calls `009DA9F0(1)` twice on an empty vector.
- Which `00435530` `009DD8F0` gate (`0x13B860C` / `0x13B86E7` / `[0x13CAA40+120]` / …) first opens after no-save New Game is **UNREAD**. Until one does, every game `009DA9F0(1)` stays the empty skip.

---

## Graph nodes

### `00417001` — game render (pre-Present)

**PROVEN**

- native VA: `00417001`
- object/vtable: game object (`ecx`); not a renderer
- record layout: n/a
- queue ownership: none
- submission order: WorldFrame `0049D870` must be `>1`. `[0x13B8630]==0` → interpolation; else `004164E0` catchup. Then `0049E080` then `00435F70`.
- primitive type / VB/IB / material / transform / DX9: none
- actual DX9 call: none (`00435F70` does Present)

`0049E080` is camera/thing walk (`004C74F0`, `0051EBD0`, `006B42F0`). **DISPROVEN** as mesh submit. `[0x13B8394].vtbl+12` is engine dtor-shaped `00B23990` (`push 1; call [eax]`), not draw.

### `00435F70` — display thunk

**PROVEN**

- native VA: `00435F70` = `jmp 00435530`
- object/vtable: display object `game+40`

### `00435530` — game display apply

**PROVEN** (order) / **PARTIAL** (overlay/interface / HUD gates)

- native VA: `00435530`
- object/vtable: display (`esi`); device `0x13B8390`; 2D owner `0x13B8384`
- record layout: n/a
- queue ownership: **drains** `0x13B8384+16020` only. Does not walk engine layers.
- submission order (recovered E8s):
  1. `00A0BF20` / `00A0B560` viewport-ish
  2. `009BEF20` BeginScene (device vtbl+164)
  3. `009D8CF0` Clear
  4. gated `[esi+224]` vtbl+8 / +12
  5. `009A4EC0`+`009F8D60` (**`009F8D60` is `ret 8`**)
  6. `0049D9D0` / `00633BE0` / `00487570`
  7. `00434870` fade tick
  8. **`00435000` overlay**
  9. **`00435070` interface**
  10. gated `009DD8F0` HUD strings → `009DB700`
  11. **`009D9C80` Flush2D**
  12. **`009DA9F0(1)` FlushLayers**
  13. `009BEF50` EndScene
  14. `009BEEB0` Present (`00435F4A`, arg3 nonzero)
- primitive type: none in this fn
- actual DX9 call: BeginScene / Clear / EndScene / Present only; DIP is in the flushes

**No E8 to `00B25950` / `00B27D90`.** Host `ApplyDisplayCamera` notes `00B25950` here — that pairing is host, not this body.

### `00435000` — player overlay

**PROVEN**

- native VA: `00435000`
- object/vtable: display; looks up player via `00449960` / `00487DD0`
- record layout: component slot `0x8E` on thing `+68` map (`004365B0`)
- queue ownership: none (text blit)
- submission order: after Clear, before interface
- primitive type: n/a (string)
- VB/IB / material / transform: n/a
- actual DX9 call: none directly
- callee: **`00639E40`** → `005BCAFE` (text). Gates: `[eax+145] bit0`, `[eax+48] 0x4000`, `[0x13B8790+656]`.

### `00435070` — player interface

**PROVEN** (entry) / **PARTIAL** (first-seen skip)

- native VA: `00435070`
- object/vtable: `[0x13B86A0]+28` → `00449970` / `00487DC0`
- record layout: requires `[thing+32] & 0x10`; slot **4** on `+68`
- queue ownership: may pack type `0x22` via `0057B43F`
- submission order: after overlay, before Flush2D
- `0057B43F`: builds a `0xC0` rec with **`0041BEB0`**, then `[arg].vtbl+92` (`00B23BC0`). First-seen often skips (`vtbl+1580`, flags).

### `00639E40` — overlay apply

**PROVEN**

- native VA: `00639E40`
- object/vtable: overlay component
- actual DX9 call: none; `005BCAFE` + `009BEDC0` viewport query
- transform owner: GUI string metrics `0043E4D0`

### `0057B43F` — interface apply

**PROVEN**

- native VA: `0057B43F`
- record layout: `0041BEB0` type `0x22` (see below)
- queue ownership: dest `esi+0x1C0` via vtbl+92
- actual DX9 call: none (enqueue only)

### `009D9C80` — Flush2D

**PROVEN** (identity) / **PARTIAL** (body)

- native VA: `009D9C80`
- object/vtable: display `0x13B8384`
- record layout: dirty-list / state-block walk (same `+10248` pattern as `009DA9F0`)
- queue ownership: 2D state, **not** type `0x22`
- submission order: immediately before `009DA9F0`
- `009D9C80–009DB000`: no `cmp …,0x22`
- actual DX9 call: may DIP (fnmap lists `00A058C0`); first 250 insns are dirty-list only

### `009DA9F0` — FlushLayers (2D sprite batch)

**PROVEN**

- native VA: `009DA9F0` (`ret 4`)
- object/vtable: display `0x13B8384` (`ebp`)
- record layout: 60-byte sprite at `+16020…+16024`; count via `0x88888889` (`imul` / `sar 5`)
- queue ownership: **this display object only**. Cleared at end (`009E15E0` records, `009E1440` VB `+16008`)
- submission order: last draw before EndScene
- primitive type: D3DPT **2** (triangle list) if `[esp+16]`, else **4** (triangle strip)
- VB/IB source: `this+16008` (32-byte verts), `push 32`
- material/state owner: display state blocks `+10248…`; `0098B5E0` Diffuse2X restore
- transform owner: `00A0AA80` / per-record `+4` quad
- actual DX9 call: `[device+88].vtbl+332` `DrawIndexedPrimitive` at `009DB645`

`009DB700` (next fn, `ret 24`) is the **enqueue**: skip if `[device+472]`; build 60-byte local; `add [+16024], 60` or grow `009E1750`.

### `009DD8F0` — HUD string → `+16020`

**PROVEN**

- native VA: `009DD8F0`
- calls `009DB700` with scale `1.0`
- callers include `00435530` (several gated sites)

### `00B23BC0` — engine vtbl+92 (type packer caller)

**PROVEN**

- native VA: `00B23BC0` (`012A0F3C+92`)
- object/vtable: engine
- body: `00B324A0([0x1436E80], dest, rec, size, 0)` — thin wrapper
- record layout: whatever `[rec+0]` type the packer wrote
- queue ownership: MainScene `0x1436E80`
- actual DX9 call: none

### `00B324A0` — frontend / scene type dispatch

**PROVEN** (control flow) / **PARTIAL** (per-type handler)

- native VA: `00B324A0` (`ret 16`)
- object/vtable: MainScene `0x1436E80`; factory table `0x1436E84` (ctor `00B8FAA0`, vtbl `012A3D14`)
- record layout: dword type at `[rec]`; dest is `{ vtbl, handler* }` at e.g. widget `+0x15C`
- queue ownership: MainScene lists / octree via `00B94D30` / `00B8FDF0`
- submission order: pack → this → later `00B25950` drain
- primitive type / VB/IB: none here
- actual DX9 call: none
- callees: `00B91840`, `00B4A670`, `00BACDD0`
- **DISPROVEN:** memcpy into `+16020`

### `0041BEB0` — type `0x22` packer

**PROVEN**

- native VA: `0041BEB0` (`ret 68`)
- object/vtable: writes into `ecx` dest buffer
- record layout (`0xC0` used at submit, this fn fills through `+80`):
  - `+0` = **`0x22`**
  - `+4` = arg
  - `+8..+10` = 0
  - `+12..+24` = rect copy from arg ptr
  - `+28..+48` = more args
  - `+52..+55` = packed bytes
  - `+56` = 0, `+60`, `+64` = 0
  - `+68..+80` = two vec2
- queue ownership: none (packer only)
- sibling `0041BF60` is the textured/alt packer (`0041AFA0` uses it when `[widget+380]!=0`)
- **DISPROVEN** as C3D / landscape / PALSKIN packer

### `0041AFA0` — frontend widget draw

**PROVEN**

- native VA: `0041AFA0` (`[node+20]` type-0 vtbl `0122F5D4` draw)
- packs `0041BEB0` type `0x22` (or `0041BF60`), dest `this+0x15C`, size `0xC0`, **`call [edx+92]`** = `00B23BC0`
- actual DX9 call: none

### `00B27D90` — engine vtbl+32 (scene submit)

**PROVEN** (body + frontend site) / **UNREAD** (game-display caller)

- native VA: `00B27D90` (`012A0F3C+32`)
- object/vtable: engine (stored `retail+88`, BSS `0x1436EA4`)
- record layout: n/a
- queue ownership: **drains** layers; does not fill `+16020`
- submission order: `00B23A90` device reset-ish; optional `00B277A0`; **`00B25950`**
- actual DX9 call: device vtbl+400 / +416 / +260 (stream clear)
- recovered site: **`0042E0BB`** `mov ecx,[esi+88]; call [eax+32]` inside frontend `0042DF9E`
- `calldisp 0x20` at `004188E9` is **game** vtbl+32 `00416953` (Loading world), **not** this
- 0 `E8` callers (vtbl only)

### `00B25950` — ScenePass walker

**PROVEN**

- native VA: `00B25950`
- object/vtable: engine / CRenderManager (`ebx`)
- record layout: ctx copy `rep movsd` 0x1C dwords; `ctx+120` = `manager+184` mask
- queue ownership: walks existing layers
- submission order:
  1. components `[+360,+364)`: `vtbl+40` query vs `+184`, then `vtbl+4` prepare
  2. layers `[+348,+352)`: `layer.vtbl+4` = `00B2AB80`
  3. components `vtbl+8` after
- actual DX9 call: none directly
- only caller: `00B27E87`

### `00B2AB80` — per-layer submit

**PROVEN**

- native VA: `00B2AB80`
- object/vtable: layer (`esi`); `+4` bit, `+12` mask, `+16…+20` renderer*
- skip if `(ctx+120 & layer+12)==0`
- for each renderer: `vtbl+40` query; `vtbl+20` prepare; if `[r+8]` then **`vtbl+16` draw**; `vtbl+24` after

### `00B6B0B0` — landscape vtbl+16

**PROVEN**

- native VA: `00B6B0B0` (`012A2B54+16`)
- object/vtable: `CEngineLandscapeRenderer`
- record layout: patch list `[0x1436E8C]+44` doubly-linked; cell 72 bytes
- queue ownership: **immediate**; not MainScene type table; not `+16020`
- submission order: bit `4` then (later layer) bit `0x40` (registration is `4` then `0x40` then `0x20`)
- primitive type: cell-stored (strip extras UNREAD)
- VB/IB source: cell `+56` / `+52` (see investigation C)
- material/state owner: `00B67480` / `00B68DA0` / `00BF50E0`
- transform owner: `009881F0` onto `0x1436E14`; camera-relative in `00BF46A2` (parity already locked)
- actual DX9 call: inside `00BF4570` (**PARTIAL**)

### `00BDC2D0` — patch AABB then cells

**PROVEN**

- native VA: `00BDC2D0`
- object/vtable: `CEngineLandscapePatch` (`edi`); `[edi+4]` AABB source; `[edi+8]` cell base; `[+12]` cols; `[+16]` rows
- reject if any of 4 planes fails (`je 00BDC3BA` before the grid)
- actual DX9 call: none (delegates)

### `00BF4570` — per-cell submit

**PARTIAL**

- native VA: `00BF4570`
- object/vtable: 72-byte cell (`ebp`)
- record layout: `+60` flags (need `0x4`); early-out `00BF5864`
- queue ownership: immediate device wrapper `0x13BC470` / `0x1436E14`
- material/state owner: `00BF50E0` stage0; leftover `c3` table `0x0139C614`
- transform owner: `009881F0`
- actual DX9 call: **UNREAD** as a concrete vtbl+332 site (body too large; x87 island at `00BF46A0`)

### `00B33010` — MainScene+616 vtbl+16 (layer drain)

**PROVEN** (switch) / **PARTIAL** (per-bit bodies)

- native VA: `00B33010` (`012A1348+16`); `this` is MainScene+616 so `lea ebx,[edi-616]`
- object/vtable: `012A1348`
- record layout: `arg+4` = layer bit
- bits recovered:
  - `2`: shadow path `00B94060` over `[+208,+212)`
  - `0x20`: default fall-through `00B93F40` / `00B942A0`
  - `0x80` / `0x100` / `0x200` / `0x400` / `0x1000` / `0x40000` / `0x8000000`: `00B849F0` prim-queue drain (slot index in the push)
  - `0x20` is the first-seen **Primitives** bit in `ScenePasses`
- actual DX9 call: via `00B849F0` → record `+20` vtbl+20 / +24 (family draw), **not** `+16020`

### `00B93F40` / `00B92500` — spatial drain

**PROVEN**

- native VA: `00B93F40` / `00B92500`
- walks MainScene linked records; `00B92500` calls `factory.vtbl+28` (`[0x1436E84]+16+[esi+8]*4`) for visibility
- PALSKIN `+28` = `00BD71B0`; static `+28` = `00BBBF70`

### `00B849F0` — prim-queue drain

**PROVEN**

- native VA: `00B849F0`
- slot = `lea esi,[ebp+(3*index+3)*8]`
- if `+20`: walk `[+0,+4)` 8-byte ents → `[ent+20].vtbl+24`
- else `00B84320` / `00B842D0`; optional `00B84400`
- `00B84320`: `[rec+20].vtbl+20` then follow `+16`

### `00BD7110` — PALSKIN vtbl+20

**PROVEN** (control) / **PARTIAL** (payload)

- native VA: `00BD7110` (`012A78DC+20` / `012A7914+20`)
- if `[rec+32]`: look up subset via `00B812C0`, then `[eax].vtbl+36`
- else `00BD3070(rec, flags)`
- wrapper `00BD7A00`: optional extra pass if `[rec+4]&1`, then `00BD7110`

### `00BD71B0` — PALSKIN vtbl+28 / draw entry

**PROVEN** (entry) / **PARTIAL** (DIP)

- native VA: `00BD71B0`
- skip if `[this+8]==0`
- uses `[0x1436E80]`, `[0x1436EA0]` camera, mesh `+80` / `+236` bones
- actual DX9 call: later in the 696-insn body (not fully walked here)

### `00BD3070` — PALSKIN bind + constants

**PARTIAL**

- native VA: `00BD3070` (2000+ insns)
- state-block dirty on `0x1436E18`
- `00989A60` VS constants; `00988020` / `00988140` attach
- palette / subset via `00BA9360` / `00BA95D0` / `00BA9A70`
- **DISPROVEN** as host flatten-to-`TexturedMesh`

### `00BBC460` — static family vtbl+20

**PROVEN**

- native VA: `00BBC460` (`012A5BB8+20`)
- copies mesh AABB `+92…+104` onto `this+14`
- dirty-state walk; `00B46C80` lights; `0098B5E0(5)`
- subset list `[mesh+72]` → `00BBC130(type=[subset+8])`
- actual DX9 call: inside type cases (`00BC04F0` / `00BBE090` / factory vtbl+20)

### `00BBC130` — static type switch

**PROVEN**

- native VA: `00BBC130`
- jump table on `[rec+8]-7`
- default: `factory = [0x1436E84+16+type*4]`; **`call factory.vtbl+20`**
- this is **draw**, not `+16020` insert

### `00B33B50` — MainScene ctor (family table)

**PROVEN**

- native VA: `00B33B50` (`0x1436E80`)
- `+616` vtbl `012A1348`; self vtbl `012A1380` (RTTI-ish)
- constructs families: `00BABB50`, `00BA1A70`, **`00BD1EF0` PALSKIN**, `00B99D90`, `00BACF70`, `00BCE070`, `00BCC5F0`, `00BCA770`, `00BC7E90`, `00BC6B50`, `00BBBF40` static, …
- `"MainScene"` / `"RepMeshScene"` octrees via `00B94A80`

### `00B8FAA0` / `00B8FAD0` — type factory table

**PROVEN**

- native VA: ctor `00B8FAA0` stored at `0x1436E84` by engine ctor `00B26548`
- `+16` is `0x2F` dword slots (types 0…)
- `00B8FAD0`: factory `vtbl+8` returns type-id vector; `mov [table+16+type*4], factory`
- only `E8` caller: `00B48491` (lighting, types `0xF`/`0x10`)
- other families must register via a **UNREAD** path (vtbl or inlined); slots for `0x9`/`0x18` are used at runtime by `00BBC130` / `00B92500`

### `00B26360` — engine ctor (layer registration)

**PROVEN**

- native VA: `00B26360` (`012A0F3C+8`); layer allocs around `00B26A75`
- `00B262C0` `"Engine: Add Render Layer"` pushes layer*; `00B2AC80` attaches renderer
- layer bits include `0x4`, `0x40`, `0x20`, `0x2000`, `0x80`, `0x200`, `0x4000000`, …
- subsystems: type table `0x1436E84`, MainScene `0x1436E80`, camera `0x1436EA0`, lighting `0x1436E9C`, landscape `00B69000`, sky `00B625E0`, water `00B73760`

### `00B662F0` — sky vtbl+16

**PROVEN** (switch)

- native VA: `00B662F0`
- bit `0x400000` → `00B64550`; else-path (first-seen `0x2000`) → `00B66190` / `00B8D040`
- not `+16020`

### Water enqueue `00BF44A0`

**PROVEN** (enqueue) / **UNREAD** (draw `00B783F0`)

- native VA: `00BF44A0`
- type 4 → list `0x1436E54+0x244`; type 5 → `+0x250`; type 8 → `+0x220`; else `+0x1FC`
- layer bit `0x20000` is registered empty first-seen

### Frontend frame `0042DF9E` (comparison)

**PROVEN**

- `[ui+84]` vtbl+8 widgets → `0041AFA0`
- **`[retail+88].vtbl+32` = `00B27D90`** (`0042E0BB`)
- `009D9C80` / `009DA9F0(1)` **twice**
- this is how 3D layers run on the **frontend** Present. Game `00435530` has no recovered twin.

---

## Where each class is inserted

| Class | Insert site | Queue | Draw site | Status |
|---|---|---|---|---|
| HUD / debug text | `009DD8F0` → `009DB700` | display `+16020` | `009DA9F0` DIP+332 | PROVEN |
| Fade / 2D overlay strings | `00639E40` / `00434870` | text helper, not `+16020` | `005BCAFE` | PROVEN |
| Frontend / GUI quad | `0041BEB0` type `0x22` → vtbl+92 | MainScene dest `+0x15C` | handler vtbl+20 then `00B25950` | PARTIAL (handler UNREAD; first-seen no DIP) |
| Player interface quad | `0057B43F` → `0041BEB0` | same | same | PARTIAL (often skipped) |
| Landscape FG/BG | stored on patch/cell at open | cell `+52/+56` | `00B6B0B0` bits `4`/`0x40` | PROVEN |
| C3D static | UNREAD packer → `00B324A0` type `0x18`? | MainScene lists | `00BBC460` / `00B33010` bit `0x20` | PARTIAL |
| Creature / PALSKIN | UNREAD packer → types `0x9`/`0xB`/`0xD` | MainScene lists | `00BD7110` / `00BD71B0` / `00BD3070` | PARTIAL |
| Effects / decals | renderer sites `00AF8150` / `00B1FF50` | UNREAD | UNREAD | UNREAD |
| Water | `00BF44A0` | `0x1436E54` lists | `00B783F0` bit `0x20000` | PARTIAL |
| Sky | constructed in engine ctor | sky object | `00B662F0` bit `0x2000` else | PROVEN |

**DISPROVEN:** any of the 3D rows insert via `0041BEB0` type `0x22` or via `+16020`.

---

## Host DIVERGE (do not treat as native)

`EngineLifecycle.SubmitCurrentWorld` / `ApplyDisplayCamera`:

- Tessellate surviving maps into one CPU mesh, flatten every primary C3D (PALSKIN skinned on CPU) into the same `TexturedMesh`, `SetMesh` once.
- Notes `00B25950` inside `00435530` even though `00435530` does not call it.
- Treats `009DA9F0` as the layer-bit walker. Native layer walk is `00B25950`/`00B2AB80`; `009DA9F0` is the 60-byte 2D batch.

Correct native split (proposed, not implemented here):

1. Keep `009DA9F0` as 2D-only.
2. Call engine vtbl+32 / `00B25950` **inside BeginScene…EndScene** (frontend already does; recover the game site).
3. Landscape: per-cell DIP from stored VB/IB after `00BDC2D0`.
4. C3D: per-family records, PALSKIN via `00BD3070`, static via `00BBC460`.

---

## UNREAD leaves

| Item | Why it matters |
|---|---|
| Game caller of engine `012A0F3C+32` | Without it, `00435530` never walks landscape / MainScene |
| Type `0x22` factory at `[0x1436E84+16+0x22*4]` | Handler vtbl+20 body |
| Thing/world packer that calls vtbl+92 with mesh types | C3D / creature insert |
| `00BF4570` DIP vtbl | Exact prim / base-vertex |
| `00B783F0` water draw | layer `0x20000` |
| Other `00B8FAD0` registrars | who owns types `0x9`/`0x18`/`0x22` |
| Effects / decal enqueue | `00AF8150` / `00B1FF50` |
| First `009DD8F0` gate that opens after New Game | first nonempty `+16020` |
