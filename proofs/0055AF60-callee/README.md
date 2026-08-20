# `0055AF60` `[eax+524]` after `[this+372]`: vtbl slot, not def field

Investigation only. No production `src/` edits.

Authority: `Fable.exe` listing
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0055AF60` / `0055B040` / `0055B520` / `00558DE0`);
`listing-00400000.txt` (`0041E6D3`);
`listing-00580000.txt` (`0059A238` / `00595582`);
`proofs/0055B9D0-post-dword/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not invent screen-specific posters. This note is only the
`0055AF60` indirect call and the boxed `[def+224]` payload.

---

## Verdict

`0055AF60` has **two** `+524` sites. They are **not** the same
kind of field.

| Site | Instruction | Base | Kind |
| --- | --- | --- | --- |
| `0055AF7F` | `mov ecx, [eax+524]` | `*CUIDef` from `vtbl+432` | **def/state field** |
| `0055AFB7` | `call [eax+524]` | `[this]` after `push [this+372]` | **vtbl slot** |

The hop in the question (`push [this+372]` then `call [eax+524]`)
is **`this.vtbl+524`**. It is **DISPROVEN** as a def/state load.

Exact callee of that slot is **not** a `.text` `E8`. It is
`[this->vtbl+524]`.

| VA | Role vs `0055AFB7` | Class |
| --- | --- | --- |
| **`00558DE0`** | 1-arg list walker that matches the push; only cluster body that then hits input `vtbl+56` | **PROVEN** ABI / unique body; rdata dword **PARTIAL** |
| **`0041E6D3`** | input `vtbl+56` **inside** `00558DE0` (`&node+8`) | **PROVEN** as walker callee; **DISPROVEN** as `+524` itself |
| **`0059A238`** | UI `vtbl+32` **inside** `0041E6D3` when `[0x13B86A0]==0` | **PROVEN** as frontend forward; **DISPROVEN** as `+524` itself |

**Does the hop reach `0041E6D3` / `0059A238` with the boxed
`[def+224]` dword?** **Yes**, when `vtbl+524` is the `00558DE0`
walker and `[this+372]` is the list `0055B520` filled from
`0055B040`’s first persist arm. The dword `0059A238` switches on
is **`[node+8][0]`**, not the list-head pointer at `widget+372`.

---

## 1. Two `+524`s in `0055AF60` (do not collapse)

`ecx` = outer widget. 0-arg (`ret`, not `ret 4`).

```
0055AF60  push ecx
0055AF61  push esi
0055AF62  mov esi, ecx
0055AF64  mov eax, [esi+328]
0055AF6A  mov edx, [esi]
0055AF6C  mov [esi+364], eax
0055AF72  lea eax, [esp+4]
0055AF76  push eax
0055AF77  call [edx+432]          ; out CUIDef**
0055AF7D  mov eax, [eax]          ; def*
0055AF7F  mov ecx, [eax+524]      ; DEF +524
0055AF85  mov edx, [esi]
0055AF87  push ecx
0055AF88  mov ecx, esi
0055AF8A  call [edx+192]          ; SelectState(def+524)
          ; release the temp def* refcount
0055AFAC  mov ecx, [esi+372]      ; list head
0055AFB2  mov eax, [esi]          ; this->vtbl
0055AFB4  push ecx
0055AFB5  mov ecx, esi
0055AFB7  call [eax+524]          ; VTBL +524(list)
0055AFBD  mov edx, [esi+4]
0055AFC0  lea ecx, [esi+4]
0055AFC3  push 28
0055AFC5  call [edx+12]
0055AFC8  pop esi
0055AFC9  pop ecx
0055AFCA  ret
```

`0055AF7F` cannot be a vtbl load: `eax` is the object returned by
`vtbl+432`, then `[eax]` again — a `CUIDef*`, not `[this]`.
Sibling `0055AFD0` is the same split with `[def+528]` /
`[this+392]` / local-map 29. That sibling is **not** this hop.

`0055AF60` never loads `[this+380]` or `[def+228]`.

---

## 2. What `+372` holds (boxed `[def+224]`, not the raw dword)

Ctor copy `0055B040` (`listing-00540000.txt`):

```
0055B052  call [eax+432]            ; CUIDef*
0055B068  mov ecx, [eax+224]
0055B06E  test ecx, ecx
0055B075  je  0055B15A              ; 0 → skip first arm
          ; 0042BE50 / 0042AA29 box
0055B0A2  mov ecx, [eax+224]
0055B0AC  mov [edx], ecx            ; boxed dword0 = [def+224]
0055B12E  call [edx+284]
```

Next persist arm is `[def+228]` → `vtbl+320` (`0055B21F`). That
list is **not** what `0055AF60` pushes.

`0055B520` (`ret 4`) is the appender that writes **`widget+372`**:

```
0055B525  mov eax, [edi+372]
          ; allocate circular sentinel if null
0055B562  mov [edi+372], esi
0055B572  call 00BFEA0E             ; 16-byte node
0055B577  lea ecx, [eax+8]
0055B581  mov edx, [esp+16]         ; pair {boxed*, ref*}
0055B585  mov edi, [edx]
0055B587  mov [ecx], edi            ; node+8  = boxed*
0055B58E  mov [ecx+4], edx          ; node+12 = ref*
          ; splice into the ring at [+372]
0055B5A5  ret 4
```

