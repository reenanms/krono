namespace Krono.Domain.Entities;

public class WorkShift
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Standard Shift";
    public TimeSpan StartHour { get; set; } = new TimeSpan(7, 0, 0); // 07:00
    public TimeSpan EndHour { get; set; } = new TimeSpan(17, 0, 0);   // 17:00
    public TimeSpan? BreakStart { get; set; } = new TimeSpan(12, 0, 0); // 12:00
    public TimeSpan? BreakEnd { get; set; } = new TimeSpan(13, 0, 0);   // 13:00
    // Comma-separated days of week, e.g. "Monday,Tuesday,Wednesday,Thursday,Friday"
    public string WorkDays { get; set; } = "Monday,Tuesday,Wednesday,Thursday,Friday";

    public bool IsWorkingDay(DayOfWeek day)
    {
        return WorkDays.Contains(day.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
