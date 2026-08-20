# Raid AVI live path vs `00DBB2A7` (`AttackOver=1`)

Investigation only. No production `src/` or `tests/` edits.

Do **not** invent `AttackOver=1` from a childhood-deeds skip.
Do **not** `ActivateQuest("Q_NewOakValeIntro")` from no-save Leave.
Do **not** follow `S_QGT` / `00D3BC60` / `GuildArrivalHSP`.
`CS_BANDITRAID_*` is the **adult** raid, not this AVI.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: `listing-00d80000.txt` `00DB97A0` / `00DBB218`–`00DBB2A7`
/ `00DBE3C0` / `00DBEB20`; `xrefs.tsv` (`CS_OAKVALE_INTRO_THERESA`
`00DBB21B`, `1_raid_on_oak_vale_comp.xmv` `00DBB249`);
`src/Fable.Game/RegionTravel.cs`, `ScriptFactoryTable.cs`
(`PersistTable`), `ScriptRuntime.cs`, `QuestFactoryTable.cs`,
`EngineLifecycle.cs`; sibling `proofs/00DBB2A7-attackover-store`,
`proofs/00DBDE40-host-gap`,
`EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Do `RegionTravel` / `PersistTable` / `ScriptRuntime` **execute** `00DBB2A7` on a live quest? | **No.** They hold the VA / names. | **DIVERGE** live |
| Are the native names / VAs already on the host? | **Yes**, constants + comments. | **MATCH** data |
| Is `AttackOver=1` written from `StartNewGame` / deeds skip / `ApplyPersist(true)`? | **Must not.** Tests that call `ApplyPersist(true)` are fixtures. | **DISPROVEN** as the writer |
| Does no-save Leave construct the quest that owns `+80`? | **No.** `Q_NewOakValeIntro` never activates. | **DISPROVEN** |
| Is `00DAADA0` the `+80=1` write? | **No.** Bind only (`004045C0("AttackOver", this+80)`). | **DISPROVEN** |
| Is the store inside `00DBDE40` `HerosOldHouse` spin? | **No.** Spin **reads** `+80`. Write is later `00DB97A0`. | **DISPROVEN** |
| Native writer after childhood deeds? | `00DB97A0` → Theresa CS → raid AVI → `00DBB2A7`. | **PROVEN** |
| `CS_BANDITRAID_*` this AVI? | **No.** Adult raid cutscenes. | **DISPROVEN** |

---

## Verdict

**Constants MATCH. Live path DIVERGE.**

`PersistTable.AttackOverStore` and `RegionTravel.AttackOverStore`
are both `00DBB2A7`. That is a recovered **VA**, not a host
instruction. No `EngineLifecycle.Pump` / `ScriptRuntime.Update` /
`ActivateQuest` arm reaches `00DB97A0` `mov [ecx+80], 1`.

First-seen persist value stays **false**. Do **not** set it true
to skip deeds.

---

## Native path (only after `S_QNOVI` is constructed)

```
00CD6E27  bind Q_NewOakValeIntro / S_QNOVI / 00DBEF70     // BIND
????      004B4A10 / 00CB5AD0("Q_NewOakValeIntro")       // ACTIVATOR UNREAD
          00DBEF70 alloc 0x10C ctor 00DAAC00 vtbl 012D7A28
00DAADA0  004045C0("AttackOver", this+80)                // bind, value 0
00DABAC0  00CB8230 NOVI_* ; factory NOVI_Theresa 00DAC420
          vtbl 012D83A4 ; [thing+20] = quest
          E8 00DBDE40                                    // StartOakVale
00DBDE40  PreAttack, 12s, HerosOldHouse, spin [this+80]  // READ, no mov 1

          // childhood deeds (chocolate / Theresa meet) — not skippable
00DB97A0  M_TriggerOutro / CS_OAKVALE_INTRO_THERESA_MEET* / deeds
00DBB218  push "CS_OAKVALE_INTRO_THERESA"
00DBB238  call 00CBFB7D                                  // not CS_BANDITRAID_*
00DBB248  push "Data\Video\1_raid_on_oak_vale_comp.xmv"
00DBB260  call [edx+1476]                                // PlayAVI
00DBB28D  call [edx+1492]                                // fade overlay
00DBB2A4  mov ecx, [ebp+20]                              // parent quest
00DBB2A7  mov [ecx+80], 1                                // AttackOver STORE

          // AFTER the store — not required to reach it
00DBE3C0  Q__OakValeIntro_PostAttack
          ENVIRONMENT_OV_POSTATTACK
