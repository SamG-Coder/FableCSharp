# First `SetNewStart` / start-position script after Leave

Investigation only. No production `src/` edits.

Do **not** start at `NOVStartHSP` / `00DBDE40` / `FindPlayerStart`.
That ranking is leftover `Q_NewOakValeIntro` / wiki kid start,
not Leave / Init Game / first no-save Present.

Do **not** invent a `00CBFB7D` verb named `SetNewStart`.
It is not in `.rdata` `0x012C1500–0x012C2C00`, not in
`script.bin`, and not in `Fable.exe` strings.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `src/Fable.Game/RegionTravel.cs`,
`EngineLifecycle.cs` (`SpawnHeroFromPlayerStart` / `GuildArrivalHsp` /
`WorldPathAltGlobalVa`),
`ScriptCommandMap.cs`, `ScriptBank.cs`;
`docs/runtime/FORWARD_TREE.md` §§2, 7–10;
`docs/runtime/COMMAND_COVERAGE.md`;
`proofs/region-travel-first/README.md`,
`proofs/ini-activate-quest/README.md`,
`proofs/hero-stats-first/README.md`,
`proofs/tng-spawn/README.md`,
`proofs/script-entity-cmds/README.md`,
`proofs/script-global-cmds/README.md`;
`EngineLifecycleTests.Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`;
`WorldSceneTests.Lookout_main_start_and_active_exits_follow_ctcd_region_exit`;
ExeIndex `strings.tsv` / `xrefs.tsv` /
`listing-00400000.txt` `00413800`–`0041387F` /
`listing-00480000.txt` `00488B20` / `00489D40` / `0048CC60` /
`listing-00cc0000.txt` `00CCA320` /
`script-bank/entries-tsv.md` `CRegionScriptDef`.

---

## Verdict

**Leave does not execute a `SetNewStart` command.**

There is no such token. The start-position *name* after Leave
is a holy-site **TNG `ScriptName`**, not a runner verb.

| Question | Answer | Class |
|---|---|---|
| First `SetNewStart` after Leave? | **none** — name does not exist | **PROVEN** absence |
| First *store* of a start-position name? | `userst.ini` `SetStartingHolySite("NOVStartHSP")` → `[0x13B866C]` | **PROVEN** store; **DISPROVEN** as after Leave |
| First *lookup* after Leave? | `0049F180` → `00489D40` → **`00488B20`** (`[0x13B866C]` + `0048CC60`) | **PROVEN** call; first-seen **miss** |
| First start-position *script that poses Hero*? | Lookout `HOLY_SITE_PLAYER_START` **`GuildArrivalHSP`** | **PROVEN** after `00501450` |
| `RegionTravel.FindPlayerStart` as that pose? | **No.** Ranks `NOVStartHSP` then `MAIN_START_POSITION` | **LEFTOVER** |
| `CRegionScriptDef` `REGION_LOOKOUT_POINT` as that script? | 37-byte stub; registered, not run | **PROVEN** name; **DISPROVEN** as pose |
| First leftover *HSP verb* if the runner later starts? | `TeleportToHSP` (`0x012C1DCC`) | **PROVEN** leftover token; **DISPROVEN** as Leave |

`NOVStartHSP` / `MAIN_START_POSITION` / `FindPlayerStart` /
`StartingRegion` are **not** the no-save pose. **PROVEN**.

---

## Timeline (no-save New Game)

```
00402510 Parse Command Line
  00413C50 register SetLevel 00413800, SetStartingHolySite 00413840
  [0x1375444]!=0 → 00414C66 009EC890 userst.ini     // BEFORE frontend
    SetStartingHolySite("NOVStartHSP") → [0x13B866C]  // not a quest
0042EC7C retail / frontend 2D
  msg 15 → [retail+41]=1
0042F2A2 Leave frontend                              // not 00DBDE40
  0042F44D FinalAlbion.wld
0042F491 Init Game → 004184BD
  00416953 Load world
    00507C30 NewRegion 1 LookoutPoint
      RegionDef "REGION_LOOKOUT_POINT"               // name only
    0049F180 Init Characters                         // FIRST lookup
      00449D90 PLAYER_HERO miss → CREATURE_HERO
      0048A070 → 00489D40 → 00488B20
        0099B2C0 [0x13B866C]                         // NOVStartHSP if userst ran
        0048CC60 name walk
        miss → "*** WARNING : failed to find a holy site with ScriptName %S"
        [0x13B8647]==0 → ret 0                       // no 006AC910
    004B4260 START_INITIAL_QUESTS                    // no SetNewStart
    user.ini ActivateQuest("Gameflow")
004189C2 first pumps
  dummy WorldMap+156=0; CurrentRegion=null
later (E8 caller UNREAD)
  00501450 00500540(1,0,0) LookoutPoint
    0051FD80 HOLY_SITE_PLAYER_START
      GuildArrivalHSP / LookoutPointHSP / MAIN_START_POSITION
    006AC910 CREATURE_HERO ScriptName=Hero
      pose GuildArrivalHSP (52.688, 69.597, 36.982)
```

