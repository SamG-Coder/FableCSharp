# When is the script bank first opened on the process-entry path?

Investigation only. No production `src/` edits.

Do **not** start at `00DBDE40` / `S_QNOVI` / `CS_OAKVALE_INTRO_FATHER`.
That is later leftover `Q_NewOakValeIntro`. Frontend must **not**
run quest scripts or `CCutsceneDef` interpreters.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources:

- `src/Fable.Game/ScriptBank.cs`
- `tools/Fable.ExeIndex/out/01-sections/script-bank/` (`INDEX.md`,
  `newgame.md`, `native-sqnovi.md`, `exe-commands.md`, `entries-tsv.md`)
- `tools/Fable.ExeIndex/out/01-sections/script-runtime/registering-scripts-00cb5d80-00cb5d80.md`
- `tools/Fable.ExeIndex/out/01-sections/script-runtime/script-def-00f2a0f0-00f2a0f0.md`
- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00480000.txt` (`004A6550`)
- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00c80000.txt` (`00CB5C70` / `00CB5D80` / `00CBF647`)
- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00cc0000.txt` (`00CD3EF0` / `00CD3F00` / `00CD3F50` / `00CD2994`)
- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00980000.txt` (`009B08C0` / `009ADA40` / `009AD410`)
- `tools/Fable.ExeIndex/out/01-sections/text-map/e8.tsv`
- `docs/runtime/FORWARD_TREE.md` §§1–4, 6–7, 10
- `EngineLifecycle.LoadWorld` / `InitCharactersAndQuests`
- `proofs/camera-after-leave/README.md`, `proofs/newgame-script/README.md`

---

## Verdict

**First native script-bank open is the first instruction of
Loading world `00416953`, not frontend and not Init Quests.**

```
00416953 Loading world
  00416968  [world].vtbl+28([game+40])     // WorldPrepareVtbl
    004A6550  "Init Scripts"
      006E7740  script manager → world+56
      00CB5C70  32-byte list   → world+88
      004A6677  call 00CB5D80               // only E8
        "Registering Script Defs"
        00F2A0F0  CScriptDef / CCutsceneDef / CRegionScriptDef
        [0x143E920].vtbl+8
          00CD3F50  path list + 009B08C0     // file open
          00CBF647  "SCRIPT_DEF" → 00CD2994 / 009ADA40
        "Registering Scripts"
        00CD52D0  quest name → factory
        [esi+17]==0  skip 00CB7780
  then "Loading world" / 004A1840 / 004B4260
```

`script.bin` is **not** a literal in `Fable.exe`. The compiled
bank is the `SCRIPT_DEF` GameBin (`ScriptBank.Load` / 611 entries).
`S_QNOVI` is **not** in that file.

Frontend opens **`frontend.bin`** only. Attract cutscenes live
on disk and are unused until this walk.

---

## Timeline (process entry → first open)

```
00401067 PE / CRT
00403480 WinMain
00402510 bootstrap
00412F90 RunModes
0042EA8F retail
0042EC7C pump
  006286F0 PlayAVI ×3
  0042E98F  UI_FRONTEND_PRESS_START_MENU     // frontend.bin
  Init Engine / Init frontend
  loop 0042E3EE / 0042DC94 / 0042DF9E
    msg 15 → [retail+41]=1
0042F2A2 Leave frontend
0042F491 Init Game → 00418DCA → 004184BD
  Init Definition Manager 00416005(1)        // not script.bin
  Init World 0041735A / 004A6E30             // no 00CB5D80
  00416953 Loading world                     // FIRST script bank
    00416968 vtbl+28 → 004A6550 → 00CB5D80
    004A1840 WLD / QST
    0049F180 / 004B4260 Init Quests          // C# ScriptBank.Load
```

WinMain / bootstrap / retail pump do **not** `E8` `00CB5D80`.
**PROVEN** (`e8.tsv`: sole caller `004A6677`).

---

## 1. Native first open

