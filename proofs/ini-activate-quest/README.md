# `00419CE0` IniActivateQuestThunk — `user.ini` vs `userst.ini` after Leave

Investigation only. No production `src/` edits.

Do **not** start at `00DBDE40` / `Q_NewOakValeIntro`.
Do **not** treat `userst.ini` `SetStartingHolySite("NOVStartHSP")`
as a quest start.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: TLC install `user.ini` / `userst.ini`;
`listing-00400000.txt` (`00413C50` / `00414C66` / `004184BD`
`00418922`–`00418981` / `004197B0` / `00419CE0` / `00419D90`);
`listing-00480000.txt` (`004A712B` / `004B4A10`);
`listing-00880000.txt` (`00892E80`);
`docs/runtime/FORWARD_TREE.md` §§2, 6–7;
`docs/PARITY.md` Init Game suffix / Who activates
`Q_NewOakValeIntro`;
`EngineLifecycle` (`IniActivateQuestThunk` / `ApplyUserstIni` /
`FinishInitGameAfterWorld`);
`EngineLifecycleTests`
(`UserIni_009EC890_RunScript_joystick_is_00999230_miss`,
`Userst_00413C50_SetFullscreen_false_is_009BF7E0_windowed`,
`Init_quests_004B4260_activates_wld_initial_list`,
`No_save_does_not_activate_Q_NewOakValeIntro`).

---

## Verdict

After Leave, **one** ini quest runs: TLC `user.ini`
`ActivateQuest("Gameflow")` via `00419CE0`.

`userst.ini` is **not** applied after Leave. It is applied at
Parse Command Line (`00413C50` → `00414C66`) **before** the
frontend, **before** message 15, **before** Init World. That
file has **zero** `ActivateQuest` lines.

`00419D90` (register `"ActivateQuest"` → handler `00419CE0`)
has **one** `E8` site: `004A712B` inside Init World
`"Init Global Console"`. That is after Leave and **after**
`userst.ini`. A `userst.ini` `ActivateQuest` line would miss
the command table (`009EB260`). `00419CE0` also needs
`[0x13B86A0]` game + `[game+36]` world + `[world+56]`
script manager — none exist at command-line parse.

| File | When `009EC890` | After Leave? | `ActivateQuest` lines | Quests started |
|---|---|---|---|---|
| `default_userst.ini` | `00414C10` if `00999230` | no | file absent (TLC miss) | none |
| `userst.ini` | `00414C66` if `[0x1375444]!=0` (PE 1) | **no** | **0** | **none** |
| `default_user.ini` | `00418922` if `00999230` | yes (gated) | file absent (TLC miss) | none |
| `user.ini` | `00418969` unconditional | **yes** | **1** (`"Gameflow"`) | **Gameflow** |
| `joystick.ini` | `009ECB70` from `user.ini` `RunScript` | yes if present | file absent (TLC miss) | none |

WLD `START_INITIAL_QUESTS` (`004B4260` on `world+172`) is
**not** an ini path. Those six names run **before** `user.ini`.

---

## Timeline (no-save New Game)

```
00402510 "Parse Command Line"
  [0x137548F]!=0 → 00413C50
    009ED190 BindKey / RunScript
    register SetLevel 00413800, SetStartingHolySite 00413840, …
      NOT ActivateQuest
    default_userst.ini 00999230 miss
    [0x1375444]!=0 → 00414C66 009EC890 userst.ini
      SetFullscreen / SetResolution / SetLevel("FinalAlbion.wld")
      SetStartingHolySite("NOVStartHSP") → [0x13B866C]   // not a quest
      no ActivateQuest
…
0059A238 msg 15 → [retail+41]=1
0042F2A2 Leave frontend
0042F491 Init Game 00418DCA → 004184BD
  Init World 004A6E30
    004A712B 00419D90                          // REGISTER only
      bind "ActivateQuest" [cmd+20]=00419CE0
      009EC5E0 into [0x13CAA40]
  00416953 Load world
    00507C30 START_INITIAL_QUESTS → world+172
    004B4260 six WLD names                     // not ini
  00418922 default_user.ini 00999230 miss
  00418969 push 0x122F01C user.ini 009EC890    // AFTER Leave
    SetMaxAnisotropy → 009EB260 unknown
    RunScript("joystick.ini") 009ECB70 / 00999230 miss
    ActivateQuest("Gameflow")
      009EB430 → [cmd+20] 00419CE0
        [game].vtbl+36 004197B0 xor al,al      // never skip
        [world+56] vtbl+1104 00892E80
        00892E80 [0x13B89FC] 004B4A10(name,1,1)
        004B4A10 → 004B4260 → 00CB5AD0 "Gameflow"
    SetFullscreen(false)                       // display, not a quest
```

