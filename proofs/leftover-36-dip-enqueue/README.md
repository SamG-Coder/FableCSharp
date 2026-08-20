# Leftover #36 — DIP enqueue (`009DB700` / `+16020`)

Investigation only. Production `src/` and `tests/` were
not edited. Do not invent dest `512,384`. Do **not**
mark leftover #36 closed.

Question: first-seen frontend `E8` of `009DB700`?
If none, which later path first enqueues 60-byte
records? Recover the `+16020` record layout.
Native first-seen stack dest `[esp+36..48]` at
`0041B173` is still **UNREAD** — do not fill it.

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-009c0000.txt`
(`009DA9F0` / `009DB700` / `009DBFF0` / `009DD8F0` /
`009DE890`);
`listing-00400000.txt` (`0041AFA0` / `0041B173` /
`0041BEB0` / `0042DF9E` / `00404C00` / `00435530`);
`listing-00b80000.txt` (`00BAD8A0` / `00BAE2D0`);
`listing-00a00000.txt` `00A0AAA0`;
`e8.tsv`;
`implementer/frontend/fn-009DB700-exact.txt`,
`fn-0041AFA0-exact.txt`, `fn-0041BEB0-exact.txt`,
`fn-00BAD8A0-exact.txt`, `fn-00BAE2D0-exact.txt`,
`06-dx9-submit.md`;
`src/Fable.Game/FrontendDx9Submit.cs`,
`EngineLifecycle.cs` (`QueueFrontend2dRecord`,
`FlushFrontendDisplay`, `DisplayFlushShouldDip`);
`tests/Fable.Formats.Tests/FrontendDx9SubmitTests.cs`,
`EngineLifecycleTests.cs`;
siblings `proofs/issue-36-verify`,
`proofs/0041AC20-dest-formula`,
`proofs/0041B173-stack-dest`,
`proofs/hud-first-present-skip`,
`proofs/frontend-0042DF9E-status`;
`docs/status/investigations/A-dx9-submit.md`;
`docs/status/README.md` (leave #36 open).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Do not re-prove GraphicIndex leftover, type-6
`0x27` pack, or the dest **formula**. Dest
**numbers** stay in `proofs/0041B173-stack-dest`.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| First-seen frontend `E8 009DB700`? | **None.** Only two `E8` sites exist, both inside display helpers, neither on `0042DF9E` / `0041AFA0` / `00BAD8A0` / `00BAE2D0` | **PROVEN** none |
| Does nonempty dest `E8 009DB700`? | **No.** Draw is factory vtbl+20 `00BAE2D0` → `00A0AEA0` DIPUP | **DISPROVEN** |
| Type-`0x22` pack into `+16020`? | **No.** `0041BEB0` writes a `0xC0` rec, dest `this+0x15C`, `[edx+92]` = `00B23BC0` | **DISPROVEN** |
| First later 60-byte `+16020` enqueue? | First-seen after Leave is still empty. Which `009DD8F0` / `009DBFF0` gate opens first is **UNREAD** | **PROVEN** empty first; producer **UNREAD** |
| 60-byte record layout? | Recovered from `009DB700` writes + `009DE890` copy + `009DA9F0` drain | **PROVEN** offsets; first-seen **values UNREAD** (queue empty) |
| `0041B173` `[esp+36..48]` first-seen? | Still **UNREAD**. Not `512,384` | **UNREAD** |
| Host `DisplayFlushShouldDip(0,0)`? | Always false. Host never stores `[this+16020]` | **MATCH** first-seen empty; later **LEFTOVER** stand-in |
| Host `EnqueuesDisplayQueue`? | Isolated records stay **false** | **MATCH** |
| Host `FrontendEnqueueRan` on nonempty dest? | **true** on PRESS START / install. That is dest-nonempty, **not** a `009DB700` enqueue | **LEFTOVER** name |
| Close leftover #36? | **No.** Dest dump missing; host still pairs dest-nonempty with enqueue language; `+16020` never stored | **LEFTOVER** open |

---

## Verdict

**No first-seen frontend `E8` of `009DB700`.
Leave leftover #36 open.**

`.text` has exactly two `call 009DB700`:
`009DC00E` (`009DBFF0` wrap) and `009DD93D`
(`009DD8F0` HUD string). Frontend Present
`0042DF9E` packs type `0x22` via `0041AFA0` /
`0041BEB0` and draws nonempty dest via
`00BAD8A0` / `00BAE2D0`. It flushes
`009DA9F0(1)` twice on an empty
`[+16020, +16024)` → `009DB6E6` skip DIP.

The first 60-byte `+16020` writer is a
**later** `009DBFF0` / `009DD8F0` take
(game `00435530` debug strings, overlay
`005BCAFE`, or another recovered caller).
First-seen after Leave still takes every
skip (`proofs/hud-first-present-skip`).
Which gate opens first after that is
**UNREAD**. Do not invent a first record.

Host `DisplayFlushShouldDip(0, 0)` is
always false and Notes `009DA9F0(1)
[+16020] empty`. That empty Note is a
stand-in, not a recovered queue read.
`FrontendEnqueueRan` is still set on
nonempty dest. Isolated
`EnqueuesDisplayQueue` stays false.

`0041B173` stack dest is still **UNREAD**.
Do not plant `512,384`.

---

## Evidence → Original → Host → Gap

### 1. Only two `E8 009DB700` — **PROVEN** none on frontend draw

**Evidence** (`listing-009c0000.txt`):

```
009DC00E  call 009DB700     ; inside 009DBFF0 (ret 24 wrap)
009DD93D  call 009DB700     ; inside 009DD8F0 (ret 20 HUD)
```

`fn-009DB700-exact.txt` callees are
`0099AED0` / `00A0AAA0` / `009E1E10` /
`009E1E40` / `0099B7D0` / `009DE890` /
`009E1750` / `009F9F70` / `0099B510`.
No inbound from `0041AFA0` /
`00BAD8A0` / `00BAE2D0`.

`0041AFA0` first-seen pack (`fn-0041AFA0-exact.txt`):

```
0041B47C  … [+376]==0 …
0041B4E6  call 0041BEB0          ; type 0x22, 0xC0
0041B53B  add edi, 0x15C
0041B543  call [edx+92]          ; 00B23BC0 → 00B324A0
```

`00BAD8A0` callees: `009FE620` / `009F9DB0`
only. Early-out `00BADB36 ret 8` when
rec+32 / +64 / +56 are 0. `00BAE2D0`
binds `VSHADER_2D_SPRITE` and DIPUP
`00A0AEA0` vtbl+336. No `E8 009DB700`.

`0042DF9E` drain (`listing-00400000.txt`):

```
0042E129  call 009D9C80
0042E134  push 1
0042E136  call 009DA9F0
0042E13B  call 00404A80
0042E142  call 00404C00          ; first-seen [+8]==0 ret
0042E14D  call 009D9C80
0042E158  push 1
0042E15A  call 009DA9F0
```

No `009DB700` / `009DBFF0` / `009DD8F0`
in that body. Mid-helper `00404C00` is
`test [ecx+8]; je 00404C44 ret`.
`00405125` `009DBFF0` lives in
`00404F60`, not this skip.

**Original:** frontend first-seen never
calls the enqueue. Two queues:

| Path | Insert | Drain | First-seen frontend |
| --- | --- | --- | --- |
| Type `0x22` sprite | `0041BEB0` → dest vtbl+92 | `00BAE2D0` → `00A0AEA0` | dest ctor `0,0,0,0`; `00BAD8A0` early-out; **no** `009DB700` |
| Display `+16020` | `009DB700` only via `009DBFF0` / `009DD8F0` | `009DA9F0` vtbl+332 | empty → `009DB6E6` |
| Type-6 glyphs | `00543910` type `0x27` size 64 | `00AB7C20` → `00A0ABE0` | pen at `+248`; **not** `+16020` |

Sibling `009DB810` also `add […], 60`, but
the vector is `this+15996` (Flush2D
`009D9C80`), not `+16020`. Not this leftover.

**Host:** `FrontendDx9Submit.FirstSeenEmptyDest`
/ `NonemptyDest` set `EnqueuesDisplayQueue =
false`. Tests
`First_seen_dest_zero_does_not_enqueue_or_dip`,
`Nonempty_dest_draws_via_00BAE2D0_not_009DB700`.
`QueueFrontend2dRecord` Notes
`00BAE2D0 no 009DB700` and still sets
`FrontendEnqueueRan = true` when dest
width/height `> 0`.

**Gap:** `FrontendEnqueueRan` means
“nonempty dest took `00BAD8A0` /
`00BAE2D0`”, not “`009DB700` ran”.
`docs/runtime/FORWARD_TREE.md` lines
that pair nonempty dest with `009DB700`
are **STALE**. Isolated flag **MATCH**.
Lifecycle name **LEFTOVER**.

---

### 2. `009DA9F0` empty skip vs host DIP stand-in — **PROVEN** / **LEFTOVER**

**Evidence** (`listing-009c0000.txt`):

```
009DA9F0  sub esp, 104
009DA9F7  mov edx, [ebp+16020]
009DA9FD  mov ecx, [ebp+16024]
009DAA03  sub ecx, edx
009DAA05  mov eax, 0x88888889
009DAA0A  imul ecx
          add edx, ecx
          sar edx, 5              ; count = bytes / 60
