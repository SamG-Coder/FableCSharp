# Playable path now — host from launch

Investigation only. **Not** a first-scene milestone lock.
Do **not** declare New Game / Oakvale intro complete.

Status words in the table: **MATCH** / **DIVERGE** / **UNREAD** /
**LEFTOVER**. Those are host-vs-native classes, not the
`docs/status/README.md` PROVEN/PARTIAL words.

Sources (read this SHA, not a snapshot freeze):

- `src/Fable.Client/Program.cs`
- `src/Fable.Client/SilkEngineHost.cs`
- `src/Fable.Client/SilkNativeInput.cs`
- `src/Fable.Game/EngineLifecycle.cs` (`BootstrapUntilGraphics` /
  `CompleteRetailLoop` / `Pump` / `FinishStartupVideo` /
  `EnterFrontendAfterAvi` / `PumpFrontendFrame` /
  `DispatchFrontendMessage` / `RequestNewGame` / `EnterGame` /
  `PumpGame` / `LoadFromFirstRealRegion`)
- `src/Fable.Game/WmvPlayer.cs` (`IBasicAudio` QI + `put_Volume(0)`)
- `docs/status/README.md` boot / frontend / Gameflow rows
- `docs/PARITY.md` Leave Press Start, New Game click, Leave /
  Init Game, Gameflow, `Q_NewOakValeIntro` activator
- `EngineLifecycleTests.Frontend_type4_posts_stored_0xE5_then_0x126_then_15`
- `EngineLifecycleTests.First_pump_004189C2_is_0040D2A0_then_00B239A0_not_a_region`
- `EngineLifecycleTests.Second_pump_00501450_is_004FEEC0_then_00500540_1_0_0`
- `EngineLifecycleTests.Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`
- `EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`
- `EngineLifecycleTests.After_004AEA70_eq_1_00417001_is_00435F70_Present`

---

## Honest path (today)

```
Fable.Client.exe
  GameInstall.TryLocate
  EngineLifecycle.BootstrapUntilGraphics   CRT → WinMain → named stages
  Silk window 1024×768, title TEXT_GUI_WINDOW_TITLE
  AttachHost + VulkanDx9Device
  CompleteRetailLoop                       00412F90 / 0042EA8F
  Pump  StartupVideos                      006286F0 ×3 WMV + IBasicAudio
  Pump  Frontend                           PRESS_START
    LMB Type4+Type6 → msg 0xE5             New Profile, seed "Default"
    LMB Apply → msg 0x126                  MAIN_MENU_NO_CONTINUE
    LMB New Game → msg 15 → [retail+41]=1
  Pump  LeaveFrontend                      0042F2A2 FinalAlbion.wld
  Pump  EnterGame                          004184BD named stages
  Pump  Game                               004189C2 dummy +156=0
                                           Gameflow Main yields on
                                           Q_NewOakValeIntro
                                           CurrentRegion = null
                                           WorldSubmitted = false
  Optional host F2                         FlyCamera WASD (debug)
  Tests only                               LoadFromFirstRealRegion
                                           LookoutPoint adult 4299
```

Live `Program.cs` never calls `ActivateNewGame`, `Key.N`, or
`LoadFromFirstRealRegion`. It never opens Oakvale. After New
Game Leave, the clock ticks with **no region**. Lookout 3D is
a **test** call of `00501450`, not the exe host. Kid
`CAM_OVIF_SHOT2` / `CS_OAKVALE_INTRO_FATHER` is the intro
contract, not this no-save Present.

---

## Table

