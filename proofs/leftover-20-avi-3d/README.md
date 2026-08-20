# Leftover #20 — Game PlayAVI vs 3D Present

Investigation only. No `src/` or `tests/` edits.

Question: during **Game** `PlayAVI`, does native keep 3D
Present running or pause world draw? Recover leftover #20
versus host.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

`FABLE_SKIP_STARTUP_AVI` is host `FinishStartupVideo`, not
native DIK skip. Do **not** treat AVI skip as a 3D Draw
fix.

Authority: ExeIndex `playavi-timeline/` (`00CCA26D` /
`0088F890` / `006286F0` / `00A3B9D0` / `009BEEB0`);
`RegionTravel` first-seen flags; `ScriptCommandMap`
`PlayAVI`; `EngineLifecycle.Pump` / `ApplyDisplayCamera`;
`SilkEngineHost` / `Program.cs`; `WmvPlayer`; tests
`PlayAvi_rewrites_xmv_to_installed_wmv_and_blocks`,
`Game_00435530_Presents_009BEEB0_and_pumps_input`.

Siblings: `proofs/issue-20-verify` (startup-era, stale);
`proofs/leftover-9-20-status` (startup Present **MATCH**;
does not dump Game `00435530`); `proofs/audio-frontend`
(`0042DED5` is voice, not Draw).

---

## Verdict

**Native Game PlayAVI pauses world draw.** It does **not**
keep `00435530` 3D Present running.

The apply `00CCA26D` → `vtbl+1476` `0088F890` → blocking
`006286F0(edx=0x1B)` does not return until EOF / skip.
`006286F0` owns the live device pump: WaitEx →
`009BE420` clear → `009BEF20` BeginScene → `009DC870`
blit → `009D9C80` flush → `009DA9F0(1)` 2D DIP →
`009BEF50` EndScene → `009BEEB0` Present. Same
`IDirect3DDevice9::Present` vtbl+68 as Game
`00435530`. FilterGraph is live on that device. No
`00417001` / `00435F70` / ScenePasses walk. `00A44880`
does not resume other fibers or tick fade.
**PROVEN.**

Host leftover #20 is **not** startup logos (those
`Draw(default)` / `playAviOnly` **MATCH**). It is Game
stage: `PumpGame` still walks `00435530` and
`Runtime.Update` never owns the pump.
**LEFTOVER.**

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Game `PlayAVI` keeps 3D `00435530` running? | **No.** Apply blocks; world Present is later in the same `004162B5` and is not reached. | **PROVEN** |
| Native still Presents? | **Yes**, blit-only `006286F0` → `009BEEB0`. Same device, not a second swapchain. | **PROVEN** |
| FilterGraph on a separate window? | **No.** No `IVideoWindow`. Custom texture renderer + game device `[engine+96]`. | **PROVEN** |
| `0042DED5` is this leftover? | **No.** Frontend audio `vtbl+68` after startup AVI returns. | **DISPROVEN** |
| `FABLE_SKIP_STARTUP_AVI` fixes 3D Draw? | **No.** Host `FinishStartupVideo` only. Not DIK 1/57/28/62. Not Game `PlayAVI`. | **DISPROVEN** |
| Startup leftover #20 still open as filed? | Startup 3D WVP under logos is **MATCH**. Remaining leftover is Game `00435530` vs `006286F0`. | **LEFTOVER** (Game) / **MATCH** (startup Present) |

---

## 1. Evidence

### 1.1 Script command `PlayAVI` (`00CCA26D`)

Listing `playavi-token-00cca26d` / apply `00CCA2BD`:

```
00CCA26D  push "PlayAVI"
          00BFEAF8 name match
00CCA2BD  first arg required else jmp 00CD17FD
          prefix "Data\Video\" 0099F570
          [0x143E8F8].vtbl+1476
00CCA319  jmp 00CD17F8          ; join, no vtbl+28
```

`vtbl+1476` is `0088F890`:

```
0088F890  call 0040D2A0         ; singleton [0x13B7D4C]
          push 0 ×5, 0xBF800000, flags, 0 ×4
          mov edx, 0x1B
          call 006286F0         ; blocking player
```

