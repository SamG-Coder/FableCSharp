# Frontend / PlayAVI / Leave audio

Dump + C# only. No invented track names.

## Order (PROVEN)

`0042EC7C` plays three blocking `006286F0` slots, then `0042E98F` binds UI, then Init Engine / black Present, then audio start:

```
006286F0 ×3          blocking PlayAVI (dest 00628B79, Present 009BEEB0)
0042EE3D             [0x13B8616]==0 skip 009A8840
0042E98F             00595582 +180; 005958F5; 00598A1C(0)
                     0041DB1D UI_FRONTEND_PRESS_START_MENU slot 0x14
0042E204             Init Engine
009D8CF0 + 009BEEB0  black Present
0042DED5(0)          audio [0x13B8394].vtbl+68
005952C3             UI show
0062F800 / 0062F8B0  fade clocks
0040F0E0             post-init
```

`EngineLifecycleTests.Retail_0042EC7C_after_AVI_clears_then_inits_frontend` locks `complete(intro_comp) < 0042E98F < Init Engine < Present`.

---

## 1. Frontend music / ambience

| Item | Class | Evidence |
|---|---|---|
| Who starts frontend audio | **PROVEN** | After last AVI, `0042F00A call 0042DED5` with `fldz` (fade 0). Singleton `[0x13B8394]`. If null, `je 0042DF9A` no-op. |
| How it starts | **PROVEN** | `0042DED5`: `0041E5F2` → vtbl+168 → `+0x1CC` string via `0041A3D0`/`0099BE70`; `0099B6B0(0x1230C48)` + `0099B6B0(0x1230C3C)`; `0099C1E0` rewrite (same helper as `.xmv`→`.wmv`); `[0x13B8394].vtbl+68(path, 0, 0, 1.0, fade, -1)`. |
| Track / file name | **UNREAD** | `0x1230C3C` / `0x1230C48` sit in the `0x1230B80`–`0x1230C84` island (next to float `0x1230C38` and vtbl `01230C34`). Not in `strings.tsv`. Do not invent `MUSIC_SET_*` / forest ambience. |
| Script `PlayMusic` on frontend | **DISPROVEN** | `PlayMusic` / `MUSIC_SET_*` is first-scene `S_QNOVI` (`MUSIC_SET_NULL` then later `MUSIC_SET_OAKVALE`). No frontend.bin sound field. |
| Screen-attach audio | **PARTIAL** | `00596763` (NEW_PROFILE slot `0x17` / later slots): if `[0x13B8394]` and name match, `[eax+184](0x100, 0.0, [0x1252370])`. Not Press Start first bind. Float at `0x1252370` unread. |
| C# plays frontend music | **DISPROVEN** | `EnterFrontendAfterAvi` only `Note(0042DED5)`. No `WmvPlayer` / ogg / `[0x13B8394]` analog. |

Not forest-sprite ambience. Not `Play2DSound`. Not AVI `IBasicAudio`.

---

## 2. Click / accept on Press Start / menus

| Item | Class | Evidence |
|---|---|---|
| `0059A238` plays a click | **DISPROVEN** (first-seen) | Msg `0xE5` → `00599D5C`; `0x126` → `00851920`; `15` → `0059A2DA` `[retail+41]=1`. No `00A01920`, no `SND_*`, no `[0x13B8394]` on those branches. |
| Press Start accept sound | **UNREAD** | Native key that posts `0xE5` is UNREAD. `0054E280` action 26 posts `&widget+352`. No sound site in that recovered poster. |
| New Game click sound | **UNREAD** | Msg 15 is leave flag only. Widget id=0 click UNREAD. |
| `Play2DSound UI_CLICK` | **DISPROVEN** as frontend | Script opcode `00CBF89E` / `00CBF8DA` only. Not UI vtbl+32. |
| `SND_MENU_04` | **DISPROVEN** as Press Start | `005F64DD` / `005F6793` → `00A01920` then `[0x13B8394].vtbl+36` → `vtbl+12`. In-game GUI (near EXP spend), not `0059A238`. |
| C# click audio | **DISPROVEN** | `DispatchFrontendMessage` has no sound side effect. |

---

## 3. AVI finished before `0042E98F`?

| Item | Class | Evidence |
|---|---|---|
| Native: yes, fully returned | **PROVEN** | `0042EC7C` loop `006286F0` three times (`add edi,32`; `cmp [ebp+108],3`; `jb 0042ED68`). `006286F0` is blocking (`FirstSeenPlayAviBlocksUpdatePump`). Bind is `0042EE3D` after the loop. Skip videos if `[0x1375448]==0` or `[0x137544A]==0` (`je 0042EE3D`). |
| C# host pump | **PROVEN** analog | `PumpStartupAvi` waits `WmvPlayer.Ended` (or skip). Third `FinishStartupVideo` → `EnterFrontendAfterAvi` → `0042E98F` notes then `InitFrontendUi`. One Present after unload has `AviPlaying=false` so the host clears the last frame. |
| Mid-AVI frontend bind | **DISPROVEN** | Stage stays `StartupVideos` until slot 3 completes. |

