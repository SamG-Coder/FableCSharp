using System.Diagnostics;
using System.Numerics;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Fable.Render;

public sealed unsafe partial class VulkanLineRenderer
{
    private struct DeviceTexture
    {
        public int Id;
        public Image Image;
        public DeviceMemory Memory;
        public ImageView View;
        public DescriptorSet Set;
    }

    public void SetTextures(IReadOnlyList<GpuTexture> textures)
    {
        _vk.DeviceWaitIdle(_device);
        DestroyTextures();

        var count = 2 + textures.Count;
        var poolSizes = stackalloc DescriptorPoolSize[]
        {
            new() { Type = DescriptorType.CombinedImageSampler, DescriptorCount = (uint)count },
        };
        var poolInfo = new DescriptorPoolCreateInfo
        {
            SType = StructureType.DescriptorPoolCreateInfo,
            MaxSets = (uint)count,
            PoolSizeCount = 1,
            PPoolSizes = poolSizes,
        };
        Check(_vk.CreateDescriptorPool(_device, in poolInfo, null, out _descriptorPool));

        _fallbackTexture = UploadTexture(GpuTexture.Fallback());
        _textures[-1] = UploadTexture(GpuTexture.White());
        foreach (var texture in textures)
        {
            if (texture.Id == -1 || texture.Width <= 0 || texture.Height <= 0 || texture.Rgba.Length < 4)
                continue;
            _textures[texture.Id] = UploadTexture(texture);
        }
    }

    private void DrawMeshBatches(CommandBuffer commandBuffer)
    {
        DrawFamily(commandBuffer, _meshBuffer, _meshCount, _draws);
        DrawFamily(commandBuffer, _objectBuffer, _objectCount, _objectDraws);
    }

