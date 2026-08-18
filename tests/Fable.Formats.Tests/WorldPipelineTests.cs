using System.Numerics;
using Fable.Core;
using Fable.Formats;
using Fable.Formats.Defs;
using Fable.Formats.Levels;
using Fable.Formats.Meshes;
using Fable.Formats.Scene;
using Fable.Formats.Sky;
using Fable.Formats.World;
using Fable.Game;
using Fable.Render;
using Fable.Render.Parity.Dx9Vulkan;

namespace Fable.Formats.Tests;

/// <summary>
/// First-scene world pipeline: file bytes → shared region-local
/// space → visibility → W → V → P. Drives
/// <see cref="FirstSceneWorld.Build"/> (the same
/// StartOakValeWest / SHOT2 / WorldGeometry path as the client).
/// </summary>
public sealed class WorldPipelineTests
{
    private static FirstSceneWorld Load()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        return FirstSceneWorld.Build(install);
    }

    [Fact]
    public void World_space_catalog_names_every_first_scene_space()
    {
        var names = WorldSpaces.Catalog().Select(s => s.Name).ToList();
        foreach (var required in new[]
                 {
                     "graphics.big C3D local",
                     "C3D units / centimetres",
                     "TNG object local transform",
                     "region-local coordinates",
                     "WLD/global map coordinates",
                     "STB file coordinates",
                     "expanded Fable landscape VB coordinates",
                     "camera-relative landscape coordinates",
                     "camera/world coordinates",
                     "static-object world coordinates",
                     "skinned-character coordinates",
                     "view space",
                     "clip space",
                     "Vulkan NDC",
                 })
            Assert.Contains(required, names);

        Assert.All(WorldSpaces.Catalog(), s =>
        {
            Assert.False(string.IsNullOrWhiteSpace(s.Units));
            Assert.False(string.IsNullOrWhiteSpace(s.Handedness));
            Assert.False(string.IsNullOrWhiteSpace(s.Axes));
            Assert.False(string.IsNullOrWhiteSpace(s.Origin));
            Assert.False(string.IsNullOrWhiteSpace(s.AbsoluteOrRelative));
            Assert.False(string.IsNullOrWhiteSpace(s.NextConversion));
            Assert.False(string.IsNullOrWhiteSpace(s.Evidence));
        });
        Assert.Contains(WorldSpaces.Catalog(), s => s.Status == WorldSpaceStatus.Proven);
        Assert.True(LandscapeFrustum.HostTcamOnWorldSpaceLandscapeIsDisproven);
        Assert.True(LandscapeFrustum.FirstSeenLandscapeFileVertsAreWorldSpace);
        Assert.True(LandscapeFrustum.FirstSeenLandscapeDeviceVbIsCameraRelative);
    }

    [Fact]
    public void House_father_kid_path_fence_terrain_share_region_local_world()
    {
        var scene = Load();
        Assert.Equal(3456f, scene.MapX);
        Assert.Equal(736f, scene.MapY);
        Assert.Equal(RegionTravel.NewGameRegion, scene.Geometry.Region);
        Assert.Equal(RegionTravel.IntroFirstSeenCamera, scene.Camera.ActiveName);

        var pts = scene.SharedWorldPoints();
        Assert.Equal(6, pts.Count);
        foreach (var p in pts)
        {
            Assert.InRange(p.X, -20f, 160f);
            Assert.InRange(p.Y, 80f, 200f);
            Assert.True(p.Z is > 0f and < 80f, $"z={p.Z}");
        }

        Assert.True(WorldSpaces.DistanceXy(scene.House, scene.Terrain) < 8f,
            $"house {scene.House} terrain {scene.Terrain}");
        Assert.True(WorldSpaces.DistanceXy(scene.House, scene.PathPoint) < 12f,
            $"house {scene.House} path {scene.PathPoint}");
        Assert.True(WorldSpaces.DistanceXy(scene.House, scene.Kid) < 25f,
            $"house {scene.House} kid {scene.Kid}");
        Assert.True(WorldSpaces.DistanceXy(scene.House, scene.Father) < 25f,
            $"house {scene.House} father {scene.Father}");
        Assert.True(WorldSpaces.DistanceXy(scene.House, scene.Fence) < 40f,
            $"house {scene.House} fence {scene.Fence}");

        var wld = WorldSpaces.RegionLocalToWld(scene.House.X, scene.House.Y, scene.MapX, scene.MapY);
        Assert.Equal(scene.House.X + scene.MapX, wld.X, 3);
        Assert.Equal(scene.House.Y + scene.MapY, wld.Y, 3);
        var back = WorldSpaces.WldToRegionLocal(wld.X, wld.Y, scene.MapX, scene.MapY);
        Assert.Equal(scene.House.X, back.X, 3);
        Assert.Equal(scene.House.Y, back.Y, 3);

        var file = new Vector3(scene.TerrainFile.WorldX, scene.TerrainFile.WorldY, scene.TerrainFile.Z);
        Assert.True(file.X > 3000f, $"STB file XY must stay WLD, got {file}");
        var local = WorldSpaces.StbFileToRegionLocal(scene.TerrainFile, scene.MapX, scene.MapY);
        Assert.Equal(scene.Terrain, local);
        Assert.True(MathF.Abs(file.X - scene.MapX - local.X) < 0.01f);
    }

    [Fact]
    public void Native_cam_relative_Tcam_clip_equals_host_identity_W()
    {
        var scene = Load();
        var (_, view, proj) = scene.WorldViewProj();
        var samples = new List<LevTileVertex> { scene.TerrainFile };
        foreach (var tile in scene.Height.Tiles.Tiles.Take(4))
        {
            if (tile.Vertices.Count > 0)
                samples.Add(tile.Vertices[0]);
            if (tile.Vertices.Count > 8)
                samples.Add(tile.Vertices[8]);
        }

        Assert.True(samples.Count >= 5);
        foreach (var v in samples)
        {
            var file = new Vector3(v.WorldX, v.WorldY, v.Z);
            var native = WorldSpaces.NativeLandscapeClip(
                file, scene.MapX, scene.MapY, scene.Camera.Position, view, proj);
            var host = WorldSpaces.HostLandscapeClip(file, scene.MapX, scene.MapY, view, proj);
            Assert.True(
                WorldSpaces.NearlyEqual(native, host, 1e-3f),
                $"diverge file={file} native={native} host={host}");

            var tcamOnWorld = WorldSpaces.Clip(
                WorldSpaces.StbFileToRegionLocal(v, scene.MapX, scene.MapY),
                LandscapeFrustum.LandscapeWorld(scene.Camera.Position), view, proj);
            Assert.False(
                WorldSpaces.NearlyEqual(tcamOnWorld, host, 0.05f),
                "T(cam) on world-space STB must stay DISPROVEN");
        }

        var hostVp = scene.Camera.HostLandscapeViewProjection(FirstSceneWorld.Aspect);
        var nativeVp = scene.Camera.LandscapeViewProjection(FirstSceneWorld.Aspect);
        var local = scene.Terrain;
        var camRel = WorldSpaces.RegionLocalToCameraRelative(local, scene.Camera.Position);
        var hostClip = Vector4.Transform(new Vector4(local, 1f), hostVp);
        var nativeClip = Vector4.Transform(new Vector4(camRel, 1f), nativeVp);
        Assert.True(WorldSpaces.NearlyEqual(hostClip, nativeClip, 1e-3f),
            $"camera WVP diverge host={hostClip} native={nativeClip}");
    }

    [Fact]
    public void First_seen_landscape_submits_primary_and_edge_strips()
    {
        var scene = Load();
        var tiles = scene.Height.Tiles.Tiles;
        Assert.True(tiles.Count > 1, $"tiles={tiles.Count}");
        var withStrip = 0;
        var withExtras = 0;
        var extraFaces = 0;
        foreach (var tile in tiles)
        {
            if (tile.Indices.Count >= 3)
            {
                withStrip++;
                Assert.True(tile.Indices.Count >= 3);
            }

            if (tile.Extras.Count > 0)
            {
                withExtras++;
                foreach (var extra in tile.Extras)
                {
                    Assert.True(extra.Indices.Count >= 3, "edge strip empty");
                    extraFaces += Math.Max(0, extra.Indices.Count - 2);
                }
            }
        }

        Assert.True(withStrip > 0, "no primary strips");
        Assert.True(withExtras > 0, "no edge strips");
        Assert.True(extraFaces > 0);

        var land = scene.Geometry.Triangles.Count(t => t.Layer == SceneLayer.Landscape);
        Assert.True(land > 1000, $"land={land}");
        Assert.DoesNotContain(scene.Geometry.Triangles, t => t.TextureId == LandscapeTextures.WaterId);

        var compiled = new LevelLibrary(scene.Install).LoadCompiledLev(FirstSceneWorld.Region);
        Assert.NotNull(compiled);
        var cells = LevCellGrid.TryParse(compiled);
        Assert.NotNull(cells);
        var enums = HeaderEnums.Load(
            Path.Combine(scene.Install.DataRoot, "Defs", "RetailHeaders", "pc", "textures.h"));
        var drawn = scene.Height.ToTileTriangles(cells, compiled.Materials, enums);
        var viaTiles = scene.Height.Tiles.ToTriangles(
            scene.Height.OriginX, scene.Height.OriginY, cells, compiled.Materials, enums);
        Assert.Equal(viaTiles.Count, drawn.Count);
    }

    [Fact]
    public void Landscape_strip_unwind_does_not_rewind_on_negative_nz()
    {
        Assert.False(LandscapeStrip.FirstSeenRewindsNegativeNz);
        Assert.Equal(5, LandscapeStrip.IndexCountFromPrimitiveCount(3, true));
        Assert.Equal(0, LandscapeStrip.IndexCountFromPrimitiveCount(3, false));

        var even = LandscapeStrip.Unwind(0, 0, 1, 2);
        Assert.Equal((0, 1, 2), even);
        var odd = LandscapeStrip.Unwind(1, 0, 1, 2);
        Assert.Equal((1, 0, 2), odd);

        var a = new Vector3(0f, 0f, 0f);
        var b = new Vector3(1f, 0f, 0f);
        var c = new Vector3(0f, 1f, 0f);
        var n = LandscapeStrip.FaceNormal(a, c, b);
        Assert.True(n.Z < 0f, "this winding points down");
        var submitted = LandscapeStrip.SubmitWinding(0, a, c, b);
        Assert.Equal(a, submitted.A);
        Assert.Equal(c, submitted.B);
        Assert.Equal(b, submitted.C);
        Assert.True(LandscapeStrip.FaceNormal(submitted.A, submitted.B, submitted.C).Z < 0f);

        var src = File.ReadAllText(Path.Combine(
            FindRepoRoot(), "src", "Fable.Formats", "Levels", "LevTileMesh.cs"));
        Assert.DoesNotContain("n.Z < 0", src);
        Assert.Contains("LandscapeStrip.Unwind", src);
        Assert.Contains("FirstSeenRewindsNegativeNz", src);
    }

    [Fact]
    public void Static_tng_object_transform_places_local_origin_at_thing()
    {
        var scene = Load();
        var xform = WorldGeometry.ObjectTransform(scene.HouseThing);
        var origin = Vector3.Transform(Vector3.Zero, xform);
        Assert.Equal(scene.House.X, origin.X, 3);
        Assert.Equal(scene.House.Y, origin.Y, 3);
        Assert.Equal(scene.House.Z, origin.Z, 3);

        var cm = new Vector3(100f, 0f, 0f);
        var metres = WorldSpaces.C3dLocalToMetres(cm);
        Assert.Equal(1f, metres.X, 4);
        var world = Vector3.Transform(cm, xform);
        Assert.True((world - scene.House).Length() > 0.05f,
            $"100 cm along C3D X must leave the TNG origin; world={world} house={scene.House}");

        var door = scene.Things
            .Where(t => t.DefinitionType == "OBJECT_BUILDING_DOOR_3" && t.PositionX is not null)
            .OrderBy(t => WorldSpaces.DistanceXy(
                new Vector3(t.PositionX!.Value, t.PositionY!.Value, t.PositionZ!.Value), scene.House))
            .First();
        var doorX = WorldGeometry.ObjectTransform(door);
        var doorP = Vector3.Transform(Vector3.Zero, doorX);
        Assert.True(WorldSpaces.DistanceXy(doorP, scene.House) < 15f, $"door {doorP} house {scene.House}");

        var table = scene.Things
            .Where(t => t.DefinitionType == "OBJECT_TABLE_LARGE_ROUND_01" && t.PositionX is not null)
            .OrderBy(t => WorldSpaces.DistanceXy(
                new Vector3(t.PositionX!.Value, t.PositionY!.Value, t.PositionZ!.Value), scene.House))
            .First();
        var tableP = Vector3.Transform(Vector3.Zero, WorldGeometry.ObjectTransform(table));
        Assert.True(WorldSpaces.DistanceXy(tableP, scene.House) < 12f, $"table {tableP} house {scene.House}");
    }

    [Fact]
    public void Camera_world_view_clip_ndc_on_real_first_scene_points()
    {
        var scene = Load();
        var (w, v, p) = scene.WorldViewProj();
        Assert.Equal(Matrix4x4.Identity, w);
        Assert.True(LandscapeFrustum.FirstSeenViewLookIsZ);
        Assert.True(LandscapeFrustum.FirstSeenProjWIsViewZ);
        Assert.Equal(1f, LandscapeFrustum.Dx9ProjectionYSign);

        var look = scene.Camera.LookAt;
        var lookView = Vector3.Transform(look, v);
        Assert.True(lookView.Z > 0f, $"look view.z={lookView.Z}");
        var lookClip = WorldSpaces.Clip(look, w, v, p);
        Assert.Equal(lookView.Z, lookClip.W, 3);
        var lookNdc = WorldSpaces.ToNdc(lookClip);
        Assert.InRange(lookNdc.X, -0.25f, 0.25f);
        Assert.True(lookNdc.W > 0f);

        foreach (var (name, pt) in new[]
                 {
                     ("father", scene.Father),
                     ("house", scene.House),
                     ("terrain", scene.Terrain),
                     ("path", scene.PathPoint),
                 })
        {
            var clip = WorldSpaces.Clip(pt, w, v, p);
            var ndc = WorldSpaces.ToNdc(clip);
            Assert.True(float.IsFinite(ndc.X) && float.IsFinite(ndc.Y), $"{name} {ndc}");
            Assert.True(clip.W != 0f, $"{name} w=0");
        }

        var sky = SkyPass.EllipsoidPoint(0, 0);
        LandscapeFrustum.ViewportZTerms(
            SkyPass.FirstSeenNear, SkyPass.FirstSeenFar,
            SkyPass.FirstSeenMinZ, SkyPass.FirstSeenMaxZ,
            out var m33, out var m34);
        var skyP = LandscapeFrustum.FirstSeenDx9Projection(m33, m34);
        var skyClip = WorldSpaces.Clip(sky, LandscapeFrustum.IdentityWorld(), v, skyP);
        Assert.True(skyClip.W != 0f);
        Assert.True(sky.Z > 3000f);

        var vulkan = Dx9VulkanProjection.ToVulkanWvp(LandscapeFrustum.ComposeWvp(w, v, p));
        var dx9Y = WorldSpaces.ToNdc(WorldSpaces.Clip(look, w, v, p)).Y;
        var vkY = WorldSpaces.ToNdc(Vector4.Transform(new Vector4(look, 1f), vulkan)).Y;
        Assert.Equal(-dx9Y, vkY, 3);
    }

    [Fact]
    public void Visibility_and_layers_drive_shipped_first_scene_lists()
    {
        var scene = Load();
        Assert.Contains(scene.Visibility, r => r.Kind == "map" && r.Name == RegionTravel.NewGameRegion && r.Submitted);
        Assert.Contains(scene.Visibility, r => r.Kind == "landscape-cell" && r.Submitted);
        Assert.Contains(scene.Visibility, r =>
            r.Kind == "water" && !r.Submitted && r.Reason.Contains("empty-out"));
        Assert.Contains(scene.Visibility, r => r.Kind == "stars" && !r.Submitted);
        Assert.Contains(scene.Visibility, r => r.Kind == "house-interior" && r.Submitted);
        Assert.Contains(scene.Visibility, r => r.Kind == "house-exterior" && r.Submitted);
        Assert.Contains(scene.Visibility, r =>
            r.Kind == "house-floor-3184" && !r.Submitted);
        Assert.Contains(scene.Visibility, r =>
            r.Kind == "object" && !r.Submitted && r.Reason.Contains("FirstSeenInstancesAsC3d"));
        Assert.Contains(scene.Visibility, r =>
            r.Name.Contains("HOLY_SITE", StringComparison.OrdinalIgnoreCase) && !r.Submitted);
        Assert.DoesNotContain(scene.Visibility, r =>
            r.Name.Contains("GAZE", StringComparison.OrdinalIgnoreCase) && r.Submitted);
        Assert.DoesNotContain(scene.Visibility, r =>
            r.Name.Contains("HOLY_SITE", StringComparison.OrdinalIgnoreCase) && r.Submitted);
        var defs = WorldGeometry.TryLoadDefs(scene.Install);
        Assert.NotNull(defs);
        var gaze = scene.Things.FirstOrDefault(t =>
                       t.DefinitionType is not null &&
                       t.DefinitionType.Contains("GAZE", StringComparison.OrdinalIgnoreCase))
                   ?? new Fable.Formats.Tng.ThingInstance
                   {
                       Kind = "MARKER",
                       Section = "NULL",
                       DefinitionType = "GAZE_OUT_OF_BUILDING_MARKER",
                       PositionX = 0f,
                       PositionY = 0f,
                       PositionZ = 0f,
                       Properties = new Dictionary<string, string>(),
                   };
        var holy = scene.Things.First(t => t.DefinitionType == "HOLY_SITE_PLAYER_START");
        var gazeSubmit = WorldGeometry.ResolveSubmit(defs, null, gaze);
        var holySubmit = WorldGeometry.ResolveSubmit(defs, null, holy);
        Assert.False(gazeSubmit.Submitted);
        Assert.Equal("MARKER", gazeSubmit.TypeName);
        Assert.False(holySubmit.Submitted);
        Assert.Equal("HOLY_SITE", holySubmit.TypeName);
        Assert.False(holySubmit.AsC3d);
        Assert.False(GameBin.FirstSeenInstancesAsC3d("HOLY_SITE", "HOLY_SITE_PLAYER_START"));
        Assert.False(GameBin.FirstSeenInstancesAsC3d("MARKER", "GAZE_OUT_OF_BUILDING_MARKER"));

        Assert.Equal(34, ScenePasses.Registration.Length);
        Assert.Equal(7, ScenePasses.FirstSeenLayers.Count);
        Assert.Equal(0x4u, ScenePasses.FirstSeenLayers[0].Bit);
        Assert.Equal(0x40u, ScenePasses.FirstSeenLayers[1].Bit);
        Assert.Equal(0x20u, ScenePasses.FirstSeenLayers[2].Bit);
        Assert.Equal(0x100u, ScenePasses.FirstSeenLayers[3].Bit);
        Assert.Equal(0x2000u, ScenePasses.FirstSeenLayers[4].Bit);
        Assert.Equal(0x80u, ScenePasses.FirstSeenLayers[5].Bit);
        Assert.Equal(0x200u, ScenePasses.FirstSeenLayers[6].Bit);

        var mesh = MeshBatches.Build(scene.Geometry.Triangles);
        Assert.Contains(mesh.Draws, d => d.PassBit == 0x4);
        Assert.Contains(mesh.Draws, d => d.PassBit == 0x40);
        Assert.Contains(mesh.Draws, d => d.PassBit == 0x20);
        Assert.Contains(mesh.Draws, d => d.PassBit == 0x2000);
        Assert.DoesNotContain(mesh.Draws, d => d.PassBit == 0x20000);
        var ranks = mesh.Draws.Select(d => ScenePasses.Rank(d.PassBit)).ToList();
        Assert.Equal(ranks.OrderBy(r => r), ranks);

        var hx = scene.House.X;
        var hy = scene.House.Y;
        Assert.Contains(scene.Geometry.Triangles, t =>
            t.Layer == SceneLayer.Prop &&
            t.TextureId == GameBin.HerosOldHouseInteriorWallTexture &&
            WorldSpaces.DistanceXy((t.A + t.B + t.C) / 3f, scene.House) < 20f);
        Assert.DoesNotContain(scene.Geometry.Triangles, t =>
            t.TextureId == GameBin.HerosOldHouseFloorTexture &&
            WorldSpaces.DistanceXy((t.A + t.B + t.C) / 3f, scene.House) < 20f);
        Assert.False(GameBin.FirstSeenHouseFloor3184HasPrims);
        Assert.Equal(6909, GameBin.HerosOldHouseExteriorMeshId);
        Assert.Equal(6911, GameBin.HerosOldHouseInteriorMeshId);
        _ = hx;
        _ = hy;
    }

    [Fact]
    public void Sky_and_materials_keep_unread_constants_unread()
    {
        var scene = Load();
        Assert.Equal(100f, SkyPass.FirstSeenNear);
        Assert.Equal(10000f, SkyPass.FirstSeenFar);
        Assert.Equal(0.99f, SkyPass.FirstSeenMinZ);
        Assert.Equal(1f, SkyPass.FirstSeenMaxZ);
        Assert.Equal(0x2000u, SkyPass.FirstSeenLayerBit);
        Assert.False(SkyPass.FirstSeenUses400000);
        Assert.False(SkyPass.FirstSeenEmitsInventedStarBillboards);
        Assert.Contains(scene.Geometry.Triangles, t =>
            t.Layer == SceneLayer.Sky && t.TextureId == SkyDef.MiddaySkyTextureId);

        Assert.False(WorldShading.FirstSeenBindsC3dBump);
        Assert.Equal("PSHADER_TEXTURE_DIFFUSE", WorldShading.FirstSeenStaticPsName);
        Assert.Equal(1f, Dx9VulkanSamplerState.MaxLod);
    }

    [Fact]
    public void End_to_end_traces_A_through_E_are_deterministic()
    {
        var scene = Load();
        var a = scene.AllTraces();
        var b = scene.AllTraces();
        Assert.Equal(5, a.Count);
        Assert.Equal(new[] { "A", "B", "C", "D", "E" }, a.Select(t => t.Id));
        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(scene.FormatTrace(a[i]), scene.FormatTrace(b[i]));
            Assert.False(string.IsNullOrWhiteSpace(a[i].File));
            Assert.False(string.IsNullOrWhiteSpace(a[i].SourceSpace));
            Assert.True(a[i].Clip.W != 0f || a[i].Id == "E", $"clip {a[i].Id}");
        }

        Assert.Equal(0x4u, a[0].Layer.Bit);
        Assert.Equal(0x20u, a[1].Layer.Bit);
        Assert.Equal(0x20u, a[2].Layer.Bit);
        Assert.Equal(0x20u, a[3].Layer.Bit);
        Assert.Equal(0x2000u, a[4].Layer.Bit);
        Assert.Contains("UNREAD", a[4].Layer.PixelShader);
        Assert.Contains("register offsets", a[3].SourceBytes);
        Assert.Contains("ObjectTransform", a[2].SourceSpace);
        Assert.Contains("ObjectTransform", a[3].SourceSpace);
        Assert.True(a[2].MeshId > 0, $"trace C mesh={a[2].MeshId}");
        Assert.True(a[2].TextureId > 0, $"trace C tex={a[2].TextureId}");
        Assert.True(a[3].MeshId > 0, $"trace D mesh={a[3].MeshId}");
        Assert.True(a[3].TextureId > 0, $"trace D tex={a[3].TextureId}");
        var propXform = WorldGeometry.ObjectTransform(scene.FenceThing);
        Assert.True(
            (Vector3.Transform(a[2].Decoded, propXform) - a[2].World).Length() < 1e-3f,
            "trace C world must be ObjectTransform(C3D)");
        Assert.Contains(scene.Geometry.Triangles, t =>
            t.Layer == SceneLayer.Prop && t.TextureId == a[2].TextureId &&
            WorldSpaces.DistanceXy((t.A + t.B + t.C) / 3f, a[2].World) < 4f);
        var fatherXform = WorldGeometry.ObjectTransform(scene.FatherThing);
        Assert.True(
            (Vector3.Transform(a[3].Decoded, fatherXform) - a[3].World).Length() < 1e-3f,
            "trace D world must be ObjectTransform(skinned), not Father+metres");
        var invented = scene.Father + WorldSpaces.C3dLocalToMetres(a[3].Decoded);
        Assert.True(
            (invented - a[3].World).Length() > 1e-4f ||
            (Vector3.Transform(a[3].Decoded, fatherXform) - invented).Length() < 1e-4f,
            "ObjectTransform is the client W path");
        Assert.Contains(scene.Geometry.Triangles, t =>
            t.Layer == SceneLayer.Prop &&
            WorldSpaces.DistanceXy((t.A + t.B + t.C) / 3f, a[3].World) < 8f);
        Assert.True(a[0].NativeClip is { } native &&
                    WorldSpaces.NearlyEqual(native, a[0].Clip, 1e-3f));

        var dest = Path.Combine(FindRepoRoot(), "docs", "render", "traces");
        scene.WriteTraces(dest);
        File.WriteAllText(Path.Combine(dest, "landscape-submit.txt"), scene.FormatLandscapeSubmit());
        File.WriteAllText(Path.Combine(dest, "visibility-layers.txt"), scene.FormatVisibility());
        foreach (var id in new[] { "A", "B", "C", "D", "E" })
            Assert.True(File.Exists(Path.Combine(dest, $"world-trace-{id}.txt")));
    }

    [Fact]
    public void Locked_palskin_and_projection_findings_are_not_reverted()
    {
        Assert.Equal(20, WorldShading.FatherPalskinStrideBytes);
        Assert.Equal(4u, WorldShading.FatherPalskinInitFlags);
        Assert.Equal(3, WorldShading.PalskinGpuAddressOffset(1));
        Assert.True(LandscapeFrustum.FirstSeenProjWIsViewZ);
        Assert.Equal(-1f, Dx9VulkanProjection.NdcYSign);
        Assert.Equal(1f, LandscapeFrustum.Dx9ProjectionYSign);
        Assert.True(LandscapeFrustum.HostTcamOnWorldSpaceLandscapeIsDisproven);
        Assert.False(WorldShading.FirstSeenLodInfoSwapsMesh);
        Assert.False(WorldShading.FirstSeenPlaysAnim);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "src", "Fable.Formats", "Levels", "LevTileMesh.cs")))
                return dir.FullName;
            dir = dir.Parent;
        }

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }
}
