using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Hangfire.AggregateJobs;

internal class ExpiredJobCleanUpManager(IServiceScopeFactory serviceScopeFactory) : IExpiredJobCleanUpManager
{
    public const string Task = "expired-cleanup";

    async Task IExpiredJobCleanUpManager.Dispatch(IJobCancellationToken? jobCancellationToken, PerformContext? performContext)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var childJobStorage = scope.ServiceProvider.GetRequiredService<IAggregateJobStorage>();

        var stopToken = jobCancellationToken?.ShutdownToken ?? default;

        var expiredJobIds = await childJobStorage.GetExpiredJobIdsAsync(stopToken);

        var jobStorage = JobStorage.Current;
        var monitoringApi = jobStorage.GetMonitoringApi();

        foreach (var expiredJobId in expiredJobIds)
        {
            var jobDetails = monitoringApi.JobDetails(expiredJobId);
            if (jobDetails != null) continue;

            var childJobIds = await childJobStorage.GetChildJobIdsAsync(expiredJobId, stopToken);

            foreach (var childJobId in childJobIds)
                await DeleteJobAsync(jobStorage, childJobStorage, childJobId, stopToken);

            await DeleteJobAsync(jobStorage, childJobStorage, expiredJobId, stopToken);
        }
    }

    private static async Task DeleteJobAsync(JobStorage jobStorage, IAggregateJobStorage aggregateJobStorage, string jobId, CancellationToken cancellationToken = default)
    {
         await aggregateJobStorage.DeleteJobEntryAsync(jobId, cancellationToken);

        jobStorage.DeleteJob(jobId);
    }
}