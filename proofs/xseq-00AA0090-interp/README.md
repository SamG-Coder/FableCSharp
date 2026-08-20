# Recover `00AA0090` interpolation

Investigation only. No production `src/` or `tests/` edits.

Do **not** slerp. Do **not** invent `Duration=1`.
`FirstSeenPlayAnimationAppliesPose=false`. Type-6 XSEQ
first-key sample drives `PaletteForPose` 48-byte locals
(`00A999B0` / `00AA4680` / `00A4C5E0`).

Question: recover `00AA0090` interpolation. Inputs, outputs,
time units. Host gap vs first-key sample only.
`Appearance_DEFAULT` Duration expected 1 vs actual
`clip.Duration/mode 6` is related **UNREAD** — classify, do
not invent `Duration=1`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `listing-00a80000.txt` `00AA0090`–`00AA09E3`;
`listing-00a40000.txt` `00A52650` / `00A4C1F0` / `00A4C5E0` /
`00A4DEA0`; `listing-00bc0000.txt` packer `00BD2E16`–`00BD2E35`;
`listing-00700000.txt` `0070D580`; `src/Fable.Formats/Anims/XSeqFile.cs`;
`src/Fable.Formats/WorldShading.cs`;
`src/Fable.Game/Scripting/ExecutionContext.cs`;
`proofs/audit-xseq`; `proofs/anim-blend-first`;
`proofs/palskin-child-hero`; `proofs/xseq-walk-first`;
tests `XSeqFormatTests`,
`ScriptRuntimeArchitectureTests.Appearance_DEFAULT_starts_0070D580_inner_play`.

---

## Verdict

**`00AA0090` time interpolation is recovered as a mixer
evaluate: lerp two 20-byte channel times, `00A52650`
time→key+frac, sample the 44-byte track, accumulate 48-byte
locals, then always `00A9E1E0`.** First-seen channel count is
**0**, so that lerp does not run on New Game.

Host product is **first-key only**. `PaletteForPose` feeds
`FloorKey(time)` into `ApplyLocals` (first stored quat +
first i16×factor pos). That **MATCH**es type-6 first-key
48-byte locals. It is **not** the mixer walk.

Do **not** slerp. Native two-key blend inside `00A4C1F0`
stays **LEFTOVER**. Host must not grow a slerp.

`Appearance_DEFAULT` `state.Duration==1` is **not** native.
Native is `[clip+44]/max(mode,1)` with mode **6**. Host
`clip.Duration/6` **MATCH**es the formula; the `1f` clip
fallback is **INVENTED**. Relation of that timer to
`00AA0090` channel `+12` is **UNREAD**.

---

## Evidence → Original → Host → Gap

