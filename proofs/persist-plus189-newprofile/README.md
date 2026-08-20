# CUIDef `+189` / `+190` on NEW_PROFILE / MAIN_MENU / ACCEPT / NEW_GAME

Investigation only. No production `src/` / `tests/` edits.

Question: `proofs/persist-plus189-first` left
`NEW_PROFILE` / `MAIN_MENU` / `ACCEPT` / `NEW_GAME` `+189` /
`+190` **UNREAD** (zlib blobs). Recover first-seen persist
u8s for those widgets from inflated `frontend.bin` /
`persist-tail` / GameInstall data. Who in `.text` reads
def `+189` / `+190` if anyone?

Authority: `implementer/frontend/persist-scan.txt`;
`export/frontend/persist-tail.txt`;
`src/Fable.Formats/Defs/FrontendUiDef.cs`;
`listing-00400000.txt` `0043314A`;
`listing-00540000.txt` (`0054ED90` / `0054F5C0`);
`listing-00600000.txt` (`00631C60` / `0062FE60` /
`006303C0` / `00631720`);
`proofs/persist-plus189-first`; TLC
`C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters\data\CompiledDefs\frontend.bin`
(+ sibling `names.bin`; no `Development/` override).

Do not invent Lionhead names. Do not copy PRESS_START /
INVISIBLE / TITLE hex onto undumped menus.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN**.

---

## Verdict

| Screen / widget | Type | `0xBDACBABA` (`+189`) | `0xAC637D43` (`+190`) |
| --- | ---: | ---: | ---: |
| `UI_FRONTEND_NEW_PROFILE_SCREEN` / `UI_NEW_PROFILE_MENU` | 10 / 12 | **UNREAD** | **UNREAD** |
| `UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` | 10 | **UNREAD** | **UNREAD** |
| `UI_ACCEPT_NEW_PROFILE` | 38 | **UNREAD** | **UNREAD** |
| `UI_FRONTEND_BUTTON_NEW_GAME` | 11 | **UNREAD** | **UNREAD** |
| `UI_TEXT_NEW_GAME` / New Profile title text | 6 | **UNREAD** | **UNREAD** |

Those five rows are the same **UNREAD** lock as
`persist-plus189-first`. This pass did **not** change them.

| Claim | Status |
| --- | --- |
| File CRCs `0xBDACBABA` / `0xAC637D43` are dest `CUIDef+189` / `+190` u8 (`0043314A`) | **PROVEN** |
| Names for those CRCs | **UNREAD** |
| TLC `frontend.bin` + `names.bin` exist; tests `FindEntry` these names | **PROVEN** |
| Checked-in hex covers these widgets | **DISPROVEN** (`persist-scan` name table / `#201` `#216` `#261` sketch only; no `hex:`) |
| `persist-tail.txt` covers these widgets | **DISPROVEN** (Press Start / TITLE / FOREST / MOUSE only) |
| This pass inflated those `entry.Raw` blobs | **UNREAD** (`read_file` rejects `.bin`; no `dump-out.txt`) |
| Tests `ReadPersistU8(raw, Plus189Crc)` / `Plus190Crc` on these entries | **DISPROVEN** |
| File bytes are PRESS_START `1`/`1` or type-6 `1`/`0` | **DISPROVEN** (method) |
| `005331A0` reads `def+189` / `def+190` | **DISPROVEN** |
| Type 10 / 11 / 38 apply load those dest u8s | **DISPROVEN** in `.text` |
| Only widget-side **def** load is type-6 `0054ED90` | **PROVEN** |

**Answer:** file u8s on these four screens stay **UNREAD**.
Lock remains `ReadPersistU8(raw, 0xBDACBABA)` /
`ReadPersistU8(raw, 0xAC637D43)` on the inflated entries.
`.text` load of the dest u8s is **`0054ED90`**
(`[eax+190]` @ `0054EE86`, `[eax+189]` @ `0054EE94`).
Type-6 ctor `0054F5C0` calls it. Persist write is
`00631C60`. Type 10 / 11 / 38 persist the fields and
nothing consumes them.

---

## 1. Why the blobs stayed UNREAD (PROVEN as a gap)

`frontend.bin` on disk is GameBin zlib-1 chunks
(`GameBin.cs` `InflateZlib`). Grep of the packed file does
not see `BABAACBD`. `read_file` refuses the `.bin`.

Checked-in inflated hex:

- `persist-scan.txt` dumps start at
  `UI_FRONTEND_PRESS_START_MENU` `#620` and walk that tree
  (TITLE / FOREST / `UI_PRESS_START_TEXT` `#623` / MOUSE).
  Name table lists `UI_FRONTEND_NEW_PROFILE_SCREEN`
  `7AFC8A56`, `UI_NEW_PROFILE_MENU` `D18CAE8B`,
  `UI_ACCEPT_NEW_PROFILE` `A24F408D`,
  `UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE`
  `AD0A46BB`, `UI_FRONTEND_BUTTON_NEW_GAME` `03093163`,
  `UI_TEXT_NEW_GAME` `749AE688`. **No** `=== … hex:` for
  any of those. No `#201` / `#216` / `#261` hex block.
