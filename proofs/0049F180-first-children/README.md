# `0049F180` first children after no-save Leave

Investigation only. No production `src/` edits.

Question: first children of `0049F180` after no-save Leave.
`00449D90` `PLAYER_HERO` miss. Host `InitCharactersAndQuests`
leftover gaps?

Do **not** start at Oakvale / `00DBDE40` /
`CREATURE_HERO_CHILD` / Graphic **4300**. That is later
`Q_NewOakValeIntro`, not Leave / Load World / first no-save
3D Present.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: ExeIndex `listing-00480000.txt` `0049F180`–
`0049F2CB` / `00416ABA` / `004A1840` / `004A21F0` /
`004A2C80` / `004A67D0`;
`listing-00440000.txt` `00449D90` / `00449970` /
`00449700` / `0044BA90`;
`listing-00680000.txt` `006B8410` / `006B84B0`;
`listing-00880000.txt` `00881210`;
`listing-00a00000.txt` `00A01B50`;
`listing-009c0000.txt` `009D8240` / `009D8250` /
`009F1760`;
`e8.tsv`;
`src/Fable.Game/EngineLifecycle.cs`
`InitCharactersAndQuests` / `ResolveHeroDefinition` /
`SpawnHeroFromPlayerStart`;
siblings `proofs/creature-after-leave`,
`hero-stats-first`, `hero-00489D40-retry`,
`hero-4299-create`, `init-gui-0043A380`,
`004B2890-empty-first`, `004A1840-second-site`,
`audit-worldcamera`;
`EngineLifecycleTests.LoadWorld_00416953_no_save_is_004A1840_then_0049F180`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| First no-save `0049F180`? | **Only** `00416BCA` `push 0` `ecx=world` after `004A1840` when `[0x13B8648]==0` | **PROVEN** |
| `004A2C80` `push 1` on this walk? | **No.** Site is inside save reader `004A21F0` (`ret 8`), not `004A1840` | **DISPROVEN** as no-save |
| First *work* children? | `00449970` / `00487DC0` miss → **`00449D90`** → `006B8410` → `0043A380` → `004B4260` → `004B2890` → tail | **PROVEN** |
| `00449D90` `PLAYER_HERO`? | `009AD410` then `0044BA90` fail (no Graphic) → `00449E0D` `"CREATURE_HERO"` → `0048A070` | **PROVEN** miss |
| Is that `CREATURE_HERO_CHILD`? | **No.** Immediate is `"CREATURE_HERO"` | **DISPROVEN** |
| First `00489D40` create? | Holy-site miss + `[0x13B8647]==0` → `ret 0`. No `006AC910` | **PROVEN** |
| Host `InitCharactersAndQuests` leftover gaps? | **Yes** — see §5 | **PROVEN** gaps; some are no-ops |

---

## Verdict

**No-save Leave has one `0049F180`: `00416BCA(0)`.**

Older notes that put `004A2C80` “inside `004A1840` when
`[world+258]==0`” used a **bad function merge**.
`004A1840` ends `004A21DF` `ret 4`. `004A2C80` lives in
`004A21F0` (FableSav HEADER). That function’s only `E8`
parents are save sites (`004A3200` / `004A2F10` /
`004A2D70`). No-save `[game+90588]` empty skips
`004A3200`. **DISPROVEN** as first-seen New Game.

First children that do work, in listing order:

1. `"Init Characters"` — player Thing miss, then the
   **only** `.text` `E8` of `00449D90`.
2. `006B8410` — unique thunk: `WorldCamera+6500` colour
   bank `+144` → `00881210` list reset.
3. `"Init GUI"` `0043A380` — reset, not ctor
   (`init-gui-0043A380`).
4. `"Init Quests"` `004B4260([world+172])` then
   `004B2890` (empty `QM+112` no-op).
5. Tail packer `009F1760` / type getter `00449700` /
   `0041649C` on `[0x13B86A0]` / `[world+140]=[0x13B89BC]`.

`004B4A10` is the **sibling** at `00416BCF`, after
`0049F180` returns. Not a child.

Host `InitCharactersAndQuests` Notes the three log names
and runs the `004B4260` activate loop. It does **not**
Note `00449D90` here. It folds `004B4A10` into this
method. Those are leftover **site** gaps, not missing
creates: first Hero Thing is still later Lookout
`006AC910`.

