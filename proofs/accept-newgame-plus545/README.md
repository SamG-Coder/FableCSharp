# `UI_FRONTEND_BUTTON_NEW_GAME` / `UI_ACCEPT_NEW_PROFILE` `CUIDef+545` CRC `0x9E47F106`

Investigation only. No production `src/` edits.

Question: file persist `u8` for CRC **`0x9E47F106`**
(`CUIDef+545`) on `UI_FRONTEND_BUTTON_NEW_GAME` and
`UI_ACCEPT_NEW_PROFILE` — **0 or 1?**

Authority: inflated TLC `frontend.bin`
`C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters\data\CompiledDefs\frontend.bin`
(`DataCatalogTests` 810 entries);
`implementer/frontend/persist-scan.txt` `#625` (INVISIBLE hex
only);
`export/frontend/persist-tail.txt` (Press Start / TITLE /
FOREST / MOUSE; **not** these two);
`proofs/cuidef-plus545/README.md`;
`proofs/invisible-plus545/README.md`;
`proofs/newgame-plus545/Dump.csx` (recipe; **no**
`dump-out.txt` in this tree);
`FrontendUiDef.ReadPersistU8` /
`FrontendUiDefTests` (`MessageId` / `Plus224` only);
dump `0054DBC0` / `0054DC30` / `0055AD60` / `0055AEB0` in
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`;
writer `00632233` `0043314A` in `listing-00600000.txt`.

Do not re-prove type 4 → `push 26`, `0x53C644E4` → `+228`
(`0x126` / 15), type-10 attach `+352`, or Lionhead name
for `0x9E47F106`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**STALE** / **LEFTOVER**.

---

## Verdict

Persist CRC **`0x9E47F106`** is dest **`CUIDef+545`**
(`0043314A` `setne` → **0 or 1**). That lock is already
**PROVEN** (`cuidef-plus545` / `invisible-plus545`).

**File bytes on these two widgets are not in the checked-in
hex.** `persist-scan.txt` only dumps the Press Start tree.
`persist-tail.txt` never opens Accept or New Game.
`FrontendUiDefTests` locks `MessageId` / `Plus224`, not this
`u8`. `proofs/newgame-plus545/dump-out.txt` was **not**
written. This pass did **not** inflate `frontend.bin`.

`cuidef-plus545` §6 already filed both payloads **UNREAD**.
That is still the file answer. Do **not** copy INVISIBLE’s
**1** onto New Game, or the Main Menu list’s **0** onto the
type-11 child (`invisible-plus545`: parent-list `+545` is
**DISPROVEN** as the type-11 apply gate).

| Widget | Type | Apply | File `0x9E47F106` |
| --- | ---: | --- | ---: |
| `UI_FRONTEND_BUTTON_INVISIBLE` | 11 | `0054DBC0` | **1** (`#625` `06F1479E 01`) |
| `UI_FRONTEND_LIST_PRESS_START_MENU` | 12 | `0053D200` | **0** (`#624`) |
| `UI_FRONTEND_BUTTON_NEW_GAME` | 11 | `0054DBC0` | **UNREAD** |
| `UI_ACCEPT_NEW_PROFILE` | 38 | `0055AD60` | **UNREAD** |
| Main Menu list / root | 12 / 10 | not `0054DBC0` | **UNREAD** (same CRC; no hex) |

If the unread New Game byte were **0**, type-11 inner
`0054DBC0` would **never** `call 0055AD60` (`test bl` /
`je 0054DC21`). Broadcast still delivers action 26 to apply.
If it were **1**, this gate is open; click still needs
`widget+352` / empty `+372` (`plus224-payloads`).

If the unread Accept byte were **0**, type-38 apply
**still** runs `0055AD60`. Enable `0055AEB0` has **no**
`+545` test (`cuidef-plus545` §2–3). Accept’s 26 gate is
`+352`, not this CRC.

| Claim | Status |
| --- | --- |
| File CRC `0x9E47F106` → dest `+545` | **PROVEN** (`00632233`) |
| `00403EB0` `setne` → dest is 0 or 1 | **PROVEN** |
| INVISIBLE file `+545` = **1** | **PROVEN** |
| NEW_GAME file `+545` is 0 | **UNREAD** |
| NEW_GAME file `+545` is 1 | **UNREAD** |
| ACCEPT file `+545` is 0 | **UNREAD** |
| ACCEPT file `+545` is 1 | **UNREAD** |
| Same writer runs on every `UI` def | **PROVEN** (`00631C60`) |
| Tests already assert this `u8` | **DISPROVEN** |
| Type 11 apply tests **this** def `+545` | **PROVEN** |
| Type 38 apply tests `+545` | **DISPROVEN** |
| Parent list `+545` is the type-11 gate | **DISPROVEN** (**STALE**) |
| C# `TryParse` stores `0x9E47F106` | **DISPROVEN** (unread) |

**Answer:** **not recovered.** CRC / dest are locked.
NEW_GAME and ACCEPT `u8` stay **UNREAD** until
`ReadPersistU8(raw, 0x9E47F106)` on the inflated entries.
Do not invent **0** or **1**.

