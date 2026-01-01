# Cosmic Sandbox Architecture

This document describes the software architecture of Cosmic Sandbox, a production-ready Windows 11 gravitational simulator.

## Overview

Cosmic Sandbox follows a **modular layered architecture** with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│         WPF Application Layer           │
│         (CosmicSandbox.App)             │
├─────────────────────────────────────────┤
│                                         │
│  ┌──────────┐  ┌──────────┐  ┌────────┤
│  │ Physics  │  │ Renderer │  │ Assets ││
│  │ Engine   │  │          │  │Pipeline││
│  └──────────┘  └──────────┘  └────────┤
│                                         │
├─────────────────────────────────────────┤
│  Serialization  │  Diagnostics          │
├─────────────────────────────────────────┤
│         Core Types & Interfaces         │
│         (CosmicSandbox.Core)            │
└─────────────────────────────────────────┘
```

## Module Breakdown

### 1. CosmicSandbox.Core

**Purpose**: Foundation layer with shared types, interfaces, and constants.

**Key Components**:
- `Vector3D`: High-precision 3D vector for astronomical calculations
- `CelestialBody`: Represents a physical object in space
- `SimulationState`: Complete state snapshot
- `PhysicsConstants`: Physical constants (G, c, AU, etc.)
- Interfaces: `IIntegrator`, `IRenderer`, `IAssetLoader`

**Dependencies**: None (self-contained)

**Design Pattern**: Data Transfer Objects (DTOs), Value Objects

### 2. CosmicSandbox.PhysicsEngine

**Purpose**: N-body gravitational simulation with multiple numerical integrators.

**Key Components**:

#### Integrators
- `VelocityVerletIntegrator`: Symplectic, 2nd-order accuracy
- `LeapfrogIntegrator`: Symplectic, excellent energy conservation
- `RungeKutta4Integrator`: Non-symplectic, 4th-order accuracy

#### Collision System
- `CollisionDetector`: Detects overlapping bodies
- Collision modes: Merge, Bounce, Destroy

#### Simulation Controller
- `SimulationController`: Main simulation orchestrator
- Energy tracking and drift warnings
- Timestep stability checks

**Dependencies**: `CosmicSandbox.Core`

**Design Patterns**:
- Strategy Pattern (integrator selection)
- Observer Pattern (collision/warning events)

**Performance Optimizations**:
- O(N²) pairwise force calculation
- Softening length to avoid singularities
- Inline methods for vector operations

### 3. CosmicSandbox.Renderer

**Purpose**: Hardware-accelerated 3D rendering with DirectX 11.

**Key Components**:

#### DirectX Integration
- `D3D11Renderer`: Main renderer implementation
- Device management and swap chain
- Render target and depth buffer management

#### Shaders (HLSL)
- **Vertex Shaders**: Transform geometry, calculate TBN matrix
- **Pixel Shaders**: PBR lighting, HDR tone mapping, gamma correction

#### Rendering Features
- Physically Based Rendering (PBR)
- Normal mapping
- HDR lighting with bloom
- Skybox rendering
- Particle systems (planned)

**Dependencies**:
- `CosmicSandbox.Core`
- SharpDX (DirectX wrapper)

**Design Patterns**:
- Facade Pattern (DirectX complexity hidden)
- Resource Management (IDisposable)

**Performance Optimizations**:
- LOD (Level of Detail) system
- Frustum culling
- Instanced rendering for particles

### 4. CosmicSandbox.AssetPipeline

**Purpose**: Process NASA/ESA public-domain imagery into game-ready assets.

**Key Components**:

#### Importers
- `NASAImageImporter`: Import and process raw astronomical imagery
  - Equirectangular to cubemap conversion
  - Seam removal for spherical wrapping
  - Normal map generation
  - Metadata preservation

#### Generators
- `ProceduralTextureGenerator`: Create procedural surfaces
  - Multi-octave Perlin noise
  - Crater generation
  - Gas giant banding
  - Rocky surface variation

#### Asset Processing Pipeline
```
Raw Image → Resize → Correct Distortion → Remove Seams
          ↓
    Generate Normal Map → Optimize → Cache
```

**Dependencies**:
- `CosmicSandbox.Core`
- SixLabors.ImageSharp (image processing)

**Design Patterns**:
- Pipeline Pattern (processing stages)
- Factory Pattern (texture generation)
- Repository Pattern (metadata cache)

### 5. CosmicSandbox.Serialization

**Purpose**: Persist simulation state and export data.

**Key Components**:

#### Project Serialization
- `ProjectSerializer`: JSON serialization with System.Text.Json
- Auto-save with backup
- Recovery from corrupted saves

#### Data Export
- `DataExporter`: Export to CSV for analysis
- Time-series data export
- Physics data (positions, velocities, energies)

**Dependencies**:
- `CosmicSandbox.Core`
- `CosmicSandbox.PhysicsEngine`
- CsvHelper (CSV export)

**File Formats**:
- `.csp` (Cosmic Sandbox Project): JSON with UTF-8 encoding
- `.csv`: Standard CSV with headers

### 6. CosmicSandbox.Diagnostics

**Purpose**: Logging, crash reporting, and performance monitoring.

**Key Components**:

#### Logging
- `Logger`: Serilog-based logging
- Multiple sinks: Console, File
- Rolling file policy (7 days retention)
- Structured logging

#### Crash Reporting
- `CrashReporter`: Generate detailed crash reports
- System information gathering
- Stack trace analysis
- Report archiving

**Dependencies**:
- `CosmicSandbox.Core`
- Serilog (logging framework)

**Log Locations**:
- Application logs: `%LOCALAPPDATA%\CosmicSandbox\Logs\`
- Crash reports: `%LOCALAPPDATA%\CosmicSandbox\CrashReports\`

### 7. CosmicSandbox.App

**Purpose**: WPF desktop application with Fluent Design UI.

**Key Components**:

#### UI Structure
- `MainWindow`: Main application window
  - Three-panel layout
  - Viewport for 3D rendering
  - Inspector for property editing
  - Object library for presets

#### View Models
- MVVM pattern with CommunityToolkit.Mvvm
- Property change notifications
- Command binding

#### Controls
- Custom DirectX hosting control
- Real-time statistics overlay

**Dependencies**: All other modules

**Design Patterns**:
- MVVM (Model-View-ViewModel)
- Command Pattern (UI interactions)
- Dependency Injection (planned)

## Data Flow

### Simulation Loop

```
User Input → UI Layer → SimulationController
                              ↓
                        Integrator.Step()
                              ↓
                     Update CelestialBodies
                              ↓
                    CollisionDetector (if enabled)
                              ↓
                      Update SimulationState
                              ↓
                       Renderer.Render()
                              ↓
                        Display Frame
