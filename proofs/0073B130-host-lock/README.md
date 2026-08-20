# `004F9129` `0073B130` fill then `004F9139` `004EBACE` commit

Lead lock after listing recovery. Note-only + flags
in `EngineLifecycle`. Not a live bump table.

Do **not** invent `ActivateQuest`. This is Init
Thing Components epilogue after n=111
`CHasNameDef`. Not `00DBDE40`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `listing-004c0000.txt` `004F9112`–
`004F9144`; `listing-00700000.txt` `0073B130`–
`0073CB40`; `functions.tsv` `0x0073B130`;
`e8.tsv` dest `0073B130` only `004F9129`, dest
`004EBACE` site `004F9139`;
`proofs/004EE23F-thing-components`;
`proofs/004F8E89-hasname-tail`.

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| What is `0073B130`? | Unrolled 8-byte `{u32 tag, fn*}` bump vs limit `0x13BAD4C`. Grow `00743270`. Second family `00743B30`. Table commit `007441D0`. `ret 0073CB40`. | **PROVEN** |
| First record? | tag **0** thunk **`00742430`**. Next recovered: tag 4 thunk `0073A0A0`; tag 5 thunk `007426F0`. | **PROVEN** |
| `ecx` / map? | Global bump (`ectA` dump-label). **Not** the `esi` def map. | **PROVEN** |
| Flag before fill? | `004F9112` stores **1**. Earlier `004EE2EF` also stores 1. | **PROVEN** store; BSS VA **PARTIAL** (IAT-adjacent dump-label) |
| Does `004EBACE` run this walk? | **Yes.** `ecx=esi`. Reads `[esi]` / `[esi+4]` / `[esi+12]` u8, `004EB9A6`, zeros `[esi+13]`. | **PROVEN** |
| Constructs `Q_NewOakValeIntro`? | **No.** | **DISPROVEN** |
| Host | Notes `004F9129` / `0073B130` / `004F9112` / `004F9139` / `004EBACE` / `004F9144`. Flags `ThingComponentsFilled` / `ThingComponentsCommitted`. Not a live table. | **MATCH** Note-only |

`functions.tsv` size **2386** is insn count, not
bytes (`0073CB40 − 0073B130 + 1` = `0x1011`).
Earlier 22666 figure is **DISPROVEN**.

Next unread: thunk bodies `00742430` /
`00743270` / `00743B30` / `007441D0`;
flag BSS VA; full tag list.
