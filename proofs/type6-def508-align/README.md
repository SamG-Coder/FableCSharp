# Type-6 ctor `0054ED90` `[def+508]` → widget `+302` bits `0x08`/`0x10`/`0x20`

Investigation only. No production `src/` edits.

Authority: `Fable.exe` complete `.text` dump
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0054ED90` / `0054F5C0` / `0054EF00` / `0054FFF0`),
`listing-00600000.txt` (`00631C60` / `006325E0` / `00632630` /
copy `00631BBA`), `listing-00500000.txt` (`005331A0`);
inflated `frontend.bin` `implementer/frontend/persist-scan.txt`
`#623` `UI_PRESS_START_TEXT`;
`src/Fable.Formats/Defs/FrontendUiDef.cs` (whether C# parses
the slot — not persist authority).

Do not re-prove dest `512,384` remap, Font `26051` →
`ENG_ARIAL_24`, Action `0xF1A22807`, `+224`/`+228`,
or `+545`. Do not start Oakvale / `S_QNOVI`.
Do not invent a Lionhead name for the file CRC.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**STALE**.

---

## Verdict

| Claim | Status |
| --- | --- |
| Type-6 ctor `0054F5C0` calls `0054ED90` | **PROVEN** `0054F630` |
| `0054ED90` reads **`[def+508]`** (def from `vtbl+432`) | **PROVEN** `0054EEAB` |
| `0` → `or [widget+302], 0x08`; `1` → `0x10`; `2` → `0x20`; else no OR | **PROVEN** `0054EEB5`…`0054EEDC` |
| `0054FFF0` (`vtbl+600`) bit4 → centre `1`, bit5 → right `2` | **PROVEN** |
| `005331A0` writes `+302` bits 4/5 | **DISPROVEN** (bits `0x01`/`0x02`/`0x40`/`0x80` only) |
| `00631C60` persist dest of `+508` is `006325E0` (CRC skip + **4-byte dword**) | **PROVEN** `0063216F` |
| File CRC of that dword | **PROVEN** **`0x02F094DB`** |
| Lionhead string for `0x02F094DB` | **UNREAD** |
| First-seen `UI_PRESS_START_TEXT` persist `+508` | **PROVEN** **`1`** |
| First-seen type-6 therefore ORs **`+302` bit `0x10` (centre)** | **PROVEN** |
| Sequential persist-tail dword at `@0971` is that CRC | **DISPROVEN** (desync after `0x56A59976`) |
| Prior “first-seen left **if** `def+508==0` (unverified)” | **STALE** (file is **1**, not 0) |
| `FrontendUiDef` parses `+508` / `0x02F094DB` | **DISPROVEN** |
| First-seen glyph pen X equals left because `+204==0` | **PARTIAL** (`0054EF00` centre uses `fmul [esi+204]`; GraphicIndex 0 → leftover 0; not required to answer the persist value) |

**Answer:** first-seen `UI_PRESS_START_TEXT` `[CUIDef+508]` is
**`1`**. There **is** a persist CRC: **`0x02F094DB`**.
`FrontendUiDef` does **not** parse it.

---

## 1. Type-6 ctor reads `[def+508]` (`listing-00540000`)

`FrontendWidgetType.TextCtor` = `0054F5C0`. After
`0052CC50` (type-4/5 base, includes `005331A0`):

```
0054F5C0  …
0054F5CA  call 0052CC50
          mov [esi], 0x1249CCC
…
0054F630  call 0054ED90
```

Copy ctor `0054F650` also `call 0054ED90` at `0054F6C1`.
Helper `0054F580` at `0054F585`.

`0054ED90` (esi = widget):

```
0054EDA1  call [eax+432]           ; CUIDef*
0054EEAB  mov ecx, [esp+12]
0054EEAF  mov eax, [ecx+508]
0054EEB5  sub eax, 0
0054EEB8  je  0054EED4             ; 0
0054EEBA  dec eax
0054EEBB  je  0054EECA             ; 1
0054EEBD  dec eax
0054EEBE  jne 0054EEE2             ; not 2 → skip
0054EEC0  mov al, [esi+302]
0054EEC6  or  al, 0x20             ; 2
0054EEC8  jmp 0054EEDC
0054EECA  mov al, [esi+302]
0054EED0  or  al, 0x10             ; 1
0054EED2  jmp 0054EEDC
0054EED4  mov al, [esi+302]
0054EEDA  or  al, 0x08             ; 0
0054EEDC  mov [esi+302], al
```

Same three bits as `0054FFF0`:

```
0054FFF0  mov al, [ecx+302]
          test al, 0x10
          je  00550000
          mov eax, 1               ; centre
          ret
00550000  movzx eax, al
          shr eax, 4
          and eax, 2               ; bit5 → 2 right, else 0 left
          ret
```

Draw `0054EF00` `call [edx+600]` then `dec eax` /
`je` centre / `dec eax` / `jne` skip (right). Centre/right
only `fmul [esi+204]`.

