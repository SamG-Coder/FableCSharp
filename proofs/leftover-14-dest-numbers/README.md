# Leftover #14 dest AABB — native dest 4-tuple numbers

Investigation only. Production `src/` and `tests/` were
not edited. Do **not** invent dest fill. Do **not**
plant `512,384`. Do **not** re-enable `Key.N` /
`ActivateNewGame`.

Question: recover the native dest 4-tuple numbers
written on first dest Present. That write is type-0
stack `[esp+36],[esp+40],[esp+44],[esp+48]` after
`0041B173` snap, copied into type-`0x22` rec
`+12..+24` (`0041BEB0`). Listing immediates? PE
constants? `export/frontend/press-start-dests.txt`?

Authority: `Fable.exe` listings
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`0041AFA0` / `0041B173` / `0041BEB0` / `0042DF9E`),
`listing-00540000.txt` (`0054EF00` / `00595222`),
`listing-00600000.txt` (`0061B308`),
`listing-00640000.txt` (`0064E386`),
`listing-00b40000.txt` (`00B4D3D3`);
`implementer/frontend/fn-0041AFA0-exact.txt`,
`fn-0041BEB0-exact.txt`, `fn-0054EF00-exact.txt`;
`export/frontend/press-start-dests.txt`,
`press-start-frame.txt`;
`export/native/`;
`assembly/compiled-defs/frontend/`
(`0157-UI_TITLE_01.md`, `0200-UI_TITLE.md`,
`0623-UI_PRESS_START_TEXT.md`,
`0643-UI_FRONTEND_BG_FORREST_1_1.md`);
`src/Fable.Game/FrontendLayout.cs`
(`NativeDestTupleUnread`);
`src/Fable.Game/FrontendInputMap.cs`
(`Leftover14OpenForDestPresentNotes`);
siblings `proofs/leftover-14-dest-aabb`,
`proofs/leftover-14-present-dest`,
`proofs/leftover-14-native-key`,
`proofs/0041AC20-dest-formula`,
`proofs/0041B173-stack-dest`,
`proofs/leftover-36-native-dest`,
`proofs/leftover-36-dest-dump`,
`proofs/005301B0-plus248-first`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Do not re-prove LMB type 4 / 6, Type4 current-inner
`0055CB10`, GraphicIndex leftover, or dest **formula**.
Dest **formula** stays in `proofs/0041AC20-dest-formula`.
Dest AABB **writer** stays in `proofs/leftover-14-dest-aabb`.
This note is only the dest **numbers**.

