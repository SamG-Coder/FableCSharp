# Attach `0xE5` is slot `0x14` `0059B5D7`, not a type-10 walk

Authority: `Fable.exe` `00598A1C` / `00598EE6` / `0059B5D7` /
`0054E4F0` (`listing-00580000.txt`, `listing-00540000.txt`);
`src/Fable.Game/EngineLifecycle.cs` (`BindFrontendSlot`,
`WriteType10AttachMessage`);
`src/Fable.Game/FrontendInputMap.cs` (`SlotLookupFn`);
`proofs/slot-0x14-lookup/README.md`;
`proofs/0054E4F0-store-shape/README.md`;
`proofs/00598A1C-only-e5/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH**.

## Verdict

Native attach write is:

1. `00598EE6` `[packet+0] = 0xE5`
2. `0059B5D7(0x14)` on `[ui+84]`
3. `vtbl+284` → type-10 `0054E4F0` stores packet* at +352

It does **not** walk `widgets[0]` or test `Type==10` /
`MessageId==0`. Host now looks up slot `0x14` the same way.
`MessageId` is still the collapsed `[packet+0]` id
(**LEFTOVER** layout; first-seen id **MATCH**).

| Claim | Status |
| --- | --- |
| `0059B5D7` keys `[ui+84]` | **PROVEN** |
| Attach targets slot `0x14` | **PROVEN** |
| Host type-10 + `MessageId==0` walk | **DISPROVEN** leftover; removed |
| Host slot map is a full multi-slot tree | **LEFTOVER** (one tree at a time) |
| Host `MessageId` is packet* | **DISPROVEN** leftover field |
