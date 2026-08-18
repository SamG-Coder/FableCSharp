# Type 7 → action 35 is not RMB / not UI_CANCEL

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `00A03D90` / `00AB4910` / `00AB4BB0` /
`00AB5420` / `009A5E48` / `0042E3EE` / `0055CB10` / `0054E280` /
`0054DBC0` / `0055AD60` / `00557AF0` / `00557EB0`;
listings `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00a00000.txt`,
`listing-00a80000.txt`, `listing-00400000.txt`, `listing-00540000.txt`,
`listing-00980000.txt`;
`proofs/type13-vs-type4/README.md`,
`proofs/type4-dinput-raw/README.md`,
`proofs/type10-plus352/README.md`,
`proofs/type11-msg15/README.md`,
`proofs/type38-msg126/README.md`,
`proofs/ui-cancel-message/README.md`;
`src/Fable.Game/FrontendInputMap.cs`;
`implementer/frontend/05-input.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER**.

Do not re-prove type 4 → action 26 / `0xE5`, type 13 → action 25, or
`0059A238` consume of `0xE5` / `0x126` / 15.

Three different “35”s:

| 35 | Meaning |
| --- | --- |
| Action **35** | `0042E3EE` `push 35` from event type 7 |
| Persist widget type **35** | factory `0055A9C0`; `vtbl+260` `cmp eax, 35` at `0055A6ED` |
| `0057A535` | `push 35; pop eax; ret` — getter for widget type 35 |

This note is the **action**. Widget type 35 is not UI_CANCEL (type
**UNREAD**; type 39 ctor `00558540` is the neighbour of Accept).

---

## Verdict

**Type 7 is middle-mouse down, not RMB.** `0042E3EE` maps it to
action 35. On Press Start / New Profile / Main Menu, action 35 does
**not** post a UI message and is **not** `UI_CANCEL`.

| Claim | Status |
| --- | --- |
| `00A03D90` writes `[record+40]=7`, `[+32]=3` | **PROVEN** |
| `0042E3EE` type 7 → `push 35` → action vtbl+0 (`0055CB10`) | **PROVEN** `0042E48C` |
| Type 7 is RMB down | **DISPROVEN** — RMB is raw **2** → event type **10** → action **27** |
| Type 7 is MMB / BUTTON2 / `WM_MBUTTON*` down | **PROVEN** DINPUT; Win32 `+222` **PARTIAL** (0x207-range table) |
| `0054E280` handles action 35 | **DISPROVEN** — table is actions 26–34 only (`lea eax,[ebx-26]`, `cmp 8`) |
| Type 11 `0054DBC0` / type 38 `0055AD60` special-case 35 | **DISPROVEN** — `0055AD60` is actions 26–32 (`cmp eax, 6`); 35 → `0055B9D0` only |
| Type 11/38 subscribe-set includes 35 | **DISPROVEN** (`0054DC7E` / `0055AEBE`: 26/27/28/29/31/32, no 35) |
| Recovered frontend subscriber of 35 | **PROVEN** `00557AF0` `push 35` / inner `00557EB0` |
| That subscriber is on first-seen Press Start / New Profile / Main Menu | **DISPROVEN** (those trees have no redefiner; `CKeyRedefiner@NUISystem` + `TEXT_GUI_PRESS_CONTROL`) |
| First-seen type 7 / action 35 posts `0xE5` / `0x126` / 15 / `UI_CANCEL` | **DISPROVEN** |
| `UI_CANCEL` is action 35 | **DISPROVEN** — persist name; action-26 stored-id path is the only recovered cancel/accept poster |
| C# `MessageFromWidgets` posts on 35 | **DISPROVEN** (action 26 only). `ActionFromEvent(7)=35` is unused by that poster |

RMB on those screens is event type **10** / action **27**, not 35.

---

## 1. `00A03D90` is event type 7 (device 3)

```
00A03D90  mov eax, [esp+4]
          fld qword [esp+8]
          mov [ecx+32], 3
          mov [ecx+40], 7
          ; [+24]/[+28] from ptr
          fst [ecx+48]
          fstp [ecx+44]
          mov [ecx+28], eax
          ret 12
