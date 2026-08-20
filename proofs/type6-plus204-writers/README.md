# Type-6 widget `+204` writers (`0054EF00` / `0054F5C0` / `0041AC20` / `0041AFA0`)

Investigation only. Production `src/` and `tests/` were not edited.

Question: confirm every `.text` writer of widget `+204` on
type-6 among `0054EF00`, `0054F5C0`, `0041AC20`,
`0041AFA0`. Is there any first-seen New Profile / Main
Menu type-6 with `GraphicIndex != 0` that would write
leftover204?

Sibling `proofs/type6-plus204-writer` answered Press Start
and disproved dest-width-as-`+204`. This pass pins the
four named VAs and the NP / MM trees.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`0041AC20` / `0041AFA0` / `0041B800` / `0041D21B`);
`listing-00540000.txt` (`0054ED90` / `0054EF00` /
`0054F5C0` / `0054E640` / `00547D52` / `00551340`);
`implementer/frontend/fn-0041AC20-exact.txt`,
`fn-0041AFA0-exact.txt`, `fn-0054EF00-exact.txt`,
`fn-0052CC50-exact.txt`, `fn-005334A0-exact.txt`;
`export/frontend/new-profile-dests.txt`,
`main-menu-dests.txt`;
`src/Fable.Formats/Defs/FrontendWidgetType.cs`
(type 6 ctor `0054F5C0`, vtbl `01249CCC`, draw
`0054EF00`);
`src/Fable.Game/EngineLifecycle.cs`
(`LayoutFrontendWidgets` leftover gate);
`src/Fable.Game/FrontendLayout.cs`
(`LeftoverFromGraphic`, `ComputeSubmitDest`).

Do not invent dest width. Do not treat `00AB7B00`
measure 301 as `+204` or as `0041AFA0` dest.

---

## Verdict

**PROVEN: none of the four VAs write type-6 widget `+204`
on first-seen New Profile / Main Menu.** Leftover stays
the unwritten hole (0 for left). Dest is a point; leftover
`+204` is **not** dest width.

| Claim | Status |
| --- | --- |
| `0054F5C0` writes `[esi+204]` / `[esi+208]` | **DISPROVEN** |
| `0054EF00` writes `[esi+204]` | **DISPROVEN** — `fmul [esi+204]` reader, centre/right only |
| `0041AFA0` writes `[edi+204]` | **DISPROVEN** — `fld [edi+204]` reader for dest size |
| `0041AFA0` writes dest from font measure | **DISPROVEN** |
| `0041AC20` leftover `fstp [esi+204]` when `+376 != 0` | **PROVEN** (bank vtbl+84) |
| `0041AC20` on type-6 ctor / factory join | **DISPROVEN** — only `.text` `call 0041AC20` sites are type-0 `0041B85E` / `0041B8CE` |
| `0041AC20` leftover when GraphicIndex / `+376 == 0` | **DISPROVEN** — `jbe 0041AF6F` skips both `fstp`s |
| First-seen NP / MM type-6 `GraphicIndex != 0` | **DISPROVEN** — all 18 rows `g=0` |
| First-seen NP / MM type-6 leftover204 written | **DISPROVEN** — none |
| Type-6 dest is dest width stored into `+204` | **DISPROVEN** — dest is a point; field is unused leftover |
| `00AB7B00` width 301 is type-6 `+204` | **DISPROVEN** as a callee of `0054F5C0` / `0054EF00` |
| Base `005334A0` zeros `+204`/`+208` | **DISPROVEN** — zeros `+196` then `+216`; later `+200`/`+212`. Hole. |
| Heap `00BFEA1A` zeros the hole | **UNREAD** (`jmp [0x1440150]` IAT) |

**Answer:** no first-seen New Profile / Main Menu type-6
has `GraphicIndex != 0`. Even if one did, type-6 never
calls `0041AC20`, so leftover204 would still not be
written by these four VAs.

**Overall: PROVEN** (no dest-width writer; leftover stays
0). Heap-zero of the hole is **UNREAD** and does not
invent a writer.

---

## 1. The four named VAs

### 1.1 `0054F5C0` — type-6 ctor, no leftover store

Factory `0041D21B` type 6 (`listing-00400000`):

```
0041D45C  push 0x18C
0041D461  call 00BFEA1A
0041D46F  push edi            ; def
0041D472  call 0054F5C0
0041D477  jmp 0041D7A1
```

`0054F5C0` (`listing-00540000` through `0054F640`):

