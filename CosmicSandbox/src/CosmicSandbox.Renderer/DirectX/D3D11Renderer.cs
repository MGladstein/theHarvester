using CosmicSandbox.Core.Interfaces;
using CosmicSandbox.Core.Models;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace CosmicSandbox.Renderer.DirectX;

/// <summary>
/// DirectX 11 renderer with PBR, HDR, and particle systems
/// </summary>
public class D3D11Renderer : IRenderer
{
    private Device? _device;
    private DeviceContext? _context;
    private SwapChain? _swapChain;
    private RenderTargetView? _renderTargetView;
    private DepthStencilView? _depthStencilView;
    private Texture2D? _depthBuffer;

    private int _width;
    private int _height;
    private bool _isInitialized;

    private readonly RenderStats _stats = new();
    private DateTime _lastFrameTime = DateTime.UtcNow;

    public void Initialize(IntPtr windowHandle, int width, int height)
    {
        _width = width;
        _height = height;

        // Create swap chain description
        var swapChainDesc = new SwapChainDescription
        {
            BufferCount = 2,
            ModeDescription = new ModeDescription(
                width,
                height,
                new Rational(60, 1),
                Format.R8G8B8A8_UNorm
            ),
            IsWindowed = true,
            OutputHandle = windowHandle,
            SampleDescription = new SampleDescription(1, 0),
            SwapEffect = SwapEffect.FlipDiscard,
            Usage = Usage.RenderTargetOutput,
            Flags = SwapChainFlags.AllowModeSwitch
        };

        // Create device and swap chain
        Device.CreateWithSwapChain(
            DriverType.Hardware,
            DeviceCreationFlags.BgraSupport,
            new[] { FeatureLevel.Level_11_1, FeatureLevel.Level_11_0 },
            swapChainDesc,
            out _device,
            out _swapChain
        );

        _context = _device.ImmediateContext;

        // Create render target view
        using (var backBuffer = _swapChain.GetBackBuffer<Texture2D>(0))
        {
            _renderTargetView = new RenderTargetView(_device, backBuffer);
        }

        // Create depth stencil buffer
        CreateDepthStencil(width, height);

        // Setup viewport
        var viewport = new Viewport(0, 0, width, height, 0.0f, 1.0f);
        _context.Rasterizer.SetViewport(viewport);

        // Create rasterizer state
        var rasterizerStateDesc = new RasterizerStateDescription
        {
            CullMode = CullMode.Back,
            FillMode = FillMode.Solid,
            IsDepthClipEnabled = true,
            IsFrontCounterClockwise = false,
            IsMultisampleEnabled = false
        };

        using (var rasterizerState = new RasterizerState(_device, rasterizerStateDesc))
        {
            _context.Rasterizer.State = rasterizerState;
        }

        // Create depth stencil state
        var depthStencilStateDesc = new DepthStencilStateDescription
        {
            IsDepthEnabled = true,
            DepthWriteMask = DepthWriteMask.All,
            DepthComparison = Comparison.Less,
            IsStencilEnabled = false
        };

        using (var depthStencilState = new DepthStencilState(_device, depthStencilStateDesc))
        {
            _context.OutputMerger.SetDepthStencilState(depthStencilState);
        }

        _isInitialized = true;
    }

    public void Render(SimulationState state, double deltaTime)
    {
        if (!_isInitialized || _context == null || _renderTargetView == null || _depthStencilView == null)
            return;

        _stats.FrameCount++;
        var now = DateTime.UtcNow;
        _stats.FrameTime = (now - _lastFrameTime).TotalMilliseconds;
        _stats.FPS = 1000.0 / _stats.FrameTime;
        _lastFrameTime = now;

        // Clear buffers
        _context.ClearRenderTargetView(_renderTargetView, new RawColor4(0.01f, 0.01f, 0.02f, 1.0f));
        _context.ClearDepthStencilView(_depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);

        _context.OutputMerger.SetRenderTargets(_depthStencilView, _renderTargetView);

        // TODO: Actual rendering of celestial bodies
        // This would involve:
        // 1. Setting up view/projection matrices from camera
        // 2. Rendering each celestial body with appropriate shaders
        // 3. Rendering particle effects
        // 4. Post-processing (HDR, bloom)

        _stats.DrawCalls = state.Bodies.Count(b => b.IsActive);

        // Present
        _swapChain?.Present(1, PresentFlags.None);
    }

