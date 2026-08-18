# 2026-08-18 — MainScene submit / drain (AGENT 7)

Investigation only. No production source (`src/`, `tests/`) was modified.
`EngineLifecycle.cs` was not edited.

Dumps: `tools/Fable.ExeIndex` `fn` / `calls` / `vtbl` / `--exact`
plus `out/01-sections/{render,newgame-trace,landscape-trace}` and
`out/03-pseudo/{scene-pass,render}.md`. Host ledgers: `ScenePass.cs`,
`A-dx9-submit.md`, `C-terrain-static-map.md`, `E-player-palskin.md`,
`G-dx9-vulkan.md`, `PARITY.md` scene-pass rows.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

---

## Verdict (read this first)

MainScene is **insert now, drain later**. Terrain is **not** a MainScene
type. UI 2D is **not** a MainScene type.

```
pack [rec+0]=type
  → engine 012A0F3C+92 = 00B23BC0
    → 00B324A0(MainScene 0x1436E80, dest, rec, flags, extra)
      → factory = [0x1436E84 + 16 + type*4]
      → handler.vtbl+20(rec, flags)          // pack / subset
      → flags&0x40 ? 00B4A670 → 00B8FDF0     // lighting lists
                   : 00BACDD0 → 00B94D30     // octree / +32 / +36
```

```
engine 012A0F3C+32 = 00B27D90
  → 00B25950  components+360 then layers+348
    → layer.vtbl+4 = 00B2AB80
      → renderer.vtbl+20 prepare / +16 draw / +24 after
```

`00B33010` (`012A1348+16`, `this` = MainScene+616) is the typed-record
draw. Bit **`0x20` is `00B32610` then prim-queue slot 0**, not the
octree walk.

**DISPROVEN** (`A-dx9-submit.md` §`00B33010`): “`0x20` is default
fall-through `00B93F40` / `00B942A0`”. Live decode:

```
sub eax, 2          ; bit 2  → 00B94060 shadows
sub eax, 30         ; 2+30 = 0x20 → 00B32610
jne 00B33299        ; every other unlisted bit → 00B93F40
```

`00B32610` walks MainScene `+628…+632` (8-byte ents) and calls
**family `vtbl+44`**. PALSKIN / static / lighting `+44` is `00B38840`
=`ret`. ZSPRITE `012A5314+44` = `00BAC0F0` (sort + `00BAA870`).
**Every** +616 bit then drains prim-queue **slot 0** (`00B849F0`)
and optionally slot 1 if `ctx+8 & 2`.

Static C3D reaches slot 0/1 from factory **`vtbl+28` = `00BBBF70`**
(`00B84720`). PALSKIN reaches slots **8 / 9 / 10 / 14** and is
drained on bits **`0x100` / `0x200` / `0x80`**, not first-seen `0x20`.

Vulkan must keep **registration order**. Flattening PALSKIN into
`0x20` is **DISPROVEN**. Concat-without-sky is **DISPROVEN**.

---

## Recovered nodes

### `00B324A0` — type dispatch / insert

**PROVEN** (control) / **PARTIAL** (per-type `vtbl+20`)

- native VA: `00B324A0` (`ret 16`)
- `this` unused for the table; factory is BSS `0x1436E84`
- args: `dest`, `rec`, `flags`, `extra`
- `type = [rec+0]`; `factory = [table+16+type*4]`
- `bl = !((flags >> 5) & 1)` written into handler `+52` bit 0
- cache hit (`dest+4` and `[handler+8]==type`): `handler.vtbl+20(rec, flags)`
  then if `[handler+36] >= 0` → `00B91840`
- miss: release old, `factory.vtbl+4(type)` construct, store `dest+4`,
  `extra` bit 1 into `+52`, then `vtbl+20`
- `flags & 0x40` → `00B4A670([0x1436E7C], handler)` else `00BACDD0(factory, handler, flags)`
- factory 0 → `je 00B325FA`, no `vtbl+20`
- **E8 callers (only two):** `00B23BD7` (`00B23BC0`, engine `+92`) and
  `00B32EBF` (`00B32E90`, hard-coded type **4**)
