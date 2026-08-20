# Native DEFAULT appearance duration (`0070D580` / mode 6)

Investigation only. Production `src/` / `tests/` were not edited.

Do **not** invent `Duration=1` to make
`Appearance_DEFAULT_starts_0070D580` pass. Current fail:
expected `1`, actual `0.166666672` (`clip.Duration` / request
mode 6).

Do **not** start at Oakvale / `3420` / create `006AC910`.
`005B37F7` is clothing / `PC_UI_FRAME`, not first-seen
(`proofs/appearance-0042B0A2-first`, `hero-idle-anim`).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: what is native DEFAULT appearance duration? Is
`1.0` a host invent? Is `clip.Duration/6` the native
first-seen? What is XSEQ / request **mode 6**?

Authority: listings `0070D580` / `0070C050` / `0070D1A0` /
`0070D100` / `0070B4D0` / `005B37F7` / `005B3A24` /
`00663E90` / `005DC340` / `0070BCE0`;
`calls-inner-play-0070d580`;
siblings `appearance-0042B0A2-first`, `xseq-walk-first`,
`audit-xseq`, `hero-idle-anim`, `anim-blend-first`.

---

## Verdict

**`clip.Duration / 6` is a host conflation, not native.**
**`Duration=1` on the isolated DEFAULT test is a host
invent** (`LookupClip` miss → `AnimationClipRecord("DEFAULT",
1f)`). Native DEFAULT clip length is **UNREAD**. First-seen
does **not** play DEFAULT, so there is **no** native
first-seen duration.

`0070C050` arg0 `6` is a **request class byte** at
`[request+4]` → `[inner+80]`. `0070D580` does **not**
divide by that byte. It divides by **arg1**, and every
DEFAULT / appearance-table site passes **arg1 = 1**.

| Claim | Class |
|---|---|
| Isolated test `Duration=1` is native DEFAULT length | **DISPROVEN** (host miss fallback) |
| `clip.Duration/6` is native `0070D580` | **DISPROVEN** |
| `clip.Duration/6` is first-seen | **DISPROVEN** (no `005B37F7` / `0070D580`) |
| Mode 6 is the duration divisor / playback speed | **DISPROVEN** |
| Mode 6 is `0070C050` arg0 → `[request+4]` → `[inner+80]` | **PROVEN** |
| DEFAULT `0070D580` arg1 is `1` | **PROVEN** (`push 1`) |
| `[channel+56] = [channel+44] / max(arg1,1)` | **PROVEN** (`fild` / `fdivr [ecx+44]`) |
| Native DEFAULT `[channel+44]` / ANRT seconds | **UNREAD** (`CAppearanceDef` +52 body) |

---

## Evidence → Original → Host → Gap

### 1. Evidence — `0070C050` request (mode 6 lives here)

`listing-00700000.txt` (`0070C050`–`0070C096`, `ret 28`).
Seven stdcall args. DEFAULT pack (`005B3A67` /
`appearance-default-request-005b3a24`):

```
push 100          ; arg6  → [req+40]
push eax          ; arg5  → [req+32]  (005DC390 flags)
push 1            ; arg4  → [req+5]
push 1            ; arg3  → [req+0]
push edi          ; arg2  → [req+8]   clip from +52
push 0            ; arg1  → [req+36]
push 6            ; arg0  → [req+4]   BYTE
lea ecx, [ebp-60]
call 0070C050
```

```
0070C050  mov dl, [esp+4]           ; arg0
0070C054  mov eax, 0x3F800000       ; 1.0
0070C059  mov [ecx+12], eax
0070C05C  mov [ecx+16], eax
0070C05F  mov [ecx+20], eax
          [ecx+24]=0; [ecx+28]=0
0070C074  mov [ecx+4], dl           ; MODE BYTE
0070C07B  mov [ecx+8], eax          ; clip
          …
0070C096  ret 28
```

Ctor sibling `0070BF10` / `0070C010` writes the same 1.0
triple and `[+40]=0x64` (100). Mode byte defaults to 0.

`0070B4D0` (`CTCAnimationComplex` vtbl+16) is the same pack:
`push "DEFAULT"` → `005DC340` → `push 6` → `0070C050`.
**PROVEN.**

Named script play `00663E90` packs **`push 1`** as arg0, then
the same `0070D580` tail. **PROVEN** (mode 1 named, mode 6
DEFAULT / appearance-table).

### 2. Evidence — `0070D580` divisor is **arg1**, not mode 6

`listing-00700000.txt` (`0070D580`–`0070D775`, `ret 12`).
Three stdcall args: request, **arg1**, arg2.

DEFAULT / `0070B4D0` / door `00730CFB` / named `00663F23`
all:

```
push 0              ; arg2
push 1              ; arg1   ← DIVISOR
push request        ; arg0
call 0070D580       ; ecx = inner ([comp+12] / 0070B460)
```

`005B37F7` does `push 0; push 1; push request; call
0070B460` (`mov eax,[ecx+12]; ret` — does **not** pop).
Leftover args are `0070D580`'s. Wrapper `0070D920` is the
same `push 0; push 1; push eax; call 0070D580`.

