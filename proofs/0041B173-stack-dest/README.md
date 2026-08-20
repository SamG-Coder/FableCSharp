# `0041B173` stack dest vs leftover #36 dest-lock

Investigation only. Production `src/` and `tests/` were
not edited. Do not invent dest numbers. Do not plant
`512,384`. Leftover #36 dest-lock stays open.

Question: is there **any** native dest 4-tuple dump
(process dump, screenshot metadata, implementer notes)
of first-seen PRESS_START stack dest
`[esp+36],[esp+40],[esp+44],[esp+48]` at
`0041B173`…`0041B1AF`?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`0041AFA0` / `0041B173` / `0041BEB0`);
`implementer/frontend/fn-0041AFA0-exact.txt`,
`fn-0041BEB0-exact.txt`, `fn-0054EF00-exact.txt`,
`02-layout.md`, `11-transform.md`, `16-resolution.md`,
`17-press-start-frame.txt`;
`export/native/`;
`export/frontend/press-start-dests.txt`,
`press-start-frame.txt`;
`proofs/0041AC20-dest-formula`;
`proofs/issue-36-verify`;
`proofs/frontend-screens-vs-native`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

`proofs/0041AC20-dest-formula` already recovered the
`0041AFA0` dest **formula**. This note is only the
**native numbers** lock. Do not re-prove GraphicIndex
leftover or inherit remap.

---

## Verdict

**UNREAD. No native dest tuple dump exists.
Leave leftover #36 dest-lock open.**

`0041B173`…`0041B1AF` snaps stack dest
`[esp+36..48]` to integer then stores floats
back onto the **stack**. It does **not** write
widget `+248`. There is no process dump, no
minidump, no PIX/ETL, no screenshot `tEXt` /
comment, and no implementer note that records
those four first-seen PRESS_START values.

Host dest tables (`export/frontend/press-start-dests.txt`,
`implementer/frontend/17-press-start-frame.txt`,
`Press_Start_first_seen_dest_table_matches_0041AFA0`)
are `LayoutFrontendWidgets` analogs. `export/native/`
is screenshots (one Press Start still: `Fable01.png`).
Pixels are not `[esp+36..48]`.

`UI_PRESS_START_TEXT` is type 6. Its draw is
`0054EF00`. It **never** reaches `0041B173`.
Host still stores a dest 4-tuple on that widget.
That 4-tuple is **LEFTOVER**, not a listing
immediate and not a native dump.

| Claim | Status |
| --- | --- |
| `0041B173`…`0041B1AF` is stack snap of dest `X0,Y0,X1,Y1` | **PROVEN** listing |
| Snap stores dest back onto the widget | **DISPROVEN** — `fistp [esp+12]` then `[esp+36..48]` |
| Type-6 `UI_PRESS_START_TEXT` runs `0041B173` | **DISPROVEN** — `0054EF00` pen at `+248` |
| Listing immediate dest `512` / `384` | **DISPROVEN** (`proofs/0041AC20-dest-formula`) |
| Native first-seen `[esp+36..48]` 4-tuple | **UNREAD** — no dump |
| Native first-seen type-0x22 rec `+12..+24` dest | **UNREAD** — no dump |
| Native first-seen widget `+248/+252` | **UNREAD** — no dump |
| `export/native/` dest tuples / PNG metadata | **DISPROVEN** as dest dump |
| Host `512,384,512,384` / forest `410` lattice | **LEFTOVER** analog, not native dest |
| Dest **formula** recovered | **PROVEN** elsewhere — not this leftover |

**Overall: UNREAD** for leftover #36 dest-lock.
Do not close it. Do not replace host dest with
new invented constants.

---

## Evidence

### Listing `0041AFA0` (`listing-00400000.txt`)

Type-0 `vtbl+8` is `0041AFA0` (`0122F5D4`;
`proofs/draw-type10-fork`). Dest is built on the
stack, then snapped here:

