# `0052CF40` arg 6 jump table, child `vtbl+188`, why old slots stay off

Investigation only. No production `src/` edits.

Question: after `00596763` old current
`vtbl+192`(6) (`0052CF40`), what does **arg 6**
do in the `cmp ebp,6` / `ja` / `jmp [0x52D368+edx*4]`
table? Type 10 `012497E4` `+188` / `+192`?
Child `vtbl+188` on forwarded 6? `0041C5A0`?
Does 6 write clip `+302`, dest offscreen,
`00530260` `vtbl+400`/`+420`, or skip at
`00595222`? How does native Press Start stay
undrawn on New Profile / New Profile undrawn on
Main Menu if `00595222` still walks every
non-null `[ui+84]` slot?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00500000.txt`
(`0052CF40`–`0052D398` / `0052CAF0`–`0052CBE0` /
`0052C730` / `0052C7E0` / `00530260`–`005303E0` /
`0052F180` / `0052F1D0` / `0052FFD0` / `00531EC0` /
`00533288`);
`listing-00540000.txt` (`0054E3D0` / `00547C90`–
`00547E3A` / `00548F40`–`00549194` / `00547600` /
`00549F60`);
`listing-00400000.txt` (`0041C5A0`–`0041C5B7` /
`0042E085`);
`listing-00580000.txt` (`00595222` / `005967C3`);
`listing-01200000.txt` ends `0122CFFE`;
`out/00-index/sections.txt`;
`proofs/0052CF40-selectstate-6`,
`proofs/0052CF40-vtbl188-forward`,
`proofs/00595222-visible-skip`,
`proofs/00596763-switch`,
`proofs/type16-18-present-child`.

Do not re-prove `00596763` as old-current
`vtbl+192`(6), `[ui+84]` keeping `0x14`/`0x17`,
or persist Type=10 on those roots. Do not map
`SelectState(6)` to `Visible=false`. Do not invent
clip CRC. Do not start Oakvale.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN**.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| Arg 6 jump? | `cmp ebp,6` / `ja 0052CFDD`. Compact byte `[0x52D374+arg]`. Arg **6 → index 0**, same group as arg **0**. Then `jmp [0x52D368+edx*4]` into `vtbl+564` (`0052CFC7`) / `vtbl+560` (`0052CFD3`) / neither (`0052CFDD`). All three fall into `vtbl+540(arg)` + child `vtbl+188`. | Compact map **PROVEN**. Which of the three arms index 0 hits **PARTIAL** (table dwords listed as code). |
| Type-10 `012497E4+192`? | Shared select body is `0052CF40`. Ctor `0054E3D0` writes `012497E4` and has **no** `+192` thunk (type 12 / 18 wrap then `E8 0052CF40`). `.rdata` dword past `listing-01200000`. | Body **PROVEN**; rdata dword **UNREAD** |
| Type-10 `012497E4+188`? | Not in `.text` ctor. `.rdata` **UNREAD**. | **UNREAD** |
| Child `vtbl+188`(6)? | `push duration; push 6; call [edx+188]`. Not draw. Type-8 skip is 1/3/4 only — **6 does not skip**. | **PROVEN** |
| `0041C5A0`? | `ret 8`. `[this+320]=arg1`; `vtbl+192(arg0)`. Type 16/18 `.rdata +188` **UNREAD** this dump set. | Body **PROVEN** |
| Write `+302`? | **No.** | **DISPROVEN** |
| Dest offscreen via `+332=6`? | Dest `0052FFD0` / `00531EC0` never load `+332`. Tick style is `vtbl+540(+328)`, and `+328=arg` only on the **animated** arm (`vtbl+176` true at `0052D0CA`). | Direct dest write **DISPROVEN**. Animated style-6 dest **PARTIAL** |
| `00530260` skip via `vtbl+400`/`+420` because of 6? | Those tests do not read `+332`. `+420` is `[+302]&1`. Arg 6 does not set it. | **DISPROVEN** |
| Skip at `00595222`? | Null `[node+20]` only. | **DISPROVEN** |
| Why old slot looks undrawn? | Walk still calls every resident slot `vtbl+8`. Covering by later full-screen slots is **DISPROVEN** as the native hide (key 0 Main Menu would sit **under** `0x14`/`0x17`). Native DIP/dest of a state-6 tree **UNREAD**. Host `SelectFrontendState` only writes `tree[0].State=6`; `ResidentSlotTrees` still presents every slot. | Walk **PROVEN**. Native hide **UNREAD**. Host overlap **LEFTOVER** vs a working native Present |

---

## Verdict

**Arg 6 is a style-key select, not a draw hide.**

`0052CF40` stores `+332=6`, clears the `+316` list,
rotates `+324/+344`, optionally calls `vtbl+564` or
`vtbl+560` (arg 6 shares compact **index 0** with
arg 0), then `vtbl+540(6)` and every owned
non-type-8 child `vtbl+188(6, duration)`.

It does **not** `or [+302],1`. `00595222` still
walks Press Start `0x14` and New Profile `0x17`
after Main Menu is current. `00530260` still walks
that root’s `+176`. Skip there is persist
`vtbl+420` / borrowed `vtbl+400`, not `+332==6`.

`Visible=false` as `SelectState(6)` is **DISPROVEN**.

Type 16/18 `0041C5A0` (candidate `+188`) writes
duration at `+320` and **forwards** `vtbl+192(6)`.
Type 18 `00547C90` then `0052CF40(6)` and returns
(extra work is **arg 3 only**). That still does not
clip.

How native actually omits those DIPs on a later
screen is **UNREAD** (no pixel metrics in this
dump). It is **not** the four skip sites above.

| Claim | Status |
| --- | --- |
| `cmp ebp,6` / `ja 0052CFDD` / `movzx edx,[ebp+0x52D374]` / `jmp [0x52D368+edx*4]` | **PROVEN** |
| Byte table `0x52D374[0..6] = 0,1,2,2,2,1,0` | **PROVEN** |
| Arg 6 compact index **0** (same as arg 0) | **PROVEN** |
| Index 0 → which of `0052CFC7` / `0052CFD3` / `0052CFDD` | **PARTIAL** |
| After that jump, `vtbl+540` + child `+188` still run | **PROVEN** |
| `+332=6`; no `+302` in `0052CF40` | **PROVEN** / **DISPROVEN** as clip |
| Type-10 ctor vtbl `012497E4`; no `+192` override in `0054E3D0` | **PROVEN** |
| `012497E4+188` / `+192` dwords | **UNREAD** (`012497E4` is `.rdata`; text-map ends `0122CFFE`) |
| `0041C5A0` is `+320=duration; vtbl+192(state)` | **PROVEN** |
| Type 16/18 `.rdata +188 == 0041C5A0` | **UNREAD** |
| Type 18 `+192` `00547C90` arg 6 is `0052CF40` then epilogue | **PROVEN** |
| Type 16 `+192` `00548F40` arg 6 is default → `0052CF40(6)` | **PROVEN** |
| `00595222` filters current / `+332` / `+302` | **DISPROVEN** |
| `00530260` loads `+332` | **DISPROVEN** |
| Host `Visible=false` on arg 6 | **DISPROVEN** as native |
| Native inactive-slot DIP list | **UNREAD** |

---

## 1. Jump table for arg == 6

`0052CF40` `ret 4`. `ebp` = arg. Early-out if
`[this+332]==arg`. Else `mov [esi+332], ebp`.

```
0052CF93  cmp ebp, 6
0052CF96  mov eax, [edi]
0052CF98  mov [eax], eax
          … relink +316 sentinel; +344←+324; +324←+328 …
