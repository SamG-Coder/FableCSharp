# First-seen particle systems: no-save Present vs Oakvale intro

Investigation only. No production `src/` edits.

Do **not** invent particle GPU / a `0x20` soup /
`PARTICLE_FRONTEND` / named fire–insect–dust draw.

Do **not** treat `004A67D0` as Oakvale VFX. Native
world ctor is inside `"Init World"` `0041735A`,
**before** `"Load Particles"` `004174F1`. After
Leave this walk is `FinalAlbion.wld`, not
`00DBDE40` / `StartOakVale`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: which `NParticleEngine` systems **run** on
no-save first Present vs Oakvale intro (fire,
insects/beetles, dust)? Host coverage?

Authority: `proofs/particles-game`,
`proofs/004A67D0-after-particles`,
`proofs/00417418-world-plus60`,
`docs/runtime/FORWARD_TREE.md` §§6–7,
`docs/status/investigations/2026-08-18-environment.md`,
`docs/status/investigations/2026-08-18-first-scene-things.md`,
`docs/status/investigations/2026-08-18-scene-layers.md`,
`docs/render/FIRST_SCENE_CONTRACT.md`,
`docs/PARITY.md` missing-mesh gizmo row,
`tools/Fable.ExeIndex/out/startoak-tng.txt`,
`listing-00400000.txt` `004174F1` / `0041888B`,
`listing-006c0000.txt` `006E0880`,
`src/Fable.Game/EngineLifecycle.cs`
(`SkipParticlesVa`, `InitGameStages`, `RetailBanks`),
`Dx9SubmitOwnership.CanRenderParticles`,
`EngineLifecycleTests.Init_World_004A67D0_runs_inside_0041735A_before_00417418`,
`WorldGeometryTests.New_game_oakvale_loads_contains_and_sees_maps`,
`RegionTravel.FirstSeenWatchBarrelsSpawnsBeetle`.

Siblings: `proofs/particles-game`,
`proofs/initgame-after-leave-order`.

---

## Verdict

**First-seen *running* particle-system list is
empty on no-save Present. Oakvale intro authors
47 `PARTICLE_EMITTER_PLACEABLE` Things; their
`BankIndexName` / `CParticleSystem` names are
UNREAD. Fire / insects / beetles / dust as named
`effects.big` systems on either path is
DISPROVEN as a recovered list.**

`[0x13B8648]==0` **does** take `"Load Particles"`
`004174F1` → open **`PARTICLE_MAIN`**. That is a
**bank**, not a running emitter. `004A67D0` is
the world ctor, not VFX.

| Claim | Class |
|---|---|
| First-seen `[0x13B8648]==0` → `004174F1` | **PROVEN** |
| First *open* is `PARTICLE_MAIN` (`00AE8980` once) | **PROVEN** |
| `PARTICLE_FRONTEND` opened on no-save | **DISPROVEN** |
| `004A67D0` is first-seen Oakvale VFX | **DISPROVEN** |
| Native `004A67D0` after `"Load Particles"` | **DISPROVEN** (inside `0041735A`, before `00417418`) |
| Host `EnterGame` still hoists `004A67D0` past particles | **DISPROVEN** stale (`00417418-world-plus60`; `InitGameStages` `"Init World"` notes `004A67D0` before `"Load Particles"`) |
| No-save first Present map | **LookoutPoint** (`CurrentRegionIndex=1`) | **PROVEN** |
| Lookout TNG `PARTICLE_EMITTER_*` count | **0** / 288 | **PROVEN** |
| Lookout `CreateEffect` / `DummyEffect` first-seen | **none** | **PROVEN** absent |
| Streetlamp `CParticleAttacherDef` `#11459` starts a system on first Present | **UNREAD** |
| Oakvale intro `PARTICLE_EMITTER_PLACEABLE` | **47** on `StartOakValeWest`; `ScriptName` **NULL** | **PROVEN** count / **UNREAD** system name |
| Those 47 first-seen C3D | **DISPROVEN** (`FirstSeenInstancesAsC3d` false) | **PROVEN** reject |
| Named fire / insects / beetles / dust *systems* on first Present | **DISPROVEN** as recovered names | **UNREAD** draw |
| `GENERIC_INTERNAL_FIREPLACE` is a particle system | **DISPROVEN** — Graphic mesh **4572** | **PROVEN** C3D |
| `WatchBarrels` first-seen spawns a beetle | **DISPROVEN** (`FirstSeenWatchBarrelsSpawnsBeetle=false`) | **PROVEN** |
| `GENERIC_DUST_EFFECT` is first-seen Oakvale/Lookout | **DISPROVEN** — `007A58C4` inside force-push `007A54A0` | **PROVEN** site |
| Particle *draw* enqueue after Leave | **UNREAD** | do not invent GPU |
| Host `004174F1` / `00AEBE20` / `00AE8980` | **Note only** | **MATCH** gate, **LEFTOVER** empty body |
| Host `CanRenderParticles` | default **false**; never set | **MATCH** unproven / **no GPU** |

