using Hangfire;
using Hangfire.Storage;

namespace LongRunningTask;

public static class StorageExtensions
{
    private readonly static string pausedKey = "paused";

    public static void Pause(this IStorageConnection connection, string jobId)
    {
        var jobPaused = connection.GetJobParameter(jobId, pausedKey);
        if (jobPaused is not null)
            throw new InvalidOperationException($"Job {jobId} already paused."); 

        using var transaction = connection.CreateWriteTransaction();
        transaction.AddToSet(pausedKey, jobId);
        transaction.Commit();
    }

    public static void Resume(this IStorageConnection connection, string jobId)
    {
        var jobPaused = connection.GetJobParameter(jobId, pausedKey) 
            ?? throw new InvalidOperationException($"There is not any paused job with name {jobId}");

        using var transaction = connection.CreateWriteTransaction();
        transaction.RemoveFromSet(pausedKey, jobId);
        transaction.Commit();
    }
}
