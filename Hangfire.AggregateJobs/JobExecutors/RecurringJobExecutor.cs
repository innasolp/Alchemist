using Hangfire.AggregateJobs.JobExecutors.Expression;
using Hangfire.Storage;
using System.Linq.Expressions;

namespace Hangfire.AggregateJobs.JobExecutors;

internal class RecurringJobExecutor(IRecurringJobManager recurringJobManager) : IJobExecutor
{
    private readonly IRecurringJobManager _recurringJobManager = recurringJobManager;

    private const int DefaultIntervalInSeconds = 10800;

    private static string ToCron(int seconds)
    {
        TimeSpan interval = TimeSpan.FromSeconds(seconds);

        if (interval.TotalSeconds < 60)
            return $"*/{(int)interval.TotalSeconds} * * * * *";

        if (interval.TotalMinutes < 60)
            return $"*/{(int)interval.TotalMinutes} * * * *";

        if (interval.TotalHours < 24)
            return $"0 */{(int)interval.TotalHours} * * *";

        return $"0 0 */{(int)interval.TotalDays} * *";
    }

    public Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> jobTask,
        string waitingQueue,
        JobExecuteOptions? jobExecuteOptions = null,
        CancellationToken cancellationToken = default)
    {
        var cron = ToCron(jobExecuteOptions?.IntervalInSeconds ?? DefaultIntervalInSeconds);

        var jobArgs = jobTask.GetArguments();

        var recurringJobId = jobArgs?.OfType<string>().FirstOrDefault()
            ?? jobArgs?.FirstOrDefault()?.ToString()
            ?? Guid.NewGuid().ToString();

        _recurringJobManager.AddOrUpdate(recurringJobId,
                waitingQueue,
                jobTask,
                cron);

        return Task.FromResult(recurringJobId);
    }

    public Task<string> ExecuteAsync<T>(string recurringJobId, string processingQueue, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Execute<T>(recurringJobId, processingQueue));
    }

    private static RecurringJobDto? GetRecurringJob(IStorageConnection connection, string recurringId)
    {
        return connection.GetRecurringJobs().FirstOrDefault(p => p.Id == recurringId);
    }

    public bool IsAccessible(bool isChild = false, JobExecuteOptions? jobExecuteOptions = null, params object?[] parameters)
    {
        return (parameters?.Length > 0) == false && !isChild && jobExecuteOptions?.IntervalInSeconds != null;
    }

    public string Execute<T>(string recurringJobId, string processingQueue)
    {
        using var connection = JobStorage.Current.GetConnection();

        var recurringJobDto = GetRecurringJob(connection, recurringJobId)
            ?? throw new InvalidOperationException($"No recurring job with id {recurringJobId}");

        var expression = recurringJobDto.Job.ToExpression<T, Task>();

        _recurringJobManager.RemoveIfExists(recurringJobId);

        _recurringJobManager.AddOrUpdate(recurringJobId, processingQueue, expression, recurringJobDto.Cron);

        _recurringJobManager.Trigger(recurringJobId);

        var lastRecurringJob = GetRecurringJob(connection, recurringJobId);

        return lastRecurringJob?.LastJobId;
    }
}