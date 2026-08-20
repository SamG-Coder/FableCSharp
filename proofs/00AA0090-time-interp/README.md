# Recover `00AA0090` time interpolation (frac + first-seen skip)

Investigation only. No production `src/` or `tests/` edits.

PlayAnimation apply is **PROVEN**; runtime is still **PARTIAL**.
`FirstSeenPlayAnimationAppliesPose=false`. Type-6 XSEQ
first-key sample drives `PaletteForPose` 48-byte locals
(`00A999B0` / `00AA4680` / `00A4C5E0`).

Do **not** slerp. Do **not** invent `Duration=1`.
Do **not** apply interp on first Present without evidence.

Question: recover the `00AA0090` body — frac formula, and
whether first-seen skips the walk. If the skip is listed,
first Present dest stays bind / first-key. If the walk
runs, document exact time→key+frac.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `listing-00a80000.txt` `00AA0090`–`00AA09E3`;
`listing-00a40000.txt` `00A52650` / `00A4DEA0`;
`listing-00bc0000.txt` packer `00BD2E16`–`00BD2E35`;
`src/Fable.Formats/Anims/XSeqFile.cs`;
`src/Fable.Formats/WorldShading.cs`;
`src/Fable.Game/RegionTravel.cs`
(`FirstSeenPlayAnimationAppliesPose`);
`src/Fable.Game/Scripting/ExecutionContext.cs` (`ApplyInner`);
tests `XSeqFormatTests`;
siblings `proofs/xseq-00AA0090-interp`,
`proofs/00A4C1F0-xseq-slerp-kernel`,
`proofs/anim-blend-first`,
`proofs/xseq-walk-first`,
`proofs/xseq-first`.

---

## Verdict

**Frac formula is recovered. First-seen skip is PROVEN.
Do not apply interp on first Present.**

`00AA0090` is the PALSKIN mixer evaluate (`ret 32`). When
source-A channel count is **0** it `jbe 00AA097D` and never
calls `00A52650` / `00A4C1F0`. First New Game pack
(`00BD2E35`) takes that skip. Dest is bind locals through
`00A9E1E0`. That is the first Present pose.

When count is `> 0` the walk lerps two 20-byte channel
times, then `00A52650`:

```
scaled = time * [clip+80]
key    = fistp(scaled - 0.5) ; inc if bits(scaled)==bits(key+1)
frac   = scaled - key
key    = key % [clip+84]     ; rem<0 → rem += wrap
```

Host `WorldShading.TimeToKey` **MATCH**es that listing.
`PaletteForPose` still `FloorKey(time)` and **drops**
`.Frac`. First-key 48-byte locals (`ApplyLocals` /
`RotationAt(0)` + `FirstTranslation`) **MATCH** type-6
sample at `time=0` / `frac=0`. They are **not** the mixer
walk.

PlayAnimation apply (`004C7470` / `0070D580` inner) stays
**PROVEN** (`ClipKey`, `PlayTime=0`, `InnerApplied`).
Runtime pose leftover is this mixer walk after a clip
**arms channels**. First Present does not arm them.
`FirstSeenPlayAnimationAppliesPose=false` **MATCH**.

---

## Evidence → Original → Host → Gap

| Evidence | Original | Host | Gap |
|---|---|---|---|
| `00AA0090` `ret 32`; packer `00BD2E35` | mixer `bank+960`; A/B headers, packer `t`, mesh, dest, n, flag `1` | `BoneHierarchyBuild` constant only | **PROVEN** addr. Evaluate leftover |
| `([list+16]-[list+12])*0x66666667 sar 3` | channel count = bytes/20 | none | **PROVEN** 20-byte stride |
| `test eax,eax` / `jbe 00AA097D` | count 0 skips time walk | `FirstSeenPalettes` bind | **MATCH** first-seen skip |
| first PALSKIN pack count **0** | `00A52650` not called | `FirstSeenPlaysAnim=false` | **PROVEN** skip. Interp **DISPROVEN** as first Present |
| `fld [A+12]/[B+12]`; `fsub`; `fmul t`; `fadd` | `time = A.t + (B.t-A.t)*t` | `PaletteForPose(..., time)` scalar only | **PROVEN** formula. No A/B lerp on host |
| `call 00A52650` | `time*[clip+80]` → key+frac; `key %= [clip+84]` | `TimeToKey` | **MATCH** listing. Rate/wrap units **PARTIAL** |
| `fistp` after `fsub 0.5`; inc if `scaled==(key+1)` | x87 floor with 0.5 bias | `Floor(scaled)` then same inc | **MATCH** tests (`t=0` → key 0, frac 0) |
| `frac = scaled - key` stored via `fstp [edx]` | frac in `[0,1)` | computed then **dropped** by `FloorKey` | **PROVEN** native. Host **MATCH** first-seen (unused) |
| `00AA0090` never `E8` `00A4DEA0` | not the 44-byte `time*[track+4]%[track+8]` wrapper | unused | **DISPROVEN** as this path |
| `00A4C1F0` after key+frac | live two-key sample | `RotationAt(FloorKey)` | **LEFTOVER** kernel. **Do not slerp** |
| tail always `00A9E1E0` | hierarchy × IBM | `FirstSeenPalettes` | **MATCH** when skip / first-key locals |
| type-6 persist `00A999B0` / `00AA4680` / `00A4C5E0` | 44-byte track → 48-byte local | `XSeqFile.Parse` + first-key into `PaletteForPose` | **MATCH** format first-key. Mixer **LEFTOVER** |
| PlayAnimation apply `004C7470` / `0070D580` | request + inner play; `PlayTime` starts 0 | `ApplyInner`: `ClipKey`, `PlayTime=0` | **PROVEN** apply. Mixer `channel+12` bind **UNREAD** |
| `FirstSeenPlayAnimationAppliesPose` | first Present dest = bind | `false` | **MATCH** |

