# 0055C650 type-32 ctor: register, map 25, `input+184`, Press Start #2

Investigation only. No production `src/` edits.

Question: `0055C650` type-32 ctor does `input.vtbl+8(inner)`,
`0052DA20(25)`, `input+184=widget`. First-seen Press Start
second `0055CB10` node after INVISIBLE. Host leftover (no
`QueueInput` type 13)?

Authority: dump `Fable.exe` `0055C650` in
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`;
`0041D21B` / `0041D27C` in `listing-00400000.txt`;
`0052DA20` in `listing-00500000.txt`;
`implementer/frontend/persist-scan.txt` `#620`;
`src/Fable.Client/Program.cs`;
`src/Fable.Game/FrontendInputMap.cs` / `EngineInput.cs`;
`proofs/mouse-pointer-action25/README.md`;
`proofs/action26-subscribers/README.md`;
`proofs/type10-subscribe-first/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH**.

Do not re-prove type 4 → `push 26`, type 13 ctor `00A03FB0`,
`0055CB10` broadcast shape, or `0124C208+4`/`+8` rdata dwords.

---

## Verdict

**Yes.** Native ctor, register, local map, and Press Start list
order are **PROVEN**. Live host never queues type 13.

| Claim | Status |
| --- | --- |
| Factory type 32 is `0041D21B` `jmp [0x41D7F8+type*4]` → `0041D27C` → `0055C650` (size `0x184`) | **PROVEN** |
| `0055C650` is the entire ctor (`ret 4` / INT3) | **PROVEN** |
| Ctor `lea edi,[esi+4]` then `0041E5F2` `call [edx+8]` is `input.vtbl+8(inner)` | **PROVEN** |
| That call registers `widget+4` as a `0055CB10` listener | **PROVEN** sites; `01230134+8` body dword **PARTIAL** |
| Ctor `push 25` / `mov ecx,edi` / `call 0052DA20` local-maps action 25 | **PROVEN** |
| New insert also `inner.vtbl+4(25)` inside `0052DA20` | **PROVEN** |
| Ctor `mov [eax+184], esi` is `input+184 = widget` | **PROVEN** |
| `input+184` is the `0055CB10` node | **DISPROVEN** (node is `widget+4`) |
| First-seen Press Start first `0055CB10` node is `UI_FRONTEND_BUTTON_INVISIBLE` type 11 | **PROVEN** |
| First-seen Press Start second `0055CB10` node is `UI_MOUSE_POINTER` type 32 | **PROVEN** |
| Type 10 / 12 / 5 / 18 / 6 on this tree also register | **DISPROVEN** |
| `FrontendInputMap.TypeMouse=13` / `ActionMouse=25` classify | **MATCH** `0042E5DC` |
| Live host `QueueInput` type 13 | **DISPROVEN** — **LEFTOVER** producer |

---

## Answer

`0055C650` is the type-32 (`CMouseCursor@NUISystem` /
`UI_MOUSE_POINTER`) ctor. After type-0 base `0041B800` it
replaces vtbls, registers `inner=widget+4` via input
`vtbl+8`, local-maps 25, and stores the widget at
`input+184`. On first-seen Press Start that register is
the **second** `0055CB10` append (after INVISIBLE). The
host leftover is the missing type-13 producer, not the
13→25 map.

---

## 1. Dump `0055C650` (entire function)

Factory `0041D21B` (`cmp eax, 43` / `jmp [0x41D7F8+eax*4]`).
Type-32 arm:

```
0041D27C  push 0x184
0041D281  call 00BFEA1A
0041D28F  push edi
0041D290  mov ecx, eax
0041D292  call 0055C650
```

`FrontendWidgetType.Info(32).Ctor = 0x0055C650` /
`.Vtbl = 0x0124C22C` / `.Size = 0x184`. Persist
`UI_MOUSE_POINTER` `*Type i32=32`.

```
0055C650  mov eax, [esp+4]          ; def
0055C654  push esi
0055C655  push edi
0055C656  push eax
0055C657  mov esi, ecx
0055C659  call 0041B800             ; type 0 (no register)
0055C65E  lea edi, [esi+4]          ; inner = widget+4
0055C661  mov [esi], 0x124C22C      ; widget vtbl
0055C667  mov [edi], 0x124C208      ; inner vtbl
0055C66D  mov [esi+24], 0x124C200
0055C674  call 0041E5F2             ; input*
0055C679  mov edx, [eax]
0055C67B  push edi
0055C67C  mov ecx, eax
0055C67E  call [edx+8]              ; input.vtbl+8(inner)
0055C681  push 25
0055C683  mov ecx, edi
0055C685  call 0052DA20             ; local map 25
0055C68A  call 0041E5F2
0055C68F  mov [eax+184], esi        ; input+184 = this widget
0055C695  pop edi
0055C696  mov eax, esi
0055C698  pop esi
0055C699  ret 4
0055C69C  int3
```

Same register + `0052DA20(25)` + `[input+184]=widget` as
`mouse-pointer-action25`. Type 0 itself does not join
`0055CB10`. The list node is `inner` (`widget+4`), not
`input+184`.

`01230134+8` as the append primitive stays **PARTIAL**
(no `.rdata` dword). The call **site** is the same
`0041E5F2` + `push inner` + `call [edx+8]` used by type
11 (`0055BA20`) and type 37 (`005407B0`).

---

## 2. `0052DA20(25)` is local map, then apply

`listing-00500000.txt`:

```
0052DA20  ; ecx = inner, arg = action
          lea  esi, [edi+4]
          call 0052DF20             ; find
          call 0052E230             ; insert if missing
          cmp  [esp+28], 25
          jne  0052DA65             ; other ids: insert only
          cmp  [esp+12], ebx
          jne  0052DA65             ; already present: no apply
          push 25
          mov  ecx, edi
          call [edx+4]              ; inner.vtbl+4(25)
          ret  4
