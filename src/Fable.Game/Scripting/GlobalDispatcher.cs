using System.Numerics;
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
            var track = line.Arg(0);
            if (track.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ctx.Audio.PlayMusic(track, ctx.Runtime.LookupMusic(track));
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, track);
        }

        if (Eq(v, "StopMusic"))
        {
            ctx.Audio.StopMusic();
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "StopMusic");
        }

        if (Eq(v, "CacheMusic"))
        {
            var track = line.Arg(0);
            if (track.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ctx.Audio.CacheMusic(track);
            ctx.Runtime.LookupMusic(track);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, track);
        }

        if (Eq(v, "Play2DSound"))
        {
            var name = line.Arg(0);
            if (name.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            // leftover helper 00CBF7FE: vtbl+2768(name), no yield.
            ctx.Audio.PlaySound(name, null, spatial: false, vtbl: 2768);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, name);
        }

        if (Eq(v, "PlaySound"))
        {
            var source = line.Arg(0);
            var name = line.Arg(1);
            if (source.Length == 0 || name.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            if (ScriptLine.IsNull(source))
            {
                ctx.Audio.PlaySound(name, null, spatial: false, vtbl: 2768);
                return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Global,
                    "PlaySound NULL vtbl+2768", name);
            }

            var criteria = line.Arg(2).Length > 0;
            var vtbl = criteria ? 2756 : 2760;
            var thing = ResolveSoundSource(ctx, source);
            ctx.Audio.PlaySound(name, thing?.ScriptName ?? source, spatial: true, criteria, vtbl);
            return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Global,
                $"PlaySound vtbl+{vtbl} 00CC907D", $"{source},{name}");
        }

        if (Eq(v, "MuteSounds"))
        {
            ctx.Audio.Mute(!ScriptLine.IsFalse(line.Arg(0)));
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                ctx.Audio.Muted ? "mute" : "unmute");
        }

        if (Eq(v, "UseTheme"))
        {
            var name = line.Arg(0);
            if (name.Length == 0 || ScriptLine.IsNull(name))
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            var param = 0f;
            if (line.Arg(1).Length > 0)
                ScriptLine.TryFloat(line.Arg(1), out param);
            var flag = line.Arg(2).Length == 0 || ScriptLine.IsTrue(line.Arg(2));
            var reset = name.Equals("RESET", StringComparison.OrdinalIgnoreCase);
            ctx.Audio.UseTheme(name, param, flag, reset);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                reset ? "RESET" : name);
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

        if (Eq(v, "CameraRotateThing"))
            return ApplyCameraRotateThing(line, ctx);

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

        if (Eq(v, "SetThingConscious"))
        {
            // 00CC8094: arg0 required; default 0; IsTrue(arg1)->1;
            // optional arg2 extra; vtbl+2324(thing,flag,extra).
            var name = line.Arg(0);
            if (name.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            var on = ScriptLine.IsTrue(line.Arg(1));
            ctx.World.SetThingConscious(name, on, line.Arg(2));
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                on ? "1" : "0");
        }

        if (Eq(v, "SetHomePosThing"))
        {
            // 00CC7D3C: arg0 required; HERO vtbl+280 else 288;
            // 004AB130; 004AA9A0; vtbl+1892. NOT a HomePos write.
            var name = line.Arg(0);
            if (name.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            if (!ctx.World.TryHomeDest(name, ctx.FindThing(name), out var pos))
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ctx.World.ResetPos(name, pos);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                $"{name}@{pos.X:0.##},{pos.Y:0.##}");
        }

        if (Eq(v, "SlideTeleport"))
        {
            // 00CC5A8D: actor,from,to required; count default 100;
            // leftover each step if [ebp+103].
            var wait = ctx.Cutscene.YieldEnable;
            return EntityDispatcher.ApplySlideTeleport(
                line, ctx, line.Arg(0), line.Arg(1), line.Arg(2),
                line.Arg(3), wait, ScriptLine.IsFalse(line.Arg(4)));
        }

        if (Eq(v, "TeleportThing"))
        {
            // 00CC7E7F: thing+marker required; IsFalse(arg2)->0 else 1;
            // 004AA980 marker pos; 004AAA40 yaw; vtbl+1892.
            var name = line.Arg(0);
            var marker = line.Arg(1);
            if (name.Length == 0 || marker.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            var thing = ctx.FindThing(marker);
            Vector3? dest = thing is { PositionX: not null }
                ? RegionTravel.PositionOf(thing)
                : ctx.World.Positions.TryGetValue(marker, out var stored)
                    ? stored
                    : null;
            if (dest is not { } pos)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ctx.World.Teleport(name, marker, pos);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                $"{name}->{marker}");
        }

        if (Eq(v, "DrawThing"))
        {
            var name = line.Arg(0);
            if (name.Length == 0 || line.Arg(1).Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            var draw = !ScriptLine.IsFalse(line.Arg(1));
            ctx.World.Drawable[name] = draw;
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                draw ? $"{name} on" : $"{name} off");
        }

        if (Eq(v, "SetLightScene"))
            return ApplySetLightScene(line, ctx);

        if (Eq(v, "TintScreenTo"))
            return ApplyTintScreenTo(line, ctx);

        if (Eq(v, "TintScreenOut"))
        {
            if (line.Arg(0).Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ScriptLine.TryFloat(line.Arg(0), out var seconds);
            ctx.Camera.TintOut(ctx.Cutscene.TintHold, seconds);
            ctx.Cutscene.TintHold = 0f;
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                seconds.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
        }

        if (Eq(v, "CameraShake"))
        {
            if (line.Arg(0).Length == 0 || line.Arg(1).Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ScriptLine.TryFloat(line.Arg(0), out var a);
            ScriptLine.TryFloat(line.Arg(1), out var b);
            ctx.Camera.Shake(a, b);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                $"{a:0.##},{b:0.##}");
        }

        if (Eq(v, "CameraPath"))
            return ApplyCameraPath(line, ctx);

        if (Eq(v, "UseCameraFOVMarkerList"))
            return ApplyUseCameraFovMarkerList(line, ctx);

        if (Eq(v, "CameraRig"))
            return ApplyCameraRig(line, ctx);

        if (Eq(v, "CameraEffect"))
        {
            if (line.Arg(0).Length == 0 || line.Arg(1).Length == 0 ||
                line.Arg(2).Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ScriptLine.TryFloat(line.Arg(0), out var a);
            ScriptLine.TryFloat(line.Arg(1), out var b);
            ScriptLine.TryFloat(line.Arg(2), out var c);
            ctx.Camera.Effect(a, b, c);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                $"{a:0.##},{b:0.##},{c:0.##}");
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
            // 00CC9356: IsTrue(arg0) classifies vtbl+788/792;
            // vtbl+520 sheathe always. FALSE still sheathes.
            ctx.World.SwordClassifyRequested = ScriptLine.IsTrue(line.Arg(0));
            ctx.World.SwordsUp = true;
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                ctx.World.SwordClassifyRequested ? "TRUE" : "sheathe");
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
            var op = ctx.Camera.WaitForCamera(ctx.Runtime.Camera);
            if (op.Complete)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "idle");
            return CommandResult.Wait(
                ExecutionKind.WaitOperation, CommandStatus.Proven, CommandFamily.Global,
                "WaitForCamera leftover vtbl+1672", "camera-idle", op.Id, ctx.Camera.ActiveName);
        }

        if (Eq(v, "WaitForMessageCamera"))
        {
            var name = line.Arg(0);
            if (name.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            var op = ctx.Camera.WaitForMessage(name);
            if (op.Complete)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, name);
            return CommandResult.Wait(
                ExecutionKind.WaitOperation, CommandStatus.Proven, CommandFamily.Global,
                "WaitForMessageCamera leftover vtbl+2316", "message-camera", op.Id, name);
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
            return ApplyCreate(line, ctx);

        if (Eq(v, "CreateEffect"))
            return ApplyCreateEffect(line, ctx);

        if (Eq(v, "DummyEffect"))
            return ApplyDummyEffect(line, ctx);

        if (Eq(v, "CreateLight"))
            return ApplyCreateLight(line, ctx);

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

        if (Eq(v, "RemoveEffect"))
            return ApplyRemoveEffect(line, ctx);

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
            var open = !ScriptLine.IsFalse(line.Arg(1));
            ctx.World.Doors[door] = open;
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                open ? $"{door} open" : $"{door} close");
        }

        if (Eq(v, "SetChestOpen"))
        {
            var chest = line.Arg(0);
            if (chest.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            var open = !ScriptLine.IsFalse(line.Arg(1));
            ctx.World.Chests[chest] = open;
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                open ? $"{chest} open" : $"{chest} close");
        }

        if (Eq(v, "AskQuestion"))
        {
            var text = line.Arg(0);
            if (text.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            if (ctx.Cutscene.QuestionLock)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "locked");
            var yes = line.Arg(1);
            if (yes.Length == 0)
                yes = "TEXT_OBJECT_HERO_ANSWER_YES";
            var no = line.Arg(2);
            if (no.Length == 0)
                no = "TEXT_OBJECT_HERO_ANSWER_NO";
            ctx.Cutscene.QuestionLock = true;
            var op = ctx.Dialogue.AskQuestion(text, yes, no, ctx.Runtime.LookupText(text));
            return CommandResult.Wait(
                ExecutionKind.WaitOperation, CommandStatus.Proven, CommandFamily.Global,
                "AskQuestion vtbl+456 poll vtbl+156", "esi>=0", op.Id, text);
        }

        if (Eq(v, "WaitActiveDialog"))
        {
            ctx.Runtime.WaitActiveDialogCount++;
            var op = ctx.Dialogue.WaitActive();
            if (op.Complete)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "idle");
            // Native leftover 00CC65C6: vtbl+28 then inc [0x13B83C8];
            // [0x13D2838]+5 != 0 → 00CC7081 (one leftover, next line).
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

        if (Eq(v, "GiveGold"))
        {
            var token = line.Arg(0);
            if (token.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ScriptLine.TryInt(token, out var amount);
            ctx.World.GiveGold(amount);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                ctx.World.HeroGold.ToString());
        }

        if (Eq(v, "GiveHero"))
        {
            var item = line.Arg(0);
            if (item.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            var count = 1;
            if (line.Arg(1).Length > 0)
                ScriptLine.TryInt(line.Arg(1), out count);
            if (count <= 0)
                count = 1;
            var extra = -1;
            if (line.Arg(2).Length > 0)
                ScriptLine.TryInt(line.Arg(2), out extra);
            var silent = ScriptLine.IsTrue(line.Arg(3));
            var yield = ScriptLine.IsTrue(line.Arg(4)) && !silent;
            ctx.World.GiveHero(item, count, extra, silent);
            if (yield && ctx.Cutscene.YieldEnable)
                return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Global,
                    "GiveHero leftover vtbl+28", item);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, item);
        }

        if (Eq(v, "TakeFromHero"))
        {
            var item = line.Arg(0);
            if (item.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ctx.World.TakeFromHero(item);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, item);
        }

        if (Eq(v, "TakeObjectFromHero"))
        {
            var item = line.Arg(0);
            if (item.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ctx.World.TakeObjectFromHero(item);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, item);
        }

        if (Eq(v, "PutInHeroHands"))
        {
            var arg0 = line.Arg(0);
            if (arg0.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            if (ScriptLine.IsNull(arg0))
            {
                ctx.World.HeroHands = "";
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "NULL");
            }

            if (line.Arg(1).Equals("NAME", StringComparison.OrdinalIgnoreCase))
            {
                ctx.World.HeroHands = arg0;
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, arg0);
            }

            var thing = ResolveSoundSource(ctx, arg0);
            ctx.World.HeroHands = thing?.ScriptName ?? arg0;
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                ctx.World.HeroHands);
        }

        if (Eq(v, "SetHeroWeapon"))
        {
            var name = line.Arg(0);
            if (name.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ctx.World.HeroWeapon = name;
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, name);
        }

        if (Eq(v, "RemoveHeroWeapons"))
        {
            // 00CC9106: IsFalse(arg0) → vtbl+560 else vtbl+552.
            var isFalse = ScriptLine.IsFalse(line.Arg(0));
            ctx.World.RemoveHeroWeaponsVtbl = isFalse ? 560 : 552;
            ctx.World.HeroWeapon = "";
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                isFalse ? "vtbl+560" : "vtbl+552");
        }

        if (Eq(v, "HeroHair"))
        {
            var name = line.Arg(0);
            if (name.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ctx.World.ApplyHeroHair(name);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, name);
        }

        if (Eq(v, "HeroTattoo"))
        {
            var name = line.Arg(0);
            if (name.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ctx.World.ApplyHeroTattoo(name);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, name);
        }

        if (Eq(v, "HeroWear"))
        {
            var name = line.Arg(0);
            if (name.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ctx.World.ApplyHeroWear(name);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, name);
        }

        if (Eq(v, "RemoveHeroClothes"))
        {
            ctx.World.RemoveHeroClothes();
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "vtbl+756");
        }

        if (Eq(v, "GiveHeroHealth"))
        {
            var token = line.Arg(0);
            if (token.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            float amount;
            if (token.Equals("MAX", StringComparison.OrdinalIgnoreCase))
                amount = ctx.World.GiveHeroHealthMax();
            else
            {
                ScriptLine.TryFloat(token, out amount);
                ctx.World.GiveHeroHealth(amount);
            }

            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                token.Equals("MAX", StringComparison.OrdinalIgnoreCase) ? "MAX" : amount.ToString("0.##"));
        }

        if (Eq(v, "GiveHeroMorality"))
        {
            var token = line.Arg(0);
            if (token.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ScriptLine.TryFloat(token, out var amount);
            ctx.World.GiveHeroMorality(amount);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                amount.ToString("0.##"));
        }

        if (Eq(v, "GiveHeroExpression"))
        {
            var name = line.Arg(0);
            if (name.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            var flag = ScriptLine.IsTrue(line.Arg(1));
            var param = -1;
            if (line.Arg(2).Length > 0)
                ScriptLine.TryInt(line.Arg(2), out param);
            ctx.World.GiveHeroExpression(name, param, flag);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, name);
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
    /// <c>00CD0071</c>: arg0 required. Walk extras
    /// 12-byte records. Name match →
    /// <c>vtbl+432(item,0,1)</c>. Empty list continue.
    /// Separate from Remove world lookup.
    /// </summary>
    internal static CommandResult ApplyRemoveEffect(
        ScriptLine line, ScriptExecutionContext ctx)
    {
        var name = line.Arg(0);
        if (name.Length == 0)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
        if (!ctx.World.RemoveEffect(name))
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, name);
        ctx.Runtime.RemoveThing(name);
        ctx.Bindings.Unbind(name);
        return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, name,
            $"unbind {name}");
    }

    /// <summary>
    /// <c>00CCBBEE</c>: arg0 type + arg1 marker required.
    /// Optional arg2 name (default empty CString
    /// <c>0x122D70E</c>). Optional arg3 Z added to
    /// marker pos. <c>vtbl+400</c>. Continue
    /// <c>00CC864B</c>.
    /// </summary>
    /// <summary>
    /// <c>00CCC29A</c>: three required args else
    /// <c>00CD17FD</c>. Optional arg4 appends to
    /// name. IsTrue(arg5) skips if persist/binding
    /// already has the name. <c>vtbl+364</c>
    /// <c>008A9100</c> at marker pos. Empty or
    /// IsTrue(arg3) extras <c>008ADF90</c>. Not
    /// IsFalse(arg6) persist-binds <c>00CD3D2E</c>.
    /// </summary>
    internal static CommandResult ApplyCreate(
        ScriptLine line, ScriptExecutionContext ctx)
    {
        var type = line.Arg(0);
        var marker = line.Arg(1);
        var name = line.Arg(2);
        if (type.Length == 0 || marker.Length == 0 || name.Length == 0)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
        if (line.Arg(4).Length > 0)
            name += line.Arg(4);
        if (ScriptLine.IsTrue(line.Arg(5)) && ctx.Bindings.Resolve(name) is not null)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, name);
        var markerThing = ctx.FindThing(marker);
        var pos = markerThing is { PositionX: not null }
            ? RegionTravel.PositionOf(markerThing)
            : (System.Numerics.Vector3?)null;
        var extras = line.Arg(3).Length == 0 || ScriptLine.IsTrue(line.Arg(3));
        var spawned = ctx.World.Spawn(type, marker, name, pos, extras);
        ctx.Runtime.AddThing(spawned);
        if (!ScriptLine.IsFalse(line.Arg(6)))
            ctx.Bindings.BindCreated(name, type, marker, pos, spawned);
        return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
            $"{type}->{name}", extras ? $"Created:{name}" : "");
    }

    internal static CommandResult ApplyCreateEffect(
        ScriptLine line, ScriptExecutionContext ctx)
    {
        var type = line.Arg(0);
        var marker = line.Arg(1);
        if (type.Length == 0 || marker.Length == 0)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
        var name = line.Arg(2);
        var z = 0f;
        if (line.Arg(3).Length > 0)
            ScriptLine.TryFloat(line.Arg(3), out z);
        var markerThing = ctx.FindThing(marker);
        Vector3? pos = null;
        if (markerThing is { PositionX: not null })
        {
            var p = RegionTravel.PositionOf(markerThing);
            pos = new Vector3(p.X, p.Y, p.Z + z);
        }
        else if (ctx.World.Positions.TryGetValue(marker, out var stored))
            pos = new Vector3(stored.X, stored.Y, stored.Z + z);
        else
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, marker);
        var spawned = ctx.World.SpawnEffect(type, marker, name, pos);
        ctx.Runtime.AddThing(spawned);
        if (name.Length > 0)
            ctx.Bindings.BindCreated(name, type, marker, pos, spawned);
        return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
            $"{type}->{name} z={z:0.##}",
            name.Length > 0 ? $"Created:{name}" : "");
    }

    /// <summary>
    /// <c>00CCB986</c>: nine required args. Marker
    /// lookup arg0. atof arg1-3 → bytes via
    /// <c>00BFEA70</c> (R,G,B). atof arg4/5 floats.
    /// arg6 &gt; 0 flag. arg7 name. IsTrue(arg8)
    /// extras. <c>vtbl+408</c>. Continue
    /// <c>00CC864B</c>.
    /// </summary>
    internal static CommandResult ApplyCreateLight(
        ScriptLine line, ScriptExecutionContext ctx)
    {
        for (var i = 0; i < 9; i++)
        {
            if (line.Arg(i).Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
        }

        var marker = line.Arg(0);
        var name = line.Arg(7);
        ScriptLine.TryFloat(line.Arg(1), out var rf);
        ScriptLine.TryFloat(line.Arg(2), out var gf);
        ScriptLine.TryFloat(line.Arg(3), out var bf);
        ScriptLine.TryFloat(line.Arg(4), out var p0);
        ScriptLine.TryFloat(line.Arg(5), out var p1);
        ScriptLine.TryFloat(line.Arg(6), out var flagf);
        var extras = ScriptLine.IsTrue(line.Arg(8));
        var markerThing = ctx.FindThing(marker);
        Vector3? pos = null;
        if (markerThing is { PositionX: not null })
            pos = RegionTravel.PositionOf(markerThing);
        else if (ctx.World.Positions.TryGetValue(marker, out var stored))
            pos = stored;
        else
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, marker);
        var spawned = ctx.World.SpawnLight(
            marker, name, FloatToByte(rf), FloatToByte(gf), FloatToByte(bf),
            p0, p1, flagf > 0f, pos, extras);
        ctx.Runtime.AddThing(spawned);
        if (name.Length > 0)
            ctx.Bindings.BindCreated(name, "Light", marker, pos, spawned);
        return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
            $"{name} {FloatToByte(rf)},{FloatToByte(gf)},{FloatToByte(bf)}",
            name.Length > 0 ? $"Created:{name}" : "");
    }

    /// <summary>
    /// <c>00BFEA70</c> fistp → <c>al</c>.
    /// </summary>
    internal static byte FloatToByte(float value) =>
        unchecked((byte)(int)value);

    /// <summary>
    /// <c>00CCBDB6</c>: type,marker,arg2 required.
    /// Optional arg3 name (default empty).
    /// <c>vtbl+404</c> not CreateEffect 400.
    /// Continue <c>00CC864B</c>.
    /// </summary>
    internal static CommandResult ApplyDummyEffect(
        ScriptLine line, ScriptExecutionContext ctx)
    {
        var type = line.Arg(0);
        var marker = line.Arg(1);
        var param = line.Arg(2);
        if (type.Length == 0 || marker.Length == 0 || param.Length == 0)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
        var name = line.Arg(3);
        var markerThing = ctx.FindThing(marker);
        Vector3? pos = null;
        if (markerThing is { PositionX: not null })
            pos = RegionTravel.PositionOf(markerThing);
        else if (ctx.World.Positions.TryGetValue(marker, out var stored))
            pos = stored;
        else
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, marker);
        var spawned = ctx.World.SpawnDummy(type, marker, name, param, pos);
        ctx.Runtime.AddThing(spawned);
        if (name.Length > 0)
            ctx.Bindings.BindCreated(name, type, marker, pos, spawned);
        return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
            $"{type}/{param}->{name}",
            name.Length > 0 ? $"Created:{name}" : "");
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
    /// <c>00CCA609</c>: five required args. Lookup
    /// arg0. atof arg1 param, arg2-4 xyz.
    /// <c>vtbl+1616</c>(thing,xyz,param).
    /// Yield <c>00CC907D</c>.
    /// </summary>
    internal static CommandResult ApplyCameraRotateThing(
        ScriptLine line, ScriptExecutionContext ctx)
    {
        var name = line.Arg(0);
        if (name.Length == 0 || line.Arg(1).Length == 0 ||
            line.Arg(2).Length == 0 || line.Arg(3).Length == 0 ||
            line.Arg(4).Length == 0)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
        ScriptLine.TryFloat(line.Arg(1), out var param);
        ScriptLine.TryFloat(line.Arg(2), out var x);
        ScriptLine.TryFloat(line.Arg(3), out var y);
        ScriptLine.TryFloat(line.Arg(4), out var z);
        ctx.Camera.Rotate(
            ctx.Runtime.Camera, ctx.FindThing(name), name,
            param, new Vector3(x, y, z));
        return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Global,
            "CameraRotateThing vtbl+1616", $"{name} {param:0.##}");
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

    /// <summary>
    /// <c>00CD0CE4</c>: seven required args.
    /// atof arg0-4. Arg5 comma-split RGB * 1/255
    /// if three tokens. Arg6 comma-split
    /// <c>ALL:</c> / <c>ALLDEF:</c> / thing.
    /// <c>vtbl+2700</c> writes handle to
    /// <c>[ebp-112]</c>. Continue <c>00CD17FD</c>.
    /// </summary>
    internal static CommandResult ApplyTintScreenTo(
        ScriptLine line, ScriptExecutionContext ctx)
    {
        for (var i = 0; i < 7; i++)
        {
            if (line.Arg(i).Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
        }

        ScriptLine.TryFloat(line.Arg(0), out var a0);
        ScriptLine.TryFloat(line.Arg(1), out var a1);
        ScriptLine.TryFloat(line.Arg(2), out var a2);
        ScriptLine.TryFloat(line.Arg(3), out var a3);
        ScriptLine.TryFloat(line.Arg(4), out var a4);
        var rgb = ParseTintRgb(line.Arg(5));
        var filters = SplitComma(line.Arg(6));
        var targets = new List<string>();
        foreach (var filter in filters)
        {
            if (ScriptLine.TokenMatches(filter, "ALLDEF:"))
            {
                var type = filter.Length > 7 ? filter[7..] : "";
                foreach (var thing in ctx.World.CollectByType(ctx.Runtime.Things, type))
                {
                    if (thing.ScriptName is { Length: > 0 } n)
                        targets.Add(n);
                }

                continue;
            }

            if (ScriptLine.TokenMatches(filter, "ALL:"))
            {
                var prefix = filter.Length > 4 ? filter[4..] : "";
                foreach (var thing in ctx.World.CollectByName(ctx.Runtime.Things, prefix))
                {
                    if (thing.ScriptName is { Length: > 0 } n)
                        targets.Add(n);
                }

                continue;
            }

            if (ctx.FindThing(filter) is { ScriptName: { Length: > 0 } name })
                targets.Add(name);
            else
                targets.Add(filter);
        }

        var handle = ctx.Camera.TintTo(a0, a1, a2, a3, a4, rgb, filters, targets);
        ctx.Cutscene.TintHold = handle;
        return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
            $"hold={handle} rgb={rgb.X:0.###},{rgb.Y:0.###},{rgb.Z:0.###}");
    }

    internal const float TintRgbScale = 1f / 255f;

    internal static Vector3 ParseTintRgb(string raw)
    {
        var parts = SplitComma(raw);
        if (parts.Count != 3)
            return default;
        ScriptLine.TryFloat(parts[0], out var r);
        ScriptLine.TryFloat(parts[1], out var g);
        ScriptLine.TryFloat(parts[2], out var b);
        return new Vector3(r * TintRgbScale, g * TintRgbScale, b * TintRgbScale);
    }

    internal static List<string> SplitComma(string raw)
    {
        var list = new List<string>();
        if (raw.Length == 0)
            return list;
        var start = 0;
        for (var i = 0; i <= raw.Length; i++)
        {
            if (i < raw.Length && raw[i] != ',')
                continue;
            var token = raw[start..i].Trim();
            if (token.Length > 0)
                list.Add(token);
            start = i + 1;
        }

        return list;
    }

    /// <summary>
    /// <c>00CD1425</c>: atoi arg0 indexes <c>+96</c>
    /// scene strings. Out of range <c>00CD17FD</c>.
    /// Blacks <c>+84</c> defs then applies comma
    /// indices via <c>vtbl+2180</c>. Yield if
    /// <c>[ebp+103]</c>.
    /// </summary>
    internal static CommandResult ApplySetLightScene(
        ScriptLine line, ScriptExecutionContext ctx)
    {
        if (line.Arg(0).Length == 0)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
        if (!ScriptLine.TryInt(line.Arg(0), out var index))
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
        if ((uint)index >= (uint)ctx.Cutscene.LightScenes.Count)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                index.ToString(System.Globalization.CultureInfo.InvariantCulture));
        ctx.World.ApplyLightScene(ctx.Cutscene.LightDefs, ctx.Cutscene.LightScenes, index);
        var side = index.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (!ctx.Cutscene.YieldEnable)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, side);
        return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Global,
            "SetLightScene vtbl+2180", side);
    }

    /// <summary>
    /// <c>00CC9436</c>: six required args else
    /// <c>00CD17FD</c>. Lookup arg0/arg1. atof
    /// arg2-4 offset. arg5 * 15 loop count.
    /// Each iter: <c>vtbl+1892</c> A to B+off,
    /// <c>vtbl+1644</c>, yield if <c>[ebp+103]</c>.
    /// </summary>
    internal static CommandResult ApplyCameraRig(
        ScriptLine line, ScriptExecutionContext ctx)
    {
        for (var i = 0; i < 6; i++)
        {
            if (line.Arg(i).Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
        }

        var nameA = line.Arg(0);
        var nameB = line.Arg(1);
        var offset = ReadOffset(line, 2);
        ScriptLine.TryFloat(line.Arg(5), out var seconds);
        var frames = seconds * RegionTravel.GamePauseScale;
        if (frames <= 0f)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "0");
        var b = ctx.FindThing(nameB);
        Vector3? dest = b is { PositionX: not null }
            ? RegionTravel.PositionOf(b) + offset
            : null;
        ctx.World.Teleport(nameA, nameB, dest);
        ctx.Camera.Rig(
            ctx.Runtime.Camera, ctx.Runtime.Things, b, nameA, nameB, offset, seconds);
        var side = dest is { } d
            ? $"{nameA}@{d.X:0.##},{d.Y:0.##},{d.Z:0.##} {seconds:0.##}s"
            : $"{nameA}|{nameB} {seconds:0.##}s";
        if (!ctx.Cutscene.YieldEnable)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, side);
        ctx.Cutscene.GamePauseTarget = frames;
        ctx.Cutscene.GamePauseCounter = 0f;
        ctx.Cutscene.GamePausePhase = 1;
        return CommandResult.Wait(
            ExecutionKind.WaitScaledFrames, CommandStatus.Proven, CommandFamily.Global,
            "CameraRig vtbl+1644 loop", "scaled-frames", null, side,
            advanceWhenDone: true);
    }

    /// <summary>
    /// <c>00CC9710</c>: seven required args else
    /// <c>00CD17FD</c>. Lookup arg0-5. atof arg6
    /// duration. Optional arg7 FOV degrees *
    /// <c>1/360</c> (default -1). IsFalse(arg8)
    /// disables best-score filter. Arg9 present
    /// takes unread <c>vtbl+1648</c>; else
    /// <c>vtbl+1632</c>. <c>jmp 00CC864B</c>.
    /// </summary>
    internal static CommandResult ApplyUseCameraFovMarkerList(
        ScriptLine line, ScriptExecutionContext ctx)
    {
        for (var i = 0; i < 7; i++)
        {
            if (line.Arg(i).Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
        }

        var nameA = line.Arg(0);
        var nameB = line.Arg(1);
        var names = new[] { line.Arg(2), line.Arg(3), line.Arg(4), line.Arg(5) };
        var a = ctx.FindThing(nameA);
        var b = ctx.FindThing(nameB);
        var markers = new ThingInstance?[4];
        for (var i = 0; i < 4; i++)
        {
            markers[i] = ctx.FindThing(names[i]);
            if (markers[i] is null || a is null || b is null)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
        }

        ScriptLine.TryFloat(line.Arg(6), out var duration);
        var fov = -1f;
        if (line.Arg(7).Length > 0)
            ScriptLine.TryFloat(line.Arg(7), out fov);
        var pickBest = !ScriptLine.IsFalse(line.Arg(8));
        var applyLook = line.Arg(9).Length == 0;
        ctx.Camera.FovMarkerList(
            ctx.Runtime.Camera, a, nameA, b, nameB,
            markers, names, duration, fov, pickBest, applyLook);
        var selected = ctx.Camera.FovMarkerSelected;
        return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
            selected.Length > 0
                ? $"{nameA}|{nameB}->{selected} d={duration:0.##}"
                : $"{nameA}|{nameB} d={duration:0.##}");
    }

    /// <summary>
    /// <c>00CCAF70</c>: five required args. Lookup
    /// arg0-3 as things. atof arg4 duration.
    /// <c>vtbl+1640</c>(pos0,pos2,pos1,pos3,dur).
    /// Continue <c>00CC864B</c>.
    /// </summary>
    internal static CommandResult ApplyCameraPath(
        ScriptLine line, ScriptExecutionContext ctx)
    {
        var a = line.Arg(0);
        var b = line.Arg(1);
        var c = line.Arg(2);
        var d = line.Arg(3);
        if (a.Length == 0 || b.Length == 0 || c.Length == 0 ||
            d.Length == 0 || line.Arg(4).Length == 0)
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
        ScriptLine.TryFloat(line.Arg(4), out var duration);
        ctx.Camera.Path(
            ctx.Runtime.Camera,
            ctx.FindThing(a), a,
            ctx.FindThing(b), b,
            ctx.FindThing(c), c,
            ctx.FindThing(d), d,
            duration);
        return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
            $"{a},{b},{c},{d} d={duration:0.##}");
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

    /// <summary>
    /// <c>00CBF9DE</c>: <c>HERO</c> is vtbl+280,
    /// else persist / thing lookup.
    /// </summary>
    private static ThingInstance? ResolveSoundSource(ScriptExecutionContext ctx, string source)
    {
        if (source.Equals("HERO", StringComparison.OrdinalIgnoreCase))
            return ctx.FindThing("HERO") ?? ctx.FindThing("Hero");
        return ctx.FindThing(source);
    }

    private static bool Eq(string a, string b) =>
        a.Equals(b, StringComparison.OrdinalIgnoreCase);
}
