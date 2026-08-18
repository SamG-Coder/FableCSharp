# Audit: `ScriptBank.cs` vs dump `script-bank/` — frontend? wrong bank?

Investigation only. No production `src/` edits.

Do **not** treat dump `newgame.md` / `CS_ATTRACT_*` / `CS_OAKVALE_INTRO_FATHER`
as the Press Start bank. Frontend is `frontend.bin`.
Do **not** treat dump `native-sqnovi.md` as a `script.bin` entry.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources:

- `src/Fable.Game/ScriptBank.cs`
- `src/Fable.Game/EngineLifecycle.cs` (`InitFrontendUi` /
  `ResolveFrontendDef` / `LoadWorld` / `InitCharactersAndQuests` /
  `EnsureDefs`)
- `src/Fable.Game/ScriptRuntime.cs` (`Load` / `StartNewGame`)
- `src/Fable.Core/GameInstall.cs` (`FindCompiledDef`)
- `tools/Fable.ExeIndex/Program.cs` `RunExportScriptBank` (v8)
- `tools/Fable.ExeIndex/out/01-sections/script-bank/` (`INDEX.md`,
  `entries-tsv.md`, `newgame.md`, `quests-qst.md`, `exe-commands.md`,
  `native-sqnovi.md`, `0481-cs-oakvale-intro-father.md`)
- `docs/runtime/SCRIPT_FORMAT.md`
- `proofs/script-bank-open/README.md`
- `DataCatalogTests.Frontend_and_script_bins_are_gamebin`
- `WorldSceneTests.Cutscene_commands_come_from_persist_vectors_not_ascii_scrape`
- `ScriptRuntimeParityTests.Cutscene_layout_is_persist_vectors_not_ascii_scrape`

---

## Verdict

**`ScriptBank.cs` is the compiled `script.bin` persist parse. It does
not open during frontend. It is not the wrong GameBin.**

Frontend opens **`frontend.bin`** (`0042E98F` / `0041DB1D` /
`ResolveFrontendDef`). Native first `script.bin` analog is
**after Leave**, first insn of Loading world `00416953` →
`004A6550` `"Init Scripts"` → `00CB5D80` / `"SCRIPT_DEF"`.
C# `ScriptBank.Load` is **later still**: `InitCharactersAndQuests`
at `004B4260`.

The dump family is **wider** than `ScriptBank.cs`. Only
`entries-tsv.md` + per-entry cutscene parts are that GameBin.
`quests-qst.md`, `exe-commands.md`, and `native-sqnovi.md` are
other stores. `newgame.md` is a host filter, not a native bank.

| Claim | Class |
|---|---|
| `ScriptBank.Load` = `names.bin` + `CompiledDefs/script.bin` | **PROVEN** |
| Dump TSV is that same GameBin (611 entries) | **PROVEN** |
| Frontend Present / Press Start opens `script.bin` | **DISPROVEN** |
| Frontend runs `CS_ATTRACT_*` or any `CCutsceneDef` | **DISPROVEN** |
| `S_QNOVI` is in `script.bin` / `ScriptBank` | **DISPROVEN** |
| Dump `ExtractAscii` = runner command list (`+60`) | **DISPROVEN** |
| C# load site `004B4260` vs native first open `00416968` | **DIVERGE** late |
| Dump `newgame.md` = first-seen New Game bank | **DISPROVEN** |

---

## 1. What `ScriptBank.cs` is

`Load(GameInstall)`:

```
FindCompiledDef("names.bin")
FindCompiledDef("script.bin")
GameBin.Load → FromEntry per named instance
```

`FromEntry` proves layout **only** for `CCutsceneDef`:
`TryReadCutsceneVectors` = persist `00F2A1D0`, 5-byte GameBin
prefix, eight `004331F9` CString vectors (skip u32 + count +
NULs). `Commands` is vector 0 (`this+60`). Light defs =
vector 3 (`+84`); light scenes = vector 4 (`+96`).

Non-cutscene types (`CScriptDef`, `CRegionScriptDef`) stay in
`Entries` / `Find` with empty `Commands` and
`CommandsLayoutProven=false`. Persist of those types is
**UNREAD** here.

`ExtractCommands` is a printable scrape. Comment and tests
mark it **DISPROVEN** as the list `00CBFB7D` walks.

Dump father hex (`0481-cs-oakvale-intro-father.md`) matches
the parser:

```
01 00 01 00 00     preamble (5)
5A 1E 6C A9        skip u32
3C 00 00 00        count 60
"PlayMusic MUSIC_SET_NULL" NUL …
```

`ScriptBank.Commands[0]` / dump scrape first line / tests all
pin `PlayMusic MUSIC_SET_NULL`. Skip vector 1 starts `FadeOut`
with no args — in dump scrape, **not** in `Commands`.

---

## 2. Dump `script-bank/` vs the class

Exporter: `RunExportScriptBank`, family `script-bank`,
`DumpStore.ScriptBankVersion = 8`. Same
`FindCompiledDef("script.bin")` as `ScriptBank.Load`.

