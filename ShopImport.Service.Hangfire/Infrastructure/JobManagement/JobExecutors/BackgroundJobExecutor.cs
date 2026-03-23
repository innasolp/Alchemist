using Hangfire;
using Hangfire.States;
using System.Linq.Expressions;

namespace ShopImport.Service.Hangfire.Infrastructure.JobManagement.JobExecutors;

internal class BackgroundJobExecutor(IBackgroundJobClient backgroundJobClient) : IJobExecutor
{
    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    public Task<string> Enqueue<T>(Expression<Func<T, Task>> jobTask, string waitingQueue, ServiceExecuteOptions? serviceExecuteOptions = null, CancellationToken cancellationToken = default)
    {
        var jobId = _backgroundJobClient.Create(
                     jobTask,
                     new EnqueuedState(waitingQueue));
        return Task.FromResult(jobId);
    }

    public Task<string> Execute<T>(string jobId, string processingQueue, CancellationToken cancellationToken = default)
    {
        _backgroundJobClient.ChangeState(jobId, new EnqueuedState(processingQueue));
        return Task.FromResult(jobId);
    }
}