`ScriptCommandMap`: `PlayAVI` `0x00CCA26D` dest
`0x006286F0`, `ScriptReturn.BlockPump`,
`CommandParity.Complete`. Host dispatcher
`GlobalDispatcher` `BeginAvi` + `ExecutionKind.BlockPump`.
Interpreter `TickWait` stays false while
`Runtime.AviPlaying`. **MATCH** at script apply.

First-seen flags (`RegionTravel`, locked by
`PlayAvi_rewrites_xmv_to_installed_wmv_and_blocks`):

| Flag | Value | Class |
|---|---|---|
| `FirstSeenPlayAviIsBlocking` | true | **PROVEN** |
| `FirstSeenPlayAviDoesNotYield` | true | **PROVEN** |
| `FirstSeenPlayAviBlocksUpdatePump` | true | **PROVEN** |
| `FirstSeenPlayAviDrawsWorld` | **false** | **PROVEN** |
| `FirstSeenPlayAviPresentIsDevicePresent` | true | **PROVEN** |
| `FirstSeenPlayAviUsesVideoWindow` | false | **PROVEN** |
| `FirstSeenPlayAvi` at Lookout teleport | **false** | **PROVEN** (Oakvale intro, not first no-save Present) |

Game-stage files: `dream_sequence_comp.xmv` (intro
cutscene) and later `1_raid_on_oak_vale_comp.xmv`
(`PersistTable.AttackOverStore` after Theresa).
**PROVEN** names. Exact first Game-stage fire from
`EngineLifecycle.Pump` is **UNREAD** (host never
calls `Runtime.Update` from the Game pump).

### 1.2 Player `006286F0` (owns Present)

Listing `playavi-player-006286f0` (walk 400 insns):

```
00A3B9D0  open FilterGraph / renderer
00A3B130  Run
00628991  loop:
  009A6460            engine pump; eax==2 → leave
  00A03B70            skip scan 1 / 57 / 28 / 62
  00628A9E            WaitEx 33 ms [0x143FE08]
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

`009BEEB0` is `IDirect3DDevice9::Present` vtbl+68
(`playavi-present-009beeb0`). Game `00435530` tail
is the **same** helper (`Game_00435530_Presents_009BEEB0`).
One swapchain. **PROVEN.**

`009DA9F0(1)` here is the video quad, not
`00435530` ScenePasses (`0x4`→`0x40`→`0x20`→`0x100`→
`0x2000`). PlayAVI never `E8` `00435530` /
`00435F70` / `00417001`. **PROVEN** (listing
callees; no those VAs).

`009A6460` inside the loop is PeekMessage / quit
only. Skip keys work. World tick `004A5A40` /
`00A44880` do not. **PROVEN.**

### 1.3 FilterGraph on the live device

`00A3B9D0` (`playavi-open-00a3b9d0`):

```
CoCreate [0x1440640] CLSID 0x12AB174 / IID 0x12A9934
         FilterGraph + IGraphBuilder
alloc 0x180 renderer 00A3B510
AddFilter vtbl+12 name "Fable Texture Renderer Filter"
RenderFile vtbl+52 when .wmv/.asf (0099C1E0 rewrite)
QI IMediaControl / IMediaPosition / IMediaSeeking /
   IMediaEvent / IBasicAudio
