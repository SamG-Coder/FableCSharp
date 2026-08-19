# `004EE23F` eighteenth `009B0AC0` / `0044C6B0` is `CHeroDef`

| Field | Value | Class |
| --- | --- | --- |
| `0044C6B0` | `004F0918` | **PROVEN** |
| `009B0AC0` | `004F091F` | **PROVEN** |
| Factory | `004D7CFF` `00BFEA1A(48)` then `0044C0C0`; vtbl **`0123A904`** | **PROVEN** |

Authority: `Fable.exe` listing `004D7CFF`;
`fn 004F08F0`; `fn 004F0918`;
`proofs/004EE23F-remaining-pairs` row 18;
`proofs/004EE23F-seventeenth-class`.
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01244128` **`CHeroDef`**.

Listing string at `004F08F0` is **`CHeroDef`**
(not invented). Shape-2 (`push` + `0042DAE0`).

```
004F08F0  push "CHeroDef"
004F0900  push 0x4D7CFF
004F0912  call 0042DAE0
004F0918  call 0044C6B0
004F091F  call 009B0AC0
```

```
004D7CFF  push esi
          push 48
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D7D1F
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123A904
          mov eax, esi
          pop esi
          ret
004D7D1F  xor eax, eax
          pop esi
          ret
```

Next pair is `CCreatureModeDef` `004F0D26` / `004F0D2D`
factory `004E0B4B` (**PROVEN** name/sites, not shipped).