```
0054F5CA  call 0052CC50       ; → 005334A0
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

### 1.2 `0054EF00` — type-6 `vtbl+8` reader, not writer

Type 6 draw is `0054EF00` (`proofs/draw-type10-fork`;
`implementer/frontend/14-container.md`). Listing
`0054EF00`…`0054F364` (`fn-0054EF00-exact.txt`):

```
0054F07C  call [edx+600]      ; 0054FFF0 align
0054F082  dec eax
0054F083  je 0054F094         ; centre
0054F085  dec eax
0054F086  jne 0054F0AC        ; left: never loads +204
0054F08C  fmul [esi+204]      ; right
0054F098  fmul [esi+204]      ; centre, then * 0.5
0054F0A4  fsubr [esp+16]
0054F0A8  fstp [esp+16]       ; pen X stack, not widget +204
```

No store of widget `+204`. Glyph packer is `00543910`
type `0x27`. **No** `call 00AB7B00` in this function
(`listing-00540000` has none). Font measure is not a
dest writer and is not leftover204.

### 1.3 `0041AC20` — leftover writer, type-0 only, GraphicIndex gate

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
no `fstp`. Persist Width/Height go to `+360`/`+364`
(`fld [def+92]/[def+88]` `00BFEA70`) **before** the gate.
That is not leftover `+204`.

Only `.text` `call 0041AC20` sites in the listings:

| Site | Caller |
| --- | --- |
| `0041B85E` | type-0 ctor `0041B800` |
| `0041B8CE` | type-0 copy `0041B870` |

Type-0 ctor zeros `+376` then may refill it from the
style list. Type-6 never enters this helper.

### 1.4 `0041AFA0` — dest reader of leftover, no write

Type 0 `vtbl+8`. `listing-00400000` `0041AFA0` /
`fn-0041AFA0-exact.txt`:

```
0041B065  mov eax, [edi+360]
0041B06B  test eax, eax
0041B06D  jne 0041B077
0041B06F  fld [edi+204]       ; leftover W if persist W==0
…
0041B0B5  fmul [edi+264]      ; dest scale
0041B0C7  fstp [esp+28]       ; dest size stack
```

Reads leftover into dest **size**. No `mov` / `fstp` of
`[edi+204]`. Dest output is origin `+248/+252` plus that
size (`* +264/+268`). Font measure is not in this
function.

On type-6 the draw path is `0054EF00`, not `0041AFA0`.
Host still uses `ComputeSubmitDest` for the widget dest
rect (point when persist W/H=0 and leftover=0). That dest
rect is **not** a store into widget `+204`.

---

## 2. First-seen New Profile / Main Menu type-6

Host dest dump column `g=` is persist `GraphicIndex`
(`FrontendUiDef.GraphicBankId` / widget `GraphicId`).
Column `+204=` is host leftover analog
(`LayoutFrontendWidgets` → `LeftoverFromGraphic`: 0 when
`GraphicId==0`). Dest `X0==X1` is a point.

### 2.1 New Profile (`export/frontend/new-profile-dests.txt`)

Screen `UI_FRONTEND_NEW_PROFILE_SCREEN`. Twelve type-6
rows; all `g=0`, `+204=0`, dest a point:

| Name | dest | g | +204 |
| --- | --- | --- | --- |
| `UI_TEXT_NEW_PROFILE_MENU_TITLE` | 166,113–166,113 | 0 | 0 |
| `UI_NEW_PROFILE_TEXT` | 115,243–115,243 | 0 | 0 |
| `UI_SCOREBOARD_EDITBOX_TEXT_FE` | 592,243–592,243 | 0 | 0 |
| `UI_OPTIONS_SLIDER_TEXT_CONTROL_METHOD` | −45,243–−45,243 | 0 | 0 |
| `UI_OPTIONS_TEXT_CONTROL_ARROWS` | 544,245–544,245 | 0 | 0 |
| `UI_OPTIONS_TEXT_CONTROL_WASD` | 544,245–544,245 | 0 | 0 |
| `UI_OPTIONS_SLIDER_TEXT_CAMERA_UP_DOWN` | −45,243–−45,243 | 0 | 0 |
| `UI_TEXT_NORMAL` | 544,245–544,245 | 0 | 0 |
| `UI_TEXT_INVERTED` | 544,245–544,245 | 0 | 0 |
| `UI_OPTIONS_SLIDER_TEXT_CAMERA_SENSITIVITY` | −45,355–−45,355 | 0 | 0 |
| `UI_ACCEPT_TEXT` | 784,680–784,680 | 0 | 0 |
| `UI_CANCEL_TEXT` | 237,680–237,680 | 0 | 0 |

`EngineLifecycleTests` locks `UI_TEXT_NEW_PROFILE_MENU_TITLE`
`DestX0==DestX1` and `Leftover204==0`.

### 2.2 Main Menu (`export/frontend/main-menu-dests.txt`)

Screen `UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE`.
Six type-6 rows; all `g=0`, `+204=0`, dest a point:

| Name | dest | g | +204 |
| --- | --- | --- | --- |
| `UI_TEXT_NEW_GAME` | 512,320–512,320 | 0 | 0 |
| `UI_TEXT_CHANGE_PROFILE` | 512,368–512,368 | 0 | 0 |
| `UI_TEXT_OPTIONS` | 512,416–512,416 | 0 | 0 |
| `UI_TEXT_CREDITS` | 512,608–512,608 | 0 | 0 |
| `UI_TEXT_ABOUT` | 512,656–512,656 | 0 | 0 |
| `UI_TEXT_QUIT_GAME` | 512,704–512,704 | 0 | 0 |

Nonzero `g` / leftover on these screens are **type 0**
sprites (`UI_TITLE_01` `g=3` `+204=256`, coastal tiles,
accept/back sprites, mouse area `g=366`). Those are
`0041AC20` on type-0, not type-6.

### 2.3 Would GraphicIndex ≠ 0 write leftover204 on type-6?

**No.** Type-6 ctor copies `def+164` into `+376` but never
calls `0041AC20`. The GraphicIndex gate lives inside
`0041AC20`, which type-6 does not enter. First-seen NP /
MM trees also have persist GraphicIndex **0**, so even a
mistaken retarget of `0041AC20` would take `jbe 0041AF6F`.

---

## 3. Other leftover stores — not type-6 first-seen

Do not retarget these onto type 6.

| VA | What | Type-6 first-seen |
| --- | --- | --- |
| `0041A8D0` / `0041A910` | same bank vtbl+84/+88 into `+204`/`+208` from `+376` | not a type-6 ctor callee |
| `0041B550` `fstp [edi+204]` | same bank vtbl+84 when arg GraphicIndex ≠ 0 | not `0054F5C0` |
| `0054E640` / `0054E680` | bank vtbl+84/+88 from **`+368`** | callee of `0053F819` (type 13 path) |
| `00547D52` / `00547D67` | PlayAVI video w/h | not NP / MM text |
| `0055135D` / `00551368` | raw `def+92`/`+88` (persist W/H floats) into `+204`/`+208` | other type; not `0054F5C0` |
| `0054350E` | different `esi` (packer / string object) | not the widget leftover |

`listing-00500000` leftover `mov`/`fstp` hits are other
objects or `005339B0` **vtbl** `call [edx+204]` / `+208`
(child methods, not the float fields).

---

## 4. Host analog (not a recovered native dest writer)

`LayoutFrontendWidgets` leftover is GraphicIndex-gated
(`FrontendLayout.LeftoverFromGraphic`). Type-6 dest is a
point at the remapped origin. That is **not** a store
into widget `+204`.

`CollectFrontendRecords` now passes `widget.Leftover204`
(0 first-seen) into `Type6Pen`. Feeding dest W
(`DestX1-DestX0`) back as `+204` would be circular and
**not** a recovered writer. First-seen dest W is also 0,
so left-align **MATCH** either stand-in.

Native `0054EF00` uses **widget `+204`**, not dest width.
Centre/right would need that field (still 0 first-seen)
and dest scale `+264`, not hard `1f`. Do not invent
measure 301.

---

## 5. Classification

| Item | Class |
| --- | --- |
| Native dest-width → type-6 `+204` | **DISPROVEN** |
| `0054EF00` / `0054F5C0` / `0041AFA0` write type-6 `+204` | **DISPROVEN** |
| `0041AC20` leftover on type-6 first-seen | **DISPROVEN** (not called; GraphicIndex 0 would skip) |
| NP / MM first-seen type-6 GraphicIndex ≠ 0 | **DISPROVEN** |
| First-seen leftover should stay 0 | **PROVEN** as “unwritten”; left unused **MATCH**. Heap dword **UNREAD** |
| Host dest W as `+204` | **LEFTOVER** vs native field; first-seen 0 **MATCH** |
| `00AB7B00` / font measure as `+204` | do not invent |

Leave #36 open for host leftovers that are not this
writer. Do not invent a dest writer to “fix” `+204`.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00540000.txt`
- `C:\FableCSharp\implementer\frontend\fn-0041AC20-exact.txt`
- `C:\FableCSharp\implementer\frontend\fn-0041AFA0-exact.txt`
- `C:\FableCSharp\implementer\frontend\fn-0054EF00-exact.txt`
- `C:\FableCSharp\export\frontend\new-profile-dests.txt`
- `C:\FableCSharp\export\frontend\main-menu-dests.txt`
- `C:\FableCSharp\proofs\type6-plus204-writer\README.md`
- `C:\FableCSharp\proofs\issue-36-verify\README.md`
