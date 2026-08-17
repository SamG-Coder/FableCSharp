using System.Numerics;
using Fable.Core;
using Fable.Formats;
using Fable.Formats.Banks;
using Fable.Formats.Defs;
using Fable.Formats.Levels;
using Fable.Formats.Meshes;
using Fable.Formats.Shaders;
using Fable.Game;
using Fable.Render.Parity.Dx9Vulkan;

namespace Fable.Formats.Tests;

/// <summary>
/// Father C3D bytes through the exe PALSKIN path:
/// <c>00A8FD40</c> primitive serialize,
/// <c>00A8E770</c>/<c>00A8EB10</c> animated block,
/// <c>00BCFB00</c> dest[bone*64] packed to c38,
/// <c>VSHADER_PALSKIN_DIRLIGHT_FOG</c> a0-relative dp4.
/// </summary>
public sealed class PalskinPipelineTests
{
    private static (GameInstall Install, MeshFile Mesh, int MeshId, string Name) LoadCreature(string def)
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var namesPath = install.FindCompiledDef("names.bin");
        var binPath = install.FindCompiledDef("game.bin");
        Assert.NotNull(namesPath);
        Assert.NotNull(binPath);
        var bin = GameBin.Load(binPath, NamesBin.Load(namesPath));
        var meshId = bin.FindMeshId(def)
                     ?? HeaderEnums.Load(Path.Combine(install.DataRoot, "Defs", "RetailHeaders", "meshdata.h"))
                         .FindMeshId(def);
        Assert.True(meshId is > 0, $"no mesh for {def}");
        var path = Path.Combine(install.DataRoot, "graphics", "graphics.big");
        using var big = BigArchive.Open(path);
        var bank = big.SubBanks.First(item => item.Name.Contains("MESH", StringComparison.OrdinalIgnoreCase));
        var entry = big.ReadEntries(bank).First(item => item.Id == (uint)meshId.Value);
        return (install, MeshFile.Parse(big.Read(entry), (int)entry.Type), meshId.Value, entry.Name);
    }

    [Fact]
    public void Exe_animated_block_is_the_00bcfb00_record()
    {
        Assert.Equal(0x00A8FD40u, WorldShading.C3dPrimitiveSerialize);
        Assert.Equal(0x00A8E770u, WorldShading.C3dAnimatedBlockWrite);
        Assert.Equal(0x00A8E8A0u, WorldShading.C3dAnimatedBlockReadHead);
        Assert.Equal(0x00A8EB10u, WorldShading.C3dAnimatedBlockReadTail);
        Assert.Equal(23, WorldShading.C3dAnimatedInfluenceCountOffset);
        Assert.Equal(24, WorldShading.C3dAnimatedBoneListOffset);
        Assert.Equal(20, WorldShading.FatherPalskinStrideBytes);
        Assert.Equal(4u, WorldShading.FatherPalskinInitFlags);
        Assert.True(WorldShading.FatherPalskinPosIsPacked);
        Assert.Equal(3, WorldShading.PalskinVsInfluenceCount);
        Assert.Equal(0, WorldShading.PalskinGpuAddressOffset(0));
        Assert.Equal(3, WorldShading.PalskinGpuAddressOffset(1));
    }

    [Fact]
    public void Father_file_stride_and_flags_are_the_00a8fd40_fields()
    {
        var (_, mesh, _, name) = LoadCreature(RegionTravel.LiveFatherCreature);
        Assert.True(mesh.BoneCount > 0, $"{name} bones={mesh.BoneCount}");
        Assert.NotEmpty(mesh.PrimitiveReports);
        Assert.NotEmpty(mesh.PalskinSamples);
        Assert.All(mesh.PrimitiveReports.Where(p => p.AnimatedBlocks > 0), p =>
        {
            Assert.Equal(WorldShading.FatherPalskinStrideBytes, p.Stride);
            Assert.Equal(WorldShading.FatherPalskinInitFlags, p.InitFlags);
            Assert.True(p.GroupBoneCount > 0);
            Assert.Equal(p.GroupBoneCount, p.GroupBones?.Length ?? 0);
            Assert.Equal(4, MeshFile.PalskinBlendIndexOffset(1, p.Stride, p.InitFlags, true));
            Assert.Equal(8, MeshFile.PalskinBlendWeightOffset(1, p.Stride, p.InitFlags, true));
            Assert.Equal(12, MeshFile.PackedNormalOffset(1, p.Stride, p.InitFlags, true));
            Assert.Equal(16, MeshFile.PackedUvOffset(1, p.Stride, p.InitFlags, true));
        });
    }

    [Fact]
    public void Father_packed_dword_unpacks_with_primitive_scale()
    {
        var (_, mesh, _, _) = LoadCreature(RegionTravel.LiveFatherCreature);
        var s = mesh.PalskinSamples[0];
        Assert.Equal(4, s.PosSize);
        var unpacked = MeshFile.UnpackPosition(s.PosDword0, s.Scale, s.Offset);
        Assert.Equal(s.Position.X, unpacked.X, 4);
        Assert.Equal(s.Position.Y, unpacked.Y, 4);
        Assert.Equal(s.Position.Z, unpacked.Z, 4);
        Assert.Equal(s.Index0, (byte)s.IndexDword);
        Assert.Equal(s.Index1, (byte)(s.IndexDword >> 8));
        Assert.True(s.Index0 % 3 == 0, $"file blend byte {s.Index0} is the 00BCFB00 register offset");
        Assert.Equal(255, s.Weight0 + s.Weight1 + s.Weight2 + s.Weight3);
    }

    [Fact]
    public void Father_file_byte_is_register_offset_into_00bcfb00_bank()
    {
        var (_, mesh, _, _) = LoadCreature(RegionTravel.LiveFatherCreature);
        var palettes = WorldShading.FirstSeenPalettes(mesh.Bones);
        foreach (var s in mesh.PalskinSamples.Take(8))
        {
            var group = s.GroupBones ?? [];
            Assert.True(group.Length > 0, $"prim {s.Primitive} has no 00A8E770 group");
            var idx = new byte[] { s.Index0, s.Index1, s.Index2, s.Index3 };
            var wgt = new byte[] { s.Weight0, s.Weight1, s.Weight2, s.Weight3 };
            var slot0 = WorldShading.PalskinSubsetSlot(s.Index0);
            Assert.InRange(slot0, 0, group.Length - 1);
            var bone0 = WorldShading.PalskinMeshBoneFromFileIndex(s.Index0, group);
            Assert.Equal(group[slot0], bone0);
            Assert.InRange(bone0, 0, mesh.BoneCount - 1);

            var subset = WorldShading.PackSubsetRegisters(palettes, group);
            var vs = WorldShading.EvaluatePalskinVsPosition(s.Position, idx, wgt, subset);
            var host = WorldShading.SkinPosition(s.Position, idx, wgt, palettes, group);
            Assert.Equal(vs.X, host.X, 3);
            Assert.Equal(vs.Y, host.Y, 3);
            Assert.Equal(vs.Z, host.Z, 3);
            Assert.Equal(s.SkinnedPosition.X, host.X, 3);
            Assert.Equal(s.SkinnedPosition.Y, host.Y, 3);
            Assert.Equal(s.SkinnedPosition.Z, host.Z, 3);
        }
    }

    [Fact]
    public void Father_skinned_cm_through_shot2_wvp_matches_vs_dp4_c5()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var (_, mesh, _, _) = LoadCreature(RegionTravel.LiveFatherCreature);
        using var shaders = BigArchive.Open(install.ShadersBigPath);
        var bank = shaders.SubBanks.First(s => s.Name == "SHADERS_PALSKIN");
        var entry = shaders.ReadEntries(bank).First(e => e.Name == "VSHADER_PALSKIN_DIRLIGHT_FOG");
        var vs = ShaderProgram.Parse(entry.Name, bank.Name, entry.Type, shaders.Read(entry));
        Assert.True(vs.TryGetPalskinInputs(out var pIn, out var iIn, out var wIn, out var nIn, out var uvIn));
        Assert.Equal(0, pIn);
        Assert.Equal(1, iIn);
        Assert.Equal(2, wIn);
        Assert.Equal(3, nIn);
        Assert.Equal(4, uvIn);
        Assert.True(vs.TryGetPalskinA0RelativeC38());
        Assert.True(vs.TryGetOPosWvpC5C8());

        var cam = new Vector3(40.033936f, 130.47711f, 16.78288f);
        var look = new Vector3(-0.704544f, 0.6710376f, -0.23092493f);
        LandscapeFrustum.LetterboxCots(
            LandscapeFrustum.TurnsToRadians(0.2f), 4f, 3f, out var cotH, out var cotV);
        var view = LandscapeFrustum.CotScaledView(cam, look, Vector3.UnitZ, cotH, cotV);
        var proj = Dx9VulkanProjection.FirstSeenDx9Projection();
        var wvp = LandscapeFrustum.ComposeWvp(LandscapeFrustum.IdentityWorld(), view, proj);
        var palettes = WorldShading.FirstSeenPalettes(mesh.Bones);

        Assert.True(mesh.PalskinSamples.Count >= 3);
        foreach (var s in mesh.PalskinSamples.Take(6))
        {
            var idx = new byte[] { s.Index0, s.Index1, s.Index2, s.Index3 };
            var wgt = new byte[] { s.Weight0, s.Weight1, s.Weight2, s.Weight3 };
            var group = s.GroupBones ?? [];
            var subset = WorldShading.PackSubsetRegisters(palettes, group);
            var skinnedCm = WorldShading.EvaluatePalskinVsPosition(s.Position, idx, wgt, subset);
            var world = skinnedCm * WorldGeometry.MeshToWorld;
            var clip = WorldShading.EvaluatePalskinVsClip(world, wvp);
            var hostWorld = WorldShading.SkinPosition(s.Position, idx, wgt, palettes, group)
                            * WorldGeometry.MeshToWorld;
            var hostClip = WorldShading.EvaluatePalskinVsClip(hostWorld, wvp);
            Assert.Equal(clip.X, hostClip.X, 3);
            Assert.Equal(clip.Y, hostClip.Y, 3);
            Assert.Equal(clip.Z, hostClip.Z, 3);
            Assert.Equal(clip.W, hostClip.W, 3);
        }
    }

    [Fact]
    public void Kid_float3_decl_and_father_packed_decl_are_both_file_fields()
    {
        var kid = LoadCreature(RegionTravel.KidCreature).Mesh;
        var father = LoadCreature(RegionTravel.LiveFatherCreature).Mesh;
        Assert.NotEmpty(kid.PalskinSamples);
        Assert.NotEmpty(father.PalskinSamples);
        Assert.All(kid.PalskinSamples, s => Assert.Equal(WorldShading.FirstSeenPalskinStrideBytes, s.Stride));
        Assert.All(father.PalskinSamples, s => Assert.Equal(WorldShading.FatherPalskinStrideBytes, s.Stride));
        Assert.All(kid.PalskinSamples, s => Assert.Equal(12, s.PosSize));
        Assert.All(father.PalskinSamples, s => Assert.Equal(4, s.PosSize));
    }
}
