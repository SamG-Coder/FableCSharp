# `0049E200` / `0051E530([world+80])` thing count

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale`.
No-save New Game is Leave `0042F2A2` → `FinalAlbion.wld` →
Init Game `004184BD` → vtbl+32 `00416953` → suffix
`0049BA70` / **`00416392`** / `004AE9D0`. First region is
later `00501450`, not this site.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: `00416392` → `0049E200` → `0051E530([world+80])`.
What is `[world+80]`? First-seen empty? Host leftover vs
`PlayerBindSlot1=WorldFrame`?

Authority: Fable.exe dump `listing-00400000.txt`
(`00416392` / `0041890E`), `listing-00480000.txt`
(`0049E200` / `0049E1B0` / `0049EBF0` / `004A67D0` /
`004A6E30` / `004AE9D0` / `0049D870` / `004A5E10`),
`listing-00500000.txt` (`0051E530` / `00523540` /
`005223F0` / `00521AE0` / `0051FD80` / `00520D00`),
`listing-00a00000.txt` (`00A373B0` / `00A372D0` /
`00A371F0` / `00A371C0`), `listing-004c0000.txt`
(`004C9030` / `004C74F0`), `rtti.txt` `CThingManager`.
Siblings: `00416392-after-initgame`, `005223F0-plus128-gate`,
`player-bind-world`, `init-world-004A6E30`, `tng-spawn`.
Host notes only: `EngineLifecycle.WorldThingCountApply` /
`PlayerBindSlot1` / `FinishInitGameAfterWorld`.

---

## Verdict table

| Claim | Answer | Class |
|---|---|---|
| What is `[world+80]`? | `CThingManager*` (`0xE8`, vtbl `0x1245C44`, RTTI `0x0137B970`). Stored by Init Thing Manager `0049EBF0` at `0049EC3D`. Getter `0049E1B0` is `mov eax,[ecx+80]; ret`. | **PROVEN** |
| Is `[world+80]` WorldFrame? | **No.** WorldFrame is BSS `[0x13B89BC]` (`0049D870`). Unique `inc` is `004A5E10`. | **DISPROVEN** |
| What is `0049E200`? | World thiscall: `ecx=[ecx+80]`; `eax=0051E530(ecx)+[0x13B89BC]`. Only `E8` of `0051E530`. | **PROVEN** |
| What is `0051E530`? | Walk `[manager+24]` circular list. Skip `[thing+145]&1`. Else add `vtbl+92()`. Empty sentinel → **0**. | **PROVEN** |
| First-seen empty after ctor? | **Yes.** `00A373B0` allocs 12-byte sentinel `[next]=[prev]=self` at `+24`. `esi==eax` → 0. | **PROVEN** |
| First-seen empty at `00416392`? | **Not** the dump-static New Game path. `005223F0` leftover `[manager+128]==1` takes `00521AE0` / `0051FD80` / `004C9030` → `00A371F0` insert onto `+24`. Ctor `+145=0x04` (bit 0 clear). Live occupancy **UNREAD**. | ctor empty **PROVEN**; at suffix empty **DISPROVEN** dump-static; live count **UNREAD** |
| Host leftover vs `PlayerBindSlot1=WorldFrame`? | **Yes leftover store.** Native `+9840` is `0051E530+WorldFrame`, not WorldFrame, not `[world+80]`. Host never calls `0051E530`. Init **MATCH** only if walk is 0. | leftover **PROVEN**; Init MATCH **DISPROVEN** vs dump-static insert |

---

## Direct answers

### What is `[world+80]`?

**`CThingManager` pointer. Not WorldFrame. Not the count.**

```
004A67D0  CWorld ctor
004A6831  mov [esi+80], ebx          ; 0

004A6E30  "Init Thing Manager"
004A7230  call 0049EBF0              ; ecx = world

