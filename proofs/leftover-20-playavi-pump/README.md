# Leftover #20 — Game PlayAVI owning Pump

Investigation only. No `src/` edits.

Question: during **Game-stage** `PlayAVI`, who
owns the pump? Does first-seen no-save hit
`PlayAVI`? Raid file is
`1_raid_on_oak_vale_comp.xmv`. Store
`AttackOverStore` `00DBB2A7` is **after** that
AVI. `RaidAviIsBanditRaid=false`.
`AttackOverStoreAfterRaidAvi=true`.

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**MATCH**.

Do **not** invent `AttackOver=1`. Do **not**
skip the raid AVI / Theresa CS. Do **not**
treat `FABLE_SKIP_STARTUP_AVI` as Game
`PlayAVI`. Do **not** treat first-pump Notes
tagged `"PlayAVI"` as `006286F0`.

Authority: ExeIndex `playavi-timeline/`
(`00CCA26D` / `0088F890` / `006286F0` /
`00A3B9D0` / `009BEEB0`);
`listing-00d80000.txt` `00DBB218`–`00DBB2A7`;
`RegionTravel` (`GamePlayAviOwnsPump`,
`RaidPlayAvi`, `AttackOverStoreAfterRaidAvi`);
`EngineLifecycle.Pump` / `PumpGame` /
`ApplyDisplayCamera` / `PumpScripts`;
`ScriptRuntime.Update` / `IScriptHost.PlayAvi`;
`WmvPlayer`; `PlayAviTimeline` (observation);
tests `PlayAvi_rewrites_xmv_to_installed_wmv_and_blocks`,
`First_pump_004189C2_is_0040D2A0_then_00B239A0_not_a_region`,
`No_save_does_not_activate_Q_NewOakValeIntro`,
`Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`.

Siblings: `proofs/leftover-20-avi-3d` (3D
Present vs blit; this file is **pump owner**);
`proofs/raid-avi-attackover-live`;
`proofs/raid-avi-live-path`;
`proofs/00DBB2A7-attackover-store`;
`proofs/leftover-9-20-status` (startup Present
**MATCH**; does not dump Game `00435530`).

---

## Verdict

**Native Game `PlayAVI` owns the pump.** Apply
`vtbl+1476` `0088F890` → blocking
`006286F0(edx=0x1B)` does not return until
EOF / DIK skip. `00A44880` is stuck in that
call. `004162B5` has not returned, so
`00417001` / `00435530` do not run. The
player loop is the live device pump:
WaitEx → BeginScene → blit → EndScene →
`009BEEB0`. **PROVEN.**

The raid file is a Game-stage PlayAVI of
that shape: `00DBB260` `call [edx+1476]`
`Data\Video\1_raid_on_oak_vale_comp.xmv`,
**then** `00DBB2A7` `mov [ecx+80],1`.
`RaidAviIsBanditRaid=false`.
`AttackOverStoreAfterRaidAvi=true`.
**PROVEN** order. Not first-seen.

**Host leftover #20 is pump ownership.**
`GamePlayAviOwnsPump=false`. `PumpGame`
always walks `004162B5` → `00417001` →
`00435530` after `WorldFrame>1`.
`PumpScripts` is Note-only.
`Runtime.Update` is never called from
`Pump()`. Raid site is not wired. First-seen
no-save never opens a Game `WmvPlayer`.
**LEFTOVER.**

Startup logos (`PumpStartupAvi`) already
own Present. That is **MATCH**, not this
leftover.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Who owns Pump during native Game `PlayAVI`? | `006286F0` blit-only loop. Script / Theresa body blocked in `vtbl+1476`. | **PROVEN** |
| Does host `PumpGame` yield to that player? | **No.** `GamePlayAviOwnsPump=false`. Always `PumpGameUpdate` / `ApplyDisplayCamera`. | **LEFTOVER** |
| Does first-seen no-save hit Game `PlayAVI`? | **No.** No `Q_NewOakValeIntro`. First pump `"PlayAVI"` Notes are singleton + fade, not `006286F0`. | **DISPROVEN** |
| Is the raid file first-seen? | **No.** Father opcode `dream_sequence_comp.xmv` is first Game AVI **if** the quest constructs. Raid is after objective 05. | **PROVEN** later / **DISPROVEN** first-seen |
| Does no-save hit the raid AVI? | **No.** Quest never constructed. `EngineLifecycle` never Notes `RaidPlayAvi` / `00DB97A0`. | **DISPROVEN** live |
| Is `00DBB2A7` after the raid AVI? | **Yes.** `AttackOverStoreAfterRaidAvi=true`. Store runs when `006286F0` **returns**. | **PROVEN** |
| Is that AVI `CS_BANDITRAID_*`? | **No.** `RaidAviIsBanditRaid=false`. Adult raid family. | **DISPROVEN** |
| May we poke `AttackOver=1` to skip the AVI? | **No.** That invents the store. | **DISPROVEN** writer |
| Does `ScriptRuntime.Update` MATCH native apply? | **Yes** at unit: `AviPlaying` → `TickAvi` only. `Pump()` never calls it. | **MATCH** (script) / **LEFTOVER** (Game pump) |
| Does `WmvPlayer` own Game Pump? | **No.** `EngineLifecycle` opens it only for `StartupVideos`. Opcode fixtures open it from `IScriptHost.PlayAvi`. | **MATCH** graph / **LEFTOVER** Game pump |

