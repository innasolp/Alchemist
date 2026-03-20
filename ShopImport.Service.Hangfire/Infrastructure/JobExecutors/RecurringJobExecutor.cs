using Hangfire;
using Hangfire.States;

namespace ShopImport.Service.Hangfire.Infrastructure.JobExecutors;

internal class RecurringJobExecutor(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager) : IJobExecutor
{
    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

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

    public Task<string> Enqueue<T>(Func<T, Task> jobTask, string waitingQueue, CancellationToken cancellationToken = default)
    {
        var jobId = _backgroundJobClient.Create<T>(
                     obj => jobTask(obj),
                     new EnqueuedState(waitingQueue));
        return Task.FromResult(jobId);
    }

    public Task Execute(string jobId, string processingQueue, ServiceExecuteOptions? serviceExecuteOptions = null, CancellationToken cancellationToken = default)
    {
        var cron = ToCron(serviceExecuteOptions?.IntervalInSeconds ?? DefaultIntervalInSeconds);

        using var connection = JobStorage.Current.GetConnection();
        var jobData = connection.GetJobData(jobId);

        var method = jobData.Job.Method;
        if (!method.ReturnType.IsAssignableTo(typeof(Task)))
            throw new InvalidOperationException($"Returntype of job {jobId} is not Task.");

        var activator = JobActivator.Current;

        var recurringJobName = jobData.Job.Args.OfType<string>().FirstOrDefault() ?? jobId;

        var context = new JobActivatorContext(connection, new BackgroundJob(jobId, jobData.Job, jobData.CreatedAt, null), new HangfireTokenAdapter(cancellationToken));

        using (var scope = activator.BeginScope(context))
        {
            var instance = scope.Resolve(jobData.Job.Type);

            _recurringJobManager.AddOrUpdate(recurringJobName, () => method.Invoke(instance, jobData.Job.Args.ToArray()), cron);
        }

        _recurringJobManager.Trigger(recurringJobName);

        _backgroundJobClient.Delete(jobId);

        return Task.CompletedTask;
    }
}