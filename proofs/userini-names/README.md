# Shipped `user.ini` `ActivateQuest` names — only `Gameflow`?

Investigation only. No production `src/` edits.

Do **not** invent a second `ActivateQuest` name.
Do **not** treat `userst.ini` `SetStartingHolySite("NOVStartHSP")`
as a quest start.
Do **not** start at `00DBDE40` / `Q_NewOakValeIntro`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: TLC install root
`C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters\user.ini`
(and sibling `userst.ini`); install-wide grep `ActivateQuest`;
`proofs/userini-activatequest/README.md`;
`proofs/ini-activate-quest/README.md`;
`docs/runtime/FORWARD_TREE.md` §2 after vtbl+32;
`docs/PARITY.md` Init Game suffix;
`EngineLifecycle.EnterGame` / `LoadWorld` /
`FinishInitGameAfterWorld` / `ApplyUserIniCommands`;
`EngineLifecycleTests`
(`InitGame_004184BD_after_00416953_reserves_then_user_ini`,
`UserIni_009EC890_RunScript_joystick_is_00999230_miss`,
`Init_quests_004B4260_activates_wld_initial_list`).

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Shipped `ActivateQuest` names? | **Only `"Gameflow"`.** One line in first-seen TLC `user.ini`. Install-wide grep under the TLC root: **one** hit. | **PROVEN** |
| Other quest names in that file? | **No.** Remaining tokens are display / mesh / `RunScript`. Not quests. | **DISPROVEN** as quest names |
| Host `ApplyUserIniCommands` after `LoadWorld`? | **Yes.** `EnterGame` is `LoadWorld()` then `FinishInitGameAfterWorld()` → `ApplyUserIniCommands`. Native is `00416953` then `00418981` `009EC890`. | **MATCH** |
| Host `ActivatedQuests` vs native first-seen starts? | Nine QST TRUE (`world+172` inside `LoadWorld`) then `Gameflow` from `user.ini`. | **MATCH** |

---

## Verdict

First-seen TLC `user.ini` has **one** `ActivateQuest`
argument: **`Gameflow`**. That is the only shipped name.
Host must not invent another.

Host applies that file **after** `LoadWorld`, same as
native `004184BD` after vtbl+32 `00416953`. Token walk
is file order. `Gameflow` is the **10th** no-save start
(`ActivatedQuests[9]`), not a `world+172` name.

| Claim | Class |
|---|---|
| Shipped `ActivateQuest` = `"Gameflow"` only | **PROVEN** |
| Second shipped name (`Q_NewOakValeIntro`, `Q_FireHeart`, …) | **DISPROVEN** |
| `userst.ini` / `default_user.ini` / `joystick.ini` add a name | **DISPROVEN** (0 lines / files absent) |
| `ApplyUserIniCommands` after `LoadWorld` | **MATCH** |
| File token order = host `UserIniCommands` walk | **MATCH** (impl); test lock **PARTIAL** (`Contains` only) |
| `00419CE0` generic — later-edit extra line | **UNREAD** as a later-edit; do not invent |
| Comment “vtbl+1104 UNREAD — do not start a quest here” | **LEFTOVER** (code **does** `ActivateNamedQuest`) |

---

## 1. First-seen TLC `user.ini`

Install root, first-seen file (re-read this walk):

```
SetMaxAnisotropy(4);
RunScript("joystick.ini");
SetMaxAnimatedMeshDist(64);
SetMaxStaticMeshDist(128);

MaxThingDrawDist 128;

ActivateQuest("Gameflow");

SetPlatform2DGain(0.6);

SetFullscreen(false);
```

| # | Token | Argument | Quest? |
|--:|---|---|---|
| 1 | `SetMaxAnisotropy` | `4` | no (`009EB260` unknown) |
| 2 | `RunScript` | `joystick.ini` | no (nested `009EC890` miss) |
| 3 | `SetMaxAnimatedMeshDist` | `64` | no |
| 4 | `SetMaxStaticMeshDist` | `128` | no |
| 5 | `MaxThingDrawDist` | `128` | no |
| 6 | **`ActivateQuest`** | **`Gameflow`** | **yes** |
| 7 | `SetPlatform2DGain` | `0.6` | no |
| 8 | `SetFullscreen` | `false` | no |

Install-wide grep of `ActivateQuest` under the TLC root:
**one** hit, line 8 of `user.ini`. **PROVEN**.

| File | Present? | `ActivateQuest` |
|---|---|---|
| `user.ini` | yes | **1**, `"Gameflow"` |
| `default_user.ini` | no | none |
| `joystick.ini` | no | none |
| `userst.ini` | yes, Parse Command Line only | **0** |
| `default_userst.ini` | no | none |

`userst.ini` has `SetLevel("FinalAlbion.wld")` and
`SetStartingHolySite("NOVStartHSP")`. Zero
`ActivateQuest`. Applied at `00414C66` **before**
frontend / Leave / `00419D90`. **DISPROVEN** as this
path (`proofs/ini-activate-quest`).

Graphics lines may be a local Steam-root edit. That
does not add a second `ActivateQuest`. **PROVEN** for
quest names; other-line provenance **PARTIAL**.

---

## 2. Native order after `LoadWorld`

`004184BD` after `[game].vtbl+32` `00416953`:

