using Krono.Domain.Entities;
using Krono.Domain.Enums;

namespace Krono.Domain.Interfaces;

public interface ILaborDurationStrategy
{
    LaborAllocationMode Mode { get; }
    decimal CalculateDuration(ProductionStage stage, int quantity, int? assignedOperators);
}
