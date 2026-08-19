# `0052C730` after `005339B0` vs host `ApplyFirstSeenState`

Investigation only. No production `src/` edits.

Question: after `005339B0`, does `0052C730` write
`+324/+328/+332=0` on type 10/12? Does host
`FrontendWidgetFactory.ApplyFirstSeenState` **MATCH**?
Type 18 child 0?

Authority: `listing-00500000.txt` (`0052C730` / `005339B0` /
`0052CF40` / `00530260`); `listing-00540000.txt`
(`0054E4B0` / `0054D660` / `00547360` / `00547600` /
`00547380`); `FrontendWidgetFactory.ApplyFirstSeenState`;
`FrontendWidgetType.FirstSeenState` / `SelectsChild`;
`implementer/frontend/14-container.md`;
`proofs/list-type12-focus`, `proofs/type12-highlight-plus348`,
`proofs/particles-game`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Do not re-prove Press Start dest table, type-10 ctor
`0054E3D0`, or list highlight `+348`.

---

## Verdict

**Yes on type 10/12. Host first-seen visible set MATCHES.
Type 18 first-seen is persist child 0.**

`0052C730` **calls** `005339B0`, then zeros `+324/+328/+332`
(and more) on `this`. Type 10 `vtbl+172` and type 12 layout
both start with that call. `+332=0` is the style key, **not**
an exclusive child pick on type 10/12.

Host `ApplyFirstSeenState` sets `ActiveChild = 0` and only
exclusive-hides children of type **18**. Type 10/12 keep every
persist `+176` child. Type 18 keeps persist child **0**.

| Claim | Class |
| --- | --- |
| `0052C730` is `call 005339B0` then `+324/+328/+332=0` | **PROVEN** |
| Same write on type 10 (`0054E4B0`) and type 12 (`0054D660`) | **PROVEN** |
| Type 10/12 `+332=0` exclusive-hides siblings | **DISPROVEN** |
| Host does **not** hide type 10/12 kids (`SelectsChild` is 18 only) | **MATCH** |
| Type 18 `vtbl+172` `00547360` also calls `0052C730` → `+332=0` | **PROVEN** |
| Type 18 first-seen persist child 0 | **MATCH** |
| Host models `+332` as `ActiveChild`; no `+324/+328` slots | **PARTIAL** |
| Native also `+344/+336/+312/+308=0`, `+320=-1.0`, `+340=1` | **UNREAD** on host |
| Type 18 skip mechanism (`Visible=false` vs `+302` / draw) | **PARTIAL** |

---

## 1. Listing `0052C730`

`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00500000.txt`:

```
0052C730  push esi
          mov esi, ecx
          call 005339B0
          xor eax, eax
          mov [esi+324], eax
          mov [esi+328], eax
          mov [esi+344], eax
          mov [esi+332], eax
          mov [esi+320], 0xBF800000    ; -1.0f
          mov [esi+336], eax
          mov [esi+312], eax
          mov [esi+308], eax
          mov [esi+340], 0x01
          pop esi
          ret
```

“After `005339B0`” is **inside** this function, not a later
caller. Layout runs first; then first-seen state dwords.

`005339B0` writes dest / inherit scale (`+280==0` →
`+272/+276=1.0`, `+144..+147=0xFF`) and walks `+176`: if
`vtbl+208` parent is 0, `vtbl+204` set parent and
`vtbl+172` recurse. It does **not** write `+324/+328/+332`.

`FrontendWidgetType.FirstSeenState = 0` is that `+332`
store.

---

## 2. Type 10 / 12 call sites

Type 10 `vtbl+172` (`012497E4`):

```
0054E4B0  push esi / push edi
          mov esi, ecx
          call 0052C730
          ; then [+48] → +348, lookup "UI_ACCEPT"
```

Type 12 layout (immediately before dtor `0054DA70`):

```
0054D660  sub esp, 32
          …
          mov esi, ecx
          call 0052C730
          ; walk +356; [+352]=[+48]; +348 = 0 (ebp)
```

