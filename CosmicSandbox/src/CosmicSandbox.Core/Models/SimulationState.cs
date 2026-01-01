namespace CosmicSandbox.Core.Models;

/// <summary>
/// Complete state of the simulation at a point in time
/// </summary>
public class SimulationState
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Untitled Simulation";
    public string? Description { get; set; }

    public List<CelestialBody> Bodies { get; set; } = new();

    // Simulation parameters
    public double CurrentTime { get; set; } // seconds since start
    public double TimeStep { get; set; } = 1.0; // seconds
    public double TimeScale { get; set; } = 1.0; // multiplier
    public bool IsPaused { get; set; } = true;
    public bool IsRecording { get; set; } = false;

    // Physics settings
    public double GravitationalConstant { get; set; } = Constants.PhysicsConstants.G;
    public IntegratorType Integrator { get; set; } = IntegratorType.VelocityVerlet;
    public bool EnableCollisions { get; set; } = true;
    public CollisionMode CollisionMode { get; set; } = CollisionMode.Merge;

    // Rendering settings
    public double UnitsScale { get; set; } = 1.0; // Render units per meter
    public bool ShowOrbits { get; set; } = true;
    public bool ShowVectors { get; set; } = false;
    public bool ShowGrid { get; set; } = true;
    public int OrbitTrailLength { get; set; } = 1000;

    // Camera state
    public CameraState Camera { get; set; } = new();

    // Metadata
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
    public string Version { get; set; } = "1.0.0";
    public Dictionary<string, string> CustomProperties { get; set; } = new();

    /// <summary>
    /// Calculate total system mass
    /// </summary>
    public double GetTotalMass() => Bodies.Where(b => b.IsActive).Sum(b => b.Mass);

    /// <summary>
    /// Calculate center of mass
    /// </summary>
    public Math.Vector3D GetCenterOfMass()
    {
        var activeBodies = Bodies.Where(b => b.IsActive).ToList();
        if (activeBodies.Count == 0) return Math.Vector3D.Zero;

        Math.Vector3D com = Math.Vector3D.Zero;
        double totalMass = 0;

        foreach (var body in activeBodies)
        {
            com += body.Position * body.Mass;
            totalMass += body.Mass;
        }

        return totalMass > 0 ? com / totalMass : Math.Vector3D.Zero;
    }

    /// <summary>
    /// Calculate total kinetic energy
    /// </summary>
    public double GetKineticEnergy()
    {
        return Bodies.Where(b => b.IsActive)
            .Sum(b => 0.5 * b.Mass * b.Velocity.LengthSquared());
    }

    /// <summary>
    /// Calculate total potential energy
    /// </summary>
    public double GetPotentialEnergy()
    {
        var activeBodies = Bodies.Where(b => b.IsActive).ToList();
        double potential = 0;

        for (int i = 0; i < activeBodies.Count; i++)
        {
            for (int j = i + 1; j < activeBodies.Count; j++)
            {
                double distance = Math.Vector3D.Distance(
                    activeBodies[i].Position,
                    activeBodies[j].Position
                );

                if (distance > 1e-10)
                {
                    potential -= GravitationalConstant *
                        activeBodies[i].Mass *
                        activeBodies[j].Mass / distance;
                }
            }
        }

        return potential;
    }

    /// <summary>
    /// Calculate total energy
    /// </summary>
    public double GetTotalEnergy() => GetKineticEnergy() + GetPotentialEnergy();
}

public enum IntegratorType
{
    Euler,
    VelocityVerlet,
    Leapfrog,
    RungeKutta4
}

public enum CollisionMode
{
    None,
    Merge,
    Bounce,
    Destroy
}

public class CameraState
{
    public Math.Vector3D Position { get; set; } = new(0, 0, -100);
    public Math.Vector3D Target { get; set; } = Math.Vector3D.Zero;
    public Math.Vector3D Up { get; set; } = Math.Vector3D.UnitY;
    public double FieldOfView { get; set; } = 60.0;
    public double NearPlane { get; set; } = 0.1;
    public double FarPlane { get; set; } = 1e12;
    public Guid? FollowingBodyId { get; set; }
}
