namespace Krono.Application.DTOs;

public class MachineTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TotalAvailable { get; set; }
}

public class CreateMachineTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TotalAvailable { get; set; } = 1;
}

public class OperatorSkillDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateOperatorSkillRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class WorkShiftDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StartHour { get; set; } = "07:00";
    public string EndHour { get; set; } = "17:00";
    public string? BreakStart { get; set; } = "12:00";
    public string? BreakEnd { get; set; } = "13:00";
    public string WorkDays { get; set; } = "Monday,Tuesday,Wednesday,Thursday,Friday";
}

public class UpdateWorkShiftRequest
{
    public string Name { get; set; } = "Standard Shift";
    public string StartHour { get; set; } = "07:00";
    public string EndHour { get; set; } = "17:00";
    public string? BreakStart { get; set; } = "12:00";
    public string? BreakEnd { get; set; } = "13:00";
    public string WorkDays { get; set; } = "Monday,Tuesday,Wednesday,Thursday,Friday";
}
