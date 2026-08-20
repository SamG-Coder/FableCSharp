# Type 13 mouse-move reaches Press Start `UI_MOUSE_POINTER` action 25

Investigation only. No production `src/` edits.

Authority: dump `Fable.exe` `0055C650` / `0055C6A0` / `0055C6F0` /
`0055C8D0` in
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`;
`0041D21B` / `0042E3EE` / `0042E5DC` in `listing-00400000.txt`;
`0052DA20` in `listing-00500000.txt`;
`00A03FB0` in `listing-00a00000.txt`;
`00AB5B3D` in `listing-00a80000.txt`;
`e8.tsv` (`0041D292` → `0055C650`; **no** `E8 0055C6F0`);
`implementer/frontend/persist-scan.txt` `#620`;
`src/Fable.Game/FrontendInputMap.cs` (`TypeMouse=13`,
`ActionMouse=25`);
`src/Fable.Game/EngineInput.cs`;
`src/Fable.Client/Program.cs`;
`proofs/type13-vs-type4/README.md`;
`proofs/action26-subscribers/README.md`;
`proofs/type10-subscribe-first/README.md`;
`proofs/host-input-type4/README.md`;
`FrontendUiDefTests` Press Start tree
(`UI_MOUSE_POINTER` visible, ctor `0055C650`).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH**.

Do not re-prove type 4 → `push 26`, type 13 ctor `00A03FB0`,
`0055CB10` broadcast shape (`[input+8]==0`), or Press Start
register order (INVISIBLE then mouse).

---

## Verdict

**Yes on native. No on the live host.**

First-seen Press Start constructs `UI_MOUSE_POINTER` type **32**
via `0055C650`. That ctor registers `widget+4` on `0055CB10` and
local-maps action **25**. Type 13 (`CInputTypeMouseMovementEvent`)
is `0042E3EE` `0042E5DC` `push 25` → `0055CB10(25)`. The mouse
inner is already the second list node, so a live type-13 record
reaches that object.

The 25-only apply on this cluster is `0055C6F0` (vtbl-only;
no `.text` `E8`). Ctor `0052DA20(25)` also applies 25 once
**without** a type-13 event.

Live `Fable.Client` never `QueueInput(TypeMouse)`. Mouse move is
debug look only. `FrontendInputMap.TypeMouse=13` /
`ActionMouse=25` is the classify, not a leftover id pair.

| Claim | Status |
| --- | --- |
| Factory type 32 is `0041D292` → `0055C650` (size `0x184`) | **PROVEN** |
| `0055C650` is the entire ctor (ends `ret 4` / INT3) | **PROVEN** |
| Ctor overrides vtbl `0124C22C` / inner `0124C208` / `+24` `0124C200` | **PROVEN** |
| Ctor `input.vtbl+8(inner)` registers the `0055CB10` node | **PROVEN** |
| Ctor `0052DA20(25)` local-maps 25; new insert also `inner.vtbl+4(25)` | **PROVEN** |
| Ctor stores the widget at `input+184` | **PROVEN** |
| First-seen Press Start second subscriber is `UI_MOUSE_POINTER` | **PROVEN** |
| Event type 13 → store `+176/+180` then `0055CB10(25)` | **PROVEN** `0042E5DC` |
| Type 13 ctor is `00A03FB0` (`+40=13`, `+32=3`, xyz at `+12`) | **PROVEN** |
| Type 13 producer is `00AB5B3D` (deadzone then enqueue) | **PROVEN** |
| Type 13 is click / action 26 | **DISPROVEN** |
| `0055C6F0` is 25-only; else `ret 4` | **PROVEN** body |
| `0124C208+4` dword is `0055C6F0` | **PARTIAL** (no rdata print; cluster + vtbl-only `E8`) |
| `0124C208+8` accept for 25 | **PARTIAL** (rdata unread; map 25 is present) |
| First-seen type 13 reaches the type-32 **list node** | **PROVEN** |
| First-seen type 13 reaches `0055C6F0` | **PROVEN** if accept is true; accept slot **PARTIAL** |
| Ctor already applies 25 once with no type 13 | **PROVEN** |
| `FrontendInputMap` / `EngineInput` type 13 → action 25 | **MATCH** |
| Live host queues type 13 | **DISPROVEN** — **LEFTOVER** producer |
| Host LMB is type 13 | **DISPROVEN** (`Type4` / `Type6`) |
| `EngineInput.ApplyEvent(TypeMouse)` writes `+176/+180` | **DISPROVEN** — **LEFTOVER** gap vs `0042E5DC` |

