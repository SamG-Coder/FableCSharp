# Leftover #20 — who owns Pump / 3D Draw during PlayAVI

Investigation only. No `src/` or `tests/` edits.

Question: while **startup** `PlayAVI` runs, who
owns the pump? Does 3D Draw / world Present run?
Native unload `00A3B380` / `00A3BC20` before the
next `006286F0` slot is **MATCH**. Filed leftover
#20 is “3D Draw during startup PlayAVI.” Recover
the exact native skip of world Present, and
whether the host **DIVERGE**s.

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**MATCH** / **DIVERGE**.

Do **not** skip AVI (`FABLE_SKIP_STARTUP_AVI` is
host `FinishStartupVideo`, not a 3D Draw fix,
not native DIK 1/57/28/62). Do **not** invent
`MUSIC_SET` (`0042DED5` after the third slot is
frontend `vtbl+68`; path at `0x1230C3C` /
`0x1230C48` is **UNREAD**).

Authority: ExeIndex `playavi-timeline/`
(`006286F0` / `00A3B9D0` / `00A3B380` /
`00A3BC20` / `00628DEB` / `009BEEB0`);
`0042EC7C` retail table (`0042EDCD`);
`RegionTravel` (`FirstSeenPlayAviDrawsWorld`,
`FirstSeenPlayAviBlocksUpdatePump`,
`GamePlayAviOwnsPump`); `EngineLifecycle.Pump`
/ `PumpStartupAvi` / `UnloadStartupAvi`;
`SilkEngineHost`; `Program.cs`; `WmvPlayer`;
PARITY “Retail pump after PlayAVI”; tests
`Startup_videos_are_three_006286F0_slots`,
`Retail_0042EC7C_after_AVI_clears_then_inits_frontend`,
`PlayAvi_rewrites_xmv_to_installed_wmv_and_blocks`.

Siblings: `proofs/leftover-20-avi-3d` (Game 3D
Present vs blit); `proofs/leftover-20-playavi-pump`
(Game pump owner); `proofs/leftover-9-20-status`
(startup Present **MATCH**; #9 CLOSED);
`proofs/issue-20-verify` (startup-era, stale
`gameCam` / `DrawFrontend` outside
`playAviOnly`); `proofs/audio-frontend`
(`0042DED5` is voice, not Draw).

---

## Verdict

**Native `006286F0` owns the pump while AVI
plays.** Same player for startup table
`0042EC7C` and Game `vtbl+1476`. The loop is
WaitEx → `009BE420` clear → `009BEF20`
BeginScene → `009DC870` blit → `009D9C80`
flush → `009DA9F0(1)` 2D DIP of the blit →
`009BEF50` EndScene → `009BEEB0` Present.
Callees do **not** include `00435530` /
`00435F70` / `00417001` / `0042DF9E`.
**PROVEN.**

**3D Draw does not run during AVI.** There is
no `if (AviPlaying) skip 00435530` flag. The
exact gate is **call-stack**: `0042EC7C` /
`0088F890` does not return from `006286F0`
until EOF / DIK skip. World Present is later
in a caller that has not resumed. Startup has
no world yet (Init Engine is after slot 3).
`FirstSeenPlayAviDrawsWorld=false`.
**PROVEN.**

Unload `00A3B380` / `00A3BC20` before the next
slot is **MATCH** (`WmvPlayer.Dispose` /
`GraphReleased`). That is **not** leftover
#20.

Filed leftover #20 (“startup PlayAVI still
runs 3D Draw”) is **MATCH** at client Present:
`Stage == StartupVideos` → `Draw(default)`;
`AviPlaying` host Draw → `Draw(default)`;
`playAviOnly` skips mesh / gizmo / fade /
frontend. Tracker row “leftover is the 3D
Draw” is **stale** for logos.

Host **DIVERGE** is Game-stage pump ownership:
`GamePlayAviOwnsPump=false`. `PumpGame` still
walks `00435530` after `WorldFrame>1`.
`Pump()` never calls `Runtime.Update`.
**LEFTOVER** (Game), not startup logos.

