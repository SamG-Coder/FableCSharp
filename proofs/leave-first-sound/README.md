# First audio after Leave / Init Game — first `SND_*` / music VA?

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `CS_OAKVALE_INTRO_FATHER` /
`PlayMusic MUSIC_SET_NULL` / `MUSIC_SET_OAKVALE`. That
path is later leftover `Q_NewOakValeIntro` (`00DABAC0`
→ `00DB86B0` → `00CBFB7D`). Leave is `0042F2A2`. First
no-save type-1 pump does not enter the runner.

Do **not** treat frontend `0042DED5` / `0x1230C3C` /
`0x1230C48` as an Init Game start. That is pre-Leave
`vtbl+68`. Track name is **UNREAD**.

Do **not** treat Press Start / New Game click as
`00A01920` / `SND_MENU_04` / `UI_CLICK`. Frontend
click leftover is **DISPROVEN**.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Authority: `proofs/audio-after-leave/README.md`;
`proofs/audio-initgame-first/README.md`.
Also: `proofs/audio-frontend/README.md` §2;
`proofs/script-playmusic/README.md`;
`docs/runtime/FORWARD_TREE.md` §§4–11;
`docs/PARITY.md` Leave / type-1 after `00640320`;
`e8.tsv` dest `0042DED5` (3) / `00A01920` (22) /
`006B2260` (1: `004A5E7B`) / `006B1960` (1: `004A65B3`) /
`00417A58` (1: `00418886`);
listings `0042F2A2` / `00A01920` / `006B1900` /
`006B1A20` / `006B2260`;
`EngineLifecycle.cs` (`LeaveFrontendAudioVtbl` /
`LeaveFrontendAudioMs`, `RetailAudioFadeFn`,
`InitGameStages` `"Init Sound"`, `TickAtmos`).

Siblings: `proofs/audio-after-leave` (no second
`0042DED5`), `proofs/audio-initgame-first` (no
`00A01920` / `SND_*`), `proofs/audio-frontend`
(click **DISPROVEN**), `proofs/script-playmusic`
(leftover `MUSIC_SET_*`).

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| First *audio op* after Leave? | `0042F2D8` `[0x13B8394].vtbl+72(0x1F4)` — 500 ms **fade** of the pre-Leave frontend voice. Not a new start. | **PROVEN** |
| Does Leave / Init Game play a click? | **No.** `0059A238` msg `0xE5` / `0x126` / `15` has no `00A01920`, no `SND_*`, no `[0x13B8394]`. | **DISPROVEN** |
| First `SND_*` play VA? | **none** on Leave / `004184BD` / first type-1. | **DISPROVEN** first-seen |
| First `MUSIC_SET_*` / `PlayMusic` VA? | **none.** Runner `00CBFB7D` / token `00CC8EAC` not on the tree. | **DISPROVEN** first-seen |
| First `00A01920` after Leave? | **none.** 22 `E8` sites, all later / leftover. Lookup, not play. | **PROVEN** skip |
| Init Game first audio work? | `"Init Sound"` `00417A58` register (`009919C0` / `00991840`); `"Init Atmos"` `004A65B3` `006B1960` `vtbl+144`. | **PROVEN** construct |
| First in-game tick site? | `004A5E7B call 006B2260` (WorldFrame already 1). Dummy `MARKER_POSITIONAL_ATMOS` miss. | **PROVEN** |
| Candidate first *in-game start* VA? | `006B1900` → `[0x13B8394].vtbl+160` `SOUND_THEME` from `006B1A20`. Fire first-seen **UNREAD**. Not `SND_*`, not `MUSIC_SET_*`. | **UNREAD** fire |
| Still-living voice? | Unnamed `0042F00A call 0042DED5` `vtbl+68`. Three `E8` only, all in `0042EC7C` **before** Leave. | **PROVEN** pre-Leave |

