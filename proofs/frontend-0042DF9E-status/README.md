# `0042DF9E` Present: Note-only vs live submit

Evidence-only. No `src/` or `tests/` edits.

Question: is frontend Present `0042DF9E` still Note-only, or does
it now execute DX9 submit?

Authority: `src/Fable.Game/EngineLifecycle.cs`
(`Pump`, `PumpFrontendFrame`, `IssueFrontendFramePresent`,
`FlushFrontendDisplay`, `CompositeFrontendPresent`);
`src/Fable.Game/FrontendDx9Submit.cs`;
`src/Fable.Game/Dx9SubmitOwnership.cs`;
`src/Fable.Render/VulkanDx9Device.cs`;
`src/Fable.Client/Program.cs`;
`tests/Fable.Formats.Tests/Dx9DeviceRecordTests.cs`
(`Native_semantic_*`);
`tests/Fable.Formats.Tests/Dx9ArchitectureTests.cs`
(`Native_semantic_*`);
`tests/Fable.Formats.Tests/EngineLifecycleTests.cs`
(`Frontend_0042EC7C_frame_is_input_then_0042DF9E_Present`);
`docs/status/README.md` (stale “still Note-only” rows).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **STALE**.

---

## Verdict

**Live submit** when `Device` is attached. Not Note-only.

`PumpFrontendFrame` still `Note`s `0042DF9E`, then
`IssueFrontendFramePresent` issues Clear / BeginScene /
recovered sprite DIPUP + glyph UP / EndScene / Present on
`IDirect3DDevice9`. NativeSemantic (sprites+glyphs) owns the
swapchain and skips `IEngineHost.Present`.

`009DA9F0(1)` ×2 is still Note-only empty skip
(`DisplayFlushShouldDip(0, 0)`). That is not “no UI draws”:
UI is `00BAE2D0` / `00AB7C20`, not `+16020`.

No-device `Bootstrap(null)` stays Note-only for the Present
body (`IssueFrontendFramePresent` returns). Ledger rows that
say “Present `0042DF9E` still Note-only” are **STALE**.

| Claim | Status |
| --- | --- |
| `0042EC7C` frame is input `0042E3EE` then draw `0042DF9E` | **PROVEN** |
| `0042DF9E` is only a `Note` (no device ops) | **DISPROVEN** when `Device` is set |
| `0042DF9E` VA is still traced via `Note` | **PROVEN** (wrapper remains) |
| Device Present is Clear → BeginScene → recovered UP draws → EndScene → Present | **PROVEN** |
| NativeSemantic skips host Present and `FrontendBatch` | **PROVEN** |
| Shadow still records the same device calls and host-Presents `FrontendBatch` | **PROVEN** |
| `009DA9F0(1)` twice is live DIP of `+16020` | **DISPROVEN** (Note + empty skip) |
| Live client (`Program.cs`) is NativeSemantic sprites+glyphs | **PROVEN** |

---

## 1. `0042EC7C` → `PumpFrontendFrame` → `0042DF9E`

Retail pump step on frontend:

```3494:3508:src/Fable.Game/EngineLifecycle.cs
        if (Stage == EngineStage.Frontend)
        {
            UnloadStartupAvi();
            if (!FrontendUiPresent)
                InitFrontendUi();
            PumpFrontendFrame();
            if (RetailNewGameFlag)
            {
                RequestNewGame();
                EnterGame();
            }

            if (!Dx9OwnsFrontendPresent)
                PresentToHost();
            return true;
        }
```

Named VA:

```357:357:src/Fable.Game/EngineLifecycle.cs
    public const uint FrontendDrawFn = 0x0042DF9E;
```

Frame body still Notes the listing, then walks UI, flushes
twice, then issues the device Present:

