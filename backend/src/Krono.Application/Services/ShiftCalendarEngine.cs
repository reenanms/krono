using Krono.Domain.Entities;

namespace Krono.Application.Services;

public class ShiftCalendarEngine
{
    /// <summary>
    /// Calculates the completion DateTime given a start DateTime and duration in minutes,
    /// respecting operating hours, breaks, and working days of the WorkShift.
    /// </summary>
    public DateTime CalculateEndTime(DateTime startTime, decimal durationMinutes, WorkShift shift)
    {
        var current = EnsureWithinWorkingTime(startTime, shift);
        decimal remainingMinutes = durationMinutes;

        while (remainingMinutes > 0)
        {
            var dayOfWeek = current.DayOfWeek;
            if (!shift.IsWorkingDay(dayOfWeek))
            {
                current = NextWorkingDayStart(current, shift);
                continue;
            }

            var timeOfDay = current.TimeOfDay;

            // Before shift starts
            if (timeOfDay < shift.StartHour)
            {
                current = current.Date.Add(shift.StartHour);
                timeOfDay = current.TimeOfDay;
            }
            // After shift ends
            else if (timeOfDay >= shift.EndHour)
            {
                current = NextWorkingDayStart(current, shift);
                continue;
            }

            // Inside lunch break
            if (shift.BreakStart.HasValue && shift.BreakEnd.HasValue &&
                timeOfDay >= shift.BreakStart.Value && timeOfDay < shift.BreakEnd.Value)
            {
                current = current.Date.Add(shift.BreakEnd.Value);
                continue;
            }

            // Determine available working minutes until next break or shift end
            DateTime nextBoundary;
            if (shift.BreakStart.HasValue && timeOfDay < shift.BreakStart.Value)
            {
                nextBoundary = current.Date.Add(shift.BreakStart.Value);
            }
            else
            {
                nextBoundary = current.Date.Add(shift.EndHour);
            }

            decimal availableMinutes = (decimal)(nextBoundary - current).TotalMinutes;

            if (remainingMinutes <= availableMinutes)
            {
                current = current.AddMinutes((double)remainingMinutes);
                remainingMinutes = 0;
            }
            else
            {
                current = nextBoundary;
                remainingMinutes -= availableMinutes;
            }
        }

        return current;
    }

    public DateTime EnsureWithinWorkingTime(DateTime time, WorkShift shift)
    {
        var current = time;
        while (true)
        {
            if (!shift.IsWorkingDay(current.DayOfWeek))
            {
                current = NextWorkingDayStart(current, shift);
                continue;
            }

            var tod = current.TimeOfDay;
            if (tod < shift.StartHour)
            {
                return current.Date.Add(shift.StartHour);
            }
            if (tod >= shift.EndHour)
            {
                current = NextWorkingDayStart(current, shift);
                continue;
            }
            if (shift.BreakStart.HasValue && shift.BreakEnd.HasValue &&
                tod >= shift.BreakStart.Value && tod < shift.BreakEnd.Value)
            {
                return current.Date.Add(shift.BreakEnd.Value);
            }

            return current;
        }
    }

    private static DateTime NextWorkingDayStart(DateTime current, WorkShift shift)
    {
        var nextDay = current.Date.AddDays(1);
        while (!shift.IsWorkingDay(nextDay.DayOfWeek))
        {
            nextDay = nextDay.AddDays(1);
        }
        return nextDay.Add(shift.StartHour);
    }
}
