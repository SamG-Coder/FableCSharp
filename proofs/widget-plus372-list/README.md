# `widget+372` is a circular pair list, not `{id, fn}`

Investigation only. No production `src/` edits.

Authority: `Fable.exe` listing
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0055B040` / `0055B460` / `0055B520` / `0055AF60` / `00558DE0` /
`0055B760`); `listing-00400000.txt` (`0042BE50` / `0042AA29`);
`proofs/0055B9D0-post-dword/README.md`.

Do not re-prove persist CRC `0x53C644E4` → file i32 `0x126` / 15,
or that action 26 reaches `0055AF60`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

---

## Verdict

| Claim | Status |
| --- | --- |
| `0055B520` appends one 16-byte node onto the list at **`widget+372`** | **PROVEN** |
| Node payload is **`{boxed*, refcount*}`** at `+8` / `+12` | **PROVEN** |
| Payload is raw `{id, fn}` | **DISPROVEN** |
| Persist `[def+224]` is dword0 of the `0042BE50` boxed object | **PROVEN** |
| `0055AF60` walks `+372` | **DISPROVEN** (it `push [+372]` then `vtbl+524`) |
| The walk is `00558DE0` (`&node+8` → input `vtbl+56`) | **PROVEN** body; `=vtbl+524` **PARTIAL** (no rdata) |
| Ctor zeros `+372` before persist | **PROVEN** |
| `[def+224]==0` never calls `0055B520` | **PROVEN** |
| First-seen empty list is **`[+372]==0`** (no header), not an empty sentinel | **PROVEN** |

**Answer:** record is **not** `{id, fn}`. It is a circular
`{next, prev, boxed*, refcount*}`. `0055AF60` does **not** walk;
it posts the head. When persist `+224` is 0 the head stays **0**.

---

## 1. Ctor: `+372` starts at 0

`0055B460` (`listing-00540000.txt`), `eax` already 0:

```
0055B485  mov [esi+364], eax
0055B48B  mov [esi+368], al
0055B491  mov [esi+372], eax      ; list head = NULL
0055B497  mov [esi+376], eax
0055B49D  mov [esi+380], eax
0055B4A3  mov [esi+384], eax
0055B4A9  mov [esi+388], al
0055B4AF  mov [esi+392], eax
0055B4B5  call 0055B040           ; persist copy
```

Four list heads. This note is only `+372`.

---

## 2. `[def+224]==0` never reaches `0055B520`

`0055B040`:

```
0055B068  mov ecx, [eax+224]
0055B06E  test ecx, ecx
0055B075  je  0055B15A            ; skip box + vtbl+284
…
0055B0A2  mov ecx, [eax+224]
0055B0AC  mov [edx], ecx          ; [boxed+0] = persist i32
0055B12E  call [edx+284]          ; type-34 store
0055B15A  mov eax, [edx+228]      ; next arm; different list
```

`0055B15A` is the `[def+228]` / `vtbl+320` / `+380` arm
(`0055B9D0-post-dword` §4). It does **not** touch `+372`.

Identity `0124BD2C+284 == 0055B520` stays **PARTIAL** (no rdata).
The **body** of `0055B520` is the only type-34 `ret 4` that writes
`+372`, and it is the first appender after the ctor. Treat that
match as **PROVEN** from `.text` order; the slot dword is
**PARTIAL**.

---

## 3. `0055B520`: header + 16-byte node, not `{id, fn}`

`ecx` = outer widget. Arg (`ret 4`) is the `0042AA29` pair*.

```
0055B525  mov eax, [edi+372]
          test eax, eax
          jne  already              ; 0055B568
          push 4
          call 00BFEA1A             ; header (one dword)
          push 16
          call 00BFEA0E             ; sentinel
          mov [eax], eax
          mov [eax+4], eax          ; next = prev = self
          mov [esi], eax            ; header[0] = sentinel
          mov [edi+372], esi
already:
          mov eax, [edi+372]
          mov esi, [eax]            ; sentinel
          push 16
          call 00BFEA0E             ; node
          lea ecx, [eax+8]
          mov edx, [esp+16]         ; pair*
          mov edi, [edx]
          mov [ecx], edi            ; node+8  = [pair+0] boxed*
          mov edx, [edx+4]
          mov [ecx+4], edx          ; node+12 = [pair+4] refcount*
          test edx, edx
          je  link
          inc [edx]                 ; refcount++, not a call
link:
          ; insert before sentinel (append)
          mov ecx, [esi+4]
          mov [eax], esi
          mov [eax+4], ecx
          mov [ecx], eax
          mov [esi+4], eax
          ret 4
