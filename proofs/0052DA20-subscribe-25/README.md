# 0052DA20(25) from type-32 ctor 0055C650: which list?

Investigation only. No production `src/` edits.

Question: `0052DA20(25)` from type-32 ctor `0055C650`. What list
does it append? First-seen Press Start node order vs INVISIBLE?
Host leftover?

Authority: dump `Fable.exe` `0055C650` in
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`;
`0052DA20` / `0052DF20` / `0052E230` / `0052D900` in
`listing-00500000.txt`;
`0055BA20` (type 11) in `listing-00540000.txt`;
`implementer/frontend/persist-scan.txt` `#620`;
`src/Fable.Client/Program.cs`;
`src/Fable.Game/FrontendInputMap.cs` / `EngineInput.cs`;
`proofs/0055C650-type32-ctor/README.md`;
`proofs/mouse-pointer-action25/README.md`;
`proofs/action26-subscribers/README.md`;
`proofs/type10-subscribe-first/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH**.

Do not re-prove type 4 → `push 26`, type 13 ctor `00A03FB0`,
`0055CB10` broadcast shape, or `0124C208+4`/`+8` rdata dwords.

---

## Verdict

**Local action-id set on the type-32 inner, not `0055CB10`.**
`0052DA20(25)` inserts **25** into `std::set`-shaped tree at
`inner+4` (`widget+8`). The `0055CB10` listener list is a
**different** append: ctor `input.vtbl+8(inner)` **before** this
call. First-seen Press Start that list is INVISIBLE then mouse.
Host leftover is the missing type-13 producer (and ctor apply),
not the 13→25 id pair.

| Claim | Status |
| --- | --- |
| `0055C650` `push 25` / `mov ecx,edi` / `call 0052DA20` (`edi=widget+4`) | **PROVEN** |
| `0052DA20` inserts the action id into `inner+4` BST (`0052DF20` find / `0052E230` insert) | **PROVEN** |
| That tree is the listener list `0055CB10` (`input+4` / `input+12`) | **DISPROVEN** |
| New insert of **25** also `inner.vtbl+4(25)` (ctor apply, no type 13) | **PROVEN** |
| Other action ids: insert only (no apply) | **PROVEN** (`cmp [arg],25` / `jne`) |
| Type 11 `0055BA20` calls `0052DA20(25)` | **DISPROVEN** (register only; 26-set is later activate) |
| First-seen Press Start `0055CB10` node 1 = `UI_FRONTEND_BUTTON_INVISIBLE` type 11 | **PROVEN** |
| First-seen Press Start `0055CB10` node 2 = `UI_MOUSE_POINTER` type 32 | **PROVEN** |
| `0052DA20(25)` is what puts the mouse **after** INVISIBLE on `0055CB10` | **DISPROVEN** (`vtbl+8` does that) |
| `FrontendInputMap.TypeMouse=13` / `ActionMouse=25` | **MATCH** `0042E5DC` |
| Live host `QueueInput` type 13 | **DISPROVEN** — **LEFTOVER** producer |
| Host ctor analog of `0052DA20(25)` / `inner.vtbl+4(25)` | **DISPROVEN** — **LEFTOVER** |

---

## Answer

`0052DA20(25)` appends **action 25** to the **type-32 inner’s
local accept-set** (`inner+4` tree). It does **not** append a
node to the input `0055CB10` circular list.

On first-seen Press Start the `0055CB10` order is:

1. **INVISIBLE** type 11 (`0055BA20` `input.vtbl+8`)
2. **mouse** type 32 (`0055C650` `input.vtbl+8`)

`0052DA20(25)` runs only on the mouse inner, after that
register, and only mutates that inner’s id set. INVISIBLE is
not on that set.

Host leftover: `Program.cs` never `QueueInput(TypeMouse)`.
Mouse move is F2-look only. Classify 13→25 **MATCH**es native;
the producer (and ctor apply) is missing.

---

## 1. Ctor site (`0055C650`)

```
0055C65E  lea edi, [esi+4]          ; inner = widget+4
0055C674  call 0041E5F2             ; input*
0055C67B  push edi
0055C67E  call [edx+8]              ; input.vtbl+8(inner) → 0055CB10 list
0055C681  push 25
0055C683  mov ecx, edi              ; this = inner, not input
0055C685  call 0052DA20             ; local map 25
0055C68A  call 0041E5F2
0055C68F  mov [eax+184], esi        ; input+184 = widget (not a list node)
```

Two appends, two objects. `ecx` for `0052DA20` is **inner**.
`input+184` is the cursor widget pointer, **not** the
`0055CB10` node (`action26-subscribers`: node `listener` is
`widget+4`).

---

## 2. `0052DA20` body — local set, not `0055CB10`

`listing-00500000.txt` entire function (`ret 4` / INT3):

```
0052DA20  sub esp, 12
          mov edi, ecx              ; inner
          lea esi, [edi+4]          ; set object at inner+4
          call 0052DF20             ; find(arg)
          mov ebx, [esi]            ; header*
          call 0052E230             ; insert-if-missing
          cmp [esp+28], 25
          jne 0052DA65              ; other ids: insert only
          cmp [esp+12], ebx
          jne 0052DA65              ; find == header? else already present
          push 25
          mov ecx, edi
          call [edx+4]              ; inner.vtbl+4(25)
          ret 4
