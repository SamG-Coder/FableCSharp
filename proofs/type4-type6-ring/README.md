# Type 6 and type 4 share one 52-byte store; `009F4ED0` is first-slot only

Investigation only. No production `src/` edits.

Question: Type 6 LMB-up `00A03D60` same 52-byte ring as type 4?
Same poll `009F4ED0` can dequeue 4 then 6?

Authority: dump `Fable.exe` `00A03D60` / `00A03C80` / `00A03B40` /
`00A66B10` / `00A66B20` / `00A66B70` / `00A66BC0` / `00A66F20` /
`00A66FD0` / `00AB5420` / `00AB5590` / `00AB58E0` / `009F4ED0` /
`009F4F10` / `009F4AC0` / `009F57A0` / `0042E3EE` / `00446330`
(`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00a00000.txt`,
`listing-00a40000.txt`, `listing-00a80000.txt`,
`listing-009c0000.txt`, `listing-00400000.txt`,
`listing-00440000.txt`); `e8.tsv` (`0x00AB55A8` →
`0x00A03D60`, `0x0042E449` → `0x009F4ED0`,
`0x0042E7FE` → `0x009F4F10`);
`proofs/type6-record-layout/README.md`,
`proofs/type4-enqueue-ring/README.md`,
`proofs/type6-same-poll-as-4/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not re-prove type 4 = LMB down → action 26, type 6 = LMB up
→ action 28, or the `+0` / DIK unused claim (`type6-record-layout`).

---

## Verdict

**Same 52-byte record and the same two stores as type 4. Not a
wrapping ring.** `00A03D60` fills the same dest that
`00A03C80` fills. Both ride `00AB58E0` → `00A66B20` into
the mouse device’s **256 × 52** linear array, then harvest
`009F57A0` copies every surviving slot into mux `[+16]`
with stride **52** and `inc [+28]`. Count on the device is
zeroed each poll (`00A66B10`). There is no second type-6
queue.

**`009F4ED0` cannot dequeue 4 then 6 by itself.** It copies
**only slot 0** (`rep movsd` 13 dwords) if `[mux+28] > 0`.
Type is not read. The next record is `009F4F10` (cursor++,
stride 52, skip **type 0** only). One frontend / player
walk is `009F4ED0` once, then `009F4F10` until miss. If
this harvest has type 4 then type 6, that walk yields
**4 then 6**. Same-frame down+up in the DINPUT buffer is
**PROVEN** as shape (two `00A66B20`s); a live click in one
`GetDeviceData` is still **UNREAD**.

| Claim | Status |
| --- | --- |
| Type 6 record is 52 bytes (same as type 4) | **PROVEN** `00A66B20` / `009F4ED0` `ecx=0xD` |
| Type 6 enqueue is the type-4 `00A66B20` | **PROVEN** `00AB59CB` after either ctor |
| Device store is 256 × 52 at `this+4` | **PROVEN** `00A66F20` / `00A66B20` |
| That store is a wrapping ring | **DISPROVEN** — `00A66B10` zeros `[+13316]` |
| Mux harvest type-filters 4 or 6 | **DISPROVEN** — no `00A03B40` in `009F57A0` copy |
| `009F4ED0` copies only the first 52-byte slot | **PROVEN** |
| `009F4ED0` reads `[+40]` / skips type 4 or 6 | **DISPROVEN** |
| Same walk can yield 4 then 6 | **PROVEN** `009F4ED0` then `009F4F10` |
| `009F4ED0` alone dequeues two records | **DISPROVEN** |
| `009F4F10` skips type 6 | **DISPROVEN** — skips type 0 only |
| `00A66FD0` type 6 drops the type-4 array slot | **DISPROVEN** — erases type-5 hold list only |
| Live LMB down+up in one `GetDeviceData` | **UNREAD** (listing-only) |

---

## 1. Same 52-byte record (`00A03D60` / `00A03C80`)

`listing-00a00000.txt`:

```
00A03C80  mov [ecx+32], 0x3
          mov [ecx+40], 0x4
          copy [eax] → [ecx+24/+28]
          fst  [ecx+48]
          fstp [ecx+44]
          ret 12

