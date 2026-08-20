# Live raid AVI + `AttackOver` after childhood objectives

Investigation only. No production `src/` or `tests/` edits.

Do **not** auto-complete the raid. Do **not** skip
cutscenes. Do **not** write `AttackOver=1` from deeds
skip / `ApplyPersist(true)` / `SkipAvi`. Do **not**
enter Guild take `00D3BC60`. Milestone ends **before**
that call.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `listing-00d80000.txt` `00DB97A0`–
`00DBB304` / `00DAADA0` / `00DAC420` / `00DBDE40`
`00DBE1F3`–`00DBE295`; `listing-00d00000.txt`
`00D3BC60`; `xrefs.tsv` (`CS_OAKVALE_INTRO_THERESA`
`00DBB21B`, `1_raid_on_oak_vale_comp.xmv` `00DBB249`);
`assembly/compiled-defs/script/0482` / `0483` / `0484`;
`src/Fable.Game/RegionTravel.cs` Theresa / Maze / raid
VAs; `ScriptFactoryTable.cs` (`PersistTable`);
`ScriptRuntime.cs`; `EngineLifecycle.cs`;
siblings `proofs/00DBB2A7-attackover-store`,
`proofs/raid-avi-live-path`, `proofs/00DBDE40-host-gap`,
`proofs/maze-pre-guild-stop`, `proofs/novi-factory-starts`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| After childhood objectives, what writes `AttackOver=1`? | Native `00DB97A0` tail: Theresa CS → raid AVI → `00DBB2A7` `mov [ecx+80],1`. | **PROVEN** |
| Is that first-seen? | **No.** First-seen is father CS + `00DBDE40` **read** of `+80`. Store is later. | **PROVEN** |
| Does the host run that tail live? | **No.** Constants MATCH. No Pump / `00DB97A0` analog. | **DISPROVEN** live |
| May we skip MEET / THERESA CS / raid AVI to poke persist? | **No.** Skip vectors do not auto-run. Raid AVI is **after** `00CBFB7D`. | **DISPROVEN** |
| Is `CS_BANDITRAID_*` this AVI? | **No.** Adult raid family. | **DISPROVEN** |
| Does this milestone enter `00D3BC60`? | **No.** `MilestoneEntersGuildTake=false`. Maze CS still Oakvale. | **PROVEN** stop |

---

## Verdict

**Native later path PROVEN. Host data MATCH. Live run DISPROVEN.**

`NOVI_Theresa` `vtbl+4` is `00DB97A0`. First named work
is `M_TriggerOutro`. After childhood deeds the same
function plays `CS_OAKVALE_INTRO_THERESA_MEET` /
`_MEET_YES`, then `CS_OAKVALE_INTRO_THERESA`, then
**native** `vtbl+1476` `Data\Video\1_raid_on_oak_vale_comp.xmv`,
then `00DBB2A7` stores `[quest+80]=1`. That wakes
`00DBDE40`’s `HerosOldHouse` spin. PostAttack + Maze
CS follow. Guild take does **not**.

First-seen persist stays **false**. Host
`RegionTravel` already names every VA. Nothing in
`EngineLifecycle.Pump` executes them.

---

## Evidence → Original → Host → Gap

