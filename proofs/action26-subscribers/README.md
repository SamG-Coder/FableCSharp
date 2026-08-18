# 0055CB10 listener walk — who is subscribed to action 26

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `0055CB10` / `0042BE7B` / `0042E3EE` /
`0041E5F2` / `0054E280` / `0054DBC0` / `0055AD60` / `0055BA20` /
`0054DC30` / `0055AEB0` / `0055B040` / `00598EE6`;
`frontend.bin` (`implementer/frontend/persist-scan.txt`);
`src/Fable.Game/FrontendInputMap.cs`;
`src/Fable.Game/EngineInput.cs`;
`proofs/who-posts-15/README.md`;
`proofs/audit-lifecycle-input/README.md`;
`implementer/frontend/05-input.md`.

Status: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not re-prove type 4 → `push 26`, Return≠`0xE5`, Leave/`FinalAlbion.wld`.

---

## Verdict

| Claim | Class |
| --- | --- |
| `0042E3EE` type 4 → `0041E5F2` then `vtbl+0` = `0055CB10(26)` | **PROVEN** |
| `0055CB10` `this` is the input singleton (`01230134`, `[0x13B8710]`) | **PROVEN** |
| `[this+8] != 0` is **focused** (exclusive accept+apply, then return) | **PROVEN** |
| `[this+8] == 0` **broadcasts** list `+12`, else fallback list `+4` | **PROVEN** |
| Accept is listener `vtbl+8(action)`; apply is `vtbl+4(action)` | **PROVEN** |
| Ctor `0042BE7B` zeros `+8`; `+4`/`+12` are empty circular 12-byte lists | **PROVEN** first-seen |
| Type **33** ctor `0055BA20` registers `widget+4` via input `vtbl+8` | **PROVEN** |
| Type **11** / **38** go through `0055B460` → `0055BA20` (so they register) | **PROVEN** |
| Type **10** ctor `0054E3D0` does **not** call input `vtbl+8` | **PROVEN** |
| Type-10 apply `0054E280` case 0 (`0054E2FA`) posts `widget+352` | **PROVEN** if invoked |
| Type-11 apply `0054DBC0` action 26 → `0055AD60` only if parent `+545` | **PROVEN** |
| Type-38 / 11 `0055AD60` action 26 **arms** `[+364]=1`; does **not** `vtbl+524` | **PROVEN** |
| Type-38/11 persist id is posted on a **later** case (`vtbl+524`, often action 27) | **PARTIAL** (table recovered; 27 is the armed-release arm) |
| Type-11 activate `0054DC30` `inner.vtbl+12(26,31,28,27,32,29)` if parent `+545` | **PROVEN** site |
| Type-38 `0055AEB0` `inner.vtbl+12(26,31,27,32)` | **PROVEN** site |
| Inner `vtbl+12` / `+16` are local action map insert/erase (`0052DA20`) | **PROVEN** shape |
| Input `vtbl+8` = add-to-broadcast vs write `+8` focus | **UNREAD** (no `.text` dump of `01230134+8`) |
| Type-10 is a `0055CB10` listener first-seen | **UNREAD** (no register in 4/5/10 ctor) |
| First-seen `+8` stays 0 on frontend | **PARTIAL** (ctor 0; no recovered writer) |

C# `MessageFromWidgets` (first visible type 10/11/38) is a **LEFTOVER** vs this walk.

---

## 1. How action 26 reaches `0055CB10`

```
0042E3EE
  00A03B40  type = [record+40]
  type 4:  call 0041E5F2
           push 26
           jmp 0042E5AB
0042E5AB
  call [eax]          ; input.vtbl+0
```

`0041E5F2` returns `[0x13B8710]` (alloc `0xD0`, ctor `0041E3F6`, vtbl `01230134`).
C# / tests lock `EngineInput.ActionApply` / `FrontendInputMap.ActionApply` = `0055CB10`.

---

## 2. `0055CB10` (listing `listing-00540000.txt`)

```
this = esi
if [esi+8] != 0:                    ; focused listener*
    if listener.vtbl+8(action):     ; accept
        listener.vtbl+4(action)     ; apply
    return                          ; exclusive even if accept fails
else:
    if [esi+12] list is non-empty:  ; head->next != head
        0055CF50 copy +12
        for node in copy:
            listener = [node+8]
            if listener.vtbl+8(action):
                listener.vtbl+4(action)
    else:
        same walk of [esi+4]
```

`0055CF50` allocs a 12-byte circular copy so apply can mutate the live list.

`0042BE7B` (input base):

| Offset | Init |
| --- | --- |
| `+4` | `0042AC0A` empty circular node (`next=prev=self`) |
| `+8` | `and [esi+8], 0` |
| `+12` | `0042AC0A` empty circular node |

`0042BEA9` is `return [ecx+8]` (getter). No other `.text` `call 0042BEA9`.

