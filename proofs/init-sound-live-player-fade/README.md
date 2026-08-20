# Init Sound live player + Leave fade — exact DIVERGE

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

Question: Init Sound **live player** +
Leave **fade** **DIVERGE** — what is the
**DIVERGE** exactly? First-seen no-save
audio path vs host. `listing-00400000.txt`
Init Sound. Status.

Authority: `listing-00400000.txt`
(`0042DED5` / `0042F00A` / `0042F2A2` /
`0042F2C7`–`0042F2D8` / `0042EBB6` /
`004184BD` `0041883A`–`00418886` /
`00417A58`–`00418288`);
`listing-00980000.txt` `00991840`;
host `EngineLifecycle.cs`
(`RetailAudioEngineVa`, `RetailAudioStartVtbl`
68, `RetailAudioFadeVtbl` 72,
`LeaveFrontendAudioVtbl` / `LeaveFrontendAudioMs`,
`ApplyInitSound`, `GamePlus16=1`,
`RequestNewGameStartsMusicSet=false`,
`InitSoundPlaysMusicSet=false`);
siblings `proofs/audio-musicset-after-leave`,
`proofs/audio-initgame-first`,
`proofs/leave-first-sound`,
`proofs/00417A58-init-sound-body`,
`proofs/audio-after-leave`,
`proofs/audio-path-to-intro`,
`proofs/leave-0042F2A2-host`.

---

## Direct answer

**The DIVERGE is the missing
`[0x13B8394]` player — therefore Leave
`0042F2D8 vtbl+72(0x1F4)`.**

It is **not** Init Sound register names.
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

Host Notes the three **sites** and now
takes Init Sound’s **live** branch
(`"013B8394 live skip je 00418286"`,
`GamePlus16=1`). Host never constructs
`[0x13B8394]`, never `vtbl+68`, never
`vtbl+72(500)`. `LeaveFrontendAudioVtbl` /
`LeaveFrontendAudioMs` are leftover
declares. Live host device is AVI
`IBasicAudio` only, already dead before
Leave.

**Answer:** **DIVERGE** = **player object
+ Leave fade**. Init Sound live-gate
**decision** / register **Notes** /
`GamePlus16=1` are **MATCH** of the
post-Leave live path. `MUSIC_SET_*` is
**DISPROVEN** as this DIVERGE.

Status: **PROVEN**.

---

## Verdict

“Live player” here is **not** Create
Players, **not** `0044C6B0` `[0x13B879C]`
(def-lookup `this`), **not**
`GamePlus16` as a music player. Host
comment: `00991840(1)` → `[game+16]` is
register lookup, **not** a music player.
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
| `RequestNewGame` starts `MUSIC_SET_*` | **DISPROVEN** |
| First Present is a `MUSIC_SET_*` | **DISPROVEN** |
| AVI `IBasicAudio` is the Leave fade target | **DISPROVEN** (wrong object; graph already dead) |

`audio-musicset-after-leave` still said
Init Sound **DIVERGE register body**.
Host now Notes that body. Register
**names** closed to Note-only **MATCH**.
The leftover DIVERGE is the **same
missing player** Leave fade needs.

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
| **Evidence** | `listing-00400000.txt` `0042DED5`: `cmp [0x13B8394],0` / `je 0042DF9A`; else `call [eax+68]` (`path, 0, 0, 1.0, fade, -1`). Only `.text` `E8` after last AVI Present: `0042F004 fldz` / `0042F00A call 0042DED5`. (`audio-frontend`, `audio-after-leave`) |
| **Original** | Frontend voice **is** `[0x13B8394].vtbl+68`. Null → no-op. After that call the singleton is **live**. Path `0x1230C3C` / `0x1230C48` **UNREAD**. **Not** `MUSIC_SET_*`. |
| **Host** | `EnterFrontendAfterAvi` `Note(RetailAudioFadeFn, …, "0042DED5 0")`. No object, no `vtbl+68`. **MATCH** site. **DIVERGE** player. |
| **Gap** | Do not invent `MUSIC_SET_TITLE` / forest as the unnamed path. Do not treat AVI quartz as this player. |

### E2. Leave fade **is** that live player — exact DIVERGE

