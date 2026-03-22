using Hangfire;
using Hangfire.States;
using ShopImport.Service.Hangfire.Infrastructure.JobManagement.JobExecutors;
using ShopImport.Service.Hangfire.Infrastructure.JobManagement.JobExecutors.Expression;
using System.Linq.Expressions;

namespace ShopImport.Service.Hangfire.Infrastructure.JobManagement;

internal class JobExecuteManager : IJobExecuteManager
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    private readonly IRecurringJobManager _recurringJobManager;

    private readonly BackgroundJobExecutor _backgroundJobExecutor;

    private readonly RecurringJobExecutor _recurringJobExecutor;

    private readonly ScheduledJobExecutor _scheduledJobExecutor;

    public JobExecuteManager(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _backgroundJobExecutor = new BackgroundJobExecutor(_backgroundJobClient);
        _recurringJobExecutor = new RecurringJobExecutor(_recurringJobManager);
        _scheduledJobExecutor = new ScheduledJobExecutor(_backgroundJobClient);
    }

    private IJobExecutor GetJobExecutor(bool isChild = false, ServiceExecuteOptions? serviceExecuteOptions = null)
    {
        if (isChild)
            return _backgroundJobExecutor;

        if (serviceExecuteOptions?.IntervalInSeconds != null)
            return _recurringJobExecutor;

        if (serviceExecuteOptions?.EnqueuedInSeconds != null)
            return _scheduledJobExecutor;

        return _backgroundJobExecutor;
    }

    private static string GetChildProcessingQueue(JobExecuteOptions jobExecuteOptions)
    {
        return !string.IsNullOrEmpty(jobExecuteOptions.ChildProcessingQueue) 
            ? jobExecuteOptions.ChildProcessingQueue 
            : jobExecuteOptions.ProcessingQueue;
    }

    public async Task Enqueue<T>(IImportServiceJob importServiceJob,
        Expression<Func<T, IImportServiceJob, Task>> execute,
        JobExecuteOptions jobExecuteOptions,
        CancellationToken cancellationToken = default)
    {
        var coreExecutor = GetJobExecutor(importServiceJob.ParentId != null, importServiceJob.ServiceExecuteOptions);
        var coreJobExpression = execute.BindSecondParameter(importServiceJob);
        importServiceJob.JobId = await coreExecutor.Enqueue(coreJobExpression, 
                                        jobExecuteOptions.WaitingQueue,
                                        importServiceJob.ServiceExecuteOptions, 
                                        cancellationToken);

        var childJobs = await importServiceJob.GetСhildJobs();

        foreach (var executedJob in childJobs)
        {
            var executor = GetJobExecutor(true);
            var jobExpression = execute.BindSecondParameter(executedJob.Value);
            executedJob.Value.JobId = await executor.Enqueue(jobExpression,
                jobExecuteOptions.WaitingQueue,
                executedJob.Value.ServiceExecuteOptions,
                cancellationToken);
        }
    }

    public async Task Execute<T>(IImportServiceJob importServiceJob, 
        JobExecuteOptions jobExecuteOptions,
        CancellationToken cancellationToken = default)
    {
        var coreExecutor = GetJobExecutor(importServiceJob.ParentId != null, importServiceJob.ServiceExecuteOptions);
        await coreExecutor.Execute<T>(importServiceJob.JobId, jobExecuteOptions.ProcessingQueue, cancellationToken);

        var childJobs = await importServiceJob.GetСhildJobs();

        var childJobProcessingQueue = GetChildProcessingQueue(jobExecuteOptions);

        foreach (var executedJob in childJobs)
        {
            var executor = GetJobExecutor(true);
            await executor.Execute<T>(executedJob.Value.JobId, childJobProcessingQueue, cancellationToken : cancellationToken);
        }
    }

    public async Task StopWithFailedState(IImportServiceJob importServiceJob, Exception exception, JobExecuteOptions? jobExecuteOptions = null, CancellationToken cancellationToken = default)
    {
        var executingChildJobs = (await importServiceJob.GetExecutionServiceJobs())
            .Where(j=>!string.IsNullOrEmpty(j.Value.JobId) && j.Value.JobId != importServiceJob.JobId).ToList();

        var serverName = !string.IsNullOrEmpty(jobExecuteOptions?.ChildServerName)
            ? jobExecuteOptions.ChildServerName
            : !string.IsNullOrEmpty(jobExecuteOptions?.ServerName) ? jobExecuteOptions.ServerName : null;

        foreach (var executedJob in executingChildJobs)
        {
            StopWithFailedState(executedJob.Value.JobId!, exception, serverName);

            if (executedJob.Value.ParentId == importServiceJob.Id)
                _backgroundJobClient.Delete(executedJob.Value.JobId);
        }
        
        //todo
        StopWithFailedState(importServiceJob.JobId, exception, jobExecuteOptions?.ServerName);
    }

    private void StopWithFailedState(string jobId, Exception exception, string? serverName)
    {
        var failedState = !string.IsNullOrEmpty(serverName) ? new FailedState(exception, serverName) : new FailedState(exception);
        failedState.Reason = exception.Message;

        _backgroundJobClient.ChangeState(jobId, failedState);
    }
}