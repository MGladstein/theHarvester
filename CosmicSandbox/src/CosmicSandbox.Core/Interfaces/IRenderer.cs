using CosmicSandbox.Core.Models;

namespace CosmicSandbox.Core.Interfaces;

/// <summary>
/// Interface for rendering the simulation
/// </summary>
public interface IRenderer : IDisposable
{
    /// <summary>
    /// Initialize the renderer
    /// </summary>
    void Initialize(IntPtr windowHandle, int width, int height);

    /// <summary>
    /// Render a frame
    /// </summary>
    void Render(SimulationState state, double deltaTime);

    /// <summary>
    /// Resize the viewport
    /// </summary>
    void Resize(int width, int height);

    /// <summary>
    /// Capture current frame to image
    /// </summary>
    byte[] CaptureFrame();

    /// <summary>
    /// Update camera
    /// </summary>
    void UpdateCamera(CameraState camera);

    /// <summary>
    /// Get rendering statistics
    /// </summary>
    RenderStats GetStats();
}

public class RenderStats
{
    public int FrameCount { get; set; }
    public double FrameTime { get; set; }
    public double FPS { get; set; }
    public int DrawCalls { get; set; }
    public int TriangleCount { get; set; }
    public long VideoMemoryUsage { get; set; }
}