`SetNewStart` / `00CBFB7D` / `Hero.Teleport` / `NOVStartHSP` spawn
are **not** on this list. **PROVEN**.

---

## 1. `SetNewStart` is not a native verb

| Place | Hit? | Class |
|---|---|---|
| `strings.tsv` | no `SetNewStart` / `NewStart` | **PROVEN** absence |
| Interpreter pushes in `listing-00cc0000.txt` | `SetHomePosThing`, `SetFlag`, `SetTime`, … — no `SetNewStart` | **PROVEN** |
| Token window `0x012C1500–0x012C2C00` | 185 verbs; HSP-named is **`TeleportToHSP`** only | **PROVEN** |
| `script.bin` `entries-tsv.md` scrape | no `SetNewStart` | **PROVEN** |
| `ScriptCommandMap.NativeTokens` | no row | **PROVEN** host match |

Closest *named* stores / verbs:

| Name | Kind | When vs Leave |
|---|---|---|
| `SetStartingHolySite` | ini / console, handler `00413840` | **before** frontend |
| `SetLevel` | sibling store `[0x13B8668]` | **before** frontend |
| `TeleportToHSP` | global runner token `0x012C1DCC` | leftover later (`CS_ACTIVATE_TELEPORTER_TWO`) |
| `SetHomePosThing` / `ResetPos` | runner apply `00CC7D3C` / `00CC4AC3` | leftover; runner not on Leave tree |
| `Teleport` / `TeleportThing` | runner | leftover father / later |

Do not alias `SetNewStart` onto those. **PROVEN** names;
pairing as `SetNewStart` is **DISPROVEN**.

---

## 2. First store: `SetStartingHolySite`, not after Leave

`00413C50` (Parse Command Line) registers:

```
00413D55  push "SetStartingHolySite"
00413D8F  [esi+20] = 00413840
```

`00413840` is the sibling of `SetLevel` `00413800`:

```
00413800  copy arg CString → [0x13B8668]   // WLD override
00413840  copy arg CString → [0x13B866C]   // holy-site ScriptName
```

TLC `userst.ini` has `SetStartingHolySite("NOVStartHSP")`.
Applied at `00414C66` when `[0x1375444]!=0` (PE 1).
**PROVEN** (`proofs/ini-activate-quest`).

That is **not** Leave. Frontend message 15 / `0042F2A2` never
re-registers or re-runs it.

`EngineLifecycle.WorldPathAltGlobalVa = 0x013B866C` comments the
slot as a WLD path fallback (`updatedscenic.wld` chain). Native
`00416953` uses `[0x13B8668]` (`SetLevel` / `WorldPathGlobalVa`)
for the filename, **not** `+866C`. Treating `+866C` as a world
path is **LEFTOVER** vs `00413840`.

`hero-stats-first` saying `[0x13B866C]` is empty first-seen
**DIVERGE**s vs `userst.ini` if that file ran. The *lookup*
still **misses**: Oakvale `NOVStartHSP` is not in the holy-site
list at `0049F180`. Empty vs `"NOVStartHSP"` both yield
`00488C0A` warning. **PROVEN** miss; stored string
**PARTIAL** (depends on `userst` apply).

---

## 3. First lookup after Leave: `00488B20` miss

Only `E8` of `00488B20` is `00489D65` inside `00489D40`
(`CreateCharacter`). Only `E8` of `00489D40` is `0048A0AF`
inside `0048A070`. After Leave that is `0049F180` /
`00449E2D` (`PLAYER_HERO` miss → `CREATURE_HERO`).

```
00488B20
  0048D5C0                 // collect candidate holy sites
  [esi+244]!=0 → skip name
  0099B2C0 [0x13B866C]     // SetStartingHolySite CString
  0048CC60(name)           // pointer walk, predicate at [esp+40]
  hit  → store thing, al=1
  miss → push "*** WARNING : failed to find a holy site with ScriptName %S"
         zero [esi+232/+236/+240]
  else nearest-site walk vs [esi+232] (004C73D0 pos)
00489D40
  test al
  mov al, [0x13B8647]
  miss && [0x13B8647]==0 → ret 0     // first-seen; no 006AC910
  hit  && [0x13B8647]==0 → 006A4D00 pose from site (00489E21)
```

`0x13B8647` has no first-seen in-repo writer. **PROVEN**
early-out on this site. Retry after Lookout TNG is
**UNREAD** (`hero-stats-first`). Host folds `0049F180` Notes
into `LoadFromFirstRealRegion` — **LEFTOVER** vs the Load
World call.

---

## 4. First start-position *script* that poses Hero

After dummy pumps, `00501450` loads native index 1
`LookoutPoint`. ContainsMap TNG has three
`HOLY_SITE_PLAYER_START`:

| ScriptName | Approx XYZ | No-save pose? |
|---|---|---|
| **`GuildArrivalHSP`** | 52.688, 69.597, 36.982 | **yes** |
| `LookoutPointHSP` | 102.781, 74.156, 37.494 | no |
| `MAIN_START_POSITION` | 102.887, 74.127, 37.488 | no |

`NOVStartHSP` / `StartOakValeHSP` are **not** in Lookout TNG.
They live on later Oakvale maps. **PROVEN** file.

Host `SpawnHeroFromPlayerStart` prefers `GuildArrivalHSP`,
then any positioned `HOLY_SITE_PLAYER_START`. Create is
`006AC910` / `CREATURE_HERO` / mesh **4299** / `ScriptName=Hero`.
**PROVEN** (`0051FD80` test; first-scene dump).

That is the first start-position **script name** consumed
for a live Hero after Leave.

---

## 5. `RegionTravel` leftovers

```csharp
public const string NewGameStartScript = "NOVStartHSP";
public const string MainStartScript = "MAIN_START_POSITION";

public static ThingInstance? FindPlayerStart(...) =>
    Named(starts, NewGameStartScript)      // NOVStartHSP
    ?? Named(starts, "StartOakValeHSP")
    ?? Named(starts, MainStartScript)      // MAIN_START_POSITION
    ?? Named(starts, "LookoutPointHSP")
    ?? starts.FirstOrDefault();
// GuildArrivalHSP is not in this list
```

| Call | Result | Class vs Leave |
|---|---|---|
| `FindPlayerStart(StartOakValeWest)` | `NOVStartHSP` | **LEFTOVER** Oakvale intro |
| `FindPlayerStart(LookoutPoint)` | `MAIN_START_POSITION` | **LEFTOVER** vs live `GuildArrivalHSP` |
| `WorldSceneTests.Lookout_main_start_*` | locks `MAIN_START_POSITION` | **LEFTOVER** vs spawn |
| `EngineLifecycle.SpawnHeroFromPlayerStart` | `GuildArrivalHSP` | **PROVEN** pairing |
| Type header “Kid start is WLD StartOakVale…” | Oakvale first | **LEFTOVER** vs FORWARD_TREE |

`FirstSceneWorld` / `WorldGeometry.Build` still call
`FindPlayerStart`. Live New Game does not. **DIVERGE** if
those façades are treated as Leave.

---

## 6. `CRegionScriptDef` is not a start-position script

`00F2A0F0` registers `CScriptDef` / `CCutsceneDef` /
`CRegionScriptDef` (`00F29FA0`) during Loading world after
Leave. **PROVEN** register (`script-bank-open`).

First authored region def in `script.bin`:

| Index | Instance | Raw | Commands |
|--:|---|--:|---|
| 2 | `NULLDEF_CRegionScriptDef` | 37 | empty |
| 598 | **`REGION_LOOKOUT_POINT`** | 37 | empty |
| 599–610 | other `REGION_*` | 37 | empty |

WLD `NewRegion 1` `RegionDef "REGION_LOOKOUT_POINT"` matches
that name. Persist / run of `CRegionScriptDef` is **UNREAD**.
Empty scrape is **not** a `SetNewStart` line. Do not treat
the def as the Hero pose. **DISPROVEN**.

---

## 7. Leftover HSP verb: `TeleportToHSP`

Token `0x012C1DCC`. Parse site `00CCA320` (after `PlayAVI`).
Apply **UNREAD** (`COMMAND_COVERAGE`). One `script.bin` line:

`CS_ACTIVATE_TELEPORTER_TWO`: `TeleportToHSP GWTeleportHSP`

Not on Leave / Init Quests / first pumps. **PROVEN** leftover.

Father leftover pose is `Hero.Teleport MK_OVI_ID_HERO`, not
an HSP verb. **PROVEN** leftover (`script-entity-cmds`).

---

## Classifications (short)

1. **`SetNewStart` after Leave — DISPROVEN.** No exe string,
   no runner token, no `script.bin` line.
2. **First start-name *store* — `SetStartingHolySite("NOVStartHSP")`
   at command line. PROVEN.** Not Leave. Slot `[0x13B866C]`.
3. **First start-name *lookup* after Leave — `00488B20` during
   `0049F180`. PROVEN miss.** No `006AC910` on that site.
4. **First start-position *script* that poses Hero —
   `GuildArrivalHSP`. PROVEN** after `00501450`.
5. **`RegionTravel.FindPlayerStart` / `NOVStartHSP` /
   `MAIN_START_POSITION` as that pose — LEFTOVER.**
6. **`CRegionScriptDef` `REGION_LOOKOUT_POINT` as pose —
   DISPROVEN.** Stub name only.
7. **`00501450` caller / post-TNG `00488B20` retry — UNREAD.**