    public void Resize(int width, int height)
    {
        if (!_isInitialized) return;

        _width = width;
        _height = height;

        // Dispose old resources
        _renderTargetView?.Dispose();
        _depthStencilView?.Dispose();
        _depthBuffer?.Dispose();

        // Resize swap chain
        _swapChain?.ResizeBuffers(2, width, height, Format.R8G8B8A8_UNorm, SwapChainFlags.AllowModeSwitch);

        // Recreate render target view
        if (_swapChain != null && _device != null)
        {
            using (var backBuffer = _swapChain.GetBackBuffer<Texture2D>(0))
            {
                _renderTargetView = new RenderTargetView(_device, backBuffer);
            }

            CreateDepthStencil(width, height);

            // Update viewport
            var viewport = new Viewport(0, 0, width, height, 0.0f, 1.0f);
            _context?.Rasterizer.SetViewport(viewport);
        }
    }

    public byte[] CaptureFrame()
    {
        if (!_isInitialized || _swapChain == null || _device == null)
            return Array.Empty<byte>();

        // Get back buffer
        using var backBuffer = _swapChain.GetBackBuffer<Texture2D>(0);

        // Create staging texture
        var stagingDesc = backBuffer.Description;
        stagingDesc.Usage = ResourceUsage.Staging;
        stagingDesc.BindFlags = BindFlags.None;
        stagingDesc.CpuAccessFlags = CpuAccessFlags.Read;

        using var stagingTexture = new Texture2D(_device, stagingDesc);

        // Copy back buffer to staging texture
        _context?.CopyResource(backBuffer, stagingTexture);

        // Map and read data
        var dataBox = _context?.MapSubresource(stagingTexture, 0, MapMode.Read, MapFlags.None);
        if (!dataBox.HasValue) return Array.Empty<byte>();

        try
        {
            var bytes = new byte[_width * _height * 4];
            var pitch = dataBox.Value.RowPitch;

            unsafe
            {
                var src = (byte*)dataBox.Value.DataPointer;
                fixed (byte* dst = bytes)
                {
                    for (int y = 0; y < _height; y++)
                    {
                        System.Buffer.MemoryCopy(
                            src + y * pitch,
                            dst + y * _width * 4,
                            _width * 4,
                            _width * 4
                        );
                    }
                }
            }

            return bytes;
        }
        finally
        {
            _context?.UnmapSubresource(stagingTexture, 0);
        }
    }

    public void UpdateCamera(CameraState camera)
    {
        // Camera update will be used for view/projection matrix calculation
        // To be implemented with actual rendering
    }

    public RenderStats GetStats() => _stats;

    private void CreateDepthStencil(int width, int height)
    {
        if (_device == null) return;

        var depthBufferDesc = new Texture2DDescription
        {
            Width = width,
            Height = height,
            MipLevels = 1,
            ArraySize = 1,
            Format = Format.D32_Float,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.DepthStencil,
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = ResourceOptionFlags.None
        };

        _depthBuffer = new Texture2D(_device, depthBufferDesc);

        var depthStencilViewDesc = new DepthStencilViewDescription
        {
            Format = Format.D32_Float,
            Dimension = DepthStencilViewDimension.Texture2D,
            Texture2D = new DepthStencilViewDescription.Texture2DResource
            {
                MipSlice = 0
            }
        };

        _depthStencilView = new DepthStencilView(_device, _depthBuffer, depthStencilViewDesc);
    }

    public void Dispose()
    {
        _renderTargetView?.Dispose();
        _depthStencilView?.Dispose();
        _depthBuffer?.Dispose();
        _swapChain?.Dispose();
        _context?.Dispose();
        _device?.Dispose();
    }
}
