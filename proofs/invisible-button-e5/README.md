# `UI_FRONTEND_BUTTON_INVISIBLE` persist `0xE5` is leftover vs type-10 attach

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `listing-00540000.txt` (`0054DBC0` /
`0054DC30` / `0054E0B0` / `0055AD60` / `0055AF60` / `0055B040` /
`0055B460` / `0055BA20` / `0055CB10`); `listing-00600000.txt`
(`00631C60` / `00632500`); inflated `frontend.bin`
`implementer/frontend/persist-scan.txt` `#625`;
`implementer/frontend/17-press-start-frame.txt`;
`implementer/frontend/14-container.md`;
`proofs/press-start-action-e5/README.md` (file values; dest
**STALE**);
`proofs/type11-msg15/README.md` (ctor / `0054DBC0` shape;
“26 posts `+228`” **STALE**);
`proofs/press-start-e5-attach/README.md`;
`proofs/action26-subscribers/README.md`;
`proofs/action-crc-plus196/README.md`;
`proofs/crc-230364D6/README.md`;
`proofs/0055B9D0-post-dword/README.md`;
`proofs/list-type12-focus/README.md`;
`src/Fable.Formats/Defs/FrontendUiDef.cs`
(`MessageIdCrc` / `Plus224Crc`);
`src/Fable.Game/FrontendInputMap.cs`.

Do not re-prove type 4 → `push 26`, Return ≠ `0xE5`,
`0059A238` consume (`0xE5` → `00599D5C`), or type-10 `+352`
layout.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **STALE**.

---

## Verdict

**First-seen Press Start `0xE5` is type-10 attach, not this
type-11.** `UI_FRONTEND_BUTTON_INVISIBLE` is the only list
child. File CRC `0x53C644E4` (`+228` MessageId) **and**
`0xF1A22807` (`Action` / `+196`) are both **229 / `0xE5`**.
Sibling `+224` / `0x230364D6` is **0**.

Type 11 **does** register as a `0055CB10` listener
(`0055BA20`). Apply is inner `0054DBC0`. Action 26 does
**not** post those `0xE5` dwords. Click `0055AF60` (0-arg
`vtbl+584`) pushes **`widget+372`**, the `0055B040` **first**
copy (`[def+224]` / `vtbl+284`). That slot is empty. Action
and MessageId land on `+408` and `vtbl+320`, which this
apply never posts.

So: the widget can **receive** 26 (register **PROVEN**;
`+545` / `+352` / accept **UNREAD**). It does **not** post
`0xE5`. Treating INVISIBLE persist as a second Press Start
poster is **LEFTOVER** vs `00598EE6` → slot `0x14`
`0054E4F0` → type-10 `+352`.

`press-start-action-e5` pairing Action with `lea [esi+228]`
is **STALE**. `type11-msg15` “action 26 posts persist 15 /
`0xE5`” is **STALE** for the `0x53C644E4` dword.

| Claim | Status |
| --- | --- |
| PRESS_START list only persist child is `UI_FRONTEND_BUTTON_INVISIBLE` type **11** | **PROVEN** |
| That def `0x53C644E4` i32 = **229 / `0xE5`** | **PROVEN** `#625` hex `E444C653 E5000000` |
| Same def `Action` `0xF1A22807` = **229 / `0xE5`** | **PROVEN** `@1089` |
| Same def `0x230364D6` / `+224` = **0** | **PROVEN** `D6640323 00000000` |
| Action dest is def **`+196`**, not `+228` | **PROVEN** (`action-crc-plus196`) |
| `0x53C644E4` dest is def **`+228`** | **PROVEN** (`messageid-plus228`) |
| Type 11 ctor registers inner via `0055BA20` → input `vtbl+8` | **PROVEN** |
| Inner apply is `0054DBC0` | **PROVEN** |
| `0054DBC0` action 26 → `0055AD60` only if `[CUIDef+545]` | **PROVEN** |
| Action 26 posts `0x53C644E4` / Action `0xE5` | **DISPROVEN** |
| Action 26 click posts `widget+372` from `[def+224]` | **PROVEN** chain; rdata `vtbl+584` **PARTIAL** |
| INVISIBLE `+372` holds `0xE5` first-seen | **DISPROVEN** (`+224==0` → `0055B040` skips `vtbl+284`) |
| First-seen Press Start `0xE5` is type-10 attach `+352` | **PROVEN** |
| Drop attach analog because INVISIBLE persist is `0xE5` | **DISPROVEN** |
| First-seen INVISIBLE `+545` / selected `+352` / accept | **UNREAD** |
| Type-10 is a `0055CB10` node first-seen | **UNREAD** (ctor has no register) |

