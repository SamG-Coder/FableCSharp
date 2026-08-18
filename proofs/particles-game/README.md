# PARTICLE_MAIN vs PARTICLE_FRONTEND first use

Investigation only. No production `src` edits.

Do **not** treat Press Start sunbeams as `NParticleEngine`.
Do **not** open `PARTICLE_FRONTEND` because the name sits next to
`PARTICLE_MAIN` in the retail pair table.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Sources: `docs/runtime/FORWARD_TREE.md` §§2–7, 10;
`docs/status/investigations/2026-08-18-resource-manager.md`;
`docs/status/investigations/2026-08-18-scene-layers.md`;
`EngineLifecycle.cs` (`SkipParticlesVa`, `RetailBanks`, `InitGameStages`);
`FrontendWidgetType.cs` / `FrontendSpriteBank.cs`;
`export/frontend/press-start-frame.txt`;
ExeIndex `strings.tsv` / `xrefs.tsv` / `listing-00400000.txt` /
`listing-00ac0000.txt` / `listing-006c0000.txt`.

---

## Verdict

| Claim | Class |
|---|---|
| Both names are registered at bootstrap, names only | **PROVEN** |
| First *open* is `PARTICLE_MAIN` in `004174F1` after Leave | **PROVEN** |
| `PARTICLE_FRONTEND` is opened on no-save New Game | **DISPROVEN** |
| `[0x13B8648]` first-seen is 0 and that run takes Load Particles | **PROVEN** |
| `[0x13B8648]` is a particles-only flag | **DISPROVEN** |
| Frontend sunbeams are type-18 swap + `frontend.big` 2D tiles | **PROVEN** |
| Frontend sunbeams come from `PARTICLE_FRONTEND` / `effects.big` | **DISPROVEN** |
| First-seen Oakvale TNG emits `PARTICLE_EMITTER_*` | **DISPROVEN** |
| First particle *draw* after Leave | **UNREAD** |

---

## Timeline (no-save New Game)

```
00402510 bootstrap
  Setup basic retail banks 00402845
    009A8150(0x13CA79C, logical, pc)   // names only; no BIGB
      PARTICLE_MAIN / PARTICLE_MAIN_PC
      PARTICLE_FRONTEND / PARTICLE_FRONTEND_PC

00412F90 RunModes
  [0x13B8648]==0 && [0x13B8605]==0 && [0x13B8642]==0
    → retail 0042EA8F   // not skip-frontend

0042EC7C retail pump
  PlayAVI ×3
  0042E98F UI bind
    00AEAA80([retail+64])              // store graphic bank ptr only
  Init Engine 0042E204
    00B4AC10 / 00BAD040 VSHADER_2D_SPRITE
    00BB1640 types 0x22 / 0x23
  Init frontend 0042EF6F
  0042DF9E 2D UI                       // sunbeam tiles here

0042F2A2 Leave frontend
  009BE420 + 009BEEB0 black Present

00418DCA / 004184BD Init Game
  … Init World 0041735A / 004A6E30
      00AEAA90 world+60 mesh bank
      00AEAA80 game+90568 graphic bank
  … Init Sound 00417A58
  [0x13B8648]==0                       // 0041888B
    "Load Particles" / "Loading particles"
    004174F1                           // FIRST bank open
      00AEBE20                         // FIRST engine construct
      00AE8980("PARTICLE_MAIN")        // only E8 of 00AE8980
  [game].vtbl+32 00416953
  [0x13B8648]==0 after world
    0049F180 / 004B4A10
```

`effects.big` sub-bank name is `PARTICLE_MAIN_PC`
(`DataCatalogTests.Remaining_big_banks_are_bigb`).
`009A8150` remaps `PARTICLE_MAIN` → that PC name. **PROVEN**.

---

## 1. Register is not use

`009A8150` / `009AC700` / `0099EFB0` insert interned pairs into
`[0x13CA79C]+24`. **No file I/O.** Same as `GBANK_*`.

String xrefs:

| String | Sites |
|---|---|
| `PARTICLE_MAIN` | `0040294F` register; `00417539` Load Particles |
| `PARTICLE_MAIN_PC` | `0040295F` register only |
| `PARTICLE_FRONTEND` | `00402995` register only |
| `PARTICLE_FRONTEND_PC` | `004029A5` register only |
| `Load Particles` / `Loading particles` | `00418894` / `004188B5` in `004184BD` |

`call 00AE8980` appears **once** in the mapped `.text`
(`00417550`). Argument is the `PARTICLE_MAIN` string just pushed.
**DISPROVEN** that frontend or Leave opens `PARTICLE_FRONTEND`.

Host `RegisterRetailBankTable` MATCHES the five pairs. Host does
**not** implement `004174F1` / `00AE8980` (trace-only
`Note(SkipParticlesVa, … run 004174F1)`).

---

## 2. `004174F1` Load Particles

Gated at `0041888B`: `cmp [0x13B8648], bl` / `jne 004188E5`.
`bl` is 0 from the Init Game walk. First-seen therefore **runs**.

```
004174F1  ecx = game
  00AEBE20
    alloc 76 → 00AE8BF0 → store [0x13D2E14]   CParticleDataBank-ish
    alloc 0x1018 → 00AEBA90 → [0x13D2E1C]
    alloc 16 → 00AF2970 → [0x13D2E24]
  alloc 36 wrapper 0041956C / 00418FBC
  00AEAB20(wrapper)                       set [0x13D2E30]
  00AEAAE0 → [0x13D2E14]
  00AE8980(that, "PARTICLE_MAIN")
    alloc 0x234 → 00AF1670
      009D5F80 base bank
      +356 table vtbl 0129F380
      this vtbl 0129F38C
    [bank].vtbl+4(72, name)               open (same family as 009D56C0)
    009CC530(0, manager)
```