no IVideoWindow / GetCurrentImage
```

Copy `00A3B730`: `IMediaSample::GetPointer` RGB24 →
`009FA450` LockRect on a texture from `009FA280`
(same `[engine+96]` device `006286F0` later Presents).
**PROVEN.** Graph is live on the game device until
`00A3B380` / dtor `00A3BC20`.

Host `WmvPlayer` CoCreates the same CLSID/IID, copies
to CPU RGBA, `SetVideoFrame` into Vulkan. Analog, not
a second CBaseRenderer cloned into the game.
`PlayAviFromExe` is dump-only. **MATCH** graph
shape; **DIVERGE** no D3D9 texture on the game
device.

### 1.4 `0042DED5` is not Draw

After retail `006286F0` ×3, `0042EC7C` binds UI,
Init Engine, black `009D8CF0`+`009BEEB0`, then
`0042F00A call 0042DED5(0)`: `[0x13B8394].vtbl+68`.
Frontend voice. Three `E8` sites, all inside
`0042EC7C`. Zero `E8` past Leave `0042F2A2`.
**PROVEN** (`proofs/audio-frontend`,
`proofs/audio-after-leave`).

Host `EnterFrontendAfterAvi` `Note(0042DED5 0)`
only. Not a 3D path. Not Game `PlayAVI`.

### 1.5 Game Present `00435530`

`004162B5` update then `vtbl+28` `00417001`. After
`WorldFrame>1`, `00435F70` jmp `00435530`:
BeginScene, Clear, overlay, interface, `009D9C80`,
`009DA9F0(1)` ScenePasses, EndScene, `00435F50`
`009BEEB0`. **PROVEN**
(`Game_00435530_Presents_009BEEB0_and_pumps_input`).

Order on a Game frame that is **not** inside
`PlayAVI`:

```
004189C2 inner
  004162B5 update
    00418289 / 004A5A40 / 00A44880   ; PlayAVI apply lives here
    00417001 render
      00435F70 → 00435530            ; 3D Present — after update returns
```

While `006286F0` runs, update has not returned.
`00417001` is not reached. World draw is **paused**,
device Present is **not**. **PROVEN.**

First Game pump `004189C2` `0040D2A0` / `0040BC80` /
`0040A7F0` is singleton + fade `00B239A0(12,20)`.
It does **not** `E8` `006286F0`. Host
`ApplyFirstPumpAviAndFade` Notes those VAs under
`"PlayAVI"` — name collision, not a video.
**PROVEN** (`First_pump_004189C2_is_0040D2A0_then_00B239A0_not_a_region`).

---

## 2. Original (native)

```
Game fiber 00A44880
  script PlayAVI 00CCA26D
    0088F890
      006286F0  BLOCKS
        FilterGraph live on [engine+96]
        loop: WaitEx / clear / BeginScene / blit / EndScene / 009BEEB0
        009A6460 + DIK 1/57/28/62
        no 00435530, no ScenePasses, no fade tick, no other fibers
      return
    jmp 00CD17F8
  00417001 / 00435530 resume next frame
```

Startup table `0042EC7C` uses the **same** player
before any world exists. After it returns:
`0042DED5` audio, then frontend 2D Present
`0042DF9E`. That is not Game 3D and not leftover
#20.

---

## 3. Host

### 3.1 Script layer **MATCH**

`ScriptRuntime.Update`: if `AviPlaying`, `TickAvi`
only — no fade, no other resume. `WmvPlayer.TryOpen`
is the live FilterGraph. Tests lock blocking +
`IBasicAudio` QI on `dream_sequence_comp.wmv`.
**MATCH** vs `00A44880` stuck in apply.

`PumpUntilSettled` force-`SkipAvi` on `BlockPump`
is a fixture analog of DIK skip, **not**
`FABLE_SKIP_STARTUP_AVI`. **LEFTOVER** only for
fixtures.

### 3.2 `EngineLifecycle` Game pump **LEFTOVER**

`Pump` Game:

```
PumpGame()
  always PumpGameUpdate → UpdateGameMode → RenderGameMode
  if GamePresentCount grew: PresentToHost()
```

No `AviPlaying` gate. `ApplyDisplayCamera` still
Notes `00435530` / `009BEEB0` when `WorldFrame>1`.
`TickWorld` → `PumpScripts` is `006E75C0` Notes;
comment: host walk of `00CB7950` / `Runtime.Update`
is leftover. Game `PlayAVI` never starts from
`Pump()`. **LEFTOVER.**

`BuildFrame` will attach `Runtime.AviPlaying` **if**
something else opened a player. `PresentToHost`
still carries `SubmittedWorld` / landscape verts.
`SilkEngineHost.Present` still `SetMesh` when verts
exist, then `SetVideoFrame` + `SetPlayAviPump(true)`.

### 3.3 Client Present (startup **MATCH**, Game analog)

`Program.cs` Render: `Stage == StartupVideos` →
`Renderer.Draw(default)`. F2 fly is `Stage == Game`
only. `FABLE_SKIP_STARTUP_AVI`:

```
while (life.Stage == StartupVideos)
    life.FinishStartupVideo();
