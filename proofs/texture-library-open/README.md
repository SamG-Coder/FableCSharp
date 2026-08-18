# textures.big / TextureLibrary first open vs frontend.big

Investigation only. Production `src/` was not edited.

Question: is `TextureLibrary` (`textures.big` / `GBANK_MAIN`) first opened **after Leave only**, or does frontend already open it? What opens `frontend.big`?

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **DIVERGE**.

Sources: `TextureLibrary.cs`, `FrontendSpriteBank.cs`, `EngineLifecycle.cs` (`OpenTextureBank`, `EnterFrontendAfterAvi`, `RequestNewGame`, `EnterGame`); ExeIndex `listing-00400000.txt` / `xrefs.tsv` / `functions.tsv`; `docs/status/investigations/2026-08-18-resource-manager.md`; `docs/runtime/FORWARD_TREE.md` §§2, 4, 6; `DataCatalogTests.Remaining_big_banks_are_bigb`.

---

## Verdict

**Yes. First-seen no-save: `textures.big` / `TextureLibrary` first open is after Leave only.**

`frontend.big` is a **different** BIGB and opens **before** Leave. Frontend sprites are not `TextureLibrary` ids.

| File / bank | First native open | First host open | Relative to Leave |
|---|---|---|---|
| `graphics/pc/frontend.big` `GBANK_FRONT_END` | `0042E98F` → `0042DDB3` → `009F83D0` | `AttachFrontendTree` → `new FrontendSpriteBank` | **before** Leave **PROVEN** |
| `graphics/pc/textures.big` `GBANK_MAIN` | `004184BD` `"Init Graphics"` → `00416C8A` → `009F83D0` | `EnterGame` → `OpenTextureBank` → `new TextureLibrary` | **after** Leave **PROVEN** |

Bootstrap `009A8150` only registers name pairs. **DISPROVEN** that it opens either file.

---

## Timeline (no-save New Game)

```
00402510 bootstrap
  009A8150  GBANK_MAIN / GBANK_MAIN_PC
            GBANK_FRONT_END / GBANK_FRONT_END_PC
            (names only; files closed)
00412F90 RunModes → 0042EA8F retail
0042F75E start
  mov [0x13B871C], esi          // retail pointer
  0042F722 intern "FRONT_END"   // string, not 009F83D0
0042EC7C pump
  006286F0 ×3 PlayAVI           // no 009F83D0
  [0x13B8616]==0 skip 009A8840
  [esi+9]=1
  0042E98F
    00595582 UI
    0042DDB3                    // FIRST frontend.big
      FourCC probes 009BE830/870/8B0
      009F83D0("GBANK_FRONT_END")  @ 0042DEA2
      004194BA store handle at retail+64
    00598A1C(0)
      skip MEDIA_PLAYER_ERROR
      0041E5F2 → 0041E3F6         // UI singleton, first ctor
        [0x13B871C]!=0 && [eax+9]!=0
        → 009F83D0("GBANK_FRONT_END") again (hit)
        skip GBANK_MAIN
      0041DB1D PRESS_START
  Init Engine / Init frontend / 0042DED5
  0042DF9E 2D UI from GBANK_FRONT_END
[esi+41] New Game
0042F2A2 Leave frontend
  0042EBB6
    mov [0x13B871C], 0
    0042DD28
      009F8290 + 00419108 on retail+64   // drop FRONT_END handle
    009BE420 + 009BEEB0
  FinalAlbion.wld
0042F491 Init Game → 00418DCA → 004184BD
  "Init Graphics" 00416C8A      // FIRST textures.big
    FourCC probes (not decode)
    "Opening Main Graphic Bank"
    009F83D0("GBANK_MAIN")       @ 00416E22
    004BBFC0 [0x13B8A08]
  later Init World / 00416953 / submit decode
```

---

## 1. Two files, two banks — **PROVEN**

`DataCatalogTests`: `frontend.big` sub-bank is `GBANK_FRONT_END_PC`.  
`TextureLibrary` ctor: `textures.big` sub-bank is `GBANK_MAIN_PC`.

Native open string is the **logical** name (`GBANK_FRONT_END` / `GBANK_MAIN`). The `_PC` remap is the `009A8150` pair. Resource-manager §3.1: `009F83D0` miss → `vtbl+4` `009D56C0` → `009A7F80` walk already-open archives, else `009A7CA0` path. First named open binds that `.big`.

`009F83D0` call sites in `.text` (all):

| VA | Function | Name pushed |
|---|---|---|
| `0042DEA2` | `0042DDB3` | `"GBANK_FRONT_END"` |
| `0041E58E` | `0041E3F6` | `"GBANK_FRONT_END"` if `[0x13B871C]+9 != 0` |
| `0041E5D0` | `0041E3F6` | `"GBANK_MAIN"` else |
| `00416E22` | `00416C8A` | `"GBANK_MAIN"` |
| `005C007E` | `005BFF07` | `[0x13B8BA4]` (appearance / later UI) |

No other `009F83D0`. **DISPROVEN** that frontend draw (`0042DF9E`) opens `textures.big`.

---

## 2. frontend.big is before Leave — **PROVEN**

`0042E98F` is after AVI and `[esi+9]=1`, still on the retail pump. First insn block after UI get is `call 0042DDB3` (`0042E9BF`).

