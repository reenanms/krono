using Krono.Domain.Enums;

namespace Krono.Domain.Entities;

public class ProductionStage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SequenceOrder { get; set; }
    
    // Machine requirement
    public Guid? MachineTypeId { get; set; }
    public MachineType? MachineType { get; set; }
    public int MachineQuantity { get; set; } = 1;

    // Labor requirement
    public Guid? OperatorSkillId { get; set; }
    public OperatorSkill? OperatorSkill { get; set; }
    public int DefaultOperators { get; set; } = 0;
    public LaborAllocationMode LaborMode { get; set; } = LaborAllocationMode.FixedTeam;

    // Time Parameters (stored in minutes)
    public decimal SetupFixedMinutes { get; set; } = 0;
    public decimal SetupMultiplierMinutes { get; set; } = 0;
    public decimal ProdFixedMinutes { get; set; } = 0;
    public decimal ProdMultiplierMinutes { get; set; } = 0;

    // Predecessors in the routing DAG
    public List<Guid> PredecessorStageIds { get; set; } = new();

    public decimal CalculateDuration(int quantity, int? assignedOperators = null)
    {
        var setup = SetupFixedMinutes + (SetupMultiplierMinutes * quantity);
        var prod = ProdFixedMinutes + (ProdMultiplierMinutes * quantity);

        return LaborMode switch
        {
            LaborAllocationMode.Passive => setup + prod,
            LaborAllocationMode.FixedTeam => setup + prod,
            LaborAllocationMode.ScalableLabor => 
                setup + (assignedOperators.HasValue && assignedOperators.Value > 0 
                    ? prod / assignedOperators.Value 
                    : (DefaultOperators > 0 ? prod / DefaultOperators : prod)),
            _ => setup + prod
        };
    }
}
