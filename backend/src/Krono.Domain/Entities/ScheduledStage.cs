namespace Krono.Domain.Entities;

public class ScheduledStage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductionOrderId { get; set; }
    public ProductionOrder? ProductionOrder { get; set; }
    public Guid ProductionStageId { get; set; }
    public ProductionStage? ProductionStage { get; set; }
    public int AssignedOperators { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal DurationMinutes { get; set; }
    public bool HasMachineConflict { get; set; }
}