---

## 1. PRESS_START tree (only type-11)

`005331A0` children of `UI_FRONTEND_PRESS_START_MENU` `#620`
type 10 (`persist-scan` `children=6`;
`17-press-start-frame.txt`):

| Persist child | Type | Role |
| --- | ---: | --- |
| `UI_BLENDING_BACKGROUNDS_FORREST` | 5 | forest |
| `UI_TITLE` | 5 | title |
| `UI_PRESS_START_SWAP` | 18 | `UI_PRESS_START_TEXT` type 6 |
| `UI_FRONTEND_LIST_PRESS_START_MENU` | **12** | list |
| `UI_LEGAL_TEXT` | 6 | legal |
| `UI_MOUSE_POINTER` | 32 | cursor |

List `#624` `Children` **1** → nested `#625`
`UI_FRONTEND_BUTTON_INVISIBLE` type **11**, dest a point
(`14-container.md`). Visible first-seen. No type 38 on this
screen.

`UI_PRESS_START_TEXT` is the type-6 label
(`TEXT_GUI_MENU_PRESS_BUTTON`). Its Action / `0x53C644E4`
are **0**. Do not fold TEXT and INVISIBLE.

---

## 2. File: both `0xE5`s, `+224` is 0

`00631C60` tail (`00632500` = CRC skip + i32), dest order:

```
+196  0xF1A22807  Action
+200  …
+224  0x230364D6  Plus224Crc     → 0055B040 first, vtbl+284
+228  0x53C644E4  MessageIdCrc   → 0055B040 second, vtbl+320
```

`#625` hex (`persist-scan.txt`):

```
0728A2F1 E5000000   ; Action +196 = 229
945C648B 00000000
FCEE790E 00000000
4268A512 00000000
65DD9ACB 00000000
D6640323 00000000   ; +224 = 0
E444C653 E5000000   ; +228 = 229 / 0xE5
```

Type-10 root `#620` has **0** in all three slots.
`press-start-action-e5` file table for INVISIBLE is
**MATCH**; its “Action is `+228`” row is **STALE**.

Lionhead name of `0x53C644E4` remains **UNREAD**
(`FableCrc("Message")` / `"MessageId"` do not match).

---

## 3. Dump `0054DBC0` — receive 26, do not post `0xE5`

Type 11 ctor `0054E0B0`:

```
call 0055B460             ; 0055BA20 register + 0055B040 copy
mov [esi],   01249554
mov [esi+4], 01249530     ; inner apply 0054DBC0
call 0054DF50             ; if [def+196] → vector +408
```

`0055BA20` zeros `+348/+352`, then `0041E5F2` + input
`vtbl+8(inner)`. INVISIBLE **is** a listener. Type 10 ctor
is **not**.

Inner apply (`listing-00540000.txt`):

```
0054DBC0  esi = inner (widget+4)
          debounce [+44] vs +400 / +392
          lea ecx, [esi-4]
          call [outer.vtbl+432]     ; this widget's CUIDef*
          mov bl, [eax+545]         ; def+545, not the list
          test bl, bl
          je  0054DC21              ; drop
          push action
          call 0055AD60             ; ecx still inner
```

