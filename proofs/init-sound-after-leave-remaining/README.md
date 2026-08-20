# Init Sound after Leave `0042F2A2` / `FinalAlbion.wld` — remaining MATCH

Investigation only. No production `src/` or `tests/` edits.

Do **not** invent `MUSIC_SET_*` on New Game.
`RequestNewGameStartsMusicSet=false`.
`InitSoundPlaysMusicSet=false`.
`ScriptPlayMusicAppliesBank=false`.
`MUSIC_SET_*` is **never** first Present
(`proofs/audio-musicset-after-leave`).

Do **not** start at Oakvale /
`CS_OAKVALE_INTRO_FATHER` / `PlayMusic
MUSIC_SET_NULL` / `MUSIC_SET_OAKVALE`.
That path is later leftover
`Q_NewOakValeIntro`. Leave is `0042F2A2`.
Path record is `FinalAlbion.wld`
(`0042F44D`). First no-save Present is
Lookout.

Do **not** re-prove the live player object
+ Leave fade **DIVERGE**. Sibling owns
that hole (`proofs/init-sound-leave-fade`,
`proofs/init-sound-live-player-fade`). This
file is the **complement**: what Init Sound
does **after** that Leave.

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**DIVERGE** / **MATCH**.

Question: remaining Init Sound work after
Leave `0042F2A2` + `FinalAlbion.wld`, given
register Notes / `GamePlus16=1` **MATCH**
and `MUSIC_SET_*` never first Present.
What does native `00417A58` still do? Nested
`00A01A4F` symbols?

Authority: `proofs/audio-musicset-after-leave`,
`proofs/audio-initgame-first`,
`proofs/init-sound-leave-fade`,
`proofs/init-sound-live-player-fade`,
`proofs/00417A58-init-sound-body`,
`proofs/00A01A4F-sound-symbols`;
`listing-00400000.txt` (`0042F2A2` /
`0042F44D` / `0042F491` / `004184BD`
`0041883A`–`00418886` / `00417A58`–
`00418288`);
`listing-00980000.txt` (`009919C0` /
`00991840` / `00991C10`);
`listing-00a00000.txt` (`00A01950` /
`00A01A4F` / `00A38C20`);
host `EngineLifecycle.cs`
(`ApplyInitSound`, `GamePlus16=1`,
`RequestNewGameStartsMusicSet=false`,
`ScriptPlayMusicAppliesBank=false`,
`InitSoundPlaysMusicSet=false`,
`InitSoundSymbolsCompiledFn` `00A38C20`,
`InitSoundSymbolsTextFn` `00A01A4F`);
`EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`
(`GamePlus16==1`, register Note, no
`00A01A4F` event, `LastMusic` empty).

---

## Direct answer

**Remaining Init Sound after Leave is
register Notes + `GamePlus16=1` MATCH.
It is not `MUSIC_SET_*`. Live player +
Leave fade stay DIVERGE (sibling).**

After `0042F2A2` New Game records
`FinalAlbion.wld` (`0042F44D`), skips
the hard-stop quartet (`0042EBB6 +41`),
then `0042F491` Init Game. Named stage
`"Init Sound"` `00417A58` runs **after
Create Players**, **before** Load
Particles and **before** `[game].vtbl+32`
`00416953` actually loads that WLD.

On that live post-Leave path the body
**registers** (locale / main / atmos),
takes compiled symbols `00A38C20`
(**not** `00A01A4F`), and stores
`00991840(1)` at `[game+16]`. Host
`ApplyInitSound` Notes those names and
sets `GamePlus16=1`. Tests assert the
same. **MATCH.**

| Slice | Class | Owner |
|---|---|---|
| Register Notes (`009919C0` ×2 / `00991C10` / `00991840(1)`) | **MATCH** | this file |
| `GamePlus16=1` (non-zero live tail) | **MATCH** | this file |
| `RequestNewGameStartsMusicSet=false` | **MATCH** | this file |
| `ScriptPlayMusicAppliesBank=false` | **MATCH** | this file |
| `InitSoundPlaysMusicSet=false` | **MATCH** | this file |
| `MUSIC_SET_*` never first Present | **PROVEN** | `audio-musicset-after-leave` |
| Nested `00A01A4F` first-seen fire | **DISPROVEN** (`00A38C20`) | this file + `00A01A4F-sound-symbols` |
| `[0x13B8394]` player object | **DIVERGE** | sibling |
| Leave `0042F2D8 vtbl+72(500)` | **DIVERGE** | sibling |

