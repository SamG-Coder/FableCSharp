# Persist `+212` first-seen vs host: native `005331A0` reads it; host does not

Investigation only. No production `src/` / `tests/` edits.

Question: persist `+212` first-seen vs host. Who reads `+212`?
Host leftover?

Authority: `proofs/persist-plus212/README.md`;
`proofs/plus224-payloads/README.md`;
`proofs/crc-230364D6/README.md`;
`proofs/005331A0-first-site/README.md`;
`Fable.exe` `listing-00500000.txt` `005331A0` / `005334A0` /
`00533720` / `00531E90` / `0052E850` / `00532C5E` / `00532F81`;
`listing-00600000.txt` `00631C60` `lea [esi+212]` / `00631880`;
`listing-00540000.txt` `0055B040`;
`e8.tsv` (`005336F6` / `00533982` → `005331A0`);
inflated `frontend.bin` `implementer/frontend/persist-scan.txt`
(`UI_FRONTEND_PRESS_START_MENU` `#620`,
`UI_PRESS_START_TEXT` `#623`,
`UI_FRONTEND_BUTTON_INVISIBLE` `#625`);
`export/frontend/persist-tail.txt` (`0xCB9ADD65` on the
Press Start root);
`implementer/frontend/fn-005331A0-exact.txt`,
`fn-005334A0-exact.txt`;
`src/Fable.Formats/Defs/FrontendUiDef.cs` (read only);
`src/Fable.Game/FrontendWidgetFactory.cs`,
`src/Fable.Game/IEngineHost.cs` (read only);
`FrontendUiDefTests.Persist_00631C60_plus189_plus190_are_u8_and_font_is_names_offset`
(`MessageId` / `Plus224` only).

Do not re-prove `+224` = `0x230364D6` / `+228` = `0x53C644E4`,
type-10 attach `0xE5`, or action 26 posting those siblings.
Do not invent a Lionhead name for `0xCB9ADD65`.
Do not fold persist `+212` into def `+520` (u8 `ScaleSizeCrc`),
widget list `+224`, or `MessageId`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **MATCH** / **LEFTOVER**.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| Who reads persist def `+212` first-seen? | **`005331A0`** after `call [vtbl+432]` (`mov eax,[ecx+212]`). Only `.text` `E8` sites are type-4 ctor `005336F6` and copy-ctor `00533982`. | **PROVEN** |
| First-seen Press Start payload? | **0** (`65DD9ACB 00000000`). `cmp eax,edi; je 00533237` **skips** the 16-byte box and **`vtbl+520`**. | **PROVEN** |
| Does host parse / copy `+212`? | **No.** `FrontendUiDef` has `Plus224Crc` / `MessageIdCrc` only. `TryParse` does not mention `0xCB9ADD65`. Factory copies `MessageId` / `Plus224`. | **DISPROVEN** (unread) |
| First-seen host vs native for this slot? | Both copy **nothing**. | **MATCH** |
| Host leftover extra field / store? | **No** `Plus212` on `FrontendWidget`. | **DISPROVEN** leftover |
| Host leftover omit of a live first-seen copy? | Native also copies nothing (`je` skip). | **DISPROVEN** leftover |
| ACCEPT / NEW_GAME `+212` i32? | Same writer CRC. Payloads **not** in `persist-scan.txt` / `persist-tail.txt`. Tests never `ReadPersistI32(0xCB9ADD65)`. | **UNREAD** |

---

## Verdict

First-seen persist `+212` (`0xCB9ADD65`) is a **zero skip**.
Native still **reads** the dword in `005331A0` and branches.
Host never reads that CRC. The first-seen **effect** is the
same empty copy, so host is **MATCH**, not leftover.

