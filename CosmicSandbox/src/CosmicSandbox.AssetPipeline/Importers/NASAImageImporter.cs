using CosmicSandbox.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Text.Json;

namespace CosmicSandbox.AssetPipeline.Importers;

/// <summary>
/// Imports and processes NASA/ESA public domain imagery
/// </summary>
public class NASAImageImporter
{
    private readonly string _rawAssetsPath;
    private readonly string _processedAssetsPath;
    private readonly Dictionary<string, AssetMetadata> _metadataCache = new();

    public NASAImageImporter(string rawPath, string processedPath)
    {
        _rawAssetsPath = rawPath;
        _processedAssetsPath = processedPath;

        Directory.CreateDirectory(rawPath);
        Directory.CreateDirectory(processedPath);
    }

    /// <summary>
    /// Import a raw NASA image with metadata
    /// </summary>
    public async Task<AssetHandle> ImportImageAsync(
        string sourcePath,
        AssetMetadata metadata,
        AssetProcessingOptions? options = null)
    {
        options ??= new AssetProcessingOptions();

        var assetId = Guid.NewGuid().ToString();
        var extension = Path.GetExtension(sourcePath);

        // Copy raw asset
        var rawDestPath = Path.Combine(_rawAssetsPath, assetId + extension);
        File.Copy(sourcePath, rawDestPath, true);

        // Save metadata
        metadata.Id = assetId;
        await SaveMetadataAsync(metadata);

        // Process image
        var processedPath = await ProcessImageAsync(rawDestPath, assetId, options);

        return new AssetHandle
        {
            Id = assetId,
            Path = processedPath,
            Type = AssetType.Texture2D,
            IsLoaded = true
        };
    }

    /// <summary>
    /// Process raw image into game-ready format
    /// </summary>
    private async Task<string> ProcessImageAsync(
        string sourcePath,
        string assetId,
        AssetProcessingOptions options)
    {
        using var image = await Image.LoadAsync<Rgba32>(sourcePath);

        // Resize if needed
        if (image.Width > options.TargetResolution || image.Height > options.TargetResolution)
        {
            var aspectRatio = (double)image.Width / image.Height;
            int newWidth, newHeight;

            if (aspectRatio > 1)
            {
                newWidth = options.TargetResolution;
                newHeight = (int)(options.TargetResolution / aspectRatio);
            }
            else
            {
                newHeight = options.TargetResolution;
                newWidth = (int)(options.TargetResolution * aspectRatio);
            }

            image.Mutate(x => x.Resize(newWidth, newHeight));
        }

        // Apply processing
        if (options.RemoveSeams)
        {
            RemoveSeams(image);
        }

        if (options.CorrectDistortion)
        {
            CorrectSphericalDistortion(image);
        }

        // Save processed image
        var processedPath = Path.Combine(_processedAssetsPath, $"{assetId}.png");
        await image.SaveAsPngAsync(processedPath);

        // Generate normal map if requested
        if (options.GenerateNormalMap)
        {
            var normalMapPath = Path.Combine(_processedAssetsPath, $"{assetId}_normal.png");
            await GenerateNormalMapAsync(image, normalMapPath);
        }

        return processedPath;
    }

