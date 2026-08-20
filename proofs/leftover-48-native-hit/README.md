# Leftover #48: native hit for type-16/37 point dest

Investigation only. No production `src/` edits.
Do **not** delete `TryChromeHit` (New Profile
`(700,300)` / `New_Profile_apply_cancel_hit_rects_are_disjoint`
needs it). `PlaceTableCell` `n==3` is **not**
native.

Question: recover native hit for type-16/37 when
dest is a point. Does `0055B8F0` invent size from
the rightmost type-2? If not, what does it use?
Next site to replace `TryChromeHit` without
breaking New Profile Accept.

Authority: dump only.
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0055B8F0` / `0055BF10` / `005491A0` / `00540CF0` /
`00549440` / `00551EA0` / `0054FF50`);
`listing-00500000.txt`
(`0052EEC0` / `0052ECC0` / `00531090` / `00531560` /
`0052E890` / `005339B0`);
`listing-00400000.txt` (`0041A850` / `0041C660`);
`e8.tsv` (**no** `.text` `E8 0055B8F0`);
ExeIndex `vtbl` on `01248A8C` / `01246B8C` /
`01249554` / `0124B04C` / `0124A224` / `01245DE4` /
`0122F5D4` / `012497E4` / `01249CCC` / `01248A68`;
`export/frontend/new-profile-dests.txt`;
`src/Fable.Game/FrontendHitTest.cs`;
`src/Fable.Game/FrontendLayout.cs`
(`NativeHitFn` / `TryChromeHitIsNativeHit=false` /
`Type12DestIsPointWhenSizeZero=true`);
`src/Fable.Game/EngineLifecycle.cs`
(`TryChromeHit` / `AssignHitRects`);
`proofs/leftover-48-dest`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

---

## Verdict

**`0055B8F0` does not invent size from a
rightmost type-2.** It never walks siblings.
It is **not** type-16/37 hit.

`0055B8F0` is type **11/38** `vtbl+568` only.
AABB is this widget’s dest **origin**
(`vtbl+488` `0052EEC0`) plus dest **scale**
(`vtbl+492` `0052ECC0`) times a 4-float extra
from **`+176` children** (`vtbl+96` `00531090`).
No children → extra `0,0,0,0` → empty hit
even when dest has area. A point dest still
hits when a child has authored leftover /
persist W (Accept’s type-0 mouse area).

Type-16 `01248A8C+568` is `005491A0` (0-arg
`+404` copy). Type-37 `01246B8C+568` is
`00540CF0` (string getter). Neither is a
point-in-rect. Type-2 `0124A224+568` is
`00551EA0` (clone cursor; **not** hit; **no**
`cmp count, 3`).

Host `TryChromeHit` copies the rightmost
type-2 **dest size** onto the type-16/37
**dest origin** and claims `0055B8F0`. That
is **DISPROVEN**. Leave the helper. New
Profile Accept does not use it: type-38 dest
stays a point; host reverse-walks the type-0
mouse-area dest and `InteractiveAt` walks up.

**Answer:** recover type-11 parent
`0055B8F0` child-union for the edit-box
**row**. Do not copy that AABB onto type-37
dest. Type-16 mouse AABB is **UNREAD**;
inner `00549440` is actions 4–21, not 25/26.
Next site is `00549440` / who maps LMB onto
type 16 — not a `TryChromeHit` delete.

| Claim | Class |
| --- | --- |
| Type 11/38 `vtbl+568` dword is `0055B8F0` | **PROVEN** rdata |
| Type 16 `vtbl+568` is `0055B8F0` | **DISPROVEN** (`005491A0`) |
| Type 37 `vtbl+568` is `0055B8F0` | **DISPROVEN** (`00540CF0`) |
| Type 2 `vtbl+568` is `00551EA0` | **PROVEN** rdata |
| `00551EA0` `count==3` leftover fill | **DISPROVEN** (no such compare) |
| `0055B8F0` walks rightmost type-2 dest size | **DISPROVEN** |
| `0055B8F0` AABB = `vtbl+488` origin + `vtbl+492` scale × `vtbl+96` extra | **PROVEN** |
| `vtbl+488` `0052EEC0` is persist target `+60/+64` remap + parent | **PROVEN** |
| `vtbl+492` `0052ECC0` is persist scale `+100/+104` remap + parent | **PROVEN** |
| `+100/+104` is dest pixel size | **DISPROVEN** (`005339B0` copies layout `+16`) |
| `vtbl+96` `00531090` extra is `+176` child AABB | **PROVEN** |
| Extra includes sibling type-2 not on `+176` | **DISPROVEN** |
| Empty `+176` → extra `0,0,0,0` → empty hit | **PROVEN** |
| Type-16/37 dest is a point | **PROVEN** dump |
| Type-12 dest is a point when persist/leftover size 0 | **PROVEN** (`Type12DestIsPointWhenSizeZero`) |
| `TryChromeHit` is native `0055B8F0` | **DISPROVEN** (`TryChromeHitIsNativeHit=false`) |
| New Profile Accept needs `TryChromeHit` | **DISPROVEN** (type 38 skipped; child dest walk) |
| Type-16 `(700,300)` needs `TryChromeHit` today | **PROVEN** host test |
| Type-16 inner `00549440` is LMB AABB | **DISPROVEN** (actions 4–21) |
| Type-16 mouse AABB writer | **UNREAD** |
| Type-6 leftover `+204` is dest width | **DISPROVEN** |

---

## Evidence

### 1. `0055B8F0` body (`listing-00540000`)

```
0055B8F0  sub esp, 32
          esi = this
          vtbl+488 → [esp+12] origin.x/y
          vtbl+492 → [esp+20] scale.x/y
          vtbl+96  → [esp+28] extra[0..3]
          left   = fistp(origin.x + extra[0])
          top    = fistp(origin.y + extra[1])
          right  = fistp(origin.x + scale.x * extra[2])
          bot    = fistp(origin.y + scale.y * extra[3])
          point = [arg]
          hit iff left <= x < right && top <= y < bot
          ret 4
