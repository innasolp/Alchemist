using Hangfire;
using Hangfire.States;

namespace ShopImport.Service.Hangfire.Infrastructure.JobExecutors;

internal class ScheduledJobExecutor(IBackgroundJobClient backgroundJobClient) : IJobExecutor
{
    private const string ProcessingQueueParameterName = "ProcessingQueue";

    private readonly static TimeSpan DefaultEnqueuedIn = TimeSpan.FromSeconds(60);

    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    public Task Enqueue(IImportServiceJob importServiceJob, JobExecuteOptions? jobExecuteOptions = null, CancellationToken cancellationToken = default)
    {
        var waitingQueue = jobExecuteOptions?.WaitingQueue ?? "waiting";

        _backgroundJobClient.Create<IHagfireServiceJobManager>(
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

        var enqueuedIn = jobExecuteOptions?.EnqueuedInSeconds != null ? TimeSpan.FromSeconds(jobExecuteOptions.EnqueuedInSeconds.Value) : DefaultEnqueuedIn;
        _backgroundJobClient.ChangeState(importServiceJob.JobId, new ScheduledState(enqueuedIn));

        var processingQueue = jobExecuteOptions?.ProcessingQueue ?? "processing";
        SetJobParameter(importServiceJob.JobId, ProcessingQueueParameterName, processingQueue);

        return Task.CompletedTask;
    }

    private static void SetJobParameter(string jobId, string parameterName, string parameterValue)
    {
        using var connection = JobStorage.Current.GetConnection();
        connection.SetJobParameter(jobId, parameterName, parameterValue);
    }

    public Task StopWithFailedState(IImportServiceJob importServiceJob, Exception exception, JobExecuteOptions? jobExecuteOptions = null, CancellationToken cancellationToken = default)
    {
        var failedState = jobExecuteOptions != null ? new FailedState(exception, jobExecuteOptions.ServerName) : new FailedState(exception);
        failedState.Reason = exception.Message;

        _backgroundJobClient.ChangeState(importServiceJob.JobId, failedState);

        return Task.CompletedTask;
    }
}