Identity `vtbl+284 == 0055B520` stays **PARTIAL** (no `.rdata`
dump of `0124BD2C+284`). Layout is **PROVEN** from `.text`: the
first persist arm fills the list `0055AF60` posts. Sibling
`0055B5B0` fills `+380` (`[def+228]`).

`widget+372` is a **list head**. Treating that dword as the
message id is **DISPROVEN**.

---

## 3. Exact callee of `call [eax+524]`

`0055AFB7` is `thiscall` + one stdcall arg:

```
push [this+372]
ecx = this
call [vtbl+524]
```

No `E8` in `0055AF60` to `00558DE0`, `0041E6D3`, or `0059A238`.

Type-34/38 cluster walker `00558DE0` (`ret 4`, ignores `ecx`):

```
00558DE0  push edi
00558DE1  mov edi, [esp+8]          ; list* == [widget+372]
00558DE5  test edi, edi
00558DE7  je  00558E09
00558DE9  mov eax, [edi]            ; sentinel
00558DEC  mov esi, [eax]            ; first node
00558DEE  cmp esi, eax
00558DF0  je  00558E08
00558DF2  call 0041E5F2             ; input singleton
00558DF7  mov edx, [eax]
00558DF9  lea ecx, [esi+8]          ; pair {boxed*, ref*}
00558DFC  push ecx
00558DFD  mov ecx, eax
00558DFF  call [edx+56]             ; 0041E6D3
00558E02  mov esi, [esi]
00558E04  cmp esi, [edi]
00558E06  jne 00558DF2
00558E0A  ret 4
```

That ABI matches the push. It is the only body next to the
type-34/38 ctors that walks a caller-supplied list and posts
`&node+8` through input `vtbl+56`.

`.rdata` slot dwords were **not** printed this pass
(`0124B04C+524` = VA `0124B258`, `0124BD2C+524` = `0124BF38`).
So **`00558DE0` is the exact callee body; the vtbl dword is
PARTIAL.**

`0055AF60` is **not** `vtbl+524`. It is the 0-arg **caller**.

---

## 4. `0041E6D3` then `0059A238` (frontend)

`0041E6D3` (`listing-00400000.txt`):

```
0041E6D3  push ebp
0041E6E6  mov edi, [ebp+124]        ; pair*
0041E6EC  mov eax, [edi]
0041E6EE  mov al, [eax+12]
0041E6F3  je  00426DFC              ; dead packet
0041E6FB  mov esi, [0x13B86A0]
0041E701  test esi, esi
0041E703  jne 0041E718              ; in-game: skip UI
0041E705  call 00595582
0041E70C  push edi
0041E70F  call [edx+32]             ; 0059A238
```

`0059A238` (`listing-00580000.txt`) double-derefs the same pair
`0055B520` wrote at `node+8`:

```
0059A281  mov eax, [ebp+8]          ; pair*
0059A284  mov eax, [eax]            ; boxed*
0059A286  mov ecx, [eax]            ; dword0 = [def+224]
```

Frontend (`[0x13B86A0]==0`): that boxed dword is what the UI
switch sees. In-game the UI forward is skipped; `0041E6D3`
continues its own id switch. That does not un-prove the frontend
hop.

---

## 5. What this is not

| Claim | Class |
| --- | --- |
| `0055AFB7` `[eax+524]` is `[def+524]` | **DISPROVEN** (`eax` is `this->vtbl`) |
| `0055AF7F` `[eax+524]` is `vtbl+524` | **DISPROVEN** (`eax` is `CUIDef*`) |
| `0055AF60` itself is `0059A238` / `0041E6D3` | **DISPROVEN** |
| `vtbl+524` callee is `0041E6D3` | **DISPROVEN** (`0041E6D3` is `vtbl+56` of the walker) |
| `widget+372` dword is the UI id | **DISPROVEN** (list head) |
| `[def+228]` / `+380` is this push | **DISPROVEN** |
| Screen X is the unique poster through this hop | **UNREAD** here (do not invent) |

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `0055AF60` | 0-arg click; `push [+372]; call [vtbl+524]` | **PROVEN** body |
| `0055AF7F` | `[def+524]` → `vtbl+192` | **PROVEN** def field |
| `0055AFB7` | `this.vtbl+524` | **PROVEN** vtbl slot |
| `0055B040` | box `[def+224]` → `vtbl+284` | **PROVEN** |
| `0055B520` | append pair to `+372` | **PROVEN** body; =`vtbl+284` **PARTIAL** |
| `00558DE0` | list → `0041E6D3(&node+8)` | **PROVEN** walker; =`vtbl+524` **PARTIAL** |
| `0041E6D3` | input `vtbl+56` | **PROVEN** |
| `0059A238` | UI `vtbl+32`; id = boxed dword0 | **PROVEN** |

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
  (`0055AF60`, `0055AFD0`, `0055B040`, `0055B520`, `0055B5B0`,
  `00558DE0`)
- `listing-00400000.txt` (`0041E6D3`)
- `listing-00580000.txt` (`0059A238`, `00595582`)
- `proofs/0055B9D0-post-dword/README.md`
- `proofs/vtbl584-post-hop/README.md` (slot map; rdata still undumped)
