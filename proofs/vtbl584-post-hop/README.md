# Type-38 action 26: `vtbl+584` / `+524` post hop

Investigation only. No production `src/` edits.

Authority: `Fable.exe` listings
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0055AD60` / `0055AF60` / `00558DE0` / `0055B040` / `0055B520` /
`0054DBC0` / `0054DD50` / `00558B90` / `0054E0B0`),
`listing-00400000.txt` (`0041E6D3`),
`listing-00580000.txt` (`00595582` / `0059A238`);
vtbl immediates `0124B04C` / `01249554` / `0124BD2C`;
`proofs/type38-msg126/README.md`,
`proofs/who-posts-0x126/README.md`,
`proofs/type11-msg15/README.md`,
`proofs/action26-subscribers/README.md`,
`proofs/input-vtbl56-vs-ui32/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**STALE**.

Do not re-prove persist `0x53C644E4` → def `+224` (`0x126` / 15),
Return ≠ poster, or type-10 `0054E2FA` `&+352` → UI `vtbl+32`.

`.rdata` vtbl dwords were **not** read this pass (`read_file`
rejects `Fable.exe`; no `ExeIndex vtbl` run). Slot **callees** are
identified from ABI + unique bodies. Slot **pointers** stay
**PARTIAL** until `vtbl 0x0124B04C 160`.

---

## Verdict

Type-38 action 26 **does** post stored `0x126` on the same click,
not only on a later action 27.

```
type 4 → 0055CB10(26)
  type-38 inner 0055AD60 case 0          ; if [widget+352] ≠ 0
    outer.vtbl+584                       ; 0-arg
      0055AF60
        vtbl+524([widget+372])           ; persist list from 0055B520
          00558DE0
            input.vtbl+56(&node+8)
              0041E6D3
                [0x13B86A0]==0 → 00595582 → UI vtbl+32
                  0059A238               ; id = [pair.heap]
```

| Claim | Status |
| --- | --- |
| `0055AD60` case 0 (action 26) is `call [outer.vtbl+584]` then `[+364]=1` | **PROVEN** |
| That `+584` body is **`0055AF60`** (0-arg; posts `[+372]`) | **PROVEN** ABI / unique click body; rdata dword **PARTIAL** |
| `0055AF60` `push [this+372]; call [vtbl+524]` | **PROVEN** |
| That `+524` body is **`00558DE0`** (1-arg list walk) | **PROVEN** ABI / unique poster; rdata dword **PARTIAL** |
| `00558DE0` calls **`0041E6D3`**, not `0059A238` | **PROVEN** |
| Frontend (`[0x13B86A0]==0`) `0041E6D3` then calls **`0059A238`** | **PROVEN** |
| Action 26 itself `E8 0059A238` / `E8 0041E6D3` / `E8 00595582` | **DISPROVEN** |
| Type-10-style `00595582` + UI `vtbl+32` inside `0055AD60` | **DISPROVEN** |
| Persist `0x126` lives in the `+372` list (`0055B040` → type-34 `vtbl+284` `0055B520`) | **PROVEN** |
| Action 27 is a **second** `vtbl+524([+372])` (armed release) | **PROVEN** |
| Type-11 15 uses the **same** `0055AD60` → `+584` → `+524([+372])` hop | **PROVEN** control flow |
| Type-11 `01249554+584` is also exactly `0055AF60` | **PARTIAL** (local 0-arg `0054DD50` is a competing slim poster) |

**Does this call `0059A238` or `0041E6D3`?** Both, in that order:
**`0041E6D3` is the `+524` callee**; **`0059A238` is the frontend
forward** inside `0041E6D3`. Not a direct UI `vtbl+32` like type-10
`0054E315`.

**Same hop for type-11 15?** Yes: `0054DBC0` → `0055AD60` case 0 →
outer `+584` → `+524([+372])` → `00558DE0` → `0041E6D3` →
`0059A238`(15). File id is persist 15, not `0x126`.