    private void DrawFamily(
        CommandBuffer commandBuffer, Buffer buffer, uint count, MeshDraw[] draws)
    {
        if (count == 0 && draws.Length == 0)
            return;
        if (buffer.Handle != 0)
        {
            var vb = buffer;
            ulong off = 0;
            _vk.CmdBindVertexBuffers(commandBuffer, 0, 1, in vb, in off);
        }

        if (draws.Length == 0)
        {
            BindTexture(commandBuffer, 0, 0);
            BindTexture(commandBuffer, 0, 1);
            _vk.CmdDraw(commandBuffer, count, 1, 0, 0);
            return;
        }

        var alpha = false;
        foreach (var draw in draws)
        {
            if (draw.VertexCount == 0)
                continue;
            if (draw.SrcAlphaBlend != alpha)
            {
                alpha = draw.SrcAlphaBlend;
                _vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Graphics,
                    alpha ? _meshAlphaPipeline : _meshPipeline);
                PushMeshConstants(commandBuffer);
            }
            if (Math.Abs(_meshPush.Pass.X - draw.ShaderMode) > 0.01f)
            {
                _meshPush.Pass = MeshPushConstants.PackPass(draw.ShaderMode);
                PushMeshConstants(commandBuffer);
            }

            var sky = draw.PassBit == Fable.Formats.Sky.SkyPass.FirstSeenLayerBit;
            var landscape = draw.PassBit is 0x4 or 0x40;
            var wanted = sky ? _skyViewProj : landscape ? _landscapeViewProj : _worldViewProj;
            if (_meshPush.ViewProj != wanted)
            {
                _meshPush.ViewProj = wanted;
                PushMeshConstants(commandBuffer);
            }

            // PSHADER_LANDSCAPE_FOREGROUND: mul_x2 t1 * v0 (RGB), t0.a * v0.a.
            // Stage 1 is the albedo. Primary TextureId must sit on t1 for FG.
            var fg = Math.Abs(draw.ShaderMode - 1f) < 0.01f;
            var albedo = draw.TextureId;
            var mask = draw.TextureId1 == 0 ? draw.TextureId : draw.TextureId1;
            BindTexture(commandBuffer, fg ? mask : albedo, 0);
            BindTexture(commandBuffer, fg ? albedo : mask, 1);
            _vk.CmdDraw(commandBuffer, draw.VertexCount, 1, draw.FirstVertex, 0);
        }
    }

    private void BindTexture(CommandBuffer commandBuffer, int textureId, uint setIndex)
    {
        var set = _textures.TryGetValue(textureId, out var texture) ? texture.Set : _fallbackTexture.Set;
        if (set.Handle == 0)
            return;
        _vk.CmdBindDescriptorSets(
            commandBuffer,
            PipelineBindPoint.Graphics,
            _meshPipelineLayout,
            setIndex, 1, in set,
            0, null);
    }

    private void CreateSamplerAndLayout()
    {
        // Fable DX9: UNREAD first-seen D3DSAMP_* writes.
        // Current Vulkan: LINEAR / REPEAT / MaxLod=1.
        // Status: TEMPORARY — NOT PARITY PROVEN
        var samplerInfo = Parity.Dx9Vulkan.Dx9VulkanSamplerState.FirstSeenTemporary();
        Check(_vk.CreateSampler(_device, in samplerInfo, null, out _sampler));

        var binding = new DescriptorSetLayoutBinding
        {
            Binding = 0,
            DescriptorType = DescriptorType.CombinedImageSampler,
            DescriptorCount = 1,
            StageFlags = ShaderStageFlags.FragmentBit,
        };
        var layoutInfo = new DescriptorSetLayoutCreateInfo
        {
            SType = StructureType.DescriptorSetLayoutCreateInfo,
            BindingCount = 1,
            PBindings = &binding,
        };
        Check(_vk.CreateDescriptorSetLayout(_device, in layoutInfo, null, out _descriptorSetLayout));
    }

    private DeviceTexture UploadTexture(GpuTexture texture)
    {
        var width = (uint)Math.Max(1, texture.Width);
        var height = (uint)Math.Max(1, texture.Height);
        var rgba = texture.Rgba.Length >= 4 ? texture.Rgba : GpuTexture.Fallback().Rgba;
        var bytes = (ulong)rgba.Length;

        CreateBuffer(bytes,
            BufferUsageFlags.TransferSrcBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            out var staging, out var stagingMemory);
        void* mapped;
        Check(_vk.MapMemory(_device, stagingMemory, 0, bytes, 0, &mapped));
        rgba.AsSpan().CopyTo(new Span<byte>(mapped, rgba.Length));
        _vk.UnmapMemory(_device, stagingMemory);

        var imageInfo = new ImageCreateInfo
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = ImageType.Type2D,
            Format = Format.R8G8B8A8Unorm,
            Extent = new Extent3D { Width = width, Height = height, Depth = 1 },
            MipLevels = 1,
            ArrayLayers = 1,
            Samples = SampleCountFlags.Count1Bit,
            Tiling = ImageTiling.Optimal,
            Usage = ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit,
            InitialLayout = ImageLayout.Undefined,
        };
        Check(_vk.CreateImage(_device, in imageInfo, null, out var image));
        _vk.GetImageMemoryRequirements(_device, image, out var req);
        var alloc = new MemoryAllocateInfo
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = req.Size,
            MemoryTypeIndex = FindMemoryType(req.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit),
        };
        Check(_vk.AllocateMemory(_device, in alloc, null, out var memory));
        Check(_vk.BindImageMemory(_device, image, memory, 0));

        Transition(image, ImageLayout.Undefined, ImageLayout.TransferDstOptimal);
        CopyBufferToImage(staging, image, width, height);
        Transition(image, ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal);

        _vk.DestroyBuffer(_device, staging, null);
        _vk.FreeMemory(_device, stagingMemory, null);

        var viewInfo = new ImageViewCreateInfo
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = image,
            ViewType = ImageViewType.Type2D,
            Format = Format.R8G8B8A8Unorm,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                LevelCount = 1,
                LayerCount = 1,
            },
        };
        Check(_vk.CreateImageView(_device, in viewInfo, null, out var view));

        var setLayout = _descriptorSetLayout;
        var allocInfo = new DescriptorSetAllocateInfo
        {
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = _descriptorPool,
            DescriptorSetCount = 1,
            PSetLayouts = &setLayout,
        };
        Check(_vk.AllocateDescriptorSets(_device, in allocInfo, out var set));
        var imageWrite = new DescriptorImageInfo
        {
            Sampler = _sampler,
            ImageView = view,
            ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
        };
        var write = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = set,
            DstBinding = 0,
            DescriptorCount = 1,
            DescriptorType = DescriptorType.CombinedImageSampler,
            PImageInfo = &imageWrite,
        };
        _vk.UpdateDescriptorSets(_device, 1, in write, 0, null);

        return new DeviceTexture
        {
            Id = texture.Id,
            Image = image,
            Memory = memory,
            View = view,
            Set = set,
        };
    }

    private void Transition(Image image, ImageLayout oldLayout, ImageLayout newLayout)
    {
        var barrier = new ImageMemoryBarrier
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = oldLayout,
            NewLayout = newLayout,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = image,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                LevelCount = 1,
                LayerCount = 1,
            },
        };
        PipelineStageFlags srcStage, dstStage;
        if (oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.TransferDstOptimal)
        {
            barrier.SrcAccessMask = 0;
            barrier.DstAccessMask = AccessFlags.TransferWriteBit;
            srcStage = PipelineStageFlags.TopOfPipeBit;
            dstStage = PipelineStageFlags.TransferBit;
        }
        else if (oldLayout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal)
        {
            barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
            barrier.DstAccessMask = AccessFlags.ShaderReadBit;
            srcStage = PipelineStageFlags.TransferBit;
            dstStage = PipelineStageFlags.FragmentShaderBit;
        }
        else if (oldLayout == ImageLayout.ShaderReadOnlyOptimal && newLayout == ImageLayout.TransferDstOptimal)
        {
            barrier.SrcAccessMask = AccessFlags.ShaderReadBit;
            barrier.DstAccessMask = AccessFlags.TransferWriteBit;
            srcStage = PipelineStageFlags.FragmentShaderBit;
            dstStage = PipelineStageFlags.TransferBit;
        }
        else
            throw new InvalidOperationException($"Unsupported layout {oldLayout} -> {newLayout}.");

        var command = BeginOneTime();
        _vk.CmdPipelineBarrier(command, srcStage, dstStage, 0, 0, null, 0, null, 1, in barrier);
        EndOneTime(command);
    }

    private void CopyBufferToImage(Buffer buffer, Image image, uint width, uint height)
    {
        var region = new BufferImageCopy
        {
            ImageSubresource = new ImageSubresourceLayers
            {
                AspectMask = ImageAspectFlags.ColorBit,
                LayerCount = 1,
            },
            ImageExtent = new Extent3D { Width = width, Height = height, Depth = 1 },
        };
        var command = BeginOneTime();
        _vk.CmdCopyBufferToImage(command, buffer, image, ImageLayout.TransferDstOptimal, 1, in region);
        EndOneTime(command);
    }

    private CommandBuffer BeginOneTime()
    {
        var alloc = new CommandBufferAllocateInfo
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = _commandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1,
        };
        Check(_vk.AllocateCommandBuffers(_device, in alloc, out var command));
        VideoOneTimeBegins++;
        var begin = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit,
        };
        Check(_vk.BeginCommandBuffer(command, in begin));
        return command;
    }

    private void EndOneTime(CommandBuffer command)
    {
        Check(_vk.EndCommandBuffer(command));
        var submit = new SubmitInfo
        {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &command,
        };
        Check(_vk.QueueSubmit(_graphicsQueue, 1, in submit, default));
        Check(_vk.QueueWaitIdle(_graphicsQueue));
        _vk.FreeCommandBuffers(_device, _commandPool, 1, in command);
    }

    private void DestroyTextures()
    {
        foreach (var texture in _textures.Values)
            DestroyTexture(texture);
        _textures.Clear();
        DestroyTexture(_fallbackTexture);
        _fallbackTexture = default;
        if (_descriptorPool.Handle != 0)
        {
            _vk.DestroyDescriptorPool(_device, _descriptorPool, null);
            _descriptorPool = default;
        }
    }

    private void DestroyTexture(DeviceTexture texture)
    {
        if (texture.View.Handle != 0)
            _vk.DestroyImageView(_device, texture.View, null);
        if (texture.Image.Handle != 0)
            _vk.DestroyImage(_device, texture.Image, null);
        if (texture.Memory.Handle != 0)
            _vk.FreeMemory(_device, texture.Memory, null);
    }

    /// <summary>
    /// <c>009DC870</c> 2D submit of the current WMV
    /// frame. <paramref name="dest"/> is 0–1 letterbox.
    /// </summary>
    public void SetVideoFrame(int width, int height, byte[]? rgba, Vector4 dest, int serial = 0)
    {
        if (rgba is null || width <= 0 || height <= 0 || rgba.Length < width * height * 4)
        {
            ClearVideoFrame();
            return;
        }

        _videoDest = dest;
        if (_videoReady &&
            _videoTexture.Image.Handle != 0 &&
            _videoTexture.Id == width &&
            serial == _videoSerial)
            return;

        // 00A3B730 GetPointer → 009FA450 LockRect
        // existing texture → copy → 009F9DE0 UnlockRect.
        // Unlock does not wait the GPU. 006286F0 later
        // consumes the texture via 009DC870.
        var upload0 = Stopwatch.GetTimestamp();
        _videoSerial = serial;
        if (_videoCpu is null || _videoCpu.Length < rgba.Length)
            _videoCpu = new byte[rgba.Length];
        rgba.AsSpan().CopyTo(_videoCpu.AsSpan(0, rgba.Length));
        _videoHeight = height;
        if (_videoTexture.Image.Handle == 0 ||
            _videoTexture.Id != width ||
            !_videoReady)
        {
            DestroyVideoImage();
            EnsureVideoPool();
            EnsureVideoStaging((ulong)rgba.Length);
            _videoTexture = CreateVideoImage(width, height);
            _videoReady = _videoTexture.Set.Handle != 0;
            _videoImageLayout = ImageLayout.Undefined;
        }

        _videoDirty = true;
        _videoUploadTicks = Stopwatch.GetTimestamp() - upload0;
    }

    public void ClearVideoFrame()
    {
        DestroyVideoTexture();
        _videoReady = false;
        _videoSerial = -1;
    }

    private DeviceTexture CreateVideoImage(int width, int height)
    {
        var imageInfo = new ImageCreateInfo
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = ImageType.Type2D,
            Format = Format.R8G8B8A8Unorm,
            Extent = new Extent3D { Width = (uint)width, Height = (uint)height, Depth = 1 },
            MipLevels = 1,
            ArrayLayers = 1,
            Samples = SampleCountFlags.Count1Bit,
            Tiling = ImageTiling.Optimal,
            Usage = ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit,
            InitialLayout = ImageLayout.Undefined,
        };
        Check(_vk.CreateImage(_device, in imageInfo, null, out var image));
        _vk.GetImageMemoryRequirements(_device, image, out var req);
        var alloc = new MemoryAllocateInfo
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = req.Size,
            MemoryTypeIndex = FindMemoryType(req.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit),
        };
        Check(_vk.AllocateMemory(_device, in alloc, null, out var memory));
        Check(_vk.BindImageMemory(_device, image, memory, 0));
        VideoImageCreates++;

        var viewInfo = new ImageViewCreateInfo
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = image,
            ViewType = ImageViewType.Type2D,
            Format = Format.R8G8B8A8Unorm,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                LevelCount = 1,
                LayerCount = 1,
            },
        };
        Check(_vk.CreateImageView(_device, in viewInfo, null, out var view));
        var setLayout = _descriptorSetLayout;
        var allocInfo = new DescriptorSetAllocateInfo
        {
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = _videoPool,
            DescriptorSetCount = 1,
            PSetLayouts = &setLayout,
        };
        Check(_vk.AllocateDescriptorSets(_device, in allocInfo, out var set));
        var imageWrite = new DescriptorImageInfo
        {
            Sampler = _sampler,
            ImageView = view,
            ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
        };
        var write = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = set,
            DstBinding = 0,
            DescriptorCount = 1,
            DescriptorType = DescriptorType.CombinedImageSampler,
            PImageInfo = &imageWrite,
        };
        _vk.UpdateDescriptorSets(_device, 1, in write, 0, null);
        VideoDescriptorUpdates++;
        return new DeviceTexture
        {
            Id = width,
            Image = image,
            Memory = memory,
            View = view,
            Set = set,
        };
    }

    private void EnsureVideoPool()
    {
        if (_videoPool.Handle != 0)
            return;
        var poolSize = new DescriptorPoolSize
        {
            Type = DescriptorType.CombinedImageSampler,
            DescriptorCount = 1,
        };
        var poolInfo = new DescriptorPoolCreateInfo
        {
            SType = StructureType.DescriptorPoolCreateInfo,
            MaxSets = 1,
            PoolSizeCount = 1,
            PPoolSizes = &poolSize,
        };
        Check(_vk.CreateDescriptorPool(_device, in poolInfo, null, out _videoPool));
    }

    /// <summary>
    /// Recorded into the existing frame command
    /// buffer after <c>WaitForFences</c> and
    /// before the render pass. Matches Unlock
    /// then 009DC870 in the same present.
    /// </summary>
    private void RecordVideoCopy(CommandBuffer command)
    {
        if (!_videoDirty ||
            !_videoReady ||
            _videoCpu is null ||
            _videoTexture.Image.Handle == 0)
            return;

        var bytes = (ulong)_videoCpu.Length;
        EnsureVideoStaging(bytes);
        var mapped = _videoMapped[_frame];
        if (mapped == null)
            return;
        _videoCpu.AsSpan().CopyTo(new Span<byte>(mapped, _videoCpu.Length));

        var from = _videoImageLayout == ImageLayout.Undefined
            ? ImageLayout.Undefined
            : ImageLayout.ShaderReadOnlyOptimal;
        CmdTransition(command, _videoTexture.Image, from, ImageLayout.TransferDstOptimal);
        var region = new BufferImageCopy
        {
            ImageSubresource = new ImageSubresourceLayers
            {
                AspectMask = ImageAspectFlags.ColorBit,
                LayerCount = 1,
            },
            ImageExtent = new Extent3D
            {
                Width = (uint)_videoTexture.Id,
                Height = (uint)Math.Max(_videoHeight, 1),
                Depth = 1,
            },
        };
        _vk.CmdCopyBufferToImage(
            command, _videoStagings[_frame], _videoTexture.Image,
            ImageLayout.TransferDstOptimal, 1, in region);
        CmdTransition(
            command, _videoTexture.Image,
            ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal);
        _videoImageLayout = ImageLayout.ShaderReadOnlyOptimal;
        _videoDirty = false;
        VideoUploads++;
        VideoSerialPresented = _videoSerial;
    }

    private void EnsureVideoStaging(ulong bytes)
    {
        if (_videoStagings[0].Handle != 0 && _videoStagingSize >= bytes)
            return;
        DestroyVideoStaging();
        for (var i = 0; i < MaxFrames; i++)
        {
            CreateBuffer(bytes,
                BufferUsageFlags.TransferSrcBit,
                MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
                out _videoStagings[i], out _videoStagingMemories[i]);
            void* mapped;
            Check(_vk.MapMemory(_device, _videoStagingMemories[i], 0, bytes, 0, &mapped));
            VideoMaps++;
            _videoMapped[i] = mapped;
            VideoStagingCreates++;
            VideoBufferCreates++;
            VideoMemoryAllocs++;
        }

        _videoStagingSize = bytes;
        VideoStagingAlive = MaxFrames;
        VideoStagingBytesAlive = bytes * MaxFrames;
    }

    private void CmdTransition(CommandBuffer command, Image image, ImageLayout oldLayout, ImageLayout newLayout)
    {
        var barrier = new ImageMemoryBarrier
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = oldLayout,
            NewLayout = newLayout,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = image,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                LevelCount = 1,
                LayerCount = 1,
            },
        };
        PipelineStageFlags srcStage, dstStage;
        if (oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.TransferDstOptimal)
        {
            barrier.SrcAccessMask = 0;
            barrier.DstAccessMask = AccessFlags.TransferWriteBit;
            srcStage = PipelineStageFlags.TopOfPipeBit;
            dstStage = PipelineStageFlags.TransferBit;
        }
        else if (oldLayout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal)
        {
            barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
            barrier.DstAccessMask = AccessFlags.ShaderReadBit;
            srcStage = PipelineStageFlags.TransferBit;
            dstStage = PipelineStageFlags.FragmentShaderBit;
        }
        else if (oldLayout == ImageLayout.ShaderReadOnlyOptimal && newLayout == ImageLayout.TransferDstOptimal)
        {
            barrier.SrcAccessMask = AccessFlags.ShaderReadBit;
            barrier.DstAccessMask = AccessFlags.TransferWriteBit;
            srcStage = PipelineStageFlags.FragmentShaderBit;
            dstStage = PipelineStageFlags.TransferBit;
        }
        else
            throw new InvalidOperationException($"Unsupported layout {oldLayout} -> {newLayout}.");

        _vk.CmdPipelineBarrier(command, srcStage, dstStage, 0, 0, null, 0, null, 1, in barrier);
    }

    private void DestroyVideoStaging()
    {
        for (var i = 0; i < MaxFrames; i++)
        {
            if (_videoMapped[i] != null && _videoStagingMemories[i].Handle != 0)
            {
                _vk.UnmapMemory(_device, _videoStagingMemories[i]);
                VideoUnmaps++;
                _videoMapped[i] = null;
            }

            if (_videoStagings[i].Handle != 0)
            {
                _vk.DestroyBuffer(_device, _videoStagings[i], null);
                _videoStagings[i] = default;
            }

            if (_videoStagingMemories[i].Handle != 0)
            {
                _vk.FreeMemory(_device, _videoStagingMemories[i], null);
                _videoStagingMemories[i] = default;
            }
        }

        _videoStagingSize = 0;
        VideoStagingAlive = 0;
        VideoStagingBytesAlive = 0;
    }

    private void DestroyVideoImage()
    {
        DestroyTexture(_videoTexture);
        _videoTexture = default;
        if (_videoPool.Handle != 0)
        {
            _vk.DestroyDescriptorPool(_device, _videoPool, null);
            _videoPool = default;
        }
    }

    private void DestroyVideoTexture()
    {
        DestroyVideoImage();
        DestroyVideoStaging();
        _videoCpu = null;
        _videoDirty = false;
        _videoImageLayout = ImageLayout.Undefined;
    }
}
