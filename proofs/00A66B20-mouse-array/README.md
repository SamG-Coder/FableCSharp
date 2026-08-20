# `00A66B20` mouse array: type 4 / 6 into `this+4` 256×52

Investigation only. No production `src/` edits.

Question: Type 4 and 6 store into mouse device `this+4` 256×52
array via `00A66B20`. Host `EngineInput` leftover vs that
array? `00A66B10` zeros `+13316` each poll?

Authority: dump `Fable.exe` `00A66B10` / `00A66B20` / `00A66F20`
/ `00A66B70` / `00A03C80` / `00A03D60` / `00AB58E0` / `00AB5420`
(`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00a40000.txt`,
`listing-00a00000.txt`, `listing-00a80000.txt`);
`e8.tsv` (`0x00AB59CB` → `0x00A66B20`, `0x00AB5B49` →
`0x00A66B20`, `0x00AB5923` → `0x00A66B10`, `0x00AB5D4D` →
`0x00A66B10`);
`proofs/type4-type6-ring/README.md`;
`src/Fable.Game/EngineInput.cs`;
`src/Fable.Client/Program.cs`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**
/ **LEFTOVER** / **MATCH**.

Do not re-prove type 4 = LMB down → action 26, type 6 = LMB up
→ action 28, or `009F4ED0` first-slot walk
(`type4-type6-ring`).

---

## Verdict

**Yes on the native store.** Type 4 (`00A03C80`) and type 6
(`00A03D60`) both land in the same dest, then `00AB58E0`
`00A66B20`s that dest into the mouse device’s **256 × 52**
linear array at **`this+4`**. It is not a wrapping ring.

**Yes, `00A66B10` zeros `[this+13316]` at the start of every
mouse poll.** That dword is the write count. Slots are not
`memset`; the prior poll is discarded because count is 0.

**Host `EngineInput` is leftover vs that array.** Classify
(type 4 / 6, LMB edge in `Program.cs`) is present. The 256×52
device store, `+13316` cap, and 52-byte `rep movsd` are not.

| Claim | Status |
| --- | --- |
| Type 4 / 6 share dest then `00A66B20` | **PROVEN** `00AB59CB` |
| Device store is `this+4`, stride 52, cap `0x100` | **PROVEN** `00A66F20` / `00A66B20` |
| That store is a wrapping ring | **DISPROVEN** — `00A66B10` zeros count |
| `00A66B20` type-filters 4 or 6 | **DISPROVEN** — blind 52-byte copy |
| `00A66B10` is `mov [ecx+13316],0; ret` | **PROVEN** |
| `00A66B10` runs each `00AB58E0` poll | **PROVEN** `00AB5923` |
| `00A66B10` `memset`s the 256 slots | **DISPROVEN** — count only |
| Host `EngineInput` is a 256×52 device array | **DISPROVEN** — unbounded `List` |
| Host `Queue` / `Pump` match `00A66B20` / `+13316` | **LEFTOVER** |
| Host LMB down/up queues type 4 then type 6 | **MATCH** `Program.cs` classify |
| Host writes 52-byte records / origin `+24/+28` | **DISPROVEN** — `(type, key)` only |
| 257th event this poll is dropped | **PROVEN** native; **LEFTOVER** host (no cap) |

---

## 1. Array plant (`00A66F20`) then append (`00A66B20`)

`listing-00a40000.txt`:

```
00A66F20  [esi] = 0x129DBC4
          lea edi, [esi+4]
          mov ebx, 0x100
          loop: 00A04410(edi); add edi, 52; dec ebx
          [esi+13324] = circular 64-byte list head
          [esi+13320] = 0
          [esi+13316] = 0
```

`256 * 52 = 13312`. First dword after the slots is
`this+4+13312` = **`this+13316`** (`0x3404`).

```
00A66B20  eax = [this+13316]
          if eax < 0 || eax >= 0x100: xor al, al; ret 4
          edi = this+4 + eax*52
          ecx = 0xD
          rep movsd                 ; 52 bytes from arg
          inc [this+13316]
          mov al, 1
          ret 4
```

