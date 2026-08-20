# `0xF1A22807` `FableCrc("Action")` is persist `+196`, not the action-26 post

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `00631C60` / `00632500` /
`0055B040` / `0054DF50` / `0054E0B0` / `0054DBC0` /
`0055AD60` / `0055AF60` / `00558E10` / `00558EC0`
(`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00600000.txt`,
`listing-00540000.txt`);
`frontend.bin` PRESS_START walk
(`implementer/frontend/persist-scan.txt`);
`src/Fable.Formats/Defs/FrontendUiDef.cs`
(`Plus224Crc`, `MessageIdCrc`, `MessageIdDefOffset=228`);
`proofs/messageid-plus228/README.md`;
`proofs/0055B9D0-post-dword/README.md`;
`proofs/press-start-action-e5/README.md` (offset **STALE**);
`proofs/who-posts-15/README.md`;
`proofs/press-start-e5-attach/README.md`;
`tests/Fable.Formats.Tests/FrontendUiDefTests.cs`.

Do not re-prove type 4 → action 26, Return ≠ `0xE5`,
type-10 attach `00598EE6` → `+352`, or
`0x53C644E4` → `+228` MessageId.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **STALE**.

---

## Verdict

`0xF1A22807` is `FableCrc("Action")`. Persist writer
`00631C60` loads it with `00632500` into CUIDef **`+196`**.
On `UI_FRONTEND_BUTTON_NEW_GAME` the file i32 is **15**,
same number as MessageId CRC `0x53C644E4` at **`+228`**.
That is a **duplicate file dword**, not the same dest slot.

**Action 26 does not read `+196`.** Type 11/38 apply
(`0054DBC0` / `0055AD60`) arms, then `0055AF60` posts
`widget+372` (the `0055B040` **first** copy,
`[def+224]` / vtbl+284). `0055B040` never loads `+196`.

Type 11 ctor `0054DF50` **does** box `[def+196]` into
vector **`+408`**. First-seen Press Start / Main Menu
apply recovered here never walks that vector. Dtor
frees it. C# does not parse `0xF1A22807`.

First-seen Press Start Action `0xE5` (229) is
**`UI_FRONTEND_BUTTON_INVISIBLE`** type 11, **not**
`UI_PRESS_START_TEXT`. TEXT Action is **0**. The
INVISIBLE copy is **not** the Press Start `0xE5`
message (`00598EE6` / type-10 `+352`).

| Claim | Status |
| --- | --- |
| `0xF1A22807` = `FableCrc("Action")` | **PROVEN** persist-scan `*Action` (785 hits) |
| Writer dest of that CRC is def **`+196`** | **PROVEN** `00631F77` first `00632500` + file order |
| Same CRC is dest `+228` | **DISPROVEN** (`+228` is `0x53C644E4`) |
| `UI_FRONTEND_BUTTON_NEW_GAME` Action i32 = **15** | **PROVEN** (`who-posts-15`) |
| NEW_GAME `+228` MessageId = **15** (same number) | **PROVEN** tests / `messageid-plus228` |
| `0055B040` copies `+196` / Action vtbl | **DISPROVEN** (`+224/+228/+232/+236` only) |
| Action **26** reads `+196` or type-11 `+408` | **DISPROVEN** |
| Actions 27–32 on `0055AD60` read `+196` / `+408` | **DISPROVEN** (recovered cases) |
| Type 11 ctor copies nonzero `+196` → `+408` | **PROVEN** `0054DF50` |
| Type 36 ctor copies `+196` via vtbl+264 | **PROVEN** `00558E10`; **not** first-seen |
| `UI_PRESS_START_TEXT` Action = `0xE5` | **DISPROVEN** (`@0705 i32=0`) |
| `UI_FRONTEND_BUTTON_INVISIBLE` Action = **229 / `0xE5`** | **PROVEN** `@1089` |
| INVISIBLE Action is the Press Start `0xE5` post | **DISPROVEN** (attach type-10 `+352`) |
| C# `MessageId` / action 26 uses `0xF1A22807` | **DISPROVEN** (unread) |