**Focused** = exclusive, one listener. **Broadcast** = every list node whose accept returns true.

First-seen frontend: `+8 == 0` unless a recovered writer runs. Classify first-seen as **broadcast** of whoever is on `+12`/`+4`.

---

## 3. Listener object is `widget+4`

Every UI widget stores an inner vtbl at `+4`. `0055CB10` calls that inner:

| Type | Ctor | Widget vtbl | Inner vtbl | Apply (`vtbl+4`) |
| ---: | --- | --- | --- | --- |
| 10 | `0054E3D0` | `012497E4` | `012497BC` | `0054E280` |
| 11 | `0054E0B0` → `0055B460` | `01249554` | `01249530` | `0054DBC0` |
| 38 | `00558B90` → `0055B460` | `0124B04C` | `0124B024` | `0055AD60` |
| 34 (11/38 base) | `0055B460` → `0055BA20` | `0124BD2C` | `0124BD08` | (overridden) |
| 33 | `0055BA20` | `0124BFB4` | `0124BF90` | — |

Accept on type 10 (site `0054E190`, `mov al,1; ret`) is **always true** if that slot is inner `vtbl+8`. Type 11/38 accept slot **UNREAD** as a named fn (same pattern likely).

### Who registers with the input singleton

```
0055BA20  (type 33)
  0052CC50
  inner = this+4
  0041E5F2
  input.vtbl+8(inner)     ; register
```

Type 11/38 ctors call `0055B460` → `0055BA20`. They **are** `0055CB10` listeners.

Type 4 ctor `005334A0` / type 5 `0052CC50` / type 10 `0054E3D0` have **no** `0041E5F2` + `vtbl+8`. Type 4 **dtor** `00532D90` does `input.vtbl+20(this+4)` (unregister analog).

So: type 11/38 **PROVEN** subscribed as objects. Type 10 **UNREAD** as a `0055CB10` node first-seen.

---

## 4. What apply(26) does (not the same as “posts”)

### Type 10 — `0054E280`

`this` = widget+4, so `[edi+348]` is widget `+352`.

1. Debounce: `[+44] - [+344]` vs `[0x12305A0]`.
2. `lea eax,[ebx-26]`; index `00 01 03 03 03 03 03 02 02` at `0x54E33C`.
3. Action **26** = case 0 = `0054E2FA`: if `[+348] != 0`, `00595582` then UI `vtbl+32` (`0059A238`) with `&widget+352`.
4. If action != 25, stamp `[+344]=[+44]`.

Press Start attach `00598EE6` `mov [eax],0xE5` then type-10 `vtbl+284` `0054E4F0` stores that at `+352`. New Profile / Main Menu type-10 persist `+224` is **0**; they are **not** patched. Type-10 apply(26) is a no-op on those roots even if invoked.

### Type 11 — `0054DBC0`

1. Debounce vs `[+400]` / `[+392]`.
2. Parent via `[this-4].vtbl+432`; `bl = [parent+545]`.
3. If `bl`: `0055AD60(action)` (type-38 switch on the type-11 object).

If parent `+545` is clear, action 26 is dropped.

### Type 38 / armed type 11 — `0055AD60`

`lea eax,[edi-26]; cmp eax,6; jmp [0x55AE88+eax*4]` (actions 26–32).

| Action | Site | Effect |
| ---: | --- | --- |
| 26 | `0055AD7B` | if `[+348]==0` → `0055B9D0` only. Else `vtbl+584`, `[+364]=1`, `0055B9D0` |
| 27 | `0055ADB2` | if armed `[+364]` and debounce: `vtbl+524([+372])` |
| others | `0055ADDE`… | hover / unarm / `vtbl+524([+388])` |

`0055B9D0` is `if action==25: vtbl+580; ret`. **Not** a UI message.

So action **26** on type 11/38 is **press/arm**, not `0059A238`. Persist `+224` is copied at ctor by `0055B040` → `vtbl+284`; `vtbl+524` is the poster. C# / `who-posts-15` “action 26 posts 15” is **STALE** as a single-step claim: type 4 delivers 26 (arm); the stored 15 / `0x126` post is a later action on the same widget (**PARTIAL**, 27 is the recovered armed-release).

### Local subscribe-set (inner `vtbl+12` / `+16`)

These insert/erase action ids in the widget’s own map (`0052DA20` at inner+4). They are **not** the `0055CB10` list.

| Site | When | Actions |
| --- | --- | --- |
| `0054DC30` | type 11 activate; parent `+545`; `vtbl+192(3)` | **26**, 31, 28, 27, 32, 29 |
| `0054DCC0` | type 11 deactivate; `vtbl+16` | same |
| `0055AEB0` | type 38 enable | **26**, 31, 27, 32 |
| `0055AEF0` | type 38 disable | same |
| `0052D7B0` | type 5 widget `vtbl+12` 0…32 | includes 26 (type-10 **overrides** widget vtbl; this site is type 5) |

