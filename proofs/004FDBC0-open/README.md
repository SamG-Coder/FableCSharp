# `004FDBC0` opens `LookoutPoint.tng` during `00507C30`

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale`. No-save
New Game is Leave `0042F2A2` → `FinalAlbion.wld` → Loading world
`004A1840` → `00507C30`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**DIVERGE**.

Question: `004FDBC0` opens `LookoutPoint.tng` during `00507C30`.
Exact args, allocator, does it construct Things or only
parse/store? Relation to later `0051FD80` construct of the same
file vs BowerstoneBridge first construct.

Authority: dump `004FDBC0` / `00507C30`;
`proofs/tng-first-after-leave`.
Siblings: `wld-00507C30-switch`, `tng-after-leave`, `tng-spawn`,
`tng-first-def`, `first-region-after-leave`.

Sources: `listing-004c0000.txt` (`004FDBC0` / `004FBF60` /
`004FAFF0`), `listing-00500000.txt` (`00507C30` / `00509859` /
`005223F0` / `00521AE0` / `0051FD80`), `listing-00480000.txt`
(`0049E220` / `004A1840`), `listing-00980000.txt` (`0099AD80` /
`0099A6A0`), `listing-00a00000.txt` (`00A39D80`),
`listing-00bc0000.txt` (`00BFEA1A` / `00BFE9BC`),
`EngineLifecycle.LoadWorldMap` / `LoadGlobalThingsFile` /
`LoadRegionMapThings` / `LoadSingleThing`,
`EngineLifecycle.TngExtVa` `0x012442C4`.

---

## Verdict

| Claim | Class |
|---|---|
| `004FDBC0` is thiscall `ecx=CWorldMap`, **no** stack args | **PROVEN** |
| Caller is `00507C30` at `00509946` after `"Load global things"` | **PROVEN** |
| First opened `.tng` is **`LookoutPoint.tng`** (native map index 1) | **PROVEN** |
| Stream object is `00BFEA1A` then `0099AD80` (first-seen `+168==0`) | **PROVEN** |
| That open **constructs** CThings (`0051FD80` / `00A371C0`) | **DISPROVEN** host; **UNREAD** live (`005223F0` `[manager+128]==1`) |
| Host stores parsed `ThingInstance`s in `GlobalThings` | **PROVEN** parse/store, no insert |
| Later first `0051FD80` is **`BowerstoneBridge.tng`** `TRACK_NODE_BASIC` | **PROVEN** |
| Lookout is **re-opened** as ContainsMap[1] for that construct walk | **PROVEN** |

**Open ≠ construct.** First file I/O is Lookout during WLD load.
First CThing is Bridge after `00501450`.

---

## Exact args

### `00507C30` Load `.wld` file  (`listing-00500000`)

```
00507C30  sub esp, 0x1BC
          ebx = ecx                    // CWorldMap
…
00509A14  add esp, 0x1BC
00509A1A  ret 8
```

Thiscall + two stack dwords. From `004A1840` → world `vtbl+8`
`0049E220`:

```
004A1A84  push -1
004A1A86  lea eax, [ebp-116]          // CFile already 0099A6A0'd
004A1A89  push eax
004A1A8A  mov ecx, esi                // CWorld
004A1A8C  call [edx+8]                // 0049E220
```

`0049E220` (`ret 8`):

```
0049E22C  mov esi, [esp+16]           // arg0 = file
0049E23A  mov ecx, [edi+20]           // CWorldMap
0049E23E  mov eax, [esp+20]           // arg1 = -1
0049E242  push eax
0049E243  push esi
0049E244  call [edx+12]               // 00507C30
```

| Slot | Value |
|---|---|
| `ecx` | `CWorldMap` (`[world+20]`) |
| `[esp+4]` | WLD `CFile*` (`FinalAlbion.wld`) |
| `[esp+8]` | **`-1`** |
| ret | `ret 8` |

### `004FDBC0` Loading global things  (`listing-004c0000`)

```
00509857  push -1
00509859  push "Load global things"
…
0050987B  mov al, [0x13B8609]
00509880  test al, al
00509882  je 00509946                 // first-seen 0
…
00509946  mov ecx, ebx                // still CWorldMap
00509948  call 004FDBC0
0050994D  push -1
0050994F  push "Load global things end"
```

```
004FDBC0  sub esp, 8
004FDBC3  push ebx
004FDBC4  push esi
004FDBC5  mov esi, ecx                // CWorldMap
004FDBC7  mov eax, [esi+32]           // map-table begin
004FDBCA  mov ecx, [esi+36]           // end
          count = (end-begin)/72      // 0x38E38E39