| Evidence | Original | Host | Gap |
|---|---|---|---|
| `00AA0090` `ret 32`; packer `00BD2E35` | `this` = mixer `bank+960`; 8 args: source A `helper+116`, source B `helper+124`, blend `t`, `00B83750` cache, mesh, dest, bone count, flag `1` | `WorldShading.BoneHierarchyBuild` constant only; no mixer object | **DIVERGE** evaluate. Addr **PROVEN** |
| `ebx=[mesh+152]`; `lea ecx,[ebx+ebx*4]; mul 0x30`; `00A5C910(mixer+20)` | scratch `n*48` locals | none | **UNREAD** mixer scratch |
| `00A9F2F0` on A/B headers | header time lerp `[+12]` by packer `t`; leaf `vtbl+8==0` | none | **UNREAD** |
| `([list+16]-[list+12])*0x66666667 sar 3` | channel count = bytes/20 | none | **PROVEN** 20-byte stride |
| first-seen count **0** → `jbe 00AA097D` | skip channel walk | `FirstSeenPalettes` bind locals | **MATCH** first-seen. `FirstSeenPlaysAnim=false` |
| channel `+8` → `00A242C0` | `[bank+896][i]` clip slot | none | **UNREAD** |
| `00A26C60` then `fld [A+12]/[B+12]`; `fsub`; `fmul t`; `fadd` | `time = A.t + (B.t-A.t)*t` | `PaletteForPose(..., time)` → `FloorKey` only | **PARTIAL** scalar `time`. No A/B lerp |
| `call 00A52650` | `time*[clip+80]` → key+frac; `key %= [clip+84]` | `TimeToKey(time, rate, wrap)` | **MATCH** listing. Rate/wrap map **PARTIAL** |
| channel `+16` weight lerp by `t` | `w = A.w + (B.w-A.w)*t` | none | **UNREAD** |
| `[clip+8]` `shr 2; test cl,1` | exact-key vs weighted accumulate | none | **UNREAD** |
| `00A4C1F0` two 16-byte keys `shl 4` | native key sample (frac path leftover) | `RotationAt(FloorKey)` / `FirstTranslation` | **PARTIAL** first key. **Do not slerp** |
| `00A88C10` 16 bytes into local `+0`; `fadd` T extras | 48-byte local TRS | `ApplyLocals` quat+first pos | **PARTIAL** first stored local |
| tail always `00A9E1E0` | hierarchy × IBM into dest `n*64` | `FirstSeenPalettes` | **MATCH** when 0 channels / first-key locals already applied |
| type-6 persist `00A999B0` / `00AA4680` / `00A4C5E0` | 44-byte track, 48-byte uncompressed local | `XSeqFile.Parse` + first-key into `PaletteForPose` | **MATCH** format first-key. Runtime **LEFTOVER** |
| `0070D580` `fdivr [ecx+44]` / mode | playback duration `[channel+56]=[clip+44]/max(mode,1)`; step `1/mode` at `[esi+140]` | `ApplyInner`: `Duration=clip.Duration/playMode`; `Step=1/playMode` | **MATCH** formula. Clip `+44` vs ANRT **UNREAD** |
| `Appearance_DEFAULT` mode 6 | `005B37F7` / `0070C050` → `0070D580` | `PlayAppearanceDefault(..., 6)` | **PROVEN** mode. **DISPROVEN** as first-seen (`FirstSeenAppearancePlaysDefault=false`) |
| test `Assert.Equal(1f, state.Duration)` | not a native store | miss clip `AnimationClipRecord(..., 1f)` then `/6` | **UNREAD** related. Do **not** invent `Duration=1` |
| `FirstSeenPlayAnimationAppliesPose` | first Present dest = bind | `false` | **MATCH** |

---

## 1. Inputs (`00AA0090`)

`this = ecx` = 76-byte mixer (`00AA0F60`, vtbl `0129E134`) at
`MBANK_ALLMESHES+960`. `ret 32` = eight dwords.

PALSKIN packer `00BD2D90` when `[helper+288]==0`
(`listing-00bc0000.txt`):

```
00BD2DB2  esi = [mesh+152]          // bone count
00BD2DE4  alloc n*64 → [helper+288]
00BD2DFD  fld [0x143B934 + [helper+44]*12]
00BD2E16  push 1                    // arg7 flag
          push esi                  // arg6 n
          push dest                 // arg5 [helper+288]
          push mesh                 // arg4 [helper+228]
          push 00B83750             // arg3 cache (0 first-seen)
          push t                    // arg2 packer blend
          push helper+124           // arg1 source B
this = [[mesh+80]+4]+960            // mixer
          push helper+116           // arg0 source A
00BD2E35  call 00AA0090
```

| Arg | Native | First-seen | Class |
|---|---|---|---|
| `this` | mixer | empty ctor, zero channels | **PROVEN** |
| 0 / 1 | blend sources A/B (`+4` list, `+12` time, `+16` weight) | headers present; list length 0 | **PROVEN** layout |
| 2 | unitless blend `t` from `0x143B934` | unused when count 0 | **PROVEN** as scalar. Live value **UNREAD** |
| 3 | extra pose cache | `eax=0` | **PROVEN** skip (`je 00AA0996`) |
| 4 | mesh; `+152` = n | C3D | **PROVEN** |
| 5 | dest `n*64` | bind product ≈ I | **PROVEN** |
| 6 | n (also read from mesh) | 4299/4300 bone count | **PROVEN** |
| 7 | flag into `00A9E1E0` | `1` | **PROVEN** packer |

