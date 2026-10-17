using Krono.Domain.Entities;
using Krono.Domain.Enums;
using Krono.Domain.Services;
using Xunit;

namespace Krono.Tests;

public class StageDurationTests
{
    private readonly StageDurationCalculator _calculator = new();

    [Fact]
    public void PassiveStage_ShouldHaveFixedDuration_RegardlessOfOperators()
    {
        // Arrange
        var stage = new ProductionStage
        {
            Name = "Rest Dough",
            LaborMode = LaborAllocationMode.Passive,
            SetupFixedMinutes = 30,
            SetupMultiplierMinutes = 0,
            ProdFixedMinutes = 0,
            ProdMultiplierMinutes = 0
        };

        // Act
        var duration = _calculator.Calculate(stage, quantity: 1000, assignedOperators: 0);

        // Assert
        Assert.Equal(30m, duration);
    }

    [Fact]
    public void FixedTeamStage_ShouldComputeSetupPlusUnitCycle()
    {
        // Arrange: Cupcake Baking (setup: 10 min, prod: 0.02 min per unit)
        var stage = new ProductionStage
        {
            Name = "Bake Cupcakes",
            LaborMode = LaborAllocationMode.FixedTeam,
            DefaultOperators = 1,
            SetupFixedMinutes = 10,
            SetupMultiplierMinutes = 0,
            ProdFixedMinutes = 0,
            ProdMultiplierMinutes = 0.02m
        };

        // Act for 1,000 cupcakes: 10 + (0.02 * 1000) = 30 minutes
        var duration = _calculator.Calculate(stage, quantity: 1000, assignedOperators: 1);

        // Assert
        Assert.Equal(30m, duration);
    }

    [Fact]
    public void ScalableLaborStage_ShouldScaleInverselyWithHeadcount()
    {
        // Arrange: Cupcake Packaging (setup: 0, prod: 120 min total for batch)
        var stage = new ProductionStage
        {
            Name = "Package Cupcakes",
            LaborMode = LaborAllocationMode.ScalableLabor,
            DefaultOperators = 2,
            SetupFixedMinutes = 0,
            SetupMultiplierMinutes = 0,
            ProdFixedMinutes = 120,
            ProdMultiplierMinutes = 0
        };

        // Act: 2 operators -> 120 / 2 = 60 min
        var durationWith2 = _calculator.Calculate(stage, quantity: 1000, assignedOperators: 2);
        // Act: 4 operators -> 120 / 4 = 30 min
        var durationWith4 = _calculator.Calculate(stage, quantity: 1000, assignedOperators: 4);

        // Assert
        Assert.Equal(60m, durationWith2);
        Assert.Equal(30m, durationWith4);
    }
}
