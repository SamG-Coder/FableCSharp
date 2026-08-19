# Issue #9 vs HEAD — `WmvPlayer` never QIs `IBasicAudio`

Investigation only. No `src/` / `tests/` edits.

GitHub: [WmvPlayer never QIs IBasicAudio; intro voice-over is silent #9](https://github.com/SamG-Coder/FableCSharp/issues/9)
(open, 2026-08-18). Native `00A3B9D0` QIs `IBasicAudio` at `0x12AA054`.

HEAD: `master` `ee084901e8212814d4ca7df599180117f9be5cec`.

Status words: **STILL OPEN** / **FIXED** / **PARTIAL**.

---

## Verdict

**STILL OPEN vs HEAD.** The claimed defect is still true: live
`WmvPlayer.BuildGraph` never QIs `IBasicAudio`, never
`put_Volume`, and never stores an audio interface. Intro
voice-over (WMV stream on the FilterGraph, not a bank) has
no recovered player path.

`docs/status/README.md` already lists this leftover as
**PARTIAL** because the native IID / VA are recovered on
`RegionTravel` and a comment names the native QI list.
That is dump recovery, not a fix. GitHub #9 is still Open.

Do **not** invent a WAV / voice bank for
`intro_comp` / `dream_sequence_comp`. Voice is the WMV
audio pin through quartz `IBasicAudio` + default DirectSound.

| Claim | vs HEAD |
|---|---|
| Native `00A3B9D0` open QI includes `IBasicAudio` `0x12AA054` | **PROVEN** (ExeIndex + `PlayAviBasicAudioIidVa`) |
| C# `BuildGraph` QIs Control / Position / Event only | **PROVEN** |
| `RegionTravel.PlayAviBasicAudioIid` used by live player | **DISPROVEN** — declare only, 0 call sites |
| `PlayAviWave` is PCM / volume | **DISPROVEN** — Receive/WaitEx/Present log |
| Intro voice is `PlayMusic` / `MuteSounds` / a WAV bank | **DISPROVEN** as the voice path |
| Issue #9 fixed at this HEAD | **DISPROVEN** |

---

## What #9 asks

Native open (`00A3B9D0`) after `RenderFile`:

- `IMediaControl` `0x12AA094`
- `IMediaPosition` `0x12AA064`
- `IMediaSeeking` `0x12A9A04`
- `IMediaEvent` `0x12AA084`
- **`IBasicAudio` `0x12AA054`**

Not `IVideoWindow` / `GetCurrentImage`. Pixels are
`IMediaSample::GetPointer` at `00A3B730`. Audio is the
WMV stream through `IBasicAudio` + the graph default
DirectSound renderer. Cinematic voice is that stream
(`intro_comp` / `dream_sequence_comp`). First-seen
`PlayMusic MUSIC_SET_NULL` only kills the music bank.
`MuteSounds` first-seen arg is FALSE.

C# (issue text): `BuildGraph` QIs Control / Position /
Event only. `PlayAviBasicAudioIid` is unused.
`RenderFile` may add a default audio renderer, but
volume/balance never go through `IBasicAudio`. If the
audio pin is left unrendered, the graph is video-only.

---

## `WmvPlayer.BuildGraph` QI list (HEAD)

Class doc is honest: it names the three live QIs and
omits `IBasicAudio` / `IMediaSeeking`.

```7:19:src/Fable.Game/WmvPlayer.cs
/// <c>00A3B9D0</c> DirectShow path when the rewritten
/// name ends <c>.wmv</c> / <c>.asf</c>. CoCreate
/// <c>CLSID_FilterGraph</c> (<c>0x12AB174</c>) +
/// <c>IID_IGraphBuilder</c> (<c>0x12A9934</c>),
/// <c>AddFilter</c> a renderer (<c>00A3B510</c>),
/// <c>RenderFile</c> vtbl+52, QI
/// <c>IMediaControl</c> / <c>IMediaPosition</c> /
/// <c>IMediaEvent</c>, <c>put_CurrentPosition(0)</c>
/// then <c>Run</c> vtbl+28 up to 50 times
/// (<c>00A3B130</c>). Samples are
/// <c>IMediaSample::GetPointer</c>, not
/// <c>IMFSample</c>. EOF is <c>EC_COMPLETE</c> (1).
```

Comment in `BuildGraph` lists native Seeking + BasicAudio.
The next statements do not QI them. Live store is three
RCW casts. No `IBasicAudio` field, no
`interface IBasicAudio`, no `put_Volume`.

```357:380:src/Fable.Game/WmvPlayer.cs
        // 00A3B9D0: alloc 00A3B510 renderer, AddFilter
        // vtbl+12 name 0x129D1AC, RenderFile vtbl+52
        // when the path is .wmv/.asf. Open QI is
        // IMediaControl / IMediaPosition /
        // IMediaSeeking / IMediaEvent / IBasicAudio
        // — not IVideoWindow. Pixels are
        // IMediaSample::GetPointer in 00A3B730.
        _renderer = new TextureRenderer(OnGetPointerSample);
        hr = _graph.AddFilter(_renderer, RegionTravel.PlayAviFilterName);
        // ...
        hr = _graph.RenderFile(path, null);
        // ...
        _control = (IMediaControl)_graph;
        _position = (IMediaPosition)_graph;
        _events = (IMediaEvent)_graph;
```

Fields on the player (`WmvPlayer.cs` 151–156): `_graph`,
`_control`, `_events`, `_position`, `_renderer`. TearDown
nulls Event / Position / Control only (`619–622`).

`IidName` can label `56a868b3-…` as `"IBasicAudio"` if
quartz QIs the **texture renderer** during CaptureQi.
That is observation of inbound filter QI, not the graph
open QI #9 wants.

`GraphSummary` joins `QueryFilterInfo` names after
`RenderFile`. Nothing requires an audio renderer, nothing
calls `IGraphBuilder.Render` on a leftover WMA pin.

---

## `RegionTravel.PlayAviBasicAudioIid` usage

Declared. Zero consumers.

```454:455:src/Fable.Game/RegionTravel.cs
    public static readonly Guid PlayAviBasicAudioIid =
        new("56a868b3-0ad4-11ce-b03a-0020af0ba770");
```

```513:517:src/Fable.Game/RegionTravel.cs
    public const uint PlayAviMediaControlIidVa = 0x012AA094;
    public const uint PlayAviMediaPositionIidVa = 0x012AA064;
    public const uint PlayAviMediaSeekingIidVa = 0x012A9A04;
    public const uint PlayAviMediaEventIidVa = 0x012AA084;
    public const uint PlayAviBasicAudioIidVa = 0x012AA054;
```

Workspace grep of `PlayAviBasicAudioIid` / `PlayAviBasicAudioIidVa`:
only those two `RegionTravel` lines. Not `WmvPlayer`, not
tests, not `PlayAviFromExe`.

`WorldSceneTests` locks Control + Event GUIDs
(`387–388`) and never asserts BasicAudio /
`0x012AA054`. `PlayAvi_rewrites_xmv_to_installed_wmv_and_blocks`
locks GetPointer video, unload, `MuteSounds false`. It
does not inspect `LastGraph` for DirectSound / audio
renderer and does not QI `IBasicAudio`.

Native comment on the copy-sample constant matches #9:

```315:318:src/Fable.Game/RegionTravel.cs
    /// existing game backbuffer. Open QI is
    /// IMediaControl/Position/Seeking/Event/BasicAudio
    /// — not <c>IVideoWindow</c> /
    /// <c>GetCurrentImage</c>.
```

Voice vs bank (recovered, not the missing QI):

```139:140:src/Fable.Game/RegionTravel.cs
    public const string IntroPlayAvi = "dream_sequence_comp.xmv";
    public const string IntroPlayAviRewritten = "dream_sequence_comp.wmv";
```

```597:597:src/Fable.Game/RegionTravel.cs
    public const string IntroPlayMusic = "PlayMusic MUSIC_SET_NULL";
```

```672:673:src/Fable.Game/RegionTravel.cs
    public const bool FirstSeenMuteSoundsDoesNotYield = true;
    public const bool FirstSeenMuteSoundsArgIsFalse = true;
```

Startup third slot is `Data\Video\intro_comp.xmv` (same
`WmvPlayer` path). `PlayAviWave` is not PCM:

```6:8:src/Fable.Core/PlayAviWave.cs
/// Observation-only Receive vs WaitEx vs Present
/// log. Does not pace playback.
```

---

## Related leftover (not #9 close)

Native open also QIs `IMediaSeeking` (`0x12A9A04`).
HEAD uses `IMediaPosition.put_CurrentPosition(0)` only.
`proofs/audio-frontend` already classifies that as a
separate PARTIAL that does **not** gate `0042E98F`.
#9 is silent AVI voice, not frontend bind.

`IBasicAudio` does **not** gate `0042E98F`. Missing
`Ended` / stuck graph does.

---

## Leftover

- Live `BuildGraph` QI of `RegionTravel.PlayAviBasicAudioIid`
  and `put_Volume(0)` (DirectShow 0 = 0 dB).
- Confirm `GraphSummary` actually contains a default
  audio / DirectSound renderer after `RenderFile` of
  `dream_sequence_comp.wmv` / `intro_comp.wmv`.
- If the WMA pin is unrendered, `Render()` that output
  pin onto the default DirectSound filter.
- Test lock: IID `56a868b3-…` / VA `0x012AA054` used
  after successful `RenderFile`.
- Do **not** invent a WAV / voice bank, `MUSIC_SET_*`
  voice, or PCM decode outside the graph.

---

## Proposed next step

In `WmvPlayer.BuildGraph`, after `RenderFile` succeeds,
QI `_graph` for `RegionTravel.PlayAviBasicAudioIid`,
store `IBasicAudio`, `put_Volume(0)`. Keep
`PlayMusic MUSIC_SET_NULL` and `MuteSounds FALSE` as
recovered. If `GraphSummary` has no audio renderer,
`Render()` the unrendered audio pin — still quartz,
not a new bank. Add the IID assert next to Control /
Event in `WorldSceneTests`.
