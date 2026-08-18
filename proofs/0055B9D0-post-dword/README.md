# `0055B9D0` does not choose `+224` vs `+228`; action 26 posts `vtbl+284` (`def+224`)

Investigation only. No production `src/` edits.

Authority: `Fable.exe` listing
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0055AD60` / `0055AF60` / `0055B040` / `0055B520` / `0055B9D0` /
`00558DE0`); `listing-00400000.txt` (`0041E6D3`);
`listing-00580000.txt` (`0059A238` / `00595582`);
`proofs/type38-msg126/README.md`;
`proofs/type11-msg15/README.md`;
`proofs/vtbl284-type11-38/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not re-prove persist CRC `0x53C644E4` → file i32 `0x126` / 15.
Do not treat type-10 `&widget+352` (`0054E2FA`) as this path.

---

## Verdict

**The dword that reaches `0059A238` on type 11/38 action 26 is
`[def+224]`, stored through `vtbl+284` into the list at
`widget+372`, posted by `0055AF60` → `vtbl+524`.**

`0055B9D0` is **not** that poster. It never reads `+224` / `+228`
and never calls UI vtbl+32. `vtbl+320` (`[def+228]`) is a sibling
list and is **not** the argument `0055AF60` pushes.

| Claim | Status |
| --- | --- |
| `0055B9D0` is `cmp arg,25` → outer `vtbl+580`; else `ret 4` | **PROVEN** |
| Action 26 case 0 calls `0055B9D0` **after** click | **PROVEN** |
| `0055B9D0` posts `vtbl+284` (`+224`) or `vtbl+320` (`+228`) | **DISPROVEN** |
| Action 26 click is 0-arg outer `vtbl+584` | **PROVEN** |
| 0-arg body `0055AF60` pushes `[this+372]` then `vtbl+524` | **PROVEN** |
| `[this+372]` is the `0055B040` / `vtbl+284` / `[def+224]` list | **PROVEN** store; slot VA **PARTIAL** (no rdata) |
| `0055AF60` `mov ecx,[eax+524]` is **def+524** (state), not vtbl+524 | **PROVEN** |
| `vtbl+320` (`[def+228]`) is the list `0055AF60` posts | **DISPROVEN** |
| `vtbl+524` walks the list and `0041E6D3` (`&node+8`) | **PROVEN** ABI at `00558DE0`; rdata dword **PARTIAL** |
| Frontend `[0x13B86A0]==0`: `0041E6D3` → `00595582` → UI `vtbl+32` `0059A238` | **PROVEN** |
| Action 27 `vtbl+524([inner+372])` is the same `+224` list | **DISPROVEN** (inner this-adjust → `widget+376`) |

**Answer:** action 26 posts **`vtbl+284` / persist `+224`**
(`0x126` on `UI_ACCEPT_NEW_PROFILE`, 15 on
`UI_FRONTEND_BUTTON_NEW_GAME`). Not `vtbl+320` / `+228`. Not
`0055B9D0`.

---

## 1. `0055B9D0` is the action-25 tail, not a UI post

```
0055B9D0  cmp [esp+4], 25
0055B9D5  jne 0055B9E3            ; ret 4
0055B9D7  mov eax, [ecx-4]
0055B9DA  add ecx, -4             ; inner → outer
0055B9DD  call [eax+580]
0055B9E3  ret 4
```

Callers from `0055AD60` always `push edi` (the action id) with
`ecx` = inner (`widget+4`). Action **26** is not 25, so the
function is a no-op `ret 4`. No `[def+224]`, no `[def+228]`, no
`0059A238`.

Flagging `0055B9D0` as the `+224` vs `+228` chooser is **STALE**.
`audit-messageid-parse` already noted action 26 “sets click state
and calls `0055B9D0`”; that call is the **tail**, not the store
read.

---

## 2. Action 26: `vtbl+584` then `0055B9D0`

`0055AD60` (`listing-00540000.txt`), `ecx` = inner:

```
0055AD66  lea eax, [edi-26]
0055AD69  cmp eax, 6
0055AD6E  ja 0055AE79             ; 0055B9D0 only
0055AD74  jmp [0x55AE88+eax*4]
```

Table dword0 `7B AD 55 00` = `0055AD7B` (action 26):

```
0055AD7B  mov al, [esi+348]       ; inner+348 = widget+352 (u8)
          test al, al
          je  0055AE3D            ; skip click; still 0055B9D0
0055AD89  mov eax, [esi-4]
0055AD8C  lea ecx, [esi-4]        ; outer
0055AD8F  call [eax+584]          ; 0-arg click
          [esi+364] = 1           ; inner+364 = widget+368 armed
          push edi
          call 0055B9D0           ; no-op for 26
```

