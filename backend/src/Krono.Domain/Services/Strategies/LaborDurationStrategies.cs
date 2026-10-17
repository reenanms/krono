using Krono.Domain.Entities;
using Krono.Domain.Enums;
using Krono.Domain.Interfaces;

namespace Krono.Domain.Services.Strategies;

public class PassiveDurationStrategy : ILaborDurationStrategy
{
    public LaborAllocationMode Mode => LaborAllocationMode.Passive;

    public decimal CalculateDuration(ProductionStage stage, int quantity, int? assignedOperators)
    {
        var setup = stage.SetupFixedMinutes + (stage.SetupMultiplierMinutes * quantity);
        var prod = stage.ProdFixedMinutes + (stage.ProdMultiplierMinutes * quantity);
        return setup + prod;
    }
}

public class FixedTeamDurationStrategy : ILaborDurationStrategy
{
    public LaborAllocationMode Mode => LaborAllocationMode.FixedTeam;

    public decimal CalculateDuration(ProductionStage stage, int quantity, int? assignedOperators)
    {
        var setup = stage.SetupFixedMinutes + (stage.SetupMultiplierMinutes * quantity);
        var prod = stage.ProdFixedMinutes + (stage.ProdMultiplierMinutes * quantity);
        return setup + prod;
    }
}

public class ScalableLaborDurationStrategy : ILaborDurationStrategy
{
    public LaborAllocationMode Mode => LaborAllocationMode.ScalableLabor;

    public decimal CalculateDuration(ProductionStage stage, int quantity, int? assignedOperators)
    {
        var setup = stage.SetupFixedMinutes + (stage.SetupMultiplierMinutes * quantity);
        var prod = stage.ProdFixedMinutes + (stage.ProdMultiplierMinutes * quantity);

        int workers = assignedOperators.HasValue && assignedOperators.Value > 0
            ? assignedOperators.Value
            : (stage.DefaultOperators > 0 ? stage.DefaultOperators : 1);

        return setup + (prod / workers);
    }
}