`action26-subscribers` “action 26 **does not** `vtbl+524`” is
**STALE**: case 0 does not call `+524` itself, but `+584` =
`0055AF60` does.

---

## 1. Dump `0055AD60` — action 26 is `vtbl+584` (0-arg)

`ecx` is the **inner** (`widget+4`). `lea ecx,[esi-4]` is the outer
widget (`0124B04C` on type 38, `01249554` on type 11).

```
0055AD60  push esi / edi
0055AD62  mov edi, [esp+12]          ; action
0055AD66  lea eax, [edi-26]
0055AD69  cmp eax, 6
0055AD6E  ja  0055AE79               ; 0055B9D0
0055AD74  jmp [0x55AE88+eax*4]
```

Table dword0 at `0055AE88` is `7B AD 55 00` = **`0055AD7B`**.

```
0055AD7B  mov al, [esi+348]          ; widget+352 click gate
          test al, al
          je  0055AE3D               ; stamp + 0055B9D0 only
0055AD89  mov eax, [esi-4]
0055AD8C  lea ecx, [esi-4]
0055AD8F  call [eax+584]             ; 0 args
0055AD95  mov ecx, [esi+44]
0055AD98  mov [esi+396], ecx
0055ADA1  mov [esi+364], 1           ; arm
0055ADA8  call 0055B9D0
          ret 4
```

Sibling cases (same switch):

| Action | Site | Effect |
| ---: | --- | --- |
| 26 | `0055AD7B` | `vtbl+584`; `[+364]=1` |
| 27 | `0055ADB2` | if armed + debounce: `vtbl+524([+372])` |
| 28 | `0055ADDE` | if armed: `vtbl+588`; `[+364]=0` |
| 31 | `0055AE20` | if hover: `vtbl+524([+388])` |

`0055B9D0` is `cmp arg,25; je [outer.vtbl+580]; ret 4`. **Not** a
UI post.

Zero `E8` to `0059A238` / `0041E6D3` / `00595582` in this function.
The post is the 0-arg `+584` (and the 1-arg `+524` on other cases).

---

## 2. Dump `0055AF60` — this **is** `vtbl+584`

0-arg, `ret` (not `ret 4`). Matches `call [eax+584]`.

```
0055AF60  push ecx / esi
          mov esi, ecx                 ; outer widget
          mov eax, [esi+328]
          mov [esi+364], eax           ; overwritten to 1 by 0055AD60
          lea eax, [esp+4]
          push eax
          call [vtbl+432]              ; CUIDef*
          push [def+524]               ; def field, not a vtbl
          call [vtbl+192]              ; SelectState
          ; release the def box
0055AFAC  mov ecx, [esi+372]
          push ecx
          call [eax+524]               ; post persist list
          lea ecx, [esi+4]
          push 28
          call [inner.vtbl+12]         ; local-map action 28
          ret
```

No other 0-arg body in the type-34/38 cluster posts `[+372]` through
`vtbl+524`. Type 40 (`0124A994`) thunks the same body
(`00557850  jmp 0055AF60`). Type 35 calls it (`0055A5D3`).

Type-38 `.text` after `00558B90` has **no** local slim clone. So
live `0124B04C+584` is `0055AF60` (or a 5-byte `jmp` to it). It is
**not** `00558DE0` (`who-posts-0x126` arg-mismatch: `+584` is 0-arg;
`00558DE0` is `ret 4`).

`ExeIndex vtbl 0x0124B04C 160` slot `[146] +584` at VA `0124B294`
(file `0xE4B294`) should read `0055AF60`. **PARTIAL** until that
dword is printed.

---

## 3. Dump `00558DE0` — this **is** `vtbl+524`

1-arg stdcall (`ret 4`). Matches `push [+372]; call [vtbl+524]`.
Does not use `ecx`/`this`.

