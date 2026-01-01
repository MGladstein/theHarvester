# Cosmic Sandbox

**Production-Ready Windows 11 Gravitational Simulator with 3D Visualization**

Cosmic Sandbox is a sophisticated desktop application for simulating and visualizing realistic gravitational systems in 3D. Built with DirectX 11, .NET 8.0, and using exclusively public-domain NASA/ESA astronomical assets.

![Cosmic Sandbox](docs/images/screenshot.png)

## Features

### Physics Simulation
- **N-body gravitational simulation** with accurate Newtonian mechanics
- **Multiple integrators**:
  - Velocity Verlet (symplectic, recommended)
  - Leapfrog (symplectic)
  - Runge-Kutta 4 (high precision)
- **Collision detection and resolution**:
  - Merge bodies with momentum conservation
  - Elastic/inelastic bouncing
  - Destruction mode
- **Configurable time scaling** (1x to 1000x+)
- **Energy conservation tracking** and drift warnings
- **Adaptive timestep recommendations** for stability

### 3D Rendering
- **DirectX 11** hardware-accelerated rendering
- **Physically Based Rendering (PBR)** for realistic materials
- **HDR lighting** with tone mapping and gamma correction
- **Dynamic skyboxes** from NASA/ESA deep space imagery
- **Particle systems** for comets, dust, and stellar phenomena
- **Real-time camera controls** with follow mode

### Asset Pipeline
- **NASA/ESA public-domain imagery processing**:
  - Planetary surface maps
  - Star and nebula textures
  - Deep space skyboxes
- **Automatic texture processing**:
  - Seam removal for spherical projection
  - Normal map generation
  - LOD generation
  - Cubemap creation from equirectangular images
- **Procedural generation**:
  - Rocky surfaces
  - Asteroid textures
  - Gas giant bands
  - Stellar surfaces

### User Interface
- **Modern Windows 11 Fluent Design** interface
- **Three-panel layout**:
  - Left: Object library and presets
  - Center: 3D viewport with controls
  - Right: Property inspector
- **Real-time statistics** (FPS, body count, energy)
- **Interactive object manipulation**
- **Preset scenes** (Solar System, Binary Stars, etc.)

### Data Management
- **JSON project format** for easy sharing
- **Auto-save with recovery**
- **Export capabilities**:
  - PNG screenshots
  - MP4 video recordings (planned)
  - CSV physics data for analysis
- **Deterministic simulation** option for reproducibility

## System Requirements

### Minimum
- **OS**: Windows 11 (22H2 or later)
- **CPU**: Intel Core i5 / AMD Ryzen 5 or equivalent
- **RAM**: 8 GB
- **GPU**: DirectX 11 compatible with 2 GB VRAM
- **Storage**: 2 GB available space

### Recommended
- **OS**: Windows 11 (23H2 or later)
- **CPU**: Intel Core i7 / AMD Ryzen 7 or equivalent
- **RAM**: 16 GB
- **GPU**: DirectX 11 compatible with 4+ GB VRAM
- **Storage**: 5 GB available space (for additional assets)

## Installation

