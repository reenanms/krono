using Krono.Domain.Entities;
using Krono.Domain.Enums;

namespace Krono.Application.Services;

public class SkillDemandMetric
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int PeakWorkers { get; set; }
    public decimal TotalManHours { get; set; }
}

public class LaborDemandAggregator
{
    public List<SkillDemandMetric> AggregateDemand(
        IEnumerable<ScheduledStage> scheduledStages,
        IEnumerable<OperatorSkill> skills)
    {
        var skillMap = skills.ToDictionary(s => s.Id);
        var result = new List<SkillDemandMetric>();

        var stagesWithSkill = scheduledStages
            .Where(s => s.ProductionStage?.OperatorSkillId.HasValue == true &&
                        s.ProductionStage.LaborMode != LaborAllocationMode.Passive)
            .GroupBy(s => s.ProductionStage!.OperatorSkillId!.Value);

        foreach (var group in stagesWithSkill)
        {
            var skillId = group.Key;
            string skillName = skillMap.TryGetValue(skillId, out var sk) ? sk.Name : "Unknown Skill";
            var stages = group.ToList();

            // Total man hours: sum of (duration in hours * assigned operators)
            decimal totalManHours = stages.Sum(s => (s.DurationMinutes / 60m) * s.AssignedOperators);

            // Peak workers calculation across all points in time
            var timePoints = stages.Select(s => s.StartTime)
                .Concat(stages.Select(s => s.EndTime))
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            int peakWorkers = 0;

            for (int i = 0; i < timePoints.Count - 1; i++)
            {
                var start = timePoints[i];
                var end = timePoints[i + 1];

                var concurrentWorkers = stages
                    .Where(s => s.StartTime <= start && s.EndTime >= end)
                    .Sum(s => s.AssignedOperators);

                if (concurrentWorkers > peakWorkers)
                {
                    peakWorkers = concurrentWorkers;
                }
            }

            // Fallback if stages are instantaneous or single
            if (peakWorkers == 0 && stages.Any())
            {
                peakWorkers = stages.Max(s => s.AssignedOperators);
            }

            result.Add(new SkillDemandMetric
            {
                SkillId = skillId,
                SkillName = skillName,
                PeakWorkers = peakWorkers,
                TotalManHours = Math.Round(totalManHours, 2)
            });
        }

        // Include any skills with 0 demand so reporting is complete
        foreach (var skill in skills)
        {
            if (!result.Any(r => r.SkillId == skill.Id))
            {
                result.Add(new SkillDemandMetric
                {
                    SkillId = skill.Id,
                    SkillName = skill.Name,
                    PeakWorkers = 0,
                    TotalManHours = 0
                });
            }
        }

        return result.OrderBy(r => r.SkillName).ToList();
    }
}
