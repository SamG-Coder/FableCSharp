# After action 26, `0055AF60` `push 28` is local-map insert (`vtbl+12`), not `vtbl+16` / `0055ACF0`

Investigation only. No production `src/` edits.

Authority: dump `Fable.exe` `0055AF60` / `0055AD60` case 28
(`0055ADDE`) / table `0x55AE88` / `0055ACF0` / `0055AEB0` /
`0055AEF0` / `0055AF30` / `0054DC30` / `0054DCC0` in
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`;
`0052DA20` / `0052DF20` in `listing-00500000.txt`;
`0042E3EE` `0042E498` in `listing-00400000.txt`;
`proofs/type6-action28/README.md`,
`proofs/action27-release/README.md`,
`proofs/type11-plus352-select/README.md`,
`proofs/0055AF60-callee/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**STALE**.

Do not re-prove type 4 → action 26, type 6 = LMB up, or that
`0055AF60` posts `[widget+372]` through `vtbl+524`.

---

## Verdict

**`0055AF60` does `push 28` / `call [inner.vtbl+12]`, not
`vtbl+16`.** That slot is local-map **subscribe** (`0052DA20`) of
the same action id type 6 (LMB up) applies. It is **not** apply of
28, **not** `0055AD60` case 28, and **not** a hop into
`0055ACF0`.

`0055ACF0` is a **sibling** 0-arg body that does the opposite
local-map call (`inner.vtbl+16(28)` = erase) and then posts
`[widget+380]` (`[def+228]`). No `E8` / `jmp` from `0055AF60` to
it.

| Claim | Status |
| --- | --- |
| `0055AF60` `0055AFC3` `push 28` / `call [edx+12]` on `lea ecx,[esi+4]` | **PROVEN** |
| That slot is inner **`vtbl+16`** | **DISPROVEN** — displacement is `+12` |
| Inner `vtbl+12` is local-map insert `0052DA20` | **PROVEN** shape (same site as type 11/38 enable); rdata dword **PARTIAL** |
| Action 28 is the id `0042E3EE` type 6 (`push 28` at `0042E49D`) | **PROVEN** (`type6-action28`) |
| `0055AF60` `push 28` **applies** `0055AD60` case 28 | **DISPROVEN** (insert only; no `E8 0055AD60`) |
| `0055AD60` case 28 (`eax=2`) is `0055ADDE` | **PROVEN** (`0x55AE88[2]=DE AD 55 00`) |
| Case 28 posts persist / calls `0055ACF0` / `vtbl+524` | **DISPROVEN** — unarm `vtbl+588` or stamp |
| `0055AF60` leads to `0055ACF0` | **DISPROVEN** |
| `0055ACF0` `push 28` is `call [inner.vtbl+16]` then `vtbl+524([+380])` | **PROVEN** |
| Type 38 enable `0055AEB0` already maps 28 | **DISPROVEN** (26, 31, 27, 32 only) |
| Type 11 activate `0054DC30` already maps 28 | **PROVEN** (if parent `+545`) |
| First-seen type 11/38 enter `0055AF60` on the first action 26 | **UNREAD** as a live hit — ctor `[widget+352]=0` **skips** (`type11-plus352-select`) |
| Inner `vtbl+12` / `+16` rdata dwords | **PARTIAL** (no `.rdata` dump) |

**Is that subscribe of action 28 (LMB up type 6)?** Yes: same
numeric action, local map insert. Not the LMB-up apply itself.

**First-seen effect on type 11/38?** None from this `push 28`
until a selected (`[widget+352]≠0`) action 26 actually enters
`0055AF60`. Type 11 may already accept 28 from activate. Type 38
does not map 28 until this insert.

**Lead to `0055ACF0`?** No.

---

## 1. Dump `0055AF60` — `push 28` is inner `vtbl+12`

`ecx` = **outer** widget. 0-arg (`ret`).

```
0055AF60  push ecx
          push esi
          mov esi, ecx
0055AF64  mov eax, [esi+328]
0055AF6C  mov [esi+364], eax          ; outer+364 = last state (dword)
          call [vtbl+432]             ; CUIDef*
0055AF7F  mov ecx, [eax+524]          ; DEF +524
          call [vtbl+192]             ; SelectState
0055AFAC  mov ecx, [esi+372]
          push ecx
          call [this.vtbl+524]        ; post +372 list
0055AFBD  mov edx, [esi+4]            ; inner vtbl
0055AFC0  lea ecx, [esi+4]            ; inner this
0055AFC3  push 28
0055AFC5  call [edx+12]               ; NOT +16
          pop esi / pop ecx
          ret
```