Case 0 itself has **no** `call [vtbl+524]` and **no**
`push 0x126` / `call 0059A238` (`type38-msg126` **DISPROVEN**
that immediate). The post is inside `vtbl+584`.

`00557850` is `jmp 0055AF60` (same 0-arg shape). Exact rdata
`0124B04C+584` / `01249554+584` stay **PARTIAL**.

If `widget+352==0`, action 26 never enters `0055AF60` and
**nothing** from `+224`/`+228` reaches `0059A238` on that apply.

---

## 3. `0055AF60` posts `widget+372`, not `+228`

`ecx` = **outer** widget:

```
0055AF60  push ecx
          mov esi, ecx
0055AF64  mov eax, [esi+328]
0055AF6C  mov [esi+364], eax      ; outer+364 (not the armed u8)
0055AF72  lea eax, [esp+4]
0055AF77  call [edx+432]          ; def*
0055AF7F  mov ecx, [eax+524]      ; DEF +524 (select-state id)
0055AF8A  call [edx+192]          ; vtbl+192(state)
          ; release the temp def* refcount
0055AFAC  mov ecx, [esi+372]      ; persist list head
0055AFB7  call [eax+524]          ; VTBL +524(list)
0055AFC3  push 28
          call [inner.vtbl+12]    ; locally map 28 after click
          ret
```

Two different `+524`s in one function:

| Site | Base | Meaning |
| --- | --- | --- |
| `0055AF7F` | `[def+524]` | argument to `vtbl+192` |
| `0055AFB7` | `this.vtbl+524` | post the list in `[this+372]` |

Sibling `0055AFD0` is the same shape with `[def+528]` / `[this+392]`
/ subscribe 29. That is **not** action 26.

`0055AF60` never loads `[this+380]` or `[def+228]`.

---

## 4. Ctor: `[def+224]` → `vtbl+284` → `+372`; `[def+228]` → `vtbl+320`

`0055B040` (type-34 vtbl `0124BD2C` still live):

```
[def+224] nonzero → box (0042BE50 / 0042AA29)
          [box] = [def+224]
          call [vtbl+284]          ; 0055B12E
[def+228] nonzero → same box
          call [vtbl+320]          ; 0055B21F
[def+232] → vtbl+288
[def+236] → vtbl+292
```

Four `ret 4` appenders follow the ctor, in **persist-call order**:

| Fn | Writes | Persist arm |
| --- | ---: | --- |
| `0055B520` | list at **`+372`** | first = `vtbl+284` / `[def+224]` |
| `0055B5B0` | list at **`+380`** | second = `vtbl+320` / `[def+228]` |
| `0055B640` | list at **`+376`** | third = `vtbl+288` / `[def+232]` |
| `0055B6D0` | list at **`+392`** | fourth = `vtbl+292` / `[def+236]` |

Each copies `{boxed id, refcount}` to `node+8` of a circular list
(`00BFEA0E` 16-byte node, sentinel at `[list]`).

Dtor `0055B760` frees `+372`, then `+392`, then `+376`, then
`+380` — four lists, not a single `+352` dword (type-10
`0054E4F0`).

rdata dwords for `0124BD2C+284` / `+320` were **not** dumped this
pass → slot VAs **PARTIAL**. The **layout** is **PROVEN** from
`.text`: the list `0055AF60` posts is `+372`, filled by the
`[def+224]` arm. The `[def+228]` arm fills a **different** list
(`+380`).

`+388` on the **outer** object is a **u8** (`0055B4A9`
`mov [esi+388], al`). It is not a message list.

---

## 5. `vtbl+524` → `0041E6D3` → `0059A238`

Type-38 list walker `00558DE0` (`ret 4`, one list arg):

```
00558DE0  mov edi, [esp+8]        ; list*
          je  empty
          mov eax, [edi]          ; sentinel
          mov esi, [eax]          ; first node
          cmp esi, eax
          je  empty
00558DF2  call 0041E5F2           ; input singleton
          lea ecx, [esi+8]        ; pair {boxed id, refcount}
          push ecx
          call [edx+56]           ; 0041E6D3
          mov esi, [esi]
          cmp esi, [edi]
          jne 00558DF2
```

`0055B520` wrote the boxed `[def+224]` at `node+8`. That is the
packet `0059A238` double-derefs (`type38-msg126` §1):

```
0059A281  mov eax, [ebp+8]        ; pair*
0059A284  mov eax, [eax]          ; boxed*
0059A286  mov ecx, [eax]          ; dword0 = message id
```

`0041E6D3` (`listing-00400000.txt`):

