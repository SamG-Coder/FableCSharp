# Who posts 0x126 and 15; type-4 constructor

Authority: `Fable.exe` + inflated `frontend.bin`.

## Type 4 (PROVEN)

| VA | Role |
| --- | --- |
| `00A03C80` | Writes `[ecx+40]=4`, `[ecx+32]=3` |
| `00AB5420` | Translator; case calls `00A03C80` |
| `00A03B40` | Returns `[ecx+40]` |
| `0042E3EE` | Type 4 → `push 26` |

Not a DIK. Return remains type 1 / action 33.

## 0xE5 (PROVEN)

Attach `00598EE6` `mov [eax],0xE5` then type-10 vtbl+284 `0054E4F0` stores at widget+352.
Inner `0054E280` ecx is widget+4 so `[edi+348]` is widget+352.
Action 26 pushes `&+352` → `0059A238` double-deref.

## 0x126 (PROVEN persist + ctor)

`UI_ACCEPT_NEW_PROFILE` type 38 file i32 `0x126` after CRC `0x53C644E4`.
`0055B040` copies `[def+224]` through vtbl+284.
`0055AD60` / `0054DBC0` handle action 26 on type 38/11.
`0059A238` consumes 0x126 → `00851920`.

Name of `0x53C644E4` is **UNREAD**.

## 15 (PROVEN persist)

`UI_FRONTEND_BUTTON_NEW_GAME` type 11 file i32 `15` after the same CRC `0x53C644E4` (also a second 15 after `0xF1A22807`).
First child of `UI_FRONTEND_LIST_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE`.
`0059A238` msg 15 → `0059A2DA` `[retail+41]=1`.

## C# 

`FrontendUiDef.MessageIdCrc` / `MessageId`. Action 26 posts first visible type-10/11/38 stored id. No Return. No screen-name message map.
