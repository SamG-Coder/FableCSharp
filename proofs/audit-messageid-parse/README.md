# Audit: `MessageIdCrc` parse vs `0055B040` def+224

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `0055B040` / `0055B460` / `00558B90` /
`0054E0B0` / `00631C60` / `00632500` / `00404500` / `00431102`;
`frontend.bin` UI blobs (`export/frontend/persist-tail.txt`,
`export/frontend/forest-persist.txt`);
`src/Fable.Formats/Defs/FrontendUiDef.cs` (WIP);
`src/Fable.Game/FrontendInputMap.cs`;
`src/Fable.Game/FrontendWidgetFactory.cs`;
`src/Fable.Game/EngineLifecycle.cs` (`AttachFrontendTree`,
`MaybeActivateNewGameFromInput`);
`tests/Fable.Formats.Tests/FrontendUiDefTests.cs`,
`FrontendInputTests.cs`;
`proofs/persist-flag-names/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH** / **STALE**.

---

## Verdict

| Item | Status | Native | C# WIP |
| --- | --- | --- | --- |
| `0055B040` reads def `+224` as dword | **PROVEN** | `[eax+224]` → box → `vtbl+284` if nonzero | Comment **MATCH** |
| File form of `+224` is CRC + i32 | **PROVEN** | `00631C60` `lea [esi+224]` → `00632500` = `00404500` skip CRC + 4-byte payload | `ReadPersistI32` assumes this |
| Helper is `00431102` | **DISPROVEN** | `+224` uses `00632500`. `00431102` is the Layer/`+304` family | `PersistDwordFn` comment on `ReadPersistI32` is **STALE** |
| CRC `0x53C644E4` is the `+224` field | **PARTIAL** | No `.text` immediate. File-recovered. Tests: ACCEPT `0x126`, NEW_GAME `15` | Constant + scan |
| Name `MessageId` | **DISPROVEN** as Lionhead name | Tests: `FableCrc("Message")` / `"MessageId"` ≠ `0x53C644E4` | Label only. Name **UNREAD** |
| Sequential walk stores `+224` | **DISPROVEN** | Native does not walk CRC names; `00631C60` is ordered | Sequential arm **skips** 4 bytes and does not assign. Value is always the scan |
| Sequential stop at `0xBDACBABA` | **STALE** (TryParse) / **MATCH** (persist-tail dump) | `+189` is `0043314A` u8 | TryParse consumes `Plus189Crc` as u8. Dump still treats it as i32 |
| Current sequential unread | **PROVEN** on forest | After `+189/+190/+191` u8 comes `+160` (`00632420`) | `forest-persist.txt` `unread=0x424AD096` `partial=True` on every FORREST widget |
| Scan vs sequential for ACCEPT / NEW_GAME | **PARTIAL** | `+224` is far after `+160` / `+148` vec / Layer / Centre | Scan finds first `0x53C644E4`. Tests match native ids. Uniqueness **UNREAD** as a corpus proof |
| Press Start persist `+224` | **PROVEN** 0 | `0055B040` `test ecx,ecx` / `je` skip | Factory leaves 0. `AttachFrontendTree` then writes `0xE5` (**LEFTOVER** vs attach `00598EE6` `widget+352`, not persist) |
| Type 11/38 ctor copies persist | **PROVEN** | `0054E0B0` / `00558B90` → `0055B460` → `0055B040` | Factory copies `def.MessageId` onto the widget |

---

## 1. `0055B040` (ctor copy, not the action poster)

```
0055B040  this = ebx
          call [vtbl+432]          ; def*
          [ebx+396] = [def+388]
          ecx = [def+224]
          test ecx, ecx
          je 0055B15A              ; skip if 0
          box ecx
          cmp [box], 65            ; id==65 → extra [def+0x1D8] string
          call [this.vtbl+284]     ; store boxed id
          then the same pattern for [def+228] → vtbl+320
          and [def+232]