**Answer (running systems):**

```
no-save first Present (Lookout):  (empty)
Oakvale intro (SHOT2 / HerosOldHouse):
  PARTICLE_EMITTER_PLACEABLE ×47   ; name UNREAD
  (no proven FIRE / INSECT / BEETLE / DUST system id)
```

Boot-only (not a running system): `PARTICLE_MAIN`.

---

## 1. Init Game gate is a bank open, not VFX

`004184BD` after `"Init Sound"`:

```
0041888B  cmp [0x13B8648], bl     ; bl=0 first-seen
00418891  jne 004188E5
00418894  push "Load Particles"
004188E0  call 004174F1
```

`004174F1` (`listing-00400000.txt`):

```
004174FA  call 00AEBE20           ; construct singletons
00417538  push "PARTICLE_MAIN"
00417550  call 00AE8980           ; only .text E8
```

`effects.big` sub-bank is `PARTICLE_MAIN_PC`
(`DataCatalogTests.Remaining_big_banks_are_bigb`).
Create later is `006E0880` → def
`PARTICLE_EMITTER_NORMAL` (not a mesh). Same as
`proofs/particles-game`.

`[0x13B8648]` is **not** particles-only
(`00412F93` also picks retail vs skip-frontend).
First-seen no-save is 0, so Load Particles **runs**.

`004A6E30` also logs `"Setting Particle Engine
Mesh/Graphic Bank"` `00AEAA90` / `00AEAA80`.
Those are **pointer stores** (`[0x13D2E0C]` /
`[0x13D2E08]`). They do **not** open a bank and
are **not** running systems.

---

## 2. `004A67D0` is not Oakvale VFX

`proofs/004A67D0-after-particles`: first *order*
DIVERGE vs an older `EnterGame` leftover was
world ctor **after** `"Load Particles"`. Native:

```
0041735A  "Init World"
  00417396  call 004A67D0         ; vtbl 012390F0, game+36
  00417410  call [eax+36]         ; 004A6E30
00417418  "Init Display Engine"
…
004174F1  "Load Particles"        ; if [0x13B8648]==0
004188E9  [game].vtbl+32 00416953
```

Only `.text` `E8` of `004A67D0` is `00417396`.
Oakvale / `00DBDE40` on this site: **DISPROVEN**.

Current host (`EngineLifecycle.InitGameStages`
`"Init World"` arm) notes `004A67D0` **before**
`"Load Particles"`.
`Init_World_004A67D0_runs_inside_0041735A_before_00417418`
locks that order. The after-particles hoist is
**stale** (`00417418-world-plus60`).

---

## 3. No-save first Present (Lookout)

ContainsMaps TNG (`006C2170`): Lookout **288**,
Bridge **88**, Guild **88**.

| Kind | Lookout | Bridge | Guild |
|---|---|---|---|
| `PARTICLE_EMITTER_*` | **0** | **0** | **0** |
| `*EFFECT*` NewThing | **0** | **0** | **0** |
| `MARKER_LIGHT` | **0** | n/a | n/a |
| `CreateEffect` / `DummyEffect` | none | none | none |

Lookout **does** have 7
`OBJECT_STREETLAMP_LIT_SINGLE_01` (mesh **4978**)
with sub `CParticleAttacherDef` `#11459`. Whether
that attacher starts `NParticleEngine` on first
Present is **UNREAD**. Do **not** emit invented
fire/glow billboards.

