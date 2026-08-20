# Leftover #14 dest AABB analog — first dest Present

Investigation only. Production `src/` and `tests/` were
not edited. Do **not** invent dest fill. Do **not**
plant `512,384`. Do **not** re-enable `Key.N` /
`ActivateNewGame`.

Question: what dest AABB does native **write** on
first dest Present? Host dest analog leftover #36
is also open. Status of leftover #14 dest AABB.

Authority: `Fable.exe` listings
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`0041A850` / `0041AC20` / `0041AFA0` / `0041B173` /
`0041BEB0` / `0041C660` / `0042DF9E` / `0042E3EE`),
`listing-00500000.txt`
(`0052ECC0` / `0052EEC0` / `0052F270` / `0052F290` /
`0052F5C0` / `0052FFD0` / `00531090`),
`listing-00540000.txt`
(`0055B8F0` / `0055BF10` / `0055C0DE` / `0055CB10` /
`0054EF00` / `00595222`),
`listing-00b80000.txt` (`00BAD8A0` / `00BADB36` /
`00BAE2D0`);
`proofs/leftover-14-present-dest/README.md`;
`proofs/leftover-14-native-key/README.md`;
`proofs/leftover-36-dip-enqueue/README.md`;
`proofs/0041AC20-dest-formula/README.md`;
`proofs/0041B173-stack-dest/README.md`;
`proofs/leftover-48-native-hit/README.md`;
`proofs/type4-current-inner-apply/README.md`;
`src/Fable.Game/FrontendInputMap.cs`;
`src/Fable.Game/FrontendHitTest.cs`;
`src/Fable.Game/FrontendLayout.cs`;
`src/Fable.Game/FrontendDx9Submit.cs`;
`src/Fable.Game/EngineLifecycle.cs`;
`export/frontend/press-start-dests.txt`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Do not re-prove LMB type 4 / 6 → `0xE5` / `0x126` / 15,
Type4 current-inner `0055CB10`, or GraphicIndex leftover.
Do **not** invent dest 4-tuples (including `512,384`).

