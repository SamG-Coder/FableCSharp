# First scripted cutscene after Leave (not AVI)

Investigation only. No production `src` edits.

Do **not** start at startup PlayAVI (`0042EC7C` / `006286F0` ×3).
That is a texture-renderer video **before** Leave, not a
`CCutsceneDef`.

Do **not** start at `CS_OAKVALE_INTRO_FATHER` / `00DB86B0` /
`00CBFB7D` as if Leave entered the runner. That path is later
leftover `Q_NewOakValeIntro` (`00DABAC0` → TNG `NOVI_LiveFather`).
Its inner `PlayAVI dream_sequence_comp.xmv` is also **not** the
startup AVI.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `proofs/cutscene-first/README.md`,
`proofs/camera-after-leave/README.md`,
`proofs/newgame-script/README.md`,
`proofs/script-interpreter/README.md`,
`proofs/script-bank-open/README.md`,
`proofs/script-factory-tables/README.md`,
`docs/runtime/SCRIPT_FORMAT.md`,
`docs/runtime/FIXTURE_COMMAND_AUDIT.md`,
`docs/runtime/FORWARD_TREE.md` §§7–11,
`ScriptBank.cs` / `QuestFactoryTable.cs` / `RegionTravel.cs` /
`EngineLifecycle.cs`,
`EngineLifecycleTests.Activate_quests_00CB5AD0_starts_factory_scripts`,
`DataCatalogTests.Frontend_and_script_bins_are_gamebin`,
`WorldSceneTests` (`FirstSeenCallsUseCamera=false`,
father vector pins),
ExeIndex `script-runtime` (`00CBFB7D` exact, `00DB86B0`,
`calls-cutscene-runner-00cbfb7d`).

---

## Verdict

**Leave does not start a scripted cutscene.**

After `0042F2A2` the pump constructs quest factories
(`004B4260` / `00CB5AD0`). None of those rows is a
`CCutsceneDef`. Runner `00CBFB7D` is **not** on Leave /
Init Game / first no-save pumps.

| Question | Answer | Class |
|---|---|---|
| First `CCutsceneDef` *executed* after Leave? | **none** | **PROVEN** |
| First cutscene-*named* object after Leave? | empty quest factory `CS_PlayCutscene` `00F01760` | **PROVEN** not a def |
| First `script.bin` defs *registered* after Leave? | `00CB5D80` / `00F2A0F0` during Loading world | **PROVEN** register; **DISPROVEN** as run |
| Startup AVI as that first scripted cutscene? | **no** — before Leave, not `00CBFB7D` | **DISPROVEN** |
| First later leftover that *is* a scripted cutscene? | `CS_OAKVALE_INTRO_FATHER` → `00CBFB7D` | **PROVEN** leftover; **DISPROVEN** as Leave |
| First leftover opcode (if father later starts)? | `PlayMusic MUSIC_SET_NULL` | **PROVEN** leftover |
| Father-line `PlayAVI dream_sequence_comp.xmv` = startup AVI? | **no** | **DISPROVEN** |

---

## Timeline (no-save New Game)

```
0042EC7C retail
  006286F0 ×3 blocking PlayAVI          // STARTUP AVI — not CCutsceneDef
  0042E98F frontend.bin UI
  msg 15 → [retail+41]=1
0042F2A2 Leave frontend
  009BE420 clear + 009BEEB0 Present
  no 00CBFB7D / no StartCutscene
0042F491 Init Game → 004184BD
  Init World 004A6E30
    004A6550 Init Scripts 006E7740
    00CB5D80 Registering Scripts
      00F2A0F0 CScriptDef / CCutsceneDef / CRegionScriptDef   // BANK OPEN
      00CD52D0 quest factories (CS_PlayCutscene row is empty)
  00416953 Load FinalAlbion.wld
    00CD6E27 bind Q_NewOakValeIntro / S_QNOVI / 00DBEF70      // BIND ONLY
    START_INITIAL_QUESTS → world+172
  004B4260 Init Quests
    00CB5AD0 six WLD names + later Gameflow
    CS_PlayCutscene 00F01760 empty   // ScriptName==null
    00CB8690 START_SCRIPT_DATA parse // not 00CBFB7D
  user.ini ActivateQuest("Gameflow")
    00CE75B0 Main watcher; S_GF CCutsceneDef DISPROVEN
004189C2 first pumps
  00CB8220 → 00A44880
  00CE7670 state 0 yields on Q_NewOakValeIntro miss
  FirstSeenCallsUseCamera=false
  FirstSeenPlayAvi=false
```

`CS_ATTRACT_*` / `CS_OAKVALE_INTRO_FATHER` / `00DB86B0` /
`00CBFB7D` / `UseCamera CAM_OVIF_SHOT2` are **not** on this
list. **PROVEN.**

---

## 1. Startup AVI is not a scripted cutscene

