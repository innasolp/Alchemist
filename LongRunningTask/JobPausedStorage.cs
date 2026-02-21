using Hangfire;

namespace LongRunningTask;

public static class JobPausedStorage
{
    public static void MarkAsResume(string jobId)
    {
        using var connection = JobStorage.Current.GetConnection();
        connection.Resume(jobId);
    }

    public static void MarkAsPause(string jobId)
    {
        using var connection = JobStorage.Current.GetConnection();
        connection.Pause(jobId);
    }
}