# `004EE23F` nineteenth `009B0AC0` / `0044C6B0` is `CCreatureModeDef`

| Field | Value | Class |
| --- | --- | --- |
| `0044C6B0` | `004F0D26` | **PROVEN** |
| `009B0AC0` | `004F0D2D` | **PROVEN** |
| Factory | `004E0B4B` `00BFEA1A(64)` then `jmp 004DE7DC`; vtbl **`01241704`** | **PROVEN** |

Authority: `Fable.exe` listing `004E0B4B`;
`fn 004F0CFE`; `fn 004DE7DC`;
`proofs/004EE23F-remaining-pairs` row 19;
`proofs/004EE23F-eighteenth-class`.
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01244114` **`CCreatureModeDef`**.

Listing string at `004F0CFE` is **`CCreatureModeDef`**
(not invented). Shape-2 (`push` + `0042DAE0`).

```
004F0CFE  push "CCreatureModeDef"
004F0D0E  push 0x4E0B4B
004F0D20  call 0042DAE0
004F0D26  call 0044C6B0
004F0D2D  call 009B0AC0
```

```
004E0B4B  push 64
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004E0B5E
          mov ecx, eax
          jmp 004DE7DC
004E0B5E  xor eax, eax
          ret

004DE7DC  push esi
          mov esi, ecx
          call 0044C0C0
          xor eax, eax
          mov [esi], 0x1241704
          mov [esi+52], eax
          mov [esi+56], eax
          mov [esi+60], eax
          mov eax, esi
          pop esi
          ret
```

Next pair is `CPerceivedThingDef` `004F0DDC` / `004F0DE3`
factory `004D7EB6` (**PROVEN** name/sites, not shipped).
