using System.Numerics;
using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Defs;
using Fable.Formats.Levels;
using Fable.Formats.World;
using Fable.Game;

namespace Fable.Formats.Tests;

/// <summary>
/// Living notes for the compiled .lev landscape format. Each fact is asserted
/// against multiple TLC regions so a bad guess fails loudly.
/// </summary>
public sealed class LevFormatTests
{
    public static TheoryData<string, int, int> Regions
    {
        get
        {
            var data = new TheoryData<string, int, int>
            {
                { "LookoutPoint", 128, 128 },
                { "PicnicArea", 128, 96 },
                { "DemonDoor_Guild", 64, 64 },
                { "OakValeEast_v2", 96, 160 },
            };
            return data;
        }
    }

    private static (GameInstall Install, byte[] Bytes) Load(string region)
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var wad = BbbArchive.Open(install.WadPath);
        var entry = wad.Find(region + ".lev");
        Assert.NotNull(entry);
        return (install, wad.Read(entry));
    }

    [Theory]
    [MemberData(nameof(Regions))]
    public void Header_is_version_25_with_format_constant_and_16_16_grid(string region, int width, int height)
    {
        var (_, bytes) = Load(region);
        Assert.Equal(LevFile.Version, BitConverter.ToInt32(bytes, 0));
        Assert.Equal(LevFile.FormatConstant, BitConverter.ToUInt32(bytes, 4));
        Assert.Equal((uint)width << 16, BitConverter.ToUInt32(bytes, 36));
        Assert.Equal((uint)height << 16, BitConverter.ToUInt32(bytes, 40));
        Assert.Equal(65536u, BitConverter.ToUInt32(bytes, 44)); // 1.0 in 16.16
    }

    [Theory]
    [MemberData(nameof(Regions))]
    public void Material_table_is_255_slots_of_132_bytes_starting_at_179(string region, int width, int height)
    {
        _ = (width, height);
        var (_, bytes) = Load(region);
        Assert.True(bytes.Length > LevFile.MaterialTableEnd);
        Assert.Equal("INVALID_THEME_STANDIN", ReadZ(bytes, 179));
        Assert.Equal(179 + 132, 311);
        Assert.StartsWith("GROUND_", ReadZ(bytes, 311));
        Assert.Equal(33839, LevFile.MaterialTableEnd);
    }

    [Theory]
    [MemberData(nameof(Regions))]
    public void Parser_reads_grid_ground_materials_and_sound_themes(string region, int width, int height)
    {
        var (_, bytes) = Load(region);
        var lev = LevFile.Parse(bytes);
        Assert.Equal(width, lev.GridWidth);
        Assert.Equal(height, lev.GridHeight);
        Assert.Equal(1f, lev.CellSize);
        Assert.Contains(lev.Materials, m => m.Name.StartsWith("GROUND_", StringComparison.Ordinal));
        Assert.Equal("INVALID_THEME_STANDIN", lev.Materials[0].Name);
        Assert.Contains(lev.SoundThemes, t => t.StartsWith("SOUND_THEME_", StringComparison.Ordinal));
        Assert.True(lev.PayloadOffset > LevFile.SecondaryTableEnd);
        Assert.True(lev.PayloadOffset < bytes.Length);
    }

    [Fact]
    public void Lookout_point_grid_covers_tng_xy_range()
    {
        var (install, bytes) = Load("LookoutPoint");
        var lev = LevFile.Parse(bytes);
        using var levels = new LevelLibrary(install);
        var things = levels.LoadThings("LookoutPoint").Things
            .Where(t => t.PositionX is not null)
            .ToList();
        var maxX = things.Max(t => t.PositionX!.Value);
        var maxY = things.Max(t => t.PositionY!.Value);
        Assert.True(maxX < lev.GridWidth * lev.CellSize + 8, $"maxX={maxX} width={lev.GridWidth}");
        Assert.True(maxY < lev.GridHeight * lev.CellSize + 8, $"maxY={maxY} height={lev.GridHeight}");
        Assert.True(things.Min(t => t.PositionX) > -8);
        Assert.True(things.Min(t => t.PositionY) > -8);
    }

    [Fact]
    public void Secondary_table_starts_at_33839_with_type_3()
    {
        foreach (var region in new[] { "LookoutPoint", "PicnicArea", "DemonDoor_Guild" })
        {
            var (_, bytes) = Load(region);
            Assert.Equal(3u, BitConverter.ToUInt32(bytes, LevFile.MaterialTableEnd));
            Assert.True(BitConverter.ToUInt32(bytes, LevFile.MaterialTableEnd + 4) > 0);
            Assert.Equal(67639, LevFile.SecondaryTableEnd);
        }
    }

    [Fact]
    public void Payload_after_sound_themes_begins_with_21()
    {
        foreach (var region in new[] { "LookoutPoint", "PicnicArea", "DemonDoor_Guild", "OakValeEast_v2" })
        {
            var lev = LevFile.Parse(Load(region).Bytes);
            Assert.True(lev.Raw.Length - lev.PayloadOffset > 64);
            Assert.Equal(21, BitConverter.ToInt32(lev.Raw, lev.PayloadOffset));
        }
    }

    [Fact]
    public void Stb_contains_expanded_lookout_lev_larger_than_wad()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var stbPath = Path.Combine(install.DataRoot, "Levels", "FinalAlbion_RT.stb");
        Assert.True(File.Exists(stbPath));
        using var wad = BbbArchive.Open(install.WadPath);
        var wadLev = wad.Find("LookoutPoint.lev");
        Assert.NotNull(wadLev);
        Assert.True(wadLev.Size > 100_000);
        Assert.True(File.Exists(stbPath));
        Assert.True(new FileInfo(stbPath).Length > 100_000_000);
    }

    [Fact]
    public void Stb_lookout_heightfield_is_8_by_8_cells_of_16_units()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var field = levels.LoadHeightField("LookoutPoint");
        Assert.NotNull(field);
        Assert.Equal(8, field.CellsX);
        Assert.Equal(8, field.CellsY);
        Assert.Equal(3232f, field.OriginX);
        Assert.Equal(3488f, field.OriginY);
        Assert.True(field.SampleCount >= 64);
        Assert.InRange(field.Heights[0, 0], 20f, 80f);
        Assert.InRange(field.Heights[4, 4], 20f, 80f);
        var tris = field.ToLocalTriangles();
        Assert.Equal(8 * 8 * 2, tris.Count);
        Assert.InRange(tris[0].A.Z, 20f, 80f);
        Assert.True(tris.Max(t => t.A.X) <= 128.1f);
    }

    [Fact]
    public void Stb_picnic_heightfield_matches_128x96_grid()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var field = levels.LoadHeightField("PicnicArea");
        Assert.NotNull(field);
        Assert.Equal(8, field.CellsX);
        Assert.Equal(6, field.CellsY);
        Assert.True(field.ToLocalTriangles().Count >= 48);
    }

    [Fact]
    public void Wad_payload_is_21_byte_cells_with_constant_60_not_height()
    {
        foreach (var (region, width, height) in new[]
                 {
                     ("LookoutPoint", 128, 128),
                     ("PicnicArea", 128, 96),
                     ("OakValeEast_v2", 96, 160),
                 })
        {
            var lev = LevFile.Parse(Load(region).Bytes);
            var grid = LevCellGrid.TryParse(lev);
            Assert.NotNull(grid);
            Assert.Equal(width, grid.Width);
            Assert.Equal(height, grid.Height);
            Assert.Equal(width * height, grid.RecordCount);
            Assert.All(Enumerable.Range(0, 64), i =>
            {
                var cell = grid.Cells[i % width, i / width];
                Assert.InRange(cell.Constant60, 50, 70);
            });
            Assert.Contains(
                Enumerable.Range(0, width * height).Select(i => grid.Cells[i % width, i / width].Material0),
                slot => slot is > 0 and < 255);
        }
    }

    [Fact]
    public void Cell_material_slots_index_the_ground_table()
    {
        var (_, bytes) = Load("LookoutPoint");
        var lev = LevFile.Parse(bytes);
        var grid = LevCellGrid.TryParse(lev);
        Assert.NotNull(grid);
        var bySlot = lev.Materials.ToDictionary(m => m.Slot);
        var used = 0;
        var named = 0;
        for (var y = 0; y < grid.Height; y++)
        for (var x = 0; x < grid.Width; x++)
        {
            var slot = grid.Cells[x, y].Material0;
            if (slot == 0xFF)
                continue;
            used++;
            if (bySlot.TryGetValue(slot, out var material) &&
                material.Name.StartsWith("GROUND_", StringComparison.Ordinal))
                named++;
        }

        Assert.True(used > 1000, $"used={used}");
        Assert.True(named > used / 2, $"named={named} used={used}");
        var sand = lev.Materials.First(m => m.Name == "GROUND_PATH_SAND");
        Assert.Equal(2, sand.Slot);
        Assert.Equal(1911u, sand.Id);
        Assert.Equal(2, grid.Cells[0, 0].Material0);
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var enums = HeaderEnums.Load(Path.Combine(install.DataRoot, "Defs", "RetailHeaders", "pc", "textures.h"));
        Assert.Equal(4133, LandscapeTextures.Resolve("GROUND_PATH_SAND", enums));
        Assert.Equal(414, LandscapeTextures.Resolve("GROUND_GRASS", enums));
        Assert.Equal(4118, LandscapeTextures.Resolve("PATH_COBBLES_IRREGULAR_ET", enums));
        Assert.NotEqual(1911, LandscapeTextures.Resolve("GROUND_PATH_SAND", enums));
        Assert.Equal(4133, LandscapeTextures.Resolve("GROUND_PATH_SAND", enums));
        Assert.True(LandscapeTextures.IsWaterOrSeaPass("SEA_OAKVALE_2"));
        Assert.True(LandscapeTextures.IsWaterOrSeaPass("WATER_GREYCLIFF_ET"));
        Assert.False(LandscapeTextures.FirstSeenWaterDrawShouldSubmit);
        Assert.Null(LandscapeTextures.TryResolve("SEA_OAKVALE_2", enums));
        Assert.Null(LandscapeTextures.TryResolve("WATER_GREYCLIFF_ET", enums));
        Assert.Null(LandscapeTextures.TryResolve("WATER_BWLAKE_0", enums));
        Assert.Equal(442, LandscapeTextures.WaterTexture(enums));
    }

    [Fact]
    public void Start_oakvale_has_a_sea_bank_and_no_water_bank()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var stb = StbArchive.Open(install.RuntimeStbPath);
        using var levels = new LevelLibrary(install);
        var sea = stb.Entries.Where(e =>
            e.Name.StartsWith("__ENGINE_SEA_STATIC_MAP_BANK_FILE__", StringComparison.Ordinal)).ToList();
        Assert.Equal(25, sea.Count);
        var oak = sea.Single(e => e.Name.EndsWith("StartOakVale", StringComparison.Ordinal));
        Assert.Equal(129966u, oak.Size);
        var oakBytes = stb.Read(oak);
        Assert.Equal(7363u, BitConverter.ToUInt32(oakBytes, 0));
        Assert.Equal(LandscapeTextures.SeaBankFirstU32, BitConverter.ToUInt32(oakBytes, 0));
        Assert.Equal(5, LandscapeTextures.StartOakValeSeaPrefix.Length);
        for (var i = 0; i < LandscapeTextures.StartOakValeSeaPrefix.Length; i++)
            Assert.Equal(LandscapeTextures.StartOakValeSeaPrefix[i], BitConverter.ToUInt32(oakBytes, i * 4));
        Assert.NotEqual(LandscapeTextures.RequiredWaterBankType, LandscapeTextures.StartOakValeSeaPrefix[0]);
        Assert.Equal(72, LandscapeTextures.SeaStreamObjectBytes);
        Assert.False(LandscapeTextures.SeaBindUsesType8Check);
        Assert.Equal(645, LandscapeTextures.WaterMeshReadySecondOffset);
        Assert.Equal(8u, LandscapeTextures.RequiredWaterBankType);
        Assert.Equal(2, LandscapeTextures.WaterType8CopiedDwords);
        Assert.Equal(508, LandscapeTextures.WaterDrawVectorFirst);
        Assert.Equal(624, LandscapeTextures.WaterDrawVectorLast);
        Assert.True(LandscapeTextures.FirstSeenWaterDrawIsEmpty);
        Assert.False(LandscapeTextures.WaterType8DwordsAreStoredOnRenderer);
        Assert.Equal(1448, LandscapeTextures.SeaInternOffset);
        Assert.Equal(1452, LandscapeTextures.SeaNameStringOffset);
        Assert.Equal(636, LandscapeTextures.WaterWantedNameOffset);
        Assert.Equal(1464, LandscapeTextures.SeaBankObjectOffset);
        Assert.Equal(630, LandscapeTextures.WaterMeshReadyOffset);
        Assert.True(LandscapeTextures.FirstSeenWaterWantedNameIsZero);
        Assert.False(LandscapeTextures.FirstSeenSeaBindRuns);
        Assert.Equal(0x00B23F00u, LandscapeTextures.WaterWantedNameSetter);
        Assert.Equal(0x00B23900u, LandscapeTextures.WaterWantedNameThisSetter);
        Assert.Equal(14, LandscapeTextures.WaterWantedNameSetterVtblSlot);
        Assert.False(LandscapeTextures.FirstSeenCallsWantedNameSetter);
        Assert.False(LandscapeTextures.FirstSeenLoadWaterDataFindsIntern);
        Assert.True(LandscapeTextures.FirstSeenWaterDrawEmptyIsBareRet);
        Assert.Equal(0x00B41FA0u, LandscapeTextures.LoadWaterData);
        Assert.Equal(0x1436EC8u, LandscapeTextures.LoadWaterDataIntern);
        Assert.Equal(0x00B429CBu, LandscapeTextures.LoadWaterDataOnlyCaller);
        Assert.Equal(0x00B420E4u, LandscapeTextures.LoadWaterDataMissingInternRet);
        Assert.Equal(0x00B783F0u, LandscapeTextures.WaterDraw);
        Assert.Equal(0x00B7851Du, LandscapeTextures.WaterDrawEmptyCheck);
        Assert.Equal(0x00B7A865u, LandscapeTextures.WaterDrawEmptyReturn);
        Assert.Equal(0x00B72180u, LandscapeTextures.WaterDrawSecondGate);
        Assert.Equal(0x00B78584u, LandscapeTextures.WaterDrawSecondGateSite);
        Assert.False(LandscapeTextures.FirstSeenWaterDrawReachesSecondGate);
        Assert.False(LandscapeTextures.FirstSeenWaterDrawShouldSubmit);
        Assert.False(LandscapeTextures.WaterDrawShouldSubmit_00B783F0(
            false, false, false, false, false, false,
            false, false, false, false, false));
        Assert.True(LandscapeTextures.WaterDrawShouldSubmit_00B783F0(
            true, false, false, false, false, false,
            false, false, false, false, false));
        Assert.True(LandscapeTextures.WaterDrawShouldSubmit_00B783F0(
            false, false, false, false, false, false,
            false, false, false, true, true));
        Assert.False(LandscapeTextures.WaterDrawShouldSubmit_00B783F0(
            false, false, false, false, false, false,
            false, false, false, true, false));
        Assert.Equal(0x012A3364u, LandscapeTextures.WaterRendererVtbl);
        Assert.Equal(16, LandscapeTextures.WaterDrawVtblOffset);
        Assert.Equal(0x00B71FB0u, LandscapeTextures.WaterPrepare);
        Assert.Equal(4, LandscapeTextures.WaterPrepareVtblOffset);
        Assert.Equal(0x00B6DC40u, LandscapeTextures.WaterPrepareBind);
        Assert.Equal(0x00B7ED70u, LandscapeTextures.WaterQuery);
        Assert.Equal(1, LandscapeTextures.WaterQueryReturns);
        Assert.True(LandscapeTextures.WaterPrepareAlwaysClearsVectors);
        Assert.False(LandscapeTextures.FirstSeenWaterPrepareFillsMesh);
        Assert.Equal(0x00BF44A0u, LandscapeTextures.WaterEnqueue);
        Assert.Equal(0x00BF57D1u, LandscapeTextures.WaterEnqueueOnlyCaller);
        Assert.Equal(28, LandscapeTextures.WaterEnqueueTypeOffset);
        Assert.Equal(0x244, LandscapeTextures.WaterType4QueueOffset);
        Assert.Equal(4, LandscapeTextures.C1LayerType);
        Assert.False(LandscapeTextures.FirstSeenReadsSeaPrefixWords);
        Assert.False(LandscapeTextures.FirstSeenCallsSeaMeshCopy);
        Assert.Equal(0x00B6D420u, LandscapeTextures.SeaMeshCopy);
        Assert.Equal(0x00BE91E0u, LandscapeTextures.SeaMeshBuilder);
        Assert.Equal(0x00B6DECDu, LandscapeTextures.SeaMeshCopyCallSite);
        Assert.Equal(12, LandscapeTextures.SeaVertexStrideBytes);
        Assert.Equal(101, LandscapeTextures.SeaIndexFormat);
        Assert.Equal(180, LandscapeTextures.SeaMeshPrimitiveCountOffset);
        Assert.Equal(7363u, LandscapeTextures.StartOakValeSeaPrefix[0]);
        Assert.Equal(44259u, LandscapeTextures.StartOakValeSeaPrefix[1]);
        Assert.False(LandscapeTextures.IsLoadableWaterBank(oakBytes));
        Assert.DoesNotContain(stb.Entries, e =>
            e.Name.Contains("__ENGINE_WATER_STATIC_MAP_BANK_FILE__", StringComparison.Ordinal));

        var height = levels.LoadHeightField("StartOakValeWest");
        var compiled = levels.LoadCompiledLev("StartOakValeWest");
        Assert.NotNull(height);
        Assert.NotNull(compiled);
        Assert.Contains(compiled.Materials, m => m.Name.StartsWith("SEA_OAKVALE", StringComparison.Ordinal));
        Assert.Contains(compiled.Materials, m => m.Name.StartsWith("WATER_", StringComparison.Ordinal));
        var cells = LevCellGrid.TryParse(compiled);
        Assert.NotNull(cells);
        var enums = HeaderEnums.Load(Path.Combine(install.DataRoot, "Defs", "RetailHeaders", "pc", "textures.h"));
        var tris = height.ToTileTriangles(cells, compiled.Materials, enums);
        Assert.True(tris.Count > 1000, $"tris={tris.Count}");
        Assert.DoesNotContain(tris, t => t.TextureId is 4106 or 4107 or 4108);
        Assert.Contains(tris, t => t.TextureId is 4130 or 414 or 428 or 412);
        Assert.DoesNotContain(tris, t => t.TextureId == 442);
        Assert.Equal(0.125f, LandscapeTextures.UvScale);
        Assert.False(LandscapeTextures.FirstSeenUploadsC1LayerFlip);
        Assert.False(LandscapeTextures.FirstSeenLandscapeVsReadsC1);
        Assert.Equal(4, LandscapeTextures.C1LayerType);
        Assert.Equal(0x0139C5D8u, LandscapeTextures.UvTable);
        Assert.Equal(0x00BF51D4u, LandscapeTextures.PerCellC1Upload);
        Assert.Equal(24, LandscapeTextures.GpuVertexStrideBytes);
        Assert.Equal(20, LandscapeTextures.GpuExtraOffset);
        Assert.Equal(15, LevTileMesh.VertexStride);
        Assert.Equal(0x00BFE050u, LandscapeTextures.ExpandVerts);
        Assert.Equal(0x00BF3E17u, LandscapeTextures.ExpandVertsCaller);
        Assert.Equal(0x00BFDEC0u, LandscapeTextures.UnpackNormal);
        Assert.Equal(0x00BDA3D0u, LandscapeTextures.CreateVertexBuffer);
        Assert.Equal(0x00A63150u, LandscapeTextures.CreateVertexBufferWrapper);
        Assert.True(LandscapeTextures.FirstSeenOt0FromV3);
        Assert.False(LandscapeTextures.FirstSeenOt0IsAlbedo);
        Assert.Equal(40, LandscapeTextures.Ot1RegisterX);
        Assert.Equal(41, LandscapeTextures.Ot1RegisterY);
        Assert.True(LandscapeTextures.FirstSeenOt1Projected);
        Assert.False(LandscapeTextures.FirstSeenOt1HasExplicitWriter);
        Assert.True(LandscapeTextures.FirstSeenOt1UsesDeviceDefault);
        Assert.Equal(2, LandscapeTextures.PerCellFirstSlot);
        Assert.Equal(3, LandscapeTextures.PerCellSecondSlot);
        Assert.Equal(1, LandscapeTextures.PerCellC1FlipSlot);
        Assert.Equal(18, LandscapeTextures.SetVsConstantF1CallCount);
        Assert.Equal(0x00B67480u, LandscapeTextures.LandscapeSharedSetup);
        Assert.Equal(Vector4.Zero, LandscapeTextures.Ot1C40);
        Assert.Equal(Vector4.Zero, LandscapeTextures.Ot1C41);
        Assert.Equal(0x00BF4EB7u, LandscapeTextures.PerCellFirstSlotSet);
        Assert.Equal(0x00989A60u, LandscapeTextures.SetVsConstantF1);
        Assert.Equal(0x0098D4A0u, LandscapeTextures.InnerVsObjectCtor);
        Assert.Equal(0, LandscapeTextures.FirstSeenInnerRegisterBase);
        var sample = tris.First(t => t.A.X > 1f && t.A.Y > 1f);
        Assert.Equal(Vector2.Zero, LandscapeTextures.ProjectOt1(sample.A));
        Assert.Equal(0f, sample.UvA.X, 5);
        Assert.Equal(0f, sample.UvA.Y, 5);
        Assert.True(LandscapeTextures.FirstSeenBackgroundOt0IsV3);
        Assert.True(LandscapeTextures.FirstSeenBackgroundPsMulX2);
        Assert.InRange(sample.ExtraA.X, 0.98f, 1.02f);
        Assert.InRange(sample.ExtraA.Y, 0.3f, 0.7f);
        Assert.InRange(sample.ExtraA.Z, 0.3f, 0.7f);
        Assert.Equal(sample.ExtraA.Y, LandscapeTextures.Ot0FromExtra(sample.ExtraA).X, 5);
        Assert.Equal(sample.ExtraA.Z, LandscapeTextures.Ot0FromExtra(sample.ExtraA).Y, 5);
        var tile = height.Tiles.Tiles.First(t => t.Vertices.Count >= 16);
        Assert.True(tile.Vertices.All(v => Math.Abs(v.ExtraRgb.X - 1f) < 0.02f));
        Assert.True(tile.Vertices.All(v => v.ExtraRgb.Y is > 0.4f and < 0.6f));
        Assert.True(tile.Vertices.All(v => v.ExtraRgb.Z is > 0.4f and < 0.6f));
        var extra0 = tile.Vertices[0];
        Assert.Equal(extra0.ExtraRgb.Y, LandscapeTextures.Ot0FromExtra(extra0.ExtraRgb).X, 5);
        Assert.True(
            Math.Abs(extra0.WorldX * LandscapeTextures.UvScale - extra0.ExtraRgb.Y) > 1f,
            $"extraY={extra0.ExtraRgb.Y} world*scale={extra0.WorldX * LandscapeTextures.UvScale}");
        Assert.Equal(4, LandscapeFrustum.PlaneCount);
        Assert.Equal(16, LandscapeFrustum.PlaneStrideBytes);
        Assert.Equal(448, LandscapeFrustum.PlaneBaseOffset);
        Assert.Equal(168, LandscapeFrustum.AabbMinOffset);
        Assert.Equal(180, LandscapeFrustum.AabbMaxOffset);
        Assert.Equal(0x00BDC2D0u, LandscapeFrustum.PatchSubmit);
        Assert.Equal(0x00B6B1A5u, LandscapeFrustum.PatchSubmitCaller);
        Assert.Equal(0x40, LandscapeFrustum.LandscapeBit40);
        Assert.True(LandscapeFrustum.FirstSeenUsesFourPlaneAabb);
        Assert.Equal(0x00B30B50u, LandscapeFrustum.CameraSetup);
        Assert.Equal(0x00B2FC50u, LandscapeFrustum.ExtractOther);
        Assert.Equal(0x00B4AF50u, LandscapeFrustum.CameraCopy);
        Assert.Equal(0x00B2FD60u, LandscapeFrustum.Extract);
        Assert.Equal(0x00A14440u, LandscapeFrustum.Normalize);
        Assert.Equal(0x00A42140u, LandscapeFrustum.StorePlane);
        Assert.Equal(0.5f, LandscapeFrustum.FovHalfScale);
        Assert.Equal(0.75f, LandscapeFrustum.LetterboxFourByThree);
        Assert.Equal(1f, LandscapeFrustum.NormalizeDivisor);
        Assert.Equal(128, LandscapeFrustum.ViewMatrixOffset);
        Assert.Equal(228, LandscapeFrustum.InverseOffset);
        Assert.Equal(2, LandscapeFrustum.InverseRow0Register);
        Assert.Equal(18, LandscapeFrustum.LayoutFogRegister);
        Assert.Equal(1000f, LandscapeFrustum.FogRecordStart);
        Assert.Equal(2000f, LandscapeFrustum.FogRecordEnd);
        Assert.Equal(new Vector4(0f, 0f, 0f, 1f), LandscapeFrustum.FogRecordColor);
        Assert.Equal(0x00B54310u, LandscapeFrustum.CameraConstantUpload);
        Assert.Equal(212, LandscapeFrustum.CotHOffset);
        Assert.Equal(216, LandscapeFrustum.CotVOffset);
        Assert.Equal(84, LandscapeFrustum.TwoFovFlagOffset);
        Assert.Equal(76, LandscapeFrustum.FovHOffset);
        Assert.Equal(104, LandscapeFrustum.SourceReadyOffset);
        Assert.Equal(0x00BF6F80u, LandscapeFrustum.AabbFill);
        Assert.Equal(0x00BDC280u, LandscapeFrustum.AabbFillCaller);
        Assert.Equal(0x00BDC180u, LandscapeFrustum.AabbFillSetup);
        Assert.Equal(0x00BF6E20u, LandscapeFrustum.TessellatorCtor);
        Assert.Equal(92, LandscapeFrustum.MapSizeXOffset);
        Assert.Equal(94, LandscapeFrustum.MapSizeYOffset);
        Assert.Equal(96, LandscapeFrustum.MapOriginXOffset);
        Assert.Equal(98, LandscapeFrustum.MapOriginYOffset);
        Assert.Equal(0, LandscapeFrustum.FirstSeenAabbStartX);
        Assert.Equal(0, LandscapeFrustum.FirstSeenAabbStartY);
        Assert.Equal(0f, LandscapeFrustum.AabbZ);
        LandscapeFrustum.PatchAabb(0f, 0f, height.FineWidth, height.FineHeight, out var patchMin, out var patchMax);
        Assert.Equal(0f, patchMin.X);
        Assert.Equal(0f, patchMin.Y);
        Assert.Equal(0f, patchMin.Z);
        Assert.Equal(0f, patchMax.Z);
        Assert.Equal(height.FineWidth, patchMax.X);
        Assert.Equal(height.FineHeight, patchMax.Y);
        Assert.True(height.FineWidth >= 128, $"oakFineW={height.FineWidth}");
        Assert.False(LandscapeFrustum.FirstSeenTwoFovFlag);
        Assert.Equal(0.2f, LandscapeFrustum.FirstSeenFovTurns);
        Assert.Equal(360f, LandscapeFrustum.FovTurnsToDegrees);
        Assert.Equal(LandscapeFrustum.TurnsToRadians(0.2f), float.DegreesToRadians(72f), 4);
        LandscapeFrustum.LetterboxCots(
            LandscapeFrustum.TurnsToRadians(LandscapeFrustum.FirstSeenFovTurns), 4f, 3f, out var cotH, out var cotV);
        Assert.Equal(LandscapeFrustum.CotHalfAngle(float.DegreesToRadians(72f)), cotH, 5);
        Assert.Equal(cotH * (4f / 3f), cotV, 5);
        var left = new LandscapeFrustum.Plane(new Vector3(1f, 0f, 0f), 0f);
        Assert.False(LandscapeFrustum.AabbIsOutside(new Vector3(-1f, -1f, -1f), new Vector3(1f, 1f, 1f), [left]));
        Assert.True(LandscapeFrustum.AabbIsOutside(new Vector3(2f, -1f, -1f), new Vector3(3f, 1f, 1f), [left]));

        var covered = new bool[cells.Width, cells.Height];
        foreach (var t in tris)
        {
            var minX = (int)MathF.Floor(MathF.Min(t.A.X, MathF.Min(t.B.X, t.C.X)));
            var minY = (int)MathF.Floor(MathF.Min(t.A.Y, MathF.Min(t.B.Y, t.C.Y)));
            var maxX = (int)MathF.Ceiling(MathF.Max(t.A.X, MathF.Max(t.B.X, t.C.X)));
            var maxY = (int)MathF.Ceiling(MathF.Max(t.A.Y, MathF.Max(t.B.Y, t.C.Y)));
            for (var y = Math.Max(0, minY); y < Math.Min(cells.Height, maxY); y++)
            for (var x = Math.Max(0, minX); x < Math.Min(cells.Width, maxX); x++)
            {
                var px = x + 0.5f;
                var py = y + 0.5f;
                if (PointInTri(px, py, t.A.X, t.A.Y, t.B.X, t.B.Y, t.C.X, t.C.Y))
                    covered[x, y] = true;
            }
        }

        var bySlot = compiled.Materials.ToDictionary(m => m.Slot);
        var pathCells = 0;
        var pathCovered = 0;
        var usable = 0;
        var usableCovered = 0;
        for (var y = 0; y < cells.Height; y++)
        for (var x = 0; x < cells.Width; x++)
        {
            var slot = cells.Cells[x, y].Material0;
            if (slot == 0xFF || !bySlot.TryGetValue(slot, out var mat))
                continue;
            if (!LandscapeTextures.IsUsable(mat.Name))
                continue;
            if (LandscapeTextures.IsWaterOrSeaPass(mat.Name))
                continue;
            usable++;
            if (covered[x, y])
                usableCovered++;
            if (!mat.Name.Contains("PATH", StringComparison.Ordinal) &&
                !mat.Name.Contains("PAVING", StringComparison.Ordinal))
                continue;
            pathCells++;
            if (covered[x, y])
                pathCovered++;
        }

        Assert.True(pathCells > 100, $"pathCells={pathCells}");
        // Exe draws STB strips only. Village adaptive tiles omit many PATH
        // 1 m cells; invented fill is not the landscape pass.
        Assert.True(pathCovered > pathCells / 2, $"pathCovered={pathCovered}/{pathCells}");
        Assert.True(pathCovered < pathCells, $"fill must not close every path cell pathCovered={pathCovered}/{pathCells}");
        Assert.True(usableCovered > usable / 2, $"usableCovered={usableCovered}/{usable}");
    }

    private static bool PointInTri(
        float px, float py, float ax, float ay, float bx, float by, float cx, float cy)
    {
        var v0x = cx - ax;
        var v0y = cy - ay;
        var v1x = bx - ax;
        var v1y = by - ay;
        var v2x = px - ax;
        var v2y = py - ay;
        var dot00 = v0x * v0x + v0y * v0y;
        var dot01 = v0x * v1x + v0y * v1y;
        var dot02 = v0x * v2x + v0y * v2y;
        var dot11 = v1x * v1x + v1y * v1y;
        var dot12 = v1x * v2x + v1y * v2y;
        var inv = dot00 * dot11 - dot01 * dot01;
        if (MathF.Abs(inv) < 1e-12f)
            return false;
        var u = (dot11 * dot02 - dot01 * dot12) / inv;
        var v = (dot00 * dot12 - dot01 * dot02) / inv;
        return u >= -1e-4f && v >= -1e-4f && u + v <= 1f + 1e-4f;
    }

    [Fact]
    public void PeekMapHeader_is_00b3efa0_not_full_parse()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var header = levels.PeekMapHeader("LookoutPoint");
        Assert.NotNull(header);
        Assert.Equal(LevFile.Version, header.Value.Version);
        Assert.Equal(LevFile.FormatConstant, header.Value.Constant);
        Assert.True(header.Value.GridWidth >= 64, $"w={header.Value.GridWidth}");
        Assert.True(header.Value.CompiledSize > 1000, $"lev={header.Value.CompiledSize}");
        Assert.True(header.Value.StbSize > 0, $"stb={header.Value.StbSize}");
        Assert.True(header.Value.HeightSamples > 0);
        Assert.Equal(48, LevFile.NativeHeaderBytes);
    }

    [Fact]
    public void LevelLibrary_reuses_lev_and_height_parses()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var a = levels.LoadCompiledLev("LookoutPoint");
        var b = levels.LoadCompiledLev("LookoutPoint");
        var h1 = levels.LoadHeightField("LookoutPoint");
        var h2 = levels.LoadHeightField("LookoutPoint");
        Assert.NotNull(a);
        Assert.Same(a, b);
        Assert.NotNull(h1);
        Assert.Same(h1, h2);
    }

    [Fact]
    public void LevelLibrary_unload_map_drops_region_not_wad()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var a = levels.LoadCompiledLev("LookoutPoint");
        var cells = levels.LoadCells("LookoutPoint");
        var things = levels.TryLoadThings("LookoutPoint");
        Assert.NotNull(a);
        Assert.NotEmpty(cells);
        Assert.NotNull(things);
        Assert.True(levels.HasCachedCells("LookoutPoint"));
        Assert.True(levels.HasCachedThings("LookoutPoint"));
        levels.UnloadMap("LookoutPoint");
        Assert.False(levels.HasCachedCells("LookoutPoint"));
        Assert.True(levels.HasCachedThings("LookoutPoint"));
        levels.UnloadThings("LookoutPoint");
        Assert.False(levels.HasCachedThings("LookoutPoint"));
        var b = levels.LoadCompiledLev("LookoutPoint");
        Assert.NotNull(b);
        Assert.NotSame(a, b);
        Assert.Equal(a.GridWidth, b.GridWidth);
    }

    [Fact]
    public void Fine_lookout_mesh_is_128_by_128_interpolated_from_coarse()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var height = levels.LoadHeightField("LookoutPoint");
        var compiled = levels.LoadCompiledLev("LookoutPoint");
        Assert.NotNull(height);
        Assert.NotNull(compiled);
        var cells = LevCellGrid.TryParse(compiled);
        Assert.NotNull(cells);
        var tris = height.ToFineTriangles(cells, compiled.Materials);
        Assert.Equal(128 * 128 * 2, tris.Count);
        Assert.InRange(tris[0].A.Z, 20f, 80f);
        Assert.Equal(0f, tris[0].A.X);
        Assert.True(tris.Max(t => t.A.X) <= 128.1f);
        Assert.True(tris.Count(t => t.TextureId is 4133 or 414 or 428 or 4118) > 1000);
    }

    [Fact]
    public void Stb_section_two_starts_at_u32_2048()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var stb = StbArchive.Open(install.RuntimeStbPath);
        foreach (var (region, expected) in new[] { ("LookoutPoint", 6144u), ("PicnicArea", 4096u) })
        {
            var entry = stb.FindLev(region);
            Assert.NotNull(entry);
            var bytes = stb.Read(entry);
            Assert.Equal(1u, BitConverter.ToUInt32(bytes, 0));
            Assert.Equal(expected, BitConverter.ToUInt32(bytes, 2048));
            Assert.NotEqual(0, bytes[expected]);
            Assert.Equal(0, bytes[expected - 1]);
        }
    }

    [Fact]
    public void Stb_tiles_are_lzo_meshes_of_world_xy_and_z()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        using var stb = StbArchive.Open(install.RuntimeStbPath);
        var lookout = levels.LoadHeightField("LookoutPoint");
        Assert.NotNull(lookout);
        Assert.Equal(64, lookout.TileCount);
        Assert.True(lookout.FineSampleCount > 2000, $"fine={lookout.FineSampleCount}");
        Assert.Equal(128, lookout.FineWidth);
        Assert.Equal(128, lookout.FineHeight);

        var bytes = stb.Read(stb.FindLev("LookoutPoint")!);
        var tiles = LevTileMesh.Parse(bytes, lookout.OriginX, lookout.OriginY, lookout.CellsX, lookout.CellsY);
        Assert.Equal(64, tiles.Tiles.Count);
        var first = tiles.Tiles.First(tile => tile.Index == 0);
        Assert.Equal(289, first.Vertices.Count);
        Assert.Equal(3248, first.Vertices[0].WorldX);
        Assert.Equal(3488, first.Vertices[0].WorldY);
        Assert.InRange(first.Vertices[0].Z, 20f, 80f);
        Assert.Equal(3264, first.Vertices[288].WorldX);
        Assert.Equal(3504, first.Vertices[288].WorldY);
        Assert.True(first.Vertices.All(v => v.Normal.Length() is > 0.9f and < 1.1f));
        Assert.True(first.Vertices.Count(v => v.Normal.Z > 0.7f) > 200);
        Assert.Equal(1f, first.Vertices[0].ExtraRgb.X, 2);
        Assert.InRange(first.Vertices[0].ExtraRgb.Y, 0.3f, 0.7f);

        // Interior 1-unit sample is not just the 16-unit bilinear.
        var pred = Bilinear(lookout, 18 / 16f, 2 / 16f);
        Assert.True(
            Math.Abs(lookout.FineHeights[18, 2] - pred) > 0.2f,
            $"fine[18,2]={lookout.FineHeights[18, 2]} bilinear={pred}");
    }

    [Fact]
    public void Stb_picnic_tiles_decompress_to_world_verts()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var picnic = levels.LoadHeightField("PicnicArea");
        Assert.NotNull(picnic);
        Assert.Equal(48, picnic.TileCount);
        Assert.True(picnic.FineSampleCount > 1000, $"fine={picnic.FineSampleCount}");
        Assert.Equal(128, picnic.FineWidth);
        Assert.Equal(96, picnic.FineHeight);
        Assert.InRange(picnic.FineHeights[16, 0], 15f, 80f);
    }

    [Fact]
    public void Stb_section_two_is_the_map_origin_tile()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        using var stb = StbArchive.Open(install.RuntimeStbPath);
        foreach (var (region, x0, y0, x1, y1) in new[]
                 {
                     ("LookoutPoint", 3232, 3488, 3248, 3504),
                     ("PicnicArea", 3104, 3520, 3120, 3536),
                 })
        {
            var field = levels.LoadHeightField(region);
            Assert.NotNull(field);
            var bytes = stb.Read(stb.FindLev(region)!);
            var tiles = LevTileMesh.Parse(bytes, field.OriginX, field.OriginY, field.CellsX, field.CellsY);
            Assert.NotNull(tiles.Section2);
            var s2 = tiles.Section2.Value;
            Assert.Equal(-1, s2.Index);
            Assert.True(s2.Vertices.Count is >= 200 and <= 289, $"{region} s2 v={s2.Vertices.Count}");
            Assert.Equal(x0, s2.Vertices[0].WorldX);
            Assert.Equal(y0, s2.Vertices[0].WorldY);
            Assert.InRange(s2.Vertices[0].Z, 10f, 80f);
            Assert.Equal(x0, s2.Vertices.Min(v => v.WorldX));
            Assert.Equal(y0, s2.Vertices.Min(v => v.WorldY));
            Assert.Equal(x1, s2.Vertices.Max(v => v.WorldX));
            Assert.Equal(y1, s2.Vertices.Max(v => v.WorldY));
        }

        var lookout = levels.LoadHeightField("LookoutPoint")!;
        var origin = LevTileMesh.Parse(
            stb.Read(stb.FindLev("LookoutPoint")!),
            lookout.OriginX, lookout.OriginY, lookout.CellsX, lookout.CellsY).Section2!.Value.Vertices[0];
        Assert.Equal(origin.Z, lookout.FineHeights[0, 0], 2);
        var pred = Bilinear(lookout, 8 / 16f, 4 / 16f);
        Assert.True(
            Math.Abs(lookout.FineHeights[8, 4] - pred) > 0.15f,
            $"origin interior fine[8,4]={lookout.FineHeights[8, 4]} bilinear={pred}");
    }

    [Fact]
    public void Lookout_tile_mesh_is_not_a_filled_128_grid()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var height = levels.LoadHeightField("LookoutPoint");
        var compiled = levels.LoadCompiledLev("LookoutPoint");
        Assert.NotNull(height);
        Assert.NotNull(compiled);
        var cells = LevCellGrid.TryParse(compiled);
        Assert.NotNull(cells);
        var tris = height.ToTileTriangles(cells, compiled.Materials);
        Assert.True(tris.Count > 8_000, $"tris={tris.Count}");
        Assert.True(tris.Count < 128 * 128 * 2 * 3, $"unbounded mesh tris={tris.Count}");
        Assert.Contains(tris, t => t.Normal.Z > 0);
        Assert.False(LandscapeStrip.FirstSeenRewindsNegativeNz);
        Assert.True(tris.Max(t => t.A.X) >= 120);
        Assert.True(tris.Min(t => t.A.X) <= 2);
        Assert.Contains(tris, t => t.TextureId is 4133 or 414);

        var full = height.Tiles.Tiles.Count(tile => tile.Vertices.Count == 289);
        Assert.True(full >= 8, $"fullTiles={full}");
        Assert.True(tris.Count > full * 16 * 16, $"expected both halves of each quad tris={tris.Count} full={full}");
    }

    [Fact]
    public void Adaptive_tile_leftover_is_a_u16_triangle_list()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var stb = StbArchive.Open(install.RuntimeStbPath);
        using var levels = new LevelLibrary(install);
        var lookout = levels.LoadHeightField("LookoutPoint")!;
        var bytes = stb.Read(stb.FindLev("LookoutPoint")!);
        var tiles = LevTileMesh.Parse(bytes, lookout.OriginX, lookout.OriginY, lookout.CellsX, lookout.CellsY);
        var adaptive = tiles.Tiles.Where(tile => tile.Vertices.Count < 289 && tile.Indices.Count >= 9).ToList();
        Assert.True(adaptive.Count >= 8, $"adaptive={adaptive.Count}");
        Assert.All(adaptive, tile =>
        {
            Assert.True(tile.Indices.Count >= 9);
            Assert.All(tile.Indices, index => Assert.InRange(index, 0, tile.Vertices.Count - 1));
        });

        // Header u16@+4 is D3D PrimitiveCount. The two indices after it finish
        // the last 1 m triangle of the 16 m strip (area ~1, not a sliver).
        Assert.Contains(adaptive, tile =>
        {
            if (tile.Indices.Count < 5)
                return false;
            var a = tile.Vertices[tile.Indices[^3]];
            var b = tile.Vertices[tile.Indices[^2]];
            var c = tile.Vertices[tile.Indices[^1]];
            var abx = b.WorldX - a.WorldX;
            var aby = b.WorldY - a.WorldY;
            var acx = c.WorldX - a.WorldX;
            var acy = c.WorldY - a.WorldY;
            var area = Math.Abs(abx * acy - aby * acx);
            return area > 0.5f && area < 4f;
        });

        var full = tiles.Tiles.Where(tile => tile.Vertices.Count == 289).ToList();
        Assert.True(full.Count >= 8);
        Assert.Contains(full, tile => tile.Indices.Count == 0);
    }

    [Fact]
    public void Adaptive_tile_stores_edge_strip_objects_after_the_primary_strip()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var stb = StbArchive.Open(install.RuntimeStbPath);
        using var levels = new LevelLibrary(install);
        var lookout = levels.LoadHeightField("LookoutPoint")!;
        var bytes = stb.Read(stb.FindLev("LookoutPoint")!);
        var tiles = LevTileMesh.Parse(bytes, lookout.OriginX, lookout.OriginY, lookout.CellsX, lookout.CellsY);
        var withExtras = tiles.Tiles.Where(tile => tile.Extras.Count >= 3).ToList();
        Assert.True(withExtras.Count >= 8, $"tilesWithExtras={withExtras.Count}");
        Assert.All(withExtras, tile =>
        {
            Assert.All(tile.Extras, extra =>
            {
                Assert.True(extra.Vertices.Count >= 3);
                Assert.True(extra.Indices.Count >= 3);
                Assert.All(extra.Indices, index => Assert.InRange(index, 0, extra.Vertices.Count - 1));
                Assert.InRange(extra.Vertices[0].WorldX, 3200, 3400);
                Assert.InRange(extra.Vertices[0].WorldY, 3450, 3650);
                Assert.InRange(extra.Vertices[0].Z, 15, 80);
            });
        });

        var compiled = levels.LoadCompiledLev("LookoutPoint")!;
        var cells = LevCellGrid.TryParse(compiled)!;
        var tris = lookout.ToTileTriangles(cells, compiled.Materials);
        Assert.True(tris.Count > 36_000, $"edge strips should raise coverage tris={tris.Count}");
        Assert.True(tris.Count < 128 * 128 * 2 * 3, $"unbounded mesh tris={tris.Count}");
    }

    [Fact]
    public void Compressed_tile_payload_is_not_a_17_by_17_float_grid()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var stb = StbArchive.Open(install.RuntimeStbPath);
        var bytes = stb.Read(stb.FindLev("LookoutPoint")!);
        var off = (int)BitConverter.ToUInt32(bytes, 2056 + 28);
        var size = (int)BitConverter.ToUInt32(bytes, 2056 + 32);
        var ok = 0;
        for (var i = 0; i < 17 * 17 && 8 + i * 4 + 4 <= size; i++)
        {
            var z = BitConverter.ToSingle(bytes, off + 8 + i * 4);
            if (z is >= 15f and <= 80f)
                ok++;
        }

        Assert.True(ok < 80, $"unexpected dense f32 grid without LZO ok={ok}");
    }

    [Fact]
    public void Stb_section_two_is_not_a_regular_xyz_stream()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var stb = StbArchive.Open(install.RuntimeStbPath);
        var bytes = stb.Read(stb.FindLev("LookoutPoint")!);
        var start = (int)BitConverter.ToUInt32(bytes, 2048);
        var count = BitConverter.ToInt32(bytes, start + 4);
        var zOk = 0;
        for (var i = 0; i < Math.Min(count, 4000); i++)
        {
            var o = start + 8 + i * 12;
            if (o + 12 > bytes.Length)
                break;
            var z = BitConverter.ToSingle(bytes, o + 8);
            if (z is >= 15f and <= 80f)
                zOk++;
        }

        Assert.True(zOk < 400, $"unexpected dense xyz zOk={zOk}");
    }

    [Fact]
    public void Lookout_payload_lzo_does_not_decode_as_dense_f32_grid()
    {
        var lev = LevFile.Parse(Load("LookoutPoint").Bytes);
        var cursor = lev.PayloadOffset;
        var decoded = Fable.Formats.IO.Lzo.DecompressFramed(lev.Raw, ref cursor, lev.CellCount * 4);
        var inRange = 0;
        for (var i = 0; i + 4 <= decoded.Length; i += 4)
        {
            var value = BitConverter.ToSingle(decoded, i);
            if (value is >= 15f and <= 80f)
                inRange++;
        }

        // Document the negative: framed LZO at payload start is not the heightfield.
        Assert.True(inRange < lev.CellCount / 4, $"unexpectedly dense height decode inRange={inRange}");
    }

    private static float Bilinear(LevHeightField field, float fx, float fy)
    {
        var x0 = Math.Clamp((int)MathF.Floor(fx), 0, field.CellsX);
        var y0 = Math.Clamp((int)MathF.Floor(fy), 0, field.CellsY);
        var x1 = Math.Min(x0 + 1, field.CellsX);
        var y1 = Math.Min(y0 + 1, field.CellsY);
        var tx = Math.Clamp(fx - x0, 0f, 1f);
        var ty = Math.Clamp(fy - y0, 0f, 1f);
        return (field.Heights[x0, y0] * (1 - tx) + field.Heights[x1, y0] * tx) * (1 - ty)
             + (field.Heights[x0, y1] * (1 - tx) + field.Heights[x1, y1] * tx) * ty;
    }

    private static string ReadZ(byte[] data, int offset)
    {
        var end = offset;
        while (end < data.Length && data[end] != 0)
            end++;
        return System.Text.Encoding.ASCII.GetString(data, offset, end - offset);
    }
}