- **DISPROVEN:** memcpy into display `+16020`

### `00B23BC0` — engine vtbl+92

**PROVEN**

- `012A0F3C+92`
- `00B324A0([0x1436E80], dest, rec, size, 0)` — extra always 0

### `00B32E90` — type-4 helper

**PROVEN**

- local dest `{ vtbl 0x122F598, handler* 0 }`
- `00B324A0(this, dest, rec, 4, 1)`
- only other `E8` of `00B324A0`

### `00BACDD0` — factory → octree

**PROVEN**

```
mov eax, [ecx+4]
mov ecx, [eax]
mov ecx, [ecx]
jmp 00B94D30
```

`ecx` on entry is the factory. `[factory+4]` is a pointer-to-pointer
to the scene object that owns the octree (`MainScene` / `RepMeshScene`).

### `00B94D30` — octree or list insert

**PROVEN**

- `ret 8` — `(rec, flags)`
- `rec+48` cookie; `rec+52` flag byte
- **octree** when `!(+52 & 2) && (+52 & 1) && (flags & 0x4001)==1`
  and `rec.vtbl+68` fills an AABB: `00B94BF0` then `00B93B30`.
  `this+28 |= rec+48`; `this+56++`
- else if `!(+52 & 2)`: `00B8FDF0(rec, &this+32)`; `this+60++`
- else: `rec+4++`; `00B8FDF0(rec, &this+36)`; `this+64++`
- **E8** besides the `00BACDD0` jmp: `00B9ABBB` (same splice shape)

### `00B8FDF0` — list splice

**PROVEN**

- `ret 4` — arg is `head*` (`MainScene+32` / `+36`, or lighting `+64`/`+68`)
- walk `[head]…+64` until `[node+48]==this+48`
- miss: link `this` at `+56/+60/+64/+68`; `[rec]=this`
- hit: insert after that node’s `+56` chain; clear `this+64/+68`
- **E8:** `00B94E2D` / `00B94E4F` plus `00B9187E` / `00B918D4` /
  `00B92421` / `00B9382D` / `00B93976` / `00B93C8F`
- `00B4A670` is `jmp 00B8FDF0` onto lighting `this+64` or `+68`
  (`test [rec+12], 0x400`)

### `00B91840` — cache-hit reinsert

**PROVEN**

- if `!(flags & 0x40)`: `00B8FE60` unlink then `00BACDD0` again
- `flags & 0x40`: no-op (already on the lighting list)

### `00B27D90` — engine vtbl+32 (frame)

**PROVEN** (body) / **UNREAD** (game-display caller)

- `012A0F3C+32`
- `00B23A90` device reset-ish; optional `00B277A0`; **`00B25950`**
- device vtbl+400 / +416 / +260 (stream / texture clear)
- recovered site: frontend `0042E0BB`. Game `00435530` has **no** `E8`
- 0 `E8` callers (vtbl only)

### `00B25950` — ScenePass walker

**PROVEN**

- only `E8`: `00B27E87`
- dirty-list push on `0x13CB508+10248` (same 8-byte log as +616)
- `00B24850` (CCW / first-seen landscape state)
- ctx: `rep movsd` 0x1C dwords from arg; `ctx+120 = manager+184`;
  `ctx+132 = manager+136`; `ctx+140 = manager+248`
- phase 1: components `[+360,+364)` — `vtbl+40` vs `+184`, then `vtbl+4`
- phase 2: layers `[+348,+352)` — **every** `layer.vtbl+4` = `00B2AB80`
- phase 3: components `vtbl+8`
- `00A5C720(this+304)`; `inc [this+136]`; `00A05840` pop dirty

### `00B2AB80` — per-layer submit

**PROVEN**

- skip if `(ctx+120 & layer+12)==0`
- three loops over `layer+16…+20` renderer*:
  1. `vtbl+40` query vs `ctx+120` → `vtbl+20` prepare
  2. same **and** `[renderer+8]` → **`vtbl+16` draw**
  3. same query → `vtbl+24` after
