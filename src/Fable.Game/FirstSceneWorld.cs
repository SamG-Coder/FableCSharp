using System.Globalization;
using System.Numerics;
using System.Text;
using Fable.Core;
using Fable.Formats;
using Fable.Formats.Banks;
using Fable.Formats.Defs;
using Fable.Formats.Levels;
using Fable.Formats.Meshes;
using Fable.Formats.Scene;
using Fable.Formats.Sky;
using Fable.Formats.Tng;
using Fable.Formats.World;

namespace Fable.Game;

/// <summary>
/// Reconstructed intro-view fixture:
/// <c>StartOakValeWest</c> / <c>CAM_OVIF_SHOT2</c> /
/// <c>ScriptRuntime.StartNewGame</c> /
/// <see cref="WorldGeometry.Build"/>.
/// Not <c>EngineLifecycle.Pump</c> (no-save Present
/// is LookoutPoint). Do not collapse leftover #4.
/// </summary>
public sealed class FirstSceneWorld
{
    public const string Region = RegionTravel.NewGameRegion;
    public const string CameraName = RegionTravel.IntroFirstSeenCamera;
    public const string HouseScript = "HerosOldHouse";
    public const int PathStoneyTextureId = 4130;
    public const float Aspect = 4f / 3f;

    public required GameInstall Install { get; init; }
    public required WorldGeometry Geometry { get; init; }
    public required ScriptedCamera Camera { get; init; }
    public required IReadOnlyList<ThingInstance> Things { get; init; }
    public required LevHeightField Height { get; init; }
    public required float MapX { get; init; }
    public required float MapY { get; init; }
    public required Vector3 House { get; init; }
    public required Vector3 Father { get; init; }
    public required Vector3 Kid { get; init; }
    public required Vector3 PathPoint { get; init; }
    public required Vector3 Fence { get; init; }
    public required Vector3 Terrain { get; init; }
    public required LevTileVertex TerrainFile { get; init; }
    public required ThingInstance HouseThing { get; init; }
    public required ThingInstance FenceThing { get; init; }
    public required ThingInstance FatherThing { get; init; }
    public required LandscapeFrustum.Plane[] Planes { get; init; }
    public required IReadOnlyList<WorldVisibilityRecord> Visibility { get; init; }

    public static FirstSceneWorld Build(GameInstall install)
    {
        using var levels = new LevelLibrary(install);
        var things = levels.LoadThings(Region).Things.ToList();
        var camera = new ScriptedCamera();
        if (!camera.UseCamera(things, CameraName))
            throw new InvalidOperationException($"missing {CameraName}");

        LandscapeFrustum.LetterboxCots(
            float.DegreesToRadians(camera.FovDegrees), Aspect, 1f,
            out var cotH, out var cotV);
        var planes = LandscapeFrustum.ExtractSidePlanes(
            camera.Position, camera.Forward, camera.Up, cotH, cotV);
        var runtime = ScriptRuntime.StartNewGame(install, things, camera);
        var geometry = WorldGeometry.Build(
            install, Region, things,
            landscapePlanes: planes,
            actorPositions: runtime.ActorPositions,
            actorPoses: runtime.Animation.PoseNames());

        var map = levels.World.FindMap(Region)
                  ?? throw new InvalidOperationException(Region);
        var height = levels.LoadHeightField(Region)
                     ?? throw new InvalidOperationException("STB");
        var houseThing = things.First(t => t.ScriptName == HouseScript);
        var house = PositionOf(houseThing);
        var fatherSource = things.First(t =>
            t.ScriptName is not null &&
            t.ScriptName.Equals(RegionTravel.LiveFatherScript, StringComparison.OrdinalIgnoreCase));
        var father = runtime.ActorPositions.TryGetValue(RegionTravel.IntroFatherActor, out var fp)
            ? fp
            : PositionOf(fatherSource);
        var fatherThing = ClonePlaced(fatherSource,
            fatherSource.DefinitionType ?? RegionTravel.LiveFatherCreature, father);
        var kid = runtime.ActorPositions.TryGetValue(RegionTravel.IntroHeroActor, out var kp)
            ? kp
            : RegionTravel.PositionOf(RegionTravel.FindPlayerStart(things)!);

        var pathTri = geometry.Triangles.First(t =>
            t.Layer == SceneLayer.Landscape &&
            t.TextureId == PathStoneyTextureId &&
            WorldSpaces.DistanceXy((t.A + t.B + t.C) / 3f, house) < 12f);
        var path = (pathTri.A + pathTri.B + pathTri.C) / 3f;

        var fenceThing = FindFence(things, house);
        var fence = PositionOf(fenceThing);

        var terrainFile = NearestStb(height, house);
        var terrain = WorldSpaces.StbFileToRegionLocal(terrainFile, map.MapX, map.MapY);

        return new FirstSceneWorld
        {
            Install = install,
            Geometry = geometry,
            Camera = camera,
            Things = things,
            Height = height,
            MapX = map.MapX,
            MapY = map.MapY,
            House = house,
            Father = father,
            Kid = kid,
            PathPoint = path,
            Fence = fence,
            Terrain = terrain,
            TerrainFile = terrainFile,
            HouseThing = houseThing,
            FenceThing = fenceThing,
            FatherThing = fatherThing,
            Planes = planes,
            Visibility = Classify(install, levels, things, geometry, planes, house),
        };
    }