---

## Timeline (no-save New Game)

```
0042F2A2  Leave frontend
0042F491  Init Game
  004A67D0  CWorld ctor  [world+258]=0
  Create Players 004166A8          // 0043B570 PLAYER_GUI_PC
  00416953  Loading world FinalAlbion.wld
    00416ABA  004A1840             // ends 004A21DF; no 0049F180
    [0x13B8648]==0
    00416BC8  push 0
    00416BCA  0049F180(ecx=world)  // THIS NOTE — only no-save site
      "Init Characters"
        00449970 / 00487DC0  miss
        0049F1D7  00449D90
          009AD410 "PLAYER_HERO"
          0044BA90 miss            // no Graphic
          00449E0D "CREATURE_HERO" // not CHILD
          0048A070 → 00489D40
            00488B20 miss
            [0x13B8647]==0 → ret 0 // no 006AC910
        0049F1E5  006B8410         // [world+24]+6500
      "Init GUI"     0043A380
      "Init Quests"  004B4260 / 004B2890
      tail           009F1760 / 0041649C / [world+140]=WorldFrame
    00416BCF  Activate Initial Quests 004B4A10   // sibling
    004BBC00  ret 4
004189C2  dummy pumps
later Lookout 006AC910 CREATURE_HERO 4299
```

`004A21F0` / `004A2C80` `0049F180(1)` is **not** on this
list. **PROVEN.**

---

## 1. Frame and callers

`listing-00480000.txt` `0049F180`–`0049F2CB`:

```
0049F180  sub esp, 48
0049F183  mov eax, [0x139C8A8]
0049F18B  mov esi, ecx              // world
… cookie …
0049F18D  push "Init Characters"
0049F1B3  mov ecx, [esi+12]         // player manager
0049F1B6  call 00449970
0049F1BD  call 00487DC0
0049F1C4  je  0049F1CF              // miss → 00449D90
0049F1C6  test [eax+145], 1
0049F1CD  je  0049F1DC              // live + bit0 clear → skip
0049F1D6  push eax                  // 0049F180 arg (0 or 1)
0049F1D7  call 00449D90
0049F1DC  mov ecx, [esi+24]         // WorldCamera
0049F1DF  mov ecx, [ecx+6500]
0049F1E5  call 006B8410
0049F1EA  push "Init GUI"
0049F20E  mov ecx, [0x13B8790]
0049F214  call 0043A380
0049F21B  push "Init Quests"
0049F247  lea edx, [esi+172]
0049F24E  call 004B4260
0049F253  mov ecx, [0x13B89FC]
0049F259  call 004B2890
0049F260  call 009D8250             // retail ret
0049F291  call 009F1760
0049F299  call 00449700
0049F2A3  mov ecx, [0x13B86A0]
0049F2AD  call 0041649C
0049F2B2  mov edx, [0x13B89BC]
0049F2BC  mov [esi+140], edx        // world+140 = WorldFrame
0049F2CB  ret 4
```

`e8.tsv` destinations of `0049F180`: **two** sites.

| Site | Arg | Parent | No-save? |
|---|---|---|---|
| `00416BCA` | `push 0` | `00416953` after `004A1840` | **PROVEN** first-seen |
| `004A2C80` | `push 1` | `004A21F0` FableSav (`ret 8` at `004A2D60`) | **DISPROVEN** |

`004A21F0` callers (`e8.tsv`): `004A32EA` / `004A340D`
(`004A3200` “Loading save”), `004A3017` (`004A2F10`),
`004A2DC2` (`004A2D70`, writes `[world+258]=1` first so
the `004A2C80` arm is skipped). No-save never enters
`004A3200`. **PROVEN** (`004A1840-second-site`).

`0049F180` forwards its stdcall arg into `00449D90`
(`ret 4`). `00449D90` does **not** read that dword.
`push 0` vs `push 1` does not change the miss path.

No-save has no player Thing → `00487DC0` `00A01B50`
returns 0 → **always** `0049F1D7`. **PROVEN.**

---

## 2. Child `E8` list (`0049F180`–`0049F2CB`)