| Claim | Status |
| --- | --- |
| File CRC of CUIDef `+212` is `0xCB9ADD65` | **PROVEN** (`00631FAF` + Press Start hex) |
| Lionhead string for `0xCB9ADD65` | **UNREAD** |
| `00631C60` writes def `+212` via `00632500` | **PROVEN** |
| `00631880` copies CUIDef `+212` to another def | **PROVEN** (not factory) |
| `005331A0` is the widget persist **reader** | **PROVEN** |
| `0055B040` reads `+212` | **DISPROVEN** (`+224/+228/+232/+236` only) |
| Press Start `#620` / TEXT `#623` / INVISIBLE `#625` i32 | **PROVEN** **0** |
| TITLE / FOREST / MOUSE `0xCB9ADD65` in `persist-tail.txt` | **UNREAD** (aligned CRC not in that dump) |
| ACCEPT / NEW_GAME `+212` i32 | **UNREAD** |
| First-seen `vtbl+520` | **DISPROVEN** as a call |
| `vtbl+520` callee (`0124608C+520`) | **UNREAD** (`.rdata` past `listing-01200000`) |
| `00531E90` is `vtbl+520` | **UNREAD** (stores `arg0` at widget `+212`; no rdata) |
| C# `Plus212Crc` / factory copy | **DISPROVEN** (absent) |
| First-seen host vs native empty copy | **MATCH** |
| Host leftover for this slot | **DISPROVEN** |
| `call [eax+212]` is persist `+212` | **DISPROVEN** (vtbl slot; `0054E35B` list walk) |

| Widget | Type | `0xCB9ADD65` (`+212`) | Host store |
| --- | ---: | ---: | --- |
| `UI_FRONTEND_PRESS_START_MENU` | 10 | **0** | none |
| `UI_PRESS_START_TEXT` | 6 | **0** | none |
| `UI_FRONTEND_BUTTON_INVISIBLE` | 11 | **0** | none |
| `UI_ACCEPT_NEW_PROFILE` | 38 | **UNREAD** | none |
| `UI_FRONTEND_BUTTON_NEW_GAME` | 11 | **UNREAD** | none |

---

## 1. Dump: file `+212` is `0` on the first-seen tree

Writer dest (`listing-00600000`, same as `persist-plus212`):

```
00631FAF  lea ecx, [esi+212]     ; 0xCB9ADD65
00631FB8  call 00632500          ; CRC skip + i32
00631FBD  lea edx, [esi+224]     ; 0x230364D6
```

`persist-scan.txt` hex (LE CRC then i32) on `#620`:

```
65DD9ACB 00000000   ; +212 = 0
D6640323 00000000   ; +224
E444C653 00000000   ; +228
```

Same window on `#623` TEXT and `#625` INVISIBLE (`Action` /
`+228` may be `0xE5`; `+212` stays **0**).
`export/frontend/persist-tail.txt` Press Start root:

```
@0475 0xCB9ADD65 ? i32=0
@0483 0x230364D6 ? i32=0
@0491 0x53C644E4 ? i32=0
```

`persist-scan` name table only stars `*Action` in this tail.
`0xCB9ADD65` has **no** names.bin / `FableCrc` string here.
Keep the identifier **UNREAD**.

`persist-scan.txt` / `persist-tail.txt` do **not** dump
`UI_ACCEPT_NEW_PROFILE` or `UI_FRONTEND_BUTTON_NEW_GAME`.
Do **not** invent those i32s as 0 / 15 / `0x126`.

---

## 2. Native readers of persist def `+212`

### 2.1 `005331A0` — first-seen reader

Type-4 ctor zeros the **widget** slot, then copies from the
**def**:

```
005336E4  mov [esi+212], ebx       ; widget+212 = 0
005336F6  call 005331A0
```

```
005331B6  call [eax+432]           ; def*
005331C0  mov eax, [ecx+212]       ; persist +212
005331C8  cmp eax, edi
005331CA  je  00533237             ; first-seen: taken
          … 00BFEA1A / 0042BE50 / 0042AA29 …
005331F3  mov eax, [edx+212]
00533208  call [edx+520]           ; skipped
```

Press Start factory: `0041D21B` type 10 → `0054E3D0` →
`0052CC50` → `005334A0` → this call, **before** the type-10
vtbl write (`005331A0-first-site`). Zero never reaches
`vtbl+520`. Children that share the type-4 ctor take the
same skip.