No-save Present is **not** Oakvale SHOT2
(environment investigation leftover #4).

---

## 4. Oakvale intro authors 47 placeables; names UNREAD

`startoak-tng.txt` / PARITY missing-mesh census:
**47** `PARTICLE_EMITTER_PLACEABLE` on
`StartOakValeWest`. All `ScriptName` **NULL**.
`Fable.Dump tng` does not print
`StartCTCDParticleEmitter` / `BankIndexName`.

Host / first-scene C3D rejects them
(`GameBin.FirstSeenInstancesAsC3d` /
`visibility-layers.txt`). Create path if later
bound is `006E0880` (`PARTICLE_EMITTER_NORMAL`).

### Fire — **DISPROVEN** as a named particle *system*

| Item | What it is | Class |
|---|---|---|
| `GENERIC_INTERNAL_FIREPLACE` @ `(26.678, 138.86, 16.854)` | Graphic **4572**, submitted C3D | **PROVEN** mesh |
| `OBJECT_OAKVALE_FIREPLACE_01/02` | game.bin OBJECT defs | **PROVEN** names; **UNREAD** as first-seen TNG here |
| Placeable emitters near HerosOldHouse (e.g. `(34.198, 133.212, 16.711)`, `(36.765, 136.054, 16.706)`, `(40.139, 137.685, 16.688)`) | `PARTICLE_EMITTER_PLACEABLE` | **PROVEN** Things; system id **UNREAD** |

Do not label those 47 `FIRE`.

### Insects / beetles — **DISPROVEN** as first-seen particle *systems*

| Item | Class |
|---|---|
| `WatchBarrels` `00DBE890` / `NOVI_Barrel` | first-seen ctor **PROVEN**; spawn beetle **DISPROVEN** |
| `NOVI_CreatedBeetle` factory `00DB7FF0` | bind **PROVEN**; first-seen fire **DISPROVEN** |
| `CREATURE_OAKVALE_STAG_BEETLE` / `CREATURE_FIREFLY` / `CREATURE_BUTTERFLY_*` | **creatures**, not `CParticleSystem` |
| Placeables near scarecrow `(75.643, 195.526, 14.649)` / `(74.258, 196.088, 17.315)` | Things **PROVEN**; insect system id **UNREAD** |

### Dust — **DISPROVEN** as first-seen

`GENERIC_DUST_EFFECT` string site is `007A58C4`
inside ability `007A54A0` (force-push), not
Lookout/Oakvale first Present. Script
`CreateEffect` first-seen on Lookout: none.
Oakvale intro script `CreateEffect` on first
SHOT2 wait: **UNREAD** as a recovered line here;
do not invent `FX_FIRE` / dust.

---

## 5. Host coverage

| Host | Native | Class |
|---|---|---|
| `SkipParticlesFirstSeen = 0` | `[0x13B8648]==0` | **MATCH** |
| `InitGameStages` last name `"Load Particles"` `004174F1` | `00418894` | **MATCH** notes |
| `Note(SkipParticlesVa, … run 004174F1)` | `00AEBE20` + `00AE8980("PARTICLE_MAIN")` | **LEFTOVER** empty |
| `RetailBanks` `PARTICLE_MAIN` / `PARTICLE_FRONTEND` pairs | `009A8150` register-only | **MATCH** names; **no** open |
| `"Init World"` notes `004A67D0` before particles | `00417396` before `004174F1` | **MATCH** order now |
| `FirstSeenInstancesAsC3d` rejects `PARTICLE_EMITTER_*` | `006E0880` not a C3D | **MATCH** |
| `Dx9SubmitCapabilities.CanRenderParticles` | default `false`; no setter | **MATCH** unproven |
| First-scene Present | particles **not submitted** | **MATCH** contract UNREAD |
| Particle GPU / `effects.big` decode / DIP | none | **UNREAD** — do **not** invent |

---

## 6. What this does **not** say

- First Present draws fireplace **particles**.
  **DISPROVEN** as proven systems; mesh only.
- Beetles / flies / dust are first-seen
  `CParticleSystem` ids. **UNREAD** names;
  WatchBarrels beetle **DISPROVEN**.
- Opening `PARTICLE_MAIN` submits sprites.
  **DISPROVEN** as a draw claim.
- `004A67D0` after particles is current host.
  **DISPROVEN** (moved inside Init World).

---

## First-seen particle list

**Running `CParticleSystem` / emitter instances**

| Path | Systems | Class |
|---|---|---|
| No-save first Present (Lookout + Bridge + Guild TNG) | **∅** | **PROVEN** empty |
| Oakvale intro (`StartOakValeWest`) | **47× `PARTICLE_EMITTER_PLACEABLE`**, `ScriptName=NULL`, `BankIndexName` **UNREAD** | **PARTIAL** |
| Named `FIRE` / insect / beetle / `DUST` | **none recovered** | **UNREAD** / **DISPROVEN** as listed ids |

**Boot (not a running system)**

| Name | Site | Class |
|---|---|---|
| `PARTICLE_MAIN` → `PARTICLE_MAIN_PC` | `004174F1` / `00AE8980` iff `[0x13B8648]==0` | **PROVEN** open |
| `PARTICLE_FRONTEND` | register only | **DISPROVEN** open |

---

## INDEX

| VA / name | Role |
|---|---|
| `0x013B8648` | skip-frontend / skip Load Particles |
| `004174F1` | Load Particles |
| `00AEBE20` | construct particle singletons |
| `00AE8980` | open `PARTICLE_MAIN` |
| `004A67D0` | world ctor — **not** VFX |
| `006E0880` | `PARTICLE_EMITTER_NORMAL` create |
| `CTCDParticleEmitter` | TNG persist class; field dump **UNREAD** |
| `CParticleAttacherDef` | streetlamp / object attach; first Present **UNREAD** |
