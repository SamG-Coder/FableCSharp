# PALSKIN type1 `0x80` / dest on kid 4300 vs no-save Pump

Investigation only. Production `src/` was not edited.

Do **not** invent kid **4300** on Pump.
Do **not** invent `CreateCharacter` **4300** on Pump.
Kid **4300** is a `FirstSceneWorld` fixture, not
`EngineLifecycle.Pump`.

Question: first-seen no-save PALSKIN of kid **4300** — is
type1 bit **`0x80`** set? Dest identity because no
play-anim? Status.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH** / **DIVERGE**.

Authority: listings
`palskin-packer-dest-null-00bd2d90-00bd2d90.md`,
`palskin-bone-pack-00bd2d90-00bd2d90.md`,
`palskin-hierarchy-00aa0090-00aa0090.md`,
`palskin-queue-slots-00bd7838-00bd780d.md`;
siblings `proofs/palskin-child-hero`,
`proofs/palskin-type1-0x80-kid`,
`proofs/hero-palskin-first-submit`,
`proofs/audit-firstsceneworld`,
`proofs/bone-after-leave`,
`proofs/xseq-00AA0090-interp`;
`WorldShading.cs`, `FirstSceneWorld.cs`,
`EngineLifecycle.PumpGameUpdate` /
`SubmitCurrentWorld` / `SpawnHero` (read only);
tests `Kid_c3d_stores_hair_flag1_and_bones`,
`Kid_4300_flag1_hair_drains_0x200_after_sky`,
`WorldPipelineTests` FirstSceneWorld layer bits,
`EngineLifecycleTests` `HeroMeshId==4299` /
`SubmittedPalskinMeshIds` contains 4299.

---

## Verdict

**There is no first-seen no-save PALSKIN of kid 4300.**
Pump after Leave submits Lookout adult Graphic **4299**.
Kid **4300** `MESH_YOUNGHERO_02` is the Oakvale
`FirstSceneWorld` fixture (`WorldGeometry.IsPrimaryStart`
clone), not `PumpGameUpdate`.

Type1 bit **`0x80`** on **4300** stays **UNREAD** as a
submit. Host skip of that bit on the 4300 file **MATCH**es
type0 (body `0x100`, Flag1 hair extra `0x200` after sky).
Do not invent a 4300 DIP on slot 14.

First-seen dest **is** identity because no play-anim.
Packer `00BD2D90` (`[helper+288]==0`) calls hierarchy
`00AA0090` with mixer channel count **0**, then
`00A9E1E0` × IBM (`00BD2F91` `dest = S * C3D`) ≈ **I**.
`FirstSeenPlaysAnim=false`. That dest math is the same
empty-channel pack whether the Graphic is Pump **4299**
or fixture **4300**.

| Claim | Class |
|---|---|
| First-seen no-save PALSKIN is kid **4300** | **DISPROVEN** |
| Pump `CreateCharacter` / `006AC910` is Graphic **4300** | **DISPROVEN** |
| Kid **4300** is `FirstSceneWorld` fixture | **PROVEN** (host). **LEFTOVER** as first Present |
| Type1 `[inst+104]+8==1` on 4300 | **UNREAD** as 1 |
| First-seen 4300 submit on `0x80` | **skip** / **UNREAD** as a 4300 DIP |
| Dest ≈ I because no play-anim | **PROVEN** |

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| First-seen no-save PALSKIN of kid 4300? | **No such submit.** Pump PALSKIN set is Lookout **4299**. | **DISPROVEN** |
| Is type1 bit `0x80` set on that 4300? | Not a Pump layer. On the 4300 *file*, type1 slot 14 is empty unless `[inst+104]+8==1`. That dword is **UNREAD as 1**. Host never emits `0x80` for 4300. | **UNREAD** as 4300 submit. Host skip **MATCH** |
| Dest identity because no play-anim? | **Yes.** `00BD2D90` → `00AA0090` (0 channels) → `00A9E1E0` × IBM ≈ I. `FirstSeenPlaysAnim=false`. Create / `00DBDE40` do not PlayAnimation. | **PROVEN** |
| `CreateCharacter` 4300 on Pump? | **No.** `00489D40` / `006AC910` is `CREATURE_HERO` Graphic **4299** at `GuildArrivalHSP`. | **DISPROVEN** |
| Wire 4300 into `PumpGameUpdate`? | **No.** | **DISPROVEN** |

