# First C3D bone / skeleton after Leave Frontend

Investigation only. No production `src` edits.

Do **not** start at Oakvale / `MESH_YOUNGHERO_02` **4300** /
`hero_young_set.bncfg` / `CS_WAKING_UP_LOOP` / `3420` /
`00DBDE40`. That path is later `Q_NewOakValeIntro`, not
Leave / Init World / first no-save 3D Present.

Do **not** treat `.bncfg` XYZ scales, mixer channels, or
`00A8E770` group-register lists as this skeleton.
Siblings: `proofs/bone-config-first` (loose `data\Bones`),
`proofs/anim-blend-first` (empty mixer),
`proofs/palskin-open` (type-5 *payload*),
`proofs/hero-appearance-first` (Graphic 4299),
`proofs/c3d-first-submit` (static `0x20`, 0 bones),
`proofs/xseq-first` (type-6, no first clip),
`proofs/morph-first` (`CSkeletalMorphDef` names).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: listings `00A89450` / `00A894ED` / `00A89519` /
`00A4BD70` / `00A9E1E0` / `00AA0090` / `00BD2D90` /
`00BD2F91` / `00BCFB00` / `00A8E770`;
`docs/runtime/FORWARD_TREE.md` §§4, 7, 14–15;
`docs/PARITY.md` §7 (kid layout; **LEFTOVER** as first
no-save id);
`docs/status/investigations/E-player-palskin.md`,
`2026-08-18-palskin.md`, `D-c3d-transforms.md` §4.6,
`2026-08-18-first-scene-things.md`;
`MeshFile.ReadBones` / `WorldShading.FirstSeenPalettes`;
`MeshFormatTests.Kid_c3d_stores_hair_flag1_and_bones`
(4300 format, **not** first after Leave);
`EngineLifecycleTests.Install_banks_and_startup_videos_exist`
(`HeroMeshId=4299`, `BoneCount>0`).

---

## Verdict

**First live skeleton after Leave is Lookout adult Graphic
4299 `MESH_HERO`: 77 C3D bones, bind-pose dest.** Not
frontend. Not Init Game. Not kid 4300. Not a `.bncfg`.

| Layer | First after Leave | Class |
|---|---|---|
| Frontend 2D / Leave Present | no C3D bones | **DISPROVEN** |
| `CSkeletalMorphDef` / `CTCSkeletalMorph` *name* | Init Thing Components `004EE23F` | **PROVEN** names. **DISPROVEN** as C3D |
| Mixer `00AA0F60` at `bank+960` | Init Mesh Bank `00A27030` | **PROVEN** empty object. **DISPROVEN** as hierarchy |
| `MBANK_ALLMESHES` directory | `0049E620` | **PROVEN** open. **DISPROVEN** as bone parse |
| Loose `.bncfg` enum | `006C37D0` tail of `004A6E30` | **PROVEN** I/O (`bone-config-first`). **DISPROVEN** as C3D skeleton |
| Static Lookout C3D (`0x20`) | after `006C2170` | **PROVEN** first *static* submit. **DISPROVEN** as skeleton (`BoneCount=0`) |
| Type-5 bone *blocks* | first `00A243B0` miss on **4299** | **PROVEN** id / file. Caller **PARTIAL** |
| Hierarchy *evaluate* | `00AA0090` tail `00A9E1E0` from packer `00BD2E35` | **PROVEN** first dest. Channels **0** |
| First-Present PALSKIN set | **`[4299]` only** | **PROVEN** |
| Lookout AICreature bodies | constructed `0051FD80`; not submitted | **PROVEN** exist. Native first draw **UNREAD** |
| Kid 4300 / father / 4126 | Oakvale / clothing leftover | **DISPROVEN** as this site |

**Answer:** first *name* is `CSkeletalMorph*` intern. First
*object that will hold dest* is the empty mixer. First
*skeleton data* is the 77-bone C3D on **4299**. First
*use* is bind-identity `00A9E1E0` × IBM.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  0042DF9E 2D UI type 0x22          // no C3D, no bones
  Init Engine 0042E204
    00B3B6D0 3, "SHADERS_PALSKIN"   // tokens, not a skeleton