| Claim | Class | Evidence |
|---|---|---|
| Retail plays three blocking `006286F0` slots before frontend | **PROVEN** | `proofs/audio-frontend`; `Retail_0042EC7C_after_AVI_clears_then_inits_frontend` |
| Those slots are `Fable Texture Renderer Filter`, not `00CBFB7D` | **PROVEN** | PARITY PlayAVI; no `CCutsceneDef` name on `0042EC7C` |
| PlayAVI opcode `00CCA26E` / prefix `Data\Video\` is that pump | **DISPROVEN** | Opcode lives inside the runner; retail pump calls `006286F0` directly |
| Startup AVI is after Leave | **DISPROVEN** | Leave is `0042F2A2` after the New Game click |
| Leave starts another PlayAVI | **DISPROVEN** | Leave is fade / clear / black Present |
| Father-cutscene `PlayAVI dream_sequence_comp.xmv` is the startup AVI | **DISPROVEN** | Lives in `CS_OAKVALE_INTRO_FATHER` def+60; `FirstSeenPlayAvi=false` on no-save |

**Answer:** ignore AVI for this walk. After Leave the screen is
black then Lookout 3D. The first *script* PlayAVI is a later
line inside the leftover father def.

---

## 2. What Leave *does* start (not `CCutsceneDef`)

`004B4260` → `00CB5AD0` walks WLD `START_INITIAL_QUESTS`
(host: six WLD names; QST TRUE also has `ChapterAndSceneManager`
/ `NPCDeath` / later `Global_WatchForHeroDeath` — see
`proofs/qst-first-load`). Then `user.ini` Gameflow.

| Order | Quest | Script | Factory | `CCutsceneDef` / `00CBFB7D`? |
|---:|---|---|---|---|
| 1 | `Q_SunnyvaleMaster` | none | `00CDD550` | **DISPROVEN** (`ChildCutscene==null`) |
| 2 | `PersonalScriptMain` | `S_PSM` | `00CDE2F0` | **DISPROVEN** (`HasStarted("S_PSM")==false`) |
| 3 | `PersonalScript_GlobalThings` | `S_PSGT` | `00CE19A0` | no |
| 4 | `HeroBoasts` | `S_HB` | `00CE6C40` | **DISPROVEN** (`HasStarted("S_HB")==false`) |
| 5 | `V_HeroDolls` | `S_VHDS` | `00E98640` | no |
| 6 | **`CS_PlayCutscene`** | **none** | **`00F01760` size 72 vtbl `012F72D0`** | **DISPROVEN** (`play.ScriptName==null`) |
| 7 | `Gameflow` | `S_GF` | `00CEF950` → `00CE75B0` | **DISPROVEN** (`HasStarted("S_GF")==false`) |

Do **not** `StartCutscene(S_PSM)` from a factory ctor. **PROVEN.**

`CS_PlayCutscene` vtbl+24 is generic fiber `00A44880` (same as
Gameflow / HeroBoasts). Base ctor is `00CB8110`. That tick is
**not** `00CBFB7D`. The name is a quest-table row, not
`script.bin` instance `CS_PlayCutscene`.

`00CB8690` `START_SCRIPT_DATA` is token parse on the factory
table, **not** the interpreter.

**Answer:** the only cutscene-*named* construct after Leave is
an empty factory. It does not load or walk a command vector.

---

## 3. `script.bin` is registered, not run

Compiled bank is GameBin `script.bin` (611 entries, mostly
`CCutsceneDef`). Persist is `00F2A1D0`: eight CString vectors
after a 5-byte preamble. Runtime command list is **def+60**
(vector 0). Runner copies it with `00432EE9`.

| Event | VA | Runs a def? | Class |
|---|---|---|---|
| Frontend opens `frontend.bin` | `0042E98F` | no | **PROVEN** |
| First script-bank open | `00416953` → `004A6550` → `00CB5D80` / `00F2A0F0` | register only | **PROVEN** |
| `CS_ATTRACT_1`…`CS_ATTRACT_12` on disk | `DataCatalogTests` | unused | **PROVEN** exist; **DISPROVEN** as Leave start |
| Attract first line (`CS_ATTRACT_12`) | `SetTime 14` then `NoLoadUseCamera …` | not executed | **PROVEN** layout; **DISPROVEN** as this walk |
| `S_QNOVI` in `script.bin` | — | — | **DISPROVEN** (`FirstSeenScriptBinHasSqnovi=false`) |
| `CS_OAKVALE_INTRO_FATHER` in `script.bin` | `FirstSeenScriptBinHasIntroCutscene=true` | not started here | **PROVEN** exist |

`00CBFB7D` callers exist (`calls-cutscene-runner-00cbfb7d`).
Address-order head includes later quests (`CS_STANDING_STONE`,
`CS_FABLE_CREDITS`, …) then leftover `00DB88F8` (`00DB86B0`)
pushing `CS_OAKVALE_INTRO_FATHER`. Frontend / Leave /
`0042EC7C` / `0042F2A2` / `004184BD` / `004B4260` /
`00F01760` / first `004189C2` have **no** `E8 00CBFB7D`.

**Answer:** after Leave the bank is live. No interpreter call
consumes it on the first pumps.

---

## 4. Leftover first *real* scripted cutscene (not this walk)

If / when `Q_NewOakValeIntro` is later constructed:

```
00DABAC0 registers NOVI_LiveFather (factory 00DAC2C0)
  TNG CREATURE_HERO_FATHER / NOVI_LiveFather
  004C97B0 → 00CB8960 → 00DB8520
  00DB8630 → [+52].vtbl+4 = 00DB86B0
