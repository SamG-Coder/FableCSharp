# `00851770` New Profile type-37 seed

Investigation only. No production `src/` edits.

Question: `00851770` binds the New Profile type-37 edit box.
What first-seen string does it seed? Is `"Default"` from
`004069E0` / `0x0122DE80` only when `[0x13B86A0]==0`?
Does `00851700` zero `+4`/`+5` first?

Authority: `Fable.exe` complete dump
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00840000.txt`
(`00851700` / `00851770`);
`listing-00400000.txt` (`004069E0` / `00406A20` / `00406A5D` /
`0040D2A0` / `0042EA8F` / `0042E98F` / `0042F75E` / `00415260` /
`00415070`);
`listing-00580000.txt` (`00596917`);
`listing-00980000.txt` (`0099B6B0` / `0099EBF0` / `0099AED0`);
`listing-009c0000.txt` (`009C85A0` / `009C95E0`);
`tools/Fable.ExeIndex/out/00-index/strings.tsv`, `xrefs.tsv`,
`sections.txt`;
`tools/Fable.ExeIndex/Program.cs` `ExtractStrings` (ASCII
`32..126`, length `5..180` only);
`proofs/0041E6D3-frontend-gate/README.md`;
`proofs/texture-library-open/README.md`.

Do not re-prove `0x126` / `00851920` / Leave.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

---

## Verdict

| Claim | Class |
| --- | --- |
| `00596917` allocs 16 bytes, `00851700`, stores `[ui+96]`, then `00851770` | **PROVEN** |
| `00851770` finds `"UI_NEW_PROFILE_EDIT_BOX"`, `vtbl+260`, `cmp eax, 37`, else skip | **PROVEN** |
| First-seen seed is `004069E0` into `vtbl+572` | **PROVEN** |
| First-seen display letters are UTF-16 `"Default"` from `0x0122DE80` | **DISPROVEN** (that VA is the **both-null** fallback; first-seen does not take it) |
| First-seen `004069E0` uses `"TEXT_GUI_PROFILE_DEFAULT"` via `[0x13B871C]+96` `009C95E0` | **PROVEN** path; payload **UNREAD** (not in the exe dump) |
| `"Default"` from `004069E0` / `0x0122DE80` **only** when `[0x13B86A0]==0` | **DISPROVEN** |
| `0x0122DE80` is used when **both** `[0x13B86A0]==0` **and** `[0x13B871C]==0` | **PROVEN** |
| `0x0122DE80` bytes are UTF-16 `"Default"` | **PARTIAL** (wide `0099B6B0`, 16-byte slot to `0x0122DE90`, absent from ASCII `strings.tsv`; listing does not emit the wchar bytes) |
| `00851700` zeros `+4` then `+5` as the first field stores after `0099AED0` | **PROVEN** |

First-seen New Profile therefore seeds whatever
`TEXT_ENGLISH_MAIN` (or the live `TEXT_*_MAIN`) holds for
`TEXT_GUI_PROFILE_DEFAULT`. Miss copies `[0x13BCA24]`, **not**
`0x0122DE80`. Host / `FORWARD_TREE` “`[0x13B86A0]=0` →
`0x122DE80` Default” skips the retail bank arm.

---

## 1. Bind (`00851770`)

`00596917` (`listing-00580000.txt`):

```
00596940  push 16
00596942  call 00BFEA1A
…
00596962  call 00851700
00596967  jmp 0059696B
00596969  xor eax, eax
0059696B  mov ecx, eax
0059696D  mov [edi+96], eax
00596970  call 00851770
```

`00851770` (`listing-00840000.txt`):

```
0085177B  mov ecx, esp
0085177D  push -1
0085177F  push "UI_NEW_PROFILE_EDIT_BOX"
00851784  call 0099EBF0
00851789  mov ecx, [esi+8]
0085178C  call [edi+12]
0085178F  mov edi, eax
00851791  test edi, edi
00851793  je 00851854
0085179D  call [edx+260]
008517A3  cmp eax, 37
008517A6  jne 00851854
008517B0  mov [esi+12], edi
008517B5  push eax              ; dest local
008517B6  call 0040D2A0         ; ret 0; dest stays
008517BB  mov ecx, eax
008517BD  call 004069E0         ; ret 4; fills dest
008517C2  mov ecx, [esi+12]
008517C5  push eax
008517C6  call [edi+572]
…
00851834  push 33
00851836  call [eax+12]
00851842  push 34
00851844  call [edx+12]
```

Type-37 is the **runtime** `vtbl+260` check. Persist
`frontend.bin` type byte is outside this dump (**PARTIAL**
here). `0040D2A0` is `[0x13B7D4C]` get-or-ctor; `004069E0`
does not read that `ecx` for the string branch.

`xrefs.tsv`: `0x012759A0` `fn=0x00851770`
`UI_NEW_PROFILE_EDIT_BOX`.

---

## 2. `004069E0` is three-way, not `[0x13B86A0]` only

```
004069E0  push ecx
004069E1  push esi
004069E2  mov esi, [0x13B86A0]
004069E8  test esi, esi
004069EA  push edi
004069EB  je 00406A20
; [game] ≠ 0
004069EF  push "TEXT_GUI_PROFILE_DEFAULT"
…
00406A01  mov ecx, [esi+20]
00406A0A  call 009C95E0
00406A18  mov eax, edi
00406A1D  ret 4

