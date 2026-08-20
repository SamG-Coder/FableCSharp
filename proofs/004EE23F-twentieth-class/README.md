# `004EE23F` twentieth `009B0AC0` / `0044C6B0` is `CPerceivedThingDef`

| Field | Value | Class |
| --- | --- | --- |
| `0044C6B0` | `004F0DDC` | **PROVEN** |
| `009B0AC0` | `004F0DE3` | **PROVEN** |
| Factory | `004D7EB6` `00BFEA1A(80)` then `0044C0C0`; vtbl **`0123AA9C`** | **PROVEN** |

Authority: `Fable.exe` listing `004D7EB6`;
`fn 004F0DB4`; `fn 0044C0C0`;
`proofs/004EE23F-remaining-pairs` row 20;
`proofs/004EE23F-nineteenth-class`.
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01244100` **`CPerceivedThingDef`**.

Listing string at `004F0DB4` is **`CPerceivedThingDef`**
(not invented). Shape-2 (`push` + `0042DAE0`).

```
004F0DB4  push "CPerceivedThingDef"
004F0DC4  push 0x4D7EB6
004F0DD6  call 0042DAE0
004F0DDC  call 0044C6B0
004F0DE3  call 009B0AC0
```

```
004D7EB6  push esi
          push 80
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D7ED6
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123AA9C
          mov eax, esi
          pop esi
          ret
004D7ED6  xor eax, eax
          pop esi
          ret
```

Next pair is `CBedDef` `004F0E92` / `004F0E99`
factory `004DA7F3` (**PROVEN** name/sites, not shipped).
