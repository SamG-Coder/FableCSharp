# Who posts frontend message 15 (New Game)

Investigation only. No production `src/` edits.

Authority: `Fable.exe` + inflated `frontend.bin` + `names.bin`.
Dumps: `tools/Fable.ExeIndex/out/01-sections/newgame-trace/ui-text-new-game-00595b24.md`,
`ui-frontend-main-menu-0059899a.md`;
`implementer/frontend/persist-scan.txt`, `05-input.md`;
`proofs/who-posts-0x126-and-15/README.md`;
`FrontendUiDefTests.Persist_00631C60_plus189_plus190_are_u8_and_font_is_names_offset`;
`FrontendInputTests.Type4_drives_lifecycle_0xE5_then_0x126_then_15`.

Status: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

---

## Verdict

| Claim | Class |
| --- | --- |
| Message **15** is persist `+224` on type **11** `UI_FRONTEND_BUTTON_NEW_GAME` | **PROVEN** |
| Same file i32 sits after CRC `0x53C644E4` and again after `0xF1A22807` (`Action`) | **PROVEN** |
| Poster is type-11 action **26** `0054DBC0` → UI vtbl+32 `0059A238`(15) | **PROVEN** |
| `0055B040` copies `[def+224]` through vtbl+284 (same copy as 0x126) | **PROVEN** |
| `UI_TEXT_NEW_GAME` is type **6** label; `00595B24` third arg **id=0** is a menu-list slot, not msg 15 | **PROVEN** |
| Type-0 `0041B800` / `0122F5D4+284` `0052F040` `ret 4` posts 15 | **DISPROVEN** |
| Type-10 `0054E280` `+352` posts 15 on Main Menu | **DISPROVEN** (that site posts attach-stored **0xE5** on Press Start) |
| Return (DIK 28) posts 15 | **DISPROVEN** (type 1 / action 33) |

**Who posts 15:** type-11 `UI_FRONTEND_BUTTON_NEW_GAME` after Main Menu attach, on input type 4 → action 26 → `0054DBC0` posting persist id 15.

---

## 1. Persist children (message id 15)

`UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` is type **10**.
`005331A0` walks persist `Children` (same walk as Press Start).

First child of `UI_FRONTEND_LIST_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` is
`UI_FRONTEND_BUTTON_NEW_GAME`:

| Widget | Type | Persist message | Role |
| --- | ---: | ---: | --- |
| `UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` | 10 | 0 (not 15) | root menu |
| `UI_FRONTEND_LIST_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` | 12 | 0 | list |
| `UI_FRONTEND_BUTTON_NEW_GAME` | **11** | **15** | click poster |
| `UI_TEXT_NEW_GAME` | 6 | 0 | `TEXT_GUI_MENU_NEW_GAME` label |
| `UI_NEW_GAME_BUTTON` | unread here | — | name only in `names.bin` |

File form (`00431102`): CRC then i32.

| CRC | Scan label | `BUTTON_NEW_GAME` i32 |
| --- | --- | ---: |
| `0x53C644E4` | name **UNREAD** (`not` `Message` / `MessageId`) | **15** |
| `0xF1A22807` | `Action` | **15** |

Same CRC pair on Press Start type-11 `UI_FRONTEND_BUTTON_INVISIBLE` holds **229** (`0xE5`) — pattern lock in `implementer/frontend/persist-scan.txt` `@1089 *Action i32=229`.

`FrontendUiDef.ReadPersistI32` + test:

```
UI_ACCEPT_NEW_PROFILE  type 38  MessageId=0x126
UI_FRONTEND_BUTTON_NEW_GAME  type 11  MessageId=15
```

`0055B040` copies `[def+224]` then vtbl+284. That is the runtime slot the action-26 poster reads.

---

## 2. Type-0 click vs type-10 / type-11 action 26

| Kind | VA | What it posts |
| --- | --- | --- |
| Type 0 ctor | `0041B800` vtbl `0122F5D4` | graphic button (`UI_TITLE_01`) |
| Type 0 vtbl+284 | `0052F040` | **`ret 4`** — **DISPROVEN** as a poster |
| Type 10 ctor | `0054E3D0` vtbl `012497E4` | Press Start / Main / New Profile roots |
| Type 10 store | `0054E4F0` | widget **+352** |
| Type 10 action | `0054E280` case 0 `0054E2FA` | `&+352` → `0059A238` |
| Type 11 ctor | `0054E0B0` size `0x1B4` | `UI_FRONTEND_BUTTON_NEW_GAME` / `…_INVISIBLE` |
| Type 11 action | **`0054DBC0`** | persist `[def+224]` / stored id |
| Type 38 action | `0055AD60` | same copy; posts **0x126** |

`0042E3EE` type **4** (`00A03C80` writes `[rec+40]=4`) → `push 26`.
Action 26 is the click/accept action. It is **not** a DIK.

