# Dump `ReadPersistU8(0x9E47F106)` on NEW_GAME / ACCEPT

Investigation only. No production `src/` edits.

Question: `FrontendUiDef.ReadPersistU8` on inflated
`frontend.bin` entries `UI_FRONTEND_BUTTON_NEW_GAME` and
`UI_ACCEPT_NEW_PROFILE` for CRC **`0x9E47F106`**
(`CUIDef+545`) — file byte **0 or 1**?

Authority: TLC
`C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters\data\CompiledDefs\frontend.bin`
and sibling `names.bin` (both present; no `Development/`
override);
`src/Fable.Formats/Defs/FrontendUiDef.cs` `ReadPersistU8`;
`src/Fable.Formats/Defs/GameBin.cs` / `NamesBin.cs`;
`FrontendUiDefTests.Persist_00631C60_plus189_plus190_are_u8_and_font_is_names_offset`
(opens both entries; **does not** scan this CRC);
`implementer/frontend/persist-scan.txt` `#625` hex only;
`export/frontend/persist-tail.txt` (Press Start / TITLE /
FOREST / MOUSE);
`proofs/newgame-plus545/Dump.csx`;
`proofs/invisible-plus545/README.md`;
`proofs/accept-newgame-plus545/README.md`.

Do **not** copy INVISIBLE `#625` `06F1479E 01`. Do not
re-prove dest `+545` / type-11 gate / type-38 no-test
(`cuidef-plus545`, `0043314A-setne-545`).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN**.

---

## Verdict

| Widget | Type | `ReadPersistU8(raw, 0x9E47F106)` | Status |
| --- | ---: | ---: | --- |
| `UI_FRONTEND_BUTTON_NEW_GAME` | 11 | **not recovered** | **UNREAD** |
| `UI_ACCEPT_NEW_PROFILE` | 38 | **not recovered** | **UNREAD** |
| `UI_FRONTEND_BUTTON_INVISIBLE` (control only) | 11 | **1** (`06F1479E 01`) | **PROVEN** (prior hex; not this answer) |

| Claim | Status |
| --- | --- |
| `frontend.bin` + `names.bin` exist at TLC `CompiledDefs\` | **PROVEN** |
| Tests already `FindEntry` both names (type 11/15, 38/`0x126`) | **PROVEN** |
| `ReadPersistU8` = first LE `u32==crc` then `raw[i+4]` | **PROVEN** (`FrontendUiDef.cs` 595–603) |
| Miss returns **0** (cannot treat miss as file 0) | **PROVEN** |
| This pass inflated those two `entry.Raw` blobs | **UNREAD** (no `dump-out.txt`; `read_file` rejects `.bin`) |
| Checked-in hex for those two names | **DISPROVEN** (`persist-scan` name table only; no `hex:`) |
| `persist-tail.txt` covers NEW_GAME / ACCEPT | **DISPROVEN** |
| Tests assert `ReadPersistU8(..., 0x9E47F106)` | **DISPROVEN** |
| File byte is INVISIBLE’s **1** | **DISPROVEN** (method) |
| File byte is PRESS_START / TEXT **0** | **DISPROVEN** (method) |

**Answer:** **UNREAD.** CRC / helper reconstructed. File
0 vs 1 on these two blobs was **not** dumped this pass.
Do not invent **0** or **1**.

---

## 1. Helper reconstructed (not executed on the two blobs)

```
public static byte ReadPersistU8(byte[] raw, uint crc)
{
    for (var i = 0; i + 5 <= raw.Length; i++)
        if (BitConverter.ToUInt32(raw, i) == crc)
            return raw[i + 4];
    return 0;
}
```

File form matches `0043314A` mode 2: skip CRC, one `u8`.
`00403EB0` `setne` makes the **runtime dest** 0 or 1;
the **file** byte is whatever sits after the CRC (Press
Start flags are already 0/1). First hit only. Neighbours
on every recovered CUIDef tail:

```
1D972DCA uu     ; 0xCA2D971D +544
559B9CE5 uu     ; 0xE59C9B55 +522
06F1479E ??     ; 0x9E47F106 +545   ← this question
EA876CF2 iiiiiiii ; 0xF26C87EA +548
```

Calibration on checked-in `#625` hex (INVISIBLE, **not**
the answer): first `06F1479E` is followed by **`01`**.
Same scan on PRESS_START / TEXT / LEGAL / list `#624`
is **`00`**. That only proves the scan; it does not
transfer the value.

A miss returns **0**. That is a helper default, **not** a
file lock. Hits must be **1** per widget (same as other
Press Start flags) before the byte counts.

---

## 2. Why this pass did not dump

`GameBin.Load(frontend.bin, names)` zlib-inflates chunks,
then `FindEntry(name).Raw` is the UI blob. `Dump.csx`
is that load plus `ReadPersistU8` / neighbour CRCs / hit
count / 29-byte window.

`proofs/newgame-plus545/dump-out.txt` is still **absent**.
`read_file` cannot open `frontend.bin` / `names.bin`
(binary). Compressed `frontend.bin` has **no** literal
`06F1479E` (zlib). Name table in `persist-scan.txt`
lists `UI_FRONTEND_BUTTON_NEW_GAME` (`03093163`) and
`UI_ACCEPT_NEW_PROFILE` (`A24F408D`); hex dump stops at
the Press Start tree (`#620`…`#625`).

`FrontendUiDefTests` already parse both entries:

```
accept.Type == 38;  accept.MessageId == 0x126;  accept.Plus224 == 0
newGame.Type == 11; newGame.MessageId == 15;    newGame.Plus224 == 0
```

No `ReadPersistU8(..., 0x9E47F106)`.

Recipe (do not treat as already run):

```
dotnet script proofs/newgame-plus545/Dump.csx
```

Expected lines (fill `??`; INVISIBLE row must stay **1**):

```
UI_FRONTEND_BUTTON_NEW_GAME ... +545=?? hits=1 ...
UI_ACCEPT_NEW_PROFILE       ... +545=?? hits=1 ...
UI_FRONTEND_BUTTON_INVISIBLE ... +545=1 hits=1 ...
```

`hits!=1` → **PARTIAL** (false first-hit). `hits=0` →
helper 0 is **not** a file 0.

---

## Do not invent

- NEW_GAME / ACCEPT `0x9E47F106` = 0.
- NEW_GAME / ACCEPT `0x9E47F106` = 1.
- INVISIBLE **1** as the Main Menu / New Profile value.
- PRESS_START / TEXT / parent-list **0** as the type-11
  child value.
- Helper miss `return 0` as a dumped file byte.
- C# `TryParse` already storing this CRC.
