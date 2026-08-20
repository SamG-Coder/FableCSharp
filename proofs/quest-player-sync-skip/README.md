# `004AFCA0` skip on first GamePump after Leave

Investigation only. No production `src/` edits.

Question: `004AFCA0` is noted as skip on first GamePump after
Leave. Why skip? Condition? First-seen non-skip later?
Relation to player thing `+48` miss `00A01B50`.

Authority: dump `004AFCA0` / `004B4490` / `00A01B50`
(`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00480000.txt`,
`listing-00a00000.txt`, `listing-00440000.txt`, `listing-004c0000.txt`,
`e8.tsv`);
`docs/runtime/FORWARD_TREE.md` §11, `docs/PARITY.md`
(`004B4490` after `00CB8220` skip);
`EngineLifecycle.PumpQuests` / `QuestPlayerSyncFn`;
`EngineLifecycleTests.Pump_004166E2_is_009E1BC0_minus_game_plus96`;
siblings `proofs/hero-stats-first`, `proofs/creature-after-leave`,
`proofs/hero-4299-create`, `proofs/player-bind-world`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER**.

---

## Verdict

**First GamePump after Leave skips `004AFCA0` because the
player Thing handle is null.** **PROVEN.**

The gate is not “empty `[esi+56]` / skip `00CB8220`” and not
“already synced.” `004B4490` always reaches the player check.
`00487DC0` is `add ecx,44; jmp 00A01B50`. Slot ctor left
`[player+48]=0`, so `00A01B50` returns 0 and
`004B455B je 004B4589` never `E8 004AFCA0`.

| Question | Answer | Class |
|---|---|---|
| Why skip on first type-1 after Leave? | `00A01B50(player+44)` is 0 | **PROVEN** |
| Exact skip condition in `004B4490`? | thing==0 **or** `[thing+145]&1` **or** `movsx(thing+142)==[QM+144]` | **PROVEN** |
| Which of those is first-seen? | thing==0 (`+48` miss) | **PROVEN** |
| `+145` / `+142` involved first-seen? | No. No Thing, so those bytes are not read | **PROVEN** |
| Same as empty-quest `00CB8220` skip? | No. That is earlier in the same pump | **DISPROVEN** |
| First-seen **non-skip** later? | First type-1 `004B4490` **after** `00487CF0` writes a live Thing into `player+44` | **PROVEN** site. Reach of that pump: after `006AC910` / `00489D40` success (Lookout `GuildArrivalHSP`), **not** Leave / first pump |
| Does that first taken call do work? | Player exists; `QM+92` / `QM+96` still ctor sentinels → empty walks | **PROVEN** empty lists. Fill **UNREAD** |
| Other `E8` of `004AFCA0`? | `00524375` (fn `00524010`) | **PROVEN** site. First-seen reach **UNREAD** (0 `E8` / 0 `jmp` to `00524010`) |

Host `PumpQuests` always notes `"004AFCA0 skip"`. That matches
first-seen. After `+44` binds it is **LEFTOVER** vs native.

---

## Call graph (dump)

```
004A5A40 type-1 WorldFrame
  [world+260]==0 || ==9
  004A5D82  mov ecx, [0x13B89FC]
  004A5D88  call 004B4490          // only E8 of 004B4490
    00CB8220 walk [QM+56]          // separate; may skip if empty
    00449970 [game+28] → 004498C0  // slot match [slot+40]
    00487DC0 add ecx,44
      jmp 00A01B50                 // [slot+48] control → Thing*
    edi==0            → skip       // FIRST-SEEN
    [edi+145] & 1     → skip       // dead / torn-down
    movsx [edi+142] == [QM+144] → skip  // already synced
    004B4577  call 004AFCA0        // only other E8: 00524375
    [QM+144] = movsx [edi+142]
```

`e8.tsv`: `004A5D88 → 004B4490`; `004B4577 → 004AFCA0`;
`00524375 → 004AFCA0`. No other `E8`.

First-seen `[world+248]=0`, `[world+260]=0` so `004A5D88`
**does** run on the first type-1 after Leave. **PROVEN.**

---

## Condition (`004B4490` tail)

Dump `listing-00480000.txt` `004B4550`–`004B458D`:

```
004B451C  xor ebx, ebx
…
004B4550  mov ecx, ebp            ; player from 00449970
004B4552  call 00487DC0           ; +44 → 00A01B50
004B4557  mov edi, eax
004B4559  cmp edi, ebx
004B455B  je 004B4589             ; (1) no Thing
004B455D  test [edi+145], 0x01
004B4564  jne 004B4589             ; (2) bit0 set
004B4566  movsx edx, [edi+142]
004B456D  cmp edx, [esi+144]
004B4573  je 004B4589             ; (3) already synced
004B4575  mov ecx, esi
004B4577  call 004AFCA0
004B457C  movsx eax, [edi+142]
004B4583  mov [esi+144], eax
```

Ctor `004B4590` writes `[QM+144]=0xFFFFFFFF` (`004B46BF`).
A live Thing with `+142==0` would **not** take (3) on first
hit (`0 != -1`). First-seen never gets that far.

