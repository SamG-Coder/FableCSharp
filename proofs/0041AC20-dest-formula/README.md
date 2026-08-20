# `0041AC20` dest formula vs leftover #36 `512,384,512,384`

Investigation only. Production `src/` and `tests/` were not edited.
Do not invent dest numbers. Leftover #36 stays open.

Question: recover dest rect writers from listing
(`0041AC20`, inherit-scale `005339B0`, type-6 draw
`0054EF00`, remap dest `0041AFA0` `+248/+264`). Host
frontend dest still locks `512,384,512,384` for
`UI_PRESS_START_TEXT`. Is that a listing dest, or an
invented 4-tuple?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`0041AC20` / `0041AFA0` / `0041B800` / `0041D21B`);
`listing-00500000.txt` (`0052C730` / `0052C7E0` /
`0052E580` / `0052F5C0` / `0052FFD0` / `00531EC0` /
`005331A0` / `005339B0`);
`listing-00540000.txt` (`0054EF00` / `0054F5C0`);
`implementer/frontend/fn-0041AC20-exact.txt`,
`fn-0041AFA0-exact.txt`, `fn-0054EF00-exact.txt`,
`02-layout.md`, `16-resolution.md`;
`src/Fable.Game/FrontendLayout.cs`;
`src/Fable.Game/EngineLifecycle.cs`
(`LayoutFrontendWidgets`);
`tests/Fable.Formats.Tests/FrontendLayoutTests.cs`;
`proofs/type6-plus204-writer`;
`proofs/issue-36-verify`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER**.

Do not re-prove GraphicIndex leftover (`0041AC20`
`+376==0` skip) or type-6 ctor skipping `0041AC20`.

---

## Verdict

**UNREAD as native dest numbers. Formula recovered.
Do not treat leftover #36 as done.**

`0041AC20` is **not** a dest-rect writer. It stores
persist size `+360/+364` and leftover `+204/+208`.
It never reads `+248` and never writes a dest 4-tuple.

Dest origin `+248/+252` is `0052FFD0`. Dest scale
`+264/+268` is `0052F5C0`. Inherit scale `+272/+276`
is `005339B0` (then parent `00531EC0` `vtbl+460`).
Type-0 draw `0041AFA0` builds dest **on the stack**
from those fields. Type-6 draw `0054EF00` reads
`+248` as a pen origin and does **not** write dest
`X0,Y0,X1,Y1`.

`512,384,512,384` is **not** in the listing. It is the
host analog of applying the type-0 dest size rule to a
type-6 widget whose leftover and persist size are 0, at
the remapped origin `320*(1024/640), 240*(768/480)`.
Host dumps and tests lock that analog. There is **no**
native dest dump of stack `[esp+36..48]` or widget
`+248` first-seen.

| Claim | Status |
| --- | --- |
| `0041AC20` dest rect from `+204/+248` | **DISPROVEN** — leftover `+204` only; no `+248` |
| `0041AC20` writes persist `+360/+364` (`ftol` def `+92/+88`) | **PROVEN** `0041AC48`…`0041AC5E` |
| `0041AC20` leftover `fstp [esi+204/+208]` when `+376!=0` | **PROVEN** `0041ACD8` / `0041AD19` / `0041AD69` |
| Type-6 first-seen calls `0041AC20` | **DISPROVEN** — factory `0041D472` `0054F5C0` |
| `005339B0` writes dest `+248` | **DISPROVEN** — inherit `+272` / parent `+256=0` |
| `0052FFD0` writes dest origin `+248/+252` | **PROVEN** `005301B0` / `005301C7` |
| `0052F5C0` writes dest scale `+264/+268` | **PROVEN** `0052F7BE` / `0052F7F3` |
| `0041AFA0` dest = `(+360 else +204)*+264` from `+248` | **PROVEN** stack only `0041B065`…`0041B1AF` |
| `0041AFA0` stores dest back onto the widget | **DISPROVEN** — `fistp [esp+12]` then `[esp+36..48]` |
| Type-6 `0054EF00` dest 4-tuple writer | **DISPROVEN** — `fld [esi+248]` pen; `fadd [0x122DCDC]` |
| Listing immediate dest `512` / `384` | **DISPROVEN** |
| Native first-seen dest 4-tuple / `+248` dump | **UNREAD** |
| Host `512,384,512,384` lock | **LEFTOVER** analog, not native dest |

**Overall: UNREAD** for leftover #36 dest-lock vs native
numbers. Recovered writers are listed below. Do not
replace host dest with new invented constants.