---

## 1. Two sites — do not mix Pump and fixture

```
Pump (no-save Present)
  0042F2A2 Leave
  0049F180 Init Characters
    00449D90 PLAYER_HERO miss → CREATURE_HERO
    00489D40 CreateCharacter
      006AC910 GuildArrivalHSP            // Graphic 4299
  later PumpGameUpdate
    SubmitCurrentWorld
      SubmittedPalskinMeshIds contains 4299
      HeroMeshId = 4299
      NO 4300

FirstSceneWorld.Build  (tests / leftover soup)
  Region = StartOakValeWest
  WorldGeometry.Build expand=true
    IsPrimaryStart && no existing hero
      CloneAs(NOVStartHSP, CREATURE_HERO_CHILD)
      PlayerMeshId = 4300
  TracePalskin is FATHER on 0x100, not kid type1
```

`FirstSceneWorld` type comment: **not**
`EngineLifecycle.Pump` (no-save Present is
LookoutPoint). Client / `PumpFrontendFrame` /
`SubmitCurrentWorld` have **no callers**. Only
`WorldPipelineTests` / `ScriptRuntimeParityTests`.

`WorldGeometry.IsPrimaryStart` injects
`RegionTravel.KidCreature` only for
`StartOakValeWest` when TNG has no
HERO/TRAINING/CHILD. Pump `PresentWorld` uses
`expandGeometry: false` on LookoutPoint and
already has adult `CREATURE_HERO`. That clone
**does not run** on Pump.

| Site | Graphic | Bones | Layer set (host) | Native family |
|---|---|---|---|---|
| Pump no-save | **4299** `MESH_HERO` | 77 | `0x100` only (`DrawnPasses`) | `0x80` + `0x100` (**DIVERGE** host). **Not 4300** |
| FirstSceneWorld | **4300** `MESH_YOUNGHERO_02` | 76 | `0x100` + Flag1 `0x200`; **no `0x80`** | type0 skip of slot 14 |

`EngineLifecycleTests` locks `HeroMeshId==4299` and
`SubmittedPalskinMeshIds` contains **4299**.
`WorldGeometryTests` locks `PlayerMeshId==4300` on the
Oakvale soup. Those are **different** worlds.

**DISPROVEN:** treat fixture 4300 as Pump PALSKIN.
**DISPROVEN:** `CreateCharacter` 4300 on Pump
(`hero-palskin-first-submit`, `00449D90-player-hero-miss`).
Kid create leftover is `00DBDE40` after a proven
`Q_NewOakValeIntro` activate — not `004189C2` pumps
(`00DBDE40-after-activate`: do not wire into Pump).

---

## 2. Type1 `0x80` on 4300 is UNREAD as a submit

Queue (`00BD780D`, listing `palskin-queue-slots`):

```
00BD77FE  mov eax, [edi+104]
          test eax, eax
          je  skip
00BD780D  mov eax, [eax+8]          ; [inst+104]+8
          sub eax, 0
          je  type0                 ; 00BD789C → slot 8
          dec eax
          jne skip
; type1:
          00BCE740 helper
          00B84720(10)              ; slot 10 → bit 0x100
          00BCE740 helper
          00B84720(14)              ; slot 14 → bit 0x80 after sky
```

