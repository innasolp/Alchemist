using Hangfire.AggregateJobs.JobExecutors;
using Hangfire.AggregateJobs.JobExecutors.Expression;
using Hangfire.States;
using System.Linq.Expressions;

namespace Hangfire.AggregateJobs;

internal class JobExecuteManager(IEnumerable<IJobExecutor> jobExecutors, 
    IBackgroundJobClient backgroundJobClient, 
    IChildJobStorage childJobStorage) : IJobExecuteManager
{
    private readonly IEnumerable<IJobExecutor> _jobExecutors = jobExecutors;

    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    private readonly IChildJobStorage _childJobStorage = childJobStorage;

    private readonly BackgroundJobExecutor DefaultJobExecutor = new(backgroundJobClient);

    public void DeleteJob(string jobId)
    {
        _childJobStorage.UpdateJobState(jobId, JobStatus.Deleted);
        _backgroundJobClient.Delete(jobId);        
    }

    public async Task Enqueue<T, TJob>(TJob coreJob,
        IEnumerable<TJob> childJobs,
        Expression<Func<T, TJob, Task>> execute,
        AggregateServerSettings aggregateServerSettings,
        JobExecuteOptions? jobExecuteOptions = null,
        Action<TJob, string>? setJobIdAction = null,
        IEnumerable<IChildJobEnricher<TJob>>? childJobEnrichers = null,
        CancellationToken cancellationToken = default)
    {
        var coreExecutor = _jobExecutors.FirstOrDefault(e=>e.IsAccessible(jobExecuteOptions: jobExecuteOptions)) ?? DefaultJobExecutor;
       
        var coreJobExpression = execute.BindSecondParameter(coreJob);
        var coreJobId = await coreExecutor.EnqueueAsync(coreJobExpression, 
                                        aggregateServerSettings.WaitingQueue,
                                        jobExecuteOptions, 
                                        cancellationToken);
        setJobIdAction?.Invoke(coreJob, coreJobId);

        if(childJobEnrichers != null)
            foreach (var childJobEnricher in childJobEnrichers)
                childJobEnricher.Enrich(coreJobId, coreJob, coreJob);

        foreach (var childJob in childJobs)
        {
            var executor = _jobExecutors.FirstOrDefault(e => e.IsAccessible(true)) ?? DefaultJobExecutor;

            var jobExpression = execute.BindSecondParameter(childJob);
            
            var jobId = await executor.EnqueueAsync(jobExpression,
                aggregateServerSettings.ChildWaitingQueue,
                cancellationToken : cancellationToken);

            if (childJobEnrichers != null)
                foreach (var childJobEnricher in childJobEnrichers)
                    childJobEnricher.Enrich(jobId, childJob, coreJob);

            setJobIdAction?.Invoke(childJob, jobId);
        }
    }

    public async Task Execute<T, TJob>(string coreJobId,
        IEnumerable<string> childJobIds,
        AggregateServerSettings aggregateServerSettings,
        JobExecuteOptions jobExecuteOptions,
        CancellationToken cancellationToken = default)
    {
        var coreExecutor = _jobExecutors.FirstOrDefault(e => e.IsAccessible(jobExecuteOptions : jobExecuteOptions)) ?? DefaultJobExecutor;
        var parentJobId = await coreExecutor.ExecuteAsync<T>(coreJobId, aggregateServerSettings.ProcessingQueue, cancellationToken);
        var parentJobCreatedAt = DateTime.Now;       

        foreach (var executedJobId in childJobIds)
        {
            await _childJobStorage.CreateChildJobEntryAsync(
                new ChildJobEntry { JobId = executedJobId, ParentJobId = parentJobId, Status = 0, ParentCreatedAt = parentJobCreatedAt });
        }
    }

    public async Task StopWithFailedState(string coreJobId, IEnumerable<string> childJobIds, 
        Exception exception, 
        AggregateServerSettings? aggregateServerSettings = null, 
        CancellationToken cancellationToken = default)
    {
        var serverName = !string.IsNullOrEmpty(aggregateServerSettings?.ChildServerName)
            ? aggregateServerSettings.ChildServerName
            : !string.IsNullOrEmpty(aggregateServerSettings?.ServerName) ? aggregateServerSettings.ServerName : null;

        foreach (var jobId in childJobIds)
        {
            StopWithFailedState(jobId, exception, serverName);

            _backgroundJobClient.Delete(jobId);
        }
        
        StopWithFailedState(coreJobId, exception, aggregateServerSettings?.ServerName);
    }

    private void StopWithFailedState(string jobId, Exception exception, string? serverName)
    {
        var failedState = !string.IsNullOrEmpty(serverName) ? new FailedState(exception, serverName) : new FailedState(exception);
        failedState.Reason = exception.Message;

        _backgroundJobClient.ChangeState(jobId, failedState);
    }
}