```
00558DE0  mov edi, [esp+8]           ; list*  == [widget+372]
          test edi, edi
          je  ret
          mov eax, [edi]             ; circular head
          mov esi, [eax]             ; first node
          cmp esi, eax
          je  empty
00558DF2  call 0041E5F2              ; input singleton
          lea ecx, [esi+8]
          push ecx                   ; &node+8 = pair {heap*, refcnt}
          call [edx+56]              ; 01230134+56 = 0041E6D3
          mov esi, [esi]
          cmp esi, [edi]
          jne 00558DF2
          ret 4
```

Zero `E8 0059A238`. The only `call [reg+56]` in the type-34/38
cluster is this site (`00558DFF`). Type-12’s cousin `005403D2` is
this-relative (`[list+352]`) and is **not** `vtbl+524`.

`0124B04C+524` is VA `0124B258` (file `0xE4B258`), slot `[131]`.
Expected dword `00558DE0`. **PARTIAL** until dumped.

### Why `[+372]` holds `0x126`

Type-38 ctor `00558B90` → type-34 `0055B460` (vtbl `0124BD2C`) →
`0055B040` **before** the overwrite to `0124B04C`.

`0055B040`: `[def+224]` boxed (`0042BE50` / `0042AA29`),
`call [vtbl+284]`. Type-34 store `0055B520` appends a 16-byte
node onto a circular list whose head pointer lives at
**`widget+372`** (ctor zeros that dword). Node layout:

| Off | Field |
| ---: | --- |
| 0 / 4 | next / prev |
| 8 | `heap*` (`[heap]=persist i32`) |
| 12 | refcount* |

`00558DE0` posts `&node+8`. That is the same pair ABI
`0059A238` double-derefs (`[arg] → [heap] → id`).

File `UI_ACCEPT_NEW_PROFILE` type 38: CRC `0x53C644E4` i32
**`0x126`**. No `.text` `mov […], 0x126`.

---

## 4. `0041E6D3` then `0059A238` (not the reverse)

`01230134+56` (`FrontendInputMap.InputVtblMessageFn`):

```
0041E6D3  mov edi, [ebp+124]         ; arg = pair*
          mov eax, [edi]
          mov al, [eax+12]
          je  00426DFC               ; dead packet
0041E6FB  mov esi, [0x13B86A0]       ; game
          test esi, esi
          jne 0041E718               ; in-game: skip UI
0041E705  call 00595582              ; UI [0x13B8B5C]
          push edi
          call [edx+32]              ; 012521A8+32 = 0059A238
```

`0059A238`:

```
0059A281  mov eax, [ebp+8]
0059A284  mov eax, [eax]             ; heap*
0059A286  mov ecx, [eax]             ; id
…                                    ; 0x126 → 00851920
```

Frontend first-seen game singleton is 0, so the hop **does** reach
`0059A238`. Contrast type-10 `0054E315`: that path calls UI
`vtbl+32` **directly** and never `0041E6D3`
(`proofs/input-vtbl56-vs-ui32`).

`0041E6D3` then continues its own in-game switch on the id. That
does not un-prove the UI forward.

---

## 5. Same hop for type-11 15

```
0054E0B0  call 0055B460              ; persist copy while 0124BD2C
          mov [esi], 01249554        ; type-11 outer
          mov [esi+4], 01249530      ; inner apply 0054DBC0
0054DBC0  ; debounce; [def+545]
          je  skip
          call 0055AD60              ; same switch
```

File `UI_FRONTEND_BUTTON_NEW_GAME`: same CRC, i32 **15**.
`0055B520` appends that 15 into **the same** `+372` list.

Action 26 on that widget is therefore:

`0054DBC0` → `0055AD60` case 0 → `01249554+584` → `+524([+372])`
→ `00558DE0` → `0041E6D3` → `0059A238`(15).

Type-11 also has a **local** 0-arg poster:

```
0054DD50  ; if [def+545]
          push [esi+372]
          call [vtbl+524]            ; no SelectState, no map-28
          ret
```

