# Audit: remaining frontend C# vs dump leftovers

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `0041AFA0` / `0041AC20` / `0054EF00` /
`0054ED90` / `0054FFF0` / `005331A0` / `00530260` / `0042E3EE` /
`0054E280` / `0059A238`; `frontend.bin` + `names.bin`;
`src/Fable.Game/EngineLifecycle.cs` (`CollectFrontendRecords`,
`LayoutFrontendWidgets`, `DrawFrontendWidgets`,
`MaybeActivateNewGameFromInput`);
`src/Fable.Game/FrontendLayout.cs`;
`src/Fable.Game/FrontendWidgetFactory.cs`;
`src/Fable.Game/FrontendInputMap.cs`;
`src/Fable.Game/FrontendTextDraw.cs`;
`src/Fable.Client/Program.cs`;
`implementer/frontend/02-layout.md`, `05-input.md`,
`11-transform.md`, `16-resolution.md`;
`proofs/glyph-uv-gaps/README.md`;
`proofs/type4-input-lifecycle/README.md`;
`tests/Fable.Formats.Tests/FrontendLayoutTests.cs`,
`FrontendInputTests.cs`, `FrontendUiDefTests.cs`,
`EngineLifecycleTests.cs`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH** / **STALE**.

Do not re-prove first-seen PRESS_START dest table
(`0,0,0,0` root; forest tiles 410; TITLE `112,48`; TEXT `512,384`),
type-10 ctor `0054E3D0`, type-6 dest as a point, Return≠`0xE5`.

---

## Verdict

Three leftovers remain. Dest math is already generic.

| Item | Status | Native | C# leftover |
| --- | --- | --- | --- |
| `CollectFrontendRecords` type-6 leftover `+204` | **LEFTOVER** (first-seen **MATCH** for left) | `0054EF00` `fmul [esi+204]` for centre/right only. First-seen GraphicIndex 0 → `0041AC20` skip → `+204=0`. | `leftoverW = DestX1-DestX0` (point → 0). Scale hard `1f`. |
| Type-6 align | **PARTIAL** | `0054ED90` `or [+302],0x10/0x20` from **`[def+508]`**. `0054FFF0` bits 4/5. | Always `FrontendTextDraw.AlignLeft`. `FrontendUiDef` does not parse `+508`. |
| Type-6 face | **PARTIAL** | `0054ED90` → `009D49B0` names offset → `009E2C80`. PRESS_START_TEXT `Font=26051` → `ENG_ARIAL_24`. `0054F4B0` `"ENG_ARIAL_16"` is a **different** helper. | Factory stores `FontFace`. Submit still `?? FrontendUiFontFace` (`ENG_ARIAL_16`) then `TryLoad(FrontendUiFontFace)`. `DumpFrontendFrame` always loads `ENG_ARIAL_16`. |
| Font leftover as `0041AFA0` dest | **MATCH** | Type-6 GraphicIndex 0; dest origin only. Glyphs are `0054EF00` `0x27`. | `LayoutFrontendWidgets` leftover only if `GraphicId!=0`. TEXT dest is a point; glyphs emit from pen. |
| Dest layout forks by screen name | **DISPROVEN** | Same `005339B0` / `00531EC0` / `0052F5C0` / `0052FFD0` / `0041AFA0` for every widget. | `FrontendLayout.Compute` has **no** screen-specific numbers. `FrontendWidgetFactory.Build` is the same walk for PRESS_START / NEW_PROFILE / MAIN_MENU. |
| Named-screen attach / draw / input forks | **LEFTOVER** | Slot names are native (`0x14` / `0x17` / `0059899A`). Draw is type (`00530260` on 5/10/12/18). Input posts **widget+352**, not a C# screen string. | See §2. |
| Return (DIK 28) → frontend message | **DISPROVEN** | Type 1 / action 33. `00597BF2(1)`, not `0xE5`. | `TryMapEvent(TypeKey, 28, *)` is null. Host still queues Enter as type 1. |
| Type-4 → action 26 → `0xE5` on Press Start | **PROVEN** | `0042E3EE` type 4; `0054E280` action 26 posts `widget+352`. | `MessageFromAction` only if `screen == PressStartMenu`. Other screens: null (no stored-id analog). |
| Physical type-4 DIK; `0x126` / 15 posters | **UNREAD** | No `.text` `mov […], 0x126`. | Tests / `FrontendInputMap.Queue` inject ids. |

