# First in-game audio after Leave vs frontend `0042DED5`

Investigation only. No production `src` edits.

Do **not** start at Oakvale / `CS_OAKVALE_INTRO_FATHER` /
`PlayMusic MUSIC_SET_NULL` / `MUSIC_SET_OAKVALE`. That path
is later leftover `Q_NewOakValeIntro` (`00DABAC0` →
`00DB86B0` → `00CBFB7D`). Leave is `0042F2A2`. First
no-save type-1 pump does not enter the runner.

Do **not** treat frontend `0042DED5` / `0x1230C3C` /
`0x1230C48` as the first *in-game* start. That is
pre-Leave `vtbl+68`. Track name is **UNREAD**. See
`proofs/audio-frontend/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: `docs/runtime/FORWARD_TREE.md` §§4–11;
`docs/PARITY.md` Leave / type-1 after `00640320`;
`proofs/audio-frontend/README.md`;
`proofs/script-playmusic/README.md`;
`proofs/dialogue-first/README.md`;
`proofs/script-opcode-after-leave/README.md`;
`proofs/script-bank-open/README.md`;
`EngineLifecycle.cs` (`RetailAudioFadeFn`,
`LeaveFrontendAudioVtbl` / `LeaveFrontendAudioMs`,
`InitGameStages` `"Init Sound"`, `TickAtmos`);
`EngineLifecycleTests`
(`Retail_0042EC7C_after_AVI_clears_then_inits_frontend`,
`New_game_is_leave_frontend_then_FinalAlbion_wld`,
`Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`);
listings `0042DED5` / `0042F2A2` / `0042EBB6` /
`00417A58` / `004A6550` / `006B1960` / `006B2260` /
`006B1900` / `006B1A20` / `006B1F30`;
`e8.tsv` `0042DED5`.

Siblings: `proofs/audio-frontend` (pre-Leave start),
`proofs/script-playmusic` (leftover `MUSIC_SET_*`),
`proofs/dialogue-first` (empty speech after Leave).

---

## Verdict

**Leave does not call `0042DED5`. It does not start a
new `vtbl+68` voice.**

`0042DED5` has **three** `E8` sites, all inside retail
`0042EC7C` **before** New Game Leave. After msg 15 the
same singleton `[0x13B8394]` is only **faded**
(`vtbl+72(500)`) and **kept**. Teardown skip is
`0042EBB6` `+41!=0`.

First *in-game* audio work after Leave is **not** a
second `0042DED5`:

| What | First native site | vs `0042DED5` | Class |
|---|---|---|---|
| Frontend start | `0042F00A call 0042DED5` `vtbl+68` | **is** `0042DED5` | **PROVEN** pre-Leave |
| Leave New Game | `0042F2D8 vtbl+72(0x1F4)` | fade that voice | **PROVEN** |
| Hard-stop | skipped | not a new start | **PROVEN** |
| `Init Sound` | `00417A58` banks / `009919C0` | no `vtbl+68` | **PROVEN** register |
| `Init Atmos` | `004A6550` → `006B1960` `world+36` | ctor `vtbl+144` | **PROVEN** construct |
| First type-1 tick | `004A5E7B call 006B2260` | same singleton; **not** `0042DED5` | **PROVEN** site |
| `MARKER_POSITIONAL_ATMOS` / `SOUND_THEME_RAIN` | dummy miss | no `006B1900` rain start | **PROVEN** miss |
| Script `PlayMusic` / `Play2DSound` / `Speak` | runner not on tree | not this object | **DISPROVEN** first-seen |
| First leftover script music | `PlayMusic MUSIC_SET_NULL` | later `00DB86B0` | **LEFTOVER** |

**Answer:** first-seen after Leave is **no new
`0042DED5` play**. The still-living voice is the
pre-Leave frontend track, fading 500 ms. Do not
start `MUSIC_SET_*`, forest ambience, or a second
`vtbl+68` from `RequestNewGame`.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  006286F0 ×3 PlayAVI
  009D8CF0 + 009BEEB0 black Present
  0042F00A call 0042DED5          // ONLY first start
    [0x13B8394].vtbl+68(path, 0, 0, 1.0, 0, -1)
  005952C3 UI show
  0062F800 / 0062F8B0 fade clocks
  loop 0042F041
    0042F07A 0042DED5             // only if [engine+364]!=0
    0042F1FD 0042DED5             // after attract .wmv
  msg 15 → [retail+41]=1
0042F2A2 Leave frontend           // not 0042DED5
  [0x13B8394].vtbl+72(0x1F4)      // 500 ms fade; keep voice
  0042EBB6 +41 skip
    vtbl+64 / vtbl+72(0) / 00991750 / 009918F0
  009BE420 + 009BEEB0 black Present
0042F491 Init Game 004184BD
  … Create Players
  "Init Sound" 00417A58           // register; no vtbl+68
  [game].vtbl+32 00416953
    004A6550 "Init Atmos"
      alloc 32 → 006B1960 → world+36
      MAIN_SOUND_SETUP; vtbl+144(0x8010, …)
    "Init Scripts" 006E7740
004189C2 first type-1 004A5A40
  004A5E10 inc WorldFrame         // 0 → 1 BEFORE atmos
  004A5E74 [world+36]
  004A5E7B 006B2260
    [0x13B8394]=0x01041044
    MARKER_POSITIONAL_ATMOS dummy miss
    006B1F30 WorldFrame&3 != 0 skip vtbl+184
    006B1A20 SOUND_THEME TOD gate
  006E37D0 speech-gain empty
```

