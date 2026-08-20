# Frontend `0042E3EE`: `009F4ED0` once, then `009F4F10` until miss

Investigation only. No production `src/` edits.

Question: Frontend `0042E3EE` does `009F4ED0` once then
`009F4F10` until miss. If one harvest has type 4 then type 6,
both apply same frame? Host `QueueInput` leftover?

Authority: dump `Fable.exe` `0042E3EE` / `0042EC7C` /
`0042F0AC` / `009F4ED0` / `009F4F10` / `009F4AC0` /
`00A03B40` (`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`,
`listing-009c0000.txt`); `functions.tsv` (`0x0042EC7C` lists
`0042E3EE`); `src/Fable.Game/EngineInput.cs`,
`src/Fable.Game/EngineLifecycle.cs` (`QueueInput` /
`PumpInput` / `PumpFrontendFrame`),
`src/Fable.Client/Program.cs`;
`proofs/type4-type6-ring/README.md`,
`proofs/type6-same-poll-as-4/README.md`,
`proofs/host-input-type4/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH** / **STALE**.

Do not re-prove type 4 = LMB down → action 26, type 6 = LMB up
→ action 28, 52-byte record / mux `+28`, or that `009F4ED0`
is first-slot only (`type4-type6-ring`).

---

## Verdict

**Yes.** One `0042E3EE` harvests once (`[0x13B8388].vtbl+8`),
copies slot 0 with **one** `009F4ED0`, classifies, then
`009F4F10` / `jne 0042E453` until `al==0`. Type 4 and type 6
are both nonzero (`00A03B40`), so the second-record walk does
not skip them. If this harvest is type 4 then type 6, the
same `0042E3EE` (same `0042EC7C` frame, before `0042DC94`)
applies **26 then 28**. There is no “already did 26, defer
28” gate.

**Host `QueueInput`:** the engine queue + `Pump` **MATCH**
that walk when both records are already queued. Live
`Program.cs` LMB edge cannot enqueue 4 and 6 in one
`Update` — that is the leftover. Older “host never queues
type 6” notes are **STALE**.

| Claim | Status |
| --- | --- |
| `0042E3EE` calls `009F4ED0` once | **PROVEN** `0042E449` then `jmp 0042E803` |
| Later records are `009F4F10` until miss | **PROVEN** `0042E7FE` / `test al` / `jne 0042E453` |
| Miss ends the walk; `009F4AC0` after the loop | **PROVEN** `0042E80B` |
| `009F4F10` skips type 4 or 6 | **DISPROVEN** — skip is type 0 only |
| One harvest type 4 then type 6 → both apply this `0042E3EE` | **PROVEN** classify → `0055CB10` → next record |
| Type 6 waits for a later frontend frame when both harvested | **DISPROVEN** |
| Same `0042EC7C` as fill/draw | **PROVEN** `0042F0AC` then `0042DC94` / `0042DF9E` |
| Live `GetDeviceData` down+up in one harvest | **UNREAD** (listing-only; shape already in `type4-type6-ring`) |
| `EngineInput.Pump` applies every queued record this pump | **MATCH** |
| Live host `QueueInput`s type 4 and type 6 same `Update` | **DISPROVEN** — exclusive edges |
| Live host never queues type 6 | **STALE** — `Program.cs` LMB-up is type 6 |
| `QueueInput` comment / `InputPollFn` name only `009F4F10` / only `009F4ED0` | **LEFTOVER** labels |

---

## 1. First slot vs next slot

`009F4ED0` (`listing-009c0000.txt`):

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

`009F4F10`:

```
edi = [iter+4]
inc edi
while edi < [this+28]:
  esi = [this+16] + edi*52
  if 00A03B40(esi) != 0: copy 52; [iter+4]=edi; al=1
  else edi++
al = 0
```

Type 4 and type 6 both have `[+40] != 0`. Empty mux is the
only `009F4ED0` miss. Type 0 is the only `009F4F10` skip.

---

## 2. `0042E3EE` walk (**PROVEN**)

`listing-00400000.txt`:

```
0042E42C  mov ecx, [0x13B8388]
          call [eax+8]              ; harvest
0042E43D  and [ebp-4], 0
0042E449  call 009F4ED0             ; first
          jmp 0042E803
0042E453  lea ecx, [ebp-80]
          call 00A03B40
          … classify / maybe 0055CB10 …
0042E4A4  push 26                   ; type 4
          jmp 0042E5AB
0042E498  push 28                   ; type 6
          jmp 0042E5AB
0042E5AB  call [edx]                ; 0055CB10
          jmp 0042E7F0
