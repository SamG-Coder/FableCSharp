# Issue #19 vs HEAD: frozen `gameCam` after `BindLifecycleFirstRegion`

Investigation only. No production `src/` or `tests/` edits.

GitHub: [SamG-Coder/FableCSharp#19](https://github.com/SamG-Coder/FableCSharp/issues/19)
(open, 2026-08-18, 0 comments). Title: *TryWalk / F2 / Title still
use frozen gameCam after BindLifecycleFirstRegion*.

Authority: issue body; `src/Fable.Client/Program.cs` at **HEAD**
`ee084901e8212814d4ca7df599180117f9be5cec` (local `master`);
`src/Fable.Client/SilkEngineHost.cs`;
`src/Fable.Game/IEngineHost.cs` `EngineFrame` / `BuildFrame`;
`src/Fable.Game/EngineLifecycle.cs` `ApplyWorldCamera` /
`PresentToHost`; `src/Fable.Game/ScriptedCamera.cs`;
`src/Fable.Game/RegionTravel.cs` `HitExit`;
pre-fix client `fe6a11e7b06c3d8a6da871ad7f75fb8de41c6bca`
(`Program.cs` still had the issue snippet);
host refactor `d9977fb716bd43374e24558b71ca9fc0817426f3`.

Status words: **FIXED** / **STILL OPEN** / **PARTIAL** /
**PROVEN** / **DISPROVEN** / **LEFTOVER**.

Do **not** start at Oakvale / `CAM_OVIF_SHOT2` / `00DBDE40`.
That is later `Q_NewOakValeIntro`, not this host-camera split.
Not issue #6 (`SeedAt` 1.6 m) and not issue #12
(`EnterRegion` `64,-40,95` overview).

---

## Verdict

| Claim | vs HEAD | Class |
| --- | --- | --- |
| `Program.cs` still has `gameCam` / `BindLifecycleFirstRegion` / `CopyGameToDebug` / `TryWalk` / camera `Title()` | **No.** Those symbols are gone from `src/`. | **FIXED** |
| `window.Render` draws a one-shot bind snapshot while `life.Camera` keeps `ApplyWorldCamera` | **No.** Non-fly draw is `host.Draw` → `_frame.Camera` from `BuildFrame()` (`life.Camera`). | **FIXED** |
| F2 copies frozen `gameCam` | **No.** F2 copies `host.LastFrame.Camera`. | **FIXED** |
| Title / `HitExit` still probe `gameCam.Position` | **No.** Title is `life.WindowTitle`. Client does not call `HitExit`. | **FIXED** (hosts deleted) |
| Client `HitExit` now uses the same camera Render uses | **No such host.** `HitExit` is tests-only. F2 fly cannot walk a region. | **LEFTOVER** (not the frozen-cam bug) |
| Lookout `life.Camera` mixed with Oakvale `CAM_OVIF_SHOT2` / host `EnterRegion` | Host `EnterRegion` / SHOT2 bind removed from `Program.cs`. | **FIXED** for this issue |
| GitHub #19 closed | Still **open**. Ledger still says “leave #19 open”. | **LEFTOVER** docs / issue |

**Status vs HEAD: FIXED.**

The named second camera is gone. Later pumps still write one
`ScriptedCamera` (`006B42F0` / `ApplyWorldCamera`). Present and
F2 seed from that object. Do not reopen #6 / #12.

---

## 1. What #19 filed (still true of `fe6a11e`, not HEAD)

Issue body (2026-08-18, after `fe6a11e`, before `d9977fb`):

After no-save New Game, `window.Render` used the lifecycle camera:

```csharp
var cam = life.Stage == EngineStage.Game && life.HeroSpawned
    ? life.Camera
    : gameCam;
```

`BindLifecycleFirstRegion` copied `life.Camera` onto `gameCam`
**once**. Later pumps can `ApplyManagerOutput` into `life.Camera`.
These hosts kept the snapshot:

```csharp
void CopyGameToDebug()
{
    debugCam.Position = gameCam.Position;
    debugCam.FovDegrees = gameCam.FovDegrees;
    debugCam.LookAt(gameCam.LookAt);
}

// Title()
var pos = debugFly ? debugCam.Position : gameCam.Position;

// TryWalk()
var hit = RegionTravel.HitExit(exits, gameCam.Position);
```

That text **MATCH**es `fe6a11e` `src/Fable.Client/Program.cs`
(`BindLifecycleFirstRegion` `gameCam.Bind(...)`, `CopyGameToDebug`,
`Title()`, `TryWalk()`, Render ternary). **PROVEN** as the bug at
file time.

Done looked like:

1. Drive Title / F2 copy / `HitExit` from the same camera
   `window.Render` uses (`life.Camera` after hero spawn,
   `debugCam` while flying).
2. Stop treating the one-shot `gameCam.Bind` in
   `BindLifecycleFirstRegion` as a live camera.
3. Keep Lookout `life.Camera` and Oakvale `CAM_OVIF_SHOT2` /
   `EnterRegion` as separate paths.

---

## 2. HEAD `Program.cs` — quoted

`src/Fable.Client/Program.cs` at `ee08490` (157 lines). No
`gameCam`, `BindLifecycleFirstRegion`, `CopyGameToDebug`,
`TryWalk`, `EnterRegion`, or camera `Title()`.

F2 seeds from the last Present camera, not a bind snapshot:

```89:97:src/Fable.Client/Program.cs
    var f2Down = keyboard.IsKeyPressed(Key.F2);
    if (f2Down && !f2WasDown)
    {
        debugFly = !debugFly;
        var cam = host.LastFrame.Camera;
        debugCam.Position = cam.Position;
        debugCam.FovDegrees = cam.FovDegrees;
        debugCam.LookAt(cam.LookAt);
    }
```

Title is the native window string (`004023F0` /
`TEXT_GUI_WINDOW_TITLE`), not `gameCam.Position`:

```126:130:src/Fable.Client/Program.cs
    host.Width = window.FramebufferSize.X;
    host.Height = window.FramebufferSize.Y;
    if (!life.Pump((float)dt) || life.Stage == EngineStage.Shutdown)
        window.Close();
    window.Title = life.WindowTitle;
```

Render: fly uses `debugCam`; otherwise the host Present camera:

```133:148:src/Fable.Client/Program.cs
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

Workspace `rg` over `src/` for `gameCam` /
`BindLifecycleFirstRegion` / `CopyGameToDebug` / `TryWalk`:
**0 hits**. **PROVEN** removal.

The cut that deleted the second camera is
`d9977fb` (*Client is IEngineHost; Pump owns AVI, New Game,
world submit.*). Same F2 / Title / Draw shape is already in that
commit’s `Program.cs`.

---

## 3. What Render / F2 actually see

`Pump` → `PresentToHost` → `BuildFrame()` always passes
`EngineLifecycle.Camera` (class, not a pose struct):

```3270:3287:src/Fable.Game/EngineLifecycle.cs
    private void PresentToHost()
    {
        Host?.Present(BuildFrame());
    }

    public EngineFrame BuildFrame()
    {
        var avi = StartupAvi;
        var runtime = Runtime;
        var playing = avi is { Rgba: not null } ||
                      runtime is { AviPlaying: true, AviRgba: not null };
        var fade = runtime?.FadeColor ?? default;
        var present = PresentDestFromViewport(
            ViewportX, ViewportY, ViewportWidth, ViewportHeight,
            BackBufferWidth, BackBufferHeight);
        return new EngineFrame(
            Camera,
```

`SilkEngineHost.Present` stores that frame. `Draw` uses it:

```177:205:src/Fable.Client/SilkEngineHost.cs
    /// <summary>
    /// Draw the last Present using the
    /// engine camera from that frame.
    /// </summary>
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

        // 00B30B50 letterbox uses camera +176/+180
        // = 1024×768. Window resize is not the
        // first-seen viewport.
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

`ApplyWorldCamera` (`0049E080` / `006B42F0`) writes that same
`Camera` (`ApplyRendererHelper` or `ApplyManagerOutput`). There
is no leftover host `gameCam.Bind` sitting beside it.

`ScriptedCamera` comment at HEAD still states the issue’s
intent: *“This object is the game camera — debug fly must not
write it.”* F2 only writes `debugCam`. **MATCH**.

---

## 4. Done-list vs HEAD

| Done item | HEAD |
| --- | --- |
| 1. Title from same camera as Render | Title is no longer a debug pose string. `window.Title = life.WindowTitle`. The old `Title()` `gameCam.Position` path is **DISPROVEN** as live. |
| 1. F2 copy from same camera as Render | **PROVEN.** `host.LastFrame.Camera` is the Present `ScriptedCamera`. |
| 1. `HitExit` from same camera as Render | **LEFTOVER.** Client has no `TryWalk`. `HitExit` remains in `RegionTravel` and `WorldSceneTests` only. |
| 2. Stop treating one-shot `gameCam.Bind` as live | **PROVEN.** Symbol gone. |
| 3. Lookout vs Oakvale `EnterRegion` / SHOT2 | **PROVEN** for the client: host `EnterRegion` / `UseCamera(CAM_OVIF_SHOT2)` no longer in `Program.cs`. Engine no-save Present stays Lookout `006B3FF0`. |

Item 1’s `HitExit` clause was **deleted**, not rewired.
That is not “still using frozen `gameCam`”. It is “client
no longer walks exits”. Native region travel is not F2 fly.

---

## 5. Leftover (do not confuse with STILL OPEN)

- GitHub #19 still **open**. No close comment.
- `docs/status/README.md` / `docs/status/index.html` still
  say “leave #19 open” on the PALSKIN `27cb7ee` row (that
  commit is **before** `d9977fb`). Stale ledger.
- Same status table still names client
  `BindLifecycleFirstRegion` as if it existed (`fe6a11e`
  Lookout row). That helper is **DISPROVEN** as live HEAD.
- `EngineLifecycleTests` comment still says
  “Client BindLifecycleFirstRegion builds”. Comment only.
- F2 `FlyCamera` is a host overlay. It does not write
  `life.Camera`. Same as the issue’s native contrast
  (one game camera `+6296/+6312/+6328`).
- `RegionTravel.HitExit` is unused on the live client path.
  Re-adding host `TryWalk` that probes `debugCam` would be a
  **new** debug travel feature, not a reopen of this bug.

---

## 6. Proposed next step

1. Close #19 with this proof (HEAD `ee08490`, cut `d9977fb`).
   Do not implement another host `gameCam`.
2. Optionally strike “leave #19 open” and the live
   `BindLifecycleFirstRegion` wording from
   `docs/status/README.md`.
3. Do **not** restore client `TryWalk` / `HitExit` to
   “finish” done-item 1. If fly-to-exit is wanted, file a
   **new** debug-only issue and probe `debugCam` /
   `host.LastFrame.Camera` — never a second bind snapshot.
   Native travel stays engine (`00501450` / map open), not
   host `EnterRegion` `64,-40,95` (#12) or `SeedAt` (#6).
