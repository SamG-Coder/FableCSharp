# Type-6 widget `+204` / `+208` first-seen writer

Investigation only. Production `src/` and `tests/` were not edited.

Question: who writes type-6 widget `+204` / `+208` first-seen?
Issue #36 leftover: host `CollectFrontendRecords` treats dest
width as `+204`. Native `0041AC20` leftover is
`GraphicIndex != 0` only. Type-6 `GraphicId=0` so leftover
should stay 0. Confirm no dest-width writer of `+204` on
type 6.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00500000.txt`
(`0052CC50` / `005334A0` / `005339B0`);
`listing-00540000.txt` (`0054F5C0` / `0054ED90` / `0054EF00`
/ `0054E640` / `00547D52` / `00551340`);
`listing-00400000.txt` (`0041AC20` / `0041B800` / `0041D21B`
`0041D472`);
`implementer/frontend/fn-0041AC20-exact.txt`,
`fn-0054EF00-exact.txt`, `fn-005334A0-exact.txt`,
`fn-0052CC50-exact.txt`, `fn-0041D21B-exact.txt`;
`src/Fable.Game/EngineLifecycle.cs`
(`LayoutFrontendWidgets`, `CollectFrontendRecords`);
`src/Fable.Game/FrontendLayout.cs`;
`src/Fable.Game/FrontendTextDraw.cs`;
`src/Fable.Formats/Defs/FrontendWidgetType.cs`;
`tests/Fable.Formats.Tests/FrontendLayoutTests.cs`;
`proofs/issue-36-verify`;
`proofs/audit-frontend-leftover`;
`proofs/glyph-uv-gaps`.

Do not invent dest writers. Do not treat font measure
(`00AB7B00` 301) as `+204` or as `0041AFA0` dest.

---

## Verdict

**PROVEN: no dest-width writer of type-6 `+204` / `+208`.**
First-seen type-6 has **no** leftover store.

| Claim | Status |
| --- | --- |
| Type-6 ctor `0054F5C0` / `0054ED90` writes `+204`/`+208` | **DISPROVEN** |
| `0041AC20` leftover on type-6 first-seen | **DISPROVEN** — only `.text` `E8` sites are type-0 ctor `0041B800` / copy `0041B870`. Factory type-6 arm is `0041D472` `call 0054F5C0` then join `0041D7A1` (`vtbl+332` only). |
| Native `0041AC20` leftover when `+376` / GraphicIndex `== 0` | **DISPROVEN** — `cmp [esi+376], ebx` / `jbe 0041AF6F` skips both `fstp`s. |
| First-seen `UI_PRESS_START_TEXT` GraphicIndex / `GraphicId` | **PROVEN** 0 |
| Dest width (`DestX1-DestX0` / persist W / `0041AFA0` dest) stored into type-6 `+204` | **DISPROVEN** |
| Host Collect `leftoverW = DestX1-DestX0` as `+204` | **LEFTOVER** vs native field. First-seen dest is a point → 0, so left-align **MATCH**. |
| `0054EF00` is a writer of `+204` | **DISPROVEN** — `fmul [esi+204]` reader, centre/right only. |
| `00AB7B00` width 301 is type-6 `+204` | **DISPROVEN** as a callee of `0054F5C0` / `0054EF00`. Do not invent. |
| Base `005334A0` zeros `+204`/`+208` | **DISPROVEN** — zeros `+196` then `+216`; later `+200`/`+212`. Hole. |
| Heap `00BFEA1A` zeros the hole | **UNREAD** (`jmp [0x1440150]` IAT). |
| First-seen type-6 `+204` value used by left align | unused → pen = origin+2 **MATCH** 0. |

**Answer:** nobody on the first-seen type-6 path. Leftover
stays the unwritten hole (treated as 0 for left). Host dest
width is not that store.

**Overall: PROVEN** (no dest-width writer; leftover stays 0
for first-seen type-6 GraphicIndex). Heap-zero of the hole
is **UNREAD** and does not invent a writer.

Leave #36 open for the Collect stand-in (and the other
issue-36 leftovers). Do not invent a dest writer to “fix”
`+204`.

---

## 1. Type-6 ctor (`listing-00540000` `0054F5C0`)

Factory `0041D21B` type 6 (`listing-00400000`):

```
0041D45C  push 0x18C
0041D461  call 00BFEA1A
0041D46F  push edi            ; def
0041D472  call 0054F5C0
0041D477  jmp 0041D7A1
```

`0054F5C0` (`listing-00540000`):

```
0054F5CA  call 0052CC50       ; listing-00500000 → 005334A0
0054F5D5  mov [esi], 0x1249CCC
… +348 string, +352/+356=0, +360 / +368 0099A2D0 objects …
0054F62A  call 0054ED90       ; font + align; not leftover
```

No `mov` / `fstp` of `[esi+204]` or `[esi+208]`.
`0052CC50` is 18 insns (`fn-0052CC50-exact.txt`): no
leftover. `0054ED90` writes `+352`/`+356` (face), copies
`def+164` → `+376` (16 bytes), `+392`, `or [+302]` from
`[def+508]`. **Not** `+204`/`+208`.

Join `0041D7A1` is `mov esi,eax` / `call [eax+332]` /
return. **No** `call 0041AC20`.

---

## 2. `0041AC20` leftover is GraphicIndex only, type-0 only

`listing-00400000` / `fn-0041AC20-exact.txt`:

```
0041ACD2  mov [esi+376], ebx     ; empty style list
0041ACD8  cmp [esi+376], ebx
0041ACDE  jbe 0041AF6F           ; skip leftover
… bank vtbl+84 …
0041AD19  fstp [esi+204]
… bank vtbl+88 …
0041AD69  fstp [esi+208]
0041AF6F  ; refcount / ret — no leftover store
```

`+376` is first style GraphicIndex (`[style+60]`). Zero →
no `fstp`.

Only `.text` `E8 0041AC20` sites in the listings:

| Site | Caller |
| --- | --- |
| `0041B85E` | type-0 ctor `0041B800` |
| `0041B8CE` | type-0 copy `0041B870` |

Type-0 ctor also **zeros** `+376` before the call
(`0041B84A mov [esi+376], eax` with `eax=0`), then
`0041AC20` may refill it from the style list. Type-6 never
enters that helper.

`listing-00500000` has **no** type-6 leftover `fstp`/`mov`
of widget `+204`/`+208`. Hits there are other objects or
`005339B0` **vtbl** `call [edx+204]` / `+208` (child
methods, not the float fields).

---

## 3. Other leftover stores — not type-6 first-seen

Do not retarget these onto type 6.

| VA | What | Type-6 first-seen |
| --- | --- | --- |
| `0041A8D0` / `0041A910` | same bank vtbl+84/+88 into `+204`/`+208` from `+376` | not a type-6 ctor callee |
| `0054E640` / `0054E680` | bank vtbl+84/+88 from **`+368`** | callee of `0053F819` (type 13 path), not `0054F5C0` |
| `00547D52` / `00547D67` | PlayAVI video w/h | not PRESS_START text |
| `0055135D` / `00551368` | raw `def+92`/`+88` (persist W/H floats) into `+204`/`+208` | other type; not `0054F5C0` |
| `0054350E` | different `esi` (packer / string object) | not the widget leftover |

`0054EF00` (`listing-00540000` `0054F08C` / `0054F098`)
**reads** `[esi+204]` after `vtbl+600` (`0054FFF0`) returns
1 or 2. Left (`eax` not 1/2) jumps to `0054F0AC` and never
loads `+204`.

---

## 4. First-seen type-6 GraphicIndex is 0

Persist / factory:

- `FrontendWidgetType.Text = 6`, ctor `0054F5C0`.
- `UI_PRESS_START_TEXT` GraphicIndex 0
  (`implementer/frontend/01-widget-construction.md`;
  `FrontendLayoutTests.Press_Start_first_seen_dest_table_matches_0041AFA0`
  `Assert.Equal(0, textWidget.GraphicId)`).
- Persist Width/Height 0.

Even if someone later called `0041AC20` on that widget,
`+376=0` would take the skip. First-seen does not call it.

---

## 5. Host: layout leftover MATCH; Collect dest-width LEFTOVER

`LayoutFrontendWidgets` leftover is GraphicIndex-gated
(native `0041AC20` shape). Type-6 dest is a point at the
remapped origin (`512,384`–`512,384`). That is **not** a
store into widget `+204`.

`FrontendLayout.ComputeSubmitDest` uses persist W/H else
leftover W/H as **dest size**, then `* destScale`. It does
not write a widget field.

`CollectFrontendRecords` then:

```7849:7852:src/Fable.Game/EngineLifecycle.cs
                    var leftoverW = MathF.Max(0f, widget.DestX1 - widget.DestX0);
                    var (penX, penY) = FrontendTextDraw.Type6Pen(
                        widget.DestX0, widget.DestY0, leftoverW, 1f,
                        FrontendTextDraw.AlignLeft);