| Site | Dest | First-seen take | Host `InitCharactersAndQuests` |
|---|---|---|---|
| `0049F19A` | `0099EBF0` `"Init Characters"` | yes | log folded into Note string |
| `0049F1A5` | `009D8240` | **`ret`** | n/a (stub) |
| `0049F1AE` | `0099EAE0` | CString dtor | n/a |
| `0049F1B6` | `00449970` | `[manager+28]` → `004498C0` | **Note** `"00449970 / 00487DC0"` |
| `0049F1BD` | `00487DC0` | `+44` `00A01B50` **0** | same Note |
| `0049F1D7` | `00449D90` | **yes** (miss gate) | **LEFTOVER gap** |
| `0049F1E5` | `006B8410` | **yes** (unconditional) | **LEFTOVER gap** |
| `0049F1F5` / `200` / `209` | `0099EBF0` / `009D8240` / `0099EAE0` `"Init GUI"` | stub + dtor | log folded |
| `0049F214` | `0043A380` | reset singleton | **Note** + `PlayerGuiReady` |
| `0049F224` / `22F` / `238` | same trio `"Init Quests"` | stub + dtor | log folded |
| `0049F24E` | `004B4260` | activate `world+172` | **Note** + `ActivateNamedQuest` |
| `0049F259` | `004B2890` | empty `QM+112` no-op | **Note** only |
| `0049F260` | `009D8250` | **`ret`** | n/a |
| `0049F291` | `009F1760` | pack type-`9` record | **LEFTOVER gap** |
| `0049F299` | `00449700` | `mov eax,[ecx+28]; ret` | **LEFTOVER gap** |
| `0049F2AD` | `0041649C` | game `[0x13B86A0]` | **LEFTOVER gap** |
| `0049F2C3` | `00BFE9F9` | cookie | n/a |

`009D8240` / `009D8250` are one-insn `ret` (retail
phase begin/end compiled out). Not leftover work.

`004B4A10` / `004B0D30` are **not** in this table.
Host Notes them inside `InitCharactersAndQuests`
anyway. **LEFTOVER** parent.

---

## 3. `00449D90` — `PLAYER_HERO` miss, not CHILD

Only `E8` of `00449D90` is `0049F1D7`. **PROVEN.**

```
00449D90  ecx = player manager
          0099EBF0 "PLAYER_HERO"
          009AD410([esi+8])
          0044BA90(def)
          je 00449E0B                 // fail
00449E0D  push "CREATURE_HERO"
          004498C0
00449E2D  call 0048A070               // both hit and miss
```

`0044BA90`: `arg<=0` → `xor al,al` / `ret 8`. Else
`009AD9E0` appearance. TLC `PLAYER_HERO` is type
`PLAYER`, raw 21, **0** subs, **no** Graphic →
`009AD410` / `0044BA90` fail. **PROVEN** file
(`hero-stats-first`, `GameBinFormatTests`
`FindMeshId("CREATURE_HERO")==4299`;
`PLAYER_HERO` has no mesh).

The miss immediate is `"CREATURE_HERO"`, not
`"CREATURE_HERO_CHILD"`. Kid **4300** is a different
def. **DISPROVEN** on this call.

`0048A070` empty `[esi+52]` → `0048A0AF` `00489D40`.
`00488B20` holy-site miss (`[0x13B866C]` empty;
`NOVStartHSP` is not a live Thing) and
`[0x13B8647]==0` → `ret 0`. No `00489FC1` /
`006AC910`. **PROVEN** (`hero-00489D40-retry`).

So the first post-Leave *name* bind of the Hero def
is this child. The first Hero *Thing* is not.

---

## 4. Other first children (short)

### `006B8410` (unique)

```
006B8410  add ecx, 0x90
          jmp 00881210
```

`ecx` is `[WorldCamera+6500]` (`006B84B0` colour-filter
bank from Init World `006B4900`). Only `E8` is
`0049F1E5`. `00881210` resets circular lists at
`+16` / `+120` / `+160` when counts are nonzero, then
walks `+4` nodes. Ctor `006B84B0` → `00881370` leaves
those counts **0** first-seen → first three arms
`je` skip. Whether the `+4` walk is empty is
**PARTIAL** (ctor dummy **PROVEN**; a pre-Leave
filler is **DISPROVEN**). Host does not model
`+6500` (`audit-worldcamera`). Implementing a
first-seen empty reset here would be leftover
theater.

### `0043A380` / `004B4260` / `004B2890`

