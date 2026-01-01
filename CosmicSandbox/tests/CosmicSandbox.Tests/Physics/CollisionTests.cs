using CosmicSandbox.Core.Math;
using CosmicSandbox.Core.Models;
using CosmicSandbox.PhysicsEngine.Collision;
using FluentAssertions;
using Xunit;

namespace CosmicSandbox.Tests.Physics;

public class CollisionTests
{
    [Fact]
    public void DetectCollisions_OverlappingBodies_ReturnsCollision()
    {
        // Arrange
        var detector = new CollisionDetector();
        var state = new SimulationState();

        var body1 = new CelestialBody
        {
            Name = "Body 1",
            Mass = 1e24,
            Radius = 1e6,
            Position = new Vector3D(0, 0, 0),
            Velocity = new Vector3D(1000, 0, 0)
        };

        var body2 = new CelestialBody
        {
            Name = "Body 2",
            Mass = 1e24,
            Radius = 1e6,
            Position = new Vector3D(1.5e6, 0, 0),
            Velocity = new Vector3D(-1000, 0, 0)
        };

        state.Bodies.Add(body1);
        state.Bodies.Add(body2);

        // Act
        var collisions = detector.DetectCollisions(state);

        // Assert
        collisions.Should().HaveCount(1);
        collisions[0].Body1.Should().Be(body1);
        collisions[0].Body2.Should().Be(body2);
    }

    [Fact]
    public void DetectCollisions_SeparatedBodies_ReturnsNoCollision()
    {
        // Arrange
        var detector = new CollisionDetector();
        var state = new SimulationState();

        state.Bodies.Add(new CelestialBody
        {
            Mass = 1e24,
            Radius = 1e6,
            Position = new Vector3D(0, 0, 0)
        });

        state.Bodies.Add(new CelestialBody
        {
            Mass = 1e24,
            Radius = 1e6,
            Position = new Vector3D(1e8, 0, 0)
        });

        // Act
        var collisions = detector.DetectCollisions(state);

        // Assert
        collisions.Should().BeEmpty();
    }

    [Fact]
    public void MergeBodies_ConservesMomentum()
    {
        // Arrange
        var detector = new CollisionDetector();

        var body1 = new CelestialBody
        {
            Name = "Body 1",
            Mass = 2e24,
            Radius = 1e6,
            Position = new Vector3D(0, 0, 0),
            Velocity = new Vector3D(1000, 0, 0)
        };

        var body2 = new CelestialBody
        {
            Name = "Body 2",
            Mass = 1e24,
            Radius = 1e6,
            Position = new Vector3D(1e6, 0, 0),
            Velocity = new Vector3D(-500, 0, 0)
        };

        var initialMomentum = body1.Velocity * body1.Mass + body2.Velocity * body2.Mass;

        // Act
        var merged = detector.MergeBodies(body1, body2);
        var finalMomentum = merged.Velocity * merged.Mass;

        // Assert
        merged.Mass.Should().Be(body1.Mass + body2.Mass);
        Vector3D.Distance(initialMomentum, finalMomentum).Should().BeLessThan(1e-6);
    }

    [Fact]
    public void MergeBodies_ConservesMass()
    {
        // Arrange
        var detector = new CollisionDetector();

        var body1 = new CelestialBody
        {
            Mass = 2.5e24,
            Radius = 1e6,
            Position = Vector3D.Zero
        };

        var body2 = new CelestialBody
        {
            Mass = 1.5e24,
            Radius = 1e6,
            Position = new Vector3D(1e6, 0, 0)
        };

        // Act
        var merged = detector.MergeBodies(body1, body2);

        // Assert
        merged.Mass.Should().Be(4e24);
    }

    [Fact]
    public void BounceBodies_ConservesEnergy()
    {
        // Arrange
        var detector = new CollisionDetector();

        var body1 = new CelestialBody
        {
            Mass = 1e24,
            Radius = 1e6,
            Position = new Vector3D(0, 0, 0),
            Velocity = new Vector3D(1000, 0, 0)
        };

        var body2 = new CelestialBody
        {
            Mass = 1e24,
            Radius = 1e6,
            Position = new Vector3D(1.5e6, 0, 0),
            Velocity = new Vector3D(-1000, 0, 0)
        };

        double initialKE = 0.5 * body1.Mass * body1.Velocity.LengthSquared() +
                          0.5 * body2.Mass * body2.Velocity.LengthSquared();

        // Act
        detector.BounceBodies(body1, body2, restitution: 1.0); // Perfectly elastic

        double finalKE = 0.5 * body1.Mass * body1.Velocity.LengthSquared() +
                        0.5 * body2.Mass * body2.Velocity.LengthSquared();

        // Assert
        Math.Abs(finalKE - initialKE).Should().BeLessThan(initialKE * 0.01);
    }
}