---

## 1. `0041AC20` (`listing-00400000.txt`) — leftover, not dest

`0041AC20`…`0041AF90` (`fn-0041AC20-exact.txt`).

```
0041AC32  call [eax+432]          ; def
0041AC48  fld [edi+92]
0041AC4B  call 00BFEA70
0041AC50  mov [esi+360], eax      ; persist W
0041AC56  fld [edi+88]
0041AC59  call 00BFEA70
0041AC5E  mov [esi+364], eax      ; persist H
…
0041ACD2  mov [esi+376], ebx      ; empty style list → GraphicIndex 0
0041ACD8  cmp [esi+376], ebx
0041ACDE  jbe 0041AF6F            ; skip leftover
0041AD02  call [edx+84]           ; bank frame W
0041AD19  fstp [esi+204]
0041AD52  call [edx+88]           ; bank frame H
0041AD69  fstp [esi+208]
0041AF6F  ; refcount / ret — no dest store
```

No `mov` / `fstp` of `[esi+248]`, `[esi+252]`,
`[esi+264]`, `[esi+268]`. No dest `X0/Y0/X1/Y1`.
`.text` `E8 0041AC20` sites are type-0 ctor
`0041B85E` and copy `0041B8CE` only.

**Unread site if someone still wants `0041AC20` dest:**
there is none in this body. The leftover claim
“`0041AC20` dest from `+204/+248`” mixes two functions.

---

## 2. `005339B0` inherit-scale (`listing-00500000.txt`)

```
005339D1  mov [esi+52], ecx       ; layout pos
005339FB  mov [esi+92], ecx       ; persist scale
00533A2E  cmp [esi+280], edi
00533A34  jne 00533A48
00533A36  mov [esi+76], edi
00533A3C  mov [esi+256], edi      ; parent dest origin = 0
00533A48  cmp [esi+280], edi
00533A68  mov eax, 0x3F800000     ; 1.0
00533A73  jne 00533A81
00533A75  mov [esi+276], eax
00533A7B  mov [esi+272], eax      ; inherit dest scale = 1
… children: vtbl+204 parent, vtbl+172 recurse
00533B20  ret
```

No dest origin. Type-10 `vtbl+172` is `0054E4B0` →
`0052C733 call 005339B0`. Tick `0052C7E0` later
`call 00531EC0`, which pushes parent `+264` through
child `vtbl+460` and parent dest origin through
child `vtbl+456`.

---

## 3. Remap dest origin / scale (not `0041AC20`)

`005331A0` (`listing-00500000`):

```
005332A8  or [ebx+302], al        ; al=0x40 from def+520 remap size
005332B8  or [ebx+302], dl        ; dl=0x80 from def+521 remap origin
```

`0052F5C0` dest scale `+264` when not absolute and
`vtbl+464` (remap size):

```
0052F7AC  fdiv [0x1375CD4]        ; 640
0052F7B2  fmul [0x13B876C]        ; vpW
0052F7B8  fmul [esi+272]
0052F7BE  fstp [esi+264]
```

Else `+264 = +272 * +92`. Root first-seen: persist
scale 1, remap size 1, `[0x13B8768]=1`, vp `1024×768`
→ `+264 = 1.6`.

`0052FFD0` dest origin `+248`:

```
0053018B  call [edx+468]          ; remap origin?
00530193  je 005301C1
005301A9  call 0052E580           ; x/640*vpW
005301B0  mov [esi+248], ecx
005301C1  mov [esi+248], eax      ; else +52
005301D7  call [edx+408]          ; absolute?
005301DF  jne 0053024D
0053020B  fmul [esi+248]          ; * +272
0053022F  fadd [esi+248]          ; + +256
```

Type-6 persist `def+521=0` → no `0052E580` on origin.
Origin is `persistPos * inherit(+272) + parent(+256)`.
Parent dest origin is 0. Inherit is parent `+264`.

That **origin formula** is recovered. The number
`512` is `320 * 1024/640`, not a dest immediate.

---

## 4. `0041AFA0` dest (`listing-00400000.txt`) — stack remap of `+248/+264`

