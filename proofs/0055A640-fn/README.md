# Window `0055A640`–`0055A740`: `0055A726 jmp 0055ACF0` lives in `0055A660`

Investigation only. No production `src/` edits.

Authority: `Fable.exe` listing
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0055A510` / `0055A5D0` / `0055A630` / `0055A660` / `0055A740` /
`0055A7C0` / `0055A9C0` / `0055ACF0` / `0055AD60` / `0055AE88` /
`0055AF60` / `0055B9D0` / `0055B9F0` / `00557AF0`);
`e8.tsv`; `listing-00400000.txt` (`0042E3EE`);
`implementer/frontend/17-press-start-frame.txt`;
`proofs/0055A726-plus228-jmp/README.md`;
`proofs/00557AF0-caller/README.md`;
`proofs/type13-vs-type4/README.md`;
`proofs/type6-action28/README.md`;
`proofs/action27-release/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not re-prove persist CRC `0x53C644E4` → def `+228` /
`0055B040` → `widget+380`. Do not treat action **35** (MMB /
`0042E3EE`) as widget type **35**. `.rdata` slot dwords were
**not** printed this pass.

---

## Verdict

**`0055A726 jmp 0055ACF0` is not inside a function that starts
at `0055A640`.** `0055A640` is `push ecx` in the middle of
**`0055A630`** (`ret 12`; calls `0055B9F0` then `vtbl+612`).
INT3 pad `0055A657`–`0055A65F` ends that body.

The jmp sits in **`0055A660`** (next prologue `push ecx` /
`push ebp` / `mov ebp, ecx`). Twin tail `0055A73B jmp 0055ACF0`
is the same function. Next frame is `0055A740`.

`0055A660` is the type-**35** (`0055A9C0`, outer `0124BA94`)
override of the type-34 unarm slot. Shared apply `0055AD60`
action **28** is `call [outer.vtbl+588]`. That is LMB-up
(`0042E3EE` type 6 → `push 28`), after action 26 armed
`[+364]` and `0055AF60` locally mapped 28.

**First-seen Press Start / New Profile / Main Menu:** this jmp
does **not** run. Those trees have type **11** / **38**, not
type 35. Type 11/38 `+588` (if ever armed) is the shared
**`0055ACF0`** body, not this slider wrap.

**Action 25 / mouse move:** sibling, not this hop. Type 13 →
`push 25`. Type-35 inner `0055A510` special-cases 25 (thumb
math `vtbl+128`) then always `E8 0055AD60`. Action 25 is
outside the `0x55AE88` 26–32 table, so it is only
`0055B9D0` → `vtbl+580`. It never reaches `vtbl+588` /
`0055A726`.

| Claim | Status |
| --- | --- |
| `0055A640` is a function start / owner of `0055A726` | **DISPROVEN** — insn in `0055A630` |
| `0055A630` is 3-arg (`ret 12`); `E8 0055B9F0` then `[vtbl+612]` | **PROVEN** |
| `0055A726` / `0055A73B` live in `0055A660` | **PROVEN** |
| Both are tail-`jmp 0055ACF0` (not `E8`; frame already torn down) | **PROVEN** |
| `e8.tsv` dest `0055ACF0` is only `00557AF4` | **PROVEN** |
| Any `.text` `E8` / `jmp` to `0055A660` | **DISPROVEN** (`e8.tsv` empty; listing has none) |
| `0055ACF0` posts `[this+380]` through `vtbl+524`; unmaps 28 | **PROVEN** |
| `0055A9C0` is factory type 35; vtbl `0124BA94` / inner `0124BA70` | **PROVEN** |
| `0055A5D0` wraps `0055AF60` → type-35 **`vtbl+584`** (action 26) | **PROVEN** ABI; rdata **PARTIAL** |
| `0055A660` is type-35 **`vtbl+588`** (action 28) | **PROVEN** ABI / unique wrap of `0055ACF0`; rdata **PARTIAL** |
| Type-11/38 `+588` is **`0055A660`** / this jmp | **DISPROVEN** — no type-35 fields; base body is `0055ACF0` **PARTIAL** rdata |
| First-seen Press Start / New Profile / Main Menu runs `0055A660` | **DISPROVEN** (no type 35/41) |
| First-seen type 11/38 action 25 reaches `0055A726` | **DISPROVEN** |
| Action 25 is type-13 mouse move (`0042E5DC` `push 25`) | **PROVEN** (`type13-vs-type4`) |
| Type-35 apply `0055A510` `cmp edi,25` is slider thumb, then `0055AD60` | **PROVEN** |
| `0055B9D0` action==25 → outer `vtbl+580`; else `ret 4` | **PROVEN** |
| `0124BA94+588` dword = `0055A660` | **PARTIAL** (no `.rdata` listing) |

