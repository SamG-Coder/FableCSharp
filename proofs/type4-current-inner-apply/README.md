# Type4 apply is current-inner `0055CB10`, not dest AABB

Investigation only. No production `src/` or `tests/` edits.

Question: native Type4 apply uses **current-inner** widget or
**dest AABB** hit? Recover leftover vs MATCH.

Authority: dump `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`0042E3EE` / `0042E4A4` / `0042E5AB`),
`listing-00540000.txt`
(`0055CB10` / `0054DBC0` / `0055AD60` / `0054E280` /
`0055ACB0` / `0055B890` / `0055BF10` / `0055B8F0` /
`0055B9D0` / `0055BA20`),
`listing-00500000.txt` (`0052D900` / `0052DA20` / `005334A0`);
`e8.tsv` (no `.text` `E8 0055BF10` / `E8 0055B8F0`);
`proofs/type4-enqueue-ring/README.md`,
`proofs/type4-type6-ring/README.md`,
`proofs/action26-subscribers/README.md`,
`proofs/type11-plus352-select/README.md`,
`proofs/leftover-14-present-dest/README.md`,
`src/Fable.Game/EngineLifecycle.cs`
(`TickType11Type38Hover` / `ArmType34Widgets` /
`MaybeActivateNewGameFromInput`),
`src/Fable.Game/FrontendInputMap.cs`,
`src/Fable.Game/FrontendHitTest.cs`,
`src/Fable.Client/SilkNativeInput.cs`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Do not re-prove type 4 = LMB down → action 26, type 6 =
LMB up → action 28, or the 52-byte enqueue ring
(`type4-enqueue-ring` / `type4-type6-ring`).
Do **not** invent WASD.

---

## Verdict

**Current-inner.** Type4 apply is `0055CB10(26)` on
registered **inner** objects (`widget+4`). It does **not**
hit-test dest AABB.

Dest AABB is hover `0055BF10` → `vtbl+568` `0055B8F0`.
That tick writes type-11/38 `+352` u8. Type4 **reads**
the byte. Hover **writes** it. They are not the same
site.

Host `TickType11Type38Hover` always dest AABB, from the
pointer, **before** Type4. That hover-from-pointer-before-Type4
order is **MATCH** player LMB (pointer dest known before
apply). Host Type4 apply itself (`ArmType34Widgets`
`HitIndex` dest AABB) is **LEFTOVER**.

| Claim | Status |
| --- | --- |
| `0042E3EE` type 4 → `push 26` / `input.vtbl+0` = `0055CB10` | **PROVEN** |
| `0055CB10` walks current inners (`input+8` else `+12` else `+4`) | **PROVEN** |
| Apply is inner `vtbl+4`; accept is inner `vtbl+8` | **PROVEN** |
| `0055CB10` calls `0055B8F0` / dest AABB | **DISPROVEN** (no call; no `E8`) |
| Type 11 inner apply `0054DBC0` dest AABB | **DISPROVEN** — parent `+545` then `0055AD60` |
| Type 38 inner apply `0055AD60` dest AABB | **DISPROVEN** — `u8 [inner+348]` then `vtbl+584` |
| Type-10 inner apply `0054E2FA` dest AABB | **DISPROVEN** — posts `&widget+352` packet* |
| Dest AABB is hover `0055BF10` / `0055B8F0` | **PROVEN** |
| Hover writes type-11/38 `+352=1` (`0055C0DE`) | **PROVEN** |
| Type4 reads that byte; zero skips `0055AF60` | **PROVEN** |
| Hover is TypeMouse-only | **DISPROVEN** — tick `0055ACB0` + optional `input+184` |
| Host `TickType11Type38Hover` dest AABB from pointer | **MATCH** hover |
| Host hover before Type4 | **MATCH** player LMB |
| Host `ArmType34Widgets` dest AABB at Type4 apply | **LEFTOVER** |
| Host `MessageFromWidgets` first visible type-10 | **LEFTOVER** vs current-inner |
| Type-4 **widget** inner (`01246064`) is a `0055CB10` node | **DISPROVEN** — ctor `005334A0` does not register |
| WASD / actions 0–5 as Type4 apply | **DISPROVEN** — do not invent |

**Answer:** native Type4 apply uses the **current-inner**
widget on the `0055CB10` list. Dest AABB is the **hover**
gate, not the apply.

---

## 1. Evidence — Type4 → `0055CB10(26)` current-inner

`listing-00400000.txt`. Type from `00A03B40` (`[record+40]`).
`dec; sub 3; je` is type 4:

```
0042E479  dec eax
0042E47C  sub eax, 3
0042E47F  je  0042E4A4              ; type == 4
0042E4A4  call 0041E5F2             ; input singleton
          push 26
          jmp 0042E5AB
