# Host apply: keep `FloorKey`, do not ship slerp

Investigation only. No production `src/` or `tests/` edits.

Native live clip rotation is slerp `00A4C1F0` plus a
small-angle **unnormalized** lerp. It is **not** nlerp.
Host `PaletteForPose` still floors the key.

Question: can first-seen apply `FloorKey` MATCH native?
When does `00A4C1F0` run vs `FloorKey`? Exact frac source?
If MATCH keep `FloorKey`, keep host. If MATCH ship slerp
with proven frac, list the exact helper change + tests.
Do **not** nlerp. Do **not** invent interpolation on first
Present. Do **not** ship `Quaternion.Slerp` blindly.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `proofs/00A4C1F0-xseq-slerp-kernel`;
`listing-00a40000.txt` `00A4C1F0`–`00A4C5DB` /
`00A4DEA0` / `00A52650`;
`listing-00a80000.txt` `00AA0090`–`00AA09E3`;
`listing-00bc0000.txt` packer `00BD2D90`–`00BD2E35`;
`src/Fable.Formats/WorldShading.cs` `PaletteForPose` /
`TimeToKey` / flags;
`src/Fable.Formats/Anims/XSeqFile.cs` `FloorKey` /
`ApplyLocals` / `RotationAt`;
`src/Fable.Formats/Meshes/MeshFile.cs` `TrianglesForPose`;
`src/Fable.Game/RegionTravel.cs`
`FirstSeenPlayAnimationAppliesPose`;
tests `XSeqFormatTests`;
`proofs/xseq-00AA0090-interp`;
`proofs/xseq-slerp-threshold`;
`proofs/palskin-child-hero`;
`proofs/anim-blend-first`.

---

## Verdict

**MATCH keep `FloorKey`. Do not ship slerp.**

First-seen no-save PALSKIN pack calls `00AA0090` with
mixer channel count **0**. That skip never calls
`00A4C1F0` and never produces a frac. Dest is C3D bind
locals through `00A9E1E0`. Host first Present is the same
bind product (`FirstSeenPalettes` / file triangles).
`FirstSeenPlayAnimationAppliesPose=false`.
`FirstSeenXseqAppliesFrac=false`.
`FirstSeenPlaysAnim=false`.

Applying `FloorKey` of a clip on first Present would
**DIVERGE** (native is bind, not first-key). Host does
**not** do that. Format tests that pass a sequence at
`time=0f` get `FloorKey=0` / `RotationAt(0)`, which also
**MATCH**es `00A4C1F0` dest at `frac=0` (scale0=1,
scale1=0). That is a format experiment, not first Present.

`00A4C1F0` runs only on the leftover live mixer walk
(count &gt; 0). Frac then comes from `00A52650`
(`time * [clip+80] − key` after the 0.5-bias floor),
fed as arg2. Wrapper `00A4DEA0` (`time * [track+4]`) is
**not** the PALSKIN caller. Host `TimeToKey` **MATCH**es
the `00A52650` listing and **drops** `.Frac` in
`FloorKey`. Live frac apply stays **LEFTOVER**.

`NativeXseqRotationIsSlerp=true` / `XseqSlerpFn=0x00A4C1F0`
name the **native** kernel. They are not a license to
call `Quaternion.Slerp` (or nlerp) on dest. Native
small-angle is unnormalized lerp at `1-|cosom| <= 0.05`.
BCL Slerp is a different kernel. Time interp body of
`00AA0090` is recovered as mixer evaluate; first-seen
does not run it (`00AA0090` unread as apply).

**No `src/` change. No new test.** Existing
`XSeqFormatTests.Xseq_persist_addrs_match_00a999b0_and_00aa4680`
already locks the three flags and `TimeToKey(0,…)=(0,0)`.

---

## Evidence → Original → Host → Gap

