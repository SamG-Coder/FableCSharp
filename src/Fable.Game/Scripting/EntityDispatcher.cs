namespace Fable.Game.Scripting;

/// <summary>
/// Entity <c>target.verb</c> tokens. Wait/yield comes
/// from parsed options, not a verb table.
/// </summary>
public static class EntityDispatcher
{
    public static CommandResult Dispatch(ScriptLine line, ScriptExecutionContext ctx)
    {
        if (string.IsNullOrEmpty(line.Target))
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "");

        var v = line.Verb;
        if (Eq(v, "Teleport"))
        {
            var marker = line.Arg(0);
            var thing = ctx.FindThing(marker);
            System.Numerics.Vector3? pos = thing is { PositionX: not null }
                ? RegionTravel.PositionOf(thing)
                : null;
            ctx.World.Teleport(line.Target, marker, pos);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity,
                $"{line.Target}->{marker}");
        }

        if (Eq(v, "LookToThing"))
        {
            if (line.Arg(0).Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "");
            ctx.World.LookToThings.Add(new ScriptLookToThing(line.Target, string.Join(",", line.Args)));
            ctx.World.LookTargets[line.Target] = line.Arg(0);
            if (line.Args.Count >= 3 && ScriptLine.IsFalse(line.Arg(2)))
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, line.Arg(0));
            return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Entity,
                "LookToThing vtbl+28", line.Arg(0));
        }

        if (Eq(v, "LookInDirection"))
        {
            if (line.Arg(0).Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "");
            ScriptLine.TryFloat(line.Arg(0), out var degrees);
            var flag = !ScriptLine.IsFalse(line.Arg(1));
            ctx.World.Looks.Add(new ScriptLookInDirection(line.Target, degrees, flag));
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity,
                degrees.ToString("0.###"));
        }

        if (Eq(v, "LookAt") || Eq(v, "LookAtNothing"))
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, v);

        if (Eq(v, "PlayAnimation"))
        {
            var name = line.Arg(0);
            if (name.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "");
            var flags = ParseAnimFlags(line);
            var op = ctx.Animation.Play(
                line.Target, name, flags.F1, flags.F2, flags.F3, flags.F4, flags.F5);
            if (!ctx.Cutscene.AnimationPauseEnabled)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, name);
            return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Entity,
                "PlayAnimation vtbl+72", name, op.Id);
        }

        if (Eq(v, "PlayLoopingAnim"))
        {
            var name = line.Arg(0);
            if (name.Length == 0 || line.Arg(1).Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "");
            ScriptLine.TryInt(line.Arg(1), out var loops);
            var flags = ParseLoopingFlags(line);
            var op = ctx.Animation.PlayLoop(
                line.Target, name, loops, flags.F1, flags.F2, flags.F3, flags.F4, flags.F5);
            if (!ctx.Cutscene.AnimationPauseEnabled)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, name);
            return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Entity,
                "PlayLoopingAnim vtbl+80", name, op.Id);
        }

        if (Eq(v, "WaitPlayAnimation"))
        {
            var op = ctx.Animation.Current(line.Target);
            return CommandResult.Wait(
                ExecutionKind.WaitOperation, CommandStatus.Proven, CommandFamily.Entity,
                "WaitPlayAnimation", "anim-complete", op?.Id, line.Arg(0));
        }

        if (Eq(v, "PlayCombatAnimation") || Eq(v, "PlayCombatAnim"))
        {
            var name = line.Arg(0);
            if (name.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "");
            var parsed = ParseCombat(line);
            var op = ctx.Animation.PlayCombat(
                line.Target, parsed.Name, parsed.A, parsed.B, parsed.C, parsed.D, parsed.E, parsed.Count);
            return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Entity,
                "PlayCombatAnim vtbl+28", name, op.Id);
        }

        if (Eq(v, "SneakTo") || Eq(v, "WalkTo") || Eq(v, "RunTo"))
        {
            var marker = line.Arg(0);
            if (marker.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "");
            var speed = Eq(v, "SneakTo")
                ? RegionTravel.SneakToDefaultSpeed
                : 0.3f;
            if (line.Arg(1).Length > 0 && ScriptLine.TryFloat(line.Arg(1), out var parsedSpeed))
                speed = parsedSpeed;
            var wait = ScriptLine.IsTrue(line.Arg(2)) || ScriptLine.IsTrue(line.Arg(3));
            var destThing = ctx.FindThing(marker);
            System.Numerics.Vector3? dest = destThing is { PositionX: not null }
                ? RegionTravel.PositionOf(destThing)
                : ctx.World.Positions.TryGetValue(marker, out var p) ? p : null;
            PendingOperation op;
            if (Eq(v, "SneakTo"))
                op = ctx.Movement.Sneak(line.Target, marker, speed, wait, dest);
            else if (Eq(v, "WalkTo"))
                op = ctx.Movement.Walk(line.Target, marker, speed, wait, dest);
            else
                op = ctx.Movement.Run(line.Target, marker, speed, wait, dest);
            ctx.Movement.SeedStart(line.Target, ctx.FindThing(line.Target), ctx.World);
            if (Eq(v, "WalkTo") && wait)
                return CommandResult.Wait(
                    ExecutionKind.WaitOperation, CommandStatus.Proven, CommandFamily.Entity,
                    "WalkTo TRUE wait", "arrival leftover", op.Id, marker);
            if (Eq(v, "SneakTo") && wait)
                return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Entity,
                    "SneakTo TRUE leftover once", marker, op.Id);
            return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Entity,
                $"{v} vtbl+20 stub", marker, op.Id);
        }

        if (Eq(v, "FollowThing"))
        {
            var target = line.Arg(0);
            if (target.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "");
            var followSpeed = 1f;
            if (line.Arg(1).Length > 0 && ScriptLine.TryFloat(line.Arg(1), out var parsedFollow))
                followSpeed = parsedFollow;
            var followThing = ctx.FindThing(target);
            System.Numerics.Vector3? followDest = followThing is { PositionX: not null }
                ? RegionTravel.PositionOf(followThing)
                : ctx.World.Positions.TryGetValue(target, out var fp) ? fp : null;
            var followOp = ctx.Movement.Follow(line.Target, target, followSpeed, followDest);
            ctx.Movement.SeedStart(line.Target, ctx.FindThing(line.Target), ctx.World);
            return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Entity,
                "FollowThing actor-vtbl+28", target, followOp.Id);
        }

        if (Eq(v, "StopFollowingThing"))
        {
            ctx.Movement.Clear(line.Target);
            return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Entity,
                "StopFollowingThing actor-vtbl+32", line.Arg(0));
        }

        if (Eq(v, "WalkUpToThing"))
        {
            var target = line.Arg(0);
            if (target.Length == 0 || line.Arg(1).Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "");
            if (!ScriptLine.TryFloat(line.Arg(1), out var distance))
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "");
            var thing = ctx.FindThing(target);
            if (thing is not { PositionX: not null } &&
                !ctx.World.Positions.TryGetValue(target, out _))
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, target);
            var pos = thing is { PositionX: not null }
                ? RegionTravel.PositionOf(thing)
                : ctx.World.Positions[target];
            var forward = thing is not null
                ? RegionTravel.ForwardOf(thing)
                : System.Numerics.Vector3.UnitY;
            var dest = RegionTravel.WalkUpToDestination(pos, forward, distance);
            var op = ctx.Movement.Walk(
                line.Target, target, RegionTravel.WalkUpToThingSpeed, wait: true, dest);
            ctx.Movement.SeedStart(line.Target, ctx.FindThing(line.Target), ctx.World);
            ctx.World.LookTargets[line.Target ?? ""] = target;
            return CommandResult.Wait(
                ExecutionKind.WaitOperation, CommandStatus.Proven, CommandFamily.Entity,
                "WalkUpToThing vtbl+16 then vtbl+104", "arrival leftover", op.Id, target);
        }

        if (Eq(v, "Speak"))
        {
            var target = line.Arg(0);
            var text = line.Arg(1);
            if (target.Length == 0 || text.Length == 0 || ScriptLine.IsNull(text))
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "");
            var mode = SpeakMode(line.Arg(3));
            var hold = ScriptLine.IsTrue(line.Arg(2));
            var body = ctx.Runtime.LookupText(text);
            ctx.Dialogue.Speak(line.Target, target, text, mode, hold, body);
            return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Entity,
                "Speak vtbl+52 leftover vtbl+104", text);
        }

        if (Eq(v, "InteractiveSpeak"))
        {
            var listener = line.Arg(0);
            var prompt = line.Arg(1);
            if (listener.Length == 0 || prompt.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "");
            var wait = ScriptLine.IsTrue(line.Arg(2));
            var op = ctx.Dialogue.InteractiveSpeak(
                line.Target, listener, prompt, wait, line.Arg(3),
                ctx.Runtime.LookupText(prompt));
            if (wait)
                return CommandResult.Wait(
                    ExecutionKind.WaitOperation, CommandStatus.Proven, CommandFamily.Entity,
                    "InteractiveSpeak TRUE vtbl+1472", "dialog", op.Id, prompt);
            return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Entity,
                "InteractiveSpeak FALSE vtbl+28", prompt, op.Id);
        }

        if (Eq(v, "DialogSpeak"))
        {
            var listener = line.Arg(0);
            var text = line.Arg(1);
            if (listener.Length == 0 || text.Length == 0 || ScriptLine.IsNull(text))
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "");
            ctx.Dialogue.DialogSpeak(line.Target, listener, text, ctx.Runtime.LookupText(text));
            return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Entity,
                "DialogSpeak vtbl+28", text);
        }

        if (Eq(v, "DialogadSpeak"))
        {
            var target = line.Arg(0);
            var text = line.Arg(1);
            if (target.Length == 0 || text.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "");
            ctx.Dialogue.DialogAdSpeak(line.Target, target, text, SpeakMode(line.Arg(3)));
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, text);
        }

        if (Eq(v, "WaitTask"))
        {
            ctx.Runtime.NoteWaitTask(line.Target, line.Arg(0));
            var op = ctx.Movement.Current(line.Target) ?? ctx.Animation.Current(line.Target);
            return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Entity,
                "WaitTask leftover vtbl+104", line.Arg(0), op?.Id);
        }

        if (Eq(v, "ClearCommands"))
        {
            ctx.Animation.Clear(line.Target);
            ctx.Movement.Clear(line.Target);
            if (ScriptLine.IsTrue(line.Arg(0)))
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "TRUE");
            return CommandResult.YieldOnce(CommandStatus.Proven, CommandFamily.Entity,
                "ClearCommands vtbl+28", "FALSE");
        }

        if (Eq(v, "AddScriptedMode") || Eq(v, "RemoveScriptedMode"))
        {
            ctx.World.Modes.Add($"{v}:{line.Arg(0)}");
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, line.Arg(0));
        }

        if (Eq(v, "EntitySetMaxWalkingSpeed"))
        {
            ScriptLine.TryFloat(line.Arg(0), out var speed);
            ctx.Movement.WalkSpeed[line.Target] = speed;
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity,
                speed.ToString("0.###"));
        }

        if (Eq(v, "EntitySetMaxRunningSpeed"))
        {
            ScriptLine.TryFloat(line.Arg(0), out var speed);
            ctx.Movement.RunSpeed[line.Target] = speed;
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity,
                speed.ToString("0.###"));
        }

        if (Eq(v, "Drawable"))
        {
            ctx.World.Drawable[line.Target] = !ScriptLine.IsFalse(line.Arg(0));
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, line.Arg(0));
        }

        if (Eq(v, "Collide"))
        {
            ctx.World.Collide[line.Target] = !ScriptLine.IsFalse(line.Arg(0));
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, line.Arg(0));
        }

        if (Eq(v, "SetAlpha"))
        {
            ScriptLine.TryFloat(line.Arg(0), out var alpha);
            ctx.World.Alpha[line.Target] = alpha;
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity,
                alpha.ToString("0.###"));
        }

        if (Eq(v, "Sheathe"))
        {
            var mode = line.Arg(0);
            ctx.World.Sheathe(line.Target ?? "", mode);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, mode);
        }

        if (Eq(v, "HoldInHand"))
        {
            var item = line.Arg(0);
            if (item.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "");
            var flag = ScriptLine.IsTrue(line.Arg(1));
            ctx.World.HoldInHand(line.Target ?? "", item, flag);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, item);
        }

        if (Eq(v, "ModifyHealth"))
        {
            var token = line.Arg(0);
            if (token.Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "");
            ScriptLine.TryFloat(token, out var amount);
            var now = ctx.World.ModifyHealth(line.Target ?? "", amount);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity,
                now.ToString("0.###"));
        }

        if (Eq(v, "SetScared"))
        {
            // 00CC12B7: default 1; IsFalse(arg0) → 0. Empty stays 1.
            var scared = !ScriptLine.IsFalse(line.Arg(0));
            ctx.World.SetScared(line.Target ?? "", scared);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity,
                scared ? "1" : "0");
        }

        if (Eq(v, "SetBound"))
        {
            // 00CC11FD: arg0 required; default 1; IsFalse → 0.
            if (line.Arg(0).Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "");
            var bound = !ScriptLine.IsFalse(line.Arg(0));
            ctx.World.SetBound(line.Target ?? "", bound);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity,
                bound ? "1" : "0");
        }

        if (Eq(v, "Killable"))
        {
            // 00CC1C82: arg0 required; default 1; IsFalse → 0;
            // vtbl+2068(actor,flag,1).
            if (line.Arg(0).Length == 0)
                return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "");
            var killable = !ScriptLine.IsFalse(line.Arg(0));
            ctx.World.SetKillable(line.Target ?? "", killable);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity,
                killable ? "1" : "0");
        }

        if (Eq(v, "SetPushable"))
        {
            // 00CC1144: default 0; IsTrue(arg0) → 1; no empty skip.
            var pushable = ScriptLine.IsTrue(line.Arg(0));
            ctx.World.SetPushable(line.Target ?? "", pushable);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity,
                pushable ? "1" : "0");
        }

        if (Eq(v, "SetDamageable"))
        {
            // 00CC10A6: ignores arg; vtbl+2064(actor,0); 008ADF90.
            ctx.World.SetDamageable(line.Target ?? "");
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "0");
        }

        if (Eq(v, "SetAttackable"))
        {
            // 00CC1008: ignores arg; vtbl+1832(actor,0); 008ADF90.
            ctx.World.SetAttackable(line.Target ?? "");
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "0");
        }

        if (Eq(v, "SetFree"))
        {
            // 00CC0F7E: ignores arg; unary vtbl+1980(actor); no extras.
            ctx.World.SetFree(line.Target ?? "");
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity, "vtbl+1980");
        }

        if (Eq(v, "SetAppearanceSeed"))
        {
            // 00CC4B7E: atoi(arg0); 004AB130; vtbl+1916(actor,seed).
            ScriptLine.TryInt(line.Arg(0), out var seed);
            ctx.World.SetAppearanceSeed(line.Target ?? "", seed);
            return CommandResult.Continue(CommandStatus.Proven, CommandFamily.Entity,
                seed.ToString());
        }

        return CommandResult.Blocked(
            "UNKNOWN", CommandStatus.Unread, CommandFamily.Entity, line.Raw);
    }

    private static (bool F1, bool F2, bool F3, bool F4, bool F5) ParseAnimFlags(ScriptLine line) =>
        (ScriptLine.IsTrue(line.Arg(1)),
            ScriptLine.IsTrue(line.Arg(2)),
            ScriptLine.IsTrue(line.Arg(3)),
            line.Args.Count <= 4 || !ScriptLine.IsFalse(line.Arg(4)),
            ScriptLine.IsTrue(line.Arg(5)));

    /// <summary>
    /// <c>00CC17B3</c>: flags start at arg2 because
    /// arg1 is the <c>0099E7F0</c> loop integer.
    /// </summary>
    private static (bool F1, bool F2, bool F3, bool F4, bool F5) ParseLoopingFlags(ScriptLine line) =>
        (ScriptLine.IsTrue(line.Arg(2)),
            ScriptLine.IsTrue(line.Arg(3)),
            ScriptLine.IsTrue(line.Arg(4)),
            !ScriptLine.IsFalse(line.Arg(5)),
            ScriptLine.IsTrue(line.Arg(6)));

    private static (string Name, bool A, bool B, bool C, bool D, bool E, int Count)
        ParseCombat(ScriptLine line)
    {
        var count = 1;
        if (ScriptLine.TryInt(line.Arg(6), out var n) && n > 0)
            count = n;
        return (
            line.Arg(0),
            line.Args.Count <= 5 || !ScriptLine.IsFalse(line.Arg(5)),
            ScriptLine.IsTrue(line.Arg(2)),
            ScriptLine.IsTrue(line.Arg(3)),
            line.Args.Count <= 4 || !ScriptLine.IsFalse(line.Arg(4)),
            ScriptLine.IsTrue(line.Arg(7)),
            count);
    }

    private static int SpeakMode(string token)
    {
        if (token.Equals("random", StringComparison.OrdinalIgnoreCase))
            return 1;
        if (token.Equals("norepeat", StringComparison.OrdinalIgnoreCase))
            return 2;
        if (token.Equals("sequence", StringComparison.OrdinalIgnoreCase))
            return 3;
        return 0;
    }

    private static bool Eq(string a, string b) =>
        a.Equals(b, StringComparison.OrdinalIgnoreCase);
}