Already closed: GUI **reset** (`init-gui-0043A380`);
quests **construct** (`004B4260` / `004B3CE0`);
first `004B2890` **no-op** (`004B2890-empty-first`).
Host activate loop is the `004B4260` arm. The
`004B2890` Note is a skip of a proven no-op.

### Tail `009F1760` / `0041649C` / `world+140`

Packs a 1-byte-extended record (`[esp+16]=9`,
`+20=0xFF`, flags) through `009F1760`, reads
`00449700` (`[manager+28]`), then
`0041649C([0x13B86A0], record)`.

`0041649C` first child `0049D8C0` tests
`[0x13B9288 + type*64]`. Type **9** first-seen
nonzero → **UNREAD**. Hit would `004AE9A0` on
`game+80568`; miss still runs `0049E1D0` /
`00434A30`. This is **not** the later pump
`00416FD7` fade (`dummy-pumps-before-region`
first-seen skip is that other site).

Then `[world+140] = [0x13B89BC]` (WorldFrame,
**0** at this instant; unique inc is later
`004A5E10`). Host has no `world+140` store.

---

## 5. Host `InitCharactersAndQuests` leftover gaps

```
Note(InitCharactersFn, … "0049F180 push 0 ecx=world");
Note(PlayerCreatureBindFn, … "00449970 / 00487DC0");
Note(InitGuiFn, … "0043A380 …");
PlayerGuiReady = true;
Note(InitQuestsFn, … "004B4260 [world+172] …");
Runtime = ScriptRuntime.Detached();
foreach name in _worldPlus172
    ActivateNamedQuest(name, "Init Quests");
Note(QuestManagerActivate, … "004B2890");
Note(ActivateInitialQuestsSite, … "00416BCF … skip 004B4A10");
Note(ActivateInitialQuestsFn, … "004B4A10 …");
Note(QuestCardFindFn, … "004B0D30/00896A30 …");
QuestsInitDone = true;
```

| Host | Native | Class |
|---|---|---|
| Called only after `LoadWorldMap` when `[0x13B8648]==0` | `00416BCA` only on no-save | **MATCH** site |
| No `004A2C80` | save-only `004A21F0` | **MATCH** (not a miss) |
| Notes `00449970` / `00487DC0` only | also `0049F1D7` `00449D90` | **LEFTOVER** gap |
| No `006B8410` | unique child after the bind | **LEFTOVER** gap (empty reset) |
| No tail `009F1760` / `0041649C` / `world+140` | listing `0049F260`–`0049F2BC` | **LEFTOVER** gap |
| `004B4A10` / `004B0D30` Notes inside this method | sibling `00416BCF` / later card find | **LEFTOVER** parent |
| `ResolveHeroDefinition` Notes `00449D90` as LevelLoader | that VA already ran here | **LEFTOVER** site |
| `SpawnHeroFromPlayerStart` second `Note(InitCharactersFn)` | `0049F180` is not under `006AC910` | **LEFTOVER** |
| No `CREATURE_HERO_CHILD` | listing `"CREATURE_HERO"` | **MATCH** |

`InitCharactersAndQuests` does **not** spawn a Thing.
That is **MATCH** vs first-seen `00489D40` `ret 0`.
Do **not** grow a create here to “fill” the
`00449D90` Note gap.

---

## 6. Not these

| Candidate | Why not first children of no-save `0049F180` |
|---|---|
| `CREATURE_HERO_CHILD` / `00DBDE40` | later Oakvale intro |
| `004A2C80` `0049F180(1)` | save `004A21F0` |
| `006AC910` / mesh 4299 | later Lookout, not a child |
| `0051FD80` | region TNG, after dummy pumps |
| `004B4A10` | sibling after return |
| Frontend `009AD410` | UI Type=10 only |
| `004EE23F` `CHeroDef` | Init Game type register |
| `004166A8` Create Players | slots + GUI **ctor** (before this fn) |

---

## Open

- `0041649C` type-**9** table `[0x13B9288+9*64]` at
  this tail: first-seen nonzero **UNREAD**. Whether
  `004AE9A0` runs here **UNREAD**.
- `00881210` `+4` walk on first `006B8410`: empty
  **PARTIAL**.
- Load-game first `0049F180` (`004A2C80` inside
  `004A21F0` when `+258==0`) **UNREAD** as a body
  (not this no-save note).