0049EBF1  push 0xE8
0049EBF8  call 00BFEA1A
0049EC2E  call 00523540              ; ret 36
0049EC3D  mov [esi+80], eax          ; world+80
0049EC4B  call 004C74F0              ; [0x13B8A1C] = manager
```

`00523540` (`listing-00500000`):

```
00523546  call 00A373B0              ; push 16; +24 empty sentinel
0052355A  mov [esi], 0x1245C44       ; CThingManager vtbl
005235CD  mov [esi+128], 0x1         ; 005223F0 construct gate
0052358E  mov [esi+72], ebx          ; flush pair +72/+76 = 0
00523591  mov [esi+76], ebx
```

`00A373B0` (`listing-00a00000`):

```
00A373D5  push 12
00A373DA  call 00BFEA0E
00A373DF  mov [eax], eax             ; next = self
00A373E1  mov [eax+4], eax           ; prev = self
00A373E4  mov [esi+24], eax          ; sentinel
00A373F0  mov [esi+20], ebx          ; occupancy 0
```

Same pointer as world vtbl+12:

```
0049E1B0  mov eax, [ecx+80]
0049E1B3  ret
```

`004FBF60` uses that getter before `005223F0`.

**Not** these nearby `+80` fields:

| Object | `+80` | Class |
|---|---|---|
| `CWorld` | this pointer | **PROVEN** |
| `[0x13B89BC]` | WorldFrame dword | **DISPROVEN** as `world+80` |
| `CWorld` `+128` | loaded flag `0049D970` | **DISPROVEN** |
| `CThingManager` `+128` | construct gate | **DISPROVEN** as the pointer |
| Create Players `0x22C` slots | player objects | **DISPROVEN** |

---

### Chain `00416392` → `0049E200` → `0051E530`

```
00416392  xor dl, dl
00416394  cmp [ecx+90394], dl
0041639A  je 004163AF                ; first-seen taken
004163AF  mov ecx, [ecx+36]          ; world = [game+36]
004163B2  jmp 0049E200
```

```
0049E200  mov ecx, [ecx+80]          ; CThingManager
0049E203  call 0051E530
0049E208  mov ecx, [0x13B89BC]       ; WorldFrame
0049E20E  add eax, ecx
0049E210  ret
```

```
0051E530  edi = ecx
          eax = [edi+24]             ; sentinel
          esi = [eax]
          ebp = 0
          if esi == eax: return 0
0051E540  ecx = [esi+8]              ; CThing
          test [ecx+145], 1
          jne skip
          call [vtbl+92]
          add ebp, eax
          esi = [esi]
          cmp esi, [edi+24]
          jne 0051E540
          return ebp
```

Only `call 0051E530` in the PE is `0049E203`. **PROVEN.**

First-seen site is `0041890E` (Init Game suffix, after
`00416953`, before dummy `004189C2` / `00501450`).
`+90394` has **cmp** only in `.text`. **PROVEN** gate 0.

`004AE9D0` (`listing-00480000`):

```
004AE9D0  if ![ecx+9826]: ret 12
          [ecx+9836] = arg1          ; [game+72]
          [ecx+9840] = arg2          ; 00416392 eax
          [ecx+9844] = arg3          ; [game+90428]
```

Create Players already set `+9826=1`. **PROVEN.**

---

### First-seen empty?

**Two times.**

| When | `[manager+24]` | Class |
|---|---|---|
| After `00523540` / before `"Loading world"` | empty circular; `0051E530==0` | **PROVEN** |
| After `004FDBC0` / at `0041890E` | dump-static **not** empty | empty **DISPROVEN** dump-static; live **UNREAD** |

Why the suffix is not empty on the dump-static path
(`proofs/005223F0-plus128-gate`):

1. Ctor writes `[manager+128]=1`. First-seen
   `AllowDataGeneration` `[0x1375459]==0` skips
   `004FE030`. Nothing else rewrites the gate before
   `004FDBC0`.
2. `005223F0` `cmp [esi+128],1` therefore **takes**
   `00521AE0` + `0051E2F0`.
3. `00520D00` `"NewThing"` → `0051FD80` →
   `"Allocate Class"` `00A371C0` → CThing ctor
   `004C9030`.
4. `004C9030` uses singleton `[0x13B8A1C]` (the same
   `[world+80]`) and `00A372D0` → `00A371F0`:

```
00A37209  push 12
00A3720D  mov esi, [ebx+24]          ; sentinel
          alloc; [node+8] = thing
          splice before sentinel
