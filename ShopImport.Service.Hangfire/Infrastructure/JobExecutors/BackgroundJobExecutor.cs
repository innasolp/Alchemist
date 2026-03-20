using Hangfire;
using Hangfire.States;

namespace ShopImport.Service.Hangfire.Infrastructure.JobExecutors;

internal class BackgroundJobExecutor(IBackgroundJobClient backgroundJobClient) : IJobExecutor
{
    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    public Task<string> Enqueue<T>(Func<T, Task> jobTask, string waitingQueue, CancellationToken cancellationToken = default)
    {
        var jobId = _backgroundJobClient.Create<T>(
                     obj => jobTask(obj),
                     new EnqueuedState(waitingQueue));
        return Task.FromResult(jobId);
    }

    public Task Execute(string jobId, string processingQueue, ServiceExecuteOptions? serviceExecuteOptions = null, CancellationToken cancellationToken = default)
    {
        _backgroundJobClient.ChangeState(jobId, new EnqueuedState(processingQueue));
        return Task.CompletedTask;
    }
}