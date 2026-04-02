using Hangfire.AggregateJobs.JobExecutors;
using Hangfire.AggregateJobs.JobExecutors.Expression;
using Hangfire.States;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace Hangfire.AggregateJobs;

internal class JobExecuteManager(IJobExecutorRegistry jobExecutorRegistry, 
    IBackgroundJobClient backgroundJobClient, 
    IServiceScopeFactory serviceScopeFactory) : IJobExecuteManager
{
    private readonly IJobExecutorRegistry _jobExecutorRegistry = jobExecutorRegistry;

    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    public void DeleteChildJob(string jobId)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var childJobStorage = scope.ServiceProvider.GetRequiredService<IChildJobStorage>();
        childJobStorage.UpdateChildJobState(jobId, JobStatus.Deleted);
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
        var parentJobId = await EnqueueJob(coreJob, 
            execute,
            aggregateServerSettings.WaitingQueue, 
            coreJob,
            jobExecuteOptions, 
            setJobIdAction,
            childJobEnrichers, 
            cancellationToken);

        using var scope = serviceScopeFactory.CreateScope();
        var childJobStorage = scope.ServiceProvider.GetRequiredService<IChildJobStorage>();

        await childJobStorage.CreateParentJobEntryAsync(new ParentJobEntry { JobId = parentJobId, CreatedAt = DateTime.Now, Status = JobStatus.Enqueued });

        foreach (var childJob in childJobs)
        {
            var childJobId = await EnqueueJob(childJob,
            execute,
            aggregateServerSettings.ChildWaitingQueue,
            coreJob,
            jobExecuteOptions,
            setJobIdAction,
            childJobEnrichers,
            cancellationToken);

            await childJobStorage.CreateChildJobEntryAsync(
                new ChildJobEntry { JobId = childJobId, ParentJobId = parentJobId, Status = 0 });
        }
    }

    public async Task EnqueueChild<T, TJob>(TJob job,
        Expression<Func<T, TJob, Task>> execute,
        AggregateServerSettings aggregateServerSettings,
        TJob? parentJob = null,
        string? parentJobId = null,
        JobExecuteOptions? jobExecuteOptions = null,
        Action<TJob, string>? setJobIdAction = null,
        IEnumerable<IChildJobEnricher<TJob>>? childJobEnrichers = null,
        CancellationToken cancellationToken = default)
        where TJob : class?
    {
        var jobId = await EnqueueJob(job,
            execute,
            aggregateServerSettings.ChildWaitingQueue,
            parentJob,
            jobExecuteOptions,
            setJobIdAction,
            childJobEnrichers,
            cancellationToken);

        if (string.IsNullOrEmpty(parentJobId)) return;

        using var scope = serviceScopeFactory.CreateScope();
        var childJobStorage = scope.ServiceProvider.GetRequiredService<IChildJobStorage>();

        await childJobStorage.CreateChildJobEntryAsync(new ChildJobEntry { JobId = jobId, ParentJobId = parentJobId, Status = JobStatus.Enqueued });
    }


    private async Task<string> EnqueueJob<T, TJob>(TJob job,
        Expression<Func<T, TJob, Task>> execute,
        string queue,
        TJob? parentJob = null,
        JobExecuteOptions? jobExecuteOptions = null,
        Action<TJob, string>? setJobIdAction = null,
        IEnumerable<IChildJobEnricher<TJob>>? childJobEnrichers = null,
        CancellationToken cancellationToken = default)
        where TJob : class?
    {
        var executor = _jobExecutorRegistry.Get(parentJob != null && job != parentJob, jobExecuteOptions);

        var jobExpression = execute.BindSecondParameter(job);

        var jobId = await executor.EnqueueAsync(jobExpression,
            queue,
            cancellationToken: cancellationToken);

        setJobIdAction?.Invoke(job, jobId);

        if (childJobEnrichers != null)
            foreach (var childJobEnricher in childJobEnrichers)
                childJobEnricher.Enrich(jobId, job, parentJob);

        return jobId;
    }

    public async Task Execute<T, TJob>(string coreJobId,
        IEnumerable<string> childJobIds,
        AggregateServerSettings aggregateServerSettings,
        JobExecuteOptions? jobExecuteOptions,
        CancellationToken cancellationToken = default)
    {
        var coreExecutor = _jobExecutorRegistry.Get(jobExecuteOptions : jobExecuteOptions);
        var newParentJobId = await coreExecutor.ExecuteAsync<T>(coreJobId, aggregateServerSettings.ProcessingQueue, cancellationToken);
        
        using var scope = serviceScopeFactory.CreateScope();
        var childJobStorage = scope.ServiceProvider.GetRequiredService<IChildJobStorage>();

        if (newParentJobId == coreJobId)
        {
            var jobStatus = GetCurrentJobStatus(newParentJobId);
            if(jobStatus != JobStatus.Enqueued)
                await childJobStorage.UpdateParentJobStateAsync(newParentJobId, jobStatus, DateTime.Now);
        }
        else
        {
            await SetNewParentJob(coreJobId, childJobIds, newParentJobId, childJobStorage);
        }

        if (jobExecuteOptions?.IdleTimeInSeconds > 0)
            await childJobStorage.CreateParentJobIdleSettingsAsync(new ChildJobStorages.ParentJobIdleSettings
            { JobId = newParentJobId, IdleTimeInSeconds = jobExecuteOptions.IdleTimeInSeconds.Value });


    }

    private static async Task SetNewParentJob(string coreJobId, IEnumerable<string> childJobIds, string newParentJobId, IChildJobStorage childJobStorage)
    {
        var jobStatus = GetCurrentJobStatus(newParentJobId);

        var parentJob = await childJobStorage.GetParentJobAsync(coreJobId);

        await childJobStorage.CreateParentJobEntryAsync(new ParentJobEntry
        {
            JobId = newParentJobId,
            Status = jobStatus,
            CreatedAt = parentJob?.CreatedAt ?? DateTime.Now,
        });

        await childJobStorage.UpdateParentJobIdAsync(childJobIds, newParentJobId);

        await childJobStorage.DeleteParentJobAsync(coreJobId);
    }

    private static JobStatus GetCurrentJobStatus(string jobId)
    {
        using var connection = JobStorage.Current.GetConnection();
        var jobData = connection.GetJobData(jobId);

        return jobData.State == EnqueuedState.StateName ? JobStatus.Enqueued :
                     jobData.State == ProcessingState.StateName ? JobStatus.Processing:
                     jobData.State == SucceededState.StateName ? JobStatus.Completed
                                                  : JobStatus.Deleted;
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

    public void DeleteParentJob(string jobId)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var childJobStorage = scope.ServiceProvider.GetRequiredService<IChildJobStorage>();
        childJobStorage.UpdateParentJobState(jobId, JobStatus.Deleted, DateTime.Now);
        _backgroundJobClient.Delete(jobId);
    }
}