# Leftover #20 remaining — Game PlayAVI 3D Present vs blit

Investigation only. No `src/` or `tests/` edits.

Question: after startup PlayAVI **MATCH**, what
**remains** of leftover #20? During **Game**
`PlayAVI`, what is `host.Draw` versus native
`00435530`? Who owns the pump? Can leftover
close?

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**MATCH** / **DIVERGE**.

Do **not** invent `AttackOver=1`. Do **not**
skip the raid AVI / Theresa CS. Do **not**
treat `FABLE_SKIP_STARTUP_AVI` as Game
`PlayAVI`. Do **not** treat first-pump Notes
tagged `"PlayAVI"` as `006286F0`.

Authority: `proofs/leftover-20-playavi-pump`,
`proofs/leftover-20-avi-3d`,
`proofs/leftover-20-pump-owner`,
`proofs/leftover-9-20-status`; ExeIndex
`playavi-timeline/` (`00CCA26D` / `0088F890` /
`006286F0` / `00A3B9D0` / `009BEEB0`);
`listing-00d80000.txt` `00DBB218`–`00DBB2A7`;
`WmvPlayer.cs`; `EngineLifecycle.Pump` /
`PumpGame` / `ApplyDisplayCamera`;
`RegionTravel.GamePlayAviOwnsPump`;
`SilkEngineHost.Draw` / `Program.cs`; tests
`PlayAvi_rewrites_xmv_to_installed_wmv_and_blocks`,
`No_save_does_not_activate_Q_NewOakValeIntro`,
`Game_00435530_Presents_009BEEB0_and_pumps_input`.

Siblings: `proofs/raid-avi-attackover-order`,
`proofs/raid-avi-attackover-live`,
`proofs/raid-avi-live-path`,
`proofs/00DBB2A7-attackover-store`.
`leftover-9-20-status` **CLOSED** the filed
startup-3D claim. This file is the
**remaining** Game leftover.

---

## Verdict

**Startup logos MATCH. Remaining leftover is
Game-stage `PlayAVI`.**

Filed leftover #20 (“startup PlayAVI still
runs 3D Draw”) is **MATCH** at client Present:
`Stage == StartupVideos` → `Draw(default)`;
`AviPlaying` host Draw → `Draw(default)`;
`playAviOnly` skips mesh / gizmo / fade /
frontend. Unload `00A3B380` / `00A3BC20`
before the next slot is **MATCH**. Tracker
row “leftover is the 3D Draw” is **stale**
for logos (`leftover-9-20-status` CLOSED
that filing).

**Native Game `PlayAVI` owns the pump.** Apply
`vtbl+1476` `0088F890` → blocking
`006286F0(edx=0x1B)` does not return until
EOF / DIK skip. The player loop is blit-only
Present (`009BEEB0`). `004162B5` has not
returned, so `00417001` / `00435530` do
**not** run. Same device, not a second
swapchain. **PROVEN.**

**Host leftover #20 is pump ownership, not
startup 3D WVP.** `GamePlayAviOwnsPump=false`.
`PumpGame` always walks `004162B5` analog →
`00417001` analog → `00435530` after
`WorldFrame>1`. `PumpScripts` is Note-only.
`Runtime.Update` is never called from
`Pump()`. Raid site is not wired. First-seen
no-save never opens a Game `WmvPlayer`.
**LEFTOVER.**

The raid file is that Game-stage PlayAVI:
`00DBB260` `call [edx+1476]`
`Data\Video\1_raid_on_oak_vale_comp.xmv`,
**then** `00DBB2A7` `mov [ecx+80],1`.
`RaidAviIsBanditRaid=false`.
`AttackOverStoreAfterRaidAvi=true`.
**PROVEN** order. Not first-seen. First
no-save does **not** hit `PlayAVI`.
**DISPROVEN** live.