00DB86B0 binds Hero / Father (00CD3D2E / 008ABD10)
  push 0,0,0 → 00CBFB7D("CS_OAKVALE_INTRO_FATHER")
    copies def+60 (not +108; [ebp+120]!=1)
    FadeOut 0.5,0 special-case at 00CBFDD0 MISSES
    first line PlayMusic MUSIC_SET_NULL   (00CC8EAC, jmp 00CD17FD)
    next FadeOut 0.5,0                    (00CD0987, vtbl+1488)
    CameraPause FALSE
    Hero.Teleport MK_OVI_ID_HERO,FALSE
    …
    UseCamera CAM_OVIF_SHOT2              (00CC9F3A)
    …
    PlayAVI dream_sequence_comp.xmv       (00CCA26E — NOT startup AVI)
    last Hero.SneakTo MK_OVIF_HERO5,0.0,TRUE
```

Father persist (host `ScriptBank` / `WorldSceneTests`):

| Vector | Offset | Contents | Auto-run after Leave? |
|---|---|---|---|
| 0 | def+60 | ≥60 command lines; `[0]=PlayMusic MUSIC_SET_NULL` | **DISPROVEN** |
| 1 | def+72 | 7 skip lines (`FadeOut`, `GamePause 0.5`, `UseCamera CAM_OVIF_SHOT7`, …) | **DISPROVEN** (`FirstSeenCutsceneVector1AutoRuns=false`; `00CBEB7E` false) |
| 2–7 | +108… | empty | — |

| Flag | Value | Class as *first after Leave* |
|---|---|---|
| `FirstSeenStartsIntroCutscene` | true (Oakvale leftover pairing) | **LEFTOVER** vs Leave |
| `FirstSeenFadeSpecialCaseRuns` | false (first line is PlayMusic) | **PROVEN** leftover |
| `FirstSeenCallsUseCamera` | false | **PROVEN** on no-save first Present |
| `FirstSeenPlayAvi` | false | **PROVEN** |
| Who activates `Q_NewOakValeIntro` on no-save | — | **UNREAD** (not Leave / not `004B4260` / not `00CE7670`) |

Host `ScriptRuntime.StartNewGame` / `StartCutscene(CS_OAKVALE_INTRO_FATHER)`
is **DIVERGE** vs Leave. Keep as Oakvale VM notes
(`FIXTURE_COMMAND_AUDIT.md` Finished def+60). Do not call from
Init Quests.

---

## 5. C# vs native

| Host | Native after Leave | Class |
|---|---|---|
| `EngineLifecycle.ActivateNamedQuest` six WLD + Gameflow | `004B4260` / `00CB5AD0` + `user.ini` | **PROVEN** |
| `CS_PlayCutscene` quest row, `ScriptName==null` | `00F01760` empty | **PROVEN** |
| `HasStarted(S_PSM/S_HB/S_GF)==false` | factories only | **PROVEN** |
| `ScriptInterpreter` / `StartCutscene` after msg 15 | unused | **LEFTOVER** vs Leave |
| `NewGameScript.CutsceneStarted` | leftover father | **LEFTOVER** |
| `ScriptRuntime.StartNewGame` + father def | invented Oakvale | **DIVERGE** |

---

## Classifications (short)

1. **First scripted cutscene after Leave — none. PROVEN.**
   No `00CBFB7D`, no `CCutsceneDef` command vector, no
   `UseCamera` / `PlayAVI` opcode.
2. **Startup AVI as that cutscene — DISPROVEN.**
   Three `006286F0` slots run **before** Leave and are not the
   runner.
3. **First cutscene-*named* object — empty `CS_PlayCutscene`
   `00F01760`. PROVEN.** Quest factory, not a def.
4. **`CS_ATTRACT_*` / `S_PSM` / `S_GF` as first runner —
   DISPROVEN.** Bank register only; `HasStarted` false.
5. **First later leftover scripted cutscene —
   `CS_OAKVALE_INTRO_FATHER`. PROVEN leftover.** First line
   `PlayMusic MUSIC_SET_NULL`. Inner PlayAVI is not startup
   AVI. Activator on no-save is **UNREAD**.
6. **Host `StartCutscene` from Leave / Init Quests — DIVERGE.**
   Keep the interpreter for leftover Oakvale, not New Game.