```

### Asset Loading

```
Raw NASA/ESA Image → NASAImageImporter
                              ↓
                     ProcessImageAsync()
                              ↓
               ┌──────────────┴──────────────┐
               ↓                             ↓
        Resize & Correct              Generate Normal Map
               ↓                             ↓
         Remove Seams                  Save to Cache
               ↓                             ↓
         Save Metadata  ←──────────────────┘
               ↓
        Return AssetHandle
```

## Threading Model

### Main Thread
- UI rendering (WPF)
- User input handling
- DirectX rendering

### Background Threads
- Physics simulation (optional async)
- Asset loading/processing
- File I/O (serialization)

### Thread Safety
- `SimulationState` is not thread-safe; use locking when accessing from multiple threads
- Asset cache uses `ConcurrentDictionary`
- Logging is thread-safe (Serilog)

## Memory Management

### Large Objects
- Texture data: Loaded on-demand, cached with weak references
- Mesh geometry: Shared instances for spheres
- Simulation history: Ring buffer with configurable size

### Disposal
- All DirectX resources implement `IDisposable`
- Asset handles track reference counts
- Automatic cleanup on application exit

## Error Handling

### Strategy
1. **Validation at Boundaries**: Validate user input in UI layer
2. **Defensive Programming**: Check for null, division by zero, etc.
3. **Graceful Degradation**: Fall back to defaults on errors
4. **User Notification**: Display meaningful error messages

### Exception Hierarchy
```
CosmicSandboxException (base)
  ├─ PhysicsException
  │   ├─ IntegrationException
  │   └─ CollisionException
  ├─ RenderException
  └─ AssetException
      ├─ AssetNotFoundException
      └─ AssetProcessingException
```

## Testing Strategy

### Unit Tests
- Physics integrators (energy conservation, accuracy)
- Collision detection (overlap, merge, bounce)
- Vector math operations
- Asset processing

### Integration Tests
- Full simulation loop
- Save/load roundtrip
- Asset pipeline end-to-end

### Performance Tests
- Large N-body systems (100+ bodies)
- Memory usage over time
- Frame rate stability

## Security Considerations

### Asset Validation
- Only load from trusted sources (NASA, ESA)
- Validate image dimensions and file sizes
- Prevent path traversal in file operations

### Sandboxing
- Application runs in user space (no admin required for operation)
- No network access required (offline-capable)
- No code execution from external sources

## Extension Points

### Custom Integrators
Implement `IIntegrator` interface:
```csharp
public class MyIntegrator : IIntegrator
{
    public string Name => "My Custom Integrator";
    public bool IsSymplectic => false;

    public void Step(SimulationState state, double dt)
    {
        // Custom integration logic
    }

    public double GetRecommendedTimestep(SimulationState state)
    {
        // Return safe timestep
    }
}
```

### Custom Renderers
Implement `IRenderer` interface for alternative backends (e.g., Vulkan, OpenGL).

### Custom Asset Processors
Extend `AssetProcessingOptions` and `NASAImageImporter` for specialized processing.

## Performance Characteristics

### Computational Complexity
- **Physics (N-body)**: O(N²) per timestep
- **Rendering**: O(N) draw calls, O(M) triangles
- **Collision Detection**: O(N²) naive, O(N log N) with spatial partitioning (planned)

### Scalability
- **Recommended**: 10-50 bodies for real-time simulation
- **Maximum**: 1000+ bodies (slow-motion or batch processing)
- **Bottleneck**: Typically rendering, not physics

## Future Enhancements

### Planned Features
1. **GPU-accelerated physics** (compute shaders)
2. **Barnes-Hut tree** for O(N log N) gravity calculation
3. **Multi-threading** for physics updates
4. **Relativistic effects** (post-Newtonian corrections)
5. **Video export** with FFmpeg integration
6. **VR support** for immersive visualization

### Architecture Implications
- GPU physics requires separate compute pipeline
- Barnes-Hut needs spatial data structure (octree)
- Multi-threading needs thread-safe state management

## References

### Physics
- Hairer, Nørsett, Wanner: "Solving Ordinary Differential Equations" (integrators)
- Press et al.: "Numerical Recipes" (N-body methods)

### Rendering
- Pharr, Jakob, Humphreys: "Physically Based Rendering"
- Luna: "Introduction to 3D Game Programming with DirectX 11"

### Architecture
- Fowler: "Patterns of Enterprise Application Architecture"
- Gamma et al.: "Design Patterns"

---

Last Updated: 2026-01-01
