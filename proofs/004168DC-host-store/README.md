# Host `004168DC` stores `ENG_ARIAL_18` at `game+90444`

Authority: `Fable.exe` `004168DC` / `009E2C80` / `00419463`;
`src/Fable.Game/EngineLifecycle.cs`;
`src/Fable.Formats/Fonts/FontFile.cs`;
`proofs/004168DC-after-graphics/README.md`.

## Verdict

`004184BD` calls `004168DC` immediately after
Init Graphics. Work is `009E2C80("ENG_ARIAL_18")`
then `00419463` into `game+90444`. Not frontend
type-6 `ENG_ARIAL_16`/`24`.

`0059A119` applies `vtbl+192`(5) to the new
current after `00596763` old `vtbl+192`(6).

| Claim | Status |
| --- | --- |
| Init Fonts after Graphics | **MATCH** |
| `game+90444` = `ENG_ARIAL_18` | **MATCH** name |
| `009E2C80` MAIN vs STREAMING arm | **PARTIAL** |
| New current `+332=5` | **MATCH** |