---

## 1. Evidence

### 1.1 Native player owns the Game pump

Listing `playavi-token-00cca26d` /
`playavi-vtbl1476-0088f890` /
`playavi-player-006286f0`:

```
00CCA26D  push "PlayAVI"
00CCA2BD  prefix "Data\Video\" 0099F570
          [0x143E8F8].vtbl+1476
00CCA319  jmp 00CD17F8          ; no vtbl+28
0088F890  call 0040D2A0
          mov edx, 0x1B
          call 006286F0         ; BLOCKS
00628991  loop:
  009A6460            PeekMessage / quit
  00A03B70            skip 1 / 57 / 28 / 62
  00628A9E            WaitEx 33 ms
  009BEF20            BeginScene
  009DC870            2D blit
  009BEF50            EndScene
  009BEEB0            Present
  jmp 00628991
```

`FirstSeenPlayAviIsBlocking=true`.
`FirstSeenPlayAviDoesNotYield=true`.
`FirstSeenPlayAviBlocksUpdatePump=true`.
`FirstSeenPlayAviDrawsWorld=false`.
`PlayAviTimeline` names those sites; it
does not pace Receive / Present.

Same `IDirect3DDevice9::Present` helper as
Game `00435530`. Order on a **non-AVI**
Game frame:

```
004189C2 inner
  004162B5 update
    00418289 / 004A5A40 / 00A44880   ; PlayAVI apply lives here
    00417001 render
      00435F70 → 00435530            ; after update returns
```

While `006286F0` runs, update has not
returned. World draw is paused. Device
Present is **not**. **PROVEN**
(`leftover-20-avi-3d`).

Raid PlayAVI is the **same** `vtbl+1476`
helper, not a second player.

### 1.2 Raid AVI is Game-stage, after Theresa CS

`listing-00d80000.txt` `00DB97A0` tail
(`raid-avi-attackover-live`):

```
00DBB218  push "CS_OAKVALE_INTRO_THERESA"
00DBB238  call 00CBFB7D                   ; MUST PLAY
00DBB248  push "Data\Video\1_raid_on_oak_vale_comp.xmv"
00DBB260  call [edx+1476]                 ; MUST PLAY — 0088F890 / 006286F0
00DBB28D  vtbl+1492 fade
00DBB2A4  mov ecx, [ebp+20]               ; parent S_QNOVI
00DBB2A7  mov [ecx+80], 1                 ; AttackOver STORE
          ret 00DBB304
```

| Flag / name | Value | Class |
|---|---|---|
| `RaidPlayAvi` | `1_raid_on_oak_vale_comp.xmv` | **PROVEN** |
| `TheresaRaidPlayAviSite` | `00DBB249` | **MATCH** VA |
| `PlayAviVtbl` | 1476 | **MATCH** |
| `AttackOverStore` | `00DBB2A7` | **PROVEN** |
| `AttackOverStoreAfterRaidAvi` | **true** | **PROVEN** |
| `FirstSeenAttackOverStoreRuns` | **false** | **PROVEN** |
| `RaidAviIsBanditRaid` | **false** | **PROVEN** |
| compiled-def `0484` vector 0 `PlayAVI` | **none** | **PROVEN** native-after-CS |

The store is **not** inside `00DBDE40`
(`HerosOldHouse` spin **reads** `+80`).
`00DAADA0` is persist **bind**, seed 0.
`CS_BANDITRAID_*` is adult raid.
**DISPROVEN** as this AVI.

`006286F0` must **return** before
`00DBB2A7`. Writing persist from C# skips
the player that owns the pump.
**DISPROVEN** writer.

### 1.3 Two Game PlayAVI sites, one player

| Site | File | How | When |
|---|---|---|---|
| Opcode `00CCA26D` in `CS_OAKVALE_INTRO_FATHER` | `dream_sequence_comp.xmv` | interpreter `PlayAVI` | first-seen **if** `NOVI_LiveFather` constructs |
| Native `00DBB260` after `CS_OAKVALE_INTRO_THERESA` | `1_raid_on_oak_vale_comp.xmv` | `call [edx+1476]` **not** the opcode | after objective 05 / radius 2.0 |