| Evidence | Original | Host | Gap |
|---|---|---|---|
| packer `00BD2E35` | first PALSKIN evaluate `00AA0090` | no mixer object | **MATCH** call site. Evaluate leftover |
| `00AA0160` `test eax,eax` / `jbe 00AA097D` | count 0 skips sample | `FirstSeenPalettes` bind | **MATCH** first-seen. No `00A4C1F0` |
| skip tail `00AA09B7` `00A9E1E0` | hierarchy × IBM, dest ≈ I | `FirstSeenPalettes` / file triangles | **MATCH** first Present |
| `FirstSeenPlayAnimationAppliesPose` | first Lookout frame is bind | `false` | **MATCH**. Do not invent pose |
| `PaletteForPose` null / no tracks | bind locals | `FirstSeenPalettes(bones)` | **MATCH** |
| `PaletteForPose` with sequence | leftover live sample | `ApplyLocals(..., FloorKey(time))` | **MATCH** first-seen (unused). Live frac **LEFTOVER** |
| `FloorKey` drops `TimeToKey.Frac` | first-seen never feeds frac | same | **MATCH** first-seen |
| `time=0` `TimeToKey` `(0,0)` | dest = key 0 even if kernel ran | `RotationAt(0)` | **MATCH** key 0. Tests lock it |
| `00A52650` then push frac, key, dest | live PALSKIN frac source | `TimeToKey` **MATCH** listing; `.Frac` discarded | **PARTIAL** live. First-seen unused |
| `00A4DEA0` `time*[track+4]` | 44-byte wrapper, `ret 8` | unused | **UNREAD** as PALSKIN path |
| `00A4C1F0` slerp + unnormalized lerp | live kernel | none | **PROVEN** native. **DISPROVEN** nlerp. **LEFTOVER** apply |
| `Quaternion.Slerp` / nlerp on dest | not this listing | not shipped | keep out |
| flags `NativeXseqRotationIsSlerp` / `XseqSlerpFn` / `FirstSeenXseqAppliesFrac` | slerp kernel, first-seen no frac | `true` / `0x00A4C1F0` / `false` | **MATCH** |

---

## 1. First-seen apply `FloorKey` **MATCH** native

Native first Present never samples a clip.

PALSKIN packer (`listing-00bc0000.txt`) when
`[helper+288]==0`:

```
00BD2DB2  esi = [mesh+152]            ; n
00BD2DE4  alloc n*64 → [helper+288]
00BD2DFD  fld [0x143B934 + [helper+44]*12]
00BD2E16  push 1                      ; flag
          push esi                    ; n
          push [helper+288]           ; dest
          push mesh
          push 00B83750               ; cache (0 first-seen)
          push t                      ; packer blend
          push helper+124             ; source B
this = [[mesh+80]+4]+960              ; mixer
          push helper+116             ; source A
00BD2E35  call 00AA0090
```

`00AA0090` (`listing-00a80000.txt`):

```
00AA0142  eax = [sourceA+4]
          count = ([+16]-[+12]) * 0x66666667 sar 3     ; /20
00AA0160  test eax, eax
00AA0162  jbe 00AA097D                                 ; FIRST-SEEN
…
00AA097D  optional 00A9DFA0 if arg3 != 0
00AA09B7  call 00A9E1E0                                ; always
00AA09E3  ret 32
```

First New Game pack: count **0**. The channel walk that
pushes key/frac and `call 00A4C1F0` (`00AA025E` /
`00AA02F1` / `00AA05A6` / `00AA0601`) does **not** run.
Dest is C3D bind 48-byte locals × 64-byte IBM. Product ≈
identity. `anim-blend-first` / `xseq-00AA0090-interp` /
`00A4C1F0-xseq-slerp-kernel` already lock this.

Host:

| Site | What runs | Class |
|---|---|---|
| first Present submit | `MeshFile.Triangles` (file verts), not `TrianglesForPose(clip)` | **MATCH** bind. `WorldGeometryTests.Palskin_submit_uses_file_triangles_not_repose` |
| `MeshFile` parse palettes | `FirstSeenPalettes(bones)` | **MATCH** |
| `PaletteForPose(..., sequence: null)` | `FirstSeenPalettes(bones)` | **MATCH** |
| `PaletteForPose(..., sequence, time)` | `ApplyLocals(bones, FloorKey(time))` then `FirstSeenPalettes` | unused on first Present |
| `TrianglesForPose(sequence)` | `PaletteForPose(..., 0f, sequence)` | format experiment, `time=0` |

`RegionTravel.FirstSeenPlayAnimationAppliesPose=false`.
Create `006AC910` / ConstructFromParams `006A9DD0` /
activate `004C9CA0` do not call `PlayAnimation` /
`0070D580`. `xseq-walk-first`: first Lookout frame is
bind.

**DISPROVEN:** first-seen applies frac slerp.
**DISPROVEN:** first Present applies a clip `FloorKey`.
**MATCH:** host bind palettes / floor-key helper that is
not fed a live clip on first Present.

Do **not** invent interpolation on first Present. Feeding
`PaletteForPose` a wake/idle sequence at `t>0` on that
frame would invent a product native never builds.

---

## 2. When `00A4C1F0` runs vs `FloorKey`

Two clocks. Do not collapse them.

### 2a. First-seen — `FloorKey` unused, kernel unused

Channel count 0 → skip. Host `FloorKey` is not the native
first-seen sampler. Native sampler is “do not sample.”
Host MATCH is bind, not “floor the first clip.”

### 2b. Live mixer — kernel **would** run

Count &gt; 0 falls through `00AA0162`. Per 20-byte
channel (`add ecx, 20` at `00AA095C`):