```4034:4074:src/Fable.Game/EngineLifecycle.cs
    public void PumpFrontendFrame()
    {
        Note(FrontendInputFn, "Frontend", "Input",
            "0042E3EE walk [0x13B8388]");
        // ...
        PumpInput();
        MaybeActivateNewGameFromInput();
        Note(FrontendUpdateFn, "Frontend", "UI", "0042DC94");
        // ...
        Note(FrontendDrawFn, "Frontend", "Render", "0042DF9E");
        Note(ClearColorFn, "Frontend", "D3D9", "009D8CF0 clear");
        Note(BeginSceneFn, "Frontend", "D3D9", "009BEF20 BeginScene");
        Note(FrontendUiGet, "Frontend", "UI", "00595582");
        TickFrontendWidgets();
        DrawFrontendWidgets();
        Note(InputActionGetter, "Frontend", "Input", "0041E5F2");
        FlushFrontendDisplay();
        Note(FrontendDisplayHelperFn, "Frontend", "D3D9",
            $"00404A80 0x{FrontendDisplaySingletonVa:X}");
        ApplyFrontendDisplay();
        FlushFrontendDisplay();
        Note(EndSceneFn, "Frontend", "D3D9", "009BEF50 EndScene");
        Note(PresentFn, "Frontend", "D3D9", "009BEEB0 Present");
        IssueFrontendFramePresent();
        FrontendFrameCount++;
        FrontendPresentCount++;
    }
```

Trace-only test (no `Device`) still sees that VA order and
`FrontendFlushCount=2`:
`EngineLifecycleTests.Frontend_0042EC7C_frame_is_input_then_0042DF9E_Present`.

---

## 2. Live Present body

```4107:4124:src/Fable.Game/EngineLifecycle.cs
    private void IssueFrontendFramePresent()
    {
        var device = Device;
        if (device is null)
            return;
        var frame = FrontendDx9Submit.FrontendFrame();
        device.Clear(Dx9Clear.WhenArgZero, frame.ClearColorArgb, 1f, 0);
        device.BeginScene();
        if (device is VulkanDx9Device vk)
            vk.BindFrontendTextures(_frontendDx9Textures);
        device.SetViewport(new Dx9Viewport(
            0, 0, BackBufferWidth > 0 ? BackBufferWidth : DisplayDefaultWidth,
            BackBufferHeight > 0 ? BackBufferHeight : DisplayDefaultHeight,
            frame.ViewportMinZ, frame.ViewportMaxZ));
        FrontendDx9Submit.IssueRecoveredDraws(device, _frontendDx9Records);
        device.EndScene();
        device.Present();
    }
```

Records come from the same widget walk, before the NativeSemantic
early-out of the host batch:

```9081:9094:src/Fable.Game/EngineLifecycle.cs
    private void CompositeFrontendPresent()
    {
        var width = BackBufferWidth > 0 ? BackBufferWidth : DisplayDefaultWidth;
        var height = BackBufferHeight > 0 ? BackBufferHeight : DisplayDefaultHeight;
        var (records, textures) = CollectFrontendRecords();
        _frontendDx9Records = records;
        _frontendDx9Textures = textures;
        if (Dx9OwnsFrontendPresent)
        {
            FrontendBatch = null;
            return;
        }

        FrontendBatch = Dx9VulkanFrontend.BuildBatch(records, textures, 0, 0, width, height);
```

`IssueRecoveredDraws` is DIPUP (sprites) + PrimitiveUP (glyphs).
Empty dest is skip (`00BADB36`). Not buffered `DrawIndexedPrimitive(0)`:

```285:325:src/Fable.Game/FrontendDx9Submit.cs
    public static void IssueRecoveredDraws(
        IDirect3DDevice9 device,
        IReadOnlyList<FrontendDx9DrawRecord> records)
    {
        foreach (var rec in records)
        {
            if (rec.DestX1 <= rec.DestX0 || rec.DestY1 <= rec.DestY0)
                continue;
            if (rec.RecordType == (int)GlyphRecordType)
                IssueGlyphUp(device, rec);
            else
                IssueSpriteUp(device, rec);
        }
    }
```

Recovered wrapper VAs (`FlushPairCount=2`):

```192:211:src/Fable.Game/FrontendDx9Submit.cs
    public static FrontendDx9FrameRecord FrontendFrame() =>
        new()
        {
            Frame = FrameFn,
            // ...
            FlushLayers = FlushLayersFn,
            FlushLayersArg = 1,
            EndScene = EndSceneFn,
            Present = PresentFn,
            ClearBeforeBeginScene = true,
            FlushPairCount = 2,
            ClearColorArgb = 0xFF000000,
```