`e8.tsv`: only `005336F6` and `00533982` call `005331A0`.

### 2.2 Writer / def copy — not widget factory

```
00631880  mov ecx, [edi+212]
00631886  mov [esi+212], ecx       ; CUIDef copy
00631FAF  lea ecx, [esi+212]       ; persist write
```

`00631880` is the in-memory CUIDef field copy next to
`+196…+228`. It is **not** the first-seen widget ctor.

`0055B040` starts at `[def+224]`. No `[…+212]` there
(`persist-plus212`).

### 2.3 Other `+212` sites — not this persist dword

| Site | What | Class vs persist `+212` |
| --- | --- | --- |
| `00531E90` `mov [ecx+212], eax` | widget store of `arg0`; then `+0xD8` / `00535800` | **UNREAD** as `vtbl+520` |
| `0052E850` `lea eax,[ecx+212]; ret` | widget getter | **not** a persist read |
| `00532C5E` / `00532F81` `mov eax,[ecx+212]; test` | child **widget** `+212` in the `+188` list; 0 → `vtbl+192(0)` | **not** `frontend.bin` |
| `0053395F` | copy-ctor zero of widget `+212` | ctor, not file |
| `0054E35B` `call [eax+212]` | vtbl slot | **DISPROVEN** |
| `00455AC8` / `00430AE9` `lea [esi+212]` | other persist writers (`00431102` / `004310A7`) | **DISPROVEN** as CUIDef `00631C60` |

Widget `+212` is the live dest **if** `vtbl+520` ran.
First-seen it stays the ctor **0**. Those later testers
do not read the file CRC.

---

## 3. Host does not read `0xCB9ADD65`

`FrontendUiDef` constants stop at the next tail pair:

```
Plus224Crc        = 0x230364D6   // dest +224
Plus224DefOffset  = 224
MessageIdCrc      = 0x53C644E4   // dest +228
MessageIdDefOffset= 228
```

No `Plus212Crc`. Sequential `TryParse` skips
`Plus224Crc or MessageIdCrc` as i32 and then treats an
unknown CRC as `Partial` / `UnreadCrcs`. `0xCB9ADD65` is
not in that skip list.

Value extract is only:

```
messageId = ReadPersistI32(raw, MessageIdCrc);
plus224   = ReadPersistI32(raw, Plus224Crc);
```

`FrontendWidgetFactory.Build` stores `MessageId` and
`Plus224`. `FrontendWidget` has those two ints, not a
`+212` analog.

`FrontendUiDefTests` locks ACCEPT `+224=0` `+228=0x126`
and NEW_GAME `+224=0` `+228=15`. It never scans
`0xCB9ADD65`.

Host `MessageId` leftovers (Press Start inject `0xE5`,
action 26 posting factory `MessageId`) are **`+228`**,
not this slot (`plus224-payloads`).

---

## 4. First-seen vs host

| Step | Native | Host |
| --- | --- | --- |
| File i32 on `#620` / TEXT / INVISIBLE | **0** | unread CRC; values unused |
| Ctor widget `+212` | `005336E4` = 0 | no field |
| `005331A0` test | read, `je` skip | no analog |
| `vtbl+520` | **not called** | no analog |
| Observable first-seen store | none | none |

**MATCH** on Press Start. Host is **not** leftover for
this dword: it does not invent a store, and it does not
omit a first-seen write native actually performs.

Later ACCEPT / NEW_GAME still run `005331A0` before
`0055B040`. Whether `vtbl+520` fires there stays
**UNREAD** until those `+212` i32s are dumped.

---

## Do not invent

- A Lionhead name for `0xCB9ADD65`.
- ACCEPT / NEW_GAME `+212` = 0 / 15 / `0x126`.
- `005331A0` reading `+224` / `+228`.
- `0055B040` reading `+212`.
- Host `MessageId` / `Plus224` as this CRC.
- `vtbl+520` = def `+520` u8.
- `00531E90` as `vtbl+520` without `0124608C+520`.
- First-seen `vtbl+520` as a live store.
- Host leftover on a skip that native also takes.