009DAA42  je  009DB6E6            ; empty: no DIP
…
009DB5FA  call 00A058C0
009DB61B  push 2                  ; or 4 at 009DB640
009DB645  call [edx+332]          ; DrawIndexedPrimitive
009DB64B  … 009E15E0 +16020
          … 009E1440 +16008
```

No `cmp …, 0x22` in this body. Type-`0x22`
DIP is `00BAE2D0`, not this flush.

**Original:** first-seen frontend and first
game Present (`00435F70` jmp `00435530`)
both take `009DB6E6`. Count is a real
`[+16024]-[+16020]` read.

**Host:**

```
// FlushFrontendDisplay / ApplyDisplayCamera
var shouldDip = DisplayFlushShouldDip(0, 0);
```

`DisplayFlushShouldDip(0, 0)` is
`DisplayQueueCount` on caller begin/end.
Host never allocates or stores
`[this+16020]`. Notes
`009DA9F0(1) [+16020] empty` /
`skip DIP`. `Frontend2dDipIssued =
false` even when
`FrontendEnqueueRan = true`
(`EngineLifecycleTests` install PRESS
START). That empty Note is **not** a
recovered vector read.

**Gap:** first-seen empty **MATCH**.
Later nonempty `+16020` cannot happen
on the host because nothing calls
`009DB700`. Do not treat the stand-in
as a live queue. Do not flip
`Frontend2dDipIssued` from dest
nonempty (that pairing was removed;
do not put it back).

---

### 3. Later 60-byte producers — first take **UNREAD**

**Evidence** — recovered `E8` into the
enqueue family (not frontend draw):

| Site | Wrapper | Gate (first-seen) | Take? |
| --- | --- | --- | --- |
| `009DC00E` | `009DBFF0` | wrap only | only if a caller takes |
| `009DD93D` | `009DD8F0` | wrap; scale `push 0x3F800000` | only if a caller takes |
| `00435A0F` | `009DBFF0` | `[0x1375720]` init `−1`; `jl 00435A36` | **skip** |
| `00435AA4` | `009DD8F0` | `[0x13B8629]` BSS 0 | **skip** (`Recording!`) |
| `00435B57` | `009DD8F0` | `[0x13B86EB]` BSS 0 | **skip** (`Frames behind `) |
| `00435BDD` | `009DD8F0` | `[0x13B860C]` BSS 0 | **skip** (`Skipping unrecorded…`) |
| `00435CC7` | `009DD8F0` | `[0x13B86E7]` BSS 0 | **skip** |
| `00487570` ×11 | `009DD8F0` | `CPlayer+8=0` (`0048A29B`) | **skip** |
| `005BCBA2` | `009DBFF0` | overlay `00639E40`; `00487DD0` miss | **skip** |
| `00405125` / `0040516C` | `009DBFF0` | `00404F60`; not on `00404C00` `[+8]==0` | **skip** first-seen |
| `00487636`… / `00494B59`… / `009BCC54`… | `009DD8F0` | later game / console | **UNREAD** first take |

`009DD8F0` (`listing-009c0000.txt`):

```
009DD931  push 0x3F800000          ; scale 1.0
009DD93D  call 009DB700
```

**Original:** nothing writes a 60-byte
`+16020` record on frontend Present or
first no-save game Present
(`proofs/hud-first-present-skip`).
Those `009DD8F0` strings are capture /
debug UTF-16, **not** `HUD_ORB_*` and
**not** PRESS START glyphs.

**Host:** `ApplyDisplayCamera` Notes the
overlay/interface skips and empty
`009DA9F0`. It does **not** walk the
four `009DD8F0` sites. No 60-byte
record is ever constructed.

**Gap:** first later take is **UNREAD**.
Candidates are the table above. Do not
invent a first record, a dest, or a
`HUD_ORB_*` enqueue to close this.
Do not call `009DB700` from
`00BAD8A0` to “fill” the queue.

---

### 4. 60-byte `+16020` record layout — **PROVEN** offsets

**Evidence** — enqueue `009DB700`
(`ret 24`, `sub esp, 60`). Skip if
`[[this+14908]+472]`. Else build a
local at `[esp+8]` after two saves
and splice into `lea esi, [edi+16020]`:

```
009DB7C6  test ecx, ecx
009DB7CA  lea edx, [esp+8]
009DB7CF  call 009DE890            ; assign 60-byte
009DB7D4  add [esi+4], 60          ; [this+16024] += 60
009DB7DA  … else grow 009E1750
```

Copy `009DE890` (`ret 4`) is the layout
authority (src → dest):

| Off | Size | Writer in `009DB700` | Drain `009DA9F0` |
| --- | --- | --- | --- |
| `+0` | 1 | `(old & ~1) \| 2` | `test al, 0x02` / bit0 vs batch |
| `+4` | 16 | `00A0AAA0` = `[device+0x204]` four dwords | `rep cmpsd` vs clip; `00A0AA80` |
| `+20` | 4 | `0099AED0` zero then `0099B720` string | `add ebx, 20` then font vtbl+20 |
| `+24` | 8 | `0099B7D0` handle (`009F9D00` copy) | `cmp [ebx+24], 0`; `009FA1C0` |
| `+32` | 4 | `fstp` `009E1E10` (`fild`×arg / `0x13961E8`) | `fld [ebx+32]; fadd [ebx+4]` |
| `+36` | 4 | `fstp` `009E1E40` (`fild`×arg / `0x13961F0`) | `fld [ebx+36]; fadd [ebx+8]` |
| `+40` | 4 | `[arg0+8]` | `mov edx, [ebx+40]` |
| `+44` | 4 | arg1 (HUD scale `1.0`) | `push [ebx+44]` |
| `+48` | 4 | arg5 | unread in first 250 of drain |
| `+52` | 4 | `[arg4]` as B,G,R,A bytes | `lea edx, [ebx+52]; push` |
| `+56` | 4 | arg3 object* | `mov ecx, [ebx+56]; call [ecx.vtbl+12]` / `+20` |

`00A0AAA0` is `mov eax, [ecx+14908]; add eax, 0x204; ret`.
`009E1E10` / `009E1E40` are stdcall `ret 4`
normalisers against display qword
`0x13961E8` / `0x13961F0` (same PE
defaults `0042DF9E` requests). Count
magic `0x88888889` / `sar 5` is
`bytes/60` (`009DE940` same helper).

This record is a **HUD / debug blit**,
not type `0x22`. `0041BEB0` layout
(`+0=0x22`, dest `+12..+24`, UV
`+68..+80`, size `0xC0`) is a
different object.

**Original:** first-seen values do not
exist; the vector is empty. Drain of
a nonempty vector: `00A058C0` then
`[device+88].vtbl+332`, stride 32, VB
`this+16008`, prim **2** if
`[esp+16]`, else **4**. Then clear
both vectors.

**Host:** constants **MATCH**
(`DisplayQueueRecordBytes=60`,
`DisplayQueueBeginOffset=16020`,
`DisplayQueueCountMagic=0x88888889`,
`DisplayEnqueueFn=0x009DB700`).
Host never **writes** a 60-byte
record. `Frontend2dRecord` remains
type `0x22` / `0xC0` (or type-6
`0x27` / 64).

**Gap:** do not memcpy a type-`0x22`
`0xC0` rec into `+16020`. Do not
invent first-seen `+32/+36` floats.
Do not treat host dest `512,384` as
this record’s position.

---

### 5. `0041B173` stack dest still **UNREAD** — do not plant `512,384`

**Evidence** (`fn-0041AFA0-exact.txt` /
`listing-00400000.txt`):

```
0041B0DD  mov [esp+36], edx       ; dest X0 = origin +248
0041B0FD  mov [esp+40], eax       ; dest Y0
0041B10D  fstp [esp+44]           ; dest X1
0041B123  fstp [esp+48]           ; dest Y1
0041B173  fld [esp+36]
0041B177  fistp [esp+12]          ; snap; not a widget store
          … same for +40/+44/+48 …
