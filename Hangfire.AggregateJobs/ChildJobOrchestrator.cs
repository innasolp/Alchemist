using Hangfire.AggregateJobs.Filters;
using Hangfire.AggregateJobs.JobExecutors;
using Hangfire.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Hangfire.AggregateJobs;

internal static class ChildJobOrchestrator
{
    public const string Task = "child-orchestrator-tick";    
}

internal class ChildJobOrchestrator<T>(IJobExecutorRegistry jobExecutorRegistry, IServiceScopeFactory serviceScopeFactory)
{
    private readonly IJobExecutorRegistry _jobExecutorRegistry = jobExecutorRegistry;

    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    [DisableConcurrentExecution(timeoutInSeconds: 30)]
    [ShortExpiration(minutes:10)]
    [JobDisplayName(nameof(ChildJobOrchestrator))]
    public async Task Dispatch(int childJobCountPerParent, string childServer, string processingChildQueue)
    {
        var storage = JobStorage.Current;

        var monitoring = storage.GetMonitoringApi();

        var serverWorkerCount = GetServerWorkerCount(monitoring, childServer);
        if (serverWorkerCount == null) return;

        var queue = monitoring.Queues().FirstOrDefault(q => q.Name == processingChildQueue);
        int busy = (int)((queue?.Length ?? 0) + (queue?.Fetched ?? 0));

        int freeSlots = serverWorkerCount.Value - busy;
        if (freeSlots <= 0) return;

        using var scope = _serviceScopeFactory.CreateScope();

        var childJobStorage = scope.ServiceProvider.GetRequiredService<IAggregateJobStorage>();

        var jobIdsToActivate = await childJobStorage.GetChildJobIdsForProcessing(childJobCountPerParent, freeSlots);

        if (!jobIdsToActivate.Any()) return;

        foreach (var jobId in jobIdsToActivate)
        {
            var jobExecutor = _jobExecutorRegistry.Get(isChild: true);
            jobExecutor.Execute<T>(jobId, processingChildQueue);
        }
    }

    private static int? GetServerWorkerCount(IMonitoringApi monitoringApi, string serverName)
    {
        var servers = monitoringApi.Servers();

        var server = servers.FirstOrDefault(s => s.Name.Contains(serverName, StringComparison.InvariantCultureIgnoreCase));

        return server?.WorkersCount;
    }
}