`005331A0` (`listing-00500000`): `or [ebx+302], 0x01`
(`+392`), `0x02` (`+188`), `al=0x40` (`+520`), `dl=0x80`
(`+521`). **No** `0x08`/`0x10`/`0x20`. Type-6 align bits
are **not** the Centre persist u8 (`CentreCrc` `+188`).

---

## 2. Persist writer: CRC + dword (`listing-00600000`)

CUIDef persist `00631C60` immediately after `+504` u8
(`0043314A`) and before `+512` u8:

```
00632161  lea edx, [esi+504]
0063216A  call 0043314A            ; u8
0063216F  lea eax, [esi+508]
00632178  call 006325E0            ; dword
0063217D  lea ecx, [esi+512]
00632186  call 0043314A            ; u8
0063218B  lea edx, [esi+520]
00632194  call 0043314A            ; ScaleSize
00632199  lea eax, [esi+521]
006321A2  call 0043314A            ; ScaleOrigin
006321A7  lea ecx, [esi+516]
006321B0  call 00431102            ; i32
```

Copy ctor stores a **dword**, not a byte:

```
00631BBA  mov ecx, [edi+508]
00631BC0  mov [esi+508], ecx
```

`006325E0` is the same intern as `00632500` (`push 0x122D70E`
/ `00404500`). File mode 2 (`[ctx+24]==2`) calls
`00632630`: remaining `>= 4`, `mov edi, [eax]`, store
`[dest]`. File form is **CRC (skipped) + 4-byte payload**.
CRC is **not** a `.text` immediate.

`+520`/`+521`/`+516` CRCs are already locked in
`proofs/cuidef-plus545` (`0xC50CA371` / `0xB466D948` /
`0x180E20C5`). That pins the three fields **before** them.

---

## 3. File CRC and first-seen value (`persist-scan.txt` `#623`)

Writer order on the inflated blob, little-endian CRC then
payload. Window immediately before the locked ScaleSize
cluster on `UI_PRESS_START_TEXT` (`raw=1080`; same hex at
the top-level `#623` dump and the nested copy under
`UI_PRESS_START_SWAP`):

```
8E6CB02C 00           ; +504 CRC 0x2CB06C8E  u8 = 0
DB94F002 01000000     ; +508 CRC 0x02F094DB  i32 = 1
DDE28470 00           ; +512 CRC 0x7084E2DD  u8 = 0
71A30CC5 00           ; +520 CRC 0xC50CA371  u8 = 0
48D966B4 00           ; +521 CRC 0xB466D948  u8 = 0
C5200E18 03000000     ; +516 CRC 0x180E20C5  i32 = 3
```

Same `0x02F094DB` appears on every dumped Press Start UI
blob. Payload **1** on first-seen type-6
`UI_PRESS_START_TEXT` and `UI_LEGAL_TEXT`
(`DB94F00201000000`). TITLE / FOREST / SWAP / MOUSE /
INVISIBLE in that dump use `DB94F00200000000` (**0**).
Those non-type-6 zeros are not the question.

`export/frontend/persist-tail.txt` walks CRC+i32 and
desyncs at style `+120` `0x56A59976`. `@0971 0x0102F094`
is **not** the field CRC. Use the hex walk above.

No `FableCrc` string is claimed. `FrontendPersistTailTests`
seeds (`Justify` / `HAlign` / `VAlign` / …) are not this
CRC. Keep the English name **UNREAD**.

---

## 4. `FrontendUiDef` does not parse it

`FrontendUiDef.cs` has no `0x02F094DB`, no `+508` property,
no `ReadPersistI32` of this CRC. `TryParse` sequential walk
breaks on the first unknown CRC (`partial = true`) well
before the `+504` tail. The second pass only loads
`CentreCrc` / `AbsoluteCrc` / `ScaleSizeCrc` /
`ScaleOriginCrc` / `MessageIdCrc` / `Plus224Crc`.

`tools/_frontend/Program.cs` `ReadI32("+508")` is a dump
walker, not `FrontendUiDef`.

C# `CollectFrontendRecords` still passes
`FrontendTextDraw.AlignLeft`. Native first-seen ORs
**centre**. That leftover is **not** “file is 0”.

---

## Classification of older notes

| Note | Now |
| --- | --- |
| `proofs/glyph-uv-gaps` §3 “dump `CUIDef+508`”; UNREAD persist | **STALE** as UNREAD — value **1**, CRC **`0x02F094DB`** |
| `proofs/audit-frontend-leftover` “PRESS_START_TEXT is left **if** `def+508==0` (unverified in C#)” | **STALE** condition — file is **1** |
| Same audit: `FrontendUiDef` does not parse `+508` | **PROVEN** (still) |
| Same audit: `+504` CRC `0x2CB06C8E` is persist **u8**, not the type-6 dword | **PROVEN** (adjacent slot) |
