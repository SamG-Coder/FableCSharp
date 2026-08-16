using System.Numerics;
using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Defs;
using Fable.Formats.Levels;
using Fable.Formats.Meshes;
using Fable.Formats.Tng;
using Fable.Formats.Wld;

namespace Fable.Game;

public sealed class WorldGeometry
{
    public const float MeshToWorld = 0.01f;

    public required string Region { get; init; }
    public required IReadOnlyList<string> Regions { get; init; }
    public required IReadOnlyList<MeshTriangle> Triangles { get; init; }
    public required int MeshInstances { get; init; }
    public required int MissingMeshes { get; init; }
    public int PlayerMeshId { get; init; }

    public static WorldGeometry Build(
        GameInstall install,
        string region,
        IEnumerable<ThingInstance> things,
        bool adjacentStaticMaps = true)
    {
        var headerPath = Path.Combine(install.DataRoot, "Defs", "RetailHeaders", "meshdata.h");
        var graphicsPath = Path.Combine(install.DataRoot, "graphics", "graphics.big");
        var enums = File.Exists(headerPath) ? HeaderEnums.Load(headerPath) : null;
        GameBin? defs = null;
        var namesPath = install.FindCompiledDef("names.bin");
        var binPath = install.FindCompiledDef("game.bin");
        if (namesPath is not null && binPath is not null)
            defs = GameBin.Load(binPath, NamesBin.Load(namesPath));
        using var big = BigArchive.Open(graphicsPath);
        var bank = big.SubBanks.First(item => item.Name.Contains("MESH", StringComparison.OrdinalIgnoreCase));
        var entries = big.ReadEntries(bank);
        var byId = entries
            .Where(entry => entry.Type is not 3)
            .GroupBy(entry => entry.Id)
            .ToDictionary(group => group.Key, group => group.First());

        var cache = new Dictionary<uint, MeshFile?>();
        var triangles = new List<MeshTriangle>(200_000);
        var loaded = new List<string>();
        var instances = 0;
        var missing = 0;

        using var levels = new LevelLibrary(install);
        var textureHeader = Path.Combine(install.DataRoot, "Defs", "RetailHeaders", "pc", "textures.h");
        var landscapeEnums = File.Exists(textureHeader) ? HeaderEnums.Load(textureHeader) : null;
        var primaryThings = things as IReadOnlyList<ThingInstance> ?? things.ToList();

        var maps = adjacentStaticMaps
            ? StaticMapsAround(levels.World, install, region)
            : PrimaryOnly(levels.World, region);
        foreach (var map in maps)
        {
            var primary = levels.World.FindMap(region);
            var dx = primary is null ? 0f : map.MapX - primary.MapX;
            var dy = primary is null ? 0f : map.MapY - primary.MapY;
            var beforeTris = triangles.Count;
            var beforeInstances = instances;
            AddTerrain(levels, map.ScriptName, dx, dy, triangles, landscapeEnums);

            var mapThings = IsPrimary(map, region)
                ? primaryThings
                : levels.TryLoadThings(map.ScriptName)?.Things ?? [];
            AddInstances(mapThings, dx, dy, defs, enums, big, byId, cache, triangles, ref instances, ref missing);

            if (triangles.Count > beforeTris || instances > beforeInstances)
                loaded.Add(map.ScriptName);
        }

        if (loaded.Count == 0)
        {
            AddTerrain(levels, region, 0, 0, triangles, landscapeEnums);
            AddInstances(primaryThings, 0, 0, defs, enums, big, byId, cache, triangles, ref instances, ref missing);
            loaded.Add(region);
        }

        var playerMeshId = 0;
        if (IsPrimaryStart(region) &&
            RegionTravel.FindPlayerStart(primaryThings) is { } start)
        {
            playerMeshId = defs?.FindMeshId(RegionTravel.KidCreature)
                           ?? enums?.FindMeshId(RegionTravel.KidCreature)
                           ?? 0;
            if (playerMeshId != 0)
            {
                var hero = CloneAs(start, RegionTravel.KidCreature);
                AddInstances([hero], 0, 0, defs, enums, big, byId, cache, triangles, ref instances, ref missing);
            }
        }

        triangles.AddRange(SkyGeometry.Build(install));

        return new WorldGeometry
        {
            Region = region,
            Regions = loaded,
            Triangles = triangles,
            MeshInstances = instances,
            MissingMeshes = missing,
            PlayerMeshId = playerMeshId,
        };
    }

