# Persist `+212` is `0xCB9ADD65`; first-seen `vtbl+520` does not run

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `listing-00500000.txt` `005331A0` /
`005334A0`; `listing-00600000.txt` `00631C60` `lea [esi+212]` /
`00632500`; `listing-00540000.txt` `0055B040` / `0055BA29` /
`0055B4B5`; inflated `frontend.bin`
`implementer/frontend/persist-scan.txt`
(`UI_FRONTEND_PRESS_START_MENU` `#620`,
`UI_PRESS_START_TEXT` `#623`,
`UI_FRONTEND_BUTTON_INVISIBLE` `#625`);
`implementer/frontend/fn-005331A0-exact.txt`;
`proofs/crc-230364D6/README.md`;
`proofs/action-crc-plus196/README.md`;
`proofs/type10-no-0055B040/README.md`;
`proofs/plus224-payloads/README.md`;
`FrontendUiDefTests.Persist_00631C60_plus189_plus190_are_u8_and_font_is_names_offset`.

Do not re-prove `+224` = `0x230364D6` / `+228` = `0x53C644E4`,
type-10 attach `0xE5`, or action 26 posting those siblings.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **STALE**.

Do not invent a Lionhead name for `0xCB9ADD65`.
Do not fold persist `+212` into def `+520` (u8 `ScaleSizeCrc`)
or into widget list `+224`.

---

## Verdict

`005331A0` boxes persist **`[def+212]`** and, if nonzero, calls
**widget `vtbl+520`**. It does **not** read `def+224` / `def+228`.
Those siblings are `0055B040` (`vtbl+284` / `vtbl+320`) on type
11/34/38 only.

File CRC of `+212` is **`0xCB9ADD65`**. Writer dest is
`00631C60` `lea ecx,[esi+212]` → `00632500` (CRC skip + i32),
fifth of the five dwords that start at Action `+196`.

On first-seen Press Start the i32 is **0**. `005331C8`
`je 00533237` therefore **skips** the packet and **`vtbl+520`**.
That is the first-seen use: the copy does not run.

`UI_ACCEPT_NEW_PROFILE` / `UI_FRONTEND_BUTTON_NEW_GAME` share the
same writer CRC. Their `+212` **payloads are not in
`persist-scan.txt`**. Tests scan `+224`/`+228` only. Those i32s
are **UNREAD** here.

| Claim | Status |
| --- | --- |
| `00631C60` writes def `+212` with `00632500` | **PROVEN** `00631FAF` |
| File CRC of that slot is `0xCB9ADD65` | **PROVEN** (5th tail CRC after `Action`) |
| Lionhead string for `0xCB9ADD65` | **UNREAD** |
| `005331A0` copies `[def+212]` through `vtbl+520` | **PROVEN** |
| `005331A0` copies `+224` / `+228` | **DISPROVEN** |
| `0055B040` copies `+212` | **DISPROVEN** (`+224/+228/+232/+236` only) |
| `vtbl+520` is def `+520` (u8 remap size) | **DISPROVEN** |
| Press Start type-10 `+212` i32 | **PROVEN** **0** |
| `UI_PRESS_START_TEXT` / INVISIBLE `+212` i32 | **PROVEN** **0** |
| ACCEPT / NEW_GAME `+212` i32 | **UNREAD** (CRC same; payload not dumped) |
| First-seen `vtbl+520` body | **DISPROVEN** as a call (`je` skip) |
| `vtbl+520` callee VA (`0124608C+520`) | **UNREAD** (no `.rdata` listing) |
| C# parses `0xCB9ADD65` | **DISPROVEN** (unread) |

| Widget | Type | `0xCB9ADD65` (`+212`) | `0x230364D6` (`+224`) | `0x53C644E4` (`+228`) |
| --- | ---: | ---: | ---: | ---: |
| `UI_FRONTEND_PRESS_START_MENU` | 10 | **0** | 0 | 0 |
| `UI_PRESS_START_TEXT` | 6 | **0** | 0 | 0 |
| `UI_FRONTEND_BUTTON_INVISIBLE` | 11 | **0** | 0 | **`0xE5`** |
| `UI_ACCEPT_NEW_PROFILE` | 38 | **UNREAD** | **0** (tests) | **`0x126`** (tests) |
| `UI_FRONTEND_BUTTON_NEW_GAME` | 11 | **UNREAD** | **0** (tests) | **15** (tests) |

