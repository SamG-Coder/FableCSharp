# Type 4 is LMB down (`00A03C80` / device 3)

Investigation plus host wiring. Authority is Fable.exe
`.text`, not C# comments.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

## Direct answer

Type 4 is **left-mouse-button down**. Not keyboard, not
“any mouse button”, not a `0041DF10` bind slot.
`00A03B40` is a getter (`[record+40]`).

## Path

```
CMouseDX 00AB5710 DINPUT8
  exclusive 00AB4910 GetDeviceData DIMOFS_BUTTON0
    dwData 0x80 → code 1 (down) / 4 (up)
  windowed 00AB4BB0 WM_LBUTTONDOWN=0x201 → code 1
00AB5420 switch(code-1) index 0 → 00AB54F0
  call 00A03C80
    [record+0]  = 0          // unused, no DIK
    [record+32] = 3          // mouse device
    [record+40] = 4          // type
0042E3EE type 4 → push 26
```

| Button | Down type | Up type |
|---|---|---|
| LMB | **4** | 6 |
| RMB | 7 | 9 |
| MMB | 10 | 12 |
| Move | 13 | — |

Return / DIK 28 is type 1 action 33. **DISPROVEN** as
`0xE5` / `0x126` / 15.

## Host

`Program.cs` LMB **edge** → `QueueInput(Type4, 0)`.
`FrontendInputMap.DikPosterUnread` is false.
Type 13 remains mouse **move**.

`ActivateNewGame()` is still the explicit `0059A238`
API (msg 15). The input path posts stored widget ids.
