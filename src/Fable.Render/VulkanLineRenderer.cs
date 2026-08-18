using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Fable.Core;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Shaderc;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using Buffer = Silk.NET.Vulkan.Buffer;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Fable.Render;

public sealed unsafe partial class VulkanLineRenderer : IDisposable
{
    private const int MaxFrames = 2;

    private readonly IWindow _window;
    private readonly Vk _vk = Vk.GetApi();
    private readonly bool _validation;

    private Instance _instance;
    private ExtDebugUtils? _debugUtils;
    private DebugUtilsMessengerEXT _debugMessenger;
    private KhrSurface _khrSurface = null!;
    private SurfaceKHR _surface;
    private PhysicalDevice _physicalDevice;
    private Device _device;
    private uint _graphicsFamily;
    private uint _presentFamily;
    private Queue _graphicsQueue;
    private Queue _presentQueue;
    private KhrSwapchain _khrSwapchain = null!;
    private SwapchainKHR _swapchain;
    private Image[] _images = [];
    private ImageView[] _views = [];
    private Framebuffer[] _framebuffers = [];
    private Format _format;
    private Extent2D _extent;
    private RenderPass _renderPass;
    private PipelineLayout _pipelineLayout;
    private PipelineLayout _meshPipelineLayout;
    private Pipeline _linePipeline;
    private Pipeline _meshPipeline;
    private Pipeline _meshAlphaPipeline;
    private PipelineLayout _overlayLayout;
    private Pipeline _overlayPipeline;
    private PipelineLayout _videoLayout;
    private Pipeline _videoPipeline;
    private DescriptorPool _videoPool;
    private DeviceTexture _videoTexture;
    private Vector4 _videoDest = new(0, 0, 1, 1);
    private bool _videoReady;
    private bool _videoDirty;
    private int _videoSerial = -1;
    private byte[]? _videoCpu;
    private int _videoHeight;
    private ImageLayout _videoImageLayout;
    private Buffer[] _videoStagings = new Buffer[MaxFrames];
    private DeviceMemory[] _videoStagingMemories = new DeviceMemory[MaxFrames];
    private void*[] _videoMapped = new void*[MaxFrames];
    private ulong _videoStagingSize;
    private long _videoUploadTicks;
    private long _videoFrameTicks;
    private int _videoLastSampled = -1;
    /// <summary>
    /// <c>009FA450</c> LockRect is one persistent
    /// texture. These count the video upload path.
    /// </summary>
    public static int VideoStagingCreates { get; private set; }
    public static int VideoUploads { get; private set; }
    public static int VideoWaitIdles { get; private set; }
    public static int VideoDeviceWaitIdles { get; private set; }
    public static int VideoBufferCreates { get; private set; }
    public static int VideoMemoryAllocs { get; private set; }
    public static int VideoMaps { get; private set; }
    public static int VideoUnmaps { get; private set; }
    public static int VideoCmdAllocs { get; private set; }
    public static int VideoQueueSubmits { get; private set; }
    public static int VideoFences { get; private set; }
    public static int VideoFenceWaits { get; private set; }
    public static int VideoImageCreates { get; private set; }
    public static int VideoDescriptorUpdates { get; private set; }
    public static int VideoStagingAlive { get; private set; }
    public static ulong VideoStagingBytesAlive { get; private set; }
    public static int VideoDeferredDestroys { get; private set; }
    public static int VideoSerialPresented { get; private set; }
    public static int VideoSerialReceived { get; set; }
    public static int VideoOneTimeBegins { get; private set; }
    public static double LastUploadMs { get; private set; }
    public static double LastFrameMs { get; private set; }
    public static int VideoOtherFenceStatus { get; private set; }
    public static readonly List<VideoPresentSample> PresentSamples = [];
    private static long _videoTraceStart;

    public static void ResetVideoTrace()
    {
        VideoStagingCreates = 0;
        VideoUploads = 0;
        VideoWaitIdles = 0;
        VideoDeviceWaitIdles = 0;
        VideoBufferCreates = 0;
        VideoMemoryAllocs = 0;
        VideoMaps = 0;
        VideoUnmaps = 0;
        VideoCmdAllocs = 0;
        VideoQueueSubmits = 0;
        VideoFences = 0;
        VideoFenceWaits = 0;
        VideoImageCreates = 0;
        VideoDescriptorUpdates = 0;
        VideoStagingAlive = 0;
        VideoStagingBytesAlive = 0;
        VideoDeferredDestroys = 0;
        VideoSerialPresented = 0;
        VideoSerialReceived = 0;
        VideoOneTimeBegins = 0;
        LastUploadMs = 0;
        LastFrameMs = 0;
        VideoOtherFenceStatus = 0;
        PresentSamples.Clear();
        _videoTraceStart = Stopwatch.GetTimestamp();
    }

    /// <summary>
    /// After <see cref="SetVideoFrame"/> returns,
    /// the DirectShow serial may have moved on
    /// during QueueWaitIdle. Snap that gap.
    /// </summary>
    public static void NoteReceived(int received)
    {
        VideoSerialReceived = received;
        if (PresentSamples.Count > 0 &&
            PresentSamples[^1].Presented == VideoUploads)
            PresentSamples[^1].ReceivedSerial = received;
    }

    private static void RecordVideoPresent(int serial, long uploadTicks)
    {
        if (_videoTraceStart == 0)
            _videoTraceStart = Stopwatch.GetTimestamp();
        var wallMs = (Stopwatch.GetTimestamp() - _videoTraceStart) * 1000.0 / Stopwatch.Frequency;
        var uploadMs = uploadTicks * 1000.0 / Stopwatch.Frequency;
        PresentSamples.Add(new VideoPresentSample
        {
            Presented = VideoUploads,
            ReceivedSerial = VideoSerialReceived != 0 ? VideoSerialReceived : serial,
            PresentedSerial = serial,
            SerialDelta = (VideoSerialReceived != 0 ? VideoSerialReceived : serial) - serial,
            WallMs = wallMs,
            UploadMs = uploadMs,
            FrameMs = LastFrameMs,
            OneTimeMs = 0,
            OneTimeBegins = VideoOneTimeBegins,
            StagingCreates = VideoStagingCreates,
            BufferCreates = VideoBufferCreates,
            MemoryAllocs = VideoMemoryAllocs,
            Maps = VideoMaps,
            Unmaps = VideoUnmaps,
            CmdAllocs = VideoCmdAllocs,
            QueueSubmits = VideoQueueSubmits,
            Fences = VideoFences,
            FenceWaits = VideoFenceWaits,
            QueueWaitIdle = VideoWaitIdles,
            DeviceWaitIdle = VideoDeviceWaitIdles,
            StagingAlive = VideoStagingAlive,
            StagingBytes = VideoStagingBytesAlive,
            DeferredDestroys = VideoDeferredDestroys,
            ImageCreates = VideoImageCreates,
            DescriptorUpdates = VideoDescriptorUpdates,
            OtherFence = VideoOtherFenceStatus,
        });
    }
    private DescriptorSetLayout _descriptorSetLayout;
    private DescriptorPool _descriptorPool;
    private Sampler _sampler;
    private readonly Dictionary<int, DeviceTexture> _textures = new();
    private DeviceTexture _fallbackTexture;
    private MeshDraw[] _draws = [];
    private MeshPushConstants _meshPush;
    private Matrix4x4 _worldViewProj;
    private Matrix4x4 _landscapeViewProj;
    private Matrix4x4 _skyViewProj;
    private Image _depthImage;
    private DeviceMemory _depthMemory;
    private ImageView _depthView;
    private CommandPool _commandPool;
    private CommandBuffer[] _commandBuffers = [];
    private Semaphore[] _imageAvailable = [];
    private Semaphore[] _renderFinished = [];
    private Fence[] _inFlight = [];
    private Buffer _vertexBuffer;
    private DeviceMemory _vertexMemory;
    private uint _vertexCapacity;
    private uint _vertexCount;
    private Buffer _meshBuffer;
    private DeviceMemory _meshMemory;
    private uint _meshCapacity;
    private uint _meshCount;
    private Buffer _objectBuffer;
    private DeviceMemory _objectMemory;
    private uint _objectCapacity;
    private uint _objectCount;
    private MeshDraw[] _objectDraws = [];
    private int _frame;
    private bool _resized;
    private bool _playAviPump;

