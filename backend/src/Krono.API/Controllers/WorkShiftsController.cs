using Krono.Application.DTOs;
using Krono.Domain.Entities;
using Krono.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Krono.API.Controllers;

[ApiController]
[Route("api/v1/work-shifts")]
public class WorkShiftsController : ControllerBase
{
    private readonly IWorkShiftRepository _repository;

    public WorkShiftsController(IWorkShiftRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<WorkShiftDto>> GetCurrent()
    {
        var shift = await _repository.GetCurrentShiftAsync();
        return Ok(MapToDto(shift));
    }

    [HttpPut]
    public async Task<ActionResult<WorkShiftDto>> Update([FromBody] UpdateWorkShiftRequest request)
    {
        var shift = await _repository.GetCurrentShiftAsync();
        shift.Name = request.Name;
        shift.StartHour = TimeSpan.Parse(request.StartHour);
        shift.EndHour = TimeSpan.Parse(request.EndHour);
        shift.BreakStart = !string.IsNullOrEmpty(request.BreakStart) ? TimeSpan.Parse(request.BreakStart) : null;
        shift.BreakEnd = !string.IsNullOrEmpty(request.BreakEnd) ? TimeSpan.Parse(request.BreakEnd) : null;
        shift.WorkDays = request.WorkDays;

        await _repository.UpdateShiftAsync(shift);
        return Ok(MapToDto(shift));
    }

    private static WorkShiftDto MapToDto(WorkShift s)
    {
        return new WorkShiftDto
        {
            Id = s.Id,
            Name = s.Name,
            StartHour = s.StartHour.ToString(@"hh\:mm"),
            EndHour = s.EndHour.ToString(@"hh\:mm"),
            BreakStart = s.BreakStart?.ToString(@"hh\:mm"),
            BreakEnd = s.BreakEnd?.ToString(@"hh\:mm"),
            WorkDays = s.WorkDays
        };
    }
}
