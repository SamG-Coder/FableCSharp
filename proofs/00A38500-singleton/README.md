# `00A38500` BSS singleton at `[0x13B8A54]`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI` / `Father.Speak`.
This is the pre-Init static ctor of the symbol
table later filled by Init Subtitled Message.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN**.

Question: Static ctor `0121A630`
`mov ecx,0x13B8A54; call 00A38500`. Object
layout? vtbl `0x129CF84`? Size? `+20` list /
`+24` count as claimed? Fields zeroed? Same
type later filled by `00A39010`?

Authority: Fable.exe dump
`listing-01200000.txt` (`0121A610`–`0121A65C`,
`01228DC0`–`01228DE5`);
`listing-00a00000.txt` (`00A38500`–`00A3854B`,
`00A38550`–`00A3858D`, `00A385A0`–`00A38660`,
`00A38E50`–`00A38FAB`, `00A39010`–`00A39187`,
`00A39900`–`00A3994F`, `00A01A0C`–`00A01A4F`,
`00A38420`);
`listing-004c0000.txt` (`004CDB41`–`004CDB46`,
`004D1D50`–`004D1DA8`);
`listing-00980000.txt` (`0099A2F0`–`0099A2F8`,
`0099A300`);
`e8.tsv` dests `00A38500` / `00A39010` /
`00A38E50`;
`rtti.txt` / `strings.tsv` (no vtbl blob);
siblings `proofs/004CDB10-00A39010`,
`proofs/004CDB10-subtitled-body`.

`.rdata` has **no** listing. vtbl **slots**
are **UNREAD**. Imm `0x129CF84` is only in
`.text`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| `0121A630` `ecx=0x13B8A54` `00A38500`? | **Yes.** Sole static-ctor site. `atexit` `01228DD0` → `004D1D50`. | **PROVEN** |
| vtbl `0x129CF84`? | **Yes.** Ctor writes `[this]=0x129CF84` after a one-insn base `0x129A7C4`. | **PROVEN** store. Slots **UNREAD** |
| Size? | Heap sibling `push 36` / `00BFEA1A` then `00A38500`. Type **36**. BSS next object `0x13B8A7C` → stride **40**. | **PROVEN** 36. BSS pad **PROVEN** 4 |
| `+20` list / `+24` count? | **Yes.** Dummy node `00BFEA0E(20)`; count 0. Same pair cleared by fill / dtor. | **PROVEN** |
| Fields zeroed? | Written fields yes. `+16` and `+28` **not** stored. BSS is already 0. | **PARTIAL** |
| Same type as `00A39010`? | **Yes.** Same `this` (`0x13B8A54` or heap). Fill uses `+4` vector / `+20`/`+24` list. | **PROVEN** |

**Answer:** layout is a 36-byte table:
vtbl `0x129CF84`, 8-byte-pair vector at `+4`,
list `{head,count}` at `+20`/`+24`, byte
`+32`. `00A39010` fills that same type. It
does not construct it.

---

## 1. Static ctor site

`listing-01200000.txt`:

```
0121A610  mov ecx, 0x13B8A50     ; prior CString
0121A615  call 0099E4B0
…
0121A630  mov ecx, 0x13B8A54
0121A635  call 00A38500
0121A63A  push 0x1228DD0
0121A63F  call 004012BC          ; atexit
0121A644  pop ecx
0121A645  ret
…
0121A650  push -1
0121A652  push "OPINION_REACTION_MANAGER_INSTANCE"
0121A657  mov ecx, 0x13B8A7C     ; next CString
```

`e8.tsv` dest `00A38500`:

| Site | Role |
|---|---|
| `0121A635` | BSS ctor |
| `00A01A0C` | heap ctor (`push 36`) |

No other `E8`. **PROVEN** two instances, one
type. Init Game is **not** a ctor site.

`01228DD0`:

```
01228DD0  mov ecx, 0x13B8A54
01228DD5  jmp 004D1D50
```

Neighbors: `01228DC0` dtor of `0x13B8A50`;
`01228DE0` dtor of `0x13B8A7C`. **PROVEN**
BSS window `[0x13B8A54, 0x13B8A7C)` = **40**.

---

## 2. `00A38500` body

`listing-00a00000.txt` `00A38500`–`00A3854B`:

```
00A38500  push ebx
00A38501  push esi
00A38502  mov esi, ecx
00A38504  call 0099A2F0          ; [this] = 0x129A7C4
00A38509  mov [esi], 0x129CF84   ; derived vtbl
00A3850F  xor ebx, ebx
00A38511  mov [esi+4], ebx
00A38514  mov [esi+8], ebx
00A38517  mov [esi+12], ebx
00A3851A  mov [esi+17], bl
00A3851D  push 20
00A3851F  mov [esi+20], ebx
00A38522  call 00BFEA0E
00A38527  mov [esi+20], eax      ; list dummy
00A3852A  mov [esi+24], ebx      ; count = 0
00A3852D  mov [eax], bl
00A3852F  mov eax, [esi+20]
00A38532  mov [eax+4], ebx
00A38535  mov eax, [esi+20]
00A38538  mov [eax+8], eax       ; next = self
00A3853B  mov eax, [esi+20]
00A3853E  add esp, 4
00A38541  mov [eax+12], eax      ; prev = self
00A38544  mov [esi+32], bl
00A38547  mov eax, esi
00A38549  pop esi
00A3854A  pop ebx
00A3854B  ret
```

`0099A2F0` is `mov eax,ecx; mov [eax],0x129A7C4; ret`.
Base vtbl lives one insn. Derived store is
**`0x129CF84`**. **PROVEN**.

