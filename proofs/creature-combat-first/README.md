# First combat / attack after Leave Frontend

Investigation only. No production `src` edits.

Do **not** start at Oakvale / `Q_NewOakValeIntro_PreAttack` /
`00DBDE40` / `00DBE3C0` / `Father.PlayCombatAnim TURNING_AC90`.
That path is later leftover `Q_NewOakValeIntro`
(`00DABAC0` → `00DBDE40`), not Leave / Init Game / first
no-save 3D Present.

Do **not** start at `CInputProcessCombat` or a WASD melee click.
Player Interface is constructed after Leave, but first pumps
do not deliver an attack.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `docs/runtime/FORWARD_TREE.md` §§4–11, 15;
`proofs/camera-after-leave/README.md`, `navmesh-first`,
`entity-task-queue`, `script-entity-cmds`, `newgame-script`,
`player-bind-world`, `audit-playerinterface`, `tng-spawn`,
`xseq-walk-first`;
`docs/status/investigations/2026-08-18-first-scene-things.md`;
`EngineLifecycle.cs` (`InitWorldInitStages` / `TickWorld`);
`NewGameScript.cs` / `RegionTravel.cs` / `EntityDispatcher.cs`;
`EngineLifecycleTests` (`New_game_is_leave_frontend_then_FinalAlbion_wld`,
`No_save_does_not_activate_Q_NewOakValeIntro`);
`WorldSceneTests` (`FirstSeenPlayCombatAnimationAppliesPose=false`);
listings `004A6E30` / `004A5A40` / `006ED3F0` / `006ED200` /
`006E8300` / `008B0D10` / `008AF2B0` / `008AF780`;
RTTI `CCombatManager` `0x01380264`, `CTCCombat` `0x01381424`,
`CInputProcessCombat` `0x01378710`.

---

## Verdict

**After Leave there is a combat *manager*, not a combat *fight*.**

First constructed combat object is `CCombatManager` `006ED3F0`
(size **92**, vtbl **`01261EB4`**, `world+76`) inside Init World
`004A6E30`. It binds `COMBAT_DIALOGUE_DEF_INSTANCE` and a table of
`COMBAT_SEQUENCE_*` names (first string
`COMBAT_SEQUENCE_BANDIT_ATTACK_MIDDLE`). Those are def / factory
binds, not an attack.

First *tick* is `004A5A40` → `006ED200` on the first type-1
WorldFrame. Both combatant lists are ctor-empty; the +56 handle
is empty. The tick is a skip.

There is **no** first-seen melee, projectile, `CTCCombat` attach,
`CActionPlayCombatAnimation`, `PlayCombatAnim`, `SetAttackable`,
or `CInputProcessCombat` on Leave / Init Game / first pumps.
Lookout AICreatures are villagers / traders / a beggar, not
bandits. Gameflow `WASP_BOSS` and TNG section `Q_WaspBoss` are
later.

Oakvale PreAttack / PostAttack / `TURNING_AC90` are **leftover**.

| Question | Answer | Class |
|---|---|---|
| Combat during frontend? | **No** | **DISPROVEN** |
| First combat object after Leave? | `CCombatManager` `006ED3F0` `world+76` | **PROVEN** |
| First combat *tick*? | `006ED200` on first `004A5A40` | **PROVEN** empty skip |
| First live attack / strike / hit? | **none** on this spine | **PROVEN** absence |
| First leftover “attack” *name*? | Oakvale `PreAttack` / `PlayCombatAnim` | **PROVEN** leftover |
| Host `CombatManager` type? | **missing** | **DIVERGE** (stage `Note` only) |

---

## Timeline (no-save New Game)

```
0042EC7C retail
  PlayAVI / 2D frontend              // no CCombatManager
  msg 15 → [retail+41]=1
0042F2A2 Leave frontend              // not 00DBDE40
0042F491 Init Game → 00418DCA → 004184BD
  Init Definition Manager            // game.bin types live
  Init Player Manager / Interface
  Init World 004A6E30
    "Init Combat Manager"
      alloc 92 → 006ED3F0 → [world+76]
        vtbl 01261EB4
        empty list +4 (24-byte sentinel)
        empty list +16 (0x88-byte sentinel)
        +28=1  +32=world  +36/+37=0
        +40=2.6f (0x40266666)  +44=0
        008AF220  init +56 (empty Thing handle +16)
        008B0D10  bind COMBAT_SEQUENCE_* table
      006E8300
        008AF2B0  COMBAT_DIALOGUE_DEF_INSTANCE
        +48=0.5f  +52=0
  00416953 Load FinalAlbion.wld
  004B4260 START_INITIAL_QUESTS      // no Q_WaspBoss
  user.ini ActivateQuest("Gameflow") // state0 yield
004189C2 first pumps
  WorldFrame 0→1: 004A5A40
    [world+248]=0 [world+260]=0
    [world+76] 006ED200              // empty +16; 008AF780 skip
    [world+80] 0051FCC0
    004B4490 / 006E75C0 …
    004A5DF3 006B3FF0
    004A5E10 inc WorldFrame
  WorldFrame<=1: skip 00446A30
  first WorldFrame>1: 00446A30 miss  // no 0041649C
  no 00CBFB7D / PlayCombatAnim
later 00501450 Lookout
  006AC910 CREATURE_HERO
    006A9DD0 → 00662880 + CTCPhysicsControlled
    no CTCCombat name, no 009035F0
```

