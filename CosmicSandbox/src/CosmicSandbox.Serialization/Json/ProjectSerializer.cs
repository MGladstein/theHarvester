using CosmicSandbox.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CosmicSandbox.Serialization.Json;

/// <summary>
/// Serializes and deserializes simulation projects to/from JSON
/// </summary>
public class ProjectSerializer
{
    private readonly JsonSerializerOptions _options;

    public ProjectSerializer()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
    }

    /// <summary>
    /// Save simulation state to file
    /// </summary>
    public async Task SaveAsync(SimulationState state, string filePath)
    {
        state.ModifiedDate = DateTime.UtcNow;

        var json = JsonSerializer.Serialize(state, _options);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// Load simulation state from file
    /// </summary>
    public async Task<SimulationState> LoadAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var state = JsonSerializer.Deserialize<SimulationState>(json, _options);

        if (state == null)
            throw new InvalidOperationException("Failed to deserialize simulation state");

        return state;
    }

    /// <summary>
    /// Auto-save with backup
    /// </summary>
    public async Task AutoSaveAsync(SimulationState state, string filePath)
    {
        // Create backup if file exists
        if (File.Exists(filePath))
        {
            var backupPath = filePath + ".backup";
            File.Copy(filePath, backupPath, true);
        }

        await SaveAsync(state, filePath);
    }

    /// <summary>
    /// Recover from auto-save backup
    /// </summary>
    public async Task<SimulationState?> RecoverAsync(string filePath)
    {
        var backupPath = filePath + ".backup";

        if (!File.Exists(backupPath))
            return null;

        try
        {
            return await LoadAsync(backupPath);
        }
        catch
        {
            return null;
        }
    }
}