**Answer:** first-seen after Leave / Init Game is **no
new `SND_*`, no `MUSIC_SET_*`, no `00A01920`, no
second `0042DED5`.** Frontend click is **DISPROVEN**.
The voice still running is the unnamed `0042DED5`
fade (`vtbl+72(500)`). Do not play `SND_MENU_04` or a
title track from `RequestNewGame`.

---

## Verdict

Leave **fades**. Init Game **registers**. First type-1
**ticks Atmos**. Nothing on that tree starts a named
`SND_*` or a script music set.

| What | First native site | `SND_*` / music? | Class |
|---|---|---|---|
| Frontend start | `0042F00A call 0042DED5` `vtbl+68` | unnamed path **UNREAD** | **PROVEN** pre-Leave |
| Frontend click / `SND_MENU_04` | `0059A238` has none | no | **DISPROVEN** leftover |
| Leave New Game | `0042F2D8 vtbl+72(0x1F4)` | fade that voice | **PROVEN** |
| Hard-stop quartet | skipped `0042EBB6 +41!=0` | not a start | **PROVEN** |
| `Init Sound` | `00417A58` | register | **PROVEN** |
| `Init Atmos` | `004A65B3` `006B1960` `vtbl+144` | construct | **PROVEN** |
| First type-1 | `004A5E7B` `006B2260` | dummy rain miss | **PROVEN** |
| First `SND_*` play | — | — | **DISPROVEN** first-seen |
| First `MUSIC_SET_*` | runner not on tree | — | **DISPROVEN** first-seen |
| Leftover first script music | `PlayMusic MUSIC_SET_NULL` `00CC8EAC` | later `00DB86B0` | **LEFTOVER** |
| Candidate in-game start | `006B1A20` → `006B1900` `vtbl+160` | `SOUND_THEME` / `NIGHT` | **UNREAD** fire |

---

## Timeline (no-save New Game)

```
0042EC7C retail
  006286F0 ×3 PlayAVI
  0042F00A call 0042DED5          // ONLY vtbl+68 start
    [0x13B8394].vtbl+68(path, 0, 0, 1.0, 0, -1)
  005952C3 UI show
  loop 0042F041
    0059A238 UI vtbl+32
      0xE5 / 0x126 / 15           // no 00A01920, no SND_*
  msg 15 → [retail+41]=1
0042F2A2 Leave frontend           // not 0042DED5, not 00A01920
  0042F2D8 vtbl+72(0x1F4)         // FIRST post-Leave audio op
  0042EBB6 +41 skip
    vtbl+64 / vtbl+72(0) / 00991750 / 009918F0
0042F491 Init Game 004184BD
  "Init Subtitled Message" 004CDB10   // 00A39010; no 00A01920
  Init World 004A6E30
  Create Players 004166A8
  "Init Sound" 00417A58               // register; no vtbl+68
  [game].vtbl+32 00416953
    004A6550 "Init Atmos"
      004A65B3 call 006B1960          // world+36; vtbl+144
    "Init Scripts" 006E7740
    0049F180 "Init GUI" 0043A380      // reset; no SND_
004189C2 first type-1 004A5A40
  004A5E10 inc WorldFrame             // 0 → 1 BEFORE atmos
  004A5E7B call 006B2260              // sole E8 of 006B2260
    MARKER_POSITIONAL_ATMOS dummy miss
    006B1F30 WorldFrame&3 != 0 skip vtbl+184
    006B1A20 SOUND_THEME TOD gate     // not 00A01920
```

`e8.tsv`: `0042DED5` = `0042F00A` / `0042F07A` /
`0042F1FD` only — all inside `0042EC7C`. **PROVEN**
none after `0042F2A2`.

`e8.tsv`: **zero** of 22 `00A01920` sites in
`004184BD` / `00417A58` / `004CDB10` / `004A6E30` /
`004A6550` / `006B1960` / `006B2260` / `0043A380` /
`004189C2` / `004A5A40`. **PROVEN.**

---

