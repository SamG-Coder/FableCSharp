# Type 34 `vtbl+524` first-seen vs host `FrontendWidgetFactory`

Investigation only. No production `src/` / `tests/` edits.

Question: first-seen type 34 `vtbl+524` (`0124BD2C+524` /
`00558DE0`) vs host factory. **MATCH** or **LEFTOVER**?

Authority: `proofs/type34-vtbl524/README.md`,
`proofs/type34-vtbl588-rdata/README.md`,
`proofs/plus224-payloads/README.md`,
`proofs/0055ACF0-first-caller/README.md`,
`proofs/type34-plus364-ctor/README.md`,
`proofs/0041E6D3-frontend-gate/README.md`,
`proofs/action26-subscribers/README.md`;
dump `listing-00540000.txt` (`0055B460` / `0055B040` /
`0055ACF0` / `0055AF60` / `00558DE0`);
`src/Fable.Game/FrontendWidgetFactory.cs` (read only);
`src/Fable.Formats/Defs/FrontendWidgetType.cs` type 34
row; `src/Fable.Game/FrontendInputMap.cs`
`Type34ClickFn` / `Plus228PostFn` /
`MessageFromPlus228List`.
Do not invent widgets.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **MATCH** / **LEFTOVER**.

Do not re-prove persist CRC `0x230364D6` → `+224` /
`0x53C644E4` → `+228`, or that `0055ACF0` *calls*
`vtbl+524` with `[+380]`. Do not re-prove action 26
`vtbl+584` = `0055AF60`. `.rdata` slot dword stays
**PARTIAL**.

---

## Verdict

**MATCH.** First-seen attach does **not** enter type 34
`vtbl+524` with a live list. Host `Build` /
`ApplyFirstSeenState` also does **not** walk or post.
There is **no** leftover walker to put in the factory.

| Claim | Class |
| --- | --- |
| Type 34 ctor `0055B460` installs `0124BD2C`, zeros `+372/+380`, then `0055B040` | **PROVEN** |
| `0055B040` stores lists through `vtbl+284` / `+320`, **not** `+524` | **PROVEN** |
| Unique 1-arg walker body is `00558DE0` (`test edi; je` empty) | **PROVEN** ABI; rdata **PARTIAL** |
| `0124BD2C+524` printed `00558DE0` | **UNREAD** (want `0124BF38`) |
| First-seen persist **Type==34** widget on Press Start / New Profile / Main Menu | **DISPROVEN** (do not invent one) |
| First-seen type 11/38 **are** type-34 derived; live outer after ctor overwrite is `01249554` / `0124B04C` | **PROVEN** |
| First-seen `+224` / `+372` empty (`INVISIBLE` / `NEW_GAME` / `ACCEPT`) | **PROVEN** |
| First-seen `+228` / `+380` may hold `0xE5` / 15 / `0x126` | **PROVEN** list fill; **not** walked at attach |
| Attach / factory / `0052C730` first-seen state calls `vtbl+524` | **DISPROVEN** |
| Action 28 first-seen already `0055ACF0` → `+524([+380])` | **DISPROVEN** (`inner+364==0`) |
| Host factory leftover side effect for `+524` | **DISPROVEN** (**MATCH** skip) |
| Host `MessageFromPlus228List` scalar vs `00558DE0` walk | **LEFTOVER** shape on **later** armed action 28; **not** first-seen attach |

**Answer:** first-seen vs host is **MATCH** (both skip).
Implementing `00558DE0` in `FrontendWidgetFactory` would
be leftover theater.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| Native first-seen call of type 34 `vtbl+524`? | **No** at ctor / persist / first-seen state. Callers are later 0-arg posters `0055AF60` (`[+372]`) and `0055ACF0` (`[+380]`). | **PROVEN** skip |
| Nonempty first-seen `+524` post? | **No.** Action 26 would push **empty** `+372`. Action 28 that would push filled `+380` is unarmed. | **PROVEN** |
| Host `FrontendWidgetFactory.Build` walks `+524`? | **No.** Stores `Plus224` / `MessageId` scalars, then `ApplyFirstSeenState` visibility. | **PROVEN** **MATCH** |
| Host leftover mutation? | **None** at attach. No `0059A238` analog, no list walk. | **PROVEN** **MATCH** |

