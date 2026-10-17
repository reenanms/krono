namespace Krono.Domain.Entities;

public class MachineType
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TotalAvailable { get; set; } = 1;
}
