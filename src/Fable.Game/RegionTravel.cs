using System.Globalization;
using System.Numerics;
using Fable.Formats.Levels;
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
    public const string IntroHeroIsSubjectKey = "CTCCameraPointScriptedSpline.HeroIsSubject";
    public const string IntroAxisUpKey = "CTCCameraPointScriptedSpline.CoordAxisUp";
    public const bool FirstSeenHeroIsSubject = false;
    /// <summary>
    /// <c>00DBDE40</c> first-seen after the map-ready /
    /// <c>00CB7940</c> / <c>[this+80]</c> gates: lookup
    /// <c>CREATURE_HERO_CHILD</c>, then three 60-byte
    /// watchers via <c>00CDD450</c> (push <c>0.1f</c> /
    /// 64 / 1). <c>WatchBarrels</c> callback
    /// <c>00DBE890</c> is first-seen. <c>WatchForGotGold</c>
    /// is <c>00DBE2E0</c>. <c>ManageQuestCoreMarkers</c>
    /// callback <c>00DBE4E0</c> names <c>NOVI_*</c> — later
    /// intro, not first-seen, do not follow off StartOakVale.
    /// Then <c>Q_NewOakValeIntro_PreAttack</c>, write
    /// <c>12.0f</c> at vtbl+2584, lookup <c>HerosOldHouse</c>.
    /// </summary>
    public const uint StartOakValeSetup = 0x00DBDE40;
    public const uint WatchBarrelsCtor = 0x00CDD450;
    public const uint WatchBarrelsCallback = 0x00DBE890;
    public const uint WatchForGotGoldCallback = 0x00DBE2E0;
    public const uint ManageQuestCoreMarkersCallback = 0x00DBE4E0;
    public const uint WatchBarrelsVtbl = 0x012D7A3C;
    public const uint WatchBarrelsIntervalBits = 0x3DCCCCCD;
    public const int WatchBarrelsCapacity = 64;
    public const int WatchBarrelsArg2 = 1;
    public const uint PreAttackQuest = 0x00DBE0C6;
    public const uint HerosOldHouseLookup = 0x00DBE15E;
    public const float PreAttackDuration = 12f;
    public const int PreAttackDurationVtbl = 2584;
    public const bool FirstSeenFollowsNoviLiveFather = false;
    /// <summary>
    /// Registrar <c>00CD6E27</c> binds <c>Q_NewOakValeIntro</c> to
    /// script <c>S_QNOVI</c> and factory <c>00DBEF70</c>.
    /// Factory allocs <c>0x10C</c> and calls ctor <c>00DAAC00</c>
    /// (vtbl <c>0x12D7A28</c>, context at <c>+64</c>).
    /// Slot 0 is the dtor. Slot 1 builds the <c>Main</c> watcher
    /// (<c>00CDD450</c>, callback <c>00CDD440</c>) — not a frame
    /// tick. Slot 2 <c>00DABAC0</c> registers <c>NOVI_*</c> names
    /// then <c>E8 00DBDE40</c> (only caller). Slot 3
    /// <c>00DAADD0</c> clears <c>+80</c>. No <c>E8</c> of slot 2;
    /// the script VM calls <c>[vtbl+8]</c>.
    /// </summary>
    public const string IntroQuest = "Q_NewOakValeIntro";
    public const string IntroQuestPreAttack = "Q_NewOakValeIntro_PreAttack";
    public const string IntroScriptName = "S_QNOVI";
    public const uint IntroQuestFactory = 0x00DBEF70;
    public const uint IntroQuestCtor = 0x00DAAC00;
    public const uint IntroQuestVtbl = 0x012D7A28;
    public const int IntroQuestSize = 0x10C;
    public const uint IntroQuestDtor = 0x00DBEFA0;
    public const uint IntroQuestMainWatcher = 0x00DAACE0;
    public const uint IntroQuestRun = 0x00DABAC0;
    public const uint IntroQuestReset = 0x00DAADD0;
    public const int IntroQuestRunSlot = 2;
    public const uint IntroQuestRunCallsSetup = 0x00DAC295;
    public const uint IntroMainWatcherCallback = 0x00CDD440;
    public const string IntroMainWatcherName = "Main";
    public const uint RenderFrame = 0x00B25950;
    public const int ScriptYieldVtbl = 28;
    public const int ScriptContextOffset = 64;
    public const int PreAttackGateOffset = 80;
    public const int ScriptWaitVtbl = 2584;
    public const bool FirstSeenPlus80WrittenInStartOakVale = false;
    public const bool FirstSeenFadeOpcodeInStartOakVale = false;
    public const bool FirstSeenWatchBarrelsSpawnsBeetle = false;
    public const bool FirstSeenHandsPlayerControl = false;
    public const bool FirstSeenCameraNameInExe = false;
    public const string WatchBarrelsThing = "NOVI_Barrel";
    /// <summary>
    /// Text-script camera matcher <c>00CBF29F</c> strcmp-walks
    /// <c>UseCamera</c> / <c>CameraLookAt</c> /
    /// <c>CameraLookBetween</c> / <c>CameraFOVLookBetween</c>.
    /// Its <c>E8</c> callers are <c>00CBFE3B</c> /
    /// <c>00CC8782</c> / <c>00CD1837</c> — not
    /// <c>00DBDE40</c>. <c>.PlayAnimation</c> lives in the
    /// opcode dispatcher (<c>00CC14B9</c>); that helper
    /// <c>00CBFACA</c> has only <c>00CD0DB2</c> /
    /// <c>00CD0E2E</c>. Fade is <c>00CC4B22</c>
    /// (<c>.FadeIn</c> / <c>.FadeOut</c>). First-seen
    /// <c>S_QNOVI</c> is the native quest object, not these
    /// text opcodes.
    /// </summary>
    public const uint ScriptCameraHooks = 0x00CBF29F;
    public const uint ScriptUseCameraToken = 0x00CBF3AC;
    public const uint ScriptCameraLookAtToken = 0x00CBF3FE;
    public const uint ScriptPlayAnimationToken = 0x00CC14B9;
    public const uint ScriptFadeInOut = 0x00CC4B22;
    public const uint RegisteringScripts = 0x00CB5D80;
    public const uint QuestBaseCtor = 0x00CB8110;
    public const uint QuestBaseVtbl = 0x012C1648;
    public const uint ActionPlayAnimationName = 0x00903570;
    public const bool FirstSeenCallsUseCamera = false;
    public const bool FirstSeenCallsPlayAnimationDispatcher = false;
    public const bool FirstSeenScriptBinHasSqnovi = false;
    public const string IntroCutscene = "CS_OAKVALE_INTRO_FATHER";
    public const bool FirstSeenScriptBinHasIntroCutscene = true;
    public static float WatchBarrelsInterval =>
        System.BitConverter.Int32BitsToSingle(unchecked((int)WatchBarrelsIntervalBits));

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
    /// <c>00B314E0</c> copies helper <c>+24</c> as up. SHOT2
    /// <c>CoordAxisUp</c> is <c>(0,0,1)</c>.
    /// </summary>
    public static Vector3 IntroCameraUp(ThingInstance thing) =>
        TryCoord(thing, IntroAxisUpKey, out var up) && up.LengthSquared() > 1e-8f
            ? Vector3.Normalize(up)
            : LandscapeFrustum.FirstSeenCameraUp;

    /// <summary>
    /// SHOT2 <c>HeroIsSubject=FALSE</c>. A TRUE subject would be
    /// follow-cam; first-seen <c>00B314E0</c> does not add a hero
    /// offset (<see cref="LandscapeFrustum.FirstSeenUsesThirdPersonView"/>).
    /// </summary>
    public static bool IntroHeroIsSubject(ThingInstance thing) =>
        thing.Properties.TryGetValue(IntroHeroIsSubjectKey, out var value)
        && value.Equals("TRUE", StringComparison.OrdinalIgnoreCase);

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
        if (TryFloatProp(best, "CTCCameraPointScriptedSpline.KeyCameras[0].FOV", out var turns) ||
            TryFloatProp(best, "CTCCameraPointScriptedSpline.FOV", out turns))
            fovDegrees = turns * 360f;
        else if (best.Properties.TryGetValue("CTCCameraPointScripted.FOV", out var fovText) &&
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

    private static bool TryFloatProp(ThingInstance thing, string key, out float value)
    {
        value = 0f;
        return thing.Properties.TryGetValue(key, out var text) && TryFloat(text, out value);
    }

    private static bool TryFloat(string text, out float value) =>
        float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
}

public readonly record struct RegionExit(
    ThingInstance Thing,
    RegionLink Link,
    float Radius,
    Vector3 Position);