    private static bool IsPrimaryStart(string region) =>
        region.Equals(RegionTravel.NewGameRegion, StringComparison.OrdinalIgnoreCase);

    private static ThingInstance CloneAs(ThingInstance source, string definitionType) =>
        new()
        {
            Kind = source.Kind,
            Section = source.Section,
            DefinitionType = definitionType,
            ScriptName = source.ScriptName,
            Uid = source.Uid,
            Player = source.Player,
            PositionX = source.PositionX,
            PositionY = source.PositionY,
            PositionZ = source.PositionZ,
            Properties = source.Properties,
        };

    /// <summary>
    /// New-game / region load set. WLD <c>NewRegion</c> <c>ContainsMap</c> +
    /// <c>SeesMap</c> is the cluster the exe groups together (StartOakVale =
    /// West/East/Garden plus fillers and seas). BWD AABBs that touch are
    /// still unioned (<c>OpenStaticMaps</c> / <c>CLandscapeBackgroundPatch</c>).
    /// </summary>
    internal static IReadOnlyList<WorldMap> StaticMapsAround(WorldFile world, GameInstall install, string region)
    {
        var primary = world.FindMap(region);
        if (primary is null)
            return [];

        return PrimaryPlus(primary, world, install);
    }

    private static IReadOnlyList<WorldMap> PrimaryOnly(WorldFile world, string region)
    {
        var primary = world.FindMap(region);
        return primary is null ? [] : [primary];
    }

    private static IReadOnlyList<WorldMap> PrimaryPlus(WorldMap primary, WorldFile world, GameInstall install)
    {
        var maps = new List<WorldMap> { primary };
        var seen = new HashSet<int> { primary.MapUid };

        void TryAdd(WorldMap? map)
        {
            if (map is null || map.IsSea || !seen.Add(map.MapUid))
                return;
            maps.Add(map);
        }

        var cluster = world.FindRegionContaining(primary.ScriptName)
                      ?? world.FindRegionContaining(primary.FileStem);
        if (cluster is not null)
        {
            foreach (var name in cluster.ContainsMaps)
                TryAdd(world.FindMap(name));
            foreach (var name in cluster.SeesMaps)
                TryAdd(world.FindMap(name));
        }

        if (!File.Exists(install.BwdPath))
            return maps;

        var bwd = BwdFile.Load(install.BwdPath);
        var home = bwd.Find(primary.ScriptName) ?? bwd.Find(primary.FileStem);
        if (home is null)
            return maps;

        foreach (var map in world.Maps)
        {
            var box = bwd.Find(map.ScriptName) ?? bwd.Find(map.FileStem);
            if (box is null || !home.Value.Touches(box.Value))
                continue;
            TryAdd(map);
        }

        return maps;
    }

    private static bool IsPrimary(WorldMap map, string region) =>
        map.ScriptName.Equals(region, StringComparison.OrdinalIgnoreCase) ||
        map.FileStem.Equals(region, StringComparison.OrdinalIgnoreCase);

    private static void AddTerrain(
        LevelLibrary levels,
        string region,
        float dx,
        float dy,
        List<MeshTriangle> triangles,
        HeaderEnums? landscapeEnums)
    {
        var height = levels.LoadHeightField(region);
        if (height is null)
            return;

        var compiled = levels.LoadCompiledLev(region);
        var cells = compiled is null ? null : LevCellGrid.TryParse(compiled);
        IEnumerable<MeshTriangle> local;
        if (cells is not null && compiled is not null)
            local = height.ToTileTriangles(cells, compiled.Materials, landscapeEnums);
        else
            local = height.ToLocalTriangles().Select(tri => tri with { TextureId = TextureLibrary.LandscapeGrassPlainId });

        var offset = new Vector3(dx, dy, 0);
        foreach (var tri in local)
            triangles.Add(tri with { A = tri.A + offset, B = tri.B + offset, C = tri.C + offset });
    }

