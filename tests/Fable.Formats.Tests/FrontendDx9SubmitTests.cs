using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Shaders;
using Fable.Game;
using Fable.Render.Parity.Dx9Vulkan;

namespace Fable.Formats.Tests;

/// <summary>
/// Locks recovered Fable.exe VAs, record
/// sizes, prim types, and first-seen empty
/// vs nonempty dest. No invented FVF /
/// blend / UV flip / half-pixel.
/// </summary>
public sealed class FrontendDx9SubmitTests
{
    [Fact]
    public void Frame_wrapper_vas_match_0042DF9E()
    {
        var frame = FrontendDx9Submit.FrontendFrame();
        Assert.Equal(0x0042DF9Eu, frame.Frame);
        Assert.Equal(0x009D8CF0u, frame.Clear);
        Assert.Equal(0x009BEF20u, frame.BeginScene);
        Assert.Equal(0x009D9C80u, frame.Flush2d);
        Assert.Equal(0x009DA9F0u, frame.FlushLayers);
        Assert.Equal(1, frame.FlushLayersArg);
        Assert.Equal(0x009BEF50u, frame.EndScene);
        Assert.Equal(0x009BEEB0u, frame.Present);
        Assert.Equal(0x009BEF80u, frame.Viewport);
        Assert.True(frame.ClearBeforeBeginScene);
        Assert.Equal(2, frame.FlushPairCount);
        Assert.Equal(0xFF000000u, frame.ClearColorArgb);
        Assert.Equal(0, frame.ClearFlagsArg);
        Assert.Equal(7, frame.ClearFlagsDefault);
        Assert.Equal(0f, frame.ViewportMinZ);
        Assert.Equal(1f, frame.ViewportMaxZ);
        Assert.Null(frame.ViewportWidth);
        Assert.Null(frame.Scissor);
        Assert.Equal(0x009BEEB0u, EngineLifecycle.PresentFn);
    }

    [Fact]
    public void Sprite_record_is_type_22_size_c0()
    {
        Assert.Equal(0x22u, FrontendDx9Submit.SpriteRecordType);
        Assert.Equal(0xC0, FrontendDx9Submit.SpriteRecordBytes);
        Assert.Equal(0x15C, FrontendDx9Submit.SpriteSubmitDestOffset);
        Assert.Equal(92, FrontendDx9Submit.SpriteSubmitVtbl);
        Assert.Equal(0x0041BEB0u, FrontendDx9Submit.PackerFn);
        Assert.Equal(0x00B23BC0u, FrontendDx9Submit.EngineSubmitFn);
        Assert.Equal(0x00B324A0u, FrontendDx9Submit.DispatchFn);
        Assert.Equal(0x00BACFD0u, FrontendDx9Submit.SpriteFactoryFn);
        Assert.Equal(0x00BAD8A0u, FrontendDx9Submit.SpriteInstanceFn);
        Assert.Equal(0x00BAE2D0u, FrontendDx9Submit.SpriteDrawFn);
        Assert.Equal(0x012A54BCu, FrontendDx9Submit.SpriteInstanceVtbl);
        Assert.Equal(0x012A5664u, FrontendDx9Submit.SpriteHandlerVtbl);
        Assert.Equal(0x8C, FrontendDx9Submit.SpriteInstanceBytes);
        Assert.Equal(Dx9FrontendState.SpriteRecordType, FrontendDx9Submit.SpriteRecordType);
        Assert.Equal(Dx9FrontendState.SpriteRecordBytes, FrontendDx9Submit.SpriteRecordBytes);
    }

    [Fact]
    public void First_seen_dest_zero_does_not_enqueue_or_dip()
    {
        var rec = FrontendDx9Submit.FirstSeenEmptyDest();
        Assert.True(rec.DestEmpty);
        Assert.Equal(0f, rec.DestX0);
        Assert.Equal(0f, rec.DestY0);
        Assert.Equal(0f, rec.DestX1);
        Assert.Equal(0f, rec.DestY1);
        Assert.Equal(0f, rec.U0);
        Assert.Equal(0f, rec.V0);
        Assert.Equal(0f, rec.U1);
        Assert.Equal(0f, rec.V1);
        Assert.Equal(0x0041BEB0u, rec.Packer);
        Assert.Equal(0x00BACFD0u, rec.Factory);
        Assert.Equal(0x00BAD8A0u, rec.InstanceSubmit);
        Assert.Equal(0x00BAE2D0u, rec.HandlerDraw);
        Assert.False(rec.EnqueuesDisplayQueue);
        Assert.False(rec.CallsDraw);
        Assert.Equal(0, rec.TextureId);
        Assert.Null(rec.Rhw);
        Assert.Null(rec.HalfPixel);
        Assert.Null(rec.SamplerMag);
        Assert.Null(rec.AlphaTest);
        Assert.Null(rec.DiffuseArgb);
        Assert.False(FrontendDx9Submit.DisplayFlushShouldDip(0, 0));
    }

