using System.Runtime.InteropServices;
using Fable.Core;
using Fable.Formats.Scene;
using Fable.Formats.Textures;
using Fable.Game;
using Fable.Render;
using Fable.Render.Parity.Dx9Vulkan;
using Silk.NET.Vulkan;

namespace Fable.Formats.Tests;

/// <summary>
/// DX9 → Vulkan frontend table. Goldens
/// are recovered exe immediates / shader
/// tokens, not screenshots.
/// </summary>
public sealed class Dx9VulkanFrontendTests
{
    [Fact]
    public void Frontend_batch_reuses_exact_size_arrays_between_frames()
    {
        var rec = new FrontendDx9DrawRecord(
            0, 0, 32, 32, 0, 0, 1, 1, 0xFFFFFFFF, 0, 2);
        var first = Dx9VulkanFrontend.BuildBatch([rec], [GpuTexture.White()]);
        var second = Dx9VulkanFrontend.BuildBatch(
            [rec with { DestX0 = 1, DestX1 = 33 }], [GpuTexture.White()],
            reuse: first);

        Assert.Same(first.Vertices, second.Vertices);
        Assert.Same(first.Indices, second.Indices);
        Assert.Same(first.Draws, second.Draws);
        Assert.Same(first.Textures, second.Textures);
        Assert.NotEqual(first.Vertices[0].Position, first.Vertices[1].Position);
    }