Host has **no** mixer, **no** 20-byte channels, **no** packer
`t`. Gap is the evaluate, not the first-key format.

---

## 2. Time units

Three clocks. Do not collapse them.

### 2a. Mixer channel time — `00AA0090` + `00A52650`

```
00AA01B8  fld [esi+12]              // A.time
00AA01BB  fld [edi+12]              // B.time
          fsub st, st(1)            // B-A
          fmul packer_t
          fadd st, st(1)            // A + (B-A)*t
00AA01DD  call 00A52650             // ecx = 00A26C60 clip
```

`00A52650` (`listing-00a40000.txt`, 56 bytes):

```
fld time
fmul [ecx+80]                       // ClipRateOffset = 80
fistp  (x87 floor with 0.5 bias; inc if exactly integer+1)
frac = scaled - key
div [ecx+84]                        // ClipWrapOffset = 84
key = remainder; if <0 add wrap
ret 12  → (key*, frac*)
```

Host `WorldShading.TimeToKey` **MATCH**es that listing.
`XSeqFormatTests` locks `(0,0)` at t=0 rate 15 wrap 8; mid
key 1 at t=0.1 rate 15 wrap 30.

`[clip+80]` / `[clip+84]` units stay **PARTIAL**. Host
`FloorKey` maps `Tracks[0].SamplesPerSecond` and
`FrameCount & 0xFF`. `WorldShading` comment: does **not** map
XSEQ fps onto `[clip+80]`.

Sibling `00A4DEA0` does `time * [track+4]` then
`idiv [track+8]` (u16) and **calls** `00A4C1F0`. That is the
44-byte record scale (`audit-xseq` `+4` time scale).
`00AA0090` does **not** call `00A4DEA0`. It uses the
`00A26C60` clip wrapper `+80/+84`.

### 2b. Playback timer — `0070D580` `[clip+44]/mode`

```
0070D5D3  cmp mode, 1
          jl → mode = 1
0070D5E3  fild mode
          fdivr [ecx+44]            // duration / mode
          fstp [ecx+56]
0070D67F  fild mode
          fdivr [0x122DED8]         // 1.0 / mode
          fstp [esi+140]            // step
```

This is the request-object duration / step, **not** mixer
`channel+12`. `xseq-walk-first`: “channel duration, not the
key lerp.”

Host `ApplyInner`:

```
playMode = mode <= 0 ? 1 : mode
Duration = clip.Duration / playMode
Step     = 1f / playMode
PlayTime = 0
```

Formula **MATCH**. What `clip.Duration` *is* (ANRT `+48` vs
request `+44`) is **UNREAD**.

### 2c. ANRT persist duration — format, not mixer

`00A98AF0`: payload `[0]` cyclic → obj+44; payload `[1]` f32
→ obj+48. Host `XSeqFile.Duration`. Fallback
`duration > 0f ? duration : 1f` is **INVENTED** (`audit-xseq`
§3; native keeps 0). Unused at sample time.

---

## 3. Outputs

`00AA0090` writes `n*48` locals then `00A9E1E0` dest.

48-byte local (`BoneLocalBytes`, `00A4F0D8` /
`00AAF1E0`):

| Off | Native fill in the channel loop | Host first-key |
|---|---|---|
| `+0..+15` | `00A88C10` copies 16-byte sample (quat path when `[track+11] & 2`) | `LocalRotation = RotationAt(key)` |
| `+16..+24` | `fadd` of `00A4C1F0` sample `+16/+20/+24` (i16×`[track+12]`) | `LocalTranslation = FirstTranslation` only |
| `+28` | extra `fadd` sample `+28` | **UNREAD** (host scale stays mesh) |

Stride `add esi, 48` **PROVEN** (`00AA02AE` / `00AA0543` /
`00AA05B4`). `[track+10]` `test al,al; js` skips a bone.

Tail:

```
00AA097D  optional 00A9DFA0 if arg3 != 0
00AA09B7  call 00A9E1E0             // always
00AA09C8  optional 00A9D750
00AA09D4  00A5C720 free mixer+20
```

