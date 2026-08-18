# Level-loader job: `006C27A0` / copy / pop (after Leave)

Investigation only. Production `src/` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Question: what do `006C27A0` (BuildLoadJob), `006C2D40` (copy maps),
`006B9E00` (copy tree), and `006C2BA0` (pop) do? When do they run
after Leave frontend? How does host `LoadTiming.cs` sit next to that?

Sources: `tools/Fable.ExeIndex/out/01-sections/text-map/listing-006c0000.txt`,
`listing-00680000.txt`, `listing-00500000.txt`, `e8.tsv`;
`docs/runtime/FORWARD_TREE.md` §9; `docs/PARITY.md`;
`src/Fable.Game/LoadTiming.cs`; `EngineLifecycle.cs`
(`BuildLoadJobFn`…`LevelLoaderPopFn`, `RequestLoadRegion`,
`PumpLevelLoader`, `ApplyLoadJob`, `SubmitCurrentWorld`);
`EngineLifecycleTests.Apply_006C2170_is_topology_then_objects_then_004FCBB0`.

---

## Verdict

**`006C27A0` is not Leave, not WLD parse, not first `004189C2` pumps,
and not `LoadTiming`.**

After Leave (`0042F2A2`) the no-save spine is Init Game →
`00416953` FinalAlbion.wld → empty `006C20A0` (no job) → dummy
WorldFrame pumps (still no job). First `006C27A0` is inside
`00500540`, itself called from `00501450` (E8 caller of
`00501450` still **UNREAD**). Build copies a stride-28 map
vector (`006C2D40` → job+16) and a tree (`006B9E00` → job+0/+4),
then writes `job+28 = index`. Enqueue is `006C2120` onto
`[WorldMap+188]+20`. Sync apply is `006C2710` → `006C2170`
then **`006C2BA0` pop** under the string
`"Level loader update end"`.

Host `LoadTiming` (`EngineLifecycle.Timing` +
`LastLoadTiming`) clocks **parse / TNG apply / submit soup**.
It does not implement the native job object. Native open is
names + directories + headers; `LastLoadTiming` is the later
draw-side rebuild.

---

## When after Leave (no-save New Game)

```
0042F2A2  Leave frontend                 // RequestNewGame
0042F491  Init Game 00418DCA / 004184BD
  00416953  Loading world FinalAlbion.wld
    00507C30  WLD parse
    006C20A0  empty skip                 PROVEN
              00507C30 does not E8 006C27A0
    vtbl+208  00B23DC0 → 00B428E0        STB miss
004189C2  first pumps
  004A5E10  inc WorldFrame
  no 00501450 / 00500540 / 006C27A0      PROVEN
later 00501450                           E8 caller UNREAD
  004FEEC0(current,0)  +156=0
  count=(+48−+44)/88                     0x2E8BA2E9 = /88
  count>1: for i=1..count-1
    00500540(i, 0, 0)                    sync
      [map+44]+i*88 ; +36 may be null
      +36 null → 006BB2F0([world+28],0) then jmp 00500887
                 still reaches 006C27A0  PROVEN
    0048D400 / 005198B0
  RegionGraph.txt  0x124467C
  00500540(saved, 0, 1)                  async / no pump
```

Host pairing `EnqueueAfterDummy` onto the **second** `Pump` is
**DISPROVEN** (`PARITY.md`; tests keep `00501450` as an
explicit `LoadFromFirstRealRegion` after dummy pumps).

Continue-save `00449E60` → `00487C20` →
`00500540(index, 0, 1)` is a different caller. Not no-save.

---

## `006C27A0` BuildLoadJob — **PROVEN**

`listing-006c0000.txt` `006C27A0`…`006C27C4` `ret 12`.
`ecx` = 32-byte job (alloc `00BFEA1A(32)` at `00500CE0`).

```
006C27A0  arg0 = src map vector
          arg1 = src tree
          arg2 = region index
  lea ecx, [job+16]
  call 006C2D40          // copy maps
  push tree ; ecx = job
  call 006B9E00          // copy tree
  mov [job+28], index
  ret 12
```

Only four `E8` sites (`e8.tsv`):

| Site | Parent | Notes |
|---|---|---|
| `00500D7A` | `00500540` | main +36 path |
| `005010AE` | `00500540` | other branch |
| `00501319` | `00500540` | other branch |
| `00501C0E` | `00501450` tail | then `"CWorldMap::UpdateNavMaps - SetAsLoading"` → `006C2120` |

WLD parse does **not** call it. **PROVEN.**

### Job (32 bytes)

Filled at `00500CE0` then `006C27A0` / `006C20B0`:

| Off | Field |
|---|---|
| +0 / +4 | tree header + count (`006B9E00`) |
| +12 | loader* (`006C20B0` at enqueue) |
| +16 / +20 / +24 | map `std::vector` begin/end/cap |
| +28 | native region index |

### Map record (stride 28)

`0x92492493` signed `/28` at `006C2D40` and every
`006C2170` pass. `00500B17` zeros 28 bytes then writes
`[rec-28]`. `006C2170` reads:

| Off | Apply |
|---|---|
| +0 | map id (`ebx`) |
| +4 / +8 | topology ptr → `"Loading topology"` `004FF080` / `00638310` / `004FF440` |
| +12 | nav → `00500230` / `0050AF10` (first-seen `00500540(1,0,0)` zeros this → skip) |
| +20 / +24 | objects ptr → `"Loading objects"` `00522720` / `00521AE0` |

---

## `006C2D40` copy maps — **PROVEN**

`006C2D40`…`006C2E82` `ret 4`. `ecx` = dest vector
(`job+16`). Arg0 = source.

- Same-pointer early out.
- `count = (src.end − src.begin) / 28`.
- Dest capacity too small → `00BFEA0E(count*28)`,
  `006C2AA0` copy, `005196A0` free old, write begin/end/cap.
