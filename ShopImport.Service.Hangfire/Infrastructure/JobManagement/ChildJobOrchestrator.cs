using Hangfire;
using Hangfire.States;
using Hangfire.Storage;

namespace ShopImport.Service.Hangfire.Infrastructure.JobManagement;

internal class ChildJobOrchestrator(IBackgroundJobClient jobClient, IJobStorage childJobStorage, JobStorage storage)
{
    private readonly IBackgroundJobClient _jobClient = jobClient;

    private readonly IJobStorage _childJobStorage = childJobStorage;

    private readonly JobStorage _storage = storage;

    public async Task Dispatch(int childJobCountPerParent,string childServer, string processingChildQueue)
    {
        ar monitoring = _storage.GetMonitoringApi();

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
            _jobClient.ChangeState(jobId, new EnqueuedState(processingChildQueue));
        }

        await _childJobStorage.UpdateJobsStateAsync(jobIdsToActivate, 1);
    }

    private static int? GetServerWorkerCount(IMonitoringApi monitoringApi, string serverName)
    {
        var servers = monitoringApi.Servers();

        var server = servers.FirstOrDefault(s => s.Name == serverName);

        return server?.WorkersCount;
    }
}