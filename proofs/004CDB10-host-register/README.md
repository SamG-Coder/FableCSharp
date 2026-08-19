# Host `004CDB10` registers `00A39010` at `[0x13B8A54]`

Authority: `Fable.exe` wchar
`0x122F3D0`=`Data\Defs\`,
`0x1239E74`=`misc_def_types.h`;
`00418637` `004CDB10` /
`0041A080` / `0099BF30` /
`00A39010` `ecx=0x13B8A54`;
`src/Fable.Game/EngineLifecycle.cs`
(`InitSubtitledMessageFn` /
`SubtitledSymbolsRegistered`);
siblings `proofs/004CDB10-00A39010`,
`proofs/004CDB10-subtitled-body`.

Init Game named sibling after
Init Fonts. Not a spoken line.
Do not invent `00A38E50` parse.

| Claim | Status |
| --- | --- |
| Path is `Data\Defs\` + `misc_def_types.h` | **PROVEN** from exe |
| Host `EnterGame` calls `00A39010` | **MATCH** |
| Host queues Speak / `004CDC40` | **DISPROVEN** |
| `00A38E50` enum table payload | **UNREAD** |
