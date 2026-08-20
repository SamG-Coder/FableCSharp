# First audio after Leave / Init Game — `00A01920`? first `SND_` / music?

Investigation only. No production `src` edits.

Do **not** start at Oakvale / `CS_OAKVALE_INTRO_FATHER` /
`PlayMusic MUSIC_SET_NULL` / `MUSIC_SET_OAKVALE`. That path
is later leftover `Q_NewOakValeIntro` (`00DABAC0` →
`00DB86B0` → `00CBFB7D`). Leave is `0042F2A2`. First
no-save type-1 pump does not enter the runner.

Do **not** treat frontend `0042DED5` / `0x1230C3C` /
`0x1230C48` as an Init Game start. That is pre-Leave
`vtbl+68`. Track name is **UNREAD**. See
`proofs/audio-frontend/README.md`.

Do **not** treat Press Start / New Game click as
`00A01920` / `SND_MENU_04` / `UI_CLICK`. Frontend click
leftover is **DISPROVEN**. See
`proofs/audio-frontend/README.md` §2.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: `docs/runtime/FORWARD_TREE.md` §§4–11;
`docs/PARITY.md` Leave / type-1 after `00640320`;
`docs/status/investigations/2026-08-18-environment.md`;
`proofs/audio-after-leave/README.md`;
`proofs/audio-frontend/README.md`;
`proofs/script-playmusic/README.md`;
`proofs/dialogue-first/README.md`;
`proofs/script-opcode-after-leave/README.md`;
`EngineLifecycle.cs` (`RetailAudioFadeFn`,
`LeaveFrontendAudioVtbl` / `LeaveFrontendAudioMs`,
`InitGameStages` `"Init Sound"`, `TickAtmos`);
listings `004184BD` / `00417A58` / `004CDB10` /
`0041CEB3` / `00A01920` / `00A38420` / `004CEA60` /
`004A6550` / `006B1960` / `006B2260` / `006B1900` /
`006B1A20` / `006BB1E0` / `006BB790` / `006BBC30` /
`005F64DD`;
`e8.tsv` `00A01920` (22 sites);
`strings.tsv` `SND_*`;
`functions.tsv` `0041CEB3` / `005F64DD` / `005F6793`.

Siblings: `proofs/audio-after-leave` (Leave fade +
`006B2260` site), `proofs/audio-frontend` (click
**DISPROVEN**), `proofs/script-playmusic` (leftover
`MUSIC_SET_*`).

---

## Verdict

**Init Game does not call `00A01920`. It does not play a
`SND_*`. It does not start `MUSIC_SET_*`.**

`00A01920` is a **31-byte bank-symbol lookup**
(`[this+4]` → `00A38420`), not a voice start. The
`SND_*` *play* that uses it is later
`[0x13B8394].vtbl+36` → `vtbl+12` (`0041CEB3` /
`005F64DD`). Frontend click leftover that blamed
`00A01920` / `SND_MENU_04` is **DISPROVEN**.

First *audio work* after Leave is still the pre-Leave
frontend voice, **faded** 500 ms (`vtbl+72(0x1F4)`).
Init Game then **registers** banks (`"Init Sound"`
`00417A58`) and **constructs** Atmos (`vtbl+144`).
First in-game tick is `006B2260` — dummy
`MARKER_POSITIONAL_ATMOS` miss, then `SOUND_THEME`
via `006B1900` `vtbl+160` (not `00A01920`, not
`SND_*`). Whether that `vtbl+160` fires first-seen
is **UNREAD**.

