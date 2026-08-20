# `00530260` `vtbl+400` / `+420` first-seen hide vs host `Visible=false`

Investigation only. No production `src/` edits.

Question: `00530260` skips a `+176` child when
`vtbl+400` is false (after parent≠`this`) or
`vtbl+420` is true. Host
`ApplyFirstSeenState` sets `Visible=false` on
inactive type-18 / type-16 siblings
(`forest_2` / `sunbeam_2` / `WASD`). What are
type 5 / 0 / 6 `vtbl+400` and `+420`? What
fields do they read? Who writes those fields
on type-18 children at first-seen? Is host
`Visible=false` **MATCH** or leftover?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00500000.txt`
(`00530260`–`005303E0` / `0052F180`–`0052F1ED` /
`0052C730` / `005331A0` `00533288` / `005334A0`
`005336CC` / `0053394D` / `0052CF40`);
`listing-00540000.txt` (`00547360` / `00547500`
`00547523` / `00547600` / `00548F40` / `00549230`
`00549360` / `00549B20` / `00549F60` / `0054F5C0`
/ `0054ED90` `0054EEDC` / `0054FFF0`);
`listing-00400000.txt` (`0041B800` / `0041AFA0`
`0041B1B7`);
`listing-01200000.txt` (ends `0122CFFE`);
`out/00-index/sections.txt` (`.rdata` VA
`0122D000`);
`implementer/frontend/14-container.md`,
`fn-005331A0-exact.txt`, `fn-005334A0-exact.txt`,
`fn-0052CC50-exact.txt`, `fn-0041B800-exact.txt`;
`src/Fable.Game/FrontendWidgetFactory.cs`
(`ApplyFirstSeenState`);
`src/Fable.Formats/Defs/FrontendWidgetType.cs`;
`tests/Fable.Formats.Tests/FrontendUiDefTests.cs`
(`Factory_builds_press_start_then_main_menu_from_the_same_walk`);
`proofs/00595222-visible-skip`,
`proofs/0052C730-host-state`,
`proofs/0052CF40-selectstate-6`,
`proofs/type16-18-present-child`,
`proofs/persist-flag-names`.

Do not re-prove type-10 ctor `0054E3D0`,
`00595222` all-slot `vtbl+8`, or list
highlight `+348`. Do **not** invent a clip
CRC for persist `+392`.

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**MATCH**.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| Type 5 `vtbl+400` / `+420` bodies | Shared getters `0052F180` / `0052F1D0`. `.rdata` dword at `01245DE4+400/+420` is past `listing-01200000`. | **PARTIAL** bodies; dword **UNREAD** |
| Type 0 `vtbl+400` / `+420` bodies | Same cluster unless the type-0 overwrite (`0122F5D4`) replaced those slots. No dump past `WriteVtblPart` 90 slots (`+356`). | **PARTIAL**; dword **UNREAD** |
| Type 6 `vtbl+400` / `+420` bodies | Same cluster unless `01249CCC` replaced them. `0054FFF0` is **not** `+420` (align bits 4/5). | **PARTIAL**; dword **UNREAD** |
| `0052F180` reads | `[+300] >> 7` (persist `def+504` at `00533258`) | **PROVEN** |
| `0052F1D0` reads | `[+302] & 1` (persist `def+392` at `00533288`) | **PROVEN** |
| Do they read `+303`? | **No.** `+303` is layer (`def+180`). `00530260` / `0041AFA0` use `this.+303` as a draw arg, not a child skip. | **DISPROVEN** as skip field |
| Is there a native `Visible` byte on this skip? | **No.** Host `Visible` is not `+300` / `+302` / `+303`. Type-0 `+368` is a draw-ready flag inside `0041AFA0`, not `vtbl+400`. | **DISPROVEN** |
| Does `0052C730` write those fields? | **No.** `+324/+328/+332=0`, `+320=-1`, `+340=1`, zeros `+344/+336/+312/+308`. | **PROVEN** |
| Does `00547360` write them? | **No.** `0052C730` then `+360=[+48]`, `+364=0xD`. | **PROVEN** |
| Does `00548F40` / type-16 layout write them? | **No.** First-seen `00549230` (`vtbl+172`) is `0052C730` then child`[+348].vtbl+192(3)`. Same `push 3` head as `00548F40`. No `+300/+302/+303`. | **PROVEN** |
| Who writes `+300/+302/+303` on type-18 **children** at first-seen? | Ctor zero `005334A0` / copy `0053394D`, then persist `005331A0`. Type-6 ctor tail `0054ED90` may `or` align `0x08/0x10/0x20` on `+302`. No first-seen writer that is exclusive to sibling index `k != 0`. | **PROVEN** |
| Host `Visible=false` on `forest_2` / `sunbeam_2` / `WASD` | Factory index hide. Native first-seen does **not** write a matching skip bit. After `00531EC0`, `parent==this` so `vtbl+400` is not consulted. `vtbl+420` is persist clip only. | **LEFTOVER** vs native skip fields |

---

## Verdict

**`00530260` skips a `+176` child with `vtbl+400`
false only when `parent != this`. Own children
are skipped only by `vtbl+420` (`[+302]&1`),
twice.**

Type 5 / 0 / 6 share the type-5 construct
(`0052CC50` → `005334A0`). The 0-arg getters
that match the skip bits are `0052F180`
(`[+300]>>7`) and `0052F1D0` (`[+302]&1`).
Those **bodies** are **PROVEN**. The three
`.rdata` dwords (`01245DE4` / `0122F5D4` /
`01249CCC` +400 / +420) sit in `.rdata` at
`0122D000+` and are **UNREAD** this pass
(text-map ends `0122CFFE`; automated type-0
`vtbl` dump is 90 slots = through `+356`).

No first-seen function writes those bits so
that `forest_2` / `sunbeam_2` / `WASD` become
`vtbl+420` true (or `vtbl+400` false on an
unowned child). `0052C730`, `00547360`, and
`00548F40` / `00549230` do not touch
`+300/+302/+303`. Persist `005331A0` copies
the same schema for every constructed child.
A Lionhead clip CRC for `def+392` is **not**
claimed.

Host `ApplyFirstSeenState` `Visible=false` on
`SelectsChild` index `k != 0` is therefore
**LEFTOVER** as a stand-in for the `00530260`
skip fields. First-seen **index 0** on type 18
(`+332`) / type 16 (`+348`) is still **MATCH**.

| Claim | Status |
| --- | --- |
| `00530260` walk is every `+176` then `+188` slot | **PROVEN** |
| `parent != this && !vtbl+400` skip | **PROVEN** |
| `vtbl+420` skip, twice, then else `vtbl+8` | **PROVEN** |
| `0052F180` = `[+300]>>7`; `0052F1D0` = `[+302]&1` | **PROVEN** |
| Type 5/0/6 `.rdata` `+400/+420` dwords | **UNREAD** |
| Type 6 `0054FFF0` is `vtbl+420` | **DISPROVEN** (bit 4/5, not bit 0) |
| `+303` / a `Visible` byte is the `+400/+420` read | **DISPROVEN** |
| `0052C730` / `00547360` write skip bits | **DISPROVEN** |
| `00549230` first-seen = `SelectState(3)` on child`[+348]` only | **PROVEN** (not a hide of siblings) |
| Runtime first-seen writer of `+302` bit 0 on inactive swap/slider kids | **DISPROVEN** |
| Persist `+392` on `BLENDING_BG_FORREST_2` / `WASD` | **UNREAD** (no clip CRC) |
| Host `Visible=false` = native first-seen `+302`/`+300` | **DISPROVEN leftover** |
| Host present set (child 0 on, others off) = first-seen `+332/+348=0` | **MATCH** index; hide **LEFTOVER** |

---

## 1. `00530260` skip (`PROVEN`)

`listing-00500000.txt`:

```
00530260  push ebx / push ebp / push esi
          mov esi, ecx
          call [vtbl+404]          ; this layer flags
          call [vtbl+416]
          … ebx = this.+303 [+ forwarded layer] …