---

## 1. Inputs (`00AA0090`)

`this = ecx` = 76-byte mixer (`00AA0F60`, vtbl `0129E134`)
at `MBANK_ALLMESHES+960`. `ret 32` = eight dwords.

PALSKIN packer `00BD2D90` when `[helper+288]==0`
(`listing-00bc0000.txt`):

```
00BD2DB2  esi = [mesh+152]                 // bone count n
00BD2DE4  alloc n*64 → [helper+288]
00BD2DFD  fld [0x143B934 + [helper+44]*12]
00BD2E16  push 1                           // arg7 flag
          push esi                         // arg6 n
          push dest                        // arg5 [helper+288]
          push mesh                        // arg4
          push 00B83750                    // arg3 cache (0 first-seen)
          push t                           // arg2 packer blend
          push helper+124                  // arg1 source B
this = [[mesh+80]+4]+960                   // mixer
          push helper+116                  // arg0 source A
00BD2E35  call 00AA0090
```

`00AA0090` (`listing-00a80000.txt`):

```
00AA0090  sub esp, 0x14C
00AA00A1  ebx = [mesh+152]                 // n
00AA00B7  lea ecx, [ebx+ebx*4]
00AA00E6  mul 0x30                         // n*48 locals
00AA0100  call 00A5C910                    // mixer+20 scratch
00AA013D  call 00A9F2F0                    // A/B header blend
```

First-seen: mixer ctor empty, list length 0, cache `eax=0`.
Arg2 `t` is unused because the channel loop never runs.

---

## 2. First-seen skip — **PROVEN**

```
00AA0142  eax = [sourceA+4]
00AA0145  test eax, eax
          je  00AA0160                     // null list → eax stays 0
00AA0149  ecx = [eax+16] - [eax+12]
00AA014F  imul 0x66666667
          sar edx, 3
          add eax, edx                     // signed bytes/20
00AA0160  test eax, eax
00AA0162  jbe 00AA097D                     // FIRST-SEEN
```

`0x66666667` + `sar 3` is signed divide by **20**. Channel
records are 20 bytes (`add ecx, 20` / `dec` at `00AA095C`).

Count **0** jumps to the tail:

```
00AA097D  optional 00A9DFA0 if arg3 != 0   // first-seen skip (je 00AA0996)
00AA09B7  call 00A9E1E0                    // always
00AA09C8  optional 00A9D750
00AA09D4  00A5C720 free mixer+20
00AA09E3  ret 32
```

**No** `00A52650`. **No** `00A4C1F0`. **No** frac.

`anim-blend-first` §2a / `xseq-first` §5 / packer
`00BD2E35`: first live drain is this empty mixer. Frontend
Present `0042DF9E` has **0** `E8` to `00AA0090`. Create
`006AC910` does not PlayAnimation
(`FirstSeenAppearancePlaysDefault=false`).

**DISPROVEN:** first Present applies time interp.
**MATCH:** host bind palettes. Keep
`FirstSeenPlayAnimationAppliesPose=false`.

Do **not** feed `PaletteForPose(clip)` into New Game submit
because a later Oakvale line names `CS_WAKING_UP_LOOP`.
`XSeqFormatTests` 3420 / synthetic first-key that **moves**
dest is a **format** experiment. Engine first Present does
not apply it.

---

## 3. Frac formula — **PROVEN** when count `> 0`

Per channel (`esi` = A record, `edi` = B record, same
`ecx` byte offset):