Both block in `0088F890` → `006286F0`.
Both own the Game pump the same way.
Host `IScriptHost.PlayAvi` is the **opcode**
path only. Raid call site is not wired.
**PROVEN** native; **LEFTOVER** raid site.

### 1.4 First-seen no-save does **not** hit PlayAVI

`FirstSeenPlayAvi=false` (Lookout teleport
is not a video). First `004189C2` after
dummy `004FC180`:

```
0040D2A0 / 0040CEC0 / 0040BC80 / 00407370 / 0040A7F0
00B239A0(12, 20.0) fade
009F2660 / 009F26B0
then inner 004162B5
```

Host `ApplyFirstPumpAviAndFade` Notes those
VAs under `"PlayAVI"`. **Name collision.**
No `E8` `006286F0`. Locked by
`First_pump_004189C2_is_0040D2A0_then_00B239A0_not_a_region`.
**PROVEN** not a video.

No-save Leave never `00CB5AD0("Q_NewOakValeIntro")`.
`No_save_does_not_activate_Q_NewOakValeIntro`:
quest not in `WorldPlus172` / `ActivatedQuests`
/ `Runtime.Quests`. Without that quest:

- father CS / opcode `dream_sequence` never starts
- Theresa `00DB97A0` never starts
- raid AVI never starts
- `00DBB2A7` never runs

`Type1_00CB8220_*` re-locks
`RaidAviIsBanditRaid=false`,
`AttackOverStoreAfterRaidAvi=true`,
`FirstSeenAttackOverStoreRuns=false` on
that no-save pump. Gameflow waits forever
on the name; it does not construct the
quest. **DISPROVEN** live PlayAVI.

Native first-seen **with** a proven
activator **would** hit opcode
`dream_sequence_comp.xmv` (fixture
`ScriptRuntime.StartNewGame` does). That
is **not** no-save `Pump()`. Activator of
`Q_NewOakValeIntro` stays **UNREAD** /
blocked-on-activator
(`raid-avi-live-path`). Do **not** invent
`ActivateQuest` on Leave to “reach”
PlayAVI.

### 1.5 `WmvPlayer` / `PlayAviTimeline`

`WmvPlayer` is the live FilterGraph analog
(`00A3B9D0` CoCreate / AddFilter /
RenderFile / `IBasicAudio` QI).
`EngineLifecycle` opens it only in
`EnsureStartupAvi` while
`Stage==StartupVideos`. Game `Pump()`
never `TryOpen`. `BuildFrame` will attach
`Runtime.AviPlaying` **if** something else
opened a player. Nothing in `PumpGame`
does.

`PlayAviTimeline` logs `006286F0` /
`00A3B730` / `00CA4AA0` wall-clock. Dump
family `playavi-timeline` v10. Observation
only.

---

## 2. Original (native)

```
Game fiber 00A44880
  either:
    opcode PlayAVI 00CCA26D          ; dream_sequence (first-seen, if quest live)
  or:
    00DB97A0 after deeds
      00CBFB7D CS_OAKVALE_INTRO_THERESA
      vtbl+1476 1_raid_on_oak_vale_comp.xmv
  → 0088F890
      006286F0  OWNS PUMP
        FilterGraph on [engine+96]
        loop: WaitEx / blit / 009BEEB0
        009A6460 + DIK 1/57/28/62
        no 00435530, no ScenePasses, no fade tick, no other fibers
      return
  raid only: 00DBB2A7 [quest+80]=1     ; AFTER player returns
  00417001 / 00435530 resume next frame
```

Startup table `0042EC7C` uses the same
player **before** any world. Not Game.
Not leftover #20.

---

## 3. Host

### 3.1 Script unit **MATCH**

`GlobalDispatcher` `PlayAVI` → `BeginAvi` +
`ExecutionKind.BlockPump`.
`ScriptRuntime.Update`:

```
if (AviPlaying) { TickAvi(dt); return; }
```

`IScriptHost.PlayAvi` → `WmvPlayer.TryOpen`.
Interpreter stays on the line until EOF /
`SkipAvi`. Fade does not tick. Tests lock
blocking + `IBasicAudio` on
`dream_sequence_comp.wmv`. **MATCH** vs
`00A44880` stuck in apply.

`PumpUntilSettled` force-`SkipAvi` on
`BlockPump` is a fixture analog of DIK
skip. **Forbidden** on live raid AVI
(`raid-avi-attackover-live`).

Raid `00DBB260` is **not** this opcode.

### 3.2 `EngineLifecycle` Game pump **LEFTOVER**

`Pump` Game:

```
PumpGame()
  always PumpGameUpdate
    UpdateGameMode          ; 00418289
      TickWorld → PumpScripts   ; 006E75C0 Notes; ScriptPumpWalked=0
    RenderGameMode          ; 00417001
      ApplyDisplayCamera    ; 00435F70 → 00435530 after WorldFrame>1
  if GamePresentCount grew: PresentToHost()
```

