# `00A38E50` store at `[0x13B8A54+20]` / `+24`

Investigation only. No production `src/` edits.
Do **not** start Oakvale. Do **not** implement a
`.h` parser.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN**.

Question: `00A39010` calls `00A38E50(this, buf,
arg1)` which walks `"enum"` via `009B9BA0` /
`00A38670`. What does it **store** at `[this+20]`
/ `[this+24]` of `[0x13B8A54]`? Record / node
layout? Token kinds only.

Authority: Fable.exe dump
`listing-00a00000.txt` (`00A38420`–`00A384EE`,
`00A38500`–`00A3854B`, `00A38550`–`00A3858D`,
`00A385A0`–`00A38660`, `00A38670`–`00A38C15`,
`00A38E50`–`00A38FAB`, `00A39010`–`00A39187`,
`00A39190`–`00A391A9`, `00A39900`–`00A3994F`,
`00A39A00`–`00A39A86`, `00A39AE0`);
`listing-004c0000.txt` (`004CDB10`–`004CDB68`,
`004CF810`–`004CF840`);
`listing-00500000.txt` (`00514C10`–`00514CC1`,
`00513940`–`005139F0`);
`listing-00980000.txt` (`009B21F0`–`009B2229`,
`009B61F0`, `009B8520`–`009B8590`,
`009B9530`–`009B9564`, `009B9700`–`009B9764`,
`009B9770`–`009B9803`, `009B9870` / `009B9880`,
`009B99C0`–`009B9A12`, `009B9BA0`–`009B9BEE`,
`009B9EE0`–`009B9FBC`, `009B9FC0`–`009BA07C`,
`009BA080`–`009BA105`, `009BA110`–`009BA16A`);
`listing-00400000.txt` (`004014A0`–`004014D5`);
`listing-00a40000.txt` (`00A60410`);
`listing-01200000.txt` (`0121A630`);
`e8.tsv` dests `00A39010` / `00A38E50` /
`00A38670` / `00A385A0` / `00A38500` /
`00514C10` / `004CF810`;
`strings.tsv` `"enum"` / `"Unexpected EOF in
enum"` / `"DefinitionManager :
CreateSymbolsFromPathList"`;
siblings `proofs/004CDB10-00A39010`,
`proofs/004CDB10-subtitled-body`,
`proofs/004CDB10-host-register`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| `[this+20]` / `[this+24]` hold the enum **records**? | **No.** They are a CRC **set** (head + count). | **DISPROVEN** as the value table |
| What `00A39010` writes there? | **Clear only.** If `[this+24]!=0`, `004CF810` frees the tree; sentinel reset; `[this+24]=0`. | **PROVEN** |
| What `00A38E50` / `00A38670` write there? | **Nothing** on this singleton. Insert is `00514C10` on `[this+20]` **only if** `[this+32]!=0`. | **PROVEN** |
| `[0x13B8A54+32]` on this path? | Ctor `00A38500` stores `0`. No later write in `00A38500`–`00A39187`. | **PROVEN** 0 |
| Where **are** the records stored? | Vector at `[this+4]` / `[this+8]` / `[this+12]`. 8-byte `{crc32, i32}`. Then `009B8520` sorts. | **PROVEN** |
| Tree **node** size at `+20`? | **20** bytes. Key = CRC at `+16`. No mapped value. | **PROVEN** |
| Vector **record** size at `+4`? | **8** bytes (`sar 3` everywhere). | **PROVEN** |

**Answer:** `[0x13B8A54+20]` is the set **head**;
`+24` is the set **count**. `00A39010` empties
them. `00A38E50` does **not** fill them while
`+32==0`. Enum members land in the **sorted
vector at `+4`**.

---

## 1. `this` is `[0x13B8A54]`

`004CDB46` `mov ecx, 0x13B8A54` / `call
00A39010`. Static ctor `0121A630` already
`00A38500` on the same BSS. Init Game **fills**,
it does not construct. **PROVEN**
(`004CDB10-00A39010`).

`00A39010` `ret 8`. Sole nested parse `E8` is
`00A3910D call 00A38E50` (`this` still `ebx`).

`00A39900` on `lea ebp, [ebx+4]` is **not** a
lock. It **frees** the 8-byte vector at `this+4`
and zeros `begin` / `end` / `cap` / dirty.
Sibling “lock” is **DISPROVEN**.

---

## 2. Singleton layout (`00A38500`)