```

Ctor insert is new, so construct applies 25 once with
**no** type-13 event. Later motion does not go through
`0052DA20` again; it is `0042E5DC` `push 25` →
`0055CB10`.

---

## 3. First-seen Press Start list order

`005331A0` builds `[def+112]..[def+116]` during the parent
ctor. Register is DFS **post-order** of types that call
input `vtbl+8` (11 / 32 / 33 / 34 / 37 / 38). Append is
before sentinel (`0055CF00` / `0055CE90` shape) → first
register is first `0055CB10` node.

`persist-scan.txt` `#620` `UI_FRONTEND_PRESS_START_MENU`
Type=10, `Children=6`:

| # | Name | Type | `vtbl+8`? |
| --- | --- | ---: | --- |
| 1 | `UI_BLENDING_BACKGROUNDS_FORREST` | 5 | no |
| 2 | `UI_TITLE` | 5 | no |
| 3 | `UI_PRESS_START_SWAP` → `UI_PRESS_START_TEXT` | 18 / 6 | no |
| 4 | `UI_FRONTEND_LIST_PRESS_START_MENU` | 12 | no |
|  | → **`UI_FRONTEND_BUTTON_INVISIBLE`** | **11** | **yes** (`0055BA20`) |
| 5 | `UI_LEGAL_TEXT` | 6 | no |
| 6 | **`UI_MOUSE_POINTER`** | **32** | **yes** (`0055C650`, map 25) |

No type 37 / 38 / 33 on this screen. Type 10 root does
not register (`type10-subscribe-first`). First node is
INVISIBLE. Second node is the mouse inner.

---

## 4. Host leftover (no `QueueInput` type 13)

Classify **MATCH**es native:

```
FrontendInputMap.TypeMouse  = 13
FrontendInputMap.ActionMouse = 25
EngineInput.TypeMouse       = 13
EngineInput.ActionMouse     = 25
ActionFromEvent(13, _)      = 25
```

Live `Fable.Client` `QueueInput` sites:

| Site | Type |
| --- | --- |
| Escape / Space / Enter / F4 / A / B | `TypeKey` |
| LMB down | `Type4` |
| LMB up | `Type6` |

`MouseMove` only `debugCam.Look` when F2-look is on. No
`QueueInput(TypeMouse)`. `EngineInput.ApplyEvent(TypeMouse)`
is `Dispatch(25)` only — missing native `+176/+180` then
`0055CB10`. Ctor apply (`0052DA20(25)`) has no host analog.

The leftover is the **missing producer** (and cursor store),
not the 13→25 id pair. Do not map LMB → type 13.

---

## Do not invent

- Type 13 as Press Start click / `0xE5`.
- `input+184` as the `0055CB10` listener.
- `0124C208+4` / `+8` or `01230134+8` rdata dwords.
- Type 10 / 12 as first-seen list nodes.
