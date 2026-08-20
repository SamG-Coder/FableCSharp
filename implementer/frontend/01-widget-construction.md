# 01 — widget construction (frontend.bin persist + type switch)

Authority: `Fable.exe` + `frontend.bin` + `names.bin`. No screenshots. No invented UI.

## What current C# was wrong

`FrontendUiDef.TryParse` byte-scanned every offset for a handful of CRCs and used `IsSize` / `IsPos` as authority. That:

- used the mistyped Height CRC `0x4341A19A` instead of `FableCrc("Height")` = `0x4323419A`
- missed `GraphicIndex` / Font / Colour* / States / Sprites
- treated later style-record `PositionX/Y` and coincidental `Layer`/`Angle` hits as the widget
- hard-coded Press Start children in `EngineLifecycle.AttachPressStartWidgets` (not edited here)

Native persist helpers skip the 4-byte field CRC then read a typed value. The reader is sequential.

## Native construct path (PROVEN)

| VA | Role |
| --- | --- |
| `0041DB1D` | Factory. Name → `009AD410` (or `0044C6B0` / `0043368D`) then `0041D21B`. Post `vtbl+332`. |
| `009AD410` | Def lookup by hashed name. Miss → `009E5170`. |
| `0042AEDA` | Resolve persist object + refcount via `009AD9E0`. |
| `0041D21B` | `Type = [def+60]`. `cmp eax, 43` / `ja` default. `jmp [0x41D7F8+eax*4]`. Alloc `00BFEA1A` then ctor(def). |
| `005331A0` | After base ctor `005334A0`. Walk `[def+112]..[def+116]` child pointers → `0041E5F2` factory singleton → `0041D21B`. Style vector `[def+64]..[def+68]` stride 124 (`0x84210843`). |
| `0054E3D0` | Type 10. Calls `0052CC50`, vtbl `012497E4`. |
| `0052CC50` | Type 5 group. Calls `005334A0`, vtbl `01245DE4`. |
| `005334A0` | Type 4 base. Ends with `005331A0`. vtbl `0124608C`. |
| `0041B800` | Type 0 button. `0052CC50` then vtbl `0122F5D4`. |
| `0054F5C0` | Type 6 text. `0052CC50` then vtbl `01249CCC`. |

Type 17 ctor `005482D0` takes an extra arg from `[0x13B86A0]+40+44`. Type 29 jump is the default arm (no alloc).

## Type → ctor / size / vtbl (PROVEN from `0x41D7F8`)

| Type | Ctor | Size | Vtbl | Observed role |
| --- | --- | --- | --- | --- |
| 0 | `0041B800` | `0x184` | `0122F5D4` | Button (`UI_TITLE_01`) |
| 1 | `005545D0` | `0x19C` | UNREAD | |
| 2 | `005517E0` | `0x170` | UNREAD | Table (`UI_TABLE_TITLE_WHOLE`) |
| 3 | `00550190` | `0x1A4` | UNREAD | |
| 4 | `005334A0` | `0x134` | `0124608C` | Base |
| 5 | `0052CC50` | `0x15C` | `01245DE4` | Group (`UI_TITLE`, forest) |
| 6 | `0054F5C0` | `0x18C` | `01249CCC` | Text |
| 7 | `0053DFE0` | `0x1BC` | UNREAD | |
| 8 | `0053B63E` | `0x1FC` | UNREAD | |
| 9 | `0054EA00` | `0x174` | UNREAD | |
| 10 | `0054E3D0` | `0x16C` | `012497E4` | Menu (Press Start / Main / New Profile) |
| 11 | `0054E0B0` | `0x1B4` | UNREAD | |
| 12 | `0054C3A0` | `0x1FC` | UNREAD | List |
| 13 | `0053F120` | `0x19C` | UNREAD | |
| 14 | `0054C1D0` | `0x190` | UNREAD | |
| 15 | `0054C050` | `0x1EC` | UNREAD | |
| 16 | `00549F60` | `0x1A0` | UNREAD | |
| 17 | `005482D0` | `0x198` | UNREAD | extra ctor arg |
| 18 | `00547600` | `0x170` | UNREAD | Swap (`UI_PRESS_START_SWAP`) |
| 19 | `00546F40` | `0x15C` | UNREAD | |
| 20 | `00546D30` | `0x16C` | UNREAD | |
| 21 | `00546B00` | `0x184` | UNREAD | |
| 22 | `005460C0` | `0x15C` | UNREAD | |
| 23 | `00545720` | `0x17C` | UNREAD | |
| 24 | `00544B70` | `0x1A8` | UNREAD | |
| 25 | `0041CADC` | `0x164` | UNREAD | |
| 26 | `0041CB70` | `0x160` | UNREAD | |
| 27 | `00544010` | `0x164` | UNREAD | |
| 28 | `0041CBE4` | `0x160` | UNREAD | |
| 29 | — | — | — | default / no construct |
| 30 | `00542330` | `0x1B4` | UNREAD | |
| 31 | `005415F0` | `0x180` | UNREAD | |
| 32 | `0055C650` | `0x184` | `0124C22C` | Mouse (derives type 0) |
| 33 | `0055BA20` | `0x16C` | UNREAD | |
| 34 | `0055B460` | `0x194` | UNREAD | |
| 35 | `0055A9C0` | `0x1AC` | UNREAD | |
| 36 | `00558EC0` | `0x170` | UNREAD | |
| 37 | `005407B0` | `0x18C` | `01246B8C` | EditBox |
| 38 | `00558B90` | `0x194` | `0124B04C` | AcceptButton (via `0055B460`) |
| 39 | `00558540` | `0x1C0` | UNREAD | |
| 40 | `00556350` | `0x190` | UNREAD | |
| 41 | `00559830` | `0x1DC` | UNREAD | |
| 42 | `00559360` | `0x1A0` | UNREAD | |
| 43 | `00555180` | `0x17C` | UNREAD | |

