# Leftover #46 — skip `vtbl+400` / `+420` vs first-seen Press Start / New Profile

Investigation only. Production `src/` and `tests/` were not edited
by this proof.

Question: leftover #46 remaining is skip VAs `0052F180` /
`0052F1D0` as actual method calls vs field checks. Host
`DrawContainerWalk` already walks every child; skip is
`Visible` / `Clip`. Exclusive `ActiveChild` walk is
**DISPROVEN**. Is leftover #46 **STALE** (walk **MATCH**)
or still **DIVERGE** because skip bits are **UNREAD** on
first-seen widgets? Recover first-seen `+300` / `+302` on
Press Start / New Profile.

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00500000.txt`
(`00530260`–`005303E0` / `0052F180` / `0052F1D0` /
`005331A0` `0053324C` `00533288`);
`implementer/frontend/fn-005331A0-exact.txt`;
inflated `frontend.bin` UI dumps
`assembly/compiled-defs/frontend/`
(`0620-UI_FRONTEND_PRESS_START_MENU.md`,
`0201-UI_FRONTEND_NEW_PROFILE_SCREEN.md`,
forest / WASD / ARROWS / PRESS_START_TEXT / RING);
`src/Fable.Formats/Defs/FrontendUiDef.cs`
(`Plus392Crc` `0x8A69D67E` / `Plus504Crc` `0x2CB06C8E`);
`src/Fable.Formats/Defs/FrontendWidgetType.cs`
(`BorrowedVisibleFn` / `ClipBitFn` /
`ContainerDrawWalksEveryChild` /
`ExclusiveWalkSelectsChild`);
`src/Fable.Game/FrontendWidgetFactory.cs`
(`Add` `Clip` / `IsPresented` /
`ApplyFirstSeenState`);
`src/Fable.Game/EngineLifecycle.cs`
(`DrawContainerWalk`);
`tests/Fable.Formats.Tests/FrontendUiDefTests.cs`
(`Factory_builds_press_start_then_main_menu_from_the_same_walk`);
`proofs/type16-18-present-child`,
`proofs/leftover-46-status`,
`proofs/vtbl400-first-seen-hide`,
`docs/status/README.md` leftover #46.

Do not re-prove native `00530260` exclusive-index (already
**DISPROVEN**). Do not invent `DrawContainerWalk` calls of
`0052F180` / `0052F1D0`. Do not invent dest pixels or DIP
counts. Do not add type 38 to `SelectsChild`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH** / **STALE**.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| Native `00530260` exclusive-walks `kids[ActiveChild]`? | **No.** Every `+176` then `+188`. | **DISPROVEN** |
| Host `DrawContainerWalk` / `IsPresented` exclusive-walk `ActiveChild`? | **No.** `foreach` / parent chain. Dropped in `88a9ab8`. | **STALE** leftover |
| First-seen walk every persist child, not sibling-index hide? | Host and native. | **MATCH** |
| Skip bits **UNREAD** on first-seen Press Start / New Profile? | **No.** Persist `def+392` / `def+504` are in the inflated blobs and parse as **0**. | **DISPROVEN** as unread |
| First-seen `+302` bit 0 (`vtbl+420` `0052F1D0`)? | **0** (no `or [+302],1`). | **PROVEN** |
| First-seen `+300` bit 7 (`vtbl+400` `0052F180`)? | **0** (no `or [+300],0x80`). | **PROVEN** |
| Host `Clip` / `Visible` first-seen present set vs those bits? | `Clip=false`, `Visible=true`; native own-child skip is clip only, clip is 0. Same kids presented. | **MATCH** |
| Leftover #46 still **DIVERGE** because skip bits unread? | **No.** | **STALE** as DIVERGE |
| Host `DrawContainerWalk` calls `0052F180` / `0052F1D0`? | **No.** Field `Visible` / `Clip`. | **UNREAD** as calls; not a first-seen present-set leftover |

---

## Verdict

**Leftover #46 is STALE (walk MATCH).** It is not still
**DIVERGE** because skip bits are unread on first-seen
widgets.

Exclusive `ActiveChild` walk is **DISPROVEN** in native
and **STALE** in host (`proofs/leftover-46-status`).
`DrawContainerWalk` / `IsPresented` walk every child.
Skip is host `Visible` / `Clip`, not a sibling pick.

First-seen Press Start / New Profile persist skip bytes
are **recovered, not UNREAD**:

| Persist | Widget | First-seen | Native getter |
| --- | --- | ---: | --- |
| `def+392` CRC `0x8A69D67E` | `+302` bit 0 | **0** | `vtbl+420` `0052F1D0` |
| `def+504` CRC `0x2CB06C8E` | `+300` bit 7 | **0** | `vtbl+400` `0052F180` |

Ctor `005331A0` only ORs those bits when the persist u8
is nonzero. First-seen `0052F1D0` returns 0 and
`0052F180` returns 0. After `00531EC0` parent bind,
own `+176` kids skip **only** clip, so they are **not**
skipped. Host factory `Clip` is `Plus392 != 0` (**false**)
and `Visible` is **true**. Same first-seen child set.

Skip VAs as **method calls** from `DrawContainerWalk`
stay named constants. That is **not** a first-seen
present-set **DIVERGE**. Do not invent those calls. Do
not leave #46 open as “skip bits unread on Press Start /
New Profile.”

| Claim | Status |
| --- | --- |
| `00530260` walks every `+176` then `+188` | **PROVEN** |
| Exclusive `ActiveChild` walk | **DISPROVEN** |
| Host exclusive-walk leftover | **STALE** (`88a9ab8`) |
| First-seen walk every persist child | **MATCH** |
| `0052F180` = `[+300]>>7`; persist `def+504` | **PROVEN** |
| `0052F1D0` = `[+302]&1`; persist `def+392` CRC `0x8A69D67E` | **PROVEN** |
| Press Start / New Profile first-seen `+392` / `+504` = 0 | **PROVEN** (hex + parse) |
| First-seen `+300` bit 7 / `+302` bit 0 = 0 | **PROVEN** |
| Skip bits UNREAD on those screens | **DISPROVEN** |
| Host `Clip` = `Plus392 != 0` | **PROVEN** field stand-in for clip |
| Host `Visible` = persist `+504` | **DISPROVEN** (factory `true`; `Plus504` not stored) |
| First-seen present set vs native own-child clip | **MATCH** |
| `DrawContainerWalk` calls `0052F180` / `0052F1D0` | **UNREAD** as calls |
| Leftover #46 as unread-bit **DIVERGE** | **STALE** |
| Type-18 inactive forest tiles presented | **MATCH** native (not a skip leftover) |
| Lionhead names of `+392` / `+504` | **UNREAD** |

---

## Evidence → Original → Host → Gap

### Evidence

`listing-00500000.txt` `00530260` `vtbl+8` (types 5 / 10 /
12 / 16 / 18; later also 11 / 38). Header `vtbl+404` /
`vtbl+416` (layer), then every `+176` child, then every
`+188` child. Per child:

```
005302C0  call [edx+208]           ; parent = [child+200]
005302C6  cmp esi, eax
005302C8  je  005302DF             ; own child: skip +400
005302D5  call [edx+400]
005302DB  test al, al
005302DD  je  00530324             ; skip if false
005302EA  call [edx+420]
005302F0  test al, al
005302F2  jne 00530324             ; skip if true
005302FF  call [edx+420]           ; same slot again
00530305  test al, al
00530307  jne 00530324
00530321  call [edx+8]             ; draw
```

No `ActiveChild`. No `+332`. No `+348`. Same four tests
on the `+188` walk (`00530355`–`005303C4`). First-seen
`+188` is ctor 0 (`005334A0`).

Getters (`listing-00500000.txt`):

```
0052F180  movzx eax, [ecx+300]
          shr eax, 7
          ret                      ; bit 7