`00DBDE40` / `Q_NewOakValeIntro_PreAttack` / `00DBE3C0` /
`Father.PlayCombatAnim` are **not** on this list. **PROVEN.**

---

## 1. Combat during frontend?

| Claim | Class | Evidence |
|---|---|---|
| Frontend Present is 2D (`0042DF9E` / `009BEEB0`) | **PROVEN** | FORWARD_TREE §4 |
| `006ED3F0` E8 sites | **1** (`004A71BF`) | `e8.tsv` |
| That site is Init World `"Init Combat Manager"` | **PROVEN** | `listing-00480000.txt` `004A7186` |
| RunModes / retail pump / Leave `E8` `006ED3F0` | **DISPROVEN** | only caller is `004A71BF` |
| `world+76` exists during frontend | **DISPROVEN** | world ctor `004A67D0` is after Leave |

**Answer:** no combat object, tick, or attack during frontend.

---

## 2. First combat object after Leave

`004A6E30` after Navigation / Global Console:

```
push "Init Combat Manager"
alloc 92 → 006ED3F0(world)
[world+76] = eax
006E8300([world+76])
```

### `006ED3F0` ctor **PROVEN**

| Offset | First-seen | Notes |
|---|---|---|
| +0 | vtbl `01261EB4` | near `"Deregistering combatant "` `01261EF0` |
| +4 / +8 | 24-byte sentinel, count 0 | `[sent+8]=[sent+12]=sent` |
| +16 / +20 | `0x88`-byte sentinel, count 0 | same empty ring |
| +28 | `1` | |
| +32 | world (`push esi` at `004A71BC`) | |
| +36, +37 | `0` | bytes |
| +40 | `0x40266666` ≈ **2.6f** | |
| +44 | `0` | |
| +56 | `008AF220` | empty handle at +56+16 |
| then | `008B0D10` | sequence table |

Dtor sibling `006ED480` restores the same vtbl then frees both
lists. RTTI name `CCombatManager` (`0x01380264`).

### `006E8300` immediately after ctor **PROVEN**

```
008AF2B0 on +56     // COMBAT_DIALOGUE_DEF_INSTANCE
[+48] = 0.5f
[+52] = 0
```

`008AF2B0` → `0044C6B0` player manager → `008AEE60` /
`009ADA40` name lookup. Miss would leave `[+56+12]=0`, then
`[ecx+72]` would fault. Init Definition Manager already ran, so
the named def is treated as **present**. Def *body* **UNREAD**.
RTTI `CCombatDialogueDef` exists (`0x01376E60`).

### `008B0D10` sequence table **PROVEN** bind / **DISPROVEN** as fight

Only E8 of `008B0D10` is the combat-manager ctor.
It `006EAB60`-registers `COMBAT_SEQUENCE_*` names with
code thunks `0x8B55xx`…`0x8B62xx`. First push is
`COMBAT_SEQUENCE_BANDIT_ATTACK_MIDDLE`. Tail
`jmp 008B08B0`. Families on this list: bandit, bandit king,
fodder, scorpion, troll, villager, will, whisper, Jack, two-strike.
**No** Lookout wasp name.

This is a **name → sequence factory** table, not a live
`CCreatureAction_*` or hit.

`006E8300` has **one** E8 (`004A71DB`). Not a pump.

---

## 3. First combat tick (`006ED200`)

`006ED200` has **one** E8: `004A5D14` inside `004A5A40`.

First type-1 WorldFrame (`[world+248]==0`, `[world+260]==0`)
reaches it **before** `004B4490` / `006E75C0`:

```
004A5D11  mov ecx, [esi+76]
004A5D14  call 006ED200
004A5D19  mov ecx, [esi+80]
004A5D1C  call 0051FCC0
…
004A5D82  call 004B4490
```

FORWARD_TREE §8 and host `TickWorld` comments skip this call.
The listing is the authority. **PROVEN** as first-seen site.

### Body first-seen **PROVEN** empty

```
esi = [this+16]          // 0x88 sentinel
[sentinel+8] == sentinel → je 006ED25B
jmp 008AF780(+56)
```

`008AF780`: `00A01B50(+16)` on the empty Thing handle from
`008AF220` → `eax=0` → skip to ret. No `008AF500`, no
`006E28A0`, no `004167C8` clock write.

So the first combat tick walks **zero** combatants and
resolves **zero** dialogue targets.

`006ED510` (`"Deregistering combatant "`) is **not** first-seen
(callers `006ED90C` / `006ED9D6` / `006EDA18` / `006EDAAC` only).

