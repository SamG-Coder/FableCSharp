# Persist CRC `0x230364D6` is def `+224`

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `listing-00600000.txt` `00631C60` /
`00632500`; `listing-00540000.txt` `0055B040` / `0055B460` /
`0055B520` / `0055AF60` / `0055AD60` / `0054E280` / `0054E4F0`;
`src/Fable.Formats/Defs/FableCrc.cs`;
`FrontendUiDefTests.Persist_00631C60_plus189_plus190_are_u8_and_font_is_names_offset`;
`implementer/frontend/persist-scan.txt`;
`export/frontend/persist-tail.txt`;
`proofs/messageid-plus228/README.md`;
`proofs/press-start-e5-attach/README.md`;
`proofs/type11-msg15/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **STALE**.

Do not invent an English name for `0x230364D6`.

---

## Verdict

| Claim | Status |
| --- | --- |
| `00631C60` writes def `+224` with `00632500` (CRC skip + i32) | **PROVEN** |
| File CRC of that slot is `0x230364D6` | **PROVEN** (6th `00632500` after `+196` `Action`) |
| `0055B040` first copy is `[def+224]` → box → `vtbl+284` | **PROVEN** |
| Next persist dword is `+228` / CRC `0x53C644E4` → `vtbl+320` | **PROVEN** |
| `0x53C644E4` is def `+224` | **DISPROVEN** |
| Lifecycle ids `15` / `0x126` / `0xE5` sit after `0x53C644E4` | **PROVEN** |
| Those ids also sit after `0x230364D6` | **DISPROVEN** (tests + INVISIBLE hex) |
| Lionhead string for `0x230364D6` | **UNREAD** |
| `FableCrc("Action")` is this CRC | **DISPROVEN** (`Action` is `+196` `0xF1A22807`) |
| `FableCrc("Message")` / `"MessageId"` is `0x53C644E4` | **DISPROVEN** |
| Type-10 action 26 posts `vtbl+284` (`&widget+352`) | **PROVEN** |
| Type-11/38 action 26 posts `vtbl+320` / `0x53C644E4` | **DISPROVEN** |
| Type-11/38 action 26 posts `vtbl+284` list (`widget+372` from `+224`) | **PARTIAL** (call chain **PROVEN**; rdata slot IDs **UNREAD**) |

C# `Plus224Crc` / `MessageIdDefOffset=228` **MATCH** this map.
Host `MessageFromWidgets` still posts `MessageId` (`+228`) on action
26 — **LEFTOVER** vs type-11/38 native.

---

## 1. Writer: `00631C60` `+196`…`+252` (`listing-00600000`)

```
00631F77  lea eax, [esi+196]
00631F80  call 00632500
00631F85  lea ecx, [esi+200]
00631F8E  call 00632500
00631F93  lea edx, [esi+204]
00631F9C  call 00632500
00631FA1  lea eax, [esi+208]
00631FAA  call 00632500
00631FAF  lea ecx, [esi+212]
00631FB8  call 00632500
00631FBD  lea edx, [esi+224]      ; 6th tail i32
00631FC6  call 00632500
00631FCB  lea eax, [esi+228]
00631FD4  call 00632500
00631FD9  lea ecx, [esi+232]
00631FE2  call 00632500
… then +236 +240 +220 +216 +244 +248 +256 +252, same helper
```

`00632500` is the tail i32 helper (`push 0x122D70E` /
`00404500`). File mode 2 **skips the 4-byte field CRC**, then
`00632550` `mov edi,[eax]; add eax,4` into the dest. File form
is **CRC + i32**. The CRC is not a `.text` immediate.

`00431102` is the Layer family, **not** this tail.
`FrontendUiDef.PersistTailDwordFn = 0x00632500`.

---

## 2. File CRCs lock to those dests

`persist-scan.txt` `UI_FRONTEND_BUTTON_INVISIBLE` `#625`:

```
@1089 crc=F1A22807 *Action i32=229 next=-1956356972
```

`-1956356972` is signed `0x8B645C94`. So the first tail CRC is
`Action`, and the next aligned CRC is `0x8B645C94`.

`export/frontend/persist-tail.txt` (same 8-byte grid once
re-aligned on this block):