0042F2A2 Leave frontend
  009BE420 + 009BEEB0 Present       // black
0042F491 Init Game → 004184BD
  Init Thing Components 004EE23F    // CSkeletalMorph names
  Init Definition Manager 00416005(1)
    game.bin Graphic 4299 lives here; not parsed
  Init World 004A6E30
    Init Mesh Bank 0049E620
      00A27030 MBANK_ALLMESHES
        00AA0F60 mixer bank+960     // empty; world+68
        009D56C0 directory          // ParsedCount=0
    tail 006C37D0 .bncfg preload    // scales, not C3D
  00416953 FinalAlbion.wld
004189C2 first pumps                // no region; no bones
later 00501450 LookoutPoint
  006C2170 Loading objects
    Graphic apply 0077BA40          // static 0-bone props
  GuildArrivalHSP
    006AC910 CREATURE_HERO
      Graphic 4299 MESH_HERO        // FIRST skeleton Graphic
then first 00A243B0(id=4299) miss
  00A26D40 type 5
    00A89450 / 00A894ED             // FIRST 60/48/64 blocks
first PALSKIN dest pack 00BD2D90
  00BD2E35 00AA0090 (channels=0)
    00A9E1E0 hierarchy bind locals  // FIRST evaluate
    00BD2F91 dest = S * IBM ≈ I
  00BCFB00 upload group → c38
```

`4300` / `Scene Root` kid tests / `3420` / father 20/4 are
**not** on this list. **PROVEN**.

---

## 1. Frontend / Leave — no skeleton

| Claim | Class | Evidence |
|---|---|---|
| Frontend Present is 2D (`0042DF9E` / type `0x22`) | **PROVEN** | FORWARD_TREE §4; `camera-after-leave` |
| `MBANK_ALLMESHES` / `00A894ED` during `0042EC7C` | **DISPROVEN** | first `0049E620` is Init World after Leave |
| Press Start `009AD410` opens 4299 / 4300 | **DISPROVEN** | UI Type=10 (`0041D21B`) |
| Leave teardown walks bones | **DISPROVEN** | `0042F2A2` fade / clear+Present |
| `SHADERS_PALSKIN` at Init Engine is a skeleton | **DISPROVEN** | name + tokens only (`palskin-open`) |
| Host `PumpFrontendFrame` calls `Meshes.Get` | **PROVEN** absence | `EngineLifecycle` frontend body |

**Answer:** no bone blocks, mixer, or `.bncfg` until after
Leave.

---

## 2. What “skeleton” is (and is not)

Four different “bones” sit near this path. Only one is the
C3D hierarchy.

| Thing | VA / file | Bones? |
|---|---|---|
| `CSkeletalMorphDef` / `CTCSkeletalMorph` | `004F40D1` / `004D481C` | **No.** Name intern. Kid `4300` / `_DEAD_CREATURE` apply later |
| `C3DSkeletalMorph` RTTI | `rtti.txt` | **PROVEN** name. First-seen **UNREAD**. Not on Leave listing |
| Mixer `00AA0F60` | `bank+960` | **No.** Scratch + 0 channels (`anim-blend-first`) |
| `.bncfg` 20-byte XYZ | `0088B0C0` | Scale table. Not parent/IBM (`bone-config-first`) |
| Type-5 C3D blocks | `00A894ED` | **Yes.** Names + 60 + 48 + 64 |
| Group list `00A8E770` +23/+24 | prim subset | Register *indices* into dest, not mesh bone ids |
| Vertex UBYTE4 | PALSKIN decl | Influences. **Not** mixer weights |
| Cloth byte after boneNameSize | `00A89450` | Skipped. **DISPROVEN** as first skeleton |
| 48-byte root at mesh+176 | `00A89564` `push 48` | Serialized. Draw multiply **UNREAD** (`D` §4.6) |

Serialize `00A894ED` (**PROVEN** sizes):

```
[esi+152]  bone count
  shl 1                → u16[count] name offsets   (vtbl+16)
