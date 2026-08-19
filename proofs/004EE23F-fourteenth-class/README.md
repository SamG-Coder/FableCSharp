# `004EE23F` fourteenth `009B0AC0` / `0044C6B0` is `CDoorDef`

| Field | Value | Class |
| --- | --- | --- |
| `0044C6B0` | `004F0640` | **PROVEN** |
| `009B0AC0` | `004F0647` | **PROVEN** |
| Factory | `004D7BE7` `00BFEA1A(60)` then `0044C0C0`; vtbl **`0123A714`** | **PROVEN** |

Authority: `Fable.exe` `fn 004D7BE7 --exact`;
`fn 004F0618`; `fn 004F0640`;
`proofs/004EE23F-remaining-pairs` row 14;
`proofs/004EE23F-thirteenth-class`.

Listing string at `004F0618` is **`CDoorDef`**
(not invented). Shape-2 (`push` + `0042DAE0`).

```
004F0618  push "CDoorDef"
004F0628  push 0x4D7BE7
004F063A  call 0042DAE0
004F0640  call 0044C6B0
004F0647  call 009B0AC0
```

```
004D7BE7  push esi
          push 60
          call 00BFEA1A
          …
          call 0044C0C0
          mov [esi], 0x123A714
          ret
```

Next pair is `CLightDef` `004F06F6` / `004F06FD`
factory `004D7C73` `00BFEA1A(92)` `0044C0C0`
vtbl `0123A814` (**PROVEN**, not shipped).
