# `004EE23F` ninth `009B0AC0` / `0044C6B0` is `CVillageDef`

Next Add Def Class after `CReadableDef`. First
shape-2 pair (`push` + `0042DAE0`).

Do **not** start at Oakvale / `00DBDE40`.

| Question | Answer | Class |
| --- | --- | --- |
| Next `0044C6B0` after `004EF5AD`? | **`004F0171`**. | **PROVEN** |
| Next `009B0AC0` after `004EF5B4`? | **`004F0178`**. | **PROVEN** |
| Class name? | **`CVillageDef`**. Push `004F0149`. Factory `push 0x4E213B` then `0042DAE0`. | **PROVEN** |
| Factory? | `004E213B`: `00BFEA1A(0x10C)` then `jmp 004DFF04`; vtbl **`01241DDC`**. | **PROVEN** |
| Next leftover? | **`CVillageMemberDef`** `004F01FF` / `004F0227` / `004F022E` / `0x4DA7AD`. | **PROVEN** later |

```
004F0149  push "CVillageDef"
004F0159  push 0x4E213B
004F016B  call 0042DAE0
004F0171  call 0044C6B0
004F0178  call 009B0AC0
```