---

## 1. Dump — type 34 ctor never calls `+524`

`listing-00540000.txt` `0055B460` (`ret 4`):

```
0055B468  call 0055BA20
0055B471  mov [esi], 0x124BD2C      ; type 34 outer (persist-time)
0055B477  mov [esi+4], 0x124BD08
0055B485  mov [esi+364], eax        ; 0
0055B48B  mov [esi+368], al         ; armed u8 = 0
0055B491  mov [esi+372], eax        ; +224 list head
0055B49D  mov [esi+380], eax        ; +228 list head
0055B4B5  call 0055B040             ; persist copy
0055B4BD  ret 4
```

No `call [eax+524]`. Sister copy-ctor `0055B4C0` is the
same shape. Persist `0055B040` (type34-vtbl524 §2) boxes
`[def+224]` → `vtbl+284` (`0055B520` → `+372`) and
`[def+228]` → `vtbl+320` (`0055B5B0` → `+380`). Identity
of those slots is **PARTIAL**; they are **not** `+524`.

Type 11/38 then overwrite the three vtbl dwords
(`0054E0BF` `01249554`, `00558B9D` `0124B04C`). Runtime
`call [this+524]` uses the **final** table. Persist-time
`0124BD2C+524` is only live inside `0055B040`, which does
not call it.

Factory type 34 (`FrontendWidgetType.Table[34]`): ctor
`0055B460`, size `0x194`, **Vtbl dword 0** in C#. Native
outer is `0124BD2C` (`type34-vtbl588-rdata`). The missing
C# vtbl is unused by `Build` (no vtbl dispatch). Do not
invent a first-seen persist Type 34 instance: recovered
trees use type **11** / **38** (and type 10/6/5/0/18/32/37
elsewhere).

---

## 2. Dump — `vtbl+524` body and first-seen lists

Walker (`type34-vtbl524`; `listing` `00558DE0`):

```
00558DE0  mov edi, [esp+8]          ; list*
          test edi, edi
          je  00558E09              ; NULL → ret 4
          ; walk sentinel; 0041E6D3(&node+8)
          ret 4
```

Callers that `push` a list then `call [this.vtbl+524]`:

| Body | Push | Typical slot | First-seen |
| --- | --- | --- | --- |
| `0055AF60` | `[this+372]` (`[def+224]`) | type-34 `+584` **PARTIAL** | **empty** (`Plus224==0`) |
| `0055ACF0` | `[this+380]` (`[def+228]`) | type-34 `+588` **PARTIAL** | **not entered** (`inner+364==0`) |

`0055AFB7` / `0055AD1A` are the **PROVEN** `+524` sites.
`0055AF7F` `[eax+524]` is **def+524**, **DISPROVEN** as
this slot (`0055AF60-callee`).

File payloads (`plus224-payloads`; factory tests; do not
invent names):

| Widget | Persist type | `+224` | `+228` |
| --- | ---: | ---: | ---: |
| `UI_FRONTEND_BUTTON_INVISIBLE` | 11 | **0** | `0xE5` |
| `UI_FRONTEND_BUTTON_NEW_GAME` | 11 | **0** | **15** |
| `UI_ACCEPT_NEW_PROFILE` | 38 | **0** | **`0x126`** |

Press Start first subscriber is INVISIBLE
(`action26-subscribers`). `0041E6D3-frontend-gate`:
first-seen Press Start `00558DE0` path is “no type 38;
type-11 INVISIBLE `+224` empty”. Empty `+372` →
`00558DE0` `je` → **no** `0041E6D3` / **no** `0059A238`.

