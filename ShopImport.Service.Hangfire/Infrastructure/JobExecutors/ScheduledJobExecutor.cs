using Hangfire;
using Hangfire.States;

namespace ShopImport.Service.Hangfire.Infrastructure.JobExecutors;

internal class ScheduledJobExecutor(IBackgroundJobClient backgroundJobClient) : IJobExecutor
{
    private const string ProcessingQueueParameterName = "ProcessingQueue";

    private readonly static TimeSpan DefaultEnqueuedIn = TimeSpan.FromSeconds(60);

    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    private static void SetJobParameter(string jobId, string parameterName, string parameterValue)
    {
        using var connection = JobStorage.Current.GetConnection();
        connection.SetJobParameter(jobId, parameterName, parameterValue);
    }

    public Task<string> Enqueue<T>(Func<T, Task> jobTask, string waitingQueue, CancellationToken cancellationToken = default)
    {
        var jobId = _backgroundJobClient.Create<T>(
                     obj => jobTask(obj),
                     new EnqueuedState(waitingQueue));
        return Task.FromResult(jobId);
    }

    public Task Execute(string jobId, string processingQueue, ServiceExecuteOptions? serviceExecuteOptions = null, CancellationToken cancellationToken = default)
    {
        var enqueuedIn = serviceExecuteOptions?.EnqueuedInSeconds != null ? TimeSpan.FromSeconds(serviceExecuteOptions.EnqueuedInSeconds.Value) : DefaultEnqueuedIn;
        _backgroundJobClient.ChangeState(jobId, new ScheduledState(enqueuedIn));

        SetJobParameter(jobId, ProcessingQueueParameterName, processingQueue);

        return Task.CompletedTask;
    }
}