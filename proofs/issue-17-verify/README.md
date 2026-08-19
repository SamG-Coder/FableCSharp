# GitHub #17 vs HEAD — Init GUI `0043A380` / `PlayerGuiReady`

Investigation only. No `src/` or `tests/` edits.
Do **not** start Oakvale / `S_QNOVI` / `00DBDE40` /
`Q_NewOakValeIntro` / `CREATURE_HERO_CHILD`.

Issue: <https://github.com/SamG-Coder/FableCSharp/issues/17>
(open, 0 comments). Title: *Init GUI 0043A380 is
Note-only; PlayerGuiReady is set true*.

HEAD: `3a7b594` (`master`). Authority:
`src/Fable.Game/EngineLifecycle.cs`
`LoadWorld` / `InitCharactersAndQuests` /
`CreatePlayers` / `TickPlayerGui`;
`tests/Fable.Formats.Tests/EngineLifecycleTests.cs`
`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`
/ `LoadWorld_00416953_no_save_is_004A1840_then_0049F180`;
`docs/status/README.md` row for `0049F180` Init GUI;
siblings `proofs/init-gui-0043A380`,
`proofs/0049F180-first-children`,
`proofs/004166A8-create-players-work`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH** / **STALE**.

---

## Verdict vs HEAD

**PARTIAL.** The issue is still the live leftover.

`0043A380` is still a `Note` plus `PlayerGuiReady = true`.
There is still no `PLAYER_GUI_PC` bank bind, no live
object at `PlayerGuiSingleton` `[0x13B878C]`, no
`0043B570` ctor, no `0043A380` reset/recopy, and no
HUD submit. Tests still lock the flag.

Two of the original issue *symptoms* are gone
(stale names / a wrong `0044C6B0` Note). Done-looks
#1 is **not** done. GitHub #17 stays open.

| Issue claim / done-looks | HEAD | Class |
|---|---|---|
| Init GUI is Note-only | `Note(InitGuiFn, …)` then flag | **STILL OPEN** |
| `PlayerGuiReady` set without bind | `= true` immediately after that Note | **STILL OPEN** |
| No `[0x13B878C]` destination | `PlayerGuiSingleton` is a const only | **STILL OPEN** |
| No HUD submit | client Present is world/AVI | **STILL OPEN** |
| Tests lock `PlayerGuiReady` | Lookout test + LoadWorld test | **STILL OPEN** |
| Do not Note `0044C6B0` as GUI bank | that Note is gone | **FIXED** |
| `InitFirstSceneAfterCharacters` after Lookout | method gone; `InitCharactersAndQuests` from `LoadWorld` **before** Lookout TNG | **STALE** |
| Init Quests honest unread | `QuestsInitDone = true`; world+172 walked | **STALE** (sibling; not this GUI leftover) |
| Do not start `S_QNOVI` / `00DBDE40` | still skipped | **MATCH** |

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Does `InitFirstSceneAfterCharacters` exist? | **No.** Zero hits. Work is `InitCharactersAndQuests`. | **STALE** name |
| When is `PlayerGuiReady` assigned? | `LoadWorld` → `InitCharactersAndQuests` during Init Game, **before** `LoadFromFirstRealRegion` Lookout objects | **PROVEN** |
| Is Init GUI more than a Note? | **No.** `Note` is `Trace.Add` only. | **LEFTOVER** |
| `0043A380` native first-seen? | Reset on live `[0x13B8790]`, not ctor (`init-gui-0043A380`) | **PROVEN** |
| Who constructs the GUI? | Create Players `00487FB0` → `0043B570` → `004195AF` `[0x13B8790]` | **PROVEN** native; host **LEFTOVER** |
| Host `CreatePlayers` Notes `0043B570`? | **No.** Slots + `004AE940` only. | **LEFTOVER** |
| Is `PlayerGuiReady` the ctor? | **No.** Flag after the reset site. | **DISPROVEN** as ctor |

---

