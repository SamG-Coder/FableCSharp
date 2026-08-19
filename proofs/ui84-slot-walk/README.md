# Tick/draw walk every resident `[ui+84]` slot

Authority: `Fable.exe` `00595222` / `0059A0C4` / `004292C0`;
`src/Fable.Game/EngineLifecycle.cs`
(`ResidentSlotTrees`, `TickFrontendWidgets`,
`DrawFrontendWidgets`);
`proofs/00599E3F-walk-slots/README.md`;
`proofs/00595222-first-node/README.md`.

Status words: **PROVEN** / **LEFTOVER** / **MATCH**.

## Verdict

`00595222` (`vtbl+8`) and `0059A0C4` (`vtbl+4`) walk
`[ui+84]` in key order and skip null values. They do
**not** filter to `[ui+32]` current.

Host now walks every resident slot tree in that order.
`_frontendWidgets` stays the switched screen for input.
SelectState(`vtbl+192`(6)) on the old current is
**UNREAD** as a hide; first-seen Press Start has one
slot so present **MATCH**es.

| Claim | Status |
| --- | --- |
| In-order walk of non-null `[ui+84]` | **MATCH** |
| Current list is still one screen | **LEFTOVER** vs all-slot input |
| Hide via `vtbl+192`(6) | **UNREAD** |
