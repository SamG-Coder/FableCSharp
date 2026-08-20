# Input type 10 is not MMB; `0042E3EE` → action 27

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `00A03E40` / `00AB4910` / `00AB4BB0` /
`00AB5420` / `0042E3EE` / `0054E280` / `0054DBC0` / `0055AD60` /
`0055CB10` / `00597BF2`; listings
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00a00000.txt`,
`listing-00a80000.txt`, `listing-00400000.txt`,
`listing-00540000.txt`, `listing-00580000.txt`,
`listing-00980000.txt`;
`proofs/type4-dinput-raw/README.md`,
`proofs/type7-action35/README.md`,
`proofs/type10-plus352/README.md`,
`proofs/action26-subscribers/README.md`;
`src/Fable.Game/EngineInput.cs`,
`src/Fable.Game/FrontendInputMap.cs`,
`src/Fable.Formats/Defs/FrontendWidgetType.cs`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **STALE**.

Do not re-prove type 4 → action 26 / `0xE5`, type 13 → action 25,
or `0059A238` consume of `0xE5` / `0x126` / 15.

Two different “type 10”s:

| 10 | Object | VA |
| --- | --- | --- |
| Input record `[+40]=10` | mouse-button event | ctor `00A03E40` |
| CUIDef / persist widget type 10 | Menu container | ctor `0054E3D0`, apply `0054E280` |

---

## Verdict

**Input type 10 is right-mouse down, not middle-mouse.**
`0042E3EE` maps it to action **27**. First-seen Press Start does
**not** post a UI message on 27.

| Claim | Status |
| --- | --- |
| `00A03E40` writes `[record+40]=10`, `[+32]=3` | **PROVEN** |
| Sole `.text` call of `00A03E40` is `00AB554E` inside `00AB5420` | **PROVEN** |
| `00AB5420` raw **2** → `00A03E40` | **PROVEN** (jt[1] `00AB553E`) |
| Raw 2 is DINPUT `dwOfs` 13 `DIMOFS_BUTTON1` down | **PROVEN** `00AB4A8D` |
| Raw 2 is Win32 `+223` / `009A4FE0` (`WM_RBUTTONDOWN`) | **PROVEN** `00AB4F4C` |
| Input type 10 is MMB / `DIMOFS_BUTTON2` / `WM_MBUTTON*` | **DISPROVEN** — that is raw **3** → ctor `00A03D90` type **7** → action **35** |
| `0042E3EE` type 10 → `push 27` → `0055CB10` | **PROVEN** `0042E557` |
| CUIDef widget type 10 is the same 10 | **DISPROVEN** |
| Widget `0054E280` action **26** posts `&widget+352` | **PROVEN** `0054E2FA` (not this event) |
| Widget `0054E280` action **27** is `00597BF2(0)` | **PROVEN** `0054E2B8` |
| `proofs/type7-action35` “action 27 is `0054E2C8` / `00597BF2(1)`” | **STALE** / **DISPROVEN** — that case is actions **33–34** |
| First-seen Press Start (slot `0x14`) `00597BF2(0)` | **PROVEN** no-op (`je 00597DF0`) |
| Type-10 CUIDef is a first-seen `0055CB10` node | **UNREAD** (ctor has no input `vtbl+8`) |
| Type-11 INVISIBLE **does** subscribe 27 | **PROVEN** `0054DC99` |
| First-seen action 27 on that type-11 posts `0xE5` | **DISPROVEN** unless already armed `[+364]` from action 26 |
| First-seen frontend effect of action 27 | **PROVEN** no `0059A238`, no screen change |

MMB is `proofs/type7-action35`. RMB is this note.

---

## 1. `00A03E40` is the input-type-10 ctor

```
00A03E40  mov eax, [esp+4]
          fld qword [esp+8]
          mov [ecx+32], 0x3        ; mouse-like device
          mov [ecx+40], 0xA        ; event type 10
          mov edx, [eax]
          mov [ecx+24], edx        ; origin pair
          mov eax, [eax+4]
          fst [ecx+48]
          fstp [ecx+44]
          mov [ecx+28], eax
          ret 12