1. index `+8` → `00A242C0` / `00A26C60` bind clip.
2. lerp times `+12` by packer `t`.
3. `call 00A52650` → integer key + frac.
4. lerp weights `+16`.
5. `[clip+8]` bit 2 (`shr 2; test cl,1`) plus `|w-1|`
   vs `[0x129BA3C]` and `[track+10]` sign select one of
   four `call 00A4C1F0` sites.

| Site | Arm |
|---|---|
| `00AA025E` | bit 2 set, `|w-1|` small: sample then `00A88C10` copy 16 bytes into local+0 |
| `00AA02F1` | bit 2 set, weight not ~1: sample then weighted accumulate (**UNREAD** as host math) |
| `00AA05A6` | bit 2 clear + `[clip+8]` bit 1: copy using `[clip+104]` mask |
| `00AA0601` | sibling accumulate into scratch |

Count &gt; 0 is **necessary**, not sufficient: bone count,
`[track+10]` sign, and the weight/bit tests still gate
each call. That walk is leftover live, **DISPROVEN** as
first-seen New Game.

Host `FloorKey` is the format stand-in for “integer key
from `TimeToKey`, no blend.” It is **not** a port of
`00A4C1F0`.

### 2c. `frac=0` coincidence (format tests only)

At `frac=0` both native arms write dest = key A
(`scale0=1`, `scale1=0`). `XSeqFormatTests` pose samples
at `time=0f` therefore **MATCH** `RotationAt(0)` even if
the kernel had run. They cannot prove blend.
`TrianglesForPose(sequence)` hard-codes `0f`. Submit
does not apply those posed triangles.

---

## 3. Exact frac source

### 3a. PALSKIN live path — `00A52650` (the one that feeds `00A4C1F0`)

`00AA0090` (`listing-00a80000.txt`):

```
00AA01B8  fld [esi+12]                ; A.time
00AA01BB  fld [edi+12]                ; B.time
          fsub st, st(1)              ; B-A
          fmul packer_t
          fadd st, st(1)              ; A + (B-A)*t
00AA01DD  call 00A52650               ; ecx = 00A26C60 clip
00AA0239  mov ebp, [esp+60]           ; frac out
00AA023D  mov ebx, [esp+64]           ; key out
00AA0254  push ebp                    ; arg2 frac
          push ebx                    ; arg1 key
          lea edx, [esp+308]
          push edx                    ; arg0 dest
00AA025E  call 00A4C1F0               ; ecx = [clip+92][bone*8]
```

`00A52650` (`listing-00a40000.txt`, `ret 12`):

```
fld [ebp+8]                           ; mixer time
fmul [ecx+80]                         ; ClipRateOffset = 80
fst  scaled
fsub 0.5 (0x3F000000)
fistp key
if scaled == key+1: inc key
frac = scaled - key                   ; before wrap
div [ecx+84]                          ; ClipWrapOffset = 84
key = remainder; if <0 add wrap
store (key*, frac*)
```

Host `WorldShading.TimeToKey` **MATCH**es that listing.
`XSeqFormatTests` locks `(0,0)` at t=0 rate 15 wrap 8;
mid key 1 at t=0.1 rate 15 wrap 30.

`[clip+80]` / `[clip+84]` vs XSEQ fps / period stay
**PARTIAL**. `FloorKey` maps `Tracks[0].SamplesPerSecond`
and `FrameCount & 0xFF`. `WorldShading` comment: does
**not** map XSEQ fps onto `[clip+80]`. Period **width**
**MATCH**es `00A4C1F0` `movzx [ecx+8]`. Clip `+84` vs
that byte is still **PARTIAL**.

`FloorKey` keeps `.Key` and **drops** `.Frac`. First-seen
never produces the frac, so dropping it **MATCH**es.

### 3b. Sibling wrapper — `00A4DEA0` (not PALSKIN)

```
fld time
fmul [ecx+4]                          ; 44-byte track scale
fistp key (same 0.5 bias / +1 fix)
frac = scaled - key
movzx esi, [ecx+8]
idiv esi                              ; key %= period
push frac
push edx                              ; key
push dest
call 00A4C1F0
ret 8
```

`00AA0090` does **not** call `00A4DEA0`. Do not treat
track `+4` as the first-seen frac source.

### 3c. `00A4C1F0` args when it **does** run

Thiscall + `ret 12`. `ecx` = 44-byte track
(`XSeqFile.ClipRecordBytes`).

| Slot | Native | First-seen |
|---|---|---|
| `ecx` | 44-byte track | not called |
| arg0 | dest | — |
| arg1 | integer key | — |
| arg2 | frac in `[0,1)` from `00A52650` | — |

Entry:

