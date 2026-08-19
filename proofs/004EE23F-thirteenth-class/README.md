# `004EE23F` thirteenth `009B0AC0` / `0044C6B0` is `CWifeDef`

| Field | Value | Class |
| --- | --- | --- |
| `0044C6B0` | `004F04B4` | **PROVEN** |
| `009B0AC0` | `004F04BB` | **PROVEN** |
| Factory | `004D7BA1` `00BFEA1A(44)` then `0044C0C0`; vtbl **`0123A69C`** | **PROVEN** |

Authority: `Fable.exe` `fn 004D7BA1 --exact`;
`proofs/004EE23F-remaining-pairs` row 13;
`proofs/004EE23F-twelfth-class`.

```
004D7BA1  push esi
          push 44
          call 00BFEA1A
          …
          call 0044C0C0
          mov [esi], 0x123A69C
          ret
```

Next pair is `CDoorDef` `004F0640` / `004F0647`
factory `004D7BE7` `00BFEA1A(60)` `0044C0C0`
vtbl `0123A714` (**PROVEN**, not shipped).
