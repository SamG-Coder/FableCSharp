# `0055A726` / `0055A73B` `jmp 0055ACF0` is type-35 `vtbl+588` (action 28)

Investigation only. No production `src/` edits.

Authority: `Fable.exe` listing
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0055A510` / `0055A5D0` / `0055A660` / `0055A9C0` / `0055ACF0` /
`0055AD60` / `0055AE88` / `0055AF60` / `00557AF0` / `0057A535`);
`listing-00400000.txt` (`0041D21B` / `0042E3EE`);
`implementer/frontend/01-widget-construction.md`,
`17-press-start-frame.txt`;
`proofs/plus224-payloads/README.md`,
`proofs/0055B9D0-post-dword/README.md`,
`proofs/0055AF60-callee/README.md`,
`proofs/action27-release/README.md`,
`proofs/type6-action28/README.md`,
`proofs/type7-action35/README.md`,
`proofs/messageid-plus228/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**STALE**.

Do not re-prove persist CRC `0x230364D6` → def `+224` /
`0x53C644E4` → def `+228`. Do not treat action **35** (MMB /
`0042E3EE`) as widget type **35**.

`.rdata` slot dwords were **not** printed this pass. Callees are
from ABI + unique bodies. Vtbl **pointers** stay **PARTIAL**.

---

## Verdict

**`0055A726` and `0055A73B` are not their own functions.** They
are the two tail-`jmp`s at the end of **`0055A660`**. Both go to
**`0055ACF0`**, the 0-arg body that `push [this+380]` /
`call [vtbl+524]` — persist list **`[def+228]`**.

`0055A660` is the type-**35** (`0055A9C0`, outer vtbl
`0124BA94`) override of the type-34 unarm slot. Shared
`0055AD60` action **28** is `call [outer.vtbl+588]`. That is
LMB-up (`0042E3EE` type 6 → `push 28`), after action 26 armed
`[+364]` and `0055AF60` locally mapped 28.

| Claim | Status |
| --- | --- |
| `0055A726` / `0055A73B` live in `0055A660` | **PROVEN** |
| Both are `jmp 0055ACF0` (not `E8`; stack already popped) | **PROVEN** |
| `0055ACF0` posts `[this+380]` through `vtbl+524` | **PROVEN** |
| That list is ctor `0055B040` arm `[def+228]` / `vtbl+320` | **PROVEN** store; slot **PARTIAL** |
| `0055A9C0` is factory type 35; vtbl `0124BA94` / inner `0124BA70` | **PROVEN** |
| Type 35 extends type 34 (`call 0055B460` then overwrite) | **PROVEN** |
| `0055A5D0` wraps `0055AF60` → type-35 **`vtbl+584`** (action 26) | **PROVEN** ABI / unique wrap; rdata **PARTIAL** |
| `0055A660` is type-35 **`vtbl+588`** (action 28) | **PROVEN** ABI / unique wrap of `0055ACF0`; rdata **PARTIAL** |
| Type-34/38 `+588` body is **`0055ACF0`** itself | **PARTIAL** (same 0-arg unmap-28 + `+380` shape; no rdata) |
| Action 26 / `0055AF60` posts `+228` / `+380` | **DISPROVEN** (`plus224-payloads`) |
| First-seen Press Start / New Profile / Main Menu runs `0055A660` | **DISPROVEN** (no type 35/41 in those trees) |
| First-seen after Leave constructs a type-35 and hits this jmp | **UNREAD** |
| RTTI `CSlider@NUISystem` (`0137C000`) == type 35 | **PARTIAL** (family + slider fields; no COL dump) |

**Answer:** function **`0055A660`**, type-35 outer **`vtbl+588`**,
action **28**. The `+228` post is the tail `0055ACF0`. Not on
the first-seen frontend screens. Not action 26. Not action 35.

---

## 1. The two jmps are the epilogue of `0055A660`

`listing-00540000.txt`. Function start is the INT3-padded
prologue at `0055A660` (`push ecx` / `push ebp` / `mov ebp, ecx`).
Next frame is `0055A740`.

