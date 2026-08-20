# Type-10 `+352` writers besides `00598EE6` / `0054E4F0`

Investigation only. No production `src/` edits.

Authority: `Fable.exe` listings
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0054E280` / `0054E3D0` / `0054E410` / `0054E450` / `0054E4B0` /
`0054E4F0`),
`listing-00580000.txt` (`00595A06` / `00596917` / `0059899A` /
`00598A1C` / `00598EE6` / `00598F06` / `00598FD0`),
`listing-00400000.txt` (`0041D512` / `0042EA62`);
`tools/Fable.ExeIndex/out/01-sections/text-map/e8.tsv`;
`implementer/frontend/01-widget-construction.md`, `05-input.md`;
`proofs/type10-plus352/README.md`;
`proofs/type10-no-0055B040/README.md`;
`proofs/press-start-e5-attach/README.md`.

Do not re-prove type 4 → action 26, Return ≠ `0xE5`,
`0059A238` consume (`0xE5` → `00599D5C`), or type-10 `+352`
layout (`type10-plus352`).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH**.

---

## Verdict

**Ctor `0054E3D0` zeros `+352/+356/+360`.** Copy-ctor `0054E410`
does the same. Dtor `0054E450` clears `+352/+356`. Those are the
only other type-10 *stores* of `+352`. All three write **0**.

`00598EE6` does **not** write widget `+352`. It stores immediate
`0xE5` into a heap packet. Slot `0x14` `vtbl+284` `0054E4F0`
then writes that packet* at `+352`. That pair is the only
**nonzero** type-10 `+352` path recovered.

No other `.text` `E8` reaches `0054E4F0`. Other `call [reg+284]`
sites in `0058xxxx` stamp cloned **buttons** (`0x125` / `0xD7` /
`0xBCA` / `0xDC`), not Main Menu / New Profile type-10 roots.

**Main Menu / New Profile type-10 `+352` first-seen stay 0.**
Same ctor zeros; persist `+224/+228` are 0 and are never copied;
neither attach path calls `vtbl+284` on those roots.

| Claim | Status |
| --- | --- |
| `0054E3D0` zeros `+352/+356/+360` | **PROVEN** `0054E3DD`–`0054E3FF` |
| Copy-ctor `0054E410` zeros the same three | **PROVEN** `0054E433` |
| Dtor `0054E450` zeros `+352/+356` | **PROVEN** `0054E493` |
| `0054E4F0` is the only type-10 store of a **nonzero** `+352` | **PROVEN** listing |
| `00598EE6` writes widget `+352` | **DISPROVEN** (writes `packet[0]`) |
| `e8.tsv` call of `0054E3D0` besides factory `0041D512` | **DISPROVEN** |
| `e8.tsv` call of `0054E4F0` | **DISPROVEN** (none) |
| `00598A1C` `.text` caller besides `0042EA62` | **DISPROVEN** |
| Other `0058` `vtbl+284` sites write Main / New Profile type-10 `+352` | **DISPROVEN** (other `this`) |
| NEW_PROFILE slot `0x17` / MAIN_MENU `00595A06` call `vtbl+284` first-seen | **DISPROVEN** |
| Main Menu / New Profile type-10 `+352` first-seen stay **0** | **PROVEN** |

---

## Dump asked — xrefs

`ExeIndex calls` / `e8.tsv` (`site → dest`). `00598EE6` is not a
prologue; dump the containing fn and its callers.

### `0054E3D0` (type-10 ctor)

| Kind | Site | Note |
| --- | --- | --- |
| `E8` | **`0041D512`** | Factory type-10 arm only |
| Jump table | `0041D21B` `jmp [0x41D7F8+type*4]` | Type 10 → `0041D4FC` |
| Body | `0054E3D0` | 14 insns, `ret 4` |

```
0041D4FC  push 0x16C
0041D501  call 00BFEA1A
0041D510  mov ecx, eax
0041D512  call 0054E3D0
0041D517  jmp 0041D7A1
```

`e8.tsv` has **one** dest `0x0054E3D0`. Copy-ctor `0054E410` and
dtor `0054E450` are siblings, not callers.

### `0054E4F0` (type-10 `vtbl+284`)

| Kind | Site | Note |
| --- | --- | --- |
| `E8` | **none** | `e8.tsv` empty |
| Data | `012497E4+284` | type-10 widget vtbl (`05-input.md`) |
| Indirect | `call [eax+284]` when `this` is type 10 | first-seen: `00598F06` only |

`.rdata` dwords were not re-dumped this pass (`vtbl284-type11-38`:
slot `01249800` **PARTIAL** as a raw listing). Identity
`012497E4+284 = 0054E4F0` is already **PROVEN** from that note
and `type10-plus352`. Generic `0122F5D4+284 = 0052F040` `ret 4`
is a different vtbl.

### `00598EE6` (packet fill, not a fn)

| Kind | Site | Note |
| --- | --- | --- |
| Instruction | `00598A1C+…` | `mov [eax], 0xE5` |
| Containing `E8` | **`0042EA62` → `00598A1C`** | retail Init UI after `005958F5` |
| Follow-on | `00598F06` `call [eax+284]` | slot `0x14` |

No other `.text` `call 00598A1C`.

---

## 1. Ctor zeros `+352/+356/+360`

```
0054E3D0  mov eax, [esp+4]
0054E3D4  push esi
0054E3D5  push eax
0054E3D6  mov esi, ecx
0054E3D8  call 0052CC50          ; type 5
0054E3DD  xor eax, eax
0054E3DF  mov [esi], 0x12497E4   ; widget vtbl
0054E3E5  mov [esi+4], 0x12497BC
0054E3EC  mov [esi+24], 0x12497B4
0054E3F3  mov [esi+352], eax     ; packet* = 0
0054E3F9  mov [esi+356], eax     ; ctrl*  = 0
0054E3FF  mov [esi+360], eax
0054E405  mov eax, esi
0054E407  pop esi
0054E408  ret 4
```

Copy-ctor `0054E410` is the same three zeros after `0052CCA0`.
It does **not** copy the source packet.

Dtor `0054E450` releases `+356` then:

```
0054E493  mov [esi+352], 0x0
0054E49D  mov [esi+356], 0x0
          jmp 0052CCF0