`FrontendInputMap.Leftover14OpenForDestPresentNotes = true`.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| Dest AABB 4-tuple native writes onto the widget on first dest Present? | **None.** Present `0042DF9E` → `00595222` `vtbl+8` never stores `DestX0,Y0,X1,Y1` on the widget | **DISPROVEN** as a widget store (`leftover-14-dest-aabb`) |
| Dest 4-tuple native **does** write on first dest Present? | Type-0 stack `[esp+36..48]` after `0041B173` snap, copied into type-`0x22` rec `+12..+24` (`0041BEB0`). Type-6 has **no** dest AABB (pen `+248`) | **PROVEN** sites |
| Listing immediates of that dest `X0,Y0,X1,Y1` at `0041B173` / `0041BEB0`? | **None.** Snap of origin / leftover / dest scale. Centre uses `[0x122F59C]=0.5`. Rec type `0x22`. No `push 0x200` / `0x180`, no `0x44000000` / `0x43C00000` | **DISPROVEN** immediates |
| PE constant dest 4-tuple for first dest Present? | **None.** Stray `0x43CD0000` (410.0f) / `0x44000000` (512.0f) live in HUD/3D, not this Present. `0x43C00000` (384.0f) is **absent** from listings | **DISPROVEN** as dest Present |
| `export/frontend/press-start-dests.txt` native `[esp+36..48]`? | **No.** Host `LayoutFrontendWidgets` analog. Forest `410` / title `112,48,522,253` / type-6 `512,384,512,384` | **LEFTOVER** analog (#36) |
| Native first-seen `[esp+36..48]` / rec `+12..+24` numbers? | **UNREAD.** No process dump, minidump, PIX/ETL, PNG `tEXt` | **UNREAD** |
| Plant dest `512,384` / forest `410` into `src/`? | **No.** Host analog leftover #36. Do not invent dest fill | **DISPROVEN** as native dest |
| Close leftover #14 dest / Present Notes? | **No.** Dest numbers still unread; Present `009DA9F0` still Note-only; Type4 still dest AABB hover | **LEFTOVER** open |

---

## Verdict

**Native dest 4-tuple numbers on first dest Present
are UNREAD. Listing immediates do not exist. PE
constants do not hold that 4-tuple.
`press-start-dests.txt` is a host analog, not a
native dump. Leave leftover #14 dest AABB analog
open. Leave leftover #36 dest-lock open. Do not
invent dest fill.**

Present `0042DF9E` walks `[node+20].vtbl+8`
(`00595222`). That walk never stores dest on the
widget. Type-0 `0041AFA0` builds dest on the
**stack**, snaps it at `0041B173`, then
`0041BEB0` copies the four floats into the sprite
record. Those four numbers are runtime products
of persist / leftover / dest scale. They are not
listing immediates and they are not a dumped
dword.

Type-6 `UI_PRESS_START_TEXT` never reaches
`0041B173`. Native dest writer there is
`0054EF00` pen `+248`. Host dest point
`512,384,512,384` is leftover **#36** analog
of applying the type-0 dest size rule to a
type-6 widget whose leftover and persist size
are 0. Do **not** plant it.

**Answer:** `[esp+36..48]` numbers stay
**UNREAD**.

---

## 1. Evidence — listing immediates at the dest write

`listing-00400000.txt` / `fn-0041AFA0-exact.txt`:

```
0041B0AD  mov eax, [edi+248]      ; origin X bits
0041B0B5  fmul [edi+264]          ; size W * dest scale
0041B0DD  mov [esp+36], edx       ; dest X0 = origin X
0041B0FD  mov [esp+40], eax       ; dest Y0
0041B10D  fstp [esp+44]           ; dest X1
0041B123  fstp [esp+48]           ; dest Y1
0041B127  call [edx+424]          ; centre? else 0041B173
0041B173  fld [esp+36]
0041B177  fistp [esp+12]          ; snap; not a widget store
          … same +40 / +44 / +48 …
0041B1AF  fstp [esp+48]
0041B4E6  call 0041BEB0
```

`0041BEB0` (`fn-0041BEB0-exact.txt`):

```
0041BEBD  mov [eax], 0x22
0041BECF  mov esi, [ecx]          ; dest X0
0041BED1  mov [eax+12], esi
0041BED7  mov [eax+16], esi       ; dest Y0  (from [ecx+4])
0041BEDD  mov [eax+20], esi       ; dest X1  (from [ecx+8])
0041BEE3  mov [eax+24], ecx       ; dest Y1  (from [ecx+12])
```

Copy of a dest **pointer**. Not dest numbers.

Immediates at this site:

| Immediate | Meaning | Dest 4-tuple? |
| --- | --- | --- |
| `[0x122F59C]` | centre half (`0.5`) | **No** |
| `[0x122DCB4]` | unsigned `fild` bias | **No** |
| `0x22` at `0041BEBD` | record type | **No** |
| `0xC0` at `0041B503` | record size | **No** |

`fn-0041AFA0-exact.txt` has **no** `push 0x200`,
`push 0x180`, `0x44000000` (512.0f), or
`0x43C00000` (384.0f). Dest formula recovered
elsewhere (`0041AC20-dest-formula`):

```
w = (+360 != 0) ? (float)+360 : +204
h = (+364 != 0) ? (float)+364 : +208
w *= +264
h *= +268
dest = centre ? (ox±w/2, oy±h/2) : (ox, oy, ox+w, oy+h)
fistp/fild snap onto [esp+36..48]
```

Formula recover is **not** dest numbers.

Type-6 `0054EF00` (`fn-0054EF00-exact.txt`):
`fld [esi+248]` pen, `fistp [esp+40]`, packer
`00543910` type `0x27`. No `[esp+36..48]` dest
4-tuple. No `0041AFA0`.

---

## 2. Evidence — PE constants that look like dest numbers

Grep of `listing-*.txt` for host dest analog
floats:

| Bits | Float | Listing hits | Dest Present? |
| --- | ---: | --- | --- |
| `0x43C00000` | 384.0 | **none** | **DISPROVEN** as a PE dest constant |
| `0x44000000` | 512.0 | **one:** `00B4D3D3` `push 0x44000000` | **DISPROVEN** — 3D helper (`ebx+700` / `00B4B8B0`), not `0041AFA0` |
| `0x43CD0000` | 410.0 | `0064E386` `mov [ebp+44], 0x43CD0000`; `0061B308` `mov [eax+4], 0x43CD0000` | **DISPROVEN** — not dest Present |

`0064E386` (`listing-00640000.txt`) is a 3D
object with `fldz` zeros, vtbl `0x125A600`,
`call 00643D5D`. Not frontend Present dest.

`0061B308` (`listing-00600000.txt`) stores
`(0x43BE0000, 0x43CD0000)` = `(380.0, 410.0)`
into a HUD/text helper (`0099B6B0` /
`005BCE49`, loop `cmp ecx, 12`). That pair
is **not** forest dest `0,0,410,410`.

Authored / viewport PE constants that dest
**formula** reads:

| VA | Role | Dest 4-tuple? |
| --- | --- | --- |
| `0x01375CD4` | authored W `640` | **No** — divisor |
| `0x01375CD8` | authored H `480` | **No** |
| `0x013B876C` / `70` | viewport W/H (runtime `1024×768`) | **No** — live store |
| `0x0122F59C` | centre `0.5` | **No** |

`FrontendLayout.AuthoredWidth = 640f` /
`GlobalWidthFloor = 1024f` are those
constants. Product analog `1.6` is
**not** a dumped dest dword
(`005301B0-plus248-first`).

Persist **inputs** (frontend.bin, not PE
dest tuples):

| Widget | Type | Persist | Leftover gate |
| --- | ---: | --- | --- |
| `UI_FRONTEND_BG_FORREST_1_1` | 0 | pos `(0,0)` size `0×0` graphic `206` | `+376 != 0` → bank frame |
| `UI_TITLE` | 5 | pos `(70,30)` size `0×0` graphic `0` | leftover 0 |
| `UI_TITLE_01` | 0 | pos `(0,0)` size `0×0` graphic `3` | bank frame |
| `UI_PRESS_START_TEXT` | 6 | pos `(320,240)` size `0×0` graphic `0` | leftover 0; **no** dest 4-tuple |

Those are dest **inputs**. They are not
`[esp+36..48]` after snap.

---

## 3. Evidence — `press-start-dests.txt` is host analog

`export/frontend/press-start-dests.txt` header:
`screen=UI_FRONTEND_PRESS_START_MENU stage=Frontend`.
`implementer/frontend/17-press-start-frame.txt`
header: “Engine-state Press Start frame **(not a
screenshot)**”. Both are
`LayoutFrontendWidgets` / `DumpFrontendFrame`.

First-seen host dest (1024×768 analog):

| Widget | Type | Host dest | Native dest write |
| --- | ---: | --- | --- |
| `UI_FRONTEND_PRESS_START_MENU` | 10 | `0,0,0,0` | none on widget; type-10 Present is child walk |
| `UI_FRONTEND_BG_FORREST_1_1` | 0 | `0,0,410,410` | stack / rec only; numbers **UNREAD** |
| `UI_TITLE_01` | 0 | `112,48,522,253` | same |
| `UI_PRESS_START_TEXT` | 6 | `512,384,512,384` | **none** — pen `+248` |
| `UI_LEGAL_TEXT` | 6 | `512,544,512,544` | **none** — pen `+248` |
| `UI_FRONTEND_BUTTON_INVISIBLE` | 11 | `320,320,320,320` | Present none; hover AABB extra **UNREAD** |
| `UI_MOUSE_POINTER` | 32 | `0,0,32,32` | not `0041B173` dest AABB |

`512,384` is `320*(1024/640), 240*(768/480)`.
Forest `410` is host snap of `256*1.6`.
Title `112,48` is `70*1.6, 30*1.6`. Analog of
the type-0 **formula**. **Not** a native dump
of `[esp+36..48]`. Leftover **#36**.

`implementer/frontend/11-transform.md` dest
table is a 640-space calculator (TITLE_01
`70,30,326,158`, TEXT `320,240,320,240`) with
remap bits still 0. **STALE** vs root
`def+520=1` (`16-resolution.md`). Also not a
process dump.

`export/native/` is screenshots. PNG `tEXt` /
`iTXt` dest tuples: **none**
(`leftover-36-dest-dump`). Pixels are not
`[esp+36..48]`.

What a dest-numbers dump would have to
contain (any one would close this unread
site):

| Dump | Site | Status in repo |
| --- | --- | --- |
| Stack `[esp+36..48]` after `0041B1AF` | type-0 Present | **UNREAD** — none |
| Type-`0x22` rec `+12,+16,+20,+24` | `0041BEB0` | **UNREAD** — none |
| Widget `+248/+252` after `005301B0` | layout origin | **UNREAD** — none |
| FPU RC / exact `fistp` of `256*1.6` | snap bits | **UNREAD** |
| Debugger / minidump / PIX of first-seen | process | **UNREAD** — no `*.dmp` `*.pix` `*.etl` |

---

## 4. Original — first dest Present dest write

```
0042DF9E
  009D8CF0 / 009BEF20
  00595582 / 00595222 [ui+84] vtbl+8
    type 0:  0041AFA0 stack dest → 0041B173 snap
             → 0041BEB0 rec +12..+24
    type 6:  0054EF00 pen +248; no dest AABB
    type 10/5/11/12: 00530260 child vtbl+8
  009DA9F0(1)×2 empty +16020 skip
  009BEF50 / 009BEEB0
```

First dest Present that has area is Press
Start type-0 tiles (GraphicIndex ≠ 0 →
leftover `+204` from bank frame). Dest AABB
for Type4 is **not** that Present dest
(`leftover-14-dest-aabb`). Type-6
`UI_PRESS_START_TEXT` never builds the
4-tuple.

Native dest **formula** is recovered. Native
dest **numbers** of that snap are **UNREAD**.

---

## 5. Host — dest analog leftover #36, not leftover #14 fill

Host **stores** dest 4-tuples on every widget
(`LayoutFrontendWidgets` / `ComputeSubmitDest`).
That store has **no** listing dest AABB writer.

`FrontendLayout.NativeDestTupleUnread = true`.
`SubmitDestStoresOnWidget = false`.
`Type6DrawWritesDestRect = false`.
`Leftover14OpenForDestPresentNotes = true`.

Leftover **#14** dest AABB analog in apply /
Present is still:

- Type4 apply via dest AABB `HitIndex`
  (native is current-inner `0055CB10`)
- hover `Contains || HitIndex` stand-in for
  `+352` u8
- Present skip `DestX1<=DestX0` on stored dest
- `009DA9F0` Note-only empty `+16020`

Closing dest **numbers** would dest-lock
leftover **#36**, not the leftover **#14**
apply / Present Notes. This note recovers
neither.

Do **not** edit dest to invented tuples.
Do **not** plant `512,384`. Do **not** treat
`press-start-dests.txt` as `[esp+36..48]`.

---

## Gap

```
Evidence              Original                         Host                          Gap
0042DF9E Present      00595222 vtbl+8; type-0          stored widget Dest* then      Host dest AABB on widgets
                      stack dest → 0x22 rec +12..+24   IssueRecoveredDraws skip      has no listing writer.
                      numbers UNREAD                   DestX1<=DestX0
0041B173 [esp+36..48] snap on stack; no dest imm       512,384 / 410 lattice         leftover #36 dest-lock
0041BEB0 rec +12..+24 dest copy; values UNREAD         host rec dest from widget     analog, not a dump
PE 0x44000000 /       HUD/3D stray; not dest Present   dest analog lock              DISPROVEN as dest fill
0x43CD0000
press-start-dests.txt host LayoutFrontendWidgets       same file                     not native dest
0054EF00 type-6       pen +248; no dest AABB           dest point 512,384,512,384    leftover #36 analog
009DA9F0(1)×2         empty +16020 skip                Note-only                     leftover #14 / #36 Present
```

| Claim | Class |
| --- | --- |
| Native dest AABB 4-tuple on widget at first dest Present | **DISPROVEN** |
| Type-0 Present dest is stack `[esp+36..48]` then rec `+12..+24` | **PROVEN** |
| Listing immediates of dest `X0,Y0,X1,Y1` at `0041B173` | **DISPROVEN** |
| PE dest 4-tuple `512` / `384` / forest `410` for this Present | **DISPROVEN** |
| `press-start-dests.txt` is native `[esp+36..48]` | **DISPROVEN** — host analog |
| Native first-seen dest 4-tuple numbers | **UNREAD** — leftover #36 dest-lock |
| Type-6 dest AABB writer | **DISPROVEN** |
| Host dest `512,384` / forest `410` | **LEFTOVER** analog (#36) |
| MATCH dest AABB writer / dest fill to put in `src/` | **DISPROVEN** — none |
| Close leftover #14 dest / Present Notes | **DISPROVEN** — stays open |
| Close leftover #36 dest-lock | **DISPROVEN** — stays open |

**Overall: UNREAD** for leftover #14 dest
**numbers**. Sites and formula are recovered
elsewhere. **Leave #14 and #36 open.**

**Proposed (do not apply here):** keep LMB
Type4/Type6. Do not restore `Key.N`. Do not
invent dest size / `512,384`. Keep Present
dest as stack / type-`0x22` rec, not a widget
dest AABB field. Dest-lock numbers need a
dump of `[esp+36..48]` / rec `+12..+24`, not
a dest fill from `press-start-dests.txt`.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00600000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00640000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00b40000.txt`
- `C:\FableCSharp\implementer\frontend\fn-0041AFA0-exact.txt`
- `C:\FableCSharp\implementer\frontend\fn-0041BEB0-exact.txt`
- `C:\FableCSharp\implementer\frontend\fn-0054EF00-exact.txt`
- `C:\FableCSharp\export\frontend\press-start-dests.txt`
- `C:\FableCSharp\export\frontend\press-start-frame.txt`
- `C:\FableCSharp\assembly\compiled-defs\frontend\0157-UI_TITLE_01.md`
- `C:\FableCSharp\assembly\compiled-defs\frontend\0200-UI_TITLE.md`
- `C:\FableCSharp\assembly\compiled-defs\frontend\0623-UI_PRESS_START_TEXT.md`
- `C:\FableCSharp\assembly\compiled-defs\frontend\0643-UI_FRONTEND_BG_FORREST_1_1.md`
- `C:\FableCSharp\proofs\leftover-14-dest-aabb\README.md`
- `C:\FableCSharp\proofs\leftover-14-present-dest\README.md`
- `C:\FableCSharp\proofs\0041AC20-dest-formula\README.md`
- `C:\FableCSharp\proofs\0041B173-stack-dest\README.md`
- `C:\FableCSharp\proofs\leftover-36-dest-dump\README.md`
- `C:\FableCSharp\proofs\005301B0-plus248-first\README.md`
- `C:\FableCSharp\src\Fable.Game\FrontendLayout.cs`
- `C:\FableCSharp\src\Fable.Game\FrontendInputMap.cs`
