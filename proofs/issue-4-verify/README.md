# Issue #4 vs HEAD — first-scene ledgers still say Oakvale intro

Investigation only. Production `src/` and `tests/` were not edited.

GitHub: [SamG-Coder/FableCSharp#4](https://github.com/SamG-Coder/FableCSharp/issues/4)
(open). Title: *No-save New Game is Lookout/GuildArrival; first-scene
ledgers still say Oakvale intro.*

Workspace HEAD: `ee08490` (`Add CCreatureNavigationDef after
CTimeAppearanceFadeDef during Init Thing Components.`). Status
snapshot freeze in `docs/status/README.md` is still `4a03969`;
that freeze is older than this HEAD and does not change the
Lookout vs Oakvale pairing.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER**.

**Oakvale as no-save New Game is DISPROVEN.** Do **not** invent a
`PlayerRegionName=StartOakVale` write. Do **not** wire
`FirstSceneWorld` onto `Pump` / `SubmitCurrentWorld`.

---

## Verdict vs HEAD

**STILL OPEN.**

Runtime no-save New Game is already **LookoutPoint +
`GuildArrivalHSP` + adult `CREATURE_HERO` / mesh 4299**. That half
of the issue is locked. The issue’s complaint is the *ledgers*:
`docs/render/FIRST_SCENE_*.md` and the status north star still
title Oakvale house / SHOT2 / kid as “first-seen New Game”.

`docs/status/README.md` added a *correction* (Lookout first
region vs Oakvale intro *view*). That is **PARTIAL** language
only. The authoritative first-scene files were not retitled.
GitHub #4 is still Open.

| Claim in #4 | HEAD | Class |
|---|---|---|
| No-save first region is LookoutPoint (WLD index 1) | `00501450` → `00500540(1,0,0)`; `FirstSceneMapName` | **PROVEN** |
| Spawn is `GuildArrivalHSP` → `006AC910` Hero 4299 | `SpawnHeroFromPlayerStart` / Lookout test | **PROVEN** |
| No-save is not `00DBDE40` / `CREATURE_HERO_CHILD` / StartOakVale | same test `DoesNotContain` | **DISPROVEN** (old pairing) |
| No-save does not activate `Q_NewOakValeIntro` | `No_save_does_not_activate_Q_NewOakValeIntro` | **PROVEN** |
| `FIRST_SCENE_CONTRACT.md` still says first-seen New Game is Oakvale | file still opens that way | **LEFTOVER** — issue still open |
| Status north star still names Oakvale as first-scene New Game | `docs/status/README.md` + `index.html` | **LEFTOVER** |
| Status correction splits Lookout vs intro view | present under “Correction vs a first region is Oakvale reading” | **PARTIAL** (status only) |
| Who writes persist `PlayerRegionName` on New Game | empty on no-save; HEADER writer unread | **UNREAD** — do not invent |
| `BindLifecycleFirstRegion` skip `StartOakVale` | **no such symbol** in `Fable.Client` | **LEFTOVER** mention; live client is `EngineLifecycle.Pump` only |

Overall: **STILL OPEN** (ledger pairing). Runtime Lookout path is
not the leftover.

---

## What #4 asked

Issue body (2026-08-18, still Open):

1. Prove who writes persist `PlayerRegionName` on retail
   `UI_TEXT_NEW_GAME` (msg 15 / `[retail+41]`). Until that write
   is found, do **not** treat Lookout/GuildArrival as the intro
   cutscene.
2. Split language in `FIRST_SCENE_CONTRACT.md` and `docs/status/`:
   **no-save first region** vs **intro view contract**. Stop
   calling both “first scene” without a qualifier.
3. Keep the client from feeding Oakvale house meshes into the
   Lookout lifecycle. Do **not** invent
   `PlayerRegionName=StartOakVale`.

Item 1 is still **UNREAD**. Item 2 is the open work. Item 3 is
already true of the live path (`FirstSceneWorld` has zero
production callers).

---

## Runtime at HEAD (Lookout / GuildArrival)

`EngineLifecycle` does not start at Oakvale.

```
msg 15 → [retail+41]=1 → Leave 0042F2A2
  FinalAlbion.wld                         // not StartOakValeWest
0042F491 Init Game → 00416953 LoadWorld
  00CD6E27 bind Q_NewOakValeIntro only    // not 00CB5AD0
  Gameflow 00CE7670 waits, yields
00501450 RequestLoadRegion(1)             // LookoutPoint
006C2170 / 00521AE0 LookoutPoint.tng
  no PlayerCreature
  HOLY_SITE_PLAYER_START GuildArrivalHSP
0049F180 / 00449D90 PLAYER_HERO miss
  CREATURE_HERO → 00489D40 / 006AC910     // mesh 4299
FirstSceneMapName = LookoutPoint
SubmitCurrentWorld → 006B3FF0 hero camera
```

Quoted from `src/Fable.Game/EngineLifecycle.cs`:

```2734:2738:src/Fable.Game/EngineLifecycle.cs
    /// <summary>
    /// Map that owns <see cref="Hero"/> —
    /// LookoutPoint <c>GuildArrivalHSP</c>
    /// on no-save, not StartOakVale.
```

```6976:7017:src/Fable.Game/EngineLifecycle.cs
    /// No-save LookoutPoint has no
    /// <c>PlayerCreature</c> NewThing. Native
    /// start marker is <c>HOLY_SITE_PLAYER_START</c>
    /// <c>GuildArrivalHSP</c>. Create is
    /// <c>006AC910</c> …
    private void SpawnHeroFromPlayerStart(IReadOnlyList<ThingInstance> things)
    {
        …
        var start = starts.FirstOrDefault(t =>
                        string.Equals(t.ScriptName, GuildArrivalHsp, …))
        …
        FirstSceneMapName ??= CurrentRegion?.RegionName;
        SpawnHero(start, bindExisting: false);
    }
```

```6430:6434:src/Fable.Game/EngineLifecycle.cs
    /// <c>00500540</c> then <c>006C27A0</c> /
    /// <c>006C2120</c>. Does not invent
    /// StartOakVale.
```

`EnqueueAfterDummy` reads persist only when
`PlayerRegionName` is already nonempty. No-save leaves it
empty and calls `LoadFromFirstRealRegion()` (index 1). That
is **not** a New Game write of `StartOakVale`.

Locking tests (not edited here):

- `Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`
  — `FirstSceneMapName == LookoutPoint`, `GuildArrivalHSP`,
  Hero def `CREATURE_HERO`, mesh **4299**, **no**
  `CREATURE_HERO_CHILD`, **no** `00DBDE40`, **no**
  `Q_NewOakValeIntro` / `S_QNOVI` activate.
- `No_save_does_not_activate_Q_NewOakValeIntro`
- `Loading_objects_00521AE0_loads_LookoutPoint_tng`
- `New_game_is_leave_frontend_then_FinalAlbion_wld`
- `Persist_PlayerRegionName_is_00487C20_not_new_game`

`src/Fable.Client/Program.cs` constructs `EngineLifecycle`,
`Bootstrap`, `Pump`. There is **no**
`BindLifecycleFirstRegion`. Status still cites that name
from `fe6a11e`; at this HEAD it is a stale client-symbol
note. Live 3D after Leave is `SubmitCurrentWorld`, not
`FirstSceneWorld.Build`.

---

## First-scene ledgers still say Oakvale is New Game

These files still pair Oakvale with “first-seen New Game” /
“First New Game”. That is the open half of #4.

### `docs/render/FIRST_SCENE_CONTRACT.md`

```
First-seen New Game is `StartOakValeWest` / `Q_NewOakValeIntro`,
camera `CAM_OVIF_SHOT2`, inside `HerosOldHouse`.
```

ASSETS table still: Region = `StartOakValeWest` via
`RegionTravel.StartingRegion`.

SHOT2 / house 6909 / kid 4300 remain a real **intro view**
fixture. Titling that fixture “First-seen New Game” is the
**LEFTOVER**.

### `docs/render/FIRST_SCENE_WORLD_PARITY.md`

```
First New Game is `StartOakValeWest` / `CAM_OVIF_SHOT2` /
`HerosOldHouse`.
```

REGION row still **PROVEN**:

```
New Game map is `StartOakValeWest` (3456, 736), not Lookout
```

and

```
Adult Lookout is not first scene | `00DBDE40` StartOakVale
```

Lookout-as-first-region is **DISPROVEN** as “not first scene”.
`00DBDE40` is the later intro map-wait, not no-save Present.

### `docs/render/WORLD_SPACE_CONTRACT.md`

```
First New Game / Oakvale: `StartOakValeWest` / `Q_NewOakValeIntro` /
`CAM_OVIF_SHOT2` / `HerosOldHouse`.
```

Live functions named: `WorldSpaces` and `FirstSceneWorld`.

### `docs/render/FIRST_SCENE_AUDIT.md`

Trace is still SHOT2 helper → kid bind-pose → house/landscape
layers. No Lookout / GuildArrival / 4299. Fine as an intro-view
submit audit; leftover if read as no-save Present.

### `docs/status/README.md` / `index.html`

North star (both files):

```
close first-scene New Game
(`StartOakValeWest` / `Q_NewOakValeIntro` / `HerosOldHouse` /
`CAM_OVIF_SHOT2`) so `CS_OAKVALE_INTRO_FATHER` can run on a real
world clock.
```

Done table still:

```
New Game *view* is `StartOakValeWest` / `HerosOldHouse` /
`CAM_OVIF_SHOT2`, not Lookout | PROVEN
```

The later **correction** is the only split:

```
No-save New Game’s first real write is 1 = LookoutPoint.
… First-scene *intro view* is still StartOakValeWest /
HerosOldHouse / CAM_OVIF_SHOT2 / kid (FIRST_SCENE_* — do not
collapse into Lookout). Persist name StartOakVale is index 4.
Who writes persist PlayerRegionName is still UNREAD.
```

That correction is why this is not a runtime reopen. It does
**not** close #4: the contract files and the north star still
use the un-qualified “first-scene New Game” title.

What’s left still says:

```
Wire persist-Oakvale (or a proven New Game region write) to
`FirstSceneWorld` | UNREAD
```

and

```
Do not invent an Oakvale write on the no-save path.
```

Agree with the second sentence. The first row must not be
satisfied by inventing `PlayerRegionName`.

---

## Host leftover that still *names* Oakvale New Game

Not edited. Quoted so the next docs pass can retitle them.

`src/Fable.Game/FirstSceneWorld.cs`:

```
/// First New Game Oakvale world built the same way as
/// <c>Fable.Client</c>: <c>StartOakValeWest</c>, SHOT2 helper,
```

`FirstSceneWorld.Region = RegionTravel.NewGameRegion`
(`StartOakValeWest`). Client never calls `Build`. Pairing
“same way as Fable.Client” is **DISPROVEN**
(`proofs/audit-firstsceneworld`).

`src/Fable.Game/RegionTravel.cs`:

```
/// Kid start is WLD region StartOakVale / map StartOakValeWest
/// …
/// WLD Maps[0] LookoutPoint is the adult overworld first map,
/// not new-game.
public const string NewGameRegion = "StartOakValeWest";
```

`FindPlayerStart` prefers `NOVStartHSP` / `StartOakValeHSP`
before `LookoutPointHSP`. Live no-save spawn does **not** use
this helper; `SpawnHeroFromPlayerStart` prefers
`GuildArrivalHSP`.

`src/Fable.Formats/Wld/WorldFile.cs`:

```
/// New-game Oakvale is region `StartOakVale`, not `Maps[0]`.
```

`src/Fable.Formats/Scene/ScenePass.cs`:

```
/// First-seen bits recovered for New Game Oakvale.
```

`WorldSceneTests` still asserts
`RegionTravel.StartingRegion(world) == "StartOakValeWest"`.
That is the intro-map helper, not no-save index 1.

---

## PlayerRegionName — do not invent

`00487C20` / `00449E60` loads a **named persist** region
(example `StartOakVale` = 4). Empty on no-save.

```2803:2806:src/Fable.Game/EngineLifecycle.cs
    /// Persist <c>PlayerRegionName</c>. Empty on
    /// no-save New Game. Non-empty takes
    /// <c>00487C20</c> instead of <c>00501450</c>.
```

```2079:2084:src/Fable.Game/EngineLifecycle.cs
    /// Caller <c>00449E60</c> reads persist
    /// <c>PlayerRegionName</c> (HEADER) —
    /// continue, not no-save New Game.
```

Who writes the HEADER on a retail New Game click is
**UNREAD**. #4 item 1 is that unread writer — not a host
assignment. Forcing Oakvale by setting `PlayerRegionName`
would reopen the DISPROVEN pairing.

---

## Two contracts (keep both; stop collapsing the names)

| Qualifier | Map / spawn | Camera | Hero | When |
|---|---|---|---|---|
| **No-save first region** (live New Game) | LookoutPoint, index **1** | `006B3FF0` / helper FOV **70** | adult 4299 at `GuildArrivalHSP` | after Leave, first 3D Present |
| **Intro view contract** (later / reconstructed) | `StartOakValeWest`, persist index **4** | `CAM_OVIF_SHOT2` FOV **72** | kid 4300 in `HerosOldHouse` | `Q_NewOakValeIntro` / `00DBDE40` after a proven activate — **not** no-save |

Gameflow **waits** on `Q_NewOakValeIntro` and **yields**. Bind
`00CD6E27` is not activate.

---

## Proposed next step

1. **Retitle the first-scene ledgers** (`FIRST_SCENE_CONTRACT.md`,
   `FIRST_SCENE_WORLD_PARITY.md`, `WORLD_SPACE_CONTRACT.md`,
   status north star / `index.html`): say **intro view contract**,
   not “first-seen New Game”. Point no-save Present at Lookout /
   `GuildArrivalHSP` / `EngineLifecycle.FirstSceneMapName`.
2. **Retitle host comments** on `FirstSceneWorld` /
   `RegionTravel.NewGameRegion` / `WorldFile.FindRegionContaining`
   the same way. Keep the Oakvale soup as a fixture; drop
   “same as Fable.Client”.
3. Leave persist `PlayerRegionName` **UNREAD**. Do not write
   `StartOakVale` on the no-save path to feed `FirstSceneWorld`.
4. Do not file a new runtime issue for Lookout spawn. That is
   already PROVEN. Close #4 only after the ledger titles match
   HEAD.

No `src/` / `tests/` change in this proof.