`00AEBE20` is also a **single** `.text` call (`004174FA`).
Frontend never constructs the particle engine.

`00AEAA80` / `00AEAA90` are two-instruction stores
(`[0x13D2E08]=ecx` / `[0x13D2E0C]=ecx`). They do **not** open a
bank. Init frontend `0042E9D5` and Init World `004A753D` /
`004A7575` only publish already-open graphic/mesh objects.

Particle create later is `006E0880` → def
`PARTICLE_EMITTER_NORMAL` (not a C3D). First-seen Lookout /
Oakvale TNG has **no** `PARTICLE_EMITTER_*`. Draw enqueue after
Leave is **UNREAD** (scene-layers: do not invent a `0x20` soup).

---

## 3. `[0x13B8648]` (`SkipParticlesVa`)

Host name is narrower than the byte.

| Site | When first-seen 0 |
|---|---|
| `00412F93` RunModes | take retail frontend, **not** skip to `00418DCA` |
| `0041888B` Init Game | run `004174F1` |
| `004188EC` after `00416953` | `0049BA70` / `00416392` / `004AE9D0` / ini |
| `00416ABF` Load world | `0049F180` not editor `0049DDD0`… |
| `004474BA` Player Interface ctor | `00415FBC` / `00446EF0` |
| `006E083E` particle type bind | `00AEAAE0` / `009E5170` |
| `006E0970` create helper | may call `006E0880` |

Writes of `1` are **not** on no-save New Game:

- `00413A8C` / `00413B55` inside CLI `004138D0`
  (`staticmap` / `build_retail_static_maps`)
- BSS default is 0 (`SkipParticlesFirstSeen`)

`[0x13B8648]!=0` also skips frontend (`00412FA6 → 0042F70B`)
and skips Load Particles. Editor / static-map / skip-frontend
flag. **DISPROVEN** that it only means “skip particle sprites”.

---

## 4. Frontend sunbeam is type 18 UI, not a particle bank

Press Start child walk (`export/frontend/press-start-frame.txt`):

```
UI_BLENDING_BACKGROUNDS_FORREST          type 5
  UI_SWAPPING_FORREST_SUNBEAM            type 18   CSwappingStateComponent
    BLENDING_BG_FORREST_SUNBEAM_1        type 5    first-seen +332=0 → visible
      UI_FRONTEND_BG_FORREST_SUNBEAM_1_* type 0    FORREST_SUNBEAM_1_* id 230–235
    BLENDING_BG_FORREST_SUNBEAM_2        type 5    hidden
    BLENDING_BG_FORREST_SUNBEAM_3        type 5    hidden
  UI_SWAPPING_FORREST                    type 18
    BLENDING_BG_FORREST_1                type 5    visible FORREST_1_*
```

Ctor `00547600`, size `0x170`. `vtbl+8 == 00530260` draws the
`+176` child list. `vtbl+192 == 0052CF40` picks child `+332`.
First-seen `+324/+328/+332=0` keeps persist child 0
(`FrontendWidgetFactory.ApplyFirstSeenState`).

Tiles bind persist `GraphicIndex` `0x38E36902` →
`graphics/pc/frontend.big` / `GBANK_FRONT_END_PC`.
`FORREST_SUNBEAM_1_1` is id **230**, not an effects.big particle.

Draw path is the same 2D sprite queue as the title:

`0041AFA0` → `0041BEB0` record type **`0x22`** (not 18) →
`00B23BC0` / `00B324A0` → `00BACFD0` / `00BAD8A0` /
`00BAE2D0` `VSHADER_2D_SPRITE`.

Type **18** is the swap *widget*. Type **0x22** is the sprite
*record*. Neither is `NParticleEngine` (`CPSCRenderSprite` et al.).

After Leave, `0042EBB6` clears + Presents black. Those type-18
trees are not the first game particle use.

---

## 5. Host leftovers

| Host | Native | Class |
|---|---|---|
| `RetailBanks` includes both pairs | `009A8150` | **MATCH** |
| `SkipParticlesFirstSeen = 0` always runs the Note for `004174F1` | gate is real | **MATCH** gate, **no** `00AEBE20`/`00AE8980` |
| `OpenTextureBank` is `GBANK_MAIN_PC` | not particles | **MATCH** (different bank) |
| Frontend sunbeam via `FrontendSpriteBank` | `frontend.big` | **MATCH** |
| Particle draw / `effects.big` decode | none | **UNREAD** / leftover empty |

---

## INDEX

| VA | Role |
|---|---|
| `0x013B8648` | skip-frontend / editor / skip Load Particles |
| `00402845` / `009A8150` | register pairs |
| `004174F1` | Load Particles |
| `00AEBE20` | construct particle singletons |
| `00AE8980` / `00AF1670` | open `PARTICLE_MAIN` (`0x234`, vtbl `0129F38C`) |
| `00AEAA80` / `00AEAA90` | store graphic / mesh bank ptrs |
| `0042E98F` | Init frontend; `00AEAA80` only |
| `00547600` | type 18 swap ctor |
| `0041BEB0` | type `0x22` 2D sprite pack |
| `006E0880` | `PARTICLE_EMITTER_NORMAL` create |