**Leftover cannot close.** Closing the filed
startup claim does not recover Game
`006286F0` owning `PumpGame`. Do not close
by skipping the raid AVI or inventing
`AttackOver=1`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Startup AVI 3D Draw leftover? | **MATCH.** `Draw(default)` / `playAviOnly`. Filed #20 CLOSED for logos. | **MATCH** |
| Remaining leftover? | Game-stage `PlayAVI`: `00435530` still runs because `006286F0` does not own `PumpGame`. | **LEFTOVER** |
| Native `00435530` during Game `PlayAVI`? | **Does not run.** Call-stack: apply blocked in `006286F0`. | **PROVEN** |
| Native Present during Game `PlayAVI`? | Blit-only `006286F0` → `009BEEB0`. Same helper as `00435530`. No ScenePasses. | **PROVEN** |
| Host `host.Draw` during Game `PlayAVI`? | Game `Program` always `host.Draw(aspect)`. `AviPlaying` → `Draw(default)`; else 3D WVP. Game `Pump()` never sets `AviPlaying`. | **LEFTOVER** |
| Who owns pump (native Game AVI)? | `006286F0` blit loop. `00A44880` stuck in `vtbl+1476`. | **PROVEN** |
| Who owns pump (host Game)? | `PumpGame` / `ApplyDisplayCamera`. `GamePlayAviOwnsPump=false`. | **LEFTOVER** |
| Who owns pump (startup)? | Native `006286F0`. Host `PumpStartupAvi` + `Draw(default)`. | **MATCH** 3D skip |
| First no-save hits Game `PlayAVI`? | **No.** No `Q_NewOakValeIntro`. First `"PlayAVI"` Notes are singleton + fade. | **DISPROVEN** |
| Raid file? | `1_raid_on_oak_vale_comp.xmv` at `00DBB260`. | **PROVEN** |
| `AttackOverStore` `00DBB2A7` vs that AVI? | **After.** `AttackOverStoreAfterRaidAvi=true`. | **PROVEN** |
| `RaidAviIsBanditRaid`? | **false.** Adult `CS_BANDITRAID_*` is a different family. | **PROVEN** |
| May we poke `AttackOver=1` to skip the AVI? | **No.** Invents the store. Skips Theresa CS + raid player. | **DISPROVEN** writer |
| Can leftover close? | **No.** Startup MATCH does not recover Game pump owner. | **LEFTOVER** open |

---

## 1. Startup MATCH vs Game leftover

`leftover-9-20-status`: leftover #20 as filed
(3D WVP under `StartupVideos`) is **CLOSED**.
`leftover-20-pump-owner` / `leftover-20-avi-3d`
recover the remaining item: Game
`00435530` vs `006286F0`.

| Stage | Native Present | Host Present | Class |
|---|---|---|---|
| Startup logos (`0042EC7C` ×3) | `006286F0` blit; no world yet | `PumpStartupAvi`; `Program` `Draw(default)`; `playAviOnly` | **MATCH** |
| Inter-slot unload | `00A3B380` / `00A3BC20` before next | `UnloadStartupAvi` / `GraphReleased` | **MATCH** |
| Game, no AVI | `00435F70` → `00435530` → `009BEEB0` | `PumpGame` → `ApplyDisplayCamera`; `host.Draw` 3D WVP | **MATCH** Present helper |
| Game `PlayAVI` | `006286F0` blit; `00435530` **not** reached | `PumpGame` still `00435530`; `host.Draw` 3D unless `AviPlaying` | **LEFTOVER** |

Do **not** reopen “calls `Draw`” as leftover
#20. `Draw(default)` is the swapchain Present
used for the video dest (`009BEEB0` analog).

---

## 2. `host.Draw` vs native `00435530` during Game PlayAVI

### 2.1 Native: blit Present, not `00435530`

Listing `playavi-player-006286f0`
(`leftover-20-avi-3d`):

