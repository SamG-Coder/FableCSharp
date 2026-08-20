# Frontend dest layout (`005339B0` / `00531EC0` / `0041AFA0`)

Native dest used by `0041AFA0` is `+248/+264`. This file is the recovered
math. Isolated calculator: `src/Fable.Game/FrontendLayout.cs`.

## Status

| Claim | Status | Evidence |
|---|---|---|
| Authored coords are **640×480 pixels**, not 0–1 | **PROVEN** | `[0x1375CD4]=640` `[0x1375CD8]=480` |
| Y increases down; no Y flip | **PROVEN** | `0041AFA0` `y1 = y0 + h`; persist TEXT `Y=240` below TITLE `Y=30` |
| `+36` is a map of layout states | **PROVEN** | `005339B0` `lea ebx,[esi+36]` then `0042D5B1` |
| Layout record `+8/+12` = persist PositionX/Y → widget `+52/+56` | **PROVEN** | `0052F440` copies def record `+72/+76`; `005339B0` writes `+52/+56` |
| Layout record `+16/+20` = persist ScaleX/Y → widget `+92/+96` (default 1) | **PROVEN** | `0052F440`; default insert `0052E5D0` `+16/+20=1.0` |
| `+272/+276=1.0` when `+280==0` | **PROVEN** | `005339B0` |
| `+76/+80/+256/+260=0` when `+280==0` | **PROVEN** | `005339B0` |
| `+116/+120=1.0` always in `005339B0` | **PROVEN** | writes `0x3F800000` |
| `+144..+147=0xFF` | **PROVEN** | `005339B0` |
| `+200` is parent pointer | **PROVEN** | ctor 0; `00531EC0` `cmp esi,[ecx+200]`; `0052F5C0` skips global scale when nonzero |
| Children inherit parent dest scale | **PROVEN** | `00531EC0` `vtbl+460` `0052F250` writes child `+272` from parent `+264` |
| Children inherit parent dest origin | **PROVEN** | `vtbl+456` `0052F230` writes child `+256` from parent pos combine |
| `vtbl+72` `0052E8D0` writes `+76/+80` | **PROVEN** | parent local offset |
| `vtbl+80` `0052E910` writes `+116/+120` | **PROVEN** | parent inner scale |
| First-seen UI ctor sets `[0x13B8768]=1` and copies display into `[0x13B876C/70]` | **PROVEN** | `0041E3F6` `mov cl,1` → `004299A8` |
| `0052E580` remap `x/640*vpW`, `y/480*vpH` when flag set | **PROVEN** | `0052E580` |
| Remap applies to dest **only if** persist flags set `+302` bits 6/7 | **PROVEN** | `vtbl+464` `0052F3A0` bit 6; `vtbl+468` `0052F3B0` bit 7; `005331A0` from def `+520/+521` |
| Frontend global UI scale is **1,1** | **PROVEN** | `0041CF47` `[0x13B86A0]==0` (game singleton still 0) |
| `0041AFA0` size: `+360!=0 ? +360 : +204`, same for H; then `* +264` from `+248` | **PROVEN** | `0041AFA0` |
| Persist Width/Height → `+360/+364` via ftol | **PROVEN** | `0041AC20` `fld [def+92]/[def+88]` `00BFEA70` |
| Leftover `+204/+208` is texture w/h when `+376!=0` | **PROVEN** | `0041AC20` vtbl+84/+88 |
| First-seen type-0: `+376=0` so leftover never written; ctor zeros `+248/+264`; Width=0 → dest **0,0,0,0** | **PROVEN** | `0041B800` `+376=0`; `005334A0` zeros origin/scale; PRESS_START persist Width=0 |
| Center is `+302` bit 1 (`vtbl+424` `0052F1E0`) from def `+188` | **PROVEN** | `005331A0` `or [+302],2`; `0041AFA0` origin ± size/2 using `[0x122F59C]=0.5` |
| PRESS_START persist positions (640-space) | **PROVEN** | root 0,0; `UI_TITLE` 70,30; `UI_TITLE_02` 256,0; `UI_PRESS_START_TEXT` 320,240 |
| Root dest Width=0 stays 0,0,0,0 | **PROVEN** | first-seen type-0 path |
| Child dest nonempty after layout | **PROVEN** | persist PositionX/Y + leftover size; enqueue `00BAD8A0` `009DB700` |
| Display 1024×768, viewport `009BEF80` full backbuffer | **PROVEN** | prior; UI ctor copies that into `0x13B876C/70` |
| Type-6 H-align (left / centre / right) | **PARTIAL** | `0054EF00` `vtbl+600` `0054FFF0`: bit4=centre, bit5=right. `005331A0` does **not** write those bits. First-seen `+302` bits 4/5 stay 0 → left |
| Persist names for def `+188/+191/+520/+521` | **PROVEN** | `PositionIsCenter`, `Independant`, `UseRelativeZoom`, `UseRelativePosition`; original names hash to the retail CRCs |
| Who writes leftover `+204` for type-6 text measure | **PARTIAL** | `0054EF00` multiplies `+204` by scale; writer not this pass |
| Right-align for type-0 sprites | **DISPROVEN** | `0041AFA0` only centre via `vtbl+424` |
| Host `parent.Dest + Position` / texture-or-font size / PlayAVI half-centre | **DISPROVEN** as native dest | see below |

## Field map

