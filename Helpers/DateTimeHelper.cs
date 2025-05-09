namespace SmartElectricityAPI.Helpers;

public static class DateTimeHelper
{
    public static bool IsWeekend(DateOnly date)
    {
        DayOfWeek dayOfWeek = date.DayOfWeek;
        return dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;
    }

    public static bool IsDateTimeWithinRequiredMinutesRange(DateTime dateTime, int minutesRange)
    {
        TimeSpan timeDifference = DateTime.Now - dateTime;
        return timeDifference.TotalMinutes < minutesRange && timeDifference.TotalMinutes > 0;
    }
}