00406A20  mov esi, [0x13B871C]
00406A26  test esi, esi
00406A28  je 00406A5D
; game == 0, retail ≠ 0
00406A2C  push "TEXT_GUI_PROFILE_DEFAULT"
…
00406A43  mov ecx, [esi+96]
00406A47  call 009C95E0
00406A55  mov eax, edi
00406A5A  ret 4

00406A5D  mov esi, [esp+16]     ; dest
00406A61  push 0x122DE80
00406A66  mov ecx, esi
00406A68  call 0099B6B0
00406A6E  mov eax, esi
00406A72  ret 4
```

`0x0122DE80` is **only** the `je 00406A5D` arm: both
globals 0. `[0x13B86A0]==0` alone is **not** enough.

Neighbor `00406A80` (`TEXT_GUI_SAVE_EMPTY_SLOT`) has **no**
`[0x13B871C]` arm: game 0 → `push 0x122DF5C` immediately.
`004069E0` is the odd one.

`009C95E0` miss (`listing-009c0000.txt`):

```
009C965F  mov esi, [esp+12]
009C9663  push 0x13BCA24
009C966A  call 0099B720
```

Miss ≠ `0x122DE80`.

---

## 3. First-seen takes the retail bank, not `0x122DE80`

| Global | First-seen | Evidence |
| --- | --- | --- |
| `[0x13B86A0]` | **0** until Leave / Init Game | `proofs/0041E6D3-frontend-gate` two writers `004184D1` / `00417663` |
| `[0x13B871C]` | **retail** before first pump | `0042F761  mov [0x13B871C], esi` (`0042F75E`) |

Retail ctor `0042EA8F` zeros `+96` (`0042EAED`, `ebx=0`).
Same-frame frontend `0042E98F` then installs a bank **before**
Press Start / New Profile:

```
0042E9DA  push 0x214
0042E9DF  call 00BFEA1A
0042E9EB  call 009C85A0          ; text-bank ctor (vtbl 0x129B4D4)
0042E9F4  lea edi, [esi+96]
0042E9F7  push eax
0042E9FA  call 00403E40          ; [retail+96] = bank
0042EA0B  call 00415260          ; TEXT_*_MAIN name
0042EA13  call [ebx+4]           ; bank load
```

`00415260` switch default is `"TEXT_ENGLISH_MAIN"`
(`00415300`). `00415070` caches `[0x13B8684]` by probing
`009A80B0` for those bank names.

First-seen `00851770` → `004069E0`:
`[0x13B86A0]==0`, `[0x13B871C]≠0`, `[retail+96]≠0` →
`TEXT_GUI_PROFILE_DEFAULT` lookup. **Not** `0x122DE80`.

The UTF-16 payload lives in `lang/English/text.big`
(`TEXT_ENGLISH_MAIN`), which this exe dump does not contain.
**UNREAD** here. `strings.tsv` has the **key**
`0x0122DF40  TEXT_GUI_PROFILE_DEFAULT` only.

---

## 4. `0x0122DE80` itself

`0099B6B0` (`listing-00980000.txt`) assigns a wide source
(`cmp [eax], 0` then `0099B3C0`). Callers:

| Site | Immediate |
| --- | --- |
| `00406A61` | `0x122DE80` (`004069E0` both-null) |
| `004065D5` | `0x122DE80` (tiny assign helper) |
| `0040D229` | `0x122DE80` |
| `004065F5` | `0x122DE90` (next slot, +16) |

`.rdata` VA `0x0122DE80` (`sections.txt`: `.rdata`
`rva=0xE2D000` → `0x0122D000`). ASCII `ExtractStrings` has
`SymbolHeight` at `0x0122DE20` then `Unable to find closing quote`
at `0x0122DF04`. No ASCII `"Default"` at `0x0122DE80`. A
7-char ASCII run would have been indexed (length 5–180).

16 bytes `0x0122DE80`–`0x0122DE90` is the size of UTF-16
`L"Default\0"`. Linear `listing-01200000.txt` has **no**
`0122DE*` instruction start, so the dump never prints those
bytes. Letters **PARTIAL**. Using that VA on first-seen
**DISPROVEN**.

---

## 5. `00851700` zeros `+4` / `+5` first

```
00851700  push esi
00851701  mov esi, ecx
00851703  call 0099AED0          ; base
00851708  mov ecx, [esp+8]       ; menu arg
0085170C  xor eax, eax
0085170E  mov [esi+4], al
00851711  mov [esi+5], al
00851714  mov [esi+12], eax
00851717  mov [esi+8], ecx
0085171A  mov eax, esi
0085171C  pop esi
0085171D  ret 4
```

After `0099AED0`, the first object stores are `+4=0`,
`+5=0`, then dword `+12=0`, then `+8=arg`. `00596917`
runs this **before** `00851770`. Same-tick `0059899A` still
skips while those bytes stay 0 (`FORWARD_TREE` §4).

---

## 6. What this does **not** say

- First-seen on-screen name is the English word Default.
  **UNREAD** (text.big).
- `004069E0` game arm (`[game+20]`) runs on first-seen.
  **DISPROVEN** (`[0x13B86A0]==0`).
- `00851770` posts `0x126`. **DISPROVEN**
  (`proofs/who-posts-0x126`).
- `0x0122DE80` is unused in the exe. **DISPROVEN** (three
  `push` sites; just not first-seen New Profile).
