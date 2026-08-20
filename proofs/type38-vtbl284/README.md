# Type-38 `0124B04C` / inner `0124B024` slot 284 — where `0x126` lives

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `0055B040` / `0055B460` / `00558B90` /
`0055B520` / `0055B5B0` / `0055B640` / `0055B6D0` / `0055B760` /
`0055AF60` / `0055AFD0` / `0055ACF0` / `00558DE0` / `0054E4F0` /
`0042BE50` / `0042AA29`;
`tools/Fable.ExeIndex/out/00-index/sections.txt`;
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`,
`listing-00400000.txt`, `listing-00600000.txt`;
`proofs/who-posts-0x126/README.md`;
`proofs/messageid-plus228/README.md`;
`proofs/vtbl284-type11-38/README.md`;
`proofs/type11-msg15/README.md`;
`FrontendUiDefTests` (`UI_ACCEPT_NEW_PROFILE`).

Status: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not re-prove type 4 → action 26, Return ≠ `0x126`, or
`0059A238` `0x126` → `00851920`.

---

## Verdict

| Claim | Status |
| --- | --- |
| `0055B040` boxes `[def+224]` and `call [outer.vtbl+284]` | **PROVEN** |
| That call runs with **type-34** outer `0124BD2C`, not `0124B04C` | **PROVEN** |
| Type-38 then overwrites outer to `0124B04C` / inner `0124B024` | **PROVEN** `00558B9D` |
| Type-38 `vtbl+284` is type-10 `0054E4F0` (widget `+352` dword pair) | **DISPROVEN** |
| Type-38 `vtbl+284` is generic `0052F040` `ret 4` | **DISPROVEN** (layout; rdata dword still **UNREAD**) |
| `.rdata` dword `0124B04C+284` / `0124B024+284` | **UNREAD** (no dump this pass; see §1) |
| Type-34/38 pair-append at **widget+372** is `0055B520` | **PROVEN** body; slot identity **PARTIAL** |
| Inner `0124B024` has ~10 methods; slot 284 is not an inner method | **PROVEN** span; dword at `0124B140` **UNREAD** |
| File `0x126` is CRC `0x53C644E4` on `UI_ACCEPT_NEW_PROFILE` | **PROVEN** |
| That CRC is persist **`+228`**, copied through **vtbl+320** | **PROVEN** (`messageid-plus228`) |
| Persist `+224` / vtbl+284 / CRC `0x230364D6` is `0x126` | **DISPROVEN** |
| `0x126` as widget `+352` dword (type-10 layout) | **DISPROVEN** (type-33 `+352` is a **u8** flag) |
| `0x126` as `.text` `mov […], 0x126` | **DISPROVEN** (`who-posts-0x126`) |
| Runtime: `0x126` is dword0 of a heap `0042BE50` box | **PROVEN** |
| Widget holds that box via a **list node+8**, not a scalar at `+352` | **PROVEN** shape |
| List head for `0x126` (vtbl+320) is widget **`+392`** | **PARTIAL** (vtbl-order hypothesis, §5) |

**Answer:** `0x126` does **not** live in type-38 `vtbl+284` and does
**not** live at widget `+352`. It is persist **`[def+228]`**, boxed by
`0055B040`, stored through **vtbl+320**. On the widget it is
`[boxed+0]` where `boxed*` hangs off a circular list (node `+8`).
The list pointer is one of `+372/+376/+380/+392`; the recovered
mapping puts **vtbl+320 → `0055B6D0` → widget `+392`**. Slot 284
stores the **other** field (`[def+224]` / `Plus224Crc`) through
`0055B520` → **`+372`**.

---

## 1. `.rdata` dump (asked, not present)

`.rdata` `rva=file=0xE2D000` (`sections.txt`). Image base `0x400000`
so file offset of a VA is `VA-0x400000`.

`read_file` rejects `Fable.exe` (binary). No
`WriteVtblPart` / `vtbl-0124B04C` exists under
`tools/Fable.ExeIndex/out/` (frontend-trace family was never
written). `listing-01200000.txt` ends in `.text`
(`.text` last VA `0x0122CFFF`).

Dump when a shell is available:

```
dotnet run --project tools/Fable.ExeIndex -- vtbl 0x0124B04C 90
dotnet run --project tools/Fable.ExeIndex -- vtbl 0x0124B024 80
dotnet run --project tools/Fable.ExeIndex -- vtbl 0x0124BD2C 90
dotnet run --project tools/Fable.ExeIndex -- vtbl 0x0124BD08 80
```

| Table | Role | Slot 284 VA | Slot 320 VA |
| --- | --- | --- | --- |
| `0124B04C` | type 38 outer (`00558B9D`) | `0124B168` | `0124B18C` |
| `0124B024` | type 38 inner (`00558BA3`) | `0124B140` | `0124B164` |
| `0124BD2C` | type 34 outer (persist-time) | `0124BE48` | `0124BE6C` |
| `012497E4` | type 10 (control) | `01249800` = `0054E4F0` **PROVEN** elsewhere | — |

Slot index `284/4 = 71`. Inner span `0124B04C-0124B024 = 0x28`
= **10 dwords**. Inner slot 284 is therefore **outer `+244`**
(`0124B140`), not an inner virtual.

Until those dwords are printed, the **function VAs** below are
tied to `.text` ABI, not to the rdata pointer.

---

## 2. `0055B040` uses type-34 `vtbl+284`, then type 38 overwrites

```
0055B460  call 0055BA20              ; type 33
          mov [esi], 0x124BD2C       ; type 34 outer
          mov [esi+4], 0x124BD08
          mov [esi+24], 0x124BD00
          ; zero +364..+392
          call 0055B040              ; persist copy