## 1. Frontend click leftover is DISPROVEN

Authority: `proofs/audio-frontend/README.md` §2;
`proofs/audio-initgame-first/README.md` §2.

| Claim | Class | Evidence |
|---|---|---|
| `0059A238` plays a click | **DISPROVEN** (first-seen) | Msg `0xE5` → `00599D5C`; `0x126` → `00851920`; `15` → `0059A2DA` `[retail+41]=1`. No `00A01920`, no `SND_*`, no `[0x13B8394]`. |
| `Play2DSound UI_CLICK` on frontend | **DISPROVEN** | Opcode `00CBF89E` / `00CBF8DA` only. Not UI vtbl+32. |
| `SND_MENU_04` is Press Start / New Game | **DISPROVEN** | `005F64DD` / `005F6793` → `00A01920` then `vtbl+36` → `vtbl+12`. In-game EXP GUI. `e8.tsv` of `005F64DD`: `005F6A54` / `005F6B2C` / `005F6BC4` / `005F6E34` / `006901CB` / `006901EB`. None in `004184BD`. |
| `00A01920` is the play | **DISPROVEN** | `listing-00a00000.txt` `00A01920`–`00A0193F`: `[this+4]` → `00A38420` hash-find; ret id or 0. No `[0x13B8394]`. Play wrapper is later `0041CEB3` (no `.text` `E8` of that owner). |

Do **not** replay `SND_MENU_04` / `UI_CLICK` because
New Game left the menu.

---

## 2. First post-Leave op is fade, not a start

`listing-00400000.txt`:

```
0042F2A2  push "Leave frontend"
…
0042F2C7  mov ecx, [0x13B8394]
0042F2CD  cmp ecx, ebx
0042F2CF  je  0042F2DB
0042F2D1  mov eax, [ecx]
0042F2D3  push 0x1F4
0042F2D8  call [eax+72]
```

`LeaveFrontendAudioVtbl=72`, `LeaveFrontendAudioMs=0x1F4`.
**PROVEN.**

`0042EBB6` `+41!=0` skips `vtbl+64` / `vtbl+72(0)` /
`00991750` / `009918F0`. Quit / load (`+41==0`) **does**
stop. New Game must not.

No new `vtbl+68`. **PROVEN** (`audio-after-leave`).

---

## 3. Init Game does not play `SND_*` / music

`"Init Sound"` `00417A58` (`e8` only `00418886`):
localised / main / atmos bank register via `009919C0` /
`00991840`. Gate `[0x13B8394]==0` → whole skip. No
`call [eax+68]`. No `00A01920`. **PROVEN** register.

`"Init Atmos"` `004A65B3 call 006B1960` (sole `E8`):
alloc 32 → `world+36`; ctor then
`[0x13B8394].vtbl+144(0x8010, …)`. Listener / bus.
**PROVEN** construct. Payload of `+144` **PARTIAL**.

`"Init Subtitled Message"` `004CDB10` is `00A39010`.
Not `00A01920`. `"Init GUI"` `0043A380` reset. No
`SND_*`. **PROVEN.**

---

## 4. First in-game tick is `006B2260`, not a `SND_` VA

Sole `E8` of `006B2260` is `004A5E7B` after
`004A5E10` (WorldFrame 0→1).

`006B2260` (`listing-00680000.txt`):

```
006B2260  mov eax, [0x13B8394]
          test eax, eax
          je  006B24FA
          … camera +452 vs 0x129BA3C
          miss → "MARKER_POSITIONAL_ATMOS"
          dummy has no instance → no 006B1900 RAIN
006B1F30  WorldFrame&3 != 0 → skip vtbl+184
006B1A20  SOUND_THEME TOD gate
```

First-seen rain `006B1900("RAIN","SOUND_THEME_RAIN",…)`
**DISPROVEN**.

`006B1900` is `[0x13B8394].vtbl+160` — **not**
`vtbl+68`, **not** `00A01920`. **PROVEN** slot split.