[esi+168]/[+172]       → framed LZO names          (00996610)
[esi+156]  * 60        → 60-byte info              (00996610)
[esi+160]  n*48        → 48-byte local TRS         (lea +eax*2; shl 4)
[esi+224]  n*64        → 64-byte IBM               (shl eax,6)
lea [esi+176] push 48  → root matrix
```

Getter `00A4BD70` (**PROVEN** 5 insns):
`return [this+156] + i*60`. Parent is dword at
**info+4** (`00A9E1E0` `[ebx+eax+4]`; `jge` walk vs
parent `< 0` leaf). First 12 bytes of the 60-byte
record are id / parent / flags; remaining 48
**UNREAD**.

`MeshFile` constants match: `BoneInfoBytes=60`,
`BoneLocalBytes=48`, `BoneMatrixBytes=64`.

---

## 3. First names / empty holders (still no C3D)

After Leave, before Lookout:

| Order | Site | What | Class |
|---|---|---|---|
| 1 | `004EE23F` → `004F40D1` / `004D481C` | `CSkeletalMorph*` intern | **PROVEN** first *name* |
| 2 | `00416005(1)` | game.bin defs; Graphic **4299** on `CREATURE_HERO` | **PROVEN** id in compiled def. **DISPROVEN** as parse |
| 3 | `00A272F6` → `00AA0F60` | mixer, zero channels | **PROVEN** first dest *holder* |
| 4 | `009D56C0` | MESH directory; type 5 indexed | **PROVEN**. `ParsedCount=0` |
| 5 | `006C37D0` | `data\Bones\*.bncfg` | **PROVEN** sibling. Not this |

Adult Lookout hero does **not** use `CSkeletalMorphDef`
(`E` §5). That def is kid **4300**. **DISPROVEN** as
first apply.

---

## 4. First C3D skeleton — 4299 `MESH_HERO`

Live `graphics.big` id **4299**, type 5, `anim=1`:

| Field | Value | Class |
|---|---|---|
| Name | `MESH_HERO` | **PROVEN** file / `GameBinFormatTests` |
| Bones | **77** | **PROVEN** file (`palskin-open`, first-scene dump) |
| Chain | `Scene Root` → `Movement_dummy` → `Sub_movement_dummy` → `Bip01` … | **PROVEN** live dump (`E` §6) |
| Weapon slots | `WEAPON_FOCUS_02` **73**, `_01` **74**, `WEAPON_SCABBARD_01` **75**, `_02` **76** | **PROVEN** names. Idle (`weapon-anim-first`) |
| Skin verts / faces | 3378 / 2117 | **PROVEN** |
| Prims | 19 | **PROVEN** |
| Helpers / dummies | 12 / 7; `MeshFile.Parse` **drops** | **PROVEN** sizes. Sockets **UNREAD** |
| Prim0 group | 9 bones (`torso_back`) → `c38–c64` | **PROVEN** file. Adult decl **PARTIAL** vs tests |
| First dest | hierarchy × IBM ≈ identity | **PROVEN** (`FirstSeenPlaysAnim=false`) |

Kid **4300** is **76** bones, 4 prims, stride 28 / flags
`0x14`. `MeshFormatTests` locks *that* file’s
`Scene Root` parent −1 and `Bip01` parent 2. It is
**DISPROVEN** as first after Leave. `PARITY.md` §7
quotes 4300 as “first-seen” layout — **LEFTOVER**
*id* vs no-save Lookout; the 60/48/64 layout is
**PROVEN** for both files.

Static Lookout Graphics (`0077BA40`, layer `0x20`) parse
first as *C3D* but have **0** bones (e.g. 4126 folded
hat trap). **DISPROVEN** as skeleton.

Lookout AICreature (`CREATURE_BEGGAR_01`, villager,
trader, …) are constructed and *would* have type-5
skeletons. Host `ResolveSubmit` skips null TNG XYZ.
`SubmittedPalskinMeshIds == [4299]`. Native first
`00A243B0` on those ids is **UNREAD**. Do not treat
them as first Present skeleton.

`005B37F7` DEFAULT / `0070D580` are **not** on
`006AC910`. **PROVEN** skip.

---

## 5. First hierarchy evaluate

Packer `00BD2D90` (`[this+288]==0` rebuilds;
`FirstSeenPalskinPackerRebuildsWhenDestNull=true`):

```
00BD2E21  helper
00BD2E28  ecx = [that+4]
00BD2E2B  ecx = [ecx+960]      // mixer
          mesh, dest, n=[mesh+152], 1