**Answer:** function **`0055A660`**, type-35 outer **`vtbl+588`**,
action **28** (LMB-up). Not `0055A640`. Not type 11/38 first-seen
menus. Not mouse-move action **25**.

---

## 1. The asked window (`listing-00540000.txt` `0055A640`–`0055A740`)

Three bodies touch this range. Only the middle one contains the
jmp.

```
0055A630  mov eax, [esp+12]
          mov edx, [esp+4]
          push esi
          mov esi, ecx
          mov ecx, [esp+12]
          push eax
0055A640  push ecx                  ; NOT a prologue
          push edx
          mov ecx, esi
          call 0055B9F0
          call [esi.vtbl+612]
          pop esi
0055A654  ret 12
0055A657  int3 … 0055A65F int3

0055A660  push ecx                  ; THIS function
          push ebp
          mov ebp, ecx
          …
0055A726  jmp 0055ACF0
          …
0055A73B  jmp 0055ACF0

0055A740  push ecx                  ; next function
          push ebp
          call 0055AEF0             ; type-34 disable (unmap 26/31/27/32)
          … jmp [input.vtbl+604]
```

`0055A640` is the second of three argument pushes into
`0055B9F0`. Sibling `0055B9F0` is the same 3-arg shape
(`ret 12`) with `E8 0052E9C0` then `vtbl+580` instead of
`+612`. Neither body has a `jmp` / `E8` to `0055ACF0`.

No `.text` `call 0055A630` / `jmp 0055A630` in this listing.
Slot for `0055A630` stays **PARTIAL**.

---

## 2. `0055A660` tails into `0055ACF0`

```
0055A660  push ecx
0055A661  push ebp
0055A662  mov ebp, ecx              ; outer widget
0055A664  mov al, [ebp+412]         ; drag latch (set by 0055A5D0)
          test al, al
          je  0055A6B9
          ; while dragging:
          ;   [input+188] = this
          ;   vtbl+524([this+416])
          ;   push 30 / input.vtbl+0
          ;   if [input+188] still set:
          ;     vtbl+524([this+420])
0055A6B9  call 0041E5F2
          mov ecx, [eax+184]
          test ecx, ecx
          je  0055A735              ; no manager → tail
          ; walk [0x13B8AD4] widgets
          ; vtbl+260 == 35 or 41 → bl=1
0055A714  jne 0055A72B              ; another slider lives
0055A716  call [ecx.vtbl+604]
          pop edi / ebx
          mov ecx, ebp
          pop ebp
          add esp, 4
0055A726  jmp 0055ACF0              ; THIS jmp
0055A72B  call [ecx.vtbl+596]
          pop edi / ebx
0055A735  mov ecx, ebp
          pop ebp
          add esp, 4
0055A73B  jmp 0055ACF0              ; and THIS jmp
```

Both paths restore `ecx = this` after `pop ebp` / `add esp, 4`.
The `jmp` is a tail-call: `0055ACF0` `ret` returns to
`0055A660`’s caller. There is no `E8 0055ACF0` in this
function.

`e8.tsv` dest `0x0055ACF0`: only `0x00557AF4` (type-39
`00557AF0`). Dest `0x0055A660`: **empty**. Listing has
`call 0055A660` / `jmp 0055A660`: **none**. Live entry is
`call [this.vtbl+k]`.

```
0055ACF0  push esi
          mov esi, ecx
          push [esi+364]
          call [vtbl+192]           ; SelectState(armed flag)
          lea ecx, [esi+4]
          push 28
          call [inner.vtbl+16]      ; unmap local action 28
          push [esi+380]
          call [this.vtbl+524]      ; walk +228 list
          ret
```

---

## 3. Vtbl slot: type-35 `+588`, not type 11/38 click

Shared inner apply `0055AD60` (`ecx` = `widget+4`):

```
lea eax, [edi-26]
cmp eax, 6
ja  0055AE79                    ; 0055B9D0 only
jmp [0x55AE88+eax*4]
```

