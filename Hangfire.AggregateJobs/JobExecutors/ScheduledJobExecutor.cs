using Hangfire.States;
using System.Linq.Expressions;

namespace Hangfire.AggregateJobs.JobExecutors;

internal class ScheduledJobExecutor(IBackgroundJobClient backgroundJobClient) : IJobExecutor
{
    private readonly static TimeSpan DefaultEnqueuedIn = TimeSpan.FromSeconds(60);

    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    private const string ProcessingQueueParameterName = "ProcessingQueue";

    private const string EnqueuedInParameterName = "EnqueuedIn";

    public Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> jobTask, string waitingQueue,
        JobExecuteOptions? jobExecuteOptions = null,
        CancellationToken cancellationToken = default)
    {
        var jobId = _backgroundJobClient.Create(
                     waitingQueue,
                     jobTask,
                     new EnqueuedState(waitingQueue));

        var enqueuedIn = jobExecuteOptions?.EnqueuedInSeconds != null
            ? TimeSpan.FromSeconds(jobExecuteOptions.EnqueuedInSeconds.Value)
            : DefaultEnqueuedIn;

        SetJobParameter(jobId, EnqueuedInParameterName, enqueuedIn.ToString());

        return Task.FromResult(jobId);
    }

    public Task<string> ExecuteAsync<T>(string jobId, string processingQueue,  CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Execute<T>(jobId, processingQueue));
    }

    public bool IsAccessible(bool isChild = false, JobExecuteOptions? jobExecuteOptions = null)
    {
        return !isChild && jobExecuteOptions?.IntervalInSeconds == null && jobExecuteOptions?.EnqueuedInSeconds != null;
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