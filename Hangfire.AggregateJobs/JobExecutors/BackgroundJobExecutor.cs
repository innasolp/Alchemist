using Hangfire.States;
using System.Linq.Expressions;

namespace Hangfire.AggregateJobs.JobExecutors;

internal class BackgroundJobExecutor(IBackgroundJobClient backgroundJobClient) : IJobExecutor
{
    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    public Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> jobTask, string waitingQueue, JobExecuteOptions? jobExecuteOptions = null, CancellationToken cancellationToken = default)
    {
        var jobId = _backgroundJobClient.Create(
                     jobTask,
                     new EnqueuedState(waitingQueue));
        return Task.FromResult(jobId);
    }

    public string Execute<T>(string jobId, string processingQueue)
    {
        _backgroundJobClient.ChangeState(jobId, new EnqueuedState(processingQueue));
        return jobId;
    }

    public Task<string> ExecuteAsync<T>(string jobId, string processingQueue, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Execute<T>(jobId, processingQueue));
    }

    public bool IsAccessible(bool isChild = false, JobExecuteOptions? jobExecuteOptions = null)
    {
        return isChild || (jobExecuteOptions?.IntervalInSeconds == null && jobExecuteOptions?.EnqueuedInSeconds == null);
    }
}