`FrontendDx9SubmitTests.Frame_wrapper_vas_match_0042DF9E` locks
`Frame=0042DF9E`, `FlushLayers=009DA9F0`, arg `1`, pair count `2`.

---

## 3. `009DA9F0` is still Note-only

Called twice from `PumpFrontendFrame`. Always `begin=0,end=0`:

```4384:4407:src/Fable.Game/EngineLifecycle.cs
    private void FlushFrontendDisplay()
    {
        Note(DisplayFlush2dFn, "Frontend", "D3D9",
            "009D9C80 [0x13BC800] device flags");
        // ...
        var shouldDip = DisplayFlushShouldDip(0, 0);
        Note(DisplayFlushLayersFn, "Frontend", "D3D9",
            shouldDip
                ? $"009DA9F0({DisplayFlushLayersArg}) [+{DisplayQueueBeginOffset}] DIP vtbl+{DrawIndexedPrimitiveVtbl}"
                : $"009DA9F0({DisplayFlushLayersArg}) [+{DisplayQueueBeginOffset}] empty");
        Note(DisplayFlushLayersFn, "Frontend", "D3D9",
            shouldDip
                ? $"00A058C0 then vtbl+{DrawIndexedPrimitiveVtbl} prim {DisplayFlushPrimitive(false)}/{DisplayFlushPrimitive(true)}"
                : "009DA9F0 skip DIP no type 0x22");
        Frontend2dDipIssued = shouldDip;
        FrontendFlushCount++;
    }
```

```2041:2042:src/Fable.Game/EngineLifecycle.cs
    public static bool DisplayFlushShouldDip(int begin, int end) =>
        DisplayQueueCount(begin, end) != 0;
```

No enqueue into `+16020`. First-seen native drain is empty
(`009DB6E6`). Host `Frontend2dDipIssued` stays false on this
path. Do not treat recovered `00BAE2D0` DIPUP as a live
`009DA9F0` queue DIP.

---

## 4. Ownership: Compatibility / Shadow / NativeSemantic

```38:43:src/Fable.Game/Dx9SubmitOwnership.cs
    public Dx9SubmitMode FrontendMode(bool deviceAttached)
    {
        if (CanRenderFrontendSprites && CanRenderFrontendGlyphs)
            return Dx9SubmitMode.NativeSemantic;
        return deviceAttached ? Dx9SubmitMode.Shadow : Dx9SubmitMode.Compatibility;
    }
```

```3374:3382:src/Fable.Game/EngineLifecycle.cs
    public bool Dx9OwnsFrontendPresent =>
        FrontendSubmitMode == Dx9SubmitMode.NativeSemantic
        && Stage == EngineStage.Frontend;
```

Live client sets both capabilities and a Vulkan device that
owns Present:

```52:61:src/Fable.Client/Program.cs
    life.SubmitCapabilities = new Dx9SubmitCapabilities
    {
        CanRenderFrontendSprites = true,
        CanRenderFrontendGlyphs = true,
    };
    life.Device = new VulkanDx9Device
    {
        Renderer = host.Renderer,
        OwnsSwapchainPresent = true,
    };
```

```177:183:src/Fable.Client/Program.cs
    // NativeSemantic Device.Present already
    // consumed the swapchain. Shadow and
    // Compatibility still need host.Draw.
    else if (life.Dx9OwnsFrontendPresent)
        return;
    else
        host.Draw(aspect);
```

`VulkanDx9Device.DrawIndexedPrimitiveUP` / `DrawPrimitiveUP`
accumulate the 2D batch; `Present` stores `LastBatch` and,
when `OwnsSwapchainPresent`, calls `PresentDx9`:

```176:257:src/Fable.Render/VulkanDx9Device.cs
    public void DrawIndexedPrimitiveUP(...)
    {
        // ...
        _draws.Add(new FrontendDraw(
            _stage0,
            firstVertex,
            (uint)numVertices,
            firstIndex,
            (uint)words.Length,
            // ...
            Dx9VulkanFrontend.D3dptTriangleList));
    }

    public void Present()
    {
        PresentCount++;
        LastBatch = new FrontendSubmitBatch(/* verts, indices, draws, textures, viewport */);
        Renderer?.SetFrontendBatch(LastBatch.IsEmpty ? null : LastBatch);
        if (OwnsSwapchainPresent)
            Renderer?.PresentDx9();
    }
```

