# Type-10 stored message is widget+352

Dump only. No production `src/` edits.

Authority: `Fable.exe` listings
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0054E280` / `0054E3D0` / `0054E4F0` / `0054E560` / `0055CB10`),
`listing-00580000.txt` (`00598A1C` / `00598EE6` / `0059A238`),
`listing-00400000.txt` (`0042AA29` / `0042BE50`);
`implementer/frontend/05-input.md`;
`src/Fable.Game/FrontendInputMap.cs`;
`tests/Fable.Formats.Tests/FrontendInputTests.cs`.

Do not re-prove type-4 → action 26, Return ≠ `0xE5`,
or the `0059A238` consume table (`0xE5` → `00599D5C`).

---

## Verdict

| Claim | Status |
| --- | --- |
| `0054E280` `ecx` is the inner object at **widget+4** | **PROVEN** |
| Therefore `[edi+348]` / `lea esi,[edi+348]` is **widget+352** | **PROVEN** |
| Attach `00598EE6` `mov [eax],0xE5` then slot `0x14` `vtbl+284` → `0054E4F0` | **PROVEN** |
| `0054E4F0` assigns the packet into **widget+352 / +356** | **PROVEN** |
| Action 26 posts **`&widget+352`** to UI `vtbl+32` `0059A238` | **PROVEN** |
| `0059A238` **double-derefs** that pointer to the id | **PROVEN** |
| `+352` holds `0xE5` as an immediate dword | **DISPROVEN** |
| Widget-this `[+348]` is the same slot | **DISPROVEN** |

C# still stand-in posts `0xE5` when `screen == PressStartMenu`.
Native posts whatever packet is in **+352** on **any** type-10.
Physical type-4 DIK, and posters for `0x126` / 15, stay **UNREAD**.

---

## 1. Inner object is widget+4

Type-10 ctor `0054E3D0` (size `0x16C` = 364):

```
0054E3D8  call 0052CC50
0054E3DF  mov [esi], 0x12497E4      ; widget vtbl
0054E3E5  mov [esi+4], 0x12497BC    ; inner / observer vtbl
0054E3EC  mov [esi+24], 0x12497B4
0054E3F3  mov [esi+352], eax        ; packet* = 0
0054E3F9  mov [esi+356], eax        ; ctrl*  = 0
0054E3FF  mov [esi+360], eax
```

Copy-ctor `0054E410` and dtor setup `0054E450` write the same
pair. Inner-to-widget thunk:

```
0054E560  sub ecx, 4
0054E563  jmp 0054E580
```

`0055CB10` delivers the action to a subscriber as `this`:

```
0055CB20  mov ecx, eax              ; subscriber
0055CB22  mov eax, [ecx]
0055CB36  call [edx+4]              ; inner vtbl+4
```

`012497BC+4` = `0054E280` (`FrontendInputMap.Type10ActionFn`).
`ecx` on entry is **widget+4**, not the widget.

Type-5 group is `0x15C` = 348. Type-10 adds 16 bytes
(`+348…+360`). Widget `+348` is a **different** dword
(`0054E4B0` copies `[esi+48]` there). The stored message
lives at **+352**.

---

## 2. `0054E280` `[edi+348]` is widget+352

```
0054E280  push edi
0054E281  mov edi, ecx              ; inner = widget+4
…
0054E29E  mov ebx, [esp+12]         ; action
0054E2A2  lea eax, [ebx-26]
0054E2A5  cmp eax, 8
0054E2AA  movzx eax, [eax+0x54E33C]
0054E2B1  jmp [0x54E32C+eax*4]
```

Index `00 01 03 03 03 03 03 02 02` for actions 26–34.
Action **26** is case 0 → `0054E2FA`:

```
0054E2FA  mov eax, [edi+348]
0054E300  test eax, eax
0054E303  lea esi, [edi+348]
0054E309  je 0054E318
0054E30B  call 00595582             ; UI singleton
0054E310  mov edx, [eax]
0054E312  push esi                  ; &inner+348
0054E313  mov ecx, eax
0054E315  call [edx+32]             ; 0059A238
```

`edi+348` = `(widget+4)+348` = **widget+352**.
If `ecx` were the widget, this would post widget+348
(the `0054E4B0` cache) and miss the packet `0054E3D0`
zeros at +352.

`test eax,eax` skips the post when the packet* is still 0.

---

## 3. Attach writes `0xE5` through `0054E4F0`

`00598A1C` builds `UI_FRONTEND_PRESS_START_MENU` into slot
`0x14` (`00598BA2` / `0041DB1D`). Later:

```
00598EC3  push 16
00598EC5  call 00BFEA1A             ; packet
00598ED1  call 0042BE50             ; [packet]=0
00598EDA  push eax
00598EDB  lea ecx, [ebp-56]
00598EDE  call 0042AA29             ; wrapper {packet*, ctrl*}
00598EE3  mov eax, [ebp-56]
00598EE6  mov [eax], 0xE5           ; packet[0] = 0xE5
00598EF2  mov [ebp+108], 0x14
00598EF9  call 0059B5D7             ; slot 0x14
00598EFE  mov ecx, [eax]
00598F00  mov eax, [ecx]            ; widget vtbl
00598F02  lea edx, [ebp-56]
00598F05  push edx                  ; &wrapper
00598F06  call [eax+284]
```

Type-10 `012497E4+284` = `0054E4F0` (not generic
`0122F5D4+284` = `0052F040` `ret 4`).

`0042AA29` stores `{ [0]=packet*, [4]=ctrl* }`.
`0054E4F0` (widget `this`):

```
0054E4F0  mov eax, [esp+4]          ; &wrapper
0054E4F5  mov ebx, [eax]            ; packet*
0054E4F9  mov edi, [eax+4]          ; ctrl*
…
0054E530  mov [esi+352], ebx
0054E536  mov [esi+356], edi
```

So **widget+352 = packet\***, and **`[packet] = 0xE5`**.
The attach site never writes `0xE5` into +352 itself.

---

## 4. `0059A238` double deref

UI `vtbl+32` (`012521A8+32` = `012521C8`) is `0059A238`.
Arg is `esi` from `0054E2FA` = `&widget+352`.

```
0059A281  mov eax, [ebp+8]          ; &packet*
0059A284  mov eax, [eax]            ; packet*
0059A286  mov ecx, [eax]            ; packet[0] = id
0059A288  mov edx, 0xDC
0059A28D  cmp ecx, edx
…
0059A6BE  sub ecx, 0xE5
0059A6C4  je 0059A77F              ; 00599D5C
```

Two loads. `+352` as an immediate `0xE5` would make the
second load `[0xE5]`. That is **DISPROVEN**.

---

## 5. Layout

```
widget+0      012497E4          type-10 vtbl
widget+4      012497BC          inner vtbl  (0054E280 this)
…
widget+348    type-10 extra     0054E4B0 cache (not the id)
widget+352    packet*           0054E4F0 / 0054E2FA
widget+356    ctrl*
widget+360    extra
```

`inner+348` == `widget+352` == address posted to `0059A238`.

---

## Tests / leftover

`FrontendInputTests.Type4_action_26_is_0xE5_on_press_start_only`
locks VAs (`0054E280`, `0054E2FA`, `00598EE6`, `0054E4F0`)
and offset **352**.

`FrontendInputMap.MessageFromAction` still returns `0xE5`
only when `screen == PressStartMenu`. Native has no screen
string; it posts `+352` on any type-10 whose packet is
nonzero. Keep the stand-in until C# stores the packet.

Constants: `Type10InnerVtbl=0x012497BC`,
`Type10WidgetVtbl=0x012497E4`, `Type10StoredMsgOffset=352`,
`WidgetMessageVtbl=284`, `UiMessageVtbl=32`.