`edx` is `[outer+4]`, the inner object’s vtbl. `+12` is slot 3
(0-based). `+16` would be the next dword. The listing is
`call [edx+12]`.

Sibling `0055AFD0` is the same shape with `push 29` /
`call [edx+12]`. Type-38 disable `0055AF30` is the **erase**
pair: `push 28` / `call [eax+16]`.

---

## 2. `vtbl+12` = subscribe; `vtbl+16` = erase

Type 38 enable / disable (`listing-00540000.txt`):

```
0055AEB0  call 0055BAE0
          inner.vtbl+12(26)
          inner.vtbl+12(31)
          inner.vtbl+12(27)
          inner.vtbl+12(32)
          ret                         ; 28 is absent

0055AEF0  call 0055B9A0
          inner.vtbl+16(26)
          inner.vtbl+16(31)
          inner.vtbl+16(27)
          inner.vtbl+16(32)
          ret
```

Type 11 activate / deactivate:

```
0054DC30  if [def+545]:
            vtbl+192(3)
            inner.vtbl+12(26, 31, 28, 27, 32, 29)

0054DCC0  if [def+545]:
            vtbl+192(4)
            inner.vtbl+16(26, 31, 28, 27, 32, 29)
```

Insert body used at those `+12` sites (`listing-00500000.txt`):

```
0052DA20  ; ecx = inner, arg = action
          lea esi, [edi+4]            ; map at inner+4
          call 0052DF20               ; tree find by key
          call 0052E230               ; insert if missing
          cmp [esp+28], 25
          jne  ret 4
          ; action 25 only: immediately call inner.vtbl+4(25)
```

Action 28 takes the `jne` — **insert only**. No apply.

`action26-subscribers` already classified inner `vtbl+12` as
`0052DA20`, not `0055CB10`. This `push 28` is that same local
map, not a second `0055CB10` register.

---

## 3. Action 28 is LMB-up’s id, and case 28 is unarm

`0042E498` (`listing-00400000.txt`): type 6 → `push 28` →
`0055CB10`. Physical producer is LMB **up** (`type6-action28`).

`0055AD60` (`ecx` = inner):

```
0055AD66  lea eax, [edi-26]
          cmp eax, 6
          ja  0055AE79
          jmp [0x55AE88+eax*4]
```

Table dwords (`action27-release`; listing junk-decode of
`0055AE88`):

| `eax` | Action | Dest |
| ---: | ---: | --- |
| 0 | 26 | `0055AD7B` |
| 1 | 27 | `0055AE01` |
| 2 | **28** | **`0055ADDE`** |
| 3 | 29 | `0055AE53` |
| 4 | 30 | `0055AE79` |
| 5 | 31 | `0055ADB2` |
| 6 | 32 | `0055AE20` |

Case 28:

```
0055ADDE  mov al, [esi+364]           ; armed u8 (inner+364)
          test al, al
          je  0055AE70                ; unarmed → stamp +44 → +396
          lea ecx, [esi-4]
          call [outer.vtbl+588]       ; 0-arg cancel
          mov [esi+364], 0
          jmp 0055AE70
0055AE70  [esi+396] = [esi+44]
0055AE79  push edi
          call 0055B9D0               ; 28 ≠ 25 → ret 4
          ret 4
```

No `E8 0055AF60`. No `E8 0055ACF0`. No `push [+372]` /
`push [+380]`. No `call [vtbl+524]`. Persist `0x126` / 15 is
**not** this case (`type6-action28`; post is action 26’s
`+584` / `0055AF60`).

`type6-action28` assigned case 28 from **code order** as
unarm — that **body** is still `0055ADDE`. Table slot **2**
confirms it. The stale 27→`0055ADB2` map in that note does not
move case 28.

---

## 4. First-seen type 11 / 38

Action 26 only reaches `0055AF60` when `[inner+348]`
(`widget+352` u8) is nonzero:

```
0055AD7B  mov al, [esi+348]
          test al, al
          je  0055AE3D                ; stamp + 0055B9D0; no +584
          lea ecx, [esi-4]
          call [eax+584]              ; 0055AF60
          [esi+364] = 1
          call 0055B9D0
```

