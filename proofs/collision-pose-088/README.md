# `CTCPhysicsStandard` pose `0x88` — no solver on first Present

Investigation only. No production `src/` edits.

Do **not** invent Unity-style physics (rigidbodies, capsules,
sweeps, generic gravity, a world tick that rejects XYZ).
Native first-seen of this object is **alloc + pose persist +
pose copy**. Dump is the source of truth.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Question: first Present — collision solver, or pose copy
only? Kid or adult?

Authority: listings `004D297B` / `004EE790`
(`listing-004c0000.txt`), `00723FD0` / `00724010` /
`00724290` / `00724F50` (`listing-00700000.txt`),
`006B0730` / `006B0780` / `006B08F0`
(`listing-00680000.txt`); vtbl `01265F84`
(`assembly/exe/00-index/vtbl.tsv`);
`proofs/collision-first-seen`, `proofs/leftover-14-native-key`,
`proofs/0055CB10-locomotion` §5, `proofs/audit-worldgeometry`,
`proofs/first-region-after-leave`, `proofs/c3d-first-submit`,
`proofs/creature-move-first`, `proofs/004F3338-hero-centre`;
`src/Fable.Game/WorldGeometry.cs` (`ObjectTransform` /
`IsPrimaryStart`), `FirstSceneWorld.cs`,
`EngineLifecycle.PresentWorld` / `SpawnHero` /
`CopyPhysicsAxes`.

Do not re-prove Leave `0042F2A2` / `FinalAlbion.wld`, type 4
LMB (`leftover-14-native-key`), or `006A80A0` bit `0x64`
(`collision-first-seen`).

---

## Verdict

**First Present does not run a collision solver.**
`CTCPhysicsStandard` is a **136-byte (`0x88`) pose
component**. First-seen use is **pose copy** (TNG
`Position*` + `RHSetForward/Up` → instance 3×4). Kid
mesh is **not** that Present.

| Question | Answer | Class |
|---|---|---|
| Frontend first Present (`0042DF9E`) runs a solver? | **No.** 2D UI. No Things. No `0x88`. Leftover **#14** is dest/hit for LMB, not physics. | **DISPROVEN** |
| First *world* Present after Leave runs a solver? | **No.** Open is handles + `ObjectTransform`. Draw AABB is frustum. No contact. | **DISPROVEN** as solver |
| Pose copy only? | **Yes, first-seen.** Factory `00BFEA1A(0x88)` → ctor `00723FD0`. Persist `006B08F0` XYZ + `00724290` RH. Copy `006B0780` (`+12`→`+24`) / `00724010` (`+80`→`+104`). Host `ObjectTransform` is that 3×4. | **PROVEN** pose; solver **UNREAD**; invent **DISPROVEN** |
| Kid or adult on no-save first Present? | **Adult** `CREATURE_HERO` mesh **4299** at `GuildArrivalHSP`. | **PROVEN** |
| `CREATURE_HERO_CHILD` / `FirstSceneWorld` kid inject? | Leftover Oakvale (`00DBDE40` / `StartOakValeWest`). **Not** first Present. | **LEFTOVER**; **DISPROVEN** as first Present |

`WorldGeometry.PlayerHeight` is mesh AABB Z × `0.01`. That
is **not** a capsule. **DISPROVEN** as a collider.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  0042DF9E  2D Present              // leftover-14 dest; no Things, no 0x88
0042F2A2 Leave
  FinalAlbion.wld
0042F491 Init Game
  004EE23F Init Thing Components
    004EE790  CTCPhysicsStandard    // name table, not a world
              004D2EF0 / 0x4D297B
              00BFEA1A(0x88) → 00723FD0
    004EE80C  CTCPhysicsControlled  00BFEA1A(0xAC) → 00724F50
                                    // 00724F58 calls 00723FD0
004189C2 first pumps
  dummy WorldMap; no 00501450
later
  00501450 LookoutPoint
    0051FD80 Things                 // Graphic: persist Standard pose
    006AC910 CREATURE_HERO @ GuildArrivalHSP
      004C9D60("CTCPhysicsControlled")   // placement, not a step
    Present: 009AD410 handles + instance 3×4 from that pose
```

`00DBDE40` / `CREATURE_HERO_CHILD` / `FirstSceneWorld`
Oakvale soup are **not** on this list. **PROVEN.**

---

## 1. Alloc `0x88` is the pose object, not a world

### 1a. Type row then factory (**PROVEN** listing)

`listing-004c0000.txt`:

```
004EE790  push "CTCPhysicsStandard"
          …
