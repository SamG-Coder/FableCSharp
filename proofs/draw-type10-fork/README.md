# `DrawFrontendWidgets` type-10 fork: MATCH vs leftover name

Investigation only. No production `src/` edits.

Authority: `EngineLifecycle.DrawFrontendWidgets` /
`FrontendRootType` / `FrontendPressStartType`;
`FrontendWidgetType` `Menu` / `DrawsChildList`;
`Fable.exe` `00595222` / `0054E3D0` / `0052CC50` /
`0054C3A0` / `00547600` / `0041B800` / `00530260` /
`0041AFA0`; persist `frontend.bin`
`implementer/frontend/01-widget-construction.md`,
`14-container.md`;
`tests/Fable.Formats.Tests/FrontendUiDefTests.Factory_builds_press_start_then_main_menu_from_the_same_walk`;
`EngineLifecycleTests.Frontend_PRESS_START_is_type_10_with_text_child`;
`proofs/audit-frontend-leftover/README.md` §2.2.

Do not re-prove PRESS_START dest table, type-10 ctor
`0054E3D0` → `0052CC50` vtbl `012497E4`, or type-6
`0054EF00` glyphs.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

---

## Verdict

**First-seen Main Menu and New Profile take the same
draw arm as Press Start.** All three roots are persist
**Type=10** (`FrontendWidgetType.Menu`). C#
`if (FrontendRootType == FrontendPressStartType)` is
`== 10`, so they all call `00530260`. That **path is
MATCH**.

The **name** is leftover. Type 10 is Menu, not
Press-Start-only. Native `00595222` does **not**
compare type 10 vs 0. It calls **`[node+20].vtbl+8`**.
`vtbl+8` is `00530260` on types **5 / 10 / 12 / 18**
and `0041AFA0` on type **0**. A type-5 root would hit
the C# else arm (`0041AFA0`) and **diverge**. No
first-seen frontend root is type 5.

| Claim | Status |
| --- | --- |
| `DrawFrontendWidgets` forks `FrontendRootType == 10` → `00530260`, else `0041AFA0` | **PROVEN** (`EngineLifecycle.cs`) |
| `FrontendPressStartType` is the int **10** | **PROVEN** |
| `UI_FRONTEND_PRESS_START_MENU` persist Type=10 | **PROVEN** `#620` |
| `UI_FRONTEND_NEW_PROFILE_SCREEN` persist Type=10 | **PROVEN** `#201` |
| `UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` persist Type=10 | **PROVEN** `#216` |
| Factory / `AttachFrontendTree` sets `FrontendRootType = widgets[0].Type` | **PROVEN** |
| After `0xE5` / `0x126` attach, root type stays **10** | **PROVEN** |
| Those three roots therefore take `00530260` | **MATCH** |
| Fork named Press Start / `FrontendPressStartType` | **LEFTOVER** name (Menu type) |
| Native walk is screen-name or `==10` at `00595222` | **DISPROVEN** |
| Native draw is `vtbl+8` per constructed type | **PROVEN** |
| Type 10 / 5 / 12 / 18 `vtbl+8` = `00530260` | **PROVEN** (`14-container.md`) |
| Type 0 `vtbl+8` = `0041AFA0` | **PROVEN** (`0122F5D4`) |
| Type 6 `vtbl+8` = `0054EF00` | **PROVEN** (leaf, not this fork) |
| Child recurse already uses `DrawsChildList` (5/10/12/18) | **MATCH** |
| Else arm as “type-0 menu draw” for a type-5/12/18 **root** | **LEFTOVER** vs native (unhit first-seen) |

---

## 1. C# fork

`DrawFrontendWidgets` (`EngineLifecycle.cs`):

```
00595222 [ui+84]
if FrontendRootType == FrontendPressStartType   // 10
    00530260 vtbl+8 012497E4 +176
    DrawContainerWalk each root
else
    0041AFA0 vtbl+8 0122F5D4
    QueueFrontend2dRecord(null)
004292C0
```

`FrontendPressStartType = 10` is documented as the
PRESS_START persist type and vtbl `012497E4`. The
integer is Menu (`FrontendWidgetType.Menu`), not a
screen id.

`AttachFrontendTree` (Press Start, New Profile
`00596917`, Main Menu `00595A06`):

```
FrontendRootType = _frontendWidgets[0].Type
```

`InitFrontendUi` first-seen factory is
`UI_FRONTEND_PRESS_START_MENU`. Later attaches
rebuild the list; they do **not** keep a Press Start
type while drawing Main / Profile.

`DrawContainerWalk` (inside the `== 10` arm) already
dispatches children by type:

```
if DrawsChildList(widget.Type)   // 5 / 10 / 12 / 18
    recurse +176
else
    0041AFA0 / 0054EF00 leaf note + QueueFrontend2dRecord
```

The leftover is **only the root predicate**, not the
child walk.

---

## 2. Three first-seen roots are type 10

Persist (`01-widget-construction.md`):

| Def | # | Type | Role |
| --- | --- | --- | --- |
| `UI_FRONTEND_PRESS_START_MENU` | 620 | **10** | first-seen `00598A1C` slot `0x14` |
| `UI_FRONTEND_NEW_PROFILE_SCREEN` | 201 | **10** | `00596917` slot `0x17` after `0xE5` |
| `UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` | 216 | **10** | `00595A06` after `0x126` / empty continue |

`FrontendUiDefTests.Factory_builds_press_start_then_main_menu_from_the_same_walk`:

```
press[0].Type  == 10
menu[0].Type   == 10
profile[0].Type == 10
DrawsChildList(10) == true
```

Ctor is always `0054E3D0` → `0052CC50` then override
vtbl `012497E4` / inner `012497BC`. Same object for
all three screens. Native has **one** Menu class.

Children differ (list / edit / forest swap). Draw of
the **root** does not.

---

## 3. Native is `vtbl+8`, not `if type==10`

`00595222` (`FrontendUiDrawFn`): circular `[ui+84]`.
Each `[node+20]` → `call [vtbl+8]`, then
`004292C0`. No `cmp [def+60], 10`. No string
PRESS_START / MAIN_MENU / NEW_PROFILE.

Ctor writes the vtbl that slot reads:

| Type | Ctor | `[esi]` vtbl | `vtbl+8` |
| --- | --- | --- | --- |
| 10 Menu | `0054E3D0` | `012497E4` | `00530260` |
| 5 Group | `0052CC50` | `01245DE4` | `00530260` |
| 12 List | `0054C3A0` | `01249224` | `00530260` |
| 18 Swap | `00547600` | `012485AC` | `00530260` |
| 0 Button | `0041B800` | `0122F5D4` | `0041AFA0` |
| 6 Text | `0054F5C0` | `01249CCC` | `0054EF00` |

`00530260` walks `+176` then `+188` (clip /
parent tests in `14-container.md`). Type 0
`0041AFA0` packs dest `+248/+264` (leaf).

`docs/runtime/FORWARD_TREE.md` still has a stale
line that first-seen `[node+20] vtbl+8 = 0041AFA0
(0122F5D4)`. That is type **0**. The Press Start
**root** is type 10 / `00530260`. Type-0 draw is
the TITLE / forest **leaves**, not the menu root.

`FrontendWidgetType.DrawsChildList` is the recovered
predicate for that slot (`5 / 10 / 12 / 18`).

---

## 4. MATCH vs leftover

**MATCH (first-seen three screens):**

- Root type 10 → C# `00530260` == native
  `012497E4+8`.
- Main Menu and New Profile are not a second draw
  function. They reuse Menu `vtbl+8`.
- Children of those trees still go through
  `DrawContainerWalk` / `DrawsChildList`.

**LEFTOVER name:**

- Constant and note say Press Start (`012497E4`)
  because first-seen populate was
  `UI_FRONTEND_PRESS_START_MENU`.
- Native name of type 10 is Menu (`0054E3D0`),
  shared with Main / New Profile.

**LEFTOVER predicate (unhit):**

- `== 10` is narrower than `DrawsChildList`.
- Type-5 / 12 / 18 **root** would `0041AFA0` in C#
  and `00530260` in native.
- First-seen attach never builds those as
  `[ui+84]` roots. Type 12 is a **child**
  (`UI_FRONTEND_LIST_*`, `UI_NEW_PROFILE_MENU`).
  Type 5 is `UI_TITLE` / forest groups.

Proposed (not applied):
`FrontendWidgetType.DrawsChildList(FrontendRootType)`
at the root, same as the child walk.

---

## 5. Tests that lock this

| Test | What it locks |
| --- | --- |
| `Frontend_PRESS_START_is_type_10_with_text_child` | `FrontendRootType==10`, note `00530260` |
| `Factory_builds_press_start_then_main_menu_from_the_same_walk` | all three factory roots Type=10 |
| `Type_switch_table_comes_from_0041D7F8` | type 10 ctor/vtbl/size |

No test pumps New Profile / Main Menu and asserts
the `00530260` note. That note is implied by
`FrontendRootType=10` after `AttachFrontendTree`.
**PARTIAL** as a dedicated assert; the type itself
is **PROVEN**.