- layer vtbl `012A0F04+4` = this fn. 0 `E8` (vtbl only)

### `00B33010` — MainScene+616 vtbl+16

**PROVEN** (switch + tail) / **PARTIAL** (slot payloads)

- `012A1348+16`. 0 `E8` (vtbl only)
- early-out unless `ctx+120 & 1`
- `lea ebx, [this-616]` = MainScene
- **`00B32AD0(bit)`** state for every bit except the `ctx+120` skip
- then the switch (below)
- **common tail (every taken bit):**
  1. `00B849F0(queue 0x1436E74, slot 0, clear=1)`
  2. if `ctx+8 & 2`: `00B849F0(slot 1, clear=1)`
  3. `00B32230` restore
  4. `00A05840`

| bit | body | status |
|---|---|---|
| `2` | if `[0x1436E60+8]` and `+820`: walk `[+616+208,+212)` → `00B94060` | **PROVEN** shadows |
| `0x20` | **`00B32610`** (MainScene `+628` `vtbl+44`) | **PROVEN** (not `00B93F40`) |
| `0x80` | if `[0x1436E3C+8]`: slots **14** then **13**; else `00B847B0(14)` | **PROVEN** PALSKIN type1 B |
| `0x100` | slots **8** then **10** (or `00B847B0` both) | **PROVEN** PALSKIN type0 / type1 A |
| `0x200` | slot **9** | **PROVEN** PALSKIN Flag1 extra |
| `0x400` | slot **11** | **PARTIAL** family |
| `0x1000` | slot **7** | **PARTIAL** family |
| `0x40000` | slot **12** | **PARTIAL** family |
| `0x8000000` | slot **5** + displacement `00B87680` / `00B869A0` | **PARTIAL** |
| `0x10000000` | slot **6** + `00B871B0` / `00B86BB0` | **PARTIAL** |
| else (`8`, `0x10`, `0x4000`, `0x8000`, …) | walk octree `[+824]` → `00B93F40` / `00B942A0` | **PROVEN** spatial |

### `00B32610` — bit `0x20` family `+44`

**PROVEN**

- only caller: `00B330A4`
- vector MainScene `+628` begin / `+632` end, stride 8
- `call [ent.vtbl+44]`
- PALSKIN `012A7914+44` / static `012A5BB8+44` / lighting `012A2274+44`
  = `00B38840` **`ret`**
- ZSPRITE `012A5314+44` = `00BAC0F0` (depth-sort `+128` then `00BAA870`)

### `00B32AD0` — +616 state setup

**PROVEN** (gates) / **PARTIAL** (slot bodies)

- copies wrapper `0x1436E14+912/+913` onto MainScene `+609/+608`
- bit **`0x1000`**: `00988290` plus a run of wrapper dirty slots
- bits **`4`, `8`, `0x10`, `0x20`, `0x40`, `0x400`, `0x40000`**:
  `00B46890` FOGENABLE on `0x1436E9C`
- `00B32300`; `[this+120] = [+189] ? 0 : -1`

### `00B32230` — +616 cleanup

**PROVEN**

- `[this+120] = -1`
- dirty a wrapper slot at `+10824`
- `00B44D50` lighting; `00B32100`
- restore `0x1436E14+912/+913` via `00987FE0` / `009880E0` / `00988110` / `00988190`

### `00B93F40` / `00B92500` / `00B91760` — spatial drain

**PROVEN**

- `00B93F40(octree, bit)`: if `octree+28 & bit`, walk tree, `00B92500`
  then `00B91760` on lists `+32` and `+36`
- `00B92500`: cell table, frustum `00B308E0`, then for each rec
  with `rec+48 & bit`: `rec.vtbl+80` then **`factory.vtbl+28(rec, bit)`**
  (`[0x1436E84+16+[rec+8]*4]`). Return `2` stamps `rec+54`
- `00B91760`: same `vtbl+28` walk on a linear `+56` / `+64` chain
- static `+28` = `00BBBF70` (**enqueue**, not DIP)
- PALSKIN `+28` = `00BD71B0` (draw entry)