| Dump part | What | In `ScriptBank.cs`? |
|---|---|---|
| `entries-tsv.md` | 611 rows: type / instance / raw / 8 scrape strings | **yes** as `Entries` (parse is vectors, not scrape) |
| `00xx-*.md` cutscenes | filtered subset + hex + scrape | **yes** if `CCutsceneDef` |
| `CCutsceneDef` (~596 of 611) | persist vectors | **yes** |
| `CScriptDef` `NULLDEF_*` / `SCRIPT_DEF` (idx 0, 597) | GameBin types; scrape is junk | **named only**; vectors **UNREAD** |
| `CRegionScriptDef` `REGION_*` (idx 2, 598–610) | 37-byte stubs | **named only** |
| `CS_ATTRACT_1`–`12` (idx 3–14) | on disk | **yes**; frontend must not run |
| `CS_OAKVALE_INTRO_FATHER` (idx 481) | leftover start `00DB86B0` | **yes**; not first after Leave |
| `newgame.md` | `IsNewGameScript` string filter (NOV / OakVale / Q_New / …) | **not** a native set; flags `CS_ATTRACT_12` because strings mention OakVale |
| `quests-qst.md` | `FinalAlbion.qst` | **no** (`QuestFile`) |
| `exe-commands.md` | `.rdata` `0x012C1500`–`0x012C2C00` | **no** |
| `native-sqnovi.md` | factory `00DBEF70` / `00DBDE40` | **no** (`S_QNOVI` absent from TSV) |

Dump per-entry `strings:` = `ExtractAscii` (same loop as
`ScriptBank.ExtractCommands`). Mixed vectors + binary
garbage (`NULLDEF_CScriptDef` / `SCRIPT_DEF`). **DISPROVEN**
as `Commands`.

---

## 3. Opens during frontend?

**No.**

```
0042EC7C  retail pump
  006286F0 PlayAVI ×3
  0042E98F  UI_FRONTEND_PRESS_START_MENU
    InitFrontendUi
      ResolveFrontendDef → frontend.bin + names.bin
      FrontendSpriteBank → frontend.bin
      EnsureDefs fallback → game.bin (WorldGeometry), not script.bin
  loop 0042E3EE / 0042DC94 / 0042DF9E     // 2D UI
0042F2A2 Leave frontend
0042F491 Init Game
  Init Definition Manager / Init World     // not script.bin
  00416953 Loading world                   // NATIVE first SCRIPT_DEF open
    00416968 [world].vtbl+28 → 004A6550 → 00CB5D80
    004A1840 WLD / QST
    0049F180 / 004B4260 Init Quests        // C# ScriptBank.Load
```

C# `ScriptBank.Load` sites in `src/`:

- `EngineLifecycle.InitCharactersAndQuests` (`004B4260`)
- `EngineLifecycle.ActivateNamedQuest` (only if `Runtime.Bank` null)
- `ScriptRuntime.StartNewGame` (**LEFTOVER** Oakvale path)

`InitFrontendUi` / `ResolveFrontendDef` never call it.
`ScriptBank` / `script.bin` have **zero** hits in
`EngineLifecycleTests` frontend cases.

Native: sole `E8` of `00CB5D80` is `004A6677` (after Leave).
Frontend `E8` of that registrar is **DISPROVEN**.
`CS_ATTRACT_*` exist in the TSV; starting them from Press
Start / Leave is **DISPROVEN** (`proofs/script-bank-open`,
`camera-after-leave`).

---

## 4. Wrong bank?

**The file is the right bank. The dump folder is not only that bank.
The C# *time* is late.**

| Bank | Native | C# | Frontend? |
|---|---|---|---|
| `frontend.bin` | `0041DB1D` / `009AD410` | `FrontendDefs` | **yes** |
| `game.bin` | Game Definition Manager `0044E95C` | `EnsureDefs` / `WorldGeometry` | lookup fallback only |
| `script.bin` `SCRIPT_DEF` | `00CB5D80` @ `00416968` | `ScriptBank.Load` @ `004B4260` | **no** |
| `FinalAlbion.qst` | `004A0D90` | `QuestFile.Load` | **no** |
| `S_QNOVI` | factory `00DBEF70`, not a bin row | `QuestFactoryTable` / leftover `StartNewGame` | **no** |

Wrong-bank traps:

1. Dump `newgame.md` as “what New Game opens” — it is a
   substring filter. Attract-12 is tagged; `S_QNOVI` is not
   even in the file.
2. Dump scrape as cutscene commands — skip/light vectors leak in.
3. Opening `script.bin` because Press Start might play
   `CS_ATTRACT_*` — those defs sit on disk unused until
   Init Scripts.
4. Equating `ScriptBank` with the quest VM / `00CD52D0` table —
   dump documents that in other parts; the class does not.

Moving parse up to `00416968` would match native order.
Running a `CCutsceneDef` there would not (`00CB7780` skipped
on first `00CB5D80`; `00CBFB7D` is not this site).

---

## Classifications (short)

1. **`ScriptBank.cs` = `script.bin` GameBin + `CCutsceneDef` persist vectors. PROVEN.**
   Same path as dump TSV. Not QST. Not exe tokens. Not `S_QNOVI`.
2. **Frontend opens `script.bin` / `ScriptBank` — DISPROVEN.**
   Frontend is `frontend.bin` UI.
3. **Dump `script-bank/` as a single “script bank” object — PARTIAL.**
   TSV is the bank; other INDEX parts are adjacent dumps.
4. **C# `ScriptBank.Load` at Init Quests — DIVERGE late** vs native
   Init Scripts. Still after Leave. Still not frontend.

## Do not invent

- `CS_ATTRACT_*` as Press Start cinema.
- `script.bin` as a literal in `Fable.exe`.
- Dump ASCII scrape as `00CBFB7D`’s list.
- `00416005` Init Definition Manager as the script-bank open.
- Starting `00CBFB7D` because the bank just opened.