00530296  mov ebp, [esi+176]
          … count = ([+180]-[+176]) >> 3 …
005302BB  mov ecx, [ecx+edi*8]     ; child*
          call [edx+208]           ; parent = [child+200]
          cmp esi, eax
          je 005302DF              ; own child: skip +400
          call [edx+400]
          test al, al
          je 00530324              ; skip if false
005302EA  call [edx+420]
          test al, al
          jne 00530324             ; skip if true
005302FF  call [edx+420]           ; same slot again
          test al, al
          jne 00530324
          call [edx+8]             ; draw
```

Same four tests on the `+188` walk
(`00530355`–`005303C4`). First-seen `+188` is
ctor 0 (`005334A0` `mov [esi+188], ebx`).

Own children (`parent==this`) never take the
`vtbl+400` arm. After same-frame tick
`00599E3F` → `0052C7E0` → `00531EC0` /
`005339B0`, type-18 / type-16 `+176` kids
have parent set (`vtbl+204`). First Present
therefore consults **only** `vtbl+420` for
those siblings (`proofs/00595222-visible-skip`).

`+303` is loaded on **`this`**, added to the
forwarded layer, and pushed into child
`vtbl+8`. Type-0 draw `0041AFA0` does the
same (`0041B1B7` `vtbl+404/+416`,
`0041B1D6` `movsx ecx, [edi+303]`). It is
not a child-skip predicate.

---

## 2. Type 5 / 0 / 6 slots (`PARTIAL` / **UNREAD** dword)

| Type | Ctor | Vtbl | Construct |
| ---: | --- | --- | --- |
| 5 | `0052CC50` | `01245DE4` | `005334A0` then that vtbl |
| 0 | `0041B800` | `0122F5D4` | `0052CC50` then overwrite |
| 6 | `0054F5C0` | `01249CCC` | `0052CC50` then overwrite |

Forest / sunbeam blending groups are type **5**.
Their tiles (`UI_FRONTEND_BG_FORREST_2_*`) are
type **0**. New Profile `UI_OPTIONS_TEXT_CONTROL_WASD`
is type **6** under type-16 `00549F60`.

`.text` getter cluster (`listing-00500000.txt`):

```
0052F180  movzx eax, [ecx+300]
          shr eax, 7
          ret                      ; bit 7

