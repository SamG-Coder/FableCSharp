using System.Numerics;
using System.Text;
using Fable.Core;
using Fable.Formats;
using Fable.Formats.Anims;
using Fable.Formats.Banks;
using Fable.Formats.Meshes;
using Fable.Game;
using Fable.Game.Scripting;

namespace Fable.Formats.Tests;

/// <summary>
/// Type-6 <c>3DAF</c>/<c>XSEQ</c> first-key sample into
/// <c>PaletteForPose</c>. Time interpolation unread.
/// </summary>
public sealed class XSeqFormatTests
{
    [Fact]
    public void Xseq_persist_addrs_match_00a999b0_and_00aa4680()
    {
        Assert.Equal(0x00A999B0u, XSeqFile.Ctor3Daf);
        Assert.Equal(0x00AA4680u, XSeqFile.CtorXseq);
        Assert.Equal(0x0129E194u, XSeqFile.XseqVtbl);
        Assert.Equal(0x00A4C5E0u, XSeqFile.UnpackFn);
        Assert.Equal(0x00A4CDD0u, XSeqFile.PersistLoadFn);
        Assert.Equal(0x00A4EFC0u, XSeqFile.CompressFn);
        Assert.Equal(0x00AAF1E0u, XSeqFile.LocalCopyFn);
        Assert.Equal(0x00AA0090u, XSeqFile.HierarchyFn);
        Assert.Equal(0x00A52650u, WorldShading.TimeToKeyFn);
        Assert.Equal(80, WorldShading.ClipRateOffset);
        Assert.Equal(84, WorldShading.ClipWrapOffset);
        Assert.Equal(0, WorldShading.TimeToKey(0f, 15f, 8).Key);
        Assert.Equal(0f, WorldShading.TimeToKey(0f, 15f, 8).Frac, 5);
        var mid = WorldShading.TimeToKey(0.1f, 15f, 30);
        Assert.Equal(1, mid.Key);
        Assert.InRange(mid.Frac, 0.4f, 0.6f);
        Assert.Equal(0, WorldShading.TimeToKey(2f, 15f, 15).Key);
        Assert.Equal(44, XSeqFile.ClipRecordBytes);
        Assert.Equal(MeshFile.BoneLocalBytes, XSeqFile.BoneLocalBytes);
        Assert.False(WorldShading.FirstSeenPlaysAnim);
        Assert.False(RegionTravel.FirstSeenPlayAnimationAppliesPose);
        Assert.True(WorldShading.NativeXseqRotationIsSlerp);
        Assert.False(WorldShading.FirstSeenXseqAppliesFrac);
        Assert.Equal(0x00A4C1F0u, WorldShading.XseqSlerpFn);
    }

    [Fact]
    public void Synthetic_xseq_first_key_changes_palette()
    {
        var clip = XSeqFile.Parse(BuildSynthetic(), "SYNTH_TURN");
        Assert.Contains("Pelvis", clip.BoneNames);
        Assert.True(clip.Tracks[0].HasRotation);
        var bones = new[]
        {
            new MeshBone(
                "Pelvis", -1, 0, 0, Matrix4x4.Identity,
                Quaternion.Identity, Vector3.Zero, Vector3.One),
        };
        var bind = WorldShading.FirstSeenPalettes(bones);
        var posed = WorldShading.PaletteForPose(bones, clip, 0f);
        Assert.False(WorldShading.IsNearIdentity(posed[0]) &&
                     WorldShading.IsNearIdentity(bind[0]) &&
                     posed[0] == bind[0]);
        Assert.True(
            Math.Abs(posed[0].M11 - bind[0].M11) > 1e-3f ||
            Math.Abs(posed[0].M12 - bind[0].M12) > 1e-3f ||
            Math.Abs(posed[0].M21 - bind[0].M21) > 1e-3f,
            "sampled local must move dest=S*C3D off bind");
    }

