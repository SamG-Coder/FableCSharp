# Script command map vs native table vs Leave / Init Game

Investigation only. No production `src` edits.

Do **not** treat `CS_OAKVALE_INTRO_FATHER` / `00DB86B0` / `00CBFB7D`
as first after Leave frontend. That path is later leftover
`Q_NewOakValeIntro` (`00DABAC0` → `00DBDE40`), not no-save Init Game.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **LEFTOVER** / **INVENTED**.

Sources:

- `src/Fable.Game/ScriptCommandMap.cs` (`NativeTokens`, `All`)
- `docs/runtime/COMMAND_MAP.md` (curated short table)
- `docs/runtime/COMMAND_MAP.generated.md` / `COMMAND_COVERAGE.md`
- dump `tools/Fable.ExeIndex/out/01-sections/script-bank/exe-commands.md`
  (ASCII `0x012C1500`–`0x012C2C00`)
- dump `tools/Fable.ExeIndex/out/01-sections/script-bank/native-sqnovi.md`
- dump `tools/Fable.ExeIndex/out/01-sections/script-runtime/`
- `docs/runtime/FORWARD_TREE.md`; `proofs/camera-after-leave/README.md`
- `EngineLifecycleTests` (`Init_quests_004B4260_activates_wld_initial_list`,
  `No_save_does_not_activate_Q_NewOakValeIntro`,
  `Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`)

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend
0042F491 Init Game → 00418DCA → 004184BD
  Init World 004A6E30
  00416953 Load world FinalAlbion.wld
  004A1840 Load Quests
  004B4260 START_INITIAL_QUESTS (world+172 from 00507C30)
    Q_SunnyvaleMaster
    PersonalScriptMain / S_PSM     factory only; HasStarted(S_PSM)==false
    PersonalScript_GlobalThings
    HeroBoasts
    V_HeroDolls
    CS_PlayCutscene 00F01760       empty; ScriptName==null; no CCutsceneDef
    Gameflow / S_GF 00CE75B0       Main watcher; HasStarted(S_GF)==false
004189C2 first pumps
  type-1 00CB8220 Gameflow state 0 yields on inactive Q_NewOakValeIntro