```
+0   vtbl 0x129CF84
+4   vec begin     ; {u32 crc, i32 value}*
+8   vec end
+12  vec cap
+16  byte          ; sort flag into 009B8520
+17  dirty         ; set by 00A385A0; 00A39010 sorts
+20  set head*     ; 20-byte sentinel
+24  set count
+32  byte          ; “insert CRC into +20 set”
```

Ctor: alloc **20** at `+20`; `[head]=0` (byte);
`[head+4]=0` (root); `[head+8]=[head+12]=head`;
`[this+24]=0`; `[this+32]=0`.

`00A39190` `([end]-[begin])>>3` is the vector
length. `00A391A0` is `begin + i*8`.

RTTI name of `0x129CF84` is **UNREAD**. Nearby
ASCII is `CDefinitionManager` / `CreateSymbolsFromPathList`
(`0x0129B20C`) — generic loader, not a spoken
line.

---

## 3. What is stored at `+20` / `+24`

### 3a. `00A39010` — clear

```
00A39028  mov eax, [ebx+24]
00A3902B  lea esi, [ebx+20]
00A39030  cmp eax, 0
00A39032  je  skip
          push [head+4]          ; root
          call 004CF810(esi)     ; free tree
          [head+8]  = head
          [head+4]  = 0
          [head+12] = head
          [esi+4]   = 0          ; [this+24]
```

`004CF810` walks `[node+12]` then `[node+8]`,
`00BFEA14` each node. **PROVEN** tree free.

### 3b. Insert path — gated

`00A38670` stores a member only via `00A385A0`
(`00A38905`, `00A38946`, `00A38A0A`). No other
`E8` of `00A385A0`.

`00A385A0(this, name_cstring, i32 value)`:

1. If `[this+32]`: CRC name (`004014A0` table
   `0x129A168`) and `00514C10` on `this+20`.
2. **Always:** append `{crc, value}` at
   `[this+8]`, or `009B61F0` grow; `[this+17]=1`.

`00514C10` is `set<u32>` find-or-insert.
`00513940` alloc **20**; key dword at
`node+16`; `[this+24]++`. `00A385A0` **ignores**
the inserted-bool. The set is CRC-only.

Because `[this+32]==0`, step 1 is skipped.
`00A38E50` writes **no** node at `+20` and
leaves `+24` at 0 after the clear.

### 3c. Tree node (only if `+32`)

| Off | Field |
|---|---|
| +0 | color (rebalance `0042971D`) |
| +4 | parent |
| +8 | left (`key <`) |
| +12 | right (`key >=`) |
| +16 | `u32` CRC |

**Size 20. PROVEN.** Not the enum value table.

---

## 4. Record layout (the real store)

Vector element, **size 8**:

| Off | Field | Source |
|---|---|---|
| +0 | `u32` CRC32 of member name | `004014A0` |
| +4 | `i32` enumerator | implicit 0,1,2… or `=` parse |

After `00A38E50` returns, `00A39010`:

- if `[this+17]`: `009B8520(begin, end, [this+16])`
  — introsort of 8-byte keys (`sar 3`,
  `009B7ED0` / `009B69A0`). **PROVEN** sort.
- `00A39A00` shrink-to-fit on `this+4`.

Lookup `00A38420` (`004CDF91` later, not this
site) binary-searches **this vector**
(`009B21F0`, `sar 3`, cmp `[esi+eax*8]`), then
returns `[hit+4]`. It does **not** walk `+20`.
**PROVEN** consumers use `+4`, not `+20`.

Binary twin `00A38C20` (`00A01A30`, later
sound) `00A39AE0` then
`[begin+i*8]=crc; [begin+i*8+4]=value`. Same
8-byte record.

---

## 5. `00A38E50` / `00A38670` walk (kinds only)

`00A38E50` `ret 8`: lexer `009B9530` on `buf`;
loop `009B9BA0("enum")`; each hit
`00A38670(this, lexer, arg1)`. Fail →
`"Unexpected EOF in enum"` family via
`00A381D0`.

`009B9BA0` is exact-keyword consume (`009B9A20`
+ advance). Not a kind.

### Token object

`+0` kind dword. `+8` CString (`009B9870` /
`009B9880` return `ecx+8`).

### Kind numbers

| Kind | Setter | Meaning |
|---|---|---|
| **1** | `009B99C0` `mov [edi], 1` | 1-char punctuation |
| **2** | `009B9FC0` / `009BA110` `mov […], 2` | ident (`[A-Za-z0-9_]`) or quoted |
| **3** | `009B9700` `mov [ecx], 3` | integer (`±digits`) |
| **4** | `009B9770` `mov [esi], 4` | float (has `.`) |

