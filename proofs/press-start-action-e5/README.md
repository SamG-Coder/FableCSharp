# Press Start type-10 `+228` / Action `0xF1A22807` is 0

Investigation only. No production `src/` edits.

Authority: inflated `frontend.bin` (`implementer/frontend/persist-scan.txt`);
`Fable.exe` `00598EE6` / `0054E4F0` / `0054E280` / `0055B040` /
`00631C60` (`listing-00580000.txt`, `listing-00540000.txt`);
`proofs/list-type12-focus/README.md`;
`proofs/press-start-e5-attach/README.md`;
`proofs/who-posts-0x126/README.md`;
`src/Fable.Formats/Defs/FrontendUiDef.cs` (`MessageIdCrc` only);
`src/Fable.Game/EngineLifecycle.cs` (`AttachFrontendTree`).

Do not re-prove type 4 → action 26, Return ≠ `0xE5`,
`0059A238` consume (`0xE5` → `00599D5C`), or type-10 +352 layout
(`type10-plus352`).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH**.

---

## Verdict

**Type-10 Press Start does not persist `0xE5` at `+228`.** File
CRC `0xF1A22807` (`Action`) on `UI_FRONTEND_PRESS_START_MENU`
`#620` is **0** (`persist-scan.txt` `@1335`). Sibling persist
`+224` / `0x53C644E4` on the same blob is also **0**. Native
`0xE5` on that menu is attach `00598EE6` `mov [eax],0xE5` then
slot `0x14` `vtbl+284` `0054E4F0` → **widget+352**.

The pairing “`UI_PRESS_START_TEXT` / `UI_FRONTEND_BUTTON_INVISIBLE`
both persist Action = 229” is **false**. Only the type-11 list
child holds 229. Type-6 TEXT Action is **0**.

**Host cannot drop** `AttachFrontendTree` (`root == PRESS_START &&
MessageId == 0` → `0xE5`). `+228` is not a hidden second source
on the type-10. C# does not even read `0xF1A22807`.

| Claim | Status |
| --- | --- |
| `0xF1A22807` is persist `Action`; runtime slot is def **`+228`** | **PROVEN** file label + `00631C60` `lea [esi+228]` |
| Type-10 `UI_FRONTEND_PRESS_START_MENU` Action / `+228` = **0** | **PROVEN** `@1335 i32=0` / hex `0728A2F1 00000000` |
| Same type-10 `+224` / `0x53C644E4` = **0** | **PROVEN** (`press-start-e5-attach`) |
| Type-10 persist already stores `0xE5` at `+228` | **DISPROVEN** |
| `UI_PRESS_START_TEXT` Action = **229** | **DISPROVEN** (`@0705 i32=0`) |
| `UI_FRONTEND_BUTTON_INVISIBLE` Action **and** `0x53C644E4` = **229 / `0xE5`** | **PROVEN** `#625` `@1089` / hex `E5000000` |
| Type-10 `0xE5` is attach `00598EE6` + `0054E4F0` +352 | **PROVEN** |
| C# `ReadPersistI32` / factory `MessageId` reads `+228` | **DISPROVEN** (only `0x53C644E4`) |
| Host PRESS_START `MessageId=0xE5` patch can be dropped because `+228` is `0xE5` | **DISPROVEN** |
| Drop the patch and let INVISIBLE persist stand in | **DISPROVEN** as native analog (type 11 ≠ type-10 attach) |

---

## 1. File scan (`frontend.bin`)

`persist-scan.txt` named-CRC walk (little-endian CRC then i32).
`Action` is `0xF1A22807`. Runtime copy order (`00631C60`):

```
00631FBD  lea edx, [esi+224]     ; file CRC 0x53C644E4
00631FC6  call 00632500
00631FCB  lea eax, [esi+228]     ; file CRC 0xF1A22807 Action
00631FD4  call 00632500
```

File CRCs are **not** adjacent bytes. `+224` / `+228` are
in-memory CUIDef slots after the CRC skip.

| Widget | Type | Off | `0xF1A22807` (`Action` / `+228`) | `0x53C644E4` (`+224`) |
| --- | ---: | ---: | ---: | ---: |
| `UI_FRONTEND_PRESS_START_MENU` `#620` | **10** | `@1335` | **0** | **0** (`E444C653 00000000`) |
| `UI_PRESS_START_TEXT` `#623` | **6** | `@0705` | **0** | **0** (`E444C653 00000000`) |
| `UI_FRONTEND_LIST_PRESS_START_MENU` `#624` | 12 | `@1311` | **0** | **0** (`list-type12-focus`) |
| `UI_FRONTEND_BUTTON_INVISIBLE` `#625` | **11** | `@1089` | **229 / `0xE5`** | **229 / `0xE5`** |