```
00AA0193  index = [esi+8] → 00A242C0 → 00A26C60   // clip wrapper
00AA01B8  fld [esi+12]                             // A.time
00AA01BB  fld [edi+12]                             // B.time
          fsub st, st(1)                           // B-A
00AA01CA  fmul [esp+packer_t]
          fadd st, st(1)                           // A + (B-A)*t
00AA01D8  fstp [esp]                               // arg0 time
00AA01DD  call 00A52650                            // ecx = clip
```

Mixer time:

```
time = A[+12] + (B[+12] - A[+12]) * packer_t
```

`00A52650` (`listing-00a40000.txt`, `ret 12`: time, `key*`,
`frac*`):

```
00A52656  fld  time                                // [ebp+8]
00A5265A  fmul [ecx+80]                            // ClipRateOffset = 80
00A5265D  mov  [ebp-8],  0x3F000000                // 0.5
00A52664  mov  [ebp-4],  0x3F800000                // 1.0
          fst  scaled; fstp bits
          fld  scaled; fsub 0.5; fistp key         // x87 RC
          fild key; fadd 1.0
          cmp  bits(scaled), bits(key+1.0)
          je   inc key
          fild key
          fsubr scaled                             // frac = scaled - key
          *keyOut  = key
          *fracOut = frac
          xor  edx, edx
          div  [ecx+84]                            // ClipWrapOffset = 84
          *keyOut  = rem
          if rem < 0: rem += wrap
          ret 12
```

Closed form:

```
scaled = time * [clip+80]
key    = (int)round_toward_rc(scaled - 0.5)
if bits(scaled) == bits((float)(key + 1)): key += 1
frac   = scaled - key
key    = key % [clip+84]          // rem<0 → rem += wrap
```

Host (`WorldShading.TimeToKey`):

```
scaled = time * rate
key    = (int)MathF.Floor(scaled)
if (scaled == key + 1f) key++
frac   = scaled - key
rem    = key % wrap; if rem < 0: rem += wrap
```

`XSeqFormatTests` locks `(0, 0)` at `t=0` rate 15 wrap 8;
mid key 1 at `t=0.1` rate 15 wrap 30 (frac in `[0.4, 0.6]`);
wrap `TimeToKey(2, 15, 15).Key == 0`.

`[clip+80]` / `[clip+84]` units stay **PARTIAL**. Host
`FloorKey` maps first-track `SamplesPerSecond` and
`FrameCount & 0xFF`. `WorldShading` comment: does **not**
map XSEQ fps onto `[clip+80]`.

Weight lerp is **after** the time call (`fld [esi+16]` /
`[edi+16]`, same `packer_t`). Not the frac.

---

## 4. Sibling clocks — do not collapse

### 4a. `00A4DEA0` is **not** this path

```
fld time
fmul [ecx+4]                       // 44-byte track scale
fistp (same 0.5 bias + inc)
frac = scaled - key
movzx esi, [ecx+8]
idiv esi                           // key %= period (u16)
call 00A4C1F0
```

`00AA0090` uses the `00A26C60` clip wrapper `+80/+84`,
**not** track `+4/+8`. Zero `E8` from `00AA0090` to
`00A4DEA0`.

### 4b. `0070D580` playback timer is **not** mixer `+12`

Apply inner: `[channel+56] = [clip+44] / max(arg1,1)`;
step `1/arg1` at `[esi+140]`. Host `ApplyInner` sets
`PlayTime=0`, `Duration=clip.Duration/playMode`,
`Step=1/playMode`. That timer is the request object.

How `PlayTime` becomes mixer channel `+12` is **UNREAD**.
`0070D580` duration **is not** `00AA0090` `+12`
(**DISPROVEN**, different objects).

### 4c. ANRT persist duration — format, not mixer

`XSeqFile.Duration` from ANRT payload `[1]`. Fallback
`duration > 0f ? duration : 1f` is **INVENTED**. Unused at
sample time.

---

## 5. After `00A52650` — leftover live walk only

`[clip+8] shr 2; test cl,1` picks copy vs weighted
accumulate. Copy arm (`|w-1|` small vs `[0x129BA3C]`):

```
00AA0254  push frac
          push key
          push dest scratch
00AA025E  call 00A4C1F0                // 44-byte track
00AA026B  call 00A88C10                // 16 bytes → local+0
          fadd T extras → local+16     // stride add esi, 48
```

That kernel is slerp + small-angle unnormalized lerp
(`proofs/00A4C1F0-xseq-slerp-kernel`). First-seen never
reaches it. Do **not** ship `Quaternion.Slerp`. Host
`RotationAt(FloorKey)` is the stored key.

