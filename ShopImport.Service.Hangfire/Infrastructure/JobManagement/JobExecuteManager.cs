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

    private readonly IChildJobStorage _childJobStorage;

    private readonly BackgroundJobExecutor _backgroundJobExecutor;

    private readonly RecurringJobExecutor _recurringJobExecutor;

    private readonly ScheduledJobExecutor _scheduledJobExecutor;

    public JobExecuteManager(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager, IChildJobStorage childJobStorage)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _childJobStorage = childJobStorage;

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

    public async Task Enqueue<T, TJob>(TJob coreJob,
        IEnumerable<TJob> childJobs,
        Expression<Func<T, TJob, Task>> execute,
        JobExecuteOptions jobExecuteOptions,
        ServiceExecuteOptions? serviceExecuteOptions = null,
        Action<TJob, string>? setJobIdAction = null,
        CancellationToken cancellationToken = default)
    {
        var coreExecutor = GetJobExecutor(serviceExecuteOptions: serviceExecuteOptions);
        
        var coreJobExpression = execute.BindSecondParameter(coreJob);
        var coreJobId = await coreExecutor.Enqueue(coreJobExpression, 
                                        jobExecuteOptions.WaitingQueue,
                                        serviceExecuteOptions, 
                                        cancellationToken);
        setJobIdAction?.Invoke(coreJob, coreJobId);

        foreach (var executedJob in childJobs)
        {
            var executor = GetJobExecutor(true);
            var jobExpression = execute.BindSecondParameter(executedJob);
            
            var jobId = await executor.Enqueue(jobExpression,
                jobExecuteOptions.WaitingQueue,
                cancellationToken : cancellationToken);
            
            setJobIdAction?.Invoke(executedJob, jobId);
        }
    }

    public async Task Execute<T, TJob>(string coreJobId,
        IEnumerable<string> childJobIds,
        JobExecuteOptions jobExecuteOptions,
        ServiceExecuteOptions serviceExecuteOptions,
        CancellationToken cancellationToken = default)
    {
        var coreExecutor = GetJobExecutor(serviceExecuteOptions : serviceExecuteOptions);
        var parentJobId = await coreExecutor.Execute<T>(coreJobId, jobExecuteOptions.ProcessingQueue, cancellationToken);
        var parentJobCreatedAt = DateTime.Now;       

        foreach (var executedJobId in childJobIds)
        {
            await _childJobStorage.CreateChildJobEntryAsync(
                new ChildJobEntry { JobId = executedJobId, ParentJobId = parentJobId, Status = 0, ParentCreatedAt = parentJobCreatedAt });
        }
    }

    public async Task StopWithFailedState(string coreJobId, IEnumerable<string> childJobIds, Exception exception, JobExecuteOptions? jobExecuteOptions = null, CancellationToken cancellationToken = default)
    {
        var serverName = !string.IsNullOrEmpty(jobExecuteOptions?.ChildServerName)
            ? jobExecuteOptions.ChildServerName
            : !string.IsNullOrEmpty(jobExecuteOptions?.ServerName) ? jobExecuteOptions.ServerName : null;

        foreach (var jobId in childJobIds)
        {
            StopWithFailedState(jobId, exception, serverName);

            _backgroundJobClient.Delete(jobId);
        }
        
        //todo
        StopWithFailedState(coreJobId, exception, jobExecuteOptions?.ServerName);
    }

    private void StopWithFailedState(string jobId, Exception exception, string? serverName)
    {
        var failedState = !string.IsNullOrEmpty(serverName) ? new FailedState(exception, serverName) : new FailedState(exception);
        failedState.Reason = exception.Message;

        _backgroundJobClient.ChangeState(jobId, failedState);
    }
}