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
        CancellationToken cancellationToken = default);

    Task Execute<T, TJob>(string coreJobId,
        IEnumerable<string> childJobIds,
        AggregateServerSettings aggregateServerSettings,
        JobExecuteOptions jobExecuteOptions,
        CancellationToken cancellationToken = default);

    Task StopWithFailedState(string coreJobId, 
        IEnumerable<string> childJobIds, 
        Exception exception,
        AggregateServerSettings? aggregateServerSettings = null, 
        CancellationToken cancellationToken = default);

    void DeleteJob(string jobId);
}