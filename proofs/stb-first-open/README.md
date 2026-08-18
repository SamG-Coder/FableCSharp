# When native first opens STB (after Leave)

Investigation only. Production `src/` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **DIVERGE**.

Question: when does native first open `FinalAlbion_RT.stb` /
`StbArchive`? Path from Leave. Compare `00B42750` OpenStaticMaps
and `00B428E0` SetStaticMapFileForUse. Frontend must not open STB.

---

## Verdict

**First-seen no-save New Game never opens `FinalAlbion_RT.stb`.**

The first STB *attempt* is after Leave, during Init Game / Load
world. It asks for `Data\Levels\FinalAlbion.stb`. TLC does not
ship that file. `[+52].vtbl+12` misses. `[+424]` stays 0. No
name table, no Lookout attach.

`FinalAlbion_RT.stb` is selected only when `[0x13B8616]!=0`
(`build_retail_static_maps` on the command line). That path is
**not** first-seen.

Frontend never reaches `00B428E0` / `00B42750`. Host frontend
must not call `StbArchive.Open` / `LevelLibrary`.

---

## Path from Leave (no-save New Game)

```
0042F2A2  Leave frontend                         PROVEN
  00404490 / 004131A0
  +90576 = FinalAlbion.wld                       PROVEN (0042F44D)
  0042EBB6  009BE420 + 009BEEB0  teardown Present
  [0x13B8616]==0  skip 009A78D0 / 009A8840       PROVEN
0042F491  Init Game
  00418DCA size 0x161E8 → vtbl+4 004184BD
    Init World 004A6E30  (cameras, not STB)
    004188E9  [game].vtbl+32 00416953            PROVEN
      "Loading world"  (no-save +90588 empty)
      004A1840(world, FinalAlbion.wld)
        Load Quests / Startup WAD / 00507C30 WLD
        006C20A0 empty; Generate Offline skip
        004A18FC  0049DDD0(world, dest, wld)     PROVEN
          [0x13B8616]==0 → 0x1238BAC ".stb"
          prefix 0x122F3B4 "Data\Levels\"
          dest = Data\Levels\FinalAlbion.stb
        [0x1375446]==0 skips 2nd 0049DDD0        first-seen
        004A1B7D  "Set Static Map for Engine"
        004A1BD3  [[world+8]+40]+44  vtbl+208    PROVEN
          00B23DC0  mov ecx,[0x1436E8C]; jmp 00B428E0
            00B428E0  SetStaticMapFileForUse
              00B40000  Close  ([+424]==0 → return)
              00BDA070(1)  pool
              0099B7D0  copy name → this+48
              00B42750(1)  OpenStaticMaps
                [+52].vtbl+12(98, +48)  MISS     PROVEN
                00B3E820 then test bl → 00B428CA
                does not write +424
                does not 00B420F0 / 00B41E50
                does not 00B42530
              00B41FA0  LoadWaterData
      0049F180  Init Characters  (after Set Static Map)
004189C2  first game pump  dummy region 0
  OpenStaticMapsMode still 0                     PROVEN
later 00501450 Lookout / 006C2170 TNG
  004FC8A0 MiniMap only  NOT 00B428E0            PROVEN
```

`00B428E0` has no `E8` and no stored dword. Only the display
thunk `00B23DC0` (`012A0F3C+208`). First-seen site is
`004A1BD3`. Host open after `004AFC00` is **DISPROVEN**.

---

## `00B428E0` vs `00B42750`

Same object: map manager `[0x1436E8C]`, ctor `00B423F0` at
Init Engine `00B268C4` (size `0x1C0`, vtbl `012A1F50`).
Ctor zeros `+424`, constructs CString `+48` and bank `+52`
(`009D5F80` / vtbl `012A1EE4` / `009D5230` = `ret 4`).
**Construct is not a file open.**

| | `00B428E0` SetStaticMapFileForUse | `00B42750` OpenStaticMaps |
|---|---|---|
| Role | Driver. One incoming path. | Bank open + attach. |
| Arg | Path CString (from `0049DDD0`) | Mode: 1 use / 2 neighbour list |
| First work | `00B40000` close if `[+424]!=0` | `cmp [+424], mode; je ret` |
| Then | `00BDA070(1)`; copy path to `+48` | `00B40070` occupancy (not a file) |
| File open | **No.** | Mode 1: `[+52].vtbl+12(98, +48)` |
| Hit | n/a | `00B3E820`, sea `00B6D4D0`, `009CCDC0` `__STATIC_MAP_COMMON_HEADER__` → `00B420F0` / `00B41E50`; `[+424]=mode` |
| Miss | still calls `00B42750` | `test bl; je 00B428CA` — **no** `+424`, **no** `00B420F0`, **no** `00B42530` |
| After | `[+432]=3`; `00B41FA0` | Mode 2 only: `00B42530(2)` per `[+32..+36)` |
| First-seen | Yes, from Leave/`00416953` | Yes, as child with mode 1 |

Mode 2 does **not** re-open the STB file. It walks the already
named slot list. First-seen never takes mode 2.

`00B42530` is the STB-**miss fallback** (and the mode-2 opener).
First-seen file miss returns **before** that loop.

---

