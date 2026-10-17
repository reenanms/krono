using Krono.Domain.Entities;
using Krono.Domain.Interfaces;
using Krono.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Krono.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly KronoDbContext _context;

    public ProductRepository(KronoDbContext context) => _context = context;

    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Stages)
                .ThenInclude(s => s.MachineType)
            .Include(p => p.Stages)
                .ThenInclude(s => s.OperatorSkill)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products
            .Include(p => p.Stages)
                .ThenInclude(s => s.MachineType)
            .Include(p => p.Stages)
                .ThenInclude(s => s.OperatorSkill)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}

public class MachineTypeRepository : IMachineTypeRepository
{
    private readonly KronoDbContext _context;

    public MachineTypeRepository(KronoDbContext context) => _context = context;

    public async Task<List<MachineType>> GetAllAsync()
    {
        return await _context.MachineTypes.AsNoTracking().ToListAsync();
    }

    public async Task<MachineType?> GetByIdAsync(Guid id)
    {
        return await _context.MachineTypes.FindAsync(id);
    }

    public async Task AddAsync(MachineType machineType)
    {
        await _context.MachineTypes.AddAsync(machineType);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MachineType machineType)
    {
        _context.MachineTypes.Update(machineType);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await _context.MachineTypes.FindAsync(id);
        if (item != null)
        {
            _context.MachineTypes.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}

public class OperatorSkillRepository : IOperatorSkillRepository
{
    private readonly KronoDbContext _context;

    public OperatorSkillRepository(KronoDbContext context) => _context = context;

    public async Task<List<OperatorSkill>> GetAllAsync()
    {
        return await _context.OperatorSkills.AsNoTracking().ToListAsync();
    }

    public async Task<OperatorSkill?> GetByIdAsync(Guid id)
    {
        return await _context.OperatorSkills.FindAsync(id);
    }

    public async Task AddAsync(OperatorSkill skill)
    {
        await _context.OperatorSkills.AddAsync(skill);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(OperatorSkill skill)
    {
        _context.OperatorSkills.Update(skill);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await _context.OperatorSkills.FindAsync(id);
        if (item != null)
        {
            _context.OperatorSkills.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}

public class ProductionOrderRepository : IProductionOrderRepository
{
    private readonly KronoDbContext _context;

    public ProductionOrderRepository(KronoDbContext context) => _context = context;

    public async Task<List<ProductionOrder>> GetAllAsync()
    {
        return await _context.ProductionOrders
            .Include(o => o.Product)
            .Include(o => o.ScheduledStages)
                .ThenInclude(s => s.ProductionStage)
                    .ThenInclude(ps => ps!.MachineType)
            .Include(o => o.ScheduledStages)
                .ThenInclude(s => s.ProductionStage)
                    .ThenInclude(ps => ps!.OperatorSkill)
            .OrderBy(o => o.PlannedStartTime)
            .ToListAsync();
    }

    public async Task<List<ProductionOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.ProductionOrders
            .Include(o => o.Product)
            .Include(o => o.ScheduledStages)
                .ThenInclude(s => s.ProductionStage)
                    .ThenInclude(ps => ps!.MachineType)
            .Include(o => o.ScheduledStages)
                .ThenInclude(s => s.ProductionStage)
                    .ThenInclude(ps => ps!.OperatorSkill)
            .Where(o => o.ScheduledStages.Any(s => s.StartTime <= endDate && s.EndTime >= startDate))
            .OrderBy(o => o.PlannedStartTime)
            .ToListAsync();
    }

    public async Task<ProductionOrder?> GetByIdAsync(Guid id)
    {
        return await _context.ProductionOrders
            .Include(o => o.Product)
            .Include(o => o.ScheduledStages)
                .ThenInclude(s => s.ProductionStage)
                    .ThenInclude(ps => ps!.MachineType)
            .Include(o => o.ScheduledStages)
                .ThenInclude(s => s.ProductionStage)
                    .ThenInclude(ps => ps!.OperatorSkill)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task AddAsync(ProductionOrder order)
    {
        await _context.ProductionOrders.AddAsync(order);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ProductionOrder order)
    {
        _context.ProductionOrders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await _context.ProductionOrders.FindAsync(id);
        if (item != null)
        {
            _context.ProductionOrders.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<ScheduledStage?> GetScheduledStageByIdAsync(Guid scheduledStageId)
    {
        return await _context.ScheduledStages
            .Include(s => s.ProductionOrder)
                .ThenInclude(o => o!.Product)
            .Include(s => s.ProductionStage)
                .ThenInclude(ps => ps!.MachineType)
            .Include(s => s.ProductionStage)
                .ThenInclude(ps => ps!.OperatorSkill)
            .FirstOrDefaultAsync(s => s.Id == scheduledStageId);
    }

    public async Task UpdateScheduledStageAsync(ScheduledStage scheduledStage)
    {
        _context.ScheduledStages.Update(scheduledStage);
        await _context.SaveChangesAsync();
    }
}

public class WorkShiftRepository : IWorkShiftRepository
{
    private readonly KronoDbContext _context;

    public WorkShiftRepository(KronoDbContext context) => _context = context;

    public async Task<WorkShift> GetCurrentShiftAsync()
    {
        var shift = await _context.WorkShifts.FirstOrDefaultAsync();
        if (shift == null)
        {
            shift = new WorkShift();
            await _context.WorkShifts.AddAsync(shift);
            await _context.SaveChangesAsync();
        }
        return shift;
    }

    public async Task UpdateShiftAsync(WorkShift shift)
    {
        _context.WorkShifts.Update(shift);
        await _context.SaveChangesAsync();
    }
}