```

No `+176` walk, no type compare, no type-2
load. `.text` `E8 0055B8F0` is empty. Dispatch
is `vtbl+568` only.

`0055BF10` (type 11/38 `vtbl+580`) builds a
mouse rect from input `+184` `vtbl+64` /
`vtbl+92`, then `this.vtbl+568` (`0055B8F0`)
with that stack point. Peer walk on
`0x13B8AD4` also calls **that** widget’s
`vtbl+568`. Type 16/37 are not on that list.

### 2. Getters (shared on 11/16/37/38/2/5/10)

| Slot | VA | Body |
| --- | --- | --- |
| `+488` | `0052EEC0` | persist **target** `+60/+64`; optional `0052E580` if `vtbl+468`; else parent `vtbl+492` × pos + parent `vtbl+488` |
| `+492` | `0052ECC0` | persist **scale** `+100/+104`; optional global `0041CF47`; optional remap; else × parent `vtbl+492` |
| `+96` | `00531090` | if `+176==+180`: store `0,0,0,0`. Else union children `vtbl+64` pos + `vtbl+444` size (centre via `vtbl+424` × `[0x122F59C]=0.5`) |
| `+472` | `0052F270` | copy dest origin `+248/+252` (not used by `0055B8F0`) |
| `+476` | `0052F290` | copy dest scale `+264/+268` (not used by `0055B8F0`) |

`005339B0` writes layout `+16/+20` into
`+92/+96` **and** `+100/+104`. First-seen
scale is 1; remap × parent makes dest scale
(1.6 when `+302` bit 6). `vtbl+492` is **not**
`0041AFA0` dest W/H. Point dest
(`persistW==0 && leftover==0`) does **not**
force `vtbl+492` to 0.

`vtbl+64` is `0052E890` (`+52/+56` authored
pos). Leaf `vtbl+444`: type 0 `0041C660` →
`vtbl+92` `0041A850` (`+360` else leftover
`+204 * +92`). Type 6 `vtbl+444` is
`0054FF50` (raw leftover `+204/+208`). Nested
container `vtbl+444` is `00531560` (same
child union as `00531090` without the 4-float
pad).

Collapsed first-seen:

```
left   = destOrigin.x + childMinAuthoredX
top    = destOrigin.y + childMinAuthoredY
right  = destOrigin.x + destScale.x * childMaxAuthoredX
bot    = destOrigin.y + destScale.y * childMaxAuthoredY
```

Sibling type-2 tables that are **not** on
this widget’s `+176` never enter extra.

### 3. Who owns `vtbl+568` (rdata)

| Type | Vtbl | `+568` | ABI |
| --- | --- | --- | --- |
| 11 | `01249554` | `0055B8F0` | `ret 4` point-in-rect |
| 38 | `0124B04C` | `0055B8F0` | same |
| 16 | `01248A8C` | `005491A0` | `ret` 0-arg; `+348/+352/+356 = +404` |
| 37 | `01246B8C` | `00540CF0` | `ret 4`; copy `+356` string via `0099B720` |
| 2 | `0124A224` | `00551EA0` | clone layout `+8/+12`; no `cmp …, 3` |
| 5 | `01245DE4` | `01339D04` | rdata, not a hit fn |
| 10 | `012497E4` | `0054E550` | `[+360]` bind, not hit |

Type 11/38 `vtbl+580` is `0055BF10`. Type 16
`vtbl+580` is `0054A480`. Type 37 `vtbl+580`
is `0053FE50` (edit insert).

---

## Original

New Profile dump (`new-profile-dests.txt`):

```
UI_NEW_PROFILE_BUTTON              t=11  dest=64,240,64,240
  UI_BUTTON_OPTIONS_LEFT           t=2   dest=64,240,352,272   +204=180
  UI_BUTTON_OPTIONS_RIGHT_EDITBOX  t=2   dest=576,235,928,267  +204=220
  UI_NEW_PROFILE_EDIT_BOX          t=37  dest=592,240,592,240  +204=0