    [Fact]
    public void Reused_frontend_batch_build_has_no_managed_allocations()
    {
        FrontendDx9DrawRecord[] records =
        [
            new(0, 0, 32, 32, 0, 0, 1, 1, 0xFFFFFFFF, 0, 2),
            new(32, 0, 64, 32, 0, 0, 1, 1, 0xFFFFFFFF, 0, 2),
        ];
        GpuTexture[] textures = [GpuTexture.White()];
        var batch = Dx9VulkanFrontend.BuildBatch(records, textures);
        batch = Dx9VulkanFrontend.BuildBatch(records, textures, reuse: batch);

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 32; i++)
            batch = Dx9VulkanFrontend.BuildBatch(records, textures, reuse: batch);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0, allocated);
    }

    [Fact]
    public void Persist_quarter_turn_rotates_sprite_corners_without_allocating_geometry()
    {
        var rec = new FrontendDx9DrawRecord(
            0, 0, 20, 10, 0, 0, 1, 1, 0xFFFFFFFF, 0, 2,
            Angle: -0.25f);
        var batch = Dx9VulkanFrontend.BuildBatch(
            [rec], [GpuTexture.White()], vpW: 100, vpH: 100);

        Assert.Equal(4, batch.Vertices.Length);
        Assert.Equal(-0.7f, batch.Vertices[0].Position.X, 4); // (15, -5)
        Assert.Equal(-1.1f, batch.Vertices[0].Position.Y, 4);
        Assert.Equal(-0.7f, batch.Vertices[1].Position.X, 4); // (15, 15)
        Assert.Equal(-0.7f, batch.Vertices[1].Position.Y, 4);
        Assert.Equal(-0.9f, batch.Vertices[2].Position.X, 4); // (5, -5)
        Assert.Equal(-1.1f, batch.Vertices[2].Position.Y, 4);

        var liveQuad = Dx9VulkanFrontend.BuildDx9Quad(rec);
        Assert.Equal((15f, -5f), (liveQuad[0].X, liveQuad[0].Y));
        Assert.Equal((15f, 15f), (liveQuad[1].X, liveQuad[1].Y));
        Assert.Equal((5f, -5f), (liveQuad[2].X, liveQuad[2].Y));
    }

    [Fact]
    public void Frontend_texture_sets_reuse_cached_pixel_storage()
    {
        var pixels = new byte[] { 1, 2, 3, 4 };
        GpuTexture[] first = [new(7, 1, 1, pixels)];
        GpuTexture[] rebuilt = [new(7, 1, 1, pixels)];
        GpuTexture[] changed = [new(7, 1, 1, [1, 2, 3, 4])];

        Assert.True(VulkanLineRenderer.TextureSetsShareStorage(first, rebuilt));
        Assert.False(VulkanLineRenderer.TextureSetsShareStorage(first, changed));
    }

    [Fact]
    public void Primitive_topology_is_triangle_list_from_00A0AEA0()
    {
        Assert.Equal(4, Dx9VulkanFrontend.D3dptTriangleList);
        Assert.Equal(2, Dx9VulkanFrontend.D3dptLineList);
        Assert.Equal(PrimitiveTopology.TriangleList,
            Dx9VulkanFrontend.MapPrimitive(Dx9VulkanFrontend.D3dptTriangleList));
        Assert.Equal(PrimitiveTopology.LineList,
            Dx9VulkanFrontend.MapPrimitive(Dx9VulkanFrontend.D3dptLineList));
        Assert.Equal(PrimitiveTopology.TriangleList, Dx9VulkanFrontend.FrontendTopology);
        Assert.Equal(336, Dx9VulkanFrontend.DrawIndexedPrimitiveUpVtbl);
        Assert.Equal(332, Dx9VulkanFrontend.DrawPrimitiveUpVtbl);
        Assert.Equal(101, Dx9VulkanFrontend.D3dfmtIndex16);
    }

    [Fact]
    public void Display_queue_prim_2_or_4_matches_009DA9F0()
    {
        Assert.Equal(2, Dx9VulkanFrontend.DisplayQueuePrimList);
        Assert.Equal(4, Dx9VulkanFrontend.DisplayQueuePrimTris);
        Assert.Equal(PrimitiveTopology.LineList,
            Dx9VulkanFrontend.MapPrimitive(Dx9VulkanFrontend.DisplayQueuePrimList));
        Assert.Equal(PrimitiveTopology.TriangleList,
            Dx9VulkanFrontend.MapPrimitive(Dx9VulkanFrontend.DisplayQueuePrimTris));
    }

    [Fact]
    public void Default_widget_blend_is_src_alpha_inv_src_alpha()
    {
        Assert.Equal(2, Dx9VulkanFrontend.WidgetBlendDefault);
        var (src, dst) = Dx9VulkanFrontend.BlendFromHandlerMode(
            Dx9VulkanFrontend.WidgetBlendDefault);
        Assert.Equal(5, src);
        Assert.Equal(6, dst);
        Assert.Equal(D3dDeviceState.BlendSrcAlpha, src);
        Assert.Equal(D3dDeviceState.BlendInvSrcAlpha, dst);

        var vk = Dx9VulkanFrontend.DefaultSpriteBlend;
        Assert.True(vk.BlendEnable);
        Assert.Equal(BlendFactor.SrcAlpha, vk.SrcColorBlendFactor);
        Assert.Equal(BlendFactor.OneMinusSrcAlpha, vk.DstColorBlendFactor);
        Assert.Equal(Dx9VulkanBlendState.FirstSeenBlendOp, vk.ColorBlendOp);
    }

    [Fact]
    public void Handler_blend_3_and_4_use_blend_table()
    {
        var add = Dx9VulkanFrontend.BlendFromHandlerMode(3);
        Assert.Equal(2, add.Src);
        Assert.Equal(2, add.Dst);
        var addVk = Dx9VulkanFrontend.MapBlend(3);
        Assert.Equal(BlendFactor.One, addVk.SrcColorBlendFactor);
        Assert.Equal(BlendFactor.One, addVk.DstColorBlendFactor);

        var inv = Dx9VulkanFrontend.BlendFromHandlerMode(4);
        Assert.Equal(2, inv.Src);
        Assert.Equal(4, inv.Dst);
        var invVk = Dx9VulkanFrontend.MapBlend(4);
        Assert.Equal(BlendFactor.One, invVk.SrcColorBlendFactor);
        Assert.Equal(BlendFactor.OneMinusSrcColor, invVk.DstColorBlendFactor);
    }

    [Fact]
    public void Viewport_is_009BEF80_1024x768_minz0_maxz1()
    {
        var vp = Dx9VulkanFrontend.FirstSeenViewport;
        Assert.Equal(0, vp.X);
        Assert.Equal(0, vp.Y);
        Assert.Equal(1024, vp.Width);
        Assert.Equal(768, vp.Height);
        Assert.Equal(0f, vp.MinDepth);
        Assert.Equal(1f, vp.MaxDepth);
        Assert.Equal(0x009BEF80u, Dx9VulkanFrontend.SetViewportFn);
        Assert.Equal(188, Dx9VulkanFrontend.SetViewportVtbl);
    }

    [Fact]
    public void Vertex_layout_is_xyzrhw_diffuse_tex1_stride_32()
    {
        Assert.Equal(32u, FrontendDx9Vertex.Stride);
        Assert.Equal(32, Dx9VulkanFrontend.VertexStride);
        Assert.Equal(32, Dx9VulkanFrontend.NativeVertexSize);
        Assert.Equal(32, Marshal.SizeOf<FrontendDx9Vertex>());
        Assert.Equal(0x144u, FrontendDx9Vertex.FvfXyzRhwDiffuseTex1);
        Assert.Equal(12u, FrontendDx9Vertex.RhwOffset);
        Assert.Equal(16u, FrontendDx9Vertex.DiffuseOffset);
        Assert.Equal(20u, FrontendDx9Vertex.UvOffset);
        Assert.Equal(28u, FrontendDx9Vertex.NativeUsedBytes);
        Assert.Equal(0, Dx9VulkanFrontend.VsPositionInput);
        Assert.Equal(1, Dx9VulkanFrontend.VsDiffuseInput);
        Assert.Equal(2, Dx9VulkanFrontend.VsTexcoordInput);
        Assert.True(Dx9VulkanFrontend.VsPassthroughOPos);
        Assert.Equal(1f, Dx9VulkanFrontend.RecoveredRhw);
    }

    [Fact]
    public void Dest_full_viewport_maps_to_vulkan_ndc()
    {
        var tl = Dx9VulkanFrontend.DestPixelToVulkanNdc(0, 0, 0, 0, 0, 1024, 768);
        var br = Dx9VulkanFrontend.DestPixelToVulkanNdc(1024, 768, 0, 0, 0, 1024, 768);
        Assert.Equal(-1f, tl.X, 5);
        Assert.Equal(-1f, tl.Y, 5);
        Assert.Equal(1f, br.X, 5);
        Assert.Equal(1f, br.Y, 5);
        Assert.Equal(1f, tl.W, 5);

        var dx9Tl = Dx9VulkanFrontend.DestPixelToDx9Clip(0, 0, 0, 0, 0, 1024, 768);
        Assert.Equal(-1f, dx9Tl.X, 5);
        Assert.Equal(1f, dx9Tl.Y, 5);
        Assert.Equal(-1f, Dx9VulkanProjection.NdcYSign);
    }

    [Fact]
    public void Half_pixel_and_uv_flip_are_not_invented()
    {
        Assert.False(Dx9VulkanFrontend.AppliesHalfPixelOffset);
        Assert.False(Dx9VulkanFrontend.FlipsUvV);
        Assert.True(Dx9VulkanFrontend.UvVZeroAtDestTop);
        Assert.False(Dx9VulkanFrontend.AppliesScissor);
        Assert.False(Dx9VulkanFrontend.FirstSeenAlphaTest);
        Assert.Equal(1f, Dx9VulkanFrontend.HudNdcBias);
        Assert.Equal(0x0122DED8u, Dx9VulkanFrontend.HudNdcBiasVa);
    }

    [Fact]
    public void Shader_names_match_shaders_big()
    {
        Assert.Equal("VSHADER_2D_SPRITE", Dx9VulkanFrontend.VertexShaderName);
        Assert.Equal("SHADERS_POINT_SPRITE1", Dx9VulkanFrontend.VertexShaderBank);
        Assert.Equal("PSHADER_2D_CLOCK_SPRITE", Dx9VulkanFrontend.PixelShaderName);
        Assert.Equal("PIXEL_SHADERS", Dx9VulkanFrontend.PixelShaderBank);
        Assert.True(Dx9VulkanFrontend.PixelShaderC0TemporaryWhite);
        Assert.Equal(32, Dx9VulkanFrontend.PixelShaderC0Slot);
        Assert.Equal(0x00BAD040u, Dx9VulkanFrontend.HandlerCtorFn);
        Assert.Equal(0x00BAE2D0u, Dx9VulkanFrontend.SpriteSubmitFn);
        Assert.Equal(0x00A058C0u, Dx9VulkanFrontend.StateFlushFn);
        Assert.Equal(0x009DB700u, Dx9VulkanFrontend.EnqueueFn);
        Assert.Equal(164, Dx9VulkanFrontend.HandlerBlendOffset);
        Assert.False(Dx9VulkanFrontend.AppliesScissor);
    }

    [Fact]
    public void Build_batch_emits_indexed_quad_not_cpu_bitmap()
    {
        var rec = new FrontendDx9DrawRecord(
            10, 20, 110, 70,
            0, 0, 1, 1,
            0xFFFFFFFF, 0, 2);
        var batch = Dx9VulkanFrontend.BuildBatch(
            [rec], [GpuTexture.White()]);
        Assert.False(batch.IsEmpty);
        Assert.Equal(4, batch.Vertices.Length);
        Assert.Equal(6, batch.Indices.Length);
        Assert.Single(batch.Draws);
        var draw = batch.Draws[0];
        Assert.Equal(4, Dx9VulkanFrontend.D3dptTriangleList);
        Assert.Equal(Dx9VulkanFrontend.D3dptTriangleList, draw.D3dPrimitiveType);
        Assert.Equal(5, draw.D3dSrcBlend);
        Assert.Equal(6, draw.D3dDestBlend);
        Assert.True(draw.BlendEnable);
        Assert.Equal(4u, draw.VertexCount);
        Assert.Equal(6u, draw.IndexCount);
        Assert.Equal(new ushort[] { 0, 1, 2, 1, 3, 2 }, batch.Indices);
        Assert.Equal(1024, batch.ViewportWidth);
        Assert.Equal(768, batch.ViewportHeight);

        Assert.Equal(0f, rec.V0);
        Assert.True(batch.Vertices[0].Uv.Y <= batch.Vertices[2].Uv.Y);
        Assert.True(batch.Vertices[0].Position.Y < batch.Vertices[2].Position.Y);
    }

    [Fact]
    public void Sprite_uv_corners_are_tl_tr_bl_br()
    {
        var rec = new FrontendDx9DrawRecord(
            10, 20, 110, 70,
            0.1f, 0.2f, 0.8f, 0.9f,
            0xFFFFFFFF, 0, 2);
        var verts = Dx9VulkanFrontend.BuildDx9Quad(rec);
        Assert.Equal(4, verts.Length);
        Assert.Equal(10f, verts[0].X);
        Assert.Equal(20f, verts[0].Y);
        Assert.Equal(0.1f, verts[0].U);
        Assert.Equal(0.2f, verts[0].V);
        Assert.Equal(110f, verts[1].X);
        Assert.Equal(20f, verts[1].Y);
        Assert.Equal(0.8f, verts[1].U);
        Assert.Equal(0.2f, verts[1].V);
        Assert.Equal(10f, verts[2].X);
        Assert.Equal(70f, verts[2].Y);
        Assert.Equal(0.1f, verts[2].U);
        Assert.Equal(0.9f, verts[2].V);
        Assert.Equal(110f, verts[3].X);
        Assert.Equal(70f, verts[3].Y);
        Assert.Equal(0.8f, verts[3].U);
        Assert.Equal(0.9f, verts[3].V);
        Assert.Equal(32, Dx9VulkanFrontend.VertexStride);
        Assert.Equal(4, Dx9VulkanFrontend.D3dptTriangleList);
    }

    [Fact]
    public void Empty_dest_is_not_submitted()
    {
        var rec = new FrontendDx9DrawRecord(
            0, 0, 0, 0, 0, 0, 1, 1, 0xFFFFFFFF, 0, 2);
        var batch = Dx9VulkanFrontend.BuildBatch([rec], []);
        Assert.True(batch.IsEmpty);
        Assert.Empty(batch.Draws);
    }

    [Fact]
    public void Packed_dx9_vertex_has_rhw_one_and_dest_pixels()
    {
        var rec = new FrontendDx9DrawRecord(
            64, 32, 192, 96, 0, 0, 1, 1, 0x80FFFFFF, 1, 2);
        var verts = Dx9VulkanFrontend.BuildDx9Quad(rec);
        Assert.Equal(4, verts.Length);
        Assert.Equal(64f, verts[0].X);
        Assert.Equal(32f, verts[0].Y);
        Assert.Equal(1f, verts[0].Rhw);
        Assert.Equal(0f, verts[0].V);
        Assert.Equal(1f, verts[3].V);
        Assert.Equal(0x80FFFFFFu, verts[0].DiffuseArgb);
        Assert.Equal(Dx9VulkanFrontend.QuadTl, 0);
        Assert.Equal(Dx9VulkanFrontend.QuadTr, 1);
        Assert.Equal(Dx9VulkanFrontend.QuadBl, 2);
        Assert.Equal(Dx9VulkanFrontend.QuadBr, 3);
        Assert.Equal(rec.U0, verts[Dx9VulkanFrontend.QuadTl].U);
        Assert.Equal(rec.V0, verts[Dx9VulkanFrontend.QuadTl].V);
        Assert.Equal(rec.U1, verts[Dx9VulkanFrontend.QuadTr].U);
        Assert.Equal(rec.V0, verts[Dx9VulkanFrontend.QuadTr].V);
        Assert.Equal(rec.U0, verts[Dx9VulkanFrontend.QuadBl].U);
        Assert.Equal(rec.V1, verts[Dx9VulkanFrontend.QuadBl].V);
        Assert.Equal(rec.U1, verts[Dx9VulkanFrontend.QuadBr].U);
        Assert.Equal(rec.V1, verts[Dx9VulkanFrontend.QuadBr].V);
        Assert.Equal(new ushort[] { 0, 1, 2, 1, 3, 2 }, Dx9VulkanFrontend.QuadIndices);
        Assert.Equal(0x00BB0970u, Dx9VulkanFrontend.QuadFillFn);
        Assert.False(Dx9VulkanFrontend.FlipsUvV);
    }

    [Fact]
    public void Title_and_forest_frame_uv_is_full_texture()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var sprites = new FrontendSpriteBank(install);
        foreach (var name in new[]
                 {
                     FrontendSpriteBank.TitleLeft,
                     "FORREST_1_1",
                 })
        {
            var tex = sprites.TryLoad(name);
            Assert.NotNull(tex);
            Assert.Equal(tex.Width, tex.FrameWidth);
            Assert.Equal(tex.Height, tex.FrameHeight);
            var frame = tex.FrameUv();
            Assert.Equal(0f, frame.U0);
            Assert.Equal(0f, frame.V0);
            Assert.Equal(1f, frame.U1);
            Assert.Equal(1f, frame.V1);
            var submitted = FrontendDx9Submit.SubmittedSpriteUv(0, 0, 0, 0,
                frame.U0, frame.V0, frame.U1, frame.V1);
            Assert.Equal((0f, 0f, 1f, 1f), submitted);
            var rec = new FrontendDx9DrawRecord(
                0, 0, tex.Width, tex.Height,
                submitted.U0, submitted.V0, submitted.U1, submitted.V1,
                0xFFFFFFFFu, 0, 2);
            var verts = Dx9VulkanFrontend.BuildDx9Quad(rec);
            Assert.Equal(0f, verts[0].U);
            Assert.Equal(0f, verts[0].V);
            Assert.Equal(1f, verts[1].U);
            Assert.Equal(0f, verts[1].V);
            Assert.Equal(0f, verts[2].U);
            Assert.Equal(1f, verts[2].V);
            Assert.Equal(1f, verts[3].U);
            Assert.Equal(1f, verts[3].V);
            Assert.True(verts[0].Y < verts[2].Y);
            Assert.False(TextureFile.FirstSeenDecodeFlipsVertical);
            Assert.True(TextureFile.DecodeRowZeroIsTop);
        }
    }

    [Fact]
    public void Glyph_type_27_emits_six_28_byte_semantics()
    {
        var rec = new FrontendDx9DrawRecord(
            10.5f, 20.5f, 18.5f, 42.5f,
            0.1f, 0.2f, 0.3f, 0.4f,
            0xFFFFFFFF, 0, 2,
            RecordType: 0x27,
            VertexStride: 28,
            NativeUsedBytes: 28,
            AppliesHalfPixel: true);
        var batch = Dx9VulkanFrontend.BuildBatch([rec], [GpuTexture.White()]);
        Assert.False(batch.IsEmpty);
        Assert.Equal(6, batch.Vertices.Length);
        Assert.Empty(batch.Indices);
        Assert.Single(batch.Draws);
        var draw = batch.Draws[0];
        Assert.Equal(0x27, rec.RecordType);
        Assert.Equal(28, rec.VertexStride);
        Assert.Equal(28, rec.NativeUsedBytes);
        Assert.True(rec.AppliesHalfPixel);
        Assert.Equal(4, draw.D3dPrimitiveType);
        Assert.Equal(6u, draw.VertexCount);
        Assert.Equal(0u, draw.IndexCount);
        Assert.Equal(5, draw.D3dSrcBlend);
        Assert.Equal(6, draw.D3dDestBlend);
        Assert.Equal(0x00AB7C20u, Dx9VulkanFrontend.GlyphDrawFn);
        Assert.Equal(0x0054EF00u, Dx9VulkanFrontend.Type6DrawFn);
        Assert.Equal(324, Dx9VulkanFrontend.DrawPrimitiveVtbl);
        var list = Dx9VulkanFrontend.BuildDx9GlyphList(rec);
        Assert.Equal(6, list.Length);
        Assert.Equal(rec.U0, list[0].U);
        Assert.Equal(rec.V0, list[0].V);
        Assert.Equal(rec.U1, list[1].U);
        Assert.Equal(rec.V0, list[1].V);
        Assert.Equal(rec.U0, list[2].U);
        Assert.Equal(rec.V1, list[2].V);
        Assert.Equal(rec.U1, list[4].U);
        Assert.Equal(rec.V1, list[4].V);
        Assert.Equal(28u, FrontendDx9Vertex.NativeUsedBytes);
    }

    [Fact]
    public void Frontend_ps_keeps_clock_sprites_white_and_applies_glyph_diffuse()
    {
        Assert.Contains("vec4 c0 = vec4(1.0)", LineShaders.FrontendFragment, StringComparison.Ordinal);
        Assert.Contains("mix(c0, fragColor, fragUseDiffuseColor)",
            LineShaders.FrontendFragment, StringComparison.Ordinal);
        Assert.Contains("texture(sprite, fragUv) * tint",
            LineShaders.FrontendFragment, StringComparison.Ordinal);
        Assert.Equal("PSHADER_2D_CLOCK_SPRITE", Dx9VulkanFrontend.PixelShaderName);
        Assert.True(Dx9VulkanFrontend.PixelShaderC0TemporaryWhite);
        Assert.False(Dx9VulkanFrontend.AppliesScissor);
    }
}
