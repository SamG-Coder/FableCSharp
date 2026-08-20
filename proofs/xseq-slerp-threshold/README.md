# `00A4C1F0` small-angle threshold `[0x125A15C]` = `0.05`

Investigation only. No production `src/` or `tests/` edits.
Do **not** ship `Quaternion.Slerp`. Do **not** change
`PaletteForPose` off `FloorKey`.

Parent leftover `proofs/00A4C1F0-xseq-slerp-kernel` proved
the kernel is slerp + unnormalized lerp, and that first-seen
is `FloorKey` MATCH. Decimal bits of `[0x125A15C]` were
left as a dump. This packet is that dump.

Question: exact `[0x125A15C]` value? If first-seen channel
count were `> 0`, would `00A4C1F0` run? Is the PALSKIN
`00AA0090` skip MATCH?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `floats 0x125A15C` on TLC `Fable.exe`
(`exeId 42D7DBDF-0106C000-16666624`, same as
`assembly/exe/manifest.json`);
`listing-00a40000.txt` `00A4C2B5`–`00A4C3FE`;
`listing-00a80000.txt` `00AA0142`–`00AA09B7`;
`listing-00bc0000.txt` packer `00BD2E16`–`00BD2E35`;
`src/Fable.Formats/WorldShading.cs` `PaletteForPose`;
`src/Fable.Formats/Anims/XSeqFile.cs` `FloorKey`;
tests `XSeqFormatTests`;
`proofs/00A4C1F0-xseq-slerp-kernel`;
`proofs/xseq-00AA0090-interp`.

---

## Verdict

**Threshold is `0.05` (`0x3D4CCCCD`). First-seen PALSKIN
`00AA0090` skip MATCH. Kernel leftover. Keep floor-key.**

`floats 0x125A15C 8`:

```
0x0125A15C          0.05  0x3D4CCCCD
0x0125A160           127  0x42FE0000
0x0125A164         0.007  0x3BE56042
```

`00A4C1F0` compares `1 - |cosom|` to that dword:

```
00A4C2B5  fld [0x122DED8]             ; 1.0  (0x3F800000)
00A4C2BB  fsub [esp+76]               ; 1 - |cosom|
00A4C2BF  fcomp [0x125A15C]           ; 0.05
00A4C2C5  fnstsw ax
00A4C2C7  test ah, 0x41               ; C0|C3  →  <=
00A4C2CA  jne 00A4C3FE                ; lerp, no fsqrt
```

So **`1 - |cosom| <= 0.05`** (`|cosom| >= 0.95`) takes the
unnormalized component lerp. Else table slerp + `fsqrt`.
Neighbors `127` / `0.007` are **not** this compare.

First-seen PALSKIN pack `00BD2E35` → `00AA0090` with
channel count **0** → `jbe 00AA097D` → `00A9E1E0`. That
skip **never** calls `00A4C1F0`. Host `PaletteForPose` →
`FloorKey` / bind palettes **MATCH** the skip.
`FirstSeenXseqAppliesFrac=false`.

If channel count were `> 0`, the `jbe` would fall through
and the walk **would** call `00A4C1F0` (four sites). That
is the leftover live mixer, **not** first-seen New Game.
Count `> 0` is necessary; `[clip+8]` bit 2, `|w-1|`,
`[track+10]` sign, and bone count still gate each call.

Do **not** replace `FloorKey` with
`Quaternion.Slerp`. Native small-angle is `0.05` on
`1-|dot|` and dest is **not** normalized. BCL Slerp is a
different kernel (`~1e-6` on `|dot|`, normalized).

---

## Evidence → Original → Host → Gap

