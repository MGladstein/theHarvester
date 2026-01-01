using CosmicSandbox.Core.Interfaces;
using CosmicSandbox.Core.Models;
using CosmicSandbox.PhysicsEngine.Collision;
using CosmicSandbox.PhysicsEngine.Integrators;

namespace CosmicSandbox.PhysicsEngine;

/// <summary>
/// Main controller for physics simulation
/// </summary>
public class SimulationController
{
    private readonly SimulationState _state;
    private IIntegrator _integrator;
    private readonly CollisionDetector _collisionDetector;

    private double _accumulatedTime = 0.0;
    private double _energyDrift = 0.0;
    private double _initialEnergy = 0.0;

    public SimulationState State => _state;
    public double EnergyDrift => _energyDrift;

    public event EventHandler<CollisionDetector.CollisionEvent>? CollisionOccurred;
    public event EventHandler<SimulationWarning>? WarningRaised;

    public SimulationController(SimulationState state)
    {
        _state = state;
        _integrator = CreateIntegrator(state.Integrator);
        _collisionDetector = new CollisionDetector();
        _initialEnergy = _state.GetTotalEnergy();
    }

    /// <summary>
    /// Step the simulation forward by the configured timestep
    /// </summary>
    public void Step()
    {
        if (_state.IsPaused) return;

        double effectiveTimestep = _state.TimeStep * _state.TimeScale;

        // Check for instability
        double recommendedTimestep = _integrator.GetRecommendedTimestep(_state);
        if (effectiveTimestep > recommendedTimestep * 2.0)
        {
            WarningRaised?.Invoke(this, new SimulationWarning
            {
                Level = WarningLevel.High,
                Message = $"Timestep ({effectiveTimestep:E2}s) exceeds recommended value ({recommendedTimestep:E2}s). Simulation may be unstable.",
                Timestamp = DateTime.UtcNow
            });
        }

        // Perform integration step
        _integrator.Step(_state, effectiveTimestep);

        // Handle collisions if enabled
        if (_state.EnableCollisions)
        {
            HandleCollisions();
        }

        // Track energy drift
        UpdateEnergyDrift();

        _accumulatedTime += effectiveTimestep;
    }

    /// <summary>
    /// Step forward by real-time delta (for real-time simulation)
    /// </summary>
    public void StepByDelta(double deltaTime)
    {
        if (_state.IsPaused) return;

        _accumulatedTime += deltaTime * _state.TimeScale;

        while (_accumulatedTime >= _state.TimeStep)
        {
            _integrator.Step(_state, _state.TimeStep);

            if (_state.EnableCollisions)
            {
                HandleCollisions();
            }

            _accumulatedTime -= _state.TimeStep;
        }

        UpdateEnergyDrift();
    }

    /// <summary>
    /// Change the integrator method
    /// </summary>
    public void SetIntegrator(IntegratorType type)
    {
        _state.Integrator = type;
        _integrator = CreateIntegrator(type);
    }

    /// <summary>
    /// Reset energy tracking
    /// </summary>
    public void ResetEnergyTracking()
    {
        _initialEnergy = _state.GetTotalEnergy();
        _energyDrift = 0.0;
    }

    /// <summary>
    /// Add a body to the simulation
    /// </summary>
    public void AddBody(CelestialBody body)
    {
        _state.Bodies.Add(body);
        _initialEnergy = _state.GetTotalEnergy();
    }

    /// <summary>
    /// Remove a body from the simulation
    /// </summary>
    public void RemoveBody(Guid bodyId)
    {
        _state.Bodies.RemoveAll(b => b.Id == bodyId);
        _initialEnergy = _state.GetTotalEnergy();
    }

    /// <summary>
    /// Get simulation statistics
    /// </summary>
    public SimulationStats GetStats()
    {
        var activeBodies = _state.Bodies.Where(b => b.IsActive).ToList();

        return new SimulationStats
        {
            TotalBodies = _state.Bodies.Count,
            ActiveBodies = activeBodies.Count,
            TotalMass = _state.GetTotalMass(),
            KineticEnergy = _state.GetKineticEnergy(),
            PotentialEnergy = _state.GetPotentialEnergy(),
            TotalEnergy = _state.GetTotalEnergy(),
            EnergyDrift = _energyDrift,
            SimulationTime = _state.CurrentTime,
            CenterOfMass = _state.GetCenterOfMass()
        };
    }

    private void HandleCollisions()
    {
        var collisions = _collisionDetector.DetectCollisions(_state);

        foreach (var collision in collisions)
        {
            CollisionOccurred?.Invoke(this, collision);

            switch (_state.CollisionMode)
            {
                case CollisionMode.Merge:
                    var mergedBody = _collisionDetector.MergeBodies(collision.Body1, collision.Body2);
                    _state.Bodies.Add(mergedBody);
                    collision.Body1.IsActive = false;
                    collision.Body2.IsActive = false;
                    break;

                case CollisionMode.Bounce:
                    _collisionDetector.BounceBodies(collision.Body1, collision.Body2);
                    break;

                case CollisionMode.Destroy:
                    collision.Body1.IsActive = false;
                    collision.Body2.IsActive = false;
                    break;
            }
        }
    }

    private void UpdateEnergyDrift()
    {
        if (_initialEnergy != 0)
        {
            double currentEnergy = _state.GetTotalEnergy();
            _energyDrift = System.Math.Abs((currentEnergy - _initialEnergy) / _initialEnergy);

            if (_energyDrift > 0.01 && !_integrator.IsSymplectic)
            {
                WarningRaised?.Invoke(this, new SimulationWarning
                {
                    Level = WarningLevel.Medium,
                    Message = $"Energy drift is {_energyDrift * 100:F2}%. Consider using a symplectic integrator for long simulations.",
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }

    private IIntegrator CreateIntegrator(IntegratorType type)
    {
        return type switch
        {
            IntegratorType.VelocityVerlet => new VelocityVerletIntegrator(),
            IntegratorType.Leapfrog => new LeapfrogIntegrator(),
            IntegratorType.RungeKutta4 => new RungeKutta4Integrator(),
            _ => new VelocityVerletIntegrator()
        };
    }
}

public class SimulationStats
{
    public int TotalBodies { get; set; }
    public int ActiveBodies { get; set; }
    public double TotalMass { get; set; }
    public double KineticEnergy { get; set; }
    public double PotentialEnergy { get; set; }
    public double TotalEnergy { get; set; }
    public double EnergyDrift { get; set; }
    public double SimulationTime { get; set; }
    public Core.Math.Vector3D CenterOfMass { get; set; }
}

public class SimulationWarning
{
    public WarningLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public enum WarningLevel
{
    Low,
    Medium,
    High,
    Critical
}