Ctor `0055BA20` / `0055B460` leaves `[widget+352]=0`
(`type11-plus352-select`). Activate / enable do **not** write
it. The only recovered `=1` is `0055C0DE` inside hit-test
`0055BF10`. So:

| Object | Local 28 before first click | If first 26 runs with `+352=0` | After a later selected 26 |
| --- | --- | --- | --- |
| Type 11 (`0054DBC0` → `0055AD60`) | **yes**, `0054DC30` `vtbl+12(28)` if parent `+545` | no `0055AF60`; 28 already mapped | `vtbl+12(28)` again (idempotent insert) |
| Type 38 (`0055AD60`) | **no** (`0055AEB0` omits 28) | no `0055AF60`; type 6 still unmapped | **first** local 28 |

First-seen `[inner+364]=0`. Even if type 6 apply runs (type 11
map, or type 38 after this insert), case 28 takes
`je 0055AE70` and posts nothing.

First-seen `[def+224]` on Accept / New Game / INVISIBLE is 0, so
when `0055AF60` *does* run the `+372` list is empty — the
`push 28` still happens. That is still not a UI message.

Press Start type 11 is the first `0055CB10` node
(`action26-subscribers`). New Profile type 38
`UI_ACCEPT_NEW_PROFILE` is a later node. Neither first-seen
click is this `push 28` until `+352` is set.

---

## 5. `0055ACF0` is not on this path

```
0055ACF0  mov esi, ecx                ; outer
          push [esi+364]
          call [vtbl+192]             ; SelectState(outer+364)
          lea ecx, [esi+4]
          push 28
          call [inner.vtbl+16]        ; ERASE 28
          push [esi+380]
          call [vtbl+524]             ; post +380 / [def+228]
          ret
```

Contrast with `0055AF60`:

| | `0055AF60` (action 26 `+584`) | `0055ACF0` |
| --- | --- | --- |
| SelectState | `[def+524]` | `[this+364]` |
| Local 28 | **`vtbl+12` insert** | **`vtbl+16` erase** |
| Post | `[+372]` / `[def+224]` | `[+380]` / `[def+228]` |

`.text` callers of `0055ACF0` in this listing: `00557AF4`
(other cluster; `TEXT_GUI_PRESS_CONTROL`) and tails of
`0055A660` (`jmp 0055ACF0` at `0055A726` / `0055A73B`, type
35/41 walk). **None** is `0055AF60` or `0055AD60`.

`00557850` is `jmp 0055AF60` (alias), not `0055ACF0`.

C# already splits them: `FrontendInputMap.Type34ClickFn =
0055AF60` (`+372`) vs `Plus228PostFn = 0055ACF0` (`+380`).
`MessageFromAction(28)` stays null.

---

## 6. C# leftover (do not apply here)

| Site | Native | Host |
| --- | --- | --- |
| type 6 → 28 | `0042E49D` | `ActionType6=28` **MATCH** |
| 26 click then map 28 | `0055AFC3` `vtbl+12` | host has no local-map insert **LEFTOVER** |
| 28 → `0xE5` / `0x126` / 15 | no on 11/38 | `MessageFromWidgets` only action 26 **MATCH** |
| `0055AF60` → `0055ACF0` | no | already separate constants **MATCH** |

Do **not** treat `0055AF60` `push 28` as `vtbl+16`. Do **not**
post persist from action 28. Do **not** route action 26 through
`0055ACF0`.

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
  (`0055AF60` `0055AFC3`, `0055ACF0` `0055AD0A`, `0055AD60` /
  `0055ADDE` / `0x55AE88`, `0055AEB0`, `0055AEF0`, `0055AF30`,
  `0054DC30`, `0054DCC0`, `00557AF4`, `0055A726`)
- `listing-00500000.txt` (`0052DA20`, `0052DF20`)
- `listing-00400000.txt` (`0042E498` `push 28`)
- `proofs/type6-action28/README.md` (type 6 = LMB up → 28; case
  28 = unarm; type 38 enable omits 28)
- `proofs/action27-release/README.md` (table dwords; 28 =
  `0055ADDE`)
- `proofs/type11-plus352-select/README.md` (`+352` gate)
- `proofs/0055AF60-callee/README.md` (`+372` post)
- `proofs/0055B9D0-post-dword/README.md` (`0055ACF0` = `+380`)
- `src/Fable.Game/FrontendInputMap.cs`
