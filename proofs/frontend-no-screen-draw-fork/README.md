# Shared frontend path has no per-screen dest/draw fork

Grep of `src/Fable.Game` layout/draw for screen-name
special cases (`PRESS_START` / `NEW_PROFILE` /
`MAIN_MENU` as dest or draw math).

Allowed: name used only to select a native
`frontend.bin` def / slot (`00598A1C` /
`00596917` / `0059899A`).

| Site | Kind | Class |
| --- | --- | --- |
| `FrontendLayout.Compute` / `LeftoverFromGraphic` / `LayoutFrontendWidgets` | no screen name | **PROVEN** generic |
| `DrawContainerWalk` / `QueueFrontend2dRecord` | type 6 vs type 0 packer, not screen | **PROVEN** generic |
| `InitFrontendUi` `FrontendRootType == 10` | type-10 ctor Note `0054E3D0` vs type-0 `0041B800` | **PROVEN** type, not dest |
| `FrontendMessages` / `DispatchFrontendMessage` | native msg ids `0xE5` / `0x126` / `15` attach named defs | **PROVEN** allowed name check |

No leftover dest numbers keyed on Press Start /
New Profile / Main Menu in the submit path.
