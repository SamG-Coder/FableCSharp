# TLC New Game from a fresh profile is still the no-save walk

Investigation only. No production `src/` edits.

Do **not** invent a save. Do **not** invent
`ActivateQuest("Q_NewOakValeIntro")`. Do **not** collapse
CUIDef persist (`+189` / `+212` / `+545`) with FableSav
`QUESTS` / `START_NEW_QUEST`.

Question: does TLC New Game from a **fresh profile** still
take the recovered no-save walk (empty `+90584` skip of
`004B4A10`), or does creating the profile write a
save / quest list that presents `Q_NewOakValeIntro`?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `listing-00400000.txt` (`00416953` / `00416BCA` /
`00416BF0` / `00430340` / `00404A80` / `00404B50` /
`00404C50`); `listing-00480000.txt` (`004B5080` /
`004B58F3`); `listing-00580000.txt` (`0059A238` /
`0059A2DA`); `listing-00840000.txt` (`00851920` /
`00851770`); `e8.tsv` dest `004B5080` / `004A3200`;
TLC `user.ini`; siblings
`proofs/persist-plus189-first`,
`proofs/persist-plus189-newprofile`,
`proofs/persist-plus212`,
`proofs/newgame-plus545`,
`proofs/accept-newgame-plus545`,
`proofs/ini-activate-quest`,
`proofs/012C5D14-fablecrc-imm`,
`proofs/004A1840-second-site`,
`proofs/who-posts-0x126-and-15`,
`proofs/0059A238-first-consumes`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Fresh-profile New Game still no-save? | **Yes.** Empty `[game+90588]` skips `"Loading save"`. Empty `[game+90584]` vs `0x122D70E` skips `004B4A10`. | **PROVEN** |
| Does Accept New Profile write a FableSav / quest list? | **No.** `0x126` → `00851920` sets `[ui+96+5]=1`. No `004A3200` / `004B5080` / `004B4A10`. No lea of `+90584` / `+90588`. | **DISPROVEN** |
| Does New Game click parse `START_NEW_QUEST`? | **No.** Msg **15** → `0059A2DA` `[retail+8]=1` `[retail+41]=1` → Leave. `004B5080` unique `E8` is `004B58F3` inside save parse. | **DISPROVEN** |
| `user.ini` Oakvale? | **No.** One `ActivateQuest("Gameflow")`. | **PROVEN** |
| persist `+189` / `+212` / `+545` = save quest list? | **No.** CUIDef file bytes on frontend widgets. | **DISPROVEN** |
| Invent a save so Oakvale autostarts? | **No.** | **DISPROVEN** |

---

## Verdict

**New Game from a fresh profile is still empty `+90584`.**
It is the same walk already called “no-save.”

Click path:

```
Press Start  0xE5 → New Profile
Accept       0x126 → 00851920   // UI [+5]=1; not a save
New Game     15    → 0059A2DA
               [ui+28] vtbl+16 00430340  [retail+8]=1
               [retail+41]=1
0042F2A2     Leave frontend
0042F491     Init Game
00416953     [game+90588] empty → skip 004A3200
00416BCA     0049F180 push 0     // world+172 TRUE only
00416BF0     [game+90584] empty vs 0x122D70E
             je 00416C16         // SKIP 004B4A10
00418969     user.ini ActivateQuest("Gameflow")
```

Creating the profile does **not** fill `+90584` with
`Q_NewOakValeIntro`, does **not** fill `+90588` with a
FableSav path, and does **not** run `004B5080`.

`proofs/persist-plus189*` / `persist-plus212` /
`newgame-plus545` recover **frontend.bin** widget
persist (`CUIDef+189` / `+212` / `+545`). They are
not a hero save and not a quest list.

Do **not** invent a save.

---

## Status table

| Claim | Class | Evidence |
|---|---|---|
| `.text` lea of `game+90584` / `+90588` | **PROVEN** two sites only | both inside `00416953` **reads** |
| Writer of `+90584` intern Oakvale on New Game | **DISPROVEN** | no other `lea […+90584]` / `+90588` in listings |
| `0099B220(+90588)` empty → `jle 004169C8` | **PROVEN** | skip `"Loading save"` `004A3200` |
| `0099E960(+90584, 0x122D70E)` equal → skip `004B4A10` | **PROVEN** | `je 00416C16` |
| `004A3200` no-save inbound | **DISPROVEN** | only `004169AF` (nonempty `+90588`) and UI `0062CF30` |
| `004B5080` New Game inbound | **DISPROVEN** | `e8.tsv` dest: **`004B58F3` only** (inside `004B5500`) |
| Click New Game = `START_NEW_QUEST` / `004B5080` | **DISPROVEN** (stale `008421C0-activate`) | click is msg 15 / Leave |
| Accept `0x126` writes `+90584` | **DISPROVEN** | `00851920` body |
| `user.ini` `ActivateQuest` | **PROVEN** `"Gameflow"` only | install grep 1 hit |
| CUIDef `+189` / `+212` / `+545` on NEW_PROFILE / NEW_GAME | **PROVEN** as widget persist; **DISPROVEN** as FableSav | siblings |
| Host Notes empty `+90584` skip | **MATCH** | `GameQuestOverrideOffset=90584` |

