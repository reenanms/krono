namespace Krono.Domain.Entities;

public class ProductionOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public DateTime PlannedStartTime { get; set; }
    public List<ScheduledStage> ScheduledStages { get; set; } = new();
}
