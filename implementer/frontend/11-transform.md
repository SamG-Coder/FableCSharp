# 11 — persist fields → screen dest (`0041AFA0`)

Authority: `Fable.exe` + `frontend.bin` + `frontend.big` info `+6/+8`.
No screenshots. Isolated calculator: `FrontendLayout.cs`.

## Required dumps

| VA | Role | Result |
|---|---|---|
| `0052F3A0` | `vtbl+464` remap size | `mov al,[ecx+302]; shr 6; and 1` |
| `0052F3B0` | `vtbl+468` remap origin | `movzx eax,[ecx+302]; shr 7` |
| `0052F1E0` | `vtbl+424` centre | `mov al,[ecx+302]; shr 1; and 1` |
| `0041AFA0` | submit dest | `w = +360 ? +360 : +204`; `* +264` from `+248`; centre ± `0.5` at `0x122F59C` |
| `0041AC20` | leftover | `+360/+364` from def `+92/+88` ftol; leftover `+204/+208` only if `+376 != 0` via bank `vtbl+84/+88` |
| `005331A0` | flag copy | `+302` bit1 `def+188`; bit6 `def+520`; bit7 `def+521`; `+300` bit6 `def+191` |

## Persist CRCs

| CRC | Name | Offset / role |
|---|---|---|
| `0xE78E700E` | **`ZoomX`** | first style scale → widget `+92`. Was `UnknownE78E`. Value 1.0 on Press Start |
| `0x90894098` | **`ZoomY`** | first style scale → widget `+96` |
| `0x38E36902` | `GraphicIndex` | style `+60` → widget `+376`. Leftover gate |
| — | `Centre` / `Center` / `Absolute` / `ScaleToScreen` / `ScalePosition` | **hits=0** in `frontend.bin` UI |

`def+188 / +191 / +392 / +504 / +520 / +521` names remain **UNREAD**. Sequential prefix after `ZoomY` is colours then `0x56A59976` nested. After `Layer`/`Angle` every Press Start widget shares the same 648-byte tail (except `UI_MOUSE_POINTER` Layer=`-10`). That tail is not a per-widget remap bit. Ctor/unread default of those bytes is **0**.

## Native dest (first-seen, flags 0, Zoom=1)

```
leftover = (GraphicIndex != 0) ? frame(+6,+8) : 0
origin   = persistPos * parent.scale + parent.origin
scale    = persistZoom * parent.scale
dest     = (origin, origin + leftover * scale)   // Width=0
```

`0052E580` 640→1024 is live (`[0x13B8768]=1`) but **gated** by unread `+520/+521`. First-seen bits stay 0.

## Dest table (Press Start, 1024×768)

| Widget | persist XYWH | leftover | C A o s | dest X0 Y0 X1 Y1 |
|---|---|---|---|---|
| `UI_FRONTEND_PRESS_START_MENU` | 0,0 0×0 | 0 | 0000 | 0,0,0,0 |
| `UI_BLENDING_BACKGROUNDS_FORREST` | 0,0 0×0 | 0 | 0000 | 0,0,0,0 |
| `UI_FRONTEND_BG_FORREST_SUNBEAM_1_1` | 0,0 0×0 | 256×256 | 0000 | 0,0,256,256 |
| `UI_FRONTEND_BG_FORREST_1_1` | 0,0 0×0 | 256×256 | 0000 | 0,0,256,256 |
| `UI_FRONTEND_BG_FORREST_1_2` | 256,0 0×0 | 256×256 | 0000 | 256,0,512,256 |
| `UI_FRONTEND_BG_FORREST_1_3` | 512,0 0×0 | 128×256 | 0000 | 512,0,640,256 |
| `UI_TITLE` | 70,30 0×0 | 0 | 0000 | 70,30,70,30 |
| `UI_TITLE_01` | 0,0 0×0 | 256×128 | 0000 | 70,30,326,158 |
| `UI_TITLE_02` | 256,0 0×0 | 256×128 | 0000 | 326,30,582,158 |
| `UI_PRESS_START_TEXT` | 320,240 0×0 | 0 | 0000 | 320,240,320,240 |
| `UI_LEGAL_TEXT` | 320,340 0×0 | 0 | 0000 | 320,340,320,340 |
| `UI_MOUSE_POINTER` | 0,0 0×0 | 32×32 | 0000 | 0,0,32,32 |

Frame `+6/+8` equals `TextureFile.Width/Height` on these sprites.

## FIRST DIVERGENCE

**`UI_PRESS_START_TEXT`**. Walk order after root / forest tiles / title sprites (those matched). Type 6, `GraphicIndex=0`, persist W/H=0.

| | leftover | dest |
|---|---|---|
| Native `0041AC20` / `0041AFA0` | 0 (`+376==0`, never written) | **320,240,320,240** |
| Old `LayoutFrontendWidgets` | font `Measure` of `TEXT_GUI_MENU_PRESS_BUTTON` | 320,240,320+w,240+h |

Earlier sprites already used GraphicIndex leftover and stayed in 640-space. That is native while `+520/+521` CRC is unread (default 0). The old host also invented font leftover; that is the first **numerical** dest miss.

## Fix (generic)

- Parse `ZoomX`/`ZoomY` into `FrontendUiDef`. Pass as `PersistScaleX/Y`.
- Pass `Center` / `Absolute` / `ScaleOriginToViewport` / `ScaleSizeToViewport` from persist (false while CRC unread).
- Leftover only when `GraphicIndex != 0`, from frame `+6/+8`.
- No parent dest+Position host add. No font leftover in `0041AFA0`.

## Files

- `src/Fable.Formats/Defs/FrontendUiDef.cs`
- `src/Fable.Formats/Textures/TextureFile.cs` (`FrameWidth`/`FrameHeight`)
- `src/Fable.Game/FrontendLayout.cs` (already the native math)
- `src/Fable.Game/FrontendWidgetFactory.cs`
- `src/Fable.Game/IEngineHost.cs` (`FrontendWidget` flags)
- `src/Fable.Game/EngineLifecycle.cs` (`LayoutFrontendWidgets`)
- `tests/Fable.Formats.Tests/FrontendLayoutTests.cs`
- `tests/Fable.Formats.Tests/FrontendUiDefTests.cs`
- `tests/Fable.Formats.Tests/EngineLifecycleTests.cs`