00558B90  call 0055B460
          mov [esi], 0x124B04C       ; type 38 outer  AFTER copy
          mov [esi+4], 0x124B024     ; inner
          mov [esi+24], 0x124B01C
          ret 4                      ; no extra store
```

`0055B040` (`ecx` = widget):

```
call [vtbl+432]                 ; CUIDef*
ecx = [def+224]
je skip                         ; 0 → no vtbl+284
alloc 16 / 0042BE50             ; boxed, [boxed]=0
0042AA29                        ; pair {boxed*, rc*}
[boxed] = [def+224]
call [outer.vtbl+284](&pair)    ; ret 4
; then [def+228] → vtbl+320
;      [def+232] → vtbl+288
;      [def+236] → vtbl+292
```

`00631C60` writes those def slots with `00632500` (CRC skip + i32).

| Def | File CRC | `0055B040` slot | ACCEPT |
| --- | --- | --- | --- |
| `+224` | `0x230364D6` (`Plus224Crc`) | **vtbl+284** | **≠** `0x126` (test) |
| `+228` | `0x53C644E4` (`MessageIdCrc`) | **vtbl+320** | **`0x126`** |
| `+232` | UNREAD name | vtbl+288 | UNREAD |
| `+236` | UNREAD name | vtbl+292 | UNREAD |

`who-posts-0x126` “`[def+224]` is `0x126`” is **STALE**.
`proofs/messageid-plus228/README.md` is the field map.

Type-38 size `0x194`. No type-38 clone of `0054E4F0`
(`mov [esi+352], ebx` only at `0054E530` in this family).

---

## 3. Slot 284 is **not** `0054E4F0`

Type-10 `0054E4F0` (`ret 4`):

```
ebx = pair.ptr          ; boxed*
edi = pair.rc
[this+352] = ebx
[this+356] = edi
```

Type-38/34 already used `+352` as a **flag byte**
(`0055AD7B` `mov al,[esi+348]` with inner this ⇒ widget `+352`;
`0055BA20` zeros that byte). Writing a packet pointer there would
collide. `0054E4F0` is **DISPROVEN** for this object.

Generic `0122F5D4+284` `0052F040` is `ret 4` no-op — **DISPROVEN**
as the persist sink (`+224` nonzero would be dropped).

---

## 4. Four type-34 pair-append stores (`.text`)

Immediately after `0055B460` / `0055B040`, four `ret 4` clones
append `{boxed*, rc*}` onto a 4-byte list head:

| Fn | List head | Same shape |
| --- | ---: | --- |
| `0055B520` | **`+372`** | alloc list if 0; node `+8` = `pair.ptr`, `+12` = `pair.rc`; `inc [rc]` |
| `0055B5B0` | **`+380`** | same |
| `0055B640` | **`+376`** | same |
| `0055B6D0` | **`+392`** | same |

Dtor `0055B760` (type-38 `00558BF0` / `00558D30` jump here)
frees `+372`, `+392`, `+376`, `+380` in that order, then
`jmp 0055BC30`.

`0042AA29` pair:

```
[pair+0] = boxed*          ; 16-byte 0042BE50
[pair+4] = rc*             ; [rc]=1, [rc+4]=00429F43, [rc+8]=boxed*
```

`0055B040` then `mov [boxed], [def+field]`. So **the id is
`[boxed+0]`**, never an immediate in `.text`.

List node (16 bytes): `next, prev, boxed*, rc*`. Head object
`[list+0]` is the sentinel. Insert is before sentinel
(`[sentinel+4]` = new). First payload:
`[sentinel.prev + 8] = boxed*`.

Poster `00558DE0` (type-38 list walk, `ret 4`):

```
for node in list:
  0041E5F2
  push &node+8              ; &boxed*  (same ABI as type-10 &+352)
  call [input.vtbl+56]      ; 0041E6D3 → UI vtbl+32 0059A238