    /// <summary>
    /// Remove visible seams from wrapped planetary textures
    /// </summary>
    private void RemoveSeams(Image<Rgba32> image)
    {
        // Blend edges for seamless wrapping
        int blendWidth = 4;

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < blendWidth; x++)
            {
                float blend = (float)x / blendWidth;

                var leftPixel = image[x, y];
                var rightPixel = image[image.Width - blendWidth + x, y];

                // Blend left and right edges
                var blendedR = (byte)(leftPixel.R * (1 - blend) + rightPixel.R * blend);
                var blendedG = (byte)(leftPixel.G * (1 - blend) + rightPixel.G * blend);
                var blendedB = (byte)(leftPixel.B * (1 - blend) + rightPixel.B * blend);
                var blendedA = (byte)(leftPixel.A * (1 - blend) + rightPixel.A * blend);

                var blended = new Rgba32(blendedR, blendedG, blendedB, blendedA);

                image[x, y] = blended;
                image[image.Width - blendWidth + x, y] = blended;
            }
        }
    }

    /// <summary>
    /// Correct distortion for equirectangular projections
    /// </summary>
    private void CorrectSphericalDistortion(Image<Rgba32> image)
    {
        // Simple correction - more advanced methods could be implemented
        // This is a placeholder for proper equirectangular correction
        image.Mutate(x => x.GaussianBlur(0.5f));
    }

    /// <summary>
    /// Generate normal map from heightmap or albedo
    /// </summary>
    private async Task GenerateNormalMapAsync(Image<Rgba32> source, string outputPath)
    {
        var normalMap = new Image<Rgba32>(source.Width, source.Height);

        float strength = 2.0f;

        for (int y = 1; y < source.Height - 1; y++)
        {
            for (int x = 1; x < source.Width - 1; x++)
            {
                // Sample neighboring pixels
                float tl = GetLuminance(source[x - 1, y - 1]);
                float t = GetLuminance(source[x, y - 1]);
                float tr = GetLuminance(source[x + 1, y - 1]);
                float l = GetLuminance(source[x - 1, y]);
                float r = GetLuminance(source[x + 1, y]);
                float bl = GetLuminance(source[x - 1, y + 1]);
                float b = GetLuminance(source[x, y + 1]);
                float br = GetLuminance(source[x + 1, y + 1]);

                // Sobel filter
                float dx = (tr + 2 * r + br) - (tl + 2 * l + bl);
                float dy = (bl + 2 * b + br) - (tl + 2 * t + tr);

                // Calculate normal
                float nx = -dx * strength;
                float ny = -dy * strength;
                float nz = 1.0f;

                // Normalize
                float length = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
                nx /= length;
                ny /= length;
                nz /= length;

                // Convert to color (0-1 range to 0-255)
                byte r_byte = (byte)((nx * 0.5f + 0.5f) * 255);
                byte g_byte = (byte)((ny * 0.5f + 0.5f) * 255);
                byte b_byte = (byte)((nz * 0.5f + 0.5f) * 255);

                normalMap[x, y] = new Rgba32(r_byte, g_byte, b_byte, 255);
            }
        }

        await normalMap.SaveAsPngAsync(outputPath);
    }

    private float GetLuminance(Rgba32 color)
    {
        return 0.299f * color.R + 0.587f * color.G + 0.114f * color.B;
    }

    /// <summary>
    /// Create a cubemap skybox from equirectangular image
    /// </summary>
    public async Task<string[]> CreateCubemapAsync(string equirectangularPath, int faceSize = 2048)
    {
        using var source = await Image.LoadAsync<Rgba32>(equirectangularPath);

        var faces = new[] { "right", "left", "top", "bottom", "front", "back" };
        var facePaths = new string[6];

        for (int i = 0; i < 6; i++)
        {
            var face = GenerateCubeFace(source, i, faceSize);
            var facePath = Path.Combine(_processedAssetsPath, $"skybox_{faces[i]}.png");
            await face.SaveAsPngAsync(facePath);
            facePaths[i] = facePath;
            face.Dispose();
        }

        return facePaths;
    }

    private Image<Rgba32> GenerateCubeFace(Image<Rgba32> equirect, int faceIndex, int size)
    {
        var face = new Image<Rgba32>(size, size);

        // Simplified cubemap generation
        // Full implementation would properly map equirectangular to cube faces
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Map cube face coordinates to equirectangular
                float u = (float)x / size;
                float v = (float)y / size;

                // This is a simplified mapping - proper implementation would
                // calculate 3D direction vector and convert to lat/long
                int srcX = (int)((u + faceIndex / 6.0f) * equirect.Width) % equirect.Width;
                int srcY = (int)(v * equirect.Height);

                face[x, y] = equirect[srcX, srcY];
            }
        }

        return face;
    }

    private async Task SaveMetadataAsync(AssetMetadata metadata)
    {
        var metadataPath = Path.Combine(_rawAssetsPath, $"{metadata.Id}_metadata.json");
        var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        await File.WriteAllTextAsync(metadataPath, json);
        _metadataCache[metadata.Id] = metadata;
    }

    public async Task<AssetMetadata?> LoadMetadataAsync(string assetId)
    {
        if (_metadataCache.TryGetValue(assetId, out var cached))
            return cached;

        var metadataPath = Path.Combine(_rawAssetsPath, $"{assetId}_metadata.json");
        if (!File.Exists(metadataPath))
            return null;

        var json = await File.ReadAllTextAsync(metadataPath);
        var metadata = JsonSerializer.Deserialize<AssetMetadata>(json);

        if (metadata != null)
            _metadataCache[assetId] = metadata;

        return metadata;
    }
}
