# Type-4 enqueue: device array, mux `+28`, consumer `009F4ED0`

Investigation only. No production `src/` edits.

Authority: dump `Fable.exe` `00A03C80` / `00AB5420` / `00A66B20` /
`00A66FD0` / `009F4ED0` / `009F4F10` / `009F4AC0` / `009F57A0` /
`00AB58E0` / `00A66B10` / `00A66B70` / `00A66BC0` / `00A66B00` /
`00A66F20` / `00A66ED0` (`listing-00a00000.txt`,
`listing-00a40000.txt`, `listing-00a80000.txt`,
`listing-009c0000.txt`, `listing-00400000.txt`);
`e8.tsv`; `functions.tsv` (`0x00AB5420`, `0x00A66FD0`,
`0x009F57A0`); `proofs/type4-dinput-raw`,
`proofs/type13-vs-type4`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not re-prove type 4 = LMB down, or `0042E3EE` type 4 → action 26.

---

## Verdict

**Type 4 is not type-filtered off the queue.** After `00A03C80`
fills the 52-byte record, `00AB58E0` (not `00AB5420` itself)
`00A66B20`s it onto a **256 × 52** linear array on the mouse
device. `00A66FD0` does **not** remove it; it inserts a **type 5**
hold node on the side list at `+13324`.

The only native drop of that type-4 copy is **`00A66B20` full /
bad count** (`[+13316]` not in `0..0xFF`). Mux harvest
`009F57A0` then copies every surviving slot into
`[0x13B8388]+16` and **`inc [+28]` once per record**. Consumer
`009F4ED0` fails only when that count is `<= 0`. `009F4F10`
skips **type 0**, not type 4.

It is **not** a wrapping ring. Count is zeroed each mouse poll
(`00A66B10`).

| Claim | Status |
| --- | --- |
| `00A03C80` writes `[+40]=4`, `[+32]=3`, 52-byte record | **PROVEN** |
| `00AB5420` raw 1 → `00A03C80`, `al=1` | **PROVEN** |
| Enqueue E8 is `00AB59CB` `00A66B20` after that `al` | **PROVEN** `e8.tsv` |
| `00AB5420` itself calls `00A66B20` | **DISPROVEN** — caller is `00AB58E0` |
| Device store is `this+4`, stride 52, cap `0x100` | **PROVEN** `00A66F20` / `00A66B20` |
| Count is `[device+13316]` (`0x3404`) | **PROVEN** |
| Full / `count<0` → `00A66B20` `al=0` (type 4 dropped) | **PROVEN** |
| `00A66FD0` drops type 4 from the array | **DISPROVEN** — type 4 → insert type 5 on list |
| `00A66B20` `al` gates `00A66FD0` | **DISPROVEN** — always called on translate hit |
| `[0x13B8388]+28` is mux harvested count | **PROVEN** `009F57A0` / `009F4ED0` |
| Harvest type-filters type 4 | **DISPROVEN** |
| Mux `+28` is a 256-cap ring index | **DISPROVEN** — growable vector at `+16` |
| `009F4ED0` skips type 4 | **DISPROVEN** — only `+28<=0` fails |
| `009F4F10` skips type 4 | **DISPROVEN** — skips type 0 only |

---

## 1. Fill (`00A03C80`) then translate (`00AB5420`)

`00A03C80` (`listing-00a00000.txt`):

```
00A03C80  mov eax, [esp+4]
          fld qword [esp+8]
          mov [ecx+32], 3
          mov [ecx+40], 4
          copy [eax] → [ecx+24/+28]
          fst  [ecx+48]
          fstp [ecx+44]
          ret 12
```

Record size used everywhere below is **52** (`rep movsd` count
`0xD`). Type is `+40` (`00A03B40`).

`00AB5420` (`listing-00a80000.txt`) is thiscall
`(sample, dest_record)`. Raw `[esi+8]==1` → `lea ecx,[eax-1]`
index 0 → `00AB54F0` → `call 00A03C80` into `[ebp+12]`, then
`mov [esp+15],1` / `mov al,[esp+15]`. Default / out-of-range
raw leaves that byte 0.

`00AB5420` returns. It does **not** enqueue.

---

## 2. Who calls `00A66B20` / `00A66FD0`

