using CosmicSandbox.Core.Interfaces;
using CosmicSandbox.Core.Math;
using CosmicSandbox.Core.Models;

namespace CosmicSandbox.PhysicsEngine.Integrators;

/// <summary>
/// Leapfrog integrator - symplectic, excellent energy conservation
/// Velocities and positions are evaluated at interleaved time points
/// </summary>
public class LeapfrogIntegrator : IIntegrator
{
    public string Name => "Leapfrog";
    public bool IsSymplectic => true;

    private bool _isInitialized = false;

    public void Step(SimulationState state, double dt)
    {
        var activeBodies = state.Bodies.Where(b => b.IsActive && !b.IsFixed).ToList();
        if (activeBodies.Count == 0) return;

        if (!_isInitialized)
        {
            // Initialize by doing a half-step backward for velocities
            ComputeAccelerations(state);
            foreach (var body in activeBodies)
            {
                body.Velocity -= body.Acceleration * (0.5 * dt);
            }
            _isInitialized = true;
        }

        // Kick: v(t+dt/2) = v(t-dt/2) + a(t)*dt
        ComputeAccelerations(state);
        foreach (var body in activeBodies)
        {
            body.Velocity += body.Acceleration * dt;
        }

        // Drift: x(t+dt) = x(t) + v(t+dt/2)*dt
        foreach (var body in activeBodies)
        {
            body.Position += body.Velocity * dt;
        }

        // Update rotation
        foreach (var body in activeBodies)
        {
            if (body.RotationPeriod > 0)
            {
                body.CurrentRotation += (2.0 * System.Math.PI / body.RotationPeriod) * dt;
                body.CurrentRotation %= 2.0 * System.Math.PI;
            }
        }

        state.CurrentTime += dt;
    }

    public double GetRecommendedTimestep(SimulationState state)
    {
        var activeBodies = state.Bodies.Where(b => b.IsActive).ToList();
        if (activeBodies.Count < 2) return 1.0;

        double minTimescale = double.MaxValue;

        for (int i = 0; i < activeBodies.Count; i++)
        {
            for (int j = i + 1; j < activeBodies.Count; j++)
            {
                double distance = Vector3D.Distance(
                    activeBodies[i].Position,
                    activeBodies[j].Position
                );

                if (distance < 1e-10) continue;

                double totalMass = activeBodies[i].Mass + activeBodies[j].Mass;
                double period = 2.0 * System.Math.PI * System.Math.Sqrt(
                    System.Math.Pow(distance, 3) / (state.GravitationalConstant * totalMass)
                );

                minTimescale = System.Math.Min(minTimescale, period);
            }
        }

        return minTimescale / 100.0;
    }

    private void ComputeAccelerations(SimulationState state)
    {
        var activeBodies = state.Bodies.Where(b => b.IsActive).ToList();

        foreach (var body in activeBodies.Where(b => !b.IsFixed))
        {
            body.Acceleration = Vector3D.Zero;
        }

        for (int i = 0; i < activeBodies.Count; i++)
        {
            if (activeBodies[i].IsFixed) continue;

            for (int j = 0; j < activeBodies.Count; j++)
            {
                if (i == j) continue;

                Vector3D direction = activeBodies[j].Position - activeBodies[i].Position;
                double distanceSquared = direction.LengthSquared();

                const double softeningLength = 1e-10;
                distanceSquared += softeningLength * softeningLength;

                double distance = System.Math.Sqrt(distanceSquared);
                double forceMagnitude = state.GravitationalConstant *
                    activeBodies[j].Mass / distanceSquared;

                Vector3D acceleration = direction.Normalize() * forceMagnitude;
                activeBodies[i].Acceleration += acceleration;
            }
        }
    }

    public void Reset()
    {
        _isInitialized = false;
    }
}