0042E7F0  mov ecx, [0x13B8388]
0042E7FE  call 009F4F10             ; next
0042E803  test al, al
          jne 0042E453
0042E815  call 009F4AC0
          … [ebp-4] priority encoder …
```

`009F4ED0` is not in the loop. The join at `0042E803` is
shared: first success falls in from `0042E44E`; later
successes come from `0042E7FE`. Dest is `[ebp-80]` each
time, so apply happens **before** the next overwrite.

Type 4 / 6 do not OR `[ebp-4]`. They `push` and `call [edx]`
then go to `0042E7F0`. The mask encoder after `009F4AC0` is
a different family (keys / analog). It cannot defer 28.

`[this+312] != 0` (`0042E3FA`) skips the whole walk
(`009F5540`). That is not a “second record later” path.

---

## 3. Same frontend frame

`0042EC7C` (`functions.tsv` callee list includes `0042E3EE`):

```
0042F0AC  call 0042E3EE
0042F0B3  call 0042DC94
0042F0BB  call 0042FA30
0042F0C6  call 0042DBFA
0042F0D1  call 0042DF9E
```

One input walk, then UI tick / fill / draw. Both applies
finish inside `0042E3EE`, so they are the same
`0042EC7C` frame.

| Mux this harvest | `009F4ED0` | later `009F4F10` | this frame |
| --- | --- | --- | --- |
| 4 only | 4 → 26 | miss | 26 |
| 6 only | 6 → 28 | miss | 28 |
| 4 then 6 | 4 → 26 | 6 → 28 | **26 then 28** |
| 6 then 4 | 6 → 28 | 4 → 26 | 28 then 26 |

Cross-frame click (down harvest, later up harvest) is two
`0042E3EE`s. That is still FIFO, not a skip of 28.

Live down+up in one `GetDeviceData` remains **UNREAD**.
The consume rule does not care: if both records are in
`[mux+16]`, both apply now.

---

## 4. Host `QueueInput`

`EngineLifecycle.QueueInput` is `Input.Queue`. `PumpInput`
→ `EngineInput.Pump` walks the whole list, then `EndPoll`.
That **MATCH**es `009F4ED0` + `009F4F10` until miss when
the host already appended both records (tests do:
`QueueInput(Type4)` then `QueueInput(Type6)` then one
`Pump`).

Live `Program.cs`:

```
if (lmbDown && !lmbWasDown)
    life.QueueInput(EngineInput.Type4, 0);
if (!lmbDown && lmbWasDown)
    life.QueueInput(EngineInput.Type6, 0);
lmbWasDown = lmbDown;
```

Those predicates cannot both be true. One `Update` queues
at most one of {4, 6}. Native harvest can hold both.
That is the leftover: host edge-split vs mux FIFO.

`host-input-type4` / `type6-record-layout` “never queues
type 6” is **STALE**. LMB-up is type 6.

Label leftovers only (no consume-bug):

- `QueueInput` xml says “one `009F4F10` record”. Slot 0
  is `009F4ED0`.
- `EngineLifecycle.InputPollFn` / `PumpFrontendFrame` Note
  name `009F4ED0` only. The second-record call is missing
  from the constant / trace string.

`TryDequeue` is first-only (**MATCH** one `009F4ED0`).
Player-interface `00446330` uses the same pair; not this
frontend leftover.

---

## 5. Answers

**`0042E3EE` does `009F4ED0` once then `009F4F10` until
miss?**
**Yes.** One harvest, one first-slot copy, then next-slot
until `al==0`, then `009F4AC0`.

**If one harvest has type 4 then type 6, both apply same
frame?**
**Yes.** Same `0042E3EE` / same `0042EC7C`. 26 then 28.
Type 6 is not held for the next frame when both records
were harvested.

**Host `QueueInput` leftover?**
**Pump MATCH** if both are queued. **Live LMB leftover:**
cannot queue 4 then 6 in one `Update`. Poll-fn labels
still name only one of the two natives.

No `src/` change in this proof.

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
  (`0042E3EE`, `0042E449`, `0042E453`, `0042E498` /
  `0042E4A4`, `0042E5AB`, `0042E7F0` / `0042E7FE`,
  `0042E815`, `0042EC7C` / `0042F0AC`)
- `listing-009c0000.txt` (`009F4ED0`, `009F4F10`, `009F4AC0`)
- `functions.tsv` (`0x0042EC7C`)
- `src/Fable.Game/EngineInput.cs`
- `src/Fable.Game/EngineLifecycle.cs`
- `src/Fable.Client/Program.cs`
- `proofs/type4-type6-ring/README.md`
- `proofs/type6-same-poll-as-4/README.md`
- `proofs/host-input-type4/README.md`
