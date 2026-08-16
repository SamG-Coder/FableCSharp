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
    public const string IntroCameraPrefix = "CAM_OVIF_SHOT";
    public const float IntroCameraFovDegrees = 72f;

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

    /// <summary>
    /// First-seen Oakvale intro view from TNG <c>CAM_OVIF_SHOT*</c> next to
    /// <c>NOVStartHSP</c>. Lowest shot number wins. Spline key 0 is used when
    /// the thing is <c>CAMERA_POINT_SCRIPTED_SPLINE</c>. FOV 0.2 on splines
    /// is not degrees; scripted cams store 72.
    /// </summary>
    public static bool TryIntroCamera(
        IEnumerable<ThingInstance> things, out Vector3 position, out Vector3 lookAt, out float fovDegrees)
    {
        position = default;
        lookAt = default;
        fovDegrees = IntroCameraFovDegrees;
        ThingInstance? best = null;
        var bestShot = int.MaxValue;
        foreach (var thing in things)
        {
            if (thing.PositionX is null || thing.ScriptName is null)
                continue;
            if (!thing.ScriptName.StartsWith(IntroCameraPrefix, StringComparison.OrdinalIgnoreCase))
                continue;
            var tail = thing.ScriptName[IntroCameraPrefix.Length..];
            var digits = new string(tail.TakeWhile(char.IsDigit).ToArray());
            if (!int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out var shot))
                continue;
            if (shot >= bestShot)
                continue;
            bestShot = shot;
            best = thing;
        }

        if (best is null)
            return false;

        position = PositionOf(best);
        var look = Vector3.UnitY;
        if (TryCoord(best, "CTCCameraPointScriptedSpline.KeyCameras[0].Position", out var keyPos))
            position += keyPos;
        if (TryCoord(best, "CTCCameraPointScriptedSpline.KeyCameras[0].LookDirection", out var keyLook) ||
            TryLook(best, "CTCCameraPointScripted.LookDirection", out keyLook))
            look = keyLook;
        if (look.LengthSquared() < 1e-8f)
            look = Vector3.UnitY;
        look = Vector3.Normalize(look);
        lookAt = position + look * 8f;
        if (best.Properties.TryGetValue("CTCCameraPointScripted.FOV", out var fovText) &&
            TryFloat(fovText, out var fov) && fov is >= 20f and <= 120f)
            fovDegrees = fov;
        return true;
    }

    private static bool TryLook(ThingInstance thing, string prefix, out Vector3 value)
    {
        value = default;
        return thing.Properties.TryGetValue(prefix + ".X", out var xs) &&
               thing.Properties.TryGetValue(prefix + ".Y", out var ys) &&
               thing.Properties.TryGetValue(prefix + ".Z", out var zs) &&
               TryFloat(xs, out var x) && TryFloat(ys, out var y) && TryFloat(zs, out var z) &&
               (value = new Vector3(x, y, z)).LengthSquared() > 1e-8f;
    }

    private static bool TryCoord(ThingInstance thing, string key, out Vector3 value)
    {
        value = default;
        return thing.Properties.TryGetValue(key, out var text) && TryC3d(text, out value);
    }

    private static bool TryC3d(string text, out Vector3 value)
    {
        value = default;
        var open = text.IndexOf('(');
        var close = text.LastIndexOf(')');
        if (open < 0 || close <= open)
            return false;
        var parts = text[(open + 1)..close].Split(',');
        if (parts.Length < 3)
            return false;
        if (!TryFloat(parts[0], out var x) || !TryFloat(parts[1], out var y) || !TryFloat(parts[2], out var z))
            return false;
        value = new Vector3(x, y, z);
        return true;
    }

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
