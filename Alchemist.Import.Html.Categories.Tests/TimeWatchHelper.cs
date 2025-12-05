using System.Diagnostics;


namespace Alchemist.Import.Html.Categories.Tests;

internal static class TimeWatchHelper
{
    public static async Task<(T, TimeSpan)> ExecuteTaskWithTimeWatchAsync<T>(Func<Task<T>> task)
    {
        var stopWatch = new Stopwatch();

        stopWatch.Start();

        var result = await Task.Run(task);       

        stopWatch.Stop();

        return (result, stopWatch.Elapsed);
    }

    public static async Task<TimeSpan> ExecuteTaskWithTimeWatchAsync(Func<Task> task)
    {
        var stopWatch = new Stopwatch();

        stopWatch.Start();

        await Task.Run(task);

        stopWatch.Stop();

        return stopWatch.Elapsed;
    }
}