---

## 4. First attack? None.

### Script / action

| Claim | Class |
|---|---|
| First pumps enter `00CBFB7D` | **DISPROVEN** (`script-entity-cmds`) |
| `.PlayCombatAnim` `00CC15E3` / apply `00CC16FD` `vtbl+76` | **DISPROVEN** as Leave |
| `CActionPlayCombatAnimation` `009035F0` | name-setter only; **DISPROVEN** as first enqueue |
| Father `vtbl+76` `00834760` / player `006AD9D0` | leftover Oakvale |
| `FirstSeenPlayCombatAnimationAppliesPose` | **false** (`WorldSceneTests`) |
| `SetAttackable` `00CC0FB6` | **DISPROVEN** as Leave |

### Hero / Things

| Claim | Class |
|---|---|
| `006AC910` / `006A9DD0` adds `CTCCombat` by name | **DISPROVEN** (only `CTCPhysicsControlled` after `00662880`) |
| `00662880` pulls `CHeroCombatDef` / `CTCCombat` from `CREATURE_HERO` | **UNREAD** |
| `005B37F7` DEFAULT / `PlayAnimation` on create | **DISPROVEN** (`xseq-walk-first`) |
| Lookout 9 `AICreature`s are combat fodder | **DISPROVEN** (beggar / bully / villagers / traders) |
| Guild `CREATURE_RIVAL_HERO_*` fight on first Present | **DISPROVEN** (exist-only; not `0x20` submit) |
| `Q_WaspBoss` TNG section / Gameflow `WASP_BOSS` | **DISPROVEN** as first activate (`qst-first-quest`, Gameflow yield) |

`CTCCombat` appears only as a **type-name** helper
(`004D3533` / `00739CF0` in table `004D2EF0`). Size helper
nearby returns `0x30`. Factory ctor that would alloc that
slot on a Thing is **UNREAD**. Not first-seen on this spine.

### Input

| Claim | Class |
|---|---|
| `CInputProcessCombat` first-seen | **DISPROVEN** (RTTI `0x01378710` only; no `e8` / xref site) |
| Player Interface on frontend | **DISPROVEN** (`audit-playerinterface`) |
| `00446A30` on WorldFrame≤1 | **DISPROVEN** skip |
| first WorldFrame>1 `00446A30` | miss; no `0041649C` |
| Keyboard defaults slots 0–3 are melee | **DISPROVEN** (movement DIKs) |

**Answer:** first *attack* after Leave is not on the no-save
spine. First *combat system* work is an empty manager tick.

---

## 5. Leftover Oakvale “attack”

Do not follow these as first-after-Leave:

| Name | VA | Why leftover |
|---|---|---|
| `StartOakVale` / PreAttack | `00DBDE40` / `00DBE0C6` | only E8 `00DAC295` inside `00DABAC0` |
| 12s `vtbl+2584` | `00DBE134` | same quest |
| `HerosOldHouse` wait `+80` | `00DBE1FA` | `AttackOver` persist |
| PostAttack | `00DBE3C0` | `M_PostAttackStart` |
| `Father.PlayCombatAnim TURNING_AC90` | `00CC15E3` | `CS_OAKVALE_INTRO_FATHER` |
| `NewGameScript` / `AttackOver` | `00DAADA0` | façade over `S_QNOVI` |

`No_save_does_not_activate_Q_NewOakValeIntro` locks this.

---

## 6. C# vs native

| Site | Native | Host | Class |
|---|---|---|---|
| `InitWorldInitStages` `"Init Combat Manager"` `006ED3F0` | alloc + ctor + `006E8300` | `Note(apply, name)` only | **PARTIAL** name / **DIVERGE** object |
| `CCombatManager` fields / lists | 92-byte object `world+76` | no type | **DIVERGE** |
| `004A5D14` `006ED200` | every first WorldFrame | `TickWorld` omits it | **DIVERGE** |
| `008B0D10` sequence names | ctor bind | absent | **UNREAD** host |
| `PlayCombatAnimation` / `EntityDispatcher` | leftover runner | implemented | **LEFTOVER** vs Leave |
| `NewGameScript.AttackOver` | `S_QNOVI+80` | persist façade | **LEFTOVER** |
| `RegionTravel.PreAttackDuration=12` | Oakvale | constants | **LEFTOVER** |

---

## Classifications (short)

1. **Frontend combat — DISPROVEN.**
2. **First combat object after Leave — `CCombatManager` `006ED3F0` `world+76`. PROVEN.** Sequence / dialogue binds are tables, not a fight.
3. **First combat tick — `006ED200` on first `004A5A40`. PROVEN empty.**
4. **First attack / `CTCCombat` / `CInputProcessCombat` / `PlayCombatAnim` — DISPROVEN** as this spine.
5. **Oakvale PreAttack / PostAttack / `TURNING_AC90` — LEFTOVER.**
6. **Host — DIVERGE** (notes the Init World string, does not construct or tick the manager).