No `.rdata` listing, no `xrefs.tsv` hit on
`0x0129CF84`. Slot table / RTTI name
**UNREAD**. Do not attach
`CDefinitionManager` (`rtti.txt`
`0x01375C24`) without a COL.

---

## 3. Field map

| Off | Ctor | Later use | Meaning |
|---:|---|---|---|
| `+0` | `0x129CF84` | dtor `0099A300` → `0x1231710` | vtbl |
| `+4` | 0 | begin; `00A39900` frees | vector of `{u32,u32}` |
| `+8` | 0 | end (`sar 3` = count) | |
| `+12` | 0 | cap | |
| `+16` | **unwritten** | `00A38FF2` `[this+16]` → `009B8520` | byte (thread/lock id) |
| `+17` | 0 | dirty; `00A38636` sets 1 | byte |
| `+18`/`+19` | — | — | pad **UNREAD** |
| `+20` | dummy 20 B | `004CF810` / `00A39010` | list head |
| `+24` | 0 | `test` before clear | list count |
| `+28` | **unwritten** | no use in `00A38500`…`00A39187` | **UNREAD** |
| `+32` | 0 | `00A385A6` gate list insert | byte flag |

Dummy node (20 B): `[0]=0`, `[4]=0`,
`[8]=self`, `[12]=self`. Circular list.
**PROVEN** `+20` list / `+24` count.

`00A38550` / `00A39010` / `004D1D50` share
the same clear:

```
if ([this+24] != 0)
    004CF810([this+20].next)
    dummy.next = dummy.prev = dummy
    dummy+4 = 0
    [this+24] = 0
```

Dtor then `00BFEA14` the dummy and
`[this+4]` if non-null, then `0099A300`.
Highest dtor offset is `+24` plus the
`+4` buffer. Highest ctor store is `+32`.
Fits size 36.

`00A39900` (`ecx=this+4`) is **not** a
mutex. `sub esi,eax` is 0; the copy loop
is dead. It zeros begin/end/cap/`+13` and
frees the old begin. Sibling “lock” wording
is **DISPROVEN**. It is a vector reset.

`00A38420` looks up a CString crc in the
`+4` vector (`009B21F0`). Later
`004CDF91`… on `[0x13B8A54]` are lookups,
not this ctor.

---

## 4. Size

Heap sibling `00A019FC`:

```
00A019FC  push 36
00A019FE  call 00BFEA1A          ; IAT new
00A01A0A  mov ecx, eax
00A01A0C  call 00A38500
00A01A18  push eax
00A01A1B  call 00A01AA0          ; store ptr
…
00A01A47  mov ecx, [esi]
00A01A4F  call 00A39010          ; same type fill
```

`00BFEA1A` is IAT alloc (`004B4063-stub-layout`).
**PROVEN** user size **36**.

BSS stride 40 = 36 + 4 unused before the
next CString. Do not report 40 as the
in-memory type size.

---

## 5. Fields zeroed?

Ctor stores 0 to `+4`,`+8`,`+12`,`+17`,
`+20` (then overwritten), `+24`,`+32`.
List dummy bytes/dwords 0 except self
links.

Not stored:

| Off | Class |
|---|---|
| `+16` | **PARTIAL** — used later; BSS 0 at process start |
| `+18`/`+19` | **UNREAD** |
| `+28` | **UNREAD** in this type’s methods |
| `+33`…`+35` | pad in the 36-byte alloc **UNREAD** |

“All fields zeroed” is **DISPROVEN** as a
complete store list. Written fields are
zero. Relying on BSS for `+16` is
**PARTIAL**.

---

## 6. Same type as `00A39010`

`e8.tsv` dest `00A39010`: `004CDB46`,
`00A01A4F`. Dest `00A38E50`: `00A3910D`
only.

`004CDB41` `mov ecx,0x13B8A54` then
`call 00A39010`. That is the BSS object
`0121A630` already constructed.

`00A39010` (`ret 8`):

1. `00A39900` on `this+4` (empty vector)
2. if `[this+24]` clear the `+20` list
3. `0099B7D0` file-stack
4. buffer + `00A60410` token rewrite
5. `00A38E50(this, buf, arg1)` walks
   `"enum"` / `"Unexpected EOF while
   reading enum"`
6. pop / free

`00A385A0` inserts `{crc, value}` into
`this+4` and, if `[this+32]`, into the
`+20` list. Fill and ctor share the map.

Heap path is the same pair: `00A38500`
then `00A39010`. **PROVEN** same type.
**DISPROVEN** that `00A39010` constructs
`[0x13B8A54]`.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `0121A630` | static ctor | **PROVEN** |
| `00A38500` | in-place ctor | **PROVEN** |
| `0x13B8A54` | BSS `this` | **PROVEN** |
| `0x129CF84` | vtbl store | **PROVEN** |
| vtbl slots / RTTI name | — | **UNREAD** |
| `0x129A7C4` | base vtbl one insn | **PROVEN** then overwritten |
| type size 36 | heap `00BFEA1A` | **PROVEN** |
| BSS stride 40 | next at `0x13B8A7C` | **PROVEN** |
| `+20` / `+24` | list / count | **PROVEN** |
| all fields stored 0 | — | **DISPROVEN** (`+16`/`+28`) |
| `00A39010` | later fill, same type | **PROVEN** |
| `00A38E50` | `"enum"` parse | **PROVEN** nested |
| `00A01A0C` | heap same ctor | **PROVEN** |
| `004D1D50` / `01228DD0` | dtor | **PROVEN** |
| `00A39900` as lock | — | **DISPROVEN** — vector reset |
| Oakvale / `00DBDE40` | — | **DISPROVEN** |