- `persist-tail.txt` is `FrontendPersistTailTests`’ fixed
  Press Start / TITLE / FOREST / MOUSE list. The only
  aligned `0xBDACBABA` rows are PRESS_START `@0995` and
  FOREST `@0107` (both u8=1).
- `01-widget-construction.md` sketches `#201` type 10,
  `#216` type 10, `#261` type 6 (`TEXT_GUI_MENU_NEW_GAME`).
  Types / children only. No CRC pair.

`FrontendUiDefTests` **does** `FindEntry` /
`TryParse` `UI_ACCEPT_NEW_PROFILE` (type 38, MessageId
`0x126`), `UI_FRONTEND_BUTTON_NEW_GAME` (type 11,
MessageId 15), `UI_TEXT_NEW_GAME` (type 6,
`TEXT_GUI_MENU_NEW_GAME`), and factory-walks
`UI_FRONTEND_NEW_PROFILE_SCREEN` /
`UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE`.
`TryParse` skips `Plus189Crc` / `Plus190Crc` as one-byte
payloads and **does not store** them. No
`ReadPersistU8(..., Plus189Crc)` on these raws.

Same inflate miss as `proofs/newgame-plus545-dump` /
`proofs/accept-newgame-plus545` for `+545`.

Do **not** treat ctor default (`0062FE60`
`mov [esi+189], al` with `eax=1`; heap-zero `+190`) as
the file byte. Persist always rewrites both via
`0043314A`.

---

## 2. Writer + helper (PROVEN; same as first-seen)

`listing-00400000.txt` `0043314A`: skip field CRC
(`00404500` + empty `0x122D70E`), then mode 2
`00403EB0` (one dest byte). File form is CRC then u8:

```
BA BA AC BD  vv     ; 0xBDACBABA +189
43 7D 63 AC  ww     ; 0xAC637D43 +190
```

`00631C60` after style vector `00632E00`:

```
00631D01  lea eax, [esi+189]
00631D0A  call 0043314A
00631D0F  lea ecx, [esi+190]
00631D18  call 0043314A
00631D1D  lea edx, [esi+191]    ; AbsoluteCrc
```

Copy `00631720` copies both. Copy `006303C0` copies
`+189` and `+191` and **skips** `+190`.

---

## 3. Who in `.text` reads def `+189` / `+190`

CUIDef-offset **loads** in `.text`:

| VA | What |
| --- | --- |
| `00631D01` / `00631D0F` | persist **write** dest |
| `00631820` / `0063182C` | copy both (`00631720`) |
| `006305B1` | copy `+189` only (`006303C0`) |
| **`0054EE86` / `0054EE94`** | **`0054ED90` type-6 load** |

`listing-00540000.txt` has **only** those two
`[reg+189]` / `[reg+190]` sites. After `vtbl+432` the
def is in `eax`:

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

Type-6 ctor `0054F5C0` (`vtbl` `01249CCC`)
`call 0054ED90` at `0054F630`. Other `0054ED90` sites
in the same listing: `0054F585`, `0054F6C1` (same
type-6 family, not type 10 / 11 / 38).

`005331A0` maps `+188` / `+191` / `+392` / `+476` /
`+504` / `+520` / `+521` / `+180`. No `[ecx+189]` /
`[ecx+190]`.

No `[reg+189]` / `[reg+190]` on type 10 / 11 / 38 apply
(`0054E3D0` / `0054E0B0` / `00558B90` / `0055AD60`).
`00A1xxxx` / `00B3xxxx` / `00F1xxxx` `+189`/`+190`
sites are other objects (not `01259F8C` / not
`0054ED90`).

So ACCEPT / NEW_GAME / MAIN_MENU / NEW_PROFILE **roots**
(and the type-11 / type-38 inners) persist the fields
and **nothing in `.text` consumes them** on those types.
The first consumer on a screen is the first type-6
child, if any:

- New Profile: `UI_TEXT_NEW_PROFILE_MENU_TITLE` type 6
  (`FrontendUiDefTests` factory walk). File u8s
  **UNREAD**.
- Main Menu: `UI_TEXT_NEW_GAME` type 6 (`#261`). File
  u8s **UNREAD**.

Callee of type-6 `vtbl+576` that takes `+189` stays
**UNREAD** as a slot VA (call site **PROVEN**).

---

## 4. What is still UNREAD

Return of the table rows that stay **UNREAD**:

| Screen / widget | Type | `+189` | `+190` |
| --- | ---: | ---: | ---: |
| `UI_FRONTEND_NEW_PROFILE_SCREEN` / `UI_NEW_PROFILE_MENU` | 10 / 12 | **UNREAD** | **UNREAD** |
| `UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` | 10 | **UNREAD** | **UNREAD** |
| `UI_ACCEPT_NEW_PROFILE` | 38 | **UNREAD** | **UNREAD** |
| `UI_FRONTEND_BUTTON_NEW_GAME` | 11 | **UNREAD** | **UNREAD** |
| `UI_TEXT_NEW_GAME` / New Profile title text | 6 | **UNREAD** | **UNREAD** |

Recipe (not run this pass): `GameBin.Load` TLC
`frontend.bin`, then `ReadPersistU8(entry.Raw, 0xBDACBABA)`
/ `0xAC637D43` on those names. Neighbour after the pair
is `AbsoluteCrc` `0x38BBD87F` (`7FD8BB38`).
