# 03 — frontend.big sprite bind + type 0x22 record

Isolated bind and draw-record construction. Authority is `Fable.exe` + `frontend.bin` persist + `frontend.big` (`GBANK_FRONT_END_PC`). No pixel-derived names. No widget-name map.

## Proven bind

frontend.bin `UI` persist stores an i32 after CRC **`0x38E36902`**. That CRC is `FableCrc("GraphicIndex")`. The string is **not** in names.bin and is **not** a dword immediate in the PE (`imm 0x38E36902` = 0). Binding authority is the CRC + the integer, which equals `BankEntry.Id` in `graphics/pc/frontend.big` / `GBANK_FRONT_END_PC`.

| Widget | persist i32 | bank id | name |
| --- | --- | --- | --- |
| `UI_TITLE_01` | 3 | 3 | `FRONTEND_TITLE_01_SPRITE` |
| `UI_TITLE_02` | 4 | 4 | `FRONTEND_TITLE_02_SPRITE` |
| `UI_FRONTEND_BG_FORREST_1_1` | 206 | 206 | `FORREST_1_1` |
| `UI_MOUSE_POINTER` | 362 | 362 | `MOUSE_POINTER_SPRITE_FE` |
| `UI_TITLE` / `UI_PRESS_START_TEXT` / root | 0 | — | no graphic |

Forest tiles under PRESS START (`UI_SWAPPING_FORREST` / `UI_SWAPPING_FORREST_SUNBEAM`) all store the matching `FORREST_*` / `FORREST_SUNBEAM_*` id. Id 0 is “no texture”, not a lookup miss.

**DISPROVEN:** `UI_TITLE_01` → `FRONTEND_TITLE_01_SPRITE` as a hardcoded name map. **DISPROVEN:** strip `UI_FRONTEND_BG_` from the widget name. Those strings happen to look related; persist does not concatenate a prefix.

## Texture info (34 bytes)

`TextureFile.ReadHeader` fields native actually uses:

| info off | field | TITLE_01 | FORREST_1_1 |
| --- | --- | --- | --- |
| +0 | Width | 256 | 256 |
| +2 | Height | 128 | 256 |
| +6 | FrameWidth | 256 | 256 |
| +8 | FrameHeight | 128 | 256 |
| +12 | FormatCode | 1 (Rgba8) | 31 (DXT1) |

`+4` is 0. `+10` is 1 on these files (unread: not proven as UV origin or flip). Bytes after +12 are unread.

`00BAD8A0` dest adjust reads **`[tex+12]+6 / +8`** (frame w/h), half or full, **only if** rec+56 is set. Both `0041BEB0` and `0041BF60` write rec+56 = 0, so first-seen `0041AFA0` does **not** size dest from the file header.

`0041AC20` dest leftover `+204/+208` comes from the font-list path (`vtbl+432`), not `TextureFile.Width/Height`. Persist Width/Height on title/forest tiles are 0. Filling missing dest from the texture is **not** proven on this path.

## Type 0x22 record (`0041BEB0` / `0041BF60`)

`0041AFA0` dest `this+0x15C` size `0xC0`, `call [edx+92]` = `012A0F3C+92` = `00B23BC0` → `00B324A0`.

| rec off | `0041BEB0` (+380==0) | `0041BF60` (+380 set) |
| --- | --- | --- |
| +0 | `0x22` | `0x22` |
| +12..+24 | dest rect | dest rect |
| +32 / +36 | font/index args | **0** |
| +48 | blend (widget+372, ctor **2**) | blend |
| +52..+55 | colour bytes (swizzled pushes) | colour |
| +56 | **0** | **0** |
| +64 | **0** | texture dword |
| +68..+80 | two vec2 UVs | two vec2 UVs |

`00BAD8A0` copies dest to instance+72, rec+64 to +112, UVs to +117..+129. Calls `009FE620` / `009F9DB0`. **Does not** `E8 009DB700`.

`00BAE2D0` is the handler submit (`VSHADER_2D_SPRITE` from ctor `00BAD040`). **Does not** `E8 009DB700`. Enqueue `009DB700` (`ret 24`) builds a **60-byte** local and advances display `+16020` by 60.

Init: `0042E204` → `00B26340` → `00B4AC10` allocs 56 and `call 00BAD040` (`push "VSHADER_2D_SPRITE"`). `00B4ABB0` → `00B8FAD0` types `0x22`/`0x23`.

## Isolated code

- `FrontendUiDef.GraphicIndexCrc` / `GraphicBankId`
- `FrontendSpriteBank.BankNameForWidget` — persist id → bank name; null if id 0
- `FrontendSpriteDraw` — 0xC0 packer record, fields the exe writes
- `tests/Fable.Formats.Tests/FrontendSpriteTests.cs`

`EngineLifecycle.cs` / `SilkEngineHost.cs` not edited. CPU blit / `FORREST_*` `_1_` filter stay where they are (invented present path, not this slice).
