using CosmicSandbox.Core.Math;

namespace CosmicSandbox.Core.Models;

/// <summary>
/// Represents a celestial body in the simulation
/// </summary>
public class CelestialBody
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Unnamed Body";

    // Physical properties
    public double Mass { get; set; } // kg
    public double Radius { get; set; } // meters
    public double Density { get; set; } // kg/m³

    // Kinematic state
    public Vector3D Position { get; set; }
    public Vector3D Velocity { get; set; }
    public Vector3D Acceleration { get; set; }

    // Orientation (for rotation)
    public Vector3D RotationAxis { get; set; } = Vector3D.UnitY;
    public double RotationPeriod { get; set; } // seconds
    public double CurrentRotation { get; set; } // radians

    // Visual properties
    public BodyType Type { get; set; } = BodyType.Planet;
    public string? TexturePath { get; set; }
    public string? NormalMapPath { get; set; }
    public bool HasAtmosphere { get; set; }
    public Vector3D AtmosphereColor { get; set; }
    public double AtmosphereHeight { get; set; }

    // Emission (for stars)
    public bool IsEmissive { get; set; }
    public Vector3D EmissiveColor { get; set; }
    public double Luminosity { get; set; } // Watts
    public double Temperature { get; set; } // Kelvin

    // Metadata
    public string? Description { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();

    // Runtime state
    public bool IsActive { get; set; } = true;
    public bool IsFixed { get; set; } = false; // For immovable objects

    /// <summary>
    /// Calculate gravitational force exerted by another body
    /// </summary>
    public Vector3D CalculateGravitationalForce(CelestialBody other, double gravitationalConstant)
    {
        if (other == this || !other.IsActive) return Vector3D.Zero;

        Vector3D direction = other.Position - Position;
        double distanceSquared = direction.LengthSquared();

        // Avoid singularities
        if (distanceSquared < 1e-10) return Vector3D.Zero;

        double forceMagnitude = gravitationalConstant * Mass * other.Mass / distanceSquared;
        return direction.Normalize() * forceMagnitude;
    }

    /// <summary>
    /// Calculate orbital velocity for circular orbit at current distance from another body
    /// </summary>
    public static double CalculateOrbitalVelocity(double mass, double distance, double gravitationalConstant)
    {
        return System.Math.Sqrt(gravitationalConstant * mass / distance);
    }

    /// <summary>
    /// Calculate escape velocity from this body's surface
    /// </summary>
    public double CalculateEscapeVelocity(double gravitationalConstant)
    {
        return System.Math.Sqrt(2.0 * gravitationalConstant * Mass / Radius);
    }

    /// <summary>
    /// Calculate Hill sphere radius (sphere of gravitational influence)
    /// </summary>
    public double CalculateHillSphere(CelestialBody primary, double gravitationalConstant)
    {
        double distance = Vector3D.Distance(Position, primary.Position);
        return distance * System.Math.Pow(Mass / (3.0 * primary.Mass), 1.0 / 3.0);
    }

    public CelestialBody Clone()
    {
        return new CelestialBody
        {
            Id = Guid.NewGuid(),
            Name = Name + " (Copy)",
            Mass = Mass,
            Radius = Radius,
            Density = Density,
            Position = Position,
            Velocity = Velocity,
            Acceleration = Acceleration,
            RotationAxis = RotationAxis,
            RotationPeriod = RotationPeriod,
            CurrentRotation = CurrentRotation,
            Type = Type,
            TexturePath = TexturePath,
            NormalMapPath = NormalMapPath,
            HasAtmosphere = HasAtmosphere,
            AtmosphereColor = AtmosphereColor,
            AtmosphereHeight = AtmosphereHeight,
            IsEmissive = IsEmissive,
            EmissiveColor = EmissiveColor,
            Luminosity = Luminosity,
            Temperature = Temperature,
            Description = Description,
            Metadata = new Dictionary<string, string>(Metadata),
            IsActive = IsActive,
            IsFixed = IsFixed
        };
    }
}

public enum BodyType
{
    Star,
    Planet,
    Moon,
    Asteroid,
    Comet,
    DwarfPlanet,
    ArtificialSatellite,
    BlackHole,
    NeutronStar,
    Custom
}