- Else overwrite via `006C2B10` / `00518B80` / `006C2AA0`.

This is the ContainsMap list for that region (Lookout:
`LookoutPoint`, `BowerstoneBridge`, `GuildExterior`).
SeesMap / BWD neighbours are **not** extra jobs.

---

## `006B9E00` copy tree — **PROVEN**

`listing-00680000.txt` `006B9E00`…`006B9EAF` `ret 4`.
`ecx` = dest tree (the job). Arg0 = source.

- `dest == src` → no-op.
- If dest has nodes (`[+4] != 0`): `004ABA70` then
  reset sentinels, count=0.
- Src empty → dest empty sentinels.
- Else `00513300(src.root)` clone; walk `+8` / `+12`
  to refresh min/max; copy count.

`006C27A0` is one caller. Same helper is also used by
unrelated trees (`006B9445`, `0070686B`, `00A2C698`, …).
Do not treat every `006B9E00` as a load job.

`006BB2F0` (`mov [ecx+24], 0` / `ret 4`) is **not**
the tree copy. `005009BE` calls it on `[world+28]`
when record+36 is null, then still builds the job.

---

## `006C2BA0` pop — **PROVEN**

`006C2BA0`…`006C2BF2` `ret 8`. Doubly-linked node unlink
+ refcount dtor + `00BFEA14` free. Not the apply body.

`006C2710` after `"Level loader update end"`:

```
006C2710  [loader+20] sentinel; empty → ret
          "Level loader update"
          node+8 job → 006C2170(loader)
          "Level loader update end"
          006C2BA0(list, first)           // pop
```

`006C20A0`: nonempty iff `[loader+20].next != sentinel`.
`006C2120`: `006C20B0` binds `job+12 = loader`, alloc 16-byte
node, insert before sentinel.
`006C27D0` / loader `vtbl+4` is a thunk to `006C2710`.
Ctor `006C26B0` vtbl `0x125D9B0`; WorldMap+188 from
`004AF160`.

Second `006C2BA0` site: `006C2CCF` (front-pop helper).

---

## Apply after pop-prep (`006C2170`) — already proven

Order (`PARITY.md` / `Apply_006C2170_…`):

1. topology (`004FF080` / `00638310` / `004FF440`)
2. objects (`00522720` / `00521AE0` `.tng`)
3. nav if `rec+12` (`00500230` / `0050AF10`)
4. `0051E2F0`
5. `"Post Load Initialise"` `004FD020`
6. `"Activate Topology"` `004FCBB0` if `rec+4`, then `004FCFE0`
7. if `job+28 > 0`: map `vtbl+88` `005064C0` then `004FC8A0`

Host `004FCBB0`-before-objects is **DISPROVEN**.

`00500540` after apply: `004AFC00([0x13B89FC], record+24)`,
dtor `0050F980` stride 28.

---

## `LoadTiming.cs` vs the native job

`LoadTiming` is a host stopwatch list (`Measure` / `Add` /
`Format`). Comment on the type: native open is names +
directories + headers; draw later reads handles.

| Clock | When | Native pair |
|---|---|---|
| `Timing` `"frontend NG"` | `RequestNewGame` / Leave | **before** any job |
| `Timing` `"WLD"` / `"TNG global"` / `"region graph"` | `LoadWorldMap` | `00507C30`; **no** `006C27A0` |
| `Timing` `"mesh bank"` / `"textures.big"` / `"STB/LEV open"` | bank / static-map open | directory + header |
| `Timing` `"region TNG"` | `ApplyLoadJob` | **`006C2170`**, after `006C27A0` |
| `LastLoadTiming` (`PresentWorld`, `TerrainCells`, `LandDraws`, `C3D`, `BuildMeshes`, `Sky`, `Textures`) | `SubmitCurrentWorld` from `PumpGameUpdate` once `HeroSpawned` | **after** apply + `004FC8A0`; not a job |

Equating `LastLoadTiming` with `006C27A0` is **DISPROVEN**.
The New Game hitch in `docs/status/investigations/F-load-performance.md`
is first `SubmitCurrentWorld` tessellate, not job build.

Host `RequestLoadRegion` only **Notes** the four VAs; the
queue is an `int` index list, not a 32-byte job + stride-28
vector + tree. Layout fidelity is **PARTIAL**.

---

## Recovered order (host notes vs native)

```
Leave 0042F2A2
Init Game / 00416953 / 00507C30
006C20A0 empty                         // no BuildLoadJob
dummy 004189C2
00501450                               // host: LoadFromFirstRealRegion
  00500540(i,0,0)
    006C27A0
      006C2D40  job+16  stride 28
      006B9E00  job tree
      job+28 = i
    006C2120  [WorldMap+188]+20
    sync: while 006C20A0
      006C2710
        006C2170                       // Timing "region TNG"
        006C2BA0                       // pop
004AFC00 / 004FC8A0
PumpGameUpdate
  SubmitCurrentWorld                   // LastLoadTiming
```

---

## Open / leftover

| Item | Class |
|---|---|
| `00501450` E8 caller (not second `004189C2`) | **UNREAD** |
| `00513300` node clone fields | **UNREAD** |
| What the job tree *contains* (vs map vector) | **PARTIAL** |
| `00500D8F` `cmp [esp+176]` vs third-arg sync (call sites push `(index, 0, flag)`) | **PARTIAL** |
| Host job as `List<int>` vs 32-byte record | **PARTIAL** |
| `006C27A0` during Leave / WLD / first WorldFrame | **DISPROVEN** |
| `LoadTiming` == BuildLoadJob | **DISPROVEN** |
| `006BB2F0` == copy tree | **DISPROVEN** |