```

Host `FinishStartupVideo`. Not native DIK. Does
not skip Game `PlayAVI`. Does not change
`00435530`. **DISPROVEN** as a 3D Draw fix.

`SilkEngineHost.Draw`: `AviPlaying` →
`Draw(default)`. `VulkanLineRenderer.Record`
`playAviOnly` skips mesh / gizmos / fade /
frontend, then blits video. Startup 3D WVP under
logos is **MATCH** (`proofs/leftover-9-20-status`).
That skip is a host `Record` gate, **not** recovered
“`00435530` did not run because `006286F0` owns the
pump.” **LEFTOVER** vs Game native.

Inter-slot startup `AviPlaying=false` clear is #11,
not this leftover.

### 3.4 `0042DED5`

`Note(0042DED5 0)` after last startup slot.
No player. **MATCH** site; **DISPROVEN** as #20.

---

## 4. Gap (leftover #20 recovered)

| Item | Original | Host | Class |
|---|---|---|---|
| Who Presents during Game `PlayAVI` | `006286F0` blit-only `009BEEB0` | If `AviPlaying`, `Draw(default)` + `playAviOnly`; else `00435530` Notes + 3D WVP | **LEFTOVER** (pump owner) |
| `00435530` / ScenePasses | Does not run until apply returns | `PumpGame` still walks it after `WorldFrame>1` | **LEFTOVER** |
| `00A44880` / fade / other fibers | Stuck in apply | `ScriptRuntime.Update` **MATCH**; `EngineLifecycle` never calls it | **LEFTOVER** (Game pump) / **MATCH** (script unit) |
| FilterGraph | Live on game D3D device | Independent Quartz → CPU RGBA → Vulkan | **MATCH** graph; **DIVERGE** device |
| Same Present helper `009BEEB0` | Yes | Vulkan `Draw` is that Present | **MATCH** |
| Startup logos 3D WVP | No world yet; blit-only | `Draw(default)` / `playAviOnly` | **MATCH** |
| Unload before next `006286F0` | `00A3B380` / `00A3BC20` | `WmvPlayer.Dispose` `GraphReleased` | **MATCH** (not #20) |
| Dest `00628B79` | Width-fit | `PlayAviLetterbox` | **MATCH** (#8/#11) |
| `0042DED5` | Frontend `vtbl+68` | Note-only | **DISPROVEN** as #20 |
| `FABLE_SKIP_STARTUP_AVI` | — | `FinishStartupVideo` | **DISPROVEN** as 3D Draw / native skip |
| First pump `0040D2A0`/`0040A7F0` | Singleton + fade, not `006286F0` | Notes tagged `"PlayAVI"` | **LEFTOVER** name; **MATCH** no video |

Filed issue #20 (“startup PlayAVI still runs 3D Draw”)
is **MATCH** at client Present. Tracker row “leftover
#20 is still 3D Draw during startup PlayAVI” is
**stale** for logos. Recovered leftover: **Game-stage
`00435530` still runs because `006286F0` does not
own `PumpGame`.**

Do **not** close #20 by skipping startup AVI. Do
**not** invent a second swapchain. Do **not** treat
`playAviOnly` comments as recovered `00435530`
skip. Next lock (not this file): `PumpGame` must
not `ApplyDisplayCamera` while script `PlayAVI`
owns the pump — or `Runtime.Update` must block
inside `004162B5` analog until `006286F0` returns.

---

## Do not invent

- `FABLE_SKIP_STARTUP_AVI` / Enter as native DIK skip.
- `0042DED5` as a Draw or Game `PlayAVI` gate.
- A second Present / overlay window.
- `0040A7F0` first pump as `006286F0`.
- Lookout first-seen as `PlayAVI` (`FirstSeenPlayAvi=false`).
- Closing #20 because `Record` skips mesh while a host
  flag is set.
- `GamePause` (`WaitScaledFrames`) as this leftover.