Hex windows:

```
#620 type-10  0728A2F1 00000000     ; Action 0
              E444C653 00000000     ; MessageIdCrc 0
#623 type-6   0728A2F1 00000000     ; Action 0  (not 229)
#625 type-11  0728A2F1 E5000000     ; Action 229
              E444C653 E5000000     ; MessageIdCrc 229
              0728A2F1 E5000000     ; same Action dword in hex tail
```

Do **not** fold TEXT and INVISIBLE into one persist claim.
TEXT is the type-6 label (`TEXT_GUI_MENU_PRESS_BUTTON`). It is
not an action-26 poster (`type6-action28`: action 28 stamps
debounce only).

---

## 2. Who consumes `+228`

`0055B040` (type 11/34/38 ctor path, **not** type 10 / type 6):

```
ecx = [def+224]
test ecx, ecx
je skip
call [vtbl+284]          ; store +224
then [def+228] → vtbl+320
     [def+232] → vtbl+288
     [def+236] → vtbl+292
```

Type-10 ctor `0054E3D0` never calls `0055B040`. Even a nonzero
`+228` on the menu would not land in widget+352.

C# `FrontendUiDef.MessageId` is `ReadPersistI32(raw, 0x53C644E4)`
only. `0xF1A22807` is **UNREAD**. Factory copies that `MessageId`.
`+228` cannot feed `MessageFromWidgets`.

On INVISIBLE the two file i32s happen to match (229). That
duplicate is the same pattern as NEW_GAME (both 15). The poster
still uses the `+224` / vtbl+284 object, not the Action dword
(`type11-msg15`).

---

## 3. Type-10 `0xE5` is attach only

`listing-00580000.txt`:

```
00598EC3  push 16
00598EC5  call 00BFEA1A          ; packet
00598ED1  call 0042BE50
00598EDE  call 0042AA29
00598EE3  mov eax, [ebp-56]
00598EE6  mov [eax], 0xE5        ; packet[0] = 0xE5
00598EF2  mov [ebp+108], 0x14
00598EF9  call 0059B5D7          ; slot 0x14
00598F06  call [eax+284]         ; type-10 0054E4F0
```

`0054E4F0` writes packet* at **widget+352**. Ctor zeros +352.
`0054E2FA` `test eax,eax` skips the post if attach never ran.

NEW_PROFILE / MAIN_MENU type-10 roots also persist `+224/+228=0`
and are **not** written by `00598EE6` (`press-start-e5-attach`).

---

## 4. Host `MessageId=0xE5` patch — keep

`EngineLifecycle.AttachFrontendTree`:

```
if rootName == UI_FRONTEND_PRESS_START_MENU &&
   built[0].MessageId == 0
  built[0].MessageId = 0xE5
```

That is the C# analog of `00598EE6`, collapsed onto
`FrontendWidget.MessageId` (persist `+224` field). It is **not**
filling a missing `+228` parse.

| If deleted | Effect |
| --- | --- |
| Type-10 factory `MessageId` stays 0 (`+224` and unread `+228` both 0) | `MessageFromWidgets` skips the root |
| Install still has INVISIBLE persist `0xE5` | First-visible leftover can post from type **11** |
| `Type4_drives_lifecycle_…` asserts root `MessageId==0xE5` | **fails** |
| Native type-10 +352 without attach | stays 0; action 26 no-op |

INVISIBLE is a **different** widget (`list-type12-focus`). Type-11
register via `0055BA20` is **PROVEN**; type-10 as a `0055CB10`
listener is **UNREAD** (`action26-subscribers`). Do not delete
the attach analog on the theory that Action `+228` on TEXT or
INVISIBLE substitutes.

`MaybeActivateNewGameFromInput` screen-name fallback is a
separate leftover for `Bootstrap(null)` empty widgets
(`press-start-e5-attach` §4). Not this `+228` question.

---

## Do not invent

- `UI_PRESS_START_TEXT` Action / `+228` = 229.
- Type-10 PRESS_START persist `+224` or `+228` = `0xE5`.
- C# reading `0xF1A22807` as `MessageId`.
- Dropping the PRESS_START attach patch because a child holds 229.
- Enter / Return → `0xE5`.
- Patching NEW_PROFILE / MAIN_MENU roots.