If accept consults this map, a type-11 that never ran `0054DC30` would reject 26 even though it is on the input list. That accept filter is **UNREAD**.

---

## 5. Screen trees (persist)

Same `005331A0` child walk. Roots are all type 10.

### Press Start — `UI_FRONTEND_PRESS_START_MENU`

| Widget | Type | Stored id | Register |
| --- | ---: | ---: | --- |
| root | 10 | attach `+352` = `0xE5` (persist `+224` = 0) | **UNREAD** as `0055CB10` node |
| `UI_FRONTEND_LIST_PRESS_START_MENU` | 12 | 0 | list |
| `UI_FRONTEND_BUTTON_INVISIBLE` (only list child) | **11** | persist **229 / `0xE5`** (`Action` `0xF1A22807` @1089) | **PROVEN** via `0055BA20` |

No type 38 on this screen.

Action-26 listeners that are **proven** objects: the invisible type-11. Type-10 apply is **PROVEN** as a handler, **UNREAD** as a list node. If first-seen is broadcast of registered type 33+, INVISIBLE is the subscriber; it **arms** on 26 (parent `+545` **UNREAD**). The attach-stored type-10 `0xE5` still needs either a type-10 register site or a different caller of `0054E280`.

Do **not** treat C# “first visible type 10” as native.

### New Profile — `UI_FRONTEND_NEW_PROFILE_SCREEN`

| Widget | Type | Stored id | Register |
| --- | ---: | ---: | --- |
| root | 10 | 0 | not in ctor |
| `UI_ACCEPT_NEW_PROFILE` | **38** | persist **`0x126`** | **PROVEN** `0055BA20` |
| `UI_NEW_PROFILE_EDIT_BOX` | 37 | — | edit box (actions 33/34, not 26) |

Action-26 object: type-38 accept. Apply(26) arms if `[+348]`. Post of `0x126` is not the 26 case.

### Main Menu — `UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE`

| Widget | Type | Stored id | Register |
| --- | ---: | ---: | --- |
| root | 10 | 0 | not in ctor |
| `UI_FRONTEND_LIST_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` | 12 | 0 | list |
| `UI_FRONTEND_BUTTON_NEW_GAME` (first list child) | **11** | persist **15** | **PROVEN** `0055BA20` |
| later list type-11s (Load / Options / …) | 11 | other persist ids | same ctor register |

If `+8` is still 0, **broadcast** delivers 26 to **every** registered type-11 (and type-38, if any). Each apply still needs parent `+545`. First-seen list highlight / which child has `+545` is **UNREAD**. C# first-DFS NEW_GAME **MATCH**es the happy path only.

Type-10 root does **not** store 15 (`who-posts-15` **DISPROVEN**).

---

## 6. Broadcast vs focused (this walk)

```
first-seen input:
  +8 = 0          → not focused
  +12 empty       → walk +4
  +4              → nodes from input.vtbl+8(inner)

type 33+ ctor     → vtbl+8(inner)     UNREAD: push onto +4/+12  OR  write +8
type 4 dtor       → vtbl+20(inner)

if a later site writes +8:
  only that listener sees action 26
```

No recovered frontend writer of input `+8`. First-seen classification: **broadcast**, not focused.

List widget `005403D2` (last-key `== 1`) walks `[list+352]` and calls input `vtbl+56` (`0041E6D3`). That is action **33**, **DISPROVEN** as the action-26 walk.

---

## 7. C# leftover vs this walk

`FrontendInputMap.MessageFromWidgets`: first `Visible && !Clip && MessageId!=0` of type 10/11/38, then `MaybeActivate` returns after **one** message.

Native:

- may call **every** registered listener (broadcast);
- type 11/38 action 26 does **not** post;
- type 10 posts only if it is actually invoked and `+352 != 0`;
- parent `+545` / `[+348]` / local `vtbl+12` set can drop 26.

Install test `Type4_drives_lifecycle_0xE5_then_0x126_then_15` **MATCH**es first-seen ids, not the listener set.

---

## 8. Proposed (do not apply here)

1. Keep type 4 → action 26. Do not map Return.
2. Do not treat “first visible type 10/11/38” as `0055CB10`.
3. Dump `01230134+8` / `+20` before inventing focus.
4. Find a type-10 `vtbl+8` register, or stop claiming `0054E280` is first-seen Press Start if INVISIBLE is the only node.
5. Split arm (26) vs post (`vtbl+524`, likely 27) in C# only after a host type-4 **and** type-10 event exist. Do not invent a second host key.

## Do not invent

- A DIK for type 4.
- Enter → `0xE5` / `0x126` / 15.
- Input `+8` = selected list child without a write site.
- Type-10 on the `0055CB10` list without a register site.
- Lionhead name for CRC `0x53C644E4`.