```

Same shape as LMB `00A03C80` (`+40=4`) and MMB `00A03D90` (`+40=7`).
RTTI is still `CInputTypeMouseButtonEvent`; the button is `[+40]`,
not a distinct class.

`00A03B40` (`mov eax, [ecx+40]; ret`) is the getter `0042E3EE` uses.
Not a constructor.

---

## 2. Who builds type 10 (RMB, not MMB)

`00AB5420` `[esi+8]` = raw, `lea ecx, [eax-1]`, index `0xAB56EC`,
jump `0xAB56C4`:

| `[esi+8]` | Site | Ctor | `[record+40]` |
| ---: | --- | --- | ---: |
| 1 | `00AB54F0` | `00A03C80` | 4 LMB down |
| **2** | **`00AB553E`** | **`00A03E40`** | **10** |
| 3 | `00AB5517` | `00A03D90` | 7 MMB down |
| 4 | `00AB5590` | `00A03D60` | 6 LMB up |
| 5 | `00AB55EE` | `00A03EC0` | 12 RMB up |
| 6 | `00AB55BF` | `00A03E10` | 9 MMB up |

```
00AB553E  fld qword [esi]
          …
          call 00A03E40
```

### DINPUT (`00AB4910`)

`dwOfs` 12/13/14 = `DIMOFS_BUTTON0/1/2`. Down bit `dwData & 0x80`.

BUTTON1 (`00AB4A8D`):

```
and cl, 0x80
neg cl
sbb ecx, ecx
and ecx, -3
add ecx, 5          ; down 2, up 5
jmp 00AB4B26        ; mov [edi+8], ecx
```

BUTTON2 (`00AB4AA2`) is `add 6` → down **3**, up **6**, then
`00A03D90` type **7**. That is MMB.

| Button | `dwOfs` | down raw | up raw | down type | down action |
| --- | ---: | ---: | ---: | ---: | ---: |
| LMB | 12 | 1 | 4 | 4 | 26 |
| **RMB** | **13** | **2** | 5 | **10** | **27** |
| **MMB** | **14** | **3** | 6 | **7** | **35** |

### Win32 (`00AB4BB0` when DINPUT is off)

Getters:

| VA | Byte | Message (from `009A5E48` `sub eax, 0x201`) |
| --- | ---: | --- |
| `009A4FC0` | `+221` | `WM_LBUTTONDOWN` `0x201` |
| `009A4FD0` | `+222` | `WM_MBUTTONDOWN` `0x207` |
| `009A4FE0` | `+223` | `WM_RBUTTONDOWN` `0x204` |

Edge encode (`dec; neg; sbb; and 3; add N`):

| Getter | Site | `add` | raw down/up | type |
| --- | --- | ---: | ---: | ---: |
| `009A4FC0` | `00AB4EAA` `inc` | 1 | 1 / 4 | 4 |
| `009A4FD0` | `00AB4EFA` | 3 | 3 / 6 | 7 |
| `009A4FE0` | `00AB4F4C` | **2** | **2 / 5** | **10** |

Same pairing as DINPUT. Type 10 is RMB on **both** backends.

---

## 3. `0042E3EE` type 10 → action 27

`00A03B40` then (`cmp eax, 10` / `je`):

```
0042E46A  cmp eax, 10
0042E46D  jg  0042E560
0042E473  je  0042E557
…
0042E557  call 0041E5F2
0042E55C  push 27
0042E55E  jmp 0042E5AB      ; mov edx,[eax]; call [edx] = 0055CB10
```

No DIK compare. Neighbours in the same chain: type 4 `push 26`,
type 6 `push 28`, type 7 `push 35`. Type 13 is the `jg` arm
(`push 25` + cursor store).

C# `FrontendInputMap.ActionFromEvent(10)=27` and
`EngineInput.ApplyEvent` type 10 → `Dispatch(27)` **MATCH**.
`EngineInput` comment “Type7 is RMB (`00A03D90`)” is **LEFTOVER**
(wrong button).

---

## 4. Widget type 10 is not input type 10

Persist / CUIDef type **10** is `FrontendWidgetType.Menu`:

| | Input type 10 | Widget type 10 |
| --- | --- | --- |
| Ctor | `00A03E40` | `0054E3D0` |
| Size / vtbl | record `[+40]` | `0x16C`, widget `012497E4`, inner `012497BC` |
| Meaning | RMB down event | screen/menu container |
| First-seen roots | — | PRESS_START / NEW_PROFILE / MAIN_MENU are all type 10 |
| Apply | `0055CB10(27)` | inner `vtbl+4` = `0054E280` **if** that inner is a listener |

Type-10 ctor does **not** call input `vtbl+8` (`action26-subscribers`).
Type 11/38 go through `0055BA20` and **do**. Do not treat
`MessageFromWidgets` “first visible type 10” as the native walk.

---

## 5. `0054E280` — widget apply, not the event ctor

`ecx` is **widget+4**. `lea eax, [ebx-26]`; `cmp eax, 8`;
index `0x54E33C` = `00 01 03 03 03 03 03 02 02`; jump `0x54E32C`.

| Action | idx | Target | Effect |
| ---: | ---: | --- | --- |
| **26** | 0 | `0054E2FA` | if `[inner+348]` (widget+352) ≠ 0, UI `vtbl+32` `0059A238` |
| **27** | 1 | **`0054E2B8`** | `00595582`; `00597BF2(0)` |
| 28–32 | 3 | `0054E319` | debounce only |
| 33–34 | 2 | `0054E2C8` | last-key==1 → `00597BF2(1)` |
| 25 | — | skip stamp | no post |
| 35 | — | `ja 0054E319` | out of table |

Action 27 body:

```
0054E2B8  push 0
          call 00595582          ; UI singleton
          mov ecx, eax
          call 00597BF2          ; arg 0
          jmp 0054E319