Stack after `sub esp,16` + four saves + `0070D100` (`ret 4`):

```
[esp+36]  arg0 request
[esp+40]  arg1          ; edi
[esp+44]  arg2          ; [inner+144]
```

Duration walk (existing channels from `0070D100`, only if
`[ch+81] & 0x10` and not already `& 0x02`):

```
0070D5B7  mov edi, [esp+40]          ; arg1
0070D5CB  mov [esp+16], edi
0070D5D3  cmp edi, 1
0070D5D9  jge 0070D5E3
0070D5DB  mov [esp+16], 1            ; floor at 1
0070D5E3  fild [esp+16]
0070D5E7  fdivr [ecx+44]             ; ST = [ch+44] / max(arg1,1)
0070D5EA  fstp [ecx+56]
          or [ch+81], 0x02
```

`arg1 <= 0` later `jle 0070D71D` skips the time walk
(`[inner+140]=0`, scale slot `1.0`).

Step (`[0x122DED8]=1.0`):

```
0070D67F  fild [esp+40]              ; arg1 again
0070D689  fdivr [0x122DED8]          ; 1.0 / arg1
0070D68F  fstp [esi+140]
          [channel+64] += [esi+140]
0070D745  fld [esi+140]
0070D74E  fchs
0070D754  fstp [esi+56]              ; [inner+56] = -step
```

**Original:** DEFAULT duration scale uses **divisor 1**.
Mode 6 never enters `fild`. **PROVEN.**

### 3. Evidence — mode 6 is copied, then compared as an id

`0070D1A0` (`ret 12`) copies the request onto the inner
object **after** the first channel walk:

```
0070D2A3  mov cl, [edi+4]            ; request mode byte
0070D2A6  mov [esi+80], cl
0070D2A9  mov edx, [edi]             ; request+0
0070D2AB  mov [esi], edx
          [esi+44] = scale (0 or 1.0 from 0070D580 [esp+36])
          [esi+48] = same
          [esi+52]=0; [esi+56]=0     ; then 0070D580 overwrites +56
          [esi+64] = [req+32]
          [esi+76] = [req+40]        ; 100
```

`0070BCE0`: `movzx eax, [arg+80]; sub eax, [ecx];` → `eax==1`
iff mode byte equals `[this]`. **PROVEN** as an equality key.
**UNREAD** as a named enum (no string, no switch on 6 in
`0070D580`).

Not FPS: `0070C050` writes 1.0 at `+12/+16/+20` independently.
Not ANRT duration: that is persist `[ANRT+48]`
(`proofs/audit-xseq`), a different object.

Sites that `push 6` into `0070C050`: `005B37F7` DEFAULT,
`0070B4D0` DEFAULT, `00730CE0` appearance `DOOR_OPEN`,
`00746942` / `00749C05` / `0077D29E` same pack. Script
`PlayAnimation` `00663F08` pushes **1**.

**Original meaning recovered:** mode 6 = appearance-table /
DEFAULT **request class**. Mode 1 = named script play.
**PARTIAL** (byte **PROVEN**; English name **UNREAD**).

### 4. Evidence — `[ecx+44]` is not proven XSEQ seconds

| Object | `+44` | Class |
|---|---|---|
| ANRT persist | cyclic u8 at +44; **f32 duration at +48** | **PROVEN** (`00A98AF0`) |
| Channel after `0070D1A0` | copy of play **scale** (0 or 1.0) | **PROVEN** store. **DISPROVEN** as ANRT seconds |
| `0070BED0` | `fld [ecx+44]; ret` | getter. Lerp sibling `0070BEB0` is `+44 + (+48-+44)*t` |
| Appearance +52 row | 20-byte name walk `005DC340` (`add esi, 20`) | **PROVEN** stride. Payload **UNREAD** |
| `CAppearanceDef` idx 10533 | type present | body **UNREAD** |

`0070D580`'s first `fdivr [ecx+44]` runs on **already-live**
channels (`0070D100` lists `[inner+88]` / `[inner+100]`).
Empty list (`edx==ebp`) skips it. A fresh DEFAULT then gets
`[inner+56] = -1/arg1 = -1.0`, not `clipSeconds/6`.

Host `AnimationClipRecord` comment “Duration is `[clip+44]`
in `0070D580`. Unread clips use 1.” mixes three fields.
The `1` is the unread-clip **invent**.

### 5. Original (native DEFAULT play, when it runs)

```
005B37F7 / 0070B4D0
  0042B0A2 CAppearanceDef
  +52  005DC340("DEFAULT")
  0070C050(mode=6, clip, 1.0s, flags, 100)
  0070B460 [comp+12]
  0070D580(request, arg1=1, arg2=0)
    existing ch: [ch+56] = [ch+44] / 1
    [inner+80] = 6
    [inner+140] = 1.0 / 1
    [inner+56] = -1.0
```

Callers: clothing GUI `005B4E7F`, `PC_UI_FRAME` `005B8758`
only. **PROVEN** (`calls-appearance-default-play-005b37f7`).
Create / Leave / first Present: **DISPROVEN**
(`FirstSeenAppearancePlaysDefault=false`).

