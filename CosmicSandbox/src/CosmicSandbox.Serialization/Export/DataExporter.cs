using CosmicSandbox.Core.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace CosmicSandbox.Serialization.Export;

/// <summary>
/// Exports simulation data to various formats
/// </summary>
public class DataExporter
{
    /// <summary>
    /// Export simulation data to CSV
    /// </summary>
    public async Task ExportToCsvAsync(SimulationState state, string filePath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        };

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, config);

        // Write header
        csv.WriteField("Time");
        csv.WriteField("BodyName");
        csv.WriteField("Mass");
        csv.WriteField("PositionX");
        csv.WriteField("PositionY");
        csv.WriteField("PositionZ");
        csv.WriteField("VelocityX");
        csv.WriteField("VelocityY");
        csv.WriteField("VelocityZ");
        csv.WriteField("AccelerationX");
        csv.WriteField("AccelerationY");
        csv.WriteField("AccelerationZ");
        csv.WriteField("KineticEnergy");
        await csv.NextRecordAsync();

        // Write data for each body
        foreach (var body in state.Bodies.Where(b => b.IsActive))
        {
            csv.WriteField(state.CurrentTime);
            csv.WriteField(body.Name);
            csv.WriteField(body.Mass);
            csv.WriteField(body.Position.X);
            csv.WriteField(body.Position.Y);
            csv.WriteField(body.Position.Z);
            csv.WriteField(body.Velocity.X);
            csv.WriteField(body.Velocity.Y);
            csv.WriteField(body.Velocity.Z);
            csv.WriteField(body.Acceleration.X);
            csv.WriteField(body.Acceleration.Y);
            csv.WriteField(body.Acceleration.Z);

            double kineticEnergy = 0.5 * body.Mass * body.Velocity.LengthSquared();
            csv.WriteField(kineticEnergy);

            await csv.NextRecordAsync();
        }
    }

    /// <summary>
    /// Export time series data
    /// </summary>
    public async Task ExportTimeSeriesAsync(
        List<SimulationState> snapshots,
        string filePath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        };

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, config);

        // Write header
        csv.WriteField("Time");
        csv.WriteField("TotalEnergy");
        csv.WriteField("KineticEnergy");
        csv.WriteField("PotentialEnergy");
        csv.WriteField("EnergyDrift");
        csv.WriteField("CenterOfMassX");
        csv.WriteField("CenterOfMassY");
        csv.WriteField("CenterOfMassZ");
        csv.WriteField("ActiveBodies");
        await csv.NextRecordAsync();

        foreach (var snapshot in snapshots)
        {
            csv.WriteField(snapshot.CurrentTime);
            csv.WriteField(snapshot.GetTotalEnergy());
            csv.WriteField(snapshot.GetKineticEnergy());
            csv.WriteField(snapshot.GetPotentialEnergy());
            csv.WriteField(0.0); // Energy drift calculation would need reference
            var com = snapshot.GetCenterOfMass();
            csv.WriteField(com.X);
            csv.WriteField(com.Y);
            csv.WriteField(com.Z);
            csv.WriteField(snapshot.Bodies.Count(b => b.IsActive));
            await csv.NextRecordAsync();
        }
    }
}
