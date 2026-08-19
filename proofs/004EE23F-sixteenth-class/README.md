# `004EE23F` sixteenth `009B0AC0` / `0044C6B0` is `CSpotLightDef`

| Field | Value | Class |
| --- | --- | --- |
| `0044C6B0` | `004F07AC` | **PROVEN** |
| `009B0AC0` | `004F07B3` | **PROVEN** |
| Factory | `004D7CB9` `00BFEA1A(68)` then `0044C0C0`; vtbl **`0123A88C`** | **PROVEN** |

Authority: `Fable.exe` listing `004D7CB9`;
`fn 004F0784`; `fn 004F07AC`;
`proofs/004EE23F-remaining-pairs` row 16;
`proofs/004EE23F-fifteenth-class`.

Listing string at `004F0784` is **`CSpotLightDef`**
(not invented). Shape-2 (`push` + `0042DAE0`).

```
004F0784  push "CSpotLightDef"
004F0794  push 0x4D7CB9
004F07A6  call 0042DAE0
004F07AC  call 0044C6B0
004F07B3  call 009B0AC0
```

```
004D7CB9  push esi
          push 68
          call 00BFEA1A
          …
          call 0044C0C0
          mov [esi], 0x123A88C
          ret
```

Next pair is `CClockDef` `004F0862` / `004F0869`
factory `004E4477` (**PROVEN** name/sites, not shipped).
