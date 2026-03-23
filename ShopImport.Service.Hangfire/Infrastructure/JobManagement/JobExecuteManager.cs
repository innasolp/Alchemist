using Hangfire;
using Hangfire.States;
using ShopImport.Service.Hangfire.Infrastructure.JobManagement.JobExecutors;
using ShopImport.Service.Hangfire.Infrastructure.JobManagement.JobExecutors.Expression;
using System.Linq.Expressions;

namespace ShopImport.Service.Hangfire.Infrastructure.JobManagement;

internal class JobExecuteManager(IEnumerable<IJobExecutor> jobExecutors, 
    IBackgroundJobClient backgroundJobClient, 
    IChildJobStorage childJobStorage) : IJobExecuteManager
{
    private readonly IEnumerable<IJobExecutor> _jobExecutors = jobExecutors;

    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    private readonly IChildJobStorage _childJobStorage = childJobStorage;

    private readonly BackgroundJobExecutor DefaultJobExecutor = new(backgroundJobClient);

    public async Task Enqueue<T, TJob>(TJob coreJob,
        IEnumerable<TJob> childJobs,
        Expression<Func<T, TJob, Task>> execute,
        JobExecuteOptions jobExecuteOptions,
        ServiceExecuteOptions? serviceExecuteOptions = null,
        Action<TJob, string>? setJobIdAction = null,
        CancellationToken cancellationToken = default)
    {
        var coreExecutor = _jobExecutors.FirstOrDefault(e=>e.IsAccessible(serviceExecuteOptions: serviceExecuteOptions)) ?? DefaultJobExecutor;
        
        var coreJobExpression = execute.BindSecondParameter(coreJob);
        var coreJobId = await coreExecutor.EnqueueAsync(coreJobExpression, 
                                        jobExecuteOptions.WaitingQueue,
                                        serviceExecuteOptions, 
                                        cancellationToken);
        setJobIdAction?.Invoke(coreJob, coreJobId);

        foreach (var executedJob in childJobs)
        {
            var executor = _jobExecutors.FirstOrDefault(e => e.IsAccessible(true)) ?? DefaultJobExecutor;

            var jobExpression = execute.BindSecondParameter(executedJob);
            
            var jobId = await executor.EnqueueAsync(jobExpression,
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
        var coreExecutor = _jobExecutors.FirstOrDefault(e => e.IsAccessible(serviceExecuteOptions : serviceExecuteOptions)) ?? DefaultJobExecutor;
        var parentJobId = await coreExecutor.ExecuteAsync<T>(coreJobId, jobExecuteOptions.ProcessingQueue, cancellationToken);
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
        
        StopWithFailedState(coreJobId, exception, jobExecuteOptions?.ServerName);
    }

    private void StopWithFailedState(string jobId, Exception exception, string? serverName)
    {
        var failedState = !string.IsNullOrEmpty(serverName) ? new FailedState(exception, serverName) : new FailedState(exception);
        failedState.Reason = exception.Message;

        _backgroundJobClient.ChangeState(jobId, failedState);
    }
}