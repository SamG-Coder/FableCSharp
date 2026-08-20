# `00A66B20` mouse array vs host: leftover, first leftover

Investigation only. No production `src/` edits.

Question: `00A66B20` mouse array vs host. **MATCH** or
**leftover**? First leftover?

Authority: existing `proofs/00A66B20-mouse-array`;
dump `Fable.exe` `00A66B10` / `00A66B20` / `00A66F20` /
`00A66B70` / `00A03C80` / `00A03D60` / `00AB58E0` /
`00AB5420` (`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00a40000.txt`,
`listing-00a00000.txt`, `listing-00a80000.txt`);
`e8.tsv` (`0x00AB59CB` → `0x00A66B20`, `0x00AB5B49` →
`0x00A66B20`, `0x00AB5923` → `0x00A66B10`, `0x00AB5D4D` →
`0x00A66B10`);
`proofs/type4-type6-ring`;
`proofs/type4-enqueue-ring`;
`src/Fable.Game/EngineInput.cs`;
`src/Fable.Client/Program.cs`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Do not invent DIK. Type 4 / 6 `[+0]` is unread
(`type6-record-layout`). Do not re-prove type 4 = LMB
down → action 26 or type 6 = LMB up → action 28.

---

## Verdict

**Leftover, not MATCH.** Host never implements the
mouse device’s **256 × 52** linear array at `this+4`.

**Yes, first leftover** after MATCH classify. Live
`Program.cs` LMB edge queues type 4 then type 6.
`EngineInput.ApplyEvent` maps those types. That
classify layer is **MATCH**. The first missing native
store after it is `00A66B20` (`rep movsd` 13 dwords,
cap `0x100`, count `[this+13316]`). Type-6 **id** is
no longer leftover (`00A66B20-mouse-array` §4).

| Claim | Status |
| --- | --- |
| Native store is `this+4`, stride 52, cap `0x100` | **PROVEN** `00A66F20` / `00A66B20` |
| `00A66B20` only from `00AB59CB` and `00AB5B49` | **PROVEN** `e8.tsv` |
| Host `EngineInput` is that 256×52 array | **DISPROVEN** — unbounded `List` |
| Host `Queue` / `Pump` **MATCH** `00A66B20` / `+13316` | **DISPROVEN** — **LEFTOVER** |
| Host LMB down/up queues type 4 then type 6 | **MATCH** `Program.cs` |
| Host type 4 / 6 → actions 26 / 28 | **MATCH** `ApplyEvent` |
| Host writes 52-byte dest / device 3 / origin | **DISPROVEN** — `(type, key)` only |
| 257th event this poll is dropped | **PROVEN** native; **LEFTOVER** host |
| First leftover after classify is this array | **PROVEN** leftover |
| First leftover is the type-6 id | **DISPROVEN** — id is **MATCH** |
| `host-input-type4` “never queues type 4 / 6” | **STALE** vs present `Program.cs` |
| Type 4 / 6 key is a DIK | **DISPROVEN** — do not invent |

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| `00A66B20` array vs host: MATCH or leftover? | **Leftover.** No `this+4`, no stride 52, no `+13316` cap. | **LEFTOVER** |
| First leftover? | **Yes** — first missing store after MATCH LMB classify. | **PROVEN** leftover |
| Implement 256×52 for first-seen click? | **No.** One type 4 (and one type 6) never hits `0x100`. Cap / wrap is leftover theater. | **LEFTOVER** theater |

---

## 1. Native `00A66B20` (unchanged)

`listing-00a40000.txt`:

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

Plant `00A66F20`: `[this]=0x129DBC4`, loop `0x100` ×
`00A04410` at `this+4` stride 52, `[this+13316]=0`.
`256 * 52 = 13312`. Count dword is `this+4+13312`.

`00A66B10` is `mov [ecx+13316],0; ret`. `e8.tsv`:
`00AB5923` (every `00AB58E0` poll) and `00AB5D4D`
(ctor after plant). Slots are not `memset`.

Callers of `00A66B20` (`e8.tsv` only two):

| Site | Dest | Role |
| --- | --- | --- |
| `00AB59CB` | `[esp+68]` after `00AB5420` | type 4 / 6 (and siblings) |
| `00AB5B49` | same dest after `00A03FB0` | type 13 motion |