```

Same shape as type-4 `00A03C80`. Sole `.text` call is `00AB5527`
inside `00AB5420`.

---

## 2. Who builds type 7 (not RMB)

`00AB5420` `[esi+8]` = raw, `lea ecx, [eax-1]`, index `0xAB56EC`,
jump `0xAB56C4` (already dumped in `type4-dinput-raw`):

| `[esi+8]` | Ctor | `[record+40]` |
| ---: | --- | ---: |
| **1** | `00A03C80` | **4** (LMB down) |
| **2** | `00A03E40` | **10** |
| **3** | **`00A03D90`** | **7** |
| 4 | `00A03D60` | 6 (LMB up) |
| 5 | `00A03EC0` | 12 |
| 6 | `00A03E10` | 9 |

### DINPUT (`00AB4910`)

`dwOfs` 12/13/14 = `DIMOFS_BUTTON0/1/2`. Down/up:

| Button | `dwOfs` | down raw | up raw | down type | down action |
| --- | ---: | ---: | ---: | ---: | ---: |
| LMB | 12 | 1 | 4 | 4 | 26 |
| **RMB** | **13** | **2** | 5 | **10** | **27** |
| **MMB** | **14** | **3** | 6 | **7** | **35** |

BUTTON1 (`00AB4A8D`): `and 0x80; neg; sbb; and -3; add 5` → down **2**,
up **5**, then `00AB4B26` `mov [edi+8], ecx`.

BUTTON2 (`00AB4AA2`): same mask, `add 6` → down **3**, up **6**.

### Win32 (`00AB4BB0` when `[this+13372]≠1`)

WndProc `009A5E48` `sub eax, 0x201` (`WM_LBUTTONDOWN`):

| Message | Slot | Getter | Edge raw | Type |
| --- | ---: | --- | ---: | ---: |
| `WM_LBUTTONDOWN` `0x201` | `+221` | `009A4FC0` | 1 / 4 | 4 |
| `WM_RBUTTONDOWN` `0x204` | **`+223`** | `009A4FE0` | **2 / 5** | **10** |
| `0x207`… table (`WM_MBUTTONDOWN` = 519) | `+222` | `009A4FD0` | 3 / 6 | 7 |

`009A4FE0` encode (`dec; neg; sbb; and 3; add 2`) is raw 2/5 = type 10.
`009A4FD0` encode (`add 3`) is raw 3/6 = type 7.

RMB is type 10 on **both** backends. Type 7 is the **next** button
(DINPUT BUTTON2 = middle). Calling type 7 “RMB” is **DISPROVEN**.

RTTI: all mouse-button records are `CInputTypeMouseButtonEvent`.
The type field is `[+40]`, not a distinct class per button.

---

## 3. `0042E3EE` type 7 → action 35

`00A03B40` then (eax ≤ 10 branch):

```
dec eax            ; 1 → type 1
je  0042E4B0
sub eax, 3         ; 4 → type 4  push 26
je  0042E4A4
dec eax / dec eax  ; 6 → type 6  push 28
je  0042E498
dec eax            ; 7 → type 7
jne 0042E7F0
0042E48C  call 0041E5F2
          push 35
          jmp 0042E5AB      ; mov edx,[eax]; call [edx]