0042E5AB  mov edx, [eax]
          mov ecx, eax
          call [edx]                ; input.vtbl+0 = 0055CB10
```

No dest, no pointer XY, no widget walk. Action is the
immediate `push 26`. Enqueue of that type-4 record is
already **PROVEN** (`type4-enqueue-ring`); consume is
`009F4ED0` then `009F4F10` (`type4-type6-ring`). This
proof only needs: **apply is `0055CB10`, not AABB**.

`listing-00540000.txt` entire `0055CB10` (`ret 4` / INT3):

```
0055CB10  esi = ecx                 ; input
          eax = [esi+8]             ; focused inner*
          test eax
          je  broadcast
          ; exclusive:
          inner.vtbl+8(action)      ; accept
          je  return                ; still exclusive
          inner.vtbl+4(action)      ; apply
          ret 4
broadcast:
          if [esi+12] list nonempty:
            0055CF50 copy +12
            for node in copy:
              inner = [node+8]
              if inner.vtbl+8(action):
                inner.vtbl+4(action)
          else:
            same walk of [esi+4]
```

No `0055B8F0`. No `vtbl+568`. No dest origin/size.
`e8.tsv` has **zero** `E8 0055B8F0` / `E8 0055BF10`.
The listener is `widget+4` (`action26-subscribers`).

First-seen `[input+8]==0` → **broadcast** of whoever
`input.vtbl+8(inner)` registered. Type 11/38 **PROVEN**
(`0055BA20`). Type-10 ctor does **not** register
(**UNREAD** as a first-seen node). Widget **type 4**
ctor `005334A0` writes inner vtbl `01246064` and
`0052D9E0`s the local action set; it does **not**
`0041E5F2` + `vtbl+8`. Type-4 **dtor** `00532D90`
unregisters `this+4` — analog only. Type4 **event**
apply is not type-4 **widget** apply.

Accept (`inner.vtbl+8`) consults the local action set
(`0052D900` contains on `inner+4` BST). That is “is 26
mapped?”, not dest AABB. Map insert is `0052DA20`
(`type11-subscribe-actions`).

---

## 2. Evidence — type 11/38 / 10 inner apply (no AABB)

### Type 11 `0054DBC0` (`ecx` = inner)

```
0054DBC0  debounce [inner+44] vs +400 / +392
          parent = [inner-4].vtbl+432
          bl = [parent+545]
          if bl: 0055AD60(action)
          ret 4
```

No dest. No pointer. Parent `+545` is the only extra
gate (`cuidef-plus545`).

### Type 38 / armed type 11 `0055AD60`

```
0055AD66  lea eax, [edi-26]
          cmp eax, 6
          jmp [0x55AE88+eax*4]      ; actions 26–32
0055AD7B  mov al, [esi+348]         ; widget+352 u8
          test al, al
          je  0055AE3D              ; 0055B9D0 only
          lea ecx, [esi-4]
          call [eax+584]            ; 0055AF60
          [esi+364] = 1             ; arm
          call 0055B9D0             ; action 26 → ret 4
```

`0055B9D0` is `cmp arg, 25; jne ret; call [outer.vtbl+580]`.
Action **26** is the `jne`. Hover `0055BF10` is **not**
this apply.

Zero `+352` → no `0055AF60`, no persist 15 / `0x126` on
26. The byte is the **selected** flag
(`type11-plus352-select`), not dest AABB at apply time.

### Type 10 `0054E280` action 26 = `0054E2FA`

```
0054E2FA  mov eax, [edi+348]        ; widget+352 packet*
          test eax, eax
          je  skip
          00595582
          UI.vtbl+32(&inner+348)    ; 0059A238
