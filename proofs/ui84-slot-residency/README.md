# `[ui+84]` keeps slot `0x14` / `0x17` across switch

Authority: `Fable.exe` `00598A1C` / `00596917` / `00596763` /
`0059697A` / `00595A06` / `0059B5D7`;
`src/Fable.Game/EngineLifecycle.cs`
(`AttachFrontendTree`, `BindFrontendSlot`,
`TryGetFrontendSlot`);
`proofs/ui84-list-after-attach/README.md`;
`proofs/00595222-first-node/README.md`.

Status words: **PROVEN** / **LEFTOVER** / **MATCH**.

## Verdict

`00596917` / `00596763` switch `[ui+32]` to already-built
slot `0x17`. They do **not** drop slot `0x14`.
`00595A06` overwrites existing key `0` with Main Menu.

Host no longer `Clear()`s the slot map on attach. Current
`_frontendWidgets` is still the switched screen (input /
draw). Slot roots stay in `_frontendSlotRoots`.

| Claim | Status |
| --- | --- |
| Slot `0x14` survives New Profile / Main Menu | **MATCH** |
| Slot `0x17` survives Main Menu | **MATCH** |
| Slot `0` is Main Menu after `00595A06` | **MATCH** |
| Host current list is still one screen | **LEFTOVER** vs `00595222` all-slot walk |
| Host factories every `00598A1C` slot at attach | **LEFTOVER** (lazy bind) |