---

## 1. Why existing dumps miss these two

`persist-scan.txt` hex starts at
`UI_FRONTEND_PRESS_START_MENU` `#620` and walks that tree
through INVISIBLE `#625`, LEGAL, MOUSE. Name table lists
`UI_FRONTEND_BUTTON_NEW_GAME` (`03093163`) and
`UI_ACCEPT_NEW_PROFILE` (`A24F408D`); **no** `hex:` line
for either instance.

`FrontendPersistTailTests` writes `persist-tail.txt` from a
fixed Press Start / TITLE / FOREST / MOUSE list. The only
aligned `0x9E47F106` rows there are TEXT / SWAP
(`u8=0`).

Tests that **do** open the two blobs:

```
accept.Type == 38;  accept.MessageId == 0x126;  accept.Plus224 == 0
newGame.Type == 11; newGame.MessageId == 15;    newGame.Plus224 == 0
```

No `ReadPersistU8(..., 0x9E47F106)`.

`proofs/newgame-plus545/Dump.csx` is the intended lock
(plus neighbouring `+544` / `+522` / `+548`).
`run-dump.ps1` would tee `dump-out.txt`. That file is
**absent**. Values below stay **UNREAD**.

Recipe (do not treat as already run):

```
FrontendUiDef.ReadPersistU8(entry.Raw, 0x9E47F106u)
```

First-hit byte scan matches `0043314A` file form (CRC then
one byte). Hits should be **1** per widget, same as Press
Start flags. Neighbouring tail on `#625`:

```
1D972DCA 00     ; 0xCA2D971D +544
559B9CE5 01     ; 0xE59C9B55 +522
06F1479E ??     ; 0x9E47F106 +545   ← this question
EA876CF2 02000000
```

Expect that same four-CRC suffix on Accept / New Game.
The `??` byte is the answer.

---

## 2. Gate: 0 vs 1 is not the same on both widgets

Type 11 inner apply (`0054DBC0`, `ecx` = `widget+4`):

```
debounce [inner+44] vs +400 / +392
call [outer.vtbl+432]     ; this CUIDef*
mov  bl, [eax+545]
test bl, bl
je   0054DC21             ; no 0055AD60
call 0055AD60
```

Activate `0054DC30` uses the same byte: `je 0054DCB2`
skips `vtbl+192(3)` and the local 26/31/28/27/32/29 map.

**NEW_GAME is type 11.** File **0** → action 26 never
enters `0055AD60` on that widget. File **1** → this gate
open. That is why the byte matters here.

Type 38 apply **is** `0055AD60` (no `[…+545]`). Enable
`0055AEB0` maps 26/31/27/32 with **no** `+545`.

**ACCEPT is type 38.** File **0** or **1** does **not**
drop first-seen Accept 26. Selected `+352` still does
(`type11-plus352-select`). `+224==0` still makes action 26
post the empty `+372` list (`plus224-payloads`).

Host leftover: `MessageFromWidgets` posts factory
`MessageId` (`+228`) on action 26 with **no** `+545` read.
Native type 11 would drop if the unread New Game byte were
0.

---

## 3. Do not copy INVISIBLE / list values

`invisible-plus545`: INVISIBLE `#625` is **1**; parent list
`#624` is **0**. Treating the type-11 child as 0 because
the list is 0 is **STALE**.

Main Menu shape (`newgame-plus380-first`):

```
UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE     type 10
└── UI_FRONTEND_LIST_MAIN_MENU_…                   type 12
    └── [0] UI_FRONTEND_BUTTON_NEW_GAME            type 11
```

List `+545` would only gate type-12 activate `0053D540`,
not New Game `0054DBC0`. Even a dumped list **0** would
not answer this question.

New Profile Accept is not a type-11 child of a list apply.
Its `+545` is unused by the recovered Accept 26 path.

---

## 4. Writer lock (unchanged)

```
00632217  lea eax, [esi+544]     ; 0xCA2D971D
00632225  lea ecx, [esi+522]     ; 0xE59C9B55
00632233  lea edx, [esi+545]     ; 0x9E47F106
          call 0043314A
00632241  add esi, 0x224
          call 006326C0          ; 0xF26C87EA → +548
```

Copy ctor `00631C38` copies the byte. Size `0x228`.
Lionhead name **UNREAD**. `Enabled` / `Visible` / `Clip` /
`Clickable` hashes are **DISPROVEN** (`persist-flag-names`).

---

## Do not invent

- NEW_GAME / ACCEPT `0x9E47F106` = 0.
- NEW_GAME / ACCEPT `0x9E47F106` = 1.
- INVISIBLE **1** as the Main Menu / New Profile value.
- Parent-list `+545` as the type-11 apply gate.
- Type 38 apply requiring `+545`.
- `+545==1` as “action 26 posts `0x126` / 15” (still
  `+352` / `+224` / `+372` / later `+380`).
- C# already parsing this CRC.