`006B1A20` pushes `"SOUND_THEME"` then may
`call 006B1900` (`006B1B1C` / `006B1B73` / …). Ctor
`[this]=0`, `[+9]=0`. First-seen `env+8` float
**UNREAD**. Whether first type-1 fires `vtbl+160` is
**UNREAD**. Even if it fires, the name is
`SOUND_THEME` / `NIGHT`, not `SND_*`, not
`MUSIC_SET_*`.

---

## 5. First `SND_*` / music VA — none on this tree

| Claim | VA | Class |
|---|---|---|
| First `push "SND_…"` on Leave / Init Game / first type-1 | none | **DISPROVEN** first-seen |
| Leftover named `SND_*` if HUD later plays | `005F64DD` `"SND_MENU_04"` `0x01256F54` | **LEFTOVER** vs this walk |
| `PlayMusic` / `MUSIC_SET_*` after Leave | none (`00CC8EAC` not on tree) | **DISPROVEN** first-seen |
| Leftover first *script* music | `00DB86B0` → `00CBFB7D` `PlayMusic MUSIC_SET_NULL` | **LEFTOVER** |
| Leftover *named* set after that | `MUSIC_SET_OAKVALE` (same def, later PC) | **LEFTOVER** |
| Frontend `0042DED5` as `MUSIC_SET_*` | different object; `0x1230C3C` / `0x1230C48` not in `strings.tsv` | **DISPROVEN** |

`MUSIC_SET_*` is **not** in `strings.tsv` (script.bin
tokens). Do **not** grep `SND_` / `MUSIC_SET_` and
play the first hit from `RequestNewGame`.

If a later implementer needs *one* in-game start VA
to instrument, it is **`006B1900`**, gated by
`006B1A20`, first reached from **`006B2260`**. That
is **not** proven to fire on first type-1.

---

## 6. C# vs native

| Host | Native | Class |
|---|---|---|
| `Note(0042DED5)` in `EnterFrontendAfterAvi` | `0042F00A` `vtbl+68` | **PROVEN** site; **DISPROVEN** player |
| `RequestNewGame` skip-stop note | `0042EBB6 +41` | **PROVEN** pairing |
| no `vtbl+72(500)` | `0042F2D8` | **DIVERGE** (nothing to fade) |
| `Note("Init Sound")` | `00417A58` | **PROVEN** name; no bank analog |
| no `006B1960` / `world+36` | `004A6550` | **DISPROVEN** host object |
| `TickAtmos` note only | `006B2260` dummy miss | **PROVEN** timing; no `00A01920` |
| no `SND_MENU_04` / click | none on this tree | **PROVEN** pairing |
| `StartNewGame` `LastMusic` | leftover father | **DIVERGE** vs Leave |

---

## Classifications (short)

1. **Frontend click / `SND_MENU_04` / `UI_CLICK`.
   DISPROVEN.** `0059A238` has no sound site.
2. **First post-Leave audio op is `0042F2D8`
   `vtbl+72(500)`. PROVEN.** Not a new start.
3. **Init Game: register `00417A58` then Atmos ctor
   `006B1960` `vtbl+144`. PROVEN.** No `SND_*`.
4. **First in-game tick VA is `006B2260` (`004A5E7B`).
   PROVEN.** Dummy rain miss **PROVEN**.
5. **First `SND_*` play VA — none. PROVEN.**
   First `MUSIC_SET_*` VA — none. **PROVEN.**
   Leftover script music remains `PlayMusic
   MUSIC_SET_NULL`.
6. **Candidate later start VA `006B1900` `vtbl+160`
   `SOUND_THEME`. Fire UNREAD.** Not `00A01920`.

Do not play `SND_MENU_04`, `UI_CLICK`, or
`MUSIC_SET_*` from New Game because “we left the
menu.” The native voice that is still running is the
unnamed `0042DED5` fade.