```
0041B0AD  mov eax, [edi+248]      ; origin X bits
0041B0B5  fmul [edi+264]          ; size W * dest scale
0041B0BB  mov ecx, [edi+252]
0041B0C1  mov [esp+12], eax
0041B0DD  mov [esp+36], edx       ; dest X0 = origin X
0041B0D1  fmul [edi+268]
0041B0F9  fadd [esp+12]
0041B0FD  mov [esp+40], eax       ; dest Y0 = origin Y
0041B10D  fstp [esp+44]           ; dest X1
0041B119  fadd [esp+16]
0041B123  fstp [esp+48]           ; dest Y1
0041B127  call [edx+424]          ; centre? else fall to 0041B173
0041B12F  je 0041B173
… centre rewrites [esp+36..48] …
0041B173  fld [esp+36]
0041B177  fistp [esp+12]          ; snap X0; not a widget store
0041B17B  fild [esp+12]
0041B17F  fstp [esp+36]
0041B183  fld [esp+40]
0041B187  fistp [esp+12]
0041B18B  fild [esp+12]
0041B18F  fstp [esp+40]
0041B193  fld [esp+44]
0041B197  fistp [esp+12]
0041B19B  fild [esp+12]
0041B19F  fstp [esp+44]
0041B1A3  fld [esp+48]
0041B1A7  fistp [esp+12]
0041B1AB  fild [esp+12]
0041B1AF  mov eax, [edi]
0041B1B3  fstp [esp+48]
0041B1B7  call [eax+404]
```

`fn-0041AFA0-exact.txt` matches. After snap the
four floats stay on the **stack**. Later
`0041BEB0` (`0041B4E6`) copies a dest pointer
into type-`0x22` record `+12..+24`
(`fn-0041BEB0-exact.txt`). That copy is still
not a process dump of the numbers.

Type-6 draw `0054EF00` (`fn-0054EF00-exact.txt`):

```
0054EF4A  fld [esi+248]           ; pen X
0054EF78  mov ecx, [esi+252]      ; pen Y
0054F0B0  fistp [esp+40]          ; snap pen, not dest rect
0054F10E  call 00543910           ; type 0x27 glyph record
```

No `[esp+36..48]` dest 4-tuple. No `0041AFA0`.

### Formula already recovered (not this leftover)

`proofs/0041AC20-dest-formula`:

```
w = (+360 != 0) ? (float)+360 : +204
h = (+364 != 0) ? (float)+364 : +208
w *= +264
h *= +268
dest = centre ? (ox±w/2, oy±h/2) : (ox, oy, ox+w, oy+h)
fistp/fild snap
```

Origin `+248` is `0052FFD0`. Scale `+264` is
`0052F5C0`. First-seen type-10 root remap size
gives inherit `1.6` on 1024×768
(`implementer/frontend/16-resolution.md`).
That is the **math**. Leftover #36 dest-lock
is the **observed native 4-tuple**.

---

## Original

Native first-seen PRESS_START that would hit
`0041B173` is type-0 leaf present (`UI_TITLE_01`,
forest tiles, mouse). Those widgets:

- persist W/H 0
- leftover `+204/+208` from GraphicIndex frame
  (`0041AC20` when `+376 != 0`)
- dest 4-tuple **only on the stack** at
  `0041B173`, then into the `0x22` record

Type-6 `UI_PRESS_START_TEXT` / `UI_LEGAL_TEXT`
never build that 4-tuple. Native pen is `+248`.

What a dest-lock dump would have to contain
(any one would close the unread site):

| Dump | Site | Status in repo |
| --- | --- | --- |
| Stack `[esp+36..48]` after `0041B1AF` | type-0 present | **UNREAD** — none |
| Type-`0x22` rec `+12,+16,+20,+24` | `0041BEB0` | **UNREAD** — none |
| Widget `+248/+252` after `005301B0` | layout origin | **UNREAD** — none |
| Debugger / minidump / PIX of first-seen | process | **UNREAD** — no `*.dmp` `*.pix` `*.etl` |

`export/native/` contents (2026-08-19):

| File | What it is |
| --- | --- |
| `01-after-launch.png` | desktop + black Fable window |
| `01-window.png` | tiny window crop |
| `02-skip-*.png` | skip-AVI desktop stills |
| `02-skip-*-wnd.png` / `03-menu-window.png` | 118-byte placeholders |
| `03-menu-desktop.png` | Steam forum page, not the game |
| `Fable01.png` | native Press Start **pixels** (title / forest / “Press Left Mouse Button To Continue” / legal) |