```
0055A660  push ecx
0055A661  push ebp
0055A662  mov ebp, ecx              ; outer widget
0055A664  mov al, [ebp+412]         ; drag latch
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
0055A720  mov ecx, ebp
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

Both paths restore `ecx = this` and have already
`pop ebp` / `add esp, 4`. The `jmp` is a tail-call: `0055ACF0`
`ret` returns to `0055A660`’s caller. There is no `E8 0055ACF0`
in this function.

`plus224-payloads` already listed these two sites as callers
of `0055ACF0`. They are not a third function.

---

## 2. `0055ACF0` is the `+228` / `+380` poster

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

Pair with click `0055AF60` (`vtbl584-post-hop` / `0055AF60-callee`):

| Body | List | Local 28 | Slot |
| --- | --- | --- | --- |
| `0055AF60` | `push [this+372]` (`[def+224]`) | inner **`vtbl+12(28)`** map | type-34 **`+584`** |
| `0055ACF0` | `push [this+380]` (`[def+228]`) | inner **`vtbl+16(28)`** unmap | type-34 **`+588`** (PARTIAL) |

`0055B040` second persist arm is `[def+228]` → `vtbl+320` →
appender `0055B5B0` writes **`widget+380`**. First arm
`[def+224]` → `vtbl+284` → `0055B520` writes `+372`.
`0055AF60` never loads `+380`.

On Accept / New Game / INVISIBLE, file `+224` is **0** and
`+228` holds `0x126` / 15 / `0xE5` (`plus224-payloads`).
Action 26 therefore posts nothing. `0055ACF0` is the body
that would deliver those ids.

`00558DE0` is the unique 1-arg list walker for `vtbl+524`
(rdata **PARTIAL**). Frontend `[0x13B86A0]==0` then
`0041E6D3` → UI `vtbl+32` `0059A238`.

---

## 3. Which action / which vtbl slot

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

`0042E3EE` type **6** (LMB up) is `push 28` (`type6-action28`).
Action 26’s `0055AF60` is what inserts 28 into the inner
local map (`vtbl+12`). Enable `0055AEB0` is 26/31/27/32 —
**not** 28 — so 28 only applies after a successful click.

Type 35 inner apply `0055A510` always ends
`push edi; call 0055AD60`. Action 28 therefore reaches
`vtbl+588` on this object once armed.

Type-35 0-arg cluster next to the ctor:

| VA | Shape | Slot |
| --- | --- | --- |
| `0055A5D0` | `call 0055AF60` then latch `+412` / `jmp [input+184].vtbl+600` | **`+584`** (action 26) |
| **`0055A660`** | slider teardown then **`jmp 0055ACF0`** | **`+588`** (action 28) |
| `0055A5B0` | `0055AEB0` then `jmp [+184].vtbl+596` | not this hop |
| `0055A740` | `0055AEF0` unsubscribe then `jmp [+184].vtbl+604` | not this hop |

`0055A5D0` is the only wrap of the proven `+584` body.
`0055A660` is the only wrap of the `+228` poster. That is
the same inheritance pattern as type 40
(`00557850 jmp 0055AF60`).

`type6-action28` “`vtbl+588` posts no UI message” is
**STALE** if `+588` is `0055ACF0` / `0055A660`: those
bodies **do** `vtbl+524([+380])`. First-seen type 11/38
still skip the call when `[+364]==0` (ctor zero; no prior
26). After a click they do not.

Rdata: `0124BA94+588` = VA `0124BCE0` should read
`0055A660`. `0124BD2C+588` / `0124B04C+588` should read
`0055ACF0` (or a 5-byte `jmp` to it). **PARTIAL**.

---

## 4. What `0055A660` is (type 35, not action 35)

Factory `0041D21B` type 35 (`01-widget-construction`):

```
0041D2DC  push 0x1AC
          call 00BFEA1A
          call 0055A9C0
```

```
0055A9C0  call 0055B460             ; type 34
          mov [esi],     0x124BA94  ; outer
          mov [esi+4],   0x124BA70  ; inner
          mov [esi+24],  0x124BA68
          ; +404..+425 = 0; +424 = 1
          call 0055A890