`FrontendInputMap.Leftover14OpenForDestPresentNotes = true`.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| Dest AABB 4-tuple native writes onto the widget on first dest Present? | **None.** Present `0042DF9E` → `00595222` `vtbl+8` never stores `DestX0,Y0,X1,Y1` on the widget | **DISPROVEN** as a widget store |
| Dest AABB `0055B8F0` on first dest Present? | **No.** `0055B8F0` is type 11/38 `vtbl+568` hover. Present walk is `vtbl+8` | **DISPROVEN** as a Present site |
| What dest **does** native write on first dest Present? | Type-0 stack `[esp+36..48]` after `0041B173` snap, copied into type-`0x22` rec `+12..+24` (`0041BEB0`). Type-6 has **no** dest AABB (pen `+248`) | **PROVEN** sites; **numbers UNREAD** |
| Native first-seen dest 4-tuple dump? | **None.** Same unread as leftover #36 dest-lock | **UNREAD** |
| Host dest `512,384` / forest `410` on widgets? | Host analog of the type-0 dest **formula**, not a native dump | **LEFTOVER** (#36) |
| Host Type4 apply via dest AABB `HitIndex`? | Native apply is current-inner `0055CB10`. Dest AABB is hover | **LEFTOVER** (#14) |
| MATCH dest AABB **writer** with listing sites that stores dest on widgets? | **None.** Do not edit `src/` | **DISPROVEN** |
| Close leftover #14 dest / Present Notes? | **No.** Dest AABB analog still host; Present `009DA9F0` still Note-only; dest numbers still unread | **LEFTOVER** open |
| Close leftover #36 dest-lock? | **No.** Sibling leftover. Do not fill dest | **LEFTOVER** open |

---

## Verdict

**Native writes no dest AABB onto widgets on first dest
Present. Leave leftover #14 dest AABB analog open.
Leave leftover #36 dest-lock open. Do not invent dest
fill.**

Two dest things are mixed in leftover #14. They are
not the same store:

| Object | Native site | What it writes | First dest Present |
| --- | --- | --- | --- |
| Hover dest AABB | `0055BF10` → `vtbl+568` `0055B8F0` | **`al`**. Take stores type-11/38 `+352` **u8** only (`0055C0DE`) | **not** on `0042DF9E` |
| Present dest | type-0 `vtbl+8` `0041AFA0` | stack `[esp+36..48]` then type-`0x22` rec `+12..+24` | **this** path |
| Widget dest origin | `0052FFD0` `+248/+252` | layout, **before** Present | already set; not AABB |
| Widget dest size leftover | `0041AC20` `+204/+208` | ctor GraphicIndex; **not** dest rect | already set |

`0055B8F0` **computes** a point-in-rect from dest
**origin** (`vtbl+488` `0052EEC0`) + dest **scale**
(`vtbl+492` `0052ECC0`) × `+176` extra (`vtbl+96`
`00531090`). It does **not** `fstp` a dest 4-tuple
onto the widget. `vtbl+492` is **not** `0041AFA0`
dest W/H (`leftover-48-native-hit`). Sibling
`leftover-14-present-dest` calling `vtbl+492` “dest
size” is **STALE** vs that listing.

First dest Present is Press Start
(`0042DF9E` / `00595222` slot `0x14`). Type-0
forest / title tiles with GraphicIndex ≠ 0 build
stack dest with area and pack `0041BEB0`. Type-6
`UI_PRESS_START_TEXT` never reaches `0041B173`.
Type-11 `UI_FRONTEND_BUTTON_INVISIBLE` Present is
child walk, not `0055B8F0`. Native **numbers** of
`[esp+36..48]` / rec `+12..+24` stay **UNREAD**.
Host widget dest tables (`512,384,512,384`, forest
`410`) are leftover **#36** analog.

Host leftover **#14** dest AABB analog: Type4 apply
still `HitIndex` dest AABB (`ArmType34Widgets`);
hover still `Contains \|\| HitIndex` on stored
widget dest (`TickType11Type38Hover`); Present skip
still `DestX1<=DestX0` on those stored dests. Native
Type4 apply is current-inner (`leftover-14-native-key`
LMB MATCH does not close this).

**Answer:** native dest AABB write on first dest
Present is **none**. Present dest is stack / type-`0x22`
rec, numbers unread. Hover dest AABB is a later tick
compute, not a dest fill. Host dest analog leftover
#36 also open.

---

## 1. Evidence — dest helpers (`listing-00400000.txt`)

None of these is a dest AABB store on the widget.

### `0041AC20` leftover, not dest AABB

```
0041AC50  mov [esi+360], eax      ; persist W
0041AC5E  mov [esi+364], eax      ; persist H
0041ACD8  cmp [esi+376], ebx
0041ACDE  jbe 0041AF6F            ; GraphicIndex 0 → skip leftover
0041AD19  fstp [esi+204]
0041AD69  fstp [esi+208]
```

No `+248`. No dest `X0,Y0,X1,Y1`.
`FrontendLayout.LeftoverAc20WritesDestRect = false`.

### `0041A850` dest **size getter** (`vtbl+92`)

```
0041A859  fld [ecx+204]
0041A863  mov eax, [ecx+360]
0041A86B  je  0041A87F            ; persist 0 → leftover * +92
0041A872  fild [esp]              ; else persist W
0041A87F  fmul [ecx+92]
… same H via +364 / +208 * +96
0041A8A0  fstp [eax]; fstp [eax+4]
```

Writes a 2-float **out-arg**. Not widget dest AABB.
Type-0 `vtbl+444` `0041C660` is `call [vtbl+92]`.

### `0041AFA0` Present dest — **stack only**

```
0041B065  mov eax, [edi+360]
0041B06D  jne 0041B077
0041B06F  fld [edi+204]           ; size W
0041B0AD  mov eax, [edi+248]      ; origin X bits
0041B0B5  fmul [edi+264]
0041B0DD  mov [esp+36], edx       ; dest X0
0041B0FD  mov [esp+40], eax       ; dest Y0
0041B10D  fstp [esp+44]           ; dest X1
0041B123  fstp [esp+48]           ; dest Y1
0041B127  call [edx+424]          ; centre? else 0041B173
0041B173  fld [esp+36]
0041B177  fistp [esp+12]          ; snap; not a widget store
          … same +40/+44/+48 …
0041B1AF  fstp [esp+48]
0041B4E6  call 0041BEB0
```

`FrontendLayout.SubmitDestStoresOnWidget = false`.
`NativeDestTupleUnread = true`.

### `0041BEB0` dest copy into type-`0x22` rec

```
0041BEBD  mov [eax], 0x22
0041BECF  mov esi, [ecx]          ; dest X0
0041BED1  mov [eax+12], esi
0041BED7  mov [eax+16], esi       ; dest Y0
0041BEDD  mov [eax+20], esi       ; dest X1
0041BEE3  mov [eax+24], ecx       ; dest Y1
```

This is the **only** dest 4-tuple **write** on first
dest Present. Target is the sprite record, not
widget dest fields, not hover AABB. First-seen
values **UNREAD** (`proofs/0041B173-stack-dest`).

Empty dest later `00BAD8A0` → `00BADB36 ret 8`
(`listing-00b80000.txt`). Nonempty dest
`00BAE2D0` DIPUP. No `009DB700`.

### Type-6 Present — no dest AABB

`0054EF00` `fld [esi+248]` pen. `fistp [esp+40]`
snaps the pen. `FrontendLayout.Type6DrawWritesDestRect
= false`. Host dest point `512,384,512,384` is **not**
this function.

---

## 2. Evidence — dest AABB is hover, not Present

`listing-00540000.txt` `0055B8F0`:

```
0055B8F0  sub esp, 32
          call [eax+488]          ; 0052EEC0 origin → [esp+12]
          call [edx+492]          ; 0052ECC0 scale → [esp+20]
          call [edx+96]           ; 00531090 extra → [esp+28]
          left  = fistp(origin.x + extra[0])
          top   = fistp(origin.y + extra[1])
          right = fistp(origin.x + scale.x * extra[2])
          bot   = fistp(origin.y + scale.y * extra[3])
          hit iff left <= x < right && top <= y < bot
          al = 1 / 0
          ret 4
```

No `fstp [esi+…]` dest. No widget dest 4-tuple.
`.text` `E8 0055B8F0` empty; dispatch is `vtbl+568`.

`0055BF10` (type 11/38 `vtbl+580`) calls that AABB
then, on take:

```
0055C0DE  mov [esi+352], 0x01     ; selected u8 only
```

That is the leftover **#14** hover gate Type4
**reads**. It is **not** a dest AABB write.

`vtbl+488` / `+492` / `+96` (`listing-00500000.txt`):

| Slot | VA | Writes dest AABB on widget? |
| ---: | --- | --- |
| `+488` | `0052EEC0` persist target `+60/+64` remap + parent | **no** — out-arg origin |
| `+492` | `0052ECC0` persist scale `+100/+104` remap + parent | **no** — out-arg scale, **not** dest W/H |
| `+96` | `00531090` `+176` child union, empty → `0,0,0,0` | **no** — out-arg extra |
| `+472` | `0052F270` copy `+248/+252` | **no** — out-arg; **unused** by `0055B8F0` |
| `+476` | `0052F290` copy `+264/+268` | **no** — unused by `0055B8F0` |

Empty `+176` → extra `0,0,0,0` → empty hit **even
when dest has area**. Point dest can still hit when
a child leftover / persist W fills extra (Accept
type-0 mouse area). Sibling `leftover-14-present-dest`
“point dest → empty AABB” is **PARTIAL**: empty
extra, not empty dest size, is the empty-hit cause.

`00595222` Present walk (`listing-00580000` in
`leftover-14-present-dest`): `[node+20].vtbl+8`
only. No dest AABB. No type filter. No `[ui+32]`.

---

## 3. Original — first dest Present vs dest AABB tick

```
0042EC7C
  0042E3EE  type 4 → 0055CB10(26)     current-inner; no dest AABB
            type 6 → 0055CB10(28)
  0042DC94 / 00599E3F
            [ui+84] vtbl+4 tick
            type 11/38: 0055ACB0 → 0055B890 → 0055BF10
            0055B8F0 dest AABB compute → +352 u8
  0042DF9E
            009D8CF0 / 009BEF20
            00595582 / 00595222 [ui+84] vtbl+8
              type 0:  0041AFA0 stack dest → 0041BEB0 rec +12..+24
              type 6:  0054EF00 pen +248; no dest AABB
              type 10/5/11/12: 00530260 child vtbl+8
            009DA9F0(1)×2 empty +16020 skip
            009BEF50 / 009BEEB0
```

First dest Present that has area is Press Start
type-0 tiles (GraphicIndex ≠ 0 → leftover `+204`
from bank frame). Dest AABB for Type4 is **not**
that Present dest. Press Start Type4 posts type-10
`+352` packet `0xE5` **without** dest AABB
(`leftover-14-native-key`). New Profile / Main Menu
Type4 arms only the current inner whose `+352` u8
is already 1; dest AABB ran on a **prior tick**.

Native dest **formula** (type-0 stack, Y-down):

```
w = (+360 != 0) ? (float)+360 : +204
h = (+364 != 0) ? (float)+364 : +208
w *= +264
h *= +268
dest = centre ? (ox±w/2, oy±h/2) : (ox, oy, ox+w, oy+h)
fistp/fild snap onto [esp+36..48]
```

Recovered (`0041AC20-dest-formula`). First-seen
**numbers** of that snap: **UNREAD**.

Native hover AABB formula (type 11/38 only):

```
left   = destOrigin.x + childMinAuthoredX
top    = destOrigin.y + childMinAuthoredY
right  = destOrigin.x + destScale.x * childMaxAuthoredX
bot    = destOrigin.y + destScale.y * childMaxAuthoredY
```

Not a dest writer. Extra numbers on first-seen
Press Start type-11 / later Accept: **UNREAD**.

---

## 4. Host dest AABB analog

Host **stores** dest 4-tuples on every widget
(`LayoutFrontendWidgets` / `ComputeSubmitDest`).
That store has **no** listing dest AABB writer.

First-seen host dest (1024×768 analog,
`export/frontend/press-start-dests.txt`):

| Widget | Type | Host dest | Native dest AABB write |
| --- | ---: | --- | --- |
| `UI_FRONTEND_PRESS_START_MENU` | 10 | `0,0,0,0` | none |
| `UI_FRONTEND_BG_FORREST_1_1` | 0 | `0,0,410,410` | stack / rec only; numbers **UNREAD** |
| `UI_TITLE_01` | 0 | `112,48,522,253` | same |
| `UI_PRESS_START_TEXT` | 6 | `512,384,512,384` | **none** — pen `+248` |
| `UI_FRONTEND_BUTTON_INVISIBLE` | 11 | `320,320,320,320` | Present none; hover AABB **UNREAD** extra |
| `UI_MOUSE_POINTER` | 32 | `0,0,32,32` | not `0055B8F0` |

`512,384` is `320*(1024/640), 240*(768/480)`.
Forest `410` is host snap of `256*1.6`. Analog of
the type-0 **formula**. **Not** a native dump.
Leftover **#36**.

Leftover **#14** dest AABB analog in apply / Present:

```
TickType11Type38Hover
  Hovered = Contains || HitIndex == i     ; stored dest AABB
ArmType34Widgets
  HitIndex dest AABB then Hovered         ; Type4 apply leftover
MessageFromType10Attach
  first visible type-10                   ; not current-inner
IssueRecoveredDraws
  skip DestX1<=DestX0                     ; stored dest AABB
FlushFrontendDisplay
  DisplayFlushShouldDip(0,0) → Note       ; leftover #36 enqueue
```

`FrontendHitTest.HitRect` prefers assigned `Hit*`
when it has area, else dest. `AssignHitRects` /
`TryChromeHit` invents type-16/37 hit
(`TryChromeHitIsNativeHit = false`) — leftover
**#48**, not a dest AABB writer.

`FrontendInputMap.NativeType4UsesDestAabb = false`.
`Leftover14OpenForDestPresentNotes = true`.

No MATCH dest AABB **writer** with listing sites
that stores dest on widgets. Do not plant dest
fill into `src/`.

---

## 5. Gap

```
Evidence              Original                         Host                          Gap
0042DF9E Present      00595222 vtbl+8; type-0          stored widget Dest* then      Host dest AABB on widgets
                      stack dest → 0x22 rec +12..+24   IssueRecoveredDraws skip      has no listing writer.
                      numbers UNREAD                   DestX1<=DestX0
0055B8F0 AABB         hover compute; al; no dest       HitRect / Contains dest       Host Type4 apply is dest
                      store; vtbl+492 is scale         AABB; ArmType34Widgets        AABB, not current-inner.
0041B173 [esp+36..48] snap on stack                    512,384 / 410 lattice         leftover #36 dest-lock
0041BEB0 rec +12..+24 dest copy; values UNREAD         host rec dest from widget     analog, not a dump
0054EF00 type-6       pen +248; no dest AABB           dest point 512,384,512,384    leftover #36 analog
009DA9F0(1)×2         empty +16020 skip                Note-only                     leftover #14 / #36 Present
LMB type 4/6          MATCH 0xE5 / 0x126 / 15          Queue Type4/Type6             not this leftover
```

| Claim | Class |
| --- | --- |
| Native dest AABB 4-tuple on widget at first dest Present | **DISPROVEN** |
| `0055B8F0` is a dest AABB **writer** | **DISPROVEN** — compute / `al` |
| `0055B8F0` runs on `0042DF9E` Present | **DISPROVEN** — `vtbl+8` walk |
| `vtbl+492` is `0041AFA0` dest W/H | **DISPROVEN** — persist scale `+100` |
| Type-0 Present dest is stack `[esp+36..48]` then rec `+12..+24` | **PROVEN** |
| Native first-seen dest 4-tuple numbers | **UNREAD** — leftover #36 dest-lock |
| Type-6 dest AABB writer | **DISPROVEN** |
| Host dest `512,384` / forest `410` | **LEFTOVER** analog (#36) |
| Host Type4 apply dest AABB `HitIndex` | **LEFTOVER** (#14) |
| Host hover `Hovered` stand-in for `+352` u8 | **LEFTOVER** (#14) |
| Native LMB type 4 / 6 posts `0xE5` / `0x126` / 15 | **MATCH** (`leftover-14-native-key`) |
| Type4 apply is current-inner `0055CB10` | **MATCH** (`type4-current-inner-apply`) |
| MATCH dest AABB writer to put in `src/` | **DISPROVEN** — none |
| Close leftover #14 dest / Present Notes | **DISPROVEN** — stays open |
| Close leftover #36 dest-lock | **DISPROVEN** — stays open |

**Overall: PARTIAL** (Present dest sites and hover AABB
formula recovered; dest **numbers UNREAD**; host dest
AABB analog **LEFTOVER**). **Leave #14 and #36 open.**

**Proposed (do not apply here):** keep LMB Type4/Type6.
Do not restore `Key.N`. Do not invent dest size /
`512,384`. Point Type4/Type6 at `0055CB10` current
inners; keep dest AABB on the **tick** that writes
`+352` u8 (`0055B8F0` origin + scale × extra, not
stored widget dest). Keep Present dest as stack /
type-`0x22` rec, not a widget dest AABB field. Keep
`0042DF9E` empty `009DA9F0` Notes. Dest-lock numbers
need a dump of `[esp+36..48]` / rec `+12..+24`, not
a dest fill.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00500000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00540000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00b80000.txt`
- `C:\FableCSharp\proofs\leftover-14-present-dest\README.md`
- `C:\FableCSharp\proofs\leftover-14-native-key\README.md`
- `C:\FableCSharp\proofs\leftover-36-dip-enqueue\README.md`
- `C:\FableCSharp\proofs\0041AC20-dest-formula\README.md`
- `C:\FableCSharp\proofs\0041B173-stack-dest\README.md`
- `C:\FableCSharp\proofs\leftover-48-native-hit\README.md`
- `C:\FableCSharp\proofs\type4-current-inner-apply\README.md`
- `C:\FableCSharp\src\Fable.Game\FrontendInputMap.cs`
- `C:\FableCSharp\src\Fable.Game\FrontendLayout.cs`
- `C:\FableCSharp\src\Fable.Game\FrontendHitTest.cs`
- `C:\FableCSharp\src\Fable.Game\FrontendDx9Submit.cs`
- `C:\FableCSharp\export\frontend\press-start-dests.txt`