| Evidence | Original | Host | Gap |
|---|---|---|---|
| `listing-00d80000.txt` `00DAADA0` `push "AttackOver"` / `add ecx,80` / `004045C0` | persist **bind**, stack default 0 | `PersistTable.AttackOverWrite=00DAADA0`; `NewGameScript.PersistAttackOverName` | **MATCH** bind. Not the `+80=1` write. |
| `00DAADE1` `mov [esi+80], bl` inside `00DAADD0` | ctor/reset **zero** | `IntroQuestReset=00DAADD0` | **MATCH** reset. First-seen value 0. |
| `00DAC158` `test [esi+80]` in `00DABAC0` | **reader**; if 1, PostAttack teardown | no host analog | **LEFTOVER** later-load branch. Not first-seen. |
| `00DBE1F3` / spin `00DBE200`–`00DBE21C` `mov al,[esi+80]` | StartOakVale **read**; no `mov 1` in `00DBDE00–00DBF000` | `FirstSeenPlus80WrittenInStartOakVale=false`; `PreAttackGateOffset=80` | **MATCH** first-seen wait. Writer is later. |
| `00DAC420` factory: `[esi]=0x12D83A4`, `[esi+20]=ebx` (quest), release `00CDEE00` | construct only | `TheresaFactory=00DAC420`; `TheresaVtbl=012D83A4` | **MATCH** data. Factory is not start. |
| rdata `012D83A8=00DB97A0` (vtbl+4) | Theresa **start** | `TheresaStart` / `TheresaMeetStart=00DB97A0` | **MATCH** VA. Host never calls it. |
| `00DB97A0` `sub esp,0x1F0`; `00DB9812` `"M_TriggerOutro"`; `00CBE2FF` r=5.0 (`0x40A00000`) | wait at outro marker | comment “first work M_TriggerOutro” | **MATCH** note. No live wait. |
| `00DB98F5` / `00DBAE20` `"SKIP"` → `007E73E0` then `00CBE2FF` | Theresa **action** name, not CS skip | none | **DISPROVEN** as cutscene-skip. |
| `00DB9B01` `"CS_OAKVALE_INTRO_THERESA_MEET"`; `00DB9B28` `00CBFB7D` | first `00CBFB7D` in this start | `TheresaMeetCutscene`; `TheresaMeetSite=00DB9B02` | **MATCH** name/site. `ConstructStartsCutscene=false` so `StartNamedScript` **no-ops**. |
| `00DB9B3B` chocolate; yes-path `00DB9D5A` `"CS_OAKVALE_INTRO_THERESA_MEET_YES"`; `00DB9D7A` `00CBFB7D`; `00DB9DE6` `OBJECTIVE_05`; `00DB9E92` `[ebp+29]=1` | childhood give + objective 05; later re-entry skips first MEET | `TheresaMeetYesCutscene`; `TheresaMeetYesSite=00DB9D5B` | **MATCH** names. No chocolate / `+29` analog. |
| `00DBA3E8` r=2.0 (`0x40000000`) `00CBE2FF`; `jne 00DBB0E4` | deeds-done radius → raid tail | none | **UNREAD** as host wait. Native fall-in is **PROVEN**. |
| `00DBB21A` `"CS_OAKVALE_INTRO_THERESA"`; `00DBB238` `00CBFB7D` (push `-1,1,0,0,0`) | raid **cutscene**; blocking runner | `TheresaCutscene`; `TheresaRaidAviSite=00DBB21B` | **MATCH** string/VA. Host bind would start this CS at construct — **DISPROVEN** first `00CBFB7D` (that is MEET). |
| compiled-def `0484` vector 0: **no** `PlayAVI` | raid file is **not** a script opcode | `PlayAviOpcode=00CCA26D` generic | **PROVEN** native-after-CS. Opcode MATCHES father dream AVI, **not** this site. |
| `00DBB248` `"Data\Video\1_raid_on_oak_vale_comp.xmv"`; `00DBB260` `call [edx+1476]` | **the** raid PlayAVI; blocking `0088F890` / `006286F0` | `RaidPlayAvi` / `RaidPlayAviRewritten`; `TheresaRaidPlayAviSite=00DBB249`; `PlayAviVtbl=1476`; rewrite `.xmv`→`.wmv` | **MATCH** file + vtbl. **This call site** is not wired. `IScriptHost.PlayAvi` is the **opcode** path. |
| `00DBB28D` `vtbl+1492` black `(0,0,0,255)` 0.5s; `00DBB29E` `vtbl+2784(25)` | fade overlay + music after AVI | generic fade/PlayMusic | **PARTIAL** opcodes exist; not bound to this tail. |
| `00DBB2A4` `mov ecx,[ebp+20]`; `00DBB2A7` `mov [ecx+80],1` | **the** `AttackOver` store on parent quest | `AttackOverStore=00DBB2A7`; `AttackOverOffset=80`; `AttackOverWriterKnown=true` | **MATCH** VA. `PersistStore` header still “Writer UNREAD” = **LEFTOVER**. Live `SetBool` only from tests. |
| `00DBE22F` `00DBE3C0` then `00DBE236` `00DBEB20` after spin exits | PostAttack env + Maze CS **after** store | `PostAttackQuest` / `PostAttackEnvironment`; `MazeCutscene`; `MazeCutsceneStart=00DBEB20` | **MATCH** names/VA. Must not run **before** `00DBB2A7`. |
| `00DBEB20` → `00DBEE7A` `00CBFB7D("CS_OAKVALEINTRO_HESDEADJIM")`; last line `PlayMusic MUSIC_SET_NULL,FALSE` `00CC8EAC` | childhood end, still Oakvale | `MazeCutsceneLastCommand`; `MazeCutsceneStop=00CC8EAC` | **MATCH**. Stop here. |
| `00DBE295` `vtbl+1152` Give | Give **after** AttackOver, still Oakvale | `QuestGiveAfterAttackOver=00DBE295` | **MATCH** VA. Not construct. |
| `00D3BC60` `"GuildArrivalHSP"` / `"LookoutPoint"` | Guild **take** | `GuildTakeFn=00D3BC60`; `MilestoneEntersGuildTake=false` | **MATCH** stop. **OUT** of this milestone. |
| no-save Leave never `00CB5AD0("Q_NewOakValeIntro")` | quest that owns `+80` never constructed | `No_save_does_not_activate_Q_NewOakValeIntro` | **DISPROVEN** live activate. Activator still **UNREAD**. |
| `EngineLifecycle` grep: no `AttackOverStore` / `RaidPlayAvi` / `TheresaMeetStart` / `00DB97A0` | Pump does not Note-execute this tail | — | **PROVEN** omit. Keep it. |
| `ScriptRuntime.Update` “Does not write persist fields” | fiber tick ≠ store | same comment | **MATCH**. |
| `PumpUntilSettled` `SkipAvi` on `BlockPump` | fixture skip of **opcode** PlayAVI | first-seen `dream_sequence_comp.xmv` tests | **LEFTOVER** vs raid. Must **not** skip live AVI. |
| `NewGameScript.ApplyPersist(true)` | C# poke of the slot | tests only | **DISPROVEN** as writer. |
| `CS_BANDITRAID_*` fixtures | adult raid CS | `ScriptRuntimeArchitectureTests` | **DISPROVEN** as this AVI. |

