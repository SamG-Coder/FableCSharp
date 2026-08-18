using System.Runtime.CompilerServices;
using Silk.NET.Shaderc;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Fable.Render;

public sealed unsafe partial class VulkanLineRenderer
{
    private PipelineLayout _frontendLayout;
    private Pipeline _frontendPipeline;
    private Buffer _frontendVertexBuffer;
    private DeviceMemory _frontendVertexMemory;
    private Buffer _frontendIndexBuffer;
    private DeviceMemory _frontendIndexMemory;
    private uint _frontendVertexCount;
    private uint _frontendIndexCount;
    private uint _frontendVertexCapacity;
    private uint _frontendIndexCapacity;
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
        if (value.Textures.Length > 0)
            SetTextures(value.Textures);
        UploadFrontendVertices(value.Vertices);
        UploadFrontendIndices(value.Indices);
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
        if (_frontendVertexBuffer.Handle != 0)
        {
            _vk.DestroyBuffer(_device, _frontendVertexBuffer, null);
            _vk.FreeMemory(_device, _frontendVertexMemory, null);
        }

        if (_frontendIndexBuffer.Handle != 0)
        {
            _vk.DestroyBuffer(_device, _frontendIndexBuffer, null);
            _vk.FreeMemory(_device, _frontendIndexMemory, null);
        }

        _frontendPipeline = default;
        _frontendLayout = default;
        _frontendVertexBuffer = default;
        _frontendIndexBuffer = default;
        _frontendReady = false;
    }

    internal void DrawFrontend(CommandBuffer commandBuffer)
    {
        if (!_frontendReady || _frontendPipeline.Handle == 0 || _frontendVertexCount == 0)
            return;
        _vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Graphics, _frontendPipeline);
        ulong offset = 0;
        var vb = _frontendVertexBuffer;
        _vk.CmdBindVertexBuffers(commandBuffer, 0, 1, in vb, in offset);
        if (_frontendIndexCount > 0)
            _vk.CmdBindIndexBuffer(commandBuffer, _frontendIndexBuffer, 0, IndexType.Uint16);
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

    private void UploadFrontendVertices(FrontendGpuVertex[] vertices)
    {
        _frontendVertexCount = (uint)vertices.Length;
        if (_frontendVertexCount == 0)
            return;
        var bytes = (ulong)(vertices.Length * Unsafe.SizeOf<FrontendGpuVertex>());
        if (_frontendVertexCapacity < _frontendVertexCount)
        {
            if (_frontendVertexBuffer.Handle != 0)
            {
                _vk.DestroyBuffer(_device, _frontendVertexBuffer, null);
                _vk.FreeMemory(_device, _frontendVertexMemory, null);
            }

            CreateBuffer(
                bytes,
                BufferUsageFlags.VertexBufferBit,
                MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
                out _frontendVertexBuffer,
                out _frontendVertexMemory);
            _frontendVertexCapacity = _frontendVertexCount;
        }

        void* mapped;
        Check(_vk.MapMemory(_device, _frontendVertexMemory, 0, bytes, 0, &mapped));
        vertices.CopyTo(new Span<FrontendGpuVertex>(mapped, vertices.Length));
        _vk.UnmapMemory(_device, _frontendVertexMemory);
    }

    private void UploadFrontendIndices(ushort[] indices)
    {
        _frontendIndexCount = (uint)indices.Length;
        if (_frontendIndexCount == 0)
            return;
        var bytes = (ulong)(indices.Length * sizeof(ushort));
        if (_frontendIndexCapacity < _frontendIndexCount)
        {
            if (_frontendIndexBuffer.Handle != 0)
            {
                _vk.DestroyBuffer(_device, _frontendIndexBuffer, null);
                _vk.FreeMemory(_device, _frontendIndexMemory, null);
            }

            CreateBuffer(
                bytes,
                BufferUsageFlags.IndexBufferBit,
                MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
                out _frontendIndexBuffer,
                out _frontendIndexMemory);
            _frontendIndexCapacity = _frontendIndexCount;
        }

        void* mapped;
        Check(_vk.MapMemory(_device, _frontendIndexMemory, 0, bytes, 0, &mapped));
        indices.CopyTo(new Span<ushort>(mapped, indices.Length));
        _vk.UnmapMemory(_device, _frontendIndexMemory);
    }
}