```

### Header (`[widget+372]`)

| Off | Field |
| ---: | --- |
| 0 | sentinel* (`00BFEA0E` 16-byte node, circular to self) |

Allocated **only** on the first append. Until then `+372` is 0.

### Node (16 bytes, `00BFEA0E`)

| Off | Field | Not |
| ---: | --- | --- |
| 0 | next* | — |
| 4 | prev* | — |
| 8 | **boxed\*** (`0042BE50`; `[boxed+0] = [def+224]`) | raw id |
| 12 | **refcount\*** (`0042AA29` ctrl*) | function pointer |

`inc [edx]` on `[pair+4]` is the same refcount bump as every
other `0042AA29` store (`type10-plus352`). A function pointer
is not incremented.

### Pair that `0055B040` pushes (`0042AA29`)

```
0042AA29  [esi]   = boxed*            ; 0042BE50, then [boxed]=id
          00BFEA1A(12) ctrl:
            [0] = 1
            [4] = 00429F43            ; dtor (the only "fn")
            [8] = boxed*
          [esi+4] = ctrl*
```

The dtor lives on the **ctrl object**, not on the list node.
`0055B520` copies `{boxed*, ctrl*}`. It does **not** copy
`00429F43` into `node+12`.

`0042BE50` zeros `[boxed+0]` then inits `+4` / `+8`;
`[boxed+12] = 1` (live-packet flag). `0055B0AC` overwrites
`[boxed+0]` with `[def+224]`. That i32 is what `0059A238`
eventually double-derefs (`[pair] → [boxed] → id`).

---

## 4. `0055AF60` does not walk `+372`

`ecx` = **outer** widget:

```
0055AFAC  mov ecx, [esi+372]
0055AFB2  mov eax, [esi]
0055AFB4  push ecx
0055AFB5  mov ecx, esi
0055AFB7  call [eax+524]          ; post the head; no loop
```

No `mov esi, [eax]`, no `cmp` against a sentinel, no
`lea …, [node+8]` in this function. It always pushes
`[this+372]`, including when that dword is still 0.

The earlier `mov ecx, [eax+524]` at `0055AF7F` is
**`[def+524]`** (SelectState), a different `+524`.

---

## 5. The walk is `00558DE0`

1-arg stdcall (`ret 4`). Matches `push [+372]; call [vtbl+524]`.

```
00558DE0  mov edi, [esp+8]        ; list* == [widget+372]
          test edi, edi
          je  00558E09            ; NULL head → nothing
          mov eax, [edi]          ; sentinel
          mov esi, [eax]          ; first real node
          cmp esi, eax
          je  00558E08            ; sentinel-only → nothing
00558DF2  call 0041E5F2
          lea ecx, [esi+8]        ; &{boxed*, refcount*}
          push ecx
          call [edx+56]           ; 0041E6D3
          mov esi, [esi]          ; next
          cmp esi, [edi]
          jne 00558DF2
          ret 4
```

Empty cases the walker treats as no-op:

| Head | Meaning |
| --- | --- |
| `edi == 0` | never appended (`[def+224]==0`, or dtor already ran) |
| `[sentinel] == sentinel` | header exists, no nodes (not first-seen persist) |

Identity `vtbl+524 == 00558DE0` stays **PARTIAL**. ABI
(one list*, walk `&node+8`) is **PROVEN**.

---

## 6. First-seen empty list is `+372==0` when `+224==0`

Yes.

1. `0055B491` writes `0` into `+372`.
2. `0055B075` skips the only caller of `vtbl+284` when
   `[def+224]==0`.
3. `0055B520` (the only `+372` allocator in this family) never
   runs, so no 4-byte header and no sentinel.
4. Click still `push [esi+372]` (`0055AFAC`). That push is **0**.
5. `00558DE0` `test edi, edi` / `je` returns without posting.

A live empty sentinel (`header != 0` and `next == sentinel`) is
**not** the first-seen persist shape. Dtor `0055B760` also
`test [esi+372]` / `je` skip, same NULL meaning.

Sibling lists (`+380` / `+376` / `+392`) are independent.
`[def+228]==0` leaves `+380` NULL the same way; that is **not**
the list `0055AF60` posts.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `0055B491` | `[+372]=0` before persist | **PROVEN** |
| `0055B075` | `[def+224]==0` → skip `vtbl+284` | **PROVEN** |
| `0055B520` | append `{boxed*, refcnt*}` to `+372` | **PROVEN** body; `=vtbl+284` **PARTIAL** |
| `0055AF60` | `vtbl+524([+372])`; no walk | **PROVEN** |
| `00558DE0` | walk; NULL / sentinel-only = empty | **PROVEN** walker; `=vtbl+524` **PARTIAL** |
| `0042AA29` | pair `{boxed*, ctrl*}`; ctrl+4 = dtor | **PROVEN** |
| node `{id, fn}` | — | **DISPROVEN** |

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
  (`0055B040`, `0055B460`, `0055B520`, `0055AF60`, `00558DE0`,
  `0055B760`)
- `listing-00400000.txt` (`0042BE50`, `0042AA29`)
- `proofs/0055B9D0-post-dword/README.md`
- `proofs/vtbl584-post-hop/README.md`
- `proofs/type10-plus352/README.md` (same pair ABI; different store)