## Which file: `.stb` vs `_RT.stb`

`0049DDD0` (`listing-00480000.txt`):

```
mov bl, [0x13B8616]
test bl, bl
jnz  → 0x1238BC8 "_RT.stb"     // UNREAD first-seen
jz   → 0x1238BAC ".stb"        // first-seen
0041A410 prefix 0x122F3B4 "Data\Levels\"
```

Only write of `[0x13B8616]` is `00413B4E` inside `004138D0`,
after a hit on `"build_retail_static_maps"` (`0x122E604` @
`00413AFB`). Same fn also parses `"staticmap"` and
`"enable_map_optimizations"`. First-seen New Game never sets
the byte (`RetailStbFlagFirstSeen = 0`). Same byte also skips
frontend bank swaps (`0042EE3D` / `0042F2DB`).

TLC on disk (`GameInstall.RuntimeStbPath`):

| Path | Exists first-seen |
|---|---|
| `Data\Levels\FinalAlbion.stb` | **No** |
| `Data\Levels\FinalAlbion_RT.stb` | **Yes** (~598 MB, BBBB, 424 entries) |

So first-seen native never even *names* the file that exists.

Later Lookout `00B23DC0` / `00B428E0` that would hit `_RT.stb`
without the cmdline flag: **UNREAD**. `004FC8A0` is **DISPROVEN**
as that site.

---

## Frontend must not open STB

| Claim | Class |
|---|---|
| Frontend `0042DF9E` / `PumpFrontendFrame` never calls `00B23DC0` / `00B428E0` / `00B42750` | **PROVEN** |
| Init Engine constructs `[0x1436E8C]` with empty `+48` / `+424=0` | **PROVEN** |
| Frontend still walks landscape `vtbl+16` via `0042E0BB` → `00B27D90` | **PROVEN** (empty list; no DIP) |
| That walk is a file open | **DISPROVEN** |
| `InitFrontendUi` / sprites / fonts / AVI open STB | **DISPROVEN** |
| Host `Bootstrap` / `PumpFrontendFrame` / `RequestNewGame` call `EnsureLevels` | **PROVEN** absence |
| `StbArchive.Open` belongs on a frontend frame | **DISPROVEN** |

Leave only records `FinalAlbion.wld`. STB is a Load-world child.

---

## Host vs native

| Host | Native first-seen | Class |
|---|---|---|
| `SetStaticMapFileForUse` names `FinalAlbion.stb`, notes miss, mode stays 0 | same | **PROVEN** |
| `OpenStaticMapsForCurrentRegion` unused on Pump / EnterGame | n/a | host helper only |
| `LevelLibrary` ctor always `StbArchive.Open(FinalAlbion_RT.stb)` | first-seen does **not** open that file | **DIVERGE** |
| First `EnsureLevels` is `LoadGlobalThingsFile` inside `004A1840`, *before* the miss | native has no STB handle yet | **DIVERGE** (after Leave, not frontend) |
| `PeekMapHeader` / cells after Lookout submit | native patches still closed (`+424=0`) | **DIVERGE** |

`StbArchive.cs` is the host stand-in for `[+52].vtbl+12` +
`009CFBC0` directory (BBBB; last record truncated). It is
**not** a frontend bank and must not run before Leave / Load
world.

---

## Classification table

| Claim | Status |
|---|---|
| First native STB *attempt* is Leave → Init Game → `00416953` → `004A1840` `004A1BD3` → `00B23DC0` → `00B428E0` → `00B42750(1)` | **PROVEN** |
| That attempt opens `FinalAlbion_RT.stb` | **DISPROVEN** (names `FinalAlbion.stb`) |
| That attempt succeeds | **DISPROVEN** (`vtbl+12` miss; `+424` stays 0) |
| `00B428E0` is the file-open | **DISPROVEN** (driver; open is `00B42750` `vtbl+12`) |
| `00B42750` mode 1 is the file-open | **PROVEN** |
| `00B42750` mode 2 re-opens the bank | **DISPROVEN** (slot walk / `00B42530` only) |
| `004FC8A0` / after `004AFC00` opens STB | **DISPROVEN** |
| `_RT.stb` suffix needs `[0x13B8616]` (`build_retail_static_maps`) | **PROVEN** |
| First-seen sets that flag | **DISPROVEN** |
| Map-manager ctor at Init Engine opens STB | **DISPROVEN** |
| Frontend opens STB | **DISPROVEN** |
| Host `LevelLibrary` ctor on frontend | must not; native does not |
| Later native hit of `FinalAlbion_RT.stb` without the cmdline flag | **UNREAD** |

Tests: `EngineLifecycleTests.LoadWorld_004A1840_set_static_map_is_00B23DC0_then_00B428E0`,
`New_game_is_leave_frontend_then_FinalAlbion_wld`.
Dumps: `setstaticmapfileforuse-00b428e0.md`, `openstaticmaps-00b42750.md`,
`listing-00480000.txt` `0049DDD0` / `004A1BD3`, `listing-00400000.txt`
`004138D0`, `fn-00B423F0.txt`.
`docs/runtime/FORWARD_TREE.md` §§9–10; `docs/PARITY.md` `00B428E0` caller.