No type read. Slot 0 is never overwritten to wrap. The 257th
append this count is `al=0`.

Helper `00A66B60` is `return this+4 + index*52`. First-slot
read `00A66B70` copies `[this+4]` when `[+13316] > 0`.

---

## 2. Type 4 and type 6 both hit that append

`00A03C80` / `00A03D60` (`listing-00a00000.txt`) write the
same dest width (highest store `+48`; copy sites use
`ecx=0xD`). `[+32]=3` (mouse), `[+40]=4` or `6`.

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

Raw 1 → `00A03C80` type 4. Raw 4 → `00A03D60` type 6. Same
`[esp+68]`, same `00A66B20`. One sample cannot be both.

`e8.tsv`: `00A66B20` only from `00AB59CB` (this loop) and
`00AB5B49` (type-13 motion after the sample loop). Type 4 / 6
never use a second store.

`00A66FD0` after the append does not rewrite `[+13316]` and
does not un-copy the slot (`type4-type6-ring` §2). Type 6
erases a type-5 **hold list** node only.

---

## 3. `00A66B10` zeros `+13316` each poll

```
00A66B10  mov [ecx+13316], 0x0
00A66B1A  ret
```

`e8.tsv` callers:

| Site | Role |
| --- | --- |
| `00AB5923` | start of every `00AB58E0` poll |
| `00AB5D4D` | mouse ctor, after `00A66F20` |

Poll path: count is cleared **before** samples. Previous
frame’s type 4 / 6 are not rotated and not harvested again
(`00A66B70` `jle` when count `<= 0`). The 256 slot bytes
are left as-is until the next `rep movsd`.

Ctor also zeros the same dword. That is init, not a second
per-frame store.

---

## 4. Host leftover vs the array

`EngineInput` (`src/Fable.Game/EngineInput.cs`):

- `_queue` is `List<(int Type, int Key)>`. Unbounded.
- `Queue` is `Add`. No `0x100` test, no `this+4`, no
  `rep movsd` 52.
- `Pump` walks every pair then `_queue.Clear()`. That
  **MATCH**es “discard last poll” at the classify layer,
  not `00A66B10` on a device object.
- `TryDequeue` is first-only (**MATCH** one `009F4ED0`),
  still not a 52-byte slot.

`Program.cs` LMB edge:

```
if (lmbDown && !lmbWasDown) QueueInput(Type4, 0);
if (!lmbDown && lmbWasDown) QueueInput(Type6, 0);
```

Classify **MATCH** (type 4 then type 6). Record shape
**LEFTOVER**: no device 3 at `+32`, no origin at `+24/+28`,
no doubles at `+44/+48`. Host never implements the 256×52
array, so it cannot drop the 257th event of a poll.

`type4-type6-ring` §7 “host still does not queue type 6”
is **STALE** vs present `Program.cs`. The leftover here is
the **device array**, not the type-6 id.

---

## 5. Answers

**Type 4 and 6 store into mouse `this+4` 256×52 via
`00A66B20`?**
**Yes.** Shared dest, shared append, cap 256, stride 52.
Not a ring.

**Host `EngineInput` leftover vs that array?**
**Yes.** Unbounded `(type, key)` list. LMB classify is
present. The 256×52 store and `+13316` cap are leftover.

**`00A66B10` zeros `+13316` each poll?**
**Yes.** That is the only write in the function. Called at
`00AB58E0` entry. Count only; slots are not cleared.

No `src/` change in this proof.

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00a40000.txt`
  (`00A66B10` / `00A66B20` / `00A66B60` / `00A66B70` /
  `00A66F20`)
- `listing-00a00000.txt` (`00A03C80`, `00A03D60`)
- `listing-00a80000.txt` (`00AB58E0` / `00AB5923` /
  `00AB59CB` / `00AB5B49` / `00AB5D4D`)
- `e8.tsv`
- `proofs/type4-type6-ring/README.md`
- `src/Fable.Game/EngineInput.cs`
- `src/Fable.Client/Program.cs`