Unread first-seen ops throw with this Present named:

```278:279:src/Fable.Render/VulkanDx9Device.cs
    private static NotSupportedException Unread(string name) =>
        new($"IDirect3DDevice9.{name} is UNREAD on first 0042DF9E Present.");
```

---

## 5. Tests

| Test | What it locks |
| --- | --- |
| `Dx9DeviceRecordTests.Native_semantic_frontend_present_builds_device_batch` | install + `VulkanDx9Device` + sprites/glyphs → `Dx9OwnsFrontendPresent`, `FrontendBatch==null`, host Present count unchanged, `LastBatch` nonempty (sprite `IndexCount==6` and glyph `IndexCount==0 && VertexCount==6`) |
| `Dx9DeviceRecordTests.No_save_guild_arrival_slice_native_semantic_frontend_then_3d` | same frontend batch, then New Game; `Dx9OwnsFrontendPresent` false in Game; Lookout not Oakvale |
| `Dx9DeviceRecordTests.Shadow_frontend_pump_records_nonempty_sprite_and_glyph_draws` | capabilities false → Shadow; host Present runs; `FrontendBatch` nonempty; recording has DIPUP + UP between Begin/End |
| `Dx9DeviceRecordTests.First_frontend_present_is_clear_begin_end_present` | Shadow recording names `Clear, BeginScene, SetViewport, EndScene, Present`; `Dx9OwnsFrontendPresent==false` |
| `Dx9ArchitectureTests.Native_semantic_frontend_skips_host_present` | capabilities on recording device → host Present not incremented; `FrontendBatch==null` |
| `Dx9ArchitectureTests.Frontend_stays_shadow_until_sprite_and_glyph_capabilities` | both flags required for NativeSemantic |
| `EngineLifecycleTests.Frontend_0042EC7C_frame_is_input_then_0042DF9E_Present` | no Device: Notes only; flush count 2; VA order BeginScene → UI draw → flush → EndScene → Present |

NativeSemantic install pump (device batch, not host batch):

```329:357:tests/Fable.Formats.Tests/Dx9DeviceRecordTests.cs
    public void Native_semantic_frontend_present_builds_device_batch()
    {
        // ...
        var device = new VulkanDx9Device { OwnsSwapchainPresent = false };
        // SubmitCapabilities sprites+glyphs
        Assert.True(life.Pump());
        Assert.True(life.Dx9OwnsFrontendPresent);
        Assert.Null(life.FrontendBatch);
        Assert.Equal(presents, host.PresentCalls);
        Assert.False(device.LastBatch.IsEmpty);
        Assert.True(device.LastBatch.Vertices.Length >= 4);
        Assert.Contains(device.LastBatch.Draws, d => d.IndexCount == 6);
        Assert.Contains(device.LastBatch.Draws, d => d.IndexCount == 0 && d.VertexCount == 6);
```

---

## 6. Split that is easy to mix

```
0042EC7C Pump
  0042E3EE input          Note + PumpInput
  0042DC94 / 00599E3F     Note + TickFrontendWidgets
  0042DF9E                Note (VA still traced)
    009D8CF0 / 009BEF20   Note, then live Clear+BeginScene
    00595222 [ui+84]      Note + DrawFrontendWidgets
    Collect records       CompositeFrontendPresent
    009D9C80 / 009DA9F0×2 Note only; +16020 empty
    009BEF50 / 009BEEB0   Note, then live EndScene+Present
                          IssueRecoveredDraws = 00BAE2D0 / 00AB7C20
```

| Path | `0042DF9E` Present |
| --- | --- |
| `Device == null` | **Note-only** (`IssueFrontendFramePresent` return) |
| Shadow (`Device` set, capabilities false) | **Live** recording + host `FrontendBatch` |
| NativeSemantic (sprites+glyphs, Frontend) | **Live** device batch; host Present skipped |

Ledger “Present `0042DF9E` still Note-only”
(`docs/status/README.md` rows that still say that) is
**STALE** against `IssueFrontendFramePresent` and
`Native_semantic_frontend_present_builds_device_batch`.
`009DA9F0` empty Note is not stale.
