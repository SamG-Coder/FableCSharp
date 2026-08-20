# Init Sound after Leave + live player fade — host DIVERGE?

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
First no-save Present is Lookout.

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**DIVERGE** / **MATCH**.

Question: native Init Sound **after Leave**
— fade, live player. Host **DIVERGE**?

Authority: dump `listing-00400000.txt`
(`0042DED5` / `0042F00A` / `0042F2A2` /
`0042F2C7`–`0042F2D8` / `0042EBB6` /
`0042EBF4`–`0042EC2A` / `004184BD`
`0041883A`–`00418886` / `00417A58`–
`00418288`); `listing-00980000.txt`
`00991840` / `009919C0` / `00991C10`;
host `EngineLifecycle.cs`
(`RetailAudioEngineVa`, `RetailAudioStartVtbl`
68, `RetailAudioFadeVtbl` 72,
`LeaveFrontendAudioVtbl` / `LeaveFrontendAudioMs`,
`ApplyInitSound`, `GamePlus16=1`,
`RequestNewGameStartsMusicSet=false`,
`ScriptPlayMusicAppliesBank=false`);
siblings `proofs/00417A58-init-sound-body`,
`proofs/init-sound-live-player-fade`,
`proofs/audio-initgame-first`,
`proofs/audio-musicset-after-leave`,
`proofs/leave-first-sound`,
`proofs/00A01A4F-sound-symbols`.

---

## Direct answer

**Yes: host DIVERGE is the missing
`[0x13B8394]` live player — therefore
Leave `0042F2D8 vtbl+72(0x1F4)`.**

It is **not** Init Sound register Notes.
It is **not** `GamePlus16!=0`.
It is **not** `MUSIC_SET_*`.

Native first-seen no-save uses **one** live
audio engine (`RetailAudioEngineVa`
`0x013B8394`) three times:

| Use | VA | Slot | Class |
|---|---|---|---|
| Start unnamed frontend voice | `0042F00A call 0042DED5` | `vtbl+68` | **PROVEN** pre-Leave; makes it **live** |
| Fade that voice 500 ms | `0042F2D8 call [eax+72]` push `0x1F4` | `vtbl+72` | **PROVEN** first post-Leave audio op |
| Gate Init Sound body | `00417A64 cmp [0x13B8394], ebx` | non-null → **not** `je 00418286` | **PROVEN** live skip |