004EE7AD  push 0x4D297B
          call 004D2EF0                 ; CTC row, not 009B0AC0

004D297B  push esi
004D297C  push 0x88
          mov  esi, ecx
          call 00BFEA1A
          test eax, eax
          je   004D2997
          push esi
          mov  ecx, eax
          call 00723FD0
          pop  esi
          ret
```

Sibling sizes: `CTCPhysicsControlled` `push 0xAC` →
`00724F50`; `CTCPhysicsNavigator` `push 0xB0` →
`007266C0`. Controlled ctor **calls** Standard:

```
00724F50  push arg
          call 00723FD0
          and  [esi+160], 0xFC
          xor  [esi+164] / [esi+168]
          mov  [esi], 0x126616C
          ret 4
```

| Claim | Class |
|---|---|
| Size **136** / `0x88` | **PROVEN** |
| Ctor `00723FD0` | **PROVEN** |
| This row is Add Def Class / a collision world | **DISPROVEN** |

### 1b. Ctor zeros pose slots (**PROVEN**)

```
00723FD0  arg0
          call 006B0730                 ; base: vtbl 0125D2C4; +12..+48=0; +52=-1.0
          xor  [esi+64]
          xor  [esi+128]
          mov  [esi+132], 0x04
          mov  [esi+133], 0x04
          mov  [esi], 0x1265F84
          ret 4
```

`006B0730` writes XYZ at **`+12/+16/+20`**. No radius,
AABB, or contact list in this body. **PROVEN** absence
on the ctor. Treating the type **name** as a solver is
**DISPROVEN**.

vtbl `01265F84` first slots:

| Slot | VA | First-seen role |
| ---: | --- | --- |
| 0 | `00724DE0` | dtor path |
| 1 | `00724290` | persist RH axes (after `006B08F0` XYZ) |
| 4 | `00724010` | copy `+80`→`+104` (24 bytes) |

---

## 2. Persist and copy — pose, not a hit

### 2a. TNG XYZ (`006B08F0`) then RH (`00724290`)

`00724290` (vtbl+1) first `E8`s `006B08F0`, then six
`00410620` keys:

```
006B08F0  "PositionX" / "PositionY" / "PositionZ"
          write esi+12 / +16 / +20     (when edi+24 is not 1 or 3)

00724290  "RHSetForwardX/Y/Z"          → this+80 / +84 / +88
          "RHSetUpX/Y/Z"               → this+92 / +96 / +100
```

Host `ThingFile` + `WorldGeometry.ObjectTransform` /
`CopyPhysicsAxes` consume those same keys. **PROVEN**
pairing.

### 2b. Copy sites (**PROVEN** listing; **DISPROVEN** as solver)

```
006B0780  dword[3]  esi+12 → esi+24     ; Position cache
00724010  dword[6]  esi+80 → esi+104    ; RH Forward+Up cache
          xor esi+76 / +72 / +68
```

`e8.tsv` of `00724010`: `00724F83` (Controlled
`00724F80`) and `007268D3` (Navigator). **Not**
`0042DF9E`. **Not** first Present.

`00724F80`:

```
00724F80  call 00724010
          xor  [esi+144] / [esi+140] / [esi+136]
          or   [esi+60], 1
          and  [esi+160], 0xFE
          ret
```

That is still pose-flag copy. Do not name it a tick.

### 2c. Host 3×4 is the same copy

`WorldGeometry.ObjectTransform`:

```
position = (PositionX, PositionY, PositionZ)
forward  = CTCPhysicsStandard.RHSetForward   (fallback +Y)
up       = CTCPhysicsStandard.RHSetUp        (fallback +Z)
right    = forward × up
scale    = 0.01 * ObjectScale
```

`PresentWorld` (`expandGeometry: false`) stores that
matrix on `WorldMeshInstance`. `SubmitCurrentWorld`
reuses it for C3D and for the spawned Hero. **No**
reject against barrels / landscape / other Things.

Landscape `00BDC2D0` four-plane AABB is **draw**
(`audit-worldgeometry`). **DISPROVEN** as physics.

---

## 3. First Present does not enter this object

### 3a. Frontend `0042DF9E` (**DISPROVEN** solver)

`leftover-14-native-key`: native first Present is
retail `0042DF9E`. User post is **LMB** type 4 / 6 →
`0xE5` / `0x126` / 15. Dest AABB is leftover **#14**.
That Present is **2D UI**.

| `0042DF9E` | Physics? | Class |
|---|---|---|
| Things / `CTCPhysicsStandard` | none | **DISPROVEN** |
| `E8` `00723FD0` / `00724010` / `00724290` / `006B08F0` | none in `listing-00400000.txt` | **DISPROVEN** |
| Landscape DIP | empty patch list (`audit-worldgeometry`) | **DISPROVEN** as a hit |
| `0055CB10` | action apply, not XYZ (`0055CB10-locomotion`) | **DISPROVEN** as a solver |

Host frontend `Pump` does not `SubmitCurrentWorld`.
`SubmitCurrentWorld` waits `HeroSpawned`.

### 3b. First world Present after Leave — pose copy

`c3d-first-submit` / `PresentWorld`:

```
00501450 Lookout
  009AD410 handles
  instance 3×4 from ObjectTransform(Standard pose)
  006AC910 hero 4299 PALSKIN — CopyPhysicsAxes from HSP
