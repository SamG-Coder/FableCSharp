# Issue #14 vs HEAD: FRONT_END Present + New Game input

Investigation only. No `src/` or `tests/` edits.

Issue: [SamG-Coder/FableCSharp#14](https://github.com/SamG-Coder/FableCSharp/issues/14)
(open on GitHub; body dated 2026-08-18; still linked to PR #35).

HEAD: `ee084901e8212814d4ca7df599180117f9be5cec` (`master`).

Authority: issue body; `src/Fable.Game/EngineLifecycle.cs`
(`Pump`, `PumpFrontendFrame`, `DrawFrontendWidgets`,
`MaybeActivateNewGameFromInput`, `DispatchFrontendMessage`,
`ActivateNewGame`, `ResidentSlotTrees`, `WriteType10AttachMessage`);
`src/Fable.Game/FrontendInputMap.cs`;
`src/Fable.Game/FrontendMessages.cs`;
`src/Fable.Game/EngineInput.cs`;
`src/Fable.Game/IEngineHost.cs`;
`src/Fable.Client/Program.cs`;
`src/Fable.Client/SilkEngineHost.cs`;
`src/Fable.Render/VulkanLineRenderer.cs` /
`VulkanLineRenderer.Frontend.cs`;
`tests/Fable.Formats.Tests/EngineLifecycleTests.cs`;
`tests/Fable.Formats.Tests/FrontendInputTests.cs`;
`docs/status/README.md`;
`docs/PARITY.md`.

Do not re-prove persist Type=10, dest first-seen table, or
Leave/`FinalAlbion.wld`. Host Return as Press Start is
**DISPROVEN** (below).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH** /
**STILL OPEN** / **FIXED**.

---

## Verdict vs HEAD

**PARTIAL.** The issue’s player-facing failure (blank Vulkan
frame after the videos + keyboard `N`/`Enter` cheat) is
**gone**. The recovered type-4 / `0xE5` attach path **exists**.
`0042DF9E` / `00595222` are **still Note wrappers** around a
host batch, so the issue title is not fully closed.

| Issue claim (2026-08-18) | vs HEAD |
| --- | --- |
| `PumpFrontendFrame` only `Note`s `0042DF9E` / `00595222` | **PARTIAL** — still Notes; then walks `[ui+84]` and builds `FrontendBatch` |
| `FrontendUiDrawFn` never emits vertices | **FIXED** for the host Present (`CompositeFrontendPresent` → Vulkan) |
| Client frontend branch `return`s after pump; `window.Render` draws unbound `gameCam` | **FIXED** — that branch is gone |
| New Game is `Key.N` / `Key.Enter` → `ActivateNewGame` | **FIXED** as a client cheat |
| Player does not see FRONT_END after videos | **FIXED** on the install path (batch + Present) |
| Host Return is Press Start / New Game | **DISPROVEN** |
| Type-4 / `0xE5` attach path missing | **FIXED** (LMB Type4 + Type6; slot `0x14` write) |
| Issue done-looks-like #1 (submit UI into Present **or** stop claiming the VAs ran) | **MATCH** first arm |
| Issue done-looks-like #2 (keep msg-15 / `[retail+41]`; no save-enumerate) | **MATCH** |
| Issue done-looks-like #3 (no Lookout / Oakvale under frontend Present) | **MATCH** |
| GitHub #14 | still **open** |
| Ledger `docs/status/README.md` | still “leave #14 open” |

**STILL OPEN** leftovers (why this is not **FIXED**):

- `0042DF9E` / `009DA9F0` DIP body is still Note-only (#36).
- Host draw does not skip `State=6` trees.
- Input still reads `_frontendWidgets` (switched screen), not a
  click hit-test / `[ui+32]` current deque.
- Host never factory-fills native slot `0x1` OPTIONS.
- `ActivateNewGame()` remains a test stand-in for msg 15.

---

## 1. What #14 asked

Issue body (quoted):

> Recovered frontend Present is `0042DF9E`: clear,
> `009BEF20` BeginScene, `00595582` / `00595222` UI vtbl+8
> walk, `009BEF50` EndScene, `009BEEB0` Present.
>
> `EngineLifecycle.PumpFrontendFrame` only `Note`s those VAs.
> `FrontendUiDrawFn` never emits vertices. The client frontend
> branch then returns after the pump; `window.Render` still
> runs and draws `gameCam` …
>
> New Game is therefore `Key.N` / `Key.Enter` →
> `ActivateNewGame` (`0059A238` msg 15 / `[retail+41]`),
> not a click on `UI_TEXT_NEW_GAME`.
>
> After the three startup videos the player should see
> FRONT_END and pick New Game. They see a blank 1600×900
> Vulkan frame and have to know the keyboard cheat.

Done looks like:

1. Submit recovered frontend UI into the same Present
   `window.Render` already is, **or** stop claiming
   `0042DF9E` / `00595222` ran.
2. Keep `ActivateNewGame` as the msg-15 / `[retail+41]`
   write. Do not invent save-enumerate (`[retail+42]` UNREAD).
3. Do not draw Lookout or Oakvale under the frontend Present.

---

## 2. Client Update / Render (cheat gone)

`src/Fable.Client/Program.cs` has **no**
`Stage == EngineStage.Frontend` early return, **no**
`Key.N`, **no** `ActivateNewGame()` call.

Enter is PlayAVI skip (type 1 / DIK 28), same as Space /
Escape / F4. LMB edge is type 4; LMB-up is type 6.

```76:107:src/Fable.Client/Program.cs
    if (keyboard.IsKeyPressed(Key.Escape))
        life.QueueInput(EngineInput.TypeKey, RegionTravel.PlayAviSkipEscape);
    if (keyboard.IsKeyPressed(Key.Space))
        life.QueueInput(EngineInput.TypeKey, RegionTravel.PlayAviSkipSpace);
    if (keyboard.IsKeyPressed(Key.Enter))
        life.QueueInput(EngineInput.TypeKey, RegionTravel.PlayAviSkipReturn);
    if (keyboard.IsKeyPressed(Key.F4))
        life.QueueInput(EngineInput.TypeKey, RegionTravel.PlayAviSkipF4);
    ...
        if (lmbDown && !lmbWasDown)
            life.QueueInput(EngineInput.Type4, 0);
        if (!lmbDown && lmbWasDown)
            life.QueueInput(EngineInput.Type6, 0);
```

`window.Update` always `life.Pump((float)dt)`.
`window.Render` always `host.Draw(aspect)` (or the F2 fly
cam). No frontend-only clear + return.

`Pump` on `EngineStage.Frontend` unloads AVI, inits UI,
pumps one `0042EC7C` analog frame, then Presents:

```2961:2974:src/Fable.Game/EngineLifecycle.cs
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

            PresentToHost();
            return true;
        }
```

After the third video `EnterFrontendAfterAvi` Notes
`FRONT_END` and `InitFrontendUi()` (Press Start slot `0x14`).

---

## 3. `PumpFrontendFrame` / `FrontendUiDrawFn`

Still Notes the recovered VAs. It is **not** Note-only
anymore: input → type-4/`0xE5` map → tick walk → draw walk
→ host batch.

```3496:3533:src/Fable.Game/EngineLifecycle.cs
    public void PumpFrontendFrame()
    {
        Note(FrontendInputFn, "Frontend", "Input",
            "0042E3EE walk [0x13B8388]");
        ...
        PumpInput();
        // 0042E3EE then 0042DC94: 0xE5
        // lands before 00599E3F so
        // 00595845 and 00596917 are
        // the same frame.
        MaybeActivateNewGameFromInput();
        Note(FrontendUpdateFn, "Frontend", "UI", "0042DC94");
        Note(FrontendUiTickFn, "Frontend", "UI", "00599E3F");
        ...
        Note(FrontendDrawFn, "Frontend", "Render", "0042DF9E");
        ...
        TickFrontendWidgets();
        DrawFrontendWidgets();
        ...
        FlushFrontendDisplay();
        ...
        Note(PresentFn, "Frontend", "D3D9", "009BEEB0 Present");
        FrontendFrameCount++;
        FrontendPresentCount++;
    }
```

`DrawFrontendWidgets` Notes `00595222` then walks every
resident `[ui+84]` tree (`ResidentSlotTrees`). Type 5/10/12/18
use `00530260` DrawsChildList; leaves queue type-`0x22`
records.

```3576:3605:src/Fable.Game/EngineLifecycle.cs
    private void DrawFrontendWidgets()
    {
        FrontendWidgetsDrawn = 0;
        Frontend2dRecordsQueued = 0;
        Note(FrontendUiDrawFn, "Frontend", "UI",
            $"00595222 [ui+{FrontendWidgetListOffset}]");
        ...
        foreach (var tree in ResidentSlotTrees())
        {
            any = true;
            DrawSlotTree(tree, ref drawn);
        }
        ...
        CompositeFrontendPresent();
    }
```

Vertices come from `CompositeFrontendPresent`, not from
executing native `009DA9F0`:

```7728:7742:src/Fable.Game/EngineLifecycle.cs
    /// Present is <see cref="FrontendBatch"/>
    /// (<c>00BAE2D0</c> / <c>00AB7C20</c> →
    /// Vulkan). CPU blit into
    /// <see cref="FrontendPresentRgba"/> is a
    /// TEMPORARY test dump only.
    private void CompositeFrontendPresent()
    {
        ...
        FrontendBatch = Dx9VulkanFrontend.BuildBatch(records, textures, 0, 0, width, height);
        DumpFrontendPresentRgba(records, textures, width, height);
    }
```

`FlushFrontendDisplay` still **Notes** `009D9C80` /
`009DA9F0(1)` and flips `Frontend2dDipIssued` from the
host enqueue flag. That is the remaining “Present
`0042DF9E` still Note-only” ledger line.

Host Present consumes the batch; Vulkan `DrawFrontend`
issues the quads on the same `window.Render` Present:

```89:97:src/Fable.Client/SilkEngineHost.cs
            else if (frame.FrontendBatch is { IsEmpty: false } batch)
            {
                ...
                renderer.SetFrontendBatch(batch);
            }
```

```1309:1309:src/Fable.Render/VulkanLineRenderer.cs
        DrawFrontend(commandBuffer);
```

`EngineFrame` comment: “Null world is frontend or loading
(clear only).” Frontend pump does not `LoadFromFirstRealRegion`.
Lookout / Oakvale under this Present is **DISPROVEN** as a
live path.

Install test
`Frontend_PRESS_START_is_type_10_with_text_child` asserts
non-empty `FrontendBatch` after videos + `Pump`.
`Frontend_present_runs_on_install_after_videos` asserts
`FrontendPresentCount == 1` and PRESS_START children.

---

## 4. Type-4 / `0xE5` vs N/Enter

### Native (recovered, already locked)

```
0042E3EE  type [record+40]
  type 4 → action 26   // 00A03C80 LMB down, not a DIK
  type 6 → action 28   // 00A03D60 LMB up
  type 1 → action 33   // DIK 28 Return
0054E280 action 26 on type-10 → +352 (attach 0xE5)
0055ACF0 action 28 on type 11/38 → +228 after arm
0059A238  0xE5 / 0x126 / 15
```

`00598A1C` attach writes `0xE5` through slot `0x14`
`0059B5D7` then `vtbl+284` `0054E4F0`. Host
`WriteType10AttachMessage` Notes that path and patches
slot `0x14` root `MessageId`.

### Host input

```3229:3247:src/Fable.Game/EngineLifecycle.cs
    private void MaybeActivateNewGameFromInput()
    {
        if (Stage != EngineStage.Frontend)
            return;
        foreach (var (type, key) in Input.Applied)
        {
            var action = FrontendInputMap.ActionFromEvent(type, key);
            if (action == FrontendInputMap.ActionType4)
                ArmType34Widgets();
            var mapped = action is int act
                ? FrontendInputMap.MessageFromWidgets(act, _frontendWidgets)
                : null;
            if (action == FrontendInputMap.ActionType6)
                UnarmType34Widgets();
            if (mapped is not int msg)
                continue;
            DispatchFrontendMessage(msg);
            return;
        }
    }
```

`MessageFromWidgets`: Type4 → first visible type-10
`MessageId` (`0xE5` on Press Start). Type6 → first armed
type 11/38 `MessageId` (`0x126` then 15). Type1 → null.

`ActivateNewGame` is still the msg-15 write only
(issue done-looks-like #2). Client does not call it.

```3976:3977:src/Fable.Game/EngineLifecycle.cs
    public void ActivateNewGame() =>
        DispatchFrontendMessage(FrontendNewGameMessage);
```

Install test `Type4_drives_lifecycle_0xE5_then_0x126_then_15`
(and `Frontend_type4_posts_stored_0xE5_then_0x126_then_15`):
Type4+Type6 three times walks PRESS_START → NEW_PROFILE
(“Default”) → MAIN_MENU_NO_CONTINUE → Leave / Game.

### Host Return as Press Start — DISPROVEN

Not a leftover of #14. Locked:

- `FrontendInputTests.Keyboard_and_Return_do_not_map_to_a_frontend_message`
- `EngineLifecycleTests.Frontend_press_start_Return_does_not_post_0xE5_or_15`
- Same file: Return on New Profile / Main Menu does **not**
  post `0x126` / 15
- `FrontendInputMap.TryMapEvent(TypeKey, 28, *)` is null
- `docs/PARITY.md`: “Host Return→msg 15 from Press Start is
  DISPROVEN.”

Client Enter queues `PlayAviSkipReturn` (28). During
frontend that is type 1 / action 33. It is **not** New Game.

`Frontend_press_start_type4_without_widgets_does_not_invent_0xE5`
locks the no-install Type4 empty-tree case (no invented
`0xE5`).

---

## 5. Later frontend work already on HEAD

These are why the original issue text is stale, not a
re-proof of each:

| Work | SHA / test | vs #14 |
| --- | --- | --- |
| Type-10 draw `00530260` DrawsChildList | `7adf621` | vertices exist |
| Slot `0x14` `0xE5` attach, not type-10 walk | `59fde69` / `Frontend_attach_0xE5_is_slot_0x14_0059B5D7_not_type10_walk` | poster |
| Keep `[ui+84]` `0x14`/`0x17` across switch | `84a8350` / `Frontend_ui84_keeps_slot_0x14_and_0x17_after_main_menu` | map |
| Tick/draw walk every resident slot | `b4a2c89` / `Frontend_tick_and_draw_walk_resident_ui84_slots` | `00595222` analog |
| Type4 posts +352 only; Type6 posts +228 after arm | `61e430f` / `48133e9` / #37 locked | click path |
| Host LMB Type4 / Type6 | `5dcc1fc` / `48133e9` | replaces N/Enter |
| `+332` SelectState(6) is **not** a `+302` hide | `b8a2b21` | leftover: host still draws State=6 |

Stale proofs vs this HEAD (do not treat as current):

- `proofs/00595222-visible-skip`: “host current-only” — walk
  is now all-slot (`ResidentSlotTrees`).
- `proofs/audit-frontend-leftover` § Type4
  `MessageFromAction(screen == PressStart)` — that helper
  now always returns null.

---

## 6. Leftover (why PARTIAL / still open)

1. **`0042DF9E` DIP is still a Note.**
   `FlushFrontendDisplay` does not run `00A058C0` /
   vtbl+332. `Frontend2dDipIssued` is a host bool.
   CPU `FrontendPresentRgba` is a test dump only.
   Ledger: “Present `0042DF9E` still Note-only.”

2. **Host draw does not skip `State=6`.**
   `SwitchFrontendSlot` writes `+332=6` on the old
   current. `CollectFrontendRecords` / `DrawSlotTree`
   only test `Visible` / `Clip` / dest size. After
   `0xE5` the Press Start tree stays resident and is
   still submitted. Native hide of inactive slots is
   **UNREAD** (not `+302`; not `[ui+32]` filter).

3. **Input leftover `_frontendWidgets`.**
   Comments: current switched screen (input).
   `MaybeActivateNewGameFromInput` does not hit-test
   dest rects. Type4 posts the first visible type-10
   in that list (any click). Type6 posts the first
   armed 11/38 (any LMB-up). Native listener set on
   `[ui+32]` is **PARTIAL**.

4. **Slot factory incomplete.**
   First-seen native `[ui+84]` also has slot `0x1`
   OPTIONS (see `proofs/00595222-first-node`). Host
   first resident key is only `0x14` until `0xE5`
   adds `0x17`.

5. **`ActivateNewGame()` test API.**
   Still `DispatchFrontendMessage(15)`. Fine as
   consumer. Not the player path.

6. **`window.Render` still calls 3D `Draw`.**
   Frontend mesh count is 0 so the 3D pass is a
   no-op; `DrawFrontend` still runs. Not the old
   “origin + intro FOV over empty world as the only
   picture,” but not a frontend-only Present either.

---

## 7. Proposed next step

Do **not** map Return / Enter / `N` to `0xE5` or 15.

Close the remaining #14 slice in this order:

1. Recover how inactive `[ui+84]` trees stay undrawn
   after `vtbl+192(6)` (dest / clip / parent / unseen
   bit — **not** invent `+302`). Until then, do not
   submit `State==6` dests in `CollectFrontendRecords`.
2. Point Type4/Type6 at the **current** slot tree
   (`FrontendCurrentSlot`) and only post if the cursor
   is inside an armed dest. Keep
   `_frontendWidgets` as that tree.
3. Keep `0042DF9E` Notes, but stop treating
   `Frontend2dDipIssued` as a native DIP. That leftover
   belongs with #36 (`009DA9F0`), not as a reason to
   reopen the blank-frame / N-key cheat.

Issue #14 can move to **PARTIAL** on the tracker (or stay
open until (1)+(2)). It is not a silent no-op Present
anymore.
