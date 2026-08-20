# `00A4C1F0` XSEQ rotation is slerp, not nlerp

Investigation only. No production `src/` or `tests/` edits.

Native live clip rotation is **slerp** `00A4C1F0` plus a
small-angle **unnormalized** lerp. It is **not** nlerp.
Host `PaletteForPose` still **floors** the key.

Question: does first-seen no-save apply frac slerp, or
`FloorKey`? If `FloorKey` MATCH, keep host. If first-seen
applies `00A4C1F0`, document exact args. Do **not** invent
nlerp. Do **not** ship `Quaternion.Slerp` without listing
proof of first-seen apply.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `listing-00a40000.txt` `00A4C1F0`–`00A4C5DB` /
`00A4DEA0` / `00A52650` / `00A0DB60`;
`listing-00a80000.txt` `00AA0090`–`00AA09E3`;
`listing-00bc0000.txt` packer `00BD2E16`–`00BD2E35`;
`iat.tsv` `_CIacos` / `_CIasin`;
`src/Fable.Formats/WorldShading.cs`;
`src/Fable.Formats/Anims/XSeqFile.cs`;
tests `XSeqFormatTests`;
`proofs/xseq-00AA0090-interp`;
`proofs/palskin-child-hero`;
`proofs/audit-xseq`;
`proofs/anim-blend-first`.

---

## Verdict

**First-seen no-save does not apply frac slerp. Host
`FloorKey` MATCH. Keep `PaletteForPose` as floor-key.**

`00AA0090` first PALSKIN pack (`00BD2E35`) sees channel
count **0** and `jbe 00AA097D`. That skip never calls
`00A4C1F0`. Dest is bind locals through `00A9E1E0`.
`FirstSeenPlaysAnim=false`. `FirstSeenXseqAppliesFrac=false`.

Native **live** rotation (channel count &gt; 0) **is** slerp
at `00A4C1F0`: shortest-path sign flip, table `acos` /
`sin`, `fsqrt` `sin(ω)`, plus a small-angle branch that
lerps quaternion components **without** normalize. That is
**DISPROVEN** as nlerp. Host does not run that kernel.

`WorldShading.NativeXseqRotationIsSlerp=true`.
`XseqSlerpFn=0x00A4C1F0`. `PaletteForPose` still
`FloorKey(time)` → `ApplyLocals` → `RotationAt(key)` and
drops `TimeToKey.Frac`. Do **not** ship
`Quaternion.Slerp` (or nlerp) on that helper: first-seen
never feeds `frac` into `00A4C1F0`, and the native kernel
is not `System.Numerics.Quaternion.Slerp`.

`00A4C1F0` args (leftover live path, **not** first-seen):

| Slot | Native | First-seen |
|---|---|---|
| `ecx` | 44-byte track | not called |
| arg0 | dest (`ret 12`) | — |
| arg1 | integer key | — |
| arg2 | frac in `[0,1)` | — |

---

## Evidence → Original → Host → Gap

| Evidence | Original | Host | Gap |
|---|---|---|---|
| `00AA0090` `test eax,eax` / `jbe 00AA097D` | channel count 0 skips sample | `FirstSeenPalettes` bind | **MATCH** first-seen. No `00A4C1F0` |
| packer `00BD2E35` | first evaluate is empty mixer | no mixer object | **MATCH** skip. Evaluate leftover |
| `00A52650` then `00A4C1F0` | live time→key+frac then slerp | `TimeToKey` **MATCH** listing; `Frac` discarded | **PARTIAL** live. First-seen unused |
| `00A4C1F0` `ret 12` dest, key, frac | slerp / small-angle lerp | `RotationAt(FloorKey)` | **LEFTOVER** kernel. Floor **MATCH** first-seen |
| `fld 1; fsub frac`; `scale1 *= sign` | shortest-path slerp | none | **PROVEN** native. Not nlerp |
| `1-\|cosom\| <= [0x125A15C]` | unnormalized lerp, no `fsqrt` on dest | none | **PROVEN** small-angle. **DISPROVEN** nlerp |
| else `fsqrt(1-cos²)` + tables `0x13CB530` / `0x13CD550` | `sin((1-t)ω)/sin(ω)` weights | none | **PROVEN** slerp |
| same palette index → `jmp 00A4C464` copy | no blend | `RotationAt` copy of one key | **MATCH** when frac unused / t=0 |
| `00A4DEA0` `time*[track+4] % [track+8]` then `00A4C1F0` | 44-byte wrapper | unused by `00AA0090` | **UNREAD** as PALSKIN path |
| `NativeXseqRotationIsSlerp` / `XseqSlerpFn` / `FirstSeenXseqAppliesFrac` | slerp kernel, first-seen no frac | `true` / `0x00A4C1F0` / `false` | **MATCH** flags |
| `PaletteForPose(..., time)` | floor key into 48-byte local | `FloorKey` → `ApplyLocals` | **MATCH** first-seen. Live frac **LEFTOVER** |
| `Quaternion.Slerp` / nlerp on dest | not this listing | not shipped | keep out until first-seen apply is listed |