Type0 Flag1 extra (`[mat+41]`) → slot **9** / **`0x200`**.
Type1 **never** reads Flag1. `WorldShading`:
`PalskinQueueSlotType1A=10`, `Type1B=14`, type0=8,
Flag1 extra=9.

Drain `00B33010`: `0x100` = slots 8 then 10; sky
`0x2000`; `0x80` = slot 14; `0x200` = slot 9.

Kid **4300** file (`Kid_c3d_*`, dump type 5):

```
#4300 MESH_YOUNGHERO_02  bones=76  prims=4  stride 28 flags 0x14
  face / torso / mouth   Flag1=0
  Young Hero Hair        Flag1=1 MapFlags=1
```

That shape is **type0 + Flag1 hair**, not type1.
If 4300 were type1, Flag1 would **drop** slot 9 and
hair would land on `0x80` instead of `0x200`. First-seen
MATCH cannot be both (`palskin-type1-0x80-kid`).

Live `[inst+104]+8` on a 4300 instance is **UNREAD as
1**. Writer of `[inst+104]` is **UNREAD**. Status.md
still lists PALSKIN type1/Flag1 routing **UNREAD**
(`f4a1efc`) as leftover research. Flag1 extra on the
4300 *file* is now host **MATCH** (`0x200` after sky);
type1 `0x80` on 4300 stays **UNREAD** as a DIP.

Host:

```
DrawnPasses(Palskin, 0) = [0x100]
DrawnPasses(Palskin, 1) = [0x100, 0x200]   // never 0x80
Kid_4300_flag1_hair_drains_0x200_after_sky
  Contains 0x100 and 0x200
  DoesNotContain 0x80
WorldPipelineTests FirstSceneWorld
  Contains 0x100 / 0x200
  DoesNotContain 0x80
InstanceDraw.PalskinPassBit80
  "Not first-seen 4300 ([inst+104]+8 unread as 1)"
```

Registration still **walks** bit `0x80` (index 25).
Empty drain of slot 14 is not a kid DIP. Do not invent
4300 triangles on that bit.

Native Lookout **4299** family **does** fill type1 slots
10+14 (`hero-palskin-first-submit`). That is adult
Pump, **not** this Graphic. Host Pump
`DrawnPasses` still only `0x100` (**DIVERGE** vs 4299,
not a 4300 fact).

---

## 3. Dest identity because no play-anim — PROVEN

Packer `00BD2D90` (listings dest-null + bone pack):

```
00BD2D9C  mov eax, [ebx+288]        ; dest
          test eax, eax
          jne  00BD3054             ; already packed
00BD2DAC  edi = [ebx+228]           ; mesh
00BD2DB2  esi = [edi+152]           ; n bones
          alloc n*64 → [ebx+288]
00BD2E0B  call 00B83750             ; cache (0 first-seen)
          push 1                    ; flag
          push esi                  ; n
          push dest
          push mesh
          push cache
          push t                    ; unused when count 0
          push helper+124           ; source B
this = [[mesh+80]+4]+960            ; mixer
          push helper+116           ; source A
00BD2E35  call 00AA0090
```

`FirstSeenPalskinPackerRebuildsWhenDestNull=true`.
`PalskinPacker=0x00BD2D90`. Dest offset **288**.

Hierarchy `00AA0090` (listing + `xseq-00AA0090-interp`):

```
ebx = [mesh+152]                    ; n
alloc n*48 scratch via mixer+20
call 00A9F2F0                       ; header lerp
channel count = ([list+16]-[list+12])*0x66666667 sar 3
test eax, eax
jbe  00AA097D                       ; FIRST-SEEN: 0 channels
; else 20-byte walk, 00A52650, 00A4C1F0  — UNREAD mixer
tail always 00A9E1E0                ; hierarchy × IBM
```