```
00A4C1F0  sub esp, 52
00A4C1F3  movzx edx, [ecx+8]          ; period = low 8 of +8
00A4C1F7  fld [0x122DED8]             ; 1.0
00A4C1FD  fsub [esp+64]               ; 1 - arg2 frac
00A4C201  push ebx
00A4C202  mov ebx, [esp+64]           ; arg1 key
```

Next key wraps: `key+1 < period ? key+1 : 0`. Rotation
then slerp / small-angle lerp as in
`proofs/00A4C1F0-xseq-slerp-kernel`. Threshold
`[0x125A15C]=0.05` (`proofs/xseq-slerp-threshold`).

No proven first-seen (or first Present) frac exists to
ship.

---

## 4. Host stays `FloorKey` — no slerp change

`WorldShading`:

```
NativeXseqRotationIsSlerp = true
FirstSeenXseqAppliesFrac  = false
XseqSlerpFn               = 0x00A4C1F0
```

`PaletteForPose`:

```
if sequence is null or no tracks → FirstSeenPalettes(bones)
else FirstSeenPalettes(sequence.ApplyLocals(bones, FloorKey(time)))
```

`XSeqFile.FloorKey`:

```
period = Tracks[0].FrameCount & 0xFF
TimeToKey(time, SamplesPerSecond, period).Key   // Frac dropped
```

`ApplyLocals` / `TrySample` / `RotationAt(key)` index
`RotationKeys` with wrap by count. No slerp. No nlerp.
Translation stays `FirstTranslation` (later keys
**UNREAD**, not this rotation question).

### Why not `Quaternion.Slerp`

| Native `00A4C1F0` | `System.Numerics.Quaternion.Slerp` |
|---|---|
| shortest-path sign on `B` weight | typically flips `B` |
| `1-\|cosom\| <= 0.05` → **unnormalized** component lerp | small-angle path is not this 0.05 / no-normalize kernel |
| else table `acos`/`sin` + `fsqrt` `sin(ω)`, dest **not** normalized | different `sin`/`acos`, usually unit dest |

Shipping `Slerp(q0, q1, TimeToKey.Frac)` on
`PaletteForPose` would:

1. Invent a live blend first-seen never runs.
2. Still not be the table kernel.
3. Be invisible to current format tests (`time=0f` →
   frac 0 → same as `FloorKey`).
4. Change first Present only if someone also wired a
   clip into submit — that wiring is itself
   **DISPROVEN** (`FirstSeenPlayAnimationAppliesPose=false`).

nlerp (`normalize((1-t)A + t B)`) is **DISPROVEN**: the
small-angle arm writes dest with no `fsqrt`.

### Exact code change if slerp were MATCH — **none**

There is **no** MATCH to ship slerp with a proven
first-seen frac. Do not edit:

- `WorldShading.PaletteForPose`
- `XSeqFile.FloorKey` / `ApplyLocals` / `RotationAt`
- `MeshFile.TrianglesForPose` (`0f` stays)

Do not add a `Quaternion.Slerp` helper test. Keep the
existing flag locks in
`XSeqFormatTests.Xseq_persist_addrs_match_00a999b0_and_00aa4680`.

Port `00A4C1F0` only when a listed first-seen (or later
chosen product) path actually calls it with the args in
§3c, using `00A52650` frac, not BCL Slerp.

`00AA0090` mixer time lerp remains **UNREAD** as apply
(body recovered; first-seen count 0; host has no mixer).

---

## Classifications (short)

1. **First-seen apply `FloorKey` MATCH native — MATCH
   bind, not MATCH “floor a clip.”** Count 0 skip →
   `00A9E1E0`. Host first Present is bind.
   `FirstSeenPlayAnimationAppliesPose=false`.
2. **`00A4C1F0` vs `FloorKey` — PROVEN split.** Kernel
   runs only on leftover live mixer (count &gt; 0, four
   call sites after `00A52650`). Host floors the key and
   drops frac. First-seen uses neither as a blend.
3. **Exact frac source — PROVEN for live PALSKIN:
   `00A52650` `scaled-key` after `time*[clip+80]`.**
   Mixer time is `A.t+(B.t-A.t)*packer_t`. Wrapper
   `00A4DEA0` is a different caller. First-seen produces
   **no** frac.
4. **Keep host `PaletteForPose` floor-key.**
   `NativeXseqRotationIsSlerp=true` is the native live
   kernel flag, not a license to slerp dest now.
5. **Do not ship `Quaternion.Slerp` or nlerp.** Native
   is slerp + unnormalized lerp. First Present must not
   grow interpolation.

Do not treat `xseq-00AA0090-interp` “do not slerp” as
“native is not slerp.” Native **is** slerp. Host must
not grow one on the first-seen floor-key path.