0052CFB7  ja 0052CFDD
0052CFB9  movzx edx, [ebp+0x52D374]
0052CFC0  jmp [0x52D368+edx*4]
0052CFC7  call [eax+564]
0052CFD1  jmp 0052CFDD
0052CFD3  call [edx+560]
0052CFDD  push ebp
          call [vtbl+540]
          push ebp
          call [vtbl+176]
```

`ja` is unsigned: arg `>6` skips the table and still
hits `0052CFDD`. Arg **6 is in range**.

Linear listing treats `0x52D368` as code
(`mov edi, 0xCFD30052`). Same artefact as type-12
`jmp [0x54D154+eax*4]` (`0054D154  dec eax` = low
byte of `0054CC48`). Do not take those mnemonics as
the table.

Byte table as data (`add [eax],al` / `add [edx],eax`
/ `add al,[edx]` / `add [eax],eax` at `0052D373`):

```
VA 0x52D374  (7 bytes, args 0..6)
00 01 02 02 02 01 00
```

| Arg | Compact `edx` | Used on frontend switch? |
| ---: | ---: | --- |
| 0 | 0 | first-seen `0052C730` already stored `+332=0` |
| 1 | 1 | |
| 2 | 2 | |
| 3 | 2 | type-8 child skip inside this fn; not `00596763` |
| 4 | 2 | |
| 5 | 1 | new current `0059A119` |
| **6** | **0** | old current `005967C9` |

Arg 6 is **not** a unique hide arm. It shares the
`vtbl+560/+564/none` group with **arg 0**.

Second table at `0x52D37C` is **not** this switch.
Animated arm (`vtbl+176` true):

```
0052D0CA  mov [esi+328], ebp          ; +328 = arg
          duration from style+28 or +320