### `00BBBF70` — static factory vtbl+28 (queue)

**PROVEN**

- skip if `[this+8]==0`
- alloc helper vtbl `012A5BB0`; camera-relative centre
- if `[mesh+160]`: slots **5** then **6** (displacement path)
- else `00B84720(queue, slot 0 or 1, helper)`
- returns `2`

This is how type-`0x18` C3Ds reach the slot-0 drain on bit `0x20`.

### `00B849F0` — prim-queue drain

**PROVEN**

- `this` = `0x1436E74`
- slot `lea esi, [this + (3*index+3)*8]`
- if `slot+23==0`: skip
- `slot+20 != 0`: walk `[+0,+4)` 8-byte ents → `[ent+20].vtbl+24`
- else `00B84320` / `00B842D0` → `[rec+20].vtbl+20` (follow `+16`)
- second arg nonzero → `00B84400` **clear** (dtor walk + compact)

### `00B84720` — prim-queue insert

**PROVEN**

- `(slot, helper, renderer, sortKey)`
- 8-byte `{ key, helper* }`; `helper+20 = renderer`

### `00B8FAA0` / `00B8FAD0` — type factory table

**PROVEN**

- ctor `00B8FAA0`: vtbl `012A3D14`; `+16` is **`0x2F` dword slots**
- `00B8FAD0(factory)`: `factory.vtbl+8` fills a type-id vector;
  `table[16+type*4] = factory`
- only recovered `E8` of `00B8FAD0`: lighting `00B48491` (types `0xF`/`0x10`)
- other families register via a **UNREAD** path (inlined / vtbl)

---

## Renderer vtables

Layer-attached draw objects (`00B2AB80` calls `+16` / `+20` / `+24` / `+40`):

| Object | vtbl | +16 draw | +20 prepare | +24 after | +40 query | status |
|---|---|---|---|---|---|---|
| MainScene+616 | `012A1348` | **`00B33010`** | `00B28C60` `ret 4` | `00B28C70` `ret 4` | `00B38CB0` → **3** | **PROVEN** |
| Landscape `0x1436EA8` | `012A2B54` | **`00B6B0B0`** | `00B28C60` | `00B28C70` | `00B6CA10` → **1** | **PROVEN** |
| Sky `0x1436E50` | `012A293C` | **`00B662F0`** | `00B28C60` | `00B28C70` | `00B66DE0` → **1** | **PROVEN** |
| Water `0x1436E54` | `012A3364` | **`00B783F0`** | `00B28C60` | `00B28C70` | `00B7ED70` → **1** | **PROVEN** (first-seen empty) |
| Layer object | `012A0F04` | n/a | n/a | n/a | n/a | `+4` = `00B2AB80` **PROVEN** |
| Engine | `012A0F3C` | n/a | n/a | n/a | n/a | `+32`=`00B27D90` `+92`=`00B23BC0` **PROVEN** |

Base ctor `00B59710` sets `[renderer+8]=1`, so `+16` runs.

Family factories (insert `vtbl+20`, spatial `vtbl+28`, bit-`0x20` `vtbl+44`):