004FDBDE  mov ebx, 1                  // skip dummy slot 0
004FDBF2  mov edi, 0x48               // stride 72
004FDCA8  ret                         // no stack args
```

| Slot | Value |
|---|---|
| `ecx` | `CWorldMap` (same `ebx` as `00507C30`) |
| stack | none |
| ret | `ret` |
| BSS | `[0x13B8609]==0` else `004FE2A0` `.gtg` **not** this path |

Per iteration, if `[slot+36] && [slot+40]` (`EndMap` filled +
`LoadedOnPlayerProximity`):

```
004FDC71  push ebx                    // native map index
004FDC72  mov ecx, esi
004FDC74  call 004FBF60               // ret 4
```

First hit: `ebx=1`, slot at begin+72 = WLD `NewMap 1`
`LevelScriptName "LookoutPoint"`. **PROVEN.**

### `004FBF60` one map file  (`listing-004c0000`)

| Slot | Value |
|---|---|
| `ecx` | `CWorldMap` |
| `[esp+4]` | map index (`ebx` from `004FDBC0`) |
| ret | `ret 4` |

```
004FBF6B  mov eax, [edi+32]
004FBF72  lea edx, [esi+esi*8]        // index * 9
004FBF76  lea ecx, [eax+edx*8+24]     // slot+24 script
004FBF7A  call 0099E480               // CString view
004FBF81  lea ecx, [esp+20]
004FBF85  call 004FAFF0               // ecx=dest, edx=script
```

`004FAFF0` (`ret`): thiscall dest string, `edx` = map script.
Pushes `0x12442C4` (host `TngExtVa`) then `00997620` /
`00997780` / `0099B720`. That is **`".tng"`**. First name is
therefore **`LookoutPoint.tng`**.

Then `005223F0`:

```
004FC004  push esi                    // map index
004FC009  sub esp, 8                  // shared_ptr {stream, ctrl}
004FC019  mov ecx, [edi+8]
004FC01C  mov edx, [ecx]
004FC01E  call [edx+12]               // thing-manager getter
004FC021  mov ecx, eax
004FC023  call 005223F0               // ret 12
```

---

## Allocator

CRT thunks (`listing-00bc0000`):

| VA | IAT | Role |
|---|---|---|
| `00BFEA1A` | `[0x1440150]` | `malloc` |
| `00BFE9BC` | `[0x1440164]` | `free` |
| `00BFEA0E` | `[0x1440158]` | `operator new[]` |
| `00BFEA14` | `[0x1440154]` | `operator delete[]` |

### On the first Lookout open (`004FBF60`)

`CWorldMap` ctor `005066E0` writes `[map+168]=bl` with `ebx=0`.
`00507C30` does not rewrite `+168`. First-seen therefore takes
the **disk `CFile`** arm, not WAD `00A39D80`.

```
004FBF93  mov al, [edi+168]
004FBFAB  je 004FBFD4                 // taken first-seen
004FBFD4  push 28
004FBFD6  call 00BFEA1A               // malloc(28)
004FBFE2  push 2
004FBFE4  push 1
004FBFE6  lea edx, [esp+28]           // path LookoutPoint.tng
004FBFEB  mov ecx, eax
004FBFED  call 0099AD80               // vtbl 0x122D06C, ret 12
```

`0099AD80` → `0099A6A0` → `call [0x143FE2C]` (`CreateFileW`).
Args to `0099AD80`: path, `1`, `2` (read).

Untaken arm (`[map+168]!=0`): `00BFEA1A(16)` + `00A39D80`
(`vtbl 0x129CF8C`, extra `[map+160]`, `ret 16`).

Then the stream is boxed:

```
0050F587  push 12
0050F589  call 00BFEA1A               // malloc(12) refcount
0050F59D  mov [eax+4], 0x50BE40
0050F5A4  mov [eax+8], ecx            // stream
```

Release is `dec [ctrl]`; at 0, vtbl dtor then `00BFE9BC`.

`00507C30` prologue also `00BFEA1A(36)` → `006C26B0` when
`[map+188]==0` (first fiber). That is **not** the TNG stream.

`005223F0` if `[manager+128]==1` additionally `00BFEA0E(20)`
(scratch list) and `00BFEA14` on the way out. **UNREAD** as a
taken New Game branch.

Thing **construct** allocator is later `00A371C0` Allocate
Class inside `0051FD80`. `004FDBC0` does not call it.

---

## Construct vs parse/store

```
005223F0  mov esi, ecx                // thing manager
005223F7  mov eax, [esi+128]
005223FF  cmp eax, 1
00522407  jne 00522502                // drop stream, ret 12
          …
