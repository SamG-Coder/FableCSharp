using System.Numerics;
using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Defs;
using Fable.Formats.Levels;
using Fable.Formats.Meshes;
using Fable.Formats.Tng;
using Fable.Formats.Wld;
using Fable.Formats.World;

namespace Fable.Game;

public sealed class WorldGeometry
{
    public const float MeshToWorld = 0.01f;

    public required string Region { get; init; }
    public required IReadOnlyList<string> Regions { get; init; }
    public required IReadOnlyList<MeshTriangle> Triangles { get; init; }
    public required int MeshInstances { get; init; }
    public required int MissingMeshes { get; init; }
    public IReadOnlyList<string> MissingMeshDefs { get; init; } = [];
    public IReadOnlyList<WorldMeshInstance> Instances { get; init; } = [];
    public bool Expanded { get; init; } = true;
    public int PlayerMeshId { get; init; }
    public float PlayerHeight { get; init; }
    /// <summary>
    /// Clip names from <c>0070D580</c> inner
    /// play. PALSKIN still bind-pose until
    /// the clip stream is sampled.
    /// </summary>
    public IReadOnlyDictionary<string, string> ActorPoses { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public static WorldGeometry Build(
        GameInstall install,
        string region,
        IEnumerable<ThingInstance> things,
        bool adjacentStaticMaps = true,
        LandscapeFrustum.Plane[]? landscapePlanes = null,
        IReadOnlyDictionary<string, Vector3>? actorPositions = null,
        IReadOnlyDictionary<string, string>? actorPoses = null,
        LevelLibrary? levels = null,
        IReadOnlyList<string>? onlyMaps = null,
        IReadOnlyDictionary<string, IReadOnlyList<ThingInstance>>? thingsByMap = null,
        MeshBank? meshes = null,
        bool expandGeometry = true)
    {
        var ownLevels = levels is null;
        levels ??= new LevelLibrary(install);
        var enums = levels.MeshEnums;
        var defs = levels.Defs;
        var ownMeshes = meshes is null;
        meshes ??= new MeshBank();
        if (!meshes.Opened)
            meshes.Open(install);
        var triangles = expandGeometry ? new List<MeshTriangle>(200_000) : [];
        var loaded = new List<string>();
        var instanceList = new List<WorldMeshInstance>();
        var instances = 0;
        var missing = 0;
        var missingDefs = new List<string>();

        try
        {
        var landscapeEnums = levels.LandscapeEnums;
        var primaryThings = ApplyActorPositions(
            things as IReadOnlyList<ThingInstance> ?? things.ToList(),
            actorPositions);

        var maps = ResolveMaps(levels.World, install, region, adjacentStaticMaps, onlyMaps);
        foreach (var map in maps)
        {
            var primary = levels.World.FindMap(region);
            var neighbour = primary is null
                ? Vector2.Zero
                : WorldSpaces.NeighbourRegionOffset(map.MapX, map.MapY, primary.MapX, primary.MapY);
            var dx = neighbour.X;
            var dy = neighbour.Y;
            if (expandGeometry)
                AddTerrain(levels, map.ScriptName, dx, dy, triangles, landscapeEnums, landscapePlanes);

            // First-seen 0x20 is primary Graphic only.
            // Neighbour TNG stays a handle until draw.
            if (expandGeometry || IsPrimary(map, region))
            {
                IReadOnlyList<ThingInstance> mapThings;
                if (IsPrimary(map, region))
                    mapThings = primaryThings;
                else if (thingsByMap is not null)
                    mapThings = thingsByMap.TryGetValue(map.ScriptName, out var listed) ? listed : [];
                else
                    mapThings = levels.TryLoadThings(map.ScriptName)?.Things.ToList() ?? [];
                AddInstances(
                    mapThings, map.ScriptName, dx, dy, defs, enums, meshes, triangles, instanceList,
                    ref instances, ref missing, missingDefs, expandGeometry);
            }

            // OpenStaticMaps still opens Sees/Contains maps when they emit
            // no landscape tris (sea/water cells are not landscape FG).
            loaded.Add(map.ScriptName);
        }

        if (loaded.Count == 0)
        {
            if (expandGeometry)
                AddTerrain(levels, region, 0, 0, triangles, landscapeEnums, landscapePlanes);
            AddInstances(
                primaryThings, region, 0, 0, defs, enums, meshes, triangles, instanceList,
                ref instances, ref missing, missingDefs, expandGeometry);
            loaded.Add(region);
        }

        var playerMeshId = 0;
        var playerHeight = 0f;
        var existingHero = primaryThings.FirstOrDefault(t =>
            t.DefinitionType is RegionTravel.AdultCreature
                or RegionTravel.TweenCreature
                or RegionTravel.KidCreature);
        if (existingHero is not null)
        {
            playerMeshId = defs?.FindMeshId(existingHero.DefinitionType!)
                           ?? enums?.FindMeshId(existingHero.DefinitionType!)
                           ?? 0;
            if (expandGeometry && playerMeshId != 0 &&
                meshes.Get((uint)playerMeshId) is { } heroMesh)
                playerHeight = (heroMesh.BoundsMax.Z - heroMesh.BoundsMin.Z) * MeshToWorld;
        }
        else if (IsPrimaryStart(region) &&
            RegionTravel.FindPlayerStart(primaryThings) is { } start)
        {
            playerMeshId = defs?.FindMeshId(RegionTravel.KidCreature)
                           ?? enums?.FindMeshId(RegionTravel.KidCreature)
                           ?? 0;
            if (playerMeshId != 0)
            {
                Vector3? heroPos = null;
                if (actorPositions is not null &&
                    actorPositions.TryGetValue(RegionTravel.IntroHeroActor, out var teleported))
                    heroPos = teleported;
                var hero = CloneAs(start, RegionTravel.KidCreature, heroPos);
                AddInstances(
                    [hero], region, 0, 0, defs, enums, meshes, triangles, instanceList,
                    ref instances, ref missing, missingDefs, expandGeometry);
                if (expandGeometry && meshes.Get((uint)playerMeshId) is { } kid)
                    playerHeight = (kid.BoundsMax.Z - kid.BoundsMin.Z) * MeshToWorld;
            }
        }

        if (expandGeometry)
            triangles.AddRange(SkyGeometry.Build(install));

        return new WorldGeometry
        {
            Region = region,
            Regions = loaded,
            Triangles = triangles,
            MeshInstances = instances,
            MissingMeshes = missing,
            MissingMeshDefs = missingDefs,
            Instances = instanceList,
            Expanded = expandGeometry,
            PlayerMeshId = playerMeshId,
            PlayerHeight = playerHeight,
            ActorPoses = actorPoses is null
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(actorPoses, StringComparer.OrdinalIgnoreCase),
        };
        }
        finally
        {
            if (ownLevels)
                levels.Dispose();
            if (ownMeshes)
                meshes.Dispose();
        }
    }

    private static IReadOnlyList<WorldMap> ResolveMaps(
        WorldFile world,
        GameInstall install,
        string region,
        bool adjacentStaticMaps,
        IReadOnlyList<string>? onlyMaps)
    {
        if (onlyMaps is { Count: > 0 })
        {
            var list = new List<WorldMap>();
            var seen = new HashSet<int>();
            foreach (var name in onlyMaps)
            {
                var map = world.FindMap(name);
                if (map is null || !seen.Add(map.MapUid))
                    continue;
                list.Add(map);
            }

            return list;
        }

        return adjacentStaticMaps
            ? StaticMapsAround(world, install, region)
            : PrimaryOnly(world, region);
    }

    private static bool IsPrimaryStart(string region) =>
        region.Equals(RegionTravel.NewGameRegion, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Consume <c>006A9960</c> dest /
    /// <c>World.Positions</c>. Father still
    /// maps <c>NOVI_LiveFather</c> via
    /// <c>00DB86B0</c> / <c>0089B780</c>.
    /// Other script names take their
    /// recorded position. Not a renderer hack.
    /// </summary>
    public static IReadOnlyList<ThingInstance> ApplyActorPositions(
        IReadOnlyList<ThingInstance> things,
        IReadOnlyDictionary<string, Vector3>? actorPositions)
    {
        if (actorPositions is null || actorPositions.Count == 0)
            return things;

        var hasFather = actorPositions.TryGetValue(
            RegionTravel.IntroFatherActor, out var fatherPos);
        var list = new List<ThingInstance>(things.Count);
        foreach (var thing in things)
        {
            if (hasFather &&
                thing.ScriptName is not null &&
                thing.ScriptName.Equals(
                    RegionTravel.LiveFatherScript, StringComparison.OrdinalIgnoreCase))
            {
                list.Add(CloneAs(
                    thing,
                    thing.DefinitionType ?? RegionTravel.LiveFatherCreature,
                    fatherPos));
                continue;
            }

            if (thing.ScriptName is { Length: > 0 } name &&
                actorPositions.TryGetValue(name, out var pos))
            {
                list.Add(CloneAs(
                    thing, thing.DefinitionType ?? name, pos));
                continue;
            }

            list.Add(thing);
        }

        return list;
    }

    private static ThingInstance CloneAs(
        ThingInstance source, string definitionType, Vector3? position = null) =>
        new()
        {
            Kind = source.Kind,
            Section = source.Section,
            DefinitionType = definitionType,
            ScriptName = source.ScriptName,
            Uid = source.Uid,
            Player = source.Player,
            PositionX = position?.X ?? source.PositionX,
            PositionY = position?.Y ?? source.PositionY,
            PositionZ = position?.Z ?? source.PositionZ,
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
        HeaderEnums? landscapeEnums,
        LandscapeFrustum.Plane[]? landscapePlanes)
    {
        // 00BDC2D0 / 00BF6F80: n-vertex AABB from
        // map size, before the cell walk. Do not
        // parse the STB height stream for a
        // rejected neighbour. The current map is
        // the 00B3E820 handle — keep it.
        var header = levels.PeekMapHeader(region);
        var sizeX = header is { GridWidth: > 0 } ? header.Value.GridWidth : 128;
        var sizeY = header is { GridHeight: > 0 } ? header.Value.GridHeight : 128;
        var primary = dx == 0 && dy == 0;
        if (!primary && landscapePlanes is { Length: > 0 })
        {
            LandscapeFrustum.PatchAabb(dx, dy, sizeX, sizeY, out var min, out var max);
            if (LandscapeFrustum.AabbIsOutside(min, max, landscapePlanes))
                return;
        }

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
        string map,
        float dx,
        float dy,
        GameBin? defs,
        HeaderEnums? enums,
        MeshBank meshes,
        List<MeshTriangle> triangles,
        List<WorldMeshInstance> instanceList,
        ref int instances,
        ref int missing,
        List<string> missingDefs,
        bool expandGeometry)
    {
        var shift = dx == 0 && dy == 0
            ? Matrix4x4.Identity
            : Matrix4x4.CreateTranslation(dx, dy, 0);

        foreach (var thing in things)
        {
            var resolved = ResolveSubmit(defs, enums, thing);
            if (resolved.MeshIds.Count == 0)
            {
                if (resolved.AsC3d)
                {
                    missing++;
                    missingDefs.Add(thing.DefinitionType ?? thing.Kind);
                }
                continue;
            }

            var meshIds = resolved.MeshIds;
            var transform = ObjectTransform(thing) * shift;
            var any = false;
            foreach (var meshId in meshIds)
            {
                if (!meshes.TryGetEntry((uint)meshId, out _))
                    continue;
                instanceList.Add(new WorldMeshInstance(
                    (uint)meshId, transform, thing.ScriptName, thing.DefinitionType, map));
                if (!expandGeometry)
                {
                    any = true;
                    instances++;
                    continue;
                }

                var mesh = meshes.Get((uint)meshId);
                if (mesh is null)
                    continue;

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
                    var na = TransformUnitNormal(tri.NormalA, transform);
                    var nb = TransformUnitNormal(tri.NormalB, transform);
                    var nc = TransformUnitNormal(tri.NormalC, transform);
                    triangles.Add(tri with
                    {
                        A = a, B = b, C = c, Normal = n,
                        NormalA = na, NormalB = nb, NormalC = nc,
                    });
                }

                any = true;
                instances++;
            }

            if (!any && resolved.AsC3d)
            {
                missing++;
                missingDefs.Add(thing.DefinitionType ?? thing.Kind);
            }
        }
    }

    /// <summary>
    /// Primary-map landscape only.
    /// Neighbours stay headers.
    /// </summary>
    public List<MeshTriangle> TessellatePrimary(
        LevelLibrary levels,
        LandscapeFrustum.Plane[]? landscapePlanes = null)
    {
        var triangles = new List<MeshTriangle>(64_000);
        AddTerrain(levels, Region, 0, 0, triangles, levels.LandscapeEnums, landscapePlanes);
        return triangles;
    }

    /// <summary>
    /// Visible 16 m cells after
    /// <c>00BDC2D0</c> AABB. Neighbour
    /// offset is ΔMapX/ΔMapY. Current
    /// map is never culled.
    /// </summary>
    public List<LandscapeCell> CollectVisibleCells(
        LevelLibrary levels,
        LandscapeFrustum.Plane[]? landscapePlanes = null,
        ICollection<string>? acceptedMaps = null)
    {
        var list = new List<LandscapeCell>(256);
        var primary = levels.World.FindMap(Region);
        foreach (var name in Regions)
        {
            var map = levels.World.FindMap(name);
            var dx = 0f;
            var dy = 0f;
            if (map is not null && primary is not null)
            {
                var neighbour = WorldSpaces.NeighbourRegionOffset(
                    map.MapX, map.MapY, primary.MapX, primary.MapY);
                dx = neighbour.X;
                dy = neighbour.Y;
            }

            var header = levels.PeekMapHeader(name);
            var sizeX = header is { GridWidth: > 0 } ? header.Value.GridWidth : 128;
            var sizeY = header is { GridHeight: > 0 } ? header.Value.GridHeight : 128;
            var isPrimary = name.Equals(Region, StringComparison.OrdinalIgnoreCase);
            if (!isPrimary && landscapePlanes is { Length: > 0 })
            {
                LandscapeFrustum.PatchAabb(dx, dy, sizeX, sizeY, out var min, out var max);
                if (LandscapeFrustum.AabbIsOutside(min, max, landscapePlanes))
                    continue;
            }

            var offset = new Vector3(dx, dy, 0);
            var before = list.Count;
            foreach (var cell in levels.LoadCells(name))
            {
                if (cell.Faces.Count == 0 &&
                    (cell.ExtraStrips is null || cell.ExtraStrips.Count == 0))
                    continue;
                list.Add(offset == Vector3.Zero ? cell : OffsetCell(cell, offset));
            }

            if (list.Count > before)
                acceptedMaps?.Add(name);
        }

        return list;
    }

    private static LandscapeCell OffsetCell(LandscapeCell cell, Vector3 offset) =>
        cell with
        {
            Min = cell.Min + offset,
            Max = cell.Max + offset,
            Faces = OffsetFaces(cell.Faces, offset),
            Points = OffsetPoints(cell.Points, offset),
            ExtraStrips = OffsetExtraStrips(cell.ExtraStrips, offset),
        };

    private static IReadOnlyList<MeshTriangle> OffsetFaces(
        IReadOnlyList<MeshTriangle> faces, Vector3 offset)
    {
        if (faces.Count == 0)
            return faces;
        var copy = new MeshTriangle[faces.Count];
        for (var i = 0; i < faces.Count; i++)
        {
            var f = faces[i];
            copy[i] = f with { A = f.A + offset, B = f.B + offset, C = f.C + offset };
        }

        return copy;
    }

    private static IReadOnlyList<LandscapePoint>? OffsetPoints(
        IReadOnlyList<LandscapePoint>? points, Vector3 offset)
    {
        if (points is not { Count: > 0 })
            return points;
        var copy = new LandscapePoint[points.Count];
        for (var i = 0; i < points.Count; i++)
        {
            var p = points[i];
            copy[i] = p with { P = p.P + offset };
        }

        return copy;
    }

    private static IReadOnlyList<LandscapeExtraStrip>? OffsetExtraStrips(
        IReadOnlyList<LandscapeExtraStrip>? extras, Vector3 offset)
    {
        if (extras is not { Count: > 0 })
            return extras;
        var copy = new LandscapeExtraStrip[extras.Count];
        for (var i = 0; i < extras.Count; i++)
        {
            var extra = extras[i];
            copy[i] = extra with
            {
                Points = OffsetPoints(extra.Points, offset) ?? extra.Points,
                Faces = OffsetFaces(extra.Faces, offset),
            };
        }

        return copy;
    }

    /// <summary>
    /// <c>00BDC2D0</c> per opened patch:
    /// neighbour offset, Z=0 AABB, four
    /// side planes, then stored STB cells
    /// (<c>00BF4570</c>). Reject is
    /// fully outside. Instances stay
    /// primary-only at submit.
    /// </summary>
    public List<MeshTriangle> TessellateVisible(
        LevelLibrary levels,
        LandscapeFrustum.Plane[]? landscapePlanes = null,
        ICollection<string>? acceptedMaps = null)
    {
        var triangles = new List<MeshTriangle>(64_000);
        var landscapeEnums = levels.LandscapeEnums;
        var primary = levels.World.FindMap(Region);
        foreach (var name in Regions)
        {
            var map = levels.World.FindMap(name);
            var dx = 0f;
            var dy = 0f;
            if (map is not null && primary is not null)
            {
                var neighbour = WorldSpaces.NeighbourRegionOffset(
                    map.MapX, map.MapY, primary.MapX, primary.MapY);
                dx = neighbour.X;
                dy = neighbour.Y;
            }

            var before = triangles.Count;
            AddTerrain(levels, name, dx, dy, triangles, landscapeEnums, landscapePlanes);
            if (triangles.Count > before)
                acceptedMaps?.Add(name);
        }

        return triangles;
    }

    /// <summary>
    /// Draw-time expand. Open keeps
    /// <c>009AD410</c> handles only.
    /// </summary>
    public WorldGeometry Expand(
        GameInstall install,
        LevelLibrary levels,
        MeshBank meshes,
        LandscapeFrustum.Plane[]? landscapePlanes = null,
        bool primaryOnly = false)
    {
        if (Expanded && !primaryOnly)
            return this;
        var triangles = new List<MeshTriangle>(primaryOnly ? 64_000 : 200_000);
        var landscapeEnums = levels.LandscapeEnums;
        var primary = levels.World.FindMap(Region);
        foreach (var name in Regions)
        {
            if (primaryOnly &&
                !name.Equals(Region, StringComparison.OrdinalIgnoreCase))
                continue;
            var map = levels.World.FindMap(name);
            var dx = 0f;
            var dy = 0f;
            if (map is not null && primary is not null)
            {
                var neighbour = WorldSpaces.NeighbourRegionOffset(
                    map.MapX, map.MapY, primary.MapX, primary.MapY);
                dx = neighbour.X;
                dy = neighbour.Y;
            }

            AddTerrain(levels, name, dx, dy, triangles, landscapeEnums, landscapePlanes);
        }

        foreach (var inst in Instances)
        {
            if (primaryOnly &&
                !inst.Map.Equals(Region, StringComparison.OrdinalIgnoreCase))
                continue;
            var mesh = meshes.Get(inst.MeshId);
            if (mesh is null)
                continue;
            var transform = inst.Transform;
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
                triangles.Add(tri with
                {
                    A = a, B = b, C = c, Normal = n,
                    NormalA = TransformUnitNormal(tri.NormalA, transform),
                    NormalB = TransformUnitNormal(tri.NormalB, transform),
                    NormalC = TransformUnitNormal(tri.NormalC, transform),
                });
            }
        }

        triangles.AddRange(SkyGeometry.Build(install));
        IReadOnlyList<string> drawn = primaryOnly
            ? [Region]
            : Regions;
        return new WorldGeometry
        {
            Region = Region,
            Regions = drawn,
            Triangles = triangles,
            MeshInstances = MeshInstances,
            MissingMeshes = MissingMeshes,
            MissingMeshDefs = MissingMeshDefs,
            Instances = Instances,
            Expanded = true,
            PlayerMeshId = PlayerMeshId,
            PlayerHeight = PlayerHeight,
            ActorPoses = ActorPoses,
        };
    }

