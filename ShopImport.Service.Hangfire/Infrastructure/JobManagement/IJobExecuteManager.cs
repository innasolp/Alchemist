using System.Linq.Expressions;

namespace ShopImport.Service.Hangfire.Infrastructure.JobManagement;

internal interface IJobExecuteManager
{
    Task Enqueue<T, TJob>(TJob coreJob,
        IEnumerable<TJob> childJobs,
        Expression<Func<T, TJob, Task>> execute,
        JobExecuteOptions jobExecuteOptions,
        ServiceExecuteOptions? serviceExecuteOptions = null,
        Action<TJob, string>? setJobIdAction = null,
        CancellationToken cancellationToken = default);

    Task Execute<T, TJob>(string coreJobId,
        IEnumerable<string> childJobIds,
        JobExecuteOptions jobExecuteOptions,
        ServiceExecuteOptions serviceExecuteOptions,
        CancellationToken cancellationToken = default);

    Task StopWithFailedState(string coreJobId, 
        IEnumerable<string> childJobIds, 
        Exception exception, 
        JobExecuteOptions? jobExecuteOptions = null, 
        CancellationToken cancellationToken = default);
}