---

## 1. Writer: `00631C60` `+196` is Action

After `+396` / `+400` (`00431102`):

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
00631FBD  lea edx, [esi+224]     ; 0x230364D6 Plus224Crc
00631FC6  call 00632500
00631FCB  lea eax, [esi+228]     ; 0x53C644E4 MessageIdCrc
00631FD4  call 00632500
```

`00632500` skips the 4-byte field CRC (`00404500`) and
stores the next i32. File form is **CRC + i32**. The
first of those five dwords is Action.

`UI_FRONTEND_BUTTON_INVISIBLE` `#625` hex tail
(`persist-scan.txt`):

```
0728A2F1 E5000000   ; 0xF1A22807 Action = 229
945C648B 00000000   ; +200
FCEE790E 00000000   ; +204
4268A512 00000000   ; +208
65DD9ACB 00000000   ; +212
D6640323 00000000   ; 0x230364D6 +224 = 0
E444C653 E5000000   ; 0x53C644E4 +228 = 229
```

That order **is** the writer dest order. `press-start-action-e5`
and `who-posts-0x126` pairing Action with `lea [esi+228]` is
**STALE**. Current host: `MessageIdDefOffset=228`,
`Plus224DefOffset=224` (`messageid-plus228`).

`FrontendUiDefTests` asserts
`FableCrc.Hash("Action") != Plus224Crc`. It does **not**
store Action. `audit-lifecycle-input`: “second file 15 after
`0xF1A22807` is unused in C#.”

---

## 2. `0055B040` copy order (not Action)

Type 34/11/38 ctor path (`0055B460` → `0055B040`):

| Def | Test / box | Store |
| --- | --- | --- |
| `+224` | `0055B068` | vtbl+284 → list `widget+372` |
| `+228` | `0055B15E` | vtbl+320 |
| `+232` | `0055B24F` | vtbl+288 |
| `+236` | `0055B340` | vtbl+292 |

No `mov ecx,[eax+196]` in this function. Zero skips that arm.

Action **26** click (`0055AD7B` → outer vtbl+584 `0055AF60`)
pushes **`[this+372]`** then vtbl+524
(`0055B9D0-post-dword`). That list is the `+224` copy.
It is **not** `+196`, **not** type-11 `+408`, **not**
vtbl+320 (`+228`).

So even when Action and MessageId hold the same i32
(NEW_GAME both 15; INVISIBLE both 229), the poster
does not consume the Action slot.

---

## 3. Who **does** read def `+196`

| Site | Who | Effect |
| --- | --- | --- |
| `0054DF50` | Type **11** ctor `0054E114` / sister `0054E184` | if `[def+196]!=0`, box and append `{id,refcount}` to `+408/+412/+416` |
| `00558E10` | Type **36** ctor `00558EC0` | box `[def+196]`, `vtbl+264` |
| `00549B20` | Type **16** ctor `00549F60` | `[widget+392]=[def+196]`; if nonzero append at `+364` |
| `0054B4B0` | Type **15** ctor `0054C050` | `[widget+468]=[def+196]` |

Type 10 `0054E3D0`, type 6 `0054F5C0`, type 38 `00558B90`
do **not** call `0054DF50`. Type 38 size `0x194` = 404:
`+408` is out of object.

`0054E1E0` appends one pair onto type-11 `+408`
(`ecx+0x198`). Slot-as-vtbl+284 after the type-11
override is **UNREAD** (no rdata). First-seen attach
does not call INVISIBLE / NEW_GAME `vtbl+284`.

Type-11 dtor `0054DF27` walks `+408` only to free
(`0054A1C0` / `00BFEA14`). That is **not** a post.

---

## 4. Action 26 (and 27–32) do not use Action

`0054DBC0` (type 11 inner apply): debounce, parent
`[def+545]`, then `0055AD60(action)`. No `+196`, no
`+408`.