```
0041E6E6  mov edi, [ebp+124]      ; same pair*
          ; dead-packet test on [boxed+12]
0041E6FB  mov esi, [0x13B86A0]
0041E701  test esi, esi
0041E703  jne 0041E718            ; in-game: skip UI
0041E705  call 00595582
0041E70C  push edi
0041E70F  call [edx+32]           ; 0059A238
```

Frontend (`[0x13B86A0]==0`): the stored `[def+224]` dword is what
`0059A238` switches on (`0x126` → `00851920`; 15 → `0059A2DA`).

Identity “type-11/38 `vtbl+524` == `00558DE0`” stays **PARTIAL**
(no rdata). ABI match (one list pointer, walk `&node+8`) is
**PROVEN**. `0055AF60` is **not** `vtbl+524`; it is the 0-arg
**caller**.

---

## 6. What is **not** on this hop

| Path | Posts | Class |
| --- | --- | --- |
| Action 26 `vtbl+584` / `0055AF60` | `[widget+372]` = `[def+224]` | **PROVEN** |
| Action 26 tail `0055B9D0` | nothing | **PROVEN** |
| `vtbl+320` / `[def+228]` / `+380` | not `0055AF60` | **DISPROVEN** as the action-26 id |
| `0055ACF0` `push [esi+380]` / `vtbl+524` | the `+228` list | **PROVEN** site; **not** action 26 |
| Action 27 `push [inner+372]` | **`widget+376`** (`[def+232]` list if the §4 map holds) | this-adjust **PROVEN**; field **PARTIAL** |
| Action 30 `push [inner+388]` | **`widget+392`** (`[def+236]` list) | this-adjust **PROVEN** |
| Type-10 `0054E2FA` | `&widget+352` attach id | **DISPROVEN** for type 11/38 persist |

Earlier notes that “action 27 posts persist `+372` / `+224`”
skipped the inner this-adjust (`0055AD60` `ecx` = `widget+4`).
`inner+372` is **not** the list `0055AF60` posts.

`who-posts-15` / C# `MessageFromWidgets` “action 26 posts the
stored id” is **EQUIVALENT** to the `0055AF60` hop when
`widget+352≠0`, not to `0055B9D0`.

---

## 7. C# leftover (do not apply here)

File `0x230364D6` (`+224`) is **0** on Accept / New Game /
INVISIBLE, so action 26 `+372` is empty first-seen.
`0x53C644E4` (`+228`) holds `0x126` / 15 / `0xE5`.
Host `MessageId` stays the `0x53C644E4` scan (`+228`).
`0055ACF0` posts `+380` / `+228`. Not a screen-specific
patch.

If a later rdata dump pins `0124BD2C+284` = `0055B520` and
`+320` = `0055B5B0`, store the runtime pair on a type-34 list
analog of `+372`, not type-10 `+352`.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `0055B9D0` | action==25 → `vtbl+580`; else nop | **PROVEN**; **DISPROVEN** poster |
| `0055AD7B` | action 26: `vtbl+584` then `0055B9D0` | **PROVEN** |
| `0055AF60` | 0-arg click; `vtbl+524([+372])` | **PROVEN** body; slot +584 **PARTIAL** |
| `0055B040` | `[def+224]` → `vtbl+284`; `[def+228]` → `vtbl+320` | **PROVEN** |
| `0055B520` | append pair to `+372` | **PROVEN** body; =`vtbl+284` **PARTIAL** |
| `0055B5B0` | append pair to `+380` | **PROVEN** body; =`vtbl+320` **PARTIAL** |
| `00558DE0` | list → `0041E6D3(&node+8)` | **PROVEN** walker; =`vtbl+524` **PARTIAL** |
| `0041E6D3` | input `vtbl+56`; frontend → UI `vtbl+32` | **PROVEN** |
| `0059A238` | UI `vtbl+32`; id = boxed dword0 | **PROVEN** |
| `widget+372` dword | the id that `0059A238` sees | **DISPROVEN** (it is a **list head**; id is `node+8` boxed `+0`) |

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
  (`0055AD60`, `0055AE88`, `0055AF60`, `0055B040`, `0055B460`,
  `0055B520`, `0055B5B0`, `0055B9D0`, `00558DE0`, `00557850`)
- `listing-00400000.txt` (`0041E6D3`)
- `listing-00580000.txt` (`0059A238`, `00595582`)
- `proofs/type38-msg126/README.md`
- `proofs/type11-msg15/README.md`
- `proofs/vtbl284-type11-38/README.md`
- `proofs/who-posts-0x126/README.md` (vtbl+584 vs `00558DE0` arg mismatch, now resolved: +584 calls +524)