```

Type 41 ctor `00559830` calls `0055A9C0` then overwrites
to `0124B7E4`. `0055A660` treats live type **35 or 41** as
the same family (`vtbl+260` `cmp eax, 35` / `cmp eax, 41`).

`0057A535` is `push 35; pop eax; ret` — widget-type getter,
not the MMB action. Action 35 is `0042E3EE` type 7
(`type7-action35`). Different 35.

Slider-shaped extras vs type 34:

- `+412` drag latch (set in `0055A5D0`, tested here)
- `+416` / `+420` extra lists posted while dragging
- action **25** path in `0055A510` (`vtbl+128` thumb math)
  then `0055B9D0` → outer `vtbl+580`
- `push 30` into input `vtbl+0` during the drag tail

RTTI `CSlider@NUISystem` / `CTextSlider@NUISystem` sit next
to each other (`0137C000` / `0137BF80`). COL → `0124BA94`
was **not** dumped. Name **PARTIAL**.

---

## 5. First-seen: not frontend Leave path

Other `.text` `E8` to `0055ACF0`: **`00557AF4`** inside
`00557AF0` (`CKeyRedefiner` + `TEXT_GUI_PRESS_CONTROL` +
subscribe 35). That is the options remapper
(`type7-action35`). **Not** Press Start / New Profile /
Main Menu.

First-seen trees have **no type 35/41**:

| Screen | Types recovered | Type 35? |
| --- | --- | --- |
| Press Start | 10, 5, 6, 18, 12, 11, 32 (`17-press-start-frame.txt`) | **no** |
| New Profile | 10 root, 12, 38 accept, 37 edit, 11 helpers | **no** |
| Main Menu | 10 / 12 / type-11 `UI_FRONTEND_BUTTON_NEW_GAME` | **no** |

Main Menu `00595B24` only **names** Options / Redefine Keys
as list slots. It does not construct `PC_SLIDER_*` /
`UI_OPTIONS_SLIDER_*` / a redefiner on the New Game click
path. Leave is msg 15 → `0042F2A2`.

So **`0055A726` / `0055A73B` do not run** on first-seen
frontend or on the no-save Leave hop.

After Leave: in-game / pause `UI_LEVEL_SLIDER` /
`PC_TICK_SLIDER` / options sliders would be the natural
type-35 sites. No listing walk in this pass proves a
first `0055A9C0` after `0042F2A2`. **UNREAD**.

If the question was “when does `0055ACF0` first fire on
Accept / New Game”: that is **not** these jmps. It would
be type-38 `vtbl+588` on the LMB-up **after** action 26
(rdata **PARTIAL**). Host `MessageFromWidgets` posting
`+228` on action **26** stays **LEFTOVER** vs native 26.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `0055A660` | type-35 0-arg unarm; tails to `0055ACF0` | **PROVEN** body; `+588` rdata **PARTIAL** |
| `0055A726` / `0055A73B` | epilogue `jmp 0055ACF0` | **PROVEN** |
| `0055ACF0` | `vtbl+192([+364])`; unmap 28; `vtbl+524([+380])` | **PROVEN** |
| `0055A5D0` | type-35 click wrap of `0055AF60` | **PROVEN** body; `+584` rdata **PARTIAL** |
| `0055A510` | type-35 inner apply; 25 then `0055AD60` | **PROVEN** body; inner `+4` **PARTIAL** |
| `0055A9C0` | type 35 ctor | **PROVEN** |
| `0055ADDE` | action 28 → `vtbl+588` | **PROVEN** |
| `00557AF0` | other `0055ACF0` caller (redefiner) | **PROVEN**; first-seen menus **DISPROVEN** |
| `0124BA94+588` | expected dword `0055A660` | **PARTIAL** |

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
  (`0055A510`, `0055A5B0`, `0055A5D0`, `0055A660`, `0055A740`,
  `0055A9C0`, `0055ACF0`, `0055AD60`, `0055AE88`, `0055AF60`,
  `0055B040`, `0055B460`, `00557AF0`, `0057A535`)
- `listing-00400000.txt` (`0041D21B` type 35, `0042E3EE` type 6)
- `implementer/frontend/01-widget-construction.md`
- `implementer/frontend/17-press-start-frame.txt`
- `proofs/plus224-payloads/README.md`
- `proofs/action27-release/README.md`
- `proofs/type6-action28/README.md`
- `proofs/type7-action35/README.md`