## Persist field CRCs

GameBin header is 3 bytes (`real`, `template`, `unknown`). UI body then has a u16 `0` pad. Persist starts at the Type CRC. Helpers: dword `00431102`, float `00431061`, u8 `0043314A`, string `004310A7`. On load (`[ctx+24]==2`) `00404500` skips 4 CRC bytes and does not match the name.

### PROVEN (name + type + consumed)

| CRC | Name | Type | Notes |
| --- | --- | --- | --- |
| `0x0DA8270B` | `Type` | i32 | `[def+60]`, switch 0..43 |
| `0x3DC30C85` | `Children` | i32 count + indices | PRESS_START count 6 |
| `0x8BF99D36` | `Width` | f32 | often 0; table title 640 |
| `0x4323419A` | `Height` | f32 | **not** `0x4341A19A` |
| `0x1EDB8A31` | `PositionX` | f32 | first style record |
| `0x69DCBAA7` | `PositionY` | f32 | |
| `0x51E278F0` | `Font` | i32 | 224 on groups; 26051 on type-6 text |
| `0x38E36902` | `GraphicIndex` | i32 | `GBANK_FRONT_END_PC` id. Not Graphic/Texture/Sprite |
| `0x5E5D8A25` | `Sprites` | i32 count | observed 0 |
| `0x87ACD3D8` | `States` | i32 count | style-record count (1/2/5/8) |
| `0x79902E65` | `ColourR` | f32 | first style |
| `0x144DCA8E` | `ColourG` | f32 | |
| `0x64273E01` | `ColourB` | f32 | |
| `0xFD2E6FBB` | `ColourA` | f32 | |

`Graphic` / `Texture` / `Sprite` / `Visible` / `Anchor` / `ScaleX` / `ScaleY` hashes do **not** appear as top-level CRCs on PRESS_START / TITLE / FOREST / TEXT.

### Complete CUIDef schema

Retail persist writer `00631C60` fixes the serialized order and storage of all
109 `CUIDef` fields and all 14 fields in each `CUIStateDef`. Original Lionhead
names come from the PDB-backed donor schema and are independently locked to the
retail file by `FableCrc(name) == serialized CRC` for every field.

Previously provisional examples are now classified as `MeshIndex`, `TextValue`,
`ExpansionType`, `HorizontalSeparations`, `VerticalSeparations`, `ZoomX`,
`ZoomY`, `UpdateTime`, `StateChangeType`, and `LinearChange`. The full table,
including retail and donor offsets and serialized storage, is encoded in
`FrontendUiFieldCatalog`.

`FrontendUiSchema` walks fields at exact boundaries (including every state,
vector, map, UTF-16 value, and byte flag). All 810 UI entries consume through
exact EOF; no CRC scanner or guessed tail boundary participates in validation.

## Hex walks (first style + children)

### `UI_FRONTEND_PRESS_START_MENU` #620 Type=10 raw=1710

Children: `#632 FOREST` `#200 TITLE` `#622 SWAP(TEXT)` `#624 LIST` `#621 LEGAL` `#119 MOUSE`. GraphicIndex 0. Pos 0,0.

### `UI_TITLE` #200 Type=5 raw=927

Pos 70,30. Children `#157 UI_TITLE_01` Type=0 GraphicIndex **3**, `#158 UI_TITLE_02` Type=0 GraphicIndex **4** PosX 256.

### `UI_BLENDING_BACKGROUNDS_FORREST`

Type 5 group. Tile children include `UI_FRONTEND_BG_FORREST_1_1` GraphicIndex **206**.

### `UI_PRESS_START_TEXT` #623 Type=6 raw=1080

UTF-16 `TEXT_GUI_MENU_PRESS_BUTTON`. Pos 320,240. GraphicIndex 0. Font persist 26051.

### `UI_FRONTEND_NEW_PROFILE_SCREEN` #201 Type=10

Children: title text `TEXT_GUI_MENU_NEW_PROFILE`, coastal BG, `UI_TABLE_TITLE_WHOLE` Type=2 W=640, `UI_NEW_PROFILE_MENU` Type=12, helpers.

### `UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` #216 Type=10

Children: list Type=12, `UI_TITLE`, coastal BG.

### `UI_TEXT_NEW_GAME` #261 Type=6

UTF-16 `TEXT_GUI_MENU_NEW_GAME`. PosX 120.

## Persist helpers (PROVEN)

| VA | Kind |
| --- | --- |
| `00431102` | dword. `00404500` then mode 2 `0040FE60` / mode 3 `00993EB0` |
| `00431061` | float. mode 2 `0040EFB0` |
| `0043314A` | u8. mode 2 `00403EB0` |
| `004310A7` | string. mode 2 stream `vtbl+28` → `0099B7D0` |
| `004331F9` | CString vector |

`0x122D70E` pushed to `00404500` is an empty C-string (first byte 0). On read the CRC is skipped, not checked.

## Implementation

- `src/Fable.Formats/Defs/FrontendUiFieldCatalog.cs` — complete original-name field table.
- `src/Fable.Formats/Defs/FrontendUiSchema.cs` — exact CRC+typed walk through EOF.
- `src/Fable.Formats/Defs/FrontendUiDef.cs` — runtime-facing parsed values use original field names.
- `src/Fable.Formats/Defs/FrontendWidgetType.cs` — full 0..43 table from the exe switch.
- `src/Fable.Game/FrontendWidgetFactory.cs` — generic Children-index walk.
- Tests: `tests/Fable.Formats.Tests/FrontendUiDefTests.cs`.