0052F190  mov al, [ecx+300]
          shr eax, 6
          and eax, 1
          ret                      ; bit 6  (00530260 header +404)

0052F1C0  mov al, [ecx+300]
          shr eax, 5
          and eax, 1
          ret                      ; bit 5  (header +416)

0052F1D0  mov al, [ecx+302]
          and eax, 1
          ret                      ; bit 0

0052F1E0  mov al, [ecx+302]
          shr eax, 1
          and eax, 1
          ret                      ; bit 1 centre
```

`005331A0` is the only `.text` `or […+302], 1`
and the only `or […+300], 0x80`:

```
0053323D  mov [ebx+300], [def+60] & 0x1F
0053324C  cmp [def+504]
          or  [ebx+300], 0x80      ; bit 7 → 0052F180
0053327F  cmp [def+392]
00533288  or  [ebx+302], 0x01      ; bit 0 → 0052F1D0
005332BE  mov [ebx+303], [def+180] ; layer, not a getter
```

`implementer/frontend/14-container.md` maps
those getters onto `vtbl+400` / `+420`. Host
`EngineLifecycle.FrontendWidgetCenterFn`
claims type-0 `0122F5D4+424 = 0052F1E0` (the
next function). That is consistent with the
cluster, but this pass did **not** `vtbl`
those three tables through slot 105.

Type-6 `0054FFF0` (and twin `00543B10`):

```
0054FFF0  mov al, [ecx+302]
          test al, 0x10
          je  00550000
          mov eax, 1
          ret
00550000  movzx eax, al
          shr eax, 4
          and eax, 2
          ret
```

That is persist align (`0054ED90` `or 0x08/0x10/0x20`
from `def+508`), **not** clip bit 0. **DISPROVEN**
as `vtbl+420`.

Type-18 `00547970` reads `+302` bit 3
(`shr 3; and 1`) from `def+512` at
`00547523`. That is on the **swap object**,
not the child skip.

---

## 3. First-seen writers (`PROVEN` / **DISPROVEN**)

### `0052C730` — does not

```
0052C730  call 005339B0
          [+324]=[+328]=[+344]=[+332]=0
          [+320]=-1.0f
          [+336]=[+312]=[+308]=0
          [+340]=1
          ret
```

`005339B0` writes dest / inherit scale and
walks `+176` to set parent + recurse
`vtbl+172`. No `+300/+302/+303`.

### Type 18 `00547360` — does not

```
00547360  call 0052C730
          [+360] = [+48]
          [+364] = 0xD
          ret
```

First tick `00547380`: `0052C7E0`, then if
`+324==+328` (both 0) and duration not
elapsed, **no** `vtbl+192`. First-seen
`+332` stays 0. `00547500` (ctor fill of
`+348` states) may `or [this+302], 0x08`
from `def+512` on the **type-18 widget**,
not on `forest_2`.

### Type 16 layout / `00548F40` — `SelectState(3)` only

`00549230` (`listing-00540000.txt`, type-16
`vtbl+172` body next to ctor `00549F60`):

```
00549239  call 0052C730
          … optional +400/+396 object layout
            (widget fields, not vtbl+400) …
0054934A  mov eax, [esi+348]       ; 0 from 00549B20
00549360  push 3
          call [child.vtbl+192]    ; SelectState(3)
          … clock +384 = [+48] …
          ret
