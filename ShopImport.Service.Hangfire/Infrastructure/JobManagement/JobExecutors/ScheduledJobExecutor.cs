using Hangfire;
using Hangfire.States;
using Hangfire.Storage.Monitoring;
using System.Linq.Expressions;

namespace ShopImport.Service.Hangfire.Infrastructure.JobManagement.JobExecutors;

internal class ScheduledJobExecutor(IBackgroundJobClient backgroundJobClient) : IJobExecutor
{
    private readonly static TimeSpan DefaultEnqueuedIn = TimeSpan.FromSeconds(60);

    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    public Task<string> Enqueue<T>(Expression<Func<T, Task>> jobTask, string waitingQueue,
        ServiceExecuteOptions? serviceExecuteOptions = null,
        CancellationToken cancellationToken = default)
    {
        var enqueuedIn = serviceExecuteOptions?.EnqueuedInSeconds != null 
            ? TimeSpan.FromSeconds(serviceExecuteOptions.EnqueuedInSeconds.Value) 
            : DefaultEnqueuedIn;

        var jobId = _backgroundJobClient.Create(
                     waitingQueue,
                     jobTask,
                     new ScheduledState(enqueuedIn));
        
        return Task.FromResult(jobId);
    }

    public Task Execute<T>(string jobId, string processingQueue,  CancellationToken cancellationToken = default)
    {
        var scheduledJob = GetScheduledJobDto(jobId);
       
        _backgroundJobClient.ChangeState(jobId, new ScheduledState(scheduledJob?.EnqueueAt - DateTime.UtcNow ?? DefaultEnqueuedIn));

        return Task.CompletedTask;
    }

    private static ScheduledJobDto? GetScheduledJobDto(string jobId)
    {
        var monitoringApi = global::Hangfire.JobStorage.Current.GetMonitoringApi();
        var scheduledJobs = monitoringApi.ScheduledJobs(0, 1000);
        var job = scheduledJobs.FirstOrDefault(x => x.Key == jobId);

        return !string.IsNullOrEmpty(job.Key) ? job.Value : null;
    }
}