00BD2E35  call 00AA0090
```

`00AA0090` (**PROVEN**): `ebx = [mesh+152]`; channel
count 0 → `jbe 00AA097D`; tail **always** `00A9E1E0`.

`00A9E1E0` (**PROVEN** parent walk): local 48-byte
quat+T+S; parent from 60-byte `+4`; writes 64-byte
worlds. Then `00BD2F91` `dest = S * C3D` IBM
(SSE first-seen: `[0x13D2880]=1` via `00A5B850`).
Product ≈ **I**. `00BCFB00` copies 12 dwords /
influence from `dest[group[i]*64]` to `c38`.

Group byte is `a0` register offset (`i*3`), **not**
a mesh bone index. `FirstSeenPalskinCpuPaletteIsMeshBone`
is the dest *row*, after remap.

**Answer:** first *evaluate* is empty-channel
`00AA0090` + `00A9E1E0` on 4299 bind locals. Not
Leave. Not a clip.

---

## 6. C# vs native

| Site | Native | Host | Class |
|---|---|---|---|
| Frontend bones | none | no `Meshes.Get` | **MATCH** |
| Mixer | `00AA0F60` | no field | **LEFTOVER** |
| `.bncfg` preload | `006C37D0` | unused parser | **LEFTOVER** (`bone-config-first`) |
| First parse | `00A26D40` type 5 on 4299 | `SubmitCurrentWorld` → `Meshes.Get(4299)` | **MATCH** id / timing |
| `ReadBones` | `00A894ED` 60/48/64 | same | **EQUIVALENT** format |
| 48-byte root +176 | serialized | skipped | **MATCH** skip. Apply **UNREAD** |
| Helpers / dummies | on file | dropped | **LEFTOVER** sockets |
| `FirstSeenPalettes` | `00A9E1E0` × IBM | parent walk × `bone.Matrix` | **EQUIVALENT** when 0 channels |
| `PaletteForPose(time)` | `00A52650` leftover | discards `time` | **DIVERGE** later. Unused first Present |
| `SubmittedHeroPalskin` | `00BD71B0` family | `BoneCount>0` | **DISPROVEN** as draw (membership only) |
| `FirstSeenPalskinStrideBytes=28` | adult prim0 **36** | kid constant | **LEFTOVER** name |
| `FirstSceneWorld` 4300 | not this tree | Oakvale helper | **LEFTOVER** |
| Adult 4299 `Scene Root` / `Bip01` | live dump | no test fact | **PROVEN** file. **UNTESTED** in `MeshFormatTests` |

---

## Classifications (short)

1. **Frontend / Leave skeleton — DISPROVEN.**
2. **First *name* after Leave — `CSkeletalMorph*` at `004EE23F`. PROVEN.** Not a C3D.
3. **First dest *object* — empty mixer `00AA0F60` at Init Mesh Bank. PROVEN.**
4. **First C3D skeleton — 4299 `MESH_HERO`, 77 bones, after `006AC910`. PROVEN.** Static props are 0-bone. Kid 4300 / father **DISPROVEN**.
5. **First evaluate — `00A9E1E0` bind locals, dest ≈ I. PROVEN.** Channels 0. `.bncfg` / group lists / UBYTE4 are other layers.
6. **Native first `00A243B0` caller / AICreature type-5 parse order — PARTIAL / UNREAD.** First-Present set is **`[4299]`**.
7. **C# — EQUIVALENT parse + bind palettes. LEFTOVER mixer / bncfg / kid stride name.**

## Do not

- Open or skin 4299 / 4300 on frontend frames.
- Treat `Kid_c3d_*` / `PARITY.md` 76-bone kid as the first
  New Game skeleton.
- Treat `006C37D0` `.bncfg` or `00AA0F60` mixer as the
  C3D hierarchy.
- Treat `00A8E770` group bytes as mesh bone ids.
- Submit Lookout AICreatures or Graphic **4126** as the
  first skeleton.
- Invent `PlayAnimation` / DEFAULT / cloth / `C3DSkeletalMorph`
  on Leave or first Lookout Present.