---

## 1. First-seen does **not** call `00A4C1F0`

PALSKIN packer (`listing-00bc0000.txt`):

```
00BD2E16  push 1
          push esi                  ; n
          push [helper+288]         ; dest
          push mesh
          push 00B83750
          push t
          push helper+124           ; source B
this = [[mesh+80]+4]+960            ; mixer
          push helper+116           ; source A
00BD2E35  call 00AA0090
```

`00AA0090` (`listing-00a80000.txt`):

```
00AA0142  eax = [sourceA+4]
          count = ([+16]-[+12]) / 20     ; 0x66666667 sar 3
00AA0162  jbe 00AA097D                   ; FIRST-SEEN
…
00AA097D  optional 00A9DFA0
00AA09B7  call 00A9E1E0                  ; hierarchy × IBM
```

First New Game pack: count **0**. The channel walk that
pushes key/frac and `call 00A4C1F0` (`00AA025E` /
`00AA02F1` / `00AA05A6` / `00AA0601`) does **not** run.
`anim-blend-first` / `xseq-00AA0090-interp` already lock
this. `FirstSeenPlaysAnim=false`.

**DISPROVEN:** first-seen no-save applies frac slerp.
**MATCH:** host floor-key / bind palettes. Keep host.

`XSeqFormatTests` pose samples at `time=0f` are a **format**
experiment (`3420` vs kid `4300`). Submit does not apply
them. `FirstSeenPlayAnimationAppliesPose=false`.
`TrianglesForPose(sequence)` still passes `time=0f`.

---

## 2. Exact args when the kernel **does** run

`00A4C1F0` is thiscall + `ret 12` (three stdcall dwords).
`ecx` = 44-byte track (`XSeqFile.ClipRecordBytes`,
`00A4DFF8` `add edx, 44`).

Entry (`listing-00a40000.txt`):

```
00A4C1F0  sub esp, 52
00A4C1F3  movzx edx, [ecx+8]          ; period = low 8 of +8
00A4C1F7  fld [0x122DED8]             ; 1.0
00A4C1FD  fsub [esp+64]               ; 1 - arg2 frac
00A4C202  mov ebx, [esp+64]           ; after one push: arg1 key
```

After `sub esp,52`: `[esp+56]=dest`, `[esp+60]=key`,
`[esp+64]=frac`. After `push ebx`: key lands at
`[esp+64]`. Later four pushes make dest `[esp+72]`.

Live caller `00AA0090` (`[clip+8]` bit 2 set,
`|w-1|` small → copy arm):

```
00AA01DD  call 00A52650               ; clip+80/+84 → key, frac
…
00AA0244  ecx = [clip+92][bone*8]     ; 44-byte track
          test [ecx+10], sign         ; js skip bone
00AA0254  push ebp                    ; frac
          push ebx                    ; key
          lea edx, [esp+308]
          push edx                    ; dest scratch
00AA025E  call 00A4C1F0
00AA026B  call 00A88C10               ; copy 16 bytes into local+0
          fadd T extras into local+16
```

`ecx` is still the track: last load is
`mov ecx, [eax+edi*8]` before the pushes.

Sibling wrapper `00A4DEA0` (`ret 8`, **not** the PALSKIN
caller):