| File CRC | Writer dest |
| --- | ---: |
| `0xF1A22807` `Action` | `+196` |
| `0x8B645C94` | `+200` |
| `0x0E79EEFC` | `+204` |
| `0x12A56842` | `+208` |
| `0xCB9ADD65` | `+212` |
| **`0x230364D6`** | **`+224`** |
| **`0x53C644E4`** | **`+228`** |
| `0xECEC0A1E` | `+232` |

Hex on the type-10 Press Start root and the type-11 child
(`press-start-e5-attach`):

```
D6640323 00000000   ; 0x230364D6 + i32 0
E444C653 00000000   ; 0x53C644E4 + i32 0     (type 10)
E444C653 E5000000   ; 0x53C644E4 + i32 0xE5  (INVISIBLE)
```

`FrontendUiDefTests` `HasAdjacentPersistI32(Plus224Crc,
MessageIdCrc)` on `UI_ACCEPT_NEW_PROFILE` and
`UI_FRONTEND_BUTTON_NEW_GAME`: the two CRCs are adjacent
CRC+i32 records. That is the file neighbour of writer
`+224` then `+228`.

Older notes that bind `0x53C644E4` to `lea [esi+224]`
(`type38-msg126`, `press-start-action-e5`, C# comments
before `Plus224Crc`) are **STALE**.

---

## 3. `0055B040` first copy is `+224` / `vtbl+284`

```
0055B068  mov ecx, [eax+224]
0055B06E  test ecx, ecx
0055B075  je  0055B15A              ; skip if 0
          box ecx (0042BE50 / 0042AA29)
          cmp [box], 65             ; string extra at def+0x1D8
0055B12E  call [edx+284]
0055B15E  mov eax, [edx+228]
0055B21F  call [edx+320]
0055B24F  mov eax, [edx+232]
0055B310  call [edx+288]
0055B340  mov eax, [edx+236]
0055B401  call [edx+292]
```

Callers: type-34 `0055B4B5` / `0055B515` (vtbl `0124BD2C` live
during the copy); type 11 `0054E0B8`; type 38 `00558B98`.
Type 10 never calls this helper.

Type-34 store `0055B520` (`ret 4`, pair ABI) appends the boxed
id onto **`widget+372`**. Sisters: `0055B640` → `+376`,
`0055B5B0` → `+380`, `0055B6D0` → `+392`. Dtor `0055B760`
frees those four lists. Which sister is `vtbl+284` vs `+320`
is **PARTIAL** (no `.rdata` dump of `0124BD2C+284`). Shape
plus “first copy / first list / first store fn after the ctor”
is how `type11-msg15` assigned `0055B520` = `vtbl+284`.

Type-10 `vtbl+284` is a **different** fn: `0054E4F0` writes
`widget+352/+356`.

---

## 4. Relation of `0x230364D6` to `0x53C644E4`

| | `0x230364D6` | `0x53C644E4` |
| --- | --- | --- |
| Writer | `+224` | `+228` |
| `0055B040` | first, `vtbl+284` | second, `vtbl+320` |
| C# | `Plus224Crc` / `Plus224` | `MessageIdCrc` / `MessageId` |
| ACCEPT | `Plus224 != 0x126` (test) | **`0x126`** |
| NEW_GAME | `Plus224 != 15` (test) | **15** |
| INVISIBLE | **0** | **`0xE5`** |
| PRESS_START type 10 | **0** | **0** |
| Name | **UNREAD** | **UNREAD** |

They are siblings, not aliases. Scanning `0x53C644E4` recovers
the lifecycle ids; that does **not** make it the first
`0055B040` dword.

`Action` `0xF1A22807` (`+196`) often **repeats** the same
integer as `0x53C644E4` (INVISIBLE `0xE5`, NEW_GAME 15). That
is a third slot (type-11 extra `0054DF50` → `+408`), not
`+224`.

---

## 5. `FableCrc` field-name seeds

Hasher: `FableCrc` (`0xEDB88320`, init 0, no xorout). Named
UI CRCs that **do** match (`Type`, `Children`, `Width`,
`Height`, `PositionX`/`Y`, `Font`, `Sprites`, `States`,
`ColourRGBA`, `Layer`, `Angle`, `ZoomX`/`Y`, `GraphicIndex`)
are **not** `0x230364D6`.

