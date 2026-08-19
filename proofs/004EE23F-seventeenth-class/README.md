# `004EE23F` seventeenth `009B0AC0` / `0044C6B0` is `CClockDef`

| Field | Value | Class |
| --- | --- | --- |
| `0044C6B0` | `004F0862` | **PROVEN** |
| `009B0AC0` | `004F0869` | **PROVEN** |
| Factory | `004E4477` `00BFEA1A(56)` then `jmp 004E380E`; vtbl **`01242C34`** | **PROVEN** |

Authority: `Fable.exe` listing `004E4477`;
`fn 004F083A`; `fn 004E380E`;
`proofs/004EE23F-remaining-pairs` row 17;
`proofs/004EE23F-sixteenth-class`.
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01244134` **`CClockDef`**.

Listing string at `004F083A` is **`CClockDef`**
(not invented). Shape-2 (`push` + `0042DAE0`).

```
004F083A  push "CClockDef"
004F084A  push 0x4E4477
004F085C  call 0042DAE0
004F0862  call 0044C6B0
004F0869  call 009B0AC0
```

```
004E4477  push 56
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004E448A
          mov ecx, eax
          jmp 004E380E
004E448A  xor eax, eax
          ret

004E380E  push esi
          mov esi, ecx
          call 0044C0C0
          lea ecx, [esi+40]
          mov [esi], 0x1242C34
          call 00430345
          mov eax, esi
          pop esi
          ret
```

Next pair is `CHeroDef` `004F0918` / `004F091F`
factory `004D7CFF` (**PROVEN** name/sites, not shipped).