| Step | VA | What | Class |
|---|---|---|---|
| Site | `00416968` | `[world].vtbl+28([game+40])` first insn of `00416953` | **PROVEN** listing |
| Body | `004A6550` | `"Init Atmos"` then `"Init Scripts"` | **PROVEN** |
| Manager | `006E7740` | alloc 80, store `world+56` | **PROVEN** |
| List ctor | `00CB5C70` | zeros; `[+17]=0`; store `world+88` | **PROVEN** |
| Open | `00CB5D80` | only `E8` is `004A6677` | **PROVEN** |
| Types | `00F2A0F0` | `009B0AC0` factories `00F29F40` / `00F2A0D0` / `00F29FA0` | **PROVEN** |
| Persist analog | `00F2A1D0` | eight `004331F9` → `this+60`…;`ScriptBank.TryReadCutsceneVectors` | **PROVEN** format; first instance **PARTIAL** |
| Global | `00CD3F00` / `00CD3F40` | 0xD0 object vtbl `012C2648` → `[0x143E920]` | **PROVEN** |
| `vtbl+8` | after `00F2A0F0` | `call [edx+8]` | **PROVEN** call. Slot body **PARTIAL** (see below) |
| File open | `009B08C0` @ `00CD422B` | `00999230` exists; `00994700` open; `009AFB90` parse | **PROVEN** as this manager’s open |
| Handle | `00CBF647` | push `"SCRIPT_DEF"`; `00CD2994` → `009ADA40` → `009AD410` → `[0x143E90C]` | **PROVEN** |
| Quest table | `00CD52D0` | `"Registering Scripts"`; bind `00CB5C90` | **PROVEN** |
| Start list | `00CB7780` | `[esi+17]` ctor 0 → skip | **PROVEN** first-seen skip |

`00CD3F50` is the method after the store-to-`[0x143E920]`. It
builds the same `00412330` path list as Game Definition Manager,
then `009B08C0` and the only `E8` to `00CBF647` (`e8.tsv`
`00CD424C`). Identifying it as `[0x143E920].vtbl+8` is
**PARTIAL** (rdata slot unread). The open still happens inside
`00CB5D80` on this path. **PROVEN** as first-seen.

`009B08C0` has two other `E8`s. They are **other** managers:

| Site | Manager | File analog | Class |
|---|---|---|---|
| `00433C7B` | CONTROL_SCHEME / CONFIG_OPTIONS_DEFAULTS_DEF | `frontend.bin` candidate | **PARTIAL** name; **DISPROVEN** as `script.bin` |
| `0044E95C` | `"Game Definition Manager: Compile"` then GLOBAL / ENGINE / ENVIRONMENT | `game.bin` | **PROVEN** other bank |
| `00CD422B` | after `"Registering Script Defs"` + `"SCRIPT_DEF"` | `script.bin` | **PROVEN** this bank |

No `.bin` literals in the exe. C# path is
`CompiledDefs/script.bin` via `GameInstall.FindCompiledDef`.

`Init Definition Manager` `00416005(1)` is `0044C6B0` then
`009ACB10` → `009E5250` (list reset). **DISPROVEN** as
`script.bin` open.

`004A6E30` Init World Init has no `00CB5D80`. **PROVEN**.

---

## 2. Frontend must not run quest scripts

| Claim | Class | Evidence |
|---|---|---|
| Frontend Present is 2D UI | **PROVEN** | FORWARD_TREE §4; `0042DF9E` / `VSHADER_2D_SPRITE` |
| `0041DB1D` binds Press Start from `frontend.bin` | **PROVEN** | 810 UI entries; `009AD410` |
| `CS_ATTRACT_*` exist in `script.bin` | **PROVEN** | `entries-tsv.md` index 3–14; `DataCatalogTests` |
| Frontend / Leave starts `CS_ATTRACT_*` or any `CCutsceneDef` | **DISPROVEN** | no `00CBFB7D` on `0042EC7C`; `camera-after-leave` |
| Frontend `E8`s `00CB5D80` / `00CBF647` / `00CD2994` | **DISPROVEN** | one caller each, all after Leave |
| `00CB7780` on first `00CB5D80` | **DISPROVEN** | `[+17]=0` |
| WLD `CS_PlayCutscene` is a `CCutsceneDef` | **DISPROVEN** | factory `00F01760` empty; `ScriptName==null` |
| `S_QNOVI` in `script.bin` | **DISPROVEN** | `newgame.md`; `FirstSeenScriptBinHasSqnovi=false` |

