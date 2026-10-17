using Krono.Application.DTOs;
using Krono.Domain.Entities;
using Krono.Domain.Enums;
using Krono.Domain.Interfaces;
using Krono.Domain.Services;

namespace Krono.Application.Services;

public class ProductionSchedulerService
{
    private readonly IProductRepository _productRepository;
    private readonly IMachineTypeRepository _machineTypeRepository;
    private readonly IOperatorSkillRepository _skillRepository;
    private readonly IProductionOrderRepository _orderRepository;
    private readonly IWorkShiftRepository _shiftRepository;
    private readonly StageDurationCalculator _durationCalculator;
    private readonly ShiftCalendarEngine _shiftCalendarEngine;
    private readonly MachineConflictDetector _conflictDetector;
    private readonly LaborDemandAggregator _laborDemandAggregator;

    public ProductionSchedulerService(
        IProductRepository productRepository,
        IMachineTypeRepository machineTypeRepository,
        IOperatorSkillRepository skillRepository,
        IProductionOrderRepository orderRepository,
        IWorkShiftRepository shiftRepository,
        StageDurationCalculator durationCalculator,
        ShiftCalendarEngine shiftCalendarEngine,
        MachineConflictDetector conflictDetector,
        LaborDemandAggregator laborDemandAggregator)
    {
        _productRepository = productRepository;
        _machineTypeRepository = machineTypeRepository;
        _skillRepository = skillRepository;
        _orderRepository = orderRepository;
        _shiftRepository = shiftRepository;
        _durationCalculator = durationCalculator;
        _shiftCalendarEngine = shiftCalendarEngine;
        _conflictDetector = conflictDetector;
        _laborDemandAggregator = laborDemandAggregator;
    }