| | |
|---|---|
| **Evidence** | `0042F2C7 mov ecx,[0x13B8394]` / `cmp ecx,ebx` / `je 0042F2DB` / `push 0x1F4` / `0042F2D8 call [eax+72]`. Then `0042F48A call 0042EBB6`: `0042EBF4 cmp [esi+41],bl` / `jne 0042EC2A` skips `vtbl+64` / `vtbl+72(0)` / `00991750` / `009918F0`. (`leave-first-sound`, `leave-0042F2A2-host`) |
| **Original** | New Game (`+41!=0`) **fades 500 ms** and **keeps** the frontend voice. Quit / load (`+41==0`) **does** hard-stop. First post-Leave audio op is fade, not a start. **PROVEN.** |
| **Host** | `RequestNewGame` Notes `"0042EBB6 +41 skip audio stop"`. No `Note` of `0042F2D8`. `LeaveFrontendAudioVtbl=72` / `LeaveFrontendAudioMs=0x1F4` **unused** (0 C# call sites). **MATCH** skip-stop. **DIVERGE** fade. |
| **Gap** | Fade cannot run until a `[0x13B8394]` analog exists. Do not hard-stop AVI `IBasicAudio` as this fade. Do not start a `MUSIC_SET_*` because “we left the menu.” |

### E3. Init Sound live-gate is the **same** player — decision MATCH

| | |
|---|---|
| **Evidence** | `listing-00400000.txt` `00418886 call 00417A58` after Create Players. Body: `00417A61 mov [ecx+16],ebx` (`[game+16]=0`); `00417A64 cmp [0x13B8394],ebx`; `00417A6D je 00418286` (skip all). After Leave the cmp is **not** taken. First work `00415550` / `0044C6B0` / `004196B2`; first audio `009919C0`; tail `00418251 mov ecx,[0x13B8394]` / `push 1` / `00418259 call 00991840` / `00418263 mov [ecx+16],eax`. (`00417A58-init-sound-body`) |
| **Original** | Live player → **register**, not play. No `call [eax+68]`. No `00A01920`. No `MUSIC_SET_*`. `[game+16]` is map-find of bank id 1 (`00991840` on `audio+48`), or 0 on miss. |
| **Host** | `ApplyInitSound`: Notes `"013B8394 live skip je 00418286"` then locale / `0044C6B0` / `MAIN_SOUND_SETUP` / two `009919C0` / `00A38C20` not `00A01A4F` / `00991C10` / `"00991840(1) [game+16]"`; `GamePlus16=1`. `InitSoundPlaysMusicSet=false`. **MATCH** live-branch names. **DIVERGE** no engine, no bank pointer. |
| **Gap** | Host **claims** live so the register Notes fire. That claim is **not** a player. Leave fade still has nothing to call. Issue #15 device / mixer **STILL OPEN**. |

### E4. `GamePlus16=1` is not the fade DIVERGE

| | |
|---|---|
| **Evidence** | Native `[game+16]` starts 0, then becomes `00991840(1)` **eax** (pointer or 0). Host `int GamePlus16` is set to **1**. Tests assert `life.GamePlus16==1`. Host xml: “not a music player.” |
| **Original** | Tail store is a **registered-bank pointer**, gated on live `[0x13B8394]`. If the live skip **were** taken, `[game+16]` stays 0. |
| **Host** | Non-zero after Init Sound **MATCH**es “live path ran.” Dword value **1** vs pointer **PARTIAL** / **DIVERGE**. Unused as a fade target. |
| **Gap** | Do not treat `GamePlus16=1` as `vtbl+68` / `vtbl+72` / `MUSIC_SET_*`. |

### E5. `MUSIC_SET_*` is not this DIVERGE

| | |
|---|---|
| **Evidence** | Zero `E8` of `00CC8EAC` on Leave / `004184BD` / first type-1. Gameflow `00CE7670` state 0 parks. Host `LastMusic` empty. Flags false. (`audio-musicset-after-leave`, `audio-initgame-first`) |
| **Original** | First Present = Lookout. No script music. Leftover first `MUSIC_SET_*` is later father PC 0 **if** runner starts — **UNREAD** fire here. |
| **Host** | `RequestNewGameStartsMusicSet=false`. `InitSoundPlaysMusicSet=false`. `ScriptPlayMusicAppliesBank=false`. **MATCH** skip. |
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
   Host Notes live + `GamePlus16=1`
   without the singleton.
3. **Skip-stop `0042EBB6 +41` — MATCH.
   PROVEN.** Fade and stop are different
   ops. Host Matches the skip, not the
   fade.
4. **`MUSIC_SET_*` on New Game / first
   Present — DISPROVEN.** Not this
   DIVERGE. Do not invent one to fill
   the fade hole.
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

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\tests\Fable.Formats.Tests\EngineLifecycleTests.cs`
- `C:\FableCSharp\proofs\audio-musicset-after-leave\README.md`
- `C:\FableCSharp\proofs\audio-initgame-first\README.md`
- `C:\FableCSharp\proofs\leave-first-sound\README.md`
- `C:\FableCSharp\proofs\00417A58-init-sound-body\README.md`
- `C:\FableCSharp\proofs\audio-after-leave\README.md`
- `C:\FableCSharp\proofs\audio-path-to-intro\README.md`
- `C:\FableCSharp\proofs\leave-0042F2A2-host\README.md`
- `C:\FableCSharp\proofs\audio-frontend\README.md`