| Evidence | Original | Host | Gap |
|---|---|---|---|
| `floats 0x125A15C` | `0.05` `0x3D4CCCCD` | none | **PROVEN** bits |
| `00A4C2BF` `fcomp` / `test ah, 0x41` / `jne 00A4C3FE` | `1-\|cosom\| <= 0.05` → unnormalized lerp | none | **PROVEN** branch. **DISPROVEN** nlerp |
| else `fsqrt` + tables `0x13CB530` / `0x13CD550` | slerp | none | **PROVEN** live. **LEFTOVER** first-seen |
| `[0x122DED8]` / `[0x122DEDC]` | `1.0` / `0.0` | none | **PROVEN** rdata |
| `[0x129BA3C]` | `0.0001` — mixer `\|w-1\|`, not this compare | none | **PROVEN** different constant |
| packer `00BD2E35` | first PALSKIN evaluate | no mixer | **MATCH** call site |
| `00AA0160` `test eax,eax` / `jbe 00AA097D` | count 0 skips `00A4C1F0` | `FirstSeenPalettes` bind | **MATCH** skip |
| count `> 0` fall-through | walk `00A52650` then `00A4C1F0` ×4 | unused first-seen | **LEFTOVER**. Would run |
| `PaletteForPose` / `FloorKey` | floor key, drop frac | same | **MATCH** first-seen |
| `Quaternion.Slerp` | not this listing | not shipped | keep out |

---

## 1. Exact dword

Dump (`dotnet run --project tools/Fable.ExeIndex -- floats 0x125A15C 8`)
on the same TLC image as `assembly/exe`:

| VA | Float | Bits |
|---|---|---|
| `0x0125A15C` | **`0.05`** | **`0x3D4CCCCD`** |
| `0x0125A160` | `127` | `0x42FE0000` |
| `0x0125A164` | `0.007` | `0x3BE56042` |

IEEE `0x3D4CCCCD` is the usual `0.05f` encoding
(`1.6 × 2^-5`). Not `1e-6`, not `1e-4`.

Related rdata used by the same kernel (not the threshold):

| VA | Float | Bits | Use |
|---|---|---|---|
| `0x0122DED8` | `1` | `0x3F800000` | `1 - frac`, `1 - \|cosom\|` |
| `0x0122DEDC` | `0` | `0x00000000` | shortest-path `cosom < 0` |
| `0x0129CD1C` | `512.5` | `0x44002000` | acos table index |
| `0x01237F08` | `0.1591549` | `0x3E22F983` | `1/(2π)` scale |
| `0x01230010` | `1024` | `0x44800000` | sin table index |

Mixer weight gate (also **not** the slerp epsilon):

| VA | Float | Bits | Use |
|---|---|---|---|
| `0x0129BA3C` | `0.0001` | `0x38D1B717` | `00AA021C` `\|w-1\|` copy vs weighted |

**DISPROVEN:** treat `[0x125A15C]` as a tiny `1e-6` slerp
epsilon. Native small-angle is **five hundredths** of
`1-|dot|` (`|dot| >= 0.95`, about `18°`).

---

## 2. `00A4C1F0` uses it as `<=`

After shortest-path (`sign = ±1`, `|cosom|` in `[esp+76]`):

```
fld 1.0
fsub |cosom|
fcomp [0x125A15C]          ; 0.05
test ah, 0x41              ; C0 (ST<src) | C3 (ST==src)
jne 00A4C3FE               ; lerp
```

MSVC `fcomp` + `test ah, 0x41` + `jne` is `ST <= mem`.

| Condition | Arm | Dest |
|---|---|---|
| `1 - \|cosom\| <= 0.05` | `00A4C3FE` | `scale0=1-frac`, `scale1=frac*sign`, xyzw add, **no** `fsqrt` |
| `1 - \|cosom\| > 0.05` | `00A4C2D0` | table `acos`/`sin`, `fsqrt(1-cos²)`, same unnormalized add |

nlerp would `normalize` the lerp dest. This arm does not.
**DISPROVEN** as nlerp. **DISPROVEN** as
`Quaternion.Slerp` (BCL small-angle is on `|dot|` near 1
with a much tighter epsilon, then nlerp).

`00AA0090` copies the same `fcomp [0x125A15C]` at
`00AA033C` / `00AA066D` when blending two already-sampled
quats by channel weight. Same `0.05`. Still behind the
count-0 skip. Mixer leftover, not first-seen.

---

## 3. Count `> 0` would run the kernel. First-seen count is `0`.

`00AA0090` (`listing-00a80000.txt`):

