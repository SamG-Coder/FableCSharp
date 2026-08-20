# Type 11/38 `vtbl+524` after `0055ACF0` `push [this+380]`

Investigation only. No production `src/` edits.

Authority: `Fable.exe` listing
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0055ACF0` / `00558DE0` / `0055B040` / `0055B5B0` / `0054E0B0` /
`00558B90` / `0055B460` / `0054DDB0`);
`listing-00400000.txt` (`0041E6D3`);
`listing-00580000.txt` (`0059A238` / `00595582`);
`.rdata` vtbls `0124B04C` / `01249554` / `0124BD2C` **if present**.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not re-prove persist CRC `0x53C644E4` → def `+228` (`0x126` / 15),
or that action 26 posts `+372` / `+224` via `0055AF60`.
Do not invent a first-seen screen that enters `0055ACF0`.

---

## Verdict

**`0055ACF0` is not `vtbl+524`.** It is a 0-arg **caller**:
`push [this+380]` then `call [this->vtbl+524]`.

| Question | Answer |
| --- | --- |
| Is that slot **`00558DE0`?** | **The only matching body, yes.** ABI / unique walker **PROVEN**. Exact rdata dword **PARTIAL**. |
| Does it **`0041E6D3` → `0059A238`** with the boxed **`+228`** dword? | **Yes**, when the slot is `00558DE0` and `[this+380]` is nonempty. |

`.rdata` slot dwords were **not** present this pass
(`listing-01200000.txt` is still `.text`; `.rdata` VA starts
`0x0122D000`. `read_file` rejects `Fable.exe`. No `ExeIndex vtbl`
dump of `0124B04C` / `01249554`). So:

| VA | Expected slot | File | Dword |
| --- | ---: | ---: | --- |
| `0124B04C` type 38 | `+524` → `0124B258` | `0xE4B258` | **UNREAD** (want `00558DE0`) |
| `01249554` type 11 | `+524` → `01249760` | `0xE49760` | **UNREAD** (want `00558DE0`) |
| `0124BD2C` type 34 | `+524` → `0124BF38` | `0xE4BF38` | **UNREAD** |

Dump: `Fable.ExeIndex vtbl 0x0124B04C 160`,
`vtbl 0x01249554 160`, `vtbl 0x0124BD2C 160`
(slot `[131]`).

`00558DE0` does **not** `E8 0059A238`. It posts `&node+8` through
input **`vtbl+56` = `0041E6D3`**. Frontend
(`[0x13B86A0]==0`) then **`00595582` → UI `vtbl+32` `0059A238`**.
The dword `0059A238` switches on is **`[boxed+0]` = `[def+228]`**,
not the list head at `widget+380`.

---

## 1. `0055ACF0` pushes `+380`, then `this.vtbl+524`

`listing-00540000.txt`, 0-arg (`ret`, not `ret 4`). `ecx` = outer
widget (`01249554` type 11, `0124B04C` type 38, after ctor
overwrite).

```
0055ACF0  push esi
0055ACF1  mov esi, ecx
0055ACF3  mov ecx, [esi+364]
0055ACF9  mov eax, [esi]
0055ACFB  push ecx
0055ACFC  mov ecx, esi
0055ACFE  call [eax+192]          ; SelectState([this+364])
0055AD04  mov edx, [esi+4]
0055AD07  lea ecx, [esi+4]
0055AD0A  push 28
0055AD0C  call [edx+16]           ; inner unsubscribe 28
0055AD0F  mov ecx, [esi+380]
0055AD15  mov eax, [esi]          ; this->vtbl
0055AD17  push ecx
0055AD18  mov ecx, esi
0055AD1A  call [eax+524]          ; VTBL +524(list)
0055AD20  pop esi
0055AD21  ret
```

No `E8` to `00558DE0`, `0041E6D3`, or `0059A238`. The hop in
the question is the indirect at `0055AD1A`.

Sibling `0055AF60` is the same shape with **`[this+372]`**
(`+224` list). Collapsing those two lists is **DISPROVEN**.

Type-11 local `0054DDB0` also `push [esi+380]; call [vtbl+524]`
(gated on `[def+545]`). That is another **caller** of the same
slot, not a second walker.

---

## 2. What `[this+380]` holds (boxed `[def+228]`)

Type-34 ctor `0055B460` zeros the head, then persist-copies
**before** type 11/38 overwrite the vtbl:

```
0055B471  mov [esi], 0x124BD2C
…
0055B49D  mov [esi+380], eax      ; 0
0055B4B5  call 0055B040
0054E0BF  mov [esi], 0x1249554    ; type 11, after persist
00558B9D  mov [esi], 0x124B04C    ; type 38, after persist
```

`0055B040` second arm:

```
0055B15E  mov eax, [edx+228]
0055B164  test eax, eax
0055B166  je  0055B24B            ; 0 → skip; +380 stays 0
          ; 0042BE50 / 0042AA29 box
0055B193  mov ecx, [eax+228]
0055B19D  mov [edx], ecx          ; boxed dword0 = [def+228]
0055B21F  call [edx+320]          ; type-34 vtbl+320
```

`0055B5B0` (`ret 4`) is the only type-34 appender that writes
**`widget+380`**:

```
0055B5B5  mov eax, [edi+380]
          ; allocate circular sentinel if null
0055B5F2  mov [edi+380], esi
0055B602  call 00BFEA0E           ; 16-byte node
0055B607  lea ecx, [eax+8]
0055B617  mov [ecx], edi          ; node+8  = boxed*
0055B61E  mov [ecx+4], edx        ; node+12 = ref*
          ; splice into the ring at [+380]