```

No call of `this.vtbl+192`. `00548F40`
(type-16 `vtbl+192`) starts with the **same**
`push 3` on child`[+348]`, then switches on
its own arg. First-seen layout never enters
that switch.

`0052CF40` (type-6/5 `vtbl+192` on the
selected text child) stores `+332=3` and
forwards `vtbl+188`. It does **not**
`or [+302],1` (`proofs/0052CF40-selectstate-6`).
Sibling `WASD` is not that child.

### Who **does** write the skip bytes

| Site | Object | Write |
| --- | --- | --- |
| `005334A0` `005336CC` | every type-5/0/6/18 ctor | `+300=+302=+303=0` then `005331A0` |
| `0053394D` | copy-ctor twin | same zeros + `005331A0` |
| `005331A0` | that same `this` | persist bits; clip bit 0 only if `def+392 != 0` |
| `0054ED90` | type-6 ctor tail | `+302` align `0x08/0x10/0x20` from `def+508` |
| `00547500` | type-18 `this` | `+302` bit 3 from `def+512` |

Whole-map `or […+302], 0x01` is still only
`00533288`. There is no first-seen `or` that
fires on sibling index `k != 0`.

Persist `+392` / `+504` numbers on
`BLENDING_BG_FORREST_2` and
`UI_OPTIONS_TEXT_CONTROL_WASD` were **not**
extracted as aligned CRCs
(`proofs/persist-flag-names`). Tiles that
**do** draw on Press Start imply `+392=0` on
those objects; that does **not** prove a 1
on the hidden siblings. **UNREAD**. Do not
invent a clip CRC.

---

## 4. Host `Visible=false` (`LEFTOVER`)

`FrontendWidgetFactory.ApplyFirstSeenState`:

1. Every widget: `Visible=true`, `Enabled=true`,
   `Clip=false`, `ActiveChild=State=0`.
2. `SelectsChild` (type **18** and **16**):
   `Visible=false` on persist child index
   `k != 0`.
3. Inherit `Visible` / `Clip` down the tree.

Tests lock `BLENDING_BG_FORREST_2` /
`BLENDING_BG_FORREST_SUNBEAM_2` /
`UI_FRONTEND_BG_FORREST_2_1` /
`UI_OPTIONS_TEXT_CONTROL_WASD` off.

That present **set** matches first-seen
index 0 (`+332` / `+348`). The **mechanism**
does not:

- Native `00530260` does not read a `Visible`
  byte.
- After parent bind, it does not use
  `vtbl+400` on those kids.
- It uses `vtbl+420` = persist clip bit.
  Nothing first-seen sets that bit on
  inactive siblings.
- Host `DrawContainerWalk`
  `if (!Visible \|\| Clip) return` therefore
  skips `forest_2` tiles that native would
  still `vtbl+8` unless some **UNREAD**
  dest/style/alpha path drops them.

`export/frontend/press-start-dests.txt` gives
type-5 `BLENDING_BG_FORREST_2` dest `0,0,0,0`
and type-0 tiles real dests (`0,0,410,410`,
…). A dest-0 **parent** does not stop
`00530260` from walking `+176`. Host inherit
of `Visible=false` is what drops those tiles.

| Site | Native first-seen | Host | Class |
| --- | --- | --- | --- |
| Type 18/16 selected index | `+332=0` / `+348=0` | `ActiveChild=0` | **MATCH** |
| Skip field on sibling | persist `+302` bit 0 only; no exclusive writer | `Visible=false` | **LEFTOVER** |
| `vtbl+400` on own `+176` | not consulted (`parent==this`) | n/a | **DISPROVEN** as this hide |
| `+303` | layer arg | unused as hide | **DISPROVEN** |
| DIP / pixels of `forest_2` on a native Press Start Present | — | hidden | **UNREAD** |

---

## Do not invent

- A persist CRC name or value for `def+392` /
  `def+504`.
- Type 5 / 0 / 6 `.rdata` dwords for
  `vtbl+400/+420` without a `vtbl` dump.
- `Visible=false` as a native store on
  `+300` / `+302` / `+303` / `+368`.
- `00548F40` as type-16 first-seen layout
  (that body is `00549230`; `00548F40` is
  `vtbl+192`).
- Dest rectangles or DIP counts for skipped
  siblings on a native Present.

**Proposed (do not apply here):** dump
`vtbl 0x01245DE4 110`, `vtbl 0x0122F5D4 110`,
`vtbl 0x01249CCC 110`. Keep first-seen
index 0. Do not treat factory `Visible=false`
as a recovered `+302` bit 0 write until a
clip byte on `forest_2` / `WASD` is extracted.
