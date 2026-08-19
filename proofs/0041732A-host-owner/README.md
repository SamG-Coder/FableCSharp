# Host `0041732A` stores 44-byte owner at `game+28`

Authority: `Fable.exe`
`listing-00400000.txt` `0041732A`–
`00417359`; `0044C6B0`;
`0044A3B0` vtbl `01231CD0`
`hero_swap_1.tng`…`_4.tng`;
`004193A0` `[esi+28]`;
`src/Fable.Game/EngineLifecycle.cs`
(`ApplyPlayerOwner`);
siblings `proofs/004473A0-player-iface`,
`proofs/004166A8-create-players-work`.

Named sibling after Conversation
Attitude. Not Create Players.
Not Oakvale.

| Claim | Status |
| --- | --- |
| `00BFEA1A(44)` `0044C6B0` `0044A3B0` `004193A0` | **PROVEN** |
| Host applies at Init Player Manager | **MATCH** |
| Host ctor note on Init Player Interface | **DISPROVEN** (moved) |
| `+24=0` write | **DISPROVEN** |
