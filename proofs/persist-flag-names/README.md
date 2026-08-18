# CUIDef persist flag CRCs (005331A0 / 00631C60)

Authority: `Fable.exe` + inflated `frontend.bin` UI blobs (`export/frontend/persist-tail.txt`)
+ PE listing `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00600000.txt`.
No English name is claimed unless `FableCrc(name)` matches a file CRC.

## Persist writer (PROVEN)

`0137DA64` is RTTI `.?AVCUIDef@NUISystem@@`, not the persist vtbl.

CUIDef persist is **`00631C60`**. It writes the `005331A0` bytes with
`0043314A` (u8) / `00431102` (i32) / `00431061` (f32). Helpers call
`00404500` and **skip the 4-byte field CRC** — the CRC is only in the file.

| def | helper | widget copy (`005331A0`) |
| --- | --- | --- |
| +188 | `0043314A` @ `00631D9B` | +302 bit 1 centre |
| +191 | `0043314A` @ `00631D1D` | +300 bit 6 absolute |
| +392 | `0043314A` @ `00632065` | +302 bit 0 clip/skip draw |
| +476 | `0043314A` @ `00632137` | +300 bit 5 |
| +504 | `0043314A` @ `00632161` | +300 bit 7 borrowed-visible |
| +520 | `0043314A` @ `0063218B` | +302 bit 6 remap size |
| +521 | `0043314A` @ `00632199` | +302 bit 7 remap origin |
| +180 | `00431102` @ `00631D7F` | +303 layer |

Order after the style vector (`+64` / `00632E00`):
`+189 u8`, `+190 u8`, `+191 u8`, `+160`, `+148` vec, four f32s,
`+180` Layer i32, `+184` Angle f32, **`+188` Centre u8**, then a long
tail, then `+392`, `+476`, `+504`, `+512`, **`+520`**, **`+521`**.

`0x56A59976` is **not** a nested object. It is style `+120` u8
(value `0` on Press Start). Sequential CRC+i32 dumps desync there.
After remaining `States` records the flag CRCs appear.

## File CRCs (PROVEN in inflated UI)

`frontend.bin` on disk is zlib; search the inflated entry (persist-tail).

| C# constant | CRC | File evidence | First-seen value |
| --- | --- | --- | --- |
| `CentreCrc` | `0x64D3430E` | Aligned CRC after Layer/Angle on TITLE / TITLE_01 / TITLE_02 / FOREST tiles / MOUSE (`persist-tail` `@0287`/`@0295`). u8 next byte. | **0** (all those widgets) |
| `AbsoluteCrc` | `0x38BBD87F` | TITLE nested-hex `7FD8BB38 00`; MOUSE `7FD8BB38 01`; aligned `@0379` on PRESS_START_TEXT. | **0** title/text; **1** `UI_MOUSE_POINTER` |
| `ScaleSizeCrc` | `0xC50CA371` | PRESS_START tail `71 A3 0C C5 01` (dump `@1611` `0xA3710070` is the 4-byte-step desync). | **1** root; **0** listed children |
| `ScaleOriginCrc` | `0xB466D948` | TEXT/SWAP dump `0x66D94800` is `48 D9 66 B4` after a u8. | **0** root and listed children |
| Layer | `0xE338F903` = `FableCrc("Layer")` | Sequential after +184. | TITLE **2**; TITLE_01 **0**; MOUSE **-10** |
| +392 / +476 / +504 | not extracted as aligned CRC in the 4-byte dump | Persist u8 in `00631C60`. First-seen forest tiles draw → +392 is **0**. +476/+504 **UNREAD** as numbers; ctor default 0. Native dest uses root remap size 1 (`UI_TITLE_01` dest `112,48` = `70,30` × 1024/640). |

Prefix / style CRCs (all **in** the sequential prefix; names UNREAD unless noted):

| CRC | Role | First-seen |
| --- | --- | --- |
| `0x0961B216` | i32 after Children (`+76` / `00431020`) | 0 |
| `0xE215EF13` | UTF-16 loc id (`+84`). **Not** `TextTag` (`0x66D9E7F9`) | empty on groups; `TEXT_GUI_MENU_PRESS_BUTTON` on type 6 |
| `0x38BB7ED4` | i32 (`+96` / `00632340`) | 1 |
| `0x6B1015E4` | i32 vec (`+124`) | n=0 |
| `0xF81F10A8` | i32 vec (`+136`) | n=0 |
| `0xF97D3844` | style f32 | 0.2 or −1 |
| `0xA5F8D969` | style i32 | 0 |
| `0x56A59976` | style +120 u8 | 0 |
| `0xF8D265DA` | style +64 i32 | 7 (or 4 on TEXT) |
| `0x2085F2AB` | style +108 i32 vec | n=0 |

Also in that block, **named**: Type, Children, Font, Height, Width, Sprites,
States, GraphicIndex, PositionX/Y, ZoomX (`0xE78E700E`), ZoomY (`0x90894098`),
ColourRGBA.

`+189` CRC `0xBDACBABA` u8=**1** and `+190` CRC `0xAC637D43` u8=**1** on
FOREST (States=0) and on TITLE/MOUSE nested-hex. Names UNREAD.

## Are C# names PROVEN or DISPROVEN?

| Claim | Verdict |
| --- | --- |
| `CentreCrc`/`AbsoluteCrc`/`ScaleSizeCrc`/`ScaleOriginCrc` **values** appear as persist CRCs | **PROVEN** |
| Those constants equal `FableCrc("Centre")` / `"Absolute"` / `"ScaleSize"` / `"ScaleOrigin"` | **DISPROVEN as recovered names** |
| Reason | Tests assert the hex constants and `ReadPersistU8`, **not** `FableCrc("Centre")==CentreCrc`. They **do** assert `FableCrc("ZoomX")==ZoomXCrc`. Comments say **Name UNREAD**. `implementer/frontend/11-transform.md`: `Centre`/`Center`/`Absolute`/`ScaleToScreen`/`ScalePosition` **hits=0** in UI blobs (so those English hashes are not the file CRCs). PE `strings.tsv` has no standalone field strings `Centre`/`Absolute`/`ScaleSize`/`ScaleOrigin`. `names.bin` is instance names; TransformDump already grepped it for those stems. Persist-tail brute of `0x56A59976` printed nothing. |

The hex constants were taken **from the persist stream** (TITLE `@0295`,
MOUSE `7FD8BB38`, PRESS_START tail) and **labelled** from the
`005331A0` bit meaning. The labels are not Lionhead names.

## Proposed parser change (do not apply in this pass)

1. Keep the four CRC constants (file-proven). Keep comments **Name UNREAD**.
   Do not add `FableCrc("Centre")==CentreCrc` asserts.
2. Stop treating `ReadPersistU8` byte-scan as the schema. Walk `00631C60`:
   prefix → `States` style records (`GraphicIndex`…`0x56A59976` u8 +
   `0xF8D265DA` + `0x2085F2AB` vec) → `+189/+190/+191` u8 → … Layer/Angle
   → `+188` u8 → tail → `+392/+476/+504/+520/+521` u8.
3. After `0x56A59976` consume **one byte**, then continue. The current
   `cursor = payload+1` in `TryParse` is correct for that field; the
   persist-tail dumper is not.
4. Parse `+189` (`0xBDACBABA`) and `+190` (`0xAC637D43`) as u8 so
   `+191` `0x38BBD87F` stays aligned (MOUSE absolute=1).
5. Root `+520=1` is real: child dest must use inherited remap size
   (`112,48` not `70,30`). Do not revert that because the English name
   is unread.
