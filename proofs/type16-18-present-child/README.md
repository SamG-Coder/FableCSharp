# Type 18 / 16 first-seen present-child

Authority: `Fable.exe` `fn` / `vtbl` of
`00547600`, `00549F60`, `00530260`,
`00549B20`, `00547360`, `00549230`,
`00548F40`, `0052CF40`; vtbls
`012485AC` / `01248A8C`.

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **MATCH**.

Do not map attach `SelectState(5)` onto
type-18 `ActiveChild`. Do not invent
type-6 dest width. Do not add type 38
to `SelectsChild`.

## Verdict

`00530260` (`vtbl+8` on type 5 / 10 / 12 /
**16** / **18**) walks **every** `+176`
then `+188` child. Skip is `vtbl+400`
(visible) or `vtbl+420` (clip). There is
**no** exclusive-index walk in that
function.

Type 18 / 16 keep persist child **0** at
first-seen. Inactive siblings stay in the
tree. Host `SelectsChild` +
`ApplyFirstSeenState` `Visible=false` on
`k != 0` is the generic hide so New
Profile `WASD` / `INVERTED` /
`COASTAL_SUNBEAM_2_*` are not presented.

| Claim | Class |
| --- | --- |
| Type 18 ctor `00547600` vtbl `012485AC` | **PROVEN** |
| Type 16 ctor `00549F60` vtbl `01248A8C` | **PROVEN** |
| Both `vtbl+8` = `00530260` | **PROVEN** |
| `00530260` exclusive-walks `ActiveChild` | **DISPROVEN** |
| Type 18 `vtbl+172` `00547360` → `0052C730` then `+332=0` | **PROVEN** |
| Type 16 `00549B20` writes `+348=0` | **PROVEN** |
| Type 16 `vtbl+192` is `00548F40`, not `0052CF40` | **PROVEN** |
| Type 16 selected child is `+348` | **PROVEN** |
| First-seen presented child is persist index 0 | **MATCH** |
| Attach `SelectState(5)` = type-18 child 5 | **DISPROVEN** |
| Type-6 leftover `+204` is dest width | **DISPROVEN** |
| Type 38 ON/OFF is `SelectsChild` | **UNREAD** |