| Off | Written by | Meaning |
|---|---|---|
| `+36` | ctor map | layout-state map; key 0 is current |
| `+52/+56` | `005339B0` from `+36+8` | current authored pos (persist PositionX/Y) |
| `+60/+64` | same | target pos (lerp) |
| `+68/+72` | copy of `+52/+56` | lerp start |
| `+76/+80` | parent `vtbl+72`; 0 if `+280==0` | parent local offset |
| `+84/+88` | `0052FFD0` | local pos after `+116` and `+76` |
| `+92/+96` | `005339B0` from `+36+16` | persist scale (default 1) |
| `+116/+120` | parent `vtbl+80`; init 1 | parent inner scale |
| `+124/+128` | `0052F5C0` | inner scale `+116 * +92` (type-6 fallback) |
| `+200` | parent attach | parent widget pointer |
| `+204/+208` | `0041AC20` | leftover dest size (texture) |
| `+248/+252` | `0052FFD0` | dest origin used by `0041AFA0` |
| `+256/+260` | parent `vtbl+456`; 0 if `+280==0` | parent dest origin |
| `+264/+268` | `0052F5C0` | dest scale used by `0041AFA0` |
| `+272/+276` | `005339B0` (=1 if `+280==0`) then parent `vtbl+460` | inherited dest scale |
| `+280` | ctor 0 | skip inherit-scale init when nonzero |
| `+300` | `005331A0` from def `+60` | bit 6 (`def+191`) = absolute / skip parent combine (`vtbl+408/+412`) |
| `+302` | `005331A0` | bit 0 `def+392`; bit 1 centre `def+188`; bit 6 remap size `def+520`; bit 7 remap origin `def+521` |
| `+360/+364` | `0041AC20` | persist Width/Height as int |

## Exact formulas

First-seen frontend viewport (`0041E3F6` / `004299A8`):

```
[0x13B8768] = 1
[0x13B876C] = display W   // 1024
[0x13B8770] = display H   // 768
[0x13B86A0] = 0            // game not constructed
```

`0052E580` (only when `[0x13B8768]`):

```
x' = x / 640 * [0x13B876C]
y' = y / 480 * [0x13B8770]
```

`0041CF47` global scale:

```
if [0x13B86A0] == 0: (1, 1)
else:
  w = flag ? vpW : 640
  h = flag ? vpH : 480
  (w < 1024 || h < 768) ? (w/1024, h/768) : (1, 1)
```

`005339B0` if `+280==0`:

```
+272 = +276 = 1
+76 = +80 = +256 = +260 = 0
+52 = layout0.pos
+92 = layout0.scale   // default 1
```

`00531EC0` onto each child (before child tick):

```
child+116 = parent+124
child+272 = parent+264
child+256 = parent dest origin   // same combine as parent +248
```

`0052F5C0` dest scale (`+264`):

```
if !absolute:
  +264 = (remapSize ? 0052E580(+92) : +92) * +272
else:
  +264 = remapSize ? 0052E580(+92) : +92
if +200 == 0 || absolute:
  +264 *= globalScale
```

`0052FFD0` dest origin (`+248`):

```
+248 = remapOrigin ? 0052E580(+52) : +52
if !absolute:
  +248 = +248 * +272 + +256
```

`0041AFA0` submit dest (Y-down pixels):

```
w = (+360 != 0) ? (float)+360 : +204
h = (+364 != 0) ? (float)+364 : +208
w *= +264
h *= +268
if center (+302 bit 1):
  dest = (ox - w/2, oy - h/2, ox + w/2, oy + h/2)
else:
  dest = (ox, oy, ox + w, oy + h)
fistp/fild snap
```

Collapsed (first-seen flags 0, persist scale 1, `+280==0`):

```
root:   origin = persistPos; scale = 1; size = persistW!=0 ? persistW : leftover
child:  origin = persistPos * parent.scale + parent.origin
        scale  = persistScale * parent.scale
        dest   = (origin, origin + leftover*scale)   // Width=0
```

640/480→viewport is **not** in that first-seen collapse unless persist `def+520/+521` set `+302` bits 6/7.

## What current C# is wrong

`EngineLifecycle.LayoutFrontendWidgets` / `FrontendWidgetDest` (do not edit):

1. **`dest = parent.Dest + PositionX/Y`** — host guess. Native is `persistPos * parentDestScale + parentDestOrigin` (`+248`, not dest.X0 if centred).
2. **Size = persist Width else texture else font measure** — leftover `+204` is texture from `0041AC20` only when `+376!=0`. Font measure is not `0041AFA0`. Persist Width goes to `+360` and wins only when nonzero.
3. **`FrontendWidgetDest` leftover + scale + optional centre** — leftover/scale/centre shape matches `0041AFA0`, but centre is `+302` bit 1, not PlayAVI. `0.5` is `[0x122F59C]`, not `PlayAviLetterboxHalf`.
4. **Root dest 0,0,0,0** is correct for Width=0, but the host still invents child size from texture/font instead of leftover `+204`.
5. **No parent dest-scale inherit** (`+272 = parent+264`).
6. **No gated 640/480 remap.** Applying `pos * 1024/640` to every widget is invented unless that widget’s persist sets `+302` bits 6/7. First-seen frontend **does** enable `[0x13B8768]`; the per-widget bits still gate the divide.
7. **`UI_TITLE_02` persist X=256** is relative to `UI_TITLE` (70,30), not the screen.

## First-seen Press Start numbers (persist)

| Widget | Type | Pos | W×H |
|---|---|---|---|
| `UI_FRONTEND_PRESS_START_MENU` | 10 | 0,0 | 0×0 |
| `UI_TITLE` | 5 | 70,30 | 0×0 |
| `UI_TITLE_01` | 0 | 0,0 | 0×0 |
| `UI_TITLE_02` | 0 | 256,0 | 0×0 |
| `UI_PRESS_START_TEXT` | 6 | 320,240 | 0×0 |
| `UI_FRONTEND_BG_FORREST_1_1` | 0 | 0,0 | 0×0 |