```

`S_QNOVI` / `Q_NewOakValeIntro` / `00DBDE40` / `00DB86B0` /
`CS_OAKVALE_INTRO_FATHER` / `00CBFB7D` are **not** on this list.
**PROVEN.**

---

## 1. Native command table (exe dump)

Authority is ASCII in `0x012C1500`–`0x012C2C00`
(`script-bank/exe-commands.md`). That slice is **not** verbs-only.
It mixes dispatcher tokens, modes (`false` / `forever` / `MELEE`),
prefixes (`Data\Video\`), collection names (`LadyGreyIntro`),
named-args (`return` / `limbo`), persist keys
(`JackBossBattleResult` …), and quest-log fragments.

`ScriptCommandMap.NativeTokens` is a **filter** of that slice:
185 entries, 124 Global + 61 Entity (5 names appear twice:
`FadeOut` / `FadeIn` / `SlideTeleport` / `LookAtNothing` / `Collide`).
Unique verb names: **180**.

The filter is **PROVEN** as a listing of strcmp tokens the
`00CBFB7D` runner uses. It is **not** a first-seen-after-Leave list.

Coverage vs `All` (`COMMAND_COVERAGE.md`):

| Metric | Count |
|---|---|
| Native token rows | 185 |
| Recovered dispatch / return | 156 |
| Recovered apply | 95 |
| Implemented runtime | 13 |
| UNREAD native tokens | 29 |

### Native tokens with no `All` row (UNREAD — not invented)

These exist in the exe table. The map correctly leaves them unread
(unknown verb → block, not no-op):

`GameInfo` `Fullscreen` `Print` `ExitGame`
`CrowdKill` `CrowdLookAt` `CrowdLookTo` `CrowdCollide`
`CrowdCombatAnimate` `CrowdMove` `CrowdTeleport`
`CrowdTeleportRipple` `CrowdRipplePosition`
`CreditScreen` `SmashWindows` `RegisterScript` `TeleportToHSP`
`StopProgressSpinner` `StartProgressSpinner` `DoCharacterPreload`
`WaitBossFight` `DebugCamera` `LadyGreyIntro`
`TurnInto` `Decapitate` `FadeCross` `FightStop`
`DoBossFight` `SummonerAttack`

`LadyGreyIntro` is listed as a Global token because it lives at
`0x012C20E0` in the slice. Apply of `RemoveAllThings` uses it as a
`vtbl+300` collection name. Treating it as a standalone verb is
**PARTIAL** (string proven; opcode unproven).

`return` at `0x012C2090` is a **named-arg** of `RemoveExtras`
(`00BFEBA8` vs arg1), not an interpreter stop. Map evidence
**DISPROVES** “return = halt”.

---

## 2. `All` vs native table — invented names

`All` has **154** specs. Three names are **not** exe tokens:

| `All` name | Native? | Class | Evidence |
|---|---|---|---|
| `PlayCombatAnimation` | **no** | **LEFTOVER alias** | script.bin `Father.PlayCombatAnimation TURNING_AC90`. Token match is `.PlayCombatAnim` `0x012C2540` / `00CC15E3`. Same apply. |
| `RemoveThing` | **no** | **LEFTOVER alias** | `00BFEAF8(verb, "Remove", 6)` prefix. Apply is `00CD0116` / `vtbl+432`. Not a separate dispatcher. |
| `Get` | **no** | **INVENTED as exe token** | script.bin `Get NAME,ALIAS` bind. TokenSite=`0` ApplySite=`0`. Not in `0x012C1500`–`0x012C2C00`. |

Coverage labels those three `script.bin`. Tests already assert
`NativeTokens` contains names with no `Find` **and**
`Find("RemoveThing").TokenSite == 0x00CD0116`.

No other invented verb names in `All`. The rest are native tokens
(or the two aliases above).

### Native names with TokenSite=`0` (name proven, handler site unread)

These are **not** invented verbs. Apply is ScriptLayer stub:

`StopMusic` `StayFadedOut` `EnableSounds` `NoDialogCam`
`CrowdClearActions` `ClearCommands` `AddScriptedMode`
`RemoveScriptedMode` `EntitySetMaxWalkingSpeed`
`EntitySetMaxRunningSpeed` `Drawable` `Collide` `SetAlpha`
`LookAt` `LookAtNothing`

Claiming a complete `00CBFB7D` site for any of these is **INVENTED**.

---

## 3. First-seen after Leave / Init Game

**Zero** `00CBFB7D` command verbs run on this path. **PROVEN.**

| Claim | Class | Evidence |
|---|---|---|
| `004B4260` activates WLD `START_INITIAL_QUESTS` (7 names) | **PROVEN** | `Init_quests_004B4260_*`; recover-004B4260 |
| List is Sunnyvale / Personal* / HeroBoasts / HeroDolls / `CS_PlayCutscene` + Gameflow | **PROVEN** | same; `ActivatedQuests.Count==7` |
| `CS_PlayCutscene` is empty factory `00F01760` / vtbl `012F72D0` | **PROVEN** | `play.ScriptName==null`; no `CCutsceneDef` |
| `00CBFB7D` on Leave / Init Game / first pumps | **DISPROVEN** | no E8; `FirstSeenCallsUseCamera=false`; `FirstSeenCallsPlayAnimationDispatcher=false` |
| `Q_NewOakValeIntro` / `S_QNOVI` activated no-save | **DISPROVEN** | `No_save_does_not_activate_Q_NewOakValeIntro`; not in WLD `+172` |
| Gameflow `S_GF` CCutsceneDef as first runner | **DISPROVEN** | `HasStarted("S_GF")==false`; state 0 yields on inactive intro quest |
| Any `COMMAND_MAP.md` verb as first Present / first 3D | **DISPROVEN** | Lookout follow-helper, not SHOT2 |

Native work after Leave is factory/fiber (`00A447D0` / `00A44880`),
WLD/TNG load, and Gameflow **C++** Main (`00CE75B0`). Those are not
command-map verbs.

Do not invent a first-seen command list by grepping `script.bin`.

---

## 4. Leftover first *interpreter* use (not Leave)

When `Q_NewOakValeIntro` later runs, `00DABAC0` registers
`NOVI_LiveFather` → `00DAC2C0` → fiber `00DB8630` → `00DB86B0`
pushes `CS_OAKVALE_INTRO_FATHER` into `00CBFB7D`.

Dump: `script-bank/0481-cs-oakvale-intro-father.md` +
`script-bank/native-sqnovi.md` + `script-runtime/` token files.

That is the **first leftover interpreter** line list, not first
after Leave:

| Order | Verb | Token in map | Native token | Class |
|---|---|---|---|---|
| 1 | `PlayMusic` | `00CC8EAC` | `0x012C1904` | leftover interpreter |
| 2 | `FadeOut` | `00CD0987` | `0x012C19A0` | leftover |
| 3 | `CameraPause` | `00CC71F1` | `0x012C2058` | leftover |
| 4 | `.Teleport` | `00CC4678` | `0x012C22D4` | leftover |
| 5 | `.LookToThing` | `00CC3B3F` | `0x012C2390` | leftover |
| 6 | `DoScriptFrame` | `00CC7085` | `0x012C2080` | leftover |
| 7 | `DoCameraPreloading` | `00CC86D0` | `0x012C18C8` | leftover |
| 8 | `PlayAVI` | `00CCA26D` | `0x012C1DE8` | leftover |
| 9 | `MuteSounds` | `00CC7258` | `0x012C204C` | leftover |
| 10 | `.PlayAnimation` | `00CC14B8` | `0x012C2550` | leftover |
| 11 | `StartTimeCode` | `00CD1373` | `0x012C18B8` | leftover |
| 12 | `NoLoadUseCamera` | `00CC9E6A` | `0x012C1DF8` | leftover |
| 13 | `FadeIn` | `00CC4B22` | `0x012C19A8` / `.FadeIn` | leftover |
| 14 | `GamePause` | `00CC88D1` | `0x012C1EF8` | leftover |
| 15 | `.Speak` | `00CC25FD` | `0x012C2498` | leftover |
| 16 | `.InteractiveSpeak` | `00CC2EAA` | `0x012C2438` | leftover |
| 17 | `UseCamera` | `00CC9F3A` | `0x012C18AC` | leftover |
| 18 | `.DialogSpeak` | `00CC3165` | `0x012C2428` | leftover |
| 19 | `.WaitTask` | `00CC0783` | `0x012C25F4` | leftover |
| 20 | `.SneakTo` | `00CC0CB5` | `0x012C25D8` | leftover |
| 21 | `PlayCombatAnimation` | alias → `00CC15E3` | **not a token** | leftover script.bin spelling |
| 22 | `Create` | `00CCC246` | `0x012C1D14` | leftover |
| 23 | `.WalkTo` | `00CC083D` | `0x012C25EC` | leftover |
| 24 | `WaitActiveDialog` | `00CC656B` | `0x012C2110` | leftover |
| 25 | `Remove` | `00CD0116` | `0x012C1A10` | leftover |
| 26 | `.DialogadSpeak` | `00CC3354` | `0x012C2418` | leftover |
| 27 | `.LookInDirection` | `00CC3F73` | `0x012C234C` | leftover |

`native-sqnovi.md` walks the same first slice through PlayMusic →
FadeOut → Teleport → LookToThing → DoScriptFrame →
DoCameraPreloading → PlayAVI → MuteSounds → NoLoadUseCamera.
Token VAs match `script-runtime/*-token-*.md`. **PROVEN** as
Oakvale leftover.

`COMMAND_MAP.md` is that leftover list plus `SetTime` (native
`0x012C19C0` / `00CD07D6`, **not** on the father def) and the
`PlayCombatAnim` / `RemoveThing` aliases. It is **not** generated
from `All` (the generated table is `COMMAND_MAP.generated.md`).

---

## 5. `COMMAND_MAP.md` vs dump vs Leave

| Row in `COMMAND_MAP.md` | In exe table | First after Leave | Class |
|---|---|---|---|
| All father verbs above | yes (except `PlayCombatAnimation`) | **no** | leftover interpreter |
| `PlayCombatAnim` | yes `.PlayCombatAnim` | no | native alias of leftover line |
| `RemoveThing` | no | no | leftover alias |
| `SetTime` | yes | no | native; **not** first-seen; not on father def |

Calling any of those “first-seen after Leave / Init Game” is
**INVENTED**.

---

## Classifications (short)

1. **Native table** = exe ASCII `0x012C1500`–`0x012C2C00`.
   `NativeTokens` 185 / 180 unique is a verb filter. **PROVEN.**
2. **First-seen after Leave / Init Game: no `00CBFB7D` verbs.**
   Empty `CS_PlayCutscene` factory + Gameflow yield. **PROVEN.**
3. **Invented `All` names:** `Get` (no exe token).
   **Leftover aliases:** `PlayCombatAnimation`, `RemoveThing`.
4. **Leftover first interpreter** is `CS_OAKVALE_INTRO_FATHER`
   (table in §4). `COMMAND_MAP.md` is that leftover plus `SetTime`.
5. **29 UNREAD native tokens** are real strings, not invented.
   Unknown verbs must stay UNREAD / block.

Do not start New Game at `00DB86B0`. Do not fill first-seen from
the command map.
