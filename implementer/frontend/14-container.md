# 14 — Type 10 container walk (`0054E3D0` → `005331A0` → `0052C730` → `00530260`)

Authority: `Fable.exe` + `frontend.bin`. No screenshots. No `FORREST_*` name filters.

## What current C# was wrong

`CompositeFrontendPresent` and the type-10 draw note walked every constructed widget with nonempty dest. Native type 10/5/12/18 `vtbl+8` is `00530260`, which walks `+176` and skips clipped/borrowed children. Type 18 (`CSwappingStateComponent`) walks every persist child; its selected state is expressed by the blending children's style colours.

## Native path (PROVEN)

| VA | Role |
| --- | --- |
| `0054E3D0` | Type 10 ctor. `0052CC50` then vtbl `012497E4`. Zeros `+352/+356/+360`. |
| `0052CC50` | Type 5 ctor. `005334A0` then vtbl `01245DE4`. |
| `005334A0` | Type 4 base. Zeros `+176/+188` lists, `+300/+302/+303`. Ends `005331A0`. |
| `005331A0` | Persist flags into `+300/+302/+303`. Style map `+36` stride 124. Children `[def+112]..[def+116]` → `0041D21B` → `vtbl+236` `00533B50` append `+176` (8-byte slots). |
| `0054E4B0` | Type 10 `vtbl+172`. `0052C730` then `UI_ACCEPT` lookup. |
| `0052C730` | `005339B0` layout, then `+324/+328/+332=0`, `+320=-1`, `+340=1`. First-seen state **0**. |
| `005339B0` | Writes dest scale. Walks `+176`: if `vtbl+208` (`+200` parent) is 0, `vtbl+204` set parent and `vtbl+172` recurse. |
| `0052CF40` | `vtbl+192` select. `+332=state`. Own `+176` children get `vtbl+188` → `vtbl+192`. |
| `00530260` | Type 5/10/12/18 `vtbl+8`. Walk `+176` then `+188`. |
| `0052FE3C..0052FFA2` | Compose local/inherited RGBA and write final draw colour at `+148`; `vtbl+404` absolute breaks inheritance. |
| `00547600` | Type 18 ctor. `0052CC50`, vtbl `012485AC`, `00547500` swap list at `+348`. |
| `00547360` | Type 18 `vtbl+172`. `0052C730` then `+364=0xD`. |

Type 0 `vtbl+8` is `0041AFA0`. Type 6 is `0054EF00`. Those are leaves.

### `00530260` skip (PROVEN)

```
parent = child.vtbl+208          // [child+200]
if parent != this && !child.vtbl+400: skip   // +300 bit 7
if child.vtbl+420: skip                      // +302 bit 0  (twice)
else child.vtbl+8(...)
```

`vtbl+400` `0052F180` = `[+300] >> 7` from def `+504`.
`vtbl+420` `0052F1D0` = `[+302] & 1` from def `+392`.
Own children (`parent==this`) only test clip. First-seen `+188` is empty (ctor 0).

`005331A0` flag map:

| Def | Widget | Bit | Draw/layout |
| --- | --- | --- | --- |
| `+60` | `+300` low 5 / `+301` | type | `vtbl+260` |
| `+504` | `+300` bit 7 | borrowed-visible | `vtbl+400` |
| `+191` | `+300` bit 6 | absolute | `vtbl+404` |
| `+476` | `+300` bit 5 | | `vtbl+416` |
| `+392` | `+302` bit 0 | clip / skip draw | `vtbl+420` |
| `+188` | `+302` bit 1 | centre | |
| `+520/+521` | `+302` bits 6/7 | remap | |
| `+180` | `+303` | layer | |

Persist names for def `+188/+191/+392/+476/+504/+520/+521` are **UNREAD**. `Visible` / `Enabled` / `Clip` CRCs do not appear on the Press Start subtree.

## Swap / select (PROVEN)

Type 18 `vtbl+4` `00547380` times the `+348` list (`00547500` from def `+480/+492`). First tick: `+324==+328==0`, elapsed 0, duration > 0 → **no** `vtbl+192`. First-seen state stays 0.

Type 18 persist `States` equals child count (`UI_SWAPPING_FORREST` 4, sunbeam 3, `UI_PRESS_START_SWAP` 1). Child index 0 is state 0.

Type 5/10/12 do not exclusive-select. `+332=0` is the style key, not a hidden sibling.

## Press Start first-seen contributing set (PROVEN tree + first-seen state 0)

`UI_FRONTEND_PRESS_START_MENU` type 10 children, persist order:

| Child | Type | First-seen |
| --- | --- | --- |
| `UI_BLENDING_BACKGROUNDS_FORREST` | 5 | visible; both type-18 children live |
| `UI_TITLE` | 5 | visible |
| `UI_PRESS_START_SWAP` | 18 | visible; child 0 is the only state |
| `UI_FRONTEND_LIST_PRESS_START_MENU` | 12 | visible (`00530260`) |
| `UI_LEGAL_TEXT` | 6 | visible |
| `UI_MOUSE_POINTER` | 32 | visible |

Forest type-18 children are all walked. State 0 gives the first blending child alpha 1 and the remaining blending children alpha 0; inherited colour makes only the first tile set contribute:

| Swap | Alpha 1 at state 0 | Alpha 0 at state 0 |
| --- | --- | --- |
| `UI_SWAPPING_FORREST` | `BLENDING_BG_FORREST_1` + `UI_FRONTEND_BG_FORREST_1_1`..`1_6` | `BLENDING_BG_FORREST_2/3/4` and their tiles |
| `UI_SWAPPING_FORREST_SUNBEAM` | `BLENDING_BG_FORREST_SUNBEAM_1` + `…_1_1`..`1_6` | sunbeam 2 and 3 sets |
| `UI_PRESS_START_SWAP` | `UI_PRESS_START_TEXT` | — |

`UI_TITLE_01` and `UI_TITLE_02` are both visible (type 5, no swap).
LIST is visible; only child is `UI_FRONTEND_BUTTON_INVISIBLE` type 11, dest 0.
LEGAL (`TEXT_GUI_MENU_LEGAL` at 320,340) is visible.
MOUSE (`GraphicIndex` 362) is visible.

Zero-effective-alpha tiles stay in the widget list and native container walk. Present may omit their no-op records. No string filter.

Visibility / enabled / clip inherit through the tree. Blending RGBA also inherits: the parent style alpha suppresses an otherwise opaque child texture. `00530260` only avoids recursion for its proven borrowed/clip checks.

## Implementation

- `FrontendWidget.Visible` / `Enabled` / `Clip` / `ActiveChild`
- `FrontendWidgetFactory.EffectiveColour` for `0052FE3C..0052FFA2` inherited RGBA
- `FrontendWidgetFactory.ApplyFirstSeenState` after the Children walk
- `FrontendWidgetType.DrawsChildList` (5/10/12/18) / `SelectsChild` (18)
- `EngineLifecycle` draw = `00530260` recurse `+176`; present only `Visible && !Clip`

## UNREAD

- Persist CRC names for def `+392/+504`
- First-seen `+188` contents after more than one tick
- Exact host timing/interpolation of the type-18 forest and sunbeam state cycle
- Type 2 table `vtbl+8`