```

Native `0054EF00` uses **widget `+204`**, not dest width.
Dest width is `0041AFA0` output (`+360` else `+204`, times
`+264`). Feeding dest W back as `+204` is circular and
**not** a recovered writer.

First-seen: dest W = 0, align left → pen = origin+2.
**MATCH** the unused-field case. Centre/right would need
the real `+204` (still 0 first-seen) and dest scale
`+264`, not hard `1f`.

---

## 6. Classification vs #36

Sibling `proofs/issue-36-verify`: type-6 `+204` writer was
**UNREAD**. This pass pins the first-seen writer:

- **No writer** on type-6 ctor / factory / `0041AC20`.
- **No dest-width writer.**

What stays open for #36 is the **host stand-in**, not an
unread native dest store.

| Item | Class |
| --- | --- |
| Native dest-width → type-6 `+204` | **DISPROVEN** |
| Native first-seen type-6 leftover write | **DISPROVEN** (none) |
| First-seen leftover should stay 0 | **PROVEN** as “unwritten”; left unused **MATCH**. Heap dword **UNREAD** |
| Host Collect dest W as `+204` | **LEFTOVER** |
| `00AB7B00` / font measure as `+204` | do not invent |

**Proposed (do not apply here):** pass leftover204 = 0
on first-seen type-6 (or a stored analog that stays 0).
Do not assign dest width. Pass dest `+264` into
`Type6Pen`. Leave #36 open until Collect stops that
stand-in.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00500000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00540000.txt`
- `C:\FableCSharp\implementer\frontend\fn-0041AC20-exact.txt`
- `C:\FableCSharp\implementer\frontend\fn-0054EF00-exact.txt`
- `C:\FableCSharp\implementer\frontend\fn-005334A0-exact.txt`
- `C:\FableCSharp\proofs\issue-36-verify\README.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\src\Fable.Game\FrontendLayout.cs`
