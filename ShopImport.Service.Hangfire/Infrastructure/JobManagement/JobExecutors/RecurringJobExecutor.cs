using Hangfire;
using Hangfire.Storage;
using ShopImport.Service.Hangfire.Infrastructure.JobManagement.JobExecutors.Expression;
using System.Linq.Expressions;

namespace ShopImport.Service.Hangfire.Infrastructure.JobManagement.JobExecutors;

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

    public Task<string> Enqueue<T>(Expression<Func<T, Task>> jobTask, 
        string waitingQueue, 
        ServiceExecuteOptions? serviceExecuteOptions = null, 
        CancellationToken cancellationToken = default)
    {
        var cron = ToCron(serviceExecuteOptions?.IntervalInSeconds ?? DefaultIntervalInSeconds);

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

    public Task Execute<T>(string recurringJobId, string processingQueue, CancellationToken cancellationToken = default)
    {
        using var connection = global::Hangfire.JobStorage.Current.GetConnection();
        
        var recurringJobDto = connection.GetRecurringJobs().FirstOrDefault(x => x.Id == recurringJobId);//GetRecurringJobByLastJobId(connection, recurringJobId);

        if (recurringJobDto?.Job != null)
        {
            var expression = recurringJobDto.Job.ToExpression<T, Task>();

            _recurringJobManager.RemoveIfExists(recurringJobId);

            _recurringJobManager.AddOrUpdate(recurringJobId, processingQueue, expression, recurringJobDto.Cron);
            
            _recurringJobManager.Trigger(recurringJobId);
        }

        return Task.CompletedTask;
    }
}