---

## 1. `CollectFrontendRecords` leftover vs dump

Site: `EngineLifecycle.cs` `CollectFrontendRecords` (called from
`CompositeFrontendPresent` after `DrawFrontendWidgets`).

### 1.1 Sprite leftover (`0041AC20` / `0041AFA0`) — MATCH

`LayoutFrontendWidgets` (not Collect) writes dest:

```
leftover only if GraphicId != 0
  leftoverW/H = FrameWidth/Height (else texture W/H)
0041AFA0: size = persist W/H else leftover; * destScale from origin
```

Dump (`11-transform.md` / `FrontendLayoutTests.Press_Start_first_seen_dest_table_matches_0041AFA0`):

- GraphicIndex 0 → leftover 0 → dest is a point (TEXT, TITLE group, root).
- GraphicIndex ≠ 0 → frame leftover (TITLE_01 256×128, forest 256/128, mouse 32).

**Do not** feed font measure into `0041AFA0`. That host leftover is gone.

`CollectFrontendRecords` then skips sprite DIP when
`DestX1 <= DestX0` (zero-size). Native `00BAD8A0` also skips empty dest.
Type-6 still emits glyphs from the origin. Frame dump `zero-size` is dest-rect only.

### 1.2 Type-6 leftover `+204` — LEFTOVER math, first-seen MATCH

`0054EF00` (centre/right only):

```
fld scale          ; +264 if +392!=0 else +124
fmul [esi+204]
; centre: fmul [0x122F59C]=0.5
fsubr originX
```

C#:

```
leftoverW = max(0, DestX1 - DestX0)   // 0041AFA0 dest width, not +204
Type6Pen(origin, leftoverW, scale=1f, AlignLeft)
```

| First-seen type-6 | Native `+204` | C# leftoverW | Align | Result |
| --- | --- | --- | --- | --- |
| `UI_PRESS_START_TEXT` dest `512,384`–`512,384` | 0 | 0 | left | **MATCH** pen = origin+2 |
| Centre/right live | UNREAD writer | dest width (0 on GraphicIndex 0) | ignored (hard left) | would **DIVERGE** if `def+508≠0` |

Writers of `+204` recovered elsewhere (`0041AC20` when `+376≠0`;
`0054E640`; PlayAVI `00547D52`). `0054F5C0` does **not** call
`00AB7B00`. Do not invent measure 301 as `+204`.

Scale hard `1f` is leftover vs dest `+264` (root remap 1.6). Left
align ignores leftover×scale. Centre/right would need both.

### 1.3 AlignLeft hardcode — PARTIAL

`0054ED90`:

```
eax = [def+508]
0 → or [widget+302], 0x08
1 → or [widget+302], 0x10   ; 0054FFF0 centre
2 → or [widget+302], 0x20   ; right
```

`005331A0` does **not** write bits 4/5. Parser never reads `+508`
(`FrontendUiDef` has `+504` CRC `0x2CB06C8E` as persist u8, not
the type-6 ctor dword). `FrontendTextDraw.AlignFromFlag302` exists
and is **unused** by Collect.