    public VulkanLineRenderer(IWindow window)
    {
        _window = window;
        _validation = CheckValidationAvailable();
        CreateInstance();
        CreateSurface();
        PickDevice();
        CreateDevice();
        CreateSwapchain();
        CreateImageViews();
        CreateDepthResources();
        CreateRenderPass();
        CreateSamplerAndLayout();
        CreatePipeline();
        CreateFramebuffers();
        CreateCommandPool();
        CreateSync();
        SetTextures([]);
        window.FramebufferResize += _ => _resized = true;
    }

    public void SetLines(ReadOnlySpan<LineVertex> lines)
    {
        _vertexCount = (uint)lines.Length;
        if (_vertexCount == 0)
            return;

        var bytes = (ulong)(lines.Length * Unsafe.SizeOf<LineVertex>());
        if (_vertexCapacity < _vertexCount)
        {
            if (_vertexBuffer.Handle != 0)
            {
                _vk.DestroyBuffer(_device, _vertexBuffer, null);
                _vk.FreeMemory(_device, _vertexMemory, null);
            }

            CreateBuffer(bytes,
                BufferUsageFlags.VertexBufferBit,
                MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
                out _vertexBuffer,
                out _vertexMemory);
            _vertexCapacity = _vertexCount;
        }

        void* mapped;
        Check(_vk.MapMemory(_device, _vertexMemory, 0, bytes, 0, &mapped));
        lines.CopyTo(new Span<LineVertex>(mapped, lines.Length));
        _vk.UnmapMemory(_device, _vertexMemory);
    }

    public void SetMesh(ReadOnlySpan<MeshVertex> vertices, ReadOnlySpan<MeshDraw> draws = default)
    {
        _draws = draws.Length == 0 ? [] : draws.ToArray();
        _meshCount = (uint)vertices.Length;
        if (_meshCount == 0)
            return;

        var bytes = (ulong)(vertices.Length * Unsafe.SizeOf<MeshVertex>());
        if (_meshCapacity < _meshCount)
        {
            if (_meshBuffer.Handle != 0)
            {
                _vk.DestroyBuffer(_device, _meshBuffer, null);
                _vk.FreeMemory(_device, _meshMemory, null);
            }

            CreateBuffer(bytes,
                BufferUsageFlags.VertexBufferBit,
                MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
                out _meshBuffer,
                out _meshMemory);
            _meshCapacity = _meshCount;
        }

        void* mapped;
        Check(_vk.MapMemory(_device, _meshMemory, 0, bytes, 0, &mapped));
        vertices.CopyTo(new Span<MeshVertex>(mapped, vertices.Length));
        _vk.UnmapMemory(_device, _meshMemory);
    }

    /// <summary>
    /// Static C3D family (layer <c>0x20</c>).
    /// Separate VB from landscape cells.
    /// </summary>
    public void SetObjects(ReadOnlySpan<MeshVertex> vertices, ReadOnlySpan<MeshDraw> draws = default)
    {
        _objectDraws = draws.Length == 0 ? [] : draws.ToArray();
        _objectCount = (uint)vertices.Length;
        if (_objectCount == 0)
            return;

        var bytes = (ulong)(vertices.Length * Unsafe.SizeOf<MeshVertex>());
        if (_objectCapacity < _objectCount)
        {
            if (_objectBuffer.Handle != 0)
            {
                _vk.DestroyBuffer(_device, _objectBuffer, null);
                _vk.FreeMemory(_device, _objectMemory, null);
            }

            CreateBuffer(bytes,
                BufferUsageFlags.VertexBufferBit,
                MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
                out _objectBuffer,
                out _objectMemory);
            _objectCapacity = _objectCount;
        }

        void* mapped;
        Check(_vk.MapMemory(_device, _objectMemory, 0, bytes, 0, &mapped));
        vertices.CopyTo(new Span<MeshVertex>(mapped, vertices.Length));
        _vk.UnmapMemory(_device, _objectMemory);
    }

    public bool ShowGizmos { get; set; }

    /// <summary>
    /// <c>006496BC</c> / <c>0041BEB0</c> type
    /// <c>0x22</c> overlay. RGB from <c>[+212]</c>,
    /// A from <c>004348D0</c>.
    /// </summary>
    public byte FadeOverlayAlpha { get; set; }
    public (byte R, byte G, byte B) FadeOverlayRgb { get; set; }
    public bool VideoOverlayActive => _videoReady;

    /// <summary>
    /// <c>006286F0</c> owns the pump: WaitEx then
    /// BeginScene/blit/EndScene/Present. Does not
    /// change the 3D swapchain interval.
    /// </summary>
    /// <summary>
    /// <c>006286F0</c> owns the pump: WaitEx then
    /// <c>009BEEB0</c> <c>IDirect3DDevice9::Present</c>.
    /// That is not Mailbox (non-blocking). FIFO is
    /// the vsync Present; recreate once on the edge.
    /// </summary>
    public void SetPlayAviPump(bool on)
    {
        if (_playAviPump == on)
            return;
        _playAviPump = on;
        _resized = true;
    }