### Option 1: Installer (Recommended)
1. Download `CosmicSandboxInstaller.exe` from the [Releases](https://github.com/cosmicsandbox/releases) page
2. Run the installer
3. Follow the installation wizard
4. Launch from Start Menu or Desktop shortcut

### Option 2: Portable
1. Download `CosmicSandbox-Portable.zip`
2. Extract to desired location
3. Run `CosmicSandbox.exe`

### Option 3: Build from Source
See [Building from Source](#building-from-source) section below.

## Quick Start

1. **Launch the application**
2. **Load a preset scene**:
   - Click "Solar System" for familiar planetary orbits
   - Click "Binary Stars" for stellar dynamics
   - Click "Three-Body Problem" for chaotic motion
3. **Press Play** (▶ button or Space key)
4. **Adjust time scale** with the slider
5. **Navigate** the 3D view:
   - Left-click + drag: Rotate camera
   - Right-click + drag: Pan camera
   - Scroll: Zoom in/out

## Usage Guide

### Creating a Simulation

#### Adding Bodies
1. Click the "+" buttons in the left panel for quick additions
2. Or use **Simulation > Add Body** for detailed configuration
3. Configure properties in the right Inspector panel:
   - Physical properties (mass, radius, density)
   - Position and velocity vectors
   - Visual properties (textures, colors)
   - Special properties (emissive for stars, atmosphere)

#### Using Textures
All textures must come from approved public-domain sources:
- **NASA Image and Video Library**: https://images.nasa.gov
- **NASA PDS**: https://pds.nasa.gov
- **ESA Hubble**: https://esahubble.org/images/
- **James Webb Space Telescope**: https://webbtelescope.org/resource-gallery/images

Import textures via:
1. Place raw images in `Assets/Raw/`
2. Run the asset processor (built-in)
3. Select processed texture in Inspector

### Simulation Controls

| Action | Keyboard | UI |
|--------|----------|-----|
| Play/Pause | Space | ▶ Button |
| Reset | R | ⏹ Button |
| Increase Speed | ] | Time Scale Slider |
| Decrease Speed | [ | Time Scale Slider |
| Focus Body | F | Right-click body |
| Delete Body | Delete | Select + Delete |

### Physics Settings

#### Integrator Selection
- **Velocity Verlet**: Best for long-term accuracy, energy conservation
- **Leapfrog**: Excellent for stable orbits, similar to Verlet
- **Runge-Kutta 4**: High precision for short simulations

#### Timestep Configuration
- Smaller timestep = more accurate but slower
- Larger timestep = faster but may be unstable
- Watch for energy drift warnings
- Use recommended timestep from Inspector

#### Collision Modes
- **Merge**: Bodies combine (conserves momentum)
- **Bounce**: Elastic/inelastic collisions
- **Destroy**: Both bodies removed
- **None**: Bodies pass through each other

### Exporting Data

#### Screenshots
**File > Export > Export Image**
- Captures current viewport
- Saves as PNG with timestamp

#### Video Recording
**File > Export > Export Video** (Planned)
- Records simulation playback
- Configurable framerate and resolution
- Saves as MP4

#### Physics Data
**File > Export > Export Data (CSV)**
- Exports time-series data
- Includes: positions, velocities, energies
- Compatible with Excel, Python, MATLAB

## Asset Guidelines

### Mandatory Asset Sourcing Rules

**ALL** graphical assets MUST come from:

1. **NASA Image and Video Library** (Public Domain)
   - Planetary maps, solar imagery, deep space photos

2. **NASA Planetary Data System** (Public Domain)
   - Raw scientific data, height maps, albedo maps

3. **ESA Public Archives** (Confirm license per asset)
   - Hubble, Gaia, Mars Express imagery

4. **JWST Public Gallery** (Public Domain)
   - Deep field images, nebulae, galaxies

5. **Celestia Motherlode** (Public Use)
   - Community-contributed planetary textures

**PROHIBITED**:
- ❌ Commercial texture packs
- ❌ Attribution-required Creative Commons
- ❌ AI-generated imagery (unless generated within app)
- ❌ Copyrighted astronomical visualizations

### Asset Processing Workflow

1. **Download** raw imagery from approved sources
2. **Document** source with metadata (mission, date, wavelength)
3. **Process** using built-in pipeline:
   - Resize to target resolution
   - Remove seams for wrapping
   - Generate normal maps
   - Create LODs
4. **Apply** to celestial bodies
5. **Include** source metadata in project file

## Building from Source

### Prerequisites

1. **Visual Studio 2022** (17.8 or later)
   - .NET desktop development workload
   - Windows 11 SDK (10.0.22621.0 or later)

2. **.NET 8.0 SDK**
   ```bash
   winget install Microsoft.DotNet.SDK.8
   ```

3. **Git**
   ```bash
   winget install Git.Git
   ```

### Build Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/cosmicsandbox/cosmicsandbox.git
   cd cosmicsandbox
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore CosmicSandbox.sln
   ```

3. **Build solution**
   ```bash
   dotnet build CosmicSandbox.sln --configuration Release
   ```

4. **Run tests**
   ```bash
   dotnet test tests/CosmicSandbox.Tests/CosmicSandbox.Tests.csproj
   ```

5. **Run application**
   ```bash
   dotnet run --project src/CosmicSandbox.App/CosmicSandbox.App.csproj
   ```

### Build Scripts

#### Windows (PowerShell)
```powershell
.\build\build.ps1 -Configuration Release
```

#### Alternative: Use Visual Studio
1. Open `CosmicSandbox.sln`
2. Set build configuration to **Release**
3. Build > Build Solution (Ctrl+Shift+B)
4. Run with F5

## Architecture

### Project Structure

```
CosmicSandbox/
├── src/
│   ├── CosmicSandbox.App/          # WPF application
│   ├── CosmicSandbox.Core/         # Core types and interfaces
│   ├── CosmicSandbox.PhysicsEngine/  # N-body simulation
│   ├── CosmicSandbox.Renderer/     # DirectX 11 rendering
│   ├── CosmicSandbox.AssetPipeline/  # Asset processing
│   ├── CosmicSandbox.Serialization/  # Save/load/export
│   └── CosmicSandbox.Diagnostics/  # Logging and crash reporting
├── tests/
│   └── CosmicSandbox.Tests/        # Unit tests
├── assets/
│   ├── Raw/                        # Original NASA/ESA imagery
│   ├── Processed/                  # Game-ready assets
│   └── Scenes/                     # Sample simulations
├── docs/                           # Documentation
├── build/                          # Build scripts
└── installer/                      # Installer configuration
```

### Module Dependencies

```
App
 ├─> Core
 ├─> PhysicsEngine ──> Core
 ├─> Renderer ──> Core
 ├─> AssetPipeline ──> Core
 ├─> Serialization ──> Core, PhysicsEngine
 └─> Diagnostics ──> Core
```

## Development

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test
dotnet test --filter "FullyQualifiedName~VelocityVerletIntegrator"
```

### Debugging

1. Open solution in Visual Studio
2. Set `CosmicSandbox.App` as startup project
3. Press F5 to debug
4. Logs are written to: `%LOCALAPPDATA%\CosmicSandbox\Logs\`

### Contributing

This is an example project demonstrating production-ready architecture.

For improvements:
1. Fork the repository
2. Create a feature branch
3. Make changes with tests
4. Submit pull request

## Performance Optimization

### For Large Simulations (100+ bodies)

1. **Reduce visual quality**:
   - Lower texture resolutions
   - Disable particle effects
   - Reduce shadow quality

2. **Optimize physics**:
   - Use Leapfrog integrator (fastest)
   - Increase timestep (watch energy drift)
   - Disable orbit trails

3. **Hardware acceleration**:
   - Ensure discrete GPU is used (not integrated)
   - Close other GPU-intensive applications
   - Update graphics drivers

## Troubleshooting

### Application won't start
- **Check DirectX**: Run `dxdiag` to verify DirectX 11 support
- **Update GPU drivers**: Download latest from manufacturer
- **Check logs**: `%LOCALAPPDATA%\CosmicSandbox\Logs\`

### Simulation is unstable
- **Reduce timestep**: Use Inspector to lower dt
- **Change integrator**: Switch to Velocity Verlet or Leapfrog
- **Check initial conditions**: Verify bodies aren't overlapping

### Low FPS
- **Reduce body count**: Start with fewer objects
- **Lower resolution**: Use Settings > Graphics Quality
- **Disable effects**: Turn off atmosphere, particles

### Crashes
- **Check crash reports**: `%LOCALAPPDATA%\CosmicSandbox\CrashReports\`
- **Verify .NET 8.0**: Reinstall runtime if needed
- **Report issue**: Create GitHub issue with crash log

## Packaging for Distribution

### Creating Installer

1. **Build in Release mode**
   ```bash
   dotnet publish src/CosmicSandbox.App/CosmicSandbox.App.csproj `
       -c Release `
       -r win-x64 `
       --self-contained true `
       -p:PublishSingleFile=true `
       -p:PublishTrimmed=true
   ```

2. **Bundle assets**
   - Copy `assets/Processed/` to publish folder
   - Include sample scenes

3. **Create installer** (using WiX Toolset or Inno Setup)
   - See `installer/setup.iss` for configuration

4. **Sign executable** (for production)
   ```bash
   signtool sign /f certificate.pfx /p password /t http://timestamp.digicert.com CosmicSandbox.exe
   ```

## License

This software is provided as-is for educational and non-commercial use.

**Asset Licenses**:
- All bundled textures are from NASA/ESA public domain sources
- See `assets/ATTRIBUTION.md` for detailed source information
- Users must comply with original source licenses when adding assets

## Credits

### Development
- Physics Engine: Classical N-body gravitational dynamics
- Rendering: DirectX 11 with PBR shaders
- Architecture: Modular .NET design

### Assets
- Planetary textures: NASA/JPL, NASA PDS
- Deep space imagery: NASA/ESA Hubble, JWST
- Solar imagery: NASA/SDO

### Libraries
- SharpDX: DirectX wrapper
- ImageSharp: Image processing
- Serilog: Logging framework
- ModernWPF: UI framework

## Support

- **Documentation**: [docs/](docs/)
- **Issues**: [GitHub Issues](https://github.com/cosmicsandbox/issues)
- **Discussions**: [GitHub Discussions](https://github.com/cosmicsandbox/discussions)

## Version History

### 1.0.0 (2026-01-01)
- Initial release
- Full N-body physics simulation
- DirectX 11 rendering
- NASA/ESA asset pipeline
- Preset scenes
- Export to PNG/CSV

---

**Built with DirectX 11, .NET 8.0, and NASA public-domain imagery**