Host Notes the three **sites** and takes
Init Sound’s **live** branch
(`"013B8394 live skip je 00418286"`,
register Notes, `GamePlus16=1`). Host never
constructs `[0x13B8394]`, never `vtbl+68`,
never `vtbl+72(500)`. `LeaveFrontendAudioVtbl`
/ `LeaveFrontendAudioMs` are leftover
declares (0 C# call sites). Live host device
is AVI `IBasicAudio` only, already dead
before Leave.

**Answer:** **DIVERGE** = **player object
+ Leave fade**. Init Sound live-gate
**decision** / register **Notes** /
`GamePlus16=1` are **MATCH** of the
post-Leave live path. `MUSIC_SET_*` is
**DISPROVEN** as this DIVERGE.

Status: **PROVEN**.

---

## Verdict

After Leave New Game, native **fades** the
frontend voice, **keeps** the singleton
(`+41` skip stop), then Init Sound
**registers** because that singleton is
live. Host Matches the register **names**
and the live-branch **decision**. Host
DIVERGEs the **object** those ops need.

“Live player” here is **not** Create
Players, **not** `0044C6B0` `[0x13B879C]`,
**not** `GamePlus16` as a music player.
It is the **audio-engine singleton**
that `0042DED5` starts, Leave fades, and
Init Sound requires non-null.

| Claim | Class |
|---|---|
| Native `[0x13B8394]` is live after frontend `vtbl+68` | **PROVEN** |
| Native Leave New Game fades that live player `vtbl+72(500)` then **keeps** it (`+41` skip stop) | **PROVEN** |
| Native Init Sound sees live → register; `[game+16]=00991840(1)` | **PROVEN** |
| Host `ApplyInitSound` Notes live skip + register; `GamePlus16=1` | **MATCH** live-branch **decision** / names |
| Host has `[0x13B8394]` analog / `vtbl+68` / `vtbl+72` | **DISPROVEN** — **DIVERGE** player |
| Host `RequestNewGame` `vtbl+72(500)` | **DISPROVEN** — **DIVERGE** fade |
| Host `0042EBB6 +41 skip audio stop` Note | **MATCH** skip-stop |
| Host `GamePlus16=1` vs native bank **pointer** | **PARTIAL** (non-zero **MATCH**; dword **DIVERGE**) |
| Init Sound plays `MUSIC_SET_*` | **DISPROVEN** (`InitSoundPlaysMusicSet=false`) |
| `RequestNewGame` starts `MUSIC_SET_*` | **DISPROVEN** (`RequestNewGameStartsMusicSet=false`) |
| Script `PlayMusic` applies a bank | **DISPROVEN** (`ScriptPlayMusicAppliesBank=false`) |
| First Present is a `MUSIC_SET_*` | **DISPROVEN** |
| AVI `IBasicAudio` is the Leave fade target | **DISPROVEN** (wrong object; graph already dead) |
| Nested `00A01A4F` fires first-seen | **DISPROVEN** (`UseCompiledSoundSymbols` → `00A38C20`) |

```
AVI FilterGraph IBasicAudio     MATCH play; dies on unload
        ↓
0042DED5 vtbl+68                MATCH Note; DIVERGE no [0x13B8394]
        ↓
Leave 0042F2D8 vtbl+72(500)     DIVERGE fade (nothing to fade)
Leave 0042EBB6 +41 skip stop    MATCH Note
        ↓
Init Sound 00417A58 live        MATCH Notes + GamePlus16=1
                                DIVERGE no engine / no bank ptr
        ↓
first Present Lookout           MATCH park; DISPROVEN MUSIC_SET
```

---

## Evidence → Original → Host → Gap

### E1. One singleton is the live player

| | |
|---|---|
| **Evidence** | `listing-00400000.txt` `0042DED5`: `cmp [0x13B8394],0` / `je 0042DF9A`; else `call [eax+68]` (`path, 0, 0, 1.0, fade, -1`). Path via `0099B6B0(0x1230C48)` + `0099B6B0(0x1230C3C)` + `0099C1E0`. Only `.text` `E8` after last AVI Present: `0042F004 fldz` / `0042F00A call 0042DED5`. (`audio-frontend`, `audio-after-leave`, `init-sound-live-player-fade`) |
| **Original** | Frontend voice **is** `[0x13B8394].vtbl+68`. Null → no-op. After that call the singleton is **live**. Path `0x1230C3C` / `0x1230C48` **UNREAD**. **Not** `MUSIC_SET_*`. |
| **Host** | `EnterFrontendAfterAvi` `Note(RetailAudioFadeFn, …, "0042DED5 0")`. No object, no `vtbl+68`. **MATCH** site. **DIVERGE** player. |
| **Gap** | Do not invent `MUSIC_SET_TITLE` / forest as the unnamed path. Do not treat AVI quartz as this player. |

### E2. Leave fade **is** that live player — exact DIVERGE

| | |
|---|---|
| **Evidence** | `0042F2A2` push `"Leave frontend"`. Then `0042F2C7 mov ecx,[0x13B8394]` / `cmp ecx,ebx` / `je 0042F2DB` / `push 0x1F4` / `0042F2D8 call [eax+72]`. Then `0042F48A call 0042EBB6`: `0042EBF4 cmp [esi+41],bl` / `jne 0042EC2A` skips `vtbl+64` / `vtbl+72(0)` / `00991750` / `009918F0`. (`leave-first-sound`, `leave-0042F2A2-host`) |
| **Original** | New Game (`+41!=0`) **fades 500 ms** and **keeps** the frontend voice. Quit / load (`+41==0`) **does** hard-stop. First post-Leave audio op is fade, not a start. **PROVEN.** |
| **Host** | `RequestNewGame` Notes `"0042EBB6 +41 skip audio stop"`. No `Note` of `0042F2D8`. `LeaveFrontendAudioVtbl=72` / `LeaveFrontendAudioMs=0x1F4` **unused**. **MATCH** skip-stop. **DIVERGE** fade. |
| **Gap** | Fade cannot run until a `[0x13B8394]` analog exists. Do not hard-stop AVI `IBasicAudio` as this fade. Do not start a `MUSIC_SET_*` because “we left the menu.” |

Dump (`listing-00400000.txt`):

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

`0042EBB6` New Game skip:

```
0042EBF4  cmp [esi+41], bl
0042EBF7  jne 0042EC2A          ; New Game: skip stop
0042EBF9  mov ecx, [0x13B8394]
          call [eax+64]
          push ebx
          call [eax+72]         ; vtbl+72(0)
          call 00991750
          call 009918F0
0042EC2A  … 009BE420 + 009BEEB0
```

### E3. Init Sound after Leave — live gate then register

| | |
|---|---|
| **Evidence** | `listing-00400000.txt` `00418886 call 00417A58` after Create Players (`00418834 call 004166A8`). Only `.text` `E8` of `00417A58`. Body: `00417A61 mov [ecx+16],ebx` (`[game+16]=0`); `00417A64 cmp [0x13B8394],ebx`; `00417A6D je 00418286` (skip all). After Leave the cmp is **not** taken. First work `00415550` / `0044C6B0` / `004196B2`; first audio `00417C67 call 009919C0`; main `00417F86 call 009919C0`; atmos `0041816A call 00991C10`; tail `00418251 mov ecx,[0x13B8394]` / `push 1` / `00418259 call 00991840` / `00418263 mov [ecx+16],eax`. (`00417A58-init-sound-body`) |
| **Original** | Live player → **register**, not play. No `call [eax+68]`. No `00A01920`. No `MUSIC_SET_*`. `[game+16]` is map-find of bank id 1 (`00991840` on `audio+48`), or 0 on miss. Nested `"Sound Bank: Init Symbols"` `00A01A4F` is **DISPROVEN** first-seen fire (`UseCompiledSoundSymbols` TRUE → `00A38C20`). |
| **Host** | `EnterGame` `if (name == "Init Sound") ApplyInitSound()`. Notes `"013B8394 live skip je 00418286"` then locale / `0044C6B0` / `MAIN_SOUND_SETUP` / two `009919C0` / `00A38C20` not `00A01A4F` / `00991C10` / `"00991840(1) [game+16]"`; `GamePlus16=1`. Tests assert `life.GamePlus16==1` and register Note present, no `00A01A4F` event. `InitSoundPlaysMusicSet=false`. **MATCH** live-branch names. **DIVERGE** no engine, no bank pointer. |
| **Gap** | Host **claims** live so the register Notes fire. That claim is **not** a player. Leave fade still has nothing to call. |

Dump (`00417A58` first-seen after Leave):

```
00417A58  push ebp
00417A5B  sub esp, 124
00417A61  mov [ecx+16], ebx          ; [game+16]=0
00417A64  cmp [0x13B8394], ebx
00417A6A  mov [ebp-124], ecx
00417A6D  je  00418286               ; NOT taken after Leave
00417A77  push "Lut register"
00417A7F  call 0099EBF0              ; FIRST E8
00417AA1  call 00415550              ; ENGLISH_SOUND_SETUP
00417AA7  call 0044C6B0              ; [0x13B879C] getter
00417AAE  call 004196B2              ; MAIN_SOUND_SETUP
…
00417C67  call 009919C0              ; Registering Localised Sound Bank
…
00417F86  call 009919C0              ; Registering Sound Bank
…
0041816A  call 00991C10              ; Registering Atmos Sound Bank
…
00418251  mov ecx, [0x13B8394]
00418257  push 1
00418259  call 00991840
00418263  mov [ecx+16], eax          ; [game+16]
```

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

### E4. `GamePlus16=1` is not the fade DIVERGE

| | |
|---|---|
| **Evidence** | Native `[game+16]` starts 0, then becomes `00991840(1)` **eax** (pointer or 0). Host `int GamePlus16` is set to **1**. Tests assert `life.GamePlus16==1`. Host xml: “not a music player.” |
| **Original** | Tail store is a **registered-bank pointer**, gated on live `[0x13B8394]`. If the live skip **were** taken, `[game+16]` stays 0. |
| **Host** | Non-zero after Init Sound **MATCH**es “live path ran.” Dword value **1** vs pointer **PARTIAL** / **DIVERGE**. Unused as a fade target. |
| **Gap** | Do not treat `GamePlus16=1` as `vtbl+68` / `vtbl+72` / `MUSIC_SET_*`. |

### E5. `MUSIC_SET_*` is not this DIVERGE — never first Present

| | |
|---|---|
| **Evidence** | Zero `E8` of `00CC8EAC` on Leave / `004184BD` / first type-1. Gameflow `00CE7670` state 0 parks. Host `LastMusic` empty. Flags false. (`audio-musicset-after-leave`, `audio-initgame-first`) |
| **Original** | First Present = Lookout. No script music. Leftover first `MUSIC_SET_*` is later father PC 0 **if** runner starts — **UNREAD** fire here. |
| **Host** | `RequestNewGameStartsMusicSet=false`. `InitSoundPlaysMusicSet=false`. `ScriptPlayMusicAppliesBank=false`. `EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`: `LastMusic` empty; `GamePlus16==1`; register Note; no `00A01A4F` event. **MATCH** skip. |
| **Gap** | Closing the player/fade DIVERGE is **not** a licence to play `MUSIC_SET_*`. |

---

## Timeline (no-save New Game)

```
0042EC7C retail
  006286F0 ×3 PlayAVI
    00A3B9D0 FilterGraph + IBasicAudio     // ONLY live host device
    00A3B380 / 00A3BC20 unload             // dead before Leave
  009D8CF0 + 009BEEB0 black Present
  0042F00A call 0042DED5                   // native: [0x13B8394].vtbl+68
                                           // host: Note only  — DIVERGE player
  005952C3 UI show
  loop 0042F041
  msg 15 → [retail+41]=1
0042F2A2 Leave frontend
  0042F2C7 mov ecx, [0x13B8394]
  0042F2D8 call [eax+72]  ; 0x1F4         // native fade
                                           // host: no call     — DIVERGE fade
  0042EBB6 +41 skip
    vtbl+64 / vtbl+72(0) / 00991750 / 009918F0
                                           // host: Note skip   — MATCH
  009BE420 + 009BEEB0 black Present
0042F491 Init Game 004184BD
  Create Players 004166A8
  0041883A / 0041885A "Init Sound"
  00418886 call 00417A58
    00417A61 [game+16]=0
    00417A64 cmp [0x13B8394]               // native live
    00417A6D je 00418286                   // NOT taken
    00415550 ENGLISH_SOUND_SETUP
    0044C6B0 [0x13B879C]                   // player-manager getter, not the fade player
    004196B2 MAIN_SOUND_SETUP
    009919C0 / 00991C10 register
    00A38C20 compiled symbols              // not 00A01A4F
    00418259 00991840(1) → [game+16]
                                           // host: Notes + GamePlus16=1
                                           // MATCH decision; DIVERGE object
004189C2 first type-1
  first Present Lookout                    // DISPROVEN MUSIC_SET
```

---

## Host vs native (one table)

| Host | Native | Class |
|---|---|---|
| `Note(0042DED5 0)` | `0042F00A vtbl+68` | **MATCH** site; **DIVERGE** player |
| no `[0x13B8394]` object | singleton live through Leave / Init Sound | **DIVERGE** player |
| `0042EBB6 +41 skip audio stop` Note | skip quartet | **MATCH** |
| no `vtbl+72(500)` | `0042F2D8` | **DIVERGE** fade |
| `LeaveFrontendAudioVtbl` / `Ms` unused | same constants | **LEFTOVER** declare |
| `Note("013B8394 live skip je 00418286")` | `00417A6D` not taken | **MATCH** decision |
| register Notes `009919C0` / `00991C10` / `00991840(1)` | same callees | **MATCH** names; **DIVERGE** bind |
| `GamePlus16=1` | `[game+16]=00991840(1)` pointer | **MATCH** non-zero; **PARTIAL** dword |
| `InitSoundPlaysMusicSet=false` | no `vtbl+68` / no `00CC8EAC` in `00417A58` | **MATCH** |
| `RequestNewGameStartsMusicSet=false` | no `PlayMusic` on Leave | **MATCH** |
| `ScriptPlayMusicAppliesBank=false` | runner not on tree | **MATCH** |
| AVI `IBasicAudio` | `00A3B9D0` then unload | **MATCH**; **DISPROVEN** as fade target |
| `LastMusic` empty | first Present no `MUSIC_SET_*` | **MATCH** |
| no `00A01A4F` event | compiled `00A38C20` | **MATCH** skip |

---

## Native Init Sound after Leave (recovered body)

Named stage `"Init Sound"` is the twelfth
`InitGameStages` entry (`0x00417A58`), after
Create Players, before Load Particles.

| Step | VA | Role | Play? |
|---|---|---|---|
| Gate | `00417A64` `[0x13B8394]` | live after Leave → body runs | no |
| First `E8` | `0099EBF0` `"Lut register"` | log | no |
| First work | `00415550` | `ENGLISH_SOUND_SETUP` | no |
| Getter | `0044C6B0` `[0x13B879C]` | def-lookup `this` | no |
| Lookup | `004196B2` | `MAIN_SOUND_SETUP` | no |
| Localised | `00417C67 call 009919C0` | `"Registering Localised Sound Bank"` | no |
| Symbols | `00A38C20` (not `00A01A4F`) | compiled first-seen | no |
| Main | `00417F86 call 009919C0` | `"Registering Sound Bank"` | no |
| Atmos | `0041816A call 00991C10` | `"Registering Atmos Sound Bank"` | no |
| Tail | `00418259 call 00991840(1)` | `[game+16]` | no |

This stage **opens** no sound bank (no
`"Opening … Sound Bank"` analog of graphic
Open). Nested `00A01A4F` is a later `.text`
site of `00A39010` on a **heap** map, not
`[0x13B8A54]`, and first-seen TLC **does not
take it**. (`00A01A4F-sound-symbols`)

---

## What “live player” is **not**

| Candidate | Class |
|---|---|
| Create Players Thing / hero | **DISPROVEN** — different stage `004166A8` |
| `0044C6B0` `[0x13B879C]` | **PROVEN** getter used as `004196B2` `this`; **DISPROVEN** as fade target |
| `GamePlus16` as `vtbl+68` / ogg player | **DISPROVEN** |
| Script `[0x143E8F8].vtbl+2784` | **DISPROVEN** first-seen; leftover `PlayMusic` |
| Atmos `vtbl+160` `SOUND_THEME` | later `006B2260`; fire **UNREAD**; not Leave fade |
| AVI FilterGraph `IBasicAudio` | **DISPROVEN** Leave target |

---

## Classifications (short)

1. **Exact DIVERGE — PROVEN.** Missing
   `[0x13B8394]` player, therefore Leave
   `0042F2D8 vtbl+72(500)` does not run.
2. **Init Sound live skip — MATCH
   decision, DIVERGE object. PROVEN.**
   Host Notes live + register +
   `GamePlus16=1` without the singleton.
3. **Skip-stop `0042EBB6 +41` — MATCH.
   PROVEN.** Fade and stop are different
   ops. Host Matches the skip, not the
   fade.
4. **`MUSIC_SET_*` on New Game / first
   Present — DISPROVEN.** Not this
   DIVERGE. `RequestNewGameStartsMusicSet=false`.
   `ScriptPlayMusicAppliesBank=false`.
   Do not invent one to fill the fade hole.
5. **`GamePlus16=1` vs bank pointer —
   PARTIAL.** Non-zero MATCH of live
   tail; not a player.

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
- Hard-stop AVI `IBasicAudio` as
  `vtbl+72(500)`.
- FMOD / XAudio / mixer for
  `[0x13B8394]`.
- Collapsing AVI quartz, frontend
  `vtbl+68`, Init Sound register, Atmos
  `vtbl+160`, and script `vtbl+2784`
  into one player.
- `00A01A4F` / `misc_def_types.h` as
  first-seen Init Sound fill.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\tests\Fable.Formats.Tests\EngineLifecycleTests.cs`
- `C:\FableCSharp\proofs\00417A58-init-sound-body\README.md`
- `C:\FableCSharp\proofs\init-sound-live-player-fade\README.md`
- `C:\FableCSharp\proofs\audio-initgame-first\README.md`
- `C:\FableCSharp\proofs\audio-musicset-after-leave\README.md`
- `C:\FableCSharp\proofs\leave-first-sound\README.md`
- `C:\FableCSharp\proofs\00A01A4F-sound-symbols\README.md`
- `C:\FableCSharp\proofs\audio-after-leave\README.md`
- `C:\FableCSharp\proofs\leave-0042F2A2-host\README.md`
