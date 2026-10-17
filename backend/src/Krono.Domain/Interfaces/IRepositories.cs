using Krono.Domain.Entities;

namespace Krono.Domain.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(Guid id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Guid id);
}

public interface IMachineTypeRepository
{
    Task<List<MachineType>> GetAllAsync();
    Task<MachineType?> GetByIdAsync(Guid id);
    Task AddAsync(MachineType machineType);
    Task UpdateAsync(MachineType machineType);
    Task DeleteAsync(Guid id);
}

public interface IOperatorSkillRepository
{
    Task<List<OperatorSkill>> GetAllAsync();
    Task<OperatorSkill?> GetByIdAsync(Guid id);
    Task AddAsync(OperatorSkill skill);
    Task UpdateAsync(OperatorSkill skill);
    Task DeleteAsync(Guid id);
}

public interface IProductionOrderRepository
{
    Task<List<ProductionOrder>> GetAllAsync();
    Task<List<ProductionOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<ProductionOrder?> GetByIdAsync(Guid id);
    Task AddAsync(ProductionOrder order);
    Task UpdateAsync(ProductionOrder order);
    Task DeleteAsync(Guid id);
    Task<ScheduledStage?> GetScheduledStageByIdAsync(Guid scheduledStageId);
    Task UpdateScheduledStageAsync(ScheduledStage scheduledStage);
}

public interface IWorkShiftRepository
{
    Task<WorkShift> GetCurrentShiftAsync();
    Task UpdateShiftAsync(WorkShift shift);
}
