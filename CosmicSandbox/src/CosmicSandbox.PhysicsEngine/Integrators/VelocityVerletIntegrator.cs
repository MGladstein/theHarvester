using CosmicSandbox.Core.Interfaces;
using CosmicSandbox.Core.Math;
using CosmicSandbox.Core.Models;

namespace CosmicSandbox.PhysicsEngine.Integrators;

/// <summary>
/// Velocity Verlet integrator - symplectic, second-order accuracy
/// Excellent for energy conservation in long-term simulations
/// </summary>
public class VelocityVerletIntegrator : IIntegrator
{
    public string Name => "Velocity Verlet";
    public bool IsSymplectic => true;

    public void Step(SimulationState state, double dt)
    {
        var activeBodies = state.Bodies.Where(b => b.IsActive && !b.IsFixed).ToList();
        if (activeBodies.Count == 0) return;

        // Store previous accelerations
        var previousAccelerations = new Dictionary<Guid, Vector3D>();
        foreach (var body in activeBodies)
        {
            previousAccelerations[body.Id] = body.Acceleration;
        }

        // Update positions: x(t+dt) = x(t) + v(t)*dt + 0.5*a(t)*dt²
        foreach (var body in activeBodies)
        {
            body.Position += body.Velocity * dt + body.Acceleration * (0.5 * dt * dt);
        }

        // Calculate new accelerations at t+dt
        ComputeAccelerations(state);

        // Update velocities: v(t+dt) = v(t) + 0.5*(a(t) + a(t+dt))*dt
        foreach (var body in activeBodies)
        {
            var avgAcceleration = (previousAccelerations[body.Id] + body.Acceleration) * 0.5;
            body.Velocity += avgAcceleration * dt;
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

        // Find minimum orbital period and use fraction of it
        for (int i = 0; i < activeBodies.Count; i++)
        {
            for (int j = i + 1; j < activeBodies.Count; j++)
            {
                double distance = Vector3D.Distance(
                    activeBodies[i].Position,
                    activeBodies[j].Position
                );

                if (distance < 1e-10) continue;

                // Estimate orbital period using Kepler's third law
                double totalMass = activeBodies[i].Mass + activeBodies[j].Mass;
                double period = 2.0 * System.Math.PI * System.Math.Sqrt(
                    System.Math.Pow(distance, 3) / (state.GravitationalConstant * totalMass)
                );

                minTimescale = System.Math.Min(minTimescale, period);
            }
        }

        // Use 1/100th of shortest orbital period
        return minTimescale / 100.0;
    }

    private void ComputeAccelerations(SimulationState state)
    {
        var activeBodies = state.Bodies.Where(b => b.IsActive).ToList();

        // Reset accelerations
        foreach (var body in activeBodies.Where(b => !b.IsFixed))
        {
            body.Acceleration = Vector3D.Zero;
        }

        // Compute pairwise gravitational forces
        for (int i = 0; i < activeBodies.Count; i++)
        {
            if (activeBodies[i].IsFixed) continue;

            for (int j = 0; j < activeBodies.Count; j++)
            {
                if (i == j) continue;

                Vector3D direction = activeBodies[j].Position - activeBodies[i].Position;
                double distanceSquared = direction.LengthSquared();

                // Softening to avoid singularities
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
}