`0042DDB3` always intern+`009F83D0("GBANK_FRONT_END")`. Width/height locals `0x800`. Not `GBANK_MAIN`.

Host: `EnterFrontendAfterAvi` → `InitFrontendUi` → `AttachFrontendTree` → `new FrontendSpriteBank(Install)` → `BigArchive.Open(.../frontend.big)`. Same stage. Press Start decode is `FrontendSpriteBank.TryLoad`, not `TextureLibrary`.

---

## 3. `0041E3F6` does **not** first-open `textures.big` on this path — **PROVEN**

UI ctor `0041E3F6` is either/or:

```
eax = [0x13B871C]
if eax==0 || [eax+9]==0
    009F83D0("GBANK_MAIN")
else
    009F83D0("GBANK_FRONT_END")
```

First-seen:

| Fact | Evidence |
|---|---|
| `[0x13B871C] = retail` before first pump | `0042F761` |
| `[esi+9]=1` before `0042E98F` | `0042EF35` |
| First `0041E5F2` that constructs | `00598BCB` inside `00598A1C`, **after** `0042DDB3` |
| Singleton `[0x13B8710]` | later `0042DED5` / `0042E3EE` hit, no second ctor |

So first-seen `0041E3F6` takes **FRONT_END** (already open). **DISPROVEN** that first-seen UI ctor binds `textures.big`.

`[0x13B871C]+9==0` **would** open `GBANK_MAIN` at first ctor. That is **not** first-seen no-save. Editor / skip-frontend (`[0x13B8648]`) is **UNREAD** here.

Native `0042E3EE` is the **frontend frame** loop, after Init frontend. Host `Pump()` also `PumpInput()` during `StartupVideos`. That extra poll does **not** construct `TextureLibrary` (`EngineInput.Construct` has no bank open). Native AVI is blocking `006286F0` before `[esi+9]=1`, so a hypothetical AVI-time `0041E3F6` would have taken MAIN. **DISPROVEN** as first-seen (no `0041E5F2` on the AVI table). Host AVI `PumpInput` is a **DIVERGE** vs native order, not a `textures.big` open.

---

## 4. TextureLibrary / `textures.big` after Leave only — **PROVEN**

Leave `0042F2A2` → `0042EBB6` → `0042DD28` releases `retail+64` (`009F8290`). That is the FRONT_END handle from `0042DDB3`. Whether the `.big` file object hits refcount 0 is **PARTIAL** (generic handle release). It is **not** an open of `GBANK_MAIN`.

`004184BD` third named stage is `"Init Graphics"` `00416C8A`. Same function later `"Opening Main Graphic Bank"` / `push "GBANK_MAIN"` / `009F83D0`. Directory only (`009D56C0` / `009CFBC0`). Decode is later `009FD910` / host `TryLoad`. **DISPROVEN** that `00416C8A` decodes payloads (resource-manager; FourCC probes first).

Other `00416C8A` callers (`00417747` when `[game+52]`, `004179EE`) are **not** first-seen (`FORWARD_TREE` `[game+52]!=0` not first-seen).

Host:

```
EnterGame
  if Frontend → RequestNewGame (Leave)
  foreach InitGameStages
    if "Init Graphics" → OpenTextureBank()
BindSubmittedTextures → OpenTextureBank()   // no-op if already open
```

`OpenTextureBank` is the only production `new TextureLibrary`. Guard `Textures is not null`. `Program.cs` does not construct it. Frontend `PumpFrontendFrame` does not call it.

---

## 5. Host vs native

| Item | Class |
|---|---|
| Names-only at `009A8150` | **MATCH** `RegisterRetailBankTable` |
| `frontend.big` directory at `0042E98F` / `FrontendSpriteBank` ctor | **MATCH** stage (before Leave) |
| `textures.big` directory at `00416C8A` / `TextureLibrary` ctor | **MATCH** stage (after Leave) |
| Frontend draw uses FRONT_END ids, not MAIN | **MATCH** |
| Decode at first `TryLoad` / submit, not ctor | **MATCH** (format DXT vs RGBA is a later DIVERGE) |
| Leave `0042DD28` drops FRONT_END handle | host keeps `_frontendSprites` until `Dispose` — **DIVERGE** lifetime, not first-open |
| `0041E3F6` bank pick | host `EngineInput.Construct` opens neither file — **DIVERGE** vs native FRONT_END hit; still no early MAIN |
| `TextureLibrary` during frontend | **DISPROVEN** in host production path |

---

## Do not

- Open `textures.big` / construct `TextureLibrary` at Bootstrap, AVI, or Press Start.
- Treat Press Start `GraphicIndex` as a `textures.big` id.
- Decode `GBANK_MAIN` at Init Graphics (directory only).
- Use `0041E3F6` as the first-seen `GBANK_MAIN` site.
- Collapse `frontend.big` and `textures.big` into one library.

---

## Open

- `009A7CA0` exact path string for each `.big` (**PARTIAL**; host hard-paths `graphics/pc/{frontend,textures}.big`).
- `009F8290` refcount: does Leave close the `frontend.big` file object or only the retail+64 handle (**PARTIAL**).
- First `009FD910` / DXT CreateTexture of a MAIN id after Leave (**UNREAD** this pass; not required for *open*).
- Skip-frontend / `[0x13B8648]` first bank (**UNREAD**; not no-save).
