# Leftovers #9 and #20 vs current `src/`

Evidence-only. Tracker rows in `docs/status/README.md` and
`proofs/issue-9-verify` / `proofs/issue-20-verify` are **stale**
against live code. Do not reopen closed items as new work.

Status words: **OPEN** / **CLOSED** / **PARTIAL**.

| Leftover | Filed claim | Status |
|---|---|---|
| **#9** | `WmvPlayer` never QIs `IBasicAudio` (native `00A3B9D0` does) | **CLOSED** |
| **#20** | 3D Draw during startup PlayAVI | **CLOSED** |

---

## #9 CLOSED — `IBasicAudio` QI + `put_Volume(0)`

Native `00A3B9D0` after `RenderFile`: QI `IBasicAudio`
`0x12AA054` then `put_Volume(0)` (DirectShow 0 = 0 dB).

Live `BuildGraph` now does that. The old “comment names
BasicAudio, next statements do not QI” snippet is gone.

### Live player

`LastBasicAudioQi` / `LastBasicAudioVolume` are public
counters. `TryOpen` clears them before STA build:

```146:147:src/Fable.Game/WmvPlayer.cs
    public static bool LastBasicAudioQi { get; private set; }
    public static int LastBasicAudioVolume { get; private set; }
```

```187:188:src/Fable.Game/WmvPlayer.cs
        LastBasicAudioQi = false;
        LastBasicAudioVolume = int.MinValue;
```

Field + COM IID (same GUID as `RegionTravel.PlayAviBasicAudioIid`):

```158:158:src/Fable.Game/WmvPlayer.cs
    private IBasicAudio? _audio;
```

```791:800:src/Fable.Game/WmvPlayer.cs
    [ComImport]
    [Guid("56a868b3-0ad4-11ce-b03a-0020af0ba770")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IBasicAudio
    {
        void GetTypeInfoCount(out int count);
        void GetTypeInfo(int itinfo, int lcid, out IntPtr info);
        void GetIDsOfNames(ref Guid iid, IntPtr names, int count, int lcid, IntPtr dispIds);
        void Invoke(int dispId, ref Guid iid, int lcid, short flags, IntPtr dispParams, IntPtr result, IntPtr excep, IntPtr argErr);
        [PreserveSig] int put_Volume(int volume);
```

After `RenderFile`, RCW QI of the graph then `put_Volume(0)`.
`LastBasicAudioQi` is set only when `put_Volume` returns `>= 0`.
TearDown drops `_audio`.

```383:403:src/Fable.Game/WmvPlayer.cs
        _control = (IMediaControl)_graph;
        _position = (IMediaPosition)_graph;
        _events = (IMediaEvent)_graph;
        // 00A3B9D0 QI IBasicAudio 0x12AA054 then
        // put_Volume(0) = 0 dB. Not a WAV mixer.
        LastBasicAudioQi = false;
        LastBasicAudioVolume = int.MinValue;
        try
        {
            _audio = (IBasicAudio)_graph;
            var volHr = _audio.put_Volume(0);
            if (volHr >= 0)
            {
                LastBasicAudioQi = true;
                LastBasicAudioVolume = 0;
            }
        }
        catch
        {
            _audio = null;
        }
```

```645:645:src/Fable.Game/WmvPlayer.cs
        _audio = null;
```

IID lock (not a live QI by itself):

```454:455:src/Fable.Game/RegionTravel.cs
    public static readonly Guid PlayAviBasicAudioIid =
        new("56a868b3-0ad4-11ce-b03a-0020af0ba770");
```

```517:517:src/Fable.Game/RegionTravel.cs
    public const uint PlayAviBasicAudioIidVa = 0x012AA054;
```

`WmvPlayer` does not read the `RegionTravel.PlayAviBasicAudioIid`
symbol. The ComImport GUID is the same IID. That is not leftover
#9.

### Tests (PlayAVI)

`WorldSceneTests` locks the IID **and** a live open:

```390:391:tests/Fable.Formats.Tests/WorldSceneTests.cs
        Assert.Equal(new Guid("56a868b3-0ad4-11ce-b03a-0020af0ba770"), RegionTravel.PlayAviBasicAudioIid);
        Assert.Equal(0x012AA054u, RegionTravel.PlayAviBasicAudioIidVa);
```

`PlayAvi_rewrites_xmv_to_installed_wmv_and_blocks` opens the
installed intro WMV, then starts New Game `PlayAVI` and asserts
QI + volume:

```1941:1954:tests/Fable.Formats.Tests/WorldSceneTests.cs
    public void PlayAvi_rewrites_xmv_to_installed_wmv_and_blocks()
    {
        var install = Require();
        var relative = RegionTravel.PlayAviPrefix + RegionTravel.IntroPlayAvi;
        // ...
        using var player = WmvPlayer.TryOpen(file);
```

```2066:2068:tests/Fable.Formats.Tests/WorldSceneTests.cs
        Assert.True(runtime.AviPlaying, WmvPlayer.LastError ?? "PlayAVI open failed");
        Assert.True(WmvPlayer.LastBasicAudioQi, WmvPlayer.LastError ?? "IBasicAudio QI missed");
        Assert.Equal(0, WmvPlayer.LastBasicAudioVolume);
```