```

`0052DF20` / `0052E230` walk `[header+4]` with `key` at
`node+16`, children `+8` / `+12`, `setl` / `jl` on the
integer — MSVC `std::set<int>` shape. `0052D900` (contains)
uses the same `inner+4` header.

`0055CB10` is `input.vtbl+0`: circular 12-byte nodes
(`next`/`prev`/`listener`) at `input+4` / `input+12`.
`0052DA20` never loads `0041E5F2`, never writes `input+4/+12`,
never calls `0055CF00`.

First-seen ctor insert is new (`find` returns header) →
construct applies 25 once with **no** type-13 event. Later
motion is `0042E5DC` `push 25` → `0055CB10`, not another
`0052DA20`.

---

## 3. Press Start order vs INVISIBLE

`persist-scan.txt` `#620` `UI_FRONTEND_PRESS_START_MENU`
Type=10, `Children=6`. Register is DFS **post-order** of
ctors that call `input.vtbl+8` (11 / 32 / 33 / 34 / 37 / 38).
Append is before sentinel (`0055CF00` / `0055CE90`) → first
register is first `0055CB10` node.

| # | Name | Type | `vtbl+8`? | `0052DA20(25)`? |
| --- | --- | ---: | --- | --- |
| 1 | `UI_BLENDING_BACKGROUNDS_FORREST` | 5 | no | no |
| 2 | `UI_TITLE` | 5 | no | no |
| 3 | `UI_PRESS_START_SWAP` → `UI_PRESS_START_TEXT` | 18 / 6 | no | no |
| 4 | `UI_FRONTEND_LIST_PRESS_START_MENU` | 12 | no | no |
|  | → **`UI_FRONTEND_BUTTON_INVISIBLE`** | **11** | **yes** (`0055BA20`) | **no** |
| 5 | `UI_LEGAL_TEXT` | 6 | no | no |
| 6 | **`UI_MOUSE_POINTER`** | **32** | **yes** (`0055C650`) | **yes** (own tree) |

`0055BA20` ends at `input.vtbl+8(inner)` / `ret 4`. No
`push 25`. Type 11 maps 26 / 31 / 28 / 27 / 32 / 29 later
(`0054DC30` activate), each on **that** inner’s set.

So vs INVISIBLE:

- **`0055CB10`:** INVISIBLE first, mouse second.
- **`0052DA20(25)`:** mouse-only local set. INVISIBLE is not
  a peer on that tree.

Type 10 / 12 / 5 / 18 / 6 on this tree do not register
(`type10-subscribe-first`). No type 37 / 38 / 33 here.

---

## 4. Host leftover

Classify **MATCH**es native:

```
FrontendInputMap.TypeMouse  = 13
FrontendInputMap.ActionMouse = 25
EngineInput.TypeMouse       = 13
EngineInput.ActionMouse     = 25
ActionFromEvent(13, _)      = 25
```

Live `Fable.Client` `QueueInput` sites: keys (`TypeKey`),
LMB down (`Type4`), LMB up (`Type6`). `MouseMove` only
`debugCam.Look` when F2-look is on. No `QueueInput(TypeMouse)`.

`EngineInput.ApplyEvent(TypeMouse)` is `Dispatch(25)` only —
missing native `+176/+180` then `0055CB10`. Ctor
`0052DA20(25)` / `inner.vtbl+4(25)` has no host analog
(no local action-id set in `Fable.Game`).

The leftover is the **missing producer** (and cursor store /
ctor apply), not the 13→25 map. Do not map LMB → type 13.

---

## Do not invent

- `0052DA20` as `0055CB10` subscribe.
- `input+184` as the `0055CB10` listener.
- Type 13 as Press Start click / `0xE5`.
- Type 10 / 12 as first-seen list nodes.
- `0124C208+4` / `+8` or `01230134+8` rdata dwords.