00A372BE  inc [ebx+20]
```

5. Same ctor: `mov [esi+145], 0x04` — bit 0 **clear**,
   so `0051E530` **adds** `vtbl+92`.

First-seen `LookoutPoint.tng` is the first `004FBF60(1)`
file (`004FDBC0-open`). One successful NewThing is
enough to make `0051E530≠0`. Exact sum (and `vtbl+92`
identity 1 vs subtree) is **UNREAD**.

Sibling `00416392-after-initgame` treated walk **0** as
the working model. That model is **DISPROVEN** against
the `+128` writer. Do not keep “parse-only so count 0”.

`0051E2F0` after the global load walks the **job
vector**, not `+24`. It does not empty the list.
`005224B0` frees the **local** vector.

---

### Host leftover vs `PlayerBindSlot1=WorldFrame`

`[world+80]` is **not** the bind slot and **not**
WorldFrame.

Native `004AE9D0` `+9840` = `00416392` =
`0051E530([world+80]) + WorldFrame`.

Host (`FinishInitGameAfterWorld` / `AdvanceGameTicks`):

```
PlayerBindSlot1 = WorldFrame;
```

No `0051E530`. No `CThingManager`. No `+24` walk.

| When | Native `+9840` | Host `PlayerBindSlot1` | Class |
|---|---|---|---|
| Init Game `0041891D` | walk + 0 | 0 (`WorldFrame`) | **DIVERGE** if walk≠0 (dump-static); host 0 is **LEFTOVER** |
| After ticks, walk still 0 | WorldFrame | WorldFrame | **MATCH** only on that counterfactual |
| After ticks, walk ≠ 0 | walk + frame | frame only | **DIVERGE** |

Host `LoadGlobalThingsFile` parse-without-`LoadSingleThing`
is the **same leftover** as the zero walk (`005223F0`
gate taken dump-static). Do not “fix” this VA by writing
`PlayerBindSlot1 = WorldFrame` as if that were
`[world+80]`.

`004A5E10` `inc [0x13B89BC]` has not run at
`0041890E`. Frame addend **0** is **PROVEN**. The
leftover is dropping the walk, not the frame.

---

## Timeline (no `00DBDE40`)

```
0041735A  Init World
  004A67D0  [world+80]=0
  004A6E30  Init Thing Manager 0049EBF0
    00523540  CThingManager; +24 empty; +128=1
    [world+80]=eax
    004C74F0  [0x13B8A1C]=eax
00416953  Loading world
  00507C30 / 004FDBC0
    005223F0  leftover +128==1 → 00521AE0   ; +24 fills
0041890E  00416392 → 0049E200 → 0051E530([world+80])+0
0041891D  004AE9D0  +9840 = that eax
… later 004A5E10 inc WorldFrame
… later 00501450 first real region
```

---

## Not these

| Claim | Class |
|---|---|
| `[world+80]` is WorldFrame / `[0x13B89BC]` | **DISPROVEN** |
| `[world+80]` is `PlayerBindSlot1` | **DISPROVEN** |
| `0049E200` constructs / opens TNG | **DISPROVEN** |
| `0051E530` is `0051E2F0` / `0051E5A0` / `0051F070` | **DISPROVEN** |
| First-seen `+90394` arm returns 1 | **DISPROVEN** |
| StartOakVale / `00DBDE40` on this walk | **DISPROVEN** |
| Host `PlayerBindSlot1=WorldFrame` equals native `+9840` at Init | **DISPROVEN** dump-static |
| `00416392` is the `+128` construct gate | **DISPROVEN** |

---

## Do not

- Start Oakvale / `00DBDE40` as New Game.
- Call `[world+80]` WorldFrame.
- Store host `PlayerBindSlot1` as proof of `0051E530`.
- Keep first-seen count 0 as MATCH against leftover `+128==1`.
- Treat `0049E200` as Thing *create*.