**Answer:** after Leave / `FinalAlbion.wld`
path, Init Sound **registers**. Host Notes
that body and `GamePlus16=1`. Do not play
`MUSIC_SET_*`. Do not close the fade hole
from this stage.

Status: **PROVEN** remaining MATCH.
Sibling **DIVERGE** still open.

---

## Verdict

Leave **fades** (sibling DIVERGE). Init
Sound **registers** (this MATCH). First
Present **parks** Gameflow — **never**
`MUSIC_SET_*`.

Native first-seen no-save after
`0042F2A2`:

```
0042F2A2 Leave frontend
  0042F2D8 vtbl+72(0x1F4)          // sibling DIVERGE fade
  0042F44D "FinalAlbion.wld"       // path record; not a load
  0042EBB6 +41 skip stop           // MATCH Note
0042F491 Init Game 004184BD
  Create Players 004166A8
  00418886 call 00417A58           // THIS remaining
    live skip je 00418286          // decision MATCH; object sibling
    009919C0 / 00991C10 register   // MATCH Notes
    00A38C20 not 00A01A4F          // MATCH skip
    00991840(1) → [game+16]        // MATCH non-zero; host GamePlus16=1
  Load Particles 004174F1
  00416953 load FinalAlbion.wld    // later; not Init Sound
```

Host `RequestNewGame` Notes Leave +
`0042F44D` then `EnterGame` hits
`"Init Sound"` → `ApplyInitSound()`.
Register **names** closed. `GamePlus16=1`
closed as the live-tail flag. Device /
mixer / `[0x13B8394]` analog remain
Issue #15 / sibling fade.

```
AVI FilterGraph IBasicAudio     MATCH play; dies on unload
        ↓
0042DED5 vtbl+68                MATCH Note; DIVERGE player (sibling)
        ↓
Leave 0042F2D8 vtbl+72(500)     DIVERGE fade (sibling)
Leave 0042F44D FinalAlbion.wld  MATCH path record
Leave 0042EBB6 +41 skip stop    MATCH Note
        ↓
Init Sound 00417A58             MATCH register Notes + GamePlus16=1
                                DISPROVEN 00A01A4F first-seen
                                DISPROVEN MUSIC_SET
                                DIVERGE no engine / no bank ptr (sibling)
        ↓
00416953 load FinalAlbion.wld   later; not this stage
        ↓
first Present Lookout           MATCH park; DISPROVEN MUSIC_SET
```

---

## Evidence → Original → Host → Gap

### E1. After Leave the WLD name is `FinalAlbion.wld` — Init Sound is later, still register

| | |
|---|---|
| **Evidence** | `listing-00400000.txt`: `0042F2A2` `"Leave frontend"`; `0042F44D` push `"FinalAlbion.wld"`; `0042F48A call 0042EBB6`; `0042F491` `"Init Game"` → `00418DCA` → `[vtbl+4]` `004184BD`. Inside `004184BD`: Create Players `00418834 call 004166A8` then `"Init Sound"` `00418886 call 00417A58`. Only `.text` `E8` of `00417A58`. World load `00416953` is `[game].vtbl+32` **after** particles. (`0042F491-init-game-callees`, `initgame-after-leave-order`, `00417A58-init-sound-body`) |
| **Original** | Leave records the no-save WLD **name**. Init Sound does **not** open that file. It does **not** wait for Lookout. It registers banks because `[0x13B8394]` is live from pre-Leave `vtbl+68`. **PROVEN.** |
| **Host** | `RequestNewGame` `WorldFileName = FinalAlbionWld` + Note `0042F44D`. `EnterGame` named loop: `"Create Players"` then `"Init Sound"` `ApplyInitSound()`. `LoadWorld()` after the named loop. **MATCH** path + stage name + relative order vs Create Players. |
| **Gap** | Host still hoists world ctor (`initgame-after-leave-order`). That reorder is **not** a licence to play `MUSIC_SET_*` from Init Sound. |

Dump (`0042F2A2` New Game arm):

```
0042F2A2  push "Leave frontend"
…
0042F44D  push "FinalAlbion.wld"
0042F48A  call 0042EBB6
0042F491  push "Init Game"
…
0042F4D2  call [eax+4]             ; 004184BD
```

Dump (`004184BD` named pair):

```
00418808  push "Create Players"
00418834  call 004166A8
0041883A  push "Init Sound"
…
00418886  call 00417A58
0041888B  cmp [0x13B8648], bl
00418894  push "Load Particles"
```

