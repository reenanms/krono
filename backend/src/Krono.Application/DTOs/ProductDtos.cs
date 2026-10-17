using Krono.Domain.Enums;

namespace Krono.Application.DTOs;

public class ProductionStageDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SequenceOrder { get; set; }
    public Guid? MachineTypeId { get; set; }
    public string? MachineTypeName { get; set; }
    public int MachineQuantity { get; set; }
    public Guid? OperatorSkillId { get; set; }
    public string? OperatorSkillName { get; set; }
    public int DefaultOperators { get; set; }
    public LaborAllocationMode LaborMode { get; set; }
    public decimal SetupFixedMinutes { get; set; }
    public decimal SetupMultiplierMinutes { get; set; }
    public decimal ProdFixedMinutes { get; set; }
    public decimal ProdMultiplierMinutes { get; set; }
    public List<Guid> PredecessorStageIds { get; set; } = new();
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<ProductionStageDto> Stages { get; set; } = new();
}

public class CreateStageRequest
{
    public string Name { get; set; } = string.Empty;
    public int SequenceOrder { get; set; }
    public Guid? MachineTypeId { get; set; }
    public int MachineQuantity { get; set; } = 1;
    public Guid? OperatorSkillId { get; set; }
    public int DefaultOperators { get; set; }
    public LaborAllocationMode LaborMode { get; set; }
    public decimal SetupFixedMinutes { get; set; }
    public decimal SetupMultiplierMinutes { get; set; }
    public decimal ProdFixedMinutes { get; set; }
    public decimal ProdMultiplierMinutes { get; set; }
    public List<Guid> PredecessorStageIds { get; set; } = new();
}

public class CreateProductRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<CreateStageRequest> Stages { get; set; } = new();
}

public class UpdateProductRequest : CreateProductRequest
{
}
