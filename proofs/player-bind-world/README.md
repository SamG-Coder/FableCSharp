# `004AE9D0` PlayerBindAfterWorld

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `004AE9D0` / call site `0041891D`;
`src/Fable.Game/EngineLifecycle.cs`;
`src/Fable.Game/PlayerInterface.cs`;
`docs/runtime/FORWARD_TREE.md` §§7–8, 11;
`docs/PARITY.md` Init Game suffix / `0041674A` / `004AEAA0`;
`docs/status/investigations/E-player-palskin.md`;
`EngineLifecycleTests.InitGame_004184BD_after_00416953_reserves_then_user_ini`,
`CreatePlayers_004AE940_sets_plus9826_via_0099A350`,
`First_pump_0041674A_is_0_so_00418289_skips_00416E78`,
`Pump_004166E2_is_009E1BC0_minus_game_plus96`,
`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

---

## Verdict

**`004AE9D0` is not Hero.**

It is a **player-manager tick-slot sync** on `this = game+80568`.
When `[player+9826] != 0` it writes three dwords:

| Offset | Host | First-seen source | First-seen value |
| --- | --- | --- | --- |
| `+9836` | `PlayerBindSlot0` | `[game+72]` | ctor 0; later `max(+72, record+0)` |
| `+9840` | `PlayerBindSlot1` | `00416392` (WorldFrame path) | 0 at Init; WorldFrame after ticks |
| `+9844` | `PlayerBindSlot2` | `[game+90428]` | ctor 0 |

It does **not** allocate a Thing, look up `PLAYER_HERO` /
`CREATURE_HERO`, call `006AC910` `CThingPlayerCreature::Create`,
or bind `ScriptName=Hero`. Those are later, on first real region
(`LookoutPoint` / `GuildArrivalHSP`).

Calling it “bind after world” is only true as **clock bind**:
copy the post-`00416953` / post-`0041726D` world tick into the
player object so `0041674A` / `004AEAA0` / `004AEA70` can compare
`DisplayTime*15 − +9836`.

---

## Is this Hero?

**DISPROVEN.**

| Claim | Status |
| --- | --- |
| `004AE9D0` is Hero spawn / `006AC910` | **DISPROVEN** |
| `004AE9D0` is `PlayerInterface` (`004473A0` / `game+32`) | **DISPROVEN** |
| `004AE9D0` is Create Players (`004AE940`) | **DISPROVEN** — sibling on same object |
| `004AE9D0` writes tick slots on `game+80568` if `+9826` | **PROVEN** |
| Hero Thing exists at Init Game `004AE9D0` | **DISPROVEN** — `CurrentRegion==null`, `HeroSpawned` later |

Hero (host `EngineLifecycle.Hero`):

- `DefinitionType=CREATURE_HERO` (TLC `PLAYER_HERO` miss → `00449E0D`)
- `ScriptName="Hero"`
- mesh **4299**
- created in `LoadFromFirstRealRegion` / `0051FD80` → `006AC910`
- after Init Game and after first pumps; not at `0041891D`

`0044A3B0` (owner ctor used by Player Interface) is also **not**
hero_swap / spawn.

Slots 0–4 (`0x22C`, `0044A530` / `0044BC10`) are player **objects**,
not a HERO Thing. `E-player-palskin.md` already recorded this.

---

## When

### 1. Init Game suffix — once, after WLD, before first pump

`004184BD` after game vtbl+32 `00416953` (`LoadWorld`).
No-save `[0x13B8648]==0` only. Site **`0041891D`**.

```
0049BA70(game+90488, 60, 0)
00416392  (+90394==0 → 0049E200 / 0051E530+WorldFrame)
004AE9D0(game+80568)   if +9826: +9836/+9840/+9844
00999230 default_user.ini  (TLC miss)
009EC890 user.ini
004167DA / [game+90592]=1
```

Order vs Init Game stages:

1. `Init Player Manager` `0041732A`
2. `Init Player Interface` `004473A0` → `PlayerInterface` at **game+32**
3. `Init World` … `Create Players` `004166A8` → `004AE940` sets **`+9826=1`**
4. `00416953` load world (no-save, not a region)
5. **`004AE9D0`** — first bind, all three slots 0
6. first `004189C2` pump (`004AE9C0` touch only)

Not first pump. Not region load. `HeroSpawned` is still false.

Host: `FinishInitGameAfterWorld` inside `EnterGame` / first
`RequestNewGame` `Pump()`. Gate is `PlayerActionReady` (`+9826`).

### 2. After a consumed world tick — every `0041726D` hit

When `00418289` → `004AEBA0` (`+9826`) → `004AEAA0` and
`0041674A` returns 1:

```
004AEB3D  inc [esi+9836]
009F1720  [game+164]=0
009F16F0  one 0x648 record, record+0 = +9836 after inc
vtbl+24   00416E78
0041726D  009F1750 / 009F1730
          [+0] > [game+76]
          +76 = record+0
          +72 = max(+72, record+0)