Host `ResolveFrontendDef` / `FrontendSpriteBank` may open
`frontend.bin` + `names.bin` at `0042E98F`. They must **not**
`ScriptBank.Load` or `StartCutscene`.

---

## 3. `ScriptBank.cs` vs `script-bank/` dump

`ScriptBank.Load` = `names.bin` + `script.bin` GameBin, then
`FromEntry` / `TryReadCutsceneVectors` (persist `00F2A1D0`,
eight CString vectors, commands at `+60`).

That is the **compiled bank parse**, not the quest VM and not
`S_QNOVI`.

| Dump / type | Role | In `ScriptBank`? |
|---|---|---|
| `CCutsceneDef` (~500 of 611) | persist vectors; runner `00CBFB7D` | **yes** (`Find` / `Commands`) |
| `CS_ATTRACT_*` | attract lists on disk | **yes**; frontend must not run |
| `CS_OAKVALE_INTRO_FATHER` | later leftover start `00DB86B0` | **yes**; not first after Leave |
| `CS_BANDITRAID_*` / `CS_CHICKING_*` | later | **yes** |
| `S_QNOVI` | native factory `00DBEF70` | **no** (`native-sqnovi.md`) |
| `00CD52D0` quest table | `QuestFactoryTable` | **not** `ScriptBank` |
| exe tokens `0x012C1500`–`0x012C2C00` | `exe-commands.md` | **not** the bank file |

`ExtractCommands` (printable scrape) is **DISPROVEN** as the
list `00CBFB7D` walks. Use vector 0 only.

---

## 4. C# vs native timing

| Host | Native | Class |
|---|---|---|
| `ResolveFrontendDef` / `FrontendDefs` | `0041DB1D` `frontend.bin` | **PROVEN** pairing |
| `LoadWorld` notes `WorldPrepareVtbl` | `00416968` | **PROVEN** note; **no** `00CB5D80` / `ScriptBank.Load` |
| `ScriptBank.Load` in `InitCharactersAndQuests` | `004B4260` after `004A1840` | **DIVERGE** late vs `00416968` |
| `Runtime.ActivateQuest` at Init Quests | `00CB5AD0` factories; no `00CBFB7D` | **PROVEN** vs Leave |
| `ScriptRuntime.StartNewGame` / Oakvale TNG | not this path | **DIVERGE** leftover |

Opening `script.bin` at Init Quests is **after** Leave and
**after** native first open. It does not make frontend run
quests. Moving the parse up to `00416968` would match native
order; running any `CCutsceneDef` there would not
(`00CB7780` skipped; `00CBFB7D` unread as this site).

---

## Classifications (short)

1. **First script-bank open — `00416953` `[world].vtbl+28` → `004A6550` → `00CB5D80` → `009B08C0` / `"SCRIPT_DEF"`. PROVEN.**
   After Leave / Init World. Before WLD / Init Quests.
2. **Frontend opens `script.bin` or runs quest / attract cutscenes — DISPROVEN.**
   Frontend is `frontend.bin` UI only.
3. **`ScriptBank.cs` = persist parse of `script.bin` `CCutsceneDef`s. PROVEN.**
   Not `S_QNOVI`. Not `00CB5AD0`.
4. **C# `ScriptBank.Load` at `004B4260` — DIVERGE late** vs native
   `00416968`. Still not frontend.

## Do not invent

- `CS_ATTRACT_*` as Press Start / New Game cinema.
- `S_QNOVI` / `00DBDE40` as the first bank consumer.
- `script.bin` as a string in `.text`.
- `00416005` as the script-bank file open.
- Starting `00CBFB7D` because the bank just opened.