No `AviPlaying` gate. `GamePlayAviOwnsPump=false`
is the lock (`WorldSceneTests`). Comment on
that flag: host `PumpGame` still walks
`00435530` because `PumpScripts` is
Note-only. `QuestManagerPumpFn` comment:
host walk of `00CB7950` / `Runtime.Update`
is leftover. `EngineLifecycle` grep: no
`Runtime.Update`, no `BeginAvi`, no
`RaidPlayAvi`, no `AttackOverStore`, no
`00DB97A0`. **PROVEN** omit.

`TickGameflowMain` Notes `PlayAviFlagFn`
`0088E090 0040D2A0 00408340 +49=1`. That
is the singleton **flag**, not
`006286F0`. Same name collision as
`ApplyFirstPumpAviAndFade`.

`SilkEngineHost.Present` will
`SetPlayAviPump(true)` and `Draw(default)`
**if** `EngineFrame.AviPlaying`. Game
`Pump()` never sets that from a raid /
dream file. `playAviOnly` is a host
`Record` gate, not recovered
“`00435530` did not run because
`006286F0` owns the pump.”
**LEFTOVER** vs Game native.

### 3.3 First-seen no-save

| Path | PlayAVI | Class |
|---|---|---|
| `Pump` `StartupVideos` | logos via `WmvPlayer` | **MATCH** (not Game) |
| First `004189C2` `"PlayAVI"` Notes | singleton + fade | **DISPROVEN** as video |
| No-save `PumpGame` | no `WmvPlayer`; `00435530` after `WorldFrame>1` | **PROVEN** omit Game AVI |
| `ScriptRuntime.StartNewGame` fixture | father CS + `dream_sequence` opcode | **MATCH** unit; **LEFTOVER** vs `Pump` |
| Raid `1_raid_on_oak_vale_comp.xmv` | never | **DISPROVEN** live |
| `ApplyPersist("AttackOver", true)` | C# skip | **DISPROVEN** writer |

---

## 4. Gap (leftover #20 recovered as pump owner)

| Item | Original | Host | Class |
|---|---|---|---|
| Who pumps during Game `PlayAVI` | `006286F0` | `PumpGame` / `00435530` | **LEFTOVER** |
| `GamePlayAviOwnsPump` | would be true while apply runs | `false` | **LEFTOVER** |
| Raid `00DBB260` | blocking `vtbl+1476` | constants only | **MATCH** data / **LEFTOVER** site |
| `00DBB2A7` after AVI | `mov [ecx+80],1` when player returns | `AttackOverStoreAfterRaidAvi=true`; live never | **MATCH** order / **DISPROVEN** live |
| First-seen no-save PlayAVI | none (quest not constructed) | none | **DISPROVEN** |
| Opcode `dream_sequence` | first Game AVI if quest live | fixture `StartNewGame` only | **MATCH** unit / **LEFTOVER** pump |
| FilterGraph | live on game D3D device | Quartz → CPU RGBA → Vulkan | **MATCH** graph; **DIVERGE** device |
| Startup logos | `006286F0` before world | `PumpStartupAvi` + `playAviOnly` | **MATCH** (not #20) |

Filed issue #20 (“startup PlayAVI still
runs 3D Draw”) is **MATCH** at client
Present. Recovered leftover: **Game-stage
`006286F0` does not own `PumpGame`.**

Do **not** close #20 by skipping startup
AVI. Do **not** invent `AttackOver=1` to
stand in for the raid player returning.
Do **not** treat `playAviOnly` comments as
recovered `00435530` skip.

Next lock (not this file): `PumpGame` must
not `ApplyDisplayCamera` while script /
Theresa `PlayAVI` owns the pump — or
`Runtime.Update` must block inside the
`004162B5` analog until `006286F0`
returns. Raid site still needs the live
`00DB97A0` tail **after** childhood deeds;
that is `raid-avi-live-path`, not a persist
poke.

---

## Do not invent

- `AttackOver=1` from deeds skip /
  `ApplyPersist(true)` / `SkipAvi` /
  `Gate80=true`.
- `RaidAviIsBanditRaid=true` /
  `CS_BANDITRAID_*` as this file.
- `AttackOverStore` inside `00DBDE40` or
  before `00DBB260`.
- First-seen no-save as `PlayAVI`
  (`FirstSeenPlayAvi=false`).
- `ApplyFirstPumpAviAndFade` /
  `PlayAviFlagFn` as `006286F0`.
- `FABLE_SKIP_STARTUP_AVI` as Game skip
  or a 3D Draw fix.
- `ActivateQuest("Q_NewOakValeIntro")` on
  no-save Leave to “hit” PlayAVI.
- Guild take `00D3BC60` as this milestone.
- Closing #20 because `Record` skips mesh
  while a host flag is set.
