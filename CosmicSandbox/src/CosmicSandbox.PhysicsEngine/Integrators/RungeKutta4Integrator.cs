using CosmicSandbox.Core.Interfaces;
using CosmicSandbox.Core.Math;
using CosmicSandbox.Core.Models;

namespace CosmicSandbox.PhysicsEngine.Integrators;

/// <summary>
/// Fourth-order Runge-Kutta integrator
/// High accuracy but not symplectic - energy may drift over long periods
/// Good for short-term high-precision simulations
/// </summary>
public class RungeKutta4Integrator : IIntegrator
{
    public string Name => "Runge-Kutta 4";
    public bool IsSymplectic => false;

    private class State
    {
        public Vector3D Position;
        public Vector3D Velocity;
    }

    public void Step(SimulationState state, double dt)
    {
        var activeBodies = state.Bodies.Where(b => b.IsActive && !b.IsFixed).ToList();
        if (activeBodies.Count == 0) return;

        // Store initial states
        var initialStates = new Dictionary<Guid, State>();
        foreach (var body in activeBodies)
        {
            initialStates[body.Id] = new State
            {
                Position = body.Position,
                Velocity = body.Velocity
            };
        }

        // k1 = f(t, y)
        var k1 = ComputeDerivatives(state);

        // k2 = f(t + dt/2, y + k1*dt/2)
        ApplyDerivatives(activeBodies, initialStates, k1, dt * 0.5);
        var k2 = ComputeDerivatives(state);

        // k3 = f(t + dt/2, y + k2*dt/2)
        ApplyDerivatives(activeBodies, initialStates, k2, dt * 0.5);
        var k3 = ComputeDerivatives(state);

        // k4 = f(t + dt, y + k3*dt)
        ApplyDerivatives(activeBodies, initialStates, k3, dt);
        var k4 = ComputeDerivatives(state);

        // y(t+dt) = y(t) + dt/6 * (k1 + 2*k2 + 2*k3 + k4)
        foreach (var body in activeBodies)
        {
            var init = initialStates[body.Id];

            body.Position = init.Position + (
                k1[body.Id].Velocity +
                k2[body.Id].Velocity * 2.0 +
                k3[body.Id].Velocity * 2.0 +
                k4[body.Id].Velocity
            ) * (dt / 6.0);

            body.Velocity = init.Velocity + (
                k1[body.Id].Acceleration +
                k2[body.Id].Acceleration * 2.0 +
                k3[body.Id].Acceleration * 2.0 +
                k4[body.Id].Acceleration
            ) * (dt / 6.0);
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

        return minTimescale / 50.0; // RK4 is more accurate, can use larger timesteps
    }

    private class Derivative
    {
        public Vector3D Velocity;
        public Vector3D Acceleration;
    }

    private Dictionary<Guid, Derivative> ComputeDerivatives(SimulationState state)
    {
        var derivatives = new Dictionary<Guid, Derivative>();
        var activeBodies = state.Bodies.Where(b => b.IsActive).ToList();

        foreach (var body in activeBodies.Where(b => !b.IsFixed))
        {
            Vector3D acceleration = Vector3D.Zero;

            foreach (var other in activeBodies)
            {
                if (other.Id == body.Id) continue;

                Vector3D direction = other.Position - body.Position;
                double distanceSquared = direction.LengthSquared();

                const double softeningLength = 1e-10;
                distanceSquared += softeningLength * softeningLength;

                double distance = System.Math.Sqrt(distanceSquared);
                double forceMagnitude = state.GravitationalConstant *
                    other.Mass / distanceSquared;

                acceleration += direction.Normalize() * forceMagnitude;
            }

            derivatives[body.Id] = new Derivative
            {
                Velocity = body.Velocity,
                Acceleration = acceleration
            };
        }

        return derivatives;
    }

    private void ApplyDerivatives(
        List<CelestialBody> bodies,
        Dictionary<Guid, State> initialStates,
        Dictionary<Guid, Derivative> derivatives,
        double dt)
    {
        foreach (var body in bodies)
        {
            var init = initialStates[body.Id];
            var deriv = derivatives[body.Id];

            body.Position = init.Position + deriv.Velocity * dt;
            body.Velocity = init.Velocity + deriv.Acceleration * dt;
        }
    }
}
