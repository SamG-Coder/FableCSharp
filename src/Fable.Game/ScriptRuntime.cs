using System.Numerics;
using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Text;
using Fable.Formats.Tng;
using Fable.Game.Scripting;

namespace Fable.Game;

/// <summary>
/// Orchestrator: quest/fiber scheduler + persist +
/// CCutsceneDef children. S_QNOVI is a
/// <see cref="QuestInstance"/>, not a command list.
/// </summary>
public sealed class ScriptRuntime : IScriptHost, IScriptTrace
{
    public ScriptBank? Bank { get; private set; }
    public float DtAtPlus8 { get; private set; }
    private float _nativeClock;
    private bool _nativeClockSet;
    public float FadeDuration { get; private set; }
    public float FadeParam { get; private set; }
    public float FadeElapsed { get; private set; }
    public float FadeRemaining { get; private set; }
    public bool FadeActive { get; private set; }
    public bool FadeLocked { get; private set; }
    public bool FadeRising { get; private set; }
    public bool FadeFalling { get; private set; }
    public (byte R, byte G, byte B, byte A) FadeColor { get; private set; }
    /// <summary>
    /// <c>004348D0</c> fraction. Draw
    /// <c>006496BC</c> uses this * 255 as the
    /// overlay alpha.
    /// </summary>
    public float OverlayAlpha => OverlayFraction();
    public byte OverlayAlphaByte =>
        (byte)Math.Clamp((int)(OverlayFraction() * RegionTravel.FadeAlphaScale), 0, 255);
    public string? LastMusic
    {
        get => Audio.Music;
        private set => Audio.PlayMusic(value ?? "");
    }
    public string? LastAvi { get; private set; }
    public string? AviRelativePath { get; private set; }
    public string? AviFile { get; private set; }
    public bool AviPlaying { get; private set; }
    public int AviWidth => _avi?.Width ?? 0;
    public int AviHeight => _avi?.Height ?? 0;
    public byte[]? AviRgba => _avi?.Rgba;
    public int AviFrameSerial => _avi?.FrameSerial ?? 0;
    public bool SoundsMuted
    {
        get => Audio.Muted;
        private set => Audio.Mute(value);
    }
    public int TimeCode { get; set; }
    public float LastGamePause { get; set; }
    public string? ActiveCutscene => _interpreters.Count == 0 ? null : _interpreters[^1].Name;
    public ScriptInterpreter? ActiveInterpreter =>
        _interpreters.Count == 0 ? null : _interpreters[^1];
    public string CameraName => _camera?.ActiveName ?? "";
    public IReadOnlyList<ScriptInterpreter> Interpreters => _interpreters;
    public IReadOnlyDictionary<string, string> NamedScripts => _named;
    public RuntimeTrace Trace { get; } = new();
    public int Frame { get; private set; }
    public float Time { get; private set; }
    public BindingKind StartNewGameFactoryKind => BindingKind.ProvenGeneric;
    public BindingKind StartNewGameFiberKind => BindingKind.ProvenGeneric;
    public int WaitActiveDialogCount { get; internal set; }
    public IReadOnlyList<string> PreloadedCameras => CameraSys.Preloaded;
    public string? LastCameraPause { get; internal set; }
    public IReadOnlyList<ScriptLookToThing> LookToThings => World.LookToThings;
    public float TimeOfDayHours { get; internal set; }
    public float TimeOfDayFraction { get; internal set; }
    public string ActiveQuestName => _quests.Count == 0 ? "" : _quests[^1].Name;
    public FiberState? ActiveFiber => Scheduler.Fibers.LastOrDefault();
    public IReadOnlyList<ThingInstance> Things => _things;
    public ScriptedCamera? Camera => _camera;
    public ScriptBindings Bindings { get; } = new();
    public ScriptArguments Arguments { get; } = new();
    public PersistStore Persist { get; } = new();
    public FlagStore Flags { get; } = new();
    public ScriptScheduler Scheduler { get; } = new();
    public CameraRuntime CameraSys { get; } = new();
    public AudioRuntime Audio { get; } = new();
    public DialogueRuntime Dialogue { get; } = new();
    public AnimationRuntime Animation { get; } = new();
    public MovementRuntime Movement { get; } = new();
    public WorldRuntime World { get; } = new();
    public IReadOnlyList<QuestInstance> Quests => _quests;

    private readonly Dictionary<string, string> _named = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ScriptFiber> _fibers = [];
    private readonly List<ScriptInterpreter> _interpreters = [];
    private readonly List<QuestInstance> _quests = [];
    private List<ThingInstance> _things = [];
    private ScriptedCamera? _camera;
    private GameInstall? _install;
    private Dictionary<string, TextRecord>? _textLines;
    private WmvPlayer? _avi;
    private int _questId;
    private int _cutsceneId;