---

## 4. Leave New Game (`+41!=0`)

| Item | Class | Evidence |
|---|---|---|
| Fade, not hard-stop first | **PROVEN** | `0042F2A2`: if `[0x13B8394]`, `vtbl+72(0x1F4)` (500 ms). C#: `LeaveFrontendAudioVtbl=72`, `LeaveFrontendAudioMs=0x1F4`. |
| Teardown skips stop | **PROVEN** | `0042EBB6`: `cmp [esi+41],bl; jne 0042EC2A` skips `vtbl+64`, `vtbl+72(0)`, `00991750`, `009918F0`. Then `009BE420`+`009BEEB0` still run. |
| What those stops are | **PARTIAL** | vtbl+64 / vtbl+72(0) / `00991750` / `009918F0` unread as named StopMusic. Do not invent dialogue teardown. |
| C# | **PARTIAL** | `RequestNewGame` notes `0042EBB6 +41 skip audio stop`. Does not fade 500 ms. No audio object to keep. |

Quit / load (`+41==0`) **does** take the stop path. New Game must not.

---

## 5. `WmvPlayer` / `PlayAviFromExe` vs frontend

`PlayAviFromExe` is dump evidence only (“Not wired into the live FilterGraph”). Live path is `WmvPlayer`.

| Mismatch | Blocks frontend? | Class |
|---|---|---|
| Never QI `IBasicAudio` (`00A3B9D0` does; IID `0x12AA054`) | **No** | **PARTIAL** issue #9. Silent AVI only. `BuildGraph` QIs Control/Position/Event; comment lists BasicAudio but no `QueryInterface` / `put_Volume`. |
| No `IMediaSeeking` store | **No** | **PARTIAL**. Native open QI includes Seeking. C# uses `IMediaPosition.put_CurrentPosition(0)`. |
| `put_CurrentPosition` retry 8 vs native 50 | **No** (unless Run fails) | **PARTIAL**. Run still retries `PlayAviRunRetry=50`. |
| RCW `GetTime` apartment marshal | **Yes if used** | **PROVEN** hang (`FirstSeenPlayAviGetTimeRcwMarshalsApartment`). Live copy must stay `call [sample+20]` on the decoder thread. |
| STA `TryOpen` 8 s / `no-sample` | **No** | **PROVEN** skip: `StartupAvi==null` → `FinishStartupVideo` next slot. Native open fail `00628DEB` also returns and the table advances. |
| STA `Join` 10 s (`sta-join`) | **Yes, one frame** | **PARTIAL**. `UnloadStartupAvi` on first Frontend `Pump`. Hung STA delays bind Present, does not skip `0042E98F`. |
| Never `EC_COMPLETE` / `Ended` | **Yes** | **PROVEN** hazard. `Pump` stays `StartupVideos` forever; `0042E98F` never runs. Native `006286F0` returns on EOF/skip. Skip scans 1/57/28/62 recovered. |
| Graph live across slots | **No** (frontend) | **PROVEN** unload `00A3B380`/`00A3BC20` before next `006286F0`. Leftover #20 is 3D Draw during AVI, not bind. |
| `PlayAviFromExe` second graph | **Yes if cloned** | **DISPROVEN** as live. Docs: do not execute a second CBaseRenderer. |
| Host `AviPlaying` true on frontend frame | **Yes (visual)** | **PROVEN** `SilkEngineHost`: AVI branch skips `SetFrontendBatch`. Must unload before bind Present. |

`IBasicAudio` does **not** gate `0042E98F`. A stuck graph / missing `Ended` does.

---

## C# recovery leftovers

| Need | Status |
|---|---|
| Call `[0x13B8394].vtbl+68` after last AVI (path from `0099C1E0`) | UNREAD name; site PROVEN |
| Keep that voice across Leave when `+41!=0`; only `vtbl+72(500)` | PROVEN skip; no player |
| Do not `StopMusic` / dtor audio on New Game | PROVEN native; C# has nothing to stop |
| Click / `SND_MENU_04` / `UI_CLICK` on Press Start | DISPROVEN / UNREAD |
| QI `IBasicAudio` on AVI graph | PARTIAL #9; not a frontend gate |
| Guarantee `Ended` or skip so `0042E98F` runs | PROVEN requirement |

## Do not invent

- Forest-loop or `MUSIC_SET_TITLE` as the `0042DED5` file.
- `Play2DSound UI_CLICK` on Enter / Press Start.
- Dialogue UI on the frontend.
- Stopping audio on New Game Leave because “we’re leaving the menu.”