`00A9E1E0` parent-walks the 48-byte locals with the 60-byte
C3D bone (`+4` parent) into 64-byte worlds. Packer then
`00BD2F91` / SSE `dest = S * C3D` IBM. First-seen no clip:
product ≈ identity.

Host `PaletteForPose(bones, clip, time, sequence)`:

```
if sequence null or no tracks → FirstSeenPalettes(bones)
else FirstSeenPalettes(sequence.ApplyLocals(bones, FloorKey(time)))
```

That **is** “first stored local replaces 48-byte mesh TRS;
hierarchy + IBM stay `FirstSeenPalettes`.” Comment leftover
“samples the first stored key” is **stale** vs `FloorKey`,
but at `time=0` they coincide.

---

## 4. Channel walk — first-seen skip **PROVEN**

```
00AA0142  eax = [sourceA+4]
          count = ([+16]-[+12]) / 20
00AA0162  jbe 00AA097D              // FIRST-SEEN
```

Per channel when count > 0 (`add ecx, 20` / `dec` at
`00AA095C`):

1. index `+8` → `00A242C0` → `00A26C60` bind to mesh.
2. lerp times `+12`, `00A52650`.
3. lerp weights `+16`.
4. `[clip+8]` bit 2 (`shr 2; test cl,1`):
   - set + `|w-1|` small (`fsub 1.0; fabs; fcomp [0x129BA3C]`):
     `00A4C1F0` then `00A88C10` + T `fadd` (**copy** sampled local).
   - set + weight not ~1: weighted accumulate into the 48-byte
     scratch (not recovered as host math; **UNREAD**).
   - clear + `[clip+8]` bit 1: second copy/accumulate using
     `[clip+104]` per-bone mask.

First New Game pack: count **0**, so none of this runs
(`anim-blend-first` §2a; `FirstSeenPlaysAnim=false`).
Interp is leftover until a clip arms channels.

---

## 5. `00A4C1F0` key sample — do **not** slerp

`ecx` = 44-byte track (`ClipRecordBytes`). `ret 12`: dest,
key, frac.

```
00A4C1F3  period = movzx [ecx+8]
          next  = key+1 < period ? key+1 : 0
          fld 1.0; fsub frac                         // 1-frac
00A4C21F  test [ecx+11], 2                           // rot mode
```

Rot path (`+11 & 2`): two `f32[4]` at `[ecx+20] + key*16`.
Palette (`+11 & 3 == 3`) indexes `[ecx+36]` as **1-byte**
(`audit-xseq`; 2-byte-when-`rotCount>255` **DISPROVEN**).
Same palette index → copy 16 bytes, no blend.

Pos path (`+11` bits 2–3): `i16[3]` at `[ecx+32] * [ecx+12]`
(`00A4C4BA` `fmul [ecx+12]`). First triple **PROVEN**
decode. Host `ApplyLocals` keeps `FirstTranslation` —
later keys **UNREAD**.

The two-key quat blend (dot, sign flip vs `0x122DEDC`,
`fsqrt`, tables `0x13CB530` / `0x13CD550`) is native
**LEFTOVER**. Do **not** port it. Host `RotationAt(key)` is
the stored key with wrap by count — first-key at t=0
**MATCH**es `frac=0` (result is key 0). Growing a slerp
would invent a later product.

`00AA0090` at `|w-1|` small already takes the copy arm
(`00AA0227 jp` → weighted). First-key sample does not need
that arm.

---

## 6. Type-6 first-key → `PaletteForPose` **MATCH** format

Persist / unpack VAs (`XSeqFile` constants, tests lock):

| Const | VA | Role | Class |
|---|---|---|---|
| `Ctor3Daf` | `00A999B0` | `"3DAF"` vtbl `0129E060` | **PROVEN** |
| `CtorXseq` | `00AA4680` | `"XSEQ"` vtbl `0129E194` size 28 | **PROVEN** |
| `UnpackFn` | `00A4C5E0` | stream → 44-byte track | **PROVEN** |
| `HierarchyFn` | `00AA0090` | this mixer evaluate | **PROVEN** addr. Interp leftover |
| `BoneLocalBytes` | 48 | uncompressed local | **PROVEN** |