Table dwords (`action27-release`; do **not** use code-order):

| Action | Site | Outer call |
| ---: | --- | --- |
| **26** | `0055AD7B` | `vtbl+584` then `[inner+364]=1` |
| **27** | `0055AE01` | `vtbl+592` hover-in |
| **28** | `0055ADDE` | if armed: **`vtbl+588`**, `[+364]=0` |
| **29** | `0055AE53` | `vtbl+596` hover-out |

```
0055ADDE  test [esi+364]
          je  0055AE70
          lea ecx, [esi-4]
          call [outer.vtbl+588]     ; 0-arg
          mov [esi+364], 0
```

`0042E3EE` type **6** (LMB up) is `push 28`. Action 26’s
`0055AF60` is what inserts 28 into the inner local map
(`vtbl+12`). Type 35 inner apply `0055A510` always ends
`push edi; call 0055AD60`, so armed action 28 reaches this
object’s `+588`.

Type-35 0-arg cluster next to the ctor:

| VA | Shape | Slot |
| --- | --- | --- |
| `0055A5D0` | `call 0055AF60` then latch `+412` / `jmp [input+184].vtbl+600` | **`+584`** (action 26) |
| **`0055A660`** | slider teardown then **`jmp 0055ACF0`** | **`+588`** (action 28) |
| `0055A5B0` | `0055AEB0` then `jmp [+184].vtbl+596` | enable |
| `0055A740` | `0055AEF0` then `jmp [+184].vtbl+604` | disable |

`0055A5D0` is the only wrap of the proven `+584` body.
`0055A660` is the only wrap of the `+228` poster. Same
inheritance pattern as type 40 (`00557850 jmp 0055AF60`).

Expected rdata (unread): `0124BA94+588` = `0055A660`.
Type-34 `0124BD2C+588` / type-38 `0124B04C+588` /
type-11 `01249554+588` should be **`0055ACF0`**, not
`0055A660`. **PARTIAL**.

Factory `0055A9C0` (type 35, size `0x1AC`):

```
0055A9C8  call 0055B460             ; type 34
          mov [esi],     0x124BA94  ; outer
          mov [esi+4],   0x124BA70  ; inner
          mov [esi+24],  0x124BA68
          ; +404..+425 = 0; +424 = 1
          call 0055A890
```

Type 11 ctor `0054E0B0` overwrites to `01249554`. Type 38
`00558B90` overwrites to `0124B04C`. Neither installs
`0124BA94` or the `+412` drag latch `0055A660` tests.
RTTI `CSlider@NUISystem` (`0137C000`) sits in this family;
COL → `0124BA94` was **not** dumped. Name **PARTIAL**.

---

## 4. First-seen type 11/38 menus: never this jmp

Press Start dump (`17-press-start-frame.txt`):

| Widget | Type |
| --- | ---: |
| `UI_FRONTEND_PRESS_START_MENU` | 10 |
| forest / title sprites | 5 / 18 / 0 |
| `UI_PRESS_START_TEXT` | 6 |
| `UI_FRONTEND_LIST_PRESS_START_MENU` | 12 |
| `UI_FRONTEND_BUTTON_INVISIBLE` | **11** |
| `UI_MOUSE_POINTER` | 32 |

**No type 35 / 38 / 39 / 41.** Prompt is
`TEXT_GUI_MENU_PRESS_BUTTON`, not a slider.

New Profile: type 10 root, type 12 list, type **38** Accept,
type 37 edit, type 11 helpers. Main Menu: type 10 / 12 /
type-11 `UI_FRONTEND_BUTTON_NEW_GAME`. **No type 35.**

So **`0055A726` does not execute** on those screens.

What those type 11/38 objects *do* on mouse:

| Input | Action | Type 11/38 slot | Reaches `0055A726`? |
| --- | ---: | --- | --- |
| LMB down (type 4) | 26 | `vtbl+584` → `0055AF60` (`+372`) | **no** |
| LMB up (type 6) | 28 | `vtbl+588` → **`0055ACF0`** if `[+364]` | **no** (no wrap) |
| mouse move (type 13) | 25 | `0055B9D0` → `vtbl+580` | **no** |

First-seen `[+364]` is ctor zero. Unarmed action 28 takes
`0055ADE6 je 0055AE70` and never calls `+588`. After a
successful click, type 11/38 would enter **`0055ACF0`
directly**, still not `0055A660`.

