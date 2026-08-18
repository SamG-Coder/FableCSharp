# BoneConfig / `data\Bones` first use after Leave Frontend

Investigation only. No production `src` edits.
Do **not** start at Oakvale / `hero_young_set.bncfg` / `CREATURE_HERO_CHILD` / `00DBDE40`.
That path is later `Q_NewOakValeIntro`, not Leave / Init Game / first no-save Present.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **LEFTOVER**.

Sources: `docs/runtime/FORWARD_TREE.md` §§4–7;
`docs/PARITY.md` (.bncfg / hero age);
`docs/status/investigations/E-player-palskin.md`;
`docs/status/investigations/2026-08-18-first-scene-things.md`;
`src/Fable.Formats/Bones/BoneConfig.cs`;
`GameInstall.BonesDirectory`;
`DataCatalogTests.Bncfg_scales_hero_and_villager_bones`;
ExeIndex listings `004A6E30` / `006C37D0` / `006C3620` / `0088B0C0` / `00786700`;
live TLC `data\Bones` (60 `*.bncfg`).

---

## Timeline (no-save New Game)

```
0042EC7C retail
  frontend 2D only                 // no Bones
0042F2A2 Leave frontend
  009BE420 + 009BEEB0 Present
0042F491 Init Game → 00418DCA → 004184BD
  Init Thing Components 004EE23F
    register CHeroMorphDef / CTCHeroMorph
    register CSkeletalMorphDef 004F40D1
    CTCSkeletalMorph name intern 004D481C
  Init Definition Manager 00416005(1)
    game.bin compiled defs (includes CSkeletalMorphDef / CHeroMorphDef)
  … Init Graphics … Create Players …
  Init World 0041735A → 004A6E30
    … Init Speech Gain Manager 006E3EC0 …
    004A76C5 singleton miss → alloc 16 004AE2A0 → 004A9A80
    004A76F7 → 006C37D0            // FIRST loose .bncfg use
      00999760 FindFirst
      loop 0099AD80 open → 0088B0C0 #Start_Bone_data
      00999850 FindNext
00416953 Load world FinalAlbion.wld
004189C2 pumps / later 00501450 Lookout
  006C2170 Loading objects
  0051FD80 / 006AC910 hero CREATURE_HERO
    CHeroMorphDef present; CSkeletalMorphDef kid-only
  Lookout AICreature (beggar / villager / trader) also constructed
```

`hero_young_set.bncfg` / `CREATURE_HERO_CHILD_02` are
**not** on this list. **PROVEN**.

---

## 1. Frontend / Leave use BoneConfig?

| Claim | Class | Evidence |
|---|---|---|
| Frontend Present is 2D UI | **PROVEN** | FORWARD_TREE §4; camera-after-leave |
| Leave teardown opens `data\Bones` | **DISPROVEN** | `0042F2A2` is fade / `0042EBB6` / clear+Present / `FinalAlbion.wld` record. No `006C37D0` / `0088B0C0` |
| `BoneConfig.Load` during frontend | **DISPROVEN** | Zero production callers (see §4) |
| Attract / `CS_ATTRACT_*` loads bncfg | **DISPROVEN** | No StartCutscene on retail pump |

**Answer:** no. Bones files are unused until Init World after Leave.

---

## 2. What `data\Bones` is

Live TLC `GameInstall.BonesDirectory` = `data\Bones`. **60** `*.bncfg`.
`DataCatalogTests` locks the count and three creature types.

Format (same files `BoneConfig.Parse` and native `0088B0C0` read):

```
Creature_type: CREATURE_HERO;
#Start_group_settings
thigh: "Bip01 L Thigh", "Bip01 R Thigh";
…
#End_group_settings
#Start_Bone_data
Bip01 Head: 0.95, 0.95, 0.95;
…
#End_bone_data
```

`BoneConfig.cs` keeps `Creature_type`, quoted group lists, and
`Name: x, y, z` scales. Lines without `:` (the `#Start_*` / `#End_*`
tags) are skipped.

Native `0088B0C0` walks **only** `#Start_Bone_data` … `#End_bone_data`
into 20-byte records `{ name, x, y, z }` (stride magic `0x66666667`).
Then `00A45150` copies non-identity XYZ. `#Start_group_settings` /
`Creature_type:` have **no** exe ASCII string. Group apply **UNREAD**.

| File | `Creature_type` | Role vs first no-save |
|---|---|---|
| `hero_weak.bncfg` | `CREATURE_HERO` | Adult preset. **Not** first *apply* (see §3) |
| `hero_teen_set.bncfg` | `CREATURE_HERO` | Guild tween + `CHeroMorphDef` Teenager. Later |
| `hero_young_set.bncfg` | `CREATURE_HERO_CHILD_02` | Oakvale leftover |
| `bs_male_weak.bncfg` | `CREATURE_BS_VILLAGER_MALE` | Lookout `WaspHelper` / `FH_Villager` |
| `bs_beggar.bncfg` | `CREATURE_BEGGAR_01` | Lookout `LookoutPointBeggar` |
| `bs_male_bully_set.bncfg` | (villager bully) | Lookout `BeggarBully` |

Also on disk: `hero_fat/strong/tall/berserk`, bandit/guard/hobbe/rival
sets, etc. Full 60-name list is the live directory.

`bncfg` / `Creature_type` / `Bip01` are **not** in `strings.tsv`.
Native finds the files by directory enum (`00999760` / `00999850`),
not by hardcoded filenames. Prefix string at `0x122F4D4` (`0041A290`)
is **UNREAD** (not dumped). Live tree is `data\Bones\*.bncfg`.