| What | First native site | `00A01920`? | Class |
|---|---|---|---|
| Frontend start | `0042F00A call 0042DED5` `vtbl+68` | no | **PROVEN** pre-Leave |
| Frontend click / `SND_MENU_04` | `0059A238` has none | no | **DISPROVEN** leftover |
| Leave New Game | `0042F2D8 vtbl+72(0x1F4)` | no | **PROVEN** fade |
| `Init Sound` | `00417A58` `009919C0` / `00991840` | no | **PROVEN** register |
| `Init Subtitled Message` | `004CDB10` `00A39010` | no | **PROVEN** register |
| `Init Atmos` | `004A6550` → `006B1960` `vtbl+144` | no | **PROVEN** construct |
| First type-1 | `004A5E7B call 006B2260` | no | **PROVEN** site |
| First `SND_*` play | none on this tree | — | **DISPROVEN** first-seen |
| First `MUSIC_SET_*` | runner not on tree | — | **DISPROVEN** first-seen |
| First leftover script music | `PlayMusic MUSIC_SET_NULL` | no | **LEFTOVER** |
| Candidate first *in-game start* | `006B1A20` → `006B1900` `vtbl+160` `SOUND_THEME` | no | **UNREAD** fire |

**Answer:** first-seen after Leave / Init Game is **no
`00A01920`, no `SND_*`, no `MUSIC_SET_*`**. Do not play
`SND_MENU_04` or a title track from `RequestNewGame`.
The still-living voice is the unnamed `0042DED5` fade.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  006286F0 ×3 PlayAVI
  0042F00A call 0042DED5          // ONLY first start (pre-Leave)
    [0x13B8394].vtbl+68(path, 0, 0, 1.0, 0, -1)
  005952C3 UI show
  loop 0042F041
    0059A238 UI vtbl+32
      0xE5 / 0x126 / 15           // no 00A01920, no SND_*
  msg 15 → [retail+41]=1
0042F2A2 Leave frontend           // not 00A01920
  [0x13B8394].vtbl+72(0x1F4)      // 500 ms fade; keep voice
  0042EBB6 +41 skip
    vtbl+64 / vtbl+72(0) / 00991750 / 009918F0
0042F491 Init Game 004184BD
  "Init Subtitled Message" 004CDB10   // 00A39010; no 00A01920
  "Init Conversation Attitude" 004CD670
  Init World 004A6E30                 // no 00A01920
    "Init UI Manager" 0041D198        // bind 004291B8
    "Init Speech Gain Manager" 006E3EC0
  Create Players 004166A8
  "Init Sound" 00417A58               // register; no vtbl+68
  [game].vtbl+32 00416953
    004A6550 "Init Atmos"
      alloc 32 → 006B1960 → world+36
      MAIN_SOUND_SETUP; vtbl+144(0x8010, …)
    "Init Scripts" 006E7740
    0049F180 "Init GUI" 0043A380      // reset; no SND_
004189C2 first type-1 004A5A40
  004A5E10 inc WorldFrame             // 0 → 1 BEFORE atmos
  004A5E7B 006B2260
    [0x13B8394]=0x01041044
    MARKER_POSITIONAL_ATMOS dummy miss
    006B1F30 WorldFrame&3 != 0 skip vtbl+184
    006B1A20 SOUND_THEME TOD gate     // not 00A01920
  006E37D0 speech-gain empty
```

`e8.tsv`: `00A01920` has **22** `E8` sites. **None** in
`004184BD` / `00417A58` / `004CDB10` / `004A6E30` /
`004A6550` / `006B1960` / `006B2260` / `0043A380` /
`004189C2` / `004A5A40`. **PROVEN.**

---

## 1. `00A01920` is a lookup, not a play

`listing-00a00000.txt` `00A01920`–`00A0193F`:

```
00A01920  push ecx
00A01921  mov ecx, [ecx+4]
00A01924  xor eax, eax
00A01926  test ecx, ecx
00A01928  mov [esp], eax
00A0192B  je  00A0193E          // null table → 0
00A0192D  mov edx, [esp+8]      // name
00A01931  lea eax, [esp]
00A01934  push eax
00A01935  push edx
00A01936  call 00A38420         // hash + table find
00A0193B  mov eax, [esp]
00A0193E  pop ecx
00A0193F  ret 4
```

`00A38420` hashes the CString (`004014A0`) and walks
`[ebx+4]` / `[ebx+8]`. Return is a symbol id or 0.
**PROVEN** lookup. No `[0x13B8394]`. No `vtbl+68`.
No `vtbl+36`.

The small wrapper that *does* play after a hit is
`0041CEB3` (`functions.tsv` callees `00991840`,
`00A01920`, `0041C930`):

```
0041CEB3
  eax = [0x13B8394];  eax==0 → 0
  [this+100]!=0      → 0
  esi = 00991840(arg1)          // bank
  edi = 00A01920(esi, arg0)     // symbol
  0041C930                      // event record
  [0x13B8394].vtbl+36
    vtbl+12(esi, edi, 1, rec, 0)
