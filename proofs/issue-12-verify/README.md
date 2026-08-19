# Verify GitHub issue #12 vs HEAD

Investigation only. No production `src/` or `tests/` edits.

Issue: [EnterRegion still invents 64,-40,95 overview and 8 m look-ahead (not SeedAt) #12](https://github.com/SamG-Coder/FableCSharp/issues/12)

HEAD: `ee084901e8212814d4ca7df599180117f9be5cec` (`refs/heads/master`).

Do **not** invent a replacement overview, eye height, or look-ahead
distance. Do **not** collapse this into `SeedAt(1.6 m)` (#6) or
Oakvale `CAM_OVIF_SHOT2`.

Status words: **FIXED** / **STILL OPEN** / **PARTIAL** / **PROVEN** /
**DISPROVEN** / **LEFTOVER** / **UNREAD**.

---

## Verdict vs HEAD

**FIXED.** The client `EnterRegion` / unused `startPosition` /
`startLook` overview is gone. `Program.cs` does not bind
`(64,-40,95)` / `(64,64,36)`, does not walk
`PlayerHeight * 0.5` / spawn `* 8f`, and does not own the
live camera.

Issue #12 is a **second, older invented camera** (explicitly not
`SpawnHero` `SeedAt`). At HEAD the host is `IEngineHost`:
`Pump` owns region, world submit, and camera; the window only
queues input and Presents the last `EngineFrame.Camera`.

| Issue claim | vs HEAD | Class |
|---|---|---|
| `Program.cs` keeps unused `startPosition = (64,-40,95)` / `startLook = (64,64,36)` | symbols absent | **DISPROVEN** on HEAD |
| `EnterRegion` applies those locals, or half-mesh-height / 8 m stand-in, when the walk has no TNG helper | `EnterRegion` absent; no `PlayerHeight * 0.5` | **DISPROVEN** on HEAD |
| No-spawn fallthrough to the same overview | no client region walk | **DISPROVEN** on HEAD |
| Frontend / pre-bind `window.Render` draws `gameCam` at origin with Oakvale intro FOV | no `gameCam`; `host.Draw` uses `_frame.Camera` | **DISPROVEN** as filed |
| This is not #6 `SeedAt` | live New Game still `SeedHero` / `ComputePose`; `SeedAt` tests-only | **PROVEN** (keep #6 closed) |
| Walk-in cameras are TNG helpers, not a host overview | live first Present is Lookout `006B3FF0`, not overview / SHOT2 | **PROVEN** |

Done-list from the issue:

1. Delete the `64,-40,95` / `64,64,36` locals or stop drawing them — **done** in `Program.cs`.
2. Do not apply `PlayerHeight * 0.5` / `* 8f` on the **walk path** until those offsets are read — **done** (walk path gone).
3. Do not feed this overview into `BindLifecycleFirstRegion` — method **absent** from `src/`.

---

## 1. `src/Fable.Client/Program.cs` — no `startPosition` / `startLook` / `EnterRegion`

Whole file is a Silk window + `EngineLifecycle` pump. Camera
locals are only the **debug** fly cam. No region enter.

Load / input / Present:

```47:66:src/Fable.Client/Program.cs
window.Load += () =>
{
    if (window.VkSurface is null)
        throw new NotSupportedException("This window backend cannot create a Vulkan surface.");

    host.Renderer = new VulkanLineRenderer(window);
    input = window.CreateInput();
    mouse = input.Mice.Count > 0 ? input.Mice[0] : null;
    // ...
    Console.WriteLine($"{install.Edition}: {install.Root}");
    Console.WriteLine($"lifecycle {life.Stage} pe=0x{EngineLifecycle.PeEntry:X8}");
};
```

F2 copies the **engine** frame camera into debug fly. It does
not invent an overview:

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

Render: engine `host.Draw`, unless F2 fly is on.

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

Grep on `src/` + `Program.cs`: **zero** hits for `EnterRegion`,
`startPosition`, `startLook`, `BindLifecycleFirstRegion`,
`64f, -40f, 95f`.

`SilkEngineHost` is Present-only. It does not enter a region:

```7:14:src/Fable.Client/SilkEngineHost.cs
/// Silk / Vulkan Present adapter for
/// <c>009BEEB0</c>. The engine already
/// chose camera, world, and AVI.
```

```50:54:src/Fable.Client/SilkEngineHost.cs
    /// <c>009BEEB0</c> Present. Does not
    /// expand, enter a region, or start
    /// New Game.
```

Draw consumes `_frame.Camera` from that Present:

```177:205:src/Fable.Client/SilkEngineHost.cs
    /// Draw the last Present using the
    /// engine camera from that frame.
    public void Draw(float aspect)
    {
        // ...
        var cam = _frame.Camera;
        // ...
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

`IEngineHost` contract:

```3:20:src/Fable.Game/IEngineHost.cs
/// Window / input / Present surface the
/// PE header implements. The engine
/// owns modes, load, AVI, camera, and
/// world submit. The host does not
/// decide New Game, region, or expand.
```

---

## 2. Live camera after Leave is not the overview (and not `SeedAt`)

`EngineLifecycle.Camera` is a `ScriptedCamera` filled from
WorldCamera apply, not from client locals:

```2523:2528:src/Fable.Game/EngineLifecycle.cs
    /// Same camera the renderer consumes.
    /// Filled by <c>006B42F0</c> from
    /// <see cref="WorldCamera"/>.
    public ScriptedCamera Camera { get; } = new();
```

Ctor FOV is leftover SHOT2 72° (`IntroCameraFovDegrees`).
Frontend Present is 2D / AVI; first 3D apply is
`ApplyWorldCamera` after WorldFrame>1. That path is **not**
`EnterRegion` and **not** `SeedAt`:

```5500:5540:src/Fable.Game/EngineLifecycle.cs
    public void ApplyWorldCamera(float tBlend)
    {
        // ...
        if (!WorldCamera.Seeded)
            Note(WorldCameraSeedFn, "GamePump", "Camera", "006B3FF0 +68");
        var output = WorldCamera.Blend(tBlend);
        // 00B314E0 copies helper +0/+12/+24
        // as eye / forward / up ...
        if (WorldCamera.IsCtorAxis(output.V0) &&
            Hero is { PositionX: not null, PositionY: not null })
        {
            var eye = RegionTravel.PositionOf(Hero);
            var forward = output.V4.LengthSquared() > 1e-8f
                ? output.V4
                : WorldCamera.SlotA.V4;
            if (forward.LengthSquared() < 1e-8f)
                forward = -Vector3.UnitX;
            Camera.ApplyRendererHelper(
                eye, forward, LandscapeFrustum.FirstSeenCameraUp);
            Camera.SetFovDegrees(GameCamera.FirstSeenFovDegrees);
            RendererHelperBound = true;
        }
        else
        {
            Camera.ApplyManagerOutput(output.V0, output.V1, output.V2);
            RendererHelperBound = false;
        }
    }
```

Live seed is `SeedHero` → `ComputePose` (`006B2CA0`), not
`SeedAt`:

```137:152:src/Fable.Game/WorldCamera.cs
    /// <c>006B3FF0</c> when <c>+68==0</c>.
    /// ... Does not invent
    /// a 1.6 m eye.
    public void SeedHero()
    {
        ComputePose();
        SlotB = SlotA;
        Seeded = true;
    }
```

`SeedAt` remains a leftover method. Production
`EngineLifecycle` never calls it. The only `cam.SeedAt(` in
the tree is the test helper in
`World_camera_006B4900_slots_lerp_into_ScriptedCamera`
(dummy `(4,5,6)` / `(7,8,9)`, not 1.6 m and not overview).

Do **not** reopen #6.

---

## 3. Leftover (do not treat as #12 still open)

These are **not** the filed `Program.cs` `EnterRegion` path.
Do not restore overview numbers to “fix” them.

### 3.1 TNG helper look-at still uses host `* 8f`

`RegionTravel.TryCameraFromThing` (named / intro TNG cameras,
including leftover Oakvale `UseCamera`) still writes:

```1254:1274:src/Fable.Game/RegionTravel.cs
    public static bool TryCameraFromThing(
        ThingInstance thing,
        out Vector3 position,
        out Vector3 lookAt,
        out float fovDegrees,
        out Vector3 up)
    {
        position = PositionOf(thing);
        lookAt = default;
        fovDegrees = IntroCameraFovDegrees;
        up = IntroCameraUp(thing);
        var look = Vector3.UnitY;
        if (TryCoord(thing, "CTCCameraPointScriptedSpline.KeyCameras[0].Position", out var keyPos))
            position += keyPos;
        if (TryCoord(thing, "CTCCameraPointScriptedSpline.KeyCameras[0].LookDirection", out var keyLook) ||
            TryLook(thing, "CTCCameraPointScripted.LookDirection", out keyLook))
            look = keyLook;
        if (look.LengthSquared() < 1e-8f)
            look = Vector3.UnitY;
        look = Vector3.Normalize(look);
        lookAt = position + look * 8f;
```

That is **not** `EnterRegion` spawn forward × 8, **not**
`PlayerHeight * 0.5`, and **not** `SeedAt`. It is the host
TNG → `LookAt` stand-in. Native helper pack for that bind is
still **UNREAD** / **PARTIAL** (`proofs/script-camera-cmd`:
“`LookAt = position + normalised look × 8` is host
`TryCameraFromThing`”).

Callers: `TryIntroCamera` / `TryNamedCamera` →
`ScriptedCamera.UseCamera` / leftover `FirstSceneWorld`
SHOT2. First no-save Present does **not** take this path
(`FirstSeenCallsUseCamera=false`).

Do **not** pick a new distance. Recover the native helper
pack, or leave the leftover marked.

### 3.2 `(64,-40,95)` lives only in projection tests

Not client, not lifecycle:

```13:16:tests/Fable.Formats.Tests/CameraProjectionTests.cs
    public void Lookout_center_is_in_front_of_south_overview_camera()
    {
        var camera = new FlyCamera { Position = new Vector3(64f, -40f, 95f) };
        camera.LookAt(new Vector3(64f, 64f, 36f));
```

Sky tests still **reject** an invented origin at `(64,64,0)`
(`First_seen_sky_dome_is_6500_by_3250_ellipsoid`,
`First_seen_star_draw_does_not_emit_stars_dat_billboards`).
That family is not the live client camera.

### 3.3 Stale docs: `BindLifecycleFirstRegion`

`docs/status/README.md` still says client
`BindLifecycleFirstRegion` skips StartOakVale (`fe6a11e`).
`EngineLifecycleTests` still names it in a recovered-goal
string. **No** `src/` method. Ledger leftover, not a live
overview bind.

---

## Proposed next step

1. **Close #12** vs HEAD `ee08490`. The filed client
   `EnterRegion` / overview locals are gone. Do not restore
   `(64,-40,95)` / `(64,64,36)` or walk-path half-height / 8 m.
2. Keep #6 closed: live seed is `006B3FF0` / `006B2CA0`, not
   `SeedAt`.
3. If TNG look-at remains a worry, treat
   `RegionTravel.TryCameraFromThing` `look * 8f` as a **new /
   existing leftover** (named-camera helper pack **UNREAD**),
   not as reopening #12. Do not invent a replacement offset.
4. Optional doc scrub: drop `BindLifecycleFirstRegion` from
   `docs/status/README.md` so it cannot be mistaken for a
   live client camera.