---

## 1. `00419CE0` is the handler, not the registrar

`00419D90` alloc 24, name `"ActivateQuest"` (`0x0122F380`),
vtbl `0x122E65C`, `[esi+20]=0x419CE0`, then `009EC5E0`.
**PROVEN** (`listing-00400000.txt` `00419DF0`).

`00419CE0`:

```
ecx = command object
call [game.vtbl+36]            // 004197B0; al=0
copy CString from [this]+8     // ini argument
ecx = [[game+36]+56]           // world script manager 006E7740
call [vtbl+1104]               // 00892E80
```

`00892E80` is `push 1; push 1; push name; 004B4A10`.
Sibling `00892EA0` is `004B4260` (same 1,1). Sibling
`00892EC0` is `004B4A10` with `(1,0)`. Direct `00CB5AD0`
from the ini walker is **DISPROVEN**.

`004197B0` (`xor al,al; ret`) is game vtbl+36. Used here as
a never-skip gate. Broader role of that slot is still
**UNREAD** (FORWARD_TREE slot 9).

---

## 2. TLC files (first-seen)

`user.ini` (install root):

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

Install-wide grep of `ActivateQuest`: **one** hit, that line.
**PROVEN**.

`userst.ini`: display / compile / `SetLevel` / `SetStartingHolySite`
only. No `ActivateQuest`. **PROVEN**.

`SetStartingHolySite` handler `00413840` stores the CString at
`[0x13B866C]`. `SetLevel` `00413800` stores at `[0x13B8668]`.
Neither calls `004B4A10` / `00CB5AD0`. **PROVEN**.

---

## 3. Quests that *do* start after Leave (order)

| # | Name | Source | Via `00419CE0`? |
|--:|---|---|---|
| 1 | `Q_SunnyvaleMaster` | WLD `world+172` `004B4260` | no |
| 2 | `PersonalScriptMain` | same | no |
| 3 | `PersonalScript_GlobalThings` | same | no |
| 4 | `HeroBoasts` | same | no |
| 5 | `V_HeroDolls` | same | no |
| 6 | `CS_PlayCutscene` | same | no |
| 7 | `Gameflow` | `user.ini` `00419CE0` | **yes** |

`GameflowAssistance` is in `00CD52D0` but is **not** on this
walk. `Q_NewOakValeIntro` is bind-only. **PROVEN**.

The thunk is generic: another `ActivateQuest("…")` line in
`user.ini` would start that name the same way. TLC first-seen
file does not have one. **PROVEN** file; extra names
**UNREAD** as a later-edit case.

---

## 4. Host vs native

| Host | Native after Leave | Class |
|---|---|---|
| `FinishInitGameAfterWorld` `009EC890 user.ini` | `00418969` | **PROVEN** |
| `ActivateNamedQuest("Gameflow")` from `ActivateQuest` | `00419CE0` → `00892E80` → `004B4A10` | **PROVEN** |
| `ApplyUserstIni` at Parse Command Line | `00413C50` / `00414C66` | **PROVEN** |
| `DispatchUserIniCommand("ActivateQuest")` notes `InitGame` even if `stage` is Parse Command Line | `00419D90` not yet run | **DIVERGE** if `userst.ini` gained a line; first-seen TLC has none |
| Comment on `ApplyUserIniCommands`: “vtbl+1104 is UNREAD — do not start a quest here” | thunk **does** start Gameflow | **LEFTOVER** comment |
| Invent `ActivateQuest(Q_NewOakValeIntro)` from `NOVStartHSP` | holy-site store only | **DISPROVEN** |

---

## Classifications (short)

1. **Ini quest after Leave — PROVEN: `Gameflow` only.**
   TLC `user.ini` one `ActivateQuest`. Path is `00419CE0` →
   `00892E80` → `004B4A10` → `00CB5AD0`.
2. **`userst.ini` after Leave — DISPROVEN.**
   Applied at command line only. No `ActivateQuest` in the
   file. Name not registered until `004A712B`.
3. **`00419D90` as the activate call — DISPROVEN.**
   It only registers. The live call is `00419CE0`.
4. **`userst.ini` `NOVStartHSP` as Oakvale activate — DISPROVEN.**
   String store at `0x13B866C`.
5. **Who later activates `Q_NewOakValeIntro` — UNREAD.**
   Not this ini walk. Do not invent it.