| step | host behaviour | native | class | blocker? |
|---|---|---|---|---|
| 1. Launch | `Program.cs` locates TLC (`FABLE_PATH`), `new EngineLifecycle()`, `BootstrapUntilGraphics` then Silk `Window.Create`. No WinMainCRT heap. | PE `00401067` CRT → `004011E7` `00403480` WinMain → `00402510` named bootstrap. | **MATCH** (Notes the dump walk; does not run MSVCR71). | no |
| 2. Window / title | Size `BackBufferWidth`/`Height` (default 1024×768). Title `life.WindowTitle` / `TEXT_GUI_WINDOW_TITLE` → `"Fable - The Lost Chapters"`. Silk starts windowed; Alt+Enter toggles OS fullscreen. | `00403079` / `009C0E50` 1024×768. Exclusive is `![0x137544A]` / `009BF7E0`. Style `0xCA0000`. | **MATCH** size/title. **LEFTOVER** windowed start vs PE exclusive default. | no |
| 3. Device attach | `window.Load`: `VulkanLineRenderer`, `SubmitCapabilities` sprites+glyphs, `VulkanDx9Device { OwnsSwapchainPresent = true }`, then `CompleteRetailLoop`. | CreateDevice after Setup library. RunModes `00412F90` retail `0042EA8F`. | **MATCH** order (graphics then mode loop). Vulkan is the DX9 submit translation. | no |
| 4. Mode loop enter | `CompleteRetailLoop` sets `EngineMode.RetailFrontend`, `Stage = StartupVideos` unless `PlayStartupVideos` false. | `004022B0` probe then `00412F90`. PlayAVI when `[0x1375448]` and `[0x137544A]` PE 1,1. | **MATCH**. | no |
| 5. Startup AVI | `Pump` → `PumpStartupAvi` → `WmvPlayer.TryOpen` rewritten `.wmv`. `Present` video dest `00628B79`. `window.Render` **`Draw(default)`** while `Stage==StartupVideos` (no 3D, no frontend, no F2). Optional `FABLE_SKIP_STARTUP_AVI` host-skips with `FinishStartupVideo`. Skip DIK Escape/Space/Enter/F4. | Table `0042ECC5` / `006286F0`: `lionhead_logo` 640×400, `Microsoft_Logo` 640×480, `intro_comp` 640×360. Rewrite `.xmv`→`.wmv`. `006286F0` blit+Present only (`FirstSeenPlayAviDrawsWorld=false`). Skip DIK 1/57/28/62. | **MATCH** slots, dest, skip keys, no 3D under logos. **LEFTOVER** #20 still listed in status (3D Draw during PlayAVI); host path now skips it. Env skip is **DIVERGE** (host-only). | no (AVI itself) |
| 6. AVI sound | `WmvPlayer.BuildGraph` QIs `IBasicAudio` (`0x12AA054`) then `put_Volume(0)` = 0 dB. `WorldSceneTests.PlayAvi_rewrites_xmv_to_installed_wmv_and_blocks` asserts `LastBasicAudioQi`. | `00A3B9D0` QI Control/Position/Seeking/Event/**BasicAudio**, `put_Volume(0)`. Voice is the WMV pin + quartz DirectSound, not a WAV bank. | **MATCH** on current `src/` (issue #9 proof `proofs/issue-9-verify` is stale vs this `WmvPlayer`). | no |
| 7. After third AVI | `FinishStartupVideo` → `EnterFrontendAfterAvi`: Notes `0042E98F` / Init Engine `0042EF40` / Init frontend `0042EF6F` / clear+Present `009D8CF0`+`009BEEB0` then `InitFrontendUi`. | Same after `006286F0` third slot: `[0x13B8616]==0` skip `009A8840`, then those inits, Present **before** UI show. | **MATCH**. | no |
| 8. Title / Press Start | `InitFrontendUi` attaches `UI_FRONTEND_PRESS_START_MENU` type 10 slot `0x14`, msg `0xE5` via `00598A1C` / `0059B5D7`. `PumpFrontendFrame` `0042EC7C` input then `0042DF9E`. Device Present when `Dx9OwnsFrontendPresent`. Sprites `frontend.big`. | `00598A1C(0)` Press Start first, not `MAIN_MENU_*`. Type 10 `0054E3D0` / draw `00530260`. Present `0042DF9E` still Note-only in ledgers. | **MATCH** screen/slot/msg. **LEFTOVER** dest invented 512,384,512,384 (#36); Present body Note-only; exclusive-walk SelectsChild (#46). | no for reaching the screen |
| 9. Leave Press Start | Client LMB edge Type4 + LMB-up Type6 (`SilkNativeInput`). Type-10 action 26 posts +352 `0xE5`. `DispatchFrontendMessage(0xE5)` → empty `005955AB` → `00595845` then same-frame `00596917` slot `0x17`. Enter is TypeKey; tests lock it does **not** post 15 from Press Start. | `0059A238` msg `0xE5` → `00599D5C` → `00595845` / `00596917`. Native **key** that posts `0xE5` is **UNREAD** (`0041E6D3` is the consumer). Host Return→msg 15 from Press Start is DISPROVEN. | **MATCH** LMB Type4/Type6 → `0xE5`. **UNREAD** native DIK poster. **LEFTOVER** #14 keyboard N/Enter. | no if player clicks |
| 10. New Profile / name | Slot `0x17` `UI_FRONTEND_NEW_PROFILE_SCREEN`. `00851770` type-37 edit box. Game singleton 0 → UTF-16 `0x122DE80` **"Default"**. Host `BindEditBoxSeed("Default")`. | Same seed. Msg `0x126` is persist +228 on `UI_ACCEPT_NEW_PROFILE`. | **MATCH** Default seed + screen. **LEFTOVER** dest/hit stand-in (#48: `PlaceTableCell` n==3, `TryChromeHit` size invent). Typing a custom name is not a recovered keyboard poster. | no for Default |
| 11. Accept profile | Click `UI_ACCEPT_NEW_PROFILE` (host dest midpoint / chrome hit) → Type4 arm + Type6 post `0x126` → `00851920` `[+5]=1` → next tick `0059697A` / `004067C0` writable → `00595A06` `MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE`. | Same. Native DIK for `0x126` UNREAD. | **MATCH** click path. **LEFTOVER** #48 hit rects. | no if click hits Apply |
| 12. Main Menu | Resident `[ui+84]` keeps slots `0` / `0x14` / `0x17`. Current tree is Main Menu for input. `UI_TEXT_NEW_GAME` id=0. Tick/draw walk every resident slot (`00595222` / `0059A0C4`). | `00596763` switches current; does not drop the map. `00595A06` overwrites key 0. | **MATCH** menu + resident slots. **LEFTOVER** host input still the switched `_frontendWidgets`; dest invented; exclusive-walk (#46). | no |
| 13. New Game click | Click `UI_FRONTEND_BUTTON_NEW_GAME` → Type4 arm + Type6 post **15** → `0059A2DA` / `00594F28` `[retail+41]=1`. Same `Pump` sees the flag → `RequestNewGame` then `EnterGame`. Client has **no** `Key.N` / `ActivateNewGame`. Enter on Main Menu stays TypeKey (does not set the flag in the type4 test). | Message 15 → Leave `0042F2A2`. Not `00DBDE40`. Keyboard N/Enter is not the recovered poster. | **MATCH** msg 15 → Leave. **LEFTOVER** #14 host keyboard. | no if click hits New Game |
| 14. Leave frontend | `RequestNewGame`: Notes Leave, `WorldFileName = FinalAlbion.wld` (`0042F44D`), `[0x1375448]=0`, skip bank swap, teardown `0042EBB6`, Present, `Stage=LeaveFrontend`, drop frontend batch, `OwnsSwapchainPresent=false`. Next `Pump` `EnterGame`. | `0042F2A2` same. `+41` skips audio stop. | **MATCH**. | no |
| 15. Init Game | `EnterGame` `0042F491` / `00418DCA` / vtbl+4 `004184BD`. Named stages (Thing Components Note-only Add Def Class, Graphics `GBANK_MAIN_PC`, Fonts `ENG_ARIAL_18`, World `004A67D0`/`004A6E30`, Create Players 5×`0x22C`, Load world `00416953` → `004A1840` → `00507C30` FinalAlbion). `user.ini` `ActivateQuest("Gameflow")`. Dummy `WorldMap+156=0`. | Same walk. `00501450` **0 E8** on this suffix. Editor/save UNREAD. | **MATCH** no-save Init Game. **LEFTOVER** Add Def Class Note-only (#42 dest UNREAD). | no for reaching Game |
| 16. Gameflow parked | Type-1 `00CB8220` / `00A44880` / `00CE7670` state 0. `00893610 Q_NewOakValeIntro` miss → yield. Watchers Main + Core + Barrow. Does **not** activate Oakvale or trader quests. Later resume still yields. | `EngineLifecycleTests.Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`. PARITY: `00CE7670` only waits. | **MATCH**. | **yes — next** (see below) |
| 17. CurrentRegion | First `004189C2`: `004FB150` +156, dummy `005066E0` record+36 null. `ActivateCurrentRegion` leaves `CurrentRegion=null`, `CurrentRegionIndex=0`. Second `Pump` inner loop, **not** `00501450`. `EnqueueAfterDummy` is **not** on this pump (DISPROVEN). | First pump not a region. `00501450` E8 caller **UNREAD**. | **MATCH** dummy / null. | no |
| 18. 3D Present | `SubmitCurrentWorld` returns if `CurrentRegion is null` or `!HeroSpawned`. Tests after New Game: `WorldSubmitted=false`, `HeroSpawned` still false. After `004AEA70=1`, `00435F70`/`00435530` Present empty dest / skip DIP. `Program.cs` then `host.Draw(aspect)` if camera non-null (ctor camera, no world verts). | Native first Present empty (`009DA9F0` skip DIP). No Lookout / no SHOT2. | **MATCH** empty Present / no region mesh. **LEFTOVER** host `Draw` may still compose a WVP with an origin camera and empty VB. | no for “game is running”; **yes** for a playable view |
| 19. F2 WASD | `Program.cs` F2 toggles `debugFly`. WASD/EQ + RMB look. `SilkNativeInput` skipped while flying. `Draw` uses `FlyCamera` WVPs. | FlyCamera is **not** `CAM_OVIF_SHOT2` / `006B3FF0`. No recovered player-move listener on frontend (`0055CB10` UNREAD). Game WASD is ActionInputListener after WorldFrame, not F2. | **DIVERGE** (debug only). | no (debug); does not unblock intro |
| 20. Lookout (tests) | `LoadFromFirstRealRegion` is public and used by tests (`Second_pump_00501450_…`, install banks). Loops `00500540(i,0,0)` i=1..141, first i=1 **LookoutPoint**, last +156=141 filler, restore `(0,0,1)`. Hero `GuildArrivalHSP` adult mesh **4299**. Host first-proximity TNG `break` is OOM workaround (#50). | Body recovered; **caller UNREAD**. Invented “only index 1” DISPROVEN. Adult Lookout is **not** first-scene intro view. | **MATCH** when tests call it. **LEFTOVER** #50 / #4 (Lookout vs Oakvale). Live exe host **does not** call it. | no for tests; **yes** for live 3D |
| 21. Oakvale intro | Host does **not** `ActivateQuest(Q_NewOakValeIntro)`. `00CD6E27` bind-only. WLD +172 is `Q_SunnyvaleMaster`. user.ini is Gameflow. Persist `PlayerRegionName` writer **UNREAD**. `CS_OAKVALE_INTRO_FATHER` / `HerosOldHouse` / `CAM_OVIF_SHOT2` / kid 4300 stay the **intro contract**, not this Present. | PARITY: activator **not on the no-save walk**. `00CE7670` only waits. | **UNREAD** activator. Host invent would be **DIVERGE**. | **yes — next blocker** |

---

## What you can play today

1. Boot TLC, window 1024×768 titled Fable TLC.
2. Three startup WMVs with DirectShow audio (`put_Volume(0)`).
3. Press Start (click).
4. New Profile seeded **Default** (click Apply).
5. Main Menu (click New Game).
6. Leave → Init Game → Gameflow **parked** waiting for
   `Q_NewOakValeIntro`.
7. `CurrentRegion` stays **null**. No Lookout, no Oakvale house.
8. F2 WASD is a debug fly camera, not hero control.

`FABLE_SKIP_STARTUP_AVI` jumps to Press Start. It is not native.

---

## Next blocker

**Who activates `Q_NewOakValeIntro` is UNREAD.**

Locked already (`docs/PARITY.md` “Who activates
`Q_NewOakValeIntro`”,
`EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`):

- `00CD6E27` bind `S_QNOVI` / `00DBEF70` only
- WLD `+172` is `Q_SunnyvaleMaster`
- `+90584` empty skips `004B4A10`
- `004B5080` is save `START_NEW_QUEST`
- `AddTestQuest` does not activate
- `00CE7670` waits; user.ini is Gameflow
- Host must **not** invent `ActivateQuest(Q_NewOakValeIntro)`

Until that writer (or persist `PlayerRegionName` → `StartOakVale`
index 4, also UNREAD) is recovered, the intro fiber
(`00DBDE40` / `NOVI_LiveFather` / `CS_OAKVALE_INTRO_FATHER` /
`CAM_OVIF_SHOT2`) cannot start on the live no-save clock.

Related, not the same blocker:

- `00501450` E8 caller UNREAD (Lookout load is test-only)
- leftover #4 Lookout vs Oakvale intro view
- leftover #14 / #36 / #46 / #48 frontend dest/hit
- leftover #20 3D Draw during PlayAVI (host logos skip it)
- leftover #50 first-proximity TNG OOM workaround

Do not collapse those into an invented Oakvale `SetRegionAsLoaded`.
