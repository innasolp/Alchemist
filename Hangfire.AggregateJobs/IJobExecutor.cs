using System.Linq.Expressions;

namespace Hangfire.AggregateJobs;

public interface IJobExecutor
{
    Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> jobTask, string waitingQueue, 
        JobExecuteOptions? jobExecuteOptions = null, 
        CancellationToken cancellationToken = default);

    Task<string> ExecuteAsync<T>(string jobId, string processingQueue, CancellationToken cancellationToken = default);

    string Execute<T>(string jobId, string processingQueue);

    bool IsAccessible(bool isChild = false, JobExecuteOptions? jobExecuteOptions = null, params object?[] parameters);
}