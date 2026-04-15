namespace Hangfire.AggregateJobs;

internal static class JobStorageExtensions
{
    public static void DeleteJob(this JobStorage jobStorage, string jobId)
    {
        using var connection = jobStorage.GetConnection();
        using var transaction = connection.CreateWriteTransaction();
        
        transaction.ExpireJob(jobId, TimeSpan.FromSeconds(1));
        transaction.Commit();
    }
}