    public void Draw(
        Matrix4x4 viewProjection,
        Vector3 cameraPosition = default,
        Vector4 fogPlane = default,
        Matrix4x4? skyViewProjection = null,
        Matrix4x4? landscapeViewProjection = null)
    {
        if (_extent.Width == 0 || _extent.Height == 0)
            return;

        var frame0 = Stopwatch.GetTimestamp();
        if (_playAviPump || _videoReady)
            PlayAviTimeline.Note("beginscene", PlayAviTimeline.SiteBeginScene, _videoSerial);
        _vk.WaitForFences(_device, 1, in _inFlight[_frame], true, ulong.MaxValue);
        var other = (_frame + 1) % MaxFrames;
        VideoOtherFenceStatus = (int)_vk.GetFenceStatus(_device, _inFlight[other]);

        uint imageIndex = 0;
        var acquire = _khrSwapchain.AcquireNextImage(
            _device, _swapchain, ulong.MaxValue, _imageAvailable[_frame], default, ref imageIndex);

        if (acquire is Result.ErrorOutOfDateKhr || _resized)
        {
            RecreateSwapchain();
            return;
        }

        if (acquire is not Result.Success and not Result.SuboptimalKhr)
            throw new InvalidOperationException($"AcquireNextImage failed: {acquire}");

        _vk.ResetFences(_device, 1, in _inFlight[_frame]);
        // Fable cameras submit the 009883F0 WVP (M22=+1).
        // Flip clip Y here — the only proven DX9→Vulkan site.
        var vkView = Parity.Dx9Vulkan.Dx9VulkanProjection.ToVulkanWvp(viewProjection);
        var vkSky = Parity.Dx9Vulkan.Dx9VulkanProjection.ToVulkanWvp(
            skyViewProjection ?? viewProjection);
        var vkLand = Parity.Dx9Vulkan.Dx9VulkanProjection.ToVulkanWvp(
            landscapeViewProjection ?? viewProjection);
        Record(
            _commandBuffers[_frame], imageIndex, vkView, cameraPosition, fogPlane,
            vkSky, vkLand);

        var wait = _imageAvailable[_frame];
        var signal = _renderFinished[_frame];
        var buffer = _commandBuffers[_frame];
        var waitStage = PipelineStageFlags.ColorAttachmentOutputBit;
        var submit = new SubmitInfo
        {
            SType = StructureType.SubmitInfo,
            WaitSemaphoreCount = 1,
            PWaitSemaphores = &wait,
            PWaitDstStageMask = &waitStage,
            CommandBufferCount = 1,
            PCommandBuffers = &buffer,
            SignalSemaphoreCount = 1,
            PSignalSemaphores = &signal,
        };
        Check(_vk.QueueSubmit(_graphicsQueue, 1, in submit, _inFlight[_frame]));
        if (_playAviPump || _videoReady)
        {
            PlayAviTimeline.Note("blit", PlayAviTimeline.SiteBlit, _videoSerial);
            PlayAviTimeline.Note("endscene", PlayAviTimeline.SiteEndScene, _videoSerial);
            PlayAviTimeline.Note("present-enter", PlayAviTimeline.SitePresentEnter, _videoSerial);
        }

        var swapchain = _swapchain;
        var present = new PresentInfoKHR
        {
            SType = StructureType.PresentInfoKhr,
            WaitSemaphoreCount = 1,
            PWaitSemaphores = &signal,
            SwapchainCount = 1,
            PSwapchains = &swapchain,
            PImageIndices = &imageIndex,
        };
        var presentResult = _khrSwapchain.QueuePresent(_presentQueue, in present);
        if (_playAviPump || _videoReady)
        {
            PlayAviTimeline.Note(
                "present-leave",
                PlayAviTimeline.SitePresentLeave,
                _videoSerial,
                extra: presentResult.ToString());
        }
        if (presentResult is Result.ErrorOutOfDateKhr or Result.SuboptimalKhr)
            RecreateSwapchain();
        else if (presentResult != Result.Success)
            throw new InvalidOperationException($"QueuePresent failed: {presentResult}");

        _frame = (_frame + 1) % MaxFrames;
        _videoFrameTicks = Stopwatch.GetTimestamp() - frame0;
        LastFrameMs = _videoFrameTicks * 1000.0 / Stopwatch.Frequency;
        LastUploadMs = _videoUploadTicks * 1000.0 / Stopwatch.Frequency;
        if (_videoReady &&
            VideoUploads > 0 &&
            (VideoUploads == 1 || VideoUploads % 100 == 0) &&
            VideoUploads != _videoLastSampled)
        {
            _videoLastSampled = VideoUploads;
            RecordVideoPresent(_videoSerial, _videoUploadTicks);
        }
    }

    public void Dispose()
    {
        _vk.DeviceWaitIdle(_device);
        DestroySwapchainObjects();
        if (_vertexBuffer.Handle != 0)
        {
            _vk.DestroyBuffer(_device, _vertexBuffer, null);
            _vk.FreeMemory(_device, _vertexMemory, null);
        }
        if (_meshBuffer.Handle != 0)
        {
            _vk.DestroyBuffer(_device, _meshBuffer, null);
            _vk.FreeMemory(_device, _meshMemory, null);
        }
        if (_objectBuffer.Handle != 0)
        {
            _vk.DestroyBuffer(_device, _objectBuffer, null);
            _vk.FreeMemory(_device, _objectMemory, null);
        }

        for (var i = 0; i < MaxFrames; i++)
        {
            _vk.DestroySemaphore(_device, _renderFinished[i], null);
            _vk.DestroySemaphore(_device, _imageAvailable[i], null);
            _vk.DestroyFence(_device, _inFlight[i], null);
        }

        _vk.DestroyCommandPool(_device, _commandPool, null);
        DestroyTextures();
        _vk.DestroyPipeline(_device, _linePipeline, null);
        _vk.DestroyPipeline(_device, _meshPipeline, null);
        if (_meshAlphaPipeline.Handle != 0)
            _vk.DestroyPipeline(_device, _meshAlphaPipeline, null);
        if (_overlayPipeline.Handle != 0)
            _vk.DestroyPipeline(_device, _overlayPipeline, null);
        if (_overlayLayout.Handle != 0)
            _vk.DestroyPipelineLayout(_device, _overlayLayout, null);
        if (_videoPipeline.Handle != 0)
            _vk.DestroyPipeline(_device, _videoPipeline, null);
        if (_videoLayout.Handle != 0)
            _vk.DestroyPipelineLayout(_device, _videoLayout, null);
        DestroyVideoTexture();
        _vk.DestroyPipelineLayout(_device, _pipelineLayout, null);
        if (_meshPipelineLayout.Handle != 0)
            _vk.DestroyPipelineLayout(_device, _meshPipelineLayout, null);
        if (_descriptorPool.Handle != 0)
            _vk.DestroyDescriptorPool(_device, _descriptorPool, null);
        if (_descriptorSetLayout.Handle != 0)
            _vk.DestroyDescriptorSetLayout(_device, _descriptorSetLayout, null);
        if (_sampler.Handle != 0)
            _vk.DestroySampler(_device, _sampler, null);
        _vk.DestroyRenderPass(_device, _renderPass, null);
        _vk.DestroyDevice(_device, null);
        if (_validation)
            _debugUtils?.DestroyDebugUtilsMessenger(_instance, _debugMessenger, null);
        _khrSurface.DestroySurface(_instance, _surface, null);
        _vk.DestroyInstance(_instance, null);
        _vk.Dispose();
    }

    private void CreateInstance()
    {
        var appName = (byte*)SilkMarshal.StringToPtr("FableCSharp");
        var engineName = (byte*)SilkMarshal.StringToPtr("Fable.Render");
        var appInfo = new ApplicationInfo
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = appName,
            ApplicationVersion = new Version32(0, 1, 0),
            PEngineName = engineName,
            EngineVersion = new Version32(0, 1, 0),
            ApiVersion = Vk.Version12,
        };

        var extensions = GetInstanceExtensions();
        var extPtr = (byte**)SilkMarshal.StringArrayToPtr(extensions);
        var layerPtr = _validation
            ? (byte**)SilkMarshal.StringArrayToPtr(["VK_LAYER_KHRONOS_validation"])
            : null;

        var debugInfo = new DebugUtilsMessengerCreateInfoEXT();
        PopulateDebug(ref debugInfo);

