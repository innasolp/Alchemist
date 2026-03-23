using Hangfire;
using Hangfire.Storage;
using ShopImport.Service.Hangfire.Infrastructure.JobManagement.JobExecutors;

namespace ShopImport.Service.Hangfire.Infrastructure.JobManagement;

internal static class ChildJobOrchestrator
{
    public const string Task = "child-orchestrator-tick";    
}

internal class ChildJobOrchestrator<T>(IEnumerable<IJobExecutor> jobExecutors, IBackgroundJobClient backgroundJobClient, IChildJobStorage childJobStorage)
{
    private readonly IEnumerable<IJobExecutor> _jobExecutors = jobExecutors;

    private readonly BackgroundJobExecutor DefaultJobExecutor = new(backgroundJobClient);

    private readonly IChildJobStorage _childJobStorage = childJobStorage;

    [DisableConcurrentExecution(timeoutInSeconds: 10)]
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

        var jobIdsToActivate = await _childJobStorage.GetChildJobIdsForProcessing(childJobCountPerParent, freeSlots);

        if (!jobIdsToActivate.Any()) return;

        foreach (var jobId in jobIdsToActivate)
        {
            var jobExecutor = _jobExecutors.FirstOrDefault(e => e.IsAccessible(isChild: true)) ?? DefaultJobExecutor;
            jobExecutor.Execute<T>(jobId, processingChildQueue);           
        }

        await _childJobStorage.UpdateJobsStateAsync(jobIdsToActivate, JobStatus.Processing);
    }

    private static int? GetServerWorkerCount(IMonitoringApi monitoringApi, string serverName)
    {
        var servers = monitoringApi.Servers();

        var server = servers.FirstOrDefault(s => s.Name.Contains(serverName, StringComparison.InvariantCultureIgnoreCase));

        return server?.WorkersCount;
    }
}