```
00AA0142  eax = [sourceA+4]
          count = ([+16]-[+12]) / 20     ; 0x66666667 sar 3
00AA0160  test eax, eax
00AA0162  jbe 00AA097D                   ; FIRST-SEEN
```

`jbe` is unsigned `<=`. Count `0` (and the `je` null-list
path that leaves `eax=0`) jumps to `00AA097D` → optional
`00A9DFA0` → always `00A9E1E0`. No `00A4C1F0`.

If count were `> 0`, fall-through at `00AA0168` walks 20-byte
channels, `00A52650` → key+frac, then:

| Site | When |
|---|---|
| `00AA025E` | `[clip+8]` bit 2 set, `\|w-1\| < 0.0001`, bone `[track+10]` not signed |
| `00AA02F1` | same bit, `\|w-1\|` not small (weighted) |
| `00AA05A6` | bit 2 clear, `\|w-1\|` small |
| `00AA0601` | bit 2 clear, weighted |

So **yes**: first-seen channel count `> 0` would reach
`00A4C1F0` (then the `0.05` branch inside it). Extra gates
can still skip a bone (`js` on `[ecx+10]`, `test ebx,ebx`
bone count). The count test is the first gate.

First New Game PALSKIN does **not** have count `> 0`.
`anim-blend-first` / `xseq-00AA0090-interp` /
`00A4C1F0-xseq-slerp-kernel` already lock empty mixer.

**DISPROVEN:** first-seen no-save applies frac slerp.
**LEFTOVER:** live count `> 0` kernel, including this
`0.05` arm.

---

## 4. PALSKIN `00AA0090` skip **MATCH**

Packer (`listing-00bc0000.txt`):

```
00BD2E16  push 1
          push esi                  ; n = [mesh+152]
          push [helper+288]         ; dest
          push mesh
          push 00B83750
          push t
          push helper+124           ; source B
this = [[mesh+80]+4]+960            ; mixer
          push helper+116           ; source A
00BD2E35  call 00AA0090
```

First pack: mixer channels **0**. `00AA0162` `jbe 00AA097D`.
Dest = bind locals × IBM (`00A9E1E0`). Product ≈ **I**.

Host (`WorldShading.PaletteForPose`):

```
if sequence is null or no tracks → FirstSeenPalettes(bones)
else FirstSeenPalettes(sequence.ApplyLocals(bones, FloorKey(time)))
```

`FloorKey` keeps `TimeToKey.Key`, drops `.Frac`.
`RotationAt(key)` copies one stored quat. No slerp. No
nlerp. `XSeqFormatTests` pose samples at `time=0f` are a
**format** experiment (`frac=0` MATCH key 0). Submit does
not apply them. `FirstSeenPlaysAnim=false`.
`FirstSeenXseqAppliesFrac=false`.

| Claim | Class |
|---|---|
| PALSKIN first evaluate is `00AA0090` | **PROVEN** `00BD2E35` |
| First-seen channel count 0 | **PROVEN** `jbe 00AA097D` |
| Skip never calls `00A4C1F0` | **PROVEN** |
| Host floor-key / bind palettes | **MATCH** that skip |
| Count `> 0` would call the kernel | **PROVEN** listing. Unused first-seen |
| `[0x125A15C] = 0.05` | **PROVEN** dump |
| Ship `Quaternion.Slerp` on `PaletteForPose` | **DISPROVEN** as first-seen. Keep out |

---

## Classifications (short)

1. **`[0x125A15C] = 0.05` (`0x3D4CCCCD`) — PROVEN.**
   `1 - |cosom| <= 0.05` → unnormalized lerp. Else slerp.
2. **First-seen PALSKIN `00AA0090` skip — MATCH.**
   Count 0 → `00AA097D` → `00A9E1E0`. No kernel.
3. **Count `> 0` would run `00A4C1F0` — PROVEN** as live
   leftover, **DISPROVEN** as first-seen New Game.
4. **Keep `PaletteForPose` floor-key.** Do not ship
   `Quaternion.Slerp` or nlerp. Native `0.05` + unnormalized
   dest is not the BCL helper.

`NativeXseqRotationIsSlerp=true` remains the **native**
live-kernel flag, not a license to slerp dest now.