    public IReadOnlyList<Vector3> SharedWorldPoints() =>
        [House, Father, Kid, PathPoint, Fence, Terrain];

    public (Matrix4x4 W, Matrix4x4 V, Matrix4x4 P) WorldViewProj()
    {
        LandscapeFrustum.LetterboxCots(
            float.DegreesToRadians(Camera.FovDegrees), Aspect, 1f,
            out var cotH, out var cotV);
        var view = LandscapeFrustum.CotScaledView(
            Camera.Position, Camera.Forward, Camera.Up, cotH, cotV);
        LandscapeFrustum.ViewportZTerms(
            LandscapeFrustum.FirstSeenNear, LandscapeFrustum.FirstSeenFar,
            LandscapeFrustum.FirstSeenMinZ, LandscapeFrustum.FirstSeenMaxZ,
            out var m33, out var m34);
        return (LandscapeFrustum.IdentityWorld(), view,
            LandscapeFrustum.FirstSeenDx9Projection(m33, m34));
    }

    public WorldPrimitiveTrace TraceLandscape()
    {
        var (w, v, p) = WorldViewProj();
        var file = new Vector3(TerrainFile.WorldX, TerrainFile.WorldY, TerrainFile.Z);
        var local = WorldSpaces.StbFileToRegionLocal(TerrainFile, MapX, MapY);
        var camRel = WorldSpaces.RegionLocalToCameraRelative(local, Camera.Position);
        var native = WorldSpaces.NativeLandscapeClip(file, MapX, MapY, Camera.Position, v, p);
        var host = WorldSpaces.HostLandscapeClip(file, MapX, MapY, v, p);
        var layer = ScenePasses.FirstSeenLayers.First(l => l.Bit == 0x4);
        return new WorldPrimitiveTrace(
            "A", "landscape", "FinalAlbion_RT.stb .lev", 0,
            $"u16 XY ({TerrainFile.WorldX},{TerrainFile.WorldY}) z={TerrainFile.Z}",
            file, "STB file WLD",
            WorldSpaces.WldToRegionLocal(file.X, file.Y, MapX, MapY),
            local, camRel,
            LandscapeFrustum.HostWorldSpaceLandscapeWorld(), v, p, host,
            WorldSpaces.ToNdc(host),
            layer, PathStoneyTextureId, "STB extra.yz / oT1=0",
            NativeClip: native);
    }

