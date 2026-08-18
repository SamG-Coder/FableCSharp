using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Defs;
using Fable.Formats.Textures;
using Fable.Game;

namespace Fable.Formats.Tests;

public sealed class FrontendSpriteTests
{
    private static GameInstall Require()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        return install;
    }

    private static (GameBin Bin, BigArchive Big, IReadOnlyList<BankEntry> Entries) Open()
    {
        var install = Require();
        var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
        var bin = GameBin.Load(install.FindCompiledDef("frontend.bin")!, names);
        var big = BigArchive.Open(Path.Combine(install.DataRoot, "graphics", "pc", FrontendSpriteBank.BankFile));
        var bank = big.SubBanks.Single(item => item.Name == FrontendSpriteBank.BankName);
        return (bin, big, big.ReadEntries(bank));
    }

    [Fact]
    public void GraphicIndex_crc_is_FableCrc_of_that_name()
    {
        Assert.Equal(0x38E36902u, FrontendUiDef.GraphicIndexCrc);
        Assert.Equal(FrontendUiDef.GraphicIndexCrc, FableCrc.Hash("GraphicIndex"));
        Assert.NotEqual(FrontendUiDef.GraphicIndexCrc, FableCrc.Hash("Graphic"));
        Assert.NotEqual(FrontendUiDef.GraphicIndexCrc, FableCrc.Hash("Texture"));
        Assert.NotEqual(FrontendUiDef.GraphicIndexCrc, FableCrc.Hash("Sprite"));
        Assert.Equal(0x4323419Au, FrontendUiDef.HeightCrc);
        Assert.Equal(FrontendUiDef.HeightCrc, FableCrc.Hash("Height"));
    }

    [Fact]
    public void Press_Start_title_and_forest_bind_from_persist_bank_id()
    {
        var (bin, big, entries) = Open();
        using (big)
        {
            var title1 = FrontendUiDef.TryParse(bin.FindEntry("UI_TITLE_01")!);
            var title2 = FrontendUiDef.TryParse(bin.FindEntry("UI_TITLE_02")!);
            var forest = FrontendUiDef.TryParse(bin.FindEntry("UI_FRONTEND_BG_FORREST_1_1")!);
            var mouse = FrontendUiDef.TryParse(bin.FindEntry("UI_MOUSE_POINTER")!);
            var text = FrontendUiDef.TryParse(bin.FindEntry("UI_PRESS_START_TEXT")!);
            Assert.NotNull(title1);
            Assert.NotNull(title2);
            Assert.NotNull(forest);
            Assert.NotNull(mouse);
            Assert.NotNull(text);
            Assert.Equal(3, title1.GraphicBankId);
            Assert.Equal(4, title2.GraphicBankId);
            Assert.Equal(206, forest.GraphicBankId);
            Assert.Equal(362, mouse.GraphicBankId);
            Assert.Equal(0, text.GraphicBankId);

            var left = entries.Single(e => e.Id == (uint)title1.GraphicBankId);
            var right = entries.Single(e => e.Id == (uint)title2.GraphicBankId);
            var tile = entries.Single(e => e.Id == (uint)forest.GraphicBankId);
            var pointer = entries.Single(e => e.Id == (uint)mouse.GraphicBankId);
            Assert.Equal(FrontendSpriteBank.TitleLeft, left.Name);
            Assert.Equal(FrontendSpriteBank.TitleRight, right.Name);
            Assert.Equal("FORREST_1_1", tile.Name);
            Assert.Equal(FrontendSpriteBank.MousePointer, pointer.Name);
        }
    }

    [Fact]
    public void BankNameForWidget_uses_persist_not_a_name_map()
    {
        var install = Require();
        using var sprites = new FrontendSpriteBank(install);
        Assert.Equal(FrontendSpriteBank.TitleLeft, FrontendSpriteBank.BankNameForWidget("UI_TITLE_01"));
        Assert.Equal(FrontendSpriteBank.TitleRight, FrontendSpriteBank.BankNameForWidget("UI_TITLE_02"));
        Assert.Equal("FORREST_1_1", FrontendSpriteBank.BankNameForWidget("UI_FRONTEND_BG_FORREST_1_1"));
        Assert.Equal(FrontendSpriteBank.MousePointer, FrontendSpriteBank.BankNameForWidget("UI_MOUSE_POINTER"));
        Assert.Null(FrontendSpriteBank.BankNameForWidget("UI_TITLE"));
        Assert.Null(FrontendSpriteBank.BankNameForWidget("UI_PRESS_START_TEXT"));
        Assert.Null(FrontendSpriteBank.BankNameForWidget("UI_FRONTEND_PRESS_START_MENU"));
        Assert.Equal("FORREST_1_1", sprites.TryNameForId(206));
        Assert.Null(sprites.TryNameForId(0));
    }

    [Fact]
    public void Title_and_forest_headers_are_34_bytes_with_frame_size()
    {
        var (_, big, entries) = Open();
        using (big)
        {
            var title = entries.Single(e => e.Name == FrontendSpriteBank.TitleLeft);
            var forest = entries.Single(e => e.Name == "FORREST_1_1");
            Assert.Equal(TextureFile.HeaderBytes, title.Info.Count);
            var th = TextureFile.ReadHeader(title.Info.ToArray());
            Assert.Equal(256, th.Width);
            Assert.Equal(128, th.Height);
            Assert.Equal(256, th.FrameWidth);
            Assert.Equal(128, th.FrameHeight);
            Assert.Equal(1, th.FormatCode);
            Assert.Equal(256, BitConverter.ToUInt16(title.Info.ToArray(), TextureFile.HeaderFrameWidthOffset));
            Assert.Equal(128, BitConverter.ToUInt16(title.Info.ToArray(), TextureFile.HeaderFrameHeightOffset));
            var fh = TextureFile.ReadHeader(forest.Info.ToArray());
            Assert.Equal(256, fh.Width);
            Assert.Equal(256, fh.FrameWidth);
            Assert.Equal(31, fh.FormatCode);
            var tex = TextureFile.Parse(title.Id, title.Name, title.Type, title.Info, big.Read(title));
            Assert.Equal(TextureCompression.Rgba8, tex.Compression);
            Assert.Equal(256, tex.Width);
            Assert.Equal(128, tex.Height);
        }
    }

    [Fact]
    public void Type_0x22_packer_writes_dest_uv_texture_colour_blend()
    {
        var rec = FrontendSpriteDraw.PackTextured(
            70f, 30f, 326f, 158f,
            textureId: 3,
            u0: 0f, v0: 0f, u1: 1f, v1: 1f,
            colourB: 0xFF, colourG: 0xFF, colourR: 0xFF, colourA: 0xFF,
            blend: FrontendSpriteDraw.DefaultBlend);
        Assert.Equal(0x22, rec.RecordType);
        Assert.Equal(0x0041BEB0u, FrontendSpriteDraw.PackerFn);
        Assert.Equal(0x0041BF60u, FrontendSpriteDraw.TexturedPackerFn);
        Assert.Equal(0x00BAD8A0u, FrontendSpriteDraw.InstanceSubmitFn);
        Assert.Equal(0x00BAE2D0u, FrontendSpriteDraw.HandlerSubmitFn);
        Assert.Equal(0x009DB700u, FrontendSpriteDraw.EnqueueFn);
        Assert.Equal(0xC0, FrontendSpriteDraw.RecordBytes);
        Assert.Equal(60, FrontendSpriteDraw.EnqueueBytes);
        Assert.Equal(64, FrontendSpriteDraw.TextureOffset);
        Assert.Equal(12, FrontendSpriteDraw.DestOffset);
        Assert.Equal(68, FrontendSpriteDraw.U0Offset);
        Assert.Equal(2, FrontendSpriteDraw.DefaultBlend);
        Assert.Equal(0, rec.SizeFromFrame);
        var bytes = rec.ToRecord();
        Assert.Equal(0xC0, bytes.Length);
        Assert.Equal(0x22, BitConverter.ToInt32(bytes, 0));
        Assert.Equal(70f, BitConverter.ToSingle(bytes, 12));
        Assert.Equal(3, BitConverter.ToInt32(bytes, 64));
        Assert.Equal(1f, BitConverter.ToSingle(bytes, 76));
        Assert.Equal(2, BitConverter.ToInt32(bytes, 48));
        Assert.Equal(0, bytes[56]);
        var back = FrontendSpriteDraw.Read(bytes);
        Assert.Equal(3, back.TextureId);
        Assert.Equal(70f, back.DestX0);
        Assert.Equal(1f, back.U1);

        var bare = FrontendSpriteDraw.PackUntextured(
            0, 0, 0, 0, fontOrIndex: 224,
            0, 0, 0, 0, 0xFF, 0xFF, 0xFF, 0xFF);
        Assert.Equal(0, bare.TextureId);
        Assert.Equal(224, bare.FontOrIndex);
        Assert.Equal(0, bare.ToRecord()[64]);
    }
}
