# `004EE23F` fifteenth `009B0AC0` / `0044C6B0` is `CLightDef`

| Field | Value | Class |
| --- | --- | --- |
| `0044C6B0` | `004F06F6` | **PROVEN** |
| `009B0AC0` | `004F06FD` | **PROVEN** |
| Factory | `004D7C73` `00BFEA1A(92)` then `0044C0C0`; vtbl **`0123A814`** | **PROVEN** |

Authority: `Fable.exe` listing `004D7C73`;
`fn 004F06CE`; `fn 004F06F6`;
`proofs/004EE23F-remaining-pairs` row 15;
`proofs/004EE23F-fourteenth-class`.

Listing string at `004F06CE` is **`CLightDef`**
(not invented). Shape-2 (`push` + `0042DAE0`).

```
004F06CE  push "CLightDef"
004F06DE  push 0x4D7C73
004F06F0  call 0042DAE0
004F06F6  call 0044C6B0
004F06FD  call 009B0AC0
```

```
004D7C73  push esi
          push 92
          call 00BFEA1A
          …
          call 0044C0C0
          mov [esi], 0x123A814
          ret
```

Next pair is `CSpotLightDef` `004F07AC` / `004F07B3`
factory `004D7CB9` `00BFEA1A(68)` `0044C0C0`
vtbl `0123A88C` (**PROVEN**, not shipped).
