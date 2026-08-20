# Type-6 LMB-up record: `+32=3`, `+40=6`, `+0` unused

Investigation only. No production `src/` edits.

Question: `00A03D60` type 6 LMB-up record: `+32` device 3,
`+40=6`, `+0` unused. Same poll ring as type 4?

Authority: dump `Fable.exe` `00A03D60` / `00AB5420` /
`00AB58E0` / `00AB4910` / `00AB4BB0` / `00A03C80` /
`00A04410` / `00A03B40` / `00A03B70` / `00A66B20` /
`00A66FD0` / `00A66B10` / `0042E3EE`
(`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00a00000.txt`,
`listing-00a80000.txt`, `listing-00a40000.txt`,
`listing-00400000.txt`); `e8.tsv` (`0x00AB55A8` →
`0x00A03D60`); `proofs/type4-dinput-raw`,
`proofs/type4-enqueue-ring`, `proofs/type6-action28`,
`proofs/host-input-type4`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not re-prove type 4 = LMB down → action 26, or that
type 6 classify is action 28 (`type6-action28`).

---

## Verdict

**Yes on the fields. Yes on the same poll / same 256×52
device array as type 4. Not a wrapping ring.**

`00A03D60` is the only `.text` filler of `[record+40]=6`.
It stamps mouse device **3** at `+32` and never writes
`[record+0]` (the type-1 DIK slot). `00AB5420` raw **4**
(LMB up) is the only caller. Enqueue is the type-4 path:
`00AB58E0` → `00A66B20` into `this+4` stride 52, then
mux harvest. Count is zeroed each poll (`00A66B10`). The
store is **not** a wrap ring (`type4-enqueue-ring`).

`00A66FD0` after a type-6 append **erases** a type-5 hold
node. That is extra list work, not a second queue.

| Claim | Status |
| --- | --- |
| `00A03D60` writes `[+32]=3`, `[+40]=6` | **PROVEN** `listing-00a00000.txt` |
| `00A03D60` writes `[+0]` / a DIK | **DISPROVEN** — no `[ecx]` store |
| Sole `.text` E8 is `00AB55A8` inside `00AB5420` | **PROVEN** `e8.tsv` |
| `00AB5420` raw 4 → `00AB5590` → `00A03D60` | **PROVEN** (`0xAB56EC[3]=3`) |
| Raw 4 is LMB **up** (`DIMOFS_BUTTON0` / primary release) | **PROVEN** `00AB4910` / `00AB4BB0` |
| Type 6 dest is the same `[esp+68]` as type 4 | **PROVEN** `00AB58E0` |
| Type 6 enqueue is `00A66B20` after translate `al==1` | **PROVEN** `00AB59CB` |
| Device store is 256 × 52 linear, same as type 4 | **PROVEN** `00A66B20` / `00A66F20` |
| That store is a wrapping ring | **DISPROVEN** — `00A66B10` zeros count |
| `00A66FD0` drops the type-6 array slot | **DISPROVEN** — walks list, erase type 5 |
| `0042E3EE` type 6 reads `00A03B70` (`+0`) | **DISPROVEN** — `push 28` only |
| Type 6 is Return / `DIK_RETURN` (28) | **DISPROVEN** — type 1 / action 33 |
| Pad button-up also calls `00A03D60` | **UNREAD** (`pad-a-vs-type4`) |

---

## 1. Record ctor `00A03D60` (`listing-00a00000.txt`)

```
00A03D60  mov eax, [esp+4]
00A03D64  fld qword [esp+8]
00A03D68  mov [ecx+32], 0x3        ; device 3 (mouse)
00A03D6F  mov [ecx+40], 0x6        ; CInputType
00A03D76  mov edx, [eax]
00A03D78  mov [ecx+24], edx        ; origin pair
00A03D7B  mov eax, [eax+4]
00A03D7E  fstp [ecx+48]            ; first double
00A03D81  fld qword [esp+16]
00A03D85  mov [ecx+28], eax
00A03D88  fstp [ecx+44]            ; second double
00A03D8B  ret 20
```

