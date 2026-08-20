# `0059A238` first-seen consumes (Press Start → New Profile → Main Menu → New Game)

Investigation only. No production `src/` edits.

Question: besides `0xE5` → `00599D5C`, `0x126` → `00851920`,
`0x124`, and 15 → `0059A2DA`, which other `0059A238` cases fire
first-seen on that path? List `cmp`/`sub` immediates and classify
each first-seen or unread.

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00580000.txt`
(`0059A238`–`0059A803`, `00599D5C` body at `00599D5E`,
`00599E3F`, `00596917`, `0059697A`, `0059899A`);
`listing-00840000.txt` (`00851920`);
`frontend.bin` persist via `FrontendUiDefTests` /
`plus224-payloads` / `who-posts-0x126-and-15`;
`docs/runtime/FORWARD_TREE.md` §4;
`src/Fable.Game/EngineLifecycle.cs` (`FrontendNoProfileFn`
comment: empty `005955AB` is **not** msg `0x125`);
`proofs/00598A1C-only-e5/README.md`,
`proofs/ui-cancel-message/README.md`.

Do not invent messages. Do not re-prove type 4 → action 26,
CRC `0x53C644E4` at `+228`, or Leave `0042F2A2`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

---

## Verdict

**None.** Empty-profile first-seen is three dests only:
`0xE5` → `00599D5C`, `0x126` → `00851920`, 15 → `0059A2DA`.
Main Menu attach on that path is tick `0059697A` /
`00595A06`, **not** `0059A238` case `0x124`.

| Claim | Class |
| --- | --- |
| Switch is `mov eax,[[ebp+8]]` then `ecx=[eax]` (boxed dword0) | **PROVEN** `0059A281`–`0059A286` |
| First-seen dest `0xE5` → `00599D5C` → empty `005955AB` → `00595845` | **PROVEN** |
| First-seen dest `0x126` → `[ui+96]` `00851920` (`[+5]=1`) | **PROVEN** |
| First-seen dest 15 → `0059A2DA` `[ui+28].vtbl+16` then `[esi+41]=1` | **PROVEN** |
| First-seen dest `0x124` → `0059899A` | **UNREAD** on this path (empty attach is `0059697A`) |
| Any other switch dest on Press Start → New Profile → Main Menu → New Game | **UNREAD** as dest; **DISPROVEN** as a recovered first-seen take |
| Msg `0x125` is the empty-profile arm | **DISPROVEN** (`EngineLifecycle` / listing: `0xE5` → `00599D5C`) |
| `.text` `mov […], 0x124` / `0x126` poster | **DISPROVEN** (`0x126` is file; no `mov [eax], 0x124` in this listing) |
| `UI_CANCEL` persist id (`0x124` or other) | **UNREAD** (`ui-cancel-message`) |
| Later Main Menu type-11 siblings (Load / Options / …) fire on New Game click | **UNREAD** (ids not dumped; broadcast vs first-child **PARTIAL**) |

---

## 1. First-seen path (no extra `0059A238`)

Empty `005955AB` (first-seen):

```
Press Start attach 00598A1C
  00598EE6 packet[0]=0xE5; slot 0x14 vtbl+284     ; store, not this switch
click → 0059A238 0xE5
  0059A6BE sub ecx, 0xE5 / je
  0059A77F call 00599D5C
    00599D79 call 005955AB
    edi==eax → 00599D8A call 00595845             ; [ui+160]=1
same-frame 00599E3F
  00599ED2 call 00596917                          ; slot 0x17; no 0059A238
click Accept → 0059A238 0x126
  fallthrough to 0059A6E5 call 00851920           ; [ui+96+5]=1
next 00599E3F
  0059A008 call 0059697A                          ; 004067C0 writable
    00596A49 call 00595A06 MAIN_MENU_NO_CONTINUE  ; not 0059899A
click New Game → 0059A238 15
  0059A2C5 je 0059A2DA