```

`0059A238`: `[arg] → boxed* → [boxed] = id`.

---

## 5. Which list is `0x126`? (PARTIAL)

`0055B040` call order is **284, 320, 288, 292**.
The four stores appear in `.text` as **`0055B520`, `0055B5B0`,
`0055B640`, `0055B6D0`**.

MSVC emits near-identical methods in **vtbl / declaration
order**, which matches increasing slot offsets
**284, 288, 292, 320**, not the caller’s 284-then-320 order.

Hypothesis (needs rdata `[0124BD2C+284]` … `+320`):

| Slot | Persist | Fn | Widget list | ACCEPT |
| ---: | ---: | --- | ---: | --- |
| 284 | `+224` | `0055B520` | **`+372`** | `Plus224` ≠ `0x126` |
| 288 | `+232` | `0055B5B0` | `+380` | UNREAD |
| 292 | `+236` | `0055B640` | `+376` | UNREAD |
| **320** | **`+228`** | **`0055B6D0`** | **`+392`** | **`0x126`** |

Alternate (call-order = `.text` order): vtbl+320 = `0055B5B0` =
`+380`. **PARTIAL** until §1 dump.

Click `0055AF60` (0-arg, matches `vtbl+584` from `0055AD8F`)
`push [this+372]` / `call [vtbl+524]`. Action 27 also pushes
`[inner+372]`. Those post the **vtbl+284 / `+224`** list, **not**
`0x126`, if the table above holds.

`0055AFD0` is the clone that `push [this+392]` / `vtbl+524` /
subscribe 29 — that is the recovered **MessageId** post if
`+392` is vtbl+320.

`0055ACF0` posts `[this+380]`.

Type-38 final `0124B04C+284` should still be `0055B520` (no
override in `00558B90`; same `0x194` tail). **UNREAD** as a
rdata dword. Later attach-style `call [eax+284]` would follow
the **final** table; persist already ran on type 34.

---

## 6. Inner `0124B024` slot 284

Inner is `widget+4`. Apply `vtbl+4` is `0055AD60` (**PROVEN**).
Thunks: `00558D10` `sub ecx,4` (inner dtor), `00558D20`
`sub ecx,24`.

`0055B040` does `mov ecx, ebx` (widget) then `call [edx+284]`.
It never loads `[widget+4]`. Inner slot 284 is **not** the
persist store.

Reading `0124B024+284` as a table index walks **into** the
outer table at `+244`. Treat that dword as **UNREAD / not a
method** until dumped.

---

## 7. Runtime location of `0x126` (ACCEPT type 38)

```
frontend.bin UI_ACCEPT_NEW_PROFILE
  CRC 0x53C644E4 + i32 0x126     → CUIDef +228
00558B90 → 0055B460 → 0055B040
  box 0042BE50; [boxed+0] = 0x126
  call [0124BD2C + 320]          ; not +284
  0055B6D0? appends to widget+392
widget+392 → list* → node+8 → boxed* → [0] = 0x126
```

| Site | Holds |
| --- | --- |
| heap `0042BE50` `+0` | **`0x126`** (**PROVEN** write) |
| heap `0042BE50` `+4…` | other `0042BE50` fields (**UNREAD** here) |
| `0042AA29` `rc+8` | `boxed*` |
| list node `+8` | `boxed*` |
| list node `+12` | `rc*` |
| widget `+392` | list* for MessageId (**PARTIAL**) |
| widget `+372` | list* for `+224` / vtbl+284 (**PROVEN** `0055B520`) |
| widget `+352` | u8 click flag, **not** the id |
| widget `+364` | armed dword (action 26), **not** the id |

C# `FrontendWidget.MessageId` is the persist i32, not a native
offset analog. Do not store `0x126` at `Type10StoredMsgOffset`.

---

## 8. C# leftovers (do not apply here)

- `FrontendInputMap.PersistMessageDefOffset = 228` **MATCH**.
- Comments / older proofs that say `0055B040` copies `0x126`
  through **vtbl+284** are **STALE**.
- `MessageFromWidgets` posts the first visible stored i32; native
  posts a **list** through `vtbl+524` / `00558DE0`.
- Pin `0124B04C+284` / `+320` once `ExeIndex vtbl` runs.

---

## Sources

- `listing-00540000.txt` (`00558B90`, `0055B040`, `0055B460`,
  `0055B520`…`0055B6D0`, `0055B760`, `0055AD60`, `0055AF60`,
  `0055AFD0`, `00558DE0`, `0054E4F0`)
- `listing-00400000.txt` (`0042AA29`, `0042BE50`)
- `listing-00600000.txt` (`00631FBD` `+224` then `+228`)
- `proofs/who-posts-0x126/README.md` (poster path; `+224` **STALE**)
- `proofs/messageid-plus228/README.md`
- `tests/Fable.Formats.Tests/FrontendUiDefTests.cs`
  (`accept.MessageId == 0x126`, `!= accept.Plus224`)