Do **not** close #20 by skipping startup AVI.
Do **not** invent `MUSIC_SET` as a Draw or
pump gate.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Who owns Pump while startup AVI plays? | Native `006286F0` inside `0042EC7C`. Host `Pump` `StartupVideos` + Silk `Draw(default)`. | **PROVEN** native / **MATCH** 3D skip |
| Who owns Pump while Game AVI plays? | Native same `006286F0` (apply blocked). Host `PumpGame` / `00435530`. | **PROVEN** native / **LEFTOVER** host |
| Does 3D Draw / world Present run during AVI? | **No.** Blit-only `009BEEB0`. Same device, not a second swapchain. | **PROVEN** |
| Exact native skip of world Present? | Call-stack, not a flag. `006286F0` has not returned; `00417001` / `00435530` / `0042DF9E` not reached. Startup: world does not exist yet. | **PROVEN** |
| Host startup gate? | `Stage == StartupVideos` → no `PumpGame`; Render `Draw(default)`. `AviPlaying` → `SilkEngineHost.Draw(default)`. `playAviOnly`. | **MATCH** 3D skip / **DIVERGE** split pump |
| Host Game gate? | None. `PumpGame` always `ApplyDisplayCamera` after `WorldFrame>1`. | **DIVERGE** / **LEFTOVER** |
| Unload `00A3B380`/`00A3BC20` before next slot? | Yes. Open-fail `00628DEB` `call 00A3B380` then `00A3BC20`. Host `UnloadStartupAvi` / `Dispose`. | **MATCH** |
| Is leftover #20 still 3D Draw under logos? | **No.** Client 3D WVP under `StartupVideos` is gone. | **MATCH** (startup Present) |
| `FABLE_SKIP_STARTUP_AVI` fixes 3D Draw? | **No.** Host `FinishStartupVideo` only. | **DISPROVEN** |
| `MUSIC_SET` / `0042DED5` is this leftover? | **No.** After third AVI. Path **UNREAD**. Tests lock `RequestNewGameStartsMusicSet=false`. | **DISPROVEN** Draw / **UNREAD** name |

---

## 1. Evidence

### 1.1 Native player owns Present (`006286F0`)

Listing `playavi-player-006286f0` (walk 400
insns). Five `E8` sites (`calls-playavi-player`):
`0041F933`, **`0042EDCD`** (retail table),
`0042F1CB`, `006D08CE`, **`0088F8C7`**
(Game `vtbl+1476`).

```
006286F0  open 00A3B9D0 / Run 00A3B130
00628991  loop:
  009A6460            PeekMessage; eax==2 → leave
  00A03B70            skip scan 1 / 57 / 28 / 62
  00628A9E            WaitEx 33 ms [player+124]
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
`00435F70` / `00417001` / `0042DF9E`.
`009DA9F0(1)` here is the video quad, not
ScenePasses (`0x4`→`0x40`→`0x20`→`0x100`→
`0x2000`). `009A6460` is PeekMessage / quit
only. World tick `004A5A40` / `00A44880` do
not. **PROVEN.**

Same `IDirect3DDevice9::Present` helper as
Game `00435530` (`playavi-present-009beeb0`).
One swapchain. **PROVEN.**

### 1.2 Exact gate: call-stack, not a flag

**Startup** (`0042EC7C`, PARITY retail row /
`FORWARD_TREE`):

```
0042EC7C  if [0x1375448] && [0x137544A]
  table 3 × 32 bytes
    [0x13961E0] = slot RGBA          ; 0042ED85
    call 006286F0                    ; 0042EDCD BLOCKS
    add edi, 32
    cmp [ebp+108], 3
    jb 0042ED68
  [0x13B8616]==0 skip 009A8840
  0042E98F bind UI
  0042E204 Init Engine
  009D8CF0 + 009BEEB0 black Present
  0042DED5(0)                        ; audio, not Draw
  then frontend 0042DF9E
```

Slots (`Startup_videos_are_three_006286F0_slots`):

| File | Size | RGBA |
|---|---|---|
| `Data\Video\lionhead_logo.xmv` | 640×400 | `0xFFFFFFFF` |
| `Data\Video\Microsoft_Logo.xmv` | 640×480 | `0xFF000000` |
| `Data\Video\intro_comp.xmv` | 640×360 | `0x00000000` |

Init Engine / frontend Present / Game
`00435530` are **after** the third return.
World does not exist while logos play. The
skip of world Present is “caller has not
resumed,” not a byte at `[engine+N]`.
**PROVEN.**

**Game** (`leftover-20-avi-3d` /
`leftover-20-playavi-pump`):

```
004189C2 inner
  004162B5 update
    00418289 / 004A5A40 / 00A44880
      0088F890 → 006286F0 BLOCKS
    00417001 render                 ; not reached
      00435F70 → 00435530           ; after update returns