### E2. Init Sound body after Leave — register Notes MATCH

| | |
|---|---|
| **Evidence** | `00417A61 mov [ecx+16],ebx`; `00417A64 cmp [0x13B8394],ebx`; `00417A6D je 00418286` **not** taken after Leave. First `E8` `"Lut register"` `0099EBF0`. First work `00415550` `ENGLISH_SOUND_SETUP`. Getter `0044C6B0` `[0x13B879C]`. Lookup `004196B2` `MAIN_SOUND_SETUP`. Localised `00417C67 call 009919C0`. Main `00417F86 call 009919C0`. Atmos `0041816A call 00991C10`. Tail `00418259 call 00991840(1)` → `[game+16]`. No `call [eax+68]`. No `00A01920`. No `00CC8EAC`. (`00417A58-init-sound-body`, `audio-initgame-first`) |
| **Original** | Live singleton → **register**, not play, not graphic-bank Open. **PROVEN.** |
| **Host** | `ApplyInitSound`: Notes live skip, locale, getter, `MAIN_SOUND_SETUP`, two `009919C0`, compiled symbols, atmos `00991C10`, `"00991840(1) [game+16]"`; `GamePlus16=1`. `InitSoundOpensBank=false`. Tests: register Note present. **MATCH** names. **DIVERGE** bind (no `audio+48` map). |
| **Gap** | Notes close the recovered **names**. They do not construct `[0x13B8394]`. Sibling fade still has nothing to call. |

Host `ApplyInitSound` (read only):

```
Note(RetailAudioEngineVa, …, "013B8394 live skip je 00418286")
Note(InitSoundLocaleFn, …, "00415550 ENGLISH_SOUND_SETUP")
Note(PlayerManagerGetter, …, "0044C6B0 [0x13B879C]")
Note(InitSoundLookupFn, …, "004196B2 MAIN_SOUND_SETUP")
Note(InitSoundRegisterFn, …, "009919C0 Registering Localised Sound Bank")
Note(InitSoundSymbolsCompiledFn, …, "00A38C20 compiled symbols not 00A01A4F")
Note(InitSoundRegisterFn, …, "009919C0 Registering Sound Bank")
Note(InitSoundAtmosRegisterFn, …, "00991C10 Registering Atmos Sound Bank")
Note(InitSoundMapLookupFn, …, "00991840(1) [game+16]")
GamePlus16 = 1
```

### E3. `GamePlus16=1` MATCH of the live tail — not a player

| | |
|---|---|
| **Evidence** | Native `[game+16]` starts 0 (`00417A61`), then `00991840(1)` **eax** (bank pointer or 0). If the live skip **were** taken, it stays 0. Host `int GamePlus16` is set to **1**. Xml: “Register lookup, not a music player.” Tests `life.GamePlus16==1`. |
| **Original** | Non-zero after Init Sound means the live register path ran. Value is a **pointer**, not the integer 1. |
| **Host** | `GamePlus16=1` **MATCH**es “live path ran / non-zero `[game+16]`.” Dword 1 vs pointer **PARTIAL**. Unused as `vtbl+68` / `vtbl+72` / ogg. |
| **Gap** | Do not treat `GamePlus16=1` as the Leave fade target or as `MUSIC_SET_*`. Remaining fade DIVERGE is the missing singleton, not this int. |

### E4. `00A01A4F` symbols — nested, not first-seen

