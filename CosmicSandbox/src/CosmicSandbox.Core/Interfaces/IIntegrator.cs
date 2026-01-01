using CosmicSandbox.Core.Models;

namespace CosmicSandbox.Core.Interfaces;

/// <summary>
/// Interface for numerical integration methods
/// </summary>
public interface IIntegrator
{
    /// <summary>
    /// Name of the integrator
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Whether this integrator is symplectic (conserves energy better)
    /// </summary>
    bool IsSymplectic { get; }

    /// <summary>
    /// Step the simulation forward by one timestep
    /// </summary>
    void Step(SimulationState state, double dt);

    /// <summary>
    /// Get recommended maximum timestep for stability
    /// </summary>
    double GetRecommendedTimestep(SimulationState state);
}