    public WorldPrimitiveTrace TraceHouse()
    {
        var (w, v, p) = WorldViewProj();
        var tri = Geometry.Triangles.First(t =>
            t.Layer == SceneLayer.Prop &&
            t.TextureId == GameBin.HerosOldHouseInteriorWallTexture &&
            WorldSpaces.DistanceXy((t.A + t.B + t.C) / 3f, House) < 20f);
        var world = (tri.A + tri.B + tri.C) / 3f;
        var clip = WorldSpaces.Clip(world, w, v, p);
        var layer = ScenePasses.FirstSeenLayers.First(l => l.Bit == 0x20);
        return new WorldPrimitiveTrace(
            "B", "static house", "graphics.big C3D 6909/6911",
            GameBin.HerosOldHouseInteriorMeshId,
            $"mid {world}", world, "C3D cm → ObjectTransform region-local",
            new Vector2(world.X, world.Y), world, default,
            w, v, p, clip, WorldSpaces.ToNdc(clip),
            layer, tri.TextureId, "C3D UV oT0=v2");
    }

    public WorldPrimitiveTrace TraceProp()
    {
        var (w, v, p) = WorldViewProj();
        var defs = WorldGeometry.TryLoadDefs(Install)
                   ?? throw new InvalidOperationException("game.bin");
        var resolved = WorldGeometry.ResolveSubmit(defs, null, FenceThing);
        var meshId = resolved.MeshIds.First();
        var mesh = LoadMesh(meshId);
        var local = mesh.Triangles[0].A;
        var xform = WorldGeometry.ObjectTransform(FenceThing);
        var world = Vector3.Transform(local, xform);
        var submitted = Geometry.Triangles.First(t =>
            t.Layer == SceneLayer.Prop &&
            (NearVert(t.A, world) || NearVert(t.B, world) || NearVert(t.C, world)));
        var clip = WorldSpaces.Clip(world, w, v, p);
        var layer = ScenePasses.FirstSeenLayers.First(l => l.Bit == 0x20);
        return new WorldPrimitiveTrace(
            "C", "static prop", "graphics.big C3D + TNG",
            meshId,
            $"def {FenceThing.DefinitionType} C3D {Fmt(local)} mesh {meshId}",
            local, "C3D cm → ObjectTransform (0.01, RHSetForward/Up)",
            new Vector2(world.X, world.Y), world, default,
            w, v, p, clip, WorldSpaces.ToNdc(clip),
            layer, submitted.TextureId, "C3D UV oT0=v2");
    }

    public WorldPrimitiveTrace TracePalskin()
    {
        var (w, v, p) = WorldViewProj();
        var defs = WorldGeometry.TryLoadDefs(Install)
                   ?? throw new InvalidOperationException("game.bin");
        var resolved = WorldGeometry.ResolveSubmit(defs, null, FatherThing);
        var meshId = resolved.MeshIds.First();
        var (mesh, sample) = LoadFatherMesh();
        var group = sample.GroupBones ?? [];
        var palettes = IdentityPalettes(64);
        var skinned = WorldShading.SkinPosition(
            sample.Position,
            [sample.Index0, sample.Index1, sample.Index2, sample.Index3],
            [sample.Weight0, sample.Weight1, sample.Weight2, sample.Weight3],
            palettes, group);
        var xform = WorldGeometry.ObjectTransform(FatherThing);
        var world = Vector3.Transform(skinned, xform);
        var submitted = Geometry.Triangles
            .Where(t => t.Layer == SceneLayer.Palskin)
            .OrderBy(t => WorldSpaces.DistanceXy((t.A + t.B + t.C) / 3f, world))
            .First();
        var clip = WorldSpaces.Clip(world, w, v, p);
        var layer = ScenePasses.FirstSeenLayers.First(l => l.Bit == 0x100);
        _ = mesh;
        return new WorldPrimitiveTrace(
            "D", "PALSKIN father", "graphics.big CREATURE_HERO_FATHER",
            meshId,
            $"file idx {sample.Index0},{sample.Index1},{sample.Index2} " +
            $"(register offsets) group[{(group.Length == 0 ? "-" : (sample.Index0 / 3).ToString())}] " +
            "ObjectTransform 0.01 RHSetForward/Up",
            skinned, "PALSKIN dest[group[a0/3]] → ObjectTransform",
            new Vector2(world.X, world.Y), world, default,
            w, v, p, clip, WorldSpaces.ToNdc(clip),
            layer, submitted.TextureId, "PALSKIN oT0=v4");
    }