`action26-subscribers` “parent `+545`” is **STALE** wording:
`vtbl+432` is the same get-def used by `0055B040`. First-seen
`[def+545]` on this blob is **UNREAD** (`00631C60` `0043314A`
at `00632233`; no CRC lock in this pass).

Activate `0054DC30` (same `+545` gate) locally maps **26**,
31, 28, 27, 32, 29 via inner `vtbl+12`. That is **not**
`0055CB10` register.

`0055AD60` action 26 (`0055AD7B`):

```
mov al, [esi+348]         ; widget+352 click gate
test al, al
je  0055AE3D              ; no click
lea ecx, [esi-4]
call [outer.vtbl+584]     ; 0-arg; 0055AF60 shape
mov [esi+364], 1          ; arm
call 0055B9D0             ; cmp 25 only; not a UI post
```

`0055AF60` (outer this):

```
vtbl+192([def+524])       ; select state, not a message
push [this+372]
call [outer.vtbl+524]     ; post that list
inner.vtbl+12(28)
```

`+372` is filled only when `0055B040` sees `[def+224] != 0`
(`0055B520` list). INVISIBLE `+224` is **0** → skip →
`0055B460` left `+372 = 0`. Click, if it runs, posts an
**empty** list. It never loads `+228` / `0x53C644E4` or
`+196` / `+408`.

Ctor **does** copy Action 229 into `+408` (`0054DF50`).
Dtor frees it. First-seen apply does not walk it
(`action-crc-plus196`).

`widget+352` starts 0 (`0055BA20`). `0055C0DE` writes 1 on
take-selection. First-seen highlight of the only list child
is **UNREAD** (`list-type12-focus`). If `+352==0`, action 26
never enters `0055AF60`.

---

## 4. First-seen type 4 on this screen

```
0042E3EE  type 4 → 0055CB10(26)
  [input+8]==0 first-seen → broadcast +12 else +4
    type-11 INVISIBLE  0054DBC0
      +545==0  → drop
      +352==0  → no 0055AF60
      else     → 0055AF60(+372==0)  no 0xE5
    type-10 menu  0054E280
      UNREAD as a list node
      if invoked: 0054E2FA posts &+352 = attach 0xE5
    type-12 list  not a proven 0055CB10 node
```

No type 38. Action 27 (RMB / event type 10) on an **unarmed**
INVISIBLE is also no post (`input-type10-mmb`). That is not
first-seen type 4.

---

## 5. Type-10 attach vs C# leftover

`00598A1C` / `00598EE6` `mov [eax],0xE5` then slot `0x14`
`vtbl+284` `0054E4F0` → menu `+352`. Persist on that type-10
is 0. Host `AttachFrontendTree` (`root==PRESS_START &&
MessageId==0` → `0xE5`) is the attach analog. **Keep it.**

`FrontendInputMap.MessageFromWidgets` returns the first
visible type 10/11/38 `MessageId` (`+228`). After the patch
that is the root. Without it, DFS hits INVISIBLE persist
`0xE5` — **LEFTOVER** vs native (wrong widget **and** wrong
slot: native 26 would still post empty `+372`).

`MaybeActivateNewGameFromInput` screen-name `0xE5` is a
separate leftover (`press-start-e5-attach` §4).

---

## Do not invent

- INVISIBLE Action / `0x53C644E4` as the first-seen Press
  Start poster.
- `UI_PRESS_START_TEXT` Action = 229.
- Action dest `+228` (`press-start-action-e5` **STALE**).
- Action 26 posting type-11 `+408` or `vtbl+320` / `+228`.
- Dropping the PRESS_START attach patch because the child
  holds 229.
- Enter / Return → `0xE5`.
- Input `+8` = this list child without a write site.
- Lionhead name for `0x53C644E4` or `0x230364D6`.
