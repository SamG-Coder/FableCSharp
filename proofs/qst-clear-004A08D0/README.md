# `004A08D0` clear of `world+184` / `+172` / `+196` (flag 1)

Investigation only. No production `src/` edits.

Question: exact `004A08D0` behavior when `004A0D90` flag = 1
(`FinalAlbion.qst`). What does it clear at `world+184` / `+172` /
`+196`? Element type, allocator, sizes. Flag = 0 skip. Any other
fields zeroed?

Statuses: **PROVEN** / **PARTIAL** / **UNREAD**.

Authority: `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00480000.txt`
`004A08D0` / `004A0D90` / `004A1840` / `004A68AE` / `004A6BB1`;
`listing-00400000.txt` `0043336A` / `00433530` / `00414E00`;
`listing-00980000.txt` `0099EAE0` / `0099E9B0`;
`listing-00bc0000.txt` `00BFEA0E` / `00BFEA14`.

Also: `listing-00480000.txt` `004AA580` / `004ABD90` / `004ADB50` /
`004A89D0` / `004A113B`.

---

## Verdict

**`004A0D90` flag 1 calls `004A08D0` (`ecx` = `CWorld`). Flag 0
jumps over it. PROVEN.**

`004A1840` `"Load Quests"` pushes **1** for
`Data\Levels\FinalAlbion.qst` (`004A1931`) and **0** for intern
`0x01238F38` `Data\Levels\GlobalQuests.qst` (`004A1991`).
**PROVEN.**

`004A08D0` is a **vector `erase(begin,end)` / `clear()`** of three
MSVC-style triples. It does **not** free the buffers, does **not**
write begin/capacity, does **not** zero neighboring world fields,
does **not** walk quest-manager `+44`. **PROVEN.**

| Offset | Kind | Live pointers | Element | Size | `clear` helper |
|---|---|---|---|---:|---|
| `+184` | `AddQuest` names | begin `+184`, end `+188`, cap `+192` | CString | **4** | `0043336A` |
| `+172` | `AddQuest` TRUE names | begin `+172`, end `+176`, cap `+180` | CString | **4** | `0043336A` |
| `+196` | `AddTestQuest` cards | begin `+196`, end `+200`, cap `+204` | struct | **28** | `004AA580` + `004ABD90`; then `[+200]=begin` |

After first-load `CWorld` ctor (`004A68AE` all three triples `=0`)
the clears are empty-range no-ops, then FinalAlbion parse fills
them. GlobalQuests **appends**. **PROVEN.**

---

## 1. Flag gate (`004A0D90`)

`listing-00480000.txt`:

```
004A0D90  mov al, [esp+8]     ; stdcall arg1 = flag (arg0 = path)
004A0D94  sub esp, 0x108
004A0D9A  test al, al
004A0D9C  push ebp
004A0D9D  push esi
004A0D9E  mov ebp, ecx        ; CWorld
004A0DA0  je 004A0DA7         ; flag 0 → skip
004A0DA2  call 004A08D0       ; thiscall ecx=world
004A0DA7  mov esi, [esp+276]  ; path
…
004A183A  ret 8
```

`test al, al` / `je`: only the low byte of the flag is tested.
A push of `0` skips; a push of `1` calls. **PROVEN.**

`004A1840` sites:

```
004A1931  push 1
004A1933  lea ecx, [ebp-144]   ; FinalAlbion.qst CString
004A1939  push ecx
004A193A  mov ecx, esi         ; world
004A193C  call 004A0D90

004A1991  push 0
004A1993  lea eax, [ebp-172]   ; 0x01238F38 GlobalQuests.qst
004A1999  push eax
004A199A  mov ecx, esi
004A199C  call 004A0D90
```

Missing file (`00999230` / `je`) never reaches `004A0D90`.
**PROVEN.**

---

## 2. `004A08D0` body (whole function)

`004A08D0`…`004A093F` `ret`. `thiscall`, no stack args.
`push ecx` is a 4-byte scratch for the `+196` dummy pointer.

```
004A08D0  push ecx
004A08D1  push esi
004A08D2  mov esi, ecx                    ; world
004A08D4  mov eax, [esi+188]              ; +184.end
004A08DA  mov edx, [esi+184]              ; +184.begin
004A08E0  lea ecx, [esi+184]
004A08E7  push eax
004A08E8  push edx
004A08E9  call 0043336A                   ; erase(begin,end) CString×4
004A08EE  mov eax, [esi+176]              ; +172.end
004A08F4  mov edx, [esi+172]              ; +172.begin
004A08FA  lea ecx, [esi+172]
004A0900  push eax
004A0901  push edx
004A0902  call 0043336A                   ; erase(begin,end) CString×4
004A0907  mov ecx, [esi+200]              ; +196.end  (source first)
004A090D  mov eax, [esi+196]              ; +196.begin
004A0913  push 0
004A0915  lea edx, [esp+15]
004A0919  push edx                        ; dummy
004A091A  push eax                        ; dest = begin
004A091B  mov edx, ecx                    ; source last = end
004A091D  call 004AA580                   ; copy [end,end) → begin; eax=begin
004A0922  mov edx, [esi+200]              ; old end
004A0928  mov edi, eax                    ; new end = begin
004A092A  lea eax, [esp+11]
004A092E  push eax
004A092F  mov ecx, edi
004A0931  call 004ABD90                   ; dtor [begin, old_end)
004A0936  mov [esi+200], edi              ; end = begin
004A093C  pop edi
004A093D  pop esi
004A093E  pop ecx
004A093F  ret
```