Family getters on the same page: `00A03B40` is
`mov eax, [ecx+40]; ret` (type). `00A03B50` is `+32`
(device). `00A03B70` is `mov eax, [ecx]; ret` (`+0`).

Sibling down ctor `00A03C80` (`listing-00a00000.txt`):

```
00A03C80  mov [ecx+32], 0x3
          mov [ecx+40], 0x4
          copy [eax] → [ecx+24/+28]
          fst  [ecx+48]
          fstp [ecx+44]            ; same double both slots
          ret 12
```

Same **52-byte** record (`00A66B20` `rep movsd` count
`0xD`). Same device and origin pair. Differences:

| Off | Type 4 `00A03C80` | Type 6 `00A03D60` |
| ---: | --- | --- |
| **+0** | **not written** | **not written** |
| +24 / +28 | origin from ptr | origin from ptr |
| **+32** | **3** | **3** |
| **+40** | **4** | **6** |
| +44 / +48 | one sample qword both | two qwords (`ret 20`) |

Keyboard type 1 (`00A03BF0`) is the ctor that stores a
key at `[ecx]`. Type 5 hold (`00A03D10`) copies 12 bytes
onto `+0` of a **list node**, not this dest.

RTTI family: `CInputTypeMouseButtonEvent`.

---

## 2. `+0` is unused (**PROVEN**)

Three independent misses:

1. **Ctor.** `00A03D60` never writes `[ecx]`, `[ecx+4]`,
   or `[ecx+8]`. Contrast `00A03BF0` `mov [ecx], eax`.
2. **Zero fill, not a key.** Poll dest is
   `lea ecx, [esp+68]; call 00A04410` once at
   `00AB58E8`. `00A04410` zeros `+0` through `+28`,
   `+36`, and `+40`. Device slots get the same zero
   at `00A66F20`. Nothing later plants a DIK there
   for type 6.
3. **Consumer.** `0042E3EE` type 6 is `0042E481`
   `dec`/`dec`/`je 0042E498` → `push 28`. The eight
   `00A03B70` calls in that function sit on the type-1
   branch (`0042E4B0`). Type 6 never reads `+0`.

`FrontendInputMap.EventKeyOffset = 0` is the type-1
slot. `ActionFromEvent` ignores `key` for type 6.

---

## 3. Who fills it: `00AB5420` raw 4

`e8.tsv`: one site, `00AB55A8` → `00A03D60`.

`00AB5420` (`listing-00a80000.txt`) is thiscall
`(sample, dest_record)`. Second switch:

```
00AB54D3  mov eax, [esi+8]          ; raw kind
00AB54D6  lea ecx, [eax-1]
00AB54D9  cmp ecx, 23
00AB54DC  ja 00AB5669               ; default, al stays 0
00AB54E2  movzx edx, [ecx+0xAB56EC]
00AB54E9  jmp [0xAB56C4+edx*4]
00AB54F0  call 00A03C80             ; jt[0] type 4
…
00AB5590  fld qword [esi]
          sub esp, 16
          fstp [esp+8]              ; ctor qword 1
          lea ecx, [esp+40]         ; origin = this+13332/+13336
          fld [esp+36]
          fstp [esp]                ; ctor qword 2
          push ecx
          mov ecx, [ebp+12]
00AB55A8  call 00A03D60
          mov [esp+15], 1
```

Index `0xAB56EC` / jump `0xAB56C4` recovered in
`type4-dinput-raw`:

| `[esi+8]` | idx | Site | Ctor | `[+40]` |
| ---: | ---: | --- | --- | ---: |
| 1 | 0 | `00AB54F0` | `00A03C80` | **4** LMB down |
| **4** | **3** | **`00AB5590`** | **`00A03D60`** | **6** LMB up |

