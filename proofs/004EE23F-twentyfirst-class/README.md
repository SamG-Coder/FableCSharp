# `004EE23F` twenty-first `009B0AC0` / `0044C6B0` is `CBedDef`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` → `"Init Game"`
`0042F491` → `00418DCA` → `[vtbl+4]`
`004184BD` → `00418585` `004EE23F`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: recover `CBedDef` factory
`004DA7F3`, ctor `004D7A25`, size 60, vtbl.
Then `CStealthDef` `004F0F48` factory
`004D7EFC` if time.

| Field | Value | Class |
| --- | --- | --- |
| `0044C6B0` | `004F0E92` | **PROVEN** |
| `009B0AC0` | `004F0E99` | **PROVEN** |
| Factory | `004DA7F3` `00BFEA1A(60)` then `jmp 004D7A25`; vtbl **`0123E8BC`** | **PROVEN** |
| Ctor | `004D7A25` `0044C0C0` then `[esi]=0123E8BC`; `[esi+40..52]=-1` | **PROVEN** |
| Size | **60** (`push 60` at factory; vtbl[20] `004D7A46` `push 60; pop eax; ret`) | **PROVEN** |

Authority: `Fable.exe` listing
`listing-004c0000.txt` `004DA7F3`,
`004D7A25`, `004F0E6A`; `fn 004F0E92`;
`proofs/004EE23F-remaining-pairs` row 21;
`proofs/004EE23F-twentieth-class`.
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x012440F8` **`CBedDef`**.
`assembly/exe/00-index/vtbl.tsv`
`0x0123E8BC`.

Listing string at `004F0E6A` is **`CBedDef`**
(not invented). Shape-2 (`push` + `0042DAE0`).

```
004F0E6A  push "CBedDef"
004F0E7A  push 0x4DA7F3
004F0E8C  call 0042DAE0
004F0E92  call 0044C6B0
004F0E99  call 009B0AC0
```

```
004DA7F3  push 60
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004DA806
          mov ecx, eax
          jmp 004D7A25
004DA806  xor eax, eax
          ret

004D7A25  push esi
          mov esi, ecx
          call 0044C0C0
          or eax, -1
          mov [esi], 0x123E8BC
          mov [esi+40], eax
          mov [esi+44], eax
          mov [esi+48], eax
          mov [esi+52], eax
          mov eax, esi
          pop esi
          ret

004D7A46  push 60
          pop eax
          ret