    private static void AddInstances(
        IEnumerable<ThingInstance> things,
        float dx,
        float dy,
        GameBin? defs,
        HeaderEnums? enums,
        BigArchive big,
        Dictionary<uint, BankEntry> byId,
        Dictionary<uint, MeshFile?> cache,
        List<MeshTriangle> triangles,
        ref int instances,
        ref int missing)
    {
        var shift = dx == 0 && dy == 0
            ? Matrix4x4.Identity
            : Matrix4x4.CreateTranslation(dx, dy, 0);

        foreach (var thing in things)
        {
            if (thing.PositionX is null || thing.DefinitionType is null)
                continue;

            var meshId = defs?.FindMeshId(thing.DefinitionType) ?? enums?.FindMeshId(thing.DefinitionType);
            if (meshId is null || !byId.TryGetValue((uint)meshId.Value, out var entry))
            {
                missing++;
                continue;
            }

            if (!cache.TryGetValue(entry.Id, out var mesh))
            {
                mesh = MeshFile.TryParse(big.Read(entry), (int)entry.Type);
                cache[entry.Id] = mesh;
            }

            if (mesh is null)
            {
                missing++;
                continue;
            }

            var transform = ObjectTransform(thing) * shift;
            foreach (var tri in mesh.Triangles)
            {
                var a = Vector3.Transform(tri.A, transform);
                var b = Vector3.Transform(tri.B, transform);
                var c = Vector3.Transform(tri.C, transform);
                var n = Vector3.TransformNormal(tri.Normal, transform);
                if (n.LengthSquared() < 1e-8f)
                    n = Vector3.UnitZ;
                else
                    n = Vector3.Normalize(n);
                triangles.Add(new MeshTriangle(a, b, c, n, tri.UvA, tri.UvB, tri.UvC, tri.TextureId));
            }

            instances++;
        }
    }

    /// <summary>
    /// C3D meshes are Z-up centimetres. TNG RHSetForward/RHSetUp are a
    /// right-handed Z-up basis. CreateWorld is Y-up and negates forward —
    /// that laid lamps and trees on their side.
    /// </summary>
    public static Matrix4x4 ObjectTransform(ThingInstance thing)
    {
        var position = new Vector3(thing.PositionX!.Value, thing.PositionY!.Value, thing.PositionZ!.Value);
        var scale = MeshToWorld * ReadObjectScale(thing);
        var forward = ReadAxis(thing, "CTCPhysicsStandard.RHSetForward", Vector3.UnitY);
        var up = ReadAxis(thing, "CTCPhysicsStandard.RHSetUp", Vector3.UnitZ);
        if (forward.LengthSquared() < 1e-6f)
            forward = Vector3.UnitY;
        if (up.LengthSquared() < 1e-6f)
            up = Vector3.UnitZ;
        forward = Vector3.Normalize(forward);
        up = Vector3.Normalize(up);
        var right = Vector3.Cross(forward, up);
        if (right.LengthSquared() < 1e-8f)
            right = Vector3.Cross(forward, Vector3.UnitZ);
        if (right.LengthSquared() < 1e-8f)
            right = Vector3.UnitX;
        right = Vector3.Normalize(right);
        up = Vector3.Normalize(Vector3.Cross(right, forward));

        var basis = new Matrix4x4(
            right.X, right.Y, right.Z, 0,
            forward.X, forward.Y, forward.Z, 0,
            up.X, up.Y, up.Z, 0,
            position.X, position.Y, position.Z, 1);
        return Matrix4x4.CreateScale(scale) * basis;
    }

    private static float ReadObjectScale(ThingInstance thing)
    {
        if (thing.Properties.TryGetValue("ObjectScale", out var text) &&
            float.TryParse(text, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var value) &&
            value is > 0.01f and < 20f)
            return value;
        return 1f;
    }

    private static Vector3 ReadAxis(ThingInstance thing, string prefix, Vector3 fallback)
    {
        if (thing.Properties.TryGetValue(prefix + "X", out var xText) &&
            thing.Properties.TryGetValue(prefix + "Y", out var yText) &&
            thing.Properties.TryGetValue(prefix + "Z", out var zText) &&
            float.TryParse(xText, out var x) &&
            float.TryParse(yText, out var y) &&
            float.TryParse(zText, out var z))
            return new Vector3(x, y, z);
        return fallback;
    }
}
