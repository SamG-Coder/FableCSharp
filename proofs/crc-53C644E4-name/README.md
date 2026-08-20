# Persist CRC `0x53C644E4` name (MessageId / +224)

Investigation only. No production `src/` edits.

Authority: `FableCrc` (`0xEDB88320`, init 0) in
`src/Fable.Formats/Defs/FableCrc.cs`; file CRC in inflated
`frontend.bin` UI (`implementer/frontend/persist-scan.txt`,
`export/frontend/persist-tail.txt`); PE `.text` listings
`tools/Fable.ExeIndex/out/01-sections/text-map/`;
`out/00-index/strings.tsv`, `xrefs.tsv`, `fourcc.tsv`;
`tests/Fable.Formats.Tests/FrontendUiDefTests.cs`
(`FableCrc("Message")` / `"MessageId"` ≠ this CRC);
`proofs/audit-messageid-parse/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

---

## Verdict

| Claim | Class |
| --- | --- |
| File CRC `0x53C644E4` is persist i32 → def `+224` / posted id (`0x126` / 15 / `0xE5`) | **PROVEN** (file + tests; not this pass) |
| English Lionhead field name for `0x53C644E4` | **UNREAD** |
| C# label `MessageId` is the Lionhead string | **DISPROVEN** |
| `FableCrc("Message")` / `"MessageId"` / `"OnClick"` / `"Action"` / `"ClickMessage"` / `"UIMessage"` / `"StoredMessage"` (and the table below) | **DISPROVEN** as this CRC |
| Sibling `0xF1A22807` is `FableCrc("Action")` | **PROVEN** |
| `.text` immediate `0x53C644E4` | **DISPROVEN** (absent; named field CRCs are also absent) |
| `names.bin` entry whose hash is `0x53C644E4` | **DISPROVEN** in persist-scan name dump |

Do **not** invent a name. Keep `FrontendUiDef.MessageIdCrc = 0x53C644E4` as a file constant. Do not add `FableCrc("MessageId")==MessageIdCrc`.

---

## 1. Hasher (PROVEN)

Same table as IEEE reflected CRC-32 polynomial `0xEDB88320`.
Init **0**, no final XOR. ASCII bytes, no NUL.

```
crc = 0
foreach b in ASCII(name):
    crc = Table[(crc ^ b) & 0xFF] ^ (crc >> 8)