```

Press Start attach packet `0xE5`. Still no dest AABB.

---

## 3. Evidence — dest AABB is hover, before Type4

Type-34 **tick** `0055ACB0` (outer `vtbl+4`, not inner
apply):

```
0055ACB0  mov al, [ecx+352]
          … maybe clear +368 / +388 …
          jmp 0055B890
0055B890  call 0052C7E0             ; dt
          ; dest w/h ~0 or dt changed:
          call [vtbl+580]           ; 0055BF10
```

`0055BF10` (`listing-00540000.txt`):

```
0055BF19  call 0041E5F2
          test [input+164]; jne leave
          inner.vtbl+8(25); je leave
          ; if [input+184] type-32 pointer present:
          ;   vtbl+64 / +92 → point in [esp+32]
0055C00C  mov al, [esi+352]
          test al, al
          jne already
          call [vtbl+568]           ; 0055B8F0 dest AABB
          je  fail
          call 0055BB40             ; lose to 0x13B8AD4 peer
          …
0055C0DE  mov [esi+352], 0x01
```

`0055B8F0` AABB: `vtbl+488` origin + `vtbl+492` size +
`vtbl+96`. Contains `[left, right) × [top, bot)`. Point
dest (size 0) → `al=0` → `+352` stays ctor 0.

Hover is **not** TypeMouse-only: tick always reaches
`0055BF10`; `input+184` only seeds the test point.
Action 25 (type 13) also reaches `vtbl+580` via
`0055B9D0`. Type 13 in the same `0042E3EE` harvest is
FIFO **before** a later type 4 (`type4-type6-ring` walk
shape). Click with no move uses the **prior** tick’s
`+352` from the type-32 pointer dest.

Hover **writes** `+352`. Type4 current-inner **reads**
it. Dest AABB is not the apply.

---

## 4. Original (native frontend order)

```
0042EC7C
  0042E3EE
    harvest 009F4ED0 / 009F4F10
    type 13 → 0055CB10(25)     pointer apply; 0055B9D0 → hover
    type  4 → 0055CB10(26)     current-inner apply
    type  6 → 0055CB10(28)     armed +380 post
  0042DC94 / 00599E3F
    [ui+84] vtbl+4 tick
    type 11/38: 0055ACB0 → 0055B890 → 0055BF10 dest AABB
    +352 u8 then the *next* Type4 can arm
```

Press Start Type4: type-10 `0054E2FA` posts attach `0xE5`
**without** dest AABB (if that inner is on the list —
**UNREAD** first-seen). New Profile / Main Menu Type4:
only the current inner whose `+352` is already 1 arms.
Dest AABB ran on a **prior** hover (tick or action 25).

Player-interface `00446330` also dequeues type 4
(`type4-type6-ring`). That walk is not frontend
`0055CB10` and is **not** WASD. Do not invent a
player-move listener here (`0055CB10` player-move
listener stays **UNREAD**).

---

## 5. Host

`PumpFrontendFrame` → `PumpInput` then
`MaybeActivateNewGameFromInput`:

```
TickType11Type38Hover(_frontendWidgets);   ; dest AABB first
foreach Applied:
  ActionType4 → ArmType34Widgets()         ; HitIndex dest AABB
                MessageFromWidgets         ; first type-10 packet
  ActionType6 → MessageFromPlus228List