```
006286F0  open 00A3B9D0 / Run 00A3B130
00628991  loop:
  009A6460            PeekMessage; eax==2 → leave
  00A03B70            skip scan 1 / 57 / 28 / 62
  00628A9E            WaitEx 33 ms
  009BE420            clear
  009BEF20            BeginScene [dev+164]
  00628B79            dest = width-fit + 0.5 leftover
  009FA4E0            texture (WaitEx==0 only)
  009DC870            2D blit
  009D9C80            flush
  009DA9F0(1)         2D DIP of the blit
  009BEF50            EndScene [dev+168]
  009BEEB0            Present [dev+68] NULL,NULL,NULL,NULL
  jmp 00628991
```

Direct-call list has **no** `00435530` /
`00435F70` / `00417001`. `009DA9F0(1)` here
is the video quad, **not** ScenePasses
(`0x4`→`0x40`→`0x20`→`0x100`→`0x2000`).
`FirstSeenPlayAviDrawsWorld=false`.
**PROVEN.**

Game `00435530` on a **non-AVI** frame
(`Game_00435530_Presents_009BEEB0`):

```
BeginScene, Clear, overlay 00435000,
interface 00435070, Flush2D 009D9C80,
FlushLayers 009DA9F0(1) ScenePasses,
EndScene, 00435F50 009BEEB0
```

Same `IDirect3DDevice9::Present` vtbl+68.
Different walk. One swapchain.

Order on a Game frame:

```
004189C2 inner
  004162B5 update
    00418289 / 004A5A40 / 00A44880
      0088F890 → 006286F0 BLOCKS     ; Game PlayAVI
    00417001 render                 ; not reached
      00435F70 → 00435530           ; after update returns
```

While `006286F0` runs, update has not
returned. World draw is **paused**. Device
Present is **not**. Exact skip is
**call-stack**, not `if (AviPlaying) skip
00435530`. **PROVEN.**

### 2.2 Host: `host.Draw` is the `00435530` analog

`Program.cs` Render after logos:

```
if (Stage == StartupVideos)
    host.Renderer.Draw(default);     ; MATCH logos
else if (debugFly) ...
else if (Dx9OwnsFrontendPresent) return;
else
    host.Draw(aspect);               ; Game always
```

`SilkEngineHost.Draw`:

```
if (cam is null || _frame.AviPlaying)
{
    Renderer.Draw(default);          ; blit analog
    return;
}
Renderer.Draw(cam.ViewProjection(...), ...);  ; 00435530 analog
```

| Native Game AVI | Host Game AVI (if `AviPlaying`) | Host Game (live `Pump`) |
|---|---|---|
| No `00435530` | `Draw(default)` skips 3D WVP | `host.Draw` **3D WVP** |
| Blit `009BEEB0` | `playAviOnly` Record skip + video dest | `ApplyDisplayCamera` Notes `00435530` |
| No ScenePasses | `Present` still `SetMesh` if verts | Landscape verts submitted |

`playAviOnly` is a host `Record` gate, **not**
recovered “`00435530` did not run because
`006286F0` owns the pump.”
`SilkEngineHost.Present` AVI branch does
**not** return: if the frame also carries
verts it still `SetMesh`. Native never walks
ScenePasses. **PARTIAL** host upload vs
native omit.

Live Game `Pump()` never `TryOpen` a Game
`WmvPlayer`. `BuildFrame` will attach
`Runtime.AviPlaying` **if** something else
opened a player. Nothing in `PumpGame`
does. So live Game `host.Draw` is the 3D
`00435530` analog even on frames that native
would spend inside `006286F0`. **LEFTOVER.**

`ApplyFirstPumpAviAndFade` Notes first
`004189C2` VAs under `"PlayAVI"`. Those are
singleton + fade `00B239A0`, **not**
`006286F0`. **DISPROVEN** as video.

---

## 3. Who owns the pump

### 3.1 Native Game — `006286F0`

`00CCA26D` opcode (father
`dream_sequence_comp.xmv`, if quest live)
and raid `00DBB260` `vtbl+1476` both call
`0088F890` → blocking `006286F0`.
`FirstSeenPlayAviBlocksUpdatePump=true`.
`FirstSeenPlayAviDoesNotYield=true`.
`00A44880` is stuck in that call. Fade does
not tick. Other fibers do not resume.
**PROVEN.**

