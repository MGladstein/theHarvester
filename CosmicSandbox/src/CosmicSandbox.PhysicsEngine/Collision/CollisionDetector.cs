using CosmicSandbox.Core.Math;
using CosmicSandbox.Core.Models;

namespace CosmicSandbox.PhysicsEngine.Collision;

/// <summary>
/// Detects and resolves collisions between celestial bodies
/// </summary>
public class CollisionDetector
{
    public class CollisionEvent
    {
        public CelestialBody Body1 { get; set; } = null!;
        public CelestialBody Body2 { get; set; } = null!;
        public double RelativeVelocity { get; set; }
        public Vector3D ImpactPoint { get; set; }
        public double ImpactEnergy { get; set; }
    }

    /// <summary>
    /// Detect all collisions in the current state
    /// </summary>
    public List<CollisionEvent> DetectCollisions(SimulationState state)
    {
        var collisions = new List<CollisionEvent>();
        var activeBodies = state.Bodies.Where(b => b.IsActive).ToList();

        for (int i = 0; i < activeBodies.Count; i++)
        {
            for (int j = i + 1; j < activeBodies.Count; j++)
            {
                var collision = CheckCollision(activeBodies[i], activeBodies[j]);
                if (collision != null)
                {
                    collisions.Add(collision);
                }
            }
        }

        return collisions;
    }

    /// <summary>
    /// Check if two bodies are colliding
    /// </summary>
    private CollisionEvent? CheckCollision(CelestialBody body1, CelestialBody body2)
    {
        double distance = Vector3D.Distance(body1.Position, body2.Position);
        double combinedRadius = body1.Radius + body2.Radius;

        if (distance < combinedRadius)
        {
            Vector3D relativeVelocity = body2.Velocity - body1.Velocity;
            Vector3D direction = (body2.Position - body1.Position).Normalize();
            Vector3D impactPoint = body1.Position + direction * body1.Radius;

            // Calculate impact energy
            double reducedMass = (body1.Mass * body2.Mass) / (body1.Mass + body2.Mass);
            double impactEnergy = 0.5 * reducedMass * relativeVelocity.LengthSquared();

            return new CollisionEvent
            {
                Body1 = body1,
                Body2 = body2,
                RelativeVelocity = relativeVelocity.Length(),
                ImpactPoint = impactPoint,
                ImpactEnergy = impactEnergy
            };
        }

        return null;
    }

    /// <summary>
    /// Resolve collision by merging bodies
    /// </summary>
    public CelestialBody MergeBodies(CelestialBody body1, CelestialBody body2)
    {
        // Conserve momentum
        double totalMass = body1.Mass + body2.Mass;
        Vector3D finalVelocity = (body1.Velocity * body1.Mass + body2.Velocity * body2.Mass) / totalMass;
        Vector3D finalPosition = (body1.Position * body1.Mass + body2.Position * body2.Mass) / totalMass;

        // Conserve mass (volume adds up, assuming same density)
        double finalVolume = (4.0 / 3.0) * System.Math.PI *
            (System.Math.Pow(body1.Radius, 3) + System.Math.Pow(body2.Radius, 3));
        double finalRadius = System.Math.Pow(3.0 * finalVolume / (4.0 * System.Math.PI), 1.0 / 3.0);

        // Create merged body (prefer larger body's properties)
        var largerBody = body1.Mass >= body2.Mass ? body1 : body2;

        return new CelestialBody
        {
            Name = $"{largerBody.Name} (merged)",
            Mass = totalMass,
            Radius = finalRadius,
            Density = totalMass / finalVolume,
            Position = finalPosition,
            Velocity = finalVelocity,
            Acceleration = Vector3D.Zero,
            Type = largerBody.Type,
            TexturePath = largerBody.TexturePath,
            NormalMapPath = largerBody.NormalMapPath,
            HasAtmosphere = largerBody.HasAtmosphere,
            AtmosphereColor = largerBody.AtmosphereColor,
            AtmosphereHeight = largerBody.AtmosphereHeight,
            IsEmissive = largerBody.IsEmissive,
            EmissiveColor = largerBody.EmissiveColor,
            Luminosity = largerBody.Luminosity + (body1.Mass >= body2.Mass ? 0 : body2.Luminosity),
            Temperature = largerBody.Temperature,
            RotationAxis = largerBody.RotationAxis,
            RotationPeriod = largerBody.RotationPeriod,
            CurrentRotation = largerBody.CurrentRotation,
            IsActive = true,
            IsFixed = false
        };
    }

    /// <summary>
    /// Resolve collision with elastic bounce
    /// </summary>
    public void BounceBodies(CelestialBody body1, CelestialBody body2, double restitution = 0.8)
    {
        Vector3D normal = (body2.Position - body1.Position).Normalize();
        Vector3D relativeVelocity = body2.Velocity - body1.Velocity;

        double velocityAlongNormal = Vector3D.Dot(relativeVelocity, normal);

        // Don't resolve if bodies are separating
        if (velocityAlongNormal > 0) return;

        // Calculate impulse
        double impulse = -(1.0 + restitution) * velocityAlongNormal;
        impulse /= (1.0 / body1.Mass) + (1.0 / body2.Mass);

        Vector3D impulseVector = normal * impulse;

        // Apply impulse
        if (!body1.IsFixed)
            body1.Velocity -= impulseVector / body1.Mass;

        if (!body2.IsFixed)
            body2.Velocity += impulseVector / body2.Mass;

        // Separate bodies to prevent overlap
        double overlap = (body1.Radius + body2.Radius) - Vector3D.Distance(body1.Position, body2.Position);
        if (overlap > 0)
        {
            Vector3D separation = normal * (overlap * 0.5);

            if (!body1.IsFixed)
                body1.Position -= separation;

            if (!body2.IsFixed)
                body2.Position += separation;
        }
    }
}
