using Hangfire.AggregateJobs.JobExecutors;
using Hangfire.AggregateJobs.JobExecutors.Expression;
using Hangfire.States;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace Hangfire.AggregateJobs;

internal class JobExecuteManager(IEnumerable<IJobExecutor> jobExecutors, 
    IBackgroundJobClient backgroundJobClient, 
    IServiceScopeFactory serviceScopeFactory) : IJobExecuteManager
{
    private readonly IEnumerable<IJobExecutor> _jobExecutors = jobExecutors;

    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    private readonly BackgroundJobExecutor DefaultJobExecutor = new(backgroundJobClient);

    public void DeleteJob(string jobId)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var childJobStorage = scope.ServiceProvider.GetRequiredService<IChildJobStorage>();
        childJobStorage.UpdateJobState(jobId, JobStatus.Deleted);
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
        where TJob : class?
    {
        await EnqueueJob(coreJob, 
            execute,
            aggregateServerSettings.WaitingQueue, 
            coreJob,
            jobExecuteOptions, 
            setJobIdAction,
            childJobEnrichers, 
            cancellationToken);

        foreach (var childJob in childJobs)
        {
            await EnqueueJob(childJob,
            execute,
            aggregateServerSettings.ChildWaitingQueue,
            coreJob,
            jobExecuteOptions,
            setJobIdAction,
            childJobEnrichers,
            cancellationToken);
        }
    }

    private async Task EnqueueJob<T, TJob>(TJob job,
        Expression<Func<T, TJob, Task>> execute,
        string queue,
        TJob? parentJob = null,
        JobExecuteOptions? jobExecuteOptions = null,
        Action<TJob, string>? setJobIdAction = null,
        IEnumerable<IChildJobEnricher<TJob>>? childJobEnrichers = null,
        CancellationToken cancellationToken = default)
        where TJob : class?
    {
        var executor = _jobExecutors.FirstOrDefault(e => e.IsAccessible(parentJob != null && job != parentJob, jobExecuteOptions))
            ?? DefaultJobExecutor;

        var jobExpression = execute.BindSecondParameter(job);

        var jobId = await executor.EnqueueAsync(jobExpression,
            queue,
            cancellationToken: cancellationToken);

        if (childJobEnrichers != null)
            foreach (var childJobEnricher in childJobEnrichers)
                childJobEnricher.Enrich(jobId, job, parentJob);

        setJobIdAction?.Invoke(job, jobId);
    }

    public async Task Execute<T, TJob>(string coreJobId,
        IEnumerable<string> childJobIds,
        AggregateServerSettings aggregateServerSettings,
        JobExecuteOptions? jobExecuteOptions,
        CancellationToken cancellationToken = default)
    {
        var coreExecutor = _jobExecutors.FirstOrDefault(e => e.IsAccessible(jobExecuteOptions : jobExecuteOptions)) ?? DefaultJobExecutor;
        var parentJobId = await coreExecutor.ExecuteAsync<T>(coreJobId, aggregateServerSettings.ProcessingQueue, cancellationToken);
        var parentJobCreatedAt = DateTime.Now;       

        using var scope = serviceScopeFactory.CreateScope();
        var childJobStorage = scope.ServiceProvider.GetRequiredService<IChildJobStorage>();

        await childJobStorage.CreateParentJobEntryAsync(new ParentJobEntry { JobId = parentJobId, CreatedAt = parentJobCreatedAt, Status = JobStatus.Processing });

        foreach (var executedJobId in childJobIds)
        {
            await childJobStorage.CreateChildJobEntryAsync(
                new ChildJobEntry { JobId = executedJobId, ParentJobId = parentJobId, Status = 0 });
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