00A03D60  mov [ecx+32], 0x3
          mov [ecx+40], 0x6
          copy [eax] → [ecx+24/+28]
          fstp [ecx+48]
          fld qword [esp+16]
          fstp [ecx+44]
          ret 20
```

`00A03B40` is `mov eax, [ecx+40]; ret`. Highest store either
ctor writes is `+48` (8-byte). Every copy site below uses
`mov ecx, 0xD` / `rep movsd` = **52** bytes.

Type 6 is not a different object size. Fields that differ
are already in `type6-record-layout` (`+40`, second qword).
This proof only needs: **same dest width**.

`e8.tsv`: sole `.text` call to `00A03D60` is `00AB55A8`
inside `00AB5420` raw-4 arm `00AB5590`. Sibling type 4 is
`00AB5500` `00A03C80`. Both write `[ebp+12]` (the poll dest)
and set `al=1`.

---

## 2. Same device array, not a ring

`00AB58E0` (`listing-00a80000.txt`):

```
00AB58E8  lea ecx, [esp+68]
          call 00A04410             ; one dest
00AB5923  call 00A66B10             ; [this+13316] = 0
loop:
  00AB4910 / 00AB4BB0               ; one sample
  00AB5420(sample, dest)            ; 00AB59B7
  test al, al
  je loop
  00A66B20(dest)                    ; 00AB59CB
  00A66FD0(dest)                    ; 00AB59D7
  jmp loop
```

Raw 1 and raw 4 are different `00AB5420` arms, same dest,
same two follow-up calls. One sample cannot be both types.

`00A66F20` plants 256 slots:

```
lea edi, [esi+4]
mov ebx, 0x100
loop: 00A04410(edi); add edi, 52; dec ebx
[esi+13316] = 0                    ; count
[esi+13320] = 0                    ; generation
[esi+13324] = list head            ; hold list
```

`00A66B20` (`listing-00a40000.txt`):

```
eax = [this+13316]
if eax < 0 || eax >= 0x100: al = 0; ret 4
edi = this+4 + eax*52
ecx = 0xD
rep movsd
inc [this+13316]
al = 1
```

`00A66B10` is `mov [ecx+13316], 0; ret`. Previous poll is
discarded, not rotated. **No wrap, no overwrite of slot 0.**
The 257th translated event this poll is dropped (`al=0`).
That bound is shared: type 6 is not immune, and a type-6
append does not start a second array.

`00A66FD0` after the append (`cmp eax, 6` at `00A67186`):
walk `[+13324]`, erase a type-5 node (`009E47E0`). No write
of `[+13316]`. Type 4 already in the array stays. Type 6
just appended stays. Hold-list work is **not** a second
ring (`type4-enqueue-ring` §4, `type6-record-layout` §4).

Device first/next twins (`00A66B70` / `00A66BC0`) copy
52-byte slots the same way for any type.

---

## 3. Same mux harvest, still 52, still untyped

`009F57A0` (`listing-009c0000.txt`) after device poll
`[+36].vtbl+20` (`00AB58E0`):

```
009F67E0 clear +16 vector
[this+28] = 0
00A66B70 / 00A66BC0                 ; every device slot
copy 52 into +16 (grow 009F6AC0)
inc [this+28]                       ; no 00A03B40
00A66B00
```

Copy is `009E4850` or grow, then `add [edi+4], 52`. No
type compare. A type-4 slot and a later type-6 slot each
increment `[mux+28]` by one.

Mux layout used here (`type4-enqueue-ring` §5):

| Off | Role |
| ---: | --- |
| +16 / +20 / +24 | event vector begin / end / cap |
| **+28** | harvested count |
| +32 | generation |
| +36 | mouse device |

`+28` is **record count**, not “type-4 count”.

---

## 4. `009F4ED0` is first slot only

`listing-009c0000.txt`:

```
009F4ED0  [iter] = 1
          esi = [this+28]
          inc [this+32]
          if esi <= 0: al = 0; ret 8
          [iter+4] = 0
          esi = [this+16]            ; slot 0
          ecx = 0xD
          rep movsd                  ; 52 bytes, no type read
          al = 1