`e8.tsv`: `00A66B20` only from `00AB59CB` and `00AB5B49`.
`00A66FD0` only from `00AB59D7`.

`00AB58E0` (mouse poll, no E8 — device `vtbl+20` from harvest):

```
00A66B10                    ; [this+13316] = 0
loop:
  00AB4910 / 00AB4BB0       ; one sample
  00AB5420(sample, dest)
  test al, al
  je loop                   ; translate miss: no enqueue
  00A66B20(dest)            ; 00AB59CB — result ignored
  00A66FD0(dest)            ; 00AB59D7 — always after hit
  jmp loop
; after samples:
  maybe 00A03FB0 type 13
  00A66B20                  ; 00AB5B49 — motion only
  00A66C20 / 00A66ED0       ; if [+13373]==1: refresh hold list
```

Type 4 therefore reaches `00A66B20` iff translate `al==1`.
`00A66B20` failure does **not** skip `00A66FD0`.

---

## 3. Device array layout (not a wrap ring)

Ctor `00A66F20`:

```
[this] = 0x129DBC4
for i in 0..0xFF:
  00A04410(this+4 + i*52)   ; 256 slots
[+13324] = circular 64-byte list head
[+13320] = 0                ; generation
[+13316] = 0                ; count
```

`256 * 52 = 13312`; `+4 + 13316` is the first dword after the
slots. Offsets:

| Off | Hex | Role |
| ---: | --- | --- |
| 0 | 0 | vtbl |
| 4 | 4 | slot 0 (52 bytes) |
| 4 + n×52 | | slot n |
| 13316 | `0x3404` | write count |
| 13320 | `0x3408` | generation |
| 13324 | `0x340C` | hold-list head |

`00A66B10` (start of every `00AB58E0`): `mov [ecx+13316],0; ret`.
Previous frame is discarded, not rotated.

### `00A66B20` append

```
eax = [this+13316]
if eax < 0 || eax >= 0x100:
  xor al, al
  ret 4                     ; DROP
edi = this+4 + eax*52
ecx = 13
rep movsd                   ; 52 bytes from arg
inc [this+13316]
mov al, 1
ret 4
```

Capacity **256 events per poll**. No wrap, no overwrite of slot 0.
The 257th type 4 this frame is dropped (`al=0`).

DINPUT acquire sets `GetDeviceData` buffer `0x100`
(`00AB5710`), so one poll can theoretically fill the array.

### First / next / end (device)

| VA | Mux twin | Body |
| --- | --- | --- |
| `00A66B70` | `009F4ED0` | if count `<=0` fail; copy **slot 0**; `inc +13320` |
| `00A66BC0` | `009F4F10` | `cursor++`; fail if `>= count`; copy slot |
| `00A66B00` | `009F4AC0` | `[arg]=0`; `dec +13320` |

`00A66B70` always copies slot 0 (no read index on the device).
Walk is cursor-in-the-iterator, same as the mux.

---

## 4. `00A66FD0` — type 4 is not removed

`00A03B40` on the just-filled dest, then:

| Type | Action |
| ---: | --- |
| **4** | `009E4A20` insert at `[this+13324]`; `00A03D10` **type 5** at node+12 |
| 7 | same insert; `00A03DC0` type 8 |
| 10 | same insert; `00A03E70` type 11 |
| 6 | walk list for type 5; `009E47E0` erase |
| 12 | erase type 11 |
| 9 | erase type 8 |
| else | later `+22` table / default `ret` |

Type 4 path (`00A66FE8` `cmp eax,4` / `je 00A66FED`) **returns
after the insert**. It does not write `[+13316]`, does not
`00A03BE0` the array slot, and does not call `00A66B20` again.

So a successful type-4 append stays in the array. The extra
type 5 is **held-button state** on the list. `00A66ED0` later
copies list nodes (`[ebx+12]`) back into the array **if count
still `< 0x100`**. Those copies are type 5/8/11, not type 4.
If the array is already full, `00A66ED0` skips the copy (same
bounds as `00A66B20`) — still not a type-4 drop.

---

## 5. Mux `[0x13B8388]` and `+28`

`004022E5` stores `[009A4EC0()+88]` at `0x13B8388`.