```

Next pair is `CStealthDef` `004F0F48` / `004F0F4F`
factory `004D7EFC` `00BFEA1A(72)` then
`0044C0C0`; vtbl **`0123AB1C`**
(**PROVEN** name/sites/factory/size/vtbl,
not shipped).

---

## Evidence

`listing-004c0000.txt` after twentieth
`CPerceivedThingDef` `004F0DE3`:

One unnamed `004D2EF0` row (`push 0x4D35E2`
at `004F0E16`, helper `004D35FF` at
`004F0E04`). Then the twenty-first pair.

`004F0E6A` `68 F8 40 24 01` =
`push 0x012440F8`. `strings.tsv`:

```
0x012440F8	0xE440F8	CBedDef
```

Same listing annotates the immediate as
`"CBedDef"`. `xrefs.tsv` `0x012440F8` first
hit `0x004F0E6B` `fn=0x004EE137`.

`004F0E7A` `push 0x4DA7F3` then
`0042DAE0` / `0044C6B0` / `009B0AC0`.
`abs.tsv` `0x004F0E7A` → `0x004DA7F3`.
Matches remaining-pairs row 21.

`004DA7F3` is the same thunk shape as
nineteenth `004E0B4B`: `00BFEA1A` with
immediate **60**, null → `xor eax, eax; ret`,
else `mov ecx, eax; jmp 004D7A25`.

`004D7A25` calls `0044C0C0`, writes
`[esi]=0x0123E8BC`, then four dwords at
`+40` `+44` `+48` `+52` from `or eax, -1`.
No other stores. Object is 60 bytes
(`00BFEA1A(60)` plus the size helper
immediately after the ctor).

`vtbl.tsv` `0x0123E8BC` slot 20 is
`004D7A46`. Listing:

```
004D7A46  6A 3C  push 60
004D7A48  58     pop eax
004D7A49  C3     ret
```

Slot 0 is `004DA809` (`mov [esi], 0x1230BA0`
then `009FC550`). Slots 1–17 / 21–24 are
the shared `0042D930`…`0042DAA0` /
`009ACE90` / `009FBEF0` / `009ACAB0` /
`009ACB20` family. No invented names.

---

## Original

Twenty-first Add Def Class on `004EE23F`:

1. `0099EBF0` name `"CBedDef"`.
2. `0042DAE0` packs factory `004DA7F3`.
3. `0044C6B0` `004F0E92`.
4. `009B0AC0` `004F0E99`.

Factory alloc 60, ctor `004D7A25`.
Base `0044C0C0`. Vtbl `0123E8BC`.
Four extra dwords `+40…+52` = `-1`.

One unnamed CTC between twentieth and
this pair. One unnamed CTC after
(`push 0x4D3612` at `004F0ECC`, helper
`004D362F` at `004F0EBA`) then
`CStealthDef`.

Not Oakvale. Not a Thing instance. Not a
file I/O site.

---

## Host

`EngineLifecycle` already Notes nineteenth
`CCreatureModeDef` (`004F0D26` / `004E0B4B`
/ `004DE7DC` / size 64 / vtbl `01241704`)
and twentieth `CPerceivedThingDef`
(`004F0DDC` / `004D7EB6` / `0044C0C0` /
size 80 / vtbl `0123AA9C`).

Twenty-first constants and Notes are
already in host: site `004F0E92`, factory
`004DA7F3`, ctor `004D7A25`, vtbl
`0123E8BC`, size 60, name `CBedDef`.
`AddFirstDefClass` tail Notes the same
pack / Add Def Class / factory / LoadDef /
`009FC4F0` `[this+40]` line, then
`TwentyFirstDefClassRegistered = true` and
**returns**.

Note-only + flag. **Not** a live 60-byte
object. `+40…+52` writes are **UNREAD**
in the host object (there is none).

Host Notes **MATCH** the listing sites.
Live ctor is **LEFTOVER**.

---

## Gap (host leftover after 19th)

After nineteenth `004F0D2D` native still
runs:

| After 19th | Native | Host after 19th |
| --- | --- | --- |
| 1 unnamed `004D2EF0` (`0x4D359C`) | listing `004F0D48`…`004F0DAE` | **LEFTOVER** |
| 20 `CPerceivedThingDef` `004F0DDC` / `004D7EB6` size 80 vtbl `0123AA9C` | **PROVEN** (`004EE23F-twentieth-class`) | Note-only **MATCH**; live 80-byte object **LEFTOVER** |
| 1 unnamed `004D2EF0` (`0x4D35E2`) | listing `004F0DFE`…`004F0E64` | **LEFTOVER** |
| 21 `CBedDef` `004F0E92` / `004DA7F3` `jmp 004D7A25` size 60 vtbl `0123E8BC` | **PROVEN** (this file) | Note-only **MATCH**; live 60-byte object **LEFTOVER** |
| 1 unnamed `004D2EF0` (`0x4D3612`) | listing `004F0EBA`…`004F0F1A` | **LEFTOVER** |
| 22 `CStealthDef` `004F0F48` / `004D7EFC` size 72 vtbl `0123AB1C` | **PROVEN** below | **LEFTOVER** (not shipped) |
| rows 23…111 | remaining-pairs | **LEFTOVER** |

`AddFirstDefClass` returns after the
twenty-first Notes. First leftover **pair**
after this ship is `CStealthDef`.

---

## `CStealthDef` (row 22; time allowed)

| Field | Value | Class |
| --- | --- | --- |
| `0044C6B0` | `004F0F48` | **PROVEN** |
| `009B0AC0` | `004F0F4F` | **PROVEN** |
| Factory | `004D7EFC` `00BFEA1A(72)` then `0044C0C0`; vtbl **`0123AB1C`** | **PROVEN** |
| Size | **72** (`push 72` at factory; vtbl[20] `004D3654` `push 72; pop eax; ret`) | **PROVEN** |

`strings.tsv` `0x012440EC` **`CStealthDef`**.
Listing `004F0F20` `68 EC 40 24 01`.
Shape-2. No `jmp` thunk: ctor is in-line
like twentieth.

```
004F0F20  push "CStealthDef"
004F0F30  push 0x4D7EFC
004F0F42  call 0042DAE0
004F0F48  call 0044C6B0
004F0F4F  call 009B0AC0
```

```
004D7EFC  push esi
          push 72
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D7F1C
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123AB1C
          mov eax, esi
          pop esi
          ret
004D7F1C  xor eax, eax
          pop esi
          ret
```

`vtbl.tsv` `0x0123AB1C` slot 20 = `004D3654`:

```
004D3654  push 72
          pop eax
          ret
```

No extra dword stores after the vtbl write.
Next pair after this is `CTrophyDef`
`004F10D4` / `004F10DB` factory `004D7F7B`
(remaining-pairs row 23; factory body
**UNREAD** here).
