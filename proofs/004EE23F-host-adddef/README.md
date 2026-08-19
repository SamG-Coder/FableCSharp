# Host `004EE23F` first `+40` consume is `CHeroMorphDef`

Authority: `Fable.exe`
`listing-004c0000.txt` `004EE303`–
`004EE33E`; `009B0AC0` `ret 4`
`"Add Def Class"`; `009AD6E0`
`009FC4F0`; `[ebp-1688]=0x4E4219`;
`src/Fable.Game/EngineLifecycle.cs`
(`AddFirstDefClass`);
siblings `proofs/0044C6C2-plus40`,
`proofs/0044C6B6-host-ensure`.

Live cap `+40=0x80000` after
`0044C71F`. First consume is
this Add Def Class. Do not invent
`00A38E50` / LoadDef payload.

| Claim | Status |
| --- | --- |
| First `+40` reader after ensure | **PROVEN** `009FC4F0` via `009B0AC0` |
| First class is `CHeroMorphDef` | **PROVEN** |
| Host registers that consume | **MATCH** |
| LoadDef byte request `[esp+28]+37` | **PARTIAL** |