    public IReadOnlyList<ScriptTeleport> Teleports => World.Teleports;
    public IReadOnlyDictionary<string, Vector3> ActorPositions => World.Positions;
    public IReadOnlyList<ScriptAnimation> Animations => Animation.Plays;
    public IReadOnlyList<ScriptSpeech> Speeches => Dialogue.Speeches;
    public IReadOnlyList<ScriptInteractiveSpeech> InteractiveSpeeches => Dialogue.Interactive;
    public IReadOnlyList<ScriptDialogSpeech> DialogSpeeches => Dialogue.Dialogs;
    public IReadOnlyList<ScriptWaitTask> WaitTasks => _waits;
    public IReadOnlyList<ScriptSneakTo> SneakTos => Movement.Sneaks;
    public IReadOnlyList<ScriptWalkTo> WalkTos => Movement.Walks;
    public IReadOnlyList<ScriptCombatAnimation> CombatAnimations => Animation.Combat;
    public IReadOnlyList<ScriptCreate> Creates => World.Creates;
    public IReadOnlyList<string> Removes => World.Removes;
    public IReadOnlyList<ScriptDialogAdSpeech> DialogAdSpeeches => Dialogue.DialogAds;
    public IReadOnlyList<ScriptLookInDirection> LookInDirections => World.Looks;

    private readonly List<ScriptWaitTask> _waits = [];

    public static ScriptRuntime Detached() => new();

    public ScriptExecutionContext BindInterpreter(ScriptInterpreter interpreter)
    {
        if (interpreter.State.InstanceId == 0)
            interpreter.State.InstanceId = ++_cutsceneId;
        return new ScriptExecutionContext(
            this, Bindings, Arguments, Persist, Flags, CameraSys, Audio,
            Dialogue, Animation, Movement, World, interpreter.State);
    }

    public ThingInstance? FindThingByName(string name)
    {
        if (name.Length == 0)
            return null;
        foreach (var thing in _things)
        {
            if (thing.ScriptName is not null &&
                thing.ScriptName.Equals(name, StringComparison.OrdinalIgnoreCase))
                return thing;
        }

        return Bindings.Resolve(name)?.Thing;
    }

    public void Load(ScriptBank bank, GameInstall? install = null)
    {
        Bank = bank;
        _install = install;
    }

    /// <summary>
    /// <c>lang/English/text.big</c> UTF-16 lookup by
    /// <c>TEXT_*</c> name. Speak stores the ID even
    /// when the body is missing.
    /// </summary>
    public string? LookupText(string id)
    {
        return LookupTextRecord(id)?.Body;
    }

    public TextRecord? LookupTextRecord(string id)
    {
        if (id.Length == 0)
            return null;
        EnsureText();
        return _textLines is not null && _textLines.TryGetValue(id, out var record)
            ? record
            : null;
    }

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
        foreach (var ext in new[] { ".ogg", ".OGG" })
        {
            var path = Path.Combine(dir, stem + ext);
            if (File.Exists(path))
                return path;
        }

        foreach (var file in Directory.EnumerateFiles(dir, "*.ogg"))
        {
            if (Path.GetFileNameWithoutExtension(file)
                    .Equals(stem, StringComparison.OrdinalIgnoreCase))
                return file;
        }