---

## 3. First native use after Leave

### 3.1 Type register — Init Thing Components

`004184BD` → `004EE23F` (still Init Game, after Leave):

| Site | What | Class |
|---|---|---|
| `004EE304` `CHeroMorphDef` | intern + def factory | **PROVEN** |
| `004EE294` `CTCHeroMorph` | component factory `004D2EF0` | **PROVEN** |
| `004F40D1` `CSkeletalMorphDef` | intern + `0042DAE0` / `009B0AC0` | **PROVEN** |
| `004D481C` `CTCSkeletalMorph` | name intern (size `0x78` at `00787490`) | **PROVEN** |

No file I/O. **PROVEN** as first *name* use. Not BoneConfig data.

### 3.2 Compiled defs — Init Definition Manager

`00416005(1)` loads `game.bin`. `CSkeletalMorphDef` / `CHeroMorphDef`
instances live there (hero sub-def idx 10535 is `CHeroMorphDef`).
Whether this site *applies* scales: **UNREAD**. It is not the loose
`data\Bones` open.

### 3.3 First loose parse — end of Init World Init

`004A6E30` tail, **after** the last FORWARD_TREE named stage
(`Init Speech Gain Manager` `006E3EC0`):

```
004A76C5  singleton == 0
004A76CD  alloc 16 → 004AE2A0
004A76EC  004A9A80 store
004A76F7  006C37D0(singleton)
```

`006C37D0` (**PROVEN** directory walk):

1. `00999760` FindFirst on the Bones prefix (`0041A290` / `0x122F4D4`).
2. Per file: `0099AD80(2,1)` open, `00A44E60` 16-byte slot,
   `0088B0C0` parse `#Start_Bone_data`.
3. `00999850` FindNext until miss.

This is **first** `BoneConfig`-equivalent I/O after Leave.
Not Oakvale. Not `006AC910`. Not first 3D Present.

Lazy twin `006C3620` (same parse) is only an `E8` from `00786781`
inside `00786700` (CTCSkeletalMorph apply). That is **later** than
the Init World preload if the enum already filled the map.

### 3.4 First *apply* (scale onto a Thing)

| Site | When | Class |
|---|---|---|
| `007868A0` | CTCSkeletalMorph init: `00787060` find `CSkeletalMorphDef`, `00786670('SKEL')`, then `00786700` | **PROVEN** body. **UNREAD** first no-save callee (vtbl `0126B6F4`, no `E8`) |
| `00786870` | dirty flag `+12` → `00786700`; `E8` from `0070E3CD` (anim, type id `0x78`) and clothing UI `005B98E4` | **PROVEN** later. First Lookout frame is bind pose (`FirstSeenPlaysAnim=false`) |
| `00835C80` `004C9D60("CTCSkeletalMorph")` | only `E8` is `0066407F` `_DEAD_CREATURE` | **DISPROVEN** as first after Leave |
| Adult Lookout hero `006AC910` uses `CSkeletalMorphDef` | kid mesh 4300 only | **DISPROVEN** (E §5) |
| Adult Lookout hero has `CHeroMorphDef` | persist Teenager/Strength/… | **PROVEN** type. Apply **UNREAD**. Do not treat as `0088B0C0` |
| Lookout TNG AICreature (`CREATURE_BEGGAR_01`, `CREATURE_BS_VILLAGER_*`) | constructed `0051FD80` after `006C2170` | **PROVEN** exist. Whether each has `CSkeletalMorphDef` **UNREAD**. Matching `.bncfg` files **PROVEN** on disk |

**Answer:** first *file* use is `006C37D0` at the end of `004A6E30`.
First *Thing apply* is unread, but it is **not** the adult Lookout
hero PALSKIN path and **not** Oakvale kid `hero_young_set`.

---

## 4. C# `BoneConfig.cs` after Leave

| Site | What it does | Class |
|---|---|---|
| `BoneConfig.Load` / `Parse` | only `DataCatalogTests` | **PROVEN** absence from `src/Fable.Game` / `Fable.Client` / `Fable.Render` |
| `GameInstall.BonesDirectory` | path helper; never consumed by lifecycle | **LEFTOVER** vs native Init World preload |
| `EngineLifecycle.EnterGame` / `InitWorldCameras` / `SpawnHero` | no bncfg open, no `CSkeletalMorph` | **PROVEN** |
| Host PALSKIN dest | mesh IBM / `FirstSeenPalettes`; no `BoneScale` multiply | **LEFTOVER** vs native 20-byte XYZ |
| `hero_young_set` / 4300 on no-save Lookout | FirstSceneWorld leftover | **DISPROVEN** as this site |

**Answer:** C# never uses `BoneConfig` after Leave. Native preloads
the directory at Init World. Host identity dest is not a bncfg apply.

---

## Classifications (short)

1. **Frontend / Leave Bones I/O — DISPROVEN.**
2. **First loose `.bncfg` after Leave — `004A6E30` tail `006C37D0` → `0088B0C0`. PROVEN.** Directory is live `data\Bones` (60 files). Prefix string **UNREAD**.
3. **First Thing apply — UNREAD pointer.** Adult Lookout `CSkeletalMorphDef` **DISPROVEN**. `CHeroMorphDef` apply **UNREAD**. Oakvale `hero_young_set` **DISPROVEN** as first.
4. **C# `BoneConfig` after Leave — LEFTOVER / unused.** Parser matches the files; nothing in the game pump calls it.