```

Sibling: type **10** (RMB) is `je 0042E557` `push 27`.

`0055CB10` walks subscribers of **that** action only (`vtbl+8` gate,
then `vtbl+4`). No subscriber → no widget body.

---

## 4. Type 10 / 11 / 38 action fns do not cover 35

### Type 10 inner `0054E280`

```
lea eax, [ebx-26]
cmp eax, 8
ja  0054E319          ; 35-26 = 9 → here
```

Index `0x54E33C` = `00 01 03 03 03 03 03 02 02` for **26–34**.
Action 26 = `0054E2FA` post `&widget+352`. Action 35 never takes a
case; `0054E319` only updates debounce `inner+344` unless ebx==25.

### Type 38 / shared `0055AD60`

```
lea eax, [edi-26]
cmp eax, 6
ja  0055AE79          ; 35-26 = 9 → 0055B9D0
```

Cases 0–6 = actions **26–32**. Case 0 (`0055AD7B`) is click
(outer vtbl+584). Action 35 is the default timestamp path.

### Type 11 `0054DBC0`

Debounce + enabled, then **unconditional** `call 0055AD60` with the
incoming action. Still no 35 case. Subscribe (`0054DC7E`) is 26, 31,
28, 27, 32, 29 — **no 35**, so `0055CB10` should not invoke this
inner for 35 at all.

Type 38 subscribe `0055AEB0`: 26, 31, 27, 32 — **no 35**.

---

## 5. Who does subscribe 35: key redefiner, not first menus

`00557AF0` (after `0055ACF0`):

```
mov [0x13B8AC8], esi          ; capture singleton
push "TEXT_GUI_PRESS_CONTROL"
…
push 33 / 26 / 27 / 35 / 38 / 39 / 40 / 41 / 42
call [inner.vtbl+12]          ; subscribe
```

Inner apply `00557EB0`:

```
cmp [0x13B8AC8], this         ; else 0055AD60
lea eax, [ebx-26]
cmp eax, 16                   ; actions 26–42
ja  00558057
jmp [0x558068 + index*4]      ; index @ 0x558090
```

Bodies write a bind blob (`[esp+16]=3` mouse device, sub-id 1 / 2 /
3 / 8…0xC) then `00557D20`. `TEXT_GUI_ACTION_MOVE_FORWARD` /
`MOVE_BACK` sit in the same object (`005581B8`). RTTI name
`CKeyRedefiner@NUISystem`.

That is the **options remapper** (“press a control”), not Press Start
accept / New Profile / Main Menu.

Index byte for action 35 is **PARTIAL** (listing of `0x558090` is
data-as-code). Whether 35 binds as mouse-2 or shares the action-27
slot is unread; it does **not** call UI `vtbl+32` `0059A238`.

---

## 6. First-seen Press Start / New Profile / Main Menu

Press Start dump (`implementer/frontend/17-press-start-frame.txt`):
types 10, 5, 6, 18, 12, 11, 32. No redefiner. Type-10 posts only on
action **26**. Type-11 `UI_FRONTEND_BUTTON_INVISIBLE` posts persist
`0xE5` on action 26 (`0054DBC0` → `0055AD60`).

New Profile: type 10 root, type 12 menu, type 38 accept, helpers
(`UI_CANCEL` hex still **UNREAD** in `ui-cancel-message`). No
`00557AF0` capture.

Main Menu: type 11 New Game persist 15 on action 26.

So first-seen type 7:

```
0042E3EE  push 35
0055CB10  no matching 10/11/38 subscriber
          redefiner singleton 13B8AC8 is 0
→ no 0059A238, no screen change
```

Host `FrontendInputMap.MessageFromWidgets` also returns null unless
action==26. **MATCH** (no-op). `EngineInput.ApplyEvent` currently
drops type 7 entirely (**LEFTOVER** vs `ActionFromEvent`).

---

## 7. Not `UI_CANCEL`

`UI_CANCEL` is a `names.bin` instance (`CCA6E7F6`). Recovered
accept/cancel posts are **action 26** stored persist i32
(`0x53C644E4` → def `+224` / vtbl+284) on type 11/38, or type-10
`+352`. Action 35 never enters those posters.

If the question was “right-click backs out”: that is event type
**10** / action **27**, not 35. Type-10 case for 27 is `0054E2C8`
(last-key==1 → `00597BF2(1)`), **not** `&widget+352`. Type 11/38
do subscribe 27; `0055AD60` case 1 is **PARTIAL** (not dumped here).
Do not invent Escape / B / RMB = `UI_CANCEL` without that case and
the CANCEL file i32.

---

## 8. C# leftover (no `src/` change)

| Site | Native | Host |
| --- | --- | --- |
| `Type7` / `ActionType7=35` | **MATCH** `0042E3EE` | constants exist |
| Physical type 7 | MMB down | unnamed |
| `MessageFromAction` / `MessageFromWidgets` | no post on 35 | null unless action 26 |
| `EngineInput.ApplyEvent` type 7 | `0055CB10(35)` | **dropped** |

Do not map RMB → type 7. RMB → type 10 → 27.

---

## Sources

- `listing-00a00000.txt` (`00A03D90`)
- `listing-00a80000.txt` (`00AB4910`, `00AB4BB0`, `00AB5420`)
- `listing-00400000.txt` (`0042E48C` `push 35`)
- `listing-00540000.txt` (`0054E280`, `0054DBC0`, `0055AD60`,
  `0055CB10`, `00557AF0`, `00557EB0`)
- `listing-00980000.txt` (`009A5E48` `WM_*BUTTON*`)
- `tools/Fable.ExeIndex/out/00-index/rtti.txt`
  (`CInputTypeMouseButtonEvent`, `CKeyRedefiner@NUISystem`)
- `proofs/type4-dinput-raw/README.md` (raw → ctor table)
- `proofs/ui-cancel-message/README.md`