`00AA0C50` / `00AA0FA0` are no-A/B-lerp siblings (single
`[edi+12]` time). First-seen PALSKIN `E8` is `00AA0090`
from `00BD2E35`, **not** those.

---

## 6. Type-6 first-key → `PaletteForPose` **MATCH** format

`XSeqFile` constants / `XSeqFormatTests`:

| Const | VA | Role | Class |
|---|---|---|---|
| `Ctor3Daf` | `00A999B0` | `"3DAF"` | **PROVEN** |
| `CtorXseq` | `00AA4680` | `"XSEQ"` vtbl `0129E194` | **PROVEN** |
| `UnpackFn` | `00A4C5E0` | stream → 44-byte track | **PROVEN** |
| `HierarchyFn` | `00AA0090` | this mixer evaluate | **PROVEN** addr. Interp leftover on live channels |
| `BoneLocalBytes` | 48 | uncompressed local | **PROVEN** |
| `TimeToKeyFn` | `00A52650` | time→key+frac | **MATCH** listing |

Host:

```
PaletteForPose(bones, clip, time, sequence)
  sequence null / no tracks → FirstSeenPalettes(bones)
  else FirstSeenPalettes(sequence.ApplyLocals(bones, FloorKey(time)))
```

`FloorKey` keeps `TimeToKey.Key` and drops `.Frac`.
`ApplyLocals` writes `RotationAt(key)` and
`FirstTranslation` into the 48-byte mesh TRS. At
`time=0` that **is** first stored key (`frac=0` would
copy key 0 inside `00A4C1F0` too). Later keys / frac
blend stay **LEFTOVER**.

`TrianglesForPose(sequence)` still passes `time=0f`.
`FirstSeenXseqAppliesFrac=false`.

Keep first-key as the host pose sample. Do not wire mixer
channels into first Present.

---

## 7. PlayAnimation apply **PROVEN**; runtime **PARTIAL**

Thing `vtbl+72` `004C7470` → `+68` `00686920` accept →
`00662A00` table → `0070C050`+`0070D580` inner. Host
`ApplyInner` sets `ClipKey`, `PlayTime=0`,
`InnerApplied`. That apply is **PROVEN**.

Runtime leftover is **not** the named-clip store. It is
the mixer time walk that would cycle keys after channels
are armed. First-seen does not arm them. Even after apply,
host pose is `FloorKey` / first-key, not `00AA0090`.

`ChannelArmed=true` in `ApplyInner` is **INVENTED** vs
native mixer list length.

---

## 8. First Present — do **not** apply interp

| Claim | Class |
|---|---|
| First `00AA0090` is first PALSKIN dest pack `00BD2E35` | **PROVEN** |
| That pack has channel count 0 | **PROVEN** `jbe 00AA097D` |
| Skip never produces key/frac | **PROVEN** |
| First Present dest = bind locals (`00A9E1E0`) | **PROVEN** / **MATCH** host |
| First Present applies `PaletteForPose(clip)` / frac slerp | **DISPROVEN** |
| `FirstSeenPlayAnimationAppliesPose=false` | **MATCH** |
| `FirstSeenPlaysAnim=false` | **MATCH** |
| `FirstSeenXseqAppliesFrac=false` | **MATCH** |
| Live count `> 0` would run `00A52650` then `00A4C1F0` | **PROVEN** listing. **LEFTOVER** as New Game |

No listing evidence that first Present arms 20-byte
channels. Applying interp there would invent a product.

---

## Classifications (short)

1. **Frac formula — PROVEN.** Mixer
   `time = A.t + (B.t-A.t)*t`, then `00A52650`
   `frac = time*[clip+80] - key` with
   `key %= [clip+84]`. Units of `+80/+84` **PARTIAL**.
2. **First-seen skip — PROVEN.** Count 0 → `jbe 00AA097D`.
   `00A52650` / `00A4C1F0` do not run.
3. **Do not apply interp on first Present.** Skip is
   listed, not unread. Host bind / first-key **MATCH**.
   `FirstSeenPlayAnimationAppliesPose=false` stays.
4. **PlayAnimation apply PROVEN; runtime PARTIAL.** Leftover
   is mixer channel arm + this walk, not the `ClipKey` store.
5. **Type-6 first-key → `PaletteForPose` MATCH format**
   (`00A999B0` / `00AA4680` / `00A4C5E0`). Host `FloorKey`
   drops frac. Do **not** slerp.

Do not treat `TimeToKey` + first-key as a finished mixer.
Do not treat `state.PlayTime` as `00AA0090` channel `+12`.
Do not apply clip locals on New Game Present.
