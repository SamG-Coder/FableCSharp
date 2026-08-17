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

        if (Eq(v, "PlaySound") || Eq(v, "Play2DSound"))
        {
            ctx.Audio.PlaySound(line.Arg(0));
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, line.Arg(0));
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
            if (!ctx.Cutscene.CameraPauseEnabled)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, name);
            return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Global,
                "UseCamera vtbl+28", name);
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
            var thing = ctx.FindThing(marker);
            var pos = thing is { PositionX: not null } ? RegionTravel.PositionOf(thing) : (System.Numerics.Vector3?)null;
            ctx.World.Creates.Add(new ScriptCreate(type, marker, name));
            ctx.Bindings.BindCreated(name, type, marker, pos);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global,
                $"{type}->{name}", $"Created:{name}");
        }

        if (Eq(v, "Remove") || Eq(v, "RemoveThing"))
        {
            if (Eq(v, "RemoveThing"))
            {
                return CommandResult.Blocked(
                    "UNKNOWN", CommandStatus.Unread, CommandFamily.Global, line.Raw);
            }

            var name = line.Arg(0);
            if (name.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, "");
            ctx.World.Removes.Add(name);
            ctx.Bindings.Unbind(name);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Global, name,
                $"unbind {name}");
        }

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

        return CommandResult.Blocked(
            "UNKNOWN", CommandStatus.Unread, CommandFamily.Global, line.Raw);
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