Who writes raw 4:

`00AB4910` `GetDeviceData`, `dwOfs==12` (`DIMOFS_BUTTON0`):

```
and al, 0x80; neg; sbb; and -3; add 4
→ down raw 1, up raw 4
```

Win32 `00AB4BB0` primary `009A4FC0` edge:

```
dec; neg; sbb; and 3; inc
→ press raw 1, release raw 4
```

Same translator, same dest. No DIK.

First switch (`lea ecx, [eax-4]`) also sees raw 4 and
can copy a hold-list dword into the second qword. That
is timestamp / hold payload for `+44`, **not** a key at
`+0`. Exact first-table bytes stay as in
`type4-dinput-raw` (listing decodes them as `push`/`stosd`).

---

## 4. Same poll / same array as type 4 (**PROVEN**)

`00AB58E0` (`listing-00a80000.txt`, device `vtbl+20`):

```
00AB58E0  sub esp, 108
          lea ecx, [esp+68]
          call 00A04410             ; dest, +0 zeroed
          call 00A66B10             ; [this+13316] = 0
loop:
  00AB4910 / 00AB4BB0               ; one sample
  00AB5420(sample, dest)            ; 00AB59B7
  test al, al
  je loop                           ; miss: no enqueue
  00A66B20(dest)                    ; 00AB59CB
  00A66FD0(dest)                    ; 00AB59D7
  jmp loop
```

Type 4 (raw 1) and type 6 (raw 4) both return `al=1`
and take the same two calls. Dest is one stack record.

`00A66B20` (`listing-00a40000.txt`):

```
eax = [this+13316]
if eax < 0 || eax >= 0x100: al = 0; ret 4
edi = this+4 + eax*52
ecx = 13
rep movsd                           ; 52 bytes
inc [this+13316]
al = 1
```

Ctor `00A66F20` plants 256 slots at `this+4`. Count
`[+13316]` is reset every poll. Full → drop. **No wrap,
no overwrite of slot 0.** Mux harvest `009F57A0` then
copies every surviving slot into `[0x13B8388]+16` and
`inc [+28]` with no type filter (`type4-enqueue-ring`).

So type 6 rides the type-4 poll and the type-4 array.
It is **not** a second ring.

### `00A66FD0` after type 6

`00A03B40` on the dest, then (`listing-00a40000.txt`):

| Type | Action |
| ---: | --- |
| 4 | insert at `[+13324]`; `00A03D10` **type 5** at node+12 |
| **6** | walk list for type **5**; `009E47E0` erase (`00A67186`) |
| 7 / 10 | insert type 8 / 11 |
| 9 / 12 | erase type 8 / 11 |

Type 6 does **not** rewrite `[+13316]` and does **not**
`00A03BE0` the just-copied slot. The type-6 record stays
in the array. The extra effect is **unhold** of LMB
(type 5), the inverse of type 4’s insert.

---

## 5. Answers

**`00A03D60`: `+32` device 3, `+40=6`, `+0` unused?**
**Yes.** Device 3 and type 6 are immediates. `+0` is
neither stored nor read on this event. Classify is
`0042E3EE` `push 28` (`type6-action28`).

**Same poll ring as type 4?**
**Same poll and same 256×52 device array. Not a ring.**
`00AB58E0` → `00AB5420` → `00A66B20` is shared.
`00A66B10` zeros the count each mouse poll. Type 6
additionally erases a type-5 hold node; it does not
use a second store.

---

## 6. C# leftover

`FrontendInputMap.Type6` / `ActionType6` and
`EngineInput` type 6 → 28 already **MATCH** `0042E3EE`.
There is no `Type6RecordCtor` constant (type 4 has
`0x00A03C80`). Host still does not queue type 6
(`host-input-type4`). Do not invent a DIK at `+0`.

No `src/` change in this proof.