| Family | vtbl | +8 types | +20 pack/draw | +24 | +28 spatial | +44 on `0x20` | status |
|---|---|---|---|---|---|---|---|
| PALSKIN | `012A7914` / `012A78DC` | `00BD27F0` → **`0x9,0xB,0xD`**; sibling `00BD28A0` → **`0x25,0x26`** | `00BD7110` | `00B91340` | `00BD71B0` | `00B38840` `ret` | **PROVEN** |
| Static | `012A5BB8` | `00BBCF30` → **`0x18`** | `00BBC460` | `00B4A460` | **`00BBBF70` queue** | `00B38840` `ret` | **PROVEN** |
| Lighting | `012A2274` | `00B48220` → **`0xF,0x10`** | `00B4A450` **`ret 8`** | `00B4A460` | `00B481E0` | `00B38840` `ret` | **PROVEN** stub pack |
| ZSPRITE | `012A5314` | `00BABF30` → **`0x2B`** | `00BAA4D0` | `00B91340` | `00BABF70` | **`00BAC0F0` draw** | **PROVEN** |
| type `0x7` | `012A55B4` | `00BB1600` → **`0x7`** | `00BAD2E0` | `00B4A460` | `00BAF9E0` | `00B38840` `ret` | **PARTIAL** |
| type `0x29` | `012A1308` | `00BACD10` → **`0x29`** | `00B38860` | `00B91340` | `00BACC50` | `00B38840` `ret` | **PARTIAL** |
| type `0x24` | (slot list `00BA7770`) | **`0x24`** | UNREAD | | | | **PARTIAL** |
| type `0x22` UI | none recovered | — | — | | | | **UNREAD** factory |

`EnablePrimitives` `00B38C90` / `EnableAnimatedMeshes` / `EnableZSprites`
are **`ret 4` name interners**, not draw.

---

## Layer bit → family → renderer → setup → draw → cleanup

Registration `00B26A75`–`00B276A8` (34 layers, vtbl `012A0F04`, bit at
`+4`, renderer vector `+16`). Frame walks **begin → end**. This **is**
submit order. `ScenePasses.Registration` matches that order.

| # | bit | attached renderer | record / family | setup | draw | cleanup | first-seen | status |
|---|---|---|---|---|---|---|---|---|
| 0 | `0x1` | none | — | — | none | — | empty | **PROVEN** None |
| 1 | `0x2` | shadows `0x1436E60` + +616 | shadow casters | `00B32AD0` | `00B94060` if `[+8]` | `00B32230` | off | **PROVEN** Shadows |
| 2 | `0x4` | landscape `0x1436EA8` | patch/cell (not typed) | `00B67480` `00B671A0` | `00BDC060` → `00BF71D0` BG | `00B67510` unbind | **yes** | **PROVEN** terrain BG |
| 3 | `0x8` | +616 | octree recs | `00B32AD0` (fog) | `00B93F40` → factory `+28` | slot0 + `00B32230` | unread | **PARTIAL** |
| 4 | `0x10` | +616 | octree recs | fog | `00B93F40` | slot0 + restore | unread | **PARTIAL** |
| 5 | `0x40` | landscape | 72-byte cells | `00B68DA0` `00B67480` `00B677D0` `0098B5E0(2)` | `00BDC2D0` → `00BF4570` FG | `00B67510` | **yes** | **PROVEN** terrain FG |
| 6 | `0x20` | +616 | static `0x18` via slot 0; ZSPRITE `0x2B` via `+44` | fog `00B46890` | `00B32610` then **`00B849F0(0)`** | `00B32230` | **yes** static | **PROVEN** static / **DISPROVEN** PALSKIN-here |
| 7 | `0x100` | +616 | PALSKIN type0 slot 8; type1 slot 10 | `00B32AD0` | `00B849F0(8)` `00B849F0(10)` | slot0 + restore | first PALSKIN | **PROVEN** |
| 8 | `0x400` | +616 | slot 11 | | `00B849F0(11)` | | unread | **PARTIAL** |
| 9 | `0x1000` | +616 | slot 7 | special `00988290` | `00B849F0(7)` | | unread | **PARTIAL** |
| 10 | `0x2000` | sky `0x1436E50` | sky mesh | prepare stub | `00B662F0` else-path `00B66190` | stub | **yes** | **PROVEN** sky |
| 11–12 | `0x4000` `0x8000` | +616 | octree default | | `00B93F40` | | unread | **PARTIAL** |
| 13 | `0x20000` | water `0x1436E54` | water lists | stub | `00B783F0` empty-out | stub | empty | **PROVEN** omit |
| … | `0x10000` | displacement `0x1436E38` | | | | | unread | **UNREAD** |
| … | `0x40000` | +616 | slot 12 | fog | `00B849F0(12)` | | unread | **PARTIAL** |
| … | `0x400000` | sky again | | | `00B64550` | | not first-seen | **PROVEN** skip |
| … | `0x2000000` | landscape again | | | profiler / unbind only | | None | **PROVEN** no-op |
| 25 | `0x80` | +616 | PALSKIN type1 slot 14 | | `00B849F0(14)` | | after sky | **PROVEN** |
| 26 | `0x200` | +616 | PALSKIN Flag1 slot 9 | | `00B849F0(9)` | | after sky | **PROVEN** |
| … | `0x4000000` | `0x1436E3C` | | | | | unread | **UNREAD** |
| … | `0x1000000` | colour filter `0x1436E40` | | | | | unread | **UNREAD** |
| … | `0x8000000` | +616 | slot 5 + displacement | | `00B849F0(5)` | | unread | **PARTIAL** |
| … | `0x10000000` | +616 | slot 6 | | `00B849F0(6)` | | unread | **PARTIAL** |
| … | `0x20000000` | weather+glow+blur+disp | | | | | unread | **UNREAD** |
| … | `0x40000000` | shader manager | | | | | unread | **UNREAD** |
| … | `0x80000000` | `0x1436E7C` | lighting lists | | | | unread | **UNREAD** |