        var createInfo = new InstanceCreateInfo
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo,
            EnabledExtensionCount = (uint)extensions.Length,
            PpEnabledExtensionNames = extPtr,
            EnabledLayerCount = _validation ? 1u : 0,
            PpEnabledLayerNames = layerPtr,
            PNext = _validation ? &debugInfo : null,
        };

        Check(_vk.CreateInstance(in createInfo, null, out _instance));
        SilkMarshal.Free((nint)appName);
        SilkMarshal.Free((nint)engineName);
        SilkMarshal.Free((nint)extPtr);
        if (layerPtr is not null)
            SilkMarshal.Free((nint)layerPtr);

        if (_validation)
        {
            _vk.TryGetInstanceExtension(_instance, out _debugUtils);
            _debugUtils!.CreateDebugUtilsMessenger(_instance, in debugInfo, null, out _debugMessenger);
        }
    }

    private void CreateSurface()
    {
        if (!_vk.TryGetInstanceExtension<KhrSurface>(_instance, out var surfaceExt) || surfaceExt is null)
            throw new NotSupportedException("VK_KHR_surface is missing.");
        _khrSurface = surfaceExt;
        if (_window.VkSurface is null)
            throw new NotSupportedException("Window does not expose a Vulkan surface.");
        _surface = _window.VkSurface.Create<AllocationCallbacks>(_instance.ToHandle(), null).ToSurface();
    }

    private void PickDevice()
    {
        foreach (var device in _vk.GetPhysicalDevices(_instance))
        {
            if (TryFindQueues(device, out _graphicsFamily, out _presentFamily) &&
                DeviceHasSwapchain(device))
            {
                _physicalDevice = device;
                return;
            }
        }

        throw new InvalidOperationException("No Vulkan GPU with a swapchain was found.");
    }

    private void CreateDevice()
    {
        var families = new[] { _graphicsFamily, _presentFamily }.Distinct().ToArray();
        var infos = stackalloc DeviceQueueCreateInfo[families.Length];
        var priority = 1f;
        for (var i = 0; i < families.Length; i++)
        {
            infos[i] = new DeviceQueueCreateInfo
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = families[i],
                QueueCount = 1,
                PQueuePriorities = &priority,
            };
        }

        var features = new PhysicalDeviceFeatures();
        var extPtr = (byte**)SilkMarshal.StringArrayToPtr([KhrSwapchain.ExtensionName]);
        var createInfo = new DeviceCreateInfo
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = (uint)families.Length,
            PQueueCreateInfos = infos,
            PEnabledFeatures = &features,
            EnabledExtensionCount = 1,
            PpEnabledExtensionNames = extPtr,
        };
        Check(_vk.CreateDevice(_physicalDevice, in createInfo, null, out _device));
        SilkMarshal.Free((nint)extPtr);
        _vk.GetDeviceQueue(_device, _graphicsFamily, 0, out _graphicsQueue);
        _vk.GetDeviceQueue(_device, _presentFamily, 0, out _presentQueue);
        if (!_vk.TryGetDeviceExtension(_instance, _device, out _khrSwapchain) || _khrSwapchain is null)
            throw new NotSupportedException("VK_KHR_swapchain is missing.");
    }

    private void CreateSwapchain()
    {
        _khrSurface.GetPhysicalDeviceSurfaceCapabilities(_physicalDevice, _surface, out var caps);
        uint formatCount = 0;
        _khrSurface.GetPhysicalDeviceSurfaceFormats(_physicalDevice, _surface, ref formatCount, null);
        var formats = new SurfaceFormatKHR[formatCount];
        fixed (SurfaceFormatKHR* ptr = formats)
            _khrSurface.GetPhysicalDeviceSurfaceFormats(_physicalDevice, _surface, ref formatCount, ptr);

        uint presentCount = 0;
        _khrSurface.GetPhysicalDeviceSurfacePresentModes(_physicalDevice, _surface, ref presentCount, null);
        var presents = new PresentModeKHR[presentCount];
        fixed (PresentModeKHR* ptr = presents)
            _khrSurface.GetPhysicalDeviceSurfacePresentModes(_physicalDevice, _surface, ref presentCount, ptr);

        var surfaceFormat = formats.FirstOrDefault(f =>
            f.Format == Format.B8G8R8A8Unorm && f.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr);
        if (surfaceFormat.Format == 0)
            surfaceFormat = formats[0];

        // 009BEEB0 is IDirect3DDevice9::Present.
        // Mailbox returns in <1 ms and lets WaitEx(33)
        // beat the 33.3 ms sample clock (csharp timeline
        // p10 present 0.82 ms, 44% WaitEx timeout).
        // PlayAVI uses FIFO (vsync), same role as D3D
        // INTERVAL_ONE. 3D keeps Mailbox when present.
        var presentMode = _playAviPump || !presents.Contains(PresentModeKHR.MailboxKhr)
            ? PresentModeKHR.FifoKhr
            : PresentModeKHR.MailboxKhr;
        PlayAviTimeline.NotePresentParams(
            $"Vulkan PresentMode={presentMode} playAvi={_playAviPump} VSync={_window.VSync} images={Math.Max(caps.MinImageCount, 2)}");

        var width = caps.CurrentExtent.Width == uint.MaxValue
            ? (uint)Math.Clamp(_window.FramebufferSize.X, (int)caps.MinImageExtent.Width, (int)caps.MaxImageExtent.Width)
            : caps.CurrentExtent.Width;
        var height = caps.CurrentExtent.Height == uint.MaxValue
            ? (uint)Math.Clamp(_window.FramebufferSize.Y, (int)caps.MinImageExtent.Height, (int)caps.MaxImageExtent.Height)
            : caps.CurrentExtent.Height;
        if (width == 0)
            width = (uint)Math.Max(1, _window.Size.X);
        if (height == 0)
            height = (uint)Math.Max(1, _window.Size.Y);
        _extent = new Extent2D { Width = width, Height = height };

        var imageCount = caps.MinImageCount + 1;
        if (caps.MaxImageCount > 0 && imageCount > caps.MaxImageCount)
            imageCount = caps.MaxImageCount;

        var indices = stackalloc[] { _graphicsFamily, _presentFamily };
        var info = new SwapchainCreateInfoKHR
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = _surface,
            MinImageCount = imageCount,
            ImageFormat = surfaceFormat.Format,
            ImageColorSpace = surfaceFormat.ColorSpace,
            ImageExtent = _extent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            ImageSharingMode = _graphicsFamily == _presentFamily ? SharingMode.Exclusive : SharingMode.Concurrent,
            QueueFamilyIndexCount = _graphicsFamily == _presentFamily ? 0u : 2u,
            PQueueFamilyIndices = _graphicsFamily == _presentFamily ? null : indices,
            PreTransform = caps.CurrentTransform,
            CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            PresentMode = presentMode,
            Clipped = true,
        };

        Check(_khrSwapchain.CreateSwapchain(_device, in info, null, out _swapchain));
        _khrSwapchain.GetSwapchainImages(_device, _swapchain, ref imageCount, null);
        _images = new Image[imageCount];
        fixed (Image* ptr = _images)
            _khrSwapchain.GetSwapchainImages(_device, _swapchain, ref imageCount, ptr);
        _format = surfaceFormat.Format;
    }

    private void CreateImageViews()
    {
        _views = new ImageView[_images.Length];
        for (var i = 0; i < _images.Length; i++)
        {
            var info = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = _images[i],
                ViewType = ImageViewType.Type2D,
                Format = _format,
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    LevelCount = 1,
                    LayerCount = 1,
                },
            };
            Check(_vk.CreateImageView(_device, in info, null, out _views[i]));
        }
    }

    private void CreateRenderPass()
    {
        var attachments = stackalloc AttachmentDescription[]
        {
            new()
            {
                Format = _format,
                Samples = SampleCountFlags.Count1Bit,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.PresentSrcKhr,
            },
            new()
            {
                Format = Format.D32Sfloat,
                Samples = SampleCountFlags.Count1Bit,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.DontCare,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.DepthStencilAttachmentOptimal,
            },
        };
        var colorRef = new AttachmentReference { Attachment = 0, Layout = ImageLayout.ColorAttachmentOptimal };
        var depthRef = new AttachmentReference { Attachment = 1, Layout = ImageLayout.DepthStencilAttachmentOptimal };
        var subpass = new SubpassDescription
        {
            PipelineBindPoint = PipelineBindPoint.Graphics,
            ColorAttachmentCount = 1,
            PColorAttachments = &colorRef,
            PDepthStencilAttachment = &depthRef,
        };
        var dependency = new SubpassDependency
        {
            SrcSubpass = Vk.SubpassExternal,
            DstSubpass = 0,
            SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit,
            DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit,
            DstAccessMask = AccessFlags.ColorAttachmentWriteBit | AccessFlags.DepthStencilAttachmentWriteBit,
        };
        var info = new RenderPassCreateInfo
        {
            SType = StructureType.RenderPassCreateInfo,
            AttachmentCount = 2,
            PAttachments = attachments,
            SubpassCount = 1,
            PSubpasses = &subpass,
            DependencyCount = 1,
            PDependencies = &dependency,
        };
        Check(_vk.CreateRenderPass(_device, in info, null, out _renderPass));
    }

    private void CreatePipeline()
    {
        var vertSpv = GlslCompiler.Compile(LineShaders.Vertex, ShaderKind.VertexShader, "line.vert");
        var fragSpv = GlslCompiler.Compile(LineShaders.Fragment, ShaderKind.FragmentShader, "line.frag");
        var vert = CreateShaderModule(vertSpv);
        var frag = CreateShaderModule(fragSpv);
        var entry = (byte*)SilkMarshal.StringToPtr("main");

        var stages = stackalloc PipelineShaderStageCreateInfo[]
        {
            new()
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.VertexBit,
                Module = vert,
                PName = entry,
            },
            new()
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.FragmentBit,
                Module = frag,
                PName = entry,
            },
        };

        var binding = new VertexInputBindingDescription
        {
            Binding = 0,
            Stride = LineVertex.Stride,
            InputRate = VertexInputRate.Vertex,
        };
        var attributes = stackalloc VertexInputAttributeDescription[]
        {
            new() { Location = 0, Binding = 0, Format = Format.R32G32B32Sfloat, Offset = 0 },
            new() { Location = 1, Binding = 0, Format = Format.R32G32B32Sfloat, Offset = 12 },
        };
        var vertexInput = new PipelineVertexInputStateCreateInfo
        {
            SType = StructureType.PipelineVertexInputStateCreateInfo,
            VertexBindingDescriptionCount = 1,
            PVertexBindingDescriptions = &binding,
            VertexAttributeDescriptionCount = 2,
            PVertexAttributeDescriptions = attributes,
        };
        var inputAssembly = new PipelineInputAssemblyStateCreateInfo
        {
            SType = StructureType.PipelineInputAssemblyStateCreateInfo,
            Topology = PrimitiveTopology.LineList,
        };
        var viewport = Parity.Dx9Vulkan.Dx9VulkanViewport.FromFramebuffer(_extent.Width, _extent.Height);
        var scissor = new Rect2D { Extent = _extent };
        var viewportState = new PipelineViewportStateCreateInfo
        {
            SType = StructureType.PipelineViewportStateCreateInfo,
            ViewportCount = 1,
            PViewports = &viewport,
            ScissorCount = 1,
            PScissors = &scissor,
        };
        var dynamicStates = stackalloc DynamicState[] { DynamicState.Viewport, DynamicState.Scissor };
        var dynamic = new PipelineDynamicStateCreateInfo
        {
            SType = StructureType.PipelineDynamicStateCreateInfo,
            DynamicStateCount = 2,
            PDynamicStates = dynamicStates,
        };
        var raster = new PipelineRasterizationStateCreateInfo
        {
            SType = StructureType.PipelineRasterizationStateCreateInfo,
            // Fable DX9: UNREAD first-seen FILLMODE write (D3D default SOLID).
            // Vulkan: VK_POLYGON_MODE_FILL
            // Status: TEMPORARY — NOT PARITY PROVEN
            PolygonMode = Parity.Dx9Vulkan.Dx9VulkanRasterState.FirstSeenFillMode,
            LineWidth = 1f,
            CullMode = CullModeFlags.None,
            FrontFace = Parity.Dx9Vulkan.Dx9VulkanRasterState.FirstSeenFrontFace,
        };
        var multi = new PipelineMultisampleStateCreateInfo
        {
            SType = StructureType.PipelineMultisampleStateCreateInfo,
            RasterizationSamples = SampleCountFlags.Count1Bit,
        };
        var blendAttachment = Parity.Dx9Vulkan.Dx9VulkanBlendState.Opaque();
        var blend = new PipelineColorBlendStateCreateInfo
        {
            SType = StructureType.PipelineColorBlendStateCreateInfo,
            AttachmentCount = 1,
            PAttachments = &blendAttachment,
        };
        var push = new PushConstantRange
        {
            StageFlags = ShaderStageFlags.VertexBit,
            Size = 64,
        };
        var setLayout = _descriptorSetLayout;
        var lineLayoutInfo = new PipelineLayoutCreateInfo
        {
            SType = StructureType.PipelineLayoutCreateInfo,
            PushConstantRangeCount = 1,
            PPushConstantRanges = &push,
        };
        Check(_vk.CreatePipelineLayout(_device, in lineLayoutInfo, null, out _pipelineLayout));
        var meshPush = new PushConstantRange
        {
            StageFlags = ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
            Size = MeshPushConstants.Size,
        };
        var setLayouts = stackalloc DescriptorSetLayout[] { setLayout, setLayout };
        var meshLayoutInfo = new PipelineLayoutCreateInfo
        {
            SType = StructureType.PipelineLayoutCreateInfo,
            SetLayoutCount = 2,
            PSetLayouts = setLayouts,
            PushConstantRangeCount = 1,
            PPushConstantRanges = &meshPush,
        };
        Check(_vk.CreatePipelineLayout(_device, in meshLayoutInfo, null, out _meshPipelineLayout));

        var lineDepth = new PipelineDepthStencilStateCreateInfo
        {
            SType = StructureType.PipelineDepthStencilStateCreateInfo,
            DepthTestEnable = false,
            DepthWriteEnable = false,
            DepthCompareOp = CompareOp.Always,
        };
        // Fable DX9:
        // D3DRS_ZFUNC = D3DCMP_LESSEQUAL
        //
        // Vulkan equivalent:
        // depthCompareOp = VK_COMPARE_OP_LESS_OR_EQUAL
        //
        // Evidence:
        // D3dDeviceState.FirstSeenZFunc / PARITY first-seen lock.
        var meshDepth = Parity.Dx9Vulkan.Dx9VulkanDepth.FirstSeenOpaque();

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
            PDepthStencilState = &lineDepth,
            PColorBlendState = &blend,
            PDynamicState = &dynamic,
            Layout = _pipelineLayout,
            RenderPass = _renderPass,
        };
        Check(_vk.CreateGraphicsPipelines(_device, default, 1, in pipelineInfo, null, out _linePipeline));

        _vk.DestroyShaderModule(_device, vert, null);
        _vk.DestroyShaderModule(_device, frag, null);

        var meshVertSpv = GlslCompiler.Compile(LineShaders.MeshVertex, ShaderKind.VertexShader, "mesh.vert");
        var meshFragSpv = GlslCompiler.Compile(LineShaders.MeshFragment, ShaderKind.FragmentShader, "mesh.frag");
        var meshVert = CreateShaderModule(meshVertSpv);
        var meshFrag = CreateShaderModule(meshFragSpv);
        stages[0].Module = meshVert;
        stages[1].Module = meshFrag;
        var meshBinding = new VertexInputBindingDescription
        {
            Binding = 0,
            Stride = MeshVertex.Stride,
            InputRate = VertexInputRate.Vertex,
        };
        var meshAttributes = stackalloc VertexInputAttributeDescription[]
        {
            new() { Location = 0, Binding = 0, Format = Format.R32G32B32Sfloat, Offset = 0 },
            new() { Location = 1, Binding = 0, Format = Format.R32G32B32Sfloat, Offset = 12 },
            new() { Location = 2, Binding = 0, Format = Format.R32G32Sfloat, Offset = MeshVertex.UvOffset },
            new() { Location = 3, Binding = 0, Format = Format.R32G32B32A32Sfloat, Offset = MeshVertex.ColorOffset },
            new() { Location = 4, Binding = 0, Format = Format.R32G32B32Sfloat, Offset = MeshVertex.ExtraOffset },
        };
        var meshVertexInput = new PipelineVertexInputStateCreateInfo
        {
            SType = StructureType.PipelineVertexInputStateCreateInfo,
            VertexBindingDescriptionCount = 1,
            PVertexBindingDescriptions = &meshBinding,
            VertexAttributeDescriptionCount = 5,
            PVertexAttributeDescriptions = meshAttributes,
        };
        inputAssembly.Topology = Parity.Dx9Vulkan.Dx9VulkanPrimitive.World;
        // Fable DX9:
        // D3DRS_CULLMODE = D3DCULL_CCW (0x01396FB0 = 3)
        //
        // Vulkan equivalent:
        // FrontFace = VK_FRONT_FACE_COUNTER_CLOCKWISE
        // CullMode = VK_CULL_MODE_BACK_BIT
        //
        // Difference:
        // Dx9VulkanProjection flips clip Y; clip-CCW stays
        // framebuffer-CCW and is kept by CCW + Back.
        //
        // Proof:
        // D3dDeviceState.CullCcw / Dx9VulkanRasterState.
        raster.CullMode = Parity.Dx9Vulkan.Dx9VulkanRasterState.FirstSeenCullMode;
        raster.FrontFace = Parity.Dx9Vulkan.Dx9VulkanRasterState.FirstSeenFrontFace;
        pipelineInfo.PVertexInputState = &meshVertexInput;
        pipelineInfo.PDepthStencilState = &meshDepth;
        pipelineInfo.Layout = _meshPipelineLayout;
        Check(_vk.CreateGraphicsPipelines(_device, default, 1, in pipelineInfo, null, out _meshPipeline));
        // Fable DX9:
        // PALSKIN 00BD3867/00BD38D4 SRCALPHA / INVSRCALPHA.
        //
        // Vulkan equivalent:
        // src=SRC_ALPHA dst=ONE_MINUS_SRC_ALPHA op=ADD
        //
        // Evidence:
        // D3dDeviceState.FirstSeenPalskinSrcBlend.
        blendAttachment = Parity.Dx9Vulkan.Dx9VulkanBlendState.PalskinSrcAlpha();
        Check(_vk.CreateGraphicsPipelines(_device, default, 1, in pipelineInfo, null, out _meshAlphaPipeline));

        _vk.DestroyShaderModule(_device, meshVert, null);
        _vk.DestroyShaderModule(_device, meshFrag, null);

        var ovVertSpv = GlslCompiler.Compile(LineShaders.OverlayVertex, ShaderKind.VertexShader, "fade.vert");
        var ovFragSpv = GlslCompiler.Compile(LineShaders.OverlayFragment, ShaderKind.FragmentShader, "fade.frag");
        var ovVert = CreateShaderModule(ovVertSpv);
        var ovFrag = CreateShaderModule(ovFragSpv);
        stages[0].Module = ovVert;
        stages[1].Module = ovFrag;
        var ovPush = new PushConstantRange
        {
            StageFlags = ShaderStageFlags.FragmentBit,
            Size = 16,
        };
        var ovLayoutInfo = new PipelineLayoutCreateInfo
        {
            SType = StructureType.PipelineLayoutCreateInfo,
            PushConstantRangeCount = 1,
            PPushConstantRanges = &ovPush,
        };
        Check(_vk.CreatePipelineLayout(_device, in ovLayoutInfo, null, out _overlayLayout));
        var emptyVertexInput = new PipelineVertexInputStateCreateInfo
        {
            SType = StructureType.PipelineVertexInputStateCreateInfo,
        };
        raster.CullMode = CullModeFlags.None;
        pipelineInfo.PVertexInputState = &emptyVertexInput;
        pipelineInfo.PDepthStencilState = &lineDepth;
        pipelineInfo.Layout = _overlayLayout;
        Check(_vk.CreateGraphicsPipelines(_device, default, 1, in pipelineInfo, null, out _overlayPipeline));
        _vk.DestroyShaderModule(_device, ovVert, null);
        _vk.DestroyShaderModule(_device, ovFrag, null);

        var vidVertSpv = GlslCompiler.Compile(LineShaders.VideoVertex, ShaderKind.VertexShader, "video.vert");
        var vidFragSpv = GlslCompiler.Compile(LineShaders.VideoFragment, ShaderKind.FragmentShader, "video.frag");
        var vidVert = CreateShaderModule(vidVertSpv);
        var vidFrag = CreateShaderModule(vidFragSpv);
        stages[0].Module = vidVert;
        stages[1].Module = vidFrag;
        var vidSet = _descriptorSetLayout;
        var vidPush = new PushConstantRange
        {
            StageFlags = ShaderStageFlags.FragmentBit,
            Size = 16,
        };
        var vidLayoutInfo = new PipelineLayoutCreateInfo
        {
            SType = StructureType.PipelineLayoutCreateInfo,
            SetLayoutCount = 1,
            PSetLayouts = &vidSet,
            PushConstantRangeCount = 1,
            PPushConstantRanges = &vidPush,
        };
        Check(_vk.CreatePipelineLayout(_device, in vidLayoutInfo, null, out _videoLayout));
        pipelineInfo.Layout = _videoLayout;
        Check(_vk.CreateGraphicsPipelines(_device, default, 1, in pipelineInfo, null, out _videoPipeline));
        _vk.DestroyShaderModule(_device, vidVert, null);
        _vk.DestroyShaderModule(_device, vidFrag, null);
        SilkMarshal.Free((nint)entry);
    }

    private void CreateDepthResources()
    {
        var imageInfo = new ImageCreateInfo
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = ImageType.Type2D,
            Format = Format.D32Sfloat,
            Extent = new Extent3D { Width = _extent.Width, Height = _extent.Height, Depth = 1 },
            MipLevels = 1,
            ArrayLayers = 1,
            Samples = SampleCountFlags.Count1Bit,
            Tiling = ImageTiling.Optimal,
            Usage = ImageUsageFlags.DepthStencilAttachmentBit,
        };
        Check(_vk.CreateImage(_device, in imageInfo, null, out _depthImage));
        _vk.GetImageMemoryRequirements(_device, _depthImage, out var req);
        var alloc = new MemoryAllocateInfo
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = req.Size,
            MemoryTypeIndex = FindMemoryType(req.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit),
        };
        Check(_vk.AllocateMemory(_device, in alloc, null, out _depthMemory));
        Check(_vk.BindImageMemory(_device, _depthImage, _depthMemory, 0));

        var viewInfo = new ImageViewCreateInfo
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = _depthImage,
            ViewType = ImageViewType.Type2D,
            Format = Format.D32Sfloat,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.DepthBit,
                LevelCount = 1,
                LayerCount = 1,
            },
        };
        Check(_vk.CreateImageView(_device, in viewInfo, null, out _depthView));
    }

    private void CreateFramebuffers()
    {
        _framebuffers = new Framebuffer[_views.Length];
        var attachments = stackalloc ImageView[2];
        for (var i = 0; i < _views.Length; i++)
        {
            attachments[0] = _views[i];
            attachments[1] = _depthView;
            var info = new FramebufferCreateInfo
            {
                SType = StructureType.FramebufferCreateInfo,
                RenderPass = _renderPass,
                AttachmentCount = 2,
                PAttachments = attachments,
                Width = _extent.Width,
                Height = _extent.Height,
                Layers = 1,
            };
            Check(_vk.CreateFramebuffer(_device, in info, null, out _framebuffers[i]));
        }
    }

    private void CreateCommandPool()
    {
        var info = new CommandPoolCreateInfo
        {
            SType = StructureType.CommandPoolCreateInfo,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit,
            QueueFamilyIndex = _graphicsFamily,
        };
        Check(_vk.CreateCommandPool(_device, in info, null, out _commandPool));
        _commandBuffers = new CommandBuffer[MaxFrames];
        var alloc = new CommandBufferAllocateInfo
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = _commandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = MaxFrames,
        };
        fixed (CommandBuffer* ptr = _commandBuffers)
            Check(_vk.AllocateCommandBuffers(_device, in alloc, ptr));
    }

    private void CreateSync()
    {
        _imageAvailable = new Semaphore[MaxFrames];
        _renderFinished = new Semaphore[MaxFrames];
        _inFlight = new Fence[MaxFrames];
        var semaphoreInfo = new SemaphoreCreateInfo { SType = StructureType.SemaphoreCreateInfo };
        var fenceInfo = new FenceCreateInfo
        {
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit,
        };
        for (var i = 0; i < MaxFrames; i++)
        {
            Check(_vk.CreateSemaphore(_device, in semaphoreInfo, null, out _imageAvailable[i]));
            Check(_vk.CreateSemaphore(_device, in semaphoreInfo, null, out _renderFinished[i]));
            Check(_vk.CreateFence(_device, in fenceInfo, null, out _inFlight[i]));
        }
    }

    private void Record(
        CommandBuffer commandBuffer, uint imageIndex, Matrix4x4 viewProjection,
        Vector3 cameraPosition, Vector4 fogPlane, Matrix4x4 skyViewProjection,
        Matrix4x4 landscapeViewProjection)
    {
        var begin = new CommandBufferBeginInfo { SType = StructureType.CommandBufferBeginInfo };
        Check(_vk.BeginCommandBuffer(commandBuffer, in begin));
        RecordVideoCopy(commandBuffer);

        var clears = stackalloc ClearValue[]
        {
            new() { Color = new ClearColorValue(
                Parity.Dx9Vulkan.Dx9VulkanColor.FirstSeenClear.X,
                Parity.Dx9Vulkan.Dx9VulkanColor.FirstSeenClear.Y,
                Parity.Dx9Vulkan.Dx9VulkanColor.FirstSeenClear.Z,
                Parity.Dx9Vulkan.Dx9VulkanColor.FirstSeenClear.W) },
            new() { DepthStencil = new ClearDepthStencilValue { Depth = 1f } },
        };
        var pass = new RenderPassBeginInfo
        {
            SType = StructureType.RenderPassBeginInfo,
            RenderPass = _renderPass,
            Framebuffer = _framebuffers[imageIndex],
            RenderArea = new Rect2D { Extent = _extent },
            ClearValueCount = 2,
            PClearValues = clears,
        };
        _vk.CmdBeginRenderPass(commandBuffer, in pass, SubpassContents.Inline);

        var viewport = Parity.Dx9Vulkan.Dx9VulkanViewport.FromFramebuffer(_extent.Width, _extent.Height);
        var scissor = new Rect2D { Extent = _extent };
        _vk.CmdSetViewport(commandBuffer, 0, 1, in viewport);
        _vk.CmdSetScissor(commandBuffer, 0, 1, in scissor);

        // System.Numerics is row-major. GLSL mat4 is column-major, so uploading
        // the row-major bytes already transposes. Do not Transpose() again.
        var viewProj = viewProjection;

        // 006286F0 BeginScene/blit/EndScene/Present
        // does not draw landscape or fade.
        var playAviOnly = _playAviPump ||
            (_videoReady && _videoPipeline.Handle != 0 && _videoTexture.Set.Handle != 0);

        if (!playAviOnly &&
            ((_meshCount > 0 && _meshBuffer.Handle != 0) ||
             (_objectCount > 0 && _objectBuffer.Handle != 0)))
        {
            _skyViewProj = skyViewProjection;
            _worldViewProj = viewProjection;
            _landscapeViewProj = landscapeViewProjection;
            _meshPush = new MeshPushConstants
            {
                ViewProj = viewProjection,
                CameraPos = fogPlane == default
                    ? new Vector4(0f, 0f, 0f, 0f)
                    : fogPlane,
                LightDir = Fable.Formats.WorldShading.DirLightDirection,
                LightColor = Fable.Formats.WorldShading.DirLightColor,
                Pass = MeshPushConstants.PackPass(0f),
            };
            _vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Graphics, _meshPipeline);
            PushMeshConstants(commandBuffer);
            ulong offset = 0;
            var meshBuffer = _meshBuffer;
            _vk.CmdBindVertexBuffers(commandBuffer, 0, 1, in meshBuffer, in offset);
            DrawMeshBatches(commandBuffer);
        }

        if (!playAviOnly && ShowGizmos && _vertexCount > 0 && _vertexBuffer.Handle != 0)
        {
            _vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Graphics, _linePipeline);
            _vk.CmdPushConstants(commandBuffer, _pipelineLayout, ShaderStageFlags.VertexBit, 0, 64, &viewProj);
            ulong offset = 0;
            var vertexBuffer = _vertexBuffer;
            _vk.CmdBindVertexBuffers(commandBuffer, 0, 1, in vertexBuffer, in offset);
            _vk.CmdDraw(commandBuffer, _vertexCount, 1, 0, 0);
        }

        if (!playAviOnly && FadeOverlayAlpha > 0 && _overlayPipeline.Handle != 0)
        {
            var color = new Vector4(
                FadeOverlayRgb.R / 255f,
                FadeOverlayRgb.G / 255f,
                FadeOverlayRgb.B / 255f,
                FadeOverlayAlpha / 255f);
            _vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Graphics, _overlayPipeline);
            _vk.CmdPushConstants(
                commandBuffer, _overlayLayout, ShaderStageFlags.FragmentBit, 0, 16, &color);
            _vk.CmdDraw(commandBuffer, 3, 1, 0, 0);
        }

        if (_videoReady && _videoPipeline.Handle != 0 && _videoTexture.Set.Handle != 0)
        {
            var dest = _videoDest;
            var set = _videoTexture.Set;
            _vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Graphics, _videoPipeline);
            _vk.CmdBindDescriptorSets(
                commandBuffer, PipelineBindPoint.Graphics, _videoLayout,
                0, 1, in set, 0, null);
            _vk.CmdPushConstants(
                commandBuffer, _videoLayout, ShaderStageFlags.FragmentBit, 0, 16, &dest);
            _vk.CmdDraw(commandBuffer, 3, 1, 0, 0);
        }

        _vk.CmdEndRenderPass(commandBuffer);
        Check(_vk.EndCommandBuffer(commandBuffer));
    }

    private void PushMeshConstants(CommandBuffer commandBuffer)
    {
        var push = _meshPush;
        _vk.CmdPushConstants(commandBuffer, _meshPipelineLayout,
            ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
            0, MeshPushConstants.Size, &push);
    }

    private void RecreateSwapchain()
    {
        while (_window.FramebufferSize.X == 0 || _window.FramebufferSize.Y == 0)
            _window.DoEvents();

        _vk.DeviceWaitIdle(_device);
        DestroySwapchainObjects();
        CreateSwapchain();
        CreateImageViews();
        CreateDepthResources();
        CreateFramebuffers();
        _resized = false;
    }

    private void DestroySwapchainObjects()
    {
        foreach (var framebuffer in _framebuffers)
            _vk.DestroyFramebuffer(_device, framebuffer, null);
        foreach (var view in _views)
            _vk.DestroyImageView(_device, view, null);
        if (_depthView.Handle != 0)
            _vk.DestroyImageView(_device, _depthView, null);
        if (_depthImage.Handle != 0)
            _vk.DestroyImage(_device, _depthImage, null);
        if (_depthMemory.Handle != 0)
            _vk.FreeMemory(_device, _depthMemory, null);
        if (_swapchain.Handle != 0)
            _khrSwapchain.DestroySwapchain(_device, _swapchain, null);
        _framebuffers = [];
        _views = [];
    }

    private ShaderModule CreateShaderModule(byte[] code)
    {
        fixed (byte* ptr = code)
        {
            var info = new ShaderModuleCreateInfo
            {
                SType = StructureType.ShaderModuleCreateInfo,
                CodeSize = (nuint)code.Length,
                PCode = (uint*)ptr,
            };
            Check(_vk.CreateShaderModule(_device, in info, null, out var module));
            return module;
        }
    }

    private void CreateBuffer(ulong size, BufferUsageFlags usage, MemoryPropertyFlags flags,
        out Buffer buffer, out DeviceMemory memory)
    {
        var info = new BufferCreateInfo
        {
            SType = StructureType.BufferCreateInfo,
            Size = size,
            Usage = usage,
            SharingMode = SharingMode.Exclusive,
        };
        Check(_vk.CreateBuffer(_device, in info, null, out buffer));
        _vk.GetBufferMemoryRequirements(_device, buffer, out var req);
        var alloc = new MemoryAllocateInfo
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = req.Size,
            MemoryTypeIndex = FindMemoryType(req.MemoryTypeBits, flags),
        };
        Check(_vk.AllocateMemory(_device, in alloc, null, out memory));
        Check(_vk.BindBufferMemory(_device, buffer, memory, 0));
    }

    private uint FindMemoryType(uint filter, MemoryPropertyFlags flags)
    {
        _vk.GetPhysicalDeviceMemoryProperties(_physicalDevice, out var props);
        for (var i = 0; i < props.MemoryTypeCount; i++)
        {
            if ((filter & (1u << i)) != 0 && (props.MemoryTypes[i].PropertyFlags & flags) == flags)
                return (uint)i;
        }

        throw new InvalidOperationException("No matching Vulkan memory type.");
    }

    private bool TryFindQueues(PhysicalDevice device, out uint graphics, out uint present)
    {
        graphics = present = uint.MaxValue;
        uint count = 0;
        _vk.GetPhysicalDeviceQueueFamilyProperties(device, ref count, null);
        var families = new QueueFamilyProperties[count];
        fixed (QueueFamilyProperties* ptr = families)
            _vk.GetPhysicalDeviceQueueFamilyProperties(device, ref count, ptr);

        for (uint i = 0; i < count; i++)
        {
            if (families[i].QueueFlags.HasFlag(QueueFlags.GraphicsBit))
                graphics = i;
            _khrSurface.GetPhysicalDeviceSurfaceSupport(device, i, _surface, out var canPresent);
            if (canPresent)
                present = i;
            if (graphics != uint.MaxValue && present != uint.MaxValue)
                return true;
        }

        return false;
    }

    private bool DeviceHasSwapchain(PhysicalDevice device)
    {
        uint count = 0;
        _vk.EnumerateDeviceExtensionProperties(device, (byte*)null, ref count, null);
        var exts = new ExtensionProperties[count];
        fixed (ExtensionProperties* ptr = exts)
            _vk.EnumerateDeviceExtensionProperties(device, (byte*)null, ref count, ptr);
        return exts.Any(e => Marshal.PtrToStringAnsi((nint)e.ExtensionName) == KhrSwapchain.ExtensionName);
    }

    private string[] GetInstanceExtensions()
    {
        var glfw = _window.VkSurface!.GetRequiredExtensions(out var count);
        var names = SilkMarshal.PtrToStringArray((nint)glfw, (int)count);
        return _validation ? names.Append(ExtDebugUtils.ExtensionName).ToArray() : names;
    }

    private bool CheckValidationAvailable()
    {
        uint count = 0;
        _vk.EnumerateInstanceLayerProperties(ref count, null);
        var layers = new LayerProperties[count];
        fixed (LayerProperties* ptr = layers)
            _vk.EnumerateInstanceLayerProperties(ref count, ptr);
        return layers.Any(l => Marshal.PtrToStringAnsi((nint)l.LayerName) == "VK_LAYER_KHRONOS_validation");
    }

    private static void PopulateDebug(ref DebugUtilsMessengerCreateInfoEXT info)
    {
        info.SType = StructureType.DebugUtilsMessengerCreateInfoExt;
        info.MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
                               DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt;
        info.MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt |
                           DebugUtilsMessageTypeFlagsEXT.ValidationBitExt |
                           DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt;
        info.PfnUserCallback = (DebugUtilsMessengerCallbackFunctionEXT)DebugCallback;
    }

    private static uint DebugCallback(
        DebugUtilsMessageSeverityFlagsEXT severity,
        DebugUtilsMessageTypeFlagsEXT types,
        DebugUtilsMessengerCallbackDataEXT* data,
        void* user)
    {
        Console.Error.WriteLine(Marshal.PtrToStringAnsi((nint)data->PMessage));
        return Vk.False;
    }

    private static void Check(Result result)
    {
        if (result != Result.Success)
            throw new InvalidOperationException($"Vulkan error: {result}");
    }
}

/// <summary>
/// One sample of the video upload/present path.
/// Observation of <c>009FA450</c> LockRect vs
/// per-frame staging.
/// </summary>
public sealed class VideoPresentSample
{
    public int Presented { get; init; }
    public int ReceivedSerial { get; set; }
    public int PresentedSerial { get; init; }
    public int SerialDelta { get; init; }
    public double WallMs { get; init; }
    public double UploadMs { get; init; }
    public double FrameMs { get; init; }
    public double OneTimeMs { get; init; }
    public int OneTimeBegins { get; init; }
    public int StagingCreates { get; init; }
    public int BufferCreates { get; init; }
    public int MemoryAllocs { get; init; }
    public int Maps { get; init; }
    public int Unmaps { get; init; }
    public int CmdAllocs { get; init; }
    public int QueueSubmits { get; init; }
    public int Fences { get; init; }
    public int FenceWaits { get; init; }
    public int QueueWaitIdle { get; init; }
    public int DeviceWaitIdle { get; init; }
    public int StagingAlive { get; init; }
    public ulong StagingBytes { get; init; }
    public int DeferredDestroys { get; init; }
    public int ImageCreates { get; init; }
    public int DescriptorUpdates { get; init; }
    public int OtherFence { get; init; }
}
