using System.Text;
using Krono.Application.DTOs;
using Krono.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Krono.API.Controllers;

[ApiController]
[Route("api/v1/reports")]
public class ReportsController : ControllerBase
{
    private readonly ProductionSchedulerService _schedulerService;

    public ReportsController(ProductionSchedulerService schedulerService)
    {
        _schedulerService = schedulerService;
    }

    [HttpGet("production-schedule")]
    public async Task<ActionResult<ProductionScheduleReportDto>> GetProductionSchedule(
        [FromQuery] string viewType = "week",
        [FromQuery] DateTime? date = null,
        [FromQuery] DateTime? startDate = null)
    {
        var targetDate = date ?? startDate ?? DateTime.Today;
        var report = await _schedulerService.GetScheduleReportAsync(viewType, targetDate);
        return Ok(report);
    }

    [HttpGet("production-schedule/export")]
    public async Task<IActionResult> ExportProductionSchedule(
        [FromQuery] string format = "csv",
        [FromQuery] string viewType = "week",
        [FromQuery] DateTime? date = null,
        [FromQuery] DateTime? startDate = null)
    {
        var targetDate = date ?? startDate ?? DateTime.Today;
        var report = await _schedulerService.GetScheduleReportAsync(viewType, targetDate);

        var csvBuilder = new StringBuilder();
        csvBuilder.AppendLine("KRONO PRODUCTION SCHEDULE REPORT");
        csvBuilder.AppendLine($"Horizon: {report.StartDate:yyyy-MM-dd} to {report.EndDate:yyyy-MM-dd}");
        csvBuilder.AppendLine();

        // Labor Summary Table
        csvBuilder.AppendLine("LABOR SIZING SUMMARY TABLE");
        csvBuilder.AppendLine("Specialty,Peak Workers,Total Man-Hours");
        foreach (var labor in report.LaborSummary)
        {
            csvBuilder.AppendLine($"\"{labor.SkillName}\",{labor.PeakWorkers},{labor.TotalManHours}");
        }
        csvBuilder.AppendLine();

        // Detailed Schedule
        csvBuilder.AppendLine("DETAILED PRODUCTION SCHEDULE");
        csvBuilder.AppendLine("Order Code,Product,Stage,Machine,Operators,Start Time,End Time,Duration (min),Conflict");
        foreach (var ev in report.Events.OrderBy(e => e.StartTime))
        {
            var conflict = ev.HasMachineConflict ? "YES (OVERLOAD)" : "NO";
            csvBuilder.AppendLine($"\"{ev.OrderCode}\",\"{ev.ProductName}\",\"{ev.StageName}\",\"{ev.MachineTypeName ?? "N/A"}\",{ev.AssignedOperators},\"{ev.StartTime:yyyy-MM-dd HH:mm}\",\"{ev.EndTime:yyyy-MM-dd HH:mm}\",{ev.DurationMinutes},\"{conflict}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
        var fileName = $"Krono_Schedule_{targetDate:yyyyMMdd}_{viewType}.csv";
        return File(bytes, "text/csv", fileName);
    }
}