0052249F  call 00521AE0               // "Thing Manager: Load From File"
005224AB  call 0051E2F0
```

| Layer | What `004FDBC0` does |
|---|---|
| Path | **always** `004FAFF0` `.tng` |
| Open | **always** `0099AD80` / `00A39D80` |
| Token walk `00521AE0` / `00520D00` `NewThing` | **only if** `[manager+128]==1` |
| CThing `0051FD80` / `00A371C0` / `004CA010` | same gate |
| Else | open + drop shared_ptr; no `NewThing` |

Host `LoadGlobalThingsFile`:

- walks `World.Maps` with `LoadedOnPlayerProximity` (no dummy 0)
  **MATCH** 151 files
- `LevelLibrary.TryLoadThings` loose then WAD **MATCH** TLC
  (no loose `LookoutPoint.tng`)
- concatenates into `GlobalThings` section `GLOBAL`
- **does not** `LoadSingleThing` / `InsertThing` / `RegionThings`

That is **parse + store**, not construct. **DIVERGE** vs a taken
`+128==1` branch; **MATCH** vs the skip plus first-seen
`00416392==0` (no countable Things yet). Live `[manager+128]`
on this first `005223F0` stays **UNREAD**.

Do not call `00521AE0` “not the global apply” as a call-graph
claim (`0052249F` exists). It is **UNREAD** as a New Game taken
branch.

`.gtg` `004FE2A0` exists on disk; BSS 0 skips it. `.gtng` miss
`0050959F` **PROVEN**.

---

## Same file later vs BowerstoneBridge first construct

Two different “first”s. Do not collapse them.

```
00416953  Loading world
  00507C30
    NewMap 1  LookoutPoint            // Maps[0], native index 1
    NewRegion 1  ContainsMap
      BowerstoneBridge, LookoutPoint, GuildExterior
    00509859  Load global things
      004FDBC0
        LookoutPoint.tng              ← FIRST OPEN
          (0051FD80 gated UNREAD)
        … 150 more proximity maps …
    00509982  Load region graph
  Set Static Map                      // after the open
0049F180  Init Characters             // no TNG
004189C2  dummy pumps  index 0        // 0 things
later 00501450 → 00500540(1,0,0) Lookout region
  006C2170  Loading objects
    BowerstoneBridge.tng              ← FIRST 0051FD80
      TRACK_NODE_BASIC  GuardTrack
    LookoutPoint.tng  288             ← reopen, ContainsMap[1]
      MARKER_BASIC  M_Maze            ← first Lookout 0051FD80
    GuildExterior.tng  88
    HOLY_SITE_PLAYER_START GuildArrivalHSP → 006AC910 later
```

| Sense | File | Def | When |
|---|---|---|---|
| First `.tng` **open** | `LookoutPoint.tng` | (tokens iff `+128==1`) | inside `00507C30` / `004FDBC0` |
| First **CThing** `0051FD80` | `BowerstoneBridge.tng` | `TRACK_NODE_BASIC` `GuardTrack` | `006C2170` pass 2, ContainsMap[0] |
| First Lookout **CThing** | `LookoutPoint.tng` again | `MARKER_BASIC` `M_Maze` | ContainsMap[1] after 88 Bridge things |

ContainsMap order is WLD bytes, **not** `NewMap` index order.
FORWARD_TREE “LookoutPoint first” is the **region name**.

If the unread global `+128==1` branch were live, the first
`0051FD80` would be Lookout `M_Maze` **during** `00416953`,
before Bridge. Host does not take that path. First-seen count 0
is **PARTIAL** evidence native did not either.

---

## Host vs native

| Site | Host | Native |
|---|---|---|
| `00507C30` args | `WorldFile.Load(FinalAlbion.wld)` | `ecx=map, file, -1` |
| `004FDBC0` args | `LoadGlobalThingsFile` on `World.Maps` | `ecx=map`, no stack |
| First open | `TryLoadThings("LookoutPoint")` | `004FBF60(1)` → `004FAFF0` `.tng` |
| Stream alloc | WAD `Read` / loose `File` | `00BFEA1A(28)` + `0099AD80` |
| Global construct | none | `005223F0` gated **UNREAD** |
| First `0051FD80` | Bridge `TRACK_NODE_BASIC` | same if global gate off |

Host WAD open vs native `CreateFileW` on a resolved path is
**PARTIAL** (same bytes, different FS). TLC has no loose
`LookoutPoint.tng`.

---

## Not these

| Candidate | Why not |
|---|---|
| `0051FD80` as the opener | construct; file open is `004FAFF0` / `0099AD80` |
| `BowerstoneBridge.tng` as first **open** | first **construct**; second-or-later open |
| `004FDBC0` as first CThing | no `00A371C0` unless unread gate |
| `StartOakVale*.tng` / `00DBDE40` | NewRegion 4; not this tree |
| `FinalAlbion.gtng` | miss `0050959F` |
| `FinalAlbion.gtg` | `[0x13B8609]=0` |
| Set Static Map / `FinalAlbion.stb` | after `004FDBC0`; not TNG |

---

## UNREAD / PARTIAL

- Live `[thing_manager+128]` on the first `005223F0`.
- Whether `0099AD80` `CreateFileW` hits a search path that is
  byte-identical to host WAD `Find("LookoutPoint.tng")`.
- `00501450` `E8` caller (already UNREAD in FORWARD_TREE).

---

## Do not

- Treat first **open** (`LookoutPoint.tng` in `004FDBC0`) as
  first **CThing**.
- Call `0051FD80` the opener, or `004FDBC0` the constructor.
- Skip `BowerstoneBridge` on the later construct walk because
  Lookout opened first.
- Bind Oakvale / `00DBDE40` / kid `CREATURE_HERO_CHILD`.
- Collapse `LoadedOnPlayerProximity` 151-file walk with
  ContainsMap 3-file apply.
