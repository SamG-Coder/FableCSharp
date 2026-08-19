# Issue #15 vs HEAD — Init Sound / PlayMusic / Mute record-only

Investigation only. No `src/` or `tests/` edits.

GitHub: [SamG-Coder/FableCSharp#15](https://github.com/SamG-Coder/FableCSharp/issues/15)
(open, 0 comments, filed 2026-08-18).

HEAD: `ee084901e8212814d4ca7df599180117f9be5cec`
(`Add CCreatureNavigationDef after CTimeAppearanceFadeDef during Init Thing Components.`).
No later commit implements `00417A58` or an audio device.

Status words: **STILL OPEN** / **FIXED** / **PARTIAL** /
**PROVEN** / **UNREAD** / **LEFTOVER** / **MATCH**.

Question: are Init Sound `00417A58` and
`AudioRuntime` `PlayMusic` / `Mute` still record-only
at HEAD, or did #15 close?

Do **not** invent sound banks, bank file names, or a
WAV for intro voice. Intro voice is issue #9
(`IBasicAudio` on the WMV graph), not this path.

Authority: issue body; host
`src/Fable.Game/EngineLifecycle.cs`
`InitGameStages` / `EnterGame`;
`src/Fable.Game/Scripting/ExecutionContext.cs`
`AudioRuntime`;
`src/Fable.Game/Scripting/GlobalDispatcher.cs`;
`src/Fable.Game/ScriptRuntime.cs`
`LookupMusic` / `IScriptHost.PlayMusic` /
`IScriptHost.MuteSounds`;
`src/Fable.Game/ScriptCommandMap.cs`;
`src/Fable.Game/RegionTravel.cs`;
`proofs/00417A58-init-sound-body/README.md`;
`proofs/audio-initgame-first/README.md`;
`proofs/script-playmusic/README.md`.

---

## Verdict vs HEAD

**STILL OPEN.**

Every claim in the issue body still matches HEAD.

`EnterGame` still `Note`s `"Init Sound"` /
`0x00417A58` and has **no** `if (name == "Init Sound")`
arm. No device, no register bind, no mixer.

`AudioRuntime.PlayMusic` / `Mute` still assign
fields only. `LookupMusic` can return a
`data/Sound/*.ogg` path; nothing plays it.
`Play2DSound` / `PlaySound` still append
`ScriptAudioInstance` records.
`IScriptHost.PlayMusic` is still `LastMusic = track`.

Done-list items **2** and **3** are already true
(constraints, not new work). Item **1** is not.

| Issue claim | HEAD | Class |
|---|---|---|
| `InitGameStages` has `("Init Sound", 0x00417A58)` | yes | **MATCH** |
| `EnterGame` Notes the stage and continues | yes — no body | **STILL OPEN** |
| No device / bank bind / mixer | yes | **STILL OPEN** |
| `PlayMusic` / `Mute` record-only | yes | **STILL OPEN** |
| Dispatcher returns **Proven** for both | yes | **STILL OPEN** |
| `LookupMusic` NULL → null; path stored, not played | yes | **MATCH** kill; play **LEFTOVER** |
| First-seen `PlayMusic MUSIC_SET_NULL` | yes | **PROVEN** leftover |
| Mute first-seen arg FALSE | yes | **PROVEN** leftover |
| `vtbl+2664` body UNREAD | yes (`ScriptLayer`) | **MATCH** item 3 |
| `vtbl+2784` player / destination | none | **UNREAD** |
| Collapse with #9 / invent intro WAV | not done | **MATCH** |

Not **FIXED**: no destination for `vtbl+2784` / mute.
Not **PARTIAL** as an issue close: recovered NULL-kill
and FALSE mute were already in the filing. The work
item is still undone.

---

## 1. EngineLifecycle Init Sound arm

`InitGameStages` twelfth name (after Create Players):

```715:731:src/Fable.Game/EngineLifecycle.cs
    public static readonly (string Stage, uint Apply)[] InitGameStages =
    [
        ("Init Thing Components", InitThingComponentsFn),
        ("Init Definition Manager", InitDefinitionManagerFn),
        ("Init Graphics", 0x00416C8A),
        // 004168DC sibling; name is logged inside the fn.
        ("Init Fonts", FontFile.InitFontsFn),
        ("Init Subtitled Message", InitSubtitledMessageFn),
        ("Init Conversation Attitude", InitConversationAttitudeFn),
        ("Init Player Manager", InitPlayerManagerFn),
        ("Init Player Interface", 0x004473A0),
        ("Init World", 0x0041735A),
        ("Init Display Engine", 0x00417418),
        ("Create Players", 0x004166A8),
        ("Init Sound", 0x00417A58),
        ("Load Particles", 0x004174F1),
    ];
```

`EnterGame` walk: every stage is Noted. Siblings have
bodies. Init Sound does not.

```4064:4138:src/Fable.Game/EngineLifecycle.cs
        foreach (var (name, apply) in InitGameStages)
        {
            if (name == "Init Conversation Attitude")
                Note(0x0041863D, "InitGame", "InitGame", "Adding Console Variables");
            if (name == "Init World")
                Note(IniActivateQuestRegister, "InitGame", "Ini",
                    "00419D90 ActivateQuest");
            Note(apply, name, "InitGame", name);
            // … Init Thing Components / Definition Manager /
            // Graphics / Fonts / Subtitled / Attitude /
            // Player Manager / Display / Player Interface /
            // World / Create Players …
            if (name == "Create Players")
                CreatePlayers();
            if (name == "Load Particles")
                Note(SkipParticlesVa, "InitGame", "InitGame",
                    $"013B8648={SkipParticlesFirstSeen} run 004174F1");
        }
```

No `if (name == "Init Sound")`. Trace lists the name
as a completed Init Game stage. Native `004184BD`
actually `call 00417A58` at `00418886` (register
loops, not play). Host leftover is that register, not
another Note.

---

## 2. AudioRuntime PlayMusic / Mute

```576:632:src/Fable.Game/Scripting/ExecutionContext.cs
public sealed class AudioRuntime
{
    public string? Music { get; private set; }
    public string? MusicResource { get; private set; }
    // …
    public bool Muted { get; private set; }
    public readonly List<ScriptAudioInstance> Instances = [];
    public readonly List<string> Cached = [];
    // …

    public void PlayMusic(string track, string? resource = null)
    {
        Music = track;
        MusicResource = resource;
    }
    // CacheMusic / PlaySound append lists …
    public void Mute(bool mute) => Muted = mute;
}
```

No device, no vtbl, no mix. **MATCH** issue quote.

Dispatcher still stamps **Proven** on the step:

```15:21:src/Fable.Game/Scripting/GlobalDispatcher.cs
        if (Eq(v, "PlayMusic"))
        {
            var track = line.Arg(0);
            if (track.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ctx.Audio.PlayMusic(track, ctx.Runtime.LookupMusic(track));
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, track);
        }
```

```71:76:src/Fable.Game/Scripting/GlobalDispatcher.cs
        if (Eq(v, "MuteSounds"))
        {
            ctx.Audio.Mute(!ScriptLine.IsFalse(line.Arg(0)));
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                ctx.Audio.Muted ? "mute" : "unmute");
        }
```

`Play2DSound` / `PlaySound` call `PlaySound(...)` and
append `Instances` (same file, Proven continue/yield).
**MATCH.**

---

## 3. LookupMusic / IScriptHost

```163:181:src/Fable.Game/ScriptRuntime.cs
    /// <summary>
    /// <c>009E5120</c> music map analog: resolve
    /// <c>MUSIC_SET_*</c> / track to <c>data/Sound/*.ogg</c>.
    /// Miss returns null (native still calls vtbl+2784
    /// with id 0). Lug decode UNREAD.
    /// </summary>
    public string? LookupMusic(string track)
    {
        if (track.Length == 0 || _install is null)
            return null;
        var dir = _install.SoundDirectory;
        if (!Directory.Exists(dir))
            return null;
        var stem = track;
        const string prefix = "MUSIC_SET_";
        if (stem.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            stem = stem[prefix.Length..];
        if (stem.Equals("NULL", StringComparison.OrdinalIgnoreCase))
            return null;
```

`MUSIC_SET_NULL` → null. **MATCH** done item 2.
A later `MUSIC_SET_OAKVALE` can resolve a real
`.ogg` into `MusicResource` and still never play.

```626:626:src/Fable.Game/ScriptRuntime.cs
    void IScriptHost.PlayMusic(string track) => LastMusic = track;
```

```767:773:src/Fable.Game/ScriptRuntime.cs
    /// <summary>
    /// <c>00CC7258</c>: <c>00CBEE0C</c> IsFalse →
    /// <c>vtbl+2664(0)</c>, else <c>(1)</c>.
    /// <c>jmp 00CC8464</c>. No yield. Body UNREAD.
    /// </summary>
    void IScriptHost.MuteSounds(string arguments) =>
        SoundsMuted = !ScriptCommand.IsFalseArg(ScriptInterpreter.FirstToken(arguments));
```

`LastMusic` setter forwards to `Audio.PlayMusic`.
Still a field write.

---

## 4. Command map / first-seen facts (already recovered)

```212:216:src/Fable.Game/ScriptCommandMap.cs
        Spec("PlayMusic", 0x00CC8EAC, 0x00CBF7FE, "track",
            ScriptReturn.CompleteNow, new CommandParity(
                CommandStatus.Proven, CommandStatus.Proven, CommandStatus.Proven,
                CommandStatus.Proven, CommandStatus.Partial),
            "009E5120 map then vtbl+2784; jmp 00CD17FD; Sound/*.ogg; player UNREAD"),
```

```348:350:src/Fable.Game/ScriptCommandMap.cs
        Spec("MuteSounds", 0x00CC7258, 0, "IsFalse?",
            ScriptReturn.CompleteNow, CommandParity.ScriptLayer,
            "vtbl+2664; jmp 00CC8464; apply body UNREAD"),
```

Overall: PlayMusic **Partial** (Runtime Partial;
Apply is marked Proven while the player is UNREAD).
MuteSounds **Partial** (`ScriptLayer`). Tests assert
`StatusOf("MuteSounds") == Partial` and that a new-game
trace has `PlayMusic` **not** `Unread`.

That is the “looks implemented, still silent” hazard
in the issue. Native contrast unchanged:

- PlayMusic `00CC8EAC` / helper `00CBF7FE` →
  `009E5120` → `vtbl+2784` → `jmp 00CD17FD`
- MuteSounds `00CC7258` → `vtbl+2664` →
  `jmp 00CC8464` (body **UNREAD**)

First-seen leftovers (keep):

| Fact | Host | Class |
|---|---|---|
| Intro line 0 `PlayMusic MUSIC_SET_NULL` | `RegionTravel.IntroPlayMusic` | **PROVEN** |
| NULL kill | `LookupMusic` → null | **PROVEN** |
| `vtbl+2784` with id 0 kills music bank | comment + `PlayMusicVtbl = 2784` | **PROVEN** as lookup; player **UNREAD** |
| Mute first-seen `false` unmutes | `FirstSeenMuteSoundsArgIsFalse` | **PROVEN** |
| `SoundsMuted` after that line | `false` (`WorldSceneTests`) | **PROVEN** flag |
| `vtbl+2664` apply body | unread | **UNREAD** |

`docs/status/README.md` still lists
`MuteSounds` apply as PARTIAL.

---

## 5. Native leftover (do not invent banks)

`proofs/00417A58-init-sound-body`: first-seen child of
`00417A58` is **register**, not play, not graphic-bank
Open. After Leave `[0x13B8394]` is live. First audio
`E8` is `009919C0` (`"Registering Localised Sound
Bank"`). Atmos is `00991C10`. Tail stores
`[game+16] = 00991840(1)`.

No `"Opening … Sound Bank"` string. Do **not** add
`OpenSoundBank()` or guessed bank files.

`00A01A4F` / `"Sound Bank: Init Symbols"` is nested
under a nonempty `009919C0`, not a second Init Game
name. First-seen `00A39010` is Init Subtitled
(`004CDB46`), not this stage.

---

## Leftover at HEAD

1. `EnterGame` Note-only `"Init Sound"` (done item 1).
2. No `[0x13B8394]` gate, no `00415550` locale /
   `MAIN_SOUND_SETUP`, no `009919C0` / `00991C10`
   register, no `[game+16]`.
3. `AudioRuntime` field writes only; no `vtbl+2784`
   destination.
4. `LookupMusic` path unused by a player.
5. `Play2DSound` / `PlaySound` instance list only.
6. Mute bool only; `vtbl+2664` body **UNREAD**.
7. Dispatcher step status **Proven** vs overall
   Partial / player UNREAD.

Not leftover (keep): NULL kill; first-seen mute
FALSE; no invented intro WAV; no merge with #9.

---

## Proposed next step

Pick **one** of the issue’s done item 1 forks. Do not
do both as a fake close.

**A (honest traces).** Stop treating Init Sound as a
ran stage: drop the name from the completed Note walk,
or Note it as leftover/unread with no apply. Keep
PlayMusic / Mute record-only until a destination
exists. Do not change dispatcher Proven unless the
trace contract is updated.

**B (register destination).** Implement the **proven**
`00417A58` register walk only: locale `00415550` →
def lookup `004196B2` → `009919C0` / `00991C10` insert
into `audio+48`, then `00991840(1)` → `[game+16]`.
Give `vtbl+2784` / mute that object as a destination.
Do **not** invent bank Open, `.ogg` playback, or
wchar prefixes (`0x122F3D0` / `0x122F4EC` still
**UNREAD**). Leave `vtbl+2664` body **UNREAD** until
it is read.

In either fork:

- Keep `MUSIC_SET_NULL` → null.
- Keep first-seen `MuteSounds false`.
- Do not invent a WAV for father/wake voice (#9).

---

## Sources

- https://github.com/SamG-Coder/FableCSharp/issues/15
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\src\Fable.Game\Scripting\ExecutionContext.cs`
- `C:\FableCSharp\src\Fable.Game\Scripting\GlobalDispatcher.cs`
- `C:\FableCSharp\src\Fable.Game\ScriptRuntime.cs`
- `C:\FableCSharp\src\Fable.Game\ScriptCommandMap.cs`
- `C:\FableCSharp\src\Fable.Game\RegionTravel.cs`
- `C:\FableCSharp\docs\runtime\COMMAND_MAP.generated.md`
- `C:\FableCSharp\docs\status\README.md`
- `C:\FableCSharp\proofs\00417A58-init-sound-body\README.md`
- `C:\FableCSharp\proofs\audio-initgame-first\README.md`
- `C:\FableCSharp\proofs\script-playmusic\README.md`