0055B635  ret 4
```

Identity `0124BD2C+320 == 0055B5B0` stays **PARTIAL** (no rdata).
Layout is **PROVEN** from `.text`: the list `0055ACF0` pushes is
the `[def+228]` arm. Sibling `0055B520` fills `+372` (`[def+224]`)
and is **not** this push.

`widget+380` is a **list head**. Treating that dword as the UI
id is **DISPROVEN**. `00558DE0` `test edi,edi; je` empty: null
head → **no** `0041E6D3`, **no** `0059A238`.

---

## 3. Exact callee of `call [eax+524]` is the `00558DE0` body

`0055AD1A` is `thiscall` + one stdcall arg:

```
push [this+380]
ecx = this
call [vtbl+524]
```

Type-38 cluster walker `00558DE0` (`ret 4`, ignores `ecx`):

```
00558DE0  push edi
00558DE1  mov edi, [esp+8]          ; list* == [widget+380]
00558DE5  test edi, edi
00558DE7  je  00558E09
00558DE9  mov eax, [edi]            ; sentinel
00558DEB  push esi
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
00558E08  pop esi
00558E09  pop edi
00558E0A  ret 4
```

That ABI matches the push. It sits in the type-38 method island
(after inner `00558D90`, before type-34 persist `0055B040`). It
is the **only** body next to the type-11/34/38 ctors that walks a
caller-supplied list and posts `&node+8` through input `vtbl+56`.

Type 11 has **no** local clone of this walk. Its 0-arg posters
(`0054DD50` / `0054DDB0` / `0054DE10`) **call** `vtbl+524`; they
do not implement it. Type-12 cousin `005403EF` is this-relative
(`[list+352]`) and is **not** this slot.

So **`00558DE0` is the callee body.** The vtbl dwords at
`0124B04C+524` / `01249554+524` stay **PARTIAL** until printed.

---

## 4. `0041E6D3` then `0059A238` (boxed `+228`)

Input vtbl `01230134+56` is `0041E6D3` (`FrontendInputMap.InputVtblMessageFn`).
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
0041E718  mov ecx, [ebp+124]
0041E71B  mov edi, [ecx]
0041E71D  mov eax, [edi]            ; boxed dword0
```

`0059A238` (`listing-00580000.txt`) double-derefs the same pair
`0055B5B0` wrote at `node+8`:

```
0059A281  mov eax, [ebp+8]          ; pair*
0059A284  mov eax, [eax]            ; boxed*
0059A286  mov ecx, [eax]            ; dword0 = [def+228]
```

Frontend (`[0x13B86A0]==0`): that boxed dword is what the UI
switch sees (`0x126` / 15 when those defs stored them at `+228`).
In-game the UI forward is skipped; `0041E6D3` continues its own
id switch. That does not un-prove the frontend hop.

`00558DE0` as **`vtbl+524` itself** vs `0041E6D3` as **`+524`**:
the latter is **DISPROVEN**. `0041E6D3` is input `vtbl+56` inside
the walker.

---

## 5. What this is not

| Claim | Class |
| --- | --- |
| `0055ACF0` itself is `vtbl+524` / `00558DE0` | **DISPROVEN** (0-arg caller) |
| `vtbl+524` callee is `0041E6D3` | **DISPROVEN** (`0041E6D3` is walker `vtbl+56`) |
| `0055ACF0` `E8`s `0059A238` | **DISPROVEN** |
| `[this+380]` dword is the UI id | **DISPROVEN** (list head) |
| `0055AF60` `push [+372]` is this hop | **DISPROVEN** (`+224` list) |
| Empty `+380` still reaches `0059A238` | **DISPROVEN** (`00558DE0` `je` ret 4) |
| First-seen Accept / New Game **enters** `0055ACF0` | **UNREAD** here (callers are type-39 wrap `00557AF4` and type-35 tails `0055A726` / `0055A73B`; do not invent) |
| `0124B04C+524` / `01249554+524` printed `00558DE0` | **UNREAD** (rdata not in the listing) |

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `0055ACF0` | 0-arg; `push [+380]; call [vtbl+524]` | **PROVEN** body |
| `0055AD1A` | `this.vtbl+524` | **PROVEN** vtbl slot |
| `0055B040` | box `[def+228]` → `vtbl+320` | **PROVEN** |
| `0055B5B0` | append pair to `+380` | **PROVEN** body; =`vtbl+320` **PARTIAL** |
| `00558DE0` | list → `0041E6D3(&node+8)` | **PROVEN** walker; = type 11/38 `vtbl+524` **PARTIAL** |
| `0041E6D3` | input `vtbl+56` | **PROVEN** |
| `00595582` | frontend UI singleton | **PROVEN** |
| `0059A238` | UI `vtbl+32`; id = boxed dword0 | **PROVEN** |
| `0124B04C+524` | type-38 slot dword | **UNREAD** |
| `01249554+524` | type-11 slot dword | **UNREAD** |
| `0124BD2C+524` | type-34 slot dword | **UNREAD** |

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
  (`0055ACF0`, `00558DE0`, `0055B040`, `0055B460`, `0055B5B0`,
  `0054E0B0`, `00558B90`, `0054DDB0`)
- `listing-00400000.txt` (`0041E6D3`)
- `listing-00580000.txt` (`0059A238`, `00595582`)
- `proofs/0055AF60-callee/README.md` (same walker, `+372` / `+224`)
- `proofs/0055B9D0-post-dword/README.md`
- `proofs/00557AF0-caller/README.md` (`0055ACF0` callers)
- `proofs/plus224-payloads/README.md`