Startup table `0042EC7C` uses the **same**
player **before** any world. Not Game. Not
this remaining leftover.

### 3.2 Host Game — `PumpGame`

`EngineLifecycle.Pump` Game:

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
is the lock (`WorldSceneTests`,
`EngineLifecycleTests`). Comment on that
flag: leftover #20. `PumpScripts` Notes
`006E75C0`; `QuestManagerPumpFn` comment:
host walk of `00CB7950` / `Runtime.Update`
is leftover. `EngineLifecycle` has no
`Runtime.Update`, no `BeginAvi`, no
`RaidPlayAvi`, no `AttackOverStore`, no
`00DB97A0`. **PROVEN** omit.

Script unit `if (AviPlaying) { TickAvi; return; }`
**MATCH** vs `00A44880` stuck in apply.
`Pump()` never reaches it.

`WmvPlayer` is the live FilterGraph analog
(`00A3B9D0` CoCreate / AddFilter /
RenderFile / `IBasicAudio` QI).
`EngineLifecycle` opens it only in
`EnsureStartupAvi` while
`Stage==StartupVideos`. Opcode fixtures
open it from `IScriptHost.PlayAvi`. Game
`Pump()` never `TryOpen`. **MATCH** graph;
**LEFTOVER** Game pump.

---

## 4. Raid AVI is the Game-stage leftover path

`listing-00d80000.txt` `00DBB218`–`00DBB2A7`:

```
00DBB218  push "CS_OAKVALE_INTRO_THERESA"
00DBB238  call 00CBFB7D                   ; MUST PLAY
00DBB248  push "Data\Video\1_raid_on_oak_vale_comp.xmv"
00DBB260  call [edx+1476]                 ; MUST PLAY — 0088F890 / 006286F0
00DBB28D  vtbl+1492 fade
00DBB29E  vtbl+2784(25)
00DBB2A4  mov ecx, [ebp+20]               ; parent S_QNOVI
00DBB2A7  mov [ecx+80], 1                 ; AttackOver STORE
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

`006286F0` must **return** before
`00DBB2A7`. Writing persist from C# skips
the player that owns the pump.
**DISPROVEN** writer.

Two Game PlayAVI sites, one player:

| Site | File | When |
|---|---|---|
| Opcode `00CCA26D` in father CS | `dream_sequence_comp.xmv` | first-seen **if** `NOVI_LiveFather` constructs |
| Native `00DBB260` after Theresa CS | `1_raid_on_oak_vale_comp.xmv` | after objective 05 / radius 2.0 |

Both own the Game pump the same way. Host
`IScriptHost.PlayAvi` is the **opcode** path
only. Raid call site is not wired.
**PROVEN** native; **LEFTOVER** raid site.

Do **not** skip `CS_OAKVALE_INTRO_THERESA`
or the raid file. Skip CS still returns to
`00DBB248`. Skip persist skips both.

---

## 5. First no-save does **not** hit PlayAVI

`FirstSeenPlayAvi=false`. First `004189C2`
after dummy `004FC180` is `0040D2A0` /
`00B239A0(12, 20.0)` fade — **not**
`E8 006286F0`. Locked by
`First_pump_004189C2_is_0040D2A0_then_00B239A0_not_a_region`.

`No_save_does_not_activate_Q_NewOakValeIntro`:
quest not in `WorldPlus172` / `ActivatedQuests`
/ `Runtime.Quests`. Without that quest:

- father CS / opcode `dream_sequence` never starts
- Theresa `00DB97A0` never starts
- raid AVI never starts
- `00DBB2A7` never runs

So first no-save **does not exercise**
remaining leftover #20. The leftover is
still open because Game `PumpGame` would
keep `00435530` if those sites later ran.
Do **not** invent `ActivateQuest` on Leave
to “reach” PlayAVI. Do **not** invent
`AttackOver=1` to skip the AVI and pretend
the leftover closed.

Native first-seen **with** a proven
activator **would** hit opcode
`dream_sequence_comp.xmv` (fixture
`ScriptRuntime.StartNewGame` does). That
is **not** no-save `Pump()`. Activator of
`Q_NewOakValeIntro` stays **UNREAD** /
blocked-on-activator
(`raid-avi-live-path`).

---

## 6. Can leftover close?

**No.**

| Close attempt | Why it fails |
|---|---|
| Startup `Draw(default)` / `playAviOnly` | Logos **MATCH**. Remaining is Game pump owner. |
| `leftover-9-20-status` CLOSED #20 | Closed the **filed** startup-3D claim. Recovered leftover is Game `00435530`. |
| `FABLE_SKIP_STARTUP_AVI` | Host `FinishStartupVideo`. Not DIK. Not Game. **DISPROVEN** as 3D Draw fix. |
| Invent `AttackOver=1` / `ApplyPersist(true)` | Skips Theresa CS + raid `006286F0`. Store is **after** the AVI. **DISPROVEN.** |
| `SkipAvi` / `PumpUntilSettled` on raid | Opcode fixture analog. **Forbidden** on live `00DBB260`. |
| First no-save never hits PlayAVI | True, and **not** a close. Remaining leftover is later Game AVI. |
| `GamePlayAviOwnsPump=false` lock | Tests assert the leftover. Flipping the constant without recovering `PumpGame` is invention. |
| Closing because `Record` skips mesh while a host flag is set | Startup **MATCH**, not Game pump owner. |

Next lock (not this file): `PumpGame` must
not `ApplyDisplayCamera` while script /
Theresa `PlayAVI` owns the pump — or
`Runtime.Update` must block inside the
`004162B5` analog until `006286F0`
returns. Raid site still needs the live
`00DB97A0` tail **after** childhood deeds;
that is `raid-avi-live-path`, not a persist
poke. Then `GamePlayAviOwnsPump` can become
true while apply runs.

---

## 7. Gap (remaining leftover #20)

| Item | Original | Host | Class |
|---|---|---|---|
| Startup 3D WVP under logos | None; blit-only | `Draw(default)` / `playAviOnly` | **MATCH** |
| Who pumps during Game `PlayAVI` | `006286F0` blit-only `009BEEB0` | `PumpGame` / `00435530` | **LEFTOVER** |
| `00435530` / ScenePasses | Does not run until apply returns | Always after `WorldFrame>1` | **LEFTOVER** |
| `host.Draw` during Game AVI | — (native has no host) | 3D WVP unless `AviPlaying`; live never sets it | **LEFTOVER** |
| `GamePlayAviOwnsPump` | would be true while apply runs | `false` | **LEFTOVER** |
| Raid `00DBB260` | blocking `vtbl+1476` | constants only | **MATCH** data / **LEFTOVER** site |
| `00DBB2A7` after AVI | `mov [ecx+80],1` when player returns | `AttackOverStoreAfterRaidAvi=true`; live never | **MATCH** order / **DISPROVEN** live |
| First-seen no-save PlayAVI | none (quest not constructed) | none | **DISPROVEN** |
| Opcode `dream_sequence` | first Game AVI if quest live | fixture `StartNewGame` only | **MATCH** unit / **LEFTOVER** pump |
| FilterGraph | live on game D3D device | Quartz → CPU RGBA → Vulkan | **MATCH** graph; **DIVERGE** device |
| Unload before next slot | `00A3B380` / `00A3BC20` | `WmvPlayer.Dispose` | **MATCH** (not remaining #20) |

---

## Do not invent

- `AttackOver=1` from deeds skip /
  `ApplyPersist(true)` / `SkipAvi` /
  `Gate80=true`.
- Skipping `CS_OAKVALE_INTRO_THERESA` or
  `1_raid_on_oak_vale_comp.xmv`.
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
- Closing remaining #20 because startup
  logos `Draw(default)` / `playAviOnly`.
- A world-Present flag native does not
  have (`if AviPlaying skip 00435530`).
- A second Present / overlay window /
  `IVideoWindow`.
- Guild take `00D3BC60` as this milestone.