0052D0F1  mov ebx, [ebx+4]
0052D0F4  cmp ebx, 4
0052D0F7  ja 0052D35E
0052D0FD  jmp [0x52D37C+ebx*4]
```

First dword of that table reconstructs as
`04 D1 52 00` = `0052D104` (style-kind 0). That
path still ends in the same child `vtbl+188` walk.

Non-animated arm does **not** write `+328=6`.
Tick `0052C7E0` looks up style with **`+328`**, not
`+332`.

---

## 2. Type 10 `012497E4 +188` / `+192`

Ctor `0054E3D0`:

```
0054E3D8  call 0052CC50          ; type 5, vtbl 01245DE4
0054E3DF  mov [esi], 0x12497E4
0054E3E5  mov [esi+4], 0x12497BC
0054E3EC  mov [esi+24], 0x12497B4
          xor +352/+356/+360
```

No store to a `+192` slot in this cluster.
`.rdata` `012497E4` sits in `.rdata`
(`sections.txt` `.rdata` VA `0x0122D000`).
`listing-01200000.txt` stops at `0122CFFE`.
`vtbl 0x012497E4` was **not** in this dump set.

`005967C9` `call [eax+192]` with `this` = type-10
Press Start therefore runs the shared
`0052CF40` body (type 12 `0054CBF0` / type 18
`00547C90` are wrappers around that same body).

Slot `+188` on the **type-10 object** is unused on
this switch: `00596763` calls **`+192`**, then
`0052CF40` calls **child** `+188`.

---

## 3. Child `vtbl+188` on forwarded 6, and `0041C5A0`

Non-animated walk (same test on every animated
walk):

```
0052D050  child = [this+176][i]
          parent = child.vtbl+208
          if parent != this: skip
          if child.vtbl+260 == 8 and +332 in {1,3,4}: skip
0052D090  child.vtbl+188( [this+332], [this+336] )
```

Arg 6 fails `{1,3,4}`. Press Start kids are types
5 / 18 / 12 / 6 / 32, not 8.

`0041C5A0` (`listing-00400000.txt`):

```
0041C5A0  mov eax, [esp+8]       ; duration
0041C5A4  mov edx, [ecx]
0041C5A6  mov [ecx+320], eax
0041C5AC  mov eax, [esp+4]       ; state
0041C5B0  push eax
0041C5B1  call [edx+192]
0041C5B7  ret 8
```

ABI matches the `0052CF40` call (2 stack args,
`ret 8`). Effect of forwarded 6: duration at
`+320`, then **this** object’s `vtbl+192(6)`.

Candidate default `0052CAF0` is a different `ret 8`:
`+332=arg0`, `vtbl+540`, child `vtbl+168` — **no**
`call [vtbl+192]`. Which class uses which dword is
`.rdata` **UNREAD**.

Type 18 `+192` `00547C90` when `0041C5A0` (or anyone)
forwards 6:

```
00547C9C  mov ebx, [esi+328]
00547CA2  push edi                 ; arg
00547CA3  call 0052CF40
00547CA8  cmp edi, 3
00547CAB  jne 00547E34             ; arg 6: epilogue
…
00547E34  pop edi / pop esi / pop ebx / add esp,12 / ret 4
```

Type 16 `+192` `00548F40`:

```
00548F75  sub eax, 0   ; arg 0
00548F7E  sub eax, 3   ; arg 3
00548F87  sub eax, 2   ; arg 5
          default: push edi / jmp 00549179
