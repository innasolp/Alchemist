using Hangfire;
using Hangfire.States;

namespace ShopImport.Service.Hangfire.Infrastructure.JobExecutors;

internal class RecurringJobExecutor(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager) : IJobExecutor
{
    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    private readonly IRecurringJobManager _recurringJobManager = recurringJobManager;

    private const int DefaultIntervalInSeconds = 10800;

    public Task Enqueue(IImportServiceJob importServiceJob, JobExecuteOptions? jobExecuteOptions = null, CancellationToken cancellationToken = default)
    {
        var waitingQueue = jobExecuteOptions?.WaitingQueue ?? "waiting";

        importServiceJob.JobId = _backgroundJobClient.Create<IHagfireServiceJobManager>(
                      serviceJobManager => serviceJobManager.Execute(importServiceJob.Id,
                                                                     importServiceJob.ImportService.Name,
                                                                     cancellationToken,
                                                                     null),
                      new EnqueuedState(waitingQueue));

        return Task.CompletedTask;
    }

    public Task Execute(IImportServiceJob importServiceJob, JobExecuteOptions? jobExecuteOptions = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(importServiceJob.JobId))
            throw new InvalidOperationException($"Job {importServiceJob.ImportService.Name} not enqueued.");

        var dateTime = jobExecuteOptions?.IntervalInSeconds != null
            ? DateTimeOffset.FromUnixTimeSeconds(jobExecuteOptions.IntervalInSeconds.Value)
            : DateTimeOffset.FromUnixTimeSeconds(DefaultIntervalInSeconds);

        var cron = $"{dateTime.Minute} {dateTime.Hour} {dateTime.Day} {dateTime.Month} *";        

        _recurringJobManager.AddOrUpdate<IHagfireServiceJobManager>(importServiceJob.ImportService.Name,
            jobExecuteOptions?.ProcessingQueue ?? "processing",
            serviceJobManager => serviceJobManager.Execute(importServiceJob.Id,
                                                                     importServiceJob.ImportService.Name,
                                                                     cancellationToken,
                                                                     null),
             cron);

        _backgroundJobClient.Delete(importServiceJob.JobId);

        return Task.CompletedTask;
    }

    public Task StopWithFailedState(IImportServiceJob importServiceJob, Exception exception, JobExecuteOptions? jobExecuteOptions = null, CancellationToken cancellationToken = default)
    {
        var failedState = jobExecuteOptions != null ? new FailedState(exception, jobExecuteOptions.ServerName) : new FailedState(exception);
        failedState.Reason = exception.Message;

        _backgroundJobClient.ChangeState(importServiceJob.JobId, failedState);

        return Task.CompletedTask;
    }
}