On Press Start the focused type-10 has +352 = `0xE5` (`00598EE6`), so `0054E280` posts **0xE5**.
On Main Menu the type-10 root does **not** store 15. The focused/listed type-11 child stores **15**. `0054DBC0` posts that.

Type-0 never holds 15 in persist (`TITLE_01` `*Action i32=0`).

---

## 3. `00595B24` menu build — id=0 is a slot, not message 15

`0059899A` (empty continue list) → `00595A06` attach
`UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE`, **then** `00595B24`.

`00595B24` interns labels and calls registrar `00595AD9` with a third-arg id:

| Label | `push` id | Class |
| --- | ---: | --- |
| `UI_TEXT_NEW_GAME` | **0** | menu-list slot |
| `UI_TEXT_LOAD_GAME` | **0** | same (second slot 0) |
| `UI_TEXT_OPTIONS_MENU_TITLE` | 24 then 1 | |
| `UI_TEXT_GAME_OPTIONS_MENU_TITLE` | 1 | |
| `UI_TEXT_VIDEO_MENU_TITLE` | 5 | |
| `UI_TEXT_SCOREBOARD_MENU_TITLE` | 25 | |
| `UI_TEXT_REDEFINE_KEYS_MENU_TITLE` | 22 | |
| `UI_TEXT_AUDIO_OPTIONS_MENU_TITLE` | 4 | |

Dump: `ui-text-new-game-00595b24.md` `push 0` at `00595B56` before first `call 00595AD9`.

`EngineLifecycle.FrontendMenuItems[0] = ("UI_TEXT_NEW_GAME", 0)` matches that slot table.
`005959AB` searches those labels. That id is **not** `0059A238` message 15.

`UI_TEXT_NEW_GAME` persist Type=6. It cannot be the click poster.

---

## 4. Path: Main Menu attach → `0059A238`(15)

```
0059A238 msg 0x126
  → 00851920  UI_ACCEPT_NEW_PROFILE  [ui+96+5]=1
same-frame / next 00599E3F
  → 0059697A  004067C0 writable
      00595A06  UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE
      005331A0  children (list + UI_FRONTEND_BUTTON_NEW_GAME type 11)
      00595B24  label table (id=0) / 00594FA9(0) clear [ui+96]
  or 0059899A (one saved name / msg 0x124) → same 00595A06 + 00595B24

0042E3EE  type 4 → action 26
  0054DBC0  type 11  posts persist 15
  0059A238  msg 15
    0059A2DA  [ui+28].vtbl+16
    00594F28  [retail+41]=1
0042EC7C  +41 → Leave 0042F2A2
```

`FrontendInputTests.Type4_drives_lifecycle_0xE5_then_0x126_then_15` (install):
after 0x126 tick, widgets contain
`UI_FRONTEND_BUTTON_NEW_GAME` `MessageId==15`; next type 4 leaves frontend.

---

## 5. Not Return

| Event | Action | Message |
| --- | ---: | --- |
| Type 4 | 26 | stored widget id (0xE5 / 0x126 / **15**) |
| Type 1, DIK 28 (Return) | 33 | `00597BF2(1)` last-key — **not** 15 |
| Type 1, other keys | 33 | same |

`FrontendInputMap.TryMapEvent(TypeKey, 28, *)` is null.
Host Enter is PlayAVI skip / type 1 only. Mapping Return → 15 is the old **DISPROVEN** stand-in.

Physical device that synthesizes type 4 stays **UNREAD**. That does not un-prove the poster once action 26 is on the type-11 widget.

---

## Classification table (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `0059A238` | UI vtbl+32 consumer | **PROVEN** |
| `0059A2DA` / `00594F28` | msg 15 → `[retail+41]=1` | **PROVEN** |
| `00595A06` | attach Main Menu name | **PROVEN** |
| `00595B24` / `00595AD9` | label + slot id (NEW_GAME **0**) | **PROVEN** as registrar; **DISPROVEN** as msg-15 poster |
| `005331A0` | persist child walk | **PROVEN** |
| `0054E0B0` | type 11 ctor | **PROVEN** |
| `0055B040` | `[def+224]` → vtbl+284 | **PROVEN** |
| `0054DBC0` | type 11 action 26 posts stored id | **PROVEN** |
| `0054E280` / `0054E2FA` | type 10 posts +352 | **PROVEN** for 0xE5; **DISPROVEN** for 15 |
| `0052F040` | type 0 vtbl+284 | **DISPROVEN** poster (`ret 4`) |
| `0042E3EE` / `00A03C80` | type 4 → action 26 | **PROVEN** |
| Return DIK 28 | type 1 / action 33 | **DISPROVEN** as 15 |
| CRC `0x53C644E4` field name | — | **UNREAD** |