XSEQ first-key / `00AA0090` time: **UNREAD** on this path
(`xseq-walk-first`). Playing until ANRT seconds elapse is
**UNREAD**.

### 6. Host

`PlayAppearanceDefault` → `BeginInnerPlay(..., "DEFAULT", 6)`
→ `ApplyInner`:

```
playMode = mode <= 0 ? 1 : mode;     // 6
RequestMode = playMode;              // MATCH request+4
Duration    = clip.Duration / playMode;
Step        = 1f / playMode;
```

`LookupClip` miss (empty `Clips`, isolated test):

```
return new AnimationClipRecord("DEFAULT", 1f);
```

`AnimationClipRecord` also `duration > 0 ? duration : 1f`.
`XSeqFile.Parse` same 1f fallback — **INVENTED** vs native
ANRT keeping 0 (`audit-xseq`).

`BeginInnerPlay` skips inventing a 1f record when the name
**is** `DEFAULT`, then `LookupClip` invents it anyway.

Test `Appearance_DEFAULT_starts_0070D580`:

| Field | Host now | Test |
|---|---|---|
| `ClipKey` | `DEFAULT` | MATCH |
| `RequestMode` | 6 | MATCH (`0070C050` arg0) |
| `PlayTime` | 0 | MATCH |
| `Duration` | `1f/6` ≈ `0.166666672` | expected `1` |
| first-seen inner play | false | MATCH |

Named `PlayAnimation` with a registered clip uses mode 1
and `Duration = clip.Duration / 1` — accidental **MATCH**
to native arg1=1, still the wrong *input* (request mode
instead of `0070D580` arg1).

### 7. Gap

| Native | Host | Class |
|---|---|---|
| `0070C050` arg0 = 6 on DEFAULT | `DefaultRequestMode=6` | **MATCH** |
| `0070D580` arg1 = 1 on DEFAULT | same `6` divides Duration/Step | **DIVERGE** |
| `[inner+140] = 1.0/arg1` | `Step = 1/6` | **DIVERGE** |
| DEFAULT clip seconds | `LookupClip` → `1f` | **INVENTED**. Native **UNREAD** |
| First-seen DEFAULT duration | none (no play) | **MATCH** skip if left idle; **LEFTOVER** if `PlayAppearanceDefault` from create |
| ANRT `[+48]` | `XSeqFile.Duration` (fallback 1) | **PARTIAL** parse; fallback **INVENTED** |
| `[channel+56] = [ch+44]/arg1` | `clip.Duration/requestMode` | **PARTIAL** formula, **wrong** inputs |

Do **not** “fix” the test by forcing `Duration=1`. That
re-encodes the miss fallback and hides the mode/arg1 split.

Do **not** change the test to expect `1/6`. That freezes
the host conflation.

---

## First-seen

```
Leave / Init Game / first pumps / 006AC910
  no 005B37F7 / 0070B4D0 / 0070C050 / 0070D580
  FirstSeenAppearancePlaysDefault=false
  FirstSeenPlaysAnim=false
  bind locals
```

**PROVEN** (`appearance-0042B0A2-first`). There is no
native first-seen DEFAULT duration to recover. `1.0` and
`clip.Duration/6` are both later-host numbers.

---

## Classification table

| Claim | Status |
|---|---|
| `0070D580` `ret 12`; args request / arg1 / arg2 | **PROVEN** |
| DEFAULT / `0070B4D0` pass arg1=`1`, arg2=`0` | **PROVEN** |
| `[ch+56] = [ch+44] / max(arg1,1)` | **PROVEN** |
| `[inner+140] = 1.0 / arg1`; `[inner+56] = -step` | **PROVEN** |
| Mode 6 is `0070C050` arg0 byte `[req+4]` / `[inner+80]` | **PROVEN** |
| Mode 6 is duration ÷6 or FPS | **DISPROVEN** |
| Mode 6 vs mode 1 = appearance DEFAULT vs named script | **PARTIAL** (sites **PROVEN**; enum name **UNREAD**) |
| Isolated `Duration=1` is native DEFAULT length | **DISPROVEN** (host `1f` invent) |
| `clip.Duration/6` is native `0070D580` | **DISPROVEN** |
| First-seen DEFAULT duration is 1 or duration/6 | **DISPROVEN** (no play) |
| Native DEFAULT ANRT / +52 seconds | **UNREAD** |
| `PlayAppearanceDefault` from `SpawnHero` | **DISPROVEN** as first-seen (**LEFTOVER** if called) |

---

## Do not

- Invent `Duration=1` as native DEFAULT length to satisfy
  `Appearance_DEFAULT_starts_0070D580`.
- Treat request mode 6 as `0070D580`'s `fild` divisor.
- Call `PlayAppearanceDefault` from `006AC910` / first
  Present (`appearance-0042B0A2-first`).
- Equate ANRT `[+48]`, channel `[+44]` scale, and
  `[inner+56] = -step`.
- Pair Lookout to `DEFAULT` / `STAND` / `3420`.