---

## 1. Assigned persist proofs are not a save

| Proof | What it is | Quest list? |
|---|---|---|
| `persist-plus189-first` / `-newprofile` | `CUIDef+189` / `+190` u8 CRC `0xBDACBABA` / `0xAC637D43` | **No.** Type-6 load `0054ED90` only. NEW_PROFILE / NEW_GAME file u8s still **UNREAD** as widget bytes. |
| `persist-plus212` | `CUIDef+212` i32 CRC `0xCB9ADD65`; Press Start **0** skips `vtbl+520` | **No.** ACCEPT / NEW_GAME payloads **UNREAD**; still CUIDef. |
| `newgame-plus545` / `accept-newgame-plus545` | `CUIDef+545` u8 CRC `0x9E47F106`; type-11 gates `0055AD60` | **No.** File 0/1 on NEW_GAME **UNREAD**. Gate is click enable, not `004B4A10`. |

None of those CRCs is a FableSav tag. None is intern
`0x012C5D14`. Inflating them would not present Oakvale
to `00CB5AD0`.

---

## 2. Fresh profile Accept is UI, not FableSav

`UI_ACCEPT_NEW_PROFILE` persist `+228` = **`0x126`**.
`0059A238` → `00851920` (`listing-00840000.txt`):

```
00851920  sub esp, 8
          … [esi+5] already set? skip …
          call 00851890            ; profile-name CString
          call 0099B220            ; length
          jg  → [esi+5]=1, [esi+4]=0
          call 00404A80            ; eax = 0x13B7CD8 singleton
          call 00404C50            ; [that+8] then 00CB28E0
          je  ret
          push 1
          call 00404A80
          call 00404B50            ; ret 4; still that singleton
          ret
```

`00404A80` is `mov eax, 0x13B7CD8; ret`. `00404B50` /
`00404C50` read **`[ecx+8]` of that object**, not
`game+90584`. No `004B5080`. No `004A3200`. No
`0099EBF0` into `+90584`.

`00851770` seeds the type-37 edit box
(`TEXT_GUI_PROFILE_DEFAULT`). Display name only.

**DISPROVEN** as a quest-list writer.

---

## 3. New Game click does not parse `START_NEW_QUEST`

`UI_FRONTEND_BUTTON_NEW_GAME` persist `+228` = **15**.

```
0059A2C5  je  0059A2DA            ; case 15
0059A2DA  …
0059A305  mov esi, [esi+28]       ; retail
          call [eax+16]           ; 00430340  mov [ecx+8], 1
0059A30F  mov [esi+41], 1
```

Then Leave `0042F2A2` → Init Game `0042F491`.
`[retail+8]=1` does **not** rewrite `world+172`
(`proofs/q-novi-later-presenter`).

`004B5080` (`listing-00480000.txt`) is save **parse**:
`START_NEW_QUEST` / stream `009BA4A0` / `END_NEW_QUEST`.
Writer of that on-disk text is `004AF450` (`[record+4]`),
not intern `0x012C5D14`. Unique `E8`:

```
e8.tsv dest 0x004B5080
  0x004B58F3   ; inside 004B5500 START_SAVED_QUESTS
```

`calls-by-dest` grouping parent is the same over-merge.
**0** inbound from `0059A2DA` / `00851920` / Leave /
Init Game. Stale line “Click New Game is
`START_NEW_QUEST` / `004B5080`” (`008421C0-activate`)
is **DISPROVEN**.

A later **load-game** FableSav may contain a streamed
`START_NEW_QUEST` name. That is **not** New Game. Live
bytes stay **UNKNOWN** and must not be invented.

---

## 4. Empty `+90584` / `+90588` after Leave

`00416953` (`listing-00400000.txt`):