If `01249554+584` is `0054DD50` instead of `0055AF60`, the **post**
is still `+524([+372])` → `00558DE0` → `0041E6D3`. Only the
def-`+524` SelectState / subscribe-28 side effects drop. Classify
the exact type-11 `+584` dword **PARTIAL**. There is no type-11
clone of `00558DE0`; `+524` stays the shared list walk.

Type-11 also requires `[def+545] ≠ 0` before `0055AD60`. That gate
is **not** on type-38 (inner apply **is** `0055AD60`).

---

## 6. Slot map (computed; dwords undumped)

`.rdata` `rva=file=0xE2D000`. Image base `0x00400000`.
Slot index = offset/4.

| Vtbl | Type | `+524` VA (slot 131) | `+584` VA (slot 146) |
| --- | ---: | --- | --- |
| `0124B04C` | 38 | `0124B258` | `0124B294` |
| `01249554` | 11 | `01249760` | `0124979C` |
| `0124BD2C` | 34 (persist-time) | `0124BF38` | `0124BF74` |

Expected (from §2–§3):

| Slot | Expected fn |
| --- | --- |
| type-38 / 34 `+584` | `0055AF60` |
| type-38 / 34 / 11 `+524` | `00558DE0` |
| type-11 `+584` | `0055AF60` **or** `0054DD50` |

Dump: `Fable.ExeIndex vtbl 0x0124B04C 160`,
`vtbl 0x01249554 160`, `vtbl 0x0124BD2C 160`.

---

## 7. What this is not

| Claim | Class |
| --- | --- |
| Action 26 `push 0x126` / `push 15` immediate | **DISPROVEN** |
| Type-10 `0054E280` posts `0x126` / 15 | **DISPROVEN** (`who-posts-0x126` / `who-posts-15`) |
| `00851770` posts `0x126` | **DISPROVEN** |
| Return / type 1 / action 33 posts these ids | **DISPROVEN** |
| Action 26 only arms; first post is action 27 | **STALE** (`action26-subscribers`) |
| `0055AD60` calls `0041E6D3` / `0059A238` | **DISPROVEN** (callees are the vtbl slots) |
| Physical device that builds type 4 | **UNREAD** |

Action **27** remains a real second post of the same `+372` list
(armed + debounce). Type 4 is action **26**, not 27
(RMB / event type 10 → 27; see `type7-action35` / `type6-action28`).
First-seen accept/New Game click is type 4 → this hop.

---

## 8. C#

`FrontendInputMap.MessageFromWidgets` still stand-in posts the
first visible type 10/11/38 `MessageId` on action 26. That
**MATCH**es first-seen ids (`0x126` / 15) and **does not** clone
`00558DE0` / `0041E6D3`. Do not treat type-38 `MessageId` as
widget `+352` (that byte is the click gate). Do not map Return.

Constants already present: `Type38ActionFn=0x0055AD60`,
`Type11ActionFn=0x0054DBC0`, `InputVtblMessageFn=0x0041E6D3`,
`FrontendMessages.UiMessageFn=0x0059A238`. Add when rdata is
printed: `Type38VtblPlus584=0x0055AF60`,
`Type38VtblPlus524=0x00558DE0`.

No production change in this proof.

---

## Sources

- `listing-00540000.txt` (`0055AD60`, `0055AE88`, `0055AF60`,
  `00558DE0`, `0055B040`, `0055B460`, `0055B520`, `00558B90`,
  `0054E0B0`, `0054DBC0`, `0054DD50`, `00557850`)
- `listing-00400000.txt` (`0041E6D3` / `0041E705`)
- `listing-00580000.txt` (`00595582`, `0059A238`)
- `proofs/type38-msg126/README.md`
- `proofs/who-posts-0x126/README.md`
- `proofs/type11-msg15/README.md`
- `proofs/input-vtbl56-vs-ui32/README.md` (Press Start is **not**
  this hop)