Sibling PlayAVI tests in the same file
(`PlayAVI_00628B79_resizes_to_viewport_width_and_centers`,
`PlayAvi_from_exe_matches_no_clock_receive`,
`PlayAvi_timeline_wait_names_match_WaitEx`) do not re-assert
BasicAudio. Do not file that as #9.

### Not #9

- `IMediaSeeking` still has no live graph QI.
- Class summary at `WmvPlayer.cs` 7–19 still lists Control /
  Position / Event and omits BasicAudio. Implementation
  contradicts the summary.
- `docs/status/README.md` 424 still says PARTIAL issue #9.

Those are docs / other QI, not “never QIs `IBasicAudio`.”

---

## #20 CLOSED — startup PlayAVI Present is not 3D WVP

Filed leftover: client `window.Render` always composed a 3D
camera WVP during `EngineStage.StartupVideos` (origin + 72°
`gameCam`). Native `006286F0` is WaitEx → BeginScene → blit →
EndScene → Present. No `00A44880` 3D.

Current client does **not** pass a 3D WVP while that stage
owns the pump. `Record` skips mesh / gizmo / fade / frontend
while `playAviOnly`. F2 fly is after the stage gate.

### Client Present (`Program.cs`)

```160:184:src/Fable.Client/Program.cs
window.Render += _ =>
{
    if (window.FramebufferSize.X == 0 || host.Renderer is null)
        return;
    var aspect = window.FramebufferSize.X / (float)window.FramebufferSize.Y;
    // 006286F0 blit+Present only. No 3D,
    // no frontend, no F2 fly under logos.
    if (life.Stage == EngineStage.StartupVideos)
        host.Renderer.Draw(default);
    else if (debugFly)
    {
        var fog = Fable.Formats.WorldShading.LinearFogPlane(debugCam.Position, debugCam.Forward);
        host.Renderer.Draw(
            debugCam.ViewProjection(aspect), debugCam.Position, fog,
            debugCam.SkyViewProjection(aspect),
            debugCam.HostLandscapeViewProjection(aspect));
    }
    // NativeSemantic Device.Present already
    // consumed the swapchain. Shadow and
    // Compatibility still need host.Draw.
    else if (life.Dx9OwnsFrontendPresent)
        return;
    else
        host.Draw(aspect);
};
```

`Draw(default)` is still `VulkanLineRenderer.Draw`. That is the
swapchain Present used for the video dest, not a 3D camera
submit. Do not reopen “calls `Draw`” as #20.

Skip-AVI env is `FinishStartupVideo` only (`Program.cs` 65–69).
Not a 3D Draw path.

### Host (`SilkEngineHost`)

Present arms the AVI pump and drops the frontend batch:

```62:87:src/Fable.Client/SilkEngineHost.cs
            if (frame is { AviPlaying: true, AviRgba: not null })
            {
                if (frame.AviWidth != _aviWidth || frame.AviHeight != _aviHeight)
                    renderer.ClearVideoFrame();
                _aviWidth = frame.AviWidth;
                _aviHeight = frame.AviHeight;
                // 009BEDC0: dest uses the presented
                // framebuffer, not a stale 1024×768
                // host size or the world camera.
                var fbW = renderer.FramebufferWidth > 0
                    ? renderer.FramebufferWidth
                    : Width;
                var fbH = renderer.FramebufferHeight > 0
                    ? renderer.FramebufferHeight
                    : Height;
                var dest = RegionTravel.PlayAviLetterbox(
                    frame.AviWidth, frame.AviHeight, fbW, fbH);
                renderer.VideoClearColor =
                    Dx9VulkanColor.FromD3dArgb(frame.AviClearArgb);
                renderer.SetVideoFrame(
                    frame.AviWidth, frame.AviHeight, frame.AviRgba,
                    new Vector4(dest.X0, dest.Y0, dest.X1, dest.Y1),
                    frame.AviSerial);
                VulkanLineRenderer.NoteReceived(frame.AviSerial);
                renderer.SetPlayAviPump(true);
                renderer.SetFrontendBatch(null);
            }
```

`Draw` refuses 3D WVP while `AviPlaying`:

```186:191:src/Fable.Client/SilkEngineHost.cs
        var cam = _frame.Camera;
        // 006286F0 does not compose a 3D WVP.
        if (cam is null || _frame.AviPlaying)
        {
            Renderer.Draw(default);
            return;
        }
```

Startup client uses `Program` `Stage == StartupVideos` and does
not enter `host.Draw`. The host gate still covers script
`PlayAVI` if something calls `host.Draw` while `AviPlaying`.

`Present` can still `SetMesh` after the AVI branch when the
frame carries vertices (`SilkEngineHost.cs` 112–149). `Record`
does not submit those batches while `playAviOnly`. Not a 3D
Draw under logos. Do not file as new work.

### Renderer (`playAviOnly`)

Pump flag:

```423:429:src/Fable.Render/VulkanLineRenderer.cs
    public void SetPlayAviPump(bool on)
    {
        if (_playAviPump == on)
            return;
        _playAviPump = on;
        _resized = true;
    }
```

```1285:1346:src/Fable.Render/VulkanLineRenderer.cs
        // 006286F0 BeginScene/blit/EndScene/Present
        // does not draw landscape or fade.
        // 0042DF9E frontend Present is 2D
        // (009D9C80 / 009DA9F0). World mesh
        // and host gizmos are 00435530.
        var playAviOnly = _playAviPump ||
            (_videoReady && _videoPipeline.Handle != 0 && _videoTexture.Set.Handle != 0);
        var frontendOnly = _frontendReady && !playAviOnly && !_dx9PresentFrame;

        if (!playAviOnly && !frontendOnly && !_dx9PresentFrame &&
            ((_meshCount > 0 && _meshBuffer.Handle != 0) ||
             (_objectCount > 0 && _objectBuffer.Handle != 0)))
        {
            // ... DrawMeshBatches ...
        }

        if (!playAviOnly && !frontendOnly && !_dx9PresentFrame &&
            ShowGizmos && _vertexCount > 0 && _vertexBuffer.Handle != 0)
        {
            // ... line gizmos ...
        }

        if (!playAviOnly && FadeOverlayAlpha > 0 && _overlayPipeline.Handle != 0)
        {
            // ... fade overlay ...
        }

        if (!playAviOnly)
            DrawFrontend(commandBuffer);

        if (_videoReady && _videoPipeline.Handle != 0 && _videoTexture.Set.Handle != 0)
        {
```

`DrawFrontend` is inside `!playAviOnly`. The
`proofs/issue-20-verify` leftover “`DrawFrontend` is outside
`playAviOnly`” is **DISPROVEN**.

Video dest still records after that skip. That is the blit.

### Inter-slot (observation, not a reopen)

`PumpStartupAvi` unloads then Presents `AviPlaying=false` so
the host clears leftover rows:

```3744:3749:src/Fable.Game/EngineLifecycle.cs
        UnloadStartupAvi();
        if (Stage == EngineStage.StartupVideos)
            FinishStartupVideo();
        // Next Pump opens the next file. This
        // Present is AviPlaying=false so the
        // host clears the previous AVI first.
```

That Present hits `SilkEngineHost` else (`SetPlayAviPump(false)`,
`ClearVideoFrame`). `Stage` is still `StartupVideos`, so
`Program` still `Draw(default)`. `playAviOnly` is false on that
one clear frame unless `_videoReady` is still set.
`ClearVideoFrame` sets `_videoReady = false`
(`VulkanLineRenderer.Textures.cs` 454–459).

Cold-start startup has not submitted Lookout, so
`_meshCount` is still 0. That is the #11 clear frame, not
the filed 3D-under-logos Draw. Do not reopen #20.

### Tests

No test asserts `Program.cs` `Draw(default)` or
`playAviOnly`. PlayAVI tests lock dest / graph / QI:

| Test | File | What it locks |
|---|---|---|
| `PlayAvi_rewrites_xmv_to_installed_wmv_and_blocks` | `WorldSceneTests.cs` 1941 | WMV open, `AviPlaying`, `#9` QI |
| `PlayAVI_00628B79_resizes_to_viewport_width_and_centers` | `WorldSceneTests.cs` 2085 | dest #11, not 3D |
| `PlayAvi_from_exe_matches_no_clock_receive` | `WorldSceneTests.cs` 2114 | Receive / WaitEx |
| `PlayAvi_timeline_wait_names_match_WaitEx` | `WorldSceneTests.cs` 2167 | timeline names |
| `EngineLifecycle.StartupVideos` table | `EngineLifecycleTests.cs` 306–321 | three slots, sizes, clear ARGB |

Missing a `Record` spy is not leftover #20. The client 3D
WVP path the issue named is gone.

---

## Tracker rows that stay stale

Do not treat these as current status:

- `docs/status/README.md` 423–424, 430–431, 456–457: #20 and
  #9 still PARTIAL.
- `proofs/issue-9-verify/README.md`: “never QIs”.
- `proofs/issue-20-verify/README.md`: “always calls `Draw` with
  3D WVP”; “`DrawFrontend` outside `playAviOnly`”; “F2 during
  logos”.
- `proofs/audio-frontend/README.md` 83: “no `QueryInterface` /
  `put_Volume`”.

---

## Verdict

**#9 CLOSED.** Live `WmvPlayer.BuildGraph` QIs `IBasicAudio`
and `put_Volume(0)`. `PlayAvi_rewrites_xmv_to_installed_wmv_and_blocks`
asserts `LastBasicAudioQi` and volume 0.

**#20 CLOSED.** `StartupVideos` Present is `Draw(default)`.
`AviPlaying` host Draw is `Draw(default)`. `playAviOnly` skips
mesh / gizmo / fade / frontend. F2 does not run under logos.
Unload-before-next-slot stays recovered (not this leftover).