    [Fact]
    public void Nonempty_dest_draws_via_00BAE2D0_not_009DB700()
    {
        var rec = FrontendDx9Submit.NonemptyDest(10, 20, 110, 70);
        Assert.False(rec.DestEmpty);
        Assert.Equal(10f, rec.DestX0);
        Assert.Equal(20f, rec.DestY0);
        Assert.Equal(110f, rec.DestX1);
        Assert.Equal(70f, rec.DestY1);
        Assert.Equal(0x00BAD8A0u, rec.InstanceSubmit);
        Assert.Equal(0x00BAE2D0u, rec.HandlerDraw);
        Assert.False(rec.EnqueuesDisplayQueue);
        Assert.True(rec.CallsDraw);
        Assert.Null(rec.TextureId);
        Assert.Equal(0x009DB700u, FrontendDx9Submit.DisplayEnqueueFn);
        Assert.Equal(0x009DBFF0u, FrontendDx9Submit.DisplayEnqueueWrapFn);
        Assert.Equal(0x009DD8F0u, FrontendDx9Submit.HudStringEnqueueFn);
        Assert.NotEqual(FrontendDx9Submit.DisplayEnqueueFn, rec.InstanceSubmit);
    }

    [Fact]
    public void Display_queue_is_60_bytes_prim_2_or_4()
    {
        Assert.Equal(60, FrontendDx9Submit.DisplayQueueRecordBytes);
        Assert.Equal(16020, FrontendDx9Submit.DisplayQueueBeginOffset);
        Assert.Equal(16024, FrontendDx9Submit.DisplayQueueEndOffset);
        Assert.Equal(0x88888889u, FrontendDx9Submit.DisplayQueueCountMagic);
        Assert.Equal(3, FrontendDx9Submit.DisplayQueueCount(0, 180));
        Assert.True(FrontendDx9Submit.DisplayFlushShouldDip(0, 60));
        Assert.False(FrontendDx9Submit.DisplayFlushShouldDip(0, 0));
        Assert.Equal(2, Dx9FrontendState.DisplayQueuePrimA);
        Assert.Equal(4, Dx9FrontendState.DisplayQueuePrimB);
        Assert.Equal(2, Dx9FrontendState.DisplayFlushPrimitive(true));
        Assert.Equal(4, Dx9FrontendState.DisplayFlushPrimitive(false));
        Assert.Equal(32, Dx9FrontendState.DisplayVertexStride);
        Assert.Equal(332, Dx9FrontendState.DrawPrimitiveUpVtbl);
        Assert.Equal(336, Dx9FrontendState.DrawIndexedPrimitiveUpVtbl);
        Assert.Equal(324, Dx9FrontendState.DrawPrimitiveVtbl);
        Assert.Equal(101, Dx9FrontendState.D3dfmtIndex16);
    }

    [Fact]
    public void Sprite_draw_is_triangle_list_dipup_stride_32()
    {
        Assert.Equal(4, Dx9FrontendState.SpritePrimitiveType);
        Assert.Equal(4, Dx9FrontendState.D3dptTriangleList);
        Assert.NotEqual(5, Dx9FrontendState.SpritePrimitiveType);
        Assert.Equal(32, Dx9FrontendState.SpriteVertexStride);
        Assert.Equal(28, Dx9FrontendState.SpriteUsedBytes);
        Assert.Equal(0x00A0AEA0u, FrontendDx9Submit.SpriteDipUpFn);
        Assert.Equal(0x00A058C0u, FrontendDx9Submit.StateFlushFn);
        Assert.Equal(2, Dx9FrontendState.SpriteTextureStages);
    }

    [Fact]
    public void Type6_glyph_is_type_27_size_64_stride_28()
    {
        var glyph = FrontendDx9Submit.Type6Glyph();
        Assert.Equal(0x27u, glyph.RecordType);
        Assert.Equal(64, glyph.RecordBytes);
        Assert.Equal(0x0054EF00u, glyph.WidgetDraw);
        Assert.Equal(0x00543910u, glyph.Packer);
        Assert.Equal(0x00AB7C20u, glyph.FaceDraw);
        Assert.Equal(0x00A0ABE0u, glyph.Primitive);
        Assert.Equal(28, glyph.VertexStride);
        Assert.Equal(4, glyph.PrimitiveType);
        Assert.Equal(0.5f, glyph.HalfPixel);
        Assert.Equal(1f, glyph.Rhw);
        Assert.Equal("ENG_ARIAL_16", glyph.FaceName);
        Assert.Null(glyph.Fvf);
        Assert.Null(glyph.SamplerMag);
        Assert.Null(glyph.AlphaTest);
        Assert.Equal(Dx9FrontendState.GlyphRecordType, glyph.RecordType);
        Assert.Equal(Dx9FrontendState.GlyphVertexStride, glyph.VertexStride);
        Assert.Equal(Dx9FrontendState.GlyphHalfPixel, glyph.HalfPixel);
        Assert.Equal(0x0122F59Cu, Dx9FrontendState.HalfPixelVa);
    }

