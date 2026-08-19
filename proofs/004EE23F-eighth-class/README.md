# `004EE23F` eighth `009B0AC0` / `0044C6B0` is `CReadableDef`

Next Add Def Class after `CLookDef`.

Do **not** start at Oakvale / `00DBDE40`.

| Question | Answer | Class |
| --- | --- | --- |
| Next `0044C6B0` after `004EF37F`? | **`004EF5AD`**. | **PROVEN** |
| Next `009B0AC0` after `004EF386`? | **`004EF5B4`**. | **PROVEN** |
| Class name? | **`CReadableDef`**. Push `004EF57A`. Factory `[ebp-1744]=0x4DAA0E`. | **PROVEN** |
| CTC between? | **Four.** Torch / ScriptedHook / Sign / Readable. | **PROVEN** |
| Factory? | `004DAA0E`: `00BFEA1A(38)` then `0044C0C0`; vtbl **`0123E9F4`**. | **PROVEN** |
| Next leftover? | **`CVillageDef`** `004F0149` / `004F0171` / `004F0178` / `0x4E213B`. | **PROVEN** later |

```
004EF57A  push "CReadableDef"
004EF5A3  mov [ebp-1744], 0x4DAA0E
004EF5AD  call 0044C6B0
004EF5B4  call 009B0AC0
```
