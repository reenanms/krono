using Krono.Application.DTOs;
using Krono.Application.Services;
using Krono.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Krono.API.Controllers;

[ApiController]
[Route("api/v1/schedules")]
public class SchedulesController : ControllerBase
{
    private readonly ProductionSchedulerService _schedulerService;
    private readonly IProductionOrderRepository _orderRepository;

    public SchedulesController(
        ProductionSchedulerService schedulerService,
        IProductionOrderRepository orderRepository)
    {
        _schedulerService = schedulerService;
        _orderRepository = orderRepository;
    }

    [HttpPost("simulate")]
    public async Task<ActionResult<SimulationResponse>> Simulate([FromBody] SimulationRequest request)
    {
        try
        {
            var result = await _schedulerService.SimulateAsync(request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("orders")]
    public async Task<ActionResult<ProductionOrderDto>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var order = await _schedulerService.CreateOrderAsync(request);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("orders")]
    public async Task<ActionResult<List<ProductionOrderDto>>> GetAllOrders()
    {
        var orders = await _orderRepository.GetAllAsync();
        return Ok(orders.Select(o => new ProductionOrderDto
        {
            Id = o.Id,
            Code = o.Code,
            ProductId = o.ProductId,
            ProductName = o.Product?.Name ?? string.Empty,
            Quantity = o.Quantity,
            PlannedStartTime = o.PlannedStartTime,
            ScheduledStages = o.ScheduledStages.Select(s => new ScheduledStageDto
            {
                Id = s.Id,
                ProductionOrderId = s.ProductionOrderId,
                OrderCode = o.Code,
                ProductId = o.ProductId,
                ProductName = o.Product?.Name ?? string.Empty,
                ProductionStageId = s.ProductionStageId,
                StageName = s.ProductionStage?.Name ?? "Stage",
                SequenceOrder = s.ProductionStage?.SequenceOrder ?? 0,
                LaborMode = s.ProductionStage?.LaborMode ?? Domain.Enums.LaborAllocationMode.FixedTeam,
                AssignedOperators = s.AssignedOperators,
                OperatorSkillName = s.ProductionStage?.OperatorSkill?.Name,
                MachineTypeName = s.ProductionStage?.MachineType?.Name,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                DurationMinutes = s.DurationMinutes,
                HasMachineConflict = s.HasMachineConflict
            }).ToList()
        }));
    }

    [HttpGet("events")]
    public async Task<ActionResult<List<ScheduledStageDto>>> GetEvents(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var events = await _schedulerService.GetEventsAsync(startDate, endDate);
        return Ok(events);
    }

    [HttpPatch("events/{scheduledStageId:guid}")]
    public async Task<ActionResult<ScheduledStageDto>> UpdateScheduledStage(
        Guid scheduledStageId,
        [FromBody] UpdateScheduledStageRequest request)
    {
        try
        {
            var updated = await _schedulerService.UpdateScheduledStageAsync(scheduledStageId, request);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpDelete("orders/{orderId:guid}")]
    public async Task<IActionResult> DeleteOrder(Guid orderId)
    {
        await _orderRepository.DeleteAsync(orderId);
        await _schedulerService.RefreshMachineConflictsAsync();
        return NoContent();
    }
}