| | |
|---|---|
| **Evidence** | `00A01A4F` is the **second** `.text` `E8` of `00A39010`, inside `00A01950` `"Sound Bank: Init Symbols"`, reached only from `009919C0` `00991A44`. `this` is heap `00A38500` at `[bank+4]`, **not** `[0x13B8A54]`. Leaf is `Data\Defs\` + SOUND_SETUP record[+8], **not** `misc_def_types.h`. TLC `UseCompiledSoundSymbols TRUE;` → `[0x13BC9F0]!=0` → `00A38C20`, skip `00A39010`. (`00A01A4F-sound-symbols`) |
| **Original** | First-seen Init Sound does **not** fire `00A01A4F`. Compiled packed reader `00A38C20` **is** taken. Exact compiled file **UNREAD**. Do not invent `atmos_types.h` / `gamesnds.bin`. |
| **Host** | Notes `"00A38C20 compiled symbols not 00A01A4F"`. Tests `DoesNotContain` `InitSoundSymbolsTextFn` (`00A01A4F`). **MATCH** skip. |
| **Gap** | Do not fill `[0x13B8A54]` from this site (wrong `this`, wrong leaf, wrong arm). |

### E5. `MUSIC_SET_*` never first Present — flags MATCH

| | |
|---|---|
| **Evidence** | Zero `E8` of `00CC8EAC` on Leave / `004184BD` / first type-1. Gameflow `00CE7670` state 0 parks on inactive `Q_NewOakValeIntro`. First Present is Lookout. Host `LastMusic` empty. (`audio-musicset-after-leave`, `audio-initgame-first`) |
| **Original** | Init Sound is not a music start. Leftover first `MUSIC_SET_*` is later father PC 0 **if** the runner starts — **UNREAD** fire here, **not** this Present. |
| **Host** | `RequestNewGameStartsMusicSet=false`. `InitSoundPlaysMusicSet=false`. `ScriptPlayMusicAppliesBank=false`. **MATCH** skip. |
| **Gap** | Closing register Notes / `GamePlus16=1` is **not** a licence to play `MUSIC_SET_*`. Closing sibling fade is not either. |

### E6. Live player + Leave fade — sibling DIVERGE (not this remaining)

| | |
|---|---|
| **Evidence** | Pre-Leave `0042F00A call 0042DED5` `[0x13B8394].vtbl+68`. Leave `0042F2C7` / `0042F2D8 call [eax+72]` push `0x1F4`. Host `HostFadesLeaveFrontendAudio=false`. `LeaveFrontendAudioVtbl` / `LeaveFrontendAudioMs` unused. (`init-sound-leave-fade`, `init-sound-live-player-fade`) |
| **Original** | Same singleton Init Sound requires non-null. New Game fades 500 ms and **keeps** it (`+41` skip stop). |
| **Host** | Skip-stop Note **MATCH**. Fade **DIVERGE**. Player object **DIVERGE**. |
| **Gap** | Sibling work. This file does not invent a mixer to fill it. Do not hard-stop AVI `IBasicAudio` as `vtbl+72(500)`. |

---

## Native Init Sound after Leave (recovered body)

Named stage `"Init Sound"` is the twelfth
`InitGameStages` entry (`0x00417A58`), after
Create Players, before Load Particles.

| Step | VA | Role | Play? | Host |
|---|---|---|---|---|
| Gate | `00417A64` `[0x13B8394]` | live after Leave → body runs | no | **MATCH** decision Note |
| First `E8` | `0099EBF0` `"Lut register"` | log | no | omitted (plumbing) |
| First work | `00415550` | `ENGLISH_SOUND_SETUP` | no | **MATCH** Note |
| Getter | `0044C6B0` `[0x13B879C]` | def-lookup `this`; **not** fade player | no | **MATCH** Note |
| Lookup | `004196B2` | `MAIN_SOUND_SETUP` | no | **MATCH** Note |
| Localised | `00417C67 call 009919C0` | `"Registering Localised Sound Bank"` | no | **MATCH** Note |
| Symbols | `00A38C20` (not `00A01A4F`) | compiled first-seen | no | **MATCH** skip Note |
| Main | `00417F86 call 009919C0` | `"Registering Sound Bank"` | no | **MATCH** Note |
| Atmos | `0041816A call 00991C10` | `"Registering Atmos Sound Bank"` | no | **MATCH** Note |
| Tail | `00418259 call 00991840(1)` | `[game+16]` | no | **MATCH** `GamePlus16=1` |

This stage **opens** no sound bank (no
`"Opening … Sound Bank"` analog of graphic
Open). Nested `00A01A4F` is a later `.text`
site of `00A39010` on a **heap** map, and
first-seen TLC **does not take it**.

---

## Host vs native (remaining table)

| Host | Native after Leave / `FinalAlbion.wld` | Class |
|---|---|---|
| `RequestNewGame` Note `0042F44D` | path record `"FinalAlbion.wld"` | **MATCH** |
| `0042EBB6 +41 skip audio stop` Note | skip quartet | **MATCH** |
| `ApplyInitSound` register Notes | `00417A58` live body | **MATCH** names; **DIVERGE** bind |
| `GamePlus16=1` | `[game+16]=00991840(1)` pointer | **MATCH** non-zero; **PARTIAL** dword |
| `Note("00A38C20 … not 00A01A4F")` | compiled arm taken | **MATCH** skip |
| no `00A01A4F` event | `je 00A01A37` not taken | **MATCH** |
| `RequestNewGameStartsMusicSet=false` | no `00CC8EAC` on tree | **MATCH** |
| `ScriptPlayMusicAppliesBank=false` | runner not on tree | **MATCH** |
| `InitSoundPlaysMusicSet=false` | no `vtbl+68` in `00417A58` | **MATCH** |
| `LastMusic` empty | first Present no `MUSIC_SET_*` | **MATCH** |
| no `[0x13B8394]` object | singleton live | **DIVERGE** (sibling) |
| no `vtbl+72(500)` | `0042F2D8` | **DIVERGE** (sibling) |
| `HostFadesLeaveFrontendAudio=false` | fade runs | **DIVERGE** (sibling) |

Tests that assert `intro.Executed[0] ==
"PlayMusic MUSIC_SET_NULL"` are **PROVEN**
leftover VM and **DISPROVEN** as first after
Leave / Init Sound / first Present.

---

## What this remaining file does **not** close

| Item | Class | Where |
|---|---|---|
| `[0x13B8394]` analog / `vtbl+68` start | **DIVERGE** | sibling live-player |
| Leave `vtbl+72(500)` | **DIVERGE** | sibling Leave fade |
| `009919C0` / `00991C10` real bind | **DIVERGE** | Issue #15 mixer |
| `00A38C20` compiled file name | **UNREAD** | `00A01A4F-sound-symbols` |
| SOUND_SETUP record[+8] leaf | **UNREAD** | same |
| First type-1 `vtbl+160` `SOUND_THEME` fire | **UNREAD** | `audio-initgame-first` |
| Who activates `Q_NewOakValeIntro` | **UNREAD** | `audio-musicset-after-leave` |

---

## Classifications (short)

1. **Register Notes after Leave —
   MATCH. PROVEN.** Host
   `ApplyInitSound` names the recovered
   `00417A58` live body. No play.
2. **`GamePlus16=1` — MATCH live tail.
   PROVEN.** Non-zero after Init Sound.
   Dword vs native pointer **PARTIAL**.
   Not a music player.
3. **`00A01A4F` first-seen — DISPROVEN.
   PROVEN skip.** Compiled `00A38C20`.
   Host Notes that skip. Different `this`
   / leaf from Init Subtitled.
4. **`MUSIC_SET_*` on New Game / Init
   Sound / first Present — DISPROVEN.**
   `RequestNewGameStartsMusicSet=false`.
   `ScriptPlayMusicAppliesBank=false`.
   Never first Present.
5. **Live player + Leave fade —
   DIVERGE. Sibling.** Not remaining
   Init Sound register work.

Do not play `MUSIC_SET_NULL`,
`MUSIC_SET_OAKVALE`, `MUSIC_SET_TITLE`,
or a Lookout / forest loop from
`RequestNewGame` / Init Sound / first
Present.

---

## Do not invent

- `MUSIC_SET_*` from `RequestNewGame` /
  Init Sound / first Present.
- `MUSIC_SET_TITLE` / forest as
  `0042DED5`.
- Host `GamePlus16=1` as a fade target
  or ogg player.
- `00A01A4F` / `misc_def_types.h` as
  first-seen Init Sound fill.
- `atmos_types.h` / `gamesnds.bin` as
  the compiled `00A38C20` file.
- Hard-stop AVI `IBasicAudio` as
  `vtbl+72(500)` (sibling fade; wrong
  object).
- FMOD / XAudio / mixer for
  `[0x13B8394]` from this MATCH close.
- Collapsing AVI quartz, frontend
  `vtbl+68`, Init Sound register, Atmos
  `vtbl+160`, and script `vtbl+2784`
  into one player.
- `ActivateQuest("Q_NewOakValeIntro")`
  from Leave / Init Sound.

---

## Sources

- `C:\FableCSharp\proofs\audio-musicset-after-leave\README.md`
- `C:\FableCSharp\proofs\audio-initgame-first\README.md`
- `C:\FableCSharp\proofs\init-sound-leave-fade\README.md`
- `C:\FableCSharp\proofs\init-sound-live-player-fade\README.md`
- `C:\FableCSharp\proofs\00417A58-init-sound-body\README.md`
- `C:\FableCSharp\proofs\00A01A4F-sound-symbols\README.md`
- `C:\FableCSharp\proofs\0042F491-init-game-callees\README.md`
- `C:\FableCSharp\proofs\initgame-after-leave-order\README.md`
- `C:\FableCSharp\proofs\leave-0042F2A2-host\README.md`
- `C:\FableCSharp\proofs\leave-first-sound\README.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\tests\Fable.Formats.Tests\EngineLifecycleTests.cs`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00a00000.txt`