```
0041696B  lea edi, [esi+90588]
          call 0099B220
          test eax, eax
          jle  004169C8            ; empty → "Loading world"
          push "Loading save"
          call 004A3200            ; FableSav HEADER / QUESTS
…
00416BC8  push 0
00416BCA  call 0049F180            ; world+172
00416BF0  lea edi, [esi+90584]
          push 0x122D70E           ; empty intern
          call 0099E960
          je   00416C16            ; SKIP 004B4A10
00416C11  call 004B4A10            ; would be +90584 name
```

Whole `listing-*.txt` `lea …+90584` / `+90588`:
**those two reads only.** No New Profile / New Game
writer. Game alloc `push 0x161E8` at `0042F4B1` then
ctor leaves the CStrings empty (equal to `0x122D70E`).
`+90576` **does** hold `FinalAlbion.wld` (Leave /
`00415E17`). That is the WLD path, **not** a quest
override.

`004A3200` callers: `004169AF` (this nonempty arm)
and UI `0062CF30` (load/save lists). Fresh New Game
takes neither.

---

## 5. `user.ini` after that skip

TLC install `ActivateQuest` grep: **one** line.

```
ActivateQuest("Gameflow");
```

`00418969` `009EC890` → `00419CE0` → vtbl+1104
`00892E80` → `004B4A10("Gameflow",1,1)`. Not Oakvale.
`userst.ini` has **0** `ActivateQuest` and is applied
at Parse Command Line, before the command is registered.

WLD `world+172` TRUE names still omit
`Q_NewOakValeIntro` (`AddQuest FALSE` → `+184` only).

---

## Timeline (fresh profile → New Game)

```
00598A1C  Press Start attach 0xE5
0059A238  0xE5 → New Profile
00851770  seed TEXT_GUI_PROFILE_DEFAULT     // not a save
0059A238  0x126 → 00851920 [ui+96+5]=1      // not +90584
00595A06  MAIN_MENU_NO_CONTINUE
0059A238  15 → 00430340 [retail+8]=1
                 [retail+41]=1
0042F2A2  Leave
00416953  +90588 empty → Loading world      // not 004A3200
00416BCA  0049F180(0) world+172
00416BF0  +90584 empty → skip 004B4A10
00418969  user.ini Gameflow
00CE7670  wait Q_NewOakValeIntro 0x33 FOREVER
```

`004B5080` / FableSav `QUESTS` / intern `0x012C5D14`
as a presenter: **not on this walk**.

---

## What this is not

| Claim | Class |
|---|---|
| Fresh profile creates a FableSav that Init Game loads | **DISPROVEN** (empty `+90588`) |
| Accept writes `Q_NewOakValeIntro` into `+90584` | **DISPROVEN** |
| New Game click = `004B5080` | **DISPROVEN** |
| `[retail+8]=1` fills `+90584` | **DISPROVEN** |
| CUIDef `+545` / `+212` / `+189` is the quest override | **DISPROVEN** |
| `user.ini` Oakvale | **DISPROVEN** |
| Host must invent a save | **DISPROVEN** |

---

## Remaining UNKNOWN

- File u8 `0x9E47F106` on `UI_FRONTEND_BUTTON_NEW_GAME` /
  `UI_ACCEPT_NEW_PROFILE` (click enable only).
- File i32 `0xCB9ADD65` on those widgets.
- Live FableSav `START_NEW_QUEST` `[record+4]` **if a
  save is loaded later** (off New Game).

None of those is a recovered New Game presenter.

---

## Host

`GameQuestOverrideOffset=90584`.
`GameSaveNameOffset=90588`.
`LoadWorld` Notes `[+90588] empty` and
`"00416BCF +90584 empty 0122D70E skip 004B4A10"`.
`No_save_does_not_activate_Q_NewOakValeIntro`.
**MATCH.**

Do **not** add a profile-create save. Do **not** call
`004B5080` from msg 15. Do **not** invent
`ActivateQuest("Q_NewOakValeIntro")`.

---

## Sources (absolute)

- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00580000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00840000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\calls-by-dest.tsv`
- `C:\FableCSharp\proofs\persist-plus189-first\README.md`
- `C:\FableCSharp\proofs\persist-plus189-newprofile\README.md`
- `C:\FableCSharp\proofs\persist-plus212\README.md`
- `C:\FableCSharp\proofs\newgame-plus545\README.md`
- `C:\FableCSharp\proofs\accept-newgame-plus545\README.md`
- `C:\FableCSharp\proofs\ini-activate-quest\README.md`
- `C:\FableCSharp\proofs\012C5D14-fablecrc-imm\README.md`
- `C:\FableCSharp\proofs\004A1840-second-site\README.md`
- `C:\FableCSharp\proofs\0059A238-first-consumes\README.md`