`XSeqFormatTests`: synthetic + wake `3420` first keys move
kid **4300** palettes/tris off bind. That is a **format**
experiment. Engine submit does not apply it.
`FirstSeenPlayAnimationAppliesPose=false`.
`TrianglesForPose(sequence)` still passes `time=0f`.

Keep first-key as the host pose sample. Do not wire mixer
channels into submit.

---

## 7. `Appearance_DEFAULT` Duration — related **UNREAD**

`PlayAppearanceDefault` → `BeginInnerPlay("DEFAULT", 6)` →
`ApplyInner(..., mode=6)`.

Native mode **6** is **PROVEN** (`005B37F7` / `0070C050` /
`0070B4D0`; clothing GUI / `PC_UI_FRAME` only). Create
`006AC910` does **not** play (`FirstSeenAppearancePlaysDefault=false`).

| Claim | Class |
|---|---|
| DEFAULT request mode is 6 | **PROVEN** |
| `[clip+44]/max(mode,1)` → `[channel+56]`; step `1/mode` | **PROVEN** `0070D580` |
| Host `Duration = clip.Duration / playMode` | **MATCH** that formula |
| Missing clip `AnimationClipRecord(name, 1f)` | **INVENTED** fallback (same family as ANRT `Duration=1`) |
| `Appearance_DEFAULT_starts_0070D580_inner_play` expects `state.Duration==1` | test wants the invented **clip** 1, not `/6`. **UNREAD** related. Do **not** invent `Duration=1` as native state duration |
| Actual host state duration with miss clip | `1f/6` | do not “fix” the test in this proof |
| ANRT `+48` **is** `[clip+44]` | **UNREAD** |
| `0070D580` duration **is** `00AA0090` `channel+12` | **DISPROVEN** (different objects). How PlayTime becomes mixer `+12` **UNREAD** |

`LookupClip` miss returns `new AnimationClipRecord("DEFAULT", 1f)`.
Named clip with a real `XSeqFile.Duration` uses that persist
float (itself possibly the invented `1f` if ANRT stored 0).
Neither is a license to hardcode state Duration=1.

`Tick` advances `PlayTime += Step`. Mixer `channel+12` is
**not** that PlayTime until a later unread bind.

---

## 8. First-seen flag stays false

`RegionTravel.FirstSeenPlayAnimationAppliesPose = false`.
`WorldShading.FirstSeenPlaysAnim = false`.
`XSeqFormatTests` asserts the flag after 3420 first-key
**moves** dest — format only. First Present dest remains
bind locals (0-channel `00AA0090` + `00A9E1E0`).

Do not apply `PaletteForPose(clip)` on New Game.

---

## Classifications (short)

1. **`00AA0090` inputs / 20-byte channels / `n*48` locals /
   tail `00A9E1E0` — PROVEN** listing. First-seen count 0
   **PROVEN**.
2. **Time lerp `A.t+(B.t-A.t)*t` then `00A52650`
   `time*[clip+80] % [clip+84]` — PROVEN** as the interp.
   Units of `+80/+84` **PARTIAL**.
3. **Host gap = first-key sample only.** `FloorKey` +
   `ApplyLocals` / `PaletteForPose` **MATCH** type-6 first
   stored 48-byte local (`00A999B0` / `00AA4680` /
   `00A4C5E0`). Mixer walk / weight / two-key sample
   **UNREAD** / **LEFTOVER**.
4. **Do not slerp.** `00A4C1F0` two-key quat blend stays
   leftover. Host `RotationAt` is the stored key.
5. **`Appearance_DEFAULT` Duration expected 1 vs
   `clip.Duration/mode 6` — UNREAD related.** Native is
   `[clip+44]/6`. Host `/playMode` **MATCH**es. `1f` is an
   invented miss-clip / ANRT fallback. Do not invent
   `Duration=1`.
6. **`FirstSeenPlayAnimationAppliesPose=false` — MATCH.**

Do not treat `TimeToKey` + first-key as a finished mixer.
Do not treat `state.Duration=1` as `00AA0090` time.