Action 28 that *would* walk filled `+380` is skipped
until a later action 26 arms `inner+364`
(`type34-plus364-ctor`). First-seen `0055ACF0` callers
`00557AF4` / type-35 tails are **not** on these trees
(`0055ACF0-first-caller`).

Wanted rdata (`type34-vtbl588-rdata`): `0124BD2C+524` =
`0124BF38` → `00558DE0`. **UNREAD** this pass.

---

## 3. Host factory — same skip

`FrontendWidgetFactory.Build`:

```
Add(… Plus224: def.Plus224, MessageId: def.MessageId …)
AttachChildren  // 005331A0 analog
ApplyFirstSeenState  // 0052C730 +324/+328/+332=0 analog
```

`Add` copies the two persist i32s onto the record. It
does **not** allocate circular sentinels, does **not**
call `00558DE0`, does **not** queue a frontend message.

`ApplyFirstSeenState` writes `Visible` / `Enabled` /
`ActiveChild=0`. No `vtbl+524`.

`FrontendInputMap` names the **bodies**, not factory
dispatch:

- `Type34ClickFn = 0055AF60` (action 26; empty `+372`)
- `Plus228PostFn = 0055ACF0` (action 28; `+380`)
- `MessageFromWidgets(26)` → type-10 `+352` analog only
- `MessageFromPlus228List` → type 11/38 `MessageId` if
  `Armed`

First-seen attach never calls `MessageFromWidgets`.
`Armed` starts false (ctor `+368==0`). That **MATCH**es
unarmed skip of `+588` / `+524([+380])`.

Host **LEFTOVER** that is **not** first-seen attach:
`MessageFromPlus228List` returns the persist scalar
instead of walking boxed `node+8`. That is the later
armed type-6 / action 28 stand-in. Do not fold it into
`Build`.

`plus224-payloads` leftover “host posts `MessageId` on
action 26” is **STALE** for current
`MessageFromWidgets(26)` (type 10 only). Action 26
native `+524([+372])` is still empty on these defs.

---

## 4. What this is not

| Claim | Class |
| --- | --- |
| `0055ACF0` / `0055AF60` **is** `vtbl+524` | **DISPROVEN** (callers) |
| `0041E6D3` **is** `vtbl+524` | **DISPROVEN** (walker `vtbl+56`) |
| First-seen factory should implement `00558DE0` | **DISPROVEN** leftover |
| First-seen Accept / New Game click already posts via `+524` | **DISPROVEN** (`+224==0`; 28 unarmed) |
| Persist Type 34 widget on first-seen menus | **DISPROVEN** (do not invent) |
| `0124BD2C+524` dword printed | **UNREAD** |

---

## Classification (VAs / host)

| Item | Role | Class |
| --- | --- | --- |
| `0055B460` | type 34 ctor; no `+524` | **PROVEN** |
| `0055B040` | persist lists via `+284/+320` | **PROVEN** |
| `00558DE0` | 1-arg list walker | **PROVEN** body; =`vtbl+524` **PARTIAL** |
| `0055AF60` `call [eax+524]` | action 26 / empty `+372` first-seen | **PROVEN** site; post **none** |
| `0055ACF0` `call [eax+524]` | action 28 / `+380`; first-seen skip | **PROVEN** skip |
| `0124BD2C+524` | type-34 slot dword | **UNREAD** |
| `FrontendWidgetFactory.Build` | first-seen vs `+524` | **MATCH** |
| `MessageFromPlus228List` | later armed 28 | **LEFTOVER** shape; **not** attach |

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
  (`0055B460`, `0055B040`, `0055ACF0`, `0055AF60`,
  `00558DE0`)
- `proofs/type34-vtbl524/README.md`
- `proofs/type34-vtbl588-rdata/README.md`
- `src/Fable.Game/FrontendWidgetFactory.cs`
- `src/Fable.Game/FrontendInputMap.cs`
- `src/Fable.Formats/Defs/FrontendWidgetType.cs`