```
fld time
fmul [ecx+4]                          ; track time scale
fistp key (0.5 bias, inc if exact int+1)
frac = scaled - key
movzx esi, [ecx+8]
idiv esi                              ; key %= period
push frac
push edx                              ; key
push dest
call 00A4C1F0
```

`00AA0090` uses the `00A26C60` clip wrapper `00A52650`
(`time * [clip+80] % [clip+84]`), not `00A4DEA0`.

Host `TimeToKey` **MATCH**es `00A52650`. `FloorKey` keeps
`.Key` and drops `.Frac`. `XSeqFormatTests` locks `(0,0)`
at t=0 rate 15 wrap 8.

Period inside `00A4C1F0` is `movzx [ecx+8]` (low byte).
Host `FloorKey` uses `Tracks[0].FrameCount & 0xFF`.
**MATCH** width. Clip `+84` vs that byte is still
**PARTIAL**.

---

## 3. Kernel: slerp + small-angle unnormalized lerp

Period wrap of the **next** key:

```
lea eax, [ebx+1]
cmp eax, edx                          ; key+1 ? period
sbb edi, edi
and edi, eax                          ; next = key+1 < period ? key+1 : 0
```

### 3a. Rotation mode `[ecx+11]`

```
00A4C21F  test dl, 0x02
          je 00A4C455                 ; no animated rot
          and dl, 0x03
          cmp dl, 0x03                ; palette
```

| `[+11] & 3` | Path |
|---|---|
| bit 1 clear, `== 1` | copy 16 bytes from `[ecx+20][0]` |
| bit 1 clear, else | identity `(0,0,0,1)` at dest |
| bit 1 set, `== 3` | pal `[ecx+36]` 1-byte index; same A/B index → copy, no blend (`jmp 00A4C464`) |
| bit 1 set, else / different pal | two `f32[4]` at `[ecx+20] + key*16` (`shl 4`) |

Palette width 1-byte is **PROVEN** (`movzx eax, [edx+ebx*1]`).
2-byte-when-`rotCount>255` is **DISPROVEN** (`audit-xseq`).

### 3b. Shortest path

```
cosom = A.w*B.w + A.x*B.x + A.z*B.z + A.y*B.y
sign  = +1                                ; 0x3F800000
fcomp [0x122DEDC]                         ; 0
test ah, 0x05 / jp                        ; cosom < 0
  sign = -1                               ; 0xBF800000
  fchs cosom
```

`scale1` is later `fmul [esp+28]` by this sign. `B` is
negated by the weight, not by rewriting the stored key.

### 3c. Small-angle branch — **not** nlerp

```
fld 1.0
fsub |cosom|                              ; 1 - |dot|
fcomp [0x125A15C]
test ah, 0x41                             ; <=
jne 00A4C3FE                              ; lerp
```

Lerp arm (`00A4C3FE`):

```
scale0 = 1 - frac                         ; [esp+16]
scale1 = frac * sign
dest   = scale0 * A + scale1 * B          ; xyzw, no fsqrt
```

No normalize of dest. **nlerp** would be
`normalize((1-t)A + t B)`. This branch is unnormalized
component lerp. **DISPROVEN** as nlerp.

`[0x125A15C]` is the compare VA. Decimal bits stay a
dump (`floats 0x125A15C`); the test is **PROVEN** as
`1-|cosom| <= threshold`.

### 3d. Slerp arm

```
omega  = lerp(acos_table[i], acos_table[i+1]) * [0x1237F08]
         after (1+|cosom|) * [0x129CD1C] → i
sinω   = sqrt(1 - cosom²)                 ; fsqrt
inv    = 1 / sinω
scale0 = sin_table((1-frac) * omega) * inv
scale1 = sin_table(frac * omega) * inv * sign
dest   = scale0 * A + scale1 * B          ; no normalize
```

Sin lookup: `* [0x1230010]=1024` (`FrontendLayout.GlobalWidthFloor`),
`fistp`, `add eax, 0xFFFFFF00`, `and eax, 0x3FF`, lerp
`[0x13CD550+eax*4]` / `+4`. Subtract 256 is a quarter-turn
on a 1024-entry cosine table (`fcos` fill) so the lookup
is **sin**.