---

## First-seen vs later

Same quest object (`S_QNOVI` / `Q_NewOakValeIntro`).
Different time. Do not collapse.

### First-seen (AttackOver still 0)

```
00CD6E27     bind Q_NewOakValeIntro / S_QNOVI / 00DBEF70
00DAADA0     004045C0("AttackOver", this+80)     // bind, 0
00DABAC0     00CB8230 NOVI_* (Theresa factory 00DAC420)
             E8 00DBDE40
00DB86B0     CS_OAKVALE_INTRO_FATHER via NOVI_LiveFather
             PlayAVI dream_sequence_comp.xmv     // OPCODE 00CCA26D
00DBDE40     PreAttack, 12s, HerosOldHouse
             spin READ [this+80]                 // 00DBE1F3
```

Childhood **objectives 01–05** run on other `NOVI_*`
starts / watchers **while** that spin holds:

| # | Text | Owner | When |
|--:|---|---|---|
| 01 | `TEXT_QUEST_OAKVALE_INTRO_OBJECTIVE_01` | `00DABAC0` / father | first-seen after names |
| 02 | `…_02` | wander `00DB0660` | deeds |
| 03 | `…_03` | `WatchForGotGold` `00DBE2E0` | deeds |
| 04 | `…_04` | `00DB4095` | deeds |
| 05 | `…_05` | **`00DB97A0`** after MEET_YES | Theresa chocolate — **last childhood** |

Host leftover `StartNewGame` / `ConstructStartsCutscene=true`
starts **only** father CS. Theresa is `false` → no-op.
First-seen AVI is `IntroPlayAvi` (`dream_sequence_comp.xmv`).
**DISPROVEN** as the raid file.

`FirstSeenCutsceneSkipFires=false`. Vector 1 does not
auto-run on father. Same `00CBEB7E` reader for later CS
— **PARTIAL** as a live skip on MEET/THERESA, **DISPROVEN**
as something the host should fire.

