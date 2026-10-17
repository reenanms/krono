using Krono.Domain.Entities;

namespace Krono.Application.Services;

public class MachineConflictInfo
{
    public Guid MachineTypeId { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public DateTime ConflictStart { get; set; }
    public DateTime ConflictEnd { get; set; }
    public int RequiredCount { get; set; }
    public int AvailableCount { get; set; }
    public string Message => $"Overload: {RequiredCount} {MachineName} required, only {AvailableCount} physically available.";
}

public class MachineConflictDetector
{
    public List<MachineConflictInfo> DetectAndFlagConflicts(
        IEnumerable<ScheduledStage> scheduledStages,
        IEnumerable<MachineType> machineTypes)
    {
        var machineMap = machineTypes.ToDictionary(m => m.Id);
        var conflicts = new List<MachineConflictInfo>();

        // Clear existing conflict flags first
        foreach (var stage in scheduledStages)
        {
            stage.HasMachineConflict = false;
        }

        // Group stages by machine type
        var stagesByMachine = scheduledStages
            .Where(s => s.ProductionStage?.MachineTypeId.HasValue == true)
            .GroupBy(s => s.ProductionStage!.MachineTypeId!.Value);

        foreach (var group in stagesByMachine)
        {
            if (!machineMap.TryGetValue(group.Key, out var machineType))
                continue;

            int available = machineType.TotalAvailable;
            var stages = group.OrderBy(s => s.StartTime).ToList();

            // Collect time points
            var timePoints = stages.Select(s => s.StartTime)
                .Concat(stages.Select(s => s.EndTime))
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            for (int i = 0; i < timePoints.Count - 1; i++)
            {
                var intervalStart = timePoints[i];
                var intervalEnd = timePoints[i + 1];

                // Find active stages in this interval
                var activeStages = stages
                    .Where(s => s.StartTime <= intervalStart && s.EndTime >= intervalEnd)
                    .ToList();

                int required = activeStages.Sum(s => s.ProductionStage?.MachineQuantity ?? 1);

                if (required > available)
                {
                    foreach (var s in activeStages)
                    {
                        s.HasMachineConflict = true;
                    }

                    conflicts.Add(new MachineConflictInfo
                    {
                        MachineTypeId = machineType.Id,
                        MachineName = machineType.Name,
                        ConflictStart = intervalStart,
                        ConflictEnd = intervalEnd,
                        RequiredCount = required,
                        AvailableCount = available
                    });
                }
            }
        }

        return conflicts;
    }
}
