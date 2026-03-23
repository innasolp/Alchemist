using Hangfire;
using Hangfire.States;
using System.Linq.Expressions;

namespace ShopImport.Service.Hangfire.Infrastructure.JobManagement.JobExecutors;

internal class ScheduledJobExecutor(IBackgroundJobClient backgroundJobClient) : IJobExecutor
{
    private readonly static TimeSpan DefaultEnqueuedIn = TimeSpan.FromSeconds(60);

    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    private const string ProcessingQueueParameterName = "ProcessingQueue";

    private const string EnqueuedInParameterName = "EnqueuedIn";

    public Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> jobTask, string waitingQueue,
        ServiceExecuteOptions? serviceExecuteOptions = null,
        CancellationToken cancellationToken = default)
    {
        var jobId = _backgroundJobClient.Create(
                     waitingQueue,
                     jobTask,
                     new EnqueuedState(waitingQueue));

        var enqueuedIn = serviceExecuteOptions?.EnqueuedInSeconds != null
            ? TimeSpan.FromSeconds(serviceExecuteOptions.EnqueuedInSeconds.Value)
            : DefaultEnqueuedIn;

        SetJobParameter(jobId, EnqueuedInParameterName, enqueuedIn.ToString());

        return Task.FromResult(jobId);
    }

    public Task<string> ExecuteAsync<T>(string jobId, string processingQueue,  CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Execute<T>(jobId, processingQueue));
    }

    public bool IsAccessible(bool isChild = false, ServiceExecuteOptions? serviceExecuteOptions = null)
    {
        return !isChild && serviceExecuteOptions?.IntervalInSeconds == null && serviceExecuteOptions?.EnqueuedInSeconds != null;
    }

    public string Execute<T>(string jobId, string processingQueue)
    {
        var enqueuedInParam = GetJobParameter(jobId, EnqueuedInParameterName);

        var enqueuedIn = TimeSpan.TryParse(enqueuedInParam, out var enqueuedInVal) ? enqueuedInVal : DefaultEnqueuedIn;

        SetJobParameter(jobId, ProcessingQueueParameterName, processingQueue);

        _backgroundJobClient.ChangeState(jobId, new ScheduledState(enqueuedIn));

        return jobId;
    }

    private static void SetJobParameter(string jobId, string parameterName, string parameterValue)
    {
        using var connection = JobStorage.Current.GetConnection();
        connection.SetJobParameter(jobId, parameterName, parameterValue);
    }
    
    private static string GetJobParameter(string jobId, string parameterName)
    {
        using var connection = JobStorage.Current.GetConnection();
        return connection.GetJobParameter(jobId, parameterName);
    }
}