---

## 1. Writer: `00631C60` `+212` (`listing-00600000`)

After `+396` / `+400` (`00431102`):

```
00631F77  lea eax, [esi+196]     ; 0xF1A22807 Action
00631F80  call 00632500
00631F85  lea ecx, [esi+200]     ; 0x8B645C94
00631F8E  call 00632500
00631F93  lea edx, [esi+204]     ; 0x0E79EEFC
00631F9C  call 00632500
00631FA1  lea eax, [esi+208]     ; 0x12A56842
00631FAA  call 00632500
00631FAF  lea ecx, [esi+212]     ; 0xCB9ADD65
00631FB8  call 00632500
00631FBD  lea edx, [esi+224]     ; 0x230364D6
00631FC6  call 00632500
00631FCB  lea eax, [esi+228]     ; 0x53C644E4
00631FD4  call 00632500
```

`00632500`: `push 0x122D70E` / `00404500` file mode 2 skips the
4-byte CRC; `00632550` stores the next i32. File form is
**CRC + i32**. The CRC is not a `.text` immediate.

This writer is CUIDef-wide, not type-10-only. ACCEPT / NEW_GAME
use the same dest and therefore the same file CRC.

---

## 2. File CRC lock (`frontend.bin`)

Aligned cluster on `#620` Press Start root
(`persist-scan.txt` hex, little-endian CRC then i32):

```
0728A2F1 00000000   ; +196 Action = 0
945C648B 00000000   ; +200
FCEE790E 00000000   ; +204
4268A512 00000000   ; +208
65DD9ACB 00000000   ; +212 0xCB9ADD65 = 0
D6640323 00000000   ; +224 0x230364D6 = 0
E444C653 00000000   ; +228 0x53C644E4 = 0
```

Same window on `#623` `UI_PRESS_START_TEXT` (Action / `+212` /
`+224` / `+228` all **0**) and on `#625` INVISIBLE:

```
0728A2F1 E5000000   ; Action = 229
945C648B 00000000
FCEE790E 00000000
4268A512 00000000
65DD9ACB 00000000   ; +212 = 0
D6640323 00000000   ; +224 = 0
E444C653 E5000000   ; +228 = 229
```

INVISIBLE proves the slots are independent: Action and
MessageId are `0xE5` while `+212` stays 0.

`persist-scan.txt` does **not** dump `UI_ACCEPT_NEW_PROFILE` or
`UI_FRONTEND_BUTTON_NEW_GAME` (only the Press Start tree plus
TITLE / FOREST / MOUSE). Tests
`ReadPersistI32(0x230364D6)` / `ReadPersistI32(0x53C644E4)` lock
ACCEPT `+224=0` `+228=0x126` and NEW_GAME `+224=0` `+228=15`
(`plus224-payloads`). They never scan `0xCB9ADD65`.

Do **not** invent those two payloads as 0, 15, or `0x126`.

`FableCrc("Action")` is `+196`. Named UI hashes used in
`FrontendUiDef` are **not** `0xCB9ADD65`. Keep the identifier
unread.

---

## 3. `005331A0` copies `+212` via `vtbl+520`, not `+224`/`+228`

Type-4 ctor `005334A0` (`listing-00500000`):

```
005334C6  mov [esi], 0x124608C     ; type-4 vtbl
…
0053361D  mov [esi+224], ebx       ; then list sentinel*
00533633  mov [esi+228], ebx
005336E4  mov [esi+212], ebx       ; widget+212 = 0
005336F6  call 005331A0
```

`005331A0` (`fn-005331A0-exact.txt`):