```

`0054E2C8` is **not** action 27. `type7-action35` §7 named the
wrong case.

`00597BF2` on Press Start (current screen == slot `0x14`, built
by `00598A1C`):

```
mov [ebp-4], 0x14
call 0059B5D7                  ; slot lookup
cmp current, [slot 0x14]
je …
cmp [ebp+8], 0                 ; arg
je  00597DF0                   ; epilogue, ret 4
```

First-seen frontend **is** slot `0x14`. Arg 0 therefore returns
immediately: **no** `00596763` swap, **no** `0059A238`.

Off Press Start, `00597BF2(0)` is the recovered back/stack walk
(slot `0x1B` / `0x17` / `0x1A` branches). That is **not** first-seen
and is **not** the stored-id poster.

Even if a type-10 inner were a `0055CB10` node, first-seen action 27
would still be a no-op. Post of attach `0xE5` remains action **26**.

---

## 6. First-seen frontend effect of action 27

Press Start tree: root widget type 10 (not registered), list type 12,
`UI_FRONTEND_BUTTON_INVISIBLE` type **11** (registered, persist
`0xE5`). No type 38. No key redefiner.

Type 11 activate `0054DC30` (parent `+545`) inner `vtbl+12`:
**26, 31, 28, 27, 32, 29**. Action 27 is in that set.

Apply `0054DBC0` forwards to `0055AD60` only if parent `+545`.
`0055AD60` case 1 (`0055ADB2`, action 27):

```
debounce vs [+396]/[+392]
if [+364]==0 → 0055AE3D          ; stamp + 0055B9D0; not armed
else vtbl+524([+372])            ; armed-release of persist id
```

`[+364]` is set by action **26** (`0055AD7B`). First RMB with no
prior LMB: unarmed → `0055B9D0` (only special-cases action 25) →
**no post**.

So first-seen action 27:

```
RMB down
  00A03E40 type 10
  0042E3EE push 27
  0055CB10
    type-11 INVISIBLE: unarmed → no 0059A238
    type-10 Menu: not a proven listener; if invoked, 00597BF2(0)
                  on slot 0x14 is ret
→ no 0xE5 / 0x126 / 15, no New Profile
```

Armed-release of persist `0xE5` / `0x126` / 15 on a **later** action
27 is **PARTIAL** (table recovered; needs a prior 26 on the same
widget). That is not first-seen.

---

## 7. C# leftover (no `src/` change)

| Site | Native | Host |
| --- | --- | --- |
| `Type10` / `ActionType10=27` | **MATCH** `0042E3EE` | constants + `ApplyEvent` |
| Physical type 10 | RMB down | live client never queues type 10 (`host-input-type4`) |
| `MessageFromAction` / `MessageFromWidgets` | no first-seen post on 27 | null unless action 26 — **MATCH** first-seen |
| `EngineInput` Type7 comment | MMB | **LEFTOVER** “RMB” |
| `Type10ActionFn=0054E280` | widget apply | name collides with **widget** type 10 |

Do not map MMB → input type 10. Do not treat widget type 10 as the
event. Do not post `0xE5` from action 27 on Press Start.

---

## Sources

- `listing-00a00000.txt` (`00A03E40`)
- `listing-00a80000.txt` (`00AB4910`, `00AB4BB0`, `00AB5420`)
- `listing-00400000.txt` (`0042E557` `push 27`)
- `listing-00540000.txt` (`0054E280`, `0054DBC0`, `0055AD60`)
- `listing-00580000.txt` (`00597BF2` slot `0x14` / arg 0)
- `listing-00980000.txt` (`009A4FC0` / `009A4FD0` / `009A4FE0`)
