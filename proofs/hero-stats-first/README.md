# Hero / player stats / inventory / creature-def first bind after Leave Frontend

Investigation only. No production `src` edits.
Do **not** start at Oakvale / `CREATURE_HERO_CHILD` / `00DBDE40`.
That path is later `Q_NewOakValeIntro`, not Leave / Init Game /
first no-save 3D Present.

Do **not** treat `PlayerInterface.cs` (`004473A0` / `game+32`) as
Hero, stats, inventory, or a creature def.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **LEFTOVER**.

Sources: `docs/runtime/FORWARD_TREE.md` §§4–10;
`proofs/bone-config-first/README.md`;
`proofs/player-bind-world/README.md`;
`proofs/tng-spawn/README.md`;
`docs/status/investigations/E-player-palskin.md`;
`PlayerInterface.cs` / `EngineLifecycle.cs`;
`EngineLifecycleTests` (`New_game_is_leave_frontend_then_FinalAlbion_wld`,
`LoadWorld_00416953_no_save_is_004A1840_then_0049F180`,
`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`);
ExeIndex `listing-00440000.txt` / `listing-00480000.txt` /
`listing-004c0000.txt` / `e8.tsv` / `xrefs.tsv`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| Frontend binds Hero / stats / inventory / creature def? | **No.** | **DISPROVEN** |
| `PlayerInterface` (`004473A0`) is that bind? | **No.** Input object at `game+32`. | **DISPROVEN** |
| First *name/factory* bind after Leave? | Init Thing Components `004EE23F` → **`004EE294` `CTCHeroMorph`** then `CHeroMorphDef` / `CTCInventory*` / `CHeroDef` | **PROVEN** |
| First compiled def table after Leave? | Init Definition Manager `00416005(1)` `game.bin` | **PROVEN** load. Apply **UNREAD** |
| First *instance* creature-def bind? | `0049F180` → **`00449D90`** `009AD410("PLAYER_HERO")` miss → **`00449E0D` `CREATURE_HERO`** → **`0048A070`** | **PROVEN** |
| `006AC910` Thing at that first `0048A070`? | **No** on first-seen if holy-site miss. | **PARTIAL** (early-out **PROVEN**; retry site **UNREAD**) |

`00DBDE40` / kid `4300` are **not** on this list. **PROVEN**.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  PlayAVI / frontend 2D              // frontend.bin 009AD410 UI only
  006286F0 may 00449970 / 00487DC0   // slot lookup; not a def bind
0042F2A2 Leave frontend
  009BE420 + 009BEEB0 Present
0042F491 Init Game → 00418DCA → 004184BD
  Init Thing Components 004EE23F          // FIRST hero/stats/inventory names
    004EE294  CTCHeroMorph
    004EE304  CHeroMorphDef
    004EECFE  CTCInventory … Stats / Experience / …
    004F08F0  CHeroDef
  Init Definition Manager 00416005(1)     // game.bin (CREATURE_HERO lives here)
  Init Player Manager 0041732A
  Init Player Interface 004473A0          // NOT Hero
  Create Players 004166A8                 // 5×0x22C slots; 00522A20 factory name
  Init World 004A6E30
  00416953 Load world FinalAlbion.wld
    00416ABA 004A1840
      QST / WLD / Set Static Map
      [world+258]==0 → 004A2C80 0049F180(1)   // PARTIAL as first-seen taken
    00416BC8 push 0
    00416BCA 0049F180(0)                      // PROVEN site
      00449970 / 00487DC0 miss
      0049F1D7 00449D90                       // FIRST instance creature-def bind
        009AD410 "PLAYER_HERO"
        0044BA90 miss
        00449E0D "CREATURE_HERO"
        004498C0 slot
        00449E2D 0048A070 InitCharacterAs
          0048A0AF 00489D40 CreateCharacter
            00488B20 holy site                // first-seen miss → al=0
            [0x13B8647]==0 → ret 0            // no 006AC910
  004189C2 pumps
later (E8 caller UNREAD as first retry)
  GuildArrivalHSP → 006AC910 CREATURE_HERO mesh 4299
