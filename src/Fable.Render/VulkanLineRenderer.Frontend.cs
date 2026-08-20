using System.Runtime.CompilerServices;
using Silk.NET.Shaderc;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Fable.Render;

public sealed unsafe partial class VulkanLineRenderer
{
    private PipelineLayout _frontendLayout;
    private Pipeline _frontendPipeline;
    private readonly Buffer[] _frontendVertexBuffers = new Buffer[MaxFrames];
    private readonly DeviceMemory[] _frontendVertexMemories = new DeviceMemory[MaxFrames];
    private readonly Buffer[] _frontendIndexBuffers = new Buffer[MaxFrames];
    private readonly DeviceMemory[] _frontendIndexMemories = new DeviceMemory[MaxFrames];
    private uint _frontendVertexCount;
    private uint _frontendIndexCount;
    private readonly uint[] _frontendVertexCapacities = new uint[MaxFrames];
    private readonly uint[] _frontendIndexCapacities = new uint[MaxFrames];
    private readonly int[] _frontendUploadedVersions = new int[MaxFrames];
    private FrontendGpuVertex[] _frontendVertices = [];
    private ushort[] _frontendIndices = [];
    private int _frontendVersion;
    private FrontendDraw[] _frontendDraws = [];
    private bool _frontendReady;

    public void SetFrontendBatch(FrontendSubmitBatch? batch)
    {
        if (batch is null || batch.Value.IsEmpty)
        {
            _frontendReady = false;
            _frontendDraws = [];
            return;
        }

        var value = batch.Value;
        if (value.Textures.Length > 0 && !SameTextureSet(value.Textures))
            SetTextures(value.Textures);
        _frontendVertices = value.Vertices;
        _frontendIndices = value.Indices;
        _frontendVertexCount = (uint)value.Vertices.Length;
        _frontendIndexCount = (uint)value.Indices.Length;
        _frontendVersion++;
        _frontendDraws = value.Draws;
        _frontendReady = _frontendVertexCount > 0 && _frontendPipeline.Handle != 0;
    }

