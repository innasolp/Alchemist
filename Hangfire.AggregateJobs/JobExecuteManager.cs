using Hangfire.AggregateJobs.ChildJobStorages;
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

    public async Task Delete(string executionId)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var childJobStorage = scope.ServiceProvider.GetRequiredService<IAggregateJobStorage>();

        var jobEntry = await childJobStorage.GetJobByExecutionIdAsync(executionId);

        if (jobEntry == null) return;

        _backgroundJobClient.Delete(jobEntry.JobId);

        await childJobStorage.UpdateJobStateAsync(jobEntry.JobId, JobStatus.Deleted, DateTime.Now);
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
            childJobEnrichers, 
            cancellationToken);

        var parentExecutionId = GetExecutionId(execute, coreJob);

        setJobIdAction?.Invoke(coreJob, parentJobId);

        using var scope = serviceScopeFactory.CreateScope();
        var childJobStorage = scope.ServiceProvider.GetRequiredService<IAggregateJobStorage>();

        await childJobStorage.CreateJobEntryAsync(new JobEntry { JobId = parentJobId,
            CreatedAt = DateTime.Now,
            Status = JobStatus.Enqueued,
            ExecutionId = parentExecutionId });

        foreach (var childJob in childJobs)
        {
            var childJobId = await EnqueueJob(childJob,
            execute,
            aggregateServerSettings.ChildWaitingQueue,
            coreJob,
            jobExecuteOptions,
            childJobEnrichers,
            cancellationToken);

            var childExecutionId = GetExecutionId(execute, childJob);

            setJobIdAction?.Invoke(childJob, childJobId);

            await childJobStorage.CreateJobEntryAsync(
                new JobEntry { JobId = childJobId, ParentJobId = parentJobId, Status = 0, ExecutionId = childExecutionId });
        }
    }

    public async Task EnqueueChild<T, TJob>(TJob job,
        Expression<Func<T, TJob, Task>> execute,
        AggregateServerSettings aggregateServerSettings,
         string parentExecutionId,
        TJob parentJob,
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
            childJobEnrichers,
            cancellationToken);

        var executionId = GetExecutionId(execute, job);
        
        setJobIdAction?.Invoke(job, jobId);

        using var scope = serviceScopeFactory.CreateScope();
        var childJobStorage = scope.ServiceProvider.GetRequiredService<IAggregateJobStorage>();

        var parentJobEntry = await childJobStorage.GetJobByExecutionIdAsync(parentExecutionId);        

        await childJobStorage.CreateJobEntryAsync(new JobEntry { JobId = jobId,
            ParentJobId = parentJobEntry?.JobId, 
            Status = JobStatus.Enqueued, 
            ExecutionId = executionId });
    }


    private async Task<string> EnqueueJob<T, TJob>(TJob job,
        Expression<Func<T, TJob, Task>> execute,
        string queue,
        TJob? parentJob = null,
        JobExecuteOptions? jobExecuteOptions = null,
        IEnumerable<IChildJobEnricher<TJob>>? childJobEnrichers = null,
        CancellationToken cancellationToken = default)
        where TJob : class?
    {
        var executor = _jobExecutorRegistry.Get(parentJob != null && job != parentJob, jobExecuteOptions);

        var jobExpression = execute.BindSecondParameter(job);
        var jobId = await executor.EnqueueAsync(jobExpression,
            queue,
            cancellationToken: cancellationToken);

        if (childJobEnrichers != null)
            foreach (var childJobEnricher in childJobEnrichers)
                childJobEnricher.Enrich(jobId, job, parentJob);

        return jobId;
    }

    private static string? GetExecutionId<T, TJob>(Expression<Func<T, TJob, Task>> execute, TJob job)
          where TJob : class?
    {
        var jobExpression = execute.BindSecondParameter(job);
        return jobExpression.GetArguments()?.FirstOrDefault()?.ToString(); 
    }

    public async Task Execute<T, TJob>(string coreExecutionId,
        IEnumerable<string> childExecutionIds,
        AggregateServerSettings aggregateServerSettings,
        JobExecuteOptions? jobExecuteOptions,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var childJobStorage = scope.ServiceProvider.GetRequiredService<IAggregateJobStorage>();

        var coreJobEntry = await childJobStorage.GetJobByExecutionIdAsync(coreExecutionId) 
            ?? throw new InvalidOperationException($"Job entry with id {coreExecutionId} not found.");
        
        var coreExecutor = _jobExecutorRegistry.Get(jobExecuteOptions :
            coreJobEntry.Status != JobStatus.Processing ? jobExecuteOptions : null);

        var newCoreJobId = await coreExecutor.ExecuteAsync<T>(coreJobEntry.JobId, aggregateServerSettings.ProcessingQueue, cancellationToken);
        
        if (newCoreJobId == coreJobEntry.JobId)
        {
            var jobStatus = GetCurrentJobStatus(newCoreJobId);
            if(jobStatus != JobStatus.Enqueued)
                await childJobStorage.UpdateJobStateAsync(newCoreJobId, jobStatus, DateTime.Now);
        }
        else
        {
            var childJobIds = await childJobStorage.GetJobIdsByExecutionIdsAsync(childExecutionIds);
            await SetNewParentJob(coreJobEntry, childJobIds, newCoreJobId, childJobStorage);
        }

        if (jobExecuteOptions?.IdleTimeInSeconds > 0)
            await childJobStorage.CreateParentJobIdleSettingsAsync(new ParentJobIdleSettings
            { JobId = newCoreJobId, IdleTimeInSeconds = jobExecuteOptions.IdleTimeInSeconds.Value });
    }

    private static async Task SetNewParentJob(JobEntry currentParentJobEntry, IEnumerable<string> childJobIds, string newParentJobId, IAggregateJobStorage childJobStorage)
    {
        var jobStatus = GetCurrentJobStatus(newParentJobId);

        //todo wrap in transaction
        await childJobStorage.CreateJobEntryAsync(new JobEntry
        {
            JobId = newParentJobId,
            Status = jobStatus,
            CreatedAt = currentParentJobEntry.CreatedAt,
            ExecutionId = currentParentJobEntry.ExecutionId
        });

        await childJobStorage.UpdateParentJobIdAsync(childJobIds, newParentJobId);

        await childJobStorage.DeleteJobAsync(currentParentJobEntry.JobId);
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

    public async Task StopWithFailedState(string coreExecutionId, IEnumerable<string> childExecutionIds, 
        Exception exception, 
        AggregateServerSettings? aggregateServerSettings = null, 
        CancellationToken cancellationToken = default)
    {
        var serverName = !string.IsNullOrEmpty(aggregateServerSettings?.ChildServerName)
            ? aggregateServerSettings.ChildServerName
            : !string.IsNullOrEmpty(aggregateServerSettings?.ServerName) ? aggregateServerSettings.ServerName : null;

        using var scope = serviceScopeFactory.CreateScope();
        var childJobStorage = scope.ServiceProvider.GetRequiredService<IAggregateJobStorage>();

        var coreJobEntry = await childJobStorage.GetJobByExecutionIdAsync(coreExecutionId) 
            ?? throw new InvalidOperationException($"Job with execution id {coreExecutionId} not found.");

        var childJobIds = await childJobStorage.GetJobIdsByExecutionIdsAsync(childExecutionIds);

        foreach (var jobId in childJobIds)
        {
            StopJobWithFailedState(jobId, exception, serverName);

            _backgroundJobClient.Delete(jobId);
        }

        StopJobWithFailedState(coreJobEntry.JobId, exception, aggregateServerSettings?.ServerName);
    }

    private void StopJobWithFailedState(string jobId, Exception exception, string? serverName)
    {
        var failedState = !string.IsNullOrEmpty(serverName) ? new FailedState(exception, serverName) : new FailedState(exception);
        failedState.Reason = exception.Message;

        _backgroundJobClient.ChangeState(jobId, failedState);
    }
}