Writes **one** world dword: `[world+200]`. The two `0043336A`
calls write `[vector+4]` (end) only. **PROVEN.**

No `00BFEA0E` / `00BFEA14` in this function. Capacity pointers
`+180` / `+192` / `+204` stay. Begin pointers stay. **PROVEN.**

---

## 3. `world+184` and `world+172` — CString vector, elem 4

### Layout

Ctor `004A68AE`:

```
[esi+172]=[esi+176]=[esi+180]=0
[esi+184]=[esi+188]=[esi+192]=0
[esi+196]=[esi+200]=[esi+204]=0
```

Dtor `004A6BE5`…`004A6C44`: walk `[begin,end)` `0099EAE0` then
`00BFEA14(begin)` if nonzero. Step **`add edi, 4`**. **PROVEN.**

Push during `AddQuest` (`004A1072`…`004A10F1`):

```
lea esi, [ebp+184]
cmp [ebp+188], [ebp+192]     ; end vs cap
jne in-place:
  0099EC30(end, name)        ; CString copy-ctor
  add [esi+4], 4             ; end += 4
else:
  00433530(..., 1)           ; grow + insert 1
```

TRUE branch repeats the same at `lea esi, [ebp+172]`.
**PROVEN** (`004A10B2` `test bl,bl`).

`sar eax, 2` in `0043336A` / `00433530` is the same 4-byte stride.

### Element type

4-byte Lionhead CString (pointer to refcounted blob).

| Op | VA | Role |
|---|---|---|
| copy-ctor | `0099EC30` | in-place push / grow |
| assign | `0099EFB0` | `0043336A` slide of survivors |
| dtor | `0099EAE0` → `0099E9B0` | slot `mov [edi], 0` after release |

`0099E9B0`: if `[slot]==0` return; else `dec [blob+13]`; last
ref frees char data `00BFEB1C` then blob `00BFE9BC`; **always**
`mov [edi],0` on the 4-byte slot. **PROVEN.**

### `0043336A` = `erase(first, last)`

`listing-00400000.txt` `0043336A`…`004333B0` `ret 8`. `ecx` =
triple. Args: first, last.

```
count = (end - last) >> 2          ; survivors after last
slide survivors onto first via 0099EFB0, +4
00414E00(new_end, old_end)         ; 0099EAE0 each leftover
[ecx+4] = new_end                  ; end only
```

`004A08D0` passes `first=begin`, `last=end` → survivor count 0 →
dtor `[begin, old_end)` → `end=begin`. That is `clear()`.
**PROVEN.**

`00414E00`: loop `0099EAE0` / `add esi, 4` until `esi==edi`.
**PROVEN.**

### Allocator (buffer, not used by `004A08D0`)

Grow `00433530` (`ret 20`):

```
push (new_cap * 4)
call 00BFEA0E                      ; cdecl alloc
…
push old_begin
call 00BFEA14                      ; cdecl free
[begin] = new_buf
[end]   = new_end
[cap]   = new_buf + new_cap*4
```

World dtor uses the same `00BFEA14` on `+184` / `+172` begin.

Thunks (`listing-00bc0000.txt`):

```
00BFEA0E  jmp [0x1440158]
00BFEA14  jmp [0x1440154]
```

These are the Lionhead vector `_Allocate` / `_Deallocate` pair
(same as `proofs/quest-manager-plus44`, `proofs/load-job`).
Exact CRT export name (`malloc` vs `operator new`) is **UNREAD**
(no IAT name dump in `out/00-index/imports.txt`).

CString *payload* alloc is a different pair (`00BFEA1A` /
`00BFE9BC` / `00BFEB1C`). `004A08D0` only hits that indirectly
via `0099EAE0` when a live name is destroyed.

---

## 4. `world+196` — 28-byte `AddTestQuest` vector

### Push (`004A16E4`)

```
lea esi, [ebp+196]
cmp [ebp+200], [esi+8]            ; end vs cap (+204)
jne in-place:
  004A89D0(end, card)             ; copy-ctor 28 B
  add [esi+4], 28                 ; end += 28
else:
  004ADB50(..., 1)                ; grow + insert
```

Stride **28** is **PROVEN** (`add [esi+4], 28`, `imul eax, 28` in
`004ADB50`, `0x92492493` signed `/28` in `004AA580` /
`004ADB50`).

### Element layout (`004A89D0` copy-ctor / `004ABD90` dtor)

28-byte record:

