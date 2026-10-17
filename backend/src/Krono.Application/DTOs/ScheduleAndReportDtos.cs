using Krono.Domain.Enums;

namespace Krono.Application.DTOs;

public class SimulatedStageDto
{
    public Guid StageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int SequenceOrder { get; set; }
    public LaborAllocationMode LaborMode { get; set; }
    public decimal DurationMinutes { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int AssignedOperators { get; set; }
    public string? SkillName { get; set; }
    public string? MachineName { get; set; }
    public bool HasConflict { get; set; }
}

public class SimulationRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime PlannedStartTime { get; set; }
    public Dictionary<Guid, int>? StageOperatorOverrides { get; set; }
}

public class SimulationResponse
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalDurationMinutes { get; set; }
    public DateTime EstimatedStartTime { get; set; }
    public DateTime EstimatedEndTime { get; set; }
    public List<SimulatedStageDto> Stages { get; set; } = new();
}

public class CreateOrderRequest
{
    public string Code { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime PlannedStartTime { get; set; }
    public Dictionary<Guid, int>? StageOperatorOverrides { get; set; }
}

public class UpdateScheduledStageRequest
{
    public DateTime? StartTime { get; set; }
    public int? AssignedOperators { get; set; }
}

public class ScheduledStageDto
{
    public Guid Id { get; set; }
    public Guid ProductionOrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid ProductionStageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int SequenceOrder { get; set; }
    public LaborAllocationMode LaborMode { get; set; }
    public int AssignedOperators { get; set; }
    public string? OperatorSkillName { get; set; }
    public string? MachineTypeName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal DurationMinutes { get; set; }
    public bool HasMachineConflict { get; set; }
}

public class ProductionOrderDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime PlannedStartTime { get; set; }
    public List<ScheduledStageDto> ScheduledStages { get; set; } = new();
}

public class LaborSizingSummaryDto
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int PeakWorkers { get; set; }
    public decimal TotalManHours { get; set; }
}

public class ProductionScheduleReportDto
{
    public string ViewType { get; set; } = "week"; // "day" or "week"
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<LaborSizingSummaryDto> LaborSummary { get; set; } = new();
    public List<ScheduledStageDto> Events { get; set; } = new();
}