0052F1D0  xor eax, eax
          mov al, [ecx+302]
          and eax, 1
          ret                      ; bit 0
```

Ctor persist (`fn-005331A0-exact.txt`):

```
0053323D  mov [ebx+300], [def+60] & 0x1F
0053324C  mov al, [ecx+504]
          test al, al
          je  0053325E
          or  [ebx+300], 0x80      ; bit 7 → 0052F180
0053327F  cmp [ecx+392], 0
          je  0053328F
00533288  or  [ebx+302], 0x01      ; bit 0 → 0052F1D0
```

File CRCs (`FrontendUiDef`): `Plus392Crc` `0x8A69D67E`
(`00632065` / `00533288`), `Plus504Crc` `0x2CB06C8E`
(`00632161` / `0053324C`). Names **UNREAD**.

Inflated hex (CRC then u8). Press Start root and New
Profile root share the same tail bytes:

```
CC 4C 3E 7E D6 69 8A 00     ; +392 CRC 0x8A69D67E  u8 = 0
...
8E 6C B0 2C 00              ; +504 CRC 0x2CB06C8E  u8 = 0
```

(`0620-UI_FRONTEND_PRESS_START_MENU.md` `@05B0` / `@0630`;
`0201-UI_FRONTEND_NEW_PROFILE_SCREEN.md` same offsets.)

Control that the CRC can be 1 (not on these screens):
`UI_RING_PIC_DRAW_FROM_VIEWPORT` `@05A0`
`7E D6 69 8A 01`.

Whole `frontend.bin` dump: `plus392 **1**` on three
widgets only (`UI_TEXT_WEAPONS_DESCRIPTION_TEMPLATE_NEW`,
`UI_RING_PIC_DRAW_FROM_VIEWPORT`,
`UI_RING_PIC_SAVE_VIEWPORT`). `plus504 **[1-9]` hits
**zero** files.

### Original (first-seen)

`005334A0` zeros `+300/+302/+303` then `005331A0`.
Press Start / New Profile persist u8s are 0, so the ORs
do not fire.

| Widget byte | First-seen | `0052F180` / `0052F1D0` |
| --- | ---: | ---: |
| `+300` bit 7 | 0 | 0 |
| `+302` bit 0 | 0 | 0 |

Same-frame tick `00599E3F` → `0052C7E0` → `00531EC0`
sets parent `+200` on every slot’s `+176` kids
(`proofs/00595222-visible-skip`). Own children never
take the `vtbl+400` arm. Clip is 0, so `vtbl+420` does
not skip. Native **presents every persist child**,
including type-18 `BLENDING_BG_FORREST_2` tiles and
type-16 `WASD` / `INVERTED`.

`0052C730` / type-18 `00547360` / type-16 `00549230`
do **not** write `+300/+302` skip bits
(`proofs/vtbl400-first-seen-hide`). No first-seen
`or [+302],1` on sibling index `k != 0`.

### Host

`FrontendWidgetType.ContainerDrawWalksEveryChild = true`.
`ExclusiveWalkSelectsChild = false`.

`DrawContainerWalk` (`EngineLifecycle.cs:4474`):

```4474:4488:src/Fable.Game/EngineLifecycle.cs
    private void DrawContainerWalk(
        IReadOnlyList<FrontendWidget> tree, int index, ref int drawn)
    {
        if ((uint)index >= (uint)tree.Count)
            return;
        var widget = tree[index];
        if (!widget.Visible || widget.Clip)
            return;
        drawn++;
        if (FrontendWidgetType.DrawsChildList(widget.Type))
        {
            var kids = FrontendWidgetFactory.ChildrenOf(tree, index);
            foreach (var child in kids)
                DrawContainerWalk(tree, child, ref drawn);
```

`IsPresented` (`FrontendWidgetFactory.cs:112`) is the
same `Visible` / `Clip` parent chain. No `ActiveChild`.

Factory `Add` (`FrontendWidgetFactory.cs:283`):
`Visible: true`, `Clip: def is { Plus392: not 0 }`,
`Flag302` bit 0 from `Plus392`. `Plus504` is parsed on
the def and **not** stored on the widget.

`ApplyFirstSeenState` forces `Visible = true` and does
**not** `Visible=false` on `k != 0`. Type 16 writes
style 3 on persist child 0 only. Tests lock
`BLENDING_BG_FORREST_2` / `WASD` / `INVERTED` `Visible`
and `forest*.Clip == false`.

`BorrowedVisibleFn` `0x0052F180` and `ClipBitFn`
`0x0052F1D0` are constants + test equals. Neither
walk calls them.

### Gap

| Native | Host HEAD | First-seen Press Start / New Profile |
| --- | --- | --- |
| Walk every `+176` | `foreach` `ChildrenOf` | **MATCH** |
| Skip `parent!=this && !vtbl+400` | `!Visible` (factory `true`) | Own kids: native does not consult `+400`. **MATCH** present set. Mechanism **LEFTOVER** (`Visible` is not bit 7). |
| Skip `vtbl+420` (`+302` bit 0) | `Clip` from `Plus392 != 0` | Both 0. **MATCH** |
| Call `[edx+400]` / `[edx+420]` | named constants | **UNREAD** as calls. Do **not** invent. Outcome **MATCH**. |
| `def+504` → bit 7 | parsed, unused | File **0** on every dumped UI, including these screens. Applying it would not hide first-seen kids. |

docs/status leftover #46 (“native skip vtbl+400 (+504)
still unused; type-18 inactive siblings present”) mixes
two facts. Unused **method calls** are true and are not
a first-seen **DIVERGE**. Inactive siblings present is
**MATCH** native (`00530260` never exclusive-walked).
Skip bits on these screens are **0**, not **UNREAD**.

---

## 1. First-seen `+300` / `+302` table

Ctor zero then persist OR. File u8 **0** ⇒ bits stay 0.

### Press Start (`UI_FRONTEND_PRESS_START_MENU` type 10)

Children persist order: `UI_BLENDING_BACKGROUNDS_FORREST`,
`UI_TITLE`, `UI_PRESS_START_SWAP`,
`UI_FRONTEND_LIST_PRESS_START_MENU`, `UI_LEGAL_TEXT`,
`UI_MOUSE_POINTER`.

| Widget | Type | `+392` | `+504` | hex |
| --- | ---: | ---: | ---: | --- |
| `UI_FRONTEND_PRESS_START_MENU` | 10 | **0** | **0** | `7E D6 69 8A 00` / `8E 6C B0 2C 00` |
| `UI_TITLE` / `UI_TITLE_01` / `UI_TITLE_02` | 5 / 0 / 0 | **0** | **0** | TITLE_01 `@02A0` / `@0320` |
| `UI_PRESS_START_TEXT` | 6 | **0** | **0** | `@0340` `7E D6 69 8A 00`; `@03C0` `8E 6C B0 2C 00` (`+508` i32 **1**) |
| `UI_PRESS_START_SWAP` | 18 | **0** | **0** | `@0310` / `@03A0` |
| `UI_SWAPPING_FORREST` | 18 | **0** | **0** | compiled dump |
| `BLENDING_BG_FORREST_1` | 5 | **0** | **0** | `0636-…` |
| `BLENDING_BG_FORREST_2` | 5 | **0** | **0** | `@0400` / `@0480` |
| `UI_FRONTEND_BG_FORREST_1_1` | 0 | **0** | **0** | compiled dump |
| `UI_FRONTEND_BG_FORREST_2_1` | 0 | **0** | **0** | `@02A0` / `@0320` |
| `UI_LEGAL_TEXT` | 6 | **0** | **0** | compiled dump (`+508` **1**) |
| `UI_MOUSE_POINTER` | 32 | **0** | **0** | compiled dump |
| `UI_FRONTEND_LIST_PRESS_START_MENU` | 12 | **0** | **0** | `@05A0` / `@0620` |

Forest / sunbeam tiles `*_1_*` through `*_4_*` /
`SUNBEAM_*` in that dump are all `plus392 **0**
plus504 **0**`.

### New Profile (`UI_FRONTEND_NEW_PROFILE_SCREEN` type 10)

Children persist order: `UI_TEXT_NEW_PROFILE_MENU_TITLE`,
`UI_BLENDING_BACKGROUNDS_COASTAL`, `UI_TABLE_TITLE_WHOLE`,
`UI_NEW_PROFILE_MENU`, `UI_HELPERS_NEW_PROFILE`.

| Widget | Type | `+392` | `+504` | hex |
| --- | ---: | ---: | ---: | --- |
| `UI_FRONTEND_NEW_PROFILE_SCREEN` | 10 | **0** | **0** | same tail as Press Start root |
| `UI_TEXT_NEW_PROFILE_MENU_TITLE` | 6 | **0** | **0** | compiled dump |
| `UI_BLENDING_BACKGROUNDS_COASTAL` | 5 | **0** | **0** | compiled dump |
| Coastal tiles / sunbeams | 0 / 5 | **0** | **0** | compiled dump |
| `UI_NEW_PROFILE_MENU` | 12 | **0** | **0** | compiled dump |
| `UI_OPTIONS_CONTROL_METHOD_TEXT_SLIDER` | 16 | **0** | **0** | compiled dump |
| `UI_OPTIONS_TEXT_CONTROL_ARROWS` | 6 | **0** | **0** | compiled dump (`+508` **1**) |
| `UI_OPTIONS_TEXT_CONTROL_WASD` | 6 | **0** | **0** | `@0420` `7E D6 69 8A 00`; `@0490` `8E 6C B0 2C 00` (`+508` i32 **1**) |
| `UI_TEXT_NORMAL` / `UI_TEXT_INVERTED` | 6 | **0** | **0** | INVERTED `@0410` / `@0490` |
| `UI_ACCEPT_NEW_PROFILE` / `UI_SPRITE_ACCEPT_ON` / `OFF` | 38 / 0 | **0** | **0** | compiled dump |
| `UI_CANCEL` | | **0** | **0** | compiled dump |
| `UI_NEW_PROFILE_EDIT_BOX` | | **0** | **0** | compiled dump |

Host first-seen: `Clip=false` on those widgets
(tests lock forest Clip). `Visible=true` including
inactive type-18 / type-16 siblings. Native clip 0 and
own-child `+400` unused ⇒ same set.

`+508` **1** on type-6 text is align (`0054ED90`
`or [+302],0x10`), **not** clip bit 0
(`proofs/type6-def508-align`). **DISPROVEN** as skip.

---

## 2. Method call vs field check

Native `00530260` **calls** `[child.vtbl+400]` and
`[child.vtbl+420]` twice. Bodies that match the skip
bits are `0052F180` / `0052F1D0`. Type 5 / 0 / 6
`.rdata` dwords at `+400/+420` remain **UNREAD** as
table slots (`proofs/vtbl400-first-seen-hide`); the
**bodies** are **PROVEN**.

Host skip is `if (!Visible || Clip)`.

| Native call | Host field | First-seen value | Present-set |
| --- | --- | --- | --- |
| `vtbl+420` `0052F1D0` | `Clip` from `Plus392` | 0 / false | **MATCH** |
| `vtbl+400` `0052F180` | **not** `Plus504`; `Visible=true` | bit 7 = 0 | Own children **MATCH** (arm unused). Borrowed `parent!=this` path unused on first-seen persist kids. |

Wiring those VAs as C# calls is **not shown** by the
listing. Field clip is an equivalent predicate for the
**PROVEN** getter body `[+302]&1`. Do not invent the
calls from leftover #46.

`Plus504` parsed and unused is **UNREAD** in the walk.
On this file it is **0** for every dumped UI def, so
first-seen present set does not **DIVERGE** for lack
of that apply.

---

## 3. Nearby leftovers that are **not** #46

- `ApplyTextSliderHit` later `Visible = k == next` is
  an exclusive **hide after input**, not first-seen
  walk (`proofs/leftover-46-status` §5).
- Style-6 alpha / dest hide of leftover **slots** after
  `00596763` is **UNREAD** and is **not** `+302`
  (`proofs/listing-present-skip`).
- Dest invented 512,384,512,384 is leftover #36.
- New Profile dest/hit is leftover #48.

---

## Do not

- Leave leftover #46 open as “skip bits unread on
  first-seen Press Start / New Profile.” Those u8s are
  **0**. Walk **MATCH**.
- Leave leftover #46 open as “host still exclusive-walks
  `kids[ActiveChild]`.” That walk is gone.
- Treat type-18 inactive siblings presented as a skip
  **DIVERGE**. Native presents them.
- Invent `DrawContainerWalk` / `IsPresented` calls of
  `0052F180` / `0052F1D0`.
- Treat skip VAs as recovered because they exist as
  constants.
- Map host `Visible` onto `+300` bit 7.
- Claim Lionhead names for `+392` / `+504`.
- File `+508` centre as clip.
- Re-file native exclusive-walk (**DISPROVEN**).