| Off | Type | Copy | Dtor |
|---:|---|---|---|
| +0 | CString | `0099EC30` | `0099EAE0` |
| +4 | CString | `0099EC30` | `0099EAE0` |
| +8 | POD dword | `mov [dst+8], [src+8]` | **none** |
| +12 | CString | `0099EC30` | `0099EAE0` |
| +16 | CString | `0099EC30` | `0099EAE0` |
| +20 | CString | `0099EC30` | `0099EAE0` |
| +24 | CString | `0099EC30` | `0099EAE0` |

`004ABD90`: `ecx`=first, `edx`=last, unused `ret 4` dummy.
Loop `+24,+20,+16,+12,+4,+0` then `add esi, 28`. **PROVEN.**

`+8` is filled from `009BA540` (`"Error parsing integer"`) during
`AddTestQuest` (`004A12F0` / `mov [esp+252], esi`). Meaning of
that integer (card id / count / …) is **PARTIAL**.

Named strings on the card (HSP / `.end` / `.ini` / …) are
**PARTIAL** here; store-only fact is **PROVEN**
(`proofs/qst-first-load`).

### Clear path

`004AA580` `ret 12`: copy `[ecx, edx)` of 28-byte records to dest
arg0 using `0099EFB0` on the six CStrings and `mov` of `+8`.
Count `(edx-ecx)/28`. Empty range returns dest.

`004A08D0` sets `ecx=edx=end`, dest=`begin` → copy 0, `eax=begin`.
Then `004ABD90(begin, old_end)` dtors live cards. Then
`[world+200]=begin`. **PROVEN** `clear()`.

POD `+8` of destroyed records is **not** written to 0 (dtor skips
it). Those bytes sit in unused capacity, not in the live range.
**PROVEN** skip; **UNREAD** whether later reuse always
overwrites `+8` before read.

### Allocator

Grow `004ADB50` `ret 20`: `00BFEA0E(new_cap*28)`, copy via
`004AA060` / `004A89D0`, `004ABD90` old range, `00BFEA14(old_begin)`,
write begin/end/cap. **PROVEN.** Same `00BFEA0E`/`00BFEA14` pair
as the CString vectors. `004A08D0` does not call them.

World dtor: `004ABD90(begin, end)` then `00BFEA14(begin)`.
**PROVEN.**

---

## 5. Other fields — **not** zeroed

`004A08D0` does **not**:

- write `+172` / `+176` / `+180` / `+184` / `+188` / `+192` /
  `+196` / `+204` (only `+200` and the two `end` dwords via
  `0043336A`)
- `00BFEA14` the buffers
- touch `world+208` (next ctor dword, `004A68DE`)
- call `004B2850` / walk `[0x13B89FC]+44` (quest-manager name
  vector). `AddQuest` still `push_back`s there **after** the
  clear (`004A10F6`). Re-parse flag 1 does **not** reset manager
  `+44`. **PROVEN** absent from `004A08D0`; manager contents
  across a second flag-1 parse are **UNREAD** on this no-save walk
  (ctor already empty)
- zero the three begin pointers to NULL (empty but allocated
  vectors stay allocated)

CString **slots** in the discarded range **are** set to 0 by
`0099E9B0`. That is element dtor, not a world-field store.
**PROVEN.**

---

## 6. First-load after Leave

```
004A67D0 CWorld ctor
  004A68AE  +172/+184/+196 triples = 0
00416ABA 004A1840
  004A0D90(FinalAlbion.qst, 1)
    004A08D0          ; empty-range clear
    AddQuest → +184; TRUE → +172; 004B2850
    AddTestQuest → +196 only
  004A0D90(GlobalQuests.qst, 0)
    skip 004A08D0     ; append
```

**PROVEN.** Host `LoadQuestDefs` notes the flag-1 clear and
`.Clear()`s C# lists; it still drops `AddTestQuest` / `+196`.
That host gap is **PARTIAL** vs this native function (see
`proofs/qst-first-load`).

---

## Classifications (short)

1. **Flag 1 → `004A08D0`; flag 0 skip — PROVEN.**
   `004A0D90` `test al,al` / `je 004A0DA7`. FinalAlbion `push 1`,
   GlobalQuests `push 0`.
2. **`+184` / `+172` — PROVEN.** `std::vector`-shaped CString
   triples, elem **4**, `clear` = `0043336A(begin,end)`. Buffer
   alloc/free `00BFEA0E` / `00BFEA14` on **grow/dtor only**.
3. **`+196` — PROVEN.** Vector of **28**-byte cards
   (6 CString + POD dword at `+8`). `clear` = empty `004AA580`
   then `004ABD90` then `[+200]=begin`. Same allocator pair on
   grow/dtor only.
4. **No other world fields written — PROVEN.** One explicit
   `[+200]` store plus two `end` updates. Not manager `+44`.
5. **CRT symbol behind `00BFEA0E`/`00BFEA14` — UNREAD.**
6. **`AddTestQuest` field names / `+8` integer role — PARTIAL.**
)