    internal void CreateFrontendPipeline(
        PipelineShaderStageCreateInfo* stages,
        PipelineRasterizationStateCreateInfo raster,
        PipelineMultisampleStateCreateInfo multi,
        PipelineDynamicStateCreateInfo dynamic,
        PipelineViewportStateCreateInfo viewportState)
    {
        var vertSpv = GlslCompiler.Compile(
            LineShaders.FrontendVertex, ShaderKind.VertexShader, "frontend.vert");
        var fragSpv = GlslCompiler.Compile(
            LineShaders.FrontendFragment, ShaderKind.FragmentShader, "frontend.frag");
        var vert = CreateShaderModule(vertSpv);
        var frag = CreateShaderModule(fragSpv);
        stages[0].Module = vert;
        stages[1].Module = frag;
        var binding = Parity.Dx9Vulkan.Dx9VulkanFrontend.VertexBinding;
        var attributes = Parity.Dx9Vulkan.Dx9VulkanFrontend.VertexAttributes;
        fixed (VertexInputAttributeDescription* attr = attributes)
        {
            var vertexInput = new PipelineVertexInputStateCreateInfo
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount = 1,
                PVertexBindingDescriptions = &binding,
                VertexAttributeDescriptionCount = (uint)attributes.Length,
                PVertexAttributeDescriptions = attr,
            };
            var inputAssembly = new PipelineInputAssemblyStateCreateInfo
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = Parity.Dx9Vulkan.Dx9VulkanFrontend.FrontendTopology,
            };
            var blendAttachment = Parity.Dx9Vulkan.Dx9VulkanFrontend.DefaultSpriteBlend;
            var blend = new PipelineColorBlendStateCreateInfo
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo,
                AttachmentCount = 1,
                PAttachments = &blendAttachment,
            };
            var depth = Parity.Dx9Vulkan.Dx9VulkanFrontend.TemporaryDepthOff();
            var set = _descriptorSetLayout;
            var layoutInfo = new PipelineLayoutCreateInfo
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = 1,
                PSetLayouts = &set,
            };
            Check(_vk.CreatePipelineLayout(_device, in layoutInfo, null, out _frontendLayout));
            var pipelineInfo = new GraphicsPipelineCreateInfo
            {
                SType = StructureType.GraphicsPipelineCreateInfo,
                StageCount = 2,
                PStages = stages,
                PVertexInputState = &vertexInput,
                PInputAssemblyState = &inputAssembly,
                PViewportState = &viewportState,
                PRasterizationState = &raster,
                PMultisampleState = &multi,
                PDepthStencilState = &depth,
                PColorBlendState = &blend,
                PDynamicState = &dynamic,
                Layout = _frontendLayout,
                RenderPass = _renderPass,
            };
            Check(_vk.CreateGraphicsPipelines(
                _device, default, 1, in pipelineInfo, null, out _frontendPipeline));
        }

        _vk.DestroyShaderModule(_device, vert, null);
        _vk.DestroyShaderModule(_device, frag, null);
    }

    internal void DestroyFrontendPipeline()
    {
        if (_frontendPipeline.Handle != 0)
            _vk.DestroyPipeline(_device, _frontendPipeline, null);
        if (_frontendLayout.Handle != 0)
            _vk.DestroyPipelineLayout(_device, _frontendLayout, null);
        for (var i = 0; i < MaxFrames; i++)
        {
            if (_frontendVertexBuffers[i].Handle != 0)
            {
                _vk.DestroyBuffer(_device, _frontendVertexBuffers[i], null);
                _vk.FreeMemory(_device, _frontendVertexMemories[i], null);
            }

            if (_frontendIndexBuffers[i].Handle != 0)
            {
                _vk.DestroyBuffer(_device, _frontendIndexBuffers[i], null);
                _vk.FreeMemory(_device, _frontendIndexMemories[i], null);
            }

            _frontendVertexBuffers[i] = default;
            _frontendVertexMemories[i] = default;
            _frontendIndexBuffers[i] = default;
            _frontendIndexMemories[i] = default;
            _frontendVertexCapacities[i] = 0;
            _frontendIndexCapacities[i] = 0;
            _frontendUploadedVersions[i] = 0;
        }

        _frontendPipeline = default;
        _frontendLayout = default;
        _frontendVertices = [];
        _frontendIndices = [];
        _frontendReady = false;
    }

    /// <summary>
    /// Called only after the fence for <see cref="_frame"/> has completed.
    /// Each in-flight frame owns its frontend buffers, so cursor movement can
    /// update vertices without waiting for or overwriting the other frame.
    /// </summary>
    internal void UploadPendingFrontend()
    {
        if (!_frontendReady || _frontendUploadedVersions[_frame] == _frontendVersion)
            return;
        UploadFrontendVertices(_frontendVertices, _frame);
        UploadFrontendIndices(_frontendIndices, _frame);
        _frontendUploadedVersions[_frame] = _frontendVersion;
    }

    internal void DrawFrontend(CommandBuffer commandBuffer)
    {
        if (!_frontendReady || _frontendPipeline.Handle == 0 || _frontendVertexCount == 0)
            return;
        _vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Graphics, _frontendPipeline);
        ulong offset = 0;
        var vb = _frontendVertexBuffers[_frame];
        _vk.CmdBindVertexBuffers(commandBuffer, 0, 1, in vb, in offset);
        if (_frontendIndexCount > 0)
            _vk.CmdBindIndexBuffer(commandBuffer, _frontendIndexBuffers[_frame], 0, IndexType.Uint16);
        foreach (var draw in _frontendDraws)
        {
            BindFrontendTexture(commandBuffer, draw.TextureId);
            if (draw.IndexCount > 0)
            {
                _vk.CmdDrawIndexed(
                    commandBuffer, draw.IndexCount, 1, draw.FirstIndex,
                    (int)draw.FirstVertex, 0);
            }
            else
                _vk.CmdDraw(commandBuffer, draw.VertexCount, 1, draw.FirstVertex, 0);
        }
    }

    private void BindFrontendTexture(CommandBuffer commandBuffer, int textureId)
    {
        var set = _textures.TryGetValue(textureId, out var texture)
            ? texture.Set
            : _fallbackTexture.Set;
        if (set.Handle == 0)
            return;
        _vk.CmdBindDescriptorSets(
            commandBuffer,
            PipelineBindPoint.Graphics,
            _frontendLayout,
            0, 1, in set,
            0, null);
    }

    private void UploadFrontendVertices(FrontendGpuVertex[] vertices, int frame)
    {
        _frontendVertexCount = (uint)vertices.Length;
        if (_frontendVertexCount == 0)
            return;
        var bytes = (ulong)(vertices.Length * Unsafe.SizeOf<FrontendGpuVertex>());
        if (_frontendVertexCapacities[frame] < _frontendVertexCount)
        {
            if (_frontendVertexBuffers[frame].Handle != 0)
            {
                _vk.DestroyBuffer(_device, _frontendVertexBuffers[frame], null);
                _vk.FreeMemory(_device, _frontendVertexMemories[frame], null);
            }

            CreateBuffer(
                bytes,
                BufferUsageFlags.VertexBufferBit,
                MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
                out _frontendVertexBuffers[frame],
                out _frontendVertexMemories[frame]);
            _frontendVertexCapacities[frame] = _frontendVertexCount;
        }

        void* mapped;
        Check(_vk.MapMemory(_device, _frontendVertexMemories[frame], 0, bytes, 0, &mapped));
        vertices.CopyTo(new Span<FrontendGpuVertex>(mapped, vertices.Length));
        _vk.UnmapMemory(_device, _frontendVertexMemories[frame]);
    }

    private void UploadFrontendIndices(ushort[] indices, int frame)
    {
        _frontendIndexCount = (uint)indices.Length;
        if (_frontendIndexCount == 0)
            return;
        var bytes = (ulong)(indices.Length * sizeof(ushort));
        if (_frontendIndexCapacities[frame] < _frontendIndexCount)
        {
            if (_frontendIndexBuffers[frame].Handle != 0)
            {
                _vk.DestroyBuffer(_device, _frontendIndexBuffers[frame], null);
                _vk.FreeMemory(_device, _frontendIndexMemories[frame], null);
            }

            CreateBuffer(
                bytes,
                BufferUsageFlags.IndexBufferBit,
                MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
                out _frontendIndexBuffers[frame],
                out _frontendIndexMemories[frame]);
            _frontendIndexCapacities[frame] = _frontendIndexCount;
        }

        void* mapped;
        Check(_vk.MapMemory(_device, _frontendIndexMemories[frame], 0, bytes, 0, &mapped));
        indices.CopyTo(new Span<ushort>(mapped, indices.Length));
        _vk.UnmapMemory(_device, _frontendIndexMemories[frame]);
    }
}