The other `.text` `E8` into `0055ACF0` (`00557AF4`) is
type-39 `CKeyRedefiner` (`00557AF0-caller`). Also absent
from these trees.

---

## 5. Relation to mouse / action 25

Physical mouse on this cluster:

| Event | `0042E3EE` | Action | This window |
| --- | --- | ---: | --- |
| LMB down | type 4 `push 26` | 26 | `0055A5D0` (`+584`), **not** `0055A726` |
| LMB up | type 6 `push 28` | 28 | **`0055A660` / `0055A726`** once armed |
| motion | type 13 store `+176/+180` `push 25` | 25 | `0055A510` / `0055B9D0` / `0055A7C0` |

`0055A510` (type-35 inner, `ret 4`):

```
0055A515  mov edi, [esp+36]         ; action
0055A519  cmp edi, 25
          jne 0055A59D
          ; if [esi+408]: cursor vs thumb → [outer.vtbl+128]
0055A59D  push edi
          mov ecx, esi
          call 0055AD60
          ret 4
```

Action 25 therefore does extra slider math, then still
enters `0055AD60`. `lea eax,[edi-26]` / `cmp eax,6` /
`ja 0055AE79` sends 25 to **`0055B9D0` only**:

```
0055B9D0  cmp [esp+4], 25
          jne  ret 4
          call [outer.vtbl+580]
```

No `vtbl+588`. No `jmp 0055ACF0`.

Nearby `0055A7C0` (`push 25` / `call [inner.vtbl+8]`) is a
**query** (“is 25 locally mapped?”) used for slider focus
vs `input.vtbl+600/+596/+604`. It is not apply and not
this jmp.

So action 25 is how a type-35 **tracks the pointer while
dragged**. The `0055A726` hop is how that same object
**unarms on LMB-up** and posts `[+380]`. First-seen type
11/38 never take either slider-only path.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `0055A640` | `push ecx` inside `0055A630` | **PROVEN** not a fn |
| `0055A630` | 3-arg wrap of `0055B9F0` + `vtbl+612` | **PROVEN** body; slot **PARTIAL** |
| `0055A660` | type-35 0-arg unarm; tails to `0055ACF0` | **PROVEN** body; `+588` rdata **PARTIAL** |
| `0055A726` / `0055A73B` | epilogue `jmp 0055ACF0` | **PROVEN** |
| `0055ACF0` | `vtbl+192([+364])`; unmap 28; `vtbl+524([+380])` | **PROVEN** |
| `0055A510` | type-35 inner apply; 25 then `0055AD60` | **PROVEN** |
| `0055A5D0` | type-35 click wrap of `0055AF60` | **PROVEN** body; `+584` rdata **PARTIAL** |
| `0055A740` | type-35 disable wrap of `0055AEF0` | **PROVEN** |
| `0055A7C0` | query inner `vtbl+8(25)` | **PROVEN** body |
| `0055A9C0` | type 35 ctor | **PROVEN** |
| `0055ADDE` | action 28 → `vtbl+588` | **PROVEN** |
| `0055B9D0` | action==25 → `vtbl+580` | **PROVEN**; **DISPROVEN** as this jmp |
| `00557AF0` | other `0055ACF0` caller (redefiner) | **PROVEN**; first-seen menus **DISPROVEN** |

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
  (`0055A510`, `0055A5B0`, `0055A5D0`, `0055A630`, `0055A660`,
  `0055A740`, `0055A7C0`, `0055A9C0`, `0055ACF0`, `0055AD60`,
  `0055AE88`, `0055AF60`, `0055B9D0`, `0055B9F0`, `00557AF0`)
- `tools/Fable.ExeIndex/out/01-sections/text-map/e8.tsv`
  (`0x00557AF4 → 0x0055ACF0`; no dest `0055A660`)
- `listing-00400000.txt` (`0042E3EE` type 6 / 13)
- `tools/Fable.ExeIndex/out/00-index/rtti.txt` (`CSlider@NUISystem`)
- `implementer/frontend/17-press-start-frame.txt`
- `proofs/0055A726-plus228-jmp/README.md`
- `proofs/00557AF0-caller/README.md`
- `proofs/type13-vs-type4/README.md`
- `proofs/type6-action28/README.md`
- `proofs/action27-release/README.md`