    public async Task<SimulationResponse> SimulateAsync(SimulationRequest request)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId)
            ?? throw new KeyNotFoundException($"Product with ID {request.ProductId} was not found.");

        var shift = await _shiftRepository.GetCurrentShiftAsync();
        var machineTypes = await _machineTypeRepository.GetAllAsync();
        var skills = await _skillRepository.GetAllAsync();

        var stageResults = new List<SimulatedStageDto>();
        var stageEndTimes = new Dictionary<Guid, DateTime>();

        // Sort stages topologically by SequenceOrder
        var sortedStages = product.Stages.OrderBy(s => s.SequenceOrder).ToList();
        var currentStart = _shiftCalendarEngine.EnsureWithinWorkingTime(request.PlannedStartTime, shift);

        foreach (var stage in sortedStages)
        {
            DateTime stageStart = currentStart;

            // Predecessors constraint: Start >= max(End(predecessors))
            if (stage.PredecessorStageIds.Any())
            {
                var maxPredEnd = stage.PredecessorStageIds
                    .Where(stageEndTimes.ContainsKey)
                    .Select(id => stageEndTimes[id])
                    .DefaultIfEmpty(currentStart)
                    .Max();

                if (maxPredEnd > stageStart)
                {
                    stageStart = maxPredEnd;
                }
            }

            stageStart = _shiftCalendarEngine.EnsureWithinWorkingTime(stageStart, shift);

            int assignedOps = stage.DefaultOperators;
            if (request.StageOperatorOverrides != null &&
                request.StageOperatorOverrides.TryGetValue(stage.Id, out var customOps))
            {
                assignedOps = customOps;
            }

            decimal duration = _durationCalculator.Calculate(stage, request.Quantity, assignedOps);
            DateTime stageEnd = _shiftCalendarEngine.CalculateEndTime(stageStart, duration, shift);
            stageEndTimes[stage.Id] = stageEnd;

            stageResults.Add(new SimulatedStageDto
            {
                StageId = stage.Id,
                StageName = stage.Name,
                SequenceOrder = stage.SequenceOrder,
                LaborMode = stage.LaborMode,
                DurationMinutes = duration,
                StartTime = stageStart,
                EndTime = stageEnd,
                AssignedOperators = assignedOps,
                SkillName = skills.FirstOrDefault(s => s.Id == stage.OperatorSkillId)?.Name,
                MachineName = machineTypes.FirstOrDefault(m => m.Id == stage.MachineTypeId)?.Name,
                HasConflict = false
            });
        }

        var totalDuration = stageResults.Any()
            ? (decimal)(stageResults.Max(s => s.EndTime) - stageResults.Min(s => s.StartTime)).TotalMinutes
            : 0;

        return new SimulationResponse
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Quantity = request.Quantity,
            TotalDurationMinutes = totalDuration,
            EstimatedStartTime = stageResults.FirstOrDefault()?.StartTime ?? currentStart,
            EstimatedEndTime = stageResults.LastOrDefault()?.EndTime ?? currentStart,
            Stages = stageResults
        };
    }

    public async Task<ProductionOrderDto> CreateOrderAsync(CreateOrderRequest request)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId)
            ?? throw new KeyNotFoundException($"Product with ID {request.ProductId} was not found.");

        var shift = await _shiftRepository.GetCurrentShiftAsync();
        var order = new ProductionOrder
        {
            Code = string.IsNullOrWhiteSpace(request.Code) ? $"PO-{DateTime.UtcNow.Ticks % 100000}" : request.Code,
            ProductId = product.Id,
            Quantity = request.Quantity,
            PlannedStartTime = request.PlannedStartTime
        };

        var stageEndTimes = new Dictionary<Guid, DateTime>();
        var sortedStages = product.Stages.OrderBy(s => s.SequenceOrder).ToList();
        var currentStart = _shiftCalendarEngine.EnsureWithinWorkingTime(request.PlannedStartTime, shift);

        foreach (var stage in sortedStages)
        {
            DateTime stageStart = currentStart;

            if (stage.PredecessorStageIds.Any())
            {
                var maxPredEnd = stage.PredecessorStageIds
                    .Where(stageEndTimes.ContainsKey)
                    .Select(id => stageEndTimes[id])
                    .DefaultIfEmpty(currentStart)
                    .Max();

                if (maxPredEnd > stageStart)
                {
                    stageStart = maxPredEnd;
                }
            }

            stageStart = _shiftCalendarEngine.EnsureWithinWorkingTime(stageStart, shift);

            int assignedOps = stage.DefaultOperators;
            if (request.StageOperatorOverrides != null &&
                request.StageOperatorOverrides.TryGetValue(stage.Id, out var customOps))
            {
                assignedOps = customOps;
            }

            decimal duration = _durationCalculator.Calculate(stage, request.Quantity, assignedOps);
            DateTime stageEnd = _shiftCalendarEngine.CalculateEndTime(stageStart, duration, shift);
            stageEndTimes[stage.Id] = stageEnd;

            order.ScheduledStages.Add(new ScheduledStage
            {
                ProductionStageId = stage.Id,
                AssignedOperators = assignedOps,
                StartTime = stageStart,
                EndTime = stageEnd,
                DurationMinutes = duration,
                HasMachineConflict = false
            });
        }

        await _orderRepository.AddAsync(order);

        // Run conflict detection on the schedule
        await RefreshMachineConflictsAsync();

        return await MapToOrderDto(order.Id);
    }

    public async Task<ScheduledStageDto> UpdateScheduledStageAsync(Guid scheduledStageId, UpdateScheduledStageRequest request)
    {
        var scheduledStage = await _orderRepository.GetScheduledStageByIdAsync(scheduledStageId)
            ?? throw new KeyNotFoundException($"Scheduled stage {scheduledStageId} not found.");

        var order = await _orderRepository.GetByIdAsync(scheduledStage.ProductionOrderId)
            ?? throw new KeyNotFoundException($"Order {scheduledStage.ProductionOrderId} not found.");

        var product = await _productRepository.GetByIdAsync(order.ProductId)
            ?? throw new KeyNotFoundException($"Product {order.ProductId} not found.");

        var shift = await _shiftRepository.GetCurrentShiftAsync();
        var stageDefinition = product.Stages.FirstOrDefault(s => s.Id == scheduledStage.ProductionStageId);

        if (stageDefinition != null)
        {
            if (request.AssignedOperators.HasValue && request.AssignedOperators.Value > 0)
            {
                scheduledStage.AssignedOperators = request.AssignedOperators.Value;
                // Recalculate duration if scalable
                scheduledStage.DurationMinutes = _durationCalculator.Calculate(
                    stageDefinition, order.Quantity, scheduledStage.AssignedOperators);
            }

            if (request.StartTime.HasValue)
            {
                scheduledStage.StartTime = _shiftCalendarEngine.EnsureWithinWorkingTime(request.StartTime.Value, shift);
            }

            // Recalculate end time
            scheduledStage.EndTime = _shiftCalendarEngine.CalculateEndTime(
                scheduledStage.StartTime, scheduledStage.DurationMinutes, shift);

            // Cascade downstream stages in the same order
            CascadeOrderStages(order, shift);
        }

        await _orderRepository.UpdateAsync(order);
        await RefreshMachineConflictsAsync();

        var refreshed = await _orderRepository.GetScheduledStageByIdAsync(scheduledStageId);
        return MapToScheduledStageDto(refreshed!);
    }

    private void CascadeOrderStages(ProductionOrder order, WorkShift shift)
    {
        var stageEndTimes = order.ScheduledStages.ToDictionary(s => s.ProductionStageId, s => s.EndTime);
        var sorted = order.ScheduledStages.OrderBy(s => s.ProductionStage?.SequenceOrder ?? 0).ToList();

        foreach (var s in sorted)
        {
            var stageDef = s.ProductionStage;
            if (stageDef == null) continue;

            if (stageDef.PredecessorStageIds.Any())
            {
                var maxPredEnd = stageDef.PredecessorStageIds
                    .Where(stageEndTimes.ContainsKey)
                    .Select(id => stageEndTimes[id])
                    .DefaultIfEmpty(s.StartTime)
                    .Max();

                if (maxPredEnd > s.StartTime)
                {
                    s.StartTime = _shiftCalendarEngine.EnsureWithinWorkingTime(maxPredEnd, shift);
                    s.EndTime = _shiftCalendarEngine.CalculateEndTime(s.StartTime, s.DurationMinutes, shift);
                    stageEndTimes[s.ProductionStageId] = s.EndTime;
                }
            }
        }
    }

    public async Task RefreshMachineConflictsAsync()
    {
        var allOrders = await _orderRepository.GetAllAsync();
        var allStages = allOrders.SelectMany(o => o.ScheduledStages).ToList();
        var machineTypes = await _machineTypeRepository.GetAllAsync();

        _conflictDetector.DetectAndFlagConflicts(allStages, machineTypes);

        foreach (var order in allOrders)
        {
            await _orderRepository.UpdateAsync(order);
        }
    }

    public async Task<ProductionScheduleReportDto> GetScheduleReportAsync(string viewType, DateTime startDate, DateTime? endDate = null)
    {
        var finalEnd = viewType.ToLowerInvariant() == "day"
            ? startDate.Date.AddDays(1).AddTicks(-1)
            : (endDate ?? startDate.Date.AddDays(7).AddTicks(-1));

        var orders = await _orderRepository.GetByDateRangeAsync(startDate.Date, finalEnd);
        var allStages = orders.SelectMany(o => o.ScheduledStages).ToList();
        var skills = await _skillRepository.GetAllAsync();
        var machineTypes = await _machineTypeRepository.GetAllAsync();

        _conflictDetector.DetectAndFlagConflicts(allStages, machineTypes);

        var laborMetrics = _laborDemandAggregator.AggregateDemand(allStages, skills);

        var mappedEvents = allStages.Select(MapToScheduledStageDto).ToList();

        return new ProductionScheduleReportDto
        {
            ViewType = viewType,
            StartDate = startDate.Date,
            EndDate = finalEnd,
            LaborSummary = laborMetrics.Select(m => new LaborSizingSummaryDto
            {
                SkillId = m.SkillId,
                SkillName = m.SkillName,
                PeakWorkers = m.PeakWorkers,
                TotalManHours = m.TotalManHours
            }).ToList(),
            Events = mappedEvents
        };
    }

    public async Task<List<ScheduledStageDto>> GetEventsAsync(DateTime startDate, DateTime endDate)
    {
        var orders = await _orderRepository.GetByDateRangeAsync(startDate, endDate);
        var stages = orders.SelectMany(o => o.ScheduledStages).ToList();
        var machineTypes = await _machineTypeRepository.GetAllAsync();
        _conflictDetector.DetectAndFlagConflicts(stages, machineTypes);
        return stages.Select(MapToScheduledStageDto).ToList();
    }

    private ScheduledStageDto MapToScheduledStageDto(ScheduledStage s)
    {
        return new ScheduledStageDto
        {
            Id = s.Id,
            ProductionOrderId = s.ProductionOrderId,
            OrderCode = s.ProductionOrder?.Code ?? string.Empty,
            ProductId = s.ProductionOrder?.ProductId ?? Guid.Empty,
            ProductName = s.ProductionOrder?.Product?.Name ?? string.Empty,
            ProductionStageId = s.ProductionStageId,
            StageName = s.ProductionStage?.Name ?? "Stage",
            SequenceOrder = s.ProductionStage?.SequenceOrder ?? 0,
            LaborMode = s.ProductionStage?.LaborMode ?? LaborAllocationMode.FixedTeam,
            AssignedOperators = s.AssignedOperators,
            OperatorSkillName = s.ProductionStage?.OperatorSkill?.Name,
            MachineTypeName = s.ProductionStage?.MachineType?.Name,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            DurationMinutes = s.DurationMinutes,
            HasMachineConflict = s.HasMachineConflict
        };
    }

    private async Task<ProductionOrderDto> MapToOrderDto(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        return new ProductionOrderDto
        {
            Id = order.Id,
            Code = order.Code,
            ProductId = order.ProductId,
            ProductName = order.Product?.Name ?? string.Empty,
            Quantity = order.Quantity,
            PlannedStartTime = order.PlannedStartTime,
            ScheduledStages = order.ScheduledStages.Select(MapToScheduledStageDto).ToList()
        };
    }
}