No type read. The 257th append this count is `al=0`.
Not a wrap ring (`00A66B20-mouse-array`).

---

## 2. Host has no array

`EngineInput` (`src/Fable.Game/EngineInput.cs`):

- `_queue` is `List<(int Type, int Key)>`. Unbounded.
- `Queue` is `Add`. No `0x100`, no `this+4`, no
  `rep movsd` 13.
- `Pump` applies every pair then `_queue.Clear()`.
  That **MATCH**es “discard last poll” at the
  **classify** layer, not `00A66B10` on the mouse
  object.
- `TryDequeue` is first-only (**MATCH** one
  `009F4ED0`), still not a 52-byte slot.
- Constants name type / action / `Type4Device=3`.
  `Queue` never stores device 3, origin `+24/+28`,
  or doubles `+44/+48`.

`src/` has **no** `00A66B20` / `00A66B10` / `00A66F20`
/ `13316` / 256×52 mouse buffer. Tests do not assert
the cap.

`Program.cs`:

```
if (lmbDown && !lmbWasDown) QueueInput(Type4, 0);
if (!lmbDown && lmbWasDown) QueueInput(Type6, 0);
```

Second arg is host `key`, not a DIK and not native
`[+0]` (unread on type 4 / 6). Classify **MATCH**.
Record shape **LEFTOVER**.

---

## 3. MATCH vs leftover, in order

Native click store walk, then host:

| Native | Host | Class |
| --- | --- | --- |
| LMB down raw 1 → type 4; up raw 4 → type 6 | Silk LMB edge → `Type4` / `Type6` | **MATCH** classify |
| `00A04410` dest + `00A03C80` / `00A03D60` 52 bytes | `(4,0)` / `(6,0)` | **LEFTOVER** dest |
| **`00A66B20` into `this+4`** | `List.Add` | **LEFTOVER** — **first store** |
| `00A66B10` zeros `+13316` each poll | `Pump` `Clear` | **LEFTOVER** (wrong object) |
| `00A66FD0` type-5 hold list | none | **LEFTOVER** (later) |
| `009F57A0` harvest into mux | none | **LEFTOVER** (later) |
| `0042E3EE` type 4 / 6 → 26 / 28 | `ApplyEvent` | **MATCH** |

So: **MATCH** classify. **First leftover** is the
device dest + `00A66B20` append. Later leftovers
(hold list, mux, type-13 `00AB5B49`) are not this
first hole.

`type4-type6-ring` §7 “host still does not queue
type 6” is **STALE**. `host-input-type4` the same.

---

## 4. What not to implement

First-seen Press Start is one LMB down (and later
up). `00A66B20` accepts that. Cap 256, wrap, and a
host 52-byte slab do not change that click.

| Host change at this leftover | Class |
| --- | --- |
| Keep `(type, key)` + LMB classify (current) | classify **MATCH**; array **LEFTOVER** |
| Plant 256×52 / `+13316` / drop 257th | leftover theater for first-seen |
| Invent wrap ring | **DISPROVEN** (`00A66B10`) |
| Store a DIK on type 4 / 6 | **DISPROVEN** |
| Treat `Pump.Clear` as `00A66B10` | leftover comment |
| Implement type-13 `00AB5B49` here | later leftover (`0042E5DC-type13-store`) |

Do not grow `EngineInput` into the mouse device
object for this proof.

---

## 5. Answers

**`00A66B20` mouse array vs host: MATCH or leftover?**
**Leftover.** Classify type 4 / 6 is present. The
256×52 `this+4` store is not.

**First leftover?**
**Yes.** After MATCH LMB classify, the first missing
native store is `00A66B20` (52-byte dest into
`this+4`, cap `0x100`). Not the type-6 id.

No `src/` change in this proof.

---

## Sources

- `proofs/00A66B20-mouse-array/README.md`
- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00a40000.txt`
  (`00A66B10` / `00A66B20` / `00A66F20`)
- `listing-00a80000.txt` (`00AB5923` / `00AB59CB` /
  `00AB5B49`)
- `listing-00a00000.txt` (`00A03C80`, `00A03D60`)
- `e8.tsv`
- `proofs/type4-type6-ring/README.md`
- `proofs/type4-enqueue-ring/README.md`
- `src/Fable.Game/EngineInput.cs`
- `src/Fable.Client/Program.cs`
