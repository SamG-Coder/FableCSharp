# Issue #20 vs HEAD — Startup PlayAVI still runs 3D Draw

Investigation only. No `src/` or `tests/` edits.

Issue: [Startup PlayAVI still runs 3D Draw; native 006286F0 is blit-only](https://github.com/SamG-Coder/FableCSharp/issues/20)
(open). Tracker row in `docs/status/README.md` already lists this as
**PARTIAL** (unload recovered; leftover is the 3D Draw).

HEAD: `ee084901e8212814d4ca7df599180117f9be5cec` (`master`).

**Status vs HEAD: PARTIAL.**

Not **FIXED**: `window.Render` still always enters
`VulkanLineRenderer.Draw` during `EngineStage.StartupVideos`.
Not **STILL OPEN** as the original snippet: `PresentAvi` /
`gameCam` are gone, and `Record` now skips mesh / gizmo / fade
while the AVI pump is live.

---

## Issue claim (opened 2026-08-18)

Native `006286F0` is WaitEx → `009BEF20` BeginScene → `009DC870`
blit → `009BEF50` EndScene → `009BEEB0` Present. Apply does not
return, so `00A44880` does not tick fade or draw 3D.

Then-client: `window.Update` on `StartupVideos` called
`PresentAvi` and returned; Silk still ran `window.Render` as:

```csharp
var cam = life.Stage == EngineStage.Game && life.HeroSpawned
    ? life.Camera
    : gameCam;
renderer.Draw(
    cam.ViewProjection(aspect), cam.Position, fogPlane,
    cam.SkyViewProjection(aspect),
    cam.HostLandscapeViewProjection(aspect));
```

Claim: during the three startup slots `gameCam` is the default
`ScriptedCamera` (origin + `IntroCameraFovDegrees` 72). `Draw`
always records the mesh/line pass. `SetPlayAviPump` only flips
the swapchain to FIFO.

**Done looks like** (issue body):

1. While `006286F0` owns the pump, Present the video dest only —
   no mesh / line / sky submit.
2. Keep WaitEx / FIFO / `009BEEB0`. Do not invent a second
   swapchain.
3. Dest sizing stays #8. Do not draw Lookout or Oakvale under
   the logos.

Related closed work is **not** this leftover: dest vs 1600×900
(#8), recreate-on-height / `00628B79` (#11). Unload
`00A3B380`/`00A3BC20` before the next slot is recovered
(`0ace433`) and does not lock #20.

---

## Native contrast (still true)

`RegionTravel` still documents blit-only Present:

```156:166:C:\FableCSharp\src\Fable.Game\RegionTravel.cs
    /// <summary>
    /// <c>006286F0</c> after WaitEx:
    /// <c>009BEF20</c> BeginScene <c>[dev+164]</c>,
    /// blit/flush, <c>009BEF50</c> EndScene
    /// <c>[dev+168]</c>, <c>009BEEB0</c>
    /// <c>IDirect3DDevice9::Present</c>
    /// <c>[dev+68]</c> (NULL,NULL,NULL,NULL).
    /// The apply does not return, so
    /// <c>00A44880</c> does not resume other
    /// fibers or tick fade. No 3D draw.
    /// WaitEx is the wait between Presents.
```

Startup table is the same three `006286F0` slots:

```2216:2221:C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs
    public static readonly StartupVideo[] StartupVideos =
    [
        new("Data\\Video\\lionhead_logo.xmv", 640, 400, 0xFFFFFFFFu, 0x0042E3CE),
        new("Data\\Video\\Microsoft_Logo.xmv", 640, 480, 0xFF000000u, 0x0042E3CE),
        new("Data\\Video\\intro_comp.xmv", 640, 360, 0x00000000u, 0x0042E3CE),
    ];
```

---

## HEAD walk

### 1. `Program.cs` Update — `PresentAvi` gone (moved)

Issue-era Update called `PresentAvi` then returned on
`StartupVideos`. HEAD Update only queues skip keys and pumps.
No stage check, no Present, no `renderer.Draw`:

```68:131:C:\FableCSharp\src\Fable.Client\Program.cs
window.Update += dt =>
{
    // ... Escape/Space/Enter/F4 queue PlayAviSkip* ...
    host.Width = window.FramebufferSize.X;
    host.Height = window.FramebufferSize.Y;
    if (!life.Pump((float)dt) || life.Stage == EngineStage.Shutdown)
        window.Close();
    window.Title = life.WindowTitle;
};
```

`PresentAvi` / `gameCam` have **no** production hits. Camera
choice moved into the host.

### 2. `EngineLifecycle` StartupVideos pump

`Pump` owns the three slots. It presents a frame every tick:

```2968:2978:C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs
    public bool Pump(float dt)
    {
        if (Stage == EngineStage.StartupVideos)
        {
            PumpInput();
            if (QueuedPlayAviSkip())
                SkipStartupVideo();
            else
                PumpStartupAvi(dt);
            PresentToHost();
            return true;
        }
```

Between slots the engine **intentionally** presents
`AviPlaying=false` so the host clears leftover rows (#11), not
to start 3D:

```3222:3236:C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs
    private void PumpStartupAvi(float dt)
    {
        EnsureStartupAvi();
        if (StartupAvi is null)
            return;
        StartupAvi.TryAdvance(dt);
        if (!StartupAvi.Ended)
            return;
        UnloadStartupAvi();
        if (Stage == EngineStage.StartupVideos)
            FinishStartupVideo();
        // Next Pump opens the next file. This
        // Present is AviPlaying=false so the
        // host clears the previous AVI first.
    }
```

`BuildFrame` always attaches `Camera` (a live
`ScriptedCamera`, default origin + 72° FOV until New Game
seeds it). AVI pixels are a side payload, not a different
Present type:

```2528:2528:C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs
    public ScriptedCamera Camera { get; } = new();
```

```3275:3292:C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs
    public EngineFrame BuildFrame()
    {
        var avi = StartupAvi;
        var runtime = Runtime;
        var playing = avi is { Rgba: not null } ||
                      runtime is { AviPlaying: true, AviRgba: not null };
        // ...
        return new EngineFrame(
            Camera,
            SubmittedWorld,
            avi?.Width ?? runtime?.AviWidth ?? 0,
            avi?.Height ?? runtime?.AviHeight ?? 0,
            avi?.Rgba ?? runtime?.AviRgba,
            avi?.FrameSerial ?? runtime?.AviFrameSerial ?? 0,
            playing,
```

`FovDegrees` default is still the issue's 72° intro camera:

```38:38:C:\FableCSharp\src\Fable.Game\ScriptedCamera.cs
    public float FovDegrees { get; private set; } = RegionTravel.IntroCameraFovDegrees;
```

### 3. `window.Render` still always calls `Draw`

Silk still invokes Render every frame during `StartupVideos`.
There is no `Stage` / `AviPlaying` gate. Debug-fly is a raw
3D `Renderer.Draw`. The normal path is `host.Draw`:

```133:148:C:\FableCSharp\src\Fable.Client\Program.cs
window.Render += _ =>
{
    if (window.FramebufferSize.X == 0 || host.Renderer is null)
        return;
    var aspect = window.FramebufferSize.X / (float)window.FramebufferSize.Y;
    if (debugFly)
    {
        var fog = Fable.Formats.WorldShading.LinearFogPlane(debugCam.Position, debugCam.Forward);
        host.Renderer.Draw(
            debugCam.ViewProjection(aspect), debugCam.Position, fog,
            debugCam.SkyViewProjection(aspect),
            debugCam.HostLandscapeViewProjection(aspect));
    }
    else
        host.Draw(aspect);
};
```

`SilkEngineHost.Draw` always builds world / sky / landscape
WVP from that default camera and calls `Renderer.Draw`:

```181:206:C:\FableCSharp\src\Fable.Client\SilkEngineHost.cs
    public void Draw(float aspect)
    {
        if (Renderer is null)
            return;

        var cam = _frame.Camera;
        if (cam is null)
        {
            Renderer.Draw(default);
            return;
        }

        var nativeAspect = EngineLifecycle.DisplayDefaultWidth
            / (float)EngineLifecycle.DisplayDefaultHeight;
        _ = aspect;
        var fogPlane = WorldShading.LinearFogPlane(cam.Position, cam.Forward);
        Renderer.Draw(
            cam.ViewProjection(nativeAspect),
            cam.Position,
            fogPlane,
            cam.SkyViewProjection(nativeAspect),
            cam.HostLandscapeViewProjection(nativeAspect));
    }
```

`EngineFrame.Camera` is a non-nullable `ScriptedCamera`, so
the `cam is null` branch never fires for lifecycle frames.

This is the leftover the issue named: client Render still
runs the 3D `Draw` entry during the blit-only pump.

### 4. `VulkanLineRenderer` — FIFO plus a Record skip

`SetPlayAviPump` still recreates the swapchain to FIFO
(done-looks-like #2). Comment still says it does **not**
change the 3D interval by itself:

```385:402:C:\FableCSharp\src\Fable.Render\VulkanLineRenderer.cs
    /// <c>006286F0</c> owns the pump: WaitEx then
    /// BeginScene/blit/EndScene/Present. Does not
    /// change the 3D swapchain interval.
    public void SetPlayAviPump(bool on)
    {
        if (_playAviPump == on)
            return;
        _playAviPump = on;
        _resized = true;
    }
```

```707:709:C:\FableCSharp\src\Fable.Render\VulkanLineRenderer.cs
        var presentMode = _playAviPump || !presents.Contains(PresentModeKHR.MailboxKhr)
            ? PresentModeKHR.FifoKhr
            : PresentModeKHR.MailboxKhr;
```

Host Present turns the pump on only when a video bitmap is
live; otherwise it **clears** video and turns the pump off
(inter-slot clear, frontend, game):

```62:106:C:\FableCSharp\src\Fable.Client\SilkEngineHost.cs
            if (frame is { AviPlaying: true, AviRgba: not null })
            {
                // ... SetVideoFrame / SetPlayAviPump(true) / SetFrontendBatch(null)
            }
            else if (frame.FrontendBatch is { IsEmpty: false } batch)
            {
                // ... ClearVideoFrame / SetPlayAviPump(false)
            }
            else
            {
                // ... ClearVideoFrame / SetPlayAviPump(false)
            }
```

`Draw` itself is still the 3D Present: fence, acquire,
`ToVulkanWvp` of world/sky/land, `Record`, submit, present:

```404:444:C:\FableCSharp\src\Fable.Render\VulkanLineRenderer.cs
    public void Draw(
        Matrix4x4 viewProjection,
        Vector3 cameraPosition = default,
        Vector4 fogPlane = default,
        Matrix4x4? skyViewProjection = null,
        Matrix4x4? landscapeViewProjection = null)
    {
        // ...
        var vkView = Parity.Dx9Vulkan.Dx9VulkanProjection.ToVulkanWvp(viewProjection);
        var vkSky = Parity.Dx9Vulkan.Dx9VulkanProjection.ToVulkanWvp(
            skyViewProjection ?? viewProjection);
        var vkLand = Parity.Dx9Vulkan.Dx9VulkanProjection.ToVulkanWvp(
            landscapeViewProjection ?? viewProjection);
        Record(
            _commandBuffers[_frame], imageIndex, vkView, cameraPosition, fogPlane,
            vkSky, vkLand);
```

Mitigation (not in the issue-era claim): `Record` now skips
mesh / gizmos / fade when the pump or a ready video texture
is up. Video blit still runs. **`DrawFrontend` is not
gated.**

```1256:1311:C:\FableCSharp\src\Fable.Render\VulkanLineRenderer.cs
        // 006286F0 BeginScene/blit/EndScene/Present
        // does not draw landscape or fade.
        var playAviOnly = _playAviPump ||
            (_videoReady && _videoPipeline.Handle != 0 && _videoTexture.Set.Handle != 0);

        if (!playAviOnly &&
            ((_meshCount > 0 && _meshBuffer.Handle != 0) ||
             (_objectCount > 0 && _objectBuffer.Handle != 0)))
        {
            // ... DrawMeshBatches (sky/land WVP written here)
        }

        if (!playAviOnly && ShowGizmos && _vertexCount > 0 && _vertexBuffer.Handle != 0)
        {
            // ... line gizmos
        }

        if (!playAviOnly && FadeOverlayAlpha > 0 && _overlayPipeline.Handle != 0)
        {
            // ... fade overlay
        }

        DrawFrontend(commandBuffer);

        if (_videoReady && _videoPipeline.Handle != 0 && _videoTexture.Set.Handle != 0)
        {
            // 00628B79 dest blit
```

`docs/status/investigations/G-dx9-vulkan.md` calls that skip
**EQUIVALENT** to native “no landscape”. It does **not** stop
the client from calling the 3D `Draw` entry or from building
origin/72° WVPs.

Cold-start `StartupVideos` has not submitted Lookout yet, so
`_meshCount` is usually 0 even when `playAviOnly` is false
(inter-slot). That is luck, not a blit-only Present. F2
debug-fly during logos still submits 3D.

---

## Score against “done looks like”

| Done item | vs HEAD |
|---|---|
| 1. Video dest only; no mesh/line/sky submit | **PARTIAL.** `Record` skips mesh/line/fade while `_playAviPump` / `_videoReady`. Client still always calls `Draw` with 3D WVP. `DrawFrontend` still runs. Inter-slot / debug-fly leave `playAviOnly` false. |
| 2. WaitEx / FIFO / `009BEEB0`; no second swapchain | **MATCH.** Same `Draw` Present. FIFO on pump edge. |
| 3. Dest not this bug; no Lookout/Oakvale under logos | **PARTIAL.** Dest is #8/#11. No world submit during first-boot AVI, but the 3D path is still the Present. |

Tracker leftovers that stay true:

- `docs/status/README.md`: “leftover #20 is still 3D Draw during
  startup PlayAVI.”
- `proofs/audio-frontend/README.md`: “Leftover #20 is 3D Draw
  during AVI, not bind.”

---

## Leftover

1. `Program.cs` `window.Render` has no `StartupVideos` /
   `AviPlaying` gate; it always calls `host.Draw` or a debug
   `Renderer.Draw`.
2. `SilkEngineHost.Draw` always composes
   `ViewProjection` / `SkyViewProjection` /
   `HostLandscapeViewProjection` from the unseeded
   `ScriptedCamera` (origin + 72°).
3. `VulkanLineRenderer.Draw` is still the 3D Record/submit
   entry; there is no blit-only Present function.
4. `DrawFrontend` is outside `playAviOnly`.
5. Inter-slot `AviPlaying=false` turns the pump off for a
   clear frame; that frame is a 3D `Draw` with no video.
6. No test locks “no mesh/line/sky submit during
   `StartupVideos`.”

---

## Proposed next step

Keep one swapchain. Do not invent a second Present.

In `SilkEngineHost.Draw` (or `Program` Render), if
`LastFrame.AviPlaying` (or `Stage == StartupVideos` via a
flag on the frame), call a blit-only path: skip
`ViewProjection` / fog / sky / landscape arguments and skip
mesh bind. Either:

- pass `default` WVP and rely on existing `playAviOnly`, and
  also skip `DrawFrontend` when `_playAviPump`, or
- add `VulkanLineRenderer.PresentVideo()` that Records clear +
  video dest only (BeginScene / blit / EndScene / Present).

Hold `playAviOnly` true across the one inter-slot clear frame
(`PumpStartupAvi` already wants a clear, not 3D). Leave F2
debug-fly as a debug override, or ignore it during
`StartupVideos`.

Lock with a host/renderer spy or a `Record` hook:
`StartupVideos` + `AviPlaying` ⇒ `DrawMeshBatches` /
gizmo / fade / frontend not recorded. Do not close #20 on the
current `playAviOnly` comment alone.