## 1. Issue text (18 Aug) vs current symbols

Issue body quoted:

```
Note(InitGuiFn, "LevelLoader", "UI",
    "0043A380 Init GUI PLAYER_GUI_PC [0x13B878C]");
Note(PlayerManagerGetter, "LevelLoader", "UI", "0044C6B0 PLAYER_GUI_PC");
PlayerGuiReady = true;
```

HEAD `InitCharactersAndQuests` (`EngineLifecycle.cs`):

```
private void InitCharactersAndQuests()
{
    Note(InitCharactersFn, "Init Characters", "Player",
        "0049F180 push 0 ecx=world");
    Note(PlayerCreatureBindFn, "Init Characters", "Player",
        "00449970 / 00487DC0");
    Note(InitGuiFn, "Init GUI", "UI",
        "0043A380 PLAYER_GUI_PC [0x13B8790]");
    PlayerGuiReady = true;
    …
    QuestsInitDone = true;
}
```

`Note` (`EngineLifecycle.cs`):

```
private void Note(uint va, string stage, string subsystem, string action) =>
    Trace.Add(va, stage, subsystem, action);
```

What changed since the issue:

- Stage `"LevelLoader"` → `"Init GUI"`.
- Action now names instance `[0x13B8790]`, not def `[0x13B878C]`.
- Second Note of `PlayerManagerGetter` / `0044C6B0` as GUI **removed**.
- Method name is `InitCharactersAndQuests`.

What did **not** change: the ready flag still flips on a
trace line.

Constants (unchanged meaning):

```
PlayerManagerGetter = 0x0044C6B0   // [0x13B879C]
PlayerGuiSingleton  = 0x013B878C   // compiled def dest
InitGuiFn           = 0x0043A380
InitCharactersFn    = 0x0049F180
PlayerGuiTickFn     = 0x0043A080
```

`PlayerGuiSingleton` has **no** other `src/` use.
No field stores a def pointer. **PROVEN** unused.

`0044C6B0` Notes that remain (`Init Player Manager`,
`Init Thing Components`, `Init Definition Manager`,
`Create Players`, `GamePump`) all say `[0x13B879C]`.
Done-looks #2 **FIXED**.

---

## 2. Order: not after Lookout objects

Issue: “After Lookout objects, `InitFirstSceneAfterCharacters`…”

HEAD `EnterGame` / Init Game ends with `LoadWorld()`
**before** game-mode pumps and **before** first-region TNG:

```
if (name == "Create Players")
    CreatePlayers();
…
LoadWorld();
```

`LoadWorld`:

```
LoadWorldMap();                    // WLD / QST / Startup WAD / static map
if (SkipParticlesFirstSeen == 0)
{
    Note(SkipParticlesVa, … "013B8648=… 0049F180");
    InitCharactersAndQuests();     // sets PlayerGuiReady
}
```

Lookout objects are `ApplyLoadJob` / `LoadRegionMapThings`
from `LoadFromFirstRealRegion` (`00501450`). That is a
later call (`Load_single_thing_0051FD80_…` does
`Pump(); Pump(); LoadFromFirstRealRegion();`).

Native order matches the host **site**, not the issue
sentence: `00416953` → `004A1840` → `[0x13B8648]==0` →
`0049F180` **then** later `006C2170` Loading objects.
See `docs/runtime/FORWARD_TREE.md` and
`EngineLifecycleTests.LoadWorld_00416953_no_save_is_004A1840_then_0049F180`
(`initChars > use`, `PlayerGuiReady` true, **empty**
`ActivatedQuests` because no install / no region).

“After Lookout objects” is **STALE**. The leftover is
still the flag.

---

## 3. Native `0043A380` (still not implemented)

`0049F180` (`listing` / `0049F180-first-children`):

```
0049F20E  mov ecx, [0x13B8790]
0049F214  call 0043A380
```

Only `.text` `E8` of `0043A380`. No null test.
First-seen work is **reset + recopy**, not construct
(`proofs/init-gui-0043A380`):

