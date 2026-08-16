using System.Globalization;
using System.Numerics;
using Fable.Formats.Tng;
using Fable.Formats.Wld;

namespace Fable.Game;

/// <summary>
/// New-game start and region-exit walk.
/// Kid start is WLD region <c>StartOakVale</c> / map <c>StartOakValeWest</c>
/// at <c>NOVStartHSP</c> (QST <c>AddTestQuest("Q_NewOakValeIntro","NOVStartHSP")</c>,
/// exe <c>00DBDE4A</c> <c>StartOakVale</c> / <c>00DBDF09</c> <c>CREATURE_HERO_CHILD</c>).
/// WLD <c>Maps[0]</c> LookoutPoint is the adult overworld first map, not new-game.
/// </summary>
public static class RegionTravel
{
    public const string PlayerStartType = "HOLY_SITE_PLAYER_START";
    public const string NewGameRegion = "StartOakValeWest";
    public const string NewGameStartScript = "NOVStartHSP";
    public const string MainStartScript = "MAIN_START_POSITION";
    public const string ExitType = "REGION_EXIT_POINT";
    public const string EntranceType = "REGION_ENTRANCE_POINT";
    public const string KidCreature = "CREATURE_HERO_CHILD";
    public const string TweenCreature = "CREATURE_HERO_TRAINING";
    public const string AdultCreature = "CREATURE_HERO";

    public static string StartingRegion(WorldFile world) =>
        world.FindMap(NewGameRegion)?.ScriptName
        ?? (world.Maps.Count > 0 ? world.Maps[0].ScriptName : NewGameRegion);

    public static ThingInstance? FindPlayerStart(IEnumerable<ThingInstance> things)
    {
        var starts = things
            .Where(t => t.DefinitionType == PlayerStartType && t.PositionX is not null)
            .ToList();
        return Named(starts, NewGameStartScript)
               ?? Named(starts, "StartOakValeHSP")
               ?? Named(starts, MainStartScript)
               ?? Named(starts, "LookoutPointHSP")
               ?? starts.FirstOrDefault();
    }

    private static ThingInstance? Named(List<ThingInstance> starts, string script) =>
        starts.FirstOrDefault(t =>
            string.Equals(t.ScriptName, script, StringComparison.OrdinalIgnoreCase));

    public static Vector3 PositionOf(ThingInstance thing) =>
        new(thing.PositionX!.Value, thing.PositionY!.Value, thing.PositionZ ?? 0);

    public static Vector3 ForwardOf(ThingInstance thing)
    {
        if (thing.Properties.TryGetValue("CTCPhysicsStandard.RHSetForwardX", out var xs) &&
            thing.Properties.TryGetValue("CTCPhysicsStandard.RHSetForwardY", out var ys) &&
            thing.Properties.TryGetValue("CTCPhysicsStandard.RHSetForwardZ", out var zs) &&
            TryFloat(xs, out var x) && TryFloat(ys, out var y) && TryFloat(zs, out var z))
        {
            var f = new Vector3(x, y, z);
            if (f.LengthSquared() > 1e-6f)
                return Vector3.Normalize(f);
        }

        return Vector3.UnitY;
    }

    public static IReadOnlyList<RegionExit> ActiveExits(IEnumerable<ThingInstance> things)
    {
        var list = new List<RegionExit>();
        foreach (var thing in things)
        {
            if (thing.DefinitionType != ExitType || thing.PositionX is null)
                continue;
            if (!IsTrue(thing, "CTCDRegionExit.Active"))
                continue;
            if (!thing.Properties.TryGetValue("CTCDRegionExit.EntranceConnectedToUID", out var uidText) ||
                !ulong.TryParse(uidText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var packed))
                continue;
            var radius = ReadFloat(thing, "CTCDRegionExit.Radius") ?? 3.5f;
            list.Add(new RegionExit(thing, RegionLink.Unpack(packed), radius, PositionOf(thing)));
        }

        return list;
    }

    public static RegionExit? HitExit(IEnumerable<RegionExit> exits, Vector3 position)
    {
        foreach (var exit in exits)
        {
            var dx = position.X - exit.Position.X;
            var dy = position.Y - exit.Position.Y;
            if (dx * dx + dy * dy <= exit.Radius * exit.Radius)
                return exit;
        }

        return null;
    }

    public static ThingInstance? FindEntrance(IEnumerable<ThingInstance> destThings, RegionLink link) =>
        link.FindEntrance(destThings);

    private static bool IsTrue(ThingInstance thing, string key) =>
        thing.Properties.TryGetValue(key, out var text) &&
        text.Equals("TRUE", StringComparison.OrdinalIgnoreCase);

    private static float? ReadFloat(ThingInstance thing, string key)
    {
        if (!thing.Properties.TryGetValue(key, out var text) || !TryFloat(text, out var value))
            return null;
        return value;
    }

    private static bool TryFloat(string text, out float value) =>
        float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
}

public readonly record struct RegionExit(
    ThingInstance Thing,
    RegionLink Link,
    float Radius,
    Vector3 Position);