    public WorldPrimitiveTrace TraceSky()
    {
        LandscapeFrustum.LetterboxCots(
            float.DegreesToRadians(Camera.FovDegrees), Aspect, 1f,
            out var cotH, out var cotV);
        var view = LandscapeFrustum.CotScaledView(
            Camera.Position, Camera.Forward, Camera.Up, cotH, cotV);
        LandscapeFrustum.ViewportZTerms(
            SkyPass.FirstSeenNear, SkyPass.FirstSeenFar,
            SkyPass.FirstSeenMinZ, SkyPass.FirstSeenMaxZ,
            out var m33, out var m34);
        var proj = LandscapeFrustum.FirstSeenDx9Projection(m33, m34);
        var world = SkyPass.EllipsoidPoint(0, 0);
        var clip = WorldSpaces.Clip(world, LandscapeFrustum.IdentityWorld(), view, proj);
        var layer = ScenePasses.FirstSeenLayers.First(l => l.Bit == 0x2000);
        return new WorldPrimitiveTrace(
            "E", "sky", "00B61DD0 ellipsoid", 0,
            $"zenith {world}", world, "sky local at origin",
            Vector2.Zero, world, default,
            LandscapeFrustum.IdentityWorld(), view, proj, clip,
            WorldSpaces.ToNdc(clip),
            layer, SkyDef.MiddaySkyTextureId, "dome UV (0,0); PS c0/c1/c2 UNREAD");
    }

    public IReadOnlyList<WorldPrimitiveTrace> AllTraces() =>
        [TraceLandscape(), TraceHouse(), TraceProp(), TracePalskin(), TraceSky()];