---

## Answer

Type 13 is mouse **move**. On first-seen Press Start it becomes
action 25 and walks the same `0055CB10` list that
`0055C650` joined. That is how motion reaches
`UI_MOUSE_POINTER`. It is not how click posts `0xE5`.

The host leftover is the **missing producer** (and the missing
cursor store), not the 13→25 map.

---

## 1. Dump `0055C650` (entire function)

Factory `0041D21B` type 32 (same alloc size as type 0):

```
0041D27C  push 0x184
0041D281  call 00BFEA1A
0041D28F  push edi
0041D290  mov ecx, eax
0041D292  call 0055C650
```

Sole recovered `.text` `E8 0055C650` is that site (`e8.tsv`).
RTTI name: `CMouseCursor@NUISystem`.

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
0055C674  call 0041E5F2
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

Copy-ctor `0055C6A0` is the same register + `0052DA20(25)` +
`[input+184]=widget` via `0041B870`. Dtor `0055C8D0` (thunks
`0055C8B0` `sub ecx,4` / `0055C8C0` `sub ecx,24`) calls input
`vtbl+20(inner)` and zeros `+184`.

`0041B800` is type 0 (`0122F5D4` / inner `0122F5AC`). Type 32
**replaces** both vtbls before register. Type 0 itself does not
join `0055CB10`.

---

## 2. `0052DA20(25)` is local map, then apply

`listing-00500000.txt`:

```
0052DA20  ; ecx = inner, arg = action
          lea  esi, [edi+4]         ; tree at inner+4
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

First-seen ctor insert is new, so `0055C650` applies 25
**during construct**, before any poll. Later type-13 events
do not go through `0052DA20` again; they go through
`0055CB10` → `listener.vtbl+8` then `vtbl+4`.

---

## 3. Dump `0055C6F0` (25-only apply on this cluster)

Immediately after the copy-ctor. `e8.tsv` has **zero**
`0055C6F0` rows (vtbl slot, same shape as type-10
`0054E280`).

```
0055C6F0  mov eax, [esp+4]          ; action
          sub esp, 48
          cmp eax, 25
          mov esi, ecx              ; this = inner
          jne 0055C7FD              ; not 25 → ret 4

          ; snapshot inner+48 / +52
          ; widget = inner-4; vtbl+128(0,0, &input+0xB0)
          ; 009A4EC0 display size via 009BEDC0
          ; clamp / write inner+48 / +52
          ; delta = new - snapshot
          call 0041E5F2
          mov [eax+176], ecx        ; overwrite poll store
          mov [eax+180], edx
0055C7FD  pop esi
          add esp, 48
          ret 4
```

`ecx` is the inner (`esi-4` is the widget). That matches
`0055CB10` / `0052DA20` delivering to `widget+4`.

Action 25 here tracks the pointer sprite. It does **not**
post `0xE5` / `0x126` / 15.

Identity of `0124C208+4` as this body stays **PARTIAL**
until `ExeIndex vtbl 0x0124C208`. The only 25-only
`ret 4` method in the type-32 INT3 island is this one.

---

## 4. Type 13 is how 25 is produced after ctor

`00A03FB0`:

```
00A03FB8  mov [ecx+32], 3
00A03FBF  mov [ecx+40], 0xD         ; type 13
          lea eax, [ecx+12]         ; 12-byte xyz
          ; +24/+28 from ptr