| Seed | CRC vs `0x230364D6` | Notes |
| --- | --- | --- |
| `"Action"` | **DISPROVEN** | Test `NotEqual`. File `0xF1A22807` = `+196` |
| `"Message"` | not this CRC | Test vs `MessageIdCrc`; persist-scan would have labelled `0x230364D6` if equal |
| `"MessageId"` | not this CRC | Same |
| `"Event"`, `"OnClick"`, `"OnSelect"`, `"Sound"`, `"ClickSound"` | no dump label | `tools/_frontend/Program.cs` extras / `TransformDump` / `FrontendPersistTailTests.BruteForce` |
| names.bin entries | no `230364D6` row | persist-scan names section |

No seed in those lists is claimed as a hit. **Do not invent a
name.** Keep the C# identifier `Plus224Crc`.

---

## 6. Which dword does action 26 post?

### Type 10 — `vtbl+284` (`widget+352`)

`0054E280` case 0 `0054E2FA`: if `[inner+348] != 0` (widget
`+352`), push `&widget+352` → UI `vtbl+32` `0059A238`.

Ctor zeros `+352`. Persist `+224` / `0x230364D6` is **0** on
`UI_FRONTEND_PRESS_START_MENU`, so even a `0055B040` copy
would skip. Attach `00598EE6` writes packet `0xE5` then slot
`0x14` `vtbl+284` `0054E4F0`. Action 26 posts **that**
`vtbl+284` dword, not `+228`.

### Type 11 / 38 — not `vtbl+320`

`0054DBC0` (11) / `0055AD60` (38) action 26:

```
0055AD7B  mov al, [esi+348]     ; widget+352 gate byte
          je  skip
          call [outer.vtbl+584] ; 0-arg click
          [esi+364] = 1
          call 0055B9D0         ; only action 25 → vtbl+580
```

No `push` of `[def+228]` / `0x53C644E4` / 15 / `0x126`.
`0055B9D0` is not a poster.

Click body `0055AF60` (0-arg, used as `vtbl+584` in
`type11-msg15`; rdata **UNREAD**):

```
push [esi+372]
call [outer.vtbl+524]
```

`+372` is the list `0055B520` fills from the **first**
`0055B040` box (`[def+224]`). So the dword action 26
eventually posts is the **`vtbl+284` / `+224` /
`0x230364D6`** list, **not** `vtbl+320` / `+228` /
`0x53C644E4`.

On ACCEPT / NEW_GAME that payload is **not** `0x126` / 15
(tests). On INVISIBLE it is **0**, so `0055B040` skips
`vtbl+284` and `+372` stays 0. The visible `0xE5` / 15 /
`0x126` values live in **`+228`**. Native action 26 does not
post that sibling.

Action 27 (armed release) also `push [esi+372]` → same
`+224` list, still not `+228`.

### C# leftover

`FrontendInputMap.MessageFromWidgets` treats action 26 as
“first visible type 10/11/38 `MessageId`”. That is `+228`.
Native type 10 posts `vtbl+284`; type 11/38 action 26 posts
the `+224` list (often 0) via `vtbl+584`. Do not “fix” the
host in this pass.

---

## 7. What this pass did not do

- Did not dump `0124BD2C+284` / `+320` / type-11/38 `+584`.
- Did not print numeric `Plus224` on ACCEPT / NEW_GAME (only
  `!= MessageId`).
- Did not recover the Lionhead string for `0x230364D6` or
  `0x53C644E4`.
- Did not walk type-11 `+408` (`Action` / `+196`) as a
  poster.

---

## Proposed (do not apply here)

1. Keep `Plus224Crc = 0x230364D6`, name **UNREAD**.
2. Keep `MessageIdCrc = 0x53C644E4` at def `+228`.
3. Cite `00632500`, not `00431102`, for this pair.
4. Treat “action 26 posts 15 / `0x126`” as **STALE** for
   type 11/38. Those integers are the `+228` file field.
5. Do not hash-assert a Lionhead name for `0x230364D6`.