```

`TickType11Type38Hover` comment dumps `0055ACB0` /
`vtbl+580` `0055BF10` “on dest AABB of the current
pointer. Not TypeMouse-only.” Body: type 11/38
`Hovered = Contains || HitIndex == i`. Always dest AABB.
Always from `FrontendPointerX/Y`. Always **before**
Type4. That order is **MATCH** player LMB.

`ArmType34Widgets`: `HitIndex` dest AABB **again**, then
require `Hovered` before `Armed = true`. Native Type4
apply does not `0055B8F0`. It applies the current inner
and reads `+352`. Host never stores that u8. `Hovered`
is the stand-in. **LEFTOVER**.

`MessageFromType10Attach`: first visible type-10
`Type10Packet` (any Type4, **no dest**, **no inner list**).
Native is `0054E2FA` on a `0055CB10` node. **LEFTOVER**.

`SilkNativeInput.QueuePointer`: set pointer, optional
type 13, then Type4 on LMB edge. Hover-from-pointer
before Type4 is **MATCH**. Live type 13 still **LEFTOVER**
producer (`mouse-pointer-action25`).

Tests: `Main_Menu_Type4_Type6_posts_15_from_current_pointer_without_TypeMouse`
sets the pointer to a dest midpoint then queues Type4.
Empty space `(12,12)` does not Accept. That locks the
host hover-before-apply stand-in, not current-inner
`0055CB10`.

Do **not** invent WASD. Host Type4 is LMB
(`SilkNativeInput`), not keys 0–5 / 20–21.

---

## 6. Gap (Evidence → Original → Host)

```
Evidence              Original                         Host                              Gap
0042E3EE type 4       push 26 / 0055CB10               Queue Type4 / ActionType4         MATCH classify
0055CB10              current inner vtbl+8 then +4     no listener list                  LEFTOVER missing walk
Type 11/38 apply      +352 u8 then arm +364            HitIndex dest AABB + Hovered      LEFTOVER apply=hover dest
Type 10 apply         0054E2FA +352 packet*            first visible Type10Packet        LEFTOVER vs inner node
0055BF10 / 0055B8F0   tick dest AABB writes +352       TickType11Type38Hover dest AABB   MATCH hover site
Hover vs Type4 order  pointer dest known before apply  hover then Type4                  MATCH player LMB
0055B8F0 at apply     no                               ArmType34Widgets HitIndex         LEFTOVER
WASD / 0–5 / 20–21    unread player-move listener      not queued as Type4               DISPROVEN invent
```

| Slice | Native | Host | Class |
| --- | --- | --- | --- |
| Type4 classify | `00A03C80` → `push 26` | `SilkNativeInput` Type4 | **MATCH** |
| Type4 **apply** | `0055CB10` current inner | dest AABB `HitIndex` | **LEFTOVER** |
| Hover dest AABB | tick `0055BF10` / `0055B8F0` | `TickType11Type38Hover` dest AABB | **MATCH** |
| Hover before Type4 | pointer dest / prior +352 | pointer then Type4 | **MATCH** player LMB |
| `+352` u8 | `0055C0DE` | `Hovered` stand-in | **LEFTOVER** |
| Type-10 post | inner `0054E2FA` | first visible packet | **LEFTOVER** |
| WASD | unread | not Type4 | **DISPROVEN** invent |

---

## 7. Do not invent

- Dest AABB as Type4 apply (`0055CB10` has no `0055B8F0`).
- Type-4 **widget** inner as a first-seen `0055CB10` node.
- WASD / actions 0–5 / 20–21 as the Type4 apply target.
- Enter / `Key.N` as Type4.
- Dest 4-tuples (leftover #48). Point dest → empty AABB
  → `+352` stays 0.
- Type-10 on the first-seen list without a register site.

**Proposed (do not apply here):** keep dest AABB on
`TickType11Type38Hover`. Point Type4/Type6 at `0055CB10`
current inners. Store type-11/38 `+352` from hover, not
a second `HitIndex` at apply. Do not invent WASD.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00540000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00500000.txt`
- `C:\FableCSharp\proofs\type4-enqueue-ring\README.md`
- `C:\FableCSharp\proofs\type4-type6-ring\README.md`
- `C:\FableCSharp\proofs\action26-subscribers\README.md`
- `C:\FableCSharp\proofs\type11-plus352-select\README.md`
- `C:\FableCSharp\proofs\leftover-14-present-dest\README.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\src\Fable.Game\FrontendInputMap.cs`
- `C:\FableCSharp\src\Fable.Game\FrontendHitTest.cs`
