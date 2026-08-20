# CUIDef `+189` / `+190` first-seen persist u8s

Investigation only. No production `src/` edits.

Question: `FrontendUiDef.Plus189Crc` `0xBDACBABA` and
`Plus190Crc` `0xAC637D43`. First-seen persist values on
PRESS_START / NEW_PROFILE / MAIN_MENU / ACCEPT / NEW_GAME.
Who in `.text` reads those u8s?

Authority: `Fable.exe` `listing-00600000.txt` (`00631C60` /
`0062FE60` / `006303C0` / `00631720` / `0043314A`);
`listing-00540000.txt` (`0054ED90` / `0054F5C0` / `0054EF00` /
`005331A0`); inflated `frontend.bin` hex in
`implementer/frontend/persist-scan.txt` and sequential
`export/frontend/persist-tail.txt`;
`src/Fable.Formats/Defs/FrontendUiDef.cs`;
`proofs/persist-flag-names/README.md`.

Do not invent Lionhead names. Do not copy PRESS_START /
INVISIBLE bytes onto undumped menus.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

---

## Verdict

| Screen / widget | Type | `0xBDACBABA` (`+189`) | `0xAC637D43` (`+190`) |
| --- | ---: | ---: | ---: |
| `UI_FRONTEND_PRESS_START_MENU` | 10 | **1** | **1** |
| `UI_PRESS_START_TEXT` (first type 6) | 6 | **1** | **0** |
| `UI_LEGAL_TEXT` | 6 | **1** | **0** |
| `UI_FRONTEND_LIST_PRESS_START_MENU` | 12 | **1** | **1** |
| `UI_FRONTEND_BUTTON_INVISIBLE` | 11 | **1** | **1** |
| TITLE / FOREST / SWAP / MOUSE (Press Start tree) | 5 / 18 / 32 | **1** | **1** |
| `UI_FRONTEND_NEW_PROFILE_SCREEN` / `UI_NEW_PROFILE_MENU` | 10 / 12 | **UNREAD** | **UNREAD** |
| `UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` | 10 | **UNREAD** | **UNREAD** |
| `UI_ACCEPT_NEW_PROFILE` | 38 | **UNREAD** | **UNREAD** |
| `UI_FRONTEND_BUTTON_NEW_GAME` | 11 | **UNREAD** | **UNREAD** |
| `UI_TEXT_NEW_GAME` / New Profile title text | 6 | **UNREAD** | **UNREAD** |

| Claim | Status |
| --- | --- |
| File CRCs `0xBDACBABA` / `0xAC637D43` are dest `CUIDef+189` / `+190` u8 (`0043314A`) | **PROVEN** |
| Names for those CRCs | **UNREAD** |
| PRESS_START root first-seen is `1` / `1` | **PROVEN** (`#620` `BABAACBD 01 437D63AC 01`) |
| First type-6 on that screen is `1` / `0` | **PROVEN** (`#623`) |
| NEW_PROFILE / MAIN_MENU / ACCEPT / NEW_GAME file bytes | **UNREAD** (no hex; blobs zlib) |
| INVISIBLE `1`/`1` is NEW_GAME’s file value | **DISPROVEN** as a lock (same type only) |
| `005331A0` reads `def+189` / `def+190` | **DISPROVEN** |
| Type 10 / 11 / 38 apply reads those dest u8s | **DISPROVEN** in `.text` |
| Only widget-side load is type-6 `0054ED90` | **PROVEN** |

**Answer:** first-seen on PRESS_START is **`1` / `1`** (root).
The first widget that **reads** the bytes is type-6
`UI_PRESS_START_TEXT` = **`1` / `0`**. The other four
screens stay **UNREAD**. `.text` load of the dest u8s is
**`0054ED90`** (`[eax+189]` / `[eax+190]`). Persist write
is `00631C60`. Shared ctor `005331A0` does not touch them.

---

## 1. Writer (PROVEN)

`00631C60` after style vector `00632E00`:

```
00631D01  lea eax, [esi+189]
00631D0A  call 0043314A
00631D0F  lea ecx, [esi+190]
00631D18  call 0043314A
00631D1D  lea edx, [esi+191]    ; AbsoluteCrc 0x38BBD87F
```