00549179  call 0052CF40            ; arg 6 lands here
```

Forwarded 6 on 16/18 is another `+332=6` + child
`+188`, not a clip.

---

## 4. Clip / dest / `00530260` / `00595222`

`+302` writers in `listing-00500000.txt`: ctor persist
`00533288` `or [ebx+302],1` from `def+392`, centre
`+2`, type-6 align `+8/+10/+20`, ctor zero.
**None** in `0052CF40` / `0041C5A0` / `00547C90` /
`00548F40`.

`vtbl+420` `0052F1D0`: `mov al,[ecx+302]; and 1`.
`vtbl+400` `0052F180`: `[+300]>>7`.

`00530260` (type 5/10/12/16/18 `vtbl+8`): layer
`vtbl+404`/`+416`, then `+176` / `+188` children:

```
parent = child.vtbl+208
if parent != this && !child.vtbl+400: skip
if child.vtbl+420: skip
if child.vtbl+420: skip
else child.vtbl+8(...)
```

No `[+332]`. Slot **root** `vtbl+8` is called from
`00595222` with no clip test on `this`.

`00595222`: in-order `[ui+84]`, skip only
`[node+20]==0`, `call [vtbl+8]`. No `ui+32`, no
`+332`, no `+302`. After `00596763` keys `0x14` and
`0x17` stay non-null.

Dest: `0052FFD0` / `00531EC0` have **no** `+332`
load in `listing-00500000.txt`. Tick applies style
flags from `vtbl+540(+328)` (`bit 0x10` colour,
`0x20` zero `+76/+80`, `0x40` scale 1). `+328=6`
only if `vtbl+176` was true. That is not a store of
an offscreen dest rectangle inside `0052CF40`.

---

## 5. All-slot walk vs “undrawn”

`0042E085` pushes device + **0** into `00595222`.
Native still calls Press Start and New Profile
`vtbl+8` when Main Menu is `[ui+32].back()`.

In-order keys put Main Menu at **slot 0**
(`00595A06` overwrites key 0) **before** Press
Start `0x14` and New Profile `0x17`. If every slot
submitted a full-screen opaque blit, later keys
would cover Main Menu and native Main Menu would
never show. Native Main Menu **does** show. Covering
is **DISPROVEN** as the hide.

Host `ResidentSlotTrees` + `DrawContainerWalk`
presents every kept tree. `SelectFrontendState`
only does `tree[0].State = 6` and does **not** call
`ApplySelectState` (and must not map that to
`Visible=false`). That is why a host CPU blit of
Main Menu can be covered by New Profile: host still
draws the later slot’s dests. That is leftover vs a
correct native Present, not a licence to invent
clip.

Native mechanism that prevents those `vtbl+8` calls
from enqueueing the old screen’s DIPs is **UNREAD**
here (submit / style-6 dest / alpha / empty
`vtbl+540(6)`). It is **not** `+302`, **not**
`00595222` current-only, **not** `Visible=false`.

---

## Do not invent

- Clip CRC / persist `+392` name.
- `SelectState(6)` → `Visible=false`.
- Jump-table dwords from the garbled
  `mov edi, 0xCFD30052` line.
- `012497E4+188` / `+192` / type 16/18 `+188`
  rdata values (run `vtbl 0x012497E4 80` /
  `vtbl 0x012485AC 80` / `vtbl 0x01248A8C 80`).
- Dest pixels of a state-6 Press Start Present.

**Proposed (do not apply here):** keep the slot map
and the all-slot `vtbl+8` walk; keep `+332=6` on
the old root; do not hide via `Visible`. Recover
the submit skip from `vtbl 012497E4+188` and
`vtbl+540(6)` before changing draw.