`0055AD60` (`lea eax,[edi-26]`, table `0x55AE88`):

| Action | Effect | Reads Action / `+408`? |
| ---: | --- | --- |
| 26 | arm / vtbl+584 → `0055AF60` posts `+372` | **no** |
| 27 | armed-release `vtbl+524([inner+372])` | **no** |
| 28 | unarm (`type6-action28`) | **no** |
| 29–32 | hover / other | **no** |

Type-10 `0054E280` case 26 posts `&widget+352` (attach
or persist MessageId path). Not `+196`.

Type-12 `0053D200` forwards to `0055AD60` or no-ops.
List persist Action is **0** (`list-type12-focus`).

---

## 5. First-seen Press Start Action `0xE5` — used?

| Widget | Type | `0xF1A22807` | `0x53C644E4` (`+228`) | `0x230364D6` (`+224`) |
| --- | ---: | ---: | ---: | ---: |
| `UI_FRONTEND_PRESS_START_MENU` `#620` | 10 | **0** `@1335` | 0 | 0 |
| `UI_PRESS_START_TEXT` `#623` | **6** | **0** `@0705` | 0 | 0 |
| `UI_FRONTEND_LIST_PRESS_START_MENU` `#624` | 12 | **0** `@1311` | 0 | 0 |
| `UI_FRONTEND_BUTTON_INVISIBLE` `#625` | **11** | **229 / `0xE5`** `@1089` | **229** | **0** |

`UI_PRESS_START_TEXT` is the type-6 label
(`TEXT_GUI_MENU_PRESS_BUTTON`). Action 28 on type 6
stamps debounce only (`type6-action28`). Persist Action
0 means `0054DF50` is not even in that ctor.

INVISIBLE **is** type 11, so ctor **does** copy 229 into
`+408`. Apply(26) still posts `+372`. `+224` is **0**, so
`0055B040` skips vtbl+284 and `+372` stays empty (ctor
zero). Sibling `+228` = 229 goes to vtbl+320, which
`0055AF60` does **not** push.

Therefore first-seen INVISIBLE Action `0xE5` is a
**ctor leftover** on `+408`, not the UI message. Press
Start `0xE5` remains attach `00598EE6` → type-10 `+352`
(`press-start-e5-attach`). Do not drop that analog
because a child Action dword is 229.

---

## 6. NEW_GAME 15 / 15

`UI_FRONTEND_BUTTON_NEW_GAME` type 11:

| CRC | Dest | i32 |
| --- | ---: | ---: |
| `0xF1A22807` Action | `+196` | **15** |
| `0x53C644E4` | `+228` MessageId | **15** |
| `0x230364D6` | `+224` | **≠ 15** (test `Plus224 != MessageId`) |

Ctor: `0055B040` then `0054DF50`. Action 26 posts the
`+224` list (`+372`), not the Action vector. C# posts
factory `MessageId` (scan of `0x53C644E4`). Host and
native agree on **15** for the lifecycle message; they
do **not** agree that Action is the source.

---

## C# leftover

`FrontendUiDef` has no `ActionCrc`. Sequential walk
does not assign `+196`. Factory copies `MessageId` only.
`MessageFromWidgets` on action 26 is first visible
type 10/11/38 `MessageId` — leftover vs `0055CB10`,
and leftover vs `+408`.

Do **not** add `ReadPersistI32(0xF1A22807)` as
`MessageId`. Do **not** treat INVISIBLE Action as the
Press Start attach.

---

## Do not invent

- Action CRC dest `+228` (`press-start-action-e5` **STALE**).
- `UI_PRESS_START_TEXT` Action = 229.
- Action 26 posting type-11 `+408`.
- `0055B040` reading `+196`.
- Dropping `AttachFrontendTree` `0xE5` because INVISIBLE Action is 229.
- C# `FableCrc("Action") == MessageIdCrc`.
