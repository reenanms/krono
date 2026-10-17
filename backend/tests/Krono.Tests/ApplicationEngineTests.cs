using Krono.Application.Services;
using Krono.Domain.Entities;
using Krono.Domain.Enums;
using Xunit;

namespace Krono.Tests;

public class ApplicationEngineTests
{
    [Fact]
    public void ShiftCalendarEngine_ShouldSkipLunchBreak()
    {
        var engine = new ShiftCalendarEngine();
        var shift = new WorkShift
        {
            StartHour = new TimeSpan(8, 0, 0),
            EndHour = new TimeSpan(17, 0, 0),
            BreakStart = new TimeSpan(12, 0, 0),
            BreakEnd = new TimeSpan(13, 0, 0),
            WorkDays = "Monday,Tuesday,Wednesday,Thursday,Friday"
        };

        // Start at 11:30 AM on Monday, duration 60 minutes
        // 11:30 to 12:00 = 30 mins
        // 12:00 to 13:00 = break (0 mins)
        // 13:00 to 13:30 = remaining 30 mins -> finishes at 13:30
        var monday = new DateTime(2026, 10, 12, 11, 30, 0); // Oct 12, 2026 is Monday
        var endTime = engine.CalculateEndTime(monday, 60, shift);

        Assert.Equal(new DateTime(2026, 10, 12, 13, 30, 0), endTime);
    }

    [Fact]
    public void MachineConflictDetector_ShouldFlagOverload_WhenDemandExceedsAvailable()
    {
        var detector = new MachineConflictDetector();
        var mixerType = new MachineType
        {
            Id = Guid.NewGuid(),
            Name = "Planetary Mixer",
            TotalAvailable = 1
        };

        var stageDef = new ProductionStage
        {
            Id = Guid.NewGuid(),
            Name = "Mix Dough",
            MachineTypeId = mixerType.Id,
            MachineQuantity = 1
        };

        // Two overlapping stages requiring 1 mixer each -> Total 2 needed, but only 1 available!
        var stage1 = new ScheduledStage
        {
            Id = Guid.NewGuid(),
            ProductionStage = stageDef,
            StartTime = new DateTime(2026, 10, 12, 9, 0, 0),
            EndTime = new DateTime(2026, 10, 12, 10, 0, 0)
        };

        var stage2 = new ScheduledStage
        {
            Id = Guid.NewGuid(),
            ProductionStage = stageDef,
            StartTime = new DateTime(2026, 10, 12, 9, 30, 0),
            EndTime = new DateTime(2026, 10, 12, 10, 30, 0)
        };

        var conflicts = detector.DetectAndFlagConflicts(new[] { stage1, stage2 }, new[] { mixerType });

        Assert.NotEmpty(conflicts);
        Assert.True(stage1.HasMachineConflict);
        Assert.True(stage2.HasMachineConflict);
    }

    [Fact]
    public void LaborDemandAggregator_ShouldCalculatePeakAndTotalManHours()
    {
        var aggregator = new LaborDemandAggregator();
        var bakerSkill = new OperatorSkill
        {
            Id = Guid.NewGuid(),
            Name = "Baker"
        };

        var stageDef = new ProductionStage
        {
            Id = Guid.NewGuid(),
            Name = "Bake",
            OperatorSkillId = bakerSkill.Id,
            LaborMode = LaborAllocationMode.FixedTeam
        };

        // 2 stages running in parallel for 1 hour, each with 2 bakers
        // Peak = 4 bakers. Total = 2 * (1h * 2) = 4 man-hours.
        var stage1 = new ScheduledStage
        {
            ProductionStage = stageDef,
            AssignedOperators = 2,
            DurationMinutes = 60,
            StartTime = new DateTime(2026, 10, 12, 8, 0, 0),
            EndTime = new DateTime(2026, 10, 12, 9, 0, 0)
        };

        var stage2 = new ScheduledStage
        {
            ProductionStage = stageDef,
            AssignedOperators = 2,
            DurationMinutes = 60,
            StartTime = new DateTime(2026, 10, 12, 8, 0, 0),
            EndTime = new DateTime(2026, 10, 12, 9, 0, 0)
        };

        var metrics = aggregator.AggregateDemand(new[] { stage1, stage2 }, new[] { bakerSkill });

        var bakerMetric = metrics.First(m => m.SkillId == bakerSkill.Id);
        Assert.Equal(4, bakerMetric.PeakWorkers);
        Assert.Equal(4m, bakerMetric.TotalManHours);
    }
}
