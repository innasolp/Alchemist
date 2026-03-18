using Hangfire;
using Hangfire.States;

namespace ShopImport.Service.Hangfire.Infrastructure.JobExecutors;

internal class BackgroundJobExecutor(IBackgroundJobClient backgroundJobClient) : IJobExecutor
{
    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    public Task Enqueue(IImportServiceJob importServiceJob, bool isAggregate = false, JobExecuteOptions? jobExecuteOptions = null, CancellationToken cancellationToken = default)
    {
        importServiceJob.JobId = _backgroundJobClient.Create<IHagfireServiceJobManager>(
                     serviceJobManager => serviceJobManager.Execute(importServiceJob.Id,
                                                                    importServiceJob.ImportService.Name,
                                                                    isAggregate,
                                                                    importServiceJob.ParentId,
                                                                    cancellationToken,
                                                                    null),
                     new EnqueuedState(jobExecuteOptions?.WaitingQueue ?? "waiting"));

        return Task.CompletedTask;        
    }

    public Task Execute(IImportServiceJob importServiceJob, bool isAggregate = false, JobExecuteOptions? jobExecuteOptions = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(importServiceJob.JobId))
            throw new InvalidOperationException($"Job {importServiceJob.ImportService.Name} not enqueued.");

        _backgroundJobClient.ChangeState(importServiceJob.JobId, new EnqueuedState(jobExecuteOptions?.ProcessingQueue ?? "processing"));
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