    public string FormatTrace(WorldPrimitiveTrace t)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"CATEGORY {t.Id} {t.Category}");
        sb.AppendLine("SOURCE");
        sb.AppendLine($"  file {t.File}");
        sb.AppendLine($"  object/mesh id {t.MeshId}");
        sb.AppendLine($"  source bytes {t.SourceBytes}");
        sb.AppendLine($"  decoded vertex {Fmt(t.Decoded)}");
        sb.AppendLine("SPACE");
        sb.AppendLine($"  source coordinate space {t.SourceSpace}");
        sb.AppendLine($"  region conversion {t.RegionXy}");
        sb.AppendLine($"  world coordinate {Fmt(t.World)}");
        sb.AppendLine($"  camera-relative {Fmt(t.CameraRelative)}");
        sb.AppendLine("TRANSFORM");
        sb.AppendLine($"  W {Fmt(t.WorldMatrix)}");
        sb.AppendLine($"  V {Fmt(t.View)}");
        sb.AppendLine($"  P {Fmt(t.Projection)}");
        sb.AppendLine($"  clip {Fmt(t.Clip)}");
        if (t.NativeClip is { } native)
            sb.AppendLine($"  native clip {Fmt(native)}");
        sb.AppendLine($"  NDC {Fmt(t.Ndc)}");
        sb.AppendLine("SUBMISSION");
        sb.AppendLine($"  layer 0x{t.Layer.Bit:X} {t.Layer.Contents}");
        sb.AppendLine($"  shader {t.Layer.VertexShader} / {t.Layer.PixelShader}");
        sb.AppendLine($"  material/texture {t.TextureId}");
        sb.AppendLine($"  UV {t.UvSource}");
        sb.AppendLine($"  depth {t.Layer.Depth}");
        sb.AppendLine($"  cull {t.Layer.Cull}");
        sb.AppendLine($"  blend {t.Layer.Blend}");
        return sb.ToString();
    }

    public string FormatLandscapeSubmit()
    {
        var sb = new StringBuilder();
        var tiles = Height.Tiles.Tiles;
        var primary = 0;
        var extras = 0;
        var extraFaces = 0;
        var gridTiles = 0;
        foreach (var tile in tiles)
        {
            if (tile.Vertices.Count == 289)
                gridTiles++;
            if (tile.Indices.Count >= 3)
                primary += Math.Max(0, tile.Indices.Count - 2);
            foreach (var extra in tile.Extras)
            {
                extras++;
                extraFaces += Math.Max(0, extra.Indices.Count - 2);
            }
        }

        var land = Geometry.Triangles.Count(t => t.Layer == SceneLayer.Landscape);
        sb.AppendLine($"region {Region} map=({MapX},{MapY})");
        sb.AppendLine($"tiles={tiles.Count} grid289={gridTiles}");
        sb.AppendLine($"primaryStripFaces={primary} edgeStrips={extras} edgeFaces={extraFaces}");
        sb.AppendLine($"submittedLandscapeTris={land}");
        sb.AppendLine($"IndexCount=PrimitiveCount+2 rewindNz={LandscapeStrip.FirstSeenRewindsNegativeNz}");
        sb.AppendLine($"invented1mFill=false");
        return sb.ToString();
    }

    public string FormatVisibility()
    {
        var sb = new StringBuilder();
        foreach (var r in Visibility)
            sb.AppendLine($"{r.Kind}\t{r.Name}\t{(r.Submitted ? "submit" : "reject")}\t{r.Reason}");
        return sb.ToString();
    }

    public void WriteTraces(string directory)
    {
        Directory.CreateDirectory(directory);
        foreach (var t in AllTraces())
            File.WriteAllText(Path.Combine(directory, $"world-trace-{t.Id}.txt"), FormatTrace(t));
    }

    private MeshFile LoadMesh(int meshId)
    {
        var path = Path.Combine(Install.DataRoot, "graphics", "graphics.big");
        using var big = BigArchive.Open(path);
        var bank = big.SubBanks.First(item => item.Name.Contains("MESH", StringComparison.OrdinalIgnoreCase));
        var entry = big.ReadEntries(bank).First(e => e.Id == (uint)meshId);
        return MeshFile.Parse(big.Read(entry), (int)entry.Type);
    }

    private (MeshFile Mesh, MeshPalskinSample Sample) LoadFatherMesh()
    {
        var defs = WorldGeometry.TryLoadDefs(Install)
                   ?? throw new InvalidOperationException("game.bin");
        var meshId = defs.FindMeshId(RegionTravel.LiveFatherCreature)
                     ?? throw new InvalidOperationException("father mesh");
        var mesh = LoadMesh(meshId);
        return (mesh, mesh.PalskinSamples[0]);
    }

    private static ThingInstance ClonePlaced(ThingInstance source, string definitionType, Vector3 position) =>
        new()
        {
            Kind = source.Kind,
            Section = source.Section,
            DefinitionType = definitionType,
            ScriptName = source.ScriptName,
            Uid = source.Uid,
            Player = source.Player,
            PositionX = position.X,
            PositionY = position.Y,
            PositionZ = position.Z,
            Properties = source.Properties,
        };

    private static bool NearVert(Vector3 a, Vector3 b) =>
        (a - b).LengthSquared() < 0.0025f;

    private static Matrix4x4[] IdentityPalettes(int count)
    {
        var palettes = new Matrix4x4[count];
        for (var i = 0; i < count; i++)
            palettes[i] = Matrix4x4.Identity;
        return palettes;
    }

    private static Vector3 PositionOf(ThingInstance thing) =>
        new(thing.PositionX!.Value, thing.PositionY!.Value, thing.PositionZ!.Value);

    internal static ThingInstance FindFence(IReadOnlyList<ThingInstance> things, Vector3 house)
    {
        static bool IsFenceName(string? name) =>
            name is not null &&
            (name.Contains("FENCE", StringComparison.OrdinalIgnoreCase) ||
             name.Contains("GATE", StringComparison.OrdinalIgnoreCase) ||
             name.Contains("WALL", StringComparison.OrdinalIgnoreCase) ||
             name.Contains("STREETLAMP", StringComparison.OrdinalIgnoreCase));

        var named = things
            .Where(t => t.PositionX is not null && IsFenceName(t.DefinitionType))
            .OrderBy(t => WorldSpaces.DistanceXy(PositionOf(t), house))
            .FirstOrDefault();
        if (named is not null)
            return named;
        return things.First(t =>
            t.DefinitionType == "OBJECT_BUILDING_DOOR_3" && t.PositionX is not null);
    }

    internal static LevTileVertex NearestStb(LevHeightField height, Vector3 house)
    {
        LevTileVertex best = default;
        var bestD = float.MaxValue;
        foreach (var tile in height.Tiles.Tiles)
        {
            foreach (var v in tile.Vertices)
            {
                var local = WorldSpaces.StbFileToRegionLocal(v, height.OriginX, height.OriginY);
                var d = WorldSpaces.DistanceXy(local, house);
                if (d < bestD)
                {
                    bestD = d;
                    best = v;
                }
            }
        }

        return best;
    }

    internal static IReadOnlyList<WorldVisibilityRecord> Classify(
        GameInstall install,
        LevelLibrary levels,
        IReadOnlyList<ThingInstance> things,
        WorldGeometry geometry,
        LandscapeFrustum.Plane[] planes,
        Vector3 house)
    {
        var records = new List<WorldVisibilityRecord>();
        foreach (var map in WorldGeometry.StaticMapsAround(levels.World, install, Region))
        {
            var reason = map.IsSea
                ? "reject IsSea"
                : "submit Contains/Sees or BWD AABB touch (OpenStaticMaps)";
            records.Add(new WorldVisibilityRecord(
                "map", map.ScriptName, !map.IsSea, reason));
        }

        var height = levels.LoadHeightField(Region)!;
        LandscapeFrustum.PatchAabb(0f, 0f, height.FineWidth, height.FineHeight, out var min, out var max);
        var outside = LandscapeFrustum.AabbIsOutside(min, max, planes);
        records.Add(new WorldVisibilityRecord(
            "landscape-cell", Region, !outside,
            outside ? "reject 00BDC2D0 AABB" : "submit 00BDC2D0 four-plane AABB"));

        records.Add(new WorldVisibilityRecord(
            "water", "0x20000", LandscapeTextures.FirstSeenWaterDrawShouldSubmit,
            LandscapeTextures.FirstSeenWaterDrawShouldSubmit
                ? "submit water"
                : "reject 00B783F0 empty-out"));
        records.Add(new WorldVisibilityRecord(
            "stars", "stars.dat", SkyPass.FirstSeenEmitsInventedStarBillboards,
            "reject 00B65A20 first dword==0"));
        records.Add(new WorldVisibilityRecord(
            "lod", "C3DMeshLODInfo", WorldShading.MeshLodInfoReady_00A23DE0(0) == 1,
            "ready-or-not, not a mesh swap"));
        records.Add(new WorldVisibilityRecord(
            "house-interior", "6911",
            !GameBin.FirstSeenHouseSkipDropsInterior &&
            GameBin.FirstSeenMultiStaticAppliesBothHouseMeshes,
            "CMultiStatic 6911 then 6909; InsideBuilding=false"));
        records.Add(new WorldVisibilityRecord(
            "house-exterior", "6909",
            !GameBin.FirstSeenHouseSkipDropsExterior,
            "Graphic 6909"));
        records.Add(new WorldVisibilityRecord(
            "house-floor-3184", "3184",
            GameBin.FirstSeenHouseFloor3184HasPrims,
            "material exists, no prims — not replaced"));

        var defs = WorldGeometry.TryLoadDefs(install);
        var headerPath = Path.Combine(install.DataRoot, "Defs", "RetailHeaders", "meshdata.h");
        var enums = File.Exists(headerPath) ? HeaderEnums.Load(headerPath) : null;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        void AddThing(ThingInstance thing)
        {
            var key = $"{thing.DefinitionType}:{thing.Uid}";
            if (!seen.Add(key))
                return;
            var resolved = WorldGeometry.ResolveSubmit(defs, enums, thing);
            var submittedInWorld = resolved.Submitted &&
                geometry.Triangles.Any(t =>
                    t.Layer is SceneLayer.Prop or SceneLayer.Palskin &&
                    WorldSpaces.DistanceXy((t.A + t.B + t.C) / 3f, PositionOf(thing)) < 8f);
            var submitted = resolved.Submitted && submittedInWorld;
            var reason = submitted
                ? $"submit Graphic type={resolved.TypeName} meshes={string.Join(",", resolved.MeshIds)}"
                : resolved.AsC3d
                    ? "reject no Graphic/CMultiStatic"
                    : $"reject TypeName={resolved.TypeName ?? "?"} FirstSeenInstancesAsC3d";
            records.Add(new WorldVisibilityRecord(
                "object", thing.DefinitionType ?? thing.Kind, submitted, reason));
        }

        foreach (var thing in things)
        {
            var def = thing.DefinitionType ?? "";
            var watch = def.Contains("GAZE", StringComparison.OrdinalIgnoreCase) ||
                        def.Contains("HOLY_SITE", StringComparison.OrdinalIgnoreCase);
            var near = thing.PositionX is not null &&
                       WorldSpaces.DistanceXy(PositionOf(thing), house) < 25f;
            if (near || watch)
                AddThing(thing);
        }

        return records;
    }

    private static string Fmt(Vector3 v) =>
        string.Create(CultureInfo.InvariantCulture, $"{v.X:0.######} {v.Y:0.######} {v.Z:0.######}");

    private static string Fmt(Vector4 v) =>
        string.Create(CultureInfo.InvariantCulture, $"{v.X:0.######} {v.Y:0.######} {v.Z:0.######} {v.W:0.######}");

    private static string Fmt(Vector2 v) =>
        string.Create(CultureInfo.InvariantCulture, $"{v.X:0.######} {v.Y:0.######}");

    private static string Fmt(Matrix4x4 m) =>
        string.Create(CultureInfo.InvariantCulture,
            $"{m.M11:0.###},{m.M12:0.###},{m.M13:0.###},{m.M14:0.###}; " +
            $"{m.M21:0.###},{m.M22:0.###},{m.M23:0.###},{m.M24:0.###}; " +
            $"{m.M31:0.###},{m.M32:0.###},{m.M33:0.###},{m.M34:0.###}; " +
            $"{m.M41:0.###},{m.M42:0.###},{m.M43:0.###},{m.M44:0.###}");
}

public readonly record struct WorldPrimitiveTrace(
    string Id,
    string Category,
    string File,
    int MeshId,
    string SourceBytes,
    Vector3 Decoded,
    string SourceSpace,
    Vector2 RegionXy,
    Vector3 World,
    Vector3 CameraRelative,
    Matrix4x4 WorldMatrix,
    Matrix4x4 View,
    Matrix4x4 Projection,
    Vector4 Clip,
    Vector4 Ndc,
    FirstSeenLayerContract Layer,
    int TextureId,
    string UvSource,
    Vector4? NativeClip = null);

public readonly record struct WorldVisibilityRecord(
    string Kind,
    string Name,
    bool Submitted,
    string Reason);