004AE9D0  +9836 = +72   (+9840 = WorldFrame)
```

First-seen inner `004166E2` is `0*15-0 <= 1` → `004AEAA0` miss
(`004AEB8A`). So the **second** bind is not on the first
`004162B5`; it waits until display time grows (`Pump(0.1f)` /
`DisplayTime=1`).

Host: `AdvanceGameTicks` after `AppendPlayerCatchupTick`.

### 3. What it is *not* (when)

| Event | Same time as `004AE9D0`? |
| --- | --- |
| `00446A30` Player Interface pump | **No** — `00416E78` only after catchup, and only if `WorldFrame>1` |
| `004AE9A0` queue input `009F1650` | **No** — only if `00446A30` selects action 1/2 |
| `006AC910` Hero Thing | **No** — first real region |
| `00DBDE40` StartOakVale | **DISPROVEN** on this path |

---

## `PlayerInterface.cs`

Different object. `004AE9D0` is **not** in this file.

```
Init Player Interface 004473A0
  alloc 0x898; vtbl 01231BDC
  store at game+32
  0044A3B0 owner at game+28 (size 44)
  00488D20 / 00687A30 listener 0123758C
```

Pump (after `WorldFrame>1` inside `00416E78`):

```
004457F0  [+2196]=0
00446A30  vtbl+4
  00446330 poll → 00449990 / vtbl+16
  miss → 00446220 fallback [+168]
hit → 0041649C
  0049D8C0 occupied or action==2
    → 004AE9A0 +9826 → 009F1650 player+0x2010
  always 0049E1D0 / 00434A30
```

`PlayerInterface.ApplyPlayerFn = 0x004AE9A0` is the **sibling**
on `game+80568` (20 bytes before `004AE9D0`). It queues a
40-byte event. It does not write `+9836/+9840/+9844`.

`PlayerInterface` never stores Hero, mesh 4299, or world XYZ.

---

## Same `game+80568` cluster

`ecx` / `esi` = player object at **game+80568** (not game+32).

| VA | Host name | Role |
| --- | --- | --- |
| `004AE940` | `PlayerObjectInit` | Create Players: `0099A350` always 1 → `+9826=1` `+9824=1`; zeros `+9836` |
| `004AE9A0` | `PlayerInterface.ApplyPlayerFn` | queue if `+9826` |
| `004AE9C0` | `GamePumpPlayerFn` | first `004189C2` touch |
| `004AE9D0` | `PlayerBindAfterWorldFn` | this note |
| `004AEA70` | `PlayerReadyQueryFn` | `+9826==0` → 1; else `!0041674A(..., +9836)` |
| `004AEAA0` | `PlayerActionFn` | catchup; `inc +9836`; pack tick |
| `004AEBA0` | `GameUpdatePlayerFn` | `+9826` gate for update |

`+9836` is the catchup cursor. `004AE9D0` **resets it to
`[game+72]`** after world ticks; `004AEAA0` **increments** it
when display time is ahead.

---

## Host mapping

| Native | Host |
| --- | --- |
| `004AE9D0` | `EngineLifecycle.PlayerBindAfterWorldFn` |
| `0041891D` | `PlayerBindAfterWorldSite` |
| `+9826` | `PlayerActionReady` (set in `CreatePlayers`) |
| `+9836/+9840/+9844` | `PlayerBindSlot0/1/2` |
| Init write | `FinishInitGameAfterWorld` |
| Tick write | `AdvanceGameTicks` |
| Hero Thing | `SpawnHero` / `Hero` — **other path** |

---

## Open

- Exact x86 of `004AE9D0` (cmp `+9826`, three movs, any call
  besides reading `00416392` / `game+72` / `game+90428`) is
  **PARTIAL** in-repo (comments + host, no `fn-004AE9D0` dump).
- `004AE9C0` body **UNREAD** beyond “touch game+80568”.
- Whether `+9844` is ever nonzero on no-save first scene
  **UNREAD** (host leaves 0 after Init).