Tables are **not** rdata immediates. `00A0DB60` (engine
init `009A5480` / `009A69A8`) fills them:

```
; 0x13CD550 : 1024 entries, ecx += 2, fcos, copy [0] to +0x1000
; 0x13CB530 : esi = 0..0x400
fild esi
fmul [0x129CA3C]
fsub [0x122DED8]                          ; x = i*scale - 1
call 00BFEBA2                             ; IAT _CIacos
fstp [0x13CB530+esi*4]
; sibling 0x13CC540 via 00BFEDC4 _CIasin
```

`iat.tsv`: `0x014400F0` `MSVCR71.dll!_CIacos`,
`0x01440064` `MSVCR71.dll!_CIasin`.

That is spherical lerp with table `acos`/`sin` and an
`fsqrt` `sin(ω)`. **PROVEN** slerp. **DISPROVEN** nlerp.

At `frac=0`: `scale0=1`, `scale1=0` on **both** arms
(lerp and slerp). Dest is key 0. Format tests at
`time=0f` therefore cannot prove the blend. They
**MATCH** floor key 0.

### 3e. Translation (not rotation)

```
shr [ecx+11], 2
test dl, 0x02                             ; original bit 3
```

Animated pos: `i16[3]` at `[ecx+32]`, indices `*3`,
`fmul [ecx+12]`, lerp by `frac` / `1-frac`, store
dest `+16/+20/+24`, `+28=0`. Static `==1`: first triple
only. Else zeros.

Host `ApplyLocals` keeps `FirstTranslation`. Later keys
**UNREAD**. Not this rotation question.

---

## 4. Host stays `FloorKey`

`WorldShading`:

```
NativeXseqRotationIsSlerp = true
FirstSeenXseqAppliesFrac  = false
XseqSlerpFn               = 0x00A4C1F0
```

`XSeqFormatTests.Xseq_persist_addrs_match_00a999b0_and_00aa4680`
locks all three.

`PaletteForPose`:

```
if sequence null or no tracks → FirstSeenPalettes(bones)
else FirstSeenPalettes(sequence.ApplyLocals(bones, FloorKey(time)))
```

`FloorKey`:

```
period = Tracks[0].FrameCount & 0xFF
TimeToKey(time, SamplesPerSecond, period).Key   // Frac dropped
```

`ApplyLocals` / `TrySample` / `RotationAt(key)` index
`RotationKeys` with wrap by count. No slerp. No nlerp.

`palskin-child-hero`: leftover “drops `time`” /
`ApplyFirstLocals` is **stale** vs this `FloorKey` code.
Floor-key **MATCH** first-seen. Mixing in
`Quaternion.Slerp(q0, q1, frac)` would invent a live
product first-seen never runs, and would still not be
the table kernel.

Do **not** ship that helper change. Port `00A4C1F0` only
when a listed first-seen (or later clip) path actually
calls it with the args in §2.

---

## Classifications (short)

1. **`00A4C1F0` is slerp + small-angle unnormalized lerp —
   PROVEN.** Shortest-path sign, table `acos`/`sin`,
   `fsqrt` `sin(ω)`. **DISPROVEN** as nlerp.
2. **Args — PROVEN:** `ecx` 44-byte track; dest, key,
   frac; `ret 12`. PALSKIN feeds `00A52650` key/frac and
   `[clip+92][bone]` track. Wrapper `00A4DEA0` is a
   different caller.
3. **First-seen no-save apply — DISPROVEN** as frac slerp.
   `00AA0090` count 0 → `00AA097D` → `00A9E1E0`.
   **MATCH** host `FloorKey` / bind palettes.
   `FirstSeenXseqAppliesFrac=false`.
4. **Keep host `PaletteForPose` floor-key.**
   `NativeXseqRotationIsSlerp=true` is the **native**
   live kernel flag, not a license to slerp dest now.
5. **Do not ship `Quaternion.Slerp` or nlerp** until a
   listing shows first-seen (or the chosen product)
   actually calling `00A4C1F0` with those args.

Do not treat `xseq-00AA0090-interp` “do not slerp” as
“native is not slerp.” Native **is** slerp. Host must
not grow one on the first-seen floor-key path.
