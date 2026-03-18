using Hangfire;

namespace ShopImport.Service.Hangfire.Infrastructure.JobExecutors;

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
        _recurringJobExecutor = new RecurringJobExecutor(_backgroundJobClient, _recurringJobManager);
        _scheduledJobExecutor = new ScheduledJobExecutor(_backgroundJobClient);
    }

    private IJobExecutor GetJobExecutor(bool isChild = false, JobExecuteOptions? jobExecuteOptions = null)
    {
        if (isChild)
            return _backgroundJobExecutor;

        if (jobExecuteOptions?.IntervalInSeconds != null)
            return _recurringJobExecutor;

        if (jobExecuteOptions?.EnqueuedInSeconds != null)
            return _scheduledJobExecutor;

        return _backgroundJobExecutor;
    }

    public async Task Enqueue(IImportServiceJob importServiceJob, JobExecuteOptions? jobExecuteOptions = null, CancellationToken cancellationToken = default)
    {
        var executingJobs = await importServiceJob.GetExecutionServiceJobs();

        var mainJobIsAggregate = importServiceJob is AggregateShopImportServiceJob;

        foreach (var executedJob in executingJobs)
        {
            var executor = GetJobExecutor(executedJob.Value.ParentId != null, jobExecuteOptions);
            await executor.Enqueue(executedJob.Value, mainJobIsAggregate && executedJob.Key == importServiceJob.Id,  jobExecuteOptions, cancellationToken);
        }
    }

    public async Task Execute(IImportServiceJob importServiceJob, JobExecuteOptions? jobExecuteOptions = null, CancellationToken cancellationToken = default)
    {
        var executingJobs = await importServiceJob.GetExecutionServiceJobs();

        var mainJobIsAggregate = importServiceJob is AggregateShopImportServiceJob;

        foreach (var executedJob in executingJobs)
        {
            var executor = GetJobExecutor(executedJob.Value.ParentId != null, jobExecuteOptions);
            await executor.Execute(executedJob.Value, mainJobIsAggregate && executedJob.Key == importServiceJob.Id, jobExecuteOptions, cancellationToken);
        }
    }

    public async Task StopWithFailedState(IImportServiceJob importServiceJob, Exception exception, JobExecuteOptions? jobExecuteOptions = null, CancellationToken cancellationToken = default)
    {
        var executingJobs = (await importServiceJob.GetExecutionServiceJobs()).ToList();

        foreach (var executedJob in executingJobs)
        {
            var executor = GetJobExecutor(executedJob.Value.ParentId != null, jobExecuteOptions);
            await executor.StopWithFailedState(executedJob.Value, exception, jobExecuteOptions, cancellationToken);

            if (executedJob.Value.ParentId == importServiceJob.Id)
                _backgroundJobClient.Delete(executedJob.Value.JobId);
        }        
    }
}