    [Fact]
    public void Wake_loop_3420_is_lzo_3daf_with_xseq_tracks()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var bank = new MeshBank();
        bank.Open(install);
        var clip = bank.GetAnim(XSeqFile.WakeLoopId);
        Assert.NotNull(clip);
        Assert.Equal(XSeqFile.WakeLoopName, clip.Name);
        Assert.True(clip.WasCompressed, "3420 first dword is uncompressed size, not >>>>");
        Assert.True(clip.UncompressedSize > 16600, $"unc={clip.UncompressedSize}");
        Assert.Contains(clip.Chunks, c => c.FourCc == XSeqFile.FourCcAnrt);
        Assert.Contains(clip.Chunks, c => c.FourCc == XSeqFile.FourCcAobj);
        Assert.Contains(clip.Chunks, c => c.FourCc == XSeqFile.FourCcXseq);
        Assert.True(clip.Tracks.Count > 0, "XSEQ tracks");
        Assert.Contains(clip.BoneNames, n =>
            n.Contains("Scene Root", StringComparison.OrdinalIgnoreCase) ||
            n.Contains("Pelvis", StringComparison.OrdinalIgnoreCase) ||
            n.Contains("Bip01", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Wake_loop_3420_palette_differs_from_kid_bind()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var graphics = Path.Combine(install.DataRoot, "graphics", "graphics.big");
        using var big = BigArchive.Open(graphics);
        var meshBank = big.SubBanks.First(b =>
            b.Name.Contains("MESH", StringComparison.OrdinalIgnoreCase));
        var entries = big.ReadEntries(meshBank);
        var kid = MeshFile.Parse(
            big.Read(entries.First(e => e.Id == 4300)), 1);
        using var bank = new MeshBank();
        bank.Open(install);
        var clip = bank.GetAnim(XSeqFile.WakeLoopId);
        Assert.NotNull(clip);
        Assert.True(kid.BoneCount > 0);
        var bind = WorldShading.FirstSeenPalettes(kid.Bones);
        var posed = WorldShading.PaletteForPose(kid.Bones, clip, 0f);
        Assert.Equal(bind.Length, posed.Length);
        var changed = 0;
        for (var i = 0; i < bind.Length; i++)
        {
            if (Math.Abs(bind[i].M11 - posed[i].M11) > 1e-4f ||
                Math.Abs(bind[i].M12 - posed[i].M12) > 1e-4f ||
                Math.Abs(bind[i].M14 - posed[i].M14) > 1e-3f ||
                Math.Abs(bind[i].M24 - posed[i].M24) > 1e-3f ||
                Math.Abs(bind[i].M34 - posed[i].M34) > 1e-3f)
            {
                changed++;
            }
        }

        Assert.True(changed > 0,
            $"3420 first keys must move at least one PALSKIN dest. tracks={clip.Tracks.Count} names={string.Join(',', clip.BoneNames.Take(8))}");
        var bindTris = kid.TrianglesForPose();
        var posedTris = kid.TrianglesForPose(clip);
        Assert.Equal(kid.Triangles.Count, bindTris.Count);
        Assert.True(posedTris.Count > 0);
        var moved = 0;
        var n = Math.Min(bindTris.Count, posedTris.Count);
        for (var i = 0; i < n; i++)
        {
            if ((bindTris[i].A - posedTris[i].A).LengthSquared() > 1e-4f ||
                (bindTris[i].B - posedTris[i].B).LengthSquared() > 1e-4f)
                moved++;
        }

        Assert.True(moved > 0, "3420 first-key must move PALSKIN triangles off bind");
        Assert.False(RegionTravel.FirstSeenPlayAnimationAppliesPose);
    }

    [Fact]
    public void PlayAnimation_named_clip_feeds_palette_sample()
    {
        var clip = XSeqFile.Parse(BuildSynthetic(), "CS_TIRED");
        var runtime = ScriptRuntime.Detached();
        runtime.Animation.Clips["CS_TIRED"] =
            new AnimationClipRecord("CS_TIRED", clip.Duration, clip);
        var interp = new ScriptInterpreter("pose",
            ["HERO.PlayAnimation CS_TIRED,FALSE,FALSE,TRUE,FALSE"]);
        interp.RunUntilYield(runtime);
        Assert.Equal("CS_TIRED", runtime.Animation.States["HERO"].ClipKey);
        var bones = new[]
        {
            new MeshBone(
                "Pelvis", -1, 0, 0, Matrix4x4.Identity,
                Quaternion.Identity, Vector3.Zero, Vector3.One),
        };
        var posed = WorldShading.PaletteForPose(
            bones, runtime.Animation.States["HERO"].ClipKey, 0f,
            runtime.Animation.LookupClip("CS_TIRED").Sequence);
        var bind = WorldShading.FirstSeenPalettes(bones);
        Assert.True(
            Math.Abs(posed[0].M11 - bind[0].M11) > 1e-3f ||
            Math.Abs(posed[0].M12 - bind[0].M12) > 1e-3f);
    }

    private static byte[] BuildSynthetic()
    {
        var xseq = new MemoryStream();
        WriteI32(xseq, 2);
        WriteI32(xseq, -1);
        WriteCString(xseq, "Pelvis");
        xseq.WriteByte(1);
        WriteF32(xseq, 15f);
        WriteU32(xseq, 2);
        xseq.Write(new byte[4]);
        WriteF32(xseq, 1f);
        WriteF32(xseq, 1f);
        WriteU16(xseq, 1);
        WriteF32(xseq, 0f);
        WriteF32(xseq, 0.70710677f);
        WriteF32(xseq, 0f);
        WriteF32(xseq, 0.70710677f);
        WriteU16(xseq, 0);
        WriteU16(xseq, 1);
        WriteI16(xseq, 0);
        WriteI16(xseq, 100);
        WriteI16(xseq, 0);
        WriteU16(xseq, 0);
        var xseqBytes = xseq.ToArray();

        var aobj = new MemoryStream();
        WriteCString(aobj, "Hero");
        WriteChunk(aobj, XSeqFile.FourCcXseq, xseqBytes);
        var aobjBytes = aobj.ToArray();

        var anrt = new MemoryStream();
        anrt.WriteByte(1);
        WriteF32(anrt, 2f);
        WriteChunk(anrt, XSeqFile.FourCcAobj, aobjBytes);
        var anrtBytes = anrt.ToArray();

        var body = new MemoryStream();
        WriteU32(body, XSeqFile.UncompressedMarker);
        WriteU32(body, XSeqFile.FourCc3Daf);
        WriteU32(body, 100);
        WriteCString(body, "Copyright Big Blue Box Studios Ltd.");
        while (body.Length % 4 != 0)
            body.WriteByte(0);
        WriteChunk(body, XSeqFile.FourCcAnrt, anrtBytes);
        return body.ToArray();
    }

    private static void WriteChunk(Stream s, uint four, byte[] payload)
    {
        WriteU32(s, four);
        WriteU32(s, (uint)payload.Length);
        s.Write(payload);
    }

    private static void WriteCString(Stream s, string text)
    {
        s.Write(Encoding.ASCII.GetBytes(text));
        s.WriteByte(0);
    }

    private static void WriteU32(Stream s, uint value)
    {
        s.Write(BitConverter.GetBytes(value));
    }

    private static void WriteI32(Stream s, int value) =>
        s.Write(BitConverter.GetBytes(value));

    private static void WriteF32(Stream s, float value) =>
        s.Write(BitConverter.GetBytes(value));

    private static void WriteU16(Stream s, ushort value) =>
        s.Write(BitConverter.GetBytes(value));

    private static void WriteI16(Stream s, short value) =>
        s.Write(BitConverter.GetBytes(value));
}
