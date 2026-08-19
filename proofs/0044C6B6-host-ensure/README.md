# Host `0044C6B6` miss constructs `0xE0` before Thing Components

Authority: `Fable.exe` `0041852D` / `0044C6B6` /
`0044C6C2` / `0044C71F`;
`src/Fable.Game/EngineLifecycle.cs`
(`EnsurePlayerManagerSingleton`);
`proofs/0044C6B6-first-omit/README.md`.

Init frontend `005952C3` applies `vtbl+192`(5) to
Press Start `[ui+32].back()` after `00598A1C`.

| Claim | Status |
| --- | --- |
| First-seen `[0x13B879C]==0` | **PROVEN** |
| Host ensure before Thing Components | **MATCH** |
| `0044C6B0` is this ctor | **DISPROVEN** (later getter) |
| `005952C3` first-seen `+332=5` on slot `0x14` | **MATCH** |
