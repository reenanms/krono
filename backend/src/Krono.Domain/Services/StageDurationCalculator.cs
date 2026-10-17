using Krono.Domain.Entities;
using Krono.Domain.Enums;
using Krono.Domain.Interfaces;
using Krono.Domain.Services.Strategies;

namespace Krono.Domain.Services;

public class StageDurationCalculator
{
    private readonly Dictionary<LaborAllocationMode, ILaborDurationStrategy> _strategies;

    public StageDurationCalculator(IEnumerable<ILaborDurationStrategy>? strategies = null)
    {
        if (strategies != null && strategies.Any())
        {
            _strategies = strategies.ToDictionary(s => s.Mode);
        }
        else
        {
            _strategies = new Dictionary<LaborAllocationMode, ILaborDurationStrategy>
            {
                { LaborAllocationMode.Passive, new PassiveDurationStrategy() },
                { LaborAllocationMode.FixedTeam, new FixedTeamDurationStrategy() },
                { LaborAllocationMode.ScalableLabor, new ScalableLaborDurationStrategy() }
            };
        }
    }

    public decimal Calculate(ProductionStage stage, int quantity, int? assignedOperators = null)
    {
        if (_strategies.TryGetValue(stage.LaborMode, out var strategy))
        {
            return strategy.CalculateDuration(stage, quantity, assignedOperators);
        }

        return stage.CalculateDuration(quantity, assignedOperators);
    }
}
