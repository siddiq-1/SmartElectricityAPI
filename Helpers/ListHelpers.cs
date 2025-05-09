namespace SmartElectricityAPI.Helpers;

public static class ListHelpers
{
    public static List<T> TakeUntilSum<T>(this IEnumerable<T> source,
    Func<T, double> selector, double maxSum)
    {
        return source
            .Select((item, index) => new
            {
                Item = item,
                RunningSum = source.Take(index + 1).Sum(x => selector(x))
            })
            .TakeWhile(x => x.RunningSum <= maxSum)
            .Select(x => x.Item).ToList();
    }

    public static List<T> TakeUntilSumV2<T>(this IEnumerable<T> source, Func<T, double> selector, double maxSum)
    {
        var result = source
            .Select((item, index) => new
            {
                Item = item,
                RunningSum = source.Take(index + 1).Sum(x => selector(x))
            })
            .ToList(); // Materialize the list to avoid multiple enumerations

        // Find the last index where sum is <= maxSum
        var lastValidIndex = result.FindLastIndex(x => x.RunningSum <= maxSum);

        // If we found a valid index and it's not the last item, take one more
        if (lastValidIndex >= 0 && lastValidIndex < result.Count - 1)
        {
            lastValidIndex++;
        }

        return result
            .Take(lastValidIndex + 1)
            .Select(x => x.Item)
            .ToList();
    }
}
