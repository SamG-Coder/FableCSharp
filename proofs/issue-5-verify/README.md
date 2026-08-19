# Issue #5 verify — hard-coded grok-goal recover traces

Investigation only. No `src/` or `tests/` edits.

**GitHub:** [SamG-Coder/FableCSharp#5](https://github.com/SamG-Coder/FableCSharp/issues/5)
**Title:** Tests write recover traces to a hard-coded grok-goal temp path
**Issue state on GitHub:** Open (opened 2026-08-18)
**HEAD:** `ee084901e8212814d4ca7df599180117f9be5cec` (`master`)
**Classification vs HEAD: STILL OPEN**

The issue is the test-side I/O, not engine behaviour. `src/` has zero `grok-goal` hits. The named `[Fact]`s still write, and the same pattern is also used in `ScriptRuntimeArchitectureTests` (not listed in the issue body).

---

## Issue claim

`EngineLifecycleTests` writes recover notes and traces to a machine-specific Grok session directory:

```
C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer
```

Cited facts: `Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`,
`CreatePlayers_004AE940_sets_plus9826_via_0099A350`,
`Frontend_00595582_new_game_message_leaves_without_RequestNewGame`,
and older pump tests. Each run `Directory.CreateDirectory` +
`File.WriteAllText` / `Trace.Write`.

Asked outcome: do not keep creating `grok-goal-*` folders from unit tests.
Suggested fix: delete the writes, or gate on `FABLE_TRACE_DIR`, and move
recover notes into `docs/` / status.

---

## Grep vs HEAD (`tests/Fable.Formats.Tests`)

| Pattern | Hits | Files |
|---|---|---|
| `grok-goal` | **178** | `EngineLifecycleTests.cs` (30), `ScriptRuntimeArchitectureTests.cs` (148) |
| `AppData\Local\Temp` | **178** (same lines) | same two files |
| `grok-goal-c0c5431552c1` | **177** | same two files |
| `grok-goal-96ce88caacfb` | **1** | `ScriptRuntimeArchitectureTests.Scratch()` |
| `"recover-` path fragments | **91** | 20 lifecycle + 71 script |
| `File.WriteAllText` in those two files | 20 + 74 | 91 recover dumps + 3 coverage docs in script tests |
| `FABLE_TRACE_DIR` | **0** | — |
| `grok-goal` under `src/` | **0** | — |

No other test file under `tests/Fable.Formats.Tests` contains `grok-goal` or `AppData\Local\Temp`. Other `File.WriteAllText` sites (`ExportDir`, `implementer/frontend`, `docs/runtime`) are repo-relative and out of scope for this issue.

---

## Cited facts: still write

All three methods named in #5 still exist and still dump.

`Frontend_00595582_new_game_message_leaves_without_RequestNewGame`
(`EngineLifecycleTests.cs` ~1223):

```csharp
var dest = Path.Combine(
    @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
    "traces");
Directory.CreateDirectory(dest);
life.Trace.Write(Path.Combine(dest, "frontend-00595582.txt"));
File.WriteAllText(
    Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
        "recover-00595582.txt"),
    """
    00595582: singleton [0x13B8B5C]
    ...
```

`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint` (~1638):

```csharp
File.WriteAllText(
    Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
        "recover-0051FD80.txt"),
    """
    00521AE0 → 00520D00 NewThing loop
    ...
```

`CreatePlayers_004AE940_sets_plus9826_via_0099A350` (~2383):

```csharp
File.WriteAllText(
    Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
        "recover-004AE940.txt"),
    """
    004AE940 (ecx = game+80568 player object):
      call 0099A350
    ...
```

---

## Remaining `File.WriteAllText` recover dumps

### `tests/Fable.Formats.Tests/EngineLifecycleTests.cs` (20)

| Line | File |
|---|---|
| 1251 | `recover-00595582.txt` |
| 1403 | `recover-0042E3EE.txt` |
| 1624 | `recover-00521AE0.txt` |
| 1706 | `recover-0051FD80.txt` |
| 1759 | `recover-00662880.txt` |
| 1776 | `recover-0049F180.txt` |
| 1794 | `recover-first-scene.txt` |
| 1895 | `recover-004B4260.txt` |
| 1946 | `recover-window-input.txt` |
| 2049 | `recover-0042E3EE.txt` (second write of same name) |
| 2127 | `recover-main-dx9.txt` |
| 2203 | `recover-00446A30.txt` |
| 2353 | `recover-00CB5AD0.txt` |
| 2408 | `recover-004AE940.txt` |
| 4712 | `recover-game-pump.txt` |
| 4737 | `recover-init-world-map.txt` |
| 4756 | `recover-main-forward.txt` |
| 4862 | `recover-004164E0.txt` |
| 4902 | `recover-record36.txt` |
| 4924 | `recover-006B42F0.txt` |

Typical block (game-pump cluster ~4705):

```csharp
var dest = Path.Combine(
    @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
    "traces");
Directory.CreateDirectory(dest);
life.Trace.Write(Path.Combine(dest, "winmain-forward.txt"));
life.Trace.Write(Path.Combine(dest, "init-world-map.txt"));
life.Trace.Write(Path.Combine(dest, "game-pump-004189C2.txt"));
File.WriteAllText(
    Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
        "recover-game-pump.txt"),
    """ ... """);
File.WriteAllText(
    Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
        "recover-init-world-map.txt"),
    """ ... """);
File.WriteAllText(
    Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
        "recover-main-forward.txt"),
    """ ... """);
```

Trace-only (no recover file) still creates the grok-goal dir, e.g. load-gtg ~4798:

```csharp
var dest = Path.Combine(
    @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
Directory.CreateDirectory(dest);
life.Trace.Write(Path.Combine(dest, "load-gtg.txt"));
```

### `tests/Fable.Formats.Tests/ScriptRuntimeArchitectureTests.cs` (71 recover writes)

Same `Path.Combine(@"C:\Users\samue\...\grok-goal-c0c5431552c1\implementer", "recover-….txt")` after each command fact. First:

```csharp
File.WriteAllText(
    Path.Combine(@"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer",
        "recover-speak.txt"),
    """
    Speak 00CC25FD / apply 00CC27EA
    ...
```

Recover files: `speak`, `return`, `playsound`, `givehero`, `giveherohealth`,
`giveheroexpression`, `takeobjectfromhero`, `putupyourswords`,
`givegold-sheathe`, `holdinhand`, `modifyhealth`, `setscared`, `setbound`,
`killable`, `setpushable`, `setdamageable`, `setattackable`, `setfree`,
`setappearanceseed`, `setdrunk`, `teleportinfrontof`, `resetpos`,
`sethomepos`, `teleportthing`, `setthingconscious`, `looktocamera`,
`pausething`, `setgravityonthing`, `liftrock`, `fadething`,
`playobjectanim`, `camerapreload`, `follownavroute`, `ailevel`,
`waitforanimationevent`, `release`, `waitforunderradius`, `followers`,
`preloadanim`, `atoskip`, `interactivespeakgroup`, `dataspeak`,
`fightwith`, `slideteleport`, `askquestion`, `usetheme`, `putinherohands`,
`setheroweapon`, `removeheroweapons`, `herohair`, `removeheroclothes`,
`walkto`, `createlight`, `dummyeffect`, `camerashake-removeeffect`,
`createeffect`, `create-0070d580`, `tintscreento`, `setlightscene`,
`camerapath`, `camerarotatething`, `camerafovlookbetweenpos`,
`cameralookbetween`, `drawthing`, `usecamerafovmarkerlist`,
`setdoorchest`, `camerarig`, `playanimation`, `waitforcamera`,
`waitformessagecamera`, `waitflag`.

Trace-only grok-goal dirs (no recover `WriteAllText`) at ~532, ~878
(cache music), ~4674, ~5480 (WalkUpToThing), ~5867.

Dual write at the native-prefix theory (~528): `Scratch()` **and**
`grok-goal-c0c5431552c1`:

```csharp
var dest = Path.Combine(Scratch(), "traces");
Directory.CreateDirectory(dest);
runtime.Trace.Write(Path.Combine(dest, name + ".txt"));
var goalScratch = Path.Combine(
    @"C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer", "traces");
Directory.CreateDirectory(goalScratch);
runtime.Trace.Write(Path.Combine(goalScratch, name + ".txt"));
```

Second session id (also a grok-goal folder):

```csharp
private static string Scratch()
{
    var dir = @"C:\Users\samue\AppData\Local\Temp\grok-goal-96ce88caacfb\implementer";
    Directory.CreateDirectory(dir);
    return dir;
}
```

Used for traces (~528) and `COMMAND_COVERAGE.md` scratch (~7963). Coverage
also writes into repo `docs/runtime/` (versioned; not the #5 path).

---

## Why not FIXED / PARTIAL

- Named facts still dump to `C:\Users\samue\AppData\Local\Temp\grok-goal-c0c5431552c1\implementer`.
- No `FABLE_TRACE_DIR` gate.
- No replacement of those literals with `ExportDir`, `Path.GetTempPath()`, or repo `docs/`.
- `ExportDir` (`tests/Fable.Formats.Tests/ExportDir.cs`) already writes gitignored `export/` for frontend/font dumps; recover traces were not moved there.
- Scope is **larger** than the issue text: 71 extra recover dumps in `ScriptRuntimeArchitectureTests`.

`src/` being clean does not close #5.

---

## Leftover (work remaining)

1. **178** hard-coded `grok-goal-*` path literals in two test files.
2. **91** `File.WriteAllText(..., "recover-….txt")` side effects (embedded recover notes).
3. Matching `Directory.CreateDirectory` + `Trace.Write` into `…\implementer\traces`.
4. `Scratch()` → `grok-goal-96ce88caacfb` (second session id).
5. Recover note content is only in test string literals, not in versioned `docs/` / status (except coverage markdown, which is a separate write).

---

## Proposed next step

Mechanical test-only PR (no `src/` behaviour change):

1. Delete every `File.WriteAllText` recover blob from `EngineLifecycleTests` and `ScriptRuntimeArchitectureTests`. Asserts already lock the behaviour.
2. Delete `Directory.CreateDirectory` + `Trace.Write` to grok-goal paths, including the dual write at the native-prefix theory.
3. Replace `Scratch()` with `Path.GetTempPath()` / `Path.Combine(Path.GetTempPath(), "FableCSharp.Tests")` **or** drop it; never `grok-goal-*`.
4. If dumps are still wanted, gate on `FABLE_TRACE_DIR` (issue option 2) or `ExportDir.PathFor("traces", name)`.
5. Optionally copy recover text that is still evidence into `docs/runtime/` or existing `proofs/*` (issue option 3) in a follow-up; not required to close the environment leak.
6. Close-out grep: `rg grok-goal tests` and `rg AppData\\\\Local\\\\Temp tests` must be empty.

Do not keep creating `grok-goal-*` folders from unit tests.