```

`0059899A` callers in this listing: `00599CC2`, `00599DE0`
(one-name `00599D5C`), `00599F95`, `0059A036` (tick miss of
`0059697A`), `0059A729` (msg `0x124`). First-seen empty does
not take those.

`00851920` (`listing-00840000.txt`) sets `[esi+5]` / `[esi+4]`
and returns. No nested `0059A238`.

---

## 2. Case ids from `cmp`/`sub`/`dec`

`ecx` is the message id. `jg`/`je` on `0xDC` / 17 / `0x127`
split the tree; `sub`/`dec` then match.

| Imm (listing) | Case id | Dest | First-seen dest |
| --- | ---: | --- | --- |
| `cmp ecx, 17` / `je` | **17** | `0059A354` string copy | **UNREAD** |
| `sub ecx, 9` / `je` | **9** | remap slot 1 | **UNREAD** |
| `dec`×2 / `je` | **11** | remap slot 3 | **UNREAD** |
| `dec` / `je` | **12** | remap slot 4 | **UNREAD** |
| `dec` / `je` | **13** | remap slot 5 | **UNREAD** |
| `dec` / `je` | **14** | remap slot 6 | **UNREAD** |
| `dec` / `je` | **15** | `0059A2DA` | **PROVEN** |
| `dec` / `je` | **16** | `00597B20` | **UNREAD** (profiles>1) |
| `sub ecx, 66` / `je` | **66** | `00598463` | **UNREAD** |
| `dec` / `je` | **67** | remap slot 9 | **UNREAD** |
| `sub ecx, 19` / `je` | **86** | `00597BF2` | **UNREAD** |
| `sub ecx, 0x80` / `je` | **`0xD6`** | `0059A4B1` delete/select | **UNREAD** |
| `dec` / `je` | **`0xD7`** | `0059A3D1` `"UI_TEXT_DELETE_PROFILE_MENU_TITLE"` | **UNREAD** |
| `sub ecx, 4` / `je` | **`0xDB`** | remap slot `0xF` | **UNREAD** |
| `mov edx, 0xDC` / `je` | **`0xDC`** | `0059A5FF` `"UI_INVALID_SAVE_TEXT"` | **UNREAD** |
| `sub ecx, 0xE5` / `je` | **`0xE5`** | `00599D5C` | **PROVEN** |
| `sub ecx, edi` (`edi=21`) / `je` | **`0xFA`** | `00597006` | **UNREAD** |
| `sub ecx, 33` / `je` | **`0x11B`** | remap slot `0x16` | **UNREAD** |
| `sub ecx, 9` / `je` | **`0x124`** | `0059899A` | **UNREAD** (this path) |
| `dec` / `je` | **`0x125`** | `00595845` (not via `00599D5C`) | **DISPROVEN** first-seen |
| `dec` / `je` | **`0x126`** | `00851920` | **PROVEN** |
| `mov edx, 0x127` / `je` | **`0x127`** | `00851860` | **UNREAD** |
| `sub ecx, 0x128` / `je` | **`0x128`** | `[0x13B871C] vtbl+16` | **UNREAD** |
| `dec` / `je` | **`0x129`** | remap slot `0x18` | **UNREAD** |
| `sub ecx, 15` / `je` | **`0x138`** | `005963DB` | **UNREAD** |
| `dec`×2 / `je` | **`0x13A`** | remap slot `0x1A` | **UNREAD** |
| `sub ecx, 7` / `je` | **`0x141`** | remap slot `0x1C` | **UNREAD** |
| `sub ecx, 0xA90` / `je` | **`0xBD1`** | remap slot `0x14` | **UNREAD** |

No other `je` after a `cmp ecx` / `sub ecx` in `0059A238`–
`0059A803`. Default is `0059A7FF` epilogue.

Posted ids recovered on this tree (file / attach only):

| Widget | Id | Screen |
| --- | ---: | --- |
| attach packet / `UI_FRONTEND_BUTTON_INVISIBLE` | `0xE5` | Press Start |
| `UI_ACCEPT_NEW_PROFILE` `+228` | `0x126` | New Profile |
| `UI_FRONTEND_BUTTON_NEW_GAME` `+228` | 15 | Main Menu no-continue |

No dumped `0x53C644E4` payload of `0x124` / 17 / `0xDC` /
`0x127` / `0xBD1` on those three roots.

---

## 3. Every `cmp`/`sub` immediate in `0059A238`

Message-switch immediates (dest class above). **Exec** =
the insn runs on first-seen `0xE5` / `0x126` / 15.

| VA | Insn | Role | Exec | Dest |
| --- | --- | --- | --- | --- |
| `0059A23B` | `sub esp, 20` | frame | yes | n/a |
| `0059A24E` | `cmp [esi+216], 0` | prologue byte | yes | n/a |
| `0059A257` | `cmp [esi+217], 0` | prologue byte | yes | n/a |
| `0059A288` | `mov edx, 0xDC` / `cmp ecx, edx` | pivot + case `0xDC` | yes | dest **UNREAD** |
| `0059A29B` | `cmp ecx, 17` | pivot + case 17 | yes (msg 15) | dest **UNREAD** |
| `0059A2AA` | `sub ecx, 9` | cases 9–16 | yes (msg 15) | only 15 dest **PROVEN** |
| `0059A2E1` | `cmp [esi+216], 0` | body 15 | yes | n/a |
| `0059A394` | `sub ecx, 66` | 66 | **no** | **UNREAD** |
| `0059A3A4` | `sub ecx, 19` | 86 | **no** | **UNREAD** |
| `0059A3AD` | `sub ecx, 0x80` | `0xD6` | **no** | **UNREAD** |
| `0059A3BC` | `sub ecx, 4` | `0xDB` | **no** | **UNREAD** |
| `0059A3D4` | `cmp [ecx], ecx` | bodies 17/`0xD7`/`0x124` | **no** | n/a |
| `0059A4BA` | `cmp [eax+12], 0` | body `0xD6` | **no** | n/a |
| `0059A50B` | `cmp [ebp+11], 0` | body `0xD6` | **no** | n/a |
| `0059A5BD` | `cmp [esi+188], ebx` | body 66 | **no** | n/a |
| `0059A602` | `cmp [eax], eax` | body `0xDC` | **no** | n/a |
| `0059A6AB` | `mov edx, 0x127` / `cmp ecx, edx` | pivot + case `0x127` | yes (`0xE5`/`0x126`) | dest **UNREAD** |
| `0059A6BE` | `sub ecx, 0xE5` | `0xE5` | yes | dest **PROVEN** |
| `0059A6CD` | `sub ecx, edi` (`21`) | `0xFA` | yes (`0x126` miss) | dest **UNREAD** |
| `0059A6D1` | `sub ecx, 33` | `0x11B` | yes (`0x126` miss) | dest **UNREAD** |
| `0059A6D6` | `sub ecx, 9` | `0x124` | yes (`0x126` miss) | dest **UNREAD** |
| `0059A798` | `sub ecx, 0x128` | `0x128` | **no** | **UNREAD** |
| `0059A7A3` | `sub ecx, 15` | `0x138` | **no** | **UNREAD** |
| `0059A7AC` | `sub ecx, 7` | `0x141` | **no** | **UNREAD** |
| `0059A7B1` | `sub ecx, 0xA90` | `0xBD1` | **no** | **UNREAD** |

`dec ecx` is not an immediate; it is the 11–16 / `0x125`–
`0x126` / `0x129` / `0x13A` stair. Register `cmp ecx, edx`
uses the `0xDC` / `0x127` immediates already listed.
`cmp [0x13CA818], ebx` has no imm (ebx=0).

---

## 4. Why `0x124` is not first-seen empty

`0059A714` (`0x124`) does `call 0059899A` then slot 0
`00596763`. First-seen Main Menu is instead:

```
0059A008  call 0059697A
00596A36  push "UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE"
00596A49  call 00595A06
```

One-name `00599D5C` (`cmp eax,4` after `005955AB`) also
`call 0059899A` **without** posting `0x124`. That arm is
not first-seen empty.

Host `Frontend_0059A238_msg_124_*` injects `0x124`. That is
consumer coverage, not this path.

`ui-cancel-message`: `UI_CANCEL` `0x53C644E4` is **UNREAD**.
Do not invent `0x124` there.

---

## What this pass did not do

- Did not inflate `UI_CANCEL` / Main Menu Load / Options
  `0x53C644E4`.
- Did not dump type-11/38 `+584`/`+588` rdata.
- Did not walk `0055CB10` first-seen listener order.

---

## Answer

No other `0059A238` dest fires first-seen. Keep `0xE5` /
`0x126` / 15. Treat `0x124` as a recovered consume that this
empty-profile sequence does not take.