`0043314A` skips the 4-byte field CRC (`00404500`) then
reads one byte. File form is CRC then u8:

```
BA BA AC BD  vv     ; 0xBDACBABA + u8
43 7D 63 AC  ww     ; 0xAC637D43 + u8
```

Default ctor `0062FE60` (`vtbl` `01259F8C`):
`mov [esi+189], al` with `eax=1`. No store to `+190`
(heap zero → 0 unless persist writes).

Copy `00631720` copies both bytes. Copy `006303C0` copies
`+189` and `+191` and **skips** `+190`.

---

## 2. PRESS_START file bytes (PROVEN)

`persist-scan.txt` inflated hex. Pattern after last style
record:

| Instance | Hex after CRC pair | u8s |
| --- | --- | --- |
| `#620` `UI_FRONTEND_PRESS_START_MENU` | `BABAACBD01437D63AC017FD8BB38` | **1, 1** |
| `#623` `UI_PRESS_START_TEXT` | `…AC00 7FD8BB38` | **1, 0** |
| `#621` `UI_LEGAL_TEXT` | `…AC00 7FD8BB38` | **1, 0** |
| `#624` list / `#625` INVISIBLE / TITLE / FOREST / SWAP / MOUSE | `…AC01 7FD8BB38` | **1, 1** |

`persist-tail.txt` `@0995` on the type-10 root is the same
CRC; payload u8=1; next i32 `1669153537` = `0x637D4301` =
`01 43 7D 63` (value + first three of `+190`).

`FrontendPersistTailTests` never opens NEW_PROFILE /
MAIN_MENU / ACCEPT / NEW_GAME. `frontend.bin` on disk is
zlib; those entries have **no** checked-in hex.
`ReadPersistU8` is not asserted on `Plus189Crc` /
`Plus190Crc`.

---

## 3. Who reads the dest u8s (PROVEN)

`005331A0` maps `+188` / `+191` / `+392` / `+476` / `+504` /
`+520` / `+521` / `+180`. **No** `[ecx+189]` / `[ecx+190]`.

CUIDef-offset loads in `.text`:

| VA | What |
| --- | --- |
| `00631D01` / `00631D0F` | persist **write** dest |
| `00631820` / `0063182C` | copy both (`00631720`) |
| `006305B1` | copy `+189` only (`006303C0`) |
| **`0054EE86` / `0054EE94`** | **`0054ED90` type-6 load** |

`0054F5C0` (type 6, `vtbl` `01249CCC`) calls `0054ED90`.
After `vtbl+432` the def is in `eax`:

```
0054EE86  mov dl, [eax+190]
0054EE8E  mov [esi+392], dl          ; widget+392
0054EE94  mov cl, [eax+189]
0054EE9C  add eax, 84                ; persist text (UTF-16)
0054EE9F  push ecx                   ; +189
0054EEA0  push 1
0054EEA2  push eax
0054EEA5  call [edx+576]
```

`0054EF00` later `cmp [esi+392], bl` — that is the
**copied** `+190`, not another def load.

No `[reg+189]` / `[reg+190]` on type 10 / 11 / 38 apply
(`0054E3D0` / `0054E0B0` / `00558B90` / `0055AD60`).
`00A1xxxx` / `00B3xxxx` `+189`/`+190` sites are other
objects (not `01259F8C` / not `0054ED90`).

So ACCEPT / NEW_GAME / MAIN_MENU **roots** persist the
fields and **nothing in `.text` consumes them** on those
types. First consumer on a screen is the first type-6
child, if any.

---

## 4. What is still UNREAD

- Lionhead names (`FableCrc` of guessed English ≠ these CRCs).
- File u8s on NEW_PROFILE / MAIN_MENU / ACCEPT / NEW_GAME
  (and `UI_TEXT_NEW_GAME`). Lock:
  `ReadPersistU8(raw, 0xBDACBABA)` /
  `ReadPersistU8(raw, 0xAC637D43)` on those entries.
- Callee of type-6 `vtbl+576` that takes `+189` (slot
  address **UNREAD** this pass; call site **PROVEN**).
