# Asset Attribution and Sources

All graphical assets used in Cosmic Sandbox are sourced from public-domain or permissive-use repositories. This document tracks the source and licensing of all bundled assets.

## Mandatory Compliance

**ALL** users must source assets from the following approved categories only:

1. NASA Image and Video Library (Public Domain)
2. NASA Planetary Data System (Public Domain)
3. ESA Public Archives (with verified permissions)
4. Hubble Space Telescope Public Gallery (Public Domain)
5. James Webb Space Telescope Public Gallery (Public Domain)
6. Celestia Motherlode (Community Public Use)

## Prohibited Sources

❌ Commercial texture marketplaces
❌ Attribution-required Creative Commons (CC-BY, CC-BY-SA, etc.)
❌ AI-generated imagery (unless generated locally within app)
❌ Copyrighted visualizations from observatories
❌ Third-party rendered astronomical art

## Current Bundled Assets

### Planetary Textures

#### Earth
- **Source**: NASA Visible Earth
- **Mission**: NASA Blue Marble
- **URL**: https://visibleearth.nasa.gov/images/73909/december-blue-marble-next-generation-w-topography-and-bathymetry
- **License**: Public Domain (NASA)
- **Processing**: Resized to 4096x2048, seam blending applied
- **Files**:
  - `earth_albedo.png`
  - `earth_normal.png` (generated)

#### Mars
- **Source**: NASA PDS Mars Reconnaissance Orbiter
- **Instrument**: HiRISE / Context Camera
- **URL**: https://pds-imaging.jpl.nasa.gov/portal/mars_orbiter_mission.html
- **License**: Public Domain (NASA)
- **Processing**: Global mosaic, color correction, 2048x1024
- **Files**:
  - `mars_albedo.png`
  - `mars_height.png`

#### Moon
- **Source**: NASA Lunar Reconnaissance Orbiter
- **Mission**: LRO LOLA/LROC
- **URL**: https://svs.gsfc.nasa.gov/4720
- **License**: Public Domain (NASA)
- **Processing**: Equirectangular projection, 2048x1024
- **Files**:
  - `moon_albedo.png`

### Solar Textures

#### Sun Surface
- **Source**: NASA Solar Dynamics Observatory
- **Instrument**: AIA 193Å / 171Å / 304Å composite
- **URL**: https://sdo.gsfc.nasa.gov/data/
- **License**: Public Domain (NASA)
- **Processing**: Composite RGB from multiple wavelengths, 2048x2048
- **Files**:
  - `sun_surface.png`

### Deep Space Skyboxes

#### Hubble Deep Field
- **Source**: NASA/ESA Hubble Space Telescope
- **Collection**: Hubble Ultra Deep Field (HUDF)
- **URL**: https://esahubble.org/images/heic0611b/
- **License**: Public Domain (NASA/ESA)
- **Processing**: Equirectangular to cubemap, HDR tone mapping
- **Files**:
  - `skybox_hubble_*.png` (6 faces)

#### JWST Carina Nebula
- **Source**: NASA James Webb Space Telescope
- **Instrument**: NIRCam
- **URL**: https://webbtelescope.org/contents/media/images/2022/031/01G77PKB8NKR7S8Z6HBXMYATGJ
- **License**: Public Domain (NASA/ESA/CSA)
- **Processing**: False color composite, cubemap generation
- **Files**:
  - `skybox_carina_*.png` (6 faces)

### Asteroid and Small Body Textures

#### Generic Rocky Surface
- **Method**: Procedurally generated from noise
- **Base Reference**: NASA Dawn mission Vesta imagery
- **URL**: https://sbn.psi.edu/pds/resource/dawn/
- **License**: Procedural (no direct texture used)
- **Files**:
  - `asteroid_generic_01.png` through `asteroid_generic_05.png`

## Adding Your Own Assets

### Step 1: Verify Source Eligibility

Before adding any asset, verify it meets **all** criteria:

1. ✅ From approved source category (NASA, ESA, etc.)
2. ✅ Explicitly public domain OR permissive license
3. ✅ No attribution requirements in license
4. ✅ Not AI-generated (unless generated in-app)
5. ✅ Original scientific imagery, not artist renderings

### Step 2: Download and Document

1. Download highest resolution available
2. Save to `assets/Raw/`
3. Record complete metadata:
   - Source organization
   - Mission/instrument name
   - Capture date
   - Original URL
   - Wavelength(s) if applicable
   - License text

### Step 3: Process

Use the built-in asset pipeline:

```csharp
var importer = new NASAImageImporter("assets/Raw", "assets/Processed");
var metadata = new AssetMetadata
{
    Name = "Jupiter",
    Source = "NASA Juno",
    Mission = "Juno",
    CaptureDate = new DateTime(2021, 7, 21),
    License = "Public Domain"
};

await importer.ImportImageAsync(
    "raw_jupiter.png",
    metadata,
    new AssetProcessingOptions
    {
        TargetResolution = 2048,
        GenerateNormalMap = true,
        RemoveSeams = true
    }
);
```

### Step 4: Update This File

Add entry to appropriate section above with:
- Asset name
- Complete source information
- Direct URL to source page
- License confirmation
- Processing steps applied
- Output file names

## Verification Checklist

Before distributing projects with custom assets:

- [ ] All assets from approved sources
- [ ] Source URLs documented
- [ ] Licenses verified as public domain or permissive
- [ ] No commercial marketplace assets
- [ ] No attribution-required CC licenses
- [ ] Metadata files included
- [ ] This ATTRIBUTION.md updated

## License Compliance

**NASA Policy**: All NASA imagery is in the public domain unless otherwise noted. However, you must verify each image individually as some NASA-published images may include data from international partners with different licensing.

**ESA Policy**: ESA imagery is typically CC BY-SA 3.0 IGO, which **requires attribution**. Only use ESA assets explicitly marked as public domain or with permission for attribution-free use.

**JWST/Hubble**: Most imagery is public domain, but always verify on the source page.

## Reporting Issues

If you find:
- An asset that doesn't meet sourcing requirements
- Incorrect attribution
- Broken source URLs
- License violations

Please report immediately to: [project maintainer contact]

## Last Updated

2026-01-01

---

**Remember**: Incorrect asset sourcing violates the core principles of this project. When in doubt, use procedural generation or find alternative public-domain sources.
