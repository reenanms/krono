using Krono.Domain.Entities;
using Krono.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Krono.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(KronoDbContext context)
    {
        if (await context.Products.AnyAsync())
        {
            return; // Already seeded
        }

        // 1. Work Shift
        var shift1 = new WorkShift
        {
            Name = "Shift 1 (6:00 AM - 2:30 PM)",
            StartHour = new TimeSpan(6, 0, 0),
            EndHour = new TimeSpan(14, 30, 0),
            BreakStart = new TimeSpan(10, 0, 0),
            BreakEnd = new TimeSpan(10, 30, 0),
            WorkDays = "Monday,Tuesday,Wednesday,Thursday,Friday"
        };
        var shift2 = new WorkShift
        {
            Name = "Shift 2 (2:30 PM - 11:00 PM)",
            StartHour = new TimeSpan(14, 30, 0),
            EndHour = new TimeSpan(23, 0, 0),
            BreakStart = new TimeSpan(18, 0, 0),
            BreakEnd = new TimeSpan(18, 30, 0),
            WorkDays = "Monday,Tuesday,Wednesday,Thursday,Friday"
        };
        await context.WorkShifts.AddRangeAsync(shift1, shift2);

        // 2. Operator Skills
        var skillMixer = new OperatorSkill { Name = "Mixer / Baker", Description = "Prepares and mixes batters and doughs" };
        var skillDepositor = new OperatorSkill { Name = "Depositor Operator", Description = "Operates automated tray depositing machines" };
        var skillOven = new OperatorSkill { Name = "Oven Tender", Description = "Controls thermal profiles and baking processes" };
        var skillPackager = new OperatorSkill { Name = "Packager", Description = "Boxes, seals, and labels finished baked goods" };
        var skillAssistant = new OperatorSkill { Name = "General Assistant", Description = "Floor sanitation and batch replenishment" };

        await context.OperatorSkills.AddRangeAsync(skillMixer, skillDepositor, skillOven, skillPackager, skillAssistant);

        // 3. Machine Types
        var machineMixer = new MachineType { Name = "Planetary Industrial Mixer", Description = "Heavy-duty 120L planetary mixer", TotalAvailable = 2 };
        var machineDepositor = new MachineType { Name = "Automatic Cupcake Depositor", Description = "High-speed multi-nozzle depositor", TotalAvailable = 1 };
        var machineOven = new MachineType { Name = "Industrial Bakery Oven", Description = "Multi-rack convection deck oven", TotalAvailable = 2 };
        var machinePackaging = new MachineType { Name = "Packaging Station & Sealer", Description = "Ergonomic packing table with heat sealer", TotalAvailable = 3 };

        await context.MachineTypes.AddRangeAsync(machineMixer, machineDepositor, machineOven, machinePackaging);
        await context.SaveChangesAsync();

        // 4. Pilot Product: Vanilla Cupcake (Batch 1,000 units)
        var stage1 = new ProductionStage
        {
            Name = "Mix Dough",
            SequenceOrder = 1,
            MachineTypeId = machineMixer.Id,
            MachineQuantity = 1,
            OperatorSkillId = skillMixer.Id,
            DefaultOperators = 1,
            LaborMode = LaborAllocationMode.FixedTeam,
            SetupFixedMinutes = 15,
            SetupMultiplierMinutes = 0,
            ProdFixedMinutes = 75,
            ProdMultiplierMinutes = 0
        };

        var stage2 = new ProductionStage
        {
            Name = "Rest Dough",
            SequenceOrder = 2,
            MachineTypeId = null,
            MachineQuantity = 0,
            OperatorSkillId = null,
            DefaultOperators = 0,
            LaborMode = LaborAllocationMode.Passive,
            SetupFixedMinutes = 30,
            SetupMultiplierMinutes = 0,
            ProdFixedMinutes = 0,
            ProdMultiplierMinutes = 0,
            PredecessorStageIds = new List<Guid> { stage1.Id }
        };

        var stage3 = new ProductionStage
        {
            Name = "Deposit Batter",
            SequenceOrder = 3,
            MachineTypeId = machineDepositor.Id,
            MachineQuantity = 1,
            OperatorSkillId = skillDepositor.Id,
            DefaultOperators = 6,
            LaborMode = LaborAllocationMode.FixedTeam,
            SetupFixedMinutes = 20,
            SetupMultiplierMinutes = 0,
            ProdFixedMinutes = 70,
            ProdMultiplierMinutes = 0,
            PredecessorStageIds = new List<Guid> { stage2.Id }
        };

        var stage4 = new ProductionStage
        {
            Name = "Bake",
            SequenceOrder = 4,
            MachineTypeId = machineOven.Id,
            MachineQuantity = 1,
            OperatorSkillId = skillOven.Id,
            DefaultOperators = 1,
            LaborMode = LaborAllocationMode.FixedTeam,
            SetupFixedMinutes = 10,
            SetupMultiplierMinutes = 0,
            ProdFixedMinutes = 20,
            ProdMultiplierMinutes = 0,
            PredecessorStageIds = new List<Guid> { stage3.Id }
        };

        var stage5 = new ProductionStage
        {
            Name = "Cool Down",
            SequenceOrder = 5,
            MachineTypeId = null,
            MachineQuantity = 0,
            OperatorSkillId = null,
            DefaultOperators = 0,
            LaborMode = LaborAllocationMode.Passive,
            SetupFixedMinutes = 45,
            SetupMultiplierMinutes = 0,
            ProdFixedMinutes = 0,
            ProdMultiplierMinutes = 0,
            PredecessorStageIds = new List<Guid> { stage4.Id }
        };

        var stage6 = new ProductionStage
        {
            Name = "Packaging",
            SequenceOrder = 6,
            MachineTypeId = machinePackaging.Id,
            MachineQuantity = 1,
            OperatorSkillId = skillPackager.Id,
            DefaultOperators = 4,
            LaborMode = LaborAllocationMode.ScalableLabor,
            SetupFixedMinutes = 10,
            SetupMultiplierMinutes = 0,
            ProdFixedMinutes = 80,
            ProdMultiplierMinutes = 0,
            PredecessorStageIds = new List<Guid> { stage5.Id }
        };

        var cupcake = new Product
        {
            Code = "CUP-101",
            Name = "Vanilla Cupcake (Batch 1,000 un)",
            Category = "Bakery & Pastry",
            Stages = new List<ProductionStage> { stage1, stage2, stage3, stage4, stage5, stage6 }
        };

        await context.Products.AddAsync(cupcake);
        await context.SaveChangesAsync();
    }
}
