# Host `00416005` runs `[vtbl+8]` then `009ACB10`

Authority: `Fable.exe`
`listing-00400000.txt` `00416005`–
`00416044`; rdata
`01232C24+8`=`0044C72B`;
`009ACB10` `[this+88]`
`009E5250`; parent `push 1`;
`src/Fable.Game/EngineLifecycle.cs`
(`PrepareDefinitionManager`);
sibling `proofs/00416005-def-manager`.

Not `[0x13B8A54]`. Not game.bin
parse. `0044C72B` first strings
are `pc\` + `*.h`.

| Claim | Status |
| --- | --- |
| `[edx+8]` dest is `0044C72B` | **PROVEN** from exe |
| Host getter + vtbl+8 + reset | **MATCH** |
| Host opens / parses `game.bin` | **DISPROVEN** |
| `009F2450` `[0x13CAA90]` first-seen live | **PARTIAL** |