```

Only `.text` call is `00AB5B3D` (same mouse process as
`GetCursorPos` / analog accumulate). Deadzone skip is
above that site; small motion never builds the record.

`0042E3EE` (`sub eax, 13` / `je 0042E5DC`):

```
0042E5DC  mov eax, [ebp-68]         ; record+12
          mov [ebp-20], eax
          mov eax, [ebp-64]         ; record+16
          mov [ebp-16], eax
          call 0041E5F2
          mov [eax+176], ecx
          mov [eax+180], ecx
          call 0041E5F2
          push 25
          jmp 0042E5AB              ; input.vtbl+0 = 0055CB10
```

First-seen `[input+8]==0` → broadcast every node
(`action26-subscribers`). Press Start register order
(DFS post-order):

1. `UI_FRONTEND_BUTTON_INVISIBLE` type 11 (`0055BA20`)
2. `UI_MOUSE_POINTER` type 32 (`0055C650`, map **25**)

Type 11 ctor / activate map is 26 / 31 / 28 / 27 / 32 / 29.
**25 is not** on that set. INVISIBLE action 26 is not a
type-13 path (`invisible-button-e5`).

So a type-13 poll delivers 25 to the mouse node. Type 11
does not steal it as a poster. Broadcast has no `return`,
so even if INVISIBLE accept were true, the mouse still
runs.

Accept `0124C208+8` is **PARTIAL** (no rdata). Generic
map-contains `0052D900` would return true for 25 after
§2. Do not invent that dword.

---

## 5. First-seen Press Start tree

`persist-scan.txt` `#620` `UI_FRONTEND_PRESS_START_MENU`
Type=10, six persist children. Child 6 is
`UI_MOUSE_POINTER` type **32**. Tests assert that widget
is visible on the built Press Start tree.

Type 10 root never registers. Motion does not need it:
type 32 registered itself inside `005331A0` of an
ancestor, before `0054E3D0` returns.

---

## 6. C# leftover

Authority lock:

```
FrontendInputMap.TypeMouse  = 13
FrontendInputMap.ActionMouse = 25
EngineInput.TypeMouse       = 13
EngineInput.ActionMouse     = 25
ActionFromEvent(13, _)      = 25
```

That classify **MATCH**es `0042E5DC`.
`MessageFromWidgets` still posts only on 26 / 28. Action
25 is not a frontend message. **MATCH**.

Host leftover:

| Site | What it does | Native | Class |
| --- | --- | --- | --- |
| `Program.cs` mouse move | `debugCam.Look` if F2-look; **no** `QueueInput` | `00A03FB0` type 13 | **LEFTOVER** producer |
| `Program.cs` LMB | `QueueInput(Type4)` / `Type6` | type 4 / 6 → 26 / 28 | **MATCH** (not type 13) |
| `EngineInput.ApplyEvent(TypeMouse)` | `Dispatch(25)` only | also writes `+176/+180` then `0055CB10` | classify **MATCH**; store **LEFTOVER** |
| Tests | no `TypeMouse` queue | — | no first-seen motion |

`host-input-type4` “`Program.cs` never queues type 4” is
**STALE**: live LMB is now type 4 / 6. Mouse **move** is
still unqueued. Do not map LMB → `TypeMouse`.

`0055C650`’s first apply (ctor `0052DA20(25)`) has **no**
host analog. Isolated construct does not call
`inner.vtbl+4(25)`.

---

## Do not invent

- Type 13 as Press Start click / `0xE5`.
- `0124C208+4` / `+8` rdata dwords without `vtbl` print.
- Host cursor pixels as the `00A03FB0` `+12` xyz.
- Type 11 / 10 as the action-25 mapper on this screen.
- `input+184` as a `0055CB10` node (the node is `widget+4`).