- `00492BAB(0)` on `this+24`
- `00647319` clear `this+456`
- five meter `[+8]=0` at `+716`…`+748`
- if `[0x13B878C]==0` bind `"PLAYER_GUI_PC"` via
  `0044C6B0` / `0043FF30` / `009ADA40` (first-seen New
  Game **skips**: ctor already stored the def)
- `00442770` recopy into `+608` / `+620`
- `[this+424]+48=0`, `[this+657]=1`

Ctor is Create Players `00487FB0` → alloc `0x338` →
`0043B570` (vtbl `0123177C`, meters `0065431D`…) →
`004195AF` store `[0x13B8790]`.

Host `CreatePlayers` Notes `0044C6B0` / `0044A530` /
`0044BC10` / `004AE940` only. No `00487FB0` /
`0043B570` / `004195AF`. **LEFTOVER** vs first
construct.

Host `TickPlayerGui`:

```
Note(PlayerGuiTickFn, "GamePump", "UI", "0043A080 +164=0");
PlayerGuiTicked = true;
```

Same Note-only pattern on the tick.

---

## 4. Tests still treat the site as done

`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`:

```
Assert.Contains(life.Trace.Events, e => e.Va == EngineLifecycle.InitGuiFn);
…
Assert.True(life.PlayerGuiReady);
```

`LoadWorld_00416953_no_save_is_004A1840_then_0049F180`:

```
Assert.True(life.PlayerGuiReady);
```

`docs/status/README.md`:

> `0049F180` after characters: Init GUI `0043A380`
> `PLAYER_GUI_PC` | PROVEN | `21491ac` / same test
> (`PlayerGuiReady`); bind still #17 / PARTIAL (Note-only)

Status row already names this leftover. Tests lock the
**call site**, not a bind.

`src/Fable.Client/Program.cs` has no
`BindLifecycleFirstRegion` (issue “Where” is **STALE**).
`SilkEngineHost.Present` is AVI / world submit. No HUD.

Init Quests in the same function is no longer unread
(`QuestsInitDone = true`, `_worldPlus172` /
`WorldQuestListOffset = 172`). Issue done-looks #3
(“leave Init Quests unread”) is **STALE**. The path
still excludes `Q_NewOakValeIntro` / `S_QNOVI` /
`00DBDE40` (**MATCH**). Do not reopen Oakvale from #17.

---

## Leftover (still open)

1. `PlayerGuiReady = true` with no `PLAYER_GUI_PC` dest
   at `[0x13B878C]` and no instance at `[0x13B8790]`.
2. `0043A380` body (reset / meter `+8` / vector recopy)
   not hosted.
3. Create Players ctor `0043B570` / `004195AF` not hosted
   (prerequisite for a real first-seen reset).
4. Tests assert the flag as if HUD ran.
5. `TickPlayerGui` Note-only (`0043A080`).

Not leftover for **this** issue: frontend `#14`, Init
Sound `#15`, Init Quests activate, Lookout hero mesh.

---

## Proposed next step

Do **not** build HUD widgets at `0043A380`. Native
first-seen constructs them in `0043B570`.

Pick one honest close for #17 done-looks #1:

1. **Stop lying:** drop `PlayerGuiReady = true` (and
   the tests that require it) until a dest exists.
   Keep the `Note(InitGuiFn)` as a recovered **site**.
2. **Bind first, then reset:** host Create Players
   `00487FB0` / `0043B570` / `004195AF` so
   `[0x13B8790]` is live and `[0x13B878C]` has the
   compiled def; then implement the `0043A380` reset
   (no `00BFEA1A`, no `0041BEB0`) and only then set
   the flag.

Keep `0044C6B0` as player-manager getter. Do not start
Oakvale from this path.

---

## Classification

**PARTIAL vs HEAD `3a7b594`.** GitHub #17 remains
open. Site recovered; bind / reset / HUD still
Note-only leftover.