```

Lock against tests / known file CRCs:

| String | `FableCrc` | Check |
| --- | --- | --- |
| `Type` | `0x0DA8270B` | `FrontendUiDefTests` |
| `Height` | `0x4323419A` | same |
| `Action` | `0xF1A22807` | file sibling; persist-scan `*Action` |

---

## 2. File siblings (PROVEN)

`00404500` file mode skips the 4-byte CRC; the name is only in the
blob. Sequential CRC+i32 in the `00632500` dword tail (Press Start
hex, same cluster on `UI_FRONTEND_BUTTON_INVISIBLE`):

| LE CRC | `FableCrc` name | Press Start i32 | INVISIBLE i32 |
| --- | --- | ---: | ---: |
| `0xA2ABA4E0` | **UNREAD** | 0 | 0 |
| `0xF1A22807` | **`Action`** | 0 | **229** (`0xE5`) |
| `0x8B645C94` | **UNREAD** | 0 | 0 |
| `0x0E79EEFC` | **UNREAD** | 0 | 0 |
| `0x12A56842` | **UNREAD** | 0 | 0 |
| `0xCB9ADD65` | **UNREAD** | 0 | 0 |
| `0x230364D6` | **UNREAD** | 0 | 0 |
| **`0x53C644E4`** | **UNREAD** | 0 | **`0xE5`** |
| `0xECEC0A1E` | **UNREAD** | 0 | 0 |
| `0x15F8091D` | **UNREAD** | 0 | `0x56` |
| `0x04158DDD` | **UNREAD** | 0 | 0 |

Hex: `E444C653` then payload. INVISIBLE:
`E444C653 E5000000`. NEW_GAME / ACCEPT payloads **15** / **`0x126`**
are tests, not this dump.

`0x53C644E4` often **equals** `Action` on the same widget (15 / `0xE5`)
but is a **different** CRC. Native `0055B040` copies `[def+224]` through
vtbl+284 (this CRC) and `[def+228]` through vtbl+320 (`Action` is the
file label for that second i32). C# only reads `0x53C644E4`.

`names.bin` persist-scan dump has no `53C644E4` row. Instance names
are not field names.

---

## 3. `.text` / `.rdata` immediates

`text-map` listings cover all of `.text` (`0x00401000`–`0x0122D000`)
and the first slice of `.rdata` (`listing-01200000.txt`). Grep
`53C644E4` / `E444C653`: **no hits**.

`xrefs.tsv`, `fourcc.tsv`, `strings.tsv`: **no** `0x53C644E4`.

`strings.tsv` also has **no** standalone field strings
`MessageId`, `OnClick`, `ClickMessage`, `UIMessage`, `StoredMessage`.
It does have `Action` at `0x0126D2A8` (the **sibling** name).

Same shape as named persist CRCs: `imm` scan of `GraphicIndex`
(`0x38E36902`) / `TypeCrc` is **0** in `.text`. Helpers skip the CRC
(`00404500`); the dword lives in the def blob (and write-side
typeinfo, **UNREAD** here — no rdata listing past `0x01240000`).

Absence of a `.text` immediate does **not** distinguish named vs
unread fields.

---

## 4. Brute (DISPROVEN)

Target **`0x53C644E4`**. No hit.

| Candidate | `FableCrc` |
| --- | --- |
| `Message` | `0xE46CD69D` |
| `MessageId` | `0xC03B36D7` |
| `MessageID` | `0xFB55161F` |
| `MsgId` | `0xE333BC52` |
| `Id` | `0x6B64510D` |
| `ID` | `0x500A71C5` |
| `Action` | `0xF1A22807` (sibling, not this CRC) |
| `ActionId` | `0x11909E92` |
| `OnClick` | `0xCDD13E86` |
| `OnSelect` | `0x95E34338` |
| `Click` | `0xF7D7086C` |
| `ClickId` | `0xC2B363D8` |
| `ClickMessage` | `0x0936C1F3` |
| `UIMessage` | `0xEA7B0B62` |
| `StoredMessage` | `0xF776D8AC` |
| `Event` | `0x768FC0EF` |
| `EventId` | `0x63241840` |
| `Command` | `0x9A791CD1` |
| `ButtonId` | `0x036163B6` |
| `nMessage` | `0x8FE7607E` |
| `iMessage` | `0x4014F7C2` |
| `PostMessage` | `0x3E97A4F3` |
| `Sound` | `0xCAE6663A` |
| `Notify` | `0x71237539` |
| `Callback` | `0x7F3CCEA2` |
| `UserData` | `0xC7C361B2` |

Also **DISPROVEN** by existing tests: `Message`, `MessageId`.

`tools/_frontend` / `TransformDump` already hashed a larger UI-field
list (`OnClick`, `OnSelect`, `Event`, `Action`, `Message`, `Id`,
`m_*`, `b*`, …) into frontend.bin; hits that printed were other
CRCs (`ZoomX`/`Layer` family), not `0x53C644E4`.

Not claimed (not hashed here): every hungarian/prefix variant
(`m_nMessage`, `dwMessage`, `nID`, …). Those remain **UNREAD**,
not invented.

---

## 5. What this pass did not do

- Did not name `0x53C644E4`.
- Did not inflate `frontend.bin` beyond in-repo hex.
- Did not dump remaining `.rdata` typeinfo for write-side CRC
  tables (`00631C60` / `00404500` skip the dword).
- Did not name sibling tail CRCs except **`Action`**.