### Later (after objective 05 / radius 2.0)

```
00DB97A0     already running (vtbl+4)
00DBA3E3     WaitForUnderRadius 2.0
00DBA400     jne 00DBB0E4                    // deeds done
00DBB0E4     clear quest extras; HERO / Theresa
00DBB218     CS_OAKVALE_INTRO_THERESA
00DBB238     call 00CBFB7D                   // MUST PLAY
00DBB248     Data\Video\1_raid_on_oak_vale_comp.xmv
00DBB260     call [edx+1476]                 // MUST PLAY
00DBB28D     vtbl+1492 fade
00DBB2A7     mov [ecx+80], 1                 // AttackOver STORE
             ret 00DBB304
00DBDE40     spin exits
00DBE3C0     Q__OakValeIntro_PostAttack / ENVIRONMENT_OV_POSTATTACK
00DBEB20     CS_OAKVALEINTRO_HESDEADJIM
00CC8EAC     PlayMusic MUSIC_SET_NULL,FALSE  // last in-milestone
             // STOP — do not 00D3BC60
```

`[ebp+29]` at `00DB9E92` is “already met”. Re-entry
skips first MEET, **not** the raid CS/AVI. **PROVEN**.

---

## Do not skip cutscenes

| Temptation | Why it is wrong | Class |
|---|---|---|
| `ApplyPersist("AttackOver", true)` / `Gate80=true` | skips MEET, THERESA CS, raid AVI | **DISPROVEN** writer |
| `PumpUntilSettled` `SkipAvi` | fixture analog of DIK 1/57/28/62 on **opcode** PlayAVI; raid AVI is native **after** CS | **LEFTOVER** vs this site |
| Theresa CS vector 1 (`FadeOut` / `GamePause 1.0` / `RemoveExtras` / `Teleport`) | does not auto-run; even if it did, `00CBFB7D` **still returns** to `00DBB248` PlayAVI | **DISPROVEN** as AVI skip |
| MEET vector 1 (`UseCamera CAM_TM_SIS` only) | does not skip MEET body or later raid | **DISPROVEN** as raid skip |
| Native `"SKIP"` at `00DB98F5` / `00DBAE20` | `007E73E0` action + `00CBE2FF` radius wait | **DISPROVEN** as CS skip |
| Start `CS_OAKVALE_INTRO_THERESA` at TNG construct | first `00CBFB7D` is MEET at `00DB9B28` | **DISPROVEN** |
| Start `CS_BANDITRAID_*` | adult raid | **DISPROVEN** |
| Start Maze / PostAttack / Give `00DBE295` **before** `00DBB2A7` | those run **after** the store | **DISPROVEN** order |
| Jump to `00D3BC60` / `GuildArrivalHSP` / `CS_GUILD_ARRIVE` | Guild take. `MilestoneEntersGuildTake=false` | **DISPROVEN** |

Raid AVI is **not** inside `0484`. Skipping the CS
interpreter does **not** skip `vtbl+1476` at `00DBB260`.

---

## Host RegionTravel / factory VAs (data MATCH)

| Host | Value | Native |
|---|---|---|
| `TheresaMeetStart` / `TheresaStart` | `00DB97A0` | vtbl+4 |
| `TheresaMeetCutscene` | `CS_OAKVALE_INTRO_THERESA_MEET` | `00DB9B02` |
| `TheresaMeetSite` | `00DB9B02` | push |
| `TheresaMeetYesCutscene` | `CS_OAKVALE_INTRO_THERESA_MEET_YES` | `00DB9D5B` |
| `TheresaMeetYesSite` | `00DB9D5B` | push |
| `TheresaCutscene` | `CS_OAKVALE_INTRO_THERESA` | `00DBB21B` |
| `TheresaRaidAviSite` | `00DBB21B` | CS string (xref) |
| `TheresaRaidPlayAviSite` | `00DBB249` | `.xmv` string |
| `RaidPlayAvi` | `1_raid_on_oak_vale_comp.xmv` | `00DBB248` |
| `RaidPlayAviRewritten` | `1_raid_on_oak_vale_comp.wmv` | `0099C1E0` generic |
| `AttackOverStore` | `00DBB2A7` | `mov [ecx+80],1` |
| `MazeCutscene` | `CS_OAKVALEINTRO_HESDEADJIM` | `00DBEE5C` |
| `MazeCutsceneStart` | `00DBEB20` | after store |
| `MazeCutsceneStop` | `00CC8EAC` | last Maze opcode |
| `GuildTakeFn` | `00D3BC60` | **OUT** |
| `MilestoneEntersGuildTake` | `false` | **MATCH** stop |
| `IntroPlayAvi` | `dream_sequence_comp.xmv` | **first-seen** father opcode |