```

---

## 1. During frontend?

| Claim | Class | Evidence |
|---|---|---|
| Frontend Present is 2D UI | **PROVEN** | FORWARD_TREE §4; `0042DF9E` |
| `E8 004EE23F` / `00449D90` / `0048A070` from `0042xxxx` | **DISPROVEN** | `e8.tsv`: `004EE23F` only `00418585`; `00449D90` only `0049F1D7`; `0048A070` only `00449B31` / `00449E2D` |
| Frontend `009AD410` looks up `PLAYER_HERO` / `CREATURE_HERO` | **DISPROVEN** | `ResolveFrontendDef` is `frontend.bin` UI names |
| PlayAVI `006286F0` `00449970` / `00487DC0` is a creature-def bind | **DISPROVEN** | slot/thing pointer only (`00A01B50`) |
| `CREATURE_HERO_CHILD` on frontend | **DISPROVEN** | only string xref `00DBDF09` / `00DBDE40` |

**Answer:** no hero stats, inventory component, or creature def bind during frontend.

---

## 2. `PlayerInterface.cs` is not this

Different object. See also `proofs/player-bind-world/README.md`.

```
Init Player Interface 004473A0
  alloc 0x898; vtbl 01231BDC; store game+32
  0044A3B0 owner game+28
```

Pump is `00416E78` → `[game+32].vtbl+4` `00446A30` after `WorldFrame>1`.
It never calls `00449D90` / `0048A070` / `006AC910`.
It never stores `CREATURE_HERO`, mesh 4299, gold, or inventory items.

`004AE9D0` (player-manager tick slots on `game+80568`) is also **not**
Hero. **DISPROVEN**.

---

## 3. First bind after Leave (type / table)

Still Init Game. Before world file, before `0049F180`.

### 3.1 `004EE23F` Init Thing Components — first *names*

`004184BD` → `00418585` `E8 004EE23F`. First hero-family string in
that walk:

| Order | VA | Name | Class |
|---|---|---|---|
| 1 | `004EE294` | `CTCHeroMorph` (factory `004D2EF0`) | **PROVEN** first hero-stats *component* name |
| 2 | `004EE304` | `CHeroMorphDef` | **PROVEN** Strength / Will / Skill / Morality / Fatness persist type |
| later | `004EECFE`… | `CTCInventory` / Clothing / Weapons / **Stats** / Abilities / Experience / Trade / Quests / Map / Item | **PROVEN** inventory *type* register |
| later | `004F08F0` | `CHeroDef` | **PROVEN** type. Body **UNREAD** |

No file I/O. No Thing. **PROVEN** as first post-Leave bind in this
family (`bone-config-first` §3.1 already locked the morph pair).

`004D2880` also intern-names the `CTCInventory*` strings. That is
the factory helper used *from* `004EE23F`, not a frontend site.

### 3.2 `00416005(1)` Init Definition Manager — compiled table

`009F2450` / `0044C6B0` / `009ACB10`. Host `EnsureDefs` / `game.bin`.
`PLAYER_HERO` (type `PLAYER`, **no** Graphic) and `CREATURE_HERO`
(type `CREATURE`, Graphic **4299**, 32 sub-defs including `CHeroDef`
idx 10531 and `CHeroMorphDef` idx 10535) live here.

This is a table load, not an attach onto a player Thing. **PROVEN**
as first compiled-def *open* after Leave. Field apply **UNREAD**.

---

## 4. First instance creature-def bind (`00449D90` / `00449E0D` / `0048A070`)

### 4.1 Callers

| Dest | `E8` sites | Class |
|---|---|---|
| `00449D90` | **only** `0049F1D7` (`0049F180`) | **PROVEN** |
| `0048A070` | `00449E2D` (inside `00449D90`) and `00449B31` (`00449B20`) | **PROVEN** |
| `00449B20` | only `0066FF89` | **LEFTOVER** vs first-seen (later helper) |
| `00489D40` | only `0048A0AF` (inside `0048A070`) | **PROVEN** |
| `0049F180` | `00416BCA` (`push 0`) and `004A2C80` (`push 1`, inside `004A1840` if `[world+258]==0`) | `00416BCA` **PROVEN**. `004A2C80` **PROVEN** insn; first-seen take **PARTIAL** (`+258` ctor 0, writer **UNREAD**) |

`00449D20` (nearby slot walk, returns `al`) is **not** the def bind.
Its only `E8` is `004397BA` (in-game HUD / display object `esi+8`).
**DISPROVEN** as first after Leave.

### 4.2 `0049F180` gate (**PROVEN** listing)

```
0049F1B6  call 00449970
0049F1BD  call 00487DC0          // player Thing
0049F1C4  je   0049F1CF          // miss → bind
0049F1C6  test [eax+145], 1
0049F1CD  je   0049F1DC          // live Thing, bit0 clear → skip
0049F1D7  call 00449D90
```

No-save Load World: no player Thing → **always** `00449D90`.

### 4.3 `00449D90` (**PROVEN** listing `00449D90`–`00449E50`)

Separate function (int3 gap after `00449D20`). `ecx` = player manager.

```
0099EBF0 "PLAYER_HERO"
009AD410([esi+8])
0044BA90(def)                  // 009AD9E0 appearance; eax<=0 fail
je 00449E0B                    // TLC: no Graphic → miss
… hit uses [edi+60] …
00449E0D  push "CREATURE_HERO"
004498C0                       // slot walk
00449E2D  call 0048A070        // BOTH hit and miss
```

`00449E0D` is the miss immediate (`push "CREATURE_HERO"`), not a
function entry. Host `InitHeroDefFn = 00449D90` is the right fn;
`00449E0D` is the TLC fallback site.

`0044BA90`: `arg<=0` → fail; else `009AD9E0`. **PROVEN** as the
`PLAYER_HERO` attach attempt. TLC `PLAYER_HERO` is type `PLAYER`,
raw 21, 0 sub-defs, **no** Graphic → miss. **PROVEN** file.

### 4.4 `0048A070` `CPlayer::InitCharacterAs` (**PROVEN**)

```
[esi+28] vtbl+12 → [esi+32]
[esi+28] vtbl+48 → [esi+36]
00A01B50 [esi+52]
if null OR [thing+145] bit0:
  0048A0AF  call 00489D40      // CreateCharacter