2D HUD / fade is **after** this walk: display `009D9C80` / `009DA9F0`
inside `00435530` (game) or `0042DF9E` (frontend). Not a layer bit.

---

## Identify each class

### Terrain

**PROVEN.** Not a MainScene type. Not `00B324A0`.

- store: opened `CEngineLandscapePatch` / `CLandscapeBackgroundPatch`
- bits `0x4` (BG `00BDC060` / `00BF71D0`) then `0x40` (FG `00BDC2D0` /
  `00BF4570`)
- renderer `012A2B54+16` = `00B6B0B0`
- setup `00B67480` (TOD `00B46C80` + FOGENABLE `00B46890` + identity
  `009881F0`); FG also `00B68DA0` / `0098B5E0(2)`
- cleanup `00B67510` unbind stages 0/1/2
- alphablend **off**; CCW; VS fog
- see investigation C

### Static C3D

**PROVEN** queue / drain split. Packer **UNREAD**.

- type **`0x18`** (`00BBCF30`)
- factory vtbl `012A5BB8`
- insert: UNREAD thing packer → `00B324A0` → `00BBC460` (vtbl+20,
  subset walk) → `00B94D30`
- visibility: `00B93F40` → `00BBBF70` → `00B84720` slot **0 or 1**
- draw: bit **`0x20`** tail `00B849F0(0)` → helper `+20` `vtbl+20`
  (`00BBC460` / `00BBC130` / `00BB2540` static-lit)
- VS `VSHADER_STATIC_DIRLIGHT_FOG`; PS `PSHADER_TEXTURE_DIFFUSE`;
  blend off; inherit CCW; identity W
- host `ScenePasses` first-seen `0x20` = this family. **Correct for
  static. Incorrect if it also means PALSKIN.**

### PALSKIN / creatures

**PROVEN** family + slots. Packer **UNREAD**. Host flatten **DISPROVEN**.

- types **`0x9, 0xB, 0xD`** (`00BD27F0`); extras `0x25, 0x26`
- ctor `00BD1EF0` `"EnableAnimatedMeshes"`; vtbl `012A7914` / `012A78DC`
- helper `00BCE740` → `00B84720`:
  - type1 → slots **10** then **14**
  - type0 → slot **8**; Flag1 → slot **9**
- drain:
  - `0x100` → 8 + 10 (**before** sky `0x2000`)
  - `0x80` → 14 (**after** sky, after water)
  - `0x200` → 9 (Flag1 extra, after sky)
- draw: `00BD7110` / `00BD71B0` / `00BD3070`; VS
  `VSHADER_PALSKIN_DIRLIGHT_FOG`; `c38` dest; SRCALPHA/INVSRCALPHA
- **DISPROVEN:** `TrianglesForPose` into the `0x20` soup
  (investigations E, G)

### Lighting