Layout used by harvest / consume:

| Off | Role |
| ---: | --- |
| +4 / +8 / +12 | aux vector (cleared each harvest) |
| **+16 / +20 / +24** | **event vector begin / end / cap** |
| **+28** | **harvested count** |
| +32 | generation (`009F4ED0` `inc`, `009F4AC0` `dec`) |
| +36 | mouse device (`00A66B70` / `00A66C00`) |
| +40 | sibling device (`00A667C0`) |
| +44 / +48 | other-device pointer array, stride 8 |
| +56 | if set, `009F57A0` skips live harvest |

`009F5600` is the standalone push: copy 52 via `009E4850` or
grow `009F6AC0`, then **`inc [this+28]`**. No E8 (vtbl / inlined
twin). Harvest inlines the same copy+inc.

### Harvest `009F57A0` (no E8)

Frontend `0042E42C` does `[0x13B8388].vtbl+8` immediately
before `009F4ED0`. Exact vtbl dword is **PARTIAL** (no E8).
Body that zeros `+28` and drains the mouse array is
`009F57A0` (**PROVEN**).

```
if [this+56]:
  replay path (not live type 4)
else:
  [+36].vtbl+20            ; 00AB58E0 fill device array
  [+40].vtbl+4
  each [+44].vtbl+40
  009F67E0 clear +4 and +16 vectors
  [this+28] = 0
  if [+36]:
    00A66B70 / 00A66BC0    ; every device slot
    copy 52 into +16 (grow 009F6AC0 if end==cap)
    inc [this+28]          ; no 00A03B40 test
    00A66B00               ; end device walk
  ; then +40 and +44 the same
```

No type compare. A type-4 slot that `00A66B20` accepted
increments `+28` by one. Growable vector: **mux does not drop
on a 256 cap**.

`009F5540` (frontend `[game+312]!=0` early-out in `0042E3EE`)
sets `[+28]=0` and `00A66C00`s the mouse array.

---

## 6. Consumer `009F4ED0`

`e8.tsv`: `0042E449`, `00446462` (`00446330` player interface),
`004963E6`, `006289FC` (PlayAVI).

```
009F4ED0  [iter] = 1
          esi = [this+28]          ; count
          inc [this+32]            ; generation
          if esi <= 0: al = 0; ret 8
          [iter+4] = 0             ; cursor
          esi = [this+16]          ; slot 0
          dest = arg1
          ecx = 13
          rep movsd
          al = 1
```

Type is not read. Empty mux (`+28==0`) is the only miss.

`009F4F10` (`0042E7FE` loop): `cursor++`; while `cursor < +28`
and `00A03B40(slot)==0`, skip; else copy. **Type 0 only.**

`009F4AC0` after the loop: `[iter]=0`; `dec [this+32]`.

Frontend `0042E3EE`: `009F4ED0` → `00A03B40` → type 4
(`dec; sub 3; je`) → `push 26`. Player `00446330` skips
device 2 / key 15 / type 0; type 4 is device 3, so it is not
that skip.

---

## 7. Can type 4 be dropped?

| Site | Drops type 4? |
| --- | --- |
| `00AB5420` default | Only if raw is not 1 (never a filled type-4 dest) |
| `00A66B20` | **Yes** if `[+13316]` not in `0..255` |
| `00A66FD0` | **No** |
| `00A66ED0` | No (may drop **type 5** re-copy) |
| `009F57A0` harvest | **No** (grows) |
| `009F4ED0` | **No** (empty mux only) |
| `009F4F10` | **No** (type 0) |
| `00446330` device/key skip | **No** for type 4 |

Practical drop: more than 256 translated events in one
`00AB58E0` (or a corrupted negative count). A single LMB down
is one slot.

`+28` after harvest = number of 52-byte records copied from
mouse + sibling + other devices that poll. It is **not** “type-4
count”. One type 4 contributes **1** if `00A66B20` succeeded.

---

## 8. C# leftover

`EngineInput` is an unbounded `List`. `Queue` / `Pump` /
`TryDequeue` have no `0x100` cap and no mux `+28`. Host still
does not have to invent wrap. A faithful device array would
drop the 257th event of a poll; current C# will not.

No `src/` change in this proof.