UI_OPTIONS_CONTROL_METHOD_TEXT_SLIDER t=16 dest=608,293,608,293 +204=0
  UI_OPTIONS_TEXT_CONTROL_ARROWS   t=6   dest=704,293,704,293  +204=0
  UI_BUTTON_OPTIONS_RIGHT          t=2   dest=594,283,786,315  +204=120   ; sibling, not +176 of type 16
UI_ACCEPT_NEW_PROFILE              t=38  dest=579,672,579,672
  UI_HELPER_BUTTON_MOUSE_AREA      t=0   dest=579,672,979,720  +204=64
```

Type-12 `UI_NEW_PROFILE_MENU` dest
`64,240,64,240` **MATCH**
`Type12DestIsPointWhenSizeZero`.

Native Accept hover is type-38 `0055BF10` →
`0055B8F0`. Extra includes the type-0 mouse
area leftover / persist W, so the point dest
still has a hit AABB. Type-37 dest stays a
point; the **row** that can 0055B8F0 is
parent type 11 (`UI_NEW_PROFILE_BUTTON`),
whose `+176` union walks the type-5 tables
→ type-2 leftover. That is a **parent**
AABB, not a type-37 dest write.

Type-16 parent is type 5, not type 11. Type
5 has no `0055B8F0`. Type-16 `+176` kids are
type-6 leftover 0. Native mouse AABB for
type 16 is **UNREAD**. Inner `01248A68+4`
`00549440` switches `action-4` for 18
values (actions 4–21). Action 25/26 fall
through.

---

## Host

`FrontendLayout.NativeHitFn = 0x0055B8F0`.
`TryChromeHitIsNativeHit = false`.
`FrontendHitTest.HitTestFn` claims the same
VA. Comments still say dest origin + dest
size; rdata says origin + **scale** × child
extra.

`AssignHitRects`: dest AABB if area, else
`TryChromeHit` for type 16/37 only:

```
walk up to type-12 list
rightmost type-2 dest under that row
hit = (destX0, destY0, destX0+type2W, destY0+type2H)
```

`(700,300)` lands in
`UI_BUTTON_OPTIONS_RIGHT` dest
`594,283,786,315` and in the invented type-16
hit `608,293,800,325`. Type 2 is not
`IsInteractive`; without the invent,
`HitIndex` would miss the slider.

Type 38 is **not** in `TryChromeHit`. Accept
`Hit*` equals dest point. `HitIndex` at
`TryDestPoint` walks the type-0 mouse-area
dest, then `InteractiveAt` to type 38.
`New_Profile_apply_cancel_hit_rects_are_disjoint`
locks that. Deleting `TryChromeHit` breaks
`(700,300)` / nonempty type-16/37 chrome
hit; it does **not** break Accept.

`PlaceTableCell` `n==3` leftover fill stays
host (`PlaceTableCellCount3IsNative=false`).
Not this hit question.

---

## Gap

| Object | Native | Host | Gap |
| --- | --- | --- | --- |
| Type 11/38 hit | `0055B8F0` origin + scale × `+176` extra | reverse-walk dest; type 38 Hit=dest point | **PARTIAL** effect MATCH on Accept via child dest; formula **LEFTOVER** |
| Type 16 hit | **UNREAD** (not `0055B8F0`) | `TryChromeHit` dest origin + rightmost type-2 dest size | **LEFTOVER** invented |
| Type 37 hit | parent type 11 `0055B8F0` row AABB; self slot is string get | same `TryChromeHit` on the edit box | **LEFTOVER** (wrong object) |
| Type 2 `+568` | `00551EA0` clone cursor | `PlaceTableCell` `n==3` | **LEFTOVER** (dest #48, not hit) |
| `(700,300)` | **UNREAD** as native type-16 | `HitIndex` → slider | keep helper |
| Accept LMB `0x126` | type 38 `0055BF10` then action 26 | child dest + `Hovered` | **MATCH** path; dest still a point |

**Next site (do not delete `TryChromeHit`):**

1. Type-11 `UI_NEW_PROFILE_BUTTON` `0055B8F0`
   child-union as the **row** hit. Do not
   write that rect onto type-37 dest. Do not
   treat it as type-16.
2. Type-16 mouse: who calls inner
   `00549440` / `vtbl+560` `00548760`
   (action 25 subscribe). Not 25/26 in the
   switch. Until that maps LMB to the
   slider, leave `TryChromeHit`.
3. Do **not** replace `TryChromeHit` with
   dest-AABB-only (`6e76ac5`). That
   regresses `(700,300)` and does not
   recover `0055B8F0`.

---

## UNREAD sites

- Type-16 LMB → `+348` writer (not
  `0055B8F0`, not `00549440` actions 4–21).
- Exact `00531090` extra numbers on New
  Profile type 11/38 (persist W vs leftover
  64 on `UI_HELPER_BUTTON_MOUSE_AREA`;
  authored vs dest-space unit on extra[0]
  vs extra[2] × scale).
- Whether type-11 row hit then focuses
  type 37 (`00540120` actions 33/34).
- Type-12 `vtbl+568` dword (list).

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00540000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00500000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\export\frontend\new-profile-dests.txt`
- `C:\FableCSharp\proofs\leftover-48-dest\README.md`
- `C:\FableCSharp\src\Fable.Game\FrontendHitTest.cs`
- `C:\FableCSharp\src\Fable.Game\FrontendLayout.cs`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
