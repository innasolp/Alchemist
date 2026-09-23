using System.Linq.Expressions;

namespace Hangfire.AggregateJobs;

public interface IJobExecuteManager
{
    Task Enqueue<T, TJob>(TJob coreJob,
        IEnumerable<TJob> childJobs,
        Expression<Func<T, TJob, Task>> execute,
        AggregateServerSettings aggregateServerSettings,
        JobExecuteOptions? jobExecuteOptions = null,
        Action<TJob, string>? setJobIdAction = null,
        IEnumerable<IChildJobEnricher<TJob>>? childJobEnrichers = null,
        CancellationToken cancellationToken = default)
         where TJob : class?;

    Task EnqueueChild<T, TJob>(TJob job,
        Expression<Func<T, TJob, Task>> execute,
        AggregateServerSettings aggregateServerSettings,
        string parentExecutionId,
        TJob parentJob,
        JobExecuteOptions? jobExecuteOptions = null,
        Action<TJob, string>? setJobIdAction = null,
        IEnumerable<IChildJobEnricher<TJob>>? childJobEnrichers = null,
        CancellationToken cancellationToken = default)
        where TJob : class?;

    Task Execute<T, TJob>(string coreExecutionId,
        IEnumerable<string> childJobIds,
        AggregateServerSettings aggregateServerSettings,
        JobExecuteOptions? jobExecuteOptions,
        CancellationToken cancellationToken = default);

    Task StopWithFailedState(string coreExecutionId, 
        IEnumerable<string> childJobIds, 
        Exception exception,
        AggregateServerSettings? aggregateServerSettings = null, 
        CancellationToken cancellationToken = default);

    Task Delete(string executionId, CancellationToken cancellationToken = default);
}