`WorldShading.BoneHierarchyBuild=0x00AA0090`.
First-seen mixer ctor is empty (`00AA0F60` at
`MBANK_ALLMESHES+960`). Channel count **0** skips
time lerp. `00A9E1E0` walks 48-byte locals + 60-byte
parent, writes 64-byte worlds. SSE path
(`[0x13D2880]=1`) then `00BD2F91` `dest = S * C3D`.
Product ≈ **I**.

Kid **4300** file palettes (`Kid_c3d_*`):

- bone 0 `Scene Root` parent −1 identity
- `Bip01` parent 2, last row (0,0,0,1)
- `FirstSeenPalettes` all `IsNearIdentity`
- `TrianglesForPose()` at t=0 equals `Triangles`
- `SkinPosition` with 255/0/0/0 on palette 0 is raw pos

`WorldShading.FirstSeenPlaysAnim=false`.
`RegionTravel.FirstSeenPlayAnimationAppliesPose=false`.
`FirstSeenAppearancePlaysDefault=false` (`005B37F7`
only clothing GUI / `PC_UI_FRAME`, not create
`006AC910`, not `00DBDE40`).

So dest is identity **because** first-seen does not
play a clip (0 mixer channels), not because 4300 is
Pump. Play-anim product is **not** first-seen dest.
Do not CPU re-skin submit (`MeshBatches` uses
`MeshFile.Triangles` already `00A9E1E0` × IBM).
GPU `c38` (`00BCFB00`) is later; do not file.

Mixer eval inside `00AA0090` (frac, 20-byte channels,
`00A4C1F0`) stays **UNREAD**. First-seen skip of that
walk is **MATCH**.

---

## 4. Host vs native

| Host | Native first-seen | Class |
|---|---|---|
| `PumpGameUpdate` → `SubmitCurrentWorld` palskin **4299** | Lookout adult `00BD71B0` | **MATCH** Graphic. **DISPROVEN** as 4300 |
| Extra `Meshes.Get(HeroMeshId)` | create does not DIP | **LEFTOVER** (dead; 4299 already in instances) |
| `FirstSceneWorld.PlayerMeshId=4300` | Oakvale intro leftover | **PROVEN** fixture. **DISPROVEN** as Pump |
| `DrawnPasses` no `0x80` on 4300 | type0 skip of slot 14 | **MATCH** skip. Type dword **UNREAD** as 1 |
| `DrawnPasses` only `0x100` on Pump 4299 | native also slot 14 / `0x80` | **DIVERGE** vs 4299. Not a 4300 layer |
| File tris as dest | `00BD2D90` empty-channel pack ≈ I | **MATCH** first-seen dest |
| `FirstSeenPlaysAnim=false` | no PlayAnimation on create / `00DBDE40` | **MATCH** |
| `00AA0090` addr lock, no mixer object | 0-channel tail + **UNREAD** lerp | **MATCH** first-seen. Mixer **UNREAD** |

---

## Do not

- Invent kid **4300** on Pump / `SubmitCurrentWorld` /
  `SubmittedPalskinMeshIds`.
- Invent `00489D40` / `006AC910` Graphic **4300**
  (`CREATURE_HERO_CHILD`). Create is **4299**.
- Wire `00DBDE40` / `Q_NewOakValeIntro` into
  `PumpGameUpdate`.
- Call `FirstSceneWorld.Build` the no-save Present.
- Invent a 4300 DIP on type1 `0x80` (hair, mouth,
  second body, clothing 4126, `Duration=1`).
- Treat Flag1 `0x200` as type1 `0x80`.
- Fold type1 into `DrawnPasses` without
  `[inst+104]+8==1`.
- CPU-skin dest as the first-seen draw. Identity is
  the empty-channel pack, not a repose.
- Pair this Graphic with Lookout adult
  (`hero-palskin-first-submit`).

Next leftover on **4300** is still the **writer** of
`[inst+104]+8` (type1 `0x80` **UNREAD** as a submit)
and Oakvale spawn, not a Pump CreateCharacter.
On Pump the leftover is 4299 per-prim + `c38`, not
this id.