```

Zero `E8` of `0041CEB3` in `e8.tsv`. It is a vtbl /
indirect play helper, **not** an Init Game stage.
Widget slot pairing **UNREAD**. Not on the recovered
Leave / `004184BD` / first type-1 tree. **PROVEN** skip.

---

## 2. Frontend click leftover is DISPROVEN

Authority: `proofs/audio-frontend/README.md` §2.

| Claim | Class | Evidence |
|---|---|---|
| `0059A238` plays a click | **DISPROVEN** (first-seen) | Msg `0xE5` → `00599D5C`; `0x126` → `00851920`; `15` → `0059A2DA` `[retail+41]=1`. No `00A01920`, no `SND_*`, no `[0x13B8394]` on those branches. |
| `Play2DSound UI_CLICK` on frontend | **DISPROVEN** | Opcode `00CBF89E` / `00CBF8DA` only. Not UI vtbl+32. |
| `SND_MENU_04` is Press Start | **DISPROVEN** | `005F64DD` / `005F6793` → `00A01920` then `vtbl+36` → `vtbl+12`. In-game GUI (near EXP spend), not `0059A238`. |
| `SND_MENU_04` on Init Game | **DISPROVEN** | `e8.tsv` callers of `005F64DD` are `005F6A54` / `005F6B2C` / `005F6BC4` / `005F6E34` / `006901CB` / `006901EB`. None in `004184BD`. |

Do **not** replay `SND_MENU_04` because New Game left the
menu.

---

## 3. All 22 `E8` of `00A01920` are later / leftover

`e8.tsv`:

| Site | Owner | `SND_*` / role | On Leave / Init Game / first type-1? |
|---|---|---|---|
| `0041CEDF` | `0041CEB3` | widget 2D play helper | **DISPROVEN** (no `E8` of owner) |
| `004CEADC` / `AFD` / `B2E` / `B4F` | `004CEA60` | prefix `"SND_"` then lookup | **DISPROVEN** first-seen |
| `005F669F` / `005F690C` | `005F64DD` / `005F6793` | `"SND_MENU_04"` | **DISPROVEN** (EXP GUI) |
| `0064ACCF` | `0064A9C8` | HUD `"SND_"` prefix | **DISPROVEN** (quest orb / health) |
| `0065CA67` | `0065C781` | `"SND_"` | **DISPROVEN** first-seen |
| `0066BB79` | thing-def `"SND_"` prefix | later | **DISPROVEN** first-seen |
| `006E2ABB` / `39AB` / `5D3C` | `006E5A00` conversation | later | **DISPROVEN** (`dialogue-first` empty) |
| `00773D05` | `007727E0` | later | **DISPROVEN** first-seen |
| `00785AD3` | `00785160` | `SND_SHOPBELL_01` | **DISPROVEN** first-seen |
| `007E613C` | `007E2DF0` | later | **DISPROVEN** first-seen |
| `0088F581` / `1181` / `0A49` / `F8A8` | later systems | later | **DISPROVEN** first-seen |
| `009BDA21` / `DA42` | `009BCA20` engine audio | bootstrap / internal | **DISPROVEN** as Init Game |

`004CEA60` is the subtitle / text-key helper: miss →
`0099F690("SND_")` → `00A01920` again. Callers of
`004CEA60` are `004CEBB0` (`004CEC88` / `004CECF4`) and
`00717CC4`. `004CEBB0` `E8` sites are `00717B18` /
`00717C04` / `00717DC3`. Those are reached from
`00904754` / `00904834` / `00904919`, **not**
`004CDB10`. **PROVEN** skip.

`004CDB10` `"Init Subtitled Message"` only
`00A39010` on `0x13B8A54`. No `00A01920`. **PROVEN.**

`004CDB70` (unknown-subtitle ctor) stores
`00991840(1)` at `[this]`. Register, not play.
**PROVEN.**

---

## 4. Init Game audio is register + construct

### 4a. `"Init Sound"` `00417A58`

After Create Players, before Load Particles and
`00416953`. Gate `[0x13B8394]==0` → `je 00418286`.

| Step | VA | Play? | Class |
|---|---|---|---|
| Localised banks | `"Init Localised Sound Bank Entries"` / `"Registering Localised Sound Bank"` → `009919C0` | no | **PROVEN** register |
| `MAIN_SOUND_SETUP` | `0044C6B0` / `004196B2` | no | **PROVEN** lookup |
| `"Init Sound Bank Entries"` | `"Registering Sound Bank"` → `009919C0` / `00991840` | no | **PROVEN** register |
| `"Registering Atmos Sound Bank"` | `00991C10` then `00991840` + `vtbl+12` | no | **PROVEN** register |
| Tail | `00991840(1)` → `[game+16]` | no | **PROVEN** |

No `call 00A01920`. No `call [eax+68]`. Not
`0042DED5`. **PROVEN.**

`004175E5` (`vtbl+64` / `vtbl+72(0)` / `00991750` /
`009918F0`) is the **same stop quartet** Leave skips
when `+41!=0`. It is a later game teardown, not
`004184BD`. **DISPROVEN** as Init Game first audio.

### 4b. `"Init Atmos"` `004A6550` → `006B1960`

`00416953` → `[world].vtbl+28`:

- alloc 32 → `006B1960` → `world+36`
- ctor: `[this]=0`, `[+8]=0` rain, `[+9]=0`,
  `[+10]` `00A01B10`, `[+20]=0xFFFFFFFF`
- if `[0x13B8394]`: `MAIN_SOUND_SETUP` then
  `[0x13B8394].vtbl+144(0x8010, +120, +124, this+4)`

Listener / bus setup, **not** `vtbl+68`, **not**
`00A01920`. **PROVEN** construct. Payload of `+144`
**PARTIAL**.

### 4c. Init GUI / Init UI Manager

`0049F180` `"Init GUI"` `0043A380` is reset + recopy
on live `[0x13B8790]` (`proofs/init-gui-0043A380`).
No `00A01920`. No `SND_*`. **PROVEN.**

`004A6E30` `"Init UI Manager"` `0041D198` is
`004291B8` bind. **PROVEN** no play.

---

## 5. First type-1 is `006B2260`, not `00A01920`

`004A5A40` after `004A5E10` (WorldFrame 0→1):

```
mov ecx, [esi+36]     ; world+36 Atmos Processor
test ecx, ecx
je  004A5E80
call 006B2260
```

`006B2260`:

```
eax = [0x13B8394]
if eax==0  ret
camera +452 vs 0x129BA3C
  ST<=thresh → 006B2496  (stop RAIN only if [this+8]==1)
  else:
    MARKER_POSITIONAL_ATMOS dummy miss
    no 006B1900("RAIN","SOUND_THEME_RAIN",1.0,2000)