0099EBF0 "CPlayer::InitCharacterAs"   // log even if create failed
00487CF0 …
```

First-seen `+52` empty → `00489D40` **does** run.

### 4.5 `00489D40` may not spawn (**PROVEN** early-out)

```
00488B20  find holy site ([0x13B866C] name + 0048CC60)
          miss: "*** WARNING : failed to find a holy site with Sc"
test al
mov al, [0x13B8647]
jne create-body
cmp [0x13B8647], 0
je  ret 0                     // no 006AC910
```

`0x13B866C` is empty first-seen (WLD path is `game+90576`, not this
global). `0x13B8647` has no first-seen writer in-repo (sibling
`0x13B8648` is 0). **PROVEN** as “bind name, no Thing” on that miss.

`006AC910` is only `E8` from `00489FC1` (this fn) and `0089F660`
(later leftover). Host `SpawnHero` at `LoadFromFirstRealRegion` is
the *successful* create, not the first `00449D90`. Folding
`0049F180` / `00449D90` / `0048A070` Notes into LevelLoader is
**LEFTOVER** vs the Load World call.

---

## 5. Inventory / stats *instance* (later)

| Site | When | Class |
|---|---|---|
| `004EECFE` `CTCInventory*` | Init Thing Components | **PROVEN** type only |
| `CHeroMorphDef` on `CREATURE_HERO` | `game.bin` sub-def 10535 | **PROVEN** file. Apply **UNREAD** |
| `004C9D60("CTCPhysicsControlled")` | `006A9DD0` after a real `006AC910` | **PROVEN** physics. **DISPROVEN** as inventory |
| Script `GiveGold` / `World.Inventory` | `00CC82F5` after a started cutscene | **DISPROVEN** first-seen (`HasStarted` false) |
| `PersonalScriptMain` item grants | after Init Quests fibers | **UNREAD** as first inventory write |

First *inventory instance attach* after Leave is **UNREAD**. It is
**after** `004EE23F` and **after** the `00449D90` name bind. It is
**not** frontend and **not** `PlayerInterface`.

---

## Host leftovers

| Host | Native | Class |
|---|---|---|
| `InitCharactersAndQuests` Notes `00449970` / `00487DC0` only | also `0049F1D7` `00449D90` | **LEFTOVER** gap |
| `SpawnHero` / `ResolveHeroDefinition` Notes `00449D90` / `00449E0D` / `0048A070` as LevelLoader | those VAs already ran in `0049F180` | **LEFTOVER** site |
| Second `Note(InitCharactersFn)` in `SpawnHeroFromPlayerStart` | `0049F180` is not called from `006AC910` | **LEFTOVER** |
| `PlayerInterface` as player bind | input `game+32` | **DISPROVEN** |

---

## Open

- First-seen take of `004A2C80` (`004A1840` tail, `[world+258]==0`)
  **PARTIAL**.
- Which later `0048A070` / `00489D40` first returns 1 and hits
  `006AC910` **UNREAD** (`004A2C80` retry vs `0066FF20` vs region
  holy-site list).
- `0044BA90` / `009AD9E0` exact fail field on `PLAYER_HERO` **PARTIAL**.
- First `CTCInventory*` attach / first gold or item write **UNREAD**.