`e8.tsv`: `0042DED5` callers are **only** `0042F00A` /
`0042F07A` / `0042F1FD`. **PROVEN.** None after
`0042F2A2`.

---

## 1. Frontend `0042DED5` is pre-Leave `vtbl+68`

| Claim | Class | Evidence |
|---|---|---|
| Who starts frontend audio | **PROVEN** | After last AVI, `0042F004 fldz` / `0042F00A call 0042DED5`. Singleton `[0x13B8394]`. Null → `je 0042DF9A`. |
| How it starts | **PROVEN** | `0041E5F2` → vtbl+168 → `+0x1CC`; `0099B6B0(0x1230C48)` + `0099B6B0(0x1230C3C)`; `0099C1E0`; `[0x13B8394].vtbl+68(path, 0, 0, 1.0, fade, -1)`. |
| Track / file name | **UNREAD** | `0x1230C3C` / `0x1230C48` not in `strings.tsv`. Do not invent `MUSIC_SET_TITLE` / forest. |
| That call is `PlayMusic` | **DISPROVEN** | Different object. No `00CC8EAC`. |
| Other `0042DED5` sites | **PROVEN** frontend-only | `0042F07A` engine+364 retrigger; `0042F1FD` after attract `006286F0`. Both still in `0042EC7C`. |
| `0042DED5` after Leave | **DISPROVEN** | zero `E8` past `0042F2A2`. |

See `proofs/audio-frontend/README.md`. This note does not
rename the file.

`[0x13B8394]` is seeded at `004022B8` from
`009A4EC0()+124` (engine audio). Same pointer later
gates `Init Sound` and `006B2260`. **PROVEN.**

---

## 2. Leave fades that voice; it does not start another

`0042F2C7`:

```
mov ecx, [0x13B8394]
cmp ecx, ebx
je  0042F2DB
mov eax, [ecx]
push 0x1F4
call [eax+72]
```

`LeaveFrontendAudioVtbl=72`, `LeaveFrontendAudioMs=0x1F4`.
**PROVEN.**

`0042EBB6` `cmp [esi+41],bl; jne 0042EC2A` skips:

- `vtbl+64`
- `vtbl+72(0)`
- `00991750`
- `009918F0`

Then `009BE420` + `009BEEB0` still run. **PROVEN.**

Quit / load (`+41==0`) **does** take the stop path.
New Game must not. Named meaning of those four stops
is **PARTIAL** (not recovered as `StopMusic`).

Host `RequestNewGame` notes `0042EBB6 +41 skip audio
stop`. It does **not** call `vtbl+72(500)`. No
`[0x13B8394]` analog. **PARTIAL** host.

---

## 3. `Init Sound` `00417A58` registers; it does not play

After Leave, `004184BD` named stage `"Init Sound"`
`00417A58` (after Create Players, before Load Particles
and before `00416953`).

| Step | VA | Play? | Class |
|---|---|---|---|
| Gate | `[0x13B8394]==0` → `je 00418286` whole skip | no | **PROVEN** |
| Localised banks | `"Init Localised Sound Bank Entries"` / `"Registering Localised Sound Bank"` → `009919C0` | no | **PROVEN** register |
| `MAIN_SOUND_SETUP` | `0044C6B0` / `004196B2` | no | **PROVEN** lookup |
| `"Init Sound Bank Entries"` | same `009919C0` / `00991840` | no | **PROVEN** register |
| `"Registering Atmos Sound Bank"` | `00991C10` then `00991840` + `vtbl+12` | no | **PROVEN** register |
| Tail | `00991840(1)` → `[game+16]` | no | **PROVEN** |

No `call [eax+68]`. Not `0042DED5`. **PROVEN.**

---

## 4. `Init Atmos` is a new object on the same singleton

Loading world `00416953` → `004A6550`:

```
"Init Atmos"
"Init Atmos Processor"
alloc 32 → 006B1960 → 004AB300 world+36
"Init Scripts" …
```

Ctor `006B1960`:

- `[this]=0`, `[+8]=0` rain flag, `[+9]=0`
- `[+10]` thing handle `00A01B10`
- `[+20]=0xFFFFFFFF`
- if `[0x13B8394]`: look up `MAIN_SOUND_SETUP`, then
  `[0x13B8394].vtbl+144(0x8010, +120, +124, this+4)`

That is listener / bus setup, **not** `vtbl+68`.
**PROVEN** as construct. Payload of `+144` **PARTIAL**.

Frontend never runs `004A6550`. Sole `006B1960` from
this walk is `004A65B3`. **PROVEN.**

---

## 5. First type-1 tick `006B2260` is not `0042DED5`

