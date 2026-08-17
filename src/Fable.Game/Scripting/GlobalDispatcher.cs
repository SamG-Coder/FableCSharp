using Fable.Formats.Tng;

namespace Fable.Game.Scripting;

/// <summary>
/// Global (no-target) 00CBFB7D tokens. Each handler
/// returns a result; nothing is a generic YieldAfter.
/// </summary>
public static class GlobalDispatcher
{
    public static CommandResult Dispatch(ScriptLine line, ScriptExecutionContext ctx)
    {
        var v = line.Verb;
        if (Eq(v, "PlayMusic"))
        {
            ctx.Audio.PlayMusic(line.Arg(0));
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, line.Arg(0));
        }

        if (Eq(v, "StopMusic"))
        {
            ctx.Audio.StopMusic();
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "StopMusic");
        }

        if (Eq(v, "Play2DSound"))
        {
            var name = line.Arg(0);
            if (name.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ctx.Audio.PlaySound(name, null, spatial: false);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, name);
        }

        if (Eq(v, "PlaySound"))
        {
            var arg0 = line.Arg(0);
            var arg1 = line.Arg(1);
            if (arg0.Length == 0 || arg1.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            if (ScriptLine.IsNull(arg0))
            {
                ctx.Audio.PlaySound(arg1, null, spatial: false);
                return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Global,
                    "PlaySound NULL vtbl+2768", arg1);
            }

            ctx.Audio.PlaySound(arg1, arg0, spatial: true);
            return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Global,
                "PlaySound vtbl+2760 00CC907D", $"{arg0},{arg1}");
        }

        if (Eq(v, "MuteSounds"))
        {
            ctx.Audio.Mute(!ScriptLine.IsFalse(line.Arg(0)));
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                ctx.Audio.Muted ? "mute" : "unmute");
        }

        if (Eq(v, "EnableSounds"))
        {
            ctx.Audio.Mute(false);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "enable");
        }

        if (Eq(v, "FadeOut"))
        {
            ParseFade(line, out var seconds, out var param);
            ctx.Runtime.ApplyFadeOut(seconds, param);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                $"fade {seconds},{param}");
        }

        if (Eq(v, "FadeIn"))
        {
            ParseFade(line, out var seconds, out var param);
            ctx.Runtime.ApplyFadeIn(seconds, param);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                $"fade {seconds},{param}");
        }

        if (Eq(v, "StayFadedOut"))
        {
            ctx.Cutscene.StayFadedOut = true;
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "StayFadedOut");
        }

        if (Eq(v, "CameraPause"))
        {
            ctx.Cutscene.CameraPauseEnabled = !ScriptLine.IsFalse(line.Arg(0));
            ctx.Runtime.LastCameraPause = line.Arg(0);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                ctx.Cutscene.CameraPauseEnabled ? "TRUE" : "FALSE");
        }

        if (Eq(v, "AnimationPause"))
        {
            ctx.Cutscene.AnimationPauseEnabled = !ScriptLine.IsFalse(line.Arg(0));
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                ctx.Cutscene.AnimationPauseEnabled ? "TRUE" : "FALSE");
        }

        if (Eq(v, "CameraLookAt"))
        {
            var name = line.Arg(0);
            if (name.Length == 0 || line.Arg(1).Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ctx.Camera.LookAtThing(ctx.Runtime.Camera, ctx.FindThing(name), name);
            if (!ctx.Cutscene.YieldEnable)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, name);
            return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Global,
                "CameraLookAt vtbl+1628", name);
        }

        if (Eq(v, "CameraLookBetween"))
            return ApplyCameraLookBetween(line, ctx);

        if (Eq(v, "CameraFOVLookBetween"))
            return ApplyCameraFovLookBetween(line, ctx);

        if (Eq(v, "CameraFOVLookBetweenPos"))
            return ApplyCameraFovLookBetweenPos(line, ctx);

        if (Eq(v, "PutInFrontOf"))
        {
            var mover = line.Arg(0);
            var face = line.Arg(1);
            if (mover.Length == 0 || face.Length == 0 || line.Arg(2).Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            if (!ScriptLine.TryFloat(line.Arg(2), out var distance))
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            var thing = ctx.FindThing(face);
            if (thing is not { PositionX: not null } &&
                !ctx.World.Positions.TryGetValue(face, out _))
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, face);
            var pos = thing is { PositionX: not null }
                ? RegionTravel.PositionOf(thing)
                : ctx.World.Positions[face];
            var forward = thing is not null
                ? RegionTravel.ForwardOf(thing)
                : System.Numerics.Vector3.UnitY;
            var dest = RegionTravel.WalkUpToDestination(pos, forward, distance);
            ctx.World.Teleport(mover, face, dest);
            ctx.World.LookTargets[mover] = face;
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                $"{mover}@{dest.X:0.##},{dest.Y:0.##}");
        }

        if (Eq(v, "ResetCamera"))
        {
            ctx.Camera.Reset(ctx.Runtime.Camera);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "reset");
        }

        if (Eq(v, "ScriptFrame"))
        {
            ctx.Cutscene.YieldEnable = !ScriptLine.IsFalse(line.Arg(0));
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                ctx.Cutscene.YieldEnable ? "TRUE" : "FALSE");
        }

        if (Eq(v, "DoOneFrame"))
        {
            if (!ctx.Cutscene.YieldEnable)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "no-yield");
            return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Global,
                "DoOneFrame vtbl+28", "1");
        }

        if (Eq(v, "PutUpYourSwords"))
        {
            ctx.World.SwordsUp = !ScriptLine.IsFalse(line.Arg(0));
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                ctx.World.SwordsUp ? "TRUE" : "FALSE");
        }

        if (Eq(v, "NoDialogCam"))
        {
            ctx.Cutscene.NoDialogCam = ScriptLine.IsTrue(line.Arg(0));
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                ctx.Cutscene.NoDialogCam ? "TRUE" : "FALSE");
        }

        if (Eq(v, "UseCamera") || Eq(v, "NoLoadUseCamera"))
        {
            var name = line.Arg(0);
            if (name.Length == 0 || ScriptLine.IsNull(name))
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ctx.Camera.Bind(ctx.Runtime.Camera, ctx.Runtime.Things, name);
            if (!ctx.Cutscene.CameraPauseEnabled || !ctx.Cutscene.YieldEnable)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, name);
            return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Global,
                "UseCamera vtbl+28", name);
        }

        if (Eq(v, "WaitForCamera"))
        {
            var op = ctx.Camera.WaitForCamera();
            if (op.Complete)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "idle");
            return CommandResult.Wait(
                ExecutionKind.WaitOperation, CommandStatus.Proven, CommandFamily.Global,
                "WaitForCamera vtbl+1672", "camera-idle", op.Id, ctx.Camera.ActiveName);
        }

        if (Eq(v, "DoCameraPreloading"))
        {
            foreach (var raw in ctx.Cutscene.Commands)
            {
                var cmd = ScriptLine.Parse(raw);
                if (Eq(cmd.Verb, "UseCamera") ||
                    Eq(cmd.Verb, "CameraLookAt") ||
                    Eq(cmd.Verb, "CameraLookBetween") ||
                    Eq(cmd.Verb, "CameraFOVLookBetween"))
                    ctx.Camera.Preload(cmd.Arg(0));
            }

            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                string.Join(",", ctx.Camera.Preloaded));
        }

        if (Eq(v, "DoScriptFrame"))
        {
            var count = ParseScriptFrame(line.Arg(0));
            if (count <= 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "0");
            ctx.Cutscene.ScriptFrameRemaining = count;
            return CommandResult.Wait(
                ExecutionKind.WaitFrames, CommandStatus.Proven, CommandFamily.Global,
                "DoScriptFrame vtbl+28", "frame", null, count.ToString(),
                advanceWhenDone: true);
        }

        if (Eq(v, "GamePause"))
        {
            ScriptLine.TryFloat(line.Arg(0), out var seconds);
            ctx.Runtime.LastGamePause = seconds;
            ctx.Cutscene.GamePauseTarget = seconds * RegionTravel.GamePauseScale;
            ctx.Cutscene.GamePauseCounter = 0f;
            ctx.Cutscene.GamePausePhase = 1;
            return CommandResult.Wait(
                ExecutionKind.WaitScaledFrames, CommandStatus.Proven, CommandFamily.Global,
                "GamePause scaled", "counter", null, seconds.ToString("0.###"),
                advanceWhenDone: true);
        }

        if (Eq(v, "PlayAVI"))
        {
            var file = line.Arg(0);
            if (file.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ctx.Runtime.BeginAvi(file);
            return CommandResult.Wait(
                ExecutionKind.BlockPump, CommandStatus.Proven, CommandFamily.Global,
                "PlayAVI 006286F0", "eof/skip", "avi", file, advanceWhenDone: true);
        }

        if (Eq(v, "StartTimeCode"))
        {
            ctx.Runtime.TimeCode = 0;
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "0");
        }

        if (Eq(v, "SetTime"))
        {
            if (line.Arg(0).Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ScriptLine.TryFloat(line.Arg(0), out var hours);
            ScriptLine.TryFloat(line.Arg(2), out var duration);
            var frac = DayFraction(hours);
            ctx.World.TimeOfDayHours = hours;
            ctx.World.TimeOfDayFraction = frac;
            ctx.Runtime.TimeOfDayHours = hours;
            ctx.Runtime.TimeOfDayFraction = frac;
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                duration > 0f ? $"{hours}->{duration}" : hours.ToString("0.###"));
        }

        if (Eq(v, "Create"))
        {
            var type = line.Arg(0);
            var marker = line.Arg(1);
            var name = line.Arg(2);
            if (type.Length == 0 || marker.Length == 0 || name.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            var markerThing = ctx.FindThing(marker);
            var pos = markerThing is { PositionX: not null }
                ? RegionTravel.PositionOf(markerThing)
                : (System.Numerics.Vector3?)null;
            var spawned = ctx.World.Spawn(type, marker, name, pos);
            ctx.Runtime.AddThing(spawned);
            ctx.Bindings.BindCreated(name, type, marker, pos, spawned);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                $"{type}->{name}", $"Created:{name}");
        }

        if (Eq(v, "CreateNear"))
        {
            var type = line.Arg(0);
            var near = line.Arg(1);
            var name = line.Arg(2);
            if (type.Length == 0 || near.Length == 0 || name.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ScriptLine.TryFloat(line.Arg(3), out var radius);
            var nearThing = ctx.FindThing(near);
            var pos = nearThing is { PositionX: not null }
                ? RegionTravel.PositionOf(nearThing)
                : (System.Numerics.Vector3?)null;
            var spawned = ctx.World.Spawn(type, near, name, pos);
            ctx.Runtime.AddThing(spawned);
            ctx.Bindings.BindCreated(name, type, near, pos, spawned);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                $"{type}->{name} r={radius:0.##}", $"Created:{name}");
        }

        if (Eq(v, "ObjectCreate"))
        {
            var type = line.Arg(0);
            var marker = line.Arg(1);
            var name = line.Arg(2);
            if (type.Length == 0 || marker.Length == 0 || name.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            var markerThing = ctx.FindThing(marker);
            var pos = markerThing is { PositionX: not null }
                ? RegionTravel.PositionOf(markerThing)
                : (System.Numerics.Vector3?)null;
            var spawned = ctx.World.Spawn(type, marker, name, pos);
            ctx.Runtime.AddThing(spawned);
            ctx.Bindings.BindCreated(name, type, marker, pos, spawned);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                $"{type}->{name}", $"Created:{name}");
        }

        if (Eq(v, "CrowdCreate"))
        {
            var type = line.Arg(0);
            var source = line.Arg(1);
            var alias = line.Arg(2);
            if (type.Length == 0 || source.Length == 0 || alias.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            var sources = ctx.World.CollectByName(ctx.Runtime.Things, source);
            var spawned = new List<ThingInstance>();
            for (var i = 0; i < sources.Count; i++)
            {
                var src = sources[i];
                var pos = src.PositionX is not null ? RegionTravel.PositionOf(src) : (System.Numerics.Vector3?)null;
                var member = alias + i.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var thing = ctx.World.Spawn(type, src.ScriptName ?? source, member, pos);
                ctx.Runtime.AddThing(thing);
                spawned.Add(thing);
            }

            ctx.Bindings.BindCrowd(type, alias, spawned);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                $"{type}->{alias}x{spawned.Count}", $"Crowd:{alias}");
        }

        if (Eq(v, "CrowdCreateMixed"))
        {
            var typeA = line.Arg(0);
            var typeB = line.Arg(1);
            var source = line.Arg(2);
            var alias = line.Arg(3);
            if (typeA.Length == 0 || typeB.Length == 0 ||
                source.Length == 0 || alias.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            var sources = ctx.World.CollectByName(ctx.Runtime.Things, source);
            var spawned = new List<ThingInstance>();
            for (var i = 0; i < sources.Count; i++)
            {
                var type = (i & 1) == 0 ? typeA : typeB;
                var src = sources[i];
                var pos = src.PositionX is not null ? RegionTravel.PositionOf(src) : (System.Numerics.Vector3?)null;
                var member = alias + i.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var thing = ctx.World.Spawn(type, src.ScriptName ?? source, member, pos);
                ctx.Runtime.AddThing(thing);
                spawned.Add(thing);
            }

            ctx.Bindings.BindCrowd(typeA, alias, spawned);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                $"{typeA}|{typeB}->{alias}x{spawned.Count}", $"Crowd:{alias}");
        }

        if (Eq(v, "RemoveAllThings"))
        {
            if (line.Arg(0).Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ctx.World.RemoveFamily.Add(("RemoveAllThings", "LadyGreyIntro"));
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                "LadyGreyIntro");
        }

        if (Eq(v, "RemoveAll"))
        {
            var hide = !ScriptLine.IsFalse(line.Arg(0));
            ctx.World.RemoveFamily.Add(("RemoveAll", hide ? "TRUE" : "FALSE"));
            ctx.World.RemoveExtras(hide, "RemoveAll");
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                hide ? "TRUE" : "FALSE");
        }

        if (Eq(v, "Remove") || Eq(v, "RemoveThing"))
            return ApplyRemove(line, ctx);

        if (Eq(v, "RemoveExtras"))
        {
            var hide = ScriptLine.IsTrue(line.Arg(0));
            var mode = line.Arg(1);
            ctx.World.RemoveExtras(hide, mode);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                hide ? $"hide {mode}" : $"show {mode}");
        }

        if (Eq(v, "Get"))
        {
            var source = line.Arg(0);
            if (source.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            var alias = line.Arg(1).Length == 0 ? source : line.Arg(1);
            var slot = ctx.Bindings.Resolve(source);
            var thing = slot?.Thing ?? ctx.FindThing(source);
            ctx.Bindings.BindAcquired(alias, thing, source);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                $"{source}->{alias}", $"Acquired:{alias}");
        }

        if (Eq(v, "FallbackAcquire"))
        {
            var alias = line.Arg(0);
            if (alias.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ThingInstance? hit = null;
            var source = alias;
            for (var i = 1; i < line.Args.Count && hit is null; i++)
            {
                var type = line.Arg(i);
                if (type.Length == 0)
                    continue;
                foreach (var thing in ctx.Runtime.Things)
                {
                    if (thing.DefinitionType is not null &&
                        thing.DefinitionType.Equals(type, StringComparison.OrdinalIgnoreCase))
                    {
                        hit = thing;
                        source = type;
                        break;
                    }
                }
            }

            ctx.Bindings.BindAcquired(alias, hit, source);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                alias, $"Acquired:{alias}");
        }

        if (Eq(v, "SetDoorOpen"))
        {
            var door = line.Arg(0);
            if (door.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ctx.World.Doors[door] = !ScriptLine.IsFalse(line.Arg(1));
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, door);
        }

        if (Eq(v, "WaitActiveDialog"))
        {
            ctx.Runtime.WaitActiveDialogCount++;
            var op = ctx.Dialogue.WaitActive();
            return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Global,
                "WaitActiveDialog leftover vtbl+1472", op.Id);
        }

        if (Eq(v, "RegisterActor"))
        {
            var name = line.Arg(0);
            if (name.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ctx.Bindings.RegisterActor(name);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, name,
                $"Registered:{name}");
        }

        if (Eq(v, "CrowdAcquire"))
        {
            var type = line.Arg(0);
            if (type.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            var alias = line.Arg(1);
            var members = ctx.Runtime.Things.Where(t =>
                    (t.ScriptName is not null &&
                     t.ScriptName.StartsWith(type, StringComparison.OrdinalIgnoreCase)) ||
                    (t.DefinitionType is not null &&
                     t.DefinitionType.Equals(type, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            ctx.Bindings.BindCrowd(type, alias, members);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                alias.Length == 0 ? type : alias, $"Crowd:{alias}");
        }

        if (Eq(v, "CrowdClearActions"))
        {
            var name = line.Arg(0);
            if (name.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            if (ctx.Bindings.TryCrowd(name, out var members))
            {
                foreach (var m in members)
                    ctx.Movement.Clear(m);
            }

            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, name);
        }

        if (Eq(v, "CrowdAnimate"))
        {
            var crowd = line.Arg(0);
            var anim = line.Arg(1);
            if (crowd.Length == 0 || anim.Length == 0 ||
                line.Arg(2).Length == 0 || line.Arg(3).Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            if (!ctx.Bindings.TryCrowd(crowd, out var members) || members.Count == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "empty");
            var flags = ScriptCommand.ParsePlayAnimationFlags(
                $"{anim},{line.Arg(4)},{line.Arg(5)},{line.Arg(6)},{line.Arg(7)},{line.Arg(8)}");
            foreach (var member in members)
                ctx.Animation.Play(member, anim, flags.Flag1, flags.Flag2, flags.Flag3, flags.Flag4, flags.Flag5);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                $"{crowd},{anim}");
        }

        if (Eq(v, "GiveHero"))
        {
            ScriptLine.TryInt(line.Arg(1), out var count);
            if (count <= 0)
                count = 1;
            ctx.World.HeroGifts.Add((line.Arg(0), count));
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, line.Arg(0));
        }

        if (Eq(v, "SetFlag"))
            return ApplySetFlag(line, ctx);

        if (Eq(v, "WaitFlag"))
            return ApplyWaitFlag(line, ctx);

        return CommandResult.Blocked(
            "UNKNOWN", CommandStatus.Unread, CommandFamily.Global, line.Raw);
    }

    /// <summary>
    /// Shared <c>00CD0116</c> path. Token match is
    /// <c>00BFEAF8("Remove", 6)</c> so
    /// <c>RemoveThing</c> is the same handler, not a
    /// separate dispatcher. Empty arg0 →
    /// <c>00CD17FD</c>. Arg1 <c>dead</c> →
    /// <c>vtbl+1608</c>. Else lookup +
    /// <c>vtbl+432</c> (<c>008910D0</c> /
    /// <c>004C9B80</c>).
    /// </summary>
    internal static CommandResult ApplyRemove(ScriptLine line, ScriptExecutionContext ctx)
    {
        var name = line.Arg(0);
        if (name.Length == 0)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
        ctx.World.RemoveFamily.Add((line.Verb, name));
        if (line.Arg(1).Equals("dead", StringComparison.OrdinalIgnoreCase))
        {
            ctx.World.Dead.Add(name);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                $"dead {name}", $"dead {name}");
        }

        ctx.World.Destroy(name);
        ctx.Runtime.RemoveThing(name);
        ctx.Bindings.Unbind(name);
        return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, name,
            $"unbind {name}");
    }

    /// <summary>
    /// <c>00CCA4C8</c>: arg0+arg1+[ebp+112] required else
    /// <c>00CD17FD</c>. IsTrue(arg2) and <c>[ebp-39]</c>
    /// skip rewrite. IsFalse(arg1) writes 0 else 1 via
    /// <c>008ADF10</c>. Always <c>jmp 00CC907D</c>.
    /// </summary>
    internal static CommandResult ApplySetFlag(ScriptLine line, ScriptExecutionContext ctx)
    {
        var name = line.Arg(0);
        if (name.Length == 0 || line.Arg(1).Length == 0)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
        if (ScriptLine.IsTrue(line.Arg(2)) && ctx.Cutscene.FlagRewriteDone)
            return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Global,
                "SetFlag skip-rewrite 00CC907D", name);
        var value = (byte)(ScriptLine.IsFalse(line.Arg(1)) ? 0 : 1);
        ctx.Flags.Set(name, value);
        ctx.Cutscene.FlagRewriteDone = true;
        return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Global,
            "SetFlag 00CC907D", $"{name}={value}");
    }

    /// <summary>
    /// <c>00CCAA6C</c>: four required args else
    /// <c>00CD17FD</c>. Lookup arg0/arg1 things.
    /// atof arg3 duration. Optional arg4-9 offsets.
    /// <c>vtbl+1632</c>(posA+off, posB+off, …, duration, -1).
    /// Yield <c>vtbl+28</c> if <c>[ebp+103]</c>.
    /// </summary>
    internal static CommandResult ApplyCameraLookBetween(
        ScriptLine line, ScriptExecutionContext ctx)
    {
        var nameA = line.Arg(0);
        var nameB = line.Arg(1);
        if (nameA.Length == 0 || nameB.Length == 0 ||
            line.Arg(2).Length == 0 || line.Arg(3).Length == 0)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
        ScriptLine.TryFloat(line.Arg(3), out var duration);
        var offsetA = ReadOffset(line, 4);
        var offsetB = ReadOffset(line, 7);
        ctx.Camera.LookBetween(
            ctx.Runtime.Camera,
            ctx.FindThing(nameA), nameA,
            ctx.FindThing(nameB), nameB,
            offsetA, offsetB, duration);
        var side = $"{nameA}|{nameB} d={duration:0.##}";
        if (!ctx.Cutscene.YieldEnable)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, side);
        return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Global,
            "CameraLookBetween vtbl+1632", side);
    }

    /// <summary>
    /// <c>00CCB4CC</c>: four required args. Same
    /// <c>vtbl+1632</c> as LookBetween. Optional arg4
    /// FOV degrees * <c>1/360</c> (<c>0x1238E00</c>),
    /// default -1. Arg5/arg6 atof discarded. Yield if
    /// <c>[ebp+103]</c>.
    /// </summary>
    internal static CommandResult ApplyCameraFovLookBetween(
        ScriptLine line, ScriptExecutionContext ctx)
    {
        var nameA = line.Arg(0);
        var nameB = line.Arg(1);
        if (nameA.Length == 0 || nameB.Length == 0 ||
            line.Arg(2).Length == 0 || line.Arg(3).Length == 0)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
        ScriptLine.TryFloat(line.Arg(3), out var duration);
        var fov = -1f;
        if (line.Arg(4).Length > 0)
            ScriptLine.TryFloat(line.Arg(4), out fov);
        ctx.Camera.LookBetween(
            ctx.Runtime.Camera,
            ctx.FindThing(nameA), nameA,
            ctx.FindThing(nameB), nameB,
            default, default, duration, fov);
        var side = fov >= 0f
            ? $"{nameA}|{nameB} d={duration:0.##} fov={fov:0.##}"
            : $"{nameA}|{nameB} d={duration:0.##}";
        if (!ctx.Cutscene.YieldEnable)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, side);
        return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Global,
            "CameraFOVLookBetween vtbl+1632", side);
    }

    /// <summary>
    /// <c>00CCB0D0</c>: four required args. Lookup
    /// arg0/arg1 things. atof arg3 duration. Optional
    /// arg4-6 add to arg2 thing/handle pos. Arg4 also
    /// FOV degrees * 1/360 (default -1).
    /// <c>vtbl+1636</c>(posA, posB, camPos, dur, fov).
    /// Yield if <c>[ebp+103]</c>.
    /// </summary>
    internal static CommandResult ApplyCameraFovLookBetweenPos(
        ScriptLine line, ScriptExecutionContext ctx)
    {
        var nameA = line.Arg(0);
        var nameB = line.Arg(1);
        var namePos = line.Arg(2);
        if (nameA.Length == 0 || nameB.Length == 0 ||
            namePos.Length == 0 || line.Arg(3).Length == 0)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
        ScriptLine.TryFloat(line.Arg(3), out var duration);
        var offset = ReadOffset(line, 4);
        var fov = -1f;
        if (line.Arg(4).Length > 0)
            ScriptLine.TryFloat(line.Arg(4), out fov);
        System.Numerics.Vector3? camPos = null;
        var posThing = ctx.FindThing(namePos);
        if (posThing is { PositionX: not null })
            camPos = RegionTravel.PositionOf(posThing) + offset;
        else if (ctx.World.Positions.TryGetValue(namePos, out var stored))
            camPos = stored + offset;
        ctx.Camera.LookBetweenPos(
            ctx.Runtime.Camera,
            ctx.FindThing(nameA), nameA,
            ctx.FindThing(nameB), nameB,
            camPos, duration, fov);
        var side = camPos is { } p
            ? $"{nameA}|{nameB}@{p.X:0.##},{p.Y:0.##} d={duration:0.##}"
            : $"{nameA}|{nameB} d={duration:0.##}";
        if (!ctx.Cutscene.YieldEnable)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, side);
        return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Global,
            "CameraFOVLookBetweenPos vtbl+1636", side);
    }

    private static System.Numerics.Vector3 ReadOffset(ScriptLine line, int start)
    {
        var x = 0f;
        var y = 0f;
        var z = 0f;
        if (line.Arg(start).Length > 0)
            ScriptLine.TryFloat(line.Arg(start), out x);
        if (line.Arg(start + 1).Length > 0)
            ScriptLine.TryFloat(line.Arg(start + 1), out y);
        if (line.Arg(start + 2).Length > 0)
            ScriptLine.TryFloat(line.Arg(start + 2), out z);
        return new System.Numerics.Vector3(x, y, z);
    }

    /// <summary>
    /// <c>00CCB893</c>: arg0+arg1+[ebp+112] required else
    /// <c>00CD17FD</c>. IsTrue(arg1) expected=1 else 0.
    /// Match continues. Mismatch leftover-polls
    /// <c>00CCB8CE</c> (vtbl+28 if yield-enable).
    /// </summary>
    internal static CommandResult ApplyWaitFlag(ScriptLine line, ScriptExecutionContext ctx)
    {
        var name = line.Arg(0);
        if (name.Length == 0 || line.Arg(1).Length == 0)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
        var expected = (byte)(ScriptLine.IsTrue(line.Arg(1)) ? 1 : 0);
        var op = ctx.Flags.Wait(name, expected);
        if (op.Complete)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                $"{name}={expected}");
        return CommandResult.Wait(
            ExecutionKind.WaitOperation, CommandStatus.Proven, CommandFamily.Global,
            "WaitFlag leftover 00CCB8CE", "flag-match", op.Id, $"{name}={expected}");
    }

    internal static void ParseFade(ScriptLine line, out float seconds, out float param)
    {
        seconds = RegionTravel.FadeSpecialCaseSeconds;
        param = 0f;
        if (line.Arg(0).Length > 0)
            ScriptLine.TryFloat(line.Arg(0), out seconds);
        if (line.Arg(1).Length > 0)
            ScriptLine.TryFloat(line.Arg(1), out param);
    }

    internal static int ParseScriptFrame(string token)
    {
        if (token.Length == 0)
            return RegionTravel.DoScriptFrameDefaultCount;
        var n = 0;
        var negative = false;
        var saw = false;
        foreach (var ch in token)
        {
            if (ch == '-')
            {
                negative = true;
                continue;
            }

            if (ch == '.')
                break;
            if (ch is < '0' or > '9')
                break;
            saw = true;
            n = n * 10 + (ch - '0');
        }

        if (!saw)
            return RegionTravel.DoScriptFrameDefaultCount;
        return negative ? -n : n;
    }

    internal static float DayFraction(float hours)
    {
        while (hours >= 24f)
            hours -= 24f;
        var frac = hours * (1f / 24f);
        if (frac < 0f)
            return 0f;
        if (frac > 1f)
            return 1f;
        return frac;
    }

    private static bool Eq(string a, string b) =>
        a.Equals(b, StringComparison.OrdinalIgnoreCase);
}