    [Fact]
    public void Device_vtbl_slots_are_byte_offsets()
    {
        Assert.Equal(164, Dx9FrontendState.BeginSceneVtbl);
        Assert.Equal(168, Dx9FrontendState.EndSceneVtbl);
        Assert.Equal(172, Dx9FrontendState.ClearVtbl);
        Assert.Equal(68, Dx9FrontendState.PresentVtbl);
        Assert.Equal(188, Dx9FrontendState.SetViewportVtbl);
        Assert.Equal(260, Dx9FrontendState.SetTextureVtbl);
    }

    [Fact]
    public void Default_widget_blend_is_src_alpha_from_handler_plus_164()
    {
        Assert.Equal(164, Dx9FrontendState.HandlerBlendOffset);
        Assert.Equal(2, Dx9FrontendState.WidgetBlendDefault);
        var (src, dst) = Dx9FrontendState.BlendFromHandlerMode(2);
        Assert.Equal(5, src);
        Assert.Equal(6, dst);
        var add = Dx9FrontendState.BlendFromHandlerMode(3);
        Assert.Equal(2, add.Src);
        Assert.Equal(2, add.Dst);
        var inv = Dx9FrontendState.BlendFromHandlerMode(4);
        Assert.Equal(2, inv.Src);
        Assert.Equal(4, inv.Dst);
        Assert.Equal(0x01396F78u, Dx9FrontendState.BlendTableSrcAlpha);
        Assert.Equal(0x01396F7Cu, Dx9FrontendState.BlendTableInvSrcAlpha);
    }

    [Fact]
    public void Shader_semantics_are_opos_v0_od0_v1_ot0_v2()
    {
        Assert.Equal("VSHADER_2D_SPRITE", Dx9FrontendState.VertexShader);
        Assert.Equal("SHADERS_POINT_SPRITE1", Dx9FrontendState.VertexShaderBank);
        Assert.Equal("PSHADER_2D_CLOCK_SPRITE", Dx9FrontendState.PixelShader);
        Assert.Equal("VSHADER_BBBLIB_2D", Dx9FrontendState.DisplayQueueVertexShader);
        Assert.Equal(0, Dx9FrontendState.VsPositionInput);
        Assert.Equal(1, Dx9FrontendState.VsDiffuseInput);
        Assert.Equal(2, Dx9FrontendState.VsTexcoordInput);
        Assert.True(Dx9FrontendState.VsPassthroughOPos);
        Assert.Equal(0x144u, Dx9FrontendState.InferredFvfXyzRhwDiffuseTex1);
    }

    [Fact]
    public void Vshader_2d_sprite_listing_is_mov_opos_v0()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var big = BigArchive.Open(install.ShadersBigPath);
        ShaderProgram? vs = null;
        ShaderProgram? ps = null;
        foreach (var bank in big.SubBanks)
        {
            foreach (var entry in big.ReadEntries(bank))
            {
                if (entry.Name == "VSHADER_2D_SPRITE")
                    vs = ShaderProgram.Parse(entry.Name, bank.Name, entry.Type, big.Read(entry));
                if (entry.Name == "PSHADER_2D_CLOCK_SPRITE")
                    ps = ShaderProgram.Parse(entry.Name, bank.Name, entry.Type, big.Read(entry));
            }
        }

        Assert.NotNull(vs);
        Assert.Equal("vs_1_1", vs.Profile);
        Assert.Equal("SHADERS_POINT_SPRITE1", vs.Bank);
        var listing = vs.ToListing();
        Assert.Contains("mov oPos, v0", listing, StringComparison.Ordinal);
        Assert.Contains("mov oT0, v2", listing, StringComparison.Ordinal);
        Assert.Contains("mov oD0, v1", listing, StringComparison.Ordinal);
        Assert.DoesNotContain("dp4 oPos", listing, StringComparison.Ordinal);
        Assert.NotNull(ps);
        Assert.Contains("mul r0, t0, c0", ps.ToListing(), StringComparison.Ordinal);
    }
}