        return null;
    }

    private void EnsureText()
    {
        if (_textLines is not null || _install is null || !File.Exists(_install.TextBigPath))
            return;
        var map = new Dictionary<string, TextRecord>(StringComparer.OrdinalIgnoreCase);
        using var big = BigArchive.Open(_install.TextBigPath);
        foreach (var bank in big.SubBanks)
        {
            foreach (var entry in big.ReadEntries(bank))
            {
                if (entry.Name.Length == 0)
                    continue;
                map[entry.Name] = TextPayload.ReadRecord(big.Read(entry));
            }
        }

        _textLines = map;
    }

    public void AddThing(ThingInstance thing)
    {
        _things.RemoveAll(t =>
            t.ScriptName is not null &&
            thing.ScriptName is not null &&
            t.ScriptName.Equals(thing.ScriptName, StringComparison.OrdinalIgnoreCase));
        _things.Add(thing);
    }

    public void RemoveThing(string name)
    {
        if (name.Length == 0)
            return;
        _things.RemoveAll(t =>
            t.ScriptName is not null &&
            t.ScriptName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public void BindScene(IEnumerable<ThingInstance> things, ScriptedCamera? camera)
    {
        _things = things as List<ThingInstance> ?? things.ToList();
        _camera = camera;
        ThingInstance? hero = null;
        foreach (var thing in _things)
        {
            Bindings.BindSceneThing(thing);
            if (IsHeroThing(thing) &&
                (hero is null || PreferHero(thing, hero)))
                hero = thing;
        }

        Bindings.BindHero(hero);
        if (hero is { PositionX: not null })
        {
            var pos = RegionTravel.PositionOf(hero);
            World.Positions[ScriptBindings.HeroAlias] = pos;
            if (hero.ScriptName is { Length: > 0 } name)
                World.Positions[name] = pos;
        }
        _ = Bindings.DrainChanges();
    }

    /// <summary>
    /// No-save Lookout is <c>CREATURE_HERO</c>
    /// <c>ScriptName=Hero</c>. Kid 4300 is only
    /// the Oakvale intro Thing.
    /// </summary>
    private static bool IsHeroThing(ThingInstance thing)
    {
        if (thing.ScriptName is { Length: > 0 } name &&
            (name.Equals(ScriptBindings.HeroAlias, StringComparison.OrdinalIgnoreCase) ||
             name.Equals(EngineLifecycle.HeroScriptName, StringComparison.OrdinalIgnoreCase)))
            return true;
        return thing.DefinitionType is RegionTravel.AdultCreature
            or RegionTravel.TweenCreature
            or RegionTravel.KidCreature;
    }

    private static bool PreferHero(ThingInstance candidate, ThingInstance current)
    {
        static int Rank(ThingInstance t)
        {
            if (t.DefinitionType == RegionTravel.AdultCreature)
                return 3;
            if (t.DefinitionType == RegionTravel.TweenCreature)
                return 2;
            if (t.DefinitionType == RegionTravel.KidCreature)
                return 1;
            return 0;
        }

        return Rank(candidate) > Rank(current);
    }

    /// <summary>
    /// <c>00CB8230</c> name record. Factory body for
    /// <c>NOVI_LiveFather</c> is <c>00DAC2C0</c> →
    /// <c>00DB86B0</c> → <c>00CBFB7D(cutscene)</c>.
    /// </summary>
    public void RegisterNamedScript(string scriptName, string cutsceneName) =>
        _named[scriptName] = cutsceneName;

    /// <summary>
    /// Recovered name-table + persist + S_QNOVI fiber.
    /// <see cref="StartNewGame"/> only calls this — it does
    /// not mention Oakvale cutscene strings.
    /// </summary>
    public void InstallRecoveredBindings()
    {
        foreach (var factory in ScriptFactoryTable.Recovered)
            RegisterNamedScript(factory.ScriptName, factory.CutsceneName);
        Persist.InstallRecovered();
        foreach (var fiber in ScriptFiberTable.Recovered)
        {
            CreateFiber(fiber.Name, fiber.PersistField);
            var quest = new QuestInstance(++_questId, fiber.Name, fiber.PersistField);
            var state = Scheduler.Create(fiber.Name, fiber.PersistField);
            quest.AttachFiber(state);
            _quests.Add(quest);
        }
    }

    /// <summary>
    /// <c>004B4260</c> QuestManager: Activate Quest.
    /// <c>00CB5AD0</c> name lookup on
    /// <c>QuestFactoryTable</c>, then
    /// <c>004BB720</c> / <c>004B3CE0</c>
    /// factory + run + fiber
    /// <c>00A447D0</c>. Oakvale factory
    /// <c>00DBEF70</c> binds persist
    /// <c>00DAADA0</c> and <c>00DABAC0</c>
    /// name-table; it does not load
    /// StartOakVale or run <c>00DBDE40</c>
    /// until the map-wait is ready.
    /// No-save Pump must not call this
    /// for <c>Q_NewOakValeIntro</c>.
    /// </summary>
    public QuestInstance ActivateQuest(string name, bool persistent = false)
    {
        var persist = persistent ? name : null;
        CreateFiber(name, persist);
        var quest = new QuestInstance(++_questId, name, persist);
        var state = Scheduler.Create(name, persist);
        quest.AttachFiber(state);
        _quests.Add(quest);

        var factory = QuestFactoryTable.Find(name);
        if (factory is { } bind)
        {
            if (bind.Init == QuestFactoryTable.SunnyvaleInit)
                Persist.Install(PersistTable.Sunnyvale);
            quest.StartFactory(bind.Factory, bind.Run, bind.Init, bind.ScriptName);
            // 00CB7900 calls factory vtbl+12 then vtbl+4
            // (00CE75B0 / 00CE1A30 Main watcher via
            // 00CDD450). Not CCutsceneDef StartCutscene.
            if (bind.Factory == RegionTravel.IntroQuestFactory)
                BindSqnoviFactory();
        }

        return quest;
    }

    /// <summary>
    /// <c>00DAADA0</c> persist bind then
    /// <c>00DABAC0</c> <c>00CB8230</c> name
    /// table. Does not <c>E8 00DBDE40</c>
    /// (map-wait leftover).
    /// </summary>
    private void BindSqnoviFactory()
    {
        Persist.InstallRecovered();
        foreach (var row in ScriptFactoryTable.Recovered)
            RegisterNamedScript(row.ScriptName, row.CutsceneName);
    }

    public ScriptFiber CreateFiber(string name, string? persistField = null)
    {
        if (persistField is not null && Persist.TypeOf(persistField) == PersistKind.Unread)
            Persist.SetBool(persistField, false);
        var fiber = new ScriptFiber(name, persistField);
        _fibers.Add(fiber);
        return fiber;
    }

    public void ApplyPersist(string name, bool value) => Persist.SetBool(name, value);

    public bool PersistBool(string name) => Persist.Bool(name);

    public int PersistInt(string name) => Persist.Int32(name);

    public PersistKind PersistType(string name) => Persist.TypeOf(name);

    public IReadOnlyDictionary<string, PersistValue> PersistSlots => Persist.Slots;

    public IReadOnlyDictionary<string, bool> PersistFields =>
        Persist.Slots.ToDictionary(p => p.Key, p => p.Value.Bool, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// <c>004C97B0</c> / <c>00CB8960</c>: construct the
    /// named factory when the name is in the registry.
    /// Factory bodies (<c>00DAC2C0</c> / <c>00DAC420</c>)
    /// do not call <c>00CBFB7D</c>. Start is object
    /// <c>vtbl+4</c>. Only LiveFather <c>00DB86B0</c>
    /// first <c>00CBFB7D</c> is
    /// <c>CS_OAKVALE_INTRO_FATHER</c> (<c>00DB88F8</c>).
    /// </summary>
    public ScriptInterpreter? ActivateThing(ThingInstance thing)
    {
        if (thing.ScriptName is null)
            return null;
        return StartNamedScript(thing.ScriptName);
    }

    public void ActivateThings(IEnumerable<ThingInstance> things)
    {
        foreach (var thing in things)
            ActivateThing(thing);
    }

    public ScriptInterpreter? StartNamedScript(string scriptName)
    {
        if (!_named.TryGetValue(scriptName, out var cutscene))
            return null;
        if (string.IsNullOrEmpty(cutscene))
            return null;
        // Theresa 00DB97A0 first named work is
        // M_TriggerOutro; first 00CBFB7D is MEET
        // (00DB9B28), raid THERESA is 00DBB238.
        // DeadFather 00DB8300 uses 007E73F0, not
        // 00CBFB7D. Do not StartCutscene those at
        // construct.
        if (ScriptFactoryTable.Find(scriptName) is { ConstructStartsCutscene: false })
            return null;
        return StartCutscene(cutscene);
    }

    public ScriptInterpreter? StartCutscene(string cutsceneName)
    {
        if (FindInterpreter(cutsceneName) is { } existing)
            return existing;
        var def = Bank?.Find(cutsceneName);
        if (def is null)
            return null;
        var interpreter = new ScriptInterpreter(def.InstanceName, def.Commands);
        if (def.Vectors.Count > ScriptBank.LightDefVectorIndex)
            interpreter.BindLightTables(
                def.Vectors[ScriptBank.LightDefVectorIndex],
                def.Vectors.Count > ScriptBank.LightSceneVectorIndex
                    ? def.Vectors[ScriptBank.LightSceneVectorIndex]
                    : []);
        _interpreters.Add(interpreter);
        if (_quests.Count > 0)
            _quests[^1].StartChildCutscene(cutsceneName);
        interpreter.RunUntilYield(BindInterpreter(interpreter));
        return interpreter;
    }

    public ScriptInterpreter? FindInterpreter(string name) =>
        _interpreters.Find(item => item.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Resume a cutscene past frame/time/task waits.
    /// PlayAVI is skipped (DIK 1/57/28/62 analog) so
    /// fixtures can continue after the blocking apply.
    /// WaitOperation leftover polls go idle on the
    /// next tick (vtbl+104 / +1472) except WaitFlag,
    /// which leftover-polls the named byte until a
    /// SetFlag writes the expected value. Do not
    /// auto-complete flag waits.
    /// </summary>
    public int PumpUntilSettled(ScriptInterpreter intro, int maxUpdates = 10_000)
    {
        var n = 0;
        var flagStall = 0;
        while (n < maxUpdates && !intro.Finished && !intro.Blocked)
        {
            if (!intro.Yielded)
                break;
            if (intro.CurrentWaitKind == ExecutionKind.BlockPump)
                SkipAvi();
            var flagWait = intro.CurrentWaitKind == ExecutionKind.WaitOperation &&
                           Flags.IsWaiting(intro.State.WaitOperationId);
            if (intro.CurrentWaitKind == ExecutionKind.WaitOperation && !flagWait)
            {
                foreach (var op in Animation.ByActor.Values)
                    op.Complete = true;
                foreach (var task in Animation.Tasks.ByActor.Values)
                    task.MarkComplete();
                foreach (var op in Movement.ByActor.Values)
                    op.Complete = true;
                foreach (var task in Movement.Tasks.ByActor.Values)
                    task.MarkComplete();
                Dialogue.CompleteWait();
                CameraSys.CompleteWait();
            }

            Update(RegionTravel.GamePauseIncrement / RegionTravel.GamePauseScale);
            n++;
            if (flagWait && Flags.IsWaiting(intro.State.WaitOperationId))
            {
                flagStall++;
                if (flagStall > 8)
                    break;
            }
            else
                flagStall = 0;
        }

        return n;
    }

    public bool HasStarted(string cutsceneName) => FindInterpreter(cutsceneName) is not null;

    public bool ExecutedVerb(string cutsceneName, string verb) =>
        FindInterpreter(cutsceneName)?.ExecutedVerb(verb) ?? false;

    /// <summary>
    /// <c>00A44880</c>: store dt at <c>+8</c>, then
    /// <c>00A44660</c> resume. Unread waits re-yield.
    /// Does not write persist fields.
    /// </summary>
    public void Update(float dt)
    {
        if (dt < 0f)
            return;
        UpdateCore(_nativeClock + dt, dt);
    }

    /// <summary>
    /// Native <c>00A44880</c> input. <c>009E1BC0</c> returns seconds elapsed
    /// from the timer baseline, not a frame delta. Fiber <c>+8</c> receives
    /// that absolute clock; time-based subsystems receive only its difference
    /// from the preceding pump.
    /// </summary>
    public void UpdateAtClock(float clock)
    {
        if (clock < 0f)
            return;
        var dt = _nativeClockSet ? Math.Max(0f, clock - _nativeClock) : 0f;
        UpdateCore(clock, dt);
    }

    private void UpdateCore(float clock, float dt)
    {
        _nativeClock = clock;
        _nativeClockSet = true;
        Frame++;
        Time = clock;
        DtAtPlus8 = clock;
        foreach (var fiber in _fibers)
            fiber.DtAtPlus8 = clock;
        // 006286F0 owns the pump until it
        // returns. 00A44880 is stuck in that
        // apply — no fade tick, no other resume.
        if (AviPlaying)
        {
            TickAvi(dt);
            return;
        }

        TickFade(dt);
        TickAvi(dt);
        Movement.Tick(dt, World);
        Animation.Tick(dt, World);
        if (_fibers.Count == 0 && _interpreters.Count == 0)
            return;
        // 00A44880 applies once to each fiber. The previous host callback
        // resumed every interpreter from inside every fiber tick, producing
        // an N-fibers × M-interpreters cross-product and advancing cutscenes
        // several times during one rendered frame. Quest fibers and cutscene
        // interpreters are not interchangeable; tick the registered fibers,
        // then resume each yielded interpreter exactly once.
        if (Scheduler.Fibers.Count > 0)
        {
            Scheduler.Pump(clock, fiber =>
            {
                fiber.State = FiberRunState.Running;
                fiber.State = FiberRunState.Ready;
            });
        }

        foreach (var interpreter in _interpreters)
        {
            if (interpreter.Yielded && !interpreter.Blocked)
                interpreter.Resume(BindInterpreter(interpreter));
        }
    }

    /// <summary>
    /// <c>00434870</c> on the +188 fade record.
    /// Rising adds dt to elapsed until duration.
    /// Falling subtracts dt from remaining then
    /// clears +188.
    /// </summary>
    private void TickFade(float dt)
    {
        if (FadeRising)
        {
            FadeElapsed += dt;
            if (FadeElapsed >= FadeDuration)
            {
                FadeElapsed = FadeDuration;
                FadeRising = false;
            }

            return;
        }

        if (!FadeFalling)
            return;
        FadeRemaining -= dt;
        if (FadeRemaining > 0f)
            return;
        FadeRemaining = 0f;
        FadeActive = false;
        FadeFalling = false;
        FadeRising = false;
    }

    /// <summary>
    /// <c>004348D0</c>: rising elapsed/duration,
    /// falling remaining/param, else +188 → 1 else 0.
    /// Duration ≤ 0.0001 returns 1.
    /// </summary>
    private float OverlayFraction()
    {
        if (FadeRising)
        {
            if (FadeDuration <= RegionTravel.FadeAlphaEpsilon)
                return 1f;
            return FadeElapsed / FadeDuration;
        }

        if (FadeFalling)
        {
            if (FadeParam <= RegionTravel.FadeAlphaEpsilon)
                return 1f;
            return FadeRemaining / FadeParam;
        }

        return FadeActive ? 1f : 0f;
    }

    /// <summary>
    /// Generic engine startup: load bank, bind scene,
    /// install recovered factory/persist/fiber tables,
    /// activate TNG ScriptName. Oakvale facts live in
    /// <see cref="ScriptFactoryTable"/>, not here.
    /// </summary>
    public static ScriptRuntime StartNewGame(
        GameInstall install,
        IEnumerable<ThingInstance> things,
        ScriptedCamera? camera = null)
    {
        var list = things as IReadOnlyList<ThingInstance> ?? things.ToList();
        var runtime = new ScriptRuntime();
        runtime.Load(ScriptBank.Load(install), install);
        runtime.BindScene(list, camera);
        runtime.InstallRecoveredBindings();
        runtime.ActivateThings(list);
        return runtime;
    }

    void IScriptTrace.OnStep(RuntimeTraceStep step) => Trace.Add(step);

    internal string TraceWorldSnapshot()
    {
        if (World.Positions.Count == 0)
            return "";
        return string.Join(",", World.Positions.Select(p =>
            $"{p.Key}:{p.Value.X:0.##},{p.Value.Y:0.##},{p.Value.Z:0.##}"));
    }

    public void ApplyFadeOut(float seconds, float param) =>
        ((IScriptHost)this).FadeOut(seconds, param);

    public void ApplyFadeIn(float seconds, float param) =>
        ((IScriptHost)this).FadeIn(seconds, param);

    public void BeginAvi(string file) => ((IScriptHost)this).PlayAvi(file);

    void IScriptHost.PlayMusic(string track) => LastMusic = track;

    /// <summary>
    /// <c>008907E0</c> <c>vtbl+1488</c>: pack
    /// <c>(0,0,0,255)</c>, call <c>vtbl+1492</c> →
    /// <c>00434C00</c> (+188=1, +201=1, +192=seconds).
    /// Then <c>[+216]=1</c> lock. Overlay draw is
    /// <c>006496BC</c> gated on +188.
    /// </summary>
    void IScriptHost.FadeOut(float seconds, float param)
    {
        FadeDuration = seconds;
        FadeParam = param;
        FadeColor = (0, 0, 0, 255);
        if (FadeLocked)
            return;
        FadeActive = true;
        FadeLocked = true;
        FadeRising = true;
        FadeFalling = false;
        FadeElapsed = 0f;
        FadeRemaining = 0f;
    }

    /// <summary>
    /// Bare <c>FadeIn</c> is <c>vtbl+1496</c>
    /// <c>0088E4C0</c>: clear <c>[+216]</c> then
    /// <c>00434C90</c> (+201=0, +200=1). Next
    /// <c>00434870</c> tick can clear +188.
    /// </summary>
    void IScriptHost.FadeIn(float seconds, float param)
    {
        FadeDuration = seconds;
        FadeParam = param;
        FadeLocked = false;
        if (!FadeActive)
            return;
        FadeRising = false;
        FadeFalling = true;
        FadeRemaining = param;
    }

    void IScriptHost.UseCamera(string name) => BindCamera(name);

    void IScriptHost.NoLoadUseCamera(string name) => BindCamera(name);

    /// <summary>
    /// <c>00CC14B8</c>: thing <c>vtbl+72</c>
    /// (<c>004C7470</c>) walks components and calls
    /// <c>[comp.vtbl+68](name)</c>. CTCAnimationComplex
    /// <c>+68</c> is <c>00686920</c> <c>al=1</c> (not
    /// handled). Inner play <c>0070D580</c> is not on
    /// this path — record name+flags only.
    /// <c>[ebp-22]</c> ctor 1 at <c>00CBFD57</c> then
    /// <c>00CC186F</c> → <c>00CC5691</c> one
    /// <c>vtbl+28</c>.
    /// </summary>
    void IScriptHost.PlayAnimation(string? actor, string arguments)
    {
        var args = ScriptCommand.SplitArgs(arguments);
        var name = args.Length == 0 ? "" : args[0];
        var flags = ScriptCommand.ParsePlayAnimationFlags(arguments);
        Animation.Play(actor, name, flags.Flag1, flags.Flag2, flags.Flag3, flags.Flag4, flags.Flag5);
    }

    void IScriptHost.CameraPause(string arguments) => LastCameraPause = arguments;

    /// <summary>
    /// <c>00CC4678</c> → <c>vtbl+1892</c>
    /// <c>0089B780</c>: marker pos
    /// <c>004AA980</c> (<c>[handle+4].vtbl+24</c>),
    /// then <c>[thing+96].vtbl+124(pos)</c>.
    /// <c>00CC47B4</c> also reads yaw
    /// <c>004AAA40</c> into <c>0089BDF0</c>.
    /// Do not invent the unread heading write.
    /// </summary>
    void IScriptHost.Teleport(string? actor, string arguments)
    {
        var args = ScriptCommand.SplitArgs(arguments);
        var marker = args.Length == 0 ? "" : args[0];
        var thing = FindThingByName(marker);
        Vector3? position = thing is { PositionX: not null } ? RegionTravel.PositionOf(thing) : null;
        World.Teleport(actor, marker, position);
    }

    void IScriptHost.LookToThing(string? actor, string arguments) =>
        World.LookToThings.Add(new ScriptLookToThing(actor, arguments));

    /// <summary>
    /// <c>00CCA26D</c>: prefix <c>Data\Video\</c> then
    /// <c>vtbl+1476</c> <c>0088F890</c> →
    /// <c>0040D2A0</c> then blocking
    /// <c>006286F0(edx=0x1B)</c>. <c>0099C1E0</c>
    /// rewrites <c>.xmv</c> → <c>.wmv</c>.
    /// Interpreter stays in the apply until the
    /// player returns (no <c>vtbl+28</c>).
    /// </summary>
    void IScriptHost.PlayAvi(string arguments)
    {
        var file = ScriptInterpreter.FirstToken(arguments);
        LastAvi = file.Length == 0 ? null : RegionTravel.PlayAviPrefix + file;
        AviRelativePath = LastAvi is null ? null : RegionTravel.RewritePlayAviPath(LastAvi);
        AviFile = LastAvi is null || _install is null
            ? null
            : RegionTravel.ResolvePlayAviFile(_install, LastAvi);
        _avi?.Dispose();
        _avi = AviFile is null ? null : WmvPlayer.TryOpen(AviFile);
        // Open fail skips present (bccc4c6). A missing
        // player must not pin 006286F0 with no frames.
        AviPlaying = _avi is not null;
    }

    /// <summary>
    /// <c>006286F0</c> skip scan 1 / 57 / 28 / 62.
    /// </summary>
    public void SkipAvi()
    {
        _avi?.Dispose();
        _avi = null;
        AviPlaying = false;
    }

    /// <summary>
    /// One <c>006286F0</c> iteration inside the
    /// blocking PlayAVI apply: 33 ms wait analog
    /// then the current sample. Interpreter
    /// <c>TickPlayAvi</c> stays on the line until
    /// this returns (EOF / skip). No extra window.
    /// </summary>
    private void TickAvi(float dt)
    {
        if (!AviPlaying)
            return;
        if (_avi is null)
            return;
        if (_avi.TryAdvance(dt) && !_avi.Ended)
            return;
        if (_avi.Ended)
            SkipAvi();
    }

    /// <summary>
    /// <c>00CC7258</c>: <c>00CBEE0C</c> IsFalse →
    /// <c>vtbl+2664(0)</c>, else <c>(1)</c>.
    /// <c>jmp 00CC8464</c>. No yield. Body UNREAD.
    /// </summary>
    void IScriptHost.MuteSounds(string arguments) =>
        SoundsMuted = !ScriptCommand.IsFalseArg(ScriptInterpreter.FirstToken(arguments));

    /// <summary>
    /// <c>00CD1373</c>: <c>and [0x13B83C8], 0</c> then
    /// <c>jmp 00CD17FD</c>. No yield. Do not invent
    /// the leftover increment as a pose clock.
    /// </summary>
    void IScriptHost.StartTimeCode() => TimeCode = 0;

    /// <summary>
    /// <c>00CC88D1</c> default wait is scaled frames,
    /// not wall-clock dt. CLOCK arg is unread here.
    /// </summary>
    void IScriptHost.GamePause(float seconds) => LastGamePause = seconds;

    /// <summary>
    /// <c>00CC25FD</c>: thing <c>vtbl+52</c> then poll
    /// <c>vtbl+104</c>. Father <c>0x0127293C</c> +52 is
    /// <c>004CD1B0</c> <c>al=1</c>, +104 is
    /// <c>00661A40</c> <c>ret 4</c> (leaves al). First
    /// poll is busy, one <c>vtbl+28</c>, next poll idle.
    /// Do not invent dialogue UI.
    /// </summary>
    void IScriptHost.Speak(string? actor, string target, string text, int mode)
    {
        var record = LookupTextRecord(text);
        Dialogue.Speak(actor, target, text, mode, body: record?.Body);
        Dialogue.BindRecord(record);
    }

    /// <summary>
    /// <c>00CC2EAA</c>: context <c>vtbl+1456/1460/1464</c>
    /// then if third arg not TRUE one <c>vtbl+28</c>
    /// and <c>jmp 00CC707C</c>. Bodies UNREAD.
    /// </summary>
    void IScriptHost.InteractiveSpeak(
        string? actor, string listener, string prompt, bool wait, string response)
    {
        var record = LookupTextRecord(prompt);
        Dialogue.InteractiveSpeak(
            actor, listener, prompt, wait, response, record?.Body);
        Dialogue.BindRecord(record);
    }

    /// <summary>
    /// <c>00CC3165</c>: context <c>vtbl+1456/1460/1464</c>
    /// then one <c>vtbl+28</c> and <c>jmp 00CC707C</c>.
    /// Bodies UNREAD — record only.
    /// </summary>
    void IScriptHost.DialogSpeak(string? actor, string listener, string text)
    {
        var record = LookupTextRecord(text);
        Dialogue.DialogSpeak(actor, listener, text, record?.Body);
        Dialogue.BindRecord(record);
    }

    /// <summary>
    /// <c>00CC0783</c>: name unused. Poll thing
    /// <c>vtbl+104</c>. Hero stub leaves al; first
    /// leftover is busy so one <c>vtbl+28</c> then
    /// continue. Do not invent a task table.
    /// </summary>
    public void NoteWaitTask(string? actor, string name) =>
        _waits.Add(new ScriptWaitTask(actor, name));

    void IScriptHost.WaitTask(string? actor, string name) =>
        NoteWaitTask(actor, name);

    /// <summary>
    /// <c>00CC0CB5</c>: thing <c>vtbl+20</c> is
    /// <c>004C72B0</c> stub. TRUE wait polls
    /// <c>vtbl+104</c> leftover once. Record only —
    /// no mesh move.
    /// </summary>
    void IScriptHost.SneakTo(string? actor, string marker, float speed, bool wait) =>
        Movement.Sneak(actor, marker, speed, wait, null);

    /// <summary>
    /// <c>00CC083D</c>: thing <c>vtbl+20</c> is
    /// <c>004C72B0</c> stub. First-seen does not
    /// wait for arrival. Record only — no mesh move.
    /// </summary>
    void IScriptHost.WalkTo(string? actor, string marker, float speed, bool wait) =>
        Movement.Walk(actor, marker, speed, wait, null);

    /// <summary>
    /// <c>00CC15E3</c>: thing <c>vtbl+76</c> does not
    /// read the name. Record only — no TURNING_AC90
    /// pose. <c>[ebp-22]</c> one <c>vtbl+28</c>.
    /// </summary>
    void IScriptHost.PlayCombatAnimation(
        string? actor, string name, bool flagA, bool flagB, bool flagC, bool flagD, bool flagE, int count) =>
        Animation.PlayCombat(actor, name, flagA, flagB, flagC, flagD, flagE, count);

    /// <summary>
    /// <c>00CCC246</c>: <c>vtbl+364</c> then
    /// <c>jmp 00CD17F8</c>. No yield. Spawn body
    /// UNREAD — record only.
    /// </summary>
    void IScriptHost.Create(string type, string marker, string name)
    {
        var thing = FindThingByName(marker);
        var pos = thing is { PositionX: not null } ? RegionTravel.PositionOf(thing) : (Vector3?)null;
        var spawned = World.Spawn(type, marker, name, pos);
        AddThing(spawned);
        Bindings.BindCreated(name, type, marker, pos, spawned);
    }

    /// <summary>
    /// <c>00CC656B</c>: leftover session poll
    /// <c>vtbl+1472</c>. Dismiss UNREAD — one yield.
    /// </summary>
    void IScriptHost.WaitActiveDialog() => WaitActiveDialogCount++;

    /// <summary>
    /// <c>00CD0116</c>: <c>vtbl+432</c> then
    /// <c>jmp 00CC864B</c>. No yield. Teardown
    /// UNREAD — record only.
    /// </summary>
    void IScriptHost.Remove(string name)
    {
        World.Destroy(name);
        RemoveThing(name);
        Bindings.Unbind(name);
    }

    /// <summary>
    /// <c>00CC3354</c>: thing <c>vtbl+52</c> then
    /// <c>jmp 00CC707C</c> / <c>00CC2C6B</c> →
    /// <c>00CC7081</c>. No <c>vtbl+28</c>. Father
    /// +52 is <c>004CD1B0</c> stub. Record only —
    /// do not invent dialogue UI.
    /// </summary>
    void IScriptHost.DialogadSpeak(string? actor, string target, string text, int mode)
    {
        var record = LookupTextRecord(text);
        Dialogue.DialogAdSpeak(actor, target, text, mode, record?.Body);
        Dialogue.BindRecord(record);
    }

    /// <summary>
    /// <c>00CC3F73</c>: context <c>vtbl+1896</c>
    /// <c>0089BDF0</c> then <c>jmp 00CC707C</c>. No
    /// <c>vtbl+28</c>. Heading body UNREAD — record
    /// only, do not invent a yaw write.
    /// </summary>
    void IScriptHost.LookInDirection(string? actor, float degrees, bool flag) =>
        World.Looks.Add(new ScriptLookInDirection(actor, degrees, flag));

    /// <summary>
    /// <c>00CC86D0</c> default path: <c>00CBF29F</c> with
    /// <c>dl=0</c> collects UseCamera names via
    /// <c>vtbl+1648</c>. First-seen has no TRUE arg so
    /// <c>vtbl+1560</c> is skipped. <c>vtbl+1564/+1568</c>
    /// bodies stay UNREAD.
    /// </summary>
    void IScriptHost.DoCameraPreloading(string arguments)
    {
        _ = arguments;
        var source = ActiveInterpreter?.Commands;
        if (source is null)
            return;
        foreach (var line in source)
        {
            var command = ScriptCommand.Parse(line);
            if (!command.Verb.Equals("UseCamera", StringComparison.OrdinalIgnoreCase) &&
                !command.Verb.Equals("CameraLookAt", StringComparison.OrdinalIgnoreCase) &&
                !command.Verb.Equals("CameraLookBetween", StringComparison.OrdinalIgnoreCase) &&
                !command.Verb.Equals("CameraFOVLookBetween", StringComparison.OrdinalIgnoreCase))
                continue;
            var name = ScriptInterpreter.FirstToken(command.Arguments);
            CameraSys.Preload(name);
        }
    }

    private ThingInstance? FindThing(string name)
    {
        if (name.Length == 0)
            return null;
        foreach (var thing in _things)
        {
            if (thing.ScriptName is not null &&
                thing.ScriptName.Equals(name, StringComparison.OrdinalIgnoreCase))
                return thing;
        }

        return null;
    }

    private void BindCamera(string name)
    {
        if (_camera is null || name.Length == 0)
            return;
        _camera.UseCamera(_things, name);
    }
}

public sealed class ScriptFiber
{
    public string Name { get; }
    public string? PersistField { get; }
    public float DtAtPlus8 { get; set; }

    public ScriptFiber(string name, string? persistField)
    {
        Name = name;
        PersistField = persistField;
    }
}

public readonly record struct ScriptTeleport(string? Actor, string Marker, Vector3? Position);

public readonly record struct ScriptAnimation(
    string? Actor,
    string Name,
    bool Flag1,
    bool Flag2,
    bool Flag3,
    bool Flag4,
    bool Flag5);

public readonly record struct ScriptSpeech(
    string? Actor,
    string Target,
    string Text,
    int Mode);

public readonly record struct ScriptInteractiveSpeech(
    string? Actor,
    string Listener,
    string Prompt,
    bool Wait,
    string Response);

public readonly record struct ScriptDialogSpeech(
    string? Actor,
    string Listener,
    string Text);

public readonly record struct ScriptWaitTask(string? Actor, string Name);

public readonly record struct ScriptSneakTo(string? Actor, string Marker, float Speed, bool Wait);

public readonly record struct ScriptWalkTo(string? Actor, string Marker, float Speed, bool Wait);

public readonly record struct ScriptCombatAnimation(
    string? Actor,
    string Name,
    bool FlagA,
    bool FlagB,
    bool FlagC,
    bool FlagD,
    bool FlagE,
    int Count);

public readonly record struct ScriptCreate(string Type, string Marker, string Name);

public readonly record struct ScriptDialogAdSpeech(
    string? Actor,
    string Target,
    string Text,
    int Mode);

public readonly record struct ScriptLookInDirection(string? Actor, float Degrees, bool Flag);

public readonly record struct ScriptLookToThing(string? Actor, string Arguments);
