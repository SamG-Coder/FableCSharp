# Present skip from the complete `.text` listing

Authority: `tools/Fable.ExeIndex/out/01-sections/text-map/`
(`listing-00500000.txt` / `listing-00540000.txt` /
`listing-00580000.txt`). No serial `fn` dump.

Status: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN**.

## System

`00595222` walks every `[ui+84]` `node+20` and
calls `vtbl+8`. Null value is the only skip.
`[ui+32]` is not loaded. After `0x126`, key `0`
is Main Menu, `0x14` Press Start, `0x17` New
Profile. Host `ResidentSlotTrees` **MATCH**es
that walk.

`00530260` (`vtbl+8` on type 5/10/12/16/18)
walks every `+176` then `+188` child:

```
parent = child.vtbl+208
if parent != this && !child.vtbl+400: skip
if child.vtbl+420: skip
else child.vtbl+8
```

| Slot | VA | Listing | Class |
| --- | --- | --- | --- |
| `vtbl+400` | `0052F180` `[+300]>>7` | `listing-00500000` | **PROVEN** |
| `vtbl+420` | `0052F1D0` `[+302]&1` | same | **PROVEN** |
| `+300` bit 7 writer | `00533258` persist `def+504` | `005331A0` | **PROVEN** |
| `+302` bit 0 writer | `00533288` persist `def+392` | `005331A0` | **PROVEN** |
| CRC of `+392` / `+504` | — | — | **UNREAD** |
| First-seen forest tile `+392` | 0 (tiles draw) | persist-flag-names | **PROVEN** |
| `parent!=this` first-seen | **DISPROVEN** (`00531EC0` writes `+200`) | | |
| `SelectState(6)` writes `+302` | **DISPROVEN** | `listing-00500000` `0052CF40` | |
| Type 16 present index | `+348` not `+332` | `listing-00540000` `00549B20` | **PROVEN** |
| Type 38 SelectsChild | — | ON and OFF both persist | **DISPROVEN** |
| Type-6 leftover204 dest width | — | — | **DISPROVEN** |

`vtbl+188` `0041C5A0` (`listing-00400000`):
`[+320]=duration` then `vtbl+192`. Select
forwards down `+176` when parent==this.

First-seen type 18/16 present persist child
**0**. `00530260` does not exclusive-walk.
Host `SelectsChild` + `Visible=false` on
`k!=0` is the first-seen present set
(forest_1 / ARROWS / NORMAL). Attach
`SelectState(5)` is **not** type-18 child 5.

Leftover-slot hide after `00596763`
`vtbl+192(6)` is **not** `Visible` / clip.
Colour `+144..+151` first-seen is ctor zero
then `005339B0` / `0052C7E0` style bit
`0x10` → `0xFF`. Style-6 alpha as the
old-slot hide is **UNREAD**. Do not invent
draw-current-only.

## Host

`FrontendWidgetType.BorrowedVisibleFn` /
`ClipBitFn` / `ForwardSelectFn` /
`TextSliderIndexOffset`.
`ResidentSlotTrees` keeps keys `0` /
`0x14` / `0x17`.
`SelectFrontendState` forwards `+332`
via `0041C5A0` to persist children.
Not `ActiveChild`. Not `Visible=false`.