```
005331B6  call [eax+432]           ; def*
005331C0  mov eax, [ecx+212]       ; persist +212
005331C8  cmp eax, edi
005331CA  je  00533237             ; skip if 0
          alloc 16 / 0042BE50 / 0042AA29
005331F3  mov eax, [edx+212]
005331FD  mov [ecx], eax           ; boxed i32
00533208  call [edx+520]           ; widget vtbl+520
00533237  mov dl, [ecx+60]         ; Type bits, then flags / styles / children
```

Zero `+212` never reaches `vtbl+520`. Nonzero uses the same
16-byte box ABI as `0055B040` (`0042BE50` / `0042AA29`).

`0055B040` (`listing-00540000`) starts at `[def+224]`:

```
0055B068  mov ecx, [eax+224]
0055B075  je  0055B15A
…
0055B12E  call [edx+284]
0055B15E  mov eax, [edx+228]
0055B21F  call [edx+320]
```

No `[…+212]` in that function.

Type 10 never calls `0055B040` (`type10-no-0055B040`). Type 11/38
do, **after** `005331A0`: `0055BA29` `call 0052CC50` (type 4/5
→ `005331A0`) then `0055B4B5` `call 0055B040`.

Call-time vtbl is type-4 `0124608C`. Type 5/10/11/38 overwrite
vtbl **after** `005334A0` returns, so first-seen `vtbl+520` is
not the type-10/11/38 override. Slot dword `0124608C+520` is
**UNREAD** (`listing-01200000` ends before that VA).

Widget `+224`/`+228` in this ctor are the type-4 **list
sentinel**, not persist MessageId. Later
`005331A0` `add ecx, 0x214` / `lea ecx,[ebx+228]` /
`0045BC09` is def `+532`, not persist `+212`.

---

## 4. Do not mix persist `+212` with def `+520`

Same function, later:

```
0053329F  cmp [ecx+520], 0x00      ; persist u8 ScaleSize
005332A8  or  [ebx+302], 0x40
005332AE  mov al, [ecx+521]        ; ScaleOrigin
```

`00631C60` writes those with `0043314A` (`persist-flag-names`).
File CRC `ScaleSizeCrc` `0xC50CA371` is **not** `0xCB9ADD65`.
Press Start root remap-size u8 is **1**; that is **not** the
dword `005331C0` tests.

`FrontendUiDef` comments that bind `005331A0` `def+520` to
`ScaleSizeCrc` describe the **u8** flag. They do **not**
describe `vtbl+520`. C# has no `Plus212Crc`.

`call [eax+212]` at `0054E35B` is a **different** vtbl slot
(list walk), not persist `+212`.

---

## 5. First-seen use

Press Start factory: `0041D21B` type 10 → `0054E3D0` →
`0052CC50` → `005334A0` → `005331A0`.

Root `#620` `+212` = **0** → `je 00533237`. No `00BFEA1A`
packet, no `vtbl+520`. Children (TEXT, INVISIBLE, …) take the
same skip.

So the first-seen frontend tree **never calls** `vtbl+520` for
this dword. The inherited ctor still copies Type bits, flags,
styles, and Children.

Later ACCEPT / NEW_GAME still run `005331A0` before
`0055B040`. Whether `vtbl+520` fires there is **UNREAD** until
those `+212` i32s are scanned.

---

## C# leftover

`FrontendUiDef` stores `Plus224` / `MessageId` only. Factory
does not copy persist `+212`. Native first-seen also copies
nothing (zero skip), so host and native **MATCH** on Press
Start for this slot. They do **not** document ACCEPT / NEW_GAME
`+212`.

Do **not** alias `MessageId` to `0xCB9ADD65`.
Do **not** treat INVISIBLE Action `0xE5` as `+212`.

---

## Do not invent

- Persist `+212` CRC `0x230364D6` / `0x53C644E4` / `Action`.
- `005331A0` reading `+224`/`+228`.
- `0055B040` reading `+212`.
- `vtbl+520` = def `+520` u8.
- Widget `+224` as this persist dword.
- ACCEPT / NEW_GAME `+212` = 0 / 15 / `0x126` without a file scan.
- A Lionhead name for `0xCB9ADD65`.
- First-seen `vtbl+520` as a live store (Press Start skips it).
