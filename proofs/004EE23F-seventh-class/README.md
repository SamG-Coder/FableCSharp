# `004EE23F` seventh `009B0AC0` / `0044C6B0` is `CLookDef`

Investigation + host Note of the next Add Def Class
after `CInventoryItemDef`.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: After `CInventoryItemDef` `004EF244`
`009B0AC0`, what is the **next** `0044C6B0` /
`009B0AC0` on `004EE23F`? Confirm `CLookDef`
at `004EF386`. Factory size / ctor / vtbl.

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
after `004EF244`; factory `004D80E4`;
ctor `0044C0C0` in `listing-00440000.txt`.

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| Next `0044C6B0` after `004EF23D`? | **`004EF37F`**. | **PROVEN** |
| Next `009B0AC0` after `004EF244`? | **`004EF386`**. | **PROVEN** |
| Next class name? | **`CLookDef`**. Push at `004EF34C`. Factory imm `[ebp-1728]=0x4D80E4`. | **PROVEN** |
| CTC between sixth and seventh? | **Two.** `CTCCreatureExpression` `004EF260` / `0x4D2AA4`; `CTCLook` `004EF2DC` / `0x4D38F3`. | **PROVEN** |
| Factory ctor? | `004D80E4`: `00BFEA1A(88)` then `0044C0C0`; vtbl **`0123AE14`**. | **PROVEN** |
| Next leftover after this Note? | **`CReadableDef`** `004EF57A` / `004EF5AD` / `004EF5B4` / factory `0x4DAA0E`. | **PROVEN** later |

```
004EF34C  push "CLookDef"
004EF375  mov [ebp-1728], 0x4D80E4
004EF37F  call 0044C6B0
004EF384  mov ecx, eax
004EF386  call 009B0AC0
```

```
004D80E4  push esi
004D80E5  push 88
004D80E7  call 00BFEA1A
004D80F3  mov ecx, esi
004D80F5  call 0044C0C0
004D80FA  mov [esi], 0x123AE14
```