0041B1AF  fstp [esp+48]
```

Snap stays on the **stack**. Later
`0041BEB0` copies a dest pointer into
type-`0x22` rec `+12..+24`. Type-6
`UI_PRESS_START_TEXT` never reaches
`0041B173` (`0054EF00` pen at `+248`).

No process dump, minidump, PIX, or
PNG `tEXt` of those four floats
(`proofs/0041B173-stack-dest`).
`export/native/` is screenshots.
`export/frontend/press-start-dests.txt`
is a host walk.

**Original:** dest **formula** is
recovered (`proofs/0041AC20-dest-formula`).
Dest **numbers** at first-seen
`0041B173` are **UNREAD**. Listing
has no immediate `512` / `384`.

**Host:** tests lock
`UI_PRESS_START_TEXT`
`(512,384,512,384)` —
`320*(1024/640), 240*(768/480)`
applied as a type-0 dest-size rule
to a type-6 point. Forest `410` is
host snap of `256*1.6`. Analog of
the formula, **not** a native dump.

**Gap:** leftover #36 dest-lock.
Do **not** replace host dest with
new invented constants. Do **not**
close #36 from this enqueue note
or from the host 4-tuple.

---

## What is already MATCH (not a close)

| Item | Class |
| --- | --- |
| No frontend `E8 009DB700` | **PROVEN** / **MATCH** notes `no 009DB700` |
| Isolated `EnqueuesDisplayQueue=false` | **MATCH** |
| `DisplayFlushShouldDip(0, 0)` false first-seen | **MATCH** empty |
| `009DA9F0` empty → `009DB6E6` | **PROVEN** |
| Type-0 packer `0041BEB0` / `0x22` / `0xC0` | **MATCH** |
| Type-6 packer `00543910` / `0x27` / 64 | **MATCH** Note |
| Nonempty dest draw `00BAE2D0` | **MATCH** |
| 60-byte count magic / `+16020` / vtbl+332 | **MATCH** constants |
| `0041AFA0` dest **formula** | **PROVEN** elsewhere |

---

## Classification

| Claim | Status |
| --- | --- |
| First-seen frontend `E8 009DB700` | **DISPROVEN** (none) |
| Nonempty dest enqueues `+16020` | **DISPROVEN** |
| `009DA9F0` first-seen DIP | **DISPROVEN** (empty skip) |
| 60-byte layout offsets | **PROVEN** |
| First-seen 60-byte **values** | **UNREAD** (queue empty) |
| First later `009DB700` take | **UNREAD** |
| `0041B173` `[esp+36..48]` numbers | **UNREAD** |
| Host dest `512,384,512,384` | **LEFTOVER** analog |
| Host `FrontendEnqueueRan` on dest | **LEFTOVER** name |
| Host `DisplayFlushShouldDip(0,0)` as a live `[+16020]` read | **DISPROVEN**; first-seen empty **MATCH** |
| Leftover #36 closed | **DISPROVEN** — stays **LEFTOVER** |

**Overall: PARTIAL** (enqueue graph +
layout recovered; first take and dest
numbers **UNREAD**). **Leave #36 open.**

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-009c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00a00000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00b80000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\implementer\frontend\fn-009DB700-exact.txt`
- `C:\FableCSharp\implementer\frontend\fn-0041AFA0-exact.txt`
- `C:\FableCSharp\implementer\frontend\fn-0041BEB0-exact.txt`
- `C:\FableCSharp\implementer\frontend\fn-00BAD8A0-exact.txt`
- `C:\FableCSharp\implementer\frontend\fn-00BAE2D0-exact.txt`
- `C:\FableCSharp\proofs\0041B173-stack-dest\README.md`
- `C:\FableCSharp\proofs\hud-first-present-skip\README.md`
- `C:\FableCSharp\src\Fable.Game\FrontendDx9Submit.cs`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