00DBEB20  CS_OAKVALEINTRO_HESDEADJIM (Maze) 00CBFB7D
          fade / vtbl+1484 — childhood milestone end
```

`00DABAC0` at `00DAC158` **tests** `[esi+80]`. If already 1 it
tears down `Q__OakValeIntro_PostAttack` (`vtbl+1120`). That is
a **reader** of the store, not the writer.

Theresa factory `00DAC420` (`listing-00d80000.txt`):
`mov [esi], 0x12D83A4` then `mov [esi+20], ebx` (quest).
`00DBB2A4` `mov ecx, [ebp+20]` is that parent. **PROVEN**.

---

## MATCH (data / notes already locked)

| Host | Native | Evidence |
|---|---|---|
| `PersistTable.AttackOverStore = 00DBB2A7` | `mov [ecx+80], 1` | `ScriptRuntimeParityTests` |
| `RegionTravel.AttackOverStore = 00DBB2A7` | same VA | same |
| `PersistTable.AttackOverWriterKnown = true` | store VA known | lock; **not** “host runs it” |
| `PersistTable.AttackOverWrite = 00DAADA0` | persist **bind** | `004045C0("AttackOver", this+80)` |
| `AttackOverOffset = 80` | `[quest+80]` | `00DBB2A7` / `00DAC158` |
| `Recovered[0]` default **false** | first-seen 0 | `InstallRecovered` / `CreateFiber` |
| `RegionTravel.TheresaCutscene` | `CS_OAKVALE_INTRO_THERESA` | string + `00DBB21A` |
| `RaidPlayAvi` / `RaidPlayAviRewritten` | `.xmv` / `.wmv` | `00DBB248` + generic rewrite |
| `MazeCutscene` | `CS_OAKVALEINTRO_HESDEADJIM` | `00DBEE5C` |
| `PostAttackQuest` / `PostAttackEnvironment` | `Q__OakValeIntro_PostAttack` / `ENVIRONMENT_OV_POSTATTACK` | `00DBE3C9` / `00DBE42D` |
| `ScriptFactoryTable` `NOVI_Theresa` / `00DAC420` | `00DABAC0` name table | Recovered bind, empty cutscene |
| `FirstSeenPlus80WrittenInStartOakVale = false` | no `mov` in `00DBDE00–00DBF000` | `00DBB2A7-attackover-store` |
| `ScriptRuntime.Update` “Does not write persist fields” | fiber tick is not the store | `ScriptRuntime.cs` |
| No-save omit `Q_NewOakValeIntro` from `WorldPlus172` / `ActivatedQuests` | QST `AddQuest FALSE` | `No_save_does_not_activate_*` |

---

## DIVERGE (live quest never reaches the store)

| Host site | Native after a proven activate | Gap |
|---|---|---|
| `QuestFactoryTable.Recovered` (8 rows, no Oakvale) | 161-row fill includes `00CD6E27` | `Find("Q_NewOakValeIntro")` is null |
| `EngineLifecycle.ActivateNamedQuest` | `004B3CE0` `00DBEF70` / `00DAAC00` | factory arm skipped |
| `ScriptRuntime.ActivateQuest` | “Does not install `S_QNOVI` / `00DBDE40`” | generic fiber; no `00DABAC0` |
| `InstallRecoveredBindings` | `00A446A0` persist then `[vtbl+8]=00DABAC0` | fixture `S_QNOVI` + `AttackOver=false`; **not** `Pump` |
| `StartNewGame` → `ActivateThings` | `NOVI_Theresa` construct `00DAC420` → `00DB97A0` | Recovered cutscene name is `""`; `StartNamedScript` no-ops |
| `ScriptRuntime.Update` | `00DB97A0` body after deeds | resumes **cutscene** interpreters only; no Theresa native |
| `EngineLifecycle.PumpScripts` | script-manager walk | Notes `006E75C0`; `ScriptPumpWalked=0` |
| `Runtime.Update` from `Pump` | leftover (`QuestManagerPumpFn` comment) | no-save never calls it |
| `NewGameScript.ApplyPersist(true)` | `00DBB2A7` | **invented** skip; tests only (`WorldSceneTests`) |
| `PersistStore.SetBool` | `mov [ecx+80], 1` | public API; no live caller except tests / default false |
| Generic `PlayAVI` opcode `00CCA26D` | `vtbl+1476` at `00DBB260` | opcode exists; raid file is **not** started from Theresa body |
| `CS_OAKVALE_INTRO_FATHER` via `NOVI_LiveFather` | first-seen father CS `00DB86B0` | **before** deeds; not the raid AVI |
| `CS_BANDITRAID_*` fixtures | adult raid | wrong cutscene family |

`EngineLifecycle` never references `AttackOverStore`,
`TheresaCutscene`, `RaidPlayAvi`, `PostAttackQuest`, or
`00DB97A0`. **PROVEN** by `src/` grep.

`PersistStore` header still says “Writer UNREAD”. That comment
is **LEFTOVER** vs `AttackOverWriterKnown=true`. The **VA** is
known; the **live call** is not.

---

## Live-path gap list (to **reach** `00DBB2A7`)

Ordered. Stop at the first unproven item.
Do **not** satisfy these by writing `AttackOver=1`.
Do **not** jump to Guild / `S_QGT`.

1. **Proven activator** of `Q_NewOakValeIntro`
   (`004B4A10` / `00CB5AD0`). Not no-save Leave.
   Not Gameflow `00893610` wait. **blocked-on-activator**.
2. **`QuestFactoryTable` row** —
   `Q_NewOakValeIntro` / `S_QNOVI` / factory `00DBEF70` /
   run `00DABAC0` / persist 0.
3. **Construct** `00DBEF70` → `00DAAC00` size `0x10C`
   vtbl `012D7A28`; fiber `00A447D0`.
4. **Persist bind** `00DAADA0` / `004045C0("AttackOver",
   this+80)` with value **0**. Not the store.
5. **`00DABAC0` name table** including `NOVI_Theresa` /
   `00DAC420` **before** `00DBDE40`.
6. **`00DBDE40` StartOakVale** — map-ready `StartOakValeWest`,
   `00CB7940`, `CREATURE_HERO_CHILD`, three watchers,
   `Q_NewOakValeIntro_PreAttack`, 12 s, `HerosOldHouse`,
   **spin on `+80`**. See `00DBDE40-host-gap`.
7. **Childhood deeds** on the live quest (chocolate /
   `CS_OAKVALE_INTRO_THERESA_MEET*`). Completing them is
   what **falls into** `00DBB218`. Skipping them and
   poking persist is **DISPROVEN**.
8. **`NOVI_Theresa` object** vtbl `012D83A4` running
   `00DB97A0` (`[thing+20]` = `S_QNOVI`). Host bind has
   empty `CutsceneName`; factory start body still **UNREAD**
   as a host analog (`ScriptFactoryTable.Bind` note).
9. **`00CBFB7D("CS_OAKVALE_INTRO_THERESA")`** at `00DBB238`.
   Not `CS_OAKVALE_INTRO_FATHER`. Not `CS_BANDITRAID_*`.
10. **PlayAVI** `Data\Video\1_raid_on_oak_vale_comp.xmv`
    `vtbl+1476` at `00DBB260`. Generic `IScriptHost.PlayAvi`
    rewrite to `.wmv` MATCHES the opcode; **this call site**
    is not wired.
11. **Then** `00DBB2A7` `mov [ecx+80], 1`.

**After** the store (milestone tail, not the gap to the write):

12. `00DBDE40` spin exits.
13. `00DBE3C0` `Q__OakValeIntro_PostAttack` +
    `ENVIRONMENT_OV_POSTATTACK`.
14. `00DBEB20` `CS_OAKVALEINTRO_HESDEADJIM` + fade.
    That is childhood end. Not Guild arrival.

---

## Fixture vs live (do not confuse)

| Path | What it does with `AttackOver` | Class |
|---|---|---|
| `EngineLifecycle` no-save Pump | never installs the slot; never stores 1 | **PROVEN** omit |
| `ScriptRuntime.StartNewGame` | `InstallRecoveredBindings` → bool **false**; father CS only | **LEFTOVER** vs Pump |
| `FirstSceneWorld.Build` | calls `StartNewGame` | leftover #4 soup |
| `NewGameScript.ApplyPersist(true)` | writes the slot from C# | **DIVERGE** / skip |
| `ScriptRuntimeParityTests` | asserts VA + names; persist still false | **MATCH** constants |

---

## Do not

- Call `ApplyPersist("AttackOver", true)` / `Gate80=true` because
  deeds are unimplemented.
- Treat `AttackOverWriterKnown=true` as “host reached the store”.
- Start `CS_BANDITRAID_*` as the childhood raid AVI.
- Start `CS_OAKVALEINTRO_HESDEADJIM` / PostAttack **before**
  `00DBB2A7`.
- Walk `S_QGT` / `00D3BC60` / `GuildArrivalHSP` as this
  milestone.
- Invent `ActivateQuest("Q_NewOakValeIntro")` on no-save Leave
  to “reach” the store.
- Grow `Pump` / `PumpScripts` to Note-execute `00DB97A0`.