```
0041B065  mov eax, [edi+360]
0041B06D  jne 0041B077
0041B06F  fld [edi+204]           ; size W
0041B089  mov eax, [edi+364]
0041B091  jne 0041B09B
0041B093  fld [edi+208]           ; size H
0041B0AD  mov eax, [edi+248]      ; origin X bits
0041B0B5  fmul [edi+264]
0041B0D1  fmul [edi+268]
0041B0F9  fadd [esp+12]           ; x1 = ox + w
0041B119  fadd [esp+16]           ; y1 = oy + h
0041B127  call [edx+424]          ; centre?
0041B173  fld [esp+36]
0041B177  fistp [esp+12]          ; snap; not a widget store
```

**Dest formula (type-0 submit, Y-down pixels):**

```
w = (+360 != 0) ? (float)+360 : +204
h = (+364 != 0) ? (float)+364 : +208
w *= +264
h *= +268
if vtbl+424:
  dest = (ox - w/2, oy - h/2, ox + w/2, oy + h/2)
else:
  dest = (ox, oy, ox + w, oy + h)     ; ox=+248, oy=+252
fistp/fild snap
```

Type-6 first-seen: persist W/H 0, GraphicIndex 0 so
`+204` never written → size 0. Applying this formula
yields a **point at `+248/+252`**. Native type-6 draw
does not run `0041AFA0`.

---

## 5. Type-6 draw `0054EF00` (`listing-00540000.txt`)

```
0054EF4A  fld [esi+248]           ; pen X
0054EF78  mov ecx, [esi+252]      ; pen Y
0054EF86  je 0054EF94             ; +392==0 → inner scale +124
0054EF88  mov edx, [esi+264]
0054F08C  fmul [esi+204]          ; centre/right only
0054F0AC  fld [esp+16]
0054F0B0  fistp [esp+40]          ; snap pen, not dest rect
0054F12D  fadd [0x122DCDC]        ; +2
0054F13B  fadd [0x122DCDC]
0054F10E  call 00543910           ; type 0x27 glyph record
```

No dest `X1=origin` store. Glyph dest is the packer,
not leftover `+204`. Host dest point is an analog of
§4 with size 0, **not** this function’s output.

---

## 6. Unread sites (leftover #36 dest-lock)

These are the sites that still block locking dest vs
**native** numbers. Do not fill them with `512,384`.

| Site | What is missing | Listing |
| --- | --- | --- |
| `0041B173`…`0041B1AF` | Native first-seen stack dest `[esp+36],[esp+40],[esp+44],[esp+48]` after snap | `listing-00400000.txt` |
| `005301B0` / `005301C7` | Native first-seen widget `+248/+252` after layout | `listing-00500000.txt` |
| `0052F7BE` | Native first-seen widget `+264` (1.6 analog only) | same |
| Type-6 object | Dest 4-tuple field | **none** — `0054EF00` does not write one |
| `0041AC20` dest combine `+204` with `+248` | Dest writer | **none** in `0041AC20`…`0041AF90` |
| Process dump | Native PRESS_START dest table | `export/native/` is screenshots, not dest tuples |
| `export/frontend/press-start-dests.txt` | Host dump of `LayoutFrontendWidgets` | not native |

Host tests that still invent the 4-tuple:

- `FrontendLayoutTests.Press_Start_first_seen_dest_table_matches_0041AFA0`
  `UI_PRESS_START_TEXT` `(512,384,512,384)`
- `EngineLifecycleTests` drawn dest `512,384,512,384`
- `Leftover204_is_0041AC20_graphic_index_not_persist_size`
  feeds origin `512,384` into `ComputeSubmitDest`

Those lock the host analog. They are **not** native dest.

---

## 7. What host already MATCHES (not leftover #36 close)

`FrontendLayout.ComputeSubmitDest` **MATCHES** the
`0041AFA0` size/origin/centre math. Leftover
`LeftoverFromGraphic(0, …)=(0,0)` **MATCHES**
`0041AC20` `jbe`. Inherit remap of child origin
**MATCHES** `0052FFD0` `* +272` when root
`def+520=1`.

Do **not** implement a new dest writer that plants
`512,384` as a constant. Do **not** mark leftover #36
done until dest tests lock **native** numbers from a
dump of §6, or until type-6 dest is stored as “no dest
rect / pen at `+248`” without a fake `X1,Y1`.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00500000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00540000.txt`
- `C:\FableCSharp\implementer\frontend\fn-0041AC20-exact.txt`
- `C:\FableCSharp\implementer\frontend\fn-0041AFA0-exact.txt`
- `C:\FableCSharp\implementer\frontend\fn-0054EF00-exact.txt`
- `C:\FableCSharp\src\Fable.Game\FrontendLayout.cs`