PNG text-chunk scan: no `tEXt` / `iTXt` dest
tuples, no `esp`, no widget names. `Fable01.png`
proves the screen exists. It does **not** give
`[esp+36..48]`.

Implementer notes (`02-layout.md`,
`11-transform.md`, `16-resolution.md`) recover
**formulas** and persist bits. `11-transform.md`
“Dest table” is a calculator in 640-space
(TITLE_01 `70,30,326,158`, TEXT
`320,240,320,240`) with remap bits still 0.
That table is **not** a process dump. It is
also **STALE** vs later remap
(`16-resolution.md` root `def+520=1`).
`17-press-start-frame.txt` header:
“Engine-state Press Start frame **(not a
screenshot)**” — host dump.

Repo grep: no `minidump` / `windbg` / `olly` /
`cheat engine` dest capture of this site.

---

## Host

`export/frontend/press-start-dests.txt` and
`export/frontend/press-start-frame.txt` /
`implementer/frontend/17-press-start-frame.txt`
are the same host walk
(`LayoutFrontendWidgets` / `DumpFrontendFrame`).

First-seen host dest (1024×768 analog):

| Widget | Type | Host dest |
| --- | --- | --- |
| `UI_FRONTEND_PRESS_START_MENU` | 10 | `0,0,0,0` |
| `UI_FRONTEND_BG_FORREST_1_1` | 0 | `0,0,410,410` |
| `UI_TITLE` | 5 | `112,48,112,48` |
| `UI_TITLE_01` | 0 | `112,48,522,253` |
| `UI_PRESS_START_TEXT` | 6 | `512,384,512,384` |
| `UI_LEGAL_TEXT` | 6 | `512,544,512,544` |
| `UI_MOUSE_POINTER` | 32 | `0,0,32,32` |

Tests lock that table:
`FrontendLayoutTests.Press_Start_first_seen_dest_table_matches_0041AFA0`,
`EngineLifecycleTests` drawn dest on
`UI_PRESS_START_TEXT`.

Those numbers are the host analog of applying
the type-0 dest size rule (including to type-6,
which native does not). `512,384` is
`320*(1024/640), 240*(768/480)`, not a listing
immediate. Forest `410` is host snap of
`256*1.6`. **MATCH** vs recovered formula on
the calculator. **Not MATCH** vs a native
stack dump, because there is none.

`proofs/frontend-screens-vs-native` dest
**MATCH** is the same oversell
(`docs/status/README.md` `f30c099`: “dest MATCH
oversell restates #36”). CPU blit of host dest
is not `0041B173` numbers.

---

## Gap

Leftover #36 dest-lock is **native numbers of
`[esp+36..48]` at `0041B173`**. Formula recover
does not close it.

| Need | Have | Missing |
| --- | --- | --- |
| Type-0 stack dest after snap | listing ops | the four floats/ints first-seen |
| Type-6 dest 4-tuple | host point analog | native has **no** dest rect field; pen `+248` **UNREAD** as a number |
| FPU `fistp` of `256*1.6` etc. | host `410` / `522` / `253` | RC-mode / exact FPU bits **UNREAD** |
| Screenshot dest metadata | `Fable01.png` pixels | tuples / comments **DISPROVEN** |

Until a dump of § Original exists, or until
type-6 dest is stored as “no dest rect / pen at
`+248`” without a fake `X1,Y1`, leave #36 open.

Do **not** implement a dest writer that plants
`512,384` as a constant. Do **not** treat
`Fable01.png` pixel measure as dest-lock.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\implementer\frontend\fn-0041AFA0-exact.txt`
- `C:\FableCSharp\implementer\frontend\fn-0041BEB0-exact.txt`
- `C:\FableCSharp\implementer\frontend\fn-0054EF00-exact.txt`
- `C:\FableCSharp\implementer\frontend\02-layout.md`
- `C:\FableCSharp\implementer\frontend\11-transform.md`
- `C:\FableCSharp\implementer\frontend\16-resolution.md`
- `C:\FableCSharp\implementer\frontend\17-press-start-frame.txt`
- `C:\FableCSharp\export\native\`
- `C:\FableCSharp\export\frontend\press-start-dests.txt`
- `C:\FableCSharp\proofs\0041AC20-dest-formula\README.md`