`009BA080` (next token used by `00A38670`):
skip (`009B9C60`); digit or `'-'+digit` →
`009B9EE0` (3 or 4); alpha/`'_'` → `009B9FC0`
(2); else → `009B99C0` (1).

### Kind 1 immediates in `00A38670`

2-byte `rep cmpsb ecx=2` or `0099E960`. Bytes
are **not** in `strings.tsv` (too short).
Grammar from control flow + error ASCII:

| Imm | Role | Class |
|---|---|---|
| `0x129B0E0` | `{` — if then `}` → empty body | **PARTIAL** (char **UNREAD**; role proven) |
| `0x123542C` | `}` — end members; then expect `;` | **PARTIAL** (char **UNREAD**; role proven) |
| `0x122E024` | `,` — store implicit, `value++` | **PARTIAL** (char **UNREAD**; role proven) |
| `0x129C1F8` | `=` — `009BADD0` + `009E2590` number, store that | **PARTIAL** (char **UNREAD**; role proven) |
| `0x129B6AC` | rejected kind-1 after type name → `"Expecting a brace in enum"` | **UNREAD** bytes |

Errors (ASCII **PROVEN**): `"Cannot read
Definition Type name"`; `"Unexpected EOF while
reading enum"`; `"Expecting a brace in enum"`;
`"Expected string in enum"`; `"Unexpected EOF
in enum def line"`; `"Expecting ',' '=' or '}'
in enum"`; `"Unexpected EOF in enum"`;
`"Expecting ';' in enum"`.

### Member store

- Type name: kind **2** (else name `"NULL"`).
- Members: kind **2** name; optional `=` +
  number (kind **3** via `009E2590`); then `,`
  or `}`.
- Implicit value starts at 0 (`[esp+20]`); `,`
  does `inc` after `00A385A0`.
- `arg1` empty or equals type name → skip that
  `enum` (`0099E960` / `0099E900`).

No full `.h` grammar beyond these kinds.

---

## 6. `00A60410` rewrite (not a store)

`00A39010` runs `00A60410` ×3 on the file
buffer (`0x129B208`/`204`/`200`/`1FC`/`1F8`)
**before** `00A38E50`. `strstr` replace.
Payload bytes **UNREAD** (same skip as
`004CDB10-subtitled-body`). **PARTIAL**.

Path into `00A39010` is
`Data\Defs\` + `misc_def_types.h`
(`004CDB10-host-register`). File contents
**UNREAD**.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `00A39010` | fill `[0x13B8A54]`; clear `+20/+24`; parse; sort `+4` | **PROVEN** |
| `00A38E50` | `"enum"` loop | **PROVEN** walk |
| `00A38670` | one `enum` body | **PROVEN** |
| `00A385A0` | append `{crc,i32}` at `+4`; set at `+20` iff `+32` | **PROVEN** |
| `00A38500` / `0121A630` | ctor; `+32=0`; 20-byte sentinel | **PROVEN** |
| `[this+20]` / `[this+24]` | CRC set head / count | **PROVEN** layout; **empty** this path |
| `[this+4]` 8-byte records | the store | **PROVEN** |
| tree node size 20 | `00513940` / ctor | **PROVEN** |
| `00A39900` | vector free at `+4` | **PROVEN**; lock **DISPROVEN** |
| `009B8520` | sort 8-byte records | **PROVEN** |
| `00A38420` | lookup on `+4`, not `+20` | **PROVEN** (later site) |
| token kinds 1/2/3/4 | setters above | **PROVEN** |
| punct chars `{ } , =` | immediates | **PARTIAL** |
| `0x129B6AC` / `00A60410` bytes | — | **UNREAD** |
| TLC / `misc_def_types.h` payload | — | **UNREAD** |
| spoken line from this store | — | **DISPROVEN** |
| Oakvale / `00DBDE40` | — | **DISPROVEN** |

### Remaining **UNREAD**

- Exact 2-byte punct at `0x129B0E0` /
  `0x123542C` / `0x122E024` / `0x129C1F8` /
  `0x129B6AC`.
- `00A60410` from/to strings.
- Which `enum` names / values the file has.
- Any write of `[0x13B8A54+32]` from **outside**
  `00A38500`–`00A39187` (none in-range).