First-seen PRESS_START_TEXT is left **if** `def+508==0` (unverified
in C#). Classification: C# AlignLeft is a first-seen **assumption**.

### 1.4 Font face fallback — PARTIAL (glyph-uv-gaps §1 is STALE)

`proofs/glyph-uv-gaps` said factory drops `def.Font` and Collect
always loads `ENG_ARIAL_16`. That is **STALE**:

- `FrontendWidget.Font` / `FontFace` exist.
- `FrontendWidgetFactory.ResolveFontFace` → `NamesBin.Get((uint)font)`.
- Test: `Font=26051` → `ENG_ARIAL_24` (`FontFile.PersistType6Face`).

Live leftover:

1. Collect: `faceName = widget.FontFace ?? FrontendUiFontFace` then
   `TryLoad(faceName) ?? TryLoad(FrontendUiFontFace)` (`ENG_ARIAL_16`).
   Null face (no names.bin) still draws 16.
2. `DumpFrontendFrame` always `TryLoad(FrontendUiFontFace)` for glyph
   flags — 16 even when the widget is 24.
3. `EngineLifecycle.FrontendUiFontFace = FontFile.UiFace` still names
   the `0054F4B0` helper, not the type-6 persist mapper.

With install + names.bin, PRESS_START_TEXT should submit **24**.
The 16 constant is leftover fallback / dump / docs, not the factory.

### 1.5 Duplicate dest helper — LEFTOVER

`EngineLifecycle.FrontendWidgetDest` still uses
`RegionTravel.PlayAviLetterboxHalf` and does **not** snap.
Live layout uses `FrontendLayout.ComputeSubmitDest` (snap via
`Round`). Only tests call the old helper.

### 1.6 Proposed (do not apply here)

1. Keep sprite leftover = GraphicIndex frame; type-6 dest a point.
2. Type-6 leftover204 = widget `+204` analog (0 first-seen), not dest W.
3. Pass dest scale `+264` into `Type6Pen`, not `1f`.
4. Parse `def+508` (or store `+302` bits 4/5). Use `AlignFromFlag302`.
5. Load `widget.FontFace` only; do not fall back to `ENG_ARIAL_16`
   on a resolved type-6. Dump should use the same face.
6. Delete or retarget `FrontendWidgetDest` to `FrontendLayout`.

---

## 2. Screen-name hardcodes: PRESS_START / NEW_PROFILE / MAIN_MENU

`FrontendLayout` comment: “No screen-specific numbers.” That holds
for dest. Remaining forks are **attach / draw notes / input**, not
640-space dest tables.

Factory test `Factory_builds_press_start_then_main_menu_from_the_same_walk`:
all three roots are **type 10**. Same `005331A0` child walk.

### 2.1 Dest layout — not a name fork (PROVEN generic)

| Screen | Root type | Layout path |
| --- | --- | --- |
| `UI_FRONTEND_PRESS_START_MENU` | 10 | `LayoutFrontendWidgets` → `FrontendLayout.Compute` |
| `UI_FRONTEND_NEW_PROFILE_SCREEN` | 10 | same |
| `UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` | 10 | same |

`FrontendLayoutTests` dest table is PRESS_START-only. NEW_PROFILE /
MAIN_MENU dests are **unasserted**, not hardcoded. Root `+520`
remap size is persist-generic (`16-resolution.md`).

### 2.2 Leftover name/type forks

| Site | What it does | Native | Leftover |
| --- | --- | --- | --- |
| `AttachPressStartWidgets` / `BindNewProfileFromArmedTick` / `AttachFrontendMainMenu` / `CommitNewProfileFromArmedEdit` | Rebuild tree by **slot name** | **PROVEN** slots `0x14` / `0x17` / `0059899A` | Fine as attach names. Triple constants also live on `FrontendMessages`. |
| `AttachFrontendTree` | After any root, still scans `UI_PRESS_START_TEXT` / `TEXT_GUI_MENU_PRESS_BUTTON` for `FrontendPressStartLabel`; notes every child as `005331A0` via `FrontendPressStartCtorFn` | Child walk is generic `005331A0` | PRESS_START **label** leftover on NEW_PROFILE / MAIN_MENU. |
| `DrawFrontendWidgets` | `if (FrontendRootType == FrontendPressStartType)` → `00530260` walk; else type-0 `0041AFA0` | Draw is `vtbl+8`: type 5/10/12/18 = `00530260` (`DrawsChildList`) | Fork is named PRESS_START **type 10**, not `DrawsChildList` at the **root**. All three first-seen roots are 10 so they take the container path. A type-5 root would take the else arm. |
| `InitFrontendUi` | Ctor notes `0054E3D0` vs `0041B800` on `FrontendRootType == 10` | Type 10 vs 0 | PRESS_START-named constant for Menu type. |
| `FrontendInputMap.MessageFromAction` | `action 26 && screen == PressStartMenu` → `0xE5`; else null | `0054E280` posts `&widget+352` on **any** type-10 | Screen-string stand-in. NEW_PROFILE / MAIN_MENU type 4 does **not** post stored id. Tests lock that null. |
| `tools/_frontend/TransformDump.cs` | Hard `rootName = UI_FRONTEND_PRESS_START_MENU`; dump filter is PRESS_START widget names | Tool | Not production. `CurrentLeftover` still uses texture W/H **without** GraphicIndex gate (diverge vs live layout). |

### 2.3 Not leftover dest hacks

- No `512`/`384`/`320`/`240` in live dest writers (`EngineLifecycle` /
  `FrontendLayout` / factory).
- Sprite bind is persist `GraphicIndex` → bank id, not a title-name map
  (`FrontendSpriteBank`).

### 2.4 Proposed (do not apply here)

1. Keep slot **names** as recovered attach strings.
2. Root draw: `FrontendWidgetType.DrawsChildList(FrontendRootType)`,
   not `== FrontendPressStartType`.
3. Stop scanning `UI_PRESS_START_TEXT` after a generic attach.
4. Input: post stored type-10 id (widget+352 analog), not
   `screen == PressStartMenu`. Until stored ids exist, keep the
   isolated table and inject `0x126` / 15.
5. Add dest-table tests for NEW_PROFILE / MAIN_MENU from persist,
   same calculator. Do not invent numbers.

---

## 3. Return mapping leftovers

### 3.1 Recovered (PROVEN / DISPROVEN)

| Event | Action | Frontend message |
| --- | --- | --- |
| Type 4 (`[record+40]`) | 26 | Press Start: post widget+352 = `0xE5` |
| Type 1, DIK 28 (Return) | 33 | last-key==1 `00597BF2` — **not** `0xE5` / `0x126` / 15 |
| Type 1, other keys | 33 | same |
| `0x126` | UNREAD poster | `UI_ACCEPT_NEW_PROFILE` persist stores it; `0059A238` → `00851920` |
| 15 | UNREAD poster | `0059A2DA` → Leave |

C# `MaybeActivateNewGameFromInput` uses `TryMapEvent` only.
Name is leftover: it dispatches **any** mapped UI message, not
New Game. Return does not enter it.

Tests: `Frontend_press_start_Return_does_not_post_0xE5_or_15`,
`Keyboard_and_Return_do_not_map_to_a_frontend_message`,
`Frontend_type4_then_injected_0x126_then_15_leaves` (Return on
NEW_PROFILE / MAIN_MENU still no-op).

### 3.2 Host leftover

`src/Fable.Client/Program.cs`:

```
Key.Enter → QueueInput(TypeKey, PlayAviSkipReturn=28)
```

That is correct for **PlayAVI skip** (`IsPlayAviSkipScan`). On
frontend it is type 1 / action 33 and does **not** leave PRESS_START,
accept the profile, or fire New Game. Live host cannot advance
frontend without injecting type 4 / `0x126` / 15.

Do **not** map Enter → `0xE5` / `0x126` / 15. That is the old
DISPROVEN host.

### 3.3 Screen-name Return leftover — none

No remaining C# path maps Return differently on PRESS_START vs
NEW_PROFILE vs MAIN_MENU. All three: `TryMapEvent` null.

The leftover **screen** fork is type-4 → `0xE5` **only** when
`FrontendMenuRoot == PressStartMenu` (§2.2), not Return.

### 3.4 Proposed (do not apply here)

1. Keep Return unmapped to UI messages.
2. Keep type 4 → action 26. Physical DIK stays **UNREAD**.
3. Host may later queue type 4 from a recovered device; do not
   guess Start / A / click.
4. `0x126` / 15 stay inject-only until a `.text` poster exists.
5. Rename `MaybeActivateNewGameFromInput` when touching (it is
   `0059A238` dispatch from recovered events).

---

## 4. Stale docs vs current C#

| Doc | Claim | Now |
| --- | --- | --- |
| `proofs/glyph-uv-gaps` §1 | Factory drops Font; Collect always `ENG_ARIAL_16` | Factory resolves 24. Collect **fallback** still 16. |
| `glyph-uv-gaps` “FrontendWidget has no Font field” | — | **STALE** (`IEngineHost.FrontendWidget.Font` / `FontFace`). |
| `FORWARD_TREE.md` PRESS_START row | type-6 + `ENG_ARIAL_16` atlas | Persist face is **24**; 16 is `0054F4B0`. |
| `11-transform.md` dest table | flags 0 → TEXT `320,240` | First-seen root `+520=1` → `512,384` (`16-resolution.md`, layout tests). |
| `05-input.md` | `MaybeActivateNewGameFromInput` left untouched | It already uses `TryMapEvent`; name leftover only. |

---

## 5. Do not invent

- Font measure as `0041AFA0` leftover / dest size.
- Dest−0.5 on type `0x22` sprites.
- Return / Escape / Space as `0xE5` / `0x126` / 15.
- Per-screen dest numbers in `FrontendLayout`.
- `00AB7B00` width 301 as type-6 `+204` without a writer.
- English names for unread persist CRCs (`+508` included).