**PROVEN** as records + setup; **DISPROVEN** as a DIP family.

- types **`0xF, 0x10`** (`00B48220`); only recovered `00B8FAD0` writer
- vtbl+20 `00B4A450` = **`ret 8`**
- insert `flags&0x40` → `00B4A670` → `00B8FDF0` on `0x1436E7C+64/+68`
- consume: `00B67480` / `00B32AD0` → `00B46C80` TOD + `00B46890`
  FOGENABLE. VS `c19/c20/c35/c3`. No first-seen `SetLight`

### Effects / decals

**UNREAD** as a MainScene drain.

- string sites `00AF8150` / `00B1FF50` intern `"Decal Renderer"` and
  **return** — not enqueue
- RTTI `CDecal@NParticleEngine`, `CPSCDecalRenderer`,
  `CEnginePrimitiveManagerParticleDecalGroup`
- shader bank `SHADERS_DECAL_GROUP` (slot 22)
- first-seen New Game does not submit them

### Particles

**PROVEN** not C3D. Draw **UNREAD**.

- `PARTICLE_EMITTER_PLACEABLE` rejected (`FirstSeenInstancesAsC3d`)
- create `006E0880` looks up `PARTICLE_EMITTER_NORMAL`, not a mesh
- RTTI: `CEnginePrimitiveManagerParticleGroup` /
  `…SpriteGroup` / `…SpriteTrailGroup` / `…2DParticleGroup`
- shader banks 16/21/24 (`POINT_SPRITE1` / `SPRITE_GROUP` /
  `PARTICLE_SPRITE_TRAIL`)
- `Load Particles` `004174F1` is boot, not a layer bit
- **do not** invent a `0x20` particle soup

### Alpha

**PROVEN** as state, not as its own layer bit.

- landscape / first-seen static-lit: blend **off**
- PALSKIN: `00BD3867/00BD38D4` SRCALPHA(5)/INVSRCALPHA(6), enable 1
- Flag1 does **not** pick blend (`FirstSeenFlag1SelectsAlphaBlend=false`);
  it adds **slot 9** / bit `0x200`
- RTTI `CEngineStateBlockAlpha*` exist; apply body **UNREAD**
- `ALPHATESTENABLE` first-seen write **UNREAD**

### UI / 2D / frontend

**PROVEN** as a **different queue**.

| path | insert | drain | type |
|---|---|---|---|
| HUD / debug text | `009DD8F0` → `009DB700` | `009DA9F0` DIP vtbl+332 | 60-byte `+16020` |
| frontend / player GUI quad | `0041BEB0` type **`0x22`** → vtbl+92 | handler `vtbl+20` then `00B25950` | MainScene dest `+0x15C` |
| fade overlay | `00639E40` / `00434870` | `005BCAFE` | not `+16020` |

Type `0x22` factory slot is **UNREAD** (no `00B8FAD0` writer).
First-seen `Frontend2dDipIssued=false`. **DISPROVEN** as world /
terrain / C3D / PALSKIN insert.

ZSPRITE type `0x2B` is a **3D** family (`VSHADER_ZSPRITE`), not HUD.

---

## MainScene ctor families (`00B33B50`)

**PROVEN** construction order (store on MainScene):

| store | size | ctor | identity |
|---|---|---|---|
| `+616` | — | `00B59710` + vtbl `012A1348` | layer renderer |
| `+824` | octree* | `00B94A80` `"MainScene"` then `"RepMeshScene"` | spatial lists |
| `+664` | `0xC4` | `00BABB50` | ZSPRITE `012A5314` type `0x2B` |
| `+640` | `0x318` | `00BA1A70` | **PARTIAL** |
| `+648` | `0x690` | `00BD1EF0` | PALSKIN |
| `+656` | `0x94` | `00B99D90` | repeated-mesh / stipple (`PSHADER_REPEATED_MESH_*`) |
| `+672` | 12 | `00BACF70` | vtbl `012A55B4` type `0x7` |
| `+680…+816` | various | `00BCE070` … `00BB6A30` | **PARTIAL** (includes static `00BBBF40` at `+792`) |