`004AFCA0` repeats (1) and (2) on entry (`004AFCAF` /
`004AFCB6` / `004AFCBD` / `004AFCC9`) then walks
`[this+92]` and `[this+96]`. Those walks are irrelevant
to the first-pump skip: the `E8` is not taken.

---

## Relation to `00A01B50` / player `+48`

### Smart pointer

```
00A01B10  [this]=vtbl 0x129C95C; [this+4]=0     ; ctor
00A01B50  ecx=[this+4]; if 0 → eax=0
          else eax=[ecx+0]                     ; Thing*
00A01B90  assign Thing* into [this+4] control
00487DC0  add ecx, 44 / jmp 00A01B50           ; alias 00487DD0
```

`player+44` is the handle object. `player+48` is
`handle+4` = control block. **`+48==0` ⇒ `00A01B50` miss.**

### Who zeros `+48` before first pump

| Site | What | First-seen result |
|---|---|---|
| `0044BC10` slot ctor | `lea edi,[esi+44]; call 00A01B10` | `+48=0` **PROVEN** |
| `0048A210` Create Players | temp `00A01B10` / `00A01B90(0)` then `lea ecx,[esi+44]; 00A01B90` | still 0 **PROVEN** |
| `0049F180` → `0048A070` | empty `+52` → `00489D40`; holy-site miss → `ret 0`; no `006AC910` | `+44` still 0 **PROVEN** |

PlayAVI `006286F0` also `00449970` / `00487DC0` during
frontend. That is a lookup, not a bind. **DISPROVEN** as
the first-pump fill.

### Who later fills `+44`

`00489D40` success (`00489FC1 006AC910`) assigns the new
Thing to **`player+52` only** (`0048A027`).
`0048A070` then `00487CF0`: same pointer into **`+52` and
`+44`** (`00487D20` / `00487D56`).

That success is **not** Leave / Init Game / first type-1.
It is the later Lookout create (`GuildArrivalHSP` /
`CThingPlayerCreature::Create`). See
`proofs/hero-4299-create`, `proofs/hero-stats-first` §4.5.

After that write, next `004B4490`:

- `00A01B50` hits
- Thing ctor `004C8FB0`…`004C911A`: `[+142]=0`,
  `[+145]=0x04` (bit0 clear) **PROVEN** on that ctor
- (1) and (2) fail-open
- (3) `0 != 0xFFFFFFFF` → **`004B4577` taken**

Then `[QM+144]=0`. Later type-1s take (3) until `+142`
changes.

---

## First-seen non-skip later

```
0042F2A2 Leave
0042F491 Init Game
  0044BC10 / 0048A210     +48=0
  00416953 LoadWorld
    0049F180 0048A070 00489D40 miss    // still +48=0
004189C2 first pumps
  type-1 004A5A40
    004B4490
      00CB8220 empty-or-yield
      00487DC0 → 00A01B50=0
      004AFCA0 SKIP                    // THIS NOTE
… later region apply …
  00489FC1 006AC910
  00487CF0 player+44 = hero
next type-1 004B4490
  00A01B50 hit, +145 bit0=0, +142 != QM+144
  004AFCA0 RUNS
    QM+92 / QM+96 sentinel-only        // empty notify
  QM+144 = +142
```

First **taken** `004AFCA0` is therefore **after** first
real Hero Thing bind, on the next type-1 that still has
`[world+260]` 0 or 9. Exact WorldFrame index is
**UNREAD** (depends when `006AC910` is first reached;
not first GamePump, not `00501450` on the host’s first
pumps).

`00524375` can also take the call if `00524010` runs
while a non-player Thing is walked and `00487DC0` already
hits. `00524010` has **no** dump `E8` / `jmp`. Do not
treat it as first-seen.

---

## What `004AFCA0` is (when taken)

Dump `004AFCA0`–`004AFD7C`:

1. Resolve player Thing (`00449970` / `00487DC0`). Same
   (1)(2) early-out.
2. Walk circular `[QM+92]`, then `[QM+96]`.
3. Per node: `00A01B50(node+8)`. Need Thing,
   `!(+145 & 1)`, `(+145 & 0x20)`, `[thing+96]!=0`.
4. `004C73D0(player)` = `player+96+12`;
   `call [comp.vtbl+124]`.

Ctor `004B4612` / `004B4624` make both lists
`[node]=[node+4]=self`. First taken call still notifies
**zero** components. **PROVEN.** List fill is **UNREAD**.

Not region load. Not `00501450`. Not Oakvale activate.

---

## Host

`EngineLifecycle.QuestPlayerSyncFn = 0x004AFCA0`.
`PumpQuests` notes `00A01B50 +48=0 miss` then
`004AFCA0 skip`. Test
`Pump_004166E2_is_009E1BC0_minus_game_plus96` asserts
both. Correct for first-seen. **PROVEN.**

Do not invent a taken `004AFCA0` on Leave / first
GamePump. Do not collapse this skip into the
`00CB8220` empty-list skip.