```

Same gate. `FirstSeenPlayAviBlocksUpdatePump=true`.
`FirstSeenPlayAviDrawsWorld=false`. **PROVEN.**

### 1.3 Unload before next `006286F0` — **MATCH**

Open-fail listing `playavi-open-fail-00628deb`:

```
00628DEB  (006286F0 jl after 00A3B9D0)
  call 00A3B380            ; Release graph QIs
  call 00A3BC20            ; player dtor
  00BFE9BC free
  xor al, al
  ret
```

`00A3B380` (`playavi-release-graph`): Release
on `this+4/+8/+12/+20/+28/+24/+16/+0`, then
CloseHandle `+124`. `00A3BC20`
(`playavi-player-dtor`): `[+32].vtbl+12`
Stop, `CoUninitialize` IAT `0x1440648`,
string dtor, `009BE060`, free. Graph is
dead before `0042EC7C` `add edi,32` opens
the next file. **PROVEN.**

Host:

```
PumpStartupAvi
  TryAdvance
  Ended → UnloadStartupAvi → Dispose
          FinishStartupVideo → next slot
```

`WmvPlayer.TearDown` is `00A3B380` then
`00A3BC20`: Stop, drop QIs, FinalRelease
FilterGraph. `GraphReleased` after join.
`PlayAvi_rewrites_xmv_to_installed_wmv_and_blocks`
locks `ReleaseGraphFn=0x00A3B380`,
`PlayerDtorFn=0x00A3BC20`,
`GraphReleased`. **MATCH.** Does not lock
3D Draw.

### 1.4 `0042DED5` / `MUSIC_SET` is not Draw

After third AVI: `0042F00A call 0042DED5(0)`
`[0x13B8394].vtbl+68`. Frontend voice.
Path `0x1230C3C` / `0x1230C48` **UNREAD**.
Do **not** invent `MUSIC_SET_*`. Script
`PlayMusic` is first-scene `S_QNOVI`, not
this pump. Tests:
`RequestNewGameStartsMusicSet=false`,
`InitSoundPlaysMusicSet=false`.
**DISPROVEN** as leftover #20.

---

## 2. Original (native)

```
0042EC7C retail vtbl+8
  006286F0 ×3                         OWNS PUMP
    00A3B9D0 FilterGraph on [engine+96]
    loop: WaitEx / clear / BeginScene / blit / EndScene / 009BEEB0
    009A6460 + DIK 1/57/28/62
    no 00435530, no ScenePasses, no 0042DF9E, no fade tick
    00A3B380 / 00A3BC20                BEFORE next slot
  0042E98F / Init Engine / black Present / 0042DED5
  then 0042DF9E frontend

Game (later, same player)
  00A44880 apply 0088F890
    006286F0  OWNS PUMP               same blit loop
  00417001 / 00435530 resume next frame
```

No world-Present flag. Skip is “`006286F0`
has the device.”

---

## 3. Host

### 3.1 Startup pump — 3D skip **MATCH**

`Pump` while `Stage == StartupVideos`:

```
PumpInput
QueuedPlayAviSkip → SkipStartupVideo
else PumpStartupAvi
PresentToHost
```

Does **not** enter `PumpGame` /
`ApplyDisplayCamera`. `BuildFrame`
attaches `StartupAvi` RGBA +
`AviPlaying`. Next stage is Frontend
only after slot 3.

`Program.cs` Render:

```
if (life.Stage == EngineStage.StartupVideos)
    host.Renderer.Draw(default);
```

F2 fly is `Stage == Game` only. Default
camera WVP under logos is **DISPROVEN**
(`leftover-9-20-status`; `issue-20-verify`
`gameCam` snippet is stale).

`SilkEngineHost.Draw`:

```
if (cam is null || _frame.AviPlaying)
{
    Renderer.Draw(default);
    return;
}
```

`VulkanLineRenderer.Record` `playAviOnly`
(`_playAviPump` or ready video texture)
skips mesh / gizmos / fade / `DrawFrontend`.
Video dest still records. That is the blit.
**MATCH** vs native “no landscape.”

`Draw(default)` is still the swapchain
Present (`009BEEB0` analog). Do **not**
reopen “calls `Draw`” as #20.

### 3.2 Split pump — analog **DIVERGE**

Native: one blocking `006286F0` both ticks
the graph and Presents.

Host: `Pump` advances `WmvPlayer`; Silk
`window.Render` Presents. Functionally
the same 3D skip on startup. Not the same
call-stack. **DIVERGE** shape; **MATCH**
3D skip.

Inter-slot: native returns, unloads, next
`006286F0` in the same `0042EC7C` loop.
Host one `Present` with `AviPlaying=false`
so `ClearVideoFrame` / `SetPlayAviPump(false)`
(#11 leftover-row clear). `Stage` still
`StartupVideos` → `Draw(default)`. Cold
start has `_meshCount==0`. That is **not**
3D-under-logos. Do not reopen #20.

`SilkEngineHost.Present` AVI branch does
**not** return: if the frame also carries
verts it still `SetMesh`. Startup has none.
Game AVI would upload; `playAviOnly` still
skips Record. Native never walks
ScenePasses. **PARTIAL** host upload vs
native omit.

### 3.3 Game pump — **DIVERGE** / leftover #20

```
PumpGame
  always PumpGameUpdate
    UpdateGameMode → RenderGameMode
      ApplyDisplayCamera    ; 00435F70 → 00435530
  if GamePresentCount grew: PresentToHost
