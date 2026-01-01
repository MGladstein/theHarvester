namespace CosmicSandbox.Core.Interfaces;

/// <summary>
/// Interface for loading and processing assets
/// </summary>
public interface IAssetLoader
{
    /// <summary>
    /// Load a texture from file
    /// </summary>
    Task<AssetHandle> LoadTextureAsync(string path, AssetLoadOptions? options = null);

    /// <summary>
    /// Load a mesh from file
    /// </summary>
    Task<AssetHandle> LoadMeshAsync(string path, AssetLoadOptions? options = null);

    /// <summary>
    /// Process raw NASA/ESA imagery
    /// </summary>
    Task<AssetHandle> ProcessRawImageAsync(string sourcePath, AssetProcessingOptions options);

    /// <summary>
    /// Generate procedural texture
    /// </summary>
    Task<AssetHandle> GenerateProceduralTextureAsync(ProceduralTextureOptions options);

    /// <summary>
    /// Get asset metadata
    /// </summary>
    AssetMetadata? GetMetadata(string assetId);

    /// <summary>
    /// Unload asset
    /// </summary>
    void UnloadAsset(string assetId);
}

public class AssetHandle
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Path { get; set; } = string.Empty;
    public AssetType Type { get; set; }
    public object? Data { get; set; }
    public bool IsLoaded { get; set; }
}

public enum AssetType
{
    Texture2D,
    TextureCube,
    Mesh,
    Material,
    Shader
}

public class AssetLoadOptions
{
    public bool GenerateMipmaps { get; set; } = true;
    public bool Compress { get; set; } = false;
    public int MaxResolution { get; set; } = 4096;
}

public class AssetProcessingOptions
{
    public bool GenerateNormalMap { get; set; }
    public bool RemoveSeams { get; set; }
    public bool CorrectDistortion { get; set; }
    public int TargetResolution { get; set; } = 2048;
}

public class ProceduralTextureOptions
{
    public int Width { get; set; } = 512;
    public int Height { get; set; } = 512;
    public string Seed { get; set; } = Guid.NewGuid().ToString();
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class AssetMetadata
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Mission { get; set; } = string.Empty;
    public DateTime? CaptureDate { get; set; }
    public string License { get; set; } = "Public Domain";
    public Dictionary<string, string> Properties { get; set; } = new();
}
