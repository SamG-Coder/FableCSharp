# Type-4 Press Start poster + persist tail u8s

Authority: `Fable.exe` `0042E3EE` / `0054E280` / `00631C60` / `005331A0`.

## Input (PROVEN)

| Claim | Status |
| --- | --- |
| Event type 4 (`[record+40]`) → action 26 | **PROVEN** `0042E3EE` |
| Type-10 action 26 posts widget+352 (`0xE5` on Press Start) | **PROVEN** `0054E2FA` |
| Return (DIK 28) is type 1 / action 33 | **PROVEN** |
| Return → `0xE5` / `0x126` / 15 | **DISPROVEN** |
| `0x126` and 15 posters | **UNREAD** — tests inject recovered ids |

C# was posting `0xE5`/`0x126`/15 from host Return. Replaced
`MaybeActivateNewGameFromInput` with `FrontendInputMap.TryMapEvent`.
`EngineInput.ApplyEvent` now dispatches type 4 → action 26.

## Persist (PROVEN)

CUIDef persist `00631C60` writes `+189`/`+190` as u8 via `0043314A`.
File CRCs `0xBDACBABA` / `0xAC637D43`. Names **UNREAD**.
Parser now consumes them so `+191` Absolute stays aligned.

Type-6 Font i32 is a `names.bin` offset (`009D49B0`). `26051` →
`ENG_ARIAL_24` from the installed names.bin. Hardcoded
`ENG_ARIAL_16` (`0054F4B0`) is a different helper, not the
type-6 persist mapper.

Root `+520` remap size = 1 remains persist-proven. Type-6 leftover
`+204` first-seen stays 0 (`GraphicIndex=0`). Dest origin is remapped
`512,384` via inherited parent dest scale.

## Proposed remaining

Physical device that produces type 4 is still **UNREAD**.
`0x126` / 15 posters still **UNREAD**.
