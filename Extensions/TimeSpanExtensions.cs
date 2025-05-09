namespace SmartElectricityAPI.Extensions;

public static class TimeSpanExtensions
{
    public static bool Between(this TimeSpan target, TimeSpan start, TimeSpan end)
    {
        if (start < end)
            return target >= start && target <= end;
        return target >= start || target <= end;
    }
}