```
00416953  Load world
  004A1840  QST / WLD / Startup WAD
  [0x13B8648]==0 → 0049F180
    004B4260([world+172])   // nine QST TRUE — not ini
  004BBC00  ret 4
0049BA70 / 00416392 / 004AE9D0
00418922  default_user.ini  00999230 miss
00418969  user.ini          009EC890          // AFTER LoadWorld
  009EC710 tokens in file order
    ActivateQuest("Gameflow")                 // ONLY this name
      00419CE0 → 00892E80 → 004B4A10(1,1)
      004B4260 → 00CB5AD0 "Gameflow"
009A4EC0 seed 004167DA / +90592
```

`00419CE0` copies `[cmd+8]`. No immediate `"Gameflow"`
in the thunk. First-seen argument is still `"Gameflow"`.
A later-edit second line would use the same three VAs.
That case is **UNREAD**. Host must not invent one.

`world+172` (QST `AddQuest` TRUE) is **inside**
`00416953`. `user.ini` is **after** `00416953` returns.
`Gameflow` is QST `AddQuest(..., FALSE)` so it is
**not** in `+172`. **PROVEN**
(`proofs/userini-activatequest`, `proofs/ini-activate-quest`).

---

## 3. Host `ApplyUserIniCommands` after `LoadWorld`

`EnterGame`:

```
LoadWorld();                     // 00416953: QST, WLD, 0049F180, 004BBC00
GameRenderEnabled = true;
Note(GameLoadWorldFn, … 00416953);
FinishInitGameAfterWorld();      // 004184BD suffix
```

`LoadWorld` (no-save `[0x13B8648]==0`) runs
`InitCharactersAndQuests` → `004B4260` on
`WorldPlus172` (nine names). Then
`AfterLoadWorldFn` `004BBC00`.

`FinishInitGameAfterWorld`:

```
0049BA70 / 00416392 / 004AE9D0
default_user.ini  00999230 miss (TLC absent)
UserIniVa         009EC890 user.ini
  if Install\user.ini exists:
    009EC710 / 009EB430
    ApplyUserIniCommands(userIni, "InitGame")
009A4EC0 004167DA
```

`ApplyUserIniCommands` walks `File.ReadAllLines` in
file order, records each token name, and
`DispatchUserIniCommand`. `ActivateQuest` →
`ActivateNamedQuest(arg, "InitGame")`.

That is the same **after-`LoadWorld`** site as
`00418981`. **MATCH**.

`EngineLifecycleTests.InitGame_004184BD_after_00416953_reserves_then_user_ini`
locks:

```
004BBC00 < 0049BA70 < 00416392 < 004AE9D0
  < default_user.ini miss < user.ini < 004167DA
```

**PROVEN** vs host trace.

`Init_quests_004B4260_activates_wld_initial_list`
locks first-seen starts:

```
WorldPlus172[0..8] = ActivatedQuests.Take(9)
ActivatedQuests[9] = "Gameflow"
Count = 10
```

`Gameflow` is later than `+172`, from `user.ini`.
**MATCH**.

---

## 4. Host vs native (this file)

| Host | Native | Class |
|---|---|---|
| `LoadWorld()` then `FinishInitGameAfterWorld` | `00416953` then `00418981` | **MATCH** |
| `ApplyUserIniCommands` line order | `009EC710` token order | **MATCH** |
| `DispatchUserIniCommand("ActivateQuest")` → `ActivateNamedQuest(arg)` | `00419CE0` copies `[cmd+8]` | **MATCH** |
| First-seen `arg` / `ActivatedQuests[9]` | `"Gameflow"` | **MATCH** |
| `UserIniCommands` contains `SetMaxAnisotropy` / `RunScript` / `ActivateQuest` | those tokens exist | **PROVEN** |
| `UserIniCommands` exact 8-name sequence | file order | **MATCH** impl; test **PARTIAL** (`Contains` only) |
| `UserIniCommands` stores names, not args | walker names | **MATCH** as names; `"Gameflow"` is on the activate note / `Runtime` |
| `joystick.ini` miss | `00999230` miss | **MATCH** |
| Comment on `ApplyUserIniCommands`: do not start a quest | thunk **does** start `Gameflow` | **LEFTOVER** |
| Invent a second `ActivateQuest` name | not in shipped file | **DISPROVEN** |
| Apply `userst.ini` after `LoadWorld` | `00414C66` is command line | **DISPROVEN** |

Without an install, host notes `009EC890 user.ini` and
skips the file walk (`Install is null`). Native always
calls `009EC890`, which then `00999230`-gates. First-seen
TLC **has** the file. No-install skip is a test harness
artifact, not a shipped-name **DIVERGE**.

---

## Classifications (short)

1. **Shipped `ActivateQuest` names — PROVEN: `Gameflow` only.**
   TLC `user.ini` one line. Install-wide grep one hit.
2. **Host after `LoadWorld` — MATCH.**
   `EnterGame` is `LoadWorld` then
   `ApplyUserIniCommands`. Native is `00416953` then
   `00418981`. Nine `+172` names first; `Gameflow` 10th.
3. **Other shipped names — DISPROVEN.**
   Not `Q_NewOakValeIntro`. Not `Q_FireHeart`. Not
   `GameflowAssistance`. Not `userst.ini` `NOVStartHSP`.
4. **Generic thunk / extra line — UNREAD as later-edit.**
   Do not invent a second name from this file.
5. **`ApplyUserIniCommands` “do not start a quest” comment — LEFTOVER.**
   Live path starts `"Gameflow"`.
