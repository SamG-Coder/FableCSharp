# Type 11 / 38 vtbl+284 store

Authority: `Fable.exe` listings under
`tools/Fable.ExeIndex/out/01-sections/text-map/`.
`.rdata` vtbl dwords were **not** dumped this pass
(`read_file` rejects `Fable.exe`; `ExeIndex vtbl` not run).

Status words: **PROVEN** / **PARTIAL** / **UNREAD**.

## Dump asked

| Vtbl | Type | Ctor write | Slot 284 VA | `[vtbl+284]` |
| --- | --- | --- | --- | --- |
| `01249554` | 11 (`CFrontEndButton`) | `0054E0BF` | `01249670` | **UNREAD** |
| `0124B04C` | 38 (`AcceptButton`) | `00558B9D` | `0124B168` | **UNREAD** |
| `0124BD2C` | 34 (base of both) | `0055B471` | `0124BE48` | **UNREAD** |
| `012497E4` | 10 menu | `0054E3DF` | `01249800` | `0054E4F0` **PROVEN** (`implementer/frontend/05-input.md`) |
| `0122F5D4` | 0 generic | — | — | `0052F040` `ret 4` **PROVEN** |

Slot index `284/4 = 71`. File offset equals RVA on this PE
(`.rdata` `rva=file=0xE2D000`).

Dump: `Fable.ExeIndex vtbl 0x01249554 80`,
`vtbl 0x0124B04C 80`, `vtbl 0x0124BD2C 80`.

## Same `0054E4F0` or different?

**PARTIAL.** Persist of `def+224` does **not** use the
final type-11/38 vtbl.

```
0055B460  type 34
  mov [esi], 0124BD2C
  call 0055B040          // vtbl+284 is type 34's
0054E0B0  type 11
  call 0055B460          // persist already done
  mov [esi], 01249554    // then overwrite
  call 0054DF50          // extra def+196 → +408 vector
00558B90  type 38
  call 0055B460
  mov [esi], 0124B04C    // no extra store
```

`0055B040` **PROVEN** (`listing-00540000`):

| Def | Call | File CRC |
| --- | --- | --- |
| `+224` | `vtbl+284` | `0x53C644E4` MessageId (15 / `0x126`) |
| `+228` | `vtbl+320` | UNREAD name |
| `+232` | `vtbl+288` | UNREAD name |
| `+236` | `vtbl+292` | UNREAD name |

Pair ABI **PROVEN**: `0042BE50` 16-byte object, `0042AA29`
wraps `{ptr, refcount}`, then `[pair]=[def+field]`.
Callee is `ret 4`.

Known `ret 4` stores with that ABI:

| Fn | Write | Who |
| --- | --- | --- |
| `0054E4F0` | replace `+352` id, `+356` refcount | type 10 slot 284 |
| `0052F040` | no-op | generic slot 284 |
| `0054E1E0` | append `{id,refcount}` to vector `+408` | type 11 only (`ecx+0x198`) |
| `0054E230` | append to vector `+420` | type 11 only (`ecx+0x1A4`) |

Type 38 size `0x194` = 404: `+408` is **out of object**.
`0054E1E0` cannot be type 38 slot 284.

Type 11/38 `.text` has **no** local clone of `0054E4F0`
(`mov [esi+352], ebx` only at `0054E530` in this family).

So:

- Persist MessageId for 11/38: type-34 `0124BD2C+284`.
  If that dword is `0054E4F0`, id lands at `+352`. **UNREAD**.
- Final type-38 `0124B04C+284`: no type-38 override body;
  likely still `0054E4F0` or type-34's pointer. **UNREAD**.
- Final type-11 `01249554+284`: may stay `0054E4F0` **or**
  switch to `0054E1E0` (`+408` vector). **UNREAD**.
  Later attach-style `call [eax+284]` would follow the
  final table, not the persist-time table.

## Where is the message object pointer?

**PROVEN** for the `0054E4F0` / `0042AA29` pair:

| Site | What |
| --- | --- |
| widget `+352` | message **id** dword (`[pair+0]` after overwrite) |
| widget `+356` | **refcount object** pointer (`[pair+4]`) |
| `[+356]+8` | 16-byte `0042BE50` object (`[refcount+8]`) |
| `[+356]+4` | dtor `00429F43` |

`0054E4F0` releases the old `+356` (`dec`, `call [eax+4]`,
`00BFE9BC`) then `inc` the new one.

Type-11 **extra** list (not slot 284 persist):

| Site | What |
| --- | --- |
| widget `+408` / `+412` / `+416` | vector of `{id, refcount}` |
| filled by `0054DF50` from `def+196` | **PROVEN** |
| `0054E1E0` appends one pair | **PROVEN** body; slot **UNREAD** |

Type-34 runtime posters (`0055AD60` action 26) use
`vtbl+584` and may push `+372` / `+388` through
`vtbl+524`. Those are **not** the persist `+224` pair.

Type-10 user post remains `&widget+352`
(`0054E2FA`, ecx = widget+4 so `edi+348`).

Type-11 dtor path `0054DEB6` `mov [esi+352], 0` is
**PROVEN**, so type 11 still owns a dword at `+352`
after the type-34 persist.

## Ctor vtbls (PROVEN)

```
0054E0B0  type 11
  call 0055B460
  [esi]    = 01249554
  [esi+4]  = 01249530   // inner (0054DBC0 lives here)
  [esi+24] = 01249528
00558B90  type 38
  call 0055B460
  [esi]    = 0124B04C
  [esi+4]  = 0124B024   // inner (0055AD60)
  [esi+24] = 0124B01C
0055B460  type 34
  [esi]    = 0124BD2C
  [esi+4]  = 0124BD08
  [esi+24] = 0124BD00
  call 0055B040
```

RTTI names: `CFrontEndButton@NUISystem` `0x0137C128`,
`CClickable@NUISystem` `0x0137BA90`. Type 38 class string
not pinned beyond ctor role `AcceptButton`.