```

Callers:

| Site | Role |
| --- | --- |
| `0055B4B5` in `0055B460` | Type **34** base ctor (`FrontendWidgetType` row 34). Sets vtbl `0x124BD2C` then copies persist |
| `0055B515` in `0055B4C0` | Sister ctor |
| `0054E0B8` | Type **11** `0054E0B0` (`UI_FRONTEND_BUTTON_NEW_GAME`) |
| `00558B98` | Type **38** `00558B90` (`UI_ACCEPT_NEW_PROFILE`) then vtbl `0x124B04C` |

`0055AD60` (type 38 action, `lea eax,[edi-26]`) is **not** the persist reader. Action 26 sets click state and calls `0055B9D0`. The persist id is already on the widget from the ctor.

Type-10 Press Start is a different store: attach `00598EE6` `mov [eax],0xE5` then `vtbl+284` = `0054E4F0` → `widget+352`. Persist `+224` on that menu is 0, so `0055B040` does not write it.

---

## 2. Persist writer `00631C60` `+224`

After style vector `00632E00` and the `+189/+190/+191` u8s (`0043314A`):

`+160` `00632420` → `+148` vec → four f32s → `+180` Layer `00431102` → `+184` Angle → `+188` Centre u8 → long tail (`+192` … `+400`) → five `00632500` dwords `+196/+200/+204/+208/+212` → **`+224` `00632500`** → `+228/+232/+236/+240/+220/+216/+244/+248/+256/+252` same helper.

`00632500` is the same shape as `00431102`:

1. `push 0x122D70E` / `call 00404500` — file mode 2 **skips the 4-byte field CRC**
2. mode 2 inner `00632550` reads **4 bytes** into the dest pointer

File form is **CRC + i32**. The CRC itself is not an immediate; it lives only in the blob (and in the write-side typeinfo). `.text` listing has **no** `0x53C644E4`.

C# `ReadPersistI32` comment citing `00431102` is the right *shape*, wrong *helper*. Sibling fields `+228/+232` are **UNREAD** in C#.

---

## 3. Sequential walk vs `ReadPersistI32` scan

### 3.1 What TryParse does now

Sequential CRC walk (prefix through styles, then flags):

- `Plus189Crc` `0xBDACBABA` / `Plus190Crc` `0xAC637D43` → `cursor = payload+1` (u8). **Does not stop.**
- `MessageIdCrc` `0x53C644E4` → `cursor = payload+4`. **Does not store.**
- Unknown CRC → `Partial=true`, `UnreadCrcs[0]=crc`, **break**.

After the walk:

```
messageId = ReadPersistI32(raw, MessageIdCrc);  // byte-step first match
```

`ReadPersistI32` is the only assignment. Sequential reach of `MessageIdCrc` is not required for the property to be set.

### 3.2 Why sequential never stores `+224`

`00631C60` order after `+191` Absolute (`0x38BBD87F`, already consumed as u8) is **`+160`**. That CRC is **not** in the sequential table.

`export/frontend/forest-persist.txt` (from `FrontendPersistTailTests`, live `TryParse`):

```
UI_BLENDING_BACKGROUNDS_FORREST  partial=True  unread=0x424AD096
UI_FRONTEND_BG_FORREST_1_1       partial=True  unread=0x424AD096
… every FORREST* widget …
```

So the live sequential stop is **`0x424AD096`**, not `0xBDACBABA`. That unread is the first field after the three u8s (`+160` / `00632420`). Everything after it — Layer, Centre, **`+224`** — is behind that break.

### 3.3 Where `0xBDACBABA` still “stops” a parser

`export/frontend/persist-tail.txt` (`DumpSequential` in `FrontendPersistTailTests`) always steps **CRC+i32** except Children / Text / a broken NESTED skip. On `UI_BLENDING_BACKGROUNDS_FORREST`:

```
@0107  0xBDACBABA  u8=1  i32=1669153537   ; 0x637DAC01 = u8 01 + first 3 of +190
@0115  0xD87F01AC                         ; desync
…
@0491  0x53C644E4  i32=0                  ; 4-byte window on the desynced grid
```

That dump **MATCH**es “partial stop at `0xBDACBABA`”. It is **not** current `TryParse`. Treating `+189` as i32 is **DISPROVEN** (`0043314A`, `proofs/persist-flag-names`).

Older sequential (no `Plus189Crc` arm) would also stop at `0xBDACBABA`. That behaviour is **STALE**.

### 3.4 Scan safety

`ReadPersistI32` is the same first-hit byte scan as `ReadPersistU8`:

- **PROVEN** on the two lifecycle blobs the tests open: ACCEPT first `0x53C644E4` payload is `0x126`; NEW_GAME is `15`.
- **UNREAD** whether any earlier unaligned window equals `0x53C644E4` on other UI entries (UTF-16 text, style floats, the desynced `@0491` window on FOREST happens to be the real CRC with payload 0).
- Sequential skip of `MessageIdCrc` cannot fire until `+160` and the tail are walked. Adding that arm without walking `0x424AD096` does not change results.

---

## 4. C# leftovers around the parse

| Site | Leftover |
| --- | --- |
| `TryParse` sequential `MessageIdCrc` arm | Advances cursor only. Dead until `+160` is parsed |
| `EngineLifecycle.AttachFrontendTree` | If Press Start root `MessageId==0`, write `0xE5`. Native attach writes `widget+352`, not persist `+224` |
| `FrontendInputMap.MessageFromWidgets` | First visible type 10/11/38 with `MessageId!=0`. Native posts **that widget’s** stored id (`0054E280` / ctor `vtbl+284`), not “first in the list” |
| `MaybeActivateNewGameFromInput` | Extra Press Start `0xE5` if `TryMapEvent` is null. Covers persist-0 root if the patch were removed |
| `+228` / `+232` | Native `0055B040` copies them too. C# **UNREAD** |

---

## 5. Proposed (do not apply here)

1. Keep `0x53C644E4` as a file CRC. Keep “Name UNREAD”. Do not add `FableCrc("MessageId")==MessageIdCrc`.
2. Keep `ReadPersistI32` until sequential walks `+160` (`0x424AD096`) and the `00631C60` tail through `+224`. Cite `00632500`, not `00431102`.
3. Assert `Partial` + `UnreadCrcs[0]==0x424AD096` on ACCEPT / NEW_GAME / FOREST so a future sequential extension is visible.
4. Optionally scan only after `UnreadOffset` (or after the last consumed u8) to ignore unaligned hits in the prefix.
5. Sequential `MessageIdCrc` arm should assign if it ever runs; today it is dead.
6. Do not treat persist-tail `@0491` as a schema walk. That dumper is still i32-stepped through `+189`.

---

## 6. What this pass did not do

- Did not inflate `frontend.bin` here; values `0x126` / `15` are from existing tests against the installed blob.
- Did not recover the Lionhead field string for `0x53C644E4` or `0x424AD096`.
- Did not map type 11/38 `vtbl+284` to a concrete store offset (type-10 `+352` is a different vtbl).
- Did not parse `+228` / `+232` / id `65` string at def `+0x1D8`.