```

No `AviPlaying` gate. `PumpScripts` is
`006E75C0` Notes; `Runtime.Update` never
called from `Pump()`. `GamePlayAviOwnsPump=false`
is the lock. Comment on that flag names
leftover #20. **LEFTOVER.**

Script unit `if (AviPlaying) { TickAvi; return; }`
**MATCH** vs `00A44880` stuck in apply.
`Pump()` never reaches it.

### 3.4 Do not skip AVI

`FABLE_SKIP_STARTUP_AVI`:

```
while (life.Stage == EngineStage.StartupVideos)
    life.FinishStartupVideo();
```

Host `FinishStartupVideo` only. Not DIK
1/57/28/62. Not a 3D Draw fix. Env skip
is **DIVERGE** (`playable-path-now`).
Native skip videos only if
`[0x1375448]==0` or `[0x137544A]==0`
(`je 0042EE3D`). PE defaults are 1, 1.
**DISPROVEN** as leftover #20 close.

---

## 4. Gap

| Item | Original | Host | Class |
|---|---|---|---|
| Pump owner during startup AVI | `006286F0` blit loop | `PumpStartupAvi` + Silk `Draw(default)` | **MATCH** 3D skip / **DIVERGE** split |
| World Present during AVI | Not reached (call-stack) | Startup: not `PumpGame`. Game: still `00435530` | **MATCH** startup / **DIVERGE** Game |
| Exact skip | No flag; `006286F0` not returned | `Stage == StartupVideos` / `AviPlaying` / `playAviOnly` | **MATCH** logos; host gates are analogs |
| Unload before next slot | `00A3B380` / `00A3BC20` | `UnloadStartupAvi` / `GraphReleased` | **MATCH** |
| 3D WVP under logos | None | `Draw(default)`; F2 off | **MATCH** |
| `playAviOnly` | — | Record skip, not recovered `00435530` omit | **MATCH** startup pixels / **LEFTOVER** as Game proof |
| Game `006286F0` owns `PumpGame` | Yes | `GamePlayAviOwnsPump=false` | **LEFTOVER** |
| `0042DED5` / `MUSIC_SET` | Audio after AVI | Note-only; no invented name | **DISPROVEN** as #20 |
| `FABLE_SKIP_STARTUP_AVI` | — | `FinishStartupVideo` | **DISPROVEN** as 3D Draw / native skip |

Filed issue #20 is **MATCH** at startup
Present. Tracker “leftover #20 is still 3D
Draw during startup PlayAVI” is **stale**
for logos. Recovered leftover: **Game-stage
`00435530` still runs because `006286F0`
does not own `PumpGame`.**

Do **not** close #20 by skipping startup
AVI. Do **not** invent a second swapchain.
Do **not** invent `MUSIC_SET`. Next lock
(not this file): `PumpGame` must not
`ApplyDisplayCamera` while script /
Theresa `PlayAVI` owns the pump — or
`Runtime.Update` must block inside the
`004162B5` analog until `006286F0`
returns (`leftover-20-playavi-pump`).

---

## Do not invent

- `FABLE_SKIP_STARTUP_AVI` / Enter as native
  DIK skip or a 3D Draw fix.
- `MUSIC_SET_*` / forest ambience as
  `0042DED5` or a pump gate.
- `0042DED5` as Draw or Game `PlayAVI`.
- A world-Present flag native does not
  have (`if AviPlaying skip 00435530`).
- A second Present / overlay window /
  `IVideoWindow`.
- Closing #20 because `Record` skips mesh
  while a host flag is set — that is the
  startup **MATCH**, not Game pump owner.
- `AttackOver=1` / skip raid AVI
  (`leftover-20-playavi-pump`).
- `0040A7F0` first Game pump as `006286F0`.