```

Empty mux is the only miss. If slot 0 is type 4, this
call dequeues 4. If slot 0 is type 6 (release-only harvest,
or up before down in the buffer), this call dequeues **6**.
It never walks to a second slot.

`009F4F10`:

```
edi = [iter+4]
inc edi
while edi < [this+28]:
  esi = [this+16] + edi*52          ; imul 52
  if 00A03B40(esi) != 0: copy 52; [iter+4]=edi; al=1
  else edi++
al = 0
```

Type 4 and type 6 are both nonzero. The only skip is
type 0.

`e8.tsv` consumers pair the two:

| First | Next | End | Caller |
| --- | --- | --- | --- |
| `0042E449` `009F4ED0` | `0042E7FE` `009F4F10` | `0042E815` `009F4AC0` | frontend `0042E3EE` |
| `00446462` `009F4ED0` | `00446677` `009F4F10` | `0044668F` `009F4AC0` | player `00446330` |

`0042E3EE` (`listing-00400000.txt`): harvest `vtbl+8`
once, `009F4ED0` → `jmp 0042E803`, classify, then
`0042E7F0` `009F4F10` / `jne 0042E453` until miss.
One poll, many records.

`00446330` skips device 2 / key 15 / type 0. Type 4 and
type 6 are device 3 (`[+32]=3`), so they are not that
skip.

---

## 5. Can one poll yield 4 then 6?

Producer order = `00AB58E0` sample order = mux FIFO.

| Buffer this harvest | `009F4ED0` | later `009F4F10` |
| --- | --- | --- |
| type 4 only | 4 | miss |
| type 6 only | 6 | miss |
| type 4 then type 6 | **4** | **6** |
| type 6 then type 4 | 6 | 4 |
| type 4, type 6, type 4 | 4 | 6, then 4 |

So: **yes, the same poll walk that starts at `009F4ED0`
can dequeue 4 then 6.** That is `009F4ED0` + `009F4F10`,
not two `009F4ED0`s.

`00A66B10` zeros the device count every `00AB58E0`. A
prior frame’s type 4 is gone. Cross-frame click is
type 4 in one `0042E3EE` and type 6 in a later one —
still the same stores, not the same harvest.

Whether a live press+release lands in one `GetDeviceData`
(`0x100` buffer at `00AB5710`) is **UNREAD**. The consume
rule does not care: if both records were harvested, both
dequeue, in enqueue order.

---

## 6. Answers

**`00A03D60` same 52-byte ring as type 4?**
**Same 52-byte record, same 256×52 device array, same mux
vector. Not a ring.** Type 6 is not a second queue.
`00A66FD0` only edits the hold list.

**Same poll `009F4ED0` can dequeue 4 then 6?**
**The walk that begins with `009F4ED0` can. `009F4ED0`
alone cannot.** First slot only, type-blind. Next slot is
`009F4F10`. Down then up in one harvest → 4 then 6.

---

## 7. C# leftover

`EngineInput.Pump` walks the whole list in one call
(**MATCH** `009F4ED0` + `009F4F10`). `TryDequeue` is
first-only (**MATCH** one `009F4ED0`). Host still does
not queue type 6 (`host-input-type4`). Do not invent a
wrap ring or a type-6-only store.

No `src/` change in this proof.

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00a00000.txt`
  (`00A03B40`, `00A03C80`, `00A03D60`)
- `listing-00a40000.txt` (`00A66B10` / `00A66B20` /
  `00A66B70` / `00A66BC0` / `00A66F20` / `00A66FD0`)
- `listing-00a80000.txt` (`00AB5420` / `00AB5590` /
  `00AB55A8`, `00AB58E0` / `00AB59CB` / `00AB59D7`)
- `listing-009c0000.txt` (`009F4ED0`, `009F4F10`,
  `009F4AC0`, `009F57A0`)
- `listing-00400000.txt` (`0042E3EE` harvest + walk)
- `listing-00440000.txt` (`00446330` same pair)
- `e8.tsv`
- `proofs/type6-record-layout/README.md`
- `proofs/type4-enqueue-ring/README.md`
- `proofs/type6-same-poll-as-4/README.md`