```

No recovered contact manifold, sweep, or XYZ reject on
that submit. Locomotion is **absent**
(`creature-move-first`). Without a step there is nothing
for a solver to bounce.

`006A80A0` bit `0x64` is a **collect filter** after each
`00500540`, not first Present and not a hit
(`collision-first-seen`).

---

## 4. Kid / adult

| Stage | Hero | Class |
|---|---|---|
| Frontend Present | none | **PROVEN** absence |
| No-save first world Present | `CREATURE_HERO` mesh **4299** at `GuildArrivalHSP` | **PROVEN** |
| `006AC910` named add | `"CTCPhysicsControlled"` (Standard ctor underneath) | **PROVEN** name |
| `CREATURE_HERO_CHILD` on Lookout TNG | **false** | **PROVEN** |
| `00DBDE40` kid + `WatchBarrels` | leftover Oakvale | **LEFTOVER** |
| `WorldGeometry.IsPrimaryStart` injects `KidCreature` | only `StartOakValeWest` when no Adult/Tween/Kid already in the list | **LEFTOVER**; **DISPROVEN** as first Present |
| `FirstSceneWorld.Build` | `Region = StartOakValeWest`, `CAM_OVIF_SHOT2`, kid at `NOVStartHSP` / runtime teleport | **LEFTOVER** soup; **zero** production callers |

No-save `PresentWorld` primary is Lookout. Spawned Hero
is already `AdultCreature`, so `Build` takes the
`existingHero` arm and **does not** clone kid.

`PlayerHeight` on that arm is kid **or** adult mesh
AABB Z × `MeshToWorld`. Draw extent. **DISPROVEN** as
collision height.

---

## 5. Host already models vs leftover

| Host | Native | Class |
|---|---|---|
| Note `004EE790` / factory `0x88` | CTC row + `004D297B` | **MATCH** type |
| `ThingFile` `Position*` | `006B08F0` | **PROVEN** |
| `ObjectTransform` RH basis | `00724290` `+80/+92` | **PROVEN** pose / **DISPROVEN** as hit |
| `CopyPhysicsAxes` HSP → Hero | `006AC910` edx pose | **PARTIAL** (placement) |
| `PresentWorld` `expand=false` | open handles + 3×4 | **PROVEN** as open |
| `FirstSceneWorld` kid / Oakvale house | leftover intro | **LEFTOVER** |
| `IsPrimaryStart` → `CREATURE_HERO_CHILD` | first Present is adult 4299 | **DIVERGE** vs Leave |
| `PlayerHeight` AABB | no recovered collider | **DISPROVEN** as solver |
| `TickMove` lerp | `004C72B0` stub | **LEFTOVER** |

Do not fill the unread solver gap with Unity physics so
leftover #14 dest, WASD, or childhood barrels “have
somewhere to go.”

---

## Classifications (short)

1. **`CTCPhysicsStandard` size `0x88` / ctor `00723FD0`
   — PROVEN.** Pose component. Name-as-solver
   **DISPROVEN.**
2. **Persist + copy — PROVEN.** `006B08F0` XYZ,
   `00724290` RH, `00724010` / `006B0780` dword copies.
   Host `ObjectTransform` **MATCH**es that pose.
3. **First Present solver — DISPROVEN.** Frontend
   `0042DF9E` is 2D (leftover **#14** dest, not
   physics). First world Present is pose copy only.
   Solver body **UNREAD**; invent **DISPROVEN.**
4. **Kid on first Present — DISPROVEN.** Adult
   `CREATURE_HERO` **4299**. `FirstSceneWorld` kid
   inject is leftover Oakvale.

Do not start collision at Lookout rocks, LMB dest, or
`CREATURE_HERO_CHILD`.