Both objects are large enough (`0x16C` / `0x1FC`) for
`+332`. First-seen menu and list therefore have
`+324=+328=+332=0`.

`0052CF40` (`vtbl+192`) later **replaces** `+332` with its
arg and forwards `vtbl+188` to own `+176` children. Type 8
children are skipped only when parent `+332` is **1 / 3 / 4**.
First-seen `0` does not take that skip. Type 12 highlight is
`+348` into `+356`, not `+332` (`type12-highlight-plus348`).

Type 5/10/12 do not exclusive-select (`14-container.md`,
`SelectsChild`).

---

## 3. Type 18 child 0

Type 18 ctor `00547600`: type 5 `0052CC50`, vtbl
`012485AC`, zeros `+348/+352/+356`, `00547500` fills the
`+348` state list from persist `States` (`def+480/+492`).

Type 18 `vtbl+172`:

```
00547360  mov esi, ecx
          call 0052C730          ; +324/+328/+332 = 0
          mov [esi+360], [esi+48]
          mov [esi+364], 0xD
          ret
```

First tick `00547380`: `0052C7E0`, then if
`+324 == +328` (both 0) and duration not elapsed, **no**
`vtbl+192`. First-seen state stays **0**.

Persist order: state 0 is child 0
(`UI_SWAPPING_FORREST` → `BLENDING_BG_FORREST_1`;
sunbeam → `…_SUNBEAM_1`; `UI_PRESS_START_SWAP` →
`UI_PRESS_START_TEXT`).

---

## 4. Host `ApplyFirstSeenState`

`FrontendWidgetFactory.Build` attaches persist `Children`,
then:

1. Every widget: `Visible=true`, `Enabled=true`,
   `Clip=false`, `ActiveChild = FirstSeenState` (0).
2. `SelectsChild` (type **18** only): hide kids with
   index `!= 0`.
3. Inherit `Visible` / `Enabled` / `Clip` down the tree
   (hidden swap sibling hides descendants).

`SelectsChild(10)` / `SelectsChild(12)` / `SelectsChild(5)`
are false. Press Start list, TITLE pair, LEGAL, MOUSE stay
visible. Factory test asserts forest/sunbeam child 0 on,
siblings off, `UI_SWAPPING_FORREST.ActiveChild == 0`.

Draw `DrawContainerWalk` skips `!Visible || Clip`. That is
the host analog of “one swap child presented.”

---

## 5. MATCH vs leftover

| Site | Native | Host | Class |
| --- | --- | --- | --- |
| First-seen `+332` | `0052C730` writes 0 | `ActiveChild = 0` | **MATCH** |
| `+324/+328` | 0 (style / elapsed pair) | no slots | **LEFTOVER** (unhit first-seen) |
| Type 10/12 siblings | stay in `+176`; draw all | not hidden | **MATCH** |
| Type 18 child 0 | `+332=0` → persist `[0]` | hide `k != 0` | **MATCH** visible set |
| Type 18 hide how | draw skip / style; `+302` **UNREAD** | `Visible=false` at factory | **PARTIAL** |
| Extra `0052C730` stores | `+320=-1`, `+340=1`, zeros `+344/+336/+312/+308` | unused | **UNREAD** |
| Order | attach `vtbl+172` = layout then zeros, recurse | factory state, later `FrontendLayout.Compute` | **PARTIAL** split; first-seen set still **MATCH** |

`ApplyFirstSeenState` is **not** a 1:1 lift of the
`0052C730` stores. It is the first-seen **visible set**
those stores plus type-18 select produce.

Do **not** treat type 10/12 `ActiveChild=0` as “selected
list row.” That row is `+348`.

---

## 6. UNREAD

- Whether first-seen type 18 `vtbl+192` ever runs on attach
  (first tick of `00547380` does **not**).
- Whether non-zero swap siblings are skipped by `vtbl+420`
  (`+302` bit 0) or only by a later style write.
- Host never writes `+320` / `+340`; first-seen menus do
  not need them for dest or the Press Start present set.