006B1F30                 // WorldFrame&3 != 0 skip vtbl+184
006B1A20                 // SOUND_THEME TOD gate
```

Dummy has **no** `MARKER_POSITIONAL_ATMOS`. First-seen
rain `006B1900` **DISPROVEN**.

`006B1900` is `[0x13B8394].vtbl+160(name, …)` —
**not** `vtbl+68`, **not** `00A01920`. **PROVEN** slot
split.

`006B1A20` first-seen: ctor `[this]=0`, `[+9]=0` so
the whole skip is not taken.

```
006BB1E0  env+8 in (0x125D640, 0x124F6F4]  → day
          then 006B1900 SOUND_THEME / NIGHT (night vol 0)
006BB790  inverse of that band
          then 006B1900 NIGHT / SOUND_THEME (day vol 0)
```

`006BBC30` writes `env+8` from
`[eax+180]*[0x122DC88]`. Script `SetTime` /
`SetTimeOfDay` is **not** on this walk; lighting TOD
bytes default 0 (`2026-08-18-environment.md`). Exact
first-seen `env+8` float is **UNREAD**
(`audit-worldcamera`). `0x125D640` / `0x124F6F4`
immediates **UNREAD** here. `SOUND_THEME` count via
`009AD3B0` **UNREAD**. Whether first type-1 fires
`vtbl+160` is **UNREAD**.

Even if it fires, the name is **`SOUND_THEME` /
`NIGHT`**, not `SND_*`, not `MUSIC_SET_*`, and the
call is **`006B1900`**, not `00A01920`.

---

## 6. First `SND_` / first music

| Claim | Class | Evidence |
|---|---|---|
| First `SND_*` string *pushed* on Leave / Init Game / first type-1 | **none** | no `push "SND_…"` in `004184BD` / `00417A58` / `006B2260` |
| First leftover named `SND_*` if HUD later plays | `SND_MENU_04` (`005F64DD`) | **LEFTOVER** vs this walk |
| First leftover `SND_` prefix helper | `004CEA60` / `0064A9C8` / `0066BB79` | **LEFTOVER** |
| `PlayMusic` / `MUSIC_SET_*` after Leave | **none** | `script-playmusic`; runner not entered |
| Frontend `0042DED5` as `MUSIC_SET_*` | **DISPROVEN** | different object; name **UNREAD** |
| First leftover script music | `PlayMusic MUSIC_SET_NULL` | **LEFTOVER** (`00DB86B0`) |
| First leftover *named* set | `MUSIC_SET_OAKVALE` | **LEFTOVER** (same def, later PC) |

Do **not** grep `script.bin` for `MUSIC_SET_` or
`strings.tsv` for `SND_` and play the first hit from
`RequestNewGame`.

---

## 7. C# vs native after Leave / Init Game

| Host | Native | Class |
|---|---|---|
| `Note(0042DED5)` in `EnterFrontendAfterAvi` | `0042F00A` `vtbl+68` | **PROVEN** site; **DISPROVEN** player |
| `RequestNewGame` skip-stop note | `0042EBB6 +41` | **PROVEN** pairing |
| no `vtbl+72(500)` | `0042F2D8` | **DIVERGE** (nothing to fade) |
| `Note("Init Sound")` via `InitGameStages` | `00417A58` | **PROVEN** name; no bank analog |
| no `006B1960` / `world+36` object | `004A6550` | **DISPROVEN** host object |
| `TickAtmos` note only | `006B2260` dummy miss | **PROVEN** timing; no `00A01920` |
| no `SND_MENU_04` / `00A01920` | none on this tree | **PROVEN** pairing |
| `StartNewGame` `LastMusic` | leftover father | **DIVERGE** vs Init Game |

---

## Classifications (short)

1. **`00A01920` is a symbol lookup. PROVEN.**
   `00A38420` on `[this+4]`. Not a voice start.
2. **Frontend click / `SND_MENU_04` leftover.
   DISPROVEN.** `0059A238` has no `00A01920`.
   `SND_MENU_04` is in-game EXP GUI.
3. **Zero `E8` of `00A01920` on Leave / Init Game /
   first type-1. PROVEN.** 22 sites, all later or
   leftover.
4. **Init Game first audio op is register
   (`00417A58`) then Atmos ctor `vtbl+144`.
   PROVEN.** No `SND_*`. No `vtbl+68`.
5. **First in-game tick is `006B2260`. PROVEN site,
   dummy rain miss. PROVEN.** Candidate start is
   `SOUND_THEME` `vtbl+160` via `006B1900`. Fire
   **UNREAD**. Not `00A01920`.
6. **First `SND_*` / `MUSIC_SET_*` after Init Game
   — none. PROVEN.** Leftover first script music
   remains `PlayMusic MUSIC_SET_NULL`.

Do not play `SND_MENU_04`, `UI_CLICK`, or
`MUSIC_SET_*` from New Game because “we left the
menu.” The native voice that is still running is the
unnamed `0042DED5` fade.