`EngineLifecycle` does not reference any row in that
table except via unrelated PlayAVI **startup** VAs
(`006286F0` on `StartupVideos`). Those are not the raid.

---

## Live-path gap (to **reach** `00DBB2A7`)

Ordered. Stop at the first unproven item.
Do **not** satisfy these by writing persist or skipping CS.

1. Proven activator of `Q_NewOakValeIntro`
   (`004B4A10` / `00CB5AD0`). Not no-save Leave.
   **UNREAD** / blocked-on-activator.
2. `QuestFactoryTable` row + construct `00DBEF70` /
   `00DAAC00` / persist bind **0**.
3. `00DABAC0` name table including `NOVI_Theresa`
   `00DAC420` **before** `00DBDE40`.
4. `00DBDE40` map-ready, child hero, watchers, 12 s,
   `HerosOldHouse`, **spin on `+80`**. See
   `00DBDE40-host-gap`.
5. Childhood deeds through **objective 05** (chocolate
   / MEET / MEET_YES). Completing them is what falls
   into `00DBB0E4`.
6. Live `NOVI_Theresa` object running `00DB97A0`
   (`[thing+20]` = parent quest). Host start body is
   constants only.
7. `00CBFB7D("CS_OAKVALE_INTRO_THERESA")` at `00DBB238`.
   Play it. Do not skip.
8. Native PlayAVI at `00DBB260`. Play it. Do not
   `SkipAvi`. Generic opcode rewrite MATCHES; this
   site is not the opcode.
9. **Then** `00DBB2A7`.

After the store (still this milestone, **not** the
gap to the write):

10. `00DBDE40` spin exits → `00DBE3C0` → `00DBEB20`.
11. Maze last opcode `00CC8EAC`. Still Oakvale.
12. **Stop.** Do not `00D3BC60`.

---

## Fixture vs live

| Path | AttackOver / raid AVI | Class |
|---|---|---|
| `EngineLifecycle` no-save Pump | never installs the slot; never stores 1; never plays raid file | **PROVEN** omit |
| `ScriptRuntime.StartNewGame` | father CS + dream AVI opcode; Theresa no-op; persist false | **LEFTOVER** vs Pump |
| `PumpUntilSettled` / `SkipAvi` | skips **opcode** AVI in fixtures | **LEFTOVER**; forbidden on live raid |
| `ApplyPersist(true)` | C# skip of deeds+CS+AVI | **DISPROVEN** writer |
| `ScriptRuntimeParityTests` | asserts VAs + names; persist still false | **MATCH** constants |

---

## Do not

- Call `ApplyPersist("AttackOver", true)` because
  childhood deeds / CS / AVI are unimplemented.
- Treat `AttackOverWriterKnown=true` as “host reached
  the store”.
- Skip `CS_OAKVALE_INTRO_THERESA_MEET*`,
  `CS_OAKVALE_INTRO_THERESA`, or the raid AVI.
- Start `CS_BANDITRAID_*` as this AVI.
- Start Maze / PostAttack / `00DBE295` Give **before**
  `00DBB2A7`.
- Walk `S_QGT` / `00D3BC60` / `GuildArrivalHSP` /
  `CS_GUILD_ARRIVE`.
- Invent `ActivateQuest("Q_NewOakValeIntro")` on
  no-save Leave to “reach” the store.
- Grow `Pump` / `PumpScripts` to Note-execute
  `00DB97A0`.
- Confuse first-seen `dream_sequence_comp.xmv` with
  `1_raid_on_oak_vale_comp.xmv`.
