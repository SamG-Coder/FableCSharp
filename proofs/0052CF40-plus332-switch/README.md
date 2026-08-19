# `00596763` `vtbl+192`(6) writes `+332`, not `+302` hide

Authority: `Fable.exe` `0052CF40` / `00596763`;
`src/Fable.Game/EngineLifecycle.cs` (`SelectFrontendState`);
`src/Fable.Formats/Defs/FrontendWidgetType.cs`;
`proofs/00595222-visible-skip/README.md`;
`proofs/0052C730-host-state/README.md`.

Status words: **PROVEN** / **UNREAD** / **DISPROVEN**.

## Verdict

`0052CF40` stores the arg at **`+332`** and forwards
`vtbl+188` to own `+176` children. It does **not**
`or [+302],1`. Host writes `FrontendWidget.State`.

`004A67D0` / `004A6E30` belong inside `"Init World"`
`0041735A`, before `"Init Display Engine"` `00417418`.

| Claim | Status |
| --- | --- |
| `vtbl+192` is `0052CF40` | **PROVEN** |
| Arg 6 on old current at `00596763` | **PROVEN** |
| `+332 = 6` is a `+302` hide | **DISPROVEN** |
| World ctor after particles | **DISPROVEN** leftover; moved |