    /// <summary>
    /// Same lookup <see cref="AddInstances"/> uses: GameBin
    /// <c>TypeName</c> + <see cref="GameBin.FirstSeenInstancesAsC3d"/>
    /// + <see cref="GameBin.FindMeshIds"/>. GAZE / HOLY_SITE are
    /// TypeName MARKER (not a <c>MARKER_</c> prefix) and do not
    /// submit Graphic.
    /// </summary>
    public static ThingSubmit ResolveSubmit(GameBin? defs, HeaderEnums? enums, ThingInstance thing)
    {
        if (thing.PositionX is null || thing.DefinitionType is null)
            return new ThingSubmit(thing.Kind, null, [], false, false);

        var entry = defs?.FindEntry(thing.DefinitionType);
        var typeName = entry?.TypeName;
        var asC3d = GameBin.FirstSeenInstancesAsC3d(typeName, thing.DefinitionType);
        var meshIds = defs?.FindMeshIds(thing.DefinitionType) ?? [];
        if (meshIds.Count == 0 && asC3d)
        {
            var one = defs?.FindMeshId(thing.DefinitionType) ?? enums?.FindMeshId(thing.DefinitionType);
            if (one is > 0)
                meshIds = [one.Value];
        }

        return new ThingSubmit(
            thing.DefinitionType, typeName, meshIds, asC3d, asC3d && meshIds.Count > 0);
    }

    public static GameBin? TryLoadDefs(GameInstall install)
    {
        var namesPath = install.FindCompiledDef("names.bin");
        var binPath = install.FindCompiledDef("game.bin");
        if (namesPath is null || binPath is null)
            return null;
        return GameBin.Load(binPath, NamesBin.Load(namesPath));
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

    private static Vector3 TransformUnitNormal(Vector3 normal, Matrix4x4 transform)
    {
        if (normal.LengthSquared() < 1e-8f)
            return Vector3.Zero;
        var n = Vector3.TransformNormal(normal, transform);
        return n.LengthSquared() < 1e-8f ? Vector3.Zero : Vector3.Normalize(n);
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

public readonly record struct ThingSubmit(
    string Definition,
    string? TypeName,
    IReadOnlyList<int> MeshIds,
    bool AsC3d,
    bool Submitted);

/// <summary>
/// <c>009AD410</c> handle + world transform.
/// C3D bytes stay in the bank until draw.
/// </summary>
public readonly record struct WorldMeshInstance(
    uint MeshId,
    Matrix4x4 Transform,
    string? ScriptName,
    string? Definition,
    string Map);