Self vtbl write `012A1380` sits on a string (`EnablePrimitives` ASCII
in the next dwords) — **not** a method table past slot 0.

---

## Vulkan order (semantically meaningful)

Walk `ScenePasses.Registration` and emit only recovered draws.
**Do not collapse bits.** Rank already encodes this:

```
0x4  land BG          (opaque, blend off)
0x40 land FG          (opaque, blend off)
0x20 static C3D       (opaque) + ZSPRITE (sorted)
0x100 PALSKIN 8+10    (SRCALPHA)          ← before sky
0x2000 sky else
0x20000 water         (first-seen empty — omit)
0x80 PALSKIN 14       (SRCALPHA)          ← after sky
0x200 PALSKIN 9       (Flag1 extra)
then display 2D       (009DA9F0 / fade / AVI)
```

Unread bits stay unread. Do not fold `0x80`/`0x100` into `0x20`.
Do not sort by texture across bits. Do not Concat landscape + C3D +
PALSKIN into one VB and call that native.

`MeshBatches.Concat(land, props)` happens to be `0x4 < 0x40 < 0x20`
only if land is those two bits and props are only `0x20`. That is
**PARTIAL** / accidentally OK for static, **DISPROVEN** for PALSKIN
and sky.

---

## Host DIVERGE (do not treat as native)

| Host | Native | status |
|---|---|---|
| `SubmitCurrentWorld` one `TexturedMesh` | per-cell DIP + per-record drain | **DISPROVEN** |
| PALSKIN `TrianglesForPose` on `0x20` | slots 8/10/14/9 on `0x100`/`0x80`/`0x200` | **DISPROVEN** |
| `009DA9F0` as layer walker | `00B25950` / `00B2AB80` | **DISPROVEN** |
| `00B25950` inside `00435530` | only `00B27D90`; game site **UNREAD** | **DISPROVEN** pairing |
| `ScenePasses.FirstSeenLayers` “`0x20` static + PALSKIN” | `0x20` static + ZSPRITE + slot 0 | **DISPROVEN** PALSKIN half |
| A-dx9-submit “`0x20` = `00B93F40`” | `0x20` = `00B32610` + slot 0 | **DISPROVEN** |

---

## UNREAD leaves

| Item | Why it matters |
|---|---|
| Game caller of `012A0F3C+32` | without it, `00435530` never walks layers |
| Thing/world packer into `00B324A0` types `0x9`/`0x18`/… | C3D / creature insert. **Not** `0041BEB0` |
| Type `0x22` factory at `[table+16+0x22*4]` | frontend handler `vtbl+20` |
| Other `00B8FAD0` registrars | who owns `0x9`/`0x18`/`0x22`/`0x2B` |
| Slot 7 / 11 / 12 families | bits `0x1000` / `0x400` / `0x40000` |
| Particle / decal enqueue → which slot / bit | effects |
| `00BF4570` DIP vtbl | terrain prim / base-vertex |
| `00B783F0` nonempty water | bit `0x20000` |
| `00B64550` sky `0x400000` | not first-seen |

---

## Evidence

Live this session: `fn` / `calls` / `vtbl` / `--exact` on
`00B324A0` `00B94D30` `00B8FDF0` `00B25950` `00B2AB80` `00B33010`
`00B32610` `00BACDD0` `00B4A670` `00B32AD0` `00B32230` `00B93F40`
`00B92500` `00B91760` `00B849F0` `00B84720` `00B84400` `00BBBF70`
`00B27D90` `00B23BC0` `00B32E90` `00B8FAD0` `00BD27F0` `00BBCF30`
`00B48220` `00BABF30` `00BB1600` `00BACD10` and vtbls
`012A1348` `012A2B54` `012A293C` `012A3364` `012A7914` `012A78DC`
`012A5BB8` `012A2274` `012A5314` `012A55B4` `012A1308` `012A0F04`
`012A0F3C`.

Host not edited: `ScenePass.cs`, `EngineLifecycle.cs`.
