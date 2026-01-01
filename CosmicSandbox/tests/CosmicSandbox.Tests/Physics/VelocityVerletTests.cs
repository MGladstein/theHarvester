using CosmicSandbox.Core.Constants;
using CosmicSandbox.Core.Math;
using CosmicSandbox.Core.Models;
using CosmicSandbox.PhysicsEngine.Integrators;
using FluentAssertions;
using Xunit;

namespace CosmicSandbox.Tests.Physics;

public class VelocityVerletTests
{
    [Fact]
    public void Step_SimpleOrbit_ConservesEnergy()
    {
        // Arrange
        var integrator = new VelocityVerletIntegrator();
        var state = CreateSimpleTwoBodySystem();

        double initialEnergy = CalculateTotalEnergy(state);

        // Act - simulate for 100 steps
        for (int i = 0; i < 100; i++)
        {
            integrator.Step(state, state.TimeStep);
        }

        double finalEnergy = CalculateTotalEnergy(state);

        // Assert - energy drift should be less than 1%
        double energyDrift = Math.Abs((finalEnergy - initialEnergy) / initialEnergy);
        energyDrift.Should().BeLessThan(0.01, "Velocity Verlet should conserve energy well");
    }

    [Fact]
    public void Step_CircularOrbit_MaintainsDistance()
    {
        // Arrange
        var integrator = new VelocityVerletIntegrator();
        var state = CreateCircularOrbitSystem();

        var planet = state.Bodies.First(b => b.Name == "Planet");
        double initialDistance = planet.Position.Length();

        // Act - simulate one complete orbit
        double orbitalPeriod = 2 * Math.PI * Math.Sqrt(
            Math.Pow(initialDistance, 3) / (state.GravitationalConstant * state.Bodies[0].Mass)
        );

        int steps = (int)(orbitalPeriod / state.TimeStep);
        for (int i = 0; i < steps; i++)
        {
            integrator.Step(state, state.TimeStep);
        }

        double finalDistance = planet.Position.Length();

        // Assert - distance should remain roughly constant for circular orbit
        double distanceChange = Math.Abs(finalDistance - initialDistance) / initialDistance;
        distanceChange.Should().BeLessThan(0.05, "Circular orbit should maintain distance");
    }

    [Fact]
    public void Step_ThreeBodySystem_PreservesAngularMomentum()
    {
        // Arrange
        var integrator = new VelocityVerletIntegrator();
        var state = CreateThreeBodySystem();

        var initialAngularMomentum = CalculateAngularMomentum(state);

        // Act
        for (int i = 0; i < 50; i++)
        {
            integrator.Step(state, state.TimeStep);
        }

        var finalAngularMomentum = CalculateAngularMomentum(state);

        // Assert
        var angularMomentumDrift = Vector3D.Distance(initialAngularMomentum, finalAngularMomentum) /
            initialAngularMomentum.Length();

        angularMomentumDrift.Should().BeLessThan(0.01, "Angular momentum should be conserved");
    }

    [Fact]
    public void GetRecommendedTimestep_ReturnsReasonableValue()
    {
        // Arrange
        var integrator = new VelocityVerletIntegrator();
        var state = CreateSimpleTwoBodySystem();

        // Act
        double recommendedTimestep = integrator.GetRecommendedTimestep(state);

        // Assert
        recommendedTimestep.Should().BeGreaterThan(0);
        recommendedTimestep.Should().BeLessThan(state.TimeStep * 10);
    }

    private SimulationState CreateSimpleTwoBodySystem()
    {
        var state = new SimulationState
        {
            TimeStep = 1000.0,
            GravitationalConstant = PhysicsConstants.G
        };

        // Central massive body (like Sun)
        state.Bodies.Add(new CelestialBody
        {
            Name = "Star",
            Mass = PhysicsConstants.SolarMass,
            Radius = PhysicsConstants.SolarRadius,
            Position = Vector3D.Zero,
            Velocity = Vector3D.Zero,
            IsFixed = true
        });

        // Orbiting body (like Earth)
        double distance = PhysicsConstants.AU;
        double orbitalVelocity = Math.Sqrt(
            state.GravitationalConstant * PhysicsConstants.SolarMass / distance
        );

        state.Bodies.Add(new CelestialBody
        {
            Name = "Planet",
            Mass = PhysicsConstants.EarthMass,
            Radius = PhysicsConstants.EarthRadius,
            Position = new Vector3D(distance, 0, 0),
            Velocity = new Vector3D(0, orbitalVelocity, 0)
        });

        return state;
    }

    private SimulationState CreateCircularOrbitSystem()
    {
        return CreateSimpleTwoBodySystem();
    }

    private SimulationState CreateThreeBodySystem()
    {
        var state = new SimulationState
        {
            TimeStep = 500.0,
            GravitationalConstant = PhysicsConstants.G
        };

        // Three equal mass bodies in triangular configuration
        double mass = PhysicsConstants.SolarMass;
        double radius = PhysicsConstants.AU;
        double velocity = Math.Sqrt(state.GravitationalConstant * mass / radius);

        state.Bodies.Add(new CelestialBody
        {
            Name = "Body 1",
            Mass = mass,
            Radius = PhysicsConstants.SolarRadius,
            Position = new Vector3D(radius, 0, 0),
            Velocity = new Vector3D(0, velocity, 0)
        });

        state.Bodies.Add(new CelestialBody
        {
            Name = "Body 2",
            Mass = mass,
            Radius = PhysicsConstants.SolarRadius,
            Position = new Vector3D(-radius / 2, radius * Math.Sqrt(3) / 2, 0),
            Velocity = new Vector3D(-velocity * Math.Sqrt(3) / 2, -velocity / 2, 0)
        });

        state.Bodies.Add(new CelestialBody
        {
            Name = "Body 3",
            Mass = mass,
            Radius = PhysicsConstants.SolarRadius,
            Position = new Vector3D(-radius / 2, -radius * Math.Sqrt(3) / 2, 0),
            Velocity = new Vector3D(velocity * Math.Sqrt(3) / 2, -velocity / 2, 0)
        });

        return state;
    }

    private double CalculateTotalEnergy(SimulationState state)
    {
        double kinetic = 0;
        double potential = 0;

        var bodies = state.Bodies.Where(b => b.IsActive && !b.IsFixed).ToList();

        foreach (var body in bodies)
        {
            kinetic += 0.5 * body.Mass * body.Velocity.LengthSquared();
        }

        for (int i = 0; i < bodies.Count; i++)
        {
            for (int j = i + 1; j < bodies.Count; j++)
            {
                double distance = Vector3D.Distance(bodies[i].Position, bodies[j].Position);
                if (distance > 1e-10)
                {
                    potential -= state.GravitationalConstant *
                        bodies[i].Mass * bodies[j].Mass / distance;
                }
            }
        }

        return kinetic + potential;
    }

    private Vector3D CalculateAngularMomentum(SimulationState state)
    {
        Vector3D totalL = Vector3D.Zero;

        foreach (var body in state.Bodies.Where(b => b.IsActive))
        {
            Vector3D r = body.Position;
            Vector3D p = body.Velocity * body.Mass;
            Vector3D L = Vector3D.Cross(r, p);
            totalL += L;
        }

        return totalL;
    }
}
