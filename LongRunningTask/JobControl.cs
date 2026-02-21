using Hangfire;
using Hangfire.Server;

namespace LongRunningTask;

public static class JobControl
{
    public static readonly string PausedHashKey = "Paused";

    public static void PauseJob(string jobName)
    {
        using var connection = JobStorage.Current.GetConnection();
        connection.SetRangeInHash($"paused-job:{jobName}",
        [
            new KeyValuePair<string, string>(PausedHashKey, "true")
        ]);
    }

    public static void PauseJob(PerformingContext context)
    {
        context.Connection.SetRangeInHash($"paused-job:{context.BackgroundJob.Id}",
        [
            new KeyValuePair<string, string>(PausedHashKey, "true")
        ]);
    }

    public static void ResumeJob(string jobName)
    {
        using var connection = JobStorage.Current.GetConnection();
        connection.SetRangeInHash($"paused-job:{jobName}",
        [
            new KeyValuePair<string, string>(PausedHashKey, "false")
        ]);
    }

    public static void ResumeJob(PerformingContext context)
    {
        context.Connection.SetRangeInHash($"paused-job:{context.BackgroundJob.Id}",
        [
            new KeyValuePair<string, string>(PausedHashKey, "false")
        ]);
    }

    public static bool IsPaused(PerformingContext context)
    {
        var jobPausedKey = $"paused-job:{context.BackgroundJob.Id}";

        var hashSet = context.Connection.GetAllEntriesFromHash(jobPausedKey);
        return hashSet.TryGetValue(PausedHashKey, out var paused) && paused == "true";
    }
}