```

`0054E4B0` writes **`+348`** (`[esi+48]` cache) and **`+360`**
(lookup `"UI_ACCEPT"`). It does **not** touch `+352`.

None of these four call `0055B040` (`type10-no-0055B040`).

---

## 2. Type-10 `+352` stores in this family

Grep of `listing-00540000.txt` `mov […+352]` in `0054E3D0`–
`0054E543`:

| VA | Fn | Value |
| --- | --- | --- |
| `0054E3F3` | ctor | `eax` = 0 |
| `0054E433` | copy-ctor | `eax` = 0 |
| `0054E493` | dtor | imm 0 |
| `0054E530` | **`0054E4F0`** | `ebx` = packet* |

That is the complete type-10 write set. Other `+352` hits in
the same listing are **other types** (type-6 text, type-12 list
index, type-33/34 **u8** select gate `0055BA4C` / `0055C0DE`).
Do not fold those onto a type-10 menu.

`00598EE6` is not in that table:

```
00598EE3  mov eax, [ebp-56]
00598EE6  mov [eax], 0xE5        ; packet[0], not widget+352
00598EF2  mov [ebp+108], 0x14
00598EF9  call 0059B5D7          ; slot 0x14 widget
00598F06  call [eax+284]         ; 0054E4F0 → [widget+352]=packet*
```

---

## 3. Other `vtbl+284` sites are not those roots

`listing-00580000.txt` `call [eax+284]`:

| Site | Packet / `this` | Type-10 Main / New Profile? |
| --- | --- | --- |
| `00583110` / `0058311E` | `0xBCB` / `0xBCA` on cloned `edi` | **no** |
| `00596E5F` | `0x125` on cloned list row | **no** |
| `00596F84` | `0xDB` or `0xDB+73` on cloned row | **no** |
| `005971C2` | `0xD7` on `UI_FRONTEND_BUTTON_FOR_PROFILES_LIST` | **no** |
| `005988CE` | `0x11` / `0xDC` on cloned row | **no** |
| **`00598F06`** | **`0xE5`**, slot **`0x14`** Press Start | **yes** (Press Start only) |

Persist helper `0055B12E` `call [edx+284]` is type-**34**
`0055B040` (`type10-no-0055B040`). Type 10 never reaches it.

---

## 4. First-seen Main Menu / New Profile `+352` stay 0

### New Profile — slot `0x17` in `00598A1C`

After the Press Start `0xE5` attach, the same fn factories
`UI_FRONTEND_NEW_PROFILE_SCREEN` with **no** packet / `vtbl+284`:

```
00598FD0  push "UI_FRONTEND_NEW_PROFILE_SCREEN"
00598FE0  mov [ebp+108], 0x17
00598FEA  call 0059B5D7
00598FFE  call 0041DB1D          ; factory → 0054E3D0 zeros +352
00599006  mov [ecx], eax
```

Later `00596917` (msg `0xE5` empty-name arm) only looks up slot
`0x17` and binds the edit box (`00851700` / `00851770`). No
`+352` store.

### Main Menu — `0059899A` → `00595A06`

`0059899A` picks
`UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE(_NO_CONTINUE)` and calls
`00595A06`. Factory when slot 0 is empty:

```
00595AAC  call 0041E5F2
00595AB3  call 0041DB1D          ; 0054E3D0 zeros +352
00595AB8  mov [edi], eax
00595ACC  call [eax+172]         ; not +284
```

No packet alloc. No `vtbl+284`. `00595B24` after that is a
**label-slot** table (`push 0` for `UI_TEXT_NEW_GAME`), not a
message store (`who-posts-15`).

### Persist is 0 and unused

Both roots are type 10 (`FrontendUiDefTests` factory walk).
File `+224/+228` on those defs are **0**
(`press-start-e5-attach`, `type10-no-0055B040`). Even a
nonzero file dword would not land on the widget: type-10 ctor
never calls `0055B040`.

Ctor 0 + no attach write ⇒ first-seen `+352 == 0`. Action 26
`0054E2FA` `test eax,eax` is then a no-op on those roots.
Clicks go through type-11 / type-38 children (`15` / `0x126`),
not the type-10 packet.

C# `AttachFrontendTree` still patches **only**
`UI_FRONTEND_PRESS_START_MENU`. That **MATCH**es native: do not
copy `0xE5` onto Main / New Profile type-10 roots.

---

## Do not invent

- `00598EE6` as a `+352` store.
- A second `.text` `mov [type-10+352], <nonzero>` besides
  `0054E4F0`.
- Main Menu / New Profile attach `vtbl+284`.
- Persist `+224` as the type-10 message slot.
- Type-33/34 `+352` u8 gate as this dword.
