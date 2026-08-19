# Root draw is `DrawsChildList`, not Press Start type==10

`00595222` calls `[node+20].vtbl+8`. Types 5/10/12/18
are `00530260`. Host `DrawFrontendWidgets` now uses
`FrontendWidgetType.DrawsChildList(FrontendRootType)`.

`AttachFrontendTree` no longer scans
`UI_PRESS_START_TEXT` after every root.
`00598EE6` stays on the `00598A1C` attach only
(slot `0x14` / vtbl+284).