`004A5A40` after `004A5E10` (WorldFrame 0→1):

```
mov ecx, [esi+36]     ; world+36 Atmos Processor
test ecx, ecx
je  004A5E80
call 006B2260
```

`esi` is the world, not the game. **PROVEN.**

`006B2260`:

```
eax = [0x13B8394]
if eax==0  ret          ; no 006B1F30 / 006B1A20
ecx = [game+36]+24      ; WorldCamera
006B2BD0  fld [cam+452]
fcomp [0x129BA3C]
  ST<=thresh → 006B2496  (stop RAIN only if [this+8]==1)
  else:
    00A01B50 [this+10]
    miss → 009AD410 "MARKER_POSITIONAL_ATMOS"
            0083D460 find in world
            [thing+60]<0 → 007E3610 "SOUND_THEME_RAIN"
            [this+8]==0 → 006B1900("RAIN","SOUND_THEME_RAIN",1.0,2000)
006B1F30
006B1A20
```

PARITY / FORWARD_TREE: dummy has **no**
`MARKER_POSITIONAL_ATMOS` instance. Host notes
`006B2260 [0x13B8394] MARKER_POSITIONAL_ATMOS`.
First-seen rain start via `006B1900` **DISPROVEN**.

`006B1900` (when later used) is
`[0x13B8394].vtbl+160(name, …)` — **not** `vtbl+68`.
**PROVEN** slot split.

`006B1F30` first-seen: `0049D870` WorldFrame is **1**,
`and 3 != 0` → `jne 006B2093`. Then
`[this+20]==0xFFFFFFFF` equals stored copy →
`je 006B2210` skips `vtbl+184` volume sets.
**PROVEN** skip.

`006B1A20` walks `SOUND_THEME` / `NIGHT` through
`006B1900` when environment `+8` is in a TOD band
(`006BB1E0` day, `006BB790` not-day). Ctor `+8` is
`[eax+180]*[0x122DC88]`. First-seen float is
**UNREAD** (`audit-worldcamera`). Lighting TOD bytes
default 0 with no `SetTimeOfDay` writer
(`2026-08-18-environment.md`). Whether first type-1
fires `vtbl+160` is **UNREAD**. Even if it does, it
is **not** `0042DED5`.

Camera gate float `+452` / `0x129BA3C` **UNREAD**.
Either arm: first-seen `[this+8]==0` so the
`006B2496` rain-stop does not play.

---

## 6. No script / speech audio on this walk

| Claim | Class | Evidence |
|---|---|---|
| `00CBFB7D` after Leave / first `004B4260` | **DISPROVEN** | `script-opcode-after-leave` |
| `PlayMusic` / `CacheMusic` / `StopMusic` / `UseTheme` | **DISPROVEN** first-seen | `script-playmusic` |
| `Play2DSound` / `PlaySound` | **DISPROVEN** first-seen | opcode in runner only |
| `Speak` / speech-gain / conversation | **DISPROVEN** / empty | `dialogue-first`; `006E37D0` empty |
| Leftover first line if father later starts | `PlayMusic MUSIC_SET_NULL` | **LEFTOVER** |

Do **not** grep `script.bin` for `MUSIC_SET_` and play
it from `RequestNewGame`.

---

## 7. C# vs native after Leave

| Host | Native | Class |
|---|---|---|
| `Note(0042DED5)` in `EnterFrontendAfterAvi` | `0042F00A` `vtbl+68` | **PROVEN** site; **DISPROVEN** player |
| `RequestNewGame` skip-stop note | `0042EBB6 +41` | **PROVEN** pairing |
| no `vtbl+72(500)` | `0042F2D8` | **DIVERGE** (nothing to fade) |
| `Note("Init Sound")` via `InitGameStages` | `00417A58` | **PROVEN** name; no bank analog |
| no `006B1960` / `world+36` object | `004A6550` | **DISPROVEN** host object |
| `TickAtmos` note only | `006B2260` dummy miss | **PROVEN** timing; no `vtbl+160` |
| `StartNewGame` `LastMusic` | leftover father | **DIVERGE** vs Leave |

---

## Classifications (short)

1. **`0042DED5` is frontend-only `vtbl+68`. PROVEN.**
   Three callers, all in `0042EC7C`. Name **UNREAD**.
2. **Leave first audio op is `vtbl+72(500)` on that
   same voice. PROVEN.** Not a new start. Stop path
   skipped when `+41!=0`.
3. **`Init Sound` / `Init Atmos` do not call
   `0042DED5`. PROVEN.** Register + ctor `vtbl+144`.
4. **First in-game tick is `006B2260`. PROVEN site,
   dummy `MARKER_POSITIONAL_ATMOS` miss. PROVEN.**
   `SOUND_THEME` `vtbl+160` first fire **UNREAD**.
5. **First leftover *script* music remains
   `PlayMusic MUSIC_SET_NULL`. LEFTOVER.** Not Leave.

Do not play a title / forest / `MUSIC_SET_*` track
from New Game because “we left the menu.” The native
voice that is still running is the unnamed
`0042DED5` fade.
